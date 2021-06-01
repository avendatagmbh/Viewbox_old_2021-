//---------------------------------------------------------------------------
// 
// File: HtmlFromXamlConverter.cs
//
// Copyright (C) Microsoft Corporation.  All rights reserved.
//
// Description: Prototype for Xaml - Html conversion 
//
//---------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Xml;

namespace eBalanceKitBusiness.RtfHtmlConverter {
    /// <summary>
    /// HtmlToXamlConverter is a static class that takes an HTML string
    /// and converts it into XAML
    /// </summary>
    internal static class HtmlFromXamlConverter {
        // ---------------------------------------------------------------------
        //
        // Internal Methods
        //
        // ---------------------------------------------------------------------

        #region Internal Methods
        internal static string ConvertXamlToHtml(string xamlString) {
            return ConvertXamlToHtml(xamlString, true);
        }
        /// <summary>
        /// Main entry point for Xaml-to-Html converter.
        /// Converts a xaml string into html string.
        /// </summary>
        /// <param name="xamlString">
        /// Xaml strinng to convert.
        /// </param>
        /// <returns>
        /// Html string produced from a source xaml.
        /// </returns>
        internal static string ConvertXamlToHtml(string xamlString, bool asFlowDocument) {
            XmlTextReader xamlReader;
            StringBuilder htmlStringBuilder;
            XmlTextWriter htmlWriter;

            if (!asFlowDocument) {
                xamlString = "<FlowDocument>" + xamlString + "</FlowDocument>";
            }


            xamlReader = new XmlTextReader(new StringReader(xamlString));

            htmlStringBuilder = new StringBuilder(100);
            htmlWriter = new XmlTextWriter(new StringWriter(htmlStringBuilder));

            if (!WriteFlowDocument(xamlReader, htmlWriter)) {
                return "";
            }


            string htmlString = htmlStringBuilder.ToString();

            //htmlString = htmlString.Replace("ä", "&auml;");
            //htmlString = htmlString.Replace("Ä", "&Auml;");
            //htmlString = htmlString.Replace("ö", "&ouml;");
            //htmlString = htmlString.Replace("Ö", "&Ouml;");
            //htmlString = htmlString.Replace("ü", "&uuml;");
            //htmlString = htmlString.Replace("Ü", "&Uuml;");
            try {

                XmlDocument htmlDoc = new XmlDocument();
                htmlDoc.LoadXml(htmlString);
                var boldEntries = htmlDoc.GetElementsByTagName("b");
                XmlNodeList underlineEntries;
                var italicEntries = htmlDoc.GetElementsByTagName("i");
                if (boldEntries.Count > 0) {
                    foreach (XmlElement boldEntry in boldEntries) {
                        underlineEntries = boldEntry.GetElementsByTagName("u");
                        if (underlineEntries.Count > 0) {
                            boldEntry.InnerXml = boldEntry.InnerXml.Replace("<u>", string.Empty).Replace("</u>", string.Empty);
                        }
                    }
                }
                if (italicEntries.Count > 0) {
                    foreach (XmlElement italicEntry in italicEntries) {
                        underlineEntries = italicEntry.GetElementsByTagName("u");
                        if (underlineEntries.Count > 0) {
                            italicEntry.InnerXml = italicEntry.InnerXml.Replace("<u>", string.Empty).Replace("</u>", string.Empty);
                        }
                    }
                }
                underlineEntries = htmlDoc.GetElementsByTagName("u");
                if (underlineEntries.Count > 0) {
                    foreach (XmlElement underlineEntry in underlineEntries) {
                        boldEntries = underlineEntry.GetElementsByTagName("b");
                        if (boldEntries.Count > 0) {
                            underlineEntry.InnerXml = underlineEntry.InnerXml.Replace("<b>", string.Empty).Replace("</b>", string.Empty);
                        }
                    }
                }

                    htmlString = htmlDoc.InnerXml;
                    while (htmlString.StartsWith(StartElement + "<br />")) {
                        htmlString = htmlString.Replace(StartElement + "<br />", StartElement);
                    }


                    var e = htmlDoc.GetElementsByTagName("b");
                    GetCleanedXaml(e, htmlDoc.GetElementsByTagName("b"));
                    e = htmlDoc.GetElementsByTagName("u");
                    GetCleanedXaml(e, htmlDoc.GetElementsByTagName("u"));
                    e = htmlDoc.GetElementsByTagName("i");
                    GetCleanedXaml(e, htmlDoc.GetElementsByTagName("u"));

                return htmlDoc.InnerXml;
            }
            catch (Exception ex) {
                //ExceptionLogging.Log(ex);
            }
            return htmlString;
        }

