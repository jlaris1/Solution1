using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using LoanAmort_driver.Models;
using System.Text.RegularExpressions;
using System.Linq;
using System.Globalization;

namespace LoanAmort_driver
{
    public partial class LoanAmortForm : Form
    {
        // AD : added output of version number so that to be sure that up-to-date version is used

        private const string VERSION = "2.1.1.1.8_02";

        public LoanAmortForm()
        {
            InitializeComponent();
            this.Text = "Loan Amort Driver v" + VERSION;
            PeriodCmb.SelectedItem = "12 - Monthly";
            DaysInYearCmb.SelectedIndex = 0;
            DaysInMonthCmb.SelectedIndex = 0;
            LoanTypeCmb.SelectedIndex = 0;
            OutputGridView.Visible = true;
            BankMethodGridView.Visible = false;
            ShowHideFields();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter:Inside LoanAmortForm.cs on button1_Click(object sender, EventArgs e) event.");
                if (LoanTypeCmb.SelectedIndex > -1)
                {
                    string[] periods = PeriodCmb.Text.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    var paymentPeriod = (Models.PaymentPeriod)Convert.ToInt16(periods[0].Trim());
                    string[] dividedPeriod = PeriodCmb.Text.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    PaymentPeriod PmtPeriod = (PaymentPeriod)Convert.ToInt16(dividedPeriod[0].Trim());
                    List<InputGrid> inputGrid = new List<InputGrid>();
                    List<InputRecord> _input = new List<InputRecord>();
                    DataGridViewRow gridRow;

                    //Additional Payment
                    DataGridViewRow addGridRow;
                    var additionalPayments = new List<Models.AdditionalPaymentRecord>();
                    List<AdditionalPaymentRecord> _additional_pmt = new List<AdditionalPaymentRecord>();
                    var outputValues = new OutputSchedule();
                    getScheduleOutput _output = new getScheduleOutput();

                    object[] rowBufferForManagementFeeTable = new object[2];
                    var rowsForManagementFeeTable = new List<DataGridViewRow>();

                    object[] rowBufferForMaintenanceFeeTable = new object[2];
                    var rowsForMaintenanceFeeTable = new List<DataGridViewRow>();

                    switch (LoanTypeCmb.SelectedIndex)
                    {
                        case 0:
                            #region Rigid Method

                            #region Validation
                            Regex regAmount = new Regex(@"^[0-9]*(\.[0-9]+)?$");
                            Regex regIntAmount = new Regex(@"^[0-9]+$");

                            if (string.IsNullOrEmpty(AmountTbx.Text))
                            {
                                throw new System.ArgumentException("Enter Loan Amount.");
                            }
                            else if (!regAmount.IsMatch(AmountTbx.Text))
                            {
                                throw new System.ArgumentException("Loan Amount can only be positive numeric value.");
                            }

                            if (string.IsNullOrEmpty(ServiceFeeCmb.Text))
                            {
                                throw new System.ArgumentException("Please select calculation method for Service Fee.");
                            }
                            if (InputGridView.Rows.Count <= 1)
                            {
                                throw new System.ArgumentException("Please provide loan schedule");
                            }
                            if (!string.IsNullOrEmpty(BalloonPaymentTbx.Text) && Convert.ToDouble(BalloonPaymentTbx.Text) >= Convert.ToDouble(AmountTbx.Text))
                            {
                                throw new ArgumentException("Balloon payment should not be greater or equals to the loan amount Value.");
                            }
                            if (!string.IsNullOrEmpty(BalloonPaymentTbx.Text) && !regIntAmount.IsMatch(BalloonPaymentTbx.Text))
                            {
                                throw new ArgumentException("Invalid balloon payment amount Value.");
                            }
                            //Validate Interest delay value
                            if (!string.IsNullOrEmpty(InterestDelayTbx.Text) && !regIntAmount.IsMatch(InterestDelayTbx.Text))
                            {
                                throw new ArgumentException("Invalid Interest Delay Value.");
                            }
                            //Validate Payment amount value
                            else if (!string.IsNullOrEmpty(PaymentAmountTbx.Text) && !regAmount.IsMatch(PaymentAmountTbx.Text))
                            {
                                throw new ArgumentException("Payment Amount value can only be a positive numeric value with maximum two decimal digits.");
                            }
                            //Validate Enforcement Principal value
                            else if (!string.IsNullOrEmpty(EnforcedPrincipalTbx.Text) && (!regAmount.IsMatch(EnforcedPrincipalTbx.Text) || Convert.ToDouble(EnforcedPrincipalTbx.Text) <= 0))
                            {
                                throw new ArgumentException("Enforcement Pricipal value can only be a positive numeric value with maximum two decimal digits.");
                            }
                            //Validate Enforcement Payment value
                            else if (!string.IsNullOrEmpty(EnforcedPaymentTbx.Text) && (!regIntAmount.IsMatch(EnforcedPaymentTbx.Text) || Convert.ToInt32(EnforcedPaymentTbx.Text) <= 0))
                            {
                                throw new ArgumentException("Enforced Payment value can only be a positive numeric value.");
                            }
                            else if ((!string.IsNullOrEmpty(EnforcedPaymentTbx.Text) && string.IsNullOrEmpty(EnforcedPrincipalTbx.Text)) ||
                                (string.IsNullOrEmpty(EnforcedPaymentTbx.Text) && !string.IsNullOrEmpty(EnforcedPrincipalTbx.Text)))
                            {
                                throw new ArgumentException("Provide Enforced Principal and Enforced Payment both.");
                            }
                            #endregion

                            #region Validate Earned Fee tab controls
                            if (!regAmount.IsMatch(EarnedInterestTbx.Text))
                            {
                                throw new System.ArgumentException("Earned Interest fee value can only be a positive numeric value with maximum two decimal digits.");
                            }
                            if (!regAmount.IsMatch(EarnedServiceFeeInterestTbx.Text))
                            {
                                throw new System.ArgumentException("Earned Service Fee Interest value can only be a positive numeric value with maximum two decimal digits.");
                            }
                            #endregion

                            #region Validate Interest Tier tab controls

                            if (string.IsNullOrEmpty(InterestRateTbx.Text))
                            {
                                throw new System.ArgumentException("Enter Interest Rate.");
                            }
                            else if (!regAmount.IsMatch(InterestRateTbx.Text))
                            {
                                throw new System.ArgumentException("Annual Interest Rate can only be positive numeric value with maximum four decimal digits.");
                            }
                            else if (!regAmount.IsMatch(InterestTier1Tbx.Text))
                            {
                                throw new System.ArgumentException("Interest tier value can only be a positive numeric value with maximum two decimal digits.");
                            }
                            else if (!string.IsNullOrEmpty(InterestTier1Tbx.Text) && Convert.ToDouble(InterestTier1Tbx.Text) <= 0)
                            {
                                throw new System.ArgumentException("Interest Tier value must be greater than zero.");
                            }
                            else if (!regAmount.IsMatch(InterestRate2Tbx.Text))
                            {
                                throw new System.ArgumentException("Annual Interest Rate can only be positive numeric value with maximum four decimal digits.");
                            }
                            else if (!regAmount.IsMatch(InterestTier2Tbx.Text))
                            {
                                throw new System.ArgumentException("Interest tier value can only be a positive numeric value with maximum two decimal digits.");
                            }
                            else if (!regAmount.IsMatch(InterestRate3Tbx.Text))
                            {
                                throw new System.ArgumentException("Annual Interest Rate can only be positive numeric value with maximum four decimal digits.");
                            }
                            else if (!regAmount.IsMatch(InterestTier3Tbx.Text))
                            {
                                throw new System.ArgumentException("Interest tier value can only be a positive numeric value with maximum two decimal digits.");
                            }
                            else if (!regAmount.IsMatch(InterestRate4Tbx.Text))
                            {
                                throw new System.ArgumentException("Annual Interest Rate can only be positive numeric value with maximum four decimal digits.");
                            }
                            else if (!regAmount.IsMatch(InterestTier4Tbx.Text))
                            {
                                throw new System.ArgumentException("Interest tier value can only be a positive numeric value with maximum two decimal digits.");
                            }
                            else if (!string.IsNullOrEmpty(InterestTier1Tbx.Text) && !string.IsNullOrEmpty(InterestTier2Tbx.Text) &&
                                        Convert.ToDouble(InterestTier2Tbx.Text) <= Convert.ToDouble(InterestTier1Tbx.Text))
                            {
                                throw new System.ArgumentException("Interest tier value must be greater than the previous interest tier value.");
                            }
                            else if (!string.IsNullOrEmpty(InterestTier2Tbx.Text) && !string.IsNullOrEmpty(InterestTier3Tbx.Text) &&
                                        Convert.ToDouble(InterestTier3Tbx.Text) <= Convert.ToDouble(InterestTier2Tbx.Text))
                            {
                                throw new System.ArgumentException("Interest tier value must be greater than the previous interest tier value.");
                            }
                            else if (!string.IsNullOrEmpty(InterestTier3Tbx.Text) && !string.IsNullOrEmpty(InterestTier4Tbx.Text) &&
                                     Convert.ToDouble(InterestTier4Tbx.Text) <= Convert.ToDouble(InterestTier3Tbx.Text))
                            {
                                throw new System.ArgumentException("Interest tier value must be greater than the previous interest tier value.");
                            }
                            else if ((string.IsNullOrEmpty(InterestTier1Tbx.Text) && (!string.IsNullOrEmpty(InterestTier2Tbx.Text) || !string.IsNullOrEmpty(InterestTier3Tbx.Text) || !string.IsNullOrEmpty(InterestTier4Tbx.Text))) ||
                           (string.IsNullOrEmpty(InterestTier2Tbx.Text) && (!string.IsNullOrEmpty(InterestTier3Tbx.Text) || !string.IsNullOrEmpty(InterestTier4Tbx.Text))) ||
                               (string.IsNullOrEmpty(InterestTier3Tbx.Text) && !string.IsNullOrEmpty(InterestTier4Tbx.Text)))
                            {
                                throw new System.ArgumentException("Invalid Balance Cap values.");
                            }
                            #endregion

                            RigidMethod();
                            #endregion
                            break;
                        case 1:
                            #region Bank Method

                            sbTracing.AppendLine("calling...Bank Method LoanType");
                            sbTracing.AppendLine("Inside LoanAmortForm class, and button1_Click() method. Calling method : ValidateInputValues();");
                            if (!ValidateInputValues())
                            {
                                return;
                            }

                            #region

                            var bankScheduleInput = new LoanDetails();
                            OutputGridView.Visible = false;
                            BankMethodGridView.Visible = true;

                            #region InputGridView
                            for (int i = 0; i < InputGridView.Rows.Count - 1; i++)
                            {
                                gridRow = InputGridView.Rows[i];
                                inputGrid.Add(new InputGrid
                                {
                                    DateIn = Convert.ToDateTime(gridRow.Cells["DateIn"].Value),
                                    Flags = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["Flags"].Value)) ? (int)Constants.FlagValues.Payment : Convert.ToInt32(gridRow.Cells["Flags"].Value),
                                    PaymentID = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["PaymentID"].Value)) ? 0 : Convert.ToInt32(gridRow.Cells["PaymentID"].Value),
                                    EffectiveDate = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["EffectiveDate"].Value)) ? DateTime.MinValue : Convert.ToDateTime(gridRow.Cells["EffectiveDate"].Value),
                                    PaymentAmount = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["PaymentAmountSchedule"].Value)) ? "" : Convert.ToString(Convert.ToDouble(gridRow.Cells["PaymentAmountSchedule"].Value)),
                                    InterestRate = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["InterestRate"].Value)) ? "" : Convert.ToString(gridRow.Cells["InterestRate"].Value),
                                    InterestPriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["InterestPriority"].Value)) ? (IntrestPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.Interest : Convert.ToInt32(IntrestPriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["InterestPriority"].Value),
                                    PrincipalPriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["PrincipalPriority"].Value)) ? (PrincipalPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.Principal : Convert.ToInt32(PrincipalPriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["PrincipalPriority"].Value),
                                    OriginationFeePriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["OriginationFeePriority"].Value)) ? (OriginationFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.OriginationFee : Convert.ToInt32(OriginationFeePriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["OriginationFeePriority"].Value),
                                    SameDayFeePriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["SameDayFeePriority"].Value)) ? (SameDayFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.SameDayFee : Convert.ToInt32(SameDayFeePriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["SameDayFeePriority"].Value),
                                    ServiceFeePriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["ServiceFeePriority"].Value)) ? (ServiceFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ServiceFee : Convert.ToInt32(ServiceFeePriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["ServiceFeePriority"].Value),
                                    ServiceFeeInterestPriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["ServiceFeeInterestPriority"].Value)) ? (ServiceFeeInterestPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ServiceFeeInterest : Convert.ToInt32(ServiceFeeInterestPriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["ServiceFeeInterestPriority"].Value),
                                    ManagementFeePriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["ManagementFeePriority"].Value)) ? (ManagementFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ManagementFee : Convert.ToInt32(ManagementFeePriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["ManagementFeePriority"].Value),
                                    MaintenanceFeePriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["MaintenanceFeePriority"].Value)) ? (MaintenanceFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.MaintenanceFee : Convert.ToInt32(MaintenanceFeePriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["MaintenanceFeePriority"].Value),
                                    NSFFeePriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["NSFFeePriority"].Value)) ? (NSFFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.NSFFee : Convert.ToInt32(NSFFeePriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["NSFFeePriority"].Value),
                                    LateFeePriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["LateFeePriority"].Value)) ? (LateFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.LateFee : Convert.ToInt32(LateFeePriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["LateFeePriority"].Value)
                                });
                            }
                            #endregion

                            #region Additional payment gridview
                            for (int i = 0; i < BankAdditionalPaymentGridView.Rows.Count - 1; i++)
                            {
                                addGridRow = BankAdditionalPaymentGridView.Rows[i];
                                additionalPayments.Add(new Models.AdditionalPaymentRecord
                                {
                                    DateIn = Convert.ToDateTime(addGridRow.Cells["DateInAdditional"].Value),
                                    AdditionalPayment = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["AdditionalPaymentBank"].Value)) ? 0.0 : Convert.ToDouble(addGridRow.Cells["AdditionalPaymentBank"].Value),
                                    PaymentID = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["PaymentIDAdditionalBank"].Value)) ? 0 : Convert.ToInt32(addGridRow.Cells["PaymentIDAdditionalBank"].Value),
                                    Flags = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["FlagsAdditionalBank"].Value)) ? 0 : Convert.ToInt32(addGridRow.Cells["FlagsAdditionalBank"].Value),
                                    InterestRate = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["InterestRateAdditional"].Value)) ? "" : Convert.ToString(addGridRow.Cells["InterestRateAdditional"].Value),
                                    PrincipalDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["PrincipalAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["PrincipalAdditional"].Value),
                                    InterestDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["InterestAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["InterestAdditional"].Value),
                                    OriginationFeeDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["OriginationFeeAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["OriginationFeeAdditional"].Value),
                                    SameDayFeeDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["SameDayFeeAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["SameDayFeeAdditional"].Value),
                                    ServiceFeeDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["ServiceFeeAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["ServiceFeeAdditional"].Value),
                                    ServiceFeeInterestDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["ServiceFeeInterestAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["ServiceFeeInterestAdditional"].Value),
                                    ManagementFeeDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["ManagementFeeAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["ManagementFeeAdditional"].Value),
                                    MaintenanceFeeDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["MaintenanceFeeAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["MaintenanceFeeAdditional"].Value),
                                    NSFFeeDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["NSFFeeAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["NSFFeeAdditional"].Value),
                                    LateFeeDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["LateFeeAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["LateFeeAdditional"].Value),
                                    InterestPriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["InterestPriorityAdditional"].Value)) ? (IntrestPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.Interest : Convert.ToInt32(IntrestPriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["InterestPriorityAdditional"].Value),
                                    PrincipalPriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["PrincipalPriorityAdditional"].Value)) ? (PrincipalPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.Principal : Convert.ToInt32(PrincipalPriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["PrincipalPriorityAdditional"].Value),
                                    OriginationFeePriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["OriginationFeePriorityAdditional"].Value)) ? (OriginationFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.OriginationFee : Convert.ToInt32(OriginationFeePriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["OriginationFeePriorityAdditional"].Value),
                                    SameDayFeePriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["SameDayFeePriorityAdditional"].Value)) ? (SameDayFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.SameDayFee : Convert.ToInt32(SameDayFeePriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["SameDayFeePriorityAdditional"].Value),
                                    ServiceFeePriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["ServiceFeePriorityAdditional"].Value)) ? (ServiceFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ServiceFee : Convert.ToInt32(ServiceFeePriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["ServiceFeePriorityAdditional"].Value),
                                    ServiceFeeInterestPriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["ServiceFeeInterestPriorityAdditional"].Value)) ? (ServiceFeeInterestPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ServiceFeeInterest : Convert.ToInt32(ServiceFeeInterestPriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["ServiceFeeInterestPriorityAdditional"].Value),
                                    ManagementFeePriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["ManagementFeePriorityAdditional"].Value)) ? (ManagementFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ManagementFee : Convert.ToInt32(ManagementFeePriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["ManagementFeePriorityAdditional"].Value),
                                    MaintenanceFeePriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["MaintenanceFeePriorityAdditional"].Value)) ? (MaintenanceFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.MaintenanceFee : Convert.ToInt32(MaintenanceFeePriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["MaintenanceFeePriorityAdditional"].Value),
                                    NSFFeePriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["NSFFeePriorityAdditional"].Value)) ? (NSFFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.NSFFee : Convert.ToInt32(NSFFeePriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["NSFFeePriorityAdditional"].Value),
                                    LateFeePriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["LateFeePriorityAdditional"].Value)) ? (LateFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.LateFee : Convert.ToInt32(LateFeePriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["LateFeePriorityAdditional"].Value)
                                });
                            }
                            #endregion

                            #region Initianlize the periodScheduleInput Object
                            sbTracing.AppendLine("Initianlize the BankScheduleInput Object");
                            bankScheduleInput = new LoanDetails
                            {
                                InputRecords = inputGrid,
                                AdditionalPaymentRecords = additionalPayments,
                                LoanAmount = string.IsNullOrEmpty(AmountTbx.Text) ? 0 : Convert.ToDouble(AmountTbx.Text),
                                InterestDelay = string.IsNullOrEmpty(InterestDelayTbx.Text) ? 0 : Convert.ToInt32(InterestDelayTbx.Text),
                                Residual = string.IsNullOrEmpty(ResidualTbx.Text) ? 0 : Convert.ToDouble(ResidualTbx.Text),
                                BalloonPayment = string.IsNullOrEmpty(BalloonPaymentTbx.Text) ? 0 : Convert.ToDouble(BalloonPaymentTbx.Text),
                                PmtPeriod = paymentPeriod,
                                EarlyPayoffDate = Convert.ToDateTime(string.IsNullOrEmpty(EarlyPayoffDateTbx.Text) ? Constants.DefaultDate : EarlyPayoffDateTbx.Text),
                                MinDuration = string.IsNullOrEmpty(MinDurationTbx.Text) ? 0 : Convert.ToInt32(MinDurationTbx.Text),
                                AfterPayment = AfterPaymentChb.Checked,
                                AccruedInterestDate = Convert.ToDateTime(string.IsNullOrEmpty(AccruedInterestDateTbx.Text) ? Constants.DefaultDate : AccruedInterestDateTbx.Text),
                                ServiceFee = Convert.ToDouble(string.IsNullOrEmpty(ServiceFeeTbx.Text) ? "0.0" : ServiceFeeTbx.Text),
                                ManagementFee = Convert.ToDouble(string.IsNullOrEmpty(ManagementFeeTbx.Text) ? "0.0" : ManagementFeeTbx.Text),
                                ManagementFeePercent = Convert.ToDouble(string.IsNullOrEmpty(ManagementFeePercentTbx.Text) ? "0.0" : ManagementFeePercentTbx.Text),
                                ManagementFeeBasis = ManagementFeeBasisCmb.SelectedIndex,
                                ManagementFeeFrequency = ManagementFeeFreqCmb.SelectedIndex,
                                ManagementFeeFrequencyNumber = Convert.ToInt32(string.IsNullOrEmpty(ManagementFeeFreqNumTbx.Text) ? "0" : ManagementFeeFreqNumTbx.Text),
                                ManagementFeeDelay = Convert.ToInt32(string.IsNullOrEmpty(ManagementFeeDelayTbx.Text) ? "0" : ManagementFeeDelayTbx.Text),
                                ManagementFeeMin = Convert.ToDouble(string.IsNullOrEmpty(ManagementFeeMinTbx.Text) ? "0.0" : ManagementFeeMinTbx.Text),
                                ManagementFeeMaxPer = Convert.ToDouble(string.IsNullOrEmpty(ManagementFeeMaxPerTbx.Text) ? "0.0" : ManagementFeeMaxPerTbx.Text),
                                ManagementFeeMaxMonth = Convert.ToDouble(string.IsNullOrEmpty(ManagementFeeMaxMonthTbx.Text) ? "0.0" : ManagementFeeMaxMonthTbx.Text),
                                ManagementFeeMaxLoan = Convert.ToDouble(string.IsNullOrEmpty(ManagementFeeMaxLoanTbx.Text) ? "0.0" : ManagementFeeMaxLoanTbx.Text),
                                IsManagementFeeGreater = ManagementFeeGreaterChb.Checked,
                                MaintenanceFee = Convert.ToDouble(string.IsNullOrEmpty(MaintenanceFeeTbx.Text) ? "0.0" : MaintenanceFeeTbx.Text),
                                MaintenanceFeePercent = Convert.ToDouble(string.IsNullOrEmpty(MaintenanceFeePercentTbx.Text) ? "0.0" : MaintenanceFeePercentTbx.Text),
                                MaintenanceFeeBasis = MaintenanceFeeBasisCmb.SelectedIndex,
                                MaintenanceFeeFrequency = MaintenanceFeeFreqCmb.SelectedIndex,
                                MaintenanceFeeFrequencyNumber = Convert.ToInt32(string.IsNullOrEmpty(MaintenanceFeeFreqNumTbx.Text) ? "0" : MaintenanceFeeFreqNumTbx.Text),
                                MaintenanceFeeDelay = Convert.ToInt32(string.IsNullOrEmpty(MaintenanceFeeDelayTbx.Text) ? "0" : MaintenanceFeeDelayTbx.Text),
                                MaintenanceFeeMin = Convert.ToDouble(string.IsNullOrEmpty(MaintenanceFeeMinTbx.Text) ? "0.0" : MaintenanceFeeMinTbx.Text),
                                MaintenanceFeeMaxPer = Convert.ToDouble(string.IsNullOrEmpty(MaintenanceFeeMaxPerTbx.Text) ? "0.0" : MaintenanceFeeMaxPerTbx.Text),
                                MaintenanceFeeMaxMonth = Convert.ToDouble(string.IsNullOrEmpty(MaintenanceFeeMaxMonthTbx.Text) ? "0.0" : MaintenanceFeeMaxMonthTbx.Text),
                                MaintenanceFeeMaxLoan = Convert.ToDouble(string.IsNullOrEmpty(MaintenanceFeeMaxLoanTbx.Text) ? "0.0" : MaintenanceFeeMaxLoanTbx.Text),
                                IsMaintenanceFeeGreater = MaintenanceFeeGreaterChb.Checked,
                                RecastAdditionalPayments = RecastAdditionalPaymentsChb.Checked,
                                UseFlexibleCalculation = UseFlexibleMethod.Checked,
                                DaysInYearBankMethod = Convert.ToString(DaysInYearCmb.Text),
                                LoanType = Convert.ToString(LoanTypeCmb.Text),
                                DaysInMonth = Convert.ToString(DaysInMonthCmb.Text),
                                ApplyServiceFeeInterest = ServiceFeeCmb.SelectedIndex,
                                IsServiceFeeFinanced = FinanceServiceFeeChb.Checked,
                                ServiceFeeFirstPayment = ServiceFeeFirstPaymentChb.Checked,
                                IsServiceFeeIncremental = ServiceFeeIncrementalChb.Checked,
                                SameDayFee = Convert.ToDouble(string.IsNullOrEmpty(SameDayFeeTbx.Text) ? "0.0" : SameDayFeeTbx.Text),
                                SameDayFeeCalculationMethod = string.IsNullOrEmpty(SameDayFeeCmb.Text) ? "Fixed" : Convert.ToString(SameDayFeeCmb.Text),
                                IsSameDayFeeFinanced = FinanceSameDayFeeChb.Checked,
                                OriginationFee = Convert.ToDouble(string.IsNullOrEmpty(OriginationFeeFixedTbx.Text) ? "0.0" : OriginationFeeFixedTbx.Text),
                                OriginationFeePercent = Convert.ToDouble(string.IsNullOrEmpty(OriginationFeePercentTbx.Text) ? "0.0" : OriginationFeePercentTbx.Text),
                                OriginationFeeMax = Convert.ToDouble(string.IsNullOrEmpty(OriginationFeeMaxTbx.Text) ? "0.0" : OriginationFeeMaxTbx.Text),
                                OriginationFeeMin = Convert.ToDouble(string.IsNullOrEmpty(OriginationFeeMinTbx.Text) ? "0.0" : OriginationFeeMinTbx.Text),
                                OriginationFeeCalculationMethod = OriginationFeeGreaterChb.Checked,
                                IsOriginationFeeFinanced = FinanceOriginationFeeChb.Checked,
                                AmountFinanced = string.IsNullOrEmpty(AmountFinancedTbx.Text) ? 0 : Convert.ToDouble(AmountFinancedTbx.Text),
                                OriginationFeePriority = OriginationFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.OriginationFee : Convert.ToInt32(OriginationFeePriorityCmb.Text),
                                SameDayFeePriority = SameDayFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.SameDayFee : Convert.ToInt32(SameDayFeePriorityCmb.Text),
                                InterestPriority = IntrestPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.Interest : Convert.ToInt32(IntrestPriorityCmb.Text),
                                PrincipalPriority = PrincipalPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.Principal : Convert.ToInt32(PrincipalPriorityCmb.Text),
                                ServiceFeeInterestPriority = ServiceFeeInterestPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ServiceFeeInterest : Convert.ToInt32(ServiceFeeInterestPriorityCmb.Text),
                                ServiceFeePriority = ServiceFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ServiceFee : Convert.ToInt32(ServiceFeePriorityCmb.Text),
                                ManagementFeePriority = ManagementFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ManagementFee : Convert.ToInt32(ManagementFeePriorityCmb.Text),
                                MaintenanceFeePriority = MaintenanceFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.MaintenanceFee : Convert.ToInt32(MaintenanceFeePriorityCmb.Text),
                                LateFeePriority = LateFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.LateFee : Convert.ToInt32(LateFeePriorityCmb.Text),
                                NSFFeePriority = NSFFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.NSFFee : Convert.ToInt32(NSFFeePriorityCmb.Text),
                                AccruedServiceFeeInterestDate = Convert.ToDateTime(string.IsNullOrEmpty(AccruedServiceFeeInterestDateTbx.Text) ? Constants.DefaultDate : AccruedServiceFeeInterestDateTbx.Text),
                                SameAsCashPayoff = SameAsCashPayOffChb.Checked,
                                PaymentAmount = string.IsNullOrEmpty(PaymentAmountTbx.Text) ? 0 : Convert.ToDouble(PaymentAmountTbx.Text),
                                EarnedInterest = string.IsNullOrEmpty(EarnedInterestTbx.Text) ? 0 : Convert.ToDouble(EarnedInterestTbx.Text),
                                EarnedServiceFeeInterest = string.IsNullOrEmpty(EarnedServiceFeeInterestTbx.Text) ? 0 : Convert.ToDouble(EarnedServiceFeeInterestTbx.Text),
                                EarnedOriginationFee = string.IsNullOrEmpty(EarnedOriginationFeeTbx.Text) ? 0 : Convert.ToDouble(EarnedOriginationFeeTbx.Text),
                                EarnedSameDayFee = string.IsNullOrEmpty(EarnedSameDayFeeTbx.Text) ? 0 : Convert.ToDouble(EarnedSameDayFeeTbx.Text),
                                EarnedManagementFee = string.IsNullOrEmpty(EarnedManagementFeeTbx.Text) ? 0 : Convert.ToDouble(EarnedManagementFeeTbx.Text),
                                EarnedMaintenanceFee = string.IsNullOrEmpty(EarnedMaintenanceFeeTbx.Text) ? 0 : Convert.ToDouble(EarnedMaintenanceFeeTbx.Text),
                                EarnedNSFFee = string.IsNullOrEmpty(EarnedNSFFeeTbx.Text) ? 0 : Convert.ToDouble(EarnedNSFFeeTbx.Text),
                                EarnedLateFee = string.IsNullOrEmpty(EarnedLateFeeTbx.Text) ? 0 : Convert.ToDouble(EarnedLateFeeTbx.Text),
                                InterestRate = string.IsNullOrEmpty(InterestRateTbx.Text) ? "" : InterestRateTbx.Text,
                                Tier1 = string.IsNullOrEmpty(InterestTier1Tbx.Text) ? 0 : Convert.ToDouble(InterestTier1Tbx.Text),
                                InterestRate2 = string.IsNullOrEmpty(InterestRate2Tbx.Text) ? "" : InterestRate2Tbx.Text,
                                Tier2 = string.IsNullOrEmpty(InterestTier2Tbx.Text) ? 0 : Convert.ToDouble(InterestTier2Tbx.Text),
                                InterestRate3 = string.IsNullOrEmpty(InterestRate3Tbx.Text) ? "" : InterestRate3Tbx.Text,
                                Tier3 = string.IsNullOrEmpty(InterestTier3Tbx.Text) ? 0 : Convert.ToDouble(InterestTier3Tbx.Text),
                                InterestRate4 = string.IsNullOrEmpty(InterestRate4Tbx.Text) ? "" : InterestRate4Tbx.Text,
                                Tier4 = string.IsNullOrEmpty(InterestTier4Tbx.Text) ? 0 : Convert.ToDouble(InterestTier4Tbx.Text),
                                EnforcedPrincipal = string.IsNullOrEmpty(EnforcedPrincipalTbx.Text) ? 0 : Convert.ToDouble(EnforcedPrincipalTbx.Text),
                                EnforcedPayment = string.IsNullOrEmpty(EnforcedPaymentTbx.Text) ? 0 : Convert.ToInt32(EnforcedPaymentTbx.Text)
                            };
                            #endregion

                            #region Validate Input and Additional payment input grid

                            //Sort the input grid records and additioanl payment grid records.
                            bankScheduleInput.InputRecords.Sort();
                            bankScheduleInput.AdditionalPaymentRecords = bankScheduleInput.AdditionalPaymentRecords.OrderBy(o => o.DateIn).ToList();
                            for (int i = 0; i < bankScheduleInput.InputRecords.Count; i++)
                            {
                                if (bankScheduleInput.InputRecords.FindLastIndex(o => o.DateIn == bankScheduleInput.InputRecords[i].DateIn) != i)
                                {
                                    DisplayMessege(ValidationMessage.RepeatedDateInInputGrid);
                                    InputGridView.Focus();
                                    return;
                                }

                                if (bankScheduleInput.InputRecords.FindLastIndex(o => o.PaymentID == bankScheduleInput.InputRecords[i].PaymentID) != i ||
                                        bankScheduleInput.AdditionalPaymentRecords.FindIndex(o => o.PaymentID == bankScheduleInput.InputRecords[i].PaymentID) != -1)
                                {
                                    DisplayMessege(ValidationMessage.RepeatedPaymentId);
                                    InputGridView.Focus();
                                    return;
                                }
                                if ((bankScheduleInput.InputRecords.FindLastIndex(o => o.EffectiveDate == bankScheduleInput.InputRecords[i].EffectiveDate) != i) &&
                                        (bankScheduleInput.InputRecords[i].EffectiveDate != DateTime.MinValue))
                                {
                                    BankMethodGridView.Rows.Clear();
                                    DisplayMessege(ValidationMessage.RepeatedDateInInputGrid);
                                    InputGridView.Focus();
                                    return;
                                }
                                if ((bankScheduleInput.InputRecords.FindLastIndex(o => o.DateIn == bankScheduleInput.InputRecords[i].EffectiveDate) != -1) &&
                                    (bankScheduleInput.InputRecords.FindLastIndex(o => o.DateIn == bankScheduleInput.InputRecords[i].EffectiveDate) != i) &&
                                    (bankScheduleInput.InputRecords[i].EffectiveDate != DateTime.MinValue))
                                {
                                    BankMethodGridView.Rows.Clear();
                                    DisplayMessege(ValidationMessage.RepeatedDateInInputGrid);
                                    InputGridView.Focus();
                                    return;
                                }
                                if (i > 0 && bankScheduleInput.InputRecords[i].EffectiveDate != DateTime.MinValue &&
                                    ((bankScheduleInput.InputRecords[i].EffectiveDate <= (bankScheduleInput.InputRecords[i - 1].EffectiveDate == DateTime.MinValue ? bankScheduleInput.InputRecords[i - 1].DateIn : bankScheduleInput.InputRecords[i - 1].EffectiveDate)) ||
                                    (i < (bankScheduleInput.InputRecords.Count - 1) && (bankScheduleInput.InputRecords[i].EffectiveDate >= (bankScheduleInput.InputRecords[i + 1].EffectiveDate == DateTime.MinValue ? bankScheduleInput.InputRecords[i + 1].DateIn : bankScheduleInput.InputRecords[i + 1].EffectiveDate)))))
                                {
                                    BankMethodGridView.Rows.Clear();
                                    DisplayMessege(ValidationMessage.DateRangeInInputGrid);
                                    InputGridView.Focus();
                                    return;
                                }
                            }

                            if (bankScheduleInput.InputRecords[bankScheduleInput.InputRecords.Count - 1].Flags == (int)Constants.FlagValues.SkipPayment)
                            {
                                DisplayMessege(ValidationMessage.SkippPayment);
                                InputGridView.Focus();
                                return;
                            }
                            else if (bankScheduleInput.AdditionalPaymentRecords.FindIndex(o => o.DateIn <= bankScheduleInput.InputRecords[0].DateIn) != -1)
                            {
                                DisplayMessege(ValidationMessage.Additionalpaymentdate);
                                BankAdditionalPaymentGridView.Focus();
                                return;
                            }
                            else if (bankScheduleInput.AdditionalPaymentRecords.FindIndex(o => o.DateIn > bankScheduleInput.InputRecords[bankScheduleInput.InputRecords.Count - 1].DateIn &&
                                     (o.Flags == (int)Constants.FlagValues.NSFFee || o.Flags == (int)Constants.FlagValues.LateFee)) != -1)
                            {
                                DisplayMessege(ValidationMessage.NSFAndLateFeeValidation);
                                BankAdditionalPaymentGridView.Focus();
                                return;
                            }
                            else if ((bankScheduleInput.ManagementFee > 0 || bankScheduleInput.ManagementFeePercent > 0) &&
                                    bankScheduleInput.AdditionalPaymentRecords.FindIndex(o => o.Flags == (int)Constants.FlagValues.ManagementFee) != -1)
                            {
                                DisplayMessege(ValidationMessage.ManagementFeeFromTwoPlaces);
                                BankAdditionalPaymentGridView.Focus();
                                return;
                            }
                            else if ((bankScheduleInput.MaintenanceFee > 0 || bankScheduleInput.MaintenanceFeePercent > 0) &&
                                    bankScheduleInput.AdditionalPaymentRecords.FindIndex(o => o.Flags == (int)Constants.FlagValues.MaintenanceFee) != -1)
                            {
                                DisplayMessege(ValidationMessage.MaintenanceFeeFromTwoPlaces);
                                BankAdditionalPaymentGridView.Focus();
                                return;
                            }
                            else
                            {
                                for (int i = 0; i < bankScheduleInput.AdditionalPaymentRecords.Count; i++)
                                {
                                    if (bankScheduleInput.AdditionalPaymentRecords.FindLastIndex(o => o.PaymentID == bankScheduleInput.AdditionalPaymentRecords[i].PaymentID) != i)
                                    {
                                        DisplayMessege(ValidationMessage.RepeatedPaymentId);
                                        BankAdditionalPaymentGridView.Focus();
                                        return;
                                    }
                                }
                            }

                            #endregion

                            sbTracing.AppendLine("Inside LoanAmortForm class, and button1_Click() method. Calling method : Business.CreateScheduleForBankMethod.DefaultSchedule(bankScheduleInput);");
                            outputValues = Business.CreateScheduleForBankMethod.DefaultSchedule(bankScheduleInput);

                            if (outputValues.LoanAmountCalc <= bankScheduleInput.Residual)
                            {
                                DisplayMessege(ValidationMessage.ValidateResidualAmount);
                                ResidualTbx.Focus();
                                return;
                            }
                            else if (outputValues.LoanAmountCalc <= bankScheduleInput.BalloonPayment)
                            {
                                DisplayMessege(ValidationMessage.ValidateBalloonPaymentWithLoanAmount);
                                ResidualTbx.Focus();
                                return;
                            }


                            #region Fill Output Grid

                            BankMethodGridView.Rows.Clear();
                            object[] rowBufferForBankMethod = new object[77];
                            var rowsForBankMethod = new List<DataGridViewRow>();
                            foreach (var grdValues in outputValues.Schedule)
                            {
                                rowBufferForBankMethod[0] = grdValues.PaymentDate.ToString("MM/dd/yyyy");
                                rowBufferForBankMethod[1] = Convert.ToString(grdValues.BeginningPrincipal);
                                rowBufferForBankMethod[2] = Convert.ToString(grdValues.BeginningServiceFee);
                                rowBufferForBankMethod[3] = Convert.ToString(grdValues.BeginningPrincipalServiceFee);
                                rowBufferForBankMethod[4] = Convert.ToString(grdValues.PeriodicInterestRate);
                                rowBufferForBankMethod[5] = Convert.ToString(grdValues.DailyInterestRate);
                                rowBufferForBankMethod[6] = Convert.ToString(grdValues.DailyInterestAmount);
                                rowBufferForBankMethod[7] = Convert.ToString(grdValues.PaymentID);
                                rowBufferForBankMethod[8] = Convert.ToString(grdValues.Flags);
                                rowBufferForBankMethod[9] = grdValues.DueDate.ToString("MM/dd/yyyy");
                                rowBufferForBankMethod[10] = Convert.ToString(grdValues.InterestAccrued);
                                rowBufferForBankMethod[11] = Convert.ToString(grdValues.InterestDue);
                                rowBufferForBankMethod[12] = Convert.ToString(grdValues.InterestCarryOver);
                                //Amount to be paid in the particular period columns
                                rowBufferForBankMethod[13] = Convert.ToString(grdValues.InterestPayment);
                                rowBufferForBankMethod[14] = Convert.ToString(grdValues.PrincipalPayment);
                                rowBufferForBankMethod[15] = Convert.ToString(grdValues.PaymentDue);
                                rowBufferForBankMethod[16] = Convert.ToString(grdValues.TotalPayment);
                                rowBufferForBankMethod[17] = Convert.ToString(grdValues.InterestServiceFeeInterestPayment);
                                rowBufferForBankMethod[18] = Convert.ToString(grdValues.PrincipalServiceFeePayment);
                                rowBufferForBankMethod[19] = Convert.ToString(grdValues.AccruedServiceFeeInterest);
                                rowBufferForBankMethod[20] = Convert.ToString(grdValues.ServiceFeeInterestDue);
                                rowBufferForBankMethod[21] = Convert.ToString(grdValues.AccruedServiceFeeInterestCarryOver);
                                rowBufferForBankMethod[22] = Convert.ToString(grdValues.ServiceFeeInterest);
                                rowBufferForBankMethod[23] = Convert.ToString(grdValues.ServiceFee);
                                rowBufferForBankMethod[24] = Convert.ToString(grdValues.ServiceFeeTotal);
                                rowBufferForBankMethod[25] = Convert.ToString(grdValues.OriginationFee);
                                rowBufferForBankMethod[26] = Convert.ToString(grdValues.MaintenanceFee);
                                rowBufferForBankMethod[27] = Convert.ToString(grdValues.ManagementFee);
                                rowBufferForBankMethod[28] = Convert.ToString(grdValues.SameDayFee);
                                rowBufferForBankMethod[29] = Convert.ToString(grdValues.NSFFee);
                                rowBufferForBankMethod[30] = Convert.ToString(grdValues.LateFee);
                                //Paid amount columns
                                rowBufferForBankMethod[31] = Convert.ToString(grdValues.PrincipalPaid);
                                rowBufferForBankMethod[32] = Convert.ToString(grdValues.InterestPaid);
                                rowBufferForBankMethod[33] = Convert.ToString(grdValues.ServiceFeePaid);
                                rowBufferForBankMethod[34] = Convert.ToString(grdValues.ServiceFeeInterestPaid);
                                rowBufferForBankMethod[35] = Convert.ToString(grdValues.OriginationFeePaid);
                                rowBufferForBankMethod[36] = Convert.ToString(grdValues.MaintenanceFeePaid);
                                rowBufferForBankMethod[37] = Convert.ToString(grdValues.ManagementFeePaid);
                                rowBufferForBankMethod[38] = Convert.ToString(grdValues.SameDayFeePaid);
                                rowBufferForBankMethod[39] = Convert.ToString(grdValues.NSFFeePaid);
                                rowBufferForBankMethod[40] = Convert.ToString(grdValues.LateFeePaid);
                                rowBufferForBankMethod[41] = Convert.ToString(grdValues.TotalPaid);
                                //Cumulative Amount paid column
                                rowBufferForBankMethod[42] = Convert.ToString(grdValues.CumulativeInterest);
                                rowBufferForBankMethod[43] = Convert.ToString(grdValues.CumulativePrincipal);
                                rowBufferForBankMethod[44] = Convert.ToString(grdValues.CumulativePayment);
                                rowBufferForBankMethod[45] = Convert.ToString(grdValues.CumulativeServiceFee);
                                rowBufferForBankMethod[46] = Convert.ToString(grdValues.CumulativeServiceFeeInterest);
                                rowBufferForBankMethod[47] = Convert.ToString(grdValues.CumulativeServiceFeeTotal);
                                rowBufferForBankMethod[48] = Convert.ToString(grdValues.CumulativeOriginationFee);
                                rowBufferForBankMethod[49] = Convert.ToString(grdValues.CumulativeMaintenanceFee);
                                rowBufferForBankMethod[50] = Convert.ToString(grdValues.CumulativeManagementFee);
                                rowBufferForBankMethod[51] = Convert.ToString(grdValues.CumulativeSameDayFee);
                                rowBufferForBankMethod[52] = Convert.ToString(grdValues.CumulativeNSFFee);
                                rowBufferForBankMethod[53] = Convert.ToString(grdValues.CumulativeLateFee);
                                rowBufferForBankMethod[54] = Convert.ToString(grdValues.CumulativeTotalFees);
                                //Past due amount column
                                rowBufferForBankMethod[55] = Convert.ToString(grdValues.PrincipalPastDue);
                                rowBufferForBankMethod[56] = Convert.ToString(grdValues.InterestPastDue);
                                rowBufferForBankMethod[57] = Convert.ToString(grdValues.ServiceFeePastDue);
                                rowBufferForBankMethod[58] = Convert.ToString(grdValues.ServiceFeeInterestPastDue);
                                rowBufferForBankMethod[59] = Convert.ToString(grdValues.OriginationFeePastDue);
                                rowBufferForBankMethod[60] = Convert.ToString(grdValues.MaintenanceFeePastDue);
                                rowBufferForBankMethod[61] = Convert.ToString(grdValues.ManagementFeePastDue);
                                rowBufferForBankMethod[62] = Convert.ToString(grdValues.SameDayFeePastDue);
                                rowBufferForBankMethod[63] = Convert.ToString(grdValues.NSFFeePastDue);
                                rowBufferForBankMethod[64] = Convert.ToString(grdValues.LateFeePastDue);
                                rowBufferForBankMethod[65] = Convert.ToString(grdValues.TotalPastDue);
                                //Cumulative past due amount columns
                                rowBufferForBankMethod[66] = Convert.ToString(grdValues.CumulativePrincipalPastDue);
                                rowBufferForBankMethod[67] = Convert.ToString(grdValues.CumulativeInterestPastDue);
                                rowBufferForBankMethod[68] = Convert.ToString(grdValues.CumulativeServiceFeePastDue);
                                rowBufferForBankMethod[69] = Convert.ToString(grdValues.CumulativeServiceFeeInterestPastDue);
                                rowBufferForBankMethod[70] = Convert.ToString(grdValues.CumulativeOriginationFeePastDue);
                                rowBufferForBankMethod[71] = Convert.ToString(grdValues.CumulativeMaintenanceFeePastDue);
                                rowBufferForBankMethod[72] = Convert.ToString(grdValues.CumulativeManagementFeePastDue);
                                rowBufferForBankMethod[73] = Convert.ToString(grdValues.CumulativeSameDayFeePastDue);
                                rowBufferForBankMethod[74] = Convert.ToString(grdValues.CumulativeNSFFeePastDue);
                                rowBufferForBankMethod[75] = Convert.ToString(grdValues.CumulativeLateFeePastDue);
                                rowBufferForBankMethod[76] = Convert.ToString(grdValues.CumulativeTotalPastDue);

                                rowsForBankMethod.Add(new DataGridViewRow());
                                rowsForBankMethod[rowsForBankMethod.Count - 1].CreateCells(BankMethodGridView, rowBufferForBankMethod);
                            }
                            sbTracing.AppendLine("Fetch the values of all outputValues.Schedule.Count = " + outputValues.Schedule.Count + " rows.");
                            BankMethodGridView.Rows.AddRange(rowsForBankMethod.ToArray());

                            #endregion

                            #region Fill Management Fee Table

                            ManagementFeeTableGrid.Rows.Clear();
                            foreach (var grdValues in outputValues.ManagementFeeAssessment)
                            {
                                if (grdValues.AssessmentDate > outputValues.Schedule[outputValues.Schedule.Count - 1].PaymentDate)
                                {
                                    break;
                                }

                                rowBufferForManagementFeeTable[0] = grdValues.AssessmentDate.ToString("MM/dd/yyyy");
                                rowBufferForManagementFeeTable[1] = Convert.ToString(grdValues.Fee);
                                rowsForManagementFeeTable.Add(new DataGridViewRow());
                                rowsForManagementFeeTable[rowsForManagementFeeTable.Count - 1].CreateCells(ManagementFeeTableGrid, rowBufferForManagementFeeTable);
                            }
                            sbTracing.AppendLine("Fetch the values of all outputValues.ManagementFeeAssessment.Count = " + outputValues.ManagementFeeAssessment.Count + " rows.");
                            ManagementFeeTableGrid.Rows.AddRange(rowsForManagementFeeTable.ToArray());

                            #endregion

                            #region Fill Maintenance Fee Table

                            MaintenanceFeeTableGrid.Rows.Clear();
                            foreach (var grdValues in outputValues.MaintenanceFeeAssessment)
                            {
                                if (grdValues.AssessmentDate > outputValues.Schedule[outputValues.Schedule.Count - 1].PaymentDate)
                                {
                                    break;
                                }

                                rowBufferForMaintenanceFeeTable[0] = grdValues.AssessmentDate.ToString("MM/dd/yyyy");
                                rowBufferForMaintenanceFeeTable[1] = Convert.ToString(grdValues.Fee);
                                rowsForMaintenanceFeeTable.Add(new DataGridViewRow());
                                rowsForMaintenanceFeeTable[rowsForMaintenanceFeeTable.Count - 1].CreateCells(MaintenanceFeeTableGrid, rowBufferForMaintenanceFeeTable);
                            }
                            sbTracing.AppendLine("Fetch the values of all outputValues.MaintenanceFeeAssessment.Count = " + outputValues.MaintenanceFeeAssessment.Count + " rows.");
                            MaintenanceFeeTableGrid.Rows.AddRange(rowsForMaintenanceFeeTable.ToArray());

                            #endregion

                            if (SameAsCashPayOffChb.Checked)
                            {
                                outputValues.PayoffBalance = outputValues.Schedule[outputValues.Schedule.Count - 1].CumulativePayment - outputValues.AmountFinanced;
                            }

                            OriginationFeeCalcTbx.Text = Convert.ToString(outputValues.OriginationFee);
                            LoanAmountCalcTbx.Text = Convert.ToString(outputValues.LoanAmountCalc);
                            AmountFinancedOutputTbx.Text = Convert.ToString(outputValues.AmountFinanced);
                            RegularPaymentTbx.Text = Convert.ToString(outputValues.RegularPayment);
                            CostOfFinancingTbx.Text = Convert.ToString(outputValues.CostOfFinancing);
                            AccruedServiceFeeInterestTbx.Text = Convert.ToString(outputValues.AccruedServiceFeeInterest);
                            AccruedInterestTbx.Text = Convert.ToString(outputValues.AccruedInterest);
                            AccruedPrincipalTbx.Text = Convert.ToString(outputValues.AccruedPrincipal);
                            MaturityDateTbx.Text = outputValues.Schedule[outputValues.Schedule.Count - 1].PaymentDate.ToString("MM/dd/yyyy");
                            PayOffBalanceTbx.Text = Convert.ToString(outputValues.PayoffBalance);
                            ManagementFeeEffectiveTbx.Text = (outputValues.ManagementFeeEffective == DateTime.MinValue ? Convert.ToDateTime(Constants.DefaultDate) :
                                                                                                        outputValues.ManagementFeeEffective).ToString("MM/dd/yyyy");
                            MaintenanceFeeEffectiveTbx.Text = (outputValues.MaintenanceFeeEffective == DateTime.MinValue ? Convert.ToDateTime(Constants.DefaultDate) :
                                                                                                        outputValues.MaintenanceFeeEffective).ToString("MM/dd/yyyy");

                            additionalPayments = additionalPayments.OrderBy(o => o.DateIn).ToList();

                            #region Validation for additional payments to be thrown, when maturity date is less than the date of adding additional payments.
                            for (int i = 0; i < additionalPayments.Count; i++)
                            {
                                if (additionalPayments[i].DateIn > Convert.ToDateTime(MaturityDateTbx.Text))
                                {
                                    if (additionalPayments[i].Flags == (int)Constants.FlagValues.NSFFee)
                                    {
                                        DisplayMessege(ValidationMessage.InvalidNSFFeeDate);
                                    }
                                    else if (additionalPayments[i].Flags == (int)Constants.FlagValues.LateFee)
                                    {
                                        DisplayMessege(ValidationMessage.InvalidLateFeeDate);
                                    }
                                    else if (additionalPayments[i].Flags == (int)Constants.FlagValues.MaintenanceFee)
                                    {
                                        DisplayMessege(ValidationMessage.ValidateMaintenanceFeeDate);
                                    }
                                    else if (additionalPayments[i].Flags == (int)Constants.FlagValues.ManagementFee)
                                    {
                                        DisplayMessege(ValidationMessage.ValidateManagementFeeDate);
                                    }
                                    else
                                    {
                                        DisplayMessege(ValidationMessage.AdditionalPaymentWithoutExtend);
                                    }
                                    BankAdditionalPaymentGridView.Focus();
                                    return;
                                }
                            }
                            #endregion

                            if (additionalPayments.Count > 0 && additionalPayments[additionalPayments.Count - 1].DateIn == Convert.ToDateTime(MaturityDateTbx.Text))
                            {
                                int countOutputGridMaturityDateRows = outputValues.Schedule.Count(o => o.PaymentDate == Convert.ToDateTime(MaturityDateTbx.Text) &&
                                                                                    o.Flags != (int)Constants.FlagValues.Payment);
                                int countAdditionalPaymentGridRow = additionalPayments.Count(o => o.DateIn == Convert.ToDateTime(MaturityDateTbx.Text));
                                if (countAdditionalPaymentGridRow == countOutputGridMaturityDateRows) { }
                                else if (countAdditionalPaymentGridRow > countOutputGridMaturityDateRows ||
                                    (outputValues.Schedule[outputValues.Schedule.Count - 1].Flags != (int)Constants.FlagValues.EarlyPayOff &&
                                    additionalPayments[additionalPayments.Count - 1].Flags != outputValues.Schedule[outputValues.Schedule.Count - 1].Flags))
                                {
                                    if (additionalPayments[additionalPayments.Count - 1].Flags == (int)Constants.FlagValues.NSFFee)
                                    {
                                        DisplayMessege(ValidationMessage.InvalidNSFFeeDate);
                                    }
                                    else if (additionalPayments[additionalPayments.Count - 1].Flags == (int)Constants.FlagValues.LateFee)
                                    {
                                        DisplayMessege(ValidationMessage.InvalidLateFeeDate);
                                    }
                                    else if (additionalPayments[additionalPayments.Count - 1].Flags == (int)Constants.FlagValues.MaintenanceFee)
                                    {
                                        DisplayMessege(ValidationMessage.ValidateMaintenanceFeeDate);
                                    }
                                    else if (additionalPayments[additionalPayments.Count - 1].Flags == (int)Constants.FlagValues.ManagementFee)
                                    {
                                        DisplayMessege(ValidationMessage.ValidateManagementFeeDate);
                                    }
                                    else
                                    {
                                        DisplayMessege(ValidationMessage.AdditionalPaymentWithoutExtend);
                                    }
                                    BankAdditionalPaymentGridView.Focus();
                                    return;
                                }
                            }

                            if ((bankScheduleInput.EarlyPayoffDate.ToShortDateString() != Convert.ToDateTime(Constants.DefaultDate).ToShortDateString()) &&
                                    (outputValues.Schedule[outputValues.Schedule.Count - 1].PaymentDate < bankScheduleInput.EarlyPayoffDate) &&
                                    outputValues.Schedule[outputValues.Schedule.Count - 1].CumulativeTotalPastDue == 0 &&
                                    outputValues.Schedule[outputValues.Schedule.Count - 1].InterestCarryOver == 0 &&
                                    outputValues.Schedule[outputValues.Schedule.Count - 1].serviceFeeInterestCarryOver == 0)
                            {
                                DisplayMessege(ValidationMessage.NoValuesDue);
                                EarlyPayoffDateTbx.Focus();
                            }

                            #endregion

                            #endregion
                            break;
                        case 2:
                            #region Rolling Method
                            sbTracing.AppendLine("calling...Rolling Method LoanType");
                            sbTracing.AppendLine("Inside LoanAmortForm class, and button1_Click() method. Calling method : ValidateInputValues();");
                            if (!ValidateInputValues())
                            {
                                return;
                            }
                            else
                            {
                                #region
                                var rollingScheduleInput = new getScheduleInput();
                                OutputGridView.Visible = false;
                                BankMethodGridView.Visible = true;

                                #region InputGridView
                                for (int i = 0; i < InputGridView.Rows.Count - 1; i++)
                                {
                                    gridRow = InputGridView.Rows[i];
                                    _input.Add(new InputRecord
                                    {
                                        DateIn = Convert.ToDateTime(gridRow.Cells["DateIn"].Value),
                                        Flags = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["Flags"].Value)) ? (int)Constants.FlagValues.Payment : Convert.ToInt32(gridRow.Cells["Flags"].Value),
                                        PaymentID = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["PaymentID"].Value)) ? 0 : Convert.ToInt32(gridRow.Cells["PaymentID"].Value),
                                        EffectiveDate = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["EffectiveDate"].Value)) ? DateTime.MinValue : Convert.ToDateTime(gridRow.Cells["EffectiveDate"].Value),
                                        PaymentAmount = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["PaymentAmountSchedule"].Value)) ? "" : Convert.ToString(Convert.ToDouble(gridRow.Cells["PaymentAmountSchedule"].Value)),
                                        InterestRate = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["InterestRate"].Value)) ? "" : Convert.ToString(gridRow.Cells["InterestRate"].Value),
                                        InterestPriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["InterestPriority"].Value)) ? (IntrestPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.Interest : Convert.ToInt32(IntrestPriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["InterestPriority"].Value),
                                        PrincipalPriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["PrincipalPriority"].Value)) ? (PrincipalPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.Principal : Convert.ToInt32(PrincipalPriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["PrincipalPriority"].Value),
                                        OriginationFeePriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["OriginationFeePriority"].Value)) ? (OriginationFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.OriginationFee : Convert.ToInt32(OriginationFeePriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["OriginationFeePriority"].Value),
                                        SameDayFeePriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["SameDayFeePriority"].Value)) ? (SameDayFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.SameDayFee : Convert.ToInt32(SameDayFeePriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["SameDayFeePriority"].Value),
                                        ServiceFeePriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["ServiceFeePriority"].Value)) ? (ServiceFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ServiceFee : Convert.ToInt32(ServiceFeePriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["ServiceFeePriority"].Value),
                                        ServiceFeeInterestPriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["ServiceFeeInterestPriority"].Value)) ? (ServiceFeeInterestPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ServiceFeeInterest : Convert.ToInt32(ServiceFeeInterestPriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["ServiceFeeInterestPriority"].Value),
                                        ManagementFeePriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["ManagementFeePriority"].Value)) ? (ManagementFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ManagementFee : Convert.ToInt32(ManagementFeePriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["ManagementFeePriority"].Value),
                                        MaintenanceFeePriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["MaintenanceFeePriority"].Value)) ? (MaintenanceFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.MaintenanceFee : Convert.ToInt32(MaintenanceFeePriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["MaintenanceFeePriority"].Value),
                                        NSFFeePriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["NSFFeePriority"].Value)) ? (NSFFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.NSFFee : Convert.ToInt32(NSFFeePriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["NSFFeePriority"].Value),
                                        LateFeePriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["LateFeePriority"].Value)) ? (LateFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.LateFee : Convert.ToInt32(LateFeePriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["LateFeePriority"].Value)
                                    });
                                }

                                #endregion

                                #region Inputs of Additional payment gridview
                                for (int i = 0; i < BankAdditionalPaymentGridView.Rows.Count - 1; i++)
                                {
                                    addGridRow = BankAdditionalPaymentGridView.Rows[i];
                                    _additional_pmt.Add(new AdditionalPaymentRecord
                                    {
                                        DateIn = Convert.ToDateTime(addGridRow.Cells["DateInAdditional"].Value),
                                        AdditionalPayment = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["AdditionalPaymentBank"].Value)) ? 0.0 : Convert.ToDouble(addGridRow.Cells["AdditionalPaymentBank"].Value),
                                        PaymentID = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["PaymentIDAdditionalBank"].Value)) ? 0 : Convert.ToInt32(addGridRow.Cells["PaymentIDAdditionalBank"].Value),
                                        Flags = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["FlagsAdditionalBank"].Value)) ? 0 : Convert.ToInt32(addGridRow.Cells["FlagsAdditionalBank"].Value),
                                        InterestRate = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["InterestRateAdditional"].Value)) ? "" : Convert.ToString(addGridRow.Cells["InterestRateAdditional"].Value),
                                        PrincipalDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["PrincipalAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["PrincipalAdditional"].Value),
                                        InterestDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["InterestAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["InterestAdditional"].Value),
                                        OriginationFeeDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["OriginationFeeAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["OriginationFeeAdditional"].Value),
                                        SameDayFeeDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["SameDayFeeAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["SameDayFeeAdditional"].Value),
                                        ServiceFeeDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["ServiceFeeAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["ServiceFeeAdditional"].Value),
                                        ServiceFeeInterestDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["ServiceFeeInterestAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["ServiceFeeInterestAdditional"].Value),
                                        ManagementFeeDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["ManagementFeeAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["ManagementFeeAdditional"].Value),
                                        MaintenanceFeeDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["MaintenanceFeeAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["MaintenanceFeeAdditional"].Value),
                                        NSFFeeDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["NSFFeeAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["NSFFeeAdditional"].Value),
                                        LateFeeDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["LateFeeAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["LateFeeAdditional"].Value),
                                        InterestPriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["InterestPriorityAdditional"].Value)) ? (IntrestPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.Interest : Convert.ToInt32(IntrestPriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["InterestPriorityAdditional"].Value),
                                        PrincipalPriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["PrincipalPriorityAdditional"].Value)) ? (PrincipalPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.Principal : Convert.ToInt32(PrincipalPriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["PrincipalPriorityAdditional"].Value),
                                        OriginationFeePriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["OriginationFeePriorityAdditional"].Value)) ? (OriginationFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.OriginationFee : Convert.ToInt32(OriginationFeePriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["OriginationFeePriorityAdditional"].Value),
                                        SameDayFeePriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["SameDayFeePriorityAdditional"].Value)) ? (SameDayFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.SameDayFee : Convert.ToInt32(SameDayFeePriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["SameDayFeePriorityAdditional"].Value),
                                        ServiceFeePriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["ServiceFeePriorityAdditional"].Value)) ? (ServiceFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ServiceFee : Convert.ToInt32(ServiceFeePriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["ServiceFeePriorityAdditional"].Value),
                                        ServiceFeeInterestPriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["ServiceFeeInterestPriorityAdditional"].Value)) ? (ServiceFeeInterestPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ServiceFeeInterest : Convert.ToInt32(ServiceFeeInterestPriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["ServiceFeeInterestPriorityAdditional"].Value),
                                        ManagementFeePriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["ManagementFeePriorityAdditional"].Value)) ? (ManagementFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ManagementFee : Convert.ToInt32(ManagementFeePriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["ManagementFeePriorityAdditional"].Value),
                                        MaintenanceFeePriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["MaintenanceFeePriorityAdditional"].Value)) ? (MaintenanceFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.MaintenanceFee : Convert.ToInt32(MaintenanceFeePriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["MaintenanceFeePriorityAdditional"].Value),
                                        NSFFeePriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["NSFFeePriorityAdditional"].Value)) ? (NSFFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.NSFFee : Convert.ToInt32(NSFFeePriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["NSFFeePriorityAdditional"].Value),
                                        LateFeePriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["LateFeePriorityAdditional"].Value)) ? (LateFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.LateFee : Convert.ToInt32(LateFeePriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["LateFeePriorityAdditional"].Value)
                                    });
                                }
                                #endregion

                                #region Initianlize the rollingScheduleInput Object
                                sbTracing.AppendLine("Initianlize the rollingScheduleInput Object");

                                rollingScheduleInput = new getScheduleInput
                                {
                                    #region
                                    InputRecords = _input,
                                    AdditionalPaymentRecords = _additional_pmt,
                                    BalloonPayment = string.IsNullOrEmpty(BalloonPaymentTbx.Text) ? 0 : Convert.ToDouble(BalloonPaymentTbx.Text),
                                    LoanAmount = string.IsNullOrEmpty(AmountTbx.Text) ? 0 : Convert.ToDouble(AmountTbx.Text),
                                    InterestDelay = string.IsNullOrEmpty(InterestDelayTbx.Text) ? 0 : Convert.ToInt32(InterestDelayTbx.Text),
                                    Residual = string.IsNullOrEmpty(ResidualTbx.Text) ? 0 : Convert.ToDouble(ResidualTbx.Text),
                                    PmtPeriod = PmtPeriod,
                                    EarlyPayoffDate = Convert.ToDateTime(string.IsNullOrEmpty(EarlyPayoffDateTbx.Text) ? Constants.DefaultDate : EarlyPayoffDateTbx.Text),
                                    MinDuration = string.IsNullOrEmpty(MinDurationTbx.Text) ? 0 : Convert.ToInt32(MinDurationTbx.Text),
                                    AfterPayment = AfterPaymentChb.Checked,
                                    AccruedInterestDate = Convert.ToDateTime(string.IsNullOrEmpty(AccruedInterestDateTbx.Text) ? Constants.DefaultDate : AccruedInterestDateTbx.Text),
                                    ServiceFee = Convert.ToDouble(string.IsNullOrEmpty(ServiceFeeTbx.Text) ? "0.0" : ServiceFeeTbx.Text),
                                    ServiceFeeFirstPayment = ServiceFeeFirstPaymentChb.Checked,
                                    ManagementFee = Convert.ToDouble(string.IsNullOrEmpty(ManagementFeeTbx.Text) ? "0.0" : ManagementFeeTbx.Text),
                                    ManagementFeePercent = Convert.ToDouble(string.IsNullOrEmpty(ManagementFeePercentTbx.Text) ? "0.0" : ManagementFeePercentTbx.Text),
                                    ManagementFeeBasis = ManagementFeeBasisCmb.SelectedIndex,
                                    ManagementFeeFrequency = ManagementFeeFreqCmb.SelectedIndex,
                                    ManagementFeeFrequencyNumber = Convert.ToInt32(string.IsNullOrEmpty(ManagementFeeFreqNumTbx.Text) ? "0" : ManagementFeeFreqNumTbx.Text),
                                    ManagementFeeDelay = Convert.ToInt32(string.IsNullOrEmpty(ManagementFeeDelayTbx.Text) ? "0" : ManagementFeeDelayTbx.Text),
                                    ManagementFeeMin = Convert.ToDouble(string.IsNullOrEmpty(ManagementFeeMinTbx.Text) ? "0.0" : ManagementFeeMinTbx.Text),
                                    ManagementFeeMaxPer = Convert.ToDouble(string.IsNullOrEmpty(ManagementFeeMaxPerTbx.Text) ? "0.0" : ManagementFeeMaxPerTbx.Text),
                                    ManagementFeeMaxMonth = Convert.ToDouble(string.IsNullOrEmpty(ManagementFeeMaxMonthTbx.Text) ? "0.0" : ManagementFeeMaxMonthTbx.Text),
                                    ManagementFeeMaxLoan = Convert.ToDouble(string.IsNullOrEmpty(ManagementFeeMaxLoanTbx.Text) ? "0.0" : ManagementFeeMaxLoanTbx.Text),
                                    IsManagementFeeGreater = ManagementFeeGreaterChb.Checked,
                                    MaintenanceFee = Convert.ToDouble(string.IsNullOrEmpty(MaintenanceFeeTbx.Text) ? "0.0" : MaintenanceFeeTbx.Text),
                                    MaintenanceFeePercent = Convert.ToDouble(string.IsNullOrEmpty(MaintenanceFeePercentTbx.Text) ? "0.0" : MaintenanceFeePercentTbx.Text),
                                    MaintenanceFeeBasis = MaintenanceFeeBasisCmb.SelectedIndex,
                                    MaintenanceFeeFrequency = MaintenanceFeeFreqCmb.SelectedIndex,
                                    MaintenanceFeeFrequencyNumber = Convert.ToInt32(string.IsNullOrEmpty(MaintenanceFeeFreqNumTbx.Text) ? "0" : MaintenanceFeeFreqNumTbx.Text),
                                    MaintenanceFeeDelay = Convert.ToInt32(string.IsNullOrEmpty(MaintenanceFeeDelayTbx.Text) ? "0" : MaintenanceFeeDelayTbx.Text),
                                    MaintenanceFeeMin = Convert.ToDouble(string.IsNullOrEmpty(MaintenanceFeeMinTbx.Text) ? "0.0" : MaintenanceFeeMinTbx.Text),
                                    MaintenanceFeeMaxPer = Convert.ToDouble(string.IsNullOrEmpty(MaintenanceFeeMaxPerTbx.Text) ? "0.0" : MaintenanceFeeMaxPerTbx.Text),
                                    MaintenanceFeeMaxMonth = Convert.ToDouble(string.IsNullOrEmpty(MaintenanceFeeMaxMonthTbx.Text) ? "0.0" : MaintenanceFeeMaxMonthTbx.Text),
                                    MaintenanceFeeMaxLoan = Convert.ToDouble(string.IsNullOrEmpty(MaintenanceFeeMaxLoanTbx.Text) ? "0.0" : MaintenanceFeeMaxLoanTbx.Text),
                                    IsMaintenanceFeeGreater = MaintenanceFeeGreaterChb.Checked,
                                    RecastAdditionalPayments = RecastAdditionalPaymentsChb.Checked,
                                    UseFlexibleCalculation = UseFlexibleMethod.Checked,
                                    DaysInYearBankMethod = Convert.ToString(DaysInYearCmb.Text),
                                    LoanType = Convert.ToString(LoanTypeCmb.Text),
                                    DaysInMonth = Convert.ToString(DaysInMonthCmb.Text),
                                    ApplyServiceFeeInterest = ServiceFeeCmb.SelectedIndex,
                                    IsServiceFeeFinanced = FinanceServiceFeeChb.Checked,
                                    IsServiceFeeIncremental = ServiceFeeIncrementalChb.Checked,
                                    SameDayFee = Convert.ToDouble(string.IsNullOrEmpty(SameDayFeeTbx.Text) ? "0.0" : SameDayFeeTbx.Text),
                                    SameDayFeeCalculationMethod = string.IsNullOrEmpty(SameDayFeeCmb.Text) ? "Fixed" : Convert.ToString(SameDayFeeCmb.Text),
                                    IsSameDayFeeFinanced = FinanceSameDayFeeChb.Checked,
                                    OriginationFee = Convert.ToDouble(string.IsNullOrEmpty(OriginationFeeFixedTbx.Text) ? "0.0" : OriginationFeeFixedTbx.Text),
                                    OriginationFeePercent = Convert.ToDouble(string.IsNullOrEmpty(OriginationFeePercentTbx.Text) ? "0.0" : OriginationFeePercentTbx.Text),
                                    OriginationFeeMax = Convert.ToDouble(string.IsNullOrEmpty(OriginationFeeMaxTbx.Text) ? "0.0" : OriginationFeeMaxTbx.Text),
                                    OriginationFeeMin = Convert.ToDouble(string.IsNullOrEmpty(OriginationFeeMinTbx.Text) ? "0.0" : OriginationFeeMinTbx.Text),
                                    OriginationFeeCalculationMethod = OriginationFeeGreaterChb.Checked,
                                    IsOriginationFeeFinanced = FinanceOriginationFeeChb.Checked,
                                    AmountFinanced = string.IsNullOrEmpty(AmountFinancedTbx.Text) ? 0 : Convert.ToDouble(AmountFinancedTbx.Text),
                                    OriginationFeePriority = OriginationFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.OriginationFee : Convert.ToInt32(OriginationFeePriorityCmb.Text),
                                    SameDayFeePriority = SameDayFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.SameDayFee : Convert.ToInt32(SameDayFeePriorityCmb.Text),
                                    InterestPriority = IntrestPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.Interest : Convert.ToInt32(IntrestPriorityCmb.Text),
                                    PrincipalPriority = PrincipalPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.Principal : Convert.ToInt32(PrincipalPriorityCmb.Text),
                                    ServiceFeeInterestPriority = ServiceFeeInterestPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ServiceFeeInterest : Convert.ToInt32(ServiceFeeInterestPriorityCmb.Text),
                                    ServiceFeePriority = ServiceFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ServiceFee : Convert.ToInt32(ServiceFeePriorityCmb.Text),
                                    ManagementFeePriority = ManagementFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ManagementFee : Convert.ToInt32(ManagementFeePriorityCmb.Text),
                                    MaintenanceFeePriority = MaintenanceFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.MaintenanceFee : Convert.ToInt32(MaintenanceFeePriorityCmb.Text),
                                    LateFeePriority = LateFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.LateFee : Convert.ToInt32(LateFeePriorityCmb.Text),
                                    NSFFeePriority = NSFFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.NSFFee : Convert.ToInt32(NSFFeePriorityCmb.Text),
                                    AccruedServiceFeeInterestDate = Convert.ToDateTime(string.IsNullOrEmpty(AccruedServiceFeeInterestDateTbx.Text) ? Constants.DefaultDate : AccruedServiceFeeInterestDateTbx.Text),
                                    SameAsCashPayoff = SameAsCashPayOffChb.Checked,
                                    PaymentAmount = string.IsNullOrEmpty(PaymentAmountTbx.Text) ? 0 : Convert.ToDouble(PaymentAmountTbx.Text),
                                    EarnedInterest = string.IsNullOrEmpty(EarnedInterestTbx.Text) ? 0 : Convert.ToDouble(EarnedInterestTbx.Text),
                                    EarnedServiceFeeInterest = string.IsNullOrEmpty(EarnedServiceFeeInterestTbx.Text) ? 0 : Convert.ToDouble(EarnedServiceFeeInterestTbx.Text),
                                    EarnedOriginationFee = string.IsNullOrEmpty(EarnedOriginationFeeTbx.Text) ? 0 : Convert.ToDouble(EarnedOriginationFeeTbx.Text),
                                    EarnedSameDayFee = string.IsNullOrEmpty(EarnedSameDayFeeTbx.Text) ? 0 : Convert.ToDouble(EarnedSameDayFeeTbx.Text),
                                    EarnedManagementFee = string.IsNullOrEmpty(EarnedManagementFeeTbx.Text) ? 0 : Convert.ToDouble(EarnedManagementFeeTbx.Text),
                                    EarnedMaintenanceFee = string.IsNullOrEmpty(EarnedMaintenanceFeeTbx.Text) ? 0 : Convert.ToDouble(EarnedMaintenanceFeeTbx.Text),
                                    EarnedNSFFee = string.IsNullOrEmpty(EarnedNSFFeeTbx.Text) ? 0 : Convert.ToDouble(EarnedNSFFeeTbx.Text),
                                    EarnedLateFee = string.IsNullOrEmpty(EarnedLateFeeTbx.Text) ? 0 : Convert.ToDouble(EarnedLateFeeTbx.Text),
                                    InterestRate = string.IsNullOrEmpty(InterestRateTbx.Text) ? 0 : Convert.ToDouble(InterestRateTbx.Text),
                                    Tier1 = string.IsNullOrEmpty(InterestTier1Tbx.Text) ? 0 : Convert.ToDouble(InterestTier1Tbx.Text),
                                    InterestRate2 = string.IsNullOrEmpty(InterestRate2Tbx.Text) ? 0 : Convert.ToDouble(InterestRate2Tbx.Text),
                                    Tier2 = string.IsNullOrEmpty(InterestTier2Tbx.Text) ? 0 : Convert.ToDouble(InterestTier2Tbx.Text),
                                    InterestRate3 = string.IsNullOrEmpty(InterestRate3Tbx.Text) ? 0 : Convert.ToDouble(InterestRate3Tbx.Text),
                                    Tier3 = string.IsNullOrEmpty(InterestTier3Tbx.Text) ? 0 : Convert.ToDouble(InterestTier3Tbx.Text),
                                    InterestRate4 = string.IsNullOrEmpty(InterestRate4Tbx.Text) ? 0 : Convert.ToDouble(InterestRate4Tbx.Text),
                                    Tier4 = string.IsNullOrEmpty(InterestTier4Tbx.Text) ? 0 : Convert.ToDouble(InterestTier4Tbx.Text),
                                    EnforcedPrincipal = string.IsNullOrEmpty(EnforcedPrincipalTbx.Text) ? 0 : Convert.ToDouble(EnforcedPrincipalTbx.Text),
                                    EnforcedPayment = string.IsNullOrEmpty(EnforcedPaymentTbx.Text) ? 0 : Convert.ToInt32(EnforcedPaymentTbx.Text)
                                    #endregion
                                };
                                #endregion

                                #region Validate Input and Additional payment input grid

                                //Sort the input grid records and additioanl payment grid records.
                                rollingScheduleInput.InputRecords.Sort();
                                rollingScheduleInput.AdditionalPaymentRecords = rollingScheduleInput.AdditionalPaymentRecords.OrderBy(o => o.DateIn).ToList();
                                for (int i = 0; i < rollingScheduleInput.InputRecords.Count; i++)
                                {
                                    if (rollingScheduleInput.InputRecords.FindLastIndex(o => o.DateIn == rollingScheduleInput.InputRecords[i].DateIn) != i)
                                    {

                                        DisplayMessege(ValidationMessage.RepeatedDateInInputGrid);
                                        InputGridView.Focus();
                                        return;
                                    }
                                    if ((rollingScheduleInput.InputRecords.FindLastIndex(o => o.EffectiveDate == rollingScheduleInput.InputRecords[i].EffectiveDate) != i) &&
                                        (rollingScheduleInput.InputRecords[i].EffectiveDate != DateTime.MinValue))
                                    {
                                        BankMethodGridView.Rows.Clear();
                                        DisplayMessege(ValidationMessage.RepeatedDateInInputGrid);
                                        InputGridView.Focus();
                                        return;
                                    }
                                    if ((rollingScheduleInput.InputRecords.FindLastIndex(o => o.DateIn == rollingScheduleInput.InputRecords[i].EffectiveDate) != -1) &&
                                        (rollingScheduleInput.InputRecords.FindLastIndex(o => o.DateIn == rollingScheduleInput.InputRecords[i].EffectiveDate) != i) &&
                                        (rollingScheduleInput.InputRecords[i].EffectiveDate != DateTime.MinValue))
                                    {
                                        BankMethodGridView.Rows.Clear();
                                        DisplayMessege(ValidationMessage.RepeatedDateInInputGrid);
                                        InputGridView.Focus();
                                        return;
                                    }
                                    if (i > 0 && rollingScheduleInput.InputRecords[i].EffectiveDate != DateTime.MinValue &&
                                        ((rollingScheduleInput.InputRecords[i].EffectiveDate <= (rollingScheduleInput.InputRecords[i - 1].EffectiveDate == DateTime.MinValue ? rollingScheduleInput.InputRecords[i - 1].DateIn : rollingScheduleInput.InputRecords[i - 1].EffectiveDate)) ||
                                        (i < (rollingScheduleInput.InputRecords.Count - 1) && (rollingScheduleInput.InputRecords[i].EffectiveDate >= (rollingScheduleInput.InputRecords[i + 1].EffectiveDate == DateTime.MinValue ? rollingScheduleInput.InputRecords[i + 1].DateIn : rollingScheduleInput.InputRecords[i + 1].EffectiveDate)))))
                                    {
                                        BankMethodGridView.Rows.Clear();
                                        DisplayMessege(ValidationMessage.DateRangeInInputGrid);
                                        InputGridView.Focus();
                                        return;
                                    }
                                    if (rollingScheduleInput.InputRecords.FindLastIndex(o => o.PaymentID == rollingScheduleInput.InputRecords[i].PaymentID) != i ||
                                            rollingScheduleInput.AdditionalPaymentRecords.FindIndex(o => o.PaymentID == rollingScheduleInput.InputRecords[i].PaymentID) != -1)
                                    {
                                        DisplayMessege(ValidationMessage.RepeatedPaymentId);
                                        InputGridView.Focus();
                                        return;
                                    }
                                }

                                if ((!string.IsNullOrEmpty(ManagementFeeTbx.Text) && Convert.ToDouble(ManagementFeeTbx.Text) > 0 || !string.IsNullOrEmpty(ManagementFeePercentTbx.Text) && Convert.ToDouble(ManagementFeePercentTbx.Text) > 0) && (rollingScheduleInput.AdditionalPaymentRecords.FindIndex(x => x.Flags == (int)Constants.FlagValues.ManagementFee && x.AdditionalPayment > 0)) > -1)
                                {
                                    DisplayMessege(ValidationMessage.ManagementFeeFromTwoPlaces);
                                    BankAdditionalPaymentGridView.Focus();
                                    return;
                                }
                                if ((!string.IsNullOrEmpty(MaintenanceFeeTbx.Text) && Convert.ToDouble(MaintenanceFeeTbx.Text) > 0 || !string.IsNullOrEmpty(MaintenanceFeePercentTbx.Text) && Convert.ToDouble(MaintenanceFeePercentTbx.Text) > 0) && (rollingScheduleInput.AdditionalPaymentRecords.FindIndex(x => x.Flags == (int)Constants.FlagValues.MaintenanceFee && x.AdditionalPayment > 0)) > -1)
                                {
                                    DisplayMessege(ValidationMessage.MaintenanceFeeFromTwoPlaces);
                                    BankAdditionalPaymentGridView.Focus();
                                    return;
                                }
                                if (rollingScheduleInput.InputRecords[rollingScheduleInput.InputRecords.Count - 1].Flags == (int)Constants.FlagValues.SkipPayment)
                                {
                                    DisplayMessege(ValidationMessage.SkippPayment);
                                    InputGridView.Focus();
                                    return;
                                }
                                else if (rollingScheduleInput.AdditionalPaymentRecords.FindIndex(o => o.DateIn <= rollingScheduleInput.InputRecords[0].DateIn) != -1)
                                {
                                    DisplayMessege(ValidationMessage.Additionalpaymentdate);
                                    BankAdditionalPaymentGridView.Focus();
                                    return;
                                }
                                else if (rollingScheduleInput.AdditionalPaymentRecords.FindIndex(o => o.DateIn > rollingScheduleInput.InputRecords[rollingScheduleInput.InputRecords.Count - 1].DateIn &&
                                         (o.Flags == (int)Constants.FlagValues.NSFFee || o.Flags == (int)Constants.FlagValues.LateFee)) != -1)
                                {
                                    DisplayMessege(ValidationMessage.NSFAndLateFeeValidation);
                                    BankAdditionalPaymentGridView.Focus();
                                    return;
                                }
                                else
                                {
                                    for (int i = 0; i < rollingScheduleInput.AdditionalPaymentRecords.Count; i++)
                                    {
                                        if (rollingScheduleInput.AdditionalPaymentRecords.FindLastIndex(o => o.PaymentID == rollingScheduleInput.AdditionalPaymentRecords[i].PaymentID) != i)
                                        {
                                            DisplayMessege(ValidationMessage.RepeatedPaymentId);
                                            BankAdditionalPaymentGridView.Focus();
                                            return;
                                        }
                                    }
                                }

                                #endregion

                                sbTracing.AppendLine("Inside LoanAmortForm class, and button1_Click() method. Calling method : RollingBusiness.CreateScheduleForRollingMethod.DefaultSchedule(bankScheduleInput);");
                                _output = RollingBusiness.CreateScheduleForRollingMethod.DefaultSchedule(rollingScheduleInput);


                                if (_output.LoanAmountCalc <= rollingScheduleInput.Residual)
                                {
                                    DisplayMessege(ValidationMessage.ValidateResidualAmount);
                                    ResidualTbx.Focus();
                                    return;
                                }
                                else if (_output.LoanAmountCalc <= rollingScheduleInput.BalloonPayment)
                                {
                                    DisplayMessege(ValidationMessage.ValidateBalloonPaymentWithLoanAmount);
                                    ResidualTbx.Focus();
                                    return;
                                }

                                #region Fill Output Grid

                                BankMethodGridView.Rows.Clear();
                                object[] rowBufferForRollingMethod = new object[77];
                                var rowsForRollingMethod = new List<DataGridViewRow>();
                                foreach (var grdValues in _output.Schedule)
                                {
                                    rowBufferForRollingMethod[0] = grdValues.PaymentDate.ToString("MM/dd/yyyy");
                                    rowBufferForRollingMethod[1] = Convert.ToString(grdValues.BeginningPrincipal);
                                    rowBufferForRollingMethod[2] = Convert.ToString(grdValues.BeginningServiceFee);
                                    rowBufferForRollingMethod[3] = Convert.ToString(grdValues.BeginningPrincipalServiceFee);
                                    rowBufferForRollingMethod[4] = Convert.ToString(grdValues.PeriodicInterestRate);
                                    rowBufferForRollingMethod[5] = Convert.ToString(grdValues.DailyInterestRate);
                                    rowBufferForRollingMethod[6] = Convert.ToString(grdValues.DailyInterestAmount);
                                    rowBufferForRollingMethod[7] = Convert.ToString(grdValues.PaymentID);
                                    rowBufferForRollingMethod[8] = Convert.ToString(grdValues.Flags);
                                    rowBufferForRollingMethod[9] = grdValues.DueDate.ToString("MM/dd/yyyy");
                                    rowBufferForRollingMethod[10] = Convert.ToString(grdValues.InterestAccrued);
                                    rowBufferForRollingMethod[11] = Convert.ToString(grdValues.InterestDue);
                                    rowBufferForRollingMethod[12] = Convert.ToString(grdValues.InterestCarryOver);
                                    //Amount to be paid in the particular period columns
                                    rowBufferForRollingMethod[13] = Convert.ToString(grdValues.InterestPayment);
                                    rowBufferForRollingMethod[14] = Convert.ToString(grdValues.PrincipalPayment);
                                    rowBufferForRollingMethod[15] = Convert.ToString(grdValues.PaymentDue);
                                    rowBufferForRollingMethod[16] = Convert.ToString(grdValues.TotalPayment);
                                    rowBufferForRollingMethod[17] = Convert.ToString(grdValues.InterestServiceFeeInterestPayment);
                                    rowBufferForRollingMethod[18] = Convert.ToString(grdValues.PrincipalServiceFeePayment);
                                    rowBufferForRollingMethod[19] = Convert.ToString(grdValues.AccruedServiceFeeInterest);
                                    rowBufferForRollingMethod[20] = Convert.ToString(grdValues.ServiceFeeInterestDue);
                                    rowBufferForRollingMethod[21] = Convert.ToString(grdValues.AccruedServiceFeeInterestCarryOver);
                                    rowBufferForRollingMethod[22] = Convert.ToString(grdValues.ServiceFeeInterest);
                                    rowBufferForRollingMethod[23] = Convert.ToString(grdValues.ServiceFee);
                                    rowBufferForRollingMethod[24] = Convert.ToString(grdValues.ServiceFeeTotal);
                                    rowBufferForRollingMethod[25] = Convert.ToString(grdValues.OriginationFee);
                                    rowBufferForRollingMethod[26] = Convert.ToString(grdValues.MaintenanceFee);
                                    rowBufferForRollingMethod[27] = Convert.ToString(grdValues.ManagementFee);
                                    rowBufferForRollingMethod[28] = Convert.ToString(grdValues.SameDayFee);
                                    rowBufferForRollingMethod[29] = Convert.ToString(grdValues.NSFFee);
                                    rowBufferForRollingMethod[30] = Convert.ToString(grdValues.LateFee);
                                    //Paid amount columns
                                    rowBufferForRollingMethod[31] = Convert.ToString(grdValues.PrincipalPaid);
                                    rowBufferForRollingMethod[32] = Convert.ToString(grdValues.InterestPaid);
                                    rowBufferForRollingMethod[33] = Convert.ToString(grdValues.ServiceFeePaid);
                                    rowBufferForRollingMethod[34] = Convert.ToString(grdValues.ServiceFeeInterestPaid);
                                    rowBufferForRollingMethod[35] = Convert.ToString(grdValues.OriginationFeePaid);
                                    rowBufferForRollingMethod[36] = Convert.ToString(grdValues.MaintenanceFeePaid);
                                    rowBufferForRollingMethod[37] = Convert.ToString(grdValues.ManagementFeePaid);
                                    rowBufferForRollingMethod[38] = Convert.ToString(grdValues.SameDayFeePaid);
                                    rowBufferForRollingMethod[39] = Convert.ToString(grdValues.NSFFeePaid);
                                    rowBufferForRollingMethod[40] = Convert.ToString(grdValues.LateFeePaid);
                                    rowBufferForRollingMethod[41] = Convert.ToString(grdValues.TotalPaid);
                                    //Cumulative Amount paid column
                                    rowBufferForRollingMethod[42] = Convert.ToString(grdValues.CumulativeInterest);
                                    rowBufferForRollingMethod[43] = Convert.ToString(grdValues.CumulativePrincipal);
                                    rowBufferForRollingMethod[44] = Convert.ToString(grdValues.CumulativePayment);
                                    rowBufferForRollingMethod[45] = Convert.ToString(grdValues.CumulativeServiceFee);
                                    rowBufferForRollingMethod[46] = Convert.ToString(grdValues.CumulativeServiceFeeInterest);
                                    rowBufferForRollingMethod[47] = Convert.ToString(grdValues.CumulativeServiceFeeTotal);
                                    rowBufferForRollingMethod[48] = Convert.ToString(grdValues.CumulativeOriginationFee);
                                    rowBufferForRollingMethod[49] = Convert.ToString(grdValues.CumulativeMaintenanceFee);
                                    rowBufferForRollingMethod[50] = Convert.ToString(grdValues.CumulativeManagementFee);
                                    rowBufferForRollingMethod[51] = Convert.ToString(grdValues.CumulativeSameDayFee);
                                    rowBufferForRollingMethod[52] = Convert.ToString(grdValues.CumulativeNSFFee);
                                    rowBufferForRollingMethod[53] = Convert.ToString(grdValues.CumulativeLateFee);
                                    rowBufferForRollingMethod[54] = Convert.ToString(grdValues.CumulativeTotalFees);
                                    //Past due amount column
                                    rowBufferForRollingMethod[55] = Convert.ToString(grdValues.PrincipalPastDue);
                                    rowBufferForRollingMethod[56] = Convert.ToString(grdValues.InterestPastDue);
                                    rowBufferForRollingMethod[57] = Convert.ToString(grdValues.ServiceFeePastDue);
                                    rowBufferForRollingMethod[58] = Convert.ToString(grdValues.ServiceFeeInterestPastDue);
                                    rowBufferForRollingMethod[59] = Convert.ToString(grdValues.OriginationFeePastDue);
                                    rowBufferForRollingMethod[60] = Convert.ToString(grdValues.MaintenanceFeePastDue);
                                    rowBufferForRollingMethod[61] = Convert.ToString(grdValues.ManagementFeePastDue);
                                    rowBufferForRollingMethod[62] = Convert.ToString(grdValues.SameDayFeePastDue);
                                    rowBufferForRollingMethod[63] = Convert.ToString(grdValues.NSFFeePastDue);
                                    rowBufferForRollingMethod[64] = Convert.ToString(grdValues.LateFeePastDue);
                                    rowBufferForRollingMethod[65] = Convert.ToString(grdValues.TotalPastDue);
                                    //Cumulative past due amount columns
                                    rowBufferForRollingMethod[66] = Convert.ToString(grdValues.CumulativePrincipalPastDue);
                                    rowBufferForRollingMethod[67] = Convert.ToString(grdValues.CumulativeInterestPastDue);
                                    rowBufferForRollingMethod[68] = Convert.ToString(grdValues.CumulativeServiceFeePastDue);
                                    rowBufferForRollingMethod[69] = Convert.ToString(grdValues.CumulativeServiceFeeInterestPastDue);
                                    rowBufferForRollingMethod[70] = Convert.ToString(grdValues.CumulativeOriginationFeePastDue);
                                    rowBufferForRollingMethod[71] = Convert.ToString(grdValues.CumulativeMaintenanceFeePastDue);
                                    rowBufferForRollingMethod[72] = Convert.ToString(grdValues.CumulativeManagementFeePastDue);
                                    rowBufferForRollingMethod[73] = Convert.ToString(grdValues.CumulativeSameDayFeePastDue);
                                    rowBufferForRollingMethod[74] = Convert.ToString(grdValues.CumulativeNSFFeePastDue);
                                    rowBufferForRollingMethod[75] = Convert.ToString(grdValues.CumulativeLateFeePastDue);
                                    rowBufferForRollingMethod[76] = Convert.ToString(grdValues.CumulativeTotalPastDue);

                                    rowsForRollingMethod.Add(new DataGridViewRow());
                                    rowsForRollingMethod[rowsForRollingMethod.Count - 1].CreateCells(BankMethodGridView, rowBufferForRollingMethod);
                                }
                                sbTracing.AppendLine("Fetch the values of all _output.Schedule.Count = " + _output.Schedule.Count + " rows.");
                                BankMethodGridView.Rows.AddRange(rowsForRollingMethod.ToArray());

                                #endregion

                                #region Fill Management Fee Table

                                ManagementFeeTableGrid.Rows.Clear();
                                foreach (var grdValues in _output.ManagementFeeAssessment)
                                {
                                    if (grdValues.AssessmentDate > _output.Schedule[_output.Schedule.Count - 1].PaymentDate)
                                    {
                                        break;
                                    }

                                    rowBufferForManagementFeeTable[0] = grdValues.AssessmentDate.ToString("MM/dd/yyyy");
                                    rowBufferForManagementFeeTable[1] = Convert.ToString(grdValues.Fee);
                                    rowsForManagementFeeTable.Add(new DataGridViewRow());
                                    rowsForManagementFeeTable[rowsForManagementFeeTable.Count - 1].CreateCells(ManagementFeeTableGrid, rowBufferForManagementFeeTable);
                                }
                                sbTracing.AppendLine("Fetch the values of all _output.ManagementFeeAssessment.Count = " + _output.ManagementFeeAssessment.Count + " rows.");
                                ManagementFeeTableGrid.Rows.AddRange(rowsForManagementFeeTable.ToArray());

                                #endregion

                                #region Fill Maintenance Fee Table

                                MaintenanceFeeTableGrid.Rows.Clear();
                                foreach (var grdValues in _output.MaintenanceFeeAssessment)
                                {
                                    if (grdValues.AssessmentDate > _output.Schedule[_output.Schedule.Count - 1].PaymentDate)
                                    {
                                        break;
                                    }

                                    rowBufferForMaintenanceFeeTable[0] = grdValues.AssessmentDate.ToString("MM/dd/yyyy");
                                    rowBufferForMaintenanceFeeTable[1] = Convert.ToString(grdValues.Fee);
                                    rowsForMaintenanceFeeTable.Add(new DataGridViewRow());
                                    rowsForMaintenanceFeeTable[rowsForMaintenanceFeeTable.Count - 1].CreateCells(MaintenanceFeeTableGrid, rowBufferForMaintenanceFeeTable);
                                }
                                sbTracing.AppendLine("Fetch the values of all _output.MaintenanceFeeAssessment.Count = " + _output.MaintenanceFeeAssessment.Count + " rows.");
                                MaintenanceFeeTableGrid.Rows.AddRange(rowsForMaintenanceFeeTable.ToArray());

                                #endregion

                                if (SameAsCashPayOffChb.Checked)
                                {
                                    _output.PayoffBalance = _output.Schedule[_output.Schedule.Count - 1].CumulativePayment - _output.AmountFinanced;
                                }

                                OriginationFeeCalcTbx.Text = Convert.ToString(_output.OriginationFee);
                                LoanAmountCalcTbx.Text = Convert.ToString(_output.LoanAmountCalc);
                                AmountFinancedOutputTbx.Text = Convert.ToString(_output.AmountFinanced);
                                RegularPaymentTbx.Text = Convert.ToString(_output.RegularPayment);
                                CostOfFinancingTbx.Text = Convert.ToString(_output.CostOfFinancing);
                                AccruedServiceFeeInterestTbx.Text = Convert.ToString(_output.AccruedServiceFeeInterest);
                                AccruedInterestTbx.Text = Convert.ToString(_output.AccruedInterest);
                                AccruedPrincipalTbx.Text = Convert.ToString(_output.AccruedPrincipal);
                                MaturityDateTbx.Text = _output.Schedule[_output.Schedule.Count - 1].PaymentDate.ToString("MM/dd/yyyy");
                                PayOffBalanceTbx.Text = Convert.ToString(_output.PayoffBalance);
                                ManagementFeeEffectiveTbx.Text = (_output.ManagementFeeEffective == DateTime.MinValue ? Convert.ToDateTime(Constants.DefaultDate) :
                                                                                                            _output.ManagementFeeEffective).ToString("MM/dd/yyyy");
                                MaintenanceFeeEffectiveTbx.Text = (_output.MaintenanceFeeEffective == DateTime.MinValue ? Convert.ToDateTime(Constants.DefaultDate) :
                                                                                                            _output.MaintenanceFeeEffective).ToString("MM/dd/yyyy");

                                _additional_pmt = _additional_pmt.OrderBy(o => o.DateIn).ToList();

                                #region Validation for additional payments to be thrown, when maturity date is less than the date of adding additional payments.
                                for (int i = 0; i < _additional_pmt.Count; i++)
                                {
                                    if (_additional_pmt[i].DateIn > Convert.ToDateTime(MaturityDateTbx.Text))
                                    {
                                        if (_additional_pmt[i].Flags == (int)Constants.FlagValues.NSFFee)
                                        {
                                            DisplayMessege(ValidationMessage.InvalidNSFFeeDate);
                                        }
                                        else if (_additional_pmt[i].Flags == (int)Constants.FlagValues.LateFee)
                                        {
                                            DisplayMessege(ValidationMessage.InvalidLateFeeDate);
                                        }
                                        else if (_additional_pmt[i].Flags == (int)Constants.FlagValues.MaintenanceFee)
                                        {
                                            DisplayMessege(ValidationMessage.ValidateMaintenanceFeeDate);
                                        }
                                        else if (_additional_pmt[i].Flags == (int)Constants.FlagValues.ManagementFee)
                                        {
                                            DisplayMessege(ValidationMessage.ValidateManagementFeeDate);
                                        }
                                        else
                                        {
                                            DisplayMessege(ValidationMessage.AdditionalPaymentWithoutExtend);
                                        }
                                        BankAdditionalPaymentGridView.Focus();
                                        return;
                                    }
                                }
                                #endregion

                                #region When additional payment date and early Payoff date are same.
                                if (_additional_pmt.Count > 0 && _additional_pmt[_additional_pmt.Count - 1].DateIn == Convert.ToDateTime(MaturityDateTbx.Text))
                                {
                                    int countOutputGridMaturityDateRows = _output.Schedule.Count(o => o.PaymentDate == Convert.ToDateTime(MaturityDateTbx.Text) &&
                                                                                        o.Flags != (int)Constants.FlagValues.Payment);
                                    int countAdditionalPaymentGridRow = _additional_pmt.Count(o => o.DateIn == Convert.ToDateTime(MaturityDateTbx.Text));
                                    if (countAdditionalPaymentGridRow == countOutputGridMaturityDateRows) { }
                                    else if (countAdditionalPaymentGridRow > countOutputGridMaturityDateRows ||
                                        (_output.Schedule[_output.Schedule.Count - 1].Flags != (int)Constants.FlagValues.EarlyPayOff &&
                                        _additional_pmt[_additional_pmt.Count - 1].Flags != _output.Schedule[_output.Schedule.Count - 1].Flags))
                                    {
                                        if (_additional_pmt[_additional_pmt.Count - 1].Flags == (int)Constants.FlagValues.NSFFee)
                                        {
                                            DisplayMessege(ValidationMessage.InvalidNSFFeeDate);
                                        }
                                        else if (_additional_pmt[_additional_pmt.Count - 1].Flags == (int)Constants.FlagValues.LateFee)
                                        {
                                            DisplayMessege(ValidationMessage.InvalidLateFeeDate);
                                        }
                                        else if (_additional_pmt[_additional_pmt.Count - 1].Flags == (int)Constants.FlagValues.MaintenanceFee)
                                        {
                                            DisplayMessege(ValidationMessage.ValidateMaintenanceFeeDate);
                                        }
                                        else if (_additional_pmt[_additional_pmt.Count - 1].Flags == (int)Constants.FlagValues.ManagementFee)
                                        {
                                            DisplayMessege(ValidationMessage.ValidateManagementFeeDate);
                                        }
                                        else
                                        {
                                            DisplayMessege(ValidationMessage.AdditionalPaymentWithoutExtend);
                                        }
                                        BankAdditionalPaymentGridView.Focus();
                                        return;
                                    }
                                }
                                #endregion

                                if ((rollingScheduleInput.EarlyPayoffDate.ToShortDateString() != Convert.ToDateTime(Constants.DefaultDate).ToShortDateString()) &&
                                        (_output.Schedule[_output.Schedule.Count - 1].PaymentDate < rollingScheduleInput.EarlyPayoffDate) &&
                                        _output.Schedule[_output.Schedule.Count - 1].CumulativeTotalPastDue == 0 &&
                                        _output.Schedule[_output.Schedule.Count - 1].InterestCarryOver == 0 &&
                                        _output.Schedule[_output.Schedule.Count - 1].serviceFeeInterestCarryOver == 0)
                                {
                                    DisplayMessege(ValidationMessage.NoValuesDue);
                                    EarlyPayoffDateTbx.Focus();
                                }
                                #endregion
                            }

                            #endregion
                            break;
                        case 3:
                            #region Period Method

                            sbTracing.AppendLine("calling...Period Method LoanType");
                            sbTracing.AppendLine("Inside LoanAmortForm class, and button1_Click() method. Calling method : ValidateInputValues();");
                            if (!ValidateInputValues())
                            {
                                return;
                            }

                            #region

                            var periodScheduleInput = new LoanDetails();
                            OutputGridView.Visible = false;
                            BankMethodGridView.Visible = true;

                            #region InputGridView
                            for (int i = 0; i < InputGridView.Rows.Count - 1; i++)
                            {
                                gridRow = InputGridView.Rows[i];
                                inputGrid.Add(new InputGrid
                                {
                                    DateIn = Convert.ToDateTime(gridRow.Cells["DateIn"].Value),
                                    Flags = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["Flags"].Value)) ? (int)Constants.FlagValues.Payment : Convert.ToInt32(gridRow.Cells["Flags"].Value),
                                    PaymentID = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["PaymentID"].Value)) ? 0 : Convert.ToInt32(gridRow.Cells["PaymentID"].Value),
                                    EffectiveDate = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["EffectiveDate"].Value)) ? DateTime.MinValue : Convert.ToDateTime(gridRow.Cells["EffectiveDate"].Value),
                                    PaymentAmount = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["PaymentAmountSchedule"].Value)) ? "" : Convert.ToString(Convert.ToDouble(gridRow.Cells["PaymentAmountSchedule"].Value)),
                                    InterestRate = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["InterestRate"].Value)) ? "" : Convert.ToString(gridRow.Cells["InterestRate"].Value),
                                    InterestPriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["InterestPriority"].Value)) ? (IntrestPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.Interest : Convert.ToInt32(IntrestPriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["InterestPriority"].Value),
                                    PrincipalPriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["PrincipalPriority"].Value)) ? (PrincipalPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.Principal : Convert.ToInt32(PrincipalPriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["PrincipalPriority"].Value),
                                    OriginationFeePriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["OriginationFeePriority"].Value)) ? (OriginationFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.OriginationFee : Convert.ToInt32(OriginationFeePriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["OriginationFeePriority"].Value),
                                    SameDayFeePriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["SameDayFeePriority"].Value)) ? (SameDayFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.SameDayFee : Convert.ToInt32(SameDayFeePriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["SameDayFeePriority"].Value),
                                    ServiceFeePriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["ServiceFeePriority"].Value)) ? (ServiceFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ServiceFee : Convert.ToInt32(ServiceFeePriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["ServiceFeePriority"].Value),
                                    ServiceFeeInterestPriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["ServiceFeeInterestPriority"].Value)) ? (ServiceFeeInterestPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ServiceFeeInterest : Convert.ToInt32(ServiceFeeInterestPriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["ServiceFeeInterestPriority"].Value),
                                    ManagementFeePriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["ManagementFeePriority"].Value)) ? (ManagementFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ManagementFee : Convert.ToInt32(ManagementFeePriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["ManagementFeePriority"].Value),
                                    MaintenanceFeePriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["MaintenanceFeePriority"].Value)) ? (MaintenanceFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.MaintenanceFee : Convert.ToInt32(MaintenanceFeePriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["MaintenanceFeePriority"].Value),
                                    NSFFeePriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["NSFFeePriority"].Value)) ? (NSFFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.NSFFee : Convert.ToInt32(NSFFeePriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["NSFFeePriority"].Value),
                                    LateFeePriority = string.IsNullOrEmpty(Convert.ToString(gridRow.Cells["LateFeePriority"].Value)) ? (LateFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.LateFee : Convert.ToInt32(LateFeePriorityCmb.Text)) : Convert.ToInt32(gridRow.Cells["LateFeePriority"].Value)
                                });
                            }
                            #endregion

                            #region Additional payment gridview
                            for (int i = 0; i < BankAdditionalPaymentGridView.Rows.Count - 1; i++)
                            {
                                addGridRow = BankAdditionalPaymentGridView.Rows[i];
                                additionalPayments.Add(new Models.AdditionalPaymentRecord
                                {
                                    DateIn = Convert.ToDateTime(addGridRow.Cells["DateInAdditional"].Value),
                                    AdditionalPayment = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["AdditionalPaymentBank"].Value)) ? 0.0 : Convert.ToDouble(addGridRow.Cells["AdditionalPaymentBank"].Value),
                                    PaymentID = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["PaymentIDAdditionalBank"].Value)) ? 0 : Convert.ToInt32(addGridRow.Cells["PaymentIDAdditionalBank"].Value),
                                    Flags = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["FlagsAdditionalBank"].Value)) ? 0 : Convert.ToInt32(addGridRow.Cells["FlagsAdditionalBank"].Value),
                                    InterestRate = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["InterestRateAdditional"].Value)) ? "" : Convert.ToString(addGridRow.Cells["InterestRateAdditional"].Value),
                                    PrincipalDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["PrincipalAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["PrincipalAdditional"].Value),
                                    InterestDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["InterestAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["InterestAdditional"].Value),
                                    OriginationFeeDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["OriginationFeeAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["OriginationFeeAdditional"].Value),
                                    SameDayFeeDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["SameDayFeeAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["SameDayFeeAdditional"].Value),
                                    ServiceFeeDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["ServiceFeeAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["ServiceFeeAdditional"].Value),
                                    ServiceFeeInterestDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["ServiceFeeInterestAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["ServiceFeeInterestAdditional"].Value),
                                    ManagementFeeDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["ManagementFeeAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["ManagementFeeAdditional"].Value),
                                    MaintenanceFeeDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["MaintenanceFeeAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["MaintenanceFeeAdditional"].Value),
                                    NSFFeeDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["NSFFeeAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["NSFFeeAdditional"].Value),
                                    LateFeeDiscount = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["LateFeeAdditional"].Value)) ? 0 : Convert.ToDouble(addGridRow.Cells["LateFeeAdditional"].Value),
                                    InterestPriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["InterestPriorityAdditional"].Value)) ? (IntrestPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.Interest : Convert.ToInt32(IntrestPriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["InterestPriorityAdditional"].Value),
                                    PrincipalPriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["PrincipalPriorityAdditional"].Value)) ? (PrincipalPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.Principal : Convert.ToInt32(PrincipalPriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["PrincipalPriorityAdditional"].Value),
                                    OriginationFeePriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["OriginationFeePriorityAdditional"].Value)) ? (OriginationFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.OriginationFee : Convert.ToInt32(OriginationFeePriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["OriginationFeePriorityAdditional"].Value),
                                    SameDayFeePriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["SameDayFeePriorityAdditional"].Value)) ? (SameDayFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.SameDayFee : Convert.ToInt32(SameDayFeePriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["SameDayFeePriorityAdditional"].Value),
                                    ServiceFeePriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["ServiceFeePriorityAdditional"].Value)) ? (ServiceFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ServiceFee : Convert.ToInt32(ServiceFeePriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["ServiceFeePriorityAdditional"].Value),
                                    ServiceFeeInterestPriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["ServiceFeeInterestPriorityAdditional"].Value)) ? (ServiceFeeInterestPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ServiceFeeInterest : Convert.ToInt32(ServiceFeeInterestPriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["ServiceFeeInterestPriorityAdditional"].Value),
                                    ManagementFeePriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["ManagementFeePriorityAdditional"].Value)) ? (ManagementFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ManagementFee : Convert.ToInt32(ManagementFeePriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["ManagementFeePriorityAdditional"].Value),
                                    MaintenanceFeePriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["MaintenanceFeePriorityAdditional"].Value)) ? (MaintenanceFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.MaintenanceFee : Convert.ToInt32(MaintenanceFeePriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["MaintenanceFeePriorityAdditional"].Value),
                                    NSFFeePriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["NSFFeePriorityAdditional"].Value)) ? (NSFFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.NSFFee : Convert.ToInt32(NSFFeePriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["NSFFeePriorityAdditional"].Value),
                                    LateFeePriority = string.IsNullOrEmpty(Convert.ToString(addGridRow.Cells["LateFeePriorityAdditional"].Value)) ? (LateFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.LateFee : Convert.ToInt32(LateFeePriorityCmb.Text)) : Convert.ToInt32(addGridRow.Cells["LateFeePriorityAdditional"].Value)
                                });
                            }
                            #endregion

                            #region Initianlize the periodScheduleInput Object
                            sbTracing.AppendLine("Initianlize the periodScheduleInput Object");
                            periodScheduleInput = new LoanDetails
                            {
                                InputRecords = inputGrid,
                                AdditionalPaymentRecords = additionalPayments,
                                LoanAmount = string.IsNullOrEmpty(AmountTbx.Text) ? 0 : Convert.ToDouble(AmountTbx.Text),
                                InterestDelay = string.IsNullOrEmpty(InterestDelayTbx.Text) ? 0 : Convert.ToInt32(InterestDelayTbx.Text),
                                Residual = string.IsNullOrEmpty(ResidualTbx.Text) ? 0 : Convert.ToDouble(ResidualTbx.Text),
                                BalloonPayment = string.IsNullOrEmpty(BalloonPaymentTbx.Text) ? 0 : Convert.ToDouble(BalloonPaymentTbx.Text),
                                PmtPeriod = paymentPeriod,
                                EarlyPayoffDate = Convert.ToDateTime(string.IsNullOrEmpty(EarlyPayoffDateTbx.Text) ? Constants.DefaultDate : EarlyPayoffDateTbx.Text),
                                MinDuration = string.IsNullOrEmpty(MinDurationTbx.Text) ? 0 : Convert.ToInt32(MinDurationTbx.Text),
                                AfterPayment = AfterPaymentChb.Checked,
                                AccruedInterestDate = Convert.ToDateTime(string.IsNullOrEmpty(AccruedInterestDateTbx.Text) ? Constants.DefaultDate : AccruedInterestDateTbx.Text),
                                ServiceFee = Convert.ToDouble(string.IsNullOrEmpty(ServiceFeeTbx.Text) ? "0.0" : ServiceFeeTbx.Text),
                                ManagementFee = Convert.ToDouble(string.IsNullOrEmpty(ManagementFeeTbx.Text) ? "0.0" : ManagementFeeTbx.Text),
                                ManagementFeePercent = Convert.ToDouble(string.IsNullOrEmpty(ManagementFeePercentTbx.Text) ? "0.0" : ManagementFeePercentTbx.Text),
                                ManagementFeeBasis = ManagementFeeBasisCmb.SelectedIndex,
                                ManagementFeeFrequency = ManagementFeeFreqCmb.SelectedIndex,
                                ManagementFeeFrequencyNumber = Convert.ToInt32(string.IsNullOrEmpty(ManagementFeeFreqNumTbx.Text) ? "0" : ManagementFeeFreqNumTbx.Text),
                                ManagementFeeDelay = Convert.ToInt32(string.IsNullOrEmpty(ManagementFeeDelayTbx.Text) ? "0" : ManagementFeeDelayTbx.Text),
                                ManagementFeeMin = Convert.ToDouble(string.IsNullOrEmpty(ManagementFeeMinTbx.Text) ? "0.0" : ManagementFeeMinTbx.Text),
                                ManagementFeeMaxPer = Convert.ToDouble(string.IsNullOrEmpty(ManagementFeeMaxPerTbx.Text) ? "0.0" : ManagementFeeMaxPerTbx.Text),
                                ManagementFeeMaxMonth = Convert.ToDouble(string.IsNullOrEmpty(ManagementFeeMaxMonthTbx.Text) ? "0.0" : ManagementFeeMaxMonthTbx.Text),
                                ManagementFeeMaxLoan = Convert.ToDouble(string.IsNullOrEmpty(ManagementFeeMaxLoanTbx.Text) ? "0.0" : ManagementFeeMaxLoanTbx.Text),
                                IsManagementFeeGreater = ManagementFeeGreaterChb.Checked,
                                MaintenanceFee = Convert.ToDouble(string.IsNullOrEmpty(MaintenanceFeeTbx.Text) ? "0.0" : MaintenanceFeeTbx.Text),
                                MaintenanceFeePercent = Convert.ToDouble(string.IsNullOrEmpty(MaintenanceFeePercentTbx.Text) ? "0.0" : MaintenanceFeePercentTbx.Text),
                                MaintenanceFeeBasis = MaintenanceFeeBasisCmb.SelectedIndex,
                                MaintenanceFeeFrequency = MaintenanceFeeFreqCmb.SelectedIndex,
                                MaintenanceFeeFrequencyNumber = Convert.ToInt32(string.IsNullOrEmpty(MaintenanceFeeFreqNumTbx.Text) ? "0" : MaintenanceFeeFreqNumTbx.Text),
                                MaintenanceFeeDelay = Convert.ToInt32(string.IsNullOrEmpty(MaintenanceFeeDelayTbx.Text) ? "0" : MaintenanceFeeDelayTbx.Text),
                                MaintenanceFeeMin = Convert.ToDouble(string.IsNullOrEmpty(MaintenanceFeeMinTbx.Text) ? "0.0" : MaintenanceFeeMinTbx.Text),
                                MaintenanceFeeMaxPer = Convert.ToDouble(string.IsNullOrEmpty(MaintenanceFeeMaxPerTbx.Text) ? "0.0" : MaintenanceFeeMaxPerTbx.Text),
                                MaintenanceFeeMaxMonth = Convert.ToDouble(string.IsNullOrEmpty(MaintenanceFeeMaxMonthTbx.Text) ? "0.0" : MaintenanceFeeMaxMonthTbx.Text),
                                MaintenanceFeeMaxLoan = Convert.ToDouble(string.IsNullOrEmpty(MaintenanceFeeMaxLoanTbx.Text) ? "0.0" : MaintenanceFeeMaxLoanTbx.Text),
                                IsMaintenanceFeeGreater = MaintenanceFeeGreaterChb.Checked,
                                RecastAdditionalPayments = RecastAdditionalPaymentsChb.Checked,
                                UseFlexibleCalculation = UseFlexibleMethod.Checked,
                                DaysInYearBankMethod = Convert.ToString(DaysInYearCmb.Text),
                                LoanType = Convert.ToString(LoanTypeCmb.Text),
                                DaysInMonth = Convert.ToString(DaysInMonthCmb.Text),
                                ApplyServiceFeeInterest = ServiceFeeCmb.SelectedIndex,
                                IsServiceFeeFinanced = FinanceServiceFeeChb.Checked,
                                ServiceFeeFirstPayment = ServiceFeeFirstPaymentChb.Checked,
                                IsServiceFeeIncremental = ServiceFeeIncrementalChb.Checked,
                                SameDayFee = Convert.ToDouble(string.IsNullOrEmpty(SameDayFeeTbx.Text) ? "0.0" : SameDayFeeTbx.Text),
                                SameDayFeeCalculationMethod = string.IsNullOrEmpty(SameDayFeeCmb.Text) ? "Fixed" : Convert.ToString(SameDayFeeCmb.Text),
                                IsSameDayFeeFinanced = FinanceSameDayFeeChb.Checked,
                                OriginationFee = Convert.ToDouble(string.IsNullOrEmpty(OriginationFeeFixedTbx.Text) ? "0.0" : OriginationFeeFixedTbx.Text),
                                OriginationFeePercent = Convert.ToDouble(string.IsNullOrEmpty(OriginationFeePercentTbx.Text) ? "0.0" : OriginationFeePercentTbx.Text),
                                OriginationFeeMax = Convert.ToDouble(string.IsNullOrEmpty(OriginationFeeMaxTbx.Text) ? "0.0" : OriginationFeeMaxTbx.Text),
                                OriginationFeeMin = Convert.ToDouble(string.IsNullOrEmpty(OriginationFeeMinTbx.Text) ? "0.0" : OriginationFeeMinTbx.Text),
                                OriginationFeeCalculationMethod = OriginationFeeGreaterChb.Checked,
                                IsOriginationFeeFinanced = FinanceOriginationFeeChb.Checked,
                                AmountFinanced = string.IsNullOrEmpty(AmountFinancedTbx.Text) ? 0 : Convert.ToDouble(AmountFinancedTbx.Text),
                                OriginationFeePriority = OriginationFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.OriginationFee : Convert.ToInt32(OriginationFeePriorityCmb.Text),
                                SameDayFeePriority = SameDayFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.SameDayFee : Convert.ToInt32(SameDayFeePriorityCmb.Text),
                                InterestPriority = IntrestPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.Interest : Convert.ToInt32(IntrestPriorityCmb.Text),
                                PrincipalPriority = PrincipalPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.Principal : Convert.ToInt32(PrincipalPriorityCmb.Text),
                                ServiceFeeInterestPriority = ServiceFeeInterestPriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ServiceFeeInterest : Convert.ToInt32(ServiceFeeInterestPriorityCmb.Text),
                                ServiceFeePriority = ServiceFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ServiceFee : Convert.ToInt32(ServiceFeePriorityCmb.Text),
                                ManagementFeePriority = ManagementFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.ManagementFee : Convert.ToInt32(ManagementFeePriorityCmb.Text),
                                MaintenanceFeePriority = MaintenanceFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.MaintenanceFee : Convert.ToInt32(MaintenanceFeePriorityCmb.Text),
                                LateFeePriority = LateFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.LateFee : Convert.ToInt32(LateFeePriorityCmb.Text),
                                NSFFeePriority = NSFFeePriorityCmb.SelectedIndex == -1 ? (int)Constants.DefaultPriority.NSFFee : Convert.ToInt32(NSFFeePriorityCmb.Text),
                                AccruedServiceFeeInterestDate = Convert.ToDateTime(string.IsNullOrEmpty(AccruedServiceFeeInterestDateTbx.Text) ? Constants.DefaultDate : AccruedServiceFeeInterestDateTbx.Text),
                                SameAsCashPayoff = SameAsCashPayOffChb.Checked,
                                PaymentAmount = string.IsNullOrEmpty(PaymentAmountTbx.Text) ? 0 : Convert.ToDouble(PaymentAmountTbx.Text),
                                EarnedInterest = string.IsNullOrEmpty(EarnedInterestTbx.Text) ? 0 : Convert.ToDouble(EarnedInterestTbx.Text),
                                EarnedServiceFeeInterest = string.IsNullOrEmpty(EarnedServiceFeeInterestTbx.Text) ? 0 : Convert.ToDouble(EarnedServiceFeeInterestTbx.Text),
                                EarnedOriginationFee = string.IsNullOrEmpty(EarnedOriginationFeeTbx.Text) ? 0 : Convert.ToDouble(EarnedOriginationFeeTbx.Text),
                                EarnedSameDayFee = string.IsNullOrEmpty(EarnedSameDayFeeTbx.Text) ? 0 : Convert.ToDouble(EarnedSameDayFeeTbx.Text),
                                EarnedManagementFee = string.IsNullOrEmpty(EarnedManagementFeeTbx.Text) ? 0 : Convert.ToDouble(EarnedManagementFeeTbx.Text),
                                EarnedMaintenanceFee = string.IsNullOrEmpty(EarnedMaintenanceFeeTbx.Text) ? 0 : Convert.ToDouble(EarnedMaintenanceFeeTbx.Text),
                                EarnedNSFFee = string.IsNullOrEmpty(EarnedNSFFeeTbx.Text) ? 0 : Convert.ToDouble(EarnedNSFFeeTbx.Text),
                                EarnedLateFee = string.IsNullOrEmpty(EarnedLateFeeTbx.Text) ? 0 : Convert.ToDouble(EarnedLateFeeTbx.Text),
                                InterestRate = string.IsNullOrEmpty(InterestRateTbx.Text) ? "" : InterestRateTbx.Text,
                                Tier1 = string.IsNullOrEmpty(InterestTier1Tbx.Text) ? 0 : Convert.ToDouble(InterestTier1Tbx.Text),
                                InterestRate2 = string.IsNullOrEmpty(InterestRate2Tbx.Text) ? "" : InterestRate2Tbx.Text,
                                Tier2 = string.IsNullOrEmpty(InterestTier2Tbx.Text) ? 0 : Convert.ToDouble(InterestTier2Tbx.Text),
                                InterestRate3 = string.IsNullOrEmpty(InterestRate3Tbx.Text) ? "" : InterestRate3Tbx.Text,
                                Tier3 = string.IsNullOrEmpty(InterestTier3Tbx.Text) ? 0 : Convert.ToDouble(InterestTier3Tbx.Text),
                                InterestRate4 = string.IsNullOrEmpty(InterestRate4Tbx.Text) ? "" : InterestRate4Tbx.Text,
                                Tier4 = string.IsNullOrEmpty(InterestTier4Tbx.Text) ? 0 : Convert.ToDouble(InterestTier4Tbx.Text),
                                EnforcedPrincipal = string.IsNullOrEmpty(EnforcedPrincipalTbx.Text) ? 0 : Convert.ToDouble(EnforcedPrincipalTbx.Text),
                                EnforcedPayment = string.IsNullOrEmpty(EnforcedPaymentTbx.Text) ? 0 : Convert.ToInt32(EnforcedPaymentTbx.Text)
                            };
                            #endregion

                            #region Validate Input and Additional payment input grid

                            //Sort the input grid records and additioanl payment grid records.
                            periodScheduleInput.InputRecords.Sort();
                            periodScheduleInput.AdditionalPaymentRecords = periodScheduleInput.AdditionalPaymentRecords.OrderBy(o => o.DateIn).ToList();
                            for (int i = 0; i < periodScheduleInput.InputRecords.Count; i++)
                            {
                                if (periodScheduleInput.InputRecords.FindLastIndex(o => o.DateIn == periodScheduleInput.InputRecords[i].DateIn) != i)
                                {
                                    DisplayMessege(ValidationMessage.RepeatedDateInInputGrid);
                                    InputGridView.Focus();
                                    return;
                                }
                                if ((periodScheduleInput.InputRecords.FindLastIndex(o => o.EffectiveDate == periodScheduleInput.InputRecords[i].EffectiveDate) != i) &&
                                    (periodScheduleInput.InputRecords[i].EffectiveDate != DateTime.MinValue))
                                {
                                    BankMethodGridView.Rows.Clear();
                                    DisplayMessege(ValidationMessage.RepeatedDateInInputGrid);
                                    InputGridView.Focus();
                                    return;
                                }
                                if ((periodScheduleInput.InputRecords.FindLastIndex(o => o.DateIn == periodScheduleInput.InputRecords[i].EffectiveDate) != -1) &&
                                    (periodScheduleInput.InputRecords.FindLastIndex(o => o.DateIn == periodScheduleInput.InputRecords[i].EffectiveDate) != i) &&
                                    (periodScheduleInput.InputRecords[i].EffectiveDate != DateTime.MinValue))
                                {
                                    BankMethodGridView.Rows.Clear();
                                    DisplayMessege(ValidationMessage.RepeatedDateInInputGrid);
                                    InputGridView.Focus();
                                    return;
                                }
                                if (i > 0 && periodScheduleInput.InputRecords[i].EffectiveDate != DateTime.MinValue &&
                                    ((periodScheduleInput.InputRecords[i].EffectiveDate <= (periodScheduleInput.InputRecords[i - 1].EffectiveDate == DateTime.MinValue ? periodScheduleInput.InputRecords[i - 1].DateIn : periodScheduleInput.InputRecords[i - 1].EffectiveDate)) ||
                                    (i < (periodScheduleInput.InputRecords.Count - 1) && (periodScheduleInput.InputRecords[i].EffectiveDate >= (periodScheduleInput.InputRecords[i + 1].EffectiveDate == DateTime.MinValue ? periodScheduleInput.InputRecords[i + 1].DateIn : periodScheduleInput.InputRecords[i + 1].EffectiveDate)))))
                                {
                                    BankMethodGridView.Rows.Clear();
                                    DisplayMessege(ValidationMessage.DateRangeInInputGrid);
                                    InputGridView.Focus();
                                    return;
                                }
                                if (periodScheduleInput.InputRecords.FindLastIndex(o => o.PaymentID == periodScheduleInput.InputRecords[i].PaymentID) != i ||
                                        periodScheduleInput.AdditionalPaymentRecords.FindIndex(o => o.PaymentID == periodScheduleInput.InputRecords[i].PaymentID) != -1)
                                {
                                    DisplayMessege(ValidationMessage.RepeatedPaymentId);
                                    InputGridView.Focus();
                                    return;
                                }
                            }

                            if (periodScheduleInput.InputRecords[periodScheduleInput.InputRecords.Count - 1].Flags == (int)Constants.FlagValues.SkipPayment)
                            {
                                DisplayMessege(ValidationMessage.SkippPayment);
                                InputGridView.Focus();
                                return;
                            }
                            else if (periodScheduleInput.AdditionalPaymentRecords.FindIndex(o => o.DateIn <= periodScheduleInput.InputRecords[0].DateIn) != -1)
                            {
                                DisplayMessege(ValidationMessage.Additionalpaymentdate);
                                BankAdditionalPaymentGridView.Focus();
                                return;
                            }
                            else if (periodScheduleInput.AdditionalPaymentRecords.FindIndex(o => o.DateIn > periodScheduleInput.InputRecords[periodScheduleInput.InputRecords.Count - 1].DateIn &&
                                     (o.Flags == (int)Constants.FlagValues.NSFFee || o.Flags == (int)Constants.FlagValues.LateFee)) != -1)
                            {
                                DisplayMessege(ValidationMessage.NSFAndLateFeeValidation);
                                BankAdditionalPaymentGridView.Focus();
                                return;
                            }
                            else if (periodScheduleInput.AdditionalPaymentRecords.FindIndex(o => o.DateIn <= ((periodScheduleInput.InputRecords[1].EffectiveDate == DateTime.MinValue) ?
                                            periodScheduleInput.InputRecords[1].DateIn : periodScheduleInput.InputRecords[1].EffectiveDate) &&
                                            (o.Flags == (int)Constants.FlagValues.NSFFee || o.Flags == (int)Constants.FlagValues.LateFee)) != -1)
                            {
                                DisplayMessege(ValidationMessage.NSFandLateFeeBeforeFirstSchedule);
                                BankAdditionalPaymentGridView.Focus();
                                return;
                            }
                            else if ((periodScheduleInput.ManagementFee > 0 || periodScheduleInput.ManagementFeePercent > 0) &&
                                    periodScheduleInput.AdditionalPaymentRecords.FindIndex(o => o.Flags == (int)Constants.FlagValues.ManagementFee) != -1)
                            {
                                DisplayMessege(ValidationMessage.ManagementFeeFromTwoPlaces);
                                BankAdditionalPaymentGridView.Focus();
                                return;
                            }
                            else if ((periodScheduleInput.MaintenanceFee > 0 || periodScheduleInput.MaintenanceFeePercent > 0) &&
                                    periodScheduleInput.AdditionalPaymentRecords.FindIndex(o => o.Flags == (int)Constants.FlagValues.MaintenanceFee) != -1)
                            {
                                DisplayMessege(ValidationMessage.MaintenanceFeeFromTwoPlaces);
                                BankAdditionalPaymentGridView.Focus();
                                return;
                            }
                            else
                            {
                                for (int i = 0; i < periodScheduleInput.AdditionalPaymentRecords.Count; i++)
                                {
                                    if (periodScheduleInput.AdditionalPaymentRecords.FindLastIndex(o => o.PaymentID == periodScheduleInput.AdditionalPaymentRecords[i].PaymentID) != i)
                                    {
                                        DisplayMessege(ValidationMessage.RepeatedPaymentId);
                                        BankAdditionalPaymentGridView.Focus();
                                        return;
                                    }
                                }
                            }

                            #endregion

                            sbTracing.AppendLine("Inside LoanAmortForm class, and button1_Click() method. Calling method : PeriodBusiness.CreateScheduleForPeriodMethod.DefaultSchedule(periodScheduleInput);");
                            outputValues = PeriodBusiness.CreateScheduleForPeriodMethod.DefaultSchedule(periodScheduleInput);

                            if (outputValues.LoanAmountCalc <= periodScheduleInput.Residual)
                            {
                                DisplayMessege(ValidationMessage.ValidateResidualAmount);
                                ResidualTbx.Focus();
                                return;
                            }
                            else if (outputValues.LoanAmountCalc <= periodScheduleInput.BalloonPayment)
                            {
                                DisplayMessege(ValidationMessage.ValidateBalloonPaymentWithLoanAmount);
                                ResidualTbx.Focus();
                                return;
                            }

                            #region Fill Output Grid

                            BankMethodGridView.Rows.Clear();
                            object[] rowBufferForPeriodMethod = new object[77];
                            var rowsForPeriodMethod = new List<DataGridViewRow>();
                            foreach (var grdValues in outputValues.Schedule)
                            {
                                rowBufferForPeriodMethod[0] = grdValues.PaymentDate.ToString("MM/dd/yyyy");
                                rowBufferForPeriodMethod[1] = Convert.ToString(grdValues.BeginningPrincipal);
                                rowBufferForPeriodMethod[2] = Convert.ToString(grdValues.BeginningServiceFee);
                                rowBufferForPeriodMethod[3] = Convert.ToString(grdValues.BeginningPrincipalServiceFee);
                                rowBufferForPeriodMethod[4] = Convert.ToString(grdValues.PeriodicInterestRate);
                                rowBufferForPeriodMethod[5] = Convert.ToString(grdValues.DailyInterestRate);
                                rowBufferForPeriodMethod[6] = Convert.ToString(grdValues.DailyInterestAmount);
                                rowBufferForPeriodMethod[7] = Convert.ToString(grdValues.PaymentID);
                                rowBufferForPeriodMethod[8] = Convert.ToString(grdValues.Flags);
                                rowBufferForPeriodMethod[9] = grdValues.DueDate.ToString("MM/dd/yyyy");
                                rowBufferForPeriodMethod[10] = Convert.ToString(grdValues.InterestAccrued);
                                rowBufferForPeriodMethod[11] = Convert.ToString(grdValues.InterestDue);
                                rowBufferForPeriodMethod[12] = Convert.ToString(grdValues.InterestCarryOver);
                                //Amount to be paid in the particular period columns
                                rowBufferForPeriodMethod[13] = Convert.ToString(grdValues.InterestPayment);
                                rowBufferForPeriodMethod[14] = Convert.ToString(grdValues.PrincipalPayment);
                                rowBufferForPeriodMethod[15] = Convert.ToString(grdValues.PaymentDue);
                                rowBufferForPeriodMethod[16] = Convert.ToString(grdValues.TotalPayment);
                                rowBufferForPeriodMethod[17] = Convert.ToString(grdValues.InterestServiceFeeInterestPayment);
                                rowBufferForPeriodMethod[18] = Convert.ToString(grdValues.PrincipalServiceFeePayment);
                                rowBufferForPeriodMethod[19] = Convert.ToString(grdValues.AccruedServiceFeeInterest);
                                rowBufferForPeriodMethod[20] = Convert.ToString(grdValues.ServiceFeeInterestDue);
                                rowBufferForPeriodMethod[21] = Convert.ToString(grdValues.AccruedServiceFeeInterestCarryOver);
                                rowBufferForPeriodMethod[22] = Convert.ToString(grdValues.ServiceFeeInterest);
                                rowBufferForPeriodMethod[23] = Convert.ToString(grdValues.ServiceFee);
                                rowBufferForPeriodMethod[24] = Convert.ToString(grdValues.ServiceFeeTotal);
                                rowBufferForPeriodMethod[25] = Convert.ToString(grdValues.OriginationFee);
                                rowBufferForPeriodMethod[26] = Convert.ToString(grdValues.MaintenanceFee);
                                rowBufferForPeriodMethod[27] = Convert.ToString(grdValues.ManagementFee);
                                rowBufferForPeriodMethod[28] = Convert.ToString(grdValues.SameDayFee);
                                rowBufferForPeriodMethod[29] = Convert.ToString(grdValues.NSFFee);
                                rowBufferForPeriodMethod[30] = Convert.ToString(grdValues.LateFee);
                                //Paid amount columns
                                rowBufferForPeriodMethod[31] = Convert.ToString(grdValues.PrincipalPaid);
                                rowBufferForPeriodMethod[32] = Convert.ToString(grdValues.InterestPaid);
                                rowBufferForPeriodMethod[33] = Convert.ToString(grdValues.ServiceFeePaid);
                                rowBufferForPeriodMethod[34] = Convert.ToString(grdValues.ServiceFeeInterestPaid);
                                rowBufferForPeriodMethod[35] = Convert.ToString(grdValues.OriginationFeePaid);
                                rowBufferForPeriodMethod[36] = Convert.ToString(grdValues.MaintenanceFeePaid);
                                rowBufferForPeriodMethod[37] = Convert.ToString(grdValues.ManagementFeePaid);
                                rowBufferForPeriodMethod[38] = Convert.ToString(grdValues.SameDayFeePaid);
                                rowBufferForPeriodMethod[39] = Convert.ToString(grdValues.NSFFeePaid);
                                rowBufferForPeriodMethod[40] = Convert.ToString(grdValues.LateFeePaid);
                                rowBufferForPeriodMethod[41] = Convert.ToString(grdValues.TotalPaid);
                                //Cumulative Amount paid column
                                rowBufferForPeriodMethod[42] = Convert.ToString(grdValues.CumulativeInterest);
                                rowBufferForPeriodMethod[43] = Convert.ToString(grdValues.CumulativePrincipal);
                                rowBufferForPeriodMethod[44] = Convert.ToString(grdValues.CumulativePayment);
                                rowBufferForPeriodMethod[45] = Convert.ToString(grdValues.CumulativeServiceFee);
                                rowBufferForPeriodMethod[46] = Convert.ToString(grdValues.CumulativeServiceFeeInterest);
                                rowBufferForPeriodMethod[47] = Convert.ToString(grdValues.CumulativeServiceFeeTotal);
                                rowBufferForPeriodMethod[48] = Convert.ToString(grdValues.CumulativeOriginationFee);
                                rowBufferForPeriodMethod[49] = Convert.ToString(grdValues.CumulativeMaintenanceFee);
                                rowBufferForPeriodMethod[50] = Convert.ToString(grdValues.CumulativeManagementFee);
                                rowBufferForPeriodMethod[51] = Convert.ToString(grdValues.CumulativeSameDayFee);
                                rowBufferForPeriodMethod[52] = Convert.ToString(grdValues.CumulativeNSFFee);
                                rowBufferForPeriodMethod[53] = Convert.ToString(grdValues.CumulativeLateFee);
                                rowBufferForPeriodMethod[54] = Convert.ToString(grdValues.CumulativeTotalFees);
                                //Past due amount column
                                rowBufferForPeriodMethod[55] = Convert.ToString(grdValues.PrincipalPastDue);
                                rowBufferForPeriodMethod[56] = Convert.ToString(grdValues.InterestPastDue);
                                rowBufferForPeriodMethod[57] = Convert.ToString(grdValues.ServiceFeePastDue);
                                rowBufferForPeriodMethod[58] = Convert.ToString(grdValues.ServiceFeeInterestPastDue);
                                rowBufferForPeriodMethod[59] = Convert.ToString(grdValues.OriginationFeePastDue);
                                rowBufferForPeriodMethod[60] = Convert.ToString(grdValues.MaintenanceFeePastDue);
                                rowBufferForPeriodMethod[61] = Convert.ToString(grdValues.ManagementFeePastDue);
                                rowBufferForPeriodMethod[62] = Convert.ToString(grdValues.SameDayFeePastDue);
                                rowBufferForPeriodMethod[63] = Convert.ToString(grdValues.NSFFeePastDue);
                                rowBufferForPeriodMethod[64] = Convert.ToString(grdValues.LateFeePastDue);
                                rowBufferForPeriodMethod[65] = Convert.ToString(grdValues.TotalPastDue);
                                //Cumulative past due amount columns
                                rowBufferForPeriodMethod[66] = Convert.ToString(grdValues.CumulativePrincipalPastDue);
                                rowBufferForPeriodMethod[67] = Convert.ToString(grdValues.CumulativeInterestPastDue);
                                rowBufferForPeriodMethod[68] = Convert.ToString(grdValues.CumulativeServiceFeePastDue);
                                rowBufferForPeriodMethod[69] = Convert.ToString(grdValues.CumulativeServiceFeeInterestPastDue);
                                rowBufferForPeriodMethod[70] = Convert.ToString(grdValues.CumulativeOriginationFeePastDue);
                                rowBufferForPeriodMethod[71] = Convert.ToString(grdValues.CumulativeMaintenanceFeePastDue);
                                rowBufferForPeriodMethod[72] = Convert.ToString(grdValues.CumulativeManagementFeePastDue);
                                rowBufferForPeriodMethod[73] = Convert.ToString(grdValues.CumulativeSameDayFeePastDue);
                                rowBufferForPeriodMethod[74] = Convert.ToString(grdValues.CumulativeNSFFeePastDue);
                                rowBufferForPeriodMethod[75] = Convert.ToString(grdValues.CumulativeLateFeePastDue);
                                rowBufferForPeriodMethod[76] = Convert.ToString(grdValues.CumulativeTotalPastDue);

                                rowsForPeriodMethod.Add(new DataGridViewRow());
                                rowsForPeriodMethod[rowsForPeriodMethod.Count - 1].CreateCells(BankMethodGridView, rowBufferForPeriodMethod);
                            }
                            sbTracing.AppendLine("Fetch the values of all outputValues.Schedule.Count = " + outputValues.Schedule.Count + " rows.");
                            BankMethodGridView.Rows.AddRange(rowsForPeriodMethod.ToArray());

                            #endregion

                            #region Fill Management Fee Table

                            ManagementFeeTableGrid.Rows.Clear();
                            foreach (var grdValues in outputValues.ManagementFeeAssessment)
                            {
                                if (grdValues.AssessmentDate > outputValues.Schedule[outputValues.Schedule.Count - 1].PaymentDate)
                                {
                                    break;
                                }

                                rowBufferForManagementFeeTable[0] = grdValues.AssessmentDate.ToString("MM/dd/yyyy");
                                rowBufferForManagementFeeTable[1] = Convert.ToString(grdValues.Fee);
                                rowsForManagementFeeTable.Add(new DataGridViewRow());
                                rowsForManagementFeeTable[rowsForManagementFeeTable.Count - 1].CreateCells(ManagementFeeTableGrid, rowBufferForManagementFeeTable);
                            }
                            sbTracing.AppendLine("Fetch the values of all outputValues.ManagementFeeAssessment.Count = " + outputValues.ManagementFeeAssessment.Count + " rows.");
                            ManagementFeeTableGrid.Rows.AddRange(rowsForManagementFeeTable.ToArray());

                            #endregion

                            #region Fill Maintenance Fee Table

                            MaintenanceFeeTableGrid.Rows.Clear();
                            foreach (var grdValues in outputValues.MaintenanceFeeAssessment)
                            {
                                if (grdValues.AssessmentDate > outputValues.Schedule[outputValues.Schedule.Count - 1].PaymentDate)
                                {
                                    break;
                                }

                                rowBufferForMaintenanceFeeTable[0] = grdValues.AssessmentDate.ToString("MM/dd/yyyy");
                                rowBufferForMaintenanceFeeTable[1] = Convert.ToString(grdValues.Fee);
                                rowsForMaintenanceFeeTable.Add(new DataGridViewRow());
                                rowsForMaintenanceFeeTable[rowsForMaintenanceFeeTable.Count - 1].CreateCells(MaintenanceFeeTableGrid, rowBufferForMaintenanceFeeTable);
                            }
                            sbTracing.AppendLine("Fetch the values of all outputValues.MaintenanceFeeAssessment.Count = " + outputValues.MaintenanceFeeAssessment.Count + " rows.");
                            MaintenanceFeeTableGrid.Rows.AddRange(rowsForMaintenanceFeeTable.ToArray());

                            #endregion

                            if (SameAsCashPayOffChb.Checked)
                            {
                                outputValues.PayoffBalance = outputValues.Schedule[outputValues.Schedule.Count - 1].CumulativePayment - outputValues.AmountFinanced;
                            }

                            OriginationFeeCalcTbx.Text = Convert.ToString(outputValues.OriginationFee);
                            LoanAmountCalcTbx.Text = Convert.ToString(outputValues.LoanAmountCalc);
                            AmountFinancedOutputTbx.Text = Convert.ToString(outputValues.AmountFinanced);
                            RegularPaymentTbx.Text = Convert.ToString(outputValues.RegularPayment);
                            CostOfFinancingTbx.Text = Convert.ToString(outputValues.CostOfFinancing);
                            AccruedServiceFeeInterestTbx.Text = Convert.ToString(outputValues.AccruedServiceFeeInterest);
                            AccruedInterestTbx.Text = Convert.ToString(outputValues.AccruedInterest);
                            AccruedPrincipalTbx.Text = Convert.ToString(outputValues.AccruedPrincipal);
                            MaturityDateTbx.Text = outputValues.Schedule[outputValues.Schedule.Count - 1].PaymentDate.ToString("MM/dd/yyyy");
                            PayOffBalanceTbx.Text = Convert.ToString(outputValues.PayoffBalance);
                            ManagementFeeEffectiveTbx.Text = (outputValues.ManagementFeeEffective == DateTime.MinValue ? Convert.ToDateTime(Constants.DefaultDate) :
                                                                                                        outputValues.ManagementFeeEffective).ToString("MM/dd/yyyy");
                            MaintenanceFeeEffectiveTbx.Text = (outputValues.MaintenanceFeeEffective == DateTime.MinValue ? Convert.ToDateTime(Constants.DefaultDate) :
                                                                                                        outputValues.MaintenanceFeeEffective).ToString("MM/dd/yyyy");

                            additionalPayments = additionalPayments.OrderBy(o => o.DateIn).ToList();

                            #region Validation for additional payments to be thrown, when maturity date is less than the date of adding additional payments.
                            for (int i = 0; i < additionalPayments.Count; i++)
                            {
                                if (additionalPayments[i].DateIn > Convert.ToDateTime(MaturityDateTbx.Text))
                                {
                                    if (additionalPayments[i].Flags == (int)Constants.FlagValues.NSFFee)
                                    {
                                        DisplayMessege(ValidationMessage.InvalidNSFFeeDate);
                                    }
                                    else if (additionalPayments[i].Flags == (int)Constants.FlagValues.LateFee)
                                    {
                                        DisplayMessege(ValidationMessage.InvalidLateFeeDate);
                                    }
                                    else if (additionalPayments[i].Flags == (int)Constants.FlagValues.MaintenanceFee)
                                    {
                                        DisplayMessege(ValidationMessage.ValidateMaintenanceFeeDate);
                                    }
                                    else if (additionalPayments[i].Flags == (int)Constants.FlagValues.ManagementFee)
                                    {
                                        DisplayMessege(ValidationMessage.ValidateManagementFeeDate);
                                    }
                                    else
                                    {
                                        DisplayMessege(ValidationMessage.AdditionalPaymentWithoutExtend);
                                    }
                                    BankAdditionalPaymentGridView.Focus();
                                    return;
                                }
                            }
                            #endregion

                            if (additionalPayments.Count > 0 && additionalPayments[additionalPayments.Count - 1].DateIn == Convert.ToDateTime(MaturityDateTbx.Text))
                            {
                                int countOutputGridMaturityDateRows = outputValues.Schedule.Count(o => o.PaymentDate == Convert.ToDateTime(MaturityDateTbx.Text) &&
                                                                                    o.Flags != (int)Constants.FlagValues.Payment);
                                int countAdditionalPaymentGridRow = additionalPayments.Count(o => o.DateIn == Convert.ToDateTime(MaturityDateTbx.Text));
                                if (countAdditionalPaymentGridRow == countOutputGridMaturityDateRows) { }
                                else if (countAdditionalPaymentGridRow > countOutputGridMaturityDateRows ||
                                    (outputValues.Schedule[outputValues.Schedule.Count - 1].Flags != (int)Constants.FlagValues.EarlyPayOff &&
                                    additionalPayments[additionalPayments.Count - 1].Flags != outputValues.Schedule[outputValues.Schedule.Count - 1].Flags))
                                {
                                    if (additionalPayments[additionalPayments.Count - 1].Flags == (int)Constants.FlagValues.NSFFee)
                                    {
                                        DisplayMessege(ValidationMessage.InvalidNSFFeeDate);
                                    }
                                    else if (additionalPayments[additionalPayments.Count - 1].Flags == (int)Constants.FlagValues.LateFee)
                                    {
                                        DisplayMessege(ValidationMessage.InvalidLateFeeDate);
                                    }
                                    else if (additionalPayments[additionalPayments.Count - 1].Flags == (int)Constants.FlagValues.MaintenanceFee)
                                    {
                                        DisplayMessege(ValidationMessage.ValidateMaintenanceFeeDate);
                                    }
                                    else if (additionalPayments[additionalPayments.Count - 1].Flags == (int)Constants.FlagValues.ManagementFee)
                                    {
                                        DisplayMessege(ValidationMessage.ValidateManagementFeeDate);
                                    }
                                    else
                                    {
                                        DisplayMessege(ValidationMessage.AdditionalPaymentWithoutExtend);
                                    }
                                    BankAdditionalPaymentGridView.Focus();
                                    return;
                                }
                            }
                            if ((periodScheduleInput.EarlyPayoffDate.ToShortDateString() != Convert.ToDateTime(Constants.DefaultDate).ToShortDateString()) &&
                                    (outputValues.Schedule[outputValues.Schedule.Count - 1].PaymentDate < periodScheduleInput.EarlyPayoffDate) &&
                                    outputValues.Schedule[outputValues.Schedule.Count - 1].CumulativeTotalPastDue == 0 &&
                                    outputValues.Schedule[outputValues.Schedule.Count - 1].InterestCarryOver == 0 &&
                                    outputValues.Schedule[outputValues.Schedule.Count - 1].serviceFeeInterestCarryOver == 0)
                            {
                                DisplayMessege(ValidationMessage.NoValuesDue);
                                EarlyPayoffDateTbx.Focus();
                            }

                            #endregion

                            #endregion
                            break;
                    }
                }
                sbTracing.AppendLine("Exit:From LoanAmortForm.cs on button1_Click(object sender, EventArgs e) event");
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

        public static bool IsValidate(string date)
        {
            DateTime dt;
            if ((DateTime.TryParseExact(date, "M/d/yyyy", null, DateTimeStyles.None, out dt) == true) || (DateTime.TryParseExact(date, "M/d/yy", null, DateTimeStyles.None, out dt) == true))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void RigidMethod()
        {

            Regex regAmount = new Regex(@"^[0-9]*(\.[0-9]{1,2})?$");
            List<InputRecord> _input = new List<InputRecord>();
            List<AdditionalPaymentRecord> _additional_pmt = new List<AdditionalPaymentRecord>();

            DataGridViewRow gRow;
            for (int i = 0; i < InputGridView.Rows.Count - 1; i++)
            {
                #region Add Input Grid value in a List
                gRow = InputGridView.Rows[i];
                if (!IsValidate(gRow.Cells[0].Value.ToString()))
                {
                    throw new ArgumentException("Provide valid date in input grid in MM/dd/yyyy format.");

                }
                if (!string.IsNullOrEmpty(Convert.ToString(gRow.Cells["EffectiveDate"].Value)))
                {
                    if (!IsValidate(gRow.Cells["EffectiveDate"].Value.ToString()))
                    {
                        throw new ArgumentException("Provide valid effective date in input grid in MM/dd/yyyy format.");
                    }
                }
                if (!string.IsNullOrEmpty(Convert.ToString(gRow.Cells["PaymentAmountSchedule"].Value)) && !regAmount.IsMatch(Convert.ToString(gRow.Cells["PaymentAmountSchedule"].Value)))
                {
                    throw new ArgumentException("Actual Payment Amount can only be positive numeric value.");
                }
                _input.Add(new InputRecord
                {

                    EffectiveDate = Convert.ToDateTime(gRow.Cells[0].Value),
                    DateIn = string.IsNullOrEmpty(Convert.ToString(gRow.Cells["EffectiveDate"].Value)) ? Convert.ToDateTime(gRow.Cells[0].Value) : Convert.ToDateTime(gRow.Cells["EffectiveDate"].Value),
                    Flags = string.IsNullOrEmpty(Convert.ToString(gRow.Cells[1].Value)) ? 0 : Convert.ToInt32(gRow.Cells[1].Value),
                    PaymentID = string.IsNullOrEmpty(Convert.ToString(gRow.Cells[2].Value)) ? 0 : Convert.ToInt32(gRow.Cells[2].Value),
                    PaymentAmount = string.IsNullOrEmpty(Convert.ToString(gRow.Cells["PaymentAmountSchedule"].Value)) ? "" : Convert.ToString(Convert.ToDouble(gRow.Cells["PaymentAmountSchedule"].Value))

                });
                #endregion
            }
            _input.Sort();

            #region  Validate input grid dates with effective date and payment id.
            for (int i = 0; i < _input.Count; i++)
            {
                if (i > 0 && _input[i].EffectiveDate != DateTime.MinValue && ((_input[i].EffectiveDate <= (_input[i - 1].EffectiveDate == DateTime.MinValue ? _input[i - 1].DateIn : _input[i - 1].EffectiveDate)) ||
                   (i < (_input.Count - 1) && (_input[i].EffectiveDate >= (_input[i + 1].EffectiveDate == DateTime.MinValue ? _input[i + 1].DateIn : _input[i + 1].EffectiveDate)))))
                {
                    throw new ArgumentException("Effective date can never be greater than or equals to next schedule date, nor be less than or equals to previous schedule date.");
                }
                else if (_input.Count(o => o.PaymentID == _input[i].PaymentID) > 1)
                {
                    throw new ArgumentException("Payment Ids must be unique.");
                }
            }
            #endregion

            for (int i = 0; i < BankAdditionalPaymentGridView.Rows.Count - 1; i++)
            {
                #region Add additional payment Grid value in a List
                gRow = BankAdditionalPaymentGridView.Rows[i];
                if (!IsValidate(gRow.Cells["DateInAdditional"].Value.ToString()))
                {
                    throw new ArgumentException("Provide valid date in additional payment grid in MM/dd/yyyy format.");

                }
                if (!string.IsNullOrEmpty(Convert.ToString(gRow.Cells["FlagsAdditionalBank"].Value)) || Convert.ToInt32(gRow.Cells["FlagsAdditionalBank"].Value) == 2 || Convert.ToInt32(gRow.Cells["FlagsAdditionalBank"].Value) == 3)
                {
                    _additional_pmt.Add(new AdditionalPaymentRecord
                    {
                        DateIn = Convert.ToDateTime(gRow.Cells["DateInAdditional"].Value),
                        AdditionalPayment = string.IsNullOrEmpty(Convert.ToString(gRow.Cells["AdditionalPaymentBank"].Value)) ? 0.0 : Convert.ToDouble(gRow.Cells["AdditionalPaymentBank"].Value),
                        PaymentID = string.IsNullOrEmpty(Convert.ToString(gRow.Cells["PaymentIDAdditionalBank"].Value)) ? 0 : Convert.ToInt32(gRow.Cells["PaymentIDAdditionalBank"].Value),
                        PrincipalOnly = string.IsNullOrEmpty(Convert.ToString(gRow.Cells["FlagsAdditionalBank"].Value)) ? true : Convert.ToInt32(gRow.Cells["FlagsAdditionalBank"].Value) == 2 ? true : (Convert.ToInt32(gRow.Cells["FlagsAdditionalBank"].Value) == 3 ? false : true)
                    });
                }
                else
                {
                    throw new ArgumentException("Invalid flag value in Additional payment gridview. Enter flag value 2 for principal only or 3 for NOT principal only.");
                }
                #endregion
            }

            #region  Validate input grid dates with effective date and payment id.
            for (int i = 0; i < _additional_pmt.Count; i++)
            {
                if (_additional_pmt.Count(o => o.PaymentID == _additional_pmt[i].PaymentID) > 1)
                {
                    throw new ArgumentException("Additional grid Payment Ids must be unique.");
                }
                else if (_input.Count(o => o.PaymentID == _additional_pmt[i].PaymentID) >= 1)
                {
                    throw new ArgumentException("Payment Ids must be unique.");
                }
            }
            #endregion

            getScheduleOutput _output;
            string[] dividedPeriod = PeriodCmb.Text.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            PaymentPeriod PmtPeriod = (PaymentPeriod)Convert.ToInt16(dividedPeriod[0].Trim());
            //call function    
            bool isInterestRounded = chkIsInterestRounded.Checked;
            _output = LoanAmort.getSchedule(
                new getScheduleInput
                {
                    #region Set all input value
                    InputRecords = _input,
                    AdditionalPaymentRecords = _additional_pmt,
                    LoanAmount = Convert.ToDouble(AmountTbx.Text),
                    BalloonPayment = string.IsNullOrEmpty(BalloonPaymentTbx.Text) ? 0 : Convert.ToDouble(BalloonPaymentTbx.Text),
                    InterestRate = Convert.ToDouble(InterestRateTbx.Text),
                    InterestDelay = string.IsNullOrEmpty(InterestDelayTbx.Text) ? 0 : Convert.ToInt32(InterestDelayTbx.Text),
                    PmtPeriod = PmtPeriod,
                    EarlyPayoffDate = Convert.ToDateTime(EarlyPayoffDateTbx.Text),
                    MinDuration = Convert.ToInt32(String.IsNullOrEmpty(MinDurationTbx.Text) ? "0" : MinDurationTbx.Text),
                    AccruedInterestDate = Convert.ToDateTime(AccruedInterestDateTbx.Text),
                    AccruedServiceFeeInterestDate = Convert.ToDateTime(AccruedServiceFeeInterestDateTbx.Text),
                    // AD 1.0.0.6 - get additional fee data from UI
                    ServiceFee = Convert.ToDouble(String.IsNullOrEmpty(ServiceFeeTbx.Text) ? "0.0" : ServiceFeeTbx.Text),
                    ServiceFeeFirstPayment = ServiceFeeFirstPaymentChb.Checked,
                    IsServiceFeeIncremental = ServiceFeeIncrementalChb.Checked,
                    OriginationFee = Convert.ToDouble(String.IsNullOrEmpty(OriginationFeeFixedTbx.Text) ? "0.0" : OriginationFeeFixedTbx.Text),
                    SameDayFee = Convert.ToDouble(String.IsNullOrEmpty(SameDayFeeTbx.Text) ? "0.0" : SameDayFeeTbx.Text),
                    MaintenanceFee = Convert.ToDouble(String.IsNullOrEmpty(MaintenanceFeeTbx.Text) ? "0.0" : MaintenanceFeeTbx.Text),
                    ManagementFee = Convert.ToDouble(String.IsNullOrEmpty(ManagementFeeTbx.Text) ? "0.0" : ManagementFeeTbx.Text),
                    ApplyServiceFeeInterest = Convert.ToInt16(ServiceFeeCmb.Text.Substring(0, 1)),
                    // AD 1.0.0.7 - read flag to recast additional payments
                    RecastAdditionalPayments = RecastAdditionalPaymentsChb.Checked,
                    //AG 1.0.0.11 - added flag to support flexible calculatin method
                    UseFlexibleCalculation = UseFlexibleMethod.Checked,
                    DaysInYear = Convert.ToInt16(DaysInYearCmb.SelectedItem),
                    IsInterestRounded = isInterestRounded,
                    PaymentAmount = string.IsNullOrEmpty(PaymentAmountTbx.Text) ? 0 : Convert.ToDouble(PaymentAmountTbx.Text),
                    AfterPayment = AfterPaymentChb.Checked,
                    EarnedInterest = string.IsNullOrEmpty(EarnedInterestTbx.Text) ? 0 : Convert.ToDouble(EarnedInterestTbx.Text),
                    EarnedServiceFeeInterest = string.IsNullOrEmpty(EarnedServiceFeeInterestTbx.Text) ? 0 : Convert.ToDouble(EarnedServiceFeeInterestTbx.Text),
                    Tier1 = string.IsNullOrEmpty(InterestTier1Tbx.Text) ? 0 : Convert.ToDouble(InterestTier1Tbx.Text),
                    InterestRate2 = string.IsNullOrEmpty(InterestRate2Tbx.Text) ? 0 : Convert.ToDouble(InterestRate2Tbx.Text),
                    Tier2 = string.IsNullOrEmpty(InterestTier2Tbx.Text) ? 0 : Convert.ToDouble(InterestTier2Tbx.Text),
                    InterestRate3 = string.IsNullOrEmpty(InterestRate3Tbx.Text) ? 0 : Convert.ToDouble(InterestRate3Tbx.Text),
                    Tier3 = string.IsNullOrEmpty(InterestTier3Tbx.Text) ? 0 : Convert.ToDouble(InterestTier3Tbx.Text),
                    InterestRate4 = string.IsNullOrEmpty(InterestRate4Tbx.Text) ? 0 : Convert.ToDouble(InterestRate4Tbx.Text),
                    Tier4 = string.IsNullOrEmpty(InterestTier4Tbx.Text) ? 0 : Convert.ToDouble(InterestTier4Tbx.Text),
                    EnforcedPrincipal = string.IsNullOrEmpty(EnforcedPrincipalTbx.Text) ? 0 : Convert.ToDouble(EnforcedPrincipalTbx.Text),
                    EnforcedPayment = string.IsNullOrEmpty(EnforcedPaymentTbx.Text) ? 0 : Convert.ToInt32(EnforcedPaymentTbx.Text)
                    #endregion
                });
            //clear and load output grid
            double PaymentAmount = string.IsNullOrEmpty(PaymentAmountTbx.Text) ? 0 : Convert.ToDouble(PaymentAmountTbx.Text);
            OutputGridView.Rows.Clear();
            object[] RowBuffer = new object[33];
            List<DataGridViewRow> rows = new List<DataGridViewRow>();
            foreach (PaymentDetail PmtDetail in _output.Schedule)
            {
                #region Fill all resulted value in output grid
                RowBuffer[0] = PmtDetail.PaymentDate.ToString("MM/dd/yyyy");
                RowBuffer[1] = Math.Round(PmtDetail.BeginningPrincipal, 2, MidpointRounding.AwayFromZero);
                RowBuffer[2] = Math.Round(PmtDetail.BeginningServiceFee, 2, MidpointRounding.AwayFromZero);
                RowBuffer[3] = Math.Round(PmtDetail.BeginningPrincipalServiceFee, 2, MidpointRounding.AwayFromZero);
                RowBuffer[4] = PmtDetail.DueDate.ToString("MM/dd/yyyy");
                RowBuffer[5] = Math.Round(PmtDetail.DailyInterestAmount, 2, MidpointRounding.AwayFromZero);
                RowBuffer[6] = Math.Round(PmtDetail.PrincipalPayment, 2, MidpointRounding.AwayFromZero);
                RowBuffer[7] = Math.Round(PmtDetail.InterestPayment, 2, MidpointRounding.AwayFromZero);

                RowBuffer[8] = isInterestRounded ? Math.Round(PmtDetail.InterestAccrued, 2, MidpointRounding.AwayFromZero) : PmtDetail.InterestAccrued;
                RowBuffer[9] = isInterestRounded ? Math.Round(PmtDetail.InterestDue, 2, MidpointRounding.AwayFromZero) : PmtDetail.InterestDue;
                RowBuffer[10] = isInterestRounded ? Math.Round(PmtDetail.InterestCarryOver, 2, MidpointRounding.AwayFromZero) : PmtDetail.InterestCarryOver;
                RowBuffer[11] = PmtDetail.TotalPayment.ToString("0.00");
                RowBuffer[12] = Math.Round(PmtDetail.PrincipalServiceFeePayment, 2, MidpointRounding.AwayFromZero);
                RowBuffer[13] = Math.Round(PmtDetail.InterestServiceFeeInterestPayment, 2, MidpointRounding.AwayFromZero);
                RowBuffer[14] = Math.Round(PmtDetail.CumulativePayment, 2, MidpointRounding.AwayFromZero);
                RowBuffer[15] = Math.Round(PmtDetail.CumulativeInterest, 2, MidpointRounding.AwayFromZero);
                RowBuffer[16] = PmtDetail.PeriodicInterestRate;
                RowBuffer[17] = PmtDetail.PaymentID;
                RowBuffer[18] = PmtDetail.Flags;

                // AD - 1.0.0.6 - added columns for Service Fee values
                RowBuffer[19] = Math.Round(PmtDetail.ServiceFee, 2, MidpointRounding.AwayFromZero);
                RowBuffer[20] = isInterestRounded ? Math.Round(PmtDetail.ServiceFeeInterest, 2, MidpointRounding.AwayFromZero) : PmtDetail.ServiceFeeInterest;
                RowBuffer[21] = Math.Round(PmtDetail.ServiceFeeTotal, 2, MidpointRounding.AwayFromZero);
                RowBuffer[22] = Math.Round(PmtDetail.OriginationFee, 2, MidpointRounding.AwayFromZero);
                RowBuffer[23] = Math.Round(PmtDetail.SameDayFee, 2, MidpointRounding.AwayFromZero);
                RowBuffer[24] = Math.Round(PmtDetail.MaintenanceFee, 2, MidpointRounding.AwayFromZero);
                RowBuffer[25] = Math.Round(PmtDetail.ManagementFee, 2, MidpointRounding.AwayFromZero);

                RowBuffer[26] = Math.Round(PmtDetail.CumulativeServiceFee, 2, MidpointRounding.AwayFromZero);
                RowBuffer[27] = Math.Round(PmtDetail.CumulativeServiceFeeInterest, 2, MidpointRounding.AwayFromZero);
                RowBuffer[28] = Math.Round(PmtDetail.CumulativeOriginationFee, 2, MidpointRounding.AwayFromZero);
                RowBuffer[29] = Math.Round(PmtDetail.CumulativeSameDayFee, 2, MidpointRounding.AwayFromZero);
                RowBuffer[30] = Math.Round(PmtDetail.CumulativeMaintenanceFee, 2, MidpointRounding.AwayFromZero);
                RowBuffer[31] = Math.Round(PmtDetail.CumulativeManagementFee, 2, MidpointRounding.AwayFromZero);
                RowBuffer[32] = Math.Round(PmtDetail.CumulativeTotalFees, 2, MidpointRounding.AwayFromZero);

                rows.Add(new DataGridViewRow());
                rows[rows.Count - 1].CreateCells(OutputGridView, RowBuffer);
                #endregion
            }
            OutputGridView.Rows.AddRange(rows.ToArray());
            double earnedInterest = string.IsNullOrEmpty(EarnedInterestTbx.Text) ? 0 : Convert.ToDouble(EarnedInterestTbx.Text);
            _output.RegularPayment = _output.RegularPayment + Math.Round(earnedInterest / (_input.Count - 1), 2, MidpointRounding.AwayFromZero);
            RegularPaymentTbx.Text = Math.Round(PaymentAmount > 0 ? PaymentAmount : _output.RegularPayment, 2, MidpointRounding.AwayFromZero).ToString();
            AccruedInterestTbx.Text = (Math.Round(_output.AccruedInterest, 2, MidpointRounding.AwayFromZero).ToString());
            // AD 1.0.0.2 - added output of Accrued Principal
            AccruedPrincipalTbx.Text = Math.Round(_output.AccruedPrincipal, 2, MidpointRounding.AwayFromZero).ToString();
            AccruedServiceFeeInterestTbx.Text = Math.Round(_output.AccruedServiceFeeInterest, 2, MidpointRounding.AwayFromZero).ToString();
        }
        public void PasteRows()
        {
            string[] DataLines = Clipboard.GetText().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string[] RowValues;
            int RowIndex = 0;
            int ColumnIndex = 0;
            int StartRow = InputGridView.SelectedCells[0].RowIndex;
            int StartColumn = InputGridView.SelectedCells[0].ColumnIndex;
            for (RowIndex = 0; RowIndex < DataLines.Length - 1; RowIndex++)
            {
                if (RowIndex + StartRow >= InputGridView.RowCount - 1)
                {
                    InputGridView.Rows.Add();
                }
                RowValues = DataLines[RowIndex].Split(new string[] { "\t" }, StringSplitOptions.None);
                for (ColumnIndex = 0; ColumnIndex < RowValues.Length; ColumnIndex++)
                {
                    if (StartColumn + ColumnIndex < InputGridView.ColumnCount /*&& StartRow + RowIndex < dataGridView1.RowCount*/)
                    {
                        InputGridView[StartColumn + ColumnIndex, StartRow + RowIndex].Value = RowValues[ColumnIndex];
                    }
                }
            }
        }
        public void PasteRows2()
        {
            string[] DataLines = Clipboard.GetText().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string[] RowValues;
            int RowIndex = 0;
            int ColumnIndex = 0;
            int StartRow = BankAdditionalPaymentGridView.SelectedCells[0].RowIndex;
            int StartColumn = BankAdditionalPaymentGridView.SelectedCells[0].ColumnIndex;
            for (RowIndex = 0; RowIndex < DataLines.Length - 1; RowIndex++)
            {
                if (RowIndex + StartRow >= BankAdditionalPaymentGridView.RowCount - 1)
                {
                    BankAdditionalPaymentGridView.Rows.Add();
                }
                RowValues = DataLines[RowIndex].Split(new string[] { "\t" }, StringSplitOptions.None);
                for (ColumnIndex = 0; ColumnIndex < RowValues.Length; ColumnIndex++)
                {
                    if (StartColumn + ColumnIndex < BankAdditionalPaymentGridView.ColumnCount /*&& StartRow + RowIndex < dataGridView1.RowCount*/)
                    {
                        BankAdditionalPaymentGridView[StartColumn + ColumnIndex, StartRow + RowIndex].Value = RowValues[ColumnIndex];
                    }
                }
            }

        }
        private void InputGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control && ((e.KeyCode == Keys.V)))
            {
                PasteRows();
            }
        }
        private void AdditionalPaymentGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control && ((e.KeyCode == Keys.V)))
            {
                PasteRows2();
            }
        }

        private void CopyRows()
        {
            StringBuilder sb = new StringBuilder("");

            sb.AppendLine("PaymentDate\t" +
               "BeginningPrincipal\t" +
               "BeginningServiceFee\t" +
               "BeginningPrincipalServiceFee\t" +
               "PrincipalPayment\t" +
               "InterestPayment\t" +
               "InterestAccrued\t" +
               "InterestDue\t" +
               "InterestCarryOver\t" +
               "TotalPayment\t" +
               "PrincipalServiceFeePayment\t" +
               "InterestServiceFeeInterestPayment\t" +
               "CumulativeLoanPayment\t" +
               "CumulativeLoanInterest");


            for (int i = 0; i < OutputGridView.Rows.Count; i++)
            {
                if (!OutputGridView.Rows[i].IsNewRow)
                {
                    sb.AppendLine(OutputGridView.Rows[i].Cells[0].Value + "\t" +
                        OutputGridView.Rows[i].Cells[1].Value + "\t" +
                        OutputGridView.Rows[i].Cells[2].Value + "\t" +
                        OutputGridView.Rows[i].Cells[3].Value + "\t" +
                        OutputGridView.Rows[i].Cells[4].Value + "\t" +
                        OutputGridView.Rows[i].Cells[5].Value + "\t" +
                        OutputGridView.Rows[i].Cells[6].Value + "\t" +
                        OutputGridView.Rows[i].Cells[7].Value + "\t" +
                        OutputGridView.Rows[i].Cells[8].Value + "\t" +
                        OutputGridView.Rows[i].Cells[9].Value + "\t" +
                        OutputGridView.Rows[i].Cells[10].Value + "\t" +
                        OutputGridView.Rows[i].Cells[11].Value + "\t" +
                        OutputGridView.Rows[i].Cells[12].Value + "\t" +
                        OutputGridView.Rows[i].Cells[13].Value);
                }
            }
            Clipboard.SetText(sb.ToString());
        }
        private void CopyRowsForBankMethod()
        {
            StringBuilder sb = new StringBuilder("");

            sb.AppendLine("PaymentDate\t" +
                "BeginningPrincipal\t" +
                "BeginningServiceFee\t" +
                "BeginningPrincipalServiceFee\t" +
                "PeriodicInterestRate\t" +
                "DailyInterestRate\t" +
                "DailyInterestAmount\t" +
                "PaymentID\t" +
                "Flags\t" +
                "DueDate\t" +
                "InterestAccrued\t" +
                (LoanTypeCmb.SelectedIndex == 3 ? "InterestDue\t" : "") +
                "InterestCarryOver\t" +
                "InterestPayment\t" +
                "PrincipalPayment\t" +
                ((LoanTypeCmb.SelectedIndex == 2 || LoanTypeCmb.SelectedIndex == 3) ? "PaymentDue\t" : "") +
                "TotalPayment\t" +
                "InterestServiceFeeInterestPayment\t" +
                "PrincipalServiceFeePayment\t" +
                "AccruedServiceFeeInterest\t" +
                (LoanTypeCmb.SelectedIndex == 3 ? "ServiceFeeInterestDue\t" : "") +
                "AccruedServiceFeeInterestCarryOver\t" +
                "ServiceFeeInterest\t" +
                "ServiceFee\t" +
                "ServiceFeeTotal\t" +
                "OriginationFee\t" +
                "MaintenanceFee\t" +
                "ManagementFee\t" +
                "SameDayFee\t" +
                "NSFFee\t" +
                "LateFee\t" +
                "PrincipalPaid\t" +
                "InterestPaid\t" +
                "ServiceFeePaid\t" +
                "ServiceFeeInterestPaid\t" +
                "OriginationFeePaid\t" +
                "MaintenanceFeePaid\t" +
                "ManagementFeePaid\t" +
                "SameDayFeePaid\t" +
                "NSFFeePaid\t" +
                "LateFeePaid\t" +
                "TotalPaid\t" +
                "CumulativeInterest\t" +
                "CumulativePrincipal\t" +
                "CumulativePayment\t" +
                "CumulativeServiceFee\t" +
                "CumulativeServiceFeeInterest\t" +
                "CumulativeServiceFeeTotal\t" +
                "CumulativeOriginationFee\t" +
                "CumulativeMaintenanceFee\t" +
                "CumulativeManagementFee\t" +
                "CumulativeSameDayFee\t" +
                "CumulativeNSFFee\t" +
                "CumulativeLateFee\t" +
                "CumulativeTotalFees\t" +
                "PrincipalPastDue\t" +
                "InterestPastDue\t" +
                "ServiceFeePastDue\t" +
                "ServiceFeeInterestPastDue\t" +
                "OriginationFeePastDue\t" +
                "MaintenanceFeePastDue\t" +
                "ManagementFeePastDue\t" +
                "SameDayFeePastDue\t" +
                "NSFFeePastDue\t" +
                "LateFeePastDue\t" +
                "TotalPastDue\t" +
                "CumulativePrincipalPastDue\t" +
                "CumulativeInterestPastDue\t" +
                "CumulativeServiceFeePastDue\t" +
                "CumulativeServiceFeeInterestPastDue\t" +
                "CumulativeOriginationFeePastDue\t" +
                "CumulativeMaintenanceFeePastDue\t" +
                "CumulativeManagementFeePastDue\t" +
                "CumulativeSameDayFeePastDue\t" +
                "CumulativeNSFFeePastDue\t" +
                "CumulativeLateFeePastDue\t" +
                "CumulativeTotalPastDue");

            for (int i = 0; i < BankMethodGridView.Rows.Count; i++)
            {
                if (!BankMethodGridView.Rows[i].IsNewRow)
                {
                    sb.AppendLine(BankMethodGridView.Rows[i].Cells["PaymentDateBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["BeginningPrincipalBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["BeginningServiceFee"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["BeginningPrincipalServiceFee"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["PeriodicInterestRateBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["DailyInterestRateBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["DailyInterestAmountBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["PaymentIDBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["FlagsBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["DueDateBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["InterestAccruedBankMethod"].Value + "\t" +
                        (LoanTypeCmb.SelectedIndex == 3 ? (BankMethodGridView.Rows[i].Cells["InterestDuePeriod"].Value + "\t") : "") +
                        BankMethodGridView.Rows[i].Cells["IneterstCarryOver"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["InterestPaymentBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["PrincipalPaymentBank"].Value + "\t" +
                        ((LoanTypeCmb.SelectedIndex == 2 || LoanTypeCmb.SelectedIndex == 3) ? (BankMethodGridView.Rows[i].Cells["PaymentDue"].Value + "\t") : "") +
                        BankMethodGridView.Rows[i].Cells["TotalPaymentBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["InterestServiceFeeInterestPayment"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["PrincipalServiceFeePayment"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["AccruedServiceFeeInterest"].Value + "\t" +
                        (LoanTypeCmb.SelectedIndex == 3 ? (BankMethodGridView.Rows[i].Cells["ServiceFeeInterestDue"].Value + "\t") : "") +
                        BankMethodGridView.Rows[i].Cells["AccruedServiceFeeInterestCarryOver"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["ServiceFeeInterestBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["ServiceFeeBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["ServiceFeeTotalBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["OriginationFeeBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["MaintenanceFeeBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["ManagementFeeBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["SameDayFeeBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["NSFFeeBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["LateFeeBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["PrincipalPaidBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["InterestPaidBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["ServiceFeePaidBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["ServiceFeeInterestPaidBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["OriginationFeePaidBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["MaintenanceFeePaidBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["ManagementFeePaidBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["SameDayFeePaidBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["NSFFeePaidBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["LateFeePaidBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["TotalPaidBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativeInterestBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativePrincipalBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativePaymentBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativeServiceFeeBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativeServiceFeeInterestBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativeServiceFeeTotalBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativeOriginationFeeBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativeMaintenanceFeeBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativeManagementFeeBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativeSameDayFeeBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativeNSFFeeBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativeLateFeeBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativeTotalFeesBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["PrincipalPastDueBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["InterestPastDueBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["ServiceFeePastDueBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["ServiceFeeInterestPastDueBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["OriginationFeePastDueBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["MaintenanceFeePastDueBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["ManagementFeePastDueBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["SameDayFeePastDueBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["NSFFeePastDueBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["LateFeePastDueBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["TotalPastDueBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativePrincipalPastDueBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativeInterestPastDueBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativeServiceFeePastDueBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativeServiceFeeInterestPastDueBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativeOriginationFeePastDueBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativeMaintenanceFeePastDueBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativeManagementFeePastDueBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativeSameDayFeePastDueBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativeNSFFeePastDueBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativeLateFeePastDueBank"].Value + "\t" +
                        BankMethodGridView.Rows[i].Cells["CumulativeTotalPastDueBank"].Value);
                }
            }
            Clipboard.SetText(sb.ToString());
        }
        private void OutputGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control && ((e.KeyCode == Keys.C)))
            {
                CopyRows();
            }
        }
        private void OutputGridViewBankMethod_KeyDown(object sender, KeyEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control && ((e.KeyCode == Keys.C)))
            {
                CopyRowsForBankMethod();
            }
        }

        private void LoanTypeDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowHideFields();

            AmountTbx.Text = "";
            AmountFinancedTbx.Text = "";
            LoanAmountCalcTbx.Text = "";
            ResidualTbx.Text = "";
            InterestDelayTbx.Text = "";
            PeriodCmb.SelectedItem = "12 - Monthly";
            DaysInYearCmb.SelectedIndex = 0;
            DaysInMonthCmb.SelectedIndex = 0;
            EarlyPayoffDateTbx.Text = Constants.DefaultDate;
            MinDurationTbx.Text = "";
            AccruedInterestDateTbx.Text = Constants.DefaultDate;
            AccruedServiceFeeInterestDateTbx.Text = Constants.DefaultDate;
            PaymentAmountTbx.Text = "";
            SameAsCashPayOffChb.Checked = false;
            RecastAdditionalPaymentsChb.Checked = false;
            UseFlexibleMethod.Checked = false;
            chkIsInterestRounded.Checked = false;
            AfterPaymentChb.Checked = false;
            MinDurationTbx.Text = "";

            #region Fee tab
            ServiceFeeTbx.Text = "";
            ServiceFeeCmb.SelectedIndex = -1;
            FinanceServiceFeeChb.Checked = false;
            ServiceFeeFirstPaymentChb.Checked = false;
            ServiceFeeIncrementalChb.Checked = false;
            SameDayFeeTbx.Text = "";
            SameDayFeeCmb.SelectedIndex = -1;
            FinanceSameDayFeeChb.Checked = false;
            OriginationFeeFixedTbx.Text = "";
            OriginationFeePercentTbx.Text = "";
            OriginationFeeMaxTbx.Text = "";
            OriginationFeeMinTbx.Text = "";
            OriginationFeeCalcTbx.Text = "";
            OriginationFeeGreaterChb.Checked = false;
            FinanceOriginationFeeChb.Checked = false;
            EnforcedPrincipalTbx.Text = "";
            EnforcedPaymentTbx.Text = "";
            BalloonPaymentTbx.Text = "";
            #endregion

            #region Output tab
            RegularPaymentTbx.Text = "";
            AccruedInterestTbx.Text = "";
            AccruedPrincipalTbx.Text = "";
            MaturityDateTbx.Text = Constants.DefaultDate;
            AmountFinancedOutputTbx.Text = "";
            CostOfFinancingTbx.Text = "";
            AccruedServiceFeeInterestTbx.Text = "";
            PayOffBalanceTbx.Text = "";
            #endregion

            #region Management fee tab
            ManagementFeeTbx.Text = "";
            ManagementFeePercentTbx.Text = "";
            ManagementFeeBasisCmb.SelectedIndex = -1;
            ManagementFeeFreqCmb.SelectedIndex = -1;
            ManagementFeeFreqNumTbx.Text = "";
            ManagementFeeDelayTbx.Text = "";
            ManagementFeeEffectiveTbx.Text = Constants.DefaultDate;
            ManagementFeeMinTbx.Text = "";
            ManagementFeeMaxPerTbx.Text = "";
            ManagementFeeMaxMonthTbx.Text = "";
            ManagementFeeMaxLoanTbx.Text = "";
            ManagementFeeGreaterChb.Checked = false;
            #endregion

            #region Maintenance fee tab
            MaintenanceFeeTbx.Text = "";
            MaintenanceFeePercentTbx.Text = "";
            MaintenanceFeeBasisCmb.SelectedIndex = -1;
            MaintenanceFeeFreqCmb.SelectedIndex = -1;
            MaintenanceFeeFreqNumTbx.Text = "";
            MaintenanceFeeDelayTbx.Text = "";
            MaintenanceFeeEffectiveTbx.Text = Constants.DefaultDate;
            MaintenanceFeeMinTbx.Text = "";
            MaintenanceFeeMaxPerTbx.Text = "";
            MaintenanceFeeMaxMonthTbx.Text = "";
            MaintenanceFeeMaxLoanTbx.Text = "";
            MaintenanceFeeGreaterChb.Checked = false;
            #endregion

            #region Eraned fee tab
            EarnedInterestTbx.Text = "";
            EarnedServiceFeeInterestTbx.Text = "";
            EarnedOriginationFeeTbx.Text = "";
            EarnedSameDayFeeTbx.Text = "";
            EarnedManagementFeeTbx.Text = "";
            EarnedMaintenanceFeeTbx.Text = "";
            EarnedNSFFeeTbx.Text = "";
            EarnedLateFeeTbx.Text = "";
            #endregion

            #region Interest tier tab
            InterestRateTbx.Text = "";
            InterestTier1Tbx.Text = "";
            InterestRate2Tbx.Text = "";
            InterestTier2Tbx.Text = "";
            InterestRate3Tbx.Text = "";
            InterestTier3Tbx.Text = "";
            InterestRate4Tbx.Text = "";
            InterestTier4Tbx.Text = "";
            #endregion

            ManagementFeeTableGrid.Rows.Clear();
            MaintenanceFeeTableGrid.Rows.Clear();
            InputGridView.Rows.Clear();
            BankAdditionalPaymentGridView.Rows.Clear();
        }
        private void ShowHideFields()
        {
            OutputGridView.Rows.Clear();
            BankMethodGridView.Rows.Clear();
            DaysInYearCmb.Items.Remove("Actual");

            if (LoanTypeCmb.SelectedIndex == 0)
            {
                #region Show/Hide Input and Output Grid columns

                OutputGridView.Visible = true;
                BankMethodGridView.Visible = false;

                InputGridView.Columns["EffectiveDate"].Visible = true;
                InputGridView.Columns["PaymentAmountSchedule"].Visible = true;
                InputGridView.Columns["InterestRate"].Visible = false;
                InputGridView.Columns["InterestPriority"].Visible = false;
                InputGridView.Columns["PrincipalPriority"].Visible = false;
                InputGridView.Columns["OriginationFeePriority"].Visible = false;
                InputGridView.Columns["SameDayFeePriority"].Visible = false;
                InputGridView.Columns["ServiceFeePriority"].Visible = false;
                InputGridView.Columns["ServiceFeeInterestPriority"].Visible = false;
                InputGridView.Columns["ManagementFeePriority"].Visible = false;
                InputGridView.Columns["MaintenanceFeePriority"].Visible = false;
                InputGridView.Columns["NSFFeePriority"].Visible = false;
                InputGridView.Columns["LateFeePriority"].Visible = false;

                BankAdditionalPaymentGridView.Columns["InterestRateAdditional"].Visible = false;
                BankAdditionalPaymentGridView.Columns["PrincipalAdditional"].Visible = false;
                BankAdditionalPaymentGridView.Columns["InterestAdditional"].Visible = false;
                BankAdditionalPaymentGridView.Columns["OriginationFeeAdditional"].Visible = false;
                BankAdditionalPaymentGridView.Columns["SameDayFeeAdditional"].Visible = false;
                BankAdditionalPaymentGridView.Columns["ServiceFeeAdditional"].Visible = false;
                BankAdditionalPaymentGridView.Columns["ServiceFeeInterestAdditional"].Visible = false;
                BankAdditionalPaymentGridView.Columns["ManagementFeeAdditional"].Visible = false;
                BankAdditionalPaymentGridView.Columns["MaintenanceFeeAdditional"].Visible = false;
                BankAdditionalPaymentGridView.Columns["NSFFeeAdditional"].Visible = false;
                BankAdditionalPaymentGridView.Columns["LateFeeAdditional"].Visible = false;
                BankAdditionalPaymentGridView.Columns["InterestPriorityAdditional"].Visible = false;
                BankAdditionalPaymentGridView.Columns["PrincipalPriorityAdditional"].Visible = false;
                BankAdditionalPaymentGridView.Columns["OriginationFeePriorityAdditional"].Visible = false;
                BankAdditionalPaymentGridView.Columns["SameDayFeePriorityAdditional"].Visible = false;
                BankAdditionalPaymentGridView.Columns["ServiceFeePriorityAdditional"].Visible = false;
                BankAdditionalPaymentGridView.Columns["ServiceFeeInterestPriorityAdditional"].Visible = false;
                BankAdditionalPaymentGridView.Columns["ManagementFeePriorityAdditional"].Visible = false;
                BankAdditionalPaymentGridView.Columns["MaintenanceFeePriorityAdditional"].Visible = false;
                BankAdditionalPaymentGridView.Columns["NSFFeePriorityAdditional"].Visible = false;
                BankAdditionalPaymentGridView.Columns["LateFeePriorityAdditional"].Visible = false;

                #endregion

                #region Enable/Disable other input controls

                AmountFinancedTbx.Enabled = false;
                DaysInMonthCmb.Enabled = false;
                SameAsCashPayOffChb.Enabled = false;
                RecastAdditionalPaymentsChb.Enabled = true;
                UseFlexibleMethod.Enabled = true;
                chkIsInterestRounded.Enabled = true;
                ResidualTbx.Enabled = false;

                #endregion

                #region Enable/Disable Fee Controls

                OriginationFeeGreaterChb.Enabled = false;
                OriginationFeePercentTbx.Enabled = false;
                OriginationFeeMaxTbx.Enabled = false;
                OriginationFeeMinTbx.Enabled = false;
                FinanceOriginationFeeChb.Enabled = false;
                label13.Text = "Origination Fee:";
                SameDayFeeCmb.Enabled = false;
                FinanceSameDayFeeChb.Enabled = false;
                FinanceServiceFeeChb.Enabled = false;

                #endregion

                #region Management fee controls

                ManagementFeePercentTbx.Enabled = false;
                ManagementFeeBasisCmb.Enabled = false;
                ManagementFeeFreqCmb.Enabled = false;
                ManagementFeeFreqNumTbx.Enabled = false;
                ManagementFeeDelayTbx.Enabled = false;
                ManagementFeeEffectiveTbx.Enabled = false;
                ManagementFeeMinTbx.Enabled = false;
                ManagementFeeMaxPerTbx.Enabled = false;
                ManagementFeeMaxMonthTbx.Enabled = false;
                ManagementFeeMaxLoanTbx.Enabled = false;
                ManagementFeeGreaterChb.Enabled = false;

                #endregion

                #region Maintenance fee controls

                MaintenanceFeePercentTbx.Enabled = false;
                MaintenanceFeeBasisCmb.Enabled = false;
                MaintenanceFeeFreqCmb.Enabled = false;
                MaintenanceFeeFreqNumTbx.Enabled = false;
                MaintenanceFeeDelayTbx.Enabled = false;
                MaintenanceFeeEffectiveTbx.Enabled = false;
                MaintenanceFeeMinTbx.Enabled = false;
                MaintenanceFeeMaxPerTbx.Enabled = false;
                MaintenanceFeeMaxMonthTbx.Enabled = false;
                MaintenanceFeeMaxLoanTbx.Enabled = false;
                MaintenanceFeeGreaterChb.Enabled = false;

                #endregion

                #region Enable/Disable Output Controls

                PayOffBalanceTbx.Enabled = false;
                LoanAmountCalcTbx.Enabled = false;
                MaturityDateTbx.Enabled = false;
                AmountFinancedOutputTbx.Enabled = false;
                CostOfFinancingTbx.Enabled = false;
                OriginationFeeCalcTbx.Enabled = false;
                ManagementFeeTableGrid.Enabled = false;
                MaintenanceFeeTableGrid.Enabled = false;

                #endregion

                #region Priority Tab

                IntrestPriorityCmb.SelectedIndex = -1;
                PrincipalPriorityCmb.SelectedIndex = -1;
                ServiceFeePriorityCmb.SelectedIndex = -1;
                ServiceFeeInterestPriorityCmb.SelectedIndex = -1;
                OriginationFeePriorityCmb.SelectedIndex = -1;
                ManagementFeePriorityCmb.SelectedIndex = -1;
                MaintenanceFeePriorityCmb.SelectedIndex = -1;
                NSFFeePriorityCmb.SelectedIndex = -1;
                LateFeePriorityCmb.SelectedIndex = -1;
                SameDayFeePriorityCmb.SelectedIndex = -1;

                IntrestPriorityCmb.Enabled = false;
                PrincipalPriorityCmb.Enabled = false;
                ServiceFeePriorityCmb.Enabled = false;
                ServiceFeeInterestPriorityCmb.Enabled = false;
                OriginationFeePriorityCmb.Enabled = false;
                ManagementFeePriorityCmb.Enabled = false;
                MaintenanceFeePriorityCmb.Enabled = false;
                NSFFeePriorityCmb.Enabled = false;
                LateFeePriorityCmb.Enabled = false;
                SameDayFeePriorityCmb.Enabled = false;

                #endregion

                #region Earned fee controls

                EarnedOriginationFeeTbx.Enabled = false;
                EarnedSameDayFeeTbx.Enabled = false;
                EarnedManagementFeeTbx.Enabled = false;
                EarnedMaintenanceFeeTbx.Enabled = false;
                EarnedNSFFeeTbx.Enabled = false;
                EarnedLateFeeTbx.Enabled = false;

                #endregion
            }
            else if (LoanTypeCmb.SelectedIndex == 1 || LoanTypeCmb.SelectedIndex == 2 || LoanTypeCmb.SelectedIndex == 3)
            {
                #region Show/Hide Input and Output Grid columns

                OutputGridView.Visible = false;
                BankMethodGridView.Visible = true;

                InputGridView.Columns["EffectiveDate"].Visible = true;
                InputGridView.Columns["PaymentAmountSchedule"].Visible = true;
                InputGridView.Columns["InterestRate"].Visible = true;
                InputGridView.Columns["InterestPriority"].Visible = true;
                InputGridView.Columns["PrincipalPriority"].Visible = true;
                InputGridView.Columns["OriginationFeePriority"].Visible = true;
                InputGridView.Columns["SameDayFeePriority"].Visible = true;
                InputGridView.Columns["ServiceFeePriority"].Visible = true;
                InputGridView.Columns["ServiceFeeInterestPriority"].Visible = true;
                InputGridView.Columns["ManagementFeePriority"].Visible = true;
                InputGridView.Columns["MaintenanceFeePriority"].Visible = true;
                InputGridView.Columns["NSFFeePriority"].Visible = true;
                InputGridView.Columns["LateFeePriority"].Visible = true;

                if (LoanTypeCmb.SelectedIndex == 2 || LoanTypeCmb.SelectedIndex == 3)
                {
                    BankMethodGridView.Columns["PaymentDue"].Visible = true;
                }
                else
                {
                    BankMethodGridView.Columns["PaymentDue"].Visible = false;
                }

                if (LoanTypeCmb.SelectedIndex == 3)
                {
                    BankMethodGridView.Columns["InterestDuePeriod"].Visible = true;
                    BankMethodGridView.Columns["ServiceFeeInterestDue"].Visible = true;
                }
                else
                {
                    BankMethodGridView.Columns["InterestDuePeriod"].Visible = false;
                    BankMethodGridView.Columns["ServiceFeeInterestDue"].Visible = false;
                }

                BankAdditionalPaymentGridView.Columns["InterestRateAdditional"].Visible = true;
                BankAdditionalPaymentGridView.Columns["PrincipalAdditional"].Visible = true;
                BankAdditionalPaymentGridView.Columns["InterestAdditional"].Visible = true;
                BankAdditionalPaymentGridView.Columns["OriginationFeeAdditional"].Visible = true;
                BankAdditionalPaymentGridView.Columns["SameDayFeeAdditional"].Visible = true;
                BankAdditionalPaymentGridView.Columns["ServiceFeeAdditional"].Visible = true;
                BankAdditionalPaymentGridView.Columns["ServiceFeeInterestAdditional"].Visible = true;
                BankAdditionalPaymentGridView.Columns["ManagementFeeAdditional"].Visible = true;
                BankAdditionalPaymentGridView.Columns["MaintenanceFeeAdditional"].Visible = true;
                BankAdditionalPaymentGridView.Columns["NSFFeeAdditional"].Visible = true;
                BankAdditionalPaymentGridView.Columns["LateFeeAdditional"].Visible = true;
                BankAdditionalPaymentGridView.Columns["InterestPriorityAdditional"].Visible = true;
                BankAdditionalPaymentGridView.Columns["PrincipalPriorityAdditional"].Visible = true;
                BankAdditionalPaymentGridView.Columns["OriginationFeePriorityAdditional"].Visible = true;
                BankAdditionalPaymentGridView.Columns["SameDayFeePriorityAdditional"].Visible = true;
                BankAdditionalPaymentGridView.Columns["ServiceFeePriorityAdditional"].Visible = true;
                BankAdditionalPaymentGridView.Columns["ServiceFeeInterestPriorityAdditional"].Visible = true;
                BankAdditionalPaymentGridView.Columns["ManagementFeePriorityAdditional"].Visible = true;
                BankAdditionalPaymentGridView.Columns["MaintenanceFeePriorityAdditional"].Visible = true;
                BankAdditionalPaymentGridView.Columns["NSFFeePriorityAdditional"].Visible = true;
                BankAdditionalPaymentGridView.Columns["LateFeePriorityAdditional"].Visible = true;

                #endregion

                #region Enable/Disable other input controls

                AmountFinancedTbx.Enabled = true;
                DaysInMonthCmb.Enabled = true;
                SameAsCashPayOffChb.Enabled = false;
                RecastAdditionalPaymentsChb.Enabled = false;
                UseFlexibleMethod.Enabled = false;
                chkIsInterestRounded.Enabled = false;
                ResidualTbx.Enabled = true;
                DaysInYearCmb.Items.Insert(3, "Actual");

                #endregion

                #region Enable/Disable Fee Controls

                OriginationFeeGreaterChb.Enabled = true;
                OriginationFeePercentTbx.Enabled = true;
                OriginationFeeMaxTbx.Enabled = true;
                OriginationFeeMinTbx.Enabled = true;
                FinanceOriginationFeeChb.Enabled = true;
                SameDayFeeCmb.Enabled = true;
                FinanceServiceFeeChb.Enabled = true;
                FinanceSameDayFeeChb.Enabled = true;
                label13.Text = "Origination Fee Fixed:";

                #endregion

                #region Management fee controls

                ManagementFeePercentTbx.Enabled = true;
                ManagementFeeBasisCmb.Enabled = true;
                ManagementFeeFreqCmb.Enabled = true;
                ManagementFeeFreqNumTbx.Enabled = true;
                ManagementFeeDelayTbx.Enabled = true;
                ManagementFeeEffectiveTbx.Enabled = true;
                ManagementFeeMinTbx.Enabled = true;
                ManagementFeeMaxPerTbx.Enabled = true;
                ManagementFeeMaxMonthTbx.Enabled = true;
                ManagementFeeMaxLoanTbx.Enabled = true;
                ManagementFeeGreaterChb.Enabled = true;

                #endregion

                #region Maintenance fee controls

                MaintenanceFeePercentTbx.Enabled = true;
                MaintenanceFeeBasisCmb.Enabled = true;
                MaintenanceFeeFreqCmb.Enabled = true;
                MaintenanceFeeFreqNumTbx.Enabled = true;
                MaintenanceFeeDelayTbx.Enabled = true;
                MaintenanceFeeEffectiveTbx.Enabled = true;
                MaintenanceFeeMinTbx.Enabled = true;
                MaintenanceFeeMaxPerTbx.Enabled = true;
                MaintenanceFeeMaxMonthTbx.Enabled = true;
                MaintenanceFeeMaxLoanTbx.Enabled = true;
                MaintenanceFeeGreaterChb.Enabled = true;

                #endregion

                #region Enable/Disable Output Controls

                PayOffBalanceTbx.Enabled = true;
                LoanAmountCalcTbx.Enabled = true;
                OriginationFeeCalcTbx.Enabled = true;
                MaturityDateTbx.Enabled = true;
                AmountFinancedOutputTbx.Enabled = true;
                CostOfFinancingTbx.Enabled = true;
                ManagementFeeTableGrid.Enabled = true;
                MaintenanceFeeTableGrid.Enabled = true;

                #endregion

                #region Priority Tab

                PrincipalPriorityCmb.SelectedIndex = ((int)Constants.DefaultPriority.Principal - 1);
                ServiceFeePriorityCmb.SelectedIndex = ((int)Constants.DefaultPriority.ServiceFee - 1);
                IntrestPriorityCmb.SelectedIndex = ((int)Constants.DefaultPriority.Interest - 1);
                ServiceFeeInterestPriorityCmb.SelectedIndex = ((int)Constants.DefaultPriority.ServiceFeeInterest - 1);
                OriginationFeePriorityCmb.SelectedIndex = ((int)Constants.DefaultPriority.OriginationFee - 1);
                ManagementFeePriorityCmb.SelectedIndex = ((int)Constants.DefaultPriority.ManagementFee - 1);
                MaintenanceFeePriorityCmb.SelectedIndex = ((int)Constants.DefaultPriority.MaintenanceFee - 1);
                NSFFeePriorityCmb.SelectedIndex = ((int)Constants.DefaultPriority.NSFFee - 1);
                LateFeePriorityCmb.SelectedIndex = ((int)Constants.DefaultPriority.LateFee - 1);
                SameDayFeePriorityCmb.SelectedIndex = ((int)Constants.DefaultPriority.SameDayFee - 1);

                PrincipalPriorityCmb.Enabled = true;
                ServiceFeePriorityCmb.Enabled = true;
                IntrestPriorityCmb.Enabled = true;
                ServiceFeeInterestPriorityCmb.Enabled = true;
                OriginationFeePriorityCmb.Enabled = true;
                ManagementFeePriorityCmb.Enabled = true;
                MaintenanceFeePriorityCmb.Enabled = true;
                NSFFeePriorityCmb.Enabled = true;
                LateFeePriorityCmb.Enabled = true;
                SameDayFeePriorityCmb.Enabled = true;

                #endregion

                #region Earned fee controls

                EarnedOriginationFeeTbx.Enabled = true;
                EarnedSameDayFeeTbx.Enabled = true;
                EarnedManagementFeeTbx.Enabled = true;
                EarnedMaintenanceFeeTbx.Enabled = true;
                EarnedNSFFeeTbx.Enabled = true;
                EarnedLateFeeTbx.Enabled = true;

                #endregion
            }
        }

        #region Validations

        private bool ValidateInputValues()
        {
            StringBuilder sbTracing = new StringBuilder();

            bool isValidated = true;
            Regex regAmount = new Regex(@"^[0-9]*(\.[0-9]{1,2})?$");
            Regex regAmountThreeDecimal = new Regex(@"^[0-9]*(\.[0-9]{1,3})?$");
            Regex regIntAmount = new Regex(@"^[0-9]+$");
            Regex regIdValue = new Regex(@"^[0-9]{1,10}$");
            try
            {
                sbTracing.AppendLine("Enter: Inside LoanAmortForm Class file and method Name ValidateInputValues().");
                #region Validate input controls

                //Validation for Loan amount and Amount Financed input control.
                if (!regAmount.IsMatch(AmountTbx.Text))
                {
                    DisplayMessege(ValidationMessage.LoanAmountCanOnlyBePositive);
                    AmountTbx.Focus();
                    isValidated = false;
                }
                else if (!regAmount.IsMatch(AmountFinancedTbx.Text))
                {
                    DisplayMessege(ValidationMessage.AmountFinancedCanOnlyBePositive);
                    AmountFinancedTbx.Focus();
                    isValidated = false;
                }
                else if ((string.IsNullOrEmpty(AmountTbx.Text) && string.IsNullOrEmpty(AmountFinancedTbx.Text)) ||
                    (string.IsNullOrEmpty(AmountTbx.Text) && Convert.ToDouble(AmountFinancedTbx.Text) == 0.0) ||
                    (string.IsNullOrEmpty(AmountFinancedTbx.Text) && Convert.ToDouble(AmountTbx.Text) == 0.0))
                {
                    BankMethodGridView.Rows.Clear();
                    LoanAmountCalcTbx.Text = "";
                    RegularPaymentTbx.Text = "";
                    AmountFinancedOutputTbx.Text = "";
                    CostOfFinancingTbx.Text = "";
                    AmountFinancedTbx.Text = "";
                    AmountTbx.Text = "";
                    DisplayMessege(ValidationMessage.bothLoanAmountAmountFinancedBlank);
                    AmountTbx.Focus();
                    isValidated = false;
                }
                else if (!string.IsNullOrEmpty(AmountTbx.Text) && Convert.ToDouble(AmountTbx.Text) == 0.0 &&
                        !string.IsNullOrEmpty(AmountFinancedTbx.Text) && Convert.ToDouble(AmountFinancedTbx.Text) == 0.0)
                {
                    BankMethodGridView.Rows.Clear();
                    LoanAmountCalcTbx.Text = "";
                    RegularPaymentTbx.Text = "";
                    AmountFinancedOutputTbx.Text = "";
                    CostOfFinancingTbx.Text = "";
                    AmountFinancedTbx.Text = "";
                    AmountTbx.Text = "";
                    DisplayMessege(ValidationMessage.bothLoanAmountAmountFinancedZero);
                    AmountTbx.Focus();
                    isValidated = false;
                }
                else if ((!string.IsNullOrEmpty(AmountTbx.Text) && Convert.ToDouble(AmountTbx.Text) > 0) &&
                            (!string.IsNullOrEmpty(AmountFinancedTbx.Text) && Convert.ToDouble(AmountFinancedTbx.Text) > 0))
                {
                    DisplayMessege(ValidationMessage.LoanAmountAndAmountFinancedBothCannotBeGreaterThanZero);
                    AmountTbx.Focus();
                    isValidated = false;
                }
                else if (!string.IsNullOrEmpty(InterestDelayTbx.Text) && !regIntAmount.IsMatch(InterestDelayTbx.Text))
                {
                    DisplayMessege(ValidationMessage.InterestDelayCanBePositive);
                    InterestDelayTbx.Focus();
                    isValidated = false;
                }
                //Validate Days in Year and Days in Month values
                else if (DaysInYearCmb.Text.ToLower() == "actual" && (DaysInMonthCmb.Text == "30" || DaysInMonthCmb.Text == "Periodic"))
                {
                    DisplayMessege(ValidationMessage.InvalidDaysInYearValue);
                    DaysInYearCmb.Focus();
                    isValidated = false;
                }
                //Validate Residual value
                else if (!string.IsNullOrEmpty(ResidualTbx.Text) && !regAmount.IsMatch(ResidualTbx.Text))
                {
                    DisplayMessege(ValidationMessage.ValidResidualAmount);
                    ResidualTbx.Focus();
                    isValidated = false;
                }
                //Validate Payment amount value
                else if (!string.IsNullOrEmpty(PaymentAmountTbx.Text) && !regAmount.IsMatch(PaymentAmountTbx.Text))
                {
                    DisplayMessege(ValidationMessage.ValidPaymentAmount);
                    PaymentAmountTbx.Focus();
                    isValidated = false;
                }
                //Validate Enforcement Principal value
                else if (!string.IsNullOrEmpty(EnforcedPrincipalTbx.Text) && (!regAmount.IsMatch(EnforcedPrincipalTbx.Text) || Convert.ToDouble(EnforcedPrincipalTbx.Text) <= 0))
                {
                    DisplayMessege(ValidationMessage.ValidEnforcedPrincipal);
                    EnforcedPrincipalTbx.Focus();
                    isValidated = false;
                }
                else if (!string.IsNullOrEmpty(MinDurationTbx.Text) && !regIntAmount.IsMatch(MinDurationTbx.Text))
                {
                    DisplayMessege(ValidationMessage.ValidMinDuration);
                    MinDurationTbx.Focus();
                    isValidated = false;
                }
                //Validate Enforcement Payment value
                else if (!string.IsNullOrEmpty(EnforcedPaymentTbx.Text) && (!regIntAmount.IsMatch(EnforcedPaymentTbx.Text) || Convert.ToInt32(EnforcedPaymentTbx.Text) <= 0))
                {
                    DisplayMessege(ValidationMessage.ValidEnforcedPayment);
                    EnforcedPaymentTbx.Focus();
                    isValidated = false;
                }
                else if ((!string.IsNullOrEmpty(EnforcedPaymentTbx.Text) && string.IsNullOrEmpty(EnforcedPrincipalTbx.Text)) ||
                    (string.IsNullOrEmpty(EnforcedPaymentTbx.Text) && !string.IsNullOrEmpty(EnforcedPrincipalTbx.Text)))
                {
                    DisplayMessege(ValidationMessage.EnforcedPrincipalAndPayment);
                    EnforcedPrincipalTbx.Focus();
                    isValidated = false;
                }
                #endregion

                #region validate Fee tab controls

                //Validate Service Fee TextBox control
                else if (!string.IsNullOrEmpty(ServiceFeeTbx.Text) && !regAmount.IsMatch(ServiceFeeTbx.Text))
                {
                    DisplayMessege(ValidationMessage.ServiceFeeCanBePositive);
                    ServiceFeeTbx.Focus();
                    isValidated = false;
                }
                //Validate Service Fee Calculation Method ComboBox control
                else if (!string.IsNullOrEmpty(ServiceFeeTbx.Text) && ServiceFeeCmb.SelectedIndex == -1)
                {
                    DisplayMessege(ValidationMessage.SelectCalculationMethodForServiceFee);
                    ServiceFeeCmb.Focus();
                    isValidated = false;
                }
                else if (FinanceServiceFeeChb.Checked && (ServiceFeeCmb.SelectedIndex == 1 || ServiceFeeCmb.SelectedIndex == 3))
                {
                    DisplayMessege(ValidationMessage.InvalidSelectionOfCalculationMethodOfServiceFee);
                    ServiceFeeCmb.Focus();
                    isValidated = false;
                }
                //Validate Same Day Fee TextBox control
                else if (!string.IsNullOrEmpty(SameDayFeeTbx.Text) && !regAmount.IsMatch(SameDayFeeTbx.Text))
                {
                    DisplayMessege(ValidationMessage.SameDayFeeCanBePositive);
                    SameDayFeeTbx.Focus();
                    isValidated = false;
                }
                //Validate Origination Fee Fixed TextBox control
                else if (!string.IsNullOrEmpty(OriginationFeeFixedTbx.Text) && !regAmount.IsMatch(OriginationFeeFixedTbx.Text))
                {
                    DisplayMessege(ValidationMessage.OriginationFeeFixedCanBePositive);
                    OriginationFeeFixedTbx.Focus();
                    isValidated = false;
                }
                //Validate Origination Fee Percent TextBox control
                else if (!string.IsNullOrEmpty(OriginationFeePercentTbx.Text) && !regAmount.IsMatch(OriginationFeePercentTbx.Text))
                {
                    DisplayMessege(ValidationMessage.OriginationFeePercentCanBePositive);
                    OriginationFeePercentTbx.Focus();
                    isValidated = false;
                }
                else if (!string.IsNullOrEmpty(OriginationFeePercentTbx.Text) && Convert.ToDouble(OriginationFeePercentTbx.Text) == 0)
                {
                    DisplayMessege(ValidationMessage.OriginationFeePercentCannotBeZero);
                    OriginationFeePercentTbx.Focus();
                    isValidated = false;
                }
                //Validate Origination Fee Min TextBox control
                else if (!string.IsNullOrEmpty(OriginationFeeMinTbx.Text) && !regAmount.IsMatch(OriginationFeeMinTbx.Text))
                {
                    DisplayMessege(ValidationMessage.OriginationFeeMinCanBePositive);
                    OriginationFeeMinTbx.Focus();
                    isValidated = false;
                }
                //Validate Origination Fee Max TextBox control
                else if (!string.IsNullOrEmpty(OriginationFeeMaxTbx.Text) && !regAmount.IsMatch(OriginationFeeMaxTbx.Text))
                {
                    DisplayMessege(ValidationMessage.OriginationFeeMaxCanBePositive);
                    OriginationFeeMaxTbx.Focus();
                    isValidated = false;
                }
                //Validation for origination fee min cannot be greater than origination fee max
                else if (!string.IsNullOrEmpty(OriginationFeeMinTbx.Text) && !string.IsNullOrEmpty(OriginationFeeMaxTbx.Text) &&
                        Convert.ToDouble(OriginationFeeMinTbx.Text) > 0 && Convert.ToDouble(OriginationFeeMaxTbx.Text) > 0 &&
                        (Convert.ToDouble(OriginationFeeMinTbx.Text) > Convert.ToDouble(OriginationFeeMaxTbx.Text)))
                {
                    DisplayMessege(ValidationMessage.OriginationFeeMinGreaterThanOriginationFeeMax);
                    OriginationFeeMinTbx.Focus();
                    isValidated = false;
                }
                //Validate Balloon Payment value
                else if (!string.IsNullOrEmpty(BalloonPaymentTbx.Text) && !regAmount.IsMatch(BalloonPaymentTbx.Text))
                {
                    DisplayMessege(ValidationMessage.ValidBalloonPayment);
                    BalloonPaymentTbx.Focus();
                    isValidated = false;
                }
                else if ((!string.IsNullOrEmpty(BalloonPaymentTbx.Text) && Convert.ToDouble(BalloonPaymentTbx.Text) > 0) && (!string.IsNullOrEmpty(ResidualTbx.Text) && Convert.ToDouble(ResidualTbx.Text) > 0))
                {
                    DisplayMessege(ValidationMessage.ValidResidualOrBalloonPayment);
                    BalloonPaymentTbx.Focus();
                    isValidated = false;
                }
                #endregion

                #region Validate Maintenance Fee tab controls

                //Validate Maintenance Fee TextBox control
                else if (!string.IsNullOrEmpty(MaintenanceFeeTbx.Text) && !regAmount.IsMatch(MaintenanceFeeTbx.Text))
                {
                    DisplayMessege(ValidationMessage.MaintenanceFeeCanBePositive);
                    MaintenanceFeeTbx.Focus();
                    isValidated = false;
                }
                //Validate Maintenance Fee Percent TextBox control
                else if (!string.IsNullOrEmpty(MaintenanceFeePercentTbx.Text) && !regAmount.IsMatch(MaintenanceFeePercentTbx.Text))
                {
                    DisplayMessege(ValidationMessage.MaintenanceFeePercentCanBePositive);
                    MaintenanceFeePercentTbx.Focus();
                    isValidated = false;
                }
                else if (!string.IsNullOrEmpty(MaintenanceFeePercentTbx.Text) && Convert.ToDouble(MaintenanceFeePercentTbx.Text) == 0)
                {
                    DisplayMessege(ValidationMessage.MaintenanceFeePercentCannotBeZero);
                    MaintenanceFeePercentTbx.Focus();
                    isValidated = false;
                }
                //Validate Maintenance Fee Basis TextBox control
                else if (!string.IsNullOrEmpty(MaintenanceFeePercentTbx.Text) && MaintenanceFeeBasisCmb.SelectedIndex == -1)
                {
                    DisplayMessege(ValidationMessage.MaintenanceFeeBasisRequired);
                    MaintenanceFeeBasisCmb.Focus();
                    isValidated = false;
                }
                else if (string.IsNullOrEmpty(MaintenanceFeePercentTbx.Text) && MaintenanceFeeBasisCmb.SelectedIndex != -1)
                {
                    DisplayMessege(ValidationMessage.MaintenanceFeePercentRequired);
                    MaintenanceFeePercentTbx.Focus();
                    isValidated = false;
                }
                //Validate Maintenance Fee Frequency TextBox control
                else if ((!string.IsNullOrEmpty(MaintenanceFeeTbx.Text) || !string.IsNullOrEmpty(MaintenanceFeePercentTbx.Text)) && MaintenanceFeeFreqCmb.SelectedIndex == -1)
                {
                    DisplayMessege(ValidationMessage.MaintenanceFeeFrequency);
                    MaintenanceFeeFreqCmb.Focus();
                    isValidated = false;
                }
                else if ((MaintenanceFeeBasisCmb.SelectedIndex == (int)Constants.FeeBasis.ActualPayment || MaintenanceFeeBasisCmb.SelectedIndex == (int)Constants.FeeBasis.ScheduledPayment) &&
                        (MaintenanceFeeFreqCmb.SelectedIndex == (int)Constants.FeeFrequency.Days || MaintenanceFeeFreqCmb.SelectedIndex == (int)Constants.FeeFrequency.Months))
                {
                    DisplayMessege(ValidationMessage.ValidMaintenanceFeeFrequency);
                    MaintenanceFeeFreqCmb.Focus();
                    isValidated = false;
                }
                //Validate Maintenance Fee Frequency Num TextBox control
                else if (!string.IsNullOrEmpty(MaintenanceFeeFreqNumTbx.Text) && !regIntAmount.IsMatch(MaintenanceFeeFreqNumTbx.Text))
                {
                    DisplayMessege(ValidationMessage.MaintenanceFeeFrequencyNumCanBePositve);
                    MaintenanceFeeFreqNumTbx.Focus();
                    isValidated = false;
                }
                else if ((!string.IsNullOrEmpty(MaintenanceFeeTbx.Text) || !string.IsNullOrEmpty(MaintenanceFeePercentTbx.Text)) &&
                            (string.IsNullOrEmpty(MaintenanceFeeFreqNumTbx.Text) || Convert.ToInt32(MaintenanceFeeFreqNumTbx.Text) == 0))
                {
                    DisplayMessege(ValidationMessage.MaintenanceFeeFrequencyNumGreaterThanZero);
                    MaintenanceFeeFreqNumTbx.Focus();
                    isValidated = false;
                }
                //Validate Maintenance Fee Delay TextBox control
                else if (!string.IsNullOrEmpty(MaintenanceFeeDelayTbx.Text) && !regIntAmount.IsMatch(MaintenanceFeeDelayTbx.Text))
                {
                    DisplayMessege(ValidationMessage.MaintenanceFeeDelayCanBePositve);
                    MaintenanceFeeDelayTbx.Focus();
                    isValidated = false;
                }
                //Validate Maintenance Fee Min TextBox control
                else if (!string.IsNullOrEmpty(MaintenanceFeeMinTbx.Text) && !regAmount.IsMatch(MaintenanceFeeMinTbx.Text))
                {
                    DisplayMessege(ValidationMessage.MaintenanceFeeMinCanBePositve);
                    MaintenanceFeeMinTbx.Focus();
                    isValidated = false;
                }
                //Validate Maintenance Fee Max per TextBox control
                else if (!string.IsNullOrEmpty(MaintenanceFeeMaxPerTbx.Text) && !regAmount.IsMatch(MaintenanceFeeMaxPerTbx.Text))
                {
                    DisplayMessege(ValidationMessage.MaintenanceFeeMaxPerCanBePositive);
                    MaintenanceFeeMaxPerTbx.Focus();
                    isValidated = false;
                }
                //Validation for Maintenance fee min cannot be greater than Maintenance fee max per
                else if (!string.IsNullOrEmpty(MaintenanceFeeMinTbx.Text) && !string.IsNullOrEmpty(MaintenanceFeeMaxPerTbx.Text) &&
                        Convert.ToDouble(MaintenanceFeeMinTbx.Text) > 0 && Convert.ToDouble(MaintenanceFeeMaxPerTbx.Text) > 0 &&
                        (Convert.ToDouble(MaintenanceFeeMinTbx.Text) > Convert.ToDouble(MaintenanceFeeMaxPerTbx.Text)))
                {
                    DisplayMessege(ValidationMessage.MaintenanceFeeMinGreaterThanMaxValue);
                    MaintenanceFeeMinTbx.Focus();
                    isValidated = false;
                }
                //Validate Maintenance Fee Max Month TextBox control
                else if (!string.IsNullOrEmpty(MaintenanceFeeMaxMonthTbx.Text) && !regAmount.IsMatch(MaintenanceFeeMaxMonthTbx.Text))
                {
                    DisplayMessege(ValidationMessage.MaintenanceFeeMaxMonthCanBePositve);
                    MaintenanceFeeMaxMonthTbx.Focus();
                    isValidated = false;
                }
                //Validate Maintenance Fee Max Loan TextBox control
                else if (!string.IsNullOrEmpty(MaintenanceFeeMaxLoanTbx.Text) && !regAmount.IsMatch(MaintenanceFeeMaxLoanTbx.Text))
                {
                    DisplayMessege(ValidationMessage.MaintenanceFeeMaxLoanCanBePositve);
                    MaintenanceFeeMaxLoanTbx.Focus();
                    isValidated = false;
                }
                else if (!string.IsNullOrEmpty(MaintenanceFeeMaxMonthTbx.Text) && !string.IsNullOrEmpty(MaintenanceFeeMaxLoanTbx.Text) && (Convert.ToDouble(MaintenanceFeeMaxMonthTbx.Text) > Convert.ToDouble(MaintenanceFeeMaxLoanTbx.Text)))
                {
                    DisplayMessege(ValidationMessage.ValidateMaintenanceMaxMonthWithMaxLoan);
                    MaintenanceFeeMaxMonthTbx.Focus();
                    isValidated = false;
                }

                #endregion

                #region Validate Management Fee tab controls

                //Validate Management Fee TextBox control
                else if (!string.IsNullOrEmpty(ManagementFeeTbx.Text) && !regAmount.IsMatch(ManagementFeeTbx.Text))
                {
                    DisplayMessege(ValidationMessage.ManagementFeeCanBePositive);
                    ManagementFeeTbx.Focus();
                    isValidated = false;
                }
                //Validate Management Fee Percent TextBox control
                else if (!string.IsNullOrEmpty(ManagementFeePercentTbx.Text) && !regAmount.IsMatch(ManagementFeePercentTbx.Text))
                {
                    DisplayMessege(ValidationMessage.ManagementFeePercentCanBePositive);
                    ManagementFeePercentTbx.Focus();
                    isValidated = false;
                }
                else if (!string.IsNullOrEmpty(ManagementFeePercentTbx.Text) && Convert.ToDouble(ManagementFeePercentTbx.Text) == 0)
                {
                    DisplayMessege(ValidationMessage.ManagementFeeCannotBeZero);
                    ManagementFeePercentTbx.Focus();
                    isValidated = false;
                }
                //Validate Management Fee Basis TextBox control
                else if (!string.IsNullOrEmpty(ManagementFeePercentTbx.Text) && ManagementFeeBasisCmb.SelectedIndex == -1)
                {
                    DisplayMessege(ValidationMessage.ManagementFeeBasisRequired);
                    ManagementFeeBasisCmb.Focus();
                    isValidated = false;
                }
                else if (string.IsNullOrEmpty(ManagementFeePercentTbx.Text) && ManagementFeeBasisCmb.SelectedIndex != -1)
                {
                    DisplayMessege(ValidationMessage.ManagementFeePercentRequired);
                    ManagementFeePercentTbx.Focus();
                    isValidated = false;
                }
                //Validate Management Fee Frequency TextBox control
                else if ((!string.IsNullOrEmpty(ManagementFeeTbx.Text) || !string.IsNullOrEmpty(ManagementFeePercentTbx.Text)) && ManagementFeeFreqCmb.SelectedIndex == -1)
                {
                    DisplayMessege(ValidationMessage.ManagementFeeFrequency);
                    ManagementFeeFreqCmb.Focus();
                    isValidated = false;
                }
                else if ((ManagementFeeBasisCmb.SelectedIndex == (int)Constants.FeeBasis.ActualPayment || ManagementFeeBasisCmb.SelectedIndex == (int)Constants.FeeBasis.ScheduledPayment) &&
                            (ManagementFeeFreqCmb.SelectedIndex == (int)Constants.FeeFrequency.Days || ManagementFeeFreqCmb.SelectedIndex == (int)Constants.FeeFrequency.Months))
                {
                    DisplayMessege(ValidationMessage.ValidManagementFeeFrequency);
                    ManagementFeeFreqCmb.Focus();
                    isValidated = false;
                }
                //Validate Management Fee Frequency Num TextBox control
                else if (!string.IsNullOrEmpty(ManagementFeeFreqNumTbx.Text) && !regIntAmount.IsMatch(ManagementFeeFreqNumTbx.Text))
                {
                    DisplayMessege(ValidationMessage.ManagementFeeFrequencyNumCanBePositve);
                    ManagementFeeFreqNumTbx.Focus();
                    isValidated = false;
                }
                else if ((!string.IsNullOrEmpty(ManagementFeeTbx.Text) || !string.IsNullOrEmpty(ManagementFeePercentTbx.Text)) &&
                            (string.IsNullOrEmpty(ManagementFeeFreqNumTbx.Text) || Convert.ToInt32(ManagementFeeFreqNumTbx.Text) == 0))
                {
                    DisplayMessege(ValidationMessage.ManagementFeeFrequencyNumGreaterThanZero);
                    ManagementFeeFreqNumTbx.Focus();
                    isValidated = false;
                }
                //Validate Management Fee Delay TextBox control
                else if (!string.IsNullOrEmpty(ManagementFeeDelayTbx.Text) && !regIntAmount.IsMatch(ManagementFeeDelayTbx.Text))
                {
                    DisplayMessege(ValidationMessage.ManagementFeeDelayCanBePositve);
                    ManagementFeeDelayTbx.Focus();
                    isValidated = false;
                }
                //Validate Management Fee Min TextBox control
                else if (!string.IsNullOrEmpty(ManagementFeeMinTbx.Text) && !regAmount.IsMatch(ManagementFeeMinTbx.Text))
                {
                    DisplayMessege(ValidationMessage.ManagementFeeMinCanBePositve);
                    ManagementFeeMinTbx.Focus();
                    isValidated = false;
                }
                //Validate Management Fee Max per TextBox control
                else if (!string.IsNullOrEmpty(ManagementFeeMaxPerTbx.Text) && !regAmount.IsMatch(ManagementFeeMaxPerTbx.Text))
                {
                    DisplayMessege(ValidationMessage.ManagementFeeMaxPerCanBePositive);
                    ManagementFeeMaxPerTbx.Focus();
                    isValidated = false;
                }
                //Validation for Management fee min cannot be greater than Management fee max per
                else if (!string.IsNullOrEmpty(ManagementFeeMinTbx.Text) && !string.IsNullOrEmpty(ManagementFeeMaxPerTbx.Text) &&
                        Convert.ToDouble(ManagementFeeMinTbx.Text) > 0 && Convert.ToDouble(ManagementFeeMaxPerTbx.Text) > 0 &&
                        (Convert.ToDouble(ManagementFeeMinTbx.Text) > Convert.ToDouble(ManagementFeeMaxPerTbx.Text)))
                {
                    DisplayMessege(ValidationMessage.ManagementFeeMinGreaterThanMaxValue);
                    ManagementFeeMinTbx.Focus();
                    isValidated = false;
                }
                //Validate Management Fee Max Month TextBox control
                else if (!string.IsNullOrEmpty(ManagementFeeMaxMonthTbx.Text) && !regAmount.IsMatch(ManagementFeeMaxMonthTbx.Text))
                {
                    DisplayMessege(ValidationMessage.ManagementFeeMaxMonthCanBePositve);
                    ManagementFeeMaxMonthTbx.Focus();
                    isValidated = false;
                }
                //Validate Management Fee Max Loan TextBox control
                else if (!string.IsNullOrEmpty(ManagementFeeMaxLoanTbx.Text) && !regAmount.IsMatch(ManagementFeeMaxLoanTbx.Text))
                {
                    DisplayMessege(ValidationMessage.ManagementFeeMaxLoanCanBePositve);
                    ManagementFeeMaxLoanTbx.Focus();
                    isValidated = false;
                }
                else if (!string.IsNullOrEmpty(ManagementFeeMaxMonthTbx.Text) && !string.IsNullOrEmpty(ManagementFeeMaxLoanTbx.Text) && (Convert.ToDouble(ManagementFeeMaxMonthTbx.Text) > Convert.ToDouble(ManagementFeeMaxLoanTbx.Text)))
                {
                    DisplayMessege(ValidationMessage.ValidateManagementMaxMonthWithMaxLoan);
                    ManagementFeeMaxMonthTbx.Focus();
                    isValidated = false;
                }

                #endregion

                #region Validate Earned Fee tab controls

                else if (!regAmount.IsMatch(EarnedInterestTbx.Text))
                {
                    DisplayMessege(ValidationMessage.EarnedInterestCanOnlyBePositive);
                    EarnedInterestTbx.Focus();
                    isValidated = false;
                }
                else if (!regAmount.IsMatch(EarnedServiceFeeInterestTbx.Text))
                {
                    DisplayMessege(ValidationMessage.EarnedServiceFeeInterestCanOnlyBePositive);
                    EarnedServiceFeeInterestTbx.Focus();
                    isValidated = false;
                }
                else if (!regAmount.IsMatch(EarnedOriginationFeeTbx.Text))
                {
                    DisplayMessege(ValidationMessage.EarnedOriginationFeeCanOnlyBePositive);
                    EarnedOriginationFeeTbx.Focus();
                    isValidated = false;
                }
                else if (!regAmount.IsMatch(EarnedSameDayFeeTbx.Text))
                {
                    DisplayMessege(ValidationMessage.EarnedSameDayFeeCanOnlyBePositive);
                    EarnedSameDayFeeTbx.Focus();
                    isValidated = false;
                }
                else if (!regAmount.IsMatch(EarnedManagementFeeTbx.Text))
                {
                    DisplayMessege(ValidationMessage.EarnedManagementFeeCanOnlyBePositive);
                    EarnedManagementFeeTbx.Focus();
                    isValidated = false;
                }
                else if (!regAmount.IsMatch(EarnedMaintenanceFeeTbx.Text))
                {
                    DisplayMessege(ValidationMessage.EarnedMaintenanceFeeCanOnlyBePositive);
                    EarnedMaintenanceFeeTbx.Focus();
                    isValidated = false;
                }
                else if (!regAmount.IsMatch(EarnedNSFFeeTbx.Text))
                {
                    DisplayMessege(ValidationMessage.EarnedNSFFeeCanOnlyBePositive);
                    EarnedNSFFeeTbx.Focus();
                    isValidated = false;
                }
                else if (!regAmount.IsMatch(EarnedLateFeeTbx.Text))
                {
                    DisplayMessege(ValidationMessage.EarnedLateFeeCanOnlyBePositive);
                    EarnedLateFeeTbx.Focus();
                    isValidated = false;
                }

                #endregion

                #region Validate Interest Tier tab controls

                else if (string.IsNullOrEmpty(InterestRateTbx.Text))
                {
                    DisplayMessege(ValidationMessage.EnterAnnualInterestRate);
                    InterestRateTbx.Focus();
                    isValidated = false;
                }
                else if (!regAmountThreeDecimal.IsMatch(InterestRateTbx.Text))
                {
                    DisplayMessege(ValidationMessage.AnnualInterestRateCanBePositive);
                    InterestRateTbx.Focus();
                    isValidated = false;
                }
                else if (!regAmount.IsMatch(InterestTier1Tbx.Text))
                {
                    DisplayMessege(ValidationMessage.InterestTierCanBePositive);
                    InterestTier1Tbx.Focus();
                    isValidated = false;
                }
                else if (!string.IsNullOrEmpty(InterestTier1Tbx.Text) && Convert.ToDouble(InterestTier1Tbx.Text) <= 0)
                {
                    DisplayMessege(ValidationMessage.InterestTier1MustBeGreaterThanZero);
                    InterestTier1Tbx.Focus();
                    isValidated = false;
                }
                else if (!regAmountThreeDecimal.IsMatch(InterestRate2Tbx.Text))
                {
                    DisplayMessege(ValidationMessage.AnnualInterestRateCanBePositive);
                    InterestRate2Tbx.Focus();
                    isValidated = false;
                }
                else if (!regAmount.IsMatch(InterestTier2Tbx.Text))
                {
                    DisplayMessege(ValidationMessage.InterestTierCanBePositive);
                    InterestTier2Tbx.Focus();
                    isValidated = false;
                }
                else if (!regAmountThreeDecimal.IsMatch(InterestRate3Tbx.Text))
                {
                    DisplayMessege(ValidationMessage.AnnualInterestRateCanBePositive);
                    InterestRate3Tbx.Focus();
                    isValidated = false;
                }
                else if (!regAmount.IsMatch(InterestTier3Tbx.Text))
                {
                    DisplayMessege(ValidationMessage.InterestTierCanBePositive);
                    InterestTier3Tbx.Focus();
                    isValidated = false;
                }
                else if (!regAmountThreeDecimal.IsMatch(InterestRate4Tbx.Text))
                {
                    DisplayMessege(ValidationMessage.AnnualInterestRateCanBePositive);
                    InterestRate4Tbx.Focus();
                    isValidated = false;
                }
                else if (!regAmount.IsMatch(InterestTier4Tbx.Text))
                {
                    DisplayMessege(ValidationMessage.InterestTierCanBePositive);
                    InterestTier4Tbx.Focus();
                    isValidated = false;
                }
                else if ((string.IsNullOrEmpty(InterestTier1Tbx.Text) && (!string.IsNullOrEmpty(InterestTier2Tbx.Text) || !string.IsNullOrEmpty(InterestTier3Tbx.Text) || !string.IsNullOrEmpty(InterestTier4Tbx.Text))) ||
                           (string.IsNullOrEmpty(InterestTier2Tbx.Text) && (!string.IsNullOrEmpty(InterestTier3Tbx.Text) || !string.IsNullOrEmpty(InterestTier4Tbx.Text))) ||
                               (string.IsNullOrEmpty(InterestTier3Tbx.Text) && !string.IsNullOrEmpty(InterestTier4Tbx.Text)))
                {
                    DisplayMessege(ValidationMessage.InvalidBalanceCapValues);
                    InterestTier1Tbx.Focus();
                    isValidated = false;
                }
                else if (!string.IsNullOrEmpty(InterestTier1Tbx.Text) && !string.IsNullOrEmpty(InterestTier2Tbx.Text) &&
                            Convert.ToDouble(InterestTier2Tbx.Text) <= Convert.ToDouble(InterestTier1Tbx.Text))
                {
                    DisplayMessege(ValidationMessage.InterestTierMustGreaterPreviousTier);
                    InterestTier2Tbx.Focus();
                    isValidated = false;
                }
                else if (!string.IsNullOrEmpty(InterestTier2Tbx.Text) && !string.IsNullOrEmpty(InterestTier3Tbx.Text) &&
                            Convert.ToDouble(InterestTier3Tbx.Text) <= Convert.ToDouble(InterestTier2Tbx.Text))
                {
                    DisplayMessege(ValidationMessage.InterestTierMustGreaterPreviousTier);
                    InterestTier3Tbx.Focus();
                    isValidated = false;
                }
                else if (!string.IsNullOrEmpty(InterestTier3Tbx.Text) && !string.IsNullOrEmpty(InterestTier4Tbx.Text) &&
                         Convert.ToDouble(InterestTier4Tbx.Text) <= Convert.ToDouble(InterestTier3Tbx.Text))
                {
                    DisplayMessege(ValidationMessage.InterestTierMustGreaterPreviousTier);
                    InterestTier4Tbx.Focus();
                    isValidated = false;
                }

                #endregion

                #region Validate Priority tab controls

                //Validation for Priority order
                else if (IntrestPriorityCmb.SelectedIndex != -1 || PrincipalPriorityCmb.SelectedIndex != -1 || ServiceFeePriorityCmb.SelectedIndex != -1 ||
                    ServiceFeeInterestPriorityCmb.SelectedIndex != -1 || OriginationFeePriorityCmb.SelectedIndex != -1 || ManagementFeePriorityCmb.SelectedIndex != -1 ||
                    MaintenanceFeePriorityCmb.SelectedIndex != -1 || NSFFeePriorityCmb.SelectedIndex != -1 || LateFeePriorityCmb.SelectedIndex != -1 ||
                    SameDayFeePriorityCmb.SelectedIndex != -1)
                {
                    if (IntrestPriorityCmb.SelectedIndex == -1 || PrincipalPriorityCmb.SelectedIndex == -1 || ServiceFeePriorityCmb.SelectedIndex == -1 ||
                    ServiceFeeInterestPriorityCmb.SelectedIndex == -1 || OriginationFeePriorityCmb.SelectedIndex == -1 || ManagementFeePriorityCmb.SelectedIndex == -1 ||
                    MaintenanceFeePriorityCmb.SelectedIndex == -1 || NSFFeePriorityCmb.SelectedIndex == -1 || LateFeePriorityCmb.SelectedIndex == -1 ||
                    SameDayFeePriorityCmb.SelectedIndex == -1)
                    {
                        DisplayMessege(ValidationMessage.PriorityValues);
                        IntrestPriorityCmb.Focus();
                        isValidated = false;
                    }
                    else
                    {
                        int[] a = new int[10];
                        a[Convert.ToInt32(IntrestPriorityCmb.Text) - 1]++;
                        a[Convert.ToInt32(PrincipalPriorityCmb.Text) - 1]++;
                        a[Convert.ToInt32(ServiceFeePriorityCmb.Text) - 1]++;
                        a[Convert.ToInt32(ServiceFeeInterestPriorityCmb.Text) - 1]++;
                        a[Convert.ToInt32(OriginationFeePriorityCmb.Text) - 1]++;
                        a[Convert.ToInt32(ManagementFeePriorityCmb.Text) - 1]++;
                        a[Convert.ToInt32(MaintenanceFeePriorityCmb.Text) - 1]++;
                        a[Convert.ToInt32(NSFFeePriorityCmb.Text) - 1]++;
                        a[Convert.ToInt32(LateFeePriorityCmb.Text) - 1]++;
                        a[Convert.ToInt32(SameDayFeePriorityCmb.Text) - 1]++;
                        for (int i = 0; i < 10; i++)
                        {
                            if (a[i] > 1)
                            {
                                DisplayMessege(ValidationMessage.DifferentPriorityValues);
                                IntrestPriorityCmb.Focus();
                                isValidated = false;
                                break;
                            }
                        }
                    }
                }

                if (!isValidated)
                {
                    return isValidated;
                }

                #endregion

                #region Validate Input Grid View

                //Validate Input Grid values
                if (InputGridView.Rows.Count <= 2)
                {
                    DisplayMessege(ValidationMessage.ProvideLoanSchedule);
                    InputGridView.Focus();
                    isValidated = false;
                }
                else if (InputGridView.Rows.Count > 1)
                {
                    for (int i = 0; i < InputGridView.Rows.Count - 1; i++)
                    {
                        isValidated = ValidateDate(Convert.ToString(InputGridView.Rows[i].Cells["DateIn"].Value), ValidationMessage.ValidDate, false);

                        if (!isValidated)
                        {
                            InputGridView.Focus();
                            return isValidated;
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["Flags"].Value)) && !regIdValue.IsMatch(Convert.ToString(InputGridView.Rows[i].Cells["Flags"].Value)))
                        {
                            DisplayMessege(ValidationMessage.FlagValueInInputGridCanBeNumber);
                            InputGridView.Focus();
                            isValidated = false;
                        }
                        else if (!string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["Flags"].Value)) &&
                                Convert.ToInt32(Convert.ToString(InputGridView.Rows[i].Cells["Flags"].Value)) != (int)Constants.FlagValues.Payment &&
                                Convert.ToInt32(Convert.ToString(InputGridView.Rows[i].Cells["Flags"].Value)) != (int)Constants.FlagValues.SkipPayment)
                        {
                            DisplayMessege(ValidationMessage.FlagValueInInputGrid);
                            InputGridView.Focus();
                            isValidated = false;
                        }
                        else if (string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["PaymentID"].Value)))
                        {
                            DisplayMessege(ValidationMessage.PaymentID);
                            InputGridView.Focus();
                            isValidated = false;
                        }
                        else if (!string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["PaymentID"].Value)) && !regIdValue.IsMatch(Convert.ToString(InputGridView.Rows[i].Cells["PaymentID"].Value)))
                        {
                            DisplayMessege(ValidationMessage.PaymentIDInInputGrid);
                            InputGridView.Focus();
                            isValidated = false;
                        }
                        else if (i == 0 && (!string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["EffectiveDate"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["PaymentAmountSchedule"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["InterestRate"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["InterestPriority"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["PrincipalPriority"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["OriginationFeePriority"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["SameDayFeePriority"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["ServiceFeePriority"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["ServiceFeeInterestPriority"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["ManagementFeePriority"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["MaintenanceFeePriority"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["NSFFeePriority"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["LateFeePriority"].Value))))
                        {
                            DisplayMessege(ValidationMessage.OtherValuesOnLoanTakenDate);
                            InputGridView.Focus();
                            isValidated = false;
                        }
                        else if (!string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["EffectiveDate"].Value)))
                        {
                            isValidated = ValidateDate(Convert.ToString(InputGridView.Rows[i].Cells["EffectiveDate"].Value), ValidationMessage.EffectiveDateInInputGrid, false);
                        }

                        if (!isValidated)
                        {
                            InputGridView.Focus();
                            return isValidated;
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["PaymentAmountSchedule"].Value)) && !regAmount.IsMatch(Convert.ToString(InputGridView.Rows[i].Cells["PaymentAmountSchedule"].Value)))
                        {
                            DisplayMessege(ValidationMessage.PaymentAmountInInputGrid);
                            InputGridView.Focus();
                            isValidated = false;
                        }
                        else if (!string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["InterestRate"].Value)) && !regAmountThreeDecimal.IsMatch(Convert.ToString(InputGridView.Rows[i].Cells["InterestRate"].Value)))
                        {
                            DisplayMessege(ValidationMessage.InterestRateValueInInputGrid);
                            InputGridView.Focus();
                            isValidated = false;
                        }
                        else if (!string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["InterestPriority"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["PrincipalPriority"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["OriginationFeePriority"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["SameDayFeePriority"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["ServiceFeePriority"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["ServiceFeeInterestPriority"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["ManagementFeePriority"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["MaintenanceFeePriority"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["NSFFeePriority"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[i].Cells["LateFeePriority"].Value)))
                        {
                            if (!regIntAmount.IsMatch(Convert.ToString(InputGridView.Rows[i].Cells["InterestPriority"].Value)) || Convert.ToInt32(Convert.ToString(InputGridView.Rows[i].Cells["InterestPriority"].Value)) > 10 ||
                                    !regIntAmount.IsMatch(Convert.ToString(InputGridView.Rows[i].Cells["PrincipalPriority"].Value)) || Convert.ToInt32(Convert.ToString(InputGridView.Rows[i].Cells["PrincipalPriority"].Value)) > 10 ||
                                    !regIntAmount.IsMatch(Convert.ToString(InputGridView.Rows[i].Cells["OriginationFeePriority"].Value)) || Convert.ToInt32(Convert.ToString(InputGridView.Rows[i].Cells["OriginationFeePriority"].Value)) > 10 ||
                                    !regIntAmount.IsMatch(Convert.ToString(InputGridView.Rows[i].Cells["SameDayFeePriority"].Value)) || Convert.ToInt32(Convert.ToString(InputGridView.Rows[i].Cells["SameDayFeePriority"].Value)) > 10 ||
                                    !regIntAmount.IsMatch(Convert.ToString(InputGridView.Rows[i].Cells["ServiceFeePriority"].Value)) || Convert.ToInt32(Convert.ToString(InputGridView.Rows[i].Cells["ServiceFeePriority"].Value)) > 10 ||
                                    !regIntAmount.IsMatch(Convert.ToString(InputGridView.Rows[i].Cells["ServiceFeeInterestPriority"].Value)) || Convert.ToInt32(Convert.ToString(InputGridView.Rows[i].Cells["ServiceFeeInterestPriority"].Value)) > 10 ||
                                    !regIntAmount.IsMatch(Convert.ToString(InputGridView.Rows[i].Cells["ManagementFeePriority"].Value)) || Convert.ToInt32(Convert.ToString(InputGridView.Rows[i].Cells["ManagementFeePriority"].Value)) > 10 ||
                                    !regIntAmount.IsMatch(Convert.ToString(InputGridView.Rows[i].Cells["MaintenanceFeePriority"].Value)) || Convert.ToInt32(Convert.ToString(InputGridView.Rows[i].Cells["MaintenanceFeePriority"].Value)) > 10 ||
                                    !regIntAmount.IsMatch(Convert.ToString(InputGridView.Rows[i].Cells["NSFFeePriority"].Value)) || Convert.ToInt32(Convert.ToString(InputGridView.Rows[i].Cells["NSFFeePriority"].Value)) > 10 ||
                                    !regIntAmount.IsMatch(Convert.ToString(InputGridView.Rows[i].Cells["LateFeePriority"].Value)) || Convert.ToInt32(Convert.ToString(InputGridView.Rows[i].Cells["LateFeePriority"].Value)) > 10)
                            {
                                DisplayMessege(ValidationMessage.PriorityValues);
                                InputGridView.Focus();
                                isValidated = false;
                            }
                            else
                            {
                                int[] a = new int[10];
                                a[Convert.ToInt32(Convert.ToString(InputGridView.Rows[i].Cells["InterestPriority"].Value)) - 1]++;
                                a[Convert.ToInt32(Convert.ToString(InputGridView.Rows[i].Cells["PrincipalPriority"].Value)) - 1]++;
                                a[Convert.ToInt32(Convert.ToString(InputGridView.Rows[i].Cells["OriginationFeePriority"].Value)) - 1]++;
                                a[Convert.ToInt32(Convert.ToString(InputGridView.Rows[i].Cells["SameDayFeePriority"].Value)) - 1]++;
                                a[Convert.ToInt32(Convert.ToString(InputGridView.Rows[i].Cells["ServiceFeePriority"].Value)) - 1]++;
                                a[Convert.ToInt32(Convert.ToString(InputGridView.Rows[i].Cells["ServiceFeeInterestPriority"].Value)) - 1]++;
                                a[Convert.ToInt32(Convert.ToString(InputGridView.Rows[i].Cells["ManagementFeePriority"].Value)) - 1]++;
                                a[Convert.ToInt32(Convert.ToString(InputGridView.Rows[i].Cells["MaintenanceFeePriority"].Value)) - 1]++;
                                a[Convert.ToInt32(Convert.ToString(InputGridView.Rows[i].Cells["NSFFeePriority"].Value)) - 1]++;
                                a[Convert.ToInt32(Convert.ToString(InputGridView.Rows[i].Cells["LateFeePriority"].Value)) - 1]++;
                                for (int j = 0; j < 10; j++)
                                {
                                    if (a[j] > 1)
                                    {
                                        DisplayMessege(ValidationMessage.DifferentPriorityValues);
                                        InputGridView.Focus();
                                        isValidated = false;
                                        break;
                                    }
                                }
                            }
                        }

                        if (!isValidated)
                        {
                            return isValidated;
                        }
                    }
                }

                #endregion

                #region  Validate Early Payoff TextBox input field
                if (!string.IsNullOrEmpty(EarlyPayoffDateTbx.Text))
                {
                    try
                    {
                        DateTime earlypayoff = Convert.ToDateTime(EarlyPayoffDateTbx.Text);
                        if ((earlypayoff <= Convert.ToDateTime(Convert.ToString(InputGridView.Rows[0].Cells["DateIn"].Value))) &&
                            earlypayoff != Convert.ToDateTime(Constants.DefaultDate))
                        {
                            DisplayMessege(ValidationMessage.EarlyPayoffDate);
                            EarlyPayoffDateTbx.Focus();
                            isValidated = false;
                        }
                    }
                    catch
                    {
                        DisplayMessege(ValidationMessage.EarlyPayoffDateFormat);
                        EarlyPayoffDateTbx.Focus();
                        return false;
                    }
                }
                else
                {
                    isValidated = ValidateDate(EarlyPayoffDateTbx.Text, ValidationMessage.EarlyPayoffDateFormat, true);
                    if (!isValidated)
                    {
                        EarlyPayoffDateTbx.Focus();
                        return isValidated;
                    }
                }
                #endregion

                #region Validate Additional Payment Grid View

                //Validate Additional Payment Input Grid values
                if (BankAdditionalPaymentGridView.Rows.Count > 1)
                {
                    for (int i = 0; i < BankAdditionalPaymentGridView.Rows.Count - 1; i++)
                    {
                        if (string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["DateInAdditional"].Value)))
                        {
                            DisplayMessege(ValidationMessage.ValidDateInAdditionalPaymentGrid);
                            BankAdditionalPaymentGridView.Focus();
                            isValidated = false;
                        }
                        else if (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["DateInAdditional"].Value)))
                        {
                            isValidated = ValidateDate(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["DateInAdditional"].Value), ValidationMessage.ValidDateInAdditionalPaymentGrid, false);
                        }

                        if (!isValidated)
                        {
                            BankAdditionalPaymentGridView.Focus();
                            return isValidated;
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["AdditionalPaymentBank"].Value)) &&
                                            !regAmount.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["AdditionalPaymentBank"].Value)))
                        {
                            DisplayMessege(ValidationMessage.PaymentAmountInAdditionalPaymentGrid);
                            BankAdditionalPaymentGridView.Focus();
                            isValidated = false;
                        }
                        else if (!regIdValue.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["FlagsAdditionalBank"].Value)))
                        {
                            DisplayMessege(ValidationMessage.FlagInAdditionalPaymentGrid);
                            BankAdditionalPaymentGridView.Focus();
                            isValidated = false;
                        }
                        else if ((string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["AdditionalPaymentBank"].Value)) &&
                                    !string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["FlagsAdditionalBank"].Value)) &&
                                    Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["FlagsAdditionalBank"].Value)) != (int)Constants.FlagValues.Discount) ||
                                    (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["AdditionalPaymentBank"].Value)) &&
                                            Convert.ToDouble(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["AdditionalPaymentBank"].Value)) <= 0))
                        {
                            DisplayMessege(ValidationMessage.PaymentAmountInAdditionalPaymentGrid);
                            BankAdditionalPaymentGridView.Focus();
                            isValidated = false;
                        }
                        else if (string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["PaymentIDAdditionalBank"].Value)))
                        {
                            DisplayMessege(ValidationMessage.PaymentID);
                            BankAdditionalPaymentGridView.Focus();
                            isValidated = false;
                        }
                        else if (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["PaymentIDAdditionalBank"].Value)) &&
                                            !regIdValue.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["PaymentIDAdditionalBank"].Value)))
                        {
                            DisplayMessege(ValidationMessage.PaymentIDInAdditionalPaymentGrid);
                            BankAdditionalPaymentGridView.Focus();
                            isValidated = false;
                        }
                        else if (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["FlagsAdditionalBank"].Value)) &&
                                Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["FlagsAdditionalBank"].Value)) != (int)Constants.FlagValues.PrincipalOnly &&
                                Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["FlagsAdditionalBank"].Value)) != (int)Constants.FlagValues.AdditionalPayment &&
                                Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["FlagsAdditionalBank"].Value)) != (int)Constants.FlagValues.Discount &&
                                Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["FlagsAdditionalBank"].Value)) != (int)Constants.FlagValues.MaintenanceFee &&
                                Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["FlagsAdditionalBank"].Value)) != (int)Constants.FlagValues.ManagementFee &&
                                Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["FlagsAdditionalBank"].Value)) != (int)Constants.FlagValues.NSFFee &&
                                Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["FlagsAdditionalBank"].Value)) != (int)Constants.FlagValues.LateFee)
                        {
                            DisplayMessege(ValidationMessage.FlagInAdditionalPaymentGrid);
                            BankAdditionalPaymentGridView.Focus();
                            isValidated = false;
                        }
                        else if (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["InterestRateAdditional"].Value)) &&
                                        !regAmountThreeDecimal.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["InterestRateAdditional"].Value)))
                        {
                            DisplayMessege(ValidationMessage.InterestRateInAdditionalPaymentGrid);
                            BankAdditionalPaymentGridView.Focus();
                            isValidated = false;
                        }
                        else if (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["PrincipalAdditional"].Value)) &&
                                            !regAmount.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["PrincipalAdditional"].Value)))
                        {
                            DisplayMessege(ValidationMessage.PrincipalDiscountValue);
                            BankAdditionalPaymentGridView.Focus();
                            isValidated = false;
                        }
                        else if (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["InterestAdditional"].Value)) &&
                                            !regAmount.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["InterestAdditional"].Value)))
                        {
                            DisplayMessege(ValidationMessage.InterestDiscountValue);
                            BankAdditionalPaymentGridView.Focus();
                            isValidated = false;
                        }
                        else if (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["OriginationFeeAdditional"].Value)) &&
                                            !regAmount.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["OriginationFeeAdditional"].Value)))
                        {
                            DisplayMessege(ValidationMessage.OriginationFeeDiscountValue);
                            BankAdditionalPaymentGridView.Focus();
                            isValidated = false;
                        }
                        else if (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["SameDayFeeAdditional"].Value)) &&
                                            !regAmount.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["SameDayFeeAdditional"].Value)))
                        {
                            DisplayMessege(ValidationMessage.SameDayFeeDiscountValue);
                            BankAdditionalPaymentGridView.Focus();
                            isValidated = false;
                        }
                        else if (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ServiceFeeAdditional"].Value)) &&
                                            !regAmount.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ServiceFeeAdditional"].Value)))
                        {
                            DisplayMessege(ValidationMessage.ServiceFeeDiscountValue);
                            BankAdditionalPaymentGridView.Focus();
                            isValidated = false;
                        }
                        else if (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ServiceFeeInterestAdditional"].Value)) &&
                                            !regAmount.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ServiceFeeInterestAdditional"].Value)))
                        {
                            DisplayMessege(ValidationMessage.ServiceFeeInterestDiscountValue);
                            BankAdditionalPaymentGridView.Focus();
                            isValidated = false;
                        }
                        else if (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ManagementFeeAdditional"].Value)) &&
                                            !regAmount.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ManagementFeeAdditional"].Value)))
                        {
                            DisplayMessege(ValidationMessage.ManagementFeeDiscountValue);
                            BankAdditionalPaymentGridView.Focus();
                            isValidated = false;
                        }
                        else if (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["MaintenanceFeeAdditional"].Value)) &&
                                            !regAmount.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["MaintenanceFeeAdditional"].Value)))
                        {
                            DisplayMessege(ValidationMessage.MaintenanceFeeDiscountValue);
                            BankAdditionalPaymentGridView.Focus();
                            isValidated = false;
                        }
                        else if (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["NSFFeeAdditional"].Value)) &&
                                            !regAmount.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["NSFFeeAdditional"].Value)))
                        {
                            DisplayMessege(ValidationMessage.NSFFeeDiscountValue);
                            BankAdditionalPaymentGridView.Focus();
                            isValidated = false;
                        }
                        else if (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["LateFeeAdditional"].Value)) &&
                                            !regAmount.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["LateFeeAdditional"].Value)))
                        {
                            DisplayMessege(ValidationMessage.LateFeeDiscountValue);
                            BankAdditionalPaymentGridView.Focus();
                            isValidated = false;
                        }
                        else if (Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["FlagsAdditionalBank"].Value)) == (int)Constants.FlagValues.Discount &&
                                    ((string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["PrincipalAdditional"].Value)) &&
                                    string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["InterestAdditional"].Value)) &&
                                    string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["OriginationFeeAdditional"].Value)) &&
                                    string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["SameDayFeeAdditional"].Value)) &&
                                    string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ServiceFeeAdditional"].Value)) &&
                                    string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ServiceFeeInterestAdditional"].Value)) &&
                                    string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ManagementFeeAdditional"].Value)) &&
                                    string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["MaintenanceFeeAdditional"].Value)) &&
                                    string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["NSFFeeAdditional"].Value)) &&
                                    string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["LateFeeAdditional"].Value))) ||

                                    ((!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["PrincipalAdditional"].Value)) ? Convert.ToDouble(BankAdditionalPaymentGridView.Rows[i].Cells["PrincipalAdditional"].Value) : 0) +
                                    (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["InterestAdditional"].Value)) ? Convert.ToDouble(BankAdditionalPaymentGridView.Rows[i].Cells["InterestAdditional"].Value) : 0) +
                                    (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["OriginationFeeAdditional"].Value)) ? Convert.ToDouble(BankAdditionalPaymentGridView.Rows[i].Cells["OriginationFeeAdditional"].Value) : 0) +
                                    (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["SameDayFeeAdditional"].Value)) ? Convert.ToDouble(BankAdditionalPaymentGridView.Rows[i].Cells["SameDayFeeAdditional"].Value) : 0) +
                                    (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ServiceFeeAdditional"].Value)) ? Convert.ToDouble(BankAdditionalPaymentGridView.Rows[i].Cells["ServiceFeeAdditional"].Value) : 0) +
                                    (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ServiceFeeInterestAdditional"].Value)) ? Convert.ToDouble(BankAdditionalPaymentGridView.Rows[i].Cells["ServiceFeeInterestAdditional"].Value) : 0) +
                                    (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ManagementFeeAdditional"].Value)) ? Convert.ToDouble(BankAdditionalPaymentGridView.Rows[i].Cells["ManagementFeeAdditional"].Value) : 0) +
                                    (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["MaintenanceFeeAdditional"].Value)) ? Convert.ToDouble(BankAdditionalPaymentGridView.Rows[i].Cells["MaintenanceFeeAdditional"].Value) : 0) +
                                    (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["NSFFeeAdditional"].Value)) ? Convert.ToDouble(BankAdditionalPaymentGridView.Rows[i].Cells["NSFFeeAdditional"].Value) : 0) +
                                    (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["LateFeeAdditional"].Value)) ? Convert.ToDouble(BankAdditionalPaymentGridView.Rows[i].Cells["LateFeeAdditional"].Value) : 0)
                                    <= 0))
                                    )
                        {
                            DisplayMessege(ValidationMessage.AdditionalPaymentAmountForDiscount);
                            BankAdditionalPaymentGridView.Focus();
                            isValidated = false;
                        }
                        else if (!string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["InterestPriorityAdditional"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["PrincipalPriorityAdditional"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["OriginationFeePriorityAdditional"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["SameDayFeePriorityAdditional"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ServiceFeePriorityAdditional"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ServiceFeeInterestPriorityAdditional"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ManagementFeePriorityAdditional"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["MaintenanceFeePriorityAdditional"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["NSFFeePriorityAdditional"].Value)) ||
                                    !string.IsNullOrEmpty(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["LateFeePriorityAdditional"].Value)))
                        {
                            if (!regIntAmount.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["InterestPriorityAdditional"].Value)) || Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["InterestPriorityAdditional"].Value)) > 10 ||
                                    !regIntAmount.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["PrincipalPriorityAdditional"].Value)) || Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["PrincipalPriorityAdditional"].Value)) > 10 ||
                                    !regIntAmount.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["OriginationFeePriorityAdditional"].Value)) || Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["OriginationFeePriorityAdditional"].Value)) > 10 ||
                                    !regIntAmount.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["SameDayFeePriorityAdditional"].Value)) || Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["SameDayFeePriorityAdditional"].Value)) > 10 ||
                                    !regIntAmount.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ServiceFeePriorityAdditional"].Value)) || Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ServiceFeePriorityAdditional"].Value)) > 10 ||
                                    !regIntAmount.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ServiceFeeInterestPriorityAdditional"].Value)) || Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ServiceFeeInterestPriorityAdditional"].Value)) > 10 ||
                                    !regIntAmount.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ManagementFeePriorityAdditional"].Value)) || Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ManagementFeePriorityAdditional"].Value)) > 10 ||
                                    !regIntAmount.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["MaintenanceFeePriorityAdditional"].Value)) || Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["MaintenanceFeePriorityAdditional"].Value)) > 10 ||
                                    !regIntAmount.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["NSFFeePriorityAdditional"].Value)) || Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["NSFFeePriorityAdditional"].Value)) > 10 ||
                                    !regIntAmount.IsMatch(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["LateFeePriorityAdditional"].Value)) || Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["LateFeePriorityAdditional"].Value)) > 10)
                            {
                                DisplayMessege(ValidationMessage.PriorityValues);
                                BankAdditionalPaymentGridView.Focus();
                                isValidated = false;
                            }
                            else
                            {
                                int[] a = new int[10];
                                a[Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["InterestPriorityAdditional"].Value)) - 1]++;
                                a[Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["PrincipalPriorityAdditional"].Value)) - 1]++;
                                a[Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["OriginationFeePriorityAdditional"].Value)) - 1]++;
                                a[Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["SameDayFeePriorityAdditional"].Value)) - 1]++;
                                a[Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ServiceFeePriorityAdditional"].Value)) - 1]++;
                                a[Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ServiceFeeInterestPriorityAdditional"].Value)) - 1]++;
                                a[Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["ManagementFeePriorityAdditional"].Value)) - 1]++;
                                a[Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["MaintenanceFeePriorityAdditional"].Value)) - 1]++;
                                a[Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["NSFFeePriorityAdditional"].Value)) - 1]++;
                                a[Convert.ToInt32(Convert.ToString(BankAdditionalPaymentGridView.Rows[i].Cells["LateFeePriorityAdditional"].Value)) - 1]++;
                                for (int j = 0; j < 10; j++)
                                {
                                    if (a[j] > 1)
                                    {
                                        DisplayMessege(ValidationMessage.DifferentPriorityValues);
                                        BankAdditionalPaymentGridView.Focus();
                                        isValidated = false;
                                        break;
                                    }
                                }
                            }
                        }

                        if (!isValidated)
                        {
                            return isValidated;
                        }
                        // All, Schedule date, Additional payment date and Early Payoff date cannot be same.
                        DateTime earlypayoff = Convert.ToDateTime(EarlyPayoffDateTbx.Text);
                        if (earlypayoff == Convert.ToDateTime(BankAdditionalPaymentGridView.Rows[i].Cells["DateInAdditional"].Value))
                        {
                            for (int index = 0; index < InputGridView.Rows.Count; index++)
                            {
                                if ((Convert.ToDateTime(InputGridView.Rows[index].Cells["DateIn"].Value) == earlypayoff) ||
                                    !string.IsNullOrEmpty(Convert.ToString(InputGridView.Rows[index].Cells["EffectiveDate"].Value))
                                    && (Convert.ToDateTime(InputGridView.Rows[index].Cells["EffectiveDate"].Value) == earlypayoff))
                                {
                                    DisplayMessege(ValidationMessage.AdditionalPaymentWithEarlyPayOff);
                                    BankAdditionalPaymentGridView.Focus();
                                    isValidated = false;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
                if (!isValidated)
                {
                    return isValidated;
                }

                #endregion


                if (!isValidated)
                {
                    return isValidated;
                }

                //Validate Accrued Interest Date TextBox input field
                isValidated = ValidateDate(AccruedInterestDateTbx.Text, ValidationMessage.AccruedInterestDateFormat, true);
                if (!isValidated)
                {
                    AccruedInterestDateTbx.Focus();
                    return isValidated;
                }

                //Validate Accrued Interest Date TextBox input field
                isValidated = ValidateDate(AccruedServiceFeeInterestDateTbx.Text, ValidationMessage.ServiceFeeInterestAccruedDateFormat, true);
                if (!isValidated)
                {
                    AccruedServiceFeeInterestDateTbx.Focus();
                }

                sbTracing.AppendLine("Exit:From LoanAmortForm Class file and method Name ValidateInputValues()");
                return isValidated;
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
                return false;
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }
        private static bool ValidateDate(string date, string invalidDateMessage, bool allowEmpty)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                sbTracing.AppendLine("Enter: Inside LoanAmortForm Class file and method Name validateDate(string date, string invalidDateMessage, bool allowEmpty).");
                sbTracing.AppendLine("Parameters are: " + ", date = " + date + ", invalidDateMessage = " + invalidDateMessage + ", allowEmpty = " + allowEmpty);

                if (string.IsNullOrEmpty(date) && !allowEmpty)
                {
                    DisplayMessege(invalidDateMessage);
                    return false;
                }

                DateTime dates = Convert.ToDateTime(date);
                string[] a = (dates.ToShortDateString()).Split('/');
                bool invalidDate = false;
                if ((Convert.ToInt32(a[0]) == 1 || Convert.ToInt32(a[0]) == 3 || Convert.ToInt32(a[0]) == 5 || Convert.ToInt32(a[0]) == 7 ||
                        Convert.ToInt32(a[0]) == 8 || Convert.ToInt32(a[0]) == 10 || Convert.ToInt32(a[0]) == 12) && (Convert.ToInt32(a[1]) > 31))
                {
                    invalidDate = true;
                }
                else if ((Convert.ToInt32(a[0]) == 4 || Convert.ToInt32(a[0]) == 6 || Convert.ToInt32(a[0]) == 9 || Convert.ToInt32(a[0]) == 11) &&
                                (Convert.ToInt32(a[1]) > 30))
                {
                    invalidDate = true;
                }
                else if ((((Convert.ToInt32(a[2]) % 4 == 0) && (Convert.ToInt32(a[1]) > 29)) || ((Convert.ToInt32(a[2]) % 4 != 0) && (Convert.ToInt32(a[1]) > 28)))
                    && Convert.ToInt32(a[0]) == 2)
                {
                    invalidDate = true;
                }

                if (invalidDate)
                {
                    DisplayMessege(invalidDateMessage);
                    return false;
                }
                sbTracing.AppendLine("Exit:From LoanAmortForm Class file and method Name validateDate(string date, string invalidDateMessage, bool allowEmpty)");
                return true;
            }
            catch (Exception ex)
            {
                LogManager.LogManager.WriteErrorLog(ex);
                DisplayMessege(invalidDateMessage);
                return false;
            }
            finally
            {
                LogManager.LogManager.WriteTraceLog(sbTracing.ToString());
            }
        }

        #endregion

        private static void DisplayMessege(string message)
        {
            MessageBox.Show(message, "Loan Amort Driver", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void InputGridView_KeyPress(object sender, KeyPressEventArgs e)
        {
            StringBuilder sbTracing = new StringBuilder();
            try
            {
                if (LoanTypeCmb.SelectedIndex == 0)
                {
                    for (int startIndex = 1; startIndex < InputGridView.Rows.Count; startIndex++)
                    {
                        InputGridView.Rows[startIndex].Cells["PaymentAmountSchedule"].ReadOnly = false;
                        if (startIndex == InputGridView.Rows.Count - 2)
                        {
                            InputGridView.Rows[startIndex].Cells["PaymentAmountSchedule"].ReadOnly = true;
                        }
                    }
                }
                InputGridView.Rows[0].Cells["EffectiveDate"].ReadOnly = true;
                InputGridView.Rows[0].Cells["PaymentAmountSchedule"].ReadOnly = true;
                InputGridView.Rows[0].Cells["InterestRate"].ReadOnly = true;
                InputGridView.Rows[0].Cells["InterestPriority"].ReadOnly = true;
                InputGridView.Rows[0].Cells["PrincipalPriority"].ReadOnly = true;
                InputGridView.Rows[0].Cells["OriginationFeePriority"].ReadOnly = true;
                InputGridView.Rows[0].Cells["SameDayFeePriority"].ReadOnly = true;
                InputGridView.Rows[0].Cells["ServiceFeePriority"].ReadOnly = true;
                InputGridView.Rows[0].Cells["ServiceFeeInterestPriority"].ReadOnly = true;
                InputGridView.Rows[0].Cells["ManagementFeePriority"].ReadOnly = true;
                InputGridView.Rows[0].Cells["MaintenanceFeePriority"].ReadOnly = true;
                InputGridView.Rows[0].Cells["NSFFeePriority"].ReadOnly = true;
                InputGridView.Rows[0].Cells["LateFeePriority"].ReadOnly = true;
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

        private void DaysInMonthCmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DaysInMonthCmb.SelectedIndex == 2)
            {
                InterestDelayTbx.Enabled = false;
                InterestDelayTbx.Text = "";
            }
            else
            {
                InterestDelayTbx.Enabled = true;
            }
        }


    }
}