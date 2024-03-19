// <copyright file="FunctionAuthorizationException.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Net;
using System.Runtime.Serialization;

namespace DarkLoop.Azure.Functions.Authorization
{
    /// <summary>
    /// Represents an exception that is thrown when an authorization error is encountered to short circuit function execution.
    /// </summary>
    public sealed class FunctionAuthorizationException : Exception
    {
        private readonly HttpStatusCode _statusCode;

        internal FunctionAuthorizationException(HttpStatusCode status)
            : base($"{ValidateStatus(status)} authorization error encountered. This is the only way to stop function execution. The correct status has been communicated to caller") 
        {
            _statusCode = status;
        }

        /// <inheritdoc/>
        public FunctionAuthorizationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Gets the status code that was returned to caller.
        /// </summary>
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
