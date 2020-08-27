﻿using LoanAmort_driver.Models;
using System;
using System.Text;

namespace LoanAmort_driver.Business
{
    public class DailyInterestAmount
    {
        /// <summary>
        /// Name : Sparsh Agarwal
        /// Date : 20/03/2017
        /// User Story : 35. PROVIDE AN OUTPUT VALUE FOR DAILY INTEREST AMOUNT
        /// Explanation : Calculate the daily interest amount as periodic interest amount divided by the days in the period. This value will be the output of the output
        /// grid. Here first we use periodic interest amount as a parameter, then calculate the days in the period then divide periodic interest amount with number of 
        /// days to calculate the daily interest amount.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="periodicInterestAmount"></param>
        /// <param name="interestDelay"></param>
        /// <param name="loanTakenDate"></param>
        /// <returns></returns>
        public static double CalculateDailyInterestAmount(DateTime startDate, DateTime endDate, double periodicInterestAmount, DateTime loanTakenDate, int interestDelay, LoanDetails scheduleInput, bool isScheduleInput, bool isForTotalPayment)
        {
            StringBuilder sbTracing = new StringBuilder();
            double dailyInterestAmount = 0;
            try
            {
                sbTracing.AppendLine("Enter:Inside DailyInterestAmount class and Method Name CalculateDailyInterestAmount(DateTime startDate, DateTime endDate, double periodicInterestAmount, DateTime loanTakenDate, int interestDelay, LoanDetails scheduleInput, bool isScheduleInput).");
                sbTracing.AppendLine("Parameter values are : startDate = " + startDate + ", endDate = " + endDate + ", isScheduleInput " + isScheduleInput);

                startDate = (loanTakenDate.AddDays(interestDelay) >= startDate ? loanTakenDate.AddDays(interestDelay) : startDate);
                startDate = (startDate > endDate) ? endDate : startDate;

                sbTracing.AppendLine("Inside DailyInterestAmount class, and DailyInterestAmount() method. Calling method :  DaysInPeriod.GetNumberOfDays(startDate, endDate,scheduleInput, isScheduleInput);");
                int days = DaysInPeriod.GetNumberOfDays(startDate, endDate, scheduleInput, isScheduleInput, isForTotalPayment);
                dailyInterestAmount = days == 0 ? 0 : (periodicInterestAmount / days);

                sbTracing.AppendLine("Exit:From DailyInterestAmount class and Method Name CalculateDailyInterestAmount(DateTime startDate, DateTime endDate, double periodicInterestAmount, DateTime loanTakenDate, int interestDelay)");
                return dailyInterestAmount;
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
                return dailyInterestAmount;
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }
    }
}