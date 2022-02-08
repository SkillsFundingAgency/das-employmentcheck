using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Mediators.Commands.CreateEmploymentCheckCacheRequestsTests
{
    public class WhenHandlingTheCommand
    {
        private Fixture _fixture;
        private readonly Mock<IEmploymentCheckClient> _employmentCheckClient;
        private readonly EmploymentCheckCacheRequest _employmentCheckCacheRequest;
        private readonly EmploymentCheckData _employmentCheckData;

        public WhenHandlingTheCommand()
        {
            _fixture = new Fixture();
            _employmentCheckClient = new Mock<IEmploymentCheckClient>();
            _employmentCheckData = _fixture.Create<EmploymentCheckData>();
        }

        [Test]
        public async Task And_EmploymentCacheRequests_Are_Returned_From_The_EmploymentCheckClient_Then_They_Returned()
        {
            // TODO:
        }
    }
}