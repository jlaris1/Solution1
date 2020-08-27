using System;
using System.Text;

namespace LoanAmort_driver.Business
{
    public class PrincipalServiceFee
    {
        /// <summary>
        /// Name : Sparsh Agarwal
        /// Date : 21/03/2017
        /// Explanation : This function is used to determine the principal service fee on which the payable amount of service fee and service fee interest is calculated.
        /// </summary>
        /// <param name="applyServiceFeeInterest"></param>
        /// <param name="loanAmount"></param>
        /// <param name="serviceFee"></param>
        /// <returns></returns>
        public static double CalculatePrincipalServiceeFee(int applyServiceFeeInterest, double loanAmount, double serviceFee)
        {
            double principalServiceFee = 0;
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside Business Class PrincipalServiceFee and Method Name CalculatePrincipalServiceeFee(int applyServiceFeeInterest, double loanAmount, double serviceFee).");
                sbTracing.AppendLine("Parameter values are : applyServiceFeeInterest = " + applyServiceFeeInterest + ", loanAmount = " + loanAmount + ", serviceFee = " + serviceFee);

                if (applyServiceFeeInterest == 0 || applyServiceFeeInterest == 1)
                {
                    //This condition will executes when selected index of service fee calculation method is either 0 or 1, i.e. per loan.
                    principalServiceFee = serviceFee;
                }
                else if (applyServiceFeeInterest == 2 || applyServiceFeeInterest == 3)
                {
                    //Otherwise selected index of service fee calculation method is either 2 or 3, i.e. per 100$.
                    principalServiceFee = loanAmount * serviceFee * .01;
                }
                sbTracing.AppendLine("Exit:From Business Class PrincipalServiceFee and Method Name CalculatePrincipalServiceeFee(int applyServiceFeeInterest, double loanAmount, double serviceFee)");
                return principalServiceFee;
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
                return principalServiceFee;
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }
    }
}
