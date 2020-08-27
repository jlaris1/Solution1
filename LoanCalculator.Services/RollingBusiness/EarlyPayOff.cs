using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoanAmort_driver.Models;

namespace LoanAmort_driver.RollingBusiness
{
    public class EarlyPayOff
    {
        /// <summary>
        /// Name : Punit Singh
        /// Date : 21/07/2017
        /// Explanation : This function is used calculate the output grid values when there is an early payoff date is provided. Early payoff date will force to pay all
        /// the remaining amount till the date of early payoff.
        /// </summary>
        /// <param name="outputGrid"></param>
        /// <param name="scheduleInput"></param>
        /// <param name="outputSchedule"></param>
        /// <param name="originationFee"></param>
        /// <param name="sameDayFee"></param>
        public static void EarlyPayOffCalculation(List<PaymentDetail> outputGrid, getScheduleInput scheduleInput, getScheduleOutput outputSchedule)
        {
            #region
            StringBuilder sbTracing = new StringBuilder();
            DateTime endDate = Convert.ToDateTime(Constants.DefaultDate);
            DateTime startDate = Convert.ToDateTime(Constants.DefaultDate);
            double dailyInterestRate = 0;
            double dailyInterestAmount = 0;
            double periodicInterestRate = 0;
            double principalPayment = 0;
            double interestAccrued = 0;
            double serviceFeeInterest = 0;
            int managementFeeTableIndex = 0;
            int maintenanceFeeTableIndex = 0;
            double managementFeePayable = 0;
            double maintenanceFeePayable = 0;
            int lastManagementFeeChargedIndex = -1;
            int lastMaintenanceFeeChargedIndex = -1;
            int additionalGridRowIndex = 0;
            int inputGridRowIndex = 1;
            int startDateIndex = -1;
            double totalResidualPrincipal = Round.RoundOffAmount(outputSchedule.LoanAmountCalc - scheduleInput.Residual);
            #endregion
            try
            {
                if (outputGrid.Count > 0)
                {
                    #region Count index where we can add the new schedule for additional payment.
                    for (int i = 0; i < outputGrid.Count && scheduleInput.EarlyPayoffDate >= outputGrid[i].PaymentDate; i++)
                    {
                        if ((i == outputGrid.FindLastIndex(o => o.PaymentDate == scheduleInput.EarlyPayoffDate)) && scheduleInput.EarlyPayoffDate == outputGrid[i].PaymentDate)
                        {
                            break;
                        }
                        else
                        {
                            DateTime lastInputDate = (scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].EffectiveDate == DateTime.MinValue ? scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].DateIn :
                                        scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].EffectiveDate);

                            bool isExtended = outputGrid[i].PaymentDate > lastInputDate ? true : false;
                            if ((outputGrid[i].Flags != (int)Constants.FlagValues.Payment && outputGrid[i].Flags != (int)Constants.FlagValues.SkipPayment))
                            {
                                sbTracing.AppendLine("Inside LoanAmort_driver.RollingBusiness, ActualPaymentAllocation class, and PrincipalOnlyPayment() Calling method : ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, inputGridRow, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);");
                                managementFeePayable = ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, additionalGridRowIndex, true, isExtended, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);
                                sbTracing.AppendLine("Inside LoanAmort_driver.RollingBusiness, ActualPaymentAllocation class, and PrincipalOnlyPayment() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, inputGridRow, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);");
                                maintenanceFeePayable = ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, additionalGridRowIndex, true, isExtended, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);
                                additionalGridRowIndex++;
                            }
                            else
                            {
                                sbTracing.AppendLine("Inside LoanAmort_driver.RollingBusiness, ActualPaymentAllocation class, and PrincipalOnlyPayment() Calling method : ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, inputGridRow, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);");
                                managementFeePayable = ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, inputGridRowIndex, false, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);
                                sbTracing.AppendLine("Inside LoanAmort_driver.RollingBusiness, ActualPaymentAllocation class, and PrincipalOnlyPayment() Calling method : ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, inputGridRow, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);");
                                maintenanceFeePayable = ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, i, inputGridRowIndex, false, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);
                                inputGridRowIndex++;
                            }
                        }
                    }
                    #endregion

                    for (int index = 0; index <= outputGrid.Count - 1; index++)
                    {
                        sbTracing.AppendLine("Enter:Inside class EarlyPayOff and method name EarlyPayOffCalculation(List<OutputGrid> outputGrid, LoanDetails scheduleInput, OutputSchedule outputSchedule).");

                        //Check whether early payoff date is graeter than the last repayment scheudled date of output grid, then no calculation is required.
                        if (scheduleInput.EarlyPayoffDate > outputGrid[outputGrid.Count - 1].PaymentDate)
                        {
                            return;
                        }
                        //Check whether early payoff date is equal to the last repayment scheudled date of output grid, then no calculation is required.
                        else if (scheduleInput.ManagementFeeFrequency != (int)Constants.FeeFrequency.Days && scheduleInput.ManagementFeeFrequency != (int)Constants.FeeFrequency.Months &&
                        scheduleInput.MaintenanceFeeFrequency != (int)Constants.FeeFrequency.Days && scheduleInput.MaintenanceFeeFrequency != (int)Constants.FeeFrequency.Months &&
                        scheduleInput.EarlyPayoffDate <= (scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].EffectiveDate == DateTime.MinValue ?
                                            scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].DateIn :
                                            scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].EffectiveDate) &&
                                  (scheduleInput.EarlyPayoffDate == outputGrid[outputGrid.Count - 1].PaymentDate) && (outputGrid[outputGrid.Count - 1].InterestCarryOver <= 0 && outputGrid[outputGrid.Count - 1].AccruedServiceFeeInterestCarryOver <= 0 &&
                                  outputGrid[outputGrid.Count - 1].PrincipalPastDue <= 0 && outputGrid[outputGrid.Count - 1].InterestPastDue <= 0 && outputGrid[outputGrid.Count - 1].ServiceFeePastDue <= 0 &&
                                  outputGrid[outputGrid.Count - 1].ServiceFeeInterestPastDue <= 0 && outputGrid[outputGrid.Count - 1].MaintenanceFeePastDue <= 0 && outputGrid[outputGrid.Count - 1].ManagementFeePastDue <= 0 &&
                                  outputGrid[outputGrid.Count - 1].OriginationFeePastDue <= 0 && outputGrid[outputGrid.Count - 1].SameDayFeePastDue <= 0 && outputGrid[outputGrid.Count - 1].NSFFeePastDue <= 0 && outputGrid[outputGrid.Count - 1].LateFeePastDue <= 0))
                        {
                            outputGrid[outputGrid.Count - 1].Flags = (int)Constants.FlagValues.EarlyPayOff;
                            return;
                        }

                        if (scheduleInput.EarlyPayoffDate == outputGrid[index].PaymentDate)
                        {
                            index = outputGrid.FindLastIndex(o => o.PaymentDate == scheduleInput.EarlyPayoffDate);
                            if (index == outputGrid.Count - 1 && scheduleInput.AfterPayment)
                            {
                                outputGrid.Insert(outputGrid.Count, new PaymentDetail
                                {
                                    BeginningPrincipal = outputGrid[outputGrid.Count - 1].PrincipalPastDue,
                                    BeginningServiceFee = outputGrid[outputGrid.Count - 1].ServiceFeePastDue,
                                    SameDayFee = outputGrid[outputGrid.Count - 1].SameDayFeePastDue,
                                    OriginationFee = outputGrid[outputGrid.Count - 1].OriginationFeePastDue,
                                    NSFFee = outputGrid[outputGrid.Count - 1].NSFFeePastDue,
                                    LateFee = outputGrid[outputGrid.Count - 1].LateFeePastDue,
                                    MaintenanceFee = outputGrid[outputGrid.Count - 1].MaintenanceFeePastDue,
                                    ManagementFee = outputGrid[outputGrid.Count - 1].ManagementFeePastDue,
                                    DueDate = scheduleInput.EarlyPayoffDate
                                });
                            }
                            if (scheduleInput.AfterPayment)
                            {
                                
                                if (outputGrid[index].PaymentDate == scheduleInput.EarlyPayoffDate)
                                {
                                    DateTime lastInputDate = (scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].EffectiveDate == DateTime.MinValue ? scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].DateIn :
                                             scheduleInput.InputRecords[scheduleInput.InputRecords.Count - 1].EffectiveDate);

                                    bool isExtended = outputGrid[index].PaymentDate > lastInputDate ? true : false;
                                    if ((outputGrid[index].Flags != (int)Constants.FlagValues.Payment && outputGrid[index].Flags != (int)Constants.FlagValues.SkipPayment))
                                    {
                                        sbTracing.AppendLine("Inside LoanAmort_driver.RollingBusiness, ActualPaymentAllocation class, and PrincipalOnlyPayment() Calling method : ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, inputGridRow, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);");
                                        managementFeePayable = ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, index + 1, additionalGridRowIndex, true, isExtended, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);
                                        sbTracing.AppendLine("Inside LoanAmort_driver.RollingBusiness, ActualPaymentAllocation class, and PrincipalOnlyPayment() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, inputGridRow, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);");
                                        maintenanceFeePayable = ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, index + 1, additionalGridRowIndex, true, isExtended, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);
                                        additionalGridRowIndex++;
                                    }
                                    else
                                    {
                                        sbTracing.AppendLine("Inside LoanAmort_driver.RollingBusiness, ActualPaymentAllocation class, and PrincipalOnlyPayment() Calling method : ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, inputGridRow, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);");
                                        managementFeePayable = ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, index + 1, inputGridRowIndex, false, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);
                                        sbTracing.AppendLine("Inside LoanAmort_driver.RollingBusiness, ActualPaymentAllocation class, and PrincipalOnlyPayment() Calling method : ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, inputGridRow, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);");
                                        maintenanceFeePayable = ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, index + 1, inputGridRowIndex, false, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);
                                        inputGridRowIndex++;
                                    }
                                }
                                index = index + 1;
                            }

                            #region Allocate amount for last row of the schedule which is equals to EarlyPayOff Date
                            int sameDateIndex = (scheduleInput.EarlyPayoffDate == outputGrid[index].PaymentDate) ?
                                          outputGrid.FindLastIndex(o => o.PaymentDate == scheduleInput.EarlyPayoffDate) : -1;
                            int countSameDateIndex = (scheduleInput.EarlyPayoffDate == outputGrid[index].PaymentDate) ?
                                          outputGrid.Count(o => o.PaymentDate == scheduleInput.EarlyPayoffDate) : -1;
                            if (countSameDateIndex > 1)
                            {
                                index = sameDateIndex;
                                outputGrid[index].InterestAccrued = 0;
                                outputGrid[index].AccruedServiceFeeInterest = 0;
                            }

                            sbTracing.AppendLine("Inside EarlyPayOff class, and EarlyPayOffCalculation() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, inputGridRow, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);");
                            managementFeePayable = ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, index, inputGridRowIndex, false, false, true, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);
                            sbTracing.AppendLine("Inside EarlyPayOff class, and EarlyPayOffCalculation() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, inputGridRow, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);");
                            maintenanceFeePayable = ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, index, inputGridRowIndex, false, false, true, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);

                            double serviceFeeWhenIncreamental = outputGrid[index].BeginningServiceFee;
                            if (scheduleInput.IsServiceFeeIncremental && (outputGrid.FindIndex(o => o.PaymentDate >= scheduleInput.EarlyPayoffDate &&
                                    (o.Flags == (int)Constants.FlagValues.Payment || o.Flags == (int)Constants.FlagValues.SkipPayment)) != -1))
                            {
                                int serviceFeeIndex = outputGrid.FindIndex(o => o.PaymentDate >= scheduleInput.EarlyPayoffDate &&
                                    (o.Flags == (int)Constants.FlagValues.Payment || o.Flags == (int)Constants.FlagValues.SkipPayment));
                                serviceFeeWhenIncreamental = outputGrid[serviceFeeIndex].ServiceFee;
                            }


                            for (int i = outputGrid.Count - 1; i > index; i--)
                            {
                                outputGrid.RemoveAt(i);
                            }
                            bool isScheduleInput;
                            #region Calculate Priodic Intrest Rate
                            startDateIndex = outputGrid.Take(index).ToList().FindLastIndex(o => o.Flags != (int)Constants.FlagValues.ManagementFee && o.Flags != (int)Constants.FlagValues.MaintenanceFee);
                            if (startDateIndex == -1)
                            {
                                //Find start date
                                startDate = scheduleInput.InputRecords[0].EffectiveDate == DateTime.MinValue ? scheduleInput.InputRecords[0].DateIn : scheduleInput.InputRecords[0].EffectiveDate;
                                //Find end date
                                endDate = scheduleInput.EarlyPayoffDate;
                                isScheduleInput = scheduleInput.InputRecords.Exists(o => o.DateIn == endDate || o.EffectiveDate == endDate);
                                isScheduleInput = scheduleInput.AfterPayment ? false : isScheduleInput;
                                periodicInterestRate = CalculatePeriodicInterestRate.CalcPeriodicInterestRateWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, outputGrid[index].BeginningPrincipal, true, isScheduleInput, false);
                            }
                            else
                            {
                                //Find start date
                                startDate = outputGrid[startDateIndex].PaymentDate;
                                //Find end date
                                endDate = scheduleInput.EarlyPayoffDate;
                                isScheduleInput = scheduleInput.InputRecords.Exists(o => o.DateIn == endDate || o.EffectiveDate == endDate);
                                isScheduleInput = scheduleInput.AfterPayment ? false : isScheduleInput;
                                periodicInterestRate = CalculatePeriodicInterestRate.CalcPeriodicInterestRateWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, outputGrid[index].BeginningPrincipal, true, isScheduleInput, false);
                            }
                            #endregion

                            //Calculating interestAccrued using Tier interest rates.
                            interestAccrued = PeriodicInterestAmount.CalcAccruedInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, outputGrid[index].BeginningPrincipal, true, isScheduleInput, false);

                            #region Calculate Daily Interest rate and interest amount 
                            //This variable calculates the daily interest rate for the period.
                            sbTracing.AppendLine("Inside LoanAmort_driver.RollingBusiness, EarlyPayOff class, and EarlyPayOffCalculation() Calling method : DailyInterestRate.CalculateDailyInterestRate(scheduleInputs.DaysInYearBankMethod, " +
                                "scheduleInputs.DaysInMonth, startDate, endDate, periodicInterestRate);");
                            dailyInterestRate = DailyInterestRate.CalculateDailyInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, scheduleInput, isScheduleInput);
                            //This variable calculates the daily interest amount for the period.
                            sbTracing.AppendLine("Inside LoanAmort_driver.RollingBusiness, EarlyPayOff class, and EarlyPayOffCalculation() Calling method : DailyInterestAmount.CalculateDailyInterestAmount(scheduleInputs.DaysInYearBankMethod, " +
                                "scheduleInputs.DaysInMonth, startDate, endDate, periodicInterestRate * principal);");
                            dailyInterestAmount = DailyInterestAmount.CalculateDailyInterestAmount(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, endDate, interestAccrued, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, scheduleInput, isScheduleInput);
                            dailyInterestAmount = Round.RoundOffAmount(dailyInterestAmount);
                            #endregion

                            outputGrid[index].DailyInterestRate = dailyInterestRate;
                            outputGrid[index].DailyInterestAmount = dailyInterestAmount;

                            interestAccrued = index == 0 ? interestAccrued + scheduleInput.EarnedInterest : interestAccrued;
                            interestAccrued = Round.RoundOffAmount(interestAccrued);
                            serviceFeeInterest = (scheduleInput.ApplyServiceFeeInterest == 0 || scheduleInput.ApplyServiceFeeInterest == 2) ? 0 : PeriodicInterestAmount.CalcAccruedInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, outputGrid[index].BeginningServiceFee, true, isScheduleInput, false);
                            serviceFeeInterest = index == 0 ? serviceFeeInterest + scheduleInput.EarnedServiceFeeInterest : serviceFeeInterest;
                            serviceFeeInterest = Round.RoundOffAmount(serviceFeeInterest);

                            outputGrid[index].InterestAccrued = interestAccrued;
                            outputGrid[index].AccruedServiceFeeInterest = serviceFeeInterest;
                            outputGrid[index].PeriodicInterestRate = periodicInterestRate;
                            principalPayment = Round.RoundOffAmount(outputGrid[index].BeginningPrincipal);

                            if (scheduleInput.Residual > 0)
                            {
                                #region  For the Residual principal amount
                                totalResidualPrincipal = Round.RoundOffAmount(outputSchedule.LoanAmountCalc - scheduleInput.Residual);
                                for (int x = 0; x <= index - 1; x++)
                                {
                                    totalResidualPrincipal = Round.RoundOffAmount(totalResidualPrincipal - outputGrid[x].PrincipalPaid);
                                }
                                totalResidualPrincipal = totalResidualPrincipal > 0 ? totalResidualPrincipal : 0;

                                if (totalResidualPrincipal > principalPayment)
                                {
                                    totalResidualPrincipal = Round.RoundOffAmount(totalResidualPrincipal - principalPayment);
                                }
                                else
                                {
                                    if (totalResidualPrincipal > 0)
                                    {
                                        principalPayment = totalResidualPrincipal;
                                        totalResidualPrincipal = 0;
                                    }
                                    else
                                    {
                                        if (totalResidualPrincipal == 0 && (outputGrid[index].BeginningServiceFee > 0 || outputGrid[index - 1].OriginationFeePastDue > 0 || outputGrid[index - 1].SameDayFeePastDue > 0 || outputGrid[index - 1].AccruedServiceFeeInterestCarryOver > 0 ||
                               outputGrid[index - 1].InterestPastDue > 0 || outputGrid[index - 1].ServiceFeeInterestPastDue > 0 || outputGrid[index - 1].InterestCarryOver > 0 || outputGrid[index - 1].MaintenanceFeePastDue > 0 || outputGrid[index - 1].ManagementFeePastDue > 0))
                                        {
                                            totalResidualPrincipal = 0;
                                            principalPayment = 0;
                                        }
                                        else
                                        {
                                            totalResidualPrincipal = 0;
                                            principalPayment = 0;
                                            outputGrid[index].InterestAccrued = 0;
                                            outputGrid[index].DailyInterestAmount = 0;
                                        }
                                    }
                                }
                                #endregion
                            }

                            outputGrid[index].BeginningServiceFee = serviceFeeWhenIncreamental;

                            if (index == 0)
                            {
                                #region
                                outputGrid[index].OriginationFee = Round.RoundOffAmount(outputGrid[index].OriginationFee);
                                outputGrid[index].SameDayFee = Round.RoundOffAmount(outputGrid[index].SameDayFee);
                                outputGrid[index].NSFFee = Round.RoundOffAmount(outputGrid[index].NSFFee);
                                outputGrid[index].LateFee = Round.RoundOffAmount(outputGrid[index].LateFee);
                                outputGrid[index].BeginningPrincipal = Round.RoundOffAmount(outputGrid[index].BeginningPrincipal);
                                outputGrid[index].PrincipalPayment = Round.RoundOffAmount(principalPayment);
                                outputGrid[index].InterestCarryOver = 0;
                                outputGrid[index].InterestPayment = Round.RoundOffAmount((outputGrid[index].InterestAccrued));
                                outputGrid[index].PeriodicInterestRate = outputGrid[index].PeriodicInterestRate;
                                outputGrid[index].DailyInterestRate = outputGrid[index].DailyInterestRate;
                                outputGrid[index].DailyInterestAmount = Round.RoundOffAmount(outputGrid[index].DailyInterestAmount);
                                outputGrid[index].BeginningServiceFee = outputGrid[index].BeginningServiceFee;
                                outputGrid[index].ServiceFeeInterest = Round.RoundOffAmount(outputGrid[index].AccruedServiceFeeInterest);
                                outputGrid[index].AccruedServiceFeeInterestCarryOver = 0;
                                outputGrid[index].ServiceFee = Round.RoundOffAmount(outputGrid[index].BeginningServiceFee);
                                outputGrid[index].ServiceFeeTotal = Round.RoundOffAmount((outputGrid[index].ServiceFee + outputGrid[index].ServiceFeeInterest));
                                outputGrid[index].MaintenanceFee = index == 0 ? maintenanceFeePayable + scheduleInput.EarnedMaintenanceFee : (outputGrid[index].Flags == (int)Constants.FlagValues.MaintenanceFee ? Round.RoundOffAmount(outputGrid[index].MaintenanceFee)
                                                                  : Round.RoundOffAmount(maintenanceFeePayable));
                                outputGrid[index].ManagementFee = index == 0 ? managementFeePayable + scheduleInput.EarnedManagementFee : (outputGrid[index].Flags == (int)Constants.FlagValues.ManagementFee ? Round.RoundOffAmount(outputGrid[index].ManagementFee)
                                                                  : Round.RoundOffAmount(managementFeePayable));

                                outputGrid[index].PrincipalPaid = Round.RoundOffAmount(outputGrid[index].PrincipalPayment);
                                outputGrid[index].InterestPaid = Round.RoundOffAmount(outputGrid[index].InterestPayment);
                                outputGrid[index].ServiceFeePaid = Round.RoundOffAmount(outputGrid[index].ServiceFee);
                                outputGrid[index].ServiceFeeInterestPaid = Round.RoundOffAmount(outputGrid[index].ServiceFeeInterest);
                                outputGrid[index].OriginationFeePaid = Round.RoundOffAmount(outputGrid[index].OriginationFee);
                                outputGrid[index].SameDayFeePaid = Round.RoundOffAmount(outputGrid[index].SameDayFee);
                                outputGrid[index].MaintenanceFeePaid = index == 0 ? maintenanceFeePayable + scheduleInput.EarnedMaintenanceFee : (outputGrid[index].Flags == (int)Constants.FlagValues.MaintenanceFee ? Round.RoundOffAmount(outputGrid[index].MaintenanceFee)
                                                                  : Round.RoundOffAmount(maintenanceFeePayable));
                                outputGrid[index].ManagementFeePaid = index == 0 ? managementFeePayable + scheduleInput.EarnedManagementFee : (outputGrid[index].Flags == (int)Constants.FlagValues.ManagementFee ? Round.RoundOffAmount(outputGrid[index].ManagementFee)
                                                                  : Round.RoundOffAmount(managementFeePayable));
                                outputGrid[index].NSFFeePaid = Round.RoundOffAmount(outputGrid[index].NSFFee);
                                outputGrid[index].LateFeePaid = Round.RoundOffAmount(outputGrid[index].LateFee);
                                outputGrid[index].Flags = (int)Constants.FlagValues.EarlyPayOff;
                                outputGrid[index].ServiceFeeTotal = Round.RoundOffAmount((outputGrid[index].ServiceFee + outputGrid[index].ServiceFeeInterest));

                                outputGrid[index].TotalPayment = outputGrid[index].PrincipalPayment + outputGrid[index].InterestPayment +
                                                                   outputGrid[index].ServiceFee + outputGrid[index].ServiceFeeInterest +
                                                                   outputGrid[index].OriginationFee + outputGrid[index].SameDayFee +
                                                                   outputGrid[index].MaintenanceFee + outputGrid[index].ManagementFee +
                                                                   outputGrid[index].NSFFee + outputGrid[index].LateFee;
                                outputGrid[index].TotalPayment = Round.RoundOffAmount(outputGrid[index].TotalPayment);

                                outputGrid[index].PaymentDue = outputGrid[index].PrincipalPayment + outputGrid[index].InterestPayment +
                                                             outputGrid[index].ServiceFee + outputGrid[index].ServiceFeeInterest +
                                                             outputGrid[index].OriginationFee + outputGrid[index].SameDayFee +
                                                             outputGrid[index].MaintenanceFee + outputGrid[index].ManagementFee +
                                                             outputGrid[index].NSFFee + outputGrid[index].LateFee;
                                outputGrid[index].PaymentDue = Round.RoundOffAmount(outputGrid[index].PaymentDue);

                                outputGrid[index].TotalPaid = outputGrid[index].PrincipalPaid + outputGrid[index].InterestPaid +
                                                            outputGrid[index].ServiceFeePaid + outputGrid[index].ServiceFeeInterestPaid +
                                                            outputGrid[index].OriginationFeePaid + outputGrid[index].SameDayFeePaid +
                                                            outputGrid[index].MaintenanceFeePaid + outputGrid[index].ManagementFeePaid +
                                                            outputGrid[index].NSFFeePaid + outputGrid[index].LateFeePaid;
                                outputGrid[index].TotalPaid = Round.RoundOffAmount(outputGrid[index].TotalPaid);
                                outputGrid[index].PaymentDate = endDate;
                                #endregion
                            }
                            else
                            {
                                #region
                                outputGrid[index].OriginationFee = Round.RoundOffAmount(outputGrid[index - 1].OriginationFeePastDue);
                                outputGrid[index].SameDayFee = Round.RoundOffAmount(outputGrid[index - 1].SameDayFeePastDue);
                                outputGrid[index].NSFFee = Round.RoundOffAmount(outputGrid[index].NSFFee);
                                outputGrid[index].LateFee = Round.RoundOffAmount(outputGrid[index].LateFee);
                                outputGrid[index].BeginningPrincipal = Round.RoundOffAmount(outputGrid[index].BeginningPrincipal);
                                outputGrid[index].PrincipalPayment = Round.RoundOffAmount(principalPayment);
                                outputGrid[index].InterestCarryOver = 0;
                                outputGrid[index].InterestPayment = Round.RoundOffAmount((outputGrid[index].InterestAccrued + Round.RoundOffAmount(outputGrid[index - 1].InterestCarryOver) + Round.RoundOffAmount(outputGrid[index - 1].InterestPastDue)));
                                outputGrid[index].PeriodicInterestRate = outputGrid[index].PeriodicInterestRate;
                                outputGrid[index].DailyInterestRate = outputGrid[index].DailyInterestRate;
                                outputGrid[index].DailyInterestAmount = Round.RoundOffAmount(outputGrid[index].DailyInterestAmount);
                                outputGrid[index].BeginningServiceFee = outputGrid[index].BeginningServiceFee;
                                outputGrid[index].AccruedServiceFeeInterestCarryOver = 0;
                                outputGrid[index].ServiceFeeInterest = Round.RoundOffAmount((outputGrid[index].AccruedServiceFeeInterest + outputGrid[index - 1].ServiceFeeInterestPastDue + Round.RoundOffAmount(outputGrid[index - 1].AccruedServiceFeeInterestCarryOver)));
                                outputGrid[index].ServiceFee = Round.RoundOffAmount(outputGrid[index].BeginningServiceFee);
                                outputGrid[index].MaintenanceFee = outputGrid[index].Flags == (int)Constants.FlagValues.MaintenanceFee ? Round.RoundOffAmount(outputGrid[index].MaintenanceFee)
                                                                        : Round.RoundOffAmount((maintenanceFeePayable + outputGrid[index - 1].MaintenanceFeePastDue));
                                outputGrid[index].ManagementFee = outputGrid[index].Flags == (int)Constants.FlagValues.ManagementFee ? Round.RoundOffAmount(outputGrid[index].ManagementFee)
                                                                  : Round.RoundOffAmount((managementFeePayable + outputGrid[index - 1].ManagementFeePastDue));

                                outputGrid[index].PrincipalPaid = Round.RoundOffAmount(outputGrid[index].PrincipalPayment);
                                outputGrid[index].InterestPaid = Round.RoundOffAmount(outputGrid[index].InterestPayment);
                                outputGrid[index].ServiceFeePaid = Round.RoundOffAmount(outputGrid[index].ServiceFee);
                                outputGrid[index].ServiceFeeInterestPaid = Round.RoundOffAmount(outputGrid[index].ServiceFeeInterest);
                                outputGrid[index].OriginationFeePaid = Round.RoundOffAmount(outputGrid[index].OriginationFee);
                                outputGrid[index].SameDayFeePaid = Round.RoundOffAmount(outputGrid[index].SameDayFee);
                                outputGrid[index].MaintenanceFeePaid = outputGrid[index].Flags == (int)Constants.FlagValues.MaintenanceFee ? Round.RoundOffAmount(outputGrid[index].MaintenanceFee)
                                                                        : Round.RoundOffAmount((maintenanceFeePayable + outputGrid[index - 1].MaintenanceFeePastDue));
                                outputGrid[index].ManagementFeePaid = outputGrid[index].Flags == (int)Constants.FlagValues.ManagementFee ? Round.RoundOffAmount(outputGrid[index].ManagementFee)
                                                                  : Round.RoundOffAmount((managementFeePayable + outputGrid[index - 1].ManagementFeePastDue));
                                outputGrid[index].NSFFeePaid = Round.RoundOffAmount(outputGrid[index].NSFFee);
                                outputGrid[index].LateFeePaid = Round.RoundOffAmount(outputGrid[index].LateFee);

                                outputGrid[index].Flags = (int)Constants.FlagValues.EarlyPayOff;

                                outputGrid[index].ServiceFeeTotal = Round.RoundOffAmount((outputGrid[index].ServiceFee + outputGrid[index].ServiceFeeInterest));
                                outputGrid[index].TotalPayment = outputGrid[index].PrincipalPayment + outputGrid[index].InterestPayment +
                                                                   outputGrid[index].ServiceFee + outputGrid[index].ServiceFeeInterest +
                                                                   outputGrid[index].OriginationFee + outputGrid[index].SameDayFee +
                                                                    outputGrid[index].MaintenanceFee + outputGrid[index].ManagementFee +
                                                                    outputGrid[index].NSFFee + outputGrid[index].LateFee;
                                outputGrid[index].TotalPayment = Round.RoundOffAmount(outputGrid[index].TotalPayment);

                                outputGrid[index].PaymentDue = outputGrid[index].PrincipalPayment + outputGrid[index].InterestPayment +
                                                             outputGrid[index].ServiceFee + outputGrid[index].ServiceFeeInterest +
                                                             outputGrid[index].OriginationFee + outputGrid[index].SameDayFee +
                                                              outputGrid[index].MaintenanceFee + outputGrid[index].ManagementFee +
                                                              outputGrid[index].NSFFee + outputGrid[index].LateFee;
                                outputGrid[index].PaymentDue = Round.RoundOffAmount(outputGrid[index].PaymentDue);

                                outputGrid[index].TotalPaid = outputGrid[index].PrincipalPaid + outputGrid[index].InterestPaid +
                                                            outputGrid[index].ServiceFeePaid + outputGrid[index].ServiceFeeInterestPaid +
                                                            outputGrid[index].OriginationFeePaid + outputGrid[index].SameDayFeePaid +
                                                            outputGrid[index].MaintenanceFeePaid + outputGrid[index].ManagementFeePaid +
                                                            outputGrid[index].NSFFeePaid + outputGrid[index].LateFeePaid;
                                outputGrid[index].TotalPaid = Round.RoundOffAmount(outputGrid[index].TotalPaid);
                                outputGrid[index].PaymentDate = endDate;
                                outputGrid[index].DueDate = endDate;
                                #endregion
                            }
                            //Set all past due amount to zero(0).
                            outputGrid[index].PrincipalPastDue = 0;
                            outputGrid[index].InterestPastDue = 0;
                            outputGrid[index].ServiceFeePastDue = 0;
                            outputGrid[index].ServiceFeeInterestPastDue = 0;
                            outputGrid[index].OriginationFeePastDue = 0;
                            outputGrid[index].MaintenanceFeePastDue = 0;
                            outputGrid[index].ManagementFeePastDue = 0;
                            outputGrid[index].SameDayFeePastDue = 0;
                            outputGrid[index].NSFFeePastDue = 0;
                            outputGrid[index].LateFeePastDue = 0;
                            outputGrid[index].TotalPastDue = 0;

                            //Cumulative amount past due is added in current past due row
                            outputGrid[index].CumulativePrincipalPastDue = 0;
                            outputGrid[index].CumulativeInterestPastDue = 0;
                            outputGrid[index].CumulativeServiceFeePastDue = 0;
                            outputGrid[index].CumulativeServiceFeeInterestPastDue = 0;
                            outputGrid[index].CumulativeOriginationFeePastDue = 0;
                            outputGrid[index].CumulativeMaintenanceFeePastDue = 0;
                            outputGrid[index].CumulativeManagementFeePastDue = 0;
                            outputGrid[index].CumulativeSameDayFeePastDue = 0;
                            outputGrid[index].CumulativeNSFFeePastDue = 0;
                            outputGrid[index].CumulativeLateFeePastDue = 0;
                            outputGrid[index].CumulativeTotalPastDue = 0;

                            #endregion
                        }
                        else if ((scheduleInput.EarlyPayoffDate < outputGrid[index].PaymentDate) && (scheduleInput.EarlyPayoffDate > scheduleInput.InputRecords[0].DateIn))
                        {
                            #region//When EarlyPayOff Date before the first scheduled date but greater than effective date of the loan

                            periodicInterestRate = 0;
                            #region Calculate Priodic Intrest Rate
                            if (index <= 0)
                            {
                                //Calculate Periodic Interest rate when when it comes before the first schedule date
                                startDate = scheduleInput.InputRecords[0].DateIn;
                                //It determines the end date. Interest will be calculated till the end date for that period.
                                endDate = scheduleInput.EarlyPayoffDate;
                                //This variable calls a function which calculate the periodic interest rate for that period.
                                periodicInterestRate = CalculatePeriodicInterestRate.CalcPeriodicInterestRateWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, outputGrid[index].BeginningPrincipal, true, false, false);
                            }
                            //Calculating interesrAccrued using tier's interest rate.
                            interestAccrued = PeriodicInterestAmount.CalcAccruedInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, outputGrid[index].BeginningPrincipal, true, false, false);

                            #endregion

                            #region Calculate Daily Interest rate and interest amount 
                            //This variable calculates the daily interest rate for the period.
                            sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : DailyInterestRate.CalculateDailyInterestRate(scheduleInputs.DaysInYearBankMethod, " +
                                "scheduleInputs.DaysInMonth, startDate, endDate, periodicInterestRate);");
                            dailyInterestRate = DailyInterestRate.CalculateDailyInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, scheduleInput, false);
                            //This variable calculates the daily interest amount for the period.
                            sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : DailyInterestAmount.CalculateDailyInterestAmount(scheduleInputs.DaysInYearBankMethod, " +
                                "scheduleInputs.DaysInMonth, startDate, endDate, periodicInterestRate * principal);");
                            dailyInterestAmount = DailyInterestAmount.CalculateDailyInterestAmount(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, endDate, interestAccrued, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, scheduleInput, false);
                            dailyInterestAmount = Round.RoundOffAmount(dailyInterestAmount);
                            #endregion

                            sbTracing.AppendLine("Inside EarlyPayOff class, and EarlyPayOffCalculation() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, inputGridRow, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);");
                            managementFeePayable = ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, index, inputGridRowIndex, false, false, true, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);
                            sbTracing.AppendLine("Inside EarlyPayOff class, and EarlyPayOffCalculation() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, inputGridRow, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);");
                            maintenanceFeePayable = ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, index, inputGridRowIndex, false, false, true, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);

                            maintenanceFeePayable += index == 0 ? scheduleInput.EarnedMaintenanceFee : 0;
                            managementFeePayable += index == 0 ? scheduleInput.EarnedManagementFee : 0;

                            interestAccrued = Round.RoundOffAmount(interestAccrued);
                            interestAccrued += index == 0 ? scheduleInput.EarnedInterest : 0;
                            interestAccrued = Round.RoundOffAmount(interestAccrued);

                            serviceFeeInterest = PeriodicInterestAmount.CalcAccruedInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, outputGrid[index].BeginningServiceFee, true, false, false);
                            serviceFeeInterest = Round.RoundOffAmount(serviceFeeInterest);
                            serviceFeeInterest += index == 0 ? scheduleInput.EarnedServiceFeeInterest : 0;
                            serviceFeeInterest = Round.RoundOffAmount(serviceFeeInterest);

                            principalPayment = Round.RoundOffAmount(outputGrid[index].BeginningPrincipal);

                            if (scheduleInput.Residual > 0)
                            {
                                #region  For the Residual principal amount
                                totalResidualPrincipal = Round.RoundOffAmount(outputSchedule.LoanAmountCalc - scheduleInput.Residual);
                                for (int x = 0; x <= index - 1; x++)
                                {
                                    totalResidualPrincipal = Round.RoundOffAmount(totalResidualPrincipal - outputGrid[x].PrincipalPaid);
                                }
                                totalResidualPrincipal = totalResidualPrincipal > 0 ? totalResidualPrincipal : 0;

                                if (totalResidualPrincipal > principalPayment)
                                {
                                    totalResidualPrincipal = Round.RoundOffAmount(totalResidualPrincipal - principalPayment);
                                }
                                else
                                {
                                    if (totalResidualPrincipal > 0)
                                    {
                                        principalPayment = totalResidualPrincipal;
                                        totalResidualPrincipal = 0;
                                    }
                                    else
                                    {
                                        if (totalResidualPrincipal == 0 && (outputGrid[index].BeginningServiceFee > 0 || outputGrid[index - 1].OriginationFeePastDue > 0 || outputGrid[index - 1].SameDayFeePastDue > 0 || outputGrid[index - 1].AccruedServiceFeeInterestCarryOver > 0 ||
                               outputGrid[index - 1].InterestPastDue > 0 || outputGrid[index - 1].ServiceFeeInterestPastDue > 0 || outputGrid[index - 1].InterestCarryOver > 0 || outputGrid[index - 1].MaintenanceFeePastDue > 0 || outputGrid[index - 1].ManagementFeePastDue > 0))
                                        {
                                            totalResidualPrincipal = 0;
                                            principalPayment = 0;
                                        }
                                        else
                                        {
                                            totalResidualPrincipal = 0;
                                            principalPayment = 0;
                                            interestAccrued = 0;
                                            dailyInterestAmount = 0;
                                        }
                                    }
                                }
                                #endregion
                            }

                            if (scheduleInput.IsServiceFeeIncremental && (outputGrid.FindIndex(o => o.PaymentDate > scheduleInput.EarlyPayoffDate &&
                                  (o.Flags == (int)Constants.FlagValues.Payment || o.Flags == (int)Constants.FlagValues.SkipPayment)) != -1))
                            {
                                int serviceFeeIndex = outputGrid.FindIndex(o => o.PaymentDate > scheduleInput.EarlyPayoffDate &&
                                    (o.Flags == (int)Constants.FlagValues.Payment || o.Flags == (int)Constants.FlagValues.SkipPayment));
                                outputGrid[index].BeginningServiceFee = outputGrid[serviceFeeIndex].ServiceFee;
                            }

                            #region Add schedule for Early Payoff date
                            outputGrid.Insert(index, new PaymentDetail
                            {
                                PaymentDate = endDate,
                                BeginningPrincipal = outputGrid[index].BeginningPrincipal,
                                BeginningServiceFee = outputGrid[index].BeginningServiceFee,
                                PeriodicInterestRate = periodicInterestRate,
                                DailyInterestRate = dailyInterestRate,
                                DailyInterestAmount = dailyInterestAmount,
                                PaymentID = Convert.ToInt32(scheduleInput.InputRecords[index].PaymentID.ToString()),
                                Flags = (int)Constants.FlagValues.EarlyPayOff,
                                DueDate = endDate,
                                InterestAccrued = interestAccrued,
                                InterestCarryOver = 0,
                                InterestPayment = interestAccrued,
                                PrincipalPayment = principalPayment,
                                PaymentDue = Round.RoundOffAmount((principalPayment + interestAccrued
                                               + outputGrid[index].BeginningServiceFee + serviceFeeInterest
                                               + outputGrid[index].OriginationFee
                                               + outputGrid[index].SameDayFee + maintenanceFeePayable + managementFeePayable)),

                                TotalPayment = Round.RoundOffAmount((principalPayment + interestAccrued
                                               + outputGrid[index].BeginningServiceFee + serviceFeeInterest
                                               + outputGrid[index].OriginationFee
                                               + outputGrid[index].SameDayFee + maintenanceFeePayable + managementFeePayable)),
                                AccruedServiceFeeInterest = serviceFeeInterest,
                                AccruedServiceFeeInterestCarryOver = 0,
                                ServiceFee = outputGrid[index].BeginningServiceFee,
                                ServiceFeeInterest = serviceFeeInterest,
                                ServiceFeeTotal = Round.RoundOffAmount((outputGrid[index].BeginningServiceFee + serviceFeeInterest)),
                                OriginationFee = Round.RoundOffAmount(outputGrid[index].OriginationFee),
                                MaintenanceFee = maintenanceFeePayable,
                                ManagementFee = managementFeePayable,
                                SameDayFee = Round.RoundOffAmount(outputGrid[index].SameDayFee),
                                NSFFee = Round.RoundOffAmount(outputGrid[index].NSFFee),
                                LateFee = Round.RoundOffAmount(outputGrid[index].LateFee),
                                //Paid amount columns
                                PrincipalPaid = principalPayment,
                                InterestPaid = interestAccrued,
                                ServiceFeePaid = outputGrid[index].BeginningServiceFee,
                                ServiceFeeInterestPaid = serviceFeeInterest,
                                OriginationFeePaid = outputGrid[index].OriginationFee,
                                MaintenanceFeePaid = maintenanceFeePayable,
                                ManagementFeePaid = managementFeePayable,
                                SameDayFeePaid = outputGrid[index].SameDayFee,
                                NSFFeePaid = outputGrid[index].NSFFee,
                                LateFeePaid = outputGrid[index].LateFee,
                                TotalPaid = Round.RoundOffAmount((principalPayment + interestAccrued
                                               + outputGrid[index].BeginningServiceFee + serviceFeeInterest
                                               + outputGrid[index].OriginationFee + outputGrid[index].LateFee
                                               + outputGrid[index].NSFFee
                                               + outputGrid[index].SameDayFee + managementFeePayable + maintenanceFeePayable)),
                                //Cumulative Amount paid column
                                CumulativeInterest = 0,
                                CumulativePrincipal = 0,
                                CumulativePayment = 0,
                                CumulativeServiceFee = 0,
                                CumulativeServiceFeeInterest = 0,
                                CumulativeServiceFeeTotal = 0,
                                CumulativeOriginationFee = 0,
                                CumulativeMaintenanceFee = 0,
                                CumulativeManagementFee = 0,
                                CumulativeSameDayFee = 0,
                                CumulativeNSFFee = 0,
                                CumulativeLateFee = 0,
                                CumulativeTotalFees = 0,
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
                                CumulativeTotalPastDue = 0,
                                BucketStatus = ""
                            });

                            #endregion

                            //Remove row after the early payoff date
                            for (int i = outputGrid.Count - 1; i > index; i--)
                            {
                                outputGrid.RemoveAt(i);
                            }
                            #endregion
                        }
                        else if ((scheduleInput.EarlyPayoffDate > outputGrid[index].PaymentDate) && (scheduleInput.EarlyPayoffDate < outputGrid[index + 1].PaymentDate))
                        {
                            #region//When EarlyPayOff Date comes between two scheduled dates.

                            #region Calculate Priodic Intrest Rate
                            startDateIndex = outputGrid.Take(index + 1).ToList().FindLastIndex(o => o.Flags != (int)Constants.FlagValues.ManagementFee && o.Flags != (int)Constants.FlagValues.MaintenanceFee);
                            if (startDateIndex == -1)
                            {
                                //Find start date
                                startDate = scheduleInput.InputRecords[0].EffectiveDate == DateTime.MinValue ? scheduleInput.InputRecords[0].DateIn : scheduleInput.InputRecords[0].EffectiveDate;
                                //Find end date
                                endDate = scheduleInput.EarlyPayoffDate;
                                periodicInterestRate = CalculatePeriodicInterestRate.CalcPeriodicInterestRateWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, outputGrid[index + 1].BeginningPrincipal, true, false, false);
                            }
                            else
                            {
                                //Find start date
                                startDate = outputGrid[startDateIndex].PaymentDate;
                                //Find end date
                                endDate = scheduleInput.EarlyPayoffDate;
                                periodicInterestRate = CalculatePeriodicInterestRate.CalcPeriodicInterestRateWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, outputGrid[index].BeginningPrincipal, true, false, false);
                            }
                            //Calculating interestAccrued using tier's interest rate.
                            interestAccrued = PeriodicInterestAmount.CalcAccruedInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, outputGrid[index + 1].BeginningPrincipal, true, false, false);

                            #endregion

                            #region Calculate Daily Interest rate and interest amount 
                            //This variable calculates the daily interest rate for the period.
                            sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : DailyInterestRate.CalculateDailyInterestRate(scheduleInputs.DaysInYearBankMethod, " +
                                "scheduleInputs.DaysInMonth, startDate, endDate, periodicInterestRate);");
                            dailyInterestRate = DailyInterestRate.CalculateDailyInterestRate(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, scheduleInput, false);
                            //This variable calculates the daily interest amount for the period.
                            sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : DailyInterestAmount.CalculateDailyInterestAmount(scheduleInputs.DaysInYearBankMethod, " +
                                "scheduleInputs.DaysInMonth, startDate, endDate, periodicInterestRate * principal);");
                            dailyInterestAmount = DailyInterestAmount.CalculateDailyInterestAmount(scheduleInput.DaysInYearBankMethod, scheduleInput.DaysInMonth, startDate, endDate, interestAccrued, scheduleInput.InputRecords[0].DateIn, scheduleInput.InterestDelay, scheduleInput, false);
                            dailyInterestAmount = Round.RoundOffAmount(dailyInterestAmount);
                            #endregion

                            sbTracing.AppendLine("Inside EarlyPayOff class, and EarlyPayOffCalculation() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, inputGridRow, false, false, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);");
                            managementFeePayable = ManagementAndMaintenanceFeeCalc.CalculateManagementFeeToCharge(scheduleInput, outputSchedule, outputGrid, index + 1, inputGridRowIndex, false, false, true, ref managementFeeTableIndex, ref lastManagementFeeChargedIndex);
                            sbTracing.AppendLine("Inside EarlyPayOff class, and EarlyPayOffCalculation() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, outputGridRowNumber, inputGridRow, false, false, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);");
                            maintenanceFeePayable = ManagementAndMaintenanceFeeCalc.CalculateMaintenanceFeeToCharge(scheduleInput, outputSchedule, outputGrid, index + 1, inputGridRowIndex, false, false, true, ref maintenanceFeeTableIndex, ref lastMaintenanceFeeChargedIndex);

                            interestAccrued = Round.RoundOffAmount(interestAccrued);
                            serviceFeeInterest = (scheduleInput.ApplyServiceFeeInterest == 0 || scheduleInput.ApplyServiceFeeInterest == 2) ? 0 : PeriodicInterestAmount.CalcAccruedInterestWithTier(scheduleInput, startDate, endDate, scheduleInput.InputRecords[0].DateIn, outputGrid[index + 1].BeginningServiceFee, true, false, false);
                            serviceFeeInterest = Round.RoundOffAmount(serviceFeeInterest);
                            principalPayment = Round.RoundOffAmount(outputGrid[index + 1].BeginningPrincipal);
                            index++;
                            if (scheduleInput.Residual > 0)
                            {
                                #region  For the Residual principal amount
                                totalResidualPrincipal = Round.RoundOffAmount(outputSchedule.LoanAmountCalc - scheduleInput.Residual);
                                for (int x = 0; x <= index - 1; x++)
                                {
                                    totalResidualPrincipal = Round.RoundOffAmount(totalResidualPrincipal - outputGrid[x].PrincipalPaid);
                                }
                                totalResidualPrincipal = totalResidualPrincipal > 0 ? totalResidualPrincipal : 0;

                                if (totalResidualPrincipal > principalPayment)
                                {
                                    totalResidualPrincipal = Round.RoundOffAmount(totalResidualPrincipal - principalPayment);
                                }
                                else
                                {
                                    if (totalResidualPrincipal > 0)
                                    {
                                        principalPayment = totalResidualPrincipal;
                                        totalResidualPrincipal = 0;
                                    }
                                    else
                                    {
                                        if (totalResidualPrincipal == 0 && (outputGrid[index].BeginningServiceFee > 0 || outputGrid[index - 1].OriginationFeePastDue > 0 || outputGrid[index - 1].SameDayFeePastDue > 0 || outputGrid[index - 1].AccruedServiceFeeInterestCarryOver > 0 ||
                               outputGrid[index - 1].InterestPastDue > 0 || outputGrid[index - 1].ServiceFeeInterestPastDue > 0 || outputGrid[index - 1].InterestCarryOver > 0 || outputGrid[index - 1].MaintenanceFeePastDue > 0 || outputGrid[index - 1].ManagementFeePastDue > 0))
                                        {
                                            totalResidualPrincipal = 0;
                                            principalPayment = 0;
                                        }
                                        else
                                        {
                                            totalResidualPrincipal = 0;
                                            principalPayment = 0;
                                            interestAccrued = 0;
                                            dailyInterestAmount = 0;
                                        }

                                    }
                                }
                                #endregion
                            }

                            if (scheduleInput.IsServiceFeeIncremental && (outputGrid.FindIndex(o => o.PaymentDate > scheduleInput.EarlyPayoffDate &&
                                  (o.Flags == (int)Constants.FlagValues.Payment || o.Flags == (int)Constants.FlagValues.SkipPayment)) != -1))
                            {
                                int serviceFeeIndex = outputGrid.FindIndex(o => o.PaymentDate > scheduleInput.EarlyPayoffDate &&
                                    (o.Flags == (int)Constants.FlagValues.Payment || o.Flags == (int)Constants.FlagValues.SkipPayment));
                                outputGrid[index].BeginningServiceFee = outputGrid[serviceFeeIndex].ServiceFee;
                            }

                            #region Add schedule for Early Payoff date
                            outputGrid.Insert(index, new PaymentDetail
                            {
                                PaymentDate = endDate,
                                BeginningPrincipal = outputGrid[index].BeginningPrincipal,
                                BeginningServiceFee = outputGrid[index].BeginningServiceFee,
                                PeriodicInterestRate = periodicInterestRate,
                                DailyInterestRate = dailyInterestRate,
                                DailyInterestAmount = dailyInterestAmount,
                                PaymentID = Convert.ToInt32(outputGrid[index].PaymentID.ToString()),
                                Flags = (int)Constants.FlagValues.EarlyPayOff,
                                DueDate = endDate,
                                InterestAccrued = interestAccrued,
                                InterestCarryOver = 0,
                                InterestPayment = interestAccrued + Round.RoundOffAmount(outputGrid[index - 1].InterestCarryOver) + Round.RoundOffAmount(outputGrid[index - 1].InterestPastDue),
                                PrincipalPayment = Round.RoundOffAmount(principalPayment),
                                PaymentDue = Round.RoundOffAmount((principalPayment
                                             + interestAccrued + outputGrid[index - 1].InterestCarryOver + outputGrid[index - 1].InterestPastDue
                                             + outputGrid[index].BeginningServiceFee
                                             + serviceFeeInterest + outputGrid[index - 1].AccruedServiceFeeInterestCarryOver + outputGrid[index - 1].ServiceFeeInterestPastDue
                                             + outputGrid[index - 1].OriginationFeePastDue + outputGrid[index - 1].SameDayFeePastDue + outputGrid[index - 1].NSFFeePastDue + outputGrid[index - 1].LateFeePastDue
                                             + maintenanceFeePayable + outputGrid[index - 1].MaintenanceFeePastDue + managementFeePayable + outputGrid[index - 1].ManagementFeePastDue)),

                                TotalPayment = Round.RoundOffAmount((principalPayment
                                             + interestAccrued + outputGrid[index - 1].InterestCarryOver + outputGrid[index - 1].InterestPastDue
                                             + outputGrid[index].BeginningServiceFee
                                             + serviceFeeInterest + outputGrid[index - 1].AccruedServiceFeeInterestCarryOver + outputGrid[index - 1].ServiceFeeInterestPastDue
                                             + outputGrid[index - 1].OriginationFeePastDue + outputGrid[index - 1].SameDayFeePastDue + outputGrid[index - 1].NSFFeePastDue + outputGrid[index - 1].LateFeePastDue
                                             + maintenanceFeePayable + outputGrid[index - 1].MaintenanceFeePastDue + managementFeePayable + outputGrid[index - 1].ManagementFeePastDue)),
                                AccruedServiceFeeInterest = serviceFeeInterest,
                                AccruedServiceFeeInterestCarryOver = 0,
                                ServiceFee = outputGrid[index].BeginningServiceFee,
                                ServiceFeeInterest = serviceFeeInterest + outputGrid[index - 1].AccruedServiceFeeInterestCarryOver + outputGrid[index - 1].ServiceFeeInterestPastDue,
                                ServiceFeeTotal = Round.RoundOffAmount((outputGrid[index].BeginningServiceFee + serviceFeeInterest + outputGrid[index - 1].AccruedServiceFeeInterestCarryOver + outputGrid[index - 1].ServiceFeeInterestPastDue)),
                                OriginationFee = Round.RoundOffAmount(outputGrid[index - 1].OriginationFeePastDue),
                                MaintenanceFee = Round.RoundOffAmount((maintenanceFeePayable + outputGrid[index - 1].MaintenanceFeePastDue)),
                                ManagementFee = Round.RoundOffAmount((managementFeePayable + outputGrid[index - 1].ManagementFeePastDue)),
                                SameDayFee = Round.RoundOffAmount(outputGrid[index - 1].SameDayFeePastDue),
                                NSFFee = Round.RoundOffAmount(outputGrid[index - 1].NSFFeePastDue),
                                LateFee = Round.RoundOffAmount(outputGrid[index - 1].LateFeePastDue),
                                //Paid amount columns
                                PrincipalPaid = Round.RoundOffAmount(principalPayment),
                                InterestPaid = interestAccrued + Round.RoundOffAmount(outputGrid[index - 1].InterestCarryOver) + Round.RoundOffAmount(outputGrid[index - 1].InterestPastDue),
                                ServiceFeePaid = outputGrid[index].BeginningServiceFee,
                                ServiceFeeInterestPaid = serviceFeeInterest + outputGrid[index - 1].AccruedServiceFeeInterestCarryOver + outputGrid[index - 1].ServiceFeeInterestPastDue,
                                OriginationFeePaid = Round.RoundOffAmount(outputGrid[index - 1].OriginationFeePastDue),
                                MaintenanceFeePaid = Round.RoundOffAmount((maintenanceFeePayable + outputGrid[index - 1].MaintenanceFeePastDue)),
                                ManagementFeePaid = Round.RoundOffAmount((managementFeePayable + outputGrid[index - 1].ManagementFeePastDue)),
                                SameDayFeePaid = Round.RoundOffAmount(outputGrid[index - 1].SameDayFeePastDue),
                                NSFFeePaid = Round.RoundOffAmount(outputGrid[index - 1].NSFFeePastDue),
                                LateFeePaid = Round.RoundOffAmount(outputGrid[index - 1].LateFeePastDue),
                                TotalPaid = Round.RoundOffAmount((principalPayment
                                             + interestAccrued + outputGrid[index - 1].InterestCarryOver + outputGrid[index - 1].InterestPastDue
                                             + outputGrid[index].BeginningServiceFee
                                             + serviceFeeInterest + outputGrid[index - 1].AccruedServiceFeeInterestCarryOver + outputGrid[index - 1].ServiceFeeInterestPastDue
                                             + outputGrid[index - 1].OriginationFeePastDue + outputGrid[index - 1].SameDayFeePastDue + outputGrid[index - 1].NSFFeePastDue + outputGrid[index - 1].LateFeePastDue
                                             + maintenanceFeePayable + outputGrid[index - 1].MaintenanceFeePastDue + managementFeePayable + outputGrid[index - 1].ManagementFeePastDue)),
                                //Cumulative Amount paid column
                                CumulativeInterest = 0,
                                CumulativePrincipal = 0,
                                CumulativePayment = 0,
                                CumulativeServiceFee = 0,
                                CumulativeServiceFeeInterest = 0,
                                CumulativeServiceFeeTotal = 0,
                                CumulativeOriginationFee = 0,
                                CumulativeMaintenanceFee = 0,
                                CumulativeManagementFee = 0,
                                CumulativeSameDayFee = 0,
                                CumulativeNSFFee = 0,
                                CumulativeLateFee = 0,
                                CumulativeTotalFees = 0,
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
                                CumulativeTotalPastDue = 0,
                                BucketStatus = ""
                            });

                            #endregion

                            //Remove row after the early payoff date
                            for (int i = outputGrid.Count - 1; i > index; i--)
                            {
                                outputGrid.RemoveAt(i);
                            }
                            #endregion
                        }
                    }

                    #region Count index where we can add the new schedule for additional payment.
                    if (scheduleInput.EarlyPayoffDate != Convert.ToDateTime(Constants.DefaultDate))
                    {
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
                }
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
