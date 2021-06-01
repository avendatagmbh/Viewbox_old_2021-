
namespace Parsers
{
    #region FromTag

    [TagType(CTagName)]
    [MatchFromTag]
    internal class FromTag : SimpleOneWordTag
    {
        #region Consts

        /// <summary>
        /// The name of the tag (its identifier).
        /// </summary>
        public const string CTagName = "FROM";

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

    #region MatchFromTagAttribute

    internal class MatchFromTagAttribute : MatchSimpleOneWordTagAttribute
    {
        #region Properties

        /// <summary>
        /// Gets the name of the tag (its identifier and sql text)
        /// </summary>
        protected override string Name
        {
            get
            {
                return FromTag.CTagName;
            }
        }

        #endregion
    }

    #endregion
}
