using NUnit.Framework;
using LoyaltyDiscount;

namespace LoyaltyDiscount.Tests;

[Category("Policy")]
public class TieBreakAndPolicyTests
{
    static DiscountRequest R(
        decimal total = 100m,
        bool loyalty = false,
        bool regular = false,
        bool bf = false,
        CouponType coupon = CouponType.None
    ) => new(total, loyalty, regular, bf, coupon);

    [Test]
    [Category("TieBreak")]
    public void Loyalty_over_regular_if_both_true()
    {
        var d = DiscountSelector.Select(R(loyalty:true, regular:true));
        TestContext.Out.WriteLine($"Both true => {d.Kind} ({d.Percentage:P0})");
        Assert.That(d.Kind, Is.EqualTo(DiscountKind.Loyalty));
    }

    [Test]
    [Category("TieBreak")]
    public void Coupon10_over_regular10_on_tie()
    {
        var d = DiscountSelector.Select(R(regular:true, coupon:CouponType.Ten));
        Assert.That(d.Kind, Is.EqualTo(DiscountKind.Coupon10));
        Assert.That(d.Percentage, Is.EqualTo(0.10m));
    }

    [Test]
    [Category("Cap")]
    public void Overall_cap_is_25_percent()
    {
        var d = DiscountSelector.Select(R(loyalty:true, bf:true, coupon:CouponType.TwentyFive));
        Assert.That(d.Kind, Is.EqualTo(DiscountKind.Coupon25), "25% coupon must win under highest-wins.");
        Assert.That(d.Percentage, Is.EqualTo(0.25m), "Must be capped at 25% (not exceed).");
    }

    [Test]
    [Category("Purity")]
    public void Selector_is_idempotent_for_same_input()
    {
        var req = R(199.99m, loyalty:false, regular:true, bf:true, coupon:CouponType.Ten);
        var a = DiscountSelector.Select(req);
        var b = DiscountSelector.Select(req);

        Assert.Multiple(() =>
        {
            Assert.That(a.Kind, Is.EqualTo(b.Kind));
            Assert.That(a.Percentage, Is.EqualTo(b.Percentage));
        });
    }
}
