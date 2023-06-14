using Microsoft.Maui.Storage;
using Newtonsoft.Json.Linq;
using SS;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using Windows.Storage.Pickers;
using SpreadsheetUtilities;
using System.Text.RegularExpressions;
using Colors = Microsoft.Maui.Graphics.Colors;

namespace SpreadsheetGUI;

/// <summary>
/// Main page class that includes all content of a window.
/// 
/// This window have three vertical sections:
/// 1. Menu bar in purple
/// 2. Property display section that includes: 
///     - file save stauts label
///     - Name 
///     - Value
///     - Content
///     - File Path
/// 3. Grid area - the spreadsheet
/// </summary>
public partial class MainPage : ContentPage
{
    private Spreadsheet sheet;
    private string version;
    private string filePath;
    private string folderPath;
    private string fileName;
    private string backUpFilePath;
    private string backUpFileName;
    private string backUpFolderPath;


    /// <summary>
    /// MainPage Constructor
    /// 
    /// By default, when first started, the spreasheet should be empty, and the first cell A1 should be selected.
    /// Therefore, in the property display section, "Name" filed should "A1", while "Value", "Content","File Path" fileds should be empty.
    /// And since there is no change has been made, "Save status" should be "Saved" in green.
    /// 
    /// If there cell selection changes, a displaySelection event will update the grid selection and property display section.
    /// </summary>
    public MainPage()
    {
        InitializeComponent();

        version = "ps6";
        sheet = new Spreadsheet(s => true, s => s.ToUpper(), version);

        //init the other field to empty
        valueField.Text = String.Empty;
        contentEntry.Text = String.Empty;
        filePath = String.Empty;
        fileName = String.Empty;
        folderPath = String.Empty;
        backUpFilePath = String.Empty;
        backUpFileName = String.Empty;
        backUpFolderPath = String.Empty;

        //Default selection is (0,0)
        DisplayA1(new Spreadsheet());

        //Updates property display section whenever there is a cell selection change
        spreadsheetGrid.SelectionChanged += displaySelection;

    }

    /// <summary>
    /// Controls and updates the display of Property dispaly section and the Grid section, whenever a cell is clicked.
    /// </summary>
    /// <param name="grid">The spreadsheet grid object</param>
    private void displaySelection(SpreadsheetGrid grid)
    {
        spreadsheetGrid.GetSelection(out int col, out int row);
        nameField.Text = ((char)('A' + col)).ToString() + (row + 1);

        // Displays the value is EMPTY if it is a FormulaError, else display the value of that cell
        string content = String.Empty;

        if (sheet.GetCellValue(nameField.Text) is FormulaError)
        {
            //Based on Spreadsheet class, value should be a FormulaError
            valueField.Text = "Formula Error";
            content = "Formula Error";
        }
        else
        {
            valueField.Text = sheet.GetCellValue(nameField.Text).ToString();
            content = valueField.Text;
        }

        //Displays the content with "=" if it is Formula
        if (sheet.GetCellContents(nameField.Text) is Formula)
        {
            contentEntry.Text = "=" + sheet.GetCellContents(nameField.Text).ToString();
        }
        else
        {
            contentEntry.Text = sheet.GetCellContents(nameField.Text).ToString();
        }

        spreadsheetGrid.SetValue(col, row, content);
    }

    /// <summary>
    /// Saves the spreadsheet.
    /// 
    /// If the spreadsheet is a newly created, then user will be prompted to give a valid folder path and a file name save it.
    /// Otherwise, the old file will be overwritten by this newest version. 
    /// 
    /// This method is void because it is an EventHandler.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SaveClicked(Object sender, EventArgs e)
    {

        if (filePath.Equals(string.Empty))
        {
            await SaveHelper(sender, e);
        }
        else // If filePath already exists, save the spreadsheet to file
        {
            sheet.Save(filePath);
        }
        UpdateSave();
    }

