using System.Windows.Controls;
using System.Windows.Input;

namespace AvdWpfControls
{
    public class AvdTreeView : TreeView
    {
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if ((e.Key == Key.Add || e.Key == Key.Subtract))
            {
                if (e.OriginalSource is NumericTextbox)
                {
                    if (e.Key == Key.Subtract)
                    {
                        var textBox = (TextBox) e.OriginalSource;
                        int caretIndex = textBox.CaretIndex;
                        if ((caretIndex == 0 && !textBox.Text.Contains("-") ||
                             (textBox.SelectionStart == 0 && textBox.SelectedText.Contains("-"))))
                        {
                            textBox.SelectedText = "";
                            textBox.Text = textBox.Text.Insert(caretIndex, "-");
                            textBox.CaretIndex = caretIndex + 1;
                        }
                    }
                }
                else if (e.OriginalSource is TextBox)
                {
                    if (e.Key == Key.Subtract)
                    {
                        var textBox = (TextBox) e.OriginalSource;
                        int caretIndex = textBox.CaretIndex;
                        textBox.SelectedText = "";
                        textBox.Text = textBox.Text.Insert(caretIndex, e.Key == Key.Add ? "+" : "-");
                        textBox.CaretIndex = caretIndex + 1;
                    }
                }
                e.Handled = true;
            }
            else
            {
                base.OnPreviewKeyDown(e);
            }
        }
    }
}