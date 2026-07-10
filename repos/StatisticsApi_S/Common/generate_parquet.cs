using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Parquet;
using Parquet.Data;
using Parquet.Schema;
namespace StatisticsApi_S.Common
{
    public class generate_parquet
    {


        private const int NUM_COLUMNS = 10;
        private const int NUM_ROWS = 100;
        private const int ROW_GROUP_SIZE = 50;
        private const string OUTPUT_PATH = "random_walk.parquet";

        /// <summary>
        /// Generates random walk columns.
        /// 
        /// </summary>
        public Dictionary<string, double[]> GenerateRandomWalkColumns(
            int numColumns,
            int numRows,
            int seed = 42)
        {
            var random = new Random(seed);
            var columns = new Dictionary<string, double[]>();

            for (int c = 0; c < numColumns; c++)
            {
                double[] values = new double[numRows];

                // Starting valueItems between 1000 and 2000
                double start = NextDouble(random, 1000.0, 2000.0);
                values[0] = start;

                double current = start;

                // Random walk
                for (int r = 1; r < numRows; r++)
                {
                    current += NextDouble(random, -1.0, 1.0);
                    values[r] = current;
                }

                columns.Add($"col_{c:D2}", values);
            }

            return columns;
        }

        /// <summary>
        /// Writes generated data into a parquet file using row groups.
        /// </summary>
        public void WriteParquet(
            Dictionary<string, double[]> data,
            string outputPath,
            int rowGroupSize)
        {
            var fields = data.Keys
                .Select(name => new DataField<double>(name))
                .ToArray();

            var schema = new ParquetSchema(fields);

            using Stream fileStream = File.Create(outputPath);

             var writer = awaitableParquetWriter(schema, fileStream);

            int rowCount = data.First().Value.Length;

            for (int offset = 0; offset < rowCount; offset += rowGroupSize)
            {
                int count = Math.Min(rowGroupSize, rowCount - offset);

                using ParquetRowGroupWriter rowGroup = writer.CreateRowGroup();

                foreach (var field in fields)
                {
                    double[] values = data[field.Name]
                        .Skip(offset)
                        .Take(count)
                        .ToArray();

                   //var column = new RawColumnData(field, values);
                   // double[] doublesValue = ((Parquet.Data.RawColumnData<double>)values).Values.ToArray();

                   // object valueItems = rowGroup.WriteColumn(column);
                }
            }
        }

        /// <summary>
        /// Displays parquet metadata.
        /// </summary>
        public void PrintMetadata(string filePath)
        {
            using Stream stream = File.OpenRead(filePath);

            using var reader =  ParquetReader.CreateAsync(stream);

            Console.WriteLine($"Wrote {filePath}");
            Console.WriteLine($"Rows       : {reader.Result.Metadata!.NumRows}");
            Console.WriteLine($"Columns    : {reader.Result.Schema.Fields.Count}");
            Console.WriteLine($"Row Groups : {reader.Result.RowGroupCount}");
        }

        /// <summary>
        /// Executes the complete workflow.
        /// </summary>
        public void Run()
        {
            var data = GenerateRandomWalkColumns(NUM_COLUMNS, NUM_ROWS);

            WriteParquet(data, OUTPUT_PATH, ROW_GROUP_SIZE);

            PrintMetadata(OUTPUT_PATH);
        }


        public async Task<MemoryStream> CreateParquetFile(List<Model.StatisticModel> statisticModels)
        {

            if (statisticModels == null || statisticModels.Count == 0)
            {
                throw new ArgumentException("The statisticModels list cannot be null or empty.");
            }
            else
            {
                var output = new MemoryStream();
                var minValue = statisticModels.SelectMany(m => m.min).ToArray();
                var maxValue = statisticModels.SelectMany(m => m.max).ToArray();
                var p10Value = statisticModels.SelectMany(m => m.p10).ToArray();
                var p50Value = statisticModels.SelectMany(m => m.p50).ToArray();
                var p90Value = statisticModels.SelectMany(m => m.p90).ToArray();
                var schema = new ParquetSchema(
                    new DataField<double>("min"),
                    new DataField<double>("max"),
                    new DataField<double>("p10"),
                    new DataField<double>("p50"),
                    new DataField<double>("p90")
                );


                await using (var writer = await ParquetWriter.CreateAsync(schema, output))
                {
                    using var rowGroup = writer.CreateRowGroup();

                    //await rowGroup.WriteAsync((DataField)schema.Fields[0], minValue);
                    //await rowGroup.WriteAsync((DataField)schema.Fields[1], maxValue);
                    //await rowGroup.WriteAsync((DataField)schema.Fields[2], p10Value);
                    //await rowGroup.WriteAsync((DataField)schema.Fields[3], p50Value);
                    //await rowGroup.WriteAsync((DataField)schema.Fields[4], p90Value);
                }
                //await using (var writer = await ParquetWriter.CreateAsync(schema, output))
                //{
                //    using var rowGroup = writer.CreateRowGroup();

                //    await rowGroup.WriteAsync(schema.Fields[0] as DataField, minValue!=null? minValue.ToString():null);
                //    await rowGroup.WriteAsync(schema.Fields[1] as DataField, maxValue != null ? maxValue.ToString() : null);
                //    await rowGroup.WriteAsync(schema.Fields[2] as DataField, p10Value != null ? p10Value.ToString() : null);
                //    await rowGroup.WriteAsync(schema.Fields[3] as DataField, p50Value != null ? p50Value.ToString() : null);
                //    await rowGroup.WriteAsync(schema.Fields[4] as DataField, p90Value != null ? p90Value.ToString() : null);


                //}

                output.Position = 0;

                return output;
            }

        }
        /// <summary>
        /// Helper for uniform random double.
        /// </summary>
        private static double NextDouble(Random random, double min, double max)
        {
            return min + random.NextDouble() * (max - min);
        }

        /// <summary>
        /// Creates a synchronous ParquetWriter.
        /// </summary>
        private static ParquetWriter awaitableParquetWriter(ParquetSchema schema, Stream stream)
        {
            return ParquetWriter.CreateAsync(schema, stream)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Creates a synchronous ParquetReader.
        /// </summary>
        private static ParquetReader awaitParquetReader(Stream stream)
        {
            return ParquetReader.CreateAsync(stream)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }
    }
}
