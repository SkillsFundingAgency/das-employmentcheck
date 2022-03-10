using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace SFA.DAS.EmploymentCheck.Queries.UnitTests.Dispatcher
{
    public class WhenSendIsCalled
    {
        private IQueryDispatcher _sut;
        private Mock<IServiceProvider> _mockServiceProvider;
        private Mock<IQueryHandler<TestQuery1, string>> _mockTestQuery1Handler;
        private Mock<IQueryHandler<TestQuery2, string>> _mockTestQuery2Handler;
        private Mock<IQueryHandler<TestQuery3, string>> _mockTestQuery3Handler;

        private Fixture _fixture;

        public class TestQuery1 : IQuery { }
        public class TestQuery2 : IQuery { }
        public class TestQuery3 : IQuery { }
        public class TestQuery4 : IQuery { }

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockTestQuery1Handler = new Mock<IQueryHandler<TestQuery1, string>>();
            _mockTestQuery2Handler = new Mock<IQueryHandler<TestQuery2, string>>();
            _mockTestQuery3Handler = new Mock<IQueryHandler<TestQuery3, string>>();

            _mockServiceProvider.Setup(m => m.GetService(typeof(IQueryHandler<TestQuery1, string>))).Returns(_mockTestQuery1Handler.Object);
            _mockServiceProvider.Setup(m => m.GetService(typeof(IQueryHandler<TestQuery2, string>))).Returns(_mockTestQuery2Handler.Object);
            _mockServiceProvider.Setup(m => m.GetService(typeof(IQueryHandler<TestQuery3, string>))).Returns(_mockTestQuery3Handler.Object);

            _sut = new QueryDispatcher(_mockServiceProvider.Object);
        }

        [Test]
        public async Task Then_the_matching_query_handler_is_retrieved_from_the_service_provider()
        {
            // Arrange
            var query = _fixture.Create<TestQuery1>();

            // Act
            await _sut.Send<TestQuery1, string>(query);

            // Assert
            _mockServiceProvider.Verify(m => m.GetService(typeof(IQueryHandler<TestQuery1, string>)), Times.Once);
        }

        [Test]
        public async Task Then_the_query_is_handled_by_the_matching_handler()
        {
            // Arrange
            var query = _fixture.Create<TestQuery1>();
            var isHandled = false;
            var mockHandler = new Mock<IQueryHandler<TestQuery1, string>>();

            _mockServiceProvider.Setup(m => m.GetService(typeof(IQueryHandler<TestQuery1, string>))).Returns(mockHandler.Object);

            mockHandler.Setup(m => m.Handle(query, It.IsAny<CancellationToken>()))
                .Callback(() =>
                {
                    isHandled = true;
                });

            // Act
            await _sut.Send<TestQuery1, string>(query);

            // Assert
            isHandled.Should().BeTrue();
        }

        [Test]
        public void Then_a_QueryDispatcherException_is_raised_if_there_is_no_matching_handler()
        {
            // Arrange
            var query = _fixture.Create<TestQuery4>();

            // Act
            Action action = () => _sut.Send<TestQuery4, string>(query);

            // Assert
            action.Should().Throw<QueryDispatcherException>()
                .Where(e => e.Message.StartsWith($"Unable to dispatch query '{query.GetType().Name}'. No matching handler found."));
        }

        [Test]
        public void Then_a_QueryException_is_raised_if_query_fails()
        {
            // Arrange
            var query = _fixture.Create<TestQuery1>();
            var mockHandler = new Mock<IQueryHandler<TestQuery1, string>>();
            const string expectedMessage = "Unable to execute query 'TestQuery1'.";
            _mockServiceProvider.Setup(m => m.GetService(typeof(IQueryHandler<TestQuery1, string>))).Returns(mockHandler.Object);
            var expectedBaseException = new DataException();

            mockHandler.Setup(m => m.Handle(query, It.IsAny<CancellationToken>()))
                .Callback(() => throw expectedBaseException);

            // Act
            Action action = () => _sut.Send<TestQuery1, string>(query);

            // Assert
            action.Should().Throw<QueryException>().Where(e =>
                e.Message.StartsWith(expectedMessage) &&
                e.InnerException == expectedBaseException);
        }
    }
}
