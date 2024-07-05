using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JungleGym
{
    public class CogJobResultRetriever : QVS_Interfaces.IResult
    {
        XDocument settingXml;
        string ResultXmlPath;
        public CogJobResultRetriever(XDocument doc)
        {
            settingXml = doc;
            ResultXmlPath = settingXml.Root.Element("ResultXmlPath").Value;
        }

        public void OutputResult(object[] parameters, out string sErrMsg)
        {
            sErrMsg = "";

            ProcessResult(parameters);
        }

        public void ProcessResult(object data)
        {

        }
    }
}
