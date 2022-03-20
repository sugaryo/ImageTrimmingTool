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
        private const long DEFAULT_JPG_QUALITY = 100;

        private readonly InputWizzard _wizzard;

#warning ストラテジの必要性がなくなったのでアルゴリズム実装を見直し。
        private readonly BaseTrimFileStrategy _strategy;

        private readonly TrimConfig _config;

        private readonly TabCompletion _tab;

        #region ctor
        public TrimApp(string[] args) : base( args )
        {
            this._wizzard = new InputWizzard( new[] { "exit" } );


            this._strategy = new TrimSubDirectory();


            #region 定義済 ParameterJSON の読み込み
            {
                // 定義済みパラメータの事前読み込み
                System.Diagnostics.Debug.WriteLine( Trimming.Default.TrimConfigPath );
                this._config = new TrimConfig();
                int n = this._config.Load( Trimming.Default.TrimConfigPath );
            
                // 定義済みパラメータの事前読み込み結果表示
                System.Diagnostics.Debug.WriteLine( $"defined config parmeters ({n})." );
                Console.WriteLine( $"defined config parmeters ({n})." );
                this._config.Names.ForEach( name => Console.WriteLine( $"  - {name}" ) );
            }
            #endregion

            #region TabCompletion の初期化
            {
                // Tab 補完入力するデータソースを構築。
                var datasource = this._config.Names
                        // 定義済み TrimParameterJSON の入力補完指定。
                        .Select( x => "config:" + x )
                        // オプション機能も入力補完候補に追加。
                        .Concat( new[] { "--jpg" } );

                // Tab 補完入力機能の初期化。
                this._tab = new TabCompletion( datasource );
                this._tab.Indent = false;
            }
            #endregion
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

        #region AnalizeInputFiles
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
            Option option = null;
            if ( _wizzard.TryInputOrPath(
                    new[] {
                            "ファイルが渡されなかったのでフォルダを指定スルノダ。",
                            @"( input ""exit"" to exit )",
                    },
                    ( input ) =>
                    {
                        option = new Option( input.ToLower() );
                    },
                    (_) => { },
                    ( folder ) =>
                    {
                        // ウィザードでフォルダが指定された場合、JPEG/PNGファイルのリストを取得。
                        files = folder.GetFiles()
                            .AsEnumerable()
                            .Where( x => x.isSupportedImageFile() )
                            .ToList();
                    } ) )
            {
                // フォルダを指定されて JPEG/PNG ファイルが有ればそれを返す。
                if ( 0 < files.Count ) return files;
            }
            // ■いずれの方法でも入力ファイルが特定されなかったら空のリストを返す（処理終了）
            System.Diagnostics.Debug.WriteLine( "入力ファイルなし。" );
            files = new List<FileInfo>();

            if ( null != option 
                && ( option.Has( "test" ) || option.Has( "help" ) ) )
            {
                if ( option.Has( "fuzzy-json" ) || option.Has( "jzon" ) )
                {
                    this.JZON();
                }
            }

            return files;
        }
        #endregion

        #region Execute / Callback

        private void Execute(List<FileInfo> files)
        {
            if ( _wizzard.TryInputOrPath(
                    new[] {
                            $"■変換ファイル - {files.Count}■",
                            "【基本機能】",
                            "    1. TrimParameterJSON を定義した .json ファイルパスを入力。",
                            "    2. `config:{name}` で 定義済みTrimParameterJSON名 を入力。",
                            "    3. 若しくは TrimParameterJSON文字列 をそのまま入力。",
                            "【その他のオプション機能】",
                            "    - `--jpeg(--<quality:int>)` to convert JPEG format." ,
                            "    - `exit` to exit.",
                    }
                    // テキスト入力
                    , ( input ) => { this.ExecuteInputCallback( files, input ); }
                    // パス入力
                    , ( path ) => { this.ExecutePathCallback( files, path ); }
                    // [TAB] 入力補完
                    , this._tab
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
                        
                        this.Jpeg( files, q );
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine( $"[debug] ★ JPEG 変換モード" );
                        
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

            #region InputConfig
            void InputConfig(string name)
            {
                var parameter = this._config[name];
                if ( null != parameter )
                {
                    System.Diagnostics.Debug.WriteLine( $"[debug] ★ Config入力 : {name}" );
                    Console.WriteLine( $"■Trim処理 config[{name}]■" );

                    this.Trim( files, parameter );
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine( "undefined config name." );
                }
            }
            #endregion

            #region InputJSON
            void InputJSON()
            {
                System.Diagnostics.Debug.WriteLine( $"[debug] ★ JSON入力 : {input}" );
                Console.WriteLine( "■Trim処理■" );

                string json = input.fuzzy();
                Console.WriteLine( json );

                var parameter = TrimParameterJSON.Parse( json );
                this.Trim( files, parameter );
            }
            #endregion


            // ■オプション入力
            if ( input.startsWith( "--" ) )
            {
                InputOption();
            }
            // ■config入力
            else if ( input.startsWith( "config:" ) )
            {
                string name = input.Substring( "config:".Length );
                InputConfig( name );
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

            string json = File.ReadAllText( path ).fuzzy();
            var parameter = TrimParameterJSON.Parse( json );
            this.Trim( files, parameter );
        }
        
        #endregion
        
        #endregion

        #region Trim
        // メインのトリミング処理、詳細はTrimParameterJSONの指定に基づく。
        private void Trim(IEnumerable<FileInfo> files, TrimParameterJSON parameter)
        {
            foreach ( var file in files )
            {
                var trimed = _strategy.Trim( file, parameter );


                Console.WriteLine( "trim." );
                Console.WriteLine( $"  - [origin] {file.FullName}" );
                Console.WriteLine( $"  - [  trim] {trimed.FullName}" );
            }
        }
        #endregion

        #region Jpeg
        // このツールに乗せるより別ツール作った方が良いと思うが、JPEGコンバータを追加。
        private void Jpeg(IEnumerable<FileInfo> files, long quality = DEFAULT_JPG_QUALITY)
        {
            Console.WriteLine( $"■JPEG変換-[Q:{quality}]■" );
            foreach ( var file in files )
            {
                var jpg = file.cnvjpg( quality );


                Console.WriteLine( "convert jpeg format." );
                Console.WriteLine( $"  - [origin] {file.FullName}" );
                Console.WriteLine( $"  - [  jpeg] {jpg.FullName}" );
            }
        }
        #endregion

        #region JZON
        // fuzzy-json のテスト兼サンプル表示
        private void JZON()
        {
#warning 出力をもう少し工夫したいが一旦これで。
            string[] inputs = {
                "padding:123",
                @"""hoge"":""moge""",
                @"""num"":999",
                "xxx : 1234567890",
                "yyy : 1234567890,",
                "zzz : 1234567890;",
                "top:11;right:22;bottom:33;left:44",
                "top:55,right:66,bottom:77,left:88",
                "X : 123, Y : 456, Z : 789",
                "A : 111; B : 222; C : 333",
                "D : 444, E : 555, F : 666,",
                "G : 777; H : 888; I : 999;",
                "  key    :    value      ;  ",
                "  key:        value      ;  ",
                "  key        :value      ;  ",
                "key=value",
                "key=value,",
                "key=value;",
            };

            Console.WriteLine( "FUZZY JSON の入出力パターン" );
            int n = 0;
            foreach ( var json in inputs )
            {
                Console.WriteLine( $"- sample[{++n}]" );
                Console.WriteLine( $"    $ {json}" );
                Console.WriteLine( $"    > {json.fuzzy()}" );
            }
        }
        #endregion
    }
}
