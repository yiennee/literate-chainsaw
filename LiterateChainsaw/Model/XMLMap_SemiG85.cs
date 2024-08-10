using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;
using ZephyrInnovations;

namespace LiterateChainsaw.Model
{
    //[XmlRootAttribute("Map", Namespace = "", IsNullable = true)]
    [XmlRoot(ElementName = "Map")]
    public class XMLMap_SemiG85 : IMap
    {
        private static Action _action = new Action(() => { return; });
        #region Variables to assign values after deserialization : Call interface method for this action
        //[XmlIgnore]
        //public int CoordX { get; set; }
        //[XmlIgnore]
        //public int CoordY { get; set; }
        [XmlIgnore]
        public int WaferRowCount { get; set; }
        [XmlIgnore]
        public int WaferColumnCount { get; set; }
        [XmlIgnore]
        public double WaferHeight { get; set; }
        [XmlIgnore]
        public double WaferWidth { get; set; }
        //[XmlIgnore]
        //public double OrgX { get; set; }
        //[XmlIgnore]
        //public double OrgY { get; set; }
        [XmlIgnore]
        public double Width { get; set; }
        [XmlIgnore]
        public double Height { get; set; }
        [XmlIgnore]
        public double GapX { get; set; }
        [XmlIgnore]
        public double GapY { get; set; }
        #endregion

        [XmlElement(ElementName = "Device")]
        public Device Device { get; set; }

        [XmlIgnore]
        public static XMLMap_SemiG85 Instance { get; set; } = new XMLMap_SemiG85();
        [XmlIgnore]
        public Bin[] BinCodeArr;

        public XMLMap_SemiG85()
        {
            Device = new Device();
        }

        /// <summary>
        /// Load Map: Used to assign map unit color according to bin code extracted from map
        /// </summary>
        /// <param name="i_BinCode"></param>
        /// <returns></returns>
        public Color AssignColorToBinCode(string i_BinCode)
        {
            Color _Color = Colors.LightGray;

            //add into switch statements for every bincode found in map file
            switch (i_BinCode)
            {
                case "777":
                    _Color = Colors.Gray;
                    break;
                case "kkk":
                    _Color = Colors.Cyan;
                    break;
                case "255":
                    _Color = Colors.Red;
                    break;
                case "444":
                    _Color = Colors.Purple;
                    break;
                case "555":
                    _Color = Colors.Yellow;
                    break;
                case "666":
                    _Color = Colors.Cyan;
                    break;
                case "001":
                    _Color = Colors.LightGray;
                    break;
                case "999":
                    _Color = Colors.Transparent;
                    break;
                default:
                    _Color = Colors.White;
                    break;
            }

            return _Color;
        }

        /// <summary>
        /// Save Map: Used to assign bin code to map unit according to result stored in the map Unit
        /// </summary>
        /// <param name="i_Unit"></param>
        /// <returns></returns>
        public string AssignBinCodeToUnit(Unit i_Unit)
        {
            string BinCode = " ";
            //add into switch statements for every result to update in map file (may need to refer to existing bincode)
            //switch (i_Unit.Primary_Defect_Code)
            //{
            //    case "PASS":
            //        BinCode = "777 ";
            //        break;
            //    case "et":
            //        BinCode = "kkk ";
            //        break;
            //    case "ss":
            //        BinCode = "255 ";
            //        break;
            //    case "CP":
            //        BinCode = "444 ";
            //        break;
            //    case "CT":
            //        BinCode = "555 ";
            //        break;
            //    case "FM":
            //        BinCode = "666 ";
            //        break;
            //    case "":
            //    case null:
            //        BinCode = "001 ";
            //        break;
            //    case "FAIL":
            //    case "MS":
            //        BinCode = "999 ";
            //        break;
            //    //case null:
            //    //    BinCode = "null ";
            //    //    break;
            //}
            if (i_Unit.Primary_Defect_Code == "")
                BinCode = "001 ";
            else
                BinCode = "009 ";

            return BinCode;
        }

