using System;

namespace Common.Logic
{
    public class Calculator
    {
        public static double RunLongComputation(double number)
        {
            double result = 0;

            int UPPER = Convert.ToInt32(number);

            for (int i = 0; i < UPPER; i++)
            {
                for (int j = 0; j < UPPER; j++)
                {
                    for (int k = 0; k < UPPER; k++)
                    {
                        result += Math.Exp(result);                        
                    }
                }
            }

            return result;
        }
    }
}
