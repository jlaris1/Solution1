using System;
using System.Text;
using LoanAmort_driver.Models;

namespace LoanAmort_driver.Business
{
    public class CalculatePeriodicInterestRate
    {
        /// <summary>
        /// Name : Sparsh Agarwal
        /// Date : 10/05/2018
        /// This function calculates the periodic interest rate with the functionality of interest tiers.
        /// </summary>
        /// <param name="scheduleInput"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="loanTakenDate"></param>
        /// <param name="beginingPrincipal"></param>
        /// <param name="withResidual"></param>
        /// <returns></returns>
        public static double PeriodicRateCalcWithTier(LoanDetails scheduleInput, DateTime startDate, DateTime endDate, DateTime loanTakenDate, double beginingPrincipal, bool withResidual, bool isScheduleInput, bool isForTotalPayment)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter: Inside Class CalculatePeriodicInterestRate and method Name PeriodicRateCalcWithTier(LoanDetails scheduleInput, DateTime startDate, DateTime endDate, DateTime loanTakenDate, double beginingPrincipal, bool withResidual, bool isScheduleInput).");
                sbTracing.AppendLine("Parameters are : " + "startDate = " + startDate + "endDate = " + endDate + "loanTakenDate = " + loanTakenDate + ", beginingPrincipal = " + beginingPrincipal + ", withResidual = " + withResidual + ",isScheduleInput" + isScheduleInput);

                double factor;
                //Calculate the factor as per the requirement of whether to use the residual value or not.
                if (withResidual)
                {
                    sbTracing.AppendLine("Inside CalculatePeriodicInterestRate class, and PeriodicRateCalcWithTier() method. Calling method :  CalculateFactor.GetFactorValue(startDate, endDate, loanTakenDate, scheduleInput.InterestDelay, isScheduleInput, scheduleInput);");
                    factor = CalculateFactor.GetFactorValue(startDate, endDate, loanTakenDate, scheduleInput.InterestDelay, isScheduleInput, scheduleInput, isForTotalPayment);
                }
                else
                {
                    sbTracing.AppendLine("Inside CalculatePeriodicInterestRate class, and PeriodicRateCalcWithTier() method. Calling method : CalculateFactor.GetFactorValue( startDate, endDate, isScheduleInput, scheduleInput);");
                    factor = CalculateFactor.GetFactorValue(startDate, endDate, isScheduleInput, scheduleInput, isForTotalPayment);
                }

                //Calculate the periodic interest rate
                double periodicInterestRate = factor * .01 * Convert.ToDouble(scheduleInput.InterestRate);
                if (!string.IsNullOrEmpty(scheduleInput.InterestRate2) && scheduleInput.Tier1 != 0 && beginingPrincipal > scheduleInput.Tier1)
                {
                    periodicInterestRate += factor * .01 * Convert.ToDouble(scheduleInput.InterestRate2);
                }

                if (!string.IsNullOrEmpty(scheduleInput.InterestRate3) && scheduleInput.Tier1 != 0 && scheduleInput.Tier2 != 0 && beginingPrincipal > scheduleInput.Tier2)
                {
                    periodicInterestRate += factor * .01 * Convert.ToDouble(scheduleInput.InterestRate3);
                }

                if (!string.IsNullOrEmpty(scheduleInput.InterestRate4) && scheduleInput.Tier1 != 0 && scheduleInput.Tier2 != 0 && scheduleInput.Tier3 != 0
                                        && beginingPrincipal > scheduleInput.Tier3)
                {
                    periodicInterestRate += factor * .01 * Convert.ToDouble(scheduleInput.InterestRate4);
                }

                periodicInterestRate = Math.Round(periodicInterestRate, 15, MidpointRounding.AwayFromZero);

                sbTracing.AppendLine("Exit:From Class CalculatePeriodicInterestRate and method Name PeriodicRateCalcWithTier(LoanDetails scheduleInput, DateTime startDate, DateTime endDate, DateTime loanTakenDate, double beginingPrincipal, bool withResidual, bool isScheduleInput).");
                return periodicInterestRate;
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
        /// <param name="withResidual"></param>
        /// <returns></returns>
        public static double PeriodicRateCalcWithoutTier(LoanDetails scheduleInput, DateTime startDate, DateTime endDate, double annualInterestRate, DateTime loanTakenDate, bool withResidual, bool isScheduleInput, bool isForTotalPayment)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter: Inside Class CalculatePeriodicInterestRate and method Name PeriodicRateCalcWithoutTier(LoanDetails scheduleInput, DateTime startDate, DateTime endDate, double annualInterestRate, DateTime loanTakenDate, bool withResidual, bool isScheduleInput).");
                sbTracing.AppendLine("Parameters are : " + "startDate = " + startDate + "endDate = " + endDate + "annualInterestRate = " + annualInterestRate + "loanTakenDate = " + loanTakenDate + ", withResidual = " + withResidual + ",isScheduleInput" + isScheduleInput);

                double factor;
                //Calculate the factor as per the requirement of whether to use the residual value or not.
                if (withResidual)
                {
                    sbTracing.AppendLine("Inside CalculatePeriodicInterestRate class, and PeriodicRateCalcWithoutTier() method. Calling method :  CalculateFactor.GetFactorValue(startDate, endDate, loanTakenDate, scheduleInput.InterestDelay, isScheduleInput, scheduleInput);");
                    factor = CalculateFactor.GetFactorValue(startDate, endDate, loanTakenDate, scheduleInput.InterestDelay, isScheduleInput, scheduleInput, isForTotalPayment);
                }
                else
                {
                    sbTracing.AppendLine("Inside CalculatePeriodicInterestRate class, and PeriodicRateCalcWithoutTier() method. Calling method : CalculateFactor.GetFactorValue(startDate, endDate, isScheduleInput, scheduleInput);");
                    factor = CalculateFactor.GetFactorValue(startDate, endDate, isScheduleInput, scheduleInput, isForTotalPayment);
                }

                //Calculate the periodic interest rate
                double periodicInterestRate = factor * .01 * annualInterestRate;
                periodicInterestRate = Math.Round(periodicInterestRate, 15, MidpointRounding.AwayFromZero);

                sbTracing.AppendLine("Exit:From Class CalculatePeriodicInterestRate and method Name PeriodicRateCalcWithoutTier(LoanDetails scheduleInput, DateTime startDate, DateTime endDate, double annualInterestRate, DateTime loanTakenDate, bool withResidual, bool isScheduleInput).");
                return periodicInterestRate;
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
