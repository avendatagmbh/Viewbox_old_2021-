using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DbComparisonV2.DocumentGenerator
{
    public class Style
    {
        #region fields
        private Point _topLeftPoint = new Point();
        private Point _bottomRightPoint = new Point();
        #endregion

        #region properties
        public System.Drawing.Color BackgroundColor { get; set; }
        public Border Border { get; set; }
        public double TopLeftX
        {
            set { _topLeftPoint.X = value; }
        }
        public double TopLeftY
        {
            set { _topLeftPoint.Y = value; }
        }
        public double BottomRightX
        {
            set { _bottomRightPoint.X = value; }
        }
        public double BottomRightY
        {
            set { _bottomRightPoint.Y = value; }
        }
        public string TopLeftRange
        {
            get { return ((char)(_topLeftPoint.Y + 64)).ToString() + _topLeftPoint.X; }
        }
        public string BottomRightRange
        {
            get { return ((char)(_bottomRightPoint.Y + 64)).ToString() + _bottomRightPoint.X; }
        }
        #endregion


        public string Name { get; set; }
    }

    [Flags]
    public enum Border { 
        None = 0x00,
        Left = 0x01,
        Right= 0x02,
        Top = 0x04,
        Bottom = 0x08,
        All = Left | Right | Top | Bottom
    }
    /// <summary>
    /// Hold a list of FileCOntent object (independant of the media supporting the content - excel - pdf - etc) 
    /// containing the location and the content (text, or formatting)
    /// Can chain the content thanks to AddContent
    /// </summary>
    public class FileContent
    {
        private static List<FileContent> _sheetContent = new List<FileContent>();

        private static int _currentLine = 0;
        private static int _currentColumn = 0;
        private Border _border = Border.None;


        public FileContent() { }

        #region methods
        /// <summary>
        /// Forward-only , method chaining way to add contents in the static list of content
        /// adds content to the line following the current one by default
        /// </summary>
        /// <param name="contentText"></param>
        /// <param name="addLine"></param>
        /// <param name="addColumn"></param>
        /// <returns>a content point to which to add content from</returns>
        public FileContent AddContent(object contentText, int addLine = 1, int addColumn = 0, Border border = Border.None, Style style = null)
        {
            // reset the column count if new line
            if (addLine == 1) _currentColumn = 0;

            _currentLine += addLine;
            _currentColumn += addColumn;
            var content = new FileContent { Location = new Location { Row = _currentLine, Column = _currentColumn }, Content = contentText, Border= border, Style = style };
            return AddContent(content);
        }
        internal FileContent AddLine()
        {
            _currentColumn = 0;
            _currentLine++;
            return this;
        }
        private FileContent AddContent(FileContent content)
        {
            _sheetContent.Add(content);
            return content;
        }
        /// <summary>
        /// Use to reset the row and column index
        /// </summary>
        public void ResetCounter() { 
            _currentLine = 0;
            _currentColumn = 0;
        }
        public Point GetCurrentPosition() {
            return new Point((double)_currentColumn, (double)_currentLine);
        }
        public void SetCurrentLine(int lineIndex) {
            _currentLine = lineIndex;
        }
        public void ClearAll()
        {
            ResetCounter();
            _sheetContent = new List<FileContent>();
        }
        #endregion

        #region properties
        internal Border Border
        {
            get { return _border; }
            set { _border = value; }
        }
        public Location Location { get; set; }
        public object Content { get; set; }

        public List<FileContent> SheetContent
        {
            get { return _sheetContent; }
        }
        public Style Style { get; set; }
        #endregion



    }
    public class Location
    {
        public int Column { get; set; }
        public int Row { get; set; }
        public override string ToString()
        {
            return string.Format("row:{0}, col:{1}", Row, Column);
        }
    }
}
