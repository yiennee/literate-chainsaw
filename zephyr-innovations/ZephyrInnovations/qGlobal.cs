using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Collections.ObjectModel;
using System.Windows;

namespace ZephyrInnovations
{
    public class qGlobal
    {
        private static LoggingCore _logger;
        private static LoggingFileText _loggingFileText;

        public static LoggingCore _VisionLogger;
        public static LoggingFileText _VisionLoggingFileText;

        public static LoggingCore Extra1Logger;
        public static LoggingFileText Extra1LoggerFileText;
        public static LoggingCore Extra2Logger;
        public static LoggingFileText Extra2LoggerFileText;
        public static LoggingCore Extra3Logger;
        public static LoggingFileText Extra3LoggerFileText;

        public static string basedir = AppDomain.CurrentDomain.BaseDirectory;
        public static string workingfolderpath = basedir.Replace(@"\Debug\", @"\WorkingFolder\").Replace(@"\Release\", @"\WorkingFolder\");

        public static string MainConfigurationFilename = "Main.Config";
        public static string MainConfigurationFileFoldername = "Configuration";

        public static string MainConfigurationFilePath = string.Format("{0}{1}{2}", workingfolderpath, MainConfigurationFileFoldername, "\\");
        public static string MainConfigurationFile = string.Format("{0}{1}", MainConfigurationFilePath, MainConfigurationFilename);

        public static ApplicationConfiguration AppConfig;
        public static ReportingConfiguration ReportConfig;
        public static ToolConfiguration ToolConfig;
        public static VisionProcessorConfiguration VisionProcessorConfig;
        public static GeoAlgoProcessorConfiguration GeoAlgoProcessorConfig;
        public static WaferMapConfiguration WaferMapConfig;

        public static ObservableCollection<VisionRecipeData> VisionRecipeDataList = new ObservableCollection<VisionRecipeData>();

        public static ObservableCollection<DefectCodeData> DefectCodeDataList = new ObservableCollection<DefectCodeData>();
        public static ObservableCollection<DefectCodeData> CompareDefectCodeDataList = new ObservableCollection<DefectCodeData>();

        public static List<DefectGroupData> DefectGroupDataList = new List<DefectGroupData>();

        static qGlobal()
        {
            _logger = LoggingManager.DefaultLogger;
            _loggingFileText = new LoggingFileText();

            _VisionLogger = LoggingManager_Vision.DefaultLogger;//yndebugtest 010423
            _VisionLoggingFileText = new LoggingFileText();

            AppConfig = new ApplicationConfiguration();
            ReportConfig = new ReportingConfiguration();
            ToolConfig = new ToolConfiguration();

            VisionProcessorConfig = new VisionProcessorConfiguration();
            GeoAlgoProcessorConfig = new GeoAlgoProcessorConfiguration();
            WaferMapConfig = new WaferMapConfiguration();
        }

        public static bool LoadDefectCodeSettingFile(out string errMsg)
        {
            errMsg = "";

            XMLUtils xmlUtils = new XMLUtils();
            DefectCodeSetting defectCodeSetting = new DefectCodeSetting();

            object myObj = xmlUtils.LoadXMLData(defectCodeSetting, GeoAlgoProcessorConfig.DefectCodeFileDirectory, "Defect Code", out errMsg);

            if (myObj == null)
            {
                WriteRunLog(true, "Load Defect Code Setting File: " + errMsg, "ERROR");
                return false;
            }
            else
            {
                DefectCodeDataList.Clear();
                CompareDefectCodeDataList.Clear();
                defectCodeSetting = (DefectCodeSetting)myObj;
            }

            for (int i = 0; i < defectCodeSetting.DEFECT_CODE_DATA_LIST.Count; i++)
            {
                DefectCodeData defectCodeData = (DefectCodeData)defectCodeSetting.DEFECT_CODE_DATA_LIST[i];
                DefectCodeDataList.Add(defectCodeData);
                CompareDefectCodeDataList.Add(defectCodeData);
            }

            return true;
        }

        public static bool LoadDefectGroupSettingFile(out string errMsg)
        {
            errMsg = "";

            XMLUtils xmlUtils = new XMLUtils();
            DefectGroupSetting defectGroupSetting = new DefectGroupSetting();

            object myObj = xmlUtils.LoadXMLData(defectGroupSetting, GeoAlgoProcessorConfig.DefectCodeFileDirectory, "Defect Group", out errMsg);

            if (myObj == null)
            {
                WriteRunLog(true, "Load Defect Group Setting File: " + errMsg, "ERROR");
                return false;
            }
            else
            {
                DefectGroupDataList.Clear();
                defectGroupSetting = (DefectGroupSetting)myObj;
            }

            for (int i = 0; i < defectGroupSetting.DEFECT_GROUP_DATA_LIST.Count; i++)
            {
                DefectGroupData defectGroupData = (DefectGroupData)defectGroupSetting.DEFECT_GROUP_DATA_LIST[i];
                DefectGroupDataList.Add(defectGroupData);
            }

            return true;
        }

        public static bool LoadVisionRecipeSettingFile(out string errMsg)
        {
            errMsg = "";

            XMLUtils xmlUtils = new XMLUtils();
            VisionRecipeSetting visionRecipeSetting = new VisionRecipeSetting();

            object myObj = xmlUtils.LoadXMLData(visionRecipeSetting, GeoAlgoProcessorConfig.DefectCodeFileDirectory, "Vision Recipe Setting", out errMsg);

            if (myObj == null)
            {
                WriteRunLog(true, "Load Vision Recipe Setting File: " + errMsg, "ERROR");
                return false;
            }
            else
            {
                VisionRecipeDataList.Clear();
                visionRecipeSetting = (VisionRecipeSetting)myObj;
            }

            for (int i = 0; i < visionRecipeSetting.RECIPE_DATA_LIST.Count; i++)
            {
                VisionRecipeData visionRecipeData = (VisionRecipeData)visionRecipeSetting.RECIPE_DATA_LIST[i];
                VisionRecipeDataList.Add(visionRecipeData);
            }

            return true;
        }

        public static void WriteRunLog(bool writeLog, string message, string messageType)
        {
            if (writeLog == false)
                return;

            string logFile = string.Format("{0}{1}", DateTime.Now.ToString("yyyy-MM-dd"), "_RunLog");//yndebugtest

            try
            {
                if (Directory.Exists(AppConfig.LogFileDirectory) == false)
                    Directory.CreateDirectory(AppConfig.LogFileDirectory);

                if (File.Exists(logFile + ".txt") == false)
                {
                    _loggingFileText.SaveDirectory(AppConfig.LogFileDirectory, logFile);
                    _logger.AttachFile(_loggingFileText);
                }

                _logger.WriteRunLog(message, messageType);
            }
            catch (Exception ex)
            {
                //qGlobal.WriteErrorLog("Write Run Log: " + ex.Message);
                //MessageBox.Show("Write Run Log: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void WriteVisionLog1(string message, string messageType)
        {
            string logFile = string.Format("{0}{1}", DateTime.Now.ToString("yyyy-MM-dd"), "_VisionLog");

            try
            {
                if (Directory.Exists(AppConfig.LogFileDirectory) == false)
                    Directory.CreateDirectory(AppConfig.LogFileDirectory);

                if (File.Exists(logFile + ".txt") == false)//yndebugtest
                {
                    _VisionLoggingFileText.SaveDirectory(AppConfig.LogFileDirectory, logFile);//Extra1LoggerFileText
                    _VisionLogger.AttachFile(_VisionLoggingFileText);//Extra1Logger
                }

                _VisionLogger.WriteVisionLog1(message, messageType);//Extra1Logger
            }
            catch (Exception ex)
            {
                //Share.Global.WriteErrorLog("Write Vision Log: " + ex.Message);
                MessageBox.Show("Write Vision Log: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void LoadMainConfigurationFile(string configurationFile)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(configurationFile);

            XmlNode configuration = xmlDoc.SelectSingleNode("configuration");

            XmlNodeList sectionList = configuration.ChildNodes;

            #region Section_Application
            int Section_Application_Counter = 2;
            int Section_Application_Found_Counter = 0;
            AppConfig.MachineID = "";
            AppConfig.LogFileDirectory = "";
            #endregion

            #region Section_Reporting
            int Section_Reporting_Counter = 1;
            int Section_Reporting_Found_Counter = 0;
            ReportConfig.ReportFileDirectory = "";
            #endregion

            #region Section_Tool
            int Section_Tool_Counter = 3;
            int Section_Tool_Found_Counter = 0;
            ToolConfig.EnableTCPIP = false;
            ToolConfig.TCPIPIP = "";
            ToolConfig.TCPIPPort = 0;
            #endregion

            #region Section_Vision_Processor
            int Section_Vision_Processor_Counter = 6;
            int Section_Vision_Processor_Found_Counter = 0;
            VisionProcessorConfig.RecipeFileDirectory = "";
            VisionProcessorConfig.ReadIMGFileDirectory = "";
            VisionProcessorConfig.ResultXMLFileDirectory = "";
            VisionProcessorConfig.EnableVidi = false;
            VisionProcessorConfig.EnableMultiThread = false;
            VisionProcessorConfig.MaxDegreeofParallelism = 5;
            #endregion

            #region Section_Geo_Algo_Processor
            int Section_Geo_Algo_Processor_Counter = 4;
            int Section_Geo_Algo_Processor_Found_Counter = 0;
            GeoAlgoProcessorConfig.DefectCodeFileDirectory = "";
            GeoAlgoProcessorConfig.UnitPassCode = "";
            GeoAlgoProcessorConfig.UnitPassCodeDesc = "";
            GeoAlgoProcessorConfig.UnitPassCodeColour = "";
            #endregion


            #region Section_Wafer_Map
            int Section_Wafer_Map_Counter = 4;
            int Section_Wafer_Map_Found_Counter = 0;
            WaferMapConfig.LoadMapFilesDirectory = "";
            WaferMapConfig.SaveMapFilesDirectory = "";
            //WaferMapConfig.InputMapType = "";
            //WaferMapConfig.OutputMapType = "";
            #endregion

            #region Section_Counter
            for (int y = 0; y < sectionList.Count; y++)
            {
                XmlNodeList configList = xmlDoc.SelectNodes("configuration/" + sectionList[y].Name + "/add");

                if (configList.Count != 0 && configList != null)
                {
                    for (int i = 0; i < configList.Count; i++)
                    {
                        XmlAttribute atrribKey = configList[i].Attributes["key"];
                        XmlAttribute attribValue = configList[i].Attributes["value"];
                        XmlAttribute attribDescription = configList[i].Attributes["description"];

                        #region Section_Application
                        if (sectionList[y].Name.ToString() == "Application")
                        {
                            switch (atrribKey.Value.ToString())
                            {
                                case "Machine ID":
                                    AppConfig.MachineID = attribValue.Value.ToString();
                                    Section_Application_Found_Counter++;
                                    break;

                                case "Log File Directory":
                                    AppConfig.LogFileDirectory = attribValue.Value.ToString();
                                    Section_Application_Found_Counter++;
                                    break;
                            }
                        }
                        #endregion

                        #region Section_Reporting
                        if (sectionList[y].Name.ToString() == "Reporting")
                        {
                            switch (atrribKey.Value.ToString())
                            {
                                case "Report File Directory":
                                    ReportConfig.ReportFileDirectory = attribValue.Value.ToString();
                                    Section_Reporting_Found_Counter++;
                                    break;
                            }
                        }
                        #endregion

                        #region Section_Tool
                        if (sectionList[y].Name.ToString() == "Tool")
                        {
                            switch (atrribKey.Value.ToString())
                            {
                                case "Enable TCPIP":
                                    ToolConfig.EnableTCPIP = Convert.ToBoolean(attribValue.Value.ToString());
                                    Section_Tool_Found_Counter++;
                                    break;

                                case "TCPIP IP":
                                    ToolConfig.TCPIPIP = attribValue.Value.ToString();
                                    Section_Tool_Found_Counter++;
                                    break;

                                case "TCPIP Port":
                                    ToolConfig.TCPIPPort = Convert.ToInt32(attribValue.Value.ToString());
                                    Section_Tool_Found_Counter++;
                                    break;
                            }
                        }
                        #endregion

                        #region Section_Vision_Processor
                        if (sectionList[y].Name.ToString() == "Vision_Processor")
                        {
                            switch (atrribKey.Value.ToString())
                            {
                                case "Recipe File Directory":
                                    VisionProcessorConfig.RecipeFileDirectory = attribValue.Value.ToString();
                                    Section_Vision_Processor_Found_Counter++;
                                    break;

                                case "Read IMG File Directory":
                                    VisionProcessorConfig.ReadIMGFileDirectory = attribValue.Value.ToString();
                                    Section_Vision_Processor_Found_Counter++;
                                    break;

                                case "Result XML File Directory":
                                    VisionProcessorConfig.ResultXMLFileDirectory = attribValue.Value.ToString();
                                    Section_Vision_Processor_Found_Counter++;
                                    break;

                                case "Enable Vidi":
                                    VisionProcessorConfig.EnableVidi = Convert.ToBoolean(attribValue.Value.ToString());
                                    Section_Vision_Processor_Found_Counter++;
                                    break;

                                case "Enable Multi-Thread Processing":
                                    VisionProcessorConfig.EnableMultiThread = Convert.ToBoolean(attribValue.Value.ToString());
                                    Section_Vision_Processor_Found_Counter++;
                                    break;

                                case "Max Degree of Parallelism":
                                    VisionProcessorConfig.MaxDegreeofParallelism = Convert.ToInt32(attribValue.Value.ToString());
                                    Section_Vision_Processor_Found_Counter++;
                                    break;
                            }
                        }
                        #endregion

                        #region Section_Geo_Algo_Processor
                        if (sectionList[y].Name.ToString() == "Geo_Algo_Processor")
                        {
                            switch (atrribKey.Value.ToString())
                            {
                                case "Defect Code File Directory":
                                    GeoAlgoProcessorConfig.DefectCodeFileDirectory = attribValue.Value.ToString();
                                    Section_Geo_Algo_Processor_Found_Counter++;
                                    break;

                                case "Unit Pass Code":
                                    GeoAlgoProcessorConfig.UnitPassCode = attribValue.Value.ToString();
                                    Section_Geo_Algo_Processor_Found_Counter++;
                                    break;

                                case "Unit Pass Code Desc":
                                    GeoAlgoProcessorConfig.UnitPassCodeDesc = attribValue.Value.ToString();
                                    Section_Geo_Algo_Processor_Found_Counter++;
                                    break;

                                case "Unit Pass Code Colour":
                                    GeoAlgoProcessorConfig.UnitPassCodeColour = attribValue.Value.ToString();
                                    Section_Geo_Algo_Processor_Found_Counter++;
                                    break;
                            }
                        }
                        #endregion

                        #region Wafer Map
                        if (sectionList[y].Name.ToString() == "Wafer_Map")
                        {
                            switch (atrribKey.Value.ToString())
                            {
                                case "Load Map File Directory":
                                    WaferMapConfig.LoadMapFilesDirectory = attribValue.Value.ToString();
                                    Section_Wafer_Map_Found_Counter++;
                                    break;

                                case "Save Output Map File Directory":
                                    WaferMapConfig.SaveMapFilesDirectory = attribValue.Value.ToString();
                                    Section_Wafer_Map_Found_Counter++;
                                    break;

                                case "Input Map Type":
                                    WaferMapConfig.InputMapType = (MapType)Enum.Parse(typeof(MapType), attribValue.Value.ToString(), true);//enum.Parse(typeof(WaferMapConfig.InputMapType)attribValue.Value.ToString());
                                    Section_Wafer_Map_Found_Counter++;
                                    break;

                                case "Output Map Type":
                                    MapType _MapType = (MapType)Enum.Parse(typeof(MapType), attribValue.Value.ToString(), true);
                                    WaferMapConfig.OutputMapType = _MapType;// Enum.Parse(typeof(_MapType), attribValue.Value.ToString());
                                    Section_Wafer_Map_Found_Counter++;
                                    break;
                            }
                        }
                        #endregion
                    }
                }
            }
            #endregion
        }

        public enum MapType
        {
            XMLMap_SemiG85,
            Klarf1_2
        }
    }
}
