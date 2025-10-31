using NUnit.Framework;
using LoyaltyDiscount;

namespace LoyaltyDiscount.Tests;

[Category("Selection")]
[Description("Verifies single-discount selection, highest-wins and tie-break rules")]
public class DiscountSelectorTests
{
    static DiscountRequest R(
        decimal total = 100m,
        bool loyalty = false,
        bool regular = false,
        bool bf = false,
        CouponType coupon = CouponType.None
    ) => new(total, loyalty, regular, bf, coupon);

    [TestCase(true,  false, false, CouponType.None,        DiscountKind.Loyalty,     0.20)]
    [TestCase(true,  false, true,  CouponType.None,        DiscountKind.Loyalty,     0.20)]
    [TestCase(true,  false, false, CouponType.Ten,         DiscountKind.Loyalty,     0.20)]
    [TestCase(true,  false, false, CouponType.TwentyFive,  DiscountKind.Coupon25,    0.25)]
    [TestCase(false, true,  false, CouponType.None,        DiscountKind.Regular,     0.10)]
    [TestCase(false, true,  true,  CouponType.None,        DiscountKind.Regular,     0.10)]
    [TestCase(false, true,  false, CouponType.TwentyFive,  DiscountKind.Coupon25,    0.25)]
    [TestCase(false, true,  false, CouponType.Ten,         DiscountKind.Coupon10,    0.10)]
    [TestCase(false, false, true,  CouponType.None,        DiscountKind.BlackFriday, 0.05)]
    [TestCase(false, false, false, CouponType.Ten,         DiscountKind.Coupon10,    0.10)]
    [TestCase(false, false, true,  CouponType.Ten,         DiscountKind.Coupon10,    0.10)]
    [TestCase(false, false, true,  CouponType.TwentyFive,  DiscountKind.Coupon25,    0.25)]
    public void Selects_expected_discount(
        bool loyalty, bool regular, bool bf, CouponType coupon,
        DiscountKind expectedKind, double expectedPct)
    {
        var decision = DiscountSelector.Select(R(100m, loyalty, regular, bf, coupon));

    TestContext.Out.WriteLine($"Input => loyalty={loyalty}, regular={regular}, bf={bf}, coupon={coupon}");
    TestContext.Out.WriteLine($"Applied => {decision.Kind} ({decision.Percentage:P0}) Expect => {expectedKind} ({expectedPct:P0})");

        Assert.That(decision.Kind, Is.EqualTo(expectedKind), "Wrong discount kind selected.");
        Assert.That((double)decision.Percentage, Is.EqualTo(expectedPct).Within(1e-9), "Wrong percentage applied.");
    }

    [Test]
    [Category("TieBreak")]
    public void Loyalty_over_regular_if_both_true()
    {
        var d = DiscountSelector.Select(R(loyalty: true, regular: true));
    TestContext.Out.WriteLine($"Both flags true => Applied {d.Kind}");
        Assert.That(d.Kind, Is.EqualTo(DiscountKind.Loyalty), "Loyalty must outrank Regular when both are true.");
    }

    [Test]
    [Category("Rounding")]
    public void Final_price_is_rounded_to_2dp()
    {
        var d = DiscountSelector.Select(R(total: 123.456m, regular: true)); // 10%
        var price = DiscountSelector.ApplyFinalPrice(123.456m, d);
    TestContext.Out.WriteLine($"Rounded Price => {price}");
        Assert.That(price, Is.EqualTo(111.11m)); // 123.456 * 0.90 = 111.1104 -> 111.11
    }

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
    public void AwayFromZero_rounding_example()
    {
        var price = DiscountSelector.ApplyFinalPrice(199.995m, new(DiscountKind.Coupon10, 0.10m));
        // 199.995 * 0.90 = 179.9955 -> AwayFromZero => 180.00
        Assert.That(price, Is.EqualTo(180.00m));
    }
}
