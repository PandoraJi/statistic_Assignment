using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StatisticsApi_S.Controllers;
using StatisticsApi_S.Model;
using StatisticsApi_S.Service;
using Xunit;

namespace StatisticsAPITest
{
    public class StatisticsControllerTests
    {


        private readonly Mock<IStatisticService> _serviceMock;
        private readonly StatisticsController _controller;

        public StatisticsControllerTests()
        {
            _serviceMock = new Mock<IStatisticService>();
            _controller = new StatisticsController(_serviceMock.Object);
        }

        private IFormFile CreateMockFile(string fileName = "test.parquet", int length = 100)
        {
            var content = new byte[length];

            var stream = new MemoryStream(content);

            return new FormFile(stream, 0, length, "file", fileName);
        }

        #region Positive Tests

        [Fact]
        public async Task Statistics_ShouldReturnOk_WhenValidParquetFile()
        {
            // Arrange

            var file = CreateMockFile();

            var expected = new List<StatisticModel>()
            {
                new StatisticModel()
                {
                    min = new List<double>(),
                    max = new List < double >(),
                    p10 = new List < double >(),
                    p50 = new List < double >(),
                    p90 = new List < double >()
                }
            };

            _serviceMock
                .Setup(x => x.GetCalculatedStatisticAsync(It.IsAny<Stream>()))
                .ReturnsAsync(expected);

            // Act

            var result = await _controller.Statistics(file);

            // Assert

            var ok = Assert.IsType<OkObjectResult>(result.Result);

            var value = Assert.IsAssignableFrom<List<StatisticModel>>(ok.Value);

            Assert.Single(value);
        }

        #endregion

        #region Negative Tests

        [Fact]
        public async Task Statistics_ShouldReturnBadRequest_WhenFileIsNull()
        {
            // Act

            var result = await _controller.Statistics(null);

            // Assert

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);

            Assert.Equal("Missing parquet file.", badRequest.Value);
        }

        [Fact]
        public async Task Statistics_ShouldReturnBadRequest_WhenFileLengthIsZero()
        {
            // Arrange

            var file = CreateMockFile(length: 0);

            // Act

            var result = await _controller.Statistics(file);

            // Assert

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);

            Assert.Equal("Missing parquet file.", badRequest.Value);
        }

        [Fact]
        public async Task Statistics_ShouldReturnBadRequest_WhenServiceReturnsNull()
        {
            // Arrange

            var file = CreateMockFile();

            _serviceMock
                .Setup(x => x.GetCalculatedStatisticAsync(It.IsAny<Stream>()))
                .ReturnsAsync((List<StatisticModel>)null);

            // Act

            var result = await _controller.Statistics(file);

            // Assert

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);

            Assert.Equal("Error processing the parquet file.", badRequest.Value);
        }

        #endregion

        #region Exception Tests

        [Fact]
        public async Task Statistics_ShouldReturnBadRequest_WhenServiceThrowsException()
        {
            // Arrange

            var file = CreateMockFile();

            _serviceMock
                .Setup(x => x.GetCalculatedStatisticAsync(It.IsAny<Stream>()))
                .ThrowsAsync(new Exception("Unexpected exception"));

            // Act

            var result = await _controller.Statistics(file);

            // Assert

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);

            Assert.Contains("Unexpected exception", badRequest.Value.ToString());
        }

        #endregion

        #region Boundary Tests

        [Fact]
        public async Task Statistics_ShouldReturnOk_WhenSingleRowExists()
        {
            // Arrange

            var file = CreateMockFile();

            var expected = new List<StatisticModel>()
            {
                new StatisticModel()
                {
                    min = new List<double>(),
                    max = new List<double>(),
                    p10 = new List<double>(),
                    p50 = new List<double>(),
                    p90 = new List<double>()
                }
            };

            _serviceMock
                .Setup(x => x.GetCalculatedStatisticAsync(It.IsAny<Stream>()))
                .ReturnsAsync(expected);

            // Act

            var result = await _controller.Statistics(file);

            // Assert

            var ok = Assert.IsType<OkObjectResult>(result.Result);

            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task Statistics_ShouldReturnOk_WhenAllValuesAreEqual()
        {
            // Arrange

            var file = CreateMockFile();

            var expected = new List<StatisticModel>()
            {
                new StatisticModel()
                {
                    min = new List < double >(),
                    max = new List < double >(),
                    p10 = new List < double >(),
                    p50 = new List < double >(),
                    p90 = new List < double >()
                }
            };

            _serviceMock
                .Setup(x => x.GetCalculatedStatisticAsync(It.IsAny<Stream>()))
                .ReturnsAsync(expected);

            // Act

            var result = await _controller.Statistics(file);

            // Assert

            var ok = Assert.IsType<OkObjectResult>(result.Result);

            var model = Assert.IsAssignableFrom<List<StatisticModel>>(ok.Value);

            Assert.Equal(100, model[0].min[0]);
            Assert.Equal(100, model[0].max[0]);
        }

        #endregion

        #region Unexpected Tests

        [Fact]
        public async Task Statistics_ShouldHandleCorruptedParquetFile()
        {
            // Arrange

            var file = CreateMockFile();

            _serviceMock
                .Setup(x => x.GetCalculatedStatisticAsync(It.IsAny<Stream>()))
                .ThrowsAsync(new InvalidDataException("Corrupted parquet file"));

            // Act

            var result = await _controller.Statistics(file);

            // Assert

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);

            Assert.Contains("Corrupted parquet file", badRequest.Value.ToString());
        }

        [Fact]
        public async Task Statistics_ShouldHandleUnauthorizedAccessException()
        {
            // Arrange

            var file = CreateMockFile();

            _serviceMock
                .Setup(x => x.GetCalculatedStatisticAsync(It.IsAny<Stream>()))
                .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

            // Act

            var result = await _controller.Statistics(file);

            // Assert

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);

            Assert.Contains("Access denied", badRequest.Value.ToString());
        }

        #endregion
    }

}

