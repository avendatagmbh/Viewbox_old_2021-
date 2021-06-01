// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-10-09
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using eBalanceKitBusiness.RtfHtmlConverter;

namespace eBalanceKit.Controls.RichTextEditor {
    public static class RichTextboxAssistant {
        public static readonly DependencyProperty BoundDocument =
           DependencyProperty.RegisterAttached("BoundDocument", typeof(string), typeof(RichTextboxAssistant),
           new FrameworkPropertyMetadata(null,
               FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
               OnBoundDocumentChanged)
               );

        public static readonly DependencyProperty Html =
           DependencyProperty.RegisterAttached("HtmlContent", typeof(string), typeof(RichTextboxAssistant),
           new FrameworkPropertyMetadata(null,
               FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
               OnBoundDocumentChanged)
               );


        public static System.Windows.Input.ICommand CommandLoading = new Utils.Commands.DelegateCommand(o => true, LoadRtfDocument );
        
        private static void LoadRtfDocument(object o) { 

            RichTextBox box = o as RichTextBox;

            if (box == null)
                return;

            var dlg = new Microsoft.Win32.OpenFileDialog();
                var r = dlg.ShowDialog();

                if (r.HasValue && r.Value) {
                    string _htmlText;
                    DataCommunicator.LoadRtf(box.Document, dlg.FileName, out _htmlText);
                }
            FireContentChanged(o, null);
        }

        private static void OnBoundDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            UpdateRichTextBox(d);
        }

        public static void UpdateRichTextBox(DependencyObject d) {
            RichTextBox box = d as RichTextBox;

            if (box == null)
                return;

            RemoveEventHandler(box);

            //string newXAML = GetBoundDocument(d);

            var html = d.GetValue(BoundDocument) as string;
            var xaml = string.Empty;

            if (!string.IsNullOrEmpty(html))
                xaml = DataCommunicator.GetXaml(html);

            //return xaml;

            //if (!newXAML.StartsWith("<FlowDocument>")) {
            //    newXAML = "<FlowDocument>" + newXAML + "</FlowDocument>";
            //}

            DataCommunicator.LoadXamlIntoDocument(box.Document, xaml);

            AttachEventHandler(box);
        }

        private static void RemoveEventHandler(RichTextBox box) {
            Binding binding = BindingOperations.GetBinding(box, BoundDocument);

            if (binding != null) {
                //box.TextChanged -= HandleTextChanged;
                if (binding.UpdateSourceTrigger == UpdateSourceTrigger.Default ||
                    binding.UpdateSourceTrigger == UpdateSourceTrigger.LostFocus) {

                    box.LostFocus -= HandleLostFocus;
                }
                else {
                    //box.TextChanged -= HandleTextChanged;
                    box.TextChanged -= HandleTextChanged;
                }
                //DataObject.RemovePastingHandler(box, FireContentChanged);
            }
        }

        private static void AttachEventHandler(RichTextBox box) {
            Binding binding = BindingOperations.GetBinding(box, BoundDocument);

            if (binding != null) {
                //box.TextChanged += HandleTextChanged;
                if (binding.UpdateSourceTrigger == UpdateSourceTrigger.Default ||
                    binding.UpdateSourceTrigger == UpdateSourceTrigger.LostFocus) {

                    box.LostFocus += HandleLostFocus;
                }
                else {
                    box.TextChanged += HandleTextChanged;
                }
                //DataObject.AddPastingHandler(box, delegate(object sender, DataObjectPastingEventArgs args) { FireContentChanged(sender, args); });
            }
        }



        private static void HandleLostFocus(object sender, RoutedEventArgs e) {
            //RichTextBox box = sender as RichTextBox;
            //var doc = box.Document;
            //TextRange tr = new TextRange(doc.ContentStart, doc.ContentEnd);

            //using (MemoryStream ms = new MemoryStream()) {
            //    tr.Save(ms, DataFormats.Xaml);
            //    string xamlText = Encoding.UTF8.GetString(ms.ToArray());
            //    SetBoundDocument(box, xamlText);
            //}
            FireContentChanged(sender, e);
            //FireContentChanged(sender, e);
        }

        private static void HandleTextChanged(object sender, RoutedEventArgs e) {

            //// TO_DO: TextChanged is currently not working!
            RichTextBox box = sender as RichTextBox;
            if (box == null) {
                return;
            }
            var c = box.Cursor;
            var p = box.CaretPosition;
            
            var s = box.Selection.Start;
            TextPointer moveTo = box.CaretPosition.GetNextInsertionPosition(LogicalDirection.Forward);
            TextPointer tp = box.CaretPosition;

            tp = tp.GetNextInsertionPosition(LogicalDirection.Forward);
            if (tp == null) {
                tp = box.CaretPosition.GetNextContextPosition(LogicalDirection.Forward);
            }
            FireContentChanged(sender, e);
            //box.CaretPosition = p;
            box.Selection.Select(tp,tp);
            box.Cursor = c;
            //if (moveTo != null) {
            //    box.CaretPosition = moveTo;
            //}
        }

        private static void FireContentChanged(object sender, RoutedEventArgs e) {
            RichTextBox box = sender as RichTextBox;
            if (box == null) {
                return;
            }
            //box.Selection.Select(box.CaretPosition, tp);
            //TextRange tr = new TextRange(box.Document.ContentStart,
            //                   box.Document.ContentEnd);

            //using (MemoryStream ms = new MemoryStream()) {
            //    tr.Save(ms, DataFormats.Xaml);
            //    string xamlText = Encoding.Default.GetString(ms.ToArray());
            //    SetBoundDocument(box, xamlText);
            //}
            var xaml = DataCommunicator.GetXaml(box.Document);
            var html = DataCommunicator.GetHtml(xaml);
            box.SetValue(BoundDocument, html);
            UpdateRichTextBox(box);
        }

        public static string GetBoundDocument(DependencyObject dp) {
            var html = dp.GetValue(BoundDocument) as string;
            var xaml = string.Empty;

            if (!string.IsNullOrEmpty(html))
                xaml = DataCommunicator.GetXaml(html);

            return xaml;
        }

        public static void SetBoundDocument(DependencyObject dp, string value) {
            var xaml = value;
            var html = DataCommunicator.GetHtml(xaml);
            dp.SetValue(BoundDocument, html);
        }


    }


}