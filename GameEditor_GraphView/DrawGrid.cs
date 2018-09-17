using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows;

using EditorLibrary;
using GameEditor_GraphView.Model;


namespace GameEditor_GraphView {

    static class DrawGrid {


        public static GraphGrid RenderGrid(CurveGraphModel model, Camera cam = null) {

            GraphGrid grid = Draw(model, cam);

            return grid;
        }


        public static List<Point> Draw(Camera cam) {

            List<Point> list = new List<Point>();



            return list;
        }

        public static GraphGrid Draw(CurveGraphModel model, Camera cam) {

            List<FrameworkElement> list = new List<FrameworkElement>();
            List<FrameworkElement> numH = new List<FrameworkElement>();
            List<FrameworkElement> numV = new List<FrameworkElement>();

            double scale = cam.GetScale;
            int rounding;

            if (scale > 0.3) { rounding = 100; }
            else if (scale > 0.05) { rounding = 10; }
            else { rounding = 1; }

            Point origin = new Point(-cam.GetTransform(0, 0), -cam.GetTransform(0, 1));
            Point offsetOrigin = new Point(origin.X - 2000, origin.Y - 2000);

            //Distance between pixels
            double spacingX = rounding / scale;
            double spacingY = rounding / scale;

            scale = 2000 / scale;
            int steps = (int)(4000 / rounding);

            for (int i = 0; i < steps; i++) {

                string xText = String.Format("{0:N0}", (i * rounding) - 2000);
                string yText = String.Format("{0:N0}", (i * rounding) - 2000);

                list.Add(DrawLines((spacingX * i) - scale, (spacingX * i) - scale, 0.3, true));
                list.Add(DrawLines((spacingY * i) - scale, (spacingX * i) - scale, 0.3, false));

                numH.Add(DrawNumbers(xText, (spacingX * i) - scale, -origin.Y, true));
                numV.Add(DrawNumbers(yText, (spacingY * i) - scale, -origin.X, false));
            }

            return new GraphGrid(list, numH, numV);
        }


        private static TextBlock DrawNumbers(string text, double start, double offset, bool IsVertical) {

            TextBlock numbers = new TextBlock();
            numbers.FontSize = 9;
            numbers.Text = text;

            if (IsVertical) {

                Canvas.SetLeft(numbers, start + 3);
                Canvas.SetTop(numbers, offset * -1);
            }
            else {

                Canvas.SetLeft(numbers, offset * -1);
                Canvas.SetTop(numbers, start + 3);
            }

            return numbers;
        }


        private static Line DrawLines(double startX, double startY, double opacity, bool vertical) {

            Line line = new Line();

            if (vertical) {

                line.X1 = startX;                
                line.Y1 = -2000;

                line.X2 = startX;
                line.Y2 = 2000;
            }
            else {

                line.X1 = -2000;                
                line.Y1 = startY;

                line.X2 = 2000;
                line.Y2 = startY;
            }

            line.Stroke = System.Windows.Media.Brushes.Black;
            line.StrokeThickness = 1;
            line.Opacity = opacity;

            return line;
        }   


    }
}
