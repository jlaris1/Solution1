using LoanAmort_driver.Models;
using System;
using System.Text;

namespace LoanAmort_driver.RollingBusiness
{
    public class DaysInPeriod
    {
        /// <summary>
        /// Name : Adesh Kumar
        /// Date : 20/03/2017
        /// User story : 1. Define the number of days in a month (DiM) and User story : 2. Define Number of days in a year (DiY)
        /// Explanation : This function is a part of user story 1 and 2. Here this method calculates the number of days in certain period on the basis of values in the 
        /// days in month and days in year dropdown.
        /// </summary>
        /// <param name="daysInYear"></param>
        /// <param name="daysInMonth"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static int GetNumberOfDays(DateTime startDate, DateTime endDate, getScheduleInput scheduleInput, bool isScheduleInput, bool isForTotalPayment)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside DaysInPeriod class and Method Name GetNumberOfDays(DateTime startDate, DateTime endDate, getScheduleInput scheduleInput, bool isScheduleInput, bool isForTotalPayment).");
                sbTracing.AppendLine("Parameter values are : startDate = " + startDate + ", endDate = " + endDate + ", isScheduleInput" + isScheduleInput);

                string daysInYear = scheduleInput.DaysInYearBankMethod;
                string daysInMonth = scheduleInput.DaysInMonth;

                //This condition will throw an error as when no. of days in month is 30, then days in year must be a numeric value.
                if ((daysInMonth.ToLower() == Constants.Periodic || daysInMonth == Constants.Thirty) && daysInYear.ToLower() == Constants.Actual)
                {
                    throw new ArgumentException("Invalid Days in Year Value");
                }

                int days;
                // We determine dates may be either under non-leap year or leap year to calculate factor.
                if (daysInMonth.ToLower() == Constants.Actual && daysInYear.ToLower() == Constants.Actual)
                {
                    int nonLeapYearDays;
                    int leapYearDays;
                    sbTracing.AppendLine("Inside DaysInPeriod class, and GetNumberOfDays() method. Calling method : LeapYearAndNonLeapYearDays.GetLeapAndNonLeapYearDays(startDate, endDate, out nonLeapYearDays, out leapYearDays);");
                    LeapYearAndNonLeapYearDays.GetLeapAndNonLeapYearDays(startDate, endDate, out nonLeapYearDays, out leapYearDays);
                    days = nonLeapYearDays + leapYearDays;
                }
                //This is the condition where no. of days in year is a numeric value, irrespective of no. of days in month.
                else
                {
                    if (scheduleInput.MinDuration > Models.Constants.MinDurationValue && !isForTotalPayment && (endDate <= (scheduleInput.InputRecords[1].EffectiveDate == DateTime.MinValue ? scheduleInput.InputRecords[1].DateIn : scheduleInput.InputRecords[1].EffectiveDate)))//this condition is used for Prorate method
                    {
                        days = (endDate - startDate).Days;
                    }
                    else if(daysInMonth == Constants.Thirty)    //In this condition factor will be evaluting as per fixed method.
                    {
                        days = (Convert.ToInt32(daysInYear) * (endDate.Year - startDate.Year)) + (30 * (endDate.Month - startDate.Month)) +
                                                (GetDate(endDate) - GetDate(startDate));
                    }
                    else if (daysInMonth.ToLower() == Constants.Periodic)   //In this condition factor will be evaluating as per selected Payment Period Frequency.
                    {
                        days = NumberOfDaysInPeriod(startDate, endDate, scheduleInput, isScheduleInput, isForTotalPayment);
                    }
                    else //In this condition factor will be evaluating as per actual method.
                    {
                        days = (endDate - startDate).Days;
                    }
                }
                sbTracing.AppendLine("Exist:From DaysInPeriod class and Method Name GetNumberOfDays(DateTime startDate, DateTime endDate, getScheduleInput scheduleInput, bool isScheduleInput, bool isForTotalPayment).");
                return days;
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
        /// This function calculates the number of days as per the value of date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private static int GetDate(DateTime date)
        {
            int days = date.Day;
            int month = date.Month;
            //This condition checks whether the month has 31 days. Then it will add 1 day in number of total days.
            if ((month == 1 || month == 3 || month == 5 || month == 7 || month == 8 || month == 10 || month == 12) && days == 31)
            {
                days = 30;
            }
            else if (month == 2)
            {
                if ((date.Year % 4 == 0) && days == 29)
                {
                    days = 30;
                }
                else if ((date.Year % 4 != 0) && days == 28)
                {
                    days = 30;
                }
            }
            return days;
        }

