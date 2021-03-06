using System;
using System.Text;

namespace Parsers
{
    #region BracesTag

    [TagType(CTagName)]
    [MatchBracesTag]
    internal class BracesTag : TagBase
    {
        #region Consts

        /// <summary>
        /// The name of the tag (its identifier).
        /// </summary>
        public const string CTagName = "BRACES";

        /// <summary>
        /// The start of the tag.
        /// </summary>
        public const string CStartTag = "(";

        /// <summary>
        /// The end of the tag.
        /// </summary>
        public const string CEndTag = ")";

        #endregion

        #region Methods

        #region Common

        /// <summary>
        /// Reads the tag at the specified position in the specified word and separator array.
        /// </summary>
        /// <returns>
        /// The position after the tag (at which to continue reading).
        /// </returns>
        protected override int InitializeCoreFromText(ParserBase parser, string sql, int position, TagBase parentTag)
        {
            #region Check the arguments

            ParserBase.CheckTextAndPositionArguments(sql, position);

            #endregion

            int myResult = MatchStartStatic(sql, position);

            if (myResult < 0)
                throw new Exception("Cannot read the Braces tag.");

            Parser = parser;

            HasContents = true;

            return myResult;
        }

        /// <summary>
        /// Returns a value indicating whether there is the tag ending at the specified position.		/// 
        /// </summary>
        /// <returns>
        /// If this value is less than zero, then there is no ending; otherwise the 
        /// position after ending is returned.
        /// </returns>
        public override int MatchEnd(string sql, int position)
        {
            CheckInitialized();

            #region Check the arguments

            ParserBase.CheckTextAndPositionArguments(sql, position);

            #endregion

            return MatchEndStatic(sql, position);
        }

        /// <summary>
        /// Writes the start of the tag.
        /// </summary>
        public override void WriteStart(StringBuilder output)
        {
            CheckInitialized();

            #region Check the parameters

            if (output == null)
                throw new ArgumentNullException();

            #endregion

            output.Append(CStartTag);
        }

        /// <summary>
        /// Writes the end of the tag.
        /// </summary>
        public override void WriteEnd(StringBuilder output)
        {
            CheckInitialized();

            #region Check the parameters

            if (output == null)
                throw new ArgumentNullException();

            #endregion

            output.Append(CEndTag);
        }

        #endregion

        #region Static

        /// <summary>
        /// Checks whether there is the tag start at the specified position 
        /// in the specified sql.
        /// </summary>
        /// <returns>
        /// The position after the tag or -1 there is no tag start at the position.
        /// </returns>
        public static int MatchStartStatic(string sql, int position)
        {
            #region Check the arguments

            ParserBase.CheckTextAndPositionArguments(sql, position);

            #endregion

            if (string.Compare(sql, position, CStartTag, 0, CStartTag.Length, true) != 0)
                return -1;

            return position + CStartTag.Length;
        }

        /// <summary>
        /// Checks whether there is the tag end at the specified position 
        /// in the specified sql.
        /// </summary>
        /// <returns>
        /// The position after the tag or -1 there is no tag end at the position.
        /// </returns>
        public static int MatchEndStatic(string sql, int position)
        {
            #region Check the arguments

            ParserBase.CheckTextAndPositionArguments(sql, position);

            #endregion

            if (string.Compare(sql, position, CEndTag, 0, CEndTag.Length, true) != 0)
                return -1;

            return position + CEndTag.Length;
        }

        #endregion

        #endregion
    }

    #endregion

    #region MatchBracesTagAttribute

    internal class MatchBracesTagAttribute : MatchTagAttributeBase
    {
        #region Methods

        public override bool Match(string sql, int position)
        {
            return BracesTag.MatchStartStatic(sql, position) >= 0;
        }

        #endregion
    }

    #endregion
}
