using LoanAmort_driver.Models;
using System;
using System.Text;

namespace LoanAmort_driver.RollingBusiness
{
    public class DailyInterestRate
    {
        /// <summary>
        /// Name : Sparsh Agarwal
        /// Date : 20/03/2017
        /// User Story : 34. PROVIDE AN OUTPUT VALUE FOR DAILY INTEREST RATE
        /// Explanation : Calculate the daily interest rate as periodic interest rate divided by the days in the period. This value will be the output of the output grid.
        /// Here first we use periodic interest rate as a parameter, then calculate the days in the period then divide periodic interest rate with number of days to
        /// calculate the daily interest rate.
        /// </summary>
        /// <param name="daysInYear"></param>
        /// <param name="daysInMonth"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="periodicInterestRate"></param>
        /// <param name="interestDelay"></param>
        /// <param name="loanTakenDate"></param>
        /// <returns></returns>
        public static double CalculateDailyInterestRate(string daysInYear, string daysInMonth, DateTime startDate, DateTime endDate, double periodicInterestRate, DateTime loanTakenDate, int interestDelay, getScheduleInput scheduleInput, bool isScheduleInput)
        {
            StringBuilder sbTracing = new StringBuilder();
            double dailyInterestRate = 0;
            try
            {
                sbTracing.AppendLine("Enter:Inside DailyInterestRate class and Method Name CalculateDailyInterestRate(string daysInYear, string daysInMonth, DateTime startDate, DateTime endDate, double periodicInterestRate, DateTime loanTakenDate, int interestDelay, getScheduleInput scheduleInput, bool isScheduleInput).");
                sbTracing.AppendLine("Parameter values are : daysInYear = " + daysInYear + ", daysInMonth = " + daysInMonth + ", startDate = " + startDate + ", endDate = " + endDate + ", loanTakenDate = " + loanTakenDate + ", interestDelay = " + interestDelay);

                startDate = (loanTakenDate.AddDays(interestDelay) >= startDate ? loanTakenDate.AddDays(interestDelay) : startDate);
                startDate = (startDate > endDate) ? endDate : startDate;

                sbTracing.AppendLine("Inside DailyInterestRate class, and CalculateDailyInterestRate() method. Calling method : DaysInPeriod.GetNumberOfDays(daysInYear, daysInMonth, startDate, endDate,false);");
                int days = DaysInPeriod.GetNumberOfDays(startDate, endDate,scheduleInput,isScheduleInput,false);
                dailyInterestRate = days == 0 ? 0 : Math.Round((periodicInterestRate / days), 15, MidpointRounding.AwayFromZero);

                sbTracing.AppendLine("Exit:From DailyInterestRate class and Method Name CalculateDailyInterestRate(string daysInYear, string daysInMonth, DateTime startDate, DateTime endDate, double periodicInterestRate, DateTime loanTakenDate, int interestDelay, getScheduleInput scheduleInput, bool isScheduleInput)");
                return dailyInterestRate;
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
                return dailyInterestRate;
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }
    }
}
