
namespace Parsers
{
    #region GroupByTag

    [TagType(CTagName)]
    [MatchGroupByTag]
    internal class GroupByTag : SimpleTwoWordTag
    {
        #region Consts

        /// <summary>
        /// The name of the tag (its identifier).
        /// </summary>
        public const string CTagName = "GROUP_BY";

        /// <summary>
        /// The first part of tag.
        /// </summary>
        public const string CTagFirstPart = "GROUP";

        /// <summary>
        /// The second part of tag.
        /// </summary>
        public const string CTagSecondPart = "BY";

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

    #region MatchGroupByTagAttribute

    internal class MatchGroupByTagAttribute : MatchSimpleTwoWordTagAttribute
    {
        #region Properties

        /// <summary>
        /// Gets the name of the tag (its identifier and sql text)
        /// </summary>
        protected override string Name
        {
            get
            {
                return GroupByTag.CTagName;
            }
        }

        /// <summary>
        /// Gets the first word of the tag.
        /// </summary>
        protected override string FirstWord
        {
            get
            {
                return GroupByTag.CTagFirstPart;
            }
        }

        /// <summary>
        /// Gets the second word of the tag.
        /// </summary>
        protected override string SecondWord
        {
            get
            {
                return GroupByTag.CTagSecondPart;
            }
        }

        #endregion
    }

    #endregion
}
