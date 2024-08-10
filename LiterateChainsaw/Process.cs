using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using GemBox.Spreadsheet;
using LiterateChainsaw.ViewModel;
using LiterateChainsaw.Model;
using System.Threading;
using System.ComponentModel;
using ZephyrInnovations;
using System.IO;
using System.Diagnostics;
using System.Windows;

namespace LiterateChainsaw
{
    public delegate bool ProcessCommandDelegate(string Command);

    public class Process : INotifyPropertyChanged
    {
        private DateTime m_WaferStart;
        private TaskCompletionSource<bool> WaferMapReadyTaskCompletionSource;
        private string LastRecipe = string.Empty;
        private Thread StartRunThread { get; set; }
        private Thread MoveImageThread { get; set; }
        private Dictionary<string, string> WaferID_ReportPath_Dict;
        private DateTime _WaferStartTime;
        private DateTime _WaferEndTime;
        private Unit FoundCurrentUnit;
        private string logMsg;
        private string errMsg;

        private UnitRunData m_UnitRunData = new UnitRunData();
        public WaferViewModel Wafer { get; set; }
        public MainViewModel MainVM { get; set; }

        static Mutex _event = new Mutex();
        public int runningTasks = 0;
        public object locker = new object();

        private string CurrentWaferID = string.Empty;
        public string LotID { get; set; }
        public string RecipeID { get; set; }
        public string CassetteID { get; set; }
        public string WaferID { get; set; }
        public string SlotID { get; set; }
        public string TotalSlot { get; set; }

        private string CurrentReportPath = string.Empty;
        public string ReportPath { get => CurrentReportPath; }

        public Queue<string> WaferID_Queue;//testing 160424
        public Dictionary<string, List<string>> WaferID_FIList_Dict;
        private FileSystemWatcher xmlFSW;
        public ProcessCommandDelegate QueueCommand;
        public bool IsVidiTurnedOn { get; set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<string> runLog = new ObservableCollection<string>();

        public ObservableCollection<string> RunLog
        {
            get { return runLog; }
            set
            {
                runLog = value;
                OnPropertyChanged(nameof(RunLog));
            }
        }

        public Process(MainViewModel iMainVM)
        {
            MainVM = iMainVM;
            QueueCommand = new ProcessCommandDelegate(ProcessTCPIPCommand);
            Wafer = new WaferViewModel();
            InitFSW();
            //Abort();
            Global.uc.StartLoopAsync();

            //function();
        }

        private void AppendData(string newData)
        {
            //Application.Current.Dispatcher.Invoke(() => RunLog.Add(newData));
        }

        public bool ProcessTCPIPCommand(string TCPIPCommand)
        {
            bool IsSuccess = false;
            string lotID, recipeID, cassetteID, waferID, slotID, totalSlot;

            try
            {
                GetWaferInfo(TCPIPCommand, out lotID, out recipeID, out cassetteID, out waferID, out slotID, out totalSlot);
                string sWaferID = string.Format("{0}_{1}_{2}_{3}_{4}_{5}", lotID, recipeID, cassetteID, waferID, slotID, totalSlot);//follow image file name format (exclude coord)

                lock (WaferID_Queue)//Get all incoming commands queued into "WaferID_Queue" Queue<string>
                {
                    if (WaferID_Queue.All(Command => Command != sWaferID))
                    {
                        IsSuccess = true;
                        WaferID_Queue.Enqueue(sWaferID);

                        logMsg = string.Format("Process[{0}] WaferID={1} Enqueued; WaferID_Queue_Count={2}", CurrentWaferID, sWaferID, WaferID_Queue.Count);
                        qGlobal.WriteRunLog(true, logMsg, "INFO");
                        AppendData(logMsg);
                    }
                    else
                    {
                        Console.WriteLine($"Process=ProcessTCPIPCommand={sWaferID} already exist in queue.");
                        logMsg = string.Format("Process[{0}] WaferID={1} already exist; WaferID_Queue_Count={2}", CurrentWaferID, sWaferID, WaferID_Queue.Count);
                        qGlobal.WriteRunLog(true, logMsg, "INFO");
                        AppendData(logMsg);
                    }
                }
                return IsSuccess;
            }
            catch (Exception ex)
            {
                IsSuccess = false;
                errMsg = string.Format("Process(ProcessTCPIPCommand): Command={0}; {1}", TCPIPCommand, ex.ToString());
                qGlobal.WriteRunLog(true, errMsg, "ERROR");
                AppendData(errMsg);
                return IsSuccess;
            }
        }
        private void GetWaferInfo(string tCPIPCommand, out string lotID, out string recipeID, out string cassetteID, out string waferID, out string slotID, out string totalSlot)
        {
            string[] splittedBody = tCPIPCommand.Trim().Split(new char[] { '=', ',' });
            lotID = recipeID = cassetteID = waferID = slotID = totalSlot = "";

            try
            {
                if (splittedBody[0].ToUpper() == "LOT_ID")
                    lotID = splittedBody[1].Trim();
                if (splittedBody[2]/*.ToUpper()*/ == "VISION_RECIPE_ID")
                    recipeID = splittedBody[3].Trim();
                if (splittedBody[4].ToUpper() == "CASSETTE_ID")
                    cassetteID = splittedBody[5].Trim();
                if (splittedBody[6].ToUpper() == "WAFER_ID")
                    waferID = splittedBody[7].Trim();
                if (splittedBody[8].ToUpper() == "SLOT_ID")
                    slotID = splittedBody[9].Trim();
                if (splittedBody[10].ToUpper() == "TOTAL_SLOT")
                    totalSlot = splittedBody[11].Trim();
            }
            catch (Exception ex)
            {
                errMsg = string.Format("Process(GetWaferInfo): TCPIPCommand={0}; {1}", tCPIPCommand, ex.ToString());
                qGlobal.WriteRunLog(true, errMsg, "ERROR");
                AppendData(errMsg);
            }
        }

        private void InitFSW()
        {
            xmlFSW = new FileSystemWatcher();
            xmlFSW.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            xmlFSW.Filter = "*.*";
            xmlFSW.InternalBufferSize = 65536;
            xmlFSW.Created += new FileSystemEventHandler(OnChanged);
        }
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                string waferID = Path.GetFileNameWithoutExtension(e.FullPath.Substring(0, e.FullPath.IndexOf('[') - 1));
                if (!WaferID_FIList_Dict.ContainsKey(waferID))
                {
                    WaferID_FIList_Dict.Add(waferID, new List<string>());
                    logMsg = string.Format("Process[{0}] NewFSWWaferID={1}", CurrentWaferID, waferID);
                    //m_WaferStart = DateTime.Now;//wafervm cmm
                    qGlobal.WriteRunLog(true, logMsg, "INFO");
                    AppendData(logMsg);

                    //Run Log //300424
                    Wafer.StartTime = DateTime.Now;//300424
                    logMsg = string.Format("Process[{0}] FirstUnitTimeStamp={1}", CurrentWaferID, Wafer.StartTime.ToString("HH:mm:ss.fff"));
                    AppendData(logMsg);
                }
                WaferID_FIList_Dict[waferID].Add(e.FullPath);
                logMsg = string.Format("Process[{0}] File_IN={1} Count={2}", CurrentWaferID, e.FullPath, WaferID_FIList_Dict[waferID].Count);
                qGlobal.WriteRunLog(true, logMsg, "INFO");
            }
            catch (Exception ex)
            {
                logMsg = string.Format("Process[{0}] File={1} Exception={2}", CurrentWaferID, e.FullPath, ex.ToString());
                qGlobal.WriteRunLog(true, logMsg, "ERROR");
                AppendData(logMsg);
            }
        }

        public void Abort()
        {
            waferindex = 1;
            ClearReadFileDirectory();
            ClearAllCameraFile();

            //string ReportToday = string.Format(@"{0}\{1}", qGlobal.ReportConfig.ReportFileDirectory, DateTime.Now.ToString("yyyy-MM-dd"));//wafervm cmm
            string ReportToday = @"E:\Report\30018A\CAS01\Archive";
            string[] desiredExtensions = { ".jpg", ".png", ".tif", ".vpp", ".xml" };
            if (Directory.Exists(ReportToday))
            {
                string[] ImageFiles = Directory.GetFiles(ReportToday, "*.*", SearchOption.AllDirectories)
                                            .Where(filePath => desiredExtensions.Contains(Path.GetExtension(filePath), StringComparer.OrdinalIgnoreCase))
                                            .ToArray();

                if (ImageFiles.Length > 0)
                {
                    foreach (string filePath in ImageFiles)
                    {
                        File.Delete(filePath);
                    }
                }
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                //ClassificationViewModel.DefectList.Clear();
                runLog.Clear();
                MainVM.Abort();
            });

            LastRecipe = string.Empty;

            if (WaferID_Queue != null)
                WaferID_Queue.Clear();
            WaferID_Queue = new Queue<string>();

            if (WaferID_FIList_Dict != null)
                WaferID_FIList_Dict.Clear();
            WaferID_FIList_Dict = new Dictionary<string, List<string>>();


            if (WaferID_ReportPath_Dict != null)
                WaferID_ReportPath_Dict.Clear();
            WaferID_ReportPath_Dict = new Dictionary<string, string>();

            if (xmlFSW != null)
                xmlFSW.EnableRaisingEvents = false;

            CurrentWaferID = CurrentReportPath = string.Empty;

            if (Wafer != null)
                Application.Current.Dispatcher.Invoke(Wafer.ClearModelCallback);

            //Global.uc.Bgw_ProcessImage.CancelAsync();
            Global.uc?.StopLoop();
            //Global.uc?.UnloadAllLibraries();
            Global.uc.IsStartProcess = false;

