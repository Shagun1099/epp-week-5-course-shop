# Decision log

## Booking Fee rule → moved to Order domain

- **What:** A $5 booking fee (waived at $50+) was calculated independently in three Web-layer files: `Basket/Index.cshtml`, `Basket/Checkout.cshtml.cs`, and `GetOrderDetailsHandler.cs`. The copies disagreed at exactly $50 because `Checkout.cshtml.cs` used `> 50` while the others used `>= 50`.
- **Decision:** Moved.
- **Where it lives now, and why that layer:** `Order.BookingFee(decimal subtotal)` in `src/ApplicationCore/Entities/OrderAggregate/Order.cs`. The booking fee is a pricing rule that belongs to the order contract. `ApplicationCore` is the domain layer; business rules that determine what a customer pays belong there, not in Razor Pages or Mediator handlers.

---

## BasketManager pass-through → left alone

- **What:** `src/Infrastructure/Data/BasketManager.cs` contains `AddItemToBasket`, `SetQuantities`, and `DeleteBasketAsync`. All three methods do nothing except call the identically-named method on `EfRepository<Basket>`. There is no business logic — the class is a pure delegation wrapper.
- **Decision:** Left alone.
- **Where it lives now, and why that layer:** Remains in `Infrastructure`. Removing it would require updating every caller (`CheckoutModel`, `BasketService`, DI registration) without any behaviour change. The class is a no-op wrapper, not a layering violation — the repository it wraps is already the right layer for data persistence.
- **What leaving it costs:** A small amount of indirection — callers go through `BasketManager` to reach the repository for no reason. If the project ever adds basket-level business logic (e.g., max-item limits), the right home already exists and the wrapper becomes genuinely useful. Cost of leaving it: negligible.
