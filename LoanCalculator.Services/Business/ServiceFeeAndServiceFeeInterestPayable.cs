﻿using System;
using System.Collections.Generic;
using System.Text;
using LoanAmort_driver.Models;
using Microsoft.VisualBasic;

namespace LoanAmort_driver.Business
{
    public class ServiceFeeAndServiceFeeInterestPayable
    {
        /// <summary>
        /// Name : Sparsh Agarwal
        /// Date : 21/03/2017
        /// Explanation : This function is used to calculate the service fee total to be paid in each of the period.
        /// </summary>
        /// <param name="principalServiceFee"></param>
        /// <param name="scheduleInputs"></param>
        /// <param name="loanAmountCalc"></param>
        public static double CalculateServiceFeeAndInterestPayable(double principalServiceFee, LoanDetails scheduleInputs, double loanAmountCalc)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside ServiceFeeAndServiceFeeInterestPayable Class file and method name CalculateServiceFeeAndInterestPayable(double principalServiceFee, LoanDetails scheduleInputs, double loanAmountCalc).");
                sbTracing.AppendLine("Parameter values are : principalServiceFee = " + principalServiceFee + ", loanAmountCalc = " + loanAmountCalc);
                double totalServiceFeePayable = 0;

                //This condition is to calculate the total service fee payable amount when the service fee interest is not calculating.
                if (scheduleInputs.ApplyServiceFeeInterest == -1 || scheduleInputs.ApplyServiceFeeInterest == 0 || scheduleInputs.ApplyServiceFeeInterest == 2 ||
                    principalServiceFee == 0 || (Convert.ToDouble(scheduleInputs.InterestRate) == 0 &&
                        (string.IsNullOrEmpty(scheduleInputs.InterestRate2) || (!string.IsNullOrEmpty(scheduleInputs.InterestRate2) && Convert.ToDouble(scheduleInputs.InterestRate2) == 0)) &&
                        (string.IsNullOrEmpty(scheduleInputs.InterestRate3) || (!string.IsNullOrEmpty(scheduleInputs.InterestRate3) && Convert.ToDouble(scheduleInputs.InterestRate3) == 0)) &&
                        (string.IsNullOrEmpty(scheduleInputs.InterestRate4) || (!string.IsNullOrEmpty(scheduleInputs.InterestRate4) && Convert.ToDouble(scheduleInputs.InterestRate4) == 0))))
                {
                    //If service fee interest is not to be paid, then total payable value will be principal service fee divided by total payaments.
                    totalServiceFeePayable = Math.Round(principalServiceFee / (scheduleInputs.InputRecords.Count - 1), 2, MidpointRounding.AwayFromZero);// +
                                                                                                                                                         //Math.Round(scheduleInputs.EarnedServiceFeeInterest / (scheduleInputs.InputRecords.Count - 1), 2, MidpointRounding.AwayFromZero);
                }
                //This condition is to calculate the total service fee payable amount when the service fee interest is being calculating.
                else if (scheduleInputs.ApplyServiceFeeInterest == 1 || scheduleInputs.ApplyServiceFeeInterest == 3)
                {
                    //This variable is assigned the periodic interest rate of first schedule.
                    sbTracing.AppendLine("Inside ServiceFeeAndServiceFeeInterestPayable class, and CalculateServiceFeeAndInterestPayable() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInputs, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[0].DateIn, principalServiceFee, false, true);");
                    double regularInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInputs, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[0].DateIn, principalServiceFee, false, true, true);

                    sbTracing.AppendLine("Inside ServiceFeeAndServiceFeeInterestPayable class, and CalculateServiceFeeAndInterestPayable() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[0].DateIn, principalServiceFee, false, true);");
                    double firstPeriodInterestAmount = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[0].DateIn, principalServiceFee, false, true, true);

                    double regularInterestAmount = firstPeriodInterestAmount;
                    //This condition checks, that there are more than one repayment schedule dates.
                    if (scheduleInputs.InputRecords.Count > 2 && scheduleInputs.DaysInMonth.ToLower() == Models.Constants.Periodic)
                    {
                        //This variable is assigned the the periodic interest rate of second repayment schedule date.
                        sbTracing.AppendLine("Inside ServiceFeeAndServiceFeeInterestPayable class, and CalculateServiceFeeAndInterestPayable() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInputs, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[2].DateIn, scheduleInputs.InputRecords[0].DateIn, principalServiceFee, false, true);");
                        regularInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInputs, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[2].DateIn, scheduleInputs.InputRecords[0].DateIn, principalServiceFee, false, true, true);

