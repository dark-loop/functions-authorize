using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DarkLoop.Azure.Functions.Authorize.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace DarkLoop.Azure.Functions.Authorize.Filters
{
    internal class FunctionsAuthorizeFilter : IFunctionsAuthorizeFilter
    {
        public IEnumerable<IAuthorizeData> AuthorizeData { get; }

        public IAuthenticationSchemeProvider SchemeProvider { get; }

        public IAuthorizationPolicyProvider PolicyProvider { get; }

        public AuthorizationPolicy Policy { get; }

        public FunctionsAuthorizeFilter(
            IAuthenticationSchemeProvider schemeProvider,
            IAuthorizationPolicyProvider policyProvider,
            IEnumerable<IAuthorizeData> authorizeData)
        {
            this.SchemeProvider = schemeProvider;
            this.PolicyProvider = policyProvider;
            this.AuthorizeData = authorizeData;

            this.IntegrateSchemes();
            this.Policy = this.ComputePolicyAsync().GetAwaiter().GetResult();
        }
        
        private void IntegrateSchemes()
        {
            var schemes = this.SchemeProvider.GetAllSchemesAsync().GetAwaiter().GetResult();
            var strSchemes = string.Join(',',
                from scheme in schemes
                where scheme.Name != Constants.WebJobsAuthScheme
                select scheme.Name);

            foreach (var data in this.AuthorizeData)
            {
                // only setting auth schemes if they have not been specified already
                if (string.IsNullOrWhiteSpace(data.AuthenticationSchemes))
                {
                    data.AuthenticationSchemes = strSchemes;
                }
            }
        }

        public async Task AuthorizeAsync(FunctionAuthorizationContext context)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));

            if (context.HttpContext.Items.ContainsKey(Constants.AuthInvokedKey))
            {
                return;
            }

            var httpContext = context.HttpContext;
            var evaluator = httpContext.RequestServices.GetRequiredService<IPolicyEvaluator>();
            var authenticateResult = await evaluator.AuthenticateAsync(this.Policy, httpContext);
            var authorizeResult = await evaluator.AuthorizeAsync(this.Policy, authenticateResult, httpContext, context);

            if (authorizeResult.Challenged)
            {
                context.Result = new ChallengeResult(this.Policy.AuthenticationSchemes.ToArray());
            }
            else if (authorizeResult.Forbidden)
            {
                context.Result = new ForbidResult(this.Policy.AuthenticationSchemes.ToArray());
            }
            else if (!authorizeResult.Succeeded)
            {
                context.Result = new UnauthorizedResult();
            }
        }

        private Task<AuthorizationPolicy> ComputePolicyAsync()
        {
            if (this.PolicyProvider == null)
            {
                throw new InvalidOperationException("Policy cannot be created.");
            }

            return AuthorizationPolicy.CombineAsync(this.PolicyProvider, this.AuthorizeData);
        }
    }
}
