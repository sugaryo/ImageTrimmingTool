using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageTrimmingTool.App.Strategy
{
    public class TrimSubDirectory : BaseTrimFileStrategy
    {
        protected override FileInfo Before(FileInfo origin)
        {
            var subdir = origin.Directory.CreateSubdirectory( Trimming.Default.SubDirectoryName );

            string filename = Path.GetFileNameWithoutExtension( origin.Name );
            string filepath = Path.Combine( subdir.FullName, filename + ".png" );

            return new FileInfo( filepath );
        }

        protected override FileInfo After(FileInfo origin, FileInfo trimed)
        {
            // 特に後処理なし。
            return trimed;
        }
    }
}
