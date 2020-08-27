using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoanAmort_driver.Models;

namespace LoanAmort_driver.RollingBusiness
{
    public class ActualPaymentAllocation1
    {
        /// <summary>
        /// Name : Sparsh Agarwal
        /// Date : 29/03/2017
        /// User story 11: Enter the Effective (Actual) Date of a scheduled loan payment, and User Story 12: Enter Actual Payment Amount for a scheduled loan payment, and 
        /// User Story 14: Track Actual Payment Amount Against a Scheduled Payment, and User Story 15: Track Actual Payment Date, and
        /// USER STORY 17: TRACK A MISSED PAYMENT, and USER STORY 18: TRACK A SKIPPED PAYMENT, and USER STORY 19: TRACK A PAYMENT ADJUSTMENT, and
        /// USER STORY 28: PROVIDE OUTPUT VALUES FOR ACTUAL AMOUNTS PAID, and USER STORY 29: PROVIDE OUTPUT VALUES FOR PERIOD PAST DUE AMOUNTS, and
        /// USER STORY 30: PROVIDE OUTPUT VALUES FOR CUMULATIVE PAST DUE AMOUNTS
        /// This class contains functions that will allocate the fund i.e. paid amount, to the past due balances first then cuurent period balance as per the defined
        /// priority. When the amount is exhausted, then I recalculate the output once again...
        /// </summary>
        /// <param name="outputGrid"></param>
        /// <param name="scheduleInput"></param>
        /// <param name="defaultTotalAmountToPay"></param>
        /// <param name="defaultTotalServiceFeePayable"></param>
        /// <param name="originationFee"></param>
        /// <param name="loanAmountCalc"></param>
        /// <param name="sameDayFee"></param>
        public static void AllocationOfFunds(List<OutputGrid> outputGrid, LoanDetails scheduleInput, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable,
                                                double originationFee, double loanAmountCalc, double sameDayFee)
        {
            int additionalPaymentCurrentRow = 0;
            int outputGridRowNumber = 0;
            int inputGridRow = 1;
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name AllocationOfFunds(List<OutputGrid> outputGrid, LoanDetails scheduleInput, " +
                    "double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double loanAmountCalc, double sameDayFee).");
                sbTracing.AppendLine("Parameter values are : defaultTotalAmountToPay = " + defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable +
                    ", originationFee = " + originationFee + ", loanAmountCalc = " + loanAmountCalc + ", sameDayFee = " + sameDayFee);

                for (inputGridRow = 1; inputGridRow < scheduleInput.InputRecords.Count && scheduleInput.InputRecords[inputGridRow].DateIn <= outputGrid[outputGrid.Count == 1 ? 0 : outputGrid.Count - 1].DueDate; inputGridRow++)
                {
                    #region This loop adds all the additonal payment either before, or in between the scheduled payments
                    for (int j = additionalPaymentCurrentRow; j < scheduleInput.AdditionalPaymentRecords.Count; j++)
                    {
                        //This condition checks whether additonal payment date is less than scheduled payment date. If it is, then it first adds the additonal payment row.
                        if (scheduleInput.AdditionalPaymentRecords[j].DateIn <= (scheduleInput.InputRecords[inputGridRow].EffectiveDate == DateTime.MinValue ? scheduleInput.InputRecords[inputGridRow].DateIn : scheduleInput.InputRecords[inputGridRow].EffectiveDate))
                        {
                            //This condition checks whether it is a principal only additonal payment.
                            if (scheduleInput.AdditionalPaymentRecords[j].Flags == 2)
                            {
                                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : PayPrincipalOnlyAmount(outputGrid, scheduleInput, additionalPaymentCurrentRow, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, false, originationFee, sameDayFee);");
                                PayPrincipalOnlyAmount(outputGrid, scheduleInput, additionalPaymentCurrentRow, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, false, originationFee, sameDayFee);
                            }
                            //This condition checks whether it is a event of adding a NSF fee or Late fee.
                            else if (scheduleInput.AdditionalPaymentRecords[j].Flags == 11 || scheduleInput.AdditionalPaymentRecords[j].Flags == 12)
                            {
                                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : AddNSFAndLateFee(outputGrid, scheduleInput, additionalPaymentCurrentRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);");
                                AddNSFAndLateFee(outputGrid, scheduleInput, additionalPaymentCurrentRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);
                            }
                            //This condition checks whether it is a additional payment where paiyment amount will be distributed among components as per priorities.
                            else if (scheduleInput.AdditionalPaymentRecords[j].Flags == 3)
                            {
                                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : PayAdditionalPayment(outputGrid, scheduleInput, additionalPaymentCurrentRow, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, false, originationFee, sameDayFee);");
                                PayAdditionalPayment(outputGrid, scheduleInput, additionalPaymentCurrentRow, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, false, originationFee, sameDayFee);
                            }
                            //This condition checks whether it is a discount.
                            else if (scheduleInput.AdditionalPaymentRecords[j].Flags == 5)
                            {
                                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : AddDiscount(outputGrid, scheduleInput, additionalPaymentCurrentRow, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, false, originationFee, sameDayFee);");
                                AddDiscount(outputGrid, scheduleInput, additionalPaymentCurrentRow, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, false, originationFee, sameDayFee);
                            }
                            additionalPaymentCurrentRow++;
                            outputGridRowNumber++;
                        }
                    }
                    #endregion
                    //This condition checks, the schedule payment is for paid, missed, adjustment.
                    if (scheduleInput.InputRecords[inputGridRow].Flags == 0)
                    {
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : AllocateFundToBuckets(outputGrid, scheduleInput, inputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, outputGridRowNumber, originationFee, sameDayFee);");
                        AllocateFundToBuckets(outputGrid, scheduleInput, inputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, outputGridRowNumber, originationFee, sameDayFee);
                    }
                    //This condition checks, the schedule payment is skipped payment.
                    else if (scheduleInput.InputRecords[inputGridRow].Flags == 4)
                    {
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : SkippedPayment(outputGrid, scheduleInput, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);");
                        SkippedPayment(outputGrid, scheduleInput, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);
                    }
                    outputGridRowNumber++;
                }

                //This loop determine the index value from which index, the rows of output grid will be deleted.
                int startDeletionIndex = outputGrid.Count;
                for (int i = 0; i <= outputGrid.Count - 1; i++)
                {
                    double nsfFee = 0;
                    double lateFee = 0;
                    for (int j = 0; j < scheduleInput.AdditionalPaymentRecords.Count; j++)
                    {
                        if (scheduleInput.AdditionalPaymentRecords[j].DateIn < outputGrid[i].PaymentDate)
                        {
                            if (scheduleInput.AdditionalPaymentRecords[j].Flags == 11)
                            {
                                nsfFee += scheduleInput.AdditionalPaymentRecords[j].AdditionalPayment;
                            }
                            else if (scheduleInput.AdditionalPaymentRecords[j].Flags == 12)
                            {
                                lateFee += scheduleInput.AdditionalPaymentRecords[j].AdditionalPayment;
                            }
                        }
                    }

                    double totalDue = originationFee + sameDayFee + nsfFee + lateFee;
                    //This loop will determine the total due amount to pay.
                    for (int j = 0; j < i; j++)
                    {
                        totalDue = totalDue + (outputGrid[j].Flags == 4 ? 0 : (outputGrid[j].ManagementFee + outputGrid[j].MaintenanceFee)) -
                            (outputGrid[j].SameDayFeePaid + outputGrid[j].OriginationFeePaid + outputGrid[j].MaintenanceFeePaid + outputGrid[j].ManagementFeePaid +
                            outputGrid[j].NSFFeePaid + outputGrid[j].LateFeePaid);
                    }
                    totalDue += (i == 0 ? 0 : (outputGrid[i - 1].InterestCarryOver + outputGrid[i - 1].serviceFeeInterestCarryOver));
                    totalDue = Math.Round(totalDue, 2, MidpointRounding.AwayFromZero);

                    if (Math.Round(outputGrid[i].BeginningPrincipal, 2, MidpointRounding.AwayFromZero) <= 0 &&
                        Math.Round(outputGrid[i].BeginningServiceFee, 2, MidpointRounding.AwayFromZero) <= 0 &&
                        Math.Round(outputGrid[i].NSFFee, 2, MidpointRounding.AwayFromZero) == 0 &&
                        Math.Round(outputGrid[i].LateFee, 2, MidpointRounding.AwayFromZero) == 0 &&
                        Math.Round(outputGrid[i].InterestPayment, 2, MidpointRounding.AwayFromZero) == 0 &&
                        Math.Round(outputGrid[i].ServiceFeeInterest, 2, MidpointRounding.AwayFromZero) == 0 &&
                        Math.Round(totalDue, 2, MidpointRounding.AwayFromZero) == 0)
                    {
                        startDeletionIndex = i;
                        break;
                    }
                }

                //This loop delete all the rows in decreasing order till startDeletionIndex.
                for (int i = outputGrid.Count - 1; i >= startDeletionIndex; i--)
                {
                    outputGrid.RemoveAt(i);
                }

                //This loop is used to extand the repayment schedule after the last date of payment, when extanded dates are available, and there is some amount to be paid.
                for (int j = additionalPaymentCurrentRow; j < scheduleInput.AdditionalPaymentRecords.Count; j++)
                {
                    if (((Math.Round((outputGrid[outputGrid.Count - 1].BeginningPrincipal - outputGrid[outputGrid.Count - 1].PrincipalPaid), 2, MidpointRounding.AwayFromZero) > 0) ||
                        (Math.Round((outputGrid[outputGrid.Count - 1].BeginningServiceFee - outputGrid[outputGrid.Count - 1].ServiceFeePaid), 2, MidpointRounding.AwayFromZero) > 0) ||
                        Math.Round(outputGrid[outputGrid.Count - 1].InterestCarryOver, 2, MidpointRounding.AwayFromZero) > 0 ||
                        Math.Round(outputGrid[outputGrid.Count - 1].serviceFeeInterestCarryOver, 2, MidpointRounding.AwayFromZero) > 0 ||
                        Math.Round(outputGrid[outputGrid.Count - 1].CumulativeTotalPastDue, 2, MidpointRounding.AwayFromZero) > 0) &&
                        (scheduleInput.AdditionalPaymentRecords[j].DateIn >= outputGrid[outputGrid.Count - 1].PaymentDate))
                    {
                        //This condition checks whether it is a principal only additonal payment.
                        if (scheduleInput.AdditionalPaymentRecords[j].Flags == 2)
                        {
                            sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : PayPrincipalOnlyAmount(outputGrid, scheduleInput, j, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, true, originationFee, sameDayFee);");
                            PayPrincipalOnlyAmount(outputGrid, scheduleInput, j, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, true, originationFee, sameDayFee);
                        }
                        //This condition checks whether it is a additional payment where paiyment amount will be distributed among components as per priorities.
                        else if (scheduleInput.AdditionalPaymentRecords[j].Flags == 3)
                        {
                            sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : PayAdditionalPayment(outputGrid, scheduleInput, j, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, true, originationFee, sameDayFee);");
                            PayAdditionalPayment(outputGrid, scheduleInput, j, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, true, originationFee, sameDayFee);
                        }
                        //This condition checks whether it is a discount.
                        else if (scheduleInput.AdditionalPaymentRecords[j].Flags == 5)
                        {
                            sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : AddDiscount(outputGrid, scheduleInput, j, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, true, originationFee, sameDayFee);");
                            AddDiscount(outputGrid, scheduleInput, j, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, true, originationFee, sameDayFee);
                        }
                        outputGridRowNumber++;
                    }
                }

                if (scheduleInput.EarlyPayoffDate.ToShortDateString() != Convert.ToDateTime("1/1/1900").ToShortDateString())
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : EarlyPayOff.CalculateEarlyPayOff(outputGrid, scheduleInput, originationFee, sameDayFee);");
                    EarlyPayOff.CalculateEarlyPayOff(outputGrid, scheduleInput,  originationFee, sameDayFee);
                }
                sbTracing.AppendLine("Exist:From class ActualPaymentAllocation and method name AllocationOfFunds(List<OutputGrid> outputGrid, LoanDetails scheduleInput, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double loanAmountCalc, double sameDayFee)");
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }

        #region common Methods

        /// <summary>
        /// This function determine the oldest bucket row number which is either outstanding, or some of NSF/Late fee is due to pay.
        /// </summary>
        /// <param name="outputGrid"></param>
        /// <param name="currentRow"></param>
        /// <param name="latestSatisfiedRow"></param>
        /// <param name="searchType"></param>
        /// <returns></returns>
        private static int OldestOutStandingBucket(List<OutputGrid> outputGrid, int currentRow, int latestSatisfiedRow, string searchType)
        {
            int bucketNumber = currentRow;
            for (int i = latestSatisfiedRow; i < currentRow; i++)
            {
                if (searchType == "OutStanding Bucket")
                {
                    if (outputGrid[i].BucketStatus == "OutStanding")
                    {
                        bucketNumber = i;
                        break;
                    }
                }
                else if (searchType == "NSF Fee")
                {
                    if (outputGrid[i].Flags == 11 && outputGrid[i].NSFFeePastDue != 0)
                    {
                        bucketNumber = i;
                        break;
                    }
                }
                else if (searchType == "Late Fee")
                {
                    if (outputGrid[i].Flags == 12 && outputGrid[i].LateFeePastDue != 0)
                    {
                        bucketNumber = i;
                        break;
                    }
                }
                else if (searchType == "Principal Only")
                {
                    if (outputGrid[i].PrincipalPastDue > 0)
                    {
                        bucketNumber = i;
                        break;
                    }
                }
            }
            return bucketNumber;
        }

        /// <summary>
        /// This function recalculates the output grid with remaining loan amount and service fee, if exists.
        /// </summary>
        /// <param name="remainingPrincipal"></param>
        /// <param name="remainingServiceFee"></param>
        /// <param name="outputGrid"></param>
        /// <param name="scheduleInput"></param>
        /// <param name="nextOutputRow"></param>
        /// <param name="defaultTotalAmountToPay"></param>
        /// <param name="defaultTotalServiceFeePayable"></param>
        /// <param name="originationFee"></param>
        /// <param name="sameDayFee"></param>
        private static void ReCalculateOutputGridValues(double remainingPrincipal, double remainingServiceFee, List<OutputGrid> outputGrid, LoanDetails scheduleInput,
                                            int nextOutputRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double sameDayFee)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name ReCalculateOutputGridValues(double remainingPrincipal, double remainingServiceFee, " +
                    "List<OutputGrid> outputGrid, LoanDetails scheduleInput, int nextOutputRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double sameDayFee).");
                sbTracing.AppendLine("Parameter values are : remainingPrincipal = " + remainingPrincipal + ", remainingServiceFee = " + remainingServiceFee + ", nextOutputRow = " + nextOutputRow +
                    ", defaultTotalAmountToPay = " + defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", originationFee = " + originationFee + ", sameDayFee = " + sameDayFee);

                remainingPrincipal = Math.Round(remainingPrincipal, 2, MidpointRounding.AwayFromZero);

                //Determine the last interest carry over, and service fee interest carry over value.
                double interestCarryOver = outputGrid[nextOutputRow == 0 ? 0 : nextOutputRow - 1].InterestCarryOver;
                double serviceFeeInterestCarryOver = outputGrid[nextOutputRow == 0 ? 0 : nextOutputRow - 1].AccruedServiceFeeInterestCarryOver;

                //Checks whether the previous scheduled event was an skipped payment or not.
                if (outputGrid[nextOutputRow == 0 ? 0 : nextOutputRow - 1].Flags == 4)
                {
                    //Add the interest payment of previous row to carry over, as it was not interest paid due to skipped payment.
                    interestCarryOver += outputGrid[nextOutputRow == 0 ? 0 : nextOutputRow - 1].InterestPayment;
                    //Add the service fee interest payment of previous row to service fee interest carry over, as it was not paid due to skipped payment.
                    serviceFeeInterestCarryOver += outputGrid[nextOutputRow == 0 ? 0 : nextOutputRow - 1].ServiceFeeInterest;
                }

                //Determine the origination fee and same day fee to be paid, which was not paid in first schedule due to skipping of schedule.
                if (scheduleInput.InputRecords[1].Flags == 4)
                {
                    bool useSameDayFee = true;
                    for (int i = 2; i <= scheduleInput.InputRecords.Count - 1; i++)
                    {
                        if (outputGrid.Count > nextOutputRow && (scheduleInput.InputRecords[i].DateIn <= outputGrid[nextOutputRow].DueDate) &&
                                    scheduleInput.InputRecords[i - 1].Flags == 0)
                        {
                            useSameDayFee = false;
                            break;
                        }
                    }

                    if (useSameDayFee)
                    {
                        //Determine the remaining amount to be paid by deducting the paid amount from original amount.
                        for (int i = 0; i < nextOutputRow; i++)
                        {
                            originationFee = (originationFee - outputGrid[i].OriginationFeePaid) < 0 ? 0 : (originationFee - outputGrid[i].OriginationFeePaid);
                            sameDayFee = (sameDayFee - outputGrid[i].SameDayFeePaid) < 0 ? 0 : (sameDayFee - outputGrid[i].SameDayFeePaid);
                        }

                        //Set the column value origination fee and same day fee in current schedule to be paid.
                        if (nextOutputRow < outputGrid.Count)
                        {
                            outputGrid[nextOutputRow].OriginationFee = originationFee;
                            outputGrid[nextOutputRow].SameDayFee = sameDayFee;
                        }
                    }
                }

