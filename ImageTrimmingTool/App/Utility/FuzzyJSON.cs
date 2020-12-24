using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageTrimmingTool.App.Utility
{
    public static class FuzzyJSON
    {
        public static string fuzzy(this string json)
        {
            string temp = json.Trim();

            // { } 形式で入力されてたら手を付けずに通す。
            if ( temp.StartsWith( "{" ) && temp.EndsWith( "}" ) ) return json;


            StringSplitOptions rm = StringSplitOptions.RemoveEmptyEntries;


            // 中途半端な括弧があったら除去しておく。
            temp = temp.TrimStart( '{' ).TrimEnd( '}' ).Trim();

            // カンマ若しくはセミコロンを区切りとして Key:Value トークンを分割。
            var token = temp.split( rm, ",", ";" );


            var sb = new StringBuilder();

            sb.Append( "{" );
            #region json {  } の書き込み
            {
                string comma = "";
                foreach ( var t in token )
                {
                    // Key:Value トークンを分割して補正。
                    var kv = t.split( rm, ":", "=" )
                        .AsEnumerable()
                        .Select( x => x.Replace( "\t", "" ).Trim() )
                        .ToArray();
                    string key = kv[0].TrimStart( '"' ).TrimEnd( '"' ).Trim();
                    string val = kv[1].Trim();

                    // JSON属性値の編集
                    sb.Append( comma );
                    sb.Append( $@"""{key}""" );
                    sb.Append( ":" );
                    sb.Append( val );
                    comma = ",";
                }
            }
            #endregion
            sb.Append( "}" );

            // fuzzy受け付けしたJSON文字列を返す。
            string jzon = sb.ToString();
            return jzon;
        }
    }
}
