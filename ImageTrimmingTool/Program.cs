using ImageTrimmingTool.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace ImageTrimmingTool
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                new TrimApp( args ).Execute();
            }
            catch ( Exception ex )
            {
                Console.WriteLine( ex.Message );
            }
        }
    }
}
