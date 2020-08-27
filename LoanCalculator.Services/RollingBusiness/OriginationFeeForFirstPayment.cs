using System;
using System.Text;

namespace LoanAmort_driver.RollingBusiness
{
    public class OriginationFeeForFirstPayment
    {
        /// <summary>
        /// Name : Sparsh Agarwal
        /// Date : 17/03/2017
        /// User Story : 3. Input Value for Amount Financed, and User Story : 4. Define Calculation Method for Origination Fee, 
        /// and User Story : 5. Define how the Origination Fee is to be Paid, and User Story : 6. Input a value for Same Day Fee,
        /// and User story : 7. Define Calculation Method for Same Day Fee, and User Story : 8. Define how the Same Day Fee is to be Paid
        /// Explanation : This function will be called when Origination fee is not financed and is paid in first payment. this function returns value of origination fee
        /// to be paid in first payment. If financed then return 0.
        /// </summary>
        /// <param name="originationFeeFixed"></param>
        /// <param name="originationFeePercent"></param>
        /// <param name="originationFeeMax"></param>
        /// <param name="originationFeeMin"></param>
        /// <param name="originationFeeCalculationMethod"></param>
        /// <param name="loanAmount"></param>
        /// <returns></returns>
        public static double GetOriginationFeeForFirstPayment(double originationFeeFixed, double originationFeePercent, double originationFeeMax, double originationFeeMin,
                                                                    bool originationFeeCalculationMethod, double loanAmount)
        {
            StringBuilder sbTracing = new StringBuilder();
            double originationFeeToPay = 0;
            try
            {
                sbTracing.AppendLine("Enter:Inside OriginationFeeForFirstPayment class and Method Name GetOriginationFeeForFirstPayment(double originationFeeFixed, double originationFeePercent, double originationFeeMax, double originationFeeMin, bool originationFeeCalculationMethod, double loanAmount).");
                sbTracing.AppendLine("Parameter values are : originationFeeFixed = " + originationFeeFixed + ", originationFeePercent = " + originationFeePercent + ", originationFeeMax = " + originationFeeMax + ", originationFeeMin = " + originationFeeMin + ", originationFeeCalculationMethod = " + originationFeeCalculationMethod + ", loanAmount = " + loanAmount);

                //Calculate the origination percentage amount value, which is "originationFeePercent" percentage of "loanAmount".
                double percentageOriginationAmount = loanAmount * originationFeePercent * 0.01;
                double greaterOrLesserOutCome;

                //Check whether the calculation method for origination fee is selected as "Greater" or not i.e. checked or not.
                if ((originationFeeFixed > 0 && originationFeePercent == 0) || (originationFeeFixed == 0 && originationFeePercent > 0))
                {
                    greaterOrLesserOutCome = (originationFeeFixed > 0 && originationFeePercent == 0) ? originationFeeFixed : percentageOriginationAmount;
                }
                else if (originationFeeCalculationMethod)
                {
                    //Determine the greater value between the fixed origination fee amount and perentage origination fee amount.
                    greaterOrLesserOutCome = (originationFeeFixed > percentageOriginationAmount) ? originationFeeFixed : percentageOriginationAmount;
                }
                else
                {
                    //Determine the lower value between the fixed origination fee amount and perentage origination fee amount.
                    greaterOrLesserOutCome = (originationFeeFixed < percentageOriginationAmount) ? originationFeeFixed : percentageOriginationAmount;
                }

                //Check whether both the limits of origination fee is either not set or both are 0.
                if (originationFeeMax == 0 && originationFeeMin == 0)
                {
                    //Select the out come from calculation method as final origination fee, as there is no limit.
                    originationFeeToPay = greaterOrLesserOutCome;
                }
                //Check whether the upper limit is either not set or set to 0.
                else if (originationFeeMax == 0)
                {
                    originationFeeToPay = (originationFeeMin > greaterOrLesserOutCome) ? originationFeeMin : greaterOrLesserOutCome;
                }
                //Check whether the lower limit is either not set or set to 0.
                else if (originationFeeMin == 0)
                {
                    originationFeeToPay = (originationFeeMax < greaterOrLesserOutCome) ? originationFeeMax : greaterOrLesserOutCome;
                }
                else
                {
                    //Check whether the out come from calculation method is less then the lower limit.
                    if (originationFeeMin >= greaterOrLesserOutCome)
                    {
                        //Select lower limit as a final origination fee.
                        originationFeeToPay = originationFeeMin;
                    }
                    //Check whether the out come from calculation method is between the lower and upper limit.
                    else if (originationFeeMin < greaterOrLesserOutCome && originationFeeMax > greaterOrLesserOutCome)
                    {
                        //Select the out come from calculation method as a final origination fee.
                        originationFeeToPay = greaterOrLesserOutCome;
                    }
                    //Check whether the out come from calculation method is greater then the upper limit.
                    else if (originationFeeMax <= greaterOrLesserOutCome)
                    {
                        //Select upper limit as a final origination fee.
                        originationFeeToPay = originationFeeMax;
                    }
                }

                originationFeeToPay = Math.Round(originationFeeToPay, 2, MidpointRounding.AwayFromZero);

                sbTracing.AppendLine("Exit:From OriginationFeeForFirstPayment class and Method Name GetOriginationFeeForFirstPayment(double originationFeeFixed, double originationFeePercent, double originationFeeMax, double originationFeeMin, bool originationFeeCalculationMethod, double loanAmount)");
                return originationFeeToPay;
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
