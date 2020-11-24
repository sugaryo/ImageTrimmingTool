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
        private readonly InputWizzard _wizzard;

        private readonly BaseTrimFileStrategy _strategy;

        private Trimming.TrimMode Mode
        {
            get { return Trimming.Default.Mode; }
        }

        #region ctor
        public TrimApp(string[] args) : base( args )
        {
            this._wizzard = new InputWizzard( new[] { "exit" } );

            switch ( this.Mode )
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
        #endregion

        #region ConsoleAppBase::CreateAppMabual 実装
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
                    new DescriptionInfo("app.config.Mode", "SUB-DIRECTORY / SWAP-FILE"),
                    new DescriptionInfo("  .Mode[SUB-DIRECTORY]", "サブディレクトリを作成し、そこにトリミングしたファイルを保存します。"),
                    new DescriptionInfo("    - SUB-DIRECTORY alt mode", "D / Directory / SubDirectory"),
                    new DescriptionInfo("    - SUB-DIRECTORY alt mode", "C / Copy "),
                    new DescriptionInfo("  .Mode[SWAP-FILE]", "トリミングした一時ファイルを作成し、オリジナルと差し替えます。（疑似上書き）"),
                    new DescriptionInfo("    - SWAP-FILE alt mode", "F / File / SwapFile"),
                    new DescriptionInfo("    - SWAP-FILE alt mode", "O / OverWrite"),
                },
            };

            return manual;
        }
        #endregion
        
        #region ConsoleAppBase::Execute 実装（メインロジック）
        protected override void Execute(Arguments arguments)
        {
            // ■入力ファイルリストを取得
            List<FileInfo> files = this.AnalizeInputFiles( arguments );
            if ( 0 == files.Count )
            {
                Console.WriteLine();
                Console.WriteLine( "【！】処理中断：入力ファイルなし【！】" );
                Console.WriteLine();
                Console.WriteLine( "特にやることも無いのでヘルプを表示します。" );
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                base.ShowHelp();
                return;
            }





            // ■トリミング条件入力
            string input;


            int dx;
            if ( _wizzard.TryInput( new[] {
                    "トリミングする領域の LEFT-MARGIN を入力。",
                    "若しくはトリミング領域を定義したJSONファイルパスを指定。",
                    @"( input ""exit"" or value less than 0, to exit )",
                }, out input ) )
            {
                // JSONファイル指定の場合
                if ( File.Exists( input ) )
                {
                    string json = File.ReadAllText( input );
                    this.Trim( TrimmingArea.Parse( json ), files );
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
            if ( _wizzard.TryInput( new[] {
                    "トリミングする領域の WIDTH を入力。",
                    "若しくはトリミング領域を定義したJSONファイルパスを指定。",
                    @"( input ""exit"" or value less than 0, to exit )",
                }, out input ) )
            {
                // JSONファイル指定の場合
                if ( File.Exists( input ) )
                {
                    string json = File.ReadAllText( input );
                    this.Trim( TrimmingArea.Parse( json ), files );
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




#warning トリミングの領域指定方法を拡充したい。
            // json 設定パラメータを渡されず、キャンセルもされなかった場合。
            // コンソール入力された値でトリミング。
            TrimmingArea area = new TrimmingArea() { DX = dx, W = w };
            this.Trim( area, files );
        }
        #endregion

        private List<FileInfo> AnalizeInputFiles(Arguments arguments)
        {
            List<FileInfo> files;


            // ■まず普通に渡された args から JPEGファイルパス をチェック。
            files = arguments.AsParameters()
                .Where( x => File.Exists( x ) )
                .Where( x => Path.GetExtension( x ).ToLower().any( ".jpg", ".jpeg" ) )
                .Select( x => new FileInfo( x ) )
                .ToList();
            if ( 0 < files.Count ) return files;


            // ■JPEGファイルが渡されてなかったら、次点で ディレクトリパス をチェック。（複数ある場合はFirstを優先して採用）
            { 
                var dir = arguments.AsParameters()
                    .Where( x => Directory.Exists( x ) )
                    .Select( x => new DirectoryInfo( x ) )
                    .FirstOrDefault();
                if ( null != dir )
                {
                    files = dir.GetFiles()
                        .AsEnumerable()
                        .Where( x => Path.GetExtension( x.Name ).ToLower().any( ".jpg", ".jpeg" ) )
                        .ToList();
                    if ( 0 < files.Count ) return files;
                }
            }


#warning このパターンいるかな？？？

            // ■パラメータで入力ファイルが渡されなかった場合、ウィザードでフォルダ指定する。
            if ( _wizzard.TryInputOrPath( 
                    new[] {
                            "ファイルが渡されなかったのでフォルダを指定スルノダ。",
                            @"( input ""exit"" to exit )",
                    },
                    (_) => { }, // action.path     (nop)
                    (_) => { }, // action.FileInfo (nop)
                                // action.DirectoryInfo
                    ( folder ) =>
                    {
                        // ウィザードでフォルダが指定された場合、JPEGファイルのリストを取得。
                        files = folder.GetFiles()
                            .AsEnumerable()
                            .Where( x => Path.GetExtension( x.Name ).ToLower().any( ".jpg", ".jpeg" ) )
                            .ToList();
                    } ) )
            {
                if ( 0 < files.Count ) return files;
            }


            // ■いずれの方法でも入力ファイルが特定されなかったら空のリストを返す（処理終了）
            System.Diagnostics.Debug.WriteLine( "入力ファイルなし。" );
            return new List<FileInfo>();
        }


#warning 実際のトリミング処理、メインロジックを改良したい。

        private void Trim(TrimmingArea area, IEnumerable<FileInfo> files)
        {
            foreach ( var file in files )
            {
                var trimed = _strategy.Trim( file, area );


                Console.WriteLine( "trim." );
                Console.WriteLine( $"  - [origin] {file.FullName}" );
                Console.WriteLine( $"  - [trimed] {trimed.FullName}" );
            }
        }

    }
}
