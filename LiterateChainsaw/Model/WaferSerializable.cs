using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LiterateChainsaw.Model
{
    public class Lot
    {
        [XmlAttribute("ID")]
        public string NAME { get; set; }

        [XmlElement("Cassette")]
        public List<Cassette> CASSETTES { get; set; }
    }

    public class Cassette
    {
        [XmlAttribute("ID")]
        public string NAME { get; set; }

        [XmlElement("Wafer")]
        public List<Wafer> WAFERS { get; set; }
    }

    public class Wafer
    {
        [XmlAttribute("ID")]
        public string NAME { get; set; }

        [XmlAttribute("Yield")]
        public double YIELD { get; set; }

        [XmlAttribute("PassCount")]
        public int PASSCOUNT { get; set; }

        [XmlAttribute("FailCount")]
        public int FAILCOUNT { get; set; }

        [XmlAttribute("InvalidCount")]
        public int INVALIDCOUNT { get; set; }

        [XmlAttribute("TotalCount")]
        public int TOTALCOUNT { get; set; }


        [XmlElement("Units")]
        public UnitResults UNITS { get; set; }


        [XmlElement("Classifications")]
        public Classifications CLASSIFICATIONS { get; set; }
    }

    public class UnitResults
    {
        [XmlElement("UnitResult")]
        public List<UnitResult> List { get; set; }
    }


    [XmlRootAttribute("SINGLE_UNIT", Namespace = "", IsNullable = true)]
    public class UnitResult
    {
        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlAttribute("RESULT")]
        public string RESULT { get; set; }
        //[XmlElement("PRIMARY_DEFECT")]
        //public Defect PRIMARY_DEFECT { get; set; }

        //[XmlElement("SECONDARY_DEFECT")]
        //public List<Defect> SECONDARY_DEFECT { get; set; }

        [XmlAttribute("PRIMARY_DEFECT_CODE")]
        public string PRIMARY_DEFECT_CODE { get; set; }

        [XmlElement("SECONDARY_DEFECT_CODES")]
        public string SECONDARY_DEFECT_CODES { get; set; }

        public bool IS_VPRO_FAIL_AS_PRIMARY { get; set; }

        public bool IS_VIDI_FAIL_AS_PRIMARY { get; set; }

        public bool IS_MANUAL_RESULT { get; set; }//if update result manually

        //[XmlElement("VPRO")]
        //public RuntimeData VISION_PRO { get; set; }

        //[XmlElement("VIDI")]
        //public RuntimeData VIDI { get; set; }
    }

    public class Classification
    {
        public string CODE { get; set; }
        public string COLOR { get; set; }
        public string DESCRIPTION { get; set; }
        public int COUNT { get; set; }
        public double PERCENTAGE { get; set; }
        public int VPRO_CONTRIBUTE { get; set; }
        public int VIDI_CONTRIBUTE { get; set; }
    }

    public class Classifications
    {
        [XmlElement("Classification")]
        public List<Classification> List { get; set; }
    }

    //futuredev
    public class RuntimeData
    {
        public bool IS_FAIL_AS_PRIMARY { get; set; }
        public string RESULT { get; set; }
        public string IMAGE_FILEPATH { get; set; }
        public string IMAGE_FILENAME { get; set; }

        [XmlElement("PRIMARY_DEFECT")]
        public string PRIMARY_DEFECT { get; set; }

        [XmlElement("SECONDARY_DEFECT")]
        public string SECONDARY_DEFECT { get; set; }

        public string TOTAL_PROCESS_TIME { get; set; }

        [XmlArray("VIDI_OUTPUT_RESULT_LIST"), XmlArrayItem("VIDI_OUTPUT_RESULT", typeof(VidiResultOutput))]
        public ArrayList VIDI_OUTPUT_RESULT_LIST = new ArrayList();
    }

    //futuredev
    public class VidiResultOutput
    {
        public string WORKSPACE;
        public string TOOL;
        public string PARAMETER;
        public string VALUE;
        public string SCORE;

        public VidiResultOutput()
        {
            WORKSPACE = "";
            TOOL = "";
            PARAMETER = "";
            VALUE = "";
            SCORE = "";
        }

        [XmlAttribute("INDEX")]
        public int INDEX
        {
            get;
            set;
        }
    }
}
