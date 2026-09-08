namespace Legacy.Quotes;

/// <summary>
/// </summary>
public static class FeeCalculator
{
    private static readonly DateTime PricingReworkCutoverUtc = new(2019, 4, 1);

    private const decimal PartnerRateDiscount = 0.0025m;
    private const decimal Legacy2016FlatRate = 0.0200m;
    private const decimal ExpeditedSurcharge = 12.50m;
    private const decimal DefaultFloor = 15.00m; // GBP default, matches proc's fallback

    private static readonly Dictionary<string, decimal> FloorsByCurrency = new(StringComparer.OrdinalIgnoreCase)
    {
        ["GBP"] = 15.00m,
        ["EUR"] = 18.00m,
        ["USD"] = 20.00m,
    };

    /// <summary>

    /// </summary>
    public static decimal Calculate(
        decimal amount,
        string currencyCode,
        string? customerTier,
        decimal discountPct,
        bool expedited,
        DateTime bookedAtUtc)
    {
        var rate = SelectRate(amount, bookedAtUtc);

        var tier = customerTier ?? "STANDARD";
        if (tier == "PARTNER") rate -= PartnerRateDiscount;
        if (tier == "LEGACY_2016") rate = Legacy2016FlatRate;

        var floor = GetFloor(currencyCode);

        var fee = Math.Round(amount * rate, 2, MidpointRounding.AwayFromZero);
        fee = Math.Round(fee * (1 - discountPct), 2, MidpointRounding.AwayFromZero);
        if (fee < floor) fee = floor;

        if (expedited) fee += ExpeditedSurcharge;

        return fee;
    }

    private static decimal SelectRate(decimal amount, DateTime bookedAtUtc)
    {
        if (bookedAtUtc < PricingReworkCutoverUtc)
        {
            // pre-2019 
            return amount < 10000m ? 0.0250m
                 : amount < 50000m ? 0.0180m
                 : 0.0125m;
        }

        // post-2019 
        return amount <= 7500m ? 0.0245m
             : amount <= 40000m ? 0.0175m
             : 0.0110m;
    }

    private static decimal GetFloor(string currencyCode) =>
        FloorsByCurrency.TryGetValue(currencyCode, out var floor) ? floor : DefaultFloor;
}