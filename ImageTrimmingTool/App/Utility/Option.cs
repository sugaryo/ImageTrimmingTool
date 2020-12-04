using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace ImageTrimmingTool.App.Utility
{
    public class Option
    {
        private readonly List<string> options;

        public Option(string parameter)
        {
            this.options = parameter.split( "--" ).ToList();
        }

        public bool Has(string name)
        {
            return this.options.Contains( name );
        }
        public bool Has(params string[] names)
        {
            foreach ( var name in names )
            {
                if ( this.options.Contains( name ) )
                {
                    return true;
                }
            }
            return false;
        }

        public bool Match(string regex)
        {
            foreach ( var option in this.options )
            {
                var m = Regex.Match( option, regex );
                if ( m.Success )
                {
                    return true;
                }
            }
            return false;
        }
        public bool Match(string regex, out string match)
        {
            foreach ( var option in this.options )
            {
                var m = Regex.Match( option, regex );
                if ( m.Success )
                {
                    match = option;
                    return true;
                }
            }
            match = null;
            return false;
        }
        public bool Match(string regex, string group, out string match)
        {
            foreach ( var option in this.options )
            {
                var m = Regex.Match( option, regex );
                if ( m.Success )
                {
                    match = m.Groups[group].Value;
                    return true;
                }
            }
            match = null;
            return false;
        }
        public bool Match(string regex, int index, out string match)
        {
            foreach ( var option in this.options )
            {
                var m = Regex.Match( option, regex );
                if ( m.Success )
                {
                    match = m.Groups[index].Value;
                    return true;
                }
            }
            match = null;
            return false;
        }

    }
}
