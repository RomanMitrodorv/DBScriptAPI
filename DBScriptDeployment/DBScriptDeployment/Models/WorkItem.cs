namespace DBScriptDeployment.Models
{
    public class WorkItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string IsNeedScripts { get; set; }
        public int Hours { get; set; }
        public int Position { get; set; }

    }
}
