// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-13
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using ScreenshotAnalyzerDatabase.Config;

namespace ScreenshotAnalyzerDatabase.Interfaces {
    public interface IDbOcrRectangle {
        double X1 { get; set; }
        double Y1 { get; set; }
        double X2 { get; set; }
        double Y2 { get; set; }

        Point UpperLeft { get; set; }
        Point LowerRight { get; set; }

        string AdditionalInfo { get; set; }
        RectType Type { get; set; }

        bool DisableSaving { get; set; }
        int Id { get; }
        void Delete();
        void Save();
    }
}
