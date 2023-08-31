using SevenDaysToDieModCreator.Controllers;
using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace SevenDaysToDieModCreator.Views
{
    public partial class ModInfoDialogBox : Window
    {
        private ComboBox AllTagsComboBox { get; set; }

        public ModInfoDialogBox(string textBoxBody = "", string windowTitle = "")
        {
            InitializeComponent();
            this.CurrentSelectedModTextBox.Text = _7d2dModEdit.Properties.Settings.Default.ModTagSetting;
            this.Title = windowTitle ?? "";
            ModInfoXmlPreviewAvalonEditor.Text = "\n\n" + textBoxBody;

            ResetModNameComboBoxes(_7d2dModEdit.Properties.Settings.Default.ModTagSetting);

            SetupLegacyFormatSetting();
            SetTextBoxEvents();
            SetTooltips();
            SetTextBoxsWithExistingModInfo();
            SetBackgroundColor();
            Closing += new CancelEventHandler(ModInfoDialogBox_Closing);
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            CheckForLegacyFormat();
        }

        private void SetupLegacyFormatSetting()
        {
            UseLegacyFormatCheckBox.Checked += UseLegacyFormatCheckBox_Checked;
            UseLegacyFormatCheckBox.Unchecked += UseLegacyFormatCheckBox_Unchecked;
            bool useLegacyFormatSetting = _7d2dModEdit.Properties.Settings.Default.IsUseLegacyFormatCheckBox;
            UseLegacyFormatCheckBox.IsChecked = useLegacyFormatSetting;
            if (useLegacyFormatSetting)
            {
                CustomDialogStackPanel.Children.Remove(ModInfoDisplayNameBox);
                CustomDialogStackPanel.Children.Remove(ModInfoWebsiteBox);
            }
        }

        private void UseLegacyFormatCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CustomDialogStackPanel.Children.Remove(ModInfoDisplayNameBox);
            CustomDialogStackPanel.Children.Remove(ModInfoWebsiteBox);
            ModInfoXmlPreviewAvalonEditor.Text = getNewModInfoFromTextBoxes().ToString();
            _7d2dModEdit.Properties.Settings.Default.IsUseLegacyFormatCheckBox = true;
            ModInfoVersionBox.AddToolTip(VersionBoxStandardToolTip);
            ModInfoVersionBox.ClearValue(TextBox.BorderBrushProperty);
            ModInfoNameBox.AddToolTip(NameBoxStandardToolTip);
            ModInfoNameBox.ClearValue(TextBox.BorderBrushProperty);
        }

        private void UseLegacyFormatCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CustomDialogStackPanel.Children.Insert(1,ModInfoDisplayNameBox);
            CustomDialogStackPanel.Children.Insert(5, ModInfoWebsiteBox);
            ModInfoXmlPreviewAvalonEditor.Text = getNewModInfoFromTextBoxes().ToString();
            _7d2dModEdit.Properties.Settings.Default.IsUseLegacyFormatCheckBox = false;
        }

        private void ResetModNameComboBoxes(string modNameToSetBox)
        {
            if (FirstRowStackPanel.Children.Contains(this.AllTagsComboBox)) FirstRowStackPanel.Children.Remove(this.AllTagsComboBox);
            List<string> allCustomModsInPath = XmlFileManager.GetCustomModFoldersInOutput();
            this.ChangeNameAllTagsComboBox.SetComboBox(allCustomModsInPath);
            this.AllTagsComboBox = allCustomModsInPath.CreateComboBoxFromList(isEditable: false);
            this.AllTagsComboBox.DropDownClosed += AllTagsComboBox_DropDownClosed;
            this.AllTagsComboBox.SelectionChanged += AllTagsComboBox_SelectionChanged;
            this.AllTagsComboBox.FontSize = 22;
            this.AllTagsComboBox.SelectedItem = modNameToSetBox;
            FirstRowStackPanel.Children.Add(this.AllTagsComboBox);
        }

        private void ModInfoDialogBox_Closing(object sender, CancelEventArgs e)
        {
            this.DialogResult = true;
        }
        private void SetBackgroundColor()
        {
            this.Background = BackgroundColorController.GetBackgroundColor();

            AllTagsComboBox.Background = BackgroundColorController.GetBackgroundColor();
            AllTagsComboBox.Resources.Add(System.Windows.SystemColors.WindowBrushKey, BackgroundColorController.GetBackgroundColor());
            ChangeNameAllTagsComboBox.Background = BackgroundColorController.GetBackgroundColor();
            ChangeNameAllTagsComboBox.Resources.Add(System.Windows.SystemColors.WindowBrushKey, BackgroundColorController.GetBackgroundColor());

            CurrentSelectedModTextBox.Background = BackgroundColorController.GetBackgroundColor();

            ModInfoNameBox.Background = BackgroundColorController.GetBackgroundColor();
            ModInfoDescriptionBox.Background = BackgroundColorController.GetBackgroundColor();
            ModInfoAuthorBox.Background = BackgroundColorController.GetBackgroundColor();
            ModInfoVersionBox.Background = BackgroundColorController.GetBackgroundColor();
            ModInfoDisplayNameBox.Background = BackgroundColorController.GetBackgroundColor();
            ModInfoWebsiteBox.Background = BackgroundColorController.GetBackgroundColor();

            ModInfoXmlPreviewAvalonEditor.Background = BackgroundColorController.GetBackgroundColor();
        }
        private void SetTextBoxEvents() 
        {
            ModInfoNameBox.TextChanged += ModInfoBox_TextChanged;
            ModInfoNameBox.LostFocus += ModInfoNameBox_LostFocus;

            ModInfoDisplayNameBox.TextChanged += ModInfoBox_TextChanged;
            ModInfoDisplayNameBox.LostFocus += ModInfoDisplayNameBox_LostFocus;

            ModInfoVersionBox.TextChanged += ModInfoBox_TextChanged;
            ModInfoVersionBox.LostFocus += ModInfoVersionBox_LostFocus;
            ModInfoDescriptionBox.TextChanged += ModInfoBox_TextChanged;
            ModInfoAuthorBox.TextChanged += ModInfoBox_TextChanged;
            ModInfoWebsiteBox.TextChanged += ModInfoBox_TextChanged;
        }

        private string VersionBoxStandardToolTip = ("Version (Required)\nSemVer version of the mod. Has to be in the format major.minor[.build[.revision]]\n(i.e.build and revision can be left out, recommend using them though as typically done with Semantic Versioning");

        private void ModInfoVersionBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ModInfo modInfo = getNewModInfoFromTextBoxes();
            string errorMessage;
            if (modInfo.ValidVersion(out errorMessage))
            {
                ModInfoVersionBox.AddToolTip(VersionBoxStandardToolTip);
                ModInfoVersionBox.ClearValue(TextBox.BorderBrushProperty);
            }
            else
            {
                ModInfoVersionBox.AddToolTip(errorMessage);
                ModInfoVersionBox.BorderBrush = Brushes.Red;
            }
        }

        private string DisplayNameBoxStandardToolTip = "Display Name (Required)\nName used for display purposes, like shown in the mods UI at some point.\nCould be the same as you would later on use on workshop or wherever mods get distributed.";
        private void ModInfoDisplayNameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ModInfo modInfo = getNewModInfoFromTextBoxes();
            string errorMessage;
            if (modInfo.ValidDisplayName(out errorMessage))
            {
                ModInfoDisplayNameBox.AddToolTip(DisplayNameBoxStandardToolTip);
                ModInfoDisplayNameBox.ClearValue(TextBox.BorderBrushProperty);
            }
            else 
            {
                ModInfoDisplayNameBox.AddToolTip(errorMessage);
                ModInfoDisplayNameBox.BorderBrush = Brushes.Red;
            }
        }
        private string NameBoxStandardToolTip = "Name (Required)\nInternal name, like an ID, of the mod.Should be globally unique,\nlike an author prefix + name.Only allowed chars: Numbers, latin letters, underscores, dash";
        private void ModInfoNameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (UseLegacyFormatCheckBox.IsChecked.Value) return;

            ModInfo modInfo = getNewModInfoFromTextBoxes();
            string errorMessage;
            if (modInfo.ValidName(out errorMessage))
            {
                ModInfoNameBox.AddToolTip(NameBoxStandardToolTip);
                ModInfoNameBox.ClearValue(TextBox.BorderBrushProperty);
            }
            else
            {
                ModInfoNameBox.AddToolTip(errorMessage);
                ModInfoNameBox.BorderBrush = Brushes.Red;
            }
        }

        private void SetTooltips()
        {
            ModInfoNameBox.AddToolTip(NameBoxStandardToolTip);
            ModInfoDisplayNameBox.AddToolTip(DisplayNameBoxStandardToolTip);
            ModInfoVersionBox.AddToolTip(VersionBoxStandardToolTip);
            ModInfoDescriptionBox.AddToolTip("Description (Optional)\nMore text to show on UIs.");
            ModInfoAuthorBox.AddToolTip("Author (Optional)\nName(s) of the author(s).");
            ModInfoWebsiteBox.AddToolTip("Website (Optional)\nURL of a website of the mod");

            AllTagsComboBox.AddToolTip("This is the selected mod. This is also used as the directory name and tag for the mod.\nThe mod selected here is what will be updated on save.");
            ChangeNameAllTagsComboBox.AddToolTip("Any values placed here can be used to either create a new mod or change the selected mod by clicking the appropriate button.");
            
            ModInfoXmlPreviewAvalonEditor.AddToolTip("This is just a preview of the ModInfo.xml file, direct changes here cannot be saved.");
            UseLegacyFormatCheckBox.AddToolTip("Check this to use the legacy V1 ModInfo.xml format.");
            ChangeModNameButton.AddToolTip("Click here to change the name of the selected mod using the value provided in the box just to the left.\n\nThis will also automatically replace old top tags with the new one. ");
            OkButton.AddToolTip("Click here to save all changes to the current selected mod or a new mod");
           
            UpdateAllModInfoMenuItem.AddToolTip("Click here to update all Mods' ModInfo.xml from the legacy format to the V2 format.");
        }
        private void CheckForLegacyFormat()
        {
            if (UseLegacyFormatCheckBox.IsChecked.Value) return;
            string modName = this.AllTagsComboBox.Text;
            if (ModInfo.ModInfoFileFormat(modName) == ModInfo.MOD_INFO_FORMAT.LEGACY_FORMAT) 
            {
                string message = "The current selected ModInfo.xml is in the legacy format. Update the required fields and click Save.\nAlternatively, you can update the ModInfo.xml for all mods loaded by clicking Tools -> Update All Mod Info V2";
                MessageBox.Show(message, "Legacy Format", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            ModInfo modInfo = getNewModInfoFromTextBoxes();

            if (!modInfo.AllFieldsValid())
            {
                CheckValidationFields(modInfo);
                string invalidFieldsError = "Some fields are not valid for V2!\n\nIt is recommended that you fix any validation errors for the ModInfo now.\n\nHover the field highlighted red for more information on the error.";
                MessageBox.Show(invalidFieldsError, "Mod Info Fields Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CheckValidationFields(ModInfo modInfo) 
        {
            string errorMessage;
            if (modInfo.ValidName(out errorMessage))
            {
                ModInfoNameBox.AddToolTip(NameBoxStandardToolTip);
                ModInfoNameBox.ClearValue(TextBox.BorderBrushProperty);
            }
            else
            {
                ModInfoNameBox.AddToolTip(errorMessage);
                ModInfoNameBox.BorderBrush = Brushes.Red;
            }
            if (modInfo.ValidDisplayName(out errorMessage)) 
            {
                ModInfoDisplayNameBox.AddToolTip(DisplayNameBoxStandardToolTip);
                ModInfoDisplayNameBox.ClearValue(TextBox.BorderBrushProperty);
            }
            else
            {
                ModInfoDisplayNameBox.AddToolTip(errorMessage);
                ModInfoDisplayNameBox.BorderBrush = Brushes.Red;
            }
            if (modInfo.ValidVersion(out errorMessage)) 
            {
                ModInfoVersionBox.AddToolTip(VersionBoxStandardToolTip);
                ModInfoVersionBox.ClearValue(TextBox.BorderBrushProperty);
            }
            else
            {
                ModInfoVersionBox.AddToolTip(errorMessage);
                ModInfoVersionBox.BorderBrush = Brushes.Red;
            }
        }

        private void AllTagsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ModSelectionChanged();
            ModInfoXmlPreviewAvalonEditor.Text = getNewModInfoFromTextBoxes().ToString();
        }
        private void AllTagsComboBox_DropDownClosed(object sender, EventArgs e)
        {
            ModSelectionChanged();
            ModInfoXmlPreviewAvalonEditor.Text = getNewModInfoFromTextBoxes().ToString();
            CheckForLegacyFormat();
        }
        private void ModInfoBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ModInfoXmlPreviewAvalonEditor.Text = getNewModInfoFromTextBoxes().ToString();
        }

        private ModInfo getNewModInfoFromTextBoxes() 
        {
            return new ModInfo(UseLegacyFormatCheckBox.IsChecked.Value, ModInfoNameBox.Text, ModInfoDisplayNameBox.Text, ModInfoDescriptionBox.Text, ModInfoAuthorBox.Text, ModInfoVersionBox.Text, ModInfoWebsiteBox.Text);
        }

        private void ModSelectionChanged()
        {
            if (!String.IsNullOrEmpty(AllTagsComboBox.Text.Trim()))
            {
                this.CurrentSelectedModTextBox.Text = AllTagsComboBox.Text;
            }
            SetTextBoxsWithExistingModInfo();
        }

        private void SetTextBoxsWithExistingModInfo()
        {
            ModInfo currentModInfo = new ModInfo(this.AllTagsComboBox.Text, UseLegacyFormatCheckBox.IsChecked.Value);
            if (currentModInfo.ModInfoExists)
            {
                ModInfoNameBox.Text = currentModInfo.Name;
                ModInfoDisplayNameBox.Text = currentModInfo.DisplayName;
                ModInfoDescriptionBox.Text = currentModInfo.Description;
                ModInfoAuthorBox.Text = currentModInfo.Author;
                ModInfoVersionBox.Text = currentModInfo.Version;
                ModInfoWebsiteBox.Text = currentModInfo.Website;
            }
            else 
            {
                ModInfoNameBox.Text = "";
                ModInfoDisplayNameBox.Text = "";
                ModInfoDescriptionBox.Text = "";
                ModInfoAuthorBox.Text = "";
                ModInfoVersionBox.Text = "";
                ModInfoWebsiteBox.Text = "";
            }
        }
        private static bool VerifyTagNameCorrectness(string nameToVerify, bool showMessageBox = true) 
        {
            bool isTagCorrect = true;
            try
            {
                XmlConvert.VerifyName(nameToVerify);
            }
            catch (XmlException)
            {
                if(showMessageBox)MessageBox.Show("The Mod Tag format was incorrect, the value must follow xml tag naming rules!\n" +
                    "Typical errors are spaces in the name, or unusable special characters.\n\n" +
                    "You must correct this mistake before saving the name.",
                    "Format Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                isTagCorrect = false;
            }
            catch (ArgumentNullException)
            {
                if (showMessageBox)MessageBox.Show("The Mod Tag format was incorrect, the Mod Tag cannot be empty! Please select or add a Mod Tag now.\n\n" +
                    "You must correct this mistake before saving the name.",
                    "Format Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                isTagCorrect = false;
            }
            return isTagCorrect;
        }
        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //Check if the name box is empty
            if (String.IsNullOrEmpty(ChangeNameAllTagsComboBox.Text)) 
            {
                string currentSelectedModTag = AllTagsComboBox.Text;
                SaveModInfo(currentSelectedModTag);
            }
            else 
            {
                //prompt user to create new mod with text provided
                string message = "The selected mod name provided does not exist, would you like to create it as a new mod now?";
                MessageBoxResult results = MessageBox.Show(message, "Create New Mod", MessageBoxButton.YesNo, MessageBoxImage.Question);
                switch (results)
                {
                    case MessageBoxResult.Yes:
                        List<string> allCustomModsInPath = XmlFileManager.GetCustomModFoldersInOutput();
                        //If the new name is not in the output path
                        if (!allCustomModsInPath.Contains(ChangeNameAllTagsComboBox.Text)) 
                        {
                            SaveModInfo(ChangeNameAllTagsComboBox.Text);
                        }
                        else 
                        {
                            string modExistsMessage = "The new mod name cannot already exist in the output location.\n\n" +
                                "You must use different name or delete the other mod folder in the output location.";
                            MessageBox.Show(modExistsMessage, "Mod Name Exists", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        break;

                }
                ChangeNameAllTagsComboBox.Text = "";
            }
        }

        private void SaveModInfo(string modNameToUse)
        {
            ModInfo newModIfo = getNewModInfoFromTextBoxes();
            if (!UseLegacyFormatCheckBox.IsChecked.Value && !newModIfo.AllFieldsValid(out _)) 
            {
                string invalidFieldsError = "Some fields are not valid!\n\nHover the field highlighted red for more information on the error.";
                MessageBox.Show(invalidFieldsError, "Saving Mod Info Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            ModInfo.CreateModInfoFile(modNameToUse);
            string modInfoXmlOut = newModIfo.ToString();
            bool didSucceed = XmlFileManager.WriteStringToFile(XmlFileManager.Get_ModDirectoryOutputPath(modNameToUse), ModInfo.MOD_INFO_FILE_NAME, modInfoXmlOut);
            if (didSucceed)
            {
                MessageBox.Show("Saved mod info for " + modNameToUse + ".", "Saving Mod info", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetModNameComboBoxes(modNameToUse);
                SetTextBoxsWithExistingModInfo();
                ModInfo newModInfo = getNewModInfoFromTextBoxes();
                ModInfoXmlPreviewAvalonEditor.Text = newModInfo.ToString();
            }
            else 
            {
                MessageBox.Show("Created new mod, with empty. Simply reselecting the mod in the combo box above may fix the issue. Alternatively, select another mod and reselect the mod you would like to edit.", 
                    "Save Mod Info Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                ResetModNameComboBoxes(modNameToUse);
                SetTextBoxsWithExistingModInfo();
                ModInfo newModInfo = getNewModInfoFromTextBoxes();
                ModInfoXmlPreviewAvalonEditor.Text = newModInfo.ToString();
            }
        }

        private void ChangeModTagName(string oldModtagName, string newTagName)
        {
            XmlFileManager.RenameModDirectory(oldModtagName, newTagName);
            bool hasConfigTags = XmlFileManager.ReplaceTagsInModFiles(oldModtagName, newTagName);
            if (hasConfigTags) 
            {
                string message = "Your mod files uses a config tag as the top tag, would you like to change them in each file to the new mod name?";
                string title = "Change Top Tag in All Mod Files";
                MessageBoxResult result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                switch (result) 
                {
                    case MessageBoxResult.Yes:
                        XmlFileManager.ReplaceTagsInModFiles(oldModtagName, newTagName, true);
                        break;
                }
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void UpdateAllModInfoMenuItem_Click(object sender, RoutedEventArgs e) 
        {
            //TODO: Add prompt
            string message = "Update all V1 ModInfos to the new V2 format?\n\n" +
                              "This will auto fill the required display name with the Mod's folder name and also run validation against the new V2 requirments.";
            MessageBox.Show(message, "Update ModInfo V2", MessageBoxButton.OK, MessageBoxImage.Question);
            StringBuilder errorsBuilder = new StringBuilder();
            string spacer = "=====================";
            errorsBuilder.AppendLine(spacer);
            errorsBuilder.AppendLine();
            foreach (var item in AllTagsComboBox.Items)
            {
                string nextMod = item.ToString();
                if (string.IsNullOrEmpty(nextMod.Trim())) continue;
                ModInfo.CreateModInfoFile(nextMod);
                ModInfo v2ModInfo = new ModInfo(nextMod, false);
                if (v2ModInfo.ModInfoExists)
                {
                    if(!v2ModInfo.AllFieldsValid(out string errorMessage)) errorsBuilder.AppendLine(nextMod);                    
                    XmlFileManager.WriteStringToFile(XmlFileManager.Get_ModDirectoryOutputPath(nextMod), ModInfo.MOD_INFO_FILE_NAME, v2ModInfo.ToString());
                }
            }
            string allErrors = errorsBuilder.ToString();
            string successMessage = "Successfully updated all of the ModInfo.xml files.\n" +
                "Below you will find any files with validation errors. It is recommened you fix them now.\n\n" +
                "Mods with invalid files:\n" + allErrors;
            MessageBox.Show(successMessage, "Update ModInfo V2 ", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ChangeModTagButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> allCustomModsInPath = XmlFileManager.GetCustomModFoldersInOutput();
            string currentSelectedModTag = AllTagsComboBox.Text;
            string newModName  = ChangeNameAllTagsComboBox.Text;
            //if the current selected mod name is not the last selected
            if (!currentSelectedModTag.Equals(newModName))
            {
                // and is does not exist in the mod output folder already
                if (!allCustomModsInPath.Contains(newModName))
                {
                    if (VerifyTagNameCorrectness(newModName))
                    {
                        try
                        {
                            //We can change the name
                            ChangeModTagName(currentSelectedModTag, newModName);
                            ResetModNameComboBoxes(currentSelectedModTag);
                            string message = "Successfully changed the mod name from " + currentSelectedModTag + " to " + newModName + ".";
                            MessageBox.Show(message, "Change Mod Name", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception exception)
                        {
                            XmlFileManager.WriteStringToLog("ERROR changing mod name. Exception:\n " + exception.ToString() + " \nMessage: " + exception.Message);
                            string message = "Error attempting to change the name for the mod " + currentSelectedModTag + "."
                                + "\nError:\n\n" +
                                exception.Message;
                            MessageBox.Show(message, "Change Mod Name Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                //the name already exists
                else 
                {
                    string message = "The new mod name cannot already exist in the output location.\n\n" +
                        "You must use different name or delete the other mod folder in the output location.";
                    MessageBox.Show(message, "Mod Name Exists", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
            //The name is the same as the selected mod
            else 
            {
                MessageBox.Show("The selected mod, and the new mod name must be different", "Mod Name Unchanged", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}
