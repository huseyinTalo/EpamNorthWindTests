using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Proxies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Northwind.Services.EntityFramework.Entities;
using Northwind.Services.EntityFramework.Repositories;
using Northwind.Services.Repositories;

namespace Northwind.Services.EntityFramework.DependencyInjection
{
    public static class EntityFrameworkDI
    {
        public static IServiceCollection AddEFServices(this IServiceCollection services, string connectionString)
        {
            _ = services.AddDbContext<NorthwindContext>(options =>
    options.UseLazyLoadingProxies()
           .UseSqlite(connectionString));

            _ = services.AddScoped<IOrderRepository, OrderRepository>();

            return services;
        }
    }
}
