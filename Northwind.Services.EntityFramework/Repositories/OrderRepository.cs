using Microsoft.EntityFrameworkCore;
using Northwind.Services.EntityFramework.Entities;
using Northwind.Services.Repositories;
using RepositoryCustomer = Northwind.Services.Repositories.Customer;
using RepositoryCustomerCode = Northwind.Services.Repositories.CustomerCode;
using RepositoryEmployee = Northwind.Services.Repositories.Employee;
using RepositoryOrder = Northwind.Services.Repositories.Order;
using RepositoryOrderDetail = Northwind.Services.Repositories.OrderDetail;
using RepositoryProduct = Northwind.Services.Repositories.Product;
using RepositoryShipper = Northwind.Services.Repositories.Shipper;

namespace Northwind.Services.EntityFramework.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly NorthwindContext northwindContext;

    public OrderRepository(NorthwindContext context) =>
        this.northwindContext = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<RepositoryOrder> GetOrderAsync(long orderId)
    {
        if (orderId <= 0)
        {
            throw new OrderNotFoundException($"Order with ID {orderId} not found.");
        }

        if (this.northwindContext.Orders is null)
        {
            throw new OrderNotFoundException($"Order with ID {orderId} not found.");
        }

        // eager loading
        var order = await this.northwindContext.Orders
    .Include(o => o.OrderDetails)
        .ThenInclude(od => od.Product)
            .ThenInclude(p => p.Category)
    .Include(o => o.OrderDetails)
        .ThenInclude(od => od.Product)
            .ThenInclude(p => p.Supplier)
    .Include(o => o.Customer)
    .Include(o => o.Shipper)
    .Include(o => o.Employee)
    .FirstOrDefaultAsync(x => x.OrderId == orderId);

        if (order is null)
        {
            throw new OrderNotFoundException($"Order with ID {orderId} not found.");
        }

        var orderDetails = order.OrderDetails;

        var repositoryOrder = new RepositoryOrder(order.OrderId)
        {
            OrderDate = order.OrderDate,
            RequiredDate = order.RequiredDate,
            ShippedDate = order.ShippedDate,
            Freight = order.Freight,
            ShipName = order.ShipName!,
            ShippingAddress = new ShippingAddress(
                order.ShipAddress!,
                order.ShipCity!,
                order.ShipRegion,
                order.ShipPostalCode!,
                order.ShipCountry!),
        };

        if (order.CustomerId != null)
        {
            var customerOfOrder = order.Customer;
            repositoryOrder.Customer = new RepositoryCustomer(new RepositoryCustomerCode(order.Customer!.CustomerId!))
            {
                CompanyName = customerOfOrder!.CompanyName!,
            };
        }

        var employeeOfOrder = order.Employee;

        repositoryOrder.Employee = new RepositoryEmployee(order.Employee!.EmployeeId)
        {
            FirstName = employeeOfOrder!.FirstName!,
            LastName = employeeOfOrder.LastName!,
            Country = employeeOfOrder.Country!,
        };

        var shipperOfOrder = order.Shipper;
        repositoryOrder.Shipper = new RepositoryShipper(order.Shipper!.ShipperId)
        {
            CompanyName = shipperOfOrder!.CompanyName!,
        };

        if (orderDetails.Count > 0)
        {
            foreach (var detail in orderDetails)
            {
                if (detail.ProductId == 0)
                {
                    throw new RepositoryException("Unable to load products of this order detail");
                }

                var entityProduct = detail.Product;

                if (entityProduct!.SupplierId == 0 || entityProduct.CategoryId == 0)
                {
                    throw new RepositoryException("Unable to load suppliers or categories of this product");
                }

                RepositoryProduct repositoryProduct = new RepositoryProduct(entityProduct.ProductId)
                {
                    ProductName = entityProduct!.ProductName!,
                    SupplierId = entityProduct!.SupplierId!,
                    CategoryId = entityProduct!.CategoryId!,
                    Category = entityProduct.Category.CategoryName!,
                    Supplier = entityProduct.Supplier.CompanyName!,
                };

                var repositoryDetail = new RepositoryOrderDetail(repositoryOrder)
                {
                    UnitPrice = detail.UnitPrice,
                    Quantity = detail.Quantity,
                    Discount = detail.Discount,
                    Product = repositoryProduct,
                };

                repositoryOrder.OrderDetails.Add(repositoryDetail);
            }
        }

        return repositoryOrder;
    }

    public async Task<IList<RepositoryOrder>> GetOrdersAsync(int skip, int count)
    {
        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip), "Skip value cannot be negative.");
        }

        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count value must be greater than zero.");
        }

        var orderEntities = await this.northwindContext.Orders
            .Skip(skip)
            .Take(count)
            .ToListAsync();

        var result = new List<RepositoryOrder>();

        foreach (var order in orderEntities)
        {
            result.Add(new RepositoryOrder(order.OrderId));
        }

        return result;
    }

    public async Task<long> AddOrderAsync(RepositoryOrder order)
    {
        using var transaction = await this.northwindContext.Database.BeginTransactionAsync();

        try
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order), "Order cannot be null.");
            }

            if (order.OrderDetails is null || order.Customer is null || order.Employee is null || order.Shipper is null)
            {
                throw new RepositoryException("Order Detail is not optional");
            }

            var entityOrder = new Entities.Order
            {
                OrderDate = order.OrderDate,
                RequiredDate = order.RequiredDate,
                ShippedDate = order.ShippedDate,
                Freight = order.Freight,
                ShipName = order.ShipName,
                ShipAddress = order.ShippingAddress?.Address,
                ShipCity = order.ShippingAddress?.City,
                ShipRegion = order.ShippingAddress?.Region,
                ShipPostalCode = order.ShippingAddress?.PostalCode,
                ShipCountry = order.ShippingAddress?.Country,
                CustomerId = order.Customer.Code?.Code,
                EmployeeId = (int)order.Employee.Id,
                ShipVia = (int)order.Shipper.Id,
            };

            var existingCustomer = await this.northwindContext.Customers
                .FirstOrDefaultAsync(x => x.CustomerId == order.Customer.Code!.Code);

            if (existingCustomer == null)
            {
                Entities.Customer orderCustomer = new Entities.Customer()
                {
                    CustomerId = order.Customer!.Code!.Code,
                    CompanyName = order.Customer.CompanyName,
                    City = order.ShippingAddress!.City,
                    Address = order.ShippingAddress!.Address,
                    Region = order.ShippingAddress!.Region,
                    PostalCode = order.ShippingAddress!.PostalCode,
                    ContactName = order.ShippingAddress!.Country,
                };

                _ = await this.northwindContext.Customers.AddAsync(orderCustomer);
                int customerResult = await this.northwindContext.SaveChangesAsync();
                if (customerResult <= 0)
                {
                    await transaction.RollbackAsync();
                    throw new RepositoryException("Failed to create customer record");
                }
            }

            var existingEmployee = await this.northwindContext.Employees
                .FirstOrDefaultAsync(x => x.EmployeeId == order.Employee.Id);

            if (existingEmployee == null)
            {
                Entities.Employee orderEmployee = new Entities.Employee()
                {
                    EmployeeId = (int)order.Employee.Id,
                    FirstName = order.Employee.FirstName,
                    LastName = order.Employee.LastName,
                    Country = order.Employee.Country,
                };
                _ = await this.northwindContext.Employees.AddAsync(orderEmployee);
                int employeeResult = await this.northwindContext.SaveChangesAsync();
                if (employeeResult <= 0)
                {
                    await transaction.RollbackAsync();
                    throw new RepositoryException("Failed to create employee record");
                }
            }

            var existingShipper = await this.northwindContext.Shippers
                .FirstOrDefaultAsync(x => x.ShipperId == order.Shipper.Id);

            if (existingShipper == null)
            {
                Entities.Shipper orderShipper = new Entities.Shipper()
                {
                    ShipperId = (int)order.Shipper.Id,
                    CompanyName = order.Shipper.CompanyName,
                };

                _ = await this.northwindContext.Shippers.AddAsync(orderShipper);
                int shipperResult = await this.northwindContext.SaveChangesAsync();
                if (shipperResult <= 0)
                {
                    await transaction.RollbackAsync();
                    throw new RepositoryException("Failed to create shipper record");
                }
            }

            _ = await this.northwindContext.Orders.AddAsync(entityOrder);
            int orderResult = await this.northwindContext.SaveChangesAsync();
            if (orderResult <= 0)
            {
                await transaction.RollbackAsync();
                throw new RepositoryException("Failed to create order record");
            }

            foreach (var item in order.OrderDetails)
            {
                if (item.Discount < 0 || item.UnitPrice <= 0 || item.Quantity <= 0 || item.Product is null || item.Product.Id == 0)
                {
                    throw new RepositoryException("Order Detail parameters are out of order");
                }

                var existingCategory = await this.northwindContext.Categories
                    .FirstOrDefaultAsync(x => x.CategoryId == item.Product.CategoryId);

                if (existingCategory == null)
                {
                    var orderCategory = new Category()
                    {
                        CategoryId = (int)item.Product.CategoryId,
                        CategoryName = item.Product.Category,
                    };
                    _ = await this.northwindContext.Categories.AddAsync(orderCategory);
                    int categoryResult = await this.northwindContext.SaveChangesAsync();
                    if (categoryResult <= 0)
                    {
                        await transaction.RollbackAsync();
                        throw new RepositoryException("Failed to create category record");
                    }
                }

                var existingProduct = await this.northwindContext.Products
                    .FirstOrDefaultAsync(x => x.ProductId == item.Product.Id);

                if (existingProduct == null)
                {
                    var category = await this.northwindContext.Categories
                        .FirstOrDefaultAsync(x => x.CategoryId == item.Product.CategoryId);

                    Entities.Product orderProduct = new Entities.Product
                    {
                        ProductId = (int)item.Product.Id,
                        ProductName = item.Product.ProductName,
                        CategoryId = category!.CategoryId,
                    };
                    _ = await this.northwindContext.Products.AddAsync(orderProduct);
                    int productResult = await this.northwindContext.SaveChangesAsync();
                    if (productResult <= 0)
                    {
                        await transaction.RollbackAsync();
                        throw new RepositoryException("Failed to create product record");
                    }
                }

                Entities.OrderDetail od = new Entities.OrderDetail
                {
                    OrderId = entityOrder.OrderId,
                    ProductId = (int)item.Product.Id,
                    Quantity = (int)item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Discount = item.Discount,
                };

                _ = await this.northwindContext.OrderDetails.AddAsync(od);
                int orderDetailResult = await this.northwindContext.SaveChangesAsync();
                if (orderDetailResult <= 0)
                {
                    await transaction.RollbackAsync();
                    throw new RepositoryException("Failed to create order detail record");
                }
            }

            await transaction.CommitAsync();

            return entityOrder.OrderId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            if (ex is InvalidOperationException)
            {
                throw new RepositoryException("Error processing order data", ex);
            }

            throw new RepositoryException($"Failed to add order: {ex.Message}", ex);
        }
    }

    public async Task RemoveOrderAsync(long orderId)
    {
        if (orderId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(orderId), "Order ID must be greater than zero.");
        }

        var orderToDelete = await this.northwindContext.Orders.FirstOrDefaultAsync(x => x.OrderId == orderId);

        if (orderToDelete is null)
        {
            throw new OrderNotFoundException($"Order with ID {orderId} not found.");
        }

        var orderDetails = await this.northwindContext.OrderDetails.Where(x => x.OrderId == orderId).ToListAsync();
        if (orderDetails.Count > 0)
        {
            this.northwindContext.OrderDetails.RemoveRange(orderDetails);
        }

        _ = this.northwindContext.Orders.Remove(orderToDelete);

        _ = await this.northwindContext.SaveChangesAsync();
    }

    public async Task UpdateOrderAsync(RepositoryOrder order)
    {
        if (order == null)
        {
            throw new ArgumentNullException(nameof(order), "Order cannot be null.");
        }

        var orderToUpdate = await this.northwindContext.Orders
            .FirstOrDefaultAsync(x => x.OrderId == order.Id);

        if (orderToUpdate is null)
        {
            throw new OrderNotFoundException($"Order with ID {order.Id} not found.");
        }

        orderToUpdate.OrderDate = order.OrderDate;
        orderToUpdate.RequiredDate = order.RequiredDate;
        orderToUpdate.ShippedDate = order.ShippedDate;
        orderToUpdate.Freight = order.Freight;
        orderToUpdate.ShipName = order.ShipName;
        orderToUpdate.ShipAddress = order.ShippingAddress?.Address;
        orderToUpdate.ShipCity = order.ShippingAddress?.City;
        orderToUpdate.ShipRegion = order.ShippingAddress?.Region;
        orderToUpdate.ShipPostalCode = order.ShippingAddress?.PostalCode;
        orderToUpdate.ShipCountry = order.ShippingAddress?.Country;
        orderToUpdate.CustomerId = order.Customer.Code.Code;
        orderToUpdate.EmployeeId = (int)order.Employee.Id;
        orderToUpdate.ShipVia = (int)order.Shipper.Id;
        orderToUpdate.Employee = await this.northwindContext.Employees.FirstOrDefaultAsync(x => x.EmployeeId == order.Employee.Id);
        orderToUpdate.Shipper = await this.northwindContext.Shippers.FirstOrDefaultAsync(x => x.ShipperId == order.Shipper.Id);
        orderToUpdate.Customer = await this.northwindContext.Customers.FirstOrDefaultAsync(x => x.CustomerId == order.Customer.Code.Code);

        var ordeusDetali = await this.northwindContext.OrderDetails.Where(x => x.OrderId == order.Id).ToListAsync();
        if (ordeusDetali.Count > 0)
        {
            this.northwindContext.OrderDetails.RemoveRange(ordeusDetali);

            _ = await this.northwindContext.SaveChangesAsync();
        }

        if (order.OrderDetails.Count > 0)
        {
            foreach (var item in order.OrderDetails)
            {
                if (item.Discount <= 0 || item.UnitPrice <= 0 || item.Quantity <= 0)
                {
                    throw new RepositoryException("Order Detail parameters are out of order");
                }

                var newOrderDetail = new Entities.OrderDetail
                {
                    Order = orderToUpdate,
                    OrderId = (int)order.Id,
                    Quantity = (int)item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Discount = item.Discount,
                    Product = (await this.northwindContext.Products!.FirstOrDefaultAsync(x => x.ProductId == item.Product.Id))
                        !,
                };
                newOrderDetail.Product.Supplier = (await this.northwindContext!.Suppliers!.FirstOrDefaultAsync(x => x.SupplierId! == item.Product!.SupplierId!))
                    !;
                newOrderDetail.Product.Category = (await this.northwindContext!.Categories!.FirstOrDefaultAsync(x => x.CategoryId! == item.Product!.CategoryId!))
                    !;
                newOrderDetail.ProductId = (int)item.Product.Id;
                orderToUpdate.OrderDetails.Add(newOrderDetail);
                _ = await this.northwindContext.OrderDetails.AddAsync(newOrderDetail);
                _ = await this.northwindContext.SaveChangesAsync();
            }
        }

        _ = this.northwindContext.Orders.Update(orderToUpdate);

        _ = await this.northwindContext.SaveChangesAsync();
    }
}
