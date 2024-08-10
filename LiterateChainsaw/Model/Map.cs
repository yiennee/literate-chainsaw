using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiterateChainsaw.Model
{
    public interface IMap
    {
        List<Unit> Load(out string errMsg);// int CanvasSizeX, int CanvasSizeY);//, out List<Unit> UnitList);
        bool Save(List<Unit> UnitList, string filePath, string fileName, out string errMsg);
    }

    public enum MapType
    {
        XMLMap_SemiG85,
        Klarf1_2
    }
}
