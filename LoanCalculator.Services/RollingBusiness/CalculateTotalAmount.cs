using System;
using System.Text;
using LoanAmort_driver.Models;

namespace LoanAmort_driver.RollingBusiness
{
    public class CalculateTotalAmount
    {
        /// <summary>
        /// Name : Punit Singh
        /// Date : 29/07/2018
        /// Explanation : This function takes the scheduleinputs to calculate the equal value of total amount to be paid for each of the repayment schedule.
        /// </summary>
        /// <param name="scheduleInputs"></param>
        /// <param name="loanAmountCalc"></param>
        /// <returns></returns>
        public static double TotalPayableAmount(getScheduleInput scheduleInputs, double loanAmountCalc)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class CalculateTotalAmount and method name TotalPayableAmount(getScheduleInput scheduleInputs, double loanAmountCalc).");
                sbTracing.AppendLine("Parameter value is : loanAmountCalc = " + loanAmountCalc);

                //Checks if interest rate to be applied for that period is 0.
                double totalAmount;
                if (Convert.ToDouble(scheduleInputs.InterestRate) == 0 &&
                    (string.IsNullOrEmpty(Convert.ToString(scheduleInputs.InterestRate2)) || (!string.IsNullOrEmpty(Convert.ToString(scheduleInputs.InterestRate2)) && Convert.ToDouble(scheduleInputs.InterestRate2) == 0)) &&
                    (string.IsNullOrEmpty(Convert.ToString(scheduleInputs.InterestRate3)) || (!string.IsNullOrEmpty(Convert.ToString(scheduleInputs.InterestRate3)) && Convert.ToDouble(scheduleInputs.InterestRate3) == 0)) &&
                    (string.IsNullOrEmpty(Convert.ToString(scheduleInputs.InterestRate4)) || (!string.IsNullOrEmpty(Convert.ToString(scheduleInputs.InterestRate4)) && Convert.ToDouble(scheduleInputs.InterestRate4) == 0)))
                {
                    totalAmount = scheduleInputs.BalloonPayment > 0 ? ((loanAmountCalc - scheduleInputs.BalloonPayment) / (scheduleInputs.InputRecords.Count - 2)) : (loanAmountCalc / (scheduleInputs.InputRecords.Count - 1));
                }
                else
                {
                    //Calculate the periodic interest rate for the first repayment schedule.
                    sbTracing.AppendLine("Inside CalculateTotalAmount class, and TotalPayableAmount() method. Calling method : PeriodicInterestAmount.CalcAccruedInterestWithTier(scheduleInputs, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[0].DateIn, loanAmountCalc, false, true);");
                    double firstPeriodAccruedInterestAmount = PeriodicInterestAmount.CalcAccruedInterestWithTier(scheduleInputs, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[0].DateIn, loanAmountCalc, false, true, true);

                    sbTracing.AppendLine("Inside CalculateTotalAmount class, and TotalPayableAmount() method. Calling method : PeriodicInterestRate.GetPeriodicInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[2].DateIn, Convert.ToDouble(scheduleInputs.InterestRate));");
                    double regularInterestRate = CalculatePeriodicInterestRate.CalcPeriodicInterestRateWithTier(scheduleInputs, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[2].DateIn, scheduleInputs.InputRecords[1].DateIn, loanAmountCalc, false, true, true);

                    double regularAccruedInterestAmount = 0;

                    //Checks whether the there are more than one repayment schedule in input grid, on which loan is to be paid.
                    if (scheduleInputs.InputRecords.Count > 2 && scheduleInputs.DaysInMonth.ToLower() != Models.Constants.Periodic)
                    {
                        //Calculates the periodic interest rate for the second repayment schedule.
                        sbTracing.AppendLine("Inside CalculateTotalAmount class, and TotalPayableAmount() method. Calling method : PeriodicInterestRate.GetPeriodicInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[2].DateIn, Convert.ToDouble(scheduleInputs.InterestRate));");
                        regularInterestRate = CalculatePeriodicInterestRate.CalcPeriodicInterestRateWithTier(scheduleInputs, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[2].DateIn, scheduleInputs.InputRecords[1].DateIn, loanAmountCalc, false, true, true);

                        sbTracing.AppendLine("Inside CalculateTotalAmount class, and TotalPayableAmount() method. Calling method : PeriodicInterestAmount.CalcAccruedInterestWithTier(scheduleInputs, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[2].DateIn, scheduleInputs.InputRecords[1].DateIn, loanAmountCalc, false);");
                        regularAccruedInterestAmount = PeriodicInterestAmount.CalcAccruedInterestWithTier(scheduleInputs, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[2].DateIn, scheduleInputs.InputRecords[1].DateIn, loanAmountCalc, false, true, true);
                    }
                    else
                    {
                        regularAccruedInterestAmount = firstPeriodAccruedInterestAmount;
                    }

                    if (regularInterestRate == 0)
                    {
                        totalAmount = scheduleInputs.BalloonPayment > 0 ? ((loanAmountCalc - scheduleInputs.BalloonPayment) / (scheduleInputs.InputRecords.Count - 2)) : loanAmountCalc / (scheduleInputs.InputRecords.Count - 1);
                    }
                    else
                    {
                        //Calculate the default total payable amount for each repayment schedule.
                        double regularPayment = (regularAccruedInterestAmount * Math.Pow((1 + regularInterestRate), (scheduleInputs.InputRecords.Count - 1))) / (Math.Pow((1 + regularInterestRate), (scheduleInputs.InputRecords.Count - 1)) - 1);

                        sbTracing.AppendLine("Inside CalculateTotalAmount class, and TotalPayableAmount() method. Calling method : CalculateAnnuity.CalcateAnnuityWithLongerFirstPeriod(regularAccruedInterestAmount, firstPeriodAccruedInterestAmount, regularPayment, scheduleInputs, false, 0, loanAmountCalc);");
                        totalAmount = CalculateAnnuity.CalcateAnnuityWithLongerFirstPeriod(regularAccruedInterestAmount, firstPeriodAccruedInterestAmount, regularPayment, scheduleInputs, false, 0, loanAmountCalc);

                    }
                }
                sbTracing.AppendLine("Exist:From class CalculateTotalAmount and method name TotalPayableAmount(getScheduleInput scheduleInputs, double loanAmountCalc)");
                totalAmount += Math.Round(scheduleInputs.EarnedInterest / (scheduleInputs.InputRecords.Count - 1), 2, MidpointRounding.AwayFromZero);
                return totalAmount;
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
