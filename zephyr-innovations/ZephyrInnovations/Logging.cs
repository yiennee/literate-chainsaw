using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using log4net;
using log4net.Config;

namespace ZephyrInnovations
{
    public enum LogTypeOptions
    {
        Info,
        Warning,
        Error,
        Nil,
    }
    public static class Logging
    {

    }

    public interface ILoggingFile
    {
        void SaveDirectory(string directoryPath, string logFileName);

        void AppendLogToFile(string fileLogMessage);
    }
    public interface ILoggingUI
    {
        void AppendLogToUI(string uiLogMessage);

        void AppendLogToUI(string uiLogMessage, LogTypeOptions logType);

        void ClearUI();
    }

    public static class LoggingManager
    {
        public static LoggingCore DefaultLogger = new LoggingCore();
    }

    public static class LoggingManager_Vision //yndebugtest 010423
    {
        public static LoggingCore DefaultLogger = new LoggingCore();
    }

    public static class LoggingManager_VisionDetail //yndebugtest 060423
    {
        public static LoggingCore DefaultLogger = new LoggingCore();
    }

    internal class StringNiceFormatter
    {
        public static string FormatStringWithNumbering(int number, string content, bool applySpacePaddingForEveryLineInContent = true, bool putNewLineAtTheEnd = true)
        {
            return StringNiceFormatter.FormatStringWithHeaderInFront(number.ToString() + ".", content, applySpacePaddingForEveryLineInContent, putNewLineAtTheEnd);
        }

        public static string FormatStringWithHeaderInFront(string header, string content, bool applySpacePaddingForEveryLineInContent = true, bool putNewLineAtTheEnd = true)
        {
            return string.Format("{0} {1}{2}", (object)header, applySpacePaddingForEveryLineInContent ? (object)content.Replace("\r\n", "\r\n" + new string(' ', (header + " ").Length)) : (object)content, putNewLineAtTheEnd ? (object)"\r\n" : (object)"");
        }

        public static string[] RemoveNumberingFromFormattedString(string formattedString, bool isSpacePaddingApplied = true)
        {
            string[] arrayStr = formattedString.Split(new string[1]
            {
        "\r\n"
            }, StringSplitOptions.RemoveEmptyEntries);
            List<string> strList1 = new List<string>();
            int index1 = 0;
            int num = 1;
        label_4:

            while (index1 < arrayStr.Length && arrayStr[index1].StartsWith(num.ToString() + ". "))
            {
                int length = (num.ToString() + ". ").Length;
                strList1.Add(arrayStr[index1].Substring(length, arrayStr[index1].Length - length));
                ++index1;

                while (true)
                {
                    if (index1 < arrayStr.Length && !arrayStr[index1].StartsWith((num + 1).ToString()))
                    {
                        List<string> strList2 = strList1;
                        int index2 = strList1.Count - 1;
                        strList2[index2] = strList2[index2] + "\r\n" + (isSpacePaddingApplied ? arrayStr[index1].Substring(length, arrayStr[index1].Length - length) : arrayStr[index1]);
                    }
                    else
                        goto label_4;
                }
            }

            return strList1.ToArray();
        }
    }
    public class LoggingCore
    {
        private string _finalLogMessage = string.Empty;
        private List<ILoggingUI> _ui;
        private List<ILoggingFile> _file;

        public LoggingCore()
        {
            this._ui = new List<ILoggingUI>();
            this._file = new List<ILoggingFile>();
        }

        public void AttachUI(ILoggingUI ui)
        {
            if (this._ui.Contains(ui))
                return;

            this._ui.Add(ui);
        }

        public void DetachUI(ILoggingUI ui)
        {
            if (!this._ui.Contains(ui))
                return;

            this._ui.Remove(ui);
        }

        public void ClearUI()
        {
            if (this._ui == null || this._ui.Count <= 0)
                return;

            this._ui.Clear();
        }

        public void AttachFile(ILoggingFile file)
        {
            if (this._file.Contains(file))
                return;

            this._file.Add(file);
        }

        public void DetachFile(ILoggingFile file)
        {
            if (!this._file.Contains(file))
                return;

            this._file.Remove(file);
        }

