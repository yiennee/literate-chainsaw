using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using LiterateChainsaw.Model;
using ZephyrInnovations;
using LiterateChainsaw.Helpers;
using GemBox.Spreadsheet;

namespace LiterateChainsaw.ViewModel
{
    public delegate bool LoadMapDelegate(string LotID, string RecipeID, string CassetteID, string WaferID, string SlotID, string TotalSlot);
    public delegate void ReadWaferInfoDelegate(string waferInfo, bool bRuntime);
    public delegate void CallMethodDelegate();
    public class WaferViewModel : VMBase
    {
        public Wafer WaferResult;// = new Wafer();
        public LoadMapDelegate LoadMapDelegateCallback;
        public ReadWaferInfoDelegate ReadWaferInfoCallback;
        public CallMethodDelegate ClearModelCallback;
        public CallMethodDelegate CompleteRunCallback;
        //map testollection<Wafer.Unit>();
        public IMap MapClass;
        public Action<int, int> OnCanvasSizeSizeChanged;
        public List<Unit> DebugItems { get; set; } = new List<Unit>();
        //private double CurrentZoomScale;
        //private double _zoom = 1;
        //public double Zoom { get => _zoom; set { _zoom = value; OnPropertyChanged(nameof(Zoom)); this.MapReconstruction(value); } }

        private ImageSource _displayImageSource;
        public ImageSource DisplayImageSource
        {
            get => _displayImageSource;
            set
            {
                _displayImageSource = value;
                OnPropertyChanged(nameof(DisplayImageSource));
            }
        }

        private string _currentProcessUnitTitle = string.Format("Unit Coordinate: R{0}-C{1}", "-", "-");
        public string CurrentProcessUnitTitle
        {
            get => _currentProcessUnitTitle;
            set
            {
                _currentProcessUnitTitle = value;
                OnPropertyChanged(nameof(CurrentProcessUnitTitle));
            }
        }
        public void MapReconstruction(double value)
        {
            if (ItemList.Count != 0)
            {
                CanvasWidth = CanvasHeight = 0;
                for (int j = 0; j < DebugItems.Count; j++)
                {
                    //var Unit = Items.FirstOrDefault(x => x.CoordX == j && x.CoordY == i);
                    ItemList[j].OrgX = DebugItems[j].OrgX * value;
                    ItemList[j].OrgY = DebugItems[j].OrgY * value;
                    ItemList[j].Width = DebugItems[j].Width * value;
                    ItemList[j].Height = DebugItems[j].Height * value;
                    ItemList[j].GapX = DebugItems[j].GapX * value;
                    ItemList[j].GapY = DebugItems[j].GapY * value;

                    CanvasWidth = Math.Max(CanvasWidth, Convert.ToInt16(ItemList[j].OrgX + ItemList[j].Width + ItemList[j].GapX));// + Convert.ToInt16(ItemList[0].Width);
                    CanvasHeight = Math.Max(CanvasHeight, Convert.ToInt16(ItemList[j].OrgY + ItemList[j].Height + ItemList[j].GapY));// + Convert.ToInt16(ItemList[0].Height);
                    //Console.WriteLine("Items: " + ItemList[j].OrgX + "," + ItemList[j].OrgY + "," + ItemList[j].Width + "," + ItemList[j].Height);
                    //Console.WriteLine("DebugItems: " + DebugItems[j].OrgX + "," + DebugItems[j].OrgY + "," + DebugItems[j].Width + "," + DebugItems[j].Height);
                    //Console.WriteLine($"CanvasWidth: {CanvasWidth}; CanvasHeight: {CanvasHeight}");
                }
            }

            OnCanvasSizeSizeChanged?.Invoke(CanvasWidth, CanvasHeight);
        }

        private int _canvasWidth;
        public int CanvasWidth { get => _canvasWidth; set { _canvasWidth = value; OnPropertyChanged(nameof(CanvasWidth)); } }//not more than xaml canvas size
        private int _canvasHeight;
        public int CanvasHeight { get => _canvasHeight; set { _canvasHeight = value; OnPropertyChanged(nameof(CanvasHeight)); } }

        private ObservableCollection<Unit> _itemList = new ObservableCollection<Unit>();
        public ObservableCollection<Unit> ItemList
        {
            get => _itemList;
            set { _itemList = value; OnPropertyChanged(nameof(ItemList)); }
        }

        private Unit _selectedUnit;
        public Unit SelectedUnit
        {
            get => _selectedUnit;
            set { _selectedUnit = value; OnPropertyChanged(nameof(SelectedUnit)); }
        }

        private DateTime _startTime;
        public DateTime StartTime
        {
            get => _startTime;
            set { _startTime = value; OnPropertyChanged(nameof(StartTime)); }
        }

        private DateTime _endTime;
        public DateTime EndTime
        {
            get => _endTime;
            set { _endTime = value; OnPropertyChanged(nameof(EndTime)); }
        }

        private TimeSpan _timeTaken;
        public TimeSpan TimeTaken
        {
            get => _timeTaken;
            set { _timeTaken = value; OnPropertyChanged(nameof(TimeTaken)); }
        }


        private int _totalUnitCount;
        public int TotalUnitCount
        {
            get
            {
                //Console.WriteLine($"Wafer TotalUnitCount={DebugItems.Count}");
                return DebugItems.Count;//_totalUnitCount;
            }
            set
            {
                _totalUnitCount = value;

                //if (defectVM == null)
                //{
                //    defectVM = new DefectViewModel();
                //    DefectVM.TotalUnitCount = _totalUnitCount;
                //}

                OnPropertyChanged(nameof(TotalUnitCount));
            }
        }

