using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoanCalculator.Services.Models;

namespace LoanCalculator.Services
{
    public class ActualPaymentAllocation
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
        /// <param name="outputSchedule"></param>
        public static void AllocationOfFunds(List<OutputGrid> outputGrid, LoanDetails scheduleInput, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable,
                                                double originationFee, double loanAmountCalc, double sameDayFee, OutputSchedule outputSchedule)
        {
            int additionalPaymentCurrentRow = 0;
            int outputGridRowNumber = 0;

            int managementFeeTableIndex = 0;
            int maintenanceFeeTableIndex = 0;
            int lastManagementFeeChargedIndex = -1;
            int lastMaintenanceFeeChargedIndex = -1;

            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name AllocationOfFunds(List<OutputGrid> outputGrid, LoanDetails scheduleInput, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double loanAmountCalc, double sameDayFee, OutputSchedule outputSchedule).");
                sbTracing.AppendLine("Parameter values are : defaultTotalAmountToPay = " + defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", originationFee = " + originationFee + ", loanAmountCalc = " + loanAmountCalc + ", sameDayFee = " + sameDayFee);

                #region Allocate Funds

                int inputGridRow;
                double managementFeePayable;
                double maintenanceFeePayable;
                for (inputGridRow = 1; inputGridRow < scheduleInput.InputRecords.Count && scheduleInput.InputRecords[inputGridRow].DateIn <= outputGrid[outputGrid.Count == 1 ? 0 : outputGrid.Count - 1].DueDate; inputGridRow++)
                {
                    //This loop adds all the additonal payment either before, or in between the scheduled payments
                    for (int j = additionalPaymentCurrentRow; j < scheduleInput.AdditionalPaymentRecords.Count; j++)
                    {
                        //This condition checks whether additonal payment date is less than scheduled payment date. If it is, then it first adds the additonal payment row.
                        //User Story:- Change logic for that when Addtional payemnt is occur on same date then  first Schedule payment and then addtional payment will process after schedule payment date.
                        if (scheduleInput.AdditionalPaymentRecords[j].DateIn < (scheduleInput.InputRecords[inputGridRow].EffectiveDate == DateTime.MinValue ? scheduleInput.InputRecords[inputGridRow].DateIn : scheduleInput.InputRecords[inputGridRow].EffectiveDate))
                        {
                            #region Calculate Management Fee and Maintenance Fee
                            sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, j, true, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);");
                            managementFeePayable = ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, j, true, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);
                            sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, j, true, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);");
                            maintenanceFeePayable = ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, j, true, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);

                            #endregion

