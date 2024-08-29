using Cognex.VisionPro;
using Cognex.VisionPro.Blob;
using Cognex.VisionPro.ImageFile;
using LiterateChainsaw.Helpers;
using LiterateChainsaw.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ZephyrInnovations;

namespace LiterateChainsaw
{
    public delegate void SetParamValueDelegate(string value);
    public class MainViewModel : VMBase
    {
        public SetParamValueDelegate SetRunStatusCallback;
        public string VisionEngineState
        {
            get => _visionEngineState;
            set
            {
                _visionEngineState = value;
                OnPropertyChanged(nameof(VisionEngineState));
            }
        }
        public string CurrentDateTime
        {
            get
            {
                return _CurrentDateTime;
            }
            set
            {
                _CurrentDateTime = DateTime.Now.ToString("MMMM dd, yyyy, hh:mm:ss");
                OnPropertyChanged("CurrentDateTime");
            }
        }
        public ICommand CMDStartRun { get { return _CMDStartRun ?? (_CMDStartRun = new RelayCommand<object>(execute => StartRun())); } }

        private void StartRun()
        {
            //ProcessRun.function();
            Global.uc.StartLoop_TimerTask();
            Global.uc.StartLoopAsync();

            string command = "STARTRUN";
            #region STARTRUN
            qGlobal.WriteRunLog(true, "MANUAL -> " + "Check All Communication Path", "INFO");
            qGlobal.WriteRunLog(true, "MANUAL -> " + "Check Software dongle", "INFO");

            Application.Current.Dispatcher.BeginInvoke(SetRunStatusCallback, new object[] { command });//offline CMD set Vision Engine state
            if (IsSTARTRUN == false)
            {
                IsSTARTRUN = true;
                ProcessRun.StartRunState();//activate Process.StartRun()

                logMsg = string.Format("{0}", "STATUS=STARTRUN");
                qGlobal.WriteRunLog(true, "MANUAL -> " + logMsg, "INFO");
                //Application.Current.Dispatcher.BeginInvoke(SetRunStatusCallback, new object[] { "START" });//offline CMD set Vision Engine state //wafervm cmm
            }
            #endregion
        }

        public ICommand CMDAbort { get { return _CMDAbort ?? (_CMDAbort = new RelayCommand<object>(execute => Abort())); } }

        public void Abort()
        {
            Global.uc.StopLoop_TimerTask();

            //SaveCanvasToImage();
            string command = "ABORT";

            IsSTARTRUN = false;
            ProcessRun.EndRunState();//deactivate Process.StartRun()
            ProcessRun.Abort();//clear process info

            logMsg = string.Format("{0}", "STATUS=ABORT");
            qGlobal.WriteRunLog(true, "MANUAL -> " + logMsg, "INFO");

            Application.Current.Dispatcher.BeginInvoke(SetRunStatusCallback, new object[] { command });//offline CMD set Vision Engine state
        }

        public ICommand CMDLoadRecipe { get { return _CMDLoadRecipe ?? (_CMDLoadRecipe = new RelayCommand<object>(execute => LoadRecipe())); } }

        private void LoadRecipe()
        {

        }
        public ICommand CMDConfigureIN { get { return _CMDConfigureIN ?? (_CMDConfigureIN = new RelayCommand<object>(execute => ConfigureIN())); } }

        private void ConfigureIN(bool IsRetry=false)
        {
            Task.Run(()=>
            {
                OpenImage();
            });
        }