        private static void GetCleanedXaml(XmlNodeList nodeListToModify, XmlNodeList nodeListToIterate) {
            //var nl = nodeList;
            //var x = new XmlDocument();
            //x.Clone()
            for (int i = 0; i < nodeListToIterate.Count; i++) {
                var entry = nodeListToModify[i];
                if(entry == null)
                    continue;
                var sibling = entry.NextSibling;
                //var x = nodeListToModify.
                if (sibling != null && entry.Name.Equals(sibling.Name)) {
                    entry.InnerXml += sibling.InnerXml;
                    if (sibling.ParentNode == null) {
                        continue;
                    }
                    sibling.ParentNode.RemoveChild(sibling);
                }
                //sibling = entry.PreviousSibling;
                ////var x = nodeListToModify.
                //if (sibling != null && entry.Name.Equals(sibling.Name)) {
                //    entry.InnerXml += sibling.InnerXml;
                //    if (sibling.ParentNode == null) {
                //        continue;
                //    }
                //    sibling.ParentNode.RemoveChild(sibling);
                //}
            }

            //foreach (XmlElement entry in nodeListToIterate) {
            //    var sibling = entry.NextSibling;
            //    var x = nodeListToModify.
            //    if (sibling != null && entry.Name.Equals(sibling.Name)) {
            //        entry.InnerXml += sibling.InnerXml;
            //        if (sibling.ParentNode == null) {
            //            continue;
            //        }
            //        sibling.ParentNode.RemoveChild(sibling);
            //    }
            //}
        }

        #endregion Internal Methods

        internal const string StartElementName = "ebk";
        internal const string StartElement = "<" + StartElementName + ">";

        // ---------------------------------------------------------------------
        //
        // Private Methods
        //
        // ---------------------------------------------------------------------

        #region Private Methods
        /// <summary>
        /// Processes a root level element of XAML (normally it's FlowDocument element).
        /// </summary>
        /// <param name="xamlReader">
        /// XmlTextReader for a source xaml.
        /// </param>
        /// <param name="htmlWriter">
        /// XmlTextWriter producing resulting html
        /// </param>
        private static bool WriteFlowDocument(XmlTextReader xamlReader, XmlTextWriter htmlWriter) {
            if (!ReadNextToken(xamlReader)) {
                // Xaml content is empty - nothing to convert
                return false;
            }

            if (xamlReader.NodeType != XmlNodeType.Element || xamlReader.Name != "FlowDocument") {
                // Root FlowDocument elemet is missing
                return false;
            }

            // Create a buffer StringBuilder for collecting css properties for inline STYLE attributes
            // on every element level (it will be re-initialized on every level).
            StringBuilder inlineStyle = new StringBuilder();

            //htmlWriter.WriteStartElement("HTML");
            htmlWriter.WriteStartElement(StartElementName);

            var counter = WriteFormattingProperties(xamlReader, htmlWriter, inlineStyle);

            WriteElementContent(xamlReader, htmlWriter, inlineStyle);

            while (counter-- > 0) { htmlWriter.WriteEndElement(); }

            htmlWriter.WriteEndElement();
            //htmlWriter.WriteEndElement();

            return true;
        }