        /// <summary>
        /// Load map details into plane of size Width = CanvasSizeX; Height = CanvasSizeY
        /// </summary>
        /// <param name="CanvasSizeX"></param>
        /// <param name="CanvasSizeY"></param>
        /// <returns></returns>
        public List<Unit> Load(out string errMsg)//, out List<Unit> o_UnitList)
        {
            errMsg = "";
            List<Unit> o_UnitList = new List<Unit>();

            try
            {
                BinCodeArr = Device.Bin.ToArray();
                //Bin[] BCs = Device.Bin.ToArray();

                LoadCommonMapParamValue();

                //Instance.Device = Device;

                Unit currentunit = new Unit(_action);

                //xaml setting
                double Zoom = 1;
                double Canvas_Width = 600;// * Zoom;
                double Canvas_Height = 600;// * Zoom;


                int dTotalCol = 20;
                int dTotalRow = 20;
                int dUnitWidth = 5500;// 1500;
                int dUnitHeight = 2500;// 3200;
                int dGapX = 150;
                int dGapY = 100;

                int truewaferwidth = (dUnitWidth + dGapX) * dTotalCol;
                int truewaferheight = (dUnitHeight + dGapY) * dTotalRow;
                double scale1 = Canvas_Width / Convert.ToDouble(truewaferwidth);//hardcode to UI canvas width
                double scale2 = Canvas_Height / Convert.ToDouble(truewaferheight);//hardcode to UI canvas height
                double scale3 = Canvas_Width / ((Width + GapX) * WaferColumnCount);// Convert.ToDouble();//hardcode to UI canvas width
                double scale4 = Canvas_Height / ((Height + GapY) * WaferRowCount);//hardcode to UI canvas height
                double dScale = Math.Min(scale3, scale4);

                //for (int y = 0; y < dTotalRow; y++)
                //    for (int x = 0; x < dTotalCol; x++)
                //    {

                //        Unit tempunit = new Unit
                //        {
                //            OrgX = x * (dUnitWidth + dGapX), //20 is unit width + gap x
                //            OrgY = y * (dUnitHeight + dGapY),
                //            Width = dUnitWidth, //10 is unit width nett
                //            Height = dUnitHeight,
                //            GapX = dGapX,
                //            GapY = dGapY,
                //            //Color = Color.FromArgb(255, (byte)(x * 20), (byte)(y * 20), 0)//030922 comment for testing
                //            Stroke = Colors.Honeydew,
                //            CoordX = x,
                //            CoordY = y,
                //            WaferWidth = (dUnitWidth + dGapX) * dTotalCol,
                //            WaferHeight = (dUnitHeight + dGapY) * dTotalRow,
                //            WaferRowCount = dTotalRow,
                //            WaferColumnCount = dTotalCol
                //        };

                //        currentunit.OrgX = (x * (dUnitWidth + dGapX) + 0.5 * dGapX) * dScale;
                //        currentunit.OrgY = (y * (dUnitHeight + dGapY) + 0.5 * dGapY) * dScale;
                //        currentunit.Width = dUnitWidth * dScale;
                //        currentunit.Height = dUnitHeight * dScale;
                //        currentunit.GapX = dGapX * dScale;
                //        currentunit.GapY = dGapY * dScale;
                //        //CanvasWidth = Convert.ToInt16(tempunit.OrgX + tempunit.Width + tempunit.GapX);
                //        //CanvasHeight = Convert.ToInt16(tempunit.OrgY + tempunit.Height + tempunit.GapY);
                //        //Items.Add(new Unit
                //        //{
                //        //    X = x * (dUnitWidth + dGapX), //20 is unit width + gap x
                //        //    Y = y * (dUnitHeight + dGapY),
                //        //    Width = dUnitWidth, //10 is unit width nett
                //        //    Height = dUnitHeight,
                //        //    //Color = Color.FromArgb(255, (byte)(x * 20), (byte)(y * 20), 0)//030922 comment for testing
                //        //    Color = CheckHypo1(x * 20 + 5, y * 20 + 5) ? Colors.AliceBlue : Colors.Red, //checkhypo input should be (runno * (unit width + gap x) + 1/2 gap x)
                //        //});
                //        //DebugItems.Add(tempunit);
                //    }              

                ////actual case sample
                for (int i = 0; i < Device.Data.RowList.Count; i++)
                {
                    Row currentRow = Device.Data.RowList[i];

                    string InnerText = currentRow.InnerText.Trim();
                    string[] CurrentRow_BCbyCol = InnerText.Split(' ').ToArray();

                    //from a whole row, decode one by one the col unit.
                    for (int j = 0; j < CurrentRow_BCbyCol.Length; j++)
                    {
                        currentunit.OrgX = (j * (Width + GapX) + 0.5 * GapX) * dScale;
                        currentunit.OrgY = (i * (Height + GapY) + 0.5 * GapY) * dScale;
                        currentunit.Width = Width * dScale;
                        currentunit.Height = Height * dScale;
                        currentunit.GapX = GapX * dScale;
                        currentunit.GapY = GapY * dScale;

                        //Console.WriteLine("CurrentRow_BCbyCol[j]= " + CurrentRow_BCbyCol[j]);
                        if (CurrentRow_BCbyCol[j] == "255" || CurrentRow_BCbyCol[j] == "~" || CurrentRow_BCbyCol[j] == "kkk") //hardcode 255
                            continue;

                        o_UnitList.Add(new Unit(_action)
                        {
                            CoordX = j,
                            CoordY = i,
                            WaferColumnCount = WaferColumnCount,// dTotalCol,
                            WaferRowCount = WaferRowCount,// dTotalRow,
                            WaferWidth = WaferWidth,//truewaferwidth,
                            WaferHeight = WaferHeight,//truewaferheight,
                            Width = currentunit.Width,
                            Height = currentunit.Height,
                            GapX = currentunit.GapX,
                            GapY = currentunit.GapY,
                            OrgX = currentunit.OrgX,
                            OrgY = currentunit.OrgY,
                            Stroke = Colors.Black,
                            Fill = AssignColorToBinCode(CurrentRow_BCbyCol[j])//CheckBinCodeColor(strArr[j])
                        });
                    }
                }

                Instance = this;
                return o_UnitList;
            }
            catch (Exception ex)
            {
                qGlobal.WriteRunLog(true, ex.Message, "ERROR");
                errMsg = string.Format("{0}", "Failed to load XML");

                return null;
            }
            #region Func<string, Color> CheckBinCodeColor
            //Func<string, Color> CheckBinCodeColor = (bincode) =>
            //{
            //    Color color = Colors.LightGray;

            //    switch (bincode)
            //    {
            //        case "777":
            //            color = Colors.Lime;
            //            break;
            //        case "kkk":
            //            color = Colors.Cyan;
            //            break;
            //        case "255":
            //            color = Colors.Red;
            //            break;
            //        case "444":
            //            color = Colors.Purple;
            //            break;
            //        case "555":
            //            color = Colors.Yellow;
            //            break;
            //        case "666":
            //            color = Colors.Cyan;
            //            break;
            //        case "001":
            //            color = Colors.Lime;
            //            break;
            //        case "999":
            //            color = Colors.Transparent;
            //            break;
            //        default:
            //            color = Colors.White;
            //            break;

            //    }
            //    return color;
            //};
            #endregion
        }


