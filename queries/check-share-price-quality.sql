
WITH PriceAnalysis AS (
    SELECT 
        StockPriceId,
        StockSymbol,
        Date,
        Price,
        Currency,
        Source,
        LAG(Date, 1) OVER (PARTITION BY StockSymbol ORDER BY Date) AS PreviousDate,
        LAG(Price, 1) OVER (PARTITION BY StockSymbol ORDER BY Date) AS PreviousPrice,
        LEAD(Date, 1) OVER (PARTITION BY StockSymbol ORDER BY Date) AS NextDate,
        LEAD(Price, 1) OVER (PARTITION BY StockSymbol ORDER BY Date) AS NextPrice,
        ROW_NUMBER() OVER (PARTITION BY StockSymbol ORDER BY Date) AS RowNum
    FROM StockPrice
),
AnomalousRows AS (
    SELECT 
        *,
        DATEDIFF(day, CONVERT(datetime, PreviousDate), CONVERT(datetime, Date)) AS DateDifference,
        ABS(100.0 * (Price - PreviousPrice) / PreviousPrice) AS PriceDifferencePercent
    FROM PriceAnalysis
    WHERE PreviousPrice IS NOT NULL
        AND (
            ABS(100.0 * (Price - PreviousPrice) / PreviousPrice) > 10
            OR DATEDIFF(day, CONVERT(datetime, PreviousDate), CONVERT(datetime, Date)) > 10
        )
),
ContextRows AS (
    SELECT DISTINCT
        pa.StockSymbol,
        pa.RowNum
    FROM PriceAnalysis pa
    INNER JOIN AnomalousRows ar ON pa.StockSymbol = ar.StockSymbol
    WHERE pa.RowNum IN (ar.RowNum - 1, ar.RowNum, ar.RowNum + 1)
)
SELECT 
    pa.StockPriceId,
    pa.StockSymbol,
    pa.Date,
    pa.Price,
    pa.Currency,
    pa.Source,
    pa.PreviousDate,
    pa.PreviousPrice,
    pa.NextDate,
    pa.NextPrice,
    CASE 
        WHEN ar.StockSymbol IS NOT NULL THEN 'ANOMALY'
        ELSE 'CONTEXT'
    END AS RowType,
    COALESCE(ar.DateDifference, 0) AS DateDifference,
    COALESCE(ar.PriceDifferencePercent, 0) AS PriceDifferencePercent,
    pa.RowNum
FROM PriceAnalysis pa
INNER JOIN ContextRows cr ON pa.StockSymbol = cr.StockSymbol AND pa.RowNum = cr.RowNum
LEFT JOIN AnomalousRows ar ON pa.StockSymbol = ar.StockSymbol AND pa.RowNum = ar.RowNum
ORDER BY pa.StockSymbol, pa.Date;