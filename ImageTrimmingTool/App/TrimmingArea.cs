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
        public int DX { get; set; }
        public int W { get; set; }

        public static TrimmingArea Parse(string json)
        {
            return JsonConvert.DeserializeObject<TrimmingArea>( json );
        }
    }
}
