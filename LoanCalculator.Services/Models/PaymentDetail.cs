using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoanCalculator.Services.Models
{
    // AD - 1.0.0.2 : Add CumulativePrincipal property 

    public class PaymentDetail : IComparable<PaymentDetail> //a List<> of these is returnded by getSchedule
    {
        public DateTime PaymentDate { get; set; }
        public double BeginningPrincipal { get; set; } //BeginningPrincipal, InterestPayment, CumulativeInterest, and PeriodicInterestRate are
        //all set to zero for additional payments.
        public double PrincipalPayment { get; set; }
        public double InterestPayment { get; set; }
        public double TotalPayment { get; set; }
        public double CumulativePayment { get; set; }
        public double CumulativeInterest { get; set; }
        public double CumulativePrincipal { get; set; }
        public double PeriodicInterestRate { get; set; }
        public int PaymentID { get; set; }
        public int Flags { get; set; }//(Flags & 2) == 2 in the output indicates an additional payment.
        //Currently this is equivalent to Flags == 2, but the bitwise test above should be used to allow for additional indications
        //in the Flags value.

        // AD 1.0.0.6 - added new properties for additional fees
        public double ServiceFee { get; set; }
        public double ServiceFeeInterest { get; set; }
        public double ServiceFeeTotal { get; set; }
        public double OriginationFee { get; set; }
        public double ManagementFee { get; set; }
        public double MaintenanceFee { get; set; }

        public double CumulativeServiceFee { get; set; }
        public double CumulativeServiceFeeInterest { get; set; }
        public double CumulativeOriginationFee { get; set; }
        public double CumulativeManagementFee { get; set; }
        public double CumulativeMaintenanceFee { get; set; }
        public double CumulativeTotalFees { get; set; }
        // AD 1.0.0.7 - add property for beginning Service Fee
        public double BeginningServiceFee { get; set; }
        // AD 1.0.0.6-1 - add Beginning anc Cumulative values for ServiceFee
        public double CumulativeServiceFeeTotal { get; set; }

        public DateTime DueDate { get; set; }
        public double NSFFee { get; set; }
        public double LateFee { get; set; }
        public double SameDayFee { get; set; }
        public double CumulativeNSFFee;
        public double CumulativeLateFee;
        public double CumulativeSameDayFee;
        public double PrincipalPaid;
        public double InterestPaid;
        public double SameDayFeePaid;
        public double ServiceFeeInterestPaid;
        public double OriginationFeePaid;
        public double MaintenanceFeePaid;
        public double ManagementFeePaid;  
        public double NSFFeePaid;
        public double LateFeePaid;
        public double TotalPaid;
        public double PrincipalPastDue;
        public double InterestPastDue;
        public double SameDayFeePastDue;
        public double ServiceFeeInterestPastDue;
        public double OriginationFeePastDue;
        public double MaintenanceFeePastDue;
        public double ManagementFeePastDue;
        public double NSFFeePastDue;
        public double LateFeePastDue;
        public double TotalPastDue;
        public double CumulativePrincipalPastDue;
        public double CumulativeInterestPastDue;
        public double CumulativeSameDayFeePastDue;
        public double CumulativeServiceFeeInterestPastDue;
        public double CumulativeOriginationFeePastDue;
        public double CumulativeMaintenanceFeePastDue;
        public double CumulativeManagementFeePastDue;
        public double CumulativeNSFFeePastDue;
        public double CumulativeLateFeePastDue;
        public double CumulativeTotalPastDue;



        int IComparable<PaymentDetail>.CompareTo(PaymentDetail other)
        {
            return this.PaymentDate.CompareTo(other.PaymentDate);
        }
    }
}
