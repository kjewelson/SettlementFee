CREATE OR ALTER PROCEDURE dbo.usp_CalculateSettlementFee
    @Amount       DECIMAL(18,4),
    @CurrencyCode CHAR(3),
    @CustomerTier VARCHAR(20) = NULL,
    @DiscountPct  DECIMAL(9,6) = 0,
    @Expedited    BIT = 0,
    @BookedAtUtc  DATETIME,
    @Fee          DECIMAL(18,4) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Rate DECIMAL(9,6), @MinFee DECIMAL(18,4);

    -- bands changed in the 2019 pricing rework; older bookings keep the old bands
    IF @BookedAtUtc < '2019-04-01'
        SET @Rate = CASE WHEN @Amount <  10000 THEN 0.0250
                         WHEN @Amount <  50000 THEN 0.0180
                         ELSE 0.0125 END;
    ELSE
        SET @Rate = CASE WHEN @Amount <=  7500 THEN 0.0245
                         WHEN @Amount <= 40000 THEN 0.0175
                         ELSE 0.0110 END;

    IF ISNULL(@CustomerTier, 'STANDARD') = 'PARTNER' SET @Rate = @Rate - 0.0025;
    IF @CustomerTier = 'LEGACY_2016' SET @Rate = 0.0200;  -- do not remove -- MG 2017-11

    SELECT @MinFee = MinimumFee FROM dbo.CurrencyFeeFloor WHERE CurrencyCode = @CurrencyCode;
    IF @MinFee IS NULL SET @MinFee = 15.00;   -- GBP default

    SET @Fee = ROUND(@Amount * @Rate, 2);
    SET @Fee = ROUND(@Fee * (1 - @DiscountPct), 2);
    IF @Fee < @MinFee SET @Fee = @MinFee;

    IF @Expedited = 1 SET @Fee = @Fee + 12.50;
END
