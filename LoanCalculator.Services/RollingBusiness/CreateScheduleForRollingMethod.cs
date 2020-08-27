using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoanAmort_driver.Models;
using Microsoft.VisualBasic;

namespace LoanAmort_driver.RollingBusiness
{
    public class CreateScheduleForRollingMethod
    {
        /// <summary>
        /// Name : Punit Singh
        /// Date : 21/06/2017
        /// This function will create the reschedule payment for rolling method as output. Here I consider Input grid for schedule dates as well as Payment dates and
        /// payment amount. This function takes the input grid values i.e. schedule dates and payment details, and some other inputs provided on top of screen.
        /// </summary>   
        /// <param name="scheduleInputs"></param>
        /// <returns></returns>
        public static getScheduleOutput DefaultSchedule(getScheduleInput scheduleInputs)
        {
            var scheduleOutPut = new getScheduleOutput();
            StringBuilder sbTracing = new StringBuilder();
            int managementFeeTableIndex = 0;
            int maintenanceFeeTableIndex = 0;

            try
            {
                sbTracing.AppendLine("Enter:Inside RollingBusiness,Class CreateScheduleForRollingMethod and Method Name DefaultSchedule(LoanDetails scheduleInputs).");
                scheduleOutPut.Schedule = new List<PaymentDetail>();
                scheduleOutPut.ManagementFeeAssessment = new List<ManagementFeeTable>();
                scheduleOutPut.MaintenanceFeeAssessment = new List<MaintenanceFeeTable>();
                double principal = 0;

                //Checke whether loan amount input is provided and amount financed input is 0.
                if (scheduleInputs.LoanAmount > 0 && scheduleInputs.AmountFinanced == 0)
                {
                    //Select loan amount input value as a principal amount.
                    principal = scheduleInputs.LoanAmount;
                }
                else
                {
                    //Otherwise select Amount financed input value as a principal amount.
                    principal = scheduleInputs.AmountFinanced;
                }

                //Calculate the principal service fee to be paid.
                sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling Method : principalSerFee.CalculatePrincipalServiceeFee(scheduleInputs.ApplyServiceFeeInterest, principal, scheduleInputs.ServiceFee);");
                double principalServiceFee = PrincipalServiceFee.CalculatePrincipalServiceeFee(scheduleInputs.ApplyServiceFeeInterest, principal, scheduleInputs.ServiceFee);

                //Calculate the Oigination Fee Calc output value.
                sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : orginationFeeForFirstPayment.GetOriginationFeeForFirstPayment(scheduleInputs.OriginationFee, scheduleInputs.OriginationFeePercent, " +
                                    "scheduleInputs.OriginationFeeMax, scheduleInputs.OriginationFeeMin, scheduleInputs.OriginationFeeCalculationMethod, principal)");
                scheduleOutPut.OriginationFee = OriginationFeeForFirstPayment.GetOriginationFeeForFirstPayment(scheduleInputs.OriginationFee, scheduleInputs.OriginationFeePercent, scheduleInputs.OriginationFeeMax,
                                                        scheduleInputs.OriginationFeeMin, scheduleInputs.OriginationFeeCalculationMethod, principal);

                //Determine the origination fee to be paid in the first payment of repayment schedule.
                double originationFee = scheduleInputs.IsOriginationFeeFinanced ? 0 : scheduleOutPut.OriginationFee;

                //Calculate the same day fee to be paid.
                sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : sameDayFeeForFirstPayment.GetSameDayFeeForFirstPayment(scheduleInputs.SameDayFee, scheduleInputs.SameDayFeeCalculationMethod, principal);");
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

                //Determine the principal service fee to be paid in the repayment schedule if it is not financed.
                principalServiceFee = scheduleInputs.IsServiceFeeFinanced ? 0 : Round.RoundOffAmount(principalServiceFee);
                //Calculate the same day fee to be paid in the first payment of repayment schedule.
                sameDayFee = scheduleInputs.IsSameDayFeeFinanced ? 0 : Round.RoundOffAmount(sameDayFee);
                principal = scheduleOutPut.LoanAmountCalc;

                //Default end date.
                DateTime endDate = Convert.ToDateTime(Models.Constants.DefaultDate);

                #region Calculate Amount Financed

                sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : AmountFinanced.GetAmountFinanced(scheduleInputs.LoanAmount, scheduleInputs.AmountFinanced, " +
                        "scheduleOutPut.OriginationFee, scheduleInputs.IsOriginationFeeFinanced, scheduleInputs.SameDayFee, scheduleInputs.SameDayFeeCalculationMethod, scheduleInputs.IsSameDayFeeFinanced, " +
                        "scheduleInputs.ServiceFee, scheduleInputs.ApplyServiceFeeInterest, scheduleInputs.IsServiceFeeFinanced);");
                scheduleOutPut.AmountFinanced = AmountFinanced.GetAmountFinanced(scheduleInputs.LoanAmount, scheduleInputs.AmountFinanced, scheduleOutPut.OriginationFee,
                        scheduleInputs.IsOriginationFeeFinanced, scheduleInputs.SameDayFee, scheduleInputs.SameDayFeeCalculationMethod,
                        scheduleInputs.IsSameDayFeeFinanced, scheduleInputs.ServiceFee, scheduleInputs.ApplyServiceFeeInterest, scheduleInputs.IsServiceFeeFinanced);

                #endregion

                //This variable calculates the total amount to pay in the particular period. It contains both principal and interest amount.
                sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : CalculateTotalAmount.TotalPayableAmount(scheduleInputs, scheduleOutPut.LoanAmountCalc);");
                double defaultTotalAmountToPay = CalculateTotalAmount.TotalPayableAmount(scheduleInputs, scheduleOutPut.LoanAmountCalc);
                defaultTotalAmountToPay = Round.RoundOffAmount(defaultTotalAmountToPay);

                //This function calculates the total service fee i.e. equal for each period, to be paid.
                sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : ServiceFeeAndServiceFeeInterestPayable.CalculateServiceFeeAndInterestPayable(principalServiceFee, scheduleInputs, scheduleOutPut.LoanAmountCalc);");
                double defaultTotalServiceFeePayable = ServiceFeeAndServiceFeeInterestPayable.CalculateServiceFeeAndInterestPayable(principalServiceFee, scheduleInputs, scheduleOutPut.LoanAmountCalc);
                defaultTotalServiceFeePayable = Round.RoundOffAmount(defaultTotalServiceFeePayable);
                if (scheduleInputs.ServiceFeeFirstPayment == true)
                {
                    scheduleOutPut.RegularPayment = defaultTotalAmountToPay;
                }
                else
                {
                    scheduleOutPut.RegularPayment = defaultTotalAmountToPay + defaultTotalServiceFeePayable;
                }
                #region Calculate Management and Maintenance Fee

                //This variable contains all the scheduled payment dates i.e. if actual payment date is given, then it will consider that date.
                List<DateTime> scheduledPayments = new List<DateTime>();
                foreach (var row in scheduleInputs.InputRecords)
                {
                    if (row.EffectiveDate != DateTime.MinValue)
                    {
                        scheduledPayments.Add(row.EffectiveDate);
                    }
                    else
                    {
                        scheduledPayments.Add(row.DateIn);
                    }
                }
                //This variable conatins the additional payment dates excluding the discounts, NSF, and Late fee events.
                List<AdditionalPaymentRecord> allPayments = scheduleInputs.AdditionalPaymentRecords.Where(o => o.Flags != (int)Models.Constants.FlagValues.Discount &&
                                                                    o.Flags != (int)Models.Constants.FlagValues.NSFFee && o.Flags != (int)Models.Constants.FlagValues.LateFee &&
                                                                    o.Flags != (int)Models.Constants.FlagValues.MaintenanceFee && o.Flags != (int)Models.Constants.FlagValues.ManagementFee).ToList();
                foreach (var payments in scheduledPayments)
                {
                    allPayments.Add(new AdditionalPaymentRecord { DateIn = payments, Flags = (int)Models.Constants.FlagValues.Payment });
                }
                allPayments = allPayments.OrderBy(o => o.DateIn).ThenBy(p => p.Flags).ToList();

                List<AdditionalPaymentRecord> mgmtAllPayments = allPayments.ToList();
                if ((scheduleInputs.ManagementFeeFrequency == (int)Models.Constants.FeeFrequency.Days || scheduleInputs.ManagementFeeFrequency == (int)Models.Constants.FeeFrequency.Months) &&
                        scheduleInputs.AdditionalPaymentRecords.Count > 0 &&
                        ((scheduleInputs.InputRecords[scheduleInputs.InputRecords.Count - 1].EffectiveDate == DateTime.MinValue ? scheduleInputs.InputRecords[scheduleInputs.InputRecords.Count - 1].DateIn : scheduleInputs.InputRecords[scheduleInputs.InputRecords.Count - 1].EffectiveDate) < scheduleInputs.EarlyPayoffDate &&
                        scheduleInputs.EarlyPayoffDate <= scheduleInputs.AdditionalPaymentRecords[scheduleInputs.AdditionalPaymentRecords.Count - 1].DateIn &&
                        scheduleInputs.AdditionalPaymentRecords.FindIndex(o => o.DateIn == scheduleInputs.AdditionalPaymentRecords[scheduleInputs.AdditionalPaymentRecords.Count - 1].DateIn &&
                            (o.Flags == (int)Models.Constants.FlagValues.PrincipalOnly || o.Flags == (int)Models.Constants.FlagValues.AdditionalPayment)) == -1))
                {
                    mgmtAllPayments.Add(new AdditionalPaymentRecord { DateIn = scheduleInputs.AdditionalPaymentRecords[scheduleInputs.AdditionalPaymentRecords.Count - 1].DateIn, Flags = -1 });
                }

                //Checks whether there is a value either in management fee fixed or percent input field.
                if (scheduleInputs.ManagementFee != 0 || scheduleInputs.ManagementFeePercent != 0)
                {
                    sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : ManagementAndMaintenanceFeeCalc.EffectiveDateOfFee(scheduleInputs.ManagementFeeFrequency, scheduleInputs.ManagementFeeDelay, scheduleInputs.InputRecords[0].DateIn, scheduledPayments, mgmtAllPayments);");
                    scheduleOutPut.ManagementFeeEffective = ManagementAndMaintenanceFeeCalc.EffectiveDateOfFee(scheduleInputs.ManagementFeeFrequency, scheduleInputs.ManagementFeeDelay, scheduleInputs.InputRecords[0].DateIn, scheduledPayments, mgmtAllPayments);
                    sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : ManagementAndMaintenanceFeeCalc.CreateManagementFeeTable(scheduleOutPut.ManagementFeeEffective, scheduleInputs.ManagementFeeFrequency, scheduleInputs.ManagementFeeFrequencyNumber, scheduledPayments, mgmtAllPayments, scheduleInputs.ManagementFeeDelay);");
                    scheduleOutPut.ManagementFeeAssessment = ManagementAndMaintenanceFeeCalc.CreateManagementFeeTable(scheduleOutPut.ManagementFeeEffective, scheduleInputs.ManagementFeeFrequency, scheduleInputs.ManagementFeeFrequencyNumber, scheduledPayments, mgmtAllPayments, scheduleInputs.ManagementFeeDelay);

                    //Checks whether fee basis is "Payment", or neither basis is provided nor percent value.
                    if (scheduleInputs.ManagementFeeBasis == (int)Models.Constants.FeeBasis.ActualPayment ||
                        scheduleInputs.ManagementFeeBasis == (int)Models.Constants.FeeBasis.ScheduledPayment ||
                        (scheduleInputs.ManagementFeeBasis == -1 && scheduleInputs.ManagementFeePercent == 0))
                    {
                        managementFeeTableIndex = 0;
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
                                                o.Flags != (int)Models.Constants.FlagValues.MaintenanceFee && o.Flags != (int)Models.Constants.FlagValues.ManagementFee &&
                                                o.PaymentID == Convert.ToInt32(scheduleOutPut.ManagementFeeAssessment[managementFeeTableIndex].PaymentId)).Sum(o => o.AdditionalPayment);
                            }

                            sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateFee(scheduleInputs.ManagementFee, " +
                                                                "scheduleInputs.ManagementFeePercent, scheduleInputs.IsManagementFeeGreater, scheduleInputs.ManagementFeeMaxPer, scheduleInputs.ManagementFeeMin, amount); ");
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
                    allPayments.Add(new AdditionalPaymentRecord { DateIn = scheduleInputs.AdditionalPaymentRecords[scheduleInputs.AdditionalPaymentRecords.Count - 1].DateIn, Flags = -1 });
                }

                //Checks whether there is a value either in maintenance fee fixed or percent input field.
                if (scheduleInputs.MaintenanceFee != 0 || scheduleInputs.MaintenanceFeePercent != 0)
                {
                    sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : ManagementAndMaintenanceFeeCalc.EffectiveDateOfFee(scheduleInputs.MaintenanceFeeFrequency, scheduleInputs.MaintenanceFeeDelay, scheduleInputs.InputRecords[0].DateIn, scheduledPayments, allPayments);");
                    scheduleOutPut.MaintenanceFeeEffective = ManagementAndMaintenanceFeeCalc.EffectiveDateOfFee(scheduleInputs.MaintenanceFeeFrequency, scheduleInputs.MaintenanceFeeDelay, scheduleInputs.InputRecords[0].DateIn, scheduledPayments, allPayments);
                    sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : ManagementAndMaintenanceFeeCalc.CreateMaintenanceFeeTable(scheduleOutPut.MaintenanceFeeEffective, scheduleInputs.MaintenanceFeeFrequency, scheduleInputs.MaintenanceFeeFrequencyNumber, scheduledPayments, allPayments, scheduleInputs.MaintenanceFeeDelay);");
                    scheduleOutPut.MaintenanceFeeAssessment = ManagementAndMaintenanceFeeCalc.CreateMaintenanceFeeTable(scheduleOutPut.MaintenanceFeeEffective, scheduleInputs.MaintenanceFeeFrequency, scheduleInputs.MaintenanceFeeFrequencyNumber, scheduledPayments, allPayments, scheduleInputs.MaintenanceFeeDelay);

                    //Checks whether fee basis is "Payment", or neither basis is provided nor percent value.
                    if (scheduleInputs.MaintenanceFeeBasis == (int)Models.Constants.FeeBasis.ActualPayment ||
                        scheduleInputs.MaintenanceFeeBasis == (int)Models.Constants.FeeBasis.ScheduledPayment ||
                        (scheduleInputs.MaintenanceFeeBasis == -1 && scheduleInputs.MaintenanceFeePercent == 0))
                    {
                        maintenanceFeeTableIndex = 0;
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
                                                o.Flags != (int)Models.Constants.FlagValues.MaintenanceFee && o.Flags != (int)Models.Constants.FlagValues.ManagementFee &&
                                                o.PaymentID == Convert.ToInt32(scheduleOutPut.MaintenanceFeeAssessment[maintenanceFeeTableIndex].PaymentId)).Sum(o => o.AdditionalPayment);
                            }

                            sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : ManagementAndMaintenanceFeeCalc.CalculateFee(scheduleInputs.MaintenanceFee, " +
                                                                "scheduleInputs.MaintenanceFeePercent, scheduleInputs.IsMaintenanceFeeGreater, scheduleInputs.MaintenanceFeeMaxPer, scheduleInputs.MaintenanceFeeMin, amount); ");
                            scheduleOutPut.MaintenanceFeeAssessment[maintenanceFeeTableIndex].Fee = ManagementAndMaintenanceFeeCalc.CalculateFee(scheduleInputs.MaintenanceFee,
                                                                scheduleInputs.MaintenanceFeePercent, scheduleInputs.IsMaintenanceFeeGreater, scheduleInputs.MaintenanceFeeMaxPer, scheduleInputs.MaintenanceFeeMin, amount);
                            maintenanceFeeTableIndex++;
                        }
                    }
                }

                #endregion

                double interestCarryOver = 0, serviceFeeInterestCarryOver = 0;
                int outputGridRowNumber = 0;
                managementFeeTableIndex = 0;

                for (int i = 1; i < scheduleInputs.InputRecords.Count; i++)
                {
                    double interestPayment = 0;
                    double totalAmountToPay = defaultTotalAmountToPay;
                    double totalServiceFeePayable = defaultTotalServiceFeePayable;

                    //It determines the start date from which interest will be calculated for that period. If it is first scheduled payment, then "DateIn" column in input 
                    //grid will considered as start date, otherwise last end date will be considered.
                    DateTime startDate = i == 1 ? scheduleInputs.InputRecords[i - 1].DateIn : endDate;

                    //It determines the end date. Interest will be calculated till the end date for that period.
                    endDate = scheduleInputs.InputRecords[i].EffectiveDate == DateTime.MinValue ? scheduleInputs.InputRecords[i].DateIn : scheduleInputs.InputRecords[i].EffectiveDate;

                    #region default Periodic Interest Rate and User story 23

                    //This variable calls a function which calculate the periodic interest rate for that period.
                    double periodicInterestRate = 0;
                    double interestAccrued = 0;
                    if (string.IsNullOrEmpty(scheduleInputs.InputRecords[i].InterestRate))
                    {
                        sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : PeriodicInterestRate.GetPeriodicInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, Convert.ToDouble(scheduleInputs.InterestRate));");
                        periodicInterestRate = CalculatePeriodicInterestRate.CalcPeriodicInterestRateWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, principal, true, true, false);
                        interestAccrued = PeriodicInterestAmount.CalcAccruedInterestWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, principal, true, true, false);
                    }
                    else
                    {
                        if (scheduleInputs.InputRecords[i].InterestRate == "0")
                        {
                            double GetServiceFee = PrincipalServiceFee.CalculatePrincipalServiceeFee(scheduleInputs.ApplyServiceFeeInterest, scheduleOutPut.LoanAmountCalc, scheduleInputs.ServiceFee);
                            totalAmountToPay = scheduleOutPut.LoanAmountCalc / (scheduleInputs.InputRecords.Count - 1);
                            totalServiceFeePayable = GetServiceFee / (scheduleInputs.InputRecords.Count - 1);
                        }
                        sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : PeriodicInterestRate.GetPeriodicInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, " +
                            "Convert.ToDouble(scheduleInputs.InputRecords[i].InterestRate));");
                        periodicInterestRate = CalculatePeriodicInterestRate.CalcPeriodicInterestRateWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.InputRecords[i].InterestRate), scheduleInputs.InputRecords[0].DateIn, true, true, false);
                        interestAccrued = PeriodicInterestAmount.CalcAccruedInterestWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.InputRecords[i].InterestRate), scheduleInputs.InputRecords[0].DateIn, principal, true, true, false);
                    }

                    #endregion

                    //This variable determines the principal amount to pay for particular period by subtracting the periodic interest from total amount.
                    double principalAmountToPay = 0;
                    //This variable is the periodic interest of that period. It is multiplication of periodic rate and remaining principal amount.
                    interestAccrued = Round.RoundOffAmount(interestAccrued);


                    //This variable calculates the daily interest rate for the period.
                    sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : DailyInterestRate.CalculateDailyInterestRate(scheduleInputs.DaysInYearBankMethod, " +
                        "scheduleInputs.DaysInMonth, startDate, endDate, periodicInterestRate);");
                    double dailyInterestRate = DailyInterestRate.CalculateDailyInterestRate(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate,
                                                               periodicInterestRate, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InterestDelay, scheduleInputs, true);

                    //This variable calculates the daily interest amount for the period.
                    sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : DailyInterestAmount.CalculateDailyInterestAmount(scheduleInputs.DaysInYearBankMethod, " +
                        "scheduleInputs.DaysInMonth, startDate, endDate, periodicInterestRate * principal);");
                    double dailyInterestAmount = DailyInterestAmount.CalculateDailyInterestAmount(scheduleInputs.DaysInYearBankMethod, scheduleInputs.DaysInMonth, startDate, endDate, interestAccrued, scheduleInputs.InputRecords[0].DateIn, scheduleInputs.InterestDelay, scheduleInputs, true);
                    dailyInterestAmount = Round.RoundOffAmount(dailyInterestAmount);

                    interestAccrued += i == 1 ? scheduleInputs.EarnedInterest : 0;

                    //This condition determine whether the row is last row. If no, the carry over of interest will be added to 
                    //interest accrued for current schedule, and limit to total payment. If it last payment schedule, all remaining principal
                    //amount will be added to total amount to be paid.

                    if (i != scheduleInputs.InputRecords.Count - 1)
                    {
                        if (scheduleInputs.MinDuration > Models.Constants.MinDurationValue && i == 1 && interestAccrued >= totalAmountToPay)
                        {
                            interestCarryOver = 0;
                            interestPayment = interestAccrued;
                            principalAmountToPay = 0;
                        }
                        else if ((interestAccrued + interestCarryOver) >= totalAmountToPay)
                        {
                            interestCarryOver = Round.RoundOffAmount((interestAccrued + interestCarryOver) - totalAmountToPay);
                            interestPayment = totalAmountToPay;
                            principalAmountToPay = 0;
                        }
                        else
                        {
                            interestPayment = (interestAccrued + interestCarryOver);
                            if ((totalAmountToPay - (interestAccrued + interestCarryOver)) > principal)
                            {
                                principalAmountToPay = principal;
                                totalAmountToPay = principalAmountToPay + interestPayment;
                            }
                            else
                            {
                                principalAmountToPay = totalAmountToPay - (interestAccrued + interestCarryOver);
                            }
                            interestCarryOver = 0;
                            principalAmountToPay = Round.RoundOffAmount(principalAmountToPay);
                        }
                        if (i >= scheduleInputs.EnforcedPayment && principalAmountToPay == 0)
                        {
                            double payableEnforcedPrincipal = scheduleInputs.EnforcedPrincipal > principal ? principal : scheduleInputs.EnforcedPrincipal;
                            if (scheduleInputs.MinDuration > Models.Constants.MinDurationValue && i == 1 && interestAccrued >= totalAmountToPay)
                            {
                                principalAmountToPay = payableEnforcedPrincipal;
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
                    }
                    else
                    {
                        principalAmountToPay = principal;
                        interestPayment = (interestAccrued + interestCarryOver);
                        totalAmountToPay = principalAmountToPay + interestPayment;
                        interestCarryOver = 0;
                    }

                    //determine the service fee, service fee interest to be paid in each period, and calculate the remaining principal service fee on which service fee
                    //interest is being calculated.
                    double payableServiceFee = 0;
                    double payableServiceFeeInterest = 0;
                    double accruedServiceFeeInterest = 0;
                    if (!scheduleInputs.IsServiceFeeFinanced)
                    {
                        if (scheduleInputs.ApplyServiceFeeInterest == 1 || scheduleInputs.ApplyServiceFeeInterest == 3)
                        {
                            accruedServiceFeeInterest = string.IsNullOrEmpty(scheduleInputs.InputRecords[i].InterestRate) ? PeriodicInterestAmount.CalcAccruedInterestWithTier(scheduleInputs, startDate, endDate, scheduleInputs.InputRecords[0].DateIn, principalServiceFee, true, true, false) :
                                                        PeriodicInterestAmount.CalcAccruedInterestWithoutTier(scheduleInputs, startDate, endDate, Convert.ToDouble(scheduleInputs.InputRecords[i].InterestRate), scheduleInputs.InputRecords[0].DateIn, principalServiceFee, true, true, false);
                            accruedServiceFeeInterest = Round.RoundOffAmount(accruedServiceFeeInterest);
                        }
                    }
                    accruedServiceFeeInterest += (i == 1 ? scheduleInputs.EarnedServiceFeeInterest : 0);
                    accruedServiceFeeInterest = Round.RoundOffAmount(accruedServiceFeeInterest);

                    if (scheduleInputs.ServiceFeeFirstPayment == true)
                    {
                        payableServiceFeeInterest = accruedServiceFeeInterest;
                        payableServiceFee = principalServiceFee;
                        totalServiceFeePayable = accruedServiceFeeInterest + principalServiceFee;
                        serviceFeeInterestCarryOver = 0;
                    }
                    else if (i != scheduleInputs.InputRecords.Count - 1)
                    {
                        if (scheduleInputs.MinDuration > Models.Constants.MinDurationValue && i == 1 && accruedServiceFeeInterest >= totalServiceFeePayable)
                        {
                            serviceFeeInterestCarryOver = 0;
                            payableServiceFeeInterest = accruedServiceFeeInterest;
                            payableServiceFee = 0;

                        }
                        else if ((accruedServiceFeeInterest + serviceFeeInterestCarryOver) >= totalServiceFeePayable)
                        {
                            serviceFeeInterestCarryOver = Round.RoundOffAmount((accruedServiceFeeInterest + serviceFeeInterestCarryOver) - totalServiceFeePayable);
                            payableServiceFeeInterest = totalServiceFeePayable;
                            payableServiceFee = 0;
                        }
                        else
                        {
                            payableServiceFeeInterest = (accruedServiceFeeInterest + serviceFeeInterestCarryOver);
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
                        payableServiceFeeInterest = (accruedServiceFeeInterest + serviceFeeInterestCarryOver);
                        totalServiceFeePayable = payableServiceFee + payableServiceFeeInterest;
                        serviceFeeInterestCarryOver = 0;
                    }

                    //This variable calculates the total amount payable for the particular period.
                    if (scheduleInputs.MinDuration > 0 && i == 1)
                    {
                        totalAmountToPay = principalAmountToPay + interestPayment;
                        totalServiceFeePayable = payableServiceFee + payableServiceFeeInterest;
                        totalAmountToPay += sameDayFee + originationFee;
                    }
                    else if (i == 1)
                    {
                        totalAmountToPay += sameDayFee + originationFee;
                    }
                    totalAmountToPay += totalServiceFeePayable;

                    #region Create default schedule from the rolling method.

                    scheduleOutPut.Schedule.Add(new PaymentDetail
                    {
                        PaymentDate = endDate,
                        BeginningPrincipal = Round.RoundOffAmount(principal),
                        BeginningServiceFee = Round.RoundOffAmount(principalServiceFee),
                        PeriodicInterestRate = periodicInterestRate,
                        DailyInterestRate = dailyInterestRate,
                        DailyInterestAmount = Round.RoundOffAmount(dailyInterestAmount),
                        PaymentID = Convert.ToInt32(scheduleInputs.InputRecords[i].PaymentID.ToString()),
                        Flags = Convert.ToInt32(scheduleInputs.InputRecords[i].Flags.ToString()),
                        DueDate = scheduleInputs.InputRecords[i].DateIn,
                        InterestAccrued = interestAccrued,
                        InterestCarryOver = interestCarryOver,
                        InterestPayment = interestPayment,
                        PrincipalPayment = principalAmountToPay,
                        TotalPayment = totalAmountToPay,
                        PaymentDue = totalAmountToPay,
                        AccruedServiceFeeInterest = accruedServiceFeeInterest,
                        AccruedServiceFeeInterestCarryOver = serviceFeeInterestCarryOver,
                        ServiceFee = payableServiceFee,
                        ServiceFeeInterest = payableServiceFeeInterest,
                        ServiceFeeTotal = totalServiceFeePayable,
                        OriginationFee = (i == 1 ? originationFee : 0),
                        MaintenanceFee = 0,
                        ManagementFee = 0,
                        SameDayFee = (i == 1 ? sameDayFee : 0),
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
                        BucketStatus = "OutStanding"
                    });

                    #endregion

                    principalServiceFee = (principalServiceFee - payableServiceFee) <= 0 ? 0 : (principalServiceFee - payableServiceFee);
                    principalServiceFee = Round.RoundOffAmount(principalServiceFee);


                    //Reduce the principal amount that remains to pay after a particular period payment.
                    principal -= principalAmountToPay;
                    principal = Round.RoundOffAmount(principal);
                    outputGridRowNumber++;

                }

                //Payment Allocation
                sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : ActualPaymentAllocation.AllocationOfFunds(scheduleOutPut.Schedule, scheduleInputs, " +
                        "defaultTotalAmountToPay, defaultTotalServiceFeePayable, scheduleOutPut.OriginationFee, scheduleOutPut.LoanAmountCalc, sameDayFee);");
                ActualPaymentAllocation.AllocationOfFunds(scheduleOutPut.Schedule, scheduleInputs, defaultTotalAmountToPay, defaultTotalServiceFeePayable, scheduleOutPut);

                //Calculate all the cumulative columns for the rolling method.
                sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : SumCumulativeAmount(scheduleOutPut.Schedule);");
                SumCumulativeAmount(scheduleOutPut.Schedule);

                //Calculate the cost of financing of a loan
                double principalAmount = (scheduleInputs.LoanAmount > 0 && scheduleInputs.AmountFinanced == 0) ? scheduleInputs.LoanAmount : scheduleInputs.AmountFinanced;
                sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : CostOfFinance.CalculateCostOfFinance(scheduleInputs, scheduleOutPut.Schedule, scheduleOutPut.OriginationFee, principalAmount, scheduleInputs.LoanAmount > 0 ? true : false);");
                scheduleOutPut.CostOfFinancing = CostOfFinance.CalculateCostOfFinance(scheduleInputs, scheduleOutPut.Schedule, scheduleOutPut.OriginationFee, principalAmount);

                #region -------Accrued Service Fee Interest Calculation----------------

                if (DateAndTime.DateDiff(DateInterval.Day, scheduleInputs.AccruedServiceFeeInterestDate, scheduleInputs.InputRecords[0].DateIn, FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1) >= 0)
                {
                    scheduleOutPut.AccruedServiceFeeInterest = 0;
                }
                else
                {
                    sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : ServiceFeeAndServiceFeeInterestPayable.GetAccruedServiceFeeInterest(scheduleOutPut.Schedule, " +
                        "scheduleInputs.AccruedServiceFeeInterestDate, scheduleInputs.InputRecords[0].DateIn, scheduleInputs);");
                    scheduleOutPut.AccruedServiceFeeInterest = ServiceFeeAndServiceFeeInterestPayable.GetAccruedServiceFeeInterest(scheduleOutPut.Schedule, scheduleInputs.AccruedServiceFeeInterestDate, scheduleInputs.InputRecords[0].DateIn, scheduleInputs, true);
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
                    sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method : ServiceFeeAndServiceFeeInterestPayable.GetInterestAccrued(scheduleOutPut.Schedule, " +
                        "scheduleInputs.AccruedInterestDate, scheduleInputs.InputRecords[0].DateIn, scheduleInputs);");
                    scheduleOutPut.AccruedInterest = ServiceFeeAndServiceFeeInterestPayable.GetInterestAccrued(scheduleOutPut.Schedule, scheduleInputs.AccruedInterestDate,
                        scheduleInputs.InputRecords[0].DateIn, scheduleInputs, true);

                    sbTracing.AppendLine("Inside CreateScheduleForRollingMethod class, and DefaultSchedule() method. Calling method :ServiceFeeAndServiceFeeInterestPayable.GetInterestAccrued(scheduleOutPut.Schedule, scheduleInputs.AccruedInterestDate," +
                        "scheduleInputs.InputRecords[0].DateIn, scheduleInputs, true); ");
                    scheduleOutPut.AccruedPrincipal = ServiceFeeAndServiceFeeInterestPayable.GetAccruedPrincipal(scheduleOutPut.Schedule, scheduleInputs.AccruedInterestDate,
                        scheduleInputs.InputRecords[0].DateIn, scheduleInputs, scheduleOutPut.LoanAmountCalc, false);
                }

                scheduleOutPut.AccruedInterest = Round.RoundOffAmount(scheduleOutPut.AccruedInterest);
                scheduleOutPut.AccruedPrincipal = Round.RoundOffAmount(scheduleOutPut.AccruedPrincipal);
                #endregion //-------------------------------------------

                sbTracing.AppendLine("Exist:From Business Class CreateScheduleForRollingMethod and Method Name DefaultSchedule(LoanDetails scheduleInputs)");
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
        /// Calculate all the cumulative columns values for the rolling method.
        /// </summary>
        /// <param name="outputGrid"></param>
        private static void SumCumulativeAmount(List<PaymentDetail> outputGrid)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside RollingBusiness Class CreateScheduleForRollingMethod and Method Name SumCumulativeAmount(List<PaymentDetail> outputGrid).");
                for (int i = 0; i < outputGrid.Count; i++)
                {
                    //Round the to be paid amounts columns to two digit decimal point.
                    outputGrid[i].BeginningServiceFee = Round.RoundOffAmount(outputGrid[i].BeginningServiceFee);
                    outputGrid[i].BeginningPrincipal = Round.RoundOffAmount(outputGrid[i].BeginningPrincipal);
                    outputGrid[i].PrincipalServiceFeePayment = Round.RoundOffAmount(outputGrid[i].PrincipalPayment + outputGrid[i].ServiceFee);
                    outputGrid[i].InterestServiceFeeInterestPayment = Round.RoundOffAmount(outputGrid[i].InterestPayment + outputGrid[i].ServiceFeeInterest);
                    outputGrid[i].BeginningPrincipalServiceFee = Round.RoundOffAmount(outputGrid[i].BeginningPrincipal + outputGrid[i].BeginningServiceFee);
                    outputGrid[i].DailyInterestAmount = Round.RoundOffAmount(outputGrid[i].DailyInterestAmount);
                    outputGrid[i].InterestAccrued = Round.RoundOffAmount(outputGrid[i].InterestAccrued);
                    outputGrid[i].InterestCarryOver = Round.RoundOffAmount(outputGrid[i].InterestCarryOver);
                    outputGrid[i].InterestPayment = Round.RoundOffAmount(outputGrid[i].InterestPayment);
                    outputGrid[i].PrincipalPayment = Round.RoundOffAmount(outputGrid[i].PrincipalPayment);
                    outputGrid[i].TotalPayment = Round.RoundOffAmount(outputGrid[i].TotalPayment);
                    outputGrid[i].AccruedServiceFeeInterest = Round.RoundOffAmount(outputGrid[i].AccruedServiceFeeInterest);
                    outputGrid[i].AccruedServiceFeeInterestCarryOver = Round.RoundOffAmount(outputGrid[i].AccruedServiceFeeInterestCarryOver);
                    outputGrid[i].ServiceFeeInterest = Round.RoundOffAmount(outputGrid[i].ServiceFeeInterest);
                    outputGrid[i].ServiceFee = Round.RoundOffAmount(outputGrid[i].ServiceFee);
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
                    outputGrid[i].TotalPastDue = Round.RoundOffAmount((outputGrid[i].PrincipalPastDue + outputGrid[i].InterestPastDue + outputGrid[i].ServiceFeePastDue
                                                + outputGrid[i].ServiceFeeInterestPastDue + outputGrid[i].OriginationFeePastDue + outputGrid[i].SameDayFeePastDue
                                                + outputGrid[i].ManagementFeePastDue + outputGrid[i].MaintenanceFeePastDue + outputGrid[i].NSFFeePastDue
                                                + outputGrid[i].LateFeePastDue));

                    //Cumulative amount past due is added in current past due row
                    outputGrid[i].CumulativePrincipalPastDue = Round.RoundOffAmount(outputGrid[i].PrincipalPastDue);
                    outputGrid[i].CumulativeInterestPastDue = Round.RoundOffAmount(outputGrid[i].InterestPastDue);
                    outputGrid[i].CumulativeServiceFeePastDue = Round.RoundOffAmount(outputGrid[i].ServiceFeePastDue);
                    outputGrid[i].CumulativeServiceFeeInterestPastDue = Round.RoundOffAmount(outputGrid[i].ServiceFeeInterestPastDue);
                    outputGrid[i].CumulativeOriginationFeePastDue = Round.RoundOffAmount(outputGrid[i].OriginationFeePastDue);
                    outputGrid[i].CumulativeMaintenanceFeePastDue = Round.RoundOffAmount(outputGrid[i].MaintenanceFeePastDue);
                    outputGrid[i].CumulativeManagementFeePastDue = Round.RoundOffAmount(outputGrid[i].ManagementFeePastDue);
                    outputGrid[i].CumulativeSameDayFeePastDue = Round.RoundOffAmount(outputGrid[i].SameDayFeePastDue);
                    outputGrid[i].CumulativeNSFFeePastDue = Round.RoundOffAmount(outputGrid[i].NSFFeePastDue);
                    outputGrid[i].CumulativeLateFeePastDue = Round.RoundOffAmount(outputGrid[i].LateFeePastDue);
                    outputGrid[i].CumulativeTotalPastDue = Round.RoundOffAmount((outputGrid[i].PrincipalPastDue + outputGrid[i].InterestPastDue + outputGrid[i].ServiceFeePastDue
                                                         + outputGrid[i].ServiceFeeInterestPastDue + outputGrid[i].OriginationFeePastDue + outputGrid[i].SameDayFeePastDue
                                                         + outputGrid[i].ManagementFeePastDue + outputGrid[i].MaintenanceFeePastDue + outputGrid[i].NSFFeePastDue
                                                         + outputGrid[i].LateFeePastDue));
                }
                sbTracing.AppendLine("Exist:From RollingBusiness Class CreateScheduleForRollingMethod and Method Name SumCumulativeAmount(List<PaymentDetail> outputGrid)");
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
