// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-11-22
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Windows;
using Utils;

namespace ScreenshotAnalyzerBusiness.Structures {
    public class 
        OcrRectangle {
        #region Contructor

        public OcrRectangle(Point upperLeft, Point lowerRight) {
            UpperLeft = upperLeft;
            LowerRight = lowerRight;
            CreateConsistendState();
        }

        #endregion Contructor

        #region Properties

        #region UpperLeft

        private Point _upperLeft;

        public Point UpperLeft {
            get { return _upperLeft; }
            set {
                if (_upperLeft != value) {
                    _upperLeft = value;
                }
            }
        }

        #endregion UpperLeft

        #region LowerRight

        private Point _lowerRight;

        public Point LowerRight {
            get { return _lowerRight; }
            set {
                if (_lowerRight != value) {
                    _lowerRight = value;
                }
            }
        }

        private void CreateConsistendState() {
            if (_upperLeft.X > _lowerRight.X) {
                double x = _upperLeft.X;
                _upperLeft.X = _lowerRight.X;
                _lowerRight.X = x;
            }
            if (_upperLeft.Y > _lowerRight.Y) {
                double y = _upperLeft.Y;
                _upperLeft.Y = _lowerRight.Y;
                _lowerRight.Y = y;
            }
        }

        #endregion LowerRight

        public double Width {
            get { return Math.Abs(UpperLeft.X - LowerRight.X); }
        }

        public double Height {
            get { return Math.Abs(UpperLeft.Y - LowerRight.Y); }
        }



        #endregion Properties

        #region Methods

        public OcrRectangle Clone() {
            OcrRectangle result = new OcrRectangle(UpperLeft, LowerRight);
            return result;
        }

        public enum NearestSegment {
            NorthWest,
            North,
            NorthEast,
            East,
            SouthEast,
            South,
            SouthWest,
            West
        };

        //Compute the dot product AB . AC
        private double DotProduct(Point pointA, Point pointB, Point pointC) {
            Point AB = new Point(pointB.X - pointA.X, pointB.Y - pointA.Y);
            Point BC = new Point(pointC.X - pointB.X, pointC.Y - pointB.Y);
            double dot = AB.X*BC.X + AB.Y*BC.Y;

            return dot;
        }

//Compute the cross product AB x AC
        private double CrossProduct(Point pointA, Point pointB, Point pointC) {
            Point AB = new Point(pointB.X - pointA.X, pointB.Y - pointA.Y);
            Point AC = new Point(pointC.X - pointA.X, pointC.Y - pointA.Y);
            double cross = AB.X*AC.Y - AB.Y*AC.X;

            return cross;
        }

        //Compute the distance from A to B
        private double Distance(Point pointA, Point pointB) {
            double d1 = pointA.X - pointB.X;
            double d2 = pointA.Y - pointB.Y;

            return Math.Sqrt(d1*d1 + d2*d2);
        }

        //Compute the distance from AB to C
        //if isSegment is true, AB is a segment, not a line.
        private double LineToPointDistance2D(Point pointA, Point pointB, Point pointC,
                                             bool isSegment = true) {
            double dist = CrossProduct(pointA, pointB, pointC)/Distance(pointA, pointB);
            if (isSegment) {
                double dot1 = DotProduct(pointA, pointB, pointC);
                if (dot1 > 0)
                    return Distance(pointB, pointC);

                double dot2 = DotProduct(pointB, pointA, pointC);
                if (dot2 > 0)
                    return Distance(pointA, pointC);
            }
            return Math.Abs(dist);
        }

        public double GetDistance(Point testPoint, out NearestSegment nearestSegment) {
            double minDist = double.MaxValue;
            nearestSegment = NearestSegment.East;

            double dist;

            if ((dist = Distance(UpperLeft, testPoint)) < minDist) {
                nearestSegment = NearestSegment.NorthWest;
                minDist = dist;
            }

            if ((dist = Distance(LowerRight, testPoint)) < minDist) {
                nearestSegment = NearestSegment.SouthEast;
                minDist = dist;
            }
            if ((dist = Distance(new Point(UpperLeft.X, LowerRight.Y), testPoint)) < minDist) {
                nearestSegment = NearestSegment.SouthWest;
                minDist = dist;
            }
            if ((dist = Distance(new Point(LowerRight.X, UpperLeft.Y), testPoint)) < minDist) {
                nearestSegment = NearestSegment.NorthEast;
                minDist = dist;
            }

            if ((dist = LineToPointDistance2D(UpperLeft, new Point(LowerRight.X, UpperLeft.Y), testPoint)) < minDist) {
                nearestSegment = NearestSegment.North;
                minDist = dist;
            }
            if ((dist = LineToPointDistance2D(new Point(UpperLeft.X, LowerRight.Y), LowerRight, testPoint)) < minDist) {
                nearestSegment = NearestSegment.South;
                minDist = dist;
            }

            if ((dist = LineToPointDistance2D(UpperLeft, new Point(UpperLeft.X, LowerRight.Y), testPoint)) < minDist) {
                nearestSegment = NearestSegment.West;
                minDist = dist;
            }
            if ((dist = LineToPointDistance2D(new Point(LowerRight.X, UpperLeft.Y), LowerRight, testPoint)) < minDist) {
                nearestSegment = NearestSegment.East;
                minDist = dist;
            }

            return minDist;
        }

        public void Resize(Point newPoint, NearestSegment currentDragSegment) {
            switch(currentDragSegment) {
                case NearestSegment.NorthWest:
                    UpperLeft = newPoint;
                    break;
                case NearestSegment.North:
                    UpperLeft = new Point(UpperLeft.X, newPoint.Y);
                    break;
                case NearestSegment.NorthEast:
                    UpperLeft = new Point(UpperLeft.X, newPoint.Y);
                    LowerRight = new Point(newPoint.X, LowerRight.Y);
                    break;
                case NearestSegment.East:
                    LowerRight = new Point(newPoint.X, LowerRight.Y);
                    break;
                case NearestSegment.SouthEast:
                    LowerRight = newPoint;
                    break;
                case NearestSegment.South:
                    LowerRight = new Point(LowerRight.X, newPoint.Y);
                    break;
                case NearestSegment.SouthWest:
                    UpperLeft = new Point(newPoint.X, UpperLeft.Y);
                    LowerRight = new Point(LowerRight.X, newPoint.Y);
                    break;
                case NearestSegment.West:
                    UpperLeft = new Point(newPoint.X, UpperLeft.Y);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("currentDragSegment");
            }
            CreateConsistendState();
        }
        #endregion

        public bool IsInside(Point point) {
            return (point.X > UpperLeft.X && point.X < LowerRight.X) &&
                   (point.Y > UpperLeft.Y && point.Y < LowerRight.Y);
        }

        public void Move(Vector vector) {
            UpperLeft = new Point(UpperLeft.X+vector.X, UpperLeft.Y + vector.Y);
            LowerRight = new Point(LowerRight.X + vector.X, LowerRight.Y + vector.Y);
            //UpperLeft.Offset(vector.X, vector.Y);
            //LowerRight.Offset(vector.X, vector.Y);
        }
    }
}
