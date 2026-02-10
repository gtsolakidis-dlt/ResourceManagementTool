-- Find Missing Allocations
-- Usage: Set @ProjectId to your project.

DECLARE @ProjectId INT = 1;

PRINT '=== ALL ALLOCATIONS FOR PROJECT ===';
PRINT 'Searching for where your inputs were saved...';

SELECT 
    v.Id as VersionId,
    v.VersionNumber,
    v.CreatedAt as VersionCreated,
    a.Month,
    COUNT(a.Id) as AllocationCount,
    SUM(a.AllocatedDays) as TotalDays
FROM ForecastVersion v
JOIN ResourceAllocation a ON v.Id = a.ForecastVersionId
WHERE v.ProjectId = @ProjectId
GROUP BY v.Id, v.VersionNumber, v.CreatedAt, a.Month
ORDER BY v.VersionNumber DESC, a.Month;

PRINT '--- INSTRUCTIONS ---';
PRINT '1. Look for the Months you edited (e.g. April 2026).';
PRINT '2. See which "VersionId" contains those days.';
PRINT '3. Check if that VersionId matches the one shown in Project Overview.';
