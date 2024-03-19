// <copyright file="FunctionsAuthorizationOptionsExtensions.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Reflection;
using DarkLoop.Azure.Functions.Authorization.Internal;
using Microsoft.AspNetCore.Authorization;

namespace DarkLoop.Azure.Functions.Authorization
{
    // This functionality can be exposed later through a builder to define the authorization rules for the functions
    // without the need to use the attributes making it more performant and flexible.
    internal static class FunctionsAuthorizationOptionsExtensions
    {
        /// <summary>
        /// Registers all the functions in the specified <paramref name="declaringType"/>.
        /// </summary>
        /// <param name="options">The current options.</param>
        /// <param name="declaringType">The type containing the functions.</param>
        /// <param name="existing">A value indicating whether the function declaring type is already registered.</param>
        /// <returns>Return a <see cref="FunctionAuthorizationMetadata"/> to keep configuring.</returns>
        internal static FunctionAuthorizationMetadata SetTypeAuthorizationInfo(
            this FunctionsAuthorizationOptions options, Type declaringType, out bool existing)
        {
            Check.NotNull(declaringType, nameof(declaringType));

            return options.AuthorizationMetadata.Add(declaringType, out existing);
        }

        /// <summary>
        /// Registers the function with the specified name in <paramref name="functionName"/> 
        /// in the type specified in <paramref name="declaringType"/>.
        /// </summary>
        /// <param name="options">The current options.</param>
        /// <param name="functionName">The name of the function.</param>
        /// <param name="declaringType">The type declaring the function.</param>
        /// <returns>Return a <see cref="FunctionAuthorizationMetadata"/> to keep configuring.</returns>
        internal static FunctionAuthorizationMetadata SetFunctionAuthorizationInfo(
            this FunctionsAuthorizationOptions options, string functionName, Type declaringType)
        {
            Check.NotNullOrWhiteSpace(functionName, nameof(functionName));
            Check.NotNull(declaringType, nameof(declaringType));

            return options.AuthorizationMetadata.Add(functionName, declaringType);
        }

        /// <inheritdoc cref="FunctionAuthorizationTypeMap.IsFunctionRegistered"/>
        internal static bool IsFunctionRegistered(this FunctionsAuthorizationOptions options, string functionName)
        {
            return options.AuthorizationMetadata.IsFunctionRegistered(functionName);
        }

        /// <summary>
        /// Registers the authorization metadata for the function extracted from attribute.
        /// </summary>
        /// <typeparam name="TAuthAttribute">The type of authorization attribute to lookup.</typeparam>
        /// <param name="options">The current options.</param>
        /// <param name="functionName">The name of the function.</param>
        /// <param name="declaringType">The type declaring the function.</param>
        /// <param name="functionMethod">The entry point method for the function.</param>
        internal static void RegisterFunctionAuthorizationAttributesMetadata<TAuthAttribute>(
            this FunctionsAuthorizationOptions options, string functionName, Type declaringType, MethodInfo functionMethod)
            where TAuthAttribute : Attribute, IAuthorizeData
        {
            var typeMetadata = options
                .SetTypeAuthorizationInfo(declaringType, out var typeAlreadyRegistered);

            if (!typeAlreadyRegistered)
            {
                var classAuthAttributes = declaringType.GetCustomAttributes<TAuthAttribute>().ToArray();
                var classAllowAnonymous = declaringType.GetCustomAttribute<AllowAnonymousAttribute>();

                if (classAuthAttributes.Length > 0)
                {
                    typeMetadata.AddAuthorizeData(classAuthAttributes);
                }

                if (classAllowAnonymous is not null)
                {
                    typeMetadata.AllowAnonymousAccess();
                }
            }

            var methodAuthAttributes = functionMethod.GetCustomAttributes<TAuthAttribute>().ToArray();
            var methodAllowAnonymous = functionMethod.GetCustomAttribute<AllowAnonymousAttribute>();
            var methodMetadata = options
                .SetFunctionAuthorizationInfo(functionName, declaringType);

            if (methodAuthAttributes.Length > 0)
            {
                methodMetadata.AddAuthorizeData(methodAuthAttributes);
            }

            if (methodAllowAnonymous is not null)
            {
                methodMetadata.AllowAnonymousAccess();
            }
        }
    }
}
