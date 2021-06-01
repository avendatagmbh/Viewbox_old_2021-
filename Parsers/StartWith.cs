
namespace Parsers
{
    #region StartWith

    [TagType(CTagName)]
    [MatchStartWithTag]
    internal class StartWith : SimpleTwoWordTag
    {
        #region Consts

        /// <summary>
        /// The name of the tag (its identifier).
        /// </summary>
        public const string CTagName = "START_WITH";

        /// <summary>
        /// The first part of tag.
        /// </summary>
        public const string CTagFirstPart = "START";

        /// <summary>
        /// The second part of tag.
        /// </summary>
        public const string CTagSecondPart = "WITH";

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

    #region MatchOrderByTagAttribute

    internal class MatchStartWithTagAttribute : MatchSimpleTwoWordTagAttribute
    {
        #region Properties

        /// <summary>
        /// Gets the name of the tag (its identifier and sql text)
        /// </summary>
        protected override string Name
        {
            get
            {
                return StartWith.CTagName;
            }
        }

        /// <summary>
        /// Gets the first word of the tag.
        /// </summary>
        protected override string FirstWord
        {
            get
            {
                return StartWith.CTagFirstPart;
            }
        }

        /// <summary>
        /// Gets the second word of the tag.
        /// </summary>
        protected override string SecondWord
        {
            get
            {
                return StartWith.CTagSecondPart;
            }
        }

        #endregion
    }

    #endregion
}
