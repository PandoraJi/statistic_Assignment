namespace StatisticsApi_S.Model
{
    public class StatisticModel
            {

       public StatisticModel()
        {
            min = new List<double>();
            max = new List<double>();
            p10 = new List<double>();
            p50 = new List<double>();
            p90 = new List<double>();

        }
        public int Id { get; set; }
        public List<double> min { get; set; /*set => min = new List<double>();*/ }

        public List<double> max { get; set; /*set => max = new List<double>();*/ }

        public List<double> p10 { get; set; /*set => p10 = new List<double>();*/ }

        public List<double> p50 { get; set; /*set => p50 = new List<double>();*/ }
        public List<double> p90 { get; set; /*set => p90 = new List<double>();*/ }



    }
}
