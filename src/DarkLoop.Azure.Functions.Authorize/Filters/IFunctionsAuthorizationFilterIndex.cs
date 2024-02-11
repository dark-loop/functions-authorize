using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.WebJobs;

namespace DarkLoop.Azure.Functions.Authorize.Filters
{
    interface IFunctionsAuthorizationFilterIndex
    {
        IFunctionsAuthorizeFilter? GetAuthorizationFilter(string functionName);

        void AddAuthorizationFilter(MethodInfo functionMethod, FunctionNameAttribute nameAttribute, IEnumerable<IAuthorizeData> authorizeData);
    }
}
