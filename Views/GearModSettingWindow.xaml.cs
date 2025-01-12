using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using SevenDaysToDieModCreator.Controllers;
using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using SevenDaysToDieModCreator.Views;


namespace _7d2dModEdit.Views
{
    /// <summary>
    /// Interaction logic for GearModSettingWindow.xaml
    /// </summary>
    public partial class GearModSettingWindow : Window
    {
        private string GEARS_SETTINGS_FILE_NAME = "ModSettings.xml";
        private string GearsSettingsFilePath { get; set; }
        private string StartingTitle { get; set; }



        public GearModSettingWindow()
        {
            InitializeComponent();
            InitFindAndReplace();
            string modName = _7d2dModEdit.Properties.Settings.Default.ModTagSetting;
            this.GearsSettingsFilePath = XmlFileManager.Get_ModDirectoryOutputPath(modName);

            this.StartingTitle = modName + " " + GEARS_SETTINGS_FILE_NAME;

            this.Title = StartingTitle;

            this.XmlOutputBox.Text = XmlFileManager.GetFileContents(GearsSettingsFilePath, GEARS_SETTINGS_FILE_NAME);
            
            this.SaveXmlButton.AddToolTip("Click to save all changes");
            this.CloseButton.AddToolTip("Click here to close the window");
            this.ValidateXmlButton.AddToolTip("Click here to validate the xml");

            TextEditorOptions newOptions = new TextEditorOptions
            {
                EnableRectangularSelection = true,
                EnableTextDragDrop = true,
                HighlightCurrentLine = true,
                ShowTabs = true
            };
            this.XmlOutputBox.TextArea.Options = newOptions;

            this.XmlOutputBox.ShowLineNumbers = true;

            this.XmlOutputBox.PreviewMouseWheel += XmlOutputBox_PreviewMouseWheel;
            this.XmlOutputBox.TextChanged += XmlOutputBox_TextChanged;
            this.XmlOutputBox.Background = BackgroundColorController.GetBackgroundColor();
            this.XmlOutputBox.Focus();
            //Look into the events


            Closing += new CancelEventHandler(GearsModSettingWindow_Closing);
        }

        private void InitFindAndReplace()
        {
            FindReplace.FindReplaceMgr FRM = new FindReplace.FindReplaceMgr();
            FRM.CurrentEditor = new FindReplace.TextEditorAdapter(XmlOutputBox);
            FRM.ShowSearchIn = false;
            FRM.OwnerWindow = this;

            CommandBindings.Add(FRM.FindBinding);
            CommandBindings.Add(FRM.ReplaceBinding);
            CommandBindings.Add(FRM.FindNextBinding);
        }

        private void XmlOutputBox_TextChanged(object sender, EventArgs e)
        {
            string currentContents = XmlFileManager.GetFileContents(GearsSettingsFilePath, GEARS_SETTINGS_FILE_NAME);
            this.Title = XmlOutputBox.Text.Equals(currentContents)
                ? this.StartingTitle
                : "*" + this.StartingTitle;
        }

        private void XmlOutputBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control) return;
            //User rotated forward
            if (e.Delta > 0)
            {
                if (XmlOutputBox.FontSize != 200) XmlOutputBox.FontSize += 1;
            }
            //User rotated backwards
            else if (e.Delta < 0)
            {
                if (XmlOutputBox.FontSize != 10) XmlOutputBox.FontSize -= 1;
            }
        }

        private void SaveXmlButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void ValidateXmlButton_Click(object sender, RoutedEventArgs e)
        {
            XmlXpathGenerator.ValidateXml(XmlOutputBox.Text, doShowValidationMessage: true);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveFile()
        {
            string xmlOut = XmlOutputBox.Text;
            string isInvalid = XmlXpathGenerator.ValidateXml(xmlOut);
            if (isInvalid != null)
            {
                MessageBoxResult saveInvalidXmlDecision = MessageBox.Show(
                    "The xml is not valid! Would you like to save anyway?\n\n" + isInvalid,
                    "Invalid XML!",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Error);
                switch (saveInvalidXmlDecision)
                {
                    case MessageBoxResult.No:
                        return;
                }
            }
            if (!String.IsNullOrEmpty(xmlOut))
            {
                XmlFileManager.WriteStringToFile(GearsSettingsFilePath, GEARS_SETTINGS_FILE_NAME, xmlOut);
            }
        }

        private void GearsModSettingWindow_Closing(object sender, CancelEventArgs e)
        {
            if (IsFileChanged())
            {
                MessageBoxResult result = MessageBox.Show(
                    "You have unsaved changes! Would you like to save them now?",
                    "Save Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        string xmlOut = XmlOutputBox.Text;
                        if (!String.IsNullOrEmpty(xmlOut)) XmlFileManager.WriteStringToFile(GearsSettingsFilePath, GEARS_SETTINGS_FILE_NAME, xmlOut);
                        break;
                    case MessageBoxResult.Cancel:
                        GearModSettingWindow gearModSettingWindow = new GearModSettingWindow();
                        gearModSettingWindow.Show();
                        break;
                }
            }
        }
        private bool IsFileChanged()
        {
            string fileContents = XmlFileManager.GetFileContents(GearsSettingsFilePath, GEARS_SETTINGS_FILE_NAME);
            //                      Are there file contents                 do the fileContents equal what is in the XmlOutput
            bool isFileChanged = !String.IsNullOrEmpty(fileContents) && !fileContents.Equals(XmlOutputBox.Text);
            return isFileChanged;
        }
    }
}
