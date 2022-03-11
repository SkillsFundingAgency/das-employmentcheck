using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Models;
using System;
using System.Collections.Generic;
using System.Net;

namespace SFA.DAS.EmploymentCheck.Data.UnitTests.Models
{
    public class WhenCallingEmploymentCheckDataValidator
    {
        const string NinoNotFound = "NinoNotFound";
        const string NinoFailure = "NinoFailure";
        const string NinoInvalid = "NinoInvalid";
        const string PAYENotFound = "PAYENotFound";
        const string PAYEFailure = "PAYEFailure";

        private Fixture _fixture;
        private EmploymentCheckDataValidator _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _sut = new EmploymentCheckDataValidator();
        }

        [Test]
        public void When_There_Is_A_LearnerNiNumber_And_It_Is_Null_IsValidNino_Returns_False_And_ErrorType_Is_Set_To_NinoFailure()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
            LearnerNiNumber learnerNiNumber = null;
            var payeScheme = _fixture.Create<EmployerPayeSchemes>();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

            // Act
            var result = _sut.IsValidNino(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual(NinoFailure, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_A_LearnerNiNumber_And_The_Nino_Is_Null_IsValidNino_Returns_False_And_ErrorType_Is_Set_To_NinoNotFound()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
            var learnerNiNumber = new LearnerNiNumber(1, null, HttpStatusCode.NoContent);
            var payeScheme = _fixture.Create<EmployerPayeSchemes>();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

            // Act
            var result = _sut.IsValidNino(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual(NinoNotFound, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_A_LearnerNiNumber_And_The_Status_Is_NoContent_IsValidNino_Returns_False_And_ErrorType_Is_Set_To_NinoNotFound()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
            var learnerNiNumber = _fixture.Build<LearnerNiNumber>().With(x => x.HttpStatusCode, HttpStatusCode.NoContent).Create();
            var payeScheme = _fixture.Create<EmployerPayeSchemes>();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

            // Act
            var result = _sut.IsValidNino(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual(NinoNotFound, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_A_LearnerNiNumber_And_The_Status_Is_NotFound_IsValidNino_Returns_False_And_ErrorType_Is_Set_To_NinoNotFound()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
            var learnerNiNumber = _fixture.Build<LearnerNiNumber>().With(x => x.HttpStatusCode, HttpStatusCode.NotFound).Create();
            var payeScheme = _fixture.Create<EmployerPayeSchemes>();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

            // Act
            var result = _sut.IsValidNino(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual(NinoNotFound, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_A_LearnerNiNumber_And_The_Status_Is_Between_400_and_599_IsValidNino_Returns_False_And_ErrorType_Is_Set_To_NinoFailure()
        {
            // Arrange
            var payeScheme = _fixture.Create<EmployerPayeSchemes>();

            for (var i = 400; i <= 599; ++i)
            {
                if (i == 404) // skip the NotFound which returns NinoNotFound instead of NinoFailure
                    continue;

                var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
                var learnerNiNumber = _fixture.Build<LearnerNiNumber>().With(x => x.HttpStatusCode, (HttpStatusCode)i).Create();
                var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

                // Act
                var result = _sut.IsValidNino(employmentCheckData);

                // Assert
                result.Equals(false);
                Assert.AreEqual(NinoFailure, employmentCheckData.EmploymentCheck.ErrorType);
            }
        }

        [Test]
        public void When_There_Is_A_LearnerNiNumber_And_The_Nino_Length_Is_Less_Than_9_IsValidNino_Returns_False_And_ErrorType_Is_Set_To_NinoInvalid()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
            var learnerNiNumber = _fixture.Build<LearnerNiNumber>().With(x => x.NiNumber, "1234").Create();
            var payeScheme = _fixture.Create<EmployerPayeSchemes>();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

            // Act
            var result = _sut.IsValidNino(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual(NinoInvalid, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_A_LearnerNiNumber_And_It_Has_A_Valid_Nino_IsValidNino_Returns_True_And_ErrorType_Is_Set_To_Null()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var payeScheme = _fixture.Create<EmployerPayeSchemes>();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

            // Act
            var result = _sut.IsValidNino(employmentCheckData);

            // Assert
            result.Equals(true);
            Assert.AreEqual(null, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_It_Is_Null_And_ErrorType_Is_Empty_IsValidPayeScheme_Returns_False_And_ErrorType_Is_Set_To_PAYEFailure()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            EmployerPayeSchemes payeScheme = null;
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

            // Act
            var result = _sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual(PAYEFailure, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_It_Is_Null_And_ErrorType_Is_NinoNotFound_IsValidPayeScheme_Returns_False_And_ErrorType_Is_Set_To_NinoNotFoundAndPAYEFailure()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().With(x => x.ErrorType, NinoNotFound).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            EmployerPayeSchemes payeScheme = null;
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

            // Act
            var result = _sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual($"{NinoNotFound}And{PAYEFailure}", employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_The_PayeScheme_Is_Empty_And_ErrorType_Is_Empty_IsValidPayeScheme_Returns_False_And_ErrorType_Is_Set_To_PAYENotFound()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            EmployerPayeSchemes payeScheme = new EmployerPayeSchemes(1, HttpStatusCode.NoContent, null);
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

            // Act
            var result = _sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual(PAYENotFound, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_The_PayeScheme_Is_Empty_And_ErrorType_Is_NinoNotFound_IsValidPayeScheme_Returns_False_And_ErrorType_Is_Set_To_NinoNotFoundAndPAYENotFound()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().With(x => x.ErrorType, NinoNotFound).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            EmployerPayeSchemes payeScheme = new EmployerPayeSchemes(1, HttpStatusCode.NoContent, null);
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

            // Act
            var result = _sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual($"{NinoNotFound}And{PAYENotFound}", employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_The_Status_Is_NoContent_And_ErrorType_Is_Empty_IsValidPayeScheme_Returns_False_And_ErrorType_Is_Set_To_PAYENotFound()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var payeScheme = _fixture.Build<EmployerPayeSchemes>().With(x => x.HttpStatusCode, HttpStatusCode.NoContent).Create();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

            // Act
            var result = _sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual(PAYENotFound, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_The_Status_Is_NoContent_And_ErrorType_NinoNotFound_IsValidPayeScheme_Returns_False_And_ErrorType_Is_Set_To_NinoNotFoundAndPAYENotFound()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().With(x => x.ErrorType, NinoNotFound).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var payeScheme = _fixture.Build<EmployerPayeSchemes>().With(x => x.HttpStatusCode, HttpStatusCode.NoContent).Create();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

            // Act
            var result = _sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual($"{NinoNotFound}And{PAYENotFound}", employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_The_Status_Is_NotFound_And_ErrorType_Is_Empty_IsValidPayeScheme_Returns_False_And_ErrorType_Is_Set_To_PAYENotFound()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var payeScheme = _fixture.Build<EmployerPayeSchemes>().With(x => x.HttpStatusCode, HttpStatusCode.NotFound).Create();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

            // Act
            var result = _sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual(PAYENotFound, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_The_Status_Is_NotFound_And_ErrorType_Is_NinoNotFound_IsValidPayeScheme_Returns_False_And_ErrorType_Is_Set_To_NinoNotFoundAndPAYENotFound()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().With(x => x.ErrorType, NinoNotFound).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var payeScheme = _fixture.Build<EmployerPayeSchemes>().With(x => x.HttpStatusCode, HttpStatusCode.NotFound).Create();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

            // Act
            var result = _sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual($"{NinoNotFound}And{PAYENotFound}", employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_The_Status_Is_Between_400_And_599__And_ErrorType_Is_Empty_IsValidPayeScheme_Returns_False_And_ErrorType_Is_Set_To_PAYEFailure()
        {
            // Arrange
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();

            for (var i = 400; i <= 599; ++i)
            {
                if (i == 404) // skip the NotFound which returns PAYENotFound instead of PAYEFailure
                    continue;

                var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
                var payeScheme = _fixture.Build<EmployerPayeSchemes>().With(x => x.HttpStatusCode, (HttpStatusCode)i).Create();
                var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

                // Act
                var result = _sut.IsValidPayeScheme(employmentCheckData);

                // Assert
                result.Equals(false);
                Assert.AreEqual(PAYEFailure, employmentCheckData.EmploymentCheck.ErrorType);
            }
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_The_Status_Is_Between_400_And_599__And_ErrorType_Is_NinoNotFound_IsValidPayeScheme_Returns_False_And_ErrorType_Is_Set_To_NinoNotFoundAndPAYEFailure()
        {
            // Arrange
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();

            for (var i = 400; i <= 599; ++i)
            {
                if (i == 404) // skip the NotFound which returns PAYENotFound instead of PAYEFailure
                    continue;

                var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().With(x => x.ErrorType, NinoNotFound).Create();
                var payeScheme = _fixture.Build<EmployerPayeSchemes>().With(x => x.HttpStatusCode, (HttpStatusCode)i).Create();
                var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

                // Act
                var result = _sut.IsValidPayeScheme(employmentCheckData);

                // Assert
                result.Equals(false);
                Assert.AreEqual($"{NinoNotFound}And{PAYEFailure}", employmentCheckData.EmploymentCheck.ErrorType);
            }
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_The_PayeScheme_Has_A_Blank_PayeScheme_And_ErrorType_Is_Empty_IsValidPayeScheme_Returns_False_And_ErrorType_Is_Set_To_PAYENotFound()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var payeScheme = new EmployerPayeSchemes(1, HttpStatusCode.OK, new List<string> { { "Paye" }, { "" } });
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

            // Act
            var result = _sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual(PAYENotFound, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_The_PayeScheme_Has_A_Blank_PayeScheme_And_ErrorType_Is_NinoNotFound_IsValidPayeScheme_Returns_False_And_ErrorType_Is_Set_To_NinoNotFoundAndPAYENotFound()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().With(x => x.ErrorType, NinoNotFound).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var payeScheme = new EmployerPayeSchemes(1, HttpStatusCode.OK, new List<string> { { "Paye" }, { "" } });
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

            // Act
            var result = _sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual($"{NinoNotFound}And{PAYENotFound}", employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_ErrorType_Is_Empty_IsValidPayeScheme_Returns_True_And_ErrorType_Is_Set_To_Null()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var payeScheme = _fixture.Create<EmployerPayeSchemes>();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

            // Act
            var result = _sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual(null, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_A_NinoNotFound_Error_And_The_PayeScheme_Is_Null_IsValidPayeScheme_Returns_False()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().With(x => x.ErrorType, "NinoNotFound").Create();
            LearnerNiNumber learnerNiNumber = null;
            EmployerPayeSchemes payeScheme = null;
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

            // Act
            var result = _sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual("NinoNotFoundAndPAYEFailure", employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_EmploymentCheckData_Is_Null_An_ArgumentNullException_Is_Returned()
        {
            // Arrange
            EmploymentCheckData employmentCheckData = null;

            // Act
            Action result = () => _sut.IsValidEmploymentCheckData(employmentCheckData);

            // Assert
            result.Should().Throw<ArgumentNullException>();
        }
    }
}