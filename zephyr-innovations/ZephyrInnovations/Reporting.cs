using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Xml.Serialization;
using System.Xml;

namespace ZephyrInnovations
{
    #region REPORTING

    #region HYBRID_UNIT_DATA (VisionPro + Vidi)
    [XmlRootAttribute("FINAL_RUN_DATA", Namespace = "", IsNullable = true)]
    public class FinalUnitData//this class at its min, includes fields to update for wafer view model unit e.g. merged result, primary defect relevants //290323
    {
        public bool IsVidiTurnedOn;

        //Inspection results - MERGED
        public string Result;//det. by IsVidiTurnedOn's state
        public bool IsVProFailAsPrimaryDefect;//150523
        public bool IsVidiFailAsPrimaryDefect;//to highlight with stroke for map unit display
        public string Primary_Defect_Code;
        public string Primary_Defect_Desc;
        public string Primary_Defect_Code_Color;
        public string Secondary_Defect_Codes;
        public string Secondary_Defect_Descs;

        [XmlElement(ElementName = "VisionPro")]
        public UnitRunData VisionProRunData { get; set; }

        [XmlElement(ElementName = "Vidi")]
        public UnitRunData VidiRunData { get; set; }

        public FinalUnitData()
        {
            //VisionProRunData = new UnitRunData();
            //VidiRunData = new UnitRunData();
        }
    }
    #endregion

    #region RESULT_FROM_VISION_PROCESSOR

    #region UNIT_RUN_DATA

    [XmlRootAttribute("RUN_DATA", Namespace = "", IsNullable = true)]
    public class UnitRunData//only process run's read vision unit result calling for deserialization
    {
        public string RESULT;                       //PASS or FAIL
        public string IMAGE_FILEPATH;               //Image location
        public string IMAGE_FILENAME;               //Image filename + "-PASS or FAIL"
        public string PRIMARY_DEFECT_CODE;          //1 defect code, KI-01
        public string PRIMARY_DEFECT_DESC;
        public string PRIMARY_DEFECT_CODE_COLOR;
        public string SECONDARY_DEFECT_CODE;        //remaining defect code, KI-01;WI-02
        public string SECONDARY_DEFECT_DESC;
        public string ALIGNMENT_X_CORRECTION;       //First Unit X Correction in pixel
        public string ALIGNMENT_Y_CORRECTION;       //First Unit Y Correction in pixel
        public string VISION_ERROR;                 //Error produce in Vision Processor

        //Additional param - 0504523
        public string START_DATE_TIME;
        public string END_DATE_TIME;
        public string TOTAL_PROCESS_TIME;

        //Measurement data from GEO_ALGO
        [XmlArray("FEATURES"), XmlArrayItem("FEATURE", typeof(UnitFeature))]
        public ArrayList FEATURE_LIST = new ArrayList();

        [XmlArray("MEASUREMENTS"), XmlArrayItem("MEASUREMENT", typeof(UnitMeasurement))]
        public ArrayList MEASUREMENT_LIST = new ArrayList();

        [XmlArray("DEFECT_OUTPUT_DATA_LIST"), XmlArrayItem("DEFECT_OUTPUT_DATA", typeof(DefectOutputData))]
        public ArrayList DEFECT_OUTPUT_DATA_LIST = new ArrayList();

        [XmlArray("VIDI_OUTPUT_RESULT_LIST"), XmlArrayItem("VIDI_OUTPUT_RESULT", typeof(VidiResultOutput))]
        public ArrayList VIDI_OUTPUT_RESULT_LIST = new ArrayList();

        public UnitRunData()
        {
            RESULT = "";
            PRIMARY_DEFECT_CODE = "";
            PRIMARY_DEFECT_DESC = "";
            PRIMARY_DEFECT_CODE_COLOR = "";
            SECONDARY_DEFECT_CODE = "";
            SECONDARY_DEFECT_DESC = "";
            ALIGNMENT_X_CORRECTION = "";
            ALIGNMENT_Y_CORRECTION = "";
            VISION_ERROR = "";
        }
    }

    [Serializable]
    public class UnitFeature
    {
        public string FEATURE;
        public string VALUE;

        public UnitFeature()
        {
            FEATURE = "";
            VALUE = "";
        }

        [XmlAttribute("INDEX")]
        public int INDEX
        {
            get;
            set;
        }
    }

    [Serializable]
    public class UnitMeasurement
    {
        public string NAME;
        public string TYPE;
        public double VALUE;
        public string UNIT;
        public double LCL_VALUE;
        public double UCL_VALUE;

        public UnitMeasurement()
        {
            NAME = "";
            TYPE = "";
            VALUE = 0.00;
            UNIT = "";
            LCL_VALUE = 0.00;
            UCL_VALUE = 0.00;
        }

        [XmlAttribute("INDEX")]
        public int INDEX
        {
            get;
            set;
        }
    }

    [Serializable]
    public class DefectOutputData
    {
        public string DEFECT_CODE;
        public string DEFECT_COLOR;
        public string DRAWING_TYPE;
        public string DRAWING_DATA;
        public string OBJECT_TYPE;
        public string OBJECT_DATA;
        public string SPEC_DEFINITION;

        public DefectOutputData()
        {
            DEFECT_CODE = "";
            DEFECT_COLOR = "";
            DRAWING_TYPE = "";
            DRAWING_DATA = "";
            OBJECT_TYPE = "";
            OBJECT_DATA = "";
            SPEC_DEFINITION = "";
        }
    }

    [Serializable]
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
    #endregion

    #endregion

    #region ID

    #region FINAL_RUN_DATA

