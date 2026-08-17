IF OBJECT_ID('dbo.CurrencyFeeFloor', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CurrencyFeeFloor (
        CurrencyCode CHAR(3)        NOT NULL PRIMARY KEY,
        MinimumFee   DECIMAL(18,4)  NOT NULL
    );
END
GO

MERGE dbo.CurrencyFeeFloor AS target
USING (VALUES
    ('GBP', 15.00),
    ('EUR', 18.00),
    ('USD', 20.00)
) AS source (CurrencyCode, MinimumFee)
ON target.CurrencyCode = source.CurrencyCode
WHEN MATCHED THEN UPDATE SET MinimumFee = source.MinimumFee
WHEN NOT MATCHED THEN INSERT (CurrencyCode, MinimumFee) VALUES (source.CurrencyCode, source.MinimumFee);
