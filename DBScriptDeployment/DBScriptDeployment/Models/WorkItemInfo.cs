namespace DBScriptDeployment.Models
{
    public class WorkItemInfo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Employee { get; set; }
        public string? ProcedureSet { get; set; }
        public string? ScriptAfter { get; set; }
        public string? ScriptBefore { get; set; }
        public string? State { get; set; }
        public string? IsNeedScripts { get; set; }
        public int Hours { get; set; }
        public int Position { get; set; }
    }
}
