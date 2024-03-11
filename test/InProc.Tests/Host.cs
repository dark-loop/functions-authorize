// <copyright file="HostUtils.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InProc.Tests
{
    internal static class Host
    {
        private const string FuncLibName = "func";
        private const string HostLibName = "Microsoft.Azure.WebJobs.Script.WebHost";
        private const string ScriptLibName = "Microsoft.Azure.WebJobs.Script";
        private const string HostLibPath = @"C:\Program Files\Microsoft\Azure Functions Core Tools\";

        public static void EnsureSdkHost()
        {
            
            var currentDomain = AppDomain.CurrentDomain;
            var sdkHostAssembly = currentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Microsoft.Azure.WebJobs.Script.WebHost");

            if (sdkHostAssembly == null)
            {
                File.Copy($"{HostLibPath}\\{FuncLibName}.exe", $"{FuncLibName}.exe", overwrite: true);
                File.Copy($"{HostLibPath}\\{HostLibName}.dll", $"{HostLibName}.dll", overwrite: true);
                File.Copy($"{HostLibPath}\\{ScriptLibName}.dll", $"{ScriptLibName}.dll", overwrite: true);
            }
        }
    }
}
