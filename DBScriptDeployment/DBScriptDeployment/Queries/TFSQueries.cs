using Dapper;
using DBScriptDeployment.Exceptions;
using DBScriptDeployment.Models;
using DBScriptDeployment.Queries;
using System.Data.SqlClient;
using System.Threading.Tasks;
//using Microsoft.TeamFoundation.Client;

namespace DBScriptDeployment.Services
{
    public class TFSQueries : ITFSQueries
    {
        //rivate const string SETTINGS = "set nocount on\r\nset quoted_identifier, ansi_nulls, ansi_warnings, arithabort, concat_null_yields_null, ansi_padding on\r\nset numeric_roundabort offset transaction isolation level read uncommitted set xact_abort on go";
        private readonly string _connectionString = string.Empty;


        public TFSQueries(string connection)
        {

            _connectionString = !string.IsNullOrWhiteSpace(connection) ? connection : throw new ArgumentNullException(nameof(connection));
        }


        public async Task<List<WorkItem>> GetReleaseWorkItemsAsync(int employeeId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var items = await connection.QueryAsync<WorkItem>(@"exec mj.Work_Release
                                                                   @EmployeeIDSet = @employeeId"
            , new { employeeId });

            if (!items.Any())
                throw new TaskNotFoundException($"Tasks for release not found");

            return items.ToList();
        }


        public async Task<List<WorkItemInfo>> GetWorkItemsAsync(int employeeId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var items = await connection.QueryAsync<WorkItemInfo>(@"select 
                                                                     mwi.ID
                                                                    ,mwi.Title
                                                                    ,mwi.[State]
                                                                    ,mwi.IsNeedScripts
                                                                    ,mwi.Hours
                                                                    ,[Position] = row_number() over(order by (select null))
                                                                   from mjWorkItems       as mwi
                                                                   inner join mj.Employee as e on e.ID = mwi.EmployeeID
                                                                   where e.EmployeeID = @employeeId
                                                                     and mwi.[State] not in ('Выпущена'
                                                                                            ,'Закрыта'
                                                                                            ,'Закрыто'
                                                                                            ,'Закрыто без выполнения'
                                                                                            ,'Не делаем'
                                                                                            ,'Не требует описания'
                                                                                            ,'Описана'
                                                                                            ,'Требуется описать')"
            , new { employeeId });

            if (!items.Any())
                throw new TaskNotFoundException($"Working tasks not found");

            return items.ToList();
        }

        public async Task<WorkItemInfo> GetTaskInfoByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var taks = await connection.QueryAsync<WorkItemInfo>(@"select
                                                                x.ID           as [Id]
                                                               ,x.Title        as [Title]
                                                               ,x.Employee     as [Employee]
                                                               ,t1.Words       as ScriptBefore
                                                               ,t2.Words       as ScriptAfter
                                                               ,x.ReleaseNotes as ProcedureSet
                                                               ,x.State
                                                               ,x.IsNeedScripts 
                                                               ,x.Hours
                                                             from mjWorkItems  as x
                                                             outer apply (
                                                               select top 1
                                                                    t.Words
                                                                 from dbo.WorkItemLongTexts as t 
                                                                 where t.ID    = x.ID
                                                                   and t.FldID = 10146
                                                                 order by
                                                                    t.AddedDate desc
                                                             ) as t1
                                                             outer apply (
                                                               select top 1
                                                                    t.Words
                                                                 from dbo.WorkItemLongTexts as t 
                                                                 where t.ID    = x.ID
                                                                   and t.FldID = 10147
                                                                 order by
                                                                    t.AddedDate desc
                                                             ) as t2
                                                             where x.ID = @id", new { id });

            if (!taks.Any())
                throw new TaskNotFoundException($"Task with id {@id} not found");

            return taks.FirstOrDefault();
        }
    }
}
