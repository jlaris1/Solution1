using System;
using System.Text;
using LoanAmort_driver.Models;

namespace LoanAmort_driver.PeriodBusiness
{
    public class CalculateAnnuity
    {
        /// <summary>
        /// This function is used to calculate a value that will be equal for each of the repayment schedule, when the day difference between two dates is larger then
        /// the period.
        /// </summary>
        /// <param name="firstRegularInterestPayment"></param>
        /// <param name="firstInterestPaymentPaid"></param>
        /// <param name="regularPaymentAsPerPaymentPeriod"></param>
        /// <param name="scheduleInputs"></param>
        /// <param name="calculateServiceFeeTotal"></param>
        /// <param name="principalServiceFee"></param>
        /// <param name="loanAmountCalc"></param>
        /// <returns></returns>
        public static double CalcateAnnuityWithLongerFirstPeriod(double firstRegularInterestPayment, double firstInterestPaymentPaid, double regularPaymentAsPerPaymentPeriod,
                                                            LoanDetails scheduleInputs, bool calculateServiceFeeTotal, double principalServiceFee, double loanAmountCalc)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class CalculateAnnuity and method name CalcateAnnuityWithLongerFirstPeriod(double firstRegularInterestPayment, double firstInterestPaymentPaid, double regularPaymentAsPerPaymentPeriod, LoanDetails scheduleInputs, bool calculateServiceFeeTotal, double principalServiceFee, double loanAmountCalc).");
                sbTracing.AppendLine("Parameter values are : firstRegularInterestPayment = " + firstRegularInterestPayment + ", firstInterestPaymentPaid = " + firstInterestPaymentPaid + ", regularPaymentAsPerPaymentPeriod = " + regularPaymentAsPerPaymentPeriod + ", calculateServiceFeeTotal = " + calculateServiceFeeTotal + ", principalServiceFee = " + principalServiceFee + ", loanAmountCalc = " + loanAmountCalc);

                //Checks whether this function is called to calculate the total payable amount of service fee or to simple loan amount.
                double currentPrincipal = calculateServiceFeeTotal ? principalServiceFee : loanAmountCalc;

                double interestPaymentDifference = firstInterestPaymentPaid - firstRegularInterestPayment;
                double paymentOne = Math.Round(regularPaymentAsPerPaymentPeriod, 2, MidpointRounding.AwayFromZero);

                double maxAllowedDiff = 0.15;
                double currentDiff = Math.Abs(interestPaymentDifference);
                double interestDiff = 0;
                double multiplicator = 0.5;

                //Calculating Count of schedules with and without Balloon payment to calculate Total Amount.
                int scheduleCount = !calculateServiceFeeTotal && scheduleInputs.BalloonPayment > 0 ? (scheduleInputs.InputRecords.Count - 1) : (scheduleInputs.InputRecords.Count);

                //Iterate while difference reaches allowed value:
                do
                {
                    for (int i = 1; i < scheduleCount; i++)
                    {
                        sbTracing.AppendLine("Inside CalculateAnnuity class, and CalcateAnnuityWithLongerFirstPeriod() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, scheduleInputs.InputRecords[i - 1].DateIn, scheduleInputs.InputRecords[i].DateIn, scheduleInputs.InputRecords[0].DateIn, currentPrincipal, false, true);");
                        double currentInterestPayment = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, scheduleInputs.InputRecords[i - 1].DateIn, scheduleInputs.InputRecords[i].DateIn, scheduleInputs.InputRecords[0].DateIn, currentPrincipal, false, true, true);
                        double currentPrincipalPayment = paymentOne - currentInterestPayment;
                        if (interestDiff > 0)
                        {
                            if (interestDiff > currentPrincipalPayment)
                            {
                                currentInterestPayment += interestDiff;
                                interestDiff -= currentPrincipalPayment;
                                currentPrincipalPayment = 0;
                            }
                            else
                            {
                                currentPrincipalPayment -= interestDiff;
                                currentInterestPayment += interestDiff;
                                interestDiff = 0;
                            }
                        }

                        if (currentPrincipalPayment < 0)
                        {
                            interestDiff = Math.Abs(currentPrincipalPayment);
                            currentPrincipalPayment = 0;
                        }

                        //Check difference on last payment period:
                        if (i == (scheduleCount - 1))
                        {
                            double paymentLast = currentPrincipal + currentInterestPayment + interestDiff - (calculateServiceFeeTotal ? 0 :
                                                (scheduleInputs.BalloonPayment > 0 ? scheduleInputs.BalloonPayment : scheduleInputs.Residual));

                            if (paymentLast < 0)
                            {
                                paymentLast = 0;
                            }
                            currentDiff = Math.Abs(paymentOne - paymentLast);
                            if (currentDiff > maxAllowedDiff)
                            {
                                multiplicator += 0.5;
                                double change = Math.Abs(currentDiff) / (paymentOne + multiplicator);
                                if (change > currentDiff)
                                {
                                    change = currentDiff / i;
                                }
                                paymentOne = paymentOne > paymentLast ? paymentOne - change : paymentOne + change;

                                //Default all values and start again:
                                currentPrincipal = calculateServiceFeeTotal ? principalServiceFee : loanAmountCalc;
                                interestDiff = 0;
                                continue;
                            }
                        }
                        currentPrincipal -= currentPrincipalPayment;
                    }
                } while (currentDiff > maxAllowedDiff);
                sbTracing.AppendLine("Exist:From class CalculateAnnuity and method name CalcateAnnuityWithLongerFirstPeriod(double regularInterestRate, double firstPeriodInterestRate, double regularPaymentAsPerPaymentPeriod, LoanDetails scheduleInputs, bool calculateServiceFeeTotal, double principalServiceFee, double loanAmountCalc).");
                return paymentOne;
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