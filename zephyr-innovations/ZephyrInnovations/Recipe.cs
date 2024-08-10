using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ZephyrInnovations
{
    //recipe camera mapping
    [XmlRootAttribute("RECIPE_CAMERA_SETTING", Namespace = "", IsNullable = true)]
    public class RecipeCameraSetting
    {
        [XmlArray("RECIPES"), XmlArrayItem("RECIPE_CAMERA_DATA", typeof(RecipeCameraData))]
        public ArrayList RECIPE_CAMERA_DATA_LIST = new ArrayList();
    }

    [Serializable]
    public class RecipeCameraData
    {
        public string INSPECTION_TYPE { get; set; }
        public string RECIPE { get; set; }
        public string RECIPE_1 { get; set; }
        public string RECIPE_2 { get; set; }
        public string RECIPE_3 { get; set; }
        public string RECIPE_4 { get; set; }
        public string RECIPE_5 { get; set; }
        public string RECIPE_6 { get; set; }
        public string RECIPE_7 { get; set; }

        public RecipeCameraData()
        {
            INSPECTION_TYPE = "";
            RECIPE = "";
            RECIPE_1 = "";
            RECIPE_2 = "";
            RECIPE_3 = "";
            RECIPE_4 = "";
            RECIPE_5 = "";
            RECIPE_6 = "";
            RECIPE_7 = "";
        }
    }


    //create vision recipe setting
    [XmlRootAttribute("VISION_RECIPE_SETTING", Namespace = "", IsNullable = true)]
    public class VisionRecipeSetting
    {
        [XmlArray("RECIPES"), XmlArrayItem("RECIPE_DATA", typeof(VisionRecipeData))]
        public ArrayList RECIPE_DATA_LIST = new ArrayList();
    }

    [Serializable]
    public class VisionRecipeData
    {
        public int RECIPE_INDEX { get; set; }
        public string TOOL_RECIPE { get; set; }
        public string RECIPE { get; set; }
        public bool ORIENTATION_RECIPE { get; set; }
        public int CAMERA_NO { get; set; }
        public int CAMERA_EXPOSURE_TIME { get; set; }
        public string CAMERA_SAVE_PATH { get; set; }
        //public string TRAIN_TEMPLATE { get; set; }
        public string MAP_ORIGIN { get; set; }
        public double PIXEL_CONVERSION_VALUE { get; set; }
        public string PIXEL_CONVERSION_UNIT { get; set; }
        public int COLUMN_X { get; set; }
        public int ROW_Y { get; set; }
        public double PART_HEIGHT_UL_VALUE { get; set; }
        public double PART_HEIGHT_LL_VALUE { get; set; }
        public string REMARK { get; set; }

        public VisionRecipeData()
        {
            RECIPE_INDEX = 0;
            TOOL_RECIPE = "";
            RECIPE = "";
            ORIENTATION_RECIPE = false;
            CAMERA_NO = 1;
            CAMERA_EXPOSURE_TIME = 0;
            //TRAIN_TEMPLATE = "";
            CAMERA_SAVE_PATH = "";
            PIXEL_CONVERSION_VALUE = 0.0;
            PIXEL_CONVERSION_UNIT = "";
            COLUMN_X = 0;
            ROW_Y = 0;
            PART_HEIGHT_UL_VALUE = 0.0;
            PART_HEIGHT_LL_VALUE = 0.0;
            REMARK = "";
        }
    }
}
