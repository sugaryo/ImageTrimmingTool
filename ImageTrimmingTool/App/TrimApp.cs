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


        #region ctor
        public TrimApp(string[] args) : base( args )
        {
            this._wizzard = new InputWizzard( new[] { "exit" } );

#warning ストラテジの必要性がなくなったのでアルゴリズム実装を見直し。
            this._strategy = new TrimSubDirectory();
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
                    new DescriptionInfo("パラメータJSON", "トリミングの詳細を指定します。"),
#warning あとでJSONの説明を追記。
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


            // ■トリミング領域入力
            // ※ コンソールからの自由入力は一旦機能削除。
            TrimmingSetting area = null;
            if (  _wizzard.TryInputOrPath( new[] { 
                        "トリミング領域を定義したJSONファイルパスを指定してください。",
                        "若しくはJSON文字列をそのまま入力",
                        @"( input ""exit"" to exit )" 
                    },
                    ( input ) =>
                    {
                        System.Diagnostics.Debug.WriteLine( $"[debug] JSON入力 : {input}" );
                        area = TrimmingSetting.Parse( input );
                    },
                    ( path ) =>
                    {
                        System.Diagnostics.Debug.WriteLine( $"[debug] パス入力 : {path}" );
                        string json = File.ReadAllText( path );
                        area = TrimmingSetting.Parse( json );
                    } 
                    ) )
            {
                this.Trim( area, files );
            }
        }
        #endregion

        private List<FileInfo> AnalizeInputFiles(Arguments arguments)
        {
            List<FileInfo> files;


            // ■まず普通に渡された args から JPEG/PNGファイルパス をチェック。
            files = arguments.AsParameters()
                .Where( x => File.Exists( x ) )
                .Select( x => new FileInfo( x ) )
                .Where( x => x.isSupportedImageFile() )
                .ToList();
            if ( 0 < files.Count ) return files;


            // ■JPEG/PNGファイルが渡されてなかったら、次点で ディレクトリパス をチェック。（複数ある場合はFirstを優先して採用）
            {
                var dir = arguments.AsParameters()
                    .Where( x => Directory.Exists( x ) )
                    .Select( x => new DirectoryInfo( x ) )
                    .FirstOrDefault();
                if ( null != dir )
                {
                    files = dir.GetFiles()
                        .AsEnumerable()
                        .Where( x => x.isSupportedImageFile() )
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
                        // ウィザードでフォルダが指定された場合、JPEG/PNGファイルのリストを取得。
                        files = folder.GetFiles()
                            .AsEnumerable()
                            .Where( x => x.isSupportedImageFile() )
                            .ToList();
                    } ) )
            {
                if ( 0 < files.Count ) return files;
            }


            // ■いずれの方法でも入力ファイルが特定されなかったら空のリストを返す（処理終了）
            System.Diagnostics.Debug.WriteLine( "入力ファイルなし。" );
            return new List<FileInfo>();
        }

        private void Trim(TrimmingSetting area, IEnumerable<FileInfo> files)
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
