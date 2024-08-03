using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace ImageTrimmingTool.App
{
    public class TrimParameterJSON
    {
        public struct BorderSetting
        {
            public Color color;
            public int width;
        }
        public struct DrawSetting
        {
            public int x;
            public int y;
            public int w;
            public int h;

            public BorderSetting border;
        }


        #region Trimming設定のプロパティ群
        // 個別プロパティ：
        public int? Width { get; set; }
        public int? Height { get; set; }

        public int? Left { get; set; }
        public int? Right { get; set; }
        public int? Top { get; set; }
        public int? Bottom { get; set; }

        // 個別プロパティのショートカット指定：
        public int? W { get; set; }
        public int? H { get; set; }

        public int? L { get; set; }
        public int? R { get; set; }
        public int? T { get; set; }
        public int? B { get; set; }


        /// <summary>
        /// css padding 風の指定。
        /// </summary>
        /// <remarks>
        /// padding の指定が一番弱く、次に padding-xxx が強い。
        /// 更に個別プロパティでの指定が最優先される。
        /// </remarks>
        public string Padding { get; set; }

        // padding 風の個別プロパティ：
        [JsonProperty( PropertyName = "padding-top" )]
        public int? PaddingTop { get; set; }
        [JsonProperty( PropertyName = "padding-right" )]
        public int? PaddingRight { get; set; }
        [JsonProperty( PropertyName = "padding-bottom" )]
        public int? PaddingBottom { get; set; }
        [JsonProperty( PropertyName = "padding-left" )]
        public int? PaddingLeft { get; set; }
        #endregion

        #region ボーダーライン用のプロパティ群
        public int? Border { get; set; }
        [JsonProperty( PropertyName = "border-color" )]
        public string BorderColor { get; set; } = "Black";
        #endregion


        public DrawSetting Compile( Size origin )
        {

            #region 矩形トリミング用の領域設定
            // padding の設定を解析して int[] に変換する。
            int[] padding = this.CompilePadding();
            // 以下の優先順位ルールに従って draw{x,y,w,h} を設定する。
            // 優先順位：無印.xxx > ショートカット.xxx > padding-xxx > padding[]
            int top    = this.Top    ?? this.T ?? this.PaddingTop    ?? padding[0];
            int right  = this.Right  ?? this.R ?? this.PaddingRight  ?? padding[1];
            int bottom = this.Bottom ?? this.B ?? this.PaddingBottom ?? padding[2];
            int left   = this.Left   ?? this.L ?? this.PaddingLeft   ?? padding[3];

            DrawSetting draw = new DrawSetting();
            draw.x = left;
            draw.y = top;
            draw.w = this.Width  ?? this.W ?? ( origin.Width - right - left );
            draw.h = this.Height ?? this.H ?? ( origin.Height - top - bottom );
            #endregion


            #region 枠線オプション用の設定
            if ( this.Border.HasValue && 0 != this.Border.Value )
            {
                draw.border.color = Color.FromName( this.BorderColor );
                draw.border.width = this.Border.Value;
            }
            else
            {
                draw.border.color = Color.Transparent;
                draw.border.width = 0;
            }
            #endregion


            return draw;
        }

        private int[] CompilePadding()
        {
            string padding = this.Padding ?? "0 0 0 0";

            var token = padding
                    .split( " " )
                    .AsEnumerable()
                    .Select( x => x.asInt() )
                    .ToList();

            switch ( token.Count )
            {
                case 1:
                    return new int[] { token[0], token[0], token[0], token[0] };
                case 2:
                    return new int[] { token[0], token[1], token[0], token[1] };
                case 3:
                    return new int[] { token[0], token[1], token[2], token[1] }; // ここちょっと自信ない。
                case 4:
                    return token.ToArray();
                default:
                    return token.Take( 4 ).ToArray();
            }
        }
        


        public static TrimParameterJSON Parse(string json)
        {
            System.Diagnostics.Debug.WriteLine( json );

            var obj = JsonConvert.DeserializeObject<TrimParameterJSON>( json );
            return obj;
        }

    }
}
