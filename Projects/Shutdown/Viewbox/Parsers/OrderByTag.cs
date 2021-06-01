
namespace Parsers
{
    #region OrderByTag

    [TagType(CTagName)]
    [MatchOrderByTag]
    internal class OrderByTag : SimpleTwoWordTag
    {
        #region Consts

        /// <summary>
        /// The name of the tag (its identifier).
        /// </summary>
        public const string CTagName = "ORDER_BY";

        /// <summary>
        /// The first part of tag.
        /// </summary>
        public const string CTagFirstPart = "ORDER";

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

    #region MatchOrderByTagAttribute

    internal class MatchOrderByTagAttribute : MatchSimpleTwoWordTagAttribute
    {
        #region Properties

        /// <summary>
        /// Gets the name of the tag (its identifier and sql text)
        /// </summary>
        protected override string Name
        {
            get
            {
                return OrderByTag.CTagName;
            }
        }

        /// <summary>
        /// Gets the first word of the tag.
        /// </summary>
        protected override string FirstWord
        {
            get
            {
                return OrderByTag.CTagFirstPart;
            }
        }

        /// <summary>
        /// Gets the second word of the tag.
        /// </summary>
        protected override string SecondWord
        {
            get
            {
                return OrderByTag.CTagSecondPart;
            }
        }

        #endregion
    }

    #endregion
}
