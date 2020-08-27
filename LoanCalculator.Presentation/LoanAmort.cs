using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;
using System.Windows.Forms;

namespace LoanAmort_driver
{
    #region properties
    public enum PaymentPeriod : short //value must be periods per year
    {   //New members for this enum must be handled in the getDaysPerPeriod method.
        Monthly = 12,
        SemiMonthly = 24,
        BiWeekly = 26,
        Weekly = 52,
        Daily = 360
    };

    public class InputRecord : IComparable<InputRecord>
    {
        public DateTime DateIn { get; set; }//First (earliest) date is the effective date. Flags and PaymentID are ignored for first record.
        public int Flags { get; set; }//When (Flags & 1) == 1, skip the payment.
        public int PaymentID { get; set; }

        #region For Rolling method only

        public DateTime EffectiveDate { get; set; }
        public string PaymentAmount { get; set; }
        public string InterestRate { get; set; }
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
        int IComparable<InputRecord>.CompareTo(InputRecord other)
        {
            return this.DateIn.CompareTo(other.DateIn);
        }
    }

    public class AdditionalPaymentRecord : IComparable<AdditionalPaymentRecord>
    {
        public DateTime DateIn { get; set; }
        public double AdditionalPayment { get; set; }
        public int PaymentID { get; set; }
        public bool PrincipalOnly { get; set; }

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

        int IComparable<AdditionalPaymentRecord>.CompareTo(AdditionalPaymentRecord other)
        {
            return this.DateIn.CompareTo(other.DateIn);
        }
    }

    // AD - 1.0.0.2 : Add CumulativePrincipal property 

    public class PaymentDetail : IComparable<PaymentDetail> //a List<> of these is returnded by getSchedule
    {
        public DateTime PaymentDate { get; set; }
        public double BeginningPrincipal { get; set; } //BeginningPrincipal, InterestPayment, CumulativeInterest, and PeriodicInterestRate are
        public double BeginningServiceFee { get; set; }
        public double PrincipalServiceFeePayment { get; set; }
        public double BeginningPrincipalServiceFee { get; set; }
        public double InterestServiceFeeInterestPayment { get; set; }
        //all set to zero for additional payments.
        public double PrincipalPayment { get; set; }
        public double InterestAccrued { get; set; }
        public double InterestDue { get; set; }
        public double InterestCarryOver { get; set; }
        public double InterestPayment { get; set; }
        public double InterestPaidPayment { get; set; }
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
        public double AccruedServiceFeeInterest { get; set; }
        public double serviceFeeInterestCarryOver { get; set; }
        public double ServiceFeeTotal { get; set; }
        public double OriginationFee { get; set; }
        public double SameDayFee { get; set; }
        public double ManagementFee { get; set; }
        public double ManagementFeeCarryOver { get; set; }
        public double MaintenanceFee { get; set; }
        public double MaintenanceFeeCarryOver { get; set; }
        public double CumulativeServiceFee { get; set; }
        public double CumulativeServiceFeeInterest { get; set; }
        public double CumulativeOriginationFee { get; set; }
        public double CumulativeSameDayFee { get; set; }
        public double CumulativeManagementFee { get; set; }
        public double CumulativeMaintenanceFee { get; set; }
        public double CumulativeTotalFees { get; set; }
        // AD 1.0.0.6-1 - add Beginning anc Cumulative values for ServiceFee
        public double CumulativeServiceFeeTotal { get; set; }

        #region For Rolling Method Only
        public double ServiceFeeInterestDue { get; set; }
        public double DailyInterestRate { get; set; }
        public double DailyInterestAmount { get; set; }
        public DateTime DueDate { get; set; }
        public double AccruedServiceFeeInterestCarryOver { get; set; }
        //Amount to be paid in the particular period columns
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
        public double InterestPaid { get; set; }
        public double PaymentDue { get; set; }
        #endregion

        int IComparable<PaymentDetail>.CompareTo(PaymentDetail other)
        {
            return this.PaymentDate.CompareTo(other.PaymentDate);
        }
    }

    public class getScheduleInput
    {
        public List<InputRecord> InputRecords;
        public List<AdditionalPaymentRecord> AdditionalPaymentRecords;

        public double LoanAmount;
        public double InterestRate; //as a percent
        public int InterestDelay;
        public double InterestRate2;
        public double InterestRate3;
        public double InterestRate4;
        public PaymentPeriod PmtPeriod;
        public DateTime EarlyPayoffDate;
        public int MinDuration; //0 indicates "don't prorate first payment. 
        //1 or more indicates the minimum number of days between the effective date and the first payment.
        //If the minimum number of days is not satisfied, skip the first payment.
        //Either way, prorate the first payment (which will be the second if the first is skipped) 
        //by the fraction of the payment period.
        public DateTime AccruedInterestDate;
        public DateTime AccruedServiceFeeInterestDate;
        // AD 1.0.0.6 - added properties for fees
        public double ServiceFee;
        public bool ServiceFeeFirstPayment;
        public bool IsServiceFeeIncremental;
        public int ApplyServiceFeeInterest;
        public double OriginationFee;
        public double SameDayFee;
        public double ManagementFee;
        public double MaintenanceFee;
        // AD 1.0.0.7 - added flag to recast additional payments
        public bool RecastAdditionalPayments;
        // AG 1.0.0.11 - added flag to support flexible calculatin method
        public bool UseFlexibleCalculation;
        // AG 1.0.0.20 - Number of days in a year
        public short DaysInYear;
        public bool IsInterestRounded { get; set; }
        public double PaymentAmount;
        public bool AfterPayment;
        public double EarnedInterest;
        public double EarnedServiceFeeInterest;
        public double Tier1;
        public double Tier2;
        public double Tier3;
        public double Tier4;
        public double EnforcedPrincipal;
        public int EnforcedPayment;
        #region Common for all
        public double BalloonPayment { get; set; }
        public string LoanType { get; set; }
        public string DaysInYearBankMethod { get; set; }
        public string DaysInMonth { get; set; }
        public double AmountFinanced { get; set; }
        public bool SameAsCashPayoff { get; set; }
        public double Residual { get; set; }


        #endregion

        #region Fee tab
        public bool IsServiceFeeFinanced { get; set; }
        public string SameDayFeeCalculationMethod { get; set; }
        public bool IsSameDayFeeFinanced { get; set; }
        public double OriginationFeePercent { get; set; }
        public double OriginationFeeMax { get; set; }
        public double OriginationFeeMin { get; set; }
        public bool OriginationFeeCalculationMethod { get; set; }
        public bool IsOriginationFeeFinanced { get; set; }

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
        public double EarnedOriginationFee { get; set; }
        public double EarnedSameDayFee { get; set; }
        public double EarnedManagementFee { get; set; }
        public double EarnedMaintenanceFee { get; set; }
        public double EarnedNSFFee { get; set; }
        public double EarnedLateFee { get; set; }

        #endregion

    }

    // AD : added AccruedPrincipal property

    public class getScheduleOutput
    {
        public List<PaymentDetail> Schedule;
        public double RegularPayment;
        public double RegularServiceFeePayment;
        public double AccruedInterest;
        public double AccruedPrincipal;
        public double AccruedServiceFeeInterest;

        #region For Bank Method only

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

    public class ManagementFeeTable
    {
        public DateTime AssessmentDate { get; set; }
        public double Fee { get; set; }
        public string PaymentId { get; set; }
    }
    public class MaintenanceFeeTable
    {
        public DateTime AssessmentDate { get; set; }
        public double Fee { get; set; }
        public string PaymentId { get; set; }
    }
    #endregion
    internal static class LoanAmort
    {
        /* 
         * The first date (earliest date) in InputRecords must be the effective date. Any AdditionalPayment in this record is ignored.
         * If the EarlyPayoffDate is the effective date or before, or if it's the last scheduled payment date or after, it is ignored.
         *      It is possible that a late effective date and/or additional payments could cause the loan to complete before the 
         *      requested payoff date. In this case the payoff date is ignored.
         *      If there is no EarlyPayoffDate, it can be set to the effective date or 1/1/1900.
         * All periods, except the first (which is from the effective date to the first payment) and last (but only in the case of an
         *      early payoff between scheduled payments), are treated as full payment periods, regardless of the dates. The first period
         *      is treated as a full payment period when MinDuration = 0
         * If MinDuration > 0, the first period is prorated.
         * If MinDuration > 0 and there are less than MinDuration days between the effective date and the first payment, the first payment
         *      is removed from the payment schedule.
         *      
         * Exceptions:
         *      "PmtPeriod out of range." - Must be a member of the PaymentPeriod enum. If members are added to this enum,
         *          add them in the getDaysPerPeriod method, where the error is generated.
         *      "AdditionalPayment cannot be negative."
         *      "Repeated date value in input list."
         *      "LoanAmount must be positive."
         *      "InterestRate must be positive."
         *      "Additional payment dates must be after the effective date and before the last payment date."
         *      "AccruedInterestDate must be on or after the effective date and on or before the last payment date."
         */

        // AD 1.0.0.18 - private class for additional payment period calculations
        private class additionalPaymentCalculatePeriod
        {
            public DateTime StartDate;
            public DateTime EndDate;
            public int DaysLength;
            public double BeginningPrincipal;
            public double InterestAmount;
        }

        /// <summary>
        /// This function is used to create schedules for the Rigid, Flexible and Prorate method.
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static getScheduleOutput getSchedule(getScheduleInput Input)
        {
            #region Initialize shiftedDeleteList of flag = 9.
            List<InputRecord> shiftedDeleteInputRecords = new List<InputRecord>();
            shiftedDeleteInputRecords = Input.InputRecords.Where(o => o.Flags == 9).Select(x => new InputRecord { DateIn = x.DateIn, Flags = x.Flags, PaymentID = x.PaymentID, EffectiveDate = x.EffectiveDate }).ToList();

            for (int i = 0; i <= Input.InputRecords.Count - 1; i++)
            {
                if (Input.InputRecords[i].Flags == 9)//This is used for shifted date
                {
                    Input.InputRecords.RemoveAt(i);
                    i--;
                }
            }
            Input.InputRecords.Sort();
            shiftedDeleteInputRecords.Sort();

            if (shiftedDeleteInputRecords.Count > 0 && shiftedDeleteInputRecords[0].DateIn <= Input.InputRecords[0].DateIn)
            {
                throw new ArgumentException("Shifted date should be greater than the effective date of the loan.");
            }
            #endregion

            // AD 1.0.0.10 - flag for switching regular payment rounding on/off
            // true - rounding is on, false - rounding is off
            var isRegularPaymentRounded = true;
            bool isInterestRounded = Input.IsInterestRounded;
            Input.AccruedInterestDate = Input.AccruedInterestDate.Date;
            if (Input.LoanAmount <= 0)
            {
                throw new System.ArgumentException("LoanAmount must be positive.");
            }

            if (Input.DaysInYear == default(short) || Input.DaysInYear < 0)
            {
                throw new ArgumentException("Number of days in a year were not provided or value is invalid");
            }
            Input.EarlyPayoffDate = Input.EarlyPayoffDate.Date;
            Input.InputRecords.Sort();

            for (int i = 0; i < Input.InputRecords.Count; i++)
            {
                Input.InputRecords[i].DateIn = Input.InputRecords[i].DateIn.Date;
                if (i > 0)
                {
                    if (Input.InputRecords[i].DateIn == Input.InputRecords[i - 1].DateIn)
                    {
                        throw new System.ArgumentException("Repeated date value in input list.");
                    }
                }
            }

            for (int i = 0; i < shiftedDeleteInputRecords.Count; i++)
            {
                shiftedDeleteInputRecords[i].DateIn = shiftedDeleteInputRecords[i].DateIn.Date;
                if (i > 0)
                {
                    if (shiftedDeleteInputRecords[i].DateIn == shiftedDeleteInputRecords[i - 1].DateIn)
                    {
                        throw new System.ArgumentException("Repeated shifted date value in input list.");
                    }
                }
            }

            // AD : if early payoff date is earlier than effective date it should be ignored
            DateTime EffectiveDate = Input.InputRecords[0].DateIn;
            bool IgnoreEarlyPayoffDate = Input.EarlyPayoffDate <= EffectiveDate;

            Input.AdditionalPaymentRecords = Input.AdditionalPaymentRecords.OrderBy(o => o.DateIn).ToList();
            for (int i = 0; i < Input.AdditionalPaymentRecords.Count; i++)
            {
                Input.AdditionalPaymentRecords[i].DateIn = Input.AdditionalPaymentRecords[i].DateIn.Date;
                if (Input.AdditionalPaymentRecords[i].AdditionalPayment <= 0)
                {
                    throw new System.ArgumentException("AdditionalPayment must be positive.");
                }

                // AD : if input records for payments do not exist or Early Payoff is before last payment date in input
                // consider Early Payoff date as last payment date
                // if Early Payoff Date is ignored, then do not use it

                DateTime lastPaymentDate = Input.InputRecords[Input.InputRecords.Count - 1].DateIn;
                if (!IgnoreEarlyPayoffDate && (Input.InputRecords.Count == 1 || DateAndTime.DateDiff(DateInterval.Day, lastPaymentDate, Input.EarlyPayoffDate, FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1) < 1))
                {
                    lastPaymentDate = Input.EarlyPayoffDate;
                }
                if (DateAndTime.DateDiff(DateInterval.Day, Input.AdditionalPaymentRecords[i].DateIn, Input.InputRecords[0].DateIn, FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1) > -1 ||
                    (DateAndTime.DateDiff(DateInterval.Day, Input.AdditionalPaymentRecords[i].DateIn, lastPaymentDate, FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1) < 1
                    && Input.InputRecords.FindIndex(o => o.Flags == 1) == -1 && Input.InputRecords.Any(o => o.DateIn == Input.AdditionalPaymentRecords[i].DateIn)))
                {
                    throw new System.ArgumentException("Additional payment dates must be after the effective date and before the last payment/early payoff date.");
                }

            }
            short DaysPerPeriod = 0;
            if (Input.UseFlexibleCalculation)
            {
                DaysPerPeriod = (short)(Input.InputRecords[1].DateIn - Input.InputRecords[0].DateIn).Days;
            }
            else
            {
                DaysPerPeriod = getDaysPerPeriod(Input.PmtPeriod);
            }
            //=================================
            double PeriodicInterestRate = Math.Abs(Convert.ToDouble(Input.InterestRate)) < 0.0001 ? 0 : 0.01 * Convert.ToDouble(Input.InterestRate) * DaysPerPeriod / Input.DaysInYear;
            if (!(Input.InterestRate2 == 0) && Input.Tier1 != 0 && Input.LoanAmount > Input.Tier1)
            {
                PeriodicInterestRate += 0.01 * Convert.ToDouble(Input.InterestRate2) * DaysPerPeriod / Input.DaysInYear;
            }

            if (!(Input.InterestRate3 == 0) && Input.Tier1 != 0 && Input.Tier2 != 0 && Input.LoanAmount > Input.Tier2)
            {
                PeriodicInterestRate += 0.01 * Convert.ToDouble(Input.InterestRate3) * DaysPerPeriod / Input.DaysInYear;
            }

            if (!(Input.InterestRate4 == 0) && Input.Tier1 != 0 && Input.Tier2 != 0 && Input.Tier3 != 0 && Input.LoanAmount > Input.Tier3)
            {
                PeriodicInterestRate += 0.01 * Convert.ToDouble(Input.InterestRate4) * DaysPerPeriod / Input.DaysInYear;
            }

            PeriodicInterestRate = Math.Round(PeriodicInterestRate, 15, MidpointRounding.AwayFromZero);

            // AD - 1.0.0.6 - all input object but not only input record participates in schedule composing 
            // AD - 1.0.0.10 - pass rounding flag to method
            // AG - 1.0.0.11 - Select calclation method: Plane or Flexible based on specific flag in Input:
            getScheduleOutput Output = Input.UseFlexibleCalculation
                ? getStartingSchedule(Input, PeriodicInterestRate, isRegularPaymentRounded)
                : getStartingSchedule(Input, Input.LoanAmount, PeriodicInterestRate, isRegularPaymentRounded);

            // AD - 1.0.0.10 - check if calculation was correct, if not (for large number of payments), disable regular payments rounding
            // (this cumulates big amount of rounding error for large number of payments)
            var isOutputCorrect = checkForCorrectOutput(Output, Input.LoanAmount);
            if (!isOutputCorrect && !Input.UseFlexibleCalculation)
            {
                isRegularPaymentRounded = false;
                Output = getStartingSchedule(Input, Input.LoanAmount, PeriodicInterestRate, isRegularPaymentRounded);
            }

            // It is used when min duration is greater then zero (0).
            if (Input.MinDuration > 0 && !Input.UseFlexibleCalculation)
            {
                adjustForOddFirstPeriod(Output, PeriodicInterestRate, DaysPerPeriod, IgnoreEarlyPayoffDate, isRegularPaymentRounded, Input);
            }

            // This is used for the additional payment
            adjustForAdditionalPayments(Output.Schedule, PeriodicInterestRate, isRegularPaymentRounded, Input);

            //AG - 1.0.0.15 - Move applyEarlyPayoffDate method call prior to adjustForSkippedPayments call
            applyEarlyPayoffDate(IgnoreEarlyPayoffDate, Output.Schedule, Input);

            #region adjustForSkippedPayments
            getScheduleOutput copiedOutput = new getScheduleOutput();
            copiedOutput.Schedule = new List<PaymentDetail>();
            for (int i = 0; i < Output.Schedule.Count; i++)
            {
                #region
                copiedOutput.Schedule.Add(new PaymentDetail
                {
                    PaymentDate = Output.Schedule[i].PaymentDate,
                    BeginningPrincipal = Output.Schedule[i].BeginningPrincipal,
                    BeginningServiceFee = Output.Schedule[i].BeginningServiceFee,
                    PrincipalPayment = Output.Schedule[i].PrincipalPayment,
                    InterestPayment = Output.Schedule[i].InterestPayment,
                    InterestAccrued = Output.Schedule[i].InterestAccrued,
                    InterestDue = Output.Schedule[i].InterestDue,
                    InterestCarryOver = Output.Schedule[i].InterestCarryOver,
                    TotalPayment = Output.Schedule[i].TotalPayment,
                    CumulativePayment = Output.Schedule[i].CumulativePayment,
                    CumulativeInterest = Output.Schedule[i].CumulativeInterest,
                    CumulativePrincipal = Output.Schedule[i].CumulativePrincipal,
                    PeriodicInterestRate = Output.Schedule[i].PeriodicInterestRate,
                    PaymentID = Output.Schedule[i].PaymentID,
                    Flags = Output.Schedule[i].Flags,

                    // AD - 1.0.0.6 - added columns for Service Fee values
                    ServiceFee = Output.Schedule[i].ServiceFee,
                    serviceFeeInterestCarryOver = Output.Schedule[i].serviceFeeInterestCarryOver,
                    AccruedServiceFeeInterest = Output.Schedule[i].AccruedServiceFeeInterest,
                    ServiceFeeInterest = Output.Schedule[i].ServiceFeeInterest,
                    ServiceFeeTotal = Output.Schedule[i].ServiceFeeTotal,
                    OriginationFee = Output.Schedule[i].OriginationFee,
                    SameDayFee = Output.Schedule[i].SameDayFee,
                    MaintenanceFee = Output.Schedule[i].MaintenanceFee,
                    MaintenanceFeeCarryOver = Output.Schedule[i].MaintenanceFeeCarryOver,
                    ManagementFee = Output.Schedule[i].ManagementFee,
                    ManagementFeeCarryOver = Output.Schedule[i].ManagementFeeCarryOver,

                    CumulativeServiceFee = Output.Schedule[i].CumulativeServiceFee,
                    CumulativeServiceFeeInterest = Output.Schedule[i].CumulativeServiceFeeInterest,
                    CumulativeOriginationFee = Output.Schedule[i].CumulativeOriginationFee,
                    CumulativeSameDayFee = Output.Schedule[i].CumulativeSameDayFee,
                    CumulativeMaintenanceFee = Output.Schedule[i].CumulativeMaintenanceFee,
                    CumulativeManagementFee = Output.Schedule[i].CumulativeManagementFee,
                    CumulativeTotalFees = Output.Schedule[i].CumulativeTotalFees

                });
                #endregion
            }
            // AD - 1.0.0.6 - apply service fees to total
            applyServiceFeesToTotalPayments(copiedOutput);

            applyServiceFeesToTotalPayments(Output);

            AdjustForSkippeedPayments(Input, Output.Schedule, DaysPerPeriod, Output, shiftedDeleteInputRecords);

            #endregion

            //AD - 1.0.0.9 - round payments so that to get precise cumulative values of total payments in the end
            roundPaymentValues(Output, isInterestRounded, Input);
            // AD - 1.0.0.6 - adjustPrincipalPayments - this method makes last Principal Payment incorrect, removed method
            // AD - 1.0.0.6 - apply service fees to total
            applyServiceFeesToTotalPayments(Output);
            // AD - 1.0.0.5 - recalculate cumulative values

            recalculateCumulativeValues(Output);
            // //AD - 1.0.0.2 : Add count of accrued principal propertywith a separate method
            // //AD - 1.0.0.2 : Add count of accrued principal propertywith a separate method


            if (DateAndTime.DateDiff(DateInterval.Day, Input.AccruedInterestDate, EffectiveDate, FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1) >= 0)
            {
                Output.AccruedInterest = 0;
                Output.AccruedPrincipal = 0;
            }
            else
            {
                Output.AccruedInterest = GetAccruedInterest(copiedOutput.Schedule, Input, EffectiveDate);
                Output.AccruedPrincipal = GetAccruedPrincipal(copiedOutput.Schedule, Input, EffectiveDate);
            }

            if (DateAndTime.DateDiff(DateInterval.Day, Input.AccruedServiceFeeInterestDate, EffectiveDate, FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1) >= 0)
            {

                Output.AccruedServiceFeeInterest = 0;
            }
            else
            {
                Output.AccruedServiceFeeInterest = GetAccruedServiceFeeInterest(copiedOutput.Schedule, Input, EffectiveDate);
            }
            return Output;
        }

        /// <summary>
        ///  This function is used to calculate Interest accrued amount to show in output tab when provide interest date.
        /// </summary>
        /// <param name="PaymentDetailIn"></param>
        /// <param name="Input"></param>
        /// <param name="EffectiveDate"></param>
        /// <returns></returns>
        private static double GetAccruedInterest(List<PaymentDetail> PaymentDetailIn, getScheduleInput Input, DateTime EffectiveDate)
        {
            double ReturnVal = GetAccruedValue(PaymentDetailIn, Input, EffectiveDate, "interest");

            return ReturnVal;
        }
        /// <summary>
        /// This function is used to calculate Principal amount to show in output tab when provide interest date.
        /// </summary>
        /// <param name="PaymentDetailIn"></param>
        /// <param name="Input"></param>
        /// <param name="EffectiveDate"></param>
        /// <returns></returns>
        private static double GetAccruedPrincipal(List<PaymentDetail> PaymentDetailIn, getScheduleInput Input, DateTime EffectiveDate)
        {
            double ReturnVal = GetAccruedValue(PaymentDetailIn, Input, EffectiveDate, "principal");

            return ReturnVal;
        }

        /// <summary>
        /// This function is used to calculate service Fee interest amount to show in output tab when provide service fee interest date.
        /// </summary>
        /// <param name="PaymentDetailIn"></param>
        /// <param name="Input"></param>
        /// <param name="EffectiveDate"></param>
        /// <returns></returns>
        private static double GetAccruedServiceFeeInterest(List<PaymentDetail> PaymentDetailIn, getScheduleInput Input, DateTime EffectiveDate)
        {
            double ReturnVal = GetAccruedValue(PaymentDetailIn, Input, EffectiveDate, "ServiceFeeInterest");

            return ReturnVal;
        }

        /// <summary>
        ///  This function is used to calculate Interest accrued, Principal and service Fee interest amount to show in output tab when provide interest accrued date and service fee interest date.
        /// </summary>
        /// <param name="PaymentDetailIn"></param>
        /// <param name="Input"></param>
        /// <param name="EffectiveDate"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        private static double GetAccruedValue(List<PaymentDetail> PaymentDetailIn, getScheduleInput Input, DateTime EffectiveDate, string dataType)
        {
            bool isMinDuration = Input.MinDuration > 0 ? true : false;
            int flag = -2;
            double ReturnVal = 0;
            Boolean DoPartialPeriod = false;
            DateTime DateFrom = EffectiveDate;
            DateTime EffecticeDelayDate = ((dataType == "interest" || dataType == "ServiceFeeInterest") && (Input.MinDuration > 0 || Input.UseFlexibleCalculation)) ? EffectiveDate.AddDays(Input.InterestDelay) : EffectiveDate;
            EffecticeDelayDate = (EffecticeDelayDate > Input.InputRecords[1].DateIn && Input.MinDuration > 0) ? Input.InputRecords[1].DateIn : EffecticeDelayDate;
            long DayCount = 0;
            int PaymentDetailIndex = 0;
            long PeriodDayCount = 0;
            DateTime DateTo = Convert.ToDateTime("1/1/1900");
            int DaysPerPeriod = getDaysPerPeriod(Input.PmtPeriod);
            double PreviousAccumulated = 0;
            double NextAccumulated = 0;
            DateTime AccruedInterestDate = (dataType == "interest" || dataType == "principal") ? Input.AccruedInterestDate : Input.AccruedServiceFeeInterestDate;

            if (AccruedInterestDate != EffectiveDate)
            {
                for (int i = 0; i < PaymentDetailIn.Count; i++)
                {
                    if (PaymentDetailIn[i].PaymentDate == AccruedInterestDate)
                    {
                        switch (dataType)
                        {
                            case "interest":
                                ReturnVal += rounndedValue(PaymentDetailIn[i].InterestAccrued);
                                break;
                            case "principal":
                                ReturnVal += PaymentDetailIn[i].PrincipalPayment;
                                break;
                            case "ServiceFeeInterest":
                                ReturnVal += rounndedValue(PaymentDetailIn[i].AccruedServiceFeeInterest);
                                break;
                        }
                        if (i < PaymentDetailIn.Count - 1 && PaymentDetailIn[i + 1].PaymentDate > AccruedInterestDate)
                        {
                            break;
                        }
                    }
                    else if (PaymentDetailIn[i].PaymentDate < AccruedInterestDate)
                    {
                        switch (dataType)
                        {
                            case "interest":
                                PreviousAccumulated += rounndedValue(PaymentDetailIn[i].InterestAccrued);
                                break;
                            case "principal":
                                PreviousAccumulated += PaymentDetailIn[i].PrincipalPayment;
                                break;
                            case "ServiceFeeInterest":
                                PreviousAccumulated += rounndedValue(PaymentDetailIn[i].AccruedServiceFeeInterest);
                                break;
                        }
                        DateFrom = PaymentDetailIn[i].PaymentDate;
                    }
                    else if (PaymentDetailIn[i].PaymentDate > AccruedInterestDate)
                    {
                        switch (dataType)
                        {
                            case "interest":
                                NextAccumulated += (i == 0) ? rounndedValue(PaymentDetailIn[i].InterestAccrued - Input.EarnedInterest) : rounndedValue(PaymentDetailIn[i].InterestAccrued);
                                break;
                            case "principal":
                                NextAccumulated += PaymentDetailIn[i].PrincipalPayment;
                                break;
                            case "ServiceFeeInterest":
                                NextAccumulated += (i == 0) ? rounndedValue(PaymentDetailIn[i].AccruedServiceFeeInterest - Input.EarnedServiceFeeInterest) : rounndedValue(PaymentDetailIn[i].AccruedServiceFeeInterest);
                                break;
                        }

                        DoPartialPeriod = true;
                        DateTo = PaymentDetailIn[i].PaymentDate;
                        PaymentDetailIndex = i;
                        flag = PaymentDetailIn[i].Flags;
                        break;
                    }
                }
                ReturnVal = ReturnVal + PreviousAccumulated + NextAccumulated;
                if (DoPartialPeriod)
                {
                    DateFrom = (Input.MinDuration > 0 || Input.UseFlexibleCalculation) ? ((DateFrom >= EffecticeDelayDate) ? DateFrom : EffecticeDelayDate) : DateFrom;
                    DayCount = DateFrom >= AccruedInterestDate ? 0 : DateAndTime.DateDiff(DateInterval.Day, DateFrom, AccruedInterestDate, FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1);
                    PeriodDayCount = CalculatDifference(PaymentDetailIn, DateTo, PaymentDetailIndex, Input, isMinDuration, DaysPerPeriod, flag, dataType);
                    DayCount = DayCount > PeriodDayCount ? PeriodDayCount : DayCount;
                    ReturnVal = ((dataType == "interest" || dataType == "ServiceFeeInterest") && DateFrom >= DateTo) ? 0 : PreviousAccumulated + ((NextAccumulated) * DayCount / PeriodDayCount);
                }
            }
            if (dataType == "interest" && AccruedInterestDate < PaymentDetailIn[0].PaymentDate)
            {
                return ReturnVal + Input.EarnedInterest;
            }
            else if (dataType == "ServiceFeeInterest" && AccruedInterestDate < PaymentDetailIn[0].PaymentDate)
            {
                return ReturnVal + Input.EarnedServiceFeeInterest;
            }
            else
            {
                return ReturnVal;
            }
        }
        public static int CalculatDifference(List<PaymentDetail> PaymentDetailIn, DateTime endDate, int PaymentDetailIndex, getScheduleInput Input, bool isMinDuration, int DaysPeriod, int flag, string dataType)
        {
            DateTime startDateWithDelay = ((Input.MinDuration > 0 || Input.UseFlexibleCalculation) && dataType != "principal") ? Input.InputRecords[0].DateIn.AddDays(Input.InterestDelay) : Input.InputRecords[0].DateIn;
            DateTime startDate;
            startDate = startDateWithDelay >= (PaymentDetailIndex == 0 ? Input.InputRecords[0].DateIn : PaymentDetailIn[PaymentDetailIndex - 1].PaymentDate) ? startDateWithDelay : (PaymentDetailIndex == 0 ? Input.InputRecords[0].DateIn : PaymentDetailIn[PaymentDetailIndex - 1].PaymentDate);
            int scheduleDayDifference = (startDate <= endDate) ? (endDate - startDate).Days : 0;
            if (!Input.UseFlexibleCalculation && (!isMinDuration || (isMinDuration && PaymentDetailIn.FindLastIndex(o => o.PaymentDate == Input.InputRecords[1].DateIn && (o.Flags == 0 || o.Flags == 1)) < PaymentDetailIndex)))
            {
                #region
                if (PaymentDetailIndex != 0)
                {
                    #region
                    if (PaymentDetailIn[PaymentDetailIndex - 1].Flags == 0 || PaymentDetailIn[PaymentDetailIndex - 1].Flags == 1)
                    {
                        scheduleDayDifference = (endDate - PaymentDetailIn[PaymentDetailIndex - 1].PaymentDate).Days;

                    }
                    else
                    {
                        int index = Input.InputRecords.FindLastIndex(o => o.DateIn < PaymentDetailIn[PaymentDetailIndex - 1].PaymentDate && (o.Flags == 0 || o.Flags == 1));
                        int Difference = (PaymentDetailIn[PaymentDetailIndex - 1].PaymentDate - Input.InputRecords[index].DateIn).Days;
                        DaysPeriod = DaysPeriod > Difference ? DaysPeriod - Difference : 0;
                    }
                    if ((flag == 0 || flag == 1) || ((flag == -1) && (Input.InputRecords.FindIndex(o => o.DateIn == endDate && (o.Flags == 0 || o.Flags == 1)) != -1)))
                    {
                        scheduleDayDifference = DaysPeriod;
                    }
                    #endregion
                }

                scheduleDayDifference = (PaymentDetailIndex == 0 && (flag == 0 || flag == 1)) ? (DaysPeriod) : (scheduleDayDifference <= DaysPeriod ? scheduleDayDifference : DaysPeriod);
                #endregion
            }
            return scheduleDayDifference;
        }

        /// <summary>
        /// This function is used to perform delete functionality for Rigid,Flexible and Prorate methods.
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="PaymentDetailIn"></param>
        /// <param name="DaysPerPeriod"></param>
        /// <param name="Output"></param>
        private static void AdjustForSkippeedPayments(getScheduleInput Input, List<PaymentDetail> PaymentDetailIn, short DaysPerPeriod, getScheduleOutput Output, List<InputRecord> shiftedDeleteInputRecords)
        {
            List<PaymentDetail> deletedPaymentDetail = new List<PaymentDetail>();
            double currentBegningPrincipal = Input.LoanAmount;
            double actualBegningPrincipal = Input.LoanAmount;
            double serviceFeeIncremental = 0;

            double currentBegningServiceFee = getStartingServiceFee(Input.ServiceFee, Input.LoanAmount, Input.ApplyServiceFeeInterest);
            double actualBegningServiceFee = currentBegningServiceFee;
            try
            {
                if (Input.InputRecords.FindLastIndex(o => o.Flags == 1) < 1)
                {
                    return;
                }
                int addPayCountForSameDate = Input.AdditionalPaymentRecords.Count(o => o.DateIn == Input.EarlyPayoffDate);
                int paymentdetailInCountForSameDate = PaymentDetailIn.Count(o => o.PaymentDate == Input.EarlyPayoffDate);
                int inputCountForSameDate = Input.InputRecords.FindIndex(o => o.DateIn == Input.EarlyPayoffDate);

                if (inputCountForSameDate > 0 && paymentdetailInCountForSameDate > 0 &&
                    ((addPayCountForSameDate > 0 && paymentdetailInCountForSameDate > addPayCountForSameDate) || (addPayCountForSameDate == 0)))
                {
                    PaymentDetailIn[PaymentDetailIn.FindLastIndex(o => o.PaymentDate == Input.EarlyPayoffDate)].PaymentID = Input.InputRecords[inputCountForSameDate].PaymentID;

                    PaymentDetailIn[PaymentDetailIn.FindLastIndex(o => o.PaymentDate == Input.EarlyPayoffDate)].Flags = Input.InputRecords[inputCountForSameDate].Flags;

                    deletedPaymentDetail = PaymentDetailIn.Where(o => o.Flags == 1).ToList();
                }
                else
                {
                    deletedPaymentDetail = PaymentDetailIn.Where(o => o.Flags == 1 && o.PaymentDate != Input.EarlyPayoffDate).ToList();
                }

                deletedPaymentDetail.Sort();
                DateTime lastDate = PaymentDetailIn[PaymentDetailIn.Count - 1].PaymentDate;
                bool lastRowDeleted = (PaymentDetailIn[PaymentDetailIn.Count - 1].Flags == 1 && shiftedDeleteInputRecords.Count <= 0) ? true : false;
                int additionalPyamentIndex = 0;

                //This loop is used to delete the shifted date when shifted date comes in between schedule date and not used.
                for (int i = 0; i < shiftedDeleteInputRecords.Count; i++)
                {
                    #region
                    if (shiftedDeleteInputRecords[i].DateIn < Input.InputRecords[Input.InputRecords.FindIndex(o => o.Flags == 1)].DateIn)
                    {
                        shiftedDeleteInputRecords.RemoveAt(i);
                        i--;
                    }
                    #endregion
                }

                for (int outputIndex = 0; outputIndex < PaymentDetailIn.Count; outputIndex++)
                {
                    #region This is used when we delete the schedule and reschedule the schedule 

                    int indexForShifted = deletedPaymentDetail.FindIndex(m => m.TotalPayment > 0) == -1 ? -1 : shiftedDeleteInputRecords.FindIndex(o => o.DateIn < PaymentDetailIn[outputIndex].PaymentDate && o.DateIn >= deletedPaymentDetail[deletedPaymentDetail.FindIndex(m => m.TotalPayment > 0)].PaymentDate);

                    if (deletedPaymentDetail.Sum(o => o.TotalPayment) > 0 && shiftedDeleteInputRecords.Count > 0 && indexForShifted != -1)
                    {
                        #region When shifted datecomes in between the schedule date
                        int deletedIndex = deletedPaymentDetail.FindIndex(o => o.TotalPayment > 0);
                        for (int i = deletedIndex; i < deletedPaymentDetail.Count && deletedPaymentDetail[i].PaymentDate <= shiftedDeleteInputRecords[indexForShifted].DateIn; i++)
                        {
                            if (i == deletedIndex)
                            {
                                PaymentDetailIn.Insert(outputIndex, new PaymentDetail
                                {
                                    #region
                                    PaymentDate = shiftedDeleteInputRecords[indexForShifted].DateIn,
                                    DueDate = shiftedDeleteInputRecords[indexForShifted].EffectiveDate,
                                    BeginningPrincipal = currentBegningPrincipal,
                                    BeginningServiceFee = currentBegningServiceFee,
                                    PrincipalPayment = deletedPaymentDetail[i].PrincipalPayment,
                                    InterestPayment = deletedPaymentDetail[i].InterestPayment,
                                    InterestAccrued = 0,
                                    InterestDue = deletedPaymentDetail[i].InterestDue,
                                    InterestCarryOver = 0,
                                    TotalPayment = deletedPaymentDetail[i].TotalPayment,
                                    CumulativePayment = 0,
                                    CumulativeInterest = 0,
                                    PeriodicInterestRate = (Input.InputRecords[Input.InputRecords.FindLastIndex(o => o.Flags == 0 || o.Flags == 1)].DateIn >= shiftedDeleteInputRecords[indexForShifted].DateIn) ? deletedPaymentDetail[i].PeriodicInterestRate : 0,
                                    PaymentID = shiftedDeleteInputRecords[indexForShifted].PaymentID,
                                    Flags = shiftedDeleteInputRecords[indexForShifted].Flags,

                                    // AD - 1.0.0.6 - added columns for Service Fee values
                                    ServiceFee = deletedPaymentDetail[i].ServiceFee,
                                    AccruedServiceFeeInterest = 0,
                                    ServiceFeeInterest = deletedPaymentDetail[i].ServiceFeeInterest,
                                    serviceFeeInterestCarryOver = 0,
                                    ServiceFeeTotal = deletedPaymentDetail[i].ServiceFeeTotal,
                                    OriginationFee = deletedPaymentDetail[i].OriginationFee,
                                    SameDayFee = deletedPaymentDetail[i].SameDayFee,
                                    MaintenanceFee = deletedPaymentDetail[i].MaintenanceFee,
                                    ManagementFee = deletedPaymentDetail[i].ManagementFee,

                                    CumulativeServiceFee = 0,
                                    CumulativeServiceFeeInterest = 0,
                                    CumulativeOriginationFee = 0,
                                    CumulativeSameDayFee = 0,
                                    CumulativeMaintenanceFee = 0,
                                    CumulativeManagementFee = 0,
                                    CumulativeTotalFees = 0
                                    #endregion
                                });
                            }
                            else
                            {
                                #region
                                PaymentDetailIn[outputIndex].PrincipalPayment += deletedPaymentDetail[i].PrincipalPayment;
                                PaymentDetailIn[outputIndex].InterestPayment += deletedPaymentDetail[i].InterestPayment;
                                PaymentDetailIn[outputIndex].InterestCarryOver = 0;
                                PaymentDetailIn[outputIndex].InterestDue += deletedPaymentDetail[i].InterestPayment;
                                PaymentDetailIn[outputIndex].TotalPayment += deletedPaymentDetail[i].TotalPayment;
                                PaymentDetailIn[outputIndex].ServiceFee += deletedPaymentDetail[i].ServiceFee;
                                PaymentDetailIn[outputIndex].ServiceFeeInterest += deletedPaymentDetail[i].ServiceFeeInterest;
                                PaymentDetailIn[outputIndex].serviceFeeInterestCarryOver = 0;
                                PaymentDetailIn[outputIndex].ServiceFeeTotal += deletedPaymentDetail[i].ServiceFeeTotal;
                                PaymentDetailIn[outputIndex].OriginationFee += deletedPaymentDetail[i].OriginationFee;
                                PaymentDetailIn[outputIndex].SameDayFee += deletedPaymentDetail[i].SameDayFee;
                                PaymentDetailIn[outputIndex].MaintenanceFee += deletedPaymentDetail[i].MaintenanceFee;
                                PaymentDetailIn[outputIndex].ManagementFee += deletedPaymentDetail[i].ManagementFee;
                                #endregion
                            }
                            if (outputIndex != PaymentDetailIn.Count - 1)
                            {
                                currentBegningPrincipal -= deletedPaymentDetail[i].PrincipalPayment;
                                currentBegningServiceFee -= deletedPaymentDetail[i].ServiceFee;
                            }
                            deletedPaymentDetail[i].TotalPayment = 0;
                            if (shiftedDeleteInputRecords.Count > 1)
                            {
                                break;
                            }
                        }

                        //shiftedDeleteInputRecords.RemoveAt(0);
                        shiftedDeleteInputRecords.RemoveAt(indexForShifted);
                        #endregion
                    }
                    else if (PaymentDetailIn[outputIndex].Flags == 1)
                    {
                        #region This is used to delete a schedule when flag value is set to 1 and Early PayOff date is not equal to the actual schedule date
                        if (PaymentDetailIn[PaymentDetailIn.Count - 1].Flags == 1)
                        {
                            lastDate = PaymentDetailIn[outputIndex].PaymentDate;
                            lastRowDeleted = shiftedDeleteInputRecords.FindIndex(o => o.DateIn > PaymentDetailIn[PaymentDetailIn.Count - 1].PaymentDate) == -1 ? true : false;
                        }

                        actualBegningPrincipal -= PaymentDetailIn[outputIndex].PrincipalPayment;
                        actualBegningServiceFee -= PaymentDetailIn[outputIndex].ServiceFee;
                        PaymentDetailIn.RemoveAt(outputIndex);
                        outputIndex--;
                        #endregion
                    }
                    else if (Input.AdditionalPaymentRecords.Count != 0 && additionalPyamentIndex <= Input.AdditionalPaymentRecords.Count - 1 && Input.AdditionalPaymentRecords[additionalPyamentIndex].DateIn == PaymentDetailIn[outputIndex].PaymentDate && PaymentDetailIn[outputIndex].Flags == 2)// && deletedPaymentDetail.Count(o => o.PaymentDate < PaymentDetailIn[outputIndex].PaymentDate && o.TotalPayment > 0) > 0)
                    {
                        #region This is used when we do an additional payment with "PrincipalOnly" and "Not PrincipalOnly"
                        if (Input.AdditionalPaymentRecords[additionalPyamentIndex].DateIn == PaymentDetailIn[outputIndex].PaymentDate)
                        {
                            PaymentDetailIn[outputIndex].PaymentID = Input.AdditionalPaymentRecords[additionalPyamentIndex].PaymentID;
                            PaymentDetailIn[outputIndex].Flags = 2;
                        }
                        if (deletedPaymentDetail.Count(o => o.PaymentDate < PaymentDetailIn[outputIndex].PaymentDate && o.TotalPayment > 0) > 0)
                        {
                            NotPrincipalOnlyWithSkippedPayment(PaymentDetailIn, outputIndex, Input, additionalPyamentIndex, currentBegningPrincipal, currentBegningServiceFee, deletedPaymentDetail, Output, ref actualBegningPrincipal, ref actualBegningServiceFee);
                        }
                        else
                        {
                            PaymentDetailIn[outputIndex].BeginningPrincipal = currentBegningPrincipal;
                            PaymentDetailIn[outputIndex].BeginningServiceFee = currentBegningServiceFee;
                            PaymentDetailIn[outputIndex].CumulativeInterest = outputIndex == 0 ? PaymentDetailIn[outputIndex].InterestAccrued : PaymentDetailIn[outputIndex - 1].CumulativeInterest + PaymentDetailIn[outputIndex].InterestAccrued;
                            PaymentDetailIn[outputIndex].CumulativePrincipal = outputIndex == 0 ? PaymentDetailIn[outputIndex].PrincipalPayment : PaymentDetailIn[outputIndex - 1].CumulativePrincipal + PaymentDetailIn[outputIndex].PrincipalPayment;
                            PaymentDetailIn[outputIndex].CumulativeServiceFee = outputIndex == 0 ? PaymentDetailIn[outputIndex].ServiceFee : PaymentDetailIn[outputIndex - 1].CumulativeServiceFee + PaymentDetailIn[outputIndex].ServiceFee;
                            PaymentDetailIn[outputIndex].CumulativeServiceFeeInterest = outputIndex == 0 ? PaymentDetailIn[outputIndex].ServiceFeeInterest : PaymentDetailIn[outputIndex - 1].CumulativeServiceFeeInterest + PaymentDetailIn[outputIndex].ServiceFeeInterest;
                            PaymentDetailIn[outputIndex].CumulativeOriginationFee = outputIndex == 0 ? PaymentDetailIn[outputIndex].OriginationFee : PaymentDetailIn[outputIndex - 1].CumulativeOriginationFee + PaymentDetailIn[outputIndex].OriginationFee;
                            PaymentDetailIn[outputIndex].CumulativeSameDayFee = outputIndex == 0 ? PaymentDetailIn[outputIndex].SameDayFee : PaymentDetailIn[outputIndex - 1].CumulativeSameDayFee + PaymentDetailIn[outputIndex].SameDayFee;
                            PaymentDetailIn[outputIndex].CumulativeManagementFee = outputIndex == 0 ? PaymentDetailIn[outputIndex].ManagementFee : PaymentDetailIn[outputIndex - 1].CumulativeManagementFee + PaymentDetailIn[outputIndex].ManagementFee;
                            PaymentDetailIn[outputIndex].CumulativeMaintenanceFee = outputIndex == 0 ? PaymentDetailIn[outputIndex].MaintenanceFee : PaymentDetailIn[outputIndex - 1].CumulativeMaintenanceFee + PaymentDetailIn[outputIndex].MaintenanceFee;
                            PaymentDetailIn[outputIndex].CumulativePayment = outputIndex == 0 ? PaymentDetailIn[outputIndex].TotalPayment : PaymentDetailIn[outputIndex - 1].CumulativePayment + PaymentDetailIn[outputIndex].TotalPayment;
                            PaymentDetailIn[outputIndex].CumulativeTotalFees = PaymentDetailIn[outputIndex].CumulativeServiceFee + PaymentDetailIn[outputIndex].CumulativeServiceFeeInterest +
                                                                              PaymentDetailIn[outputIndex].CumulativeOriginationFee + PaymentDetailIn[outputIndex].CumulativeSameDayFee +
                                                                              PaymentDetailIn[outputIndex].CumulativeManagementFee + PaymentDetailIn[outputIndex].CumulativeMaintenanceFee;

                            if (outputIndex != PaymentDetailIn.Count - 1)
                            {
                                actualBegningPrincipal -= PaymentDetailIn[outputIndex].PrincipalPayment;
                                actualBegningServiceFee -= PaymentDetailIn[outputIndex].ServiceFee;
                                currentBegningPrincipal -= PaymentDetailIn[outputIndex].PrincipalPayment;
                                currentBegningServiceFee -= PaymentDetailIn[outputIndex].ServiceFee;
                            }
                        }
                        additionalPyamentIndex++;
                        #endregion
                    }
                    else
                    {
                        #region This is used when flag value is not set to 1
                        PaymentDetailIn[outputIndex].BeginningPrincipal = currentBegningPrincipal;
                        PaymentDetailIn[outputIndex].BeginningServiceFee = currentBegningServiceFee;
                        PaymentDetailIn[outputIndex].CumulativeInterest = outputIndex == 0 ? PaymentDetailIn[outputIndex].InterestAccrued : PaymentDetailIn[outputIndex - 1].CumulativeInterest + PaymentDetailIn[outputIndex].InterestAccrued;
                        PaymentDetailIn[outputIndex].CumulativePrincipal = outputIndex == 0 ? PaymentDetailIn[outputIndex].PrincipalPayment : PaymentDetailIn[outputIndex - 1].CumulativePrincipal + PaymentDetailIn[outputIndex].PrincipalPayment;
                        PaymentDetailIn[outputIndex].CumulativeServiceFee = outputIndex == 0 ? PaymentDetailIn[outputIndex].ServiceFee : PaymentDetailIn[outputIndex - 1].CumulativeServiceFee + PaymentDetailIn[outputIndex].ServiceFee;
                        PaymentDetailIn[outputIndex].CumulativeServiceFeeInterest = outputIndex == 0 ? PaymentDetailIn[outputIndex].ServiceFeeInterest : PaymentDetailIn[outputIndex - 1].CumulativeServiceFeeInterest + PaymentDetailIn[outputIndex].ServiceFeeInterest;
                        PaymentDetailIn[outputIndex].CumulativeOriginationFee = outputIndex == 0 ? PaymentDetailIn[outputIndex].OriginationFee : PaymentDetailIn[outputIndex - 1].CumulativeOriginationFee + PaymentDetailIn[outputIndex].OriginationFee;
                        PaymentDetailIn[outputIndex].CumulativeSameDayFee = outputIndex == 0 ? PaymentDetailIn[outputIndex].SameDayFee : PaymentDetailIn[outputIndex - 1].CumulativeSameDayFee + PaymentDetailIn[outputIndex].SameDayFee;
                        PaymentDetailIn[outputIndex].CumulativeManagementFee = outputIndex == 0 ? PaymentDetailIn[outputIndex].ManagementFee : PaymentDetailIn[outputIndex - 1].CumulativeManagementFee + PaymentDetailIn[outputIndex].ManagementFee;
                        PaymentDetailIn[outputIndex].CumulativeMaintenanceFee = outputIndex == 0 ? PaymentDetailIn[outputIndex].MaintenanceFee : PaymentDetailIn[outputIndex - 1].CumulativeMaintenanceFee + PaymentDetailIn[outputIndex].MaintenanceFee;
                        PaymentDetailIn[outputIndex].CumulativePayment = outputIndex == 0 ? PaymentDetailIn[outputIndex].TotalPayment : PaymentDetailIn[outputIndex - 1].CumulativePayment + PaymentDetailIn[outputIndex].TotalPayment;
                        PaymentDetailIn[outputIndex].CumulativeTotalFees = PaymentDetailIn[outputIndex].CumulativeServiceFee + PaymentDetailIn[outputIndex].CumulativeServiceFeeInterest +
                                                                          PaymentDetailIn[outputIndex].CumulativeOriginationFee + PaymentDetailIn[outputIndex].CumulativeSameDayFee +
                                                                          PaymentDetailIn[outputIndex].CumulativeManagementFee + PaymentDetailIn[outputIndex].CumulativeMaintenanceFee;
                        if (outputIndex != PaymentDetailIn.Count - 1)
                        {
                            actualBegningPrincipal -= PaymentDetailIn[outputIndex].PrincipalPayment;
                            actualBegningServiceFee -= PaymentDetailIn[outputIndex].ServiceFee;
                            currentBegningPrincipal -= PaymentDetailIn[outputIndex].PrincipalPayment;
                            currentBegningServiceFee -= PaymentDetailIn[outputIndex].ServiceFee;
                        }
                        if (PaymentDetailIn[outputIndex].Flags == 2)
                        {
                            additionalPyamentIndex++;
                        }
                        #endregion
                    }
                    #endregion
                }
                currentBegningPrincipal = PaymentDetailIn.Count == 1 ? Input.LoanAmount : currentBegningPrincipal;
                currentBegningServiceFee = PaymentDetailIn.Count == 1 ? getStartingServiceFee(Input.ServiceFee, Input.LoanAmount, Input.ApplyServiceFeeInterest) : currentBegningServiceFee;

                //This loop is used to delete the shifted date when shifted date comes in between schedule date and not used.
                for (int i = 0; i < shiftedDeleteInputRecords.Count; i++)
                {
                    #region
                    if (shiftedDeleteInputRecords[i].DateIn < Input.InputRecords[Input.InputRecords.FindIndex(o => o.Flags == 1)].DateIn)
                    {
                        shiftedDeleteInputRecords.RemoveAt(i);
                        i--;
                    }
                    #endregion
                }
                //This is used when we add the deleted schedule into the last schedule or new shifted schedule date
                for (int i = 0; i < deletedPaymentDetail.Count; i++)
                {
                    #region 
                    if (deletedPaymentDetail[i].TotalPayment <= 0)// This is used when particular deleted schedule have zero amount
                    {
                        continue;
                    }

                    int indexForLastShifted = shiftedDeleteInputRecords.FindIndex(o => o.DateIn >= deletedPaymentDetail[i].PaymentDate);
                    if (shiftedDeleteInputRecords.Count > 0 && indexForLastShifted != -1)
                    {
                        #region This is used when shifted schedule are available and then add the deleted schedule into the shifted schedule date

                        int insertIndex = PaymentDetailIn.FindIndex(o => o.PaymentDate > shiftedDeleteInputRecords[indexForLastShifted].DateIn);
                        insertIndex = (insertIndex == -1 ? PaymentDetailIn.Count : insertIndex);
                        PaymentDetailIn.Insert(insertIndex, new PaymentDetail
                        {
                            #region
                            PaymentDate = shiftedDeleteInputRecords[indexForLastShifted].DateIn,
                            BeginningPrincipal = currentBegningPrincipal,
                            BeginningServiceFee = currentBegningServiceFee,
                            PrincipalPayment = deletedPaymentDetail[i].PrincipalPayment,
                            InterestPayment = deletedPaymentDetail[i].InterestPayment,
                            InterestAccrued = 0,
                            InterestDue = deletedPaymentDetail[i].InterestPayment,
                            InterestCarryOver = 0,
                            TotalPayment = deletedPaymentDetail[i].TotalPayment,
                            CumulativePayment = 0,
                            CumulativeInterest = 0,
                            PeriodicInterestRate = (Input.InputRecords[Input.InputRecords.FindLastIndex(o => o.Flags == 0 || o.Flags == 1)].DateIn >= shiftedDeleteInputRecords[indexForLastShifted].DateIn) ? deletedPaymentDetail[i].PeriodicInterestRate : 0,
                            PaymentID = shiftedDeleteInputRecords[indexForLastShifted].PaymentID,
                            Flags = shiftedDeleteInputRecords[indexForLastShifted].Flags,

                            // AD - 1.0.0.6 - added columns for Service Fee values
                            ServiceFee = deletedPaymentDetail[i].ServiceFee,
                            AccruedServiceFeeInterest = 0,
                            ServiceFeeInterest = deletedPaymentDetail[i].ServiceFeeInterest,
                            serviceFeeInterestCarryOver = 0,
                            ServiceFeeTotal = deletedPaymentDetail[i].ServiceFeeTotal,
                            OriginationFee = deletedPaymentDetail[i].OriginationFee,
                            SameDayFee = deletedPaymentDetail[i].SameDayFee,
                            MaintenanceFee = deletedPaymentDetail[i].MaintenanceFee,
                            ManagementFee = deletedPaymentDetail[i].ManagementFee,

                            CumulativeServiceFee = 0,
                            CumulativeServiceFeeInterest = 0,
                            CumulativeOriginationFee = 0,
                            CumulativeSameDayFee = 0,
                            CumulativeMaintenanceFee = 0,
                            CumulativeManagementFee = 0,
                            CumulativeTotalFees = 0
                            #endregion
                        });
                        if (Input.InputRecords.FindIndex(o => o.DateIn == Input.EarlyPayoffDate) != -1 && Input.InputRecords[Input.InputRecords.FindIndex(o => o.DateIn == Input.EarlyPayoffDate)].Flags == 1)
                        {
                            PaymentDetailIn[insertIndex].PeriodicInterestRate = deletedPaymentDetail[i].PeriodicInterestRate;
                        }
                        shiftedDeleteInputRecords.RemoveAt(indexForLastShifted);
                        #endregion
                    }
                    else
                    {
                        #region This else part is used when shifted schedule are not available and then add the deleted schedule into the last schedule date

                        if (lastRowDeleted)//This is used when last scheduled date row is deleted and early payoff date is not equals to last date of the actual schedule date
                        {
                            if (PaymentDetailIn.Count > 0 ? ((lastDate > PaymentDetailIn[PaymentDetailIn.Count - 1].PaymentDate && PaymentDetailIn[PaymentDetailIn.Count - 1].Flags != 2) || (lastDate >= PaymentDetailIn[PaymentDetailIn.Count - 1].PaymentDate && PaymentDetailIn[PaymentDetailIn.Count - 1].Flags == 2)) : lastDate > Input.InputRecords[0].DateIn)
                            {
                                PaymentDetailIn.Add(new PaymentDetail
                                {
                                    PaymentDate = lastDate.AddDays(DaysPerPeriod),
                                    PaymentID = -2,
                                    Flags = 0,
                                    InterestAccrued = 0
                                });
                            }
                        }

                        PaymentDetailIn[PaymentDetailIn.Count - 1].BeginningPrincipal = PaymentDetailIn.Count == 1 ? currentBegningPrincipal : PaymentDetailIn[PaymentDetailIn.Count - 1].BeginningPrincipal + deletedPaymentDetail[i].PrincipalPayment;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].BeginningServiceFee = PaymentDetailIn.Count == 1 ? currentBegningServiceFee : PaymentDetailIn[PaymentDetailIn.Count - 1].BeginningServiceFee + deletedPaymentDetail[i].ServiceFee;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment += deletedPaymentDetail[i].PrincipalPayment;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment += deletedPaymentDetail[i].InterestPayment;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].InterestCarryOver = 0;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].InterestDue = PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].TotalPayment += deletedPaymentDetail[i].TotalPayment;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee += deletedPaymentDetail[i].ServiceFee;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest += deletedPaymentDetail[i].ServiceFeeInterest;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].serviceFeeInterestCarryOver = 0;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeTotal += deletedPaymentDetail[i].ServiceFeeTotal;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].OriginationFee += deletedPaymentDetail[i].OriginationFee;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].SameDayFee += deletedPaymentDetail[i].SameDayFee;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee += deletedPaymentDetail[i].MaintenanceFee;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee += deletedPaymentDetail[i].ManagementFee;

                        if (PaymentDetailIn[PaymentDetailIn.Count - 1].PaymentDate == deletedPaymentDetail[i].PaymentDate && PaymentDetailIn[PaymentDetailIn.Count - 1].Flags == 2)
                        {
                            PaymentDetailIn[PaymentDetailIn.Count - 1].PeriodicInterestRate += deletedPaymentDetail[i].PeriodicInterestRate;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].InterestAccrued += deletedPaymentDetail[i].InterestAccrued;
                        }
                        if (Input.InputRecords.FindIndex(o => o.DateIn == Input.EarlyPayoffDate) != -1 && Input.InputRecords[Input.InputRecords.FindIndex(o => o.DateIn == Input.EarlyPayoffDate)].Flags == 1)
                        {
                            PaymentDetailIn[PaymentDetailIn.Count - 1].PeriodicInterestRate = deletedPaymentDetail[i].PeriodicInterestRate;
                        }

                        #endregion
                    }
                    #endregion
                }

                //This is used when shifted schedules comes after the last actual schedule date and perform additional payment after the last actual schedule date
                for (int i = additionalPyamentIndex; i < Input.AdditionalPaymentRecords.Count; i++)
                {
                    #region
                    if (Input.AdditionalPaymentRecords[i].DateIn > PaymentDetailIn[PaymentDetailIn.Count - 1].PaymentDate)
                    {
                        return;
                    }
                    int insertIndex = PaymentDetailIn.FindLastIndex(o => o.PaymentDate < Input.AdditionalPaymentRecords[i].DateIn);
                    insertIndex = (insertIndex == -1 ? 0 : insertIndex + 1);
                    PaymentDetailIn.Insert(insertIndex, new PaymentDetail
                    {
                        PaymentDate = Input.AdditionalPaymentRecords[i].DateIn,
                        DueDate = Input.AdditionalPaymentRecords[i].DateIn,
                        Flags = 2,
                        PaymentID = Input.AdditionalPaymentRecords[i].PaymentID
                    });
                    int outputLastIndex = insertIndex;

                    PaymentDetailIn[outputLastIndex].BeginningPrincipal = outputLastIndex == 0 ? Input.LoanAmount : rounndedValue(PaymentDetailIn[outputLastIndex - 1].BeginningPrincipal - PaymentDetailIn[outputLastIndex - 1].PrincipalPayment);
                    PaymentDetailIn[outputLastIndex].BeginningServiceFee = outputLastIndex == 0 ? getStartingServiceFee(Input.ServiceFee, Input.LoanAmount, Input.ApplyServiceFeeInterest) : rounndedValue(PaymentDetailIn[outputLastIndex - 1].BeginningServiceFee - PaymentDetailIn[outputLastIndex - 1].ServiceFee);
                    double additionalPaymentAmount = rounndedValue(Input.AdditionalPaymentRecords[i].AdditionalPayment);
                    for (int j = PaymentDetailIn.Count - 1; j > outputLastIndex; j--)
                    {
                        if (!Input.AdditionalPaymentRecords[i].PrincipalOnly)
                        {
                            #region For NotPrincipal Only Payment
                            if (PaymentDetailIn[j].OriginationFee >= additionalPaymentAmount)
                            {
                                PaymentDetailIn[outputLastIndex].OriginationFee = additionalPaymentAmount;
                                PaymentDetailIn[j].OriginationFee = rounndedValue(PaymentDetailIn[j].OriginationFee - additionalPaymentAmount);
                                additionalPaymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailIn[outputLastIndex].OriginationFee = PaymentDetailIn[j].OriginationFee;
                                additionalPaymentAmount = rounndedValue(additionalPaymentAmount - PaymentDetailIn[j].OriginationFee);
                                PaymentDetailIn[j].OriginationFee = 0;
                            }
                            //===============================================
                            if (PaymentDetailIn[j].SameDayFee >= additionalPaymentAmount)
                            {
                                PaymentDetailIn[outputLastIndex].SameDayFee = additionalPaymentAmount;
                                PaymentDetailIn[j].SameDayFee = rounndedValue(PaymentDetailIn[j].SameDayFee - additionalPaymentAmount);
                                additionalPaymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailIn[outputLastIndex].SameDayFee = PaymentDetailIn[j].SameDayFee;
                                additionalPaymentAmount = rounndedValue(additionalPaymentAmount - PaymentDetailIn[j].SameDayFee);
                                PaymentDetailIn[j].SameDayFee = 0;
                            }

                            if (PaymentDetailIn[j].InterestPayment >= additionalPaymentAmount)
                            {
                                PaymentDetailIn[outputLastIndex].InterestPayment += additionalPaymentAmount;
                                PaymentDetailIn[j].InterestPayment = PaymentDetailIn[j].InterestPayment - additionalPaymentAmount;
                                PaymentDetailIn[j].InterestDue = PaymentDetailIn[j].InterestDue - additionalPaymentAmount;
                                additionalPaymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailIn[outputLastIndex].InterestPayment += PaymentDetailIn[j].InterestPayment;
                                additionalPaymentAmount = rounndedValue(additionalPaymentAmount - PaymentDetailIn[j].InterestPayment);
                                PaymentDetailIn[j].InterestPayment = 0;
                                PaymentDetailIn[j].InterestDue = 0;
                            }
                            PaymentDetailIn[outputLastIndex].InterestDue = PaymentDetailIn[outputLastIndex].InterestPayment;

                            //========================================================
                            if (PaymentDetailIn[j].ServiceFeeInterest >= additionalPaymentAmount)
                            {
                                PaymentDetailIn[outputLastIndex].ServiceFeeInterest += additionalPaymentAmount;
                                PaymentDetailIn[j].ServiceFeeInterest = PaymentDetailIn[j].ServiceFeeInterest - additionalPaymentAmount;
                                additionalPaymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailIn[outputLastIndex].ServiceFeeInterest += PaymentDetailIn[j].ServiceFeeInterest;
                                additionalPaymentAmount = rounndedValue(additionalPaymentAmount - PaymentDetailIn[j].ServiceFeeInterest);
                                PaymentDetailIn[j].ServiceFeeInterest = 0;
                            }
                            //========================================================

                            if (PaymentDetailIn[j].PrincipalPayment >= additionalPaymentAmount)
                            {
                                PaymentDetailIn[outputLastIndex].PrincipalPayment += rounndedValue(additionalPaymentAmount);
                                PaymentDetailIn[j].PrincipalPayment = rounndedValue(PaymentDetailIn[j].PrincipalPayment - additionalPaymentAmount);
                                additionalPaymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailIn[outputLastIndex].PrincipalPayment += rounndedValue(PaymentDetailIn[j].PrincipalPayment);
                                additionalPaymentAmount = rounndedValue(additionalPaymentAmount - PaymentDetailIn[j].PrincipalPayment);
                                PaymentDetailIn[j].PrincipalPayment = 0;
                            }
                            //========================================================
                            if (PaymentDetailIn[j].ServiceFee >= additionalPaymentAmount)
                            {
                                PaymentDetailIn[outputLastIndex].ServiceFee += rounndedValue(additionalPaymentAmount);
                                PaymentDetailIn[j].ServiceFee = rounndedValue(PaymentDetailIn[j].ServiceFee - additionalPaymentAmount);
                                additionalPaymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailIn[outputLastIndex].ServiceFee += rounndedValue(PaymentDetailIn[j].ServiceFee);
                                additionalPaymentAmount = rounndedValue(additionalPaymentAmount - PaymentDetailIn[j].ServiceFee);
                                PaymentDetailIn[j].ServiceFee = 0;
                            }
                            //========================================================
                            if (PaymentDetailIn[j].ManagementFee >= additionalPaymentAmount)
                            {
                                PaymentDetailIn[outputLastIndex].ManagementFee += additionalPaymentAmount;
                                PaymentDetailIn[j].ManagementFee = rounndedValue(PaymentDetailIn[j].ManagementFee - additionalPaymentAmount);
                                additionalPaymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailIn[outputLastIndex].ManagementFee += PaymentDetailIn[j].ManagementFee;
                                additionalPaymentAmount = rounndedValue(additionalPaymentAmount - PaymentDetailIn[j].ManagementFee);
                                PaymentDetailIn[j].ManagementFee = 0;
                            }
                            //========================================================
                            if (PaymentDetailIn[j].MaintenanceFee >= additionalPaymentAmount)
                            {
                                PaymentDetailIn[outputLastIndex].MaintenanceFee += additionalPaymentAmount;
                                PaymentDetailIn[j].MaintenanceFee = rounndedValue(PaymentDetailIn[j].MaintenanceFee - additionalPaymentAmount);
                                additionalPaymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailIn[outputLastIndex].MaintenanceFee += PaymentDetailIn[j].MaintenanceFee;
                                additionalPaymentAmount = rounndedValue(additionalPaymentAmount - PaymentDetailIn[j].MaintenanceFee);
                                PaymentDetailIn[j].MaintenanceFee = 0;
                            }
                            //========================================================
                            #endregion
                        }
                        else
                        {
                            #region
                            if (PaymentDetailIn[j].PrincipalPayment >= additionalPaymentAmount)
                            {
                                PaymentDetailIn[outputLastIndex].PrincipalPayment += rounndedValue(additionalPaymentAmount);
                                PaymentDetailIn[j].PrincipalPayment = rounndedValue(PaymentDetailIn[j].PrincipalPayment - additionalPaymentAmount);
                                additionalPaymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailIn[outputLastIndex].PrincipalPayment += rounndedValue(PaymentDetailIn[j].PrincipalPayment);
                                additionalPaymentAmount = rounndedValue(additionalPaymentAmount - PaymentDetailIn[j].PrincipalPayment);
                                PaymentDetailIn[j].PrincipalPayment = 0;
                            }

                            #endregion
                        }
                        PaymentDetailIn[outputLastIndex].TotalPayment = PaymentDetailIn[outputLastIndex].PrincipalPayment + PaymentDetailIn[outputLastIndex].InterestPayment + PaymentDetailIn[outputLastIndex].ServiceFeeInterest
                                                              + PaymentDetailIn[outputLastIndex].ServiceFee + PaymentDetailIn[outputLastIndex].OriginationFee + PaymentDetailIn[outputLastIndex].SameDayFee + PaymentDetailIn[outputLastIndex].ManagementFee
                                                              + PaymentDetailIn[outputLastIndex].MaintenanceFee;
                        PaymentDetailIn[outputLastIndex].ServiceFeeTotal = PaymentDetailIn[outputLastIndex].ServiceFee + PaymentDetailIn[outputLastIndex].ServiceFeeInterest;

                        PaymentDetailIn[j].TotalPayment = PaymentDetailIn[j].PrincipalPayment + PaymentDetailIn[j].InterestPayment + PaymentDetailIn[j].ServiceFeeInterest
                                                          + PaymentDetailIn[j].ServiceFee + PaymentDetailIn[j].OriginationFee + PaymentDetailIn[j].SameDayFee + PaymentDetailIn[j].ManagementFee
                                                          + PaymentDetailIn[j].MaintenanceFee;
                        PaymentDetailIn[j].ServiceFeeTotal = PaymentDetailIn[j].ServiceFee + PaymentDetailIn[j].ServiceFeeInterest;
                        if (PaymentDetailIn[j].TotalPayment == 0)
                        {
                            PaymentDetailIn.RemoveAt(j);
                        }
                    }
                    #endregion
                }

                #region calculate Early payoff when early payoff date is greater than the actual payment last schedule date
                // This code is used When we calculate early payoff and early payoff date is greater than actual last schedule date
                if (Input.EarlyPayoffDate >= lastDate && PaymentDetailIn[PaymentDetailIn.Count - 1].PaymentDate >= Input.EarlyPayoffDate)
                {
                    #region
                    int index = PaymentDetailIn.Count(o => o.PaymentID == -1) > 0 ?
                                    PaymentDetailIn.FindIndex(o => o.PaymentDate == Input.EarlyPayoffDate && o.PaymentID == -1) :
                                    PaymentDetailIn.FindLastIndex(o => o.PaymentDate == Input.EarlyPayoffDate);
                    if (index == -1)
                    {
                        index = PaymentDetailIn.FindIndex(o => o.PaymentDate > Input.EarlyPayoffDate);
                        PaymentDetailIn[index].PaymentDate = Input.EarlyPayoffDate;
                        PaymentDetailIn[index].PaymentID = -1;
                        PaymentDetailIn[index].Flags = 0;
                        //=====================Used for The serviceFeeIncremental============================
                        serviceFeeIncremental = PaymentDetailIn[index].ServiceFee;
                    }
                    else
                    {
                        //=====================Used for The serviceFeeIncremental============================
                        serviceFeeIncremental = PaymentDetailIn[index].ServiceFee;
                    }

                    for (int i = index + 1; i < PaymentDetailIn.Count;)
                    {
                        PaymentDetailIn[index].InterestPayment += PaymentDetailIn[i].InterestPayment;
                        PaymentDetailIn[index].InterestDue = PaymentDetailIn[index].InterestPayment;
                        PaymentDetailIn[index].TotalPayment += PaymentDetailIn[i].TotalPayment;
                        PaymentDetailIn[index].ServiceFeeInterest += PaymentDetailIn[i].ServiceFeeInterest;
                        PaymentDetailIn[index].ServiceFeeTotal += PaymentDetailIn[i].ServiceFeeTotal;
                        PaymentDetailIn[index].OriginationFee += PaymentDetailIn[i].OriginationFee;
                        PaymentDetailIn[index].SameDayFee += PaymentDetailIn[i].SameDayFee;
                        PaymentDetailIn[index].MaintenanceFee += PaymentDetailIn[i].MaintenanceFee;
                        PaymentDetailIn[index].ManagementFee += PaymentDetailIn[i].ManagementFee;
                        serviceFeeIncremental += PaymentDetailIn[i].ServiceFee;
                        PaymentDetailIn.RemoveAt(i);

                    }

                    PaymentDetailIn[index].PrincipalPayment = PaymentDetailIn[index].BeginningPrincipal;
                    PaymentDetailIn[index].InterestCarryOver = 0;
                    PaymentDetailIn[index].ServiceFee = Input.IsServiceFeeIncremental ? serviceFeeIncremental : PaymentDetailIn[index].BeginningServiceFee;
                    PaymentDetailIn[index].BeginningServiceFee = Input.IsServiceFeeIncremental ? serviceFeeIncremental : PaymentDetailIn[index].BeginningServiceFee;
                    PaymentDetailIn[index].serviceFeeInterestCarryOver = 0;

                    //======================================================================================
                    if (Input.InputRecords[Input.InputRecords.FindLastIndex(o => o.Flags == 0 || o.Flags == 1)].DateIn < PaymentDetailIn[index].PaymentDate)
                    {
                        PaymentDetailIn[index].PeriodicInterestRate = 0;
                    }



                    #endregion
                }
                // This code is used When we calculate early payoff and early payoff date is equqls to last schedule date
                if (Input.EarlyPayoffDate == PaymentDetailIn[PaymentDetailIn.Count - 1].PaymentDate)
                {
                    PaymentDetailIn[PaymentDetailIn.Count - 1].PaymentID = -1;
                    PaymentDetailIn[PaymentDetailIn.Count - 1].Flags = 0;
                }
                #endregion
            }
            catch
            {

            }
        }

        /// <summary>
        /// This method is used to pay the deleted schedule amount first and then current schedule amount when we do an additional payment.
        /// </summary>
        /// <param name="PaymentDetailIn"></param>
        /// <param name="outputIndex"></param>
        /// <param name="Input"></param>
        /// <param name="additionalPyamentIndex"></param>
        /// <param name="currentBegningPrincipal"></param>
        /// <param name="currentBegningServiceFee"></param>
        /// <param name="deletedPaymentDetail"></param>
        /// <param name="Output"></param>
        /// <param name="actualBegningPrincipal"></param>
        /// <param name="actualBegningServiceFee"></param>
        private static void NotPrincipalOnlyWithSkippedPayment(List<PaymentDetail> PaymentDetailIn, int outputIndex, getScheduleInput Input, int additionalPyamentIndex,
                double currentBegningPrincipal, double currentBegningServiceFee, List<PaymentDetail> deletedPaymentDetail, getScheduleOutput Output,
                ref double actualBegningPrincipal, ref double actualBegningServiceFee)
        {
            try
            {
                int inputIndex = 0;
                int serviceFeeIndex = -1;
                int index = deletedPaymentDetail.FindLastIndex(o => o.PaymentDate < PaymentDetailIn[outputIndex].PaymentDate && o.TotalPayment > 0);
                double additionalPaymentAmount = Input.AdditionalPaymentRecords[additionalPyamentIndex].AdditionalPayment;
                bool IsSrviceFeeApply = (Input.ApplyServiceFeeInterest == 1 || Input.ApplyServiceFeeInterest == 3) ? true : false;
                double paidServiceFeeInterest = 0;
                double serviceFeeInterestCarryOver = 0;
                double interestCarryOver = 0;
                int DaysPeriod = getDaysPerPeriod(Input.PmtPeriod);
                bool isMinDuration = Input.MinDuration > 0 ? true : false;
                int scheduleDayDifference;
                double paymentAmount = 0;
                double ManagementFeeCarryOver = 0;
                double maintenencefeeCarryOver = 0;
                bool isPaymentAmount = false;

                if (outputIndex == 0)
                {
                    PaymentDetailIn[outputIndex].BeginningPrincipal = currentBegningPrincipal;
                    PaymentDetailIn[outputIndex].BeginningServiceFee = currentBegningServiceFee;
                }
                PaymentDetailIn[outputIndex].OriginationFee = 0;
                PaymentDetailIn[outputIndex].SameDayFee = 0;
                PaymentDetailIn[outputIndex].MaintenanceFee = 0;
                PaymentDetailIn[outputIndex].ManagementFee = 0;
                PaymentDetailIn[outputIndex].InterestPayment = 0;
                PaymentDetailIn[outputIndex].PrincipalPayment = 0;
                PaymentDetailIn[outputIndex].ServiceFee = 0;

                #region This is used to pay the deleted schedule amount before current additional payment date.
                while (additionalPaymentAmount > 0 && index != -1)
                {

                    if (!Input.AdditionalPaymentRecords[additionalPyamentIndex].PrincipalOnly)
                    {
                        #region Used for NotPrincipalOnly
                        #region
                        if (deletedPaymentDetail[index].OriginationFee >= additionalPaymentAmount)
                        {
                            PaymentDetailIn[outputIndex].OriginationFee += additionalPaymentAmount;
                            deletedPaymentDetail[index].OriginationFee = rounndedValue(deletedPaymentDetail[index].OriginationFee - additionalPaymentAmount);
                            additionalPaymentAmount = 0;
                        }
                        else
                        {
                            PaymentDetailIn[outputIndex].OriginationFee += deletedPaymentDetail[index].OriginationFee;
                            additionalPaymentAmount = rounndedValue(additionalPaymentAmount - deletedPaymentDetail[index].OriginationFee);
                            deletedPaymentDetail[index].OriginationFee = 0;
                        }
                        #endregion
                        //===============================================
                        #region
                        if (deletedPaymentDetail[index].SameDayFee >= additionalPaymentAmount)
                        {
                            PaymentDetailIn[outputIndex].SameDayFee += additionalPaymentAmount;
                            deletedPaymentDetail[index].SameDayFee = rounndedValue(deletedPaymentDetail[index].SameDayFee - additionalPaymentAmount);
                            additionalPaymentAmount = 0;
                        }
                        else
                        {
                            PaymentDetailIn[outputIndex].SameDayFee += deletedPaymentDetail[index].SameDayFee;
                            additionalPaymentAmount = rounndedValue(additionalPaymentAmount - deletedPaymentDetail[index].SameDayFee);
                            deletedPaymentDetail[index].SameDayFee = 0;
                        }
                        #endregion
                        //====================================================
                        #region
                        if (deletedPaymentDetail[index].InterestPayment >= additionalPaymentAmount)
                        {
                            PaymentDetailIn[outputIndex].InterestPayment += additionalPaymentAmount;
                            deletedPaymentDetail[index].InterestPayment = deletedPaymentDetail[index].InterestPayment - additionalPaymentAmount;
                            deletedPaymentDetail[index].InterestDue = deletedPaymentDetail[index].InterestPayment - additionalPaymentAmount;
                            additionalPaymentAmount = 0;
                        }
                        else
                        {
                            PaymentDetailIn[outputIndex].InterestPayment += deletedPaymentDetail[index].InterestPayment;
                            additionalPaymentAmount = rounndedValue(additionalPaymentAmount - deletedPaymentDetail[index].InterestPayment);
                            deletedPaymentDetail[index].InterestDue = 0;
                            deletedPaymentDetail[index].InterestPayment = 0;
                        }
                        #endregion
                        //========================================================
                        #region
                        if (deletedPaymentDetail[index].ServiceFeeInterest >= additionalPaymentAmount)
                        {
                            paidServiceFeeInterest += additionalPaymentAmount;
                            deletedPaymentDetail[index].ServiceFeeInterest = deletedPaymentDetail[index].ServiceFeeInterest - additionalPaymentAmount;
                            additionalPaymentAmount = 0;
                        }
                        else
                        {
                            paidServiceFeeInterest += deletedPaymentDetail[index].ServiceFeeInterest;
                            additionalPaymentAmount = rounndedValue(additionalPaymentAmount - deletedPaymentDetail[index].ServiceFeeInterest);
                            deletedPaymentDetail[index].ServiceFeeInterest = 0;
                        }
                        #endregion
                        //========================================================
                        #region
                        if (deletedPaymentDetail[index].PrincipalPayment >= additionalPaymentAmount)
                        {
                            PaymentDetailIn[outputIndex].PrincipalPayment += additionalPaymentAmount;
                            deletedPaymentDetail[index].PrincipalPayment = rounndedValue(deletedPaymentDetail[index].PrincipalPayment - additionalPaymentAmount);
                            additionalPaymentAmount = 0;
                        }
                        else
                        {
                            PaymentDetailIn[outputIndex].PrincipalPayment += deletedPaymentDetail[index].PrincipalPayment;
                            additionalPaymentAmount = rounndedValue(additionalPaymentAmount - deletedPaymentDetail[index].PrincipalPayment);
                            deletedPaymentDetail[index].PrincipalPayment = 0;
                        }
                        #endregion
                        //========================================================
                        #region
                        if (deletedPaymentDetail[index].ServiceFee >= additionalPaymentAmount)
                        {
                            PaymentDetailIn[outputIndex].ServiceFee += additionalPaymentAmount;
                            deletedPaymentDetail[index].ServiceFee = rounndedValue(deletedPaymentDetail[index].ServiceFee - additionalPaymentAmount);
                            additionalPaymentAmount = 0;
                        }
                        else
                        {
                            PaymentDetailIn[outputIndex].ServiceFee += deletedPaymentDetail[index].ServiceFee;
                            additionalPaymentAmount = rounndedValue(additionalPaymentAmount - deletedPaymentDetail[index].ServiceFee);
                            deletedPaymentDetail[index].ServiceFee = 0;
                        }
                        #endregion
                        //========================================================
                        #region
                        if (deletedPaymentDetail[index].ManagementFee >= additionalPaymentAmount)
                        {
                            PaymentDetailIn[outputIndex].ManagementFee += additionalPaymentAmount;
                            deletedPaymentDetail[index].ManagementFee = rounndedValue(deletedPaymentDetail[index].ManagementFee - additionalPaymentAmount);
                            additionalPaymentAmount = 0;
                        }
                        else
                        {
                            PaymentDetailIn[outputIndex].ManagementFee += deletedPaymentDetail[index].ManagementFee;
                            additionalPaymentAmount = rounndedValue(additionalPaymentAmount - deletedPaymentDetail[index].ManagementFee);
                            deletedPaymentDetail[index].ManagementFee = 0;
                        }
                        #endregion
                        //========================================================
                        #region
                        if (deletedPaymentDetail[index].MaintenanceFee >= additionalPaymentAmount)
                        {
                            PaymentDetailIn[outputIndex].MaintenanceFee += additionalPaymentAmount;
                            deletedPaymentDetail[index].MaintenanceFee = rounndedValue(deletedPaymentDetail[index].MaintenanceFee - additionalPaymentAmount);
                            additionalPaymentAmount = 0;
                        }
                        else
                        {
                            PaymentDetailIn[outputIndex].MaintenanceFee += deletedPaymentDetail[index].MaintenanceFee;
                            additionalPaymentAmount = rounndedValue(additionalPaymentAmount - deletedPaymentDetail[index].MaintenanceFee);
                            deletedPaymentDetail[index].MaintenanceFee = 0;
                        }
                        #endregion
                        //========================================================
                        deletedPaymentDetail[index].TotalPayment = deletedPaymentDetail[index].PrincipalPayment + deletedPaymentDetail[index].InterestPayment + deletedPaymentDetail[index].ServiceFeeInterest
                                                                + deletedPaymentDetail[index].ServiceFee + deletedPaymentDetail[index].OriginationFee + deletedPaymentDetail[index].SameDayFee + deletedPaymentDetail[index].ManagementFee
                                                                + deletedPaymentDetail[index].MaintenanceFee;
                        index = deletedPaymentDetail.FindLastIndex(o => o.PaymentDate < PaymentDetailIn[outputIndex].PaymentDate && o.TotalPayment > 0);
                        #endregion
                    }
                    else
                    {
                        #region used for PrincipalOnly
                        if (deletedPaymentDetail[index].PrincipalPayment >= additionalPaymentAmount)
                        {
                            PaymentDetailIn[outputIndex].PrincipalPayment += additionalPaymentAmount;
                            deletedPaymentDetail[index].PrincipalPayment = rounndedValue(deletedPaymentDetail[index].PrincipalPayment - additionalPaymentAmount);
                            additionalPaymentAmount = 0;
                        }
                        else
                        {
                            PaymentDetailIn[outputIndex].PrincipalPayment += deletedPaymentDetail[index].PrincipalPayment;
                            additionalPaymentAmount = rounndedValue(additionalPaymentAmount - deletedPaymentDetail[index].PrincipalPayment);
                            deletedPaymentDetail[index].PrincipalPayment = 0;
                        }
                        deletedPaymentDetail[index].TotalPayment = deletedPaymentDetail[index].PrincipalPayment + deletedPaymentDetail[index].InterestPayment + deletedPaymentDetail[index].ServiceFeeInterest
                                                                + deletedPaymentDetail[index].ServiceFee + deletedPaymentDetail[index].OriginationFee + deletedPaymentDetail[index].SameDayFee + deletedPaymentDetail[index].ManagementFee
                                                                + deletedPaymentDetail[index].MaintenanceFee;
                        index = deletedPaymentDetail.FindLastIndex(o => o.PaymentDate < PaymentDetailIn[outputIndex].PaymentDate && o.PrincipalPayment > 0);
                        #endregion
                    }
                }
                #endregion

                #region This is used to pay the amount of the current schedule
                if (additionalPaymentAmount >= 0)
                {
                    #region
                    if (!Input.AdditionalPaymentRecords[additionalPyamentIndex].PrincipalOnly)
                    {
                        #region used for NotPrincipalOnly
                        #region For Interest payment Allocation
                        if (PaymentDetailIn[outputIndex].InterestDue >= additionalPaymentAmount)
                        {
                            PaymentDetailIn[outputIndex].InterestPayment += additionalPaymentAmount;
                            PaymentDetailIn[outputIndex].InterestCarryOver = PaymentDetailIn[outputIndex].InterestDue - additionalPaymentAmount;
                            additionalPaymentAmount = 0;
                        }
                        else
                        {
                            PaymentDetailIn[outputIndex].InterestPayment += PaymentDetailIn[outputIndex].InterestDue;
                            additionalPaymentAmount = rounndedValue(additionalPaymentAmount - PaymentDetailIn[outputIndex].InterestDue);
                            PaymentDetailIn[outputIndex].InterestCarryOver = 0;
                        }
                        #endregion
                        //========================================================
                        #region For Service fee Interest payment Allocation
                        index = deletedPaymentDetail.FindLastIndex(o => o.PaymentDate < PaymentDetailIn[outputIndex].PaymentDate);
                        //serviceFeeInterestCarryOver = outputIndex == 0 ? 0 : 
                        //    (deletedPaymentDetail.FindIndex(o => o.PaymentDate >= PaymentDetailIn[outputIndex - 1].PaymentDate && o.PaymentDate < PaymentDetailIn[outputIndex].PaymentDate) == -1 ? 
                        //    PaymentDetailIn[outputIndex - 1].serviceFeeInterestCarryOver : 0);
                        serviceFeeInterestCarryOver = outputIndex == 0 ? (index != -1 ? deletedPaymentDetail[index].serviceFeeInterestCarryOver : 0) :
                            (deletedPaymentDetail.FindIndex(o => o.PaymentDate >= PaymentDetailIn[outputIndex - 1].PaymentDate && o.PaymentDate < PaymentDetailIn[outputIndex].PaymentDate) == -1 ?
                            PaymentDetailIn[outputIndex - 1].serviceFeeInterestCarryOver : 0);

                        if (rounndedValue(PaymentDetailIn[outputIndex].AccruedServiceFeeInterest + serviceFeeInterestCarryOver) >= additionalPaymentAmount)
                        {
                            PaymentDetailIn[outputIndex].serviceFeeInterestCarryOver = ((PaymentDetailIn[outputIndex].AccruedServiceFeeInterest + serviceFeeInterestCarryOver) - additionalPaymentAmount);
                            PaymentDetailIn[outputIndex].ServiceFeeInterest = additionalPaymentAmount;
                            additionalPaymentAmount = 0;
                        }
                        else
                        {
                            additionalPaymentAmount = rounndedValue(additionalPaymentAmount - rounndedValue(PaymentDetailIn[outputIndex].AccruedServiceFeeInterest + serviceFeeInterestCarryOver));
                            PaymentDetailIn[outputIndex].serviceFeeInterestCarryOver = 0;
                        }
                        PaymentDetailIn[outputIndex].ServiceFeeInterest += paidServiceFeeInterest;
                        #endregion
                        //========================================================
                        #region For Principal payment Allocation
                        if (actualBegningPrincipal >= additionalPaymentAmount)
                        {
                            PaymentDetailIn[outputIndex].PrincipalPayment = rounndedValue(PaymentDetailIn[outputIndex].PrincipalPayment + additionalPaymentAmount);
                            actualBegningPrincipal = rounndedValue(actualBegningPrincipal - additionalPaymentAmount);
                            additionalPaymentAmount = 0;
                        }
                        else
                        {
                            PaymentDetailIn[outputIndex].PrincipalPayment = rounndedValue(PaymentDetailIn[outputIndex].PrincipalPayment + actualBegningPrincipal);
                            additionalPaymentAmount = rounndedValue(additionalPaymentAmount - actualBegningPrincipal);
                            actualBegningPrincipal = 0;
                        }
                        #endregion
                        //========================================================
                        #region For Service Fee payment Allocation
                        if (actualBegningServiceFee >= additionalPaymentAmount)
                        {
                            PaymentDetailIn[outputIndex].ServiceFee = rounndedValue(PaymentDetailIn[outputIndex].ServiceFee + additionalPaymentAmount);
                            actualBegningServiceFee = rounndedValue(actualBegningServiceFee - additionalPaymentAmount);
                            additionalPaymentAmount = 0;
                        }
                        else
                        {
                            PaymentDetailIn[outputIndex].ServiceFee = rounndedValue(PaymentDetailIn[outputIndex].ServiceFee + actualBegningServiceFee);
                            additionalPaymentAmount = rounndedValue(additionalPaymentAmount - actualBegningServiceFee);
                            actualBegningServiceFee = 0;
                        }
                        #endregion
                        //========================================================
                        #region Calculate management and maintenance fee
                        ManagementFeeCarryOver = outputIndex == 0 ? (index != -1 ? deletedPaymentDetail[index].ManagementFeeCarryOver : 0) :
                        (deletedPaymentDetail.FindIndex(o => o.PaymentDate >= PaymentDetailIn[outputIndex - 1].PaymentDate && o.PaymentDate < PaymentDetailIn[outputIndex].PaymentDate) == -1 ?
                        PaymentDetailIn[outputIndex - 1].ManagementFeeCarryOver : 0);

                        maintenencefeeCarryOver = outputIndex == 0 ? (index != -1 ? deletedPaymentDetail[index].MaintenanceFeeCarryOver : 0) :
                        (deletedPaymentDetail.FindIndex(o => o.PaymentDate >= PaymentDetailIn[outputIndex - 1].PaymentDate && o.PaymentDate < PaymentDetailIn[outputIndex].PaymentDate) == -1 ?
                        PaymentDetailIn[outputIndex - 1].MaintenanceFeeCarryOver : 0);

                        #region Calulate management Fee payment
                        if (ManagementFeeCarryOver > additionalPaymentAmount)
                        {
                            PaymentDetailIn[outputIndex].ManagementFee += additionalPaymentAmount;
                            PaymentDetailIn[outputIndex].ManagementFeeCarryOver -= additionalPaymentAmount;
                            additionalPaymentAmount = 0;
                        }
                        else
                        {
                            PaymentDetailIn[outputIndex].ManagementFee += ManagementFeeCarryOver;
                            additionalPaymentAmount = rounndedValue(additionalPaymentAmount - ManagementFeeCarryOver);
                            PaymentDetailIn[outputIndex].ManagementFeeCarryOver = 0;
                        }
                        #endregion
                        //=========================================================
                        #region Calulate maintenance Fee payment
                        if (maintenencefeeCarryOver > additionalPaymentAmount)
                        {
                            PaymentDetailIn[outputIndex].MaintenanceFee += additionalPaymentAmount;
                            PaymentDetailIn[outputIndex].MaintenanceFeeCarryOver -= additionalPaymentAmount;
                            additionalPaymentAmount = 0;
                        }
                        else
                        {
                            PaymentDetailIn[outputIndex].MaintenanceFee += maintenencefeeCarryOver;
                            PaymentDetailIn[outputIndex].MaintenanceFeeCarryOver = 0;
                            additionalPaymentAmount = rounndedValue(additionalPaymentAmount - maintenencefeeCarryOver);

                        }
                        #endregion
                        //=========================================================
                        #endregion
                        //===========================================================
                        #endregion
                    }
                    else
                    {
                        #region Used for PrincipalOnly
                        index = deletedPaymentDetail.FindLastIndex(o => o.PaymentDate < PaymentDetailIn[outputIndex].PaymentDate);

                        ManagementFeeCarryOver = outputIndex == 0 ? (index != -1 ? deletedPaymentDetail[index].ManagementFeeCarryOver : 0) :
                            (deletedPaymentDetail.FindIndex(o => o.PaymentDate >= PaymentDetailIn[outputIndex - 1].PaymentDate && o.PaymentDate < PaymentDetailIn[outputIndex].PaymentDate) == -1 ?
                            PaymentDetailIn[outputIndex - 1].ManagementFeeCarryOver : 0);
                        PaymentDetailIn[outputIndex].ManagementFeeCarryOver = ManagementFeeCarryOver;

                        maintenencefeeCarryOver = outputIndex == 0 ? (index != -1 ? deletedPaymentDetail[index].MaintenanceFeeCarryOver : 0) :
                        (deletedPaymentDetail.FindIndex(o => o.PaymentDate >= PaymentDetailIn[outputIndex - 1].PaymentDate && o.PaymentDate < PaymentDetailIn[outputIndex].PaymentDate) == -1 ?
                        PaymentDetailIn[outputIndex - 1].MaintenanceFeeCarryOver : 0);
                        PaymentDetailIn[outputIndex].ManagementFeeCarryOver = maintenencefeeCarryOver;


                        if (actualBegningPrincipal >= additionalPaymentAmount)
                        {
                            PaymentDetailIn[outputIndex].PrincipalPayment = rounndedValue(PaymentDetailIn[outputIndex].PrincipalPayment + additionalPaymentAmount);
                            actualBegningPrincipal = rounndedValue(actualBegningPrincipal - additionalPaymentAmount);
                            additionalPaymentAmount = 0;
                        }
                        else
                        {
                            PaymentDetailIn[outputIndex].PrincipalPayment = rounndedValue(PaymentDetailIn[outputIndex].PrincipalPayment + actualBegningPrincipal);
                            additionalPaymentAmount = rounndedValue(additionalPaymentAmount - actualBegningPrincipal);
                            actualBegningPrincipal = 0;
                        }

                        interestCarryOver = outputIndex == 0 ? 0 : (deletedPaymentDetail.FindIndex(o => o.PaymentDate >= PaymentDetailIn[outputIndex - 1].PaymentDate && o.PaymentDate < PaymentDetailIn[outputIndex].PaymentDate) == -1 ? PaymentDetailIn[outputIndex - 1].InterestCarryOver : 0);
                        PaymentDetailIn[outputIndex].InterestDue = PaymentDetailIn[outputIndex].InterestAccrued + interestCarryOver;
                        PaymentDetailIn[outputIndex].InterestCarryOver = PaymentDetailIn[outputIndex].InterestAccrued + interestCarryOver;
                        PaymentDetailIn[outputIndex].InterestPayment = 0;

                        serviceFeeInterestCarryOver = outputIndex == 0 ? PaymentDetailIn[outputIndex].serviceFeeInterestCarryOver : PaymentDetailIn[outputIndex - 1].serviceFeeInterestCarryOver;
                        PaymentDetailIn[outputIndex].serviceFeeInterestCarryOver = outputIndex == 0 ? PaymentDetailIn[outputIndex].serviceFeeInterestCarryOver : rounndedValue(PaymentDetailIn[outputIndex].AccruedServiceFeeInterest + serviceFeeInterestCarryOver);

                        PaymentDetailIn[outputIndex].ServiceFeeInterest = 0;
                        #endregion
                    }
                    PaymentDetailIn[outputIndex].TotalPayment = PaymentDetailIn[outputIndex].PrincipalPayment + PaymentDetailIn[outputIndex].InterestPayment + PaymentDetailIn[outputIndex].ServiceFeeInterest
                                                    + PaymentDetailIn[outputIndex].ServiceFee + PaymentDetailIn[outputIndex].OriginationFee + PaymentDetailIn[outputIndex].SameDayFee + PaymentDetailIn[outputIndex].ManagementFee
                                                    + PaymentDetailIn[outputIndex].MaintenanceFee;
                    PaymentDetailIn[outputIndex].ServiceFeeTotal = PaymentDetailIn[outputIndex].ServiceFee + PaymentDetailIn[outputIndex].ServiceFeeInterest;
                    #endregion
                }
                #endregion

                double actualBegningPrincipalRemain = actualBegningPrincipal;
                double actualBegningServicefeeRemain = actualBegningServiceFee;

                // This is used for recalculating the schdules that exist in the output schedule
                for (int i = outputIndex + 1; i <= PaymentDetailIn.Count - 1; i++)
                {
                    #region

                    scheduleDayDifference = CalculatScheduleDifference(PaymentDetailIn, PaymentDetailIn[i].PaymentDate, i, Input, isMinDuration, DaysPeriod, PaymentDetailIn[i].Flags);
                    PaymentDetailIn[i].PeriodicInterestRate = CalculatePeriodicInterestRateTier(Input, actualBegningPrincipalRemain, scheduleDayDifference);
                    PaymentDetailIn[i].InterestAccrued = CalculateCurrentInterestTier(Input, actualBegningPrincipalRemain, scheduleDayDifference);
                    PaymentDetailIn[i].DailyInterestAmount = (PaymentDetailIn[i].PeriodicInterestRate == 0 || scheduleDayDifference == 0) ? 0 : rounndedValue((PaymentDetailIn[i].PeriodicInterestRate / scheduleDayDifference) * actualBegningPrincipalRemain);
                    PaymentDetailIn[i].AccruedServiceFeeInterest = IsSrviceFeeApply ? CalculateCurrentServiceFeeInterestTier(Input, actualBegningServicefeeRemain, scheduleDayDifference) : 0;
                    PaymentDetailIn[i].ServiceFeeInterest = PaymentDetailIn[i].AccruedServiceFeeInterest + PaymentDetailIn[i - 1].serviceFeeInterestCarryOver;
                    PaymentDetailIn[i].InterestDue = PaymentDetailIn[i].InterestAccrued + PaymentDetailIn[i - 1].InterestCarryOver;

                    index = deletedPaymentDetail.FindLastIndex(o => o.PaymentDate < PaymentDetailIn[i].PaymentDate);

                    ManagementFeeCarryOver = i == 0 ? (index != -1 ? deletedPaymentDetail[index].ManagementFeeCarryOver : 0) :
                        (deletedPaymentDetail.FindIndex(o => o.PaymentDate >= PaymentDetailIn[i - 1].PaymentDate && o.PaymentDate < PaymentDetailIn[i].PaymentDate) == -1 ?
                        PaymentDetailIn[i - 1].ManagementFeeCarryOver : 0);

                    maintenencefeeCarryOver = i == 0 ? (index != -1 ? deletedPaymentDetail[index].MaintenanceFeeCarryOver : 0) :
                    (deletedPaymentDetail.FindIndex(o => o.PaymentDate >= PaymentDetailIn[i - 1].PaymentDate && o.PaymentDate < PaymentDetailIn[i].PaymentDate) == -1 ?
                    PaymentDetailIn[i - 1].MaintenanceFeeCarryOver : 0);


                    if (PaymentDetailIn[i].PaymentID == -1)
                    {
                        PaymentDetailIn.RemoveAt(i);
                        i--;
                    }
                    else if (PaymentDetailIn[i].Flags == 2)
                    {
                        #region  used for additional payment date

                        additionalPyamentIndex++;
                        additionalPaymentAmount = rounndedValue(Input.AdditionalPaymentRecords[additionalPyamentIndex].AdditionalPayment);
                        if (Input.AdditionalPaymentRecords[additionalPyamentIndex].PrincipalOnly)
                        {
                            #region// For Principal Only
                            PaymentDetailIn[i].PrincipalPayment = actualBegningPrincipalRemain > additionalPaymentAmount ? additionalPaymentAmount : actualBegningPrincipalRemain;
                            PaymentDetailIn[i].InterestPayment = 0;
                            PaymentDetailIn[i].InterestCarryOver = PaymentDetailIn[i].InterestDue;
                            PaymentDetailIn[i].serviceFeeInterestCarryOver = PaymentDetailIn[i].ServiceFeeInterest;
                            PaymentDetailIn[i].ServiceFeeInterest = 0;
                            PaymentDetailIn[i].ServiceFee = 0;
                            PaymentDetailIn[i].TotalPayment = PaymentDetailIn[i].PrincipalPayment;
                            PaymentDetailIn[i].ServiceFeeTotal = 0;
                            PaymentDetailIn[i].ManagementFeeCarryOver = ManagementFeeCarryOver;
                            PaymentDetailIn[i].ManagementFeeCarryOver = maintenencefeeCarryOver;
                            actualBegningPrincipalRemain = rounndedValue(actualBegningPrincipalRemain - PaymentDetailIn[i].PrincipalPayment);
                            #endregion
                        }
                        else
                        {
                            #region For NotPrincipal Only Payment
                            #region For Interest Payment
                            if (PaymentDetailIn[i].InterestDue >= additionalPaymentAmount)
                            {
                                PaymentDetailIn[i].InterestPayment = additionalPaymentAmount;
                                PaymentDetailIn[i].InterestCarryOver = PaymentDetailIn[i].InterestDue - additionalPaymentAmount;
                                additionalPaymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailIn[i].InterestPayment = PaymentDetailIn[i].InterestDue;
                                additionalPaymentAmount = rounndedValue(additionalPaymentAmount - PaymentDetailIn[i].InterestDue);
                                PaymentDetailIn[i].InterestCarryOver = 0;
                            }
                            #endregion
                            //========================================================
                            #region For Service Fee Interest
                            if (PaymentDetailIn[i].ServiceFeeInterest >= additionalPaymentAmount)
                            {
                                PaymentDetailIn[i].serviceFeeInterestCarryOver = PaymentDetailIn[i].ServiceFeeInterest - additionalPaymentAmount;
                                PaymentDetailIn[i].ServiceFeeInterest = additionalPaymentAmount;
                                additionalPaymentAmount = 0;
                            }
                            else
                            {
                                additionalPaymentAmount = rounndedValue(additionalPaymentAmount - PaymentDetailIn[i].ServiceFeeInterest);
                                PaymentDetailIn[i].serviceFeeInterestCarryOver = 0;
                            }
                            #endregion
                            //========================================================
                            #region For Principal Payment
                            if (actualBegningPrincipalRemain >= additionalPaymentAmount)
                            {
                                PaymentDetailIn[i].PrincipalPayment = rounndedValue(additionalPaymentAmount);
                                actualBegningPrincipalRemain = rounndedValue(actualBegningPrincipalRemain - additionalPaymentAmount);
                                additionalPaymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailIn[i].PrincipalPayment = rounndedValue(actualBegningPrincipalRemain);
                                additionalPaymentAmount = rounndedValue(additionalPaymentAmount - actualBegningPrincipalRemain);
                                actualBegningPrincipalRemain = 0;
                            }
                            #endregion
                            //========================================================
                            #region For Service Fee 
                            if (actualBegningServicefeeRemain >= additionalPaymentAmount)
                            {
                                PaymentDetailIn[i].ServiceFee = rounndedValue(additionalPaymentAmount);
                                actualBegningServicefeeRemain = rounndedValue(actualBegningServicefeeRemain - additionalPaymentAmount);
                                additionalPaymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailIn[i].ServiceFee = rounndedValue(actualBegningServicefeeRemain);
                                additionalPaymentAmount = rounndedValue(additionalPaymentAmount - actualBegningServicefeeRemain);
                                actualBegningServicefeeRemain = 0;
                            }
                            #endregion
                            //========================================================
                            #region Calulate management Fee payment
                            if (ManagementFeeCarryOver > additionalPaymentAmount)
                            {
                                PaymentDetailIn[i].ManagementFee = additionalPaymentAmount;
                                PaymentDetailIn[i].ManagementFeeCarryOver = ManagementFeeCarryOver - additionalPaymentAmount;
                                additionalPaymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailIn[i].ManagementFee = ManagementFeeCarryOver;
                                additionalPaymentAmount = rounndedValue(additionalPaymentAmount - ManagementFeeCarryOver);
                                PaymentDetailIn[i].ManagementFeeCarryOver = 0;
                            }
                            #endregion
                            //=========================================================
                            #region Calulate maintenance Fee payment
                            if (maintenencefeeCarryOver > additionalPaymentAmount)
                            {
                                PaymentDetailIn[i].MaintenanceFee = additionalPaymentAmount;
                                PaymentDetailIn[i].MaintenanceFeeCarryOver = maintenencefeeCarryOver - additionalPaymentAmount;
                                additionalPaymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailIn[i].MaintenanceFee = maintenencefeeCarryOver;
                                additionalPaymentAmount = rounndedValue(additionalPaymentAmount - maintenencefeeCarryOver);
                                PaymentDetailIn[i].MaintenanceFeeCarryOver = 0;
                            }
                            #endregion
                            //=========================================================
                            PaymentDetailIn[i].TotalPayment = PaymentDetailIn[i].PrincipalPayment + PaymentDetailIn[i].InterestPayment + PaymentDetailIn[i].ServiceFeeInterest
                                                              + PaymentDetailIn[i].ServiceFee + PaymentDetailIn[i].OriginationFee + PaymentDetailIn[i].SameDayFee + PaymentDetailIn[i].ManagementFee
                                                              + PaymentDetailIn[i].MaintenanceFee;
                            PaymentDetailIn[i].ServiceFeeTotal = PaymentDetailIn[i].ServiceFee + PaymentDetailIn[i].ServiceFeeInterest;
                            #endregion
                        }
                        #endregion
                    }
                    else
                    {
                        #region
                        inputIndex = Input.InputRecords.FindIndex(o => o.DateIn == PaymentDetailIn[i].PaymentDate && (o.Flags == 0 || o.Flags == 1));
                        if (!string.IsNullOrEmpty(Input.InputRecords[inputIndex].PaymentAmount) || Input.PaymentAmount > 0)
                        {
                            #region
                            if (!string.IsNullOrEmpty(Input.InputRecords[inputIndex].PaymentAmount))
                            {
                                paymentAmount = rounndedValue(Convert.ToDouble(Input.InputRecords[inputIndex].PaymentAmount));
                                isPaymentAmount = false;
                            }
                            else
                            {
                                isPaymentAmount = true;
                                paymentAmount = rounndedValue(Input.PaymentAmount);
                                ManagementFeeCarryOver += Input.ManagementFee;
                                maintenencefeeCarryOver += inputIndex == 1 ? 0 : Input.MaintenanceFee;
                            }

                            #region For Interest Payment
                            if (Input.InputRecords.FindIndex(o => o.DateIn > PaymentDetailIn[i].PaymentDate && (o.Flags == 0 || o.Flags == 1)) == -1)
                            {
                                PaymentDetailIn[i].InterestPayment = PaymentDetailIn[i].InterestDue;
                            }
                            else if (PaymentDetailIn[i].InterestDue >= paymentAmount)
                            {
                                PaymentDetailIn[i].InterestPayment = paymentAmount;
                                PaymentDetailIn[i].InterestCarryOver = PaymentDetailIn[i].InterestDue - paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailIn[i].InterestPayment = PaymentDetailIn[i].InterestDue;
                                paymentAmount = rounndedValue(paymentAmount - PaymentDetailIn[i].InterestDue);
                                PaymentDetailIn[i].InterestCarryOver = 0;
                            }
                            #endregion
                            //========================================================
                            #region For Service Fee Interest
                            if (Input.InputRecords.FindIndex(o => o.DateIn > PaymentDetailIn[i].PaymentDate && (o.Flags == 0 || o.Flags == 1)) == -1)
                            {
                                PaymentDetailIn[i].ServiceFeeInterest = PaymentDetailIn[i].ServiceFeeInterest;
                            }
                            else if (PaymentDetailIn[i].ServiceFeeInterest >= paymentAmount)
                            {
                                PaymentDetailIn[i].serviceFeeInterestCarryOver = PaymentDetailIn[i].ServiceFeeInterest - paymentAmount;
                                PaymentDetailIn[i].ServiceFeeInterest = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                paymentAmount = rounndedValue(paymentAmount - PaymentDetailIn[i].ServiceFeeInterest);
                                PaymentDetailIn[i].serviceFeeInterestCarryOver = 0;
                            }
                            #endregion
                            //========================================================
                            #region For Principal Payment
                            if (Input.InputRecords.FindIndex(o => o.DateIn > PaymentDetailIn[i].PaymentDate && (o.Flags == 0 || o.Flags == 1)) == -1)
                            {
                                PaymentDetailIn[i].PrincipalPayment = actualBegningPrincipalRemain;
                                actualBegningPrincipalRemain = 0;
                            }
                            else if (actualBegningPrincipalRemain >= paymentAmount)
                            {
                                PaymentDetailIn[i].PrincipalPayment = rounndedValue(paymentAmount);
                                actualBegningPrincipalRemain = rounndedValue(actualBegningPrincipalRemain - paymentAmount);
                                paymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailIn[i].PrincipalPayment = rounndedValue(actualBegningPrincipalRemain);
                                paymentAmount = rounndedValue(paymentAmount - actualBegningPrincipalRemain);
                                actualBegningPrincipalRemain = 0;
                            }
                            #region calculate principal payment amount when principal payment is zero and provide enforce principal amount
                            if (i >= Input.EnforcedPayment && PaymentDetailIn[i].PrincipalPayment == 0 && PaymentDetailIn[i].InterestPayment > 0 && i != (PaymentDetailIn.Count - 1))
                            {
                                PaymentDetailIn[i].PrincipalPayment = PaymentDetailIn[i].InterestPayment <= Input.EnforcedPrincipal ? rounndedValue(PaymentDetailIn[i].InterestPayment) : rounndedValue(Input.EnforcedPrincipal);
                                PaymentDetailIn[i].InterestPayment -= PaymentDetailIn[i].PrincipalPayment;
                                PaymentDetailIn[i].InterestCarryOver += PaymentDetailIn[i].PrincipalPayment;
                                PaymentDetailIn[i].PrincipalPayment = PaymentDetailIn[i].PrincipalPayment <= 0 ? 0 : PaymentDetailIn[i].PrincipalPayment;
                                actualBegningPrincipalRemain = rounndedValue(actualBegningPrincipalRemain - PaymentDetailIn[i].PrincipalPayment);
                                PaymentDetailIn[i].PrincipalPayment = PaymentDetailIn[i].PrincipalPayment <= actualBegningPrincipalRemain ? PaymentDetailIn[i].PrincipalPayment : actualBegningPrincipalRemain;
                            }
                            #endregion

                            #endregion
                            //========================================================
                            #region For Service Fee 
                            if (Input.InputRecords.FindIndex(o => o.DateIn > PaymentDetailIn[i].PaymentDate && (o.Flags == 0 || o.Flags == 1)) == -1)
                            {
                                PaymentDetailIn[i].ServiceFee = actualBegningServicefeeRemain;
                                actualBegningServicefeeRemain = 0;
                            }
                            else if (actualBegningServicefeeRemain >= paymentAmount)
                            {
                                PaymentDetailIn[i].ServiceFee = rounndedValue(paymentAmount);
                                actualBegningServicefeeRemain = rounndedValue(actualBegningServicefeeRemain - paymentAmount);
                                paymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailIn[i].ServiceFee = rounndedValue(actualBegningServicefeeRemain);
                                paymentAmount = rounndedValue(paymentAmount - actualBegningServicefeeRemain);
                                actualBegningServicefeeRemain = 0;
                            }
                            #endregion
                            //========================================================
                            #region Calulate management and maintenance Fee payment
                            if (Input.InputRecords.FindIndex(o => o.DateIn > PaymentDetailIn[i].PaymentDate && (o.Flags == 0 || o.Flags == 1)) == -1)
                            {
                                PaymentDetailIn[i].ManagementFee = ManagementFeeCarryOver;
                                PaymentDetailIn[i].MaintenanceFee = maintenencefeeCarryOver;
                                PaymentDetailIn[i].ManagementFeeCarryOver = 0;
                                PaymentDetailIn[i].MaintenanceFeeCarryOver = 0;
                            }
                            else if (isPaymentAmount)
                            {
                                #region calculate management and maintenance fee amount
                                if (ManagementFeeCarryOver > 0)
                                {
                                    #region
                                    if (ManagementFeeCarryOver > paymentAmount)
                                    {
                                        PaymentDetailIn[i].ManagementFee = paymentAmount;
                                        ManagementFeeCarryOver -= paymentAmount;
                                        PaymentDetailIn[i].ManagementFeeCarryOver = ManagementFeeCarryOver;
                                        paymentAmount = 0;
                                    }
                                    else
                                    {
                                        PaymentDetailIn[i].ManagementFee = ManagementFeeCarryOver;
                                        paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[i].ManagementFee));
                                        PaymentDetailIn[i].ManagementFeeCarryOver = 0;
                                        ManagementFeeCarryOver = 0;
                                    }
                                    #endregion
                                }
                                if (maintenencefeeCarryOver > 0)
                                {
                                    #region
                                    if (maintenencefeeCarryOver > paymentAmount)
                                    {
                                        PaymentDetailIn[i].MaintenanceFee = paymentAmount;
                                        maintenencefeeCarryOver -= paymentAmount;
                                        PaymentDetailIn[i].MaintenanceFeeCarryOver = maintenencefeeCarryOver;
                                        paymentAmount = 0;
                                    }
                                    else
                                    {
                                        PaymentDetailIn[i].MaintenanceFee = maintenencefeeCarryOver;
                                        paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[i].MaintenanceFee));
                                        PaymentDetailIn[i].MaintenanceFeeCarryOver = 0;
                                        maintenencefeeCarryOver = 0;
                                    }
                                    #endregion
                                }
                                #endregion
                            }
                            else
                            {
                                #region calculate management and maintenance fee amount
                                if (PaymentDetailIn[i].ManagementFee > 0)
                                {
                                    #region
                                    if (PaymentDetailIn[i].ManagementFee > paymentAmount)
                                    {
                                        PaymentDetailIn[i].ManagementFee = paymentAmount;
                                        PaymentDetailIn[i].ManagementFeeCarryOver = ManagementFeeCarryOver;
                                        paymentAmount = 0;
                                    }
                                    else
                                    {
                                        PaymentDetailIn[i].ManagementFeeCarryOver = ManagementFeeCarryOver;
                                        paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[i].ManagementFee));
                                    }
                                    #endregion
                                }
                                if (PaymentDetailIn[i].MaintenanceFee > 0)
                                {
                                    #region
                                    if (PaymentDetailIn[i].MaintenanceFee > paymentAmount)
                                    {
                                        PaymentDetailIn[i].MaintenanceFee = paymentAmount;
                                        PaymentDetailIn[i].MaintenanceFeeCarryOver = maintenencefeeCarryOver;
                                        paymentAmount = 0;
                                    }
                                    else
                                    {
                                        PaymentDetailIn[i].MaintenanceFeeCarryOver = maintenencefeeCarryOver;
                                        paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[i].MaintenanceFee));
                                    }
                                    #endregion
                                }
                                #endregion
                            }
                            #region commented code
                            //else if (remainingManagementFee > paymentAmount)
                            //{
                            //    PaymentDetailIn[i].ManagementFee = paymentAmount;
                            //    paymentAmount = 0;
                            //}
                            //else
                            //{
                            //    PaymentDetailIn[i].ManagementFee = remainingManagementFee;
                            //    paymentAmount = rounndedValue(paymentAmount - remainingManagementFee);
                            //}
                            //#endregion
                            ////=========================================================
                            //#region Calulate maintenance Fee payment
                            //if (Input.InputRecords.FindIndex(o => o.DateIn > PaymentDetailIn[i].PaymentDate && (o.Flags == 0 || o.Flags == 1)) == -1)
                            //{
                            //    PaymentDetailIn[i].MaintenanceFee = remainingMaintenanceFee;
                            //}
                            //else if (remainingMaintenanceFee > paymentAmount)
                            //{
                            //    PaymentDetailIn[i].MaintenanceFee = paymentAmount;
                            //    paymentAmount = 0;
                            //}
                            //else
                            //{
                            //    PaymentDetailIn[i].MaintenanceFee = remainingMaintenanceFee;
                            //    paymentAmount = rounndedValue(paymentAmount - remainingMaintenanceFee);

                            //}
                            #endregion
                            //=========================================================
                            #endregion

                            if (paymentAmount > 0)//When payment amount remains after full payment of schedule
                            {
                                #region
                                #region
                                if (PaymentDetailIn[i].InterestCarryOver > paymentAmount)
                                {
                                    PaymentDetailIn[i].InterestPayment += paymentAmount;
                                    PaymentDetailIn[i].InterestCarryOver -= paymentAmount;
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    PaymentDetailIn[i].InterestPayment += PaymentDetailIn[i].InterestCarryOver;
                                    paymentAmount = rounndedValue(paymentAmount - PaymentDetailIn[i].InterestCarryOver);
                                    PaymentDetailIn[i].InterestCarryOver = 0;

                                }
                                #endregion

                                #region
                                if (PaymentDetailIn[i].serviceFeeInterestCarryOver > paymentAmount)
                                {
                                    PaymentDetailIn[i].ServiceFeeInterest += paymentAmount;
                                    PaymentDetailIn[i].serviceFeeInterestCarryOver -= paymentAmount;
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    PaymentDetailIn[i].ServiceFeeInterest += PaymentDetailIn[i].serviceFeeInterestCarryOver;
                                    paymentAmount = rounndedValue(paymentAmount - PaymentDetailIn[i].serviceFeeInterestCarryOver);
                                    PaymentDetailIn[i].serviceFeeInterestCarryOver = 0;
                                }
                                #endregion

                                #region
                                if (actualBegningPrincipalRemain > paymentAmount)
                                {
                                    PaymentDetailIn[i].PrincipalPayment += paymentAmount;
                                    actualBegningPrincipalRemain = rounndedValue(actualBegningPrincipalRemain - paymentAmount);
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    PaymentDetailIn[i].PrincipalPayment += actualBegningPrincipalRemain;
                                    paymentAmount = rounndedValue(paymentAmount - actualBegningPrincipalRemain);
                                    actualBegningPrincipalRemain = 0;
                                }
                                #endregion

                                #region
                                if (actualBegningServicefeeRemain > paymentAmount)
                                {
                                    PaymentDetailIn[i].ServiceFee += paymentAmount;
                                    actualBegningServicefeeRemain = rounndedValue(actualBegningServicefeeRemain - paymentAmount);
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    PaymentDetailIn[i].ServiceFee += actualBegningServicefeeRemain;
                                    paymentAmount = rounndedValue(paymentAmount - actualBegningServicefeeRemain);
                                    actualBegningServicefeeRemain = 0;
                                }
                                #endregion

                                #region
                                if (PaymentDetailIn[i].ManagementFeeCarryOver > paymentAmount)
                                {
                                    PaymentDetailIn[i].ManagementFee += paymentAmount;
                                    ManagementFeeCarryOver -= paymentAmount;
                                    PaymentDetailIn[i].ManagementFeeCarryOver -= paymentAmount;
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    PaymentDetailIn[i].ManagementFee += PaymentDetailIn[i].ManagementFeeCarryOver;
                                    PaymentDetailIn[i].ManagementFeeCarryOver = 0;
                                    ManagementFeeCarryOver = 0;
                                    paymentAmount = rounndedValue(paymentAmount - PaymentDetailIn[i].ManagementFeeCarryOver);
                                }
                                #endregion

                                #region
                                if (PaymentDetailIn[i].MaintenanceFeeCarryOver > paymentAmount)
                                {
                                    PaymentDetailIn[i].MaintenanceFee += paymentAmount;
                                    maintenencefeeCarryOver -= paymentAmount;
                                    PaymentDetailIn[i].MaintenanceFeeCarryOver -= paymentAmount;
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    PaymentDetailIn[i].MaintenanceFee += PaymentDetailIn[i].MaintenanceFeeCarryOver;
                                    PaymentDetailIn[i].MaintenanceFeeCarryOver = 0;
                                    maintenencefeeCarryOver = 0;
                                    paymentAmount = rounndedValue(paymentAmount - PaymentDetailIn[i].MaintenanceFeeCarryOver);
                                }
                                #endregion

                                #endregion
                            }
                            #endregion
                        }
                        else
                        {
                            #region For Interest payment,principal payment allocation
                            if (Input.InputRecords.FindIndex(o => o.DateIn > PaymentDetailIn[i].PaymentDate && (o.Flags == 0 || o.Flags == 1)) == -1)
                            {
                                PaymentDetailIn[i].InterestPayment = PaymentDetailIn[i].InterestDue;
                                PaymentDetailIn[i].PrincipalPayment = actualBegningPrincipalRemain;
                                PaymentDetailIn[i].InterestCarryOver = 0;
                            }
                            else if (PaymentDetailIn[i].InterestDue >= Output.RegularPayment)
                            {
                                PaymentDetailIn[i].InterestPayment = Output.RegularPayment;
                                PaymentDetailIn[i].InterestCarryOver = PaymentDetailIn[i].InterestDue - PaymentDetailIn[i].InterestPayment;
                                PaymentDetailIn[i].PrincipalPayment = 0;


                            }
                            else
                            {
                                PaymentDetailIn[i].InterestPayment = PaymentDetailIn[i].InterestDue;
                                PaymentDetailIn[i].PrincipalPayment = rounndedValue(Output.RegularPayment - PaymentDetailIn[i].InterestDue);
                                PaymentDetailIn[i].PrincipalPayment = actualBegningPrincipalRemain >= PaymentDetailIn[i].PrincipalPayment ? PaymentDetailIn[i].PrincipalPayment : actualBegningPrincipalRemain;
                                PaymentDetailIn[i].InterestCarryOver = 0;
                            }
                            #region calculate principal payment amount when principal payment is zero and provide enforce principal amount
                            if (i >= Input.EnforcedPayment && PaymentDetailIn[i].PrincipalPayment == 0 && PaymentDetailIn[i].InterestPayment > 0 && i != (PaymentDetailIn.Count - 1))
                            {
                                PaymentDetailIn[i].PrincipalPayment = PaymentDetailIn[i].InterestPayment <= Input.EnforcedPrincipal ? rounndedValue(PaymentDetailIn[i].InterestPayment) : rounndedValue(Input.EnforcedPrincipal);
                                PaymentDetailIn[i].InterestPayment -= PaymentDetailIn[i].PrincipalPayment;
                                PaymentDetailIn[i].InterestCarryOver += PaymentDetailIn[i].PrincipalPayment;
                                PaymentDetailIn[i].PrincipalPayment = PaymentDetailIn[i].PrincipalPayment <= 0 ? 0 : PaymentDetailIn[i].PrincipalPayment;
                                PaymentDetailIn[i].PrincipalPayment = actualBegningPrincipalRemain >= PaymentDetailIn[i].PrincipalPayment ? PaymentDetailIn[i].PrincipalPayment : actualBegningPrincipalRemain;
                            }
                            #endregion
                            actualBegningPrincipalRemain = rounndedValue(actualBegningPrincipalRemain - PaymentDetailIn[i].PrincipalPayment);
                            #endregion
                            //=========================================================
                            #region For ServiceFee interest and service fee allocation
                            if ((Input.InputRecords.FindIndex(o => o.DateIn > PaymentDetailIn[i].PaymentDate && (o.Flags == 0 || o.Flags == 1)) == -1) || actualBegningPrincipalRemain == 0)
                            {
                                PaymentDetailIn[i].ServiceFee = actualBegningServicefeeRemain;
                                PaymentDetailIn[i].serviceFeeInterestCarryOver = 0;
                            }
                            else if (PaymentDetailIn[i].ServiceFeeInterest >= Output.RegularServiceFeePayment)
                            {
                                PaymentDetailIn[i].serviceFeeInterestCarryOver = PaymentDetailIn[i].ServiceFeeInterest - (Input.PaymentAmount > 0 ? paymentAmount : Output.RegularServiceFeePayment);
                                PaymentDetailIn[i].ServiceFeeInterest = Output.RegularServiceFeePayment;
                                PaymentDetailIn[i].ServiceFee = 0;

                            }
                            else
                            {
                                PaymentDetailIn[i].ServiceFee = rounndedValue(Output.RegularServiceFeePayment - PaymentDetailIn[i].ServiceFeeInterest);
                                PaymentDetailIn[i].ServiceFee = actualBegningServicefeeRemain >= PaymentDetailIn[i].ServiceFee ? PaymentDetailIn[i].ServiceFee : actualBegningServicefeeRemain;
                                PaymentDetailIn[i].serviceFeeInterestCarryOver = 0;
                            }
                            actualBegningServicefeeRemain = rounndedValue(actualBegningServicefeeRemain - PaymentDetailIn[i].ServiceFee);

                            #endregion
                            //=========================================================

                        }
                        PaymentDetailIn[i].TotalPayment = PaymentDetailIn[i].PrincipalPayment + PaymentDetailIn[i].InterestPayment + PaymentDetailIn[i].ServiceFeeInterest
                                                            + PaymentDetailIn[i].ServiceFee + PaymentDetailIn[i].OriginationFee + PaymentDetailIn[i].SameDayFee + PaymentDetailIn[i].ManagementFee
                                                            + PaymentDetailIn[i].MaintenanceFee;
                        PaymentDetailIn[i].ServiceFeeTotal = PaymentDetailIn[i].ServiceFee + PaymentDetailIn[i].ServiceFeeInterest;
                        #endregion
                    }
                    #endregion
                }

                additionalPyamentIndex++;
                int j = Input.InputRecords.FindIndex(o => o.DateIn > PaymentDetailIn[PaymentDetailIn.Count - 1].PaymentDate && (o.Flags == 0 || o.Flags == 1));

                if (Input.InputRecords.FindIndex(o => o.DateIn == PaymentDetailIn[PaymentDetailIn.Count - 1].PaymentDate && PaymentDetailIn[PaymentDetailIn.Count - 1].Flags == 2 && (o.Flags == 0 || o.Flags == 1)) != -1)
                {
                    j = Input.InputRecords.FindIndex(o => o.DateIn == PaymentDetailIn[PaymentDetailIn.Count - 1].PaymentDate && (o.Flags == 0 || o.Flags == 1));
                }
                else
                {
                    j = Input.InputRecords.FindIndex(o => o.DateIn > PaymentDetailIn[PaymentDetailIn.Count - 1].PaymentDate && (o.Flags == 0 || o.Flags == 1));
                }
                for (; j <= Input.InputRecords.FindLastIndex(o => o.Flags == 0 || o.Flags == 1) && (actualBegningPrincipalRemain > 0 || actualBegningServicefeeRemain > 0 || PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFeeCarryOver > 0 || PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFeeCarryOver > 0); j++)
                {
                    int k = additionalPyamentIndex;
                    for (; k < Input.AdditionalPaymentRecords.Count; k++)
                    {
                        if (Input.InputRecords[j].DateIn >= Input.AdditionalPaymentRecords[k].DateIn)
                        {
                            CreateNewScheduleDuringSkipped(PaymentDetailIn, Input, k, deletedPaymentDetail, Output, true, ref actualBegningPrincipalRemain, ref actualBegningServicefeeRemain);
                            additionalPyamentIndex++;
                        }
                    }
                    CreateNewScheduleDuringSkipped(PaymentDetailIn, Input, j, deletedPaymentDetail, Output, false, ref actualBegningPrincipalRemain, ref actualBegningServicefeeRemain);
                }
                //This is used for early PayOff when Early PayOff date less then equals to last input schedule date.
                if (PaymentDetailIn[PaymentDetailIn.Count - 1].PaymentDate > Input.EarlyPayoffDate && Input.EarlyPayoffDate > Input.InputRecords[0].DateIn)
                {
                    #region
                    int earlyPayOffIndex = PaymentDetailIn.FindLastIndex(o => o.PaymentDate == Input.EarlyPayoffDate);
                    if (earlyPayOffIndex == -1)
                    {
                        #region When EarlypayOff date does not exist in the output schedule.
                        earlyPayOffIndex = PaymentDetailIn.FindIndex(o => o.PaymentDate > Input.EarlyPayoffDate);
                        DateTime endDate = Input.EarlyPayoffDate;
                        double currentPeriodicInterestRate;
                        scheduleDayDifference = CalculatScheduleDifference(PaymentDetailIn, endDate, earlyPayOffIndex, Input, isMinDuration, DaysPeriod, -1);
                        currentPeriodicInterestRate = CalculatePeriodicInterestRateTier(Input, PaymentDetailIn[earlyPayOffIndex].BeginningPrincipal, scheduleDayDifference);
                        double dailyInterestAmount = (currentPeriodicInterestRate == 0 || scheduleDayDifference == 0) ? 0 : rounndedValue((currentPeriodicInterestRate / scheduleDayDifference) * PaymentDetailIn[earlyPayOffIndex].BeginningPrincipal);
                        double interestAccrued = CalculateCurrentInterestTier(Input, PaymentDetailIn[earlyPayOffIndex].BeginningPrincipal, scheduleDayDifference);
                        double interestDue = interestAccrued + PaymentDetailIn[earlyPayOffIndex == 0 ? earlyPayOffIndex : earlyPayOffIndex - 1].InterestCarryOver;
                        double accruedServiceFeeInterest = IsSrviceFeeApply ? CalculateCurrentServiceFeeInterestTier(Input, PaymentDetailIn[earlyPayOffIndex].BeginningServiceFee, scheduleDayDifference) : 0;

                        double serrviceFeeInterest = accruedServiceFeeInterest + PaymentDetailIn[earlyPayOffIndex == 0 ? earlyPayOffIndex : earlyPayOffIndex - 1].serviceFeeInterestCarryOver;
                        //=====================Used for The serviceFeeIncremental============================
                        serviceFeeIndex = PaymentDetailIn.FindIndex(o => o.PaymentDate >= Input.EarlyPayoffDate);
                        if (Input.IsServiceFeeIncremental && (serviceFeeIndex != -1))
                        {
                            if (PaymentDetailIn[serviceFeeIndex].Flags == 2 && serviceFeeIndex < PaymentDetailIn.Count - 1)
                            {
                                int nextIndex = PaymentDetailIn.FindIndex(o => o.PaymentDate >= PaymentDetailIn[serviceFeeIndex].PaymentDate && (o.Flags == 0 || o.Flags == 9));
                                PaymentDetailIn[earlyPayOffIndex].BeginningServiceFee = rounndedValue(PaymentDetailIn[serviceFeeIndex].ServiceFee + PaymentDetailIn[nextIndex].ServiceFee);
                            }
                            else
                            {
                                PaymentDetailIn[earlyPayOffIndex].BeginningServiceFee = PaymentDetailIn[serviceFeeIndex].ServiceFee;
                            }
                        }
                        //======================================================================================

                        PaymentDetailIn[earlyPayOffIndex].PeriodicInterestRate = currentPeriodicInterestRate;
                        PaymentDetailIn[earlyPayOffIndex].PrincipalPayment = PaymentDetailIn[earlyPayOffIndex].BeginningPrincipal;
                        PaymentDetailIn[earlyPayOffIndex].DailyInterestAmount = dailyInterestAmount;
                        PaymentDetailIn[earlyPayOffIndex].InterestAccrued = interestAccrued;
                        PaymentDetailIn[earlyPayOffIndex].InterestPayment = interestDue;
                        PaymentDetailIn[earlyPayOffIndex].InterestDue = interestDue;
                        PaymentDetailIn[earlyPayOffIndex].ServiceFee = PaymentDetailIn[earlyPayOffIndex].BeginningServiceFee;
                        PaymentDetailIn[earlyPayOffIndex].ServiceFeeInterest = serrviceFeeInterest;
                        PaymentDetailIn[earlyPayOffIndex].ManagementFee = PaymentDetailIn[earlyPayOffIndex - 1].ManagementFeeCarryOver;
                        PaymentDetailIn[earlyPayOffIndex].MaintenanceFee = PaymentDetailIn[earlyPayOffIndex].MaintenanceFeeCarryOver;
                        #endregion
                    }
                    else
                    {
                        if ((Input.AfterPayment) && ((PaymentDetailIn.Count - 1) > earlyPayOffIndex) && (PaymentDetailIn.FindIndex(o => o.PaymentDate == Input.EarlyPayoffDate) != -1))
                        {
                            #region When EarlypayOff date exists in the output schedule and "After payment" checkbox is checked and early payoff date is not last schedule date.
                            for (int x = earlyPayOffIndex + 2; j <= PaymentDetailIn.Count - 1; x++)
                            {
                                PaymentDetailIn[earlyPayOffIndex + 1].OriginationFee += PaymentDetailIn[x].OriginationFee;
                                PaymentDetailIn[earlyPayOffIndex + 1].SameDayFee += PaymentDetailIn[x].SameDayFee;
                            }

                            PaymentDetailIn[earlyPayOffIndex + 1].BeginningPrincipal = rounndedValue(PaymentDetailIn[earlyPayOffIndex].BeginningPrincipal - PaymentDetailIn[earlyPayOffIndex].PrincipalPayment);
                            PaymentDetailIn[earlyPayOffIndex + 1].DailyInterestAmount = 0;
                            PaymentDetailIn[earlyPayOffIndex + 1].InterestAccrued = 0;
                            PaymentDetailIn[earlyPayOffIndex + 1].InterestDue = PaymentDetailIn[earlyPayOffIndex].InterestCarryOver;
                            PaymentDetailIn[earlyPayOffIndex + 1].InterestPayment = PaymentDetailIn[earlyPayOffIndex].InterestCarryOver;
                            PaymentDetailIn[earlyPayOffIndex + 1].BeginningServiceFee = rounndedValue(PaymentDetailIn[earlyPayOffIndex].BeginningServiceFee - PaymentDetailIn[earlyPayOffIndex].ServiceFee);
                            PaymentDetailIn[earlyPayOffIndex + 1].AccruedServiceFeeInterest = 0;
                            PaymentDetailIn[earlyPayOffIndex + 1].ServiceFeeInterest = PaymentDetailIn[earlyPayOffIndex].serviceFeeInterestCarryOver;
                            PaymentDetailIn[earlyPayOffIndex + 1].ServiceFee = PaymentDetailIn[earlyPayOffIndex + 1].BeginningServiceFee;
                            PaymentDetailIn[earlyPayOffIndex + 1].ManagementFee = Input.ManagementFee > 0 ? PaymentDetailIn[earlyPayOffIndex].ManagementFeeCarryOver : 0;
                            PaymentDetailIn[earlyPayOffIndex + 1].MaintenanceFee = Input.MaintenanceFee > 0 ? PaymentDetailIn[earlyPayOffIndex].MaintenanceFeeCarryOver : 0;

                            //=====================Used for The serviceFeeIncremental============================
                            int previousIndex = PaymentDetailIn.FindLastIndex(o => o.PaymentDate == Input.EarlyPayoffDate && o.Flags == 0);
                            serviceFeeIndex = previousIndex != -1 ? -1 : PaymentDetailIn.FindIndex(o => (o.PaymentDate >= Input.EarlyPayoffDate && o.Flags != 2));
                            if (Input.IsServiceFeeIncremental && serviceFeeIndex != -1)
                            {
                                #region

                                PaymentDetailIn[earlyPayOffIndex + 1].BeginningServiceFee = PaymentDetailIn[serviceFeeIndex].ServiceFee;
                                PaymentDetailIn[earlyPayOffIndex + 1].ServiceFee = PaymentDetailIn[serviceFeeIndex].ServiceFee;

                                #endregion
                            }
                            if (Input.IsServiceFeeIncremental && serviceFeeIndex == -1)
                            {
                                PaymentDetailIn[earlyPayOffIndex + 1].ServiceFee = 0;
                                PaymentDetailIn[earlyPayOffIndex + 1].BeginningServiceFee = 0;
                            }
                            //======================================================================================
                            PaymentDetailIn[earlyPayOffIndex + 1].PaymentDate = Input.EarlyPayoffDate;

                            for (int y = PaymentDetailIn.Count - 1; y > earlyPayOffIndex + 1; y--)
                            {
                                PaymentDetailIn.RemoveAt(y);
                            }

                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeInterest = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativeInterest + PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativePrincipal = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativePrincipal + PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativePayment = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].TotalPayment : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativePayment + PaymentDetailIn[PaymentDetailIn.Count - 1].TotalPayment;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeMaintenanceFee = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativeMaintenanceFee + PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeManagementFee = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativeManagementFee + PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeOriginationFee = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].OriginationFee : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativeOriginationFee + PaymentDetailIn[PaymentDetailIn.Count - 1].OriginationFee;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeSameDayFee = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].SameDayFee : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativeSameDayFee + PaymentDetailIn[PaymentDetailIn.Count - 1].SameDayFee;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeServiceFee = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativeServiceFee + PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeServiceFeeInterest = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativeServiceFeeInterest + PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest;

                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeTotalFees = rounndedValue((PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeMaintenanceFee +
                                                                                                           PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeManagementFee +
                                                                                                           PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeOriginationFee +
                                                                                                           PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeSameDayFee +
                                                                                                           PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeServiceFee +
                                                                                                           PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeServiceFeeInterest));
                            earlyPayOffIndex = earlyPayOffIndex + 1;

                            #endregion
                        }
                        else
                        {
                            #region When EarlypayOff date exists in the output schedule and "After payment" checkbox is not checked or the early payoff date is last schedule date and "After payment  checkbox is checked.
                            //=====================Used for The serviceFeeIncremental============================
                            serviceFeeIndex = PaymentDetailIn.FindIndex(o => o.PaymentDate >= Input.EarlyPayoffDate);
                            if (Input.IsServiceFeeIncremental && (serviceFeeIndex != -1))
                            {
                                if (PaymentDetailIn[serviceFeeIndex].Flags == 2 && serviceFeeIndex < PaymentDetailIn.Count - 1)
                                {
                                    int nextIndex = PaymentDetailIn.FindIndex(o => o.PaymentDate >= PaymentDetailIn[serviceFeeIndex].PaymentDate && (o.Flags == 0 || o.Flags == 1 || o.Flags == 9));
                                    PaymentDetailIn[earlyPayOffIndex].BeginningServiceFee = rounndedValue(PaymentDetailIn[serviceFeeIndex].ServiceFee + PaymentDetailIn[nextIndex].ServiceFee);
                                }
                                else
                                {
                                    PaymentDetailIn[earlyPayOffIndex].BeginningServiceFee = PaymentDetailIn[serviceFeeIndex].ServiceFee;
                                }
                            }
                            //======================================================================================
                            int secheduleIndex = Input.InputRecords.FindIndex(o => o.DateIn == Input.EarlyPayoffDate && Input.EarlyPayoffDate > Input.InputRecords[0].DateIn);
                            PaymentDetailIn[earlyPayOffIndex].PrincipalPayment = PaymentDetailIn[earlyPayOffIndex].BeginningPrincipal;
                            PaymentDetailIn[earlyPayOffIndex].InterestPayment += PaymentDetailIn[earlyPayOffIndex].InterestCarryOver;
                            PaymentDetailIn[earlyPayOffIndex].InterestDue = PaymentDetailIn[earlyPayOffIndex].InterestPayment;
                            PaymentDetailIn[earlyPayOffIndex].ServiceFee = PaymentDetailIn[earlyPayOffIndex].BeginningServiceFee;
                            PaymentDetailIn[earlyPayOffIndex].ServiceFeeInterest += PaymentDetailIn[earlyPayOffIndex].serviceFeeInterestCarryOver;
                            PaymentDetailIn[earlyPayOffIndex].ManagementFee = secheduleIndex > 0 ? Input.ManagementFee + PaymentDetailIn[(earlyPayOffIndex > 1 ? earlyPayOffIndex - 1 : earlyPayOffIndex)].ManagementFeeCarryOver : PaymentDetailIn[(earlyPayOffIndex > 1 ? earlyPayOffIndex - 1 : earlyPayOffIndex)].ManagementFeeCarryOver;
                            PaymentDetailIn[earlyPayOffIndex].MaintenanceFee = secheduleIndex > 0 && PaymentDetailIn[earlyPayOffIndex].PaymentDate > Input.InputRecords[1].DateIn ? Input.MaintenanceFee + PaymentDetailIn[earlyPayOffIndex - 1].MaintenanceFeeCarryOver : PaymentDetailIn[earlyPayOffIndex - 1].MaintenanceFeeCarryOver;
                            #endregion
                        }
                    }
                    int addPayCountForSameDate = Input.AdditionalPaymentRecords.Count(o => o.DateIn == Input.EarlyPayoffDate);
                    int paymentdetailInCountForSameDate = PaymentDetailIn.Count(o => o.PaymentDate == Input.EarlyPayoffDate);
                    int inputCountForSameDate = Input.InputRecords.FindIndex(o => o.DateIn == Input.EarlyPayoffDate);

                    if (inputCountForSameDate > 0 && paymentdetailInCountForSameDate > 0 &&
                    ((addPayCountForSameDate > 0 && paymentdetailInCountForSameDate > addPayCountForSameDate) || (addPayCountForSameDate == 0)))
                    {
                        PaymentDetailIn[PaymentDetailIn.FindLastIndex(o => o.PaymentDate == Input.EarlyPayoffDate)].PaymentID = Input.InputRecords[inputCountForSameDate].PaymentID;
                        PaymentDetailIn[PaymentDetailIn.FindLastIndex(o => o.PaymentDate == Input.EarlyPayoffDate)].Flags = Input.InputRecords[inputCountForSameDate].Flags;
                    }
                    else
                    {
                        PaymentDetailIn[earlyPayOffIndex].PaymentID = -1;
                        PaymentDetailIn[earlyPayOffIndex].Flags = 0;
                    }
                    PaymentDetailIn[earlyPayOffIndex].PaymentDate = Input.EarlyPayoffDate;
                    PaymentDetailIn[earlyPayOffIndex].DueDate = Input.EarlyPayoffDate;
                    PaymentDetailIn[earlyPayOffIndex].InterestCarryOver = 0;
                    PaymentDetailIn[earlyPayOffIndex].serviceFeeInterestCarryOver = 0;
                    PaymentDetailIn[earlyPayOffIndex].TotalPayment = PaymentDetailIn[earlyPayOffIndex].PrincipalPayment +
                            PaymentDetailIn[earlyPayOffIndex].InterestDue + PaymentDetailIn[earlyPayOffIndex].ServiceFee +
                            PaymentDetailIn[earlyPayOffIndex].ServiceFeeInterest + PaymentDetailIn[earlyPayOffIndex].OriginationFee +
                            PaymentDetailIn[earlyPayOffIndex].SameDayFee + PaymentDetailIn[earlyPayOffIndex].ManagementFee +
                            PaymentDetailIn[earlyPayOffIndex].MaintenanceFee;

                    for (int i = earlyPayOffIndex + 1; i < PaymentDetailIn.Count;)
                    {
                        PaymentDetailIn.RemoveAt(i);
                    }
                    #endregion
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// This method is used to create new schedule when perform delete operation due to principal and service fee amount remains
        /// </summary>
        /// <param name="PaymentDetailIn"></param>
        /// <param name="Input"></param>
        /// <param name="index"></param>
        /// <param name="deletedPaymentDetail"></param>
        /// <param name="Output"></param>
        /// <param name="forAdditionalPayment"></param>
        /// <param name="actualBegningPrincipal"></param>
        /// <param name="currentBegningServiceFee"></param>
        private static void CreateNewScheduleDuringSkipped(List<PaymentDetail> PaymentDetailIn, getScheduleInput Input, int index, List<PaymentDetail> deletedPaymentDetail,
                getScheduleOutput Output, bool forAdditionalPayment, ref double actualBegningPrincipal, ref double currentBegningServiceFee)
        {
            try
            {
                double ManagementFeeCarryOver = 0;
                double maintenencefeeCarryOver = 0;
                bool isPaymentAmount = false;
                int inputIndex = 0;
                double paymentAmount = 0;

                bool IsSrviceFeeApply = (Input.ApplyServiceFeeInterest == 1 || Input.ApplyServiceFeeInterest == 3) ? true : false;
                DateTime endDate = forAdditionalPayment ? Input.AdditionalPaymentRecords[index].DateIn : Input.InputRecords[index].DateIn;
                int flag = forAdditionalPayment ? 2 : Input.InputRecords[index].Flags;
                int DaysPeriod = getDaysPerPeriod(Input.PmtPeriod);
                bool isMinDuration = Input.MinDuration > 0 ? true : false;

                int scheduleDayDifference = CalculatScheduleDifference(PaymentDetailIn, endDate, PaymentDetailIn.Count, Input, isMinDuration, DaysPeriod, flag);
                double currentPeriodicInterestRate = CalculatePeriodicInterestRateTier(Input, actualBegningPrincipal, scheduleDayDifference);
                double dailyInterestAmount = (currentPeriodicInterestRate == 0 || scheduleDayDifference == 0) ? 0 : rounndedValue((currentPeriodicInterestRate / scheduleDayDifference) * actualBegningPrincipal);
                double interestAccrued = CalculateCurrentInterestTier(Input, actualBegningPrincipal, scheduleDayDifference);
                double interestDue = interestAccrued + PaymentDetailIn[PaymentDetailIn.Count - 1].InterestCarryOver;
                double accruedServiceFeeInterest = IsSrviceFeeApply ? CalculateCurrentServiceFeeInterestTier(Input, currentBegningServiceFee, scheduleDayDifference) : 0;
                double serrviceFeeInterest = accruedServiceFeeInterest + PaymentDetailIn[PaymentDetailIn.Count - 1].serviceFeeInterestCarryOver;

                PaymentDetailIn.Add(new PaymentDetail
                {
                    PaymentDate = endDate,
                    DueDate = forAdditionalPayment ? Input.AdditionalPaymentRecords[index].DateIn : Input.InputRecords[index].EffectiveDate,
                    BeginningPrincipal = actualBegningPrincipal,
                    BeginningServiceFee = currentBegningServiceFee,
                    PeriodicInterestRate = currentPeriodicInterestRate,
                    DailyInterestAmount = dailyInterestAmount,
                    InterestAccrued = interestAccrued,
                    InterestDue = interestDue,
                    AccruedServiceFeeInterest = accruedServiceFeeInterest,
                    ServiceFeeInterest = serrviceFeeInterest
                });

                ManagementFeeCarryOver = PaymentDetailIn[PaymentDetailIn.Count - 2].ManagementFeeCarryOver;

                maintenencefeeCarryOver = PaymentDetailIn[PaymentDetailIn.Count - 2].MaintenanceFeeCarryOver;

                if (forAdditionalPayment)
                {
                    #region  used for additional payment date
                    PaymentDetailIn[PaymentDetailIn.Count - 1].PaymentID = Input.AdditionalPaymentRecords[index].PaymentID;
                    PaymentDetailIn[PaymentDetailIn.Count - 1].Flags = 2;
                    double additionalPaymentAmount = rounndedValue(Input.AdditionalPaymentRecords[index].AdditionalPayment);
                    if (Input.AdditionalPaymentRecords[index].PrincipalOnly)
                    {
                        #region For Principal Only
                        PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment = actualBegningPrincipal > additionalPaymentAmount ? additionalPaymentAmount : actualBegningPrincipal;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment = 0;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].InterestCarryOver = PaymentDetailIn[PaymentDetailIn.Count - 1].InterestDue;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].serviceFeeInterestCarryOver = PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest = 0;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee = 0;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].TotalPayment = PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeTotal = 0;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFeeCarryOver = ManagementFeeCarryOver;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFeeCarryOver = maintenencefeeCarryOver;
                        actualBegningPrincipal = rounndedValue(actualBegningPrincipal - PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment);
                        #endregion
                    }
                    else
                    {
                        #region For NotPrincipal Only Payment
                        if (PaymentDetailIn[PaymentDetailIn.Count - 1].InterestDue >= additionalPaymentAmount)
                        {
                            PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment = additionalPaymentAmount;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].InterestCarryOver = PaymentDetailIn[PaymentDetailIn.Count - 1].InterestDue - additionalPaymentAmount;
                            additionalPaymentAmount = 0;
                        }
                        else
                        {
                            PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment = PaymentDetailIn[PaymentDetailIn.Count - 1].InterestDue;
                            additionalPaymentAmount = rounndedValue(additionalPaymentAmount - PaymentDetailIn[PaymentDetailIn.Count - 1].InterestDue);
                            PaymentDetailIn[PaymentDetailIn.Count - 1].InterestCarryOver = 0;
                        }
                        //========================================================
                        if (PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest >= additionalPaymentAmount)
                        {
                            PaymentDetailIn[PaymentDetailIn.Count - 1].serviceFeeInterestCarryOver = PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest - additionalPaymentAmount;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest = additionalPaymentAmount;
                            additionalPaymentAmount = 0;
                        }
                        else
                        {
                            additionalPaymentAmount = rounndedValue(additionalPaymentAmount - PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest);
                            PaymentDetailIn[PaymentDetailIn.Count - 1].serviceFeeInterestCarryOver = 0;
                        }
                        //========================================================

                        if (actualBegningPrincipal >= additionalPaymentAmount)
                        {
                            PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment = rounndedValue(additionalPaymentAmount);
                            actualBegningPrincipal = rounndedValue(actualBegningPrincipal - additionalPaymentAmount);
                            additionalPaymentAmount = 0;
                        }
                        else
                        {
                            PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment = rounndedValue(actualBegningPrincipal);
                            additionalPaymentAmount = rounndedValue(additionalPaymentAmount - actualBegningPrincipal);
                            actualBegningPrincipal = 0;
                        }
                        //========================================================
                        if (currentBegningServiceFee >= additionalPaymentAmount)
                        {
                            PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee = rounndedValue(additionalPaymentAmount);
                            currentBegningServiceFee = rounndedValue(currentBegningServiceFee - additionalPaymentAmount);
                            additionalPaymentAmount = 0;
                        }
                        else
                        {
                            PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee = rounndedValue(currentBegningServiceFee);
                            additionalPaymentAmount = rounndedValue(additionalPaymentAmount - currentBegningServiceFee);
                            currentBegningServiceFee = 0;
                        }
                        //========================================================
                        #region Calulate management Fee payment
                        if (ManagementFeeCarryOver > additionalPaymentAmount)
                        {
                            PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee = additionalPaymentAmount;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFeeCarryOver = ManagementFeeCarryOver - additionalPaymentAmount;
                            ManagementFeeCarryOver -= additionalPaymentAmount;
                            additionalPaymentAmount = 0;
                        }
                        else
                        {
                            PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee = ManagementFeeCarryOver;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFeeCarryOver = 0;
                            additionalPaymentAmount = rounndedValue(additionalPaymentAmount - ManagementFeeCarryOver);
                            ManagementFeeCarryOver = 0;
                        }
                        #endregion
                        //=========================================================
                        #region Calulate maintenance Fee payment
                        if (maintenencefeeCarryOver > additionalPaymentAmount)
                        {
                            PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee = additionalPaymentAmount;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFeeCarryOver = maintenencefeeCarryOver - additionalPaymentAmount;
                            maintenencefeeCarryOver -= additionalPaymentAmount;
                            additionalPaymentAmount = 0;
                        }
                        else
                        {
                            PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee = maintenencefeeCarryOver;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFeeCarryOver = 0;
                            additionalPaymentAmount = rounndedValue(additionalPaymentAmount - maintenencefeeCarryOver);
                            maintenencefeeCarryOver = 0;
                        }
                        #endregion
                        //=========================================================
                        PaymentDetailIn[PaymentDetailIn.Count - 1].TotalPayment = PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment + PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment + PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest
                                                          + PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee + PaymentDetailIn[PaymentDetailIn.Count - 1].OriginationFee + PaymentDetailIn[PaymentDetailIn.Count - 1].SameDayFee + PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee
                                                          + PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeTotal = PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee + PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest;
                        #endregion
                    }
                    #endregion
                }
                else
                {
                    #region Used for input grid
                    inputIndex = Input.InputRecords.FindIndex(o => o.DateIn == PaymentDetailIn[PaymentDetailIn.Count - 1].PaymentDate && (o.Flags == 0 || o.Flags == 1));
                    PaymentDetailIn[PaymentDetailIn.Count - 1].PaymentID = Input.InputRecords[index].PaymentID;
                    PaymentDetailIn[PaymentDetailIn.Count - 1].Flags = Input.InputRecords[index].Flags;
                    PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee = Input.MaintenanceFee;
                    PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee = Input.ManagementFee;
                    if (endDate == Input.InputRecords[Input.InputRecords.FindLastIndex(o => o.Flags == 0 || o.Flags == 1)].DateIn)
                    {
                        #region When last row input grid
                        PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment = actualBegningPrincipal;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee = currentBegningServiceFee;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment = interestDue;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].InterestCarryOver = 0;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest = serrviceFeeInterest;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].serviceFeeInterestCarryOver = 0;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].TotalPayment = actualBegningPrincipal + currentBegningServiceFee + interestDue + serrviceFeeInterest;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeTotal = currentBegningServiceFee + serrviceFeeInterest;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee = Input.ManagementFee > 0 ? Input.ManagementFee + ManagementFeeCarryOver : 0;
                        PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee = Input.MaintenanceFee > 0 ? Input.MaintenanceFee + maintenencefeeCarryOver : 0;
                        actualBegningPrincipal = 0;
                        currentBegningServiceFee = 0;
                        #endregion
                    }
                    else
                    {
                        #region Otherwise

                        if (!string.IsNullOrEmpty(Input.InputRecords[inputIndex].PaymentAmount) || Input.PaymentAmount > 0)
                        {
                            #region
                            if (!string.IsNullOrEmpty(Input.InputRecords[inputIndex].PaymentAmount))
                            {
                                paymentAmount = rounndedValue(Convert.ToDouble(Input.InputRecords[inputIndex].PaymentAmount));
                                isPaymentAmount = false;
                            }
                            else
                            {
                                isPaymentAmount = true;
                                paymentAmount = rounndedValue(Input.PaymentAmount);
                                ManagementFeeCarryOver += Input.ManagementFee;
                                maintenencefeeCarryOver += inputIndex == 1 ? 0 : Input.MaintenanceFee;
                            }
                            #region For Interest Payment
                            if (PaymentDetailIn[PaymentDetailIn.Count - 1].InterestDue >= paymentAmount)
                            {
                                PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment = paymentAmount;
                                PaymentDetailIn[PaymentDetailIn.Count - 1].InterestCarryOver = PaymentDetailIn[PaymentDetailIn.Count - 1].InterestDue - paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment = PaymentDetailIn[PaymentDetailIn.Count - 1].InterestDue;
                                paymentAmount = rounndedValue(paymentAmount - PaymentDetailIn[PaymentDetailIn.Count - 1].InterestDue);
                                PaymentDetailIn[PaymentDetailIn.Count - 1].InterestCarryOver = 0;
                            }
                            #endregion
                            //========================================================
                            #region For Service Fee Interest
                            if (PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest >= paymentAmount)
                            {
                                PaymentDetailIn[PaymentDetailIn.Count - 1].serviceFeeInterestCarryOver = PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest - paymentAmount;
                                PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                paymentAmount = rounndedValue(paymentAmount - PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest);
                                PaymentDetailIn[PaymentDetailIn.Count - 1].serviceFeeInterestCarryOver = 0;
                            }
                            #endregion
                            //========================================================
                            #region For Principal Payment
                            if (actualBegningPrincipal >= paymentAmount)
                            {
                                PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment = rounndedValue(paymentAmount);
                                actualBegningPrincipal = rounndedValue(actualBegningPrincipal - paymentAmount);
                                paymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment = rounndedValue(actualBegningPrincipal);
                                paymentAmount = rounndedValue(paymentAmount - actualBegningPrincipal);
                                actualBegningPrincipal = 0;
                            }
                            #region calculate principal payment amount when principal payment is zero and provide enforce principal amount
                            if ((PaymentDetailIn.Count - 1) >= Input.EnforcedPayment && PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment == 0 && PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment > 0)
                            {
                                PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment = PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment <= Input.EnforcedPrincipal ? rounndedValue(PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment) : rounndedValue(Input.EnforcedPrincipal);
                                PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment -= PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment;
                                PaymentDetailIn[PaymentDetailIn.Count - 1].InterestCarryOver += PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment;
                                PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment = PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment <= 0 ? 0 : PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment;
                                PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment = actualBegningPrincipal >= PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment ? PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment : actualBegningPrincipal;
                            }
                            #endregion
                            #endregion
                            //========================================================
                            #region For Service Fee 
                            if (currentBegningServiceFee >= paymentAmount)
                            {
                                PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee = rounndedValue(paymentAmount);
                                currentBegningServiceFee = rounndedValue(currentBegningServiceFee - paymentAmount);
                                paymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee = rounndedValue(currentBegningServiceFee);
                                paymentAmount = rounndedValue(paymentAmount - currentBegningServiceFee);
                                currentBegningServiceFee = 0;
                            }
                            #endregion
                            //========================================================
                            #region Calculate Management and Maintenance fee
                            if (isPaymentAmount)
                            {
                                #region calculate management and maintenance fee amount
                                if (ManagementFeeCarryOver > 0)
                                {
                                    #region
                                    if (ManagementFeeCarryOver > paymentAmount)
                                    {
                                        PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee = paymentAmount;
                                        ManagementFeeCarryOver -= paymentAmount;
                                        PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFeeCarryOver = ManagementFeeCarryOver;
                                        paymentAmount = 0;
                                    }
                                    else
                                    {
                                        PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee = ManagementFeeCarryOver;
                                        paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee));
                                        PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFeeCarryOver = 0;
                                        ManagementFeeCarryOver = 0;
                                    }
                                    #endregion
                                }
                                if (maintenencefeeCarryOver > 0)
                                {
                                    #region
                                    if (maintenencefeeCarryOver > paymentAmount)
                                    {
                                        PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee = paymentAmount;
                                        maintenencefeeCarryOver -= paymentAmount;
                                        PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFeeCarryOver = maintenencefeeCarryOver;
                                        paymentAmount = 0;
                                    }
                                    else
                                    {
                                        PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee = maintenencefeeCarryOver;
                                        paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee));
                                        PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFeeCarryOver = 0;
                                        maintenencefeeCarryOver = 0;
                                    }
                                    #endregion
                                }
                                #endregion
                            }
                            else
                            {
                                #region calculate management and maintenance fee amount
                                if (PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee > 0)
                                {
                                    #region
                                    if (PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee > paymentAmount)
                                    {
                                        PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee = paymentAmount;
                                        PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFeeCarryOver = ManagementFeeCarryOver;
                                        paymentAmount = 0;
                                    }
                                    else
                                    {
                                        PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFeeCarryOver = ManagementFeeCarryOver;
                                        paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee));
                                    }
                                    #endregion
                                }
                                if (PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee > 0)
                                {
                                    #region
                                    if (PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee > paymentAmount)
                                    {
                                        PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee = paymentAmount;
                                        PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFeeCarryOver = maintenencefeeCarryOver;
                                        paymentAmount = 0;
                                    }
                                    else
                                    {
                                        PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFeeCarryOver = maintenencefeeCarryOver;
                                        paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee));
                                    }
                                    #endregion
                                }
                                #endregion
                            }
                            //=========================================================
                            if (paymentAmount > 0)//When payment amount remains after full payment of schedule
                            {
                                #region
                                #region
                                if (PaymentDetailIn[PaymentDetailIn.Count - 1].InterestCarryOver > paymentAmount)
                                {
                                    PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment += paymentAmount;
                                    PaymentDetailIn[PaymentDetailIn.Count - 1].InterestCarryOver -= paymentAmount;
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment += PaymentDetailIn[PaymentDetailIn.Count - 1].InterestCarryOver;
                                    paymentAmount = rounndedValue(paymentAmount - PaymentDetailIn[PaymentDetailIn.Count - 1].InterestCarryOver);
                                    PaymentDetailIn[PaymentDetailIn.Count - 1].InterestCarryOver = 0;

                                }
                                #endregion

                                #region
                                if (PaymentDetailIn[PaymentDetailIn.Count - 1].serviceFeeInterestCarryOver > paymentAmount)
                                {
                                    PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest += paymentAmount;
                                    PaymentDetailIn[PaymentDetailIn.Count - 1].serviceFeeInterestCarryOver -= paymentAmount;
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest += PaymentDetailIn[PaymentDetailIn.Count - 1].serviceFeeInterestCarryOver;
                                    paymentAmount = rounndedValue(paymentAmount - PaymentDetailIn[PaymentDetailIn.Count - 1].serviceFeeInterestCarryOver);
                                    PaymentDetailIn[PaymentDetailIn.Count - 1].serviceFeeInterestCarryOver = 0;
                                }
                                #endregion

                                #region
                                if (actualBegningPrincipal > paymentAmount)
                                {
                                    PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment += paymentAmount;
                                    actualBegningPrincipal = rounndedValue(actualBegningPrincipal - paymentAmount);
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment += actualBegningPrincipal;
                                    paymentAmount = rounndedValue(paymentAmount - actualBegningPrincipal);
                                    actualBegningPrincipal = 0;
                                }
                                #endregion

                                #region
                                if (currentBegningServiceFee > paymentAmount)
                                {
                                    PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee += paymentAmount;
                                    currentBegningServiceFee = rounndedValue(currentBegningServiceFee - paymentAmount);
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee += currentBegningServiceFee;
                                    paymentAmount = rounndedValue(paymentAmount - currentBegningServiceFee);
                                    currentBegningServiceFee = 0;
                                }
                                #endregion

                                #region
                                if (PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFeeCarryOver > paymentAmount)
                                {
                                    PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee += paymentAmount;
                                    ManagementFeeCarryOver -= paymentAmount;
                                    PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFeeCarryOver -= paymentAmount;
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee += PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFeeCarryOver;
                                    PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFeeCarryOver = 0;
                                    ManagementFeeCarryOver = 0;
                                    paymentAmount = rounndedValue(paymentAmount - PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFeeCarryOver);
                                }
                                #endregion

                                #region
                                if (PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFeeCarryOver > paymentAmount)
                                {
                                    PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee += paymentAmount;
                                    maintenencefeeCarryOver -= paymentAmount;
                                    PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFeeCarryOver -= paymentAmount;
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee += PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFeeCarryOver;
                                    PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFeeCarryOver = 0;
                                    maintenencefeeCarryOver = 0;
                                    paymentAmount = rounndedValue(paymentAmount - PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFeeCarryOver);
                                }
                                #endregion

                                #endregion
                            }
                            #endregion
                            #endregion
                        }
                        else
                        {
                            #region
                            if (PaymentDetailIn[PaymentDetailIn.Count - 1].InterestDue >= Output.RegularPayment)
                            {
                                PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment = Output.RegularPayment;
                                PaymentDetailIn[PaymentDetailIn.Count - 1].InterestCarryOver = PaymentDetailIn[PaymentDetailIn.Count - 1].InterestDue - PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment;
                                PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment = 0;
                                paymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment = PaymentDetailIn[PaymentDetailIn.Count - 1].InterestDue;
                                PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment = rounndedValue(Output.RegularPayment - rounndedValue(PaymentDetailIn[PaymentDetailIn.Count - 1].InterestDue));
                                PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment = actualBegningPrincipal >= PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment ? PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment : actualBegningPrincipal;
                                PaymentDetailIn[PaymentDetailIn.Count - 1].InterestCarryOver = 0;
                            }
                            #region calculate principal payment amount when principal payment is zero and provide enforce principal amount
                            if ((PaymentDetailIn.Count - 1) >= Input.EnforcedPayment && PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment == 0 && PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment > 0)
                            {
                                PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment = PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment <= Input.EnforcedPrincipal ? rounndedValue(PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment) : rounndedValue(Input.EnforcedPrincipal);
                                PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment -= PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment;
                                PaymentDetailIn[PaymentDetailIn.Count - 1].InterestCarryOver += PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment;
                                PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment = PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment <= 0 ? 0 : PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment;
                                PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment = actualBegningPrincipal >= PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment ? PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment : actualBegningPrincipal;
                            }
                            #endregion
                            actualBegningPrincipal = rounndedValue(actualBegningPrincipal - PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment);
                            if (actualBegningPrincipal == 0)
                            {
                                PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee = currentBegningServiceFee;
                                PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest = serrviceFeeInterest;
                                PaymentDetailIn[PaymentDetailIn.Count - 1].serviceFeeInterestCarryOver = 0;
                                currentBegningServiceFee = 0;
                            }
                            else if (PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest >= Output.RegularServiceFeePayment)
                            {
                                PaymentDetailIn[PaymentDetailIn.Count - 1].serviceFeeInterestCarryOver = PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest - (Input.PaymentAmount > 0 ? paymentAmount : Output.RegularServiceFeePayment);
                                PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest = Output.RegularServiceFeePayment;
                                PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee = 0;
                            }
                            else
                            {
                                PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee = rounndedValue(Output.RegularServiceFeePayment - rounndedValue(PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest));
                                PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee = currentBegningServiceFee >= PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee ? PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee : currentBegningServiceFee;
                                PaymentDetailIn[PaymentDetailIn.Count - 1].serviceFeeInterestCarryOver = 0;
                            }
                            #endregion
                        }
                        currentBegningServiceFee = rounndedValue(currentBegningServiceFee - PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee);

                        PaymentDetailIn[PaymentDetailIn.Count - 1].TotalPayment = PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment + PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment + PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest
                                                          + PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee + PaymentDetailIn[PaymentDetailIn.Count - 1].OriginationFee + PaymentDetailIn[PaymentDetailIn.Count - 1].SameDayFee + PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee
                                                          + PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee;
                        #endregion
                    }

                    if (PaymentDetailIn[PaymentDetailIn.Count - 1].Flags == 1 && deletedPaymentDetail.FindIndex(o => o.PaymentDate == PaymentDetailIn[PaymentDetailIn.Count - 1].PaymentDate) == -1 &&
                        (Input.EarlyPayoffDate <= Input.InputRecords[0].DateIn ? true : PaymentDetailIn[PaymentDetailIn.Count - 1].PaymentDate <= Input.EarlyPayoffDate))
                    {
                        deletedPaymentDetail.Add(PaymentDetailIn[PaymentDetailIn.Count - 1]);
                    }
                    #endregion
                }
            }
            catch
            {

            }
        }


        /// <summary>
        /// This method is used to perform early pay off operation.
        /// </summary>
        /// <param name="IgnoreEarlyPayoffDate"></param>
        /// <param name="PaymentDetailIn"></param>
        /// <param name="Input"></param>
        private static void applyEarlyPayoffDate(bool IgnoreEarlyPayoffDate, List<PaymentDetail> PaymentDetailIn, getScheduleInput Input)
        {
            double getServiceFeeInterest = 0;
            double serviceinterestcarryOver = 0;
            int serviceFeeIndex = -1;
            bool isServiceFeeInterestApplied = (Input.ApplyServiceFeeInterest & 1) == 1;
            int DaysPeriod = getDaysPerPeriod(Input.PmtPeriod);
            bool isMinDuration = Input.MinDuration > 0 ? true : false;
            int scheduleDayDifference;
            int secheduleIndex;

            if ((PaymentDetailIn.Count > 0) && (Input.EarlyPayoffDate >= PaymentDetailIn.Min().PaymentDate && Input.EarlyPayoffDate <= PaymentDetailIn.Max().PaymentDate))
            {
                for (int i = 0; i <= PaymentDetailIn.Count - 1; i++)
                {
                    scheduleDayDifference = CalculatScheduleDifference(PaymentDetailIn, PaymentDetailIn[i].PaymentDate, i, Input, isMinDuration, DaysPeriod, PaymentDetailIn[i].Flags);
                    getServiceFeeInterest = isServiceFeeInterestApplied ? CalculateCurrentServiceFeeInterestTier(Input, PaymentDetailIn[i].BeginningServiceFee, scheduleDayDifference) : 0;
                    getServiceFeeInterest += (i == 0) ? Input.EarnedServiceFeeInterest + serviceinterestcarryOver : serviceinterestcarryOver;

                    if ((PaymentDetailIn[i].ServiceFeeTotal < getServiceFeeInterest) && i != PaymentDetailIn.Count - 1)
                    {
                        serviceinterestcarryOver = getServiceFeeInterest - PaymentDetailIn[i].ServiceFeeTotal;
                    }
                    else
                    {
                        serviceinterestcarryOver = 0;
                    }

                    if (Input.EarlyPayoffDate == PaymentDetailIn[i].PaymentDate && PaymentDetailIn.FindLastIndex(o => o.PaymentDate == Input.EarlyPayoffDate) == i)
                    {
                        #region
                        if ((Input.AfterPayment) && ((PaymentDetailIn.Count - 1) != i) && (PaymentDetailIn.FindIndex(o => o.PaymentDate == Input.EarlyPayoffDate) != -1))
                        {
                            #region
                            for (int j = i + 2; j <= PaymentDetailIn.Count - 1; j++)
                            {
                                PaymentDetailIn[i + 1].OriginationFee += PaymentDetailIn[j].OriginationFee;
                                PaymentDetailIn[i + 1].SameDayFee += PaymentDetailIn[j].SameDayFee;
                            }

                            PaymentDetailIn[i + 1].BeginningPrincipal = rounndedValue(PaymentDetailIn[i].BeginningPrincipal - PaymentDetailIn[i].PrincipalPayment);
                            PaymentDetailIn[i + 1].InterestAccrued = 0;
                            PaymentDetailIn[i + 1].DailyInterestAmount = 0;
                            PaymentDetailIn[i + 1].InterestDue = PaymentDetailIn[i].InterestCarryOver;
                            PaymentDetailIn[i + 1].InterestPayment = PaymentDetailIn[i].InterestCarryOver;
                            PaymentDetailIn[i + 1].BeginningServiceFee = rounndedValue(PaymentDetailIn[i].BeginningServiceFee - PaymentDetailIn[i].ServiceFee);
                            PaymentDetailIn[i + 1].AccruedServiceFeeInterest = 0;
                            PaymentDetailIn[i + 1].ServiceFeeInterest = PaymentDetailIn[i].serviceFeeInterestCarryOver;
                            PaymentDetailIn[i + 1].ManagementFee = Input.ManagementFee > 0 ? PaymentDetailIn[i].ManagementFeeCarryOver : 0;
                            PaymentDetailIn[i + 1].MaintenanceFee = Input.MaintenanceFee > 0 ? PaymentDetailIn[i].MaintenanceFeeCarryOver : 0;

                            //=====================Used for The serviceFeeIncremental============================
                            int previousIndex = PaymentDetailIn.FindLastIndex(o => o.PaymentDate == Input.EarlyPayoffDate && o.Flags == 0);
                            serviceFeeIndex = previousIndex != -1 ? -1 : PaymentDetailIn.FindIndex(o => (o.PaymentDate >= Input.EarlyPayoffDate && o.Flags != 2));
                            if (Input.IsServiceFeeIncremental && serviceFeeIndex != -1)
                            {
                                #region

                                PaymentDetailIn[i + 1].BeginningServiceFee = PaymentDetailIn[serviceFeeIndex].ServiceFee;
                                PaymentDetailIn[i + 1].ServiceFee = rounndedValue(PaymentDetailIn[serviceFeeIndex].ServiceFee);

                                #endregion
                            }
                            if (Input.IsServiceFeeIncremental && serviceFeeIndex == -1)
                            {
                                PaymentDetailIn[i + 1].ServiceFee = 0;
                                PaymentDetailIn[i + 1].BeginningServiceFee = 0;
                            }
                            //======================================================================================
                            if (!Input.IsServiceFeeIncremental)
                            {
                                PaymentDetailIn[i + 1].ServiceFee = rounndedValue(PaymentDetailIn[i + 1].BeginningServiceFee);

                            }
                            PaymentDetailIn[i + 1].ServiceFee = PaymentDetailIn[i + 1].ServiceFee <= 0 ? 0 : PaymentDetailIn[i + 1].ServiceFee;
                            PaymentDetailIn[i + 1].PaymentDate = Input.EarlyPayoffDate;
                            PaymentDetailIn[i + 1].DueDate = Input.EarlyPayoffDate;
                            PaymentDetailIn[i + 1].PeriodicInterestRate = 0;
                            for (int j = PaymentDetailIn.Count - 1; j > i + 1; j--)
                            {
                                PaymentDetailIn.RemoveAt(j);
                            }

                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeInterest = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativeInterest + PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativePrincipal = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativePrincipal + PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativePayment = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].TotalPayment : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativePayment + PaymentDetailIn[PaymentDetailIn.Count - 1].TotalPayment;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeMaintenanceFee = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativeMaintenanceFee + PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeManagementFee = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativeManagementFee + PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeOriginationFee = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].OriginationFee : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativeOriginationFee + PaymentDetailIn[PaymentDetailIn.Count - 1].OriginationFee;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeSameDayFee = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].SameDayFee : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativeSameDayFee + PaymentDetailIn[PaymentDetailIn.Count - 1].SameDayFee;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeServiceFee = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativeServiceFee + PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeServiceFeeInterest = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativeServiceFeeInterest + PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest;

                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeTotalFees = rounndedValue((PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeMaintenanceFee +
                                                                                                           PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeManagementFee +
                                                                                                           PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeOriginationFee +
                                                                                                           PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeSameDayFee +
                                                                                                           PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeServiceFee +
                                                                                                           PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeServiceFeeInterest));
                            i = i + 1;

                            #endregion
                        }
                        else
                        {
                            #region

                            //Changes are done as part of version 1.0.0.24
                            if (PaymentDetailIn[i].InterestDue <= PaymentDetailIn[i].InterestAccrued)
                            {
                                PaymentDetailIn[i].TotalPayment = rounndedValue(PaymentDetailIn[i].BeginningPrincipal + PaymentDetailIn[i].InterestAccrued);
                                PaymentDetailIn[i].InterestCarryOver = 0;
                                PaymentDetailIn[i].InterestPayment = PaymentDetailIn[i].InterestAccrued;
                            }
                            else
                            {
                                PaymentDetailIn[i].TotalPayment = rounndedValue(PaymentDetailIn[i].BeginningPrincipal + PaymentDetailIn[i].InterestDue);
                                PaymentDetailIn[i].InterestPayment = PaymentDetailIn[i].InterestDue;
                                PaymentDetailIn[i].InterestCarryOver = 0;
                            }
                            if (i == 0)
                            {
                                PaymentDetailIn[i].OriginationFee = Input.OriginationFee;
                                PaymentDetailIn[i].SameDayFee = Input.SameDayFee;
                            }
                            else
                            {
                                for (int j = i + 1; j <= PaymentDetailIn.Count - 1; j++)
                                {
                                    PaymentDetailIn[i].OriginationFee += PaymentDetailIn[j].OriginationFee;
                                    PaymentDetailIn[i].SameDayFee += PaymentDetailIn[j].SameDayFee;
                                }
                            }

                            PaymentDetailIn[i].PrincipalPayment = PaymentDetailIn[i].BeginningPrincipal;

                            //=====================Used for The serviceFeeIncremental============================
                            serviceFeeIndex = PaymentDetailIn.FindIndex(o => o.PaymentDate >= Input.EarlyPayoffDate && o.Flags != 2);
                            if ((Input.IsServiceFeeIncremental && serviceFeeIndex != -1) || (i == PaymentDetailIn.Count - 1))
                            {
                                #region
                                if (i == PaymentDetailIn.Count - 1)
                                {
                                    PaymentDetailIn[i].ServiceFee = PaymentDetailIn[i].Flags == 2 ? (PaymentDetailIn[i > 0 ? (i - 1) : i].BeginningServiceFee - PaymentDetailIn[i > 0 ? (i - 1) : i].ServiceFee) : PaymentDetailIn[serviceFeeIndex].BeginningServiceFee;
                                }
                                else
                                {
                                    PaymentDetailIn[i].BeginningServiceFee = PaymentDetailIn[serviceFeeIndex].ServiceFee;
                                    PaymentDetailIn[i].ServiceFee = PaymentDetailIn[serviceFeeIndex].ServiceFee;
                                }
                                #endregion
                            }
                            //======================================================================================

                            for (int j = PaymentDetailIn.Count - 1; j > i; j--)
                            {
                                PaymentDetailIn.RemoveAt(j);
                            }
                            secheduleIndex = Input.InputRecords.FindIndex(o => o.DateIn == Input.EarlyPayoffDate && Input.EarlyPayoffDate > Input.InputRecords[0].DateIn);

                            PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest += serviceinterestcarryOver;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee = PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee <= 0 ? 0 : rounndedValue(PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee);

                            PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeTotal = rounndedValue(PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee + PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest);


                            PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee = secheduleIndex > 0 ? Input.ManagementFee + PaymentDetailIn[(PaymentDetailIn.Count > 1 ? PaymentDetailIn.Count - 2 : PaymentDetailIn.Count - 1)].ManagementFeeCarryOver : PaymentDetailIn[(PaymentDetailIn.Count > 1 ? PaymentDetailIn.Count - 2 : PaymentDetailIn.Count - 1)].ManagementFeeCarryOver;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee = secheduleIndex > 0 && PaymentDetailIn[PaymentDetailIn.Count - 1].PaymentDate > Input.InputRecords[1].DateIn ? Input.MaintenanceFee + PaymentDetailIn[(PaymentDetailIn.Count > 1 ? PaymentDetailIn.Count - 2 : PaymentDetailIn.Count - 1)].MaintenanceFeeCarryOver : PaymentDetailIn[(PaymentDetailIn.Count > 1 ? PaymentDetailIn.Count - 2 : PaymentDetailIn.Count - 1)].MaintenanceFeeCarryOver;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeInterest = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativeInterest + PaymentDetailIn[PaymentDetailIn.Count - 1].InterestPayment;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativePrincipal = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativePrincipal + PaymentDetailIn[PaymentDetailIn.Count - 1].PrincipalPayment;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativePayment = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].TotalPayment : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativePayment + PaymentDetailIn[PaymentDetailIn.Count - 1].TotalPayment;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeMaintenanceFee = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativeMaintenanceFee + PaymentDetailIn[PaymentDetailIn.Count - 1].MaintenanceFee;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeManagementFee = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativeManagementFee + PaymentDetailIn[PaymentDetailIn.Count - 1].ManagementFee;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeOriginationFee = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].OriginationFee : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativeOriginationFee + PaymentDetailIn[PaymentDetailIn.Count - 1].OriginationFee;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeSameDayFee = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].SameDayFee : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativeSameDayFee + PaymentDetailIn[PaymentDetailIn.Count - 1].SameDayFee;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeServiceFee = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativeServiceFee + PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFee;
                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeServiceFeeInterest = (PaymentDetailIn.Count) == 1 ? PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest : PaymentDetailIn[PaymentDetailIn.Count - 2].CumulativeServiceFeeInterest + PaymentDetailIn[PaymentDetailIn.Count - 1].ServiceFeeInterest;

                            PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeTotalFees = rounndedValue((PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeMaintenanceFee +
                                                                                                           PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeManagementFee +
                                                                                                           PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeOriginationFee +
                                                                                                           PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeSameDayFee +
                                                                                                           PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeServiceFee +
                                                                                                           PaymentDetailIn[PaymentDetailIn.Count - 1].CumulativeServiceFeeInterest));


                            #endregion
                        }
                        #endregion
                    }
                    else if (Input.EarlyPayoffDate > PaymentDetailIn[i].PaymentDate && Input.EarlyPayoffDate < PaymentDetailIn[i + 1].PaymentDate)
                    {
                        #region
                        //=====================Used for The serviceFeeIncremental============================
                        serviceFeeIndex = PaymentDetailIn.FindIndex(o => o.PaymentDate >= Input.EarlyPayoffDate && o.Flags != 2);
                        if (Input.IsServiceFeeIncremental && (serviceFeeIndex != -1))
                        {
                            if (serviceFeeIndex == PaymentDetailIn.Count - 1)
                            {
                                PaymentDetailIn[i + 1].ServiceFee = PaymentDetailIn[serviceFeeIndex].BeginningServiceFee;
                            }
                            else
                            {
                                PaymentDetailIn[i + 1].ServiceFee = PaymentDetailIn[serviceFeeIndex].ServiceFee;
                            }
                        }
                        //======================================================================================
                        if (i < PaymentDetailIn.Count - 2)
                        {
                            //Keep the next record (at i + 1) and modify it for the loan payoff.
                            for (int j = PaymentDetailIn.Count - 1; j > i + 1; j--)
                            {
                                PaymentDetailIn.RemoveAt(j);
                            }
                        }

                        // AD : count period between previous and next payments according to schedule

                        PaymentDetailIn[i + 1].PaymentDate = Input.EarlyPayoffDate;
                        PaymentDetailIn[i + 1].DueDate = Input.EarlyPayoffDate;
                        double ShortRate = 0;
                        scheduleDayDifference = CalculatScheduleDifference(PaymentDetailIn, Input.EarlyPayoffDate, i + 1, Input, isMinDuration, DaysPeriod, -1);
                        ShortRate = CalculatePeriodicInterestRateTier(Input, PaymentDetailIn[i + 1].BeginningPrincipal, scheduleDayDifference);
                        PaymentDetailIn[i + 1].PeriodicInterestRate = ShortRate;
                        PaymentDetailIn[i + 1].DailyInterestAmount = (ShortRate == 0 || scheduleDayDifference == 0) ? 0 : rounndedValue((ShortRate / scheduleDayDifference) * PaymentDetailIn[i + 1].BeginningPrincipal);
                        //Changes are done as part of version 1.0.0.24
                        if (PaymentDetailIn[i].InterestCarryOver > 0)
                        {
                            PaymentDetailIn[i + 1].InterestAccrued = CalculateCurrentInterestTier(Input, PaymentDetailIn[i + 1].BeginningPrincipal, scheduleDayDifference); //PaymentDetailIn[i + 1].BeginningPrincipal * ShortRate;
                            PaymentDetailIn[i + 1].InterestDue = PaymentDetailIn[i].InterestCarryOver + PaymentDetailIn[i + 1].InterestAccrued;
                            PaymentDetailIn[i + 1].InterestPayment = PaymentDetailIn[i + 1].InterestDue;
                            PaymentDetailIn[i + 1].InterestCarryOver = 0;
                        }
                        else
                        {
                            PaymentDetailIn[i + 1].InterestAccrued = CalculateCurrentInterestTier(Input, PaymentDetailIn[i + 1].BeginningPrincipal, scheduleDayDifference); //PaymentDetailIn[i + 1].BeginningPrincipal * ShortRate;
                            PaymentDetailIn[i + 1].InterestDue = PaymentDetailIn[i + 1].InterestAccrued;
                            PaymentDetailIn[i + 1].InterestPayment = PaymentDetailIn[i + 1].InterestDue;
                            PaymentDetailIn[i + 1].InterestCarryOver = 0;
                        }
                        PaymentDetailIn[i + 1].PrincipalPayment = PaymentDetailIn[i + 1].BeginningPrincipal;
                        //Changes are done as part of version 1.0.0.24
                        if (PaymentDetailIn[i + 1].InterestAccrued < PaymentDetailIn[i + 1].InterestDue)
                        {
                            PaymentDetailIn[i + 1].TotalPayment = rounndedValue(PaymentDetailIn[i + 1].InterestDue + PaymentDetailIn[i + 1].PrincipalPayment);
                        }
                        else
                        {
                            PaymentDetailIn[i + 1].TotalPayment = rounndedValue(PaymentDetailIn[i].BeginningPrincipal + PaymentDetailIn[i].InterestAccrued);
                        }
                        PaymentDetailIn[i + 1].CumulativePayment = PaymentDetailIn[i].CumulativePayment + PaymentDetailIn[i + 1].TotalPayment;
                        PaymentDetailIn[i + 1].CumulativeInterest = PaymentDetailIn[i].CumulativeInterest + PaymentDetailIn[i + 1].InterestAccrued;
                        PaymentDetailIn[i + 1].CumulativePrincipal = PaymentDetailIn[i].CumulativePrincipal + PaymentDetailIn[i + 1].PrincipalPayment;
                        PaymentDetailIn[i + 1].Flags = 0;

                        //AD 1.0.0.6 - remove additional fees from next payments after payoff
                        PaymentDetailIn[i + 1].ServiceFee = Input.IsServiceFeeIncremental ? rounndedValue(PaymentDetailIn[i + 1].ServiceFee) : 0;
                        PaymentDetailIn[i + 1].ServiceFee = PaymentDetailIn[i + 1].ServiceFee <= 0 ? 0 : PaymentDetailIn[i + 1].ServiceFee;
                        PaymentDetailIn[i + 1].AccruedServiceFeeInterest = isServiceFeeInterestApplied ? CalculateCurrentServiceFeeInterestTier(Input, PaymentDetailIn[i + 1].BeginningServiceFee, scheduleDayDifference) : 0;
                        PaymentDetailIn[i + 1].ServiceFeeInterest = serviceinterestcarryOver + PaymentDetailIn[i + 1].AccruedServiceFeeInterest;
                        PaymentDetailIn[i + 1].BeginningServiceFee = Input.IsServiceFeeIncremental ? PaymentDetailIn[serviceFeeIndex].ServiceFee : PaymentDetailIn[i + 1].BeginningServiceFee;
                        PaymentDetailIn[i + 1].MaintenanceFee = PaymentDetailIn[i].MaintenanceFeeCarryOver;
                        PaymentDetailIn[i + 1].ManagementFee = PaymentDetailIn[i].ManagementFeeCarryOver;
                        PaymentDetailIn[i + 1].ServiceFeeTotal = 0;
                        PaymentDetailIn[i + 1].CumulativeMaintenanceFee = PaymentDetailIn[i].CumulativeMaintenanceFee + PaymentDetailIn[i + 1].MaintenanceFee;
                        PaymentDetailIn[i + 1].CumulativeManagementFee = PaymentDetailIn[i].CumulativeManagementFee + PaymentDetailIn[i + 1].ManagementFee;
                        PaymentDetailIn[i + 1].CumulativeOriginationFee = PaymentDetailIn[i].CumulativeOriginationFee + PaymentDetailIn[i + 1].OriginationFee;
                        PaymentDetailIn[i + 1].CumulativeSameDayFee = PaymentDetailIn[i].CumulativeSameDayFee + PaymentDetailIn[i + 1].SameDayFee;
                        PaymentDetailIn[i + 1].CumulativeServiceFee = PaymentDetailIn[i].CumulativeServiceFee + PaymentDetailIn[i + 1].ServiceFee;
                        PaymentDetailIn[i + 1].CumulativeServiceFeeInterest = PaymentDetailIn[i].CumulativeServiceFeeInterest + PaymentDetailIn[i + 1].ServiceFeeInterest;

                        PaymentDetailIn[i + 1].CumulativeTotalFees = rounndedValue((PaymentDetailIn[i + 1].CumulativeMaintenanceFee +
                                                                                                       PaymentDetailIn[i + 1].CumulativeManagementFee +
                                                                                                       PaymentDetailIn[i + 1].CumulativeOriginationFee +
                                                                                                       PaymentDetailIn[i + 1].CumulativeSameDayFee +
                                                                                                       PaymentDetailIn[i + 1].CumulativeServiceFee +
                                                                                                       PaymentDetailIn[i + 1].CumulativeServiceFeeInterest));
                        i = i + 1;
                        #endregion
                    }
                }
                PaymentDetailIn[PaymentDetailIn.Count - 1].PaymentID = -1;
                PaymentDetailIn[PaymentDetailIn.Count - 1].Flags = 0;
            }
            else if (!IgnoreEarlyPayoffDate && (PaymentDetailIn.Count == 0 || Input.EarlyPayoffDate < PaymentDetailIn.Min().PaymentDate))
            {
                // AD : this block is added to apply Early Payoff Date before or on first payment date, or payment dates are not entered
                // AD : if Early Payoff Date is ignored this block should not be performed
                #region
                double PeriodicIntRateOnDailyBasis = 0;
                double incrementalBegnningServiceFee = 0;
                double incrementalServiceFee = 0;

                scheduleDayDifference = CalculatScheduleDifference(PaymentDetailIn, Input.EarlyPayoffDate, 0, Input, isMinDuration, DaysPeriod, -1);
                PeriodicIntRateOnDailyBasis = CalculatePeriodicInterestRateTier(Input, Input.LoanAmount, scheduleDayDifference);

                //=====================Used for The serviceFeeIncremental============================
                serviceFeeIndex = PaymentDetailIn.FindIndex(o => o.PaymentDate >= Input.EarlyPayoffDate && o.Flags != 2);
                if (Input.IsServiceFeeIncremental && (serviceFeeIndex != -1))
                {
                    incrementalBegnningServiceFee = PaymentDetailIn[serviceFeeIndex].ServiceFee;
                    incrementalServiceFee = PaymentDetailIn[serviceFeeIndex].ServiceFee;
                }
                //======================================================================================

                double BeginningPrincipal = Input.LoanAmount;
                double dailyInterestAmount = rounndedValue((PeriodicIntRateOnDailyBasis / scheduleDayDifference) * BeginningPrincipal);
                double InterestPayment = CalculateCurrentInterestTier(Input, BeginningPrincipal, scheduleDayDifference);
                double serviceFeeInterest = isServiceFeeInterestApplied ? CalculateCurrentServiceFeeInterestTier(Input, PaymentDetailIn[0].BeginningServiceFee, scheduleDayDifference) : 0;

                InterestPayment += Input.EarnedInterest;
                serviceFeeInterest += Input.EarnedServiceFeeInterest;


                PaymentDetail earlyPayoffPaymentDetail = new PaymentDetail()
                {
                    BeginningPrincipal = Input.LoanAmount,
                    BeginningServiceFee = rounndedValue(PaymentDetailIn[0].BeginningServiceFee),
                    InterestDue = InterestPayment,
                    InterestPayment = InterestPayment,
                    CumulativeInterest = InterestPayment,
                    InterestCarryOver = 0,
                    CumulativePrincipal = Input.LoanAmount,
                    CumulativePayment = rounndedValue(Input.LoanAmount + InterestPayment),
                    Flags = 0,
                    DailyInterestAmount = dailyInterestAmount,
                    InterestAccrued = InterestPayment,
                    PaymentDate = Input.EarlyPayoffDate,
                    DueDate = Input.EarlyPayoffDate,
                    PaymentID = -1,
                    PeriodicInterestRate = PeriodicIntRateOnDailyBasis,
                    PrincipalPayment = Input.LoanAmount,
                    TotalPayment = Input.LoanAmount + InterestPayment,
                    ServiceFee = rounndedValue(PaymentDetailIn[0].BeginningServiceFee),
                    AccruedServiceFeeInterest = serviceFeeInterest,
                    ServiceFeeInterest = serviceFeeInterest,
                    ServiceFeeTotal = rounndedValue(PaymentDetailIn[0].BeginningServiceFee + serviceFeeInterest),
                    OriginationFee = Input.OriginationFee,
                    SameDayFee = Input.SameDayFee,
                    ManagementFee = 0
                };

                // clear PaymentDetailIn and add the payment on Early Payoff Date
                PaymentDetailIn.Clear();
                PaymentDetailIn.Add(earlyPayoffPaymentDetail);
                //=====================Used for The serviceFeeIncremental============================
                if (Input.IsServiceFeeIncremental && (serviceFeeIndex != -1))
                {
                    PaymentDetailIn[0].BeginningServiceFee = incrementalBegnningServiceFee;
                    PaymentDetailIn[0].ServiceFee = incrementalServiceFee;
                }
                #endregion
            }
        }

        /// <summary>
        /// This method is used for additional payment operation and then reschedule the remaining schedule after additional payment.
        /// </summary>
        /// <param name="PaymentDetailIn"></param>
        /// <param name="PeriodicInterestRate"></param>
        /// <param name="isRegularPaymentRounded"></param>
        /// <param name="Input"></param>
        private static void adjustForAdditionalPayments(List<PaymentDetail> PaymentDetailIn, double PeriodicInterestRate, bool isRegularPaymentRounded, getScheduleInput Input)
        {
            double CurrentPrincipal = rounndedValue(Input.LoanAmount);
            // AD - 1.0.0.7 - add service fee variables
            bool isServiceFeeInterestApplied = (Input.ApplyServiceFeeInterest & 1) == 1;
            double CurrentServiceFee = rounndedValue(Input.LoanAmount);
            DateTime AdditionalPaymentDate;
            int PaymentDetailIndex = 0;
            Boolean FirstOne;
            double remainingAmount = 0;
            double dailyInterestAmount = 0;
            double InterestAccrued = 0;
            double InterestDue = 0;
            double InterestPaymentPaid = 0;
            double InterestCarryOver = 0;
            double serviceinterestcarryOver = 0;
            double getServiceFee = 0;
            double getOriginationFee = 0;
            double getSameDayFee = 0;
            int scheduleDayDifference = 0;
            double getAdditionalServiceFeeInterest = 0;
            double additionalPaymentTotal = 0;
            double additionalPaymentServiceFee = 0;
            double additionalPaymentPrincipal = 0;
            double getAdditionalPaymentPrincipal = 0;
            double periodicRateForPeriod = 0;
            double PreviousAdditionalPaymentSum = 0;
            double PreviousServiceFeeAdditionalPaymentSum = 0;
            int DaysPeriod = getDaysPerPeriod(Input.PmtPeriod);
            int inputdateIndex = 0;
            double paymentAmount = 0;
            bool isPaymentAmount = false;
            double managementFee = 0;
            double ManagementFeeCarryOver = 0;
            double maintenanceFee = 0;
            double maintenencefeeCarryOver = 0;
            int indexCount;
            bool isMinDuration = Input.MinDuration != 0 ? true : false;
            /*
             * Find Origionation fee amount where the first schedule calculated if flexible method is checked and assign to a viriable 
             * which is used for next time.
             */

            for (int i = 0; i < Input.AdditionalPaymentRecords.Count; i++)
            {
                double periodicInterestRate = 0;
                serviceinterestcarryOver = 0;
                PaymentDetailIndex = 0;
                AdditionalPaymentDate = Input.AdditionalPaymentRecords[i].DateIn;

                // AD : first check that PaymentDetailIndex exists in PaymentDetailIn
                // and check element only if PaymentDetailIndex is correct.
                // changed order of checking in &&

                // AD 1.0.0.7 - divide additional payment amount between principal and service fee
                additionalPaymentTotal = Input.AdditionalPaymentRecords[i].AdditionalPayment;
                additionalPaymentServiceFee = 0;
                additionalPaymentPrincipal = rounndedValue(additionalPaymentTotal);

                // AD 1.0.0.7 - Additional payment should go after scheduled payment if occur on the same date
                // AdditionalPaymentDate >= PaymentDetailIn[PaymentDetailIndex].PaymentDate instead of >
                while (PaymentDetailIndex < PaymentDetailIn.Count && AdditionalPaymentDate >= PaymentDetailIn[PaymentDetailIndex].PaymentDate)
                {
                    #region
                    if ((PaymentDetailIndex == PaymentDetailIn.FindLastIndex(o => o.PaymentDate == AdditionalPaymentDate && (o.Flags != 9))) &&
                                                    AdditionalPaymentDate == PaymentDetailIn[PaymentDetailIndex].PaymentDate)
                    {
                        PaymentDetailIndex++;
                        break;
                    }
                    PaymentDetailIndex = PaymentDetailIndex + 1;
                    #endregion
                }

                FirstOne = true;
                while (PaymentDetailIndex < PaymentDetailIn.Count) //Need to update payments while incrementing index.
                {
                    periodicInterestRate = PaymentDetailIn[PaymentDetailIndex].PeriodicInterestRate;
                    if ((PaymentDetailIn[PaymentDetailIndex].Flags & 2) == 2)
                    {
                        PreviousAdditionalPaymentSum = PreviousAdditionalPaymentSum + PaymentDetailIn[PaymentDetailIndex].TotalPayment;
                        PreviousServiceFeeAdditionalPaymentSum = PreviousServiceFeeAdditionalPaymentSum + PaymentDetailIn[PaymentDetailIndex].ServiceFeeTotal;
                    }
                    else
                    {
                        if (FirstOne)
                        //The additional payment record is inserted here. After the first one, payment records just need to be adjusted.
                        {
                            #region
                            FirstOne = false;

                            #region //Calculate interestAccrued for Additional payment if Flexible or Rigid checkbox is true

                            if (PaymentDetailIndex <= 0 && PeriodicInterestRate >= 0)//calculate Interest Accrued if additional payment date comes before the first scheduled payment date and periodicInterestRate should be greater then zero.
                            {
                                #region
                                /*
                                 * Here first find the total days from effective date to first scheduled payment date.
                                 * calculate the days difference b/w effective date and additional payment date.
                                 * calculate periodicInterestRate from effective date to additional payment date.
                                 * then calculate Interest Accrued amount for additional payment.
                                 */
                                scheduleDayDifference = CalculatScheduleDifference(PaymentDetailIn, AdditionalPaymentDate, PaymentDetailIndex, Input, isMinDuration, DaysPeriod, 2);
                                periodicRateForPeriod = CalculatePeriodicInterestRateTier(Input, PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal, scheduleDayDifference);

                                periodicInterestRate = Math.Round(periodicInterestRate, 15, MidpointRounding.AwayFromZero);
                                if (!Input.UseFlexibleCalculation && (!isMinDuration || (isMinDuration && PaymentDetailIn.FindLastIndex(o => o.PaymentDate == Input.InputRecords[1].DateIn && o.Flags == 0) < PaymentDetailIndex)))
                                {
                                    periodicRateForPeriod = (periodicInterestRate > periodicRateForPeriod) ? periodicRateForPeriod : periodicInterestRate;
                                    periodicInterestRate = (periodicInterestRate > periodicRateForPeriod) ? (periodicInterestRate - periodicRateForPeriod) : 0;
                                }
                                dailyInterestAmount = (periodicRateForPeriod == 0 || scheduleDayDifference == 0) ? 0 : rounndedValue((periodicRateForPeriod / scheduleDayDifference) * PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal);
                                InterestAccrued = CalculateCurrentInterestTier(Input, PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal, scheduleDayDifference);
                                getAdditionalServiceFeeInterest = CalculateCurrentServiceFeeInterestTier(Input, PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee, scheduleDayDifference);
                                PaymentDetailIn[PaymentDetailIndex].AccruedServiceFeeInterest = CalculateCurrentServiceFeeInterestTier(Input, PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee, scheduleDayDifference);
                                #endregion
                            }
                            else
                            {
                                #region
                                /*
                                 * calculate Interest Accrued if additional payment date comes after the first scheduled payment date and periodicInterestRate should be greater then zero.
                                 * Here first find the total days from date1 to date2.
                                 * calculate the days difference b/w date1 to additional payment date.
                                 * calculate periodicInterestRate from date1 to additional payment date.
                                 * then calculate Interest Accrued amount for additional payment.
                                 */
                                if (PeriodicInterestRate >= 0)
                                {
                                    scheduleDayDifference = CalculatScheduleDifference(PaymentDetailIn, AdditionalPaymentDate, PaymentDetailIndex, Input, isMinDuration, DaysPeriod, 2);
                                    periodicRateForPeriod = CalculatePeriodicInterestRateTier(Input, PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal, scheduleDayDifference);
                                    periodicInterestRate = Math.Round(periodicInterestRate, 15, MidpointRounding.AwayFromZero);

                                    if (!Input.UseFlexibleCalculation && (!isMinDuration || (isMinDuration && PaymentDetailIn.FindLastIndex(o => o.PaymentDate == Input.InputRecords[1].DateIn && o.Flags == 0) < PaymentDetailIndex)))
                                    {
                                        periodicRateForPeriod = (periodicInterestRate > periodicRateForPeriod) ? periodicRateForPeriod : periodicInterestRate;
                                        periodicInterestRate = (periodicInterestRate > periodicRateForPeriod) ? (periodicInterestRate - periodicRateForPeriod) : 0;
                                    }
                                    dailyInterestAmount = (periodicRateForPeriod == 0 || scheduleDayDifference == 0) ? 0 : rounndedValue((periodicRateForPeriod / scheduleDayDifference) * PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal);
                                    InterestAccrued = CalculateCurrentInterestTier(Input, PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal, scheduleDayDifference);
                                    getAdditionalServiceFeeInterest = PaymentDetailIn[PaymentDetailIndex - 1].serviceFeeInterestCarryOver + CalculateCurrentServiceFeeInterestTier(Input, PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee, scheduleDayDifference);
                                    PaymentDetailIn[PaymentDetailIndex].AccruedServiceFeeInterest = CalculateCurrentServiceFeeInterestTier(Input, PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee, scheduleDayDifference);
                                }
                                #endregion
                            }
                            #endregion

                            #region //Calculate InterestAccrued,InterestDue,InterestCarryOver,InterestPaidPayment and Origination fee for Additional payment

                            if (Input.AdditionalPaymentRecords[i].PrincipalOnly)
                            {
                                #region This is used for Earned interest and earned serviceFeeInterest
                                if (PaymentDetailIndex <= 0)
                                {
                                    InterestAccrued += Input.EarnedInterest;
                                    getAdditionalServiceFeeInterest += Input.EarnedServiceFeeInterest;
                                }
                                #endregion
                                serviceinterestcarryOver = getAdditionalServiceFeeInterest;
                                #region
                                getOriginationFee = 0;
                                getAdditionalServiceFeeInterest = 0;
                                getSameDayFee = 0;
                                getServiceFee = 0;
                                managementFee = 0;
                                maintenanceFee = 0;
                                /*
                                * If PaymentDetailIndex is zero and additional payment date is less then first scheduled date.
                                */
                                if (PaymentDetailIndex <= 0)
                                {
                                    #region   
                                    maintenencefeeCarryOver = 0;
                                    ManagementFeeCarryOver = 0;
                                    InterestDue = InterestAccrued + InterestCarryOver;
                                    if (rounndedValue(additionalPaymentPrincipal) > rounndedValue(PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal))
                                    {
                                        additionalPaymentPrincipal = rounndedValue(PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal);
                                        InterestPaymentPaid = 0;
                                        InterestCarryOver = InterestDue;
                                    }
                                    else
                                    {
                                        getAdditionalPaymentPrincipal = rounndedValue(additionalPaymentPrincipal);
                                        InterestPaymentPaid = 0;
                                        InterestCarryOver = InterestDue;
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region
                                    maintenencefeeCarryOver = PaymentDetailIn[PaymentDetailIndex - 1].MaintenanceFeeCarryOver;
                                    ManagementFeeCarryOver = PaymentDetailIn[PaymentDetailIndex - 1].ManagementFeeCarryOver;
                                    /*
                                    * If PaymentDetailIndex is not zero and additional payment date is greater then first scheduled date.
                                    */
                                    if (rounndedValue(additionalPaymentPrincipal) > rounndedValue(PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal))
                                    {
                                        additionalPaymentPrincipal = rounndedValue(PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal);
                                        InterestDue = InterestAccrued + PaymentDetailIn[PaymentDetailIndex - 1].InterestCarryOver;
                                        InterestPaymentPaid = 0;
                                        InterestCarryOver = InterestDue;
                                    }
                                    else
                                    {
                                        getAdditionalPaymentPrincipal = rounndedValue(additionalPaymentPrincipal);
                                        InterestPaymentPaid = 0;
                                        InterestDue = InterestAccrued + PaymentDetailIn[PaymentDetailIndex - 1].InterestCarryOver;
                                        InterestCarryOver = InterestDue;
                                    }
                                    #endregion
                                }

                                #endregion
                            }
                            else
                            {
                                #region
                                /*
                                 * If PrincipleOnly is false and PaymentDetailIndex<=0 means calculate origination fee first then accrued interest and then principle amount.
                                */
                                if (PaymentDetailIndex <= 0)
                                {
                                    #region
                                    #region
                                    InterestAccrued += Input.EarnedInterest;
                                    getAdditionalServiceFeeInterest += Input.EarnedServiceFeeInterest;
                                    #endregion
                                    managementFee = 0;
                                    maintenanceFee = 0;
                                    ManagementFeeCarryOver = 0;
                                    maintenencefeeCarryOver = 0;
                                    InterestDue = InterestAccrued + InterestCarryOver;

                                    #region //Calculate origionation fee if PaymentDetailIndex<=0 and PaymentDetailIn[0].OriginationFee < additionalPaymentPrincipal.

                                    if (Input.OriginationFee < additionalPaymentPrincipal)
                                    {
                                        getOriginationFee = rounndedValue(Input.OriginationFee);
                                        additionalPaymentPrincipal = rounndedValue(additionalPaymentPrincipal - Input.OriginationFee);
                                        getAdditionalPaymentPrincipal = rounndedValue(additionalPaymentPrincipal);
                                        PaymentDetailIn[0].OriginationFee = 0;
                                    }
                                    else
                                    {
                                        /*
                                         * Calculate origionation fee if PaymentDetailIndex is zero and PaymentDetailIn[0].OriginationFee > additionalPaymentPrincipal.
                                         */
                                        PaymentDetailIn[0].OriginationFee = rounndedValue(Input.OriginationFee - additionalPaymentPrincipal);
                                        getOriginationFee = rounndedValue(additionalPaymentPrincipal);
                                        additionalPaymentPrincipal = 0;
                                        getAdditionalPaymentPrincipal = rounndedValue(additionalPaymentPrincipal);
                                    }

                                    #endregion

                                    #region //Calculate samedayFee fee if PaymentDetailIndex<=0 and PaymentDetailIn[0].SameDayFee < additionalPaymentPrincipal.

                                    if (Input.SameDayFee < additionalPaymentPrincipal)
                                    {
                                        getSameDayFee = rounndedValue(Input.SameDayFee);
                                        additionalPaymentPrincipal = rounndedValue(additionalPaymentPrincipal - Input.SameDayFee);
                                        getAdditionalPaymentPrincipal = rounndedValue(additionalPaymentPrincipal);
                                        PaymentDetailIn[0].SameDayFee = 0;
                                    }
                                    else
                                    {
                                        /*
                                         * Calculate samedayFee fee if PaymentDetailIndex is zero and PaymentDetailIn[0].SameDayFee > additionalPaymentPrincipal.
                                         */
                                        PaymentDetailIn[0].SameDayFee = rounndedValue(Input.SameDayFee - additionalPaymentPrincipal);
                                        getSameDayFee = rounndedValue(additionalPaymentPrincipal);
                                        additionalPaymentPrincipal = 0;
                                        getAdditionalPaymentPrincipal = rounndedValue(additionalPaymentPrincipal);
                                    }

                                    #endregion

                                    #region //Calculate InterestPaymentPaid,InterestDue,InterestCarryOver,CumulativeInterest if InterestDue > additionalPaymentPrincipal.

                                    if (InterestDue > additionalPaymentPrincipal)
                                    {
                                        InterestPaymentPaid = rounndedValue(additionalPaymentPrincipal);
                                        InterestCarryOver = InterestDue - additionalPaymentPrincipal;
                                        additionalPaymentPrincipal = 0;
                                    }
                                    else
                                    {
                                        /*
                                        * Calculate InterestPaymentPaid,InterestDue,InterestCarryOver,CumulativeInterest if InterestDue < additionalPaymentPrincipal.
                                        */
                                        InterestPaymentPaid = rounndedValue(InterestDue);
                                        additionalPaymentPrincipal = rounndedValue(additionalPaymentPrincipal - InterestPaymentPaid);
                                        InterestCarryOver = 0;
                                    }

                                    #endregion

                                    #region //Calculate ServiceFeeInterest if additionalPaymentPrincipal>0.

                                    if (rounndedValue(getAdditionalServiceFeeInterest) > rounndedValue(additionalPaymentPrincipal))
                                    {
                                        serviceinterestcarryOver = getAdditionalServiceFeeInterest - additionalPaymentPrincipal;
                                        getAdditionalServiceFeeInterest = additionalPaymentPrincipal;
                                        getAdditionalPaymentPrincipal = rounndedValue(additionalPaymentPrincipal);
                                        additionalPaymentPrincipal = 0;
                                    }
                                    else
                                    {
                                        /*
                                        * Calculate ServiceFeeInterest if additionalPaymentPrincipal<0.
                                        */
                                        additionalPaymentPrincipal = rounndedValue(additionalPaymentPrincipal - getAdditionalServiceFeeInterest);
                                        getAdditionalPaymentPrincipal = rounndedValue(additionalPaymentPrincipal);
                                        serviceinterestcarryOver = 0;
                                    }
                                    #endregion

                                    #region //Calculate principle if additionalPaymentPrincipal>0.

                                    if (additionalPaymentPrincipal > 0)
                                    {
                                        if (PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal > additionalPaymentPrincipal)
                                        {
                                            remainingAmount = 0;
                                            getAdditionalPaymentPrincipal = 0;
                                        }
                                        else
                                        {
                                            /*
                                            * Calculate Principle if additionalPaymentPrincipal<0.
                                            */
                                            remainingAmount = additionalPaymentPrincipal - PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal;
                                            additionalPaymentPrincipal = PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal;
                                            getAdditionalPaymentPrincipal = rounndedValue(remainingAmount);
                                        }
                                    }

                                    #endregion

                                    #region //Calculate serviceFee if additionalPaymentPrincipal>0.

                                    if (remainingAmount > 0)
                                    {
                                        if (PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee > remainingAmount)
                                        {
                                            getServiceFee = remainingAmount;
                                            remainingAmount = 0;
                                            getAdditionalPaymentPrincipal = 0;
                                        }
                                        else
                                        {
                                            /*
                                            * Calculate serviceFee if additionalPaymentPrincipal<0.
                                            */
                                            getServiceFee = PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee;
                                            remainingAmount = PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee - remainingAmount;
                                            getAdditionalPaymentPrincipal = 0;
                                        }
                                    }

                                    #endregion

                                    #endregion
                                }
                                else
                                {
                                    #region
                                    /*
                                    * If PrincipleOnly is false and PaymentDetailIndex>=0 means calculate origination fee first then accrued interest and then principle amount.
                                    */
                                    getOriginationFee = 0;
                                    getSameDayFee = 0;
                                    int count = 0;
                                    InterestDue = InterestAccrued + PaymentDetailIn[PaymentDetailIndex - 1].InterestCarryOver;
                                    //Find first schedule index where origination fee are exist
                                    for (int k = 0; k < PaymentDetailIn.Count; k++)
                                    {
                                        if (PaymentDetailIn[k].Flags != 2)
                                        {
                                            count = k;
                                            break;
                                        }
                                    }

                                    #region //Calculate origionation fee if PaymentDetailIndex>=0 and PaymentDetailIn[count].OriginationFee < additionalPaymentPrincipal

                                    if (PaymentDetailIn[count].PaymentDate > Input.AdditionalPaymentRecords[i].DateIn)
                                    {
                                        if (PaymentDetailIn[count].OriginationFee < additionalPaymentPrincipal)
                                        {
                                            getOriginationFee = rounndedValue(PaymentDetailIn[count].OriginationFee);
                                            additionalPaymentPrincipal = rounndedValue(additionalPaymentPrincipal - PaymentDetailIn[count].OriginationFee);
                                            getAdditionalPaymentPrincipal = rounndedValue(additionalPaymentPrincipal);
                                            PaymentDetailIn[count].OriginationFee = 0;
                                        }
                                        else
                                        {
                                            /*
                                            *Calculate origionation fee if PaymentDetailIndex>=0 and PaymentDetailIn[count].OriginationFee > additionalPaymentPrincipal
                                            */
                                            getOriginationFee = rounndedValue(additionalPaymentPrincipal);
                                            PaymentDetailIn[count].OriginationFee = rounndedValue(PaymentDetailIn[count].OriginationFee - additionalPaymentPrincipal);
                                            additionalPaymentPrincipal = 0;
                                            getAdditionalPaymentPrincipal = rounndedValue(additionalPaymentPrincipal);
                                        }
                                    }

                                    #endregion

                                    #region //Calculate samedayFee fee if PaymentDetailIndex>=0 and PaymentDetailIn[count].SameDayFee < additionalPaymentPrincipal

                                    if (PaymentDetailIn[count].PaymentDate > Input.AdditionalPaymentRecords[i].DateIn)
                                    {
                                        if (PaymentDetailIn[count].SameDayFee < additionalPaymentPrincipal)
                                        {
                                            getSameDayFee = rounndedValue(PaymentDetailIn[count].SameDayFee);
                                            additionalPaymentPrincipal = rounndedValue(additionalPaymentPrincipal - PaymentDetailIn[count].SameDayFee);
                                            getAdditionalPaymentPrincipal = rounndedValue(additionalPaymentPrincipal);
                                            PaymentDetailIn[count].SameDayFee = 0;
                                        }
                                        else
                                        {
                                            /*
                                            *Calculate origionation fee if PaymentDetailIndex>=0 and PaymentDetailIn[count].OriginationFee > additionalPaymentPrincipal
                                            */
                                            getSameDayFee = rounndedValue(additionalPaymentPrincipal);
                                            PaymentDetailIn[count].SameDayFee = rounndedValue(PaymentDetailIn[count].SameDayFee - additionalPaymentPrincipal);
                                            additionalPaymentPrincipal = 0;
                                            getAdditionalPaymentPrincipal = rounndedValue(additionalPaymentPrincipal);
                                        }
                                    }

                                    #endregion

                                    #region //Calculate InterestPaymentPaid,InterestDue,InterestCarryOver,CumulativeInterest if InterestDue > additionalPaymentPrincipal.

                                    if (InterestDue > additionalPaymentPrincipal)
                                    {
                                        InterestPaymentPaid = additionalPaymentPrincipal;
                                        InterestCarryOver = InterestDue - additionalPaymentPrincipal;
                                        additionalPaymentPrincipal = 0;
                                    }
                                    else
                                    {
                                        /*
                                        * Calculate InterestPaymentPaid,InterestDue,InterestCarryOver,CumulativeInterest if InterestDue < additionalPaymentPrincipal.
                                        */
                                        additionalPaymentPrincipal = rounndedValue(additionalPaymentPrincipal - rounndedValue(InterestDue));
                                        InterestPaymentPaid = InterestDue;
                                        InterestCarryOver = 0;
                                    }

                                    #endregion

                                    #region //Calculate ServiceFeeInterest if additionalPaymentPrincipal>0.
                                    if (getAdditionalServiceFeeInterest > additionalPaymentPrincipal)
                                    {
                                        serviceinterestcarryOver = getAdditionalServiceFeeInterest - additionalPaymentPrincipal;
                                        getAdditionalServiceFeeInterest = additionalPaymentPrincipal;
                                        additionalPaymentPrincipal = 0;
                                        getAdditionalPaymentPrincipal = rounndedValue(additionalPaymentPrincipal);
                                    }
                                    else
                                    {
                                        /*
                                        * Calculate ServiceFeeInterest if additionalPaymentPrincipal<0.
                                        */
                                        additionalPaymentPrincipal = rounndedValue(additionalPaymentPrincipal - getAdditionalServiceFeeInterest);
                                        getAdditionalPaymentPrincipal = rounndedValue(additionalPaymentPrincipal);
                                        serviceinterestcarryOver = 0;
                                    }
                                    #endregion

                                    #region //Calculate principle if additionalPaymentPrincipal>0.

                                    if (additionalPaymentPrincipal > 0)
                                    {
                                        if (PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal > additionalPaymentPrincipal)
                                        {
                                            remainingAmount = 0;
                                            getAdditionalPaymentPrincipal = 0;
                                        }
                                        else
                                        {
                                            /*
                                            * Calculate Principle if additionalPaymentPrincipal<0.
                                            */
                                            remainingAmount = additionalPaymentPrincipal - PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal;
                                            additionalPaymentPrincipal = rounndedValue(PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal);
                                            getAdditionalPaymentPrincipal = rounndedValue(remainingAmount);
                                        }
                                    }

                                    #endregion

                                    #region //Calculate serviceFee if additionalPaymentPrincipal>0.

                                    if (remainingAmount > 0)
                                    {
                                        if (PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee > remainingAmount)
                                        {
                                            getServiceFee = rounndedValue(remainingAmount);
                                            remainingAmount = 0;
                                            getAdditionalPaymentPrincipal = 0;
                                        }
                                        else
                                        {
                                            /*
                                            * Calculate serviceFee if additionalPaymentPrincipal<0.
                                            */
                                            getServiceFee = rounndedValue(PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee);
                                            remainingAmount = remainingAmount - PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee;
                                            getAdditionalPaymentPrincipal = 0;
                                        }
                                    }

                                    #endregion

                                    #region Calculate management Fee amount

                                    if (PaymentDetailIn[PaymentDetailIndex - 1].ManagementFeeCarryOver > remainingAmount)
                                    {
                                        ManagementFeeCarryOver = PaymentDetailIn[PaymentDetailIndex - 1].ManagementFeeCarryOver - remainingAmount;
                                        managementFee = remainingAmount;
                                        remainingAmount = 0;

                                    }
                                    else
                                    {
                                        ManagementFeeCarryOver = 0;
                                        managementFee = PaymentDetailIn[PaymentDetailIndex - 1].ManagementFeeCarryOver;
                                        remainingAmount -= rounndedValue(PaymentDetailIn[PaymentDetailIndex - 1].ManagementFeeCarryOver);
                                    }
                                    #endregion

                                    #region Calculate maintenance Fee amount

                                    if (PaymentDetailIn[PaymentDetailIndex - 1].MaintenanceFeeCarryOver > remainingAmount)
                                    {
                                        maintenencefeeCarryOver = PaymentDetailIn[PaymentDetailIndex - 1].MaintenanceFeeCarryOver - remainingAmount;
                                        maintenanceFee = remainingAmount;
                                        remainingAmount = 0;

                                    }
                                    else
                                    {
                                        maintenencefeeCarryOver = 0;
                                        maintenanceFee = PaymentDetailIn[PaymentDetailIndex - 1].MaintenanceFeeCarryOver;
                                        remainingAmount -= rounndedValue(PaymentDetailIn[PaymentDetailIndex - 1].MaintenanceFeeCarryOver);
                                    }
                                    #endregion

                                    #endregion
                                }
                                #endregion
                            }
                            #endregion
                            // AD 1.0.0.7 - count beginning principal for additional payment
                            var beginningPrincipal = rounndedValue(PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal);
                            var beginningServiceFee = rounndedValue(PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee);

                            if (PaymentDetailIndex > 0)
                            {
                                #region
                                PaymentDetailIn.Insert(PaymentDetailIndex, new PaymentDetail
                                {
                                    AccruedServiceFeeInterest = PaymentDetailIn[PaymentDetailIndex].AccruedServiceFeeInterest,
                                    serviceFeeInterestCarryOver = serviceinterestcarryOver,
                                    PaymentDate = AdditionalPaymentDate,
                                    DueDate = AdditionalPaymentDate,
                                    BeginningPrincipal = beginningPrincipal,
                                    PrincipalPayment = additionalPaymentPrincipal,
                                    InterestPayment = InterestPaymentPaid,
                                    DailyInterestAmount = dailyInterestAmount,
                                    InterestAccrued = InterestAccrued,
                                    InterestDue = InterestDue,
                                    InterestCarryOver = InterestCarryOver,
                                    TotalPayment = getAdditionalPaymentPrincipal,
                                    CumulativePayment = Input.AdditionalPaymentRecords[i].AdditionalPayment + PaymentDetailIn[PaymentDetailIndex - 1].CumulativePayment,
                                    CumulativeInterest = 0,
                                    CumulativePrincipal = Input.AdditionalPaymentRecords[i].AdditionalPayment + PaymentDetailIn[PaymentDetailIndex - 1].CumulativePrincipal,
                                    PeriodicInterestRate = periodicRateForPeriod,
                                    PaymentID = Input.AdditionalPaymentRecords[i].PaymentID,
                                    Flags = 2,
                                    OriginationFee = getOriginationFee,
                                    SameDayFee = getSameDayFee,
                                    // AD 1.0.0.7 - count service fee
                                    BeginningServiceFee = beginningServiceFee,
                                    ServiceFee = getServiceFee,
                                    ServiceFeeInterest = getAdditionalServiceFeeInterest,
                                    ServiceFeeTotal = getServiceFee + getAdditionalServiceFeeInterest,
                                    ManagementFee = managementFee,
                                    ManagementFeeCarryOver = ManagementFeeCarryOver,
                                    MaintenanceFee = maintenanceFee,
                                    MaintenanceFeeCarryOver = maintenencefeeCarryOver
                                });
                                #endregion
                            }
                            else
                            {
                                #region
                                PaymentDetailIn.Insert(PaymentDetailIndex, new PaymentDetail
                                {
                                    AccruedServiceFeeInterest = PaymentDetailIn[PaymentDetailIndex].AccruedServiceFeeInterest,
                                    serviceFeeInterestCarryOver = serviceinterestcarryOver,
                                    PaymentDate = AdditionalPaymentDate,
                                    DueDate = AdditionalPaymentDate,
                                    BeginningPrincipal = beginningPrincipal,
                                    PrincipalPayment = additionalPaymentPrincipal,
                                    InterestPayment = InterestPaymentPaid,
                                    DailyInterestAmount = dailyInterestAmount,
                                    InterestAccrued = InterestAccrued,
                                    InterestDue = InterestDue,
                                    InterestCarryOver = InterestCarryOver,
                                    TotalPayment = getAdditionalPaymentPrincipal,
                                    CumulativePayment = Input.AdditionalPaymentRecords[i].AdditionalPayment,
                                    CumulativePrincipal = Input.AdditionalPaymentRecords[i].AdditionalPayment,
                                    CumulativeInterest = 0,
                                    PeriodicInterestRate = periodicRateForPeriod,
                                    PaymentID = Input.AdditionalPaymentRecords[i].PaymentID,
                                    Flags = 2,
                                    OriginationFee = getOriginationFee,
                                    SameDayFee = getSameDayFee,
                                    // AD 1.0.0.7 - count service fee
                                    BeginningServiceFee = beginningServiceFee,
                                    ServiceFee = getServiceFee,
                                    ServiceFeeInterest = getAdditionalServiceFeeInterest,
                                    ServiceFeeTotal = getServiceFee + getAdditionalServiceFeeInterest,
                                    ManagementFee = managementFee,
                                    ManagementFeeCarryOver = ManagementFeeCarryOver,
                                    MaintenanceFee = maintenanceFee,
                                    MaintenanceFeeCarryOver = maintenencefeeCarryOver
                                });
                                #endregion
                            }
                            #region Calculate Cumulative amount
                            PaymentDetailIn[PaymentDetailIndex].CumulativeInterest = PaymentDetailIndex == 0 ? PaymentDetailIn[PaymentDetailIndex].InterestPayment : PaymentDetailIn[PaymentDetailIndex - 1].CumulativeInterest + PaymentDetailIn[PaymentDetailIndex].InterestPayment;
                            PaymentDetailIn[PaymentDetailIndex].CumulativePayment = PaymentDetailIndex == 0 ? additionalPaymentPrincipal : PaymentDetailIn[PaymentDetailIndex - 1].CumulativePayment + additionalPaymentPrincipal;
                            PaymentDetailIn[PaymentDetailIndex].CumulativePrincipal = PaymentDetailIndex == 0 ? PaymentDetailIn[PaymentDetailIndex].PrincipalPayment : PaymentDetailIn[PaymentDetailIndex - 1].CumulativePrincipal + PaymentDetailIn[PaymentDetailIndex].PrincipalPayment;
                            PaymentDetailIn[PaymentDetailIndex].CumulativeServiceFee = PaymentDetailIndex == 0 ? PaymentDetailIn[PaymentDetailIndex].ServiceFee : PaymentDetailIn[PaymentDetailIndex - 1].CumulativeServiceFee + PaymentDetailIn[PaymentDetailIndex].ServiceFee;
                            PaymentDetailIn[PaymentDetailIndex].CumulativeServiceFeeInterest = PaymentDetailIndex == 0 ? PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest : PaymentDetailIn[PaymentDetailIndex - 1].CumulativeServiceFeeInterest + PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest;
                            PaymentDetailIn[PaymentDetailIndex].CumulativeMaintenanceFee = PaymentDetailIndex == 0 ? PaymentDetailIn[PaymentDetailIndex].MaintenanceFee : PaymentDetailIn[PaymentDetailIndex - 1].CumulativeMaintenanceFee + PaymentDetailIn[PaymentDetailIndex].MaintenanceFee;
                            PaymentDetailIn[PaymentDetailIndex].CumulativeManagementFee = PaymentDetailIndex == 0 ? PaymentDetailIn[PaymentDetailIndex].ManagementFee : PaymentDetailIn[PaymentDetailIndex - 1].CumulativeManagementFee + PaymentDetailIn[PaymentDetailIndex].ManagementFee;
                            PaymentDetailIn[PaymentDetailIndex].CumulativeOriginationFee = PaymentDetailIndex == 0 ? PaymentDetailIn[PaymentDetailIndex].OriginationFee : PaymentDetailIn[PaymentDetailIndex - 1].CumulativeOriginationFee + PaymentDetailIn[PaymentDetailIndex].OriginationFee;
                            PaymentDetailIn[PaymentDetailIndex].CumulativeSameDayFee = PaymentDetailIndex == 0 ? PaymentDetailIn[PaymentDetailIndex].SameDayFee : PaymentDetailIn[PaymentDetailIndex - 1].CumulativeSameDayFee + PaymentDetailIn[PaymentDetailIndex].SameDayFee;

                            PaymentDetailIn[PaymentDetailIndex].CumulativeTotalFees = rounndedValue((PaymentDetailIn[PaymentDetailIndex].CumulativeOriginationFee + PaymentDetailIn[PaymentDetailIndex].CumulativeSameDayFee + PaymentDetailIn[PaymentDetailIndex].CumulativeMaintenanceFee +
                                                                    PaymentDetailIn[PaymentDetailIndex].CumulativeManagementFee + PaymentDetailIn[PaymentDetailIndex].CumulativeServiceFee + PaymentDetailIn[PaymentDetailIndex].CumulativeServiceFeeInterest));
                            #endregion
                            PaymentDetailIndex = PaymentDetailIndex + 1; //This adjusts for the inserted record.

                            // AD 1.0.0.7 - count beginning principal for the payment next to additional
                            PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal = rounndedValue(beginningPrincipal - additionalPaymentPrincipal);
                            PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee = rounndedValue(beginningServiceFee - getServiceFee);

                            // AD 1.0.0.18 - recalculate interest for the period with additional payment(s)
                            var startPeriodPrincipal = getStartPeriodBeginningPrincipal(PaymentDetailIn[PaymentDetailIndex].PaymentDate, PaymentDetailIn, true);
                            var startPeriodServiceFee = getStartPeriodBeginningPrincipal(PaymentDetailIn[PaymentDetailIndex].PaymentDate, PaymentDetailIn, false);
                            var startPeriodDate = getStartPeriodDate(PaymentDetailIn[PaymentDetailIndex].PaymentDate, Input.InputRecords);

                            // AD 1.0.0.23 - subtract additional payment principal from start period principal
                            // this is needed because scheduled payment follows after additional one if on the same date

                            if (startPeriodDate == AdditionalPaymentDate)
                            {
                                startPeriodPrincipal = startPeriodPrincipal - additionalPaymentPrincipal;
                                startPeriodServiceFee = startPeriodServiceFee - additionalPaymentServiceFee;
                            }

                            double interestPayment = 0;
                            double interestServiceFeePayment = 0;
                            #region Calculate PeriodicInterestrate and Intrest Accrued after the Additional payment date for the next schedule.
                            if (PeriodicInterestRate > 0)
                            {
                                #region Calculate interest amount of the loan
                                scheduleDayDifference = CalculatScheduleDifference(PaymentDetailIn, PaymentDetailIn[PaymentDetailIndex].PaymentDate, PaymentDetailIndex, Input, isMinDuration, DaysPeriod, PaymentDetailIn[PaymentDetailIndex].Flags);
                                periodicRateForPeriod = CalculatePeriodicInterestRateTier(Input, PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal, scheduleDayDifference);
                                periodicInterestRate = Math.Round(periodicInterestRate, 15, MidpointRounding.AwayFromZero);
                                if (!Input.UseFlexibleCalculation && (!isMinDuration || (isMinDuration && PaymentDetailIn.FindLastIndex(o => o.PaymentDate == Input.InputRecords[1].DateIn && o.Flags == 0) < PaymentDetailIndex)))
                                {
                                    periodicRateForPeriod = periodicInterestRate <= 0 ? 0 : Math.Round(periodicInterestRate, 15, MidpointRounding.AwayFromZero);
                                    periodicInterestRate = (periodicInterestRate > periodicRateForPeriod) ? (periodicInterestRate - periodicRateForPeriod) : 0;
                                }
                                interestPayment = CalculateCurrentInterestTier(Input, PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal, scheduleDayDifference);
                                #endregion
                            }
                            else
                            {
                                interestPayment = calculateInterestForPeriodWithAdditionalPayment(startPeriodDate, PaymentDetailIn[PaymentDetailIndex].PaymentDate,
                                startPeriodPrincipal, Input.AdditionalPaymentRecords, Input.InterestRate, Input.DaysInYear);
                            }
                            #endregion

                            #region Calculate service Fee interest
                            periodicInterestRate += Math.Round(periodicRateForPeriod, 15, MidpointRounding.AwayFromZero);
                            scheduleDayDifference = CalculatScheduleDifference(PaymentDetailIn, PaymentDetailIn[PaymentDetailIndex].PaymentDate, PaymentDetailIndex, Input, isMinDuration, DaysPeriod, PaymentDetailIn[PaymentDetailIndex].Flags);
                            periodicRateForPeriod = CalculatePeriodicInterestRateTier(Input, PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee, scheduleDayDifference);

                            if (!Input.UseFlexibleCalculation && (!isMinDuration || (isMinDuration && PaymentDetailIn.FindLastIndex(o => o.PaymentDate == Input.InputRecords[1].DateIn && o.Flags == 0) < PaymentDetailIndex)))
                            {
                                periodicRateForPeriod = periodicInterestRate;
                                periodicInterestRate = (periodicInterestRate > periodicRateForPeriod) ? (periodicInterestRate - periodicRateForPeriod) : 0;
                            }
                            interestServiceFeePayment = (PaymentDetailIn[PaymentDetailIndex - 1].serviceFeeInterestCarryOver + (CalculateCurrentServiceFeeInterestTier(Input, PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee, scheduleDayDifference)));
                            PaymentDetailIn[PaymentDetailIndex].AccruedServiceFeeInterest = CalculateCurrentServiceFeeInterestTier(Input, PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee, scheduleDayDifference);
                            PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver = 0;
                            #endregion

                            PaymentDetailIn[PaymentDetailIndex].PeriodicInterestRate = Math.Round(periodicRateForPeriod, 15, MidpointRounding.AwayFromZero);
                            PaymentDetailIn[PaymentDetailIndex].DailyInterestAmount = (periodicRateForPeriod == 0 || scheduleDayDifference == 0) ? 0 : rounndedValue((periodicRateForPeriod / scheduleDayDifference) * PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal);
                            PaymentDetailIn[PaymentDetailIndex].InterestAccrued = interestPayment;
                            PaymentDetailIn[PaymentDetailIndex].InterestDue = interestPayment + PaymentDetailIn[PaymentDetailIndex - 1].InterestCarryOver;
                            PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest = interestServiceFeePayment;

                            #region Calculate interest,InterestCarryOver,service fee interest, principal and service fee amount 
                            if (PaymentDetailIndex != (PaymentDetailIn.Count - 1))
                            {
                                #region Calculate Interest Payment and Principal Payment Amount
                                if (PaymentDetailIn[PaymentDetailIndex].InterestDue > PaymentDetailIn[PaymentDetailIndex].TotalPayment)
                                {
                                    PaymentDetailIn[PaymentDetailIndex].InterestPayment = PaymentDetailIn[PaymentDetailIndex].TotalPayment;
                                    PaymentDetailIn[PaymentDetailIndex].InterestCarryOver = PaymentDetailIn[PaymentDetailIndex].InterestDue - PaymentDetailIn[PaymentDetailIndex].TotalPayment;
                                }
                                else
                                {
                                    PaymentDetailIn[PaymentDetailIndex].InterestPayment = PaymentDetailIn[PaymentDetailIndex].InterestDue;
                                    PaymentDetailIn[PaymentDetailIndex].InterestCarryOver = 0;
                                }
                                if (PaymentDetailIn[PaymentDetailIndex].InterestDue > PaymentDetailIn[PaymentDetailIndex].TotalPayment)
                                {
                                    PaymentDetailIn[PaymentDetailIndex].PrincipalPayment = 0;
                                }
                                else
                                {
                                    PaymentDetailIn[PaymentDetailIndex].PrincipalPayment = rounndedValue((PaymentDetailIn[PaymentDetailIndex].TotalPayment - rounndedValue(PaymentDetailIn[PaymentDetailIndex].InterestPayment)));
                                }
                                PaymentDetailIn[PaymentDetailIndex].PrincipalPayment = PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal > 0 ? (PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal <= PaymentDetailIn[PaymentDetailIndex].PrincipalPayment ? PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal : PaymentDetailIn[PaymentDetailIndex].PrincipalPayment) : 0;
                                indexCount = PaymentDetailIn.Count(o => o.PaymentDate <= PaymentDetailIn[PaymentDetailIndex].PaymentDate && (o.Flags == 0 || o.Flags == 1));
                                if (indexCount >= (Input.EnforcedPayment) && PaymentDetailIn[PaymentDetailIndex].PrincipalPayment == 0 && PaymentDetailIn[PaymentDetailIndex].Flags != 2 && PaymentDetailIn[PaymentDetailIndex].InterestPayment > 0)
                                {
                                    PaymentDetailIn[PaymentDetailIndex].PrincipalPayment = PaymentDetailIn[PaymentDetailIndex].InterestPayment <= Input.EnforcedPrincipal ? rounndedValue(PaymentDetailIn[PaymentDetailIndex].InterestPayment) : rounndedValue(Input.EnforcedPrincipal);
                                    PaymentDetailIn[PaymentDetailIndex].InterestPayment -= PaymentDetailIn[PaymentDetailIndex].PrincipalPayment;
                                    PaymentDetailIn[PaymentDetailIndex].InterestCarryOver += PaymentDetailIn[PaymentDetailIndex].PrincipalPayment;
                                    PaymentDetailIn[PaymentDetailIndex].PrincipalPayment = PaymentDetailIn[PaymentDetailIndex].PrincipalPayment <= 0 ? 0 : PaymentDetailIn[PaymentDetailIndex].PrincipalPayment;
                                }

                                PaymentDetailIn[PaymentDetailIndex].PrincipalPayment = PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal > 0 ? (PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal <= PaymentDetailIn[PaymentDetailIndex].PrincipalPayment ? PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal : PaymentDetailIn[PaymentDetailIndex].PrincipalPayment) : 0;
                                #endregion

                                #region Calculate Service Fee Interest  and Service Fee Amount

                                if (Input.ServiceFeeFirstPayment && (PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee > 0 || Input.EarnedServiceFeeInterest > 0))
                                {
                                    PaymentDetailIn[PaymentDetailIndex].ServiceFee = rounndedValue(PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee);
                                    PaymentDetailIn[PaymentDetailIndex].ServiceFeeTotal = rounndedValue(PaymentDetailIn[PaymentDetailIndex].ServiceFee + PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest);
                                }
                                else if (PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest >= PaymentDetailIn[PaymentDetailIndex].ServiceFeeTotal)
                                {
                                    PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver = PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest - PaymentDetailIn[PaymentDetailIndex].ServiceFeeTotal;
                                    PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest = PaymentDetailIn[PaymentDetailIndex].ServiceFeeTotal;
                                    PaymentDetailIn[PaymentDetailIndex].ServiceFee = 0;
                                }
                                else
                                {
                                    PaymentDetailIn[PaymentDetailIndex].ServiceFee = rounndedValue(PaymentDetailIn[PaymentDetailIndex].ServiceFeeTotal - rounndedValue(PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest));
                                    PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver = 0;
                                }
                                PaymentDetailIn[PaymentDetailIndex].ServiceFee = PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee > 0 ? (PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee <= PaymentDetailIn[PaymentDetailIndex].ServiceFee ? PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee : PaymentDetailIn[PaymentDetailIndex].ServiceFee) : 0;
                                #endregion

                                #region This is used for Actual Payment amount allocation on the basis of prioritys

                                inputdateIndex = Input.InputRecords.FindIndex(o => o.DateIn == PaymentDetailIn[PaymentDetailIndex].PaymentDate);
                                if (!string.IsNullOrEmpty(Input.InputRecords[inputdateIndex].PaymentAmount) || Input.PaymentAmount > 0)
                                {
                                    #region Allocate amount on the basis of priority when Actual amount is given
                                    if (!string.IsNullOrEmpty(Input.InputRecords[inputdateIndex].PaymentAmount))
                                    {
                                        paymentAmount = rounndedValue(Convert.ToDouble(Input.InputRecords[inputdateIndex].PaymentAmount));
                                        isPaymentAmount = false;
                                    }
                                    else
                                    {
                                        isPaymentAmount = true;
                                        paymentAmount = rounndedValue(Input.PaymentAmount + (inputdateIndex == 1 ? PaymentDetailIn[PaymentDetailIndex].OriginationFee + PaymentDetailIn[PaymentDetailIndex].SameDayFee : 0));
                                        ManagementFeeCarryOver += Input.ManagementFee;
                                        maintenencefeeCarryOver += inputdateIndex == 1 ? 0 : Input.MaintenanceFee;
                                    }

                                    if (Input.OriginationFee > 0)
                                    {
                                        #region 
                                        if (Input.InputRecords[1].DateIn == PaymentDetailIn[PaymentDetailIndex].PaymentDate && PaymentDetailIn[PaymentDetailIndex].Flags != 2)
                                        {
                                            if (PaymentDetailIn[PaymentDetailIndex].OriginationFee > paymentAmount)
                                            {
                                                PaymentDetailIn[PaymentDetailIndex].OriginationFee = paymentAmount;
                                                paymentAmount = 0;
                                            }
                                            else
                                            {
                                                paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].OriginationFee));
                                            }
                                        }
                                        else
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].OriginationFee = 0;
                                        }
                                        #endregion
                                    }
                                    if (Input.SameDayFee > 0)
                                    {
                                        #region
                                        if (Input.InputRecords[1].DateIn == PaymentDetailIn[PaymentDetailIndex].PaymentDate && PaymentDetailIn[PaymentDetailIndex].Flags != 2)
                                        {
                                            if (PaymentDetailIn[PaymentDetailIndex].SameDayFee > paymentAmount)
                                            {
                                                PaymentDetailIn[PaymentDetailIndex].SameDayFee = paymentAmount;
                                                paymentAmount = 0;
                                            }
                                            else
                                            {
                                                paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].SameDayFee));
                                            }
                                        }
                                        else
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].SameDayFee = 0;
                                        }
                                        #endregion
                                    }
                                    if (PaymentDetailIn[PaymentDetailIndex].InterestPayment > 0)
                                    {
                                        #region
                                        if (PaymentDetailIn[PaymentDetailIndex].InterestPayment > paymentAmount)
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].InterestCarryOver = PaymentDetailIn[PaymentDetailIndex].InterestDue - paymentAmount;
                                            PaymentDetailIn[PaymentDetailIndex].InterestPayment = paymentAmount;
                                            paymentAmount = 0;
                                        }
                                        else
                                        {
                                            paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].InterestPayment));
                                        }
                                        #endregion
                                    }
                                    if (PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest > 0)
                                    {
                                        #region
                                        if (PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest > paymentAmount)
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver += PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest - paymentAmount;
                                            PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest = paymentAmount;
                                            paymentAmount = 0;
                                        }
                                        else
                                        {
                                            paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest));
                                        }
                                        #endregion
                                    }
                                    if (PaymentDetailIn[PaymentDetailIndex].PrincipalPayment > 0)
                                    {
                                        #region
                                        if (PaymentDetailIn[PaymentDetailIndex].PrincipalPayment > paymentAmount)
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].PrincipalPayment = paymentAmount;
                                            paymentAmount = 0;
                                        }
                                        else
                                        {
                                            paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].PrincipalPayment));
                                        }
                                        #endregion
                                    }
                                    if (PaymentDetailIn[PaymentDetailIndex].ServiceFee > 0)
                                    {
                                        #region
                                        if (PaymentDetailIn[PaymentDetailIndex].ServiceFee > paymentAmount)
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].ServiceFee = paymentAmount;
                                            paymentAmount = 0;
                                        }
                                        else
                                        {
                                            paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].ServiceFee));
                                        }
                                        #endregion
                                    }
                                    if (i != (Input.InputRecords.Count - 1))
                                    {
                                        if (isPaymentAmount)
                                        {
                                            #region calculate management and maintenance fee amount
                                            if (ManagementFeeCarryOver > 0)
                                            {
                                                #region
                                                if (ManagementFeeCarryOver > paymentAmount)
                                                {
                                                    PaymentDetailIn[PaymentDetailIndex].ManagementFee = paymentAmount;
                                                    ManagementFeeCarryOver -= paymentAmount;
                                                    PaymentDetailIn[PaymentDetailIndex].ManagementFeeCarryOver = ManagementFeeCarryOver;
                                                    paymentAmount = 0;
                                                }
                                                else
                                                {
                                                    PaymentDetailIn[PaymentDetailIndex].ManagementFee = ManagementFeeCarryOver;
                                                    paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].ManagementFee));
                                                    PaymentDetailIn[PaymentDetailIndex].ManagementFeeCarryOver = 0;
                                                    ManagementFeeCarryOver = 0;
                                                }
                                                #endregion
                                            }
                                            if (maintenencefeeCarryOver > 0)
                                            {
                                                #region
                                                if (maintenencefeeCarryOver > paymentAmount)
                                                {
                                                    PaymentDetailIn[PaymentDetailIndex].MaintenanceFee = paymentAmount;
                                                    maintenencefeeCarryOver -= paymentAmount;
                                                    PaymentDetailIn[PaymentDetailIndex].MaintenanceFeeCarryOver = maintenencefeeCarryOver;
                                                    paymentAmount = 0;
                                                }
                                                else
                                                {
                                                    PaymentDetailIn[PaymentDetailIndex].MaintenanceFee = maintenencefeeCarryOver;
                                                    paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].MaintenanceFee));
                                                    PaymentDetailIn[PaymentDetailIndex].MaintenanceFeeCarryOver = 0;
                                                    maintenencefeeCarryOver = 0;
                                                }
                                                #endregion
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            #region calculate management and maintenance fee amount
                                            if (PaymentDetailIn[PaymentDetailIndex].ManagementFee > 0)
                                            {
                                                #region
                                                if (PaymentDetailIn[PaymentDetailIndex].ManagementFee > paymentAmount)
                                                {
                                                    PaymentDetailIn[PaymentDetailIndex].ManagementFee = paymentAmount;
                                                    PaymentDetailIn[PaymentDetailIndex].ManagementFeeCarryOver = ManagementFeeCarryOver;
                                                    paymentAmount = 0;
                                                }
                                                else
                                                {
                                                    //ManagementFeeCarryOver += payments[payments.Count - 1].ManagementFee;
                                                    //payments[payments.Count - 1].ManagementFee = (ManagementFeeCarryOver > paymentAmount) ? paymentAmount : (paymentAmount - ManagementFeeCarryOver);
                                                    //ManagementFeeCarryOver = ManagementFeeCarryOver - payments[payments.Count - 1].ManagementFee;
                                                    PaymentDetailIn[PaymentDetailIndex].ManagementFeeCarryOver = ManagementFeeCarryOver;
                                                    paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].ManagementFee));
                                                }
                                                #endregion
                                            }
                                            if (PaymentDetailIn[PaymentDetailIndex].MaintenanceFee > 0)
                                            {
                                                #region
                                                if (PaymentDetailIn[PaymentDetailIndex].MaintenanceFee > paymentAmount)
                                                {
                                                    PaymentDetailIn[PaymentDetailIndex].MaintenanceFee = paymentAmount;
                                                    PaymentDetailIn[PaymentDetailIndex].MaintenanceFeeCarryOver = maintenencefeeCarryOver;
                                                    paymentAmount = 0;
                                                }
                                                else
                                                {
                                                    PaymentDetailIn[PaymentDetailIndex].MaintenanceFeeCarryOver = maintenencefeeCarryOver;
                                                    paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].MaintenanceFee));
                                                }
                                                #endregion
                                            }
                                            #endregion
                                        }
                                    }
                                    else
                                    {
                                        #region calculate management and maintenance fee amount
                                        PaymentDetailIn[PaymentDetailIndex].ManagementFee = ManagementFeeCarryOver;
                                        PaymentDetailIn[PaymentDetailIndex].ManagementFeeCarryOver = 0;
                                        ManagementFeeCarryOver = 0;
                                        PaymentDetailIn[PaymentDetailIndex].MaintenanceFee = maintenencefeeCarryOver;
                                        PaymentDetailIn[PaymentDetailIndex].MaintenanceFeeCarryOver = 0;
                                        maintenencefeeCarryOver = 0;
                                        #endregion
                                    }
                                    #region
                                    //if (PaymentDetailIn[PaymentDetailIndex].ManagementFee > 0)
                                    //{
                                    //    #region
                                    //    if (PaymentDetailIn[PaymentDetailIndex].ManagementFee > paymentAmount)
                                    //    {
                                    //        PaymentDetailIn[PaymentDetailIndex].ManagementFee = paymentAmount;
                                    //    }
                                    //    else
                                    //    {
                                    //        paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].ManagementFee));
                                    //    }
                                    //    #endregion
                                    //}
                                    //if (PaymentDetailIn[PaymentDetailIndex].MaintenanceFee > 0)
                                    //{
                                    //    #region
                                    //    if (PaymentDetailIn[PaymentDetailIndex].MaintenanceFee > paymentAmount)
                                    //    {
                                    //        PaymentDetailIn[PaymentDetailIndex].MaintenanceFee = paymentAmount;
                                    //    }
                                    //    else
                                    //    {
                                    //        paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].MaintenanceFee));
                                    //    }
                                    //    #endregion
                                    //}
                                    #endregion
                                    if (paymentAmount > 0)//When payment amount remains after full payment of schedule
                                    {
                                        #region
                                        double remainingPincipal = PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal - PaymentDetailIn[PaymentDetailIndex].PrincipalPayment;
                                        remainingPincipal = remainingPincipal <= 0 ? 0 : remainingPincipal;
                                        double remainingServiceFee = PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee - PaymentDetailIn[PaymentDetailIndex].ServiceFee;
                                        remainingServiceFee = remainingServiceFee <= 0 ? 0 : remainingServiceFee;
                                        #region
                                        if (PaymentDetailIn[PaymentDetailIndex].InterestCarryOver > paymentAmount)
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].InterestPayment += paymentAmount;
                                            PaymentDetailIn[PaymentDetailIndex].InterestCarryOver -= paymentAmount;
                                            paymentAmount = 0;
                                        }
                                        else
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].InterestPayment += PaymentDetailIn[PaymentDetailIndex].InterestCarryOver;
                                            paymentAmount = rounndedValue(paymentAmount - PaymentDetailIn[PaymentDetailIndex].InterestCarryOver);
                                            PaymentDetailIn[PaymentDetailIndex].InterestCarryOver = 0;

                                        }
                                        #endregion

                                        #region
                                        if (PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver > paymentAmount)
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest += paymentAmount;
                                            PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver -= paymentAmount;
                                            paymentAmount = 0;
                                        }
                                        else
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest += PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver;
                                            paymentAmount = rounndedValue(paymentAmount - PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver);
                                            PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver = 0;
                                        }
                                        #endregion

                                        #region
                                        if (remainingPincipal > paymentAmount)
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].PrincipalPayment += paymentAmount;
                                            paymentAmount = 0;
                                        }
                                        else
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].PrincipalPayment += remainingPincipal;
                                            paymentAmount = rounndedValue(paymentAmount - remainingPincipal);
                                        }
                                        #endregion

                                        #region
                                        if (remainingServiceFee > paymentAmount)
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].ServiceFee += paymentAmount;
                                            paymentAmount = 0;
                                        }
                                        else
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].ServiceFee += remainingServiceFee;
                                            paymentAmount = rounndedValue(paymentAmount - remainingServiceFee);
                                        }
                                        #endregion

                                        #region
                                        if (PaymentDetailIn[PaymentDetailIndex].ManagementFeeCarryOver > paymentAmount)
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].ManagementFee += paymentAmount;
                                            ManagementFeeCarryOver -= paymentAmount;
                                            PaymentDetailIn[PaymentDetailIndex].ManagementFeeCarryOver -= paymentAmount;
                                            paymentAmount = 0;
                                        }
                                        else
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].ManagementFee += PaymentDetailIn[PaymentDetailIndex].ManagementFeeCarryOver;
                                            PaymentDetailIn[PaymentDetailIndex].ManagementFeeCarryOver = 0;
                                            ManagementFeeCarryOver = 0;
                                            paymentAmount = rounndedValue(paymentAmount - PaymentDetailIn[PaymentDetailIndex].ManagementFeeCarryOver);
                                        }
                                        #endregion

                                        #region
                                        if (PaymentDetailIn[PaymentDetailIndex].MaintenanceFeeCarryOver > paymentAmount)
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].MaintenanceFee += paymentAmount;
                                            maintenencefeeCarryOver -= paymentAmount;
                                            PaymentDetailIn[PaymentDetailIndex].MaintenanceFeeCarryOver -= paymentAmount;
                                            paymentAmount = 0;
                                        }
                                        else
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].MaintenanceFee += PaymentDetailIn[PaymentDetailIndex].MaintenanceFeeCarryOver;
                                            PaymentDetailIn[PaymentDetailIndex].MaintenanceFeeCarryOver = 0;
                                            maintenencefeeCarryOver = 0;
                                            paymentAmount = rounndedValue(paymentAmount - PaymentDetailIn[PaymentDetailIndex].MaintenanceFeeCarryOver);
                                        }
                                        #endregion
                                        #endregion
                                    }
                                    #endregion
                                }
                                #endregion
                            }
                            else
                            {
                                #region Calculate Interest Payment and Principal Payment and Service Fee Amount 
                                PaymentDetailIn[PaymentDetailIndex].ManagementFee = Input.ManagementFee + PaymentDetailIn[PaymentDetailIndex - 1].ManagementFeeCarryOver;
                                PaymentDetailIn[PaymentDetailIndex].ManagementFeeCarryOver = 0;
                                PaymentDetailIn[PaymentDetailIndex].MaintenanceFee = Input.MaintenanceFee + PaymentDetailIn[PaymentDetailIndex - 1].MaintenanceFeeCarryOver;
                                PaymentDetailIn[PaymentDetailIndex].MaintenanceFeeCarryOver = 0;
                                PaymentDetailIn[PaymentDetailIndex].InterestPayment = PaymentDetailIn[PaymentDetailIndex].InterestDue;
                                PaymentDetailIn[PaymentDetailIndex].InterestCarryOver = 0;
                                PaymentDetailIn[PaymentDetailIndex].PrincipalPayment = rounndedValue(PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal);
                                PaymentDetailIn[PaymentDetailIndex].ServiceFee = rounndedValue(PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee);
                                PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver = 0;
                                #endregion
                            }
                            #endregion

                            // AD 1.0.0.7 - call payments recast if needed
                            if (Input.RecastAdditionalPayments)
                            {
                                // AD 1.0.0.10 - pass regular payment rounding flag to the method
                                // AD 1.0.0.18 - pass recalculated interests to recast method
                                RecastPayments(PaymentDetailIn, PaymentDetailIndex, isServiceFeeInterestApplied, isRegularPaymentRounded, interestPayment, interestServiceFeePayment);
                            }
                            CurrentPrincipal = rounndedValue(PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal - PaymentDetailIn[PaymentDetailIndex].PrincipalPayment);

                            // AD 1.0.0.7 - count cumulative service fees
                            PaymentDetailIn[PaymentDetailIndex].ServiceFee = PaymentDetailIn[PaymentDetailIndex].ServiceFee <= 0 ? 0 : PaymentDetailIn[PaymentDetailIndex].ServiceFee;
                            CurrentServiceFee = rounndedValue(PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee) - rounndedValue(PaymentDetailIn[PaymentDetailIndex].ServiceFee);


                            if (CurrentPrincipal <= 0)
                            {
                                #region
                                // AD 1.0.0.7 - removed subtraction of additional payment
                                // Beginning Principal has already subtracted payment
                                PaymentDetailIn[PaymentDetailIndex].TotalPayment = rounndedValue(PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal - PreviousAdditionalPaymentSum + PaymentDetailIn[PaymentDetailIndex].InterestAccrued);
                                PaymentDetailIn[PaymentDetailIndex].PrincipalPayment = PaymentDetailIn[PaymentDetailIndex].TotalPayment - rounndedValue(PaymentDetailIn[PaymentDetailIndex].InterestPayment);
                                PaymentDetailIn[PaymentDetailIndex].CumulativePayment = PaymentDetailIn[PaymentDetailIndex - 1].CumulativePayment + PaymentDetailIn[PaymentDetailIndex].TotalPayment;

                                // AD 1.0.0.3 - adjust cumulative principal for additional payments
                                PaymentDetailIn[PaymentDetailIndex].CumulativePrincipal = PaymentDetailIn[PaymentDetailIndex - 1].CumulativePrincipal + PaymentDetailIn[PaymentDetailIndex].PrincipalPayment;
                                PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest += PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver;


                                if (PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal <= 0 && PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee <= 0 && PaymentDetailIn[PaymentDetailIndex].InterestDue <= 0 && PaymentDetailIn[PaymentDetailIndex].InterestCarryOver <= 0 && PaymentDetailIn[PaymentDetailIndex].ManagementFee <= Input.ManagementFee && PaymentDetailIn[PaymentDetailIndex].MaintenanceFee <= Input.MaintenanceFee)
                                {
                                    PaymentDetailIndex--;
                                }
                                for (int j = PaymentDetailIn.Count - 1; j > PaymentDetailIndex; j--)
                                {
                                    PaymentDetailIn.RemoveAt(j);
                                }

                                PaymentDetailIn[PaymentDetailIndex].ServiceFee = rounndedValue(PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee);
                                PaymentDetailIn[PaymentDetailIndex].ServiceFeeTotal = rounndedValue(PaymentDetailIn[PaymentDetailIndex].ServiceFee + PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest);
                                #endregion
                            }
                            // AD 1.0.0.7 - add service fee count
                            if (CurrentServiceFee <= 0 && PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver <= 0)
                            {
                                #region
                                if (!Input.ServiceFeeFirstPayment)
                                {

                                    PaymentDetailIn[PaymentDetailIndex].ServiceFeeTotal = PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee + PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest;

                                    PaymentDetailIn[PaymentDetailIndex].ServiceFee = rounndedValue(PaymentDetailIn[PaymentDetailIndex].ServiceFeeTotal - rounndedValue(PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest));
                                    PaymentDetailIn[PaymentDetailIndex].ServiceFee = PaymentDetailIn[PaymentDetailIndex].ServiceFee <= 0 ? 0 : PaymentDetailIn[PaymentDetailIndex].ServiceFee;
                                    PaymentDetailIn[PaymentDetailIndex].CumulativeServiceFee = PaymentDetailIndex == 0 ? PaymentDetailIn[PaymentDetailIndex].ServiceFee :
                                                            (PaymentDetailIn[PaymentDetailIndex - 1].CumulativeServiceFee + PaymentDetailIn[PaymentDetailIndex].ServiceFee);
                                }
                                #endregion
                            }
                            #region Calculate Cumulative amount
                            PaymentDetailIn[PaymentDetailIndex].CumulativeInterest = PaymentDetailIndex == 0 ? PaymentDetailIn[PaymentDetailIndex].InterestPayment : PaymentDetailIn[PaymentDetailIndex - 1].CumulativeInterest + PaymentDetailIn[PaymentDetailIndex].InterestPayment;
                            PaymentDetailIn[PaymentDetailIndex].CumulativePayment = PaymentDetailIndex == 0 ? PaymentDetailIn[PaymentDetailIndex].TotalPayment : PaymentDetailIn[PaymentDetailIndex - 1].CumulativePayment + PaymentDetailIn[PaymentDetailIndex].TotalPayment;
                            PaymentDetailIn[PaymentDetailIndex].CumulativePrincipal = PaymentDetailIndex == 0 ? PaymentDetailIn[PaymentDetailIndex].PrincipalPayment : PaymentDetailIn[PaymentDetailIndex - 1].CumulativePrincipal + PaymentDetailIn[PaymentDetailIndex].PrincipalPayment;
                            PaymentDetailIn[PaymentDetailIndex].CumulativeServiceFee = PaymentDetailIndex == 0 ? PaymentDetailIn[PaymentDetailIndex].ServiceFee : PaymentDetailIn[PaymentDetailIndex - 1].CumulativeServiceFee + PaymentDetailIn[PaymentDetailIndex].ServiceFee;
                            PaymentDetailIn[PaymentDetailIndex].CumulativeServiceFeeInterest = PaymentDetailIndex == 0 ? PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest : PaymentDetailIn[PaymentDetailIndex - 1].CumulativeServiceFeeInterest + PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest;
                            PaymentDetailIn[PaymentDetailIndex].CumulativeMaintenanceFee = PaymentDetailIndex == 0 ? PaymentDetailIn[PaymentDetailIndex].MaintenanceFee : PaymentDetailIn[PaymentDetailIndex - 1].CumulativeMaintenanceFee + PaymentDetailIn[PaymentDetailIndex].MaintenanceFee;
                            PaymentDetailIn[PaymentDetailIndex].CumulativeManagementFee = PaymentDetailIndex == 0 ? PaymentDetailIn[PaymentDetailIndex].ManagementFee : PaymentDetailIn[PaymentDetailIndex - 1].CumulativeManagementFee + PaymentDetailIn[PaymentDetailIndex].ManagementFee;
                            PaymentDetailIn[PaymentDetailIndex].CumulativeOriginationFee = PaymentDetailIndex == 0 ? PaymentDetailIn[PaymentDetailIndex].OriginationFee : PaymentDetailIn[PaymentDetailIndex - 1].CumulativeOriginationFee + PaymentDetailIn[PaymentDetailIndex].OriginationFee;
                            PaymentDetailIn[PaymentDetailIndex].CumulativeSameDayFee = PaymentDetailIndex == 0 ? PaymentDetailIn[PaymentDetailIndex].SameDayFee : PaymentDetailIn[PaymentDetailIndex - 1].CumulativeSameDayFee + PaymentDetailIn[PaymentDetailIndex].SameDayFee;
                            PaymentDetailIn[PaymentDetailIndex].CumulativeTotalFees = rounndedValue((PaymentDetailIn[PaymentDetailIndex].CumulativeOriginationFee + PaymentDetailIn[PaymentDetailIndex].CumulativeSameDayFee +
                                                                                                  PaymentDetailIn[PaymentDetailIndex].CumulativeMaintenanceFee + PaymentDetailIn[PaymentDetailIndex].CumulativeManagementFee +
                                                                                                  PaymentDetailIn[PaymentDetailIndex].CumulativeServiceFee +
                                                                                                  PaymentDetailIn[PaymentDetailIndex].CumulativeServiceFeeInterest));
                            #endregion
                            #endregion
                        }
                        else //if (FirstOne)
                        {
                            #region
                            PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal = rounndedValue(CurrentPrincipal);
                            scheduleDayDifference = CalculatScheduleDifference(PaymentDetailIn, PaymentDetailIn[PaymentDetailIndex].PaymentDate, PaymentDetailIndex, Input, isMinDuration, DaysPeriod, PaymentDetailIn[PaymentDetailIndex].Flags);
                            periodicRateForPeriod = CalculatePeriodicInterestRateTier(Input, PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal, scheduleDayDifference);
                            periodicInterestRate = Math.Round(periodicInterestRate, 15, MidpointRounding.AwayFromZero);

                            PaymentDetailIn[PaymentDetailIndex].PeriodicInterestRate = periodicInterestRate;
                            PaymentDetailIn[PaymentDetailIndex].DailyInterestAmount = (periodicRateForPeriod == 0 || scheduleDayDifference == 0) ? 0 : rounndedValue((periodicRateForPeriod / scheduleDayDifference) * PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal);
                            PaymentDetailIn[PaymentDetailIndex].InterestAccrued = CalculateCurrentInterestTier(Input, PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal, scheduleDayDifference);
                            PaymentDetailIn[PaymentDetailIndex].InterestDue = PaymentDetailIn[PaymentDetailIndex].InterestAccrued + PaymentDetailIn[PaymentDetailIndex - 1].InterestCarryOver;
                            // AD 1.0.0.7 - count service fee data
                            CurrentServiceFee = CurrentServiceFee <= 0 ? 0 : CurrentServiceFee;
                            PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee = rounndedValue(CurrentServiceFee);
                            PaymentDetailIn[PaymentDetailIndex].AccruedServiceFeeInterest = (isServiceFeeInterestApplied ? CalculateCurrentServiceFeeInterestTier(Input, PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee, scheduleDayDifference) : 0.0);
                            PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest = PaymentDetailIn[PaymentDetailIndex].AccruedServiceFeeInterest + PaymentDetailIn[PaymentDetailIndex - 1].serviceFeeInterestCarryOver;

                            if (PaymentDetailIndex != PaymentDetailIn.Count - 1)
                            {
                                #region Calculate Interest Payment and Principal Payment Amount
                                if (PaymentDetailIn[PaymentDetailIndex].InterestDue > PaymentDetailIn[PaymentDetailIndex].TotalPayment)
                                {
                                    PaymentDetailIn[PaymentDetailIndex].InterestPayment = PaymentDetailIn[PaymentDetailIndex].TotalPayment;
                                    PaymentDetailIn[PaymentDetailIndex].InterestCarryOver = PaymentDetailIn[PaymentDetailIndex].InterestDue - PaymentDetailIn[PaymentDetailIndex].TotalPayment;
                                }
                                else
                                {
                                    PaymentDetailIn[PaymentDetailIndex].InterestPayment = PaymentDetailIn[PaymentDetailIndex].InterestDue;
                                    PaymentDetailIn[PaymentDetailIndex].InterestCarryOver = 0;
                                }
                                if (PaymentDetailIn[PaymentDetailIndex].InterestDue > PaymentDetailIn[PaymentDetailIndex].TotalPayment)
                                {
                                    PaymentDetailIn[PaymentDetailIndex].PrincipalPayment = 0;
                                }
                                else
                                {
                                    PaymentDetailIn[PaymentDetailIndex].PrincipalPayment = rounndedValue(PaymentDetailIn[PaymentDetailIndex].TotalPayment - rounndedValue(PaymentDetailIn[PaymentDetailIndex].InterestPayment));
                                }
                                indexCount = PaymentDetailIn.Count(o => o.PaymentDate <= PaymentDetailIn[PaymentDetailIndex].PaymentDate && (o.Flags == 0 || o.Flags == 1));
                                if (indexCount >= (Input.EnforcedPayment) && PaymentDetailIn[PaymentDetailIndex].PrincipalPayment == 0 && PaymentDetailIn[PaymentDetailIndex].Flags != 2 && PaymentDetailIn[PaymentDetailIndex].InterestPayment > 0)
                                {
                                    PaymentDetailIn[PaymentDetailIndex].PrincipalPayment = PaymentDetailIn[PaymentDetailIndex].InterestPayment <= Input.EnforcedPrincipal ? rounndedValue(PaymentDetailIn[PaymentDetailIndex].InterestPayment) : rounndedValue(Input.EnforcedPrincipal);
                                    PaymentDetailIn[PaymentDetailIndex].InterestPayment -= PaymentDetailIn[PaymentDetailIndex].PrincipalPayment;
                                    PaymentDetailIn[PaymentDetailIndex].InterestCarryOver += PaymentDetailIn[PaymentDetailIndex].PrincipalPayment;
                                    PaymentDetailIn[PaymentDetailIndex].PrincipalPayment = PaymentDetailIn[PaymentDetailIndex].PrincipalPayment <= 0 ? 0 : PaymentDetailIn[PaymentDetailIndex].PrincipalPayment;
                                }

                                PaymentDetailIn[PaymentDetailIndex].PrincipalPayment = PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal > 0 ? (PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal <= PaymentDetailIn[PaymentDetailIndex].PrincipalPayment ? PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal : PaymentDetailIn[PaymentDetailIndex].PrincipalPayment) : 0;
                                #endregion
                                #region Calculate Service Fee Interest  and Service Fee Amount

                                if (PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest >= PaymentDetailIn[PaymentDetailIndex].ServiceFeeTotal)
                                {
                                    PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver = PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest - PaymentDetailIn[PaymentDetailIndex].ServiceFeeTotal;
                                    PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest = PaymentDetailIn[PaymentDetailIndex].ServiceFeeTotal;
                                    PaymentDetailIn[PaymentDetailIndex].ServiceFee = 0;
                                }
                                else
                                {
                                    PaymentDetailIn[PaymentDetailIndex].ServiceFee = rounndedValue(PaymentDetailIn[PaymentDetailIndex].ServiceFeeTotal - rounndedValue(PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest));
                                    PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver = 0;
                                }
                                PaymentDetailIn[PaymentDetailIndex].ServiceFee = PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee > 0 ? (PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee <= PaymentDetailIn[PaymentDetailIndex].ServiceFee ? PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee : PaymentDetailIn[PaymentDetailIndex].ServiceFee) : 0;
                                #endregion

                                #region This is used for Actual Payment amount allocation on the basis of prioritys
                                inputdateIndex = Input.InputRecords.FindIndex(o => o.DateIn == PaymentDetailIn[PaymentDetailIndex].PaymentDate);
                                if (!string.IsNullOrEmpty(Input.InputRecords[inputdateIndex].PaymentAmount) || Input.PaymentAmount > 0)
                                {
                                    #region Allocate amount on the basis of priority when Actual amount is given
                                    if (!string.IsNullOrEmpty(Input.InputRecords[inputdateIndex].PaymentAmount))
                                    {
                                        paymentAmount = rounndedValue(Convert.ToDouble(Input.InputRecords[inputdateIndex].PaymentAmount));
                                        isPaymentAmount = false;
                                    }
                                    else
                                    {
                                        isPaymentAmount = true;
                                        paymentAmount = rounndedValue(Input.PaymentAmount + (inputdateIndex == 1 ? PaymentDetailIn[PaymentDetailIndex].OriginationFee + PaymentDetailIn[PaymentDetailIndex].SameDayFee : 0));
                                        ManagementFeeCarryOver += Input.ManagementFee;
                                        maintenencefeeCarryOver += inputdateIndex == 1 ? 0 : Input.MaintenanceFee;
                                    }
                                    if (Input.OriginationFee > 0)
                                    {
                                        #region 
                                        if (Input.InputRecords[1].DateIn == PaymentDetailIn[PaymentDetailIndex].PaymentDate && PaymentDetailIn[PaymentDetailIndex].Flags != 2)
                                        {
                                            if (PaymentDetailIn[PaymentDetailIndex].OriginationFee > paymentAmount)
                                            {
                                                PaymentDetailIn[PaymentDetailIndex].OriginationFee = paymentAmount;
                                                paymentAmount = 0;
                                            }
                                            else
                                            {
                                                paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].OriginationFee));
                                            }
                                        }
                                        else
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].OriginationFee = 0;
                                        }
                                        #endregion
                                    }
                                    if (Input.SameDayFee > 0)
                                    {
                                        #region
                                        if (Input.InputRecords[1].DateIn == PaymentDetailIn[PaymentDetailIndex].PaymentDate && PaymentDetailIn[PaymentDetailIndex].Flags != 2)
                                        {
                                            if (PaymentDetailIn[PaymentDetailIndex].SameDayFee > paymentAmount)
                                            {
                                                PaymentDetailIn[PaymentDetailIndex].SameDayFee = paymentAmount;
                                                paymentAmount = 0;
                                            }
                                            else
                                            {
                                                paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].SameDayFee));
                                            }
                                        }
                                        else
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].SameDayFee = 0;
                                        }
                                        #endregion
                                    }
                                    if (PaymentDetailIn[PaymentDetailIndex].InterestPayment > 0)
                                    {
                                        #region
                                        if (PaymentDetailIn[PaymentDetailIndex].InterestPayment > paymentAmount)
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].InterestCarryOver = PaymentDetailIn[PaymentDetailIndex].InterestDue - paymentAmount;
                                            PaymentDetailIn[PaymentDetailIndex].InterestPayment = paymentAmount;
                                            paymentAmount = 0;
                                        }
                                        else
                                        {
                                            paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].InterestPayment));
                                        }
                                        #endregion
                                    }
                                    if (PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest > 0)
                                    {
                                        #region
                                        if (PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest > paymentAmount)
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver += PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest - paymentAmount;
                                            PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest = paymentAmount;
                                            paymentAmount = 0;
                                        }
                                        else
                                        {
                                            paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest));
                                        }
                                        #endregion
                                    }
                                    if (PaymentDetailIn[PaymentDetailIndex].PrincipalPayment > 0)
                                    {
                                        #region
                                        if (PaymentDetailIn[PaymentDetailIndex].PrincipalPayment > paymentAmount)
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].PrincipalPayment = paymentAmount;
                                            paymentAmount = 0;
                                        }
                                        else
                                        {
                                            paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].PrincipalPayment));
                                        }
                                        #endregion
                                    }
                                    if (PaymentDetailIn[PaymentDetailIndex].ServiceFee > 0)
                                    {
                                        #region
                                        if (PaymentDetailIn[PaymentDetailIndex].ServiceFee > paymentAmount)
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].ServiceFee = paymentAmount;
                                            paymentAmount = 0;
                                        }
                                        else
                                        {
                                            paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].ServiceFee));
                                        }
                                        #endregion
                                    }
                                    if (i != (Input.InputRecords.Count - 1))
                                    {
                                        if (isPaymentAmount)
                                        {
                                            #region calculate management and maintenance fee amount
                                            if (ManagementFeeCarryOver > 0)
                                            {
                                                #region
                                                if (ManagementFeeCarryOver > paymentAmount)
                                                {
                                                    PaymentDetailIn[PaymentDetailIndex].ManagementFee = paymentAmount;
                                                    ManagementFeeCarryOver -= paymentAmount;
                                                    PaymentDetailIn[PaymentDetailIndex].ManagementFeeCarryOver = ManagementFeeCarryOver;
                                                    paymentAmount = 0;
                                                }
                                                else
                                                {
                                                    PaymentDetailIn[PaymentDetailIndex].ManagementFee = ManagementFeeCarryOver;
                                                    paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].ManagementFee));
                                                    PaymentDetailIn[PaymentDetailIndex].ManagementFeeCarryOver = 0;
                                                    ManagementFeeCarryOver = 0;
                                                }
                                                #endregion
                                            }
                                            if (maintenencefeeCarryOver > 0)
                                            {
                                                #region
                                                if (maintenencefeeCarryOver > paymentAmount)
                                                {
                                                    PaymentDetailIn[PaymentDetailIndex].MaintenanceFee = paymentAmount;
                                                    maintenencefeeCarryOver -= paymentAmount;
                                                    PaymentDetailIn[PaymentDetailIndex].MaintenanceFeeCarryOver = maintenencefeeCarryOver;
                                                    paymentAmount = 0;
                                                }
                                                else
                                                {
                                                    PaymentDetailIn[PaymentDetailIndex].MaintenanceFee = maintenencefeeCarryOver;
                                                    paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].MaintenanceFee));
                                                    PaymentDetailIn[PaymentDetailIndex].MaintenanceFeeCarryOver = 0;
                                                    maintenencefeeCarryOver = 0;
                                                }
                                                #endregion
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            #region calculate management and maintenance fee amount
                                            if (PaymentDetailIn[PaymentDetailIndex].ManagementFee > 0)
                                            {
                                                #region
                                                if (PaymentDetailIn[PaymentDetailIndex].ManagementFee > paymentAmount)
                                                {
                                                    PaymentDetailIn[PaymentDetailIndex].ManagementFee = paymentAmount;
                                                    PaymentDetailIn[PaymentDetailIndex].ManagementFeeCarryOver = ManagementFeeCarryOver;
                                                    paymentAmount = 0;
                                                }
                                                else
                                                {
                                                    //ManagementFeeCarryOver += payments[payments.Count - 1].ManagementFee;
                                                    //payments[payments.Count - 1].ManagementFee = (ManagementFeeCarryOver > paymentAmount) ? paymentAmount : (paymentAmount - ManagementFeeCarryOver);
                                                    //ManagementFeeCarryOver = ManagementFeeCarryOver - payments[payments.Count - 1].ManagementFee;
                                                    PaymentDetailIn[PaymentDetailIndex].ManagementFeeCarryOver = ManagementFeeCarryOver;
                                                    paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].ManagementFee));
                                                }
                                                #endregion
                                            }
                                            if (PaymentDetailIn[PaymentDetailIndex].MaintenanceFee > 0)
                                            {
                                                #region
                                                if (PaymentDetailIn[PaymentDetailIndex].MaintenanceFee > paymentAmount)
                                                {
                                                    PaymentDetailIn[PaymentDetailIndex].MaintenanceFee = paymentAmount;
                                                    PaymentDetailIn[PaymentDetailIndex].MaintenanceFeeCarryOver = maintenencefeeCarryOver;
                                                    paymentAmount = 0;
                                                }
                                                else
                                                {
                                                    PaymentDetailIn[PaymentDetailIndex].MaintenanceFeeCarryOver = maintenencefeeCarryOver;
                                                    paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].MaintenanceFee));
                                                }
                                                #endregion
                                            }
                                            #endregion
                                        }
                                    }
                                    else
                                    {
                                        #region calculate management and maintenance fee amount
                                        PaymentDetailIn[PaymentDetailIndex].ManagementFee = ManagementFeeCarryOver;
                                        PaymentDetailIn[PaymentDetailIndex].ManagementFeeCarryOver = 0;
                                        ManagementFeeCarryOver = 0;
                                        PaymentDetailIn[PaymentDetailIndex].MaintenanceFee = maintenencefeeCarryOver;
                                        PaymentDetailIn[PaymentDetailIndex].MaintenanceFeeCarryOver = 0;
                                        maintenencefeeCarryOver = 0;
                                        #endregion
                                    }
                                    #region
                                    //if (PaymentDetailIn[PaymentDetailIndex].ManagementFee > 0)
                                    //{
                                    //    #region
                                    //    if (PaymentDetailIn[PaymentDetailIndex].ManagementFee > paymentAmount)
                                    //    {
                                    //        PaymentDetailIn[PaymentDetailIndex].ManagementFee = paymentAmount;
                                    //        paymentAmount = 0;
                                    //    }
                                    //    else
                                    //    {
                                    //        paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].ManagementFee));
                                    //    }
                                    //    #endregion
                                    //}
                                    //if (PaymentDetailIn[PaymentDetailIndex].MaintenanceFee > 0)
                                    //{
                                    //    #region
                                    //    if (PaymentDetailIn[PaymentDetailIndex].MaintenanceFee > paymentAmount)
                                    //    {
                                    //        PaymentDetailIn[PaymentDetailIndex].MaintenanceFee = paymentAmount;
                                    //        paymentAmount = 0;
                                    //    }
                                    //    else
                                    //    {
                                    //        paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailIn[PaymentDetailIndex].MaintenanceFee));
                                    //    }
                                    //    #endregion
                                    //}
                                    #endregion
                                    if (paymentAmount > 0)//When payment amount remains after full payment of schedule
                                    {
                                        #region
                                        double remainingPincipal = PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal - PaymentDetailIn[PaymentDetailIndex].PrincipalPayment;
                                        remainingPincipal = remainingPincipal <= 0 ? 0 : remainingPincipal;
                                        double remainingServiceFee = PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee - PaymentDetailIn[PaymentDetailIndex].ServiceFee;
                                        remainingServiceFee = remainingServiceFee <= 0 ? 0 : remainingServiceFee;
                                        #region
                                        if (PaymentDetailIn[PaymentDetailIndex].InterestCarryOver > paymentAmount)
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].InterestPayment += paymentAmount;
                                            PaymentDetailIn[PaymentDetailIndex].InterestCarryOver -= paymentAmount;
                                            paymentAmount = 0;
                                        }
                                        else
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].InterestPayment += PaymentDetailIn[PaymentDetailIndex].InterestCarryOver;
                                            paymentAmount = rounndedValue(paymentAmount - PaymentDetailIn[PaymentDetailIndex].InterestCarryOver);
                                            PaymentDetailIn[PaymentDetailIndex].InterestCarryOver = 0;

                                        }
                                        #endregion

                                        #region
                                        if (PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver > paymentAmount)
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest += paymentAmount;
                                            PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver -= paymentAmount;
                                            paymentAmount = 0;
                                        }
                                        else
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest += PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver;
                                            paymentAmount = rounndedValue(paymentAmount - PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver);
                                            PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver = 0;
                                        }
                                        #endregion

                                        #region
                                        if (remainingPincipal > paymentAmount)
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].PrincipalPayment += paymentAmount;
                                            paymentAmount = 0;
                                        }
                                        else
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].PrincipalPayment += remainingPincipal;
                                            paymentAmount = rounndedValue(paymentAmount - remainingPincipal);
                                        }
                                        #endregion

                                        #region
                                        if (remainingServiceFee > paymentAmount)
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].ServiceFee += paymentAmount;
                                            paymentAmount = 0;
                                        }
                                        else
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].ServiceFee += remainingServiceFee;
                                            paymentAmount = rounndedValue(paymentAmount - remainingServiceFee);
                                        }
                                        #endregion

                                        #region
                                        if (PaymentDetailIn[PaymentDetailIndex].ManagementFeeCarryOver > paymentAmount)
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].ManagementFee += paymentAmount;
                                            ManagementFeeCarryOver -= paymentAmount;
                                            PaymentDetailIn[PaymentDetailIndex].ManagementFeeCarryOver -= paymentAmount;
                                            paymentAmount = 0;
                                        }
                                        else
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].ManagementFee += PaymentDetailIn[PaymentDetailIndex].ManagementFeeCarryOver;
                                            PaymentDetailIn[PaymentDetailIndex].ManagementFeeCarryOver = 0;
                                            ManagementFeeCarryOver = 0;
                                            paymentAmount = rounndedValue(paymentAmount - PaymentDetailIn[PaymentDetailIndex].ManagementFeeCarryOver);
                                        }
                                        #endregion

                                        #region
                                        if (PaymentDetailIn[PaymentDetailIndex].MaintenanceFeeCarryOver > paymentAmount)
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].MaintenanceFee += paymentAmount;
                                            maintenencefeeCarryOver -= paymentAmount;
                                            PaymentDetailIn[PaymentDetailIndex].MaintenanceFeeCarryOver -= paymentAmount;
                                            paymentAmount = 0;
                                        }
                                        else
                                        {
                                            PaymentDetailIn[PaymentDetailIndex].MaintenanceFee += PaymentDetailIn[PaymentDetailIndex].MaintenanceFeeCarryOver;
                                            PaymentDetailIn[PaymentDetailIndex].MaintenanceFeeCarryOver = 0;
                                            maintenencefeeCarryOver = 0;
                                            paymentAmount = rounndedValue(paymentAmount - PaymentDetailIn[PaymentDetailIndex].MaintenanceFeeCarryOver);
                                        }
                                        #endregion
                                        #endregion
                                    }

                                    #endregion
                                }
                                #endregion
                            }
                            else
                            {
                                #region Calculate Interest Payment and Principal Payment and Service Fee Amount 
                                PaymentDetailIn[PaymentDetailIndex].ManagementFee = Input.ManagementFee + PaymentDetailIn[PaymentDetailIndex - 1].ManagementFeeCarryOver;
                                PaymentDetailIn[PaymentDetailIndex].ManagementFeeCarryOver = 0;
                                PaymentDetailIn[PaymentDetailIndex].MaintenanceFee = Input.MaintenanceFee + PaymentDetailIn[PaymentDetailIndex - 1].MaintenanceFeeCarryOver;
                                PaymentDetailIn[PaymentDetailIndex].MaintenanceFeeCarryOver = 0;
                                PaymentDetailIn[PaymentDetailIndex].InterestPayment = PaymentDetailIn[PaymentDetailIndex].InterestDue;
                                PaymentDetailIn[PaymentDetailIndex].InterestCarryOver = 0;
                                PaymentDetailIn[PaymentDetailIndex].PrincipalPayment = rounndedValue((PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal));
                                PaymentDetailIn[PaymentDetailIndex].ServiceFee = rounndedValue(PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee);
                                PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver = 0;
                                #endregion
                            }
                            CurrentPrincipal = PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal - PaymentDetailIn[PaymentDetailIndex].PrincipalPayment;
                            PaymentDetailIn[PaymentDetailIndex].ServiceFee = PaymentDetailIn[PaymentDetailIndex].ServiceFee <= 0 ? 0 : PaymentDetailIn[PaymentDetailIndex].ServiceFee;

                            CurrentServiceFee = rounndedValue(PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee - PaymentDetailIn[PaymentDetailIndex].ServiceFee);

                            if (CurrentPrincipal <= 0 && ManagementFeeCarryOver <= 0 && maintenencefeeCarryOver <= 0)
                            {
                                #region
                                PaymentDetailIn[PaymentDetailIndex].TotalPayment = rounndedValue(PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal + PaymentDetailIn[PaymentDetailIndex].InterestAccrued);
                                PaymentDetailIn[PaymentDetailIndex].PrincipalPayment = rounndedValue(PaymentDetailIn[PaymentDetailIndex].TotalPayment - rounndedValue(PaymentDetailIn[PaymentDetailIndex].InterestPayment));
                                PaymentDetailIn[PaymentDetailIndex].CumulativePayment = PaymentDetailIn[PaymentDetailIndex - 1].CumulativePayment + PaymentDetailIn[PaymentDetailIndex].TotalPayment;

                                // AD 1.0.0.3 - fixed count cumulative principal for the last payment when applied both additional payments and early payoff
                                PaymentDetailIn[PaymentDetailIndex].CumulativePrincipal = PaymentDetailIn[PaymentDetailIndex - 1].CumulativePrincipal + PaymentDetailIn[PaymentDetailIndex].PrincipalPayment;
                                PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest += PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver;
                                for (int j = PaymentDetailIn.Count - 1; j > PaymentDetailIndex; j--)
                                {
                                    PaymentDetailIn.RemoveAt(j);
                                }
                                PaymentDetailIn[PaymentDetailIndex].ServiceFee = rounndedValue(PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee);
                                PaymentDetailIn[PaymentDetailIndex].ServiceFeeTotal = rounndedValue(PaymentDetailIn[PaymentDetailIndex].ServiceFee + PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest);
                                #endregion
                            }

                            // AD 1.0.0.7 - add service fee

                            if (CurrentServiceFee <= 0 && PaymentDetailIn[PaymentDetailIndex].serviceFeeInterestCarryOver <= 0)
                            {
                                #region
                                PaymentDetailIn[PaymentDetailIndex].ServiceFeeTotal = rounndedValue(PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee + (isServiceFeeInterestApplied ? PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest : 0.0));
                                PaymentDetailIn[PaymentDetailIndex].ServiceFee = rounndedValue(PaymentDetailIn[PaymentDetailIndex].ServiceFeeTotal - rounndedValue(PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest));
                                PaymentDetailIn[PaymentDetailIndex].ServiceFee = PaymentDetailIn[PaymentDetailIndex].ServiceFee <= 0 ? 0 : PaymentDetailIn[PaymentDetailIndex].ServiceFee;
                                PaymentDetailIn[PaymentDetailIndex].CumulativeServiceFee = PaymentDetailIn[PaymentDetailIndex - 1].CumulativeServiceFee + PaymentDetailIn[PaymentDetailIndex].ServiceFeeTotal;
                                PaymentDetailIn[PaymentDetailIndex].CumulativeServiceFee = PaymentDetailIn[PaymentDetailIndex - 1].CumulativeServiceFee + PaymentDetailIn[PaymentDetailIndex].ServiceFee;
                                #endregion
                            }
                            #region CALCULATE CUMULATIVE AMOUNT
                            PaymentDetailIn[PaymentDetailIndex].CumulativeInterest = PaymentDetailIn[PaymentDetailIndex - 1].CumulativeInterest + PaymentDetailIn[PaymentDetailIndex].InterestPayment;
                            PaymentDetailIn[PaymentDetailIndex].CumulativePayment = PaymentDetailIn[PaymentDetailIndex - 1].CumulativePayment + PaymentDetailIn[PaymentDetailIndex].TotalPayment;
                            PaymentDetailIn[PaymentDetailIndex].CumulativePrincipal = PaymentDetailIn[PaymentDetailIndex - 1].CumulativePrincipal + PaymentDetailIn[PaymentDetailIndex].PrincipalPayment;
                            PaymentDetailIn[PaymentDetailIndex].CumulativeServiceFee = PaymentDetailIn[PaymentDetailIndex - 1].CumulativeServiceFee + PaymentDetailIn[PaymentDetailIndex].ServiceFee;
                            PaymentDetailIn[PaymentDetailIndex].CumulativeServiceFeeInterest = PaymentDetailIn[PaymentDetailIndex - 1].CumulativeServiceFeeInterest + PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest;
                            PaymentDetailIn[PaymentDetailIndex].CumulativeMaintenanceFee = PaymentDetailIn[PaymentDetailIndex - 1].CumulativeMaintenanceFee + PaymentDetailIn[PaymentDetailIndex].MaintenanceFee;
                            PaymentDetailIn[PaymentDetailIndex].CumulativeManagementFee = PaymentDetailIn[PaymentDetailIndex - 1].CumulativeManagementFee + PaymentDetailIn[PaymentDetailIndex].ManagementFee;
                            PaymentDetailIn[PaymentDetailIndex].CumulativeOriginationFee = PaymentDetailIn[PaymentDetailIndex - 1].CumulativeOriginationFee + PaymentDetailIn[PaymentDetailIndex].OriginationFee;
                            PaymentDetailIn[PaymentDetailIndex].CumulativeSameDayFee = PaymentDetailIn[PaymentDetailIndex - 1].CumulativeSameDayFee + PaymentDetailIn[PaymentDetailIndex].SameDayFee;
                            PaymentDetailIn[PaymentDetailIndex].CumulativeTotalFees = rounndedValue((PaymentDetailIn[PaymentDetailIndex].CumulativeOriginationFee + PaymentDetailIn[PaymentDetailIndex].CumulativeSameDayFee + PaymentDetailIn[PaymentDetailIndex].CumulativeMaintenanceFee +
                                                                    PaymentDetailIn[PaymentDetailIndex].CumulativeManagementFee + PaymentDetailIn[PaymentDetailIndex].CumulativeServiceFee + PaymentDetailIn[PaymentDetailIndex].CumulativeServiceFeeInterest));
                            #endregion
                            #endregion
                        }
                        PreviousAdditionalPaymentSum = 0;
                        PreviousServiceFeeAdditionalPaymentSum = 0;
                    }
                    PaymentDetailIndex = PaymentDetailIndex + 1;
                }
            }
        }

        /// <summary>
        /// AD 1.0.0.18 - calculae interest for period which contains additional payments 
        /// </summary>
        /// <param name="endPeriodDate"></param>
        /// <param name="InputRecords"></param>
        /// <returns></returns>
        private static DateTime getStartPeriodDate(DateTime endPeriodDate, List<InputRecord> InputRecords)
        {
            var nearestPreceedingDate = InputRecords.Where(p => p.DateIn < endPeriodDate).Max(p => p.DateIn);
            return nearestPreceedingDate;
        }

        /// <summary>
        /// This method is used to calculate begnning principal and begnning service fee just after the additional payment.
        /// </summary>
        /// <param name="endPeriodDate"></param>
        /// <param name="PaymentDetailIn"></param>
        /// <param name="isPrincipal"></param>
        /// <returns></returns>
        private static double getStartPeriodBeginningPrincipal(DateTime endPeriodDate, List<PaymentDetail> PaymentDetailIn, bool isPrincipal)
        {
            if (PaymentDetailIn.Any(p => p.PaymentDate < endPeriodDate && p.Flags == 0))
            {
                var nearestPreceedingDate = PaymentDetailIn.Where(p => p.PaymentDate < endPeriodDate && p.Flags == 0).Max(p => p.PaymentDate);
                var nearestPreceedingScheduledPayment = PaymentDetailIn.FirstOrDefault(p => p.PaymentDate == nearestPreceedingDate && p.Flags == 0);
                return isPrincipal ? (nearestPreceedingScheduledPayment.BeginningPrincipal - nearestPreceedingScheduledPayment.PrincipalPayment) :
                    (nearestPreceedingScheduledPayment.BeginningServiceFee - nearestPreceedingScheduledPayment.ServiceFee);
            }
            else
            {
                // no preceeding payments (additional payment occurs before first payment)
                // start BeginningPrincipal is used
                return isPrincipal ? PaymentDetailIn.FirstOrDefault().BeginningPrincipal : PaymentDetailIn.FirstOrDefault().BeginningServiceFee;
            }
        }

        /// <summary>
        /// This method is used to calculate interest accrued just after the additional payemnt date
        /// </summary>
        /// <param name="startPeriod"></param>
        /// <param name="endPeriod"></param>
        /// <param name="startPrincipalAmount"></param>
        /// <param name="AdditionalPaymentRecords"></param>
        /// <param name="interestRate"></param>
        /// <param name="daysInYear"></param>
        /// <returns></returns>
        private static double calculateInterestForPeriodWithAdditionalPayment(DateTime startPeriod, DateTime endPeriod, double startPrincipalAmount,
            List<AdditionalPaymentRecord> AdditionalPaymentRecords, double interestRate, short daysInYear)
        {
            var additionalPaymentsWithinPeriod =
            AdditionalPaymentRecords.Where(p => p.DateIn > startPeriod && p.DateIn <= endPeriod).OrderBy(p => p.DateIn).ToList();
            var periods = new List<additionalPaymentCalculatePeriod>();
            var preceedingDate = startPeriod;
            var period = new additionalPaymentCalculatePeriod();
            foreach (var additionalPayment in additionalPaymentsWithinPeriod)
            {
                period = periods.FirstOrDefault(p => p.EndDate == additionalPayment.DateIn);
                if (period == null)
                {
                    period = new additionalPaymentCalculatePeriod
                    {
                        StartDate = preceedingDate,
                        EndDate = additionalPayment.DateIn,
                        BeginningPrincipal = startPrincipalAmount
                    };

                    periods.Add(period);
                }

                preceedingDate = additionalPayment.DateIn;
            }

            // add last period
            period = periods.FirstOrDefault(p => p.EndDate == endPeriod);
            if (period == null)
            {
                period = new additionalPaymentCalculatePeriod
                {
                    StartDate = preceedingDate,
                    EndDate = endPeriod,
                    BeginningPrincipal = startPrincipalAmount
                };
                periods.Add(period);
            }


            // distribute interest payment accordng to periods lengths
            foreach (var interestPeriod in periods)
            {
                interestPeriod.DaysLength = (int)interestPeriod.EndDate.Subtract(interestPeriod.StartDate).TotalDays;
                var periodicRateForPeriod = ((double)interestPeriod.DaysLength / daysInYear) * interestRate * .01;
                interestPeriod.InterestAmount = rounndedValue(interestPeriod.BeginningPrincipal) * periodicRateForPeriod;
            }

            var totalInterest = periods.Sum(p => p.InterestAmount);

            return totalInterest;
        }

        /// <summary>
        ///  AD 1.0.0.7 - recast payments after additional payment
        ///  AD 1.0.0.10 - add parameter isRegularPaymentRounded
        ///  AD 1.0.0.18 - take into account changed interest in the period when additional payment was made
        /// </summary>
        /// <param name="PaymentDetailIn"></param>
        /// <param name="PaymentDetailIndex"></param>
        /// <param name="isServiceFeeInterestApplied"></param>
        /// <param name="isRegularPaymentRounded"></param>
        /// <param name="interestPayment"></param>
        /// <param name="serviceInterstPayment"></param>
        private static void RecastPayments(List<PaymentDetail> PaymentDetailIn, int PaymentDetailIndex, bool isServiceFeeInterestApplied, bool isRegularPaymentRounded,
            double interestPayment, double serviceInterstPayment)
        {
            var remainingPayments = PaymentDetailIn.Count - PaymentDetailIndex;
            var beginningPrincipal = PaymentDetailIn[PaymentDetailIndex].BeginningPrincipal;
            var beginningServiceFee = PaymentDetailIn[PaymentDetailIndex].BeginningServiceFee;
            var periodicInterestRate = PaymentDetailIn[PaymentDetailIndex].PeriodicInterestRate;

            // re-calculate remaining payment amount based on remaining principal amount
            // AD - 1.0.0.10 - use method to get regular payment, which takes rounding flag into account
            double recastPayment = getRegularPaymentAmount(periodicInterestRate, remainingPayments, beginningPrincipal, isRegularPaymentRounded);
            double recastServiceFee = getRegularPaymentAmount(isServiceFeeInterestApplied ? periodicInterestRate : 0.0, remainingPayments, beginningServiceFee, isRegularPaymentRounded);


            // recalculate interest for loan and service fee payments based on new payment amounts

            // AD 1.0.0.18 - use interest calculated for the period according to exact date of additional payment
            var recastInterestPayment = interestPayment;
            var recastPrincipalPayment = recastPayment - recastInterestPayment;
            PaymentDetailIn[PaymentDetailIndex].PrincipalPayment = recastPrincipalPayment;
            PaymentDetailIn[PaymentDetailIndex].InterestAccrued = recastInterestPayment;

            // AD 1.0.0.18 - use interest calculated for the period according to exact date of additional payment
            var recastServiceFeeInterestPayment = isServiceFeeInterestApplied ? serviceInterstPayment : 0.0;
            var recastServiceFeePrincipal = recastServiceFee - recastServiceFeeInterestPayment;
            PaymentDetailIn[PaymentDetailIndex].ServiceFee = recastServiceFeePrincipal;
            PaymentDetailIn[PaymentDetailIndex].ServiceFeeInterest = recastServiceFeeInterestPayment;

            // put new walue of Total Payment to all payments after additional
            // principal  and interest amounts will be calculated in the calling method
            for (int i = PaymentDetailIndex; i < PaymentDetailIn.Count; i++)
            {
                PaymentDetailIn[i].TotalPayment = recastPayment;
                PaymentDetailIn[i].ServiceFeeTotal = recastServiceFee;
            }
        }

        /// <summary>
        /// This method is used to create the default schedule for the Prorate method.
        /// </summary>
        /// <param name="Output"></param>
        /// <param name="PeriodicInterestRate"></param>
        /// <param name="DaysPerPeriod"></param>
        /// <param name="IgnoreEarlyPayoffDate"></param>
        /// <param name="isRegularPaymentRounded"></param>
        /// <param name="Input"></param>
        private static void adjustForOddFirstPeriod(getScheduleOutput Output, double PeriodicInterestRate, short DaysPerPeriod, bool IgnoreEarlyPayoffDate, bool isRegularPaymentRounded, getScheduleInput Input)
        {
            // AD - 1.0.0.4 : Add variables needed to re-count payments after implementing of pro-rating of the first payment
            double paymentAmount = 0;
            bool isPaymentAmount = false;
            double ManagementFeeCarryOver = 0;
            double maintenencefeeCarryOver = 0;
            double Payment = 0.0;
            double serviceFeeInterestDue = 0;
            double earnedServiceFeeTotal = 0;
            double earnedTotal = 0;
            DateTime startDateWithDelay = Input.InputRecords[0].DateIn.AddDays(Input.InterestDelay);
            DateTime startDate;
            // AD - 1.0.0.6 : Add variables needed to re-count service fee payments after implementing of pro-rating of the first payment
            double CurrentServiceFeeInterestPayment = 0.0;
            double ServiceFeePayment = 0.0;
            bool isServiceInterestApplied = (Input.ApplyServiceFeeInterest & 1) == 1;
            double ServiceFee = rounndedValue(getStartingServiceFee(Input.ServiceFee, Input.LoanAmount, Input.ApplyServiceFeeInterest));

            // AD : throw exception if payment dates are not defined in the grid and early payment date is ignored
            // i.e. no one payment date is known
            if (Output.Schedule.Count == 0 && IgnoreEarlyPayoffDate)
            {
                throw new System.ArgumentException("No one payment date is defined.");
            }

            // AD : if first payment date is after or on EarlyPayoff date, use EarlyPayoffDate to count FirstPeriodInterestRate
            // take into account if early payment date is ignored - then use first payment date from the list
            // added flag that early payoff is used
            DateTime firstPaymentDate = Input.EarlyPayoffDate;
            if (IgnoreEarlyPayoffDate || (Output.Schedule.Count > 0 && Output.Schedule[0].PaymentDate < firstPaymentDate))
            {
                firstPaymentDate = Output.Schedule[0].PaymentDate;
            }
            double FirstPeriodInterestRate = PeriodicInterestRate;
            if (Input.MinDuration > 0)
            {
                #region
                long DaysBeforeFirstPayment = DateAndTime.DateDiff(DateInterval.Day, Input.InputRecords[0].DateIn, firstPaymentDate, FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1);
                double CurrentPrincipal = rounndedValue(Input.LoanAmount);

                // AD - 1.0.0.6 : Add variables needed to re-count service fee payments after implementing of pro-rating of the first payment
                double CurrentServiceFeePrincipal = ServiceFee;
                FirstPeriodInterestRate = PeriodicInterestRate * DaysBeforeFirstPayment / DaysPerPeriod;
                for (int i = 0; i < Output.Schedule.Count; i++)
                {
                    Output.Schedule[i].BeginningPrincipal = rounndedValue(CurrentPrincipal);
                    // AD 1.0.0.8 - count service payment after pro-rating
                    Output.Schedule[i].BeginningServiceFee = rounndedValue(CurrentServiceFeePrincipal);
                    if (i == 0)
                    {
                        #region calculated for first schedule
                        startDate = startDateWithDelay >= Input.InputRecords[0].DateIn ? startDateWithDelay : Input.InputRecords[0].DateIn;
                        long firstScheduleDifference = (startDate <= Output.Schedule[i].PaymentDate) ? (Output.Schedule[i].PaymentDate - startDate).Days : 0;

                        FirstPeriodInterestRate = CalculatePeriodicInterestRateTier(Input, CurrentPrincipal, Convert.ToInt32(firstScheduleDifference));
                        Output.Schedule[i].DailyInterestAmount = (FirstPeriodInterestRate == 0 || firstScheduleDifference == 0) ? 0 : rounndedValue((FirstPeriodInterestRate / firstScheduleDifference) * CurrentPrincipal);
                        Output.Schedule[i].InterestAccrued = CalculateCurrentInterestTier(Input, CurrentPrincipal, Convert.ToInt32(firstScheduleDifference));
                        Output.Schedule[i].InterestAccrued = Output.Schedule[i].InterestAccrued <= 0 ? 0 : Output.Schedule[i].InterestAccrued;

                        // AD - 1.0.0.6 - count service fee payment
                        Output.Schedule[i].AccruedServiceFeeInterest = isServiceInterestApplied ? CalculateCurrentServiceFeeInterestTier(Input, Output.Schedule[i].BeginningServiceFee, Convert.ToInt32(firstScheduleDifference)) : 0;
                        Output.Schedule[i].AccruedServiceFeeInterest = Output.Schedule[i].AccruedServiceFeeInterest <= 0 ? 0 : Output.Schedule[i].AccruedServiceFeeInterest;

                        Output.Schedule[i].TotalPayment = rounndedValue(Output.RegularPayment * DaysBeforeFirstPayment / DaysPerPeriod);
                        Output.Schedule[i].ServiceFeeTotal = rounndedValue(Output.RegularServiceFeePayment * DaysBeforeFirstPayment / DaysPerPeriod);
                        Output.Schedule[i].PeriodicInterestRate = FirstPeriodInterestRate;

                        #region This is used for earned fee
                        Output.Schedule[i].InterestAccrued += Input.EarnedInterest;
                        Output.Schedule[i].AccruedServiceFeeInterest += Input.EarnedServiceFeeInterest;
                        earnedTotal = rounndedValue(Input.EarnedInterest / (Output.Schedule.Count));
                        earnedServiceFeeTotal = rounndedValue(Input.EarnedServiceFeeInterest / Output.Schedule.Count);
                        Output.Schedule[i].TotalPayment = rounndedValue(Output.Schedule[i].TotalPayment + earnedTotal);
                        Output.Schedule[i].ServiceFeeTotal = rounndedValue(Output.Schedule[i].ServiceFeeTotal + earnedServiceFeeTotal);
                        #endregion =================================================

                        Output.Schedule[i].InterestDue = Output.Schedule[i].InterestAccrued;
                        Output.Schedule[i].ServiceFeeInterest = Output.Schedule[i].AccruedServiceFeeInterest;
                        serviceFeeInterestDue = Output.Schedule[i].ServiceFeeInterest;

                        #region calculate interest Payment and interest Carry over amount
                        if (Output.Schedule[i].InterestDue > Output.Schedule[i].TotalPayment)
                        {
                            Output.Schedule[i].InterestCarryOver = Output.Schedule[i].InterestAccrued - Output.Schedule[i].TotalPayment;
                            Output.Schedule[i].InterestPayment = Output.Schedule[i].TotalPayment;
                            Output.Schedule[i].PrincipalPayment = 0;
                        }
                        else
                        {
                            Output.Schedule[i].InterestCarryOver = 0;
                            Output.Schedule[i].InterestPayment = Output.Schedule[i].InterestAccrued;
                            Output.Schedule[i].PrincipalPayment = rounndedValue(Output.Schedule[i].TotalPayment - rounndedValue(Output.Schedule[i].InterestPayment));
                        }
                        Output.Schedule[i].PrincipalPayment = Output.Schedule[i].PrincipalPayment >= CurrentPrincipal ? CurrentPrincipal : Output.Schedule[i].PrincipalPayment;
                        Output.Schedule[i].PrincipalPayment = Output.Schedule[i].PrincipalPayment <= 0 ? 0 : Output.Schedule[i].PrincipalPayment;

                        #region This is used for the enforce payment principal
                        if (i >= (Input.EnforcedPayment - 1) && Output.Schedule[i].PrincipalPayment == 0 && Output.Schedule[i].InterestPayment > 0 && (i != Output.Schedule.Count - 1))
                        {
                            Output.Schedule[i].PrincipalPayment = Output.Schedule[i].InterestPayment <= Input.EnforcedPrincipal ? Output.Schedule[i].InterestPayment : rounndedValue(Input.EnforcedPrincipal);
                            Output.Schedule[i].InterestPayment -= Output.Schedule[i].PrincipalPayment;
                            Output.Schedule[i].InterestCarryOver += Output.Schedule[i].PrincipalPayment;
                            Output.Schedule[i].PrincipalPayment = Output.Schedule[i].PrincipalPayment <= CurrentPrincipal ? Output.Schedule[i].PrincipalPayment : CurrentPrincipal;
                            Output.Schedule[i].PrincipalPayment = Output.Schedule[i].PrincipalPayment <= 0 ? 0 : Output.Schedule[i].PrincipalPayment;
                        }
                        #endregion
                        #endregion

                        #region calculate Service fee interest Payment andservice fee interest Carry over amount
                        if (Input.ServiceFeeFirstPayment)
                        {
                            Output.Schedule[i].ServiceFee = rounndedValue(Output.Schedule[i].BeginningServiceFee);
                            Output.Schedule[i].ServiceFeeTotal = rounndedValue(Output.Schedule[i].ServiceFee + Output.Schedule[i].ServiceFeeInterest);
                            Output.Schedule[i].serviceFeeInterestCarryOver = 0;
                        }
                        else
                        {

                            if (serviceFeeInterestDue > Output.Schedule[i].ServiceFeeTotal)
                            {
                                Output.Schedule[i].serviceFeeInterestCarryOver = serviceFeeInterestDue - Output.Schedule[i].ServiceFeeTotal;
                                Output.Schedule[i].ServiceFeeInterest = Output.Schedule[i].ServiceFeeTotal;
                                Output.Schedule[i].ServiceFee = 0;
                            }
                            else
                            {
                                Output.Schedule[i].serviceFeeInterestCarryOver = 0;
                                Output.Schedule[i].ServiceFeeInterest = Output.Schedule[i].ServiceFeeInterest;
                                Output.Schedule[i].ServiceFee = rounndedValue(Output.Schedule[i].ServiceFeeTotal - rounndedValue(Output.Schedule[i].ServiceFeeInterest));
                            }
                            Output.Schedule[i].ServiceFee = Output.Schedule[i].ServiceFee >= ServiceFee ? ServiceFee : Output.Schedule[i].ServiceFee;
                            Output.Schedule[i].ServiceFee = Output.Schedule[i].ServiceFee <= 0 ? 0 : Output.Schedule[i].ServiceFee;
                        }
                        #endregion

                        #region Calculate new new Regular payment and ServicefeeRegular payment amount after first payment pro-rating
                        //This is the only PeriodicInterestRate that can change in this method. 
                        // AD - 1.0.0.4 - count of new total payment amount after first payment pro-rating
                        // AD - 1.0.0.10 - use method to get regular payment, which takes rounding flag into account
                        var remaingPrincipal = (Input.LoanAmount - Output.Schedule[i].PrincipalPayment);

                        remaingPrincipal = remaingPrincipal <= 0 ? 0 : remaingPrincipal;
                        Payment = rounndedValue((getRegularPaymentAmount(PeriodicInterestRate, Output.Schedule.Count - 1, remaingPrincipal, isRegularPaymentRounded)));

                        if (Input.InterestRate >= 0)
                        {
                            Payment = rounndedValue(CalculateRegularAmountWithTier(remaingPrincipal, PeriodicInterestRate, PeriodicInterestRate, Payment, Output.Schedule.Count - 1, Input.InputRecords, Input, true, true, false));

                        }

                        // AD - 1.0.0.4 - count of new total payment amount after first payment pro-rating
                        // AD - 1.0.0.10 - use method to get service fee regular payment, which takes rounding flag into account
                        var remaingServiceFee = rounndedValue(ServiceFee - Output.Schedule[i].ServiceFee);
                        remaingServiceFee = remaingServiceFee <= 0 ? 0 : remaingServiceFee;
                        ServiceFeePayment = getRegularPaymentAmount((isServiceInterestApplied ? PeriodicInterestRate : 0), Output.Schedule.Count - 1, remaingServiceFee, isRegularPaymentRounded);

                        if (Input.InterestRate >= 0)
                        {
                            ServiceFeePayment = CalculateRegularAmountWithTier(remaingServiceFee, (isServiceInterestApplied ? PeriodicInterestRate : 0), PeriodicInterestRate, ServiceFeePayment, Output.Schedule.Count - 1, Input.InputRecords, Input, (isServiceInterestApplied ? true : false), true, true);

                        }
                        ServiceFeePayment += ServiceFee == 0 ? earnedServiceFeeTotal : 0;
                        ServiceFeePayment = rounndedValue(ServiceFeePayment);
                        #endregion

                        #endregion
                    }
                    else
                    {
                        #region
                        // AD - 1.0.0.4 - fill other payment except first with re-calculated payment values
                        Output.Schedule[i].BeginningPrincipal = CurrentPrincipal;
                        Output.Schedule[i].BeginningServiceFee = CurrentServiceFeePrincipal;
                        PeriodicInterestRate = CalculatePeriodicInterestRateTier(Input, CurrentPrincipal, DaysPerPeriod);
                        Output.Schedule[i].PeriodicInterestRate = PeriodicInterestRate;
                        Output.Schedule[i].InterestAccrued = CalculateCurrentInterestTier(Input, CurrentPrincipal, DaysPerPeriod);
                        CurrentServiceFeeInterestPayment = isServiceInterestApplied ? CalculateCurrentServiceFeeInterestTier(Input, CurrentServiceFeePrincipal, DaysPerPeriod) : 0;
                        Output.Schedule[i].AccruedServiceFeeInterest = isServiceInterestApplied ? CalculateCurrentServiceFeeInterestTier(Input, Output.Schedule[i].BeginningServiceFee, DaysPerPeriod) : 0;

                        Output.Schedule[i].DailyInterestAmount = (PeriodicInterestRate == 0 || DaysPerPeriod == 0) ? 0 : rounndedValue((PeriodicInterestRate / DaysPerPeriod) * CurrentPrincipal);
                        Output.Schedule[i].InterestDue = Output.Schedule[i].InterestAccrued + Output.Schedule[i - 1].InterestCarryOver;
                        serviceFeeInterestDue = CurrentServiceFeeInterestPayment + Output.Schedule[i - 1].serviceFeeInterestCarryOver;
                        Output.Schedule[i].TotalPayment = Payment;
                        Output.Schedule[i].ServiceFeeTotal = ServiceFeePayment;

                        #region calculate interest Payment and interest Carry over amount
                        if (Output.Schedule[i].InterestDue > Payment)
                        {
                            Output.Schedule[i].InterestCarryOver = Output.Schedule[i].InterestDue - Payment;
                            Output.Schedule[i].InterestPayment = Payment;
                            Output.Schedule[i].PrincipalPayment = 0;
                        }
                        else
                        {
                            Output.Schedule[i].InterestCarryOver = 0;
                            Output.Schedule[i].InterestPayment = Output.Schedule[i].InterestAccrued + Output.Schedule[i - 1].InterestCarryOver;
                            Output.Schedule[i].PrincipalPayment = rounndedValue(Output.Schedule[i].TotalPayment - rounndedValue(Output.Schedule[i].InterestPayment));
                        }
                        Output.Schedule[i].PrincipalPayment = Output.Schedule[i].PrincipalPayment <= 0 ? 0 : Output.Schedule[i].PrincipalPayment;
                        #region This is used for the enforce payment principal
                        if (i >= (Input.EnforcedPayment - 1) && Output.Schedule[i].PrincipalPayment == 0 && Output.Schedule[i].InterestPayment > 0 && (i != Output.Schedule.Count - 1))
                        {
                            Output.Schedule[i].PrincipalPayment = Output.Schedule[i].InterestPayment <= Input.EnforcedPrincipal ? Output.Schedule[i].InterestPayment : rounndedValue(Input.EnforcedPrincipal);
                            Output.Schedule[i].InterestPayment -= Output.Schedule[i].PrincipalPayment;
                            Output.Schedule[i].InterestCarryOver += Output.Schedule[i].PrincipalPayment;
                            Output.Schedule[i].PrincipalPayment = Output.Schedule[i].PrincipalPayment <= CurrentPrincipal ? Output.Schedule[i].PrincipalPayment : CurrentPrincipal;
                            Output.Schedule[i].PrincipalPayment = Output.Schedule[i].PrincipalPayment <= 0 ? 0 : Output.Schedule[i].PrincipalPayment;
                        }
                        #endregion

                        #endregion

                        #region calculate Service fee interest Payment andservice fee interest Carry over amount
                        // AD 1.0.0.8 - count service payment after pro-rating
                        if (Input.ServiceFeeFirstPayment)
                        {
                            Output.Schedule[i].ServiceFee = Output.Schedule[i].BeginningServiceFee;
                            Output.Schedule[i].ServiceFeeInterest = serviceFeeInterestDue;
                            Output.Schedule[i].serviceFeeInterestCarryOver = 0;
                        }
                        else
                        {
                            if (serviceFeeInterestDue > ServiceFeePayment)
                            {
                                Output.Schedule[i].serviceFeeInterestCarryOver = serviceFeeInterestDue - ServiceFeePayment;
                                Output.Schedule[i].ServiceFeeInterest = ServiceFeePayment;
                                Output.Schedule[i].ServiceFee = 0;
                            }
                            else
                            {
                                Output.Schedule[i].serviceFeeInterestCarryOver = 0;
                                Output.Schedule[i].ServiceFeeInterest = serviceFeeInterestDue;
                                Output.Schedule[i].ServiceFee = rounndedValue(ServiceFeePayment - rounndedValue(Output.Schedule[i].ServiceFeeInterest));
                            }
                            Output.Schedule[i].ServiceFee = Output.Schedule[i].ServiceFee <= 0 ? 0 : Output.Schedule[i].ServiceFee;
                        }
                        #endregion

                        #endregion
                    }
                    if ((!string.IsNullOrEmpty(Input.InputRecords[i + 1].PaymentAmount) || Input.PaymentAmount > 0) && i != Output.Schedule.Count - 1)
                    {
                        #region Allocate amount on the basis of priority when Actual amount is given
                        if (!string.IsNullOrEmpty(Input.InputRecords[i + 1].PaymentAmount))
                        {
                            paymentAmount = rounndedValue(Convert.ToDouble(Input.InputRecords[i + 1].PaymentAmount));
                            isPaymentAmount = false;
                        }
                        else
                        {
                            isPaymentAmount = true;
                            paymentAmount = rounndedValue(Input.PaymentAmount + (i == 0 ? Input.OriginationFee + Input.SameDayFee : 0));
                            ManagementFeeCarryOver += Input.ManagementFee;
                            maintenencefeeCarryOver += i == 0 ? 0 : Input.MaintenanceFee;
                        }

                        if (Input.OriginationFee > 0 && i == 0)
                        {
                            #region
                            if (Input.OriginationFee > paymentAmount)
                            {
                                Output.Schedule[i].OriginationFee = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                paymentAmount = rounndedValue(paymentAmount - rounndedValue(Output.Schedule[i].OriginationFee));
                            }
                            #endregion
                        }
                        if (Input.SameDayFee > 0 && i == 0)
                        {
                            #region
                            if (Input.SameDayFee > paymentAmount)
                            {
                                Output.Schedule[i].SameDayFee = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                paymentAmount = rounndedValue(paymentAmount - rounndedValue(Output.Schedule[i].SameDayFee));
                            }
                            #endregion
                        }
                        if (Output.Schedule[i].InterestPayment > 0)
                        {
                            #region
                            if (Output.Schedule[i].InterestPayment > paymentAmount)
                            {
                                Output.Schedule[i].InterestCarryOver = Output.Schedule[i].InterestDue - paymentAmount;
                                Output.Schedule[i].InterestPayment = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                paymentAmount = rounndedValue(paymentAmount - rounndedValue(Output.Schedule[i].InterestPayment));
                            }
                            #endregion
                        }
                        if (Output.Schedule[i].ServiceFeeInterest > 0)
                        {
                            #region
                            if (Output.Schedule[i].ServiceFeeInterest > paymentAmount)
                            {
                                Output.Schedule[i].serviceFeeInterestCarryOver += Output.Schedule[i].ServiceFeeInterest - paymentAmount;
                                Output.Schedule[i].ServiceFeeInterest = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                paymentAmount = rounndedValue(paymentAmount - rounndedValue(Output.Schedule[i].ServiceFeeInterest));
                            }
                            #endregion
                        }
                        if (Output.Schedule[i].PrincipalPayment > 0)
                        {
                            #region
                            if (Output.Schedule[i].PrincipalPayment > paymentAmount)
                            {
                                Output.Schedule[i].PrincipalPayment = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                paymentAmount = rounndedValue(paymentAmount - rounndedValue(Output.Schedule[i].PrincipalPayment));
                            }
                            #endregion
                        }
                        if (Output.Schedule[i].ServiceFee > 0)
                        {
                            #region
                            if (Output.Schedule[i].ServiceFee > paymentAmount)
                            {
                                Output.Schedule[i].ServiceFee = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                paymentAmount = rounndedValue(paymentAmount - rounndedValue(Output.Schedule[i].ServiceFee));
                            }
                            #endregion
                        }
                        if (i != (Input.InputRecords.Count - 1))
                        {
                            if (isPaymentAmount)
                            {
                                #region calculate management and maintenance fee amount
                                if (ManagementFeeCarryOver > 0)
                                {
                                    #region
                                    if (ManagementFeeCarryOver > paymentAmount)
                                    {
                                        Output.Schedule[i].ManagementFee = paymentAmount;
                                        ManagementFeeCarryOver -= paymentAmount;
                                        Output.Schedule[i].ManagementFeeCarryOver = ManagementFeeCarryOver;
                                        paymentAmount = 0;
                                    }
                                    else
                                    {
                                        Output.Schedule[i].ManagementFee = ManagementFeeCarryOver;
                                        paymentAmount = rounndedValue(paymentAmount - rounndedValue(Output.Schedule[i].ManagementFee));
                                        Output.Schedule[i].ManagementFeeCarryOver = 0;
                                        ManagementFeeCarryOver = 0;
                                    }
                                    #endregion
                                }
                                if (maintenencefeeCarryOver > 0)
                                {
                                    #region
                                    if (maintenencefeeCarryOver > paymentAmount)
                                    {
                                        Output.Schedule[i].MaintenanceFee = paymentAmount;
                                        maintenencefeeCarryOver -= paymentAmount;
                                        Output.Schedule[i].MaintenanceFeeCarryOver = maintenencefeeCarryOver;
                                        paymentAmount = 0;
                                    }
                                    else
                                    {
                                        Output.Schedule[i].MaintenanceFee = maintenencefeeCarryOver;
                                        paymentAmount = rounndedValue(paymentAmount - rounndedValue(Output.Schedule[i].MaintenanceFee));
                                        Output.Schedule[i].MaintenanceFeeCarryOver = 0;
                                        maintenencefeeCarryOver = 0;
                                    }
                                    #endregion
                                }
                                #endregion
                            }
                            else
                            {
                                #region calculate management and maintenance fee amount
                                if (Output.Schedule[i].ManagementFee > 0)
                                {
                                    #region
                                    if (Output.Schedule[i].ManagementFee > paymentAmount)
                                    {
                                        Output.Schedule[i].ManagementFee = paymentAmount;
                                        Output.Schedule[i].ManagementFeeCarryOver = ManagementFeeCarryOver;
                                        paymentAmount = 0;
                                    }
                                    else
                                    {
                                        //ManagementFeeCarryOver += payments[payments.Count - 1].ManagementFee;
                                        //payments[payments.Count - 1].ManagementFee = (ManagementFeeCarryOver > paymentAmount) ? paymentAmount : (paymentAmount - ManagementFeeCarryOver);
                                        //ManagementFeeCarryOver = ManagementFeeCarryOver - payments[payments.Count - 1].ManagementFee;
                                        Output.Schedule[i].ManagementFeeCarryOver = ManagementFeeCarryOver;
                                        paymentAmount = rounndedValue(paymentAmount - rounndedValue(Output.Schedule[i].ManagementFee));
                                    }
                                    #endregion
                                }
                                if (Output.Schedule[i].MaintenanceFee > 0)
                                {
                                    #region
                                    if (Output.Schedule[i].MaintenanceFee > paymentAmount)
                                    {
                                        Output.Schedule[i].MaintenanceFee = paymentAmount;
                                        Output.Schedule[i].MaintenanceFeeCarryOver = maintenencefeeCarryOver;
                                        paymentAmount = 0;
                                    }
                                    else
                                    {

                                        //maintenencefeeCarryOver += payments[payments.Count - 1].MaintenanceFee;
                                        //payments[payments.Count - 1].MaintenanceFee = (maintenencefeeCarryOver > paymentAmount) ?  paymentAmount : (paymentAmount - maintenencefeeCarryOver);
                                        //maintenencefeeCarryOver = maintenencefeeCarryOver - payments[payments.Count - 1].MaintenanceFee;
                                        Output.Schedule[i].MaintenanceFeeCarryOver = maintenencefeeCarryOver;
                                        paymentAmount = rounndedValue(paymentAmount - rounndedValue(Output.Schedule[i].MaintenanceFee));
                                    }
                                    #endregion
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            #region calculate management and maintenance fee amount
                            Output.Schedule[i].ManagementFee = ManagementFeeCarryOver;
                            Output.Schedule[i].ManagementFeeCarryOver = 0;
                            ManagementFeeCarryOver = 0;
                            Output.Schedule[i].MaintenanceFee = maintenencefeeCarryOver;
                            Output.Schedule[i].MaintenanceFeeCarryOver = 0;
                            maintenencefeeCarryOver = 0;
                            #endregion
                        }

                        #region
                        //if (isPaymentAmount)
                        //{
                        //    if (i != (Input.InputRecords.Count - 1))
                        //    {
                        //        #region calculate management and maintenance fee amount
                        //        if (ManagementFeeCarryOver > 0)
                        //        {
                        //            #region
                        //            if (ManagementFeeCarryOver > paymentAmount)
                        //            {
                        //                Output.Schedule[i].ManagementFee = paymentAmount;
                        //                ManagementFeeCarryOver -= paymentAmount;
                        //                paymentAmount = 0;
                        //            }
                        //            else
                        //            {
                        //                Output.Schedule[i].ManagementFee = ManagementFeeCarryOver;
                        //                paymentAmount = rounndedValue(paymentAmount - rounndedValue(Output.Schedule[i].ManagementFee));
                        //                ManagementFeeCarryOver = 0;
                        //            }
                        //            #endregion
                        //        }
                        //        if (maintenencefeeCarryOver > 0)
                        //        {
                        //            #region
                        //            if (maintenencefeeCarryOver > paymentAmount)
                        //            {
                        //                Output.Schedule[i].MaintenanceFee = paymentAmount;
                        //                maintenencefeeCarryOver -= paymentAmount;
                        //                paymentAmount = 0;
                        //            }
                        //            else
                        //            {
                        //                Output.Schedule[i].MaintenanceFee = maintenencefeeCarryOver;
                        //                paymentAmount = rounndedValue(paymentAmount - rounndedValue(Output.Schedule[i].MaintenanceFee));
                        //                maintenencefeeCarryOver = 0;
                        //            }
                        //            #endregion
                        //        }
                        //        #endregion
                        //    }
                        //    else
                        //    {
                        //        Output.Schedule[i].ManagementFee = ManagementFeeCarryOver;
                        //        ManagementFeeCarryOver = 0;
                        //        Output.Schedule[i].MaintenanceFee = maintenencefeeCarryOver;
                        //        maintenencefeeCarryOver = 0;
                        //    }
                        //}
                        //else
                        //{
                        //    if (Output.Schedule[i].ManagementFee > 0)
                        //    {
                        //        #region
                        //        if (Output.Schedule[i].ManagementFee > paymentAmount)
                        //        {
                        //            Output.Schedule[i].ManagementFee = paymentAmount;
                        //            paymentAmount = 0;
                        //        }
                        //        else
                        //        {
                        //            paymentAmount = rounndedValue(paymentAmount - rounndedValue(Output.Schedule[i].ManagementFee));
                        //        }
                        //        #endregion
                        //    }
                        //    if (Output.Schedule[i].MaintenanceFee > 0)
                        //    {
                        //        #region
                        //        if (Output.Schedule[i].MaintenanceFee > paymentAmount)
                        //        {
                        //            Output.Schedule[i].MaintenanceFee = paymentAmount;
                        //            paymentAmount = 0;
                        //        }
                        //        else
                        //        {
                        //            paymentAmount = rounndedValue(paymentAmount - rounndedValue(Output.Schedule[i].MaintenanceFee));
                        //        }
                        //        #endregion
                        //    }
                        //}
                        #endregion
                        if (paymentAmount > 0)//When payment amount remains after full payment of schedule
                        {
                            #region
                            double remainingPincipal = CurrentPrincipal - Output.Schedule[i].PrincipalPayment;
                            remainingPincipal = remainingPincipal <= 0 ? 0 : remainingPincipal;
                            double remainingServiceFee = CurrentServiceFeePrincipal - Output.Schedule[i].ServiceFee;
                            remainingServiceFee = remainingServiceFee <= 0 ? 0 : remainingServiceFee;
                            #region
                            if (Output.Schedule[i].InterestCarryOver > paymentAmount)
                            {
                                Output.Schedule[i].InterestPayment += paymentAmount;
                                Output.Schedule[i].InterestCarryOver -= paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                Output.Schedule[i].InterestPayment += Output.Schedule[i].InterestCarryOver;
                                paymentAmount = rounndedValue(paymentAmount - Output.Schedule[i].InterestCarryOver);
                                Output.Schedule[i].InterestCarryOver = 0;

                            }
                            #endregion

                            #region
                            if (Output.Schedule[i].serviceFeeInterestCarryOver > paymentAmount)
                            {
                                Output.Schedule[i].ServiceFeeInterest += paymentAmount;
                                Output.Schedule[i].serviceFeeInterestCarryOver -= paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                Output.Schedule[i].ServiceFeeInterest += Output.Schedule[i].serviceFeeInterestCarryOver;
                                paymentAmount = rounndedValue(paymentAmount - Output.Schedule[i].serviceFeeInterestCarryOver);
                                Output.Schedule[i].serviceFeeInterestCarryOver = 0;
                            }
                            #endregion

                            #region
                            if (remainingPincipal > paymentAmount)
                            {
                                Output.Schedule[i].PrincipalPayment += paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                Output.Schedule[i].PrincipalPayment += remainingPincipal;
                                paymentAmount = rounndedValue(paymentAmount - remainingPincipal);
                            }
                            #endregion

                            #region
                            if (remainingServiceFee > paymentAmount)
                            {
                                Output.Schedule[i].ServiceFee += paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                Output.Schedule[i].ServiceFee += remainingServiceFee;
                                paymentAmount = rounndedValue(paymentAmount - remainingServiceFee);
                            }
                            #endregion

                            #region
                            if (Output.Schedule[i].ManagementFeeCarryOver > paymentAmount)
                            {
                                Output.Schedule[i].ManagementFee += paymentAmount;
                                ManagementFeeCarryOver -= paymentAmount;
                                Output.Schedule[i].ManagementFeeCarryOver -= paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                Output.Schedule[i].ManagementFee += Output.Schedule[i].ManagementFeeCarryOver;
                                Output.Schedule[i].ManagementFeeCarryOver = 0;
                                ManagementFeeCarryOver = 0;
                                paymentAmount = rounndedValue(paymentAmount - Output.Schedule[i].ManagementFeeCarryOver);
                            }
                            #endregion

                            #region
                            if (Output.Schedule[i].MaintenanceFeeCarryOver > paymentAmount)
                            {
                                Output.Schedule[i].MaintenanceFee += paymentAmount;
                                maintenencefeeCarryOver -= paymentAmount;
                                Output.Schedule[i].MaintenanceFeeCarryOver -= paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                Output.Schedule[i].MaintenanceFee += Output.Schedule[i].MaintenanceFeeCarryOver;
                                Output.Schedule[i].MaintenanceFeeCarryOver = 0;
                                maintenencefeeCarryOver = 0;
                                paymentAmount = rounndedValue(paymentAmount - Output.Schedule[i].MaintenanceFeeCarryOver);
                            }
                            #endregion

                            #endregion
                        }

                        #endregion
                    }
                    if (Output.Schedule[i].TotalPayment >= CurrentPrincipal + Output.Schedule[i].InterestPayment)
                    {
                        #region
                        Output.Schedule[i].TotalPayment = rounndedValue(CurrentPrincipal + Output.Schedule[i].InterestPayment);
                        if (i < Output.Schedule.Count - 1)
                        {
                            for (int j = Output.Schedule.Count - 1; j > i; j--)
                            {
                                Output.Schedule.RemoveAt(j);
                            }
                        }
                        #endregion
                    }
                    if (i == Output.Schedule.Count - 1)
                    {
                        #region
                        Output.Schedule[i].PrincipalPayment = Output.Schedule[i].BeginningPrincipal;
                        Output.Schedule[i].ServiceFee = Output.Schedule[i].BeginningServiceFee;
                        Output.Schedule[i].ServiceFeeInterest += Output.Schedule[i].serviceFeeInterestCarryOver;
                        Output.Schedule[i].serviceFeeInterestCarryOver = 0;
                        Output.Schedule[i].InterestPayment += Output.Schedule[i].InterestCarryOver;
                        Output.Schedule[i].InterestCarryOver = 0;
                        Output.Schedule[i].TotalPayment = rounndedValue(Output.Schedule[i].BeginningPrincipal + Output.Schedule[i].InterestPayment);
                        Output.Schedule[i].ServiceFeeTotal = rounndedValue(Output.Schedule[i].ServiceFeeInterest + Output.Schedule[i].BeginningServiceFee);
                        #endregion
                    }
                    CurrentPrincipal = rounndedValue((CurrentPrincipal - Output.Schedule[i].PrincipalPayment));
                    CurrentServiceFeePrincipal = rounndedValue((CurrentServiceFeePrincipal - Output.Schedule[i].ServiceFee));

                    #region calculate Cumulative amount
                    Output.Schedule[i].CumulativeInterest = i == 0 ? Output.Schedule[i].InterestPayment : Output.Schedule[i - 1].CumulativeInterest + Output.Schedule[i].InterestPayment;
                    Output.Schedule[i].CumulativePayment = i == 0 ? Output.Schedule[i].TotalPayment : Output.Schedule[i - 1].CumulativePayment + Output.Schedule[i].TotalPayment;
                    Output.Schedule[i].CumulativePrincipal = i == 0 ? Output.Schedule[i].PrincipalPayment : Output.Schedule[i].CumulativePrincipal + Output.Schedule[i].PrincipalPayment;

                    Output.Schedule[i].CumulativeMaintenanceFee = i == 0 ? Output.Schedule[i].MaintenanceFee : Output.Schedule[i - 1].CumulativeMaintenanceFee + Output.Schedule[i].MaintenanceFee;
                    Output.Schedule[i].CumulativeManagementFee = i == 0 ? Output.Schedule[i].ManagementFee : Output.Schedule[i - 1].CumulativeManagementFee + Output.Schedule[i].ManagementFee;
                    Output.Schedule[i].CumulativeOriginationFee = i == 0 ? Output.Schedule[i].OriginationFee : Output.Schedule[i - 1].CumulativeOriginationFee + Output.Schedule[i].OriginationFee;
                    Output.Schedule[i].CumulativeSameDayFee = i == 0 ? Output.Schedule[i].SameDayFee : Output.Schedule[i - 1].CumulativeSameDayFee + Output.Schedule[i].SameDayFee;
                    Output.Schedule[i].CumulativeServiceFee = i == 0 ? Output.Schedule[i].ServiceFee : Output.Schedule[i - 1].CumulativeServiceFee + Output.Schedule[i].ServiceFee;
                    Output.Schedule[i].CumulativeServiceFeeInterest = i == 0 ? Output.Schedule[i].ServiceFeeInterest : Output.Schedule[i - 1].CumulativeServiceFeeInterest + Output.Schedule[i].ServiceFeeInterest;
                    Output.Schedule[i].CumulativeTotalFees = rounndedValue((Output.Schedule[i].CumulativeOriginationFee + Output.Schedule[i].CumulativeSameDayFee + Output.Schedule[i].CumulativeMaintenanceFee +
                                                            Output.Schedule[i].CumulativeManagementFee + Output.Schedule[i].CumulativeServiceFee + Output.Schedule[i].CumulativeServiceFeeInterest));
                    #endregion
                }
                #endregion
            }
        }

        /// <summary>
        ///AD - 1.0.0.9 - create method to round all values correctly, so that to get precise cumulative principal values in the end 
        /// </summary>
        /// <param name="Output"></param>
        /// <param name="isInterestRounded"></param>
        /// <param name="Input"></param>
        private static void roundPaymentValues(getScheduleOutput Output, bool isInterestRounded, getScheduleInput Input)
        {
            double differenceServiceFee = 0;
            for (int i = 0; i < Output.Schedule.Count; i++)
            {

                Output.Schedule[i].PrincipalPayment = rounndedValue(Output.Schedule[i].PrincipalPayment);
                Output.Schedule[i].InterestAccrued = isInterestRounded ? rounndedValue(Output.Schedule[i].InterestAccrued) : Output.Schedule[i].InterestAccrued;
                Output.Schedule[i].DailyInterestAmount = rounndedValue(Output.Schedule[i].DailyInterestAmount);
                Output.Schedule[i].InterestCarryOver = isInterestRounded ? rounndedValue(Output.Schedule[i].InterestCarryOver) : Output.Schedule[i].InterestCarryOver;
                Output.Schedule[i].InterestDue = isInterestRounded ? rounndedValue(Output.Schedule[i].InterestDue) : Output.Schedule[i].InterestDue;
                Output.Schedule[i].ServiceFee = rounndedValue(Output.Schedule[i].ServiceFee);
                Output.Schedule[i].ServiceFeeInterest = isInterestRounded ? rounndedValue(Output.Schedule[i].ServiceFeeInterest) : Output.Schedule[i].ServiceFeeInterest;
                if (i > 0)
                {
                    Output.Schedule[i].BeginningPrincipal = rounndedValue(Output.Schedule[i - 1].BeginningPrincipal - Output.Schedule[i - 1].PrincipalPayment);
                    Output.Schedule[i].BeginningServiceFee = rounndedValue(Output.Schedule[i - 1].BeginningServiceFee - Output.Schedule[i - 1].ServiceFee);
                }

                if (i == Output.Schedule.Count - 1)
                {
                    #region
                    Output.Schedule[i].BeginningPrincipal = Output.Schedule[i].BeginningPrincipal < 0 ? 0 : Output.Schedule[i].BeginningPrincipal;
                    var differenceLoan = Output.Schedule[i].BeginningPrincipal - Output.Schedule[i].PrincipalPayment;
                    Output.Schedule[i].PrincipalPayment = Output.Schedule[i].BeginningPrincipal;

                    if (Output.Schedule[i].Flags != 2)
                    {
                        Output.Schedule[i].InterestPayment = Output.Schedule[i].InterestDue;
                        Output.Schedule[i].InterestCarryOver = 0;
                    }
                    if (Input.IsServiceFeeIncremental && Input.EarlyPayoffDate > Input.InputRecords[0].DateIn)
                    {
                        Output.Schedule[i].BeginningServiceFee = (Output.Schedule[i].Flags == 2) ? Output.Schedule[i].BeginningServiceFee : Output.Schedule[i].ServiceFee;
                        Output.Schedule[i].ServiceFee = Output.Schedule[i].BeginningServiceFee;
                    }
                    else
                    {
                        differenceServiceFee = Output.Schedule[i].BeginningServiceFee - Output.Schedule[i].ServiceFee;
                        Output.Schedule[i].ServiceFee = Output.Schedule[i].BeginningServiceFee;
                    }
                    Output.Schedule[i].TotalPayment += differenceLoan;
                    Output.Schedule[i].ServiceFeeTotal += differenceServiceFee;
                    #endregion
                }
                Output.Schedule[i].TotalPayment = rounndedValue(Output.Schedule[i].TotalPayment);
                Output.Schedule[i].ServiceFeeTotal = rounndedValue(Output.Schedule[i].ServiceFeeTotal);
                Output.Schedule[i].InterestPayment = rounndedValue(Output.Schedule[i].InterestPayment);
                Output.Schedule[i].PrincipalServiceFeePayment = rounndedValue(Output.Schedule[i].PrincipalPayment + Output.Schedule[i].ServiceFee);
                Output.Schedule[i].InterestServiceFeeInterestPayment = rounndedValue(Output.Schedule[i].InterestPayment + Output.Schedule[i].ServiceFeeInterest);
                Output.Schedule[i].BeginningPrincipalServiceFee = rounndedValue(Output.Schedule[i].BeginningPrincipal + Output.Schedule[i].BeginningServiceFee);


            }
        }

        /// <summary>
        /// This method is used to calculate the cummulative amount
        ///  AD 1.0.0.5 - recalculate cumulative values after all changes (prorating, payment deleting etc)
        /// AD 1.0.0.6 - recalculate cumulative values for additional fees
        /// </summary>
        /// <param name="Output"></param>
        private static void recalculateCumulativeValues(getScheduleOutput Output)
        {
            double CumulativePrincipal = 0.0;
            double CumulativeInterest = 0.0;

            double CumulativeServiceFee = 0.0;
            double CumulativeServiceFeeInterest = 0.0;
            double CumulativeOriginationFee = 0.0;
            double CumulativeSameDayFee = 0.0;
            double CumulativeManagementFee = 0.0;
            double CumulativeMaintenanceFee = 0.0;
            double CumulativeTotalFees = 0.0;
            double cumulativePayment = 0.0;

            for (int i = 0; i < Output.Schedule.Count; i++)
            {
                CumulativePrincipal = CumulativePrincipal + Output.Schedule[i].PrincipalPayment;
                CumulativeInterest = CumulativeInterest + rounndedValue(Output.Schedule[i].InterestPayment);
                CumulativeServiceFee += Output.Schedule[i].ServiceFee;
                CumulativeServiceFeeInterest += Output.Schedule[i].ServiceFeeInterest;
                CumulativeManagementFee += Output.Schedule[i].ManagementFee;
                CumulativeMaintenanceFee += Output.Schedule[i].MaintenanceFee;
                Output.Schedule[i].CumulativePrincipal = CumulativePrincipal;
                Output.Schedule[i].CumulativeInterest = CumulativeInterest;
                cumulativePayment += rounndedValue(Output.Schedule[i].TotalPayment);
                Output.Schedule[i].CumulativePayment = cumulativePayment;
                CumulativeOriginationFee += Output.Schedule[i].OriginationFee;
                CumulativeSameDayFee += Output.Schedule[i].SameDayFee;
                CumulativeTotalFees = CumulativeServiceFee +
                                      CumulativeServiceFeeInterest +
                                      CumulativeOriginationFee +
                                      CumulativeManagementFee +
                                      CumulativeMaintenanceFee +
                                      CumulativeSameDayFee;

                Output.Schedule[i].CumulativeServiceFee = CumulativeServiceFee;
                Output.Schedule[i].CumulativeServiceFeeInterest = CumulativeServiceFeeInterest;
                Output.Schedule[i].CumulativeOriginationFee = CumulativeOriginationFee;
                Output.Schedule[i].CumulativeSameDayFee = CumulativeSameDayFee;
                Output.Schedule[i].CumulativeMaintenanceFee = CumulativeMaintenanceFee;
                Output.Schedule[i].CumulativeManagementFee = CumulativeManagementFee;
                Output.Schedule[i].CumulativeTotalFees = CumulativeTotalFees;
            }
        }

        /// <summary>
        /// This mehod is use to calculate the service fee total and total payment amount
        /// </summary>
        /// <param name="Output"></param>
        private static void applyServiceFeesToTotalPayments(getScheduleOutput Output)
        {
            for (int i = 0; i < Output.Schedule.Count; i++)
            {
                //AD-34 - calculate service fee with rounded values
                Output.Schedule[i].ServiceFeeTotal = Output.Schedule[i].ServiceFee +
                                                    rounndedValue(Output.Schedule[i].ServiceFeeInterest);
                Output.Schedule[i].TotalPayment = Output.Schedule[i].PrincipalPayment +
                                                 rounndedValue(Output.Schedule[i].InterestPayment) +
                                                  Output.Schedule[i].ServiceFee +
                                                 rounndedValue(Output.Schedule[i].ServiceFeeInterest) +
                                                  Output.Schedule[i].OriginationFee +
                                                  Output.Schedule[i].SameDayFee +
                                                  Output.Schedule[i].MaintenanceFee +
                                                  Output.Schedule[i].ManagementFee;
            }
        }

        /// <summary>
        ///This method is used to days as per payment period selection.
        /// </summary>
        /// <param name="PmtPeriod"></param>
        /// <returns></returns>
        private static short getDaysPerPeriod(PaymentPeriod PmtPeriod)
        {
            switch (PmtPeriod)
            {
                case PaymentPeriod.Weekly: return 7;
                case PaymentPeriod.BiWeekly: return 14;
                case PaymentPeriod.SemiMonthly: return 15;
                case PaymentPeriod.Monthly: return 30;
                case PaymentPeriod.Daily: return 1;
                default:
                    throw new ArgumentOutOfRangeException("PmtPeriod", PmtPeriod, "PmtPeriod out of range.");
            }
        }

        /// <summary>
        /// This method is used to calculate the starting(begnning service fee) service fee on the basis of calculation method.
        /// AD - 1.0.0.7 - count starting Service Fee
        /// </summary>
        /// <param name="ServiceFeeAmount"></param>
        /// <param name="LoanAmount"></param>
        /// <param name="ApplyServiceFeeInterest"></param>
        /// <returns></returns>
        private static double getStartingServiceFee(double ServiceFeeAmount, double LoanAmount, int ApplyServiceFeeInterest)
        {
            bool isServiceFeePerHundred = (ApplyServiceFeeInterest & 2) == 2;

            double ServiceFee = ServiceFeeAmount;
            if (isServiceFeePerHundred)
            {
                ServiceFee = ServiceFeeAmount / 100 * LoanAmount;
            }

            return ServiceFee;
        }

        /// <summary>
        /// This method is used to create default schedule for the Rigid method.
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="LoanAmount"></param>
        /// <param name="PeriodicInterestRate"></param>
        /// <param name="isRegularPaymentRounded"></param>
        /// <returns></returns>
        private static getScheduleOutput getStartingSchedule(getScheduleInput Input, double LoanAmount, double PeriodicInterestRate, bool isRegularPaymentRounded)
        {
            double paymentAmount = 0;
            bool isPaymentAmount = false;
            double ManagementFeeCarryOver = 0;
            double maintenencefeeCarryOver = 0;
            List<InputRecord> InputRecords = Input.InputRecords;
            List<PaymentDetail> PaymentDetailOut = new List<PaymentDetail>();
            double begnningPrincipal = rounndedValue(LoanAmount);
            double principalPayment = 0;
            double dailyInterestAmount = 0;
            double interestAccrued;
            double interestPayment = 0;
            double ServiceFee = rounndedValue(getStartingServiceFee(Input.ServiceFee, Input.LoanAmount, Input.ApplyServiceFeeInterest));
            double begnningServiceFee = ServiceFee;
            double serviceFee = 0;
            double accruedServiceFeeInterest = 0;
            double serviceFeeInterest = 0;
            double serviceFeeInterestCarryOver = 0;
            double serviceFeeInterestDue = 0;
            double InterestCarryOver = 0;
            double interestDue = 0;
            int DaysPerPeriod = getDaysPerPeriod(Input.PmtPeriod);
            // AD - 1.0.0.6 - add variables for service fee and interest count
            bool applyServiceFeeInterest = (Input.ApplyServiceFeeInterest & 1) == 1;
            int NPer = InputRecords.Count - 1;
            if (NPer == 0)
            {
                NPer = 1;
            }
            double ServicePeriodicInterestRate = applyServiceFeeInterest ? CalculatePeriodicInterestRateTier(Input, ServiceFee, DaysPerPeriod) : 0;
            // AD - 1.0.0.10 - use method to get regular payment, which takes rounding flag into account
            double ServicePayment = getRegularPaymentAmount(ServicePeriodicInterestRate, NPer, ServiceFee, isRegularPaymentRounded);
            double Payment = rounndedValue(getRegularPaymentAmount(PeriodicInterestRate, NPer, LoanAmount, isRegularPaymentRounded));
            //calculate interest tier for interest and service fee(user story 101)

            Payment = rounndedValue(CalculateRegularAmountWithTier(LoanAmount, PeriodicInterestRate, PeriodicInterestRate, Payment, NPer, InputRecords, Input, true, false, false));
            ServicePayment = rounndedValue(CalculateRegularAmountWithTier(ServiceFee, ServicePeriodicInterestRate, ServicePeriodicInterestRate, ServicePayment, NPer, InputRecords, Input, (applyServiceFeeInterest ? true : false), false, true));

            //====================================
            double RegularPayment = Payment;
            double RegularServiceFeePayment = ServicePayment;


            for (int i = 1; i < InputRecords.Count; i++)
            {
                PeriodicInterestRate = CalculatePeriodicInterestRateTier(Input, begnningPrincipal, DaysPerPeriod);
                //AD - 1.0.0.6 - count service fee payment amount
                accruedServiceFeeInterest = CalculateCurrentServiceFeeInterestTier(Input, begnningServiceFee, DaysPerPeriod);
                interestAccrued = CalculateCurrentInterestTier(Input, begnningPrincipal, DaysPerPeriod);
                dailyInterestAmount = (PeriodicInterestRate == 0 || DaysPerPeriod == 0) ? 0 : rounndedValue((PeriodicInterestRate / DaysPerPeriod) * begnningPrincipal);

                #region Calculate Earned value
                if (i == 1)
                {
                    interestAccrued += Input.EarnedInterest;
                    Payment = Payment + (Input.EarnedInterest / NPer);
                    accruedServiceFeeInterest += Input.EarnedServiceFeeInterest;
                    ServicePayment = ServicePayment + (Input.EarnedServiceFeeInterest / NPer);

                }

                #endregion
                interestDue = interestAccrued + InterestCarryOver;
                serviceFeeInterestDue = accruedServiceFeeInterest + serviceFeeInterestCarryOver;

                if ((i != InputRecords.Count - 1) && begnningPrincipal > 0)
                {
                    #region Calculate Interest payment and principal payment amount
                    if (interestDue > Payment)
                    {
                        interestPayment = Payment;
                        InterestCarryOver = interestDue - Payment;
                        principalPayment = 0;
                    }
                    else
                    {
                        interestPayment = interestAccrued + InterestCarryOver;
                        InterestCarryOver = 0;
                        principalPayment = rounndedValue((Payment - rounndedValue(interestPayment)));
                        principalPayment = principalPayment <= begnningPrincipal ? principalPayment : begnningPrincipal;
                    }
                    if (i >= Input.EnforcedPayment && principalPayment == 0 && interestPayment > 0)
                    {
                        principalPayment = interestPayment <= Input.EnforcedPrincipal ? interestPayment : rounndedValue(Input.EnforcedPrincipal);
                        interestPayment -= principalPayment;
                        InterestCarryOver += principalPayment;
                        principalPayment = principalPayment <= begnningPrincipal ? principalPayment : begnningPrincipal;
                    }
                    principalPayment = principalPayment <= 0 ? 0 : principalPayment;
                    #endregion
                    #region calculate service fee interest and service fee amount
                    if (Input.ServiceFeeFirstPayment)
                    {
                        serviceFee = rounndedValue(begnningServiceFee);
                        serviceFeeInterest = accruedServiceFeeInterest + serviceFeeInterestCarryOver;
                        serviceFeeInterestCarryOver = 0;
                    }
                    else
                    {
                        if (serviceFeeInterestDue > ServicePayment)
                        {
                            serviceFeeInterest = ServicePayment;
                            serviceFeeInterestCarryOver = serviceFeeInterestDue - ServicePayment;
                            serviceFee = 0;
                        }
                        else
                        {
                            serviceFeeInterest = accruedServiceFeeInterest + (i == 1 ? 0 : serviceFeeInterestCarryOver);
                            serviceFeeInterestCarryOver = 0;
                            serviceFee = rounndedValue((ServicePayment - rounndedValue(serviceFeeInterest)));
                            serviceFee = serviceFee <= 0 ? 0 : serviceFee;
                            serviceFee = serviceFee <= begnningServiceFee ? serviceFee : begnningServiceFee;
                        }

                    }
                    #endregion
                }
                else
                {
                    #region
                    interestPayment = interestDue;
                    principalPayment = rounndedValue(begnningPrincipal);
                    serviceFeeInterest = serviceFeeInterestDue;
                    serviceFee = rounndedValue(begnningServiceFee);
                    serviceFeeInterestCarryOver = 0;
                    InterestCarryOver = 0;
                    ServicePayment = rounndedValue(serviceFee + rounndedValue(serviceFeeInterest));
                    Payment = rounndedValue(begnningPrincipal + rounndedValue(interestPayment));
                    #endregion
                }
                if (begnningPrincipal > 0 || begnningServiceFee > 0 || interestPayment > 0 || serviceFeeInterest > 0)
                {
                    PaymentDetailOut.Add(new PaymentDetail
                    {
                        #region Add Schedule
                        PaymentDate = InputRecords[i].DateIn,
                        BeginningPrincipal = begnningPrincipal,
                        PrincipalPayment = principalPayment,
                        DailyInterestAmount = dailyInterestAmount,
                        DueDate = InputRecords[i].EffectiveDate,
                        InterestAccrued = interestAccrued,
                        InterestPayment = interestPayment,
                        InterestDue = interestDue,
                        InterestCarryOver = InterestCarryOver,
                        TotalPayment = Payment,
                        PeriodicInterestRate = PeriodicInterestRate,
                        PaymentID = InputRecords[i].PaymentID,
                        Flags = InputRecords[i].Flags,
                        //AD - 1.0.0.6 - put additional fees to output

                        BeginningServiceFee = begnningServiceFee,
                        AccruedServiceFeeInterest = accruedServiceFeeInterest,
                        ServiceFee = serviceFee,
                        ServiceFeeInterest = serviceFeeInterest,
                        serviceFeeInterestCarryOver = serviceFeeInterestCarryOver,
                        ServiceFeeTotal = ServicePayment,
                        OriginationFee = i == 1 ? Input.OriginationFee : 0.0,
                        SameDayFee = i == 1 ? Input.SameDayFee : 0.0,//AD - 1.0.0.24 - New property same day fee
                        MaintenanceFee = i == 1 ? 0.0 : Input.MaintenanceFee,
                        MaintenanceFeeCarryOver = 0,
                        ManagementFee = Input.ManagementFee,
                        ManagementFeeCarryOver = 0
                        #endregion
                    });

                    if ((!string.IsNullOrEmpty(Input.InputRecords[i].PaymentAmount) || Input.PaymentAmount > 0) && Input.MinDuration <= 0)
                    {
                        #region Allocate amount on the basis of priority when Actual amount is given
                        if (!string.IsNullOrEmpty(Input.InputRecords[i].PaymentAmount))
                        {
                            paymentAmount = rounndedValue(Convert.ToDouble(Input.InputRecords[i].PaymentAmount));
                            isPaymentAmount = false;
                        }
                        else
                        {
                            isPaymentAmount = true;
                            paymentAmount = rounndedValue(Input.PaymentAmount + (i == 1 ? Input.OriginationFee + Input.SameDayFee : 0));
                            ManagementFeeCarryOver += Input.ManagementFee;
                            maintenencefeeCarryOver += i == 1 ? 0 : Input.MaintenanceFee;
                        }
                        if (Input.OriginationFee > 0 && i == 1)
                        {
                            #region
                            if (Input.OriginationFee > paymentAmount)
                            {
                                PaymentDetailOut[PaymentDetailOut.Count - 1].OriginationFee = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailOut[PaymentDetailOut.Count - 1].OriginationFee));
                            }
                            #endregion
                        }
                        if (Input.SameDayFee > 0 && i == 1)
                        {
                            #region
                            if (Input.SameDayFee > paymentAmount)
                            {
                                PaymentDetailOut[PaymentDetailOut.Count - 1].SameDayFee = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailOut[PaymentDetailOut.Count - 1].SameDayFee));
                            }
                            #endregion
                        }
                        if (PaymentDetailOut[PaymentDetailOut.Count - 1].InterestPayment > 0)
                        {
                            #region
                            if (PaymentDetailOut[PaymentDetailOut.Count - 1].InterestPayment > paymentAmount)
                            {
                                InterestCarryOver = PaymentDetailOut[PaymentDetailOut.Count - 1].InterestDue - paymentAmount;
                                PaymentDetailOut[PaymentDetailOut.Count - 1].InterestCarryOver = InterestCarryOver;
                                PaymentDetailOut[PaymentDetailOut.Count - 1].InterestPayment = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailOut[PaymentDetailOut.Count - 1].InterestPayment));
                            }
                            #endregion
                        }
                        if (PaymentDetailOut[PaymentDetailOut.Count - 1].ServiceFeeInterest > 0)
                        {
                            #region
                            if (PaymentDetailOut[PaymentDetailOut.Count - 1].ServiceFeeInterest > paymentAmount)
                            {
                                serviceFeeInterestCarryOver += PaymentDetailOut[PaymentDetailOut.Count - 1].ServiceFeeInterest - paymentAmount;
                                PaymentDetailOut[PaymentDetailOut.Count - 1].serviceFeeInterestCarryOver = serviceFeeInterestCarryOver;
                                PaymentDetailOut[PaymentDetailOut.Count - 1].ServiceFeeInterest = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailOut[PaymentDetailOut.Count - 1].ServiceFeeInterest));
                            }
                            #endregion
                        }
                        if (PaymentDetailOut[PaymentDetailOut.Count - 1].PrincipalPayment > 0)
                        {
                            #region
                            if (PaymentDetailOut[PaymentDetailOut.Count - 1].PrincipalPayment > paymentAmount)
                            {
                                PaymentDetailOut[PaymentDetailOut.Count - 1].PrincipalPayment = paymentAmount;
                                principalPayment = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailOut[PaymentDetailOut.Count - 1].PrincipalPayment));
                            }
                            #endregion
                        }
                        if (PaymentDetailOut[PaymentDetailOut.Count - 1].ServiceFee > 0)
                        {
                            #region
                            if (PaymentDetailOut[PaymentDetailOut.Count - 1].ServiceFee > paymentAmount)
                            {
                                PaymentDetailOut[PaymentDetailOut.Count - 1].ServiceFee = paymentAmount;
                                serviceFee = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailOut[PaymentDetailOut.Count - 1].ServiceFee));
                            }
                            #endregion
                        }
                        if (i != (Input.InputRecords.Count - 1))
                        {
                            if (isPaymentAmount)
                            {
                                #region calculate management and maintenance fee amount
                                if (ManagementFeeCarryOver > 0)
                                {
                                    #region
                                    if (ManagementFeeCarryOver > paymentAmount)
                                    {
                                        PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFee = paymentAmount;
                                        ManagementFeeCarryOver -= paymentAmount;
                                        PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFeeCarryOver = ManagementFeeCarryOver;
                                        paymentAmount = 0;
                                    }
                                    else
                                    {
                                        PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFee = ManagementFeeCarryOver;
                                        paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFee));
                                        PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFeeCarryOver = 0;
                                        ManagementFeeCarryOver = 0;
                                    }
                                    #endregion
                                }
                                if (maintenencefeeCarryOver > 0)
                                {
                                    #region
                                    if (maintenencefeeCarryOver > paymentAmount)
                                    {
                                        PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFee = paymentAmount;
                                        maintenencefeeCarryOver -= paymentAmount;
                                        PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFeeCarryOver = maintenencefeeCarryOver;
                                        paymentAmount = 0;
                                    }
                                    else
                                    {
                                        PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFee = maintenencefeeCarryOver;
                                        paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFee));
                                        PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFeeCarryOver = 0;
                                        maintenencefeeCarryOver = 0;
                                    }
                                    #endregion
                                }
                                #endregion
                            }
                            else
                            {
                                #region calculate management and maintenance fee amount
                                if (PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFee > 0)
                                {
                                    #region
                                    if (PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFee > paymentAmount)
                                    {
                                        PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFee = paymentAmount;
                                        PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFeeCarryOver = ManagementFeeCarryOver;
                                        paymentAmount = 0;
                                    }
                                    else
                                    {
                                        //ManagementFeeCarryOver += payments[payments.Count - 1].ManagementFee;
                                        //payments[payments.Count - 1].ManagementFee = (ManagementFeeCarryOver > paymentAmount) ? paymentAmount : (paymentAmount - ManagementFeeCarryOver);
                                        //ManagementFeeCarryOver = ManagementFeeCarryOver - payments[payments.Count - 1].ManagementFee;
                                        PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFeeCarryOver = ManagementFeeCarryOver;
                                        paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFee));
                                    }
                                    #endregion
                                }
                                if (PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFee > 0)
                                {
                                    #region
                                    if (PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFee > paymentAmount)
                                    {
                                        PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFee = paymentAmount;
                                        PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFeeCarryOver = maintenencefeeCarryOver;
                                        paymentAmount = 0;
                                    }
                                    else
                                    {

                                        //maintenencefeeCarryOver += payments[payments.Count - 1].MaintenanceFee;
                                        //payments[payments.Count - 1].MaintenanceFee = (maintenencefeeCarryOver > paymentAmount) ?  paymentAmount : (paymentAmount - maintenencefeeCarryOver);
                                        //maintenencefeeCarryOver = maintenencefeeCarryOver - payments[payments.Count - 1].MaintenanceFee;
                                        PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFeeCarryOver = maintenencefeeCarryOver;
                                        paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFee));
                                    }
                                    #endregion
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            #region calculate management and maintenance fee amount
                            PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFee = ManagementFeeCarryOver;
                            PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFeeCarryOver = 0;
                            ManagementFeeCarryOver = 0;
                            PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFee = maintenencefeeCarryOver;
                            PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFeeCarryOver = 0;
                            maintenencefeeCarryOver = 0;
                            #endregion
                        }

                        #region
                        //if (isPaymentAmount)
                        //{
                        //    #region
                        //    if (i != (Input.InputRecords.Count - 1))
                        //    {
                        //        #region calculate management and maintenance fee amount
                        //        if (ManagementFeeCarryOver > 0)
                        //        {
                        //            #region
                        //            if (ManagementFeeCarryOver > paymentAmount)
                        //            {
                        //                PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFee = paymentAmount;
                        //                ManagementFeeCarryOver -= paymentAmount;
                        //                paymentAmount = 0;
                        //            }
                        //            else
                        //            {
                        //                PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFee = ManagementFeeCarryOver;
                        //                paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFee));
                        //                ManagementFeeCarryOver = 0;
                        //            }
                        //            #endregion
                        //        }
                        //        if (maintenencefeeCarryOver > 0)
                        //        {
                        //            #region
                        //            if (maintenencefeeCarryOver > paymentAmount)
                        //            {
                        //                PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFee = paymentAmount;
                        //                maintenencefeeCarryOver -= paymentAmount;
                        //                paymentAmount = 0;
                        //            }
                        //            else
                        //            {
                        //                PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFee = maintenencefeeCarryOver;
                        //                paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFee));
                        //                maintenencefeeCarryOver = 0;
                        //            }
                        //            #endregion
                        //        }
                        //        #endregion
                        //    }
                        //    else
                        //    {
                        //        PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFee = ManagementFeeCarryOver;
                        //        ManagementFeeCarryOver = 0;
                        //        PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFee = maintenencefeeCarryOver;
                        //        maintenencefeeCarryOver = 0;
                        //    }
                        //    #endregion
                        //}
                        //else
                        //{
                        //    #region calculate management and maintenance fee amount
                        //    if (PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFee > 0)
                        //    {
                        //        #region
                        //        if (PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFee > paymentAmount)
                        //        {
                        //            PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFee = paymentAmount;
                        //            paymentAmount = 0;
                        //        }
                        //        else
                        //        {
                        //            paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFee));
                        //        }
                        //        #endregion
                        //    }
                        //    if (PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFee > 0)
                        //    {
                        //        #region
                        //        if (PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFee > paymentAmount)
                        //        {
                        //            PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFee = paymentAmount;
                        //            paymentAmount = 0;
                        //        }
                        //        else
                        //        {
                        //            paymentAmount = rounndedValue(paymentAmount - rounndedValue(PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFee));
                        //        }
                        //        #endregion
                        //    }
                        //    #endregion
                        //}
                        #endregion
                        if (paymentAmount > 0)//When payment amount remains after full payment of schedule
                        {
                            #region
                            double remainingPincipal = begnningPrincipal - PaymentDetailOut[PaymentDetailOut.Count - 1].PrincipalPayment;
                            remainingPincipal = remainingPincipal <= 0 ? 0 : remainingPincipal;
                            double remainingServiceFee = begnningServiceFee - PaymentDetailOut[PaymentDetailOut.Count - 1].ServiceFee;
                            remainingServiceFee = remainingServiceFee <= 0 ? 0 : remainingServiceFee;
                            #region
                            if (InterestCarryOver > paymentAmount)
                            {
                                PaymentDetailOut[PaymentDetailOut.Count - 1].InterestPayment += paymentAmount;
                                PaymentDetailOut[PaymentDetailOut.Count - 1].InterestCarryOver -= paymentAmount;
                                InterestCarryOver -= paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailOut[PaymentDetailOut.Count - 1].InterestPayment += InterestCarryOver;
                                paymentAmount = rounndedValue(paymentAmount - InterestCarryOver);
                                PaymentDetailOut[PaymentDetailOut.Count - 1].InterestCarryOver = 0;
                                InterestCarryOver = 0;
                            }
                            #endregion

                            #region
                            if (serviceFeeInterestCarryOver > paymentAmount)
                            {
                                PaymentDetailOut[PaymentDetailOut.Count - 1].ServiceFeeInterest += paymentAmount;
                                PaymentDetailOut[PaymentDetailOut.Count - 1].serviceFeeInterestCarryOver -= paymentAmount;
                                serviceFeeInterestCarryOver -= paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailOut[PaymentDetailOut.Count - 1].ServiceFeeInterest += serviceFeeInterestCarryOver;
                                paymentAmount = rounndedValue(paymentAmount - serviceFeeInterestCarryOver);
                                PaymentDetailOut[PaymentDetailOut.Count - 1].serviceFeeInterestCarryOver = 0;
                                serviceFeeInterestCarryOver = 0;
                            }
                            #endregion

                            #region
                            if (remainingPincipal > paymentAmount)
                            {
                                PaymentDetailOut[PaymentDetailOut.Count - 1].PrincipalPayment += paymentAmount;
                                principalPayment += paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailOut[PaymentDetailOut.Count - 1].PrincipalPayment += remainingPincipal;
                                principalPayment += remainingPincipal;
                                paymentAmount = rounndedValue(paymentAmount - remainingPincipal);
                            }
                            #endregion

                            #region
                            if (remainingServiceFee > paymentAmount)
                            {
                                PaymentDetailOut[PaymentDetailOut.Count - 1].ServiceFee += paymentAmount;
                                serviceFee += paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailOut[PaymentDetailOut.Count - 1].ServiceFee += remainingServiceFee;
                                serviceFee += remainingServiceFee;
                                paymentAmount = rounndedValue(paymentAmount - remainingServiceFee);
                            }
                            #endregion

                            #region
                            if (PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFeeCarryOver > paymentAmount)
                            {
                                PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFee += paymentAmount;
                                ManagementFeeCarryOver -= paymentAmount;
                                PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFeeCarryOver -= paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFee += PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFeeCarryOver;
                                PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFeeCarryOver = 0;
                                ManagementFeeCarryOver = 0;
                                paymentAmount = rounndedValue(paymentAmount - PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFeeCarryOver);
                            }
                            #endregion

                            #region
                            if (PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFeeCarryOver > paymentAmount)
                            {
                                PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFee += paymentAmount;
                                maintenencefeeCarryOver -= paymentAmount;
                                PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFeeCarryOver -= paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFee += PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFeeCarryOver;
                                PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFeeCarryOver = 0;
                                maintenencefeeCarryOver = 0;
                                paymentAmount = rounndedValue(paymentAmount - PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFeeCarryOver);
                            }
                            #endregion
                            #endregion
                        }

                        #endregion
                    }
                    #region Cumulative amount
                    PaymentDetailOut[PaymentDetailOut.Count - 1].CumulativeInterest = (PaymentDetailOut.Count) == 1 ? PaymentDetailOut[PaymentDetailOut.Count - 1].InterestPayment : PaymentDetailOut[PaymentDetailOut.Count - 2].CumulativeInterest + PaymentDetailOut[PaymentDetailOut.Count - 1].InterestPayment;
                    PaymentDetailOut[PaymentDetailOut.Count - 1].CumulativePrincipal = (PaymentDetailOut.Count) == 1 ? PaymentDetailOut[PaymentDetailOut.Count - 1].PrincipalPayment : PaymentDetailOut[PaymentDetailOut.Count - 2].CumulativePrincipal + PaymentDetailOut[PaymentDetailOut.Count - 1].PrincipalPayment;
                    PaymentDetailOut[PaymentDetailOut.Count - 1].CumulativePayment = (PaymentDetailOut.Count) == 1 ? PaymentDetailOut[PaymentDetailOut.Count - 1].TotalPayment : PaymentDetailOut[PaymentDetailOut.Count - 2].CumulativePayment + PaymentDetailOut[PaymentDetailOut.Count - 1].TotalPayment;
                    PaymentDetailOut[PaymentDetailOut.Count - 1].CumulativeMaintenanceFee = (PaymentDetailOut.Count) == 1 ? PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFee : PaymentDetailOut[PaymentDetailOut.Count - 2].CumulativeMaintenanceFee + PaymentDetailOut[PaymentDetailOut.Count - 1].MaintenanceFee;
                    PaymentDetailOut[PaymentDetailOut.Count - 1].CumulativeManagementFee = (PaymentDetailOut.Count) == 1 ? PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFee : PaymentDetailOut[PaymentDetailOut.Count - 2].CumulativeManagementFee + PaymentDetailOut[PaymentDetailOut.Count - 1].ManagementFee;
                    PaymentDetailOut[PaymentDetailOut.Count - 1].CumulativeOriginationFee = (PaymentDetailOut.Count) == 1 ? PaymentDetailOut[PaymentDetailOut.Count - 1].OriginationFee : PaymentDetailOut[PaymentDetailOut.Count - 2].CumulativeOriginationFee + PaymentDetailOut[PaymentDetailOut.Count - 1].OriginationFee;
                    PaymentDetailOut[PaymentDetailOut.Count - 1].CumulativeSameDayFee = (PaymentDetailOut.Count) == 1 ? PaymentDetailOut[PaymentDetailOut.Count - 1].SameDayFee : PaymentDetailOut[PaymentDetailOut.Count - 2].CumulativeSameDayFee + PaymentDetailOut[PaymentDetailOut.Count - 1].SameDayFee;
                    PaymentDetailOut[PaymentDetailOut.Count - 1].CumulativeServiceFee = (PaymentDetailOut.Count) == 1 ? PaymentDetailOut[PaymentDetailOut.Count - 1].ServiceFee : PaymentDetailOut[PaymentDetailOut.Count - 2].CumulativeServiceFee + PaymentDetailOut[PaymentDetailOut.Count - 1].ServiceFee;
                    PaymentDetailOut[PaymentDetailOut.Count - 1].CumulativeServiceFeeInterest = (PaymentDetailOut.Count) == 1 ? PaymentDetailOut[PaymentDetailOut.Count - 1].ServiceFeeInterest : PaymentDetailOut[PaymentDetailOut.Count - 2].CumulativeServiceFeeInterest + PaymentDetailOut[PaymentDetailOut.Count - 1].ServiceFeeInterest;

                    PaymentDetailOut[PaymentDetailOut.Count - 1].CumulativeTotalFees = rounndedValue((PaymentDetailOut[PaymentDetailOut.Count - 1].CumulativeMaintenanceFee +
                                                                                                   PaymentDetailOut[PaymentDetailOut.Count - 1].CumulativeManagementFee +
                                                                                                   PaymentDetailOut[PaymentDetailOut.Count - 1].CumulativeOriginationFee +
                                                                                                   PaymentDetailOut[PaymentDetailOut.Count - 1].CumulativeSameDayFee +
                                                                                                   PaymentDetailOut[PaymentDetailOut.Count - 1].CumulativeServiceFee +
                                                                                                   PaymentDetailOut[PaymentDetailOut.Count - 1].CumulativeServiceFeeInterest));
                    #endregion
                }
                begnningPrincipal = rounndedValue((begnningPrincipal - principalPayment));
                //AD - 1.0.0.6 - count principal payment for service fee
                begnningServiceFee = rounndedValue((begnningServiceFee - serviceFee));

            }
            return new getScheduleOutput { Schedule = PaymentDetailOut, RegularPayment = rounndedValue(RegularPayment), RegularServiceFeePayment = rounndedValue(RegularServiceFeePayment), AccruedInterest = 0 };
        }

        /// <summary>
        /// This method is used to calculate schedule difference in two schedule dates on the basis of Loan type selection and payment period, days in year and days in month.
        /// </summary>
        /// <param name="PaymentDetailIn"></param>
        /// <param name="endDate"></param>
        /// <param name="PaymentDetailIndex"></param>
        /// <param name="Input"></param>
        /// <param name="isMinDuration"></param>
        /// <param name="DaysPeriod"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static int CalculatScheduleDifference(List<PaymentDetail> PaymentDetailIn, DateTime endDate, int PaymentDetailIndex, getScheduleInput Input, bool isMinDuration, int DaysPeriod, int flag)
        {
            DateTime startDateWithDelay = (Input.MinDuration > 0 || Input.UseFlexibleCalculation) ? Input.InputRecords[0].DateIn.AddDays(Input.InterestDelay) : Input.InputRecords[0].DateIn;
            DateTime startDate;
            startDate = startDateWithDelay >= (PaymentDetailIndex == 0 ? Input.InputRecords[0].DateIn : PaymentDetailIn[PaymentDetailIndex - 1].PaymentDate) ? startDateWithDelay : (PaymentDetailIndex == 0 ? Input.InputRecords[0].DateIn : PaymentDetailIn[PaymentDetailIndex - 1].PaymentDate);
            int scheduleDayDifference = (startDate <= endDate) ? (endDate - startDate).Days : 0;
            if (!Input.UseFlexibleCalculation && (!isMinDuration || (isMinDuration && PaymentDetailIn.FindLastIndex(o => o.PaymentDate == Input.InputRecords[1].DateIn && (o.Flags == 0 || o.Flags == 1)) < PaymentDetailIndex)))
            {
                #region
                if (PaymentDetailIndex != 0)
                {
                    #region
                    if (PaymentDetailIn[PaymentDetailIndex - 1].Flags == 0 || PaymentDetailIn[PaymentDetailIndex - 1].Flags == 1)
                    {
                        scheduleDayDifference = (endDate - PaymentDetailIn[PaymentDetailIndex - 1].PaymentDate).Days;

                    }
                    else
                    {
                        int index = Input.InputRecords.FindLastIndex(o => o.DateIn <= PaymentDetailIn[PaymentDetailIndex - 1].PaymentDate && (o.Flags == 0 || o.Flags == 1));
                        int Difference = (PaymentDetailIn[PaymentDetailIndex - 1].PaymentDate - Input.InputRecords[index].DateIn).Days;
                        DaysPeriod = DaysPeriod > Difference ? DaysPeriod - Difference : 0;
                    }
                    if ((flag == 0 || flag == 1) || ((flag == -1) && (Input.InputRecords.FindIndex(o => o.DateIn == endDate && (o.Flags == 0 || o.Flags == 1)) != -1)))
                    {
                        scheduleDayDifference = DaysPeriod;
                    }
                    #endregion
                }

                scheduleDayDifference = (PaymentDetailIndex == 0 && (flag == 0 || flag == 1)) ? (DaysPeriod) : (scheduleDayDifference <= DaysPeriod ? scheduleDayDifference : DaysPeriod);
                #endregion
            }
            return scheduleDayDifference;
        }

        /// <summary>
        /// This method is used to calculate periodic interest rate on the basis of different annual interest rate and different Tiers
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="CurrentPrincipal"></param>
        /// <param name="DaysPerPeriod"></param>
        /// <returns></returns>
        public static double CalculatePeriodicInterestRateTier(getScheduleInput Input, double CurrentPrincipal, int DaysPerPeriod)
        {
            double PeriodicInterestRate = 0;

            PeriodicInterestRate = Convert.ToDouble(Input.InterestRate) * 0.01 / Input.DaysInYear * DaysPerPeriod;

            if (!(Input.InterestRate2 == 0) && Input.Tier1 != 0 && CurrentPrincipal > Input.Tier1)
            {
                PeriodicInterestRate += Convert.ToDouble(Input.InterestRate2) * 0.01 / Input.DaysInYear * DaysPerPeriod;
            }

            if (!(Input.InterestRate3 == 0) && Input.Tier1 != 0 && Input.Tier2 != 0 && CurrentPrincipal > Input.Tier2)
            {
                PeriodicInterestRate += Convert.ToDouble(Input.InterestRate3) * 0.01 / Input.DaysInYear * DaysPerPeriod;
            }

            if (!(Input.InterestRate4 == 0) && Input.Tier1 != 0 && Input.Tier2 != 0 && Input.Tier3 != 0 && CurrentPrincipal > Input.Tier3)
            {
                PeriodicInterestRate += Convert.ToDouble(Input.InterestRate4) * 0.01 / Input.DaysInYear * DaysPerPeriod;
            }

            PeriodicInterestRate = Math.Round(PeriodicInterestRate, 15, MidpointRounding.AwayFromZero);
            return PeriodicInterestRate;
        }

        /// <summary>
        /// This method is used to calculate Accrued interest on the basis of different annual interest rate and different Tiers
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="CurrentPrincipal"></param>
        /// <param name="DaysPerPeriod"></param>
        /// <returns></returns>
        public static double CalculateCurrentInterestTier(getScheduleInput Input, double CurrentPrincipal, int DaysPerPeriod)
        {
            double CurrentInterestPayment;
            double principalVal = Input.Tier1 != 0 ? (Input.Tier1 > CurrentPrincipal ? CurrentPrincipal : Input.Tier1) : CurrentPrincipal;
            double PeriodicInterestRate = Input.InterestRate * 0.01 / Input.DaysInYear * DaysPerPeriod;
            CurrentInterestPayment = principalVal * PeriodicInterestRate;

            //Calculate the interest for second tier
            if (!(Input.InterestRate2 == 0) && Input.Tier1 != 0 && CurrentPrincipal > Input.Tier1)
            {
                principalVal = (Input.Tier2 != 0 ? (Input.Tier2 > CurrentPrincipal ? CurrentPrincipal : Input.Tier2) : CurrentPrincipal) -
                                                                Input.Tier1;
                CurrentInterestPayment += (Convert.ToDouble(Input.InterestRate2) * 0.01 / Input.DaysInYear * DaysPerPeriod) * principalVal;
            }

            //Calculate the interest for third tier
            if (!(Input.InterestRate3 == 0) && Input.Tier1 != 0 && Input.Tier2 != 0 && CurrentPrincipal > Input.Tier2)
            {
                principalVal = (Input.Tier3 != 0 ? (Input.Tier3 > CurrentPrincipal ? CurrentPrincipal : Input.Tier3) : CurrentPrincipal) -
                                                                Input.Tier2;
                CurrentInterestPayment += (Convert.ToDouble(Input.InterestRate3) * 0.01 / Input.DaysInYear * DaysPerPeriod) * principalVal;
            }

            //Calculate the interest for fourth tier
            if (!(Input.InterestRate4 == 0) && Input.Tier1 != 0 && Input.Tier2 != 0 && Input.Tier3 != 0 &&
                                                CurrentPrincipal > Input.Tier3)
            {
                principalVal = (Input.Tier4 != 0 ? (Input.Tier4 > CurrentPrincipal ? CurrentPrincipal : Input.Tier4) : CurrentPrincipal) -
                                                                Input.Tier3;
                CurrentInterestPayment += (Convert.ToDouble(Input.InterestRate4) * 0.01 / Input.DaysInYear * DaysPerPeriod) * principalVal;
            }
            return CurrentInterestPayment;
        }

        /// <summary>
        /// This method is used to calculate service fee interest on the basis of different annual interest rate and different Tiers
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="CurrentPrincipal"></param>
        /// <param name="DaysPerPeriod"></param>
        /// <returns></returns>
        public static double CalculateCurrentServiceFeeInterestTier(getScheduleInput Input, double CurrentPrincipal, int DaysPerPeriod)
        {
            bool applyServiceFeeInterest = (Input.ApplyServiceFeeInterest & 1) == 1;
            double serviceInterestRate = applyServiceFeeInterest ? Input.InterestRate : 0;
            double CurrentInterestPayment;
            double principalVal = Input.Tier1 != 0 ? (Input.Tier1 > CurrentPrincipal ? CurrentPrincipal : Input.Tier1) : CurrentPrincipal;
            double PeriodicInterestRate = Convert.ToDouble(serviceInterestRate) * 0.01 / Input.DaysInYear * DaysPerPeriod;
            CurrentInterestPayment = principalVal * PeriodicInterestRate;

            //Calculate the interest for second tier
            if (!(Input.InterestRate2 == 0) && Input.Tier1 != 0 && CurrentPrincipal > Input.Tier1)
            {
                principalVal = (Input.Tier2 != 0 ? (Input.Tier2 > CurrentPrincipal ? CurrentPrincipal : Input.Tier2) : CurrentPrincipal) -
                                                                Input.Tier1;
                serviceInterestRate = applyServiceFeeInterest ? Input.InterestRate2 : 0;
                CurrentInterestPayment += (Convert.ToDouble(serviceInterestRate) * 0.01 / Input.DaysInYear * DaysPerPeriod) * principalVal;
            }

            //Calculate the interest for third tier
            if (!(Input.InterestRate3 == 0) && Input.Tier1 != 0 && Input.Tier2 != 0 && CurrentPrincipal > Input.Tier2)
            {
                principalVal = (Input.Tier3 != 0 ? (Input.Tier3 > CurrentPrincipal ? CurrentPrincipal : Input.Tier3) : CurrentPrincipal) -
                                                                Input.Tier2;
                serviceInterestRate = applyServiceFeeInterest ? Input.InterestRate3 : 0;
                CurrentInterestPayment += (Convert.ToDouble(serviceInterestRate) * 0.01 / Input.DaysInYear * DaysPerPeriod) * principalVal;
            }

            //Calculate the interest for fourth tier
            if (!(Input.InterestRate4 == 0) && Input.Tier1 != 0 && Input.Tier2 != 0 && Input.Tier3 != 0 &&
                                                CurrentPrincipal > Input.Tier3)
            {
                principalVal = (Input.Tier4 != 0 ? (Input.Tier4 > CurrentPrincipal ? CurrentPrincipal : Input.Tier4) : CurrentPrincipal) -
                                                                Input.Tier3;
                serviceInterestRate = applyServiceFeeInterest ? Input.InterestRate4 : 0;
                CurrentInterestPayment += (Convert.ToDouble(serviceInterestRate) * 0.01 / Input.DaysInYear * DaysPerPeriod) * principalVal;
            }
            return CurrentInterestPayment;
        }

        /// <summary>
        /// This method is used to calculate regular amount when we provide different annual interest rate with different Tiers.
        /// </summary>
        /// <param name="totalPrincipal"></param>
        /// <param name="interestRate"></param>
        /// <param name="firstPeriodInterestRate"></param>
        /// <param name="regularPayment"></param>
        /// <param name="nPer"></param>
        /// <param name="inputRecords"></param>
        /// <param name="input"></param>
        /// <param name="serviceInterest"></param>
        /// <param name="MinDuration"></param>
        /// <returns></returns>
        private static double CalculateRegularAmountWithTier(double totalPrincipal, double interestRate, double firstPeriodInterestRate, double regularPayment, int nPer, List<InputRecord> inputRecords, getScheduleInput input, bool serviceInterest, bool MinDuration, bool calculateServiceFeeTotal)
        {
            nPer = (input.BalloonPayment > 0 && !calculateServiceFeeTotal) ? nPer - 1 : nPer;


            var payments = new List<PaymentDetail>();
            double cumulativePayment = 0;
            double cumulativeInterest = 0;
            double cumulativePrincipal = 0;
            double currentPrincipal = totalPrincipal;

            double firstRegularInterestPayment = totalPrincipal * interestRate;
            double firstInterestPaymentPaid = totalPrincipal * firstPeriodInterestRate;
            double interestPaymentDifference = firstInterestPaymentPaid - firstRegularInterestPayment;
            double paymentOne = regularPayment;

            double interestCarryOver = 0;
            double interestPaymentDue = 0;
            double paymentLast = 0;
            double maxAllowedDiff = 0.15; //Maximum allowed difference between annuity payment and last period payment
            double currentDiff = Math.Abs(interestPaymentDifference); //Current difference between payments
            double interestDiff = 0;
            double multiplicator = 0.5; //AG: 1.0.0.16 - Multiplacator to make loop step everytime different in order to avoid infinite loops
            double currentInterestPayment = 0;
            double periodicIntrestrate = 0;
            int scheduleDayDifference = 0;
            if (input.InterestRate == 0 && input.InterestRate2 == 0 && input.InterestRate3 == 0 && input.InterestRate4 == 0)
            {
                paymentOne = input.BalloonPayment > 0 ? ((totalPrincipal - input.BalloonPayment) / nPer) : totalPrincipal / nPer;
                paymentOne = paymentOne <= 0 ? 0 : paymentOne;

            }
            else
            {
                //Iterate while difference reaches allowed value:
                while (currentDiff > maxAllowedDiff || !payments.Any())
                {
                    #region
                    for (int i = (MinDuration ? 2 : 1); i < ((input.BalloonPayment > 0 && !calculateServiceFeeTotal) ? input.InputRecords.Count() - 1 : inputRecords.Count()); i++)
                    {
                        interestPaymentDue = 0;
                        if (serviceInterest)
                        {
                            scheduleDayDifference = getDaysPerPeriod(input.PmtPeriod);
                            double principalVal = input.Tier1 != 0 ? (input.Tier1 > currentPrincipal ? currentPrincipal : input.Tier1) : currentPrincipal;
                            //Calculate the interest for first tier
                            periodicIntrestrate = Convert.ToDouble(input.InterestRate) * 0.01 / input.DaysInYear * scheduleDayDifference;
                            currentInterestPayment = principalVal * periodicIntrestrate;

                            //Calculate the interest for second tier
                            if (!(input.InterestRate2 == 0) && input.Tier1 != 0 && currentPrincipal > input.Tier1)
                            {
                                principalVal = (input.Tier2 != 0 ? (input.Tier2 > currentPrincipal ? currentPrincipal : input.Tier2) : currentPrincipal) -
                                                                                input.Tier1;
                                currentInterestPayment += (Convert.ToDouble(input.InterestRate2) * 0.01 / input.DaysInYear * scheduleDayDifference) * principalVal;
                            }

                            //Calculate the interest for third tier
                            if (!(input.InterestRate3 == 0) && input.Tier1 != 0 && input.Tier2 != 0 && currentPrincipal > input.Tier2)
                            {
                                principalVal = (input.Tier3 != 0 ? (input.Tier3 > currentPrincipal ? currentPrincipal : input.Tier3) : currentPrincipal) -
                                                                                input.Tier2;
                                currentInterestPayment += (Convert.ToDouble(input.InterestRate3) * 0.01 / input.DaysInYear * scheduleDayDifference) * principalVal;
                            }

                            //Calculate the interest for fourth tier
                            if (!(input.InterestRate4 == 0) && input.Tier1 != 0 && input.Tier2 != 0 && input.Tier3 != 0 &&
                                                                currentPrincipal > input.Tier3)
                            {
                                principalVal = (input.Tier4 != 0 ? (input.Tier4 > currentPrincipal ? currentPrincipal : input.Tier4) : currentPrincipal) -
                                                                                input.Tier3;
                                currentInterestPayment += (Convert.ToDouble(input.InterestRate4) * 0.01 / input.DaysInYear * scheduleDayDifference) * principalVal;
                            }
                        }
                        else
                        {
                            currentInterestPayment = 0;
                        }

                        double currentPrincipalPayment = paymentOne - currentInterestPayment;

                        if (interestDiff > 0)
                        {
                            if (interestDiff > currentPrincipalPayment)
                            {
                                interestPaymentDue = currentInterestPayment + interestDiff;
                                interestDiff -= currentPrincipalPayment;
                                //intersetCarryOver carry the remeaining amount of the period
                                interestCarryOver = interestDiff;
                                currentPrincipalPayment = 0;
                            }
                            else
                            {
                                //intersetCarryOver carry the remeaining amount of the period
                                interestPaymentDue = currentInterestPayment + interestCarryOver;
                                currentPrincipalPayment = paymentOne - interestPaymentDue;
                                interestCarryOver = 0;
                            }
                        }

                        if (currentPrincipalPayment < 0)
                        {
                            interestDiff = Math.Abs(currentPrincipalPayment);
                            //intersetCarryOver carry the remeaining amount of the period
                            if (i == 1)
                            {
                                interestPaymentDue = currentInterestPayment;
                                interestCarryOver = interestPaymentDue - paymentOne;
                            }
                            else
                            {
                                interestPaymentDue = currentInterestPayment + interestCarryOver;
                                interestCarryOver = interestPaymentDue - paymentOne;
                            }
                            currentPrincipalPayment = 0;
                        }
                        if (interestCarryOver == 0)
                        {
                            interestPaymentDue = currentInterestPayment + interestDiff;
                            interestDiff = 0;
                        }
                        cumulativePayment += paymentOne;
                        cumulativeInterest += currentInterestPayment;
                        cumulativePrincipal += currentPrincipalPayment;
                        //Check difference on last payment period:
                        if (i == nPer)
                        {
                            paymentLast = currentPrincipal + currentInterestPayment + interestDiff - (calculateServiceFeeTotal ? 0 : input.BalloonPayment);
                            if (paymentLast < 0) paymentLast = 0;
                            currentDiff = Math.Abs(paymentOne - paymentLast);
                            if (currentDiff > maxAllowedDiff)
                            {
                                multiplicator += 0.5;
                                double change = Math.Abs(currentDiff) / (paymentOne + multiplicator);
                                if (change > currentDiff) change = currentDiff / nPer;
                                paymentOne = paymentOne > paymentLast ? paymentOne - change : paymentOne + change;

                                //Default all values and start again:
                                cumulativePayment = default(double);
                                cumulativeInterest = default(double);
                                cumulativePrincipal = default(double);
                                currentPrincipal = totalPrincipal;
                                interestDiff = 0;
                                payments.Clear();
                                continue;
                            }
                        }

                        payments.Add(new PaymentDetail
                        {
                            PaymentDate = inputRecords[i].DateIn,
                            BeginningPrincipal = currentPrincipal,
                            PrincipalPayment = currentPrincipalPayment > 0 ? rounndedValue(paymentOne) - rounndedValue(interestPaymentDue) : currentPrincipalPayment,
                            InterestAccrued = currentInterestPayment,
                            InterestCarryOver = interestCarryOver,
                            InterestDue = rounndedValue(interestPaymentDue),
                            TotalPayment = i == nPer ? paymentLast : paymentOne,
                            CumulativePayment = rounndedValue(cumulativePayment),
                            CumulativeInterest = rounndedValue(cumulativeInterest),
                            CumulativePrincipal = rounndedValue(cumulativePrincipal),
                            PeriodicInterestRate = periodicIntrestrate,
                            PaymentID = inputRecords[i].PaymentID,
                            Flags = inputRecords[i].Flags,
                        });
                        currentPrincipal -= currentPrincipalPayment;
                    }
                    #endregion
                }

            }
            return paymentOne;
        }

        /// <summary>
        /// AG - 1.0.0.11 - Method to support Flexible calculation of Schedule
        /// </summary>
        private static getScheduleOutput getStartingSchedule(getScheduleInput input, double interestRate, bool isRegularPaymentRounded)
        {
            int nPer = input.InputRecords.Count - 1;
            if (nPer == 0)
            {
                nPer = 1; //1 is minimum value
            }
            int DaysPerPeriod = getDaysPerPeriod(input.PmtPeriod);
            double serviceFee = getStartingServiceFee(input.ServiceFee, input.LoanAmount, input.ApplyServiceFeeInterest);
            bool applyServiceFeeInterest = (input.ApplyServiceFeeInterest & 1) == 1;
            double servicePeriodicInterestRate = applyServiceFeeInterest ? CalculatePeriodicInterestRateTier(input, serviceFee, DaysPerPeriod) : 0;
            double servicePayment = getRegularPaymentAmount(servicePeriodicInterestRate, nPer, serviceFee, isRegularPaymentRounded);
            double annuityRegularPayment = getRegularPaymentAmount(interestRate, nPer, input.LoanAmount, isRegularPaymentRounded);
            int firstPeriod = CalculateFirstPaymentPeriod(input); // AG - 1.0.0.14 - New logic for calculating payment period

            return CalculateAnnuityWithUnRegularFirstPeriod(input, interestRate, annuityRegularPayment, firstPeriod, serviceFee, servicePayment);
        }

        /// <summary>
        /// AG - 1.0.0.11 - Method to support Flexible calculation for Schedule with UnRegular date in first payment period
        /// </summary>
        private static getScheduleOutput CalculateAnnuityWithUnRegularFirstPeriod(getScheduleInput input, double interestRate, double annuityRegularPayment, int firstPeriod, double serviceFee, double servicePayment)
        {
            bool serviceFeeInterest = true;
            //Calculate payments
            double firstPeriodInterestRate = Convert.ToDouble(input.InterestRate) * 0.01 / input.DaysInYear * firstPeriod;
            double regularPayment = CalcateAnnuityWithLongerFirstPeriod(input.LoanAmount, interestRate, firstPeriodInterestRate, annuityRegularPayment, input, serviceFeeInterest, false);
            //Calculate Fee payments
            bool applyServiceFeeInterest = (input.ApplyServiceFeeInterest & 1) == 1;
            double servicefirstPeriodInterestRate = applyServiceFeeInterest ? firstPeriodInterestRate : 0;
            double serviceInterestRate = applyServiceFeeInterest ? interestRate : 0;
            serviceFeeInterest = serviceInterestRate > 0 ? true : false;
            double regularServiceFee = CalcateAnnuityWithLongerFirstPeriod(serviceFee, serviceInterestRate, servicefirstPeriodInterestRate, servicePayment, input, serviceFeeInterest, true);
            List<PaymentDetail> loanPayments = RoundValueForFlexibleCalculation(input, regularPayment, regularServiceFee);
            return new getScheduleOutput
            {
                Schedule = loanPayments,
                RegularPayment = rounndedValue(loanPayments[0].TotalPayment),
                RegularServiceFeePayment = rounndedValue(loanPayments[0].ServiceFeeTotal),
                AccruedInterest = 0
            };
        }

        /// <summary>
        /// AG - 1.0.0.11 - Method to support Flexible calculation for Schedule with Longer first payment period
        /// </summary>
        private static double CalcateAnnuityWithLongerFirstPeriod(double totalPrincipal, double interestRate, double firstPeriodInterestRate, double regularPayment, getScheduleInput input, bool serviceInterest, bool calculateServiceFeeTotal)
        {
            int nPer = input.BalloonPayment > 0 && !calculateServiceFeeTotal ? input.InputRecords.Count - 2 : input.InputRecords.Count - 1;
            if (nPer == 0)
            {
                nPer = 1; //1 is minimum value
            }
            var payments = new List<PaymentDetail>();
            double currentPrincipal = totalPrincipal;
            double firstRegularInterestPayment = totalPrincipal * interestRate;
            double firstInterestPaymentPaid = totalPrincipal * firstPeriodInterestRate;
            double interestPaymentDifference = firstInterestPaymentPaid - firstRegularInterestPayment;
            double paymentOne = regularPayment;

            double interestCarryOver = 0;
            double interestPaymentDue = 0;
            double paymentLast = 0;
            double maxAllowedDiff = 0.15; //Maximum allowed difference between annuity payment and last period payment
            double currentDiff = Math.Abs(interestPaymentDifference); //Current difference between payments
            double interestDiff = 0;
            double multiplicator = 0.5; //AG: 1.0.0.16 - Multiplacator to make loop step everytime different in order to avoid infinite loops
            double currentInterestPayment = 0;
            double periodicIntrestrate = 0;
            int scheduleDayDifference = 0;
            //Iterate while difference reaches allowed value:
            if (input.InterestRate == 0 && input.InterestRate2 == 0 && input.InterestRate3 == 0 && input.InterestRate4 == 0)
            {
                paymentOne = input.BalloonPayment > 0 ? ((totalPrincipal - input.BalloonPayment) / (input.InputRecords.Count - 2)) : (totalPrincipal / (input.InputRecords.Count - 1));
                paymentOne = paymentOne <= 0 ? 0 : paymentOne;

            }
            else
            {
                while (currentDiff > maxAllowedDiff || !payments.Any())
                {
                    #region
                    for (int i = 1; i < (input.BalloonPayment > 0 && !calculateServiceFeeTotal ? input.InputRecords.Count() - 1 : input.InputRecords.Count()); i++)
                    {
                        interestPaymentDue = 0;
                        if (serviceInterest)
                        {
                            scheduleDayDifference = (input.InputRecords[i].DateIn - input.InputRecords[i - 1].DateIn).Days;
                            #region
                            double principalVal = input.Tier1 != 0 ? (input.Tier1 > currentPrincipal ? currentPrincipal : input.Tier1) : currentPrincipal;
                            //Calculate the interest for first tier
                            periodicIntrestrate = Convert.ToDouble(input.InterestRate) * 0.01 / input.DaysInYear * scheduleDayDifference;
                            currentInterestPayment = principalVal * periodicIntrestrate;

                            //Calculate the interest for second tier
                            if (!(input.InterestRate2 == 0) && input.Tier1 != 0 && currentPrincipal > input.Tier1)
                            {
                                principalVal = (input.Tier2 != 0 ? (input.Tier2 > currentPrincipal ? currentPrincipal : input.Tier2) : currentPrincipal) -
                                                                                input.Tier1;
                                currentInterestPayment += (Convert.ToDouble(input.InterestRate2) * 0.01 / input.DaysInYear * scheduleDayDifference) * principalVal;
                            }

                            //Calculate the interest for third tier
                            if (!(input.InterestRate3 == 0) && input.Tier1 != 0 && input.Tier2 != 0 && currentPrincipal > input.Tier2)
                            {
                                principalVal = (input.Tier3 != 0 ? (input.Tier3 > currentPrincipal ? currentPrincipal : input.Tier3) : currentPrincipal) -
                                                                                input.Tier2;
                                currentInterestPayment += (Convert.ToDouble(input.InterestRate3) * 0.01 / input.DaysInYear * scheduleDayDifference) * principalVal;
                            }

                            //Calculate the interest for fourth tier
                            if (!(input.InterestRate4 == 0) && input.Tier1 != 0 && input.Tier2 != 0 && input.Tier3 != 0 &&
                                                                currentPrincipal > input.Tier3)
                            {
                                principalVal = (input.Tier4 != 0 ? (input.Tier4 > currentPrincipal ? currentPrincipal : input.Tier4) : currentPrincipal) -
                                                                                input.Tier3;
                                currentInterestPayment += (Convert.ToDouble(input.InterestRate4) * 0.01 / input.DaysInYear * scheduleDayDifference) * principalVal;
                            }
                            #endregion
                        }
                        else
                        {
                            currentInterestPayment = 0;
                        }

                        double currentPrincipalPayment = paymentOne - currentInterestPayment;

                        if (interestDiff > 0)
                        {
                            if (interestDiff > currentPrincipalPayment)
                            {
                                interestPaymentDue = currentInterestPayment + interestDiff;
                                interestDiff -= currentPrincipalPayment;
                                //intersetCarryOver carry the remeaining amount of the period
                                interestCarryOver = interestDiff;
                                currentPrincipalPayment = 0;
                            }
                            else
                            {
                                //intersetCarryOver carry the remeaining amount of the period
                                interestPaymentDue = currentInterestPayment + interestCarryOver;
                                currentPrincipalPayment = paymentOne - interestPaymentDue;
                                interestCarryOver = 0;
                            }
                        }

                        if (currentPrincipalPayment < 0)
                        {
                            interestDiff = Math.Abs(currentPrincipalPayment);
                            //intersetCarryOver carry the remeaining amount of the period
                            if (i == 1)
                            {
                                interestPaymentDue = currentInterestPayment;
                                interestCarryOver = interestPaymentDue - paymentOne;
                            }
                            else
                            {
                                interestPaymentDue = currentInterestPayment + interestCarryOver;
                                interestCarryOver = interestPaymentDue - paymentOne;
                            }
                            currentPrincipalPayment = 0;
                        }
                        if (interestCarryOver == 0)
                        {
                            interestPaymentDue = currentInterestPayment + interestDiff;
                            interestDiff = 0;
                        }
                        //Check difference on last payment period:
                        if (i == nPer)
                        {
                            paymentLast = currentPrincipal + currentInterestPayment + interestDiff - (calculateServiceFeeTotal ? 0 : input.BalloonPayment);
                            if (paymentLast < 0)
                            {
                                paymentLast = 0;
                            }
                            currentDiff = Math.Abs(paymentOne - paymentLast);
                            if (currentDiff > maxAllowedDiff)
                            {
                                multiplicator += 0.5;
                                double change = Math.Abs(currentDiff) / (paymentOne + multiplicator);
                                if (change > currentDiff) change = currentDiff / nPer;
                                paymentOne = paymentOne > paymentLast ? paymentOne - change : paymentOne + change;

                                //Default all values and start again:
                                currentPrincipal = totalPrincipal;
                                interestDiff = 0;
                                payments.Clear();
                                continue;
                            }
                        }
                        payments.Add(new PaymentDetail
                        {
                            PaymentDate = input.InputRecords[i].DateIn,
                            BeginningPrincipal = currentPrincipal,
                            PrincipalPayment = currentPrincipalPayment > 0 ? rounndedValue(paymentOne) - rounndedValue(interestPaymentDue) : currentPrincipalPayment,
                            InterestAccrued = currentInterestPayment,
                            InterestCarryOver = interestCarryOver,
                            InterestDue = rounndedValue(interestPaymentDue),
                            TotalPayment = i == nPer ? paymentLast : paymentOne,
                            PeriodicInterestRate = periodicIntrestrate,
                            PaymentID = input.InputRecords[i].PaymentID,
                            Flags = input.InputRecords[i].Flags,
                        });
                        currentPrincipal -= currentPrincipalPayment;
                    }
                    #endregion
                }
            }
            return paymentOne;
        }

        /// <summary>
        /// This function is used to calculate the default Flexible method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="regularPayment"></param>
        /// <param name="regularServiceFee"></param>
        /// <returns></returns>
        private static List<PaymentDetail> RoundValueForFlexibleCalculation(getScheduleInput input, double regularPayment, double regularServiceFee)
        {
            var payments = new List<PaymentDetail>();
            double PeriodicInterestRate;
            double dailyInterestAmount = 0;
            double interestAccrued;
            double interestPayment;
            double interestDue;
            double InterestCarryOver = 0;
            double principalPayment;
            double serviceFee;
            double accruedServiceFeeInterest;
            double serviceFeeInterest;
            double serviceFeeInterestDue;
            double serviceFeeInterestCarryOver = 0;
            double ManagementFeeCarryOver = 0;
            double maintenencefeeCarryOver = 0;
            int scheduleDayDifference;
            double paymentAmount = 0;
            bool isPaymentAmount = false;
            double begnningPrincipal = rounndedValue(input.LoanAmount);
            double begnningServiceFee = rounndedValue(getStartingServiceFee(input.ServiceFee, input.LoanAmount, input.ApplyServiceFeeInterest));
            double regularLoanPayment = rounndedValue(regularPayment);
            double regularServiceFeePayment = rounndedValue(regularServiceFee);
            DateTime startDateWithDelay = input.InputRecords[0].DateIn.AddDays(input.InterestDelay);
            DateTime startDate;

            for (int i = 1; i < input.InputRecords.Count(); i++)
            {
                startDate = startDateWithDelay >= input.InputRecords[i - 1].DateIn ? startDateWithDelay : input.InputRecords[i - 1].DateIn;
                scheduleDayDifference = (startDate <= input.InputRecords[i].DateIn) ? (input.InputRecords[i].DateIn - startDate).Days : 0;

                PeriodicInterestRate = CalculatePeriodicInterestRateTier(input, begnningPrincipal, scheduleDayDifference);
                interestAccrued = CalculateCurrentInterestTier(input, begnningPrincipal, scheduleDayDifference);
                accruedServiceFeeInterest = CalculateCurrentServiceFeeInterestTier(input, begnningServiceFee, scheduleDayDifference);
                dailyInterestAmount = (PeriodicInterestRate == 0 || scheduleDayDifference == 0) ? 0 : rounndedValue((PeriodicInterestRate / scheduleDayDifference) * begnningPrincipal);
                if (i == 1)
                {
                    interestAccrued += input.EarnedInterest;
                    regularLoanPayment = regularLoanPayment + (input.EarnedInterest / (input.InputRecords.Count - 1));
                    accruedServiceFeeInterest += input.EarnedServiceFeeInterest;
                    regularServiceFeePayment = regularServiceFeePayment + (input.EarnedServiceFeeInterest / (input.InputRecords.Count - 1));
                }

                interestDue = interestAccrued + InterestCarryOver;
                serviceFeeInterestDue = accruedServiceFeeInterest + serviceFeeInterestCarryOver;
                if (i != (input.InputRecords.Count - 1) && (begnningPrincipal > 0))
                {
                    #region Calculate Interest Accrued and Principal amount
                    if (interestDue > regularLoanPayment)
                    {
                        interestPayment = regularLoanPayment;
                        InterestCarryOver = interestDue - interestPayment;
                        principalPayment = 0;
                    }
                    else
                    {
                        interestPayment = interestDue;
                        InterestCarryOver = 0;
                        //principalPayment = rounndedValue(regularLoanPayment - rounndedValue(interestPayment));
                        principalPayment = rounndedValue(regularLoanPayment - rounndedValue(interestPayment));
                    }
                    if (i >= input.EnforcedPayment && principalPayment == 0 && interestPayment > 0)
                    {
                        principalPayment = interestPayment <= input.EnforcedPrincipal ? interestPayment : rounndedValue(input.EnforcedPrincipal);
                        interestPayment -= principalPayment;
                        InterestCarryOver += principalPayment;
                        principalPayment = principalPayment <= 0 ? 0 : principalPayment;
                    }
                    principalPayment = principalPayment <= begnningPrincipal ? principalPayment : begnningPrincipal;

                    #endregion

                    #region Calculate Accrued Service Fee Interest and service fee amount
                    if (input.ServiceFeeFirstPayment && string.IsNullOrEmpty(input.InputRecords[i].PaymentAmount) && input.PaymentAmount <= 0)//&& i == 1)
                    {
                        #region
                        serviceFee = begnningServiceFee;
                        serviceFeeInterest = serviceFeeInterestDue;
                        serviceFeeInterestCarryOver = 0;
                        #endregion
                    }
                    else if (serviceFeeInterestDue > regularServiceFeePayment)
                    {
                        serviceFeeInterest = regularServiceFeePayment;
                        serviceFeeInterestCarryOver = serviceFeeInterestDue - regularServiceFeePayment;
                        serviceFee = 0;
                    }
                    else
                    {
                        serviceFeeInterest = serviceFeeInterestDue;
                        serviceFeeInterestCarryOver = 0;
                        serviceFee = rounndedValue(regularServiceFeePayment - rounndedValue(serviceFeeInterest));
                        serviceFee = serviceFee <= begnningServiceFee ? serviceFee : begnningServiceFee;
                    }
                    #endregion
                }
                else
                {
                    #region
                    interestPayment = interestDue;
                    principalPayment = rounndedValue(begnningPrincipal);
                    serviceFeeInterest = serviceFeeInterestDue;
                    serviceFee = rounndedValue(begnningServiceFee);
                    serviceFeeInterestCarryOver = 0;
                    InterestCarryOver = 0;
                    regularLoanPayment = rounndedValue(interestPayment + principalPayment);
                    regularServiceFeePayment = rounndedValue(serviceFee + serviceFeeInterest);
                    #endregion
                }
                if (begnningPrincipal > 0 || begnningServiceFee > 0 || interestPayment > 0 || serviceFeeInterest > 0 || ManagementFeeCarryOver > 0 || maintenencefeeCarryOver > 0)
                {
                    payments.Add(new PaymentDetail
                    {
                        #region Add Schedule
                        PaymentDate = input.InputRecords[i].DateIn,
                        DueDate = input.InputRecords[i].EffectiveDate,
                        BeginningPrincipal = begnningPrincipal,
                        PrincipalPayment = principalPayment,
                        DailyInterestAmount = dailyInterestAmount,
                        InterestAccrued = interestAccrued,
                        InterestPayment = interestPayment,
                        InterestDue = interestDue,
                        InterestCarryOver = InterestCarryOver,
                        TotalPayment = regularLoanPayment,
                        PeriodicInterestRate = PeriodicInterestRate,
                        PaymentID = input.InputRecords[i].PaymentID,
                        Flags = input.InputRecords[i].Flags,
                        //AD - 1.0.0.6 - put additional fees to output
                        BeginningServiceFee = begnningServiceFee,
                        AccruedServiceFeeInterest = accruedServiceFeeInterest,
                        serviceFeeInterestCarryOver = serviceFeeInterestCarryOver,
                        ServiceFee = begnningPrincipal > 0 ? serviceFee : begnningServiceFee,
                        ServiceFeeInterest = serviceFeeInterest,
                        ServiceFeeTotal = regularServiceFeePayment,
                        OriginationFee = i == 1 ? input.OriginationFee : 0,
                        SameDayFee = i == 1 ? input.SameDayFee : 0,//AD - 1.0.0.24 - New property same day fee
                        MaintenanceFee = i == 1 ? 0 : input.MaintenanceFee,
                        MaintenanceFeeCarryOver = 0,
                        ManagementFee = input.ManagementFee,
                        ManagementFeeCarryOver = 0
                        #endregion
                    });

                    if (!string.IsNullOrEmpty(input.InputRecords[i].PaymentAmount) || input.PaymentAmount > 0)
                    {
                        #region Allocate amount on the basis of priority when Actual amount is given

                        if (!string.IsNullOrEmpty(input.InputRecords[i].PaymentAmount))
                        {
                            paymentAmount = rounndedValue(Convert.ToDouble(input.InputRecords[i].PaymentAmount));
                            isPaymentAmount = false;
                        }
                        else
                        {
                            isPaymentAmount = true;
                            paymentAmount = rounndedValue(input.PaymentAmount + (i == 1 ? input.OriginationFee + input.SameDayFee : 0));
                            ManagementFeeCarryOver += input.ManagementFee;
                            maintenencefeeCarryOver += i == 1 ? 0 : input.MaintenanceFee;
                        }

                        if (input.OriginationFee > 0 && i == 1)
                        {

                            #region
                            if (input.OriginationFee > paymentAmount)
                            {
                                payments[payments.Count - 1].OriginationFee = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                paymentAmount = rounndedValue(paymentAmount - rounndedValue(payments[payments.Count - 1].OriginationFee));
                            }
                            #endregion
                        }
                        if (input.SameDayFee > 0 && i == 1)
                        {
                            #region
                            if (input.SameDayFee > paymentAmount)
                            {
                                payments[payments.Count - 1].SameDayFee = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                paymentAmount = rounndedValue(paymentAmount - rounndedValue(payments[payments.Count - 1].SameDayFee));
                            }
                            #endregion
                        }
                        if (payments[payments.Count - 1].InterestPayment > 0)
                        {
                            #region
                            if (payments[payments.Count - 1].InterestPayment > paymentAmount)
                            {
                                InterestCarryOver = payments[payments.Count - 1].InterestDue - paymentAmount;
                                payments[payments.Count - 1].InterestCarryOver = InterestCarryOver;
                                payments[payments.Count - 1].InterestPayment = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                paymentAmount = rounndedValue(paymentAmount - rounndedValue(payments[payments.Count - 1].InterestPayment));
                            }
                            #endregion
                        }
                        if (payments[payments.Count - 1].ServiceFeeInterest > 0)
                        {
                            #region
                            if (payments[payments.Count - 1].ServiceFeeInterest > paymentAmount)
                            {
                                serviceFeeInterestCarryOver += payments[payments.Count - 1].ServiceFeeInterest - paymentAmount;
                                payments[payments.Count - 1].serviceFeeInterestCarryOver = serviceFeeInterestCarryOver;
                                payments[payments.Count - 1].ServiceFeeInterest = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                paymentAmount = rounndedValue(paymentAmount - rounndedValue(payments[payments.Count - 1].ServiceFeeInterest));
                            }
                            #endregion
                        }
                        if (payments[payments.Count - 1].PrincipalPayment > 0)
                        {
                            #region
                            if (payments[payments.Count - 1].PrincipalPayment > paymentAmount)
                            {
                                payments[payments.Count - 1].PrincipalPayment = paymentAmount;
                                principalPayment = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                paymentAmount = rounndedValue(paymentAmount - rounndedValue(payments[payments.Count - 1].PrincipalPayment));
                            }
                            #endregion
                        }
                        if (payments[payments.Count - 1].ServiceFee > 0)
                        {
                            #region
                            if (payments[payments.Count - 1].ServiceFee > paymentAmount)
                            {
                                payments[payments.Count - 1].ServiceFee = paymentAmount;
                                serviceFee = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                paymentAmount = rounndedValue(paymentAmount - rounndedValue(payments[payments.Count - 1].ServiceFee));
                            }
                            #endregion
                        }
                        if (i != (input.InputRecords.Count - 1))
                        {
                            if (isPaymentAmount)
                            {
                                #region calculate management and maintenance fee amount
                                if (ManagementFeeCarryOver > 0)
                                {
                                    #region
                                    if (ManagementFeeCarryOver > paymentAmount)
                                    {
                                        payments[payments.Count - 1].ManagementFee = paymentAmount;
                                        ManagementFeeCarryOver -= paymentAmount;
                                        payments[payments.Count - 1].ManagementFeeCarryOver = ManagementFeeCarryOver;
                                        paymentAmount = 0;
                                    }
                                    else
                                    {
                                        payments[payments.Count - 1].ManagementFee = ManagementFeeCarryOver;
                                        paymentAmount = rounndedValue(paymentAmount - rounndedValue(payments[payments.Count - 1].ManagementFee));
                                        payments[payments.Count - 1].ManagementFeeCarryOver = 0;
                                        ManagementFeeCarryOver = 0;
                                    }
                                    #endregion
                                }
                                if (maintenencefeeCarryOver > 0)
                                {
                                    #region
                                    if (maintenencefeeCarryOver > paymentAmount)
                                    {
                                        payments[payments.Count - 1].MaintenanceFee = paymentAmount;
                                        maintenencefeeCarryOver -= paymentAmount;
                                        payments[payments.Count - 1].MaintenanceFeeCarryOver = maintenencefeeCarryOver;
                                        paymentAmount = 0;
                                    }
                                    else
                                    {
                                        payments[payments.Count - 1].MaintenanceFee = maintenencefeeCarryOver;
                                        paymentAmount = rounndedValue(paymentAmount - rounndedValue(payments[payments.Count - 1].MaintenanceFee));
                                        payments[payments.Count - 1].MaintenanceFeeCarryOver = 0;
                                        maintenencefeeCarryOver = 0;
                                    }
                                    #endregion
                                }
                                #endregion
                            }
                            else
                            {
                                #region calculate management and maintenance fee amount
                                if (payments[payments.Count - 1].ManagementFee > 0)
                                {
                                    #region
                                    if (payments[payments.Count - 1].ManagementFee > paymentAmount)
                                    {
                                        payments[payments.Count - 1].ManagementFee = paymentAmount;
                                        payments[payments.Count - 1].ManagementFeeCarryOver = ManagementFeeCarryOver;
                                        paymentAmount = 0;
                                    }
                                    else
                                    {
                                        //ManagementFeeCarryOver += payments[payments.Count - 1].ManagementFee;
                                        //payments[payments.Count - 1].ManagementFee = (ManagementFeeCarryOver > paymentAmount) ? paymentAmount : (paymentAmount - ManagementFeeCarryOver);
                                        //ManagementFeeCarryOver = ManagementFeeCarryOver - payments[payments.Count - 1].ManagementFee;
                                        payments[payments.Count - 1].ManagementFeeCarryOver = ManagementFeeCarryOver;
                                        paymentAmount = rounndedValue(paymentAmount - rounndedValue(payments[payments.Count - 1].ManagementFee));
                                    }
                                    #endregion
                                }
                                if (payments[payments.Count - 1].MaintenanceFee > 0)
                                {
                                    #region
                                    if (payments[payments.Count - 1].MaintenanceFee > paymentAmount)
                                    {
                                        payments[payments.Count - 1].MaintenanceFee = paymentAmount;
                                        payments[payments.Count - 1].MaintenanceFeeCarryOver = maintenencefeeCarryOver;
                                        paymentAmount = 0;
                                    }
                                    else
                                    {

                                        //maintenencefeeCarryOver += payments[payments.Count - 1].MaintenanceFee;
                                        //payments[payments.Count - 1].MaintenanceFee = (maintenencefeeCarryOver > paymentAmount) ?  paymentAmount : (paymentAmount - maintenencefeeCarryOver);
                                        //maintenencefeeCarryOver = maintenencefeeCarryOver - payments[payments.Count - 1].MaintenanceFee;
                                        payments[payments.Count - 1].MaintenanceFeeCarryOver = maintenencefeeCarryOver;
                                        paymentAmount = rounndedValue(paymentAmount - rounndedValue(payments[payments.Count - 1].MaintenanceFee));
                                    }
                                    #endregion
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            #region calculate management and maintenance fee amount
                            payments[payments.Count - 1].ManagementFee = ManagementFeeCarryOver;
                            payments[payments.Count - 1].ManagementFeeCarryOver = 0;
                            ManagementFeeCarryOver = 0;
                            payments[payments.Count - 1].MaintenanceFee = maintenencefeeCarryOver;
                            payments[payments.Count - 1].MaintenanceFeeCarryOver = 0;
                            maintenencefeeCarryOver = 0;
                            #endregion
                        }

                        if (paymentAmount > 0)//When payment amount remains after full payment of schedule
                        {
                            #region
                            double remainingPincipal = begnningPrincipal - payments[payments.Count - 1].PrincipalPayment;
                            remainingPincipal = remainingPincipal <= 0 ? 0 : remainingPincipal;
                            double remainingServiceFee = begnningServiceFee - payments[payments.Count - 1].ServiceFee;
                            remainingServiceFee = remainingServiceFee <= 0 ? 0 : remainingServiceFee;
                            #region
                            if (InterestCarryOver > paymentAmount)
                            {
                                payments[payments.Count - 1].InterestPayment += paymentAmount;
                                payments[payments.Count - 1].InterestCarryOver -= paymentAmount;
                                InterestCarryOver -= paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                payments[payments.Count - 1].InterestPayment += InterestCarryOver;
                                paymentAmount = rounndedValue(paymentAmount - InterestCarryOver);
                                payments[payments.Count - 1].InterestCarryOver = 0;
                                InterestCarryOver = 0;
                            }
                            #endregion

                            #region
                            if (serviceFeeInterestCarryOver > paymentAmount)
                            {
                                payments[payments.Count - 1].ServiceFeeInterest += paymentAmount;
                                payments[payments.Count - 1].serviceFeeInterestCarryOver -= paymentAmount;
                                serviceFeeInterestCarryOver -= paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                payments[payments.Count - 1].ServiceFeeInterest += serviceFeeInterestCarryOver;
                                paymentAmount = rounndedValue(paymentAmount - serviceFeeInterestCarryOver);
                                payments[payments.Count - 1].serviceFeeInterestCarryOver = 0;
                                serviceFeeInterestCarryOver = 0;
                            }
                            #endregion

                            #region
                            if (remainingPincipal > paymentAmount)
                            {
                                payments[payments.Count - 1].PrincipalPayment += paymentAmount;
                                principalPayment += paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                payments[payments.Count - 1].PrincipalPayment += remainingPincipal;
                                principalPayment += remainingPincipal;
                                paymentAmount = rounndedValue(paymentAmount - remainingPincipal);
                            }
                            #endregion

                            #region
                            if (remainingServiceFee > paymentAmount)
                            {
                                payments[payments.Count - 1].ServiceFee += paymentAmount;
                                serviceFee += paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                payments[payments.Count - 1].ServiceFee += remainingServiceFee;
                                serviceFee += remainingServiceFee;
                                paymentAmount = rounndedValue(paymentAmount - remainingServiceFee);
                            }
                            #endregion

                            #region
                            if (payments[payments.Count - 1].ManagementFeeCarryOver > paymentAmount)
                            {
                                payments[payments.Count - 1].ManagementFee += paymentAmount;
                                ManagementFeeCarryOver -= paymentAmount;
                                payments[payments.Count - 1].ManagementFeeCarryOver -= paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                payments[payments.Count - 1].ManagementFee += payments[payments.Count - 1].ManagementFeeCarryOver;
                                payments[payments.Count - 1].ManagementFeeCarryOver = 0;
                                ManagementFeeCarryOver = 0;
                                paymentAmount = rounndedValue(paymentAmount - payments[payments.Count - 1].ManagementFeeCarryOver);
                            }
                            #endregion

                            #region
                            if (payments[payments.Count - 1].MaintenanceFeeCarryOver > paymentAmount)
                            {
                                payments[payments.Count - 1].MaintenanceFee += paymentAmount;
                                maintenencefeeCarryOver -= paymentAmount;
                                payments[payments.Count - 1].MaintenanceFeeCarryOver -= paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                payments[payments.Count - 1].MaintenanceFee += payments[payments.Count - 1].MaintenanceFeeCarryOver;
                                payments[payments.Count - 1].MaintenanceFeeCarryOver = 0;
                                maintenencefeeCarryOver = 0;
                                paymentAmount = rounndedValue(paymentAmount - payments[payments.Count - 1].MaintenanceFeeCarryOver);
                            }
                            #endregion

                            #endregion
                        }

                        #endregion

                    }
                    #region Calculate Cumulative Amount
                    payments[payments.Count - 1].CumulativeInterest = (payments.Count) == 1 ? payments[payments.Count - 1].InterestPayment : payments[payments.Count - 2].CumulativeInterest + payments[payments.Count - 1].InterestPayment;
                    payments[payments.Count - 1].CumulativePrincipal = (payments.Count) == 1 ? payments[payments.Count - 1].PrincipalPayment : payments[payments.Count - 2].CumulativePrincipal + payments[payments.Count - 1].PrincipalPayment;
                    payments[payments.Count - 1].CumulativePayment = (payments.Count) == 1 ? payments[payments.Count - 1].TotalPayment : payments[payments.Count - 2].CumulativePayment + payments[payments.Count - 1].TotalPayment;
                    payments[payments.Count - 1].CumulativeMaintenanceFee = (payments.Count) == 1 ? payments[payments.Count - 1].MaintenanceFee : payments[payments.Count - 2].CumulativeMaintenanceFee + payments[payments.Count - 1].MaintenanceFee;
                    payments[payments.Count - 1].CumulativeManagementFee = (payments.Count) == 1 ? payments[payments.Count - 1].ManagementFee : payments[payments.Count - 2].CumulativeManagementFee + payments[payments.Count - 1].ManagementFee;
                    payments[payments.Count - 1].CumulativeOriginationFee = (payments.Count) == 1 ? payments[payments.Count - 1].OriginationFee : payments[payments.Count - 2].CumulativeOriginationFee + payments[payments.Count - 1].OriginationFee;
                    payments[payments.Count - 1].CumulativeSameDayFee = (payments.Count) == 1 ? payments[payments.Count - 1].SameDayFee : payments[payments.Count - 2].CumulativeSameDayFee + payments[payments.Count - 1].SameDayFee;
                    payments[payments.Count - 1].CumulativeServiceFee = (payments.Count) == 1 ? payments[payments.Count - 1].ServiceFee : payments[payments.Count - 2].CumulativeServiceFee + payments[payments.Count - 1].ServiceFee;
                    payments[payments.Count - 1].CumulativeServiceFeeInterest = (payments.Count) == 1 ? payments[payments.Count - 1].ServiceFeeInterest : payments[payments.Count - 2].CumulativeServiceFeeInterest + payments[payments.Count - 1].ServiceFeeInterest;

                    payments[payments.Count - 1].CumulativeTotalFees = rounndedValue((payments[payments.Count - 1].CumulativeMaintenanceFee +
                                                                                                   payments[payments.Count - 1].CumulativeManagementFee +
                                                                                                   payments[payments.Count - 1].CumulativeOriginationFee +
                                                                                                   payments[payments.Count - 1].CumulativeSameDayFee +
                                                                                                   payments[payments.Count - 1].CumulativeServiceFee +
                                                                                                   payments[payments.Count - 1].CumulativeServiceFeeInterest));
                    #endregion
                }
                begnningPrincipal = rounndedValue((begnningPrincipal - principalPayment));

                //AD - 1.0.0.6 - count principal payment for service fee
                begnningServiceFee = rounndedValue((begnningServiceFee - serviceFee));
            }
            return payments;
        }

        public static double rounndedValue(double value)
        {
            int decimalPlace = 2;
            StringBuilder sb = new StringBuilder();
            StringBuilder decimalPlaceVale = new StringBuilder();
            decimalPlaceVale.Append(".");
            double rounded = 0; ;
            string[] splitAfterResult;
            string[] split = value.ToString().Split('.');
            string decimalvalue = split.Length > 1 ? split[1].ToString() : string.Empty;
            if (decimalvalue.Length > decimalPlace)
            {
                int indexvalue = Convert.ToInt32(decimalvalue.Substring(decimalPlace, 1));
                for (int i = 0; i < decimalPlace - 1; i++)
                {
                    decimalPlaceVale.Append("0");
                }
                decimalPlaceVale.Append("1");
                if (indexvalue >= 5)
                {
                    rounded = value + Convert.ToDouble(decimalPlaceVale.ToString());
                    splitAfterResult = rounded.ToString().Split('.');
                    decimalvalue = splitAfterResult[1].ToString();
                    sb.Append(splitAfterResult[0].ToString() + ".");
                }
                else
                {
                    sb.Append(split[0].ToString() + ".");
                }

            }
            else
            {
                sb.Append(split[0].ToString() + ".");
            }
            if (decimalvalue.Length > 0)
            {
                for (int decimalIndex = 0; decimalIndex < (decimalvalue.Length >= decimalPlace ? decimalPlace : decimalvalue.Length); decimalIndex++)
                {
                    sb.Append(decimalvalue[decimalIndex]);
                }
            }

            return Convert.ToDouble(sb.ToString());
        }

        /// <summary>
        /// This function is used to calculate first payment period.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static int CalculateFirstPaymentPeriod(getScheduleInput input)
        {
            List<InputRecord> transactions = input.InputRecords;
            int daysInRegularPaymentPeriod = getDaysPerPeriod(input.PmtPeriod);
            if (transactions.Count < 2)
            {
                return daysInRegularPaymentPeriod;
            }
            int calendarPeriod = (input.InputRecords[1].DateIn - input.InputRecords[0].DateIn).Days;
            if (calendarPeriod == daysInRegularPaymentPeriod)
            {
                return daysInRegularPaymentPeriod;
            }
            int daysDiff = Math.Abs(calendarPeriod - daysInRegularPaymentPeriod);
            if (daysDiff > 3)
            {
                return calendarPeriod; //3 is maximum allowed diff: Feb 28 - March 31
            }

            DateTime effectiveDate = transactions[0].DateIn.Date;
            DateTime firstPaymentDate = transactions[1].DateIn.Date;
            bool firstPaymentInTheNextMonth = effectiveDate.AddMonths(1).Month == firstPaymentDate.Month;

            switch (input.PmtPeriod)
            {
                case PaymentPeriod.SemiMonthly:
                    return calendarPeriod;
                case PaymentPeriod.Monthly:
                    if (!firstPaymentInTheNextMonth) return calendarPeriod;

                    //Same day of the month:
                    if (effectiveDate.Day == firstPaymentDate.Day)
                    {
                        return daysInRegularPaymentPeriod;
                    }

                    //Last day of the month:
                    if (effectiveDate.Day == DateTime.DaysInMonth(effectiveDate.Year, effectiveDate.Month)
                        && firstPaymentDate.Day == DateTime.DaysInMonth(firstPaymentDate.Year, firstPaymentDate.Month))
                    {
                        return daysInRegularPaymentPeriod;
                    }
                    //Febrary:
                    if ((effectiveDate.Day == 30 || effectiveDate.Day == 29)
                        && transactions[1].DateIn.Month == 2 && transactions[1].DateIn.Day == 28)
                    {
                        return daysInRegularPaymentPeriod;
                    }
                    return calendarPeriod;
                default:
                    return calendarPeriod;
            }
        }

        /// <summary>
        /// AD - 1.0.0.10 - check if payment schedule was counted correctly
        /// (cumulative principal payment is neither negative nor extremely big)
        /// </summary>
        /// <param name="Output"></param>
        /// <param name="LoanAmount"></param>
        /// <returns></returns>
        private static bool checkForCorrectOutput(getScheduleOutput Output, double LoanAmount)
        {
            const double acceptableVariance = 0.1;
            var lastPayment = Output.Schedule[Output.Schedule.Count - 1];
            var regularPaymentAmount = lastPayment.TotalPayment;
            var acceptableVarianceAbsolute = regularPaymentAmount * acceptableVariance;
            return Math.Abs(lastPayment.CumulativePrincipal - LoanAmount) <= acceptableVarianceAbsolute;
        }

        /// <summary>
        ///  AD 1.0.0.10 - calculate regular payment taking rounding flag into account
        /// </summary>
        /// <param name="periodicInterestRate"></param>
        /// <param name="numberOfPayments"></param>
        /// <param name="beginningAmount"></param>
        /// <param name="isRegularPaymentRounded"></param>
        /// <returns></returns>
        private static double getRegularPaymentAmount(double periodicInterestRate, int numberOfPayments, double beginningAmount, bool isRegularPaymentRounded)
        {
            // AD 1.0.0.23 - number of payments cannot be less than 1
            if (numberOfPayments < 1)
            {
                numberOfPayments = 1;
            }

            double payment = -1 * Financial.Pmt(periodicInterestRate, numberOfPayments, beginningAmount, 0, DueDate.EndOfPeriod);
            if (isRegularPaymentRounded)
            {
                payment = rounndedValue(payment);
            }

            return payment;
        }
    }
}
