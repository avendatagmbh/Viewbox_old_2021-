// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-06-21
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;

namespace eBalanceKitBase.Structures {
    /// <summary>
    /// Static class to log Exceptions.
    /// File destination can be configured. Per default it's System.Environment.CurrentDirectory + "\\" + "exceptions.ex".
    /// Base for most of the logic behind this class is the project on http://www.codeproject.com/Articles/7482/User-Friendly-Exception-Handling
    /// </summary>
    public static class ExceptionLogging {

        private static string _logFilename;
        public static string LogFilename { get { return _logFilename ?? (_logFilename = System.Environment.CurrentDirectory + "\\" + "exceptions.ex"); } set { _logFilename = value; } }

        public static void LogException(Exception exception) {

            string distanceToNewEntry = Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine;

            try {
                File.AppendAllText(LogFilename, exception == null ? "empty Exception" : ExceptionToString(exception) + distanceToNewEntry);
            } catch (Exception e) {
                // Do nothing
            }
        }

        //--
        //-- translate exception object to string, with additional system info
        //--
        static internal string ExceptionToString(Exception objException) {
            System.Text.StringBuilder objStringBuilder = new System.Text.StringBuilder();

            if ((objException.InnerException != null)) {
                //-- sometimes the original exception is wrapped in a more relevant outer exception
                //-- the detail exception is the "inner" exception
                //-- see http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnbda/html/exceptdotnet.asp
                var _with5 = objStringBuilder;
                _with5.Append("(Inner Exception)");
                _with5.Append(Environment.NewLine);
                _with5.Append(ExceptionToString(objException.InnerException));
                _with5.Append(Environment.NewLine);
                _with5.Append("(Outer Exception)");
                _with5.Append(Environment.NewLine);
            }
            var _with6 = objStringBuilder;
            //-- get general system and app information
            _with6.Append(SysInfoToString());

            //-- get exception-specific information
            _with6.Append("Exception Source:      ");
            try {
                _with6.Append(objException.Source);
            }
            catch (Exception e) {
                _with6.Append(e.Message);
            }
            _with6.Append(Environment.NewLine);

            _with6.Append("Exception Type:        ");
            try {
                _with6.Append(objException.GetType().FullName);
            }
            catch (Exception e) {
                _with6.Append(e.Message);
            }
            _with6.Append(Environment.NewLine);

            _with6.Append("Exception Message:     ");
            try {
                _with6.Append(objException.Message);
            }
            catch (Exception e) {
                _with6.Append(e.Message);
            }
            _with6.Append(Environment.NewLine);

            try {
                _with6.Append("Exception Target Site: ");
                _with6.Append(objException.TargetSite != null ? objException.TargetSite.Name : "none");
            }
            catch (Exception e) {
                _with6.Append(e.Message);
            }
            _with6.Append(Environment.NewLine);

            try {
                string x = EnhancedStackTrace(objException);
                _with6.Append(x);
            }
            catch (Exception e) {
                _with6.Append(e.Message);
            }
            _with6.Append(Environment.NewLine);


            return objStringBuilder.ToString();
        }
        private static System.Reflection.Assembly _objParentAssembly = null;

        private static System.Reflection.Assembly ParentAssembly() {
            if (_objParentAssembly == null) {
                if (System.Reflection.Assembly.GetEntryAssembly() == null) {
                    _objParentAssembly = System.Reflection.Assembly.GetCallingAssembly();
                }
                else {
                    _objParentAssembly = System.Reflection.Assembly.GetEntryAssembly();
                }
            }
            return _objParentAssembly;
        }

        //--
        //-- exception-safe WindowsIdentity.GetCurrent retrieval returns "domain\username"
        //-- per MS, this sometimes randomly fails with "Access Denied" particularly on NT4
        //--
        private static string CurrentWindowsIdentity() {
            try {
                //return System.Security.Principal.WindowsIdentity.GetCurrent().Name();
                return WindowsIdentity.GetCurrent().Name;
            }
            catch (Exception ex) {
                return "";
            }
        }

        //--
        //-- exception-safe "domain\username" retrieval from Environment
        //--
        private static string CurrentEnvironmentIdentity() {
            try {
                return System.Environment.UserDomainName + "\\" + System.Environment.UserName;
            }
            catch (Exception ex) {
                return "";
            }
        }
 
        //--
        //-- retrieve identity with fallback on error to safer method
        //--
        private static string UserIdentity() {
            string strTemp = null;
            strTemp = CurrentWindowsIdentity();
            if (string.IsNullOrEmpty(strTemp)) {
                strTemp = CurrentEnvironmentIdentity();
            }
            return strTemp;
        }
        private static System.Collections.Specialized.NameValueCollection _objAssemblyAttribs = null;

