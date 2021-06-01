using System;
using System.Reflection;
using System.Web.Mvc;

public static class VersionHelper
{
    /// <summary>
    ///   Return the Current Version from the AssemblyInfo.cs file.
    /// </summary>
    public static string AddCurrentVersion(this HtmlHelper helper, string fileName)
    {
        try
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            return fileName + "?v=" + version.Major + version.Minor + version.Build + version.Revision;
        }
        catch (Exception)
        {
            return "?.?.?";
        }
    }
}