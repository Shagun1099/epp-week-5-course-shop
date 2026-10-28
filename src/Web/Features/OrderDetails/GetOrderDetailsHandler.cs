using Mediator;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.Web.ViewModels;

namespace Microsoft.eShopWeb.Web.Features.OrderDetails;

public class GetOrderDetailsHandler : IRequestHandler<GetOrderDetails, OrderDetailViewModel?>
{
    private readonly IReadRepository<Order> _orderRepository;

    public GetOrderDetailsHandler(IReadRepository<Order> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async ValueTask<OrderDetailViewModel?> Handle(GetOrderDetails request,
        CancellationToken cancellationToken)
    {
        var spec = new OrderWithItemsByIdSpec(request.OrderId);
        var order = await _orderRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (order == null)
        {
            return null;
        }

        var subtotal = order.OrderItems.Sum(i => i.UnitPrice * i.Units);
        var bookingFee = Order.BookingFee(subtotal);
        var certPrice = order.PrintedCertificate ? Order.CertificatePrice : 0m;

        return new OrderDetailViewModel
        {
            OrderDate = order.OrderDate,
            OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
            {
                PictureUrl = oi.ItemOrdered.PictureUri,
                ProductId = oi.ItemOrdered.CatalogItemId,
                ProductName = oi.ItemOrdered.ProductName,
                UnitPrice = oi.UnitPrice,
                Units = oi.Units
            }).ToList(),
            OrderNumber = order.Id,
            ShippingAddress = order.ShipToAddress,
            Subtotal = subtotal,
            BookingFee = bookingFee,
            PrintedCertificate = order.PrintedCertificate,
            CertificatePrice = certPrice,
            Total = subtotal + bookingFee + certPrice,
            Refundable = (DateTimeOffset.UtcNow - order.OrderDate).TotalDays < 14
        };
    }
}
