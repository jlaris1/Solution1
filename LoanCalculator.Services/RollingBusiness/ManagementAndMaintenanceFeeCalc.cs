using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoanAmort_driver.Models;

namespace LoanAmort_driver.RollingBusiness
{
    public class ManagementAndMaintenanceFeeCalc
    {
        /// <summary>
        /// This function will evaluate the effective date of assessing the management and maintenance fee.
        /// </summary>
        /// <param name="frequency"></param>
        /// <param name="delay"></param>
        /// <param name="effectiveDate"></param>
        /// <param name="scheduledPaymentDates"></param>
        /// <param name="allPaymentDates"></param>
        /// <returns></returns>
        public static DateTime EffectiveDateOfFee(int frequency, int delay, DateTime effectiveDate, List<DateTime> scheduledPaymentDates, List<AdditionalPaymentRecord> allPaymentDates)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside Business Class ManagementAndMaintenanceFeeCalc and method name EffectiveDateOfFee(int frequency, int delay, DateTime effectiveDate, List<DateTime> scheduledPaymentDates, List<AdditionalPaymentRecord> allPaymentDates).");
                sbTracing.AppendLine("Parameter values are : frequency = " + frequency + ", delay = " + delay + ", effectiveDate = " + effectiveDate);

                switch (frequency)
                {
                    //Checks whether frequency is set to "Days".
                    case (int)Constants.FeeFrequency.Days:
                        effectiveDate = effectiveDate.AddDays(delay);
                        break;

                    //Checks whether frequency is set to "Months".
                    case (int)Constants.FeeFrequency.Months:
                        effectiveDate = effectiveDate.AddMonths(delay);
                        break;

                    //Checks whether frequency is set to "Scheduled Payments".
                    case (int)Constants.FeeFrequency.ScheduledPayments:
                        effectiveDate = (delay >= scheduledPaymentDates.Count) ? scheduledPaymentDates[scheduledPaymentDates.Count - 1].AddDays(1) : scheduledPaymentDates[delay];
                        break;

                    //Checks whether frequency is set to "All Payments".
                    case (int)Constants.FeeFrequency.AllPayments:
                        effectiveDate = (delay >= allPaymentDates.Count) ? allPaymentDates[allPaymentDates.Count - 1].DateIn.AddDays(1) : allPaymentDates[delay].DateIn;
                        break;
                }

                sbTracing.AppendLine("Exit:From Business Class ManagementAndMaintenanceFeeCalc and method name EffectiveDateOfFee(int frequency, int delay, DateTime effectiveDate, List<DateTime> scheduledPaymentDates, List<AdditionalPaymentRecord> allPaymentDates)");
                return effectiveDate;
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
                return effectiveDate;
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }

        /// <summary>
        /// This function evaluates the management and maintenance fee, that is assessed for a particular date.
        /// </summary>
        /// <param name="fixedFee"></param>
        /// <param name="inputFeePercent"></param>
        /// <param name="isGreaterValue"></param>
        /// <param name="feeMaxPer"></param>
        /// <param name="feeMin"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static double CalculateFee(double fixedFee, double inputFeePercent, bool isGreaterValue, double feeMaxPer, double feeMin, double amount)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside Business Class ManagementAndMaintenanceFeeCalc and method name CalculateFee(double fixedFee, double inputFeePercent, bool isGreaterValue, double feeMaxPer, double feeMin, double amount).");
                sbTracing.AppendLine("Parameter values are : fixedFee = " + fixedFee + ", inputFeePercent = " + inputFeePercent + ", isGreaterValue = " + isGreaterValue + ", feeMaxPer = " + feeMaxPer + ", feeMin = " + feeMin + ", amount = " + amount);

                double feePercent = amount * inputFeePercent * .01;
                double greaterOrLesserOutCome;
                double feeToPay = 0;

                //Check whether the calculation method for fee is selected as "Greater" or not i.e. checked or not.
                if ((fixedFee > 0 && inputFeePercent == 0) || (fixedFee == 0 && inputFeePercent > 0))
                {
                    greaterOrLesserOutCome = (fixedFee > 0 && inputFeePercent == 0) ? fixedFee : feePercent;
                }
                else if (isGreaterValue)
                {
                    //Determine the greater value between the fixed fee amount and perentage fee amount.
                    greaterOrLesserOutCome = (fixedFee > feePercent) ? fixedFee : feePercent;
                }
                else
                {
                    //Determine the lower value between the fixed fee amount and perentage fee amount.
                    greaterOrLesserOutCome = (fixedFee < feePercent) ? fixedFee : feePercent;
                }

