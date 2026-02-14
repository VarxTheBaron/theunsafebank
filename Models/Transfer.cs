namespace theunsafebank.Models;

public class Transfer
{
    public int Id { get; set; }
    public int FromAccountId { get; set; }
    public int ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public string? ReceiverMessage { get; set; }
    public string? SenderNote { get; set; }
    public DateTime Date { get; set; }

    // Navigation properties
    public required Account FromAccount { get; set; }
    public required Account ToAccount { get; set; }
}
