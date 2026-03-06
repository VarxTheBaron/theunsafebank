using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using theunsafebank.Data;
using theunsafebank.Models;

namespace theunsafebank.Controllers;

public class AccountController : Controller
{
    private readonly BankContext _context;

    public AccountController(BankContext context)
    {
        _context = context;
    }

    public IActionResult Dashboard()
    {
        var customerId = GetCustomerIdFromSession();

        if (customerId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        // Get account with transfers
        var account = _context.Accounts
            .Include(a => a.Customer)
            .Include(a => a.TransfersFrom)
                .ThenInclude(t => t.ToAccount)
            .Include(a => a.TransfersTo)
                .ThenInclude(t => t.FromAccount)
            .FirstOrDefault(a => a.CustomerId == customerId);

        if (account == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        return View(account);
    }

    [HttpPost]
    public IActionResult Transfer(string toAccountNumber, decimal amount, string receiverMessage, string senderNote)
    {
        var customerId = GetCustomerIdFromSession();

        if (customerId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var fromAccount = _context.Accounts
            .FirstOrDefault(a => a.CustomerId == customerId);

        if (fromAccount == null)
        {
            return RedirectToAction("Dashboard");
        }

        // FIXME: Minimal validation
        if (amount <= 0)
        {
            TempData["Error"] = "Amount must be positive";
            return RedirectToAction("Dashboard");
        }

        var toAccount = _context.Accounts
            .FirstOrDefault(a => a.AccountNumber == toAccountNumber);

        if (toAccount == null)
        {
            TempData["Error"] = "Account not found";
            return RedirectToAction("Dashboard");
        }

        if (fromAccount.Id == toAccount.Id)
        {
            TempData["Error"] = "Cannot transfer to your own account";
            return RedirectToAction("Dashboard");
        }

        // FIXME: No transaction, race condition possible. Fix with a database transaction and row locking.
        decimal transactionFee = 5m;

        if (fromAccount.Balance < amount + transactionFee)
        {
            TempData["Error"] = "Insufficient funds (a 5 SEK transaction fee applies)";
            return RedirectToAction("Dashboard");
        }

        // Perform transfer
        fromAccount.Balance -= amount + transactionFee;
        toAccount.Balance += amount;

        // Credit the transaction fee to the bank's account (1001)
        var bankAccount = _context.Accounts.FirstOrDefault(a => a.AccountNumber == "1001");
        if (bankAccount != null && bankAccount.Id != fromAccount.Id)
        {
            bankAccount.Balance += transactionFee;
        }

        var transfer = new Transfer
        {
            FromAccountId = fromAccount.Id,
            ToAccountId = toAccount.Id,
            FromAccount = fromAccount,
            ToAccount = toAccount,
            Amount = amount,
            ReceiverMessage = receiverMessage,
            SenderNote = senderNote,
            Date = DateTime.Now
        };

        _context.Transfers.Add(transfer);
        _context.SaveChanges();

        TempData["Success"] = "Transfer completed successfully";
        return RedirectToAction("Dashboard");
    }

    [HttpGet]
    public IActionResult ExportData()
    {
        return View();
    }

    [HttpPost]
    public IActionResult ExportData(string username, string password)
    {
        string testInput = username + password;
        if (testInput.Contains("DROP", StringComparison.OrdinalIgnoreCase))
        {
            ViewBag.Error = "Jag sa ju INGA DROP-kommandon!";
            return View();
        }

        // VULNERABLE: CRITICAL SQL INJECTION!
        // Example attack: username = ' OR 1=1 -- 

        var exportData = new { customers = new List<dynamic>(), accounts = new List<dynamic>() };

        using (var connection = _context.Database.GetDbConnection())
        {
            connection.Open();

            var passwordWhere = $"AND Password = '{password}'";

            var sql = $@"
                    SELECT 
                        Id, 
                        Username, 
                        Password, 
                        FullName
                    FROM Customers
                    WHERE Username = '{username}' {passwordWhere}
                ";

            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                using (var reader = command.ExecuteReader())
                {
                    var customerList = new List<dynamic>();
                    while (reader.Read())
                    {
                        customerList.Add(new
                        {
                            Id = reader["Id"],
                            Username = reader["Username"],
                            Password = reader["Password"],
                            FullName = reader["FullName"]
                        });
                    }

                    if (!customerList.Any())
                    {
                        ViewBag.Error = "Invalid username or password";
                        return View();
                    }

                    exportData = new { customers = customerList, accounts = new List<dynamic>() };
                }
            }

            var accountSql = $@"
                    SELECT * FROM Accounts
                    WHERE CustomerId IN (
                        SELECT Id FROM Customers 
                        WHERE Username = '{username}' {passwordWhere}
                    )
                ";

            using (var command = connection.CreateCommand())
            {
                command.CommandText = accountSql;
                using (var reader = command.ExecuteReader())
                {
                    var accountList = new List<dynamic>();
                    while (reader.Read())
                    {
                        accountList.Add(new
                        {
                            Id = reader["Id"],
                            AccountNumber = reader["AccountNumber"],
                            Balance = reader["Balance"],
                            CustomerId = reader["CustomerId"]
                        });
                    }
                    exportData = new { exportData.customers, accounts = accountList };
                }
            }
        }

        // Returnera json som filnedladdning
        var json = System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", $"bank_export_{DateTime.Now:yyyyMMdd_HHmmss}.json");


    }

    [HttpGet]
    public IActionResult LookupAccount(string accountNumber)
    {
        var account = _context.Accounts
            .Include(a => a.Customer)
            .FirstOrDefault(a => a.AccountNumber == accountNumber);

        if (account == null)
        {
            return Json(new { success = false, message = "Kontot hittades inte" });
        }

        return Json(new { success = true, name = account.Customer.FullName });
    }

    private int? GetCustomerIdFromSession()
    {
        var info = HttpContext.Session.GetString("customerId");

        if (info != null && int.TryParse(info, out var customerId))
        {
            return customerId;
        }

        return null;
    }
}
