using DBScriptDeployment.Exceptions;
using DBScriptDeployment.Models;
using DBScriptDeployment.Queries;
using DBScriptDeployment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Net;
using System.Security.Claims;

namespace DBScriptDeployment.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ILogger<TaskController> _logger;
        private readonly ITFSQueries _tfsQueries;
        private readonly IDBService _dbService;
        private readonly ITFSApiClient _tfsService;

        private readonly char[] _delimiterChars = { ',', ':', '\t', '\n', ';' };

        public TaskController(ILogger<TaskController> logger,
                              ITFSQueries tfsQueries,
                              IDBService dbService,
                              ITFSApiClient tfsService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tfsQueries = tfsQueries ?? throw new ArgumentNullException(nameof(tfsQueries));
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _tfsService = tfsService ?? throw new ArgumentNullException(nameof(tfsService));
        }

        [Route("current")]
        [HttpGet]
        [ProducesResponseType(typeof(List<WorkItemInfo>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<WorkItemInfo>>> GetCurrentTasksAsync()
        {
            var claims = User.Identity as ClaimsIdentity;

            var id = int.Parse(claims.FindFirst(ClaimTypes.Name).Value);

            var workItems = await _tfsQueries.GetWorkItemsAsync(id);
            return Ok(workItems);

        }

        [Route("release")]
        [HttpGet]
        [ProducesResponseType(typeof(List<WorkItem>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<WorkItem>>> GetReleaseTasksAsync()
        {
            var claims = User.Identity as ClaimsIdentity;

            var id = int.Parse(claims.FindFirst(ClaimTypes.Name).Value);

            var workItems = await _tfsQueries.GetReleaseWorkItemsAsync(id);
            return Ok(workItems);
        }

        [Route("{taskId:int}")]
        [HttpGet]
        [ProducesResponseType(typeof(WorkItemInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> GetTaskAsync(int taskId)
        {
            try
            {
                var task = await _tfsQueries.GetTaskInfoByIdAsync(taskId);

                return Ok(task);
            }
            catch (TaskNotFoundException exception)
            {
                return NotFound(new ErrorView("Task not found", exception.Message));
            }
        }


        [Route("deploy/{taskId:int}")]
        [HttpPost]
        [ProducesResponseType(typeof(WorkItemInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> DeployTaskAsync(int taskId)
        {
            try
            {
                var task = await _tfsQueries.GetTaskInfoByIdAsync(taskId);

                var claims = User.Identity as ClaimsIdentity;

                var connection = claims.FindFirst(ClaimTypes.UserData).Value;

                if (!string.IsNullOrEmpty(task.ScriptBefore?.Trim()))
                    await _dbService.ExecuteScript(task.ScriptBefore, connection);

                if (!string.IsNullOrEmpty(task.ProcedureSet?.Trim()))
                {
                    task.ProcedureSet = task.ProcedureSet.Replace("<P>", "")
                                                         .Replace("</P>", "")
                                                         .Replace("<BR>", "\n");

                    foreach (var procedurePath in task.ProcedureSet.Split(_delimiterChars))
                    {

                        var procedure = await _tfsService.GetFileValue(procedurePath);
                        await _dbService.ExecuteScript(procedure, connection);
                    }
                }

                if (!string.IsNullOrEmpty(task.ScriptAfter?.Trim()))
                    await _dbService.ExecuteScript(task.ScriptAfter, connection);

                return Ok(task);
            }
            catch (TaskNotFoundException exception)
            {
                return NotFound(new ErrorView("Task not found", exception.Message));
            }
            catch (SqlException exception)
            {
                return BadRequest(new ErrorView("Execute script failed",exception.Message));
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, $"Failed get task info. TID: {taskId}!");
                return StatusCode(500, new ErrorView("Get task info failed",exception.Message));
            }
        }

    }
}
