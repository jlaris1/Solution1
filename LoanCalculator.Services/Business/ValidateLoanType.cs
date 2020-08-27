using LoanAmort_driver.Model;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoanAmort_driver.Business
{
    public class ValidateLoanType
    {
        public static void isValidate(GetScheduleInput Input)
        {
            Input.AccruedInterestDate = Input.AccruedInterestDate.Date;
            if (Input.LoanAmount <= 0)
            {
                throw new System.ArgumentException("LoanAmount must be positive.");
            }
            if (Input.InterestRate <= 0 && Input.PmtPeriod != PaymentPeriod.Daily)
            {
                throw new System.ArgumentException("InterestRate must be positive.");
            }
            if (Input.PmtPeriod == PaymentPeriod.Daily && Input.InterestRate != 0)
            {
                throw new ArgumentException("Only 0 value interest rate is allowed for Daily period");
            }
            if (Input.DaysInYear == default(short) || Input.DaysInYear < 0)
            {
                throw new ArgumentException("Number of days in a year were not provided or value is invalid");
            }
            Input.EarlyPayoffDate = Input.EarlyPayoffDate.Date;
            Input.InputRecords.Sort();
            for (int i = 0; i < Input.InputRecords.Count; i++)
            {
                Input.InputRecords[i].DateIn = Input.InputRecords[i].DateIn.Date;
                if (i > 0)
                {
                    if (Input.InputRecords[i].DateIn == Input.InputRecords[i - 1].DateIn)
                    {
                        throw new System.ArgumentException("Repeated date value in input list.");
                    }
                }
            }
            /*if (DateAndTime.DateDiff(DateInterval.Day, Input.AccruedInterestDate, Input.InputRecords[0].DateIn, FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1) > 0 ||
                DateAndTime.DateDiff(DateInterval.Day, Input.AccruedInterestDate, Input.InputRecords[Input.InputRecords.Count - 1].DateIn, FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1) < 0)
            {
                throw new System.ArgumentException("AccruedInterestDate must be on or after the effective date and on or before the last payment date.");
            }*/


            // AD : if early payoff date is earlier than effective date it should be ignored
            DateTime EffectiveDate = Input.InputRecords[0].DateIn;
            bool IgnoreEarlyPayoffDate = Input.EarlyPayoffDate <= EffectiveDate;

            if (Input.InputPaymentRecords != null)
            {
                Input.InputPaymentRecords.Sort();
                for (int i = 0; i < Input.InputPaymentRecords.Count; i++)
                {
                    Input.InputPaymentRecords[i].DateIn = Input.InputPaymentRecords[i].DateIn.Date;
                    if (Input.InputPaymentRecords[i].PaymentAmount <= 0)
                    {
                        throw new System.ArgumentException("AdditionalPayment must be positive.");
                    }

                    // AD : if input records for payments do not exist or Early Payoff is before last payment date in input
                    // consider Early Payoff date as last payment date
                    // if Early Payoff Date is ignored, then do not use it

                    DateTime lastPaymentDate = Input.InputRecords[Input.InputRecords.Count - 1].DateIn;
                    if (!IgnoreEarlyPayoffDate &&
                        (Input.InputRecords.Count == 1 ||
                         DateAndTime.DateDiff(DateInterval.Day, lastPaymentDate, Input.EarlyPayoffDate,
                             FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1) < 1))
                    {
                        lastPaymentDate = Input.EarlyPayoffDate;
                    }


                    if (
                        DateAndTime.DateDiff(DateInterval.Day, Input.InputPaymentRecords[i].DateIn,
                            Input.InputRecords[0].DateIn, FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1) > -1 ||
                        DateAndTime.DateDiff(DateInterval.Day, Input.InputPaymentRecords[i].DateIn, lastPaymentDate,
                            FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1) < 1)
                    {
                        throw new System.ArgumentException(
                            "Additional payment dates must be after the effective date and before the last payment date.");
                    }
                }
            }

        }

    }
}
