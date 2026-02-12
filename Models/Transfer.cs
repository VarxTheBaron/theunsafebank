namespace theunsafebank.Models;

public class Transfer
{
    public int Id { get; set; }
    public int FromAccountId { get; set; }
    public int ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Message { get; set; }
    public DateTime Date { get; set; }

    // Navigation properties
    public Account FromAccount { get; set; }
    public Account ToAccount { get; set; }
}