        /// <summary>
        /// Load map details into plane of size Width = CanvasSizeX; Height = CanvasSizeY
        /// </summary>
        /// <param name="CanvasSizeX"></param>
        /// <param name="CanvasSizeY"></param>
        /// <returns></returns>
        public List<UnitResult> Load_Test(out string errMsg)//, out List<Unit> o_UnitList)
        {
            errMsg = "";
            List<UnitResult> o_UnitList = new List<UnitResult>();

            try
            {
                BinCodeArr = Device.Bin.ToArray();
                //Bin[] BCs = Device.Bin.ToArray();

                LoadCommonMapParamValue();

                //Instance.Device = Device;

                UnitResult currentunit = new UnitResult();

                //xaml setting
                double Zoom = 1;
                double Canvas_Width = 1000;// 346.5;// * Zoom;
                double Canvas_Height = 1000;// 346.5;// * Zoom;

                double scale3 = Canvas_Width / ((Width + GapX) * WaferColumnCount);// Convert.ToDouble();//hardcode to UI canvas width
                double scale4 = Canvas_Height / ((Height + GapY) * WaferRowCount);//hardcode to UI canvas height
                double dScale = Math.Min(scale3, scale4);

                ////actual case sample
                for (int i = 0; i < Device.Data.RowList.Count; i++)
                {
                    Row currentRow = Device.Data.RowList[i];

                    string InnerText = currentRow.InnerText.Trim();
                    string[] CurrentRow_BCbyCol = InnerText.Split(' ').ToArray();

                    //from a whole row, decode one by one the col unit.
                    for (int j = 0; j < CurrentRow_BCbyCol.Length; j++)
                    {
                        //currentunit.OrgX = (j * (Width + GapX) + 0.5 * GapX) * dScale;
                        //currentunit.OrgY = (i * (Height + GapY) + 0.5 * GapY) * dScale;
                        //currentunit.Width = Width * dScale;
                        //currentunit.Height = Height * dScale;
                        //currentunit.GapX = GapX * dScale;
                        //currentunit.GapY = GapY * dScale;

                        //Console.WriteLine("CurrentRow_BCbyCol[j]= " + CurrentRow_BCbyCol[j]);
                        if (CurrentRow_BCbyCol[j] == "255" || CurrentRow_BCbyCol[j] == "~" || CurrentRow_BCbyCol[j] == "kkk") //hardcode 255
                            continue;

                        //o_UnitList.Add(new UnitResult()
                        //{
                        //    CoordX = j,
                        //    CoordY = i,
                        //    Width = currentunit.Width,
                        //    Height = currentunit.Height,
                        //    GapX = currentunit.GapX,
                        //    GapY = currentunit.GapY,
                        //    OrgX = currentunit.OrgX,
                        //    OrgY = currentunit.OrgY
                        //    //Stroke = Colors.Black,
                        //    //Fill = AssignColorToBinCode(CurrentRow_BCbyCol[j])//CheckBinCodeColor(strArr[j])
                        //});
                    }
                }

                Instance = this;
                return o_UnitList;
            }
            catch (Exception ex)
            {
                qGlobal.WriteRunLog(true, ex.Message, "ERROR");
                errMsg = string.Format("{0}", "Failed to load XML");

                return null;
            }
        }