    [XmlRootAttribute("RUN_DATA", Namespace = "", IsNullable = true)]
    public class FinalRunData
    {
        public string LOT_ID;
        public string TRAY_ID;
        public string MAGAZINE_ID;
        public string EMPLOYEE_ID;
        public string SLOT_NO;
        public string TOTAL_SLOT;
        public string RECIPE;
        public string INSPECTION_TYPE;
        public string TOTAL_UNIT;
        public string TOTAL_UNIT_PASS;
        public string TOTAL_UNIT_FAIL;
        public string UNIT_PASS_PCT;
        public string UNIT_FAIL_PCT;
        public string UNIT_X;
        public string UNIT_Y;
        public string BASE_HEIGHT_1;
        public string BASE_HEIGHT_2;
        public string PART_HEIGHT_UL;
        public string PART_HEIGHT_LL;
        public string MAP_ORIGIN;
        public string ORIENTATION_RESULT;
        public string START_DATE_TIME;
        public string END_DATE_TIME;
        public string TOTAL_PROCESS_TIME;
        public string UPH;

        [XmlArray("UNITS"), XmlArrayItem("UNIT", typeof(FinalUnitRunData))]
        public ArrayList UNIT_LIST = new ArrayList();

        public FinalRunData()
        {
            LOT_ID = "";
            TRAY_ID = "";
            MAGAZINE_ID = "";
            EMPLOYEE_ID = "";
            RECIPE = "";
            INSPECTION_TYPE = "";
            TOTAL_UNIT = "";
            TOTAL_UNIT_PASS = "";
            TOTAL_UNIT_FAIL = "";
            UNIT_PASS_PCT = "";
            UNIT_FAIL_PCT = "";
            UNIT_X = "";
            UNIT_Y = "";
            BASE_HEIGHT_1 = "";
            BASE_HEIGHT_2 = "";
            PART_HEIGHT_UL = "";
            PART_HEIGHT_LL = "";
            START_DATE_TIME = "";
            END_DATE_TIME = "";
            TOTAL_PROCESS_TIME = "";
            UPH = "";
        }
    }

    [Serializable]
    public class FinalUnitRunData
    {
        public string UNIT;
        public string X;
        public string Y;
        public string VISION_RESULT;
        public string IMAGE_FILENAME;
        public string IMAGE_FILEPATH;
        public string PRIMARY_DEFECT_CODE;
        public string PRIMARY_DEFECT_DESC;
        public string PRIMARY_DEFECT_CODE_COLOR;
        public string SECONDARY_DEFECT_CODE;
        public string SECONDARY_DEFECT_DESC;
        public string FOUND_VISION_IMAGE_DATE_TIME;
        public string PART_HEIGHT_MEASUREMENT;
        public string PART_HEIGHT_MEASUREMENT_UNIT;
        public string PART_HEIGHT_MEASUREMENT_RESULT;
        public string PART_HEIGHT_UL;
        public string PART_HEIGHT_LL;
        public bool FINAL_REJECT_2D_3D;
        public string VISION_RECIPE;

        [XmlArray("MEASUREMENTS"), XmlArrayItem("MEASUREMENT", typeof(UnitMeasurement))]
        public ArrayList MEASUREMENT_LIST = new ArrayList();

        [XmlArray("DEFECT_OUTPUT_DATA_LIST"), XmlArrayItem("DEFECT_OUTPUT_DATA", typeof(DefectOutputData))]
        public ArrayList DEFECT_OUTPUT_DATA_LIST = new ArrayList();

        public FinalUnitRunData()
        {
            UNIT = "";
            X = "";
            Y = "";
            VISION_RESULT = "";
            IMAGE_FILENAME = "";
            IMAGE_FILEPATH = "";
            PRIMARY_DEFECT_CODE = "";
            PRIMARY_DEFECT_DESC = "";
            PRIMARY_DEFECT_CODE_COLOR = "";
            SECONDARY_DEFECT_CODE = "";
            SECONDARY_DEFECT_DESC = "";
            FOUND_VISION_IMAGE_DATE_TIME = "";
            PART_HEIGHT_MEASUREMENT = "";
            PART_HEIGHT_MEASUREMENT_UNIT = "";
            PART_HEIGHT_MEASUREMENT_RESULT = "";
            PART_HEIGHT_UL = "";
            PART_HEIGHT_LL = "";
            FINAL_REJECT_2D_3D = false;
        }

        [XmlAttribute("INDEX")]
        public int INDEX
        {
            get;
            set;
        }
    }

    #endregion

    #endregion

    #endregion

    #region SPLIT_IMAGE

    [XmlRootAttribute("SPLIT_IMAGE_RUN_DATA", Namespace = "", IsNullable = true)]
    public class SplitImageRunData
    {
        public string LOT_ID;
        public string TRAY_ID;
        public string MAGAZINE_ID;
        public string RECIPE;
        public string MAP_ORIGIN;
        public string IMAGE_NO;
        public string TOTAL_UNIT;
        public string IMAGE_FILENAME;
        public string IMAGE_SUBMIT_DIRECTORY;
        public string START_DATE_TIME;

        public SplitImageRunData()
        {
            LOT_ID = "";
            TRAY_ID = "";
            MAGAZINE_ID = "";
            RECIPE = "";
            MAP_ORIGIN = "";
            TOTAL_UNIT = "";
            IMAGE_FILENAME = "";
            START_DATE_TIME = "";
        }
    }

    [Serializable]
    public class SplitUnitRunData
    {
        public string UNIT;
        public string X;
        public string Y;
        public string IMAGE_FILENAME;

        public SplitUnitRunData()
        {
            UNIT = "";
            X = "";
            Y = "";
            IMAGE_FILENAME = "";
        }

        [XmlAttribute("INDEX")]
        public int INDEX
        {
            get;
            set;
        }
    }


    #endregion
}
