using System.Threading.Tasks;
using DarkLoop.Azure.Functions.Authorize.Security;

namespace DarkLoop.Azure.Functions.Authorize.Filters
{
    interface IFunctionsAuthorizeFilter
    {
        Task AuthorizeAsync(FunctionAuthorizationContext context);
    }
}