        /// <summary>
        /// Get from Device the Common Params of Current Wafer
        /// </summary>
        private void LoadCommonMapParamValue()
        {
            WaferRowCount = Instance.WaferRowCount = Device.Rows;
            WaferColumnCount = Instance.WaferColumnCount = Device.Columns;
            Width = Instance.Width = Device.DeviceSizeX;
            Height = Instance.Height = Device.DeviceSizeY;
            GapX = Instance.GapX = 0.05 * Width;
            GapY = Instance.GapY = 0.05 * Height;
            WaferHeight = Instance.WaferHeight = /*Device.WaferSize > 0 ? Device.WaferSize : */(WaferRowCount - 1) * (Height + GapY) + Height;
            WaferWidth = Instance.WaferWidth = /*Device.WaferSize > 0 ? Device.WaferSize :*/ (WaferColumnCount - 1) * (Width + GapX) + Width;

            //Console.WriteLine("WaferRowCount = " + WaferRowCount + "; WaferColumnCount = " + WaferColumnCount);
            //Console.WriteLine("Device Width = " + Width + "; Device Height = " + Height);
            //Console.WriteLine("Device GapX = " + GapX + "; Device GapY = " + GapY);
            //Console.WriteLine("WaferWidth = " + WaferWidth + "; WaferHeight = " + WaferHeight);
        }