        /// <summary>
        /// Reads attributes of the current xaml element and converts
        /// them into appropriate html attributes or css styles.
        /// </summary>
        /// <param name="xamlReader">
        /// XmlTextReader which is expected to be at XmlNodeType.Element
        /// (opening element tag) position.
        /// The reader will remain at the same level after function complete.
        /// </param>
        /// <param name="htmlWriter">
        /// XmlTextWriter for output html, which is expected to be in
        /// after WriteStartElement state.
        /// </param>
        /// <param name="inlineStyle">
        /// String builder for collecting css properties for inline STYLE attribute.
        /// </param>
        private static int WriteFormattingProperties(XmlTextReader xamlReader, XmlTextWriter htmlWriter, StringBuilder inlineStyle) {
            Debug.Assert(xamlReader.NodeType == XmlNodeType.Element);

            // Clear string builder for the inline style
            inlineStyle.Remove(0, inlineStyle.Length);
            var elementCounter = 0;

            if (!xamlReader.HasAttributes) {
                return 0;
            }

            bool borderSet = false;

            while (xamlReader.MoveToNextAttribute()) {
                string css = null;

                switch (xamlReader.Name) {
                    // Character fomatting properties
                    // ------------------------------
                    case "Background":
                        //css = "background-color:" + ParseXamlColor(xamlReader.Value) + ";";
                        break;
                    case "FontFamily":
                        //css = "font-family:" + xamlReader.Value + ";";
                        break;
                    case "FontStyle":
                        if (xamlReader.Value.ToLower().Equals("italic")) {
                            htmlWriter.WriteStartElement("i");
                            elementCounter++;
                        }
                        //css = "font-style:" + xamlReader.Value.ToLower() + ";";
                        break;
                    case "FontWeight":
                        if (xamlReader.Value.ToLower().Equals("bold")) {
                            htmlWriter.WriteStartElement("b");
                            elementCounter++;
                        }
                        //css = "font-weight:" + xamlReader.Value.ToLower() + ";";
                        break;
                    case "FontStretch":
                        break;
                    case "FontSize":
                        //css = "font-size:" + xamlReader.Value + ";";
                        int dec;
                        var value = xamlReader.Value.Contains(".")
                                        ? xamlReader.Value.Substring(0, xamlReader.Value.IndexOf('.'))
                                        : xamlReader.Value;
                        value = value.Contains(",")
                                        ? value.Substring(0, xamlReader.Value.IndexOf(','))
                                        : value;
                        if (int.TryParse(value, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.CurrentCulture, out dec)) {
                            if (dec >= 20) {
                                htmlWriter.WriteStartElement("h1");
                                elementCounter++;
                            }
                            else if (dec >= 18) {
                                htmlWriter.WriteStartElement("h2");
                                elementCounter++;
                            }
                        }
                        break;
                    case "Foreground":
                        //css = "color:" + ParseXamlColor(xamlReader.Value) + ";";
                        break;
                    case "TextDecorations":
                        if (xamlReader.Value.ToLower().Equals("underline")) {
                            htmlWriter.WriteStartElement("u");
                            elementCounter++;
                        }
                        //css = "text-decoration:underline;";
                        break;
                    case "TextEffects":
                        break;
                    case "Emphasis":
                        break;
                    case "StandardLigatures":
                        break;
                    case "Variants":
                        break;
                    case "Capitals":
                        break;
                    case "Fraction":
                        break;

                    // Paragraph formatting properties
                    // -------------------------------
                    case "Padding":
                        //css = "padding:" + ParseXamlThickness(xamlReader.Value) + ";";
                        break;
                    case "Margin":
                        //css = "margin:" + ParseXamlThickness(xamlReader.Value) + ";";
                        break;
                    case "BorderThickness":
                        //css = "border-width:" + ParseXamlThickness(xamlReader.Value) + ";";
                        borderSet = true;
                        break;
                    case "BorderBrush":
                        //css = "border-color:" + ParseXamlColor(xamlReader.Value) + ";";
                        borderSet = true;
                        break;
                    case "LineHeight":
                        break;
                    case "TextIndent":
                        //css = "text-indent:" + xamlReader.Value + ";";
                        break;
                    case "TextAlignment":
                        //css = "text-align:" + xamlReader.Value + ";";
                        break;
                    case "IsKeptTogether":
                        break;
                    case "IsKeptWithNext":
                        break;
                    case "ColumnBreakBefore":
                        break;
                    case "PageBreakBefore":
                        break;
                    case "FlowDirection":
                        break;

                    // Table attributes
                    // ----------------
                    case "Width":
                        //css = "width:" + xamlReader.Value + ";";
                        break;
                    case "ColumnSpan":
                        htmlWriter.WriteAttributeString("colspan", xamlReader.Value);
                        break;
                    case "RowSpan":
                        htmlWriter.WriteAttributeString("rowspan", xamlReader.Value);
                        break;
                }

                if (css != null) {
                    inlineStyle.Append(css);
                }
            }

            //if (borderSet) {
            //    inlineStyle.Append("border-style:solid;mso-element:para-border-div;");
            //}

            // Return the xamlReader back to element level
            xamlReader.MoveToElement();
            Debug.Assert(xamlReader.NodeType == XmlNodeType.Element);

            return elementCounter;
        }

