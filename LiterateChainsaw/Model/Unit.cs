using LiterateChainsaw.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace LiterateChainsaw.Model
{
    public class Unit : VMBase
    {
        public Action ResetSelected { get; set; }

        private int _CoordX;
        public int CoordX
        {
            get { return _CoordX; }
            set { _CoordX = value; OnPropertyChanged(nameof(CoordX)); }
        }

        private int _CoordY;
        public int CoordY
        {
            get { return _CoordY; }
            set { _CoordY = value; OnPropertyChanged(nameof(CoordY)); }
        }

        private int _WaferRowCount;
        public int WaferRowCount
        {
            get { return _WaferRowCount; }
            set { _WaferRowCount = value; OnPropertyChanged(nameof(WaferRowCount)); }
        }

        private int _WaferColumnCount;
        public int WaferColumnCount
        {
            get { return _WaferColumnCount; }
            set { _WaferColumnCount = value; OnPropertyChanged(nameof(WaferColumnCount)); }
        }

        private double _OrgX;
        public double OrgX
        {
            get { return _OrgX; }
            set { _OrgX = value; OnPropertyChanged(nameof(OrgX)); }
        }

        private double _OrgY;
        public double OrgY
        {
            get { return _OrgY; }
            set { _OrgY = value; OnPropertyChanged(nameof(OrgY)); }
        }

        private double _Width;
        public double Width
        {
            get { return _Width; }
            set { _Width = value; OnPropertyChanged(nameof(Width)); }
        }

        private double _Height;
        public double Height
        {
            get { return _Height; }
            set { _Height = value; OnPropertyChanged(nameof(Height)); }
        }

        private double _GapX;
        public double GapX
        {
            get { return _GapX; }
            set { _GapX = value; OnPropertyChanged(nameof(GapX)); }
        }

        private double _GapY;
        public double GapY
        {
            get { return _GapY; }
            set { _GapY = value; OnPropertyChanged(nameof(GapY)); }
        }

        private double _WaferWidth;
        public double WaferWidth
        {
            get { return _WaferWidth; }
            set { _WaferWidth = value; OnPropertyChanged(nameof(WaferWidth)); }
        }

        private double _WaferHeight;
        public double WaferHeight
        {
            get { return _WaferHeight; }
            set { _WaferHeight = value; OnPropertyChanged(nameof(WaferHeight)); }
        }

        private Color _Stroke;
        public Color Stroke
        {
            get { return _Stroke; }
            set { _Stroke = value; OnPropertyChanged(nameof(Stroke)); }
        }

        private Color _Fill;
        public Color Fill
        {
            get { return _Fill; }
            set { _Fill = value; OnPropertyChanged(nameof(Fill)); }
        }

        private double _StrokeThickness = 2.0;
        public double StrokeThickness
        {
            get { return _StrokeThickness; }
            set { _StrokeThickness = value; OnPropertyChanged(nameof(StrokeThickness)); }
        }

        private bool _IsManualResult = false;
        public bool IsManualResult
        {
            get { return _IsManualResult; }
            set { _IsManualResult = value; OnPropertyChanged(nameof(IsManualResult)); }
        }

        private bool _IsProcessed = false;
        public bool IsProcessed
        {
            get { return _IsProcessed; }
            set { _IsProcessed = value; OnPropertyChanged(nameof(IsProcessed)); }
        }

        private bool _IsUnitInvalid = false;
        public bool IsUnitInvalid
        {
            get { return _IsUnitInvalid; }
            set { _IsUnitInvalid = value; OnPropertyChanged(nameof(IsUnitInvalid)); }
        }

        private bool _IsVidiFailAsPrimaryDefect;
        public bool IsVidiFailAsPrimaryDefect
        {
            get { return _IsVidiFailAsPrimaryDefect; }
            set { _IsVidiFailAsPrimaryDefect = value; OnPropertyChanged(nameof(IsVidiFailAsPrimaryDefect)); }
        }

        private bool _IsVProFailAsPrimaryDefect;
        public bool IsVProFailAsPrimaryDefect
        {
            get { return _IsVProFailAsPrimaryDefect; }
            set { _IsVProFailAsPrimaryDefect = value; OnPropertyChanged(nameof(IsVProFailAsPrimaryDefect)); }
        }

        //public int CoordX { get; set; }
        //public int CoordY { get; set; }
        //public int WaferRowCount { get; set; }
        //public int WaferColumnCount { get; set; }
        //public double OrgX { get; set; }
        //public double OrgY { get; set; }
        //public double Width { get; set; }
        //public double Height { get; set; }
        //public double GapX { get; set; }
        //public double GapY { get; set; }
        //public double WaferWidth { get; set; }
        //public double WaferHeight { get; set; }
        //public Color Stroke { get; set; }
        //public Color Fill { get; set; }
        public List<Measurements> MeasurementsList { get; set; }// = new List<Measurements>();
        public List<DefectOutputData> DefectOutputDataList { get; set; }// = new List<DefectOutputData>();

        private ICommand _TestCommand1 = new RelayCommand<object>(param => { });
        public ICommand TestCommand1
        {
            get
            {
                return _TestCommand1 ?? (_TestCommand1 = new RelayCommand<object>(param => { TestMethod1(); }));
            }
            set
            {
                _TestCommand1 = value;
            }
        }// { get; }
        public ICommand TestCommand2 { get; }

        public string Found_Vision_Image_Date_Time { get; set; }
        private string _Result = "";
        public string Result
        {
            get { return _Result; }
            set { _Result = value; OnPropertyChanged(nameof(Result)); }
        }
        public string Date_Time { get; set; }
        public string Image_Filepath { get; set; }
        public string Image_Filename { get; set; }

        private string _Primary_Defect_Code = "";
        public string Primary_Defect_Code
        {
            get { return _Primary_Defect_Code; }
            set { _Primary_Defect_Code = value; OnPropertyChanged(nameof(Primary_Defect_Code)); }
        }
        public string Primary_Defect_Desc { get; set; }

        private string _Primary_Defect_Code_Color = "";
        public string Primary_Defect_Code_Color
        {
            get { return _Primary_Defect_Code_Color; }
            set { _Primary_Defect_Code_Color = value; OnPropertyChanged(nameof(Primary_Defect_Code_Color)); }
        }
        public string Secondary_Defect_Code { get; set; }
        public string Secondary_Defect_Desc { get; set; }
        public string Orientation_Result { get; set; }
        public string Map_Origin { get; set; }
        public string Start_Date_Time { get; set; }
        public string End_Date_Time { get; set; }
        public double Base_Height_1 { get; set; }
        public double Base_Height_2 { get; set; }
        public double Part_Height_UL { get; set; }
        public double Part_Height_LL { get; set; }

        public Unit(Action Reset)
        {
            TestCommand1 = new Command(TestMethod1);
            TestCommand2 = new Command(TestMethod2);
            ResetSelected = Reset;

            ////WaferResult
            //Wafer _waferResult = new Wafer() { NAME = "samplewafer", YIELD = 99.9, PASSCOUNT = 100, FAILCOUNT = 2, INVALIDCOUNT = 0, TOTALCOUNT = 2056 };
            //_waferResult.UNITS = new List<UnitResult>();
        }

        private void TestMethod2()
        {
            Console.WriteLine("Wafer Unit TestMethod2()");
        }

        private void TestMethod1()
        {
            this.Stroke = Colors.Gold;
            Console.WriteLine($"Wafer Unit TestMethod1() - OrgX={OrgX}; OrgY={OrgY}; CoordX={CoordX}; CoordY={CoordY}");
            ResetSelected?.Invoke();
        }
    }

    public class Measurements
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
        public double LCL_Value { get; set; }
        public double UCL_Value { get; set; }
    }

    public class DefectOutputData
    {
        public string DefectCode { get; set; }
        public string DefectColor { get; set; }
        public string DrawingType { get; set; }
        public string DrawingData { get; set; }
        public string ObjectType { get; set; }
        public string ObjectData { get; set; }
        public string SpecDefination { get; set; }
    }
}
