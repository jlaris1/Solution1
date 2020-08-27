using System;
using System.Text;
using LoanAmort_driver.Models;

namespace LoanAmort_driver.Business
{
    public class CalculateTotalAmount
    {
        /// <summary>
        /// Name : Sparsh Agarwal
        /// Date : 03/04/2017
        /// Explanation : This function takes the scheduleinputs to calculate the equal value of total amount to be paid for each of the repayment schedule.
        /// </summary>
        /// <param name="scheduleInputs"></param>
        /// <param name="loanAmountCalc"></param>
        /// <returns></returns>
        public static double TotalPayableAmount(LoanDetails scheduleInputs, double loanAmountCalc)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class CalculateTotalAmount and method name TotalPayableAmount(LoanDetails scheduleInputs, double loanAmountCalc).");
                sbTracing.AppendLine("Parameter value is : loanAmountCalc = " + loanAmountCalc);

                //Checks if interest rate to be applied for that period is 0.
                double totalAmount;
                if (Convert.ToDouble(scheduleInputs.InterestRate) == 0 &&
                    (string.IsNullOrEmpty(scheduleInputs.InterestRate2) || (!string.IsNullOrEmpty(scheduleInputs.InterestRate2) && Convert.ToDouble(scheduleInputs.InterestRate2) == 0)) &&
                    (string.IsNullOrEmpty(scheduleInputs.InterestRate3) || (!string.IsNullOrEmpty(scheduleInputs.InterestRate3) && Convert.ToDouble(scheduleInputs.InterestRate3) == 0)) &&
                    (string.IsNullOrEmpty(scheduleInputs.InterestRate4) || (!string.IsNullOrEmpty(scheduleInputs.InterestRate4) && Convert.ToDouble(scheduleInputs.InterestRate4) == 0)))
                {
                    totalAmount = scheduleInputs.BalloonPayment > 0 ? Math.Round((loanAmountCalc - scheduleInputs.BalloonPayment) / (scheduleInputs.InputRecords.Count - 2), 2, MidpointRounding.AwayFromZero) :
                                  Math.Round(loanAmountCalc / (scheduleInputs.InputRecords.Count - 1), 2, MidpointRounding.AwayFromZero);
                }
                else
                {
                    //Calculate the periodic interest rate for the first repayment schedule.
                    sbTracing.AppendLine("Inside CalculateTotalAmount class, and TotalPayableAmount() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInputs, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[0].DateIn, loanAmountCalc, false,true,true);");
                    double regularInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInputs, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[0].DateIn, loanAmountCalc, false, true, true);

                    sbTracing.AppendLine("Inside CalculateTotalAmount class, and TotalPayableAmount() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[0].DateIn, loanAmountCalc, false,true,true);");
                    double firstPeriodInterestAmount = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[0].DateIn, loanAmountCalc, false, true, true);

                    double regularInterestAmount = firstPeriodInterestAmount;
                    //Checks whether the there are more than one repayment schedule in input grid, on which loan is to be paid.
                    if (scheduleInputs.InputRecords.Count > 2 && scheduleInputs.DaysInMonth.ToLower() != Constants.Periodic)
                    {
                        //Calculates the periodic interest rate for the second repayment schedule.
                        sbTracing.AppendLine("Inside CalculateTotalAmount class, and TotalPayableAmount() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInputs, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[2].DateIn, scheduleInputs.InputRecords[0].DateIn, loanAmountCalc, false,true,true);");
                        regularInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInputs, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[2].DateIn, scheduleInputs.InputRecords[0].DateIn, loanAmountCalc, false, true, true);

                        sbTracing.AppendLine("Inside CalculateTotalAmount class, and TotalPayableAmount() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[2].DateIn, scheduleInputs.InputRecords[0].DateIn, loanAmountCalc, false,true,true);");
                        regularInterestAmount = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, scheduleInputs.InputRecords[1].DateIn, scheduleInputs.InputRecords[2].DateIn, scheduleInputs.InputRecords[0].DateIn, loanAmountCalc, false, true, true);
                    }

                    //Calculate the default total payable amount for each repayment schedule.
                    if (regularInterestRate == 0)
                    {
                        totalAmount = scheduleInputs.BalloonPayment > 0 ? Math.Round((loanAmountCalc - scheduleInputs.BalloonPayment) / (scheduleInputs.InputRecords.Count - 2), 2, MidpointRounding.AwayFromZero) :
                            Math.Round(loanAmountCalc / (scheduleInputs.InputRecords.Count - 1), 2, MidpointRounding.AwayFromZero);// +
                                                                                                                                   //Math.Round(scheduleInputs.EarnedInterest / (scheduleInputs.InputRecords.Count - 1), 2, MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        double regularPayment = (loanAmountCalc * regularInterestRate * Math.Pow((1 + regularInterestRate), (scheduleInputs.InputRecords.Count - 1))) / (Math.Pow((1 + regularInterestRate), (scheduleInputs.InputRecords.Count - 1)) - 1);

                        //calculating totalAmount payment for schedule.
                        sbTracing.AppendLine("Inside CalculateTotalAmount class, and TotalPayableAmount() method. Calling method : CalculateAnnuity.CalcateAnnuityWithLongerFirstPeriod(regularInterestAmount, firstPeriodInterestAmount, regularPayment, scheduleInputs, false, 0, loanAmountCalc);");
                        totalAmount = CalculateAnnuity.CalcateAnnuityWithLongerFirstPeriod(regularInterestAmount, firstPeriodInterestAmount, regularPayment, scheduleInputs, false, 0, loanAmountCalc);
                    }
                }
                sbTracing.AppendLine("Exist:From class CalculateTotalAmount and method name TotalPayableAmount(LoanDetails scheduleInputs, double loanAmountCalc)");
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
