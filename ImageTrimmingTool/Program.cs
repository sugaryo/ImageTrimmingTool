using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Imaging;

using System.IO;
using Encoder = System.Drawing.Imaging.Encoder;



namespace ImageTrimmingTool
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var files = args
                    .Where( x => File.Exists(x) )
                    .Where( x => Path.GetExtension(x).ToLower().any(".jpg",".jpeg") )
                    .Select( x => new FileInfo(x) );

                Trimming(files);

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("press any key to exit.");
                Console.ReadKey(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void Trimming(IEnumerable<FileInfo> files)
        {
            // jpeg エンコーダの取得
            var encoder = GetEncoder(ImageFormat.Jpeg);

            // jpeg エンコードパラメータの設定
            long quality = 90;
            var parameters = new EncoderParameters(1);
            parameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);



#warning いずれパラメータ化。
            // 切り出し設定ベタ書き。
            int x = -18;
            int y = 0;
            int w = 960;

            foreach (var file in files)
            {
                string origin = file.FullName;
                string trimed = origin + ".trim";

                // トリミング。
                using (Bitmap src = new Bitmap(file.FullName))
                {
                    int h = src.Height;

                    using (Bitmap dst = new Bitmap(w, h))
                    {
                        using (Graphics g = Graphics.FromImage(dst))
                        {
                            g.DrawImage(src, x, y);
                        }

                        dst.Save(trimed, encoder, parameters);
                        Console.WriteLine("trimed: " + trimed);
                    }
                }

                // ファイルをスワップ。
                File.Delete(origin);
                File.Move(trimed, origin);
            }

        }

        /// <seealso cref="https://docs.microsoft.com/ja-jp/dotnet/framework/winforms/advanced/how-to-set-jpeg-compression-level"/>
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