        private string _lotID;
        public string LotID
        {
            get => _lotID;
            set { _lotID = value; OnPropertyChanged(nameof(LotID)); }
        }

        private string _recipeID;
        public string RecipeID
        {
            get => _recipeID;
            set { _recipeID = value; OnPropertyChanged(nameof(RecipeID)); }
        }

        private string _cassetteID;
        public string CassetteID
        {
            get => _cassetteID;
            set { _cassetteID = value; OnPropertyChanged(nameof(CassetteID)); }
        }

        private string _waferID;
        public string WaferID
        {
            get => _waferID;
            set { _waferID = value; OnPropertyChanged(nameof(WaferID)); }
        }

        private string _slotID;
        public string SlotID
        {
            get => _slotID;
            set { _slotID = value; OnPropertyChanged(nameof(SlotID)); }
        }

        private string _totalSlot;
        public string TotalSlot
        {
            get => _totalSlot;
            set { _totalSlot = value; OnPropertyChanged(nameof(TotalSlot)); }
        }

        private int _inspectedCount;
        public int InspectedCount
        {
            get => _inspectedCount;
            set { _inspectedCount = value; OnPropertyChanged(nameof(InspectedCount)); _currentProcessUnitTitle = string.Format("Unit Coordinate: R{0}-C{1}", "-", "-"); }
        }

        private int _passCount;
        public int PassCount
        {
            get => _passCount;
            set { _passCount = value; OnPropertyChanged(nameof(PassCount)); }
        }

        private int _failCount;
        public int FailCount
        {
            get => _failCount;
            set { _failCount = value; OnPropertyChanged(nameof(FailCount)); }
        }

        private int _invalidCount;
        public int InvalidCount
        {
            get => _invalidCount;
            set { _invalidCount = value; OnPropertyChanged(nameof(InvalidCount)); }
        }

        private double _inspectedCountPerc;
        public double InspectedCountPerc
        {
            get => _inspectedCountPerc;
            set { _inspectedCountPerc = value; OnPropertyChanged(nameof(InspectedCountPerc)); }
        }

        private double _passCountPerc;
        public double PassCountPerc
        {
            get => _passCountPerc;
            set { _passCountPerc = value; OnPropertyChanged(nameof(PassCountPerc)); }
        }

        private double _failCountPerc;
        public double FailCountPerc
        {
            get => _failCountPerc;
            set { _failCountPerc = value; OnPropertyChanged(nameof(FailCountPerc)); }
        }

        private double _invalidCountPerc;
        public double InvalidCountPerc
        {
            get => _invalidCountPerc;
            set { _invalidCountPerc = value; OnPropertyChanged(nameof(InvalidCountPerc)); }
        }

        private double _yield = 100;
        public double Yield
        {
            get => _yield;
            set { _yield = value; OnPropertyChanged(nameof(Yield)); }
        }

        private double _yieldThreshold = 99f;
        public double YieldThreshold
        {
            get => _yieldThreshold;
            set { _yieldThreshold = value; OnPropertyChanged(nameof(YieldThreshold)); }
        }

        public Command TestCommand1 { get; }
        public Command TestCommand2 { get; }

        public Process Process { get; set; }//no use
        public string sWaferInfo { get; set; }
        public string ReportPath { get; set; }
        private DefectViewModel defectVM;
        public DefectViewModel DefectVM //must have binding logic
        {
            get => defectVM; set
            {
                defectVM = value; OnPropertyChanged(nameof(DefectVM));
            }
        }
        public WaferViewModel()
        {
            TestCommand2 = new Command(TestMethod2);
            LoadMapDelegateCallback = new LoadMapDelegate(LoadMap);
            ReadWaferInfoCallback = new ReadWaferInfoDelegate(ReadWaferInfo);
            ClearModelCallback = new CallMethodDelegate(ClearModel);
            CompleteRunCallback = new CallMethodDelegate(CompleteRun);
            ClearModel();

            DefectVM = new DefectViewModel();

        }

