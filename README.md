# Loyalty Discount Engine (.NET + NUnit)

A small, testable **.NET 9** rules engine that applies **one** discount per order (no stacking).  
**Policy:** Highest discount wins; **tie-break** = `Loyalty > Coupon > Black Friday > Regular`.  
**Rates:** Loyalty **20%**, Regular **10%**, Black Friday **5%**, Coupon **10%** or **25%** (max cap **25%**).  
**Price calc:** `final = total * (1 - pct)` (rounded to **2dp**, AwayFromZero).

---

## 🧰 Tech Stack
- C# / .NET 9
- NUnit (+ optional Coverlet for coverage)
- (Optional) Spectre.Console for a nicer CLI dashboard

---

## 🚀 Quick Start (Clone & Run)

```bash
# 1) Clone
git clone https://github.com/KawanRupasinghe/loyalty-discount-engine.git
cd loyalty-discount-engine

# 2) Restore & build
dotnet restore
dotnet build

# 3) Run CLI examples
dotnet run --project src/LoyaltyDiscount.Cli -- --regular --coupon 25 --total 250
dotnet run --project src/LoyaltyDiscount.Cli -- --loyalty --total 1200
dotnet run --project src/LoyaltyDiscount.Cli -- --coupon 10 --bf --total 99.99

# 4) Run tests
dotnet test tests/LoyaltyDiscount.Tests

.
├─ src/
│  ├─ LoyaltyDiscount/          # Class Library (business rules)
│  │  └─ DiscountEngine.cs
│  └─ LoyaltyDiscount.Cli/      # Console app (friendly runner)
│     └─ Program.cs
└─ tests/
   └─ LoyaltyDiscount.Tests/    # NUnit tests
      └─ DiscountSelectorTests.cs
