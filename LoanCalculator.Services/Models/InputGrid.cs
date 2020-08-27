using System;

namespace LoanCalculator.Services.Models
{
    /// <summary>
    /// This class holds the input details which defines the scheduled dates on which payments have to be paid. This class will be used to create repayment schedule.
    /// </summary>
    public class InputGrid : IComparable<InputGrid>
    {
        #region Common For all

        public DateTime DateIn { get; set; }//First (earliest) date is the effective date. Flags and PaymentID are ignored for first record.
        public int Flags { get; set; }//When (Flags & 1) == 1, skip the payment.
        public int PaymentID { get; set; }

        #endregion

        #region For Bank method only

        public DateTime EffectiveDate { get; set; }
        public string PaymentAmount { get; set; }
        public string InterestRate { get; set; }
        public int InterestPriority { get; set; }
        public int PrincipalPriority { get; set; }
        public int OriginationFeePriority { get; set; }
        public int SameDayFeePriority { get; set; }
        public int ServiceFeePriority { get; set; }
        public int ServiceFeeInterestPriority { get; set; }
        public int ManagementFeePriority { get; set; }
        public int MaintenanceFeePriority { get; set; }
        public int NSFFeePriority { get; set; }
        public int LateFeePriority { get; set; }

        #endregion

        int IComparable<InputGrid>.CompareTo(InputGrid other)
        {
            return this.DateIn.CompareTo(other.DateIn);
        }
    }
}
