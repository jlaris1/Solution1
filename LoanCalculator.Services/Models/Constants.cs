using System;

LoanCalculator.Services.Models
{
    /// <summary>
    /// This class defines the default values used in the application.
    /// </summary>
    public class Constants
    {
        //This enum define default priority order, that will be used to allocate the available fund among all the components.
        public enum DefaultPriority : short
        {
            OriginationFee = 1,
            SameDayFee = 2,
            Interest = 3,
            ServiceFeeInterest = 4,
            Principal = 5,
            ServiceFee = 6,
            ManagementFee = 7,
            MaintenanceFee = 8,
            LateFee = 9,
            NSFFee = 10
        }

        //This enum defines the default flag values to determine the action to be perfomred.
        public enum FlagValues : short
        {
            EarlyPayOff = -1,
            Payment = 0,
            PrincipalOnly = 2,
            AdditionalPayment = 3,
            SkipPayment = 4,
            Discount = 5,
            MaintenanceFee = 6,
            ManagementFee = 7,
            NSFFee = 11,
            LateFee = 12
        }

        //This enum defines the Fee Frequency of management and maintenance fee.
        public enum FeeFrequency : int
        {
            Days = 0,
            AllPayments = 1,
            ScheduledPayments = 2,
            Months = 3
        }

        //This enum defines the Fee Basis of management and maintenance fee.
        public enum FeeBasis : int
        {
            BeginningPrincipal = 0,
            ActualPayment = 1,
            ScheduledPayment = 2
        }

        
        //This constant defines the default date used.
        public const string DefaultDate = "1/1/1900";

        //This constant defines the default values
        public const string Actual = "actual";
        public const string Thirty = "30";
        public const string Periodic = "periodic";
        public const string ServiceFeeInterest = "ServiceFeeInterest";
        public const string InterestAccrued = "InterestAccrued";
        public const string Principal = "principal";
        public const string Fixed = "fixed";
        public const int MinDurationValue = 0;
    }
}
