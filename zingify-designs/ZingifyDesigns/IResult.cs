using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ZingifyDesigns
{
    public interface IResult
    {
        void OutputResult(object[] parameters, out string sErrMsg);
    }
}
