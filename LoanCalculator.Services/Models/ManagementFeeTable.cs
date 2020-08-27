using System;

namespace LoanCalculator.Services.Models
{
    /// <summary>
    /// This class holds the properties to define the management fee assessment values.
    /// </summary>
    public class ManagementFeeTable
    {
        public DateTime AssessmentDate { get; set; }
        public double Fee { get; set; }
        public string PaymentId { get; set; }
    }
}
