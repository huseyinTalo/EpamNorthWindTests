using Mapster;
using Microsoft.AspNetCore.Mvc;
using Northwind.Orders.WebApi.Models;
using Northwind.Services.Repositories;

namespace Northwind.Orders.WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderRepository orderRepository;
    private readonly ILogger<OrdersController> logger;

    public OrdersController(IOrderRepository orderRepositoryAbs, ILogger<OrdersController> loggerAbs)
    {
        this.orderRepository = orderRepositoryAbs ?? throw new ArgumentNullException(nameof(orderRepositoryAbs));
        this.logger = loggerAbs ?? throw new ArgumentNullException(nameof(loggerAbs));
    }

    [HttpGet("{orderId}")]
    public async Task<ActionResult<FullOrder>> GetOrderAsync(long orderId)
    {
        try
        {
            if (!await this.IsOrderExists(orderId))
            {
                return this.BadRequest(orderId);
            }

            var order = await this.orderRepository.GetOrderAsync(orderId);
            var fullOrder = order.Adapt<FullOrder>();
            fullOrder.Customer.Code = order.Customer.Code.Code;
            return this.Ok(fullOrder);
        }
        catch (OrderNotFoundException)
        {
            return this.NotFound();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving order {OrderId}", orderId);
            return this.StatusCode(500);
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BriefOrder>>> GetOrdersAsync(int? skip = 0, int? count = 10)
    {
        try
        {
            if (skip < 0 || count <= 0)
            {
                return this.BadRequest();
            }

            var orders = await this.orderRepository.GetOrdersAsync(skip ?? 0, count ?? 10);
            var briefOrders = orders.Adapt<IList<BriefOrder>>();
            foreach (var briefOrder in briefOrders)
            {
                var order = orders.FirstOrDefault(x => x.Id == briefOrder.Id);
                briefOrder.CustomerId = order!.Customer.Code.Code;
                briefOrder.ShipAddress = order.ShippingAddress.Address;
                briefOrder.ShipCity = order.ShippingAddress.City;
                briefOrder.ShipCountry = order.ShippingAddress.Country;
                briefOrder.ShipPostalCode = order.ShippingAddress.PostalCode;
                briefOrder.ShipRegion = order.ShippingAddress.Region;
            }

            return this.Ok(briefOrders);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving orders with skip={Skip}, count={Count}", skip, count);
            return this.StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<ActionResult<AddOrder>> AddOrderAsync(BriefOrder order)
    {
        try
        {
            if (order is null)
            {
                return this.BadRequest(order);
            }

            var repoOrder = BriefToOrder(order.Id, order);

            long orderId = await this.orderRepository.AddOrderAsync(repoOrder);
            return this.Ok(new AddOrder { OrderId = orderId });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error adding order");
            return this.StatusCode(500);
        }
    }

    [HttpDelete("{orderId}")]
    public async Task<ActionResult> RemoveOrderAsync(long orderId)
    {
        try
        {
            if (await this.IsOrderExists(orderId))
            {
                return this.BadRequest(orderId);
            }

            await this.orderRepository.RemoveOrderAsync(orderId);

            return this.NoContent();
        }
        catch (OrderNotFoundException)
        {
            return this.NotFound();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error removing order {OrderId}", orderId);

            return this.StatusCode(500);
        }
    }

    [HttpPut("{orderId}")]
    public async Task<ActionResult> UpdateOrderAsync(long orderId, BriefOrder order)
    {
        try
        {
            if (order is null)
            {
                return this.BadRequest(order);
            }

            if (await this.IsOrderExists(orderId))
            {
                return this.BadRequest(orderId);
            }

            var repoOrder = BriefToOrder(orderId, order);
            await this.orderRepository.UpdateOrderAsync(repoOrder);
            return this.NoContent();
        }
        catch (OrderNotFoundException)
        {
            return this.NotFound();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating order {OrderId}", orderId);
            return this.StatusCode(500);
        }
    }

    private static Order BriefToOrder(long orderId, BriefOrder order)
    {
        Order repoOrder = new Order(orderId)
        {
            Customer = new Northwind.Services.Repositories.Customer(new CustomerCode(order.CustomerId)),

            ShippingAddress = new Northwind.Services.Repositories.ShippingAddress(order.ShipAddress, order.ShipCity, order.ShipRegion, order.ShipPostalCode, order.ShipCountry),

            Freight = order.Freight,

            Employee = new Northwind.Services.Repositories.Employee(order.EmployeeId),
            OrderDate = order.OrderDate,
            RequiredDate = order.RequiredDate,
            ShippedDate = order.ShippedDate,
        };
        repoOrder.Freight = order.Freight;
        repoOrder.ShipName = order.ShipName;
        foreach (var detail in order.OrderDetails)
        {
            repoOrder.OrderDetails.Add(new OrderDetail(repoOrder)
            {
                Discount = detail.Discount,
                UnitPrice = detail.UnitPrice,
                Quantity = detail.Quantity,
                Product = new Product(detail.ProductId),
            });
        }

        return repoOrder;
    }

    private async Task<bool> IsOrderExists(long orderId)
    {
        var order = await this.orderRepository.GetOrderAsync(orderId);

        if (order is null)
        {
            return false;
        }

        return true;
    }
}
