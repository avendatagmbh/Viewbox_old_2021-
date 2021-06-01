// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-11-22
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using ImageHelper;
using ScreenshotAnalyzerBusiness.Manager;
using ScreenshotAnalyzerBusiness.Resources;
using ScreenshotAnalyzerBusiness.Structures.History;
using ScreenshotAnalyzerDatabase.Factories;
using ScreenshotAnalyzerDatabase.Interfaces;
using Utils;
using Point = System.Windows.Point;

namespace ScreenshotAnalyzerBusiness.Structures {
    public class ScreenshotGroup {
        public ScreenshotGroup(IDbScreenshotGroup dbScreenshotGroup) {
            DbScreenshotGroup = dbScreenshotGroup;
            Screenshots = new ObservableCollection<Screenshot>();
            foreach(var screenshot in DbScreenshotGroup.Screenshots)
                Screenshots.Add(new Screenshot(screenshot.Path, screenshot, true));
            //for (int i = 10; i <= 15; ++i)
            //    Screenshots.Add(new Screenshot(@"C:\Users\beh\Documents\Verprobungsbilder\" + i + ".PNG"));
        }

        #region Properties
        internal IDbScreenshotGroup DbScreenshotGroup { get; set; }
        public string Name {
            get { return DbScreenshotGroup.Name; }
            set { DbScreenshotGroup.Name = value; }
        }

        public ObservableCollection<Screenshot> Screenshots { get; private set; }
        #endregion Properties

        #region Methods
        public void Add(string screenshotPath) {
            Screenshots.Add(new Screenshot(screenshotPath, DatabaseObjectFactory.CreateScreenshot(DbScreenshotGroup), false));
        }

        public void SetRectanglesForGroup(Screenshot refScreenshot, ProgressCalculator progress) {
            if (refScreenshot == null) throw new InvalidOperationException(ResourcesBusiness.ScreenshotGroup_SetRectanglesForGroup_Error_NoReferenceImageSelected);
            if (refScreenshot.Anchor == null) throw new InvalidOperationException(ResourcesBusiness.ScreenshotGroup_SetRectanglesForGroup_Error_NoAnchorInRefImage);
            progress.SetWorkSteps(Screenshots.Count, false);
            ScreenshotInfo[] infos = Images.FindImageInImages(refScreenshot, refScreenshot.Anchor, this, progress);
            for(int i = 0; i < infos.Length; ++i) {
                if (refScreenshot == Screenshots[i]) continue;
                //Screenshots[i].Rectangles.Clear();
                double correctionX = (refScreenshot.Anchor.UpperLeft.X - infos[i].foundX);
                double correctionY = (refScreenshot.Anchor.UpperLeft.Y - infos[i].foundY);

                if (Screenshots[i].AnchorRemained && refScreenshot.AnchorRemained) {
                    //Calculate correction from already transfered anchors (which were calculated last time)
                    //These anchors can be reused as they have not been touched and therefore there is no need to start the registration process again
                    correctionX = (refScreenshot.Anchor.UpperLeft.X - Screenshots[i].Anchor.UpperLeft.X);
                    correctionY = (refScreenshot.Anchor.UpperLeft.Y - Screenshots[i].Anchor.UpperLeft.Y);
                } else {
                    Screenshots[i].SetAnchor(
                        new System.Windows.Point(refScreenshot.Anchor.UpperLeft.X - correctionX,refScreenshot.Anchor.UpperLeft.Y - correctionY),
                        new System.Windows.Point(refScreenshot.Anchor.LowerRight.X - correctionX,refScreenshot.Anchor.LowerRight.Y - correctionY)
                        );
                }

                SetRectangles(refScreenshot, Screenshots[i], correctionX, correctionY);

                //foreach (var rect in refScreenshot.Rectangles) {
                //    System.Windows.Point upperLeft = new System.Windows.Point(rect.UpperLeft.X - correctionX, rect.UpperLeft.Y - correctionY);
                //    System.Windows.Point lowerRight = new System.Windows.Point(rect.LowerRight.X - correctionX, rect.LowerRight.Y - correctionY);
                //    OcrRectangle newRect = new OcrRectangle(upperLeft, lowerRight);
                //    Screenshots[i].Rectangles.Add(newRect);
                //}
                Screenshots[i].AnchorRemained = true;

            }
            refScreenshot.RectangleHistory.Clear();
            refScreenshot.AnchorRemained = true;
            progress.StepDone();
        }

        private void SetRectangles(Screenshot refScreenshot, Screenshot screenshot, double correctionX, double correctionY) {
            RectangleMapping currentRefToLastRef = new RectangleMapping(refScreenshot.Rectangles.ToList(), refScreenshot.RectangleHistory);
            RectangleMapping currentScrToLastScr = new RectangleMapping(screenshot.Rectangles.ToList(), screenshot.RectangleHistory);
            RectangleMapping currentRefToCurrentScr = currentRefToLastRef.Combine(currentScrToLastScr, true);

            HashSet<int> foundIndices = new HashSet<int>();
            //Move/Resize found rectangles
            for (int i = 0; i < refScreenshot.Rectangles.Count; ++i) {
                OcrRectangle rect = refScreenshot.Rectangles[i];
                Point upperLeft = new Point(rect.UpperLeft.X - correctionX, rect.UpperLeft.Y - correctionY);
                Point lowerRight = new Point(rect.LowerRight.X - correctionX, rect.LowerRight.Y - correctionY);

                int mappedIndex = currentRefToCurrentScr.MappingTo(i);
                if (mappedIndex != -1) {
                    foundIndices.Add(mappedIndex);
                    //Adjust found rectangle
                    screenshot.Rectangles[mappedIndex].UpperLeft = upperLeft;
                    screenshot.Rectangles[mappedIndex].LowerRight = lowerRight;
                    //Update to database
                    OcrRectangleRegistry.UpdateRectangle(screenshot, screenshot.Rectangles[mappedIndex]);
                }
            }

            //Delete all rectangles from back to front which could not be found
            for (int i = screenshot.Rectangles.Count - 1; i >= 0 ; --i) {
                if(!foundIndices.Contains(i)) screenshot.Rectangles.RemoveAt(i);
            }

            for (int i = 0; i < refScreenshot.Rectangles.Count; ++i) {
                OcrRectangle rect = refScreenshot.Rectangles[i];
                Point upperLeft = new Point(rect.UpperLeft.X - correctionX, rect.UpperLeft.Y - correctionY);
                Point lowerRight = new Point(rect.LowerRight.X - correctionX, rect.LowerRight.Y - correctionY);
                int mappedIndex = currentRefToCurrentScr.MappingTo(i);
                if (mappedIndex == -1) {
                    OcrRectangle newRect = new OcrRectangle(upperLeft, lowerRight);
                    screenshot.Rectangles.Insert(i, newRect);
                }
            }
            screenshot.RectangleHistory.Clear();
        }


        public void RemoveScreenshot(Screenshot screenshot) {
            screenshot.DbScreenshot.Delete();
            Screenshots.Remove(screenshot);
        }
        #endregion Methods

        public Screenshot FromId(int screenshotId) {
            foreach (var screenshot in Screenshots)
                if (screenshot.DbScreenshot.Id == screenshotId)
                    return screenshot;
            return null;

        }
    }
}
