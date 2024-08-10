using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZephyrInnovations
{
    class MainConfiguration
    {
    }

    public class ApplicationConfiguration
    {
        public ApplicationConfiguration()
        {
        }

        public string MachineID { get; set; }
        public string LogFileDirectory { get; set; }
    }

    public class ReportingConfiguration
    {
        public ReportingConfiguration()
        {
        }

        public string ReportFileDirectory { get; set; }
    }

    public class ToolConfiguration
    {
        public ToolConfiguration()
        {
        }

        public bool EnableTCPIP { get; set; }

        public string TCPIPIP { get; set; }
        public int TCPIPPort { get; set; }
    }


    public class VisionProcessorConfiguration
    {
        public VisionProcessorConfiguration()
        {
        }
        public string RecipeFileDirectory { get; set; }
        public string ReadIMGFileDirectory { get; set; }
        public string ResultXMLFileDirectory { get; set; }
        public bool EnableVidi { get; set; }
        public bool EnableMultiThread { get; set; }
        public int MaxDegreeofParallelism { get; set; }
    }

    public class GeoAlgoProcessorConfiguration
    {
        public GeoAlgoProcessorConfiguration()
        {
        }

        public string DefectCodeFileDirectory { get; set; }
        public string UnitPassCode { get; set; }
        public string UnitPassCodeDesc { get; set; }
        public string UnitPassCodeColour { get; set; }
    }

    public class WaferMapConfiguration
    {
        public WaferMapConfiguration()
        {
        }

        public string LoadMapFilesDirectory { get; set; }
        public string SaveMapFilesDirectory { get; set; }
        public qGlobal.MapType InputMapType { get; set; }
        public qGlobal.MapType OutputMapType { get; set; }
    }

}
