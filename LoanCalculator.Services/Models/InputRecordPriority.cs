using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoanCalculator.Services.Models
{
    public class InputRecordPriority
    {
        public string InterestPriority { get; set; }
        public string PrincipalPriority { get; set; }
        public string OriginationFeePriority { get; set; }
        public string ServiceFeePriority { get; set; }
        public string ServiceFeeInterestPriority { get; set; }
        public string ManagementFeePriority { get; set; }
        public string MaintenanceFeePriority { get; set; }
        public string NSFFeePriority { get; set; }
        public string LateFeePriority { get; set; }
        public string SameDayFeePriority { get; set; }
    }
}
