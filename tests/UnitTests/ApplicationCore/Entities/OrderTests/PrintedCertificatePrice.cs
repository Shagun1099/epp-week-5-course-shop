using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Entities.OrderTests;

public class PrintedCertificatePrice
{
    [Fact]
    public void CertificatePriceIsFifteen()
    {
        Assert.Equal(15m, Order.CertificatePrice);
    }

    [Fact]
    public void OrderWithoutCertificateDefaultsToFalse()
    {
        var order = new Order("buyer", new Address("", "", "", "", ""), new List<OrderItem>());

        Assert.False(order.PrintedCertificate);
    }

    [Fact]
    public void OrderWithCertificateSetsPropertyTrue()
    {
        var order = new Order("buyer", new Address("", "", "", "", ""), new List<OrderItem>(), printedCertificate: true);

        Assert.True(order.PrintedCertificate);
    }

    [Fact]
    public void OrderWithoutCertificateConstructorSetsPropertyFalse()
    {
        var order = new Order("buyer", new Address("", "", "", "", ""), new List<OrderItem>(), printedCertificate: false);

        Assert.False(order.PrintedCertificate);
    }
}