        private static Assembly GetEntryAssembly() {
            if (System.Reflection.Assembly.GetEntryAssembly() == null) {
                return System.Reflection.Assembly.GetCallingAssembly();
            }
            else {
                return System.Reflection.Assembly.GetEntryAssembly();
            }
        }
 
        //--
        //-- returns string name / string value pair of all attribs
        //-- for specified assembly
        //--
        //-- note that Assembly* values are pulled from AssemblyInfo file in project folder
        //--
        //-- Product         = AssemblyProduct string
        //-- Copyright       = AssemblyCopyright string
        //-- Company         = AssemblyCompany string
        //-- Description     = AssemblyDescription string
        //-- Title           = AssemblyTitle string
        //--
        private static NameValueCollection GetAssemblyAttribs() {
            object[] objAttributes = null;
            object objAttribute = null;
            string strAttribName = null;
            string strAttribValue = null;
            System.Collections.Specialized.NameValueCollection objNameValueCollection = new System.Collections.Specialized.NameValueCollection();
            System.Reflection.Assembly objAssembly = GetEntryAssembly();

            objAttributes = objAssembly.GetCustomAttributes(false);
            foreach (object objAttribute_loopVariable in objAttributes) {
                objAttribute = objAttribute_loopVariable;
                strAttribName = objAttribute.GetType().ToString();
                strAttribValue = "";
                switch (strAttribName) {
                    case "System.Reflection.AssemblyTrademarkAttribute":
                        strAttribName = "Trademark";
                        strAttribValue = ((AssemblyTrademarkAttribute)objAttribute).Trademark.ToString();
                        break;
                    case "System.Reflection.AssemblyProductAttribute":
                        strAttribName = "Product";
                        strAttribValue = ((AssemblyProductAttribute)objAttribute).Product.ToString();
                        break;
                    case "System.Reflection.AssemblyCopyrightAttribute":
                        strAttribName = "Copyright";
                        strAttribValue = ((AssemblyCopyrightAttribute)objAttribute).Copyright.ToString();
                        break;
                    case "System.Reflection.AssemblyCompanyAttribute":
                        strAttribName = "Company";
                        strAttribValue = ((AssemblyCompanyAttribute)objAttribute).Company.ToString();
                        break;
                    case "System.Reflection.AssemblyTitleAttribute":
                        strAttribName = "Title";
                        strAttribValue = ((AssemblyTitleAttribute)objAttribute).Title.ToString();
                        break;
                    case "System.Reflection.AssemblyDescriptionAttribute":
                        strAttribName = "Description";
                        strAttribValue = ((AssemblyDescriptionAttribute)objAttribute).Description.ToString();
                        break;
                    default:
                        break;
                    //Console.WriteLine(strAttribName)
                }
                if (!string.IsNullOrEmpty(strAttribValue)) {
                    if (string.IsNullOrEmpty(objNameValueCollection[strAttribName])) {
                        objNameValueCollection.Add(strAttribName, strAttribValue);
                    }
                }
            }

            //-- add some extra values that are not in the AssemblyInfo, but nice to have
            var _with1 = objNameValueCollection;
            _with1.Add("CodeBase", objAssembly.CodeBase.Replace("file:///", ""));
            _with1.Add("BuildDate", AssemblyBuildDate(objAssembly).ToString());
            _with1.Add("Version", objAssembly.GetName().Version.ToString());
            _with1.Add("FullName", objAssembly.FullName);

            //-- we must have certain assembly keys to proceed.
            if (objNameValueCollection["Product"] == null) {
                throw new MissingFieldException("The AssemblyInfo file for the assembly " + objAssembly.GetName().Name + " must have the <Assembly:AssemblyProduct()> key populated.");
            }
            if (objNameValueCollection["Company"] == null) {
                throw new MissingFieldException("The AssemblyInfo file for the assembly " + objAssembly.GetName().Name + " must have the <Assembly: AssemblyCompany()>  key populated.");
            }

            return objNameValueCollection;
        }

        //--
        //-- exception-safe file attrib retrieval; we don't care if this fails
        //--
        private static DateTime AssemblyFileTime(System.Reflection.Assembly objAssembly) {
            try {
                return System.IO.File.GetLastWriteTime(objAssembly.Location);
            }
            catch (Exception ex) {
                return DateTime.MaxValue;
            }
        }

