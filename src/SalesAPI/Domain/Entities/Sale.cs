namespace SalesAPI.Domain.Entities;

public class Sale
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SaleNumber { get; set; }
    public DateTime SaleDate { get; set; }
    public string CustomerName { get; set; }
    public string Branch { get; set; }
    public bool IsCancelled { get; set; }
    public List<SaleItem> Items { get; set; } = new();

    public decimal TotalAmount => Items.Sum(item => item.Total);
}