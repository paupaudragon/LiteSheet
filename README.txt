#Introduction 
This is a spreadsheet project for CS3500 2022 Fall at the Univeristy of Utah.

#Description
This program enables users to create a spreadsheet with basic math functions. 

#User's Guide
Users can either open a sprd file to get a spreadsheet or create one on the spreadsheet by entering cell content in the content page.
1. To edit a cell, users need to click on this cell and hover mouse to click the Content entry box to start editing, and press Enter key to set the content.
After this,the spreadsheet will automatically update all related cells' values.
2. To Save the spreadsheet, users can go to "File", and choose "Save" or "Save As" based on their need. 
By default, files will be saved as sprd file, but users can clear the ".sprd" entry and type a file name with wanted file extension.
3. To open a file, users need to go to "File", and then choose "Open". 
The file will be opened successfully, as long as it can be deserialized as a Spreadsheet object.
4. To start a new empty spread sheet, users can go to "File", and then choose "New".

#Extra Feature
Users can see if the data has been saved on left corner of Property Display section. 
"Saved" in green means data is saved, "Unsaved" in red means, data hasn't been saved.
This is a user friendly feature to prevent data loss.


#Implementation Notes: 
Oct 14, 2022 Tingting Zhou: 
1. Added a VerticalStackLayout to add the Property Display(Name, Value, Content) section on top of the spreadsheet area.
2. Enabled the Content entry to set a cell's content.
3. Enabled the Property Display area to correctly display values of a cell.
Problem: Spreadsheet cannot be scrolled. 

#Oct 15, 2022 Tuan Nguyen Tran:
1. Added some labels to the Property Display section and formatted them. 
2. Enabled the spreadsheet GUI to connect with Spreadsheet class.
3. Enabled to set cell Content and Value correctly on the Display section.

#Oct 17, 2022 Tingting Zhou:
1. Changed the VerticalStackLayout to GridLayout to fix scrolling issues.

#Oct 17, 2022 Tuan Nguyen Tran:
1. Implemented Double-clicked event.
2. Added recalculate feature for all other related cells when a cell changes.

#Oct 18, 2022 Tingting Zhou:
1. Implemented Save method by grabbing user's inout for a full file path.
Problem:
1. Cannot save a file without a file path but only the file name
2. Cannot detect the file type. If the use doesn't give the file type(.sprd or .txt), file will be save as "File".

#Oct 18, 2022 Tuan Nguyen Tran:
1. Added functionility to Save method: when the file already exists, the file will be overwritten.

#Oct 19, 2022 Tingting Zhou:
1. Forces file saved as a sprd file.
2. Limit the file type to be sprd when openning file.

#Oct 20, 2022 Tingting: 
1. Added Save Staus label and it's implementation.
2. Added Help menu.
3. Added Save As function. 
Problem:
1. Save status update is not abstracted to be a method.
2. Help menu is not formatted.
3. Save and Save As does not detect all save faliure. Some exceptions(CircularException, ReadAndWriteException not caught).
4. Cannot specify what causes Save faliure with correct message. 

#Oct 20-21, 2022 Tuan Nguyen Tran:
1. Implemented Save Status Update method
2. Changed Save and Save As to correctly display error message.
3. Added a method to prompt user to save data before discarding. 
4. Deleted double-click feature because it is unnecessary
5. Deleted TextChanged attribute of Content's Entry 
6. Eliminate the bug that when user is trying to edit content but not hitting enter, 
the value is still there in the spreadsheet until users click on that cell again, the value set back to empty string.
7. Impleted checking valid file name for Save, Save As, Open buttons and catch event in Open button to alert message when open the file have been using.

Problem: 
1. When users creates a circular dependency by typing, the Alert shows FileReadWriteException.
2. When a formula has a invalid cell name or cell value, the alert only states variable name is invalid.

#Oct 21, 2022	Tingting Zhou: 
1. Changed the Wrong error messages. 
2. Added comments to all files if needed.
3. Added content to Help menu and formatted the text.


#Design decisions: 
1. Use Enter key to confirm entry instead of a button: 
This is more user friendly than clicking a button everytime after editing.

2. Curser not focused in entry box
This is due to Maui is main for phone app developement instead of for pc. 
We did successfully make the curser focused in the entry box but was not able to unfocus it when needed.

3. Have "async void" and "async Task" for Save and Save As functions
async will not await if it is void. Per Microsoft documentation, if async method doesn't return anything we should return Task.
We use void in some of them, because they are EventHandler, and they have to be void.
