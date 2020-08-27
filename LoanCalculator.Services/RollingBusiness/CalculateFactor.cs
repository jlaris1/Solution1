using LoanAmort_driver.Models;
using System;
using System.Text;

namespace LoanAmort_driver.RollingBusiness
{
    public class CalculateFactor
    {
        /// <summary>
        /// Name :Asad Anwar
        /// Date : 15/05/2018
        /// User story : 1. Define the number of days in a month (DiM) and User story : 2. Define Number of days in a year (DiY)
        /// Explanation : This function will calculate the factor and periodic interest rate, and will return the periodic interest rate. This method combines the user
        /// story 1 and 2. Here the conditions for days in month and days in year are taken to determine whether it is a wrong combination, or to use leap year 
        /// calculation, or to use fixed or actual method to calculate the factor. Then it will calculate and return the periodic interest rate.
        /// Also Interest delay functionality is implemented here.
        /// </summary>
        /// <param name="daysInYear"></param>
        /// <param name="daysInMonth"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="loanTakenDate"></param>
        /// <param name="interestDelay"></param>
        /// <returns></returns>
        public static double GetFactorValue(DateTime startDate, DateTime endDate, DateTime loanTakenDate, int interestDelay, getScheduleInput scheduleInput, bool isScheduleInput, bool isForTotalPayment)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                double factor;

                string daysInYear = scheduleInput.DaysInYearBankMethod;
                string daysInMonth = scheduleInput.DaysInMonth;
                sbTracing.AppendLine("Enter: Inside CalculateFactor Class file and method Name GetFactorValue(DateTime startDate, DateTime endDate, DateTime loanTakenDate, int interestDelay, getScheduleInput scheduleInput, bool isScheduleInput, bool isForTotalPayment).");
                sbTracing.AppendLine("Parameters are : " + "daysInYear = " + daysInYear + ", daysInMonth = " + daysInMonth + ", startDate = " + startDate + ", endDate = " + endDate + ", loanTakenDate = " + loanTakenDate + ", interestDelay = " + interestDelay + ",isForTotalPayment =" + isForTotalPayment);

                startDate = (loanTakenDate.AddDays(interestDelay) >= startDate ? loanTakenDate.AddDays(interestDelay) : startDate);
                startDate = (startDate > endDate) ? endDate : startDate;

                //This condition will throw an error as when no. of days in month is 30, then days in year must be a numeric value.
                if (daysInMonth ==Constants.Thirty  && daysInYear.ToLower() == Constants.Actual)
                {
                    throw new ArgumentException("Invalid Days in Year Value");
                }

                // We determine dates may be either under non-leap year or leap year to calculate factor.
                if (daysInMonth.ToLower() == Constants.Actual && daysInYear.ToLower() == Constants.Actual)
                {
                    int nonleapYearDays;
                    int leapYearDays;
                    sbTracing.AppendLine("Inside CalculateFactor class, and GetFactorValue() method. Calling method : LeapYearAndNonLeapYearDays.GetLeapAndNonLeapYearDays(startDate, endDate, out nonleapYearDays, out leapYearDays);");
                    LeapYearAndNonLeapYearDays.GetLeapAndNonLeapYearDays(startDate, endDate, out nonleapYearDays, out leapYearDays);
                    factor = ((double)leapYearDays / 366) + ((double)nonleapYearDays / 365);
                }
                //This is the condition where no. of days in year is a numeric value, irrespective of no. of days in month.
                else
                {
                    sbTracing.AppendLine("Inside CalculateFactor class, and GetFactorValue() method. Calling method : DaysInPeriod.GetNumberOfDays(daysInYear, daysInMonth, startDate, endDate, isForTotalPayment);");
                    int days = DaysInPeriod.GetNumberOfDays(startDate, endDate, scheduleInput, isScheduleInput, isForTotalPayment);
                    factor = (double)days / Convert.ToInt32(daysInYear);
                }

                sbTracing.AppendLine("Exit:From CalculateFactor Class file and method Name GetFactorValue(DateTime startDate, DateTime endDate, DateTime loanTakenDate, int interestDelay, LoanDetails scheduleInput, bool isScheduleInput)");
                return factor;
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
        /// Name : Asad Avwar
        /// Date : 15/05/2018
        /// User story : 1. Define the number of days in a month (DiM) and User story : 2. Define Number of days in a year (DiY)
        /// Explanation : This function will calculate the factor and periodic interest rate, and will return the periodic interest rate. This method combines the user
        /// story 1 and 2. Here the conditions for days in month and days in year are taken to determine whether it is a wrong combination, or to use leap year 
        /// calculation, or to use fixed or actual method to calculate the factor. Then it will calculate and return the periodic interest rate.
        /// </summary>
        /// <param name="daysInYear"></param>
        /// <param name="daysInMonth"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static double GetFactorValue(DateTime startDate, DateTime endDate, getScheduleInput scheduleInput, bool isScheduleInput, bool isForTotalPayment)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                double factor;
                string daysInYear = scheduleInput.DaysInYearBankMethod;
                string daysInMonth = scheduleInput.DaysInMonth;

                sbTracing.AppendLine("Enter: Inside CalculateFactor Class file and method Name GetFactorValue(DateTime startDate, DateTime endDate, getScheduleInput scheduleInput, bool isScheduleInput, bool isForTotalPayment).");
                sbTracing.AppendLine("Parameters are : " + "daysInYear = " + daysInYear + ", daysInMonth = " + daysInMonth + ", startDate = " + startDate + ", endDate = " + endDate + ", isForTotalPayment=" + isForTotalPayment);

                //This condition will throw an error as when no. of days in month is 30, then days in year must be a numeric value.
                if (daysInMonth == Constants.Thirty && daysInYear.ToLower() == Constants.Actual)
                {
                    throw new ArgumentException("Invalid Days in Year Value");
                }

                // We determine dates may be either under non-leap year or leap year to calculate factor.
                if (daysInMonth.ToLower() == Constants.Actual && daysInYear.ToLower() == Constants.Actual)
                {
                    int nonleapYearDays;
                    int leapYearDays;
                    sbTracing.AppendLine("Inside CalculateFactor class, and GetFactorValue() method. Calling method : LeapYearAndNonLeapYearDays.GetLeapAndNonLeapYearDays(startDate, endDate, out nonleapYearDays, out leapYearDays);");
                    LeapYearAndNonLeapYearDays.GetLeapAndNonLeapYearDays(startDate, endDate, out nonleapYearDays, out leapYearDays);
                    factor = ((double)leapYearDays / 366) + ((double)nonleapYearDays / 365);
                }
                //This is the condition where no. of days in year is a numeric value, irrespective of no. of days in month.
                else
                {
                    sbTracing.AppendLine("Inside CalculateFactor class, and GetFactorValue() method. Calling method : DaysInPeriod.GetNumberOfDays(daysInYear, daysInMonth, startDate, endDate, isForTotalPayment);");
                    int days = DaysInPeriod.GetNumberOfDays(startDate, endDate, scheduleInput, isScheduleInput, isForTotalPayment);
                    factor = (double)days / Convert.ToInt32(daysInYear);
                }

                sbTracing.AppendLine("Exit:From CalculateFactor Class file and method Name GetFactorValue(DateTime startDate, DateTime endDate, getScheduleInput scheduleInput, bool isScheduleInput)");
                return factor;
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
