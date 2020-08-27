using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoanAmort_driver.Models;
using Microsoft.VisualBasic;

namespace LoanAmort_driver.PeriodBusiness
{
    public class CreateScheduleForPeriodMethod
    {
        /// <summary>
        /// Name : Sparsh Agarwal
        /// Date : 21/09/2017
        /// This function will create the reschedule payment for period method as output. Here I consider Input grid for schedule dates as well as Payment dates and
        /// payment amount. This function takes the input grid values i.e. schedule dates and payment details, and some other inputs provided on top of screen.
        /// </summary>   
        /// <param name="scheduleInputs"></param>
        /// <returns></returns>
        public static OutputSchedule DefaultSchedule(LoanDetails scheduleInputs)
        {
            var scheduleOutPut = new OutputSchedule();
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside PeriodBusiness Class CreateScheduleForPeriodMethod and Method Name DefaultSchedule(LoanDetails scheduleInputs).");
                scheduleOutPut.Schedule = new List<OutputGrid>();
                scheduleOutPut.ManagementFeeAssessment = new List<Models.ManagementFeeTable>();
                scheduleOutPut.MaintenanceFeeAssessment = new List<Models.MaintenanceFeeTable>();

                double principal = (scheduleInputs.LoanAmount > 0 && scheduleInputs.AmountFinanced == 0) ? scheduleInputs.LoanAmount : scheduleInputs.AmountFinanced;

                //Calculate the principal service fee to be paid.
                sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling Method : principalSerFee.CalculatePrincipalServiceeFee(scheduleInputs.ApplyServiceFeeInterest, principal, scheduleInputs.ServiceFee);");
                double principalServiceFee = PrincipalServiceFee.CalculatePrincipalServiceeFee(scheduleInputs.ApplyServiceFeeInterest, principal, scheduleInputs.ServiceFee);

                //Calculate the Oigination Fee Calc output value.
                sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : orginationFeeForFirstPayment.GetOriginationFeeForFirstPayment(scheduleInputs.OriginationFee, scheduleInputs.OriginationFeePercent, scheduleInputs.OriginationFeeMax, scheduleInputs.OriginationFeeMin, scheduleInputs.OriginationFeeCalculationMethod, principal)");
                scheduleOutPut.OriginationFee = OriginationFeeForFirstPayment.GetOriginationFeeForFirstPayment(scheduleInputs.OriginationFee, scheduleInputs.OriginationFeePercent, scheduleInputs.OriginationFeeMax,
                                                        scheduleInputs.OriginationFeeMin, scheduleInputs.OriginationFeeCalculationMethod, principal);

                //Determine the origination fee to be paid in the first payment of repayment schedule.
                double originationFee = scheduleInputs.IsOriginationFeeFinanced ? 0 : scheduleOutPut.OriginationFee;

                //Calculate the same day fee to be paid.
                sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : sameDayFeeForFirstPayment.GetSameDayFeeForFirstPayment(scheduleInputs.SameDayFee, scheduleInputs.SameDayFeeCalculationMethod, principal);");
                double sameDayFee = SameDayFeeForFirstPayment.GetSameDayFeeForFirstPayment(scheduleInputs.SameDayFee, scheduleInputs.SameDayFeeCalculationMethod, principal);

                //Determine the loan amount calc output value upon which the repayment schedule based.
                scheduleOutPut.LoanAmountCalc = (scheduleInputs.LoanAmount > 0 && scheduleInputs.AmountFinanced == 0) ? scheduleInputs.LoanAmount :
                                                    (scheduleInputs.AmountFinanced + (scheduleInputs.IsServiceFeeFinanced ? principalServiceFee : 0) +
                                                    (scheduleInputs.IsOriginationFeeFinanced ? scheduleOutPut.OriginationFee : 0) +
                                                    (scheduleInputs.IsSameDayFeeFinanced ? sameDayFee : 0));

                if (scheduleOutPut.LoanAmountCalc <= scheduleInputs.Residual)
                {
                    return scheduleOutPut;
                }
                else if (scheduleOutPut.LoanAmountCalc <= scheduleInputs.BalloonPayment)
                {
                    return scheduleOutPut;
                }

                //Determine the principal service fee to be paid in the repayment schedule if it is not financed.
                principalServiceFee = scheduleInputs.IsServiceFeeFinanced ? 0 : Round.RoundOffAmount(principalServiceFee);
                //Calculate the same day fee to be paid in the first payment of repayment schedule.
                sameDayFee = scheduleInputs.IsSameDayFeeFinanced ? 0 : Round.RoundOffAmount(sameDayFee);
                principal = scheduleOutPut.LoanAmountCalc;

                //Default end date.
                DateTime endDate = Convert.ToDateTime(Models.Constants.DefaultDate);

                #region Calculate Amount Financed

                sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : AmountFinanced.GetAmountFinanced(scheduleInputs.LoanAmount, scheduleInputs.AmountFinanced, scheduleOutPut.OriginationFee, scheduleInputs.IsOriginationFeeFinanced, scheduleInputs.SameDayFee, scheduleInputs.SameDayFeeCalculationMethod, scheduleInputs.IsSameDayFeeFinanced, scheduleInputs.ServiceFee, scheduleInputs.ApplyServiceFeeInterest, scheduleInputs.IsServiceFeeFinanced);");
                scheduleOutPut.AmountFinanced = AmountFinanced.GetAmountFinanced(scheduleInputs.LoanAmount, scheduleInputs.AmountFinanced, scheduleOutPut.OriginationFee,
                        scheduleInputs.IsOriginationFeeFinanced, scheduleInputs.SameDayFee, scheduleInputs.SameDayFeeCalculationMethod,
                        scheduleInputs.IsSameDayFeeFinanced, scheduleInputs.ServiceFee, scheduleInputs.ApplyServiceFeeInterest, scheduleInputs.IsServiceFeeFinanced);

                #endregion

                //This variable calculates the total amount to pay in the particular period. It contains both principal and interest amount.
                sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : CalculateTotalAmount.TotalPayableAmount(scheduleInputs, scheduleOutPut.LoanAmountCalc);");
                double defaultTotalAmountToPay = CalculateTotalAmount.TotalPayableAmount(scheduleInputs, scheduleOutPut.LoanAmountCalc);
                defaultTotalAmountToPay = Round.RoundOffAmount(defaultTotalAmountToPay);

                //This function calculates the total service fee i.e. equal for each period, to be paid.
                sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : ServiceFeeAndServiceFeeInterestPayable.CalculateServiceFeeAndInterestPayable(principalServiceFee, scheduleInputs, scheduleOutPut.LoanAmountCalc);");
                double defaultTotalServiceFeePayable = ServiceFeeAndServiceFeeInterestPayable.CalculateServiceFeeAndInterestPayable(principalServiceFee, scheduleInputs, scheduleOutPut.LoanAmountCalc);
                defaultTotalServiceFeePayable = Round.RoundOffAmount(defaultTotalServiceFeePayable);
                scheduleOutPut.RegularPayment = scheduleInputs.ServiceFeeFirstPayment ? defaultTotalAmountToPay : (defaultTotalAmountToPay + defaultTotalServiceFeePayable);

                #region Calculate Management and Maintenance Fee

                //This variable contains all the scheduled payment dates i.e. if actual payment date is given, then it will consider that date.
                List<DateTime> scheduledPayments = new List<DateTime>();
                foreach (var row in scheduleInputs.InputRecords)
                {
                    scheduledPayments.Add(row.EffectiveDate != DateTime.MinValue ? row.EffectiveDate : row.DateIn);
                }
                //This variable conatins the additional payment dates excluding the discounts, NSF, and Late fee events.
                List<Models.AdditionalPaymentRecord> allPayments = scheduleInputs.AdditionalPaymentRecords.Where(o => o.Flags != (int)Models.Constants.FlagValues.Discount &&
                                                                    o.Flags != (int)Models.Constants.FlagValues.NSFFee && o.Flags != (int)Models.Constants.FlagValues.LateFee &&
                                                                    o.Flags != (int)Models.Constants.FlagValues.ManagementFee && o.Flags != (int)Models.Constants.FlagValues.MaintenanceFee).ToList();
                foreach (var payments in scheduledPayments)
                {
                    allPayments.Add(new Models.AdditionalPaymentRecord { DateIn = payments, Flags = (int)Models.Constants.FlagValues.Payment });
                }
                allPayments = allPayments.OrderBy(o => o.DateIn).ThenBy(p=>p.Flags).ToList();

                List<Models.AdditionalPaymentRecord> mgmtAllPayments = allPayments.ToList();
                if ((scheduleInputs.ManagementFeeFrequency == (int)Models.Constants.FeeFrequency.Days || scheduleInputs.ManagementFeeFrequency == (int)Models.Constants.FeeFrequency.Months) &&
                        scheduleInputs.AdditionalPaymentRecords.Count > 0 &&
                        ((scheduleInputs.InputRecords[scheduleInputs.InputRecords.Count - 1].EffectiveDate == DateTime.MinValue ? scheduleInputs.InputRecords[scheduleInputs.InputRecords.Count - 1].DateIn : scheduleInputs.InputRecords[scheduleInputs.InputRecords.Count - 1].EffectiveDate) < scheduleInputs.EarlyPayoffDate &&
                        scheduleInputs.EarlyPayoffDate <= scheduleInputs.AdditionalPaymentRecords[scheduleInputs.AdditionalPaymentRecords.Count - 1].DateIn &&
                        scheduleInputs.AdditionalPaymentRecords.FindIndex(o => o.DateIn == scheduleInputs.AdditionalPaymentRecords[scheduleInputs.AdditionalPaymentRecords.Count - 1].DateIn &&
                            (o.Flags == (int)Models.Constants.FlagValues.PrincipalOnly || o.Flags == (int)Models.Constants.FlagValues.AdditionalPayment)) == -1))
                {
                    mgmtAllPayments.Add(new Models.AdditionalPaymentRecord { DateIn = scheduleInputs.AdditionalPaymentRecords[scheduleInputs.AdditionalPaymentRecords.Count - 1].DateIn, Flags = -1 });
                }

                //Checks whether there is a value either in management fee fixed or percent input field.
                if (scheduleInputs.ManagementFee != 0 || scheduleInputs.ManagementFeePercent != 0)
                {
                    sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : ManagementAndMaintenanceFeeCalc.EffectiveDateOfFee(scheduleInputs.ManagementFeeFrequency, scheduleInputs.ManagementFeeDelay, scheduleInputs.InputRecords[0].DateIn, scheduledPayments, mgmtAllPayments);");
                    scheduleOutPut.ManagementFeeEffective = ManagementAndMaintenanceFeeCalc.EffectiveDateOfFee(scheduleInputs.ManagementFeeFrequency, scheduleInputs.ManagementFeeDelay, scheduleInputs.InputRecords[0].DateIn, scheduledPayments, mgmtAllPayments);
                    sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : ManagementAndMaintenanceFeeCalc.CreateManagementFeeTable(scheduleOutPut.ManagementFeeEffective, scheduleInputs.ManagementFeeFrequency, scheduleInputs.ManagementFeeFrequencyNumber, scheduledPayments, mgmtAllPayments, scheduleInputs.ManagementFeeDelay);");
                    scheduleOutPut.ManagementFeeAssessment = ManagementAndMaintenanceFeeCalc.CreateManagementFeeTable(scheduleOutPut.ManagementFeeEffective, scheduleInputs.ManagementFeeFrequency, scheduleInputs.ManagementFeeFrequencyNumber, scheduledPayments, mgmtAllPayments, scheduleInputs.ManagementFeeDelay);

                    //Checks whether fee basis is "Payment", or neither basis is provided nor percent value.
                    if (scheduleInputs.ManagementFeeBasis == (int)Models.Constants.FeeBasis.ActualPayment ||
                        scheduleInputs.ManagementFeeBasis == (int)Models.Constants.FeeBasis.ScheduledPayment ||
                        (scheduleInputs.ManagementFeeBasis == -1 && scheduleInputs.ManagementFeePercent == 0))
                    {
                        int managementFeeTableIndex = 0;
                        while (managementFeeTableIndex < scheduleOutPut.ManagementFeeAssessment.Count)
                        {
                            double amount = 0;
                            if (string.IsNullOrEmpty(scheduleOutPut.ManagementFeeAssessment[managementFeeTableIndex].PaymentId) || scheduleOutPut.ManagementFeeAssessment[managementFeeTableIndex].PaymentId == "0")
                            {
                                if (scheduleInputs.ManagementFeeBasis == (int)Models.Constants.FeeBasis.ActualPayment)
                                {
                                    int index = scheduledPayments.IndexOf(scheduleOutPut.ManagementFeeAssessment[managementFeeTableIndex].AssessmentDate);
                                    //Evaluate the amount on which fee will be calculated.
                                    amount = (string.IsNullOrEmpty(scheduleInputs.InputRecords[index].PaymentAmount) || scheduleInputs.InputRecords[index].PaymentAmount == "0") ? defaultTotalAmountToPay : Convert.ToDouble(scheduleInputs.InputRecords[index].PaymentAmount);
                                }
                                else if (scheduleInputs.ManagementFeeBasis == (int)Models.Constants.FeeBasis.ScheduledPayment)
                                {
                                    amount = defaultTotalAmountToPay;
                                }
                            }
                            else
                            {
                                //Sum the additional payment amount when frquency is "All Payments".
                                amount = scheduleInputs.AdditionalPaymentRecords.Where(o => o.DateIn == scheduleOutPut.ManagementFeeAssessment[managementFeeTableIndex].AssessmentDate &&
                                                o.Flags != (int)Models.Constants.FlagValues.Discount && o.Flags != (int)Models.Constants.FlagValues.NSFFee && o.Flags != (int)Models.Constants.FlagValues.LateFee &&
                                                o.Flags != (int)Models.Constants.FlagValues.ManagementFee && o.Flags != (int)Models.Constants.FlagValues.MaintenanceFee &&
                                                o.PaymentID == Convert.ToInt32(scheduleOutPut.ManagementFeeAssessment[managementFeeTableIndex].PaymentId)).Sum(o => o.AdditionalPayment);
                            }

                            sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateFee(scheduleInputs.ManagementFee, scheduleInputs.ManagementFeePercent, scheduleInputs.IsManagementFeeGreater, scheduleInputs.ManagementFeeMaxPer, scheduleInputs.ManagementFeeMin, amount);");
                            scheduleOutPut.ManagementFeeAssessment[managementFeeTableIndex].Fee = ManagementAndMaintenanceFeeCalc.CalculateFee(scheduleInputs.ManagementFee,
                                                                scheduleInputs.ManagementFeePercent, scheduleInputs.IsManagementFeeGreater, scheduleInputs.ManagementFeeMaxPer, scheduleInputs.ManagementFeeMin, amount);
                            managementFeeTableIndex++;
                        }
                    }
                }

                if ((scheduleInputs.MaintenanceFeeFrequency == (int)Models.Constants.FeeFrequency.Days || scheduleInputs.MaintenanceFeeFrequency == (int)Models.Constants.FeeFrequency.Months) &&
                        scheduleInputs.AdditionalPaymentRecords.Count > 0 &&
                        ((scheduleInputs.InputRecords[scheduleInputs.InputRecords.Count - 1].EffectiveDate == DateTime.MinValue ? scheduleInputs.InputRecords[scheduleInputs.InputRecords.Count - 1].DateIn : scheduleInputs.InputRecords[scheduleInputs.InputRecords.Count - 1].EffectiveDate) < scheduleInputs.EarlyPayoffDate &&
                        scheduleInputs.EarlyPayoffDate <= scheduleInputs.AdditionalPaymentRecords[scheduleInputs.AdditionalPaymentRecords.Count - 1].DateIn &&
                        scheduleInputs.AdditionalPaymentRecords.FindIndex(o => o.DateIn == scheduleInputs.AdditionalPaymentRecords[scheduleInputs.AdditionalPaymentRecords.Count - 1].DateIn &&
                            (o.Flags == (int)Models.Constants.FlagValues.PrincipalOnly || o.Flags == (int)Models.Constants.FlagValues.AdditionalPayment)) == -1))
                {
                    allPayments.Add(new Models.AdditionalPaymentRecord { DateIn = scheduleInputs.AdditionalPaymentRecords[scheduleInputs.AdditionalPaymentRecords.Count - 1].DateIn, Flags = -1 });
                }

                //Checks whether there is a value either in maintenance fee fixed or percent input field.
                if (scheduleInputs.MaintenanceFee != 0 || scheduleInputs.MaintenanceFeePercent != 0)
                {
                    sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : ManagementAndMaintenanceFeeCalc.EffectiveDateOfFee(scheduleInputs.MaintenanceFeeFrequency, scheduleInputs.MaintenanceFeeDelay, scheduleInputs.InputRecords[0].DateIn, scheduledPayments, allPayments);");
                    scheduleOutPut.MaintenanceFeeEffective = ManagementAndMaintenanceFeeCalc.EffectiveDateOfFee(scheduleInputs.MaintenanceFeeFrequency, scheduleInputs.MaintenanceFeeDelay, scheduleInputs.InputRecords[0].DateIn, scheduledPayments, allPayments);
                    sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : ManagementAndMaintenanceFeeCalc.CreateMaintenanceFeeTable(scheduleOutPut.MaintenanceFeeEffective, scheduleInputs.MaintenanceFeeFrequency, scheduleInputs.MaintenanceFeeFrequencyNumber, scheduledPayments, allPayments, scheduleInputs.MaintenanceFeeDelay);");
                    scheduleOutPut.MaintenanceFeeAssessment = ManagementAndMaintenanceFeeCalc.CreateMaintenanceFeeTable(scheduleOutPut.MaintenanceFeeEffective, scheduleInputs.MaintenanceFeeFrequency, scheduleInputs.MaintenanceFeeFrequencyNumber, scheduledPayments, allPayments, scheduleInputs.MaintenanceFeeDelay);

                    //Checks whether fee basis is "Payment", or neither basis is provided nor percent value.
                    if (scheduleInputs.MaintenanceFeeBasis == (int)Models.Constants.FeeBasis.ActualPayment ||
                        scheduleInputs.MaintenanceFeeBasis == (int)Models.Constants.FeeBasis.ScheduledPayment ||
                        (scheduleInputs.MaintenanceFeeBasis == -1 && scheduleInputs.MaintenanceFeePercent == 0))
                    {
                        int maintenanceFeeTableIndex = 0;
                        while (maintenanceFeeTableIndex < scheduleOutPut.MaintenanceFeeAssessment.Count)
                        {
                            double amount = 0;
                            if (string.IsNullOrEmpty(scheduleOutPut.MaintenanceFeeAssessment[maintenanceFeeTableIndex].PaymentId) || scheduleOutPut.MaintenanceFeeAssessment[maintenanceFeeTableIndex].PaymentId == "0")
                            {
                                if (scheduleInputs.MaintenanceFeeBasis == (int)Models.Constants.FeeBasis.ActualPayment)
                                {
                                    int index = scheduledPayments.IndexOf(scheduleOutPut.MaintenanceFeeAssessment[maintenanceFeeTableIndex].AssessmentDate);
                                    //Evaluate the amount on which fee will be calculated.
                                    amount = (string.IsNullOrEmpty(scheduleInputs.InputRecords[index].PaymentAmount) || scheduleInputs.InputRecords[index].PaymentAmount == "0") ? defaultTotalAmountToPay : Convert.ToDouble(scheduleInputs.InputRecords[index].PaymentAmount);
                                }
                                else if (scheduleInputs.MaintenanceFeeBasis == (int)Models.Constants.FeeBasis.ScheduledPayment)
                                {
                                    amount = defaultTotalAmountToPay;
                                }
                            }
                            else
                            {
                                //Sum the additional payment amount when frquency is "All Payments".
                                amount += scheduleInputs.AdditionalPaymentRecords.Where(o => o.DateIn == scheduleOutPut.MaintenanceFeeAssessment[maintenanceFeeTableIndex].AssessmentDate &&
                                                o.Flags != (int)Models.Constants.FlagValues.Discount && o.Flags != (int)Models.Constants.FlagValues.NSFFee && o.Flags != (int)Models.Constants.FlagValues.LateFee &&
                                                o.Flags != (int)Models.Constants.FlagValues.ManagementFee && o.Flags != (int)Models.Constants.FlagValues.MaintenanceFee &&
                                                o.PaymentID == Convert.ToInt32(scheduleOutPut.MaintenanceFeeAssessment[maintenanceFeeTableIndex].PaymentId)).Sum(o => o.AdditionalPayment);
                            }

                            sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateFee(scheduleInputs.MaintenanceFee, scheduleInputs.MaintenanceFeePercent, scheduleInputs.IsMaintenanceFeeGreater, scheduleInputs.MaintenanceFeeMaxPer, scheduleInputs.MaintenanceFeeMin, amount); ");
                            scheduleOutPut.MaintenanceFeeAssessment[maintenanceFeeTableIndex].Fee = ManagementAndMaintenanceFeeCalc.CalculateFee(scheduleInputs.MaintenanceFee,
                                                                scheduleInputs.MaintenanceFeePercent, scheduleInputs.IsMaintenanceFeeGreater, scheduleInputs.MaintenanceFeeMaxPer, scheduleInputs.MaintenanceFeeMin, amount);
                            maintenanceFeeTableIndex++;
                        }
                    }
                }

                #endregion

                double interestCarryOver = 0;
                double serviceFeeInterestCarryOver = 0;
                for (int i = 1; i < scheduleInputs.InputRecords.Count; i++)
                {
                    double interestPayment;
                    double totalAmountToPay = defaultTotalAmountToPay;
                    double totalServiceFeePayable = defaultTotalServiceFeePayable;

                    //It determines the start date from which interest will be calculated for that period. If it is first scheduled payment, then "DateIn" column in input 
                    //grid will considered as start date, otherwise last end date will be considered.
                    DateTime startDate = i == 1 ? scheduleInputs.InputRecords[i - 1].DateIn : endDate;

                    //It determines the end date. Interest will be calculated till the end date for that period.
                    endDate = scheduleInputs.InputRecords[i].EffectiveDate == DateTime.MinValue ? scheduleInputs.InputRecords[i].DateIn : scheduleInputs.InputRecords[i].EffectiveDate;

                    #region default Periodic Interest Rate, Interest accrued and User story 23

                    //This variable calls a function which calculate the periodic interest rate for that period.
                    double periodicInterestRate;
                    double interestAccrued;
                    if (string.IsNullOrEmpty(scheduleInputs.InputRecords[i].InterestRate))
                    {
                        sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, principal, true, true);");
                        periodicInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, principal, true, true, false);

                        sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, principal, true, true);");
                        interestAccrued = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, principal, true, true, false);
                    }
                    else
                    {
                        if (Convert.ToInt32(scheduleInputs.InputRecords[i].InterestRate) == 0)
                        {
                            totalAmountToPay = Round.RoundOffAmount(scheduleOutPut.LoanAmountCalc / (scheduleInputs.InputRecords.Count - 1)) +
                                               Round.RoundOffAmount(scheduleInputs.EarnedInterest / (scheduleInputs.InputRecords.Count - 1));
                            totalServiceFeePayable = Round.RoundOffAmount(principalServiceFee / (scheduleInputs.InputRecords.Count - 1)) +
                                                     Round.RoundOffAmount(scheduleInputs.EarnedServiceFeeInterest / (scheduleInputs.InputRecords.Count - 1));
                        }
                        sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : CalculatePeriodicInterestRate.PeriodicRateCalcWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.InputRecords[i].InterestRate), scheduleInputs.InputRecords[0].DateIn, true, true);");
                        periodicInterestRate = CalculatePeriodicInterestRate.PeriodicRateCalcWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.InputRecords[i].InterestRate), scheduleInputs.InputRecords[0].DateIn, true, true, false);

                        sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.InputRecords[i].InterestRate), scheduleInputs.InputRecords[0].DateIn, principal, true, true);");
                        interestAccrued = PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.InputRecords[i].InterestRate), scheduleInputs.InputRecords[0].DateIn, principal, true, true, false);
                    }

                    #endregion

                    double payablePrincipal = (principal < Round.RoundOffAmount(principal - scheduleInputs.Residual) ? principal : Round.RoundOffAmount(principal - scheduleInputs.Residual));

                    interestAccrued = Round.RoundOffAmount(interestAccrued);

                    //This variable calculates the daily interest amount for the period.
                    sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : DailyInterestAmount.CalculateDailyInterestAmount(startDate, endDate, interestAccrued, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InterestDelay,scheduleInputs, true);");
                    double dailyInterestAmount = DailyInterestAmount.CalculateDailyInterestAmount(startDate, endDate, interestAccrued, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InterestDelay, scheduleInputs, true);
                    dailyInterestAmount = Round.RoundOffAmount(dailyInterestAmount);

                    //Add the earned interest amount to the accrued value.
                    interestAccrued += (i == 1 ? scheduleInputs.EarnedInterest : 0);

                    #region Calculate principal and interest to pay
                    double interestDue = interestAccrued + interestCarryOver;

                    //This variable determines the principal amount to pay for particular period by subtracting the periodic interest from total amount.
                    double principalAmountToPay;
                    //This condition determine whether the row is last row. If no, the carry over of interest will be added to interest accrued for current schedule, and
                    //limit to total payment. If it last payment schedule, all remaining principal amount will be added to total amount to be paid.
                    if (i != scheduleInputs.InputRecords.Count - 1)
                    {
                        if ((interestAccrued + interestCarryOver) >= totalAmountToPay)
                        {
                            if (i == 1 && scheduleInputs.MinDuration > Models.Constants.MinDurationValue)
                            {
                                interestCarryOver = 0;
                                interestPayment = interestAccrued;
                                totalAmountToPay = interestAccrued;
                            }
                            else
                            {
                                interestCarryOver = Round.RoundOffAmount(interestAccrued + interestCarryOver - totalAmountToPay);
                                interestPayment = totalAmountToPay;
                            }
                            principalAmountToPay = 0;
                        }
                        else
                        {
                            interestPayment = interestAccrued + interestCarryOver;
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

                        #region Apply Enforcement Principal from given enforcement Payment
                        if (i >= scheduleInputs.EnforcedPayment && principalAmountToPay == 0)
                        {
                            double payableEnforcedPrincipal = scheduleInputs.EnforcedPrincipal > payablePrincipal ? payablePrincipal : scheduleInputs.EnforcedPrincipal;
                            if (i == 1 && scheduleInputs.MinDuration > Models.Constants.MinDurationValue)
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
                        principalAmountToPay = payablePrincipal;
                        interestPayment = interestAccrued + interestCarryOver;
                        totalAmountToPay = principalAmountToPay + interestPayment;
                        interestCarryOver = 0;
                    }
                    #endregion

                    #region Calculate the service fee and service fee interest to pay
                    //determine the service fee, service fee interest to be paid in each period, and calculate the remaining principal service fee on which service fee
                    //interest is being calculated.
                    double payableServiceFee = 0;
                    double payableServiceFeeInterest = 0;
                    double accruedServiceFeeInterest = 0;
                    double serviceFeeInterestDue = 0;
                    if (!scheduleInputs.IsServiceFeeFinanced)
                    {
                        if (scheduleInputs.ApplyServiceFeeInterest == 1 || scheduleInputs.ApplyServiceFeeInterest == 3)
                        {
                            if (string.IsNullOrEmpty(scheduleInputs.InputRecords[i].InterestRate))
                            {
                                sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, principalServiceFee, true,true);");
                                accruedServiceFeeInterest = PeriodicInterestAmount.CalculateInterestWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, principalServiceFee, true, true, false);
                            }
                            else
                            {
                                sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.InputRecords[i].InterestRate), scheduleInputs.InputRecords[0].DateIn, principalServiceFee, true,true);");
                                accruedServiceFeeInterest = PeriodicInterestAmount.CalculateInterestWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.InputRecords[i].InterestRate), scheduleInputs.InputRecords[0].DateIn, principalServiceFee, true, true, false);
                            }
                            accruedServiceFeeInterest = Round.RoundOffAmount(accruedServiceFeeInterest);
                        }
                    }

                    //Add earned service fee interest value to the accrued value.
                    accruedServiceFeeInterest += (i == 1 ? scheduleInputs.EarnedServiceFeeInterest : 0);
                    serviceFeeInterestDue = accruedServiceFeeInterest + serviceFeeInterestCarryOver;

                    if (scheduleInputs.ServiceFeeFirstPayment)
                    {
                        payableServiceFeeInterest = accruedServiceFeeInterest;
                        payableServiceFee = principalServiceFee;
                        totalServiceFeePayable = payableServiceFeeInterest + payableServiceFee;
                    }
                    else if (i != scheduleInputs.InputRecords.Count - 1)
                    {
                        if ((accruedServiceFeeInterest + serviceFeeInterestCarryOver) >= totalServiceFeePayable)
                        {
                            if (i == 1 && scheduleInputs.MinDuration > Models.Constants.MinDurationValue)
                            {
                                serviceFeeInterestCarryOver = 0;
                                payableServiceFeeInterest = accruedServiceFeeInterest;
                                totalServiceFeePayable = accruedServiceFeeInterest;
                            }
                            else
                            {
                                serviceFeeInterestCarryOver = Round.RoundOffAmount(accruedServiceFeeInterest + serviceFeeInterestCarryOver - totalServiceFeePayable);
                                payableServiceFeeInterest = totalServiceFeePayable;
                            }
                            payableServiceFee = 0;
                        }
                        else
                        {
                            payableServiceFeeInterest = accruedServiceFeeInterest + serviceFeeInterestCarryOver;
                            if ((totalServiceFeePayable - (accruedServiceFeeInterest + serviceFeeInterestCarryOver)) > principalServiceFee)
                            {
                                payableServiceFee = principalServiceFee;
                                totalServiceFeePayable = payableServiceFee + payableServiceFeeInterest;
                            }
                            else
                            {
                                payableServiceFee = totalServiceFeePayable - (accruedServiceFeeInterest + serviceFeeInterestCarryOver);
                            }
                            serviceFeeInterestCarryOver = 0;
                            payableServiceFee = Round.RoundOffAmount(payableServiceFee);
                        }
                    }
                    else
                    {
                        payableServiceFee = principalServiceFee;
                        payableServiceFeeInterest = accruedServiceFeeInterest + serviceFeeInterestCarryOver;
                        totalServiceFeePayable = payableServiceFee + payableServiceFeeInterest;
                        serviceFeeInterestCarryOver = 0;
                    }
                    #endregion

                    //This variable calculates the total amount payable for the particular period.
                    if (i == 1)
                    {
                        totalAmountToPay += sameDayFee + originationFee;
                    }

                    totalAmountToPay += totalServiceFeePayable + Round.RoundOffAmount((scheduleInputs.EarnedOriginationFee + scheduleInputs.EarnedSameDayFee +
                                                                scheduleInputs.EarnedManagementFee + scheduleInputs.EarnedMaintenanceFee +
                                                                scheduleInputs.EarnedNSFFee + scheduleInputs.EarnedLateFee) / (scheduleInputs.InputRecords.Count - 1));

                totalAmountToPay = Round.RoundOffAmount(totalAmountToPay);

                    //This variable calculates the daily interest rate for the period.
                    sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : DailyInterestRate.CalculateDailyInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InterestDelay,scheduleInputs,true);");
                    double dailyInterestRate = DailyInterestRate.CalculateDailyInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, periodicInterestRate, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InterestDelay, scheduleInputs, true);

                    #region Create default schedule for the period method.

                    scheduleOutPut.Schedule.Add(new OutputGrid
                    {
                        PaymentDate = endDate,
                        BeginningPrincipal = Round.RoundOffAmount(principal),
                        BeginningServiceFee = Round.RoundOffAmount(principalServiceFee),
                        PeriodicInterestRate = periodicInterestRate,
                        DailyInterestRate = dailyInterestRate,
                        DailyInterestAmount = dailyInterestAmount,
                        PaymentID = Convert.ToInt32(scheduleInputs.InputRecords[i].PaymentID.ToString()),
                        Flags = Convert.ToInt32(scheduleInputs.InputRecords[i].Flags.ToString()),
                        DueDate = scheduleInputs.InputRecords[i].DateIn,
                        InterestAccrued = interestAccrued,
                        InterestDue = interestDue,
                        InterestCarryOver = interestCarryOver,
                        InterestPayment = interestPayment,
                        PrincipalPayment = principalAmountToPay,
                        PaymentDue = totalAmountToPay,
                        TotalPayment = totalAmountToPay,
                        AccruedServiceFeeInterest = accruedServiceFeeInterest,
                        ServiceFeeInterestDue = serviceFeeInterestDue,
                        AccruedServiceFeeInterestCarryOver = serviceFeeInterestCarryOver,
                        ServiceFee = payableServiceFee,
                        ServiceFeeInterest = payableServiceFeeInterest,
                        ServiceFeeTotal = totalServiceFeePayable,
                        OriginationFee = (i == 1 ? originationFee + scheduleInputs.EarnedOriginationFee : 0),
                        MaintenanceFee = (i == 1 ? scheduleInputs.EarnedMaintenanceFee : 0),
                        ManagementFee = (i == 1 ? scheduleInputs.EarnedManagementFee : 0),
                        SameDayFee = (i == 1 ? sameDayFee + scheduleInputs.EarnedSameDayFee : 0),
                        NSFFee = (i == 1 ? scheduleInputs.EarnedNSFFee : 0),
                        LateFee = (i == 1 ? scheduleInputs.EarnedLateFee : 0),
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
                        CumulativeTotalPastDue = 0
                    });

                    #endregion

                    principalServiceFee = (principalServiceFee - payableServiceFee) <= 0 ? 0 : (principalServiceFee - payableServiceFee);
                    principalServiceFee = Round.RoundOffAmount(principalServiceFee);

                    //Reduce the principal amount that remains to pay after a particular period payment.
                    principal -= principalAmountToPay;
                    principal = Round.RoundOffAmount(principal);
                }

                //Payment Allocation
                sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : ActualPaymentAllocation.AllocationOfFunds(scheduleOutPut.Schedule, scheduleInputs, defaultTotalAmountToPay, defaultTotalServiceFeePayable, scheduleOutPut.LoanAmountCalc, scheduleOutPut, originationFee + + scheduleInputs.EarnedOriginationFee, sameDayFee + scheduleInputs.EarnedSameDayFee); ");
                ActualPaymentAllocation.AllocationOfFunds(scheduleOutPut.Schedule, scheduleInputs, defaultTotalAmountToPay, defaultTotalServiceFeePayable, scheduleOutPut.LoanAmountCalc, scheduleOutPut, originationFee + scheduleInputs.EarnedOriginationFee, sameDayFee + scheduleInputs.EarnedSameDayFee);

                //Calculate all the cumulative columns for the period method.
                sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : SumCumulativeAmount(scheduleOutPut.Schedule);");
                SumCumulativeAmount(scheduleOutPut.Schedule);

                //Calculate the cost of financing of a loan
                double principalAmount = (scheduleInputs.LoanAmount > 0 && scheduleInputs.AmountFinanced == 0) ? scheduleInputs.LoanAmount : scheduleInputs.AmountFinanced;
                sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : CostOfFinance.CalculateCostOfFinance(scheduleInputs, scheduleOutPut.Schedule, scheduleOutPut.OriginationFee, principalAmount);");
                scheduleOutPut.CostOfFinancing = CostOfFinance.CalculateCostOfFinance(scheduleInputs, scheduleOutPut.Schedule, scheduleOutPut.OriginationFee, principalAmount);

                #region -------Accrued Service Fee Interest Calculation----------------

                if (DateAndTime.DateDiff(DateInterval.Day, scheduleInputs.AccruedServiceFeeInterestDate, scheduleInputs.InputRecords[0].DateIn, FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1) >= 0)
                {
                    scheduleOutPut.AccruedServiceFeeInterest = 0;
                }
                else
                {
                    sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : ServiceFeeAndServiceFeeInterestPayable.GetAccruedServiceFeeInterest(scheduleOutPut.Schedule, scheduleInputs.AccruedServiceFeeInterestDate, scheduleInputs.InputRecords[0].DateIn, scheduleInputs);");
                    scheduleOutPut.AccruedServiceFeeInterest = ServiceFeeAndServiceFeeInterestPayable.GetAccruedServiceFeeInterest(scheduleOutPut.Schedule, scheduleInputs.AccruedServiceFeeInterestDate,
                        scheduleInputs.InputRecords[0].DateIn, scheduleInputs);
                }

                scheduleOutPut.AccruedServiceFeeInterest = Round.RoundOffAmount(scheduleOutPut.AccruedServiceFeeInterest);
                #endregion //-------------------------------------------

                #region -------Interest Accrued Calculation----------------

                if (DateAndTime.DateDiff(DateInterval.Day, scheduleInputs.AccruedInterestDate, scheduleInputs.InputRecords[0].DateIn, FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1) >= 0)
                {
                    scheduleOutPut.AccruedInterest = 0;
                    scheduleOutPut.AccruedPrincipal = 0;
                }
                else
                {
                    sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : ServiceFeeAndServiceFeeInterestPayable.GetInterestAccrued(scheduleOutPut.Schedule, scheduleInputs.AccruedInterestDate, scheduleInputs.InputRecords[0].DateIn, scheduleInputs);");
                    scheduleOutPut.AccruedInterest = ServiceFeeAndServiceFeeInterestPayable.GetInterestAccrued(scheduleOutPut.Schedule, scheduleInputs.AccruedInterestDate,
                        scheduleInputs.InputRecords[0].DateIn, scheduleInputs);

                    sbTracing.AppendLine("Inside CreateScheduleForPeriodMethod class, and DefaultSchedule() method. Calling method : ServiceFeeAndServiceFeeInterestPayable.GetAccruedPrincipal(scheduleOutPut.Schedule, scheduleInputs.AccruedInterestDate, scheduleInputs.InputRecords[0].DateIn, scheduleInputs, scheduleOutPut.LoanAmountCalc);");
                    scheduleOutPut.AccruedPrincipal = ServiceFeeAndServiceFeeInterestPayable.GetAccruedPrincipal(scheduleOutPut.Schedule, scheduleInputs.AccruedInterestDate,
                        scheduleInputs.InputRecords[0].DateIn, scheduleInputs, scheduleOutPut.LoanAmountCalc);
                }

                scheduleOutPut.AccruedInterest = Round.RoundOffAmount(scheduleOutPut.AccruedInterest);
                scheduleOutPut.AccruedPrincipal = Round.RoundOffAmount(scheduleOutPut.AccruedPrincipal);
                #endregion //-------------------------------------------

                sbTracing.AppendLine("Exist:From PeriodBusiness Class CreateScheduleForPeriodMethod and Method Name DefaultSchedule(LoanDetails scheduleInputs)");
                return scheduleOutPut;
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
                return scheduleOutPut;
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }

        /// <summary>
        /// Calculate all the cumulative columns values for the period method.
        /// </summary>
        /// <param name="outputGrid"></param>
        private static void SumCumulativeAmount(List<OutputGrid> outputGrid)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside PeriodBusiness Class CreateScheduleForPeriodMethod and Method Name SumCumulativeAmount(List<OutputGrid> outputGrid).");
                for (int i = 0; i < outputGrid.Count; i++)
                {
                    //Round the to be paid amounts columns to two digit decimal point.
                    outputGrid[i].BeginningPrincipal = Round.RoundOffAmount(outputGrid[i].BeginningPrincipal);
                    outputGrid[i].BeginningServiceFee = Round.RoundOffAmount(outputGrid[i].BeginningServiceFee);
                    outputGrid[i].PrincipalServiceFeePayment = Round.RoundOffAmount(outputGrid[i].PrincipalPayment + outputGrid[i].ServiceFee);
                    outputGrid[i].InterestServiceFeeInterestPayment = Round.RoundOffAmount(outputGrid[i].InterestPayment + outputGrid[i].ServiceFeeInterest);
                    outputGrid[i].BeginningPrincipalServiceFee = Round.RoundOffAmount(outputGrid[i].BeginningPrincipal + outputGrid[i].BeginningServiceFee);
                    outputGrid[i].DailyInterestAmount = Round.RoundOffAmount(outputGrid[i].DailyInterestAmount);
                    outputGrid[i].InterestAccrued = Round.RoundOffAmount(outputGrid[i].InterestAccrued);
                    outputGrid[i].InterestCarryOver = Round.RoundOffAmount(outputGrid[i].InterestCarryOver);
                    outputGrid[i].InterestPayment = Round.RoundOffAmount(outputGrid[i].InterestPayment);
                    outputGrid[i].PrincipalPayment = Round.RoundOffAmount(outputGrid[i].PrincipalPayment);
                    outputGrid[i].TotalPayment = Round.RoundOffAmount(outputGrid[i].TotalPayment);
                    outputGrid[i].PaymentDue = Round.RoundOffAmount(outputGrid[i].PaymentDue);
                    outputGrid[i].AccruedServiceFeeInterest = Round.RoundOffAmount(outputGrid[i].AccruedServiceFeeInterest);
                    outputGrid[i].AccruedServiceFeeInterestCarryOver = Round.RoundOffAmount(outputGrid[i].AccruedServiceFeeInterestCarryOver);
                    outputGrid[i].ServiceFeeInterest = Round.RoundOffAmount(outputGrid[i].ServiceFeeInterest);
                    outputGrid[i].ServiceFeeTotal = Round.RoundOffAmount(outputGrid[i].ServiceFeeTotal);
                    outputGrid[i].OriginationFee = Round.RoundOffAmount(outputGrid[i].OriginationFee);
                    outputGrid[i].MaintenanceFee = Round.RoundOffAmount(outputGrid[i].MaintenanceFee);
                    outputGrid[i].ManagementFee = Round.RoundOffAmount(outputGrid[i].ManagementFee);
                    outputGrid[i].SameDayFee = Round.RoundOffAmount(outputGrid[i].SameDayFee);
                    outputGrid[i].NSFFee = Round.RoundOffAmount(outputGrid[i].NSFFee);
                    outputGrid[i].LateFee = Round.RoundOffAmount(outputGrid[i].LateFee);

                    //Round the paid amounts to two digit decimal point.
                    outputGrid[i].PrincipalPaid = Round.RoundOffAmount(outputGrid[i].PrincipalPaid);
                    outputGrid[i].InterestPaid = Round.RoundOffAmount(outputGrid[i].InterestPaid);
                    outputGrid[i].ServiceFeePaid = Round.RoundOffAmount(outputGrid[i].ServiceFeePaid);
                    outputGrid[i].ServiceFeeInterestPaid = Round.RoundOffAmount(outputGrid[i].ServiceFeeInterestPaid);
                    outputGrid[i].OriginationFeePaid = Round.RoundOffAmount(outputGrid[i].OriginationFeePaid);
                    outputGrid[i].MaintenanceFeePaid = Round.RoundOffAmount(outputGrid[i].MaintenanceFeePaid);
                    outputGrid[i].ManagementFeePaid = Round.RoundOffAmount(outputGrid[i].ManagementFeePaid);
                    outputGrid[i].SameDayFeePaid = Round.RoundOffAmount(outputGrid[i].SameDayFeePaid);
                    outputGrid[i].NSFFeePaid = Round.RoundOffAmount(outputGrid[i].NSFFeePaid);
                    outputGrid[i].LateFeePaid = Round.RoundOffAmount(outputGrid[i].LateFeePaid);
                    outputGrid[i].TotalPaid = Round.RoundOffAmount(outputGrid[i].TotalPaid);

                    //Cumulative amount paid is added in current payment row.
                    outputGrid[i].CumulativePrincipal = Round.RoundOffAmount(((i == 0 ? 0 : outputGrid[i - 1].CumulativePrincipal) + outputGrid[i].PrincipalPaid));
                    outputGrid[i].CumulativeInterest = Round.RoundOffAmount(((i == 0 ? 0 : outputGrid[i - 1].CumulativeInterest) + outputGrid[i].InterestPaid));
                    outputGrid[i].CumulativePayment = Round.RoundOffAmount(((i == 0 ? 0 : outputGrid[i - 1].CumulativePayment) + outputGrid[i].TotalPaid));
                    outputGrid[i].CumulativeServiceFee = Round.RoundOffAmount(((i == 0 ? 0 : outputGrid[i - 1].CumulativeServiceFee) + outputGrid[i].ServiceFeePaid));
                    outputGrid[i].CumulativeServiceFeeInterest = Round.RoundOffAmount(((i == 0 ? 0 : outputGrid[i - 1].CumulativeServiceFeeInterest) + outputGrid[i].ServiceFeeInterestPaid));
                    outputGrid[i].CumulativeServiceFeeTotal = Round.RoundOffAmount(((i == 0 ? 0 : outputGrid[i - 1].CumulativeServiceFeeTotal) + outputGrid[i].ServiceFeePaid + outputGrid[i].ServiceFeeInterestPaid));
                    outputGrid[i].CumulativeOriginationFee = Round.RoundOffAmount(((i == 0 ? 0 : outputGrid[i - 1].CumulativeOriginationFee) + outputGrid[i].OriginationFeePaid));
                    outputGrid[i].CumulativeMaintenanceFee = Round.RoundOffAmount(((i == 0 ? 0 : outputGrid[i - 1].CumulativeMaintenanceFee) + outputGrid[i].MaintenanceFeePaid));
                    outputGrid[i].CumulativeManagementFee = Round.RoundOffAmount(((i == 0 ? 0 : outputGrid[i - 1].CumulativeManagementFee) + outputGrid[i].ManagementFeePaid));
                    outputGrid[i].CumulativeSameDayFee = Round.RoundOffAmount(((i == 0 ? 0 : outputGrid[i - 1].CumulativeSameDayFee) + outputGrid[i].SameDayFeePaid));
                    outputGrid[i].CumulativeNSFFee = Round.RoundOffAmount(((i == 0 ? 0 : outputGrid[i - 1].CumulativeNSFFee) + outputGrid[i].NSFFeePaid));
                    outputGrid[i].CumulativeLateFee = Round.RoundOffAmount(((i == 0 ? 0 : outputGrid[i - 1].CumulativeLateFee) + outputGrid[i].LateFeePaid));
                    outputGrid[i].CumulativeTotalFees = Round.RoundOffAmount(((i == 0 ? 0 : outputGrid[i - 1].CumulativeTotalFees) +
                                    outputGrid[i].ServiceFeePaid + outputGrid[i].ServiceFeeInterestPaid + outputGrid[i].OriginationFeePaid +
                                    outputGrid[i].MaintenanceFeePaid + outputGrid[i].ManagementFeePaid + outputGrid[i].SameDayFeePaid +
                                    outputGrid[i].NSFFeePaid + outputGrid[i].LateFeePaid));

                    //Round the past due amounts to two digit decimal point.
                    outputGrid[i].PrincipalPastDue = Round.RoundOffAmount(outputGrid[i].PrincipalPastDue);
                    outputGrid[i].InterestPastDue = Round.RoundOffAmount(outputGrid[i].InterestPastDue);
                    outputGrid[i].ServiceFeePastDue = Round.RoundOffAmount(outputGrid[i].ServiceFeePastDue);
                    outputGrid[i].ServiceFeeInterestPastDue = Round.RoundOffAmount(outputGrid[i].ServiceFeeInterestPastDue);
                    outputGrid[i].OriginationFeePastDue = Round.RoundOffAmount(outputGrid[i].OriginationFeePastDue);
                    outputGrid[i].SameDayFeePastDue = Round.RoundOffAmount(outputGrid[i].SameDayFeePastDue);
                    outputGrid[i].ManagementFeePastDue = Round.RoundOffAmount(outputGrid[i].ManagementFeePastDue);
                    outputGrid[i].MaintenanceFeePastDue = Round.RoundOffAmount(outputGrid[i].MaintenanceFeePastDue);
                    outputGrid[i].NSFFeePastDue = Round.RoundOffAmount(outputGrid[i].NSFFeePastDue);
                    outputGrid[i].LateFeePastDue = Round.RoundOffAmount(outputGrid[i].LateFeePastDue);
                    outputGrid[i].TotalPastDue = Round.RoundOffAmount(outputGrid[i].TotalPastDue);

                    //Cumulative amount past due is added in current past due row
                    outputGrid[i].CumulativePrincipalPastDue = outputGrid[i].PrincipalPastDue;
                    outputGrid[i].CumulativeInterestPastDue = outputGrid[i].InterestPastDue;
                    outputGrid[i].CumulativeServiceFeePastDue = outputGrid[i].ServiceFeePastDue;
                    outputGrid[i].CumulativeServiceFeeInterestPastDue = outputGrid[i].ServiceFeeInterestPastDue;
                    outputGrid[i].CumulativeOriginationFeePastDue = outputGrid[i].OriginationFeePastDue;
                    outputGrid[i].CumulativeMaintenanceFeePastDue = outputGrid[i].MaintenanceFeePastDue;
                    outputGrid[i].CumulativeManagementFeePastDue = outputGrid[i].ManagementFeePastDue;
                    outputGrid[i].CumulativeSameDayFeePastDue = outputGrid[i].SameDayFeePastDue;
                    outputGrid[i].CumulativeNSFFeePastDue = outputGrid[i].NSFFeePastDue;
                    outputGrid[i].CumulativeLateFeePastDue = outputGrid[i].LateFeePastDue;
                    outputGrid[i].CumulativeTotalPastDue = outputGrid[i].TotalPastDue;
                }
                sbTracing.AppendLine("Exist:From PeriodBusiness Class CreateScheduleForPeriodMethod and Method Name SumCumulativeAmount(List<OutputGrid> outputGrid)");
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
