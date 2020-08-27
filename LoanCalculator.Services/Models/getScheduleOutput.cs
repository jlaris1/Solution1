using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoanCalculator.Services.Models
{
    public class getScheduleOutput
    {
        public List<PaymentDetail> Schedule;
        public double RegularPayment;
        public double RegularServiceFeePayment;
        public double AccruedInterest;
        public double AccruedPrincipal;
        public DateTime MaturityDate;
        public double AmountFinanced;
        public double FinancingCost;
        public double APR;
        public double PastDueEffective;

    }
}
