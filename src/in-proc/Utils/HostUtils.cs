// <copyright file="HostUtils.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Reflection;
using DarkLoop.Azure.Functions.Authorization.Properties;

namespace DarkLoop.Azure.Functions.Authorization.Utils
{
    internal class HostUtils
    {
        protected static readonly Assembly WebJobsHostAssembly;

        // These are a series of publicly available types that are used to interact with the Azure Functions runtime.
        // We use reflection to access these types to not create a hard dependency on the Azure Functions WebHost.
        internal static readonly Type? FunctionExecutionFeatureType;

        static HostUtils()
        {
            WebJobsHostAssembly = Assembly.Load(Strings.WJH_Assembly);

            if (WebJobsHostAssembly is null)
            {
                throw new InvalidOperationException($"{Assembly.GetExecutingAssembly()} cannot be used outside of an Azure Functions environment.");
            }

            var entryAssembly = Assembly.GetEntryAssembly();
            var entryFullName = entryAssembly!.FullName;
            var entryName = entryFullName!.Substring(0, entryFullName.IndexOf(','));
            IsLocalDevelopment = !entryName.Equals(Strings.WJH_Assembly, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsLocalDevelopment { get; }
    }
}
