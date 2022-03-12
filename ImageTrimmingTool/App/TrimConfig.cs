using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ImageTrimmingTool.App.Utility;

namespace ImageTrimmingTool.App
{
    public class TrimConfig
    {
        private readonly Dictionary<string, TrimParameterJSON> _config = new Dictionary<string, TrimParameterJSON>();

        public int Count { get { return this._config.Count; }  }

        public List<string> Names { get { return this._config.Keys.OrderBy( x => x ).ToList(); }  }

        #region Load
        public int Load(string path)
        {
            if ( !File.Exists( path ) ) return 0;

            // Map形式でJSONパースして指定されたJSON定義を展開しておく。
            var map = this.Read( path );
            foreach ( var itor in map )
            {
                string name = itor.Key;    // パラメータファイルの識別名
                string text = itor.Value;  // パラメータファイルのパス

                // 指定されたパラメータファイルがあればJSON定義を読み込む。
                if ( File.Exists( text ) )
                {
                    string json = File.ReadAllText( text ).fuzzy();
                    var parameter = TrimParameterJSON.Parse( json );

                    this.Add( name, parameter );
                }
                // ファイルが存在しなかった場合 JZON リテラルとしてパースしてみる。
                else
                {
                    string json = text.fuzzy();
                    var parameter = TrimParameterJSON.Parse( json );

                    this.Add( name, parameter );
                }
            }

            return this._config.Count;
        }
        private Dictionary<string, string> Read(string path)
        {
            string json = File.ReadAllText( path );
            return JsonConvert.DeserializeObject<Dictionary<string, string>>( json );
        }
        #endregion


        #region Add
        public void Add(string name, TrimParameterJSON parameter)
        {
            // Java の Map ふうに追加。
            if ( this._config.ContainsKey( name ) )
            {
                this._config.Add( name, parameter );
            }
            else
            {
                this._config[name] = parameter;
            }
        }
        #endregion


        #region Clear
        public void Clear()
        {
            this._config.Clear();
        }
        #endregion


        #region indexer[]
        public TrimParameterJSON this[string name]
        {
            get { return this._config.ContainsKey(name) ? this._config[name] : null; }
        }
        #endregion
    }
}