        //--
        //-- returns build datetime of assembly
        //-- assumes default assembly value in AssemblyInfo:
        //-- <Assembly: AssemblyVersion("1.0.*")>
        //--
        //-- filesystem create time is used, if revision and build were overridden by user
        //--
        private static DateTime AssemblyBuildDate(System.Reflection.Assembly objAssembly, bool blnForceFileDate = false) {
            System.Version objVersion = objAssembly.GetName().Version;
            DateTime dtBuild = default(DateTime);

            if (blnForceFileDate) {
                dtBuild = AssemblyFileTime(objAssembly);
            }
            else {
                //dtBuild = ((DateTime)"01/01/2000").AddDays(objVersion.Build).AddSeconds(objVersion.Revision * 2);
                dtBuild = Convert.ToDateTime("01/01/2000").AddDays((double)objVersion.Build).AddSeconds((double)(objVersion.Revision * 2));
                if (TimeZone.IsDaylightSavingTime(dtBuild, TimeZone.CurrentTimeZone.GetDaylightChanges(dtBuild.Year))) {
                    dtBuild = dtBuild.AddHours(1);
                }
                if (dtBuild > DateTime.Now | objVersion.Build < 730 | objVersion.Revision == 0) {
                    dtBuild = AssemblyFileTime(objAssembly);
                }
            }

            return dtBuild;
        }
 
        //--
        //-- gather some system information that is helpful to diagnosing
        //-- exception
        //--
        static internal string SysInfoToString(bool blnIncludeStackTrace = false) {
            System.Text.StringBuilder objStringBuilder = new System.Text.StringBuilder();

            var _with4 = objStringBuilder;

            _with4.Append("Date and Time:         ");
            _with4.Append(DateTime.Now);
            _with4.Append(Environment.NewLine);

            _with4.Append("Machine Name:          ");
            try {
                _with4.Append(Environment.MachineName);
            }
            catch (Exception e) {
                _with4.Append(e.Message);
            }
            _with4.Append(Environment.NewLine);
            
            _with4.Append("Current User:          ");
            _with4.Append(UserIdentity());
            _with4.Append(Environment.NewLine);
            _with4.Append(Environment.NewLine);

            _with4.Append("Application Domain:    ");
            try {
                //_with4.Append(System.AppDomain.CurrentDomain.FriendlyName());
                _with4.Append(System.AppDomain.CurrentDomain.FriendlyName);
            }
            catch (Exception e) {
                _with4.Append(e.Message);
            }

            _with4.Append(Environment.NewLine);
            _with4.Append("Assembly Codebase:     ");
            try {
                //_with4.Append(ParentAssembly().CodeBase());
                _with4.Append(ParentAssembly().CodeBase);
            }
            catch (Exception e) {
                _with4.Append(e.Message);
            }
            _with4.Append(Environment.NewLine);

            _with4.Append("Assembly Full Name:    ");
            try {
                _with4.Append(ParentAssembly().FullName);
            }
            catch (Exception e) {
                _with4.Append(e.Message);
            }
            _with4.Append(Environment.NewLine);

            _with4.Append("Assembly Version:      ");
            try {
                //_with4.Append(ParentAssembly().GetName().Version().ToString);
                _with4.Append(ParentAssembly().GetName().Version.ToString());
            }
            catch (Exception e) {
                _with4.Append(e.Message);
            }
            _with4.Append(Environment.NewLine);

            _with4.Append("Assembly Build Date:   ");
            try {
                _with4.Append(AssemblyBuildDate(ParentAssembly()).ToString());
            }
            catch (Exception e) {
                _with4.Append(e.Message);
            }
            _with4.Append(Environment.NewLine);
            _with4.Append(Environment.NewLine);

            if (blnIncludeStackTrace) {
                _with4.Append(EnhancedStackTrace());
            }

            return objStringBuilder.ToString();
        }

