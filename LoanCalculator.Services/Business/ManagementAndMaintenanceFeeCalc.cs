using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoanAmort_driver.Models;

namespace LoanAmort_driver.Business
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
        public static DateTime EffectiveDateOfFee(int frequency, int delay, DateTime effectiveDate, List<DateTime> scheduledPaymentDates, List<Models.AdditionalPaymentRecord> allPaymentDates)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside Class ManagementAndMaintenanceFeeCalc and method name EffectiveDateOfFee(int frequency, int delay, DateTime effectiveDate, List<DateTime> scheduledPaymentDates, List<DateTime> allPaymentDates).");
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
                        effectiveDate = delay >= scheduledPaymentDates.Count ? scheduledPaymentDates[scheduledPaymentDates.Count - 1].AddDays(1) : scheduledPaymentDates[delay];
                        break;

                    //Checks whether frequency is set to "All Payments".
                    case (int)Constants.FeeFrequency.AllPayments:
                        effectiveDate = delay >= allPaymentDates.Count ? allPaymentDates[allPaymentDates.Count - 1].DateIn.AddDays(1) : allPaymentDates[delay].DateIn;
                        break;
                }

                sbTracing.AppendLine("Exit:From Class ManagementAndMaintenanceFeeCalc and method name EffectiveDateOfFee(int frequency, int delay, DateTime effectiveDate, List<DateTime> scheduledPaymentDates, List<DateTime> allPaymentDates)");
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
                sbTracing.AppendLine("Enter:Inside Class ManagementAndMaintenanceFeeCalc and method name CalculateFee(double fixedFee, double inputFeePercent, bool isGreaterValue, double feeMaxPer, double feeMin, double amount).");
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
                    feeToPay = feeMin > greaterOrLesserOutCome ? feeMin : greaterOrLesserOutCome;
                }
                //Check whether the lower limit is either not set or set to 0.
                else if (feeMin == 0)
                {
                    feeToPay = feeMaxPer < greaterOrLesserOutCome ? feeMaxPer : greaterOrLesserOutCome;
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
                sbTracing.AppendLine("Exit:From Class ManagementAndMaintenanceFeeCalc and method name CalculateFee(double fixedFee, double inputFeePercent, bool isGreaterValue, double feeMaxPer, double feeMin, double amount)");
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
        /// <param name="date"></param>
        /// <param name="outputGrid"></param>
        /// <param name="payableFee"></param>
        /// <param name="isManagementFeeCalculation"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static double PayableAmountAsPerMonthAndLoan(LoanDetails scheduleInput, DateTime date, List<OutputGrid> outputGrid, double payableFee, bool isManagementFeeCalculation, int index)
        {
            StringBuilder sbTracing = new StringBuilder();
            sbTracing.AppendLine("Enter:Inside Class ManagementAndMaintenanceFeeCalc and method name PayableAmountAsPerMonthAndLoan(LoanDetails scheduleInput, DateTime date, List<OutputGrid> outputGrid, double payableFee, bool isManagementFeeCalculation, int index).");
            sbTracing.AppendLine("Parameter values are : date = " + date + ", payableFee = " + payableFee + ", isManagementFeeCalculation = " + isManagementFeeCalculation + ", index = " + index);

            try
            {
                double maxFeePerMonth = isManagementFeeCalculation ? scheduleInput.ManagementFeeMaxMonth : scheduleInput.MaintenanceFeeMaxMonth;
                double maxFeePerLoan = isManagementFeeCalculation ? scheduleInput.ManagementFeeMaxLoan : scheduleInput.MaintenanceFeeMaxLoan;
                List<OutputGrid> copyOutputGrid = outputGrid.Take(index).ToList();

                //Calculate fee for per month
                if (maxFeePerMonth != 0)
                {
                    sbTracing.AppendLine("Inside ManagementAndMaintenanceFeeCalc class, and PayableAmountAsPerMonthAndLoan() method. Calling method : PaidAmountTillDate(copyOutputGrid, date, isManagementFeeCalculation, true);");
                    double paidFeePerMonth = copyOutputGrid.Count(o => o.PaymentDate.Month == date.Month && o.PaymentDate.Year == date.Year) == 0 ? 0 : PaidAmountTillDate(copyOutputGrid, date, isManagementFeeCalculation, true);

                    if (copyOutputGrid.Count > 0 && copyOutputGrid[0].PaymentDate.Month == date.Month && copyOutputGrid[0].PaymentDate.Year == date.Year)
                    {
                        paidFeePerMonth -= isManagementFeeCalculation ? scheduleInput.EarnedManagementFee : scheduleInput.EarnedMaintenanceFee;
                        paidFeePerMonth = Math.Round(paidFeePerMonth, 2, MidpointRounding.AwayFromZero);
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
                    sbTracing.AppendLine("Inside ManagementAndMaintenanceFeeCalc class, and PayableAmountAsPerMonthAndLoan() method. Calling method : PaidAmountTillDate(copyOutputGrid, date, isManagementFeeCalculation, false);");
                    double paidFeePerLoan = copyOutputGrid.Count == 0 ? 0 : PaidAmountTillDate(copyOutputGrid, date, isManagementFeeCalculation, false);

                    if (copyOutputGrid.Count > 0)
                    {
                        paidFeePerLoan -= isManagementFeeCalculation ? scheduleInput.EarnedManagementFee : scheduleInput.EarnedMaintenanceFee;
                        paidFeePerLoan = Math.Round(paidFeePerLoan, 2, MidpointRounding.AwayFromZero);
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

                sbTracing.AppendLine("Exit:From Class ManagementAndMaintenanceFeeCalc and method name PayableAmountAsPerMonthAndLoan(LoanDetails scheduleInput, DateTime date, List<OutputGrid> outputGrid, double payableFee, bool isManagementFeeCalculation, int index)");
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
        /// This function determine the amount that has been paid till date.
        /// </summary>
        /// <param name="copyOutputGrid"></param>
        /// <param name="date"></param>
        /// <param name="isManagementFeeCalculation"></param>
        /// <param name="isForMonth"></param>
        /// <returns></returns>
        private static double PaidAmountTillDate(List<OutputGrid> copyOutputGrid, DateTime date, bool isManagementFeeCalculation, bool isForMonth)
        {
            double paidFee = 0;
            int lastSkippedIndex = 0;
            StringBuilder sbTracing = new StringBuilder();
            sbTracing.AppendLine("Enter:Inside Class ManagementAndMaintenanceFeeCalc and method name PaidAmountTillDate(List<OutputGrid> copyOutputGrid, DateTime date, bool isManagementFeeCalculation, bool isForMonth).");
            sbTracing.AppendLine("Parameter values are : date = " + date + ", isForMonth = " + isForMonth + ", isManagementFeeCalculation = " + isManagementFeeCalculation);

            try
            {
                int startIndex = (isForMonth ? copyOutputGrid.FindIndex(o => o.PaymentDate.Month == date.Month && o.PaymentDate.Year == date.Year) : 0);
                for (int i = startIndex; i < copyOutputGrid.Count; i++)
                {
                    #region Paid Management Fee Amount

                    if (isManagementFeeCalculation)
                    {
                        paidFee += copyOutputGrid[i].ManagementFee;

                        int outputRow = i - 1;
                        while (outputRow >= lastSkippedIndex)
                        {
                            if (copyOutputGrid[outputRow].Flags == (int)Constants.FlagValues.SkipPayment)
                            {
                                paidFee -= copyOutputGrid[outputRow].ManagementFee;
                                lastSkippedIndex = outputRow + 1;
                                break;
                            }
                            outputRow--;
                        }
                    }

                    #endregion

                    #region Paid Maintenance Fee Amount

                    else
                    {
                        paidFee += copyOutputGrid[i].MaintenanceFee;

                        int outputRow = i - 1;
                        while (outputRow >= lastSkippedIndex)
                        {
                            if (copyOutputGrid[outputRow].Flags == (int)Constants.FlagValues.SkipPayment)
                            {
                                paidFee -= copyOutputGrid[outputRow].MaintenanceFee;
                                lastSkippedIndex = outputRow + 1;
                                break;
                            }
                            outputRow--;
                        }
                    }

                    #endregion
                }
                sbTracing.AppendLine("Exit:From Class ManagementAndMaintenanceFeeCalc and method name PaidAmountTillDate(List<OutputGrid> copyOutputGrid, DateTime date, bool isManagementFeeCalculation, bool isForMonth)");
                return paidFee;
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
        public static List<Models.ManagementFeeTable> CreateManagementFeeTable(DateTime effectiveDate, int frequency, int frequencyNumber, List<DateTime> scheduledPaymentDates, List<Models.AdditionalPaymentRecord> allPaymentDates, int delay)
        {
            List<Models.ManagementFeeTable> managementFeeTable = new List<Models.ManagementFeeTable>();
            DateTime lastFeeAssessmentDate = (frequency == (int)Constants.FeeFrequency.Days || frequency == (int)Constants.FeeFrequency.Months || frequency == (int)Constants.FeeFrequency.AllPayments) ?
                                                                    allPaymentDates[allPaymentDates.Count - 1].DateIn : scheduledPaymentDates[scheduledPaymentDates.Count - 1];

            StringBuilder sbTracing = new StringBuilder();
            sbTracing.AppendLine("Enter:Inside Business Class ManagementAndMaintenanceFeeCalc and method name CreateManagementFeeTable(DateTime effectiveDate, int frequency, int frequencyNumber, List<DateTime> scheduledPaymentDates, List<DateTime> allPaymentDates).");
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
                            managementFeeTable.Add(new Models.ManagementFeeTable { AssessmentDate = effectiveDate, Fee = 0 });
                            effectiveDate = effectiveDate.AddDays(frequencyNumber);
                        }
                        break;

                    //Checks whether management fee frequency is set to "Months".
                    case (int)Constants.FeeFrequency.Months:
                        effectiveDate = effectiveDate.AddMonths(frequencyNumber);
                        while (effectiveDate <= lastFeeAssessmentDate)
                        {
                            managementFeeTable.Add(new Models.ManagementFeeTable { AssessmentDate = effectiveDate, Fee = 0 });
                            effectiveDate = effectiveDate.AddMonths(frequencyNumber);
                        }
                        break;

                    //Checks whether management fee frequency is set to "Scheduled Payments".
                    case (int)Constants.FeeFrequency.ScheduledPayments:
                        int index = scheduledPaymentDates.IndexOf(effectiveDate);
                        for (int i = (index + frequencyNumber); i < scheduledPaymentDates.Count && index != -1; i += frequencyNumber)
                        {
                            effectiveDate = scheduledPaymentDates[i];
                            managementFeeTable.Add(new Models.ManagementFeeTable { AssessmentDate = effectiveDate, Fee = 0 });
                        }
                        break;

                    //Checks whether management fee frequency is set to "All Payments".
                    case (int)Constants.FeeFrequency.AllPayments:
                        for (int i = (delay + frequencyNumber); i < allPaymentDates.Count && delay < allPaymentDates.Count; i += frequencyNumber)
                        {
                            effectiveDate = allPaymentDates[i].DateIn;
                            managementFeeTable.Add(new Models.ManagementFeeTable { AssessmentDate = effectiveDate, Fee = 0, PaymentId = Convert.ToString(allPaymentDates[i].PaymentID) });
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
        public static List<Models.MaintenanceFeeTable> CreateMaintenanceFeeTable(DateTime effectiveDate, int frequency, int frequencyNumber, List<DateTime> scheduledPaymentDates, List<Models.AdditionalPaymentRecord> allPaymentDates, int delay)
        {
            List<Models.MaintenanceFeeTable> maintenanceFeeTable = new List<Models.MaintenanceFeeTable>();
            DateTime lastFeeAssessmentDate = (frequency == (int)Constants.FeeFrequency.Days || frequency == (int)Constants.FeeFrequency.Months || frequency == (int)Constants.FeeFrequency.AllPayments) ?
                                                                    allPaymentDates[allPaymentDates.Count - 1].DateIn : scheduledPaymentDates[scheduledPaymentDates.Count - 1];

            StringBuilder sbTracing = new StringBuilder();
            sbTracing.AppendLine("Enter:Inside Business Class ManagementAndMaintenanceFeeCalc and method name CreateMaintenanceFeeTable(DateTime effectiveDate, int frequency, int frequencyNumber, List<DateTime> scheduledPaymentDates, List<AdditionalPaymentRecord> allPaymentDates, int delay).");
            sbTracing.AppendLine("Parameter values are : effectiveDate = " + effectiveDate + ", frequency = " + frequency + ", frequencyNumber = " + frequencyNumber + ", delay = " + delay);

            try
            {
                switch(frequency)
                {
                    //Checks whether maintenance fee frequency is set to "Days".
                    case (int)Constants.FeeFrequency.Days:
                        effectiveDate = effectiveDate.AddDays(frequencyNumber);
                        while (effectiveDate <= lastFeeAssessmentDate)
                        {
                            maintenanceFeeTable.Add(new Models.MaintenanceFeeTable { AssessmentDate = effectiveDate, Fee = 0 });
                            effectiveDate = effectiveDate.AddDays(frequencyNumber);
                        }
                        break;

                    //Checks whether maintenance fee frequency is set to "Months".
                    case (int)Constants.FeeFrequency.Months:
                        effectiveDate = effectiveDate.AddMonths(frequencyNumber);
                        while (effectiveDate <= lastFeeAssessmentDate)
                        {
                            maintenanceFeeTable.Add(new Models.MaintenanceFeeTable { AssessmentDate = effectiveDate, Fee = 0 });
                            effectiveDate = effectiveDate.AddMonths(frequencyNumber);
                        }
                        break;

                    //Checks whether maintenance fee frequency is set to "Scheduled Payments".
                    case (int)Constants.FeeFrequency.ScheduledPayments:
                        int index = scheduledPaymentDates.IndexOf(effectiveDate);
                        for (int i = (index + frequencyNumber); i < scheduledPaymentDates.Count && index != -1; i += frequencyNumber)
                        {
                            effectiveDate = scheduledPaymentDates[i];
                            maintenanceFeeTable.Add(new Models.MaintenanceFeeTable { AssessmentDate = effectiveDate, Fee = 0 });
                        }
                        break;

                    //Checks whether maintenance fee frequency is set to "All Payments".
                    case (int)Constants.FeeFrequency.AllPayments:
                        for (int i = (delay + frequencyNumber); i < allPaymentDates.Count && delay < allPaymentDates.Count; i += frequencyNumber)
                        {
                            effectiveDate = allPaymentDates[i].DateIn;
                            maintenanceFeeTable.Add(new Models.MaintenanceFeeTable { AssessmentDate = effectiveDate, Fee = 0, PaymentId = Convert.ToString(allPaymentDates[i].PaymentID) });
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
        /// <param name="isEarlyPayOff"></param>
        /// <param name="managementFeeTableIndex"></param>
        /// <param name="lastManagementFeeChargedIndex"></param>
        /// <returns></returns>
        public static double CalculateManagementFeeToCharge(LoanDetails scheduleInput, OutputSchedule outputSchedule, List<OutputGrid> outputGrid, int outputGridRow,
                                        int inputRowNumber, bool isForAdditionalPayment, bool isExtendedRow, bool isEarlyPayOff, ref int managementFeeTableIndex, ref int lastManagementFeeChargedIndex)
        {
            int previousChargedIndex = lastManagementFeeChargedIndex;
            int managementFeeIndex = managementFeeTableIndex;
            double managementFeePayable = 0;
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside Business Class ManagementAndMaintenanceFeeCalc and method name CalculateManagementFeeToCharge(LoanDetails scheduleInput, OutputSchedule outputSchedule, List<OutputGrid> outputGrid, " +
                                        "int outputGridRow, int inputRowNumber, bool isForAdditionalPayment, bool isExtendedRow, bool isEarlyPayOff, ref int managementFeeTableIndex, ref int lastManagementFeeChargedIndex).");
                sbTracing.AppendLine("Parameter values are : outputGridRow = " + outputGridRow + ", inputRowNumber = " + inputRowNumber + ", isForAdditionalPayment = " + isForAdditionalPayment + ", isExtendedRow = " + isExtendedRow +
                    ", isEarlyPayOff = " + isEarlyPayOff + ", managementFeeTableIndex = " + managementFeeTableIndex + ", lastManagementFeeChargedIndex = " + lastManagementFeeChargedIndex);

                //Checks whether this function is called for calculating the fee while schedule for additional payment is generated.
                if (isForAdditionalPayment)
                {
                    #region Management Fee calculations in Additional Payment row
                    if (scheduleInput.ManagementFeeBasis != (int)Constants.FeeBasis.ActualPayment && scheduleInput.ManagementFeeBasis != (int)Constants.FeeBasis.ScheduledPayment)
                    {
                        if (scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.PrincipalOnly || scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.AdditionalPayment ||
                            scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.NSFFee || scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.LateFee ||
                            scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.ManagementFee || scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.MaintenanceFee ||
                                (((scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags != (int)Constants.FlagValues.Discount) && (scheduleInput.ManagementFeeFrequency == (int)Constants.FeeFrequency.AllPayments ||
                                scheduleInput.ManagementFeeFrequency == (int)Constants.FeeFrequency.ScheduledPayments)) ||
                                ((scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.Discount) && (scheduleInput.ManagementFeeFrequency == (int)Constants.FeeFrequency.Days ||
                                scheduleInput.ManagementFeeFrequency == (int)Constants.FeeFrequency.Months))))
                        {
                            int flagValueIndex = scheduleInput.AdditionalPaymentRecords.FindIndex(o => o.Flags == (int)Constants.FlagValues.Discount &&
                                                    managementFeeIndex < outputSchedule.ManagementFeeAssessment.Count &&
                                                    o.DateIn == outputSchedule.ManagementFeeAssessment[managementFeeIndex].AssessmentDate);
                            while (managementFeeIndex < outputSchedule.ManagementFeeAssessment.Count &&
                                ((outputSchedule.ManagementFeeAssessment[managementFeeIndex].AssessmentDate == scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn &&
                                (flagValueIndex != -1 || outputSchedule.ManagementFeeAssessment[managementFeeIndex].PaymentId == Convert.ToString(scheduleInput.AdditionalPaymentRecords[inputRowNumber].PaymentID) ||
                                scheduleInput.ManagementFeeFrequency == (int)Constants.FeeFrequency.Days || scheduleInput.ManagementFeeFrequency == (int)Constants.FeeFrequency.Months)) ||
                                outputSchedule.ManagementFeeAssessment[managementFeeIndex].AssessmentDate < scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn))
                            {
                                double amount = isExtendedRow ? outputGrid[outputGridRow - 1].BeginningPrincipal - outputGrid[outputGridRow - 1].PrincipalPaid : outputGrid[outputGridRow].BeginningPrincipal;

                                sbTracing.AppendLine("Inside ManagementAndMaintenanceFeeCalc class, and CalculateManagementFeeToCharge() method. Calling method : CalculateFee(scheduleInput.ManagementFee, " +
                                                        "scheduleInput.ManagementFeePercent, scheduleInput.IsManagementFeeGreater, scheduleInput.ManagementFeeMaxPer, scheduleInput.ManagementFeeMin, amount); ");
                                outputSchedule.ManagementFeeAssessment[managementFeeIndex].Fee = CalculateFee(scheduleInput.ManagementFee,
                                                        scheduleInput.ManagementFeePercent, scheduleInput.IsManagementFeeGreater, scheduleInput.ManagementFeeMaxPer, scheduleInput.ManagementFeeMin, amount);
                                managementFeeIndex++;

                                if (managementFeeIndex != outputSchedule.ManagementFeeAssessment.Count)
                                {
                                    flagValueIndex = scheduleInput.AdditionalPaymentRecords.FindIndex(o => o.Flags == (int)Constants.FlagValues.Discount &&
                                                        o.DateIn == outputSchedule.ManagementFeeAssessment[managementFeeIndex].AssessmentDate);
                                }
                            }
                        }
                    }
                    if (scheduleInput.ManagementFeeFrequency == (int)Constants.FeeFrequency.AllPayments)
                    {
                        
                        managementFeePayable = outputSchedule.ManagementFeeAssessment.Where(o => o.AssessmentDate == scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn &&
                                                            o.PaymentId == Convert.ToString(scheduleInput.AdditionalPaymentRecords[inputRowNumber].PaymentID)).Sum(o => o.Fee);

                        lastManagementFeeChargedIndex = outputSchedule.ManagementFeeAssessment.FindIndex(o => o.AssessmentDate == scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn &&
                                                            o.PaymentId == Convert.ToString(scheduleInput.AdditionalPaymentRecords[inputRowNumber].PaymentID));
                        lastManagementFeeChargedIndex = (lastManagementFeeChargedIndex == -1 ? previousChargedIndex : lastManagementFeeChargedIndex);

                        sbTracing.AppendLine("Inside ManagementAndMaintenanceFeeCalc class, and CalculateManagementFeeToCharge() method. Calling method : PayableAmountAsPerMonthAndLoan(scheduleInput, " +
                                                            "scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn, outputGrid, managementFeePayable, true, outputGridRow); ");
                        //Determine the payable amount
                        managementFeePayable = PayableAmountAsPerMonthAndLoan(scheduleInput, scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn, outputGrid, managementFeePayable, true, outputGridRow);
                    }
                    #endregion
                }
                else
                {
                    #region Management Fee calculations in input grid row
                    DateTime inputGridDate;
                    
                    if (isEarlyPayOff)
                    {
                        inputGridDate = scheduleInput.EarlyPayoffDate;
                    }
                    else
                    {
                        inputGridDate = scheduleInput.InputRecords[inputRowNumber].EffectiveDate == DateTime.MinValue ? scheduleInput.InputRecords[inputRowNumber].DateIn : scheduleInput.InputRecords[inputRowNumber].EffectiveDate;
                    }
                    int mgmtTblIndex = outputSchedule.ManagementFeeAssessment.FindIndex(o => o.AssessmentDate == inputGridDate);
                    if (scheduleInput.ManagementFeeBasis != (int)Constants.FeeBasis.ActualPayment && scheduleInput.ManagementFeeBasis != (int)Constants.FeeBasis.ScheduledPayment)
                    {
       
                        while (managementFeeIndex < outputSchedule.ManagementFeeAssessment.Count && managementFeeIndex<= mgmtTblIndex &&
                                    (outputSchedule.ManagementFeeAssessment[managementFeeIndex].AssessmentDate <= inputGridDate))
                        {
                            double amount = outputGrid[outputGridRow].BeginningPrincipal;

                            sbTracing.AppendLine("Inside ManagementAndMaintenanceFeeCalc class, and CalculateManagementFeeToCharge() method. Calling method : CalculateFee(scheduleInput.ManagementFee, " +
                                                    "scheduleInput.ManagementFeePercent, scheduleInput.IsManagementFeeGreater, scheduleInput.ManagementFeeMaxPer, scheduleInput.ManagementFeeMin, amount); ");
                            outputSchedule.ManagementFeeAssessment[managementFeeIndex].Fee = CalculateFee(scheduleInput.ManagementFee,
                                                    scheduleInput.ManagementFeePercent, scheduleInput.IsManagementFeeGreater, scheduleInput.ManagementFeeMaxPer, scheduleInput.ManagementFeeMin, amount);
                            managementFeeIndex++;
                        }
                    }

                    for (int index = previousChargedIndex + 1; index < outputSchedule.ManagementFeeAssessment.Count && index <= mgmtTblIndex && (outputSchedule.ManagementFeeAssessment[index].AssessmentDate <= inputGridDate); index++)
                    {
                        managementFeePayable += outputSchedule.ManagementFeeAssessment[index].Fee;
                        lastManagementFeeChargedIndex = index;
                    }
                    sbTracing.AppendLine("Inside ManagementAndMaintenanceFeeCalc class, and CalculateManagementFeeToCharge() method. Calling method : PayableAmountAsPerMonthAndLoan(scheduleInput, " +
                                                                 "inputGridDate, outputGrid, managementFeePayable, true, outputGridRow); ");
                    managementFeePayable = PayableAmountAsPerMonthAndLoan(scheduleInput, inputGridDate, outputGrid, managementFeePayable, true, outputGridRow);
                    #endregion
                }
                managementFeeTableIndex = managementFeeIndex;



                sbTracing.AppendLine("Exit:From Business Class ManagementAndMaintenanceFeeCalc and method name CalculateManagementFeeToCharge(LoanDetails scheduleInput, OutputSchedule outputSchedule, List<OutputGrid> outputGrid, " +
                                        "int outputGridRow, int inputRowNumber, bool isForAdditionalPayment, bool isExtendedRow, bool isEarlyPayOff, ref int managementFeeTableIndex, ref int lastManagementFeeChargedIndex)");
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
        /// <param name="isEarlyPayOff"></param>
        /// <param name="maintenanceFeeTableIndex"></param>
        /// <param name="lastMaintenanceFeeChargedIndex"></param>
        /// <returns></returns>
        public static double CalculateMaintenanceFeeToCharge(LoanDetails scheduleInput, OutputSchedule outputSchedule, List<OutputGrid> outputGrid, int outputGridRow,
                                        int inputRowNumber, bool isForAdditionalPayment, bool isExtendedRow, bool isEarlyPayOff, ref int maintenanceFeeTableIndex, ref int lastMaintenanceFeeChargedIndex)
        {
            int previousChargedIndex = lastMaintenanceFeeChargedIndex;
            int maintenanceFeeIndex = maintenanceFeeTableIndex;
            double maintenanceFeePayable = 0;
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside Business Class ManagementAndMaintenanceFeeCalc and method name CalculateMaintenanceFeeToCharge(LoanDetails scheduleInput, OutputSchedule outputSchedule, List<OutputGrid> outputGrid, int outputGridRow, " +
                                        "int inputRowNumber, bool isForAdditionalPayment, bool isExtendedRow, bool isEarlyPayOff, ref int maintenanceFeeTableIndex, ref int lastMaintenanceFeeChargedIndex).");
                sbTracing.AppendLine("Parameter values are : outputGridRow = " + outputGridRow + ", inputRowNumber = " + inputRowNumber + ", isForAdditionalPayment = " + isForAdditionalPayment +
                    ", isEarlyPayOff = " + isEarlyPayOff + ", isExtendedRow = " + isExtendedRow + ", maintenanceFeeTableIndex = " + maintenanceFeeTableIndex + ", lastMaintenanceFeeChargedIndex = " + lastMaintenanceFeeChargedIndex);

                //Checks whether this function is called for calculating the fee while schedule for additional payment is generated.
                if (isForAdditionalPayment)
                {
                    #region Maintenance Fee calculations in Additional Payment row
                    if (scheduleInput.MaintenanceFeeBasis != (int)Constants.FeeBasis.ActualPayment && scheduleInput.MaintenanceFeeBasis != (int)Constants.FeeBasis.ScheduledPayment)
                    {
                        if (scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.PrincipalOnly || scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.AdditionalPayment ||
                            scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.NSFFee || scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.LateFee ||
                            scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.ManagementFee || scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.MaintenanceFee ||
                                (((scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags != (int)Constants.FlagValues.Discount) && (scheduleInput.MaintenanceFeeFrequency == (int)Constants.FeeFrequency.AllPayments ||
                                scheduleInput.MaintenanceFeeFrequency == (int)Constants.FeeFrequency.ScheduledPayments)) ||
                                ((scheduleInput.AdditionalPaymentRecords[inputRowNumber].Flags == (int)Constants.FlagValues.Discount) && (scheduleInput.MaintenanceFeeFrequency == (int)Constants.FeeFrequency.Days ||
                                scheduleInput.MaintenanceFeeFrequency == (int)Constants.FeeFrequency.Months))))
                        {
                            int flagValueIndex = scheduleInput.AdditionalPaymentRecords.FindIndex(o => o.Flags == (int)Constants.FlagValues.Discount &&
                                                    maintenanceFeeIndex < outputSchedule.MaintenanceFeeAssessment.Count &&
                                                     o.DateIn == outputSchedule.MaintenanceFeeAssessment[maintenanceFeeIndex].AssessmentDate);
                            while (maintenanceFeeIndex < outputSchedule.MaintenanceFeeAssessment.Count &&
                                ((outputSchedule.MaintenanceFeeAssessment[maintenanceFeeIndex].AssessmentDate == scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn &&
                                (flagValueIndex != -1 || outputSchedule.MaintenanceFeeAssessment[maintenanceFeeIndex].PaymentId == Convert.ToString(scheduleInput.AdditionalPaymentRecords[inputRowNumber].PaymentID) ||
                                scheduleInput.MaintenanceFeeFrequency == (int)Constants.FeeFrequency.Days || scheduleInput.MaintenanceFeeFrequency == (int)Constants.FeeFrequency.Months)) ||
                                outputSchedule.MaintenanceFeeAssessment[maintenanceFeeIndex].AssessmentDate < scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn))
                            {
                                double amount = isExtendedRow ? outputGrid[outputGridRow - 1].BeginningPrincipal - outputGrid[outputGridRow - 1].PrincipalPaid : outputGrid[outputGridRow].BeginningPrincipal;

                                sbTracing.AppendLine("Inside ManagementAndMaintenanceFeeCalc class, and CalculateMaintenanceFeeToCharge() method. Calling method : CalculateFee(scheduleInput.MaintenanceFee, " +
                                                        "scheduleInput.MaintenanceFeePercent, scheduleInput.IsMaintenanceFeeGreater, scheduleInput.MaintenanceFeeMaxPer, scheduleInput.MaintenanceFeeMin, amount); ");
                                outputSchedule.MaintenanceFeeAssessment[maintenanceFeeIndex].Fee = CalculateFee(scheduleInput.MaintenanceFee,
                                                        scheduleInput.MaintenanceFeePercent, scheduleInput.IsMaintenanceFeeGreater, scheduleInput.MaintenanceFeeMaxPer, scheduleInput.MaintenanceFeeMin, amount);
                                maintenanceFeeIndex++;

                                if (maintenanceFeeIndex != outputSchedule.MaintenanceFeeAssessment.Count)
                                {
                                    flagValueIndex = scheduleInput.AdditionalPaymentRecords.FindIndex(o => o.Flags == (int)Constants.FlagValues.Discount &&
                                                         o.DateIn == outputSchedule.MaintenanceFeeAssessment[maintenanceFeeIndex].AssessmentDate);
                                }
                            }
                        }
                    }

                    if (scheduleInput.MaintenanceFeeFrequency == (int)Constants.FeeFrequency.AllPayments)
                    {
                        maintenanceFeePayable = outputSchedule.MaintenanceFeeAssessment.Where(o => o.AssessmentDate == scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn &&
                                                    o.PaymentId == Convert.ToString(scheduleInput.AdditionalPaymentRecords[inputRowNumber].PaymentID)).Sum(o => o.Fee);

                        lastMaintenanceFeeChargedIndex = outputSchedule.MaintenanceFeeAssessment.FindIndex(o => o.AssessmentDate == scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn &&
                                            o.PaymentId == Convert.ToString(scheduleInput.AdditionalPaymentRecords[inputRowNumber].PaymentID));
                        lastMaintenanceFeeChargedIndex = (lastMaintenanceFeeChargedIndex == -1 ? previousChargedIndex : lastMaintenanceFeeChargedIndex);

                        sbTracing.AppendLine("Inside ManagementAndMaintenanceFeeCalc class, and CalculateMaintenanceFeeToCharge() method. Calling method : PayableAmountAsPerMonthAndLoan(scheduleInput, " +
                                                "scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn, outputGrid, maintenanceFeePayable, false, outputGridRow); ");
                        //Determine the payable amount
                        maintenanceFeePayable = PayableAmountAsPerMonthAndLoan(scheduleInput, scheduleInput.AdditionalPaymentRecords[inputRowNumber].DateIn, outputGrid, maintenanceFeePayable, false, outputGridRow);
                    }
                    #endregion
                }
                else
                {
                    #region Maintenance Fee calculations in input grid row
                    DateTime inputGridDate;
                    if (isEarlyPayOff)
                    {
                        inputGridDate = scheduleInput.EarlyPayoffDate;
                    }
                    else
                    {
                        inputGridDate = scheduleInput.InputRecords[inputRowNumber].EffectiveDate == DateTime.MinValue ? scheduleInput.InputRecords[inputRowNumber].DateIn : scheduleInput.InputRecords[inputRowNumber].EffectiveDate;
                    }
                    int maintenanceTblIndex = outputSchedule.MaintenanceFeeAssessment.FindIndex(o => o.AssessmentDate == inputGridDate);
                    if (scheduleInput.MaintenanceFeeBasis != (int)Constants.FeeBasis.ActualPayment && scheduleInput.MaintenanceFeeBasis != (int)Constants.FeeBasis.ScheduledPayment)
                    {
                        while (maintenanceFeeIndex < outputSchedule.MaintenanceFeeAssessment.Count && maintenanceFeeIndex <= maintenanceTblIndex
                                   && (outputSchedule.MaintenanceFeeAssessment[maintenanceFeeIndex].AssessmentDate <= inputGridDate))
                                     
                        {
                            double amount = outputGrid[outputGridRow].BeginningPrincipal;

                            sbTracing.AppendLine("Inside ManagementAndMaintenanceFeeCalc class, and CalculateMaintenanceFeeToCharge() method. Calling method : CalculateFee(scheduleInput.MaintenanceFee, " +
                                                    "scheduleInput.MaintenanceFeePercent, scheduleInput.IsMaintenanceFeeGreater, scheduleInput.MaintenanceFeeMaxPer, scheduleInput.MaintenanceFeeMin, amount); ");
                            outputSchedule.MaintenanceFeeAssessment[maintenanceFeeIndex].Fee = CalculateFee(scheduleInput.MaintenanceFee,
                                                    scheduleInput.MaintenanceFeePercent, scheduleInput.IsMaintenanceFeeGreater, scheduleInput.MaintenanceFeeMaxPer, scheduleInput.MaintenanceFeeMin, amount);
                            maintenanceFeeIndex++;
                        }
                    }

                    for (int index = previousChargedIndex + 1; index < outputSchedule.MaintenanceFeeAssessment.Count && index <= maintenanceTblIndex && (outputSchedule.MaintenanceFeeAssessment[index].AssessmentDate <= inputGridDate); index++)
                    {
                        maintenanceFeePayable += outputSchedule.MaintenanceFeeAssessment[index].Fee;
                        lastMaintenanceFeeChargedIndex = index;
                    }
                    sbTracing.AppendLine("Inside ManagementAndMaintenanceFeeCalc class, and CalculateMaintenanceFeeToCharge() method. Calling method : PayableAmountAsPerMonthAndLoan(scheduleInput, " +
                                            "inputGridDate, outputGrid, maintenanceFeePayable, false, outputGridRow); ");
                    maintenanceFeePayable = PayableAmountAsPerMonthAndLoan(scheduleInput, inputGridDate, outputGrid, maintenanceFeePayable, false, outputGridRow);
                    #endregion
                }
                maintenanceFeeTableIndex = maintenanceFeeIndex;
                sbTracing.AppendLine("Exit:From Business Class ManagementAndMaintenanceFeeCalc and method name CalculateMaintenanceFeeToCharge(LoanDetails scheduleInput, OutputSchedule outputSchedule, List<OutputGrid> outputGrid, int outputGridRow, " +
                                        "int inputRowNumber, bool isForAdditionalPayment, bool isExtendedRow, bool isEarlyPayOff, ref int maintenanceFeeTableIndex, ref int lastMaintenanceFeeChargedIndex)");
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