        private static string ParseXamlColor(string color) {
            if (color.StartsWith("#")) {
                // Remove transparancy value
                color = "#" + color.Substring(3);
            }
            return color;
        }

        private static string ParseXamlThickness(string thickness) {
            string[] values = thickness.Split(',');

            for (int i = 0; i < values.Length; i++) {
                double value;
                if (double.TryParse(values[i], out value)) {
                    values[i] = Math.Ceiling(value).ToString();
                }
                else {
                    values[i] = "1";
                }
            }

            string cssThickness;
            switch (values.Length) {
                case 1:
                    cssThickness = thickness;
                    break;
                case 2:
                    cssThickness = values[1] + " " + values[0];
                    break;
                case 4:
                    cssThickness = values[1] + " " + values[2] + " " + values[3] + " " + values[0];
                    break;
                default:
                    cssThickness = values[0];
                    break;
            }

            return cssThickness;
        }

        /// <summary>
        /// Reads a content of current xaml element, converts it
        /// </summary>
        /// <param name="xamlReader">
        /// XmlTextReader which is expected to be at XmlNodeType.Element
        /// (opening element tag) position.
        /// </param>
        /// <param name="htmlWriter">
        /// May be null, in which case we are skipping the xaml element;
        /// witout producing any output to html.
        /// </param>
        /// <param name="inlineStyle">
        /// StringBuilder used for collecting css properties for inline STYLE attribute.
        /// </param>
        private static void WriteElementContent(XmlTextReader xamlReader, XmlTextWriter htmlWriter, StringBuilder inlineStyle) {
            Debug.Assert(xamlReader.NodeType == XmlNodeType.Element);

            bool elementContentStarted = false;

            if (xamlReader.IsEmptyElement) {
                if (htmlWriter != null && !elementContentStarted && inlineStyle.Length > 0) {
                    var stylePart = inlineStyle.ToString().Split(':');
                    // Output STYLE attribute and clear inlineStyle buffer.
                    int limit = stylePart.Length;
                    for (int i = 0; i < limit; i = i + 2)
                        htmlWriter.WriteAttributeString(stylePart[i], stylePart[i + 1]);
                    //htmlWriter.WriteAttributeString("STYLE", inlineStyle.ToString());
                    inlineStyle.Remove(0, inlineStyle.Length);
                }
                elementContentStarted = true;
            }
            else {
                while (ReadNextToken(xamlReader) && xamlReader.NodeType != XmlNodeType.EndElement) {
                    switch (xamlReader.NodeType) {
                        case XmlNodeType.Element:
                            if (xamlReader.Name.Contains(".")) {
                                AddComplexProperty(xamlReader, inlineStyle, htmlWriter);
                            }
                            else {
                                if (htmlWriter != null && !elementContentStarted && inlineStyle != null && inlineStyle.Length > 0) {
                                    // Output STYLE attribute and clear inlineStyle buffer.
                                    htmlWriter.WriteAttributeString("STYLE", inlineStyle.ToString());
                                    inlineStyle.Remove(0, inlineStyle.Length);
                                }
                                elementContentStarted = true;
                                WriteElement(xamlReader, htmlWriter, inlineStyle);
                            }
                            Debug.Assert(xamlReader.NodeType == XmlNodeType.EndElement || xamlReader.NodeType == XmlNodeType.Element && xamlReader.IsEmptyElement);
                            break;
                        case XmlNodeType.Comment:
                            if (htmlWriter != null) {
                                if (!elementContentStarted && inlineStyle.Length > 0) {
                                    htmlWriter.WriteAttributeString("style", inlineStyle.ToString());
                                }
                                htmlWriter.WriteComment(xamlReader.Value);
                            }
                            elementContentStarted = true;
                            break;
                        case XmlNodeType.CDATA:
                        case XmlNodeType.Text:
                        case XmlNodeType.SignificantWhitespace:
                            if (htmlWriter != null) {
                                if (!elementContentStarted && inlineStyle.Length > 0) {
                                    htmlWriter.WriteAttributeString("STYLE", inlineStyle.ToString());
                                }
                                var f = System.Net.WebUtility.HtmlEncode(xamlReader.Value);
                                htmlWriter.WriteString(f);
                            }
                            elementContentStarted = true;
                            break;
                    }
                }

                Debug.Assert(xamlReader.NodeType == XmlNodeType.EndElement);
            }
        }