    /// <summary>
    /// Saves a copy of the current spreadsheet with a location and name of choice.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SaveAsClicked(Object sender, EventArgs e)
    {
        await SaveHelper(sender, e);
        UpdateSave();

    }

    /// <summary>
    /// Prompts user to type a valid folder path and a file name separately.Saves this spreadsheet to this designated location.
    /// If saved successfully, "Save status" will be updated to "Saved".
    /// Else if the folder path or the file name is invalid, a "Save Failed" alert window would be poped up, and "Save status" will not be changed.
    /// 
    /// Example:
    /// 1.  Folder path should only include the folder directory, instead of including a file name.
    ///     It should be something like :C:\Users\owner\Desktop\TestFolder
    /// 
    /// If the user folder path input is:C:\Users\owner\Desktop\TestFolder, and file name input is:test, then this file should be found as
    /// "test.sprd" under the directory of "C:\Users\owner\Desktop\TestFolder", if this folder exists.
    /// Otherwise, user will get an alert window stating file saving failed.
    /// 
    /// If the user puts a valid file path, and puts file name with the extention of a file type, such as "test.txt"(gets rid of the .sprd in the entry box),
    /// the file will be saved as the wanted file type with a full file path like: "C:\Users\owner\Desktop\TestFolder\test.txt".
    /// 
    /// If the user gives a valid file path but a file name with illegal characters forbidden by the operating system, such as '*', '\',or '/', 
    /// a "Save failed" alert should be expected.
    /// 
    /// This method return type is Task because it is not an EventHandler and it doesn't return anything.
    /// </summary>
    private async Task SaveHelper(Object sender, EventArgs e)
    {
        // Back up File Name, File Path, File Folder
        BackUpFile();

        try
        {
            folderPath = await DisplayPromptAsync("Folder Path", "Please enter a valid folder path:", "OK", "Cancel", "Ex: C:\\Users\\");
            if (folderPath == null)
            {
                return;
            }

            fileName = await DisplayPromptAsync("File Name", "Please enter a valid file name:", "OK", "Cancel", null, -1, null, ".sprd");
            if (fileName == null)
            {
                return;
            }

            string name = Path.GetFileNameWithoutExtension(fileName);
            if (name != String.Empty) // File name is not empty
            {
                UpdateFilePath(folderPath + "\\" + fileName);

                //This condition check would only happen when it's Save As
                if (filePath.Equals(backUpFilePath))
                {
                    await AskOverwrite(sender, e);
                }
                else
                {
                    try
                    {
                        sheet.Save(filePath);
                        UpdateFilePath(filePath);
                    }
                    catch (SpreadsheetReadWriteException ex)
                    {
                        RetriveFile();
                        await DisplayAlert("Save Failed", ex.Message, "OK");
                    }
                }

            }
            else // Not contain a name
            {
                RetriveFile();
                await DisplayAlert("Save Failed", "File name cannot be empty", "OK");
            }


            UpdateSave();

        }
        catch (System.Exception)
        {
            RetriveFile();
            await DisplayAlert("Save Failed", "Invalid folder path", "OK");
        }
    }



    /// <summary>
    /// Updates Save status label in the Property Display section.
    /// 
    /// If the Spreadsheet object has been changed, the label should be "UNSAVED" in red.
    /// Else it should be "SAVED" in green.
    /// </summary>
    private void UpdateSave()
    {
        if (sheet.Changed)
        {
            Save_Status_Label.TextColor = Colors.Red;
            Save_Status_Label.Text = "UNSAVED";
        }
        else
        {
            Save_Status_Label.TextColor = Colors.Green;
            Save_Status_Label.Text = "SAVED";
        }
    }

    /// <summary>
    /// Back up fileName, filePath, folderPath
    /// </summary>
    private void BackUpFile()
    {
        backUpFileName = fileName;
        backUpFilePath = filePath;
        backUpFolderPath = folderPath;
    }

