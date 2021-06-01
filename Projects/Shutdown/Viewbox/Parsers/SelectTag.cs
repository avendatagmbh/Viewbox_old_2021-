
namespace Parsers
{
    #region SelectTag

    [TagType(CTagName)]
    [MatchSelectTag]
    internal class SelectTag : SimpleOneWordTag
    {
        #region Consts

        /// <summary>
        /// The name of the tag (its identifier).
        /// </summary>
        public const string CTagName = "SELECT";

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

    #region MatchSelectTagAttribute

    internal class MatchSelectTagAttribute : MatchSimpleOneWordTagAttribute
    {
        #region Properties

        /// <summary>
        /// Gets the name of the tag (its identifier and sql text)
        /// </summary>
        protected override string Name
        {
            get
            {
                return SelectTag.CTagName;
            }
        }

        #endregion
    }

    #endregion
}
