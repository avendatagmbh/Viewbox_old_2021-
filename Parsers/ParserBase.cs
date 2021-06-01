using System;
using System.Linq;
using System.Text;
using System.Xml;

namespace Parsers
{
    #region ParserBase

    /// <summary>
    /// An abstract class which is used as the base class for parsers.
    /// It contains basic methods which allow you to create a logical tree
    /// from a document.
    /// </summary>
    public abstract class ParserBase
    {
        #region Consts

        /// <summary>
        /// The name of the Value xml attribute.
        /// </summary>
        protected const string CValueXmlAttributeName = "Value";

        /// <summary>
        /// The name of the Tag Type xml attribute.
        /// </summary>
        protected const string CTagTypeXmlAttributeName = "Type";

        /// <summary>
        /// The name of the root node in the xml representation of a parsed text.
        /// </summary>
        protected const string CRootXmlNodeName = "ParsedDocument";

        /// <summary>
        /// The name of the Tag xml node.
        /// </summary>
        protected const string CTagXmlNodeName = "Tag";

        /// <summary>
        /// The name of the Text xml node.
        /// </summary>
        protected const string CTextXmlNodeName = "Text";

        /// <summary>
        /// The white space text (is used when adding white 
        /// spaces between text elements).
        /// </summary>
        public const string CWhiteSpace = " ";

        /// <summary>
        /// The new line string.
        /// </summary>
        public const string CNewLine = "\r\n";

        #endregion

        #region Fields

        /// <summary>
        /// The tree representation of a parsed document as an xml document.
        /// </summary>
        private XmlDocument _fParsedDocument;

        #endregion

        #region Methods

        /// <summary>
        /// Parses the specified text.
        /// </summary>
        public void Parse(string text)
        {
            #region Check the arguments

            if (text == null)
                throw new ArgumentNullException("text");

            #endregion

            _fParsedDocument = new XmlDocument();
            XmlNode myRootNode = _fParsedDocument.CreateElement(CRootXmlNodeName);
            _fParsedDocument.AppendChild(myRootNode);

            try
            {
                ParseBlock(myRootNode, null, text, 0);
            }
            catch
            {
                _fParsedDocument = null;
                throw;
            }

            // m_ParsedDocument.Save(@"D:\Temp\ParserTest.xml");
        }

        /// <summary>
        /// Parses the specified block of a text.
        /// </summary>
        /// <returns>
        /// Returns the end position of the parsed block.
        /// </returns>
        protected int ParseBlock(XmlNode parentNode, TagBase parentTag, string text, int position)
        {
            #region Check the arguments

            if (parentNode == null)
                throw new ArgumentNullException("parentNode");

            CheckTextAndPositionArguments(text, position);

            #endregion

            while (position < text.Length)
            {
                if (IsSkipWhiteSpace)
                    SkipWhiteSpace(text, ref position);

                if (position == text.Length)
                    break;

                #region Read the parent tag ending

                if (parentTag != null)
                {
                    int myParentTagEndingEndPosition = parentTag.MatchEnd(text, position);
                    if (myParentTagEndingEndPosition >= 0)
                    {
                        position = myParentTagEndingEndPosition;
                        return position;
                    }
                }

                #endregion

                Type myTagType = IsTag(text, position);
                if (myTagType != null)
                {
                    #region Read a tag

                    #region Create the tag class instance

                    var myTag = Activator.CreateInstance(myTagType) as TagBase;
                    position = (myTag == null) ? 0 : myTag.InitializeFromText(this, text, position, parentTag);

                    #endregion

                    #region Create an xml node for the tag

                    XmlNode myTagXmlNode = CreateTagXmlNode(myTag);
                    parentNode.AppendChild(myTagXmlNode);

                    #endregion

                    if (myTag == null || myTag.HasContents)
                        position = ParseBlock(myTagXmlNode, myTag, text, position);

                    #endregion
                }
                else
                {
                    #region Read text

                    string myText = ReadWordOrSeparator(text, ref position, !IsSkipWhiteSpace);
                    parentNode.AppendChild(CreateTextXmlNode(myText));

                    #endregion
                }
            }

            if (parentTag != null && !parentTag.CanTerminateByStringEnd)
                throw new Exception("Invalid format");

            return position;
        }

        /// <summary>
        /// Checks whether there is a tag in the text at the specified position, and returns its type.
        /// </summary>
        protected Type IsTag(string text, int position)
        {
            return (from myTagType in Tags let myMatchTagAttribute = TagBase.GetTagMatchAttribute(myTagType) where myMatchTagAttribute.Match(text, position) select myTagType).FirstOrDefault();
        }

