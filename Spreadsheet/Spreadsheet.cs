// Written by Tuan Nguyen Tran
// Course: CS 3500
// Date: Sep 23, 2022
// Assigment: PS4


using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpreadsheetUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SS
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Spreadsheet : AbstractSpreadsheet
    {
        //field
        [JsonProperty("cells")]
        private Dictionary<string, Cell> sheet; // A dictionary to define a cell (cell's name -> cell's content)

        private readonly DependencyGraph graph; // dependency graph for cells' relationship
        private readonly string? filePath;
        private bool changed;

        //Calling base constructor of Spreadsheet
        public Spreadsheet() : this(valid => true, s => s, "default")
        {
            sheet = new Dictionary<string, Cell>();
            graph = new DependencyGraph();
            Changed = false;
        }

        //Base constructor of Spreadsheet
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version) : base(isValid, normalize, version)
        {
            sheet = new Dictionary<string, Cell>();
            graph = new DependencyGraph();
            Changed = false;
        }

        //Calling base constructor of Spreadsheet
        public Spreadsheet(string filePath, Func<string, bool> isValid, Func<string, string> normalize, string version) : this(isValid, normalize, version)
        {
            //Check for version
            if (version != GetVersion(filePath))
                throw new SpreadsheetReadWriteException("The version of the file does not matach version input");

            try
            {
                this.filePath = filePath;


                Spreadsheet? dataList = JsonConvert.DeserializeObject<Spreadsheet>(File.ReadAllText(filePath));
                if (dataList is not null)
                {
                    foreach (string s in dataList.sheet.Keys)
                    {
                        if (dataList.sheet[s].StringForm is not null)
                        {
                            this.SetContentsOfCell(s, dataList.sheet[s].StringForm);
                        }

                    }
                }

                Changed = false;
            }
            catch (InvalidNameException)
            {
                throw new SpreadsheetReadWriteException("Invalid variable name");
            }
            catch (CircularException)
            {
                throw new SpreadsheetReadWriteException("Formula created Circle");
            }
            catch (FormulaFormatException)
            {
                throw new SpreadsheetReadWriteException("The Formula contains invalid string");
            }
            catch (Exception)
            {
                throw new SpreadsheetReadWriteException("Something wrong when opening or reading the file");
            }

        }

        // ADDED FOR PS5
        /// <summary>
        /// True if this spreadsheet has been modified since it was created or saved                  
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed
        {
            get { return changed; }
            protected set { changed = value; }
        }

        /// <summary>
        /// Getting the version of the file
        /// </summary>
        /// <param name="filename">name of the file</param>
        /// <returns></returns>
        /// <exception cref="SpreadsheetReadWriteException"></exception>
        private string GetVersion(string filename)
        {
            string version = string.Empty;
            try
            {
                Spreadsheet? dataList = JsonConvert.DeserializeObject<Spreadsheet>(File.ReadAllText(filename));
                if (dataList is not null)
                {
                    version = dataList.Version;
                }
            }
            catch (Exception)
            {
                throw new SpreadsheetReadWriteException("Something wrong when trying to read the version of file");
            }

            return version;
        }



        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(string name)
        {
            //Checking valid cell name and Normalize it
            if (!CheckCellName(ref name))
                throw new InvalidNameException();

            //If spreadsheet contains this cell, return the content of it
            if (sheet.ContainsKey(name))
            {
                return sheet[name].Content;
            }

            return string.Empty;
        }
        /// <summary>
        /// Cell look up method
        /// </summary>
        /// <param name="name">the name of cell</param>
        /// <returns>return the value of that cell OR throw ArgumentException</returns>
        /// <exception cref="ArgumentException"></exception>
        private double lookup(string name)
        {
            //Checking valid cell name and Normalize it
            if (!CheckCellName(ref name))
                throw new InvalidNameException();

            if (GetCellValue(name) is double d)
            {
                return d;
            }
            else
            {
                throw new ArgumentException("The variable cells don't exist, or their values are invalid for the function");// It could be the variable has no value or it is an illegal variable like A01.
            }

        }

        // ADDED FOR PS5
        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>
        public override object GetCellValue(string name)
        {
            //Checking valid cell name and Normalize it
            if (!CheckCellName(ref name))
                throw new InvalidNameException();

            if (sheet.ContainsKey(name))
            {
                return sheet[name].Value;
            }

            return string.Empty;
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            return sheet.Keys;
        }

        // ADDED FOR PS5
        /// <summary>
        /// Writes the contents of this spreadsheet to the named file using a JSON format.
        /// The JSON object should have the following fields:
        /// "Version" - the version of the spreadsheet software (a string)
        /// "cells" - an object containing 0 or more cell objects
        ///           Each cell object has a field named after the cell itself 
        ///           The value of that field is another object representing the cell's contents
        ///               The contents object has a single field called "stringForm",
        ///               representing the string form of the cell's contents
        ///               - If the contents is a string, the value of stringForm is that string
        ///               - If the contents is a double d, the value of stringForm is d.ToString()
        ///               - If the contents is a Formula f, the value of stringForm is "=" + f.ToString()
        /// 
        /// For example, if this spreadsheet has a version of "default" 
        /// and contains a cell "A1" with contents being the double 5.0 
        /// and a cell "B3" with contents being the Formula("A1+2"), 
        /// a JSON string produced by this method would be:
        /// 
        /// {
        ///   "cells": {
        ///     "A1": {
        ///       "stringForm": "5"
        ///     },
        ///     "B3": {
        ///       "stringForm": "=A1+2"
        ///     }
        ///   },
        ///   "Version": "default"
        /// }
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override void Save(string filename)
        {
            try
            {
                File.WriteAllText(filename, JsonConvert.SerializeObject(this));
                Changed = false;
            }
            catch (Exception e)
            {
                throw new SpreadsheetReadWriteException(e.Message);
            }

        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        protected override IList<string> SetCellContents(string name, double number)
        {

            //Set new content for the cell
            SetCellContentsHelper(name, number);

            //Replace all the variables that this cell depent on
            graph.ReplaceDependees(name, new HashSet<string>());

            //Recaculate for every cell related to this cell
            return GetCellsToRecalculate(name).ToList();
        }


        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        protected override IList<string> SetCellContents(string name, string text)
        {


            //Set new content for the cell
            SetCellContentsHelper(name, text);

            //Replace all the variables that this cell depent on
            graph.ReplaceDependees(name, new HashSet<string>());

            //Recaculate for every cell related to this cell
            return GetCellsToRecalculate(name).ToList();
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException, and no change is made to the spreadsheet.
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        protected override IList<string> SetCellContents(string name, Formula formula)
        {
            //back up Denpendees' list of the cell
            HashSet<string> backupDependees = (HashSet<string>)graph.GetDependees(name);

            //Get all the variables of the formula
            IEnumerable<string> tokens = formula.GetVariables();

            //Replace all the variables that this cell depent on
            graph.ReplaceDependees(name, tokens);

            try
            {
                //Recaculate for every cell related to this cell
                IEnumerable<string> cellRecalculate = GetCellsToRecalculate(name);

                //Set new content for the cell
                SetCellContentsHelper(name, formula);

                return cellRecalculate.ToList();
            }
            catch (CircularException)
            {
                //If it throws CircularException, back up everything if there is a change
                graph.ReplaceDependees(name, backupDependees);
                //throw new SpreadsheetReadWriteException("Formula created circle");
                throw new CircularException();// this is supposed to be a circular exception because it doesn't happen when read or write.
            }

        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        /// 
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor.  There are then three possibilities:
        /// 
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown,
        ///       and no change is made to the spreadsheet.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a list consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell. The order of the list should be any
        /// order such that if cells are re-evaluated in that order, their dependencies 
        /// are satisfied by the time they are evaluated.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        public override IList<string> SetContentsOfCell(string name, string content)
        {
            //Checking valid cell name and Normalize it
            if (!CheckCellName(ref name))
                throw new InvalidNameException();

            IList<string> list;

            if (content.Equals(""))
            {
                sheet.Remove(name);
                return new List<string>();
            }
                
            if (Double.TryParse(content, out double d))
            {
                list = SetCellContents(name, d);
            }
            else if (content[0].Equals('='))
            {
                string f = content.Substring(1);
                list = SetCellContents(name, new Formula(f, Normalize, IsValidCell));
            }
            else
            {
                list = SetCellContents(name, content);
            }

            //Recaculate every cells in the list
            foreach (string cellName in list)
            {
                if (sheet.ContainsKey(cellName))
                    sheet[cellName].UpdateField(lookup);
            }

            return list;

        }

        /// <summary>
        /// Returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            return graph.GetDependents(name);
        }



        /// <summary>
        /// Check the pattern of what is the valid cell
        /// </summary>
        /// <param name="s">Cell's name</param>
        /// <returns></returns>
        private bool IsValidCell(string s)
        {
            String varPattern = @"^[a-zA-Z](?:[a-zA-Z]|\d)*$";
            if (Regex.IsMatch(s, varPattern))
            {
                if (IsValid(s))
                    return true;
            }
            return false;
        }

        private bool CheckCellName(ref string name)
        {
            if (IsValidCell(name))
            {
                name = Normalize(name);
                return true;
            }

            return false;

        }

        /// <summary>
        /// A helper for SetCellContents() to add or update the cell's content
        /// </summary>
        /// <param name="name">Cell's name</param>
        /// <param name="content">Cell's content can be string, number, or Formula</param>
        private void SetCellContentsHelper(string name, object content)
        {

            if (sheet.ContainsKey(name))
            {
                sheet[name].Content = content;
            }
            else //if don't, create new cell with new content
            {
                Cell cell = new Cell(content);
                sheet.Add(name, cell);            
            }
            Changed = true;
        }


        /// <summary>
        /// Every Cell class contains the content(string, number, or Formula)
        /// </summary>
        /// 

        private class Cell
        {

            [Newtonsoft.Json.JsonIgnore]
            public object Content { get; set; }

            [Newtonsoft.Json.JsonIgnore]
            public object Value { get; set; }

            [JsonProperty(PropertyName = "stringForm")]
            public string StringForm { get; set; }

            public Cell(object content)
            {
                this.Content = content;
                StringForm = string.Empty;
                this.Value = string.Empty;
            }

            //Update Value and StringForm when have any change
            public void UpdateField(Func<string, double> lookup)
            {
                Value = Content;
                if (this.Content is Formula f)
                {
                    StringForm = "=" + f.ToString();
                    Value = f.Evaluate(lookup);
                }
                else if (Content.ToString() is string s)
                {
                    StringForm = s;
                }
            }

        }

    }
}
