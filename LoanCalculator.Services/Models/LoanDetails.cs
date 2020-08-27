using System;
using System.Collections.Generic;

namespace LoanCalculator.Services.Models
{
    //This class holds all the inputs to create the repayment schedule.
    public class LoanDetails
    {
        #region Common for all

        public List<InputGrid> InputRecords { get; set; }
        public List<AdditionalPaymentRecord> AdditionalPaymentRecords { get; set; }
        public double LoanAmount { get; set; }
        public PaymentPeriod PmtPeriod { get; set; }
        public DateTime EarlyPayoffDate { get; set; }
        public int MinDuration { get; set; }
        public DateTime AccruedInterestDate { get; set; }
        public bool RecastAdditionalPayments { get; set; }
        public bool UseFlexibleCalculation { get; set; }
        public string LoanType { get; set; }
        public DateTime AccruedServiceFeeInterestDate { get; set; }
        public string DaysInYearBankMethod { get; set; }
        public string DaysInMonth { get; set; }
        public double AmountFinanced { get; set; }
        public bool SameAsCashPayoff { get; set; }
        public int InterestDelay { get; set; }
        public double Residual { get; set; }
        public double PaymentAmount { get; set; }
        public bool AfterPayment { get; set; }
        public double BalloonPayment { get; set; }

        #endregion

        #region For Rigid Method only

        public short DaysInYear { get; set; }
        public bool IsInterestRounded { get; set; }

        #endregion

        #region Fee tab

        public double ServiceFee { get; set; }
        public int ApplyServiceFeeInterest { get; set; }
        public bool IsServiceFeeFinanced { get; set; }
        public bool ServiceFeeFirstPayment { get; set; }
        public bool IsServiceFeeIncremental { get; set; }
        public double SameDayFee { get; set; }
        public string SameDayFeeCalculationMethod { get; set; }
        public bool IsSameDayFeeFinanced { get; set; }
        public double OriginationFee { get; set; }
        public double OriginationFeePercent { get; set; }
        public double OriginationFeeMax { get; set; }
        public double OriginationFeeMin { get; set; }
        public bool OriginationFeeCalculationMethod { get; set; }
        public bool IsOriginationFeeFinanced { get; set; }
        public double EnforcedPrincipal { get; set; }
        public int EnforcedPayment { get; set; }

        #endregion

        #region Priority tab

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

        #region Management Fee tab

        public double ManagementFee { get; set; }
        public double ManagementFeePercent { get; set; }
        public int ManagementFeeBasis { get; set; }
        public int ManagementFeeFrequency { get; set; }
        public int ManagementFeeFrequencyNumber { get; set; }
        public int ManagementFeeDelay { get; set; }
        public double ManagementFeeMin { get; set; }
        public double ManagementFeeMaxPer { get; set; }
        public double ManagementFeeMaxMonth { get; set; }
        public double ManagementFeeMaxLoan { get; set; }
        public bool IsManagementFeeGreater { get; set; }

        #endregion

        #region Maintenance Fee tab

        public double MaintenanceFee { get; set; }
        public double MaintenanceFeePercent { get; set; }
        public int MaintenanceFeeBasis { get; set; }
        public int MaintenanceFeeFrequency { get; set; }
        public int MaintenanceFeeFrequencyNumber { get; set; }
        public int MaintenanceFeeDelay { get; set; }
        public double MaintenanceFeeMin { get; set; }
        public double MaintenanceFeeMaxPer { get; set; }
        public double MaintenanceFeeMaxMonth { get; set; }
        public double MaintenanceFeeMaxLoan { get; set; }
        public bool IsMaintenanceFeeGreater { get; set; }

        #endregion

        #region Earned Fee tab

        public double EarnedInterest { get; set; }
        public double EarnedServiceFeeInterest { get; set; }
        public double EarnedOriginationFee { get; set; }
        public double EarnedSameDayFee { get; set; }
        public double EarnedManagementFee { get; set; }
        public double EarnedMaintenanceFee { get; set; }
        public double EarnedNSFFee { get; set; }
        public double EarnedLateFee { get; set; }

        #endregion

        #region interest rate tier tab
        
        public string InterestRate { get; set; }
        public double Tier1 { get; set; }
        public string InterestRate2 { get; set; }
        public double Tier2 { get; set; }
        public string InterestRate3 { get; set; }
        public double Tier3 { get; set; }
        public string InterestRate4 { get; set; }
        public double Tier4 { get; set; }

        #endregion
    }
}