        /// <summary>
        /// Determine the Number of days in the payment period.
        /// </summary>
        /// <param name="paymentPeriod"></param>
        /// <returns></returns>
        public static short DaysInPaymentPeriod(short paymentPeriod)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class CalculateTotalAmount and method name daysInPeriod(short paymentPeriod).");
                sbTracing.AppendLine("Parameter values are : paymentPeriod = " + paymentPeriod);

                short days;
                switch (paymentPeriod)
                {
                    case (int)PaymentPeriod.Monthly:
                        days = 30;
                        break;
                    case (int)PaymentPeriod.SemiMonthly:
                        days = 15;
                        break;
                    case (int)PaymentPeriod.BiWeekly:
                        days = 14;
                        break;
                    case (int)PaymentPeriod.Weekly:
                        days = 7;
                        break;
                    default:
                        days = 1;
                        break;
                }
                sbTracing.AppendLine("Exist:From method name daysInPeriod(short paymentPeriod)");
                return days;
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
        /// Determine the number of days in a period.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="scheduleInput"></param>
        /// <param name="isScheduleInput"></param>
        /// <param name="isForTotalPayment"></param>
        /// <returns></returns>
        private static int NumberOfDaysInPeriod(DateTime startDate, DateTime endDate, getScheduleInput scheduleInput, bool isScheduleInput, bool isForTotalPayment)
        {
            /*
                     109 - USER STORY: Add periodic rate for new loan methods 
                     110 -  USER STORY: Set Interest Max to Periodic rate within a scheduled
                     */
            int days;
            short DaysPerPeriod = DaysInPaymentPeriod((short)scheduleInput.PmtPeriod);
            if (endDate > (scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].EffectiveDate == DateTime.MinValue ?
                                          scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].DateIn :
                                          scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].EffectiveDate))
            {
                days = DaysPerPeriod > (endDate - startDate).Days ? (endDate - startDate).Days : DaysPerPeriod;
            }
            else
            {
                int LastScheduleInputIndex = 0;
                int InitialDaysDiff = 0;
                if (isForTotalPayment)
                {
                    LastScheduleInputIndex = scheduleInput.InputRecords.FindLastIndex(o => o.DateIn < endDate);
                    InitialDaysDiff = (DaysPerPeriod - (startDate - scheduleInput.InputRecords[LastScheduleInputIndex].DateIn).Days);
                }
                else
                {
                    LastScheduleInputIndex = scheduleInput.InputRecords.FindIndex(o => o.DateIn == endDate || o.EffectiveDate == endDate) - 1;
                    if (LastScheduleInputIndex < 0)
                    {
                        for (int i = 0; i < scheduleInput.InputRecords.Count; i++)
                        {
                            if ((scheduleInput.InputRecords[i].EffectiveDate != DateTime.MinValue && scheduleInput.InputRecords[i].EffectiveDate > endDate) ||
                                (scheduleInput.InputRecords[i].EffectiveDate == DateTime.MinValue && scheduleInput.InputRecords[i].DateIn > endDate))
                            {
                                LastScheduleInputIndex = i - 1;
                                break;
                            }
                        }
                    }

                    InitialDaysDiff = (DaysPerPeriod - (startDate - (scheduleInput.InputRecords[LastScheduleInputIndex].EffectiveDate == DateTime.MinValue ?
                                                                            scheduleInput.InputRecords[LastScheduleInputIndex].DateIn :
                                                                            scheduleInput.InputRecords[LastScheduleInputIndex].EffectiveDate)).Days);
                }

                InitialDaysDiff = InitialDaysDiff <= 0 ? 0 : InitialDaysDiff;
                days = InitialDaysDiff >= (endDate - startDate).Days ? (endDate - startDate).Days : InitialDaysDiff;

                if (isScheduleInput) //&& scheduleInput.InputRecords[0].DateIn.AddDays(scheduleInput.InterestDelay) <= endDate)
                {
                    days = InitialDaysDiff;
                }
            }
            return days;
        }

    }
}
