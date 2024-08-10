using Cognex.VisionPro.ImageFile;
using FuzzScape;
using IdealPancake;
using JungleGym;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using ZephyrInnovations;
using ZingifyDesigns;

namespace AnimatedDisco
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class UserControl : Window
    {
        public ScriptedCogJob runtimeCogJob;
        public List<FileInfo> ImageList = new List<FileInfo>();
        private int CurrentCount;
        private int LastCount;
        private object _lock = new object();
        public bool IsStartProcess;
        public object currentEditor = new object();

        private bool bEnableLog;
        private object cogjobprocessinglocker = new object();

        private Dictionary<FSWImageInput, string> input_recipe_dic = new Dictionary<FSWImageInput, string>();
        private Dictionary<string, IResult> path_output_dic = new Dictionary<string, IResult>();
        private List<ScriptedCogJob> cogjobs = new List<ScriptedCogJob>();

        string FSWInputImageSetting_Path;
        string CogJobResultRetriever_Path;
        string library_path;

        public static string basedir = AppDomain.CurrentDomain.BaseDirectory;
        public static string workingfolderpath = basedir.Replace(@"\Debug\", @"\WorkingFolder\").Replace(@"\Release\", @"\WorkingFolder\");

        public static string SettingXMLFile = string.Format("{0}{1}", workingfolderpath, "LibraryConfiguration.xml");

        public UserControl()
        {
            InitializeComponent();

            XDocument librarydoc = XDocument.Load(System.IO.Path.Combine(workingfolderpath, "LibraryConfiguration.xml"));

            FSWInputImageSetting_Path = librarydoc.Root.Element("FSWInputImageSetting_Path").Value;
            CogJobResultRetriever_Path = librarydoc.Root.Element("CogJobResultRetriever_Path").Value;
            library_path = librarydoc.Root.Element("LibraryPath").Value;

            ImageIncomingPath = System.IO.Path.Combine(workingfolderpath, @"TransactionPath\To");
            //string sImageOutgoingPath = System.IO.Path.Combine(workingfolderpath, @"TransactionPath\From");
            //string sRecipeName = "";
            ////ConfigureResultRetriever(sImageIncomingPath, sImageOutgoingPath, sRecipeName);

            //timer.Elapsed += OnTimedEvent;
            //timer.AutoReset = true;
            //timer.Enabled = true;

            //// Start a thread to process the working queue
            //Thread workerThread = new Thread(ProcessQueue);
            //workerThread.Start();

            //Console.WriteLine("Press [Enter] to exit the application.");
            //Console.ReadLine();
        }

        private void ProcessQueue()
        {
            while (true)
            {
                if (workingQueue.TryDequeue(out FileInfo file))
                {
                    // Process the file
                    Console.WriteLine($"Processing {file.Name}");
                    string logMsg = string.Format("ProcessQueue; {0} dequeued; QueueCount= {1}", file.Name, workingQueue.Count); qGlobal.WriteVisionLog1(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));
                    // Simulate work with Thread.Sleep
                    Thread.Sleep(10);
                }
                else
                {
                    // No files to process, sleep for a while
                    Thread.Sleep(500);
                }
            }
        }

        private static readonly ConcurrentDictionary<string, FileInfo> processedFiles = new ConcurrentDictionary<string, FileInfo>();
        private static readonly ConcurrentQueue<FileInfo> workingQueue = new ConcurrentQueue<FileInfo>();
        private static readonly ConcurrentQueue<CustomFileInfo> workingQueue_Custom = new ConcurrentQueue<CustomFileInfo>();
        private static readonly System.Timers.Timer timer = new System.Timers.Timer(5000); // Interval in milliseconds (e.g., 5000ms = 5 seconds)
        private static CancellationTokenSource cancellationTokenSource_TimerTask;
        private static Task[] processingTasks;
        private static Task[] imgprocessingTasks;
        string ImageIncomingPath = "";
        private string uclogMsg;

        public bool LoadRecipeOnTask(string sImageIncomingPath, string sImageOutgoingPath, string sRecipeName, double ConversionRatio = 1, string ConversionUnit = "Pixel")
        {
            Stopwatch Watch = Stopwatch.StartNew();

            try
            {
                ImageIncomingPath = sImageIncomingPath;

                timer.Elapsed += OnTimedEvent;
                timer.AutoReset = true;
                timer.Enabled = true;


                // Start multiple tasks to process the working queue
                cancellationTokenSource_TimerTask = new CancellationTokenSource();
                CancellationToken token_TimerTask = cancellationTokenSource_TimerTask.Token;
                int numberOfTasks = qGlobal.VisionProcessorConfig.EnableMultiThread ? qGlobal.VisionProcessorConfig.MaxDegreeofParallelism : 1; // Number of concurrent tasks

                imgprocessingTasks = new Task[numberOfTasks];
                processingTasks = new Task[numberOfTasks];
                for (int i = 0; i < numberOfTasks; i++)
                {
                    //ScriptedCogJob newcogjob = new ScriptedCogJob(_myCogDisplay, SetTrainStepView) { IsProcessing = true };

                    ////logMsg = string.Format("{0} {1}", System.IO.Path.GetFileNameWithoutExtension(fi.Name), "LoadRecipe[START]"); currentcogjob.WriteLog(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));//ppi4424
                    //Application.Current.Dispatcher.Invoke((Action)delegate
                    //{
                    //    newcogjob.LibraryFileName = sRecipeName;
                    //    newcogjob.LoadLibrary(true, false);//*load during process if needed
                    //    newcogjob.IsProcessing = false;
                    //    if (library_to_flow_dic.ContainsKey(sRecipeName))
                    //        newcogjob.ChangeProcessFlow(library_to_flow_dic[sRecipeName]);

                    //    cogjobs.Add(newcogjob);
                    //});

                    //imgprocessingTasks[i] = Task.Factory.StartNew(() => imgProcessQueue(token_TimerTask), token_TimerTask);
                    //processingTasks[i] = Task.Factory.StartNew(() => ProcessQueue(token_TimerTask), token_TimerTask);
                    //processingTasks[i] = Task.Run(() => ProcessQueue(token_TimerTask), token_TimerTask);
                    imgprocessingTasks[i] = Task.Run(() => imgProcessQueue(token_TimerTask), token_TimerTask);
                }
                processingTasks[0] = Task.Run(() => ProcessQueue(token_TimerTask), token_TimerTask);
                //processingTasks[0] = Task.Factory.StartNew(() => ProcessQueue(token_TimerTask), token_TimerTask);

                Watch.Stop();
                uclogMsg = string.Format("{0} {1}", "LoadRecipeOnTask_CogJobList", Watch.ElapsedMilliseconds.ToString()); //cogjobs[0].WriteLog(uclogMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));
                return true;
            }
            catch (Exception ex)
            {
                uclogMsg = string.Format("{0} {1}", "LoadRecipeOnTask", "Error"); runtimeCogJob.WriteLog(uclogMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));
                MessageBox.Show($"LoadRecipeOnTask Error : {ex}");
                return false;
            }
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(ImageIncomingPath);
            FileInfo[] files = directoryInfo.GetFiles("*.tif").ToArray();

            foreach (FileInfo file in files)
            {
                if (processedFiles.TryAdd(file.FullName, file))
                {
                    //using (CogImageFile ImgFile = new CogImageFile())
                    //{
                    //    try
                    //    {
                    //        ImgFile.Open(file.FullName, CogImageFileModeConstants.Read);
                    //        CogImageFileTool ImgFileTool = new CogImageFileTool();
                    //        ImgFileTool.Operator = ImgFile;
                    //        ImgFileTool.Run();

                    //        var output = ImgFileTool.OutputImage;
                    //        CustomFileInfo customFI = new CustomFileInfo(file.FullName, output);
                    //        workingQueue_Custom.Enqueue(customFI);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        MessageBox.Show($"OnTimedEvent Error : {ex}");
                    //    }
                    //}

                    workingQueue.Enqueue(file);
                    string logMsg = string.Format("{0} IN; workingQueueCount= {1}", file.Name, workingQueue.Count); qGlobal.WriteVisionLog1(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));
                }
            }
        }

        private async Task imgProcessQueue(CancellationToken token)
        {
            while (true)
            {
                // Check if cancellation is requested
                if (token.IsCancellationRequested)
                {
                    string logMsg = string.Format("imgPQ Cancellation requested"); qGlobal.WriteVisionLog1(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));
                    break;
                }

                if (workingQueue.TryDequeue(out FileInfo file))
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(55000);
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    bool Isopen = false;
                    while (!Isopen)
                    {
                        try
                        {
                            using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                            {
                                Isopen = true;
                                // File opened successfully, return true
                            }
                        }
                        catch (IOException IOex)
                        {
                            // Log the exception
                            Console.WriteLine($"{file} IOException {IOex}");
                            //string logMsg1 = string.Format("XYZ {0} wait...", file.Name); qGlobal.WriteVisionLog1(logMsg1, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));

                        }
                        Task.Delay(500, token);
                    }

                    using (CogImageFile ImgFile = new CogImageFile())
                    {
                        try
                        {
                            ImgFile.Open(file.FullName, CogImageFileModeConstants.Read);
                            CogImageFileTool ImgFileTool = new CogImageFileTool();
                            ImgFileTool.Operator = ImgFile;
                            ImgFileTool.Run();

                            var output = ImgFileTool.OutputImage;
                            CustomFileInfo customFI = new CustomFileInfo(file.FullName, output);
                            workingQueue_Custom.Enqueue(customFI);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"XYZ Error : {ex}");
                        }
                    }

                    // Process the file
                    string logMsg = string.Format("AddToCogQueue; File={0}; FI_Queue={1}; Cog_Queue={2}", System.IO.Path.GetFileNameWithoutExtension(file.FullName), workingQueue.Count, workingQueue_Custom.Count); qGlobal.WriteVisionLog1(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));
                    //ProcessImage_TryLoadRecipe(file, token);

                    // Simulate work with Task.Delay
                    await Task.Delay(50, token);
                }
                //if (workingQueue.TryDequeue(out FileInfo file))
                //{
                //    TimeSpan timeout = TimeSpan.FromMilliseconds(55000);
                //    Stopwatch stopwatch = Stopwatch.StartNew();
                //    bool Isopen = false;
                //    while (!Isopen)
                //    {
                //        try
                //        {
                //            using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                //            {
                //                Isopen = true;
                //                // File opened successfully, return true
                //            }
                //        }
                //        catch (IOException IOex)
                //        {
                //            // Log the exception
                //            Console.WriteLine($"{file} IOException {IOex}");
                //            string logMsg1 = string.Format("imgPQ(IOException) {0} {1}", file.Name, IOex); qGlobal.WriteVisionLog1(logMsg1, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));

                //        }
                //        Task.Delay(50, token);
                //    }

                //    using (CogImageFile ImgFile = new CogImageFile())
                //    {
                //        try
                //        {
                //            ImgFile.Open(file.FullName, CogImageFileModeConstants.Read);
                //            CogImageFileTool ImgFileTool = new CogImageFileTool();
                //            ImgFileTool.Operator = ImgFile;
                //            ImgFileTool.Run();

                //            var output = ImgFileTool.OutputImage;
                //            CustomFileInfo customFI = new CustomFileInfo(file.FullName, output);
                //            workingQueue_Custom.Enqueue(customFI);
                //        }
                //        catch (Exception ex)
                //        {
                //            MessageBox.Show($"imgProcessQueue Error : {ex}");
                //        }
                //    }

                //    // Process the file
                //    string logMsg = string.Format("imgPQ(Parallel); {0} ; imgPQCount= {1}", file.FullName, workingQueue.Count); qGlobal.WriteVisionLog1(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));

                //    //ProcessImage_TryLoadRecipe(file, token);

                //    // Simulate work with Task.Delay
                //    await Task.Delay(50, token);
                //}
                //if (workingQueue.TryDequeue(out CustomFileInfo csfile))
                //{


                //    // Process the file
                //    string logMsg = string.Format("imgPQ(Parallel); {0} cscsw*; imgPQCount= {1}", csfile.FileName, workingQueue.Count); qGlobal.WriteVisionLog1(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));

                //    //ProcessImage_TryLoadRecipe(file, token);

                //    // Simulate work with Task.Delay
                //    await Task.Delay(50, token);
                //}
                else
                {
                    // No files to process, delay for a while
                    await Task.Delay(150, token);
                }
            }
        }

        private async Task ProcessQueue(CancellationToken token)
        {
            while (true)
            {
                // Check if cancellation is requested
                if (token.IsCancellationRequested)
                {
                    string logMsg = string.Format("ProcessQueue Cancellation requested"); qGlobal.WriteVisionLog1(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));
                    break;
                }

                if (workingQueue_Custom.TryDequeue(out CustomFileInfo csfile))//cmm for queue from origin
                {
                    // Process the file
                    string logMsg = string.Format("ProcessCog; File={0}; FI_Queue={1}; Cog_Queue={2}", System.IO.Path.GetFileNameWithoutExtension(csfile.FileName), workingQueue.Count, workingQueue_Custom.Count); qGlobal.WriteVisionLog1(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));


                    //ProcessImage_TryLoadRecipe(file, token);

                    // Simulate work with Task.Delay
                    await Task.Delay(550, token);
                }

                //if (workingQueue.TryDequeue(out FileInfo file))
                //{
                //    if (file == null)
                //        Console.WriteLine("nullllllll");
                //    TimeSpan timeout = TimeSpan.FromMilliseconds(55000);
                //    Stopwatch stopwatch = Stopwatch.StartNew();
                //    bool Isopen = false;
                //    while (!Isopen)
                //    {
                //        try
                //        {
                //            using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                //            {
                //                Isopen = true;
                //                // File opened successfully, return true
                //            }
                //        }
                //        catch (IOException IOex)
                //        {
                //            // Log the exception
                //            Console.WriteLine($"{file} IOException {IOex}");
                //            //string logMsg1 = string.Format("XYZ {0} wait...", file.Name); qGlobal.WriteVisionLog1(logMsg1, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));

                //        }
                //        Task.Delay(500, token);
                //    }

                //    using (CogImageFile ImgFile = new CogImageFile())
                //    {
                //        try
                //        {
                //            ImgFile.Open(file.FullName, CogImageFileModeConstants.Read);
                //            CogImageFileTool ImgFileTool = new CogImageFileTool();
                //            ImgFileTool.Operator = ImgFile;
                //            ImgFileTool.Run();

                //            var output = ImgFileTool.OutputImage;
                //            CustomFileInfo customFI = new CustomFileInfo(file.FullName, output);
                //            //workingQueue_Custom.Enqueue(customFI);
                //        }
                //        catch (Exception ex)
                //        {
                //            MessageBox.Show($"XYZ Error : {ex}");
                //        }
                //    }

                //    // Process the file
                //    string logMsg = string.Format("ProcessQueue; File={0}; FI_Queue={1}; Cog_Queue={2}", System.IO.Path.GetFileNameWithoutExtension(file.FullName), workingQueue.Count, workingQueue_Custom.Count); qGlobal.WriteVisionLog1(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));
                //    //ProcessImage_TryLoadRecipe(file, token);

                //    // Simulate work with Task.Delay
                //    await Task.Delay(50, token);
                //}
                else
                {
                    // No files to process, delay for a while
                    await Task.Delay(150, token);
                }
            }
        }

        public void ConfigureResultRetriever(string sImageIncomingPath, string sImageOutgoingPath, string sRecipeName)
        {
            FSWImageInput fswimageinput = new FSWImageInput(XDocument.Load(System.IO.Path.Combine(workingfolderpath, FSWInputImageSetting_Path)));
            fswimageinput.IncomingPath = sImageIncomingPath;
            fswimageinput.SetProcessAction(Process);
            fswimageinput.SetErrorAction(HandlePathError);

            if (path_output_dic.ContainsKey(sImageIncomingPath))
            {
                path_output_dic.Remove(sImageIncomingPath);
            }

            var input_recipe = input_recipe_dic.FirstOrDefault(dic => dic.Key.IncomingPath == sImageIncomingPath);
            if (input_recipe.Key != null)
            {
                input_recipe.Key.Dispose();
                input_recipe_dic.Remove(input_recipe.Key);
            }

            input_recipe_dic.Add(fswimageinput, sRecipeName);

            CogJobResultRetriever retriever = new CogJobResultRetriever(XDocument.Load(System.IO.Path.Combine(workingfolderpath, CogJobResultRetriever_Path)));
            //retriever.OutputPath = sImageOutgoingPath;
            //retriever.EnableLog = bEnableLog;
            path_output_dic.Add(sImageIncomingPath, retriever);
        }

        public void Process(FileInfo fi)
        {
            //return;//testing 160424
            try
            {
                lock (ImageList)
                {
                    if (ImageList.All(e => e.FullName != fi.FullName))
                    {
                        ImageList.Add(fi);
                        string logMsg = string.Format("{0} IN; Count= {1}", fi.Name, ImageList.Count); qGlobal.WriteVisionLog1(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));
                    }
                }
            }
            catch (Exception ex)
            {
                lock (currentEditor)
                {
                    string logfilepath = qGlobal.AppConfig.LogFileDirectory + @"\" + DateTime.Now.ToString("yyyy-MM-dd") + "_ExceptionLog.txt";
                    File.AppendAllText(logfilepath, $"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")} ThreadID={Thread.CurrentThread.ManagedThreadId.ToString("D2")} UserControl(Process) {ex.ToString()}\n");

                    Console.WriteLine($"UC=Process(FI)={fi.Name} Catched");
                }
            }
            finally
            {

            }
        }

        public void HandlePathError(string path, string filter)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] fis = di.GetFiles(filter);

            string logMsg = string.Format("{0} {1}", "HandlePath Error", path); qGlobal.WriteVisionLog1(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));
            foreach (var fi in fis)
            {
                string logMsg1 = string.Format("{0} {1}", "PathErrorFile", fi.Name); qGlobal.WriteVisionLog1(logMsg1, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));
                Process(fi);
            }
        }

        private CancellationTokenSource cancellationTokenSource;
        public async Task StartLoopAsync()
        {
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            try
            {
                await Task.Run(() => Loop(cancellationToken), cancellationToken);
            }
            catch (OperationCanceledException ex)
            {
                string logfilepath = @"C:\QVS\Log\" + "PrintTestProgram_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                File.AppendAllText(logfilepath, $"{DateTime.Now.ToString("HH:mm:ss.fff")}; Exception; StartLoopAsync; {ex}" + "\n");
            }
        }

        public void StopLoop()
        {
            cancellationTokenSource?.Cancel();

            //20240722
            // Stop the timer
            timer.Enabled = false;

            // Signal cancellation and wait for the tasks to complete
            cancellationTokenSource_TimerTask?.Cancel();
            try
            {
                if (imgprocessingTasks != null)
                    Task.WaitAll(imgprocessingTasks);
                if (processingTasks != null)
                    Task.WaitAll(processingTasks);
            }
            catch (AggregateException ex)
            {
                // Handle the case where tasks were cancelled
                string logMsg = string.Format("StopLoop; AggregateException {0}", ex.ToString()); qGlobal.WriteVisionLog1(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));
            }

            //Reset
            // Clear the collections
            processedFiles.Clear();
            while (workingQueue.TryDequeue(out _)) { }

            Console.WriteLine("Operation reset and restarted.");
        }

        void Loop(CancellationToken cancellationToken)
        {
            //string logfilepath;
            string logfilepath = @"C:\QVS\Log\" + "PrintTestProgram_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            string logMsg = string.Format("{0} {1}", "UserControl(Loop)", "STARTUP"); qGlobal.WriteVisionLog1(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));
            File.AppendAllText(logfilepath, $"{DateTime.Now.ToString("HH:mm:ss.fff")}; UC; StartLoopAsync; {logMsg}" + "\n");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (!IsStartProcess)
                        continue;
                    if (ImageList.Count > 0)
                    {
                        Stopwatch _watch = Stopwatch.StartNew();

                        if (qGlobal.VisionProcessorConfig.EnableMultiThread)
                        {
                            CurrentCount = ImageList.Count;
                            int ProcessCount = CurrentCount - LastCount > qGlobal.VisionProcessorConfig.MaxDegreeofParallelism ? qGlobal.VisionProcessorConfig.MaxDegreeofParallelism : CurrentCount - LastCount;

                            var subList = ImageList.Skip(LastCount).Take(ProcessCount).ToList();

                            if (ProcessCount == 0)
                            {
                                //do nothing
                            }
                            else
                            {

                                try
                                {
                                    logMsg = string.Format("MaxDegreeofParallelism={0} LastCount={1};CurrentCount={2};Take={3};SubListCount={4}", qGlobal.VisionProcessorConfig.MaxDegreeofParallelism, LastCount, CurrentCount, CurrentCount - LastCount, subList.Count);
                                    qGlobal.WriteVisionLog1(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));
                                    File.AppendAllText(logfilepath, $"{DateTime.Now.ToString("HH:mm:ss.fff")}; UC; StartLoopAsync(ImageList); {logMsg}" + "\n");

                                    LastCount += ProcessCount;

                                    if (subList.Count > 0)
                                    {


                                        //var parallell_in_thread = new Thread(() =>
                                        //{
                                        //    //ParallelOptions _option = new ParallelOptions() { MaxDegreeOfParallelism = qGlobal.VisionProcessorConfig.TotalCameraShot };
                                        //    //Parallel.ForEach(testcogList, _option, i =>//use parallel foreach vs. async due to collection can be large and vision process is deemed heavy task
                                        //    //{
                                        //    //    i.TestLoadImageOnly();
                                        //    //});


                                        ParallelOptions _option = new ParallelOptions() { CancellationToken = cancellationToken, MaxDegreeOfParallelism = qGlobal.VisionProcessorConfig.MaxDegreeofParallelism };
                                        Parallel.ForEach(subList, _option, i =>//use parallel foreach vs. async due to collection can be large and vision process is deemed heavy task
                                        {
                                            Console.WriteLine($"Loop cancellation request={cancellationToken.IsCancellationRequested}; Parallel.ForEach");
                                            cancellationToken.ThrowIfCancellationRequested();
                                            //ProcessImage_TryLoadRecipe(i, cancellationToken);
                                            //ProcessImage(i);//ppi4424
                                        });

                                        //});
                                        //parallell_in_thread.Start();
                                    }
                                    _watch.Stop();

                                    //foreach (var t in subList)
                                    //{
                                    //    //ImageList.Remove(t);
                                    //    Console.WriteLine($"removed {t}");
                                    //}

                                    logMsg = string.Format("MaxDegreeofParallelism={0} SubListCount={1} CYCLETIME {2} ms", qGlobal.VisionProcessorConfig.MaxDegreeofParallelism, subList.Count, _watch.ElapsedMilliseconds.ToString());
                                    qGlobal.WriteVisionLog1(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));
                                    File.AppendAllText(logfilepath, $"{DateTime.Now.ToString("HH:mm:ss.fff")}; UC; StartLoopAsync(ImageList); {logMsg}" + "\n");

                                }
                                catch (Exception ex)
                                {
                                    lock (currentEditor)
                                    {
                                        logfilepath = qGlobal.AppConfig.LogFileDirectory + @"\" + DateTime.Now.ToString("yyyy-MM-dd") + "_ExceptionLog.txt";
                                        File.AppendAllText(logfilepath, $"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")} ThreadID={Thread.CurrentThread.ManagedThreadId.ToString("D2")} UserControl(BGW_ProcessImage)_SubListException {ex.ToString()}\n");
                                    }
                                }
                            }
                        }
                        else
                        {
                            lock (ImageList)
                            {
                                Func<FileInfo, Point> CoordOf = (p) =>
                                {
                                    double UnitX = Convert.ToInt16(p.Name.Substring(p.Name.IndexOf('[') + 1, 2));
                                    double UnitY = Convert.ToInt16(p.Name.Substring(p.Name.LastIndexOf('[') + 1, 2));

                                    Point Pt = new Point(UnitX, UnitY);

                                    return Pt;
                                };
                                //int CoordX = Convert.ToInt16(currentUnitFileName.Substring(currentUnitFileName.IndexOf('[') + 1, 2));
                                //int CoordY = Convert.ToInt16(currentUnitFileName.Substring(currentUnitFileName.LastIndexOf('[') + 1, 2));

                                if (false)
                                {

                                    ImageList.Sort((p, q) =>
                                    {

                                        if (CoordOf(p).X >= CoordOf(q).X && CoordOf(p).Y >= CoordOf(q).Y)//(1,2) (2,1) (2,2)
                                            return 1;
                                        else if (CoordOf(p).X > CoordOf(q).X)//(2,0)
                                            return 1;

                                        return -1;
                                    });
                                }

                                var fi = ImageList.First();
                                if (fi == null)
                                    continue;

                                ImageList.Remove(fi);

                                if (File.Exists(fi.FullName))
                                {
                                    ProcessImage_LoadedRecipe(fi, cancellationToken);
                                    //ProcessImage(fi);//ppi4424
                                }
                            }
                        }
                    }
                }
                //catch (Exception ex)
                //{
                //    lock (currentEditor)
                //    {
                //        logfilepath = qGlobal.AppConfig.LogFileDirectory + @"\" + DateTime.Now.ToString("yyyy-MM-dd") + "_ExceptionLog.txt";
                //        File.AppendAllText(logfilepath, $"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")} ThreadID={Thread.CurrentThread.ManagedThreadId} UserControl(BGW_ProcessImage) {ex.ToString()}\n");
                //    }
                //}
                catch (OperationCanceledException ex)
                {
                    // Handle cancellation
                    Console.WriteLine($"Loop was cancelled. {ex}");
                }

                Thread.Sleep(50);
                //cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private void ProcessImage_LoadedRecipe(Object obj_fi, CancellationToken cancellationToken)//using runtimeCogJob
        {
            try
            {
                //cancellationToken.ThrowIfCancellationRequested();//240424 test if comment out ok
                string logMsg;
                TimeSpan totalElapsedTime = TimeSpan.Zero;
                Stopwatch Watch = Stopwatch.StartNew();

                runtimeCogJob.IsProcessing = true;//no effect to entering ProcessImage
                FileInfo fi = obj_fi as FileInfo;
                XElement ele_trainstep_results;// = new XElement();// runtimeCogJob.ProcessImage(fi.FullName, false);
                Watch.Stop();

                totalElapsedTime = totalElapsedTime.Add(Watch.Elapsed);
                logMsg = string.Format("{0} {1} {2} ms", System.IO.Path.GetFileNameWithoutExtension(fi.FullName), "ProcessImage", Watch.ElapsedMilliseconds.ToString()); //runtimeCogJob.WriteLog(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2")); OnWriteRunLog(logMsg);

                runtimeCogJob.IsProcessing = false;

                //Watch.Reset(); Watch.Start();
                //IResult resultRetriever = path_output_dic.FirstOrDefault(dic => dic.Key.TrimEnd('\\') == fi.Directory.FullName.TrimEnd('\\')).Value;//camera save path | IResult
                //resultRetriever.OutputResult(new object[] { ele_trainstep_results, fi, runtimeCogJob.CurrentFlowSetting.EnableOutputGraphic, CanSaveRawImage, RawImageBackupPath }, out string sErrMsg);
                //Watch.Stope();

                totalElapsedTime = totalElapsedTime.Add(Watch.Elapsed);
                logMsg = string.Format("{0} {1} {2} ms", System.IO.Path.GetFileNameWithoutExtension(fi.FullName), "CreateResult", Watch.ElapsedMilliseconds.ToString()); //runtimeCogJob.WriteLog(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2")); OnWriteRunLog(logMsg);
                //logMsg = string.Format("{0} {1} {2} ms", System.IO.Path.GetFileNameWithoutExtension(fi.FullName), "TotalProcessTime", totalElapsedTime.TotalMilliseconds.ToString()); runtimeCogJob.WriteLog(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2")); OnWriteRunLog(logMsg);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Loop cancellation request={cancellationToken.IsCancellationRequested}; ProcessImage_LoadedRecipe");
                lock (currentEditor)
                {
                    string logfilepath = qGlobal.AppConfig.LogFileDirectory + @"\" + DateTime.Now.ToString("yyyy-MM-dd") + "_ExceptionLog.txt";
                    File.AppendAllText(logfilepath, $"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")} ThreadID={Thread.CurrentThread.ManagedThreadId.ToString("D2")} UserControl(ProcessImage_LoadedRecipe) {ex.ToString()}\n");
                }
            }
            finally
            {
                Console.WriteLine($"Loop cancellation request={cancellationToken.IsCancellationRequested}; ProcessImage_LoadedRecipe");
                //cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private void ProcessImage_TryLoadRecipe(Object obj_fi, CancellationToken cancellationToken)//check processing flag; use non-processeing instance or create new instance
        {
            try
            {
                string logMsg;
                TimeSpan totalElapsedTime = TimeSpan.Zero;
                Stopwatch Watch = Stopwatch.StartNew();

                FileInfo fi = obj_fi as FileInfo;
                ScriptedCogJob currentcogjob = null;


                var FullName = fi.FullName;
                var Name = fi.Name;
                var fileExtension = fi.Extension;

                //lock (cogjobprocessinglocker)
                //{
                //    currentcogjob = cogjobs.FirstOrDefault(cogjob => !cogjob.IsProcessing);
                //}

                while (currentcogjob == null) // Continue until myClass is not null
                {
                    currentcogjob = cogjobs.FirstOrDefault(cogjob => !cogjob.IsProcessing);
                    if (currentcogjob == null) // If myClass is still null, sleep for a while before retrying
                    {
                        Thread.Sleep(200); // Adjust the sleep duration as per your requirement //change 1000 to 200
                    }
                }


                //if (currentcogjob == null)
                //{
                //    currentcogjob = new ScriptedCogJob(_myCogDisplay, SetTrainStepView) { IsProcessing = true };

                //    //logMsg = string.Format("{0} {1}", System.IO.Path.GetFileNameWithoutExtension(fi.Name), "LoadRecipe[START]"); currentcogjob.WriteLog(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));//ppi4424
                //    Application.Current.Dispatcher.Invoke((Action)delegate
                //    {
                //        currentcogjob.LibraryFileName = runtimeCogJob.LibraryFileName;
                //        currentcogjob.LoadLibrary(true, false);//*load during process if needed
                //        if (library_to_flow_dic.ContainsKey(runtimeCogJob.LibraryFileName))
                //            currentcogjob.ChangeProcessFlow(library_to_flow_dic[runtimeCogJob.LibraryFileName]);

                //        lock (cogjobprocessinglocker)
                //        {
                //            cogjobs.Add(currentcogjob);
                //        }
                //    });
                //    logMsg = string.Format("{0} {1}", System.IO.Path.GetFileNameWithoutExtension(fi.FullName), "LoadRecipe[NEWRecipe]"); currentcogjob.WriteLog(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));
                //}
                //else
                //{
                lock (cogjobprocessinglocker)
                {
                    currentcogjob.IsProcessing = true;
                }
                //    logMsg = string.Format("{0} {1}", System.IO.Path.GetFileNameWithoutExtension(fi.FullName), "LoadRecipe[SKIP]"); currentcogjob.WriteLog(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));
                //}
                Watch.Stop();

                totalElapsedTime = totalElapsedTime.Add(Watch.Elapsed);
                logMsg = string.Format("{0} {1} {2} ms", "ProcessImage_TryLoadRecipe", "LoadRecipe", Watch.ElapsedMilliseconds.ToString()); currentcogjob.WriteLog(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2")); OnWriteRunLog(logMsg);

                XElement ele_trainstep_results = currentcogjob.ProcessImage(fi.FullName, false);
                Watch.Stop();

                totalElapsedTime = totalElapsedTime.Add(Watch.Elapsed);
                logMsg = string.Format("{0} {1} {2} ms", System.IO.Path.GetFileNameWithoutExtension(fi.FullName), "ProcessImage", Watch.ElapsedMilliseconds.ToString()); currentcogjob.WriteLog(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2")); OnWriteRunLog(logMsg);


                Watch.Reset(); Watch.Start();
                IResult resultRetriever = path_output_dic.FirstOrDefault(dic => dic.Key.TrimEnd('\\') == fi.Directory.FullName.TrimEnd('\\')).Value;//camera save path | IResult
                resultRetriever.OutputResult(new object[] { ele_trainstep_results, fi, true, true, "" }, out string sErrMsg);
                Watch.Stop();

                totalElapsedTime = totalElapsedTime.Add(Watch.Elapsed);
                logMsg = string.Format("{0} {1} {2} ms", System.IO.Path.GetFileNameWithoutExtension(fi.FullName), "CreateResult", Watch.ElapsedMilliseconds.ToString()); currentcogjob.WriteLog(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2")); OnWriteRunLog(logMsg);
                //logMsg = string.Format("{0} {1} {2} ms", System.IO.Path.GetFileNameWithoutExtension(fi.FullName), "TotalProcessTime", totalElapsedTime.TotalMilliseconds.ToString()); currentcogjob.WriteLog(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2")); OnWriteRunLog(logMsg);

                lock (cogjobprocessinglocker)
                {
                    currentcogjob.IsProcessing = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Loop cancellation request={cancellationToken.IsCancellationRequested}; ProcessImage_TryLoadRecipe");
                lock (currentEditor)
                {
                    string logfilepath = qGlobal.AppConfig.LogFileDirectory + @"\" + DateTime.Now.ToString("yyyy-MM-dd") + "_ExceptionLog.txt";
                    File.AppendAllText(logfilepath, $"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")} ThreadID={Thread.CurrentThread.ManagedThreadId.ToString("D2")} UserControl(ProcessImage_TryLoadRecipe) {ex.ToString()}\n");
                }
            }
            finally
            {
                Console.WriteLine($"Loop cancellation request={cancellationToken.IsCancellationRequested}; ProcessImage_TryLoadRecipe");
                //cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public event EventHandler<string> WriteLogEventHandler;
        public void OnWriteRunLog(string logMsg)
        {
            //if (WriteLogEventHandler != null)
            //    WriteLogEventHandler?.Invoke(this, logMsg);
        }

        private void BtnStartLoop_Click(object sender, RoutedEventArgs e)
        {
            StartLoop_TimerTask();
        }

        public void StartLoop_TimerTask()//simulate
        {
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;

            // Start multiple tasks to process the working queue
            cancellationTokenSource_TimerTask = new CancellationTokenSource();
            CancellationToken token_TimerTask = cancellationTokenSource_TimerTask.Token;
            int numberOfTasks = 4; // Number of concurrent tasks

            ////for (int i = 0; i < numberOfTasks; i++)
            ////{
            ////    Task.Factory.StartNew(() => ProcessQueue(token_TimerTask), token_TimerTask);
            ////}
            //processingTasks = new Task[numberOfTasks];
            //for (int i = 0; i < numberOfTasks; i++)
            //{
            //    processingTasks[i] = Task.Factory.StartNew(() => ProcessQueue(token_TimerTask), token_TimerTask);
            //}

            imgprocessingTasks = new Task[numberOfTasks];
            processingTasks = new Task[numberOfTasks];
            for (int i = 0; i < numberOfTasks; i++)
            {
                imgprocessingTasks[i] = Task.Run(() => imgProcessQueue(token_TimerTask), token_TimerTask);
            }
            processingTasks[0] = Task.Run(() => ProcessQueue(token_TimerTask), token_TimerTask);
        }

        private void BtnCancelLoop_Click(object sender, RoutedEventArgs e)
        {
            StopLoop_TimerTask();
        }

        public void StopLoop_TimerTask()//simulate
        {
            // Stop the timer
            timer.Enabled = false;

            // Signal cancellation and wait for the tasks to complete
            cancellationTokenSource_TimerTask.Cancel();
            try
            {
                Task.WaitAll(processingTasks);
            }
            catch (AggregateException ex)
            {
                // Handle the case where tasks were cancelled
                string logMsg = string.Format("StopLoop; AggregateException {0}", ex.ToString()); qGlobal.WriteVisionLog1(logMsg, "ThreadID=" + Thread.CurrentThread.ManagedThreadId.ToString("D2"));
            }

            Console.WriteLine("Operation stopped.");

            //Reset
            // Clear the collections
            processedFiles.Clear();
            while (workingQueue.TryDequeue(out _)) { }

            Console.WriteLine("Operation reset and restarted.");
        }
    }


    // Custom class to hold FileInfo and ID
    public class CustomFileInfo
    {
        public string FileName { get; set; }
        public object OutputImage { get; set; }

        public CustomFileInfo(string filename, object id)
        {
            FileName = filename;
            OutputImage = id;
        }
    }
}
