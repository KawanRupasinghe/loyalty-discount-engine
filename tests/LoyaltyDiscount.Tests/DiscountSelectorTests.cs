using NUnit.Framework;
using LoyaltyDiscount;

namespace LoyaltyDiscount.Tests;

[TestFixture]
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
        Assert.That(decision.Kind, Is.EqualTo(expectedKind));
        Assert.That((double)decision.Percentage, Is.EqualTo(expectedPct).Within(1e-9));
    }

    [Test]
    public void Final_price_is_rounded_to_2dp()
    {
        var d = DiscountSelector.Select(R(total: 123.456m, regular: true)); // 10%
        var price = DiscountSelector.ApplyFinalPrice(123.456m, d);
        Assert.That(price, Is.EqualTo(111.11m)); // 123.456 * 0.90 = 111.1104
    }

    [Test]
    public void Loyalty_over_regular_if_both_true()
    {
        var d = DiscountSelector.Select(R(loyalty: true, regular: true));
        Assert.That(d.Kind, Is.EqualTo(DiscountKind.Loyalty));
    }
}
