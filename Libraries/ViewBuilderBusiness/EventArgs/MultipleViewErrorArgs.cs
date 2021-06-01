using System.Collections.Generic;

namespace ViewBuilderBusiness.EventArgs
{
    public class MultipleViewErrorArgs : System.EventArgs
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="MultipleViewErrorArgs" /> class.
        /// </summary>
        /// <param name="view"> The view. </param>
        /// <param name="scriptfiles"> The scriptfiles. </param>
        public MultipleViewErrorArgs(string view, List<string> scriptfiles)
        {
            View = view;
            Scriptfiles = scriptfiles;
        }

        /// <summary>
        ///   Gets or sets the view.
        /// </summary>
        /// <value> The view. </value>
        public string View { get; set; }

        /// <summary>
        ///   Gets or sets the scriptfiles.
        /// </summary>
        /// <value> The scriptfiles. </value>
        public List<string> Scriptfiles { get; set; }

        /// <summary>
        ///   Gets the view display string.
        /// </summary>
        /// <value> The view display string. </value>
        public string ViewDisplayString
        {
            get { return "View: " + View; }
        }
    }
}