using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolicyTracker.DomainModel.PostNotice
{
    public class Transaction
    {
        public string transactionid { get; set; }
        public string dimeattachmentid { get; set; }
        public string confirmationcode { get; set; }
        public string totallocationcount { get; set; }
    }
}
