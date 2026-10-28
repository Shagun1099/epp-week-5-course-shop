# Rule trace

## The rule

Charge a $5 booking fee when the order subtotal is below $50; charge nothing at $50 or above.

## Every place it lived

| File | What this place does with the rule | Agrees with the others? |
|---|---|---|
| `src/Web/Pages/Basket/Index.cshtml` | Calculates and displays the fee in the cart summary | Yes — used `>= 50 ? 0m : 5m`, correct boundary |
| `src/Web/Pages/Basket/Checkout.cshtml.cs` | Calculates the fee to display on the checkout page | **No** — used `> 50 ? 0 : 5` (strict greater-than), so at exactly $50 it charged $5 instead of $0 |
| `src/Web/Features/OrderDetails/GetOrderDetailsHandler.cs` | Calculates the fee when building the order-detail view model | Yes — used `subtotal < 50 ? 5m : 0m`, correct boundary |

### Disagreement at exactly $50

At a subtotal of exactly $50 the three copies produced different results:

- **Basket page** showed: subtotal $50.00 · booking fee $0.00 · total **$50.00**
- **Checkout page** showed: booking fee **$5.00** · total **$55.00** (off by $5)
- **Order detail** (after placing the order) showed: booking fee $0.00 · total **$50.00**

A customer buying a single $50 course would see the total change from $50 to $55 between the cart and the checkout screen, then revert to $50 on the receipt.

## Where it belongs

`ApplicationCore` — specifically `Order.BookingFee(decimal subtotal)` in `src/ApplicationCore/Entities/OrderAggregate/Order.cs`.

The booking fee is a pricing rule that is part of the order contract. Keeping it on the `Order` aggregate root means the rule has one authoritative definition and the three callers above now all delegate to it, so the disagreement at $50 cannot recur.
