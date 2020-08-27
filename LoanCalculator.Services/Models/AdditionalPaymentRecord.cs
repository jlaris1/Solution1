using System;

namespace LoanCalculator.Services.Models
{
    /// <summary>
    /// This class will hold the additional payment details, and will be used to create repayment schedule.
    /// </summary>
    public class AdditionalPaymentRecord: IComparable<AdditionalPaymentRecord>
    {
        #region For Bank Method only

        public int Flags { get; set; }
        public string InterestRate { get; set; }
        public double PrincipalDiscount { get; set; }
        public double InterestDiscount { get; set; }
        public double OriginationFeeDiscount { get; set; }
        public double SameDayFeeDiscount { get; set; }
        public double ServiceFeeDiscount { get; set; }
        public double ServiceFeeInterestDiscount { get; set; }
        public double ManagementFeeDiscount { get; set; }
        public double MaintenanceFeeDiscount { get; set; }
        public double NSFFeeDiscount { get; set; }
        public double LateFeeDiscount { get; set; }
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

        #region Common for all

        public DateTime DateIn { get; set; }
        public double AdditionalPayment { get; set; }
        public int PaymentID { get; set; }
        public bool PrincipalOnly { get; set; }

        #endregion

        int IComparable<AdditionalPaymentRecord>.CompareTo(AdditionalPaymentRecord other)
        {
            return this.DateIn.CompareTo(other.DateIn);
        }

    }
}
