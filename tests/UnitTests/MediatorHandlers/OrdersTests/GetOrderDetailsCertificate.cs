using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.Web.Features.OrderDetails;
using NSubstitute;
using Xunit;
using GetOrderDetailsRequest = Microsoft.eShopWeb.Web.Features.OrderDetails.GetOrderDetails;

namespace Microsoft.eShopWeb.UnitTests.MediatorHandlers.OrdersTests;

public class GetOrderDetailsCertificate
{
    private static GetOrderDetailsHandler HandlerWithOrder(decimal unitPrice, int units, bool printedCertificate)
    {
        var repo = Substitute.For<IReadRepository<Order>>();
        var item = new OrderItem(new CatalogItemOrdered(1, "Course", "uri"), unitPrice, units);
        var order = new Order("buyer", new Address("", "", "", "", ""), new List<OrderItem> { item }, printedCertificate);
        repo.FirstOrDefaultAsync(Arg.Any<OrderWithItemsByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(order);
        return new GetOrderDetailsHandler(repo);
    }

    [Fact]
    public async Task NoCertificateAddsZeroToTotal()
    {
        var handler = HandlerWithOrder(unitPrice: 75.00m, units: 1, printedCertificate: false);

        var result = await handler.Handle(
            new GetOrderDetailsRequest("user", 1), TestContext.Current.CancellationToken);

        Assert.False(result!.PrintedCertificate);
        Assert.Equal(0m, result.CertificatePrice);
        Assert.Equal(75.00m, result.Total);
    }

    [Fact]
    public async Task WithCertificateAddsFifteenToTotal()
    {
        var handler = HandlerWithOrder(unitPrice: 75.00m, units: 1, printedCertificate: true);

        var result = await handler.Handle(
            new GetOrderDetailsRequest("user", 1), TestContext.Current.CancellationToken);

        Assert.True(result!.PrintedCertificate);
        Assert.Equal(15m, result.CertificatePrice);
        Assert.Equal(90.00m, result.Total);
    }

    [Fact]
    public async Task CertificateIsIncludedWhenSubtotalAlsoTriggersBookingFee()
    {
        // subtotal $40 → booking fee $5 + certificate $15 → total $60
        var handler = HandlerWithOrder(unitPrice: 40.00m, units: 1, printedCertificate: true);

        var result = await handler.Handle(
            new GetOrderDetailsRequest("user", 1), TestContext.Current.CancellationToken);

        Assert.Equal(5m, result!.BookingFee);
        Assert.Equal(15m, result.CertificatePrice);
        Assert.Equal(60.00m, result.Total);
    }

    [Fact]
    public async Task CertificateDoesNotAffectBookingFeeCalculation()
    {
        // The $15 certificate is added after the booking fee threshold check — it must not change the fee
        var handler = HandlerWithOrder(unitPrice: 45.00m, units: 1, printedCertificate: true);

        var result = await handler.Handle(
            new GetOrderDetailsRequest("user", 1), TestContext.Current.CancellationToken);

        Assert.Equal(5m, result!.BookingFee);   // subtotal $45 < $50 → fee applies
        Assert.Equal(15m, result.CertificatePrice);
        Assert.Equal(65.00m, result.Total);
    }
}
