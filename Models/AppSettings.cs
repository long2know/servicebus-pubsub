using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class AppSettings
    {
        public string PubEndpoint { get; set; }
        public string SubEndpoint { get; set; }
        public string SubName { get; set; }
        public string TopicName { get; set; }
    }
}
