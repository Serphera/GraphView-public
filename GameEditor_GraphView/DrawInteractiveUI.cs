using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

using GameEditor_GraphView.ViewModel;

namespace GameEditor_GraphView {


    static class DrawInteractiveUI {


        /// <summary>
        /// Draws rectangles on control points
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        public static List<FrameworkElement> DrawRectangles(CurveGraphViewModel vm, List<Point> list) {

            var curveList = vm.ModelItems.Item;
            List<FrameworkElement> elementList = new List<FrameworkElement>();
            int originalPos = 0;
            int smoothPos = 0;

            for (int j = 0; j < curveList.GetCount; j++) {

                var curve = curveList.Curve[j];
                //Draws point rectangles
                for (int i = 0; i < curveList.Curve[j].Points.Count - 1; i++) {

                    Rectangle rect = new Rectangle();

                    rect.Height = 5;
                    rect.Width = 5;
                    rect.Fill = Brushes.DarkBlue;

                    if (i == originalPos && smoothPos < curve.Smoothness.Count || i == curve.Points.Count - 1) {

                        //_Curve.Original[smoothPos] = _Curve.Points[i];

                        if (smoothPos < curve.Smoothness.Count) {

                            originalPos += curve.Smoothness[smoothPos] + 1;
                        }


                        smoothPos++;
                        rect.Fill = Brushes.Yellow;
                    }

                    string name = String.Format("point{0}", i);
                    rect.Tag = name;

                    elementList.Add(rect);

                    try {

                        var pos = vm.Camera.OffsetPosition(new Point(list[i].X + vm.Camera.GetTransform(0, 0) - 2.5f, list[i].Y + vm.Camera.GetTransform(0, 1) - 2.5f));
                        var camera = vm.Camera;

                        Canvas.SetTop(rect, pos.Y);
                        Canvas.SetLeft(rect, pos.X);
                    }
                    catch (IndexOutOfRangeException) {

                        throw;
                    }
                }
            }



            return elementList;
        }
    }
}
