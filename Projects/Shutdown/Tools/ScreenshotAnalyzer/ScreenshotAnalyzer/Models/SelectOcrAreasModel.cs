using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ScreenshotAnalyzer.Models.Results;
using ScreenshotAnalyzer.Resources.Localisation;
using ScreenshotAnalyzer.Structures.SelectOcrAreasHelper;
using ScreenshotAnalyzer.Windows;
using ScreenshotAnalyzerBusiness.Manager;
using ScreenshotAnalyzerBusiness.Structures;
using ScreenshotAnalyzerBusiness.Structures.Results;
using Utils;
using ViewValidator.Windows;
using ZoomAndPan;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using Size = System.Windows.Size;

namespace ScreenshotAnalyzer.Models {
    public enum CurrentEditMode {
        SelectRectangle,
        ResizeRectangle,
        SelectArea,
        SelectAnchor,
        MoveRectangle
    };

    public class SelectOcrAreasModel : NotifyPropertyChangedBase {

        #region Constructor
        public SelectOcrAreasModel(MainWindowModel mainWindowModel) {
            //ScreenshotGroup = screenshotGroup;
            MainWindowModel = mainWindowModel;
            CurrentEditMode = CurrentEditMode.SelectRectangle;
            Init();
        }

        private void Init() {
            RectangleToOcrRectangle = new Dictionary<Rectangle, OcrRectangle>();
            OcrRectangleToRectangle = new Dictionary<OcrRectangle, Rectangle>();
            if (Canvas != null) {
                Canvas.Children.Clear();
                Canvas.Children.Add(Image);
                if (Screenshot != null) {
                    foreach (var ocrRect in Screenshot.Rectangles) {
                        AddRectangle(ocrRect, CurrentEditMode.SelectArea);
                    }
                    if (Screenshot.Anchor != null) {
                        AddRectangle(Screenshot.Anchor, CurrentEditMode.SelectAnchor);
                    }
                }
            }
            Rectangles = new ObservableCollectionAsync<Rectangle>();
        }
        #endregion Constructor

        #region Properties
        private MainWindowModel MainWindowModel { get; set; }

        #region ScreenshotGroup
        private ScreenshotGroup _screenshotGroup;
        public ScreenshotGroup ScreenshotGroup {
            get { return _screenshotGroup; }
            set {
                if (_screenshotGroup != value) {
                    _screenshotGroup = value;
                    OnPropertyChanged("ScreenshotGroup");
                }
            }
        }
        #endregion ScreenshotGroup

        private EditModeBase _currentEditModeInstance;
        public EditModeBase CurrentEditModeInstance { get { return _currentEditModeInstance; } }

        #region Screenshot
        private Screenshot _screenshot;
        public Screenshot Screenshot {
            get { return _screenshot; }
            set {
                if (_screenshot != value) {
                    //Unregister from events of old Screenshot
                    if (_screenshot != null) _screenshot.Rectangles.CollectionChanged -= Rectangles_CollectionChanged;
                    _screenshot = value;
                    
                    Init();
                    //Register for events from new screenshot
                    if (_screenshot != null) _screenshot.Rectangles.CollectionChanged += Rectangles_CollectionChanged;
                    OnPropertyChanged("Screenshot");
                }
            }
        }

        #endregion Screenshot

        public ObservableCollectionAsync<Rectangle> Rectangles { get; set; }
        //Canvas in the corresponding control
        public Canvas Canvas { get; set; }
        //Image in the corresponding control
        public Image Image { get; set; }
        public ZoomAndPanControl ZoomAndPanControl { get; set; }
        public Dictionary<Rectangle, OcrRectangle> RectangleToOcrRectangle { get; set; }
        public Dictionary<OcrRectangle,Rectangle> OcrRectangleToRectangle { get; set; }

