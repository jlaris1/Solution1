using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoanCalculator.Services.Models
{
    public class InputRecordSchedule : IComparable<InputRecordSchedule>
    {
        public DateTime DateIn { get; set; }//First (earliest) date is the effective date. Flags and PaymentID are ignored for first record.
        //public int Flags { get; set; }//When (Flags & 1) == 1, skip the payment.
        //public int PaymentID { get; set; }

        int IComparable<InputRecordSchedule>.CompareTo(InputRecordSchedule other)
        {
            return this.DateIn.CompareTo(other.DateIn);
        }
    }
}
