using System;
using System.Collections.Generic;

namespace LoanCalculator.Services.Models
{
    //This class holds all the output values related to the loan repayment module.
    public class OutputSchedule
    {
        #region Common for all

        public List<OutputGrid> Schedule { get; set; }
        public double RegularPayment { get; set; }
        public double RegularServiceFeePayment { get; set; }
        public double AccruedInterest { get; set; }
        public double AccruedPrincipal { get; set; }

        #endregion

        #region For Bank Method only

        public double AccruedServiceFeeInterest { get; set; }
        public double AmountFinanced { get; set; }
        public DateTime MaturityDate { get; set; }
        public double CostOfFinancing { get; set; }
        public double LoanAmountCalc { get; set; }
        public double OriginationFee { get; set; }

        #endregion

        public List<ManagementFeeTable> ManagementFeeAssessment { get; set; }
        public List<MaintenanceFeeTable> MaintenanceFeeAssessment { get; set; }
        public double PayoffBalance { get; set; }
        public DateTime ManagementFeeEffective { get; set; }
        public DateTime MaintenanceFeeEffective { get; set; }
    }
}
