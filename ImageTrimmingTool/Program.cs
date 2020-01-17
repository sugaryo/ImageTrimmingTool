using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Imaging;

using System.IO;

namespace ImageTrimmingTool
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var files = args
                    .Where(x => File.Exists(x))
                    .Select(x => new FileInfo(x));

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
            int x = -18;
            int y = 0;
            int w = 960;

            foreach (var file in files)
            {
                using (Bitmap src = new Bitmap(file.FullName))
                {
                    int h = src.Height;

                    using (Bitmap dst = new Bitmap(w, h))
                    {
                        // 座標を変えてイメージを描画＝トリミング
                        using (Graphics g = Graphics.FromImage(dst))
                        {
                            g.DrawImage(src, x, y);
                        }

                        // トリムしたビットマップを保存。
                        string save = file.FullName + ".trimed.png";
                        dst.Save(save, ImageFormat.Png);

                        Console.WriteLine("trimed: " + save);
                    }
                }

                // 元のファイルを削除。
                File.Delete(file.FullName);
            }

        }
    }
}
