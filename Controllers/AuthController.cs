using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using theunsafebank.Data;
using theunsafebank.Models;

namespace theunsafebank.Controllers;

public class AuthController : Controller
{
    private readonly BankContext _context;

    public AuthController(BankContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        var customer = _context.Customers
            .FirstOrDefault(c => c.Username == username && c.Password == password);

        if (customer != null)
        {
            HttpContext.Session.SetString("currentSession", customer.Id.ToString());
            return RedirectToAction("Dashboard", "Account");
        }

        ViewBag.Error = "Invalid username or password";
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(string username, string password, string fullName)
    {
        var existingCustomer = _context.Customers.FirstOrDefault(c => c.Username == username);

        if (existingCustomer != null)
        {
            ViewBag.Error = "Username already exists";
            return View();
        }

        var customer = new Customer
        {
            Username = username,
            Password = password,
            FullName = fullName
        };

        _context.Customers.Add(customer);
        _context.SaveChanges();

        var accountNumber = (1000 + customer.Id).ToString();

        var account = new Account
        {
            AccountNumber = accountNumber,
            Balance = 10000m, // 10,000 SEK
            CustomerId = customer.Id
        };

        if (customer.Id % 10 == 0)
        {
            account.Balance += 10000m;
        }

        _context.Accounts.Add(account);
        _context.SaveChanges();

        HttpContext.Session.SetString("currentSession", customer.Id.ToString());
        return RedirectToAction("Dashboard", "Account");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Remove("currentSession");
        return RedirectToAction("Login");
    }
}
