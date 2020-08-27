using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoanAmort_driver.Models;
using System.Data;

namespace LoanAmort_driver.Business
{

    //This class is just for test purpose, as we  are using this class to verify the output values for different inputs.
    class TestScheduleForBankMethod
    {
        public static void GetSchedulevalues()
        {
            List<InputGrid> grd = new List<InputGrid>();
            grd.Add(new InputGrid { DateIn = Convert.ToDateTime("01/01/2017"), Flags = 0, PaymentID = 0 });
            grd.Add(new InputGrid { DateIn = Convert.ToDateTime("01/15/2017"), Flags = 0, PaymentID = 2 });
            grd.Add(new InputGrid { DateIn = Convert.ToDateTime("01/08/2017"), Flags = 0, PaymentID = 1 });
            grd.Add(new InputGrid { DateIn = Convert.ToDateTime("01/22/2017"), Flags = 0, PaymentID = 3 });
            grd.Add(new InputGrid { DateIn = Convert.ToDateTime("01/29/2017"), Flags = 0, PaymentID = 4 });
            grd.Add(new InputGrid { DateIn = Convert.ToDateTime("02/05/2017"), Flags = 0, PaymentID = 5 });
            grd.Add(new InputGrid { DateIn = Convert.ToDateTime("02/12/2017"), Flags = 0, PaymentID = 6 });
            grd.Add(new InputGrid { DateIn = Convert.ToDateTime("02/19/2017"), Flags = 0, PaymentID = 7 });
            grd.Add(new InputGrid { DateIn = Convert.ToDateTime("02/26/2017"), Flags = 0, PaymentID = 8 });
            grd.Add(new InputGrid { DateIn = Convert.ToDateTime("03/05/2017"), Flags = 0, PaymentID = 9 });
            grd.Add(new InputGrid { DateIn = Convert.ToDateTime("03/12/2017"), Flags = 0, PaymentID = 10 });
            grd.Add(new InputGrid { DateIn = Convert.ToDateTime("03/19/2017"), Flags = 0, PaymentID = 11 });
            grd.Add(new InputGrid { DateIn = Convert.ToDateTime("03/26/2017"), Flags = 0, PaymentID = 12 });


            LoanDetails inp = new LoanDetails()
            {
                InputRecords = grd,
                LoanAmount = 1000,
                //InterestRate = 250,
                PmtPeriod = Models.PaymentPeriod.Weekly,
                EarlyPayoffDate = Convert.ToDateTime("01/01/1900"),
                AccruedInterestDate = Convert.ToDateTime("01/01/1900"),
                ServiceFee = 10,
                ManagementFee = 20,
                MaintenanceFee = 30,
                RecastAdditionalPayments = false,
                UseFlexibleCalculation = false,
                DaysInYearBankMethod = "364",
                LoanType = "Bank Method",
                DaysInMonth = "30",
                SameDayFee = 40,
                SameDayFeeCalculationMethod = "Fixed",
                IsSameDayFeeFinanced = true,
                OriginationFee = 50,
                OriginationFeeCalculationMethod = "Fixed",
                IsOriginationFeeFinanced = true,
                AmountFinanced = 1000
            };

            OutputSchedule schedule = new OutputSchedule();
            schedule = CreateScheduleForBankMethod.DefaultSchedule(inp);

            DataTable dt = new DataTable();
            dt.Columns.Add("PaymentDate", typeof(DateTime));
            dt.Columns.Add("BeginningPrincipal", typeof(double));
            dt.Columns.Add("PeriodicInterestRate", typeof(double));
            dt.Columns.Add("DailyInterestRate", typeof(double));
            dt.Columns.Add("DailyInterestAmount", typeof(double));
            dt.Columns.Add("PaymentID", typeof(double));
            dt.Columns.Add("Flags", typeof(double));
            dt.Columns.Add("DueDate", typeof(DateTime));
            dt.Columns.Add("InterestPayment", typeof(double));
            dt.Columns.Add("PrincipalPayment", typeof(double));
            dt.Columns.Add("TotalPayment", typeof(double));
            dt.Columns.Add("ServiceFee", typeof(double));
            dt.Columns.Add("ServiceFeeInterest", typeof(double));
            dt.Columns.Add("ServiceFeeTotal", typeof(double));
            dt.Columns.Add("OriginationFee", typeof(double));
            dt.Columns.Add("MaintenanceFee", typeof(double));
            dt.Columns.Add("ManagementFee", typeof(double));
            dt.Columns.Add("SameDayFee", typeof(double));
            dt.Columns.Add("NSFFee", typeof(double));
            dt.Columns.Add("LateFee", typeof(double));

            foreach (var a in schedule.Schedule)
                dt.Rows.Add(a.PaymentDate, a.BeginningPrincipal, a.PeriodicInterestRate, a.DailyInterestRate, a.DailyInterestAmount, a.PaymentID, a.Flags, a.DueDate,
                                    a.InterestPayment, a.PrincipalPayment, a.TotalPayment, a.ServiceFee, a.ServiceFeeInterest, a.ServiceFeeTotal, a.OriginationFee,
                                    a.MaintenanceFee, a.ManagementFee, a.SameDayFee, a.NSFFee, a.LateFee);

        }
    }
}
