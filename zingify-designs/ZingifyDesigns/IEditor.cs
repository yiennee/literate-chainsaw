using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;

namespace ZingifyDesigns
{
    public interface IEditor
    {
        void Display(CogDisplay display);

        //XElement GetEditParameters();

        void RefreshDisplay();

        string GetResultXmlString();

        ICogTool GetTool();
    }
}
