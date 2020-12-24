using ArgsAnalyzer;
using CliToolTemplate;
using CliToolTemplate.Description;
using CliToolTemplate.Utility;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageTrimmingTool.App.Strategy;
using ImageTrimmingTool.App.Utility;


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
                    (folder) =>
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
        protected override void Execute(Arguments arguments)
        {
            // ■入力ファイルリストを取得
            List<FileInfo> files = this.AnalizeInputFiles( arguments );

            // 有効な入力ファイルが得られなかったらヘルプ表示して終わり。
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
            }
            // 有効な入力ファイルが渡されたらトリミング条件を受け付けて処理。
            else
            {
                this.Execute( files );
            }
        }
        private void Execute(List<FileInfo> files)
        {
            if ( _wizzard.TryInputOrPath(
                    new[] {
                            $"■変換ファイル - {files.Count}■",
                            "■詳細入力■",
                            "- TrimParameterJSON を定義したファイルパスを入力。",
                            "- 若しくは TrimParameterJSON文字列 をそのまま入力。",
                            "- その他のオプション",
                            "    - input `--jpeg(--<quality:int>)` to convert JPEG format." ,
                            "    - input `exit` to exit.",
                    }
                    // テキスト入力
                    , ( input ) => { this.ExecuteInputCallback( files, input ); }
                    // パス入力
                    , ( path ) => { this.ExecutePathCallback( files, path ); }
                ) )
            {
                // 後処理は特になし。
            }
            else
            {
                // 入力のキャンセル
                Console.WriteLine();
                Console.WriteLine( "【！】処理中断：キャンセルされました【！】" );
            }
        }
        private void ExecuteInputCallback(IEnumerable<FileInfo> files, string input)
        {
            #region InputOption
            void InputOption()
            {
                var option = new Option( input.ToLower() );

                // png -> jpg 変換機能
                if ( option.Has( "jpeg", "jpg" ) )
                {
                    string value;
                    if ( option.Match( "[0-9]+", out value ) )
                    {
                        int q = value.asInt();

                        System.Diagnostics.Debug.WriteLine( $"[debug] ★ JPEG 変換モード[Q={q}]" );
                        Console.WriteLine( $"■JPEG変換[Q={q}]■" );

                        this.Jpeg( files, q );
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine( $"[debug] ★ JPEG 変換モード" );
                        Console.WriteLine( "■JPEG変換■" );

                        this.Jpeg( files );
                    }
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine( "unknown option." );
                }
            };
            #endregion

            #region InputJSON
            void InputJSON()
            {
                System.Diagnostics.Debug.WriteLine( $"[debug] ★ JSON入力 : {input}" );
                Console.WriteLine( "■Trim処理■" );

                var area = TrimParameterJSON.Parse( input.fuzzy() );
                this.Trim( files, area );
            }
            #endregion

            // ■オプション入力
            if ( input.startsWith( "--" ) )
            {
                InputOption();
            }
            // ■JSON入力
            else
            {
                InputJSON();
            }
        }
        private void ExecutePathCallback(IEnumerable<FileInfo> files, string path)
        {
            System.Diagnostics.Debug.WriteLine( $"[debug] ★ パス入力 : {path}" );
            Console.WriteLine( "■Trim処理■" );

            string json = File.ReadAllText( path );
            var area = TrimParameterJSON.Parse( json.fuzzy() );
            this.Trim( files, area );
        }
        #endregion

        #region Trim
        // メインのトリミング処理、詳細はTrimParameterJSONの指定に基づく。
        private void Trim(IEnumerable<FileInfo> files, TrimParameterJSON area)
        {
            foreach ( var file in files )
            {
                var trimed = _strategy.Trim( file, area );


                Console.WriteLine( "trim." );
                Console.WriteLine( $"  - [origin] {file.FullName}" );
                Console.WriteLine( $"  - [  trim] {trimed.FullName}" );
            }
        }
        #endregion

        #region Jpeg
        // このツールに乗せるより別ツール作った方が良いと思うが、JPEGコンバータを追加。
        private void Jpeg(IEnumerable<FileInfo> files, long quality = 100)
        {
            foreach ( var file in files )
            {
                var jpg = file.cnvjpg( quality );


                Console.WriteLine( "convert jpeg format." );
                Console.WriteLine( $"  - [origin] {file.FullName}" );
                Console.WriteLine( $"  - [  jpeg] {jpg.FullName}" );
            }
        }
        #endregion
    }
}
