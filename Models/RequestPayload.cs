using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class RequestPayload
    {
        public Guid RequestId { get; set; }
        public long RequestGroupId { get; set; }

        public Guid UserId { get; set; }
        public string UserName { get; set; }

        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }

        public string RequestorComments { get; set; }
        public string BusinessJustification { get; set; }
    }
}
