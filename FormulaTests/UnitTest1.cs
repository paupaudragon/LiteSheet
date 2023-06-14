using SpreadsheetUtilities;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace FormulaTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestForException1()
        {
            // Balanced Parentheses Rule: Extra one Open Parentheses
            Formula formula = new Formula("5+((1+2*10)");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestForException2()
        {
            //Right Parentheses Rule: Extra one Closed Parentheses
            Formula formula = new Formula("5+(1+2))+10");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestForException3()
        {
            //Balanced Parentheses Rule: Missing one Closed Parentheses
            Formula formula = new Formula("(1+2+5*(6+9)");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestForException4()
        {
            //Violate Parenthesis/Operator Following Rule: 2++5
            Formula formula = new Formula("(1+2++5*(6+9)");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestForException5()
        {
            //Violate Extra Following Rule: 5(6+9)
            Formula formula = new Formula("(1+2+5(6+9)");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestForException6()
        {
            //Violate Parsing Rule: 5.(6+9)
            Formula formula = new Formula("(1+2+5.(6+9)");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestForException7()
        {
            //Violate Starting token rule: ")"
            Formula formula = new Formula(")+1+10*2");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestForException8()
        {
            //Violate Ending token rule: "("
            Formula formula = new Formula("1+10*2+(");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestForException9()
        {
            //Violate One token rule: Empty Formula
            Formula formula = new Formula("");
        }

        [TestMethod]
        public void TestForUnknownVariable()
        {
            //Test for unknown variables: A4 is unknown variable
            Formula formula = new Formula("A1+1e-7+a4+0.5+1.5-5*(1+2+3)/3", normalize, isValid);
            Assert.IsInstanceOfType(formula.Evaluate(LookUp), typeof(FormulaError));
        }

        [TestMethod]
        public void TestComparingToEmptyString()
        {
            Formula formula = new Formula("a1+2");
            Assert.IsFalse(formula.Equals(""));
        }


        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestConstructor()
        {
            //valid variables
            Formula formula = new Formula("x1+x2+0.123");
            formula = new Formula("__+x2+0.123");
            formula = new Formula("_1+x2+0.123");
            formula = new Formula("_+x2+0.123");

            //Invalid variables because of isValid() delegate don't allow underscore for variables
            formula = new Formula("A1+_2+0.123", normalize, isValid);
        }

        [TestMethod]
        public void TestGetHashCode()
        {
            Formula formula1 = new Formula("X1+X2+0.1230+1", normalize, isValid);
            Formula formula2 = new Formula("x1+x2+0.123+1.00", normalize, isValid);
            Assert.IsTrue(formula1.GetHashCode() == formula2.GetHashCode());

            formula1 = new Formula("X1+0.123+X2", normalize, isValid);
            formula2 = new Formula("X1+X2+0.123", normalize, isValid);
            Assert.IsFalse(formula1.GetHashCode() == formula2.GetHashCode());

            formula1 = new Formula("A1+a1+b2+5+7+8+9.01*5*(1+2+3+4+5)", normalize, isValid);
            formula2 = new Formula("a1+a1+b2+5+7+8+9.01*5*(5+2+3+4+1)");
            Assert.IsFalse(formula1.GetHashCode() == formula2.GetHashCode());

        }


        [TestMethod]
        public void TestEqualAndNotEqualOperator()
        {
            //Test for == operator
            Formula formula1 = new Formula("X1+X2+0.123", normalize, isValid);
            Formula formula2 = new Formula("x1+x2+0.123000", normalize, isValid);
            Assert.IsTrue(formula1 == formula2);

             formula1 = new Formula("X1+X2+0.123", normalize, isValid);
             formula2 = new Formula("x1+x2+0.123000");
            Assert.IsTrue(formula1 == formula2);

            //Test for != operator
            formula1 = new Formula("X1+X2+0.123", normalize, isValid);
            formula2 = new Formula("X1+0.123+X2", normalize, isValid);
            Assert.IsTrue(formula1 != formula2);

            //Test for null 
            Formula formula = new Formula("a1+2");
            Assert.IsFalse(formula == null);

            formula = new Formula("a1+2");
            Assert.IsFalse(formula != null);
        }


        [TestMethod]
        public void TestEqualsMethod()
        {
            Formula formula = new Formula("0.123 + X1 +X2 + 1", normalize, isValid);
            Assert.IsTrue(formula.Equals(new Formula("0.123000 + x1 +x2 + 1.0")));
            Assert.IsFalse(formula.Equals(null));
        }


        [TestMethod]
        public void TestGetVariables()
        {
            Formula formula = new Formula("A1+a1+b2+5+7+8+9.01", normalize, isValid);
            Assert.IsTrue(new string[] { "a1", "b2" }.SequenceEqual(formula.GetVariables()));

            formula = new Formula("A1+a1+b2+5+7+8+9.01");
            Assert.IsTrue(new string[] { "A1", "a1", "b2" }.SequenceEqual(formula.GetVariables()));
        }

        [TestMethod]
        public void TestEvaluateMethod()
        {

            //Normal case           
            Formula formula = new Formula("5+3", normalize, isValid);
            Assert.AreEqual(8.0, formula.Evaluate(LookUp));

            formula = new Formula("A1*A2*A3+5*(5+5)/5", normalize, isValid);
            Assert.AreEqual(16.0, formula.Evaluate(LookUp));


            formula = new Formula("A1+A2+0.5+1.5-5*(1+2+3)/3", normalize, isValid);
            Assert.AreEqual(-5.0, formula.Evaluate(LookUp));

            //divide by 0 
            formula = new Formula("A1+A2+0.5+1.5-5*(1+2+3)/0", normalize, isValid);
            Assert.IsInstanceOfType(formula.Evaluate(LookUp), typeof(FormulaError));

            formula = new Formula("A3/0", normalize, isValid);
            Assert.IsInstanceOfType(formula.Evaluate(LookUp), typeof(FormulaError));

            //Test for single token
            formula = new Formula("55", normalize, isValid);
            Assert.AreEqual(formula.Evaluate(LookUp), 55.0);

        }

        private bool isValid(string s)
        {
            return Regex.IsMatch(s, "^[A-Za-z]+[0-9]+$");
        }

        private string normalize(string s)
        {
            foreach (char c in s)
            {
                if (Char.IsUpper(c))
                    return s.Substring(0, 1).ToLower() + s.Substring(1);
            }
            return s;
        }

        private double LookUp(String s) // Lookup function
        {
            if (s.Equals("a1"))
            {

                return 1;
            }
            if (s.Equals("a2"))
            {
                return 2;
            }
            if (s.Equals("a3"))
            {
                return 3;
            }
            else
            {
                throw new ArgumentException("The variables don't exist");
            }
        }
    }
}