        private EditModeSelectAndResizeRectangle _editModeSelectAndResizeRectangle = null;
        private EditModeSelectAreaOrAnchor _editModeSelectAreaOrAnchor = null;
        private DlgPopupProgress _popupProgressDialog;
        #region CurrentEditMode
        private CurrentEditMode _currentEditMode;
        public CurrentEditMode CurrentEditMode {
            get { return _currentEditMode; }
            set {
                _currentEditMode = value;
                switch(_currentEditMode) {
                    case CurrentEditMode.SelectRectangle:
                    case CurrentEditMode.ResizeRectangle:
                    case CurrentEditMode.MoveRectangle:
                        _currentEditModeInstance = _editModeSelectAndResizeRectangle;
                        break;
                    case CurrentEditMode.SelectArea:
                    case CurrentEditMode.SelectAnchor:
                        _currentEditModeInstance = _editModeSelectAreaOrAnchor;//new EditModeSelectAreaOrAnchor(Canvas, ZoomAndPanControl, this, Canvas);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                OnPropertyChanged("CurrentEditMode");
            }
        }


        #endregion CurrentEditMode

        #endregion Properties

        #region Methods
        #region SetControls
        public void SetControls(Canvas canvas, Image image, ZoomAndPanControl zoomAndPanControl) {
            Canvas = canvas;
            Image = image;
            ZoomAndPanControl = zoomAndPanControl;
            _editModeSelectAndResizeRectangle = new EditModeSelectAndResizeRectangle(Canvas, ZoomAndPanControl, this, Canvas);
            _editModeSelectAreaOrAnchor = new EditModeSelectAreaOrAnchor(Canvas, ZoomAndPanControl, this, Canvas);
            CurrentEditMode = CurrentEditMode;
        }
        #endregion SetControls

        void Rectangles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch(e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (OcrRectangle ocrRect in e.NewItems) {
                        AddRectangle(ocrRect, CurrentEditMode.SelectArea);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (OcrRectangle ocrRect in e.OldItems) {
                        Rectangle visualRect = OcrRectangleToRectangle[ocrRect];
                        Canvas.Children.Remove(visualRect);
                        OcrRectangleToRectangle.Remove(ocrRect);
                        RectangleToOcrRectangle.Remove(visualRect);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region AddRectangle
        private void AddRectangle(OcrRectangle ocrRect, CurrentEditMode type) {
            Rectangle rect = CreateRectangle(Canvas, ocrRect, type);
            Rectangles.Add(rect);
            RectangleToOcrRectangle.Add(rect, ocrRect);
            OcrRectangleToRectangle.Add(ocrRect, rect);
        }

        public void AddRectangle(Point dragStartPosition, Point dragEndPosition) {
            //OcrRectangle rect = new OcrRectangle(dragStartPosition, dragEndPosition);
            //Images.ExtractImage(Screenshot.Path, new System.Drawing.Rectangle((int) rect.UpperLeft.X, (int) rect.UpperLeft.Y, (int)rect.Width, (int)rect.Height));
            if (CurrentEditMode == CurrentEditMode.SelectAnchor) {
                //Delete current anchor rectangle in canvas
                if (Screenshot.Anchor != null) {
                    Canvas.Children.Remove(OcrRectangleToRectangle[Screenshot.Anchor]);
                }
                Screenshot.SetAnchor(dragStartPosition, dragEndPosition);
                AddRectangle(Screenshot.Anchor, CurrentEditMode.SelectAnchor);
            } else if (CurrentEditMode == CurrentEditMode.SelectArea) {
                Screenshot.AddRectangle(dragStartPosition, dragEndPosition);
            }
        }
        #endregion AddRectangle

        #region DeleteRectangles
        public void DeleteRectangle(OcrRectangle rect) {
            if (Screenshot != null) Screenshot.DeleteRectangle(rect);
        }
        #endregion DeleteRectangles

        #region CreateRectangle
        internal Rectangle CreateRectangle(Canvas canvas, OcrRectangle ocrRect, CurrentEditMode type) {
            Rectangle rect = null;
            canvas.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                rect = new Rectangle();

                rect.StrokeThickness = 1;
                rect.Stroke = type == CurrentEditMode.SelectArea ? Brushes.Red : Brushes.Blue;
                canvas.Children.Add(rect);
                Canvas.SetZIndex(rect, 1);
                SetRectanglePositionFromOcrRectangle(rect, ocrRect);
            }));
            return rect;
        }
        #endregion CreateRectangle

        #region SetRectanglePositionFromOcrRectangle
        public void SetRectanglePositionFromOcrRectangle(Rectangle rect, OcrRectangle ocrRect) {
            ocrRect = ImageToCanvas(ocrRect);
            Canvas.SetLeft(rect, ocrRect.UpperLeft.X);
            Canvas.SetTop(rect, ocrRect.UpperLeft.Y);
            rect.Width = ocrRect.Width;
            rect.Height = ocrRect.Height;
        }
        #endregion SetRectanglePositionFromOcrRectangle

        #region Coordinate System Helper
        public Point ImageToCanvas(Point point) {
            Point result = new Point();
            result.X = point.X <= 0.001 ? 0 : Image.ActualWidth * point.X / Screenshot.ImageSize.Width;
            result.Y = point.Y <= 0.001 ? 0 : Image.ActualHeight * point.Y / Screenshot.ImageSize.Height;

            return result;
        }

        public OcrRectangle ImageToCanvas(OcrRectangle rect) {
            return new OcrRectangle(ImageToCanvas(rect.UpperLeft), ImageToCanvas(rect.LowerRight));
        }

        //Converts a position on the canvas to a position in image pixel coordinates (real pixel coordinates of the image)
        public Point CanvasToImage(Point point) {
            Point result = new Point();
            result.X = Screenshot.ImageSize.Width * point.X / Image.ActualWidth;
            result.Y = Screenshot.ImageSize.Height * point.Y / Image.ActualHeight;

            return result;
        }
        #endregion Coordinate System Helper

        public void ExtractTextForAll() {
            ExtractTexts(false);
            //RecognitionResult recognitionResult = new RecognitionResult(ScreenshotGroup);
            //recognitionResult.ExtractText();
            //MainWindowModel.SelectedTable.RecognitionResult = recognitionResult;
            //DlgTextTable dlg = new DlgTextTable() { DataContext = new TextTableModel(recognitionResult) };
            //dlg.Show();
        }

        public void ExtractText() {
            if(MainWindowModel.SelectedTable == null) throw new InvalidOperationException(ResourcesGui.SelectOcrAreasModel_ExtractText_ErrorNoScreenshotgroupSelected);
            ExtractTexts(true);
        }

        private void ExtractTexts(bool onlyCurrentlySelectedScreenshot) {
            if (onlyCurrentlySelectedScreenshot && Screenshot == null)
                throw new InvalidOperationException(ResourcesGui.SelectOcrAreasModel_ExtractText_NoScreenshotSelected);
            RecognitionResult recognitionResult = MainWindowModel.SelectedTable.RecognitionResult;
            if(recognitionResult == null)
                recognitionResult = new RecognitionResult(ScreenshotGroup);
            //ProgressCalculator progress = new ProgressCalculator();
            if (onlyCurrentlySelectedScreenshot) recognitionResult.ExtractText(Screenshot);
            else recognitionResult.ExtractText();
            MainWindowModel.SelectedTable.RecognitionResult = recognitionResult;
            CorrectionAssistantModel correctionModel = new CorrectionAssistantModel(MainWindowModel.SelectedTable);
            DlgCorrectionAssistant dlg = new DlgCorrectionAssistant() { DataContext = correctionModel, Owner = UIHelpers.TryFindParent<Window>(Canvas) };
            dlg.ShowDialog();
            //DlgTextTable dlg = new DlgTextTable() {DataContext = new TextTableModel(recognitionResult)};
            //dlg.Show();
        }


        public void UseSelectionForAll() {
            if (MainWindowModel.SelectedTable == null) throw new InvalidOperationException(ResourcesGui.SelectOcrAreasModel_ExtractText_ErrorNoScreenshotgroupSelected);
            ProgressCalculator progress = new ProgressCalculator();
            progress.DoWork += progress_DoWork;
            progress.RunWorkerCompleted += progress_RunWorkerCompleted;
            _popupProgressDialog = new DlgPopupProgress() { DataContext = progress, Owner = UIHelpers.TryFindParent<Window>(Canvas) };
            progress.RunWorkerAsync(_popupProgressDialog);

            _popupProgressDialog.ShowDialog();
        }

        void progress_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e) {
            if(_popupProgressDialog != null)
                _popupProgressDialog.Close();
        }

        void progress_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e) {
            try {
                ScreenshotGroup.SetRectanglesForGroup(Screenshot, sender as ProgressCalculator);
            } catch (Exception ex) {
                Canvas.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => MessageBox.Show(ResourcesGui.SelectOcrAreasModel_progress_DoWork_Error + Environment.NewLine + ex.Message)));
            }
        }

        public void EditingFinished(OcrRectangle rect) {
            OcrRectangleRegistry.UpdateRectangle(Screenshot, rect);
        }
        #endregion Methods
    }
}
