using System;
using System.Text;
using LoanAmort_driver.Models;

namespace LoanAmort_driver.RollingBusiness
{
    public class PeriodicInterestAmount
    {
        /// <summary>
        /// Name :Asad Anwar
        /// Date : 10/05/2018
        /// This function calculate the interest amount to be paid in the schedule using interest tiers.
        /// </summary>
        /// <param name="scheduleInput"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="loanTakenDate"></param>
        /// <param name="beginingPrincipal"></param>
        /// <param name="withResidual"></param>
        /// <returns></returns>
        public static double CalcAccruedInterestWithTier(getScheduleInput scheduleInput, DateTime startDate, DateTime endDate, DateTime loanTakenDate, double remainingPrincipal, bool IsInterestDelay, bool isScheduleInput, bool isForTotalPayment)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                #region Calculating accrued interest rate using tier values.

                sbTracing.AppendLine("Enter: Inside PeriodicInterestAmount Class file and method Name CalcAccruedInterestWithTier(getScheduleInput scheduleInput, DateTime startDate, DateTime endDate, DateTime loanTakenDate, double remainingPrincipal, bool IsInterestDelay,bool isScheduleInput, bool isForTotalPayment).");
                sbTracing.AppendLine("Parameters are : " + "scheduleInput=" + scheduleInput + ",startDate = " + startDate + ", endDate = " + endDate + ", loanTakenDate = " + loanTakenDate + ", remainingPrincipal = " + remainingPrincipal + ", IsInterestDelay = " + IsInterestDelay + ", isScheduleInput =" + isScheduleInput + ",isForTotalPayment=" + isForTotalPayment);

                double factor = IsInterestDelay ? CalculateFactor.GetFactorValue(startDate, endDate, loanTakenDate, scheduleInput.InterestDelay, scheduleInput, isScheduleInput, isForTotalPayment) :
                                                CalculateFactor.GetFactorValue(startDate, endDate, scheduleInput, isScheduleInput, isForTotalPayment);

                double principal = scheduleInput.Tier1 != 0 ? (scheduleInput.Tier1 > remainingPrincipal ? remainingPrincipal : scheduleInput.Tier1) : remainingPrincipal;
                double AccruedInterestAmount = factor * .01 * Convert.ToDouble(scheduleInput.InterestRate) * principal;

                if (!string.IsNullOrEmpty(Convert.ToString(scheduleInput.InterestRate2)) && scheduleInput.Tier1 != 0 && remainingPrincipal > scheduleInput.Tier1)
                {
                    principal = (scheduleInput.Tier2 != 0 ? (scheduleInput.Tier2 > remainingPrincipal ? remainingPrincipal : scheduleInput.Tier2) : remainingPrincipal) -
                                                                    scheduleInput.Tier1;
                    AccruedInterestAmount += factor * .01 * Convert.ToDouble(scheduleInput.InterestRate2) * principal;
                }

                if (!string.IsNullOrEmpty(Convert.ToString(scheduleInput.InterestRate3)) && scheduleInput.Tier2 != 0 && remainingPrincipal > scheduleInput.Tier2)
                {
                    principal = (scheduleInput.Tier3 != 0 ? (scheduleInput.Tier3 > remainingPrincipal ? remainingPrincipal : scheduleInput.Tier3) : remainingPrincipal) -
                                                                    scheduleInput.Tier2;
                    AccruedInterestAmount += factor * .01 * Convert.ToDouble(scheduleInput.InterestRate3) * principal;
                }

                if (!string.IsNullOrEmpty(Convert.ToString(scheduleInput.InterestRate4)) && scheduleInput.Tier3 != 0 && remainingPrincipal > scheduleInput.Tier3)
                {
                    principal = (scheduleInput.Tier4 != 0 ? (scheduleInput.Tier4 > remainingPrincipal ? remainingPrincipal : scheduleInput.Tier4) : remainingPrincipal) -
                                                                    scheduleInput.Tier3;
                    AccruedInterestAmount += factor * .01 * Convert.ToDouble(scheduleInput.InterestRate4) * principal;
                }
                sbTracing.AppendLine("Exit:From PeriodicInterestAmount Class file and method Name CalcAccruedInterestWithTier(getScheduleInput scheduleInput, DateTime startDate, DateTime endDate, DateTime loanTakenDate, double remainingPrincipal, bool IsInterestDelay,bool isScheduleInput, bool isForTotalPayment)");
                return AccruedInterestAmount;

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
        /// Calculate the interest amount to be paid in the schedule without using tiers.
        /// </summary>
        /// <param name="scheduleInput"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="annualInterestRate"></param>
        /// <param name="loanTakenDate"></param>
        /// <param name="remainingPrincipal"></param>
        /// <param name="IsInterestDelay"></param>
        /// <returns></returns>
        public static double CalcAccruedInterestWithoutTier(getScheduleInput scheduleInput, DateTime startDate, DateTime endDate, double annualInterestRate, DateTime loanTakenDate, double remainingPrincipal, bool IsInterestDelay, bool isScheduleInput, bool isForTotalPayment)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                #region Calculating accrued interest without using tier values.

                sbTracing.AppendLine("Enter: Inside PeriodicInterestAmount Class file and method Name CalcAccruedInterestWithoutTier(getScheduleInput scheduleInput, DateTime startDate, DateTime endDate, double annualInterestRate, DateTime loanTakenDate, double remainingPrincipal, bool IsInterestDelay,bool isScheduleInput, bool isForTotalPayment).");
                sbTracing.AppendLine("Parameters are : " + "scheduleInput=" + scheduleInput + ",startDate = " + startDate + ", endDate = " + endDate + ",annualInterestRate =" + annualInterestRate + ", loanTakenDate = " + loanTakenDate + ", remainingPrincipal = " + remainingPrincipal + ", IsInterestDelay = " + IsInterestDelay + ", isScheduleInput =" + isScheduleInput +",isForTotalPayment=" + isForTotalPayment);

                double factor = IsInterestDelay ? CalculateFactor.GetFactorValue(startDate, endDate, loanTakenDate, scheduleInput.InterestDelay, scheduleInput, isScheduleInput, isForTotalPayment) :
                                               CalculateFactor.GetFactorValue(startDate, endDate, scheduleInput, isScheduleInput, isForTotalPayment);


                double AccruedInterestAmount = factor * .01 * annualInterestRate * remainingPrincipal;
                sbTracing.AppendLine("Exit:From PeriodicInterestAmount Class file and method Name CalcAccruedInterestWithoutTier(getScheduleInput scheduleInput, DateTime startDate, DateTime endDate, double annualInterestRate, DateTime loanTakenDate, double remainingPrincipal, bool IsInterestDelay,bool isScheduleInput)");

                return AccruedInterestAmount;
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
