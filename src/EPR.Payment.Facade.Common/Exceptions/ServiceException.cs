namespace EPR.Payment.Facade.Common.Exceptions
{
    [Serializable]
    public class ServiceException : Exception
    {
        public ServiceException()
        {
        }

        public ServiceException(string message) : base(message)
        {
        }

        public ServiceException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}