        /// <summary>
        /// Creates an xml node for the specified tag.
        /// </summary>
        protected XmlNode CreateTagXmlNode(TagBase tag)
        {
            #region Check the arguments

            if (tag == null)
                throw new ArgumentNullException();

            #endregion

            CheckXmlDocInitialized();

            XmlElement myTagNode = _fParsedDocument.CreateElement(CTagXmlNodeName);

            XmlAttribute myTypeAttribute = _fParsedDocument.CreateAttribute(CTagTypeXmlAttributeName);
            myTypeAttribute.Value = tag.Type;
            myTagNode.Attributes.Append(myTypeAttribute);

            if (tag.Value != null)
            {
                XmlAttribute myValueAttribute = _fParsedDocument.CreateAttribute(CValueXmlAttributeName);
                myValueAttribute.Value = tag.Value;
                myTagNode.Attributes.Append(myValueAttribute);
            }

            return myTagNode;
        }

        /// <summary>
        /// Creates an xml node for the specified text.
        /// </summary>
        protected XmlNode CreateTextXmlNode(string text)
        {
            #region Check the arguments

            if (text == null)
                throw new ArgumentNullException("text");

            #endregion

            CheckXmlDocInitialized();

            XmlElement myTextNode = _fParsedDocument.CreateElement(CTextXmlNodeName);

            XmlAttribute myValueAttribute = _fParsedDocument.CreateAttribute(CValueXmlAttributeName);
            myValueAttribute.Value = text;
            myTextNode.Attributes.Append(myValueAttribute);

            return myTextNode;
        }

        /// <summary>
        /// Skips the white space symbols located at the specified position.
        /// </summary>
        public static void SkipWhiteSpace(string text, ref int position)
        {
            #region Check the parameters

            CheckTextAndPositionArguments(text, position);

            #endregion

            while (position < text.Length)
            {
                if (!char.IsWhiteSpace(text, position))
                    break;
                position++;
            }
        }

        /// <summary>
        /// Reads a single word or separator at the specified position.
        /// </summary>
        public static string ReadWordOrSeparator(string text, ref int position, bool treatWhiteSpaceAsSeparator)
        {
            #region Check the parameters

            CheckTextAndPositionArguments(text, position);

            #endregion

            int myStartPosition = position;

            while (position < text.Length)
            {
                #region Check is white space

                if (char.IsWhiteSpace(text, position))
                {
                    if (position == myStartPosition && treatWhiteSpaceAsSeparator)
                    {
                        if (position + CNewLine.Length <= text.Length && text.Substring(position, CNewLine.Length) == CNewLine)
                            position += CNewLine.Length;
                        else
                            position += 1;
                    }
                    break;
                }

                #endregion

                #region Check is separator

                if (!char.IsLetterOrDigit(text, position) && text[position] != '_')
                {
                    if (position == myStartPosition)
                        position++;
                    break;
                }

                #endregion

                position++;
            }

            if (position == myStartPosition)
                return string.Empty;

            return text.Substring(myStartPosition, position - myStartPosition);
        }

        /// <summary>
        /// Checks the text and position parameters.
        /// </summary>
        public static void CheckTextAndPositionArguments(string text, int position)
        {
            #region Check the parameters

            if (text == null)
                throw new ArgumentNullException("text");

            if (position < 0 || position >= text.Length)
                throw new ArgumentOutOfRangeException("position");

            #endregion
        }

        /// <summary>
        /// Checks whether the m_ParsedDocument is initialized.
        /// </summary>
        protected void CheckXmlDocInitialized()
        {
            if (_fParsedDocument == null)
                throw new Exception("Xml document is not initialized");
        }

        /// <summary>
        /// Returns the text string processed by the parser.
        /// </summary>
        public string ToText()
        {
            CheckXmlDocInitialized();

            var myStringBuilder = new StringBuilder();

            XmlNodesToText(myStringBuilder, _fParsedDocument.ChildNodes[0].ChildNodes);

            return myStringBuilder.ToString();
        }

        /// <summary>
        /// Converts the specified xml node collection to the text.
        /// </summary>
        protected void XmlNodesToText(StringBuilder output, XmlNodeList nodes)
        {
            #region Check arguments

            if (nodes == null)
                throw new ArgumentNullException("nodes");

            if (output == null)
                throw new ArgumentNullException("output");

            #endregion

            foreach (XmlNode myNode in nodes)
                XmlNodeToText(output, myNode);
        }

        /// <summary>
        /// Converts the specified xml node to text.
        /// </summary>
        protected void XmlNodeToText(StringBuilder output, XmlNode node)
        {
            #region Check arguments

            if (node == null)
                throw new ArgumentNullException("node");

            if (output == null)
                throw new ArgumentNullException("output");

            #endregion

            if (IsTagXmlNode(node))
            {
                TagXmlNodeToText(output, node);
            }
            else if (IsTextXmlNode(node))
            {
                output.Append(GetTextNodeText(node));

                if (IsSkipWhiteSpace)
                    output.Append(CWhiteSpace);
            }
            else
            {
                throw new Exception("Unrecognized xml node.");
            }

            //m_ParsedDocument.Save(@"D:\Temp\testparser.xml");
        }

        /// <summary>
        /// Determines whether the specified node is a tag node.
        /// </summary>
        protected bool IsTagXmlNode(XmlNode node)
        {
            #region Check arguments

            if (node == null)
                throw new ArgumentNullException("node");

            #endregion

            return node.Name == CTagXmlNodeName;
        }

