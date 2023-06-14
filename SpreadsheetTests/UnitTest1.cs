// Written by Tuan Nguyen Tran
// Course: CS 3500
// Date: Sep 23, 2022
// Assigment: PS4

using SpreadsheetUtilities;
using SS;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SpreadsheetTests
{
    [TestClass]
    public class UnitTest1
    {
        private bool IsValid(string s)
        {
            return Regex.IsMatch(s, "^[A-Za-z]+[0-9]+$");
        }

        private string Normalize(string s)
        {
            foreach (char c in s)
            {
                if (Char.IsLower(c))
                    return s.Substring(0, 1).ToUpper() + s.Substring(1);
            }
            return s;
        }


        [TestMethod]
        public void TestExplicitEmptySet()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("C1", "");
            Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }



        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestForSaveMethod2()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet();

            //test for empty path
            spreadsheet.Save("");
        }


        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestForInvalidPath1()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet("&^file", IsValid, Normalize, "2.0");

        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestForInvalidPath2()
        {
            AbstractSpreadsheet spreadsheet = new Spreadsheet("", IsValid, Normalize, "2.0");
        }


        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestForInvalidCellName()
        {
            //Create Json file
            string sheet = @"{
                  ""cells"": {
                     ""A1"": {
                       ""stringForm"": ""ABC""
                     },
                    ""1B"": { //invalid cell name
                           ""stringForm"": ""=A1+5""
                     }
                   },
                   ""Version"": ""default""
            }";
            File.WriteAllText("JsonFile.txt", sheet);
            AbstractSpreadsheet spreadsheet = new Spreadsheet("JsonFile.txt", IsValid, Normalize, "default");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestForInvalidFormula()
        {
            //Create Json file
            string sheet = @"{
                  ""cells"": {
                     ""A1"": {
                       ""stringForm"": ""ABC""
                     },
                    ""B1"": { 
                           ""stringForm"": ""=_1+5"" //invalid Formula
                     }
                   },
                   ""Version"": ""default""
            }";
            File.WriteAllText("JsonFile.txt", sheet);
            AbstractSpreadsheet spreadsheet = new Spreadsheet("JsonFile.txt", IsValid, Normalize, "2.0");
        }

        [TestMethod]
        public void TestForGetCellValue()
        {
            //Create Json file
            string sheet = @"{
                  ""cells"": {
                     ""a1"": {
                       ""stringForm"": ""50""
                     },
                    ""b1"": {
                           ""stringForm"": ""=A1+5""
                    },
                    ""c1"": {
                           ""stringForm"": ""=B1+A1+5""
                    },
                    ""d1"": {
                           ""stringForm"": ""String""
                    },
                    ""e1"": {
                           ""stringForm"": ""=D1+C1""
                    }
                    
                   },
                   ""Version"": ""default""
            }";
            File.WriteAllText("JsonFile.txt", sheet);
            AbstractSpreadsheet spreadsheet = new Spreadsheet("JsonFile.txt", IsValid, Normalize, "default");

            //Trying to get value of certain cells
            Assert.AreEqual(110.0, spreadsheet.GetCellValue("C1"));
            Assert.AreEqual(55.0, spreadsheet.GetCellValue("B1"));
            Assert.AreEqual(50.0, spreadsheet.GetCellValue("A1"));
            Assert.AreEqual("String", spreadsheet.GetCellValue("D1"));
            Assert.IsInstanceOfType(spreadsheet.GetCellValue("E1"), typeof(FormulaError));
        }


        [TestMethod]
        public void TestForConstructor()
        {
            //Test for first two constructors
            AbstractSpreadsheet sheet1 = new Spreadsheet();
            sheet1.Save("JsonFile1.txt");

            AbstractSpreadsheet sheet2 = new Spreadsheet(IsValid, Normalize, "");
            sheet2.Save("JsonFile2.txt");


            //Create Json file
            string sheet = @"{
                  ""cells"": {
                     ""A1"": {
                       ""stringForm"": ""ABC""
                     },
                    ""B3"": {
                           ""stringForm"": ""=A1+5""
                     }
                   },
                   ""Version"": ""default""
            }";
            File.WriteAllText("JsonFile3.txt", sheet);
            AbstractSpreadsheet spreadsheet = new Spreadsheet("JsonFile3.txt", IsValid, Normalize, "default");

        }

        [TestMethod]
        public void StressTest()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("A2", "abc");
            sheet.SetContentsOfCell("B2", "5.0");
            sheet.SetContentsOfCell("C2", "=B2+A2");
            sheet.SetContentsOfCell("D2", "=C2+5");
            sheet.SetContentsOfCell("E2", "=A2+B2+C2+D2");
            sheet.SetContentsOfCell("G2", "=H2");

            //Testing Content of Cells
            Assert.IsTrue(new string[] { "A2", "B2", "C2", "D2", "E2", "G2" }.SequenceEqual(sheet.GetNamesOfAllNonemptyCells()));
            Assert.AreEqual("abc", sheet.GetCellContents("A2"));
            Assert.AreEqual(5.0, sheet.GetCellContents("B2"));
            Assert.AreEqual(new Formula("B2+A2"), sheet.GetCellContents("C2"));
            Assert.AreEqual("", sheet.GetCellContents("F2"));

            //Replace existing Cell
            //Update A2 = 10.0
            Assert.IsTrue(new List<string>() { "A2", "C2", "D2", "E2" }.SequenceEqual(sheet.SetContentsOfCell("A2", "10.0")));

            //Update C2 = "Z1+1"
            Assert.IsTrue(new List<string>() { "C2", "D2", "E2", }.SequenceEqual(sheet.SetContentsOfCell("C2", "=Z1+1")));


            //Test for dependent list of B2, now only { "B2", "E2" }, because C2 is no longer dependent on B2
            Assert.IsTrue(new List<string>() { "B2", "E2" }.SequenceEqual(sheet.SetContentsOfCell("B2", "=1+2+3")));


            //Update C2 = Formula("B2+5")
            Assert.IsTrue(new List<string>() { "C2", "D2", "E2", }.SequenceEqual(sheet.SetContentsOfCell("C2", "=B2+5")));

            //Test for dependent list of B2, now is { "B2","C2","D2","E2" }
            Console.WriteLine(String.Join(", ", sheet.SetContentsOfCell("B2", "=1+2+3")));
            Assert.IsTrue(new List<string>() { "B2", "C2", "D2", "E2" }.SequenceEqual(sheet.SetContentsOfCell("B2", "=1+2+3")));

            //Create Json file
            string sheet1 = @"{
                  ""cells"": {
                     ""a1"": {
                       ""stringForm"": ""50""
                     },
                    ""b1"": {
                           ""stringForm"": ""=A1+5""
                    },
                    ""c1"": {
                           ""stringForm"": ""=B1+A1+5""
                    },
                    ""d1"": {
                           ""stringForm"": ""=B1+C1""
                    }
                    
                   },
                   ""Version"": ""default""
            }";
            File.WriteAllText("JsonFile.txt", sheet1);
            AbstractSpreadsheet spreadsheet = new Spreadsheet("JsonFile.txt", IsValid, Normalize, "default");
            spreadsheet.SetContentsOfCell("e1", "20");
            spreadsheet.SetContentsOfCell("f1", "40");
            spreadsheet.SetContentsOfCell("g1", "=e1+f1");
            Assert.AreEqual(60.0, spreadsheet.GetCellValue("g1"));


        }

        [TestMethod]
        public void TestForSetContentsOfCell()
        {
            AbstractSpreadsheet s = new Spreadsheet(s => true, s => s.ToUpper(), "");
            s.SetContentsOfCell("a1", "10");
            s.SetContentsOfCell("A1", "12");
            s.SetContentsOfCell("C1", "= a1");
            Assert.AreEqual(12, (double)s.GetCellValue("C1"));


        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestInvalidException1()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("%A", "5.0");
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestInvalidException2()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("2A", "=D1+1");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestInvalidException3()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("@B", "Hello World");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestInvalidException4()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.GetCellContents("#a");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestForCircularException()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("A1", "5.0");
            sheet.SetContentsOfCell("B1", "=A1+1");
            sheet.SetContentsOfCell("C1", "=B1+A1");
            sheet.SetContentsOfCell("D1", "=B1+C1");
            sheet.SetContentsOfCell("A1", "=D1");
        }

        [TestMethod]
        public void TestGetDirectDependents()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("A1", "5.0");
            sheet.SetContentsOfCell("B1", "=A1*2");
            sheet.SetContentsOfCell("C1", "=B1+A1");
            Assert.IsTrue(new List<string>() { "A1", "B1", "C1", }.SequenceEqual(sheet.SetContentsOfCell("A1", "abc")));
        }

        [TestMethod]
        public void TestGetCellContents()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("A1", "5");
            sheet.SetContentsOfCell("B1", "=A1+1");
            sheet.SetContentsOfCell("C1", "=B1+A1");
            sheet.SetContentsOfCell("D1", "=B1+C1");
            sheet.SetContentsOfCell("E1", "Hello World");

            //Check for the type of Cells
            Assert.AreEqual(5.0, sheet.GetCellContents("A1"));
            Assert.IsInstanceOfType(sheet.GetCellContents("A1"), typeof(Double));
            Assert.IsInstanceOfType(sheet.GetCellContents("B1"), typeof(Formula));
            Assert.IsInstanceOfType(sheet.GetCellContents("E1"), typeof(String));
            Assert.AreEqual(sheet.GetCellContents("F1"), "");
        }

        [TestMethod]
        public void TestSetContentsOfCell()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();

            //sheet.SetContentsOfCell("B1", "");
            //Assert.IsFalse(sheet.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());

            sheet.SetContentsOfCell("A1", "5.0");
            sheet.SetContentsOfCell("B1", "=A1+1");
            sheet.SetContentsOfCell("C1", "=B1+A1");
            sheet.SetContentsOfCell("D1", "=B1+C1");
            sheet.SetContentsOfCell("E1", "");
            Assert.IsTrue(new string[] { "A1", "B1", "C1", "D1" }.SequenceEqual(sheet.GetNamesOfAllNonemptyCells()));

        }
    }
}