        private void OpenImage(bool IsRetry = false)
        {
            CogImageFile cogimagefile = new CogImageFile();
            CogImageFileTool cogimagefiletool = new CogImageFileTool();
            string status = "ReadImage";
            object ICogImage = new object();
            int mstimeout = 2000;

            try
            {
                //using (CogImageFile ImgFile = new CogImageFile())
                //{
                //    try
                //    {
                //        ImgFile.Open(imagefile, CogImageFileModeConstants.Read);
                //        CogImageFileTool ImgFileTool = new CogImageFileTool();
                //        ImgFileTool.Operator = ImgFile;
                //        ImgFileTool.Run();
                //    }
                //    catch (Exception ex)
                //    {
                //        //MessageBox.Show($"Open Image Error : {ex}");
                //        status = "ReadImage(backup)";
                //    }
                //}
                //cogimagefile.Open(imagefile, CogImageFileModeConstants.Read);//cog opens an image file in certain mode
                //cogimagefiletool.Operator = cogimagefile;


                string imagefile = @"C:\Users\yn.leong\Downloads\blobimg8.bmp";

                if (status == "ReadImage(backup)")
                {
                    string backupimg = @"D:\30018A_PPIDUMMY_CAS01_1_1_25_[01]_[12].tif";
                    cogimagefile = new CogImageFile();//ppi4424
                    cogimagefiletool = new CogImageFileTool();
                    cogimagefile.Open(backupimg, CogImageFileModeConstants.Read);//cog opens an image file in certain mode
                    cogimagefiletool.Operator = cogimagefile; cogimagefiletool.Run();
                    ICogImage = cogimagefiletool.OutputImage;
                    status = "ReadImage(backup)";
                    //File.Delete(imagefile);
                }
                else
                {
                    cogimagefile = new CogImageFile();//ppi4424
                    cogimagefiletool = new CogImageFileTool();
                    cogimagefile.Open(imagefile, CogImageFileModeConstants.Read);//cog opens an image file in certain mode
                    cogimagefiletool.Operator = cogimagefile; cogimagefiletool.Run();
                    ICogImage = cogimagefiletool.OutputImage;
                }


                TimeSpan timeout = TimeSpan.FromMilliseconds(mstimeout);
                Stopwatch stopwatch = Stopwatch.StartNew();
                bool IsFileOpenedSuccessfully = false;
                List<string> exceptionMessages = new List<string>();

                while (stopwatch.Elapsed < timeout)
                {
                    try
                    {
                        cogimagefile = new CogImageFile();//ppi4424
                        cogimagefiletool = new CogImageFileTool();
                        cogimagefile.Open(imagefile, CogImageFileModeConstants.Read);//cog opens an image file in certain mode
                        cogimagefiletool.Operator = cogimagefile; cogimagefiletool.Run();
                        ICogImage = cogimagefiletool.OutputImage;
                        IsFileOpenedSuccessfully = true;
                        break;
                    }
                    catch (IOException IOex)
                    {
                        exceptionMessages.Add($"IOException: {IOex}");
                    }
                    catch (UnauthorizedAccessException unauthEx)
                    {
                        exceptionMessages.Add($"UnauthorizedAccessException: {unauthEx}");
                        break;
                    }
                    catch (Exception ex)
                    {
                        exceptionMessages.Add($"Exception: {ex}");
                        break;
                    }

                    System.Threading.Thread.Sleep(100);
                }

                if (!IsFileOpenedSuccessfully)
                {
                    foreach (var message in exceptionMessages)
                    {
                        qGlobal.WriteRunLog(true, $"OpenImage {message}", "ERROR");
                    }
                }
            }
            catch (Exception ex)
            {
                qGlobal.WriteRunLog(true, $"OpenImageError={ex.ToString()}", "ERROR");
                OpenImage(true);
            }
        }

        public ICommand CMDConfigureOUT { get { return _CMDConfigureOUT ?? (_CMDConfigureOUT = new RelayCommand<object>(execute => ConfigureOUT())); } }

