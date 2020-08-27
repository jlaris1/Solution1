using System;

namespace LoanCalculator.Services.Models
{
    class Round
    {
        private Round()
        {

        }
        /// <summary>
        /// This function rounds the amount to two decimal place.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static double RoundOffAmount(double amount)
        {
            return Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        }
    }
}
