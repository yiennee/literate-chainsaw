using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Xml.Linq;
using System.IO;

namespace IdealPancake
{
    public class FSWImageInput : QVS_Interfaces.ImageInputBase
    {
        FileSystemWatcher fsw;
        XDocument settingXml;
        string[] extensions;
        private string sPath;

        public FSWImageInput(XDocument doc) : base()
        {
            fsw = new FileSystemWatcher();
            settingXml = doc;
            // fsw.Path = settingXml.Root.Element("IncomingPath").Value;
            extensions = settingXml.Root.Element("Extension").Value.Split(',');
            fsw.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            fsw.Filter = "*.*";
            fsw.InternalBufferSize = 65536;
            fsw.Created += new FileSystemEventHandler(OnChanged);
            fsw.IncludeSubdirectories = false;//060323
            // fsw.Error += Fsw_Error;
        }

        public void Dispose()
        {
            fsw.EnableRaisingEvents = false;
            fsw.Dispose();
        }

        private void Fsw_Error(object sender, ErrorEventArgs e)
        {
            HandleErrorPath(IncomingPath, fsw.Filter);
        }

        // Define the event handlers.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            FileInfo fi = new FileInfo(e.FullPath);
            if (extensions.Contains(fi.Extension))
                ProcessImage(fi);
        }

        public string IncomingPath
        {
            get { return sPath; }
            set
            {
                sPath = value;
                fsw.Path = sPath;
                fsw.EnableRaisingEvents = true;
            }
        }
    }
}
