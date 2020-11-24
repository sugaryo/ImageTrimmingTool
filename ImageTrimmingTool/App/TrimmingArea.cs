using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace ImageTrimmingTool.App
{
    public class TrimmingArea
    {
        // rect系

        [Obsolete("廃止予定")]
        public int DX { get; set; }
        [Obsolete( "廃止予定" )]
        public int W { get; set; }



        public int? Left { get; set; }
        public int? Right { get; set; }
        public int? Top { get; set; }
        public int? Bottom { get; set; }

        public int? Width { get; set; }
        public int? Height { get; set; }


        // marginシリーズ

        public string Margin { get; set; }
        [JsonProperty( PropertyName = "margin-top" )]
        public int? MarginTop { get; set; }
        [JsonProperty( PropertyName = "margin-right" )]
        public int? MarginRight { get; set; }
        [JsonProperty( PropertyName = "margin-bottom" )]
        public int? MarginBottom { get; set; }
        [JsonProperty( PropertyName = "margin-left" )]
        public int? MarginLeft { get; set; }


        // padding シリーズ

        public string Padding { get; set; }
        [JsonProperty( PropertyName = "padding-top" )]
        public int? PaddingTop { get; set; }
        [JsonProperty( PropertyName = "padding-right" )]
        public int? PaddingRight { get; set; }
        [JsonProperty( PropertyName = "padding-bottom" )]
        public int? PaddingBottom { get; set; }
        [JsonProperty( PropertyName = "padding-left" )]
        public int? PaddingLeft { get; set; }


        public static TrimmingArea Parse(string json)
        {
            System.Diagnostics.Debug.WriteLine( json );
            var obj = JsonConvert.DeserializeObject<TrimmingArea>( json );

            // 一時的にLeftとWidthに値を伝搬しておく。
            obj.Left = obj.DX;
            obj.Width = obj.W;

            System.Diagnostics.Debug.WriteLine( $"  - left    : {obj.Left}" );
            System.Diagnostics.Debug.WriteLine( $"  - right   : {obj.Right}" );
            System.Diagnostics.Debug.WriteLine( $"  - top     : {obj.Top}" );
            System.Diagnostics.Debug.WriteLine( $"  - bottom  : {obj.Bottom}" );
            System.Diagnostics.Debug.WriteLine( $"  - W       : {obj.Width}" );
            System.Diagnostics.Debug.WriteLine( $"  - H       : {obj.Height}" );

            System.Diagnostics.Debug.WriteLine( $"  - m       : {obj.Margin}" );
            System.Diagnostics.Debug.WriteLine( $"  - m.top   : {obj.MarginTop}" );
            System.Diagnostics.Debug.WriteLine( $"  - m.right : {obj.MarginRight}" );
            System.Diagnostics.Debug.WriteLine( $"  - m.bottom: {obj.MarginBottom}" );
            System.Diagnostics.Debug.WriteLine( $"  - m.left  : {obj.MarginLeft}" );
            System.Diagnostics.Debug.WriteLine( $"  - p       : {obj.Padding}" );
            System.Diagnostics.Debug.WriteLine( $"  - p.top   : {obj.PaddingTop}" );
            System.Diagnostics.Debug.WriteLine( $"  - p.right : {obj.PaddingRight}" );
            System.Diagnostics.Debug.WriteLine( $"  - p.bottom: {obj.PaddingBottom}" );
            System.Diagnostics.Debug.WriteLine( $"  - p.left  : {obj.PaddingLeft}" );
            return obj;
        }
    }
}
