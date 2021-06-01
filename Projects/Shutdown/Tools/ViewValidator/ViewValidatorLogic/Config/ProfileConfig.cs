/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-11      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using AvdCommon.Rules;
using ViewValidatorLogic.Structures.InitialSetup;
using ViewValidatorLogic.Structures.Misc;

namespace ViewValidatorLogic.Config {

    /// <summary>
    /// Profile configuration.
    /// </summary>
    public class ProfileConfig : INotifyPropertyChanged {

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileConfig"/> class.
        /// </summary>
        public ProfileConfig() {
            this.Name = "ProfileName";
            this.Description = "keine Profilbeschreibung eingegeben";
            this.ValidationSetup = new ValidationSetup();
            CustomRules = new RuleSet();
            //this.DbConfig = new ConfigDatabase();
        }

        /*****************************************************************************************************/

        #region events

        /// <summary>
        /// Occurs when an error occured.
        /// </summary>
        public event EventHandler<MessageEventArgs> Error;

        /// <summary>
        /// Occurs when a property changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion events

        /*****************************************************************************************************/

        #region eventTrigger

        /// <summary>
        /// Called when an error occured.
        /// </summary>
        private void OnError(string message) {
            if (Error != null) Error(this, new MessageEventArgs(message));
        }

        /// <summary>
        /// Called when a property changed.
        /// </summary>
        /// <param name="property">The property.</param>
        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));            
        }

        #endregion eventTrigger

        /*****************************************************************************************************/

        #region fields


        #endregion fields

        /*****************************************************************************************************/

        #region persistent properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        private string _name;
        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    foreach (var character in Path.GetInvalidFileNameChars())
                        if (value.Contains(character)) throw new ArgumentException("Der Name darf keines der folgenden Zeichen enthalten: " + string.Join("", Path.GetInvalidPathChars()));
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        private string _description;
        public string Description {
            get { return _description; }
            set {
                if (_description != value) {
                    _description = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        public ValidationSetup ValidationSetup { get; set; }
        public string DbConfig { get { if (ValidationSetup.DbConfigView == null) return ""; else return ValidationSetup.DbConfigView.DbName; } }
        public RuleSet CustomRules { get; set; }
        #endregion persistent properties

        /*****************************************************************************************************/

        #region non persistent properties

        public string DisplayString { get { return Name; } }
        #endregion non persistent properties

        /*****************************************************************************************************/

        #region methods


        #endregion methods
    }
}
