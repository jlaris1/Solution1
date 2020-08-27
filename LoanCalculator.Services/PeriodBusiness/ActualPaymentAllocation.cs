using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoanAmort_driver.Models;

namespace LoanAmort_driver.PeriodBusiness
{
    public class ActualPaymentAllocation
    {
        /// <summary>
        /// Name : Sparsh Agarwal
        /// Date : 25/09/2017
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
        /// <param name="loanAmountCalc"></param>
        /// <param name="outputSchedule"></param>
        /// <param name="originationFee"></param>
        /// <param name="sameDayFee"></param>
        public static void AllocationOfFunds(List<OutputGrid> outputGrid, LoanDetails scheduleInput, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable,
                                                double loanAmountCalc, OutputSchedule outputSchedule, double originationFee, double sameDayFee)
        {
            int additionalPaymentCurrentRow = 0;
            int outputGridRow = 0;

            int managementFeeTableIndex = 0;
            int maintenanceFeeTableIndex = 0;
            int lastManagementFeeChargedIndex = -1;
            int lastMaintenanceFeeChargedIndex = -1;

            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name AllocationOfFunds(List<OutputGrid> outputGrid, LoanDetails scheduleInput, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double loanAmountCalc, OutputSchedule outputSchedule, double originationFee, double sameDayFee).");
                sbTracing.AppendLine("Parameter values are : defaultTotalAmountToPay = " + defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", loanAmountCalc = " + loanAmountCalc + ", originationFee = " + originationFee + ", sameDayFee = " + sameDayFee);

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
                        //Process Additional payment after schedule when additional payment date and schedule date occures on same date.
                        if (scheduleInput.AdditionalPaymentRecords[j].DateIn < (scheduleInput.InputRecords[inputGridRow].EffectiveDate == DateTime.MinValue ? scheduleInput.InputRecords[inputGridRow].DateIn : scheduleInput.InputRecords[inputGridRow].EffectiveDate))
                        {
                            #region Calculate Management Fee and Maintenance Fee

                            sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRow, j, true, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);");
                            managementFeePayable = ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRow, j, true, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);
                            sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRow, j, true, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);");
                            maintenanceFeePayable = ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRow, j, true, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);

                            #endregion

                            switch (scheduleInput.AdditionalPaymentRecords[j].Flags)
                            {
                                case (int)Constants.FlagValues.PrincipalOnly:
                                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : PrincipalOnlyPayment(outputGrid, scheduleInput, additionalPaymentCurrentRow, inputGridRow, outputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, false, originationFee, sameDayFee, managementFeePayable, maintenanceFeePayable);");
                                    PrincipalOnlyPayment(outputGrid, scheduleInput, additionalPaymentCurrentRow, inputGridRow, outputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, false, originationFee, sameDayFee, managementFeePayable, maintenanceFeePayable);
                                    break;

                                case (int)Constants.FlagValues.AdditionalPayment:
                                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : AdditionalPayment(outputGrid, scheduleInput, additionalPaymentCurrentRow, inputGridRow, outputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, false, originationFee, sameDayFee, managementFeePayable, maintenanceFeePayable);");
                                    AdditionalPayment(outputGrid, scheduleInput, additionalPaymentCurrentRow, inputGridRow, outputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, false, originationFee, sameDayFee, managementFeePayable, maintenanceFeePayable);
                                    break;

                                case (int)Constants.FlagValues.NSFFee:
                                case (int)Constants.FlagValues.LateFee:
                                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : NSFandLateFee(outputGrid, scheduleInput, additionalPaymentCurrentRow, inputGridRow, outputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);");
                                    NSFandLateFee(outputGrid, scheduleInput, additionalPaymentCurrentRow, inputGridRow, outputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);
                                    break;

                                case (int)Constants.FlagValues.Discount:
                                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : DiscountPayment(outputGrid, scheduleInput, additionalPaymentCurrentRow, inputGridRow, outputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, false, originationFee, sameDayFee);");
                                    DiscountPayment(outputGrid, scheduleInput, additionalPaymentCurrentRow, inputGridRow, outputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, false, originationFee, sameDayFee);
                                    break;

                                case (int)Constants.FlagValues.ManagementFee:
                                case (int)Constants.FlagValues.MaintenanceFee:
                                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFee(outputGrid, scheduleInput, additionalPaymentCurrentRow, inputGridRow, outputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, false, originationFee, sameDayFee);");
                                    ManagementAndMaintenanceFee(outputGrid, scheduleInput, additionalPaymentCurrentRow, inputGridRow, outputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, false, originationFee, sameDayFee);
                                    break;
                            }
                            additionalPaymentCurrentRow++;
                            outputGridRow++;
                        }
                    }

                    #region Calculate Management Fee and Maintenance Fee

                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRow, inputGridRow, false, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);");
                    managementFeePayable = ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRow, inputGridRow, false, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRow, inputGridRow, false, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);");
                    maintenanceFeePayable = ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRow, inputGridRow, false, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);
                    outputGrid[outputGridRow].ManagementFee += managementFeePayable;
                    outputGrid[outputGridRow].MaintenanceFee += maintenanceFeePayable;
                    outputGrid[outputGridRow].TotalPayment += managementFeePayable + maintenanceFeePayable;
                    outputGrid[outputGridRow].PaymentDue = outputGrid[outputGridRow].TotalPayment;

                    #endregion

                    //This condition checks, the schedule payment is for paid, missed, adjustment, and skip.
                    if (scheduleInput.InputRecords[inputGridRow].Flags == (int)Constants.FlagValues.Payment || scheduleInput.InputRecords[inputGridRow].Flags == (int)Constants.FlagValues.SkipPayment)
                    {
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : AllocateFundToBuckets(outputGrid, scheduleInput, inputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, outputGridRow, originationFee, sameDayFee, outputSchedule.LoanAmountCalc);");
                        AllocateFundToBuckets(outputGrid, scheduleInput, inputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, outputGridRow, originationFee, sameDayFee, outputSchedule.LoanAmountCalc);
                    }
                    outputGridRow++;
                }

                #endregion

                #region Delete unused rows

                int inputGridRowForDelete = 1;
                int additionalGridRowForDelete = 0;
                int managementFeeTableIndexForDelete = 0;
                int maintenanceFeeTableIndexForDelete = 0;
                int lastManagementFeeChargedIndexForDelete = -1;
                int lastMaintenanceFeeChargedIndexForDelete = -1;
                double managementFee = 0;
                double maintenanceFee = 0;
                int startDeletionIndex = outputGrid.Count;
                int sameDateOfAdditionalPayment = 0;
                DateTime previousDate = scheduleInput.InputRecords[0].DateIn;
                for (int i = 0; i <= outputGrid.Count - 1; i++)
                {
                    if (inputGridRowForDelete < scheduleInput.InputRecords.Count && scheduleInput.InputRecords[inputGridRowForDelete].DateIn == outputGrid[i].DueDate &&
                           scheduleInput.InputRecords[inputGridRowForDelete].Flags == outputGrid[i].Flags)
                    {
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, inputGridRowForDelete, false, false, false, ref managementFeeTableIndexForDelete, ref lastManagementFeeChargedIndexForDelete);");
                        managementFee += ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, inputGridRowForDelete, false, false, false, ref managementFeeTableIndexForDelete, ref lastManagementFeeChargedIndexForDelete);
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, inputGridRowForDelete, false, false, false, ref maintenanceFeeTableIndexForDelete, ref lastMaintenanceFeeChargedIndexForDelete);");
                        maintenanceFee += ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, inputGridRowForDelete, false, false, false, ref maintenanceFeeTableIndexForDelete, ref lastMaintenanceFeeChargedIndexForDelete);
                        inputGridRowForDelete++;
                    }
                    else
                    {
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, additionalGridRowForDelete, true, false, false, ref managementFeeTableIndexForDelete, ref lastManagementFeeChargedIndexForDelete);");
                        managementFee += ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, additionalGridRowForDelete, true, false, false, ref managementFeeTableIndexForDelete, ref lastManagementFeeChargedIndexForDelete);
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, additionalGridRowForDelete, true, false, false, ref maintenanceFeeTableIndexForDelete, ref lastMaintenanceFeeChargedIndexForDelete);");
                        maintenanceFee += ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, additionalGridRowForDelete, true, false, false, ref maintenanceFeeTableIndexForDelete, ref lastMaintenanceFeeChargedIndexForDelete);
                        additionalGridRowForDelete++;
                    }

                    if ((outputGrid[i].Flags != (int)Constants.FlagValues.Payment && outputGrid[i].Flags != (int)Constants.FlagValues.SkipPayment) &&
                        (((scheduleInput.ManagementFeeFrequency == (int)Constants.FeeFrequency.Days || scheduleInput.ManagementFeeFrequency == (int)Constants.FeeFrequency.Months) &&
                            (scheduleInput.ManagementFee != 0 || scheduleInput.ManagementFeePercent != 0) &&
                            outputSchedule.ManagementFeeAssessment.FindIndex(o => o.AssessmentDate > previousDate && o.AssessmentDate <= outputGrid[i].PaymentDate) != -1 &&
                            outputSchedule.ManagementFeeAssessment.Where(o => o.AssessmentDate > previousDate && o.AssessmentDate <= outputGrid[i].PaymentDate).Sum(o => o.Fee) > 0 &&
                            (managementFee < (scheduleInput.ManagementFeeMaxLoan > 0 ? scheduleInput.ManagementFeeMaxLoan : (managementFee + 1)))) ||
                        ((scheduleInput.MaintenanceFeeFrequency == (int)Constants.FeeFrequency.Days || scheduleInput.MaintenanceFeeFrequency == (int)Constants.FeeFrequency.Months) &&
                            (scheduleInput.MaintenanceFee != 0 || scheduleInput.MaintenanceFeePercent != 0) &&
                            outputSchedule.MaintenanceFeeAssessment.FindIndex(o => o.AssessmentDate > previousDate && o.AssessmentDate <= outputGrid[i].PaymentDate) != -1 &&
                            outputSchedule.MaintenanceFeeAssessment.Where(o => o.AssessmentDate > previousDate && o.AssessmentDate <= outputGrid[i].PaymentDate).Sum(o => o.Fee) > 0 &&
                            (maintenanceFee < (scheduleInput.MaintenanceFeeMaxLoan > 0 ? scheduleInput.MaintenanceFeeMaxLoan : (maintenanceFee + 1))))))
                    {
                        continue;
                    }

                    if (i < outputGrid.Count - 1 && outputGrid[i].PaymentDate != outputGrid[i + 1].PaymentDate)
                    {
                        previousDate = outputGrid[i].PaymentDate;
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
                                        Round.RoundOffAmount(managementFee) + Round.RoundOffAmount(maintenanceFee) - scheduleInput.Residual;
                    totalDue = Round.RoundOffAmount(totalDue);
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
                    if ((Round.RoundOffAmount(outputGrid[outputGrid.Count - 1].BeginningPrincipal - outputGrid[outputGrid.Count - 1].PrincipalPaid - scheduleInput.Residual) > 0 ||
                        Round.RoundOffAmount(outputGrid[outputGrid.Count - 1].BeginningServiceFee - outputGrid[outputGrid.Count - 1].ServiceFeePaid) > 0 ||
                        Round.RoundOffAmount(outputGrid[outputGrid.Count - 1].InterestCarryOver) > 0 ||
                        Round.RoundOffAmount(outputGrid[outputGrid.Count - 1].serviceFeeInterestCarryOver) > 0 ||
                        Round.RoundOffAmount(outputGrid[outputGrid.Count - 1].TotalPastDue) > 0) &&
                        scheduleInput.AdditionalPaymentRecords[j].DateIn >= outputGrid[outputGrid.Count - 1].PaymentDate)
                    {
                        #region Calculate Management Fee and Maintenance Fee

                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRow, j, true, true, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);");
                        managementFeePayable = ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRow, j, true, true, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRow, j, true, true, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);");
                        maintenanceFeePayable = ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRow, j, true, true, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);

                        #endregion
                        switch (scheduleInput.AdditionalPaymentRecords[j].Flags)
                        {
                            case (int)Constants.FlagValues.PrincipalOnly:
                                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : PrincipalOnlyPayment(outputGrid, scheduleInput, j, inputGridRow, outputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, true, originationFee, sameDayFee, managementFeePayable, maintenanceFeePayable);");
                                PrincipalOnlyPayment(outputGrid, scheduleInput, j, inputGridRow, outputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, true, originationFee, sameDayFee, managementFeePayable, maintenanceFeePayable);
                                break;

                            case (int)Constants.FlagValues.AdditionalPayment:
                                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : AdditionalPayment(outputGrid, scheduleInput, j, inputGridRow, outputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, true, originationFee, sameDayFee, managementFeePayable, maintenanceFeePayable);");
                                AdditionalPayment(outputGrid, scheduleInput, j, inputGridRow, outputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, true, originationFee, sameDayFee, managementFeePayable, maintenanceFeePayable);
                                break;

                            case (int)Constants.FlagValues.Discount:
                                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : DiscountPayment(outputGrid, scheduleInput, j, inputGridRow, outputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, true, originationFee, sameDayFee);");
                                DiscountPayment(outputGrid, scheduleInput, j, inputGridRow, outputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, true, originationFee, sameDayFee);
                                break;

                            case (int)Constants.FlagValues.ManagementFee:
                            case (int)Constants.FlagValues.MaintenanceFee:
                                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : DiscountPayment(outputGrid, scheduleInput, j, inputGridRow, outputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, true, originationFee, sameDayFee);");
                                ManagementAndMaintenanceFee(outputGrid, scheduleInput, j, inputGridRow, outputGridRow, defaultTotalAmountToPay, defaultTotalServiceFeePayable, true, originationFee, sameDayFee);
                                break;
                        }
                        outputGridRow++;
                    }
                }

                #endregion

                #region Early Payoff

                if (scheduleInput.EarlyPayoffDate.ToShortDateString() != Convert.ToDateTime(Constants.DefaultDate).ToShortDateString())
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : EarlyPayOff.CalculateEarlyPayOff(outputGrid, scheduleInput, originationFee, sameDayFee, outputSchedule);");
                    EarlyPayOff.CalculateEarlyPayOff(outputGrid, scheduleInput, originationFee, sameDayFee, outputSchedule);
                }
                else
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
                            bool isExtendingRow = scheduleInput.AdditionalPaymentRecords[additionalPaymentCurrentRow].DateIn > effectiveDate;

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

                sbTracing.AppendLine("Exist:From class ActualPaymentAllocation and method name AllocationOfFunds(List<OutputGrid> outputGrid, LoanDetails scheduleInput, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double loanAmountCalc, OutputSchedule outputSchedule, double originationFee, double sameDayFee)");
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

        #region Scheduled Payment i.e. for paid, adjustment, missed and skipped payment

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
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name AllocateFundToBuckets(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int inputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, int outputGridRow, double originationFee, double sameDayFee, double loanAmount).");
                sbTracing.AppendLine("Parameter values are : inputGridRow = " + inputGridRow + ", defaultTotalAmountToPay = " + defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", outputGridRow = " + outputGridRow + ", originationFee = " + originationFee + ", sameDayFee = " + sameDayFee + ", loanAmount = " + loanAmount);

                //Determine that current row is the last row or not.
                bool isLastRow = (outputGridRow + 1 == outputGrid.Count);

                //If current row is the last row than adding the past due values in current row.
                if (isLastRow)
                {
                    outputGrid[outputGridRow].InterestDue += (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].InterestPastDue);
                    outputGrid[outputGridRow].InterestPayment += (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].InterestPastDue);
                    outputGrid[outputGridRow].ServiceFeeInterest += (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].ServiceFeeInterestPastDue);
                    outputGrid[outputGridRow].ServiceFeeInterestDue += (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].ServiceFeeInterestPastDue);
                    outputGrid[outputGridRow].ServiceFeeTotal = outputGrid[outputGridRow].ServiceFee + outputGrid[outputGridRow].ServiceFeeInterest;
                    outputGrid[outputGridRow].ManagementFee += (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].ManagementFeePastDue);
                    outputGrid[outputGridRow].MaintenanceFee += (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].MaintenanceFeePastDue);
                    outputGrid[outputGridRow].NSFFee += (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].NSFFeePastDue);
                    outputGrid[outputGridRow].LateFee += (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].LateFeePastDue);
                    outputGrid[outputGridRow].TotalPayment += (outputGridRow == 0 ? 0 : (outputGrid[outputGridRow - 1].InterestPastDue + outputGrid[outputGridRow - 1].ServiceFeeInterestPastDue +
                                                                outputGrid[outputGridRow - 1].ManagementFeePastDue + outputGrid[outputGridRow - 1].MaintenanceFeePastDue +
                                                                outputGrid[outputGridRow - 1].NSFFeePastDue + outputGrid[outputGridRow - 1].LateFeePastDue));
                    outputGrid[outputGridRow].PaymentDue = outputGrid[outputGridRow].TotalPayment;
                }

                double paymentAmount = string.IsNullOrEmpty(scheduleInput.InputRecords[inputGridRow].PaymentAmount) ?
                                            ((scheduleInput.PaymentAmount > 0 && !isLastRow) ? scheduleInput.PaymentAmount :
                                                outputGrid[outputGridRow].TotalPayment) : Convert.ToDouble(scheduleInput.InputRecords[inputGridRow].PaymentAmount);

                if (scheduleInput.InputRecords[inputGridRow].Flags == (int)Constants.FlagValues.SkipPayment)
                {
                    paymentAmount = 0;
                }

                #region Allocate fund to current row

                if (outputGridRow != 0 && paymentAmount >= outputGrid[outputGridRow].TotalPayment)
                {
                    outputGrid[outputGridRow].PrincipalPaid = outputGrid[outputGridRow].PrincipalPayment;
                    outputGrid[outputGridRow].InterestPaid = outputGrid[outputGridRow].InterestPayment;
                    outputGrid[outputGridRow].ServiceFeePaid = outputGrid[outputGridRow].ServiceFee;
                    outputGrid[outputGridRow].ServiceFeeInterestPaid = outputGrid[outputGridRow].ServiceFeeInterest;
                    outputGrid[outputGridRow].OriginationFeePaid = outputGrid[outputGridRow].OriginationFee;
                    outputGrid[outputGridRow].SameDayFeePaid = outputGrid[outputGridRow].SameDayFee;
                    outputGrid[outputGridRow].ManagementFeePaid = outputGrid[outputGridRow].ManagementFee;
                    outputGrid[outputGridRow].MaintenanceFeePaid = outputGrid[outputGridRow].MaintenanceFee;
                    outputGrid[outputGridRow].NSFFeePaid = outputGrid[outputGridRow].NSFFee;
                    outputGrid[outputGridRow].LateFeePaid = outputGrid[outputGridRow].LateFee;

                    paymentAmount = Round.RoundOffAmount(paymentAmount - (outputGrid[outputGridRow].PrincipalPaid + outputGrid[outputGridRow].InterestPaid +
                                                                    outputGrid[outputGridRow].ServiceFeePaid + outputGrid[outputGridRow].ServiceFeeInterestPaid +
                                                                    outputGrid[outputGridRow].OriginationFeePaid + outputGrid[outputGridRow].SameDayFeePaid +
                                                                    outputGrid[outputGridRow].ManagementFeePaid + outputGrid[outputGridRow].MaintenanceFeePaid +
                                                                    outputGrid[outputGridRow].NSFFeePaid + outputGrid[outputGridRow].LateFeePaid));
                }
                else
                {
                    for (int i = 1; i <= 10; i++)
                    {
                        if (scheduleInput.InputRecords[inputGridRow].PrincipalPriority == i)
                        {
                            if (outputGrid[outputGridRow].PrincipalPayment >= paymentAmount)
                            {
                                outputGrid[outputGridRow].PrincipalPaid = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                outputGrid[outputGridRow].PrincipalPaid = outputGrid[outputGridRow].PrincipalPayment;
                                paymentAmount -= outputGrid[outputGridRow].PrincipalPayment;
                            }
                        }
                        else if (scheduleInput.InputRecords[inputGridRow].InterestPriority == i)
                        {
                            if (outputGrid[outputGridRow].InterestPayment >= paymentAmount)
                            {
                                outputGrid[outputGridRow].InterestPaid = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                outputGrid[outputGridRow].InterestPaid = outputGrid[outputGridRow].InterestPayment;
                                paymentAmount -= outputGrid[outputGridRow].InterestPayment;
                            }
                        }
                        else if (scheduleInput.InputRecords[inputGridRow].ServiceFeePriority == i)
                        {
                            if (outputGrid[outputGridRow].ServiceFee >= paymentAmount)
                            {
                                outputGrid[outputGridRow].ServiceFeePaid = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                outputGrid[outputGridRow].ServiceFeePaid = outputGrid[outputGridRow].ServiceFee;
                                paymentAmount -= outputGrid[outputGridRow].ServiceFee;
                            }
                        }
                        else if (scheduleInput.InputRecords[inputGridRow].ServiceFeeInterestPriority == i)
                        {
                            if (outputGrid[outputGridRow].ServiceFeeInterest >= paymentAmount)
                            {
                                outputGrid[outputGridRow].ServiceFeeInterestPaid = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                outputGrid[outputGridRow].ServiceFeeInterestPaid = outputGrid[outputGridRow].ServiceFeeInterest;
                                paymentAmount -= outputGrid[outputGridRow].ServiceFeeInterest;
                            }
                        }
                        else if (scheduleInput.InputRecords[inputGridRow].OriginationFeePriority == i)
                        {
                            if (outputGrid[outputGridRow].OriginationFee >= paymentAmount)
                            {
                                outputGrid[outputGridRow].OriginationFeePaid = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                outputGrid[outputGridRow].OriginationFeePaid = outputGrid[outputGridRow].OriginationFee;
                                paymentAmount -= outputGrid[outputGridRow].OriginationFee;
                            }
                        }
                        else if (scheduleInput.InputRecords[inputGridRow].SameDayFeePriority == i)
                        {
                            if (outputGrid[outputGridRow].SameDayFee >= paymentAmount)
                            {
                                outputGrid[outputGridRow].SameDayFeePaid = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                outputGrid[outputGridRow].SameDayFeePaid = outputGrid[outputGridRow].SameDayFee;
                                paymentAmount -= outputGrid[outputGridRow].SameDayFee;
                            }
                        }
                        else if (scheduleInput.InputRecords[inputGridRow].ManagementFeePriority == i)
                        {
                            if (outputGrid[outputGridRow].ManagementFee >= paymentAmount)
                            {
                                outputGrid[outputGridRow].ManagementFeePaid = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                outputGrid[outputGridRow].ManagementFeePaid = outputGrid[outputGridRow].ManagementFee;
                                paymentAmount -= outputGrid[outputGridRow].ManagementFee;
                            }
                        }
                        else if (scheduleInput.InputRecords[inputGridRow].MaintenanceFeePriority == i)
                        {
                            if (outputGrid[outputGridRow].MaintenanceFee >= paymentAmount)
                            {
                                outputGrid[outputGridRow].MaintenanceFeePaid = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                outputGrid[outputGridRow].MaintenanceFeePaid = outputGrid[outputGridRow].MaintenanceFee;
                                paymentAmount -= outputGrid[outputGridRow].MaintenanceFee;
                            }
                        }
                        else if (scheduleInput.InputRecords[inputGridRow].NSFFeePriority == i)
                        {
                            if (outputGrid[outputGridRow].NSFFee >= paymentAmount)
                            {
                                outputGrid[outputGridRow].NSFFeePaid = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                outputGrid[outputGridRow].NSFFeePaid = outputGrid[outputGridRow].NSFFee;
                                paymentAmount -= outputGrid[outputGridRow].NSFFee;
                            }
                        }
                        else if (scheduleInput.InputRecords[inputGridRow].LateFeePriority == i)
                        {
                            if (outputGrid[outputGridRow].LateFee >= paymentAmount)
                            {
                                outputGrid[outputGridRow].LateFeePaid = paymentAmount;
                                paymentAmount = 0;
                            }
                            else
                            {
                                outputGrid[outputGridRow].LateFeePaid = outputGrid[outputGridRow].LateFee;
                                paymentAmount -= outputGrid[outputGridRow].LateFee;
                            }
                        }

                        paymentAmount = Round.RoundOffAmount(paymentAmount);
                        if (paymentAmount == 0)
                        {
                            break;
                        }
                    }
                }

                #endregion

                outputGrid[outputGridRow].PrincipalPastDue = Round.RoundOffAmount(outputGrid[outputGridRow].PrincipalPayment - outputGrid[outputGridRow].PrincipalPaid);
                outputGrid[outputGridRow].InterestPastDue = Round.RoundOffAmount(outputGrid[outputGridRow].InterestPayment - outputGrid[outputGridRow].InterestPaid);
                outputGrid[outputGridRow].ServiceFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].ServiceFee - outputGrid[outputGridRow].ServiceFeePaid);
                outputGrid[outputGridRow].ServiceFeeInterestPastDue = Round.RoundOffAmount(outputGrid[outputGridRow].ServiceFeeInterest - outputGrid[outputGridRow].ServiceFeeInterestPaid);
                outputGrid[outputGridRow].OriginationFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].OriginationFee - outputGrid[outputGridRow].OriginationFeePaid);
                outputGrid[outputGridRow].SameDayFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].SameDayFee - outputGrid[outputGridRow].SameDayFeePaid);
                outputGrid[outputGridRow].ManagementFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].ManagementFee - outputGrid[outputGridRow].ManagementFeePaid);
                outputGrid[outputGridRow].MaintenanceFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].MaintenanceFee - outputGrid[outputGridRow].MaintenanceFeePaid);
                outputGrid[outputGridRow].NSFFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].NSFFee - outputGrid[outputGridRow].NSFFeePaid);
                outputGrid[outputGridRow].LateFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].LateFee - outputGrid[outputGridRow].LateFeePaid);

                //Create a variable that will determine the total principal amount paid till current date.
                double totalPrincipalPaid = outputGrid.Where(o => o.PaymentDate <= outputGrid[outputGridRow].PaymentDate).Sum(o => o.PrincipalPaid);

                #region Allocate fund to Past Due balances

                if (outputGridRow != 0 && !isLastRow)
                {
                    double pastDuePrincipal = (outputGrid[outputGridRow].BeginningPrincipal - outputGrid[outputGridRow].PrincipalPaid < outputGrid[outputGridRow - 1].PrincipalPastDue) ?
                                                    outputGrid[outputGridRow].BeginningPrincipal - outputGrid[outputGridRow].PrincipalPaid : outputGrid[outputGridRow - 1].PrincipalPastDue;
                    double pastDueServiceFee = (outputGrid[outputGridRow].BeginningServiceFee - outputGrid[outputGridRow].ServiceFeePaid < outputGrid[outputGridRow - 1].ServiceFeePastDue) ?
                                                    outputGrid[outputGridRow].BeginningServiceFee - outputGrid[outputGridRow].ServiceFeePaid : outputGrid[outputGridRow - 1].ServiceFeePastDue;
                    pastDuePrincipal = Round.RoundOffAmount(pastDuePrincipal);
                    pastDueServiceFee = Round.RoundOffAmount(pastDueServiceFee);

                    //Adjust the past due principal amount to be paid with the Residual value.
                    pastDuePrincipal = (pastDuePrincipal < Round.RoundOffAmount(loanAmount - scheduleInput.Residual - totalPrincipalPaid) ? pastDuePrincipal : (loanAmount - scheduleInput.Residual - totalPrincipalPaid));
                    pastDuePrincipal = Round.RoundOffAmount(pastDuePrincipal);

                    if (paymentAmount >= Round.RoundOffAmount(outputGrid[outputGridRow - 1].TotalPastDue - outputGrid[outputGridRow - 1].PrincipalPastDue -
                                                    outputGrid[outputGridRow - 1].ServiceFeePastDue - outputGrid[outputGridRow - 1].OriginationFeePastDue -
                                                    outputGrid[outputGridRow - 1].SameDayFeePastDue + pastDuePrincipal + pastDueServiceFee))
                    {
                        outputGrid[outputGridRow].PrincipalPaid += pastDuePrincipal;
                        outputGrid[outputGridRow].InterestPaid += outputGrid[outputGridRow - 1].InterestPastDue;
                        outputGrid[outputGridRow].ServiceFeePaid += pastDueServiceFee;
                        outputGrid[outputGridRow].ServiceFeeInterestPaid += outputGrid[outputGridRow - 1].ServiceFeeInterestPastDue;
                        outputGrid[outputGridRow].ManagementFeePaid += outputGrid[outputGridRow - 1].ManagementFeePastDue;
                        outputGrid[outputGridRow].MaintenanceFeePaid += outputGrid[outputGridRow - 1].MaintenanceFeePastDue;
                        outputGrid[outputGridRow].NSFFeePaid += outputGrid[outputGridRow - 1].NSFFeePastDue;
                        outputGrid[outputGridRow].LateFeePaid += outputGrid[outputGridRow - 1].LateFeePastDue;

                        paymentAmount = Round.RoundOffAmount(paymentAmount - (pastDuePrincipal + outputGrid[outputGridRow - 1].InterestPastDue +
                                                                        pastDueServiceFee + outputGrid[outputGridRow - 1].ServiceFeeInterestPastDue +
                                                                        outputGrid[outputGridRow - 1].ManagementFeePastDue + outputGrid[outputGridRow - 1].MaintenanceFeePastDue +
                                                                        outputGrid[outputGridRow - 1].NSFFeePastDue + outputGrid[outputGridRow - 1].LateFeePastDue));
                    }
                    else
                    {
                        for (int i = 1; i <= 10; i++)
                        {
                            if (scheduleInput.InputRecords[inputGridRow].PrincipalPriority == i)
                            {
                                if (pastDuePrincipal >= paymentAmount)
                                {
                                    outputGrid[outputGridRow].PrincipalPaid += paymentAmount;
                                    outputGrid[outputGridRow].PrincipalPastDue += Round.RoundOffAmount(pastDuePrincipal - paymentAmount);
                                    outputGrid[outputGridRow].PrincipalPastDue = (outputGrid[outputGridRow].PrincipalPastDue > (Round.RoundOffAmount(outputGrid[outputGridRow].BeginningPrincipal - outputGrid[outputGridRow].PrincipalPaid))) ?
                                                                        Round.RoundOffAmount(outputGrid[outputGridRow].BeginningPrincipal - outputGrid[outputGridRow].PrincipalPaid) :
                                                                        outputGrid[outputGridRow].PrincipalPastDue;

                                    totalPrincipalPaid += paymentAmount;
                                    outputGrid[outputGridRow].PrincipalPastDue = (outputGrid[outputGridRow].PrincipalPastDue < Round.RoundOffAmount(loanAmount - scheduleInput.Residual - totalPrincipalPaid) ?
                                                                                    outputGrid[outputGridRow].PrincipalPastDue : Round.RoundOffAmount(loanAmount - scheduleInput.Residual - totalPrincipalPaid));

                                    paymentAmount = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].PrincipalPaid += pastDuePrincipal;
                                    paymentAmount -= pastDuePrincipal;
                                    totalPrincipalPaid += pastDuePrincipal;
                                }
                            }
                            else if (scheduleInput.InputRecords[inputGridRow].InterestPriority == i)
                            {
                                if (outputGrid[outputGridRow - 1].InterestPastDue >= paymentAmount)
                                {
                                    outputGrid[outputGridRow].InterestPaid += paymentAmount;
                                    outputGrid[outputGridRow].InterestPastDue += Round.RoundOffAmount(outputGrid[outputGridRow - 1].InterestPastDue - paymentAmount);
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].InterestPaid += outputGrid[outputGridRow - 1].InterestPastDue;
                                    paymentAmount -= outputGrid[outputGridRow - 1].InterestPastDue;
                                }
                            }
                            else if (scheduleInput.InputRecords[inputGridRow].ServiceFeePriority == i)
                            {
                                if (pastDueServiceFee >= paymentAmount)
                                {
                                    outputGrid[outputGridRow].ServiceFeePaid += paymentAmount;
                                    outputGrid[outputGridRow].ServiceFeePastDue += Round.RoundOffAmount(pastDueServiceFee - paymentAmount);
                                    outputGrid[outputGridRow].ServiceFeePastDue = (outputGrid[outputGridRow].ServiceFeePastDue > (Round.RoundOffAmount(outputGrid[outputGridRow].BeginningServiceFee - outputGrid[outputGridRow].ServiceFeePaid))) ?
                                                                        Round.RoundOffAmount(outputGrid[outputGridRow].BeginningServiceFee - outputGrid[outputGridRow].ServiceFeePaid) :
                                                                        outputGrid[outputGridRow].ServiceFeePastDue;
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].ServiceFeePaid += pastDueServiceFee;
                                    paymentAmount -= pastDueServiceFee;
                                }
                            }
                            else if (scheduleInput.InputRecords[inputGridRow].ServiceFeeInterestPriority == i)
                            {
                                if (outputGrid[outputGridRow - 1].ServiceFeeInterestPastDue >= paymentAmount)
                                {
                                    outputGrid[outputGridRow].ServiceFeeInterestPaid += paymentAmount;
                                    outputGrid[outputGridRow].ServiceFeeInterestPastDue += Round.RoundOffAmount(outputGrid[outputGridRow - 1].ServiceFeeInterestPastDue - paymentAmount);
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].ServiceFeeInterestPaid += outputGrid[outputGridRow - 1].ServiceFeeInterestPastDue;
                                    paymentAmount -= outputGrid[outputGridRow - 1].ServiceFeeInterestPastDue;
                                }
                            }
                            else if (scheduleInput.InputRecords[inputGridRow].ManagementFeePriority == i)
                            {
                                if (outputGrid[outputGridRow - 1].ManagementFeePastDue >= paymentAmount)
                                {
                                    outputGrid[outputGridRow].ManagementFeePaid += paymentAmount;
                                    outputGrid[outputGridRow].ManagementFeePastDue += Round.RoundOffAmount(outputGrid[outputGridRow - 1].ManagementFeePastDue - paymentAmount);
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].ManagementFeePaid += outputGrid[outputGridRow - 1].ManagementFeePastDue;
                                    paymentAmount -= outputGrid[outputGridRow - 1].ManagementFeePastDue;
                                }
                            }
                            else if (scheduleInput.InputRecords[inputGridRow].MaintenanceFeePriority == i)
                            {
                                if (outputGrid[outputGridRow - 1].MaintenanceFeePastDue >= paymentAmount)
                                {
                                    outputGrid[outputGridRow].MaintenanceFeePaid += paymentAmount;
                                    outputGrid[outputGridRow].MaintenanceFeePastDue += Round.RoundOffAmount(outputGrid[outputGridRow - 1].MaintenanceFeePastDue - paymentAmount);
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].MaintenanceFeePaid += outputGrid[outputGridRow - 1].MaintenanceFeePastDue;
                                    paymentAmount -= outputGrid[outputGridRow - 1].MaintenanceFeePastDue;
                                }
                            }
                            else if (scheduleInput.InputRecords[inputGridRow].NSFFeePriority == i)
                            {
                                if (outputGrid[outputGridRow - 1].NSFFeePastDue >= paymentAmount)
                                {
                                    outputGrid[outputGridRow].NSFFeePaid += paymentAmount;
                                    outputGrid[outputGridRow].NSFFeePastDue += Round.RoundOffAmount(outputGrid[outputGridRow - 1].NSFFeePastDue - paymentAmount);
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].NSFFeePaid += outputGrid[outputGridRow - 1].NSFFeePastDue;
                                    paymentAmount -= outputGrid[outputGridRow - 1].NSFFeePastDue;
                                }
                            }
                            else if (scheduleInput.InputRecords[inputGridRow].LateFeePriority == i)
                            {
                                if (outputGrid[outputGridRow - 1].LateFeePastDue >= paymentAmount)
                                {
                                    outputGrid[outputGridRow].LateFeePaid += paymentAmount;
                                    outputGrid[outputGridRow].LateFeePastDue += Round.RoundOffAmount(outputGrid[outputGridRow - 1].LateFeePastDue - paymentAmount);
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].LateFeePaid += outputGrid[outputGridRow - 1].LateFeePastDue;
                                    paymentAmount -= outputGrid[outputGridRow - 1].LateFeePastDue;
                                }
                            }

                            paymentAmount = Round.RoundOffAmount(paymentAmount);
                        }
                    }
                }

                #endregion

                outputGrid[outputGridRow].TotalPastDue = outputGrid[outputGridRow].PrincipalPastDue + outputGrid[outputGridRow].InterestPastDue +
                                                        outputGrid[outputGridRow].ServiceFeePastDue + outputGrid[outputGridRow].ServiceFeeInterestPastDue +
                                                        outputGrid[outputGridRow].OriginationFeePastDue + outputGrid[outputGridRow].SameDayFeePastDue +
                                                        outputGrid[outputGridRow].ManagementFeePastDue + outputGrid[outputGridRow].MaintenanceFeePastDue +
                                                        outputGrid[outputGridRow].NSFFeePastDue + outputGrid[outputGridRow].LateFeePastDue;

                outputGrid[outputGridRow].CumulativePrincipalPastDue = outputGrid[outputGridRow].PrincipalPastDue;
                outputGrid[outputGridRow].CumulativeInterestPastDue = outputGrid[outputGridRow].InterestPastDue;
                outputGrid[outputGridRow].CumulativeServiceFeePastDue = outputGrid[outputGridRow].ServiceFeePastDue;
                outputGrid[outputGridRow].CumulativeServiceFeeInterestPastDue = outputGrid[outputGridRow].ServiceFeeInterestPastDue;
                outputGrid[outputGridRow].CumulativeOriginationFeePastDue = outputGrid[outputGridRow].OriginationFeePastDue;
                outputGrid[outputGridRow].CumulativeSameDayFeePastDue = outputGrid[outputGridRow].SameDayFeePastDue;
                outputGrid[outputGridRow].CumulativeManagementFeePastDue = outputGrid[outputGridRow].ManagementFeePastDue;
                outputGrid[outputGridRow].CumulativeMaintenanceFeePastDue = outputGrid[outputGridRow].MaintenanceFeePastDue;
                outputGrid[outputGridRow].CumulativeNSFFeePastDue = outputGrid[outputGridRow].NSFFeePastDue;
                outputGrid[outputGridRow].CumulativeLateFeePastDue = outputGrid[outputGridRow].LateFeePastDue;
                outputGrid[outputGridRow].CumulativeTotalPastDue = outputGrid[outputGridRow].TotalPastDue;

                #region Pay extra payment after paid down all the current and past due balances

                double remainingPrincipal = Round.RoundOffAmount(outputGrid[outputGridRow].BeginningPrincipal - outputGrid[outputGridRow].PrincipalPaid);
                double remainingServiceFee = Round.RoundOffAmount(outputGrid[outputGridRow].BeginningServiceFee - outputGrid[outputGridRow].ServiceFeePaid);

                paymentAmount = Round.RoundOffAmount(paymentAmount);
                if (paymentAmount > 0)
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocateFundToBuckets() method. Calling method : PayRemainingPrincipalAndServiceFee(outputGrid, outputGridRow, scheduleInput, inputGridRow, paymentAmount, ref remainingPrincipal, ref remainingServiceFee);");
                    PayRemainingPrincipalAndServiceFee(outputGrid, outputGridRow, scheduleInput, inputGridRow, paymentAmount, ref remainingPrincipal, ref remainingServiceFee);
                }

                #endregion

                outputGrid[outputGridRow].TotalPaid = outputGrid[outputGridRow].PrincipalPaid + outputGrid[outputGridRow].InterestPaid +
                                                        outputGrid[outputGridRow].ServiceFeePaid + outputGrid[outputGridRow].ServiceFeeInterestPaid +
                                                        outputGrid[outputGridRow].OriginationFeePaid + outputGrid[outputGridRow].SameDayFeePaid +
                                                        outputGrid[outputGridRow].ManagementFeePaid + outputGrid[outputGridRow].MaintenanceFeePaid +
                                                        outputGrid[outputGridRow].NSFFeePaid + outputGrid[outputGridRow].LateFeePaid;

                outputGrid[outputGridRow].CumulativePrincipal = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePrincipal) + outputGrid[outputGridRow].PrincipalPaid;
                outputGrid[outputGridRow].CumulativeInterest = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeInterest) + outputGrid[outputGridRow].InterestPaid;
                outputGrid[outputGridRow].CumulativePayment = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePayment) + outputGrid[outputGridRow].TotalPaid;
                outputGrid[outputGridRow].CumulativeServiceFee = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFee) + outputGrid[outputGridRow].ServiceFeePaid;
                outputGrid[outputGridRow].CumulativeServiceFeeInterest = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeInterest) + outputGrid[outputGridRow].ServiceFeeInterestPaid;
                outputGrid[outputGridRow].CumulativeOriginationFee = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeOriginationFee) + outputGrid[outputGridRow].OriginationFeePaid;
                outputGrid[outputGridRow].CumulativeSameDayFee = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeSameDayFee) + outputGrid[outputGridRow].SameDayFeePaid;
                outputGrid[outputGridRow].CumulativeManagementFee = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeManagementFee) + outputGrid[outputGridRow].ManagementFeePaid;
                outputGrid[outputGridRow].CumulativeMaintenanceFee = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeMaintenanceFee) + outputGrid[outputGridRow].MaintenanceFeePaid;
                outputGrid[outputGridRow].CumulativeNSFFee = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeNSFFee) + outputGrid[outputGridRow].NSFFeePaid;
                outputGrid[outputGridRow].CumulativeLateFee = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeLateFee) + outputGrid[outputGridRow].LateFeePaid;
                outputGrid[outputGridRow].CumulativeTotalFees = outputGrid[outputGridRow].CumulativeServiceFee + outputGrid[outputGridRow].CumulativeServiceFeeInterest +
                                                                outputGrid[outputGridRow].CumulativeOriginationFee + outputGrid[outputGridRow].CumulativeSameDayFee +
                                                                outputGrid[outputGridRow].CumulativeManagementFee + outputGrid[outputGridRow].CumulativeMaintenanceFee +
                                                                outputGrid[outputGridRow].CumulativeNSFFee + outputGrid[outputGridRow].CumulativeLateFee;

                #region Recalculate remaining repayment schedules

                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocateFundToBuckets() method. Calling method : ReCalculateSchedules(remainingPrincipal, remainingServiceFee, outputGrid, scheduleInput, outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);");
                ReCalculateSchedules(remainingPrincipal, remainingServiceFee, outputGrid, scheduleInput, outputGridRow + 1, defaultTotalAmountToPay,
                                        defaultTotalServiceFeePayable, originationFee, sameDayFee);

                #endregion

                sbTracing.AppendLine("Exist:From method name AllocateFundToBuckets(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int inputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, int outputGridRow, double originationFee, double sameDayFee, double loanAmount)");
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
        /// This function will the extra amount of principal and service fee as per priority
        /// </summary>
        /// <param name="outputGrid"></param>
        /// <param name="outputGridRow"></param>
        /// <param name="scheduleInput"></param>
        /// <param name="inputGridRow"></param>
        /// <param name="paymentAmount"></param>
        /// <param name="remainingPrincipal"></param>
        /// <param name="remainingServiceFee"></param>
        private static void PayRemainingPrincipalAndServiceFee(List<OutputGrid> outputGrid, int outputGridRow, LoanDetails scheduleInput, int inputGridRow,
                                                                double paymentAmount, ref double remainingPrincipal, ref double remainingServiceFee)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name PayRemainingPrincipalAndServiceFee(List<OutputGrid> outputGrid, int outputGridRow, LoanDetails scheduleInput, int inputGridRow, double paymentAmount, ref double remainingPrincipal, ref double remainingServiceFee).");
                sbTracing.AppendLine("Parameter values are : outputGridRow = " + outputGridRow + ", inputGridRow = " + inputGridRow + ", paymentAmount = " + paymentAmount + ", remainingPrincipal = " + remainingPrincipal + ", remainingServiceFee = " + remainingServiceFee);

                double payablePrincipal = (remainingPrincipal < Round.RoundOffAmount(remainingPrincipal - scheduleInput.Residual) ? remainingPrincipal :
                                                                Round.RoundOffAmount(remainingPrincipal - scheduleInput.Residual));

                if (scheduleInput.InputRecords[inputGridRow].PrincipalPriority < scheduleInput.InputRecords[inputGridRow].ServiceFeePriority)
                {
                    if (payablePrincipal >= paymentAmount)
                    {
                        outputGrid[outputGridRow].PrincipalPaid += paymentAmount;
                        remainingPrincipal -= paymentAmount;
                        paymentAmount = 0;
                    }
                    else
                    {
                        outputGrid[outputGridRow].PrincipalPaid += payablePrincipal;
                        paymentAmount -= payablePrincipal;
                        remainingPrincipal -= payablePrincipal;
                    }

                    paymentAmount = Round.RoundOffAmount(paymentAmount);

                    if (remainingServiceFee >= paymentAmount)
                    {
                        outputGrid[outputGridRow].ServiceFeePaid += paymentAmount;
                        remainingServiceFee -= paymentAmount;
                    }
                    else
                    {
                        outputGrid[outputGridRow].ServiceFeePaid += remainingServiceFee;
                        remainingServiceFee = 0;
                    }
                }
                else
                {
                    if (remainingServiceFee >= paymentAmount)
                    {
                        outputGrid[outputGridRow].ServiceFeePaid += paymentAmount;
                        remainingServiceFee -= paymentAmount;
                        paymentAmount = 0;
                    }
                    else
                    {
                        outputGrid[outputGridRow].ServiceFeePaid += remainingServiceFee;
                        paymentAmount -= remainingServiceFee;
                        remainingServiceFee = 0;
                    }

                    paymentAmount = Round.RoundOffAmount(paymentAmount);

                    if (payablePrincipal >= paymentAmount)
                    {
                        outputGrid[outputGridRow].PrincipalPaid += paymentAmount;
                        remainingPrincipal -= paymentAmount;
                    }
                    else
                    {
                        outputGrid[outputGridRow].PrincipalPaid += payablePrincipal;
                        remainingPrincipal -= payablePrincipal;
                    }
                }

                remainingPrincipal = Round.RoundOffAmount(remainingPrincipal);
                remainingServiceFee = Round.RoundOffAmount(remainingServiceFee);

                sbTracing.AppendLine("Exist:From method name PayRemainingPrincipalAndServiceFee(List<OutputGrid> outputGrid, int outputGridRow, LoanDetails scheduleInput, int inputGridRow, double paymentAmount, ref double remainingPrincipal, ref double remainingServiceFee).");
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

        #region Recalculate remaining repayment schedules

        /// <summary>
        /// Recalculates the remaining repayment schedules after the payment of current schedule.
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
        private static void ReCalculateSchedules(double remainingPrincipal, double remainingServiceFee, List<OutputGrid> outputGrid, LoanDetails scheduleInput,
                                int nextOutputRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double sameDayFee)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name ReCalculateSchedules(double remainingPrincipal, double remainingServiceFee, List<OutputGrid> outputGrid, LoanDetails scheduleInput, int nextOutputRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double sameDayFee).");
                sbTracing.AppendLine("Parameter values are : remainingPrincipal = " + remainingPrincipal + ", remainingServiceFee = " + remainingServiceFee + ", nextOutputRow = " + nextOutputRow + ", defaultTotalAmountToPay = " + defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", originationFee = " + originationFee + ", sameDayFee = " + sameDayFee);

                //Determine the last interest carry over, and service fee interest carry over value.
                double interestCarryOver = outputGrid[nextOutputRow == 0 ? 0 : nextOutputRow - 1].InterestCarryOver;
                double serviceFeeInterestCarryOver = outputGrid[nextOutputRow == 0 ? 0 : nextOutputRow - 1].AccruedServiceFeeInterestCarryOver;

                for (int i = 0; i < nextOutputRow; i++)
                {
                    originationFee = (originationFee - outputGrid[i].OriginationFeePaid) < 0 ? 0 : (originationFee - outputGrid[i].OriginationFeePaid);
                    sameDayFee = (sameDayFee - outputGrid[i].SameDayFeePaid) < 0 ? 0 : (sameDayFee - outputGrid[i].SameDayFeePaid);
                }

                //Set the column value origination fee and same day fee in current schedule to be paid.
                if (nextOutputRow < outputGrid.Count)
                {
                    outputGrid[nextOutputRow].OriginationFee = Round.RoundOffAmount(originationFee);
                    outputGrid[nextOutputRow].SameDayFee = Round.RoundOffAmount(sameDayFee);
                }

                //Determine the last index in output grid where interest was accrued.
                int lastOutputRow = outputGrid.Take(nextOutputRow).ToList().FindLastIndex(o => o.Flags != (int)Constants.FlagValues.ManagementFee &&
                                                                                                o.Flags != (int)Constants.FlagValues.MaintenanceFee);
                DateTime lastAccruedInterestDate = (lastOutputRow != -1 ? outputGrid[lastOutputRow].PaymentDate : scheduleInput.InputRecords[0].DateIn);

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
                    DateTime startDate = (i == nextOutputRow ? lastAccruedInterestDate : outputGrid[i - 1].PaymentDate);

                    //This condition checks whether a manual interest rate for that schedule payment is provided or not.
                    if (string.IsNullOrEmpty(scheduleInput.InputRecords[j].InterestRate))
                    {
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateSchedules() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInput, startDate, outputGrid[i].PaymentDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true,true);");
                        periodicInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInput, startDate, outputGrid[i].PaymentDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, true, false);

                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateSchedules() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, outputGrid[i].PaymentDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true,true);");
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
                            sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateSchedules() method. Calling method : PrincipalServiceFee.CalculatePrincipalServiceeFee(scheduleInput.ApplyServiceFeeInterest, principalLoanAmount, scheduleInput.ServiceFee);");
                            double principalServiceFee = scheduleInput.IsServiceFeeFinanced ? 0 : PrincipalServiceFee.CalculatePrincipalServiceeFee(scheduleInput.ApplyServiceFeeInterest, principalLoanAmount, scheduleInput.ServiceFee);

                            totalServiceFeePayable = Round.RoundOffAmount(principalServiceFee / (scheduleInput.InputRecords.Count - 1)) +
                                                     Round.RoundOffAmount(scheduleInput.EarnedServiceFeeInterest / (scheduleInput.InputRecords.Count - 1));
                        }
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateSchedules() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithoutTier(scheduleInput, startDate, outputGrid[i].PaymentDate, Convert.ToDouble(scheduleInput.InputRecords[j].InterestRate), scheduleInput.InputRecords[0].DateIn, true,true);");
                        periodicInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithoutTier(scheduleInput, startDate, outputGrid[i].PaymentDate, Convert.ToDouble(scheduleInput.InputRecords[j].InterestRate), scheduleInput.InputRecords[0].DateIn, true, true, false);

                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateSchedules() method. Calling method : PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, outputGrid[i].PaymentDate, Convert.ToDouble(scheduleInput.InputRecords[j].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true,true);");
                        interestAccrued = PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, outputGrid[i].PaymentDate, Convert.ToDouble(scheduleInput.InputRecords[j].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, true, false);
                    }
                    interestAccrued = Round.RoundOffAmount(interestAccrued);

                    //This variable calculates the daily interest rate for the period.
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateSchedules() method. Calling method : DailyInterestRate.CalculateDailyInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, outputGrid[i].PaymentDate, periodicInterestRate, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay,scheduleInput,true);");
                    double dailyInterestRate = DailyInterestRate.CalculateDailyInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, outputGrid[i].PaymentDate, periodicInterestRate, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, scheduleInput, true);

                    //This variable calculates the daily interest amount for the period.
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateSchedules() method. Calling method : DailyInterestAmount.CalculateDailyInterestAmount(startDate, outputGrid[i].PaymentDate, interestAccrued, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay,scheduleInput,true);");
                    double dailyInterestAmount = DailyInterestAmount.CalculateDailyInterestAmount(startDate, outputGrid[i].PaymentDate, interestAccrued, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, scheduleInput, true);
                    dailyInterestAmount = Round.RoundOffAmount(dailyInterestAmount);

                    #region Calculate principal and interest payable
                    double principalAmountToPay;

                    //This variable is the periodic interest of that period. It is multiplication of periodic rate and remaining principal amount.
                    double interestDue = interestAccrued + interestCarryOver;

                    //This condition determine whether the row is either last row. 
                    //If no, the carry over of interest will be added to interest accrued for current schedule, and limit to total payment. If it last payment schedule, 
                    //all remaining principal amount will be added to total amount to be paid.
                    if (i != outputGrid.Count - 1)
                    {
                        if (interestDue >= totalAmountToPay)
                        {
                            if (j == 1 && scheduleInput.MinDuration > Constants.MinDurationValue)
                            {
                                interestCarryOver = 0;
                                interestPayment = interestDue;
                                totalAmountToPay = interestDue;
                            }
                            else
                            {
                                interestCarryOver = Round.RoundOffAmount(interestDue - totalAmountToPay);
                                interestPayment = totalAmountToPay;
                            }
                            principalAmountToPay = 0;
                        }
                        else
                        {
                            interestPayment = interestDue;
                            if ((totalAmountToPay - interestDue) > payablePrincipal)
                            {
                                principalAmountToPay = payablePrincipal;
                            }
                            else
                            {
                                principalAmountToPay = totalAmountToPay - interestDue;
                            }
                            interestCarryOver = 0;
                            principalAmountToPay = Round.RoundOffAmount(principalAmountToPay);
                        }

                        #region Apply Enforcement Principal from givem Enforcement Payment.
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
                        interestPayment = interestDue;
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
                    double serviceFeeInterestDue = 0;
                    if (!scheduleInput.IsServiceFeeFinanced)
                    {
                        if (scheduleInput.ApplyServiceFeeInterest == 1 || scheduleInput.ApplyServiceFeeInterest == 3)
                        {
                            if (string.IsNullOrEmpty(scheduleInput.InputRecords[j].InterestRate))
                            {
                                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateSchedules() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, outputGrid[i].PaymentDate, scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true,true);");
                                accruedServiceFeeInterest = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, outputGrid[i].PaymentDate, scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true, true, false);
                            }
                            else
                            {
                                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ReCalculateSchedules() method. Calling method : PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, outputGrid[i].PaymentDate, Convert.ToDouble(scheduleInput.InputRecords[j].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true,true);");
                                accruedServiceFeeInterest = PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, outputGrid[i].PaymentDate, Convert.ToDouble(scheduleInput.InputRecords[j].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true, true, false);
                            }
                            accruedServiceFeeInterest = Round.RoundOffAmount(accruedServiceFeeInterest);
                        }
                    }

                    serviceFeeInterestDue = accruedServiceFeeInterest + serviceFeeInterestCarryOver;

                    //Checks whether the row is last row of output grid or payable loan amount will become 0 after this row.
                    if (i == outputGrid.Count - 1 || scheduleInput.ServiceFeeFirstPayment)
                    {
                        payableServiceFee = Round.RoundOffAmount(remainingServiceFee);
                        payableServiceFeeInterest = serviceFeeInterestDue;
                        totalServiceFeePayable = payableServiceFee + payableServiceFeeInterest;
                        serviceFeeInterestCarryOver = 0;
                    }
                    else
                    {
                        if (serviceFeeInterestDue >= totalServiceFeePayable)
                        {
                            if (i == 1 && scheduleInput.MinDuration > Constants.MinDurationValue)
                            {
                                serviceFeeInterestCarryOver = 0;
                                payableServiceFeeInterest = serviceFeeInterestDue;
                                totalServiceFeePayable = serviceFeeInterestDue;
                            }
                            else
                            {
                                serviceFeeInterestCarryOver = Round.RoundOffAmount(serviceFeeInterestDue - totalServiceFeePayable);
                                payableServiceFeeInterest = totalServiceFeePayable;
                            }
                            payableServiceFee = 0;
                        }
                        else
                        {
                            payableServiceFeeInterest = serviceFeeInterestDue;
                            if ((totalServiceFeePayable - serviceFeeInterestDue) > remainingServiceFee)
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

                    totalAmountToPay += totalServiceFeePayable + ((i == outputGrid.Count - 1) ? 0 :
                                                                                Round.RoundOffAmount((scheduleInput.EarnedOriginationFee + scheduleInput.EarnedSameDayFee +
                                                                scheduleInput.EarnedManagementFee + scheduleInput.EarnedMaintenanceFee +
                                                                scheduleInput.EarnedNSFFee + scheduleInput.EarnedLateFee) / (scheduleInput.InputRecords.Count - 1)));

                    if (totalAmountToPay > (interestPayment + payablePrincipal + remainingServiceFee + payableServiceFeeInterest +
                                    (outputGrid[i - 1].TotalPastDue - outputGrid[i - 1].PrincipalPastDue - outputGrid[i - 1].ServiceFeePastDue -
                                        outputGrid[i - 1].OriginationFeePastDue - outputGrid[i - 1].SameDayFeePastDue)))
                    {
                        totalAmountToPay = interestPayment + payablePrincipal + remainingServiceFee + payableServiceFeeInterest + (outputGrid[i - 1].TotalPastDue -
                                                    outputGrid[i - 1].PrincipalPastDue - outputGrid[i - 1].ServiceFeePastDue - outputGrid[i - 1].OriginationFeePastDue
                                                    - outputGrid[i - 1].SameDayFeePastDue);
                    }
                    //totalAmountToPay += outputGrid[i].OriginationFee + outputGrid[i].SameDayFee;
                    totalAmountToPay += (Round.RoundOffAmount(outputGrid[i].OriginationFee - scheduleInput.EarnedOriginationFee) <= 0 ? 0 :
                                            Round.RoundOffAmount(outputGrid[i].OriginationFee - scheduleInput.EarnedOriginationFee)) +
                                        (Round.RoundOffAmount(outputGrid[i].SameDayFee - scheduleInput.EarnedSameDayFee) <= 0 ? 0 :
                                            Round.RoundOffAmount(outputGrid[i].SameDayFee - scheduleInput.EarnedSameDayFee));

                    //After calculations assign all the values in the output grid.
                    outputGrid[i].BeginningPrincipal = Round.RoundOffAmount(remainingPrincipal);
                    outputGrid[i].BeginningServiceFee = Round.RoundOffAmount(remainingServiceFee);
                    outputGrid[i].PeriodicInterestRate = periodicInterestRate;
                    outputGrid[i].DailyInterestRate = dailyInterestRate;
                    outputGrid[i].DailyInterestAmount = dailyInterestAmount;
                    outputGrid[i].InterestAccrued = Round.RoundOffAmount(interestAccrued);
                    outputGrid[i].InterestDue = Round.RoundOffAmount(interestDue);
                    outputGrid[i].InterestCarryOver = Round.RoundOffAmount(interestCarryOver);
                    outputGrid[i].InterestPayment = Round.RoundOffAmount(interestPayment);
                    outputGrid[i].PrincipalPayment = Round.RoundOffAmount(principalAmountToPay);
                    outputGrid[i].TotalPayment = Round.RoundOffAmount(totalAmountToPay);
                    outputGrid[i].PaymentDue = Round.RoundOffAmount(totalAmountToPay);
                    outputGrid[i].AccruedServiceFeeInterest = Round.RoundOffAmount(accruedServiceFeeInterest);
                    outputGrid[i].ServiceFeeInterestDue = Round.RoundOffAmount(serviceFeeInterestDue);
                    outputGrid[i].AccruedServiceFeeInterestCarryOver = Round.RoundOffAmount(serviceFeeInterestCarryOver);
                    outputGrid[i].ServiceFee = Round.RoundOffAmount(payableServiceFee);
                    outputGrid[i].ServiceFeeInterest = Round.RoundOffAmount(payableServiceFeeInterest);
                    outputGrid[i].ServiceFeeTotal = Round.RoundOffAmount(payableServiceFee) + Round.RoundOffAmount(payableServiceFeeInterest);
                    outputGrid[i].ManagementFee = 0;
                    outputGrid[i].MaintenanceFee = 0;
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
                    outputGrid[i].NSFFeePastDue = 0;
                    outputGrid[i].LateFeePastDue = 0;
                    outputGrid[i].TotalPastDue = 0;

                    //Reduce the service fee amount that remains to be paid after current schedule.
                    remainingServiceFee = (remainingServiceFee - payableServiceFee) <= 0 ? 0 : (remainingServiceFee - payableServiceFee);
                    remainingServiceFee = Round.RoundOffAmount(remainingServiceFee);

                    //Reduce the principal amount that remains to pay after a particular period payment.
                    remainingPrincipal -= principalAmountToPay;
                    remainingPrincipal = Round.RoundOffAmount(remainingPrincipal);
                }

                sbTracing.AppendLine("Exist:From method name ReCalculateSchedules(double remainingPrincipal, double remainingServiceFee, List<OutputGrid> outputGrid, LoanDetails scheduleInput, int nextOutputRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double sameDayFee).");
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

        #region Principal Only Payment

        /// <summary>
        /// This function adds an principal only additional payment event to the output grid, that will pay only the principal amount.
        /// </summary>
        /// <param name="outputGrid"></param>
        /// <param name="scheduleInput"></param>
        /// <param name="additionalPaymentRow"></param>
        /// <param name="inputGridRow"></param>
        /// <param name="outputGridRow"></param>
        /// <param name="defaultTotalAmountToPay"></param>
        /// <param name="defaultTotalServiceFeePayable"></param>
        /// <param name="isExtendingSchedule"></param>
        /// <param name="originationFee"></param>
        /// <param name="sameDayFee"></param>
        /// <param name="managementFee"></param>
        /// <param name="maintenanceFee"></param>
        private static void PrincipalOnlyPayment(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int additionalPaymentRow, int inputGridRow, int outputGridRow,
                double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee,
                double managementFee, double maintenanceFee)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name PrincipalOnlyPayment(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int additionalPaymentRow, int inputGridRow, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee, double managementFee, double maintenanceFee).");
                sbTracing.AppendLine("Parameter values are : additionalPaymentRow = " + additionalPaymentRow + ", inputGridRow = " + inputGridRow + ", outputGridRow = " + outputGridRow + ", defaultTotalAmountToPay = " + defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", isExtendingSchedule = " + isExtendingSchedule + ", originationFee = " + originationFee + ", sameDayFee = " + sameDayFee + ", managementFee = " + managementFee + ", maintenanceFee = " + maintenanceFee);

                double remainingPrincipal = isExtendingSchedule ? (outputGrid[outputGridRow - 1].BeginningPrincipal - outputGrid[outputGridRow - 1].PrincipalPaid) :
                                                                        outputGrid[outputGridRow].BeginningPrincipal;
                double remainingServiceFee = isExtendingSchedule ? (outputGrid[outputGridRow - 1].BeginningServiceFee - outputGrid[outputGridRow - 1].ServiceFeePaid) :
                                                                            outputGrid[outputGridRow].BeginningServiceFee;

                double payablePrincipal = (remainingPrincipal < Round.RoundOffAmount(remainingPrincipal - scheduleInput.Residual) ? remainingPrincipal :
                                                                Round.RoundOffAmount(remainingPrincipal - scheduleInput.Residual));

                //Determine the last index in output grid where interest was accrued.
                int lastOutputRow = outputGrid.Take(outputGridRow).ToList().FindLastIndex(o => o.Flags != (int)Constants.FlagValues.ManagementFee &&
                                                                                                o.Flags != (int)Constants.FlagValues.MaintenanceFee);

                DateTime startDate;
                if (lastOutputRow == -1)
                {
                    startDate = scheduleInput.InputRecords[inputGridRow - 1].EffectiveDate == DateTime.MinValue ?
                                        scheduleInput.InputRecords[inputGridRow - 1].DateIn : scheduleInput.InputRecords[inputGridRow - 1].EffectiveDate;
                }
                else
                {
                    startDate = outputGrid[lastOutputRow].PaymentDate;
                }
                DateTime endDate = scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].DateIn;

                double periodicInterestRate;
                double interestAccrued;
                //This condition determines whether there is an interest rate in additional payment grid, for that row. If it is provided, that value will be used, otherwise
                //default value will be used for periodic interest calculations.
                if (string.IsNullOrEmpty(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate))
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PrincipalOnlyPayment() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true,isExtendingSchedule);");
                    periodicInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule, false);

                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PrincipalOnlyPayment() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true,isExtendingSchedule);");
                    interestAccrued = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule, false);
                }
                else
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PrincipalOnlyPayment() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, true,isExtendingSchedule);");
                    periodicInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, true, isExtendingSchedule, false);

                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PrincipalOnlyPayment() method. Calling method : PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true,isExtendingSchedule);");
                    interestAccrued = PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule, false);
                }

                interestAccrued = Round.RoundOffAmount(interestAccrued);

                //This variable calculates the daily interest amount for the period.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PrincipalOnlyPayment() method. Calling method : DailyInterestAmount.CalculateDailyInterestAmount(startDate, endDate, interestAccrued, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, isExtendingSchedule);");
                double dailyInterestAmount = DailyInterestAmount.CalculateDailyInterestAmount(startDate, endDate, interestAccrued, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, scheduleInput, isExtendingSchedule);
                dailyInterestAmount = Round.RoundOffAmount(dailyInterestAmount);

                //Add the earned interest amount to the accrued value.
                interestAccrued += (outputGridRow == 0 ? scheduleInput.EarnedInterest : 0);

                //It determines the total interest to be paid.
                double interestDue = interestAccrued + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].InterestCarryOver);

                double serviceFeeInterestAccrued = 0;
                //Checks whether service fee interest is to be taken or not.
                if (scheduleInput.ApplyServiceFeeInterest == 1 || scheduleInput.ApplyServiceFeeInterest == 3)
                {
                    //Calculate the accrued service fee interest value.
                    if (string.IsNullOrEmpty(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate))
                    {
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PrincipalOnlyPayment() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true, isExtendingSchedule);");
                        serviceFeeInterestAccrued = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true, isExtendingSchedule, false);
                    }
                    else
                    {
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PrincipalOnlyPayment() method. Calling method : PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true, isExtendingSchedule);");
                        serviceFeeInterestAccrued = PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true, isExtendingSchedule, false);
                    }
                    serviceFeeInterestAccrued = Round.RoundOffAmount(serviceFeeInterestAccrued);
                }

                //Add earned service fee interest value to the accrued value.
                serviceFeeInterestAccrued += (outputGridRow == 0 ? scheduleInput.EarnedServiceFeeInterest : 0);
                //It determines the total interest to be paid.
                double serviceFeeInterestDue = serviceFeeInterestAccrued + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].AccruedServiceFeeInterestCarryOver);

                //It calculates the remaining principal amount that will be paid after deducting the additional payment amount.
                double principalAmountToPay = Round.RoundOffAmount(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment > payablePrincipal ?
                                                                payablePrincipal : scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment);

                //This variable calculates the daily interest rate for the period.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PrincipalOnlyPayment() method. Calling method : DailyInterestRate.CalculateDailyInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, isExtendingSchedule);");
                double dailyInterestRate = DailyInterestRate.CalculateDailyInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, scheduleInput, isExtendingSchedule);

                #region Inserting the additional payment row

                //It adds a new row on the selected indes i.e. "outputGridRow" in the output grid.
                outputGrid.Insert(outputGridRow,
                        new OutputGrid
                        {
                            PaymentDate = endDate,
                            BeginningPrincipal = remainingPrincipal,
                            BeginningServiceFee = remainingServiceFee,
                            PeriodicInterestRate = periodicInterestRate,
                            DailyInterestRate = dailyInterestRate,
                            DailyInterestAmount = dailyInterestAmount,
                            PaymentID = Convert.ToInt32(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].PaymentID.ToString()),
                            Flags = Convert.ToInt32(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].Flags.ToString()),
                            DueDate = endDate,
                            InterestAccrued = interestAccrued,
                            InterestDue = interestDue,
                            InterestCarryOver = interestDue,
                            InterestPayment = 0,
                            PrincipalPayment = 0,
                            TotalPayment = 0,
                            PaymentDue = 0,
                            AccruedServiceFeeInterest = serviceFeeInterestAccrued,
                            ServiceFeeInterestDue = serviceFeeInterestDue,
                            AccruedServiceFeeInterestCarryOver = serviceFeeInterestDue,
                            ServiceFee = 0,
                            ServiceFeeInterest = 0,
                            ServiceFeeTotal = 0,
                            OriginationFee = (outputGridRow == 0 ? scheduleInput.EarnedOriginationFee : 0),
                            MaintenanceFee = (outputGridRow == 0 ? scheduleInput.EarnedMaintenanceFee : 0) + maintenanceFee,
                            ManagementFee = (outputGridRow == 0 ? scheduleInput.EarnedManagementFee : 0) + managementFee,
                            SameDayFee = (outputGridRow == 0 ? scheduleInput.EarnedSameDayFee : 0),
                            NSFFee = (outputGridRow == 0 ? scheduleInput.EarnedNSFFee : 0),
                            LateFee = (outputGridRow == 0 ? scheduleInput.EarnedLateFee : 0),
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
                            CumulativePrincipal = principalAmountToPay + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePrincipal),
                            CumulativePayment = principalAmountToPay + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePayment),
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
                            PrincipalPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].PrincipalPastDue,
                            InterestPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].InterestPastDue,
                            ServiceFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].ServiceFeePastDue,
                            ServiceFeeInterestPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].ServiceFeeInterestPastDue,
                            OriginationFeePastDue = outputGridRow == 0 ? scheduleInput.EarnedOriginationFee : outputGrid[outputGridRow - 1].OriginationFeePastDue,
                            MaintenanceFeePastDue = (outputGridRow == 0 ? scheduleInput.EarnedMaintenanceFee : outputGrid[outputGridRow - 1].MaintenanceFeePastDue) + maintenanceFee,
                            ManagementFeePastDue = (outputGridRow == 0 ? scheduleInput.EarnedManagementFee : outputGrid[outputGridRow - 1].ManagementFeePastDue) + managementFee,
                            SameDayFeePastDue = outputGridRow == 0 ? scheduleInput.EarnedSameDayFee : outputGrid[outputGridRow - 1].SameDayFeePastDue,
                            NSFFeePastDue = outputGridRow == 0 ? scheduleInput.EarnedNSFFee : outputGrid[outputGridRow - 1].NSFFeePastDue,
                            LateFeePastDue = outputGridRow == 0 ? scheduleInput.EarnedLateFee : outputGrid[outputGridRow - 1].LateFeePastDue,
                            TotalPastDue = (outputGridRow == 0 ? (scheduleInput.EarnedOriginationFee + scheduleInput.EarnedSameDayFee + scheduleInput.EarnedManagementFee +
                                                                    scheduleInput.EarnedMaintenanceFee + scheduleInput.EarnedNSFFee + scheduleInput.EarnedLateFee) :
                                                                        outputGrid[outputGridRow - 1].TotalPastDue) +
                                                                    managementFee + maintenanceFee,
                            //Cumulative past due amount columns
                            CumulativePrincipalPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePrincipalPastDue,
                            CumulativeInterestPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeInterestPastDue,
                            CumulativeServiceFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeePastDue,
                            CumulativeServiceFeeInterestPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeInterestPastDue,
                            CumulativeOriginationFeePastDue = outputGridRow == 0 ? scheduleInput.EarnedOriginationFee : outputGrid[outputGridRow - 1].CumulativeOriginationFeePastDue,
                            CumulativeMaintenanceFeePastDue = maintenanceFee + (outputGridRow == 0 ? scheduleInput.EarnedMaintenanceFee : outputGrid[outputGridRow - 1].CumulativeMaintenanceFeePastDue),
                            CumulativeManagementFeePastDue = managementFee + (outputGridRow == 0 ? scheduleInput.EarnedManagementFee : outputGrid[outputGridRow - 1].CumulativeManagementFeePastDue),
                            CumulativeSameDayFeePastDue = outputGridRow == 0 ? scheduleInput.EarnedSameDayFee : outputGrid[outputGridRow - 1].CumulativeSameDayFeePastDue,
                            CumulativeNSFFeePastDue = outputGridRow == 0 ? scheduleInput.EarnedNSFFee : outputGrid[outputGridRow - 1].CumulativeNSFFeePastDue,
                            CumulativeLateFeePastDue = outputGridRow == 0 ? scheduleInput.EarnedLateFee : outputGrid[outputGridRow - 1].CumulativeLateFeePastDue,
                            CumulativeTotalPastDue = managementFee + maintenanceFee +
                                                     (outputGridRow == 0 ? (scheduleInput.EarnedOriginationFee + scheduleInput.EarnedSameDayFee +
                                                                            scheduleInput.EarnedManagementFee + scheduleInput.EarnedMaintenanceFee +
                                                                            scheduleInput.EarnedNSFFee + scheduleInput.EarnedLateFee) : outputGrid[outputGridRow - 1].CumulativeTotalPastDue),
                        });

                #endregion

                //Determine the additional payment value that is being paid in the principal only payment event.
                double additionalPayment = scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment;

                if (outputGridRow != 0)
                {
                    //Determine the amount that is being deducted from the principal past due amount.
                    double deductedPrincipal = (outputGrid[outputGridRow].PrincipalPastDue - additionalPayment) <= 0 ? outputGrid[outputGridRow].PrincipalPastDue : additionalPayment;
                    outputGrid[outputGridRow].PrincipalPastDue = Round.RoundOffAmount(outputGrid[outputGridRow].PrincipalPastDue - deductedPrincipal);
                    outputGrid[outputGridRow].TotalPastDue = Round.RoundOffAmount(outputGrid[outputGridRow].TotalPastDue - deductedPrincipal);
                    outputGrid[outputGridRow].CumulativePrincipalPastDue = Round.RoundOffAmount(outputGrid[outputGridRow].CumulativePrincipalPastDue - deductedPrincipal);
                    outputGrid[outputGridRow].CumulativeTotalPastDue = Round.RoundOffAmount(outputGrid[outputGridRow].CumulativeTotalPastDue - deductedPrincipal);
                }

                remainingPrincipal = Round.RoundOffAmount(remainingPrincipal - principalAmountToPay);

                //Recalculates the output grid values for the remaining scheduled dates if the aditional payment is not after last scheduled payment date.
                if (!isExtendingSchedule)
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and PrincipalOnlyPayment() method. Calling method : ReCalculateSchedules(remainingPrincipal, remainingServiceFee, outputGrid, scheduleInput, outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);");
                    ReCalculateSchedules(remainingPrincipal, remainingServiceFee, outputGrid, scheduleInput, outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);
                }

                sbTracing.AppendLine("Exist:From method name PrincipalOnlyPayment(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int additionalPaymentRow, int inputGridRow, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee, double managementFee, double maintenanceFee).");
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
        /// <param name="scheduleInput"></param>
        /// <param name="additionalPaymentRow"></param>
        /// <param name="inputGridRow"></param>
        /// <param name="outputGridRow"></param>
        /// <param name="defaultTotalAmountToPay"></param>
        /// <param name="defaultTotalServiceFeePayable"></param>
        /// <param name="isExtendingSchedule"></param>
        /// <param name="originationFee"></param>
        /// <param name="sameDayFee"></param>
        /// <param name="managementFee"></param>
        /// <param name="maintenanceFee"></param>
        private static void AdditionalPayment(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int additionalPaymentRow, int inputGridRow, int outputGridRow,
                double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee,
                double managementFee, double maintenanceFee)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name AdditionalPayment(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int additionalPaymentRow, int inputGridRow, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee, double managementFee, double maintenanceFee).");
                sbTracing.AppendLine("Parameter values are : additionalPaymentRow = " + additionalPaymentRow + ", inputGridRow = " + inputGridRow + ", outputGridRow = " + outputGridRow + ", defaultTotalAmountToPay = " + defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", isExtendingSchedule = " + isExtendingSchedule + ", originationFee = " + originationFee + ", sameDayFee = " + sameDayFee + ", managementFee = " + managementFee + ", maintenanceFee = " + maintenanceFee);

                double remainingPrincipal = isExtendingSchedule ? (outputGrid[outputGridRow - 1].BeginningPrincipal - outputGrid[outputGridRow - 1].PrincipalPaid) :
                                                                    outputGrid[outputGridRow].BeginningPrincipal;
                double remainingServiceFee = isExtendingSchedule ? (outputGrid[outputGridRow - 1].BeginningServiceFee - outputGrid[outputGridRow - 1].ServiceFeePaid) :
                                                                        outputGrid[outputGridRow].BeginningServiceFee;
                double remainingOriginationFee = originationFee;
                double remainingSameDayFee = sameDayFee;
                for (int i = 0; i < outputGridRow; i++)
                {
                    remainingOriginationFee = (remainingOriginationFee - outputGrid[i].OriginationFeePaid) < 0 ? 0 : Round.RoundOffAmount(remainingOriginationFee - outputGrid[i].OriginationFeePaid);
                    remainingSameDayFee = (remainingSameDayFee - outputGrid[i].SameDayFeePaid) < 0 ? 0 : Round.RoundOffAmount(remainingSameDayFee - outputGrid[i].SameDayFeePaid);
                }

                //Determine the last index in output grid where interest was accrued.
                int lastOutputRow = outputGrid.Take(outputGridRow).ToList().FindLastIndex(o => o.Flags != (int)Constants.FlagValues.ManagementFee &&
                                                                                                o.Flags != (int)Constants.FlagValues.MaintenanceFee);

                DateTime startDate;
                if (lastOutputRow == -1)
                {
                    startDate = scheduleInput.InputRecords[inputGridRow - 1].EffectiveDate == DateTime.MinValue ?
                                        scheduleInput.InputRecords[inputGridRow - 1].DateIn : scheduleInput.InputRecords[inputGridRow - 1].EffectiveDate;
                }
                else
                {
                    startDate = outputGrid[lastOutputRow].PaymentDate;
                }
                DateTime endDate = scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].DateIn;

                double periodicInterestRate;
                double interestAccrued;
                //This condition determines whether there is an interest rate in additional payment grid, for that row. If it is provided, that value will be used, otherwise
                //default value will be used for periodic interest calculations.
                if (string.IsNullOrEmpty(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate))
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AdditionalPayment() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule);");
                    periodicInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule, false);

                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AdditionalPayment() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule);");
                    interestAccrued = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule, false);
                }
                else
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AdditionalPayment() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, true, isExtendingSchedule);");
                    periodicInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, true, isExtendingSchedule, false);

                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AdditionalPayment() method. Calling method : PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule);");
                    interestAccrued = PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule, false);
                }

                interestAccrued = Round.RoundOffAmount(interestAccrued);

                //This variable calculates the daily interest amount for the period.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AdditionalPayment() method. Calling method : DailyInterestAmount.CalculateDailyInterestAmount(startDate, endDate, interestAccrued, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay,scheduleInput, isExtendingSchedule);");
                double dailyInterestAmount = DailyInterestAmount.CalculateDailyInterestAmount(startDate, endDate, interestAccrued, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, scheduleInput, isExtendingSchedule);
                dailyInterestAmount = Round.RoundOffAmount(dailyInterestAmount);

                //Add the earned interest amount to the accrued value.
                interestAccrued += (outputGridRow == 0 ? scheduleInput.EarnedInterest : 0);

                //It determines the total interest carry over to be paid.
                double interestDue = interestAccrued + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].InterestCarryOver);

                double serviceFeeInterestAccrued = 0;
                //Checks whether service fee interest is to be taken or not.
                if (scheduleInput.ApplyServiceFeeInterest == 1 || scheduleInput.ApplyServiceFeeInterest == 3)
                {
                    //Calculate the accrued service fee interest value.
                    if (string.IsNullOrEmpty(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate))
                    {
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AdditionalPayment() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true, isExtendingSchedule);");
                        serviceFeeInterestAccrued = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true, isExtendingSchedule, false);
                    }
                    else
                    {
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AdditionalPayment() method. Calling method : PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true, isExtendingSchedule);");
                        serviceFeeInterestAccrued = PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true, isExtendingSchedule, false);
                    }

                    serviceFeeInterestAccrued = Round.RoundOffAmount(serviceFeeInterestAccrued);
                }

                //Add earned service fee interest value to the accrued value.
                serviceFeeInterestAccrued += (outputGridRow == 0 ? scheduleInput.EarnedServiceFeeInterest : 0);
                //It determines the total interest carry over to be paid.
                double serviceFeeInterestDue = serviceFeeInterestAccrued + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].AccruedServiceFeeInterestCarryOver);

                //This variable calculates the daily interest rate for the period.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AdditionalPayment() method. Calling method : DailyInterestRate.CalculateDailyInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay,scheduleInput, isExtendingSchedule);");
                double dailyInterestRate = DailyInterestRate.CalculateDailyInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, scheduleInput, isExtendingSchedule);

                #region Inserting the additional payment row

                //It adds a new row on the selected indes i.e. "outputGridRow" in the output grid.
                outputGrid.Insert(outputGridRow,
                        new OutputGrid
                        {
                            PaymentDate = endDate,
                            BeginningPrincipal = remainingPrincipal,
                            BeginningServiceFee = remainingServiceFee,
                            PeriodicInterestRate = periodicInterestRate,
                            DailyInterestRate = dailyInterestRate,
                            DailyInterestAmount = dailyInterestAmount,
                            PaymentID = Convert.ToInt32(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].PaymentID.ToString()),
                            Flags = Convert.ToInt32(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].Flags.ToString()),
                            DueDate = endDate,
                            InterestAccrued = interestAccrued,
                            InterestDue = interestDue,
                            InterestCarryOver = 0,
                            InterestPayment = 0,
                            PrincipalPayment = 0,
                            TotalPayment = 0,
                            PaymentDue = 0,
                            AccruedServiceFeeInterest = serviceFeeInterestAccrued,
                            ServiceFeeInterestDue = serviceFeeInterestDue,
                            AccruedServiceFeeInterestCarryOver = 0,
                            ServiceFee = 0,
                            ServiceFeeInterest = 0,
                            ServiceFeeTotal = 0,
                            OriginationFee = remainingOriginationFee,
                            MaintenanceFee = (outputGridRow == 0 ? scheduleInput.EarnedMaintenanceFee : 0) + maintenanceFee,
                            ManagementFee = (outputGridRow == 0 ? scheduleInput.EarnedManagementFee : 0) + managementFee,
                            SameDayFee = remainingSameDayFee,
                            NSFFee = (outputGridRow == 0 ? scheduleInput.EarnedNSFFee : 0),
                            LateFee = (outputGridRow == 0 ? scheduleInput.EarnedLateFee : 0),
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
                            NSFFeePastDue = 0,
                            LateFeePastDue = 0,
                            TotalPastDue = 0,
                            //Cumulative past due amount columns
                            CumulativePrincipalPastDue = 0,
                            CumulativeInterestPastDue = 0,
                            CumulativeServiceFeePastDue = 0,
                            CumulativeServiceFeeInterestPastDue = 0,
                            CumulativeOriginationFeePastDue = 0,
                            CumulativeMaintenanceFeePastDue = 0,
                            CumulativeManagementFeePastDue = 0,
                            CumulativeSameDayFeePastDue = 0,
                            CumulativeNSFFeePastDue = 0,
                            CumulativeLateFeePastDue = 0,
                            CumulativeTotalPastDue = 0
                        });

                #endregion

                //Determine the payment amount for the current additional payment bucket which will be distributed among all the components.
                double paymentAmount = Round.RoundOffAmount(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment);

                #region Pay current schedule row

                for (int i = 1; i <= 10; i++)
                {
                    if (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestPriority == i)
                    {
                        if (outputGrid[outputGridRow].InterestDue >= paymentAmount)
                        {
                            outputGrid[outputGridRow].InterestPaid = paymentAmount;
                            outputGrid[outputGridRow].InterestPayment = paymentAmount;
                            outputGrid[outputGridRow].InterestCarryOver = Round.RoundOffAmount(outputGrid[outputGridRow].InterestDue - paymentAmount);
                            paymentAmount = 0;
                        }
                        else
                        {
                            outputGrid[outputGridRow].InterestPaid = outputGrid[outputGridRow].InterestDue;
                            outputGrid[outputGridRow].InterestPayment = outputGrid[outputGridRow].InterestDue;
                            outputGrid[outputGridRow].InterestCarryOver = 0;
                            paymentAmount -= outputGrid[outputGridRow].InterestDue;
                        }
                    }
                    else if (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].ServiceFeeInterestPriority == i)
                    {
                        if (outputGrid[outputGridRow].ServiceFeeInterestDue >= paymentAmount)
                        {
                            outputGrid[outputGridRow].ServiceFeeInterestPaid = paymentAmount;
                            outputGrid[outputGridRow].ServiceFeeInterest = paymentAmount;
                            outputGrid[outputGridRow].AccruedServiceFeeInterestCarryOver = Round.RoundOffAmount(outputGrid[outputGridRow].ServiceFeeInterestDue - paymentAmount);
                            paymentAmount = 0;
                        }
                        else
                        {
                            outputGrid[outputGridRow].ServiceFeeInterestPaid = outputGrid[outputGridRow].ServiceFeeInterestDue;
                            outputGrid[outputGridRow].ServiceFeeInterest = outputGrid[outputGridRow].ServiceFeeInterestDue;
                            outputGrid[outputGridRow].AccruedServiceFeeInterestCarryOver = 0;
                            paymentAmount -= outputGrid[outputGridRow].ServiceFeeInterestDue;
                        }
                    }
                    else if (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].OriginationFeePriority == i)
                    {
                        if (outputGrid[outputGridRow].OriginationFee >= paymentAmount)
                        {
                            outputGrid[outputGridRow].OriginationFeePaid = paymentAmount;
                            paymentAmount = 0;
                        }
                        else
                        {
                            outputGrid[outputGridRow].OriginationFeePaid = outputGrid[outputGridRow].OriginationFee;
                            paymentAmount -= outputGrid[outputGridRow].OriginationFee;
                        }
                    }
                    else if (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].SameDayFeePriority == i)
                    {
                        if (outputGrid[outputGridRow].SameDayFee >= paymentAmount)
                        {
                            outputGrid[outputGridRow].SameDayFeePaid = paymentAmount;
                            paymentAmount = 0;
                        }
                        else
                        {
                            outputGrid[outputGridRow].SameDayFeePaid = outputGrid[outputGridRow].SameDayFee;
                            paymentAmount -= outputGrid[outputGridRow].SameDayFee;
                        }
                    }
                    else if (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].ManagementFeePriority == i)
                    {
                        if (outputGrid[outputGridRow].ManagementFee >= paymentAmount)
                        {
                            outputGrid[outputGridRow].ManagementFeePaid = paymentAmount;
                            paymentAmount = 0;
                        }
                        else
                        {
                            outputGrid[outputGridRow].ManagementFeePaid = outputGrid[outputGridRow].ManagementFee;
                            paymentAmount -= outputGrid[outputGridRow].ManagementFee;
                        }
                    }
                    else if (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].MaintenanceFeePriority == i)
                    {
                        if (outputGrid[outputGridRow].MaintenanceFee >= paymentAmount)
                        {
                            outputGrid[outputGridRow].MaintenanceFeePaid = paymentAmount;
                            paymentAmount = 0;
                        }
                        else
                        {
                            outputGrid[outputGridRow].MaintenanceFeePaid = outputGrid[outputGridRow].MaintenanceFee;
                            paymentAmount -= outputGrid[outputGridRow].MaintenanceFee;
                        }
                    }
                    else if (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].NSFFeePriority == i)
                    {
                        if (outputGrid[outputGridRow].NSFFee >= paymentAmount)
                        {
                            outputGrid[outputGridRow].NSFFeePaid = paymentAmount;
                            paymentAmount = 0;
                        }
                        else
                        {
                            outputGrid[outputGridRow].NSFFeePaid = outputGrid[outputGridRow].NSFFee;
                            paymentAmount -= outputGrid[outputGridRow].NSFFee;
                        }
                    }
                    else if (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].LateFeePriority == i)
                    {
                        if (outputGrid[outputGridRow].LateFee >= paymentAmount)
                        {
                            outputGrid[outputGridRow].LateFeePaid = paymentAmount;
                            paymentAmount = 0;
                        }
                        else
                        {
                            outputGrid[outputGridRow].LateFeePaid = outputGrid[outputGridRow].LateFee;
                            paymentAmount -= outputGrid[outputGridRow].LateFee;
                        }
                    }

                    paymentAmount = Round.RoundOffAmount(paymentAmount);
                }

                #endregion

                outputGrid[outputGridRow].OriginationFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].OriginationFee - outputGrid[outputGridRow].OriginationFeePaid);
                outputGrid[outputGridRow].SameDayFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].SameDayFee - outputGrid[outputGridRow].SameDayFeePaid);
                outputGrid[outputGridRow].ManagementFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].ManagementFee - outputGrid[outputGridRow].ManagementFeePaid);
                outputGrid[outputGridRow].MaintenanceFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].MaintenanceFee - outputGrid[outputGridRow].MaintenanceFeePaid);
                outputGrid[outputGridRow].NSFFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].NSFFee - outputGrid[outputGridRow].NSFFeePaid);
                outputGrid[outputGridRow].LateFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].LateFee - outputGrid[outputGridRow].LateFeePaid);
                outputGrid[outputGridRow].ServiceFeeTotal = outputGrid[outputGridRow].ServiceFeeInterest;

                #region Pay Past due amount

                if (outputGridRow != 0)
                {
                    double pastDuePrincipal = (outputGrid[outputGridRow].BeginningPrincipal < outputGrid[outputGridRow - 1].PrincipalPastDue) ?
                                                    outputGrid[outputGridRow].BeginningPrincipal : outputGrid[outputGridRow - 1].PrincipalPastDue;
                    double pastDueServiceFee = (outputGrid[outputGridRow].BeginningServiceFee < outputGrid[outputGridRow - 1].ServiceFeePastDue) ?
                                                    outputGrid[outputGridRow].BeginningServiceFee : outputGrid[outputGridRow - 1].ServiceFeePastDue;

                    pastDuePrincipal = (pastDuePrincipal < Round.RoundOffAmount(remainingPrincipal - scheduleInput.Residual) ? pastDuePrincipal :
                                                           Round.RoundOffAmount(remainingPrincipal - scheduleInput.Residual));

                    if (paymentAmount >= Round.RoundOffAmount(outputGrid[outputGridRow - 1].TotalPastDue - outputGrid[outputGridRow - 1].PrincipalPastDue -
                                                    outputGrid[outputGridRow - 1].ServiceFeePastDue - outputGrid[outputGridRow - 1].OriginationFeePastDue -
                                                    outputGrid[outputGridRow - 1].SameDayFeePastDue + pastDuePrincipal + pastDueServiceFee))
                    {
                        outputGrid[outputGridRow].PrincipalPaid += pastDuePrincipal;
                        outputGrid[outputGridRow].InterestPaid += outputGrid[outputGridRow - 1].InterestPastDue;
                        outputGrid[outputGridRow].ServiceFeePaid += pastDueServiceFee;
                        outputGrid[outputGridRow].ServiceFeeInterestPaid += outputGrid[outputGridRow - 1].ServiceFeeInterestPastDue;
                        outputGrid[outputGridRow].ManagementFeePaid += outputGrid[outputGridRow - 1].ManagementFeePastDue;
                        outputGrid[outputGridRow].MaintenanceFeePaid += outputGrid[outputGridRow - 1].MaintenanceFeePastDue;
                        outputGrid[outputGridRow].NSFFeePaid += outputGrid[outputGridRow - 1].NSFFeePastDue;
                        outputGrid[outputGridRow].LateFeePaid += outputGrid[outputGridRow - 1].LateFeePastDue;

                        paymentAmount = Round.RoundOffAmount(paymentAmount - (pastDuePrincipal + outputGrid[outputGridRow - 1].InterestPastDue +
                                                                        pastDueServiceFee + outputGrid[outputGridRow - 1].ServiceFeeInterestPastDue +
                                                                        outputGrid[outputGridRow - 1].ManagementFeePastDue + outputGrid[outputGridRow - 1].MaintenanceFeePastDue +
                                                                        outputGrid[outputGridRow - 1].NSFFeePastDue + outputGrid[outputGridRow - 1].LateFeePastDue));
                    }
                    else
                    {
                        for (int i = 1; i <= 10; i++)
                        {
                            if (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].PrincipalPriority == i)
                            {
                                if (pastDuePrincipal >= paymentAmount)
                                {
                                    outputGrid[outputGridRow].PrincipalPaid += paymentAmount;
                                    outputGrid[outputGridRow].PrincipalPastDue += Round.RoundOffAmount(pastDuePrincipal - paymentAmount);
                                    outputGrid[outputGridRow].PrincipalPastDue = (outputGrid[outputGridRow].PrincipalPastDue < Round.RoundOffAmount(remainingPrincipal - scheduleInput.Residual) ?
                                                                                    outputGrid[outputGridRow].PrincipalPastDue : Round.RoundOffAmount(remainingPrincipal - scheduleInput.Residual));
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].PrincipalPaid += pastDuePrincipal;
                                    paymentAmount -= pastDuePrincipal;
                                }
                            }
                            else if (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestPriority == i)
                            {
                                if (outputGrid[outputGridRow - 1].InterestPastDue >= paymentAmount)
                                {
                                    outputGrid[outputGridRow].InterestPaid += paymentAmount;
                                    outputGrid[outputGridRow].InterestPastDue += Round.RoundOffAmount(outputGrid[outputGridRow - 1].InterestPastDue - paymentAmount);
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].InterestPaid += outputGrid[outputGridRow - 1].InterestPastDue;
                                    paymentAmount -= outputGrid[outputGridRow - 1].InterestPastDue;
                                }
                            }
                            else if (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].ServiceFeePriority == i)
                            {
                                if (pastDueServiceFee >= paymentAmount)
                                {
                                    outputGrid[outputGridRow].ServiceFeePaid += paymentAmount;
                                    outputGrid[outputGridRow].ServiceFeePastDue += Round.RoundOffAmount(pastDueServiceFee - paymentAmount);
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].ServiceFeePaid += pastDueServiceFee;
                                    paymentAmount -= pastDueServiceFee;
                                }
                            }
                            else if (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].ServiceFeeInterestPriority == i)
                            {
                                if (outputGrid[outputGridRow - 1].ServiceFeeInterestPastDue >= paymentAmount)
                                {
                                    outputGrid[outputGridRow].ServiceFeeInterestPaid += paymentAmount;
                                    outputGrid[outputGridRow].ServiceFeeInterestPastDue += Round.RoundOffAmount(outputGrid[outputGridRow - 1].ServiceFeeInterestPastDue - paymentAmount);
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].ServiceFeeInterestPaid += outputGrid[outputGridRow - 1].ServiceFeeInterestPastDue;
                                    paymentAmount -= outputGrid[outputGridRow - 1].ServiceFeeInterestPastDue;
                                }
                            }
                            else if (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].ManagementFeePriority == i)
                            {
                                if (outputGrid[outputGridRow - 1].ManagementFeePastDue >= paymentAmount)
                                {
                                    outputGrid[outputGridRow].ManagementFeePaid += paymentAmount;
                                    outputGrid[outputGridRow].ManagementFeePastDue += Round.RoundOffAmount(outputGrid[outputGridRow - 1].ManagementFeePastDue - paymentAmount);
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].ManagementFeePaid += outputGrid[outputGridRow - 1].ManagementFeePastDue;
                                    paymentAmount -= outputGrid[outputGridRow - 1].ManagementFeePastDue;
                                }
                            }
                            else if (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].MaintenanceFeePriority == i)
                            {
                                if (outputGrid[outputGridRow - 1].MaintenanceFeePastDue >= paymentAmount)
                                {
                                    outputGrid[outputGridRow].MaintenanceFeePaid += paymentAmount;
                                    outputGrid[outputGridRow].MaintenanceFeePastDue += Round.RoundOffAmount(outputGrid[outputGridRow - 1].MaintenanceFeePastDue - paymentAmount);
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].MaintenanceFeePaid += outputGrid[outputGridRow - 1].MaintenanceFeePastDue;
                                    paymentAmount -= outputGrid[outputGridRow - 1].MaintenanceFeePastDue;
                                }
                            }
                            else if (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].NSFFeePriority == i)
                            {
                                if (outputGrid[outputGridRow - 1].NSFFeePastDue >= paymentAmount)
                                {
                                    outputGrid[outputGridRow].NSFFeePaid += paymentAmount;
                                    outputGrid[outputGridRow].NSFFeePastDue += Round.RoundOffAmount(outputGrid[outputGridRow - 1].NSFFeePastDue - paymentAmount);
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].NSFFeePaid += outputGrid[outputGridRow - 1].NSFFeePastDue;
                                    paymentAmount -= outputGrid[outputGridRow - 1].NSFFeePastDue;
                                }
                            }
                            else if (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].LateFeePriority == i)
                            {
                                if (outputGrid[outputGridRow - 1].LateFeePastDue >= paymentAmount)
                                {
                                    outputGrid[outputGridRow].LateFeePaid += paymentAmount;
                                    outputGrid[outputGridRow].LateFeePastDue += Round.RoundOffAmount(outputGrid[outputGridRow - 1].LateFeePastDue - paymentAmount);
                                    paymentAmount = 0;
                                }
                                else
                                {
                                    outputGrid[outputGridRow].LateFeePaid += outputGrid[outputGridRow - 1].LateFeePastDue;
                                    paymentAmount -= outputGrid[outputGridRow - 1].LateFeePastDue;
                                }
                            }

                            paymentAmount = Round.RoundOffAmount(paymentAmount);
                        }
                    }
                }

                #endregion

                outputGrid[outputGridRow].TotalPastDue = outputGrid[outputGridRow].PrincipalPastDue + outputGrid[outputGridRow].InterestPastDue +
                                                            outputGrid[outputGridRow].ServiceFeePastDue + outputGrid[outputGridRow].ServiceFeeInterestPastDue +
                                                            outputGrid[outputGridRow].OriginationFeePastDue + outputGrid[outputGridRow].SameDayFeePastDue +
                                                            outputGrid[outputGridRow].ManagementFeePastDue + outputGrid[outputGridRow].MaintenanceFeePastDue +
                                                            outputGrid[outputGridRow].NSFFeePastDue + outputGrid[outputGridRow].LateFeePastDue;

                outputGrid[outputGridRow].CumulativePrincipalPastDue = outputGrid[outputGridRow].PrincipalPastDue;
                outputGrid[outputGridRow].CumulativeInterestPastDue = outputGrid[outputGridRow].InterestPastDue;
                outputGrid[outputGridRow].CumulativeServiceFeePastDue = outputGrid[outputGridRow].ServiceFeePastDue;
                outputGrid[outputGridRow].CumulativeServiceFeeInterestPastDue = outputGrid[outputGridRow].ServiceFeeInterestPastDue;
                outputGrid[outputGridRow].CumulativeOriginationFeePastDue = outputGrid[outputGridRow].OriginationFeePastDue;
                outputGrid[outputGridRow].CumulativeSameDayFeePastDue = outputGrid[outputGridRow].SameDayFeePastDue;
                outputGrid[outputGridRow].CumulativeManagementFeePastDue = outputGrid[outputGridRow].ManagementFeePastDue;
                outputGrid[outputGridRow].CumulativeMaintenanceFeePastDue = outputGrid[outputGridRow].MaintenanceFeePastDue;
                outputGrid[outputGridRow].CumulativeNSFFeePastDue = outputGrid[outputGridRow].NSFFeePastDue;
                outputGrid[outputGridRow].CumulativeLateFeePastDue = outputGrid[outputGridRow].LateFeePastDue;
                outputGrid[outputGridRow].CumulativeTotalPastDue = outputGrid[outputGridRow].TotalPastDue;

                remainingPrincipal = Round.RoundOffAmount(outputGrid[outputGridRow].BeginningPrincipal - outputGrid[outputGridRow].PrincipalPaid);
                remainingServiceFee = Round.RoundOffAmount(outputGrid[outputGridRow].BeginningServiceFee - outputGrid[outputGridRow].ServiceFeePaid);

                double payablePrincipal = (remainingPrincipal < Round.RoundOffAmount(remainingPrincipal - scheduleInput.Residual) ? remainingPrincipal :
                                                            Round.RoundOffAmount(remainingPrincipal - scheduleInput.Residual));

                #region Extra payment

                if (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].PrincipalPriority < scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].ServiceFeePriority)
                {
                    if (payablePrincipal >= paymentAmount)
                    {
                        outputGrid[outputGridRow].PrincipalPaid += paymentAmount;
                        remainingPrincipal -= paymentAmount;
                        paymentAmount = 0;
                    }
                    else
                    {
                        outputGrid[outputGridRow].PrincipalPaid += payablePrincipal;
                        paymentAmount -= payablePrincipal;
                        remainingPrincipal -= payablePrincipal;
                    }

                    paymentAmount = Round.RoundOffAmount(paymentAmount);

                    if (remainingServiceFee >= paymentAmount)
                    {
                        outputGrid[outputGridRow].ServiceFeePaid += paymentAmount;
                        remainingServiceFee -= paymentAmount;
                    }
                    else
                    {
                        outputGrid[outputGridRow].ServiceFeePaid += remainingServiceFee;
                        remainingServiceFee = 0;
                    }
                }
                else
                {
                    if (remainingServiceFee >= paymentAmount)
                    {
                        outputGrid[outputGridRow].ServiceFeePaid += paymentAmount;
                        remainingServiceFee -= paymentAmount;
                        paymentAmount = 0;
                    }
                    else
                    {
                        outputGrid[outputGridRow].ServiceFeePaid += remainingServiceFee;
                        paymentAmount -= remainingServiceFee;
                        remainingServiceFee = 0;
                    }

                    paymentAmount = Round.RoundOffAmount(paymentAmount);

                    if (payablePrincipal >= paymentAmount)
                    {
                        outputGrid[outputGridRow].PrincipalPaid += paymentAmount;
                        remainingPrincipal -= paymentAmount;
                    }
                    else
                    {
                        outputGrid[outputGridRow].PrincipalPaid += payablePrincipal;
                        remainingPrincipal -= payablePrincipal;
                    }
                }

                remainingPrincipal = Round.RoundOffAmount(remainingPrincipal);
                remainingServiceFee = Round.RoundOffAmount(remainingServiceFee);

                #endregion

                outputGrid[outputGridRow].TotalPaid = outputGrid[outputGridRow].PrincipalPaid + outputGrid[outputGridRow].InterestPaid +
                                                            outputGrid[outputGridRow].ServiceFeePaid + outputGrid[outputGridRow].ServiceFeeInterestPaid +
                                                            outputGrid[outputGridRow].OriginationFeePaid + outputGrid[outputGridRow].SameDayFeePaid +
                                                            outputGrid[outputGridRow].ManagementFeePaid + outputGrid[outputGridRow].MaintenanceFeePaid +
                                                            outputGrid[outputGridRow].NSFFeePaid + outputGrid[outputGridRow].LateFeePaid;

                outputGrid[outputGridRow].CumulativePrincipal = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePrincipal) + outputGrid[outputGridRow].PrincipalPaid;
                outputGrid[outputGridRow].CumulativeInterest = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeInterest) + outputGrid[outputGridRow].InterestPaid;
                outputGrid[outputGridRow].CumulativePayment = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePayment) + outputGrid[outputGridRow].TotalPaid;
                outputGrid[outputGridRow].CumulativeServiceFee = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFee) + outputGrid[outputGridRow].ServiceFeePaid;
                outputGrid[outputGridRow].CumulativeServiceFeeInterest = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeInterest) + outputGrid[outputGridRow].ServiceFeeInterestPaid;
                outputGrid[outputGridRow].CumulativeOriginationFee = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeOriginationFee) + outputGrid[outputGridRow].OriginationFeePaid;
                outputGrid[outputGridRow].CumulativeSameDayFee = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeSameDayFee) + outputGrid[outputGridRow].SameDayFeePaid;
                outputGrid[outputGridRow].CumulativeManagementFee = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeManagementFee) + outputGrid[outputGridRow].ManagementFeePaid;
                outputGrid[outputGridRow].CumulativeMaintenanceFee = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeMaintenanceFee) + outputGrid[outputGridRow].MaintenanceFeePaid;
                outputGrid[outputGridRow].CumulativeNSFFee = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeNSFFee) + outputGrid[outputGridRow].NSFFeePaid;
                outputGrid[outputGridRow].CumulativeLateFee = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeLateFee) + outputGrid[outputGridRow].LateFeePaid;
                outputGrid[outputGridRow].CumulativeTotalFees = outputGrid[outputGridRow].CumulativeServiceFee + outputGrid[outputGridRow].CumulativeServiceFeeInterest +
                                                                outputGrid[outputGridRow].CumulativeOriginationFee + outputGrid[outputGridRow].CumulativeSameDayFee +
                                                                outputGrid[outputGridRow].CumulativeManagementFee + outputGrid[outputGridRow].CumulativeMaintenanceFee +
                                                                outputGrid[outputGridRow].CumulativeNSFFee + outputGrid[outputGridRow].CumulativeLateFee;

                //Recalculate values in output grid as per remaining loan amount and service fee when the event is not the part of extending the repayment schedule.
                if (!isExtendingSchedule)
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AdditionalPayment() method. Calling method : ReCalculateSchedules(remainingPrincipal, remainingServiceFee, outputGrid, scheduleInput, outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);");
                    ReCalculateSchedules(remainingPrincipal, remainingServiceFee, outputGrid, scheduleInput, outputGridRow + 1, defaultTotalAmountToPay,
                        defaultTotalServiceFeePayable, originationFee, sameDayFee);
                }

                sbTracing.AppendLine("Exist:From method name AdditionalPayment(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int additionalPaymentRow, int inputGridRow, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee, double managementFee, double maintenanceFee).");
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

        #region NSF and Late fee

        /// <summary>
        /// This function adds the NSF fee and Late fee event of additional payment to the output grid.
        /// </summary>
        /// <param name="outputGrid"></param>
        /// <param name="scheduleInput"></param>
        /// <param name="additionalPaymentRow"></param>
        /// <param name="inputGridRow"></param>
        /// <param name="outputGridRow"></param>
        /// <param name="defaultTotalAmountToPay"></param>
        /// <param name="defaultTotalServiceFeePayable"></param>
        /// <param name="originationFee"></param>
        /// <param name="sameDayFee"></param>
        private static void NSFandLateFee(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int additionalPaymentRow, int inputGridRow, int outputGridRow,
                                                double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double sameDayFee)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name NSFandLateFee(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int additionalPaymentRow, int inputGridRow, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double sameDayFee).");
                sbTracing.AppendLine("Parameter values are : additionalPaymentRow = " + additionalPaymentRow + ", inputGridRow = " + inputGridRow + ", outputGridRow = " + outputGridRow + ", defaultTotalAmountToPay = " + defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", originationFee = " + originationFee + ", sameDayFee = " + sameDayFee);

                double remainingPrincipal = outputGrid[outputGridRow].BeginningPrincipal;
                double remainingServiceFee = outputGrid[outputGridRow].BeginningServiceFee;

                //Determine the last index in output grid where interest was accrued.
                int lastOutputRow = outputGrid.Take(outputGridRow).ToList().FindLastIndex(o => o.Flags != (int)Constants.FlagValues.ManagementFee &&
                                                                                                o.Flags != (int)Constants.FlagValues.MaintenanceFee);

                DateTime startDate;
                if (lastOutputRow == -1)
                {
                    startDate = scheduleInput.InputRecords[inputGridRow - 1].EffectiveDate == DateTime.MinValue ?
                                        scheduleInput.InputRecords[inputGridRow - 1].DateIn : scheduleInput.InputRecords[inputGridRow - 1].EffectiveDate;
                }
                else
                {
                    startDate = outputGrid[lastOutputRow].PaymentDate;
                }
                DateTime endDate = scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].DateIn;

                double periodicInterestRate;
                double interestAccrued;
                //This condition determines whether there is an interest rate in additional payment grid, for that row. If it is provided, that value will be used, otherwise
                //default value will be used for periodic interest calculations.
                if (string.IsNullOrEmpty(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate))
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and NSFandLateFee() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, false);");
                    periodicInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, false, false);

                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and NSFandLateFee() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, false);");
                    interestAccrued = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, false, false);
                }
                else
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and NSFandLateFee() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, true, false);");
                    periodicInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, true, false, false);

                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and NSFandLateFee() method. Calling method : PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, false);");
                    interestAccrued = PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, false, false);
                }

                interestAccrued = Round.RoundOffAmount(interestAccrued);

                //This variable calculates the daily interest amount for the period.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and NSFandLateFee() method. Calling method : DailyInterestAmount.CalculateDailyInterestAmount(startDate, endDate, interestAccrued, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay,scheduleInput, false);");
                double dailyInterestAmount = DailyInterestAmount.CalculateDailyInterestAmount(startDate, endDate, interestAccrued, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, scheduleInput, false);
                dailyInterestAmount = Round.RoundOffAmount(dailyInterestAmount);

                //It determines the total interest carry over to be paid.
                double interestDue = interestAccrued + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].InterestCarryOver);

                double serviceFeeInterestAccrued = 0;
                double serviceFeeInterestDue = 0;
                //Checks whether service fee interest is to be taken or not.
                if (scheduleInput.ApplyServiceFeeInterest == 1 || scheduleInput.ApplyServiceFeeInterest == 3)
                {
                    //Calculate the accrued service fee interest value.
                    if (string.IsNullOrEmpty(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate))
                    {
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and NSFandLateFee() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true, false);");
                        serviceFeeInterestAccrued = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true, false, false);
                    }
                    else
                    {
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and NSFandLateFee() method. Calling method : PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true, false);");
                        serviceFeeInterestAccrued = PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true, false, false);
                    }

                    serviceFeeInterestAccrued = Round.RoundOffAmount(serviceFeeInterestAccrued);

                    //It determines the total interest carry over to be paid.
                    serviceFeeInterestDue = serviceFeeInterestAccrued + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].AccruedServiceFeeInterestCarryOver);
                }

                //This variable calculates the daily interest rate for the period.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and NSFandLateFee() method. Calling method : DailyInterestRate.CalculateDailyInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay,scheduleInput, false);");
                double dailyInterestRate = DailyInterestRate.CalculateDailyInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, scheduleInput, false);

                #region Inserting the additional payment row

                //It adds a new row on the selected indes i.e. "outputGridRow" in the output grid.
                outputGrid.Insert(outputGridRow,
                        new OutputGrid
                        {
                            PaymentDate = endDate,
                            BeginningPrincipal = remainingPrincipal,
                            BeginningServiceFee = remainingServiceFee,
                            PeriodicInterestRate = periodicInterestRate,
                            DailyInterestRate = dailyInterestRate,
                            DailyInterestAmount = dailyInterestAmount,
                            PaymentID = Convert.ToInt32(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].PaymentID.ToString()),
                            Flags = Convert.ToInt32(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].Flags.ToString()),
                            DueDate = endDate,
                            InterestAccrued = interestAccrued,
                            InterestDue = interestDue,
                            InterestCarryOver = interestDue,
                            InterestPayment = 0,
                            PrincipalPayment = 0,
                            TotalPayment = 0,
                            PaymentDue = 0,
                            AccruedServiceFeeInterest = serviceFeeInterestAccrued,
                            ServiceFeeInterestDue = serviceFeeInterestDue,
                            AccruedServiceFeeInterestCarryOver = serviceFeeInterestDue,
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
                            PrincipalPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].PrincipalPastDue,
                            InterestPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].InterestPastDue,
                            ServiceFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].ServiceFeePastDue,
                            ServiceFeeInterestPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].ServiceFeeInterestPastDue,
                            OriginationFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].OriginationFeePastDue,
                            MaintenanceFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].MaintenanceFeePastDue,
                            ManagementFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].ManagementFeePastDue,
                            SameDayFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].SameDayFeePastDue,
                            NSFFeePastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].NSFFeePastDue) +
                                                            (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].Flags == (int)Constants.FlagValues.NSFFee ?
                                                                scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment : 0),
                            LateFeePastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].LateFeePastDue) +
                                                            (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].Flags == (int)Constants.FlagValues.LateFee ?
                                                                scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment : 0),
                            TotalPastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].TotalPastDue) +
                                                            scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment,
                            //Cumulative past due amount columns
                            CumulativePrincipalPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePrincipalPastDue,
                            CumulativeInterestPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeInterestPastDue,
                            CumulativeServiceFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeePastDue,
                            CumulativeServiceFeeInterestPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeInterestPastDue,
                            CumulativeOriginationFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeOriginationFeePastDue,
                            CumulativeMaintenanceFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeMaintenanceFeePastDue,
                            CumulativeManagementFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeManagementFeePastDue,
                            CumulativeSameDayFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeSameDayFeePastDue,
                            CumulativeNSFFeePastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeNSFFeePastDue) +
                                                            (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].Flags == (int)Constants.FlagValues.NSFFee ?
                                                                scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment : 0),
                            CumulativeLateFeePastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeLateFeePastDue) +
                                                            (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].Flags == (int)Constants.FlagValues.LateFee ?
                                                                scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment : 0),
                            CumulativeTotalPastDue = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeTotalPastDue) +
                                                            scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment,
                        });

                #endregion

                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and NSFandLateFee() method. Calling method : ReCalculateSchedules(remainingPrincipal, remainingServiceFee, outputGrid, scheduleInput, outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);");
                ReCalculateSchedules(remainingPrincipal, remainingServiceFee, outputGrid, scheduleInput, outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);

                sbTracing.AppendLine("Exist:From method name NSFandLateFee(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int additionalPaymentRow, int inputGridRow, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, double originationFee, double sameDayFee).");
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
        /// This function adds the Management and Maintenance fee event of additional payment to the output grid.
        /// </summary>
        /// <param name="outputGrid"></param>
        /// <param name="scheduleInput"></param>
        /// <param name="additionalPaymentRow"></param>
        /// <param name="inputGridRow"></param>
        /// <param name="outputGridRow"></param>
        /// <param name="defaultTotalAmountToPay"></param>
        /// <param name="defaultTotalServiceFeePayable"></param>
        /// <param name="isExtendingSchedule"></param>
        /// <param name="originationFee"></param>
        /// <param name="sameDayFee"></param>
        private static void ManagementAndMaintenanceFee(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int additionalPaymentRow, int inputGridRow,
                int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee,
                double sameDayFee)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name ManagementAndMaintenanceFee(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int additionalPaymentRow, int inputGridRow, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee).");
                sbTracing.AppendLine("Parameter values are : additionalPaymentRow = " + additionalPaymentRow + ", inputGridRow = " + inputGridRow + ", outputGridRow = " + outputGridRow + ", defaultTotalAmountToPay = " + defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", isExtendingSchedule = " + isExtendingSchedule + ", originationFee = " + originationFee + ", sameDayFee = " + sameDayFee);

                double remainingPrincipal = isExtendingSchedule ? (outputGrid[outputGridRow - 1].BeginningPrincipal - outputGrid[outputGridRow - 1].PrincipalPaid) :
                                                                    outputGrid[outputGridRow].BeginningPrincipal;
                double remainingServiceFee = isExtendingSchedule ? (outputGrid[outputGridRow - 1].BeginningServiceFee - outputGrid[outputGridRow - 1].ServiceFeePaid) :
                                                                        outputGrid[outputGridRow].BeginningServiceFee;

                #region Inserting the additional payment row

                //It adds a new row on the selected indes i.e. "outputGridRow" in the output grid.
                outputGrid.Insert(outputGridRow,
                        new OutputGrid
                        {
                            PaymentDate = scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].DateIn,
                            BeginningPrincipal = remainingPrincipal,
                            BeginningServiceFee = remainingServiceFee,
                            PeriodicInterestRate = 0,
                            DailyInterestRate = 0,
                            DailyInterestAmount = 0,
                            PaymentID = Convert.ToInt32(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].PaymentID.ToString()),
                            Flags = Convert.ToInt32(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].Flags.ToString()),
                            DueDate = scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].DateIn,
                            InterestAccrued = (outputGridRow == 0 ? scheduleInput.EarnedInterest : 0),
                            InterestDue = (outputGridRow == 0 ? scheduleInput.EarnedInterest : 0),
                            InterestCarryOver = (outputGridRow == 0 ? scheduleInput.EarnedInterest : outputGrid[outputGridRow - 1].InterestCarryOver),
                            InterestPayment = 0,
                            PrincipalPayment = 0,
                            TotalPayment = 0,
                            PaymentDue = 0,
                            AccruedServiceFeeInterest = (outputGridRow == 0 ? scheduleInput.EarnedServiceFeeInterest : 0),
                            ServiceFeeInterestDue = (outputGridRow == 0 ? scheduleInput.EarnedServiceFeeInterest : 0),
                            AccruedServiceFeeInterestCarryOver = (outputGridRow == 0 ? scheduleInput.EarnedServiceFeeInterest : outputGrid[outputGridRow - 1].AccruedServiceFeeInterestCarryOver),
                            ServiceFee = 0,
                            ServiceFeeInterest = 0,
                            ServiceFeeTotal = 0,
                            OriginationFee = (outputGridRow == 0 ? scheduleInput.EarnedOriginationFee : 0),
                            MaintenanceFee = (outputGridRow == 0 ? scheduleInput.EarnedMaintenanceFee : 0),
                            ManagementFee = (outputGridRow == 0 ? scheduleInput.EarnedManagementFee : 0),
                            SameDayFee = (outputGridRow == 0 ? scheduleInput.EarnedSameDayFee : 0),
                            NSFFee = (outputGridRow == 0 ? scheduleInput.EarnedNSFFee : 0),
                            LateFee = (outputGridRow == 0 ? scheduleInput.EarnedLateFee : 0),
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
                            PrincipalPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].PrincipalPastDue,
                            InterestPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].InterestPastDue,
                            ServiceFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].ServiceFeePastDue,
                            ServiceFeeInterestPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].ServiceFeeInterestPastDue,
                            OriginationFeePastDue = outputGridRow == 0 ? scheduleInput.EarnedOriginationFee : outputGrid[outputGridRow - 1].OriginationFeePastDue,
                            MaintenanceFeePastDue = (outputGridRow == 0 ? scheduleInput.EarnedMaintenanceFee : outputGrid[outputGridRow - 1].MaintenanceFeePastDue) +
                                                        (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].Flags == (int)Constants.FlagValues.MaintenanceFee ?
                                                            scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment : 0),
                            ManagementFeePastDue = (outputGridRow == 0 ? scheduleInput.EarnedManagementFee : outputGrid[outputGridRow - 1].ManagementFeePastDue) +
                                                        (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].Flags == (int)Constants.FlagValues.ManagementFee ?
                                                            scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment : 0),
                            SameDayFeePastDue = outputGridRow == 0 ? scheduleInput.EarnedSameDayFee : outputGrid[outputGridRow - 1].SameDayFeePastDue,
                            NSFFeePastDue = outputGridRow == 0 ? scheduleInput.EarnedNSFFee : outputGrid[outputGridRow - 1].NSFFeePastDue,
                            LateFeePastDue = outputGridRow == 0 ? scheduleInput.EarnedLateFee : outputGrid[outputGridRow - 1].LateFeePastDue,
                            TotalPastDue = (outputGridRow == 0 ? (scheduleInput.EarnedOriginationFee + scheduleInput.EarnedSameDayFee + scheduleInput.EarnedManagementFee +
                                                                    scheduleInput.EarnedMaintenanceFee + scheduleInput.EarnedNSFFee + scheduleInput.EarnedLateFee) :
                                                                        outputGrid[outputGridRow - 1].TotalPastDue) +
                                                            scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment,
                            //Cumulative past due amount columns
                            CumulativePrincipalPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePrincipalPastDue,
                            CumulativeInterestPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeInterestPastDue,
                            CumulativeServiceFeePastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeePastDue,
                            CumulativeServiceFeeInterestPastDue = outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeInterestPastDue,
                            CumulativeOriginationFeePastDue = outputGridRow == 0 ? scheduleInput.EarnedOriginationFee : outputGrid[outputGridRow - 1].CumulativeOriginationFeePastDue,
                            CumulativeMaintenanceFeePastDue = (outputGridRow == 0 ? scheduleInput.EarnedMaintenanceFee : outputGrid[outputGridRow - 1].CumulativeMaintenanceFeePastDue) +
                                                                (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].Flags == (int)Constants.FlagValues.MaintenanceFee ?
                                                                    scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment : 0),
                            CumulativeManagementFeePastDue = (outputGridRow == 0 ? scheduleInput.EarnedManagementFee : outputGrid[outputGridRow - 1].CumulativeManagementFeePastDue) +
                                                                (scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].Flags == (int)Constants.FlagValues.ManagementFee ?
                                                                    scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment : 0),
                            CumulativeSameDayFeePastDue = outputGridRow == 0 ? scheduleInput.EarnedSameDayFee : outputGrid[outputGridRow - 1].CumulativeSameDayFeePastDue,
                            CumulativeNSFFeePastDue = outputGridRow == 0 ? scheduleInput.EarnedNSFFee : outputGrid[outputGridRow - 1].CumulativeNSFFeePastDue,
                            CumulativeLateFeePastDue = outputGridRow == 0 ? scheduleInput.EarnedLateFee : outputGrid[outputGridRow - 1].CumulativeLateFeePastDue,
                            CumulativeTotalPastDue = (outputGridRow == 0 ? (scheduleInput.EarnedOriginationFee + scheduleInput.EarnedSameDayFee +
                                                                            scheduleInput.EarnedManagementFee + scheduleInput.EarnedMaintenanceFee +
                                                                            scheduleInput.EarnedNSFFee + scheduleInput.EarnedLateFee) : outputGrid[outputGridRow - 1].CumulativeTotalPastDue) +
                                                            scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].AdditionalPayment
                        });

                #endregion

                if (!isExtendingSchedule)
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and ManagementAndMaintenanceFee() method. Calling method : ReCalculateSchedules(remainingPrincipal, remainingServiceFee, outputGrid, scheduleInput, outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);");
                    ReCalculateSchedules(remainingPrincipal, remainingServiceFee, outputGrid, scheduleInput, outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);
                }

                sbTracing.AppendLine("Exist:From method name ManagementAndMaintenanceFee(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int additionalPaymentRow, int inputGridRow, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee).");
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
        /// This function will give discount on the components for the Period method. After deducting the discount amount, the remaining fund will be used to create to 
        /// recalculate the repayment schedule.
        /// </summary>
        /// <param name="outputGrid"></param>
        /// <param name="scheduleInput"></param>
        /// <param name="additionalPaymentRow"></param>
        /// <param name="inputGridRow"></param>
        /// <param name="outputGridRow"></param>
        /// <param name="defaultTotalAmountToPay"></param>
        /// <param name="defaultTotalServiceFeePayable"></param>
        /// <param name="isExtendingSchedule"></param>
        /// <param name="originationFee"></param>
        /// <param name="sameDayFee"></param>
        private static void DiscountPayment(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int additionalPaymentRow, int inputGridRow, int outputGridRow,
                            double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class ActualPaymentAllocation and method name DiscountPayment(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int additionalPaymentRow, int inputGridRow, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee).");
                sbTracing.AppendLine("Parameter values are : additionalPaymentRow = " + additionalPaymentRow + ", inputGridRow = " + inputGridRow + ", outputGridRow = " + outputGridRow + ", defaultTotalAmountToPay = " + defaultTotalAmountToPay + ", defaultTotalServiceFeePayable = " + defaultTotalServiceFeePayable + ", isExtendingSchedule = " + isExtendingSchedule + ", originationFee = " + originationFee + ", sameDayFee = " + sameDayFee);
                double remainingPrincipal = isExtendingSchedule ? (outputGrid[outputGridRow - 1].BeginningPrincipal - outputGrid[outputGridRow - 1].PrincipalPaid) :
                                                                    outputGrid[outputGridRow].BeginningPrincipal;
                double remainingServiceFee = isExtendingSchedule ? (outputGrid[outputGridRow - 1].BeginningServiceFee - outputGrid[outputGridRow - 1].ServiceFeePaid) :
                                                                        outputGrid[outputGridRow].BeginningServiceFee;

                double payablePrincipal = (remainingPrincipal < Round.RoundOffAmount(remainingPrincipal - scheduleInput.Residual) ? remainingPrincipal :
                                                                Round.RoundOffAmount(remainingPrincipal - scheduleInput.Residual));

                double remainingOriginationFee = originationFee;
                double remainingSameDayFee = sameDayFee;
                for (int i = 0; i < outputGridRow; i++)
                {
                    remainingOriginationFee = (remainingOriginationFee - outputGrid[i].OriginationFeePaid) < 0 ? 0 : Round.RoundOffAmount(remainingOriginationFee - outputGrid[i].OriginationFeePaid);
                    remainingSameDayFee = (remainingSameDayFee - outputGrid[i].SameDayFeePaid) < 0 ? 0 : Round.RoundOffAmount(remainingSameDayFee - outputGrid[i].SameDayFeePaid);
                }

                //Determine the last index in output grid where interest was accrued.
                int lastOutputRow = outputGrid.Take(outputGridRow).ToList().FindLastIndex(o => o.Flags != (int)Constants.FlagValues.ManagementFee &&
                                                                                                o.Flags != (int)Constants.FlagValues.MaintenanceFee);

                DateTime startDate;
                if (lastOutputRow == -1)
                {
                    startDate = scheduleInput.InputRecords[inputGridRow - 1].EffectiveDate == DateTime.MinValue ?
                                        scheduleInput.InputRecords[inputGridRow - 1].DateIn : scheduleInput.InputRecords[inputGridRow - 1].EffectiveDate;
                }
                else
                {
                    startDate = outputGrid[lastOutputRow].PaymentDate;
                }
                DateTime endDate = scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].DateIn;

                double periodicInterestRate;
                double interestAccrued;
                //This condition determines whether there is an interest rate in additional payment grid, for that row. If it is provided, that value will be used, otherwise
                //default value will be used for periodic interest calculations.
                if (string.IsNullOrEmpty(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate))
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and DiscountPayment() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule);");
                    periodicInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule, false);

                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and DiscountPayment() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule);");
                    interestAccrued = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule, false);
                }
                else
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and DiscountPayment() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, true, isExtendingSchedule);");
                    periodicInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, true, isExtendingSchedule, false);

                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and DiscountPayment() method. Calling method : PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule);");
                    interestAccrued = PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, isExtendingSchedule, false);
                }

                interestAccrued = Round.RoundOffAmount(interestAccrued);

                //This variable calculates the daily interest amount for the period.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and DiscountPayment() method. Calling method : DailyInterestAmount.CalculateDailyInterestAmount(startDate, endDate, interestAccrued, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay,scheduleInput,isExtendingSchedule);");
                double dailyInterestAmount = DailyInterestAmount.CalculateDailyInterestAmount(startDate, endDate, interestAccrued, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, scheduleInput, isExtendingSchedule);
                dailyInterestAmount = Round.RoundOffAmount(dailyInterestAmount);

                //Add the earned interest amount to the accrued value.
                interestAccrued += (outputGridRow == 0 ? scheduleInput.EarnedInterest : 0);

                //It determines the total interest carry over to be paid.
                double interestDue = interestAccrued + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].InterestCarryOver);

                double serviceFeeInterestAccrued = 0;
                //Checks whether service fee interest is to be taken or not.
                if (scheduleInput.ApplyServiceFeeInterest == 1 || scheduleInput.ApplyServiceFeeInterest == 3)
                {
                    //Calculate the accrued service fee interest value.
                    if (string.IsNullOrEmpty(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate))
                    {
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and DiscountPayment() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true, isExtendingSchedule);");
                        serviceFeeInterestAccrued = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true, isExtendingSchedule, false);
                    }
                    else
                    {
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and DiscountPayment() method. Calling method : PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true,isExtendingSchedule);");
                        serviceFeeInterestAccrued = PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInput, startDate, endDate, Convert.ToDouble(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestRate), scheduleInput.InputRecords[0].DateIn, remainingServiceFee, true, isExtendingSchedule, false);
                    }
                    serviceFeeInterestAccrued = Round.RoundOffAmount(serviceFeeInterestAccrued);
                }

                //Add earned service fee interest value to the accrued value.
                serviceFeeInterestAccrued += (outputGridRow == 0 ? scheduleInput.EarnedServiceFeeInterest : 0);
                //It determines the total interest carry over to be paid.
                double serviceFeeInterestDue = serviceFeeInterestAccrued + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].AccruedServiceFeeInterestCarryOver);

                //This variable calculates the daily interest rate for the period.
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and DiscountPayment() method. Calling method : DailyInterestRate.CalculateDailyInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay,scheduleInput,isExtendingSchedule);");
                double dailyInterestRate = DailyInterestRate.CalculateDailyInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, scheduleInput, isExtendingSchedule);

                #region Inserting the additional payment row

                //It adds a new row on the selected indes i.e. "outputGridRow" in the output grid.
                outputGrid.Insert(outputGridRow,
                        new OutputGrid
                        {
                            PaymentDate = endDate,
                            BeginningPrincipal = remainingPrincipal,
                            BeginningServiceFee = remainingServiceFee,
                            PeriodicInterestRate = periodicInterestRate,
                            DailyInterestRate = dailyInterestRate,
                            DailyInterestAmount = dailyInterestAmount,
                            PaymentID = Convert.ToInt32(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].PaymentID.ToString()),
                            Flags = Convert.ToInt32(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].Flags.ToString()),
                            DueDate = endDate,
                            InterestAccrued = interestAccrued,
                            InterestDue = interestDue,
                            InterestCarryOver = 0,
                            InterestPayment = 0,
                            PrincipalPayment = 0,
                            TotalPayment = 0,
                            PaymentDue = 0,
                            AccruedServiceFeeInterest = serviceFeeInterestAccrued,
                            ServiceFeeInterestDue = serviceFeeInterestDue,
                            AccruedServiceFeeInterestCarryOver = 0,
                            ServiceFee = 0,
                            ServiceFeeInterest = 0,
                            ServiceFeeTotal = 0,
                            OriginationFee = remainingOriginationFee,
                            MaintenanceFee = (outputGridRow == 0 ? scheduleInput.EarnedMaintenanceFee : 0),
                            ManagementFee = (outputGridRow == 0 ? scheduleInput.EarnedManagementFee : 0),
                            SameDayFee = remainingSameDayFee,
                            NSFFee = (outputGridRow == 0 ? scheduleInput.EarnedNSFFee : 0),
                            LateFee = (outputGridRow == 0 ? scheduleInput.EarnedLateFee : 0),
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
                            NSFFeePastDue = 0,
                            LateFeePastDue = 0,
                            TotalPastDue = 0,
                            //Cumulative past due amount columns
                            CumulativePrincipalPastDue = 0,
                            CumulativeInterestPastDue = 0,
                            CumulativeServiceFeePastDue = 0,
                            CumulativeServiceFeeInterestPastDue = 0,
                            CumulativeOriginationFeePastDue = 0,
                            CumulativeMaintenanceFeePastDue = 0,
                            CumulativeManagementFeePastDue = 0,
                            CumulativeSameDayFeePastDue = 0,
                            CumulativeNSFFeePastDue = 0,
                            CumulativeLateFeePastDue = 0,
                            CumulativeTotalPastDue = 0
                        });

                #endregion

                double principalDiscount = Round.RoundOffAmount(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].PrincipalDiscount);
                double interestDiscount = Round.RoundOffAmount(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].InterestDiscount);
                double serviceFeeDiscount = Round.RoundOffAmount(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].ServiceFeeDiscount);
                double serviceFeeInterestDiscount = Round.RoundOffAmount(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].ServiceFeeInterestDiscount);
                double originationFeeDiscount = Round.RoundOffAmount(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].OriginationFeeDiscount);
                double maintenanceFeeDiscount = Round.RoundOffAmount(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].MaintenanceFeeDiscount);
                double managementFeeDiscount = Round.RoundOffAmount(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].ManagementFeeDiscount);
                double sameDayFeeDiscount = Round.RoundOffAmount(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].SameDayFeeDiscount);
                double nsfFeeDiscount = Round.RoundOffAmount(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].NSFFeeDiscount);
                double lateFeeDiscount = Round.RoundOffAmount(scheduleInput.AdditionalPaymentRecords[additionalPaymentRow].LateFeeDiscount);

                #region Pay current row amount

                if (outputGrid[outputGridRow].InterestDue >= interestDiscount)
                {
                    outputGrid[outputGridRow].InterestPaid = interestDiscount;
                    outputGrid[outputGridRow].InterestPayment = interestDiscount;
                    outputGrid[outputGridRow].InterestCarryOver = Round.RoundOffAmount(outputGrid[outputGridRow].InterestDue - interestDiscount);
                    interestDiscount = 0;
                }
                else
                {
                    outputGrid[outputGridRow].InterestPaid = outputGrid[outputGridRow].InterestDue;
                    outputGrid[outputGridRow].InterestPayment = outputGrid[outputGridRow].InterestDue;
                    outputGrid[outputGridRow].InterestCarryOver = 0;
                    interestDiscount = Round.RoundOffAmount(interestDiscount - outputGrid[outputGridRow].InterestDue);
                }

                if (outputGrid[outputGridRow].ServiceFeeInterestDue >= serviceFeeInterestDiscount)
                {
                    outputGrid[outputGridRow].ServiceFeeInterestPaid = serviceFeeInterestDiscount;
                    outputGrid[outputGridRow].ServiceFeeInterest = serviceFeeInterestDiscount;
                    outputGrid[outputGridRow].AccruedServiceFeeInterestCarryOver = Round.RoundOffAmount(outputGrid[outputGridRow].ServiceFeeInterestDue - serviceFeeInterestDiscount);
                    serviceFeeInterestDiscount = 0;
                }
                else
                {
                    outputGrid[outputGridRow].ServiceFeeInterestPaid = outputGrid[outputGridRow].ServiceFeeInterestDue;
                    outputGrid[outputGridRow].ServiceFeeInterest = outputGrid[outputGridRow].ServiceFeeInterestDue;
                    outputGrid[outputGridRow].AccruedServiceFeeInterestCarryOver = 0;
                    serviceFeeInterestDiscount = Round.RoundOffAmount(serviceFeeInterestDiscount - outputGrid[outputGridRow].ServiceFeeInterestDue);
                }

                outputGrid[outputGridRow].OriginationFeePaid = (outputGrid[outputGridRow].OriginationFee >= originationFeeDiscount) ? originationFeeDiscount :
                                                                outputGrid[outputGridRow].OriginationFee;
                outputGrid[outputGridRow].SameDayFeePaid = (outputGrid[outputGridRow].SameDayFee >= sameDayFeeDiscount) ? sameDayFeeDiscount :
                                                            outputGrid[outputGridRow].SameDayFee;

                if (outputGridRow == 0)
                {
                    outputGrid[outputGridRow].ManagementFeePaid = (outputGrid[outputGridRow].ManagementFee >= managementFeeDiscount) ? managementFeeDiscount :
                                                                    outputGrid[outputGridRow].ManagementFee;
                    outputGrid[outputGridRow].MaintenanceFeePaid = (outputGrid[outputGridRow].MaintenanceFee >= maintenanceFeeDiscount) ? maintenanceFeeDiscount :
                                                                    outputGrid[outputGridRow].MaintenanceFee;
                    outputGrid[outputGridRow].NSFFeePaid = (outputGrid[outputGridRow].NSFFee >= nsfFeeDiscount) ? nsfFeeDiscount : outputGrid[outputGridRow].NSFFee;
                    outputGrid[outputGridRow].LateFeePaid = (outputGrid[outputGridRow].LateFee >= lateFeeDiscount) ? lateFeeDiscount : outputGrid[outputGridRow].LateFee;

                    outputGrid[outputGridRow].ManagementFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].ManagementFee - outputGrid[outputGridRow].ManagementFeePaid);
                    outputGrid[outputGridRow].MaintenanceFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].MaintenanceFee - outputGrid[outputGridRow].MaintenanceFeePaid);
                    outputGrid[outputGridRow].NSFFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].NSFFee - outputGrid[outputGridRow].NSFFeePaid);
                    outputGrid[outputGridRow].LateFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].LateFee - outputGrid[outputGridRow].LateFeePaid);
                }

                #endregion

                outputGrid[outputGridRow].OriginationFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].OriginationFee - outputGrid[outputGridRow].OriginationFeePaid);
                outputGrid[outputGridRow].SameDayFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow].SameDayFee - outputGrid[outputGridRow].SameDayFeePaid);
                outputGrid[outputGridRow].ServiceFeeTotal = outputGrid[outputGridRow].ServiceFeeInterest;

                #region Pay Past due amount

                if (outputGridRow != 0)
                {
                    double pastDuePrincipal = (payablePrincipal < outputGrid[outputGridRow - 1].PrincipalPastDue) ? payablePrincipal :
                                                                    outputGrid[outputGridRow - 1].PrincipalPastDue;
                    double pastDueServiceFee = (outputGrid[outputGridRow].BeginningServiceFee < outputGrid[outputGridRow - 1].ServiceFeePastDue) ?
                                                    outputGrid[outputGridRow].BeginningServiceFee : outputGrid[outputGridRow - 1].ServiceFeePastDue;

                    //Pay past due Principal amount
                    if (pastDuePrincipal >= principalDiscount)
                    {
                        outputGrid[outputGridRow].PrincipalPaid = principalDiscount;
                        outputGrid[outputGridRow].PrincipalPastDue = Round.RoundOffAmount(pastDuePrincipal - principalDiscount);
                        payablePrincipal = Round.RoundOffAmount(payablePrincipal - principalDiscount);
                        principalDiscount = 0;
                    }
                    else
                    {
                        outputGrid[outputGridRow].PrincipalPaid = pastDuePrincipal;
                        principalDiscount = Round.RoundOffAmount(principalDiscount - pastDuePrincipal);
                        payablePrincipal = Round.RoundOffAmount(payablePrincipal - pastDuePrincipal);
                    }

                    //Pay past due Interest amount
                    if (outputGrid[outputGridRow - 1].InterestPastDue >= interestDiscount)
                    {
                        outputGrid[outputGridRow].InterestPaid += interestDiscount;
                        outputGrid[outputGridRow].InterestPastDue = Round.RoundOffAmount(outputGrid[outputGridRow - 1].InterestPastDue - interestDiscount);
                    }
                    else
                    {
                        outputGrid[outputGridRow].InterestPaid += outputGrid[outputGridRow - 1].InterestPastDue;
                    }

                    //Pay past due Service Fee amount
                    if (pastDueServiceFee >= serviceFeeDiscount)
                    {
                        outputGrid[outputGridRow].ServiceFeePaid = serviceFeeDiscount;
                        outputGrid[outputGridRow].ServiceFeePastDue = Round.RoundOffAmount(pastDueServiceFee - serviceFeeDiscount);
                        serviceFeeDiscount = 0;
                    }
                    else
                    {
                        outputGrid[outputGridRow].ServiceFeePaid = pastDueServiceFee;
                        serviceFeeDiscount = Round.RoundOffAmount(serviceFeeDiscount - pastDueServiceFee);
                    }

                    //Pay past due Service Fee Interest amount
                    if (outputGrid[outputGridRow - 1].ServiceFeeInterestPastDue >= serviceFeeInterestDiscount)
                    {
                        outputGrid[outputGridRow].ServiceFeeInterestPaid += serviceFeeInterestDiscount;
                        outputGrid[outputGridRow].ServiceFeeInterestPastDue = Round.RoundOffAmount(outputGrid[outputGridRow - 1].ServiceFeeInterestPastDue - serviceFeeInterestDiscount);
                    }
                    else
                    {
                        outputGrid[outputGridRow].ServiceFeeInterestPaid += outputGrid[outputGridRow - 1].ServiceFeeInterestPastDue;
                    }

                    //Pay past due Management Fee amount
                    if (outputGrid[outputGridRow - 1].ManagementFeePastDue >= managementFeeDiscount)
                    {
                        outputGrid[outputGridRow].ManagementFeePaid += managementFeeDiscount;
                        outputGrid[outputGridRow].ManagementFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow - 1].ManagementFeePastDue - managementFeeDiscount);
                    }
                    else
                    {
                        outputGrid[outputGridRow].ManagementFeePaid += outputGrid[outputGridRow - 1].ManagementFeePastDue;
                    }

                    //Pay past due Maintenance amount
                    if (outputGrid[outputGridRow - 1].MaintenanceFeePastDue >= maintenanceFeeDiscount)
                    {
                        outputGrid[outputGridRow].MaintenanceFeePaid += maintenanceFeeDiscount;
                        outputGrid[outputGridRow].MaintenanceFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow - 1].MaintenanceFeePastDue - maintenanceFeeDiscount);
                    }
                    else
                    {
                        outputGrid[outputGridRow].MaintenanceFeePaid += outputGrid[outputGridRow - 1].MaintenanceFeePastDue;
                    }

                    //Pay past due NSF Fee amount
                    if (outputGrid[outputGridRow - 1].NSFFeePastDue >= nsfFeeDiscount)
                    {
                        outputGrid[outputGridRow].NSFFeePaid += nsfFeeDiscount;
                        outputGrid[outputGridRow].NSFFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow - 1].NSFFeePastDue - nsfFeeDiscount);
                    }
                    else
                    {
                        outputGrid[outputGridRow].NSFFeePaid += outputGrid[outputGridRow - 1].NSFFeePastDue;
                    }

                    //Pay past due Late Fee amount
                    if (outputGrid[outputGridRow - 1].LateFeePastDue >= lateFeeDiscount)
                    {
                        outputGrid[outputGridRow].LateFeePaid += lateFeeDiscount;
                        outputGrid[outputGridRow].LateFeePastDue = Round.RoundOffAmount(outputGrid[outputGridRow - 1].LateFeePastDue - lateFeeDiscount);
                    }
                    else
                    {
                        outputGrid[outputGridRow].LateFeePaid += outputGrid[outputGridRow - 1].LateFeePastDue;
                    }
                }

                #endregion

                outputGrid[outputGridRow].TotalPastDue = outputGrid[outputGridRow].PrincipalPastDue + outputGrid[outputGridRow].InterestPastDue +
                                                            outputGrid[outputGridRow].ServiceFeePastDue + outputGrid[outputGridRow].ServiceFeeInterestPastDue +
                                                            outputGrid[outputGridRow].OriginationFeePastDue + outputGrid[outputGridRow].SameDayFeePastDue +
                                                            outputGrid[outputGridRow].ManagementFeePastDue + outputGrid[outputGridRow].MaintenanceFeePastDue +
                                                            outputGrid[outputGridRow].NSFFeePastDue + outputGrid[outputGridRow].LateFeePastDue;

                outputGrid[outputGridRow].CumulativePrincipalPastDue = outputGrid[outputGridRow].PrincipalPastDue;
                outputGrid[outputGridRow].CumulativeInterestPastDue = outputGrid[outputGridRow].InterestPastDue;
                outputGrid[outputGridRow].CumulativeServiceFeePastDue = outputGrid[outputGridRow].ServiceFeePastDue;
                outputGrid[outputGridRow].CumulativeServiceFeeInterestPastDue = outputGrid[outputGridRow].ServiceFeeInterestPastDue;
                outputGrid[outputGridRow].CumulativeOriginationFeePastDue = outputGrid[outputGridRow].OriginationFeePastDue;
                outputGrid[outputGridRow].CumulativeSameDayFeePastDue = outputGrid[outputGridRow].SameDayFeePastDue;
                outputGrid[outputGridRow].CumulativeManagementFeePastDue = outputGrid[outputGridRow].ManagementFeePastDue;
                outputGrid[outputGridRow].CumulativeMaintenanceFeePastDue = outputGrid[outputGridRow].MaintenanceFeePastDue;
                outputGrid[outputGridRow].CumulativeNSFFeePastDue = outputGrid[outputGridRow].NSFFeePastDue;
                outputGrid[outputGridRow].CumulativeLateFeePastDue = outputGrid[outputGridRow].LateFeePastDue;
                outputGrid[outputGridRow].CumulativeTotalPastDue = outputGrid[outputGridRow].TotalPastDue;

                remainingPrincipal = Round.RoundOffAmount(outputGrid[outputGridRow].BeginningPrincipal - outputGrid[outputGridRow].PrincipalPaid);
                remainingServiceFee = Round.RoundOffAmount(outputGrid[outputGridRow].BeginningServiceFee - outputGrid[outputGridRow].ServiceFeePaid);

                #region Extra payment

                if (principalDiscount > 0)
                {
                    outputGrid[outputGridRow].PrincipalPaid += ((payablePrincipal >= principalDiscount) ? principalDiscount : payablePrincipal);
                    remainingPrincipal = Round.RoundOffAmount(remainingPrincipal - ((payablePrincipal >= principalDiscount) ? principalDiscount : payablePrincipal));
                }

                if (serviceFeeDiscount > 0)
                {
                    outputGrid[outputGridRow].ServiceFeePaid += ((remainingServiceFee >= serviceFeeDiscount) ? serviceFeeDiscount : remainingServiceFee);
                    remainingServiceFee = Round.RoundOffAmount(remainingServiceFee - ((remainingServiceFee >= serviceFeeDiscount) ? serviceFeeDiscount : remainingServiceFee));
                }

                #endregion

                outputGrid[outputGridRow].TotalPaid = outputGrid[outputGridRow].PrincipalPaid + outputGrid[outputGridRow].InterestPaid +
                                                            outputGrid[outputGridRow].ServiceFeePaid + outputGrid[outputGridRow].ServiceFeeInterestPaid +
                                                            outputGrid[outputGridRow].OriginationFeePaid + outputGrid[outputGridRow].SameDayFeePaid +
                                                            outputGrid[outputGridRow].ManagementFeePaid + outputGrid[outputGridRow].MaintenanceFeePaid +
                                                            outputGrid[outputGridRow].NSFFeePaid + outputGrid[outputGridRow].LateFeePaid;

                outputGrid[outputGridRow].CumulativePrincipal = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePrincipal) + outputGrid[outputGridRow].PrincipalPaid;
                outputGrid[outputGridRow].CumulativeInterest = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeInterest) + outputGrid[outputGridRow].InterestPaid;
                outputGrid[outputGridRow].CumulativePayment = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePayment) + outputGrid[outputGridRow].TotalPaid;
                outputGrid[outputGridRow].CumulativeServiceFee = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFee) + outputGrid[outputGridRow].ServiceFeePaid;
                outputGrid[outputGridRow].CumulativeServiceFeeInterest = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeInterest) + outputGrid[outputGridRow].ServiceFeeInterestPaid;
                outputGrid[outputGridRow].CumulativeOriginationFee = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeOriginationFee) + outputGrid[outputGridRow].OriginationFeePaid;
                outputGrid[outputGridRow].CumulativeSameDayFee = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeSameDayFee) + outputGrid[outputGridRow].SameDayFeePaid;
                outputGrid[outputGridRow].CumulativeManagementFee = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeManagementFee) + outputGrid[outputGridRow].ManagementFeePaid;
                outputGrid[outputGridRow].CumulativeMaintenanceFee = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeMaintenanceFee) + outputGrid[outputGridRow].MaintenanceFeePaid;
                outputGrid[outputGridRow].CumulativeNSFFee = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeNSFFee) + outputGrid[outputGridRow].NSFFeePaid;
                outputGrid[outputGridRow].CumulativeLateFee = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeLateFee) + outputGrid[outputGridRow].LateFeePaid;
                outputGrid[outputGridRow].CumulativeTotalFees = outputGrid[outputGridRow].CumulativeServiceFee + outputGrid[outputGridRow].CumulativeServiceFeeInterest +
                                                                outputGrid[outputGridRow].CumulativeOriginationFee + outputGrid[outputGridRow].CumulativeSameDayFee +
                                                                outputGrid[outputGridRow].CumulativeManagementFee + outputGrid[outputGridRow].CumulativeMaintenanceFee +
                                                                outputGrid[outputGridRow].CumulativeNSFFee + outputGrid[outputGridRow].CumulativeLateFee;

                //Recalculate values in output grid as per remaining loan amount and service fee when the event is not the part of extending the repayment schedule.
                if (!isExtendingSchedule)
                {
                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and DiscountPayment() method. Calling method : ReCalculateSchedules(remainingPrincipal, remainingServiceFee, outputGrid, scheduleInput, outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);");
                    ReCalculateSchedules(remainingPrincipal, remainingServiceFee, outputGrid, scheduleInput, outputGridRow + 1, defaultTotalAmountToPay, defaultTotalServiceFeePayable, originationFee, sameDayFee);
                }

                sbTracing.AppendLine("Exist:From method name DiscountPayment(List<OutputGrid> outputGrid, LoanDetails scheduleInput, int additionalPaymentRow, int inputGridRow, int outputGridRow, double defaultTotalAmountToPay, double defaultTotalServiceFeePayable, bool isExtendingSchedule, double originationFee, double sameDayFee).");
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