        private void ConfigureOUT()
        {
            try
            {
                string imagefile = @"C:\Users\yn.leong\Downloads\blobimg8.bmp";
                CogImageFile cogimagefile = new CogImageFile();//ppi4424
                CogImageFileTool cogimagefiletool = new CogImageFileTool();
                cogimagefile.Open(imagefile, CogImageFileModeConstants.Read);//cog opens an image file in certain mode
                cogimagefiletool.Operator = cogimagefile;
                cogimagefiletool.Run();

                ICogImage cogimage = (CogImage8Grey)cogimagefiletool.OutputImage;

                CogBlobTool cogblobtool_surface = new CogBlobTool();
                cogblobtool_surface.InputImage = cogimage;
                cogblobtool_surface.RunParams.SegmentationParams.Mode = CogBlobSegmentationModeConstants.HardDynamicThreshold;
                cogblobtool_surface.RunParams.SegmentationParams.Polarity = CogBlobSegmentationPolarityConstants.LightBlobs;

                cogblobtool_surface.Run();
                CogBlobResults blobresults = cogblobtool_surface.Results;

                //(2)*dispose CogBlobResultCollection, which is enumerable collection of CogBlobResult
                var vblobs = blobresults?.GetBlobs();//test vblobs type: (2a)var; (2b)CogBlobResultCollection 

                if (vblobs != null)
                {
                    foreach (CogBlobResult blob in vblobs)
                    {
                        Console.WriteLine($"Blob ID={blob.ID}; Area={blob.Area}");
                    }
                    //vblobs.Dispose();//(2)
                }

                //(1)*dispose CogBlobResults
                //cogblobtool_surface.Results?.Dispose();//(1a)
                //blobresults?.Dispose();//(1a+1b)
            }
            catch (Exception ee)
            {
                qGlobal.WriteRunLog(true, $"ConfigureOUT={ee.ToString()}", "ERROR");
            }
        }

        public Process ProcessRun { get; set; }
        public WaferViewModel Wafer_Review { get; set; }

        public event EventHandler SaveCanvasAsImageRequested;

        private bool IsSTARTRUN = false;
        private string logMsg = "";

        private string _visionEngineState = "START UP";
        private string _CurrentDateTime;
        private ICommand _CMDStartRun;
        private ICommand _CMDAbort;
        private ICommand _CMDLoadRecipe;
        private ICommand _CMDConfigureIN;
        private ICommand _CMDConfigureOUT;

        public MainViewModel()
        {
            StartUpdatingTime();
            InitStartUp();

            ProcessRun = new Process(this);
            Wafer_Review = new WaferViewModel();

            SetRunStatusCallback = SetRunStatus;
            //Global.uc.WriteLogEventHandler += ProcessRun.CogProcessLog_Generated;

            //20240724
            Wafer_Review.InspectedCount = 415;
            ProcessRun.Wafer.PassCount = 6;
        }
        public void SaveCanvasToImage()
        {
            // Notify the view (code-behind) to save the Canvas as an image
            OnSaveCanvasAsImageRequested();
        }

        private void OnSaveCanvasAsImageRequested()
        {
            SaveCanvasAsImageRequested?.Invoke(this, EventArgs.Empty);
        }

        private void InitStartUp()
        {
            ClearAllVisionProcessorFile();
        }

        private void ClearAllVisionProcessorFile()
        {
            if (!Directory.Exists(qGlobal.VisionProcessorConfig.ReadIMGFileDirectory))
                return;
            DirectoryInfo dirInfo = new DirectoryInfo(qGlobal.VisionProcessorConfig.ReadIMGFileDirectory);
            FileInfo[] fileInfo = dirInfo.GetFiles();
            for (int i = 0; i < fileInfo.Length; i++)
            {
                File.Delete(fileInfo[i].FullName);
            }
            dirInfo = null;
        }

        public void SetRunStatus(string Status)
        {
            try
            {
                switch (Status)
                {
                    case "STARTRUN":
                        VisionEngineState = "READY";
                        break;
                    case "START":
                        VisionEngineState = "PRODUCTION";
                        break;
                    case "STOPRUN":
                        VisionEngineState = "SUSPENDED";
                        break;
                    case "ABORT":
                        VisionEngineState = "ABORTED";
                        ProcessRun.Wafer.ClearModel();
                        break;
                    case "REVIEW":
                        VisionEngineState = "REVIEW";
                        //Wafer.ClearModel();
                        break;
                    case "ERROR":
                        VisionEngineState = "ERROR";
                        //Wafer.ClearModel();
                        break;
                    default:
                        VisionEngineState = "INVALID COMMAND";
                        break;
                }
            }
            catch (Exception ee)
            {
                qGlobal.WriteRunLog(true, $"SetRunStatus={ee.ToString()}", "ERROR");
            }
        }

        public void StartUpdatingTime()
        {
            var thread = new Thread(() =>
            {
                while (true)
                {
                    // Update the CurrentTime property on the UI thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CurrentDateTime = DateTime.Now.ToString("MMMM dd, yyyy, hh:mm:ss");
                    });

                    Thread.Sleep(500);
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }
    }
}
