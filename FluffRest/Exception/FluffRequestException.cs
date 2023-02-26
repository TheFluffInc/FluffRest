namespace FluffRest.Exception
{
    public class FluffRequestException : System.Exception
    {
        public string Content { get; private set; }

        public FluffRequestException(string message, string content, System.Exception inner)
            : base (message, inner)
        {
            Content = content;
        }
    }
}
