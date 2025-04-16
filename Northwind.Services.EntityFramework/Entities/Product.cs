namespace Northwind.Services.EntityFramework.Entities;

public class Product
{
    public int ProductId { get; set; }

    public string? ProductName { get; set; } = default!;

    public int SupplierId { get; set; }

    public int CategoryId { get; set; }

    public string? QuantityPerUnit { get; set; } = default!;

    public double UnitPrice { get; set; }

    public int UnitsInStock { get; set; }

    public int UnitsOnOrder { get; set; }

    public int ReorderLevel { get; set; }

    public int Discontinued { get; set; }

    public virtual IList<OrderDetail> OrderDetails { get; set; } = [];

    public virtual Category Category { get; set; } = default!;

    public virtual Supplier Supplier { get; set; } = default!;
}
