using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageTrimmingTool.App
{
    public static class extensions
    {
        public static bool any(this string str, params string[] strings) {

            foreach (var x in strings)
            {
                if (str == x) return true;
            }

            return false;
        }

        public static bool isSupportedImageFile( this FileInfo file )
        {
            return Path.GetExtension( file.Name ).ToLower().any( ".jpg", ".jpeg", ".png", ".bmp" );
        }

        public static int asInt(this string str) {
            return int.Parse(str);
        }



        public static string directory(this FileInfo file)
        {
            return file.Directory.FullName;
        }
        public static string filename(this FileInfo file, string extension = null)
        {
            return Path.GetFileNameWithoutExtension( file.FullName ) + ( extension ?? "" );
        }
    }
}
