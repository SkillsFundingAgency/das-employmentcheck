using AutoFixture;
using FluentAssertions;
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

        [Test]
        public async Task Then_Guard_Throws_Exception_When_Input_Is_Null()
        {
            // Arrange
            var request = new ResetEmploymentChecksMessageSentDateQueryRequest(null);
            var employmentCheckMessageSentData = "CorrelationId=E269AE35-5A56-4DC8-A478-170E88280C31";

            // Act
            string error = string.Empty;
            try
            {
                await _sut.Handle(null);
            }
            catch(Exception ex)
            {
                error = ex.Message;
            }

            // Assert
            error.Should().Contain("Value cannot be null");
        }

        [Test]
        public async Task Then_ResetEmploymentChecksMessageSentDate_Is_Called_With_CorrelationId()
        {
            // Arrange
            var employmentCheckMessageSentData = "correlationid=E269AE35-5A56-4DC8-A478-170E88280C31";
            var request = new ResetEmploymentChecksMessageSentDateQueryRequest(employmentCheckMessageSentData);
            var args = request.EmploymentCheckMessageSentData;
            var correlationIdString = args.Split("=")[1];
            Guid CorrelationId = Guid.Parse(correlationIdString);

            _serviceMock
                .Setup(x => x.ResetEmploymentChecksMessageSentDate(CorrelationId))
                .ReturnsAsync(await Task.FromResult(1L));

            // Act
            var result = await _sut.Handle(request);

            // Assert
            result.UpdatedRowsCount.Should().Be(1);
        }

        [Test]
        public async Task Then_ResetEmploymentChecksMessageSentDate_Is_Called_With_MessageSentDate_Range()
        {
            // Arrange
            var employmentCheckMessageSentData = "MessageSentFromDate=2022-03-23&MessageSentToDate=2022-03-25";
            var request = new ResetEmploymentChecksMessageSentDateQueryRequest(employmentCheckMessageSentData);
            var args = request.EmploymentCheckMessageSentData;
            var temp = args.Split("=")[1];
            var messageSentFromDate = Convert.ToDateTime(temp.Split('&')[0]);
            var messageSentToDate = Convert.ToDateTime(args.Split("=")[2]);

            _serviceMock
                .Setup(x => x.ResetEmploymentChecksMessageSentDate(messageSentFromDate, messageSentToDate))
                .ReturnsAsync(await Task.FromResult(1L));

            // Act
            var result = await _sut.Handle(request);

            // Assert
            result.UpdatedRowsCount.Should().Be(1);
        }
    }
}
