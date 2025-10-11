using System;
using System.Collections.Generic;
using System.Linq;

namespace LoyaltyDiscount;

public enum CouponType { None = 0, Ten = 10, TwentyFive = 25 }

public enum DiscountKind
{
    None, Loyalty, Regular, BlackFriday, Coupon10, Coupon25
}

public readonly record struct DiscountRequest(
    decimal Total,
    bool IsLoyaltyMember,
    bool IsRegularCustomer,
    bool IsBlackFriday,
    CouponType Coupon
);

public readonly record struct DiscountDecision(DiscountKind Kind, decimal Percentage);

public static class DiscountSelector
{
    // No stacking: pick highest discount. Tie-break: Loyalty > Coupon > BlackFriday > Regular
    public static DiscountDecision Select(DiscountRequest r)
    {
        var candidates = new List<DiscountDecision>();

        if (r.IsLoyaltyMember)       candidates.Add(new(DiscountKind.Loyalty, 0.20m));
        else if (r.IsRegularCustomer) candidates.Add(new(DiscountKind.Regular, 0.10m));
        if (r.IsBlackFriday)          candidates.Add(new(DiscountKind.BlackFriday, 0.05m));
        if (r.Coupon == CouponType.Ten)        candidates.Add(new(DiscountKind.Coupon10, 0.10m));
        else if (r.Coupon == CouponType.TwentyFive) candidates.Add(new(DiscountKind.Coupon25, 0.25m));

        if (candidates.Count == 0) return new(DiscountKind.None, 0m);

        var bestPct = candidates.Max(c => c.Percentage);

        DiscountKind[] priority = {
            DiscountKind.Loyalty, DiscountKind.Coupon25, DiscountKind.Coupon10,
            DiscountKind.BlackFriday, DiscountKind.Regular
        };

        var best = candidates
            .Where(c => c.Percentage == bestPct)
            .OrderBy(c => Array.IndexOf(priority, c.Kind))
            .First();

        var pct = Math.Min(best.Percentage, 0.25m); // cap at 25%
        return best with { Percentage = pct };
    }

    public static decimal ApplyFinalPrice(decimal total, DiscountDecision d) =>
        Math.Round(total * (1 - d.Percentage), 2, MidpointRounding.AwayFromZero);
}
