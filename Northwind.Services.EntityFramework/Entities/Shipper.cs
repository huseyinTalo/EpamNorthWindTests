namespace Northwind.Services.EntityFramework.Entities;

public class Shipper
{
    public int ShipperId { get; set; }

    public string? CompanyName { get; set; } = default!;

    public string? Phone { get; set; } = default!;

    public virtual IList<Order> Orders { get; set; } = [];
}
