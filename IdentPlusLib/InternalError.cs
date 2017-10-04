namespace IdentPlusLib
{
    public sealed class InternalError : Reply
    {
        public InternalError(string errorInfo)
        {
            ErrorInfo = errorInfo;
        }

        public readonly string ErrorInfo;
    }
}