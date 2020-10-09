using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Imaging;

using System.IO;
using Encoder = System.Drawing.Imaging.Encoder;

using ArgsAnalyzer;
using CliToolTemplate;
using CliToolTemplate.Description;
using CliToolTemplate.Utility;
using Newtonsoft.Json;

namespace ImageTrimmingTool.App
{
    public class TrimApp : ConsoleAppBase
    {
        public TrimApp(string[] args) : base( args )
        {
        }

        protected override AppManual CreateAppManual()
        {
            var manual = new AppManual()
            {
                Summary = new DescriptionInfo(
                    null,
                    @"画像をトリミングするツール" ),
                MainParameter = new DescriptionInfo(
                    null,
                    @"トリミングしたい画像のパス（複数指定可）を渡します。" ),
                Options = new List<DescriptionInfo>()
                {
                    //今の所特にオプション無いんだよね。
                },
            };

            return manual;
        }

        protected override void Execute(Arguments arguments)
        {
            var wizzard = new InputWizzard( new[] { "exit" } );


            // ■入力ファイルリストを取得
            List<FileInfo> files;

            // まず普通に渡されたファイルパスをチェック。
            files = arguments.AsParameters()
                .Where( x => File.Exists( x ) )
                .Where( x => Path.GetExtension( x ).ToLower().any( ".jpg", ".jpeg" ) )
                .Select( x => new FileInfo( x ) )
                .ToList();

            // パラメータにファイルパスが無ければディレクトリパスをチェック。
            if ( 0 == files.Count )
            {
                var dir = arguments.AsParameters()
                    .Where( x => Directory.Exists( x ) )
                    .Select( x => new DirectoryInfo( x ) )
                    .FirstOrDefault();

                if ( null != dir )
                {
                    Console.WriteLine( $"folder:{dir.FullName}" );
                    files = dir.GetFiles()
                        .AsEnumerable()
                        .Where( x => Path.GetExtension( x.Name ).ToLower().any( ".jpg", ".jpeg" ) )
                        .ToList();
                }
            }

            // パラメータになかったらフォルダ入力。
            if ( 0 == files.Count )
            {
                DirectoryInfo dir = null;
                wizzard.TryInputOrPath( new[] {
                        "ファイルが渡されなかったのでフォルダを指定スルノダ。",
                        @"( input ""exit"" to exit )",
                    }
                    , (_) => { }
                    , (_) => { }
                    , (d) => { dir = d; }
                );
                if ( null == dir ) return;

                Console.WriteLine( $"folder:{dir.FullName}" );
                files = dir.GetFiles()
                    .AsEnumerable()
                    .Where( x => Path.GetExtension( x.Name ).ToLower().any( ".jpg", ".jpeg" ) )
                    .ToList();

                // それでも無ければもう終了。
                if ( 0 == files.Count ) return;
            }



            // ■トリミング条件入力
            string input;


            int dx;
            if ( wizzard.TryInput( new[] {
                    "トリミングする領域の LEFT-MARGIN を入力。",
                    "若しくはトリミング領域を定義したJSONファイルパスを指定。",
                    @"( input ""exit"" or value less than 0, to exit )",
                }, out input ) )
            {
                // JSONファイル指定の場合
                if ( File.Exists( input ) )
                {
                    string json = File.ReadAllText( input );
                    Trimming( json, files );
                    return;
                }
                // 数値入力
                else
                {
                    dx = input.asInt();
                    if ( dx < 0 ) return;
                }
            }
            else
            {
                return;
            }

            int w;
            if ( wizzard.TryInput( new[] {
                    "トリミングする領域の WIDTH を入力。",
                    "若しくはトリミング領域を定義したJSONファイルパスを指定。",
                    @"( input ""exit"" or value less than 0, to exit )",
                }, out input ) )
            {
                // JSONファイル指定の場合
                if ( File.Exists( input ) )
                {
                    string json = File.ReadAllText( input );
                    Trimming( json, files );
                    return;
                }
                // 数値入力
                else
                {
                    w = input.asInt();
                    if ( w < 0 ) return;
                }
            }
            else
            {
                return;
            }


            Trimming( dx, w, files );
        }

        public class TrimmingArea
        {
            public int dx { get; set; }
            public int w { get; set; }
        }

        private static void Trimming(string json, IEnumerable<FileInfo> files)
        {
            var area = JsonConvert.DeserializeObject<TrimmingArea>( json );
            Trimming( area.dx, area.w, files );
        }

        private static void Trimming(int dx, int w, IEnumerable<FileInfo> files)
        {
            // jpeg エンコーダの取得
            var encoder = GetEncoder( ImageFormat.Jpeg );

            // jpeg エンコードパラメータの設定
            long quality = 90;
            var parameters = new EncoderParameters( 1 );
            parameters.Param[0] = new EncoderParameter( Encoder.Quality, quality );


#warning トリムファイルをスワップ式にするか /trim サブディレクトリ出力にするかオプション作るか。
            foreach ( var file in files )
            {
                string origin = file.FullName;
                string trimed = origin + ".trim";

                // トリミング。
                #region トリミング処理
                using ( Bitmap src = new Bitmap( file.FullName ) )
                {
                    int h = src.Height;

                    using ( Bitmap dst = new Bitmap( w, h ) )
                    {
                        using ( Graphics g = Graphics.FromImage( dst ) )
                        {
                            g.DrawImage( src, -dx, 0 );
                        }

                        dst.Save( trimed, encoder, parameters );
                        Console.WriteLine( "trimed: " + trimed );
                    }
                }
                #endregion


                // ファイルをスワップ。
                File.Delete( origin );
                File.Move( trimed, origin );
            }

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
