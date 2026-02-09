using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Api.Application.Controllers;
using SFA.DAS.EmploymentCheck.Api.Repositories;
using SFA.DAS.EmploymentCheck.Api.Responses;
using ApplicationModels = SFA.DAS.EmploymentCheck.Api.Application.Models;

namespace SFA.DAS.EmploymentCheck.Api.UnitTests.Application.Controllers.EmploymentChecksControllerTests;

public class WhenGettingEmploymentChecksByApprenticeshipIds
{
    private Fixture _fixture;
    private Mock<IEmploymentCheckRepository> _repository;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _repository = new Mock<IEmploymentCheckRepository>();
    }

    [Test]
    public async Task Then_Repository_Is_Called_With_ApprenticeshipIds()
    {
        // Arrange
        var apprenticeshipIds = new List<long> { 1, 2, 3 };
        _repository
            .Setup(x => x.GetLatestChecksByApprenticeshipIds(apprenticeshipIds))
            .ReturnsAsync(new List<ApplicationModels.EmploymentCheck>());

        var sut = new EmploymentChecksController(_repository.Object);

        // Act
        await sut.Get(apprenticeshipIds);

        // Assert
        _repository.VerifyAll();
        _repository.VerifyNoOtherCalls();
    }

    [Test]
    public async Task And_Repository_Returns_Checks_Then_200_And_Mapped_EvsCheck_List_Returned()
    {
        // Arrange
        var apprenticeshipIds = new List<long> { 100 };
        var createdOn = _fixture.Create<DateTime>();
        var lastUpdatedOn = createdOn.AddDays(1);
        var checks = new List<ApplicationModels.EmploymentCheck>
        {
            new()
            {
                Id = 1,
                ApprenticeshipId = 100,
                AccountId = 200,
                Uln = 1234567890,
                Employed = true,
                RequestCompletionStatus = 2,
                ErrorType = null,
                CreatedOn = createdOn,
                LastUpdatedOn = lastUpdatedOn
            }
        };
        _repository
            .Setup(x => x.GetLatestChecksByApprenticeshipIds(It.IsAny<IReadOnlyList<long>>()))
            .ReturnsAsync(checks);

        var sut = new EmploymentChecksController(_repository.Object);

        // Act
        var result = await sut.Get(apprenticeshipIds) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        var response = result.Value.Should().BeAssignableTo<GetEmploymentChecksResponse>().Subject;
        response.Checks.Should().HaveCount(1);
        response.Checks[0].EmployerId.Should().Be(200);
        response.Checks[0].ApprenticeshipId.Should().Be(100);
        response.Checks[0].Uln.Should().Be("1234567890");
        response.Checks[0].RequestDate.Should().Be(createdOn);
        response.Checks[0].DateOfCheck.Should().Be(lastUpdatedOn);
        response.Checks[0].Result.Employed.Should().Be(true);
        response.Checks[0].Result.CompletionStatus.Should().Be(2);
        response.Checks[0].Result.ErrorCode.Should().BeNull();
    }

    [Test]
    public async Task And_Repository_Returns_Empty_Then_200_And_Empty_List_Returned()
    {
        // Arrange
        var apprenticeshipIds = new List<long> { 1, 2 };
        _repository
            .Setup(x => x.GetLatestChecksByApprenticeshipIds(apprenticeshipIds))
            .ReturnsAsync(new List<ApplicationModels.EmploymentCheck>());

        var sut = new EmploymentChecksController(_repository.Object);

        // Act
        var result = await sut.Get(apprenticeshipIds) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        var response = result.Value.Should().BeAssignableTo<GetEmploymentChecksResponse>().Subject;
        response.Checks.Should().BeEmpty();
    }

    [Test]
    public async Task And_ApprenticeshipIds_Is_Null_Then_400_BadRequest()
    {
        // Arrange
        var sut = new EmploymentChecksController(_repository.Object);

        // Act
        var result = await sut.Get(null) as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
        result.Value.Should().Be("apprenticeshipIds is required and must not be empty.");
    }

    [Test]
    public async Task And_ApprenticeshipIds_Is_Empty_Then_400_BadRequest()
    {
        // Arrange
        var sut = new EmploymentChecksController(_repository.Object);

        // Act
        var result = await sut.Get([]) as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
        result.Value.Should().Be("apprenticeshipIds is required and must not be empty.");
    }

    [Test]
    public async Task And_ApprenticeshipIds_Exceeds_1000_Then_400_BadRequest()
    {
        // Arrange
        var tooMany = new List<long>();
        for (var i = 0; i < 1001; i++)
            tooMany.Add(i);

        var sut = new EmploymentChecksController(_repository.Object);

        // Act
        var result = await sut.Get(tooMany) as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
        result.Value.Should().Be("apprenticeshipIds must not exceed 1000.");
        _repository.VerifyNoOtherCalls();
    }
}
