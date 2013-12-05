using System;

namespace Example.UiCoreService.Model.Exceptions
{
    public class BaseServiceException : Exception
    {
        public BaseServiceException()
        {
        }

        public BaseServiceException(string message) : base(message)
        {
        }
    }
}