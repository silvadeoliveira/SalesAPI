namespace SalesAPI.Domain.Entities;

public class SaleItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SaleId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public decimal Discount
    {
        get
        {
            if (Quantity > 20) throw new InvalidOperationException("Cannot sell more than 20 items.");
            if (Quantity >= 10) return UnitPrice * Quantity * 0.20m;
            if (Quantity >= 4) return UnitPrice * Quantity * 0.10m;
            return 0m;
        }
    }

    public decimal Total => (UnitPrice * Quantity) - Discount;
}