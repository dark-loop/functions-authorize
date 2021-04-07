using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Moq;

namespace DarkLoop.Azure.Functions.Authorize.Tests.Mocks
{
    class FunctionExecutingContextMock : Mock<FunctionExecutingContext>
    {
        public FunctionExecutingContextMock(
            IReadOnlyDictionary<string, object> arguments, 
            IDictionary<string, object> properties, Guid functionInstanceId, string functionName, ILogger logger)
            : base(arguments, properties, functionInstanceId, functionName, logger)
        {
            
        }
    }
}