        /// <summary>
        /// Conberts an element notation of complex property into
        /// </summary>
        /// <param name="xamlReader">
        /// On entry this XmlTextReader must be on Element start tag;
        /// on exit - on EndElement tag.
        /// </param>
        /// <param name="inlineStyle">
        /// StringBuilder containing a value for STYLE attribute.
        /// </param>
        private static void AddComplexProperty(XmlTextReader xamlReader, StringBuilder inlineStyle, XmlTextWriter htmlWriter) {
            Debug.Assert(xamlReader.NodeType == XmlNodeType.Element);

            if (inlineStyle != null && xamlReader.Name.EndsWith(".TextDecorations")) {
                inlineStyle.Append("text-decoration:underline;");
            }

            // Skip the element representing the complex property
            WriteElementContent(xamlReader, htmlWriter/*null:*/, inlineStyle/*:null*/);
        }

        /// <summary>
        /// Converts a xaml element into an appropriate html element.
        /// </summary>
        /// <param name="xamlReader">
        /// On entry this XmlTextReader must be on Element start tag;
        /// on exit - on EndElement tag.
        /// </param>
        /// <param name="htmlWriter">
        /// May be null, in which case we are skipping xaml content
        /// without producing any html output
        /// </param>
        /// <param name="inlineStyle">
        /// StringBuilder used for collecting css properties for inline STYLE attributes on every level.
        /// </param>
        private static void WriteElement(XmlTextReader xamlReader, XmlTextWriter htmlWriter, StringBuilder inlineStyle) {
            Debug.Assert(xamlReader.NodeType == XmlNodeType.Element);

            if (htmlWriter == null) {
                // Skipping mode; recurse into the xaml element without any output
                WriteElementContent(xamlReader, /*htmlWriter:*/null, null);
            }
            else {
                string htmlElementName = null;

                switch (xamlReader.Name) {
                    case "Run":
                    case "Span":
                        //htmlElementName = "SPAN";
                        htmlElementName = string.Empty;
                        break;
                    case "LineBreak":
                        //htmlElementName = "br";
                        htmlElementName = string.Empty;
                        htmlWriter.WriteStartElement("br");
                        htmlWriter.WriteEndElement();
                        break;
                    case "InlineUIContainer":
                        //htmlElementName = "SPAN";
                        htmlElementName = string.Empty;
                        //htmlElementName = "br";
                        break;
                    case "Bold":
                        htmlElementName = "b";
                        break;
                    case "Italic":
                        htmlElementName = "i";
                        break;
                    case "Paragraph":
                        //htmlElementName = "P";
                        //htmlWriter.WriteStartElement("br");
                        //htmlWriter.WriteEndElement();
                        htmlElementName = string.Empty;
                        break;
                    case "BlockUIContainer":
                        htmlElementName = string.Empty;
                        //htmlElementName = "DIV";
                        break;
                    case "Section":
                        //htmlElementName = "DIV";
                        htmlElementName = string.Empty;
                        break;
                    case "Table":
                        htmlElementName = "table";
                        break;
                    case "TableColumn":
                        //htmlElementName = "col";
                        htmlElementName = string.Empty;
                        break;
                    case "TableRowGroup":
                        //htmlElementName = "TBODY";
                        htmlElementName = string.Empty;
                        break;
                    case "TableRow":
                        htmlElementName = "tr";
                        break;
                    case "TableCell":
                        htmlElementName = "td";
                        break;
                    case "List":
                        string marker = xamlReader.GetAttribute("MarkerStyle");
                        if (marker == null || marker == "None" || marker == "Disc" || marker == "Circle" || marker == "Square" || marker == "Box") {
                            htmlElementName = "ul";
                        }
                        else {
                            htmlElementName = "ol";
                        }
                        break;
                    case "ListItem":
                        htmlElementName = "li";
                        break;
                    default:
                        htmlElementName = null; // Ignore the element
                        break;
                }

                if (htmlWriter != null && htmlElementName != null) {
                    if (htmlElementName != string.Empty) {
                        htmlWriter.WriteStartElement(htmlElementName);
                    }


                    var counter = WriteFormattingProperties(xamlReader, htmlWriter, inlineStyle);

                    WriteElementContent(xamlReader, htmlWriter, inlineStyle);
                    while (counter-- > 0) { htmlWriter.WriteEndElement(); }

                    if (htmlElementName != string.Empty) {
                        htmlWriter.WriteEndElement();
                    }
                }
                else {
                    // Skip this unrecognized xaml element
                    WriteElementContent(xamlReader, /*htmlWriter:*/null, null);
                }
            }
        }

