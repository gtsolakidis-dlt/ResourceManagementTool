using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ResourceManagement.Application.Financials.Commands.UpsertBilling;
using ResourceManagement.Application.Financials.Commands.UpsertExpense;
using ResourceManagement.Application.Financials.Commands.UpsertOverride;
using ResourceManagement.Application.Financials.Commands.ConfirmMonth;
using ResourceManagement.Application.Financials.Commands.OverwriteSnapshot;
using ResourceManagement.Application.Financials.Commands.ClearOverride;
using ResourceManagement.Application.Financials.Queries.GetFinancials;
using ResourceManagement.Application.Financials.Queries.GetSnapshots;
using ResourceManagement.Contracts.Financials;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResourceManagement.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FinancialsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{projectId}/calculate/{forecastVersionId}")]
        public async Task<ActionResult<List<MonthlyFinancialDto>>> GetProjectFinancials(int projectId, int forecastVersionId)
        {
            return await _mediator.Send(new GetProjectFinancialsQuery(projectId, forecastVersionId));
        }

        [HttpPost("billing")]
        public async Task<ActionResult> UpsertBilling(UpsertBillingCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPost("expense")]
        public async Task<ActionResult> UpsertExpense(UpsertExpenseCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPost("override")]
        public async Task<ActionResult<int>> UpsertOverride(UpsertOverrideCommand command)
        {
            var id = await _mediator.Send(command);
            return Ok(id);
        }

        // ==================== Snapshot Endpoints ====================

        /// <summary>
        /// Get all monthly snapshots for a project and forecast version
        /// </summary>
        [HttpGet("{projectId}/snapshots/{forecastVersionId}")]
        public async Task<ActionResult<List<ProjectMonthlySnapshotDto>>> GetSnapshots(int projectId, int forecastVersionId)
        {
            return await _mediator.Send(new GetSnapshotsQuery(projectId, forecastVersionId));
        }

        /// <summary>
        /// Confirm the editable month, locking its values and promoting the next pending month
        /// </summary>
        [HttpPost("snapshots/confirm")]
        public async Task<ActionResult<bool>> ConfirmMonth(ConfirmMonthCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Overwrite calculated values for the editable month
        /// </summary>
        [HttpPost("snapshots/overwrite")]
        public async Task<ActionResult<bool>> OverwriteSnapshot(OverwriteSnapshotCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Clear manual overrides and restore calculated values for the editable month
        /// </summary>
        [HttpPost("snapshots/clear-override")]
        public async Task<ActionResult<bool>> ClearOverride(ClearOverrideCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
