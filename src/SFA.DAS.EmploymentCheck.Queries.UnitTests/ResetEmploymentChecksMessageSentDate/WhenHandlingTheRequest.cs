using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Queries.ResetEmploymentChecksMessageSentDate;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Queries.UnitTests.ResetEmploymentChecksMessageSentDate
{
    public class WhenHandlingTheRequest
    {
        private ResetEmploymentChecksMessageSentDateQueryHandler _sut;
        private Mock<IEmploymentCheckService> _serviceMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _serviceMock = new Mock<IEmploymentCheckService>();
            _sut = new ResetEmploymentChecksMessageSentDateQueryHandler(_serviceMock.Object);
        }

        [Test]
        public void Then_The_CorrelationId_Is_Set()
        {
            // Arrange
            var employmentCheckMessageSentData = "CorrelationId=E269AE35-5A56-4DC8-A478-170E88280C31";

            // Act
            var request = new ResetEmploymentChecksMessageSentDateQueryRequest(employmentCheckMessageSentData);

            // Assert
            Assert.AreEqual(employmentCheckMessageSentData, request.EmploymentCheckMessageSentData);
        }

        [Test]
        public void Then_The_MessageFrom_To_Dates_Are_Set()
        {
            // Arrange
            var employmentCheckMessageSentData = "MessageSentFromDate=2022-03-23&MessageSentToDate=2022-03-25";

            // Act
            var request = new ResetEmploymentChecksMessageSentDateQueryRequest(employmentCheckMessageSentData);

            // Assert
            Assert.AreEqual(employmentCheckMessageSentData, request.EmploymentCheckMessageSentData);
        }
    }
}
