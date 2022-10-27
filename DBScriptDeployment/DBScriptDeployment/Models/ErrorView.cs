namespace DBScriptDeployment.Models
{
    public class ErrorView
    {
        public string Status { get; set; }
        public string Text { get; set; }

        public ErrorView(string status, string text)
        {
            Text = text;
            Status = status;
        }
    }
}
