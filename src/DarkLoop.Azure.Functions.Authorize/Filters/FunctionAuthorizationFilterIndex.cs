using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using DarkLoop.Azure.Functions.Authorize.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DarkLoop.Azure.Functions.Authorize.Filters
{
    class FunctionAuthorizationFilterIndex : IFunctionsAuthorizationFilterIndex
    {
        private readonly ConcurrentDictionary<string, IFunctionsAuthorizeFilter> _index =
            new ConcurrentDictionary<string, IFunctionsAuthorizeFilter>();

        private readonly IServiceProvider _serviceProvider;
        private readonly ObjectFactory _filterFactory;

        public FunctionAuthorizationFilterIndex(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _filterFactory = ActivatorUtilities.CreateFactory(typeof(FunctionsAuthorizeFilter), new[] { typeof(IEnumerable<IAuthorizeData>) });
        }

        public void AddAuthorizationFilter(MethodInfo functionMethod, FunctionNameAttribute nameAttribute, IEnumerable<IAuthorizeData> authorizeData)
        {
            if (functionMethod is null) throw new ArgumentNullException(nameof(functionMethod));
            if (authorizeData is null) throw new ArgumentNullException(nameof(authorizeData));

            var name = this.GetFunctionName(functionMethod, nameAttribute);
            var filter = _filterFactory.Invoke(_serviceProvider, new[] { authorizeData }) as FunctionsAuthorizeFilter;

            if (!this._index.TryAdd(name, filter!))
            {
                throw new InvalidOperationException($"An authorization filter for function {name} has already been processed. Make sure function names are unique within your Functions App.");
            }
        }

        public IFunctionsAuthorizeFilter? GetAuthorizationFilter(string functionName)
        {
            _index.TryGetValue(functionName, out var stored);

            return stored;
        }

        private string GetFunctionName(MethodInfo method, FunctionNameAttribute nameAttribute)
        {
            if (nameAttribute is null)
            {
                return $"{method.DeclaringType!.Name}.{method.Name}";
            }
            else
            {
                return nameAttribute.Name;
            }
        }


    }
}
