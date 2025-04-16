using System.Collections;

namespace Northwind.Services.EntityFramework.Entities;

public class Order
{
    public int OrderId { get; set; }

    public string? CustomerId { get; set; }

    public int EmployeeId { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime RequiredDate { get; set; }

    public DateTime? ShippedDate { get; set; }

    public int ShipVia { get; set; }

    public double Freight { get; set; }

    public string? ShipName { get; set; }

    public string? ShipAddress { get; set; }

    public string? ShipCity { get; set; }

    public string? ShipRegion { get; set; }

    public string? ShipPostalCode { get; set; } = default!;

    public string? ShipCountry { get; set; }

    public virtual IList<OrderDetail> OrderDetails { get; set; } = [];

    public virtual Customer? Customer { get; set; }

    public virtual Shipper? Shipper { get; set; }

    public virtual Employee? Employee { get; set; }
}
