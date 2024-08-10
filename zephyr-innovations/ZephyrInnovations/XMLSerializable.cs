using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Collections;

namespace ZephyrInnovations
{
    [XmlRootAttribute("DEFECT_GROUP_SETTING", Namespace = "", IsNullable = true)]
    public class DefectGroupSetting
    {
        [XmlArray("DEFECT_GROUPS"), XmlArrayItem("DEFECT_GROUP_DATA", typeof(DefectGroupData))]
        public ArrayList DEFECT_GROUP_DATA_LIST = new ArrayList();
    }

    [XmlRootAttribute("DEFECT_CODE_SETTING", Namespace = "", IsNullable = true)]
    public class DefectCodeSetting
    {
        [XmlArray("DEFECT_CODES"), XmlArrayItem("DEFECT_CODE_DATA", typeof(DefectCodeData))]
        public ArrayList DEFECT_CODE_DATA_LIST = new ArrayList();
    }

    [Serializable]
    public class DefectCodeData : IComparable<DefectCodeData>
    {
        public string DEFECT_GROUP { get; set; }
        public string DEFECT_CODE { get; set; }
        public string DEFECT_NAME { get; set; }
        public string DEFECT_CODE_COLOR { get; set; }
        public string PRIORITY { get; set; }
        //public int INT_PRIORITY { get; set; }
        public string ACTIVATE { get; set; }
        private string Activate_DL = "false";
        public string ACTIVATE_DL { get { return Activate_DL; } set => Activate_DL = value == "" ? Activate_DL : value; }

        public DefectCodeData()
        {
            DEFECT_GROUP = "";
            DEFECT_CODE = "";
            DEFECT_NAME = "";
            DEFECT_CODE_COLOR = "";
            PRIORITY = "";
            ACTIVATE = "";
            ACTIVATE_DL = "";
        }

        public int CompareTo(DefectCodeData others)
        {
            int Priority1;
            int Priority2;

            //unlikely but if fail parse, set to lowest
            if (!Int32.TryParse(this.PRIORITY, out Priority1))
            {
                Priority1 = 1000;
            }
            if (!Int32.TryParse(others.PRIORITY, out Priority2))
            {
                Priority2 = 1000;
            }

            if (Priority1 < Priority2)
                return -1;
            else if (Priority1 > Priority2)
                return 1;
            else
                return 0;
        }
    }

    [Serializable]
    public class DefectGroupData
    {
        public string DEFECT_GROUP { get; set; }

        public DefectGroupData()
        {
            DEFECT_GROUP = "";
        }
    }


    [Serializable]
    public class PixelConversion
    {
        public string Value { get; set; }
        public string Unit { get; set; }

        public PixelConversion()
        {
            Value = "";
            Unit = "";
        }
    }

}
