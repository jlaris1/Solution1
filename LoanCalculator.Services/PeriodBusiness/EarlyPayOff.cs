using LoanAmort_driver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoanAmort_driver.PeriodBusiness
{
    public class EarlyPayOff
    {
        /// <summary>
        /// Name : Sparsh Agarwal
        /// Date : 10/10/2017
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
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside class EarlyPayOff and method name CalculateEarlyPayOff(List<OutputGrid> outputGrid, LoanDetails scheduleInput, double originationFee, double sameDayFee, OutputSchedule outputSchedule).");
                sbTracing.AppendLine("Parameter values are : originationFee = " + originationFee + ", sameDayFee = " + sameDayFee);

                DateTime earlyPayOffDate = scheduleInput.EarlyPayoffDate;

                if (earlyPayOffDate > outputGrid[outputGrid.Count - 1].PaymentDate)
                {
                    return;
                }

                //Check whether early payoff date is equal to the last repayment scheudled date of output grid, then no calculation is required.
                if (scheduleInput.ManagementFeeFrequency != (int)Constants.FeeFrequency.Days && scheduleInput.ManagementFeeFrequency != (int)Constants.FeeFrequency.Months &&
                        scheduleInput.MaintenanceFeeFrequency != (int)Constants.FeeFrequency.Days && scheduleInput.MaintenanceFeeFrequency != (int)Constants.FeeFrequency.Months &&
                        earlyPayOffDate <= (scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].EffectiveDate == DateTime.MinValue ?
                                            scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].DateIn :
                                            scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].EffectiveDate) &&
                        earlyPayOffDate == outputGrid[outputGrid.Count - 1].PaymentDate && outputGrid[outputGrid.Count - 1].InterestCarryOver == 0 &&
                        outputGrid[outputGrid.Count - 1].serviceFeeInterestCarryOver == 0 && outputGrid[outputGrid.Count - 1].CumulativeTotalPastDue == 0)
                {
                    if (scheduleInput.InputRecords.FindIndex(o => o.DateIn == earlyPayOffDate && (o.Flags == (int)Constants.FlagValues.Payment || o.Flags == (int)Constants.FlagValues.SkipPayment)) != -1
                            && scheduleInput.IsServiceFeeIncremental)
                    {
                        outputGrid[outputGrid.Count - 1].TotalPaid = Round.RoundOffAmount(outputGrid[outputGrid.Count - 1].TotalPaid - outputGrid[outputGrid.Count - 1].ServiceFeePaid);
                        outputGrid[outputGrid.Count - 1].ServiceFeePaid = outputGrid[outputGrid.Count - 1].ServiceFee;
                        outputGrid[outputGrid.Count - 1].TotalPaid = Round.RoundOffAmount(outputGrid[outputGrid.Count - 1].TotalPaid + outputGrid[outputGrid.Count - 1].ServiceFeePaid);
                        outputGrid[outputGrid.Count - 1].TotalPayment = outputGrid[outputGrid.Count - 1].TotalPaid;
                        outputGrid[outputGrid.Count - 1].PaymentDue = outputGrid[outputGrid.Count - 1].TotalPaid;
                    }

                    outputGrid[outputGrid.Count - 1].Flags = (int)Constants.FlagValues.EarlyPayOff;
                    return;
                }

                //Determines the index in output grid, where the payment date is greater than or equal to the early pay off date.
                int outputGridRow = outputGrid.FindLastIndex(o => o.PaymentDate == earlyPayOffDate) == -1 ?
                                                            outputGrid.FindIndex(o => o.PaymentDate > earlyPayOffDate) :
                                                            outputGrid.FindLastIndex(o => o.PaymentDate == earlyPayOffDate);

                if (outputGrid[outputGridRow].PaymentDate == earlyPayOffDate && scheduleInput.AfterPayment)
                {
                    outputGridRow += 1;
                    if (outputGridRow == outputGrid.Count)
                    {
                        outputGrid.Insert(outputGridRow, new OutputGrid
                        {

                        });
                    }
                }

                for (int i = 0; i < outputGridRow; i++)
                {
                    originationFee = Round.RoundOffAmount(originationFee - outputGrid[i].OriginationFeePaid);
                    sameDayFee = Round.RoundOffAmount(sameDayFee - outputGrid[i].SameDayFeePaid);
                }

                //Determine the total remaining principal and service fee amount to be paid. If early payoff is on or before first row of output grid. Default principal and
                //service fee amount is taken, otherwise the row before lastRowOfOutputGrid is deducted from paid amount of that row.
                double remainingPrincipal = outputGridRow == 0 ? outputGrid[outputGridRow].BeginningPrincipal :
                                                    outputGrid[outputGridRow - 1].BeginningPrincipal - outputGrid[outputGridRow - 1].PrincipalPaid;
                double remainingPrincipalServiceFee = outputGridRow == 0 ? outputGrid[outputGridRow].BeginningServiceFee :
                                                    outputGrid[outputGridRow - 1].BeginningServiceFee - outputGrid[outputGridRow - 1].ServiceFeePaid;

                double payablePrincipal = (remainingPrincipal < Round.RoundOffAmount(remainingPrincipal - scheduleInput.Residual) ? remainingPrincipal :
                                                                Round.RoundOffAmount(remainingPrincipal - scheduleInput.Residual));

                //Determine the last index in output grid where interest was accrued.
                int lastOutputRow = outputGrid.Take(outputGridRow).ToList().FindLastIndex(o => o.Flags != (int)Constants.FlagValues.ManagementFee &&
                                                                                                o.Flags != (int)Constants.FlagValues.MaintenanceFee);

                DateTime startDate;
                if (lastOutputRow == -1)
                {
                    startDate = scheduleInput.InputRecords[0].EffectiveDate == DateTime.MinValue ? scheduleInput.InputRecords[0].DateIn :
                                                                                                    scheduleInput.InputRecords[0].EffectiveDate;
                }
                else
                {
                    startDate = outputGrid[lastOutputRow].PaymentDate;
                }
                DateTime endDate = earlyPayOffDate;
                bool isScheduleInput = scheduleInput.InputRecords.Exists(o => o.DateIn == endDate || o.EffectiveDate == endDate);
                isScheduleInput = scheduleInput.AfterPayment ? false : isScheduleInput;
                //This condition determines whether there is an interest rate in additional payment grid, for that row. If it is provided, that value will be used, otherwise
                //default value will be used for periodic interest calculations.
                sbTracing.AppendLine("Inside EarlyPayOff class, and CalculateEarlyPayOff() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, isScheduleInput);");
                double periodicInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, isScheduleInput, false);

                //Calculates the interest accrued for that period.
                sbTracing.AppendLine("Inside EarlyPayOff class, and CalculateEarlyPayOff() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, isScheduleInput);");
                double interestAccrued = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipal, true, isScheduleInput, false);
                interestAccrued = Round.RoundOffAmount(interestAccrued);

                //This variable calculates the daily interest amount for the period.
                sbTracing.AppendLine("Inside EarlyPayOff class, and CalculateEarlyPayOff() method. Calling method : DailyInterestAmount.CalculateDailyInterestAmount(startDate, endDate, interestAccrued, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay,scheduleInput, isScheduleInput);");
                double dailyInterestAmount = DailyInterestAmount.CalculateDailyInterestAmount(startDate, endDate, interestAccrued, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, scheduleInput, isScheduleInput);
                dailyInterestAmount = Round.RoundOffAmount(dailyInterestAmount);

                //Add the earned interest amount to the accrued value.
                interestAccrued += (outputGridRow == 0 ? scheduleInput.EarnedInterest : 0);

                double interestDue = interestAccrued + (outputGridRow == 0 ? 0 : (outputGrid[outputGridRow - 1].InterestPastDue +
                                                                                    outputGrid[outputGridRow - 1].InterestCarryOver));

                double serviceFeeInterestAccrued = 0;
                if (scheduleInput.ApplyServiceFeeInterest == 1 || scheduleInput.ApplyServiceFeeInterest == 3)
                {
                    //Calculate the accrued service fee interest value.
                    sbTracing.AppendLine("Inside EarlyPayOff class, and CalculateEarlyPayOff() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipalServiceFee, true, isScheduleInput);");
                    serviceFeeInterestAccrued = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, remainingPrincipalServiceFee, true, isScheduleInput, false);
                    serviceFeeInterestAccrued = Round.RoundOffAmount(serviceFeeInterestAccrued);
                }

                //Add earned service fee interest value to the accrued value.
                serviceFeeInterestAccrued += (outputGridRow == 0 ? scheduleInput.EarnedServiceFeeInterest : 0);
                //Add the past due and carry over value in accrued interest value.
                double serviceFeeInterestDue = serviceFeeInterestAccrued + (outputGridRow == 0 ? 0 : (outputGrid[outputGridRow - 1].ServiceFeeInterestPastDue +
                                                                                               outputGrid[outputGridRow - 1].AccruedServiceFeeInterestCarryOver));

                sbTracing.AppendLine("Inside EarlyPayOff class, and CalculateEarlyPayOff() method. Calling method : DailyInterestRate.CalculateDailyInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InterestDelay,scheduleInput, isScheduleInput);");
                double dailyInterestRate = DailyInterestRate.CalculateDailyInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay,scheduleInput, isScheduleInput);

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
                for (int i = 0; i < outputGridRow; i++)
                {
                    if (inputGridRow < scheduleInput.InputRecords.Count && scheduleInput.InputRecords[inputGridRow].DateIn == outputGrid[i].DueDate &&
                            scheduleInput.InputRecords[inputGridRow].Flags == outputGrid[i].Flags)
                    {
                        sbTracing.AppendLine("Inside EarlyPayOff class, and CalculateEarlyPayOff() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, inputGridRow, false, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);");
                        ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, inputGridRow, false, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);
                        sbTracing.AppendLine("Inside EarlyPayOff class, and CalculateEarlyPayOff() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, inputGridRow, false, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);");
                        ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, inputGridRow, false, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);
                        inputGridRow++;
                    }
                    else
                    {
                        sbTracing.AppendLine("Inside EarlyPayOff class, and CalculateEarlyPayOff() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, additionalGridRow, true, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);");
                        ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, additionalGridRow, true, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);
                        sbTracing.AppendLine("Inside EarlyPayOff class, and CalculateEarlyPayOff() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, additionalGridRow, true, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);");
                        ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, additionalGridRow, true, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);
                        additionalGridRow++;
                    }
                }

                sbTracing.AppendLine("Inside EarlyPayOff class, and CalculateEarlyPayOff() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRow, inputGridRow, false, false, true, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);");
                double managementFee = ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRow, inputGridRow, false, false, true, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);
                sbTracing.AppendLine("Inside EarlyPayOff class, and CalculateEarlyPayOff() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRow, inputGridRow, false, false, true, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);");
                double maintenanceFee = ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRow, inputGridRow, false, false, true, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);
                managementFee = managementFee + (outputGrid[outputGridRow].Flags == (int)Constants.FlagValues.ManagementFee ?
                                                        outputGrid[outputGridRow].ManagementFeePastDue :
                                                        (outputGridRow == 0 ? scheduleInput.EarnedManagementFee : outputGrid[outputGridRow - 1].ManagementFeePastDue));
                maintenanceFee = maintenanceFee + (outputGrid[outputGridRow].Flags == (int)Constants.FlagValues.MaintenanceFee ?
                                                        outputGrid[outputGridRow].MaintenanceFeePastDue :
                                                        (outputGridRow == 0 ? scheduleInput.EarnedMaintenanceFee : outputGrid[outputGridRow - 1].MaintenanceFeePastDue));

                //Delete unused fee assessed rows.
                for (int i = outputSchedule.ManagementFeeAssessment.Count - 1; i > lastManagementFeeChargedIndex; i--)
                {
                    outputSchedule.ManagementFeeAssessment.RemoveAt(i);
                }
                for (int i = outputSchedule.MaintenanceFeeAssessment.Count - 1; i > lastMaintenanceFeeChargedIndex; i--)
                {
                    outputSchedule.MaintenanceFeeAssessment.RemoveAt(i);
                }

                double nsfFeePastDue = (outputGrid[outputGridRow].Flags == (int)Constants.FlagValues.NSFFee ? outputGrid[outputGridRow].NSFFeePastDue :
                                                                                (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].NSFFeePastDue));
                double lateFeePastDue = (outputGrid[outputGridRow].Flags == (int)Constants.FlagValues.LateFee ? outputGrid[outputGridRow].LateFeePastDue :
                                                                                (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].LateFeePastDue));

                nsfFeePastDue += (outputGridRow == 0 ? scheduleInput.EarnedNSFFee : 0);
                lateFeePastDue += (outputGridRow == 0 ? scheduleInput.EarnedLateFee : 0);

                //Sum all the fee values to calculate the total payment amount to be paid.
                double totalAmountToPay = payablePrincipal + interestDue + remainingPrincipalServiceFee + serviceFeeInterestDue +
                                        originationFee + sameDayFee + maintenanceFee + managementFee +
                                        nsfFeePastDue + lateFeePastDue;

                #region Make effective payoff date row in output grid

                outputGrid[outputGridRow].PaymentDate = endDate;
                outputGrid[outputGridRow].BeginningPrincipal = remainingPrincipal;
                outputGrid[outputGridRow].BeginningServiceFee = remainingPrincipalServiceFee;
                outputGrid[outputGridRow].PeriodicInterestRate = periodicInterestRate;
                outputGrid[outputGridRow].DailyInterestRate = dailyInterestRate;
                outputGrid[outputGridRow].DailyInterestAmount = dailyInterestAmount;
                outputGrid[outputGridRow].PaymentID = 0;
                outputGrid[outputGridRow].Flags = (int)Constants.FlagValues.EarlyPayOff;
                outputGrid[outputGridRow].DueDate = endDate;
                outputGrid[outputGridRow].InterestAccrued = interestAccrued;
                outputGrid[outputGridRow].InterestDue = interestDue;
                outputGrid[outputGridRow].InterestCarryOver = 0;
                outputGrid[outputGridRow].InterestPayment = interestDue;
                outputGrid[outputGridRow].PrincipalPayment = payablePrincipal;
                outputGrid[outputGridRow].PaymentDue = totalAmountToPay;
                outputGrid[outputGridRow].TotalPayment = totalAmountToPay;
                outputGrid[outputGridRow].AccruedServiceFeeInterest = serviceFeeInterestAccrued;
                outputGrid[outputGridRow].ServiceFeeInterestDue = serviceFeeInterestDue;
                outputGrid[outputGridRow].AccruedServiceFeeInterestCarryOver = 0;
                outputGrid[outputGridRow].ServiceFeeInterest = serviceFeeInterestDue;
                outputGrid[outputGridRow].ServiceFee = remainingPrincipalServiceFee;
                outputGrid[outputGridRow].ServiceFeeTotal = remainingPrincipalServiceFee + serviceFeeInterestDue;
                outputGrid[outputGridRow].OriginationFee = originationFee;
                outputGrid[outputGridRow].MaintenanceFee = maintenanceFee;
                outputGrid[outputGridRow].ManagementFee = managementFee;
                outputGrid[outputGridRow].SameDayFee = sameDayFee;
                outputGrid[outputGridRow].NSFFee = nsfFeePastDue;
                outputGrid[outputGridRow].LateFee = lateFeePastDue;
                //Paid amount columns
                outputGrid[outputGridRow].PrincipalPaid = payablePrincipal;
                outputGrid[outputGridRow].InterestPaid = interestDue;
                outputGrid[outputGridRow].ServiceFeePaid = remainingPrincipalServiceFee;
                outputGrid[outputGridRow].ServiceFeeInterestPaid = serviceFeeInterestDue;
                outputGrid[outputGridRow].OriginationFeePaid = originationFee;
                outputGrid[outputGridRow].MaintenanceFeePaid = maintenanceFee;
                outputGrid[outputGridRow].ManagementFeePaid = managementFee;
                outputGrid[outputGridRow].SameDayFeePaid = sameDayFee;
                outputGrid[outputGridRow].NSFFeePaid = nsfFeePastDue;
                outputGrid[outputGridRow].LateFeePaid = lateFeePastDue;
                outputGrid[outputGridRow].TotalPaid = totalAmountToPay;
                //Cumulative Amount paid column
                outputGrid[outputGridRow].CumulativePrincipal = payablePrincipal + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePrincipal);
                outputGrid[outputGridRow].CumulativeInterest = interestDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeInterest);
                outputGrid[outputGridRow].CumulativePayment = totalAmountToPay + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativePayment);
                outputGrid[outputGridRow].CumulativeServiceFee = remainingPrincipalServiceFee + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFee);
                outputGrid[outputGridRow].CumulativeServiceFeeInterest = serviceFeeInterestDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeInterest);
                outputGrid[outputGridRow].CumulativeServiceFeeTotal = (remainingPrincipalServiceFee + serviceFeeInterestDue) + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeServiceFeeTotal);
                outputGrid[outputGridRow].CumulativeOriginationFee = originationFee + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeOriginationFee);
                outputGrid[outputGridRow].CumulativeMaintenanceFee = maintenanceFee + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeMaintenanceFee);
                outputGrid[outputGridRow].CumulativeManagementFee = managementFee + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeManagementFee);
                outputGrid[outputGridRow].CumulativeSameDayFee = sameDayFee + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeSameDayFee);
                outputGrid[outputGridRow].CumulativeNSFFee = nsfFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeNSFFee);
                outputGrid[outputGridRow].CumulativeLateFee = lateFeePastDue + (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeLateFee);
                outputGrid[outputGridRow].CumulativeTotalFees = (outputGridRow == 0 ? 0 : outputGrid[outputGridRow - 1].CumulativeTotalFees) +
                                                    remainingPrincipalServiceFee + serviceFeeInterestDue + originationFee + maintenanceFee + managementFee + sameDayFee +
                                                    nsfFeePastDue + lateFeePastDue;
                //Past due amount columns
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
                //Cumulative past due amount columns
                outputGrid[outputGridRow].CumulativePrincipalPastDue = 0;
                outputGrid[outputGridRow].CumulativeInterestPastDue = 0;
                outputGrid[outputGridRow].CumulativeServiceFeePastDue = 0;
                outputGrid[outputGridRow].CumulativeServiceFeeInterestPastDue = 0;
                outputGrid[outputGridRow].CumulativeOriginationFeePastDue = 0;
                outputGrid[outputGridRow].CumulativeMaintenanceFeePastDue = 0;
                outputGrid[outputGridRow].CumulativeManagementFeePastDue = 0;
                outputGrid[outputGridRow].CumulativeSameDayFeePastDue = 0;
                outputGrid[outputGridRow].CumulativeNSFFeePastDue = 0;
                outputGrid[outputGridRow].CumulativeLateFeePastDue = 0;
                outputGrid[outputGridRow].CumulativeTotalPastDue = 0;

                #endregion

                //Remove all the remaining rows after effective date row from output grid.
                for (int i = outputGrid.Count - 1; i > (totalAmountToPay == 0 ? (outputGridRow - 1) : outputGridRow); i--)
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
