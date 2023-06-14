// Skeleton written by Profs Zachary, Kopta and Martin for CS 3500
// Read the entire skeleton carefully and completely before you
// do anything else!

// Change log:
// Last updated: 9/8, updated for non-nullable types

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  The allowed symbols are non-negative numbers written using double-precision 
    /// floating-point syntax (without unary preceeding '-' or '+'); 
    /// variables that consist of a letter or underscore followed by 
    /// zero or more letters, underscores, or digits; parentheses; and the four operator 
    /// symbols +, -, *, and /.  
    /// 
    /// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
    /// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable; 
    /// and "x 23" consists of a variable "x" and a number "23".
    /// 
    /// Associated with every formula are two delegates:  a normalizer and a validator.  The
    /// normalizer is used to convert variables into a canonical form, and the validator is used
    /// to add extra restrictions on the validity of a variable (beyond the standard requirement 
    /// that it consist of a letter or underscore followed by zero or more letters, underscores,
    /// or digits.)  Their use is described in detail in the constructor and method comments.
    /// </summary>
    public class Formula
    {
        private Func<string, string> normalize;
        private Func<string, bool> isValid;
        private IEnumerable<string> tokens;

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically invalid,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer is the identity function, and the associated validator
        /// maps every string to true.  
        /// </summary>
        public Formula(String formula) :
            this(formula, s => s, s => true)
        {
        }

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically incorrect,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer and validator are the second and third parameters,
        /// respectively.  
        /// 
        /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
        /// throws a FormulaFormatException with an explanatory message. 
        /// 
        /// If the formula contains a variable v such that isValid(normalize(v)) is false,
        /// throws a FormulaFormatException with an explanatory message.
        /// 
        /// Suppose that N is a method that converts all the letters in a string to upper case, and
        /// that V is a method that returns true only if a string consists of one letter followed
        /// by one digit.  Then:
        /// 
        /// new Formula("x2+y3", N, V) should succeed
        /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
        /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
        /// </summary>
        public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
        {
            this.normalize = normalize;
            this.isValid = isValid;

            tokens = GetTokens(formula);

            //One Token Rule: There must be at least one token.
            if (tokens.Count() < 1)
            {
                throw new FormulaFormatException("One Token Rule: There must be at least one token.");
            }

            //Starting Token Rule: The first token of an expression must be a number, a variable, or an opening parenthesis.
            string firstToken = tokens.First();
            if (!Double.TryParse(firstToken, out double d1) && firstToken != "(" && !isVariable(firstToken))
                throw new FormulaFormatException("Starting Token Rule: The first token of an expression must be a number, a variable, or an opening parenthesis");

            //Ending Token Rule: The last token of an expression must be a number, a variable, or a closing parenthesis.
            string lastToken = tokens.Last();
            if (!Double.TryParse(lastToken, out double d2) && lastToken != ")" && !isVariable(lastToken))
                throw new FormulaFormatException("Ending Token Rule: The last token of an expression must be a number, a variable, or a closing parenthesis");

            int leftParenCount = 0;
            int rightParenCount = 0;
            bool previousIsOpParen = false;
            bool previousIsNumVarCloseParen = false;

            foreach (string s in tokens)
            {
                string str = s.Trim();

                //Parsing Rule: need to be (, ), +, -, *, /, variables, and decimal real numbers
                //Also check for Valid Variable
                if (!isValidTokens(str))
                {
                    throw new FormulaFormatException("Parsing Rule: We have provided a private method that will convert an input string into tokens. " +
                        "After tokenizing, your code should verify that the only tokens are (, ), +, -, *, /, variables, and decimal real numbers (including scientific notation).");
                }


                //Parenthesis/Operator Following Rule: Any token that immediately follows an opening parenthesis or an operator
                //must be either a number, a variable, or an opening parenthesis.
                if (previousIsOpParen)
                {
                    if (!Double.TryParse(str, out double d3) && str != "(" && !isVariable(str))
                        throw new FormulaFormatException("Parenthesis/Operator Following Rule: Any token that immediately follows an opening parenthesis or an operator " +
                            "must be either a number, a variable, or an opening parenthesis.");
                }
                if (isOpOrOpenParen(str))
                    previousIsOpParen = true;
                else previousIsOpParen = false;

                //Extra Following Rule: Any token that immediately follows a number, a variable, or a closing parenthesis
                //must be either an operator or a closing parenthesis.
                if (previousIsNumVarCloseParen)
                {
                    if (!isOpOrCloseParen(str))
                        throw new FormulaFormatException("Extra Following Rule:  Any token that immediately follows a number, a variable, or a closing parenthesis" +
                            " must be either an operator or a closing parenthesis.");
                }
                if (Double.TryParse(str, out double d4) || str == ")" || isVariable(str))
                    previousIsNumVarCloseParen = true;
                else previousIsNumVarCloseParen = false;

                // Right Parentheses Rule: When reading tokens from left to right,
                // at no point should the number of closing parentheses seen so far be greater than the number of opening parentheses seen so far.
                if (str == "(")
                    leftParenCount++;
                if (str == ")")
                    rightParenCount++;
                if (rightParenCount > leftParenCount)
                    throw new FormulaFormatException("Right Parentheses Rule: When reading tokens from left to right," +
                        " at no point should the number of closing parentheses seen so far be greater than the number of opening parentheses seen so far.");

            }

            //Balanced Parentheses Rule: The total number of opening parentheses must equal the total number of closing parentheses.
            if (rightParenCount != leftParenCount)
                throw new FormulaFormatException("Balanced Parentheses Rule: The total number of opening parentheses must equal the total number of closing parentheses.");

        }

        /// <summary>
        /// Get reference of Value Stack and Operation Stack to TRY to perfrom the Operations (+,-,*,/)
        /// </summary>
        /// <param name="valueStack"> reference type of value stack</param>
        /// <param name="opStack"> reference type of operator stack</param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static bool TryPerformOps(ref Stack<double> valueStack, ref Stack<string> opStack, out double result) //perform the operations
        {
            double firstVal = valueStack.Pop();
            double secondVal = valueStack.Pop();
            string operation = opStack.Pop();
            result = 0f;
            switch (operation)
            {
                case "+":
                    result = firstVal + secondVal;
                    valueStack.Push(result);
                    return true;
                case "-":
                    result = secondVal - firstVal;
                    valueStack.Push(result);
                    return true;
                case "*":
                    result = firstVal * secondVal;
                    valueStack.Push(result);
                    return true;
                case "/":
                    if (firstVal == 0)
                    {
                        result = 0f;
                        return false;
                    }
                    else
                    {
                        result = secondVal / firstVal;
                        valueStack.Push(result);
                        return true;
                    }
            }

            return false;

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
        private static bool isOnTop(Stack<double> valueStack, Stack<string> opStack, string exp1, string exp2)
        {
            if (valueStack.Count > 1 && opStack.Count > 0 && (opStack.Peek().Equals(exp1) || opStack.Peek().Equals(exp2)))
            {
                return true;
            }

            return false;
        }

        //Check for +,-,*,/ or (
        private bool isOpOrOpenParen(string s)
        {
            return Regex.IsMatch(s, @"^[\+\-*/\(]$");

        }

        //Check for +,-,*,/ or )
        private bool isOpOrCloseParen(string s)
        {
            return Regex.IsMatch(s, @"^[\+\-*/\)]$");

        }

        //Check for valid variables
        private bool isVariable(string s)
        {

            String varPattern = @"^[a-zA-Z_](?:[a-zA-Z_]|\d)*$";

            if (Regex.IsMatch(s, varPattern))
            {
                if (isValid(normalize(s)))
                    return true;
            }

            return false;

        }

        //Check for number
        private bool isNumber(string s)
        {
            if (Double.TryParse(s, out double d))
                return true;
            return false;
        }

        //Check for Opertor such as +,-,*,/,(,)
        private bool isOperatorAndParentheses(string s)
        {
            return Regex.IsMatch(s, @"^[\+\-*/\(\)]$");

        }

        //Check for Valid tokens (Number or Operator)
        private bool isValidTokens(string s)
        {
            return isOperatorAndParentheses(s) || isNumber(s) || isVariable(s);
        }

        /// <summary>
        /// Evaluates this Formula, using the lookup delegate to determine the values of
        /// variables.  When a variable symbol v needs to be determined, it should be looked up
        /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
        /// the constructor.)
        /// 
        /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
        /// in a string to upper case:
        /// 
        /// new Formula("x+7", N, s => true).Evaluate(L) is 11
        /// new Formula("x+7").Evaluate(L) is 9
        /// 
        /// Given a variable symbol as its parameter, lookup returns the variable's value 
        /// (if it has one) or throws an ArgumentException (otherwise).
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.  
        /// The Reason property of the FormulaError should have a meaningful explanation.
        ///
        /// This method should never throw an exception.
        /// </summary>
        public object Evaluate(Func<string, double> lookup)
        {
            Stack<double> valueStack = new Stack<double>();
            Stack<string> opStack = new Stack<string>();

            foreach (string t in tokens)
            {
                string s = t.Trim();

                if (Double.TryParse(s, out double firstNumber)) // t is an integer
                {
                    valueStack.Push(firstNumber);
                    if (isOnTop(valueStack, opStack, "*", "/")) //if the top of Operator Stack is "*" or "/"
                    {
                        if (!TryPerformOps(ref valueStack, ref opStack, out double result))
                        {
                            return new FormulaError("Can't Divided by 0");
                        }
                    }

                }
                else if (isVariable(s))// t is a variable
                {
                    try // Try to look up varibles, if it doesn't exist return FormulaError
                    {
                        double firstVal = lookup(normalize(s));
                        valueStack.Push(firstVal);
                        if (isOnTop(valueStack, opStack, "*", "/"))  // if the top of Operator Stack is "*" or "/"
                        {
                            // perform the "*" or "/" operation
                            if (!TryPerformOps(ref valueStack, ref opStack, out double result))
                            {
                                return new FormulaError("Can't Divided by 0");
                            }
                        }
                    } catch (Exception e)
                    {
                        return new FormulaError(e.Message);
                    }                  

                }
                else if (s == "+" || s == "-") // t is "+" or "-"
                {
                    if (isOnTop(valueStack, opStack, "+", "-")) // if the top of Operator Stack is "+" or "-"
                    {
                        // perform the "+" or "-" operation
                        if (!TryPerformOps(ref valueStack, ref opStack, out double result))
                        {
                            return new FormulaError("Can't Divided by 0");
                        }
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
                        if (!TryPerformOps(ref valueStack, ref opStack, out double result))
                        {
                            return new FormulaError("Can't Divided by 0");
                        }
                    }

                    // left parathesis checking and pop it out
                    if (opStack.Count > 0 && opStack.Peek() == "(")
                    {
                        opStack.Pop();
                    }
                    else
                    {
                        return new FormulaError("Can't Divided by 0");
                    }


                    if (isOnTop(valueStack, opStack, "*", "/")) // if the top of Operator Stack is "*" or "/"
                    {
                        // perform the "*" or "/" operation
                        if (!TryPerformOps(ref valueStack, ref opStack, out double result))
                        {
                            return new FormulaError("Can't Divided by 0");
                        }
                    }
                }
                else // errors with the tokens
                {
                    return new FormulaError("Invalid tokens. Please check the Formula again!");
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
                if (!TryPerformOps(ref valueStack, ref opStack, out double result))
                {
                    return new FormulaError("Can't Divided by 0");
                }
                else
                {
                    return result;
                }
            }
            else // errors with the tokens
            {
                return new FormulaError("Invalid tokens. Please check the Formula again!");
            }


        }

        /// <summary>
        /// Enumerates the normalized versions of all of the variables that occur in this 
        /// formula.  No normalization may appear more than once in the enumeration, even 
        /// if it appears more than once in this Formula.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
        /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
        /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
        /// </summary>
        public IEnumerable<String> GetVariables()
        {
            HashSet<String> variables = new HashSet<String>();
            foreach (string s in tokens)
            {
                if (isVariable(s))
                {
                    variables.Add(normalize(s));
                }
            }
            return variables;
        }

        /// <summary>
        /// Returns a string containing no spaces which, if passed to the Formula
        /// constructor, will produce a Formula f such that this.Equals(f).  All of the
        /// variables in the string should be normalized.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
        /// new Formula("x + Y").ToString() should return "x+Y"
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in tokens)
            {
                sb.Append(normalize(s));
            }
            return sb.ToString();
        }

        /// <summary>
        /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
        /// whether or not this Formula and obj are equal.
        /// 
        /// Two Formulae are considered equal if they consist of the same tokens in the
        /// same order.  To determine token equality, all tokens are compared as strings 
        /// except for numeric tokens and variable tokens.
        /// Numeric tokens are considered equal if they are equal after being "normalized" 
        /// by C#'s standard conversion from string to double, then back to string. This 
        /// eliminates any inconsistencies due to limited floating point precision.
        /// Variable tokens are considered equal if their normalized forms are equal, as 
        /// defined by the provided normalizer.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        ///  
        /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
        /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
        /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
        /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj == null || !(obj is Formula))
            {
                return false;
            }

            Formula thatFormula = (Formula)obj;
            IEnumerable<string> thatObject = GetTokens(thatFormula.ToString());
            int i = 0;

            foreach (string s in tokens)
            {
                if (i >= thatObject.Count())
                    return false;

                if (isOperatorAndParentheses(s))
                {
                    if (!s.Equals(thatObject.ElementAt(i)))
                        return false;
                }
                else if (Double.TryParse(s, out double d1) && Double.TryParse(thatObject.ElementAt(i), out double d2))
                {
                    string str1 = d1.ToString();
                    string str2 = d2.ToString();
                    if (!d1.Equals(d2))
                        return false;
                }
                else if (isVariable(s))
                {
                    if (!normalize(s).Equals(thatObject.ElementAt(i)))
                        return false;
                }
                else
                {
                    return false;
                }
                i++;
            }


            return true;
        }

        /// <summary>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// Note that f1 and f2 cannot be null, because their types are non-nullable
        /// </summary>
        public static bool operator ==(Formula f1, Formula f2)
        {
            if (f1 is null || f2 is null)
            {
                return false;
            }
            return f1.Equals(f2);
        }

        /// <summary>
        /// Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// Note that f1 and f2 cannot be null, because their types are non-nullable
        /// </summary>
        public static bool operator !=(Formula f1, Formula f2)
        {
            if (f1 is null || f2 is null)
            {
                return false;
            }
            return !f1.Equals(f2);
        }

        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            int charHash = 0;
            int i = 1;

            foreach (string s in tokens)
            {
                string str = s;
                if (Double.TryParse(s, out double d))
                {
                    str = d.ToString();
                }
                else if (isVariable(s))
                {
                    str = normalize(s);
                }

                charHash += i * str.GetHashCode();
                i++;
            }

            return charHash;
        }

        /// <summary>
        /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
        /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
        /// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z_](?:[a-zA-Z_]|\d)*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }

        }
    }

    /// <summary>
    /// Used to report syntactic errors in the argument to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Used as a possible return value of the Formula.Evaluate method.
    /// </summary>
    public struct FormulaError
    {
        /// <summary>
        /// Constructs a FormulaError containing the explanatory reason.
        /// </summary>
        /// <param name="reason"></param>
        public FormulaError(String reason)
            : this()
        {
            Reason = reason;
        }

        /// <summary>
        ///  The reason why this FormulaError was created.
        /// </summary>
        public string Reason { get; private set; }
    }
}