        public bool Save(List<Unit> UnitList, string filePath, string fileName, out string errMsg)
        {
            errMsg = "";
            if (UnitList.Count == 0)
                return false;

            List<Row> _RowList = new List<Row>();

            #region Func<Unit, string> ConvertToCode
            //Func<Unit, string> ConvertToCode = (U) =>
            //{
            //    string str = " ";

            //    switch (U.Result)
            //    {
            //        case "":
            //            str = "s ";
            //            break;
            //        case null:
            //            str = "null ";
            //            break;
            //    }

            //    return str;
            //};
            #endregion

            try
            {
                for (int i = 0; i < Device.Data.RowList.Count; i++)
                {
                    string InnerText = "";

                    var RowUnits = UnitList.Where(x => x.CoordY == i);
                    RowUnits.OrderBy(x => x.CoordX);

                    for (int j = 0; j < WaferColumnCount; j++)
                    {
                        var Unit = RowUnits.FirstOrDefault(y => y.CoordX == j);
                        if (Unit != null)
                            InnerText += AssignBinCodeToUnit(Unit);
                        else
                            InnerText += "255 ";
                    }

                    InnerText = InnerText.Trim();
                    Row _Row = new Row();
                    _Row.InnerText = InnerText;

                    _RowList.Add(_Row);
                }
                Instance.Device.Data.CreateDate = DateTime.Now.ToString();
                Instance.Device.Data.RowList = _RowList;

                //this.Device.Bin
                //if (Instance.Device.Bin.Count != 0)
                //    Instance.Device.Bin.Clear();

                //Instance.Device.Bin = new List<Bin> { new Bin { BinCode = "", BinQuality = "", BinCount = 0, BinDescription = "" },
                //    new Bin { BinCode = "", BinQuality = "", BinCount = 1, BinDescription = "" },
                //    new Bin { BinCode = "", BinQuality = "", BinCount = 2, BinDescription = "" } };

                string MapFile = string.Format("{0}{1}.xml", filePath, fileName);
                XMLMap_SemiG85 obj_to_Serialize = this;

                string strErrMsg = "";
                System.Xml.Serialization.XmlSerializer serializer = null;
                System.IO.FileStream stream = null;
                try
                {
                    serializer = new System.Xml.Serialization.XmlSerializer(typeof(XMLMap_SemiG85));
                    stream = new System.IO.FileStream(MapFile, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                    serializer.Serialize(stream, obj_to_Serialize);
                    return true;
                }
                catch (Exception ex)
                {
                    strErrMsg = string.Format("Serialization Error. File={0}; \nException: {1}", MapFile, ex.ToString());
                    return false;
                }
                finally
                {
                    if (stream != null)
                        stream.Close();
                }
            }
            catch (Exception ex)
            {
                qGlobal.WriteRunLog(true, ex.Message, "ERROR");
                errMsg = string.Format("{0}{1}", "Failed to Save Map. Map Type = ", nameof(XMLMap_SemiG85));
                return false;
            }
        }
    }

    [XmlRoot(ElementName = "ReferenceDevice")]
    public class ReferenceDevice
    {

        [XmlAttribute(AttributeName = "ReferenceDeviceX")]
        public int ReferenceDeviceX { get; set; }

        [XmlAttribute(AttributeName = "ReferenceDeviceY")]
        public int ReferenceDeviceY { get; set; }

        public ReferenceDevice()
        {

        }
    }

    [XmlRoot(ElementName = "Bin")]
    public class Bin
    {

        [XmlAttribute(AttributeName = "BinCode")]
        public string BinCode { get; set; }

        [XmlAttribute(AttributeName = "BinQuality")]
        public string BinQuality { get; set; }

        [XmlAttribute(AttributeName = "BinDescription")]
        public string BinDescription { get; set; }

        [XmlAttribute(AttributeName = "BinCount")]
        public int BinCount { get; set; }

        public Bin()
        {

        }
    }

    [XmlRoot(ElementName = "SupplierData")]
    public class SupplierData
    {

