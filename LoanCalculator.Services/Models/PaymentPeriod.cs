using System;

namespace LoanCalculator.Services.Models
{
    /// <summary>
    /// value must be periods per year.
    /// New members for this enum must be handled in the getDaysPerPeriod method.
    /// </summary>
    public enum PaymentPeriod : short 
    {   
        Monthly = 12,
        SemiMonthly = 24,
        BiWeekly = 26,
        Weekly = 52,
        Daily = 360
    };
}
