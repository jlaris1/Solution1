using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoanAmort_driver.Models;

namespace LoanAmort_driver.Business
{
    public class EarlyPayOff
    {
        /// <summary>
        /// Name : Sparsh Agarwal
        /// Date : 27/04/2017
        /// Explanation : This function is used calculate the output grid values when there is an early payoff date is provided. Early payoff date will force to pay all
        /// the remaining amount till the date of early payoff.
        /// </summary>
        /// <param name="outputGrid"></param>
        /// <param name="scheduleInput"></param>
        /// <param name="originationFee"></param>
        /// <param name="sameDayFee"></param>
        /// <param name="outputSchedule"></param>
        public static void CalculateEarlyPayOff(List<OutputGrid> outputGrid, LoanDetails scheduleInput, double originationFee, double sameDayFee, OutputSchedule outputSchedule)
        {
            DateTime earlyPayOffDate = scheduleInput.EarlyPayoffDate;
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class EarlyPayOff and method name CalculateEarlyPayOff(List<OutputGrid> outputGrid, LoanDetails scheduleInput, double originationFee, double sameDayFee, OutputSchedule outputSchedule).");

                //Check whether early payoff date is graeter than the last repayment scheudled date of output grid, then no calculation is required.
                if (earlyPayOffDate > outputGrid[outputGrid.Count - 1].PaymentDate)
                {
                    return;
                }
                //Check whether early payoff date is equal to the last repayment scheudled date of output grid, then no calculation is required.
                else if (scheduleInput.ManagementFeeFrequency != (int)Constants.FeeFrequency.Days && scheduleInput.ManagementFeeFrequency != (int)Constants.FeeFrequency.Months &&
                        scheduleInput.MaintenanceFeeFrequency != (int)Constants.FeeFrequency.Days && scheduleInput.MaintenanceFeeFrequency != (int)Constants.FeeFrequency.Months &&
                        earlyPayOffDate <= (scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].EffectiveDate == DateTime.MinValue ?
                                            scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].DateIn :
                                            scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].EffectiveDate) &&
                        earlyPayOffDate == outputGrid[outputGrid.Count - 1].PaymentDate && outputGrid[outputGrid.Count - 1].InterestCarryOver == 0 &&
                        outputGrid[outputGrid.Count - 1].serviceFeeInterestCarryOver == 0 && outputGrid[outputGrid.Count - 1].CumulativeTotalPastDue == 0)
                {
                    outputGrid[outputGrid.Count - 1].Flags = (int)Constants.FlagValues.EarlyPayOff;
                    return;
                }

                //Determines the index in output grid, where the payment date is greater than or equal to the early pay off date.
                int lastRowOfOutputGrid = (outputGrid.FindLastIndex(o => o.PaymentDate == earlyPayOffDate) == -1 ?
                                                            outputGrid.FindIndex(o => o.PaymentDate > earlyPayOffDate) :
                                                            outputGrid.FindLastIndex(o => o.PaymentDate == earlyPayOffDate));

                if (outputGrid[lastRowOfOutputGrid].PaymentDate == earlyPayOffDate && scheduleInput.AfterPayment)
                {
                    lastRowOfOutputGrid += 1;
                    if (lastRowOfOutputGrid == outputGrid.Count)
                    {
                        outputGrid.Insert(lastRowOfOutputGrid, new OutputGrid
                        {

                        });
                    }
                }

                double interestPastDue = 0;
                double serviceFeeInterestPastDue = 0;
                double originationFeePastDue = originationFee;
                double sameDayFeePastDue = sameDayFee;
                double managementFeePastDue = 0;
                double maintenanceFeePastDue = 0;

                //Determine the past due NSF and Late fee amounts, that has been added to the schedule.
                double nsfFeePastDue = scheduleInput.AdditionalPaymentRecords.Where(o => o.DateIn <= earlyPayOffDate && o.Flags == (int)Constants.FlagValues.NSFFee).Sum(o => o.AdditionalPayment);
                double lateFeePastDue = scheduleInput.AdditionalPaymentRecords.Where(o => o.DateIn <= earlyPayOffDate && o.Flags == (int)Constants.FlagValues.LateFee).Sum(o => o.AdditionalPayment);

                nsfFeePastDue += scheduleInput.EarnedNSFFee;
                lateFeePastDue += scheduleInput.EarnedLateFee;

                //This loop determines all of the past due amounts leaving the NSF and Late fee values, to be paid.
                for (int i = 0; i < lastRowOfOutputGrid; i++)
                {
                    interestPastDue += (outputGrid[i].InterestAccrued - outputGrid[i].InterestPaid);
                    serviceFeeInterestPastDue += (outputGrid[i].AccruedServiceFeeInterest - outputGrid[i].ServiceFeeInterestPaid);
                    originationFeePastDue -= outputGrid[i].OriginationFeePaid;
                    sameDayFeePastDue -= outputGrid[i].SameDayFeePaid;
                    managementFeePastDue = managementFeePastDue + (outputGrid[i].Flags == (int)Constants.FlagValues.SkipPayment ? 0 : (outputGrid[i].ManagementFee - outputGrid[i].ManagementFeePaid));
                    maintenanceFeePastDue = maintenanceFeePastDue + (outputGrid[i].Flags == (int)Constants.FlagValues.SkipPayment ? 0 : (outputGrid[i].MaintenanceFee - outputGrid[i].MaintenanceFeePaid));
                    nsfFeePastDue -= outputGrid[i].NSFFeePaid;
                    lateFeePastDue -= outputGrid[i].LateFeePaid;
                }

                if (scheduleInput.AdditionalPaymentRecords.FindIndex(o => o.Flags == (int)Constants.FlagValues.ManagementFee) != -1)
                {
                    managementFeePastDue += scheduleInput.AdditionalPaymentRecords.Where(a => outputGrid.Any(o => o.Flags == (int)Constants.FlagValues.ManagementFee &&
                                                                                o.PaymentDate <= earlyPayOffDate && o.PaymentID == a.PaymentID)).Sum(o => o.AdditionalPayment);
                    //o.PaymentDate <= outputGrid[lastRowOfOutputGrid].PaymentDate && o.PaymentID == a.PaymentID)).Sum(o => o.AdditionalPayment);
                }
                if (scheduleInput.AdditionalPaymentRecords.FindIndex(o => o.Flags == (int)Constants.FlagValues.MaintenanceFee) != -1)
                {
                    maintenanceFeePastDue += scheduleInput.AdditionalPaymentRecords.Where(a => outputGrid.Any(o => o.Flags == (int)Constants.FlagValues.MaintenanceFee &&
                                                                                o.PaymentDate <= earlyPayOffDate && o.PaymentID == a.PaymentID)).Sum(o => o.AdditionalPayment);
                    //o.PaymentDate <= outputGrid[lastRowOfOutputGrid].PaymentDate && o.PaymentID == a.PaymentID)).Sum(o => o.AdditionalPayment);
                }

                //Determine the total remaining principal amount to be paid. If early payoff is on or before first row of output grid. Default principal amount is taken, 
                //otherwise remaining principal of row before lastRowOfOutputGrid is deducted from paid amount of that row.
                double remainingPrincipal = lastRowOfOutputGrid == 0 ? outputGrid[lastRowOfOutputGrid].BeginningPrincipal :
                                                    outputGrid[lastRowOfOutputGrid - 1].BeginningPrincipal - outputGrid[lastRowOfOutputGrid - 1].PrincipalPaid;
                remainingPrincipal = Round.RoundOffAmount(remainingPrincipal);

                //Determine the total remaining service fee amount to be paid. If early payoff is on or before first row of output grid. Default service fee amount is taken, 
                //otherwise remaining service fee of row before lastRowOfOutputGrid is deducted from paid amount of that row.
                double remainingPrincipalServiceFee = lastRowOfOutputGrid == 0 ? outputGrid[lastRowOfOutputGrid].BeginningServiceFee :
                                                    outputGrid[lastRowOfOutputGrid - 1].BeginningServiceFee - outputGrid[lastRowOfOutputGrid - 1].ServiceFeePaid;
                remainingPrincipalServiceFee = Round.RoundOffAmount(remainingPrincipalServiceFee);

                double payablePrincipal = (remainingPrincipal < Round.RoundOffAmount(remainingPrincipal - scheduleInput.Residual) ? remainingPrincipal :
                                                                Round.RoundOffAmount(remainingPrincipal - scheduleInput.Residual));

                DateTime startDate;
                int row = outputGrid.Take(lastRowOfOutputGrid).ToList().FindLastIndex(o => o.Flags != (int)Constants.FlagValues.NSFFee &&
                                                                o.Flags != (int)Constants.FlagValues.LateFee && o.Flags != (int)Constants.FlagValues.ManagementFee &&
                                                                o.Flags != (int)Constants.FlagValues.MaintenanceFee);

                //Checks if the row which is neither NSF fee nor late fee event, doesn't exists.
                if (row == -1)
                {
                    //Select the scheduled date of input grid if no effective payment date is provided, otherwise effective date is being taken, of first row.
                    startDate = scheduleInput.InputRecords[0].EffectiveDate == DateTime.MinValue ? scheduleInput.InputRecords[0].DateIn : scheduleInput.InputRecords[0].EffectiveDate;
                }
                else
                {
                    //Select payment date from index row, of output grid
                    startDate = outputGrid[row].PaymentDate;
                }
                DateTime endDate = earlyPayOffDate;

                bool isScheduleInput = scheduleInput.InputRecords.Exists(o => o.DateIn == endDate || o.EffectiveDate == endDate);
                isScheduleInput = scheduleInput.AfterPayment ? false : isScheduleInput;
                //This condition determines whether there is an interest rate in additional payment grid, for that row. If it is provided, that value will be used, otherwise
                //default value will be used for periodic interest calculations.
                sbTracing.AppendLine("Inside EarlyPayOff class, and CalculateEarlyPayOff() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, false);");
                double periodicInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, isScheduleInput, false);

                //Calculates the interest accrued for that period.
                sbTracing.AppendLine("Inside EarlyPayOff class, and CalculateEarlyPayOff() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, false);");
                double interestAccrued = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, isScheduleInput, false);
                interestAccrued = Round.RoundOffAmount(interestAccrued);

                //This variable calculates the daily interest amount for the period.
                sbTracing.AppendLine("Inside EarlyPayOff class, and CalculateEarlyPayOff() method. Calling method : DailyInterestAmount.CalculateDailyInterestAmount(startDate, endDate, interestAccrued, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, false);");
                double dailyInterestAmount = DailyInterestAmount.CalculateDailyInterestAmount(startDate, endDate, interestAccrued, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, scheduleInput, isScheduleInput, false);
                dailyInterestAmount = Round.RoundOffAmount(dailyInterestAmount);

                //Add the earned interest amount to the accrued value.
                interestAccrued += (lastRowOfOutputGrid == 0 ? scheduleInput.EarnedInterest : 0);

                //Assign accrued interest value to interest payment, then add carry over value and past due value, if exists.
                double interestPayment = interestAccrued + interestPastDue;

                double serviceFeeInterestAccrued = 0;
                double serviceFeeInterestPayable = 0;
                //Checks whether service fee interest is to be taken or not.
                if (scheduleInput.ApplyServiceFeeInterest == 1 || scheduleInput.ApplyServiceFeeInterest == 3)
                {
                    //Calculate the accrued service fee interest value.
                    sbTracing.AppendLine("Inside EarlyPayOff class, and CalculateEarlyPayOff() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipalServiceFee, true, false);");
                    serviceFeeInterestAccrued = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipalServiceFee, true, isScheduleInput, false);
                    serviceFeeInterestAccrued = Round.RoundOffAmount(serviceFeeInterestAccrued);
                }

                //Add earned service fee interest value to the accrued value.
                serviceFeeInterestAccrued += (lastRowOfOutputGrid == 0 ? scheduleInput.EarnedServiceFeeInterest : 0);
                //Add the past due and carry over value in accrued interest value.
                serviceFeeInterestPayable = serviceFeeInterestAccrued + serviceFeeInterestPastDue;

                //This variable calculates the daily interest rate for the period.
                sbTracing.AppendLine("Inside EarlyPayOff class, and CalculateEarlyPayOff() method. Calling method : DailyInterestRate.CalculateDailyInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, false);");
                double dailyInterestRate = DailyInterestRate.CalculateDailyInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, scheduleInput, isScheduleInput, false);

                //This condition checks whether service fee is selected as "Incremental" or not.
                if (scheduleInput.IsServiceFeeIncremental)
                {
                    int index;
                    if (outputGrid.FindLastIndex(o => o.PaymentDate == earlyPayOffDate &&
                            (o.Flags == (int)Constants.FlagValues.Payment || o.Flags == (int)Constants.FlagValues.SkipPayment)) != -1)
                    {
                        index = outputGrid.FindLastIndex(o => o.PaymentDate == earlyPayOffDate);
                        if (scheduleInput.AfterPayment)
                        {
                            remainingPrincipalServiceFee = outputGrid[index].ServiceFeePastDue;
                        }
                        else
                        {
                            remainingPrincipalServiceFee = outputGrid[index].ServiceFee;
                        }
                    }
                    else if (outputGrid.FindIndex(o => o.PaymentDate > earlyPayOffDate &&
                            (o.Flags == (int)Constants.FlagValues.Payment || o.Flags == (int)Constants.FlagValues.SkipPayment)) != -1)
                    {
                        index = outputGrid.FindIndex(o => o.PaymentDate > earlyPayOffDate &&
                               (o.Flags == (int)Constants.FlagValues.Payment || o.Flags == (int)Constants.FlagValues.SkipPayment));
                        remainingPrincipalServiceFee = outputGrid[index].ServiceFee;
                    }
                }

                //Assign maintenance fee if lastRowOfOutputGrid is not 0 index row of output grid.
                int lastManagementFeeChargedIndex = -1;
                int lastMaintenanceFeeChargedIndex = -1;
                int managementFeeTableIndex = 0;
                int maintenanceFeeTableIndex = 0;
                int inputGridRow = 1;
                int additionalGridRow = 0;
                for (int i = 0; i < lastRowOfOutputGrid; i++)
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
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, additionalGridRow, true, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);");
                        ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, additionalGridRow, true, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);
                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, additionalGridRow, true, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);");
                        ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, additionalGridRow, true, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);
                        additionalGridRow++;
                    }
                }

                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, lastRowOfOutputGrid, inputGridRow, false, false, true, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);");
                double managementFee = ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, lastRowOfOutputGrid, inputGridRow, false, false, true, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);
                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and AllocationOfFunds() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, lastRowOfOutputGrid, inputGridRow, false, false, true, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);");
                double maintenanceFee = ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, lastRowOfOutputGrid, inputGridRow, false, false, true, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);

                //Delete unused fee assessed rows.
                for (int i = outputSchedule.ManagementFeeAssessment.Count - 1; i > lastManagementFeeChargedIndex; i--)
                {
                    outputSchedule.ManagementFeeAssessment.RemoveAt(i);
                }
                for (int i = outputSchedule.MaintenanceFeeAssessment.Count - 1; i > lastMaintenanceFeeChargedIndex; i--)
                {
                    outputSchedule.MaintenanceFeeAssessment.RemoveAt(i);
                }

                #region Get management and maintenance fee of upper rows when payment is skipped

                if (scheduleInput.ManagementFeeFrequency != (int)Constants.FeeFrequency.AllPayments)
                {
                    int outputRow = lastRowOfOutputGrid - 1;
                    while (outputRow >= 0)
                    {
                        if (outputGrid[outputRow].Flags == (int)Constants.FlagValues.SkipPayment)
                        {
                            managementFee += outputGrid[outputRow].ManagementFee;
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
                    int outputRow = lastRowOfOutputGrid - 1;
                    while (outputRow >= 0)
                    {
                        if (outputGrid[outputRow].Flags == (int)Constants.FlagValues.SkipPayment)
                        {
                            managementFee += outputGrid[outputRow].ManagementFee;
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
                    int outputRow = lastRowOfOutputGrid - 1;
                    while (outputRow >= 0)
                    {
                        if (outputGrid[outputRow].Flags == (int)Constants.FlagValues.SkipPayment)
                        {
                            maintenanceFee += outputGrid[outputRow].MaintenanceFee;
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
                    int outputRow = lastRowOfOutputGrid - 1;
                    while (outputRow >= 0)
                    {
                        if (outputGrid[outputRow].Flags == (int)Constants.FlagValues.SkipPayment)
                        {
                            maintenanceFee += outputGrid[outputRow].MaintenanceFee;
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

                managementFee += (lastRowOfOutputGrid == 0 ? scheduleInput.EarnedManagementFee : 0);
                maintenanceFee += (lastRowOfOutputGrid == 0 ? scheduleInput.EarnedMaintenanceFee : 0);

                //Sum all the fee values to calculate the total payment amount to be paid.
                double totalAmountToPay = payablePrincipal + interestPayment + remainingPrincipalServiceFee + serviceFeeInterestPayable +
                                    originationFeePastDue +
                                    sameDayFeePastDue +
                                    (maintenanceFee + maintenanceFeePastDue) +
                                    (managementFee + managementFeePastDue) +
                                    nsfFeePastDue +
                                    lateFeePastDue;

                #region Make effective payoff date row in output grid

                outputGrid[lastRowOfOutputGrid].PaymentDate = endDate;
                outputGrid[lastRowOfOutputGrid].BeginningPrincipal = remainingPrincipal;
                outputGrid[lastRowOfOutputGrid].BeginningServiceFee = remainingPrincipalServiceFee;
                outputGrid[lastRowOfOutputGrid].PeriodicInterestRate = periodicInterestRate;
                outputGrid[lastRowOfOutputGrid].DailyInterestRate = dailyInterestRate;
                outputGrid[lastRowOfOutputGrid].DailyInterestAmount = dailyInterestAmount;
                outputGrid[lastRowOfOutputGrid].PaymentID = 0;
                outputGrid[lastRowOfOutputGrid].Flags = (int)Constants.FlagValues.EarlyPayOff;
                outputGrid[lastRowOfOutputGrid].DueDate = endDate;
                outputGrid[lastRowOfOutputGrid].InterestAccrued = interestAccrued;
                outputGrid[lastRowOfOutputGrid].InterestCarryOver = 0;
                outputGrid[lastRowOfOutputGrid].InterestPayment = interestPayment;
                outputGrid[lastRowOfOutputGrid].PrincipalPayment = payablePrincipal;
                outputGrid[lastRowOfOutputGrid].TotalPayment = totalAmountToPay;
                outputGrid[lastRowOfOutputGrid].AccruedServiceFeeInterest = serviceFeeInterestAccrued;
                outputGrid[lastRowOfOutputGrid].AccruedServiceFeeInterestCarryOver = 0;
                outputGrid[lastRowOfOutputGrid].ServiceFee = remainingPrincipalServiceFee;
                outputGrid[lastRowOfOutputGrid].ServiceFeeInterest = serviceFeeInterestPayable;
                outputGrid[lastRowOfOutputGrid].ServiceFeeTotal = (remainingPrincipalServiceFee + serviceFeeInterestPayable);
                outputGrid[lastRowOfOutputGrid].OriginationFee = originationFeePastDue;
                outputGrid[lastRowOfOutputGrid].MaintenanceFee = maintenanceFee;
                outputGrid[lastRowOfOutputGrid].ManagementFee = managementFee;
                outputGrid[lastRowOfOutputGrid].SameDayFee = sameDayFeePastDue;
                outputGrid[lastRowOfOutputGrid].NSFFee = nsfFeePastDue;
                outputGrid[lastRowOfOutputGrid].LateFee = lateFeePastDue;

                //Add past due amounts to the variables.
                maintenanceFee += maintenanceFeePastDue;
                managementFee += managementFeePastDue;

                //Paid amount columns
                outputGrid[lastRowOfOutputGrid].PrincipalPaid = payablePrincipal;
                outputGrid[lastRowOfOutputGrid].InterestPaid = interestPayment;
                outputGrid[lastRowOfOutputGrid].ServiceFeePaid = remainingPrincipalServiceFee;
                outputGrid[lastRowOfOutputGrid].ServiceFeeInterestPaid = serviceFeeInterestPayable;
                outputGrid[lastRowOfOutputGrid].OriginationFeePaid = originationFeePastDue;
                outputGrid[lastRowOfOutputGrid].MaintenanceFeePaid = maintenanceFee;
                outputGrid[lastRowOfOutputGrid].ManagementFeePaid = managementFee;
                outputGrid[lastRowOfOutputGrid].SameDayFeePaid = sameDayFeePastDue;
                outputGrid[lastRowOfOutputGrid].NSFFeePaid = nsfFeePastDue;
                outputGrid[lastRowOfOutputGrid].LateFeePaid = lateFeePastDue;
                outputGrid[lastRowOfOutputGrid].TotalPaid = totalAmountToPay;
                //Past due amount columns
                outputGrid[lastRowOfOutputGrid].PrincipalPastDue = 0;
                outputGrid[lastRowOfOutputGrid].InterestPastDue = 0;
                outputGrid[lastRowOfOutputGrid].ServiceFeePastDue = 0;
                outputGrid[lastRowOfOutputGrid].ServiceFeeInterestPastDue = 0;
                outputGrid[lastRowOfOutputGrid].OriginationFeePastDue = 0;
                outputGrid[lastRowOfOutputGrid].MaintenanceFeePastDue = 0;
                outputGrid[lastRowOfOutputGrid].ManagementFeePastDue = 0;
                outputGrid[lastRowOfOutputGrid].SameDayFeePastDue = 0;
                outputGrid[lastRowOfOutputGrid].NSFFeePastDue = 0;
                outputGrid[lastRowOfOutputGrid].LateFeePastDue = 0;
                outputGrid[lastRowOfOutputGrid].TotalPastDue = 0;
                //Cumulative past due amount columns
                outputGrid[lastRowOfOutputGrid].CumulativePrincipalPastDue = 0;
                outputGrid[lastRowOfOutputGrid].CumulativeInterestPastDue = 0;
                outputGrid[lastRowOfOutputGrid].CumulativeServiceFeePastDue = 0;
                outputGrid[lastRowOfOutputGrid].CumulativeServiceFeeInterestPastDue = 0;
                outputGrid[lastRowOfOutputGrid].CumulativeOriginationFeePastDue = 0;
                outputGrid[lastRowOfOutputGrid].CumulativeMaintenanceFeePastDue = 0;
                outputGrid[lastRowOfOutputGrid].CumulativeManagementFeePastDue = 0;
                outputGrid[lastRowOfOutputGrid].CumulativeSameDayFeePastDue = 0;
                outputGrid[lastRowOfOutputGrid].CumulativeNSFFeePastDue = 0;
                outputGrid[lastRowOfOutputGrid].CumulativeLateFeePastDue = 0;
                outputGrid[lastRowOfOutputGrid].CumulativeTotalPastDue = 0;
                outputGrid[lastRowOfOutputGrid].BucketStatus = "";

                #endregion

                //Clear all past due amounts
                for (int i = 0; i <= lastRowOfOutputGrid; i++)
                {
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
                    outputGrid[i].BucketStatus = "";
                }

                //Remove all the remaining rows after effective date row from output grid.
                for (int i = outputGrid.Count - 1; i > lastRowOfOutputGrid; i--)
                {
                    outputGrid.RemoveAt(i);
                }
                sbTracing.AppendLine("Exist:From class EarlyPayOff and method name CalculateEarlyPayOff(List<OutputGrid> outputGrid, LoanDetails scheduleInput, double originationFee, double sameDayFee, OutputSchedule outputSchedule)");
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
    }
}
