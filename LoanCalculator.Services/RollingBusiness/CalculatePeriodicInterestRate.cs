using System;
using System.Text;
using LoanAmort_driver.Models;

namespace LoanAmort_driver.RollingBusiness
{
    public class CalculatePeriodicInterestRate
    {
        /// <summary>
        /// Name : Punit Singh
        /// Date : 29/07/2018
        /// This function calculates the periodic interest rate with the functionality of interest tiers.
        /// </summary>
        /// <param name="scheduleInput"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="loanTakenDate"></param>
        /// <param name="remainingPrincipal"></param>
        /// <param name="IsInterestDelay"></param>
        /// <returns></returns>
        public static double CalcPeriodicInterestRateWithTier(getScheduleInput scheduleInput, DateTime startDate, DateTime endDate, DateTime loanTakenDate, double remainingPrincipal, bool IsInterestDelay, bool isScheduleInput, bool isForTotalPayment)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                #region Calculate Periodic interest rate using tier values.

                sbTracing.AppendLine("Enter: Inside CalculatePeriodicInterestRate Class file and method Name CalcPeriodicInterestRateWithTier(getScheduleInput scheduleInput, DateTime startDate, DateTime endDate, DateTime loanTakenDate, double remainingPrincipal, bool IsInterestDelay,bool isScheduleInput, bool isForTotalPayment).");
                sbTracing.AppendLine("Parameters are : " + "scheduleInput=" + scheduleInput + ",startDate = " + startDate + ", endDate = " + endDate + ", loanTakenDate = " + loanTakenDate + ", remainingPrincipal = " + remainingPrincipal + ", IsInterestDelay = " + IsInterestDelay + ",isScheduleInput =" + isScheduleInput + ",isForTotalPayment=" + isForTotalPayment);

                double periodicInterestRate = 0;
                double factor = IsInterestDelay ? CalculateFactor.GetFactorValue(startDate, endDate, loanTakenDate, scheduleInput.InterestDelay, scheduleInput, isScheduleInput, isForTotalPayment) :
                                                  CalculateFactor.GetFactorValue(startDate, endDate, scheduleInput, isScheduleInput, isForTotalPayment);

                periodicInterestRate = factor * .01 * Convert.ToDouble(scheduleInput.InterestRate);
                if (!string.IsNullOrEmpty(Convert.ToString(scheduleInput.InterestRate2)) && scheduleInput.Tier1 != 0 && remainingPrincipal > scheduleInput.Tier1)
                {
                    periodicInterestRate += factor * .01 * Convert.ToDouble(scheduleInput.InterestRate2);
                }

                if (!string.IsNullOrEmpty(Convert.ToString(scheduleInput.InterestRate3)) && scheduleInput.Tier2 != 0 && remainingPrincipal > scheduleInput.Tier2)
                {
                    periodicInterestRate += factor * .01 * Convert.ToDouble(scheduleInput.InterestRate3);
                }

                if (!string.IsNullOrEmpty(Convert.ToString(scheduleInput.InterestRate4)) && scheduleInput.Tier3 != 0 && remainingPrincipal > scheduleInput.Tier3)
                {
                    periodicInterestRate += factor * .01 * Convert.ToDouble(scheduleInput.InterestRate4);
                }

                periodicInterestRate = Math.Round(periodicInterestRate, 15, MidpointRounding.AwayFromZero);
                sbTracing.AppendLine("Exit:From CalculatePeriodicInterestRate Class file and method Name CalcPeriodicInterestRateWithTier(getScheduleInput scheduleInput, DateTime startDate, DateTime endDate, DateTime loanTakenDate, double remainingPrincipal, bool IsInterestDelay,bool isScheduleInput)");
                return periodicInterestRate;

                #endregion
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
        /// This function calculates the periodic interest rate withuot the functionality of interest tiers.
        /// </summary>
        /// <param name="scheduleInput"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="annualInterestRate"></param>
        /// <param name="loanTakenDate"></param>
        /// <param name="IsInterestDelay"></param>
        /// <returns></returns>
        public static double CalcPeriodicInterestRateWithoutTier(getScheduleInput scheduleInput, DateTime startDate, DateTime endDate, double annualInterestRate, DateTime loanTakenDate, bool IsInterestDelay, bool isScheduleInput, bool isForTotalPayment)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                #region Calculating Periodic interest rate without tier values.

                sbTracing.AppendLine("Enter: Inside CalculatePeriodicInterestRate Class file and method Name CalcPeriodicInterestRateWithoutTier(getScheduleInput scheduleInput, DateTime startDate, DateTime endDate, double annualInterestRate, DateTime loanTakenDate, bool IsInterestDelay,bool isScheduleInput, bool isForTotalPayment).");
                sbTracing.AppendLine("Parameters are : " + "scheduleInput=" + scheduleInput + ",startDate = " + startDate + ", endDate = " + endDate + ",annualInterestRate =" + annualInterestRate + ", loanTakenDate = " + loanTakenDate + ", IsInterestDelay = " + IsInterestDelay + ", isScheduleInput =" + isScheduleInput + ", isForTotalPayment=" + isForTotalPayment);

                double periodicInterestRate = 0;
                double factor = IsInterestDelay ? CalculateFactor.GetFactorValue(startDate, endDate, loanTakenDate, scheduleInput.InterestDelay, scheduleInput, isScheduleInput, isForTotalPayment) :
                                                  CalculateFactor.GetFactorValue(startDate, endDate, scheduleInput, isScheduleInput, isForTotalPayment);

                periodicInterestRate = factor * .01 * annualInterestRate;
                periodicInterestRate = Math.Round(periodicInterestRate, 15, MidpointRounding.AwayFromZero);
                sbTracing.AppendLine("Exit:From PeriodicInterestRate Class file and method Name CalcPeriodicInterestRateWithoutTier(getScheduleInput scheduleInput, DateTime startDate, DateTime endDate, double annualInterestRate, DateTime loanTakenDate, bool IsInterestDelay,bool isScheduleInput)");
                return periodicInterestRate;

                #endregion
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
