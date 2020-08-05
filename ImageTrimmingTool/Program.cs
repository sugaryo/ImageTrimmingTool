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

                Console.WriteLine("input left margin of trim area.");
                Console.Write("dx:");
                int dx = Console.ReadLine().asInt();
                if (dx <= 0)
                {
                    Console.WriteLine("exit by parameter[dx:{0}]", dx);
                    Console.WriteLine("press any key to exit.");
                    Console.ReadKey(true);
                    return;
                }

                Console.WriteLine("input width of trim area.");
                Console.Write("w:");
                int w = Console.ReadLine().asInt();
                if (w <= 0)
                {
                    Console.WriteLine("exit by parameter[w:{0}]", w);
                    Console.WriteLine("press any key to exit.");
                    Console.ReadKey(true);
                    return;
                }

                Trimming(dx, w, files);

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
                Console.WriteLine("press any key to exit.");
                Console.ReadKey(true);
            }
        }

        private static void Trimming(int dx, int w, IEnumerable<FileInfo> files)
        {
            // jpeg エンコーダの取得
            var encoder = GetEncoder(ImageFormat.Jpeg);

            // jpeg エンコードパラメータの設定
            long quality = 90;
            var parameters = new EncoderParameters(1);
            parameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);



#warning いずれパラメータ化。
            // 切り出し設定ベタ書き。

            foreach (var file in files)
            {
                string origin = file.FullName;
                string trimed = origin + ".trim";

                // トリミング。
                #region トリミング処理
                using (Bitmap src = new Bitmap(file.FullName))
                {
                    int h = src.Height;

                    using (Bitmap dst = new Bitmap(w, h))
                    {
                        using (Graphics g = Graphics.FromImage(dst))
                        {
                            g.DrawImage(src, -dx, 0);
                        }

                        dst.Save(trimed, encoder, parameters);
                        Console.WriteLine("trimed: " + trimed);
                    }
                }
                #endregion


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
