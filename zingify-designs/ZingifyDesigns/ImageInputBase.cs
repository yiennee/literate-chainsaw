using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZingifyDesigns
{
    public abstract class ImageInputBase
    {
        Action<FileInfo> processImage;
        Action<string, string> processErrorPath;

        public void SetProcessAction(Action<FileInfo> process)
        {
            processImage = process;
        }

        public void ProcessImage(FileInfo fi)
        {
            processImage(fi);
        }

        public void SetErrorAction(Action<string, string> error)
        {
            processErrorPath = error;
        }

        public void HandleErrorPath(string path, string filter)
        {
            processErrorPath(path, filter);
        }

    }
}
