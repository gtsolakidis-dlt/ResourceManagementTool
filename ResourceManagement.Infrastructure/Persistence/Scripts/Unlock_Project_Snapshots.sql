-- Unlock Project Snapshots
-- Usage: Set @ProjectId to the ID of the project you want to unlock.
-- Ensure you have a backup before running!

DECLARE @ProjectId INT = 1; -- CHANGE THIS TO YOUR AFFECTED PROJECT ID
DECLARE @ForecastVersionId INT = 1; -- CHANGE THIS TO YOUR FORECAST VERSION ID (Usually 1)

PRINT 'Unlocking snapshots for Project ' + CAST(@ProjectId AS NVARCHAR(10));

-- Update Status from 2 (Confirmed) to 1 (Editable)
UPDATE ProjectMonthlySnapshot
SET 
    Status = 1, -- Editable
    ConfirmedAt = NULL,
    ConfirmedBy = NULL,
    UpdatedAt = GETUTCDATE()
WHERE 
    ProjectId = @ProjectId 
    AND ForecastVersionId = @ForecastVersionId
    AND Status = 2; -- Only update Confirmed ones

PRINT 'Snapshots unlocked. Please verify in the application.';
