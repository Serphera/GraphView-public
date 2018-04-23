using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;

namespace GameEditor_GraphView
{
    class DrawGraphV2 {

        public static List<Point> Draw(BezierCurveV2 curve, TestingCamera cam) {

            double[] offset = new double[] { cam.GetTransform(0, 0), cam.GetTransform(0, 1) };
            double scale = cam.GetScale;

            BezierCurveV2 _Curve = BezierCurveV2.ScaleAdjusted(curve, cam);
            PointCollection lerpList = new PointCollection();


            for (int i = 0; i < _Curve.Smoothness.Count; i++) {

                double steps = 0;
                int multiplier = 0;

                for (int j = 0; j < i; j++) {

                    multiplier += _Curve.Smoothness[j] + 1;
                }

                try {

                    int lineSmoothness = _Curve.Smoothness[i];

                    while (steps < 1.05) {

                        if (lineSmoothness != 0) {

                            if (lineSmoothness == 1) {

                                Point lerp1 = MathLerp.Lerp(_Curve.Points[multiplier], _Curve.Points[1 + multiplier], steps);
                                Point lerp2 = MathLerp.Lerp(_Curve.Points[1 + multiplier], _Curve.Points[2 + multiplier], steps);

                                Point lerp4 = MathLerp.Lerp(lerp1, lerp2, steps);

                                lerpList.Add(lerp4);
                            }
                            else {

                                Point lerp1 = MathLerp.Lerp(_Curve.Points[multiplier], _Curve.Points[1 + (multiplier)], steps);
                                Point lerp2 = MathLerp.Lerp(_Curve.Points[1 + multiplier], _Curve.Points[2 + multiplier], steps);

                                Point lerp3 = MathLerp.Lerp(_Curve.Points[2 + multiplier], _Curve.Points[3 + multiplier], steps);

                                Point lerp4 = MathLerp.Lerp(lerp1, lerp2, steps);
                                Point lerp5 = MathLerp.Lerp(lerp2, lerp3, steps);

                                Point lerp6 = MathLerp.Lerp(lerp4, lerp5, steps);

                                lerpList.Add(lerp6);
                            }
                        }
                        else {

                            lerpList.Add(MathLerp.Lerp(_Curve.Points[multiplier], _Curve.Points[multiplier + 1], steps));
                        }

                        if (scale < 0.5) {

                            steps += 0.05;
                        }
                        else {

                            steps += 0.1;
                        }


                    }
                }
                catch (Exception) {

                    //throw;
                }

            }

            return lerpList.ToList();
        }


        /*
        public static List<FrameworkElement> Draw(BezierCurveV2 curve, TestingCamera cam) {

            double[] offset = new double[] { cam.GetTransform(0, 0), cam.GetTransform(0, 1) };
            double scale = cam.GetScale;

            List<FrameworkElement> lineList = new List<FrameworkElement>();
            BezierCurveV2 _Curve = BezierCurveV2.ScaleAdjusted(curve, cam);
            PointCollection lerpList = new PointCollection();

            Console.WriteLine("drawing graph \n");

            for (int i = 0; i < _Curve.Smoothness.Count; i++) {
                
                double steps = 0;
                int multiplier = 0;

                for (int j = 0; j < i; j++) {

                    multiplier += _Curve.Smoothness[j] + 1;
                }

                try {

                    int lineSmoothness = _Curve.Smoothness[i];

                    while (steps < 1.05) {

                        if (lineSmoothness != 0) {

                            if (lineSmoothness == 1) {

                                Point lerp1 = MathLerp.Lerp(_Curve.Points[multiplier], _Curve.Points[1 + multiplier], steps);
                                Point lerp2 = MathLerp.Lerp(_Curve.Points[1 + multiplier], _Curve.Points[2 + multiplier], steps);

                                Point lerp4 = MathLerp.Lerp(lerp1, lerp2, steps);

                                lerpList.Add(lerp4);
                            }
                            else {

                                Point lerp1 = MathLerp.Lerp(_Curve.Points[multiplier], _Curve.Points[1 + (multiplier)], steps);
                                Point lerp2 = MathLerp.Lerp(_Curve.Points[1 + multiplier], _Curve.Points[2 + multiplier], steps);

                                Point lerp3 = MathLerp.Lerp(_Curve.Points[2 + multiplier], _Curve.Points[3 + multiplier], steps);

                                Point lerp4 = MathLerp.Lerp(lerp1, lerp2, steps);
                                Point lerp5 = MathLerp.Lerp(lerp2, lerp3, steps);

                                Point lerp6 = MathLerp.Lerp(lerp4, lerp5, steps);

                                lerpList.Add(lerp6);
                            }
                        }
                        else {

                            lerpList.Add(MathLerp.Lerp(_Curve.Points[multiplier], _Curve.Points[multiplier + 1], steps));
                        }

                        if (scale < 0.5) {

                            steps += 0.05;
                        }
                        else {

                            steps += 0.1;
                        }
                        

                    }
                }
                catch (Exception) {

                    //throw;
                }

            }

            //Draws Lines
            for (int i = 0; i < lerpList.Count - 1; i++) {                

                Line line = new Line();

                line.StrokeThickness = 2;
                line.Stroke = Brushes.LightGreen;

                line.X1 = lerpList[i].X;
                line.X2 = lerpList[i + 1].X;
                line.Y1 = lerpList[i].Y;
                line.Y2 = lerpList[i + 1].Y;

                lineList.Add(line);
            }

            int originalPos = 0;
            int smoothPos = 0;

            //Draws point rectangles
            for (int i = 0; i < _Curve.Points.Count; i++) {

                Rectangle rect = new Rectangle();

                rect.Height = 5;
                rect.Width = 5;
                rect.Fill = Brushes.DarkBlue;                

                if (i == originalPos && smoothPos < _Curve.Smoothness.Count || i == _Curve.Points.Count - 1) {

                    //_Curve.Original[smoothPos] = _Curve.Points[i];

                    if (smoothPos < _Curve.Smoothness.Count) {

                        originalPos += _Curve.Smoothness[smoothPos] + 1;
                    }

                    
                    smoothPos++;
                    rect.Fill = Brushes.Yellow;
                }

                string name = String.Format("point{0}", i);
                rect.Tag = name;

                lineList.Add(rect);

                try {

                    double posY = _Curve.Points[i].Y - 2.5;
                    double posX = _Curve.Points[i].X - 2.5;

                    Canvas.SetTop(rect, posY);
                    Canvas.SetLeft(rect, posX);
                }
                catch (IndexOutOfRangeException) {

                    throw;
                }
            }

            return lineList;
        }

    */
    }
}
