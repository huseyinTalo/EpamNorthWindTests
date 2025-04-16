namespace Northwind.Services.EntityFramework.Entities;

public class OrderDetail
{
    public int? OrderId { get; set; }

    public int? ProductId { get; set; }

    public double UnitPrice { get; set; }

    public int Quantity { get; set; }

    public double Discount { get; set; }

    public virtual Order Order { get; set; } = default!;

    public virtual Product Product { get; set; } = default!;
}
