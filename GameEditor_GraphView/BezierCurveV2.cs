using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;

using EditorLibrary;

namespace GameEditor_GraphView
{
    public class BezierCurveV2
    {

        private double[] scale;

        private List<Point> _points;
        private List<Point> _Original;

        //0: linear, 1: semi-linear, 2: full bezier
        private List<int> _smoothness;

        public List<int> Smoothness { get { return _smoothness; } }

        public List<Point> Original { get { return _Original; } private set { _Original = value; } }

        public List<Point> Points { get { return _points; } set { _points = value; } }


        public BezierCurveV2(List<Point> points) {

            _smoothness = new List<int>();
            _Original = new List<Point>(points);

            for (int i = 0; i < points.Count - 1; i++) {

                _smoothness.Add(0);
            }

            _points = KeyFramesToCurve(points);
            scale = new double[] { 1, 1, 1 };
        }


        private BezierCurveV2(BezierCurveV2 curve, TestingCamera cam) {

            double scale = cam.GetScale;
            _points = new List<Point>();

            _smoothness = new List<int>(curve.Smoothness);
            _Original = new List<Point>(curve._Original);

            for (int i = 0; i < curve.Points.Count; i++) {

                _points.Add(new Point(curve.Points[i].X / scale, curve.Points[i].Y / scale));
            }
        }


        //TODO: Figure out why insert Point is at a different Y value than curve is on
        public void InsertPoint(Point pos) {
            
            int position = 0;
            double delta = 0;
            Point insertPos = new Point(0, 0);

            for (int i = 0; i < Points.Count; i++) {

                if (Points[i].X > pos.X) {

                    delta = pos.X / Points[i].X;               
                    insertPos = LerpMath.GetY(Points[i - 1], Points[i], pos.X);

                    if ((pos.Y + 20) > insertPos.Y) {

                        for (int j = 0; j < Original.Count; j++) {

                            if (pos.X < Original[j].X) {

                                position = j;
                                break;
                            }
                        }
                        break;
                    }
                }
            }

            if (position == 0 || delta == 0 || insertPos.X == 0) { return; }

            List<Point> list = new List<Point>(Original);

            list.Insert(position, new Point(pos.X , insertPos.Y));
            Original.Insert(position, new Point(pos.X, insertPos.Y));
            _smoothness.Insert(position, 0);
            _points = KeyFramesToCurve(list);

            //TODO: return the keyframes to the parent object
        }


        public List<Point> KeyFramesToCurve(List<Point> keyframes) {

            int modifier = 1;
            for (int i = 0; i < _smoothness.Count; i++) {

                List<Point> insertPoints = new List<Point>();

                switch (_smoothness[i]) {

                    case 0:                        

                        break;

                    case 1:

                        insertPoints.Add(LerpMath.Lerp(keyframes[i + modifier - 1], keyframes[i + modifier], 0.5));
                        keyframes.InsertRange(i  + modifier, insertPoints);
                        modifier += 1;
                        break;

                    case 2:

                        insertPoints.Add(LerpMath.Lerp(keyframes[i + modifier - 1], keyframes[i + modifier], 0.30));
                        insertPoints.Add(LerpMath.Lerp(keyframes[i + modifier - 1], keyframes[i + modifier], 0.70));
                        keyframes.InsertRange(i + modifier, insertPoints);
                        modifier += 2;
                        break;

                    default:
                        break;
                }
             
            }
            return keyframes;
        }


        public static BezierCurveV2 ScaleAdjusted(BezierCurveV2 curve, TestingCamera cam) {

            return new BezierCurveV2(curve, cam);
        }

    }
}
