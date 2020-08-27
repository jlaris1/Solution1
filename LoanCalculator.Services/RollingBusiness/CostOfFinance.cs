using System;
using System.Collections.Generic;
using System.Text;
using LoanAmort_driver.Models;

namespace LoanAmort_driver.RollingBusiness
{
    public class CostOfFinance
    {
        /// <summary>
        /// Name : Sparsh Agarwal
        /// Date : 6/4/2017
        /// User Story 26 : Calculate Finance Cost of a Loan
        /// Explanation : This function calculates the total cost in taking the loan. The value will be the sum of financed fees, total interest paid and total service
        /// fee interest paid throughout the loan repayment schedule.
        /// </summary>
        /// <param name="loanDetails"></param>
        /// <param name="outputGrid"></param>
        /// <param name="originationFee"></param>
        /// <param name="principal"></param>
        /// <returns></returns>
        public static double CalculateCostOfFinance(getScheduleInput loanDetails, List<PaymentDetail> outputGrid, double originationFee, double principal)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class CostOfFinance and method name CalculateCostOfFinance(LoanDetails loanDetails, List<OutputGrid> outputGrid, double originationFee, double principal).");
                sbTracing.AppendLine("Parameter value is : originationFee = " + originationFee + ", principal = " + principal);

                double costOfFinancing = 0;
                //Check whether the origination fee is set to financed.
                if (loanDetails.IsOriginationFeeFinanced)
                {
                    costOfFinancing = originationFee;
                }

                //Check whether the same day fee is set to financed.
                if (loanDetails.IsSameDayFeeFinanced)
                {
                    if (loanDetails.SameDayFeeCalculationMethod.ToLower() == Models.Constants.Fixed)
                    {
                        //In this case, input value of same day fee will be used to calculate financed amount.
                        costOfFinancing += loanDetails.SameDayFee;
                    }
                    else
                    {
                        //In this case, input value of same day fee will be used as percentage to calculate financed amount.
                        costOfFinancing += (principal * loanDetails.SameDayFee * .01);
                    }
                }

                //Check whether the service fee is set to financed.
                if (loanDetails.IsServiceFeeFinanced)
                {
                    if (loanDetails.ApplyServiceFeeInterest == 0)
                    {
                        //In this case, input value of same day fee will be used to calculate financed amount.
                        costOfFinancing += loanDetails.ServiceFee;
                    }
                    else
                    {
                        //In this case, input value of same day fee will be used as percentage to calculate financed amount.
                        costOfFinancing += (principal * loanDetails.ServiceFee * .01);
                    }
                }

                costOfFinancing += outputGrid[outputGrid.Count - 1].CumulativeInterest + outputGrid[outputGrid.Count - 1].CumulativeMaintenanceFee +
                                        outputGrid[outputGrid.Count - 1].CumulativeManagementFee + outputGrid[outputGrid.Count - 1].CumulativeOriginationFee +
                                        outputGrid[outputGrid.Count - 1].CumulativeSameDayFee + outputGrid[outputGrid.Count - 1].CumulativeServiceFeeTotal;

                sbTracing.AppendLine("Exist:From class CostOfFinance and method name CalculateCostOfFinance(LoanDetails loanDetails, List<OutputGrid> outputGrid, double originationFee, double principal)");
                return costOfFinancing;
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