        // Reader advance helpers
        // ----------------------

        /// <summary>
        /// Reads several items from xamlReader skipping all non-significant stuff.
        /// </summary>
        /// <param name="xamlReader">
        /// XmlTextReader from tokens are being read.
        /// </param>
        /// <returns>
        /// True if new token is available; false if end of stream reached.
        /// </returns>
        private static bool ReadNextToken(XmlReader xamlReader) {
            while (xamlReader.Read()) {
                Debug.Assert(xamlReader.ReadState == ReadState.Interactive, "Reader is expected to be in Interactive state (" + xamlReader.ReadState + ")");
                switch (xamlReader.NodeType) {
                    case XmlNodeType.Element:
                    case XmlNodeType.EndElement:
                    case XmlNodeType.None:
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Text:
                    case XmlNodeType.SignificantWhitespace:
                        return true;

                    case XmlNodeType.Whitespace:
                        if (xamlReader.XmlSpace == XmlSpace.Preserve) {
                            return true;
                        }
                        // ignore insignificant whitespace
                        break;

                    case XmlNodeType.EndEntity:
                    case XmlNodeType.EntityReference:
                        //  Implement entity reading
                        //xamlReader.ResolveEntity();
                        //xamlReader.Read();
                        //ReadChildNodes( parent, parentBaseUri, xamlReader, positionInfo);
                        break; // for now we ignore entities as insignificant stuff

                    case XmlNodeType.Comment:
                        return true;
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.DocumentType:
                    case XmlNodeType.XmlDeclaration:
                    default:
                        // Ignorable stuff
                        break;
                }
            }
            return false;
        }

        #endregion Private Methods

        // ---------------------------------------------------------------------
        //
        // Private Fields
        //
        // ---------------------------------------------------------------------

        #region Private Fields

        #endregion Private Fields
    }
}
