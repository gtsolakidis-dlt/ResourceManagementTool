using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ResourceManagement.Domain.Entities;

namespace ResourceManagement.Domain.Interfaces
{
    public interface IProjectMonthlySnapshotRepository
    {
        /// <summary>
        /// Get all snapshots for a project and forecast version
        /// </summary>
        Task<List<ProjectMonthlySnapshot>> GetByProjectAsync(int projectId, int forecastVersionId);

        /// <summary>
        /// Get a specific snapshot by month
        /// </summary>
        Task<ProjectMonthlySnapshot?> GetByMonthAsync(int projectId, int forecastVersionId, DateTime month);

        /// <summary>
        /// Get the currently editable month (Status = Editable)
        /// </summary>
        Task<ProjectMonthlySnapshot?> GetEditableMonthAsync(int projectId, int forecastVersionId);

        /// <summary>
        /// Get all non-confirmed snapshots from a specific month onwards
        /// </summary>
        Task<List<ProjectMonthlySnapshot>> GetNonConfirmedFromMonthAsync(int projectId, int forecastVersionId, DateTime fromMonth);

        /// <summary>
        /// Create a new snapshot
        /// </summary>
        Task<int> CreateAsync(ProjectMonthlySnapshot snapshot);

        /// <summary>
        /// Update an existing snapshot
        /// </summary>
        Task UpdateAsync(ProjectMonthlySnapshot snapshot);

        /// <summary>
        /// Upsert a snapshot (create or update based on existence)
        /// </summary>
        Task UpsertAsync(ProjectMonthlySnapshot snapshot);

        /// <summary>
        /// Confirm the editable month and return success status
        /// </summary>
        Task<bool> ConfirmMonthAsync(int id, string confirmedBy);

        /// <summary>
        /// Promote the next pending month to editable status
        /// </summary>
        Task<bool> PromoteNextPendingToEditableAsync(int projectId, int forecastVersionId);

        /// <summary>
        /// Initialize snapshots for all months in a project's date range.
        /// First month = Editable, subsequent months = Pending.
        /// </summary>
        Task InitializeSnapshotsForProjectAsync(int projectId, int forecastVersionId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Delete all snapshots for a forecast version
        /// </summary>
        Task DeleteByForecastVersionAsync(int forecastVersionId);
    }
}