        [XmlAttribute(AttributeName = "Facility_id")]
        public string FacilityId { get; set; }

        [XmlAttribute(AttributeName = "Fab_id")]
        public string FabId { get; set; }

        [XmlAttribute(AttributeName = "Device")]
        public string Device { get; set; }

        [XmlAttribute(AttributeName = "Wafers")]
        public int Wafers { get; set; }

        public SupplierData()
        {

        }
    }

    [XmlRoot(ElementName = "Data")]
    public class Data
    {
        [XmlAttribute(AttributeName = "MapRevision")]
        public string MapRevision { get; set; }

        [XmlAttribute(AttributeName = "LotSequence")]
        public int LotSequence { get; set; }

        [XmlAttribute(AttributeName = "CreateDate")]
        public string CreateDate { get; set; }

        //[XmlArray("Row"), XmlArrayItem("Row", typeof(Row))]
        //public ArrayList Row = new ArrayList();

        [XmlElement(ElementName = "Row")]
        public List<Row> RowList { get; set; }
        public Data()
        {

        }
    }

    [XmlRoot(ElementName = "Row")]
    public class Row
    {
        [XmlText]
        public string InnerText { get; set; }

        public Row()
        {
            InnerText = "";
        }
    }

    [XmlRoot(ElementName = "Device")]
    //[Serializable]
    public class Device
    {

        [XmlElement(ElementName = "ReferenceDevice")]
        public ReferenceDevice ReferenceDevice { get; set; }

        [XmlElement(ElementName = "Bin")]
        public List<Bin> Bin { get; set; }

        [XmlElement(ElementName = "SupplierData")]
        public SupplierData SupplierData { get; set; }

        [XmlElement(ElementName = "Data")]
        public Data Data { get; set; }

        //[XmlElement(ElementName = "Data")]
        //public List<Row> Data { get; set; }

        //[XmlArray("Data"), XmlArrayItem("Row", typeof(Data))]
        //public ArrayList Data = new ArrayList();

        [XmlAttribute(AttributeName = "LotId")]
        public int LotId { get; set; }

        [XmlAttribute(AttributeName = "DeviceSizeX")]
        public double DeviceSizeX { get; set; }

        [XmlAttribute(AttributeName = "DeviceSizeY")]
        public double DeviceSizeY { get; set; }

        [XmlAttribute(AttributeName = "BinType")]//
        public string BinType { get; set; }

        [XmlAttribute(AttributeName = "NullBin")]
        public int NullBin { get; set; }

        [XmlAttribute(AttributeName = "Rows")]
        public int Rows { get; set; }

        [XmlAttribute(AttributeName = "Columns")]
        public int Columns { get; set; }

        [XmlAttribute(AttributeName = "MapType")]//
        public string MapType { get; set; }

        [XmlAttribute(AttributeName = "OriginLocation")]
        public int OriginLocation { get; set; }

        [XmlAttribute(AttributeName = "Orientation")]
        public int Orientation { get; set; }

        [XmlAttribute(AttributeName = "WaferSize")]
        public int WaferSize { get; set; }

        [XmlAttribute(AttributeName = "Status")]//
        public string Status { get; set; }

        //public List<int> single = new List<int>();
        public Device()
        {
            //int count = 100;
            //while (count != 0)
            //{
            //    //single.Add(count);
            //    count -= 1;
            //    Console.WriteLine($"Device countdown: " + count);
            //}

            //ReferenceDevice = new ReferenceDevice();
            //Bin = new List<Bin> { new Bin { BinCode = "", BinQuality = "", BinCount = 0, BinDescription = "" },
            //    new Bin { BinCode = "", BinQuality = "", BinCount = 1, BinDescription = "" },
            //    new Bin { BinCode = "", BinQuality = "", BinCount = 2, BinDescription = "" } };
            //SupplierData = new SupplierData { FacilityId = "", FabId = "", Device = "", Wafers = 5 };
            //Data = new Data();

        }
    }
}
