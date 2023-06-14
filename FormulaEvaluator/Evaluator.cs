using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;



namespace FormulaEvaluator
{
    /// <summary>
    /// Evaluator to evaluate arithmetic expressions
    /// </summary>
    public static class Evaluator
    {
        /// <summary>
        /// a delegate to look up the variables
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public delegate int Lookup(String v);

        /// <summary>
        /// Get a Value Stack and Operation Stack to perfrom the Operations (+,-,*,/)
        /// </summary>
        /// <param name="opStack"></param>
        /// <param name="firstVal"></param>
        /// <param name="secondVal"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static int performOps(Stack<int> valueStack, Stack<string> opStack) //perform the operations
        {
            int firstVal = valueStack.Pop();
            int secondVal = valueStack.Pop();
            string operation = opStack.Pop();
            switch (operation)
            {
                case "+":
                    return firstVal + secondVal;
                case "-":
                    return secondVal - firstVal;
                case "*":
                    return firstVal * secondVal;
                case "/":
                    if (firstVal == 0)
                    {
                        throw new ArgumentException("Cannot divide by zero");
                    }
                    return secondVal / firstVal;
            }
            return 0;
        }

        /// <summary>
        /// A helper method to check the valid variables of the expressions
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static bool isValidVar(string s)
        {
            return Regex.IsMatch(s, "^[A-Za-z]+[0-9]+$");
        }

        /// <summary>
        /// A helper method to check if the Value Stack CONTAINS at least 2 numbers, 
        /// the Operator Stack CONTAINS at least 1 operator, 
        /// and the two operators passed as arguments MATCH the operator at the top of Operator Stack
        /// </summary>
        /// <param name="valueStack"></param>
        /// <param name="opStack"></param>
        /// <param name="exp1"></param>
        /// <param name="exp2"></param>
        /// <returns></returns>
        private static bool isOnTop(Stack<int> valueStack, Stack<string> opStack, string exp1, string exp2)
        {
            if(valueStack.Count > 1 && opStack.Count > 0 && (opStack.Peek().Equals(exp1) || opStack.Peek().Equals(exp2)))
            {
                    return true;   
            }

            return false;
        }

        /// <summary>
        /// Method that get arithmetic expressions and the Lookup function to evaluate and return the value of that arithmetic expressions
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="variableEvaluator"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static int Evaluate(String exp, Lookup variableEvaluator)
        {
            Stack<int> valueStack = new Stack<int>();
            Stack<string> opStack = new Stack<string>();

            if (!string.IsNullOrEmpty(exp))
            {
                string[] tokens = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
                foreach (string t in tokens)
                {
                    string s = t.Trim();
                    if (string.IsNullOrEmpty(s) || s.Equals(" ")) // empty or white space -> ignore it
                    {
                        continue;
                    }

                    if (Int32.TryParse(s, out int firstNumber)) // t is an integer
                    {
                        //
                        valueStack.Push(firstNumber);
                        if (isOnTop(valueStack,opStack,"*","/")) //if the top of Operator Stack is "*" or "/"
                        {                          
                            valueStack.Push(performOps(valueStack,opStack));
                        }

                    }
                    else if (isValidVar(s)) // t is a variable
                    {

                        int firstVal = variableEvaluator(s);
                        valueStack.Push(firstVal);
                        if (isOnTop(valueStack, opStack, "*", "/"))  // if the top of Operator Stack is "*" or "/"
                        {
                            // perform the "*" or "/" operation
                            valueStack.Push(performOps(valueStack, opStack));
                        }

                    }
                    else if (s == "+" || s == "-") // t is "+" or "-"
                    {
                        if (isOnTop(valueStack, opStack, "+", "-")) // if the top of Operator Stack is "+" or "-"
                        {
                            // perform the "+" or "-" operation
                            valueStack.Push(performOps(valueStack, opStack));
                        }
                        opStack.Push(s);
                    }
                    else if (s == "(" || s == "*" || s == "/") // t is "(" or "*" or "/"
                    {
                        opStack.Push(s);
                    }
                    else if (s == ")") // t is ")"
                    {

                        if (isOnTop(valueStack, opStack, "+", "-")) // if the top of Operator Stack is "+" or "-"
                        {
                            // perform the "+" or "-" operation
                            valueStack.Push(performOps(valueStack, opStack));
                        }

                        // left parathesis checking and pop it out
                        if (opStack.Count > 0 && opStack.Peek() == "(")
                        {
                            opStack.Pop();
                        }
                        else
                        {
                            throw new ArgumentException("the expressions are invalid");
                        }


                        if (isOnTop(valueStack, opStack, "*", "/")) // if the top of Operator Stack is "*" or "/"
                        {
                            // perform the "*" or "/" operation
                            valueStack.Push(performOps(valueStack, opStack));
                        }
                    }
                    else // errors with the tokens
                    {
                        throw new ArgumentException("the expressions are invalid");
                    }
                }

                // last step to the result
                if (opStack.Count == 0 && valueStack.Count == 1)
                {                    
                    return valueStack.Pop(); // pop the result and return it
                }
                else if (opStack.Count == 1 && valueStack.Count == 2)
                {
                    // perform the last operator (should be "+" or "-")
                    return performOps(valueStack, opStack);
                }
                else // errors with the tokens
                {
                    throw new ArgumentException("the expressions are invalid");
                }
            }
            else
            {
                throw new ArgumentException("the expressions are invalid");
            }

            return -1;
        }

    }
}