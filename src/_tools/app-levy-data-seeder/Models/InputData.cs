using System;

namespace app_levy_data_seeder.Models
{
    public class InputData  
    {
        public string status { get; set; }

        public jsonBody jsonBody { get; set; }
    }

    public class jsonBody
    {
        public string empref { get; set; }
        public string nino { get; set; }
        public DateTime fromDate { get; set; }
        public DateTime toDate { get; set; }
        public bool employed { get; set; }
    }
}
