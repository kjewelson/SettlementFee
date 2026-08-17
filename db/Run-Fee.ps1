<#
.SYNOPSIS
    Calls dbo.usp_CalculateSettlementFee directly on LocalDB with whatever inputs you give it.
    This is the "ask production" tool -- use it to generate expected values for the tests
    in the rules module, and to probe edge cases.

.EXAMPLE
    .\db\Run-Fee.ps1 -Amount 12500 -Currency GBP -Tier PARTNER -DiscountPct 0.10 -BookedAtUtc 2018-06-01

.EXAMPLE
    .\db\Run-Fee.ps1 -Amount 7500 -Currency USD -BookedAtUtc 2024-01-01 -Expedited
#>

param(
    [Parameter(Mandatory = $true)] [decimal]$Amount,
    [Parameter(Mandatory = $true)] [string]$Currency,
    [string]$Tier = $null,
    [decimal]$DiscountPct = 0,
    [switch]$Expedited,
    [Parameter(Mandatory = $true)] [string]$BookedAtUtc
)

$instance = "(localdb)\MSSQLLocalDB"
$database = "SettlementFeeDb"

$tierLiteral = if ($Tier) { "'$Tier'" } else { "NULL" }
$expeditedBit = if ($Expedited) { 1 } else { 0 }

$sql = @"
DECLARE @Fee DECIMAL(18,4);
EXEC dbo.usp_CalculateSettlementFee
    @Amount = $Amount,
    @CurrencyCode = '$Currency',
    @CustomerTier = $tierLiteral,
    @DiscountPct = $DiscountPct,
    @Expedited = $expeditedBit,
    @BookedAtUtc = '$BookedAtUtc',
    @Fee = @Fee OUTPUT;
SELECT @Fee AS Fee;
"@

sqlcmd -S $instance -d $database -Q $sql -b
