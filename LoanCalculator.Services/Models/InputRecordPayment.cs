using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoanCalculator.Services.Models
{
    public class InputRecordPayment : IComparable<InputRecordPayment>
    {
        public DateTime DateIn { get; set; }
        public double PaymentAmount { get; set; }
        public int PaymentID { get; set; }
        public string flag { get; set; }

        int IComparable<InputRecordPayment>.CompareTo(InputRecordPayment other)
        {
            return this.DateIn.CompareTo(other.DateIn);
        }
    }
}
