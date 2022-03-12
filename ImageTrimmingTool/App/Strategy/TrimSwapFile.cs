using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageTrimmingTool.App.Strategy
{
    [Obsolete("PNG化した事で色々と事情が変わったのでSWAPモード自体を廃止した。", true)]
    public class TrimSwapFile : BaseTrimFileStrategy
    {
        protected override FileInfo Before(FileInfo origin)
        {
            // とりあえず .trim ファイルを作る。
            return new FileInfo( origin.FullName + ".trim" );
        }

        protected override FileInfo After(FileInfo origin, FileInfo trimed)
        {
#warning こうなってくるとSWAPモード必要ない説。
            string filename = Path.GetFileNameWithoutExtension( origin.Name );
            string filepath = Path.Combine( origin.Directory.FullName, filename + ".png" );

            // ファイルをスワップする。
            File.Delete( origin.FullName );
            File.Move( trimed.FullName, filepath );

            return origin;
        }
    }
}