                //Check whether both the limits of fee is either not set or both are 0.
                if (feeMaxPer == 0 && feeMin == 0)
                {
                    //Select the out come from calculation method as final fee, as there is no limit.
                    feeToPay = greaterOrLesserOutCome;
                }
                //Check whether the upper limit is either not set or set to 0.
                else if (feeMaxPer == 0)
                {
                    feeToPay = (feeMin > greaterOrLesserOutCome) ? feeMin : greaterOrLesserOutCome;
                }
                //Check whether the lower limit is either not set or set to 0.
                else if (feeMin == 0)
                {
                    feeToPay = (feeMaxPer < greaterOrLesserOutCome) ? feeMaxPer : greaterOrLesserOutCome;
                }
                else
                {
                    //Check whether the out come from calculation method is less then the lower limit.
                    if (feeMin >= greaterOrLesserOutCome)
                    {
                        //Select lower limit as a final fee.
                        feeToPay = feeMin;
                    }
                    //Check whether the out come from calculation method is between the lower and upper limit.
                    else if (feeMin < greaterOrLesserOutCome && feeMaxPer > greaterOrLesserOutCome)
                    {
                        //Select the out come from calculation method as a final fee.
                        feeToPay = greaterOrLesserOutCome;
                    }
                    //Check whether the out come from calculation method is greater then the upper limit.
                    else if (feeMaxPer <= greaterOrLesserOutCome)
                    {
                        //Select upper limit as a final  fee.
                        feeToPay = feeMaxPer;
                    }
                }
                feeToPay = Math.Round(feeToPay, 2, MidpointRounding.AwayFromZero);
                sbTracing.AppendLine("Exit:From Business Class ManagementAndMaintenanceFeeCalc and method name CalculateFee(double fixedFee, double inputFeePercent, bool isGreaterValue, double feeMaxPer, double feeMin, double amount)");
                return feeToPay;
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

