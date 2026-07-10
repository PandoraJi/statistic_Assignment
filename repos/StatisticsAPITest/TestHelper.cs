using Parquet;
using Parquet.Data;
using Parquet.Schema;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StatisticsAPITest
{
    public static class TestHelper
    {
        public static async Task<MemoryStream> CreateParquet(double[][] data)
        {
            int columnCount = data[0].Length;

            var fields = Enumerable.Range(1, columnCount)
                .Select(i => new DataField<double>($"C{i}"))
                .ToArray();

            var schema = new ParquetSchema(fields);

            var stream = new MemoryStream();

            await using (var writer = await ParquetWriter.CreateAsync(schema, stream))
            {
                using var rowGroup = writer.CreateRowGroup();

                for (int c = 0; c < columnCount; c++)
                {
                    var values = data.Select(r => r[c].ToString()).ToArray();
                    await rowGroup.WriteAsync(fields[c], values);
                    // Use synchronous API provided by Parquet.Net
                    //rowGroup.WriteColumn(new DataColumn(fields[c], values));
                }
            }

            stream.Position = 0;

            return stream;
        }
    }
}