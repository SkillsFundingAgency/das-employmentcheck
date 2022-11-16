using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Api;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SFA.DAS.EmploymentCheck.Api.Mediators.Commands.RegisterCheckCommand;
using FluentAssertions;
using System.Net.Http;
using SFA.DAS.EmploymentCheck.Api.Application.Services;
using SFA.DAS.EmploymentCheck.Api.Repositories;

namespace SFA.DAS.EmploymentCheck.Api.UnitTests.Application.Services
{
    public class WhenAddingServices
    {
        private IServiceCollection _services = null;

        [SetUp]
        public void Setup()
        {
            _services = new ServiceCollection();
        }

        [Test]
        public void Then_AddHandlers()
        {
            // Arrange and Act
            _services.AddHandlers();

            // Assert
            var result = _services.Any(x => x.ServiceType == typeof(MediatR.IMediator));

            result.Should().BeTrue();

        }

        [Test]
        public void Then_AddServices()
        {
            // Arrange and Act
            _services.AddServices();

            // Assert
            var result1 = _services.Any(x => x.ServiceType == typeof(IHttpClientFactory));
            var result2 = _services.Any(x => x.ServiceType == typeof(IEmploymentCheckService));
            var result3 = _services.Any(x => x.ServiceType == typeof(IRegisterCheckCommandValidator));

            result1.Should().BeTrue();
            result2.Should().BeTrue();
            result3.Should().BeTrue();

        }

        [Test]
        public void Then_AddRepositories()
        {
            // Arrange and Act
            _services.AddRepositories();

            // Assert
            var result = _services.Any(x => x.ServiceType == typeof(IEmploymentCheckRepository));

            result.Should().BeTrue();

        }

        [Test]
        public void Then_AddNLogForApi()
        {
            // Arrange and Act
            _services.AddNLogForApi();

            // Assert
            _services.Count.Should().Be(16);
            
        }
    }
}
