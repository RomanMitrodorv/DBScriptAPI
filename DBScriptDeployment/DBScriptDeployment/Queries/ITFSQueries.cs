using DBScriptDeployment.Models;

namespace DBScriptDeployment.Queries
{
    public interface ITFSQueries
    {
        Task<WorkItemInfo> GetTaskInfoByIdAsync(int id);
        Task<List<WorkItem>> GetReleaseWorkItemsAsync(int employeeId);
        Task<List<WorkItemInfo>> GetWorkItemsAsync(int employeeId);
    }
}
