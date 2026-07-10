namespace StatisticsApi_S.Service
{
    public interface IStatisticService
    {
        Task<List<Model.StatisticModel>> GetCalculatedStatisticAsync(Stream parquetData);
    }
}
