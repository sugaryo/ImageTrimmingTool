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
            public Size size;
            public int x;
            public int y;
            public int w;
            public int h;

            public BorderSetting border;
        }

        public int? Width { get; set; }
        public int? Height { get; set; }

        public int? Left { get; set; }
        public int? Right { get; set; }
        public int? Top { get; set; }
        public int? Bottom { get; set; }

        public string Padding { get; set; }

        [JsonProperty( PropertyName = "padding-top" )]
        public int? PaddingTop { get; set; }
        [JsonProperty( PropertyName = "padding-right" )]
        public int? PaddingRight { get; set; }
        [JsonProperty( PropertyName = "padding-bottom" )]
        public int? PaddingBottom { get; set; }
        [JsonProperty( PropertyName = "padding-left" )]
        public int? PaddingLeft { get; set; }


        public int? Border { get; set; }
        [JsonProperty( PropertyName = "border-color" )]
        public string BorderColor { get; set; } = "Black";


        public DrawSetting Compile( Size origin )
        {
            DrawSetting draw = new DrawSetting();

            #region 矩形トリミング用の領域設定
            // { top, right, bottom, left } を取得。
            // 優先順位：無印.xxx > padding-xxx > padding[]
            int[] padding = this.CompilePadding();
            int top    = this.Top    ?? this.PaddingTop    ?? padding[0];
            int right  = this.Right  ?? this.PaddingRight  ?? padding[1];
            int bottom = this.Bottom ?? this.PaddingBottom ?? padding[2];
            int left   = this.Left   ?? this.PaddingLeft   ?? padding[3];

            draw.x = left;
            draw.y = top;
            draw.w = origin.Width - ( right + left );
            draw.h = origin.Height - ( top + bottom );

            // WidthHeight が指定されていない場合は自動計算。
            int width  = this.Width  ?? draw.w;
            int height = this.Height ?? draw.h;

            draw.size = new Size( width, height );
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
