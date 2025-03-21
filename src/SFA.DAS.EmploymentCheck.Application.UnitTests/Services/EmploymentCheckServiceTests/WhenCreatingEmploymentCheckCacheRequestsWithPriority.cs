using System.Collections.Generic;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.EmploymentCheckServiceTests
{
    public class WhenCreatingEmploymentCheckCacheRequestsWithPriority
    {
        private IEmploymentCheckService _sut;
        private Fixture _fixture;
        private Mock<IEmploymentCheckRepository> _employmentCheckRepositoryMock;
        private Mock<IEmploymentCheckCacheRequestRepository> _employmentCheckCashRequestRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _employmentCheckRepositoryMock = new Mock<IEmploymentCheckRepository>();
            _employmentCheckCashRequestRepositoryMock = new Mock<IEmploymentCheckCacheRequestRepository>();

            _sut = new EmploymentCheckService(
                _employmentCheckRepositoryMock.Object,
                _employmentCheckCashRequestRepositoryMock.Object,
                Mock.Of<IUnitOfWork>(), Mock.Of<ILogger<EmploymentCheckService>>());
        }

        [Test]
        public async Task And_Only_One_PayeScheme_In_Current_List_Then_GetLearnerPayeCheckPriority_From_DB_Not_called()
        {
            // Arrange
            var nino = _fixture.Create<LearnerNiNumber>();
            var payeSchemes = _fixture.CreateMany<string>(1).ToList();
            var paye = _fixture.Build<EmployerPayeSchemes>().With(x => x.PayeSchemes, payeSchemes.ToArray()).Create();
            var check = _fixture.Build<Data.Models.EmploymentCheck>().With(c => c.Uln, nino.Uln).With(c => c.AccountId, paye.EmployerAccountId).Create();

            _employmentCheckCashRequestRepositoryMock.Setup(x => x.GetLearnerPayeCheckPriority(It.IsAny<string>()))
                .ReturnsAsync(payeSchemes.Select((ps, index) => new LearnerPayeCheckPriority(ps, index + 1)).ToList());

            var employmentCheckData = new EmploymentCheckData(check, nino, paye);

            // Act
            await _sut.CreateEmploymentCheckCacheRequests(employmentCheckData);

            // Assert
            _employmentCheckCashRequestRepositoryMock.Verify(x => x.GetLearnerPayeCheckPriority(It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public async Task And_Two_PayeScheme_In_Current_List_Then_GetLearnerPayeCheckPriority_From_DB_Called()
        {
            // Arrange
            var nino = _fixture.Create<LearnerNiNumber>();
            var payeSchemes = _fixture.CreateMany<string>(2).ToList();
            var paye = _fixture.Build<EmployerPayeSchemes>().With(x => x.PayeSchemes, payeSchemes.ToArray()).Create();
            var check = _fixture.Build<Data.Models.EmploymentCheck>().With(c => c.Uln, nino.Uln).With(c => c.AccountId, paye.EmployerAccountId).Create();

            _employmentCheckCashRequestRepositoryMock.Setup(x => x.GetLearnerPayeCheckPriority(It.IsAny<string>()))
                .ReturnsAsync(payeSchemes.Select((ps, index) => new LearnerPayeCheckPriority(ps, index + 1)).ToList());

            var employmentCheckData = new EmploymentCheckData(check, nino, paye);

            // Act
            await _sut.CreateEmploymentCheckCacheRequests(employmentCheckData);

            // Assert
            _employmentCheckCashRequestRepositoryMock.Verify(x => x.GetLearnerPayeCheckPriority(It.IsAny<string>()),
                Times.Once);
        }

        [Test]
        public async Task And_One_PayeScheme_In_Current_List_And_GetLearnerPayeCheckPriority_Returns_Two_Item_Then_Final_List_Has_Only_One_Matching_Item()
        {
            // Arrange
            var nino = _fixture.Create<LearnerNiNumber>();
            var currentPayeSchemes = new[] { "Paye1" };
            var historicalPayeSchemes = new[] { "Paye1", "Paye2" };
            var paye = _fixture.Build<EmployerPayeSchemes>().With(x => x.PayeSchemes, currentPayeSchemes.ToArray()).Create();
            var check = _fixture.Build<Data.Models.EmploymentCheck>().With(c => c.Uln, nino.Uln).With(c => c.AccountId, paye.EmployerAccountId).Create();

            _employmentCheckCashRequestRepositoryMock.Setup(x => x.GetLearnerPayeCheckPriority(It.IsAny<string>()))
                .ReturnsAsync(historicalPayeSchemes.Select((ps, index) => new LearnerPayeCheckPriority(ps, index + 1)).ToList());

            var employmentCheckData = new EmploymentCheckData(check, nino, paye);

            var expected = new List<EmploymentCheckCacheRequest>
            {
                new EmploymentCheckCacheRequest
                {
                    ApprenticeEmploymentCheckId = check.Id,
                    CorrelationId = check.CorrelationId,
                    Nino = nino.NiNumber,
                    PayeScheme = "Paye1",
                    PayeSchemePriority = 1,
                    MaxDate = check.MaxDate,
                    MinDate = check.MinDate,
                    RequestCompletionStatus = null
                }
            };

            // Act
            var result = await _sut.CreateEmploymentCheckCacheRequests(employmentCheckData);

            // Assert
            _employmentCheckCashRequestRepositoryMock.Verify(x => x.GetLearnerPayeCheckPriority(It.IsAny<string>()),
                Times.Never);

            result.Count.Should().Be(1);
            result.Should().BeEquivalentTo(expected);
            foreach (var employmentCheckCacheRequest in result)
            {
                _employmentCheckCashRequestRepositoryMock.Verify(r => r.Save(employmentCheckCacheRequest), Times.Once);
            }
        }

        [Test]
        public async Task And_Two_PayeScheme_In_Current_List_And_GetLearnerPayeCheckPriority_Returns_Two_Matching_Item_Then_Final_List_Has_Two_Items()
        {
            // Arrange
            var nino = _fixture.Create<LearnerNiNumber>();
            var currentPayeSchemes = new[] { "Paye1", "Paye2" };
            var historicalPayeSchemes = new[] { "Paye1", "Paye2" };
            var paye = _fixture.Build<EmployerPayeSchemes>().With(x => x.PayeSchemes, currentPayeSchemes.ToArray()).Create();
            var check = _fixture.Build<Data.Models.EmploymentCheck>().With(c => c.Uln, nino.Uln).With(c => c.AccountId, paye.EmployerAccountId).Create();

            _employmentCheckCashRequestRepositoryMock.Setup(x => x.GetLearnerPayeCheckPriority(It.IsAny<string>()))
                .ReturnsAsync(historicalPayeSchemes.Select((ps, index) => new LearnerPayeCheckPriority(ps, index + 1)).ToList());

            var employmentCheckData = new EmploymentCheckData(check, nino, paye);

            var expected = new List<EmploymentCheckCacheRequest>
            {
                new EmploymentCheckCacheRequest
                {
                    ApprenticeEmploymentCheckId = check.Id,
                    CorrelationId = check.CorrelationId,
                    Nino = nino.NiNumber,
                    PayeScheme = "Paye1",
                    PayeSchemePriority = 1,
                    MaxDate = check.MaxDate,
                    MinDate = check.MinDate,
                    RequestCompletionStatus = null
                },
                new EmploymentCheckCacheRequest
                {
                    ApprenticeEmploymentCheckId = check.Id,
                    CorrelationId = check.CorrelationId,
                    Nino = nino.NiNumber,
                    PayeScheme = "Paye2",
                    PayeSchemePriority = 2,
                    MaxDate = check.MaxDate,
                    MinDate = check.MinDate,
                    RequestCompletionStatus = null
                }
            };

            // Act
            var result = await _sut.CreateEmploymentCheckCacheRequests(employmentCheckData);

            // Assert

            result.Count.Should().Be(2);
            result.Should().BeEquivalentTo(expected);
            foreach (var employmentCheckCacheRequest in result)
            {
                _employmentCheckCashRequestRepositoryMock.Verify(r => r.Save(employmentCheckCacheRequest), Times.Once);
            }
        }

        [Test]
        public async Task And_Two_PayeScheme_In_Current_List_And_GetLearnerPayeCheckPriority_Returns_Only_First_Matching_Item_Then_Final_List_Has_Two_items_And_First_Item_Is_Prioritised_First()
        {
            // Arrange
            var nino = _fixture.Create<LearnerNiNumber>();
            var currentPayeSchemes = new[] { "Paye1", "Paye2" };
            var historicalPayeSchemes = new[] { "Paye1" };
            var paye = _fixture.Build<EmployerPayeSchemes>().With(x => x.PayeSchemes, currentPayeSchemes.ToArray()).Create();
            var check = _fixture.Build<Data.Models.EmploymentCheck>().With(c => c.Uln, nino.Uln).With(c => c.AccountId, paye.EmployerAccountId).Create();

            _employmentCheckCashRequestRepositoryMock.Setup(x => x.GetLearnerPayeCheckPriority(It.IsAny<string>()))
                .ReturnsAsync(historicalPayeSchemes.Select((ps, index) => new LearnerPayeCheckPriority(ps, index + 1)).ToList());

            var employmentCheckData = new EmploymentCheckData(check, nino, paye);

            var expected = new List<EmploymentCheckCacheRequest>
            {
                new EmploymentCheckCacheRequest
                {
                    ApprenticeEmploymentCheckId = check.Id,
                    CorrelationId = check.CorrelationId,
                    Nino = nino.NiNumber,
                    PayeScheme = "Paye1",
                    PayeSchemePriority = 1,
                    MaxDate = check.MaxDate,
                    MinDate = check.MinDate,
                    RequestCompletionStatus = null
                },
                new EmploymentCheckCacheRequest
                {
                    ApprenticeEmploymentCheckId = check.Id,
                    CorrelationId = check.CorrelationId,
                    Nino = nino.NiNumber,
                    PayeScheme = "Paye2",
                    PayeSchemePriority = 2,
                    MaxDate = check.MaxDate,
                    MinDate = check.MinDate,
                    RequestCompletionStatus = null
                }
            };

            // Act
            var result = await _sut.CreateEmploymentCheckCacheRequests(employmentCheckData);

            // Assert

            result.Count.Should().Be(2);
            result.Should().BeEquivalentTo(expected);
            foreach (var employmentCheckCacheRequest in result)
            {
                _employmentCheckCashRequestRepositoryMock.Verify(r => r.Save(employmentCheckCacheRequest), Times.Once);
            }
        }

        [Test]
        public async Task And_Two_PayeScheme_In_Current_List_And_GetLearnerPayeCheckPriority_Returns_Only_Second_Matching_Item_Then_Final_List_Has_Two_items_And_Second_Item_Is_Prioritised_First()
        {
            // Arrange
            var nino = _fixture.Create<LearnerNiNumber>();
            var currentPayeSchemes = new[] { "Paye1", "Paye2" };
            var historicalPayeSchemes = new[] { "Paye2" };
            var paye = _fixture.Build<EmployerPayeSchemes>().With(x => x.PayeSchemes, currentPayeSchemes.ToArray()).Create();
            var check = _fixture.Build<Data.Models.EmploymentCheck>().With(c => c.Uln, nino.Uln).With(c => c.AccountId, paye.EmployerAccountId).Create();

            _employmentCheckCashRequestRepositoryMock.Setup(x => x.GetLearnerPayeCheckPriority(It.IsAny<string>()))
                .ReturnsAsync(historicalPayeSchemes.Select((ps, index) => new LearnerPayeCheckPriority(ps, index + 1)).ToList());

            var employmentCheckData = new EmploymentCheckData(check, nino, paye);

            var expected = new List<EmploymentCheckCacheRequest>
            {
                new EmploymentCheckCacheRequest
                {
                    ApprenticeEmploymentCheckId = check.Id,
                    CorrelationId = check.CorrelationId,
                    Nino = nino.NiNumber,
                    PayeScheme = "Paye2",
                    PayeSchemePriority = 1,
                    MaxDate = check.MaxDate,
                    MinDate = check.MinDate,
                    RequestCompletionStatus = null
                },
                new EmploymentCheckCacheRequest
                {
                    ApprenticeEmploymentCheckId = check.Id,
                    CorrelationId = check.CorrelationId,
                    Nino = nino.NiNumber,
                    PayeScheme = "Paye1",
                    PayeSchemePriority = 2,
                    MaxDate = check.MaxDate,
                    MinDate = check.MinDate,
                    RequestCompletionStatus = null
                }
            };

            // Act
            var result = await _sut.CreateEmploymentCheckCacheRequests(employmentCheckData);

            // Assert

            result.Count.Should().Be(2);
            result.Should().BeEquivalentTo(expected);
            foreach (var employmentCheckCacheRequest in result)
            {
                _employmentCheckCashRequestRepositoryMock.Verify(r => r.Save(employmentCheckCacheRequest), Times.Once);
            }
        }

        [Test]
        public async Task And_Two_PayeScheme_In_Current_List_And_GetLearnerPayeCheckPriority_Returns_Two_Not_Matching_Item_Then_Final_List_Has_Two_Items()
        {
            // Arrange
            var nino = _fixture.Create<LearnerNiNumber>();
            var currentPayeSchemes = new[] { "Paye1", "Paye2" };
            var historicalPayeSchemes = new[] { "Paye3", "Paye4" };
            var paye = _fixture.Build<EmployerPayeSchemes>().With(x => x.PayeSchemes, currentPayeSchemes.ToArray()).Create();
            var check = _fixture.Build<Data.Models.EmploymentCheck>().With(c => c.Uln, nino.Uln).With(c => c.AccountId, paye.EmployerAccountId).Create();

            _employmentCheckCashRequestRepositoryMock.Setup(x => x.GetLearnerPayeCheckPriority(It.IsAny<string>()))
                .ReturnsAsync(historicalPayeSchemes.Select((ps, index) => new LearnerPayeCheckPriority(ps, index + 1)).ToList());

            var employmentCheckData = new EmploymentCheckData(check, nino, paye);

            var expected = new List<EmploymentCheckCacheRequest>
            {
                new EmploymentCheckCacheRequest
                {
                    ApprenticeEmploymentCheckId = check.Id,
                    CorrelationId = check.CorrelationId,
                    Nino = nino.NiNumber,
                    PayeScheme = "Paye1",
                    PayeSchemePriority = 1,
                    MaxDate = check.MaxDate,
                    MinDate = check.MinDate,
                    RequestCompletionStatus = null
                },
                new EmploymentCheckCacheRequest
                {
                    ApprenticeEmploymentCheckId = check.Id,
                    CorrelationId = check.CorrelationId,
                    Nino = nino.NiNumber,
                    PayeScheme = "Paye2",
                    PayeSchemePriority = 2,
                    MaxDate = check.MaxDate,
                    MinDate = check.MinDate,
                    RequestCompletionStatus = null
                }
            };

            // Act
            var result = await _sut.CreateEmploymentCheckCacheRequests(employmentCheckData);

            // Assert
            result.Count.Should().Be(2);
            result.Should().BeEquivalentTo(expected);
            foreach (var employmentCheckCacheRequest in result)
            {
                _employmentCheckCashRequestRepositoryMock.Verify(r => r.Save(employmentCheckCacheRequest), Times.Once);
            }
        }

        [Test]
        public async Task And_Two_PayeScheme_In_Current_List_And_GetLearnerPayeCheckPriority_Returns_First_Not_Matching_And_Second_Matching_Item_Then_Final_List_Has_Two_Items_And_Second_Item_Is_Prioritised_First()
        {
            // Arrange
            var nino = _fixture.Create<LearnerNiNumber>();
            var currentPayeSchemes = new[] { "Paye1", "Paye2" };
            var historicalPayeSchemes = new[] { "Paye3", "Paye2" };
            var paye = _fixture.Build<EmployerPayeSchemes>().With(x => x.PayeSchemes, currentPayeSchemes.ToArray()).Create();
            var check = _fixture.Build<Data.Models.EmploymentCheck>().With(c => c.Uln, nino.Uln).With(c => c.AccountId, paye.EmployerAccountId).Create();

            _employmentCheckCashRequestRepositoryMock.Setup(x => x.GetLearnerPayeCheckPriority(It.IsAny<string>()))
                .ReturnsAsync(historicalPayeSchemes.Select((ps, index) => new LearnerPayeCheckPriority(ps, index + 1)).ToList());

            var employmentCheckData = new EmploymentCheckData(check, nino, paye);

            var expected = new List<EmploymentCheckCacheRequest>
            {
                new EmploymentCheckCacheRequest
                {
                    ApprenticeEmploymentCheckId = check.Id,
                    CorrelationId = check.CorrelationId,
                    Nino = nino.NiNumber,
                    PayeScheme = "Paye2",
                    PayeSchemePriority = 1,
                    MaxDate = check.MaxDate,
                    MinDate = check.MinDate,
                    RequestCompletionStatus = null
                },
                new EmploymentCheckCacheRequest
                {
                    ApprenticeEmploymentCheckId = check.Id,
                    CorrelationId = check.CorrelationId,
                    Nino = nino.NiNumber,
                    PayeScheme = "Paye1",
                    PayeSchemePriority = 2,
                    MaxDate = check.MaxDate,
                    MinDate = check.MinDate,
                    RequestCompletionStatus = null
                }
            };

            // Act
            var result = await _sut.CreateEmploymentCheckCacheRequests(employmentCheckData);

            // Assert
            result.Count.Should().Be(2);
            result.Should().BeEquivalentTo(expected);
            foreach (var employmentCheckCacheRequest in result)
            {
                _employmentCheckCashRequestRepositoryMock.Verify(r => r.Save(employmentCheckCacheRequest), Times.Once);
            }
        }

        [Test]
        public async Task And_Two_PayeScheme_In_Current_List_And_GetLearnerPayeCheckPriority_Returns_Nothing_Then_Final_List_Has_Two_Items()
        {
            // Arrange
            var nino = _fixture.Create<LearnerNiNumber>();
            var currentPayeSchemes = new[] { "Paye1", "Paye2" };
            var historicalPayeSchemes = new string[] { };
            var paye = _fixture.Build<EmployerPayeSchemes>().With(x => x.PayeSchemes, currentPayeSchemes.ToArray()).Create();
            var check = _fixture.Build<Data.Models.EmploymentCheck>().With(c => c.Uln, nino.Uln).With(c => c.AccountId, paye.EmployerAccountId).Create();

            _employmentCheckCashRequestRepositoryMock.Setup(x => x.GetLearnerPayeCheckPriority(It.IsAny<string>()))
                .ReturnsAsync(historicalPayeSchemes.Select((ps, index) => new LearnerPayeCheckPriority(ps, index + 1)).ToList());

            var employmentCheckData = new EmploymentCheckData(check, nino, paye);

            var expected = new List<EmploymentCheckCacheRequest>
            {
                new EmploymentCheckCacheRequest
                {
                    ApprenticeEmploymentCheckId = check.Id,
                    CorrelationId = check.CorrelationId,
                    Nino = nino.NiNumber,
                    PayeScheme = "Paye1",
                    PayeSchemePriority = 1,
                    MaxDate = check.MaxDate,
                    MinDate = check.MinDate,
                    RequestCompletionStatus = null
                },
                new EmploymentCheckCacheRequest
                {
                    ApprenticeEmploymentCheckId = check.Id,
                    CorrelationId = check.CorrelationId,
                    Nino = nino.NiNumber,
                    PayeScheme = "Paye2",
                    PayeSchemePriority = 2,
                    MaxDate = check.MaxDate,
                    MinDate = check.MinDate,
                    RequestCompletionStatus = null
                }
            };

            // Act
            var result = await _sut.CreateEmploymentCheckCacheRequests(employmentCheckData);

            // Assert
            result.Count.Should().Be(2);
            result.Should().BeEquivalentTo(expected);
            foreach (var employmentCheckCacheRequest in result)
            {
                _employmentCheckCashRequestRepositoryMock.Verify(r => r.Save(employmentCheckCacheRequest), Times.Once);
            }
        }
    }
}