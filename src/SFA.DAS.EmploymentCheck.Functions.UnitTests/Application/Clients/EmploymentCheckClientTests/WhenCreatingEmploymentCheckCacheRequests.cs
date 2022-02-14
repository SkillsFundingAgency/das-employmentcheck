//using AutoFixture;
//using FluentAssertions;
//using Moq;
//using NUnit.Framework;
//using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
//using SFA.DAS.EmploymentCheck.Functions.Application.Models;
//using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

//namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Clients.EmploymentCheckClientTests
//{
//    public class WhenCreatingEmploymentCheckCacheRequests
//    {
//        private const string NINO = "AB123456";
//        private const string PAYE = "Paye001";

//        private readonly Fixture _fixture;
//        private readonly Mock<IEmploymentCheckService> _employmentCheckServiceMock = new Mock<IEmploymentCheckService>();

//        private readonly IList<Models.EmploymentCheck> _employmentChecks;
//        private readonly IList<LearnerNiNumber> _ninos;
//        private readonly IList<EmployerPayeSchemes> _payes;
//        private readonly EmploymentCheckData _employmentCheckData;
//        private readonly IList<EmploymentCheckCacheRequest> _employmentCheckCacheRequests;

//        private readonly EmploymentCheckClient _sut;

//        public WhenCreatingEmploymentCheckCacheRequests()
//        {
//            _fixture = new Fixture();
//            _sut = new EmploymentCheckClient(_employmentCheckServiceMock.Object);

//            _employmentChecks = new List<Models.EmploymentCheck>
//            {
//                _fixture
//                .Build<Models.EmploymentCheck>()
//                .With(c => c.Uln, 1)
//                .With(c => c.AccountId, 1)
//                .Create()
//            };

//            _ninos = new List<LearnerNiNumber>
//            {
//                _fixture
//                .Build<LearnerNiNumber>()
//                .With(n => n.Uln, 1)
//                .With(n => n.NiNumber, NINO)
//                .Create()
//            };

//            _payes = new List<EmployerPayeSchemes>
//            {
//                _fixture
//                .Build<EmployerPayeSchemes>()
//                .With(p => p.EmployerAccountId, 1)
//                .With(p => p.PayeSchemes, new List<string> { PAYE })
//                .Create()
//            };

//            _employmentCheckData = _fixture
//                .Build<EmploymentCheckData>()
//                .With(d => d.ApprenticeNiNumbers, _ninos)
//                .With(d => d.EmployerPayeSchemes, _payes)
//                .With(d => d.EmploymentChecks, _employmentChecks)
//                .Create();

//            _employmentCheckCacheRequests = new List<EmploymentCheckCacheRequest>
//            {
//                _fixture
//                .Build<EmploymentCheckCacheRequest>()
//                .Create()
//            };
//        }

//        [Test]
//        public async Task Then_The_EmploymentCheckService_Is_Called()
//        {
//            // Arrange
//            _employmentCheckServiceMock
//                .Setup(x => x.CreateEmploymentCheckCacheRequests(_employmentCheckData))
//                .ReturnsAsync(_employmentCheckCacheRequests);

//            // Act
//            await _sut
//                .CreateEmploymentCheckCacheRequests(_employmentCheckData);

//            // Assert
//            _employmentCheckServiceMock
//                .Verify(x => x.CreateEmploymentCheckCacheRequests(_employmentCheckData), Times.AtLeastOnce);
//        }

//        [Test]
//        public async Task And_The_EmploymentCheckService_Returns_EmploymentChecks_Then_They_Are_Returned()
//        {
//            // Arrange
//            _employmentCheckServiceMock
//                .Setup(x => x.CreateEmploymentCheckCacheRequests(_employmentCheckData))
//                    .ReturnsAsync(_employmentCheckCacheRequests);

//            // Act
//            var result = await _sut
//                .CreateEmploymentCheckCacheRequests(_employmentCheckData);

//            //Assert
//            result
//                .Should()
//                .BeEquivalentTo(_employmentCheckCacheRequests);
//        }

//        [Test]
//        public async Task And_The_EmploymentCheckService_Returns_An_Empty_List_Then_An_Empty_List_Is_Returned()
//        {
//            // Arrange
//            _employmentCheckServiceMock
//                .Setup(x => x.CreateEmploymentCheckCacheRequests(_employmentCheckData))
//                    .ReturnsAsync(new List<EmploymentCheckCacheRequest>());

//            // Act
//            var result = await _sut
//                .CreateEmploymentCheckCacheRequests(_employmentCheckData);

//            //Assert
//            result
//                .Should()
//                .BeEquivalentTo(new List<Models.EmploymentCheck>());
//        }

//        [Test]
//        public async Task And_The_EmploymentCheckService_Returns_Null_Then_Null_Is_Returned()
//        {
//            // Arrange
//            _employmentCheckServiceMock
//                .Setup(x => x.CreateEmploymentCheckCacheRequests(_employmentCheckData))
//                    .ReturnsAsync(() => null);

//            // Act
//            var result = await _sut
//                .CreateEmploymentCheckCacheRequests(_employmentCheckData);

//            //Assert
//            result
//                .Should()
//                .BeNull();
//        }
//    }
//}