using System;
using System.Text;

namespace LoanAmort_driver.RollingBusiness
{
    public class LeapYearAndNonLeapYearDays
    {
        /// <summary>
        /// Name : Adesh Kumar
        /// Date : 20/03/2017
        /// User story : 2. Define Number of days in a year (DiY)
        /// Explanation : Basically this function is a part of user story 2. This function calculates the no. of days in non-leap year as well as in leap year. This
        /// function is called when days in month and days in year both are actual.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="nonLeapYearDays"></param>
        /// <param name="leapYearDays"></param>
        public static void GetLeapAndNonLeapYearDays(DateTime startDate, DateTime endDate, out int nonLeapYearDays, out int leapYearDays)
        {
            nonLeapYearDays = 0;
            leapYearDays = 0;
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class LeapYearAndNonLeapYearDays and method name GetLeapAndNonLeapYearDays(DateTime startDate, DateTime endDate, out int nonLeapYearDays, out int leapYearDays).");
                sbTracing.AppendLine("Parameter values are : startDate = " + startDate + ", endDate = " + endDate + ", nonLeapYearDays = " + nonLeapYearDays + ", leapYearDays = " + leapYearDays);
                if (startDate.Year == endDate.Year) //This condition checks if both dates are in same year, then days will lay either in leap year or in non-leap year.
                {
                    if ((startDate.Year) % 4 == 0)
                    {
                        //This condition checks whether the year is leap year, then it will calculate no. of days in leap year.
                        leapYearDays = (endDate - startDate).Days;
                    }
                    else
                    {
                        //Otherwise no. of days will be in non-leap year.
                        nonLeapYearDays = (endDate - startDate).Days;
                    }
                }
                else    //When year of both dates are not same.
                {
                    if ((startDate.Year) % 4 == 0) //It will check if start date year is in leap year, if yes then subtract start date from last date of year, and no. of days will be part of leap year.

                    {
                        leapYearDays = (Convert.ToDateTime("12/31/" + startDate.Year) - startDate).Days;
                    }
                    else
                    {
                        //Otherwise, no. of days will be part of non-leap year
                        nonLeapYearDays = (Convert.ToDateTime("12/31/" + startDate.Year) - startDate).Days;
                    }
                    if ((endDate.Year) % 4 == 0)    //It will check if end date year is in leap year, if yes then subtract last date of start date year from end date, and add no. of days in leap year.
                    {
                        leapYearDays += (endDate - Convert.ToDateTime("12/31/" + startDate.Year)).Days;
                    }
                    else
                    {
                        //Otherwise, no. of days will be added to non-leap year
                        nonLeapYearDays += (endDate - Convert.ToDateTime("12/31/" + startDate.Year)).Days;
                    }
                }
                sbTracing.AppendLine("Exist:From class LeapYearAndNonLeapYearDays and method name GetLeapAndNonLeapYearDays(DateTime startDate, DateTime endDate, out int nonLeapYearDays, out int leapYearDays)");
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }
    }
}
