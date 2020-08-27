using System;
using System.Text;

namespace LoanAmort_driver.Business
{
    public class AmountFinanced
    {
        /// <summary>
        /// Name : Adesh Kumar
        /// Date : 17/03/2017
        /// User Story : 3. Input Value for Amount Financed, and User Story : 4. Define Calculation Method for Origination Fee, 
        /// and User Story : 5. Define how the Origination Fee is to be Paid, and User Story : 6. Input a value for Same Day Fee,
        /// and User story : 7. Define Calculation Method for Same Day Fee, and User Story : 8. Define how the Same Day Fee is to be Paid
        /// Explanation : This function takes the input values of origination fee and same day fee, their calculation methods, and boolean value of whether they are 
        /// financed or not. This function calculates the amount financed and return the amount financed value.
        /// </summary>
        /// <param name="inputLoanAmount"></param>
        /// <param name="inputAmountFinanced"></param>
        /// <param name="originationFee"></param>
        /// <param name="isOriginationFeeFinanced"></param>
        /// <param name="sameDayFee"></param>
        /// <param name="sameDayFeeCalculationMethod"></param>
        /// <param name="isSameDayFeeFinanced"></param>
        /// <param name="serviceFee"></param>
        /// <param name="serviceFeeCalculationMethod"></param>
        /// <param name="isServiceFeeFinanced"></param>
        /// <returns></returns>
        public static double GetAmountFinanced(double inputLoanAmount, double inputAmountFinanced, double originationFee, bool isOriginationFeeFinanced, double sameDayFee,
                        string sameDayFeeCalculationMethod, bool isSameDayFeeFinanced, double serviceFee, int serviceFeeCalculationMethod, bool isServiceFeeFinanced)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter: Inside class AmountFinanced and method name GetAmountFinanced(double inputLoanAmount, double inputAmountFinanced, double originationFee, bool isOriginationFeeFinanced, double sameDayFee, string sameDayFeeCalculationMethod, bool isSameDayFeeFinanced, double serviceFee, int serviceFeeCalculationMethod, bool isServiceFeeFinanced).");
                sbTracing.AppendLine("Parameter values are : inputLoanAmount = " + inputLoanAmount + ", inputAmountFinanced = " + inputAmountFinanced + ", originationFee = " + originationFee +
                    ", isOriginationFeeFinanced = " + isOriginationFeeFinanced + ", sameDayFee = " + sameDayFee + ", sameDayFeeCalculationMethod = " + sameDayFeeCalculationMethod + ", isSameDayFeeFinanced = " +
                    isSameDayFeeFinanced + ", serviceFee = " + serviceFee + ", serviceFeeCalculationMethod = " + serviceFeeCalculationMethod + ", isServiceFeeFinanced = " + isServiceFeeFinanced);

                //Checke whether loan amount input is provided and amount financed input is 0.
                double beginningAmountFinanced = (inputLoanAmount > 0 && inputAmountFinanced == 0) ? inputLoanAmount : inputAmountFinanced;

                double amountFinanced = beginningAmountFinanced;

                //Checke whether loan amount input is provided and amount financed input is 0.
                if (inputLoanAmount > 0 && inputAmountFinanced == 0)
                {
                    //This condition checks whether the origination fee is financed, or will be paid as first payment. If financed, only then origination fee will be used to
                    //calculate amount financed.
                    if (isOriginationFeeFinanced)
                    {
                        //In this case, input value of origination fee will be used to calculate financed amount.
                        amountFinanced = beginningAmountFinanced - originationFee;
                    }

                    //This condition checks whether the same day fee is financed, or will be paid as first payment. If financed, only then same day fee will be used to
                    //calculate amount financed.
                    if (isSameDayFeeFinanced)
                    {
                        amountFinanced -= (sameDayFeeCalculationMethod.ToLower() == Models.Constants.Fixed) ? sameDayFee : (beginningAmountFinanced * sameDayFee * .01);
                    }

                    //This condition checks whether the service fee is financed, or not. If financed, only then service fee will be used to calculate amount financed.
                    if (isServiceFeeFinanced)
                    {
                        amountFinanced -= (serviceFeeCalculationMethod == 0) ? serviceFee : (beginningAmountFinanced * serviceFee * .01);
                    }
                }
                amountFinanced = amountFinanced < 0 ? 0 : amountFinanced;

                sbTracing.AppendLine("Exit:From class AmountFinanced and method name GetAmountFinanced(double inputLoanAmount, double inputAmountFinanced, double originationFee, bool isOriginationFeeFinanced, double sameDayFee, string sameDayFeeCalculationMethod, bool isSameDayFeeFinanced, double serviceFee, int serviceFeeCalculationMethod, bool isServiceFeeFinanced)");
                return amountFinanced;
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
