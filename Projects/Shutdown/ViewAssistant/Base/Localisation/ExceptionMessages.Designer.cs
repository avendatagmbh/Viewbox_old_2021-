﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18052
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Base.Localisation {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class ExceptionMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ExceptionMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Base.Localisation.ExceptionMessages", typeof(ExceptionMessages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error during config database initialization process: {0}.
        /// </summary>
        public static string ConfigDbInitializationException {
            get {
                return ResourceManager.GetString("ConfigDbInitializationException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The config database has not yet been initialized..
        /// </summary>
        public static string ConfigDbNotInitializedException {
            get {
                return ResourceManager.GetString("ConfigDbNotInitializedException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to create table {0}: {1}.
        /// </summary>
        public static string CreateTableFailed {
            get {
                return ResourceManager.GetString("CreateTableFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not load datas!.
        /// </summary>
        public static string LoadDataException {
            get {
                return ResourceManager.GetString("LoadDataException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not load final datas!.
        /// </summary>
        public static string LoadFinalDataException {
            get {
                return ResourceManager.GetString("LoadFinalDataException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not load source datas!.
        /// </summary>
        public static string LoadSourceDataException {
            get {
                return ResourceManager.GetString("LoadSourceDataException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not load Viewbox datas!.
        /// </summary>
        public static string LoadViewboxDataException {
            get {
                return ResourceManager.GetString("LoadViewboxDataException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not connect to final database!.
        /// </summary>
        public static string TestFinalConnectionException {
            get {
                return ResourceManager.GetString("TestFinalConnectionException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not connect to source database!.
        /// </summary>
        public static string TestSourceConnectionException {
            get {
                return ResourceManager.GetString("TestSourceConnectionException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not connect to Viewbox database!.
        /// </summary>
        public static string TestViewboxConnectionException {
            get {
                return ResourceManager.GetString("TestViewboxConnectionException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An unhandled exception has been occured: {0}.
        /// </summary>
        public static string UnhandledException {
            get {
                return ResourceManager.GetString("UnhandledException", resourceCulture);
            }
        }
    }
}
