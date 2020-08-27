using System;

namespace LoanCalculator.Services.Models
{
    //This class contains the repayment schedule output, that will define what amount to pay on which date, what amount has been paid, what amount is due etc.
    public class OutputGrid : IComparable<OutputGrid>
    {
        #region For Period Method Only

        public double InterestDue { get; set; }

        public double ServiceFeeInterestDue { get; set; }

        #endregion

        #region For Bank Method Only

        public double DailyInterestRate { get; set; }
        public double DailyInterestAmount { get; set; }
        public DateTime DueDate { get; set; }
        public double AccruedServiceFeeInterest { get; set; }
        public double AccruedServiceFeeInterestCarryOver { get; set; }
        //Amount to be paid in the particular period columns
        public double SameDayFee { get; set; }
        public double NSFFee { get; set; }
        public double LateFee { get; set; }
        //Paid amount columns
        public double PrincipalPaid { get; set; }
        public double ServiceFeePaid { get; set; }
        public double ServiceFeeInterestPaid { get; set; }
        public double OriginationFeePaid { get; set; }
        public double MaintenanceFeePaid { get; set; }
        public double ManagementFeePaid { get; set; }
        public double SameDayFeePaid { get; set; }
        public double NSFFeePaid { get; set; }
        public double LateFeePaid { get; set; }
        public double TotalPaid { get; set; }
        //Cumulative Amount paid column
        public double CumulativeServiceFeeTotal { get; set; }
        public double CumulativeSameDayFee { get; set; }
        public double CumulativeNSFFee { get; set; }
        public double CumulativeLateFee { get; set; }
        //Past due amount columns
        public double PrincipalPastDue { get; set; }
        public double InterestPastDue { get; set; }
        public double ServiceFeePastDue { get; set; }
        public double ServiceFeeInterestPastDue { get; set; }
        public double OriginationFeePastDue { get; set; }
        public double MaintenanceFeePastDue { get; set; }
        public double ManagementFeePastDue { get; set; }
        public double SameDayFeePastDue { get; set; }
        public double NSFFeePastDue { get; set; }
        public double LateFeePastDue { get; set; }
        public double TotalPastDue { get; set; }
        //Cumulative past due amount columns
        public double CumulativePrincipalPastDue { get; set; }
        public double CumulativeInterestPastDue { get; set; }
        public double CumulativeServiceFeePastDue { get; set; }
        public double CumulativeServiceFeeInterestPastDue { get; set; }
        public double CumulativeOriginationFeePastDue { get; set; }
        public double CumulativeMaintenanceFeePastDue { get; set; }
        public double CumulativeManagementFeePastDue { get; set; }
        public double CumulativeSameDayFeePastDue { get; set; }
        public double CumulativeNSFFeePastDue { get; set; }
        public double CumulativeLateFeePastDue { get; set; }
        public double CumulativeTotalPastDue { get; set; }
        public string BucketStatus { get; set; }

        #endregion

        #region Common for all

        public double InterestAccrued { get; set; }
        public double InterestCarryOver { get; set; }
        public DateTime PaymentDate { get; set; }
        public double BeginningPrincipal { get; set; }
        public double BeginningServiceFee { get; set; }
        public double PeriodicInterestRate { get; set; }
        public int PaymentID { get; set; }
        public int Flags { get; set; }
        //Amount to be paid in the particular period columns
        public double InterestPayment { get; set; }
        public double PrincipalPayment { get; set; }
        public double TotalPayment { get; set; }
        public double ServiceFee { get; set; }
        public double ServiceFeeInterest { get; set; }
        public double ServiceFeeTotal { get; set; }
        public double OriginationFee { get; set; }
        public double MaintenanceFee { get; set; }
        public double ManagementFee { get; set; } 
        //Paid amount columns
        public double InterestPaid { get; set; }
        //Cumulative Amount paid column
        public double CumulativeInterest { get; set; }
        public double CumulativePrincipal { get; set; }
        public double CumulativePayment { get; set; }
        public double CumulativeServiceFee { get; set; }
        public double CumulativeServiceFeeInterest { get; set; }
        public double CumulativeOriginationFee { get; set; }
        public double CumulativeMaintenanceFee { get; set; }
        public double CumulativeManagementFee { get; set; }
        public double CumulativeTotalFees { get; set; }
        public double PrincipalServiceFeePayment { get; set; }
        public double InterestServiceFeeInterestPayment { get; set; }
        public double BeginningPrincipalServiceFee { get; set; }

        //---- Integrate----------
        public double serviceFeeInterestCarryOver { get; set; }

        #endregion

        #region For rolling Method

        public double PaymentDue { get; set; }

        #endregion

        int IComparable<OutputGrid>.CompareTo(OutputGrid other)
        {
            return this.PaymentDate.CompareTo(other.PaymentDate);
        }
    }
}
