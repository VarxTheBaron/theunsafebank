namespace theunsafebank.Models;

public class Account
{
    public int Id { get; set; }
    public string AccountNumber { get; set; }
    public decimal Balance { get; set; }
    public int CustomerId { get; set; }

    // Navigation properties
    public Customer Customer { get; set; }
    public List<Transfer> TransfersFrom { get; set; }
    public List<Transfer> TransfersTo { get; set; }
}