                        sbTracing.AppendLine("Inside ServiceFeeAndServiceFeeInterestPayable class, and CalculateServiceFeeAndInterestPayable() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[2].DateIn, scheduleInputs.InputRecords[0].DateIn, principalServiceFee, false, true);");
                        regularInterestAmount = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[2].DateIn, scheduleInputs.InputRecords[0].DateIn, principalServiceFee, false, true, true);
                    }

                    //This variable is assigned the default total payment for each schedule.
                    if (regularInterestRate == 0)
                    {
                        totalServiceFeePayable = Math.Round(principalServiceFee / (scheduleInputs.InputRecords.Count - 1), 2, MidpointRounding.AwayFromZero);// +
                                                                                                                                                             //Math.Round(scheduleInputs.EarnedServiceFeeInterest / (scheduleInputs.InputRecords.Count - 1), 2, MidpointRounding.AwayFromZero));
                    }
                    else
                    {
                        double regularPayment = ((principalServiceFee * regularInterestRate * Math.Pow(1 + regularInterestRate, (scheduleInputs.InputRecords.Count - 1))) / (Math.Pow(1 + regularInterestRate, (scheduleInputs.InputRecords.Count - 1)) - 1));

                        //Calculating tatal service fee payable.
                        sbTracing.AppendLine("Inside ServiceFeeAndServiceFeeInterestPayable class, and CalculateServiceFeeAndInterestPayable() method. Calling method : CalculateAnnuity.CalcateAnnuityWithLongerFirstPeriod(regularInterestAmount, firstPeriodInterestAmount, regularPayment, scheduleInputs, true, principalServiceFee, loanAmountCalc);");
                        totalServiceFeePayable = CalculateAnnuity.CalcateAnnuityWithLongerFirstPeriod(regularInterestAmount, firstPeriodInterestAmount, regularPayment, scheduleInputs, true, principalServiceFee, loanAmountCalc);
                    }
                }
                sbTracing.AppendLine("Exit:From ServiceFeeAndServiceFeeInterestPayable class file and method name CalculateServiceFeeAndInterestPayable(double principalServiceFee, LoanDetails scheduleInputs, double loanAmountCalc)");
                totalServiceFeePayable += Math.Round(scheduleInputs.EarnedServiceFeeInterest / (scheduleInputs.InputRecords.Count - 1), 2, MidpointRounding.AwayFromZero);

                return totalServiceFeePayable;
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
                return 0;
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }

        /// <summary>
        /// This function calculates the total servicee fee interest accrued amount till the provided date in the input field, by calling the GetAccruedValue() function.
        /// </summary>
        /// <param name="paymentDetailIn">Output grid where all repayment schedules get generated.</param>
        /// <param name="accruedInterestDate"></param>
        /// <param name="effectiveDate"></param>
        /// <param name="scheduleInputs"></param>
        /// <returns></returns>
        public static double GetAccruedServiceFeeInterest(List<OutputGrid> paymentDetailIn, DateTime accruedInterestDate, DateTime effectiveDate, LoanDetails scheduleInputs)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside ServiceFeeAndServiceFeeInterestPayable Class file and method name GetAccruedServiceFeeInterest(List<OutputGrid> paymentDetailIn, DateTime accruedInterestDate, DateTime effectiveDate, LoanDetails scheduleInputs).");
                sbTracing.AppendLine("Parameter values are : accruedInterestDate = " + accruedInterestDate + ", effectiveDate = " + effectiveDate);

                double returnVal = GetAccruedValue(paymentDetailIn, accruedInterestDate, effectiveDate, Models.Constants.ServiceFeeInterest, scheduleInputs);

                sbTracing.AppendLine("Exit:From ServiceFeeAndServiceFeeInterestPayable Class file and method name GetAccruedServiceFeeInterest(List<OutputGrid> paymentDetailIn, DateTime accruedInterestDate, DateTime effectiveDate, LoanDetails scheduleInputs)");
                return returnVal;
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
                return 0;
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }

        /// <summary>
        /// This function calculate the total accrued principal, interest, and service fee interest amount till the provided date in the input field.
        /// </summary>
        /// <param name="paymentDetailIn">Output grid where all repayment schedules get generated.</param>
        /// <param name="accruedInterestDate"></param>
        /// <param name="effectiveDate"></param>
        /// <param name="dataType"></param>
        /// <param name="scheduleInputs"></param>
        /// <returns></returns>
        private static double GetAccruedValue(List<OutputGrid> paymentDetailIn, DateTime accruedInterestDate, DateTime effectiveDate, string dataType, LoanDetails scheduleInputs)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside Business Class ServiceFeeAndServiceFeeInterestPayable and method name GetAccruedValue(List<OutputGrid> paymentDetailIn, DateTime accruedInterestDate, DateTime effectiveDate, string dataType, LoanDetails scheduleInputs).");
                sbTracing.AppendLine("Parameter values are : " + "accruedInterestDate = " + accruedInterestDate + ", effectiveDate = " + effectiveDate + ", dataType = " + dataType);
                double returnVal = 0;
                bool doPartialPeriod = false;
                DateTime dateFrom = dataType == Models.Constants.Principal ? effectiveDate : effectiveDate.AddDays(scheduleInputs.InterestDelay);
                DateTime dateTo = Convert.ToDateTime(Models.Constants.DefaultDate);
                double previousAccumulated = 0;
                double nextAccumulated = 0;
                bool isEqualToScheduledDate = false;
                double earnedFee = 0;

                if (paymentDetailIn[0].PaymentDate <= accruedInterestDate &&
                        (paymentDetailIn[0].Flags == (int)Models.Constants.FlagValues.NSFFee || paymentDetailIn[0].Flags == (int)Models.Constants.FlagValues.LateFee ||
                        paymentDetailIn[0].Flags == (int)Models.Constants.FlagValues.ManagementFee || paymentDetailIn[0].Flags == (int)Models.Constants.FlagValues.MaintenanceFee))
                {
                    switch (dataType)
                    {
                        case Models.Constants.ServiceFeeInterest:
                            earnedFee = scheduleInputs.EarnedServiceFeeInterest;
                            break;
                        case Models.Constants.InterestAccrued:
                            earnedFee = scheduleInputs.EarnedInterest;
                            break;
                    }
                }

                //This condition checks whether the accrued interest date is not equal to the effective date.
                if (dateFrom < accruedInterestDate)
                {
                    //The loop will calculate the value starting from 0 index to last index of output grid.
                    for (int i = 0; i < paymentDetailIn.Count; i++)
                    {
                        #region This condition checks whether the payment date is equal to the accrued interest date till which the amount is to be calculated.
                        if (paymentDetailIn[i].PaymentDate == accruedInterestDate)
                        {
                            //This condition checks whether current output grid row is not an event of either NSF fee or Late fee.
                            if (paymentDetailIn[i].Flags != (int)Models.Constants.FlagValues.NSFFee && paymentDetailIn[i].Flags != (int)Models.Constants.FlagValues.LateFee &&
                                paymentDetailIn[i].Flags != (int)Models.Constants.FlagValues.ManagementFee && paymentDetailIn[i].Flags != (int)Models.Constants.FlagValues.MaintenanceFee)
                            {
                                isEqualToScheduledDate = true;
                                if (dataType == Models.Constants.Principal && paymentDetailIn[i].PrincipalPayment == 0 &&
                                    (paymentDetailIn[i].Flags != (int)Models.Constants.FlagValues.Discount &&
                                        paymentDetailIn[i].Flags != (int)Models.Constants.FlagValues.AdditionalPayment))
                                { }
                                else
                                {
                                    switch (dataType)
                                    {
                                        case Models.Constants.ServiceFeeInterest:
                                            returnVal += paymentDetailIn[i].AccruedServiceFeeInterest;
                                            break;
                                        case Models.Constants.InterestAccrued:
                                            returnVal += paymentDetailIn[i].InterestAccrued;
                                            break;
                                        case Models.Constants.Principal:
                                            returnVal += (paymentDetailIn[i].Flags == (int)Models.Constants.FlagValues.Discount ||
                                                            paymentDetailIn[i].Flags == (int)Models.Constants.FlagValues.AdditionalPayment) ?
                                                                paymentDetailIn[i].PrincipalPaid : paymentDetailIn[i].PrincipalPayment;
                                            break;
                                    }
                                }
                            }
                        }
                        #endregion

                        #region This condition checks whether the payment date is less than the accrued interest date till which the amount is to be calculated.
                        else if (paymentDetailIn[i].PaymentDate < accruedInterestDate)
                        {
                            if (paymentDetailIn[i].Flags != (int)Models.Constants.FlagValues.NSFFee && paymentDetailIn[i].Flags != (int)Models.Constants.FlagValues.LateFee &&
                                paymentDetailIn[i].Flags != (int)Models.Constants.FlagValues.ManagementFee && paymentDetailIn[i].Flags != (int)Models.Constants.FlagValues.MaintenanceFee)
                            {
                                if (dataType == Models.Constants.Principal && paymentDetailIn[i].PrincipalPayment == 0 &&
                                    (paymentDetailIn[i].Flags != (int)Models.Constants.FlagValues.Discount &&
                                        paymentDetailIn[i].Flags != (int)Models.Constants.FlagValues.AdditionalPayment))
                                { }
                                else
                                {
                                    switch (dataType)
                                    {
                                        case Models.Constants.ServiceFeeInterest:
                                            previousAccumulated += paymentDetailIn[i].AccruedServiceFeeInterest;
                                            break;
                                        case Models.Constants.InterestAccrued:
                                            previousAccumulated += paymentDetailIn[i].InterestAccrued;
                                            break;
                                        case Models.Constants.Principal:
                                            previousAccumulated += (paymentDetailIn[i].Flags == (int)Models.Constants.FlagValues.Discount ||
                                                                        paymentDetailIn[i].Flags == (int)Models.Constants.FlagValues.AdditionalPayment) ?
                                                                            paymentDetailIn[i].PrincipalPaid : paymentDetailIn[i].PrincipalPayment;
                                            break;
                                    }
                                }
                                dateFrom = (dateFrom > paymentDetailIn[i].PaymentDate) ? dateFrom : paymentDetailIn[i].PaymentDate;
                            }
                        }
                        #endregion

                        #region This condition checks whether the payment date is greater than the accrued interest date till which the amount is to be calculated.
                        else if ((paymentDetailIn[i].PaymentDate > accruedInterestDate) && !isEqualToScheduledDate)
                        {
                            if (paymentDetailIn[i].Flags != (int)Models.Constants.FlagValues.NSFFee && paymentDetailIn[i].Flags != (int)Models.Constants.FlagValues.LateFee &&
                                paymentDetailIn[i].Flags != (int)Models.Constants.FlagValues.ManagementFee && paymentDetailIn[i].Flags != (int)Models.Constants.FlagValues.MaintenanceFee)
                            {
                                if (dataType == Models.Constants.Principal && paymentDetailIn[i].PrincipalPayment == 0 &&
                                    (paymentDetailIn[i].Flags != (int)Models.Constants.FlagValues.Discount &&
                                        paymentDetailIn[i].Flags != (int)Models.Constants.FlagValues.AdditionalPayment))
                                { }
                                else
                                {
                                    switch (dataType)
                                    {
                                        case Models.Constants.ServiceFeeInterest:
                                            nextAccumulated += Math.Round(paymentDetailIn[i].AccruedServiceFeeInterest - (i == 0 ? scheduleInputs.EarnedServiceFeeInterest : 0), 2, MidpointRounding.AwayFromZero);
                                            break;
                                        case Models.Constants.InterestAccrued:
                                            nextAccumulated += Math.Round(paymentDetailIn[i].InterestAccrued - (i == 0 ? scheduleInputs.EarnedInterest : 0), 2, MidpointRounding.AwayFromZero);
                                            break;
                                        case Models.Constants.Principal:
                                            nextAccumulated += (paymentDetailIn[i].Flags == (int)Models.Constants.FlagValues.Discount ||
                                                                    paymentDetailIn[i].Flags == (int)Models.Constants.FlagValues.AdditionalPayment) ?
                                                                        paymentDetailIn[i].PrincipalPaid : paymentDetailIn[i].PrincipalPayment;
                                            break;
                                    }
                                }
                                doPartialPeriod = true;
                                dateTo = paymentDetailIn[i].PaymentDate;
                                break;
                            }
                        }
                        #endregion
                    }
                    returnVal = returnVal + previousAccumulated + nextAccumulated;

                    //Calculate the amount if the date is between any two of the schedule dates of output grid.
                    if (doPartialPeriod)
                    {
                        //Counts the days betweent the last matched scheduled date and accrued interest date, to alculate the interest for these remaining days.
                        long dayCount = DateAndTime.DateDiff(DateInterval.Day, dateFrom, accruedInterestDate, FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1);
                        sbTracing.AppendLine("Inside ServiceFeeAndServiceFeeInterestPayable class, and CalculateServiceFeeAndInterestPayable() method. Calling method :  Business.DaysInPeriod.GetNumberOfDays(dateFrom, dateTo, scheduleInputs, false);");
                        long periodDayCount = Business.DaysInPeriod.GetNumberOfDays(dateFrom, dateTo, scheduleInputs, false, false);
                        if (scheduleInputs.DaysInMonth.ToLower() == Models.Constants.Periodic)
                        {
                            dayCount = dayCount > periodDayCount ? periodDayCount : dayCount;
                        }
                        returnVal = previousAccumulated + ((nextAccumulated) * dayCount / periodDayCount);

                        if (paymentDetailIn[0].PaymentDate > accruedInterestDate)
                        {
                            switch (dataType)
                            {
                                case Models.Constants.ServiceFeeInterest:
                                    returnVal += scheduleInputs.EarnedServiceFeeInterest;
                                    break;
                                case Models.Constants.InterestAccrued:
                                    returnVal += scheduleInputs.EarnedInterest;
                                    break;
                            }
                        }
                    }
                }
                returnVal += earnedFee;

                sbTracing.AppendLine("Exit:From Business Class ServiceFeeAndServiceFeeInterestPayable and method name GetAccruedValue(List<OutputGrid> paymentDetailIn, DateTime accruedInterestDate, DateTime effectiveDate, string dataType, LoanDetails scheduleInputs)");
                return returnVal;
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
                return 0;
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }

        /// <summary>
        /// This function calculates the total interest accrued amount till the provided date in the input field, by calling the GetAccruedValue() function.
        /// </summary>
        /// <param name="paymentDetailIn">Output grid where all repayment schedules get generated.</param>
        /// <param name="accruedInterestDate"></param>
        /// <param name="effectiveDate"></param>
        /// <param name="scheduleInputs"></param>
        /// <returns></returns>
        public static double GetInterestAccrued(List<OutputGrid> paymentDetailIn, DateTime accruedInterestDate, DateTime effectiveDate, LoanDetails scheduleInputs)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside Business Class ServiceFeeAndServiceFeeInterestPayable and method name GetInterestAccrued(List<OutputGrid> paymentDetailIn, DateTime accruedInterestDate, DateTime effectiveDate, LoanDetails scheduleInputs).");
                sbTracing.AppendLine("Parameter values are : accruedInterestDate = " + accruedInterestDate + ", effectiveDate = " + effectiveDate);

                double returnVal = GetAccruedValue(paymentDetailIn, accruedInterestDate, effectiveDate, Models.Constants.InterestAccrued, scheduleInputs);

                sbTracing.AppendLine("Exit:From Business Class ServiceFeeAndServiceFeeInterestPayable and method name GetInterestAccrued(List<OutputGrid> paymentDetailIn, DateTime accruedInterestDate, DateTime effectiveDate, LoanDetails scheduleInputs)");
                return returnVal;
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
                return 0;
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }

        /// <summary>
        /// This function calculates the total principal accrued amount till the provided date in the input field, by calling the GetAccruedValue() function.
        /// </summary>
        /// <param name="paymentDetailIn">Output grid where all repayment schedules get generated.</param>
        /// <param name="accruedInterestDate"></param>
        /// <param name="effectiveDate"></param>
        /// <param name="scheduleInputs"></param>
        /// <param name="loanAmountCalc"></param>
        /// <returns></returns>
        public static double GetAccruedPrincipal(List<OutputGrid> paymentDetailIn, DateTime accruedInterestDate, DateTime effectiveDate, LoanDetails scheduleInputs, double loanAmountCalc)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside Business Class ServiceFeeAndServiceFeeInterestPayable and method name GetAccruedPrincipal(List<OutputGrid> paymentDetailIn, DateTime accruedInterestDate, DateTime effectiveDate, LoanDetails scheduleInputs, double loanAmountCalc).");
                sbTracing.AppendLine("Parameter values are : accruedInterestDate = " + accruedInterestDate + ", effectiveDate = " + effectiveDate + ", loanAmountCalc = " + loanAmountCalc);

                double returnVal = GetAccruedValue(paymentDetailIn, accruedInterestDate, effectiveDate, Models.Constants.Principal, scheduleInputs);
                returnVal = (returnVal > loanAmountCalc) ? loanAmountCalc : returnVal;

                sbTracing.AppendLine("Exit:From Business Class ServiceFeeAndServiceFeeInterestPayable and method name GetAccruedPrincipal(List<OutputGrid> paymentDetailIn, DateTime accruedInterestDate, DateTime effectiveDate, " +
                    "LoanDetails scheduleInputs, double loanAmountCalc)");
                return returnVal;
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
                return 0;
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }
    }
}
