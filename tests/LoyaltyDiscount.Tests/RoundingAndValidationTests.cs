using NUnit.Framework;
using LoyaltyDiscount;

namespace LoyaltyDiscount.Tests;

[Category("ValidationAndRounding")]
public class RoundingAndValidationTests
{
    [Test]
    [Category("Validation")]
    public void Negative_total_throws()
    {
        Assert.That(
            () => DiscountSelector.Select(new(-1m, false, false, false, CouponType.None)),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    [Category("Rounding")]
    public void Price_rounds_away_from_zero_at_half_cent()
    {
        var decision = new DiscountDecision(DiscountKind.Coupon10, 0.10m);
        var price = DiscountSelector.ApplyFinalPrice(199.995m, decision);
        TestContext.Out.WriteLine($"Rounded => {price}");
        Assert.That(price, Is.EqualTo(180.00m)); // 199.995*0.90 = 179.9955 -> 180.00
    }

    [Test]
    [Category("Rounding")]
    public void Price_rounds_down_when_below_half_cent()
    {
        var d = new DiscountDecision(DiscountKind.Regular, 0.10m);
        var p = DiscountSelector.ApplyFinalPrice(123.456m, d);
        Assert.That(p, Is.EqualTo(111.11m)); // 123.456*0.90 = 111.1104 -> 111.11
    }

    [Test]
    [Category("Rounding")]
    public void Large_totals_are_handled_correctly()
    {
        var d = new DiscountDecision(DiscountKind.Coupon25, 0.25m);
        var p = DiscountSelector.ApplyFinalPrice(100_000.99m, d);
        Assert.That(p, Is.EqualTo(Math.Round(100_000.99m * 0.75m, 2, MidpointRounding.AwayFromZero)));
    }
}
