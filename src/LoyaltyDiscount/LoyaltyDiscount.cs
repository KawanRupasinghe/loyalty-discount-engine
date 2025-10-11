using System;
using System.Collections.Generic;
using System.Linq;

namespace LoyaltyDiscount;

public enum CouponType { None = 0, Ten = 10, TwentyFive = 25 }

public enum DiscountKind
{
    None, Loyalty, Regular, BlackFriday, Coupon10, Coupon25
}

/// <summary>
/// Request describing an order and applicable discount flags.
/// </summary>
public readonly record struct DiscountRequest(
    decimal Total,
    bool IsLoyaltyMember,
    bool IsRegularCustomer,
    bool IsBlackFriday,
    CouponType Coupon
);

/// <summary>
/// The selected discount and its percentage (0..1).
/// </summary>
public readonly record struct DiscountDecision(DiscountKind Kind, decimal Percentage);

/// <summary>
/// Single-discount policy engine:
/// - Picks exactly one discount (no stacking)
/// - Highest percentage wins
/// - Tie-break: Loyalty &gt; Coupon(25,10) &gt; BlackFriday &gt; Regular
/// - Cap: 25%
/// </summary>
public static class DiscountSelector
{
    /// <summary>
    /// Selects exactly one discount according to the policy.
    /// Throws <see cref="ArgumentOutOfRangeException"/> if Total &lt; 0.
    /// </summary>
    public static DiscountDecision Select(DiscountRequest r)
    {
        if (r.Total < 0m)
            throw new ArgumentOutOfRangeException(nameof(r.Total), "Total must be non-negative.");

        var candidates = new List<DiscountDecision>();

        // Loyalty vs Regular (mutually exclusive by policy; Loyalty has priority if both are true)
        if (r.IsLoyaltyMember)
            candidates.Add(new(DiscountKind.Loyalty, 0.20m));
        else if (r.IsRegularCustomer)
            candidates.Add(new(DiscountKind.Regular, 0.10m));

        // Seasonal promo
        if (r.IsBlackFriday)
            candidates.Add(new(DiscountKind.BlackFriday, 0.05m));

        // Coupon per item
        if (r.Coupon == CouponType.Ten)
            candidates.Add(new(DiscountKind.Coupon10, 0.10m));
        else if (r.Coupon == CouponType.TwentyFive)
            candidates.Add(new(DiscountKind.Coupon25, 0.25m));

        if (candidates.Count == 0) return new(DiscountKind.None, 0m);

        var bestPct = candidates.Max(c => c.Percentage);

        // Tie-break priority
        DiscountKind[] priority =
        {
            DiscountKind.Loyalty, DiscountKind.Coupon25, DiscountKind.Coupon10,
            DiscountKind.BlackFriday, DiscountKind.Regular
        };

        var best = candidates
            .Where(c => c.Percentage == bestPct)
            .OrderBy(c => Array.IndexOf(priority, c.Kind))
            .First();

        // Cap at 25%
        var pct = Math.Min(best.Percentage, 0.25m);
        return best with { Percentage = pct };
    }

    /// <summary>
    /// Applies the discount to the total and rounds to 2 dp (AwayFromZero).
    /// </summary>
    public static decimal ApplyFinalPrice(decimal total, DiscountDecision d) =>
        Math.Round(total * (1 - d.Percentage), 2, MidpointRounding.AwayFromZero);
}
