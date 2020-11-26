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

        public FileInfo Trim(FileInfo origin, TrimmingSetting setting)
        {
            FileInfo trimed = this.Before( origin );


            #region イメージのトリミング処理 { origin -> trimed }
            using ( Bitmap src = new Bitmap( origin.FullName ) )
            {
                TrimmingSetting.DrawSetting draw = setting.Compile( src.Size );

                using ( Bitmap dst = new Bitmap( draw.size.Width, draw.size.Height ) )
                {
                    using ( Graphics g = Graphics.FromImage( dst ) )
                    {
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;


                        g.DrawImage( src
                            , 0
                            , 0
                            , new Rectangle( draw.x, draw.y, draw.w, draw.h ) 
                            , GraphicsUnit.Pixel
                        );


                        if ( 0 != draw.border.width )
                        {
                            // 綺麗に且つ正確に指定幅のボーダーラインを引くために、１ドットの枠をｎ回描画する事にした。
                            using ( var p = new Pen( draw.border.color, 1 ) )
                            {
                                int w = dst.Width - 1;  // draw.size.Width
                                int h = dst.Height - 1; // draw.size.Height

                                for ( int i = 0; i < draw.border.width; i++ )
                                {
                                    g.DrawRectangle( p, 0 + i, 0 + i, w - i - i, h - i - i );
                                }
                            }
                        }
                    }

                    // dst.Save( trimed.FullName, _encoder, _parameters );
                    // .NET のJPEG Encoderを通すとどうしても画像がボヤける（品質１００でも無理だった）ので、取り敢えず素直にpngで出力するようにしておく。
                    dst.Save( trimed.FullName + ".png", ImageFormat.Png );
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