        public void ReadWaferInfo(string waferInfo, bool bRuntime = true)
        {
            sWaferInfo = waferInfo;
            string[] WaferInfo = waferInfo.Trim().Split(new char[] { '_' });

            try
            {
                LotID = WaferInfo[0].Trim();
                RecipeID = WaferInfo[1].Trim();
                CassetteID = WaferInfo[2].Trim();
                WaferID = WaferInfo[3].Trim();
                SlotID = WaferInfo[4].Trim();
                TotalSlot = WaferInfo[5].Trim();
                Yield = 100;

                //start load map when wafer-info are read into local
                LoadMap();

                //Note: waferInfo = "30018A_PPIWAFER2_CAS01_1_1_25"
                ReportPath = string.Format(@"{0}\{1}\{2}\{3}\", qGlobal.ReportConfig.ReportFileDirectory, LotID, CassetteID, WaferID);

                if (!bRuntime)//review mode
                    ReadWaferResult();
                else
                    ConfigureReportPath();
            }
            catch (Exception ex)
            {

            }
        }

        public void ConfigureReportPath()
        {
            //always configured to save into 'ReportFileDirectory' set in MainConfig, in folder named as 'LotID\CassetteID\WaferID\'
            string sourceDirectory = /*ReportPath;// */string.Format(@"{0}\{1}\{2}\{3}", qGlobal.ReportConfig.ReportFileDirectory, LotID, CassetteID, WaferID);
            string archiveDirectory = /*ReportPath.Replace(WaferID, "Archive");// */string.Format(@"{0}\{1}\{2}\{3}\", qGlobal.ReportConfig.ReportFileDirectory, LotID, CassetteID, "Archive");

            if (Directory.Exists(sourceDirectory))//if report exists, move into Archive
            {
                DirectoryInfo sourceDir = new DirectoryInfo(sourceDirectory);

                if (!Directory.EnumerateFileSystemEntries(sourceDirectory).Any())
                    return;

                DateTime lastWriteTime = sourceDir.LastWriteTime;
                string timestamp = lastWriteTime.ToString("yyyyMMdd_HHmm");
                string destDirPath = Path.Combine(Path.GetDirectoryName(archiveDirectory), Path.GetFileName(sourceDirectory) + "_" + timestamp);

                ////1) Check if ANY file exists, Move whole directory
                //if (!Directory.Exists(archiveDirectory))
                //    Directory.CreateDirectory(archiveDirectory);
                //Directory.Move(sourceDirectory, destDirPath);//cant create directory for destDirPath if were to use Directory.Move; but directory of destDirPath per se must exist
                ////1) Check if ANY file exists, Move whole directory - end

                if (!Directory.Exists(destDirPath))
                    Directory.CreateDirectory(destDirPath);

                // Get all files in the source directory
                FileInfo[] allFiles = sourceDir.GetFiles();
                string[] excludedExtensions = new string[] { ".bmp", ".tif", ".png" };

                // Get files to move to the destination directory
                FileInfo[] filesToMove = allFiles
                    .Where(file => !excludedExtensions.Contains(file.Extension.ToLower()))
                    .ToArray();

                // Move each file except image to the destination directory (Archive\WaferID_DateTime)
                foreach (FileInfo file in filesToMove)
                {
                    string destFilePath = Path.Combine(destDirPath, file.Name);//path.combine auto add '\\' btw path1 and path2
                    file.MoveTo(destFilePath);
                }

                // Handle remaining image files - Delete/Move
                FileInfo[] remainingFiles = allFiles.Except(filesToMove).ToArray();
                foreach (FileInfo file in remainingFiles)
                {
                    file.Delete();

                    // //move option
                    // string imgDestDirPath = destDirPath;
                    // string destFilePath = Path.Combine(imgDestDirPath, file.Name);
                    // file.MoveTo(destFilePath);
                }
            }
            else
                Directory.CreateDirectory(sourceDirectory);
        }

        public void CompleteRun()
        {
            #region Save Report
            Stopwatch Watch = Stopwatch.StartNew();

            EndTime = DateTime.Now;
            TimeTaken = EndTime - StartTime;
            GenerateReport();

            Watch.Stop();
            //logMsg = string.Format("Process[{0}] WaferEndTime={1}; TimeTaken={2}", CurrentWaferID, Wafer.EndTime.ToString("HH:mm:ss.fff"), Wafer.TimeTaken.ToString("g"));
            string logMsg = string.Format("SaveReport[TimeTaken] {0}", Watch.ElapsedMilliseconds.ToString());
            qGlobal.WriteRunLog(true, logMsg, "INFO");
            #endregion

            #region Save Map
            Watch.Reset(); Watch.Start();

            SaveMap();

            Watch.Stop();
            logMsg = string.Format("SaveMap[TimeTaken] {0}", Watch.ElapsedMilliseconds.ToString());
            qGlobal.WriteRunLog(true, logMsg, "INFO");
            #endregion

            ////move image
            //Thread MoveImageThread = new Thread(() => StartMoveImage(CurrentWaferID));
            //MoveImageThread.Start();

            ClearModelCallback();//reset


        }

        public void ReadWaferResult()
        {
            string xmlPath = string.Format("{0}{1}{2}", ReportPath, sWaferInfo, ".xml");//@"D:\Report\30018A\CAS01\1\FILE.xml" where FILE can be configured as 'summary.xml' as well
            if (!File.Exists(xmlPath))
                return;

            if (ItemList.Count == 0)
                return;

            while (FileStreamer.CheckFileLock(xmlPath, FileAccess.ReadWrite))
                Thread.Sleep(100);

            string strErrMsg;
            bool IsSuccess = Serialization.Deserialize(xmlPath, typeof(Wafer), out object waferObj, out strErrMsg);

            if (!IsSuccess || waferObj == null)
                return;

            WaferResult = (Wafer)waferObj;

            for (int i = 0; i < ItemList.Count; i++)
            {
                UnitResult inspResult = WaferResult.UNITS.List.FirstOrDefault(p =>
                {
                    return p.ID == ItemList[i].Image_Filename;
                });

                if (inspResult != null)
                {
                    //ItemList[i].Image_Filename = inspResult.ID;
                    ItemList[i].Result = inspResult.RESULT;
                    ItemList[i].Primary_Defect_Code = inspResult.PRIMARY_DEFECT_CODE;
                    ItemList[i].Secondary_Defect_Code = inspResult.SECONDARY_DEFECT_CODES;
                    ItemList[i].IsVProFailAsPrimaryDefect = inspResult.IS_VPRO_FAIL_AS_PRIMARY;
                    ItemList[i].IsVidiFailAsPrimaryDefect = inspResult.IS_VIDI_FAIL_AS_PRIMARY;
                    ItemList[i].IsManualResult = inspResult.IS_MANUAL_RESULT;
                }
            }
        }

        public void SaveWaferResult()
        {
            if (ItemList.Count == 0)
                return;

            //serialize WaferResult after wafer complete, or after review
            WaferResult = new Wafer
            {
                NAME = _waferID,
                YIELD = _yield,
                PASSCOUNT = _passCount,
                FAILCOUNT = _failCount,
                INVALIDCOUNT = _invalidCount,
                TOTALCOUNT = _totalUnitCount
            };

            for (int i = 0; i < ItemList.Count; i++)
            {
                var runtimeUnit = ItemList[i];

                if (runtimeUnit != null)
                {
                    UnitResult unit = new UnitResult
                    {
                        ID = runtimeUnit.Image_Filename,
                        RESULT = runtimeUnit.Result,
                        PRIMARY_DEFECT_CODE = runtimeUnit.Primary_Defect_Code,
                        SECONDARY_DEFECT_CODES = runtimeUnit.Secondary_Defect_Code,
                        IS_VPRO_FAIL_AS_PRIMARY = runtimeUnit.IsVProFailAsPrimaryDefect,
                        IS_VIDI_FAIL_AS_PRIMARY = runtimeUnit.IsVidiFailAsPrimaryDefect,
                        IS_MANUAL_RESULT = runtimeUnit.IsProcessed
                    };

                    WaferResult.UNITS.List.Add(unit);
                }
            }
        }

        public void GenerateReport()
        {
            try
            {
                string reportTemplatePath = string.Format(@"{0}\{1}", qGlobal.ReportConfig.ReportFileDirectory, "PPI_ReportTemplate1.xlsx");
                string bmpPath = string.Format("{0}{1}{2}", ReportPath, "WaferMap", ".bmp");
                string pdfPath = string.Format("{0}{1}{2}", ReportPath, sWaferInfo, ".pdf");
                string xmlPath = string.Format("{0}{1}{2}", ReportPath, sWaferInfo, ".xml");

                SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
                ExcelFile workbook = ExcelFile.Load(reportTemplatePath);
                ExcelWorksheet worksheet = workbook.Worksheets[0];

                #region Assign ExcelCells
                ExcelCell exWaferID = worksheet.Cells["B7"];
                ExcelCell exPerformance = worksheet.Cells["E7"];
                ExcelCell exMultiThread = worksheet.Cells["H7"];
                ExcelCell exThreadCount = worksheet.Cells["K7"];
                ExcelCell exDeepLearning = worksheet.Cells["N7"];
                ExcelCell exYield = worksheet.Cells["Q7"];

                ExcelCell exStartTime = worksheet.Cells["B10"];
                ExcelCell exEndTime = worksheet.Cells["H10"];
                ExcelCell exTimeElapsed = worksheet.Cells["N10"];

                ExcelCell exPassCounter = worksheet.Cells["Q14"];
                ExcelCell exFailCounter = worksheet.Cells["Q15"];
                ExcelCell exInspectedCounter = worksheet.Cells["Q16"];
                ExcelCell exInvalidCounter = worksheet.Cells["Q17"];
                ExcelCell exPassPercentage = worksheet.Cells["R14"];
                ExcelCell exFailPercentage = worksheet.Cells["R15"];
                ExcelCell exInspectedPercentage = worksheet.Cells["R16"];
                ExcelCell exInvalidPercentage = worksheet.Cells["R17"];
                ExcelCell exTotalUnitNo = worksheet.Cells["Q18"];

                ExcelCell exLotID = worksheet.Cells["Q21"];
                ExcelCell exRecipeID = worksheet.Cells["Q22"];
                ExcelCell exCassetteID = worksheet.Cells["Q23"];
                ExcelCell exSlotID = worksheet.Cells["Q24"];
                ExcelCell exTotalSlot = worksheet.Cells["Q25"];
                #endregion

                //append lot info
                exWaferID.Value = WaferID;
                exPerformance.Value = Convert.ToInt16(3600000 / TimeTaken.TotalMilliseconds).ToString("0.00") + "WPH";
                exPerformance.Value = Convert.ToDouble(3600000 / (TimeTaken.TotalMilliseconds / InspectedCount) * 0.001f).ToString("0.00") + "k UPH";
                exMultiThread.Value = qGlobal.VisionProcessorConfig.EnableMultiThread.ToString();
                exThreadCount.Value = qGlobal.VisionProcessorConfig.MaxDegreeofParallelism.ToString();
                exDeepLearning.Value = qGlobal.VisionProcessorConfig.EnableVidi.ToString();
                exYield.Value = Yield.ToString("0.00");

                exStartTime.Value = StartTime.ToString("HH:mm:ss.fff");
                exEndTime.Value = EndTime.ToString("HH:mm:ss.fff");
                exTimeElapsed.Value = TimeTaken.ToString("g");

                exPassCounter.Value = PassCount;
                exFailCounter.Value = FailCount;
                exInspectedCounter.Value = InspectedCount;
                exInvalidCounter.Value = InvalidCount;
                exPassPercentage.Value = PassCountPerc.ToString("0.00");
                exFailPercentage.Value = FailCountPerc.ToString("0.00");
                exInspectedPercentage.Value = InspectedCountPerc.ToString("0.00");
                exInvalidPercentage.Value = InvalidCountPerc.ToString("0.00");
                exTotalUnitNo.Value = TotalUnitCount;

                exLotID.Value = LotID;
                exRecipeID.Value = RecipeID;
                exCassetteID.Value = CassetteID;
                exSlotID.Value = SlotID;
                exTotalSlot.Value = TotalSlot;

                //append defect classification
                int defectCount = 100;// ClassificationViewModel.DefectList.Count();
                for (int i = 0; i < defectCount; i++)
                {
                    string rowindex;
                    string Defect;
                    string DefectColor;
                    string DefectCode;
                    string Qty_combined;
                    string Perc;
                    string Qty_VPro;
                    string Qty_Vidi;
                    if (i >= 1)
                    {
                        // Insert a copy of the initial row below the last used row.
                        worksheet.Rows.InsertCopy(28 + i, worksheet.Rows[27 + i]);
                        rowindex = Convert.ToString(29 + i);
                        Defect = "B" + rowindex;
                        DefectColor = "J" + rowindex;
                        DefectCode = "K" + rowindex;
                        Qty_combined = "M" + rowindex;
                        Perc = "P" + rowindex;
                        Qty_VPro = "Q" + rowindex;
                        Qty_Vidi = "R" + rowindex;
                    }
                    else
                    {
                        Defect = "B" + Convert.ToString(29 + i);
                        DefectColor = "J" + Convert.ToString(29 + i);
                        DefectCode = "K" + Convert.ToString(29 + i);
                        Qty_combined = "M" + Convert.ToString(29 + i);
                        Perc = "P" + Convert.ToString(29 + i);
                        Qty_VPro = "Q" + Convert.ToString(29 + i);
                        Qty_Vidi = "R" + Convert.ToString(29 + i);
                    }
                    //worksheet.Cells[Defect].Value = ClassificationViewModel.DefectList[i].defectCodeData.DEFECT_NAME;
                    //worksheet.Cells[DefectCode].Value = ClassificationViewModel.DefectList[i].defectCodeData.DEFECT_CODE;
                    //worksheet.Cells[Qty_combined].Value = ClassificationViewModel.DefectList[i].defectCount; ;
                    //worksheet.Cells[Perc].Value = ClassificationViewModel.DefectList[i].defectPerc.ToString("0.00");
                    //worksheet.Cells[Qty_VPro].Value = ClassificationViewModel.DefectList[i].VProCount;
                    //worksheet.Cells[Qty_Vidi].Value = ClassificationViewModel.DefectList[i].VidiCount;

                    //request 05122023
                    string defectCode = worksheet.Cells[DefectCode].Value.ToString();
                    var defect = qGlobal.DefectCodeDataList.FirstOrDefault(q => q.DEFECT_CODE == defectCode);
                    string defectColorCode = defect.DEFECT_CODE_COLOR;
                    if (!defectColorCode.StartsWith("#"))
                        defectColorCode = "#" + defectColorCode;
                    var color = System.Drawing.ColorTranslator.FromHtml(defectColorCode);
                    worksheet.Cells[DefectColor].Style.FillPattern.SetSolid(color);
                }

                // Read the image file as a byte array.
                byte[] imageBytes = File.ReadAllBytes(bmpPath);
                // Load the image from the file.
                using (var imageStream = new MemoryStream(imageBytes))
                {
                    // Insert the image into the worksheet.
                    worksheet.Pictures.Add(imageStream, ExcelPictureFormat.Bmp, 12, 305, 370, 370, LengthUnit.Pixel);
                    // Save the modified workbook to a new Excel file.
                    workbook.Save(pdfPath);
                }

                //SaveWaferResult();
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Wafer(GenerateReport): {0}", ex.ToString());
                qGlobal.WriteRunLog(true, errMsg, "ERROR");
            }
        }

        public void UpdateMapSize()
        {
            //double Zoom = 1;
            //double Canvas_Width = 630;// * Zoom;
            //double Canvas_Height = 630;// * Zoom;

            //double scale3 = Canvas_Width / ((Width + GapX) * WaferColumnCount);// Convert.ToDouble();//hardcode to UI canvas width
            //double scale4 = Canvas_Height / ((Height + GapY) * WaferRowCount);//hardcode to UI canvas height
            //double dScale = Math.Min(scale3, scale4);
        }



        //wafervm old ctor
        public WaferViewModel(Action<int, int> CanvasSizeChange)
        {
            TestCommand2 = new Command(TestMethod2);
            LoadMapDelegateCallback = new LoadMapDelegate(LoadMap);
            ClearModelCallback = new CallMethodDelegate(ClearModel);
            ClearModel();
            OnCanvasSizeSizeChanged = CanvasSizeChange;
        }

        private bool LoadWaferMap(string WaferID)
        {
            #region Load Map
            string logMsg = "";
            string errMsg = "";
            string fileName = string.Format("{0}", WaferID);//30018A_A268B_TEST01_2_1
            string filePath = qGlobal.WaferMapConfig.LoadMapFilesDirectory;

            string MapFile = string.Format("{0}{1}.xml", filePath, fileName);//30018A_A268B_TEST01_2_1.xml

            if (!File.Exists(MapFile))
            {
                errMsg = string.Format("No matching Map File. File path= {0}; Please check if map file exists.", MapFile);
                qGlobal.WriteRunLog(true, errMsg, "ERROR");
                return false;
            }

            logMsg = string.Format("Check Map Type: {0}", qGlobal.WaferMapConfig.InputMapType);
            qGlobal.WriteRunLog(true, logMsg, "INFO");

            //Set MapClass instance to null
            if (MapClass != null)
                MapClass = null;

            //Select Map Type instance for MapClass
            switch (qGlobal.WaferMapConfig.InputMapType)
            {
                case qGlobal.MapType.XMLMap_SemiG85:
                    MapClass = new XMLMap_SemiG85();
                    break;
                case qGlobal.MapType.Klarf1_2:
                    MapClass = new KlarfMap();
                    break;
            }

            //Check MapClass Instance status
            if (MapClass == null)
            {
                errMsg = string.Format("No matching Map Type. Current Input Map Type = {0}; Please check Input Map Type.", qGlobal.WaferMapConfig.InputMapType);
                qGlobal.WriteRunLog(true, errMsg, "ERROR");

                return false;
            }

            logMsg = string.Format("Read Map. File = {0}{1}.xml", filePath, fileName);
            qGlobal.WriteRunLog(true, logMsg, "INFO");

            List<Unit> LoadedMapParam = new List<Unit>();
            XMLUtils xmlUtils = new XMLUtils();
            object myObj = xmlUtils.LoadXMLData(MapClass, filePath, fileName, out errMsg);

            if (myObj == null)
            {
                //errMsg = string.Format("Failed to load map file. Map Type = {0}; File = {1}", qGlobal.WaferMapConfig.InputMapType, fileName);
                qGlobal.WriteRunLog(true, errMsg, "ERROR");
                return false;
            }
            else
            {
                switch (qGlobal.WaferMapConfig.InputMapType)
                {
                    case qGlobal.MapType.XMLMap_SemiG85:
                        MapClass = (XMLMap_SemiG85)myObj;
                        break;
                    case qGlobal.MapType.Klarf1_2:
                        MapClass = (KlarfMap)myObj;
                        break;
                }

                LoadedMapParam = MapClass.Load(out errMsg);

                if (errMsg != "")
                {
                    qGlobal.WriteRunLog(true, errMsg, "ERROR");
                    return false;
                }
            }

            logMsg = string.Format("Acquisition of Map Info");
            qGlobal.WriteRunLog(true, logMsg, "INFO");

            /*ReadOnlyDebugItems = */
            DebugItems = LoadedMapParam;
            //Items = LoadedMapParam;//unable to convert implicitly
            TotalUnitCount = DebugItems.Count;

            //var Item = DebugItems.FindLast(x => true);
            CanvasWidth = 550;// Convert.ToInt16(Item.OrgX + Item.Width + Item.GapX);
            CanvasHeight = 550;// Convert.ToInt16(Item.OrgY + Item.Height + Item.GapY);

            //This type of CollectionView does not support changes to its SourceCollection from a thread different from the Dispatcher thread
            if (ItemList != null)
                ItemList.Clear();

            //Zoom = /*ZoomScale = */CurrentZoomScale = 1;

            //int count = 1;
            for (int i = 0; i < LoadedMapParam.Count; i++)
            {
                var temp = LoadedMapParam[i];
                //Console.WriteLine($"Current Coord: X: " + temp.CoordX + "; Y: " + temp.CoordY);
                //Console.WriteLine($"Count = " + count + " | X: " + temp.OrgX + "; Y: " + temp.OrgY + "; Width: " + temp.Width + "; Height: " + temp.Height);
                //count++;
                if (temp != null)
                {
                    ItemList.Add(new Unit(ResetSelectedUnit)
                    {
                        OrgX = temp.OrgX * 1.8,
                        OrgY = temp.OrgY * 1.8,
                        Width = temp.Width * 1.8,
                        Height = temp.Height * 1.8,
                        GapX = temp.GapX * 1.8,
                        GapY = temp.GapY * 1.8,
                        Stroke = temp.Fill,
                        Fill = temp.Fill,
                        CoordX = temp.CoordX,
                        CoordY = temp.CoordY,
                        WaferWidth = temp.WaferWidth,
                        WaferHeight = temp.WaferHeight,
                        WaferRowCount = temp.WaferRowCount,
                        WaferColumnCount = temp.WaferColumnCount
                    });
                }
            }
            #endregion

            //LotID = iLotID;
            //RecipeID = iRecipeID;
            //CassetteID = iCassetteID;
            //WaferID = iWaferID;
            //SlotID = iSlotID;
            //TotalSlot = iTotalSlot;
            return true;
        }

        private void SetImage(string value)
        {
        }

        public void ClearModel()
        {
            if (DebugItems != null)
                DebugItems.Clear();

            if (ItemList != null)
                ItemList.Clear();

            if (DisplayImageSource != null)
                DisplayImageSource = null;

            if (DefectVM != null)
                DefectVM = null;

            SelectedUnit = null;
            LotID = RecipeID = CassetteID = WaferID = SlotID = TotalSlot = ReportPath = "";
            InspectedCount = PassCount = FailCount = InvalidCount = TotalUnitCount = 0;
            InspectedCountPerc = PassCountPerc = FailCountPerc = InvalidCountPerc = Yield = 0;

            _currentProcessUnitTitle = string.Format("Unit Coordinate: R{0}-C{1}", "-", "-");
        }

        public void ResetSelectedUnit()
        {
            //SelectedUnit = null;
            //var unit = ItemList.FirstOrDefault(x => { return x.CoordX == qGlobal.SelectedCoordY && x.CoordY == qGlobal.SelectedCoordX; });

            //if (unit != null)
            //{
            //    SelectedUnit = unit;
            //    //Console.WriteLine($"Selected Wafer Item: CoordX={SelectedUnit.CoordX}; CoordY={SelectedUnit.CoordY}");
            //    CurrentProcessUnitTitle = string.Format("Unit Coordinate: R{0}-C{1}", SelectedUnit.CoordX, SelectedUnit.CoordY);
            //}
        }

        public bool SaveMap()//string iLotID, string iRecipeID, string iCassetteID, string iWaferID, string iSlotID, string iTotalSlot)
        {
            string logMsg = "";
            string errMsg = "";
            //string fileName = string.Format("{0}", WaferID);//30018A_A268B_TEST01_2_1
            string fileName = string.Format("{0}_{1}_{2}_{3}_{4}", LotID, CassetteID, WaferID, SlotID, TotalSlot);
            string filePath = qGlobal.WaferMapConfig.SaveMapFilesDirectory;

            string MapFile = string.Format("{0}{1}.xml", filePath, fileName);

            if (!Directory.Exists(System.IO.Path.GetDirectoryName(MapFile)))
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(MapFile));

            if (MapClass == null)
            {
                logMsg = string.Format("Map Type instance [{0}]=null", qGlobal.WaferMapConfig.InputMapType);
                qGlobal.WriteRunLog(true, logMsg, "INFO");
                return false;
            }

            Stopwatch Watch = new Stopwatch();
            Watch.Reset();
            Watch.Start();

            #region Save Map
            List<Unit> ListItem = new List<Unit>(ItemList);
            bool success = MapClass.Save(ListItem, filePath, fileName, out errMsg);

            if (success == true)
            {
                logMsg = string.Format("Saved Map. Map Type={0}; File = {1}", qGlobal.WaferMapConfig.InputMapType, MapFile);
                qGlobal.WriteRunLog(true, logMsg, "INFO");
            }
            else
            {
                errMsg = string.Format("Failed to Save Map. Map Type={0}; File = {1}", qGlobal.WaferMapConfig.InputMapType, MapFile);
                qGlobal.WriteRunLog(true, errMsg, "ERROR");
            }
            #endregion

            Watch.Stop();

            return success;
        }

        /// <summary>
        /// Load Map File from directory: qGlobal.WaferMapConfig.LoadMapFilesDirectory
        /// </summary>
        /// <param name="iLotID"></param>
        /// <param name="iRecipeID"></param>
        /// <param name="iCassetteID"></param>
        /// <param name="iWaferID"></param>
        /// <param name="iSlotID"></param>
        /// <param name="iTotalSlot"></param>
        /// <returns></returns>
        public bool LoadMap(string iLotID, string iRecipeID, string iCassetteID, string iWaferID, string iSlotID, string iTotalSlot)
        {
            #region Load Map
            string logMsg = "";
            string errMsg = "";
            string fileName = iCassetteID;// string.Format("{0}_{1}_{2}_{3}_{4}", iLotID, iCassetteID, iWaferID, iSlotID, iTotalSlot);//"LOT01_M01_LF01_1_1";
            string filePath = qGlobal.WaferMapConfig.LoadMapFilesDirectory;

            string MapFile = string.Format("{0}{1}.xml", filePath, fileName);

            if (!File.Exists(MapFile))
            {
                errMsg = string.Format("No matching Map File. File path= {0}; Please check if map file exists.", MapFile);
                qGlobal.WriteRunLog(true, errMsg, "ERROR");
                return false;
            }

            logMsg = string.Format("Check Map Type: {0}", qGlobal.WaferMapConfig.InputMapType);
            qGlobal.WriteRunLog(true, logMsg, "INFO");

            //Set MapClass instance to null
            if (MapClass != null)
                MapClass = null;

            //Select Map Type instance for MapClass
            switch (qGlobal.WaferMapConfig.InputMapType)
            {
                case qGlobal.MapType.XMLMap_SemiG85:
                    MapClass = new XMLMap_SemiG85();
                    break;
                case qGlobal.MapType.Klarf1_2:
                    MapClass = new KlarfMap();
                    break;
            }

            //Check MapClass Instance status
            if (MapClass == null)
            {
                errMsg = string.Format("No matching Map Type. Current Input Map Type = {0}; Please check Input Map Type.", qGlobal.WaferMapConfig.InputMapType);
                qGlobal.WriteRunLog(true, errMsg, "ERROR");

                return false;
            }

            logMsg = string.Format("Read Map. File = {0}{1}.xml", filePath, fileName);
            qGlobal.WriteRunLog(true, logMsg, "INFO");

            List<Unit> LoadedMapParam = new List<Unit>();
            XMLUtils xmlUtils = new XMLUtils();
            object myObj = xmlUtils.LoadXMLData(MapClass, filePath, fileName, out errMsg);

            if (myObj == null)
            {
                //errMsg = string.Format("Failed to load map file. Map Type = {0}; File = {1}", qGlobal.WaferMapConfig.InputMapType, fileName);
                qGlobal.WriteRunLog(true, errMsg, "ERROR");
                return false;
            }
            else
            {
                switch (qGlobal.WaferMapConfig.InputMapType)
                {
                    case qGlobal.MapType.XMLMap_SemiG85:
                        MapClass = (XMLMap_SemiG85)myObj;
                        break;
                    case qGlobal.MapType.Klarf1_2:
                        MapClass = (KlarfMap)myObj;
                        break;
                }

                LoadedMapParam = MapClass.Load(out errMsg);

                if (errMsg != "")
                {
                    qGlobal.WriteRunLog(true, errMsg, "ERROR");
                    return false;
                }
            }

            logMsg = string.Format("Acquisition of Map Info");
            qGlobal.WriteRunLog(true, logMsg, "INFO");

            /*ReadOnlyDebugItems = */
            DebugItems = LoadedMapParam;//get from loaded map
            //Items = LoadedMapParam;//unable to convert implicitly
            TotalUnitCount = DebugItems.Count;

            //var Item = DebugItems.FindLast(x => true);
            CanvasWidth = 800;// Convert.ToInt16(Item.OrgX + Item.Width + Item.GapX);
            CanvasHeight = 800;// Convert.ToInt16(Item.OrgY + Item.Height + Item.GapY);

            //This type of CollectionView does not support changes to its SourceCollection from a thread different from the Dispatcher thread
            if (ItemList != null)
                ItemList.Clear();

            //get the canvas size for scale e.g. 630
            int Scale = 1;

            //Zoom = /*ZoomScale = */CurrentZoomScale = 1;

            //int count = 1;
            for (int i = 0; i < LoadedMapParam.Count; i++)
            {
                var temp = LoadedMapParam[i];
                //Console.WriteLine($"Current Coord: X: " + temp.CoordX + "; Y: " + temp.CoordY);
                //Console.WriteLine($"Count = " + count + " | X: " + temp.OrgX + "; Y: " + temp.OrgY + "; Width: " + temp.Width + "; Height: " + temp.Height);
                //count++;
                if (temp != null)
                {
                    ItemList.Add(new Unit(ResetSelectedUnit)
                    {
                        OrgX = temp.OrgX * Scale,
                        OrgY = temp.OrgY * Scale,
                        Width = temp.Width * Scale,
                        Height = temp.Height * Scale,
                        GapX = temp.GapX * Scale,
                        GapY = temp.GapY * Scale,
                        Stroke = temp.Fill,
                        Fill = temp.Fill,
                        CoordX = temp.CoordX,
                        CoordY = temp.CoordY,
                        WaferWidth = temp.WaferWidth,
                        WaferHeight = temp.WaferHeight,
                        WaferRowCount = temp.WaferRowCount,
                        WaferColumnCount = temp.WaferColumnCount
                    });
                }
            }
            #endregion

            LotID = iLotID;
            RecipeID = iRecipeID;
            CassetteID = iCassetteID;
            WaferID = iWaferID;
            SlotID = iSlotID;
            TotalSlot = iTotalSlot;
            return true;
        }

        public void LoadMap()
        {
            string fileName = CassetteID;// string.Format("{0}_{1}_{2}_{3}_{4}", iLotID, iCassetteID, iWaferID, iSlotID, iTotalSlot);//"LOT01_M01_LF01_1_1";
            string filePath = qGlobal.WaferMapConfig.LoadMapFilesDirectory;
            string MapFile = string.Format("{0}{1}.xml", filePath, fileName);

            string errMsg = ""; string logMsg = "";
            if (!File.Exists(MapFile))
            {
                errMsg = string.Format("No matching Map File. File path= {0}; Please check if map file exists.", MapFile);
                qGlobal.WriteRunLog(true, errMsg, "ERROR");
            }

            logMsg = string.Format("Check Map Type: {0}", qGlobal.WaferMapConfig.InputMapType);
            qGlobal.WriteRunLog(true, logMsg, "INFO");

            //Set MapClass instance to null
            if (MapClass != null)
                MapClass = null;

            //Select Map Type instance for MapClass
            switch (qGlobal.WaferMapConfig.InputMapType)
            {
                case qGlobal.MapType.XMLMap_SemiG85:
                    MapClass = new XMLMap_SemiG85();
                    break;
                case qGlobal.MapType.Klarf1_2:
                    MapClass = new KlarfMap();
                    break;
            }

            //Check MapClass Instance status
            if (MapClass == null)
            {
                errMsg = string.Format("No matching Map Type. Current Input Map Type = {0}; Please check Input Map Type.", qGlobal.WaferMapConfig.InputMapType);
                qGlobal.WriteRunLog(true, errMsg, "ERROR");
            }

            logMsg = string.Format("Read Map. File = {0}{1}.xml", filePath, fileName);
            qGlobal.WriteRunLog(true, logMsg, "INFO");

            List<Unit> LoadedMapParam = new List<Unit>();
            XMLUtils xmlUtils = new XMLUtils();
            object myObj = xmlUtils.LoadXMLData(MapClass, filePath, fileName, out errMsg);

            if (myObj == null)
            {
                //errMsg = string.Format("Failed to load map file. Map Type = {0}; File = {1}", qGlobal.WaferMapConfig.InputMapType, fileName);
                qGlobal.WriteRunLog(true, errMsg, "ERROR");
            }
            else
            {
                switch (qGlobal.WaferMapConfig.InputMapType)
                {
                    case qGlobal.MapType.XMLMap_SemiG85:
                        MapClass = (XMLMap_SemiG85)myObj;
                        break;
                    case qGlobal.MapType.Klarf1_2:
                        MapClass = (KlarfMap)myObj;
                        break;
                }

                LoadedMapParam = MapClass.Load(out errMsg);

                if (errMsg != "")
                {
                    qGlobal.WriteRunLog(true, errMsg, "ERROR");
                }
            }

            logMsg = string.Format("Acquisition of Map Info");
            qGlobal.WriteRunLog(true, logMsg, "INFO");

            /*ReadOnlyDebugItems = */
            DebugItems = LoadedMapParam;//get from loaded map
            //Items = LoadedMapParam;//unable to convert implicitly
            TotalUnitCount = DebugItems.Count;

            //This type of CollectionView does not support changes to its SourceCollection from a thread different from the Dispatcher thread
            if (ItemList != null)
                ItemList.Clear();

            //get the canvas size for scale e.g. 630
            int Scale = 630;
            for (int i = 0; i < LoadedMapParam.Count; i++)
            {
                var temp = LoadedMapParam[i];
                if (temp != null)
                {
                    ItemList.Add(new Unit(ResetSelectedUnit)
                    {
                        OrgX = temp.OrgX * Scale,
                        OrgY = temp.OrgY * Scale,
                        Width = temp.Width * Scale,
                        Height = temp.Height * Scale,
                        GapX = temp.GapX * Scale,
                        GapY = temp.GapY * Scale,
                        Stroke = temp.Fill,
                        Fill = temp.Fill,
                        CoordX = temp.CoordX,
                        CoordY = temp.CoordY,
                        WaferWidth = temp.WaferWidth,
                        WaferHeight = temp.WaferHeight,
                        WaferRowCount = temp.WaferRowCount,
                        WaferColumnCount = temp.WaferColumnCount
                    });
                }
            }

            //runtime load map successful
            //StartTime = DateTime.Now;//300424
        }

        private void TestMethod2()
        {
            Console.WriteLine("Wafer View Model TestMethod2()");
        }

        private void TestMethod1()
        {
            Console.WriteLine("Wafer View Model TestMethod1()");
        }

        public async Task<string> SetWaferParamValue_Async(string TCPIPCommand, string c, string wde, string dfg)
        {
            string result = "";
            /*var result = */
            await Task.Run(() =>
            {
                //set param
                Console.WriteLine("WHAT I DO IN WAFER VIEW/// " + TCPIPCommand + " *Sleep 1000");
                Thread.Sleep(1000);

                //return "Async*** " + TCPIPCommand;
            });

            return result;
        }
    }
}
