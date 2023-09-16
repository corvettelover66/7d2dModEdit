using SevenDaysToDieModCreator.Extensions;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace SevenDaysToDieModCreator.Models
{
    class ModInfo
    {
        private const string MOD_INFO_NAME_TAG = "Name";
        private const string MOD_INFO_DISPLAY_NAME_TAG = "DisplayName";
        private const string MOD_INFO_DESCRIPTION_TAG = "Description";
        private const string MOD_INFO_AUTHOR_TAG = "Author";
        private const string MOD_INFO_VERSION_TAG = "Version";
        private const string MOD_INFO_WEBSITE_TAG = "Website";

        public static string MOD_INFO_FILE_NAME = "ModInfo.xml";

        public bool ModInfoExists { get; private set; }
        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }
        public string Author { get; private set; }
        public string Version { get; private set; }
        public string Website { get; private set; }

        private bool UseLegacyFormat;

        public ModInfo(string modName, bool useLegacyFormat)
        {
            UseLegacyFormat = useLegacyFormat;
            DisplayName = modName;
            string modInfoFilePath = Path.Combine(XmlFileManager.Get_ModDirectoryOutputPath(modName), MOD_INFO_FILE_NAME);
            if (File.Exists(modInfoFilePath))
            {
                bool didSucceed = LoadSettingsFromFile(modInfoFilePath, ModInfoFileFormat(modName));
                ModInfoExists = didSucceed;
                if (!didSucceed)
                {
                    ModInfoExists = false;
                    XmlFileManager.WriteStringToLog("Failed Loading mod info.");
                }
            }
        }
        public ModInfo(bool useLegacyFormat, string name, string displayName, string description, string author, string version, string website)
        {
            UseLegacyFormat = useLegacyFormat;
            Name = name;
            DisplayName = displayName;
            Description = description;
            Author = author;
            Version = version;
            Website = website;
        }

        public enum MOD_INFO_FORMAT 
        {
            XML_LOADING_ERROR, 
            LEGACY_FORMAT,
            NEW_FORMAT, 
            MISSING_FILE
        }
        public static MOD_INFO_FORMAT ModInfoFileFormat(string modName) 
        {
            string modInfoFilePath = Path.Combine(XmlFileManager.Get_ModDirectoryOutputPath(modName), MOD_INFO_FILE_NAME);
            MOD_INFO_FORMAT modInfoFormat = MOD_INFO_FORMAT.MISSING_FILE;
            if (File.Exists(modInfoFilePath))
            {
                XmlDocument xmlDocument = new XmlDocument();
                try
                {
                    xmlDocument.Load(modInfoFilePath);
                    XmlNodeList allModInfoNodes = xmlDocument.GetElementsByTagName("ModInfo");
                    if (allModInfoNodes.Count > 0) modInfoFormat = MOD_INFO_FORMAT.LEGACY_FORMAT;
                    else 
                    {
                        allModInfoNodes = xmlDocument.GetElementsByTagName("xml");
                        if (allModInfoNodes.Count > 0) modInfoFormat = MOD_INFO_FORMAT.NEW_FORMAT;
                    }
                }
                catch (Exception e)
                {
                    XmlFileManager.WriteStringToLog(e.Message);
                    modInfoFormat = MOD_INFO_FORMAT.XML_LOADING_ERROR;
                }
            }

            return modInfoFormat;
        }

        public static void CreateModInfoFile(string modTagDirectoryName) 
        {
            string modDirectoryWithConfig = XmlFileManager.Get_ModDirectoryConfigPath(modTagDirectoryName);
            if (!Directory.Exists(modDirectoryWithConfig))
            {
                Directory.CreateDirectory(modDirectoryWithConfig);
            }
            string modInfoFilePath = Path.Combine(XmlFileManager.Get_ModDirectoryOutputPath(modTagDirectoryName), MOD_INFO_FILE_NAME);
            if (!File.Exists(modInfoFilePath))
            {
                File.Create(modInfoFilePath);
            }
        }

        public bool AllFieldsValid()
        {
            return ValidName(out _) && ValidDisplayName(out string _) && ValidVersion(out string _); ;
        }

        public bool AllFieldsValid(out string errorMessage) 
        {
            StringBuilder builder = new StringBuilder();

            bool valid = ValidName(out string nameErrorMessage);
            builder.AppendLine(nameErrorMessage);

            valid = ValidDisplayName(out string displapNameErrorMessage) && valid;
            builder.AppendLine(displapNameErrorMessage);

            valid = ValidVersion(out string versionErrorMessage) && valid;
            builder.AppendLine(versionErrorMessage);
            errorMessage = builder.ToString();
            return valid;
        }

        public bool ValidName(out string errorMessage)
        {
            errorMessage = string.Empty;
            if (Name.Length == 0)
            {
                errorMessage = "Name is required.";
                return false;
            }
            // This is the game regex ^[0-9a-zA-Z_\-]+$
            var nameRegex = new Regex(@"^[0-9a-zA-Z_\-]+$");

            if (!nameRegex.IsMatch(Name))
            {
                errorMessage = "Name can only contain numbers, latin letters, dashes and underscores with no spaces.";
                return false;
            }
            errorMessage.Trim();
            return true;
        }

        public bool ValidDisplayName(out string errorMessage)
        {
            if (string.IsNullOrEmpty(DisplayName.Trim()))
            {
                errorMessage = "Display Name is required.";
                return false;
            }

            errorMessage = "";
            return true;
        }

        public bool ValidVersion(out string errorMessage)
        {
            errorMessage= string.Empty;
            if (string.IsNullOrEmpty(Version.Trim()))
            {
                errorMessage = "Version is required.";
                return false;
            }
            var versionRegex = new Regex(@"^(?:(\d+))(?:\.(\d+))(?:\.(\d+))?(?:\.(\*|\d+))?$");

            if (!versionRegex.IsMatch(Version))
            {
                errorMessage = "Version must be in the format major.minor[.build[.revision]] with only numbers.";
                return false;
            }
            errorMessage.Trim();
            return true;
        }

        public bool LoadSettingsFromFile(string modInfoFilePath, MOD_INFO_FORMAT modInfoFileFormat) 
        {
            bool didSucceed = false;
            XmlDocument xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.Load(modInfoFilePath);
                XmlNodeList allModInfoNodes = null;
                if(modInfoFileFormat == MOD_INFO_FORMAT.LEGACY_FORMAT) allModInfoNodes = xmlDocument.GetElementsByTagName("ModInfo");
                else if(modInfoFileFormat == MOD_INFO_FORMAT.NEW_FORMAT) allModInfoNodes = xmlDocument.GetElementsByTagName("xml");
                
                if (allModInfoNodes == null) return didSucceed;

                XmlNode firstChild = allModInfoNodes.Item(0);
                if (firstChild != null) 
                {
                    foreach (XmlNode nextNode in firstChild.ChildNodes) 
                    {
                        if (nextNode.Name.Equals(MOD_INFO_NAME_TAG)) this.Name = nextNode.GetAvailableAttribute().Value;
                        else if (nextNode.Name.Equals(MOD_INFO_DISPLAY_NAME_TAG)) this.DisplayName = nextNode.GetAvailableAttribute().Value;
                        else if (nextNode.Name.Equals(MOD_INFO_DESCRIPTION_TAG)) this.Description = nextNode.GetAvailableAttribute().Value;
                        else if (nextNode.Name.Equals(MOD_INFO_AUTHOR_TAG)) this.Author = nextNode.GetAvailableAttribute().Value;
                        else if (nextNode.Name.Equals(MOD_INFO_VERSION_TAG)) this.Version = nextNode.GetAvailableAttribute().Value;
                        else if (nextNode.Name.Equals(MOD_INFO_WEBSITE_TAG)) this.Website = nextNode.GetAvailableAttribute().Value;
                    }
                    didSucceed = true;
                }
            }
            catch (Exception e)
            {
                XmlFileManager.WriteStringToLog(e.Message);
            }
            return didSucceed;
        }
        
        override public string ToString() 
        {
            string outputXml = UseLegacyFormat ? LegacyFormat() : VersionTwoFormat();
            return outputXml;
        }

        private string LegacyFormat() 
        {
            StringBuilder modInfoBuilder = new StringBuilder();
            modInfoBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            modInfoBuilder.AppendLine("<xml>");
            modInfoBuilder.AppendLine("\t<ModInfo>");
            modInfoBuilder.AppendLine("\t\t<Name value=\"" + this.Name + "\" />");
            modInfoBuilder.AppendLine("\t\t<Description value=\"" + this.Description + "\" />");
            modInfoBuilder.AppendLine("\t\t<Author value=\"" + this.Author + "\" />");
            modInfoBuilder.AppendLine("\t\t<Version value=\"" + this.Version + "\" />");
            modInfoBuilder.AppendLine("\t</ModInfo>");
            modInfoBuilder.AppendLine("</xml>");
            return modInfoBuilder.ToString();
        }

        private string VersionTwoFormat()
        {
            StringBuilder modInfoBuilder = new StringBuilder();
            modInfoBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            modInfoBuilder.AppendLine("<xml>");
            modInfoBuilder.AppendLine("\t<Name value=\"" + this.Name + "\" />");
            modInfoBuilder.AppendLine("\t<DisplayName value=\"" + this.DisplayName + "\" />");
            modInfoBuilder.AppendLine("\t<Version value=\"" + this.Version + "\" />");
            modInfoBuilder.AppendLine("\t<Description value=\"" + this.Description + "\" />");
            modInfoBuilder.AppendLine("\t<Author value=\"" + this.Author + "\" />");
            modInfoBuilder.AppendLine("\t<Website value=\"" + this.Website + "\" />");
            modInfoBuilder.AppendLine("</xml>");
            return modInfoBuilder.ToString();
        }
    }
}
