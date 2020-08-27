using System;
using System.Text;
using LoanAmort_driver.Models;

namespace LoanAmort_driver.PeriodBusiness
{
    public class PeriodicInterestAmount
    {
        /// <summary>
        /// Name : Sparsh Agarwal
        /// Date : 1/05/2018
        /// This function calculate the interest amount to be paid in the schedule using interest tiers.
        /// </summary>
        /// <param name="scheduleInput"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="loanTakenDate"></param>
        /// <param name="beginingPrincipal"></param>
        /// <param name="withResidual"></param>
        /// <returns></returns>
        public static double CalculateInterestWithTier(LoanDetails scheduleInput, DateTime startDate, DateTime endDate, DateTime loanTakenDate, double beginingPrincipal, bool withResidual, bool isScheduleInput, bool isForTotalPayment)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter: Inside PeriodicInterestAmount Class file and method Name CalculateInterestWithTier(LoanDetails scheduleInput, DateTime startDate, DateTime endDate, DateTime loanTakenDate, double beginingPrincipal, bool withResidual, bool isScheduleInput).");
                sbTracing.AppendLine("Parameters are : " + "startDate = " + startDate + ", endDate = " + endDate + ", loanTakenDate = " + loanTakenDate + ", beginingPrincipal = " + beginingPrincipal + ", withResidual = " + withResidual);

                double factor;

                //Calculate the factor as per the value of withResidual.
                if (withResidual)
                {
                    sbTracing.AppendLine("Inside PeriodicInterestAmount class, and CalculateInterestWithTier() method. Calling method : CalculateFactor.GetFactorValue(startDate, endDate, loanTakenDate, scheduleInput.InterestDelay,isScheduleInput,scheduleInput);");
                    factor = CalculateFactor.GetFactorValue(startDate, endDate, loanTakenDate, scheduleInput.InterestDelay, isScheduleInput, scheduleInput, isForTotalPayment);
                }
                else
                {
                    sbTracing.AppendLine("Inside PeriodicInterestAmount class, and CalculateInterestWithTier() method. Calling method : CalculateFactor.GetFactorValue(startDate, endDate,isScheduleInput, scheduleInput);");
                    factor = CalculateFactor.GetFactorValue(startDate, endDate, isScheduleInput, scheduleInput, isForTotalPayment);
                }

                double principalVal = scheduleInput.Tier1 != 0 ? (scheduleInput.Tier1 > beginingPrincipal ? beginingPrincipal : scheduleInput.Tier1) : beginingPrincipal;
                //Calculate the interest for first tier
                double interestAmount = factor * .01 * Convert.ToDouble(scheduleInput.InterestRate) * principalVal;

                //Calculate the interest for second tier
                if (!string.IsNullOrEmpty(scheduleInput.InterestRate2) && scheduleInput.Tier1 != 0 && beginingPrincipal > scheduleInput.Tier1)
                {
                    principalVal = (scheduleInput.Tier2 != 0 ? (scheduleInput.Tier2 > beginingPrincipal ? beginingPrincipal : scheduleInput.Tier2) : beginingPrincipal) -
                                                                    scheduleInput.Tier1;
                    interestAmount += factor * .01 * Convert.ToDouble(scheduleInput.InterestRate2) * principalVal;
                }

                //Calculate the interest for third tier
                if (!string.IsNullOrEmpty(scheduleInput.InterestRate3) && scheduleInput.Tier1 != 0 && scheduleInput.Tier2 != 0 && beginingPrincipal > scheduleInput.Tier2)
                {
                    principalVal = (scheduleInput.Tier3 != 0 ? (scheduleInput.Tier3 > beginingPrincipal ? beginingPrincipal : scheduleInput.Tier3) : beginingPrincipal) -
                                                                    scheduleInput.Tier2;
                    interestAmount += factor * .01 * Convert.ToDouble(scheduleInput.InterestRate3) * principalVal;
                }

                //Calculate the interest for fourth tier
                if (!string.IsNullOrEmpty(scheduleInput.InterestRate4) && scheduleInput.Tier1 != 0 && scheduleInput.Tier2 != 0 && scheduleInput.Tier3 != 0 &&
                                                    beginingPrincipal > scheduleInput.Tier3)
                {
                    principalVal = (scheduleInput.Tier4 != 0 ? (scheduleInput.Tier4 > beginingPrincipal ? beginingPrincipal : scheduleInput.Tier4) : beginingPrincipal) -
                                                                    scheduleInput.Tier3;
                    interestAmount += factor * .01 * Convert.ToDouble(scheduleInput.InterestRate4) * principalVal;
                }

                sbTracing.AppendLine("Exit:From PeriodicInterestAmount Class file and method Name CalculateInterestWithTier(LoanDetails scheduleInput, DateTime startDate, DateTime endDate, DateTime loanTakenDate, double beginingPrincipal, bool withResidual, bool isScheduleInput)");
                return interestAmount;
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
        /// <param name="beginingPrincipal"></param>
        /// <param name="withResidual"></param>
        /// <returns></returns>
        public static double CalculateInterestWithoutTier(LoanDetails scheduleInput, DateTime startDate, DateTime endDate, double annualInterestRate, DateTime loanTakenDate, double beginingPrincipal, bool withResidual, bool isScheduleInput, bool isForTotalPayment)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter: Inside PeriodicInterestAmount Class file and method Name CalculateInterestWithoutTier(LoanDetails scheduleInput, DateTime startDate, DateTime endDate, double annualInterestRate, DateTime loanTakenDate, double beginingPrincipal, bool withResidual, bool isScheduleInput).");
                sbTracing.AppendLine("Parameters are : " + "startDate = " + startDate + ", endDate = " + endDate + ", annualInterestRate = " + annualInterestRate + ", loanTakenDate = " + loanTakenDate + ", beginingPrincipal = " + beginingPrincipal + ", withResidual = " + withResidual);
                double factor;

                if (withResidual)
                {
                    sbTracing.AppendLine("Inside PeriodicInterestAmount class, and CalculateInterestWithoutTier() method. Calling method : CalculateFactor.GetFactorValue(startDate, endDate, loanTakenDate, scheduleInput.InterestDelay,isScheduleInput,scheduleInput);");
                    factor = CalculateFactor.GetFactorValue(startDate, endDate, loanTakenDate, scheduleInput.InterestDelay, isScheduleInput, scheduleInput, isForTotalPayment);
                }
                else
                {
                    sbTracing.AppendLine("Inside PeriodicInterestAmount class, and CalculateInterestWithoutTier() method. Calling method : CalculateFactor.GetFactorValue(startDate, endDate,isScheduleInput,scheduleInput);");
                    factor = CalculateFactor.GetFactorValue(startDate, endDate, isScheduleInput, scheduleInput, isForTotalPayment);
                }

                double interestAmount = factor * .01 * annualInterestRate * beginingPrincipal;

                sbTracing.AppendLine("Exit:From PeriodicInterestAmount Class file and method Name CalculateInterestWithoutTier(LoanDetails scheduleInput, DateTime startDate, DateTime endDate, double annualInterestRate, DateTime loanTakenDate, double beginingPrincipal, bool withResidual, bool isScheduleInput)");
                return interestAmount;
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
