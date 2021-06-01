
namespace Parsers
{
    #region WhereTag

    [TagType(CTagName)]
    [MatchWhereTag]
    internal class WhereTag : SimpleOneWordTag
    {
        #region Consts

        /// <summary>
        /// The name of the tag (its identifier).
        /// </summary>
        public const string CTagName = "WHERE";

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the tag (its identifier and sql text)
        /// </summary>
        protected override string Name
        {
            get
            {
                return CTagName;
            }
        }

        #endregion
    }

    #endregion

    #region MatchWhereTagAttribute

    internal class MatchWhereTagAttribute : MatchSimpleOneWordTagAttribute
    {
        #region Properties

        /// <summary>
        /// Gets the name of the tag (its identifier and sql text)
        /// </summary>
        protected override string Name
        {
            get
            {
                return WhereTag.CTagName;
            }
        }

        #endregion
    }

    #endregion
}
