using System;

namespace ImageTrimmingTool {
    
    //[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    //[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
    internal partial class Trimming : global::System.Configuration.ApplicationSettingsBase {

        public enum TrimMode
        {
            SubDirectory,
            SwapFile,
        }

        //[global::System.Configuration.ApplicationScopedSettingAttribute()]
        //[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        //[global::System.Configuration.DefaultSettingValueAttribute("f")]
        public TrimMode Mode {
            get {
                string mode = this.ModeValue;

                switch ( mode.ToLower() )
                {
                    case "sub-directory":
                    case "d":
                    case "directory":
                    case "subdirectory":
                        return TrimMode.SubDirectory;
                    case "c":
                    case "copy":
                        return TrimMode.SubDirectory;

                    case "swap-file":
                    case "f":
                    case "file":
                    case "swapfile":
                        return TrimMode.SwapFile;
                    case "o":
                    case "overwrite":
                        return TrimMode.SwapFile;

                    default:
                        throw new ArgumentException( mode );
                }
            }
        }
    }
}
