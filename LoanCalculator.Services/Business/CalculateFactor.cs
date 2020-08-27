using LoanAmort_driver.Models;
using System;
using System.Text;

namespace LoanAmort_driver.Business
{
    public class CalculateFactor
    {
        /// <summary>
        /// Name : Sparsh Agarwal
        /// Date : 20/03/2017
        /// User story : 1. Define the number of days in a month (DiM) and User story : 2. Define Number of days in a year (DiY)
        /// Explanation : This function will calculate the factor and periodic interest rate, and will return the periodic interest rate. This method combines the user
        /// story 1 and 2. Here the conditions for days in month and days in year are taken to determine whether it is a wrong combination, or to use leap year 
        /// calculation, or to use fixed or actual method to calculate the factor. Then it will calculate and return the periodic interest rate.
        /// </summary>
        /// <param name="daysInYear"></param>
        /// <param name="daysInMonth"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="interestDelay"></param>
        /// <param name="loanTakenDate"></param>
        /// <returns></returns>
        public static double GetFactorValue(DateTime startDate, DateTime endDate, DateTime loanTakenDate, int interestDelay, bool isScheduleInput, LoanDetails scheduleInput, bool isForTotalPayment)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                double factor;
                string daysInYear = scheduleInput.DaysInYearBankMethod;
                string daysInMonth = ((scheduleInput.InputRecords[1].EffectiveDate == default(DateTime) ? scheduleInput.InputRecords[1].DateIn : scheduleInput.InputRecords[1].EffectiveDate) >= endDate && !isForTotalPayment && scheduleInput.MinDuration > Constants.MinDurationValue) ? Constants.Actual : scheduleInput.DaysInMonth;

                sbTracing.AppendLine("Enter: Inside Class CalculateFactor and method Name GetFactorValue(DateTime startDate, DateTime endDate, DateTime loanTakenDate, int interestDelay, bool isScheduleInput, LoanDetails scheduleInput).");
                sbTracing.AppendLine("Parameters are : " + ", startDate = " + startDate + ", endDate = " + endDate + ", loanTakenDate = " + loanTakenDate + ", interestDelay = " + interestDelay + ", isScheduleInput = " + isScheduleInput);

                startDate = loanTakenDate.AddDays(interestDelay) >= startDate ? loanTakenDate.AddDays(interestDelay) : startDate;
                startDate = startDate > endDate ? endDate : startDate;

                //This condition will throw an error as when no. of days in month is 30, then days in year must be a numeric value.
                if ((daysInMonth.ToLower() == Constants.Periodic || daysInMonth == Constants.Thirty) && daysInYear.ToLower() == Constants.Actual)
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
                    //factor = Math.Round(((double)leapYearDays / 366) + ((double)nonleapYearDays / 365), 15, MidpointRounding.AwayFromZero);
                    factor = ((double)leapYearDays / 366) + ((double)nonleapYearDays / 365);
                }
                //This is the condition where no. of days in year is a numeric value, irrespective of no. of days in month.
                else
                {
                    sbTracing.AppendLine("Inside CalculateFactor class, and GetFactorValue() method. Calling method :  DaysInPeriod.GetNumberOfDays(startDate, endDate, scheduleInput, isScheduleInput);");
                    int days = DaysInPeriod.GetNumberOfDays(startDate, endDate, scheduleInput, isScheduleInput, isForTotalPayment);
                    //factor = Math.Round(((double)days / Convert.ToInt32(daysInYear)), 15, MidpointRounding.AwayFromZero);
                    factor = (double)days / Convert.ToInt32(daysInYear);
                }

                sbTracing.AppendLine("Exit:From Class CalculateFactor and method Name GetFactorValue(string daysInYear, string daysInMonth, DateTime startDate, DateTime endDate, DateTime loanTakenDate, int interestDelay)");
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
        /// Name : Sparsh Agarwal
        /// Date : 20/03/2017
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
        public static double GetFactorValue(DateTime startDate, DateTime endDate, bool isScheduleInput, LoanDetails scheduleInput, bool isForTotalPayment)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                double factor;
                string daysInYear = scheduleInput.DaysInYearBankMethod;
                string daysInMonth = ((scheduleInput.InputRecords[1].EffectiveDate == default(DateTime) ? scheduleInput.InputRecords[1].DateIn : scheduleInput.InputRecords[1].EffectiveDate) >= endDate && !isForTotalPayment && scheduleInput.MinDuration > Constants.MinDurationValue) ? Constants.Actual : scheduleInput.DaysInMonth;
                sbTracing.AppendLine("Enter: Inside Class CalculateFactor and method Name GetFactorValue( DateTime startDate, DateTime endDate, bool isScheduleInput, LoanDetails scheduleInput).");
                sbTracing.AppendLine("Parameters are : " + ", startDate = " + startDate + ", endDate = " + endDate + ", isScheduleInput = " + isScheduleInput);

                //This condition will throw an error as when no. of days in month is 30, then days in year must be a numeric value.
                if ((daysInMonth.ToLower() == Constants.Periodic || daysInMonth == Constants.Thirty) && daysInYear.ToLower() == Constants.Actual)
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
                    //factor = Math.Round(((double)leapYearDays / 366) + ((double)nonleapYearDays / 365), 15, MidpointRounding.AwayFromZero);
                    factor = ((double)leapYearDays / 366) + ((double)nonleapYearDays / 365);
                }
                //This is the condition where no. of days in year is a numeric value, irrespective of no. of days in month.
                else
                {
                    sbTracing.AppendLine("Inside CalculateFactor class, and GetFactorValue() method. Calling method : DaysInPeriod.GetNumberOfDays(startDate, endDate, scheduleInput, isScheduleInput);");
                    int days = DaysInPeriod.GetNumberOfDays(startDate, endDate, scheduleInput, isScheduleInput, isForTotalPayment);
                    //factor = Math.Round(((double)days / Convert.ToInt32(daysInYear)), 15, MidpointRounding.AwayFromZero);
                    factor = (double)days / Convert.ToInt32(daysInYear);
                }

                sbTracing.AppendLine("Exit:From Class CalculateFactor and method Name GetFactorValue(string daysInYear, string daysInMonth, DateTime startDate, DateTime endDate)");
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
