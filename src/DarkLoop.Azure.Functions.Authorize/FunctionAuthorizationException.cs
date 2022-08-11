using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace DarkLoop.Azure.Functions.Authorize
{
    public sealed class FunctionAuthorizationException : Exception
    {
        private readonly HttpStatusCode _statusCode;

        internal FunctionAuthorizationException(HttpStatusCode status)
            : base($"{ValidateStatus(status)} authorization error encountered. This is the only way to stop function execution. The correct status has been communicated to caller") 
        {
            _statusCode = status;
        }

        public FunctionAuthorizationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public HttpStatusCode AuthorizationStatus => _statusCode;

        private static int ValidateStatus(HttpStatusCode status)
        {
            if (status != HttpStatusCode.Unauthorized && status != HttpStatusCode.Forbidden)
            {
                throw new ArgumentException("Only unauthorized and forbidden status are accepted for this exception.");
            }

            return (int)status;
        }
    }
}
