using System;

namespace LoanCalculator.Services.Models
{
    /// <summary>
    /// This class holds the properties to define the maintenance fee assessment values.
    /// </summary>
    public class MaintenanceFeeTable
    {
        public DateTime AssessmentDate { get; set; }
        public double Fee { get; set; }
        public string PaymentId { get; set; }
    }
}
