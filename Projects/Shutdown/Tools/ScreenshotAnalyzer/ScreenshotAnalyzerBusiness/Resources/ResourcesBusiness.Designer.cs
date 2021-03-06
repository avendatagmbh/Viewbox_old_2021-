//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.239
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ScreenshotAnalyzerBusiness.Resources {
    using System;
    
    
    /// <summary>
    ///   Eine stark typisierte Ressourcenklasse zum Suchen von lokalisierten Zeichenfolgen usw.
    /// </summary>
    // Diese Klasse wurde von der StronglyTypedResourceBuilder automatisch generiert
    // -Klasse über ein Tool wie ResGen oder Visual Studio automatisch generiert.
    // Um einen Member hinzuzufügen oder zu entfernen, bearbeiten Sie die .ResX-Datei und führen dann ResGen
    // mit der /str-Option erneut aus, oder Sie erstellen Ihr VS-Projekt neu.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class ResourcesBusiness {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ResourcesBusiness() {
        }
        
        /// <summary>
        ///   Gibt die zwischengespeicherte ResourceManager-Instanz zurück, die von dieser Klasse verwendet wird.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ScreenshotAnalyzerBusiness.Resources.ResourcesBusiness", typeof(ResourcesBusiness).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Überschreibt die CurrentUICulture-Eigenschaft des aktuellen Threads für alle
        ///   Ressourcenzuordnungen, die diese stark typisierte Ressourcenklasse verwenden.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die  (inaktiv: Datenbank muss upgegradet werden) ähnelt.
        /// </summary>
        internal static string Profile_DisplayString_inactive_DbVersionTooOld {
            get {
                return ResourceManager.GetString("Profile_DisplayString_inactive_DbVersionTooOld", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die  (inaktiv: Fehler beim Laden) ähnelt.
        /// </summary>
        internal static string Profile_DisplayString_inactive_LoadFailed {
            get {
                return ResourceManager.GetString("Profile_DisplayString_inactive_LoadFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die  (inaktiv: ScreenshotAnalyzer Version ist zu alt) ähnelt.
        /// </summary>
        internal static string Profile_DisplayString_inactive_ProgramVersionTooOld {
            get {
                return ResourceManager.GetString("Profile_DisplayString_inactive_ProgramVersionTooOld", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Fehler beim laden des Profils:  ähnelt.
        /// </summary>
        internal static string ProfileManager_AddProfile_ErrorLoadingProfile {
            get {
                return ResourceManager.GetString("ProfileManager_AddProfile_ErrorLoadingProfile", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Die Bilder haben unterschiedlich viele Rechtecke, bitte übertragen Sie zuerst die Rechtecke auf die anderen Bilder. ähnelt.
        /// </summary>
        internal static string RecognitionResult_ExtractText_ErrorDifferentRectanglesCount {
            get {
                return ResourceManager.GetString("RecognitionResult_ExtractText_ErrorDifferentRectanglesCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Es wurden keine Screenshots ausgewählt, bitte zuerst Bilder zur Liste hinzufügen ähnelt.
        /// </summary>
        internal static string RecognitionResult_ExtractText_ErrorNoScreenshotsLoaded {
            get {
                return ResourceManager.GetString("RecognitionResult_ExtractText_ErrorNoScreenshotsLoaded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Kein Anker im Referenzbild ausgewählt ähnelt.
        /// </summary>
        internal static string ScreenshotGroup_SetRectanglesForGroup_Error_NoAnchorInRefImage {
            get {
                return ResourceManager.GetString("ScreenshotGroup_SetRectanglesForGroup_Error_NoAnchorInRefImage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Kein Referenzbild ausgewählt ähnelt.
        /// </summary>
        internal static string ScreenshotGroup_SetRectanglesForGroup_Error_NoReferenceImageSelected {
            get {
                return ResourceManager.GetString("ScreenshotGroup_SetRectanglesForGroup_Error_NoReferenceImageSelected", resourceCulture);
            }
        }
    }
}