        public void ClearFile()
        {
            if (this._file == null || this._file.Count <= 0)
                return;

            this._file.Clear();
        }

        public void WriteInfoLog(string logContent, bool displayTimeStamp = true)
        {
            this.AppendLog((displayTimeStamp ? "[" + DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss.fff") + "]" : "") + " [INFO]", logContent, LogTypeOptions.Info);
        }

        public void WriteWarningLog(string logContent, bool displayTimeStamp = true)
        {
            this.AppendLog((displayTimeStamp ? "[" + DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss.fff") + "]" : "") + " [WARNING]", logContent, LogTypeOptions.Warning);
        }

        public void WriteErrorLog(string logContent, bool displayTimeStamp = true)
        {
            this.AppendLog((displayTimeStamp ? "[" + DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss.fff") + "]" : "") + " [ERROR]", logContent, LogTypeOptions.Error);
        }

        public void WriteIOLog1(string logContent, string messageType, bool displayTimeStamp = true)
        {
            this.AppendLog((displayTimeStamp ? "[" + DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss.fff") + "]" : "") + " [" + messageType + "]", logContent, LogTypeOptions.Error);
        }

        public void WriteIOLog2(string logContent, string messageType, bool displayTimeStamp = true)
        {
            this.AppendLog((displayTimeStamp ? "[" + DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss.fff") + "]" : "") + " [" + messageType + "]", logContent, LogTypeOptions.Error);
        }

        public void WriteRunLog(string logContent, string messageType, bool displayTimeStamp = true)
        {
            this.AppendLog((displayTimeStamp ? "[" + DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss.fff") + "]" : "") + " [" + messageType + "]", logContent, LogTypeOptions.Error);
        }

        public void WriteRunLog1(string logContent, string messageType, bool displayTimeStamp = true)
        {
            this.AppendLog((displayTimeStamp ? "[" + DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss.fff") + "]" : "") + " [" + messageType + "]", logContent, LogTypeOptions.Error);
        }

        public void WriteRunLog2(string logContent, string messageType, bool displayTimeStamp = true)
        {
            this.AppendLog((displayTimeStamp ? "[" + DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss.fff") + "]" : "") + " [" + messageType + "]", logContent, LogTypeOptions.Error);
        }

        public void WriteVisionLog(string logContent, string messageType, bool displayTimeStamp = true)
        {
            this.AppendLog((displayTimeStamp ? "[" + DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss.fff") + "]" : "") + " [" + messageType + "]", logContent, LogTypeOptions.Error);
        }

        public void WriteVisionLog1(string logContent, string messageType, bool displayTimeStamp = true)//yndebugtest
        {
            //this.AppendLog((displayTimeStamp ? DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss.fff") : "") + " " + messageType, logContent, LogTypeOptions.Error);//wafervm cmm
            this.AppendLog((displayTimeStamp ? DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss.fff") : "") + "; " + messageType + ";", logContent, LogTypeOptions.Error);
        }


        public void WriteVisionLog2(string logContent, string messageType, bool displayTimeStamp = true)
        {
            this.AppendLog((displayTimeStamp ? "[" + DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss.fff") + "]" : "") + " [" + messageType + "]", logContent, LogTypeOptions.Error);
        }

        public void WriteLog(string logContent, string logCategory = "", bool displayTimeStamp = true)
        {
            this.AppendLog((displayTimeStamp ? "[" + DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss.fff") + "]" : "") + " [" + logCategory + "]", logContent, LogTypeOptions.Nil);
        }

        private void AppendLog(string header, string content, LogTypeOptions logType)
        {
            string str = StringNiceFormatter.FormatStringWithHeaderInFront(header, content, true, false);

            if (this._ui.Count > 0)
            {
                foreach (ILoggingUI loggingUi in this._ui)
                    loggingUi.AppendLogToUI(str, logType);
            }

            if (this._file.Count <= 0)
                return;

            foreach (ILoggingFile loggingFile in this._file)
                loggingFile.AppendLogToFile(str);
        }
    }
    public class LogItem
    {
        private string _message;
        private Brush _color;

        public LogItem()
        {
            this.Message = "";
            this.MessageColor = (Brush)Brushes.DeepSkyBlue;
        }

        public string Message
        {
            get
            {
                return this._message;
            }
            set
            {
                this._message = value;
            }
        }

        public Brush MessageColor
        {
            get
            {
                return this._color;
            }
            set
            {
                this._color = value;
            }
        }

    }

    public class LogModule
    {
        private static object _logTextObject = new object();
        public static readonly ILog _log = LogManager.GetLogger(typeof(LogModule));
        private static LogItem _item;
        private static ObservableCollection<LogItem> _logText;

        public LogModule()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory, "log4net.config")));
        }

        public static ObservableCollection<LogItem> LogText
        {
            get
            {
                if (LogModule._logText == null)
                    LogModule._logText = new ObservableCollection<LogItem>();
                lock (LogModule._logTextObject)
                    return LogModule._logText;
            }
            set
            {
                lock (LogModule._logTextObject)
                    LogModule._logText = value;
            }
        }

        private static LogItem Item
        {
            get
            {
                return LogModule._item;
            }
            set
            {
                LogModule._item = value;
            }
        }

        public static void WriteWarning(string message)
        {
            LogModule.Item = new LogItem();
            LogModule.Item.Message = string.Format("[{0}]:Warning: {1}", (object)DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss.fff"), (object)message);
            LogModule.Item.MessageColor = (Brush)Brushes.Black;
            LogModule.AppendIntoLogText(LogModule.Item);
            LogModule._log.Warn((object)LogModule.Item.Message);
        }

        public static void WriteError(string message)
        {
            LogModule.Item = new LogItem();
            LogModule.Item.Message = string.Format("[{0}]:Error: {1}", (object)DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss.fff"), (object)message);
            LogModule.Item.MessageColor = (Brush)Brushes.Tomato;
            LogModule.AppendIntoLogText(LogModule.Item);
            LogModule._log.Error((object)LogModule.Item.Message);
        }

        public static void WriteInformation(string message)
        {
            LogModule.Item = new LogItem();
            LogModule.Item.Message = string.Format("[{0}]:Info: {1}", (object)DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss.fff"), (object)message);
            LogModule.Item.MessageColor = (Brush)Brushes.SteelBlue;
            LogModule.AppendIntoLogText(LogModule.Item);
            LogModule._log.Info((object)LogModule.Item.Message);
        }

        public static void WriteDebug(string message)
        {
            LogModule.Item = new LogItem();
            LogModule.Item.Message = string.Format("[{0}]:Debug: {1}", (object)DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss.fff"), (object)message);
            LogModule.Item.MessageColor = (Brush)Brushes.SteelBlue;
            LogModule.AppendIntoLogText(LogModule.Item);
            LogModule._log.Debug((object)LogModule.Item.Message);
        }

        private static void AppendIntoLogText(LogItem item)
        {
            if (LogModule.LogText.Count > 10000)
                LogModule.LogText.RemoveAt(0);

            LogModule.LogText.Add(item);
        }
    }

    public class LoggingFileText : ILoggingFile
    {
        private string _fullLogPath = (string)null;
        private ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        public void AppendLogToFile(string fileLogMessage)
        {
            if (string.IsNullOrEmpty(this._fullLogPath))
                return;

            this._readWriteLock.EnterWriteLock();

            try
            {
                using (StreamWriter streamWriter = new StreamWriter(this._fullLogPath, true))
                    streamWriter.WriteLine(fileLogMessage);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //return;

                string logfilepath = qGlobal.AppConfig.LogFileDirectory + @"\" + DateTime.Now.ToString("yyyy-MM-dd") + "_ExceptionLog.txt";
                File.AppendAllText(logfilepath, $"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")} ThreadID={Thread.CurrentThread.ManagedThreadId.ToString("D2")} Logging(AppendLogToFile){fileLogMessage} {ex.ToString()}\n");
                //return;
            }
            finally
            {
                this._readWriteLock.ExitWriteLock();
            }
        }

        public void SaveDirectory(string directoryPath, string logFileName)
        {
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            this._fullLogPath = directoryPath + "\\" + logFileName + ".txt";
        }
    }
}
