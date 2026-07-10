using Parquet;
using Parquet.Data;
using Parquet.Meta;
using Parquet.Schema;
using StatisticsApi_S.Model;
using System.Data;
using System.Data.Common;
using System.Reflection.PortableExecutable;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace StatisticsApi_S.Service
{
    public class StatisticService : IStatisticService
    {
        public async Task<List<Model.StatisticModel>> GetCalculatedStatisticAsync(Stream parquetData)
        {
            try
            {
                var statisticModelList = new List<Model.StatisticModel>();
                using (var stream = parquetData)
                {
                   
                    var readerStatistic = await ParquetReader.CreateAsync(stream);
                    var items = new List<double[]>();
                    int totalRows = (int)readerStatistic.Metadata!.NumRows;
                    for (int i = 0; i < readerStatistic.RowGroupCount; i++)
                    {
                        using (var rowGroupReader = readerStatistic.OpenRowGroupReader(i))
                        {
                            var columns = new List<double[]>();
                            foreach (DataField field in readerStatistic.Schema.GetDataFields())
                            {

                                //DataColumn columnData = await rowGroupReader.ReadRawColumnDataBaseAsync(field);
                                    //DataColumn values = (DataColumn) rowGroupReader.ReadAsync(field,columns);
                                RawColumnData values = await rowGroupReader.ReadRawColumnDataBaseAsync(field);

                                double[] doublesValue = ((Parquet.Data.RawColumnData<double>)values).Values.ToArray();

                                //items.Add(doubles);

                                columns.Add(doublesValue as double[]);
                            }
                            //items.AddRange(columns);
                           
                            int rowCount = columns[0].Length;

                            for (int r = 0; r < rowCount; r++)
                            {
                                var item = new double[columns.Count];

                                for (int c = 0; c < columns.Count; c++)
                                    item[c] = columns[c][r];

                                items.Add(item);
                            }
                        }
                    }
                   

                    StatisticModel statisticModel;
                        foreach (var row in items)
                        {
                         statisticModel= new StatisticModel();
                            Array.Sort(row);

                        statisticModel.min.Add(row.First());
                        statisticModel.max.Add(row.Last());                        
                        statisticModel.p10.Add(Percentile(row, 0.10));
                        statisticModel.p50.Add(Percentile(row, 0.50));
                        statisticModel.p90.Add(Percentile(row, 0.90));
                        statisticModelList.Add(statisticModel);
                        
                    }
                   

                }

       //         var schema = new ParquetSchema(
       //new DataField<double>("min"),
       //new DataField<double>("max"),
       //new DataField<double>("p10"),
       //new DataField<double>("p50"),
       //new DataField<double>("p90"));


       //         var output = new MemoryStream();

       //         // use await using for IAsyncDisposable
       //         await using (var writer = await ParquetWriter.CreateAsync(schema, output))
       //         {
       //             using var group = writer.CreateRowGroup();

       //             // Parquet.Net v6 uses synchronous WriteColumn
       //             group.WriteAsync((DataField<double>)schema.Fields[0], statisticModelList.SelectMany(m => m.min.ToString()).ToArray());
       //             group.WriteAsync((DataField<double>)schema.Fields[1], statisticModelList.SelectMany(m => m.max.ToString()).ToArray() );
       //             group.WriteAsync((DataField<double>)schema.Fields[2], statisticModelList.SelectMany(m => m.p10.ToString()).ToArray() );
       //             group.WriteAsync((DataField<double>)schema.Fields[3], statisticModelList.SelectMany(m => m.p50.ToString()).ToArray() );
       //             group.WriteAsync((DataField<double>)schema.Fields[4], statisticModelList.SelectMany(m => m.p90.ToString()).ToArray() );

       //         }

                //output.Position = 0;
                //return output;



                return statisticModelList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while calculating statistics: {ex.Message}");
                return null;
            }
        }


       

        private static double Percentile(double[] sorted, double percentile)
        {
            if (sorted.Length == 1)
                return sorted[0];

            double index = percentile * (sorted.Length - 1);

            int lower = (int)Math.Floor(index);
            int upper = (int)Math.Ceiling(index);

            if (lower == upper)
                return sorted[lower];

            double fraction = index - lower;

            return sorted[lower] +
                   (sorted[upper] - sorted[lower]) * fraction;
        }


    }
}
