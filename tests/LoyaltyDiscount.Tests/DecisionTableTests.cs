using NUnit.Framework;
using LoyaltyDiscount;

namespace LoyaltyDiscount.Tests;

/// <summary>
/// Decision table to verify: single discount, highest wins, no stacking.
/// </summary>
[Category("DecisionTable")]
public class DecisionTableTests
{
    public record Case(
        string Name,
        DiscountRequest Request,
        DiscountKind ExpectedKind,
        decimal ExpectedPct
    );

    // helper to avoid named-arg typos
    static DiscountRequest Req(
        decimal total, bool loyalty, bool regular, bool bf, CouponType coupon
    ) => new(total, loyalty, regular, bf, coupon);

    public static readonly Case[] Rows = new[]
    {
        new Case("Loyalty only",
            Req(100, loyalty:true,  regular:false, bf:false, coupon:CouponType.None),
            DiscountKind.Loyalty, 0.20m),

        new Case("Loyalty vs BF -> Loyalty",
            Req(100, loyalty:true,  regular:false, bf:true,  coupon:CouponType.None),
            DiscountKind.Loyalty, 0.20m),

        new Case("Coupon25 beats Loyalty20",
            Req(100, loyalty:true,  regular:false, bf:false, coupon:CouponType.TwentyFive),
            DiscountKind.Coupon25, 0.25m),

        new Case("Regular only",
            Req(100, loyalty:false, regular:true,  bf:false, coupon:CouponType.None),
            DiscountKind.Regular, 0.10m),

        new Case("Regular vs BF -> Regular",
            Req(100, loyalty:false, regular:true,  bf:true,  coupon:CouponType.None),
            DiscountKind.Regular, 0.10m),

        new Case("Coupon25 beats Regular10",
            Req(100, loyalty:false, regular:true,  bf:false, coupon:CouponType.TwentyFive),
            DiscountKind.Coupon25, 0.25m),

        new Case("Tie 10%: Coupon10 over Regular10",
            Req(100, loyalty:false, regular:true,  bf:false, coupon:CouponType.Ten),
            DiscountKind.Coupon10, 0.10m),

        new Case("BF only",
            Req(100, loyalty:false, regular:false, bf:true,  coupon:CouponType.None),
            DiscountKind.BlackFriday, 0.05m),

        new Case("Coupon10 only",
            Req(100, loyalty:false, regular:false, bf:false, coupon:CouponType.Ten),
            DiscountKind.Coupon10, 0.10m),

        new Case("Coupon10 vs BF -> Coupon10",
            Req(100, loyalty:false, regular:false, bf:true,  coupon:CouponType.Ten),
            DiscountKind.Coupon10, 0.10m),

        new Case("Coupon25 vs BF -> Coupon25",
            Req(100, loyalty:false, regular:false, bf:true,  coupon:CouponType.TwentyFive),
            DiscountKind.Coupon25, 0.25m),

        new Case("No flags -> None",
            Req(100, loyalty:false, regular:false, bf:false, coupon:CouponType.None),
            DiscountKind.None, 0.00m),
    };

    [TestCaseSource(nameof(Rows))]
    public void Table_row_passes(Case row)
    {
        var d = DiscountSelector.Select(row.Request);
        TestContext.Out.WriteLine($"[{row.Name}] Input => {row.Request}");
        TestContext.Out.WriteLine($"Applied => {d.Kind} ({d.Percentage:P0}) | Expected => {row.ExpectedKind} ({row.ExpectedPct:P0})");

        Assert.That(d.Kind, Is.EqualTo(row.ExpectedKind), $"{row.Name}: wrong discount kind.");
        Assert.That(d.Percentage, Is.EqualTo(row.ExpectedPct), $"{row.Name}: wrong percentage.");
    }
}
