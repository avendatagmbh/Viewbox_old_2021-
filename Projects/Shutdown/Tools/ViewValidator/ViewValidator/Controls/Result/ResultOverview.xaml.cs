using System.Windows;
using System.Windows.Controls;
using ViewValidator.Models.Result;
using ViewValidatorLogic.Structures.Results;

namespace ViewValidator.Controls.Result {
    /// <summary>
    /// Interaktionslogik für ResultOverview.xaml
    /// </summary>
    public partial class ResultOverview : UserControl {
        public ResultOverview() {
            InitializeComponent();
        }

        ValidationResults Results { get { return (DataContext as ResultOverviewModel).Results; } }

        public void NewResults() {
            this.mainPanel.Children.Clear();
            this.mainPanel.Children.Add(new ResultOverviewDetails() { DataContext = this.DataContext });

            int id = 0;
            foreach (var result in Results.TableValidationResults.Values) {
                if (result != null && result.TableMapping.Used) {
                    this.mainPanel.Children.Add(
                        new ResultOverviewTableDetails() {
                            DataContext = new ResultTableDetailsModel(result, id++)
                        }
                    );
                }
            }
            
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            
        }
    }
}
