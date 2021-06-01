// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-10-08
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Linq;

namespace eBalanceKitBusiness.RtfHtmlConverter {
    public class DataCommunicator {
        /// <summary> 
        /// Converts the parameter <see cref="value"/> to valid HTML by using the <see cref="HtmlFromXamlConverter"/>
        /// </summary>
        /// <param name="value">The Xaml.</param>
        /// <returns></returns>
        public static string GetHtml(string value) {
            var xaml = value;
            var html = HtmlFromXamlConverter.ConvertXamlToHtml(xaml, false);
            html = html.Replace("&amp;#", "&#");
            return html;
        }

        public static string GetHtml(FlowDocument doc) { return GetHtml(GetXaml(doc)); }



        /// <summary>
        /// Converts the parameter <see cref="value"/> to valid XAML by using the <see cref="HtmlToXamlConverter"/>
        /// </summary>
        /// <param name="value">The Html.</param>
        /// <returns>The Xaml representation.</returns>
        public static string GetXaml(string value) { return HtmlToXamlConverter.ConvertHtmlToXaml(value, false); }


        /// <summary>
        /// Converts the content of the parameter <see cref="doc"/> to valid XAML by using TextRange and MemoryStram.
        /// </summary>
        /// <param name="doc">A FlowDocument that could be the <see cref="RichTextBox.Document"/>.</param>
        /// <returns>The Xaml representation.</returns>
        public static string GetXaml(FlowDocument doc) {

            using (MemoryStream ms = new MemoryStream()) {

                TextRange tr = new TextRange(doc.ContentStart, doc.ContentEnd);
                tr.Save(ms, DataFormats.Xaml);
                string xamlText = Encoding.UTF8.GetString(ms.ToArray());

                XmlDocument xamlDoc = new XmlDocument();
                xamlDoc.LoadXml(xamlText);


                //XElement textsElement = XElement.Load(new XmlNodeReader(xamlDoc));
                //var newTextsElement = new XElement(xamlDoc.DocumentElement.Name,
                //                                   textsElement.Elements().Distinct(new TextElementEqualityComparer()));
                var xaml2 = xamlDoc.InnerXml;
                return xamlText;
            }
        }



    //private class TextElementEqualityComparer : IEqualityComparer<XElement>
    //{
    //    public bool Equals(XElement x, XElement y)
    //    {
    //        return x.Attribute("b").Value == y.Attribute("b").Value
    //            && x.Attribute("b").Value == y.Attribute("b").Value;
    //    }

    //    public int GetHashCode(XElement obj)
    //    {
    //        if (!obj.HasAttributes)
    //            return 0;
    //        return obj.Attribute("b").Value.GetHashCode() ^ obj.Attribute("u").Value.GetHashCode();
    //    }
    //}



        public static void GetFromHtml(string html, FlowDocument flowDocument) {
            LoadXamlIntoDocument(flowDocument, GetXaml(html));
        }

        public const int AnsiCodePage = 1252;
        public static readonly Encoding AnsiEncoding = Encoding.GetEncoding(AnsiCodePage);

        private static System.Windows.Controls.RichTextBox _rtBox;

        public static Stream LoadRtfFromFile(string link) {
            if (!File.Exists(link)) {
                throw new FileNotFoundException("Datei konnte nicht geladen werden", link);
            }
            Stream result = new MemoryStream();
            using (FileStream stream = File.Open(link, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                var reader = new StreamReader(stream, AnsiEncoding);
                reader.BaseStream.CopyTo(result);
                //Selection.Load(reader.BaseStream, DataFormats.Rtf);
            }
            return result;
        }

        public static void LoadXamlIntoDocument(FlowDocument document, string newXaml) {

            if (!string.IsNullOrEmpty(newXaml)) {
                using (MemoryStream xamlMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(newXaml))) {
                    LoadRtf(document, xamlMemoryStream);
                }
            }
        }
        public static FlowDocument LoadRtf(RichTextBox rtBox, string file) {
            
            var stream = LoadRtfFromFile(file);
            //LoadRtf(rtBox.Document, stream);
            rtBox.Selection.Load(stream, DataFormats.Rtf);
            //var xaml = GetXaml(rtBox.Document);
            var html = GetHtml(rtBox.Document);
            GetFromHtml(html, rtBox.Document);
            //LoadXamlIntoDocument(rtBox.Document, xaml);
            return rtBox.Document;
            //LoadRtf(document, stream);
        }

        public static void LoadRtf(FlowDocument document, string file, out string html) {
            html = GetHtmlFromFile(file);
            GetFromHtml(html, document);
        }

        public static void LoadRtf(FlowDocument document, string file) {
            var html = GetHtmlFromFile(file);
            GetFromHtml(html, document);
        }

        public static string GetHtmlFromFile(string file) {
            var rtBox = new RichTextBox();
            var stream = LoadRtfFromFile(file);
            //LoadRtf(rtBox.Document, stream);
            rtBox.Selection.Load(stream, DataFormats.Rtf);
            //var xaml = GetXaml(rtBox.Document);
            var html = GetHtml(rtBox.Document);
            return html;
        }

        //public static void LoadRtf() {

        //    DataCommunicator.LoadRtf(richTextBox, dlg.Filename);
        //    DataCommunicator.LoadXamlIntoDocument(RichTextBoxDocument, DataCommunicator.GetXaml());
        //}

        public static void LoadRtf(FlowDocument document, Stream stream) {
            document.Blocks.Clear();
            
            ParserContext parser = new ParserContext();
            parser.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            parser.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            //FlowDocument doc = new FlowDocument();
            var section = XamlReader.Load(stream, parser) as Section;

            if (section != null) {
                document.Blocks.Add(section);
            } else {
                System.Diagnostics.Debug.Fail("section is null");
            }

        }
    }
}