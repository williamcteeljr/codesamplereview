using PolicyTracker.DomainModel.Framework;
using System;

namespace PolicyTracker.DomainModel.Other
{
    /// <summary>
    /// Used for tracking what records have been imported successfully into the system.
    /// </summary>
    public class DataImport : BaseEntity
    {
        public int ID { get; set; }
        public string Company { get; set; }
        public string Prefix { get; set; }
        public string PolicyNumber { get; set; }
        public string Suffix { get; set; }
        public DateTime? DateInserted { get; set; }
        public int WrittenPremium { get; set; }
        public int AnnualPremium { get; set; }
        public bool IsImported { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
