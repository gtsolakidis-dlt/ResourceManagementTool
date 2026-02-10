-- Investigate Project State (v2 - Better Diagnostics)
-- Usage: Set @ProjectId to the affected project.

DECLARE @ProjectId INT = 1; -- CHANGE THIS TO YOUR AFFECTED PROJECT ID

PRINT '=== FORECAST VERSIONS ===';
PRINT 'Check which Version ID you are actually using in the UI.';
SELECT Id, VersionNumber, CreatedAt, IsActive = CASE WHEN Id = (SELECT MAX(Id) FROM ForecastVersion WHERE ProjectId = @ProjectId) THEN 1 ELSE 0 END
FROM ForecastVersion
WHERE ProjectId = @ProjectId
ORDER BY VersionNumber DESC;

-- SET THIS TO THE VERSION ID YOU FOUND ABOVE
DECLARE @TargetVersionId INT = 1; 

PRINT '';
PRINT '=== DATA DUMP FOR VERSION ' + CAST(@TargetVersionId AS NVARCHAR(10)) + ' ===';

PRINT '--- 1. ROSTER LEVELS (Check for whitespace/casing) ---';
SELECT DISTINCT 
    r.Id,
    r.FullNameEn, 
    OriginalLevel = r.Level,
    HexLevel = CONVERT(VARBINARY(MAX), r.Level), -- To see hidden chars
    Delimited = '|' + r.Level + '|'
FROM Roster r
JOIN ResourceAllocation a ON r.Id = a.RosterId
WHERE a.ForecastVersionId = @TargetVersionId;

PRINT '--- 2. PROJECT RATES (Check matches) ---';
SELECT 
    Level, 
    ActualDailyRate,
    Delimited = '|' + Level + '|'
FROM ProjectRate
WHERE ProjectId = @ProjectId;

PRINT '--- 3. SNAPSHOT DELTA CHECK ---';
-- If this returns rows, we have snapshots. If Delta is high negative, it is the Locked suppression.
SELECT 
    s.Month, 
    s.Status, 
    s.Wip as 'Stored',
    COALESCE(Theoretical.CalcWip, 0) as 'Theoretical',
    (s.Wip - COALESCE(Theoretical.CalcWip, 0)) as 'Delta'
FROM ProjectMonthlySnapshot s
OUTER APPLY (
    SELECT SUM(a.AllocatedDays * pr.ActualDailyRate) as CalcWip
    FROM ResourceAllocation a
    JOIN Roster r ON a.RosterId = r.Id
    JOIN ProjectRate pr ON r.Level = pr.Level AND pr.ProjectId = s.ProjectId
    WHERE a.ForecastVersionId = s.ForecastVersionId
    AND YEAR(a.Month) = YEAR(s.Month) AND MONTH(a.Month) = MONTH(s.Month)
) as Theoretical
WHERE s.ProjectId = @ProjectId AND s.ForecastVersionId = @TargetVersionId
ORDER BY s.Month;