                //This loop recreates the repayment schedules starting from the next schedule date for which payment will be paid.
                for (int i = nextOutputRow; i < outputGrid.Count; i++)
                {
                    double interestPayment = 0;
                    double totalAmountToPay = defaultTotalAmountToPay;
                    double totalServiceFeePayable = defaultTotalServiceFeePayable;
                    double dailyInterestRate = 0;
                    double dailyInterestAmount = 0;
                    double periodicInterestRate = 0;
                    for (int j = 1; j < scheduleInput.InputRecords.Count; j++)
                    {
                        //This condition checks whether the next schedule date is equals to the schedule input dates.
                        if (scheduleInput.InputRecords[j].DateIn == outputGrid[i].DueDate)
                        {
                            DateTime startDate = DateTime.MinValue;
                            int row = i - 1;
                            //This loop determines the row which is neither the NSF fee event nor Late fee event.
                            while (row >= 0)
                            {
                                if (outputGrid[row].Flags != 11 && outputGrid[row].Flags != 12)
                                {
                                    break;
                                }
                                row--;
                            }

                            //Checks if the row which is neither NSF fee nor late fee event, doesn't exists.
                            if (row == -1)
                            {
                                startDate = scheduleInput.InputRecords[j - 1].EffectiveDate == DateTime.MinValue ?
                                                    scheduleInput.InputRecords[j - 1].DateIn : scheduleInput.InputRecords[j - 1].EffectiveDate;
                            }
                            else
                            {
                                startDate = outputGrid[row].PaymentDate;
                            }

                            //This condition checks whether a manual interest rate for that schedule payment is provided or not.
                            if (string.IsNullOrEmpty(scheduleInput.InputRecords[j].InterestRate))
                            {
                                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateOutPutGridValues() method. Calling method : PeriodicInterestRate.GetPeriodicInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, outputGrid[i].PaymentDate, Convert.ToDouble(scheduleInput.InterestRate));");
                                periodicInterestRate = PeriodicInterestRate.GetPeriodicInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, outputGrid[i].PaymentDate, Convert.ToDouble(scheduleInput.InterestRate));
                            }
                            else
                            {
                                //This condition checks whether interest rate is 0 for that period or not. If it is 0, then total payment amount will be loan amount divided
                                //by number of payments.
                                if (scheduleInput.InputRecords[j].InterestRate == "0")
                                {
                                    totalAmountToPay = outputGrid[0].BeginningPrincipal / (scheduleInput.InputRecords.Count - 1);

                                    double principalLoanAmount = (scheduleInput.LoanAmount > 0 && scheduleInput.AmountFinanced == 0) ? scheduleInput.LoanAmount : scheduleInput.AmountFinanced;
                                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateOutPutGridValues() method. Calling method : PrincipalServiceFee.CalculatePrincipalServiceeFee(scheduleInput.ApplyServiceFeeInterest, principalAmount, scheduleInput.ServiceFee);");
                                    double principalServiceFee = scheduleInput.IsServiceFeeFinanced ? 0 : PrincipalServiceFee.CalculatePrincipalServiceeFee(scheduleInput.ApplyServiceFeeInterest, principalLoanAmount, scheduleInput.ServiceFee);

                                    totalServiceFeePayable = principalServiceFee / (scheduleInput.InputRecords.Count - 1);
                                }
                                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateOutPutGridValues() method. Calling method : PeriodicInterestRate.GetPeriodicInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, outputGrid[i].PaymentDate, Convert.ToDouble(scheduleInput.InputRecords[j].InterestRate));");
                                periodicInterestRate = PeriodicInterestRate.GetPeriodicInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, outputGrid[i].PaymentDate, Convert.ToDouble(scheduleInput.InputRecords[j].InterestRate));
                            }
                            //This variable calculates the daily interest rate for the period.
                            sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateOutPutGridValues() method. Calling method : DailyInterestRate.CalculateDailyInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, outputGrid[i].PaymentDate, periodicInterestRate);");
                            dailyInterestRate = DailyInterestRate.CalculateDailyInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, outputGrid[i].PaymentDate, periodicInterestRate);

                            //This variable calculates the daily interest amount for the period.
                            sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateOutPutGridValues() method. Calling method : DailyInterestAmount.CalculateDailyInterestAmount(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, outputGrid[i].PaymentDate, periodicInterestRate * remainingPrincipal);");
                            dailyInterestAmount = DailyInterestAmount.CalculateDailyInterestAmount(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, outputGrid[i].PaymentDate, periodicInterestRate * remainingPrincipal);
                            dailyInterestAmount = Math.Round(dailyInterestAmount, 2, MidpointRounding.AwayFromZero);

                            break;
                        }
                    }

                    //This variable determines the principal amount to pay for particular period by subtracting the periodic interest from total amount.
                    double principalAmountToPay = 0;
                    //This variable is the periodic interest of that period. It is multiplication of periodic rate and remaining principal amount.
                    double interestAccrued = periodicInterestRate * remainingPrincipal;
                    interestAccrued = Math.Round(interestAccrued, 2, MidpointRounding.AwayFromZero);

                    //This condition determine whether the row is either last row. 
                    //If no, the carry over of interest will be added to interest accrued for current schedule, and limit to total payment. If it last payment schedule, 
                    //all remaining principal amount will be added to total amount to be paid.
                    if ((i != outputGrid.Count - 1))
                    {
                        if ((interestAccrued + interestCarryOver) >= totalAmountToPay)
                        {
                            interestCarryOver = Math.Round(((interestAccrued + interestCarryOver) - totalAmountToPay), 2, MidpointRounding.AwayFromZero);
                            interestPayment = totalAmountToPay;
                            principalAmountToPay = 0;
                        }
                        else
                        {
                            interestPayment = (interestAccrued + interestCarryOver);
                            if ((totalAmountToPay - (interestAccrued + interestCarryOver)) > remainingPrincipal)
                            {
                                principalAmountToPay = remainingPrincipal;
                                totalAmountToPay = principalAmountToPay + interestPayment;
                            }
                            else
                            {
                                principalAmountToPay = totalAmountToPay - (interestAccrued + interestCarryOver);
                            }
                            interestCarryOver = 0;
                            principalAmountToPay = Math.Round(principalAmountToPay, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    else
                    {
                        principalAmountToPay = Math.Round(remainingPrincipal, 2, MidpointRounding.AwayFromZero);
                        interestPayment = (interestAccrued + interestCarryOver);
                        totalAmountToPay = principalAmountToPay + interestPayment;
                        interestCarryOver = 0;
                    }

                    //determine the service fee, service fee interest to be paid in each period, and calculate the remaining principal service fee on which service fee
                    //interest is being calculated.
                    double payableServiceFee = 0, payableServiceFeeInterest = 0, accruedServiceFeeInterest = 0;
                    if (!scheduleInput.IsServiceFeeFinanced)
                    {
                        if (scheduleInput.ApplyServiceFeeInterest == 1 || scheduleInput.ApplyServiceFeeInterest == 3)
                        {
                            accruedServiceFeeInterest = remainingServiceFee * periodicInterestRate;
                            accruedServiceFeeInterest = Math.Round(accruedServiceFeeInterest, 2, MidpointRounding.AwayFromZero);
                        }

                        //Checks whether the row is last row of output grid or payable loan amount will become 0 after this row.
                        if (((i == outputGrid.Count - 1) || (remainingPrincipal - principalAmountToPay) <= 0))
                        {
                            payableServiceFee = Math.Round(remainingServiceFee, 2, MidpointRounding.AwayFromZero);
                            payableServiceFeeInterest = (accruedServiceFeeInterest + serviceFeeInterestCarryOver);
                            totalServiceFeePayable = payableServiceFee + payableServiceFeeInterest;
                            serviceFeeInterestCarryOver = 0;
                        }
                        else if ((i != outputGrid.Count - 1))
                        {
                            if ((accruedServiceFeeInterest + serviceFeeInterestCarryOver) >= totalServiceFeePayable)
                            {
                                serviceFeeInterestCarryOver = Math.Round(((accruedServiceFeeInterest + serviceFeeInterestCarryOver) - totalServiceFeePayable), 2, MidpointRounding.AwayFromZero);
                                payableServiceFeeInterest = totalServiceFeePayable;
                                payableServiceFee = 0;
                            }
                            else
                            {
                                payableServiceFeeInterest = (accruedServiceFeeInterest + serviceFeeInterestCarryOver);
                                if ((totalServiceFeePayable - (accruedServiceFeeInterest + serviceFeeInterestCarryOver)) > remainingServiceFee)
                                {
                                    payableServiceFee = remainingServiceFee;
                                    totalServiceFeePayable = payableServiceFee + payableServiceFeeInterest;
                                }
                                else
                                {
                                    payableServiceFee = totalServiceFeePayable - payableServiceFeeInterest;
                                }
                                serviceFeeInterestCarryOver = 0;
                                payableServiceFee = Math.Round(payableServiceFee, 2, MidpointRounding.AwayFromZero);
                            }
                        }
                    }

                    //This variable calculates the total amount payable for the particular period.
                    totalAmountToPay += totalServiceFeePayable + Math.Round(outputGrid[i].MaintenanceFee, 2, MidpointRounding.AwayFromZero) +
                                                Math.Round(outputGrid[i].ManagementFee, 2, MidpointRounding.AwayFromZero) +
                                                Math.Round(outputGrid[i].OriginationFee, 2, MidpointRounding.AwayFromZero) +
                                                Math.Round(outputGrid[i].SameDayFee, 2, MidpointRounding.AwayFromZero);

                    //After calculations assign all the values in the output grid.
                    outputGrid[i].BeginningPrincipal = Math.Round(remainingPrincipal, 2, MidpointRounding.AwayFromZero);
                    outputGrid[i].BeginningServiceFee = Math.Round(remainingServiceFee, 2, MidpointRounding.AwayFromZero);
                    outputGrid[i].PeriodicInterestRate = periodicInterestRate;
                    outputGrid[i].DailyInterestRate = dailyInterestRate;
                    outputGrid[i].DailyInterestAmount = dailyInterestAmount;
                    outputGrid[i].InterestAccrued = Math.Round(interestAccrued, 2, MidpointRounding.AwayFromZero);
                    outputGrid[i].InterestCarryOver = Math.Round(interestCarryOver, 2, MidpointRounding.AwayFromZero);
                    outputGrid[i].InterestPayment = Math.Round(interestPayment, 2, MidpointRounding.AwayFromZero);
                    outputGrid[i].PrincipalPayment = Math.Round(principalAmountToPay, 2, MidpointRounding.AwayFromZero);
                    outputGrid[i].TotalPayment = Math.Round(totalAmountToPay, 2, MidpointRounding.AwayFromZero);
                    outputGrid[i].AccruedServiceFeeInterest = Math.Round(accruedServiceFeeInterest, 2, MidpointRounding.AwayFromZero);
                    outputGrid[i].AccruedServiceFeeInterestCarryOver = Math.Round(serviceFeeInterestCarryOver, 2, MidpointRounding.AwayFromZero);
                    outputGrid[i].ServiceFee = Math.Round(payableServiceFee, 2, MidpointRounding.AwayFromZero);
                    outputGrid[i].ServiceFeeInterest = Math.Round(payableServiceFeeInterest, 2, MidpointRounding.AwayFromZero);
                    outputGrid[i].ServiceFeeTotal = Math.Round(totalServiceFeePayable, 2, MidpointRounding.AwayFromZero);
                    //Past due amount columns
                    outputGrid[i].PrincipalPastDue = 0;
                    outputGrid[i].InterestPastDue = 0;
                    outputGrid[i].ServiceFeePastDue = 0;
                    outputGrid[i].ServiceFeeInterestPastDue = 0;
                    outputGrid[i].OriginationFeePastDue = 0;
                    outputGrid[i].MaintenanceFeePastDue = 0;
                    outputGrid[i].ManagementFeePastDue = 0;
                    outputGrid[i].SameDayFeePastDue = 0;
                    outputGrid[i].NSFFeePastDue = 0;
                    outputGrid[i].LateFeePastDue = 0;
                    outputGrid[i].TotalPastDue = 0;
                    //Cumulative past due amount columns
                    if (i != nextOutputRow)
                    {
                        outputGrid[i].CumulativePrincipalPastDue = 0;
                        outputGrid[i].CumulativeInterestPastDue = 0;
                        outputGrid[i].CumulativeServiceFeePastDue = 0;
                        outputGrid[i].CumulativeServiceFeeInterestPastDue = 0;
                        outputGrid[i].CumulativeOriginationFeePastDue = 0;
                        outputGrid[i].CumulativeMaintenanceFeePastDue = 0;
                        outputGrid[i].CumulativeManagementFeePastDue = 0;
                        outputGrid[i].CumulativeSameDayFeePastDue = 0;
                        outputGrid[i].CumulativeNSFFeePastDue = 0;
                        outputGrid[i].CumulativeLateFeePastDue = 0;
                        outputGrid[i].CumulativeTotalPastDue = 0;
                    }
                    outputGrid[i].BucketStatus = "OutStanding";

                    //Reduce the service fee amount that remains to be paid after current schedule.
                    remainingServiceFee = (remainingServiceFee - payableServiceFee) <= 0 ? 0 : (remainingServiceFee - payableServiceFee);
                    remainingServiceFee = Math.Round(remainingServiceFee, 2, MidpointRounding.AwayFromZero);

                    //Reduce the principal amount that remains to pay after a particular period payment.
                    remainingPrincipal -= principalAmountToPay;
                    remainingPrincipal = Math.Round(remainingPrincipal, 2, MidpointRounding.AwayFromZero);
                }
                sbTracing.AppendLine("Exist:From class ActualPaymentAllocation and method name ReCalculateOutputGridValues(double remainingPrincipal, double remainingServiceFee, " +
                    "List<OutputGrid> outputGrid, LoanDetails scheduleInput, int nextOutputRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, " +
                    "double sameDayFee)");
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }

        #endregion

        #region For Paid, missed, and adjustment of payment

        /// <summary>
        /// This function allocates the funds first to the oldest outstanding bucket in priority order, then remaining amount will be allocated to current bucket.
        /// </summary>
        /// <param name="outputGrid"></param>
        /// <param name="scheduleInput"></param>
        /// <param name="currentInputGridRow"></param>
        /// <param name="defaultTotalAmountToPay"></param>
        /// <param name="defaultTotalServiceFeePayable"></param>
        /// <param name="currentOutputGridRow"></param>
        /// <param name="originationFee"></param>
        /// <param name="sameDayFee"></param>
        private static void AllocateFundToBuckets(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int currentInputGridRow, double defaultTotalAmountToPay,
                                                        double defaultTotalServiceFeePayable, int currentOutputGridRow, double originationFee, double sameDayFee)
        {
            int previousOutStandingBucketNumber = 0;
            bool isLastRow = false;
            bool isPrincipalEqual = false;
            bool isServiceFeeEqual = false;
            double interestPaidDuePaidInLastSchedule = 0, serviceFeeInterestPaidDuePaidInLastSchedule = 0, otherRemainingDueInLastSchedule = 0;

            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name AllocateFundToBuckets(List<OutputGrid> outputGrid, LoanDetails scheduleInput," +
                    " int currentInputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, int currentOutputGridRow, double originationFee, double sameDayFee).");
                sbTracing.AppendLine("Parameter values are : currentInputGridRow = " + currentInputGridRow + ", defaultTotalAmountToPay = " + defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " +
                    defaultTotalServiceFeePayable + ", currentOutputGridRow = " + currentOutputGridRow + ", originationFee = " + originationFee + ", sameDayFee = " + sameDayFee);

                double pastDueAmount = 0;
                for (int i = 0; i < currentOutputGridRow; i++)
                {
                    pastDueAmount = pastDueAmount + outputGrid[i].TotalPastDue - outputGrid[i].PrincipalPastDue - outputGrid[i].ServiceFeePastDue;
                }

                double totalAmountToPay = defaultTotalAmountToPay;
                if (scheduleInput.InputRecords[currentInputGridRow].InterestRate == "0")
                {
                    totalAmountToPay = outputGrid[0].BeginningPrincipal / (scheduleInput.InputRecords.Count - 1);
                }

                //Checks whether the row is the last row of the output grid.
                if ((outputGrid.Count == currentOutputGridRow + 1) ||
                        ((Math.Round(outputGrid[currentOutputGridRow].PrincipalPayment + outputGrid[currentOutputGridRow].InterestPayment, 2, MidpointRounding.AwayFromZero) < totalAmountToPay) &&
                            (string.IsNullOrEmpty(scheduleInput.InputRecords[currentInputGridRow].PaymentAmount) ||
                            (((outputGrid[currentOutputGridRow].TotalPayment + pastDueAmount) <= Convert.ToDouble(scheduleInput.InputRecords[currentInputGridRow].PaymentAmount)) &&
                            !string.IsNullOrEmpty(scheduleInput.InputRecords[currentInputGridRow].PaymentAmount)))))
                {
                    isLastRow = true;
                }

                #region This condition determines whether it is the last row of output grid.
                if (isLastRow)
                {
                    outputGrid[currentOutputGridRow].NSFFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeNSFFeePastDue;
                    outputGrid[currentOutputGridRow].LateFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeLateFeePastDue;
                    interestPaidDuePaidInLastSchedule = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeInterestPastDue;
                    serviceFeeInterestPaidDuePaidInLastSchedule = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeServiceFeeInterestPastDue;
                    otherRemainingDueInLastSchedule = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeMaintenanceFeePastDue +
                                                        outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeManagementFeePastDue +
                                                        outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeOriginationFeePastDue +
                                                        outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeSameDayFeePastDue +
                                                        outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeLateFeePastDue +
                                                        outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeNSFFeePastDue;

                    //Add the total past due values in the payable amounts to be paid in the last row of output grid.
                    outputGrid[currentOutputGridRow].InterestPayment += interestPaidDuePaidInLastSchedule;
                    outputGrid[currentOutputGridRow].ServiceFeeTotal += serviceFeeInterestPaidDuePaidInLastSchedule;
                    outputGrid[currentOutputGridRow].ServiceFeeInterest += serviceFeeInterestPaidDuePaidInLastSchedule;

                    outputGrid[currentOutputGridRow].TotalPayment += interestPaidDuePaidInLastSchedule + serviceFeeInterestPaidDuePaidInLastSchedule + otherRemainingDueInLastSchedule;
                }
                #endregion

                //Determine the payment amount of the schedule that will be applied all the components as per priority values.
                double paymentAmount = string.IsNullOrEmpty(scheduleInput.InputRecords[currentInputGridRow].PaymentAmount) ?
                                        outputGrid[currentOutputGridRow].TotalPayment : Convert.ToDouble(scheduleInput.InputRecords[currentInputGridRow].PaymentAmount);

                outputGrid[currentOutputGridRow].PrincipalPastDue = outputGrid[currentOutputGridRow].PrincipalPayment;
                outputGrid[currentOutputGridRow].ServiceFeePastDue = outputGrid[currentOutputGridRow].ServiceFee;
                outputGrid[currentOutputGridRow].InterestPastDue = outputGrid[currentOutputGridRow].InterestPayment;
                outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue = outputGrid[currentOutputGridRow].ServiceFeeInterest;
                outputGrid[currentOutputGridRow].OriginationFeePastDue = outputGrid[currentOutputGridRow].OriginationFee;
                outputGrid[currentOutputGridRow].MaintenanceFeePastDue = outputGrid[currentOutputGridRow].MaintenanceFee;
                outputGrid[currentOutputGridRow].ManagementFeePastDue = outputGrid[currentOutputGridRow].ManagementFee;
                outputGrid[currentOutputGridRow].SameDayFeePastDue = outputGrid[currentOutputGridRow].SameDayFee;

                #region Amount allocation in outstanding Buckets before the current bucket

                while (previousOutStandingBucketNumber < currentOutputGridRow && paymentAmount > 0)
                {
                    //Determine the oldest outstanding bucket index in output grid.
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocateFundToBuckets() method. Calling method : OldestOutStandingBucket(outputGrid, currentOutputGridRow, previousOutStandingBucketNumber, 'OutStanding Bucket');");
                    int bucketNumber = OldestOutStandingBucket(outputGrid, currentOutputGridRow, previousOutStandingBucketNumber, "OutStanding Bucket");

                    //If oldest outstanding bucket is cuurent bucket, then break from the loop.
                    if (bucketNumber == currentOutputGridRow)
                    {
                        break;
                    }

                    if (Math.Round(outputGrid[currentOutputGridRow].BeginningPrincipal, 2, MidpointRounding.AwayFromZero) ==
                                            Math.Round(outputGrid[bucketNumber].PrincipalPastDue, 2, MidpointRounding.AwayFromZero))
                    {
                        isPrincipalEqual = true;
                    }
                    if (Math.Round(outputGrid[currentOutputGridRow].BeginningServiceFee, 2, MidpointRounding.AwayFromZero) ==
                                            Math.Round(outputGrid[bucketNumber].ServiceFeePastDue, 2, MidpointRounding.AwayFromZero))
                    {
                        isServiceFeeEqual = true;
                    }

                    //Checks if total past due amount of oldest outstanding bucket is less than or equal to the payment amount.
                    if (outputGrid[bucketNumber].TotalPastDue <= paymentAmount)
                    {
                        //Paid amount is added in current payment row
                        outputGrid[currentOutputGridRow].PrincipalPaid += outputGrid[bucketNumber].PrincipalPastDue;
                        outputGrid[currentOutputGridRow].InterestPaid += outputGrid[bucketNumber].InterestPastDue;
                        outputGrid[currentOutputGridRow].ServiceFeePaid += outputGrid[bucketNumber].ServiceFeePastDue;
                        outputGrid[currentOutputGridRow].ServiceFeeInterestPaid += outputGrid[bucketNumber].ServiceFeeInterestPastDue;
                        outputGrid[currentOutputGridRow].OriginationFeePaid += outputGrid[bucketNumber].OriginationFeePastDue;
                        outputGrid[currentOutputGridRow].MaintenanceFeePaid += outputGrid[bucketNumber].MaintenanceFeePastDue;
                        outputGrid[currentOutputGridRow].ManagementFeePaid += outputGrid[bucketNumber].ManagementFeePastDue;
                        outputGrid[currentOutputGridRow].SameDayFeePaid += outputGrid[bucketNumber].SameDayFeePastDue;
                        outputGrid[currentOutputGridRow].TotalPaid += outputGrid[bucketNumber].TotalPastDue;

                        paymentAmount = Math.Round(paymentAmount - outputGrid[bucketNumber].TotalPastDue, 2, MidpointRounding.AwayFromZero);

                        for (int i = bucketNumber + 1; i <= currentOutputGridRow; i++)
                        {
                            if (isPrincipalEqual)
                            {
                                outputGrid[i].PrincipalPastDue = 0;
                            }
                            if (isServiceFeeEqual)
                            {
                                outputGrid[i].ServiceFeePastDue = 0;
                            }
                        }

                        //Set all past due balances to 0, in the oldest outstanding row.
                        outputGrid[bucketNumber].PrincipalPastDue = 0;
                        outputGrid[bucketNumber].InterestPastDue = 0;
                        outputGrid[bucketNumber].ServiceFeePastDue = 0;
                        outputGrid[bucketNumber].ServiceFeeInterestPastDue = 0;
                        outputGrid[bucketNumber].OriginationFeePastDue = 0;
                        outputGrid[bucketNumber].MaintenanceFeePastDue = 0;
                        outputGrid[bucketNumber].ManagementFeePastDue = 0;
                        outputGrid[bucketNumber].SameDayFeePastDue = 0;
                        outputGrid[bucketNumber].TotalPastDue = 0;

                        //Set all Cumulative past due balances of next row to 0.
                        for (int i = bucketNumber + 1; i <= currentOutputGridRow; i++)
                        {
                            outputGrid[i].CumulativePrincipalPastDue = Math.Round(outputGrid[i].CumulativePrincipalPastDue - outputGrid[bucketNumber].CumulativePrincipalPastDue, 2, MidpointRounding.AwayFromZero);
                            outputGrid[i].CumulativeInterestPastDue = Math.Round(outputGrid[i].CumulativeInterestPastDue - outputGrid[bucketNumber].CumulativeInterestPastDue, 2, MidpointRounding.AwayFromZero);
                            outputGrid[i].CumulativeServiceFeePastDue = Math.Round(outputGrid[i].CumulativeServiceFeePastDue - outputGrid[bucketNumber].CumulativeServiceFeePastDue, 2, MidpointRounding.AwayFromZero);
                            outputGrid[i].CumulativeServiceFeeInterestPastDue = Math.Round(outputGrid[i].CumulativeServiceFeeInterestPastDue - outputGrid[bucketNumber].CumulativeServiceFeeInterestPastDue, 2, MidpointRounding.AwayFromZero);
                            outputGrid[i].CumulativeOriginationFeePastDue = Math.Round(outputGrid[i].CumulativeOriginationFeePastDue - outputGrid[bucketNumber].CumulativeOriginationFeePastDue, 2, MidpointRounding.AwayFromZero);
                            outputGrid[i].CumulativeMaintenanceFeePastDue = Math.Round(outputGrid[i].CumulativeMaintenanceFeePastDue - outputGrid[bucketNumber].CumulativeMaintenanceFeePastDue, 2, MidpointRounding.AwayFromZero);
                            outputGrid[i].CumulativeManagementFeePastDue = Math.Round(outputGrid[i].CumulativeManagementFeePastDue - outputGrid[bucketNumber].CumulativeManagementFeePastDue, 2, MidpointRounding.AwayFromZero);
                            outputGrid[i].CumulativeSameDayFeePastDue = Math.Round(outputGrid[i].CumulativeSameDayFeePastDue - outputGrid[bucketNumber].CumulativeSameDayFeePastDue, 2, MidpointRounding.AwayFromZero);
                            outputGrid[i].CumulativeTotalPastDue -= (outputGrid[bucketNumber].CumulativePrincipalPastDue + outputGrid[bucketNumber].CumulativeInterestPastDue +
                                                        outputGrid[bucketNumber].CumulativeServiceFeePastDue + outputGrid[bucketNumber].CumulativeServiceFeeInterestPastDue +
                                                        outputGrid[bucketNumber].CumulativeOriginationFeePastDue + outputGrid[bucketNumber].CumulativeMaintenanceFeePastDue +
                                                        outputGrid[bucketNumber].CumulativeManagementFeePastDue + outputGrid[bucketNumber].CumulativeSameDayFeePastDue);
                            outputGrid[i].CumulativeTotalPastDue = Math.Round(outputGrid[i].CumulativeTotalPastDue, 2, MidpointRounding.AwayFromZero);
                        }

                        //Set all Cumulative past due balances to 0, in the oldest outstanding row.
                        outputGrid[bucketNumber].CumulativePrincipalPastDue = 0;
                        outputGrid[bucketNumber].CumulativeInterestPastDue = 0;
                        outputGrid[bucketNumber].CumulativeServiceFeePastDue = 0;
                        outputGrid[bucketNumber].CumulativeServiceFeeInterestPastDue = 0;
                        outputGrid[bucketNumber].CumulativeOriginationFeePastDue = 0;
                        outputGrid[bucketNumber].CumulativeMaintenanceFeePastDue = 0;
                        outputGrid[bucketNumber].CumulativeManagementFeePastDue = 0;
                        outputGrid[bucketNumber].CumulativeSameDayFeePastDue = 0;
                        outputGrid[bucketNumber].CumulativeTotalPastDue = outputGrid[bucketNumber].CumulativeNSFFeePastDue + outputGrid[bucketNumber].CumulativeLateFeePastDue;
                        outputGrid[bucketNumber].BucketStatus = "Satisfied";
                    }
                    else
                    {
                        for (int i = 1; i <= 10; i++)
                        {
                            //Allocate funds to interest amount
                            if (scheduleInput.InputRecords[currentInputGridRow].InterestPriority == i)
                            {
                                if (outputGrid[bucketNumber].InterestPastDue <= paymentAmount)
                                {
                                    outputGrid[currentOutputGridRow].InterestPaid += outputGrid[bucketNumber].InterestPastDue;
                                    paymentAmount -= outputGrid[bucketNumber].InterestPastDue;
                                    outputGrid[bucketNumber].InterestPastDue = 0;
                                    outputGrid[bucketNumber].CumulativeInterestPastDue = 0;
                                }
                                else
                                {
                                    outputGrid[currentOutputGridRow].InterestPaid += paymentAmount;
                                    outputGrid[bucketNumber].InterestPastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativeInterestPastDue -= paymentAmount;
                                    paymentAmount = 0;
                                    break;
                                }
                            }

                            //Allocate funds to principal amount
                            else if (scheduleInput.InputRecords[currentInputGridRow].PrincipalPriority == i)
                            {
                                double deductedAmount = 0;
                                if (outputGrid[bucketNumber].PrincipalPastDue <= paymentAmount)
                                {
                                    outputGrid[currentOutputGridRow].PrincipalPaid += outputGrid[bucketNumber].PrincipalPastDue;
                                    paymentAmount -= outputGrid[bucketNumber].PrincipalPastDue;
                                    deductedAmount = outputGrid[bucketNumber].PrincipalPastDue;
                                    outputGrid[bucketNumber].PrincipalPastDue = 0;
                                    outputGrid[bucketNumber].CumulativePrincipalPastDue = 0;
                                }
                                else
                                {
                                    outputGrid[currentOutputGridRow].PrincipalPaid += paymentAmount;
                                    outputGrid[bucketNumber].PrincipalPastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativePrincipalPastDue -= paymentAmount;
                                    deductedAmount = paymentAmount;
                                    paymentAmount = 0;
                                }

                                if (isPrincipalEqual)
                                {
                                    for (int j = bucketNumber + 1; j <= currentOutputGridRow; j++)
                                    {
                                        outputGrid[j].PrincipalPastDue -= deductedAmount;
                                    }
                                }

                                if (paymentAmount == 0)
                                {
                                    break;
                                }
                            }

                            //Allocate funds to Service Fee amount
                            else if (scheduleInput.InputRecords[currentInputGridRow].ServiceFeePriority == i)
                            {
                                double deductedAmount = 0;
                                if (outputGrid[bucketNumber].ServiceFeePastDue <= paymentAmount)
                                {
                                    outputGrid[currentOutputGridRow].ServiceFeePaid += outputGrid[bucketNumber].ServiceFeePastDue;
                                    paymentAmount -= outputGrid[bucketNumber].ServiceFeePastDue;
                                    deductedAmount = outputGrid[bucketNumber].PrincipalPastDue;
                                    outputGrid[bucketNumber].ServiceFeePastDue = 0;
                                    outputGrid[bucketNumber].CumulativeServiceFeePastDue = 0;
                                }
                                else
                                {
                                    outputGrid[currentOutputGridRow].ServiceFeePaid += paymentAmount;
                                    outputGrid[bucketNumber].ServiceFeePastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativeServiceFeePastDue -= paymentAmount;
                                    deductedAmount = paymentAmount;
                                    paymentAmount = 0;
                                }

                                if (isServiceFeeEqual)
                                {
                                    for (int j = bucketNumber + 1; j <= currentOutputGridRow; j++)
                                    {
                                        outputGrid[j].ServiceFeePastDue -= deductedAmount;
                                    }
                                }

                                if (paymentAmount == 0)
                                {
                                    break;
                                }
                            }

                            //Allocate funds to Service Fee Interest amount
                            else if (scheduleInput.InputRecords[currentInputGridRow].ServiceFeeInterestPriority == i)
                            {
                                if (outputGrid[bucketNumber].ServiceFeeInterestPastDue <= paymentAmount)
                                {
                                    outputGrid[currentOutputGridRow].ServiceFeeInterestPaid += outputGrid[bucketNumber].ServiceFeeInterestPastDue;
                                    paymentAmount -= outputGrid[bucketNumber].ServiceFeeInterestPastDue;
                                    outputGrid[bucketNumber].ServiceFeeInterestPastDue = 0;
                                    outputGrid[bucketNumber].CumulativeServiceFeeInterestPastDue = 0;
                                }
                                else
                                {
                                    outputGrid[currentOutputGridRow].ServiceFeeInterestPaid += paymentAmount;
                                    outputGrid[bucketNumber].ServiceFeeInterestPastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativeServiceFeeInterestPastDue -= paymentAmount;
                                    paymentAmount = 0;
                                    break;
                                }
                            }

                            //Allocate funds to Origination Fee amount
                            else if (scheduleInput.InputRecords[currentInputGridRow].OriginationFeePriority == i)
                            {
                                if (outputGrid[bucketNumber].OriginationFeePastDue <= paymentAmount)
                                {
                                    outputGrid[currentOutputGridRow].OriginationFeePaid += outputGrid[bucketNumber].OriginationFeePastDue;
                                    paymentAmount -= outputGrid[bucketNumber].OriginationFeePastDue;
                                    outputGrid[bucketNumber].OriginationFeePastDue = 0;
                                    outputGrid[bucketNumber].CumulativeOriginationFeePastDue = 0;
                                }
                                else
                                {
                                    outputGrid[currentOutputGridRow].OriginationFeePaid += paymentAmount;
                                    outputGrid[bucketNumber].OriginationFeePastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativeOriginationFeePastDue -= paymentAmount;
                                    paymentAmount = 0;
                                    break;
                                }
                            }

                            //Allocate funds to Management Fee amount
                            else if (scheduleInput.InputRecords[currentInputGridRow].ManagementFeePriority == i)
                            {
                                if (outputGrid[bucketNumber].ManagementFeePastDue <= paymentAmount)
                                {
                                    outputGrid[currentOutputGridRow].ManagementFeePaid += outputGrid[bucketNumber].ManagementFeePastDue;
                                    paymentAmount -= outputGrid[bucketNumber].ManagementFeePastDue;
                                    outputGrid[bucketNumber].ManagementFeePastDue = 0;
                                    outputGrid[bucketNumber].CumulativeManagementFeePastDue = 0;
                                }
                                else
                                {
                                    outputGrid[currentOutputGridRow].ManagementFeePaid += paymentAmount;
                                    outputGrid[bucketNumber].ManagementFeePastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativeManagementFeePastDue -= paymentAmount;
                                    paymentAmount = 0;
                                    break;
                                }
                            }

                            //Allocate funds to Maintenance Fee amount
                            else if (scheduleInput.InputRecords[currentInputGridRow].MaintenanceFeePriority == i)
                            {
                                if (outputGrid[bucketNumber].MaintenanceFeePastDue <= paymentAmount)
                                {
                                    outputGrid[currentOutputGridRow].MaintenanceFeePaid += outputGrid[bucketNumber].MaintenanceFeePastDue;
                                    paymentAmount -= outputGrid[bucketNumber].MaintenanceFeePastDue;
                                    outputGrid[bucketNumber].MaintenanceFeePastDue = 0;
                                    outputGrid[bucketNumber].CumulativeMaintenanceFeePastDue = 0;
                                }
                                else
                                {
                                    outputGrid[currentOutputGridRow].MaintenanceFeePaid += paymentAmount;
                                    outputGrid[bucketNumber].MaintenanceFeePastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativeMaintenanceFeePastDue -= paymentAmount;
                                    paymentAmount = 0;
                                    break;
                                }
                            }

                            //Allocate funds to Same Day Fee amount
                            else if (scheduleInput.InputRecords[currentInputGridRow].SameDayFeePriority == i)
                            {
                                if (outputGrid[bucketNumber].SameDayFeePastDue <= paymentAmount)
                                {
                                    outputGrid[currentOutputGridRow].SameDayFeePaid += outputGrid[bucketNumber].SameDayFeePastDue;
                                    paymentAmount -= outputGrid[bucketNumber].SameDayFeePastDue;
                                    outputGrid[bucketNumber].SameDayFeePastDue = 0;
                                    outputGrid[bucketNumber].CumulativeSameDayFeePastDue = 0;
                                }
                                else
                                {
                                    outputGrid[currentOutputGridRow].SameDayFeePaid += paymentAmount;
                                    outputGrid[bucketNumber].SameDayFeePastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativeSameDayFeePastDue -= paymentAmount;
                                    paymentAmount = 0;
                                    break;
                                }
                            }
                            paymentAmount = Math.Round(paymentAmount, 2, MidpointRounding.AwayFromZero);
                        }

                        //Recalculate the Cumulative past due column values from next row of oldest outstanding bucket to current bucket.
                        for (int i = bucketNumber + 1; i <= currentOutputGridRow; i++)
                        {
                            outputGrid[i].CumulativeInterestPastDue = outputGrid[i - 1].CumulativeInterestPastDue + outputGrid[i].InterestPastDue;
                            outputGrid[i].CumulativePrincipalPastDue = outputGrid[i - 1].CumulativePrincipalPastDue + outputGrid[i].PrincipalPastDue;
                            outputGrid[i].CumulativeServiceFeePastDue = outputGrid[i - 1].CumulativeServiceFeePastDue + outputGrid[i].ServiceFeePastDue;
                            outputGrid[i].CumulativeServiceFeeInterestPastDue = outputGrid[i - 1].CumulativeServiceFeeInterestPastDue + outputGrid[i].ServiceFeeInterestPastDue;
                            outputGrid[i].CumulativeOriginationFeePastDue = outputGrid[i - 1].CumulativeOriginationFeePastDue + outputGrid[i].OriginationFeePastDue;
                            outputGrid[i].CumulativeManagementFeePastDue = outputGrid[i - 1].CumulativeManagementFeePastDue + outputGrid[i].ManagementFeePastDue;
                            outputGrid[i].CumulativeMaintenanceFeePastDue = outputGrid[i - 1].CumulativeMaintenanceFeePastDue + outputGrid[i].MaintenanceFeePastDue;
                            outputGrid[i].CumulativeSameDayFeePastDue = outputGrid[i - 1].CumulativeSameDayFeePastDue + outputGrid[i].SameDayFeePastDue;
                            outputGrid[i].CumulativeTotalPastDue = outputGrid[i].CumulativePrincipalPastDue + outputGrid[i].CumulativeInterestPastDue +
                                                        outputGrid[i].CumulativeServiceFeePastDue + outputGrid[i].CumulativeServiceFeeInterestPastDue +
                                                        outputGrid[i].CumulativeOriginationFeePastDue + outputGrid[i].CumulativeMaintenanceFeePastDue +
                                                        outputGrid[i].CumulativeManagementFeePastDue + outputGrid[i].CumulativeSameDayFeePastDue +
                                                        outputGrid[i].CumulativeNSFFeePastDue + outputGrid[i].CumulativeLateFeePastDue;
                        }

                        outputGrid[currentOutputGridRow].TotalPaid = outputGrid[currentOutputGridRow].PrincipalPaid + outputGrid[currentOutputGridRow].InterestPaid +
                                                    outputGrid[currentOutputGridRow].ServiceFeePaid + outputGrid[currentOutputGridRow].ServiceFeeInterestPaid +
                                                    outputGrid[currentOutputGridRow].OriginationFeePaid + outputGrid[currentOutputGridRow].MaintenanceFeePaid +
                                                    outputGrid[currentOutputGridRow].ManagementFeePaid + outputGrid[currentOutputGridRow].SameDayFeePaid +
                                                    outputGrid[currentOutputGridRow].NSFFeePaid + outputGrid[currentOutputGridRow].LateFeePaid;
                        outputGrid[bucketNumber].TotalPastDue = outputGrid[bucketNumber].PrincipalPastDue + outputGrid[bucketNumber].InterestPastDue +
                                                        outputGrid[bucketNumber].ServiceFeePastDue + outputGrid[bucketNumber].ServiceFeeInterestPastDue +
                                                        outputGrid[bucketNumber].OriginationFeePastDue + outputGrid[bucketNumber].MaintenanceFeePastDue +
                                                        outputGrid[bucketNumber].ManagementFeePastDue + outputGrid[bucketNumber].SameDayFeePastDue +
                                                        outputGrid[bucketNumber].NSFFeePastDue + outputGrid[bucketNumber].LateFeePastDue;
                        outputGrid[bucketNumber].CumulativeTotalPastDue = outputGrid[bucketNumber].CumulativePrincipalPastDue + outputGrid[bucketNumber].CumulativeInterestPastDue +
                                                        outputGrid[bucketNumber].CumulativeServiceFeePastDue + outputGrid[bucketNumber].CumulativeServiceFeeInterestPastDue +
                                                        outputGrid[bucketNumber].CumulativeOriginationFeePastDue + outputGrid[bucketNumber].CumulativeMaintenanceFeePastDue +
                                                        outputGrid[bucketNumber].CumulativeManagementFeePastDue + outputGrid[bucketNumber].CumulativeSameDayFeePastDue +
                                                        outputGrid[bucketNumber].CumulativeNSFFeePastDue + outputGrid[bucketNumber].CumulativeLateFeePastDue;

                        outputGrid[bucketNumber].BucketStatus = "OutStanding";
                    }
                    previousOutStandingBucketNumber = bucketNumber;
                }

                #endregion

                #region Amount allocation for current bucket

                //Remaining funds are allocating to the current row of grid
                if (!isLastRow ? (paymentAmount >= outputGrid[currentOutputGridRow].TotalPayment) : (paymentAmount >= Math.Round(outputGrid[currentOutputGridRow].TotalPayment - outputGrid[currentOutputGridRow].TotalPaid, 2, MidpointRounding.AwayFromZero)))
                {
                    paymentAmount += outputGrid[currentOutputGridRow].TotalPaid;

                    //Paid amount is added in current payment row
                    if (isLastRow)
                    {
                        outputGrid[currentOutputGridRow].PrincipalPaid = outputGrid[currentOutputGridRow].PrincipalPayment;
                        outputGrid[currentOutputGridRow].ServiceFeePaid = outputGrid[currentOutputGridRow].ServiceFee;
                        outputGrid[currentOutputGridRow].InterestPaid = outputGrid[currentOutputGridRow].InterestPayment;
                        outputGrid[currentOutputGridRow].ServiceFeeInterestPaid = outputGrid[currentOutputGridRow].ServiceFeeInterest;
                        outputGrid[currentOutputGridRow].TotalPaid = outputGrid[currentOutputGridRow].TotalPayment;
                    }
                    else
                    {
                        outputGrid[currentOutputGridRow].PrincipalPaid += outputGrid[currentOutputGridRow].PrincipalPayment;
                        outputGrid[currentOutputGridRow].ServiceFeePaid += outputGrid[currentOutputGridRow].ServiceFee;
                        outputGrid[currentOutputGridRow].InterestPaid += outputGrid[currentOutputGridRow].InterestPayment;
                        outputGrid[currentOutputGridRow].ServiceFeeInterestPaid += outputGrid[currentOutputGridRow].ServiceFeeInterest;
                        outputGrid[currentOutputGridRow].TotalPaid += outputGrid[currentOutputGridRow].TotalPayment;
                    }
                    outputGrid[currentOutputGridRow].OriginationFeePaid += outputGrid[currentOutputGridRow].OriginationFee;
                    outputGrid[currentOutputGridRow].MaintenanceFeePaid += outputGrid[currentOutputGridRow].MaintenanceFee;
                    outputGrid[currentOutputGridRow].ManagementFeePaid += outputGrid[currentOutputGridRow].ManagementFee;
                    outputGrid[currentOutputGridRow].SameDayFeePaid += outputGrid[currentOutputGridRow].SameDayFee;

                    //Set all past due balances to 0, in the oldest outstanding row.
                    outputGrid[currentOutputGridRow].PrincipalPastDue = 0;
                    outputGrid[currentOutputGridRow].InterestPastDue = 0;
                    outputGrid[currentOutputGridRow].ServiceFeePastDue = 0;
                    outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue = 0;
                    outputGrid[currentOutputGridRow].OriginationFeePastDue = 0;
                    outputGrid[currentOutputGridRow].MaintenanceFeePastDue = 0;
                    outputGrid[currentOutputGridRow].ManagementFeePastDue = 0;
                    outputGrid[currentOutputGridRow].SameDayFeePastDue = 0;
                    outputGrid[currentOutputGridRow].TotalPastDue = 0;

                    //Set all Cumulative past due balances to 0, in the oldest outstanding row.
                    outputGrid[currentOutputGridRow].CumulativePrincipalPastDue = 0;
                    outputGrid[currentOutputGridRow].CumulativeInterestPastDue = 0;
                    outputGrid[currentOutputGridRow].CumulativeServiceFeePastDue = 0;
                    outputGrid[currentOutputGridRow].CumulativeServiceFeeInterestPastDue = 0;
                    outputGrid[currentOutputGridRow].CumulativeOriginationFeePastDue = 0;
                    outputGrid[currentOutputGridRow].CumulativeMaintenanceFeePastDue = 0;
                    outputGrid[currentOutputGridRow].CumulativeManagementFeePastDue = 0;
                    outputGrid[currentOutputGridRow].CumulativeSameDayFeePastDue = 0;
                    outputGrid[currentOutputGridRow].CumulativeTotalPastDue = outputGrid[currentOutputGridRow].CumulativeNSFFeePastDue + outputGrid[currentOutputGridRow].CumulativeLateFeePastDue;
                    outputGrid[currentOutputGridRow].BucketStatus = "Satisfied";

                    paymentAmount -= (outputGrid[currentOutputGridRow].PrincipalPaid + outputGrid[currentOutputGridRow].ServiceFeePaid +
                                        outputGrid[currentOutputGridRow].InterestPaid + outputGrid[currentOutputGridRow].ServiceFeeInterestPaid +
                                        outputGrid[currentOutputGridRow].OriginationFeePaid + outputGrid[currentOutputGridRow].MaintenanceFeePaid +
                                        outputGrid[currentOutputGridRow].ManagementFeePaid + outputGrid[currentOutputGridRow].SameDayFeePaid);
                }
                else
                {
                    //Set all past due balances of current row equals to original amount to be paid.
                    if (isLastRow)
                    {
                        outputGrid[currentOutputGridRow].PrincipalPastDue = outputGrid[currentOutputGridRow].PrincipalPayment - outputGrid[currentOutputGridRow].PrincipalPaid;
                        outputGrid[currentOutputGridRow].ServiceFeePastDue = outputGrid[currentOutputGridRow].ServiceFee - outputGrid[currentOutputGridRow].ServiceFeePaid;
                        outputGrid[currentOutputGridRow].InterestPastDue = outputGrid[currentOutputGridRow].InterestPayment - outputGrid[currentOutputGridRow].InterestPaid;
                        outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue = outputGrid[currentOutputGridRow].ServiceFeeInterest - outputGrid[currentOutputGridRow].ServiceFeeInterestPaid;
                    }

                    outputGrid[currentOutputGridRow].OriginationFeePastDue = outputGrid[currentOutputGridRow].OriginationFee;
                    outputGrid[currentOutputGridRow].MaintenanceFeePastDue = outputGrid[currentOutputGridRow].MaintenanceFee;
                    outputGrid[currentOutputGridRow].ManagementFeePastDue = outputGrid[currentOutputGridRow].ManagementFee;
                    outputGrid[currentOutputGridRow].SameDayFeePastDue = outputGrid[currentOutputGridRow].SameDayFee;

                    //Set all Cumulative past due balances of current row equals to current past due plus remaining cumulative past due amount to be paid.
                    outputGrid[currentOutputGridRow].CumulativePrincipalPastDue = outputGrid[currentOutputGridRow == 0 ? 0 : (currentOutputGridRow - 1)].CumulativePrincipalPastDue + outputGrid[currentOutputGridRow].PrincipalPastDue;
                    outputGrid[currentOutputGridRow].CumulativeInterestPastDue = outputGrid[currentOutputGridRow == 0 ? 0 : (currentOutputGridRow - 1)].CumulativeInterestPastDue + outputGrid[currentOutputGridRow].InterestPastDue;
                    outputGrid[currentOutputGridRow].CumulativeServiceFeePastDue = outputGrid[currentOutputGridRow == 0 ? 0 : (currentOutputGridRow - 1)].CumulativeServiceFeePastDue + outputGrid[currentOutputGridRow].ServiceFeePastDue;
                    outputGrid[currentOutputGridRow].CumulativeServiceFeeInterestPastDue = outputGrid[currentOutputGridRow == 0 ? 0 : (currentOutputGridRow - 1)].CumulativeServiceFeeInterestPastDue + outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue;
                    outputGrid[currentOutputGridRow].CumulativeOriginationFeePastDue = outputGrid[currentOutputGridRow == 0 ? 0 : (currentOutputGridRow - 1)].CumulativeOriginationFeePastDue + outputGrid[currentOutputGridRow].OriginationFeePastDue;
                    outputGrid[currentOutputGridRow].CumulativeMaintenanceFeePastDue = outputGrid[currentOutputGridRow == 0 ? 0 : (currentOutputGridRow - 1)].CumulativeMaintenanceFeePastDue + outputGrid[currentOutputGridRow].MaintenanceFeePastDue;
                    outputGrid[currentOutputGridRow].CumulativeManagementFeePastDue = outputGrid[currentOutputGridRow == 0 ? 0 : (currentOutputGridRow - 1)].CumulativeManagementFeePastDue + outputGrid[currentOutputGridRow].ManagementFeePastDue;
                    outputGrid[currentOutputGridRow].CumulativeSameDayFeePastDue = outputGrid[currentOutputGridRow == 0 ? 0 : (currentOutputGridRow - 1)].CumulativeSameDayFeePastDue + outputGrid[currentOutputGridRow].SameDayFeePastDue;
                    #region fund allocation basedon priority
                    for (int i = 1; i <= 10; i++)
                    {
                        if (paymentAmount <= 0)
                        {
                            break;  
                        }

                        //Allocate funds to interest amount
                        if (scheduleInput.InputRecords[currentInputGridRow].InterestPriority == i)
                        {
                            double interestAmount = isLastRow ? (outputGrid[currentOutputGridRow].InterestPayment - outputGrid[currentOutputGridRow].InterestPaid) :
                                                                                   outputGrid[currentOutputGridRow].InterestPayment;
                            if (interestAmount <= paymentAmount)
                            {
                                outputGrid[currentOutputGridRow].InterestPaid += interestAmount;
                                paymentAmount -= interestAmount;
                                outputGrid[currentOutputGridRow].InterestPastDue = 0;
                                outputGrid[currentOutputGridRow].CumulativeInterestPastDue = 0;
                            }
                            else
                            {
                                outputGrid[currentOutputGridRow].InterestPaid += paymentAmount;
                                outputGrid[currentOutputGridRow].InterestPastDue = Math.Round((outputGrid[currentOutputGridRow].InterestPastDue - paymentAmount), 2, MidpointRounding.AwayFromZero);
                                outputGrid[currentOutputGridRow].CumulativeInterestPastDue = Math.Round((outputGrid[currentOutputGridRow].CumulativeInterestPastDue - paymentAmount), 2, MidpointRounding.AwayFromZero);
                                paymentAmount = 0;
                                break;
                            }
                            outputGrid[currentOutputGridRow].InterestPaid = Math.Round(outputGrid[currentOutputGridRow].InterestPaid, 2, MidpointRounding.AwayFromZero);
                        }

                        //Allocate funds to principal amount
                        else if (scheduleInput.InputRecords[currentInputGridRow].PrincipalPriority == i)
                        {
                            double principalAmount = isLastRow ? (outputGrid[currentOutputGridRow].PrincipalPayment - outputGrid[currentOutputGridRow].PrincipalPaid) :
                                                                                   outputGrid[currentOutputGridRow].PrincipalPayment;
                            if (principalAmount <= paymentAmount)
                            {
                                outputGrid[currentOutputGridRow].PrincipalPaid += principalAmount;
                                paymentAmount -= principalAmount;
                                outputGrid[currentOutputGridRow].PrincipalPastDue = 0;
                                outputGrid[currentOutputGridRow].CumulativePrincipalPastDue = 0;
                            }
                            else
                            {
                                outputGrid[currentOutputGridRow].PrincipalPaid += paymentAmount;
                                outputGrid[currentOutputGridRow].PrincipalPastDue = Math.Round((outputGrid[currentOutputGridRow].PrincipalPastDue - paymentAmount), 2, MidpointRounding.AwayFromZero);
                                outputGrid[currentOutputGridRow].CumulativePrincipalPastDue = Math.Round((outputGrid[currentOutputGridRow].CumulativePrincipalPastDue - paymentAmount), 2, MidpointRounding.AwayFromZero);
                                paymentAmount = 0;
                                break;
                            }
                            outputGrid[currentOutputGridRow].PrincipalPaid = Math.Round(outputGrid[currentOutputGridRow].PrincipalPaid, 2, MidpointRounding.AwayFromZero);
                        }

                        //Allocate funds to Service Fee amount
                        else if (scheduleInput.InputRecords[currentInputGridRow].ServiceFeePriority == i)
                        {
                            double serviceFeeAmount = isLastRow ? (outputGrid[currentOutputGridRow].ServiceFee - outputGrid[currentOutputGridRow].ServiceFeePaid) :
                                                                                   outputGrid[currentOutputGridRow].ServiceFee;
                            if (serviceFeeAmount <= paymentAmount)
                            {
                                outputGrid[currentOutputGridRow].ServiceFeePaid += serviceFeeAmount;
                                paymentAmount -= serviceFeeAmount;
                                outputGrid[currentOutputGridRow].ServiceFeePastDue = 0;
                                outputGrid[currentOutputGridRow].CumulativeServiceFeePastDue = 0;
                            }
                            else
                            {
                                outputGrid[currentOutputGridRow].ServiceFeePaid += paymentAmount;
                                outputGrid[currentOutputGridRow].ServiceFeePastDue = Math.Round((outputGrid[currentOutputGridRow].ServiceFeePastDue - paymentAmount), 2, MidpointRounding.AwayFromZero);
                                outputGrid[currentOutputGridRow].CumulativeServiceFeePastDue = Math.Round((outputGrid[currentOutputGridRow].CumulativeServiceFeePastDue - paymentAmount), 2, MidpointRounding.AwayFromZero);
                                paymentAmount = 0;
                                break;
                            }
                            outputGrid[currentOutputGridRow].ServiceFeePaid = Math.Round(outputGrid[currentOutputGridRow].ServiceFeePaid, 2, MidpointRounding.AwayFromZero);
                        }

                        //Allocate funds to Service Fee Interest amount
                        else if (scheduleInput.InputRecords[currentInputGridRow].ServiceFeeInterestPriority == i)
                        {
                            double serviceFeeInterestAmount = isLastRow ? (outputGrid[currentOutputGridRow].ServiceFeeInterest - outputGrid[currentOutputGridRow].ServiceFeeInterestPaid) :
                                                                                   outputGrid[currentOutputGridRow].ServiceFeeInterest;
                            if (serviceFeeInterestAmount <= paymentAmount)
                            {
                                outputGrid[currentOutputGridRow].ServiceFeeInterestPaid += serviceFeeInterestAmount;
                                paymentAmount -= serviceFeeInterestAmount;
                                outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue = 0;
                                outputGrid[currentOutputGridRow].CumulativeServiceFeeInterestPastDue = 0;
                            }
                            else
                            {
                                outputGrid[currentOutputGridRow].ServiceFeeInterestPaid += paymentAmount;
                                outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue = Math.Round((outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue - paymentAmount), 2, MidpointRounding.AwayFromZero);
                                outputGrid[currentOutputGridRow].CumulativeServiceFeeInterestPastDue = Math.Round((outputGrid[currentOutputGridRow].CumulativeServiceFeeInterestPastDue - paymentAmount), 2, MidpointRounding.AwayFromZero);
                                paymentAmount = 0;
                                break;
                            }
                            outputGrid[currentOutputGridRow].ServiceFeeInterestPaid = Math.Round(outputGrid[currentOutputGridRow].ServiceFeeInterestPaid, 2, MidpointRounding.AwayFromZero);
                        }

                        //Allocate funds to Origination Fee amount
                        else if (scheduleInput.InputRecords[currentInputGridRow].OriginationFeePriority == i)
                        {
                            if (outputGrid[currentOutputGridRow].OriginationFee <= paymentAmount)
                            {
                                outputGrid[currentOutputGridRow].OriginationFeePaid += outputGrid[currentOutputGridRow].OriginationFee;
                                paymentAmount -= outputGrid[currentOutputGridRow].OriginationFee;
                                outputGrid[currentOutputGridRow].OriginationFeePastDue = 0;
                                outputGrid[currentOutputGridRow].CumulativeOriginationFeePastDue = 0;
                            }
                            else
                            {
                                outputGrid[currentOutputGridRow].OriginationFeePaid += paymentAmount;
                                outputGrid[currentOutputGridRow].OriginationFeePastDue = Math.Round((outputGrid[currentOutputGridRow].OriginationFeePastDue - paymentAmount), 2, MidpointRounding.AwayFromZero);
                                outputGrid[currentOutputGridRow].CumulativeOriginationFeePastDue = Math.Round((outputGrid[currentOutputGridRow].CumulativeOriginationFeePastDue - paymentAmount), 2, MidpointRounding.AwayFromZero);
                                paymentAmount = 0;
                                break;
                            }
                            outputGrid[currentOutputGridRow].OriginationFeePaid = Math.Round(outputGrid[currentOutputGridRow].OriginationFeePaid, 2, MidpointRounding.AwayFromZero);
                        }

                        //Allocate funds to Management Fee amount
                        else if (scheduleInput.InputRecords[currentInputGridRow].ManagementFeePriority == i)
                        {
                            if (outputGrid[currentOutputGridRow].ManagementFee <= paymentAmount)
                            {
                                outputGrid[currentOutputGridRow].ManagementFeePaid += outputGrid[currentOutputGridRow].ManagementFee;
                                paymentAmount -= outputGrid[currentOutputGridRow].ManagementFee;
                                outputGrid[currentOutputGridRow].ManagementFeePastDue = 0;
                                outputGrid[currentOutputGridRow].CumulativeManagementFeePastDue = 0;
                            }
                            else
                            {
                                outputGrid[currentOutputGridRow].ManagementFeePaid += paymentAmount;
                                outputGrid[currentOutputGridRow].ManagementFeePastDue = Math.Round((outputGrid[currentOutputGridRow].ManagementFeePastDue - paymentAmount), 2, MidpointRounding.AwayFromZero);
                                outputGrid[currentOutputGridRow].CumulativeManagementFeePastDue = Math.Round((outputGrid[currentOutputGridRow].CumulativeManagementFeePastDue - paymentAmount), 2, MidpointRounding.AwayFromZero);
                                paymentAmount = 0;
                                break;
                            }
                            outputGrid[currentOutputGridRow].ManagementFeePaid = Math.Round(outputGrid[currentOutputGridRow].ManagementFeePaid, 2, MidpointRounding.AwayFromZero);
                        }

                        //Allocate funds to Maintenance Fee amount
                        else if (scheduleInput.InputRecords[currentInputGridRow].MaintenanceFeePriority == i)
                        {
                            if (outputGrid[currentOutputGridRow].MaintenanceFee <= paymentAmount)
                            {
                                outputGrid[currentOutputGridRow].MaintenanceFeePaid += outputGrid[currentOutputGridRow].MaintenanceFee;
                                paymentAmount -= outputGrid[currentOutputGridRow].MaintenanceFee;
                                outputGrid[currentOutputGridRow].MaintenanceFeePastDue = 0;
                                outputGrid[currentOutputGridRow].CumulativeMaintenanceFeePastDue = 0;
                            }
                            else
                            {
                                outputGrid[currentOutputGridRow].MaintenanceFeePaid += paymentAmount;
                                outputGrid[currentOutputGridRow].MaintenanceFeePastDue = Math.Round((outputGrid[currentOutputGridRow].MaintenanceFeePastDue - paymentAmount), 2, MidpointRounding.AwayFromZero);
                                outputGrid[currentOutputGridRow].CumulativeMaintenanceFeePastDue = Math.Round((outputGrid[currentOutputGridRow].CumulativeMaintenanceFeePastDue - paymentAmount), 2, MidpointRounding.AwayFromZero);
                                paymentAmount = 0;
                                break;
                            }
                            outputGrid[currentOutputGridRow].MaintenanceFeePaid = Math.Round(outputGrid[currentOutputGridRow].MaintenanceFeePaid, 2, MidpointRounding.AwayFromZero);
                        }

                        //Allocate funds to Same Day Fee amount
                        else if (scheduleInput.InputRecords[currentInputGridRow].SameDayFeePriority == i)
                        {
                            if (outputGrid[currentOutputGridRow].SameDayFee <= paymentAmount)
                            {
                                outputGrid[currentOutputGridRow].SameDayFeePaid += outputGrid[currentOutputGridRow].SameDayFee;
                                paymentAmount -= outputGrid[currentOutputGridRow].SameDayFee;
                                outputGrid[currentOutputGridRow].SameDayFeePastDue = 0;
                                outputGrid[currentOutputGridRow].CumulativeSameDayFeePastDue = 0;
                            }
                            else
                            {
                                outputGrid[currentOutputGridRow].SameDayFeePaid += paymentAmount;
                                outputGrid[currentOutputGridRow].SameDayFeePastDue = Math.Round((outputGrid[currentOutputGridRow].SameDayFeePastDue - paymentAmount), 2, MidpointRounding.AwayFromZero);
                                outputGrid[currentOutputGridRow].CumulativeSameDayFeePastDue = Math.Round((outputGrid[currentOutputGridRow].CumulativeSameDayFeePastDue - paymentAmount), 2, MidpointRounding.AwayFromZero);
                                paymentAmount = 0;
                                break;
                            }
                            outputGrid[currentOutputGridRow].SameDayFeePaid = Math.Round(outputGrid[currentOutputGridRow].SameDayFeePaid, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    #endregion
                    outputGrid[currentOutputGridRow].TotalPastDue = outputGrid[currentOutputGridRow].PrincipalPastDue + outputGrid[currentOutputGridRow].InterestPastDue +
                                                        outputGrid[currentOutputGridRow].ServiceFeePastDue + outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue +
                                                        outputGrid[currentOutputGridRow].OriginationFeePastDue + outputGrid[currentOutputGridRow].MaintenanceFeePastDue +
                                                        outputGrid[currentOutputGridRow].ManagementFeePastDue + outputGrid[currentOutputGridRow].SameDayFeePastDue +
                                                        outputGrid[currentOutputGridRow].NSFFeePastDue + outputGrid[currentOutputGridRow].LateFeePastDue;
                    outputGrid[currentOutputGridRow].CumulativeTotalPastDue = outputGrid[currentOutputGridRow].CumulativePrincipalPastDue + outputGrid[currentOutputGridRow].CumulativeInterestPastDue +
                                                    outputGrid[currentOutputGridRow].CumulativeServiceFeePastDue + outputGrid[currentOutputGridRow].CumulativeServiceFeeInterestPastDue +
                                                    outputGrid[currentOutputGridRow].CumulativeOriginationFeePastDue + outputGrid[currentOutputGridRow].CumulativeMaintenanceFeePastDue +
                                                    outputGrid[currentOutputGridRow].CumulativeManagementFeePastDue + outputGrid[currentOutputGridRow].CumulativeSameDayFeePastDue +
                                                    outputGrid[currentOutputGridRow].CumulativeNSFFeePastDue + outputGrid[currentOutputGridRow].CumulativeLateFeePastDue;

                    outputGrid[currentOutputGridRow].BucketStatus = "OutStanding";
                }

                #endregion

                double remainingPrincipal = outputGrid[0].BeginningPrincipal;
                double principalServiceFee = outputGrid[0].BeginningServiceFee;

                //This loop determines the remaining principal loan amount and principal service fee amount ot be paid.
                for (int i = 0; i <= currentOutputGridRow; i++)
                {
                    remainingPrincipal -= outputGrid[i].PrincipalPaid;
                    principalServiceFee -= outputGrid[i].ServiceFeePaid;
                }
                remainingPrincipal = Math.Round(remainingPrincipal, 2, MidpointRounding.AwayFromZero);
                principalServiceFee = Math.Round(principalServiceFee, 2, MidpointRounding.AwayFromZero);

                //Check whether it is currently the last row of output grid, and there is some amount to be paid, and input grid contains dates after last date of output grid.
                if (isLastRow && (remainingPrincipal > 0 || principalServiceFee > 0 || outputGrid[outputGrid.Count - 1].InterestCarryOver > 0 ||
                        outputGrid[outputGrid.Count - 1].serviceFeeInterestCarryOver > 0 || outputGrid[outputGrid.Count - 1].CumulativeTotalPastDue > 0)
                        && scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].DateIn > outputGrid[currentOutputGridRow].DueDate)
                {
                    // Deduct the NSF and Late fee past due amounts from the last row of output grid as another row will be added.
                    outputGrid[currentOutputGridRow].TotalPayment = outputGrid[currentOutputGridRow].TotalPayment -
                                (outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeNSFFeePastDue +
                                outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeLateFeePastDue);
                    outputGrid[currentOutputGridRow].NSFFee = 0;
                    outputGrid[currentOutputGridRow].LateFee = 0;
                }

                //This condition checks whether there is any payment amount remains after pay down all the outstanding amount as well as current bucket.
                if (paymentAmount > 0)
                {
                    //Allocate the remaining amount to the action fees i.e. NSF fee and late fee.
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocateFundToBuckets() method. Calling method : AllocateRemainingFundsToNSFAndLateFee(outputGrid, scheduleInput, currentInputGridRowRow, currentOutputGridRow, false, ref paymentAmount);");
                    AllocateRemainingFundsToNSFAndLateFee(outputGrid, scheduleInput, currentInputGridRow, currentOutputGridRow, false, ref paymentAmount);

                    //Allocate the remaining amount to the principal amount and service fee as per their priorities.
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocateFundToBuckets() method. Calling method : AllocateRemainingFunds(outputGrid, scheduleInput, currentInputGridRowRow, currentOutputGridRow, paymentAmount, ref remainingPrincipal, ref principalServiceFee);");
                    AllocateRemainingFunds(outputGrid, scheduleInput, currentInputGridRow, currentOutputGridRow, paymentAmount, ref remainingPrincipal, ref principalServiceFee);
                }

                //Recalculate values in output grid as per remaining loan amount and service fee.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocateFundToBuckets() method. Calling method : ReCalculateOutputGridValues(remainingPrincipal, principalServiceFee, outputGrid, scheduleInput, currentOutputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);");
                ReCalculateOutputGridValues(remainingPrincipal, principalServiceFee, outputGrid, scheduleInput, currentOutputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);

                outputGrid[currentOutputGridRow].TotalPaid = outputGrid[currentOutputGridRow].PrincipalPaid + outputGrid[currentOutputGridRow].InterestPaid +
                                                        outputGrid[currentOutputGridRow].ServiceFeePaid + outputGrid[currentOutputGridRow].ServiceFeeInterestPaid +
                                                        outputGrid[currentOutputGridRow].OriginationFeePaid + outputGrid[currentOutputGridRow].MaintenanceFeePaid +
                                                        outputGrid[currentOutputGridRow].ManagementFeePaid + outputGrid[currentOutputGridRow].SameDayFeePaid +
                                                        outputGrid[currentOutputGridRow].NSFFeePaid + outputGrid[currentOutputGridRow].LateFeePaid;

                //Cumulative amount paid is added in current payament row
                outputGrid[currentOutputGridRow].CumulativePrincipal = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativePrincipal + outputGrid[currentOutputGridRow].PrincipalPaid;
                outputGrid[currentOutputGridRow].CumulativeInterest = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeInterest + outputGrid[currentOutputGridRow].InterestPaid;
                outputGrid[currentOutputGridRow].CumulativePayment = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativePayment + outputGrid[currentOutputGridRow].TotalPaid;
                outputGrid[currentOutputGridRow].CumulativeServiceFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeServiceFee + outputGrid[currentOutputGridRow].ServiceFeePaid;
                outputGrid[currentOutputGridRow].CumulativeServiceFeeInterest = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeServiceFeeInterest + outputGrid[currentOutputGridRow].ServiceFeeInterestPaid;
                outputGrid[currentOutputGridRow].CumulativeServiceFeeTotal = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeServiceFeeTotal +
                                                                    outputGrid[currentOutputGridRow].ServiceFeePaid + outputGrid[currentOutputGridRow].ServiceFeeInterestPaid;
                outputGrid[currentOutputGridRow].CumulativeOriginationFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeOriginationFee + outputGrid[currentOutputGridRow].OriginationFeePaid;
                outputGrid[currentOutputGridRow].CumulativeMaintenanceFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeMaintenanceFee + outputGrid[currentOutputGridRow].MaintenanceFeePaid;
                outputGrid[currentOutputGridRow].CumulativeManagementFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeManagementFee + outputGrid[currentOutputGridRow].ManagementFeePaid;
                outputGrid[currentOutputGridRow].CumulativeSameDayFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeSameDayFee + outputGrid[currentOutputGridRow].SameDayFeePaid;
                outputGrid[currentOutputGridRow].CumulativeNSFFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeNSFFee + outputGrid[currentOutputGridRow].NSFFeePaid;
                outputGrid[currentOutputGridRow].CumulativeLateFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeLateFee + outputGrid[currentOutputGridRow].LateFeePaid;
                outputGrid[currentOutputGridRow].CumulativeTotalFees = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeTotalFees +
                                outputGrid[currentOutputGridRow].ServiceFeePaid + outputGrid[currentOutputGridRow].ServiceFeeInterestPaid + outputGrid[currentOutputGridRow].OriginationFeePaid +
                                outputGrid[currentOutputGridRow].MaintenanceFeePaid + outputGrid[currentOutputGridRow].ManagementFeePaid + outputGrid[currentOutputGridRow].SameDayFeePaid +
                                outputGrid[currentOutputGridRow].NSFFeePaid + outputGrid[currentOutputGridRow].LateFeePaid;

                //Cumulative amount past due is added in current row
                if (!isLastRow)
                {
                    outputGrid[currentOutputGridRow + 1].CumulativePrincipalPastDue = outputGrid[currentOutputGridRow].CumulativePrincipalPastDue + outputGrid[currentOutputGridRow + 1].PrincipalPayment;
                    outputGrid[currentOutputGridRow + 1].CumulativeInterestPastDue = outputGrid[currentOutputGridRow].CumulativeInterestPastDue + outputGrid[currentOutputGridRow + 1].InterestPayment;
                    outputGrid[currentOutputGridRow + 1].CumulativeServiceFeePastDue = outputGrid[currentOutputGridRow].CumulativeServiceFeePastDue + outputGrid[currentOutputGridRow + 1].ServiceFee;
                    outputGrid[currentOutputGridRow + 1].CumulativeServiceFeeInterestPastDue = outputGrid[currentOutputGridRow].CumulativeServiceFeeInterestPastDue + outputGrid[currentOutputGridRow + 1].ServiceFeeInterest;
                    outputGrid[currentOutputGridRow + 1].CumulativeOriginationFeePastDue = outputGrid[currentOutputGridRow].CumulativeOriginationFeePastDue + outputGrid[currentOutputGridRow + 1].OriginationFee;
                    outputGrid[currentOutputGridRow + 1].CumulativeMaintenanceFeePastDue = outputGrid[currentOutputGridRow].CumulativeMaintenanceFeePastDue + outputGrid[currentOutputGridRow + 1].MaintenanceFee;
                    outputGrid[currentOutputGridRow + 1].CumulativeManagementFeePastDue = outputGrid[currentOutputGridRow].CumulativeManagementFeePastDue + outputGrid[currentOutputGridRow + 1].ManagementFee;
                    outputGrid[currentOutputGridRow + 1].CumulativeSameDayFeePastDue = outputGrid[currentOutputGridRow].CumulativeSameDayFeePastDue + outputGrid[currentOutputGridRow + 1].SameDayFee;
                    outputGrid[currentOutputGridRow + 1].CumulativeNSFFeePastDue = outputGrid[currentOutputGridRow].CumulativeNSFFeePastDue;
                    outputGrid[currentOutputGridRow + 1].CumulativeLateFeePastDue = outputGrid[currentOutputGridRow].CumulativeLateFeePastDue;
                    outputGrid[currentOutputGridRow + 1].CumulativeTotalPastDue = outputGrid[currentOutputGridRow].CumulativeTotalPastDue + outputGrid[currentOutputGridRow + 1].TotalPayment;
                }
                else
                {
                    outputGrid[currentOutputGridRow].BucketStatus = (outputGrid[currentOutputGridRow].CumulativeTotalPastDue != 0 ? "OutStanding" : "Satisfied");
                }
                sbTracing.AppendLine("Exist:From class ActualPaymentAllocation and method name AllocateFundToBuckets(List<OutputGrid> outputGrid, LoanDetails scheduleInput," +
                    " int currentInputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, int currentOutputGridRow, double originationFee, double sameDayFee)");
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }

        #endregion

        #region Additional Fund Allocation

        /// <summary>
        /// This function allocates the remaining fund to the NSF fee and Late Fee as per their priority order.
        /// </summary>
        /// <param name="outputGrid"></param>
        /// <param name="scheduleInput"></param>
        /// <param name="currentRow"></param>
        /// <param name="currentOutputRow"></param>
        /// <param name="isAdditionalPaymentAllocation"></param>
        /// <param name="paymentAmount"></param>
        private static void AllocateRemainingFundsToNSFAndLateFee(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int currentRow, int currentOutputRow, bool isAdditionalPaymentAllocation, ref double paymentAmount)
        {
            int previousOutStandingBucketNumber = 0;

            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name AllocateRemainingFundsToNSFAndLateFee(List<OutputGrid> outputGrid, LoanDetails " +
                    "scheduleInput, int currentRow, int currentOutputRow, bool isAdditionalPaymentAllocation, ref double paymentAmount).");
                sbTracing.AppendLine("Parameter values are : currentRow = " + currentRow + ", currentOutputRow = " + currentOutputRow + ", isAdditionalPaymentAllocation = " + isAdditionalPaymentAllocation + ", paymentAmount = " + paymentAmount);

                //This loop pay down all the nsf and late fee starting from first row to current row.
                while (previousOutStandingBucketNumber < currentOutputRow && paymentAmount > 0)
                {
                    paymentAmount = Math.Round(paymentAmount, 2, MidpointRounding.AwayFromZero);
                    int actionFeeRow = 0;

                    int nsfFeePriority = isAdditionalPaymentAllocation ? scheduleInput.AdditionalPaymentRecords[currentRow].NSFFeePriority :
                                                                        scheduleInput.InputRecords[currentRow].NSFFeePriority;
                    int lateFeePriroity = isAdditionalPaymentAllocation ? scheduleInput.AdditionalPaymentRecords[currentRow].LateFeePriority :
                                                                        scheduleInput.InputRecords[currentRow].LateFeePriority;

                    //checks whether the priority of NSF fee is less than the Late fee.
                    if (nsfFeePriority < lateFeePriroity)
                    {
                        //Determine the oldest index which has NSF fee past due in output grid.
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocateRemainingFundsToNSFAndLateFee() method. Calling method : OldestOutStandingBucket(outputGrid, currentOutputRow, 0, 'NSF Fee');");
                        actionFeeRow = OldestOutStandingBucket(outputGrid, currentOutputRow, 0, "NSF Fee");
                        //Allocate funds to NSF Fee amount
                        if (outputGrid[actionFeeRow].NSFFeePastDue <= paymentAmount)
                        {
                            outputGrid[currentOutputRow].NSFFeePaid += outputGrid[actionFeeRow].NSFFeePastDue;
                            paymentAmount -= outputGrid[actionFeeRow].NSFFeePastDue;

                            //This loop recalculate the cumulative past due amounts of NSF and late fee after processing the row.
                            for (int i = actionFeeRow; i <= currentOutputRow; i++)
                            {
                                outputGrid[i].CumulativeNSFFeePastDue = (outputGrid[i].CumulativeNSFFeePastDue < outputGrid[actionFeeRow].NSFFeePastDue ? 0 : outputGrid[i].CumulativeNSFFeePastDue - outputGrid[actionFeeRow].NSFFeePastDue);
                                outputGrid[i].CumulativeTotalPastDue = (outputGrid[i].CumulativeTotalPastDue < outputGrid[actionFeeRow].NSFFeePastDue ? 0 : outputGrid[i].CumulativeTotalPastDue - outputGrid[actionFeeRow].NSFFeePastDue);
                            }

                            outputGrid[actionFeeRow].NSFFeePastDue = 0;
                            outputGrid[actionFeeRow].TotalPastDue = 0;
                        }
                        else
                        {
                            outputGrid[currentOutputRow].NSFFeePaid += paymentAmount;
                            outputGrid[actionFeeRow].NSFFeePastDue -= paymentAmount;
                            outputGrid[actionFeeRow].TotalPastDue -= paymentAmount;

                            //This loop recalculate the cumulative past due amounts of NSF and late fee after processing the row.
                            for (int i = actionFeeRow; i <= currentOutputRow; i++)
                            {
                                outputGrid[i].CumulativeNSFFeePastDue = (outputGrid[i].CumulativeNSFFeePastDue < paymentAmount ? 0 : outputGrid[i].CumulativeNSFFeePastDue - paymentAmount);
                                outputGrid[i].CumulativeTotalPastDue = (outputGrid[i].CumulativeTotalPastDue < paymentAmount ? 0 : outputGrid[i].CumulativeTotalPastDue - paymentAmount);
                            }

                            paymentAmount = 0;
                        }

                        paymentAmount = Math.Round(paymentAmount, 2, MidpointRounding.AwayFromZero);
                        //Determine the oldest index which has Late fee past due in output grid.
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocateRemainingFundsToNSFAndLateFee() method. Calling method : OldestOutStandingBucket(outputGrid, currentOutputRow, 0, 'Late Fee');");
                        actionFeeRow = OldestOutStandingBucket(outputGrid, currentOutputRow, 0, "Late Fee");
                        //Allocate funds to Late Fee amount
                        if (outputGrid[actionFeeRow].LateFeePastDue <= paymentAmount)
                        {
                            outputGrid[currentOutputRow].LateFeePaid += outputGrid[actionFeeRow].LateFeePastDue;
                            paymentAmount -= outputGrid[actionFeeRow].LateFeePastDue;

                            //This loop recalculate the cumulative past due amounts of NSF and late fee after processing the row.
                            for (int i = actionFeeRow; i <= currentOutputRow; i++)
                            {
                                outputGrid[i].CumulativeLateFeePastDue = (outputGrid[i].CumulativeLateFeePastDue < outputGrid[actionFeeRow].LateFeePastDue ? 0 : outputGrid[i].CumulativeLateFeePastDue - outputGrid[actionFeeRow].LateFeePastDue);
                                outputGrid[i].CumulativeTotalPastDue = (outputGrid[i].CumulativeTotalPastDue < outputGrid[actionFeeRow].LateFeePastDue ? 0 : outputGrid[i].CumulativeTotalPastDue - outputGrid[actionFeeRow].LateFeePastDue);
                            }

                            outputGrid[actionFeeRow].LateFeePastDue = 0;
                            outputGrid[actionFeeRow].TotalPastDue = 0;
                        }
                        else
                        {
                            outputGrid[currentOutputRow].LateFeePaid += paymentAmount;
                            outputGrid[actionFeeRow].LateFeePastDue -= paymentAmount;
                            outputGrid[actionFeeRow].TotalPastDue -= paymentAmount;

                            //This loop recalculate the cumulative past due amounts of NSF and late fee after processing the row.
                            for (int i = actionFeeRow; i <= currentOutputRow; i++)
                            {
                                outputGrid[i].CumulativeLateFeePastDue = (outputGrid[i].CumulativeLateFeePastDue < paymentAmount ? 0 : outputGrid[i].CumulativeLateFeePastDue - paymentAmount);
                                outputGrid[i].CumulativeTotalPastDue = (outputGrid[i].CumulativeTotalPastDue < paymentAmount ? 0 : outputGrid[i].CumulativeTotalPastDue - paymentAmount);
                            }

                            paymentAmount = 0;
                        }
                        paymentAmount = Math.Round(paymentAmount, 2, MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        //Determine the oldest index which has Late fee past due in output grid.
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocateRemainingFundsToNSFAndLateFee() method. Calling method : OldestOutStandingBucket(outputGrid, currentOutputRow, 0, 'Late Fee');");
                        actionFeeRow = OldestOutStandingBucket(outputGrid, currentOutputRow, 0, "Late Fee");
                        //Allocate funds to Late Fee amount
                        if (outputGrid[actionFeeRow].LateFeePastDue <= paymentAmount)
                        {
                            outputGrid[currentOutputRow].LateFeePaid += outputGrid[actionFeeRow].LateFeePastDue;
                            paymentAmount -= outputGrid[actionFeeRow].LateFeePastDue;

                            //This loop recalculate the cumulative past due amounts of NSF and late fee after processing the row.
                            for (int i = actionFeeRow; i <= currentOutputRow; i++)
                            {
                                outputGrid[i].CumulativeLateFeePastDue = (outputGrid[i].CumulativeLateFeePastDue < outputGrid[actionFeeRow].LateFeePastDue ? 0 : outputGrid[i].CumulativeLateFeePastDue - outputGrid[actionFeeRow].LateFeePastDue);
                                outputGrid[i].CumulativeTotalPastDue = (outputGrid[i].CumulativeTotalPastDue < outputGrid[actionFeeRow].LateFeePastDue ? 0 : outputGrid[i].CumulativeTotalPastDue - outputGrid[actionFeeRow].LateFeePastDue);
                            }

                            outputGrid[actionFeeRow].LateFeePastDue = 0;
                            outputGrid[actionFeeRow].TotalPastDue = 0;
                        }
                        else
                        {
                            outputGrid[currentOutputRow].LateFeePaid += paymentAmount;
                            outputGrid[actionFeeRow].LateFeePastDue -= paymentAmount;
                            outputGrid[actionFeeRow].TotalPastDue -= paymentAmount;

                            //This loop recalculate the cumulative past due amounts of NSF and late fee after processing the row.
                            for (int i = actionFeeRow; i <= currentOutputRow; i++)
                            {
                                outputGrid[i].CumulativeLateFeePastDue = (outputGrid[i].CumulativeLateFeePastDue < paymentAmount ? 0 : outputGrid[i].CumulativeLateFeePastDue - paymentAmount);
                                outputGrid[i].CumulativeTotalPastDue = (outputGrid[i].CumulativeTotalPastDue < paymentAmount ? 0 : outputGrid[i].CumulativeTotalPastDue - paymentAmount);
                            }

                            paymentAmount = 0;
                        }

                        paymentAmount = Math.Round(paymentAmount, 2, MidpointRounding.AwayFromZero);
                        //Determine the oldest index which has NSF fee past due in output grid.
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocateRemainingFundsToNSFAndLateFee() method. Calling method : OldestOutStandingBucket(outputGrid, currentOutputRow, 0, 'NSF Fee');");
                        actionFeeRow = OldestOutStandingBucket(outputGrid, currentOutputRow, 0, "NSF Fee");
                        //Allocate funds to NSF Fee amount
                        if (outputGrid[actionFeeRow].NSFFeePastDue <= paymentAmount)
                        {
                            outputGrid[currentOutputRow].NSFFeePaid += outputGrid[actionFeeRow].NSFFeePastDue;
                            paymentAmount -= outputGrid[actionFeeRow].NSFFeePastDue;

                            //This loop recalculate the cumulative past due amounts of NSF and late fee after processing the row.
                            for (int i = actionFeeRow; i <= currentOutputRow; i++)
                            {
                                outputGrid[i].CumulativeNSFFeePastDue = (outputGrid[i].CumulativeNSFFeePastDue < outputGrid[actionFeeRow].NSFFeePastDue ? 0 : outputGrid[i].CumulativeNSFFeePastDue - outputGrid[actionFeeRow].NSFFeePastDue);
                                outputGrid[i].CumulativeTotalPastDue = (outputGrid[i].CumulativeTotalPastDue < outputGrid[actionFeeRow].NSFFeePastDue ? 0 : outputGrid[i].CumulativeTotalPastDue - outputGrid[actionFeeRow].NSFFeePastDue);
                            }

                            outputGrid[actionFeeRow].NSFFeePastDue = 0;
                            outputGrid[actionFeeRow].TotalPastDue = 0;
                        }
                        else
                        {
                            outputGrid[currentOutputRow].NSFFeePaid += paymentAmount;
                            outputGrid[actionFeeRow].NSFFeePastDue -= paymentAmount;
                            outputGrid[actionFeeRow].TotalPastDue -= paymentAmount;

                            //This loop recalculate the cumulative past due amounts of NSF and late fee after processing the row.
                            for (int i = actionFeeRow; i <= currentOutputRow; i++)
                            {
                                outputGrid[i].CumulativeNSFFeePastDue = (outputGrid[i].CumulativeNSFFeePastDue < paymentAmount ? 0 : outputGrid[i].CumulativeNSFFeePastDue - paymentAmount);
                                outputGrid[i].CumulativeTotalPastDue = (outputGrid[i].CumulativeTotalPastDue < paymentAmount ? 0 : outputGrid[i].CumulativeTotalPastDue - paymentAmount);
                            }

                            paymentAmount = 0;
                        }
                        paymentAmount = Math.Round(paymentAmount, 2, MidpointRounding.AwayFromZero);
                    }
                    previousOutStandingBucketNumber = actionFeeRow;
                }
                sbTracing.AppendLine("Exist:From class ActualPaymentAllocation and method name AllocateRemainingFundsToNSFAndLateFee(List<OutputGrid> outputGrid, LoanDetails " +
                    "scheduleInput, int currentRow, int currentOutputRow, bool isAdditionalPaymentAllocation, ref double paymentAmount)");
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }

        /// <summary>
        /// This function allocates the remaining fund to the service fee and principal amount as per their priority order, and the values of remainingPrincipal and 
        /// principalServiceFee will be calculated to recalculate the output grid.
        /// </summary>
        /// <param name="outputGrid"></param>
        /// <param name="scheduleInput"></param>
        /// <param name="currentInputGridRow"></param>
        /// <param name="currentOutputRow"></param>
        /// <param name="paymentAmount"></param>
        /// <param name="remainingPrincipal"></param>
        /// <param name="principalServiceFee"></param>
        private static void AllocateRemainingFunds(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int currentInputGridRow, int currentOutputRow, double paymentAmount, ref double remainingPrincipal, ref double principalServiceFee)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name AllocateRemainingFunds(List<OutputGrid> outputGrid, LoanDetails scheduleInput, " +
                    "int currentInputGridRow, int currentOutputRow, double paymentAmount, ref double remainingPrincipal, ref double principalServiceFee).");
                sbTracing.AppendLine("Parameter values are : currentInputGridRow = " + currentInputGridRow + ", currentOutputRow = " + currentOutputRow + ", paymentAmount = " + paymentAmount +
                    ", remainingPrincipal = " + remainingPrincipal + ", principalServiceFee = " + principalServiceFee);

                //Checks whether the priority of service fee is less than principal priority.
                if (scheduleInput.InputRecords[currentInputGridRow].ServiceFeePriority < scheduleInput.InputRecords[currentInputGridRow].PrincipalPriority)
                {
                    //Allocate funds to Service Fee amount
                    if (principalServiceFee <= paymentAmount)
                    {
                        outputGrid[currentOutputRow].ServiceFeePaid += principalServiceFee;
                        paymentAmount -= principalServiceFee;
                        principalServiceFee = 0;
                    }
                    else
                    {
                        outputGrid[currentOutputRow].ServiceFeePaid += paymentAmount;
                        principalServiceFee = principalServiceFee - paymentAmount;
                        paymentAmount = 0;
                    }

                    //Allocate funds to principal amount
                    if (remainingPrincipal <= paymentAmount)
                    {
                        outputGrid[currentOutputRow].PrincipalPaid += remainingPrincipal;
                        paymentAmount -= remainingPrincipal;
                        remainingPrincipal = 0;
                    }
                    else
                    {
                        outputGrid[currentOutputRow].PrincipalPaid += paymentAmount;
                        remainingPrincipal = remainingPrincipal - paymentAmount;
                        paymentAmount = 0;
                    }
                }
                else
                {
                    //Allocate funds to principal amount
                    if (remainingPrincipal <= paymentAmount)
                    {
                        outputGrid[currentOutputRow].PrincipalPaid += remainingPrincipal;
                        paymentAmount -= remainingPrincipal;
                        remainingPrincipal = 0;
                    }
                    else
                    {
                        outputGrid[currentOutputRow].PrincipalPaid += paymentAmount;
                        remainingPrincipal = remainingPrincipal - paymentAmount;
                        paymentAmount = 0;
                    }

                    //Allocate funds to Service Fee amount
                    if (principalServiceFee <= paymentAmount)
                    {
                        outputGrid[currentOutputRow].ServiceFeePaid += principalServiceFee;
                        paymentAmount -= principalServiceFee;
                        principalServiceFee = 0;
                    }
                    else
                    {
                        outputGrid[currentOutputRow].ServiceFeePaid += paymentAmount;
                        principalServiceFee = principalServiceFee - paymentAmount;
                        paymentAmount = 0;
                    }
                }
                principalServiceFee = Math.Round(principalServiceFee, 2, MidpointRounding.AwayFromZero);
                remainingPrincipal = Math.Round(remainingPrincipal, 2, MidpointRounding.AwayFromZero);

                sbTracing.AppendLine("Exist:From class ActualPaymentAllocation and method name AllocateRemainingFunds(List<OutputGrid> outputGrid, LoanDetails scheduleInput, " +
                        "int currentInputGridRow, int currentOutputRow, double paymentAmount, ref double remainingPrincipal, ref double principalServiceFee)");
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }

        #endregion

        #region For Skipped Payment

        /// <summary>
        /// This function skips the scheduled payment, and put all the paid amount to 0, as nothing is paid. Also the interest and service fee interest calculated will
        /// be carry forward to next schedule.
        /// </summary>
        /// <param name="outputGrid"></param>
        /// <param name="scheduleInput"></param>
        /// <param name="currentOutputGridRow"></param>
        /// <param name="defaultTotalAmountToPay"></param>
        /// <param name="defaultTotalServiceFeePayable"></param>
        /// <param name="originationFee"></param>
        /// <param name="sameDayFee"></param>
        private static void SkippedPayment(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int currentOutputGridRow, double defaultTotalAmountToPay
                    , double defaultTotalServiceFeePayable, double originationFee, double sameDayFee)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name SkippedPayment(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int currentOutputGridRow, " +
                    "double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double sameDayFee).");
                sbTracing.AppendLine("Parameter values are : currentOutputGridRow = " + currentOutputGridRow + ", defaultTotalAmountToPay = " + defaultTotalAmountToPay +
                        ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", originationFee = " + originationFee + ", sameDayFee = " + sameDayFee);

                //Set all paid amount to 0 in current row.
                outputGrid[currentOutputGridRow].PrincipalPaid = 0;
                outputGrid[currentOutputGridRow].InterestPaid = 0;
                outputGrid[currentOutputGridRow].ServiceFeePaid = 0;
                outputGrid[currentOutputGridRow].ServiceFeeInterestPaid = 0;
                outputGrid[currentOutputGridRow].OriginationFeePaid = 0;
                outputGrid[currentOutputGridRow].MaintenanceFeePaid = 0;
                outputGrid[currentOutputGridRow].ManagementFeePaid = 0;
                outputGrid[currentOutputGridRow].SameDayFeePaid = 0;
                outputGrid[currentOutputGridRow].NSFFeePaid = 0;
                outputGrid[currentOutputGridRow].LateFeePaid = 0;
                outputGrid[currentOutputGridRow].TotalPaid = 0;

                //Set all cumulative paid amount to 0 in current row.
                outputGrid[currentOutputGridRow].CumulativePrincipal = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativePrincipal;
                outputGrid[currentOutputGridRow].CumulativeInterest = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeInterest;
                outputGrid[currentOutputGridRow].CumulativePayment = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativePayment;
                outputGrid[currentOutputGridRow].CumulativeServiceFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeServiceFee;
                outputGrid[currentOutputGridRow].CumulativeServiceFeeInterest = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeServiceFeeInterest;
                outputGrid[currentOutputGridRow].CumulativeServiceFeeTotal = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeServiceFeeTotal;
                outputGrid[currentOutputGridRow].CumulativeOriginationFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeOriginationFee;
                outputGrid[currentOutputGridRow].CumulativeMaintenanceFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeMaintenanceFee;
                outputGrid[currentOutputGridRow].CumulativeManagementFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeManagementFee;
                outputGrid[currentOutputGridRow].CumulativeSameDayFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeSameDayFee;
                outputGrid[currentOutputGridRow].CumulativeNSFFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeNSFFee;
                outputGrid[currentOutputGridRow].CumulativeLateFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeLateFee;
                outputGrid[currentOutputGridRow].CumulativeTotalFees = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeTotalFees;

                //Set all past due balances to 0, in the current row.
                outputGrid[currentOutputGridRow].PrincipalPastDue = 0;
                outputGrid[currentOutputGridRow].InterestPastDue = 0;
                outputGrid[currentOutputGridRow].ServiceFeePastDue = 0;
                outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue = 0;
                outputGrid[currentOutputGridRow].OriginationFeePastDue = 0;
                outputGrid[currentOutputGridRow].MaintenanceFeePastDue = 0;
                outputGrid[currentOutputGridRow].ManagementFeePastDue = 0;
                outputGrid[currentOutputGridRow].SameDayFeePastDue = 0;
                outputGrid[currentOutputGridRow].NSFFeePastDue = 0;
                outputGrid[currentOutputGridRow].LateFeePastDue = 0;
                outputGrid[currentOutputGridRow].TotalPastDue = 0;

                //Set all Cumulative past due balances to 0, in the current row.
                outputGrid[currentOutputGridRow].CumulativePrincipalPastDue = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativePrincipalPastDue;
                outputGrid[currentOutputGridRow].CumulativeInterestPastDue = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeInterestPastDue;
                outputGrid[currentOutputGridRow].CumulativeServiceFeePastDue = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeServiceFeePastDue;
                outputGrid[currentOutputGridRow].CumulativeServiceFeeInterestPastDue = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeServiceFeeInterestPastDue;
                outputGrid[currentOutputGridRow].CumulativeOriginationFeePastDue = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeOriginationFeePastDue;
                outputGrid[currentOutputGridRow].CumulativeMaintenanceFeePastDue = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeMaintenanceFeePastDue;
                outputGrid[currentOutputGridRow].CumulativeManagementFeePastDue = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeManagementFeePastDue;
                outputGrid[currentOutputGridRow].CumulativeSameDayFeePastDue = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeSameDayFeePastDue;
                outputGrid[currentOutputGridRow].CumulativeNSFFeePastDue = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeNSFFeePastDue;
                outputGrid[currentOutputGridRow].CumulativeLateFeePastDue = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeLateFeePastDue;
                outputGrid[currentOutputGridRow].CumulativeTotalPastDue = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeTotalPastDue;
                outputGrid[currentOutputGridRow].BucketStatus = "";

                //Set the cumulative NSF and Late fee past due amount columns of next row.
                if (outputGrid.Count >= currentOutputGridRow + 1)
                {
                    outputGrid[currentOutputGridRow + 1].CumulativeNSFFeePastDue = outputGrid[currentOutputGridRow].CumulativeNSFFeePastDue;
                    outputGrid[currentOutputGridRow + 1].CumulativeLateFeePastDue = outputGrid[currentOutputGridRow].CumulativeLateFeePastDue;
                    outputGrid[currentOutputGridRow + 1].CumulativeTotalPastDue = outputGrid[currentOutputGridRow].CumulativeTotalPastDue;
                }

                //Recalculates the output grid values for the remaining scheduled dates.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and SkippedPayment() method. Calling method : ReCalculateOutputGridValues(outputGrid[currentOutputGridRow].BeginningPrincipal, " +
                    "outputGrid[currentOutputGridRow].BeginningServiceFee, outputGrid, scheduleInput, currentOutputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);");
                ReCalculateOutputGridValues(outputGrid[currentOutputGridRow].BeginningPrincipal, outputGrid[currentOutputGridRow].BeginningServiceFee, outputGrid,
                                                scheduleInput, currentOutputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);

                sbTracing.AppendLine("Exist:From class ActualPaymentAllocation and method name SkippedPayment(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int currentOutputGridRow, " +
                    "double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double sameDayFee)");
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }

        #endregion

        #region For Additional Payment

        /// <summary>
        /// This function adds the NSF fee and Late fee event of additional payment to the output grid.
        /// </summary>
        /// <param name="outputGrid"></param>
        /// <param name="scheduleInputs"></param>
        /// <param name="additionalPaymentRow"></param>
        /// <param name="outputGridRow"></param>
        /// <param name="defaultTotalAmountToPay"></param>
        /// <param name="defaultTotalServiceFeePayable"></param>
        /// <param name="originationFee"></param>
        /// <param name="sameDayFee"></param>
        private static void AddNSFAndLateFee(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, int outputGridRow,
                                                double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double sameDayFee)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name AddNSFAndLateFee(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, " +
                    "int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double sameDayFee).");
                sbTracing.AppendLine("Parameter values are : additionalPaymentRow = " + additionalPaymentRow + ", outputGridRow = " + outputGridRow + ", defaultTotalAmountToPay = " + defaultTotalAmountToPay +
                    ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", originationFee = " + originationFee + ", sameDayFee = " + sameDayFee);

                //It determines the total interest carry over to be paid.
                double interestCarryOver = outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].InterestCarryOver;
                double serviceFeeInterestCarryOver = outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].AccruedServiceFeeInterestCarryOver;

                //Checks whether the previous row was the skipped payment event.
                if (outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].Flags == 4)
                {
                    //Add the previous interest payable amount to carry over value, as previously due to skip payment it was not paid.
                    interestCarryOver += outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].InterestPayment;
                    //Add the previous service fee interest payable amount to carry over value, as previously due to skip payment it was not paid.
                    serviceFeeInterestCarryOver += outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].ServiceFeeInterest;
                }

                //Insert a new row in the output grid on index "outputGridRow".
                outputGrid.Insert(outputGridRow,
                        new OutputGrid
                        {
                            PaymentDate = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].DateIn,
                            BeginningPrincipal = outputGrid[outputGridRow].BeginningPrincipal,
                            BeginningServiceFee = outputGrid[outputGridRow].BeginningServiceFee,
                            PeriodicInterestRate = 0,
                            DailyInterestRate = 0,
                            DailyInterestAmount = 0,
                            PaymentID = Convert.ToInt32(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].PaymentID.ToString()),
                            Flags = Convert.ToInt32(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].Flags.ToString()),
                            DueDate = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].DateIn,
                            InterestAccrued = 0,
                            InterestCarryOver = interestCarryOver,
                            InterestPayment = 0,
                            PrincipalPayment = 0,
                            TotalPayment = 0,
                            AccruedServiceFeeInterest = 0,
                            AccruedServiceFeeInterestCarryOver = serviceFeeInterestCarryOver,
                            ServiceFee = 0,
                            ServiceFeeInterest = 0,
                            ServiceFeeTotal = 0,
                            OriginationFee = 0,
                            MaintenanceFee = 0,
                            ManagementFee = 0,
                            SameDayFee = 0,
                            NSFFee = 0,
                            LateFee = 0,
                            //Paid amount columns
                            PrincipalPaid = 0,
                            InterestPaid = 0,
                            ServiceFeePaid = 0,
                            ServiceFeeInterestPaid = 0,
                            OriginationFeePaid = 0,
                            MaintenanceFeePaid = 0,
                            ManagementFeePaid = 0,
                            SameDayFeePaid = 0,
                            NSFFeePaid = 0,
                            LateFeePaid = 0,
                            TotalPaid = 0,
                            //Cumulative Amount paid column
                            CumulativeInterest = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeInterest,
                            CumulativePrincipal = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePrincipal,
                            CumulativePayment = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePayment,
                            CumulativeServiceFee = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFee,
                            CumulativeServiceFeeInterest = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeInterest,
                            CumulativeServiceFeeTotal = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeTotal,
                            CumulativeOriginationFee = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeOriginationFee,
                            CumulativeMaintenanceFee = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeMaintenanceFee,
                            CumulativeManagementFee = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeManagementFee,
                            CumulativeSameDayFee = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeSameDayFee,
                            CumulativeNSFFee = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeNSFFee,
                            CumulativeLateFee = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeLateFee,
                            CumulativeTotalFees = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeTotalFees,
                            //Past due amount columns
                            PrincipalPastDue = 0,
                            InterestPastDue = 0,
                            ServiceFeePastDue = 0,
                            ServiceFeeInterestPastDue = 0,
                            OriginationFeePastDue = 0,
                            MaintenanceFeePastDue = 0,
                            ManagementFeePastDue = 0,
                            SameDayFeePastDue = 0,
                            NSFFeePastDue = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].Flags.ToString() == "11" ? scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment : 0,
                            LateFeePastDue = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].Flags.ToString() == "12" ? scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment : 0,
                            TotalPastDue = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment,
                            //Cumulative past due amount columns
                            CumulativePrincipalPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePrincipalPastDue,
                            CumulativeInterestPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeInterestPastDue,
                            CumulativeServiceFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeePastDue,
                            CumulativeServiceFeeInterestPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeInterestPastDue,
                            CumulativeOriginationFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeOriginationFeePastDue,
                            CumulativeMaintenanceFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeMaintenanceFeePastDue,
                            CumulativeManagementFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeManagementFeePastDue,
                            CumulativeSameDayFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeSameDayFeePastDue,
                            CumulativeNSFFeePastDue = (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].Flags.ToString() == "11" ? scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment : 0) + outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeNSFFeePastDue,
                            CumulativeLateFeePastDue = (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].Flags.ToString() == "12" ? scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment : 0) + outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeLateFeePastDue,
                            CumulativeTotalPastDue = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment + outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeTotalPastDue,
                            BucketStatus = ""
                        });

                //Recalculate the output grid for the remaining schedule dates in the input grid.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AddNSFAndLateFee() method. Calling method : ReCalculateOutputGridValues(outputGrid[outputGridRow].BeginningPrincipal, outputGrid[outputGridRow].BeginningServiceFee, " +
                        "outputGrid, scheduleInputs, outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);");
                ReCalculateOutputGridValues(outputGrid[outputGridRow].BeginningPrincipal, outputGrid[outputGridRow].BeginningServiceFee, outputGrid, scheduleInputs,
                                                                outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);

                //Set the cumulative NSF and Late fee past due amount columns of next row.
                if (outputGrid.Count >= outputGridRow + 1)
                {
                    outputGrid[outputGridRow + 1].CumulativeNSFFeePastDue = outputGrid[outputGridRow].CumulativeNSFFeePastDue;
                    outputGrid[outputGridRow + 1].CumulativeLateFeePastDue = outputGrid[outputGridRow].CumulativeLateFeePastDue;
                    outputGrid[outputGridRow + 1].CumulativeTotalPastDue = outputGrid[outputGridRow].CumulativeTotalPastDue;
                }
                sbTracing.AppendLine("Exist:From class ActualPaymentAllocation and method name AddNSFAndLateFee(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, " +
                    "int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double sameDayFee)");
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }

        /// <summary>
        /// This function adds an principal only additional payment event to the output grid, that will pay only the principal amount.
        /// </summary>
        /// <param name="outputGrid"></param>
        /// <param name="scheduleInputs"></param>
        /// <param name="additionalPaymentRow"></param>
        /// <param name="scheduleInputGridRow"></param>
        /// <param name="outputGridRow"></param>
        /// <param name="defaultTotalAmountToPay"></param>
        /// <param name="defaultTotalServiceFeePayable"></param>
        /// <param name="isExtendingSchedule"></param>
        /// <param name="originationFee"></param>
        /// <param name="sameDayFee"></param>
        private static void PayPrincipalOnlyAmount(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, int scheduleInputGridRow, int outputGridRow,
                double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name PayPrincipalOnlyAmount(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, " +
                    "int scheduleInputGridRow, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee).");
                sbTracing.AppendLine("parameter values are : additionalPaymentRow = " + additionalPaymentRow + ", scheduleInputGridRow = " + scheduleInputGridRow + ", outputGridRow = " + outputGridRow + ", defaultTotalAmountToPay = " +
                    defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", isExtendingSchedule = " + isExtendingSchedule + ", originationFee = " + originationFee + ", sameDayFee = " + sameDayFee);

                double remainingPrincipal = isExtendingSchedule ? (outputGrid[outputGridRow - 1].BeginningPrincipal - outputGrid[outputGridRow - 1].PrincipalPaid) :
                                                                    outputGrid[outputGridRow].BeginningPrincipal;

                double remainingPrincipalServiceFee = isExtendingSchedule ? (outputGrid[outputGridRow - 1].BeginningServiceFee - outputGrid[outputGridRow - 1].ServiceFeePaid) :
                                                                    outputGrid[outputGridRow].BeginningServiceFee;
                double periodicInterestRate = 0;
                DateTime startDate = DateTime.MinValue;

                int row = outputGridRow - 1;
                //This loop determines the row which is neither the NSF fee event nor Late fee event.
                while (row >= 0)
                {
                    if (outputGrid[row].Flags != 11 && outputGrid[row].Flags != 12)
                    {
                        break;
                    }
                    row--;
                }

                //Checks if the row which is neither NSF fee nor late fee event, doesn't exists.
                if (row == -1)
                {
                    startDate = scheduleInputs.InputRecords[scheduleInputGridRow - 1].EffectiveDate == DateTime.MinValue ?
                                        scheduleInputs.InputRecords[scheduleInputGridRow - 1].DateIn : scheduleInputs.InputRecords[scheduleInputGridRow - 1].EffectiveDate;
                }
                else
                {
                    startDate = outputGrid[row].PaymentDate;
                }
                DateTime endDate = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].DateIn;

                //This condition determines whether there is an interest rate in additional payment grid, for that row. If it is provided, that value will be used, otherwise
                //default value will be used for periodic interest calculations.
                if (string.IsNullOrEmpty(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate))
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayPrincipalOnlyAmount() method. Calling method : PeriodicInterestRate.GetPeriodicInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, Convert.ToDouble(scheduleInputs.InterestRate));");
                    periodicInterestRate = PeriodicInterestRate.GetPeriodicInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, Convert.ToDouble(scheduleInputs.InterestRate));
                }
                else
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayPrincipalOnlyAmount() method. Calling method : PeriodicInterestRate.GetPeriodicInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate));");
                    periodicInterestRate = PeriodicInterestRate.GetPeriodicInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate));
                }

                //Calculates the interest accrued dfor the added schedule in the output grid.
                double interestAccrued = periodicInterestRate * remainingPrincipal;
                interestAccrued = Math.Round(interestAccrued, 2, MidpointRounding.AwayFromZero);

                //It determines the total interest carry over to be paid.
                double interestCarryOver = interestAccrued + outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].InterestCarryOver;

                double serviceFeeInterestAccrued = 0, serviceFeeInterestCarryOver = 0;
                //Checks whether service fee interest is to be taken or not.
                if (scheduleInputs.ApplyServiceFeeInterest == 1 || scheduleInputs.ApplyServiceFeeInterest == 3)
                {
                    //Calculate the accrued service fee interest value.
                    serviceFeeInterestAccrued = periodicInterestRate * remainingPrincipalServiceFee;
                    serviceFeeInterestAccrued = Math.Round(serviceFeeInterestAccrued, 2, MidpointRounding.AwayFromZero);

                    //It determines the total interest carry over to be paid.
                    serviceFeeInterestCarryOver = serviceFeeInterestAccrued + outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].AccruedServiceFeeInterestCarryOver;
                }

                //Checks whether the pervious event was a skipped payment and is not the first row of output grid.
                if (outputGridRow != 0 && outputGrid[outputGridRow - 1].Flags == 4)
                {
                    //Add the interest payment amount of previous schedule to the calculated interest payment of current row, as previous interest amount was not paid due to skipped payment.
                    interestCarryOver += outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].InterestPayment;

                    //Add the service fee interest payment amount of previous schedule to the calculated service fee interest payment of current row, as previous service fee interest amount was not paid due to skipped payment.
                    serviceFeeInterestCarryOver += outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].ServiceFeeInterest;
                }

                //It calculates the remaining principal amount that will be paid after deducting the additional payment amount.
                double principalAmountToPay = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment > remainingPrincipal ? remainingPrincipal :
                                                            scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment;

                //This variable calculates the daily interest rate for the period.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayPrincipalOnlyAmount() method. Calling method : DailyInterestRate.CalculateDailyInterestRate(scheduleInputs.DaysInYearBankMethod, " +
                    "scheduleInputs.DaysInMonth, startDate, endDate, periodicInterestRate);");
                double dailyInterestRate = DailyInterestRate.CalculateDailyInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate,
                                                            periodicInterestRate);

                //This variable calculates the daily interest amount for the period.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayPrincipalOnlyAmount() method. Calling method : DailyInterestAmount.CalculateDailyInterestAmount(scheduleInputs.DaysInYearBankMethod, " +
                    "scheduleInputs.DaysInMonth, startDate, endDate, periodicInterestRate * remainingPrincipal);");
                double dailyInterestAmount = DailyInterestAmount.CalculateDailyInterestAmount(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate,
                                                            endDate, periodicInterestRate * remainingPrincipal);
                dailyInterestAmount = Math.Round(dailyInterestAmount, 2, MidpointRounding.AwayFromZero);

                //It adds a new row on the selected indes i.e. "outputGridRow" in the output grid.
                outputGrid.Insert(outputGridRow,
                        new OutputGrid
                        {
                            PaymentDate = endDate,
                            BeginningPrincipal = remainingPrincipal,
                            BeginningServiceFee = remainingPrincipalServiceFee,
                            PeriodicInterestRate = periodicInterestRate,
                            DailyInterestRate = dailyInterestRate,
                            DailyInterestAmount = dailyInterestAmount,
                            PaymentID = Convert.ToInt32(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].PaymentID.ToString()),
                            Flags = Convert.ToInt32(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].Flags.ToString()),
                            DueDate = endDate,
                            InterestAccrued = interestAccrued,
                            InterestCarryOver = interestCarryOver,
                            InterestPayment = 0,
                            PrincipalPayment = principalAmountToPay,
                            TotalPayment = principalAmountToPay,
                            AccruedServiceFeeInterest = serviceFeeInterestAccrued,
                            AccruedServiceFeeInterestCarryOver = serviceFeeInterestCarryOver,
                            ServiceFee = 0,
                            ServiceFeeInterest = 0,
                            ServiceFeeTotal = 0,
                            OriginationFee = 0,
                            MaintenanceFee = (isExtendingSchedule && scheduleInputGridRow > 0) ? scheduleInputs.MaintenanceFee : 0,
                            ManagementFee = isExtendingSchedule ? scheduleInputs.ManagementFee : 0,
                            SameDayFee = 0,
                            NSFFee = 0,
                            LateFee = 0,
                            //Paid amount columns
                            PrincipalPaid = principalAmountToPay,
                            InterestPaid = 0,
                            ServiceFeePaid = 0,
                            ServiceFeeInterestPaid = 0,
                            OriginationFeePaid = 0,
                            MaintenanceFeePaid = 0,
                            ManagementFeePaid = 0,
                            SameDayFeePaid = 0,
                            NSFFeePaid = 0,
                            LateFeePaid = 0,
                            TotalPaid = principalAmountToPay,
                            //Cumulative Amount paid column
                            CumulativeInterest = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeInterest,
                            CumulativePrincipal = outputGridRow == 0 ? principalAmountToPay : outputGrid[outputGridRow - 1].CumulativePrincipal + principalAmountToPay,
                            CumulativePayment = outputGridRow == 0 ? principalAmountToPay : outputGrid[outputGridRow - 1].CumulativePayment + principalAmountToPay,
                            CumulativeServiceFee = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFee,
                            CumulativeServiceFeeInterest = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeInterest,
                            CumulativeServiceFeeTotal = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeTotal,
                            CumulativeOriginationFee = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeOriginationFee,
                            CumulativeMaintenanceFee = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeMaintenanceFee,
                            CumulativeManagementFee = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeManagementFee,
                            CumulativeSameDayFee = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeSameDayFee,
                            CumulativeNSFFee = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeNSFFee,
                            CumulativeLateFee = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeLateFee,
                            CumulativeTotalFees = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeTotalFees,
                            //Past due amount columns
                            PrincipalPastDue = 0,
                            InterestPastDue = 0,
                            ServiceFeePastDue = 0,
                            ServiceFeeInterestPastDue = 0,
                            OriginationFeePastDue = 0,
                            MaintenanceFeePastDue = (isExtendingSchedule && scheduleInputGridRow > 0) ? scheduleInputs.MaintenanceFee : 0,
                            ManagementFeePastDue = isExtendingSchedule ? scheduleInputs.ManagementFee : 0,
                            SameDayFeePastDue = 0,
                            NSFFeePastDue = 0,
                            LateFeePastDue = 0,
                            TotalPastDue = ((isExtendingSchedule && scheduleInputGridRow > 0) ? scheduleInputs.MaintenanceFee : 0) +
                                                        (isExtendingSchedule ? scheduleInputs.ManagementFee : 0),
                            //Cumulative past due amount columns
                            CumulativePrincipalPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePrincipalPastDue,
                            CumulativeInterestPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeInterestPastDue,
                            CumulativeServiceFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeePastDue,
                            CumulativeServiceFeeInterestPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeInterestPastDue,
                            CumulativeOriginationFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeOriginationFeePastDue,
                            CumulativeMaintenanceFeePastDue = ((isExtendingSchedule && scheduleInputGridRow > 0) ? scheduleInputs.MaintenanceFee : 0) +
                                                                    outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeMaintenanceFeePastDue,
                            CumulativeManagementFeePastDue = (isExtendingSchedule ? scheduleInputs.ManagementFee : 0) + outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeManagementFeePastDue,
                            CumulativeSameDayFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeSameDayFeePastDue,
                            CumulativeNSFFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeNSFFeePastDue,
                            CumulativeLateFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeLateFeePastDue,
                            CumulativeTotalPastDue = ((isExtendingSchedule && scheduleInputGridRow > 0) ? scheduleInputs.MaintenanceFee : 0) +
                                                        (isExtendingSchedule ? scheduleInputs.ManagementFee : 0) +
                                                        outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeTotalPastDue,
                            BucketStatus = isExtendingSchedule ? "OutStanding" : ""
                        });

                //Determine the additional payment value that is being paid in the principal only payment event.
                double additionalPayment = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment;
                int previousOutStandingBucketNumber = 0;

                //This loop pay al the past due principal amount.
                while (previousOutStandingBucketNumber < outputGridRow && additionalPayment > 0)
                {
                    //Determine the oldestoutstanding bucket which has the principal past due amount greater than 0.
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayPrincipalOnlyAmount() method. Calling method : OldestOutStandingBucket(outputGrid, outputGridRow, previousOutStandingBucketNumber, 'Principal Only;');");
                    int bucketNumber = OldestOutStandingBucket(outputGrid, outputGridRow, previousOutStandingBucketNumber, "Principal Only");
                    if (bucketNumber == outputGridRow)
                    {
                        break;
                    }

                    //Determie the amount that is being deducted from the principal past due amount of selected bucket.
                    double deductedPrincipal = (outputGrid[bucketNumber].PrincipalPastDue - additionalPayment) <= 0 ? outputGrid[bucketNumber].PrincipalPastDue : additionalPayment;
                    additionalPayment -= deductedPrincipal;

                    outputGrid[bucketNumber].PrincipalPastDue -= deductedPrincipal;
                    outputGrid[bucketNumber].TotalPastDue -= deductedPrincipal;

                    if (isExtendingSchedule)
                    {
                        int lastRowIndex = outputGrid.Count - 1;
                        for (int i = 0; i < outputGrid.Count; i++)
                        {
                            if (scheduleInputs.InputRecords[scheduleInputs.InputRecords.Count - 1].DateIn < outputGrid[i].DueDate)
                            {
                                lastRowIndex = i - 1;
                                break;
                            }
                        }
                        if (lastRowIndex != bucketNumber)
                        {
                            outputGrid[lastRowIndex].PrincipalPastDue -= deductedPrincipal;
                            outputGrid[lastRowIndex].TotalPastDue -= deductedPrincipal;
                        }
                    }

                    //Recalculate the cumulative principal and total past due amount columns after processing the principal only amount, till current bucket.
                    for (int i = bucketNumber; i <= outputGridRow; i++)
                    {
                        outputGrid[i].CumulativePrincipalPastDue -= deductedPrincipal;
                        outputGrid[i].CumulativeTotalPastDue -= deductedPrincipal;
                    }

                    //Checks whether the cumulative total past due amount is 0. If it is, set the bucket status to "Satisfied".
                    if (outputGrid[bucketNumber].CumulativeTotalPastDue == 0)
                    {
                        outputGrid[bucketNumber].BucketStatus = "Satisfied";
                    }
                    previousOutStandingBucketNumber = bucketNumber;
                }

                remainingPrincipal -= principalAmountToPay;

                //Recalculates the output grid values for the remaining scheduled dates if the aditional payment is not after last scheduled payment date.
                if (!isExtendingSchedule)
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayPrincipalOnlyAmount() method. Calling method : ReCalculateOutputGridValues(remainingPrincipal, remainingprincipalServiceFee, outputGrid, scheduleInputs, " +
                        "outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);");
                    ReCalculateOutputGridValues(remainingPrincipal, remainingPrincipalServiceFee, outputGrid, scheduleInputs, outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);
                }

                sbTracing.AppendLine("Exist:From class ActualPaymentAllocation and method name PayPrincipalOnlyAmount(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, " +
                    "int scheduleInputGridRow, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee)");
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }

        /// <summary>
        /// This function adds an additional payment which will be distributed among all the components as per their priorities.
        /// </summary>
        /// <param name="outputGrid"></param>
        /// <param name="scheduleInputs"></param>
        /// <param name="additionalPaymentRow"></param>
        /// <param name="scheduleInputGridRow"></param>
        /// <param name="currentOutputGridRow"></param>
        /// <param name="defaultTotalAmountToPay"></param>
        /// <param name="defaultTotalServiceFeePayable"></param>
        /// <param name="isExtendingSchedule"></param>
        /// <param name="originationFee"></param>
        /// <param name="sameDayFee"></param>
        private static void PayAdditionalPayment(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, int scheduleInputGridRow, int currentOutputGridRow,
                double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name PayAdditionalPayment(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, int scheduleInputGridRow, " +
                    "int currentOutputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee).");
                sbTracing.AppendLine("parameter values are : additionalPaymentRow = " + additionalPaymentRow + ", scheduleInputGridRow = " + scheduleInputGridRow + ", currentOutputGridRow = " + currentOutputGridRow +
                    ", defaultTotalAmountToPay = " + defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", isExtendingSchedule = " + isExtendingSchedule + ", originationFee = " + originationFee +
                    ", sameDayFee = " + sameDayFee);

                double remainingPrincipal = isExtendingSchedule ? (outputGrid[currentOutputGridRow - 1].BeginningPrincipal - outputGrid[currentOutputGridRow - 1].PrincipalPaid) :
                                                                            outputGrid[currentOutputGridRow].BeginningPrincipal;
                double remainingPrincipalServiceFee = isExtendingSchedule ? (outputGrid[currentOutputGridRow - 1].BeginningServiceFee - outputGrid[currentOutputGridRow - 1].ServiceFeePaid) :
                                                                outputGrid[currentOutputGridRow].BeginningServiceFee;

                double periodicInterestRate = 0;
                DateTime startDate = DateTime.MinValue;

                int row = currentOutputGridRow - 1;
                //This loop determines the row which is neither the NSF fee event nor Late fee event.
                while (row >= 0)
                {
                    if (outputGrid[row].Flags != 11 && outputGrid[row].Flags != 12)
                    {
                        break;
                    }
                    row--;
                }

                //Checks if the row which is neither NSF fee nor late fee event, doesn't exists.
                if (row == -1)
                {
                    startDate = scheduleInputs.InputRecords[scheduleInputGridRow - 1].EffectiveDate == DateTime.MinValue ?
                                        scheduleInputs.InputRecords[scheduleInputGridRow - 1].DateIn : scheduleInputs.InputRecords[scheduleInputGridRow - 1].EffectiveDate;
                }
                else
                {
                    startDate = outputGrid[row].PaymentDate;
                }
                DateTime endDate = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].DateIn;

                //This condition determines whether there is an interest rate in additional payment grid, for that row. If it is provided, that value will be used, otherwise
                //default value will be used for periodic interest calculations.
                if (string.IsNullOrEmpty(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate))
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayAdditionalPayment() method. Calling method : PeriodicInterestRate.GetPeriodicInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, Convert.ToDouble(scheduleInputs.InterestRate));");
                    periodicInterestRate = PeriodicInterestRate.GetPeriodicInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, Convert.ToDouble(scheduleInputs.InterestRate));
                }
                else
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayAdditionalPayment() method. Calling method : PeriodicInterestRate.GetPeriodicInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate));");
                    periodicInterestRate = PeriodicInterestRate.GetPeriodicInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate));
                }

                //Calculates the interest accrued for that period.
                double interestAccrued = periodicInterestRate * remainingPrincipal;
                interestAccrued = Math.Round(interestAccrued, 2, MidpointRounding.AwayFromZero);

                //It determines the total interest carry over to be paid.
                double interestCarryOver = interestAccrued + outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].InterestCarryOver;

                double serviceFeeInterestAccrued = 0, serviceFeeInterestCarryOver = 0;
                //Checks whether service fee interest is to be taken or not.
                if (scheduleInputs.ApplyServiceFeeInterest == 1 || scheduleInputs.ApplyServiceFeeInterest == 3)
                {
                    //Calculate the accrued service fee interest value.
                    serviceFeeInterestAccrued = periodicInterestRate * remainingPrincipalServiceFee;
                    serviceFeeInterestAccrued = Math.Round(serviceFeeInterestAccrued, 2, MidpointRounding.AwayFromZero);

                    //It determines the total interest carry over to be paid.
                    serviceFeeInterestCarryOver = serviceFeeInterestAccrued + outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].AccruedServiceFeeInterestCarryOver;
                }

                //Checks whether the pervious event was a skipped payment and is not the first row of output grid.
                if (outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].Flags == 4)
                {
                    interestCarryOver += outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].InterestPayment;
                    serviceFeeInterestCarryOver += outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].ServiceFeeInterest;
                }

                //Determine the origination fee and same day fee to be paid, which was not paid due to skipping the first repayment schedule.
                if (endDate > scheduleInputs.InputRecords[1].DateIn)
                {
                    if (scheduleInputs.InputRecords[1].Flags == 4)
                    {
                        //Calculate remaining origination and same day fee to be paid after deducting the paid amount from original payable amount.
                        for (int i = 0; i < currentOutputGridRow; i++)
                        {
                            originationFee = (originationFee - outputGrid[i].OriginationFeePaid) < 0 ? 0 : (originationFee - outputGrid[i].OriginationFeePaid);
                            sameDayFee = (sameDayFee - outputGrid[i].SameDayFeePaid) < 0 ? 0 : (sameDayFee - outputGrid[i].SameDayFeePaid);
                        }
                    }
                    else
                    {
                        originationFee = 0;
                        sameDayFee = 0;
                    }
                }

                //This variable calculates the daily interest rate for the period.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayAdditionalPayment() method. Calling method : DailyInterestRate.CalculateDailyInterestRate(scheduleInputs.DaysInYearBankMethod, " +
                    "scheduleInputs.DaysInMonth, startDate, endDate, periodicInterestRate);");
                double dailyInterestRate = DailyInterestRate.CalculateDailyInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate,
                                                            periodicInterestRate);

                //This variable calculates the daily interest amount for the period.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayAdditionalPayment() method. Calling method : DailyInterestAmount.CalculateDailyInterestAmount(scheduleInputs.DaysInYearBankMethod, " +
                    "scheduleInputs.DaysInMonth, startDate, endDate, periodicInterestRate * remainingPrincipal);");
                double dailyInterestAmount = DailyInterestAmount.CalculateDailyInterestAmount(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate,
                                                            endDate, periodicInterestRate * remainingPrincipal);
                dailyInterestAmount = Math.Round(dailyInterestAmount, 2, MidpointRounding.AwayFromZero);

                //It adds a new row on the selected indes i.e. "outputGridRow" in the output grid.
                outputGrid.Insert(currentOutputGridRow,
                        new OutputGrid
                        {
                            PaymentDate = endDate,
                            BeginningPrincipal = remainingPrincipal,
                            BeginningServiceFee = remainingPrincipalServiceFee,
                            PeriodicInterestRate = periodicInterestRate,
                            DailyInterestRate = dailyInterestRate,
                            DailyInterestAmount = dailyInterestAmount,
                            PaymentID = Convert.ToInt32(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].PaymentID.ToString()),
                            Flags = Convert.ToInt32(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].Flags.ToString()),
                            DueDate = endDate,
                            InterestAccrued = interestAccrued,
                            InterestCarryOver = interestCarryOver,
                            InterestPayment = 0,
                            PrincipalPayment = 0,
                            TotalPayment = Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment.ToString()),
                            AccruedServiceFeeInterest = serviceFeeInterestAccrued,
                            AccruedServiceFeeInterestCarryOver = serviceFeeInterestCarryOver,
                            ServiceFee = 0,
                            ServiceFeeInterest = 0,
                            ServiceFeeTotal = 0,
                            OriginationFee = 0,
                            MaintenanceFee = (isExtendingSchedule && scheduleInputGridRow > 0) ? scheduleInputs.MaintenanceFee : 0,
                            ManagementFee = isExtendingSchedule ? scheduleInputs.ManagementFee : 0,
                            SameDayFee = 0,
                            NSFFee = 0,
                            LateFee = 0,
                            //Paid amount columns
                            PrincipalPaid = 0,
                            InterestPaid = 0,
                            ServiceFeePaid = 0,
                            ServiceFeeInterestPaid = 0,
                            OriginationFeePaid = 0,
                            MaintenanceFeePaid = 0,
                            ManagementFeePaid = 0,
                            SameDayFeePaid = 0,
                            NSFFeePaid = 0,
                            LateFeePaid = 0,
                            TotalPaid = 0,
                            //Cumulative Amount paid column
                            CumulativeInterest = currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeInterest,
                            CumulativePrincipal = currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativePrincipal,
                            CumulativePayment = currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativePayment,
                            CumulativeServiceFee = currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeServiceFee,
                            CumulativeServiceFeeInterest = currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeServiceFeeInterest,
                            CumulativeServiceFeeTotal = currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeServiceFeeTotal,
                            CumulativeOriginationFee = currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeOriginationFee,
                            CumulativeMaintenanceFee = currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeMaintenanceFee,
                            CumulativeManagementFee = currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeManagementFee,
                            CumulativeSameDayFee = currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeSameDayFee,
                            CumulativeNSFFee = currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeNSFFee,
                            CumulativeLateFee = currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeLateFee,
                            CumulativeTotalFees = currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeTotalFees,
                            //Past due amount columns
                            PrincipalPastDue = 0,
                            InterestPastDue = 0,
                            ServiceFeePastDue = 0,
                            ServiceFeeInterestPastDue = 0,
                            OriginationFeePastDue = 0,
                            MaintenanceFeePastDue = (isExtendingSchedule && scheduleInputGridRow > 0) ? scheduleInputs.MaintenanceFee : 0,
                            ManagementFeePastDue = isExtendingSchedule ? scheduleInputs.ManagementFee : 0,
                            SameDayFeePastDue = 0,
                            NSFFeePastDue = 0,
                            LateFeePastDue = 0,
                            TotalPastDue = ((isExtendingSchedule && scheduleInputGridRow > 0) ? scheduleInputs.MaintenanceFee : 0) +
                                                        (isExtendingSchedule ? scheduleInputs.ManagementFee : 0),
                            //Cumulative past due amount columns
                            CumulativePrincipalPastDue = currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativePrincipalPastDue,
                            CumulativeInterestPastDue = currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeInterestPastDue,
                            CumulativeServiceFeePastDue = currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeServiceFeePastDue,
                            CumulativeServiceFeeInterestPastDue = currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeServiceFeeInterestPastDue,
                            CumulativeOriginationFeePastDue = currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeOriginationFeePastDue,
                            CumulativeMaintenanceFeePastDue = ((isExtendingSchedule && scheduleInputGridRow > 0) ? scheduleInputs.MaintenanceFee : 0) +
                                                                currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeMaintenanceFeePastDue,
                            CumulativeManagementFeePastDue = (isExtendingSchedule ? scheduleInputs.ManagementFee : 0) + currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeManagementFeePastDue,
                            CumulativeSameDayFeePastDue = currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeSameDayFeePastDue,
                            CumulativeNSFFeePastDue = currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeNSFFeePastDue,
                            CumulativeLateFeePastDue = currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeLateFeePastDue,
                            CumulativeTotalPastDue = ((isExtendingSchedule && scheduleInputGridRow > 0) ? scheduleInputs.MaintenanceFee : 0) +
                                                        (isExtendingSchedule ? scheduleInputs.ManagementFee : 0) +
                                                        currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeTotalPastDue,
                            BucketStatus = isExtendingSchedule ? "OutStanding" : ""
                        });

                int previousOutStandingBucketNumber = 0;
                //Determine the payment amount for the current additional payment bucket which will be distributed among all the components.
                double paymentAmount = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment;

                int lastRowIndex = outputGrid.Count - 1;
                if (isExtendingSchedule)
                {
                    for (int i = 0; i < outputGrid.Count; i++)
                    {
                        if (scheduleInputs.InputRecords[scheduleInputs.InputRecords.Count - 1].DateIn < outputGrid[i].DueDate)
                        {
                            lastRowIndex = i - 1;
                            break;
                        }
                    }
                }

                #region Amount allocation in outstanding Buckets before the current bucket

                while (previousOutStandingBucketNumber < currentOutputGridRow && paymentAmount > 0)
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayAdditionalPayment() method. Calling method : OldestOutStandingBucket(outputGrid, currentOutputGridRow, " +
                        "previousOutStandingBucketNumber, 'OutStanding Bucket');");
                    int bucketNumber = OldestOutStandingBucket(outputGrid, currentOutputGridRow, previousOutStandingBucketNumber, "OutStanding Bucket");

                    //If oldest outstanding bucket is cuurent bucket, then break from the loop.
                    if (bucketNumber >= currentOutputGridRow)
                    {
                        break;
                    }

                    //Checks whether total past due amount of oldest outstanding bucket is less than the payment amount.
                    if (outputGrid[bucketNumber].TotalPastDue <= paymentAmount)
                    {
                        //Paid amount is added in current payment row
                        outputGrid[currentOutputGridRow].PrincipalPaid += outputGrid[bucketNumber].PrincipalPastDue;
                        outputGrid[currentOutputGridRow].InterestPaid += outputGrid[bucketNumber].InterestPastDue;
                        outputGrid[currentOutputGridRow].ServiceFeePaid += outputGrid[bucketNumber].ServiceFeePastDue;
                        outputGrid[currentOutputGridRow].ServiceFeeInterestPaid += outputGrid[bucketNumber].ServiceFeeInterestPastDue;
                        outputGrid[currentOutputGridRow].OriginationFeePaid += outputGrid[bucketNumber].OriginationFeePastDue;
                        outputGrid[currentOutputGridRow].MaintenanceFeePaid += outputGrid[bucketNumber].MaintenanceFeePastDue;
                        outputGrid[currentOutputGridRow].ManagementFeePaid += outputGrid[bucketNumber].ManagementFeePastDue;
                        outputGrid[currentOutputGridRow].SameDayFeePaid += outputGrid[bucketNumber].SameDayFeePastDue;
                        outputGrid[currentOutputGridRow].TotalPaid += outputGrid[bucketNumber].TotalPastDue;

                        paymentAmount -= outputGrid[bucketNumber].TotalPastDue;

                        if (isExtendingSchedule && (bucketNumber != lastRowIndex))
                        {
                            //Set all past due balances to 0, in the oldest outstanding row.
                            outputGrid[lastRowIndex].PrincipalPastDue -= outputGrid[bucketNumber].PrincipalPastDue;
                            outputGrid[lastRowIndex].InterestPastDue -= outputGrid[bucketNumber].InterestPastDue;
                            outputGrid[lastRowIndex].ServiceFeePastDue -= outputGrid[bucketNumber].ServiceFeePastDue;
                            outputGrid[lastRowIndex].ServiceFeeInterestPastDue -= outputGrid[bucketNumber].ServiceFeeInterestPastDue;
                            outputGrid[lastRowIndex].TotalPastDue = outputGrid[lastRowIndex].TotalPastDue - (outputGrid[bucketNumber].PrincipalPastDue +
                                        outputGrid[bucketNumber].InterestPastDue + outputGrid[bucketNumber].ServiceFeePastDue +
                                        outputGrid[bucketNumber].ServiceFeeInterestPastDue);
                        }

                        //Set all past due balances to 0, in the oldest outstanding row.
                        outputGrid[bucketNumber].PrincipalPastDue = 0;
                        outputGrid[bucketNumber].InterestPastDue = 0;
                        outputGrid[bucketNumber].ServiceFeePastDue = 0;
                        outputGrid[bucketNumber].ServiceFeeInterestPastDue = 0;
                        outputGrid[bucketNumber].OriginationFeePastDue = 0;
                        outputGrid[bucketNumber].MaintenanceFeePastDue = 0;
                        outputGrid[bucketNumber].ManagementFeePastDue = 0;
                        outputGrid[bucketNumber].SameDayFeePastDue = 0;
                        outputGrid[bucketNumber].TotalPastDue = 0;

                        //Set all Cumulative past due balances of next row to 0.
                        for (int i = bucketNumber + 1; i <= currentOutputGridRow; i++)
                        {
                            outputGrid[i].CumulativePrincipalPastDue = Math.Round(outputGrid[i].CumulativePrincipalPastDue - outputGrid[bucketNumber].CumulativePrincipalPastDue, 2, MidpointRounding.AwayFromZero);
                            outputGrid[i].CumulativeInterestPastDue = Math.Round(outputGrid[i].CumulativeInterestPastDue - outputGrid[bucketNumber].CumulativeInterestPastDue, 2, MidpointRounding.AwayFromZero);
                            outputGrid[i].CumulativeServiceFeePastDue = Math.Round(outputGrid[i].CumulativeServiceFeePastDue - outputGrid[bucketNumber].CumulativeServiceFeePastDue, 2, MidpointRounding.AwayFromZero);
                            outputGrid[i].CumulativeServiceFeeInterestPastDue = Math.Round(outputGrid[i].CumulativeServiceFeeInterestPastDue - outputGrid[bucketNumber].CumulativeServiceFeeInterestPastDue, 2, MidpointRounding.AwayFromZero);
                            outputGrid[i].CumulativeOriginationFeePastDue = Math.Round(outputGrid[i].CumulativeOriginationFeePastDue - outputGrid[bucketNumber].CumulativeOriginationFeePastDue, 2, MidpointRounding.AwayFromZero);
                            outputGrid[i].CumulativeMaintenanceFeePastDue = Math.Round(outputGrid[i].CumulativeMaintenanceFeePastDue - outputGrid[bucketNumber].CumulativeMaintenanceFeePastDue, 2, MidpointRounding.AwayFromZero);
                            outputGrid[i].CumulativeManagementFeePastDue = Math.Round(outputGrid[i].CumulativeManagementFeePastDue - outputGrid[bucketNumber].CumulativeManagementFeePastDue, 2, MidpointRounding.AwayFromZero);
                            outputGrid[i].CumulativeSameDayFeePastDue = Math.Round(outputGrid[i].CumulativeSameDayFeePastDue - outputGrid[bucketNumber].CumulativeSameDayFeePastDue, 2, MidpointRounding.AwayFromZero);
                            outputGrid[i].CumulativeTotalPastDue -= (outputGrid[bucketNumber].CumulativePrincipalPastDue + outputGrid[bucketNumber].CumulativeInterestPastDue +
                                                        outputGrid[bucketNumber].CumulativeServiceFeePastDue + outputGrid[bucketNumber].CumulativeServiceFeeInterestPastDue +
                                                        outputGrid[bucketNumber].CumulativeOriginationFeePastDue + outputGrid[bucketNumber].CumulativeMaintenanceFeePastDue +
                                                        outputGrid[bucketNumber].CumulativeManagementFeePastDue + outputGrid[bucketNumber].CumulativeSameDayFeePastDue);
                            outputGrid[i].CumulativeTotalPastDue = Math.Round(outputGrid[i].CumulativeTotalPastDue, 2, MidpointRounding.AwayFromZero);
                        }

                        //Set all Cumulative past due balances to 0, in the oldest outstanding row.
                        outputGrid[bucketNumber].CumulativePrincipalPastDue = 0;
                        outputGrid[bucketNumber].CumulativeInterestPastDue = 0;
                        outputGrid[bucketNumber].CumulativeServiceFeePastDue = 0;
                        outputGrid[bucketNumber].CumulativeServiceFeeInterestPastDue = 0;
                        outputGrid[bucketNumber].CumulativeOriginationFeePastDue = 0;
                        outputGrid[bucketNumber].CumulativeMaintenanceFeePastDue = 0;
                        outputGrid[bucketNumber].CumulativeManagementFeePastDue = 0;
                        outputGrid[bucketNumber].CumulativeSameDayFeePastDue = 0;
                        outputGrid[bucketNumber].CumulativeTotalPastDue = outputGrid[bucketNumber].CumulativeNSFFeePastDue + outputGrid[bucketNumber].CumulativeLateFeePastDue;
                        outputGrid[bucketNumber].BucketStatus = "Satisfied";
                    }
                    else
                    {
                        for (int i = 1; i <= 10; i++)
                        {
                            //Allocate funds to interest amount
                            if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestPriority == i)
                            {
                                if (outputGrid[bucketNumber].InterestPastDue <= paymentAmount)
                                {
                                    outputGrid[currentOutputGridRow].InterestPaid += outputGrid[bucketNumber].InterestPastDue;
                                    paymentAmount -= outputGrid[bucketNumber].InterestPastDue;
                                    if ((lastRowIndex != bucketNumber) && isExtendingSchedule)
                                    {
                                        outputGrid[lastRowIndex].InterestPastDue -= outputGrid[bucketNumber].InterestPastDue;
                                    }
                                    outputGrid[bucketNumber].InterestPastDue = 0;
                                    outputGrid[bucketNumber].CumulativeInterestPastDue = 0;
                                }
                                else
                                {
                                    outputGrid[currentOutputGridRow].InterestPaid += paymentAmount;
                                    outputGrid[bucketNumber].InterestPastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativeInterestPastDue -= paymentAmount;
                                    if ((lastRowIndex != bucketNumber) && isExtendingSchedule)
                                    {
                                        outputGrid[lastRowIndex].InterestPastDue -= paymentAmount;
                                    }
                                    paymentAmount = 0;
                                    break;
                                }
                            }

                            //Allocate funds to principal amount
                            else if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].PrincipalPriority == i)
                            {
                                if (outputGrid[bucketNumber].PrincipalPastDue <= paymentAmount)
                                {
                                    outputGrid[currentOutputGridRow].PrincipalPaid += outputGrid[bucketNumber].PrincipalPastDue;
                                    paymentAmount -= outputGrid[bucketNumber].PrincipalPastDue;
                                    if ((lastRowIndex != bucketNumber) && isExtendingSchedule)
                                    {
                                        outputGrid[lastRowIndex].PrincipalPastDue -= outputGrid[bucketNumber].PrincipalPastDue;
                                    }
                                    outputGrid[bucketNumber].PrincipalPastDue = 0;
                                    outputGrid[bucketNumber].CumulativePrincipalPastDue = 0;
                                }
                                else
                                {
                                    outputGrid[currentOutputGridRow].PrincipalPaid += paymentAmount;
                                    outputGrid[bucketNumber].PrincipalPastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativePrincipalPastDue -= paymentAmount;
                                    if ((lastRowIndex != bucketNumber) && isExtendingSchedule)
                                    {
                                        outputGrid[lastRowIndex].PrincipalPastDue -= paymentAmount;
                                    }
                                    paymentAmount = 0;
                                    break;
                                }
                            }

                            //Allocate funds to Service Fee amount
                            else if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].ServiceFeePriority == i)
                            {
                                if (outputGrid[bucketNumber].ServiceFeePastDue <= paymentAmount)
                                {
                                    outputGrid[currentOutputGridRow].ServiceFeePaid += outputGrid[bucketNumber].ServiceFeePastDue;
                                    paymentAmount -= outputGrid[bucketNumber].ServiceFeePastDue;
                                    if ((lastRowIndex != bucketNumber) && isExtendingSchedule)
                                    {
                                        outputGrid[lastRowIndex].ServiceFeePastDue -= outputGrid[bucketNumber].ServiceFeePastDue;
                                    }
                                    outputGrid[bucketNumber].ServiceFeePastDue = 0;
                                    outputGrid[bucketNumber].CumulativeServiceFeePastDue = 0;
                                }
                                else
                                {
                                    outputGrid[currentOutputGridRow].ServiceFeePaid += paymentAmount;
                                    outputGrid[bucketNumber].ServiceFeePastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativeServiceFeePastDue -= paymentAmount;
                                    if ((lastRowIndex != bucketNumber) && isExtendingSchedule)
                                    {
                                        outputGrid[lastRowIndex].ServiceFeePastDue -= paymentAmount;
                                    }
                                    paymentAmount = 0;
                                    break;
                                }
                            }

                            //Allocate funds to Service Fee Interest amount
                            else if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].ServiceFeeInterestPriority == i)
                            {
                                if (outputGrid[bucketNumber].ServiceFeeInterestPastDue <= paymentAmount)
                                {
                                    outputGrid[currentOutputGridRow].ServiceFeeInterestPaid += outputGrid[bucketNumber].ServiceFeeInterestPastDue;
                                    paymentAmount -= outputGrid[bucketNumber].ServiceFeeInterestPastDue;
                                    if ((lastRowIndex != bucketNumber) && isExtendingSchedule)
                                    {
                                        outputGrid[lastRowIndex].ServiceFeeInterestPastDue -= outputGrid[bucketNumber].ServiceFeeInterestPastDue;
                                    }
                                    outputGrid[bucketNumber].ServiceFeeInterestPastDue = 0;
                                    outputGrid[bucketNumber].CumulativeServiceFeeInterestPastDue = 0;
                                }
                                else
                                {
                                    outputGrid[currentOutputGridRow].ServiceFeeInterestPaid += paymentAmount;
                                    outputGrid[bucketNumber].ServiceFeeInterestPastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativeServiceFeeInterestPastDue -= paymentAmount;
                                    if ((lastRowIndex != bucketNumber) && isExtendingSchedule)
                                    {
                                        outputGrid[lastRowIndex].ServiceFeeInterestPastDue -= paymentAmount;
                                    }
                                    paymentAmount = 0;
                                    break;
                                }
                            }

                            //Allocate funds to Origination Fee amount
                            else if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].OriginationFeePriority == i)
                            {
                                if (outputGrid[bucketNumber].OriginationFeePastDue <= paymentAmount)
                                {
                                    outputGrid[currentOutputGridRow].OriginationFeePaid += outputGrid[bucketNumber].OriginationFeePastDue;
                                    paymentAmount -= outputGrid[bucketNumber].OriginationFeePastDue;
                                    outputGrid[bucketNumber].OriginationFeePastDue = 0;
                                    outputGrid[bucketNumber].CumulativeOriginationFeePastDue = 0;
                                }
                                else
                                {
                                    outputGrid[currentOutputGridRow].OriginationFeePaid += paymentAmount;
                                    outputGrid[bucketNumber].OriginationFeePastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativeOriginationFeePastDue -= paymentAmount;
                                    paymentAmount = 0;
                                    break;
                                }
                            }

                            //Allocate funds to Management Fee amount
                            else if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].ManagementFeePriority == i)
                            {
                                if (outputGrid[bucketNumber].ManagementFeePastDue <= paymentAmount)
                                {
                                    outputGrid[currentOutputGridRow].ManagementFeePaid += outputGrid[bucketNumber].ManagementFeePastDue;
                                    paymentAmount -= outputGrid[bucketNumber].ManagementFeePastDue;
                                    outputGrid[bucketNumber].ManagementFeePastDue = 0;
                                    outputGrid[bucketNumber].CumulativeManagementFeePastDue = 0;
                                }
                                else
                                {
                                    outputGrid[currentOutputGridRow].ManagementFeePaid += paymentAmount;
                                    outputGrid[bucketNumber].ManagementFeePastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativeManagementFeePastDue -= paymentAmount;
                                    paymentAmount = 0;
                                    break;
                                }
                            }

                            //Allocate funds to Maintenance Fee amount
                            else if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].MaintenanceFeePriority == i)
                            {
                                if (outputGrid[bucketNumber].MaintenanceFeePastDue <= paymentAmount)
                                {
                                    outputGrid[currentOutputGridRow].MaintenanceFeePaid += outputGrid[bucketNumber].MaintenanceFeePastDue;
                                    paymentAmount -= outputGrid[bucketNumber].MaintenanceFeePastDue;
                                    outputGrid[bucketNumber].MaintenanceFeePastDue = 0;
                                    outputGrid[bucketNumber].CumulativeMaintenanceFeePastDue = 0;
                                }
                                else
                                {
                                    outputGrid[currentOutputGridRow].MaintenanceFeePaid += paymentAmount;
                                    outputGrid[bucketNumber].MaintenanceFeePastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativeMaintenanceFeePastDue -= paymentAmount;
                                    paymentAmount = 0;
                                    break;
                                }
                            }

                            //Allocate funds to Same Day Fee amount
                            else if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].SameDayFeePriority == i)
                            {
                                if (outputGrid[bucketNumber].SameDayFeePastDue <= paymentAmount)
                                {
                                    outputGrid[currentOutputGridRow].SameDayFeePaid += outputGrid[bucketNumber].SameDayFeePastDue;
                                    paymentAmount -= outputGrid[bucketNumber].SameDayFeePastDue;
                                    outputGrid[bucketNumber].SameDayFeePastDue = 0;
                                    outputGrid[bucketNumber].CumulativeSameDayFeePastDue = 0;
                                }
                                else
                                {
                                    outputGrid[currentOutputGridRow].SameDayFeePaid += paymentAmount;
                                    outputGrid[bucketNumber].SameDayFeePastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativeSameDayFeePastDue -= paymentAmount;
                                    paymentAmount = 0;
                                    break;
                                }
                            }
                            paymentAmount = Math.Round(paymentAmount, 2, MidpointRounding.AwayFromZero);
                        }

                        if (isExtendingSchedule)
                        {
                            outputGrid[lastRowIndex].TotalPastDue = outputGrid[lastRowIndex].PrincipalPastDue + outputGrid[lastRowIndex].InterestPastDue +
                                                        outputGrid[lastRowIndex].ServiceFeePastDue + outputGrid[lastRowIndex].ServiceFeeInterestPastDue +
                                                        outputGrid[lastRowIndex].OriginationFeePastDue + outputGrid[lastRowIndex].MaintenanceFeePastDue +
                                                        outputGrid[lastRowIndex].ManagementFeePastDue + outputGrid[lastRowIndex].SameDayFeePastDue +
                                                        outputGrid[lastRowIndex].NSFFeePastDue + outputGrid[lastRowIndex].LateFeePastDue;
                        }

                        //This loop recalculates the cumulative past due columns from next index of selected index, to current index.
                        for (int i = bucketNumber + 1; i <= currentOutputGridRow; i++)
                        {
                            outputGrid[i].CumulativeInterestPastDue = outputGrid[i - 1].CumulativeInterestPastDue + outputGrid[i].InterestPastDue;
                            outputGrid[i].CumulativePrincipalPastDue = outputGrid[i - 1].CumulativePrincipalPastDue + outputGrid[i].PrincipalPastDue;
                            outputGrid[i].CumulativeServiceFeePastDue = outputGrid[i - 1].CumulativeServiceFeePastDue + outputGrid[i].ServiceFeePastDue;
                            outputGrid[i].CumulativeServiceFeeInterestPastDue = outputGrid[i - 1].CumulativeServiceFeeInterestPastDue + outputGrid[i].ServiceFeeInterestPastDue;
                            outputGrid[i].CumulativeOriginationFeePastDue = outputGrid[i - 1].CumulativeOriginationFeePastDue + outputGrid[i].OriginationFeePastDue;
                            outputGrid[i].CumulativeManagementFeePastDue = outputGrid[i - 1].CumulativeManagementFeePastDue + outputGrid[i].ManagementFeePastDue;
                            outputGrid[i].CumulativeMaintenanceFeePastDue = outputGrid[i - 1].CumulativeMaintenanceFeePastDue + outputGrid[i].MaintenanceFeePastDue;
                            outputGrid[i].CumulativeSameDayFeePastDue = outputGrid[i - 1].CumulativeSameDayFeePastDue + outputGrid[i].SameDayFeePastDue;
                            outputGrid[i].CumulativeTotalPastDue = outputGrid[i].CumulativePrincipalPastDue + outputGrid[i].CumulativeInterestPastDue +
                                                        outputGrid[i].CumulativeServiceFeePastDue + outputGrid[i].CumulativeServiceFeeInterestPastDue +
                                                        outputGrid[i].CumulativeOriginationFeePastDue + outputGrid[i].CumulativeMaintenanceFeePastDue +
                                                        outputGrid[i].CumulativeManagementFeePastDue + outputGrid[i].CumulativeSameDayFeePastDue +
                                                        outputGrid[i].CumulativeNSFFeePastDue + outputGrid[i].CumulativeLateFeePastDue;
                        }
                        outputGrid[bucketNumber].TotalPaid = outputGrid[bucketNumber].PrincipalPaid + outputGrid[bucketNumber].InterestPaid +
                                                        outputGrid[bucketNumber].ServiceFeePaid + outputGrid[bucketNumber].ServiceFeeInterestPaid +
                                                        outputGrid[bucketNumber].OriginationFeePaid + outputGrid[bucketNumber].MaintenanceFeePaid +
                                                        outputGrid[bucketNumber].ManagementFeePaid + outputGrid[bucketNumber].SameDayFeePaid +
                                                        outputGrid[bucketNumber].NSFFeePaid + outputGrid[bucketNumber].LateFeePaid;
                        outputGrid[bucketNumber].TotalPastDue = outputGrid[bucketNumber].PrincipalPastDue + outputGrid[bucketNumber].InterestPastDue +
                                                        outputGrid[bucketNumber].ServiceFeePastDue + outputGrid[bucketNumber].ServiceFeeInterestPastDue +
                                                        outputGrid[bucketNumber].OriginationFeePastDue + outputGrid[bucketNumber].MaintenanceFeePastDue +
                                                        outputGrid[bucketNumber].ManagementFeePastDue + outputGrid[bucketNumber].SameDayFeePastDue +
                                                        outputGrid[bucketNumber].NSFFeePastDue + outputGrid[bucketNumber].LateFeePastDue;
                        outputGrid[bucketNumber].CumulativeTotalPastDue = outputGrid[bucketNumber].CumulativePrincipalPastDue + outputGrid[bucketNumber].CumulativeInterestPastDue +
                                                        outputGrid[bucketNumber].CumulativeServiceFeePastDue + outputGrid[bucketNumber].CumulativeServiceFeeInterestPastDue +
                                                        outputGrid[bucketNumber].CumulativeOriginationFeePastDue + outputGrid[bucketNumber].CumulativeMaintenanceFeePastDue +
                                                        outputGrid[bucketNumber].CumulativeManagementFeePastDue + outputGrid[bucketNumber].CumulativeSameDayFeePastDue +
                                                        outputGrid[bucketNumber].CumulativeNSFFeePastDue + outputGrid[bucketNumber].CumulativeLateFeePastDue;

                        outputGrid[bucketNumber].BucketStatus = "OutStanding";
                    }
                    previousOutStandingBucketNumber = bucketNumber;
                }

                #endregion

                int firstScheduleIndex = 0;
                //Determine the first repayment schedule.
                for (int rowIndex = outputGrid.Count - 1; rowIndex >= 0; rowIndex--)
                {
                    if (scheduleInputs.InputRecords[1].DateIn == outputGrid[rowIndex].DueDate)
                    {
                        firstScheduleIndex = rowIndex;
                        break;
                    }
                }

                #region Amount allocation in current interest amounts

                for (int i = 1; i <= 10; i++)
                {
                    //Allocate funds to Management Fee amount
                    if ((scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].ManagementFeePriority == i) && isExtendingSchedule)
                    {
                        if (outputGrid[currentOutputGridRow].ManagementFeePastDue <= paymentAmount)
                        {
                            outputGrid[currentOutputGridRow].ManagementFeePaid += outputGrid[currentOutputGridRow].ManagementFeePastDue;
                            paymentAmount -= outputGrid[currentOutputGridRow].ManagementFeePastDue;
                            outputGrid[currentOutputGridRow].ManagementFeePastDue = 0;
                            outputGrid[currentOutputGridRow].CumulativeManagementFeePastDue = 0;
                        }
                        else
                        {
                            outputGrid[currentOutputGridRow].ManagementFeePaid += paymentAmount;
                            outputGrid[currentOutputGridRow].ManagementFeePastDue -= paymentAmount;
                            outputGrid[currentOutputGridRow].CumulativeManagementFeePastDue -= paymentAmount;
                            paymentAmount = 0;
                        }
                    }

                    //Allocate funds to Maintenance Fee amount
                    else if ((scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].MaintenanceFeePriority == i) && isExtendingSchedule)
                    {
                        if (outputGrid[currentOutputGridRow].MaintenanceFeePastDue <= paymentAmount)
                        {
                            outputGrid[currentOutputGridRow].MaintenanceFeePaid += outputGrid[currentOutputGridRow].MaintenanceFeePastDue;
                            paymentAmount -= outputGrid[currentOutputGridRow].MaintenanceFeePastDue;
                            outputGrid[currentOutputGridRow].MaintenanceFeePastDue = 0;
                            outputGrid[currentOutputGridRow].CumulativeMaintenanceFeePastDue = 0;
                        }
                        else
                        {
                            outputGrid[currentOutputGridRow].MaintenanceFeePaid += paymentAmount;
                            outputGrid[currentOutputGridRow].MaintenanceFeePastDue -= paymentAmount;
                            outputGrid[currentOutputGridRow].CumulativeMaintenanceFeePastDue -= paymentAmount;
                            paymentAmount = 0;
                        }
                    }

                    //Allocate funds to interest amount
                    else if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestPriority == i)
                    {
                        if (outputGrid[currentOutputGridRow].InterestCarryOver <= paymentAmount)
                        {
                            outputGrid[currentOutputGridRow].InterestPayment = Math.Round(outputGrid[currentOutputGridRow].InterestCarryOver, 2, MidpointRounding.AwayFromZero);
                            outputGrid[currentOutputGridRow].InterestCarryOver = 0;
                            paymentAmount -= outputGrid[currentOutputGridRow].InterestPayment;
                            outputGrid[currentOutputGridRow].InterestPaid += outputGrid[currentOutputGridRow].InterestPayment;
                        }
                        else
                        {
                            outputGrid[currentOutputGridRow].InterestPayment = Math.Round(paymentAmount, 2, MidpointRounding.AwayFromZero);
                            outputGrid[currentOutputGridRow].InterestCarryOver -= outputGrid[currentOutputGridRow].InterestPayment;
                            outputGrid[currentOutputGridRow].InterestCarryOver = Math.Round(outputGrid[currentOutputGridRow].InterestCarryOver, 2, MidpointRounding.AwayFromZero);
                            paymentAmount = 0;
                            outputGrid[currentOutputGridRow].InterestPaid += outputGrid[currentOutputGridRow].InterestPayment;
                        }
                        outputGrid[currentOutputGridRow].InterestPastDue = (outputGrid[currentOutputGridRow].InterestPastDue - paymentAmount) <= 0 ? 0 : (outputGrid[currentOutputGridRow].InterestPastDue - paymentAmount);
                        outputGrid[currentOutputGridRow].CumulativeInterestPastDue = (outputGrid[currentOutputGridRow].CumulativeInterestPastDue - paymentAmount) <= 0 ? 0 : (outputGrid[currentOutputGridRow].CumulativeInterestPastDue - paymentAmount);
                    }

                    //Allocate funds to Service Fee Interest amount
                    else if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].ServiceFeeInterestPriority == i)
                    {
                        if (outputGrid[currentOutputGridRow].AccruedServiceFeeInterestCarryOver <= paymentAmount)
                        {
                            outputGrid[currentOutputGridRow].ServiceFeeInterest = Math.Round(outputGrid[currentOutputGridRow].AccruedServiceFeeInterestCarryOver, 2, MidpointRounding.AwayFromZero);
                            outputGrid[currentOutputGridRow].AccruedServiceFeeInterestCarryOver = 0;
                            paymentAmount -= outputGrid[currentOutputGridRow].ServiceFeeInterest;
                            outputGrid[currentOutputGridRow].ServiceFeeInterestPaid += outputGrid[currentOutputGridRow].ServiceFeeInterest;
                        }
                        else
                        {
                            outputGrid[currentOutputGridRow].ServiceFeeInterest = Math.Round(paymentAmount, 2, MidpointRounding.AwayFromZero);
                            outputGrid[currentOutputGridRow].AccruedServiceFeeInterestCarryOver -= outputGrid[currentOutputGridRow].ServiceFeeInterest;
                            outputGrid[currentOutputGridRow].AccruedServiceFeeInterestCarryOver = Math.Round(outputGrid[currentOutputGridRow].AccruedServiceFeeInterestCarryOver, 2, MidpointRounding.AwayFromZero);
                            paymentAmount = 0;
                            outputGrid[currentOutputGridRow].ServiceFeeInterestPaid += outputGrid[currentOutputGridRow].ServiceFeeInterest;
                        }
                        outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue = (outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue - paymentAmount) <= 0 ? 0 : (outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue - paymentAmount);
                        outputGrid[currentOutputGridRow].CumulativeServiceFeeInterestPastDue = (outputGrid[currentOutputGridRow].CumulativeServiceFeeInterestPastDue - paymentAmount) <= 0 ? 0 : (outputGrid[currentOutputGridRow].CumulativeServiceFeeInterestPastDue - paymentAmount);
                    }

                    //Allocate funds to Origination Fee amount
                    else if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].OriginationFeePriority == i)
                    {
                        //Check whether the additional payment date is before the first repayment schedule date.
                        if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].DateIn <= outputGrid[firstScheduleIndex].PaymentDate)
                        {
                            if (outputGrid[firstScheduleIndex].OriginationFee <= paymentAmount)
                            {
                                outputGrid[currentOutputGridRow].OriginationFeePaid += outputGrid[firstScheduleIndex].OriginationFee;
                                paymentAmount -= outputGrid[firstScheduleIndex].OriginationFee;
                                outputGrid[currentOutputGridRow].OriginationFee = outputGrid[firstScheduleIndex].OriginationFee;
                                outputGrid[firstScheduleIndex].OriginationFee = 0;
                            }
                            else
                            {
                                outputGrid[currentOutputGridRow].OriginationFeePaid += paymentAmount;
                                outputGrid[firstScheduleIndex].OriginationFee -= paymentAmount;
                                outputGrid[currentOutputGridRow].OriginationFee = paymentAmount;
                                paymentAmount = 0;
                                break;
                            }
                        }
                        //Check whether there is an amount remains to pay due to skipping the first repayment schedule.
                        else if (originationFee > 0)
                        {
                            if (originationFee <= paymentAmount)
                            {
                                outputGrid[currentOutputGridRow].OriginationFee = originationFee;
                                outputGrid[currentOutputGridRow].OriginationFeePaid += originationFee;
                                paymentAmount -= originationFee;
                            }
                            else
                            {
                                outputGrid[currentOutputGridRow].OriginationFee = paymentAmount;
                                outputGrid[currentOutputGridRow].OriginationFeePaid += paymentAmount;
                                paymentAmount = 0;
                                break;
                            }
                        }
                    }

                    //Allocate funds to Same Day Fee amount
                    else if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].SameDayFeePriority == i)
                    {
                        //Check whether the additional payment date is before the first repayment schedule date.
                        if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].DateIn <= outputGrid[firstScheduleIndex].PaymentDate)
                        {
                            if (outputGrid[firstScheduleIndex].SameDayFee <= paymentAmount)
                            {
                                outputGrid[currentOutputGridRow].SameDayFeePaid += outputGrid[firstScheduleIndex].SameDayFee;
                                paymentAmount -= outputGrid[firstScheduleIndex].SameDayFee;
                                outputGrid[currentOutputGridRow].SameDayFee = outputGrid[firstScheduleIndex].SameDayFee;
                                outputGrid[firstScheduleIndex].SameDayFee = 0;
                            }
                            else
                            {
                                outputGrid[currentOutputGridRow].SameDayFeePaid += paymentAmount;
                                outputGrid[firstScheduleIndex].SameDayFee -= paymentAmount;
                                outputGrid[currentOutputGridRow].SameDayFee = paymentAmount;
                                paymentAmount = 0;
                                break;
                            }
                        }
                        //Check whether there is an amount remains to pay due to skipping the first repayment schedule.
                        else if (sameDayFee > 0)
                        {
                            if (sameDayFee <= paymentAmount)
                            {
                                outputGrid[currentOutputGridRow].SameDayFee = sameDayFee;
                                outputGrid[currentOutputGridRow].SameDayFeePaid += sameDayFee;
                                paymentAmount -= sameDayFee;
                            }
                            else
                            {
                                outputGrid[currentOutputGridRow].SameDayFee = paymentAmount;
                                outputGrid[currentOutputGridRow].SameDayFeePaid += paymentAmount;
                                paymentAmount = 0;
                                break;
                            }
                        }
                    }
                    paymentAmount = Math.Round(paymentAmount, 2, MidpointRounding.AwayFromZero);
                    if (paymentAmount <= 0)
                    {
                        break;
                    }
                }

                outputGrid[currentOutputGridRow].InterestPastDue = Math.Round(outputGrid[currentOutputGridRow].InterestPastDue, 2, MidpointRounding.AwayFromZero);
                outputGrid[currentOutputGridRow].InterestPaid = Math.Round(outputGrid[currentOutputGridRow].InterestPaid, 2, MidpointRounding.AwayFromZero);
                outputGrid[currentOutputGridRow].CumulativeInterestPastDue = Math.Round(outputGrid[currentOutputGridRow].CumulativeInterestPastDue, 2, MidpointRounding.AwayFromZero);
                outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue = Math.Round(outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue, 2, MidpointRounding.AwayFromZero);
                outputGrid[currentOutputGridRow].ServiceFeeInterestPaid = Math.Round(outputGrid[currentOutputGridRow].ServiceFeeInterestPaid, 2, MidpointRounding.AwayFromZero);
                outputGrid[currentOutputGridRow].CumulativeServiceFeeInterestPastDue = Math.Round(outputGrid[currentOutputGridRow].CumulativeServiceFeeInterestPastDue, 2, MidpointRounding.AwayFromZero);
                outputGrid[currentOutputGridRow].ServiceFeeTotal = outputGrid[currentOutputGridRow].ServiceFeeInterest;

                #endregion

                //This condition checks whether there is any payment amount remains after pay down all the outstanding amount as well as current bucket.
                if (paymentAmount > 0)
                {
                    //Allocate the remaining amount to the action fees i.e. NSF fee and late fee.
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayAdditionalPayment() method. Calling method : AllocateRemainingFundsToNSFAndLateFee(outputGrid, scheduleInputs, additionalPaymentRow, currentOutputGridRow, true, ref paymentAmount);");
                    AllocateRemainingFundsToNSFAndLateFee(outputGrid, scheduleInputs, additionalPaymentRow, currentOutputGridRow, true, ref paymentAmount);
                }

                remainingPrincipal = outputGrid[0].BeginningPrincipal;
                remainingPrincipalServiceFee = outputGrid[0].BeginningServiceFee;
                //Calculate the remaining principal and servie fee amount to be paid.
                for (int i = 0; i <= currentOutputGridRow; i++)
                {
                    remainingPrincipal -= outputGrid[i].PrincipalPaid;
                    remainingPrincipalServiceFee -= outputGrid[i].ServiceFeePaid;
                }
                remainingPrincipal = Math.Round(remainingPrincipal, 2, MidpointRounding.AwayFromZero);
                remainingPrincipalServiceFee = Math.Round(remainingPrincipalServiceFee, 2, MidpointRounding.AwayFromZero);

                //Check whether the priority of service fee is greater than the principal amount
                if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].ServiceFeePriority > scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].PrincipalPriority)
                {
                    //Allocate funds to principal amount
                    if (remainingPrincipal <= paymentAmount)
                    {
                        outputGrid[currentOutputGridRow].PrincipalPaid += remainingPrincipal;
                        paymentAmount -= remainingPrincipal;
                        remainingPrincipal = 0;
                    }
                    else
                    {
                        outputGrid[currentOutputGridRow].PrincipalPaid += paymentAmount;
                        remainingPrincipal = remainingPrincipal - paymentAmount;
                        paymentAmount = 0;
                    }

                    //Allocate funds to Service Fee amount
                    if (remainingPrincipalServiceFee <= paymentAmount)
                    {
                        outputGrid[currentOutputGridRow].ServiceFeePaid += remainingPrincipalServiceFee;
                        paymentAmount -= remainingPrincipalServiceFee;
                        remainingPrincipalServiceFee = 0;
                    }
                    else
                    {
                        outputGrid[currentOutputGridRow].ServiceFeePaid += paymentAmount;
                        remainingPrincipalServiceFee = remainingPrincipalServiceFee - paymentAmount;
                        paymentAmount = 0;
                    }
                }
                else
                {
                    //Allocate funds to Service Fee amount
                    if (remainingPrincipalServiceFee <= paymentAmount)
                    {
                        outputGrid[currentOutputGridRow].ServiceFeePaid += remainingPrincipalServiceFee;
                        paymentAmount -= remainingPrincipalServiceFee;
                        remainingPrincipalServiceFee = 0;
                    }
                    else
                    {
                        outputGrid[currentOutputGridRow].ServiceFeePaid += paymentAmount;
                        remainingPrincipalServiceFee = remainingPrincipalServiceFee - paymentAmount;
                        paymentAmount = 0;
                    }

                    //Allocate funds to Service Fee amount
                    if (remainingPrincipal <= paymentAmount)
                    {
                        outputGrid[currentOutputGridRow].PrincipalPaid += remainingPrincipal;
                        paymentAmount -= remainingPrincipal;
                        remainingPrincipal = 0;
                    }
                    else
                    {
                        outputGrid[currentOutputGridRow].PrincipalPaid += paymentAmount;
                        remainingPrincipal = remainingPrincipal - paymentAmount;
                        paymentAmount = 0;
                    }
                }

                //Recalculate values in output grid as per remaining loan amount and service fee when the event is not the part of extending the repayment schedule.
                if (!isExtendingSchedule)
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayAdditionalPayment() method. Calling method : ReCalculateOutputGridValues(remainingPrincipal, remainingprincipalServiceFee, outputGrid, scheduleInputs, currentOutputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);");
                    ReCalculateOutputGridValues(remainingPrincipal, remainingPrincipalServiceFee, outputGrid, scheduleInputs, currentOutputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);
                }
                else
                {
                    outputGrid[currentOutputGridRow].TotalPastDue = outputGrid[currentOutputGridRow].PrincipalPastDue + outputGrid[currentOutputGridRow].InterestPastDue +
                                        outputGrid[currentOutputGridRow].ServiceFeePastDue + outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue +
                                        outputGrid[currentOutputGridRow].ManagementFeePastDue + outputGrid[currentOutputGridRow].MaintenanceFeePastDue +
                                        outputGrid[currentOutputGridRow].NSFFeePastDue + outputGrid[currentOutputGridRow].LateFeePastDue +
                                        outputGrid[currentOutputGridRow].OriginationFeePastDue + outputGrid[currentOutputGridRow].SameDayFeePastDue;
                    outputGrid[currentOutputGridRow].CumulativeTotalPastDue = outputGrid[currentOutputGridRow - 1].CumulativeTotalPastDue +
                                                                                          outputGrid[currentOutputGridRow].TotalPastDue;
                }

                outputGrid[currentOutputGridRow].TotalPaid = outputGrid[currentOutputGridRow].PrincipalPaid + outputGrid[currentOutputGridRow].InterestPaid +
                                                        outputGrid[currentOutputGridRow].ServiceFeePaid + outputGrid[currentOutputGridRow].ServiceFeeInterestPaid +
                                                        outputGrid[currentOutputGridRow].OriginationFeePaid + outputGrid[currentOutputGridRow].MaintenanceFeePaid +
                                                        outputGrid[currentOutputGridRow].ManagementFeePaid + outputGrid[currentOutputGridRow].SameDayFeePaid +
                                                        outputGrid[currentOutputGridRow].NSFFeePaid + outputGrid[currentOutputGridRow].LateFeePaid;

                //Cumulative amount paid is added in current payament row
                outputGrid[currentOutputGridRow].CumulativePrincipal = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativePrincipal + outputGrid[currentOutputGridRow].PrincipalPaid;
                outputGrid[currentOutputGridRow].CumulativeInterest = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeInterest + outputGrid[currentOutputGridRow].InterestPaid;
                outputGrid[currentOutputGridRow].CumulativePayment = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativePayment + outputGrid[currentOutputGridRow].TotalPaid;
                outputGrid[currentOutputGridRow].CumulativeServiceFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeServiceFee + outputGrid[currentOutputGridRow].ServiceFeePaid;
                outputGrid[currentOutputGridRow].CumulativeServiceFeeInterest = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeServiceFeeInterest + outputGrid[currentOutputGridRow].ServiceFeeInterestPaid;
                outputGrid[currentOutputGridRow].CumulativeServiceFeeTotal = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeServiceFeeTotal +
                                                                    outputGrid[currentOutputGridRow].ServiceFeePaid + outputGrid[currentOutputGridRow].ServiceFeeInterestPaid;
                outputGrid[currentOutputGridRow].CumulativeOriginationFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeOriginationFee + outputGrid[currentOutputGridRow].OriginationFeePaid;
                outputGrid[currentOutputGridRow].CumulativeMaintenanceFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeMaintenanceFee + outputGrid[currentOutputGridRow].MaintenanceFeePaid;
                outputGrid[currentOutputGridRow].CumulativeManagementFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeManagementFee + outputGrid[currentOutputGridRow].ManagementFeePaid;
                outputGrid[currentOutputGridRow].CumulativeSameDayFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeSameDayFee + outputGrid[currentOutputGridRow].SameDayFeePaid;
                outputGrid[currentOutputGridRow].CumulativeNSFFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeNSFFee + outputGrid[currentOutputGridRow].NSFFeePaid;
                outputGrid[currentOutputGridRow].CumulativeLateFee = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeLateFee + outputGrid[currentOutputGridRow].LateFeePaid;
                outputGrid[currentOutputGridRow].CumulativeTotalFees = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeTotalFees +
                                outputGrid[currentOutputGridRow].ServiceFeePaid + outputGrid[currentOutputGridRow].ServiceFeeInterestPaid + outputGrid[currentOutputGridRow].OriginationFeePaid +
                                outputGrid[currentOutputGridRow].MaintenanceFeePaid + outputGrid[currentOutputGridRow].ManagementFeePaid + outputGrid[currentOutputGridRow].SameDayFeePaid +
                                outputGrid[currentOutputGridRow].NSFFeePaid + outputGrid[currentOutputGridRow].LateFeePaid;

                //Cumulative amount past due is added in current row
                if (outputGrid.Count > currentOutputGridRow + 1)
                {
                    outputGrid[currentOutputGridRow + 1].CumulativePrincipalPastDue = outputGrid[currentOutputGridRow].CumulativePrincipalPastDue + outputGrid[currentOutputGridRow + 1].PrincipalPayment;
                    outputGrid[currentOutputGridRow + 1].CumulativeInterestPastDue = outputGrid[currentOutputGridRow].CumulativeInterestPastDue + outputGrid[currentOutputGridRow + 1].InterestPayment;
                    outputGrid[currentOutputGridRow + 1].CumulativeServiceFeePastDue = outputGrid[currentOutputGridRow].CumulativeServiceFeePastDue + outputGrid[currentOutputGridRow + 1].ServiceFee;
                    outputGrid[currentOutputGridRow + 1].CumulativeServiceFeeInterestPastDue = outputGrid[currentOutputGridRow].CumulativeServiceFeeInterestPastDue + outputGrid[currentOutputGridRow + 1].ServiceFeeInterest;
                    outputGrid[currentOutputGridRow + 1].CumulativeOriginationFeePastDue = outputGrid[currentOutputGridRow].CumulativeOriginationFeePastDue + outputGrid[currentOutputGridRow + 1].OriginationFee;
                    outputGrid[currentOutputGridRow + 1].CumulativeMaintenanceFeePastDue = outputGrid[currentOutputGridRow].CumulativeMaintenanceFeePastDue + outputGrid[currentOutputGridRow + 1].MaintenanceFee;
                    outputGrid[currentOutputGridRow + 1].CumulativeManagementFeePastDue = outputGrid[currentOutputGridRow].CumulativeManagementFeePastDue + outputGrid[currentOutputGridRow + 1].ManagementFee;
                    outputGrid[currentOutputGridRow + 1].CumulativeSameDayFeePastDue = outputGrid[currentOutputGridRow].CumulativeSameDayFeePastDue + outputGrid[currentOutputGridRow + 1].SameDayFee;
                    outputGrid[currentOutputGridRow + 1].CumulativeNSFFeePastDue = outputGrid[currentOutputGridRow].CumulativeNSFFeePastDue;
                    outputGrid[currentOutputGridRow + 1].CumulativeLateFeePastDue = outputGrid[currentOutputGridRow].CumulativeLateFeePastDue;
                    outputGrid[currentOutputGridRow + 1].CumulativeTotalPastDue = outputGrid[currentOutputGridRow].CumulativeTotalPastDue + outputGrid[currentOutputGridRow + 1].TotalPayment;
                }
                sbTracing.AppendLine("Exist:From class ActualPaymentAllocation and method name PayAdditionalPayment(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, int scheduleInputGridRow, " +
                    "int currentOutputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee)");
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }

        #endregion

        #region For adding a Discount

        /// <summary>
        /// This function will give discount on the components for the rolling method. After deducting the discount amount, the remaining fund will be used to craete to 
        /// recalculate the repayment schedule.
        /// </summary>
        /// <param name="outputGrid"></param>
        /// <param name="scheduleInputs"></param>
        /// <param name="additionalPaymentRow"></param>
        /// <param name="scheduleInputGridRow"></param>
        /// <param name="outputGridRow"></param>
        /// <param name="defaultTotalAmountToPay"></param>
        /// <param name="defaultTotalServiceFeePayable"></param>
        /// <param name="isExtendingSchedule"></param>
        /// <param name="originationFee"></param>
        /// <param name="sameDayFee"></param>
        private static void AddDiscount(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, int scheduleInputGridRow, int outputGridRow,
                        double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name AddDiscount(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, " +
                    "int scheduleInputGridRow, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee).");
                sbTracing.AppendLine("Parameter values are : additionalPaymentRow = " + additionalPaymentRow + ", scheduleInputGridRow = " + scheduleInputGridRow + ", outputGridRow = " + outputGridRow +
                    ", defaultTotalAmountToPay = " + defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", isExtendingSchedule = " + isExtendingSchedule +
                    ", originationFee = " + originationFee + ", sameDayFee = " + sameDayFee);

                double remainingPrincipal = isExtendingSchedule ? (outputGrid[outputGridRow - 1].BeginningPrincipal - outputGrid[outputGridRow - 1].PrincipalPaid) :
                                                                    outputGrid[outputGridRow].BeginningPrincipal;
                double remainingPrincipalServiceFee = isExtendingSchedule ? (outputGrid[outputGridRow - 1].BeginningServiceFee - outputGrid[outputGridRow - 1].ServiceFeePaid) :
                                                                    outputGrid[outputGridRow].BeginningServiceFee;
                double periodicInterestRate = 0;
                DateTime startDate = DateTime.MinValue;

                int row = outputGridRow - 1;
                //This loop determines the row which is neither the NSF fee event nor Late fee event.
                while (row >= 0)
                {
                    if (outputGrid[row].Flags != 11 && outputGrid[row].Flags != 12)
                    {
                        break;
                    }
                    row--;
                }

                //Checks if the row which is neither NSF fee nor late fee event, doesn't exists.
                if (row == -1)
                {
                    startDate = scheduleInputs.InputRecords[scheduleInputGridRow - 1].EffectiveDate == DateTime.MinValue ?
                                        scheduleInputs.InputRecords[scheduleInputGridRow - 1].DateIn : scheduleInputs.InputRecords[scheduleInputGridRow - 1].EffectiveDate;
                }
                else
                {
                    startDate = outputGrid[row].PaymentDate;
                }
                DateTime endDate = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].DateIn;

                //This condition determines whether there is an interest rate in additional payment grid, for that row. If it is provided, that value will be used, otherwise
                //default value will be used for periodic interest calculations.
                if (string.IsNullOrEmpty(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate))
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AddDiscount() method. Calling method : PeriodicInterestRate.GetPeriodicInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, Convert.ToDouble(scheduleInputs.InterestRate));");
                    periodicInterestRate = PeriodicInterestRate.GetPeriodicInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, Convert.ToDouble(scheduleInputs.InterestRate));
                }
                else
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AddDiscount() method. Calling method : PeriodicInterestRate.GetPeriodicInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate));");
                    periodicInterestRate = PeriodicInterestRate.GetPeriodicInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate));
                }

                //Calculates the interest accrued for that period.
                double interestAccrued = periodicInterestRate * remainingPrincipal;
                interestAccrued = Math.Round(interestAccrued, 2, MidpointRounding.AwayFromZero);

                //It determines the total interest carry over to be paid.
                double interestCarryOver = interestAccrued + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].InterestCarryOver);

                double serviceFeeInterestAccrued = 0, serviceFeeInterestCarryOver = 0;
                //Checks whether service fee interest is to be taken or not.
                if (scheduleInputs.ApplyServiceFeeInterest == 1 || scheduleInputs.ApplyServiceFeeInterest == 3)
                {
                    //Calculate the accrued service fee interest value.
                    serviceFeeInterestAccrued = periodicInterestRate * remainingPrincipalServiceFee;
                    serviceFeeInterestAccrued = Math.Round(serviceFeeInterestAccrued, 2, MidpointRounding.AwayFromZero);

                    //It determines the total interest carry over to be paid.
                    serviceFeeInterestCarryOver = serviceFeeInterestAccrued + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].AccruedServiceFeeInterestCarryOver);
                }

                //Checks whether the pervious event was a skipped payment and is not the first row of output grid.
                if (outputGridRow != 0 && outputGrid[outputGridRow - 1].Flags == 4)
                {
                    interestCarryOver += outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].InterestPayment;
                    serviceFeeInterestCarryOver += outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].ServiceFeeInterest;
                }

                //This variable calculates the daily interest rate for the period.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AddDiscount() method. Calling method : DailyInterestRate.CalculateDailyInterestRate(scheduleInputs.DaysInYearBankMethod, " +
                    "scheduleInputs.DaysInMonth, startDate, endDate, periodicInterestRate);");
                double dailyInterestRate = DailyInterestRate.CalculateDailyInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate,
                                                            periodicInterestRate);

                //This variable calculates the daily interest amount for the period.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AddDiscount() method. Calling method : DailyInterestAmount.CalculateDailyInterestAmount(scheduleInputs.DaysInYearBankMethod, " +
                    "scheduleInputs.DaysInMonth, startDate, endDate, periodicInterestRate * remainingPrincipal);");
                double dailyInterestAmount = DailyInterestAmount.CalculateDailyInterestAmount(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate,
                                                            endDate, periodicInterestRate * remainingPrincipal);
                dailyInterestAmount = Math.Round(dailyInterestAmount, 2, MidpointRounding.AwayFromZero);

                //It adds a new row on the selected indes i.e. "outputGridRow" in the output grid.
                outputGrid.Insert(outputGridRow,
                        new OutputGrid
                        {
                            PaymentDate = endDate,
                            BeginningPrincipal = remainingPrincipal,
                            BeginningServiceFee = remainingPrincipalServiceFee,
                            PeriodicInterestRate = periodicInterestRate,
                            DailyInterestRate = dailyInterestRate,
                            DailyInterestAmount = dailyInterestAmount,
                            PaymentID = Convert.ToInt32(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].PaymentID.ToString()),
                            Flags = Convert.ToInt32(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].Flags.ToString()),
                            DueDate = endDate,
                            InterestAccrued = interestAccrued,
                            InterestCarryOver = interestCarryOver,
                            InterestPayment = 0,
                            PrincipalPayment = 0,
                            TotalPayment = 0,
                            AccruedServiceFeeInterest = serviceFeeInterestAccrued,
                            AccruedServiceFeeInterestCarryOver = serviceFeeInterestCarryOver,
                            ServiceFee = 0,
                            ServiceFeeInterest = 0,
                            ServiceFeeTotal = 0,
                            OriginationFee = 0,
                            MaintenanceFee = (isExtendingSchedule && scheduleInputGridRow > 0) ? scheduleInputs.MaintenanceFee : 0,
                            ManagementFee = isExtendingSchedule ? scheduleInputs.ManagementFee : 0,
                            SameDayFee = 0,
                            NSFFee = 0,
                            LateFee = 0,
                            //Paid amount columns
                            PrincipalPaid = 0,
                            InterestPaid = 0,
                            ServiceFeePaid = 0,
                            ServiceFeeInterestPaid = 0,
                            OriginationFeePaid = 0,
                            MaintenanceFeePaid = 0,
                            ManagementFeePaid = 0,
                            SameDayFeePaid = 0,
                            NSFFeePaid = 0,
                            LateFeePaid = 0,
                            TotalPaid = 0,
                            //Cumulative Amount paid column
                            CumulativeInterest = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeInterest,
                            CumulativePrincipal = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePrincipal,
                            CumulativePayment = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePayment,
                            CumulativeServiceFee = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFee,
                            CumulativeServiceFeeInterest = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeInterest,
                            CumulativeServiceFeeTotal = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeTotal,
                            CumulativeOriginationFee = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeOriginationFee,
                            CumulativeMaintenanceFee = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeMaintenanceFee,
                            CumulativeManagementFee = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeManagementFee,
                            CumulativeSameDayFee = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeSameDayFee,
                            CumulativeNSFFee = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeNSFFee,
                            CumulativeLateFee = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeLateFee,
                            CumulativeTotalFees = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeTotalFees,
                            //Past due amount columns
                            PrincipalPastDue = 0,
                            InterestPastDue = 0,
                            ServiceFeePastDue = 0,
                            ServiceFeeInterestPastDue = 0,
                            OriginationFeePastDue = 0,
                            MaintenanceFeePastDue = (isExtendingSchedule && scheduleInputGridRow > 0) ? scheduleInputs.MaintenanceFee : 0,
                            ManagementFeePastDue = isExtendingSchedule ? scheduleInputs.ManagementFee : 0,
                            SameDayFeePastDue = 0,
                            NSFFeePastDue = 0,
                            LateFeePastDue = 0,
                            TotalPastDue = ((isExtendingSchedule && scheduleInputGridRow > 0) ? scheduleInputs.MaintenanceFee : 0) +
                                                        (isExtendingSchedule ? scheduleInputs.ManagementFee : 0),
                            //Cumulative past due amount columns
                            CumulativePrincipalPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePrincipalPastDue,
                            CumulativeInterestPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeInterestPastDue,
                            CumulativeServiceFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeePastDue,
                            CumulativeServiceFeeInterestPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeInterestPastDue,
                            CumulativeOriginationFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeOriginationFeePastDue,
                            CumulativeMaintenanceFeePastDue = ((isExtendingSchedule && scheduleInputGridRow > 0) ? scheduleInputs.MaintenanceFee : 0) +
                                                                outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeMaintenanceFeePastDue,
                            CumulativeManagementFeePastDue = (isExtendingSchedule ? scheduleInputs.ManagementFee : 0) + outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeManagementFeePastDue,
                            CumulativeSameDayFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeSameDayFeePastDue,
                            CumulativeNSFFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeNSFFeePastDue,
                            CumulativeLateFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeLateFeePastDue,
                            CumulativeTotalPastDue = ((isExtendingSchedule && scheduleInputGridRow > 0) ? scheduleInputs.MaintenanceFee : 0) +
                                                        (isExtendingSchedule ? scheduleInputs.ManagementFee : 0) +
                                                        outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeTotalPastDue,
                            BucketStatus = isExtendingSchedule ? "OutStanding" : ""
                        });

                double principalDiscount = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].PrincipalDiscount;
                double interestDiscount = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestDiscount;
                double serviceFeeDiscount = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].ServiceFeeDiscount;
                double serviceFeeInterestDiscount = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].ServiceFeeInterestDiscount;
                double originationFeeDiscount = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].OriginationFeeDiscount;
                double maintenanceFeeDiscount = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].MaintenanceFeeDiscount;
                double managementFeeDiscount = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].ManagementFeeDiscount;
                double sameDayFeeDiscount = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].SameDayFeeDiscount;
                double nsfFeeDiscount = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].NSFFeeDiscount;
                double lateFeeDiscount = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].LateFeeDiscount;

                int lastRowIndex = outputGrid.Count - 1;
                if (isExtendingSchedule)
                {
                    for (int i = 0; i < outputGrid.Count; i++)
                    {
                        if (scheduleInputs.InputRecords[scheduleInputs.InputRecords.Count - 1].DateIn < outputGrid[i].DueDate)
                        {
                            lastRowIndex = i - 1;
                            break;
                        }
                    }
                }

                #region Allocate Discounts to past due amounts

                for (int i = 0; i < outputGridRow; i++)
                {
                    //Applying discount amount in principal past due amounts.
                    if (principalDiscount >= outputGrid[i].PrincipalPastDue)
                    {
                        outputGrid[i].TotalPastDue = Math.Round(outputGrid[i].TotalPastDue - outputGrid[i].PrincipalPastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativePrincipalPastDue = Math.Round(outputGrid[i].CumulativePrincipalPastDue - outputGrid[i].PrincipalPastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeTotalPastDue = Math.Round(outputGrid[i].CumulativeTotalPastDue - outputGrid[i].PrincipalPastDue, 2, MidpointRounding.AwayFromZero);
                        principalDiscount = Math.Round(principalDiscount - outputGrid[i].PrincipalPastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[outputGridRow].PrincipalPaid += outputGrid[i].PrincipalPastDue;
                        if ((lastRowIndex != i) && isExtendingSchedule)
                        {
                            outputGrid[lastRowIndex].PrincipalPastDue -= outputGrid[i].PrincipalPastDue;
                        }
                        outputGrid[i].PrincipalPastDue = 0;
                    }
                    else
                    {
                        outputGrid[i].PrincipalPastDue = Math.Round(outputGrid[i].PrincipalPastDue - principalDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].TotalPastDue = Math.Round(outputGrid[i].TotalPastDue - principalDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativePrincipalPastDue = Math.Round(outputGrid[i].CumulativePrincipalPastDue - principalDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeTotalPastDue = Math.Round(outputGrid[i].CumulativeTotalPastDue - principalDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[outputGridRow].PrincipalPaid += principalDiscount;
                        if ((lastRowIndex != i) && isExtendingSchedule)
                        {
                            outputGrid[lastRowIndex].PrincipalPastDue -= principalDiscount;
                        }
                        principalDiscount = 0;
                    }

                    //Applying discount amount in interest past due amounts.
                    if (interestDiscount >= outputGrid[i].InterestPastDue)
                    {
                        outputGrid[i].TotalPastDue = Math.Round(outputGrid[i].TotalPastDue - outputGrid[i].InterestPastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeInterestPastDue = Math.Round(outputGrid[i].CumulativeInterestPastDue - outputGrid[i].InterestPastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeTotalPastDue = Math.Round(outputGrid[i].CumulativeTotalPastDue - outputGrid[i].InterestPastDue, 2, MidpointRounding.AwayFromZero);
                        interestDiscount = Math.Round(interestDiscount - outputGrid[i].InterestPastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[outputGridRow].InterestPaid += outputGrid[i].InterestPastDue;
                        if ((lastRowIndex != i) && isExtendingSchedule)
                        {
                            outputGrid[lastRowIndex].InterestPastDue -= outputGrid[i].InterestPastDue;
                        }
                        outputGrid[i].InterestPastDue = 0;
                    }
                    else
                    {
                        outputGrid[i].InterestPastDue = Math.Round(outputGrid[i].InterestPastDue - interestDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].TotalPastDue = Math.Round(outputGrid[i].TotalPastDue - interestDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeInterestPastDue = Math.Round(outputGrid[i].CumulativeInterestPastDue - interestDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeTotalPastDue = Math.Round(outputGrid[i].CumulativeTotalPastDue - interestDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[outputGridRow].InterestPaid += interestDiscount;
                        if ((lastRowIndex != i) && isExtendingSchedule)
                        {
                            outputGrid[lastRowIndex].InterestPastDue -= interestDiscount;
                        }
                        interestDiscount = 0;
                    }

                    //Applying discount amount in Service Fee past due amounts.
                    if (serviceFeeDiscount >= outputGrid[i].ServiceFeePastDue)
                    {
                        outputGrid[i].TotalPastDue = Math.Round(outputGrid[i].TotalPastDue - outputGrid[i].ServiceFeePastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeServiceFeePastDue = Math.Round(outputGrid[i].CumulativeServiceFeePastDue - outputGrid[i].ServiceFeePastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeTotalPastDue = Math.Round(outputGrid[i].CumulativeTotalPastDue - outputGrid[i].ServiceFeePastDue, 2, MidpointRounding.AwayFromZero);
                        serviceFeeDiscount = Math.Round(serviceFeeDiscount - outputGrid[i].ServiceFeePastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[outputGridRow].ServiceFeePaid += outputGrid[i].ServiceFeePastDue;
                        if ((lastRowIndex != i) && isExtendingSchedule)
                        {
                            outputGrid[lastRowIndex].ServiceFeePastDue -= outputGrid[i].ServiceFeePastDue;
                        }
                        outputGrid[i].ServiceFeePastDue = 0;
                    }
                    else
                    {
                        outputGrid[i].ServiceFeePastDue = Math.Round(outputGrid[i].ServiceFeePastDue - serviceFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].TotalPastDue = Math.Round(outputGrid[i].TotalPastDue - serviceFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeServiceFeePastDue = Math.Round(outputGrid[i].CumulativeServiceFeePastDue - serviceFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeTotalPastDue = Math.Round(outputGrid[i].CumulativeTotalPastDue - serviceFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[outputGridRow].ServiceFeePaid += serviceFeeDiscount;
                        if ((lastRowIndex != i) && isExtendingSchedule)
                        {
                            outputGrid[lastRowIndex].ServiceFeePastDue -= serviceFeeDiscount;
                        }
                        serviceFeeDiscount = 0;
                    }

                    //Applying discount amount in Service Fee Interest past due amounts.
                    if (serviceFeeInterestDiscount >= outputGrid[i].ServiceFeeInterestPastDue)
                    {
                        outputGrid[i].TotalPastDue = Math.Round(outputGrid[i].TotalPastDue - outputGrid[i].ServiceFeeInterestPastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeServiceFeeInterestPastDue = Math.Round(outputGrid[i].CumulativeServiceFeeInterestPastDue - outputGrid[i].ServiceFeeInterestPastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeTotalPastDue = Math.Round(outputGrid[i].CumulativeTotalPastDue - outputGrid[i].ServiceFeeInterestPastDue, 2, MidpointRounding.AwayFromZero);
                        serviceFeeInterestDiscount = Math.Round(serviceFeeInterestDiscount - outputGrid[i].ServiceFeeInterestPastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[outputGridRow].ServiceFeeInterestPaid += outputGrid[i].ServiceFeeInterestPastDue;
                        if ((lastRowIndex != i) && isExtendingSchedule)
                        {
                            outputGrid[lastRowIndex].ServiceFeeInterestPastDue -= outputGrid[i].ServiceFeeInterestPastDue;
                        }
                        outputGrid[i].ServiceFeeInterestPastDue = 0;
                    }
                    else
                    {
                        outputGrid[i].ServiceFeeInterestPastDue = Math.Round(outputGrid[i].ServiceFeeInterestPastDue - serviceFeeInterestDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].TotalPastDue = Math.Round(outputGrid[i].TotalPastDue - serviceFeeInterestDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeServiceFeeInterestPastDue = Math.Round(outputGrid[i].CumulativeServiceFeeInterestPastDue - serviceFeeInterestDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeTotalPastDue = Math.Round(outputGrid[i].CumulativeTotalPastDue - serviceFeeInterestDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[outputGridRow].ServiceFeeInterestPaid += serviceFeeInterestDiscount;
                        if ((lastRowIndex != i) && isExtendingSchedule)
                        {
                            outputGrid[lastRowIndex].ServiceFeeInterestPastDue -= serviceFeeInterestDiscount;
                        }
                        serviceFeeInterestDiscount = 0;
                    }

                    //Applying discount amount in Origination Fee past due amounts.
                    if (originationFeeDiscount >= outputGrid[i].OriginationFeePastDue)
                    {
                        outputGrid[i].TotalPastDue = Math.Round(outputGrid[i].TotalPastDue - outputGrid[i].OriginationFeePastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeOriginationFeePastDue = Math.Round(outputGrid[i].CumulativeOriginationFeePastDue - outputGrid[i].OriginationFeePastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeTotalPastDue = Math.Round(outputGrid[i].CumulativeTotalPastDue - outputGrid[i].OriginationFeePastDue, 2, MidpointRounding.AwayFromZero);
                        originationFeeDiscount = Math.Round(originationFeeDiscount - outputGrid[i].OriginationFeePastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[outputGridRow].OriginationFeePaid += outputGrid[i].OriginationFeePastDue;
                        outputGrid[i].OriginationFeePastDue = 0;
                    }
                    else
                    {
                        outputGrid[i].OriginationFeePastDue = Math.Round(outputGrid[i].OriginationFeePastDue - originationFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].TotalPastDue = Math.Round(outputGrid[i].TotalPastDue - originationFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeOriginationFeePastDue = Math.Round(outputGrid[i].CumulativeOriginationFeePastDue - originationFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeTotalPastDue = Math.Round(outputGrid[i].CumulativeTotalPastDue - originationFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[outputGridRow].OriginationFeePaid += originationFeeDiscount;
                        originationFeeDiscount = 0;
                    }

                    //Applying discount amount in maintenance Fee past due amounts.
                    if (maintenanceFeeDiscount >= outputGrid[i].MaintenanceFeePastDue)
                    {
                        outputGrid[i].TotalPastDue = Math.Round(outputGrid[i].TotalPastDue - outputGrid[i].MaintenanceFeePastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeMaintenanceFeePastDue = Math.Round(outputGrid[i].CumulativeMaintenanceFeePastDue - outputGrid[i].MaintenanceFeePastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeTotalPastDue = Math.Round(outputGrid[i].CumulativeTotalPastDue - outputGrid[i].MaintenanceFeePastDue, 2, MidpointRounding.AwayFromZero);
                        maintenanceFeeDiscount = Math.Round(maintenanceFeeDiscount - outputGrid[i].MaintenanceFeePastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[outputGridRow].MaintenanceFeePaid += outputGrid[i].MaintenanceFeePastDue;
                        outputGrid[i].MaintenanceFeePastDue = 0;
                    }
                    else
                    {
                        outputGrid[i].MaintenanceFeePastDue = Math.Round(outputGrid[i].MaintenanceFeePastDue - maintenanceFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].TotalPastDue = Math.Round(outputGrid[i].TotalPastDue - maintenanceFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeMaintenanceFeePastDue = Math.Round(outputGrid[i].CumulativeMaintenanceFeePastDue - maintenanceFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeTotalPastDue = Math.Round(outputGrid[i].CumulativeTotalPastDue - maintenanceFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[outputGridRow].MaintenanceFeePaid += maintenanceFeeDiscount;
                        maintenanceFeeDiscount = 0;
                    }

                    //Applying discount amount in management Fee past due amounts.
                    if (managementFeeDiscount >= outputGrid[i].ManagementFeePastDue)
                    {
                        outputGrid[i].TotalPastDue = Math.Round(outputGrid[i].TotalPastDue - outputGrid[i].ManagementFeePastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeManagementFeePastDue = Math.Round(outputGrid[i].CumulativeManagementFeePastDue - outputGrid[i].ManagementFeePastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeTotalPastDue = Math.Round(outputGrid[i].CumulativeTotalPastDue - outputGrid[i].ManagementFeePastDue, 2, MidpointRounding.AwayFromZero);
                        managementFeeDiscount = Math.Round(managementFeeDiscount - outputGrid[i].ManagementFeePastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[outputGridRow].ManagementFeePaid += outputGrid[i].ManagementFeePastDue;
                        outputGrid[i].ManagementFeePastDue = 0;
                    }
                    else
                    {
                        outputGrid[i].ManagementFeePastDue = Math.Round(outputGrid[i].ManagementFeePastDue - managementFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].TotalPastDue = Math.Round(outputGrid[i].TotalPastDue - managementFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeManagementFeePastDue = Math.Round(outputGrid[i].CumulativeManagementFeePastDue - managementFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeTotalPastDue = Math.Round(outputGrid[i].CumulativeTotalPastDue - managementFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[outputGridRow].ManagementFeePaid += managementFeeDiscount;
                        managementFeeDiscount = 0;
                    }

                    //Applying discount amount in Same Day Fee past due amounts.
                    if (sameDayFeeDiscount >= outputGrid[i].SameDayFeePastDue)
                    {
                        outputGrid[i].TotalPastDue = Math.Round(outputGrid[i].TotalPastDue - outputGrid[i].SameDayFeePastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeSameDayFeePastDue = Math.Round(outputGrid[i].CumulativeSameDayFeePastDue - outputGrid[i].SameDayFeePastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeTotalPastDue = Math.Round(outputGrid[i].CumulativeTotalPastDue - outputGrid[i].SameDayFeePastDue, 2, MidpointRounding.AwayFromZero);
                        sameDayFeeDiscount = Math.Round(sameDayFeeDiscount - outputGrid[i].SameDayFeePastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[outputGridRow].SameDayFeePaid += outputGrid[i].SameDayFeePastDue;
                        outputGrid[i].SameDayFeePastDue = 0;
                    }
                    else
                    {
                        outputGrid[i].SameDayFeePastDue = Math.Round(outputGrid[i].SameDayFeePastDue - sameDayFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].TotalPastDue = Math.Round(outputGrid[i].TotalPastDue - sameDayFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeSameDayFeePastDue = Math.Round(outputGrid[i].CumulativeSameDayFeePastDue - sameDayFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeTotalPastDue = Math.Round(outputGrid[i].CumulativeTotalPastDue - sameDayFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[outputGridRow].SameDayFeePaid += sameDayFeeDiscount;
                        sameDayFeeDiscount = 0;
                    }

                    //Applying discount amount in NSF Fee past due amounts.
                    if (nsfFeeDiscount >= outputGrid[i].NSFFeePastDue)
                    {
                        outputGrid[i].TotalPastDue = Math.Round(outputGrid[i].TotalPastDue - outputGrid[i].NSFFeePastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeNSFFeePastDue = Math.Round(outputGrid[i].CumulativeNSFFeePastDue - outputGrid[i].NSFFeePastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeTotalPastDue = Math.Round(outputGrid[i].CumulativeTotalPastDue - outputGrid[i].NSFFeePastDue, 2, MidpointRounding.AwayFromZero);
                        nsfFeeDiscount = Math.Round(nsfFeeDiscount - outputGrid[i].NSFFeePastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[outputGridRow].NSFFeePaid += outputGrid[i].NSFFeePastDue;
                        outputGrid[i].NSFFeePastDue = 0;
                    }
                    else
                    {
                        outputGrid[i].NSFFeePastDue = Math.Round(outputGrid[i].NSFFeePastDue - nsfFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].TotalPastDue = Math.Round(outputGrid[i].TotalPastDue - nsfFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeNSFFeePastDue = Math.Round(outputGrid[i].CumulativeNSFFeePastDue - nsfFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeTotalPastDue = Math.Round(outputGrid[i].CumulativeTotalPastDue - nsfFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[outputGridRow].NSFFeePaid += nsfFeeDiscount;
                        nsfFeeDiscount = 0;
                    }

                    //Applying discount amount in Late Fee past due amounts.
                    if (lateFeeDiscount >= outputGrid[i].LateFeePastDue)
                    {
                        outputGrid[i].TotalPastDue = Math.Round(outputGrid[i].TotalPastDue - outputGrid[i].LateFeePastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeLateFeePastDue = Math.Round(outputGrid[i].CumulativeLateFeePastDue - outputGrid[i].LateFeePastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeTotalPastDue = Math.Round(outputGrid[i].CumulativeTotalPastDue - outputGrid[i].LateFeePastDue, 2, MidpointRounding.AwayFromZero);
                        lateFeeDiscount = Math.Round(lateFeeDiscount - outputGrid[i].LateFeePastDue, 2, MidpointRounding.AwayFromZero);
                        outputGrid[outputGridRow].LateFeePaid += outputGrid[i].LateFeePastDue;
                        outputGrid[i].LateFeePastDue = 0;
                    }
                    else
                    {
                        outputGrid[i].LateFeePastDue = Math.Round(outputGrid[i].LateFeePastDue - lateFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].TotalPastDue = Math.Round(outputGrid[i].TotalPastDue - lateFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeLateFeePastDue = Math.Round(outputGrid[i].CumulativeLateFeePastDue - lateFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[i].CumulativeTotalPastDue = Math.Round(outputGrid[i].CumulativeTotalPastDue - lateFeeDiscount, 2, MidpointRounding.AwayFromZero);
                        outputGrid[outputGridRow].LateFeePaid += lateFeeDiscount;
                        lateFeeDiscount = 0;
                    }

                    //Checks whether cumulative past due amount is 0.
                    if (outputGrid[i].CumulativeTotalPastDue == 0)
                    {
                        outputGrid[i].BucketStatus = string.IsNullOrEmpty(outputGrid[i].BucketStatus) ? "" : "Satisfied";
                    }

                    if (isExtendingSchedule)
                    {
                        outputGrid[lastRowIndex].TotalPastDue = outputGrid[lastRowIndex].PrincipalPastDue + outputGrid[lastRowIndex].InterestPastDue +
                                                    outputGrid[lastRowIndex].ServiceFeePastDue + outputGrid[lastRowIndex].ServiceFeeInterestPastDue +
                                                    outputGrid[lastRowIndex].OriginationFeePastDue + outputGrid[lastRowIndex].MaintenanceFeePastDue +
                                                    outputGrid[lastRowIndex].ManagementFeePastDue + outputGrid[lastRowIndex].SameDayFeePastDue +
                                                    outputGrid[lastRowIndex].NSFFeePastDue + outputGrid[lastRowIndex].LateFeePastDue;
                    }

                    //This loop recalculates the cumulative past due amounts.
                    for (int j = i + 1; j <= ((outputGridRow + 1) < outputGrid.Count ? (outputGridRow + 1) : (outputGrid.Count - 1)); j++)
                    {
                        outputGrid[j].CumulativePrincipalPastDue = outputGrid[j].PrincipalPastDue + outputGrid[i].CumulativePrincipalPastDue;
                        outputGrid[j].CumulativeInterestPastDue = outputGrid[j].InterestPastDue + outputGrid[i].CumulativeInterestPastDue;
                        outputGrid[j].CumulativeServiceFeePastDue = outputGrid[j].ServiceFeePastDue + outputGrid[i].CumulativeServiceFeePastDue;
                        outputGrid[j].CumulativeServiceFeeInterestPastDue = outputGrid[j].ServiceFeeInterestPastDue + outputGrid[i].CumulativeServiceFeeInterestPastDue;
                        outputGrid[j].CumulativeOriginationFeePastDue = outputGrid[j].OriginationFeePastDue + outputGrid[i].CumulativeOriginationFeePastDue;
                        outputGrid[j].CumulativeMaintenanceFeePastDue = outputGrid[j].MaintenanceFeePastDue + outputGrid[i].CumulativeMaintenanceFeePastDue;
                        outputGrid[j].CumulativeManagementFeePastDue = outputGrid[j].ManagementFeePastDue + outputGrid[i].CumulativeManagementFeePastDue;
                        outputGrid[j].CumulativeSameDayFeePastDue = outputGrid[j].SameDayFeePastDue + outputGrid[i].CumulativeSameDayFeePastDue;
                        outputGrid[j].CumulativeNSFFeePastDue = outputGrid[j].NSFFeePastDue + outputGrid[i].CumulativeNSFFeePastDue;
                        outputGrid[j].CumulativeLateFeePastDue = outputGrid[j].LateFeePastDue + outputGrid[i].CumulativeLateFeePastDue;
                        outputGrid[j].CumulativeTotalPastDue = outputGrid[j].TotalPastDue + outputGrid[i].CumulativeTotalPastDue;
                    }
                }

                #endregion

                int firstScheduleIndex = 0;
                //Determine the first repayment schedule.
                for (int rowIndex = outputGrid.Count - 1; rowIndex >= 0; rowIndex--)
                {
                    if (scheduleInputs.InputRecords[1].DateIn == outputGrid[rowIndex].DueDate)
                    {
                        firstScheduleIndex = rowIndex;
                        break;
                    }
                }

                //Calculate the origination and same day fee to be paid after deducting the paid amount from payable.
                for (int i = 0; i <= outputGridRow; i++)
                {
                    originationFee = (originationFee - outputGrid[i].OriginationFeePaid) < 0 ? 0 : (originationFee - outputGrid[i].OriginationFeePaid);
                    sameDayFee = (sameDayFee - outputGrid[i].SameDayFeePaid) < 0 ? 0 : (sameDayFee - outputGrid[i].SameDayFeePaid);
                }

                //Give discount on Origination fee.
                //Check whether discount date is larger than first scheduled date. 
                if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].DateIn <= outputGrid[firstScheduleIndex].PaymentDate)
                {
                    if (outputGrid[firstScheduleIndex].OriginationFee <= originationFeeDiscount)
                    {
                        outputGrid[outputGridRow].OriginationFeePaid += outputGrid[firstScheduleIndex].OriginationFee;
                        originationFeeDiscount -= outputGrid[firstScheduleIndex].OriginationFee;
                        outputGrid[outputGridRow].OriginationFee = outputGrid[firstScheduleIndex].OriginationFee;
                        outputGrid[firstScheduleIndex].OriginationFee = 0;
                    }
                    else
                    {
                        outputGrid[outputGridRow].OriginationFeePaid += originationFeeDiscount;
                        outputGrid[firstScheduleIndex].OriginationFee -= originationFeeDiscount;
                        outputGrid[outputGridRow].OriginationFee = originationFeeDiscount;
                        originationFeeDiscount = 0;
                    }
                }
                //Check whether there is an amount remains to pay due to skipping the first repayment schedule.
                else if (originationFee > 0)
                {
                    if (originationFee <= originationFeeDiscount)
                    {
                        outputGrid[outputGridRow].OriginationFee = originationFee;
                        outputGrid[outputGridRow].OriginationFeePaid += originationFee;
                        originationFeeDiscount -= originationFee;
                    }
                    else
                    {
                        outputGrid[outputGridRow].OriginationFee = originationFeeDiscount;
                        outputGrid[outputGridRow].OriginationFeePaid += originationFeeDiscount;
                        originationFeeDiscount = 0;
                    }
                }

                //Give discount on same day fee.
                //Check whether same day fee date is larger than first scheduled date.
                if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].DateIn <= outputGrid[firstScheduleIndex].PaymentDate)
                {
                    if (outputGrid[firstScheduleIndex].SameDayFee <= sameDayFeeDiscount)
                    {
                        outputGrid[outputGridRow].SameDayFeePaid += outputGrid[firstScheduleIndex].SameDayFee;
                        sameDayFeeDiscount -= outputGrid[firstScheduleIndex].SameDayFee;
                        outputGrid[outputGridRow].SameDayFee = outputGrid[firstScheduleIndex].SameDayFee;
                        outputGrid[firstScheduleIndex].SameDayFee = 0;
                    }
                    else
                    {
                        outputGrid[outputGridRow].SameDayFeePaid += sameDayFeeDiscount;
                        outputGrid[firstScheduleIndex].SameDayFee -= sameDayFeeDiscount;
                        outputGrid[outputGridRow].SameDayFee = sameDayFeeDiscount;
                        sameDayFeeDiscount = 0;
                    }
                }
                //Check whether there is an amount remains to pay due to skipping the first repayment schedule.
                else if (sameDayFee > 0)
                {
                    if (sameDayFee <= sameDayFeeDiscount)
                    {
                        outputGrid[outputGridRow].SameDayFee = sameDayFee;
                        outputGrid[outputGridRow].SameDayFeePaid += sameDayFee;
                        sameDayFeeDiscount -= sameDayFee;
                    }
                    else
                    {
                        outputGrid[outputGridRow].SameDayFee = sameDayFeeDiscount;
                        outputGrid[outputGridRow].SameDayFeePaid += sameDayFeeDiscount;
                        sameDayFeeDiscount = 0;
                    }
                }

                //Determine the remaining principal amount to be paid after processing the current row.
                remainingPrincipal -= outputGrid[outputGridRow].PrincipalPaid;

                //Checks whether there is any remaining principal discount.
                if (principalDiscount > 0)
                {
                    //Pay the remaining principal amount till the discount amount is set to 0.
                    outputGrid[outputGridRow].PrincipalPaid = outputGrid[outputGridRow].PrincipalPaid + (principalDiscount > remainingPrincipal ? remainingPrincipal : principalDiscount);
                    //Then calculate the remining principal amount.
                    remainingPrincipal = (remainingPrincipal - principalDiscount) <= 0 ? 0 : (remainingPrincipal - principalDiscount);
                }

                //Determine the remaining principalservice fee amount to be paid after processing the current row.
                remainingPrincipalServiceFee -= outputGrid[outputGridRow].ServiceFeePaid;

                //Checks whether there is any remaining principal service fee discount.
                if (serviceFeeDiscount > 0)
                {
                    //Pay the remaining service fee amount till the discount amount is set to 0.
                    outputGrid[outputGridRow].ServiceFeePaid = outputGrid[outputGridRow].ServiceFeePaid + (serviceFeeDiscount > remainingPrincipalServiceFee ? remainingPrincipalServiceFee : serviceFeeDiscount);
                    //Then calculate the remining principal service fee amount.
                    remainingPrincipalServiceFee = (remainingPrincipalServiceFee - serviceFeeDiscount) <= 0 ? 0 : (remainingPrincipalServiceFee - serviceFeeDiscount);
                }

                //Checks whether there is any remaining interest discount.
                if (interestDiscount > 0)
                {
                    //Pay the remaining interest amount till the discount amount is set to 0.
                    outputGrid[outputGridRow].InterestPaid = outputGrid[outputGridRow].InterestPaid +
                            (interestDiscount > outputGrid[outputGridRow].InterestCarryOver ? outputGrid[outputGridRow].InterestCarryOver : interestDiscount);
                    //Recalculate the interest payment and interest carry over for the current row.
                    outputGrid[outputGridRow].InterestPayment = (interestDiscount > outputGrid[outputGridRow].InterestCarryOver ? outputGrid[outputGridRow].InterestCarryOver : interestDiscount);
                    outputGrid[outputGridRow].InterestCarryOver -= (interestDiscount > outputGrid[outputGridRow].InterestCarryOver ? outputGrid[outputGridRow].InterestCarryOver : interestDiscount);
                }

                //Checks whether there is any remaining service fee interest discount.
                if (serviceFeeInterestDiscount > 0)
                {
                    //Pay the remaining service fee interest amount till the discount amount is set to 0.
                    outputGrid[outputGridRow].ServiceFeeInterestPaid = outputGrid[outputGridRow].ServiceFeeInterestPaid +
                            (serviceFeeInterestDiscount > outputGrid[outputGridRow].AccruedServiceFeeInterestCarryOver ? outputGrid[outputGridRow].AccruedServiceFeeInterestCarryOver :
                                serviceFeeInterestDiscount);
                    //Recalculate the service fee interest payment and interest carry over for the current row.
                    outputGrid[outputGridRow].ServiceFeeInterest = (serviceFeeInterestDiscount > outputGrid[outputGridRow].AccruedServiceFeeInterestCarryOver ?
                                        outputGrid[outputGridRow].AccruedServiceFeeInterestCarryOver : serviceFeeInterestDiscount);
                    outputGrid[outputGridRow].AccruedServiceFeeInterestCarryOver -= (serviceFeeInterestDiscount > outputGrid[outputGridRow].AccruedServiceFeeInterestCarryOver ?
                                        outputGrid[outputGridRow].AccruedServiceFeeInterestCarryOver : serviceFeeInterestDiscount);

                }

                //Checks whether there is any remaining Management fee discount.
                if (managementFeeDiscount > 0 && isExtendingSchedule)
                {
                    //Pay the remaining management fee past due amount till the discount amount is set to 0.
                    outputGrid[outputGridRow].ManagementFeePaid = outputGrid[outputGridRow].ManagementFeePaid +
                            (managementFeeDiscount > outputGrid[outputGridRow].ManagementFeePastDue ? outputGrid[outputGridRow].ManagementFeePastDue : managementFeeDiscount);
                    //Recalculate the management fee past due payment and total past due amount for the current row.
                    outputGrid[outputGridRow].TotalPastDue -= (managementFeeDiscount > outputGrid[outputGridRow].ManagementFeePastDue ? outputGrid[outputGridRow].ManagementFeePastDue : managementFeeDiscount);
                    outputGrid[outputGridRow].TotalPastDue = Math.Round(outputGrid[outputGridRow].TotalPastDue, 0, MidpointRounding.AwayFromZero);
                    outputGrid[outputGridRow].CumulativeTotalPastDue -= (managementFeeDiscount > outputGrid[outputGridRow].ManagementFeePastDue ? outputGrid[outputGridRow].ManagementFeePastDue : managementFeeDiscount);
                    outputGrid[outputGridRow].CumulativeTotalPastDue = Math.Round(outputGrid[outputGridRow].CumulativeTotalPastDue, 0, MidpointRounding.AwayFromZero);
                    outputGrid[outputGridRow].CumulativeManagementFeePastDue -= (managementFeeDiscount > outputGrid[outputGridRow].ManagementFeePastDue ? outputGrid[outputGridRow].ManagementFeePastDue : managementFeeDiscount);
                    outputGrid[outputGridRow].ManagementFeePastDue -= (managementFeeDiscount > outputGrid[outputGridRow].ManagementFeePastDue ? outputGrid[outputGridRow].ManagementFeePastDue : managementFeeDiscount);
                }

                //Checks whether there is any remaining maintenance fee discount.
                if (maintenanceFeeDiscount > 0 && isExtendingSchedule)
                {
                    //Pay the remaining management fee past due amount till the discount amount is set to 0.
                    outputGrid[outputGridRow].MaintenanceFeePaid = outputGrid[outputGridRow].MaintenanceFeePaid +
                            (maintenanceFeeDiscount > outputGrid[outputGridRow].MaintenanceFeePastDue ? outputGrid[outputGridRow].MaintenanceFeePastDue : maintenanceFeeDiscount);
                    //Recalculate the management fee past due payment and total past due amount for the current row.
                    outputGrid[outputGridRow].TotalPastDue -= (maintenanceFeeDiscount > outputGrid[outputGridRow].MaintenanceFeePastDue ? outputGrid[outputGridRow].MaintenanceFeePastDue : maintenanceFeeDiscount);
                    outputGrid[outputGridRow].TotalPastDue = Math.Round(outputGrid[outputGridRow].TotalPastDue, 0, MidpointRounding.AwayFromZero);
                    outputGrid[outputGridRow].CumulativeTotalPastDue -= (maintenanceFeeDiscount > outputGrid[outputGridRow].MaintenanceFeePastDue ? outputGrid[outputGridRow].MaintenanceFeePastDue : maintenanceFeeDiscount);
                    outputGrid[outputGridRow].CumulativeTotalPastDue = Math.Round(outputGrid[outputGridRow].CumulativeTotalPastDue, 0, MidpointRounding.AwayFromZero);
                    outputGrid[outputGridRow].CumulativeMaintenanceFeePastDue -= (maintenanceFeeDiscount > outputGrid[outputGridRow].MaintenanceFeePastDue ? outputGrid[outputGridRow].MaintenanceFeePastDue : maintenanceFeeDiscount);
                    outputGrid[outputGridRow].MaintenanceFeePastDue -= (maintenanceFeeDiscount > outputGrid[outputGridRow].MaintenanceFeePastDue ? outputGrid[outputGridRow].MaintenanceFeePastDue : maintenanceFeeDiscount);
                }

                outputGrid[outputGridRow].TotalPaid = outputGrid[outputGridRow].PrincipalPaid + outputGrid[outputGridRow].InterestPaid +
                                                outputGrid[outputGridRow].ServiceFeePaid + outputGrid[outputGridRow].ServiceFeeInterestPaid +
                                                outputGrid[outputGridRow].OriginationFeePaid + outputGrid[outputGridRow].MaintenanceFeePaid +
                                                outputGrid[outputGridRow].ManagementFeePaid + outputGrid[outputGridRow].SameDayFeePaid +
                                                outputGrid[outputGridRow].NSFFeePaid + outputGrid[outputGridRow].LateFeePaid;

                //Calculate the cumulative paid amount columns.
                outputGrid[outputGridRow].CumulativePrincipal = outputGrid[outputGridRow].PrincipalPaid + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePrincipal);
                outputGrid[outputGridRow].CumulativeInterest = outputGrid[outputGridRow].InterestPaid + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeInterest);
                outputGrid[outputGridRow].CumulativeServiceFee = outputGrid[outputGridRow].ServiceFeePaid + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFee);
                outputGrid[outputGridRow].CumulativeServiceFeeInterest = outputGrid[outputGridRow].ServiceFeeInterestPaid + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeInterest);
                outputGrid[outputGridRow].CumulativeServiceFeeTotal = outputGrid[outputGridRow].ServiceFeePaid + outputGrid[outputGridRow].ServiceFeeInterestPaid +
                                                                            (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeTotal);
                outputGrid[outputGridRow].CumulativeOriginationFee = outputGrid[outputGridRow].OriginationFeePaid + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeOriginationFee);
                outputGrid[outputGridRow].CumulativeMaintenanceFee = outputGrid[outputGridRow].MaintenanceFeePaid + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeMaintenanceFee);
                outputGrid[outputGridRow].CumulativeManagementFee = outputGrid[outputGridRow].ManagementFeePaid + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeManagementFee);
                outputGrid[outputGridRow].CumulativeSameDayFee = outputGrid[outputGridRow].SameDayFeePaid + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeSameDayFee);
                outputGrid[outputGridRow].CumulativeNSFFee = outputGrid[outputGridRow].NSFFeePaid + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeNSFFee);
                outputGrid[outputGridRow].CumulativeLateFee = outputGrid[outputGridRow].LateFeePaid + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeLateFee);
                outputGrid[outputGridRow].CumulativeTotalFees = (outputGrid[outputGridRow].ServiceFeePaid + outputGrid[outputGridRow].ServiceFeeInterestPaid +
                                                outputGrid[outputGridRow].OriginationFeePaid + outputGrid[outputGridRow].MaintenanceFeePaid +
                                                outputGrid[outputGridRow].ManagementFeePaid + outputGrid[outputGridRow].SameDayFeePaid +
                                                outputGrid[outputGridRow].NSFFeePaid + outputGrid[outputGridRow].LateFeePaid) + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeTotalFees);
                outputGrid[outputGridRow].CumulativePayment = outputGrid[outputGridRow].TotalPaid + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePayment);

                //Recalculates the output grid values for the remaining scheduled dates if the current event is not the part of schedule extend functionality.
                if (!isExtendingSchedule)
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AddDiscount() method. Calling method : ReCalculateOutputGridValues(remainingPrincipal, remainingprincipalServiceFee, outputGrid, scheduleInputs, " +
                        "outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);");
                    ReCalculateOutputGridValues(remainingPrincipal, remainingPrincipalServiceFee, outputGrid, scheduleInputs, outputGridRow + 1, defaultTotalAmountToPay,
                                                        defaultTotalServiceFeePayable, originationFee, sameDayFee);
                }
                sbTracing.AppendLine("Exist:From class ActualPaymentAllocation and method name AddDiscount(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, " +
                    "int scheduleInputGridRow, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee)");
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }

        #endregion
    }
}
