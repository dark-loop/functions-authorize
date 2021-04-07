using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DarkLoop.Azure.Functions.Authorize.Filters;
using DarkLoop.Azure.Functions.Authorize.Security;
using DarkLoop.Azure.Functions.Authorize.Tests.Mocks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DarkLoop.Azure.Functions.Authorize.Tests
{
    [TestClass]
    public class FunctionsHttpAuthorizationHandlerShould
    {
        [TestMethod]
        public void ThrowExceptionWhenFiltersIndexIsNull()
        {
            var action = new Action(() => new FunctionsHttpAuthorizationHandler(null));
            action.Should().Throw<ArgumentNullException>("No null param is allowed");
        }

        [TestMethod]
        public void NotThrowExceptionWhenFiltersIndexIsNotNull()
        {
            var index = new Mock<IFunctionsAuthorizationFilterIndex>();
            var action = new Action(() => new FunctionsHttpAuthorizationHandler(index.Object));
            action.Should().NotThrow<ArgumentNullException>("index is expected");
        }

        [TestMethod]
        public void ThrowWhenOnAuthorizingFunctionContextParamIsNull()
        {
            var index = new Mock<IFunctionsAuthorizationFilterIndex>();
            var handler = new FunctionsHttpAuthorizationHandler(index.Object);
            var action = new Func<Task>(async () => await handler.OnAuthorizingFunctionInstance(null, null));
            action.Should().Throw<ArgumentNullException>("functionContext is expected not to be null");
        }
    }
}
