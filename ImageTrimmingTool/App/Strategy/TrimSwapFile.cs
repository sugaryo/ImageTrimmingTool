using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageTrimmingTool.App.Strategy
{
    public class TrimSwapFile : BaseTrimFileStrategy
    {
        protected override FileInfo Before(FileInfo origin)
        {
            // とりあえず .trim ファイルを作る。
            return new FileInfo( origin.FullName + ".trim" );
        }

        protected override FileInfo After(FileInfo origin, FileInfo trimed)
        {
            // ファイルをスワップする。
            File.Delete( origin.FullName );
            File.Move( trimed.FullName, origin.FullName );

            return origin;
        }
    }
}