        /// <summary>
        /// This function determines the management and maintenance fee to charge in the particular schedule, as per max allowed fee monthly and per loan.
        /// </summary>
        /// <param name="scheduleInput"></param>
        /// <param name="maxFeePerMonth"></param>
        /// <param name="maxFeePerLoan"></param>
        /// <param name="date"></param>
        /// <param name="outputGrid"></param>
        /// <param name="payableFee"></param>
        /// <param name="isManagementFeeCalculation"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static double PayableAmountAsPerMonthAndLoan(getScheduleInput scheduleInput, double maxFeePerMonth, double maxFeePerLoan, DateTime date, List<PaymentDetail> outputGrid, double payableFee, bool isManagementFeeCalculation, int index)
        {
            StringBuilder sbTracing = new StringBuilder();
            sbTracing.AppendLine("Enter:Inside Business Class ManagementAndMaintenanceFeeCalc and method name PayableAmountAsPerMonthAndLoan(LoanDetails scheduleInput, double maxFeePerMonth, double maxFeePerLoan, DateTime date, List<OutputGrid> outputGrid, double payableFee, bool isManagementFeeCalculation, int index).");
            sbTracing.AppendLine("Parameter values are : maxFeePerMonth = " + maxFeePerMonth + ", maxFeePerLoan = " + maxFeePerLoan + ", date = " + date + ", payableFee = " + payableFee + ", isManagementFeeCalculation = " + isManagementFeeCalculation + ", index = " + index);

            try
            {
                List<PaymentDetail> copyOutputGrid = new List<PaymentDetail>();
                for (int i = 0; i < index; i++)
                {
                    copyOutputGrid.Add(outputGrid[i]);
                }

                //Calculate fee for per month
                if (maxFeePerMonth != 0)
                {
                    double paidFeePerMonth = 0;
                    for (int i = 0; i < copyOutputGrid.Count; i++)
                    {
                        if (copyOutputGrid[i].PaymentDate.Month == date.Month && copyOutputGrid[i].PaymentDate.Year == date.Year)
                        {
                            if (isManagementFeeCalculation)
                            {
                                paidFeePerMonth += copyOutputGrid[i].ManagementFee - (i == 0 ? 0 : copyOutputGrid[i - 1].ManagementFeePastDue) - (i == 0 ? scheduleInput.EarnedManagementFee : 0);
                            }
                            else
                            {
                                paidFeePerMonth += copyOutputGrid[i].MaintenanceFee - (i == 0 ? 0 : copyOutputGrid[i - 1].MaintenanceFeePastDue) - (i == 0 ? scheduleInput.EarnedMaintenanceFee : 0);
                            }
                        }
                    }

                    if (paidFeePerMonth >= maxFeePerMonth)
                    {
                        payableFee = 0;
                    }
                    else if ((paidFeePerMonth + payableFee) > maxFeePerMonth)
                    {
                        payableFee = maxFeePerMonth - paidFeePerMonth;
                    }
                }

                //Calculate fee for per loan
                if (maxFeePerLoan != 0)
                {
                    double paidFeePerLoan = 0;
                    for (int i = 0; i < copyOutputGrid.Count; i++)
                    {
                        if (isManagementFeeCalculation)
                        {
                            paidFeePerLoan += copyOutputGrid[i].ManagementFee - (i == 0 ? 0 : copyOutputGrid[i - 1].ManagementFeePastDue) - (i == 0 ? scheduleInput.EarnedManagementFee : 0);
                        }
                        else
                        {
                            paidFeePerLoan += copyOutputGrid[i].MaintenanceFee - (i == 0 ? 0 : copyOutputGrid[i - 1].MaintenanceFeePastDue) - (i == 0 ? scheduleInput.EarnedMaintenanceFee : 0);
                        }
                    }

                    if (paidFeePerLoan >= maxFeePerLoan)
                    {
                        payableFee = 0;
                    }
                    else if ((paidFeePerLoan + payableFee) > maxFeePerLoan)
                    {
                        payableFee = maxFeePerLoan - paidFeePerLoan;
                    }
                }

                sbTracing.AppendLine("Exit:From Business Class ManagementAndMaintenanceFeeCalc and method name PayableAmountAsPerMonthAndLoan(double maxFeePerMonth, double maxFeePerLoan, DateTime date, List<OutputGrid> outputGrid, double payableFee, bool isManagementFeeCalculation)");
                return payableFee;
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

        /// <summary>
        /// This function creates the table for the management fee assessment, with default assessed fee value i.e. 0
        /// </summary>
        /// <param name="effectiveDate"></param>
        /// <param name="frequency"></param>
        /// <param name="frequencyNumber"></param>
        /// <param name="scheduledPaymentDates"></param>
        /// <param name="allPaymentDates"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static List<ManagementFeeTable> CreateManagementFeeTable(DateTime effectiveDate, int frequency, int frequencyNumber, List<DateTime> scheduledPaymentDates, List<AdditionalPaymentRecord> allPaymentDates, int delay)
        {
            List<ManagementFeeTable> managementFeeTable = new List<ManagementFeeTable>();
            DateTime lastFeeAssessmentDate = (frequency == (int)Constants.FeeFrequency.Days || frequency == (int)Constants.FeeFrequency.Months || frequency == (int)Constants.FeeFrequency.AllPayments) ?
                                                                    allPaymentDates[allPaymentDates.Count - 1].DateIn : scheduledPaymentDates[scheduledPaymentDates.Count - 1];

            StringBuilder sbTracing = new StringBuilder();
            sbTracing.AppendLine("Enter:Inside Business Class ManagementAndMaintenanceFeeCalc and method name CreateManagementFeeTable(DateTime effectiveDate, int frequency, int frequencyNumber, List<DateTime> scheduledPaymentDates, List<AdditionalPaymentRecord> allPaymentDates, int delay).");
            sbTracing.AppendLine("Parameter values are : effectiveDate = " + effectiveDate + ", frequency = " + frequency + ", frequencyNumber = " + frequencyNumber + ", delay = " + delay);

            try
            {
                switch (frequency)
                {
                    //Checks whether management fee frequency is set to "Days".
                    case (int)Constants.FeeFrequency.Days:
                        effectiveDate = effectiveDate.AddDays(frequencyNumber);
                        while (effectiveDate <= lastFeeAssessmentDate)
                        {
                            managementFeeTable.Add(new ManagementFeeTable { AssessmentDate = effectiveDate, Fee = 0 });
                            effectiveDate = effectiveDate.AddDays(frequencyNumber);
                        }
                        break;

                    //Checks whether management fee frequency is set to "Months".
                    case (int)Constants.FeeFrequency.Months:
                        effectiveDate = effectiveDate.AddMonths(frequencyNumber);
                        while (effectiveDate <= lastFeeAssessmentDate)
                        {
                            managementFeeTable.Add(new ManagementFeeTable { AssessmentDate = effectiveDate, Fee = 0 });
                            effectiveDate = effectiveDate.AddMonths(frequencyNumber);
                        }
                        break;

                    //Checks whether management fee frequency is set to "Scheduled Payments".
                    case (int)Constants.FeeFrequency.ScheduledPayments:
                        int index = scheduledPaymentDates.IndexOf(effectiveDate);
                        for (int i = (index + frequencyNumber); i < scheduledPaymentDates.Count && index != -1; i += frequencyNumber)
                        {
                            effectiveDate = scheduledPaymentDates[i];
                            managementFeeTable.Add(new ManagementFeeTable { AssessmentDate = effectiveDate, Fee = 0 });
                        }
                        break;

                    //Checks whether management fee frequency is set to "All Payments".
                    case (int)Constants.FeeFrequency.AllPayments:
                        for (int i = (delay + frequencyNumber); i < allPaymentDates.Count && delay < allPaymentDates.Count; i += frequencyNumber)
                        {
                            effectiveDate = allPaymentDates[i].DateIn;
                            managementFeeTable.Add(new ManagementFeeTable { AssessmentDate = effectiveDate, Fee = 0, PaymentId = Convert.ToString(allPaymentDates[i].PaymentID) });
                        }
                        break;
                }

                sbTracing.AppendLine("Exit:From Business Class ManagementAndMaintenanceFeeCalc and method name CreateManagementFeeTable(DateTime effectiveDate, int frequency, int frequencyNumber, List<DateTime> scheduledPaymentDates, List<AdditionalPaymentRecord> allPaymentDates, int delay)");
                return managementFeeTable;
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
                return managementFeeTable;
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }

        /// <summary>
        /// This function creates the table for maintenance fee assessment, with default assessed fee value i.e. 0
        /// </summary>
        /// <param name="effectiveDate"></param>
        /// <param name="frequency"></param>
        /// <param name="frequencyNumber"></param>
        /// <param name="scheduledPaymentDates"></param>
        /// <param name="allPaymentDates"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static List<MaintenanceFeeTable> CreateMaintenanceFeeTable(DateTime effectiveDate, int frequency, int frequencyNumber, List<DateTime> scheduledPaymentDates, List<AdditionalPaymentRecord> allPaymentDates, int delay)
        {
            List<MaintenanceFeeTable> maintenanceFeeTable = new List<MaintenanceFeeTable>();
            DateTime lastFeeAssessmentDate = (frequency == (int)Constants.FeeFrequency.Days || frequency == (int)Constants.FeeFrequency.Months || frequency == (int)Constants.FeeFrequency.AllPayments) ?
                                                                    allPaymentDates[allPaymentDates.Count - 1].DateIn : scheduledPaymentDates[scheduledPaymentDates.Count - 1];

            StringBuilder sbTracing = new StringBuilder();
            sbTracing.AppendLine("Enter:Inside Business Class ManagementAndMaintenanceFeeCalc and method name CreateMaintenanceFeeTable(DateTime effectiveDate, int frequency, int frequencyNumber, List<DateTime> scheduledPaymentDates, List<AdditionalPaymentRecord> allPaymentDates, int delay).");
            sbTracing.AppendLine("Parameter values are : effectiveDate = " + effectiveDate + ", frequency = " + frequency + ", frequencyNumber = " + frequencyNumber + ", delay = " + delay);

            try
            {
                switch (frequency)
                {
                    //Checks whether maintenance fee frequency is set to "Days".
                    case (int)Constants.FeeFrequency.Days:
                        effectiveDate = effectiveDate.AddDays(frequencyNumber);
                        while (effectiveDate <= lastFeeAssessmentDate)
                        {
                            maintenanceFeeTable.Add(new MaintenanceFeeTable { AssessmentDate = effectiveDate, Fee = 0 });
                            effectiveDate = effectiveDate.AddDays(frequencyNumber);
                        }
                        break;

                    //Checks whether maintenance fee frequency is set to "Months".
                    case (int)Constants.FeeFrequency.Months:
                        effectiveDate = effectiveDate.AddMonths(frequencyNumber);
                        while (effectiveDate <= lastFeeAssessmentDate)
                        {
                            maintenanceFeeTable.Add(new MaintenanceFeeTable { AssessmentDate = effectiveDate, Fee = 0 });
                            effectiveDate = effectiveDate.AddMonths(frequencyNumber);
                        }
                        break;

                    //Checks whether maintenance fee frequency is set to "Scheduled Payments".
                    case (int)Constants.FeeFrequency.ScheduledPayments:
                        int index = scheduledPaymentDates.IndexOf(effectiveDate);
                        for (int i = (index + frequencyNumber); i < scheduledPaymentDates.Count && index != -1; i += frequencyNumber)
                        {
                            effectiveDate = scheduledPaymentDates[i];
                            maintenanceFeeTable.Add(new MaintenanceFeeTable { AssessmentDate = effectiveDate, Fee = 0 });
                        }
                        break;

                    //Checks whether maintenance fee frequency is set to "All Payments".
                    case (int)Constants.FeeFrequency.AllPayments:
                        for (int i = (delay + frequencyNumber); i < allPaymentDates.Count && delay < allPaymentDates.Count; i += frequencyNumber)
                        {
                            effectiveDate = allPaymentDates[i].DateIn;
                            maintenanceFeeTable.Add(new MaintenanceFeeTable { AssessmentDate = effectiveDate, Fee = 0, PaymentId = Convert.ToString(allPaymentDates[i].PaymentID) });
                        }
                        break;
                }

                sbTracing.AppendLine("Exit:From Business Class ManagementAndMaintenanceFeeCalc and method name CreateMaintenanceFeeTable(DateTime effectiveDate, int frequency, int frequencyNumber, List<DateTime> scheduledPaymentDates, List<AdditionalPaymentRecord> allPaymentDates, int delay)");
                return maintenanceFeeTable;
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
                return maintenanceFeeTable;
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }

        #region Calculate Management and Maintenance Fee to charge

        /// <summary>
        /// This function evaluates the management fee assessed, and return the payable amount.
        /// </summary>
        /// <param name="scheduleInput"></param>
        /// <param name="outputSchedule"></param>
        /// <param name="outputGrid"></param>
        /// <param name="outputGridRow"></param>
        /// <param name="inputRowNumber"></param>
        /// <param name="isForAdditionalPayment"></param>
        /// <param name="isExtendedRow"></param>
        /// <param name="managementFeeTableIndex"></param>
        /// <param name="lastManagementFeeChargedIndex"></param>
        /// <returns></returns>
        public static double CalculateManagementFeeToCharge(getScheduleInput scheduleInput, getScheduleOutput outputSchedule, List<PaymentDetail> outputGrid, int outputGridRow,
                                       int inputRowNumber, bool isForAdditionalPayment, bool isExtendedRow, bool isEarlyPayOff, ref int managementFeeTableIndex, ref int lastManagementFeeChargedIndex)
        {
            int previousChargedIndex = lastManagementFeeChargedIndex;
            int managementFeeIndex = managementFeeTableIndex;
            double managementFeePayable = 0;
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside Business Class ActualPaymentAllocation and method name CalculateManagementFeeToCharge(LoanDetails scheduleInput, OutputSchedule outputSchedule, List<OutputGrid> outputGrid, " +
                                        "int outputGridRow, int inputRowNumber, bool isForAdditionalPayment, bool isExtendedRow, ref int managementFeeTableIndex, ref int lastManagementFeeChargedIndex).");
                sbTracing.AppendLine("Parameter values are : outputGridRow = " + outputGridRow + ", inputRowNumber = " + inputRowNumber + ", isForAdditionalPayment = " + isForAdditionalPayment + ", isExtendedRow = " + isExtendedRow +
                    ", managementFeeTableIndex = " + managementFeeTableIndex + ", lastManagementFeeChargedIndex = " + lastManagementFeeChargedIndex);

                //Checks whether this function is called for calculating the fee while schedule for additional payment is generated.
                if (isForAdditionalPayment)
                {
                    #region Management Fee calculations in Additional Payment row
                    if (scheduleInput.ManagementFeeBasis != (int)Models.Constants.FeeBasis.ActualPayment && scheduleInput.ManagementFeeBasis != (int)Models.Constants.FeeBasis.ScheduledPayment)
                    {
                        #region
                        if (scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.PrincipalOnly || scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.AdditionalPayment ||
                            scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.NSFFee || scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.LateFee ||
                            scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.MaintenanceFee || scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.ManagementFee ||
                                (((scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags != (int)Constants.FlagValues.Discount) && (scheduleInput.ManagementFeeFrequency == (int)Constants.FeeFrequency.AllPayments ||
                                scheduleInput.ManagementFeeFrequency == (int)Constants.FeeFrequency.ScheduledPayments)) ||
                                ((scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.Discount) && (scheduleInput.ManagementFeeFrequency == (int)Constants.FeeFrequency.Days ||
                                scheduleInput.ManagementFeeFrequency == (int)Constants.FeeFrequency.Months))))
                        {
                            int flagValueIndex = scheduleInput.AdditionalPaymentRecords.FindIndex(o => o.Flags == (int)Models.Constants.FlagValues.Discount &&
                                                    managementFeeIndex < outputSchedule.ManagementFeeAssessment.Count &&
                                                    o.DateIn == outputSchedule.ManagementFeeAssessment[managementFeeIndex].AssessmentDate);
                            while (managementFeeIndex < outputSchedule.ManagementFeeAssessment.Count &&
                                ((outputSchedule.ManagementFeeAssessment[managementFeeIndex].AssessmentDate == scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn &&
                                (flagValueIndex != -1 || outputSchedule.ManagementFeeAssessment[managementFeeIndex].PaymentId == Convert.ToString(scheduleInput.AdditionalPaymentRecords[inputRowNumber].PaymentID) ||
                                scheduleInput.ManagementFeeFrequency == (int)Constants.FeeFrequency.Days || scheduleInput.ManagementFeeFrequency == (int)Constants.FeeFrequency.Months)) ||
                                outputSchedule.ManagementFeeAssessment[managementFeeIndex].AssessmentDate < scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn))
                            {
                                double amount = isExtendedRow ? outputGrid[outputGridRow - 1].BeginningPrincipal - outputGrid[outputGridRow - 1].PrincipalPaid : outputGrid[outputGridRow].BeginningPrincipal;

                                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and CalculateManagementFeeToCharge() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateFee(scheduleInput.ManagementFee, " +
                                                        "scheduleInput.ManagementFeePercent, scheduleInput.IsManagementFeeGreater, scheduleInput.ManagementFeeMaxPer, scheduleInput.ManagementFeeMin, amount); ");
                                outputSchedule.ManagementFeeAssessment[managementFeeIndex].Fee = ManagementAndMaintenanceFeeCalc.CalculateFee(scheduleInput.ManagementFee,
                                                        scheduleInput.ManagementFeePercent, scheduleInput.IsManagementFeeGreater, scheduleInput.ManagementFeeMaxPer, scheduleInput.ManagementFeeMin, amount);
                                managementFeeIndex++;

                                if (managementFeeIndex != outputSchedule.ManagementFeeAssessment.Count)
                                {
                                    flagValueIndex = scheduleInput.AdditionalPaymentRecords.FindIndex(o => o.Flags == (int)Models.Constants.FlagValues.Discount &&
                                                        o.DateIn == outputSchedule.ManagementFeeAssessment[managementFeeIndex].AssessmentDate);
                                }
                            }
                        }
                        #endregion
                    }
                    if (scheduleInput.ManagementFeeFrequency == (int)Constants.FeeFrequency.AllPayments)
                    {
                        #region When fee frequency is set to AllPayment

                        managementFeePayable = outputSchedule.ManagementFeeAssessment.Where(o => o.AssessmentDate == scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn &&
                                                            o.PaymentId == Convert.ToString(scheduleInput.AdditionalPaymentRecords[inputRowNumber].PaymentID)).Sum(o => o.Fee);

                        lastManagementFeeChargedIndex = outputSchedule.ManagementFeeAssessment.FindIndex(o => o.AssessmentDate == scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn &&
                                                            o.PaymentId == Convert.ToString(scheduleInput.AdditionalPaymentRecords[inputRowNumber].PaymentID));
                        lastManagementFeeChargedIndex = (lastManagementFeeChargedIndex == -1 ? previousChargedIndex : lastManagementFeeChargedIndex);

                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and CalculateManagementFeeToCharge() method. Calling method : ManagementAndMaintenanceFeeCalc.PayableAmountAsPerMonthAndLoan(scheduleInput.ManagementFeeMaxMonth, scheduleInput.ManagementFeeMaxLoan, " +
                                                            "scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn, outputGrid, managementFeePayable, true); ");
                        //Determine the payable amount
                        managementFeePayable = ManagementAndMaintenanceFeeCalc.PayableAmountAsPerMonthAndLoan(scheduleInput, scheduleInput.ManagementFeeMaxMonth, scheduleInput.ManagementFeeMaxLoan,
                                                            scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn, outputGrid, managementFeePayable, true, outputGridRow);
                        #endregion
                    }
                    #endregion
                }
                else
                {
                    #region Management Fee calculations in input grid row
                    DateTime inputGridDate = DateTime.MinValue;
                    if (isEarlyPayOff)
                    {
                        inputGridDate = scheduleInput.EarlyPayoffDate;
                    }
                    else
                    {
                        inputGridDate = scheduleInput.InputRecords[inputRowNumber].EffectiveDate == DateTime.MinValue ? scheduleInput.InputRecords[inputRowNumber].DateIn : scheduleInput.InputRecords[inputRowNumber].EffectiveDate;
                    }
                    int mgmtIndex = outputSchedule.ManagementFeeAssessment.FindIndex(o => o.AssessmentDate == inputGridDate);

                    if (scheduleInput.ManagementFeeBasis != (int)Models.Constants.FeeBasis.ActualPayment && scheduleInput.ManagementFeeBasis != (int)Models.Constants.FeeBasis.ScheduledPayment)
                    {
                        while (managementFeeIndex < outputSchedule.ManagementFeeAssessment.Count && managementFeeIndex <= mgmtIndex &&
                                    (outputSchedule.ManagementFeeAssessment[managementFeeIndex].AssessmentDate <= inputGridDate))
                        {
                            double amount = outputGrid[outputGridRow].BeginningPrincipal;

                            sbTracing.AppendLine("Inside ActualPaymentAllocation class, and CalculateManagementFeeToCharge() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateFee(scheduleInput.ManagementFee, " +
                                                    "scheduleInput.ManagementFeePercent, scheduleInput.IsManagementFeeGreater, scheduleInput.ManagementFeeMaxPer, scheduleInput.ManagementFeeMin, amount); ");
                            outputSchedule.ManagementFeeAssessment[managementFeeIndex].Fee = ManagementAndMaintenanceFeeCalc.CalculateFee(scheduleInput.ManagementFee,
                                                    scheduleInput.ManagementFeePercent, scheduleInput.IsManagementFeeGreater, scheduleInput.ManagementFeeMaxPer, scheduleInput.ManagementFeeMin, amount);
                            managementFeeIndex++;
                        }
                    }

                    for (int i = previousChargedIndex + 1; i < outputSchedule.ManagementFeeAssessment.Count && i <= mgmtIndex && outputSchedule.ManagementFeeAssessment[i].AssessmentDate <= inputGridDate; i++)
                    {
                        managementFeePayable += outputSchedule.ManagementFeeAssessment[i].Fee;
                        lastManagementFeeChargedIndex = i;
                    }

                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and CalculateManagementFeeToCharge() method. Calling method : ManagementAndMaintenanceFeeCalc.PayableAmountAsPerMonthAndLoan(scheduleInput.ManagementFeeMaxMonth, scheduleInput.ManagementFeeMaxLoan, " +
                                                                 "inputGridDate, outputGrid, managementFeePayable, true); ");
                    managementFeePayable = ManagementAndMaintenanceFeeCalc.PayableAmountAsPerMonthAndLoan(scheduleInput, scheduleInput.ManagementFeeMaxMonth, scheduleInput.ManagementFeeMaxLoan,
                                                                 inputGridDate, outputGrid, managementFeePayable, true, outputGridRow);
                    #endregion
                }
                managementFeeTableIndex = managementFeeIndex;
                sbTracing.AppendLine("Exit:From Business Class ActualPaymentAllocation and method name CalculateManagementFeeToCharge(LoanDetails scheduleInput, OutputSchedule outputSchedule, List<OutputGrid> outputGrid, " +
                                        "int outputGridRow, int inputRowNumber, bool isForAdditionalPayment, bool isExtendedRow, ref int managementFeeTableIndex, ref int lastManagementFeeChargedIndex)");
                return managementFeePayable;
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

        /// <summary>
        /// This function evaluates the maintenance fee assessed, and return the payable amount.
        /// </summary>
        /// <param name="scheduleInput"></param>
        /// <param name="outputSchedule"></param>
        /// <param name="outputGrid"></param>
        /// <param name="outputGridRow"></param>
        /// <param name="inputRowNumber"></param>
        /// <param name="isForAdditionalPayment"></param>
        /// <param name="isExtendedRow"></param>
        /// <param name="maintenanceFeeTableIndex"></param>
        /// <param name="lastMaintenanceFeeChargedIndex"></param>
        /// <returns></returns>
        public static double CalculateMaintenanceFeeToCharge(getScheduleInput scheduleInput, getScheduleOutput outputSchedule, List<PaymentDetail> outputGrid, int outputGridRow,
                                       int inputRowNumber, bool isForAdditionalPayment, bool isExtendedRow, bool isEarlyPayOff, ref int maintenanceFeeTableIndex, ref int lastMaintenanceFeeChargedIndex)
        {
            int previousChargedIndex = lastMaintenanceFeeChargedIndex;
            int maintenanceFeeIndex = maintenanceFeeTableIndex;
            double maintenanceFeePayable = 0;
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside Business Class ActualPaymentAllocation and method name CalculateMaintenanceFeeToCharge(LoanDetails scheduleInput, OutputSchedule outputSchedule, List<OutputGrid> outputGrid, int outputGridRow, " +
                                        "int inputRowNumber, bool isForAdditionalPayment, bool isExtendedRow, ref int maintenanceFeeTableIndex, ref int lastMaintenanceFeeChargedIndex).");
                sbTracing.AppendLine("Parameter values are : outputGridRow = " + outputGridRow + ", inputRowNumber = " + inputRowNumber + ", isForAdditionalPayment = " + isForAdditionalPayment +
                    ", isExtendedRow = " + isExtendedRow + ", maintenanceFeeTableIndex = " + maintenanceFeeTableIndex + ", lastMaintenanceFeeChargedIndex = " + lastMaintenanceFeeChargedIndex);

                //Checks whether this function is called for calculating the fee while schedule for additional payment is generated.
                if (isForAdditionalPayment)
                {
                    #region Maintenance Fee calculations in Additional Payment row
                    if (scheduleInput.MaintenanceFeeBasis != (int)Models.Constants.FeeBasis.ActualPayment && scheduleInput.MaintenanceFeeBasis != (int)Models.Constants.FeeBasis.ScheduledPayment)
                    {
                        if (scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.PrincipalOnly || scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.AdditionalPayment ||
                            scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.NSFFee || scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.LateFee ||
                            scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.MaintenanceFee || scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.ManagementFee ||
                                (((scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags != (int)Constants.FlagValues.Discount) && (scheduleInput.MaintenanceFeeFrequency == (int)Constants.FeeFrequency.AllPayments ||
                                scheduleInput.MaintenanceFeeFrequency == (int)Constants.FeeFrequency.ScheduledPayments)) ||
                                ((scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.Discount) && (scheduleInput.MaintenanceFeeFrequency == (int)Constants.FeeFrequency.Days ||
                                scheduleInput.MaintenanceFeeFrequency == (int)Constants.FeeFrequency.Months))))
                        {
                            int flagValueIndex = scheduleInput.AdditionalPaymentRecords.FindIndex(o => o.Flags == (int)Models.Constants.FlagValues.Discount &&
                                                    maintenanceFeeIndex < outputSchedule.MaintenanceFeeAssessment.Count &&
                                                     o.DateIn == outputSchedule.MaintenanceFeeAssessment[maintenanceFeeIndex].AssessmentDate);
                            while (maintenanceFeeIndex < outputSchedule.MaintenanceFeeAssessment.Count &&
                                ((outputSchedule.MaintenanceFeeAssessment[maintenanceFeeIndex].AssessmentDate == scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn &&
                                (flagValueIndex != -1 || outputSchedule.MaintenanceFeeAssessment[maintenanceFeeIndex].PaymentId == Convert.ToString(scheduleInput.AdditionalPaymentRecords[inputRowNumber].PaymentID) ||
                                scheduleInput.MaintenanceFeeFrequency == (int)Constants.FeeFrequency.Days || scheduleInput.MaintenanceFeeFrequency == (int)Constants.FeeFrequency.Months)) ||
                                outputSchedule.MaintenanceFeeAssessment[maintenanceFeeIndex].AssessmentDate < scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn))
                            {
                                double amount = isExtendedRow ? outputGrid[outputGridRow - 1].BeginningPrincipal - outputGrid[outputGridRow - 1].PrincipalPaid : outputGrid[outputGridRow].BeginningPrincipal;

                                sbTracing.AppendLine("Inside ActualPaymentAllocation class, and CalculateMaintenanceFeeToCharge() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateFee(scheduleInput.MaintenanceFee, " +
                                                        "scheduleInput.MaintenanceFeePercent, scheduleInput.IsMaintenanceFeeGreater, scheduleInput.MaintenanceFeeMaxPer, scheduleInput.MaintenanceFeeMin, amount); ");
                                outputSchedule.MaintenanceFeeAssessment[maintenanceFeeIndex].Fee = ManagementAndMaintenanceFeeCalc.CalculateFee(scheduleInput.MaintenanceFee,
                                                        scheduleInput.MaintenanceFeePercent, scheduleInput.IsMaintenanceFeeGreater, scheduleInput.MaintenanceFeeMaxPer, scheduleInput.MaintenanceFeeMin, amount);
                                maintenanceFeeIndex++;

                                if (maintenanceFeeIndex != outputSchedule.MaintenanceFeeAssessment.Count)
                                {
                                    flagValueIndex = scheduleInput.AdditionalPaymentRecords.FindIndex(o => o.Flags == (int)Models.Constants.FlagValues.Discount &&
                                                         o.DateIn == outputSchedule.MaintenanceFeeAssessment[maintenanceFeeIndex].AssessmentDate);
                                }
                            }
                        }
                    }

                    if (scheduleInput.MaintenanceFeeFrequency == (int)Constants.FeeFrequency.AllPayments)
                    {
                        #region When fee frequency is set to AllPayment
                        maintenanceFeePayable = outputSchedule.MaintenanceFeeAssessment.Where(o => o.AssessmentDate == scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn &&
                                                    o.PaymentId == Convert.ToString(scheduleInput.AdditionalPaymentRecords[inputRowNumber].PaymentID)).Sum(o => o.Fee);

                        lastMaintenanceFeeChargedIndex = outputSchedule.MaintenanceFeeAssessment.FindIndex(o => o.AssessmentDate == scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn &&
                                            o.PaymentId == Convert.ToString(scheduleInput.AdditionalPaymentRecords[inputRowNumber].PaymentID));
                        lastMaintenanceFeeChargedIndex = (lastMaintenanceFeeChargedIndex == -1 ? previousChargedIndex : lastMaintenanceFeeChargedIndex);

                        sbTracing.AppendLine("Inside ActualPaymentAllocation class, and CalculateMaintenanceFeeToCharge() method. Calling method : ManagementAndMaintenanceFeeCalc.PayableAmountAsPerMonthAndLoan(scheduleInput.MaintenanceFeeMaxMonth, " +
                                                "scheduleInput.MaintenanceFeeMaxLoan, scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn, outputGrid, maintenanceFeePayable, false); ");
                        //Determine the payable amount
                        maintenanceFeePayable = ManagementAndMaintenanceFeeCalc.PayableAmountAsPerMonthAndLoan(scheduleInput, scheduleInput.MaintenanceFeeMaxMonth, scheduleInput.MaintenanceFeeMaxLoan,
                                                            scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn, outputGrid, maintenanceFeePayable, false, outputGridRow);
                        #endregion
                    }
                    #endregion
                }
                else
                {
                    #region Maintenance Fee calculations in input grid row
                    DateTime inputGridDate = DateTime.MinValue;
                    if (isEarlyPayOff)
                    {
                        inputGridDate = scheduleInput.EarlyPayoffDate;
                    }
                    else
                    {
                        inputGridDate = scheduleInput.InputRecords[inputRowNumber].EffectiveDate == DateTime.MinValue ? scheduleInput.InputRecords[inputRowNumber].DateIn : scheduleInput.InputRecords[inputRowNumber].EffectiveDate;
                    }
                    int maintenanceIndex = outputSchedule.MaintenanceFeeAssessment.FindIndex(o => o.AssessmentDate == inputGridDate);

                    if (scheduleInput.MaintenanceFeeBasis != (int)Models.Constants.FeeBasis.ActualPayment && scheduleInput.MaintenanceFeeBasis != (int)Models.Constants.FeeBasis.ScheduledPayment)
                    {
                        while (maintenanceFeeIndex < outputSchedule.MaintenanceFeeAssessment.Count && maintenanceFeeIndex <= maintenanceIndex &&
                                    (outputSchedule.MaintenanceFeeAssessment[maintenanceFeeIndex].AssessmentDate <= inputGridDate))
                        {
                            double amount = outputGrid[outputGridRow].BeginningPrincipal;

                            sbTracing.AppendLine("Inside ActualPaymentAllocation class, and CalculateMaintenanceFeeToCharge() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateFee(scheduleInput.MaintenanceFee, " +
                                                    "scheduleInput.MaintenanceFeePercent, scheduleInput.IsMaintenanceFeeGreater, scheduleInput.MaintenanceFeeMaxPer, scheduleInput.MaintenanceFeeMin, amount); ");
                            outputSchedule.MaintenanceFeeAssessment[maintenanceFeeIndex].Fee = ManagementAndMaintenanceFeeCalc.CalculateFee(scheduleInput.MaintenanceFee,
                                                    scheduleInput.MaintenanceFeePercent, scheduleInput.IsMaintenanceFeeGreater, scheduleInput.MaintenanceFeeMaxPer, scheduleInput.MaintenanceFeeMin, amount);
                            maintenanceFeeIndex++;
                        }
                    }

                    for (int i = previousChargedIndex + 1; i < outputSchedule.MaintenanceFeeAssessment.Count && i <= maintenanceIndex && outputSchedule.MaintenanceFeeAssessment[i].AssessmentDate <= inputGridDate; i++)
                    {
                        maintenanceFeePayable += outputSchedule.MaintenanceFeeAssessment[i].Fee;
                        lastMaintenanceFeeChargedIndex = i;
                    }

                    sbTracing.AppendLine("Inside ActualPaymentAllocation class, and CalculateMaintenanceFeeToCharge() method. Calling method : ManagementAndMaintenanceFeeCalc.PayableAmountAsPerMonthAndLoan(scheduleInput.MaintenanceFeeMaxMonth, " +
                                            "scheduleInput.MaintenanceFeeMaxLoan, inputGridDate, outputGrid, maintenanceFeePayable, false); ");
                    maintenanceFeePayable = ManagementAndMaintenanceFeeCalc.PayableAmountAsPerMonthAndLoan(scheduleInput, scheduleInput.MaintenanceFeeMaxMonth, scheduleInput.MaintenanceFeeMaxLoan,
                                                                 inputGridDate, outputGrid, maintenanceFeePayable, false, outputGridRow);
                    #endregion
                }
                maintenanceFeeTableIndex = maintenanceFeeIndex;
                sbTracing.AppendLine("Exit:From Business Class ActualPaymentAllocation and method name CalculateMaintenanceFeeToCharge(LoanDetails scheduleInput, OutputSchedule outputSchedule, List<OutputGrid> outputGrid, int outputGridRow, " +
                                        "int inputRowNumber, bool isForAdditionalPayment, bool isExtendedRow, ref int maintenanceFeeTableIndex, ref int lastMaintenanceFeeChargedIndex)");
                return maintenanceFeePayable;
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

        #endregion
    }
}
