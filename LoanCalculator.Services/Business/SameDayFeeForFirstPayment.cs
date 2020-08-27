using System;
using System.Text;

namespace LoanAmort_driver.Business
{
    public class SameDayFeeForFirstPayment
    {
        /// <summary>
        /// Name : Sparsh Agarwal
        /// Date : 17/03/2017
        /// User Story : 3. Input Value for Amount Financed, and User Story : 4. Define Calculation Method for Origination Fee, 
        /// and User Story : 5. Define how the Origination Fee is to be Paid, and User Story : 6. Input a value for Same Day Fee,
        /// and User story : 7. Define Calculation Method for Same Day Fee, and User Story : 8. Define how the Same Day Fee is to be Paid
        /// Explanation : This function will be called when same day fee is not financed and is paid in first payment. this function returns value of same day fee to be
        /// paid in first payment. If financed then return 0
        /// </summary>
        /// <param name="sameDayFee"></param>
        /// <param name="sameDayFeeCalculationMethod"></param>
        /// <param name="beginningPrincipal"></param>
        /// <returns></returns>
        public static double GetSameDayFeeForFirstPayment(double sameDayFee, string sameDayFeeCalculationMethod, double beginningPrincipal)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside Business Class SameDayFeeForFirstPayment and method name GetSameDayFeeForFirstPayment(double sameDayFee, string sameDayFeeCalculationMethod, double beginningPrincipal).");
                sbTracing.AppendLine("Parameter values are : sameDayFee = " + sameDayFee + ", sameDayFeeCalculationMethod = " + sameDayFeeCalculationMethod + ", beginningPrincipal = " + beginningPrincipal);
                double sameDayFeeToPay;
                if (sameDayFeeCalculationMethod.ToLower() == Models.Constants.Fixed)
                {
                    //In this case, input value of same day fee will be considered as payable amount.
                    sameDayFeeToPay = sameDayFee;
                }
                else
                {
                    //In this case, input value of same day fee will be used as percentage to be paid.
                    sameDayFeeToPay = beginningPrincipal * sameDayFee * .01;
                }
                sbTracing.AppendLine("Exist:From SameDayFeeForFirstPayment Class file and method name GetSameDayFeeForFirstPayment(double sameDayFee, string sameDayFeeCalculationMethod, double beginningPrincipal)");
                return sameDayFeeToPay;
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
