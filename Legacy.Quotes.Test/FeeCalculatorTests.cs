using Xunit;
using Legacy.Quotes;

namespace Legacy.Quotes.Test;

public class FeeCalculatorTests
{
    private static readonly DateTime Post2019 = new(2024, 1, 1);
    private static readonly DateTime Pre2019 = new(2019, 3, 31);
    
    // --- rate bands, post-2019, mid-tier ---
    [Fact]
    public void MidBand_Standard_ReturnsExpectedFee()
    {
        var fee = FeeCalculator.Calculate(20000m, "GBP", null, 0m, false, Post2019);
        Assert.Equal(350.00m, fee); // 20000 * 0.0175
    }

    // --- boundary: exactly 7500, post-2019 (should use 2.45%, not 1.75%) ---
    [Fact]
    public void Boundary_7500_UsesLowerBand()
    {
        var fee = FeeCalculator.Calculate(7500m, "GBP", null, 0m, false, Post2019);
        Assert.Equal(183.75m, fee); // 7500 * 0.0245
    }

    [Fact]
    public void Boundary_7500_01_UsesNextBand()
    {
        var fee = FeeCalculator.Calculate(7500.01m, "GBP", null, 0m, false, Post2019);
        Assert.Equal(131.25m, fee); // 7500.01 * 0.0175, rounded
    }

    // --- boundary: exactly 40000 ---
    [Fact]
    public void Boundary_40000_UsesMidBand()
    {
        var fee = FeeCalculator.Calculate(40000m, "GBP", null, 0m, false, Post2019);
        Assert.Equal(700.00m, fee); // 40000 * 0.0175
    }

    [Fact]
    public void Boundary_40000_01_UsesTopBand()
    {
        var fee = FeeCalculator.Calculate(40000.01m, "GBP", null, 0m, false, Post2019);
        Assert.Equal(440.00m, fee); // 40000.01 * 0.0110, rounded
    }

    // --- pre-2019 date branch, different boundary operator (< not <=) ---
    [Fact]
    public void PreCutover_Boundary_10000_UsesNextBand()
    {
        // proc uses strict "<", so exactly 10000 falls OUT of the first band
        var fee = FeeCalculator.Calculate(10000m, "GBP", null, 0m, false, Pre2019);
        Assert.Equal(180.00m, fee); // 10000 * 0.0180
    }

    [Fact]
    public void PreCutover_9999_99_UsesFirstBand()
    {
        // rounding edge case — verify against Run-Fee.ps1
        var fee = FeeCalculator.Calculate(9999.99m, "GBP", null, 0m, false, Pre2019);
        Assert.Equal(250.00m, fee); // 9999.99 * 0.0250 = 249.99975, rounds up
    }

    // --- tier adjustments ---
    [Fact]
    public void PartnerTier_ReducesRate()
    {
        var fee = FeeCalculator.Calculate(20000m, "GBP", "PARTNER", 0m, false, Post2019);
        Assert.Equal(300.00m, fee); // rate 0.0175 - 0.0025 = 0.0150
    }

    [Fact]
    public void Legacy2016Tier_FlatRateOverridesEverything()
    {
        // should override band AND ignore amount/date entirely
        var fee = FeeCalculator.Calculate(20000m, "GBP", "LEGACY_2016", 0m, false, Post2019);
        Assert.Equal(400.00m, fee); // 20000 * 0.0200
    }

    // --- floor behaviour ---
    [Fact]
    public void SmallAmount_GBP_HitsFloor()
    {
        var fee = FeeCalculator.Calculate(100m, "GBP", null, 0m, false, Post2019);
        Assert.Equal(15.00m, fee);
    }

    [Fact]
    public void SmallAmount_EUR_HitsCurrencyFloor()
    {
        var fee = FeeCalculator.Calculate(100m, "EUR", null, 0m, false, Post2019);
        Assert.Equal(18.00m, fee);
    }

    [Fact]
    public void SmallAmount_USD_HitsCurrencyFloor()
    {
        var fee = FeeCalculator.Calculate(100m, "USD", null, 0m, false, Post2019);
        Assert.Equal(20.00m, fee);
    }

    [Fact]
    public void UnknownCurrency_DefaultsToFifteen()
    {
        var fee = FeeCalculator.Calculate(100m, "XXX", null, 0m, false, Post2019);
        Assert.Equal(15.00m, fee);
    }

    [Fact]
    public void Discount_AppliedBeforeFloor_StillHitsFloor()
    {
        // raw fee (2.45) discounted down further (2.21) is still below floor,
        // so the final result should be the floor, not the discounted sub-floor value
        var fee = FeeCalculator.Calculate(100m, "GBP", null, 0.10m, false, Post2019);
        Assert.Equal(15.00m, fee);
    }

    [Fact]
    public void Discount_AppliedAboveFloor()
    {
        var fee = FeeCalculator.Calculate(20000m, "GBP", null, 0.10m, false, Post2019);
        Assert.Equal(315.00m, fee); // 350.00 * 0.90
    }

    // --- expedited surcharge, applied last, never discounted/floored ---
    [Fact]
    public void Expedited_AddsFlatSurchargeAfterEverything()
    {
        var fee = FeeCalculator.Calculate(20000m, "GBP", null, 0m, true, Post2019);
        Assert.Equal(362.50m, fee); // 350.00 + 12.50
    }
}