using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.Web.Features.OrderDetails;
using NSubstitute;
using Xunit;
using GetOrderDetailsRequest = Microsoft.eShopWeb.Web.Features.OrderDetails.GetOrderDetails;

namespace Microsoft.eShopWeb.UnitTests.MediatorHandlers.OrdersTests;

// Characterization tests that pin the booking-fee behaviour of GetOrderDetailsHandler
// before the rule is moved to the domain layer.
//
// Rule as currently implemented in GetOrderDetailsHandler.cs:30:
//   var bookingFee = subtotal < 50 ? 5m : 0m;
// Correct boundary: subtotal >= $50 → $0 fee; subtotal < $50 → $5 fee.
//
// KNOWN DISAGREEMENT AT $50:
// Checkout.cshtml.cs:39 uses "BasketModel.Total() > 50 ? 0 : 5" (strict greater-than).
// At exactly $50 that page charges $5 instead of $0. The handler is correct;
// the checkout page is the outlier. The refactor will fix Checkout.cshtml.cs to
// call the same domain method, eliminating the disagreement.
public class GetOrderDetailsBookingFee
{
    private static GetOrderDetailsHandler HandlerWithOrder(decimal unitPrice, int units)
    {
        var repo = Substitute.For<IReadRepository<Order>>();
        var item = new OrderItem(new CatalogItemOrdered(1, "Course", "uri"), unitPrice, units);
        var order = new Order("buyer", new Address("", "", "", "", ""), new List<OrderItem> { item });
        repo.FirstOrDefaultAsync(Arg.Any<OrderWithItemsByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(order);
        return new GetOrderDetailsHandler(repo);
    }

    [Fact]
    public async Task BookingFeeIsZeroWhenSubtotalIsExactlyFifty()
    {
        // "Leading Your First Project" costs exactly $50 in seed data.
        // Correct behaviour: subtotal == $50 → fee = $0, total = $50.
        var handler = HandlerWithOrder(unitPrice: 50.00m, units: 1);

        var result = await handler.Handle(
            new GetOrderDetailsRequest("user", 1), TestContext.Current.CancellationToken);

        Assert.Equal(0m, result!.BookingFee);
        Assert.Equal(50.00m, result.Subtotal);
        Assert.Equal(50.00m, result.Total);
    }

    [Fact]
    public async Task BookingFeeIsFiveWhenSubtotalIsJustBelowFifty()
    {
        var handler = HandlerWithOrder(unitPrice: 49.99m, units: 1);

        var result = await handler.Handle(
            new GetOrderDetailsRequest("user", 1), TestContext.Current.CancellationToken);

        Assert.Equal(5m, result!.BookingFee);
        Assert.Equal(54.99m, result.Total);
    }

    [Fact]
    public async Task BookingFeeIsZeroWhenSubtotalExceedsFifty()
    {
        var handler = HandlerWithOrder(unitPrice: 75.00m, units: 1);

        var result = await handler.Handle(
            new GetOrderDetailsRequest("user", 1), TestContext.Current.CancellationToken);

        Assert.Equal(0m, result!.BookingFee);
        Assert.Equal(75.00m, result.Total);
    }

    [Fact]
    public async Task BookingFeeIsFiveAndTotalIsSubtotalPlusFive()
    {
        var handler = HandlerWithOrder(unitPrice: 40.00m, units: 1);

        var result = await handler.Handle(
            new GetOrderDetailsRequest("user", 1), TestContext.Current.CancellationToken);

        Assert.Equal(5m, result!.BookingFee);
        Assert.Equal(40.00m, result.Subtotal);
        Assert.Equal(45.00m, result.Total);
    }
}
