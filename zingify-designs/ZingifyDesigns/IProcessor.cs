using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZingifyDesigns
{
    public interface IProcessor
    {
        void CreateLibrary();
        void OpenLibrary();

        //void Initialize(ISetting setting);
        //void SelectTemplate(FileInfo file);
        //void LoadTemplate(FileInfo file);
        //void StartProcess(IInput input);
    }
}
