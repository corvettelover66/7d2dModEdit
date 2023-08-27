using SevenDaysToDieModCreator.Controllers;
using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.Design.AxImporter;
using System.Xml.Linq;

namespace SevenDaysToDieModCreator.Views
{
    public partial class ModInfoDialogBox : Window
    {
        private ComboBox AllTagsComboBox { get; set; }

        public ModInfoDialogBox(string textBoxBody = "", string windowTitle = "")
        {
            InitializeComponent();
            this.CurrentSelectedModTextBox.Text = Properties.Settings.Default.ModTagSetting;
            this.Title = windowTitle ?? "";

            ModInfoXmlPreviewAvalonEditor.Text = "\n\n" + textBoxBody;

            ResetModNameComboBoxes(Properties.Settings.Default.ModTagSetting);


            SetTextBoxTextChanged();
            SetTooltips();
            SetTextBoxsWithExistingModInfo();
            SetBackgroundColor();
            CheckForLegacyFormat(this.AllTagsComboBox.Text);
            Closing += new CancelEventHandler(ModInfoDialogBox_Closing);
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
        private void SetTextBoxTextChanged() 
        {
            ModInfoNameBox.TextChanged += ModInfoBox_TextChanged;
            ModInfoDisplayNameBox.TextChanged += ModInfoBox_TextChanged;
            ModInfoVersionBox.TextChanged += ModInfoBox_TextChanged;
            ModInfoDescriptionBox.TextChanged += ModInfoBox_TextChanged;
            ModInfoAuthorBox.TextChanged += ModInfoBox_TextChanged;
            ModInfoWebsiteBox.TextChanged += ModInfoBox_TextChanged;
        }

        private void SetTooltips()
        {
            ModInfoNameBox.AddToolTip("Name\n(Required)Internal name, like an ID, of the mod.Should be globally unique,\nlike an author prefix + name.Only allowed chars: Numbers, latin letters, underscores, dash");
            ModInfoDisplayNameBox.AddToolTip("Display Name\n(Required)Name used for display purposes, like shown in the mods UI at some point.\nCould be the same as you would later on use on workshop or wherever mods get distributed.");
            ModInfoVersionBox.AddToolTip("Version\n(Required)SemVer version of the mod. Has to be in the format major.minor[.build[.revision]]\n(i.e.build and revision can be left out, recommend using them though as typically done with Semantic Versioning)");
            ModInfoDescriptionBox.AddToolTip("Description\n(Optional)More text to show on UIs.");
            ModInfoAuthorBox.AddToolTip("Author\n(Optional)Name(s) of the author(s).");
            ModInfoWebsiteBox.AddToolTip("Website\n(Optional)URL of a website of the mod");

            AllTagsComboBox.AddToolTip("This is the selected mod. This is also used as the directory name and tag for the mod.\nThe mod selected here is what will be updated on save.");
            ChangeNameAllTagsComboBox.AddToolTip("Any values placed here can be used to either create a new mod or change the selected mod by clicking the appropriate button.");
            
            ModInfoXmlPreviewAvalonEditor.AddToolTip("This is just a preview of the ModInfo.xml file, direct changes here cannot be saved.");
            
            ChangeModNameButton.AddToolTip("Click here to change the name of the selected mod using the value provided in the box just to the left.\n\nThis will also automatically replace old top tags with the new one. ");
            OkButton.AddToolTip("Click here to save all changes to the current selected mod or a new mod");
           
            UpdateAllModInfoMenuItem.AddToolTip("Click here to update all Mods' ModInfo.xml from the Legacy format.");
        }
        private void CheckForLegacyFormat(string modName) 
        {
            if(ModInfo.ModInfoFileFormat(modName) == ModInfo.MOD_INFO_FORMAT.LEGACY_FORMAT) 
            {
                string message = "The current selected ModInfo.xml is in the legacy format. Update using the fields below and click Save.\nAlternatively, you can update the ModInfo.xml for all mods loaded by clicking Tools -> Update All Mod Info";
                MessageBox.Show(message, "Legacy Format", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AllTagsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ModSelectionChanged();
            ModInfoXmlPreviewAvalonEditor.Text = getNewModInfoFromTextBoxes().ToString();
            CheckForLegacyFormat(this.AllTagsComboBox.Text);
        }
        private void AllTagsComboBox_DropDownClosed(object sender, EventArgs e)
        {
            ModSelectionChanged();
            ModInfoXmlPreviewAvalonEditor.Text = getNewModInfoFromTextBoxes().ToString();
            CheckForLegacyFormat(this.AllTagsComboBox.Text);
        }
        private void ModInfoBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ModInfoXmlPreviewAvalonEditor.Text = getNewModInfoFromTextBoxes().ToString();
        }

        private ModInfo getNewModInfoFromTextBoxes() 
        {
            return new ModInfo(ModInfoNameBox.Text, ModInfoDisplayNameBox.Text, ModInfoDescriptionBox.Text, ModInfoAuthorBox.Text, ModInfoVersionBox.Text, ModInfoWebsiteBox.Text);
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
            ModInfo currentModInfo = new ModInfo(this.AllTagsComboBox.Text);
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
        private bool VerifyTagNameCorrectness(string nameToVerify, bool showMessageBox = true) 
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
            ModInfo.CreateModInfoFile(modNameToUse);
            ModInfo newModIfo = getNewModInfoFromTextBoxes();
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
            foreach (var item in AllTagsComboBox.Items)
            {
                string nextMod = item.ToString();
                if (string.IsNullOrEmpty(nextMod.Trim())) continue;
                ModInfo.CreateModInfoFile(nextMod);
                ModInfo newModIfo = new ModInfo(nextMod);
                XmlFileManager.WriteStringToFile(XmlFileManager.Get_ModDirectoryOutputPath(nextMod), ModInfo.MOD_INFO_FILE_NAME, newModIfo.ToString());
            }
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
