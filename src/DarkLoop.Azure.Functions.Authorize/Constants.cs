using System;
using System.Collections.Generic;
using System.Text;

namespace DarkLoop.Azure.Functions.Authorize
{
    internal class Constants
    {
        internal const string AuthInvokedKey = "__WebJobAuthInvoked";
        internal const string WebJobsAuthScheme = "WebJobsAuthLevel";
        internal const string ArmTokenAuthScheme = "ArmToken";
    }
}
