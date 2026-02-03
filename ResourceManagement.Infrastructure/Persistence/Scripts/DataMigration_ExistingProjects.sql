-- Data Migration: Populate ProjectRate and ProjectMonthlySnapshot for existing projects
-- Run this ONCE after InitialSchema.sql has been applied

-- ============================================================================
-- STEP 1: Populate ProjectRate for existing projects
-- ============================================================================
-- This creates rate entries for all existing projects based on GlobalRate values
-- and the project's discount percentage.

INSERT INTO ProjectRate (ProjectId, Level, NominalRate, ActualDailyRate)
SELECT
    p.Id,
    gr.Level,
    gr.NominalRate,
    gr.NominalRate * (1 - ISNULL(p.Discount, 0) / 100.0)
FROM Project p
CROSS JOIN GlobalRate gr
WHERE p.IsDeleted = 0
  AND NOT EXISTS (
    SELECT 1 FROM ProjectRate pr
    WHERE pr.ProjectId = p.Id AND pr.Level = gr.Level
);

PRINT 'ProjectRate records created: ' + CAST(@@ROWCOUNT AS VARCHAR(10));

-- ============================================================================
-- STEP 2: Initialize ProjectMonthlySnapshot for existing forecast versions
-- ============================================================================
-- This creates snapshot records for each month in the project's date range.
-- The FIRST month is set to 'Editable' (Status = 1), all others are 'Pending' (Status = 0).

-- Use a numbers table approach for generating months
WITH Numbers AS (
    SELECT TOP 120 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS Number
    FROM master.dbo.spt_values
    WHERE type = 'P'
),
MonthsToCreate AS (
    SELECT
        fv.Id AS ForecastVersionId,
        p.Id AS ProjectId,
        DATEADD(MONTH, n.Number, DATEFROMPARTS(YEAR(p.StartDate), MONTH(p.StartDate), 1)) AS Month,
        ROW_NUMBER() OVER (PARTITION BY fv.Id ORDER BY n.Number) AS MonthOrder
    FROM Project p
    INNER JOIN ForecastVersion fv ON fv.ProjectId = p.Id
    CROSS JOIN Numbers n
    WHERE p.IsDeleted = 0
      AND DATEADD(MONTH, n.Number, DATEFROMPARTS(YEAR(p.StartDate), MONTH(p.StartDate), 1)) <= p.EndDate
)
INSERT INTO ProjectMonthlySnapshot (ProjectId, ForecastVersionId, Month, Status)
SELECT
    ProjectId,
    ForecastVersionId,
    Month,
    CASE WHEN MonthOrder = 1 THEN 1 ELSE 0 END -- 1 = Editable, 0 = Pending
FROM MonthsToCreate m
WHERE NOT EXISTS (
    SELECT 1 FROM ProjectMonthlySnapshot pms
    WHERE pms.ProjectId = m.ProjectId
      AND pms.ForecastVersionId = m.ForecastVersionId
      AND pms.Month = m.Month
);

PRINT 'ProjectMonthlySnapshot records created: ' + CAST(@@ROWCOUNT AS VARCHAR(10));

-- ============================================================================
-- VERIFICATION QUERIES (Optional - run to check results)
-- ============================================================================
-- SELECT COUNT(*) AS TotalProjectRates FROM ProjectRate;
-- SELECT COUNT(*) AS TotalSnapshots FROM ProjectMonthlySnapshot;
--
-- SELECT p.Name, pr.Level, pr.NominalRate, pr.ActualDailyRate
-- FROM ProjectRate pr
-- INNER JOIN Project p ON p.Id = pr.ProjectId
-- ORDER BY p.Name, pr.Level;
--
-- SELECT p.Name, pms.Month,
--        CASE pms.Status WHEN 0 THEN 'Pending' WHEN 1 THEN 'Editable' WHEN 2 THEN 'Confirmed' END AS Status
-- FROM ProjectMonthlySnapshot pms
-- INNER JOIN Project p ON p.Id = pms.ProjectId
-- ORDER BY p.Name, pms.Month;
