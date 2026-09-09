using Legacy.Quotes;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

var connectionString = Environment.GetEnvironmentVariable("LEGACY_DB");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("LEGACY_DB is not set.");
}

var feeService = new FeeService(connectionString);

// GET /api/quote/preview?amount=12500&currency=GBP&tier=PARTNER&discountPct=0.10&expedited=false&bookedAtUtc=2018-06-01
app.MapGet("/api/quote/preview", (
    decimal amount,
    string currency,
    string tier,
    decimal? discountPct,
    bool? expedited,
    DateTime? bookedAtUtc) =>
{
    var request = new QuoteRequest
    {
        Amount = amount,
        Currency = currency,
        Tier = tier,
        DiscountPct = discountPct ?? 0m,
        Expedited = expedited ?? false,
        BookedAtUtc = bookedAtUtc ?? DateTime.UtcNow
    };

    return Results.Ok(new { fee = feeService.GetPreviewFee(request) });
});

app.Run();
