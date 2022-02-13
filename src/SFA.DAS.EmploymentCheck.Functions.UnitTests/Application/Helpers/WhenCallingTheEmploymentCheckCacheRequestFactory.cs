using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Clients.EmploymentCheck.EmploymentCheckTests
{
    public class WhenCallingTheEmploymentCheckCacheRequestFactory
    {
        private const string NINO = "AB123456";
        private const string PAYE = "Paye001";

        private readonly Fixture _fixture;
        private readonly ProcessingCompletionStatus _status;
        private readonly Models.EmploymentCheck _employmentCheck;

        public WhenCallingTheEmploymentCheckCacheRequestFactory()
        {
            _fixture = new Fixture();
            _status = ProcessingCompletionStatus.Completed;
            _employmentCheck = _fixture.Build<Models.EmploymentCheck>()
                .With(e => e.Id, 1)
                .With(e => e.CorrelationId, Guid.NewGuid())
                .With(e => e.CheckType, "CheckType")
                .With(e => e.Uln, 1)
                .With(e => e.ApprenticeshipId, 1)
                .With(e => e.AccountId, 1)
                .With(e => e.MinDate, new DateTime(2022, 2, 11))
                .With(e => e.MaxDate, new DateTime(2022, 2, 12))
                .With(e => e.Employed, (bool?)null)
                .With(e => e.RequestCompletionStatus, (short)-1)
                .With(e => e.LastUpdatedOn, (DateTime?)null)
                .With(e => e.CreatedOn, new DateTime(2022, 2, 11))
                .Create();
        }

        [Test]
        public async Task Then_The_Factory_Is_Called()
        {
            //Arrange
            var sut = new EmploymentCheckCacheRequestFactory();

            //Act
            var request = await sut.CreateEmploymentCheckCacheRequest(_employmentCheck, NINO, PAYE);

            //Assert
            request.Id.Should().Be(-1);
            request.ApprenticeEmploymentCheckId.Should().Be(_employmentCheck.Id);
            request.Nino.Should().Be(NINO);
            request.PayeScheme.Should().Be(PAYE);
            request.MinDate.Should().Be(_employmentCheck.MinDate);
            request.MaxDate.Should().Be(_employmentCheck.MaxDate);
            request.Employed.Should().Be(_employmentCheck.Employed);
            request.RequestCompletionStatus.Should().Be(_employmentCheck.RequestCompletionStatus);
        }

        //[Test]
        //public async Task Then_With_Nullable_Parameters_The_Cache_Request_Is_Created()
        //{
        //    //Arrange
        //    var id = 1L;
        //    var apprenticeEmploymentCheckId = 2L;
        //    Guid? guid = null;
        //    var nino = "A123456";
        //    var paye = "Paye";
        //    var minDate = new DateTime(2022, 2, 10);
        //    var maxDate = new DateTime(2022, 2, 11);
        //    bool? employed = true;
        //    var requestCompletionStatus = (short)-1;

        //    var sut = new EmploymentCheckCacheRequestFactory();

        //    //Act
        //    var request = await sut.CreateEmploymentCheckCacheRequest(
        //        id,
        //        apprenticeEmploymentCheckId,
        //        guid,
        //        nino,
        //        paye,
        //        minDate,
        //        maxDate,
        //        employed,
        //        requestCompletionStatus);

        //    //Assert
        //    request.Id.Should().Be(id);
        //    request.ApprenticeEmploymentCheckId.Should().Be(apprenticeEmploymentCheckId);
        //    request.Nino.Should().Be(nino);
        //    request.PayeScheme.Should().Be(paye);
        //    request.MinDate.Should().Be(minDate);
        //    request.MaxDate.Should().Be(maxDate);
        //    request.Employed.Should().Be(employed);
        //    request.RequestCompletionStatus.Should().Be(requestCompletionStatus);
        //}

        //[Test]
        //public async Task Then_With_Generated_Parameters_The_Cache_Request_Is_Created()
        //{
        //    //Arrange
        //    var employmentCheck = _fixture.Create<Models.EmploymentCheck>();
        //    var id = 1L;
        //    var apprenticeEmploymentCheckId = employmentCheck.Id;
        //    Guid? guid = employmentCheck.CorrelationId;
        //    var nino = "A123456";
        //    var paye = "Paye";
        //    var minDate = employmentCheck.MinDate;
        //    var maxDate = employmentCheck.MaxDate;
        //    bool? employed = null;
        //    var requestCompletionStatus = (short)-1;

        //    var sut = new EmploymentCheckCacheRequestFactory();

        //    //Act
        //    var request = await sut.CreateEmploymentCheckCacheRequest(
        //       id,
        //        apprenticeEmploymentCheckId,
        //        guid,
        //        nino,
        //        paye,
        //        minDate,
        //        maxDate,
        //        employed,
        //        requestCompletionStatus);

        //    //Assert
        //    request.Id.Should().Be((long)1);
        //    request.ApprenticeEmploymentCheckId.Should().Be(apprenticeEmploymentCheckId);
        //    request.Nino.Should().Be(nino);
        //    request.PayeScheme.Should().Be(paye);
        //    request.MinDate.Should().Be(minDate);
        //    request.MaxDate.Should().Be(maxDate);
        //    request.Employed.Should().Be(employed);
        //    request.RequestCompletionStatus.Should().Be(requestCompletionStatus);
        //}
    }
}