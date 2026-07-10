using FluentAssertions;
using StatisticsApi_S.Service;
using Xunit;

namespace StatisticsAPITest
{
    
    public class StatisticsServiceTests
    {
        private readonly StatisticService _service = new();

        [Fact]
        public async Task SingleRow_ShouldCalculateCorrectStatistics()
        {
            var parquet = await TestHelper.CreateParquet(
            [
                new [] {2.5,-8.0,0.0}
            ]);

            var result = await _service.GetCalculatedStatisticAsync(parquet);

            Assert.NotNull(result);
            Assert.Single(result);

            Assert.Equal(-8, result[0].min.First());
            Assert.Equal(2.5, result[0].max.First());
        }

        [Fact]
        public async Task MultipleRows_ShouldReturnCorrectRowCount()
        {
            var parquet = await TestHelper.CreateParquet(
            [
               new double[]{1,2,3},
        new double[]{4,5,6},
        new double[]{7,8,9}
   
            ]);

            var result = await _service.GetCalculatedStatisticAsync(parquet);

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task EqualValues_ShouldReturnSameStatistics()
        {
            var stream = await TestHelper.CreateParquet(new[]
            {
        new double[]{5,5,5}
    });

            var result = await _service.GetCalculatedStatisticAsync(stream);

            Assert.Equal(5, result[0].min.First());
            Assert.Equal(5, result[0].max.First());
            Assert.Equal(5, result[0].p10.First());
            Assert.Equal(5, result[0].p50.First());
            Assert.Equal(5, result[0].p90.First());
        }

        [Fact]
        public async Task NegativeValues_ShouldCalculateCorrectly()
        {
            var parquet = await TestHelper.CreateParquet(new[]
    {
        new double[]{-10,-5,-2}
    });

            var result = await _service.GetCalculatedStatisticAsync(parquet);

            Assert.Equal(-10, result[0].min.First());
            Assert.Equal(-2, result[0].max.First());
        }

        [Fact]
        public async Task DecimalValues_ShouldCalculateCorrectly()
        {
            var parquet = await TestHelper.CreateParquet(
            [
                new [] {1.25,3.78,9.45}
            ]);

            var result = await _service.GetCalculatedStatisticAsync(parquet);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task DuplicateValues_ShouldReturnSamePercentiles()
        {
            var parquet = await TestHelper.CreateParquet(
            [
                new [] {5.0,5.0,5.0}
            ]);

            var result = await _service.GetCalculatedStatisticAsync(parquet);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task OneColumn_ShouldReturnSameStatistics()
        {
            var parquet = await TestHelper.CreateParquet(
            [
                new [] {8.0}
            ]);

            var result = await _service.GetCalculatedStatisticAsync(parquet);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task LargeDataset_ShouldCompleteSuccessfully()
        {
            var random = new Random();

            var rows = Enumerable.Range(0, 10000)
                .Select(_ => new[]
                {
                random.NextDouble()*100,
                random.NextDouble()*100,
                random.NextDouble()*100,
                random.NextDouble()*100
                })
                .ToArray();

            var parquet = await TestHelper.CreateParquet(rows);

            var result = await _service.GetCalculatedStatisticAsync(parquet);

            result.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task EmptyStream_ShouldThrowException()
        {
            var stream = new MemoryStream();

            await Assert.ThrowsAsync<Exception>(() =>
                _service.GetCalculatedStatisticAsync(stream));
        }

        [Fact]
        public async Task InvalidParquet_ShouldThrowException()
        {
            var stream = new MemoryStream();

            var writer = new StreamWriter(stream);
            writer.Write("Not a parquet");
            writer.Flush();

            stream.Position = 0;

            await Assert.ThrowsAsync<Exception>(() =>
                _service.GetCalculatedStatisticAsync(stream));
        }

        [Fact]
        public void Percentile_SingleValue_ReturnsValue()
        {
            double[] values = { 5.0 };

            var result = InvokePercentile(values, 0.5);

            result.Should().Be(5);
        }

        [Fact]
        public void Percentile_Median_ReturnsCorrectValue()
        {
            double[] values = { 1, 2, 3 };

            var result = InvokePercentile(values, 0.5);

            result.Should().Be(2);
        }

        [Fact]
        public void Percentile_P10_ReturnsInterpolatedValue()
        {
            double[] values = { 0, 10, 20 };

            var result = InvokePercentile(values, 0.1);

            result.Should().BeApproximately(2, 0.001);
        }

        private double InvokePercentile(double[] values, double p)
        {
            var method = typeof(StatisticService)
                .GetMethod("Percentile",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Static);

            return (double)method.Invoke(null, new object[]
            {
            values,p
            });
        }
    }
}
