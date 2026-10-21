using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.Extensions.Logging;

namespace Microsoft.eShopWeb.Infrastructure.Data;

public class CatalogContextSeed
{
    public static async Task SeedAsync(CatalogContext catalogContext,
        ILogger logger,
        int retry = 0)
    {
        var retryForAvailability = retry;
        try
        {
            if (catalogContext.Database.IsSqlServer())
            {
                catalogContext.Database.Migrate();
            }

            if (!await catalogContext.CatalogBrands.AnyAsync())
            {
                await catalogContext.CatalogBrands.AddRangeAsync(
                    GetPreconfiguredCatalogBrands());

                await catalogContext.SaveChangesAsync();
            }

            if (!await catalogContext.CatalogTypes.AnyAsync())
            {
                await catalogContext.CatalogTypes.AddRangeAsync(
                    GetPreconfiguredCatalogTypes());

                await catalogContext.SaveChangesAsync();
            }

            if (!await catalogContext.CatalogItems.AnyAsync())
            {
                await catalogContext.CatalogItems.AddRangeAsync(
                    GetPreconfiguredItems());

                await catalogContext.SaveChangesAsync();
            }

            if (!await catalogContext.Orders.AnyAsync())
            {
                await catalogContext.Orders.AddRangeAsync(
                    GetPreconfiguredOrders(await catalogContext.CatalogItems.ToListAsync()));

                await catalogContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            if (retryForAvailability >= 10) throw;

            retryForAvailability++;
            
            logger.LogError(ex.Message);
            await SeedAsync(catalogContext, logger, retryForAvailability);
            throw;
        }
    }

    static IEnumerable<CatalogBrand> GetPreconfiguredCatalogBrands()
    {
        return new List<CatalogBrand>
            {
                new("Northfield Academy"),
                new("Brightpath Learning"),
                new("Harbour Institute"),
                new("Open Workshop Co.")
            };
    }

    static IEnumerable<CatalogType> GetPreconfiguredCatalogTypes()
    {
        return new List<CatalogType>
            {
                new("Data"),
                new("Design"),
                new("Leadership"),
                new("Software")
            };
    }

    static IEnumerable<CatalogItem> GetPreconfiguredItems()
    {
        return new List<CatalogItem>
            {
                new(1,1, "Spreadsheets to dashboards in a weekend.", "Data Storytelling Basics", 25M, "http://catalogbaseurltobereplaced/images/products/1.svg"),
                new(1,2, "Clean, join and summarise data with SQL.", "SQL for Analysts", 40M, "http://catalogbaseurltobereplaced/images/products/2.svg"),
                new(2,3, "Layout, type and colour for non-designers.", "Visual Design Foundations", 30M, "http://catalogbaseurltobereplaced/images/products/3.svg"),
                new(2,1, "Run a usability test with five people.", "Usability Testing on a Budget", 12.50M, "http://catalogbaseurltobereplaced/images/products/4.svg"),
                new(3,4, "Give feedback people can act on.", "Feedback That Lands", 15M, "http://catalogbaseurltobereplaced/images/products/5.svg"),
                new(3,2, "Plan, run and close a team project.", "Leading Your First Project", 50M, "http://catalogbaseurltobereplaced/images/products/6.svg"),
                new(4,3, "Version control for whole teams.", "Git for Teams", 20M, "http://catalogbaseurltobereplaced/images/products/7.svg"),
                new(4,1, "Write tests before you change code.", "Testing Legacy Code", 35M, "http://catalogbaseurltobereplaced/images/products/8.svg"),
                new(1,4, "Read charts and spot misleading ones.", "Chart Literacy", 10M, "http://catalogbaseurltobereplaced/images/products/9.svg"),
                new(2,4, "Write interface text that helps.", "UX Writing Essentials", 22.50M, "http://catalogbaseurltobereplaced/images/products/10.svg"),
                new(3,3, "Chair meetings that end on time.", "Better Meetings", 12.50M, "http://catalogbaseurltobereplaced/images/products/11.svg"),
                new(4,2, "Design small, clear HTTP APIs.", "API Design Fundamentals", 45M, "http://catalogbaseurltobereplaced/images/products/12.svg")
            };
    }

    static IEnumerable<Order> GetPreconfiguredOrders(List<CatalogItem> courses)
    {
        const string buyerId = "learner@courseshop.test";
        var address = new Address("100 King St W", "Toronto", "ON", "Canada", "M5X 1A9");
        var today = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);

        OrderItem Line(string courseName, int seats)
        {
            var course = courses.First(c => c.Name == courseName);
            return new OrderItem(new CatalogItemOrdered(course.Id, course.Name, course.PictureUri), course.Price, seats);
        }

        return new List<Order>
            {
                new(buyerId, address, new List<OrderItem> { Line("Better Meetings", 1) }, today.AddDays(-3)),
                new(buyerId, address, new List<OrderItem> { Line("SQL for Analysts", 1), Line("Chart Literacy", 1) }, today.AddDays(-10)),
                new(buyerId, address, new List<OrderItem> { Line("Feedback That Lands", 2) }, today.AddDays(-11)),
                new(buyerId, address, new List<OrderItem> { Line("Leading Your First Project", 1) }, today.AddDays(-12)),
                new(buyerId, address, new List<OrderItem> { Line("Git for Teams", 1), Line("Usability Testing on a Budget", 1) }, today.AddDays(-13)),
                new(buyerId, address, new List<OrderItem> { Line("Data Storytelling Basics", 1) }, today.AddDays(-14)),
                new(buyerId, address, new List<OrderItem> { Line("API Design Fundamentals", 1) }, today.AddDays(-21)),
                new(buyerId, address, new List<OrderItem> { Line("UX Writing Essentials", 1) }, today.AddDays(-30))
            };
    }
}
