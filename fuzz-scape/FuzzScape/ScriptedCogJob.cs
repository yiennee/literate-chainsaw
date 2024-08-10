using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ZephyrInnovations;

namespace FuzzScape
{
    public class ScriptedCogJob
    {
        public bool IsProcessing { get; set; } = false;
        public bool bEnableLog = true;

        public XElement ProcessImage(string imagefile, bool IsEditting)
        {
            return null;
        }

        /// <summary>
        /// if bIsError = true, Skip WriteLog
        /// </summary>
        /// <param name="sLog"></param>
        /// <param name="header"></param>
        /// <param name="bIsError"></param>
        public void WriteLog(string sLog, string header, bool bIsError = false)
        {
            if (bIsError)//if error, dont write log
                return;

            if (bEnableLog)//write log or not, depends on whether scripted cogjob log is enabled
                qGlobal.WriteVisionLog1(sLog, header);
        }
    }
}