        //--
        //-- turns a single stack frame object into an informative string
        //--
        private static string StackFrameToString(StackFrame sf) {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int intParam = 0;
            MemberInfo mi = sf.GetMethod();

            var _with1 = sb;
            //-- build method name
            _with1.Append("   ");
            _with1.Append(mi.DeclaringType.Namespace);
            _with1.Append(".");
            _with1.Append(mi.DeclaringType.Name);
            _with1.Append(".");
            _with1.Append(mi.Name);

            //-- build method params
            ParameterInfo[] objParameters = sf.GetMethod().GetParameters();
            ParameterInfo objParameter = null;
            _with1.Append("(");
            intParam = 0;
            foreach (ParameterInfo objParameter_loopVariable in objParameters) {
                objParameter = objParameter_loopVariable;
                intParam += 1;
                if (intParam > 1)
                    _with1.Append(", ");
                _with1.Append(objParameter.Name);
                _with1.Append(" As ");
                _with1.Append(objParameter.ParameterType.Name);
            }
            _with1.Append(")");
            _with1.Append(Environment.NewLine);

            //-- if source code is available, append location info
            _with1.Append("       ");
            if (sf.GetFileName() == null || sf.GetFileName().Length == 0) {
                _with1.Append(System.IO.Path.GetFileName(ParentAssembly().CodeBase));
                //-- native code offset is always available
                _with1.Append(": N ");
                _with1.Append(string.Format("{0:#00000}", sf.GetNativeOffset()));
            }
            else {
                _with1.Append(System.IO.Path.GetFileName(sf.GetFileName()));
                _with1.Append(": line ");
                _with1.Append(string.Format("{0:#0000}", sf.GetFileLineNumber()));
                _with1.Append(", col ");
                _with1.Append(string.Format("{0:#00}", sf.GetFileColumnNumber()));
                //-- if IL is available, append IL location info
                if (sf.GetILOffset() != StackFrame.OFFSET_UNKNOWN) {
                    _with1.Append(", IL ");
                    _with1.Append(string.Format("{0:#0000}", sf.GetILOffset()));
                }
            }
            _with1.Append(Environment.NewLine);
            return sb.ToString();
        }

        //--
        //-- enhanced stack trace generator
        //--
        private static string EnhancedStackTrace(StackTrace objStackTrace, string strSkipClassName = "") {
            int intFrame = 0;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append(Environment.NewLine);
            sb.Append("---- Stack Trace ----");
            sb.Append(Environment.NewLine);

            for (intFrame = 0; intFrame <= objStackTrace.FrameCount - 1; intFrame++) {
                StackFrame sf = objStackTrace.GetFrame(intFrame);
                MemberInfo mi = sf.GetMethod();

                if (!string.IsNullOrEmpty(strSkipClassName) && mi.DeclaringType.Name.IndexOf(strSkipClassName) > -1) {
                    //-- don't include frames with this name
                }
                else {
                    sb.Append(StackFrameToString(sf));
                }
            }
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }

        //--
        //-- enhanced stack trace generator (exception)
        //--
        private static string EnhancedStackTrace(Exception objException) {
            StackTrace objStackTrace = new StackTrace(objException, true);
            return EnhancedStackTrace(objStackTrace);
        }

        //--
        //-- enhanced stack trace generator (no params)
        //--
        private static string EnhancedStackTrace() {
            StackTrace objStackTrace = new StackTrace(true);
            return EnhancedStackTrace(objStackTrace, "ExceptionManager");
        }
 



        //private static void TakeScreenshotPrivate(string strFilename) {
        //    Rectangle objRectangle = Screen.PrimaryScreen.Bounds;
        //    Bitmap objBitmap = new Bitmap(objRectangle.Right, objRectangle.Bottom);
        //    Graphics objGraphics = null;
        //    IntPtr hdcDest = default(IntPtr);
        //    int hdcSrc = 0;
        //    const int SRCCOPY = 0xcc0020;
        //    string strFormatExtension = null;

        //    //objGraphics = objGraphics.FromImage(objBitmap);
        //    objGraphics = Graphics.FromImage(objBitmap);

        //    //-- get a device context to the windows desktop and our destination  bitmaps
        //    hdcSrc = GetDC(0);
        //    hdcDest = objGraphics.GetHdc();
        //    //-- copy what is on the desktop to the bitmap
        //    BitBlt(hdcDest.ToInt32(), 0, 0, objRectangle.Right, objRectangle.Bottom, hdcSrc, 0, 0, SRCCOPY);
        //    //-- release device contexts
        //    objGraphics.ReleaseHdc(hdcDest);
        //    ReleaseDC(0, hdcSrc);

        //    strFormatExtension = _ScreenshotImageFormat.ToString().ToLower();
        //    if (System.IO.Path.GetExtension(strFilename) != "." + strFormatExtension) {
        //        strFilename += "." + strFormatExtension;
        //    }
        //    switch (strFormatExtension) {
        //        case "jpeg":
        //            BitmapToJPEG(objBitmap, strFilename, 80);
        //            break;
        //        default:
        //            objBitmap.Save(strFilename, _ScreenshotImageFormat);
        //            break;
        //    }

        //    //-- save the complete path/filename of the screenshot for possible later use
        //    _strScreenshotFullPath = strFilename;
        //}
 
    }
}