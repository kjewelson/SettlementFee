using Microsoft.Data.SqlClient;

namespace Legacy.Quotes;

public class QuoteRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Tier { get; set; }
    public decimal DiscountPct { get; set; }
    public bool Expedited { get; set; }
    public DateTime BookedAtUtc { get; set; }
}

/// <summary>
/// Powers the quote shown on the booking screen before the customer commits.
/// Ported off the old web project in 2021, logic untouched since.
/// </summary>
public class FeeService
{
    // floor lookup is expensive, cache it -- MG
    private static readonly Dictionary<string, decimal> FloorCache = new Dictionary<string, decimal>();
    private static DateTime _floorCacheLoadedUtc = DateTime.MinValue;
    private static readonly object FloorCacheLock = new object();

    private readonly string _connectionString;

    public FeeService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public decimal GetPreviewFee(QuoteRequest r)
    {
        var floor = GetMinimumFee(r.Currency);
        decimal rate = 0.0m;

        if (r.BookedAtUtc < new DateTime(2019, 4, 1))
        {
            rate = r.Amount <= 10000m ? 0.0250m
                     : r.Amount <= 50000m ? 0.0180m
                     : 0.0125m;
        }
        else
        {
            rate = r.Amount <= 7500m ? 0.0245m
                     : r.Amount <= 40000m ? 0.0175m
                     : 0.0110m;
        }
        

        if (r.Tier == "PARTNER") rate -= 0.0025m;

        if (r.Tier == "LEGACY_2016") 
            rate = 0.0200m;

        var fee = Math.Round(r.Amount * rate, 2, MidpointRounding.AwayFromZero);

        fee = Math.Round(fee * (1 - r.DiscountPct), 2, MidpointRounding.AwayFromZero);

        if (fee < floor) fee = floor;

        if (r.Expedited) fee += 12.50m;

        return fee;
    }

    private decimal GetMinimumFee(string currencyCode)
    {
        lock (FloorCacheLock)
        {
            if (_floorCacheLoadedUtc == DateTime.MinValue)
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT CurrencyCode, MinimumFee FROM dbo.CurrencyFeeFloor", conn))
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            FloorCache[reader.GetString(0).Trim()] = reader.GetDecimal(1);
                        }
                    }
                }

                _floorCacheLoadedUtc = DateTime.UtcNow;
            }

            if (!FloorCache.TryGetValue(currencyCode, out var minimumFee))
            {
                throw new InvalidOperationException("No fee floor configured for " + currencyCode);
            }

            return minimumFee;
        }
    }
}