                            //This condition checks whether it is a principal only additonal payment.
                            switch (scheduleInput.AdditionalPaymentRecords[j].Flags)
                            {
                                case (int)Constants.FlagValues.PrincipalOnly:
                                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : PayPrincipalOnlyAmount(outputGrid, scheduleInput, additionalPaymentCurrentRow, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, false, originationFee, sameDayFee, managementFeePayable, maintenanceFeePayable);");
                                    PayPrincipalOnlyAmount(outputGrid, scheduleInput, additionalPaymentCurrentRow, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, false, originationFee, sameDayFee, managementFeePayable, maintenanceFeePayable);
                                    break;

                                case (int)Constants.FlagValues.NSFFee:
                                case (int)Constants.FlagValues.LateFee:
                                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : AddNSFAndLateFee(outputGrid, scheduleInput, additionalPaymentCurrentRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);");
                                    AddNSFAndLateFee(outputGrid, scheduleInput, additionalPaymentCurrentRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);
                                    break;

                                case (int)Constants.FlagValues.AdditionalPayment:
                                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : PayAdditionalPayment(outputGrid, scheduleInput, additionalPaymentCurrentRow, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, false, originationFee, sameDayFee, managementFeePayable, maintenanceFeePayable);");
                                    PayAdditionalPayment(outputGrid, scheduleInput, additionalPaymentCurrentRow, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, false, originationFee, sameDayFee, managementFeePayable, maintenanceFeePayable);
                                    break;

                                case (int)Constants.FlagValues.Discount:
                                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : AddDiscount(outputGrid, scheduleInput, additionalPaymentCurrentRow, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, false, originationFee, sameDayFee);");
                                    AddDiscount(outputGrid, scheduleInput, additionalPaymentCurrentRow, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, false, originationFee, sameDayFee);
                                    break;

                                case (int)Constants.FlagValues.ManagementFee:
                                case (int)Constants.FlagValues.MaintenanceFee:
                                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFee(outputGrid, scheduleInput, additionalPaymentCurrentRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, false, originationFee, sameDayFee);");
                                    ManagementAndMaintenanceFee(outputGrid, scheduleInput, additionalPaymentCurrentRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, false, originationFee, sameDayFee);
                                    break;
                            }
                            additionalPaymentCurrentRow++;
                            outputGridRowNumber++;
                        }
                    }

                    #region Calculate Management Fee and Maintenance Fee

                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, inputGridRow, false, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);");
                    managementFeePayable = ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, inputGridRow, false, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, inputGridRow, false, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);");
                    maintenanceFeePayable = ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, inputGridRow, false, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);
                    outputGrid[outputGridRowNumber].ManagementFee += managementFeePayable;
                    outputGrid[outputGridRowNumber].MaintenanceFee += maintenanceFeePayable;
                    outputGrid[outputGridRowNumber].TotalPayment += managementFeePayable + maintenanceFeePayable;

                    #endregion

                    //This condition checks, the schedule payment is for paid, missed, adjustment.
                    if (scheduleInput.InputRecords[inputGridRow].Flags == (int)Constants.FlagValues.Payment)
                    {
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : AllocateFundToBuckets(outputGrid, scheduleInput, inputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, outputGridRowNumber, originationFee, sameDayFee, outputSchedule.LoanAmountCalc);");
                        AllocateFundToBuckets(outputGrid, scheduleInput, inputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, outputGridRowNumber, originationFee, sameDayFee, outputSchedule.LoanAmountCalc);
                    }
                    //This condition checks, the schedule payment is skipped payment.
                    else if (scheduleInput.InputRecords[inputGridRow].Flags == (int)Constants.FlagValues.SkipPayment)
                    {
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : SkippedPayment(outputGrid, scheduleInput, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);");
                        SkippedPayment(outputGrid, scheduleInput, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);
                    }
                    outputGridRowNumber++;
                }

                #endregion

                #region Delete unused row

                //This loop determine the index value from which index, the rows of output grid will be deleted.
                int startDeletionIndex = outputGrid.Count;
                int sameDateOfAdditionalPayment = 0;
                for (int i = 0; i <= outputGrid.Count - 1; i++)
                {
                    double managementFee = 0;
                    double maintenanceFee = 0;
                    for (int j = 0; j <= i; j++)
                    {
                        double managementFeePayableFee = outputSchedule.ManagementFeeAssessment.Where(o => o.AssessmentDate > (j == 0 ? scheduleInput.InputRecords[0].DateIn : outputGrid[j - 1].PaymentDate) &&
                                                          o.AssessmentDate <= outputGrid[j].PaymentDate).Sum(o => o.Fee);
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.PayableAmountAsPerMonthAndLoan(scheduleInput, outputGrid[j].PaymentDate, outputGrid, managementFeePayableFee, true, j);");
                        managementFee += ManagementAndMaintenanceFeeCalc.PayableAmountAsPerMonthAndLoan(scheduleInput, outputGrid[j].PaymentDate, outputGrid, managementFeePayableFee, true, j);

                        double maintenanceFeePayableFee = outputSchedule.MaintenanceFeeAssessment.Where(o => o.AssessmentDate > (j == 0 ? scheduleInput.InputRecords[0].DateIn : outputGrid[j - 1].PaymentDate) &&
                                                          o.AssessmentDate <= outputGrid[j].PaymentDate).Sum(o => o.Fee);
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.PayableAmountAsPerMonthAndLoan(scheduleInput, outputGrid[j].PaymentDate, outputGrid, maintenanceFeePayableFee, false, j);");
                        maintenanceFee += ManagementAndMaintenanceFeeCalc.PayableAmountAsPerMonthAndLoan(scheduleInput, outputGrid[j].PaymentDate, outputGrid, maintenanceFeePayableFee, false, j);
                    }

                    if (i != 0 && (outputGrid[i].PaymentDate == outputGrid[i - 1].PaymentDate))
                    {
                        sameDateOfAdditionalPayment++;
                    }
                    else
                    {
                        sameDateOfAdditionalPayment = 0;
                    }

                    //Calculate the management and maintenance fee to be paid when fee is added via additional payment.
                    if (scheduleInput.ManagementFee == 0 && scheduleInput.ManagementFeePercent == 0)
                    {
                        managementFee = scheduleInput.AdditionalPaymentRecords.Where(o => o.DateIn < outputGrid[i].PaymentDate && o.Flags == (int)Constants.FlagValues.ManagementFee).Sum(o => o.AdditionalPayment);
                    }
                    if (scheduleInput.MaintenanceFee == 0 && scheduleInput.MaintenanceFeePercent == 0)
                    {
                        maintenanceFee = scheduleInput.AdditionalPaymentRecords.Where(o => o.DateIn < outputGrid[i].PaymentDate && o.Flags == (int)Constants.FlagValues.MaintenanceFee).Sum(o => o.AdditionalPayment);
                    }

                    double nsfFee = scheduleInput.AdditionalPaymentRecords.Where(o => o.DateIn < outputGrid[i].PaymentDate && o.Flags == (int)Constants.FlagValues.NSFFee).Sum(o => o.AdditionalPayment);
                    double lateFee = scheduleInput.AdditionalPaymentRecords.Where(o => o.DateIn < outputGrid[i].PaymentDate && o.Flags == (int)Constants.FlagValues.LateFee).Sum(o => o.AdditionalPayment);

                    for (int j = 0; j < scheduleInput.AdditionalPaymentRecords.Count; j++)
                    {
                        if (scheduleInput.AdditionalPaymentRecords[j].DateIn == outputGrid[i].PaymentDate)
                        {
                            for (int k = 0; k <= sameDateOfAdditionalPayment && j < scheduleInput.AdditionalPaymentRecords.Count; k++)
                            {
                                if (scheduleInput.AdditionalPaymentRecords[j].Flags == (int)Constants.FlagValues.NSFFee && scheduleInput.AdditionalPaymentRecords[j].DateIn == outputGrid[i].PaymentDate)
                                {
                                    nsfFee += scheduleInput.AdditionalPaymentRecords[j].AdditionalPayment;
                                }
                                else if (scheduleInput.AdditionalPaymentRecords[j].Flags == (int)Constants.FlagValues.LateFee && scheduleInput.AdditionalPaymentRecords[j].DateIn == outputGrid[i].PaymentDate)
                                {
                                    lateFee += scheduleInput.AdditionalPaymentRecords[j].AdditionalPayment;
                                }
                                else if (scheduleInput.AdditionalPaymentRecords[j].Flags == (int)Constants.FlagValues.ManagementFee && scheduleInput.AdditionalPaymentRecords[j].DateIn == outputGrid[i].PaymentDate)
                                {
                                    managementFee += scheduleInput.AdditionalPaymentRecords[j].AdditionalPayment;
                                }
                                else if (scheduleInput.AdditionalPaymentRecords[j].Flags == (int)Constants.FlagValues.MaintenanceFee && scheduleInput.AdditionalPaymentRecords[j].DateIn == outputGrid[i].PaymentDate)
                                {
                                    maintenanceFee += scheduleInput.AdditionalPaymentRecords[j].AdditionalPayment;
                                }
                                j++;
                            }
                            break;
                        }
                    }

                    double totalDue = scheduleInput.EarnedManagementFee + scheduleInput.EarnedMaintenanceFee + scheduleInput.EarnedNSFFee + scheduleInput.EarnedLateFee +
                                            originationFee + sameDayFee + nsfFee + lateFee + outputGrid[0].BeginningPrincipal + outputGrid[0].BeginningServiceFee +
                                                            managementFee + maintenanceFee - scheduleInput.Residual;
                    //This loop will determine the total due amount to pay.
                    for (int j = 0; j <= i; j++)
                    {
                        totalDue = totalDue -
                            (outputGrid[j].SameDayFeePaid + outputGrid[j].OriginationFeePaid + outputGrid[j].MaintenanceFeePaid + outputGrid[j].ManagementFeePaid +
                            outputGrid[j].NSFFeePaid + outputGrid[j].LateFeePaid) + (outputGrid[j].InterestAccrued - outputGrid[j].InterestPaid) +
                            (outputGrid[j].AccruedServiceFeeInterest - outputGrid[j].ServiceFeeInterestPaid) - outputGrid[j].PrincipalPaid -
                            outputGrid[j].ServiceFeePaid;
                    }

                    if (Round.RoundOffAmount(totalDue) == 0)
                    {
                        startDeletionIndex = i + 1;
                        break;
                    }
                }

                //This loop delete all the rows in decreasing order till startDeletionIndex.
                for (int i = outputGrid.Count - 1; i >= startDeletionIndex; i--)
                {
                    outputGrid.RemoveAt(i);
                }

                #endregion

                #region Extending the schedule

                //This loop is used to extand the repayment schedule after the last date of payment, when extanded dates are available, and there is some amount to be paid.
                for (int j = additionalPaymentCurrentRow; j < scheduleInput.AdditionalPaymentRecords.Count; j++)
                {
                    if (((Round.RoundOffAmount(outputGrid[outputGrid.Count - 1].BeginningPrincipal - outputGrid[outputGrid.Count - 1].PrincipalPaid - scheduleInput.Residual) > 0) ||
                        (Round.RoundOffAmount(outputGrid[outputGrid.Count - 1].BeginningServiceFee - outputGrid[outputGrid.Count - 1].ServiceFeePaid) > 0) ||
                        Round.RoundOffAmount(outputGrid[outputGrid.Count - 1].InterestCarryOver) > 0 ||
                        Round.RoundOffAmount(outputGrid[outputGrid.Count - 1].serviceFeeInterestCarryOver) > 0 ||
                        Round.RoundOffAmount(outputGrid[outputGrid.Count - 1].CumulativeTotalPastDue) > 0) &&
                        (scheduleInput.AdditionalPaymentRecords[j].DateIn >= outputGrid[outputGrid.Count - 1].PaymentDate))
                    {
                        #region Calculate Management Fee and Maintenance Fee

                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, j, true, true, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);");
                        managementFeePayable = ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, j, true, true, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, j, true, true, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);");
                        maintenanceFeePayable = ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, j, true, true, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);

                        #endregion

                        switch (scheduleInput.AdditionalPaymentRecords[j].Flags)
                        {
                            //This condition checks whether it is a principal only additonal payment.
                            case (int)Constants.FlagValues.PrincipalOnly:
                                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : PayPrincipalOnlyAmount(outputGrid, scheduleInput, j, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, true, originationFee, sameDayFee, managementFeePayable, maintenanceFeePayable);");
                                PayPrincipalOnlyAmount(outputGrid, scheduleInput, j, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, true, originationFee, sameDayFee, managementFeePayable, maintenanceFeePayable);
                                break;

                            //This condition checks whether it is a additional payment where paiyment amount will be distributed among components as per priorities.
                            case (int)Constants.FlagValues.AdditionalPayment:
                                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : PayAdditionalPayment(outputGrid, scheduleInput, j, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, true, originationFee, sameDayFee, managementFeePayable, maintenanceFeePayable);");
                                PayAdditionalPayment(outputGrid, scheduleInput, j, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, true, originationFee, sameDayFee, managementFeePayable, maintenanceFeePayable);
                                break;

                            //This condition checks whether it is a discount.
                            case (int)Constants.FlagValues.Discount:
                                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : AddDiscount(outputGrid, scheduleInput, j, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, true, originationFee, sameDayFee);");
                                AddDiscount(outputGrid, scheduleInput, j, inputGridRow, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, true, originationFee, sameDayFee);
                                break;

                            //This condition checks whether it is a event of adding a Mnagament fee or Maintenance fee.
                            case (int)Constants.FlagValues.ManagementFee:
                            case (int)Constants.FlagValues.MaintenanceFee:
                                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFee(outputGrid, scheduleInput, j, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, true, originationFee, sameDayFee);");
                                ManagementAndMaintenanceFee(outputGrid, scheduleInput, j, outputGridRowNumber, defaultTotalAmountToPay, defaultTotalServiceFeePayable, true, originationFee, sameDayFee);
                                break;
                        }
                        outputGridRowNumber++;
                    }
                }

                #endregion

                #region Early Payoff

                if (scheduleInput.EarlyPayoffDate.ToShortDateString() != Convert.ToDateTime(Constants.DefaultDate).ToShortDateString())
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : EarlyPayOff.CalculateEarlyPayOff(outputGrid, scheduleInput, originationFee, sameDayFee, outputSchedule);");
                    EarlyPayOff.CalculateEarlyPayOff(outputGrid, scheduleInput, originationFee, sameDayFee, outputSchedule);
                }
                else if (outputGrid[outputGrid.Count - 1].DueDate > scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].DateIn)
                {
                    additionalPaymentCurrentRow = 0;
                    inputGridRow = 1;
                    managementFeeTableIndex = 0;
                    maintenanceFeeTableIndex = 0;
                    lastManagementFeeChargedIndex = -1;
                    lastMaintenanceFeeChargedIndex = -1;
                    for (int i = 0; i < outputGrid.Count; i++)
                    {
                        if (inputGridRow < scheduleInput.InputRecords.Count && scheduleInput.InputRecords[inputGridRow].DateIn == outputGrid[i].DueDate &&
                                scheduleInput.InputRecords[inputGridRow].Flags == outputGrid[i].Flags)
                        {
                            sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, inputGridRow, false, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);");
                            ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, inputGridRow, false, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);
                            sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, inputGridRow, false, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);");
                            ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, inputGridRow, false, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);
                            inputGridRow++;
                        }
                        else
                        {
                            DateTime effectiveDate = scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].EffectiveDate == DateTime.MinValue ?
                                                    scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].DateIn : scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].EffectiveDate;
                            bool isExtendingRow = (scheduleInput.AdditionalPaymentRecords[additionalPaymentCurrentRow].DateIn > effectiveDate);

                            sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, additionalPaymentCurrentRow, true, isExtendingRow, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);");
                            ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, additionalPaymentCurrentRow, true, isExtendingRow, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);
                            sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, additionalPaymentCurrentRow, true, isExtendingRow, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);");
                            ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, additionalPaymentCurrentRow, true, isExtendingRow, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);
                            additionalPaymentCurrentRow++;
                        }
                    }

                    for (int i = outputSchedule.ManagementFeeAssessment.Count - 1; i > lastManagementFeeChargedIndex; i--)
                    {
                        outputSchedule.ManagementFeeAssessment.RemoveAt(i);
                    }
                    for (int i = outputSchedule.MaintenanceFeeAssessment.Count - 1; i > lastMaintenanceFeeChargedIndex; i--)
                    {
                        outputSchedule.MaintenanceFeeAssessment.RemoveAt(i);
                    }
                }

                #endregion

                sbTracing.AppendLine("Exist:From class ActualPaymentAllocation and method name AllocationOfFunds(List<OutputGrid> outputGrid, LoanDetails scheduleInput, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double loanAmountCalc, double sameDayFee, OutputSchedule outputSchedule)");
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
                if (outputGrid[i].TotalPastDue > (outputGrid[i].NSFFeePastDue + outputGrid[i].LateFeePastDue) && searchType == "OutStanding Bucket" &&
                        outputGrid[i].BucketStatus == "OutStanding")
                {
                    bucketNumber = i;
                    break;
                }
                else if (searchType == "Principal Only" && outputGrid[i].PrincipalPastDue > 0)
                {
                    bucketNumber = i;
                    break;
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
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name ReCalculateOutputGridValues(double remainingPrincipal, double remainingServiceFee, List<OutputGrid> outputGrid, LoanDetails scheduleInput, int nextOutputRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double sameDayFee).");
                sbTracing.AppendLine("Parameter values are : remainingPrincipal = " + remainingPrincipal + ", remainingServiceFee = " + remainingServiceFee + ", nextOutputRow = " + nextOutputRow + ", defaultTotalAmountToPay = " + defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", originationFee = " + originationFee + ", sameDayFee = " + sameDayFee);

                remainingPrincipal = Round.RoundOffAmount(remainingPrincipal);

                //Determine the last interest carry over, and service fee interest carry over value.
                double interestCarryOver = outputGrid[nextOutputRow == 0 ? 0 : nextOutputRow - 1].InterestCarryOver;
                double serviceFeeInterestCarryOver = outputGrid[nextOutputRow == 0 ? 0 : nextOutputRow - 1].AccruedServiceFeeInterestCarryOver;

                //Checks whether the previous scheduled event was an skipped payment or not.
                if (outputGrid[nextOutputRow == 0 ? 0 : nextOutputRow - 1].Flags == (int)Constants.FlagValues.SkipPayment)
                {
                    //Add the interest payment of previous row to carry over, as it was not interest paid due to skipped payment.
                    interestCarryOver += outputGrid[nextOutputRow == 0 ? 0 : nextOutputRow - 1].InterestPayment;
                    //Add the service fee interest payment of previous row to service fee interest carry over, as it was not paid due to skipped payment.
                    serviceFeeInterestCarryOver += outputGrid[nextOutputRow == 0 ? 0 : nextOutputRow - 1].ServiceFeeInterest;
                }

                #region Get management and maintenance fee of upper rows when payment is skipped
                if (nextOutputRow < outputGrid.Count)
                {
                    outputGrid[nextOutputRow].ManagementFee = 0;
                    outputGrid[nextOutputRow].MaintenanceFee = 0;
                }
                if (scheduleInput.ManagementFeeFrequency != (int)Constants.FeeFrequency.AllPayments)
                {
                    int outputRow = nextOutputRow - 1;
                    while (outputRow >= 0)
                    {
                        if (outputGrid[outputRow].Flags == (int)Constants.FlagValues.SkipPayment)
                        {
                            outputGrid[nextOutputRow].ManagementFee = outputGrid[outputRow].ManagementFee;
                            break;
                        }
                        if (outputGrid[outputRow].Flags == (int)Constants.FlagValues.Payment)
                        {
                            break;
                        }
                        outputRow--;
                    }
                }
                else
                {
                    int outputRow = nextOutputRow - 1;
                    while (outputRow >= 0)
                    {
                        if (outputGrid[outputRow].Flags == (int)Constants.FlagValues.SkipPayment)
                        {
                            outputGrid[nextOutputRow].ManagementFee = outputGrid[outputRow].ManagementFee;
                            break;
                        }
                        if (outputGrid[outputRow].Flags == (int)Constants.FlagValues.PrincipalOnly || outputGrid[outputRow].Flags == (int)Constants.FlagValues.AdditionalPayment ||
                                    outputGrid[outputRow].Flags == (int)Constants.FlagValues.Payment)
                        {
                            break;
                        }
                        outputRow--;
                    }
                }

                if (scheduleInput.MaintenanceFeeFrequency != (int)Constants.FeeFrequency.AllPayments)
                {
                    int outputRow = nextOutputRow - 1;
                    while (outputRow >= 0)
                    {
                        if (outputGrid[outputRow].Flags == (int)Constants.FlagValues.SkipPayment)
                        {
                            outputGrid[nextOutputRow].MaintenanceFee = outputGrid[outputRow].MaintenanceFee;
                            break;
                        }
                        if (outputGrid[outputRow].Flags == (int)Constants.FlagValues.Payment)
                        {
                            break;
                        }
                        outputRow--;
                    }
                }
                else
                {
                    int outputRow = nextOutputRow - 1;
                    while (outputRow >= 0)
                    {
                        if (outputGrid[outputRow].Flags == (int)Constants.FlagValues.SkipPayment)
                        {
                            outputGrid[nextOutputRow].MaintenanceFee = outputGrid[outputRow].MaintenanceFee;
                            break;
                        }
                        if (outputGrid[outputRow].Flags == (int)Constants.FlagValues.PrincipalOnly || outputGrid[outputRow].Flags == (int)Constants.FlagValues.AdditionalPayment ||
                                    outputGrid[outputRow].Flags == (int)Constants.FlagValues.Payment)
                        {
                            break;
                        }
                        outputRow--;
                    }
                }

                #endregion

                #region Get NSF and Late fee of upper rows when payment is skipped

                int lastSkippedRow = nextOutputRow;
                for (int i = 0; i < nextOutputRow; i++)
                {
                    if (outputGrid[i].Flags != (int)Constants.FlagValues.SkipPayment)
                    {
                        break;
                    }
                    lastSkippedRow = i;
                }
                if (lastSkippedRow == nextOutputRow - 1)
                {
                    outputGrid[nextOutputRow].NSFFeePastDue = scheduleInput.EarnedNSFFee;
                    outputGrid[nextOutputRow].LateFeePastDue = scheduleInput.EarnedLateFee;
                    outputGrid[nextOutputRow].CumulativeNSFFeePastDue += outputGrid[nextOutputRow].NSFFeePastDue;
                    outputGrid[nextOutputRow].CumulativeLateFeePastDue += outputGrid[nextOutputRow].LateFeePastDue;
                }
                else if (nextOutputRow < outputGrid.Count)
                {
                    outputGrid[nextOutputRow].NSFFeePastDue = 0;
                    outputGrid[nextOutputRow].LateFeePastDue = 0;
                    outputGrid[nextOutputRow].CumulativeNSFFeePastDue += outputGrid[nextOutputRow].NSFFeePastDue;
                    outputGrid[nextOutputRow].CumulativeLateFeePastDue += outputGrid[nextOutputRow].LateFeePastDue;
                }

                #endregion

                #region Get Origination and same day fee to be charged.

                originationFee = Round.RoundOffAmount(originationFee - outputGrid[0].OriginationFeePastDue);
                sameDayFee = Round.RoundOffAmount(sameDayFee - outputGrid[0].SameDayFeePastDue);

                bool useOrgnAndSameDayFee = true;
                for (int i = 2; i <= scheduleInput.InputRecords.Count - 1; i++)
                {
                    if (outputGrid.Count > nextOutputRow && (scheduleInput.InputRecords[i].DateIn <= outputGrid[nextOutputRow].DueDate) &&
                                scheduleInput.InputRecords[i - 1].Flags == (int)Constants.FlagValues.Payment)
                    {
                        useOrgnAndSameDayFee = false;
                        break;
                    }
                }
                if (useOrgnAndSameDayFee)
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

                #endregion

                //This loop recreates the repayment schedules starting from the next schedule date for which payment will be paid.
                for (int i = nextOutputRow; i < outputGrid.Count; i++)
                {
                    double interestPayment;
                    double totalAmountToPay = defaultTotalAmountToPay;
                    double totalServiceFeePayable = defaultTotalServiceFeePayable;
                    double periodicInterestRate;
                    double interestAccrued;

                    double payablePrincipal = (remainingPrincipal < Round.RoundOffAmount(remainingPrincipal - scheduleInput.Residual) ? remainingPrincipal :
                                                                    Round.RoundOffAmount(remainingPrincipal - scheduleInput.Residual));

                    int j = scheduleInput.InputRecords.FindIndex(o => o.DateIn == outputGrid[i].DueDate);
                    //int index = outputGrid.FindIndex(o => o.DueDate == scheduleInput.InputRecords[1].DateIn);

                    DateTime startDate;
                    int row = outputGrid.Take(i).ToList().FindLastIndex(o => o.Flags != (int)Constants.FlagValues.NSFFee &&
                                                        o.Flags != (int)Constants.FlagValues.LateFee && o.Flags != (int)Constants.FlagValues.ManagementFee &&
                                                        o.Flags != (int)Constants.FlagValues.MaintenanceFee);

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
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateOutPutGridValues() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInput, startDate, outputGrid[i].PaymentDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, true,false);");
                        periodicInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInput, startDate, outputGrid[i].PaymentDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, true, false);

                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateOutPutGridValues() method. Calling method :  PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, outputGrid[i].PaymentDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true,true,false);");
                        interestAccrued = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, outputGrid[i].PaymentDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, true, false);
                    }
                    else
                    {
                        //This condition checks whether interest rate is 0 for that period or not. If it is 0, then total payment amount will be loan amount divided
                        //by number of payments.
                        if (Convert.ToInt32(scheduleInput.InputRecords[j].InterestRate) == 0)
                        {
                            totalAmountToPay = Round.RoundOffAmount(outputGrid[0].BeginningPrincipal / (scheduleInput.InputRecords.Count - 1)) +
                                               Round.RoundOffAmount(scheduleInput.EarnedInterest / (scheduleInput.InputRecords.Count - 1));

                            double principalLoanAmount = (scheduleInput.LoanAmount > 0 && scheduleInput.AmountFinanced == 0) ? scheduleInput.LoanAmount : scheduleInput.AmountFinanced;
                            sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateOutPutGridValues() method. Calling method : PrincipalServiceFee.CalculatePrincipalServiceeFee(scheduleInput.ApplyServiceFeeInterest, principalAmount, scheduleInput.ServiceFee);");
                            double principalServiceFee = scheduleInput.IsServiceFeeFinanced ? 0 : PrincipalServiceFee.CalculatePrincipalServiceeFee(scheduleInput.ApplyServiceFeeInterest, principalLoanAmount, scheduleInput.ServiceFee);

                            totalServiceFeePayable = Round.RoundOffAmount(principalServiceFee / (scheduleInput.InputRecords.Count - 1)) +
                                                     Round.RoundOffAmount(scheduleInput.EarnedServiceFeeInterest / (scheduleInput.InputRecords.Count - 1));
                        }
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateOutPutGridValues() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithoutTier(scheduleInput, startDate, outputGrid[i].PaymentDate, Convert.ToDouble(scheduleInput.InputRecords[j].InterestRate), scheduleInput.InputRecords[0].DateIn, true, true, false);");
                        periodicInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithoutTier(scheduleInput, startDate, outputGrid[i].PaymentDate, Convert.ToDouble(scheduleInput.InputRecords[j].InterestRate), scheduleInput.InputRecords[0].DateIn, true, true, false);

                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateOutPutGridValues() method. Calling method : PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, outputGrid[i].PaymentDate, Convert.ToDouble(scheduleInput.InputRecords[j].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, true, false);");
                        interestAccrued = PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, outputGrid[i].PaymentDate, Convert.ToDouble(scheduleInput.InputRecords[j].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, true, false);
                    }
                    interestAccrued = Round.RoundOffAmount(interestAccrued);

                    //This variable calculates the daily interest rate for the period.
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateOutPutGridValues() method. Calling method : DailyInterestRate.CalculateDailyInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, outputGrid[i].PaymentDate, periodicInterestRate, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, false);");
                    double dailyInterestRate = DailyInterestRate.CalculateDailyInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, outputGrid[i].PaymentDate, periodicInterestRate, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, scheduleInput, true, false);

                    //This variable calculates the daily interest amount for the period.
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateOutPutGridValues() method. Calling method : DailyInterestAmount.CalculateDailyInterestAmount(startDate, outputGrid[i].PaymentDate, interestAccrued, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, false);");
                    double dailyInterestAmount = DailyInterestAmount.CalculateDailyInterestAmount(startDate, outputGrid[i].PaymentDate, interestAccrued, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, scheduleInput, true, false);
                    dailyInterestAmount = Round.RoundOffAmount(dailyInterestAmount);

                    #region Calculate principal and interest payable
                    double principalAmountToPay;

                    //This condition determine whether the row is either last row. 
                    //If no, the carry over of interest will be added to interest accrued for current schedule, and limit to total payment. If it last payment schedule, 
                    //all remaining principal amount will be added to total amount to be paid.
                    if ((i != outputGrid.Count - 1))
                    {
                        if ((interestAccrued + interestCarryOver) >= totalAmountToPay)
                        {
                            if (j == 1 && scheduleInput.MinDuration > Constants.MinDurationValue)
                            {
                                totalAmountToPay = interestAccrued + interestCarryOver;
                                interestPayment = interestAccrued + interestCarryOver;
                                interestCarryOver = 0;
                            }
                            else
                            {
                                interestCarryOver = Round.RoundOffAmount((interestAccrued + interestCarryOver) - totalAmountToPay);
                                interestPayment = totalAmountToPay;
                            }
                            principalAmountToPay = 0;
                        }
                        else
                        {
                            interestPayment = (interestAccrued + interestCarryOver);
                            if ((totalAmountToPay - (interestAccrued + interestCarryOver)) > payablePrincipal)
                            {
                                principalAmountToPay = payablePrincipal;
                                totalAmountToPay = principalAmountToPay + interestPayment;
                            }
                            else
                            {
                                principalAmountToPay = totalAmountToPay - (interestAccrued + interestCarryOver);
                            }
                            interestCarryOver = 0;
                            principalAmountToPay = Round.RoundOffAmount(principalAmountToPay);
                        }

                        #region Apply Enforcement Principal from given Enforcement Payment
                        if (j >= scheduleInput.EnforcedPayment && principalAmountToPay == 0)
                        {
                            double payableEnforcedPrincipal = scheduleInput.EnforcedPrincipal > payablePrincipal ? payablePrincipal : scheduleInput.EnforcedPrincipal;
                            if (j == 1 && scheduleInput.MinDuration > Constants.MinDurationValue)
                            {
                                principalAmountToPay = payableEnforcedPrincipal;
                                totalAmountToPay += payableEnforcedPrincipal;
                            }
                            else
                            {
                                double interestPaymentAfterEnforcedPrincipal = interestPayment - payableEnforcedPrincipal;
                                interestPaymentAfterEnforcedPrincipal = interestPaymentAfterEnforcedPrincipal < 0 ? 0 : interestPaymentAfterEnforcedPrincipal;
                                principalAmountToPay = interestPaymentAfterEnforcedPrincipal > 0 ? payableEnforcedPrincipal : interestPayment;
                                interestCarryOver += interestPaymentAfterEnforcedPrincipal > 0 ? payableEnforcedPrincipal : interestPayment;
                                interestPayment = interestPaymentAfterEnforcedPrincipal < 0 ? 0 : interestPaymentAfterEnforcedPrincipal;
                            }
                        }
                        #endregion

                    }
                    else
                    {
                        principalAmountToPay = Round.RoundOffAmount(payablePrincipal);
                        interestPayment = (interestAccrued + interestCarryOver);
                        totalAmountToPay = principalAmountToPay + interestPayment;
                        interestCarryOver = 0;
                    }

                    #endregion

                    #region Calculate service fee and service fee interest payable
                    //determine the service fee, service fee interest to be paid in each period, and calculate the remaining principal service fee on which service fee
                    //interest is being calculated.
                    double payableServiceFee = 0;
                    double payableServiceFeeInterest = 0;
                    double accruedServiceFeeInterest = 0;
                    if (!scheduleInput.IsServiceFeeFinanced)
                    {
                        if (scheduleInput.ApplyServiceFeeInterest == 1 || scheduleInput.ApplyServiceFeeInterest == 3)
                        {
                            if (string.IsNullOrEmpty(scheduleInput.InputRecords[j].InterestRate))
                            {
                                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateSchedules() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, outputGrid[i].PaymentDate, scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true, true,false);");
                                accruedServiceFeeInterest = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, outputGrid[i].PaymentDate, scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true, true, false);
                            }
                            else
                            {
                                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateSchedules() method. Calling method : PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, outputGrid[i].PaymentDate, Convert.ToDouble(scheduleInput.InputRecords[j].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true, true,false);");
                                accruedServiceFeeInterest = PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, outputGrid[i].PaymentDate, Convert.ToDouble(scheduleInput.InputRecords[j].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true, true, false);
                            }
                            accruedServiceFeeInterest = Round.RoundOffAmount(accruedServiceFeeInterest);
                        }
                    }

                    //Checks whether the row is last row of output grid or payable loan amount will become 0 after this row.
                    if (((i == outputGrid.Count - 1) || (payablePrincipal - principalAmountToPay) <= 0) || scheduleInput.ServiceFeeFirstPayment)
                    {
                        payableServiceFee = Round.RoundOffAmount(remainingServiceFee);
                        payableServiceFeeInterest = (accruedServiceFeeInterest + serviceFeeInterestCarryOver);
                        totalServiceFeePayable = payableServiceFee + payableServiceFeeInterest;
                        serviceFeeInterestCarryOver = 0;
                    }
                    else if ((i != outputGrid.Count - 1))
                    {
                        if ((accruedServiceFeeInterest + serviceFeeInterestCarryOver) >= totalServiceFeePayable)
                        {
                            if (j == 1 && scheduleInput.MinDuration > Constants.MinDurationValue)
                            {
                                totalServiceFeePayable = accruedServiceFeeInterest + serviceFeeInterestCarryOver;
                                payableServiceFeeInterest = accruedServiceFeeInterest + serviceFeeInterestCarryOver;
                                serviceFeeInterestCarryOver = 0;
                            }
                            else
                            {
                                serviceFeeInterestCarryOver = Round.RoundOffAmount((accruedServiceFeeInterest + serviceFeeInterestCarryOver) - totalServiceFeePayable);
                                payableServiceFeeInterest = totalServiceFeePayable;
                            }
                            payableServiceFee = 0;
                        }
                        else
                        {
                            payableServiceFeeInterest = (accruedServiceFeeInterest + serviceFeeInterestCarryOver);
                            if ((totalServiceFeePayable - (accruedServiceFeeInterest + serviceFeeInterestCarryOver)) > remainingServiceFee)
                            {
                                payableServiceFee = remainingServiceFee;
                                totalServiceFeePayable = payableServiceFee + payableServiceFeeInterest;

                                if (scheduleInput.EarnedServiceFeeInterest > 0 && totalServiceFeePayable == 0)
                                {
                                    totalServiceFeePayable = (scheduleInput.EarnedServiceFeeInterest / (scheduleInput.InputRecords.Count - 1));
                                }
                            }
                            else
                            {
                                payableServiceFee = totalServiceFeePayable - payableServiceFeeInterest;
                            }
                            serviceFeeInterestCarryOver = 0;
                            payableServiceFee = Round.RoundOffAmount(payableServiceFee);
                        }
                    }
                    #endregion

                    //This variable calculates the total amount payable for the particular period.
                    totalAmountToPay += totalServiceFeePayable + (Round.RoundOffAmount(outputGrid[i].OriginationFee - scheduleInput.EarnedOriginationFee) <= 0 ? 0 :
                                                                            Round.RoundOffAmount(outputGrid[i].OriginationFee - scheduleInput.EarnedOriginationFee)) +
                                                                        (Round.RoundOffAmount(outputGrid[i].SameDayFee - scheduleInput.EarnedSameDayFee) <= 0 ? 0 :
                                                                            Round.RoundOffAmount(outputGrid[i].SameDayFee - scheduleInput.EarnedSameDayFee)) +
                                        ((i == outputGrid.Count - 1) ? 0 :
                                            Round.RoundOffAmount((scheduleInput.EarnedOriginationFee + scheduleInput.EarnedSameDayFee +
                                                                scheduleInput.EarnedManagementFee + scheduleInput.EarnedMaintenanceFee +
                                                                scheduleInput.EarnedNSFFee + scheduleInput.EarnedLateFee) / (scheduleInput.InputRecords.Count - 1)));

                    //After calculations assign all the values in the output grid.
                    outputGrid[i].BeginningPrincipal = Round.RoundOffAmount(remainingPrincipal);
                    outputGrid[i].BeginningServiceFee = Round.RoundOffAmount(remainingServiceFee);
                    outputGrid[i].PeriodicInterestRate = periodicInterestRate;
                    outputGrid[i].DailyInterestRate = dailyInterestRate;
                    outputGrid[i].DailyInterestAmount = dailyInterestAmount;
                    outputGrid[i].InterestAccrued = Round.RoundOffAmount(interestAccrued);
                    outputGrid[i].InterestCarryOver = Round.RoundOffAmount(interestCarryOver);
                    outputGrid[i].InterestPayment = Round.RoundOffAmount(interestPayment);
                    outputGrid[i].PrincipalPayment = Round.RoundOffAmount(principalAmountToPay);
                    outputGrid[i].TotalPayment = Round.RoundOffAmount(totalAmountToPay);
                    outputGrid[i].AccruedServiceFeeInterest = Round.RoundOffAmount(accruedServiceFeeInterest);
                    outputGrid[i].AccruedServiceFeeInterestCarryOver = Round.RoundOffAmount(serviceFeeInterestCarryOver);
                    outputGrid[i].ServiceFee = Round.RoundOffAmount(payableServiceFee);
                    outputGrid[i].ServiceFeeInterest = Round.RoundOffAmount(payableServiceFeeInterest);
                    outputGrid[i].ServiceFeeTotal = Round.RoundOffAmount(totalServiceFeePayable);
                    outputGrid[i].NSFFee = 0;
                    outputGrid[i].LateFee = 0;
                    //Past due amount columns
                    outputGrid[i].PrincipalPastDue = 0;
                    outputGrid[i].InterestPastDue = 0;
                    outputGrid[i].ServiceFeePastDue = 0;
                    outputGrid[i].ServiceFeeInterestPastDue = 0;
                    outputGrid[i].OriginationFeePastDue = 0;
                    outputGrid[i].MaintenanceFeePastDue = 0;
                    outputGrid[i].ManagementFeePastDue = 0;
                    outputGrid[i].SameDayFeePastDue = 0;
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
                    remainingServiceFee = Round.RoundOffAmount(remainingServiceFee);

                    //Reduce the principal amount that remains to pay after a particular period payment.
                    remainingPrincipal -= principalAmountToPay;
                    remainingPrincipal = Round.RoundOffAmount(remainingPrincipal);
                }
                sbTracing.AppendLine("Exist:From class ActualPaymentAllocation and method name ReCalculateOutputGridValues(double remainingPrincipal, double remainingServiceFee, List<OutputGrid> outputGrid, LoanDetails scheduleInput, int nextOutputRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double sameDayFee)");
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
        /// <param name="inputGridRow"></param>
        /// <param name="defaultTotalAmountToPay"></param>
        /// <param name="defaultTotalServiceFeePayable"></param>
        /// <param name="outputGridRow"></param>
        /// <param name="originationFee"></param>
        /// <param name="sameDayFee"></param>
        /// <param name="loanAmount"></param>
        private static void AllocateFundToBuckets(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int inputGridRow, double defaultTotalAmountToPay,
                                                    double defaultTotalServiceFeePayable, int outputGridRow, double originationFee, double sameDayFee, double loanAmount)
        {
            int previousOutStandingBucketNumber = 0;
            bool isLastRow = false;
            bool isPrincipalEqual = false;
            bool isServiceFeeEqual = false;
            double beginningPricipal = Round.RoundOffAmount(loanAmount - outputGrid.Where(o => o.PaymentDate <= outputGrid[outputGridRow].PaymentDate).Sum(o => o.PrincipalPaid)
                                                                    - scheduleInput.Residual);
            double beginningServiceFee = outputGrid[outputGridRow].BeginningServiceFee;

            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name AllocateFundToBuckets(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int inputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, int outputGridRow, double originationFee, double sameDayFee, double loanAmount).");
                sbTracing.AppendLine("Parameter values are : inputGridRow = " + inputGridRow + ", defaultTotalAmountToPay = " + defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", outputGridRow = " + outputGridRow + ", originationFee = " + originationFee + ", sameDayFee = " + sameDayFee + ", loanAmount = " + loanAmount);

                double pastDueAmount = 0;
                for (int i = 0; i < outputGridRow; i++)
                {
                    pastDueAmount = pastDueAmount + outputGrid[i].TotalPastDue - outputGrid[i].PrincipalPastDue - outputGrid[i].ServiceFeePastDue;
                }

                double totalAmountToPay = defaultTotalAmountToPay;
                if (scheduleInput.InputRecords[inputGridRow].InterestRate == "0")
                {
                    totalAmountToPay = outputGrid[0].BeginningPrincipal / (scheduleInput.InputRecords.Count - 1);
                }

                //Checks whether the row is the last row of the output grid.
                if ((outputGrid.Count == outputGridRow + 1) ||
                        (Round.RoundOffAmount(outputGrid[outputGridRow].PrincipalPayment + outputGrid[outputGridRow].InterestPayment) < totalAmountToPay &&
                            (
                                (scheduleInput.PaymentAmount == 0 && string.IsNullOrEmpty(scheduleInput.InputRecords[inputGridRow].PaymentAmount)) ||
                                ((outputGrid[outputGridRow].TotalPayment + pastDueAmount) <= scheduleInput.PaymentAmount && scheduleInput.PaymentAmount > 0) ||
                                (
                                    !string.IsNullOrEmpty(scheduleInput.InputRecords[inputGridRow].PaymentAmount) &&
                                    (outputGrid[outputGridRow].TotalPayment + pastDueAmount) <= Convert.ToDouble(scheduleInput.InputRecords[inputGridRow].PaymentAmount)
                                )
                            )
                        )
                    )
                {
                    isLastRow = true;
                }

                //This condition determines whether it is the last row of output grid.
                if (isLastRow)
                {
                    outputGrid[outputGridRow].NSFFee = outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeNSFFeePastDue;
                    outputGrid[outputGridRow].LateFee = outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeLateFeePastDue;
                    double interestPaidDuePaidInLastSchedule = outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeInterestPastDue;
                    double serviceFeeInterestPaidDuePaidInLastSchedule = outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeServiceFeeInterestPastDue;
                    double otherRemainingDueInLastSchedule = outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeMaintenanceFeePastDue +
                                                        outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeManagementFeePastDue +
                                                        outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeOriginationFeePastDue +
                                                        outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeSameDayFeePastDue;

                    //Add the total past due values in the payable amounts to be paid in the last row of output grid.
                    outputGrid[outputGridRow].InterestPayment += interestPaidDuePaidInLastSchedule;
                    outputGrid[outputGridRow].ServiceFeeTotal += serviceFeeInterestPaidDuePaidInLastSchedule;
                    outputGrid[outputGridRow].ServiceFeeInterest += serviceFeeInterestPaidDuePaidInLastSchedule;

                    outputGrid[outputGridRow].TotalPayment = outputGrid[outputGridRow].PrincipalPayment + outputGrid[outputGridRow].InterestPayment +
                                                            outputGrid[outputGridRow].ServiceFee + outputGrid[outputGridRow].ServiceFeeInterest +
                                                            outputGrid[outputGridRow].OriginationFee + outputGrid[outputGridRow].SameDayFee +
                                                            outputGrid[outputGridRow].ManagementFee + outputGrid[outputGridRow].MaintenanceFee +
                                                            outputGrid[outputGridRow].NSFFee + outputGrid[outputGridRow].LateFee + otherRemainingDueInLastSchedule;
                }

                //Determine the payment amount of the schedule that will be applied all the components as per priority values.
                double paymentAmount = string.IsNullOrEmpty(scheduleInput.InputRecords[inputGridRow].PaymentAmount) ?
                                            ((scheduleInput.PaymentAmount > 0 && !isLastRow) ? scheduleInput.PaymentAmount : outputGrid[outputGridRow].TotalPayment)
                                                : Convert.ToDouble(scheduleInput.InputRecords[inputGridRow].PaymentAmount);

                outputGrid[outputGridRow].PrincipalPastDue = outputGrid[outputGridRow].PrincipalPayment;
                outputGrid[outputGridRow].ServiceFeePastDue = outputGrid[outputGridRow].ServiceFee;
                outputGrid[outputGridRow].InterestPastDue = outputGrid[outputGridRow].InterestPayment;
                outputGrid[outputGridRow].ServiceFeeInterestPastDue = outputGrid[outputGridRow].ServiceFeeInterest;
                outputGrid[outputGridRow].OriginationFeePastDue = outputGrid[outputGridRow].OriginationFee;
                outputGrid[outputGridRow].MaintenanceFeePastDue = outputGrid[outputGridRow].MaintenanceFee;
                outputGrid[outputGridRow].ManagementFeePastDue = outputGrid[outputGridRow].ManagementFee;
                outputGrid[outputGridRow].SameDayFeePastDue = outputGrid[outputGridRow].SameDayFee;

                #region Amount allocation in outstanding Buckets before the current bucket

                while (previousOutStandingBucketNumber < outputGridRow && paymentAmount > 0)
                {
                    //Determine the oldest outstanding bucket index in output grid.
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocateFundToBuckets() method. Calling method : OldestOutStandingBucket(outputGrid, outputGridRow, previousOutStandingBucketNumber, 'OutStanding Bucket');");
                    int bucketNumber = OldestOutStandingBucket(outputGrid, outputGridRow, previousOutStandingBucketNumber, "OutStanding Bucket");

                    if (Round.RoundOffAmount(beginningPricipal) <= Round.RoundOffAmount(outputGrid[bucketNumber].PrincipalPastDue))
                    {
                        isPrincipalEqual = true;
                    }
                    if (Round.RoundOffAmount(beginningServiceFee) <= Round.RoundOffAmount(outputGrid[bucketNumber].ServiceFeePastDue))
                    {
                        isServiceFeeEqual = true;
                    }

                    //If oldest outstanding bucket is cuurent bucket, then break from the loop.
                    if (bucketNumber == outputGridRow)
                    {
                        break;
                    }

                    double payablePrincipal = (beginningPricipal <= outputGrid[bucketNumber].PrincipalPastDue ? beginningPricipal : outputGrid[bucketNumber].PrincipalPastDue);
                    double payableServiceFee = (beginningServiceFee <= outputGrid[bucketNumber].ServiceFeePastDue ? beginningServiceFee : outputGrid[bucketNumber].ServiceFeePastDue);

                    //Checks if total past due amount of oldest outstanding bucket is less than or equal to the payment amount.
                    if (outputGrid[bucketNumber].TotalPastDue <= paymentAmount)
                    {
                        #region Amount greater than past due

                        //Paid amount is added in current payment row
                        outputGrid[outputGridRow].PrincipalPaid += payablePrincipal;
                        outputGrid[outputGridRow].InterestPaid += outputGrid[bucketNumber].InterestPastDue;
                        outputGrid[outputGridRow].ServiceFeePaid += payableServiceFee;
                        outputGrid[outputGridRow].ServiceFeeInterestPaid += outputGrid[bucketNumber].ServiceFeeInterestPastDue;
                        outputGrid[outputGridRow].OriginationFeePaid += outputGrid[bucketNumber].OriginationFeePastDue;
                        outputGrid[outputGridRow].MaintenanceFeePaid += outputGrid[bucketNumber].MaintenanceFeePastDue;
                        outputGrid[outputGridRow].ManagementFeePaid += outputGrid[bucketNumber].ManagementFeePastDue;
                        outputGrid[outputGridRow].SameDayFeePaid += outputGrid[bucketNumber].SameDayFeePastDue;
                        outputGrid[outputGridRow].TotalPaid += (outputGrid[bucketNumber].TotalPastDue - outputGrid[bucketNumber].PrincipalPastDue +
                                                                    payablePrincipal - outputGrid[bucketNumber].ServiceFeePastDue + payableServiceFee -
                                                                    (outputGrid[bucketNumber].NSFFeePastDue + outputGrid[bucketNumber].LateFeePastDue));

                        paymentAmount = Round.RoundOffAmount(paymentAmount - outputGrid[bucketNumber].TotalPastDue + outputGrid[bucketNumber].PrincipalPastDue - payablePrincipal
                                            + outputGrid[bucketNumber].ServiceFeePastDue - payableServiceFee +
                                                    outputGrid[bucketNumber].NSFFeePastDue + outputGrid[bucketNumber].LateFeePastDue);

                        for (int i = bucketNumber + 1; i <= outputGridRow; i++)
                        {
                            if (isPrincipalEqual)
                            {
                                outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - outputGrid[i].PrincipalPastDue);
                                outputGrid[i].PrincipalPastDue = 0;
                            }
                            if (isServiceFeeEqual)
                            {
                                outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - outputGrid[i].ServiceFeePastDue);
                                outputGrid[i].ServiceFeePastDue = 0;
                            }
                        }

                        if (Round.RoundOffAmount(outputGrid[outputGridRow].BeginningPrincipal) <= Round.RoundOffAmount(outputGrid[outputGridRow].PrincipalPayment))
                        {
                            outputGrid[outputGridRow].PrincipalPastDue -= payablePrincipal;
                            outputGrid[outputGridRow].PrincipalPastDue = outputGrid[outputGridRow].PrincipalPastDue <= 0 ? 0 : outputGrid[outputGridRow].PrincipalPastDue;
                        }
                        if (Round.RoundOffAmount(outputGrid[outputGridRow].BeginningServiceFee) == Round.RoundOffAmount(outputGrid[outputGridRow].ServiceFee))
                        {
                            outputGrid[outputGridRow].ServiceFeePastDue -= payableServiceFee;
                            outputGrid[outputGridRow].ServiceFeePastDue = outputGrid[outputGridRow].ServiceFeePastDue <= 0 ? 0 : outputGrid[outputGridRow].ServiceFeePastDue;
                        }

                        beginningPricipal = Round.RoundOffAmount(beginningPricipal - payablePrincipal);
                        beginningServiceFee = Round.RoundOffAmount(beginningServiceFee - payableServiceFee);

                        if (beginningPricipal <= 0)
                        {
                            for (int i = 0; i < outputGridRow; i++)
                            {
                                outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - outputGrid[i].PrincipalPastDue);
                                outputGrid[i].PrincipalPastDue = 0;
                                outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue - outputGrid[i].CumulativePrincipalPastDue);
                                outputGrid[i].CumulativePrincipalPastDue = 0;
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
                        outputGrid[bucketNumber].TotalPastDue = outputGrid[bucketNumber].NSFFeePastDue + outputGrid[bucketNumber].LateFeePastDue;

                        //Set all Cumulative past due balances of next row to 0.
                        for (int i = bucketNumber + 1; i <= outputGridRow; i++)
                        {
                            outputGrid[i].CumulativePrincipalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativePrincipalPastDue - outputGrid[bucketNumber].CumulativePrincipalPastDue);
                            outputGrid[i].CumulativeInterestPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeInterestPastDue - outputGrid[bucketNumber].CumulativeInterestPastDue);
                            outputGrid[i].CumulativeServiceFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeServiceFeePastDue - outputGrid[bucketNumber].CumulativeServiceFeePastDue);
                            outputGrid[i].CumulativeServiceFeeInterestPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeServiceFeeInterestPastDue - outputGrid[bucketNumber].CumulativeServiceFeeInterestPastDue);
                            outputGrid[i].CumulativeOriginationFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeOriginationFeePastDue - outputGrid[bucketNumber].CumulativeOriginationFeePastDue);
                            outputGrid[i].CumulativeMaintenanceFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeMaintenanceFeePastDue - outputGrid[bucketNumber].CumulativeMaintenanceFeePastDue);
                            outputGrid[i].CumulativeManagementFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeManagementFeePastDue - outputGrid[bucketNumber].CumulativeManagementFeePastDue);
                            outputGrid[i].CumulativeSameDayFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeSameDayFeePastDue - outputGrid[bucketNumber].CumulativeSameDayFeePastDue);
                            outputGrid[i].CumulativeTotalPastDue -= (outputGrid[bucketNumber].CumulativePrincipalPastDue + outputGrid[bucketNumber].CumulativeInterestPastDue +
                                                        outputGrid[bucketNumber].CumulativeServiceFeePastDue + outputGrid[bucketNumber].CumulativeServiceFeeInterestPastDue +
                                                        outputGrid[bucketNumber].CumulativeOriginationFeePastDue + outputGrid[bucketNumber].CumulativeMaintenanceFeePastDue +
                                                        outputGrid[bucketNumber].CumulativeManagementFeePastDue + outputGrid[bucketNumber].CumulativeSameDayFeePastDue);
                            outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue);
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

                        #endregion
                    }
                    else
                    {
                        #region Amount less than past due

                        for (int i = 1; i <= 10; i++)
                        {
                            //Allocate funds to interest amount
                            if (scheduleInput.InputRecords[inputGridRow].InterestPriority == i)
                            {
                                if (outputGrid[bucketNumber].InterestPastDue <= paymentAmount)
                                {
                                    outputGrid[outputGridRow].InterestPaid += outputGrid[bucketNumber].InterestPastDue;
                                    paymentAmount -= outputGrid[bucketNumber].InterestPastDue;
                                    outputGrid[bucketNumber].InterestPastDue = 0;
                                    outputGrid[bucketNumber].CumulativeInterestPastDue = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].InterestPaid += paymentAmount;
                                    outputGrid[bucketNumber].InterestPastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativeInterestPastDue -= paymentAmount;
                                    paymentAmount = 0;
                                    break;
                                }
                            }

                            //Allocate funds to principal amount
                            else if (scheduleInput.InputRecords[inputGridRow].PrincipalPriority == i)
                            {
                                if (payablePrincipal <= paymentAmount)
                                {
                                    outputGrid[outputGridRow].PrincipalPaid += payablePrincipal;
                                    paymentAmount -= payablePrincipal;
                                    outputGrid[bucketNumber].PrincipalPastDue = 0;
                                    outputGrid[bucketNumber].CumulativePrincipalPastDue = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].PrincipalPaid += paymentAmount;
                                    outputGrid[bucketNumber].PrincipalPastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativePrincipalPastDue -= paymentAmount;
                                    payablePrincipal = paymentAmount;
                                    paymentAmount = 0;
                                }

                                if (isPrincipalEqual)
                                {
                                    for (int j = bucketNumber + 1; j <= outputGridRow; j++)
                                    {
                                        if (outputGrid[j].Flags == (int)Constants.FlagValues.Payment)
                                        {
                                            outputGrid[j].PrincipalPastDue -= payablePrincipal;
                                        }
                                    }
                                }

                                if (Round.RoundOffAmount(outputGrid[outputGridRow].BeginningPrincipal) <= Round.RoundOffAmount(outputGrid[outputGridRow].PrincipalPastDue))
                                {
                                    outputGrid[outputGridRow].PrincipalPastDue -= payablePrincipal;
                                    outputGrid[outputGridRow].PrincipalPastDue = (outputGrid[outputGridRow].PrincipalPastDue < 0 ? 0 : outputGrid[outputGridRow].PrincipalPastDue);
                                }

                                beginningPricipal = Round.RoundOffAmount(beginningPricipal - payablePrincipal);

                                if (beginningPricipal <= 0)
                                {
                                    for (int j = 0; j < outputGridRow; j++)
                                    {
                                        outputGrid[j].TotalPastDue = Round.RoundOffAmount(outputGrid[j].TotalPastDue - outputGrid[j].PrincipalPastDue);
                                        outputGrid[j].PrincipalPastDue = 0;
                                        outputGrid[j].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[j].CumulativeTotalPastDue - outputGrid[j].CumulativePrincipalPastDue);
                                        outputGrid[j].CumulativePrincipalPastDue = 0;
                                    }
                                }

                                if (paymentAmount == 0)
                                {
                                    break;
                                }
                            }

                            //Allocate funds to Service Fee amount
                            else if (scheduleInput.InputRecords[inputGridRow].ServiceFeePriority == i)
                            {
                                if (payableServiceFee <= paymentAmount)
                                {
                                    outputGrid[outputGridRow].ServiceFeePaid += payableServiceFee;
                                    paymentAmount -= payableServiceFee;
                                    outputGrid[bucketNumber].ServiceFeePastDue = 0;
                                    outputGrid[bucketNumber].CumulativeServiceFeePastDue = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].ServiceFeePaid += paymentAmount;
                                    outputGrid[bucketNumber].ServiceFeePastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativeServiceFeePastDue -= paymentAmount;
                                    payableServiceFee = paymentAmount;
                                    paymentAmount = 0;
                                }

                                if (isServiceFeeEqual)
                                {
                                    for (int j = bucketNumber + 1; j <= outputGridRow; j++)
                                    {
                                        if (outputGrid[j].Flags == (int)Constants.FlagValues.Payment)
                                        {
                                            outputGrid[j].ServiceFeePastDue -= payableServiceFee;
                                        }
                                    }
                                }

                                if (Round.RoundOffAmount(outputGrid[outputGridRow].BeginningServiceFee) <= Round.RoundOffAmount(outputGrid[outputGridRow].ServiceFeePastDue))
                                {
                                    outputGrid[outputGridRow].ServiceFeePastDue -= payableServiceFee;
                                    outputGrid[outputGridRow].ServiceFeePastDue = (outputGrid[outputGridRow].ServiceFeePastDue <= 0 ? 0 : outputGrid[outputGridRow].ServiceFeePastDue);
                                }

                                beginningServiceFee = Round.RoundOffAmount(beginningServiceFee - payableServiceFee);
                                if (paymentAmount == 0)
                                {
                                    break;
                                }
                            }

                            //Allocate funds to Service Fee Interest amount
                            else if (scheduleInput.InputRecords[inputGridRow].ServiceFeeInterestPriority == i)
                            {
                                if (outputGrid[bucketNumber].ServiceFeeInterestPastDue <= paymentAmount)
                                {
                                    outputGrid[outputGridRow].ServiceFeeInterestPaid += outputGrid[bucketNumber].ServiceFeeInterestPastDue;
                                    paymentAmount -= outputGrid[bucketNumber].ServiceFeeInterestPastDue;
                                    outputGrid[bucketNumber].ServiceFeeInterestPastDue = 0;
                                    outputGrid[bucketNumber].CumulativeServiceFeeInterestPastDue = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].ServiceFeeInterestPaid += paymentAmount;
                                    outputGrid[bucketNumber].ServiceFeeInterestPastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativeServiceFeeInterestPastDue -= paymentAmount;
                                    paymentAmount = 0;
                                    break;
                                }
                            }

                            //Allocate funds to Origination Fee amount
                            else if (scheduleInput.InputRecords[inputGridRow].OriginationFeePriority == i)
                            {
                                if (outputGrid[bucketNumber].OriginationFeePastDue <= paymentAmount)
                                {
                                    outputGrid[outputGridRow].OriginationFeePaid += outputGrid[bucketNumber].OriginationFeePastDue;
                                    paymentAmount -= outputGrid[bucketNumber].OriginationFeePastDue;
                                    outputGrid[bucketNumber].OriginationFeePastDue = 0;
                                    outputGrid[bucketNumber].CumulativeOriginationFeePastDue = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].OriginationFeePaid += paymentAmount;
                                    outputGrid[bucketNumber].OriginationFeePastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativeOriginationFeePastDue -= paymentAmount;
                                    paymentAmount = 0;
                                    break;
                                }
                            }

                            //Allocate funds to Management Fee amount
                            else if (scheduleInput.InputRecords[inputGridRow].ManagementFeePriority == i)
                            {
                                if (outputGrid[bucketNumber].ManagementFeePastDue <= paymentAmount)
                                {
                                    outputGrid[outputGridRow].ManagementFeePaid += outputGrid[bucketNumber].ManagementFeePastDue;
                                    paymentAmount -= outputGrid[bucketNumber].ManagementFeePastDue;
                                    outputGrid[bucketNumber].ManagementFeePastDue = 0;
                                    outputGrid[bucketNumber].CumulativeManagementFeePastDue = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].ManagementFeePaid += paymentAmount;
                                    outputGrid[bucketNumber].ManagementFeePastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativeManagementFeePastDue -= paymentAmount;
                                    paymentAmount = 0;
                                    break;
                                }
                            }

                            //Allocate funds to Maintenance Fee amount
                            else if (scheduleInput.InputRecords[inputGridRow].MaintenanceFeePriority == i)
                            {
                                if (outputGrid[bucketNumber].MaintenanceFeePastDue <= paymentAmount)
                                {
                                    outputGrid[outputGridRow].MaintenanceFeePaid += outputGrid[bucketNumber].MaintenanceFeePastDue;
                                    paymentAmount -= outputGrid[bucketNumber].MaintenanceFeePastDue;
                                    outputGrid[bucketNumber].MaintenanceFeePastDue = 0;
                                    outputGrid[bucketNumber].CumulativeMaintenanceFeePastDue = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].MaintenanceFeePaid += paymentAmount;
                                    outputGrid[bucketNumber].MaintenanceFeePastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativeMaintenanceFeePastDue -= paymentAmount;
                                    paymentAmount = 0;
                                    break;
                                }
                            }

                            //Allocate funds to Same Day Fee amount
                            else if (scheduleInput.InputRecords[inputGridRow].SameDayFeePriority == i)
                            {
                                if (outputGrid[bucketNumber].SameDayFeePastDue <= paymentAmount)
                                {
                                    outputGrid[outputGridRow].SameDayFeePaid += outputGrid[bucketNumber].SameDayFeePastDue;
                                    paymentAmount -= outputGrid[bucketNumber].SameDayFeePastDue;
                                    outputGrid[bucketNumber].SameDayFeePastDue = 0;
                                    outputGrid[bucketNumber].CumulativeSameDayFeePastDue = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].SameDayFeePaid += paymentAmount;
                                    outputGrid[bucketNumber].SameDayFeePastDue -= paymentAmount;
                                    outputGrid[bucketNumber].CumulativeSameDayFeePastDue -= paymentAmount;
                                    paymentAmount = 0;
                                    break;
                                }
                            }
                            paymentAmount = Round.RoundOffAmount(paymentAmount);
                        }

                        //Recalculate the Cumulative past due column values from next row of oldest outstanding bucket to current bucket.
                        for (int i = bucketNumber + 1; i <= outputGridRow; i++)
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

                        outputGrid[outputGridRow].TotalPaid = outputGrid[outputGridRow].PrincipalPaid + outputGrid[outputGridRow].InterestPaid +
                                                    outputGrid[outputGridRow].ServiceFeePaid + outputGrid[outputGridRow].ServiceFeeInterestPaid +
                                                    outputGrid[outputGridRow].OriginationFeePaid + outputGrid[outputGridRow].MaintenanceFeePaid +
                                                    outputGrid[outputGridRow].ManagementFeePaid + outputGrid[outputGridRow].SameDayFeePaid +
                                                    outputGrid[outputGridRow].NSFFeePaid + outputGrid[outputGridRow].LateFeePaid;
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

                        outputGrid[bucketNumber].BucketStatus = (outputGrid[bucketNumber].NSFFeePastDue + outputGrid[bucketNumber].LateFeePastDue ==
                                                                                                            outputGrid[bucketNumber].TotalPastDue) ? "" : "OutStanding";

                        #endregion
                    }

                    previousOutStandingBucketNumber = bucketNumber;
                }

                #endregion

                #region Amount allocation for current bucket

                //Remaining funds are allocating to the current row of grid
                if (scheduleInput.InputRecords.FindIndex(o => o.DateIn == outputGrid[outputGridRow].DueDate) != 1 &&
                        (!isLastRow ? (paymentAmount >= outputGrid[outputGridRow].TotalPayment) : (paymentAmount >= Round.RoundOffAmount(outputGrid[outputGridRow].TotalPayment - outputGrid[outputGridRow].TotalPaid))) &&
                        outputGrid[outputGridRow].TotalPayment > 0)
                {
                    paymentAmount += outputGrid[outputGridRow].TotalPaid;

                    //Paid amount is added in current payment row
                    if (isLastRow)
                    {
                        outputGrid[outputGridRow].PrincipalPaid = (beginningPricipal <= outputGrid[outputGridRow].PrincipalPayment ? beginningPricipal : outputGrid[outputGridRow].PrincipalPayment);
                        outputGrid[outputGridRow].ServiceFeePaid = (beginningServiceFee <= outputGrid[outputGridRow].ServiceFee ? beginningServiceFee : outputGrid[outputGridRow].ServiceFee);
                        outputGrid[outputGridRow].InterestPaid = outputGrid[outputGridRow].InterestPayment;
                        outputGrid[outputGridRow].ServiceFeeInterestPaid = outputGrid[outputGridRow].ServiceFeeInterest;
                        outputGrid[outputGridRow].TotalPaid = outputGrid[outputGridRow].TotalPayment - outputGrid[outputGridRow].PrincipalPayment +
                                                                            outputGrid[outputGridRow].PrincipalPaid - outputGrid[outputGridRow].ServiceFee +
                                                                            outputGrid[outputGridRow].ServiceFeePaid;
                    }
                    else
                    {
                        outputGrid[outputGridRow].PrincipalPaid += (beginningPricipal <= outputGrid[outputGridRow].PrincipalPayment ? beginningPricipal : outputGrid[outputGridRow].PrincipalPayment);
                        outputGrid[outputGridRow].ServiceFeePaid += (beginningServiceFee <= outputGrid[outputGridRow].ServiceFee ? beginningServiceFee : outputGrid[outputGridRow].ServiceFee);
                        outputGrid[outputGridRow].InterestPaid += outputGrid[outputGridRow].InterestPayment;
                        outputGrid[outputGridRow].ServiceFeeInterestPaid += outputGrid[outputGridRow].ServiceFeeInterest;
                        outputGrid[outputGridRow].TotalPaid += (outputGrid[outputGridRow].TotalPayment - outputGrid[outputGridRow].PrincipalPayment +
                                                                            outputGrid[outputGridRow].PrincipalPaid - outputGrid[outputGridRow].ServiceFee +
                                                                            outputGrid[outputGridRow].ServiceFeePaid);
                    }
                    outputGrid[outputGridRow].OriginationFeePaid += outputGrid[outputGridRow].OriginationFee;
                    outputGrid[outputGridRow].MaintenanceFeePaid += outputGrid[outputGridRow].MaintenanceFee;
                    outputGrid[outputGridRow].ManagementFeePaid += outputGrid[outputGridRow].ManagementFee;
                    outputGrid[outputGridRow].SameDayFeePaid += outputGrid[outputGridRow].SameDayFee;

                    //Set all past due balances to 0, in the oldest outstanding row.
                    outputGrid[outputGridRow].PrincipalPastDue = 0;
                    outputGrid[outputGridRow].InterestPastDue = 0;
                    outputGrid[outputGridRow].ServiceFeePastDue = 0;
                    outputGrid[outputGridRow].ServiceFeeInterestPastDue = 0;
                    outputGrid[outputGridRow].OriginationFeePastDue = 0;
                    outputGrid[outputGridRow].MaintenanceFeePastDue = 0;
                    outputGrid[outputGridRow].ManagementFeePastDue = 0;
                    outputGrid[outputGridRow].SameDayFeePastDue = 0;
                    outputGrid[outputGridRow].TotalPastDue = 0;

                    //Set all Cumulative past due balances to 0, in the oldest outstanding row.
                    outputGrid[outputGridRow].CumulativePrincipalPastDue = 0;
                    outputGrid[outputGridRow].CumulativeInterestPastDue = 0;
                    outputGrid[outputGridRow].CumulativeServiceFeePastDue = 0;
                    outputGrid[outputGridRow].CumulativeServiceFeeInterestPastDue = 0;
                    outputGrid[outputGridRow].CumulativeOriginationFeePastDue = 0;
                    outputGrid[outputGridRow].CumulativeMaintenanceFeePastDue = 0;
                    outputGrid[outputGridRow].CumulativeManagementFeePastDue = 0;
                    outputGrid[outputGridRow].CumulativeSameDayFeePastDue = 0;
                    outputGrid[outputGridRow].CumulativeTotalPastDue = outputGrid[outputGridRow].CumulativeNSFFeePastDue + outputGrid[outputGridRow].CumulativeLateFeePastDue;
                    outputGrid[outputGridRow].BucketStatus = "Satisfied";

                    paymentAmount -= (outputGrid[outputGridRow].PrincipalPaid + outputGrid[outputGridRow].ServiceFeePaid +
                                        outputGrid[outputGridRow].InterestPaid + outputGrid[outputGridRow].ServiceFeeInterestPaid +
                                        outputGrid[outputGridRow].OriginationFeePaid + outputGrid[outputGridRow].MaintenanceFeePaid +
                                        outputGrid[outputGridRow].ManagementFeePaid + outputGrid[outputGridRow].SameDayFeePaid);
                    paymentAmount = Round.RoundOffAmount(paymentAmount);
                }
                else
                {
                    //Set all past due balances of current row equals to original amount to be paid.
                    if (isLastRow)
                    {
                        outputGrid[outputGridRow].PrincipalPastDue = outputGrid[outputGridRow].PrincipalPayment - outputGrid[outputGridRow].PrincipalPaid;
                        outputGrid[outputGridRow].ServiceFeePastDue = outputGrid[outputGridRow].ServiceFee - outputGrid[outputGridRow].ServiceFeePaid;
                        outputGrid[outputGridRow].InterestPastDue = outputGrid[outputGridRow].InterestPayment - outputGrid[outputGridRow].InterestPaid;
                        outputGrid[outputGridRow].ServiceFeeInterestPastDue = outputGrid[outputGridRow].ServiceFeeInterest - outputGrid[outputGridRow].ServiceFeeInterestPaid;
                    }

                    outputGrid[outputGridRow].OriginationFeePastDue = outputGrid[outputGridRow].OriginationFee;
                    outputGrid[outputGridRow].MaintenanceFeePastDue = outputGrid[outputGridRow].MaintenanceFee;
                    outputGrid[outputGridRow].ManagementFeePastDue = outputGrid[outputGridRow].ManagementFee;
                    outputGrid[outputGridRow].SameDayFeePastDue = outputGrid[outputGridRow].SameDayFee;

                    //Set all Cumulative past due balances of current row equals to current past due plus remaining cumulative past due amount to be paid.
                    outputGrid[outputGridRow].CumulativePrincipalPastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePrincipalPastDue) + outputGrid[outputGridRow].PrincipalPastDue;
                    outputGrid[outputGridRow].CumulativeInterestPastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeInterestPastDue) + outputGrid[outputGridRow].InterestPastDue;
                    outputGrid[outputGridRow].CumulativeServiceFeePastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeePastDue) + outputGrid[outputGridRow].ServiceFeePastDue;
                    outputGrid[outputGridRow].CumulativeServiceFeeInterestPastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeInterestPastDue) + outputGrid[outputGridRow].ServiceFeeInterestPastDue;
                    outputGrid[outputGridRow].CumulativeOriginationFeePastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeOriginationFeePastDue) + outputGrid[outputGridRow].OriginationFeePastDue;
                    outputGrid[outputGridRow].CumulativeMaintenanceFeePastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeMaintenanceFeePastDue) + outputGrid[outputGridRow].MaintenanceFeePastDue;
                    outputGrid[outputGridRow].CumulativeManagementFeePastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeManagementFeePastDue) + outputGrid[outputGridRow].ManagementFeePastDue;
                    outputGrid[outputGridRow].CumulativeSameDayFeePastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeSameDayFeePastDue) + outputGrid[outputGridRow].SameDayFeePastDue;

                    for (int i = 1; i <= 10; i++)
                    {
                        if (paymentAmount <= 0)
                        {
                            break;
                        }

                        //Allocate funds to interest amount
                        if (scheduleInput.InputRecords[inputGridRow].InterestPriority == i)
                        {
                            double interestAmount = isLastRow ? (outputGrid[outputGridRow].InterestPayment - outputGrid[outputGridRow].InterestPaid) :
                                                                                   outputGrid[outputGridRow].InterestPayment;
                            if (interestAmount <= paymentAmount)
                            {
                                outputGrid[outputGridRow].InterestPaid += interestAmount;
                                paymentAmount -= interestAmount;
                                outputGrid[outputGridRow].InterestPastDue = 0;
                                outputGrid[outputGridRow].CumulativeInterestPastDue = 0;
                            }
                            else
                            {
                                outputGrid[outputGridRow].InterestPaid += paymentAmount;
                                outputGrid[outputGridRow].InterestPastDue = Round.RoundOffAmount(outputGrid[outputGridRow].InterestPastDue - paymentAmount);
                                outputGrid[outputGridRow].CumulativeInterestPastDue = Round.RoundOffAmount(outputGrid[outputGridRow].CumulativeInterestPastDue - paymentAmount);
                                paymentAmount = 0;
                                break;
                            }
                            outputGrid[outputGridRow].InterestPaid = Round.RoundOffAmount(outputGrid[outputGridRow].InterestPaid);
                        }

                        //Allocate funds to principal amount
                        else if (scheduleInput.InputRecords[inputGridRow].PrincipalPriority == i)
                        {
                            double principalAmount = (isLastRow || isPrincipalEqual) ? (outputGrid[outputGridRow].PrincipalPayment - outputGrid[outputGridRow].PrincipalPaid) :
                                                                                   outputGrid[outputGridRow].PrincipalPayment;
                            principalAmount = (principalAmount <= beginningPricipal ? principalAmount : beginningPricipal);
                            if (principalAmount <= paymentAmount)
                            {
                                outputGrid[outputGridRow].PrincipalPaid += principalAmount;
                                paymentAmount -= principalAmount;
                                outputGrid[outputGridRow].PrincipalPastDue = 0;
                                outputGrid[outputGridRow].CumulativePrincipalPastDue = 0;
                            }
                            else
                            {
                                outputGrid[outputGridRow].PrincipalPaid += paymentAmount;
                                outputGrid[outputGridRow].PrincipalPastDue = Round.RoundOffAmount(outputGrid[outputGridRow].PrincipalPastDue - paymentAmount);
                                outputGrid[outputGridRow].CumulativePrincipalPastDue = Round.RoundOffAmount(outputGrid[outputGridRow].CumulativePrincipalPastDue - paymentAmount);
                                paymentAmount = 0;
                                break;
                            }
                            outputGrid[outputGridRow].PrincipalPaid = Round.RoundOffAmount(outputGrid[outputGridRow].PrincipalPaid);
                        }

                        //Allocate funds to Service Fee amount
                        else if (scheduleInput.InputRecords[inputGridRow].ServiceFeePriority == i)
                        {
                            double serviceFeeAmount = (isLastRow || isServiceFeeEqual) ? (outputGrid[outputGridRow].ServiceFee - outputGrid[outputGridRow].ServiceFeePaid) :
                                                                                   outputGrid[outputGridRow].ServiceFee;
                            serviceFeeAmount = (serviceFeeAmount <= beginningServiceFee ? serviceFeeAmount : beginningServiceFee);
                            if (serviceFeeAmount <= paymentAmount)
                            {
                                outputGrid[outputGridRow].ServiceFeePaid += serviceFeeAmount;
                                paymentAmount -= serviceFeeAmount;
                                outputGrid[outputGridRow].ServiceFeePastDue = 0;
                                outputGrid[outputGridRow].CumulativeServiceFeePastDue = 0;
                            }
                            else
                            {
                                outputGrid[outputGridRow].ServiceFeePaid += paymentAmount;
                                outputGrid[outputGridRow].ServiceFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].ServiceFeePastDue - paymentAmount);
                                outputGrid[outputGridRow].CumulativeServiceFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].CumulativeServiceFeePastDue - paymentAmount);
                                paymentAmount = 0;
                                break;
                            }
                            outputGrid[outputGridRow].ServiceFeePaid = Round.RoundOffAmount(outputGrid[outputGridRow].ServiceFeePaid);
                        }

                        //Allocate funds to Service Fee Interest amount
                        else if (scheduleInput.InputRecords[inputGridRow].ServiceFeeInterestPriority == i)
                        {
                            double serviceFeeInterestAmount = isLastRow ? (outputGrid[outputGridRow].ServiceFeeInterest - outputGrid[outputGridRow].ServiceFeeInterestPaid) :
                                                                                   outputGrid[outputGridRow].ServiceFeeInterest;
                            if (serviceFeeInterestAmount <= paymentAmount)
                            {
                                outputGrid[outputGridRow].ServiceFeeInterestPaid += serviceFeeInterestAmount;
                                paymentAmount -= serviceFeeInterestAmount;
                                outputGrid[outputGridRow].ServiceFeeInterestPastDue = 0;
                                outputGrid[outputGridRow].CumulativeServiceFeeInterestPastDue = 0;
                            }
                            else
                            {
                                outputGrid[outputGridRow].ServiceFeeInterestPaid += paymentAmount;
                                outputGrid[outputGridRow].ServiceFeeInterestPastDue = Round.RoundOffAmount(outputGrid[outputGridRow].ServiceFeeInterestPastDue - paymentAmount);
                                outputGrid[outputGridRow].CumulativeServiceFeeInterestPastDue = Round.RoundOffAmount(outputGrid[outputGridRow].CumulativeServiceFeeInterestPastDue - paymentAmount);
                                paymentAmount = 0;
                                break;
                            }
                            outputGrid[outputGridRow].ServiceFeeInterestPaid = Round.RoundOffAmount(outputGrid[outputGridRow].ServiceFeeInterestPaid);
                        }

                        //Allocate funds to Origination Fee amount
                        else if (scheduleInput.InputRecords[inputGridRow].OriginationFeePriority == i)
                        {
                            if (outputGrid[outputGridRow].OriginationFee <= paymentAmount)
                            {
                                outputGrid[outputGridRow].OriginationFeePaid += outputGrid[outputGridRow].OriginationFee;
                                paymentAmount -= outputGrid[outputGridRow].OriginationFee;
                                outputGrid[outputGridRow].OriginationFeePastDue = 0;
                                outputGrid[outputGridRow].CumulativeOriginationFeePastDue = 0;
                            }
                            else
                            {
                                outputGrid[outputGridRow].OriginationFeePaid += paymentAmount;
                                outputGrid[outputGridRow].OriginationFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].OriginationFeePastDue - paymentAmount);
                                outputGrid[outputGridRow].CumulativeOriginationFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].CumulativeOriginationFeePastDue - paymentAmount);
                                paymentAmount = 0;
                                break;
                            }
                            outputGrid[outputGridRow].OriginationFeePaid = Round.RoundOffAmount(outputGrid[outputGridRow].OriginationFeePaid);
                        }

                        //Allocate funds to Management Fee amount
                        else if (scheduleInput.InputRecords[inputGridRow].ManagementFeePriority == i)
                        {
                            if (outputGrid[outputGridRow].ManagementFee <= paymentAmount)
                            {
                                outputGrid[outputGridRow].ManagementFeePaid += outputGrid[outputGridRow].ManagementFee;
                                paymentAmount -= outputGrid[outputGridRow].ManagementFee;
                                outputGrid[outputGridRow].ManagementFeePastDue = 0;
                                outputGrid[outputGridRow].CumulativeManagementFeePastDue = 0;
                            }
                            else
                            {
                                outputGrid[outputGridRow].ManagementFeePaid += paymentAmount;
                                outputGrid[outputGridRow].ManagementFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].ManagementFeePastDue - paymentAmount);
                                outputGrid[outputGridRow].CumulativeManagementFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].CumulativeManagementFeePastDue - paymentAmount);
                                paymentAmount = 0;
                                break;
                            }
                            outputGrid[outputGridRow].ManagementFeePaid = Round.RoundOffAmount(outputGrid[outputGridRow].ManagementFeePaid);
                        }

                        //Allocate funds to Maintenance Fee amount
                        else if (scheduleInput.InputRecords[inputGridRow].MaintenanceFeePriority == i)
                        {
                            if (outputGrid[outputGridRow].MaintenanceFee <= paymentAmount)
                            {
                                outputGrid[outputGridRow].MaintenanceFeePaid += outputGrid[outputGridRow].MaintenanceFee;
                                paymentAmount -= outputGrid[outputGridRow].MaintenanceFee;
                                outputGrid[outputGridRow].MaintenanceFeePastDue = 0;
                                outputGrid[outputGridRow].CumulativeMaintenanceFeePastDue = 0;
                            }
                            else
                            {
                                outputGrid[outputGridRow].MaintenanceFeePaid += paymentAmount;
                                outputGrid[outputGridRow].MaintenanceFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].MaintenanceFeePastDue - paymentAmount);
                                outputGrid[outputGridRow].CumulativeMaintenanceFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].CumulativeMaintenanceFeePastDue - paymentAmount);
                                paymentAmount = 0;
                                break;
                            }
                            outputGrid[outputGridRow].MaintenanceFeePaid = Round.RoundOffAmount(outputGrid[outputGridRow].MaintenanceFeePaid);
                        }

                        //Allocate funds to Same Day Fee amount
                        else if (scheduleInput.InputRecords[inputGridRow].SameDayFeePriority == i)
                        {
                            if (outputGrid[outputGridRow].SameDayFee <= paymentAmount)
                            {
                                outputGrid[outputGridRow].SameDayFeePaid += outputGrid[outputGridRow].SameDayFee;
                                paymentAmount -= outputGrid[outputGridRow].SameDayFee;
                                outputGrid[outputGridRow].SameDayFeePastDue = 0;
                                outputGrid[outputGridRow].CumulativeSameDayFeePastDue = 0;
                            }
                            else
                            {
                                outputGrid[outputGridRow].SameDayFeePaid += paymentAmount;
                                outputGrid[outputGridRow].SameDayFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].SameDayFeePastDue - paymentAmount);
                                outputGrid[outputGridRow].CumulativeSameDayFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].CumulativeSameDayFeePastDue - paymentAmount);
                                paymentAmount = 0;
                                break;
                            }
                            outputGrid[outputGridRow].SameDayFeePaid = Round.RoundOffAmount(outputGrid[outputGridRow].SameDayFeePaid);
                        }
                    }
                    outputGrid[outputGridRow].TotalPastDue = outputGrid[outputGridRow].PrincipalPastDue + outputGrid[outputGridRow].InterestPastDue +
                                                        outputGrid[outputGridRow].ServiceFeePastDue + outputGrid[outputGridRow].ServiceFeeInterestPastDue +
                                                        outputGrid[outputGridRow].OriginationFeePastDue + outputGrid[outputGridRow].MaintenanceFeePastDue +
                                                        outputGrid[outputGridRow].ManagementFeePastDue + outputGrid[outputGridRow].SameDayFeePastDue +
                                                        outputGrid[outputGridRow].NSFFeePastDue + outputGrid[outputGridRow].LateFeePastDue;
                    outputGrid[outputGridRow].CumulativeTotalPastDue = outputGrid[outputGridRow].CumulativePrincipalPastDue + outputGrid[outputGridRow].CumulativeInterestPastDue +
                                                    outputGrid[outputGridRow].CumulativeServiceFeePastDue + outputGrid[outputGridRow].CumulativeServiceFeeInterestPastDue +
                                                    outputGrid[outputGridRow].CumulativeOriginationFeePastDue + outputGrid[outputGridRow].CumulativeMaintenanceFeePastDue +
                                                    outputGrid[outputGridRow].CumulativeManagementFeePastDue + outputGrid[outputGridRow].CumulativeSameDayFeePastDue +
                                                    outputGrid[outputGridRow].CumulativeNSFFeePastDue + outputGrid[outputGridRow].CumulativeLateFeePastDue;

                    if (outputGrid[outputGridRow].TotalPastDue > 0)
                    {
                        outputGrid[outputGridRow].BucketStatus = "OutStanding";
                    }
                    else
                    {
                        outputGrid[outputGridRow].BucketStatus = "Satisfied";
                    }
                }

                #endregion

                double remainingPrincipal = outputGrid[0].BeginningPrincipal;
                double remainingServiceFee = outputGrid[0].BeginningServiceFee;

                //This loop determine sthe remaining principal loan amount and principal service fee amount ot be paid.
                for (int i = 0; i <= outputGridRow; i++)
                {
                    remainingPrincipal -= outputGrid[i].PrincipalPaid;
                    remainingServiceFee -= outputGrid[i].ServiceFeePaid;
                }
                remainingPrincipal = Round.RoundOffAmount(remainingPrincipal);
                remainingServiceFee = Round.RoundOffAmount(remainingServiceFee);

                //Check whether it is currently the last row of output grid, and there is some amount to be paid, and input grid contains dates after last date of output grid.
                if (isLastRow && ((remainingPrincipal - scheduleInput.Residual) > 0 || remainingServiceFee > 0 || outputGrid[outputGrid.Count - 1].InterestCarryOver > 0 ||
                        outputGrid[outputGrid.Count - 1].serviceFeeInterestCarryOver > 0 || outputGrid[outputGrid.Count - 1].CumulativeTotalPastDue > 0)
                        && scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].DateIn > outputGrid[outputGridRow].DueDate)
                {
                    // Deduct the NSF and Late fee past due amounts from the last row of output grid as another row will be added.
                    outputGrid[outputGridRow].TotalPayment = outputGrid[outputGridRow].TotalPayment -
                                (outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeNSFFeePastDue +
                                outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeLateFeePastDue);
                    outputGrid[outputGridRow].NSFFee = 0;
                    outputGrid[outputGridRow].LateFee = 0;
                }

                //This condition checks whether there is any payment amount remains after pay down all the outstanding amount as well as current bucket.
                if (paymentAmount > 0)
                {
                    //Allocate the remaining amount to the action fees i.e. NSF fee and late fee.
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocateFundToBuckets() method. Calling method : AllocateRemainingFundsToNSFAndLateFee(outputGrid, scheduleInput, currentInputGridRowRow, outputGridRow, false, ref paymentAmount);");
                    AllocateRemainingFundsToNSFAndLateFee(outputGrid, scheduleInput, inputGridRow, outputGridRow, false, ref paymentAmount);

                    //Allocate the remaining amount to the principal amount and service fee as per their priorities.
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocateFundToBuckets() method. Calling method : AllocateRemainingFunds(outputGrid, scheduleInput, currentInputGridRowRow, outputGridRow, paymentAmount, ref remainingPrincipal, ref remainingServiceFee);");
                    AllocateRemainingFunds(outputGrid, scheduleInput, inputGridRow, outputGridRow, paymentAmount, ref remainingPrincipal, ref remainingServiceFee);
                }

                //Recalculate values in output grid as per remaining loan amount and service fee.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocateFundToBuckets() method. Calling method : ReCalculateOutputGridValues(remainingPrincipal, remainingServiceFee, outputGrid, scheduleInput, outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);");
                ReCalculateOutputGridValues(remainingPrincipal, remainingServiceFee, outputGrid, scheduleInput, outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);

                outputGrid[outputGridRow].TotalPaid = outputGrid[outputGridRow].PrincipalPaid + outputGrid[outputGridRow].InterestPaid +
                                                        outputGrid[outputGridRow].ServiceFeePaid + outputGrid[outputGridRow].ServiceFeeInterestPaid +
                                                        outputGrid[outputGridRow].OriginationFeePaid + outputGrid[outputGridRow].MaintenanceFeePaid +
                                                        outputGrid[outputGridRow].ManagementFeePaid + outputGrid[outputGridRow].SameDayFeePaid +
                                                        outputGrid[outputGridRow].NSFFeePaid + outputGrid[outputGridRow].LateFeePaid;

                //Cumulative amount paid is added in current payament row
                outputGrid[outputGridRow].CumulativePrincipal = outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativePrincipal + outputGrid[outputGridRow].PrincipalPaid;
                outputGrid[outputGridRow].CumulativeInterest = outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeInterest + outputGrid[outputGridRow].InterestPaid;
                outputGrid[outputGridRow].CumulativePayment = outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativePayment + outputGrid[outputGridRow].TotalPaid;
                outputGrid[outputGridRow].CumulativeServiceFee = outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeServiceFee + outputGrid[outputGridRow].ServiceFeePaid;
                outputGrid[outputGridRow].CumulativeServiceFeeInterest = outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeServiceFeeInterest + outputGrid[outputGridRow].ServiceFeeInterestPaid;
                outputGrid[outputGridRow].CumulativeServiceFeeTotal = outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeServiceFeeTotal +
                                                                    outputGrid[outputGridRow].ServiceFeePaid + outputGrid[outputGridRow].ServiceFeeInterestPaid;
                outputGrid[outputGridRow].CumulativeOriginationFee = outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeOriginationFee + outputGrid[outputGridRow].OriginationFeePaid;
                outputGrid[outputGridRow].CumulativeMaintenanceFee = outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeMaintenanceFee + outputGrid[outputGridRow].MaintenanceFeePaid;
                outputGrid[outputGridRow].CumulativeManagementFee = outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeManagementFee + outputGrid[outputGridRow].ManagementFeePaid;
                outputGrid[outputGridRow].CumulativeSameDayFee = outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeSameDayFee + outputGrid[outputGridRow].SameDayFeePaid;
                outputGrid[outputGridRow].CumulativeNSFFee = outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeNSFFee + outputGrid[outputGridRow].NSFFeePaid;
                outputGrid[outputGridRow].CumulativeLateFee = outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeLateFee + outputGrid[outputGridRow].LateFeePaid;
                outputGrid[outputGridRow].CumulativeTotalFees = outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].CumulativeTotalFees +
                                outputGrid[outputGridRow].ServiceFeePaid + outputGrid[outputGridRow].ServiceFeeInterestPaid + outputGrid[outputGridRow].OriginationFeePaid +
                                outputGrid[outputGridRow].MaintenanceFeePaid + outputGrid[outputGridRow].ManagementFeePaid + outputGrid[outputGridRow].SameDayFeePaid +
                                outputGrid[outputGridRow].NSFFeePaid + outputGrid[outputGridRow].LateFeePaid;

                //Cumulative amount past due is added in current row
                if (!isLastRow)
                {
                    outputGrid[outputGridRow + 1].CumulativePrincipalPastDue = outputGrid[outputGridRow].CumulativePrincipalPastDue + outputGrid[outputGridRow + 1].PrincipalPayment;
                    outputGrid[outputGridRow + 1].CumulativeInterestPastDue = outputGrid[outputGridRow].CumulativeInterestPastDue + outputGrid[outputGridRow + 1].InterestPayment;
                    outputGrid[outputGridRow + 1].CumulativeServiceFeePastDue = outputGrid[outputGridRow].CumulativeServiceFeePastDue + outputGrid[outputGridRow + 1].ServiceFee;
                    outputGrid[outputGridRow + 1].CumulativeServiceFeeInterestPastDue = outputGrid[outputGridRow].CumulativeServiceFeeInterestPastDue + outputGrid[outputGridRow + 1].ServiceFeeInterest;
                    outputGrid[outputGridRow + 1].CumulativeOriginationFeePastDue = outputGrid[outputGridRow].CumulativeOriginationFeePastDue + outputGrid[outputGridRow + 1].OriginationFee;
                    outputGrid[outputGridRow + 1].CumulativeMaintenanceFeePastDue = outputGrid[outputGridRow].CumulativeMaintenanceFeePastDue + outputGrid[outputGridRow + 1].MaintenanceFee;
                    outputGrid[outputGridRow + 1].CumulativeManagementFeePastDue = outputGrid[outputGridRow].CumulativeManagementFeePastDue + outputGrid[outputGridRow + 1].ManagementFee;
                    outputGrid[outputGridRow + 1].CumulativeSameDayFeePastDue = outputGrid[outputGridRow].CumulativeSameDayFeePastDue + outputGrid[outputGridRow + 1].SameDayFee;
                    outputGrid[outputGridRow + 1].CumulativeNSFFeePastDue = outputGrid[outputGridRow].CumulativeNSFFeePastDue;
                    outputGrid[outputGridRow + 1].CumulativeLateFeePastDue = outputGrid[outputGridRow].CumulativeLateFeePastDue;
                    outputGrid[outputGridRow + 1].CumulativeTotalPastDue = outputGrid[outputGridRow].CumulativeTotalPastDue + outputGrid[outputGridRow + 1].TotalPayment;
                }
                else
                {
                    outputGrid[outputGridRow].BucketStatus = (outputGrid[outputGridRow].CumulativeTotalPastDue != 0 ? "OutStanding" : "Satisfied");
                }
                sbTracing.AppendLine("Exist:From class ActualPaymentAllocation and method name AllocateFundToBuckets(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int inputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, int outputGridRow, double originationFee, double sameDayFee, double loanAmount)");
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
            int actionFeeRow = 0;

            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name AllocateRemainingFundsToNSFAndLateFee(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int currentRow, int currentOutputRow, bool isAdditionalPaymentAllocation, ref double paymentAmount).");
                sbTracing.AppendLine("Parameter values are : currentRow = " + currentRow + ", currentOutputRow = " + currentOutputRow + ", isAdditionalPaymentAllocation = " + isAdditionalPaymentAllocation + ", paymentAmount = " + paymentAmount);

                //This loop pay down all the nsf and late fee starting from first row to current row.
                while (actionFeeRow <= currentOutputRow && paymentAmount > 0)
                {
                    int nsfFeePriority = isAdditionalPaymentAllocation ? scheduleInput.AdditionalPaymentRecords[currentRow].NSFFeePriority :
                                                                        scheduleInput.InputRecords[currentRow].NSFFeePriority;
                    int lateFeePriroity = isAdditionalPaymentAllocation ? scheduleInput.AdditionalPaymentRecords[currentRow].LateFeePriority :
                                                                        scheduleInput.InputRecords[currentRow].LateFeePriority;

                    //checks whether the priority of NSF fee is less than the Late fee.
                    if (nsfFeePriority < lateFeePriroity)
                    {
                        #region NSF fee payment

                        double lowestValueInNsfFee = (outputGrid[actionFeeRow].NSFFeePastDue <= paymentAmount) ? outputGrid[actionFeeRow].NSFFeePastDue : paymentAmount;
                        if (lowestValueInNsfFee > 0)
                        {
                            outputGrid[currentOutputRow].NSFFeePaid += lowestValueInNsfFee;
                            outputGrid[actionFeeRow].NSFFeePastDue = Round.RoundOffAmount(outputGrid[actionFeeRow].NSFFeePastDue - lowestValueInNsfFee);
                            outputGrid[actionFeeRow].TotalPastDue = Round.RoundOffAmount(outputGrid[actionFeeRow].TotalPastDue - lowestValueInNsfFee);

                            for (int i = actionFeeRow; i <= currentOutputRow; i++)
                            {
                                outputGrid[i].CumulativeNSFFeePastDue = (outputGrid[i].CumulativeNSFFeePastDue < lowestValueInNsfFee ? 0 : outputGrid[i].CumulativeNSFFeePastDue - lowestValueInNsfFee);
                                outputGrid[i].CumulativeTotalPastDue = (outputGrid[i].CumulativeTotalPastDue < lowestValueInNsfFee ? 0 : outputGrid[i].CumulativeTotalPastDue - lowestValueInNsfFee);
                            }
                            paymentAmount = Round.RoundOffAmount(paymentAmount - lowestValueInNsfFee);
                        }

                        #endregion

                        #region Late fee payment

                        double lowestValueInLateFee = (outputGrid[actionFeeRow].LateFeePastDue <= paymentAmount) ? outputGrid[actionFeeRow].LateFeePastDue : paymentAmount;
                        if (lowestValueInLateFee > 0)
                        {
                            outputGrid[currentOutputRow].LateFeePaid += lowestValueInLateFee;
                            outputGrid[actionFeeRow].LateFeePastDue = Round.RoundOffAmount(outputGrid[actionFeeRow].LateFeePastDue - lowestValueInLateFee);
                            outputGrid[actionFeeRow].TotalPastDue = Round.RoundOffAmount(outputGrid[actionFeeRow].TotalPastDue - lowestValueInLateFee);

                            for (int i = actionFeeRow; i <= currentOutputRow; i++)
                            {
                                outputGrid[i].CumulativeLateFeePastDue = (outputGrid[i].CumulativeLateFeePastDue < lowestValueInLateFee ? 0 : outputGrid[i].CumulativeLateFeePastDue - lowestValueInLateFee);
                                outputGrid[i].CumulativeTotalPastDue = (outputGrid[i].CumulativeTotalPastDue < lowestValueInLateFee ? 0 : outputGrid[i].CumulativeTotalPastDue - lowestValueInLateFee);
                            }
                            paymentAmount = Round.RoundOffAmount(paymentAmount - lowestValueInLateFee);
                        }

                        #endregion
                    }
                    else
                    {
                        #region Late fee payment

                        double lowestValueInLateFee = (outputGrid[actionFeeRow].LateFeePastDue <= paymentAmount) ? outputGrid[actionFeeRow].LateFeePastDue : paymentAmount;
                        if (lowestValueInLateFee > 0)
                        {
                            outputGrid[currentOutputRow].LateFeePaid += lowestValueInLateFee;
                            outputGrid[actionFeeRow].LateFeePastDue = Round.RoundOffAmount(outputGrid[actionFeeRow].LateFeePastDue - lowestValueInLateFee);
                            outputGrid[actionFeeRow].TotalPastDue = Round.RoundOffAmount(outputGrid[actionFeeRow].TotalPastDue - lowestValueInLateFee);

                            for (int i = actionFeeRow; i <= currentOutputRow; i++)
                            {
                                outputGrid[i].CumulativeLateFeePastDue = (outputGrid[i].CumulativeLateFeePastDue < lowestValueInLateFee ? 0 : outputGrid[i].CumulativeLateFeePastDue - lowestValueInLateFee);
                                outputGrid[i].CumulativeTotalPastDue = (outputGrid[i].CumulativeTotalPastDue < lowestValueInLateFee ? 0 : outputGrid[i].CumulativeTotalPastDue - lowestValueInLateFee);
                            }
                            paymentAmount = Round.RoundOffAmount(paymentAmount - lowestValueInLateFee);
                        }

                        #endregion

                        #region NSF fee payment

                        double lowestValueInNsfFee = (outputGrid[actionFeeRow].NSFFeePastDue <= paymentAmount) ? outputGrid[actionFeeRow].NSFFeePastDue : paymentAmount;
                        if (lowestValueInNsfFee > 0)
                        {
                            outputGrid[currentOutputRow].NSFFeePaid += lowestValueInNsfFee;
                            outputGrid[actionFeeRow].NSFFeePastDue = Round.RoundOffAmount(outputGrid[actionFeeRow].NSFFeePastDue - lowestValueInNsfFee);
                            outputGrid[actionFeeRow].TotalPastDue = Round.RoundOffAmount(outputGrid[actionFeeRow].TotalPastDue - lowestValueInNsfFee);

                            for (int i = actionFeeRow; i <= currentOutputRow; i++)
                            {
                                outputGrid[i].CumulativeNSFFeePastDue = (outputGrid[i].CumulativeNSFFeePastDue < lowestValueInNsfFee ? 0 : outputGrid[i].CumulativeNSFFeePastDue - lowestValueInNsfFee);
                                outputGrid[i].CumulativeTotalPastDue = (outputGrid[i].CumulativeTotalPastDue < lowestValueInNsfFee ? 0 : outputGrid[i].CumulativeTotalPastDue - lowestValueInNsfFee);
                            }
                            paymentAmount = Round.RoundOffAmount(paymentAmount - lowestValueInNsfFee);
                        }

                        #endregion
                    }
                    actionFeeRow++;
                }
                sbTracing.AppendLine("Exist:From class ActualPaymentAllocation and method name AllocateRemainingFundsToNSFAndLateFee(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int currentRow, int currentOutputRow, bool isAdditionalPaymentAllocation, ref double paymentAmount)");
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
        /// remainingServiceFee will be calculated to recalculate the output grid.
        /// </summary>
        /// <param name="outputGrid"></param>
        /// <param name="scheduleInput"></param>
        /// <param name="inputGridRow"></param>
        /// <param name="outputGridRow"></param>
        /// <param name="paymentAmount"></param>
        /// <param name="remainingPrincipal"></param>
        /// <param name="remainingServiceFee"></param>
        private static void AllocateRemainingFunds(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int inputGridRow, int outputGridRow, double paymentAmount, ref double remainingPrincipal, ref double remainingServiceFee)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name AllocateRemainingFunds(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int inputGridRow, int outputGridRow, double paymentAmount, ref double remainingPrincipal, ref double remainingServiceFee).");
                sbTracing.AppendLine("Parameter values are : inputGridRow = " + inputGridRow + ", outputGridRow = " + outputGridRow + ", paymentAmount = " + paymentAmount + ", remainingPrincipal = " + remainingPrincipal + ", remainingServiceFee = " + remainingServiceFee);

                double payablePrincipal = (remainingPrincipal < Round.RoundOffAmount(remainingPrincipal - scheduleInput.Residual) ? remainingPrincipal :
                                                                Round.RoundOffAmount(remainingPrincipal - scheduleInput.Residual));

                //Checks whether the priority of service fee is less than principal priority.
                if (scheduleInput.InputRecords[inputGridRow].ServiceFeePriority < scheduleInput.InputRecords[inputGridRow].PrincipalPriority)
                {
                    //Allocate funds to Service Fee amount
                    outputGrid[outputGridRow].ServiceFeePaid += (remainingServiceFee <= paymentAmount) ? remainingServiceFee : paymentAmount;
                    remainingServiceFee = (remainingServiceFee <= paymentAmount) ? 0 : Round.RoundOffAmount(remainingServiceFee - paymentAmount);
                    paymentAmount = (remainingServiceFee <= paymentAmount) ? Round.RoundOffAmount(paymentAmount - remainingServiceFee) : 0;

                    //Allocate funds to principal amount
                    outputGrid[outputGridRow].PrincipalPaid += (payablePrincipal <= paymentAmount) ? payablePrincipal : paymentAmount;
                    remainingPrincipal -= (payablePrincipal <= paymentAmount) ? payablePrincipal : paymentAmount;
                    remainingPrincipal = Round.RoundOffAmount(remainingPrincipal);
                }
                else
                {
                    //Allocate funds to principal amount
                    outputGrid[outputGridRow].PrincipalPaid += (payablePrincipal <= paymentAmount) ? payablePrincipal : paymentAmount;
                    remainingPrincipal -= (payablePrincipal <= paymentAmount) ? payablePrincipal : paymentAmount;
                    paymentAmount = (payablePrincipal <= paymentAmount) ? Round.RoundOffAmount(paymentAmount - payablePrincipal) : 0;

                    remainingPrincipal = Round.RoundOffAmount(remainingPrincipal);

                    //Allocate funds to Service Fee amount
                    outputGrid[outputGridRow].ServiceFeePaid += (remainingServiceFee <= paymentAmount) ? remainingServiceFee : paymentAmount;
                    remainingServiceFee = (remainingServiceFee <= paymentAmount) ? 0 : Round.RoundOffAmount(remainingServiceFee - paymentAmount);
                }

                sbTracing.AppendLine("Exist:From class ActualPaymentAllocation and method name AllocateRemainingFunds(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int inputGridRow, int outputGridRow, double paymentAmount, ref double remainingPrincipal, ref double remainingServiceFee)");
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

        #endregion

        #region Principal Only Payment

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
        /// <param name="managementFee"></param>
        /// <param name="maintenanceFee"></param>
        private static void PayPrincipalOnlyAmount(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, int scheduleInputGridRow, int outputGridRow,
                double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee, double managementFee, double maintenanceFee)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name PayPrincipalOnlyAmount(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, int scheduleInputGridRow, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee, double managementFee, double maintenanceFee).");
                sbTracing.AppendLine("parameter values are : additionalPaymentRow = " + additionalPaymentRow + ", scheduleInputGridRow = " + scheduleInputGridRow + ", outputGridRow = " + outputGridRow + ", defaultTotalAmountToPay = " + defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", isExtendingSchedule = " + isExtendingSchedule + ", originationFee = " + originationFee + ", sameDayFee = " + sameDayFee + ", managementFee = " + managementFee + ", maintenanceFee = " + maintenanceFee);

                double remainingPrincipal = isExtendingSchedule ? (outputGrid[outputGridRow - 1].BeginningPrincipal - outputGrid[outputGridRow - 1].PrincipalPaid) :
                                                                    outputGrid[outputGridRow].BeginningPrincipal;
                double remainingPrincipalServiceFee = isExtendingSchedule ? (outputGrid[outputGridRow - 1].BeginningServiceFee - outputGrid[outputGridRow - 1].ServiceFeePaid) :
                                                                    outputGrid[outputGridRow].BeginningServiceFee;

                double payablePrincipal = (remainingPrincipal < Round.RoundOffAmount(remainingPrincipal - scheduleInputs.Residual) ? remainingPrincipal :
                                                                Round.RoundOffAmount(remainingPrincipal - scheduleInputs.Residual));

                DateTime startDate;

                int row = outputGrid.Take(outputGridRow).ToList().FindLastIndex(o => o.Flags != (int)Constants.FlagValues.NSFFee &&
                                                                o.Flags != (int)Constants.FlagValues.LateFee && o.Flags != (int)Constants.FlagValues.ManagementFee &&
                                                                o.Flags != (int)Constants.FlagValues.MaintenanceFee);

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

                double periodicInterestRate;
                double interestAccrued;
                //This condition determines whether there is an interest rate in additional payment grid, for that row. If it is provided, that value will be used, otherwise
                //default value will be used for periodic interest calculations.
                if (string.IsNullOrEmpty(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate))
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayPrincipalOnlyAmount() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule,false);");
                    periodicInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule, false);

                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayPrincipalOnlyAmount() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule,false);");
                    interestAccrued = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule, false);
                }
                else
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayPrincipalOnlyAmount() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInputs.InputRecords[0].DateIn, true,isExtendingSchedule,false);");
                    periodicInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInputs.InputRecords[0].DateIn, true, isExtendingSchedule, false);

                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayPrincipalOnlyAmount() method. Calling method : PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInputs.InputRecords[0].DateIn, remainingPrincipal, true,isExtendingSchedule,false);");
                    interestAccrued = PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInputs.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule, false);
                }
                interestAccrued = Round.RoundOffAmount(interestAccrued);

                //This variable calculates the daily interest amount for the period.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayPrincipalOnlyAmount() method. Calling method : DailyInterestAmount.CalculateDailyInterestAmount(startDate, endDate, interestAccrued, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InterestDelay ,scheduleInputs ,isExtendingSchedule,false);");
                double dailyInterestAmount = DailyInterestAmount.CalculateDailyInterestAmount(startDate, endDate, interestAccrued, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InterestDelay, scheduleInputs, isExtendingSchedule, false);
                dailyInterestAmount = Round.RoundOffAmount(dailyInterestAmount);

                //Add the earned interest amount to the accrued value.
                interestAccrued += (outputGridRow == 0 ? scheduleInputs.EarnedInterest : 0);

                //It determines the total interest carry over to be paid.
                double interestCarryOver = interestAccrued + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].InterestCarryOver);

                double serviceFeeInterestAccrued = 0;
                double serviceFeeInterestCarryOver = 0;
                //Checks whether service fee interest is to be taken or not.
                if (scheduleInputs.ApplyServiceFeeInterest == 1 || scheduleInputs.ApplyServiceFeeInterest == 3)
                {
                    if (string.IsNullOrEmpty(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate))
                    {
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayPrincipalOnlyAmount() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, remainingPrincipalServiceFee, true, isExtendingSchedule,false);");
                        serviceFeeInterestAccrued = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, remainingPrincipalServiceFee, true, isExtendingSchedule, false);
                    }
                    else
                    {
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayPrincipalOnlyAmount() method. Calling method : PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInputs.InputRecords[0].DateIn, remainingPrincipalServiceFee, true, isExtendingSchedule,false);");
                        serviceFeeInterestAccrued = PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInputs.InputRecords[0].DateIn, remainingPrincipalServiceFee, true, isExtendingSchedule, false);
                    }
                    serviceFeeInterestAccrued = Round.RoundOffAmount(serviceFeeInterestAccrued);
                }
                //Add earned service fee interest value to the accrued value.
                serviceFeeInterestAccrued += (outputGridRow == 0 ? scheduleInputs.EarnedServiceFeeInterest : 0);
                //It determines the total interest carry over to be paid.
                serviceFeeInterestCarryOver = serviceFeeInterestAccrued + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].AccruedServiceFeeInterestCarryOver);

                //Checks whether the pervious event was a skipped payment and is not the first row of output grid.
                if (outputGridRow != 0 && outputGrid[outputGridRow - 1].Flags == (int)Constants.FlagValues.SkipPayment)
                {
                    //Add the interest payment amount of previous schedule to the calculated interest payment of current row, as previous interest amount was not paid due to skipped payment.
                    interestCarryOver += outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].InterestPayment;

                    //Add the service fee interest payment amount of previous schedule to the calculated service fee interest payment of current row, as previous service fee interest amount was not paid due to skipped payment.
                    serviceFeeInterestCarryOver += outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].ServiceFeeInterest;
                }

                #region Get management and maintenance fee of upper rows when payment is skipped

                if (scheduleInputs.ManagementFeeFrequency == (int)Constants.FeeFrequency.AllPayments)
                {
                    int outputRow = outputGridRow - 1;
                    while (outputRow >= 0)
                    {
                        if (outputGrid[outputRow].Flags == (int)Constants.FlagValues.SkipPayment)
                        {
                            managementFee += outputGrid[outputRow].ManagementFee;
                            break;
                        }
                        if (outputGrid[outputRow].Flags != (int)Constants.FlagValues.NSFFee && outputGrid[outputRow].Flags != (int)Constants.FlagValues.LateFee &&
                                    outputGrid[outputRow].Flags != (int)Constants.FlagValues.Discount && outputGrid[outputRow].Flags != (int)Constants.FlagValues.ManagementFee &&
                                    outputGrid[outputRow].Flags != (int)Constants.FlagValues.MaintenanceFee)
                        {
                            break;
                        }
                        outputRow--;
                    }
                }

                if (scheduleInputs.MaintenanceFeeFrequency == (int)Constants.FeeFrequency.AllPayments)
                {
                    int outputRow = outputGridRow - 1;
                    while (outputRow >= 0)
                    {
                        if (outputGrid[outputRow].Flags == (int)Constants.FlagValues.SkipPayment)
                        {
                            maintenanceFee += outputGrid[outputRow].MaintenanceFee;
                            break;
                        }
                        if (outputGrid[outputRow].Flags != (int)Constants.FlagValues.NSFFee && outputGrid[outputRow].Flags != (int)Constants.FlagValues.LateFee &&
                                    outputGrid[outputRow].Flags != (int)Constants.FlagValues.Discount && outputGrid[outputRow].Flags != (int)Constants.FlagValues.ManagementFee &&
                                    outputGrid[outputRow].Flags != (int)Constants.FlagValues.MaintenanceFee)
                        {
                            break;
                        }
                        outputRow--;
                    }
                }
                managementFee = Round.RoundOffAmount(managementFee);
                maintenanceFee = Round.RoundOffAmount(maintenanceFee);

                #endregion

                #region Get NSF and Late fee of upper rows when payment is skipped

                int lastSkippedRow = outputGridRow;
                for (int i = 0; i < outputGridRow; i++)
                {
                    if (outputGrid[i].Flags != (int)Constants.FlagValues.SkipPayment)
                    {
                        break;
                    }
                    lastSkippedRow = i;
                }

                #endregion

                //It calculates the remaining principal amount that will be paid after deducting the additional payment amount.
                double principalAmountToPay = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment > payablePrincipal ? payablePrincipal :
                                                            scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment;

                //This variable calculates the daily interest rate for the period.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayPrincipalOnlyAmount() method. Calling method : DailyInterestRate.CalculateDailyInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InterestDelay, scheduleInputs, isExtendingSchedule,false);");
                double dailyInterestRate = DailyInterestRate.CalculateDailyInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InterestDelay, scheduleInputs, isExtendingSchedule, false);

                #region Add row for Principal only payment

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
                            OriginationFee = (outputGridRow == 0 ? scheduleInputs.EarnedOriginationFee : 0),
                            MaintenanceFee = (outputGridRow == 0 ? scheduleInputs.EarnedMaintenanceFee : 0) + maintenanceFee,
                            ManagementFee = (outputGridRow == 0 ? scheduleInputs.EarnedManagementFee : 0) + managementFee,
                            SameDayFee = (outputGridRow == 0 ? scheduleInputs.EarnedSameDayFee : 0),
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
                            //Past due amount columns
                            PrincipalPastDue = 0,
                            InterestPastDue = 0,
                            ServiceFeePastDue = 0,
                            ServiceFeeInterestPastDue = 0,
                            OriginationFeePastDue = (outputGridRow == 0 ? scheduleInputs.EarnedOriginationFee : 0),
                            MaintenanceFeePastDue = (outputGridRow == 0 ? scheduleInputs.EarnedMaintenanceFee : 0) + maintenanceFee,
                            ManagementFeePastDue = (outputGridRow == 0 ? scheduleInputs.EarnedManagementFee : 0) + managementFee,
                            SameDayFeePastDue = (outputGridRow == 0 ? scheduleInputs.EarnedSameDayFee : 0),
                            NSFFeePastDue = ((outputGridRow == 0 || lastSkippedRow == outputGridRow - 1) ? scheduleInputs.EarnedNSFFee : 0),
                            LateFeePastDue = ((outputGridRow == 0 || lastSkippedRow == outputGridRow - 1) ? scheduleInputs.EarnedLateFee : 0),
                            BucketStatus = "OutStanding"
                        });

                outputGrid[outputGridRow].TotalPastDue = outputGrid[outputGridRow].PrincipalPastDue + outputGrid[outputGridRow].InterestPastDue +
                                                            outputGrid[outputGridRow].ServiceFeePastDue + outputGrid[outputGridRow].ServiceFeeInterestPastDue +
                                                            outputGrid[outputGridRow].OriginationFeePastDue + outputGrid[outputGridRow].SameDayFeePastDue +
                                                            outputGrid[outputGridRow].MaintenanceFeePastDue + outputGrid[outputGridRow].ManagementFeePastDue +
                                                            outputGrid[outputGridRow].NSFFeePastDue + outputGrid[outputGridRow].LateFeePastDue;

                outputGrid[outputGridRow].CumulativePrincipalPastDue = outputGrid[outputGridRow].PrincipalPastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePrincipalPastDue);
                outputGrid[outputGridRow].CumulativeInterestPastDue = outputGrid[outputGridRow].InterestPastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeInterestPastDue);
                outputGrid[outputGridRow].CumulativeServiceFeePastDue = outputGrid[outputGridRow].ServiceFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeePastDue);
                outputGrid[outputGridRow].CumulativeServiceFeeInterestPastDue = outputGrid[outputGridRow].ServiceFeeInterestPastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeInterestPastDue);
                outputGrid[outputGridRow].CumulativeOriginationFeePastDue = outputGrid[outputGridRow].OriginationFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeOriginationFeePastDue);
                outputGrid[outputGridRow].CumulativeMaintenanceFeePastDue = outputGrid[outputGridRow].MaintenanceFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeMaintenanceFeePastDue);
                outputGrid[outputGridRow].CumulativeManagementFeePastDue = outputGrid[outputGridRow].ManagementFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeManagementFeePastDue);
                outputGrid[outputGridRow].CumulativeSameDayFeePastDue = outputGrid[outputGridRow].SameDayFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeSameDayFeePastDue);
                outputGrid[outputGridRow].CumulativeNSFFeePastDue = outputGrid[outputGridRow].NSFFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeNSFFeePastDue);
                outputGrid[outputGridRow].CumulativeLateFeePastDue = outputGrid[outputGridRow].LateFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeLateFeePastDue);
                outputGrid[outputGridRow].CumulativeTotalPastDue = outputGrid[outputGridRow].TotalPastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeTotalPastDue);

                #endregion

                //Determine the additional payment value that is being paid in the principal only payment event.
                double additionalPayment = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment;
                int previousOutStandingBucketNumber = 0;

                #region Pay principal amount

                //This loop pay all the past due principal amount.
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
                    double deductedPrincipal = ((outputGrid[bucketNumber].PrincipalPastDue > payablePrincipal ? payablePrincipal : outputGrid[bucketNumber].PrincipalPastDue) - additionalPayment) <= 0
                                                    ? (outputGrid[bucketNumber].PrincipalPastDue > payablePrincipal ? payablePrincipal : outputGrid[bucketNumber].PrincipalPastDue)
                                                    : additionalPayment;
                    payablePrincipal = Round.RoundOffAmount(payablePrincipal - deductedPrincipal);
                    additionalPayment -= deductedPrincipal;

                    outputGrid[bucketNumber].PrincipalPastDue -= deductedPrincipal;
                    outputGrid[bucketNumber].TotalPastDue -= deductedPrincipal;

                    if (isExtendingSchedule)
                    {
                        int lastRowIndex = outputGrid.FindLastIndex(o => o.DueDate <= scheduleInputs.InputRecords[scheduleInputs.InputRecords.Count - 1].DateIn);
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

                    if (payablePrincipal <= 0)
                    {
                        for (int i = 0; i < outputGridRow; i++)
                        {
                            outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - outputGrid[i].PrincipalPastDue);
                            outputGrid[i].PrincipalPastDue = 0;
                            outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue - outputGrid[i].CumulativePrincipalPastDue);
                            outputGrid[i].CumulativePrincipalPastDue = 0;
                            if (outputGrid[i].CumulativeTotalPastDue == 0)
                            {
                                outputGrid[i].BucketStatus = "Satisfied";
                            }
                        }
                    }

                    //Checks whether the cumulative total past due amount is 0. If it is, set the bucket status to "Satisfied".
                    if (outputGrid[bucketNumber].CumulativeTotalPastDue == 0)
                    {
                        outputGrid[bucketNumber].BucketStatus = "Satisfied";
                    }
                    previousOutStandingBucketNumber = bucketNumber;
                }

                #endregion

                remainingPrincipal -= principalAmountToPay;

                //Recalculates the output grid values for the remaining scheduled dates if the aditional payment is not after last scheduled payment date.
                if (!isExtendingSchedule)
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayPrincipalOnlyAmount() method. Calling method : ReCalculateOutputGridValues(remainingPrincipal, remainingprincipalServiceFee, outputGrid, scheduleInputs, outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);");
                    ReCalculateOutputGridValues(remainingPrincipal, remainingPrincipalServiceFee, outputGrid, scheduleInputs, outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);
                }

                //Cumulative amount past due is added in current row
                if (outputGrid.Count > outputGridRow + 1)
                {
                    outputGrid[outputGridRow + 1].CumulativePrincipalPastDue = outputGrid[outputGridRow].CumulativePrincipalPastDue + outputGrid[outputGridRow + 1].PrincipalPayment;
                    outputGrid[outputGridRow + 1].CumulativeInterestPastDue = outputGrid[outputGridRow].CumulativeInterestPastDue + outputGrid[outputGridRow + 1].InterestPayment;
                    outputGrid[outputGridRow + 1].CumulativeServiceFeePastDue = outputGrid[outputGridRow].CumulativeServiceFeePastDue + outputGrid[outputGridRow + 1].ServiceFee;
                    outputGrid[outputGridRow + 1].CumulativeServiceFeeInterestPastDue = outputGrid[outputGridRow].CumulativeServiceFeeInterestPastDue + outputGrid[outputGridRow + 1].ServiceFeeInterest;
                    outputGrid[outputGridRow + 1].CumulativeOriginationFeePastDue = outputGrid[outputGridRow].CumulativeOriginationFeePastDue + outputGrid[outputGridRow + 1].OriginationFee;
                    outputGrid[outputGridRow + 1].CumulativeMaintenanceFeePastDue = outputGrid[outputGridRow].CumulativeMaintenanceFeePastDue + outputGrid[outputGridRow + 1].MaintenanceFee;
                    outputGrid[outputGridRow + 1].CumulativeManagementFeePastDue = outputGrid[outputGridRow].CumulativeManagementFeePastDue + outputGrid[outputGridRow + 1].ManagementFee;
                    outputGrid[outputGridRow + 1].CumulativeSameDayFeePastDue = outputGrid[outputGridRow].CumulativeSameDayFeePastDue + outputGrid[outputGridRow + 1].SameDayFee;
                    outputGrid[outputGridRow + 1].CumulativeNSFFeePastDue = outputGrid[outputGridRow].CumulativeNSFFeePastDue;
                    outputGrid[outputGridRow + 1].CumulativeLateFeePastDue = outputGrid[outputGridRow].CumulativeLateFeePastDue;
                    outputGrid[outputGridRow + 1].CumulativeTotalPastDue = outputGrid[outputGridRow].CumulativeTotalPastDue + outputGrid[outputGridRow + 1].TotalPayment;
                }

                sbTracing.AppendLine("Exist:From class ActualPaymentAllocation and method name PayPrincipalOnlyAmount(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, int scheduleInputGridRow, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee, double managementFee, double maintenanceFee)");
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

        #region Additional Payment

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
        /// <param name="managementFee"></param>
        /// <param name="maintenanceFee"></param>
        private static void PayAdditionalPayment(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, int scheduleInputGridRow, int currentOutputGridRow,
                double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee, double managementFee, double maintenanceFee)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name PayAdditionalPayment(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, int scheduleInputGridRow, int currentOutputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee, double managementFee, double maintenanceFee).");
                sbTracing.AppendLine("parameter values are : additionalPaymentRow = " + additionalPaymentRow + ", scheduleInputGridRow = " + scheduleInputGridRow + ", currentOutputGridRow = " + currentOutputGridRow + ", defaultTotalAmountToPay = " + defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", isExtendingSchedule = " + isExtendingSchedule + ", originationFee = " + originationFee + ", sameDayFee = " + sameDayFee + ", managementFee = " + managementFee + ", maintenanceFee = " + maintenanceFee);

                double actualOrginationFee = originationFee;
                double actualSameDayFee = sameDayFee;

                double remainingPrincipal = isExtendingSchedule ? (outputGrid[currentOutputGridRow - 1].BeginningPrincipal - outputGrid[currentOutputGridRow - 1].PrincipalPaid) :
                                                                            outputGrid[currentOutputGridRow].BeginningPrincipal;
                double remainingPrincipalServiceFee = isExtendingSchedule ? (outputGrid[currentOutputGridRow - 1].BeginningServiceFee - outputGrid[currentOutputGridRow - 1].ServiceFeePaid) :
                                                                outputGrid[currentOutputGridRow].BeginningServiceFee;

                double remainingPayablePrincipal = (remainingPrincipal < Round.RoundOffAmount(remainingPrincipal - scheduleInputs.Residual) ? remainingPrincipal :
                                                                Round.RoundOffAmount(remainingPrincipal - scheduleInputs.Residual));

                DateTime startDate;
                int row = outputGrid.Take(currentOutputGridRow).ToList().FindLastIndex(o => o.Flags != (int)Constants.FlagValues.NSFFee &&
                                                                o.Flags != (int)Constants.FlagValues.LateFee && o.Flags != (int)Constants.FlagValues.ManagementFee &&
                                                                o.Flags != (int)Constants.FlagValues.MaintenanceFee);

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

                double periodicInterestRate;
                double interestAccrued;
                //This condition determines whether there is an interest rate in additional payment grid, for that row. If it is provided, that value will be used, otherwise
                //default value will be used for periodic interest calculations.
                if (string.IsNullOrEmpty(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate))
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayAdditionalPayment() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, remainingPrincipal, isExtendingSchedule,false);");
                    periodicInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule, false);

                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayAdditionalPayment() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, remainingPrincipal, isExtendingSchedule,false);");
                    interestAccrued = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule, false);
                }
                else
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayAdditionalPayment() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInputs.InputRecords[0].DateIn, isExtendingSchedule,false);");
                    periodicInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInputs.InputRecords[0].DateIn, true, isExtendingSchedule, false);

                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayAdditionalPayment() method. Calling method : PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInputs.InputRecords[0].DateIn, remainingPrincipal, isExtendingSchedule,false);");
                    interestAccrued = PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInputs.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule, false);
                }
                interestAccrued = Round.RoundOffAmount(interestAccrued);

                //This variable calculates the daily interest amount for the period.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayAdditionalPayment() method. Calling method : DailyInterestAmount.CalculateDailyInterestAmount(startDate, endDate, interestAccrued, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InterestDelay, scheduleInputs, isExtendingSchedule,false);");
                double dailyInterestAmount = DailyInterestAmount.CalculateDailyInterestAmount(startDate, endDate, interestAccrued, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InterestDelay, scheduleInputs, isExtendingSchedule, false);
                dailyInterestAmount = Round.RoundOffAmount(dailyInterestAmount);

                //Add the earned interest amount to the accrued value.
                interestAccrued += (currentOutputGridRow == 0 ? scheduleInputs.EarnedInterest : 0);

                //It determines the total interest carry over to be paid.
                double interestCarryOver = interestAccrued + (currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].InterestCarryOver);

                double serviceFeeInterestAccrued = 0;
                double serviceFeeInterestCarryOver = 0;
                //Checks whether service fee interest is to be taken or not.
                if (scheduleInputs.ApplyServiceFeeInterest == 1 || scheduleInputs.ApplyServiceFeeInterest == 3)
                {
                    if (string.IsNullOrEmpty(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate))
                    {
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayAdditionalPayment() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, remainingPrincipalServiceFee, isExtendingSchedule,false);");
                        serviceFeeInterestAccrued = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, remainingPrincipalServiceFee, true, isExtendingSchedule, false);
                    }
                    else
                    {
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayAdditionalPayment() method. Calling method : PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInputs.InputRecords[0].DateIn, remainingPrincipalServiceFee, true, isExtendingSchedule,false);");
                        serviceFeeInterestAccrued = PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInputs.InputRecords[0].DateIn, remainingPrincipalServiceFee, true, isExtendingSchedule, false);
                    }
                    serviceFeeInterestAccrued = Round.RoundOffAmount(serviceFeeInterestAccrued);
                }
                //Add earned service fee interest value to the accrued value.
                serviceFeeInterestAccrued += (currentOutputGridRow == 0 ? scheduleInputs.EarnedServiceFeeInterest : 0);
                //It determines the total interest carry over to be paid.
                serviceFeeInterestCarryOver = serviceFeeInterestAccrued + (currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].AccruedServiceFeeInterestCarryOver);

                //Checks whether the pervious event was a skipped payment and is not the first row of output grid.
                if (outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].Flags == (int)Constants.FlagValues.SkipPayment)
                {
                    interestCarryOver += (currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].InterestPayment);
                    serviceFeeInterestCarryOver += (currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].ServiceFeeInterest);
                }

                #region Get management and maintenance fee of upper rows when payment is skipped

                if (scheduleInputs.ManagementFeeFrequency == (int)Constants.FeeFrequency.AllPayments)
                {
                    int outputRow = currentOutputGridRow - 1;
                    while (outputRow >= 0)
                    {
                        if (outputGrid[outputRow].Flags == (int)Constants.FlagValues.SkipPayment)
                        {
                            managementFee += outputGrid[outputRow].ManagementFee;
                            break;
                        }
                        if (outputGrid[outputRow].Flags != (int)Constants.FlagValues.NSFFee && outputGrid[outputRow].Flags != (int)Constants.FlagValues.LateFee &&
                                    outputGrid[outputRow].Flags != (int)Constants.FlagValues.Discount && outputGrid[outputRow].Flags != (int)Constants.FlagValues.ManagementFee &&
                                    outputGrid[outputRow].Flags != (int)Constants.FlagValues.MaintenanceFee)
                        {
                            break;
                        }
                        outputRow--;
                    }
                }

                if (scheduleInputs.MaintenanceFeeFrequency == (int)Constants.FeeFrequency.AllPayments)
                {
                    int outputRow = currentOutputGridRow - 1;
                    while (outputRow >= 0)
                    {
                        if (outputGrid[outputRow].Flags == (int)Constants.FlagValues.SkipPayment)
                        {
                            maintenanceFee += outputGrid[outputRow].MaintenanceFee;
                            break;
                        }
                        if (outputGrid[outputRow].Flags != (int)Constants.FlagValues.NSFFee && outputGrid[outputRow].Flags != (int)Constants.FlagValues.LateFee &&
                                    outputGrid[outputRow].Flags != (int)Constants.FlagValues.Discount && outputGrid[outputRow].Flags != (int)Constants.FlagValues.ManagementFee &&
                                    outputGrid[outputRow].Flags != (int)Constants.FlagValues.MaintenanceFee)
                        {
                            break;
                        }
                        outputRow--;
                    }
                }
                managementFee = Round.RoundOffAmount(managementFee);
                maintenanceFee = Round.RoundOffAmount(maintenanceFee);

                #endregion

                #region Get NSF and Late fee of upper rows when payment is skipped

                int lastSkippedRow = currentOutputGridRow;
                for (int i = 0; i < currentOutputGridRow; i++)
                {
                    if (outputGrid[i].Flags != (int)Constants.FlagValues.SkipPayment)
                    {
                        break;
                    }
                    lastSkippedRow = i;
                }

                #endregion

                //Determine the origination fee and same day fee to be paid, which was not paid due to skipping the first repayment schedule.
                if (endDate > scheduleInputs.InputRecords[1].DateIn)
                {
                    if (scheduleInputs.InputRecords[1].Flags == (int)Constants.FlagValues.SkipPayment)
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
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayAdditionalPayment() method. Calling method : DailyInterestRate.CalculateDailyInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InterestDelay, scheduleInputs, isExtendingSchedule, false);");
                double dailyInterestRate = DailyInterestRate.CalculateDailyInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InterestDelay, scheduleInputs, isExtendingSchedule, false);

                #region Add row for additional payment
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
                            TotalPayment = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment,
                            AccruedServiceFeeInterest = serviceFeeInterestAccrued,
                            AccruedServiceFeeInterestCarryOver = serviceFeeInterestCarryOver,
                            ServiceFee = 0,
                            ServiceFeeInterest = 0,
                            ServiceFeeTotal = 0,
                            OriginationFee = (currentOutputGridRow == 0 ? scheduleInputs.EarnedOriginationFee : 0),
                            MaintenanceFee = (currentOutputGridRow == 0 ? scheduleInputs.EarnedMaintenanceFee : 0) + maintenanceFee,
                            ManagementFee = (currentOutputGridRow == 0 ? scheduleInputs.EarnedManagementFee : 0) + managementFee,
                            SameDayFee = (currentOutputGridRow == 0 ? scheduleInputs.EarnedSameDayFee : 0),
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
                            //Past due amount columns
                            PrincipalPastDue = 0,
                            InterestPastDue = 0,
                            ServiceFeePastDue = 0,
                            ServiceFeeInterestPastDue = 0,
                            OriginationFeePastDue = 0,
                            MaintenanceFeePastDue = (currentOutputGridRow == 0 ? scheduleInputs.EarnedMaintenanceFee : 0) + maintenanceFee,
                            ManagementFeePastDue = (currentOutputGridRow == 0 ? scheduleInputs.EarnedManagementFee : 0) + managementFee,
                            SameDayFeePastDue = 0,
                            NSFFeePastDue = ((currentOutputGridRow == 0 || lastSkippedRow == currentOutputGridRow - 1) ? scheduleInputs.EarnedNSFFee : 0),
                            LateFeePastDue = ((currentOutputGridRow == 0 || lastSkippedRow == currentOutputGridRow - 1) ? scheduleInputs.EarnedLateFee : 0),
                            BucketStatus = "OutStanding"
                        });

                outputGrid[currentOutputGridRow].TotalPastDue = outputGrid[currentOutputGridRow].PrincipalPastDue + outputGrid[currentOutputGridRow].InterestPastDue +
                                                            outputGrid[currentOutputGridRow].ServiceFeePastDue + outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue +
                                                            outputGrid[currentOutputGridRow].OriginationFeePastDue + outputGrid[currentOutputGridRow].SameDayFeePastDue +
                                                            outputGrid[currentOutputGridRow].MaintenanceFeePastDue + outputGrid[currentOutputGridRow].ManagementFeePastDue +
                                                            outputGrid[currentOutputGridRow].NSFFeePastDue + outputGrid[currentOutputGridRow].LateFeePastDue;

                outputGrid[currentOutputGridRow].CumulativePrincipalPastDue = outputGrid[currentOutputGridRow].PrincipalPastDue + (currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativePrincipalPastDue);
                outputGrid[currentOutputGridRow].CumulativeInterestPastDue = outputGrid[currentOutputGridRow].InterestPastDue + (currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeInterestPastDue);
                outputGrid[currentOutputGridRow].CumulativeServiceFeePastDue = outputGrid[currentOutputGridRow].ServiceFeePastDue + (currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeServiceFeePastDue);
                outputGrid[currentOutputGridRow].CumulativeServiceFeeInterestPastDue = outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue + (currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeServiceFeeInterestPastDue);
                outputGrid[currentOutputGridRow].CumulativeOriginationFeePastDue = outputGrid[currentOutputGridRow].OriginationFeePastDue + (currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeOriginationFeePastDue);
                outputGrid[currentOutputGridRow].CumulativeMaintenanceFeePastDue = outputGrid[currentOutputGridRow].MaintenanceFeePastDue + (currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeMaintenanceFeePastDue);
                outputGrid[currentOutputGridRow].CumulativeManagementFeePastDue = outputGrid[currentOutputGridRow].ManagementFeePastDue + (currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeManagementFeePastDue);
                outputGrid[currentOutputGridRow].CumulativeSameDayFeePastDue = outputGrid[currentOutputGridRow].SameDayFeePastDue + (currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeSameDayFeePastDue);
                outputGrid[currentOutputGridRow].CumulativeNSFFeePastDue = outputGrid[currentOutputGridRow].NSFFeePastDue + (currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeNSFFeePastDue);
                outputGrid[currentOutputGridRow].CumulativeLateFeePastDue = outputGrid[currentOutputGridRow].LateFeePastDue + (currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeLateFeePastDue);
                outputGrid[currentOutputGridRow].CumulativeTotalPastDue = outputGrid[currentOutputGridRow].TotalPastDue + (currentOutputGridRow == 0 ? 0 : outputGrid[currentOutputGridRow - 1].CumulativeTotalPastDue);

                #endregion

                int previousOutStandingBucketNumber = 0;
                //Determine the payment amount for the current additional payment bucket which will be distributed among all the components.
                double paymentAmount = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment;

                int lastRowIndex = outputGrid.FindLastIndex(o => o.DueDate <= scheduleInputs.InputRecords[scheduleInputs.InputRecords.Count - 1].DateIn);

                bool isPrincipalEqual = false;
                bool isServiceFeeEqual = false;
                double beginningServiceFee = outputGrid[currentOutputGridRow].BeginningServiceFee;

                #region Amount allocation in outstanding Buckets before the current bucket

                while (previousOutStandingBucketNumber < currentOutputGridRow && paymentAmount > 0)
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayAdditionalPayment() method. Calling method : OldestOutStandingBucket(outputGrid, currentOutputGridRow, previousOutStandingBucketNumber, 'OutStanding Bucket');");
                    int bucketNumber = OldestOutStandingBucket(outputGrid, currentOutputGridRow, previousOutStandingBucketNumber, "OutStanding Bucket");

                    if (Round.RoundOffAmount(remainingPayablePrincipal) <= Round.RoundOffAmount(outputGrid[bucketNumber].PrincipalPastDue))
                    {
                        isPrincipalEqual = true;
                    }
                    if (Round.RoundOffAmount(beginningServiceFee) <= Round.RoundOffAmount(outputGrid[bucketNumber].ServiceFeePastDue))
                    {
                        isServiceFeeEqual = true;
                    }

                    //If oldest outstanding bucket is cuurent bucket, then break from the loop.
                    if (bucketNumber >= currentOutputGridRow)
                    {
                        break;
                    }

                    double payablePrincipal = (remainingPayablePrincipal <= outputGrid[bucketNumber].PrincipalPastDue ? remainingPayablePrincipal : outputGrid[bucketNumber].PrincipalPastDue);
                    double payableServiceFee = (beginningServiceFee <= outputGrid[bucketNumber].ServiceFeePastDue ? beginningServiceFee : outputGrid[bucketNumber].ServiceFeePastDue);

                    //Checks whether total past due amount of oldest outstanding bucket is less than the payment amount.
                    if (outputGrid[bucketNumber].TotalPastDue <= paymentAmount)
                    {
                        #region Amount greater than past due

                        //Paid amount is added in current payment row
                        outputGrid[currentOutputGridRow].PrincipalPaid += payablePrincipal;
                        outputGrid[currentOutputGridRow].InterestPaid += outputGrid[bucketNumber].InterestPastDue;
                        outputGrid[currentOutputGridRow].ServiceFeePaid += payableServiceFee;
                        outputGrid[currentOutputGridRow].ServiceFeeInterestPaid += outputGrid[bucketNumber].ServiceFeeInterestPastDue;
                        outputGrid[currentOutputGridRow].OriginationFeePaid += outputGrid[bucketNumber].OriginationFeePastDue;
                        outputGrid[currentOutputGridRow].MaintenanceFeePaid += outputGrid[bucketNumber].MaintenanceFeePastDue;
                        outputGrid[currentOutputGridRow].ManagementFeePaid += outputGrid[bucketNumber].ManagementFeePastDue;
                        outputGrid[currentOutputGridRow].SameDayFeePaid += outputGrid[bucketNumber].SameDayFeePastDue;
                        outputGrid[currentOutputGridRow].TotalPaid += (outputGrid[bucketNumber].TotalPastDue - outputGrid[bucketNumber].PrincipalPastDue +
                                                                        payablePrincipal - outputGrid[bucketNumber].ServiceFeePastDue + payableServiceFee -
                                                                         (outputGrid[bucketNumber].NSFFeePastDue + outputGrid[bucketNumber].LateFeePastDue));

                        paymentAmount = Round.RoundOffAmount(paymentAmount - outputGrid[bucketNumber].TotalPastDue + outputGrid[bucketNumber].PrincipalPastDue - payablePrincipal
                                            + outputGrid[bucketNumber].ServiceFeePastDue - payableServiceFee +
                                                (outputGrid[bucketNumber].NSFFeePastDue + outputGrid[bucketNumber].LateFeePastDue));

                        if (!isExtendingSchedule)
                        {
                            for (int i = bucketNumber + 1; i <= currentOutputGridRow; i++)
                            {
                                if (isPrincipalEqual)
                                {
                                    outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - outputGrid[i].PrincipalPastDue);
                                    outputGrid[i].PrincipalPastDue = 0;
                                }
                                if (isServiceFeeEqual)
                                {
                                    outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - outputGrid[i].ServiceFeePastDue);
                                    outputGrid[i].ServiceFeePastDue = 0;
                                }
                            }
                        }

                        if (isExtendingSchedule && (bucketNumber != lastRowIndex))
                        {
                            //Set all past due balances to 0, in the oldest outstanding row.
                            outputGrid[lastRowIndex].PrincipalPastDue -= payablePrincipal;
                            outputGrid[lastRowIndex].InterestPastDue -= outputGrid[bucketNumber].InterestPastDue;
                            outputGrid[lastRowIndex].ServiceFeePastDue -= payableServiceFee;
                            outputGrid[lastRowIndex].ServiceFeeInterestPastDue -= outputGrid[bucketNumber].ServiceFeeInterestPastDue;
                            outputGrid[lastRowIndex].TotalPastDue = outputGrid[lastRowIndex].TotalPastDue - (payablePrincipal +
                                        outputGrid[bucketNumber].InterestPastDue + payableServiceFee +
                                        outputGrid[bucketNumber].ServiceFeeInterestPastDue);
                        }

                        remainingPayablePrincipal = Round.RoundOffAmount(remainingPayablePrincipal - payablePrincipal);
                        beginningServiceFee = Round.RoundOffAmount(beginningServiceFee - payableServiceFee);

                        if (remainingPayablePrincipal <= 0)
                        {
                            for (int j = 0; j < currentOutputGridRow; j++)
                            {
                                outputGrid[j].TotalPastDue = Round.RoundOffAmount(outputGrid[j].TotalPastDue - outputGrid[j].PrincipalPastDue);
                                outputGrid[j].PrincipalPastDue = 0;
                                outputGrid[j].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[j].CumulativeTotalPastDue - outputGrid[j].CumulativePrincipalPastDue);
                                outputGrid[j].CumulativePrincipalPastDue = 0;
                            }
                        }

                        if (beginningServiceFee <= 0)
                        {
                            for (int j = 0; j < currentOutputGridRow; j++)
                            {
                                outputGrid[j].TotalPastDue = Round.RoundOffAmount(outputGrid[j].TotalPastDue - outputGrid[j].ServiceFeePastDue);
                                outputGrid[j].ServiceFeePastDue = 0;
                                outputGrid[j].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[j].CumulativeTotalPastDue - outputGrid[j].CumulativeServiceFeePastDue);
                                outputGrid[j].CumulativeServiceFeePastDue = 0;
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
                        outputGrid[bucketNumber].TotalPastDue = outputGrid[bucketNumber].NSFFeePastDue + outputGrid[bucketNumber].LateFeePastDue;

                        //Set all Cumulative past due balances of next row to 0.
                        for (int i = bucketNumber + 1; i <= currentOutputGridRow; i++)
                        {
                            outputGrid[i].CumulativePrincipalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativePrincipalPastDue - outputGrid[bucketNumber].CumulativePrincipalPastDue);
                            outputGrid[i].CumulativeInterestPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeInterestPastDue - outputGrid[bucketNumber].CumulativeInterestPastDue);
                            outputGrid[i].CumulativeServiceFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeServiceFeePastDue - outputGrid[bucketNumber].CumulativeServiceFeePastDue);
                            outputGrid[i].CumulativeServiceFeeInterestPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeServiceFeeInterestPastDue - outputGrid[bucketNumber].CumulativeServiceFeeInterestPastDue);
                            outputGrid[i].CumulativeOriginationFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeOriginationFeePastDue - outputGrid[bucketNumber].CumulativeOriginationFeePastDue);
                            outputGrid[i].CumulativeMaintenanceFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeMaintenanceFeePastDue - outputGrid[bucketNumber].CumulativeMaintenanceFeePastDue);
                            outputGrid[i].CumulativeManagementFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeManagementFeePastDue - outputGrid[bucketNumber].CumulativeManagementFeePastDue);
                            outputGrid[i].CumulativeSameDayFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeSameDayFeePastDue - outputGrid[bucketNumber].CumulativeSameDayFeePastDue);
                            outputGrid[i].CumulativeTotalPastDue -= (outputGrid[bucketNumber].CumulativePrincipalPastDue + outputGrid[bucketNumber].CumulativeInterestPastDue +
                                                        outputGrid[bucketNumber].CumulativeServiceFeePastDue + outputGrid[bucketNumber].CumulativeServiceFeeInterestPastDue +
                                                        outputGrid[bucketNumber].CumulativeOriginationFeePastDue + outputGrid[bucketNumber].CumulativeMaintenanceFeePastDue +
                                                        outputGrid[bucketNumber].CumulativeManagementFeePastDue + outputGrid[bucketNumber].CumulativeSameDayFeePastDue);
                            outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue);
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

                        #endregion
                    }
                    else
                    {
                        #region Amount less than past due

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
                                if (payablePrincipal <= paymentAmount)
                                {
                                    outputGrid[currentOutputGridRow].PrincipalPaid += payablePrincipal;
                                    paymentAmount -= payablePrincipal;
                                    if ((lastRowIndex != bucketNumber) && isExtendingSchedule)
                                    {
                                        outputGrid[lastRowIndex].PrincipalPastDue -= payablePrincipal;
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
                                    payablePrincipal = paymentAmount;
                                    paymentAmount = 0;
                                }

                                if (isPrincipalEqual && !isExtendingSchedule)
                                {
                                    for (int j = bucketNumber + 1; j <= currentOutputGridRow; j++)
                                    {
                                        if (outputGrid[j].Flags == (int)Constants.FlagValues.Payment)
                                        {
                                            outputGrid[j].PrincipalPastDue -= payablePrincipal;
                                        }
                                    }
                                }

                                remainingPayablePrincipal = Round.RoundOffAmount(remainingPayablePrincipal - payablePrincipal);
                                if (remainingPayablePrincipal <= 0)
                                {
                                    for (int j = 0; j < currentOutputGridRow; j++)
                                    {
                                        outputGrid[j].TotalPastDue = Round.RoundOffAmount(outputGrid[j].TotalPastDue - outputGrid[j].PrincipalPastDue);
                                        outputGrid[j].PrincipalPastDue = 0;
                                        outputGrid[j].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[j].CumulativeTotalPastDue - outputGrid[j].CumulativePrincipalPastDue);
                                        outputGrid[j].CumulativePrincipalPastDue = 0;
                                    }
                                }

                                if (paymentAmount == 0)
                                {
                                    break;
                                }
                            }

                            //Allocate funds to Service Fee amount
                            else if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].ServiceFeePriority == i)
                            {
                                double deductedAmount = (beginningServiceFee <= outputGrid[bucketNumber].ServiceFeePastDue) ? beginningServiceFee :
                                                                                        outputGrid[bucketNumber].ServiceFeePastDue;

                                if (deductedAmount <= paymentAmount)
                                {
                                    outputGrid[currentOutputGridRow].ServiceFeePaid += deductedAmount;
                                    paymentAmount -= deductedAmount;
                                    if ((lastRowIndex != bucketNumber) && isExtendingSchedule)
                                    {
                                        outputGrid[lastRowIndex].ServiceFeePastDue -= deductedAmount;
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
                                    deductedAmount = paymentAmount;
                                    paymentAmount = 0;
                                }

                                if (isServiceFeeEqual && !isExtendingSchedule)
                                {
                                    for (int j = bucketNumber + 1; j <= currentOutputGridRow; j++)
                                    {
                                        if (outputGrid[j].Flags == (int)Constants.FlagValues.Payment)
                                        {
                                            outputGrid[j].ServiceFeePastDue -= deductedAmount;
                                        }
                                    }
                                }

                                beginningServiceFee = Round.RoundOffAmount(beginningServiceFee - deductedAmount);
                                if (beginningServiceFee <= 0)
                                {
                                    for (int j = 0; j < currentOutputGridRow; j++)
                                    {
                                        outputGrid[j].TotalPastDue = Round.RoundOffAmount(outputGrid[j].TotalPastDue - outputGrid[j].ServiceFeePastDue);
                                        outputGrid[j].ServiceFeePastDue = 0;
                                        outputGrid[j].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[j].CumulativeTotalPastDue - outputGrid[j].CumulativeServiceFeePastDue);
                                        outputGrid[j].CumulativeServiceFeePastDue = 0;
                                    }
                                }

                                if (paymentAmount == 0)
                                {
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
                            paymentAmount = Round.RoundOffAmount(paymentAmount);
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

                        outputGrid[bucketNumber].BucketStatus = (outputGrid[bucketNumber].NSFFeePastDue + outputGrid[bucketNumber].LateFeePastDue ==
                                                                                                            outputGrid[bucketNumber].TotalPastDue) ? "" : "OutStanding";

                        #endregion
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
                    if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].ManagementFeePriority == i)
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
                    else if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].MaintenanceFeePriority == i)
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
                            outputGrid[currentOutputGridRow].InterestPayment = Round.RoundOffAmount(outputGrid[currentOutputGridRow].InterestCarryOver);
                            outputGrid[currentOutputGridRow].InterestCarryOver = 0;
                            paymentAmount -= outputGrid[currentOutputGridRow].InterestPayment;
                            outputGrid[currentOutputGridRow].InterestPaid += outputGrid[currentOutputGridRow].InterestPayment;
                        }
                        else
                        {
                            outputGrid[currentOutputGridRow].InterestPayment = Round.RoundOffAmount(paymentAmount);
                            outputGrid[currentOutputGridRow].InterestCarryOver -= outputGrid[currentOutputGridRow].InterestPayment;
                            outputGrid[currentOutputGridRow].InterestCarryOver = Round.RoundOffAmount(outputGrid[currentOutputGridRow].InterestCarryOver);
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
                            outputGrid[currentOutputGridRow].ServiceFeeInterest = Round.RoundOffAmount(outputGrid[currentOutputGridRow].AccruedServiceFeeInterestCarryOver);
                            outputGrid[currentOutputGridRow].AccruedServiceFeeInterestCarryOver = 0;
                            paymentAmount -= outputGrid[currentOutputGridRow].ServiceFeeInterest;
                            outputGrid[currentOutputGridRow].ServiceFeeInterestPaid += outputGrid[currentOutputGridRow].ServiceFeeInterest;
                        }
                        else
                        {
                            outputGrid[currentOutputGridRow].ServiceFeeInterest = Round.RoundOffAmount(paymentAmount);
                            outputGrid[currentOutputGridRow].AccruedServiceFeeInterestCarryOver -= outputGrid[currentOutputGridRow].ServiceFeeInterest;
                            outputGrid[currentOutputGridRow].AccruedServiceFeeInterestCarryOver = Round.RoundOffAmount(outputGrid[currentOutputGridRow].AccruedServiceFeeInterestCarryOver);
                            paymentAmount = 0;
                            outputGrid[currentOutputGridRow].ServiceFeeInterestPaid += outputGrid[currentOutputGridRow].ServiceFeeInterest;
                        }
                        outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue = (outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue - paymentAmount) <= 0 ? 0 : (outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue - paymentAmount);
                        outputGrid[currentOutputGridRow].CumulativeServiceFeeInterestPastDue = (outputGrid[currentOutputGridRow].CumulativeServiceFeeInterestPastDue - paymentAmount) <= 0 ? 0 : (outputGrid[currentOutputGridRow].CumulativeServiceFeeInterestPastDue - paymentAmount);
                    }

                    //Allocate funds to Origination Fee amount
                    else if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].OriginationFeePriority == i)
                    {
                        double remainingOriginationFee = ((originationFee - outputGrid[currentOutputGridRow].OriginationFeePaid) < 0 ? 0 : (originationFee - outputGrid[currentOutputGridRow].OriginationFeePaid));
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
                                if (currentOutputGridRow == 0)
                                {
                                    if (outputGrid[firstScheduleIndex].OriginationFee - scheduleInputs.EarnedOriginationFee > paymentAmount)
                                    {
                                        outputGrid[currentOutputGridRow].OriginationFee = paymentAmount + scheduleInputs.EarnedOriginationFee;
                                    }
                                    else
                                    {
                                        outputGrid[currentOutputGridRow].OriginationFee = outputGrid[firstScheduleIndex].OriginationFee;
                                    }
                                }
                                else
                                {
                                    outputGrid[currentOutputGridRow].OriginationFee = paymentAmount;
                                }
                                outputGrid[firstScheduleIndex].OriginationFee -= paymentAmount;
                                paymentAmount = 0;
                                break;
                            }
                        }
                        //Check whether there is an amount remains to pay due to skipping the first repayment schedule.
                        else if (remainingOriginationFee > 0)
                        {
                            if (remainingOriginationFee <= paymentAmount)
                            {
                                outputGrid[currentOutputGridRow].OriginationFee = remainingOriginationFee;
                                outputGrid[currentOutputGridRow].OriginationFeePaid += remainingOriginationFee;
                                paymentAmount -= remainingOriginationFee;
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
                        double remainingSameDayFee = ((sameDayFee - outputGrid[currentOutputGridRow].SameDayFeePaid) < 0 ? 0 : (sameDayFee - outputGrid[currentOutputGridRow].SameDayFeePaid));
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
                                if (currentOutputGridRow == 0)
                                {
                                    if (outputGrid[firstScheduleIndex].SameDayFee - scheduleInputs.EarnedSameDayFee > paymentAmount)
                                    {
                                        outputGrid[currentOutputGridRow].SameDayFee = paymentAmount + scheduleInputs.EarnedSameDayFee;
                                    }
                                    else
                                    {
                                        outputGrid[currentOutputGridRow].SameDayFee = outputGrid[firstScheduleIndex].SameDayFee;
                                    }
                                }
                                else
                                {
                                    outputGrid[currentOutputGridRow].SameDayFee = paymentAmount;
                                }
                                outputGrid[firstScheduleIndex].SameDayFee -= paymentAmount;
                                paymentAmount = 0;
                                break;
                            }
                        }
                        //Check whether there is an amount remains to pay due to skipping the first repayment schedule.
                        else if (remainingSameDayFee > 0)
                        {
                            if (remainingSameDayFee <= paymentAmount)
                            {
                                outputGrid[currentOutputGridRow].SameDayFee = remainingSameDayFee;
                                outputGrid[currentOutputGridRow].SameDayFeePaid += remainingSameDayFee;
                                paymentAmount -= remainingSameDayFee;
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
                    paymentAmount = Round.RoundOffAmount(paymentAmount);
                    if (paymentAmount <= 0)
                    {
                        break;
                    }
                }

                if (currentOutputGridRow == 0)
                {
                    outputGrid[currentOutputGridRow].OriginationFeePastDue = Round.RoundOffAmount(outputGrid[currentOutputGridRow].OriginationFee - outputGrid[currentOutputGridRow].OriginationFeePaid);
                    outputGrid[currentOutputGridRow].SameDayFeePastDue = Round.RoundOffAmount(outputGrid[currentOutputGridRow].SameDayFee - outputGrid[currentOutputGridRow].SameDayFeePaid);
                }

                outputGrid[currentOutputGridRow].InterestPastDue = Round.RoundOffAmount(outputGrid[currentOutputGridRow].InterestPastDue);
                outputGrid[currentOutputGridRow].InterestPaid = Round.RoundOffAmount(outputGrid[currentOutputGridRow].InterestPaid);
                outputGrid[currentOutputGridRow].CumulativeInterestPastDue = Round.RoundOffAmount(outputGrid[currentOutputGridRow].CumulativeInterestPastDue);
                outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue = Round.RoundOffAmount(outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue);
                outputGrid[currentOutputGridRow].ServiceFeeInterestPaid = Round.RoundOffAmount(outputGrid[currentOutputGridRow].ServiceFeeInterestPaid);
                outputGrid[currentOutputGridRow].CumulativeServiceFeeInterestPastDue = Round.RoundOffAmount(outputGrid[currentOutputGridRow].CumulativeServiceFeeInterestPastDue);
                outputGrid[currentOutputGridRow].ServiceFeeTotal = outputGrid[currentOutputGridRow].ServiceFeeInterest;

                #endregion

                //This condition checks whether there is any payment amount remains after pay down all the outstanding amount as well as current bucket.
                if (paymentAmount > 0)
                {
                    //Allocate the remaining amount to the action fees i.e. NSF fee and late fee.
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayAdditionalPayment() method. Calling method : AllocateRemainingFundsToNSFAndLateFee(outputGrid, scheduleInputs, additionalPaymentRow, currentOutputGridRow, true, ref paymentAmount);");
                    AllocateRemainingFundsToNSFAndLateFee(outputGrid, scheduleInputs, additionalPaymentRow, currentOutputGridRow, true, ref paymentAmount);
                }

                #region Pay extra payment

                remainingPrincipal = Round.RoundOffAmount(outputGrid[currentOutputGridRow].BeginningPrincipal - outputGrid[currentOutputGridRow].PrincipalPaid);
                remainingPrincipalServiceFee = Round.RoundOffAmount(outputGrid[currentOutputGridRow].BeginningServiceFee - outputGrid[currentOutputGridRow].ServiceFeePaid);

                //Check whether the priority of service fee is greater than the principal amount
                if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].ServiceFeePriority > scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].PrincipalPriority)
                {
                    //Allocate funds to principal amount
                    outputGrid[currentOutputGridRow].PrincipalPaid += (remainingPayablePrincipal <= paymentAmount) ? remainingPayablePrincipal : paymentAmount;
                    remainingPrincipal -= (remainingPayablePrincipal <= paymentAmount) ? remainingPayablePrincipal : paymentAmount;
                    remainingPrincipal = Round.RoundOffAmount(remainingPrincipal);
                    paymentAmount = (remainingPayablePrincipal <= paymentAmount) ? Round.RoundOffAmount(paymentAmount - remainingPayablePrincipal) : 0;

                    //Allocate funds to Service Fee amount
                    outputGrid[currentOutputGridRow].ServiceFeePaid += (remainingPrincipalServiceFee <= paymentAmount) ? remainingPrincipalServiceFee : paymentAmount;
                    remainingPrincipalServiceFee = (remainingPrincipalServiceFee <= paymentAmount) ? 0 : Round.RoundOffAmount(remainingPrincipalServiceFee - paymentAmount);
                }
                else
                {
                    //Allocate funds to Service Fee amount
                    outputGrid[currentOutputGridRow].ServiceFeePaid += (remainingPrincipalServiceFee <= paymentAmount) ? remainingPrincipalServiceFee : paymentAmount;
                    remainingPrincipalServiceFee = (remainingPrincipalServiceFee <= paymentAmount) ? 0 : Round.RoundOffAmount(remainingPrincipalServiceFee - paymentAmount);
                    paymentAmount = (remainingPrincipalServiceFee <= paymentAmount) ? Round.RoundOffAmount(paymentAmount - remainingPrincipalServiceFee) : 0;

                    //Allocate funds to Service Fee amount
                    outputGrid[currentOutputGridRow].PrincipalPaid += (remainingPayablePrincipal <= paymentAmount) ? remainingPayablePrincipal : paymentAmount;
                    remainingPrincipal -= (remainingPayablePrincipal <= paymentAmount) ? remainingPayablePrincipal : paymentAmount;
                    remainingPrincipal = Round.RoundOffAmount(remainingPrincipal);
                }

                #endregion

                //Recalculate values in output grid as per remaining loan amount and service fee when the event is not the part of extending the repayment schedule.
                if (!isExtendingSchedule)
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PayAdditionalPayment() method. Calling method : ReCalculateOutputGridValues(remainingPrincipal, remainingprincipalServiceFee, outputGrid, scheduleInputs, currentOutputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, actualOrginationFee, actualSameDayFee);");
                    ReCalculateOutputGridValues(remainingPrincipal, remainingPrincipalServiceFee, outputGrid, scheduleInputs, currentOutputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, actualOrginationFee, actualSameDayFee);
                }

                outputGrid[currentOutputGridRow].TotalPastDue = outputGrid[currentOutputGridRow].PrincipalPastDue + outputGrid[currentOutputGridRow].InterestPastDue +
                                                        outputGrid[currentOutputGridRow].ServiceFeePastDue + outputGrid[currentOutputGridRow].ServiceFeeInterestPastDue +
                                                        outputGrid[currentOutputGridRow].ManagementFeePastDue + outputGrid[currentOutputGridRow].MaintenanceFeePastDue +
                                                        outputGrid[currentOutputGridRow].NSFFeePastDue + outputGrid[currentOutputGridRow].LateFeePastDue +
                                                        outputGrid[currentOutputGridRow].OriginationFeePastDue + outputGrid[currentOutputGridRow].SameDayFeePastDue;
                outputGrid[currentOutputGridRow].CumulativeTotalPastDue = outputGrid[currentOutputGridRow == 0 ? 0 : currentOutputGridRow - 1].CumulativeTotalPastDue +
                                                                                    outputGrid[currentOutputGridRow].TotalPastDue;

                outputGrid[currentOutputGridRow].TotalPaid = outputGrid[currentOutputGridRow].PrincipalPaid + outputGrid[currentOutputGridRow].InterestPaid +
                                                        outputGrid[currentOutputGridRow].ServiceFeePaid + outputGrid[currentOutputGridRow].ServiceFeeInterestPaid +
                                                        outputGrid[currentOutputGridRow].OriginationFeePaid + outputGrid[currentOutputGridRow].MaintenanceFeePaid +
                                                        outputGrid[currentOutputGridRow].ManagementFeePaid + outputGrid[currentOutputGridRow].SameDayFeePaid +
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
                sbTracing.AppendLine("Exist:From class ActualPaymentAllocation and method name PayAdditionalPayment(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, int scheduleInputGridRow, int currentOutputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee, double managementFee, double maintenanceFee)");
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

        #region Skipped Payment

        /// <summary>
        /// This function skips the scheduled payment, and put all the paid amount to 0, as nothing is paid. Also the interest and service fee interest calculated will
        /// be carry forward to next schedule.
        /// </summary>
        /// <param name="outputGrid"></param>
        /// <param name="scheduleInput"></param>
        /// <param name="outputGridRow"></param>
        /// <param name="defaultTotalAmountToPay"></param>
        /// <param name="defaultTotalServiceFeePayable"></param>
        /// <param name="originationFee"></param>
        /// <param name="sameDayFee"></param>
        private static void SkippedPayment(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int outputGridRow, double defaultTotalAmountToPay,
                                            double defaultTotalServiceFeePayable, double originationFee, double sameDayFee)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name SkippedPayment(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double sameDayFee).");
                sbTracing.AppendLine("Parameter values are : outputGridRow = " + outputGridRow + ", defaultTotalAmountToPay = " + defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", originationFee = " + originationFee + ", sameDayFee = " + sameDayFee);

                //Set all paid amount to 0 in current row.
                outputGrid[outputGridRow].PrincipalPaid = 0;
                outputGrid[outputGridRow].InterestPaid = 0;
                outputGrid[outputGridRow].ServiceFeePaid = 0;
                outputGrid[outputGridRow].ServiceFeeInterestPaid = 0;
                outputGrid[outputGridRow].OriginationFeePaid = 0;
                outputGrid[outputGridRow].MaintenanceFeePaid = 0;
                outputGrid[outputGridRow].ManagementFeePaid = 0;
                outputGrid[outputGridRow].SameDayFeePaid = 0;
                outputGrid[outputGridRow].NSFFeePaid = 0;
                outputGrid[outputGridRow].LateFeePaid = 0;
                outputGrid[outputGridRow].TotalPaid = 0;

                //Set all past due balances to 0, in the current row.
                outputGrid[outputGridRow].PrincipalPastDue = 0;
                outputGrid[outputGridRow].InterestPastDue = 0;
                outputGrid[outputGridRow].ServiceFeePastDue = 0;
                outputGrid[outputGridRow].ServiceFeeInterestPastDue = 0;
                outputGrid[outputGridRow].OriginationFeePastDue = 0;
                outputGrid[outputGridRow].MaintenanceFeePastDue = 0;
                outputGrid[outputGridRow].ManagementFeePastDue = 0;
                outputGrid[outputGridRow].SameDayFeePastDue = 0;
                outputGrid[outputGridRow].NSFFeePastDue = 0;
                outputGrid[outputGridRow].LateFeePastDue = 0;
                outputGrid[outputGridRow].TotalPastDue = 0;

                //Set all Cumulative past due balances to 0, in the current row.
                outputGrid[outputGridRow].CumulativePrincipalPastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePrincipalPastDue);
                outputGrid[outputGridRow].CumulativeInterestPastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeInterestPastDue);
                outputGrid[outputGridRow].CumulativeServiceFeePastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeePastDue);
                outputGrid[outputGridRow].CumulativeServiceFeeInterestPastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeInterestPastDue);
                outputGrid[outputGridRow].CumulativeOriginationFeePastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeOriginationFeePastDue);
                outputGrid[outputGridRow].CumulativeMaintenanceFeePastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeMaintenanceFeePastDue);
                outputGrid[outputGridRow].CumulativeManagementFeePastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeManagementFeePastDue);
                outputGrid[outputGridRow].CumulativeSameDayFeePastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeSameDayFeePastDue);
                outputGrid[outputGridRow].CumulativeNSFFeePastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeNSFFeePastDue);
                outputGrid[outputGridRow].CumulativeLateFeePastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeLateFeePastDue);
                outputGrid[outputGridRow].CumulativeTotalPastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeTotalPastDue);
                outputGrid[outputGridRow].BucketStatus = "";

                //Set the cumulative NSF and Late fee past due amount columns of next row.
                if (outputGrid.Count >= outputGridRow + 1)
                {
                    outputGrid[outputGridRow + 1].CumulativeNSFFeePastDue = outputGrid[outputGridRow].CumulativeNSFFeePastDue;
                    outputGrid[outputGridRow + 1].CumulativeLateFeePastDue = outputGrid[outputGridRow].CumulativeLateFeePastDue;
                    outputGrid[outputGridRow + 1].CumulativeTotalPastDue = outputGrid[outputGridRow].CumulativeTotalPastDue;
                }

                //Recalculates the output grid values for the remaining scheduled dates.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and SkippedPayment() method. Calling method : ReCalculateOutputGridValues(outputGrid[outputGridRow].BeginningPrincipal, outputGrid[outputGridRow].BeginningServiceFee, outputGrid, scheduleInput, outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);");
                ReCalculateOutputGridValues(outputGrid[outputGridRow].BeginningPrincipal, outputGrid[outputGridRow].BeginningServiceFee, outputGrid,
                                                scheduleInput, outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);

                sbTracing.AppendLine("Exist:From class ActualPaymentAllocation and method name SkippedPayment(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double sameDayFee)");
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

        #region NSF and Late Fee

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
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name AddNSFAndLateFee(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double sameDayFee).");
                sbTracing.AppendLine("Parameter values are : additionalPaymentRow = " + additionalPaymentRow + ", outputGridRow = " + outputGridRow + ", defaultTotalAmountToPay = " + defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", originationFee = " + originationFee + ", sameDayFee = " + sameDayFee);

                //It determines the total interest carry over to be paid.
                double interestCarryOver = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].InterestCarryOver);
                double serviceFeeInterestCarryOver = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].AccruedServiceFeeInterestCarryOver);

                //Checks whether the previous row was the skipped payment event.
                if (outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].Flags == (int)Constants.FlagValues.SkipPayment)
                {
                    //Add the previous interest payable amount to carry over value, as previously due to skip payment it was not paid.
                    interestCarryOver += (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].InterestPayment);
                    //Add the previous service fee interest payable amount to carry over value, as previously due to skip payment it was not paid.
                    serviceFeeInterestCarryOver += (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].ServiceFeeInterest);
                }

                #region Get NSF and Late fee of upper rows when payment is skipped

                int lastSkippedRow = outputGridRow;
                for (int i = 0; i < outputGridRow; i++)
                {
                    if (outputGrid[i].Flags != (int)Constants.FlagValues.SkipPayment)
                    {
                        break;
                    }
                    lastSkippedRow = i;
                }

                #endregion

                #region Add row for NSF and Late fee

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
                            InterestAccrued = (outputGridRow == 0 ? scheduleInputs.EarnedInterest : 0),
                            InterestCarryOver = (outputGridRow == 0 ? scheduleInputs.EarnedInterest : 0) + interestCarryOver,
                            InterestPayment = 0,
                            PrincipalPayment = 0,
                            TotalPayment = 0,
                            AccruedServiceFeeInterest = (outputGridRow == 0 ? scheduleInputs.EarnedServiceFeeInterest : 0),
                            AccruedServiceFeeInterestCarryOver = (outputGridRow == 0 ? scheduleInputs.EarnedServiceFeeInterest : 0) + serviceFeeInterestCarryOver,
                            ServiceFee = 0,
                            ServiceFeeInterest = 0,
                            ServiceFeeTotal = 0,
                            OriginationFee = (outputGridRow == 0 ? scheduleInputs.EarnedOriginationFee : 0),
                            MaintenanceFee = (outputGridRow == 0 ? scheduleInputs.EarnedMaintenanceFee : 0),
                            ManagementFee = (outputGridRow == 0 ? scheduleInputs.EarnedManagementFee : 0),
                            SameDayFee = (outputGridRow == 0 ? scheduleInputs.EarnedSameDayFee : 0),
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
                            //Past due amount columns
                            PrincipalPastDue = 0,
                            InterestPastDue = 0,
                            ServiceFeePastDue = 0,
                            ServiceFeeInterestPastDue = 0,
                            OriginationFeePastDue = (outputGridRow == 0 ? scheduleInputs.EarnedOriginationFee : 0),
                            MaintenanceFeePastDue = (outputGridRow == 0 ? scheduleInputs.EarnedMaintenanceFee : 0),
                            ManagementFeePastDue = (outputGridRow == 0 ? scheduleInputs.EarnedManagementFee : 0),
                            SameDayFeePastDue = (outputGridRow == 0 ? scheduleInputs.EarnedSameDayFee : 0),
                            NSFFeePastDue = ((outputGridRow == 0 || lastSkippedRow == outputGridRow - 1) ? scheduleInputs.EarnedNSFFee : 0) +
                                                (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].Flags == (int)Constants.FlagValues.NSFFee ? scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment : 0),
                            LateFeePastDue = ((outputGridRow == 0 || lastSkippedRow == outputGridRow - 1) ? scheduleInputs.EarnedLateFee : 0) +
                                                (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].Flags == (int)Constants.FlagValues.LateFee ? scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment : 0),
                            BucketStatus = (outputGridRow == 0 && (scheduleInputs.EarnedMaintenanceFee + scheduleInputs.EarnedManagementFee > 0)) ? "OutStanding" : ""
                        });

                outputGrid[outputGridRow].TotalPastDue = outputGrid[outputGridRow].PrincipalPastDue + outputGrid[outputGridRow].InterestPastDue +
                                                            outputGrid[outputGridRow].ServiceFeePastDue + outputGrid[outputGridRow].ServiceFeeInterestPastDue +
                                                            outputGrid[outputGridRow].OriginationFeePastDue + outputGrid[outputGridRow].SameDayFeePastDue +
                                                            outputGrid[outputGridRow].MaintenanceFeePastDue + outputGrid[outputGridRow].ManagementFeePastDue +
                                                            outputGrid[outputGridRow].NSFFeePastDue + outputGrid[outputGridRow].LateFeePastDue;

                outputGrid[outputGridRow].CumulativePrincipalPastDue = outputGrid[outputGridRow].PrincipalPastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePrincipalPastDue);
                outputGrid[outputGridRow].CumulativeInterestPastDue = outputGrid[outputGridRow].InterestPastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeInterestPastDue);
                outputGrid[outputGridRow].CumulativeServiceFeePastDue = outputGrid[outputGridRow].ServiceFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeePastDue);
                outputGrid[outputGridRow].CumulativeServiceFeeInterestPastDue = outputGrid[outputGridRow].ServiceFeeInterestPastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeInterestPastDue);
                outputGrid[outputGridRow].CumulativeOriginationFeePastDue = outputGrid[outputGridRow].OriginationFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeOriginationFeePastDue);
                outputGrid[outputGridRow].CumulativeMaintenanceFeePastDue = outputGrid[outputGridRow].MaintenanceFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeMaintenanceFeePastDue);
                outputGrid[outputGridRow].CumulativeManagementFeePastDue = outputGrid[outputGridRow].ManagementFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeManagementFeePastDue);
                outputGrid[outputGridRow].CumulativeSameDayFeePastDue = outputGrid[outputGridRow].SameDayFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeSameDayFeePastDue);
                outputGrid[outputGridRow].CumulativeNSFFeePastDue = outputGrid[outputGridRow].NSFFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeNSFFeePastDue);
                outputGrid[outputGridRow].CumulativeLateFeePastDue = outputGrid[outputGridRow].LateFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeLateFeePastDue);
                outputGrid[outputGridRow].CumulativeTotalPastDue = outputGrid[outputGridRow].TotalPastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeTotalPastDue);

                #endregion

                //Recalculate the output grid for the remaining schedule dates in the input grid.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AddNSFAndLateFee() method. Calling method : ReCalculateOutputGridValues(outputGrid[outputGridRow].BeginningPrincipal, outputGrid[outputGridRow].BeginningServiceFee, outputGrid, scheduleInputs, outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);");
                ReCalculateOutputGridValues(outputGrid[outputGridRow].BeginningPrincipal, outputGrid[outputGridRow].BeginningServiceFee, outputGrid, scheduleInputs,
                                                                outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);

                //Set the cumulative NSF and Late fee past due amount columns of next row.
                if (outputGrid.Count >= outputGridRow + 1)
                {
                    outputGrid[outputGridRow + 1].CumulativeNSFFeePastDue = outputGrid[outputGridRow].CumulativeNSFFeePastDue;
                    outputGrid[outputGridRow + 1].CumulativeLateFeePastDue = outputGrid[outputGridRow].CumulativeLateFeePastDue;
                    outputGrid[outputGridRow + 1].CumulativeTotalPastDue = outputGrid[outputGridRow].CumulativeTotalPastDue;
                }
                sbTracing.AppendLine("Exist:From class ActualPaymentAllocation and method name AddNSFAndLateFee(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double sameDayFee)");
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

        #region Management and Maintenance Fee

        /// <summary>
        /// This function adds the Management fee and Maintenance fee event of additional payment to the output grid.
        /// </summary>
        /// <param name="outputGrid"></param>
        /// <param name="scheduleInputs"></param>
        /// <param name="additionalPaymentRow"></param>
        /// <param name="outputGridRow"></param>
        /// <param name="defaultTotalAmountToPay"></param>
        /// <param name="defaultTotalServiceFeePayable"></param>
        /// <param name="isExtendingSchedule"></param>
        /// <param name="originationFee"></param>
        /// <param name="sameDayFee"></param>
        private static void ManagementAndMaintenanceFee(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, int outputGridRow,
                            double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name ManagementAndMaintenanceFee(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee).");
                sbTracing.AppendLine("Parameter values are : additionalPaymentRow = " + additionalPaymentRow + ", outputGridRow = " + outputGridRow + ", defaultTotalAmountToPay = " + defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", isExtendingSchedule = " + isExtendingSchedule + ", originationFee = " + originationFee + ", sameDayFee = " + sameDayFee);

                double remainingPrincipal = isExtendingSchedule ? (outputGrid[outputGridRow - 1].BeginningPrincipal - outputGrid[outputGridRow - 1].PrincipalPaid) :
                                                                            outputGrid[outputGridRow].BeginningPrincipal;
                double remainingPrincipalServiceFee = isExtendingSchedule ? (outputGrid[outputGridRow - 1].BeginningServiceFee - outputGrid[outputGridRow - 1].ServiceFeePaid) :
                                                                outputGrid[outputGridRow].BeginningServiceFee;

                //It determines the total interest carry over to be paid.
                double interestCarryOver = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].InterestCarryOver);
                double serviceFeeInterestCarryOver = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].AccruedServiceFeeInterestCarryOver);

                //Checks whether the previous row was the skipped payment event.
                if (outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].Flags == (int)Constants.FlagValues.SkipPayment)
                {
                    //Add the previous interest payable amount to carry over value, as previously due to skip payment it was not paid.
                    interestCarryOver += (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].InterestPayment);
                    //Add the previous service fee interest payable amount to carry over value, as previously due to skip payment it was not paid.
                    serviceFeeInterestCarryOver += (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].ServiceFeeInterest);
                }

                #region Get NSF and Late fee of upper rows when payment is skipped

                int lastSkippedRow = outputGridRow;
                for (int i = 0; i < outputGridRow; i++)
                {
                    if (outputGrid[i].Flags != (int)Constants.FlagValues.SkipPayment)
                    {
                        break;
                    }
                    lastSkippedRow = i;
                }

                #endregion

                #region Add row for Management or maintenance fee

                //Insert a new row in the output grid on index "outputGridRow".
                outputGrid.Insert(outputGridRow,
                        new OutputGrid
                        {
                            PaymentDate = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].DateIn,
                            BeginningPrincipal = remainingPrincipal,
                            BeginningServiceFee = remainingPrincipalServiceFee,
                            PeriodicInterestRate = 0,
                            DailyInterestRate = 0,
                            DailyInterestAmount = 0,
                            PaymentID = Convert.ToInt32(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].PaymentID.ToString()),
                            Flags = Convert.ToInt32(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].Flags.ToString()),
                            DueDate = scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].DateIn,
                            InterestAccrued = (outputGridRow == 0 ? scheduleInputs.EarnedInterest : 0),
                            InterestCarryOver = (outputGridRow == 0 ? scheduleInputs.EarnedInterest : 0) + interestCarryOver,
                            InterestPayment = 0,
                            PrincipalPayment = 0,
                            TotalPayment = 0,
                            AccruedServiceFeeInterest = (outputGridRow == 0 ? scheduleInputs.EarnedServiceFeeInterest : 0),
                            AccruedServiceFeeInterestCarryOver = (outputGridRow == 0 ? scheduleInputs.EarnedServiceFeeInterest : 0) + serviceFeeInterestCarryOver,
                            ServiceFee = 0,
                            ServiceFeeInterest = 0,
                            ServiceFeeTotal = 0,
                            OriginationFee = (outputGridRow == 0 ? scheduleInputs.EarnedOriginationFee : 0),
                            MaintenanceFee = (outputGridRow == 0 ? scheduleInputs.EarnedMaintenanceFee : 0),
                            ManagementFee = (outputGridRow == 0 ? scheduleInputs.EarnedManagementFee : 0),
                            SameDayFee = (outputGridRow == 0 ? scheduleInputs.EarnedSameDayFee : 0),
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
                            //Past due amount columns
                            PrincipalPastDue = 0,
                            InterestPastDue = 0,
                            ServiceFeePastDue = 0,
                            ServiceFeeInterestPastDue = 0,
                            OriginationFeePastDue = (outputGridRow == 0 ? scheduleInputs.EarnedOriginationFee : 0),
                            MaintenanceFeePastDue = (outputGridRow == 0 ? scheduleInputs.EarnedMaintenanceFee : 0) +
                                                (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].Flags == (int)Constants.FlagValues.MaintenanceFee ? scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment : 0),
                            ManagementFeePastDue = (outputGridRow == 0 ? scheduleInputs.EarnedManagementFee : 0) +
                                                (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].Flags == (int)Constants.FlagValues.ManagementFee ? scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment : 0),
                            SameDayFeePastDue = (outputGridRow == 0 ? scheduleInputs.EarnedSameDayFee : 0),
                            NSFFeePastDue = ((outputGridRow == 0 || lastSkippedRow == outputGridRow - 1) ? scheduleInputs.EarnedNSFFee : 0),
                            LateFeePastDue = ((outputGridRow == 0 || lastSkippedRow == outputGridRow - 1) ? scheduleInputs.EarnedLateFee : 0),
                            BucketStatus = "OutStanding"
                        });

                outputGrid[outputGridRow].TotalPastDue = outputGrid[outputGridRow].PrincipalPastDue + outputGrid[outputGridRow].InterestPastDue +
                                                            outputGrid[outputGridRow].ServiceFeePastDue + outputGrid[outputGridRow].ServiceFeeInterestPastDue +
                                                            outputGrid[outputGridRow].OriginationFeePastDue + outputGrid[outputGridRow].SameDayFeePastDue +
                                                            outputGrid[outputGridRow].MaintenanceFeePastDue + outputGrid[outputGridRow].ManagementFeePastDue +
                                                            outputGrid[outputGridRow].NSFFeePastDue + outputGrid[outputGridRow].LateFeePastDue;

                outputGrid[outputGridRow].CumulativePrincipalPastDue = outputGrid[outputGridRow].PrincipalPastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePrincipalPastDue);
                outputGrid[outputGridRow].CumulativeInterestPastDue = outputGrid[outputGridRow].InterestPastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeInterestPastDue);
                outputGrid[outputGridRow].CumulativeServiceFeePastDue = outputGrid[outputGridRow].ServiceFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeePastDue);
                outputGrid[outputGridRow].CumulativeServiceFeeInterestPastDue = outputGrid[outputGridRow].ServiceFeeInterestPastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeInterestPastDue);
                outputGrid[outputGridRow].CumulativeOriginationFeePastDue = outputGrid[outputGridRow].OriginationFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeOriginationFeePastDue);
                outputGrid[outputGridRow].CumulativeMaintenanceFeePastDue = outputGrid[outputGridRow].MaintenanceFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeMaintenanceFeePastDue);
                outputGrid[outputGridRow].CumulativeManagementFeePastDue = outputGrid[outputGridRow].ManagementFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeManagementFeePastDue);
                outputGrid[outputGridRow].CumulativeSameDayFeePastDue = outputGrid[outputGridRow].SameDayFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeSameDayFeePastDue);
                outputGrid[outputGridRow].CumulativeNSFFeePastDue = outputGrid[outputGridRow].NSFFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeNSFFeePastDue);
                outputGrid[outputGridRow].CumulativeLateFeePastDue = outputGrid[outputGridRow].LateFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeLateFeePastDue);
                outputGrid[outputGridRow].CumulativeTotalPastDue = outputGrid[outputGridRow].TotalPastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeTotalPastDue);

                #endregion

                if (!isExtendingSchedule)
                {
                    //Recalculate the output grid for the remaining schedule dates in the input grid.
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ManagementAndMaintenanceFee() method. Calling method : ReCalculateOutputGridValues(remainingPrincipal, remainingPrincipalServiceFee, outputGrid, scheduleInputs, outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);");
                    ReCalculateOutputGridValues(remainingPrincipal, remainingPrincipalServiceFee, outputGrid, scheduleInputs, outputGridRow + 1,
                                                                    defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);
                }

                //Set the cumulative NSF and Late fee past due amount columns of next row.
                if (outputGrid.Count > outputGridRow + 1)
                {
                    outputGrid[outputGridRow + 1].CumulativeNSFFeePastDue = outputGrid[outputGridRow].CumulativeNSFFeePastDue;
                    outputGrid[outputGridRow + 1].CumulativeLateFeePastDue = outputGrid[outputGridRow].CumulativeLateFeePastDue;
                    outputGrid[outputGridRow + 1].CumulativeTotalPastDue = outputGrid[outputGridRow].CumulativeTotalPastDue;
                }
                sbTracing.AppendLine("Exist:From class ActualPaymentAllocation and method name ManagementAndMaintenanceFee(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee)");
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

        #region Discount Payment

        /// <summary>
        /// This function will give discount on the components for the bank method. After deducting the discount amount, the remaining fund will be used to craete to 
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
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name AddDiscount(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, int scheduleInputGridRow, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee).");
                sbTracing.AppendLine("Parameter values are : additionalPaymentRow = " + additionalPaymentRow + ", scheduleInputGridRow = " + scheduleInputGridRow + ", outputGridRow = " + outputGridRow + ", defaultTotalAmountToPay = " + defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", isExtendingSchedule = " + isExtendingSchedule + ", originationFee = " + originationFee + ", sameDayFee = " + sameDayFee);

                double actualOrginationFee = originationFee;
                double actualSameDayFee = sameDayFee;

                double remainingPrincipal = isExtendingSchedule ? (outputGrid[outputGridRow - 1].BeginningPrincipal - outputGrid[outputGridRow - 1].PrincipalPaid) :
                                                                    outputGrid[outputGridRow].BeginningPrincipal;
                double remainingPrincipalServiceFee = isExtendingSchedule ? (outputGrid[outputGridRow - 1].BeginningServiceFee - outputGrid[outputGridRow - 1].ServiceFeePaid) :
                                                                    outputGrid[outputGridRow].BeginningServiceFee;

                double payablePrincipal = (remainingPrincipal < Round.RoundOffAmount(remainingPrincipal - scheduleInputs.Residual) ? remainingPrincipal :
                                                                Round.RoundOffAmount(remainingPrincipal - scheduleInputs.Residual));
                double payableServiceFee = remainingPrincipalServiceFee;

                DateTime startDate;
                int row = outputGrid.Take(outputGridRow).ToList().FindLastIndex(o => o.Flags != (int)Constants.FlagValues.NSFFee &&
                                                                o.Flags != (int)Constants.FlagValues.LateFee && o.Flags != (int)Constants.FlagValues.ManagementFee &&
                                                                o.Flags != (int)Constants.FlagValues.MaintenanceFee);

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

                double periodicInterestRate;
                double interestAccrued;
                //This condition determines whether there is an interest rate in additional payment grid, for that row. If it is provided, that value will be used, otherwise
                //default value will be used for periodic interest calculations.
                if (string.IsNullOrEmpty(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate))
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AddDiscount() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, remainingPrincipal, true,isExtendingSchedule,false);");
                    periodicInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule, false);

                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AddDiscount() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, remainingPrincipal, true,isExtendingSchedule,false);");
                    interestAccrued = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule, false);
                }
                else
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AddDiscount() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate),scheduleInputs.InputRecords[0].DateIn, true,isExtendingSchedule,false);");
                    periodicInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInputs.InputRecords[0].DateIn, true, isExtendingSchedule, false);

                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AddDiscount() method. Calling method : PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInputs.InputRecords[0].DateIn, remainingPrincipal, true,isExtendingSchedule,false);");
                    interestAccrued = PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInputs.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule, false);
                }
                interestAccrued = Round.RoundOffAmount(interestAccrued);

                //This variable calculates the daily interest amount for the period.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AddDiscount() method. Calling method : DailyInterestAmount.CalculateDailyInterestAmount(startDate, endDate, interestAccrued, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InterestDelay, scheduleInputs, isExtendingSchedule,false);");
                double dailyInterestAmount = DailyInterestAmount.CalculateDailyInterestAmount(startDate, endDate, interestAccrued, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InterestDelay, scheduleInputs, isExtendingSchedule, false);
                dailyInterestAmount = Round.RoundOffAmount(dailyInterestAmount);

                //Add the earned interest amount to the accrued value.
                interestAccrued += (outputGridRow == 0 ? scheduleInputs.EarnedInterest : 0);

                //It determines the total interest carry over to be paid.
                double interestCarryOver = interestAccrued + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].InterestCarryOver);

                double serviceFeeInterestAccrued = 0;
                double serviceFeeInterestCarryOver = 0;
                //Checks whether service fee interest is to be taken or not.
                if (scheduleInputs.ApplyServiceFeeInterest == 1 || scheduleInputs.ApplyServiceFeeInterest == 3)
                {
                    if (string.IsNullOrEmpty(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate))
                    {
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AddDiscount() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, remainingPrincipalServiceFee, true, isExtendingSchedule,false);");
                        serviceFeeInterestAccrued = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, remainingPrincipalServiceFee, true, isExtendingSchedule, false);
                    }
                    else
                    {
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AddDiscount() method. Calling method : PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInputs.InputRecords[0].DateIn, remainingPrincipalServiceFee, true, isExtendingSchedule,false);");
                        serviceFeeInterestAccrued = PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInputs.InputRecords[0].DateIn, remainingPrincipalServiceFee, true, isExtendingSchedule, false);
                    }
                    serviceFeeInterestAccrued = Round.RoundOffAmount(serviceFeeInterestAccrued);
                }
                //Add earned service fee interest value to the accrued value.
                serviceFeeInterestAccrued += (outputGridRow == 0 ? scheduleInputs.EarnedServiceFeeInterest : 0);
                //It determines the total interest carry over to be paid.
                serviceFeeInterestCarryOver = serviceFeeInterestAccrued + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].AccruedServiceFeeInterestCarryOver);

                //Checks whether the pervious event was a skipped payment and is not the first row of output grid.
                if (outputGridRow != 0 && outputGrid[outputGridRow - 1].Flags == (int)Constants.FlagValues.SkipPayment)
                {
                    interestCarryOver += outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].InterestPayment;
                    serviceFeeInterestCarryOver += outputGrid[outputGridRow == 0 ? 0 : outputGridRow - 1].ServiceFeeInterest;
                }

                //This variable calculates the daily interest rate for the period.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AddDiscount() method. Calling method : DailyInterestRate.CalculateDailyInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InterestDelay, scheduleInputs, isExtendingSchedule,false);");
                double dailyInterestRate = DailyInterestRate.CalculateDailyInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InterestDelay, scheduleInputs, isExtendingSchedule, false);

                #region Get NSF and Late fee of upper rows when payment is skipped

                int lastSkippedRow = outputGridRow;
                for (int i = 0; i < outputGridRow; i++)
                {
                    if (outputGrid[i].Flags != (int)Constants.FlagValues.SkipPayment)
                    {
                        break;
                    }
                    lastSkippedRow = i;
                }

                #endregion

                #region Add row for Discount payment

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
                            OriginationFee = (outputGridRow == 0 ? scheduleInputs.EarnedOriginationFee : 0),
                            MaintenanceFee = (outputGridRow == 0 ? scheduleInputs.EarnedMaintenanceFee : 0),
                            ManagementFee = (outputGridRow == 0 ? scheduleInputs.EarnedManagementFee : 0),
                            SameDayFee = (outputGridRow == 0 ? scheduleInputs.EarnedSameDayFee : 0),
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
                            //Past due amount columns
                            PrincipalPastDue = 0,
                            InterestPastDue = 0,
                            ServiceFeePastDue = 0,
                            ServiceFeeInterestPastDue = 0,
                            OriginationFeePastDue = 0,
                            MaintenanceFeePastDue = 0,
                            ManagementFeePastDue = 0,
                            SameDayFeePastDue = 0,
                            NSFFeePastDue = ((outputGridRow == 0 || lastSkippedRow == outputGridRow - 1) ? scheduleInputs.EarnedNSFFee : 0),
                            LateFeePastDue = ((outputGridRow == 0 || lastSkippedRow == outputGridRow - 1) ? scheduleInputs.EarnedLateFee : 0),
                            BucketStatus = "OutStanding"
                        });

                outputGrid[outputGridRow].TotalPastDue = outputGrid[outputGridRow].PrincipalPastDue + outputGrid[outputGridRow].InterestPastDue +
                                                            outputGrid[outputGridRow].ServiceFeePastDue + outputGrid[outputGridRow].ServiceFeeInterestPastDue +
                                                            outputGrid[outputGridRow].OriginationFeePastDue + outputGrid[outputGridRow].SameDayFeePastDue +
                                                            outputGrid[outputGridRow].MaintenanceFeePastDue + outputGrid[outputGridRow].ManagementFeePastDue +
                                                            outputGrid[outputGridRow].NSFFeePastDue + outputGrid[outputGridRow].LateFeePastDue;

                outputGrid[outputGridRow].CumulativePrincipalPastDue = outputGrid[outputGridRow].PrincipalPastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePrincipalPastDue);
                outputGrid[outputGridRow].CumulativeInterestPastDue = outputGrid[outputGridRow].InterestPastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeInterestPastDue);
                outputGrid[outputGridRow].CumulativeServiceFeePastDue = outputGrid[outputGridRow].ServiceFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeePastDue);
                outputGrid[outputGridRow].CumulativeServiceFeeInterestPastDue = outputGrid[outputGridRow].ServiceFeeInterestPastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeInterestPastDue);
                outputGrid[outputGridRow].CumulativeOriginationFeePastDue = outputGrid[outputGridRow].OriginationFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeOriginationFeePastDue);
                outputGrid[outputGridRow].CumulativeMaintenanceFeePastDue = outputGrid[outputGridRow].MaintenanceFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeMaintenanceFeePastDue);
                outputGrid[outputGridRow].CumulativeManagementFeePastDue = outputGrid[outputGridRow].ManagementFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeManagementFeePastDue);
                outputGrid[outputGridRow].CumulativeSameDayFeePastDue = outputGrid[outputGridRow].SameDayFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeSameDayFeePastDue);
                outputGrid[outputGridRow].CumulativeNSFFeePastDue = outputGrid[outputGridRow].NSFFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeNSFFeePastDue);
                outputGrid[outputGridRow].CumulativeLateFeePastDue = outputGrid[outputGridRow].LateFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeLateFeePastDue);
                outputGrid[outputGridRow].CumulativeTotalPastDue = outputGrid[outputGridRow].TotalPastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeTotalPastDue);

                #endregion

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

                int lastRowIndex = outputGrid.FindLastIndex(o => o.DueDate <= scheduleInputs.InputRecords[scheduleInputs.InputRecords.Count - 1].DateIn);

                #region Allocate Discounts to past due amounts

                for (int i = 0; i < outputGridRow; i++)
                {
                    double principalAmount = (payablePrincipal > outputGrid[i].PrincipalPastDue ? outputGrid[i].PrincipalPastDue : payablePrincipal);
                    //Applying discount amount in principal past due amounts.
                    if (principalDiscount >= principalAmount)
                    {
                        outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - principalAmount);
                        outputGrid[i].CumulativePrincipalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativePrincipalPastDue - principalAmount);
                        outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue - principalAmount);
                        principalDiscount = Round.RoundOffAmount(principalDiscount - principalAmount);
                        outputGrid[outputGridRow].PrincipalPaid += principalAmount;
                        if ((lastRowIndex != i) && isExtendingSchedule)
                        {
                            outputGrid[lastRowIndex].PrincipalPastDue -= principalAmount;
                        }
                        payablePrincipal = Round.RoundOffAmount(payablePrincipal - principalAmount);
                        outputGrid[i].PrincipalPastDue = 0;
                    }
                    else
                    {
                        outputGrid[i].PrincipalPastDue = Round.RoundOffAmount(principalAmount - principalDiscount);
                        outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - principalDiscount);
                        outputGrid[i].CumulativePrincipalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativePrincipalPastDue - principalDiscount);
                        outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue - principalDiscount);
                        outputGrid[outputGridRow].PrincipalPaid += principalDiscount;
                        if ((lastRowIndex != i) && isExtendingSchedule)
                        {
                            outputGrid[lastRowIndex].PrincipalPastDue -= principalDiscount;
                        }
                        payablePrincipal = Round.RoundOffAmount(payablePrincipal - principalDiscount);

                        for (int j = i + 1; j < lastRowIndex; j++)
                        {
                            outputGrid[j].PrincipalPastDue = Round.RoundOffAmount(principalAmount - principalDiscount);
                            outputGrid[j].TotalPastDue = Round.RoundOffAmount(outputGrid[j].TotalPastDue - principalDiscount);
                            outputGrid[j].CumulativePrincipalPastDue = Round.RoundOffAmount(outputGrid[j].CumulativePrincipalPastDue - principalDiscount);
                            outputGrid[j].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[j].CumulativeTotalPastDue - principalDiscount);
                        }

                        principalDiscount = 0;
                    }

                    if (payablePrincipal <= 0)
                    {
                        for (int j = 0; j < outputGridRow; j++)
                        {
                            outputGrid[j].TotalPastDue = Round.RoundOffAmount(outputGrid[j].TotalPastDue - outputGrid[j].PrincipalPastDue);
                            outputGrid[j].PrincipalPastDue = 0;
                            outputGrid[j].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[j].CumulativeTotalPastDue - outputGrid[j].CumulativePrincipalPastDue);
                            outputGrid[j].CumulativePrincipalPastDue = 0;
                        }
                    }

                    //Applying discount amount in interest past due amounts.
                    if (interestDiscount >= outputGrid[i].InterestPastDue)
                    {
                        outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - outputGrid[i].InterestPastDue);
                        outputGrid[i].CumulativeInterestPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeInterestPastDue - outputGrid[i].InterestPastDue);
                        outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue - outputGrid[i].InterestPastDue);
                        interestDiscount = Round.RoundOffAmount(interestDiscount - outputGrid[i].InterestPastDue);
                        outputGrid[outputGridRow].InterestPaid += outputGrid[i].InterestPastDue;
                        if ((lastRowIndex != i) && isExtendingSchedule)
                        {
                            outputGrid[lastRowIndex].InterestPastDue -= outputGrid[i].InterestPastDue;
                        }
                        outputGrid[i].InterestPastDue = 0;
                    }
                    else
                    {
                        outputGrid[i].InterestPastDue = Round.RoundOffAmount(outputGrid[i].InterestPastDue - interestDiscount);
                        outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - interestDiscount);
                        outputGrid[i].CumulativeInterestPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeInterestPastDue - interestDiscount);
                        outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue - interestDiscount);
                        outputGrid[outputGridRow].InterestPaid += interestDiscount;
                        if ((lastRowIndex != i) && isExtendingSchedule)
                        {
                            outputGrid[lastRowIndex].InterestPastDue -= interestDiscount;
                        }
                        interestDiscount = 0;
                    }

                    double serviceFeeAmount = (payableServiceFee > outputGrid[i].ServiceFeePastDue ? outputGrid[i].ServiceFeePastDue : payableServiceFee);
                    //Applying discount amount in Service Fee past due amounts.
                    if (serviceFeeDiscount >= serviceFeeAmount)
                    {
                        outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - serviceFeeAmount);
                        outputGrid[i].CumulativeServiceFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeServiceFeePastDue - serviceFeeAmount);
                        outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue - serviceFeeAmount);
                        serviceFeeDiscount = Round.RoundOffAmount(serviceFeeDiscount - serviceFeeAmount);
                        outputGrid[outputGridRow].ServiceFeePaid += serviceFeeAmount;
                        if ((lastRowIndex != i) && isExtendingSchedule)
                        {
                            outputGrid[lastRowIndex].ServiceFeePastDue -= serviceFeeAmount;
                        }
                        payableServiceFee = Round.RoundOffAmount(payableServiceFee - serviceFeeAmount);
                        outputGrid[i].ServiceFeePastDue = 0;
                    }
                    else
                    {
                        outputGrid[i].ServiceFeePastDue = Round.RoundOffAmount(serviceFeeAmount - serviceFeeDiscount);
                        outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - serviceFeeDiscount);
                        outputGrid[i].CumulativeServiceFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeServiceFeePastDue - serviceFeeDiscount);
                        outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue - serviceFeeDiscount);
                        outputGrid[outputGridRow].ServiceFeePaid += serviceFeeDiscount;
                        if ((lastRowIndex != i) && isExtendingSchedule)
                        {
                            outputGrid[lastRowIndex].ServiceFeePastDue -= serviceFeeDiscount;
                        }
                        payableServiceFee = Round.RoundOffAmount(payableServiceFee - serviceFeeDiscount);

                        for (int j = i + 1; j < lastRowIndex; j++)
                        {
                            outputGrid[j].ServiceFeePastDue = Round.RoundOffAmount(serviceFeeAmount - serviceFeeDiscount);
                            outputGrid[j].TotalPastDue = Round.RoundOffAmount(outputGrid[j].TotalPastDue - serviceFeeDiscount);
                            outputGrid[j].CumulativeServiceFeePastDue = Round.RoundOffAmount(outputGrid[j].CumulativeServiceFeePastDue - serviceFeeDiscount);
                            outputGrid[j].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[j].CumulativeTotalPastDue - serviceFeeDiscount);
                        }

                        serviceFeeDiscount = 0;
                    }

                    if (payableServiceFee <= 0)
                    {
                        for (int j = 0; j < outputGridRow; j++)
                        {
                            outputGrid[j].TotalPastDue = Round.RoundOffAmount(outputGrid[j].TotalPastDue - outputGrid[j].ServiceFeePastDue);
                            outputGrid[j].ServiceFeePastDue = 0;
                            outputGrid[j].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[j].CumulativeTotalPastDue - outputGrid[j].CumulativeServiceFeePastDue);
                            outputGrid[j].CumulativeServiceFeePastDue = 0;
                        }
                    }

                    //Applying discount amount in Service Fee Interest past due amounts.
                    if (serviceFeeInterestDiscount >= outputGrid[i].ServiceFeeInterestPastDue)
                    {
                        outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - outputGrid[i].ServiceFeeInterestPastDue);
                        outputGrid[i].CumulativeServiceFeeInterestPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeServiceFeeInterestPastDue - outputGrid[i].ServiceFeeInterestPastDue);
                        outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue - outputGrid[i].ServiceFeeInterestPastDue);
                        serviceFeeInterestDiscount = Round.RoundOffAmount(serviceFeeInterestDiscount - outputGrid[i].ServiceFeeInterestPastDue);
                        outputGrid[outputGridRow].ServiceFeeInterestPaid += outputGrid[i].ServiceFeeInterestPastDue;
                        if ((lastRowIndex != i) && isExtendingSchedule)
                        {
                            outputGrid[lastRowIndex].ServiceFeeInterestPastDue -= outputGrid[i].ServiceFeeInterestPastDue;
                        }
                        outputGrid[i].ServiceFeeInterestPastDue = 0;
                    }
                    else
                    {
                        outputGrid[i].ServiceFeeInterestPastDue = Round.RoundOffAmount(outputGrid[i].ServiceFeeInterestPastDue - serviceFeeInterestDiscount);
                        outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - serviceFeeInterestDiscount);
                        outputGrid[i].CumulativeServiceFeeInterestPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeServiceFeeInterestPastDue - serviceFeeInterestDiscount);
                        outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue - serviceFeeInterestDiscount);
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
                        outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - outputGrid[i].OriginationFeePastDue);
                        outputGrid[i].CumulativeOriginationFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeOriginationFeePastDue - outputGrid[i].OriginationFeePastDue);
                        outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue - outputGrid[i].OriginationFeePastDue);
                        originationFeeDiscount = Round.RoundOffAmount(originationFeeDiscount - outputGrid[i].OriginationFeePastDue);
                        outputGrid[outputGridRow].OriginationFeePaid += outputGrid[i].OriginationFeePastDue;
                        outputGrid[i].OriginationFeePastDue = 0;
                    }
                    else
                    {
                        outputGrid[i].OriginationFeePastDue = Round.RoundOffAmount(outputGrid[i].OriginationFeePastDue - originationFeeDiscount);
                        outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - originationFeeDiscount);
                        outputGrid[i].CumulativeOriginationFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeOriginationFeePastDue - originationFeeDiscount);
                        outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue - originationFeeDiscount);
                        outputGrid[outputGridRow].OriginationFeePaid += originationFeeDiscount;
                        originationFeeDiscount = 0;
                    }

                    //Applying discount amount in maintenance Fee past due amounts.
                    if (maintenanceFeeDiscount >= outputGrid[i].MaintenanceFeePastDue)
                    {
                        outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - outputGrid[i].MaintenanceFeePastDue);
                        outputGrid[i].CumulativeMaintenanceFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeMaintenanceFeePastDue - outputGrid[i].MaintenanceFeePastDue);
                        outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue - outputGrid[i].MaintenanceFeePastDue);
                        maintenanceFeeDiscount = Round.RoundOffAmount(maintenanceFeeDiscount - outputGrid[i].MaintenanceFeePastDue);
                        outputGrid[outputGridRow].MaintenanceFeePaid += outputGrid[i].MaintenanceFeePastDue;
                        outputGrid[i].MaintenanceFeePastDue = 0;
                    }
                    else
                    {
                        outputGrid[i].MaintenanceFeePastDue = Round.RoundOffAmount(outputGrid[i].MaintenanceFeePastDue - maintenanceFeeDiscount);
                        outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - maintenanceFeeDiscount);
                        outputGrid[i].CumulativeMaintenanceFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeMaintenanceFeePastDue - maintenanceFeeDiscount);
                        outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue - maintenanceFeeDiscount);
                        outputGrid[outputGridRow].MaintenanceFeePaid += maintenanceFeeDiscount;
                        maintenanceFeeDiscount = 0;
                    }

                    //Applying discount amount in management Fee past due amounts.
                    if (managementFeeDiscount >= outputGrid[i].ManagementFeePastDue)
                    {
                        outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - outputGrid[i].ManagementFeePastDue);
                        outputGrid[i].CumulativeManagementFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeManagementFeePastDue - outputGrid[i].ManagementFeePastDue);
                        outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue - outputGrid[i].ManagementFeePastDue);
                        managementFeeDiscount = Round.RoundOffAmount(managementFeeDiscount - outputGrid[i].ManagementFeePastDue);
                        outputGrid[outputGridRow].ManagementFeePaid += outputGrid[i].ManagementFeePastDue;
                        outputGrid[i].ManagementFeePastDue = 0;
                    }
                    else
                    {
                        outputGrid[i].ManagementFeePastDue = Round.RoundOffAmount(outputGrid[i].ManagementFeePastDue - managementFeeDiscount);
                        outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - managementFeeDiscount);
                        outputGrid[i].CumulativeManagementFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeManagementFeePastDue - managementFeeDiscount);
                        outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue - managementFeeDiscount);
                        outputGrid[outputGridRow].ManagementFeePaid += managementFeeDiscount;
                        managementFeeDiscount = 0;
                    }

                    //Applying discount amount in Same Day Fee past due amounts.
                    if (sameDayFeeDiscount >= outputGrid[i].SameDayFeePastDue)
                    {
                        outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - outputGrid[i].SameDayFeePastDue);
                        outputGrid[i].CumulativeSameDayFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeSameDayFeePastDue - outputGrid[i].SameDayFeePastDue);
                        outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue - outputGrid[i].SameDayFeePastDue);
                        sameDayFeeDiscount = Round.RoundOffAmount(sameDayFeeDiscount - outputGrid[i].SameDayFeePastDue);
                        outputGrid[outputGridRow].SameDayFeePaid += outputGrid[i].SameDayFeePastDue;
                        outputGrid[i].SameDayFeePastDue = 0;
                    }
                    else
                    {
                        outputGrid[i].SameDayFeePastDue = Round.RoundOffAmount(outputGrid[i].SameDayFeePastDue - sameDayFeeDiscount);
                        outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - sameDayFeeDiscount);
                        outputGrid[i].CumulativeSameDayFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeSameDayFeePastDue - sameDayFeeDiscount);
                        outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue - sameDayFeeDiscount);
                        outputGrid[outputGridRow].SameDayFeePaid += sameDayFeeDiscount;
                        sameDayFeeDiscount = 0;
                    }

                    //Applying discount amount in NSF Fee past due amounts.
                    if (nsfFeeDiscount >= outputGrid[i].NSFFeePastDue)
                    {
                        outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - outputGrid[i].NSFFeePastDue);
                        outputGrid[i].CumulativeNSFFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeNSFFeePastDue - outputGrid[i].NSFFeePastDue);
                        outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue - outputGrid[i].NSFFeePastDue);
                        nsfFeeDiscount = Round.RoundOffAmount(nsfFeeDiscount - outputGrid[i].NSFFeePastDue);
                        outputGrid[outputGridRow].NSFFeePaid += outputGrid[i].NSFFeePastDue;
                        outputGrid[i].NSFFeePastDue = 0;
                    }
                    else
                    {
                        outputGrid[i].NSFFeePastDue = Round.RoundOffAmount(outputGrid[i].NSFFeePastDue - nsfFeeDiscount);
                        outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - nsfFeeDiscount);
                        outputGrid[i].CumulativeNSFFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeNSFFeePastDue - nsfFeeDiscount);
                        outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue - nsfFeeDiscount);
                        outputGrid[outputGridRow].NSFFeePaid += nsfFeeDiscount;
                        nsfFeeDiscount = 0;
                    }

                    //Applying discount amount in Late Fee past due amounts.
                    if (lateFeeDiscount >= outputGrid[i].LateFeePastDue)
                    {
                        outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - outputGrid[i].LateFeePastDue);
                        outputGrid[i].CumulativeLateFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeLateFeePastDue - outputGrid[i].LateFeePastDue);
                        outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue - outputGrid[i].LateFeePastDue);
                        lateFeeDiscount = Round.RoundOffAmount(lateFeeDiscount - outputGrid[i].LateFeePastDue);
                        outputGrid[outputGridRow].LateFeePaid += outputGrid[i].LateFeePastDue;
                        outputGrid[i].LateFeePastDue = 0;
                    }
                    else
                    {
                        outputGrid[i].LateFeePastDue = Round.RoundOffAmount(outputGrid[i].LateFeePastDue - lateFeeDiscount);
                        outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue - lateFeeDiscount);
                        outputGrid[i].CumulativeLateFeePastDue = Round.RoundOffAmount(outputGrid[i].CumulativeLateFeePastDue - lateFeeDiscount);
                        outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[i].CumulativeTotalPastDue - lateFeeDiscount);
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

                #region Pay down remaining amount

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
                        outputGrid[outputGridRow].OriginationFee = outputGrid[firstScheduleIndex].OriginationFee;
                        outputGrid[firstScheduleIndex].OriginationFee = 0;
                    }
                    else
                    {
                        outputGrid[outputGridRow].OriginationFeePaid += originationFeeDiscount;
                        if (outputGridRow == 0)
                        {
                            if (outputGrid[firstScheduleIndex].OriginationFee - scheduleInputs.EarnedOriginationFee > originationFeeDiscount)
                            {
                                outputGrid[outputGridRow].OriginationFee = originationFeeDiscount + scheduleInputs.EarnedOriginationFee;
                            }
                            else
                            {
                                outputGrid[outputGridRow].OriginationFee = outputGrid[firstScheduleIndex].OriginationFee;
                            }
                        }
                        else
                        {
                            outputGrid[outputGridRow].OriginationFee = originationFeeDiscount;
                        }
                        outputGrid[firstScheduleIndex].OriginationFee -= originationFeeDiscount;
                    }
                }
                //Check whether there is an amount remains to pay due to skipping the first repayment schedule.
                else if (originationFee > 0)
                {
                    outputGrid[outputGridRow].OriginationFee = (originationFee <= originationFeeDiscount) ? originationFee : originationFeeDiscount;
                    outputGrid[outputGridRow].OriginationFeePaid += (originationFee <= originationFeeDiscount) ? originationFee : originationFeeDiscount;
                }

                //Give discount on same day fee.
                //Check whether same day fee date is larger than first scheduled date.
                if (scheduleInputs.AdditionalPaymentRecords[additionalPaymentRow].DateIn <= outputGrid[firstScheduleIndex].PaymentDate)
                {
                    if (outputGrid[firstScheduleIndex].SameDayFee <= sameDayFeeDiscount)
                    {
                        outputGrid[outputGridRow].SameDayFeePaid += outputGrid[firstScheduleIndex].SameDayFee;
                        outputGrid[outputGridRow].SameDayFee = outputGrid[firstScheduleIndex].SameDayFee;
                        outputGrid[firstScheduleIndex].SameDayFee = 0;
                    }
                    else
                    {
                        outputGrid[outputGridRow].SameDayFeePaid += sameDayFeeDiscount;
                        if (outputGridRow == 0)
                        {
                            if (outputGrid[firstScheduleIndex].SameDayFee - scheduleInputs.EarnedSameDayFee > sameDayFeeDiscount)
                            {
                                outputGrid[outputGridRow].SameDayFee = sameDayFeeDiscount + scheduleInputs.EarnedSameDayFee;
                            }
                            else
                            {
                                outputGrid[outputGridRow].SameDayFee = outputGrid[firstScheduleIndex].SameDayFee;
                            }
                        }
                        else
                        {
                            outputGrid[outputGridRow].SameDayFee = sameDayFeeDiscount;
                        }
                        outputGrid[firstScheduleIndex].SameDayFee -= sameDayFeeDiscount;
                    }
                }
                //Check whether there is an amount remains to pay due to skipping the first repayment schedule.
                else if (sameDayFee > 0)
                {
                    outputGrid[outputGridRow].SameDayFee = (sameDayFee <= sameDayFeeDiscount) ? sameDayFee : sameDayFeeDiscount;
                    outputGrid[outputGridRow].SameDayFeePaid += (sameDayFee <= sameDayFeeDiscount) ? sameDayFee : sameDayFeeDiscount;
                }

                if (outputGridRow == 0)
                {
                    outputGrid[outputGridRow].ManagementFeePaid = (outputGrid[outputGridRow].ManagementFee >= managementFeeDiscount) ? managementFeeDiscount :
                                                                    outputGrid[outputGridRow].ManagementFee;
                    outputGrid[outputGridRow].MaintenanceFeePaid = (outputGrid[outputGridRow].MaintenanceFee >= maintenanceFeeDiscount) ? maintenanceFeeDiscount :
                                                                    outputGrid[outputGridRow].MaintenanceFee;

                    outputGrid[outputGridRow].ManagementFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].ManagementFee - outputGrid[outputGridRow].ManagementFeePaid);
                    outputGrid[outputGridRow].MaintenanceFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].MaintenanceFee - outputGrid[outputGridRow].MaintenanceFeePaid);

                    outputGrid[outputGridRow].OriginationFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].OriginationFee - outputGrid[outputGridRow].OriginationFeePaid);
                    outputGrid[outputGridRow].SameDayFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].SameDayFee - outputGrid[outputGridRow].SameDayFeePaid);
                }

                if (outputGridRow == 0 || lastSkippedRow == outputGridRow - 1)
                {
                    outputGrid[outputGridRow].NSFFeePaid = (outputGrid[outputGridRow].NSFFeePastDue >= nsfFeeDiscount) ? nsfFeeDiscount : outputGrid[outputGridRow].NSFFeePastDue;
                    outputGrid[outputGridRow].LateFeePaid = (outputGrid[outputGridRow].LateFeePastDue >= lateFeeDiscount) ? lateFeeDiscount : outputGrid[outputGridRow].LateFeePastDue;
                    outputGrid[outputGridRow].NSFFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].NSFFeePastDue - outputGrid[outputGridRow].NSFFeePaid);
                    outputGrid[outputGridRow].LateFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].LateFeePastDue - outputGrid[outputGridRow].LateFeePaid);
                    outputGrid[outputGridRow].CumulativeNSFFeePastDue = outputGrid[outputGridRow].NSFFeePastDue;
                    outputGrid[outputGridRow].CumulativeLateFeePastDue = outputGrid[outputGridRow].LateFeePastDue;
                }

                outputGrid[outputGridRow].TotalPastDue = outputGrid[outputGridRow].PrincipalPastDue + outputGrid[outputGridRow].InterestPastDue +
                                                           outputGrid[outputGridRow].ServiceFeePastDue + outputGrid[outputGridRow].ServiceFeeInterestPastDue +
                                                           outputGrid[outputGridRow].OriginationFeePastDue + outputGrid[outputGridRow].SameDayFeePastDue +
                                                           outputGrid[outputGridRow].ManagementFeePastDue + outputGrid[outputGridRow].MaintenanceFeePastDue +
                                                           outputGrid[outputGridRow].NSFFeePastDue + outputGrid[outputGridRow].LateFeePastDue;

                //Determine the remaining principal amount to be paid after processing the current row.
                remainingPrincipal -= outputGrid[outputGridRow].PrincipalPaid;

                //Checks whether there is any remaining principal discount.
                if (principalDiscount > 0)
                {
                    //Pay the remaining principal amount till the discount amount is set to 0.
                    outputGrid[outputGridRow].PrincipalPaid += (principalDiscount > payablePrincipal ? payablePrincipal : principalDiscount);
                    //Then calculate the remining principal amount.
                    remainingPrincipal = Round.RoundOffAmount(remainingPrincipal - (principalDiscount > payablePrincipal ? payablePrincipal : principalDiscount));
                }

                //Determine the remaining principalservice fee amount to be paid after processing the current row.
                remainingPrincipalServiceFee -= outputGrid[outputGridRow].ServiceFeePaid;

                //Checks whether there is any remaining principal service fee discount.
                if (serviceFeeDiscount > 0)
                {
                    //Pay the remaining service fee amount till the discount amount is set to 0.
                    outputGrid[outputGridRow].ServiceFeePaid += (serviceFeeDiscount > remainingPrincipalServiceFee ? remainingPrincipalServiceFee : serviceFeeDiscount);
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
                    outputGrid[outputGridRow].TotalPastDue = Round.RoundOffAmount(outputGrid[outputGridRow].TotalPastDue);
                    outputGrid[outputGridRow].CumulativeTotalPastDue -= (managementFeeDiscount > outputGrid[outputGridRow].ManagementFeePastDue ? outputGrid[outputGridRow].ManagementFeePastDue : managementFeeDiscount);
                    outputGrid[outputGridRow].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[outputGridRow].CumulativeTotalPastDue);
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
                    outputGrid[outputGridRow].TotalPastDue = Round.RoundOffAmount(outputGrid[outputGridRow].TotalPastDue);
                    outputGrid[outputGridRow].CumulativeTotalPastDue -= (maintenanceFeeDiscount > outputGrid[outputGridRow].MaintenanceFeePastDue ? outputGrid[outputGridRow].MaintenanceFeePastDue : maintenanceFeeDiscount);
                    outputGrid[outputGridRow].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[outputGridRow].CumulativeTotalPastDue);
                    outputGrid[outputGridRow].CumulativeMaintenanceFeePastDue -= (maintenanceFeeDiscount > outputGrid[outputGridRow].MaintenanceFeePastDue ? outputGrid[outputGridRow].MaintenanceFeePastDue : maintenanceFeeDiscount);
                    outputGrid[outputGridRow].MaintenanceFeePastDue -= (maintenanceFeeDiscount > outputGrid[outputGridRow].MaintenanceFeePastDue ? outputGrid[outputGridRow].MaintenanceFeePastDue : maintenanceFeeDiscount);
                }

                #endregion

                outputGrid[outputGridRow].TotalPaid = outputGrid[outputGridRow].PrincipalPaid + outputGrid[outputGridRow].InterestPaid +
                                                outputGrid[outputGridRow].ServiceFeePaid + outputGrid[outputGridRow].ServiceFeeInterestPaid +
                                                outputGrid[outputGridRow].OriginationFeePaid + outputGrid[outputGridRow].MaintenanceFeePaid +
                                                outputGrid[outputGridRow].ManagementFeePaid + outputGrid[outputGridRow].SameDayFeePaid +
                                                outputGrid[outputGridRow].NSFFeePaid + outputGrid[outputGridRow].LateFeePaid;

                //Set the cumulative NSF and Late fee past due amount columns of next row.
                if (outputGrid.Count > outputGridRow + 1)
                {
                    outputGrid[outputGridRow + 1].CumulativeNSFFeePastDue = outputGrid[outputGridRow].CumulativeNSFFeePastDue;
                    outputGrid[outputGridRow + 1].CumulativeLateFeePastDue = outputGrid[outputGridRow].CumulativeLateFeePastDue;
                }

                //Recalculates the output grid values for the remaining scheduled dates if the current event is not the part of schedule extend functionality.
                if (!isExtendingSchedule)
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AddDiscount() method. Calling method : ReCalculateOutputGridValues(remainingPrincipal, remainingprincipalServiceFee, outputGrid, scheduleInputs, outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, actualOrginationFee, actualSameDayFee);");
                    ReCalculateOutputGridValues(remainingPrincipal, remainingPrincipalServiceFee, outputGrid, scheduleInputs, outputGridRow + 1, defaultTotalAmountToPay,
                                                        defaultTotalServiceFeePayable, actualOrginationFee, actualSameDayFee);
                }
                sbTracing.AppendLine("Exist:From class ActualPaymentAllocation and method name AddDiscount(List<OutputGrid> outputGrid, LoanDetails scheduleInputs, int additionalPaymentRow, int scheduleInputGridRow, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee)");
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