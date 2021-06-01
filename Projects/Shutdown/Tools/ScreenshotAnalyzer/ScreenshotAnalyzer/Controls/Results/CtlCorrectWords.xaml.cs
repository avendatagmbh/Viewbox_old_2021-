using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ScreenshotAnalyzer.Models.Results;
using Utils;

namespace ScreenshotAnalyzer.Controls.Results {
    /// <summary>
    /// Interaktionslogik für CtlCorrectWords.xaml
    /// </summary>
    public partial class CtlCorrectWords : UserControl {
        public CtlCorrectWords() {
            InitializeComponent();
            txtEditedText.TextChanged += new TextChangedEventHandler(txtEditedText_TextChanged);
        }

        void txtEditedText_TextChanged(object sender, TextChangedEventArgs e) {
            if (_firstTimeTextChange) {
                SetEditTextProperties();
            }
            _firstTimeTextChange = false;
        }

        private bool _firstTimeTextChange = true;
        CorrectionAssistantModel Model { get { return DataContext as CorrectionAssistantModel; } }

        private void btnNext_Click(object sender, RoutedEventArgs e) {
            Next();
        }

        private void Next() {
            txtEditedText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            if (Model.IsAtEnd)
                UIHelpers.TryFindParent<Window>(this).Close();
            else {
                Model.ShowNext();
                SetEditTextProperties();
            }
        }

        private void SetEditTextProperties() {
            txtEditedText.CaretIndex = txtEditedText.Text.Length;
            txtEditedText.Focus();
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e) {
            txtEditedText.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            if (Model.IsNotAtBeginning)
                Model.ShowPrevious();
        }

        private void txtEditedText_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Return && (Keyboard.Modifiers == ModifierKeys.Control)) {
                int caretIndex = txtEditedText.CaretIndex;
                Model.CurrentRecognitionInfo.ResultRowEntry.EditedText = Model.CurrentRecognitionInfo.ResultRowEntry.EditedText.Insert(caretIndex, Environment.NewLine);
                txtEditedText.CaretIndex = caretIndex + 1;
                e.Handled = true;
            } else if (e.Key == Key.Return && (Keyboard.Modifiers == ModifierKeys.None)) {
                Next();
                e.Handled = true;
            }

        }
    }
}
