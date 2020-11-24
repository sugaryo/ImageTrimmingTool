using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

using Encoder = System.Drawing.Imaging.Encoder;

namespace ImageTrimmingTool.App.Strategy
{
    public abstract class BaseTrimFileStrategy
    {
        private readonly ImageCodecInfo _encoder;
        private readonly EncoderParameters _parameters;

        public BaseTrimFileStrategy()
        {
            #region jpeg圧縮設定
            // jpeg エンコーダの取得
            _encoder = GetEncoder( ImageFormat.Jpeg );

            // jpeg エンコードパラメータの設定
            long quality = Trimming.Default.Quality;
            _parameters = new EncoderParameters( 1 );
            _parameters.Param[0] = new EncoderParameter( Encoder.Quality, quality );
            #endregion
        }

        public FileInfo Trim(FileInfo origin, TrimmingArea area)
        {
            FileInfo trimed = this.Before( origin );


#warning ここの処理を更に拡張できるようにAlgorithmストラテジで置き換えたい。
            #region イメージのトリミング処理 { origin -> trimed }
            using ( Bitmap src = new Bitmap( origin.FullName ) )
            {
                int h = src.Height;

                using ( Bitmap dst = new Bitmap( area.Width.Value, h ) )
                {
                    using ( Graphics g = Graphics.FromImage( dst ) )
                    {
                        g.DrawImage( src, -area.Left.Value, 0 );
                    }

                    dst.Save( trimed.FullName, _encoder, _parameters );
                }
            }
            #endregion


            return this.After( origin, trimed );
        }

        /// <summary>
        /// 前処理：トリミング処理の出力ファイルを用意する前処理を実装する。
        /// </summary>
        /// <param name="origin">入力ファイル</param>
        /// <returns>出力ファイル</returns>
        protected abstract FileInfo Before(FileInfo origin);
        
        /// <summary>
        /// 後処理：入力ファイル及び出力ファイルを用いて必要な後処理を実装する。
        /// </summary>
        /// <param name="origin">入力ファイル</param>
        /// <param name="trimed">出力ファイル</param>
        /// <returns>最終的な出力ファイル</returns>
        protected abstract FileInfo After(FileInfo origin, FileInfo trimed);


        /// <seealso cref="https://docs.microsoft.com/ja-jp/dotnet/framework/winforms/advanced/how-to-set-jpeg-compression-level"/>
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach ( ImageCodecInfo codec in codecs )
            {
                if ( codec.FormatID == format.Guid )
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
