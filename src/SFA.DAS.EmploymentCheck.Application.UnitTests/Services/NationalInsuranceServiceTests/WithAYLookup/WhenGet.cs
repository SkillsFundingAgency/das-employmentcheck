using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.NationalInsuranceNumber;
using SFA.DAS.EmploymentCheck.Data.Models;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.NationalInsuranceNumberTests.WithAYLookup
{
    public class WhenGet
    {
        private NationalInsuranceNumberServiceWithAYLookup _sut;
        private Fixture _fixture;
        private Mock<INationalInsuranceNumberService> _nationalInsuranceNumberServiceMock;
        private Mock<INationalInsuranceNumberYearsService> _mockNationalInsuranceNumberYearsService;

        private Data.Models.EmploymentCheck _employmentCheck;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _nationalInsuranceNumberServiceMock = new Mock<INationalInsuranceNumberService>();
            _mockNationalInsuranceNumberYearsService = new Mock<INationalInsuranceNumberYearsService>();

            _employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();

            _mockNationalInsuranceNumberYearsService
                .Setup(m => m.Get())
                .ReturnsAsync(new List<string>() { "2223", "2122", "2021", "1920" });

            _nationalInsuranceNumberServiceMock
                .Setup(m => m.Get(It.Is<NationalInsuranceNumberRequest>( i => i.EmploymentCheck == _employmentCheck && i.AcademicYear == "2223")))
                .ReturnsAsync(_fixture
                    .Build<LearnerNiNumber>()
                    .With(l => l.HttpStatusCode, HttpStatusCode.NotFound)
                    .Create());

            _nationalInsuranceNumberServiceMock
                .Setup(m => m.Get(It.Is<NationalInsuranceNumberRequest>(i => i.EmploymentCheck == _employmentCheck && i.AcademicYear == "2122")))
                .ReturnsAsync(_fixture
                    .Build<LearnerNiNumber>()
                    .With(l => l.HttpStatusCode, HttpStatusCode.NoContent)
                    .Create());

            _nationalInsuranceNumberServiceMock
                .Setup(m => m.Get(new NationalInsuranceNumberRequest(_employmentCheck, "2021")))
                .ReturnsAsync(_fixture
                    .Build<LearnerNiNumber>()
                    .With(l => l.HttpStatusCode, HttpStatusCode.OK)
                    .Create());

            _nationalInsuranceNumberServiceMock
                .Setup(m => m.Get(new NationalInsuranceNumberRequest(_employmentCheck, "1920")))
                .ReturnsAsync(_fixture
                    .Build<LearnerNiNumber>()
                    .With(l => l.HttpStatusCode, HttpStatusCode.OK)
                    .Create());

            _sut = new NationalInsuranceNumberServiceWithAYLookup(
                _nationalInsuranceNumberServiceMock.Object,
                _mockNationalInsuranceNumberYearsService.Object
                );
        }

        [Test]
        public async Task Then_the_call_is_passed_to_the_service()
        {
            //Arrange
            var request = new NationalInsuranceNumberRequest(_employmentCheck, "2021");

            //Act
            await _sut.Get(request);

            // Assert
            _nationalInsuranceNumberServiceMock.Verify(m => m.Get(request), Times.Once);
        }

        [Test]
        public async Task Then_the_call_is_passed_to_the_service_for_each_academic_year_until_a_match_is_found()
        {
            //Arrange
            var request = new NationalInsuranceNumberRequest(_employmentCheck, "2021");

            //Act
            await _sut.Get(request);

            // Assert
            _nationalInsuranceNumberServiceMock.Verify(m => m.Get(It.IsAny<NationalInsuranceNumberRequest>()), Times.Exactly(3));
        }
    }
}