    /// <summary>
    /// Set fileName, filePath, folderPath back before
    /// </summary>
    private void RetriveFile()
    {
        UpdateFileName(backUpFileName);
        UpdateFilePath(backUpFilePath);
        UpdateFolderPath(backUpFolderPath);
    }

    /// <summary>
    /// Updates the filePath variable, and the "File Path" display.
    /// </summary>
    /// <param name="path">A string repres a full file path</param>
    private void UpdateFilePath(string path)
    {
        filePath = path;
        FilePathEntry.Text = path;
    }

    /// <summary>
    /// Updates the fileName variable
    /// </summary>
    /// <param name="name">A string repres a full file path</param>
    private void UpdateFileName(string name)
    {
        fileName = name;
    }


    /// <summary>
    /// Updates the folderPath variable
    /// </summary>
    /// <param name="fPath">A string repres a full file path</param>
    private void UpdateFolderPath(string fPath)
    {
        folderPath = fPath;
    }

    /// <summary>
    /// Copy of SaveClicked() with return type is Task
    /// 
    /// This method returns Taks because async method doesn't await void, if it is not a EventHandler.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns>None</returns>
    private async Task ToSaveClicked(Object sender, EventArgs e)
    {

        if (filePath.Equals(string.Empty))
        {
            await SaveHelper(sender, e);
        }
        else // If filePath already exists, save the spreadsheet to file
        {
            sheet.Save(filePath);
        }
        UpdateSave();
    }

    /// <summary>
    /// Prompts the user to save the current data before discarding.
    /// 
    /// This method returns Taks because async method doesn't await void, if it is not a EventHandler.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns>None</returns>
    private async Task AskSave(Object sender, EventArgs e)
    {
        var action = await DisplayAlert("Save File?", "Do you want to save unsaved changes?", "Yes", "No");
        if (action)
        {
            await ToSaveClicked(sender, e);
            Thread.Sleep(300);
        }
    }

    /// <summary>
    /// Asks the user if they want to overwrite the current file.
    /// If not, they will be propmt to give a different file path.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private async Task AskOverwrite(Object sender, EventArgs e)
    {
        var action = await DisplayAlert("Overwrite File?", "Do you want to overwrite current file?", "Yes", "No");
        if (action)
        {
            await ToSaveClicked(sender, e);
            await DisplayAlert("Successfully Overwritten", "", "OK");
        }
        else
        {
            await SaveHelper(sender, e);
        }
    }

    /// <summary>
    /// Creates a new empty spreadsheet and start view with everything the same as what the constructor creates.
    /// 
    /// If the current spreadsheet has unsaved data, the user will be asked to save it.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void NewClicked(Object sender, EventArgs e)
    {
        if (sheet.Changed)
        {
            await AskSave(sender, e); // Ask to save to a file
        }

        //Clear and create a new spreadsheet, set all fields as init
        spreadsheetGrid.Clear();
        sheet = new Spreadsheet(s => true, s => s.ToUpper(), version);
        spreadsheetGrid.SetSelection(0, 0);
        DisplayA1(sheet);
        filePath = String.Empty;
        fileName = String.Empty;
        folderPath = String.Empty;
        UpdateFilePath(filePath);
        UpdateFileName(fileName);
        UpdateFolderPath(folderPath);
        UpdateSave();
    }

