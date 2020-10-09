using ArgsAnalyzer;
using CliToolTemplate;
using CliToolTemplate.Description;
using CliToolTemplate.Utility;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using ImageTrimmingTool.App.Strategy;

using Encoder = System.Drawing.Imaging.Encoder;

namespace ImageTrimmingTool.App
{
    public class TrimApp : ConsoleAppBase
    {
        private readonly BaseTrimFileStrategy _strategy;

        public TrimApp(string[] args) : base( args )
        {
            var mode = Trimming.Default.Mode;
            switch ( mode )
            {
                case Trimming.TrimMode.SubDirectory:
                    _strategy = new TrimSubDirectory();
                    Console.WriteLine( "--------------------------------" );
                    Console.WriteLine( "tool mode [SUB-DIRECTORY]" );
                    Console.WriteLine( "--------------------------------" );
                    break;
                case Trimming.TrimMode.SwapFile:
                    _strategy = new TrimSwapFile();
                    Console.WriteLine( "--------------------------------" );
                    Console.WriteLine( "tool mode [SWAP-FILE]" );
                    Console.WriteLine( "--------------------------------" );
                    break;
                default:
                    break;
            }
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
                    Trim( json, files );
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
                    Trim( json, files );
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

            // json 設定パラメータを渡されず、キャンセルもされなかった場合。
            // コンソール入力された値でトリミング。
            TrimmingArea area = new TrimmingArea() { DX = dx, W = w };
            Trim( area, files );
        }

        private void Trim(string json, IEnumerable<FileInfo> files)
        {
            var area = JsonConvert.DeserializeObject<TrimmingArea>( json );
            this.Trim( area, files );
        }

        private void Trim(TrimmingArea area, IEnumerable<FileInfo> files)
        {
            foreach ( var file in files )
            {
                _strategy.Trim( file, area );
            }
        }
    }
}