            logMsg = string.Format("Process[{0}] PROCESS ABORTED", CurrentWaferID);
            qGlobal.WriteRunLog(true, logMsg, "INFO");
            AppendData(logMsg);
        }

        private void ClearReadFileDirectory()
        {
            try
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
            catch (Exception ex)
            {
                errMsg = string.Format("Process(ClearAllReadFile): {0}", ex.ToString());
                qGlobal.WriteRunLog(true, errMsg, "ERROR");
                AppendData(errMsg);
            }
        }

        public void StartRunState()
        {
            //GenerateWaferReport();
            StartRunThread = new Thread(new ThreadStart(StartRun));//category: worker thread
            xmlFSW.Path = qGlobal.VisionProcessorConfig.ReadIMGFileDirectory;
            xmlFSW.EnableRaisingEvents = true;
            StartRunThread.Priority = ThreadPriority.AboveNormal;
            StartRunThread.Start();
            logMsg = string.Format("Process[{0}] PROCESS STARTED; MaxDegreeofParallelism={1}; {2}", CurrentWaferID, qGlobal.VisionProcessorConfig.MaxDegreeofParallelism, qGlobal.VisionProcessorConfig.EnableMultiThread);
            qGlobal.WriteRunLog(true, logMsg, "INFO");
            AppendData(logMsg);
        }

        public void EndRunState()
        {
            if (StartRunThread != null && StartRunThread.IsAlive)
            {
                StartRunThread.Abort();
                logMsg = string.Format("Process[{0}] PROCESS ENDED", CurrentWaferID);
                qGlobal.WriteRunLog(true, logMsg, "INFO");
                AppendData(logMsg);
            }
        }

        public bool CanDequeue = false;
        int waferindex = 1;
        public void StartRun()
        {
            //MainVM.Wafer_Review.InspectedCount = 45;
            //bool CanDequeue = true;//wafervm cmm
            CanDequeue = true;
            Global.uc.StartLoopAsync();
            Global.uc.StartLoop_TimerTask();
            LastRecipe = string.Empty;
            bool bStopWatchStarted = false;
            TimeSpan timeout = TimeSpan.FromMilliseconds(20000);
            Stopwatch stopwatch = new Stopwatch();
            LoadVisionRecipeSetting("PPIDUMMY");//hardcode to load once receive STARTRUN from handler

            while (true)
            {
                try
                {
                    if (WaferID_FIList_Dict.ContainsKey(CurrentWaferID) && Wafer.InspectedCount != Wafer.TotalUnitCount)//process when FI found for dequeued WaferID (result xml successfully obtained for current wafer ID)
                    {
                        if (WaferID_FIList_Dict[CurrentWaferID].Count == 0)
                        {
                            if (!bStopWatchStarted)
                            {
                                stopwatch = Stopwatch.StartNew();
                                bStopWatchStarted = true;
                            }

                            logMsg = string.Format("Process[{0}] WaferID_FIList_Dict[{1}].Count=NULL", CurrentWaferID, CurrentWaferID);
                            //qGlobal.WriteRunLog(true, logMsg, "INFO");

                            //050524
                            if (WaferID_Queue.Count != 0)
                            {
                                if (stopwatch.Elapsed > timeout && false)
                                {
                                    //System.Windows.MessageBoxResult result;
                                    //if (WaferID_Queue.Count != 0)
                                    //{
                                    //    result = System.Windows.MessageBox.Show($"RESULT TIMEOUT \nCurrentWafer={CurrentWaferID} \nSkip and Proceed to Next Wafer in Queue?", "WAFER RESULT TIMEOUT",
                                    //    System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
                                    //}
                                    //else
                                    //{
                                    //    result = System.Windows.MessageBox.Show($"RESULT TIMEOUT \nCurrentWafer={CurrentWaferID} \nAbort Wafer?", "WAFER RESULT TIMEOUT",
                                    //    System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
                                    //}


                                    //if (result == System.Windows.MessageBoxResult.Yes)
                                    //{
                                    logMsg = string.Format("Process[{0}] WaferID_FIList_Dict[{1}].Count=NULL; Wafer AUTO Skipped/Aborted", CurrentWaferID, CurrentWaferID);
                                    qGlobal.WriteRunLog(true, logMsg, "INFO");
                                    WaferID_FIList_Dict.Remove(CurrentWaferID);
                                    LastRecipe = Wafer.RecipeID;

                                    WaferComplete();
                                    CanDequeue = true;
                                    //}
                                    //else
                                    //{
                                    //    stopwatch.Restart();
                                    //}
                                }
                            }
                            continue;
                        }
                        string FirstItem;
                        FirstItem = WaferID_FIList_Dict[CurrentWaferID][0];//get first item
                        if (FirstItem == null)
                        {
                            WaferID_FIList_Dict[CurrentWaferID].RemoveAll(item => item == null);//170424
                            logMsg = string.Format("Process[{0}] FirstItem=NULL", CurrentWaferID);
                            qGlobal.WriteRunLog(true, logMsg, "INFO");
                            continue;
                        }
                        int index = FirstItem.LastIndexOf(']');
                        string CurrentUnitFileName = FirstItem.Substring(0, index + 1);
                        bStopWatchStarted = false; stopwatch.Reset();

                        if (qGlobal.VisionProcessorConfig.EnableVidi)
                        {
                            bool IsResultNotReady()
                            {
                                //get unit coordinate identity
                                index = FirstItem.LastIndexOf(']');
                                if (index == -1)
                                {
                                    Console.WriteLine($"FirstItem index Error; FirstItem={FirstItem.ToString()}");
                                    WaferID_FIList_Dict[CurrentWaferID].Remove(FirstItem);
                                    return true;
                                }

                                CurrentUnitFileName = FirstItem.Substring(0, index + 1);
                                List<string> XMLResultFile = WaferID_FIList_Dict[CurrentWaferID].FindAll(p =>
                                {
                                    if (p == null)
                                        return false;
                                    else if (p.Contains(CurrentUnitFileName))
                                        return true;
                                    else
                                        return false;
                                });

                                if (XMLResultFile.Count > 2)
                                {
                                    Console.WriteLine($"{CurrentUnitFileName} count={XMLResultFile.Count}");
                                    XMLResultFile = XMLResultFile.Distinct().ToList();
                                    Console.WriteLine($"{CurrentUnitFileName} count={XMLResultFile.Count}");
                                }

                                return XMLResultFile.Count != 2;

                            };

                            while (IsResultNotReady())//move next item up while file count != 2
                            {
                                WaferID_FIList_Dict[CurrentWaferID].Remove(FirstItem);
                                WaferID_FIList_Dict[CurrentWaferID].Add(FirstItem);
                                FirstItem = WaferID_FIList_Dict[CurrentWaferID][0];
                                if (FirstItem == null)
                                {
                                    logMsg = string.Format("Process[{0}] IsResultNotReady=NULL", CurrentWaferID);
                                    continue;

                                }
                                Thread.Sleep(100);
                            }
                        }

                        logMsg = string.Format("Process[{0}] CurrentProcessUnit={1}", CurrentWaferID, Path.GetFileName(FirstItem));
                        qGlobal.WriteRunLog(true, logMsg, "INFO");
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ReadResultXML(CurrentUnitFileName);
                        });//remove all files with CurrentUnitFileName inside this method

                        AppendData(string.Format("{0} {1}", Path.GetFileNameWithoutExtension(FirstItem), "done processing"));

                        if (Wafer.InspectedCount == Wafer.TotalUnitCount)//reach wafer total unit count
                        {
                            WaferID_FIList_Dict.Remove(CurrentWaferID);
                            //LastRecipe = RecipeID;//wafervm cmm
                            LastRecipe = Wafer.RecipeID;


                            WaferComplete();
                            CanDequeue = true;
                        }
                    }
                    else
                    {
                        try
                        {
                            if (WaferID_Queue.Count == 0 && CanDequeue)
                            {
                                string wafercommand = "WAFER_ID=" + waferindex; waferindex++;
                                string str_C_1 = "LOT_ID=30018A,VISION_RECIPE_ID=PPIDUMMY,CASSETTE_ID=CAS01,WAFER_ID=1,SLOT_ID=1,TOTAL_SLOT=25";
                                string TCPCommand = str_C_1.Replace("WAFER_ID=1", wafercommand);
                                bool IsSuccess = (bool)Application.Current.Dispatcher.Invoke(QueueCommand, new object[] { TCPCommand });
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Main: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        if (CurrentWaferID != string.Empty && !WaferID_FIList_Dict.ContainsKey(CurrentWaferID))//FI NOT found for dequeued WaferID (Command OK, Load Map OK, no result xml obtained)
                        {
                            //Console.WriteLine($"No related file info for [{CurrentWaferID}]");
                            //timeout prompt for user action => Can skip command? YES=> CanDequeue = true; NO=> continue wait
                        }

                        if (WaferID_Queue.Count != 0 && CanDequeue)
                        {
                            CanDequeue = false;
                            bool IsLoadMapSuccess = false;
                            bool IsLoadRecipeSuccess = false;

                            CurrentWaferID = CurrentReportPath = string.Empty;
                            CurrentWaferID = WaferID_Queue.Dequeue();//*dequeue //30018A_A268B_TEST01_1_1_25


                            logMsg = string.Format("Process[{0}] WaferID={1} Dequeued; WaferID_Queue_Count={2}", CurrentWaferID, CurrentWaferID, WaferID_Queue.Count);
                            qGlobal.WriteRunLog(true, logMsg, "INFO");
                            AppendData(string.Format("{0} Dequeued; WaferID_Queue_Count={1}", CurrentWaferID, WaferID_Queue.Count));

                            if (CurrentWaferID != string.Empty)
                            {
                                bool IsReadCurrentWaferID = GetWaferIDInfo(CurrentWaferID);

                                if (IsReadCurrentWaferID)
                                {
                                    //ClearAllCameraFile();
                                    //exe method on UI thread and waits for completion before proceeding //alternative: Application.Current.Dispatcher.InvokeAsync() / Dispatcher.BeginInvoke()
                                    IsLoadMapSuccess = (bool)Application.Current.Dispatcher.Invoke(Wafer.LoadMapDelegateCallback, new object[] { LotID, RecipeID, CassetteID, WaferID, SlotID, TotalSlot });
                                    IsLoadRecipeSuccess = RecipeID == LastRecipe ? true : LoadVisionRecipeSetting(RecipeID);//load recipe only when different recipe as last recipe

                                    if (/*IsLoadMapSuccess &&*/ IsLoadRecipeSuccess)//wafervm cmm
                                    {
                                        #region wafervm cmm
                                        //string CurrentWaferID_Timestamp = string.Format("{0}_{1}", CurrentWaferID, DateTime.Now.ToString("HHmmss"));
                                        //CurrentReportPath = string.Format(@"{0}\{1}\{2}\", qGlobal.ReportConfig.ReportFileDirectory, DateTime.Now.ToString("yyyy-MM-dd"), CurrentWaferID_Timestamp);

                                        //if (!Directory.Exists(CurrentReportPath))
                                        //    Directory.CreateDirectory(CurrentReportPath);

                                        //DirectoryInfo dirInfo = new DirectoryInfo(CurrentReportPath);
                                        //FileInfo[] fileInfo = dirInfo.GetFiles();
                                        //for (int i = 0; i < fileInfo.Length; i++)
                                        //    File.Delete(fileInfo[i].FullName);

                                        //Global.uc?.SetRawImageBackup(false, CurrentReportPath);//change to use for report path

                                        ////Add to WaferID_ReportPath_Dict, pending dequeue when wafer complete
                                        //if (!WaferID_ReportPath_Dict.ContainsKey(CurrentWaferID))
                                        //    WaferID_ReportPath_Dict.Add(CurrentWaferID, CurrentReportPath);

                                        //logMsg = string.Format("Process[{0}] CurrentReportPath={1}", CurrentWaferID, CurrentReportPath);
                                        //qGlobal.WriteRunLog(true, logMsg, "INFO");
                                        //AppendData(logMsg);
                                        #endregion wafervm cmm

                                        //TCP
                                        Global.IsTCPIPReplyToolMsg = string.Format("{0},{1}", CurrentWaferID, "STATUS=Loaded");
                                        logMsg = string.Format("TCPIP -> {0}", Global.IsTCPIPReplyToolMsg);
                                        qGlobal.WriteRunLog(true, logMsg, "INFO");
                                        AppendData(logMsg);

                                        //let wafer view model handle
                                        _WaferStartTime = DateTime.Now;
                                        //Wafer.StartTime = _WaferStartTime.ToString("dd-MM-yyyy HH:mm:ss.fff");//wafervm cmm

                                        logMsg = string.Format("Process[{0}] WaferStartTime={1}", CurrentWaferID, _WaferStartTime.ToString("HH:mm:ss.fff"));
                                        //qGlobal.WriteRunLog(true, logMsg, "INFO");//wafervm cmm
                                        //Application.Current.Dispatcher.Invoke(MainVM.SetRunStatusCallback, new object[] { "START" });//wafervm cmm
                                        AppendData(logMsg);

                                        string CurrentWaferID_Timestamp = string.Format("{0}_{1}", CurrentWaferID, DateTime.Now.ToString("HHmmss"));
                                        CurrentReportPath = string.Format(@"{0}\{1}\{2}\", qGlobal.ReportConfig.ReportFileDirectory, DateTime.Now.ToString("yyyy-MM-dd"), CurrentWaferID_Timestamp);
                                        //move to wafer view model - pending
                                        if (!WaferID_ReportPath_Dict.ContainsKey(CurrentWaferID))
                                            WaferID_ReportPath_Dict.Add(CurrentWaferID, CurrentReportPath);//wafervm waferreportpath or CurrentReportPath?

                                        Application.Current.Dispatcher.Invoke(MainVM.SetRunStatusCallback, new object[] { "START" });

                                        Global.uc.IsStartProcess = true;
                                    }
                                    else//Load Map FAIL
                                    {
                                        CanDequeue = true;
                                        Application.Current.Dispatcher.BeginInvoke(MainVM.SetRunStatusCallback, new object[] { "ERROR" });//set Vision Engine state
                                    }
                                }
                            }
                            else//CurrentWaferID error
                            {
                                CanDequeue = true;
                                Application.Current.Dispatcher.BeginInvoke(MainVM.SetRunStatusCallback, new object[] { "ERROR" });//set Vision Engine state
                                //prompt action TBC
                                //1) ignore error and proceed to next wafer => set empty, clear wafer info & ready for next load
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    errMsg = string.Format("Process(StartRunHybrid): {0}", ex.ToString());
                    qGlobal.WriteRunLog(true, errMsg, "ERROR");
                    AppendData(errMsg);
                }

                Thread.Sleep(10);
            }
        }

        public void SetReportPath(string currentWaferID = "")
        {
            //GetWaferIDInfo(currentWaferID);//debug only

            string sourceDirectory = string.Format(@"{0}\{1}\{2}\{3}", qGlobal.ReportConfig.ReportFileDirectory, LotID, CassetteID, WaferID);
            string archiveDirectory = string.Format(@"{0}\{1}\{2}\{3}", qGlobal.ReportConfig.ReportFileDirectory, LotID, CassetteID, "Archive");

            if (Directory.Exists(sourceDirectory))//if report exists, move into Archive
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirectory);

                if (!Directory.EnumerateFileSystemEntries(sourceDirectory).Any())
                    return;

                DateTime lastWriteTime = directoryInfo.LastWriteTime;
                string timestamp = lastWriteTime.ToString("yyyyMMdd_HHmmss");
                string newDirectoryName = Path.Combine(Path.GetDirectoryName(sourceDirectory), Path.GetFileName(sourceDirectory) + "_" + timestamp);

                Directory.Move(sourceDirectory, newDirectoryName);

                if (!Directory.Exists(archiveDirectory))
                    Directory.CreateDirectory(archiveDirectory);

                string archiveDestination = Path.Combine(archiveDirectory, Path.GetFileName(newDirectoryName));
                Directory.Move(newDirectoryName, archiveDestination);
            }

            if (!Directory.Exists(sourceDirectory))
                Directory.CreateDirectory(sourceDirectory);
        }

        public void NextWafer()
        {
            logMsg = string.Format("Process[{0}] WaferID_FIList_Dict[{1}].Count=NULL; Wafer Skipped/Aborted", CurrentWaferID, CurrentWaferID);
            qGlobal.WriteRunLog(true, logMsg, "INFO");
            WaferID_FIList_Dict.Remove(CurrentWaferID);
            LastRecipe = Wafer.RecipeID;

            WaferComplete();
            CanDequeue = true;
        }

        public void StartMoveImage(string waferID)
        {
            try
            {
                if (WaferID_ReportPath_Dict.ContainsKey(waferID))
                {
                    string reportPath = WaferID_ReportPath_Dict[waferID];
                    string cameraFilesPath = qGlobal.VisionProcessorConfig.ReadIMGFileDirectory;

                    string[] waferImageFiles = Directory.GetFiles(cameraFilesPath)
                                   .Where(filePath => Path.GetFileName(filePath).Contains(waferID))
                                   .ToArray();

                    if (waferImageFiles.Length > 0)
                    {
                        //Console.WriteLine($"Moving from Camera File Path Image Files:");
                        foreach (string imagefile in waferImageFiles)
                        {
                            string sourceFolderPath = imagefile;
                            string destinationFolderPath = sourceFolderPath.Replace(qGlobal.VisionProcessorConfig.ReadIMGFileDirectory, reportPath);

                            File.Move(sourceFolderPath, destinationFolderPath);

                            logMsg = string.Format("Process[{0}] {1} Moved into Report Path", CurrentWaferID, Path.GetFileName(destinationFolderPath));
                            qGlobal.WriteRunLog(true, logMsg, "INFO");
                            AppendData(logMsg);
                            //Console.WriteLine($"File Move from {sourceFolderPath} into {destinationFolderPath}");
                        }
                    }


                    string[] ImageFiles = Directory.GetFiles(reportPath)
                                    .Where(filePath =>
                                        System.IO.Path.GetFileName(filePath).Contains(waferID) &&
                                        new[] { ".jpg", ".bmp", ".png", ".tif" }.Contains(System.IO.Path.GetExtension(filePath), StringComparer.OrdinalIgnoreCase))
                                    .ToArray();

                    if (ImageFiles.Length > 0)
                    {
                        foreach (string filePath in ImageFiles)
                        {
                            //File.Delete(filePath);
                        }
                    }
                }
                else
                {
                    logMsg = string.Format("Process[{0}] No relevant WaferID '{1}' found for Move Image", CurrentWaferID, waferID);
                    qGlobal.WriteRunLog(true, logMsg, "INFO");
                    AppendData(logMsg);
                }
            }
            catch (Exception ex)
            {
                errMsg = string.Format("Process(StartMoveImage): {0}", ex.ToString());
                qGlobal.WriteRunLog(true, errMsg, "ERROR");
                AppendData(errMsg);
            }

            Thread.Sleep(10);
        }

        private void ReadResultXML(string currentUnitFileName)
        {
            bool IsSuccess = false; string strErrMsg = string.Empty;
            UnitRunData VProRunData = null; UnitRunData VidiRunData = null;

            //to cater no matching coordinate for captured image
            int CoordX = Convert.ToInt16(currentUnitFileName.Substring(currentUnitFileName.IndexOf('[') + 1, 2));
            int CoordY = Convert.ToInt16(currentUnitFileName.Substring(currentUnitFileName.LastIndexOf('[') + 1, 2));
            FoundCurrentUnit = Wafer.ItemList.FirstOrDefault(x =>
            {
                return x.CoordX == CoordY && x.CoordY == CoordX;
            });
            if (FoundCurrentUnit == null)
            {
                string VProResultXML = string.Format("{0}{1}", currentUnitFileName, ".xml");
                if (WaferID_FIList_Dict[CurrentWaferID].Contains(VProResultXML))
                {
                    WaferID_FIList_Dict[CurrentWaferID].Remove(VProResultXML);
                    logMsg = string.Format("Process[{0}] RemovedFromList={1}", CurrentWaferID, VProResultXML);
                    //qGlobal.WriteRunLog(true, logMsg, "INFO");
                }
                string VidiResultXML = string.Format("{0}{1}", currentUnitFileName, "_Vidi.xml");
                if (WaferID_FIList_Dict[CurrentWaferID].Contains(VidiResultXML))
                {
                    WaferID_FIList_Dict[CurrentWaferID].Remove(VidiResultXML);
                    logMsg = string.Format("Process[{0}] RemovedFromList={1}", CurrentWaferID, VidiResultXML);
                    //qGlobal.WriteRunLog(true, logMsg, "INFO");
                }
                Console.WriteLine($"currentUnitFileName={currentUnitFileName}; CoordX={CoordX}; CoordY={CoordY}");
                return;
            }

            Wafer.InspectedCount++;
            Wafer.InspectedCountPerc = Convert.ToDouble(Wafer.InspectedCount) / Wafer.TotalUnitCount * 100;

            try
            {
                #region Get XML results: VProRunData & VidiRunData
                string VProResultXML = string.Format("{0}{1}", currentUnitFileName, ".xml");
                if (File.Exists(VProResultXML))
                {
                    object VProData = null;
                    if (VisionFileIsReady(VProResultXML))
                        IsSuccess = DeserializeFromXmlFile(VProResultXML, typeof(UnitRunData), out VProData, out strErrMsg);
                    else
                        IsSuccess = false;

                    if (!IsSuccess || strErrMsg != string.Empty)
                    {
                        errMsg = string.Format("Process(ReadResultXML): {0}", strErrMsg);
                        qGlobal.WriteRunLog(true, errMsg, "ERROR");
                        AppendData(errMsg);
                    }
                    else
                    {
                        logMsg = string.Format("Process[{0}] Deserialized={1}", CurrentWaferID, VProResultXML);
                        //qGlobal.WriteRunLog(true, logMsg, "INFO");
                    }
                    strErrMsg = string.Empty;

                    if (VProData != null)
                        VProRunData = (UnitRunData)VProData;
                }
                if (WaferID_FIList_Dict[CurrentWaferID].Contains(VProResultXML))
                {
                    WaferID_FIList_Dict[CurrentWaferID].Remove(VProResultXML);
                    logMsg = string.Format("Process[{0}] RemovedFromList={1}", CurrentWaferID, VProResultXML);
                    //qGlobal.WriteRunLog(true, logMsg, "INFO");
                }

                string VidiResultXML = string.Format("{0}{1}", currentUnitFileName, "_Vidi.xml");
                if (File.Exists(VidiResultXML))
                {
                    object VidiData = null;
                    if (VisionFileIsReady(VidiResultXML))
                        IsSuccess = DeserializeFromXmlFile(VidiResultXML, typeof(UnitRunData), out VidiData, out strErrMsg);
                    else
                        IsSuccess = false;

                    if (!IsSuccess || strErrMsg != string.Empty)
                    {
                        errMsg = string.Format("Process(ReadResultXML): {0}", strErrMsg);
                        qGlobal.WriteRunLog(true, errMsg, "ERROR");
                        AppendData(errMsg);
                    }
                    else
                    {
                        logMsg = string.Format("Process[{0}] Deserialized={1}", CurrentWaferID, VidiResultXML);
                        //qGlobal.WriteRunLog(true, logMsg, "INFO");
                    }
                    strErrMsg = string.Empty;

                    if (VidiData != null)
                        VidiRunData = (UnitRunData)VidiData;
                }
                if (WaferID_FIList_Dict[CurrentWaferID].Contains(VidiResultXML))
                {
                    WaferID_FIList_Dict[CurrentWaferID].Remove(VidiResultXML);
                    logMsg = string.Format("Process[{0}] RemovedFromList={1}", CurrentWaferID, VidiResultXML);
                    //qGlobal.WriteRunLog(true, logMsg, "INFO");
                }
                #endregion

                #region Get final result: FinalUnitData
                List<string> DefectCodeCollection = new List<string>();
                string combinedSecondaryDefectCode = string.Empty;
                string[] SecondaryDefectCodeArr;
                FinalUnitData finalUnitData = new FinalUnitData();
                IEnumerable<DefectCodeData> uniqueDefectCollection = new List<DefectCodeData>();
                DefectCodeData IsTopVProActivatedDefect;
                DefectCodeData IsTopVidiActivatedDefect;

                if (qGlobal.VisionProcessorConfig.EnableVidi)
                {
                    if (VProRunData != null && VidiRunData != null)
                    {
                        DefectCodeCollection.Add(VProRunData.PRIMARY_DEFECT_CODE);

                        combinedSecondaryDefectCode = VProRunData.SECONDARY_DEFECT_CODE;
                        SecondaryDefectCodeArr = combinedSecondaryDefectCode.Split(';');
                        DefectCodeCollection.AddRange(SecondaryDefectCodeArr.ToList());

                        DefectCodeCollection.Add(VidiRunData.PRIMARY_DEFECT_CODE);

                        combinedSecondaryDefectCode = VidiRunData.SECONDARY_DEFECT_CODE;
                        SecondaryDefectCodeArr = combinedSecondaryDefectCode.Split(';');
                        DefectCodeCollection.AddRange(SecondaryDefectCodeArr.ToList());

                        finalUnitData.IsVidiTurnedOn = true;
                        uniqueDefectCollection = qGlobal.DefectCodeDataList.Where(d =>
                        {
                            bool IsDefectExist = false;
                            foreach (string item in DefectCodeCollection)
                            {
                                if (item == d.DEFECT_CODE)
                                {
                                    IsDefectExist = true;
                                    break;
                                }
                            }
                            return IsDefectExist;
                        });

                        IsTopVProActivatedDefect = uniqueDefectCollection.FirstOrDefault(d => d.ACTIVATE == "True");
                        IsTopVidiActivatedDefect = uniqueDefectCollection.FirstOrDefault(d => d.ACTIVATE_DL == "True");

                        uniqueDefectCollection = uniqueDefectCollection.ToList().OrderBy(p => Convert.ToInt32(p.PRIORITY));
                        if (uniqueDefectCollection != null && uniqueDefectCollection.Count() != 0)//uniqueDefectCollection stores all defect codes from VPro and Vidi xml result; activate status unknown
                        {
                            if (IsTopVProActivatedDefect != null && IsTopVidiActivatedDefect != null)
                            {
                                bool IsVProMatched = IsTopVProActivatedDefect.DEFECT_CODE != VProRunData.PRIMARY_DEFECT_CODE ? false : true;
                                bool IsVidiMatched = IsTopVidiActivatedDefect.DEFECT_CODE != VidiRunData.PRIMARY_DEFECT_CODE ? false : true;

                                if (!IsVProMatched && !IsVidiMatched)//all activated defects do not match with RunData defects
                                {
                                    finalUnitData.IsVProFailAsPrimaryDefect = false;
                                    finalUnitData.IsVidiFailAsPrimaryDefect = false;
                                    finalUnitData.Primary_Defect_Code = "";
                                    finalUnitData.Primary_Defect_Desc = "";
                                    finalUnitData.Primary_Defect_Code_Color = "";
                                }
                                else if (!IsVProMatched)//Vidi Matched
                                {
                                    finalUnitData.IsVProFailAsPrimaryDefect = false;
                                    finalUnitData.IsVidiFailAsPrimaryDefect = true;

                                    finalUnitData.Primary_Defect_Code = IsTopVidiActivatedDefect.DEFECT_CODE;
                                    finalUnitData.Primary_Defect_Desc = IsTopVidiActivatedDefect.DEFECT_NAME;
                                    finalUnitData.Primary_Defect_Code_Color = IsTopVidiActivatedDefect.DEFECT_CODE_COLOR;
                                }
                                else if (!IsVidiMatched)//VPro Matched
                                {
                                    finalUnitData.IsVProFailAsPrimaryDefect = true;
                                    finalUnitData.IsVidiFailAsPrimaryDefect = false;

                                    finalUnitData.Primary_Defect_Code = IsTopVProActivatedDefect.DEFECT_CODE;
                                    finalUnitData.Primary_Defect_Desc = IsTopVProActivatedDefect.DEFECT_NAME;
                                    finalUnitData.Primary_Defect_Code_Color = IsTopVProActivatedDefect.DEFECT_CODE_COLOR;
                                }
                                else
                                {
                                    int prioritycheck = IsTopVProActivatedDefect.PRIORITY.CompareTo(IsTopVidiActivatedDefect.PRIORITY);

                                    if (prioritycheck == 0)//VPro and Vidi share same matched defect
                                    {
                                        finalUnitData.IsVProFailAsPrimaryDefect = true;
                                        finalUnitData.IsVidiFailAsPrimaryDefect = true;

                                        finalUnitData.Primary_Defect_Code = IsTopVProActivatedDefect.DEFECT_CODE;
                                        finalUnitData.Primary_Defect_Desc = IsTopVProActivatedDefect.DEFECT_NAME;
                                        finalUnitData.Primary_Defect_Code_Color = IsTopVProActivatedDefect.DEFECT_CODE_COLOR;
                                    }
                                    else if (prioritycheck == 1)
                                    {
                                        finalUnitData.IsVProFailAsPrimaryDefect = false;
                                        finalUnitData.IsVidiFailAsPrimaryDefect = true;

                                        finalUnitData.Primary_Defect_Code = IsTopVidiActivatedDefect.DEFECT_CODE;
                                        finalUnitData.Primary_Defect_Desc = IsTopVidiActivatedDefect.DEFECT_NAME;
                                        finalUnitData.Primary_Defect_Code_Color = IsTopVidiActivatedDefect.DEFECT_CODE_COLOR;
                                    }
                                    else
                                    {
                                        finalUnitData.IsVProFailAsPrimaryDefect = true;
                                        finalUnitData.IsVidiFailAsPrimaryDefect = false;

                                        finalUnitData.Primary_Defect_Code = IsTopVProActivatedDefect.DEFECT_CODE;
                                        finalUnitData.Primary_Defect_Desc = IsTopVProActivatedDefect.DEFECT_NAME;
                                        finalUnitData.Primary_Defect_Code_Color = IsTopVProActivatedDefect.DEFECT_CODE_COLOR;
                                    }
                                }
                            }
                            else if (IsTopVProActivatedDefect != null)//VPro defect activated => check if matches with VPro Pri Defect
                            {
                                finalUnitData.IsVidiFailAsPrimaryDefect = false;
                                finalUnitData.IsVProFailAsPrimaryDefect = IsTopVProActivatedDefect.DEFECT_CODE != VProRunData.PRIMARY_DEFECT_CODE ? false : true;

                                finalUnitData.Primary_Defect_Code = finalUnitData.IsVProFailAsPrimaryDefect ? IsTopVProActivatedDefect.DEFECT_CODE : "";
                                finalUnitData.Primary_Defect_Desc = finalUnitData.IsVProFailAsPrimaryDefect ? IsTopVProActivatedDefect.DEFECT_NAME : "";
                                finalUnitData.Primary_Defect_Code_Color = finalUnitData.IsVProFailAsPrimaryDefect ? IsTopVProActivatedDefect.DEFECT_CODE_COLOR : "";
                            }
                            else if (IsTopVidiActivatedDefect != null)//Vidi defect activated => check if matches with Vidi Pri Defect
                            {
                                finalUnitData.IsVProFailAsPrimaryDefect = false;
                                finalUnitData.IsVidiFailAsPrimaryDefect = IsTopVidiActivatedDefect.DEFECT_CODE != VidiRunData.PRIMARY_DEFECT_CODE ? false : true;

                                finalUnitData.Primary_Defect_Code = finalUnitData.IsVidiFailAsPrimaryDefect ? IsTopVidiActivatedDefect.DEFECT_CODE : "";
                                finalUnitData.Primary_Defect_Desc = finalUnitData.IsVidiFailAsPrimaryDefect ? IsTopVidiActivatedDefect.DEFECT_NAME : "";
                                finalUnitData.Primary_Defect_Code_Color = finalUnitData.IsVidiFailAsPrimaryDefect ? IsTopVidiActivatedDefect.DEFECT_CODE_COLOR : "";
                            }
                            else//all defects in uniqueDefectCollection are not activated
                            {
                                finalUnitData.IsVProFailAsPrimaryDefect = false;
                                finalUnitData.IsVidiFailAsPrimaryDefect = false;
                                finalUnitData.Primary_Defect_Code = "";
                                finalUnitData.Primary_Defect_Desc = "";
                                finalUnitData.Primary_Defect_Code_Color = "";
                            }
                        }
                        else//no defect code read from VPro and Vidi xml results
                        {
                            finalUnitData.IsVProFailAsPrimaryDefect = false;
                            finalUnitData.IsVidiFailAsPrimaryDefect = false;
                            finalUnitData.Primary_Defect_Code = "";
                            finalUnitData.Primary_Defect_Desc = "";
                            finalUnitData.Primary_Defect_Code_Color = "";
                        }

                        if (finalUnitData.Primary_Defect_Code != "")//comment for testing
                            finalUnitData.Result = "FAIL";
                        else
                            finalUnitData.Result = "PASS";

                        //retain all read defect codes from result xml files; save into secondary defects //Note: Unit may not have primary defect due to defect activate status
                        finalUnitData.Secondary_Defect_Codes = string.Join(";", uniqueDefectCollection.Where(d => d.DEFECT_CODE != finalUnitData.Primary_Defect_Code).ToList().Select(x => x.DEFECT_CODE));
                        finalUnitData.Secondary_Defect_Descs = string.Join(";", uniqueDefectCollection.Where(d => d.DEFECT_CODE != finalUnitData.Primary_Defect_Code).ToList().Select(x => x.DEFECT_NAME));
                    }
                    else if (VProRunData == null)
                    {
                        errMsg = string.Format("Process(ReadResultXML): VProRunData=NULL");
                        qGlobal.WriteRunLog(true, errMsg, "ERROR");
                        AppendData(errMsg);
                    }
                    else if (VidiRunData == null)
                    {
                        errMsg = string.Format("Process(ReadResultXML): VidiRunData=NULL");
                        qGlobal.WriteRunLog(true, errMsg, "ERROR");
                        AppendData(errMsg);
                    }
                }
                else
                {
                    if (VProRunData != null)
                    {
                        finalUnitData.IsVidiTurnedOn = false;
                        finalUnitData.Result = VProRunData.RESULT.ToUpper();
                        finalUnitData.IsVidiFailAsPrimaryDefect = false;
                        finalUnitData.Primary_Defect_Code = VProRunData.PRIMARY_DEFECT_CODE;
                        finalUnitData.Primary_Defect_Desc = VProRunData.PRIMARY_DEFECT_DESC;
                        finalUnitData.Primary_Defect_Code_Color = VProRunData.PRIMARY_DEFECT_CODE_COLOR;
                        finalUnitData.Secondary_Defect_Codes = VProRunData.SECONDARY_DEFECT_CODE;
                        finalUnitData.Secondary_Defect_Descs = VProRunData.SECONDARY_DEFECT_DESC;


                        DefectCodeCollection.Add(VProRunData.PRIMARY_DEFECT_CODE);

                        combinedSecondaryDefectCode = VProRunData.SECONDARY_DEFECT_CODE;
                        SecondaryDefectCodeArr = combinedSecondaryDefectCode.Split(';');
                        DefectCodeCollection.AddRange(SecondaryDefectCodeArr.ToList());

                        uniqueDefectCollection = qGlobal.DefectCodeDataList.Where(d =>
                        {
                            bool IsDefectExist = false;
                            foreach (string item in DefectCodeCollection)
                            {
                                if (item == d.DEFECT_CODE)
                                {
                                    IsDefectExist = true;
                                    break;
                                }
                            }
                            return IsDefectExist;
                        });

                        //sort uniqueDefectCollection here
                        uniqueDefectCollection = uniqueDefectCollection.ToList().OrderBy(p => Convert.ToInt32(p.PRIORITY));

                        var erew = uniqueDefectCollection.ToList();
                        if (uniqueDefectCollection.ToList().Count != 0)
                            finalUnitData.IsVProFailAsPrimaryDefect = uniqueDefectCollection.FirstOrDefault().DEFECT_CODE != VProRunData.PRIMARY_DEFECT_CODE ? false : true;
                        else
                            finalUnitData.IsVProFailAsPrimaryDefect = false;
                    }
                    else
                    {
                        errMsg = string.Format("Process(ReadResultXML): VProRunData=NULL");
                        qGlobal.WriteRunLog(true, errMsg, "ERROR");
                        AppendData(errMsg);
                    }
                }

                finalUnitData.VisionProRunData = VProRunData;
                finalUnitData.VidiRunData = VidiRunData;

                if (finalUnitData.Result == "FAIL")
                {
                    Wafer.FailCount++;
                    Wafer.FailCountPerc = Convert.ToDouble(Wafer.FailCount) / Wafer.TotalUnitCount * 100;

                    #region wafervm cmm
                    //var item = ClassificationViewModel.DefectList.SingleOrDefault(i =>
                    //{
                    //    return i.defectCodeData.DEFECT_CODE == finalUnitData.Primary_Defect_Code;
                    //});

                    //if (item == null && uniqueDefectCollection.Count() != 0)
                    //{
                    //    ClassificationViewModel.DefectList.Add(new DefectClassification
                    //    {
                    //        defectCodeData = uniqueDefectCollection.First(),
                    //        IsVidi = finalUnitData.IsVidiFailAsPrimaryDefect,
                    //        defectcolor = finalUnitData.Primary_Defect_Code_Color,
                    //        defectCount = 1,
                    //        VProCount = finalUnitData.IsVProFailAsPrimaryDefect ? 1 : 0,
                    //        VidiCount = finalUnitData.IsVidiFailAsPrimaryDefect ? 1 : 0
                    //    });
                    //}
                    //else
                    //{
                    //    item.defectCount++;
                    //    if (finalUnitData.IsVProFailAsPrimaryDefect)
                    //        item.VProCount++;
                    //    if (finalUnitData.IsVidiFailAsPrimaryDefect)
                    //        item.VidiCount++;
                    //}
                    #endregion wafervm cmm

                    if (Wafer.DefectVM == null)
                    {
                        Wafer.DefectVM = new DefectViewModel();
                        Wafer.DefectVM.TotalUnitCount = Wafer.TotalUnitCount;
                        //{
                        //    DefectList = new ObservableCollection<Defect.Classification>()
                        //};
                    }
                    //else
                    //    Wafer.DefectVM.CurrentInspectedCount++;

                    var defclass = Wafer.DefectVM.DefectList.SingleOrDefault(i =>
                    {
                        return i.defectCodeData.DEFECT_CODE == finalUnitData.Primary_Defect_Code;
                    });

                    //var item = ClassificationViewModel.DefectList.SingleOrDefault(i =>
                    //{
                    //    return i.defectCodeData.DEFECT_CODE == finalUnitData.Primary_Defect_Code;
                    //});

                    if (defclass == null && uniqueDefectCollection.Count() != 0)
                    {
                        //ClassificationViewModel.DefectList.Add(new DefectClassification
                        //{
                        //    defectCodeData = uniqueDefectCollection.First(),
                        //    IsVidi = finalUnitData.IsVidiFailAsPrimaryDefect,
                        //    defectcolor = finalUnitData.Primary_Defect_Code_Color,
                        //    defectCount = 1,
                        //    VProCount = finalUnitData.IsVProFailAsPrimaryDefect ? 1 : 0,
                        //    VidiCount = finalUnitData.IsVidiFailAsPrimaryDefect ? 1 : 0
                        //});

                        //
                        //    Defect.Classification dc = new Defect.Classification
                        //    {
                        //        defectCodeData = uniqueDefectCollection.First(),
                        //        //Color = finalUnitData.Primary_Defect_Code_Color,
                        //        Count = 1,
                        //        VProContributes = finalUnitData.IsVProFailAsPrimaryDefect ? 1 : 0,
                        //        VidiContributes = finalUnitData.IsVidiFailAsPrimaryDefect ? 1 : 0
                        //    };
                        ////};
                        ////dc.PropertyChanged += Wafer.DefectVM.UpdatePercentage;

                        Wafer.DefectVM.DefectList.Add(new ViewModel.Classification
                        {
                            defectCodeData = uniqueDefectCollection.First(),
                            //Color = finalUnitData.Primary_Defect_Code_Color,
                            Count = 1,
                            VProContributes = finalUnitData.IsVProFailAsPrimaryDefect ? 1 : 0,
                            VidiContributes = finalUnitData.IsVidiFailAsPrimaryDefect ? 1 : 0
                        });
                    }
                    else
                    {
                        //item.defectCount++;
                        //if (finalUnitData.IsVProFailAsPrimaryDefect)
                        //    item.VProCount++;
                        //if (finalUnitData.IsVidiFailAsPrimaryDefect)
                        //    item.VidiCount++;

                        //
                        defclass.Count++;
                        if (finalUnitData.IsVProFailAsPrimaryDefect)
                            defclass.VProContributes++;
                        if (finalUnitData.IsVidiFailAsPrimaryDefect)
                            defclass.VidiContributes++;
                    }
                }
                else if (finalUnitData.Result == "PASS")
                {
                    Wafer.PassCount++;
                    Wafer.PassCountPerc = Convert.ToDouble(Wafer.PassCount) / Wafer.TotalUnitCount * 100;
                }
                else
                {
                    Wafer.InvalidCount++;
                    Wafer.InvalidCountPerc = Convert.ToDouble(Wafer.InvalidCount) / Wafer.TotalUnitCount * 100;
                    Wafer.InspectedCount--;
                    Wafer.InspectedCountPerc = Convert.ToDouble(Wafer.InspectedCount) / Wafer.TotalUnitCount * 100;
                    if (FoundCurrentUnit != null)
                        FoundCurrentUnit.IsUnitInvalid = true;
                }

                //int CoordX = Convert.ToInt16(currentUnitFileName.Substring(currentUnitFileName.IndexOf('[') + 1, 2));
                //int CoordY = Convert.ToInt16(currentUnitFileName.Substring(currentUnitFileName.LastIndexOf('[') + 1, 2));
                Wafer.Yield = 100 - Wafer.FailCountPerc;// Convert.ToDouble(Wafer.PassCount) / Wafer.TotalUnitCount * 100f;
                //ClassificationViewModel.UpdateDefectStatistics();
                Wafer.CurrentProcessUnitTitle = string.Format("Unit Coordinate: R{0}-C{1}", CoordX, CoordY);

                //wafervm ucmm
                FoundCurrentUnit = Wafer.ItemList.FirstOrDefault(x =>
                {
                    return x.CoordX == CoordY && x.CoordY == CoordX;
                });
                //FoundCurrentUnit = Wafer.ItemList[0];

                if (FoundCurrentUnit != null)
                {
                    FoundCurrentUnit.IsProcessed = true;
                    FoundCurrentUnit.Image_Filename = System.IO.Path.GetFileNameWithoutExtension(currentUnitFileName).Replace("_Vidi", "");
                    FoundCurrentUnit.IsVProFailAsPrimaryDefect = finalUnitData.IsVProFailAsPrimaryDefect;
                    FoundCurrentUnit.IsVidiFailAsPrimaryDefect = finalUnitData.IsVidiFailAsPrimaryDefect;
                    FoundCurrentUnit.Primary_Defect_Code = finalUnitData.Primary_Defect_Code;
                    FoundCurrentUnit.Primary_Defect_Code_Color = finalUnitData.Primary_Defect_Code_Color;
                    FoundCurrentUnit.Result = finalUnitData.Result;

                    #region Unit Result Serializable
                    //UnitResult UNIT = new UnitResult();
                    //UNIT.ID = currentUnitFileName.Substring(currentUnitFileName.IndexOf('['), 8);//[xx][yy]
                    //UNIT.PRIMARY_DEFECT = new Defect() { CODE = finalUnitData.Primary_Defect_Code, COLOR = finalUnitData.Primary_Defect_Code_Color, DESCRIPTION = finalUnitData.Primary_Defect_Desc };
                    //UNIT.IS_VPRO_FAIL_AS_PRIMARY = finalUnitData.IsVProFailAsPrimaryDefect;
                    //UNIT.IS_VIDI_FAIL_AS_PRIMARY = finalUnitData.IsVidiFailAsPrimaryDefect;

                    //var SecondaryDefects = uniqueDefectCollection.Where(d => d.DEFECT_CODE != finalUnitData.Primary_Defect_Code).ToList();

                    //foreach (var item in SecondaryDefects)
                    //{
                    //    Defect secDefect = new Defect
                    //    {
                    //        CODE = item.DEFECT_CODE,
                    //        COLOR = item.DEFECT_CODE_COLOR,
                    //        DESCRIPTION = item.DEFECT_NAME
                    //    };
                    //    UNIT.SECONDARY_DEFECT.Add(secDefect);
                    //}

                    //Wafer.WaferResult.UNITS.Add(UNIT);
                    #endregion
                }
                else
                {
                    errMsg = string.Format("Process(ReadResultXML): FoundCurrentUnit=NULL");
                    qGlobal.WriteRunLog(true, errMsg, "ERROR");
                    AppendData(errMsg);
                }
                #endregion

                #region Serialize Result
                string unitFileName = Path.GetFileNameWithoutExtension(currentUnitFileName);
                string UnitResultXML = string.Format("{0}{1}.xml", CurrentReportPath, unitFileName);
                //string UnitResultXML = string.Format(@"{0}\{1}.xml", Wafer.ReportPath, unitFileName);

                IsSuccess = SerializeToXmlFile(UnitResultXML, typeof(FinalUnitData), finalUnitData, out strErrMsg);
                if (!IsSuccess || strErrMsg != string.Empty)
                {
                    errMsg = string.Format("Process(ReadResultXML): {0}", strErrMsg);
                    qGlobal.WriteRunLog(true, errMsg, "ERROR");
                    AppendData(errMsg);
                }
                else
                {
                    logMsg = string.Format("Process[{0}] Serialized={1}", CurrentWaferID, UnitResultXML);
                    //qGlobal.WriteRunLog(true, logMsg, "INFO");
                }
                strErrMsg = string.Empty;

                if (File.Exists(VProResultXML))
                {
                    File.Delete(VProResultXML);
                    logMsg = string.Format("Process[{0}] FileDeleted={1}", CurrentWaferID, VProResultXML);
                    qGlobal.WriteRunLog(true, logMsg, "INFO");
                }
                if (File.Exists(VidiResultXML))
                {
                    File.Delete(VidiResultXML);
                    logMsg = string.Format("Process[{0}] FileDeleted={1}", CurrentWaferID, VidiResultXML);
                    qGlobal.WriteRunLog(true, logMsg, "INFO");
                }
                #endregion

                #region Move Image
                string imagepath = string.Format("{0}{1}", currentUnitFileName, ".tif").Replace(System.IO.Path.GetDirectoryName(currentUnitFileName), qGlobal.VisionProcessorConfig.ReadIMGFileDirectory);
                //var newname = imagepath.Replace(System.IO.Path.GetDirectoryName(imagepath), ReportPath);//wafervm cmm
                var newname = imagepath.Replace(System.IO.Path.GetDirectoryName(imagepath), Wafer.ReportPath);
                if (File.Exists(imagepath))
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            if (File.Exists(newname))
                                File.Delete(newname);

                            File.Move(imagepath, newname);
                        }
                        catch (Exception ex)
                        {
                            errMsg = string.Format("ReadResultXML(File.Move): File={0}; {1}", currentUnitFileName, ex.ToString());
                            qGlobal.WriteRunLog(true, errMsg, "ERROR");
                            AppendData(errMsg);
                            Application.Current.Dispatcher.Invoke(() => RunLog.Add(errMsg));
                        }
                    });
                }
                string vpppath = string.Format("{0}{1}", currentUnitFileName, ".vpp").Replace(System.IO.Path.GetDirectoryName(currentUnitFileName), @"E:\Report\vpp_temp"/*qGlobal.VisionProcessorConfig.CommunicationReadFileDirectory*/);
                var newnamevpp = vpppath.Replace(System.IO.Path.GetDirectoryName(vpppath), Wafer.ReportPath);
                if (File.Exists(vpppath))
                {
                    //Task.Run(() =>
                    //{
                    //    File.Move(imagepath, newname);
                    //});
                    if (File.Exists(newnamevpp))
                        File.Delete(newnamevpp);

                    File.Move(vpppath, newnamevpp);
                }
                #endregion
            }
            catch (Exception ex)
            {
                errMsg = string.Format("Process(ReadResultXML): File={0}; {1}", currentUnitFileName, ex.ToString());
                qGlobal.WriteRunLog(true, errMsg, "ERROR");
                AppendData(errMsg);
            }
        }
        public static bool DeserializeFromXmlFile(string strXMLFile, Type type, out object obj, out string strErrMsg)
        {
            strErrMsg = "";
            obj = null;
            System.IO.TextReader reader = null;//= new System.IO.StreamReader(strXMLFile);

            try
            {
                if (!System.IO.File.Exists(strXMLFile))
                {
                    strErrMsg = string.Format("XML file \"{0}\" not found!", strXMLFile);
                    return false;
                }
                reader = new System.IO.StreamReader(strXMLFile);
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(type);
                obj = serializer.Deserialize(reader);
                reader.Close();
                return true;
            }
            catch (Exception ex)
            {
                strErrMsg = string.Format("Deserialization Error. File={0}; \nException: {1}", strXMLFile, ex.ToString());
                if (reader != null)
                    reader.Close();
                return false;
            }
        }
        public static bool SerializeToXmlFile(string sTmpXMLFileName, Type type, object obj, out string strErrMsg)
        {
            strErrMsg = "";
            System.Xml.Serialization.XmlSerializer serializer = null;
            System.IO.FileStream stream = null;
            try
            {
                serializer = new System.Xml.Serialization.XmlSerializer(type);
                stream = new System.IO.FileStream(sTmpXMLFileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                serializer.Serialize(stream, obj);
                return true;
            }
            catch (Exception ex)
            {
                strErrMsg = string.Format("Serialization Error. File={0}; \nException: {1}", sTmpXMLFileName, ex.ToString());
                return false;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }
        private bool VisionFileIsLocked(string filename, FileAccess file_access)
        {
            // Try to open the file with the indicated access.
            try
            {
                FileStream fs = new FileStream(filename, FileMode.Open, file_access);
                fs.Close();
                return false;
            }
            catch (IOException)
            {
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool VisionFileIsReady(string file, double mstimeout = 1000)
        {
            TimeSpan timeout = TimeSpan.FromMilliseconds(mstimeout);
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed < timeout)
            {
                try
                {
                    using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.ReadWrite))
                    {
                        // File opened successfully, return true
                        return true;
                    }
                }
                catch (IOException IOex)
                {
                    // Log the exception
                    errMsg = string.Format("Process(VisionFileIsReady): CurrentWaferID={0}; File={1}; {2}", CurrentWaferID, file, IOex.ToString());
                    qGlobal.WriteRunLog(true, errMsg, "ERROR");
                    Console.WriteLine($"{errMsg}");
                }
                catch (Exception ex)
                {
                    // Log the exception
                    errMsg = string.Format("Process(VisionFileIsReady): CurrentWaferID={0}; File={1}; {2}", CurrentWaferID, file, ex.ToString());
                    qGlobal.WriteRunLog(true, errMsg, "ERROR");
                    Console.WriteLine($"{errMsg}");
                }

                System.Threading.Thread.Sleep(100);
            }

            return true;
        }

        public void CogResultReceiver(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                qGlobal.WriteRunLog(true, "Main: " + ex.Message, "ERROR");
                MessageBox.Show("Main: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool GetWaferIDInfo(string currentWaferID)
        {
            bool IsReadSuccess = false;
            string[] WaferInfo = currentWaferID.Trim().Split(new char[] { '_' });

            try
            {
                LotID = WaferInfo[0].Trim();
                RecipeID = WaferInfo[1].Trim();
                CassetteID = WaferInfo[2].Trim();
                WaferID = WaferInfo[3].Trim();
                SlotID = WaferInfo[4].Trim();
                TotalSlot = WaferInfo[5].Trim();

                IsReadSuccess = true;
                return IsReadSuccess;
            }
            catch (Exception ex)
            {
                IsReadSuccess = false;
                errMsg = string.Format("Process(GetWaferIDInfo): CurrentWaferID={0}; {1}", currentWaferID, ex.ToString());
                qGlobal.WriteRunLog(true, errMsg, "ERROR");
                AppendData(errMsg);
                return IsReadSuccess;
            }
        }

        private bool LoadVisionRecipeSetting(string recipeID)
        {
            bool foundRecipeSetting = false;
            for (int i = 0; i < qGlobal.VisionRecipeDataList.Count(); i++)
            {
                if (qGlobal.VisionRecipeDataList[i].TOOL_RECIPE.ToUpper() == recipeID.ToUpper())
                {
                    string inputPath = qGlobal.VisionProcessorConfig.ReadIMGFileDirectory;
                    string outputPath = qGlobal.VisionProcessorConfig.ResultXMLFileDirectory;
                    bool saveRawImage = true;

                    string visionRecipeUnitFile = string.Format("{0}{1}.qvs", qGlobal.VisionProcessorConfig.RecipeFileDirectory, qGlobal.VisionRecipeDataList[i].RECIPE);
                    double visionRecipePixelConversionValue = qGlobal.VisionRecipeDataList[i].PIXEL_CONVERSION_VALUE;
                    string visionRecipePixelConversionUnit = qGlobal.VisionRecipeDataList[i].PIXEL_CONVERSION_UNIT;

                    string rawImagePath = qGlobal.VisionProcessorConfig.RecipeFileDirectory + DateTime.Now.ToString("d-MMM-yyyy_HH_mm_ss_ffffff");
                    if (saveRawImage == true)
                    {
                        if (true)
                        {
                            if (Directory.Exists(rawImagePath) == false)
                                Directory.CreateDirectory(rawImagePath);
                        }

                        //MessageBox.Show(qGlobal.VisionProcessorConfig.EnableSaveRawImage + " " + rawImagePath);

                        rawImagePath = Wafer.ReportPath;//140524

                        //Global.uc?.SetRawImageBackup(true, rawImagePath);
                        saveRawImage = false;
                    }

                    //bool? success = Global.uc?.SetImagePathRecipe(inputPath, outputPath, visionRecipeUnitFile, visionRecipePixelConversionValue, visionRecipePixelConversionUnit);//ppi4424
                    //bool? success = Global.uc?.LoadRecipeUC(inputPath, outputPath, visionRecipeUnitFile, visionRecipePixelConversionValue, visionRecipePixelConversionUnit);//wafervm cmm
                    //bool? success = Global.uc?.LoadRecipeUC(inputPath, outputPath, visionRecipeUnitFile);
                    bool? success = Global.uc?.LoadRecipeOnTask(inputPath, outputPath, visionRecipeUnitFile, visionRecipePixelConversionValue, visionRecipePixelConversionUnit);

                    //Global.uc?.ConfigureResultRetriever(inputPath, outputPath, visionRecipeUnitFile);
                    if ((bool)success)
                    {
                        //logMsg = string.Format("Load vision recipe. File: {0}, InputPath: {1}, OutputPath: {2}", visionRecipeUnitFile, inputPath, outputPath);//wafervm cmm
                        logMsg = string.Format("[{0}] IN={1}, OUT={2}", Path.GetFileNameWithoutExtension(visionRecipeUnitFile), inputPath, outputPath);

                        qGlobal.WriteRunLog(true, logMsg, "INFO");
                        AppendData(logMsg);
                        //SetLog(true, logMsg, Color.Purple);

                        foundRecipeSetting = true;
                        LastRecipe = recipeID;
                    }
                    else
                    {
                        foundRecipeSetting = false;
                    }

                    //ClearAllCameraFile();
                    break;
                }
            }
            return foundRecipeSetting;
        }
        private void ClearAllCameraFile()
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(qGlobal.VisionProcessorConfig.ResultXMLFileDirectory);
                FileInfo[] fileInfo = dirInfo.GetFiles();
                for (int i = 0; i < fileInfo.Length; i++)
                {
                    File.Delete(fileInfo[i].FullName);
                }
                dirInfo = null;
            }
            catch (Exception ex)
            {
                errMsg = string.Format("Process(ClearAllCameraFile): {0}", ex.ToString());
                qGlobal.WriteRunLog(true, errMsg, "ERROR");
                AppendData(errMsg);
            }
        }

        /// <summary>
        /// Send TCP > Create PDF > Save Map > Clear wafer model > Clear CurrentWaferID > Set Main Status
        /// </summary>
        private void WaferComplete()
        {
            //_WaferEndTime = DateTime.Now;//wafervm cmm
            //Wafer.TimeTaken = _WaferEndTime - _WaferStartTime;//wafervm cmm
            //Wafer.EndTime = _WaferEndTime.ToString("dd-MM-yyyy HH:mm:ss.fff");//wafervm cmm

            Application.Current.Dispatcher.Invoke(() =>
            {
                MainVM.SaveCanvasToImage();
            });

            if (Wafer != null)
                Application.Current.Dispatcher.Invoke(Wafer.CompleteRunCallback);

            GC.Collect();
            Global.IsTCPIPReplyToolMsg = string.Format("{0},{1}", CurrentWaferID, "STATUS=Complete");
            logMsg = string.Format("TCPIP -> {0}", Global.IsTCPIPReplyToolMsg);
            qGlobal.WriteRunLog(true, logMsg, "INFO");
            AppendData(logMsg);


            #region Save Report //wafervm cmm
            //logMsg = string.Format("SaveReport[Start]");
            //qGlobal.WriteRunLog(true, logMsg, "INFO");

            //Stopwatch Watch = Stopwatch.StartNew();
            //GenerateWaferReport();
            //Watch.Stop();

            //logMsg = string.Format("SaveReport[End]");
            //qGlobal.WriteRunLog(true, logMsg, "INFO");

            //logMsg = string.Format("SaveReport[TimeTaken] {0}", Watch.ElapsedMilliseconds.ToString());
            //qGlobal.WriteRunLog(true, logMsg, "INFO");
            #endregion

            #region Save Map //wafervm cmm
            //logMsg = string.Format("SaveMap[Start]");
            //qGlobal.WriteRunLog(true, logMsg, "INFO");

            //Watch.Reset(); Watch.Start();
            //Wafer.SaveMap();
            //Watch.Stop();
            //logMsg = string.Format("SaveMap[End]");
            //qGlobal.WriteRunLog(true, logMsg, "INFO");

            //logMsg = string.Format("SaveMap[TimeTaken] {0}", Watch.ElapsedMilliseconds.ToString());
            //qGlobal.WriteRunLog(true, logMsg, "INFO");
            #endregion

            #region wafervm cmm
            //logMsg = string.Format("Process[{0}] WaferEndTime={1}; TimeTaken={2}", CurrentWaferID, _WaferEndTime.ToString("HH:mm:ss.fff"), Wafer.TimeTaken.ToString("g"));
            //qGlobal.WriteRunLog(true, logMsg, "INFO");
            //AppendData(logMsg);

            //TimeSpan timespan_firstUnit = _WaferEndTime - m_WaferStart;
            //logMsg = string.Format("Process[{0}] WaferStartTime={1}; TS_FirstUnit={2}", CurrentWaferID, m_WaferStart.ToString("HH:mm:ss.fff"), timespan_firstUnit.ToString("g"));
            //qGlobal.WriteRunLog(true, logMsg, "INFO");
            //AppendData(logMsg);
            #endregion wafervm cmm

            logMsg = string.Format("Process[{0}]; WaferStart_CommandReceived={1}; WaferEnd={2}; TimeTaken={3}", CurrentWaferID, _WaferStartTime.ToString("HH:mm:ss.fff"), Wafer.EndTime.ToString("HH:mm:ss.fff"), Wafer.TimeTaken.ToString("g"));
            qGlobal.WriteRunLog(true, logMsg, "INFO");
            logMsg = string.Format("Process[{0}]; WaferStart_FirstUnit={1}; WaferEnd={1}; TimeTaken={2}", CurrentWaferID, Wafer.StartTime.ToString("HH:mm:ss.fff"), Wafer.EndTime.ToString("HH:mm:ss.fff"), Wafer.TimeTaken.ToString("g"));
            qGlobal.WriteRunLog(true, logMsg, "INFO");
            AppendData(logMsg);


            Thread MoveImageThread = new Thread(() => StartMoveImage(CurrentWaferID));
            MoveImageThread.Start();//holdwafermap
            logMsg = string.Format("Process[{0}] MOVE IMAGE THREAD STARTED", CurrentWaferID);
            qGlobal.WriteRunLog(true, logMsg, "INFO");
            AppendData(logMsg);

            //CurrentWaferID = CurrentReportPath = string.Empty;//reset when FI count=0
            Application.Current.Dispatcher.Invoke(() =>
            {//holdwafermap
                //Wafer.ClearModelCallback();//wafervm cmm
                //ClassificationViewModel.DefectList.Clear();//wafervm cmm
                MainVM.Abort();
            });
            Application.Current.Dispatcher.Invoke(MainVM.SetRunStatusCallback, new object[] { "STARTRUN" });
        }

        private void EndMoveImage()
        {
            //if (MoveImageThread != null && MoveImageThread.IsAlive)
            //{
            //    MoveImageThread.Abort();
            //    logMsg = string.Format("Process[{0}] MOVE IMAGE THREAD ENDED", CurrentWaferID);
            //    qGlobal.WriteRunLog(true, logMsg, "INFO");
            //    AppendData(logMsg);
            //}
        }

        private void StartMoveImageThread()
        {
            //MoveImageThread = new Thread(new ThreadStart(StartMoveImage));//category: worker thread
            //MoveImageThread.Priority = ThreadPriority.Normal;
            //MoveImageThread.Start();
            //logMsg = string.Format("Process[{0}] MOVE IMAGE THREAD STARTED", CurrentWaferID);
            //qGlobal.WriteRunLog(true, logMsg, "INFO");
            //AppendData(logMsg);
        }

        private void GenerateWaferReport()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainVM.SaveCanvasToImage();
                });
                // If using the Professional version, put your serial key below.
                SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");

                // Load Excel workbook from file's path.
                ExcelFile workbook = ExcelFile.Load(@"C:\QVS\Report\PPI_ReportTemplate1.xlsx");
                ExcelWorksheet worksheet = workbook.Worksheets[0];

                #region Assign ExcelCells
                ExcelCell WaferID = worksheet.Cells["B7"];
                ExcelCell Performance = worksheet.Cells["E7"];
                ExcelCell MultiThread = worksheet.Cells["H7"];
                ExcelCell ThreadCount = worksheet.Cells["K7"];
                ExcelCell DeepLearning = worksheet.Cells["N7"];
                ExcelCell Yield = worksheet.Cells["Q7"];

                ExcelCell StartTime = worksheet.Cells["B10"];
                ExcelCell EndTime = worksheet.Cells["H10"];
                ExcelCell TimeElapsed = worksheet.Cells["N10"];

                ExcelCell PassCounter = worksheet.Cells["Q14"];
                ExcelCell FailCounter = worksheet.Cells["Q15"];
                ExcelCell InspectedCounter = worksheet.Cells["Q16"];
                ExcelCell InvalidCounter = worksheet.Cells["Q17"];
                ExcelCell PassPercentage = worksheet.Cells["R14"];
                ExcelCell FailPercentage = worksheet.Cells["R15"];
                ExcelCell InspectedPercentage = worksheet.Cells["R16"];
                ExcelCell InvalidPercentage = worksheet.Cells["R17"];
                ExcelCell TotalUnitNo = worksheet.Cells["Q18"];

                ExcelCell LotID = worksheet.Cells["Q21"];
                ExcelCell RecipeID = worksheet.Cells["Q22"];
                ExcelCell CassetteID = worksheet.Cells["Q23"];
                ExcelCell SlotID = worksheet.Cells["Q24"];
                ExcelCell TotalSlot = worksheet.Cells["Q25"];
                #endregion

                //append lot info
                WaferID.Value = Wafer.WaferID;
                //Performance.Value = 3600000 / Wafer.TimeTaken.TotalMilliseconds;
                Performance.Value = Convert.ToDouble(3600000 / Wafer.TimeTaken.TotalMilliseconds).ToString("0.00") + "WPH";
                //Performance.Value = Convert.ToDouble(3600000 / (Wafer.TimeTaken.TotalMilliseconds / Wafer.InspectedCount) * 0.001f).ToString("0.00") + "k UPH";
                MultiThread.Value = qGlobal.VisionProcessorConfig.EnableMultiThread.ToString();
                ThreadCount.Value = qGlobal.VisionProcessorConfig.MaxDegreeofParallelism.ToString();
                DeepLearning.Value = qGlobal.VisionProcessorConfig.EnableVidi.ToString();
                Yield.Value = Wafer.Yield.ToString("0.00");

                //StartTime.Value = _WaferStartTime;//wafervm cmm
                //EndTime.Value = _WaferEndTime;//wafervm cmm
                StartTime.Value = Wafer.StartTime;
                EndTime.Value = Wafer.EndTime;
                TimeElapsed.Value = Wafer.TimeTaken.ToString("g");

                PassCounter.Value = Wafer.PassCount;
                FailCounter.Value = Wafer.FailCount;
                InspectedCounter.Value = Wafer.InspectedCount;
                InvalidCounter.Value = Wafer.InvalidCount;
                PassPercentage.Value = Wafer.PassCountPerc.ToString("0.00");
                FailPercentage.Value = Wafer.FailCountPerc.ToString("0.00");
                InspectedPercentage.Value = Wafer.InspectedCountPerc.ToString("0.00");
                InvalidPercentage.Value = Wafer.InvalidCountPerc.ToString("0.00");
                TotalUnitNo.Value = Wafer.TotalUnitCount;

                LotID.Value = Wafer.LotID;
                RecipeID.Value = Wafer.RecipeID;
                CassetteID.Value = Wafer.CassetteID;
                SlotID.Value = Wafer.SlotID;
                TotalSlot.Value = Wafer.TotalSlot;

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


                string waferreportpath = string.Format("{0}WaferSummary.pdf", CurrentReportPath);//,  CurrentWaferID);

                // Image path to insert into the worksheet.
                string wafermapimagepath = string.Format("{0}WaferMap.bmp", CurrentReportPath);//, CurrentWaferID);

                // Read the image file as a byte array.
                byte[] imageBytes = File.ReadAllBytes(wafermapimagepath);
                // Load the image from the file.
                using (var imageStream = new MemoryStream(imageBytes))
                {
                    // Insert the image into the worksheet.
                    worksheet.Pictures.Add(imageStream, ExcelPictureFormat.Bmp, 12, 305, 370, 370, LengthUnit.Pixel);
                    // Save the modified workbook to a new Excel file.
                    workbook.Save(@waferreportpath);

                }
            }
            catch (Exception ex)
            {
                errMsg = string.Format("Process(GenerateWaferReport): {0}", ex.ToString());
                qGlobal.WriteRunLog(true, errMsg, "ERROR");
            }
        }

        private void SaveWaferMapAsImage()
        {
            MainVM.SaveCanvasToImage();
        }

        public void CogProcessLog_Generated(object sender, string _logMsg)
        {
            AppendData("[CogProcess] " + _logMsg);
        }

        //20240724

        private Thread wThread { get; set; }
        public void function()
        {
            core();
            //wThread = new Thread(new ThreadStart(core));
            //wThread.Start();
            //Task.Run(() => core());
        }


        public bool IsEntercore = false;
        private void core()
        {
            //20240724
            MainVM.Wafer_Review.InspectedCount = 4105;
            Wafer.PassCount = 23406;

            //while (true)
            //{
            //    if (IsEntercore)
            //    {
            //        MainVM.Wafer_Review.InspectedCount = 48885;
            //        Wafer.PassCount = 7776;
            //    }
            //}
        }
    }
}
