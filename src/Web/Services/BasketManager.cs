using Ardalis.Result;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Web.Interfaces;

namespace Microsoft.eShopWeb.Web.Services;

public class BasketManager : IBasketManager
{
    private readonly IBasketService _basketService;

    public BasketManager(IBasketService basketService)
    {
        _basketService = basketService;
    }

    public async Task<Basket> AddItemToBasket(string username, int catalogItemId, decimal price, int quantity = 1)
    {
        return await _basketService.AddItemToBasket(username, catalogItemId, price, quantity);
    }

    public async Task<Result<Basket>> SetQuantities(int basketId, Dictionary<string, int> quantities)
    {
        return await _basketService.SetQuantities(basketId, quantities);
    }

    public async Task DeleteBasketAsync(int basketId)
    {
        await _basketService.DeleteBasketAsync(basketId);
    }
}
