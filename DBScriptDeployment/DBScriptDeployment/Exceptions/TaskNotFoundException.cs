namespace DBScriptDeployment.Exceptions
{
    public class TaskNotFoundException : Exception
    {
        public TaskNotFoundException()
        { }

        public TaskNotFoundException(string message)
            : base(message)
        { }

        public TaskNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
