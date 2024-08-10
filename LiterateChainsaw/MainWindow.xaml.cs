using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZephyrInnovations;
using System.Reflection;
using System.ComponentModel;
using System.Xml;
using System.IO;

namespace LiterateChainsaw
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //ConfigFilename = qGlobal.MainConfigurationFile;
        }

        //[TimingAspect]
        private void SomeMethod(string print)
        {
            Thread.Sleep(1000);
        }

        private string _configFilename = "";

        public virtual string ConfigFilename
        {
            get
            {
                return _configFilename;
            }
            set
            {
                _configFilename = value;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ResizePropertyGridSplitter(pgdConfigurationEditor, 30);

            if (File.Exists(qGlobal.MainConfigurationFile))
                pgdConfigurationEditor.SelectedObject = LoadConfiguration(qGlobal.MainConfigurationFile);

            pgdConfigurationEditor.Focus();
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveConfiguration(qGlobal.MainConfigurationFile, (PropertyCustom)pgdConfigurationEditor.SelectedObject);
                System.Windows.MessageBox.Show("Successfully save configuration", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

                //reload the config data
                qGlobal.LoadMainConfigurationFile(qGlobal.MainConfigurationFile);

                pgdConfigurationEditor.Focus();
            }
            catch (Exception ex)
            {
                qGlobal.WriteRunLog(true, "Configuration Editor:" + ex.Message, "ERROR");
                System.Windows.MessageBox.Show("Configuration Editor: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void ResizePropertyGridSplitter(System.Windows.Forms.PropertyGrid propertyGrid, int labelColumnPercentageWidth)
        {
            try
            {
                var width = propertyGrid.Width * (labelColumnPercentageWidth / 100.0);

                var realType = propertyGrid.GetType();
                while (realType != null && realType != typeof(System.Windows.Forms.PropertyGrid))
                {
                    realType = realType.BaseType;
                }

                var gvf = realType.GetField(@"gridView", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
                var gv = gvf.GetValue(propertyGrid);

                var mtf = gv.GetType().GetMethod(@"MoveSplitterTo", BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance);
                mtf.Invoke(gv, new object[] { (int)width });
            }
            catch (Exception ex)
            {
                qGlobal.WriteRunLog(true, "Configuration Editor:" + ex.Message, "ERROR");
                System.Windows.MessageBox.Show("Configuration Editor: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Configuration_Loading_and_Saving

        public PropertyCustom LoadConfiguration(string configurationFile)
        {
            PropertyCustom propertyCustom = new PropertyCustom();

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(configurationFile);

                XmlNode configuration = xmlDoc.SelectSingleNode("configuration");

                //Build the node list
                XmlNodeList sectionList = configuration.ChildNodes;

                for (int y = 0; y < sectionList.Count; y++)
                {
                    XmlNodeList settingsList = xmlDoc.SelectNodes("configuration/" + sectionList[y].Name + "/add");

                    if (settingsList.Count != 0 && settingsList != null)
                    {
                        //Add a property to PropertyCustom for each node found			
                        for (int i = 0; i < settingsList.Count; i++)
                        {
                            XmlAttribute atrribKey = settingsList[i].Attributes["key"];
                            XmlAttribute attribValue = settingsList[i].Attributes["value"];
                            XmlAttribute attribDescription = settingsList[i].Attributes["description"];

                            if (atrribKey != null && attribValue != null)
                            {
                                //If there's no description for the key - assign the name to the description
                                if (attribDescription == null)
                                    attribDescription = atrribKey;

                                //Display dropdown list of True False for boolean type
                                Type propType;
                                if (attribValue.Value.ToLower() == "true" || attribValue.Value.ToLower() == "false")
                                    propType = typeof(System.Boolean);
                                else
                                    propType = typeof(System.String);

                                //Add the property
                                propertyCustom.AddProperty(atrribKey.Value.ToString(), attribValue.Value.ToString(), attribDescription.Value.ToString(), sectionList[y].Name, propType, false, false);
                            }
                        }
                    }
                }

                xmlDoc = null;
            }
            catch (Exception ex)
            {
                qGlobal.WriteRunLog(true, "Configuration Editor:" + ex.Message, "ERROR");
                System.Windows.MessageBox.Show("Configuration Editor: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return propertyCustom;
        }

        public void SaveConfiguration(string configurationFile, PropertyCustom propertyCustom)
        {
            string archiveDirectory = string.Format("{0}{1}\\", qGlobal.MainConfigurationFilePath, "Archive");
            string newFile = string.Format("{0}{1}_{2}.config", archiveDirectory, "Main", DateTime.Now.ToString("d-MMM-yyyy_HHmmss"));

            if (!Directory.Exists(archiveDirectory))
                Directory.CreateDirectory(archiveDirectory);

            File.Copy(configurationFile, newFile);

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(configurationFile);

                //Populate our property collection
                PropertyDescriptorCollection props = propertyCustom.GetProperties();

                //Repolulate the supported sections
                RepopulateXmlSection("Application", xmlDoc, props);
                RepopulateXmlSection("User_Control", xmlDoc, props);
                RepopulateXmlSection("Reporting", xmlDoc, props);
                RepopulateXmlSection("Camera", xmlDoc, props);
                RepopulateXmlSection("Tool", xmlDoc, props);
                RepopulateXmlSection("Measurement", xmlDoc, props);
                RepopulateXmlSection("Multi_Vision_Processor", xmlDoc, props);
                RepopulateXmlSection("Vision_Processor", xmlDoc, props);
                RepopulateXmlSection("Geo_Algo_Processor", xmlDoc, props);
                RepopulateXmlSection("Keyence_Sensor", xmlDoc, props);

                xmlDoc.Save(configurationFile);
            }
            catch (Exception ex)
            {
                qGlobal.WriteRunLog(true, "Configuration Editor:" + ex.Message, "ERROR");
                System.Windows.MessageBox.Show("Configuration Editor: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RepopulateXmlSection(string sectionName, XmlDocument xmlDoc, PropertyDescriptorCollection props)
        {
            try
            {
                XmlNodeList nodes = xmlDoc.SelectNodes("configuration/" + sectionName + "/add");

                int i = 0;

                for (int j = 0; j < props.Count; j++)
                {
                    if (props[j].Category == sectionName)
                    {
                        nodes[i].Attributes["value"].Value = props[j].GetValue(null).ToString();
                        nodes[i].Attributes["description"].Value = props[j].Description.ToString();

                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                qGlobal.WriteRunLog(true, "Configuration Editor:" + ex.Message, "ERROR");
                System.Windows.MessageBox.Show("Configuration Editor: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState != WindowState.Maximized)
                this.WindowState = WindowState.Maximized;
        }

        private void Btn6_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;

            vm.ProcessRun.IsEntercore = true;
            //function();
            //Thread thewer = new Thread(function);
            //thewer.Priority = ThreadPriority.AboveNormal;
            //thewer.Start();
        }

        private void function()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var vm = DataContext as MainViewModel;

                //20240724
                vm.Wafer_Review.InspectedCount = 4123105;
                vm.ProcessRun.Wafer.PassCount = 234234906;
            });
        }
    }
}
