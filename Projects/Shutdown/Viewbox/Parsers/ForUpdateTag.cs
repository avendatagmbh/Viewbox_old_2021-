
namespace Parsers
{
    #region ForUpdateTag

    [TagType(CTagName)]
    [MatchForUpdateTag]
    internal class ForUpdateTag : SimpleTwoWordTag
    {
        #region Consts

        /// <summary>
        /// The name of the tag (its identifier).
        /// </summary>
        public const string CTagName = "FOR_UPDATE";

        /// <summary>
        /// The first part of tag.
        /// </summary>
        public const string CTagFirstPart = "FOR";

        /// <summary>
        /// The second part of tag.
        /// </summary>
        public const string CTagSecondPart = "UPDATE";

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the tag.
        /// </summary>
        protected override string Name
        {
            get
            {
                return CTagName;
            }
        }

        /// <summary>
        /// Gets the first word of the tag.
        /// </summary>
        protected override string FirstWord
        {
            get
            {
                return CTagFirstPart;
            }
        }

        /// <summary>
        /// Gets the second word of the tag.
        /// </summary>
        protected override string SecondWord
        {
            get
            {
                return CTagSecondPart;
            }
        }

        #endregion
    }

    #endregion

    #region MatchForUpdateTagAttribute

    internal class MatchForUpdateTagAttribute : MatchSimpleTwoWordTagAttribute
    {
        #region Properties

        /// <summary>
        /// Gets the name of the tag (its identifier and sql text)
        /// </summary>
        protected override string Name
        {
            get
            {
                return ForUpdateTag.CTagName;
            }
        }

        /// <summary>
        /// Gets the first word of the tag.
        /// </summary>
        protected override string FirstWord
        {
            get
            {
                return ForUpdateTag.CTagFirstPart;
            }
        }

        /// <summary>
        /// Gets the second word of the tag.
        /// </summary>
        protected override string SecondWord
        {
            get
            {
                return ForUpdateTag.CTagSecondPart;
            }
        }

        #endregion
    }

    #endregion
}
