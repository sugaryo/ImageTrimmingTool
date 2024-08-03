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
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine( "エラーが発生しました!!" );
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine( ex.Message );
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine( ex.StackTrace );
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine( "何かキーを押してください。" );
                Console.ResetColor();
                Console.ReadKey( true );
            }
        }
    }
}
