namespace Microsoft.eShopWeb.Web.ViewModels;

public class OrderDetailViewModel : OrderViewModel
{
    public List<OrderItemViewModel> OrderItems { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal BookingFee { get; set; }
    public bool Refundable { get; set; }
}
