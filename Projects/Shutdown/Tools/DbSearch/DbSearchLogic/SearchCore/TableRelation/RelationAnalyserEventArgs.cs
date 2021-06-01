using System;

namespace DbSearchLogic.SearchCore.TableRelation {
    
    public class RelationAnalyserEventArgs : EventArgs {

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationAnalyserEventArgs"/> class.
        /// </summary>
        /// <param name="msg">The message.</param>
        public RelationAnalyserEventArgs(string msg) {
            this.Msg = msg;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationAnalyserEventArgs"/> class.
        /// </summary>
        /// <param name="relation">The relation.</param>
        public RelationAnalyserEventArgs(TableRelation relation) {
            this.Relation = relation;
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Msg { get; private set; }

        /// <summary>
        /// Gets or sets the relation.
        /// </summary>
        /// <value>The relation.</value>
        public TableRelation Relation { get; private set; }        
    }
}