    /// <summary>
    /// Opens a sprd file and reads it into the spreadsheet.
    /// 
    /// If the current spreadsheet has unsaved data, the user will be asked to save it. 
    /// 
    /// File will be opened successfully in one conditions:
    /// 1. File has be to successfully read into a Spreadsheet object.
    /// 
    /// For example:
    /// If a file's content is invalid for Spreadsheet class, such as "Hello World", a "Open Failed" alert should be expected.
    /// 
    /// </summary>
    private async void OpenClicked(Object sender, EventArgs e)
    {
        if (sheet.Changed)
        {
            await AskSave(sender, e); // Ask to save to a file
        }

        //Back up the file path
        BackUpFile();

        try
        {
            FileResult fileResult = await FilePicker.Default.PickAsync();


            if (fileResult != null)
            {

                if (fileName == fileResult.FileName)
                {
                    await DisplayAlert("This file has been used in spreadsheet", "", "OK");
                }


                //Load spreadsheet from a file and display to the SpreadsheetGrid
                fileName = fileResult.FileName;
                UpdateFilePath(fileResult.FullPath);

                //need to catch Exception here to avoid lose data from the pre-opened file.
                try
                {
                    sheet = new Spreadsheet(fileResult.FullPath, s => true, s => s.ToUpper(), version);
                    DisplayA1(sheet);
                    spreadsheetGrid.Clear();
                }
                catch (System.Exception)
                {
                    RetriveFile();
                    await DisplayAlert("Open Failed", "", "OK");
                }

                await DisplayListOfValue(sheet, sheet.GetNamesOfAllNonemptyCells());
            }

            UpdateSave();
        }
        catch (System.Exception)
        {
            RetriveFile();
            await DisplayAlert("Open Failed", "", "OK");
        }


    }

    /// <summary>
    /// Ends the user input in the Content Entry box by pressing "Enter" key. And updates the contents related to this cell and its dependent cells.
    /// 
    /// If the new content would trigger a FormulaFormatException, nothing will change on this spreadsheet and an alert will be poped up. 
    /// 
    /// For Example:
    /// 1. A1: "=A01": an alert should be expected because "A01" is not a valid cell name.
    /// 2. If set A1: "1", A2: "=A1+1". Then, set A1: "abc".
    ///    An alert should be expected, and the value a A1 will be "abc", while A2 being "FormulaError", 
    ///    because the variable cell A1's value is invalid for this function.
    ///       
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Entry_Completed(object sender, EventArgs e)
    {
        //Set the content for the cell when enter is hit
        spreadsheetGrid.GetSelection(out int col, out int row);
        string cellName = ((char)('A' + col)).ToString() + (row + 1);

        try
        {
            List<string> recalculateList = (List<string>)sheet.SetContentsOfCell(cellName, contentEntry.Text);

            if (contentEntry.Text == String.Empty)
            {
                spreadsheetGrid.SetValue(col, row, String.Empty);
            }

            await DisplayListOfValue(sheet, recalculateList);

            //Move to the next row 
            if (row + 1 <= 99)
            {
                spreadsheetGrid.SetSelection(col, row + 1);
                displaySelection(spreadsheetGrid);
            }

            UpdateSave();
        }
        catch (SpreadsheetReadWriteException ex)
        {

            string exception = "Spreadsheet Read And Write Exception";
            HandleException(col, row, ex.Message, exception);
        }
        catch (FormulaFormatException ex) // Fail to parse the remainder to formula, read message to know the reason
        {
            string exception = "Formula Format Exception";
            HandleException(col, row, ex.Message, exception);
        }
        catch (CircularException)
        {
            HandleException(col, row, "A circular dependecy occurred", "Circular Exception");
        }


    }

    /// <summary>
    /// Helper method to handle exception
    /// 
    /// Sets value of a given cell to empty string, and displays an alert with exception type and message.
    /// Redisplay the whole view.
    /// </summary>
    /// <param name="col"></param>
    /// <param name="row"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    private async void HandleException(int col, int row, string message, string exception)
    {
        spreadsheetGrid.SetValue(col, row, "");
        await DisplayAlert(exception, message, "OK");
        displaySelection(spreadsheetGrid);
    }

    /// <summary>
    /// Displays cell A1's name, value and content, and select A1 in the spreadsheet grid.
    /// </summary>
    /// <param name="sheet">A spreadsheet</param>
    private void DisplayA1(Spreadsheet sheet)
    {
        spreadsheetGrid.SetSelection(0, 0);
        nameField.Text = "A1";
        valueField.Text = sheet.GetCellValue("A1").ToString();
        contentEntry.Text = sheet.GetCellContents("A1").ToString();
    }

