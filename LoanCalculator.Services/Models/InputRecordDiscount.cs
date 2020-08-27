using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoanCalculator.Services.Models
{
    public class InputRecordDiscount : IComparable<InputRecordDiscount>
    {
        public DateTime DateIn { get; set; }
        public int PaymentID { get; set; }
        public double InterestDiscount { get; set; }
        public double PrincipalDiscount { get; set; }
        public double ServiceFeeDiscount { get; set; }
        public double ServiceFeeInterestDiscount { get; set; }
        public double OriginationFeeDiscount { get; set; }
        public double MaintenanceFeeDiscount { get; set; }
        public double ManagementFeeDiscount { get; set; }
        public double NSFFeeDiscount { get; set; }
        public double LateFeeDiscount { get; set; }
        public double SameDayFeeDiscount { get; set; }
        int IComparable<InputRecordDiscount>.CompareTo(InputRecordDiscount other)
        {
            return this.DateIn.CompareTo(other.DateIn);
        }
    }
}