        /// <summary>
        /// Determines whether the specified node is a text node.
        /// </summary>
        protected bool IsTextXmlNode(XmlNode node)
        {
            #region Check arguments

            if (node == null)
                throw new ArgumentNullException("node");

            #endregion

            return node.Name == CTextXmlNodeName;
        }

        /// <summary>
        /// Returns the text stored in the text node.
        /// </summary>
        protected string GetTextNodeText(XmlNode node)
        {
            string myValue = GetXmlNodeValue(node);
            if (myValue == null)
                throw new Exception("Cannot get text node's value.");

            return myValue;
        }

        /// <summary>
        /// Retrieves the value of the specified tag xml node.
        /// </summary>
        protected string GetXmlNodeValue(XmlNode node)
        {
            #region Check arguments

            if (node == null || node.Attributes == null)
                throw new ArgumentNullException("node");

            #endregion

            XmlAttribute myValueXmlAttribute = node.Attributes[CValueXmlAttributeName];
            if (myValueXmlAttribute == null)
                return null;

            return myValueXmlAttribute.Value;
        }

        /// <summary>
        /// Converts the specified tag xml node to text.
        /// </summary>
        private void TagXmlNodeToText(StringBuilder output, XmlNode node)
        {
            #region Check arguments

            if (node == null)
                throw new ArgumentNullException("node");

            if (output == null)
                throw new ArgumentNullException("output");

            #endregion

            TagBase myTag = TagXmlNodeToTag(node);

            myTag.WriteStart(output);

            if (IsSkipWhiteSpace)
                output.Append(CWhiteSpace);

            if (node.ChildNodes.Count > 0)
                XmlNodesToText(output, node.ChildNodes);

            myTag.WriteEnd(output);

            if (IsSkipWhiteSpace)
                output.Append(CWhiteSpace);
        }

        /// <summary>
        /// Converts the specified tag xml node into a tag.
        /// </summary>
        protected TagBase TagXmlNodeToTag(XmlNode node)
        {
            #region Check arguments

            if (node == null || node.Attributes == null)
                throw new ArgumentNullException("node");

            #endregion

            #region Get tag parameters

            XmlAttribute myTagTypeXmlAttribute = node.Attributes[CTagTypeXmlAttributeName];
            if (myTagTypeXmlAttribute == null)
                throw new Exception("Cannot find the tag type attribute");
            string myTagType = myTagTypeXmlAttribute.Value;

            XmlAttribute myValueXmlAttribute = node.Attributes[CValueXmlAttributeName];
            string myTagValue = myValueXmlAttribute == null ? null : myValueXmlAttribute.Value;

            #endregion

            return GetTagFromType(myTagType, myTagValue, node.ChildNodes.Count > 0);
        }

        /// <summary>
        /// Retrurns the tag instance from its type and value.
        /// </summary>
        protected TagBase GetTagFromType(string type, string value, bool hasContents)
        {
            foreach (Type myTagType in Tags)
            {
                string myType = TagBase.GetTagType(myTagType);
                if (String.Compare(myType, type, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var myTag = Activator.CreateInstance(myTagType) as TagBase;
                    if (myTag != null)
                    {
                        myTag.InitializeFromData(this, value, hasContents);
                        return myTag;
                    }
                }
            }

            throw new Exception(string.Format("Tag cannot be found: {0}", type));
        }

        /// <summary>
        /// Removes all the sub nodes from the specified node. 
        /// </summary>
        protected static void ClearXmlNode(XmlNode node)
        {
            #region Check arguments

            if (node == null)
                throw new ArgumentNullException("node");

            #endregion

            for (int myChildIndex = node.ChildNodes.Count - 1; myChildIndex >= 0; myChildIndex--)
                node.RemoveChild(node.ChildNodes[myChildIndex]);
        }

        /// <summary>
        /// Returns how many continuous specified strings are before of the specified
        /// position in the specified text
        /// </summary>
        public static int CountStringsBefore(string text, int position, string @string)
        {
            CheckTextAndPositionArguments(text, position);

            #region Count the Strings before

            int myStringsBeforeCount = 0;
            int myPrevPosition = position - @string.Length;
            while (myPrevPosition >= 0)
            {
                // If it is not the specified string prefix
                if (string.Compare(text, myPrevPosition, @string, 0, @string.Length, true) != 0)
                    break;

                myStringsBeforeCount++;
                myPrevPosition -= @string.Length;
            }

            #endregion

            return myStringsBeforeCount;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Indicates whether the white space is considered as a non-valuable
        /// character.
        /// </summary>
        protected abstract bool IsSkipWhiteSpace { get; }

        /// <summary>
        /// The list of all available tags.
        /// </summary>
        protected abstract Type[] Tags { get; }

        /// <summary>
        /// Gets the parsed document in xml format.
        /// </summary>
        protected XmlDocument ParsedDocument
        {
            get
            {
                CheckXmlDocInitialized();

                return _fParsedDocument;
            }
        }

        #endregion
    }

    #endregion
}
