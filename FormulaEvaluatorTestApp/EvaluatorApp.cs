using FormulaEvaluator;
using System.Text.RegularExpressions;

namespace FormulaEvaluatorTestApp
{
    /// <summary>
    /// a test app for Evaluator library
    /// </summary>
    internal class EvaluatorApp
    {
        /// <summary>
        /// function to look up the variables in the sheet
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static int valueVal(String s) // Lookup function
        {
            if (s.Equals("BWQEA111"))
            {

                return 1;
            }
            if (s.Equals("A1"))
            {
                return 6;
            }
            if (s.Equals("aAA1"))
            {
                return 9;
            }
            else
            {
                throw new ArgumentException("The variables don't exist");
            }
        }

        public static void Main(string[] agrs)
        {
            string s = "((6 / (4 - (15 / 5)))*5+3)/(11)";
            Console.WriteLine(s + " = " + Evaluator.Evaluate(s, valueVal)); // Call Evaluator
            s = "5+2+3";
            Console.WriteLine(s + " = " + Evaluator.Evaluate(s, valueVal)); // Call Evaluator
            s = "((aAA1 / (4 - (15 / 5)))*5+3)/(11)";
            Console.WriteLine(s + " = " + Evaluator.Evaluate(s, valueVal)); // Call Evaluator
            s = "((5+5)*5+4/2)/(2)";
            Console.WriteLine(s + " = " + Evaluator.Evaluate(s, valueVal)); // Call Evaluator
            s = "A1 /(3+3)";
            Console.WriteLine(s + " = " + Evaluator.Evaluate(s, valueVal)); // Call Evaluator
        }


    }
}