    /// <summary>
    /// Displays one cell's value in the spreadsheet grid.
    /// </summary>
    /// <param name="name">The name of a cell</param>
    /// <param name="sheet">A spreadsheet</param>
    /// <param name="columnChar">The column letter of the cell</param>
    /// <param name="rowNum">The row number of the cell</param>
    /// <returns></returns>
    private async Task DisplayOneValue(string name, Spreadsheet sheet, int columnChar, int rowNum)
    {
        if (sheet.GetCellValue(name) is FormulaError) // Error with Formula (divided by 0 OR unknown variables)
        {
            FormulaError fe = (FormulaError)sheet.GetCellValue(name);
            spreadsheetGrid.SetValue(columnChar, rowNum, "Formula Error");
            await DisplayAlert("Formula Error", name + ": " + fe.Reason, "OK");
        }
        else //Display the cell's value
        {
            spreadsheetGrid.SetValue(columnChar, rowNum, sheet.GetCellValue(name).ToString());
        }
    }

    /// <summary>
    /// Displays a list of cell values on the spreadsheet grid.
    /// </summary>
    /// <param name="sheet">A spreadsheet object</param>
    /// <param name="list">A list of spreadsheet cell names</param>
    /// <returns>None</returns>
    private async Task DisplayListOfValue(Spreadsheet sheet, IEnumerable<string> list)
    {
        foreach (string s in list)
        {
            int columnChar = s[0] - 'A'; // Get the column number

            if (Int32.TryParse(s.Substring(1), out int rowNum))
            {
                rowNum = rowNum - 1;  // Get the row number             
            }

            await DisplayOneValue(s, sheet, columnChar, rowNum);
        }
    }

    /// <summary>
    /// Displays a user's guide.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void HelpClicked(Object sender, EventArgs e)
    {

        string message =
             "1. Edit Cell Content\r\n" +
             "* Select a cell by clicking on the cell.\r" +
             "* Click the content edit entry to edit this cell's content.\r" +
             "* Press \"Enter\" key to set the content to selected cell.\r" +
             "Note: If \"Enter\" is not pressed, data in the entry box will be lost.\r\n\n" +

             "2. Save File\r\n" +
             "* Click \"File\" on the menu bar.\r" +
             "* Click \"Save\".\r " +
             " - If the spreasheet is a newly created, the user will be prompted to put a folder\r" +
             "   path and a file name. Both entries have to be valid to sucessfully save the file.\r " +
             " - If the file already exists, it will be overwritten. \r" +
             "* Click \"Save As\". File will be saved in a valid user-given location and name.\r" +
             " - If the new file path is identical to the current one, a decision pop-up will ocurr to remind the user.\r" +
             "Note: Both by default will save the file as sprd file, but users can edit file extention when entering file name.\r\n\n"

             + "3. Open File\r\n" +
             "* Click \"File\" on the menu bar.\r" +
             "* Click \"Open\".\r" +
             "* Choose a file that can be deserialized by Spreadsheet class.\r" +
             " - If the sprd file cannot be deserialized by Spreadsheet, a fail alert occurs.\r\n\n" +

             "4. Formula Error\r\n" +
             " - If a function has invalid variable cell name or value, a Formula Error alert occurs.\r" +
             " - If a division of zero occurs, a Formula Error alert should be expected.\r\n\n" +

            "5. FormulaFormatException\r\n" +
            " - If a function has syntax error, a Formula Format Exception alert occurs.\r\n\n" +

            "6. CircularException\r\n" +
            " - If a formula create a circle, Circula Exception alert accurrs.\r\n\n" +

            "7. SpreadsheetReadWriteException\r\n" +
            " - If the program is fail to save file or load file, SpreadsheetReadWriteException alert occurs.\n";

        await DisplayAlert("User's Guide", message, "OK");

    }

}
