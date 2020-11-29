using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Encoder = System.Drawing.Imaging.Encoder;

namespace ImageTrimmingTool.App.Utility
{
    public static class JPEG
    {

        public static FileInfo convert(this FileInfo file, long quality = 100)
        {
            string path = Path.Combine( file.directory(), file.filename( ".jpg" ) );

            return file.convert( path, quality );
        }
        public static FileInfo convert(this FileInfo file, string path, long quality = 100)
        {
            using ( var bmp = Bitmap.FromFile( file.FullName ) )
            {
                bmp.save( path, quality );
            }
            return new FileInfo( path );
        }

        public static void save(this Image img, string path, long quality = 100)
        {
            var encoder = GetEncoder( ImageFormat.Jpeg );

            var parameters = new EncoderParameters( 1 );
            parameters.Param[0] = new EncoderParameter( Encoder.Quality, quality );


            img.Save( path, encoder, parameters );
        }



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
