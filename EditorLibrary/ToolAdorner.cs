using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Controls;
using System.Threading;
using System.Text.RegularExpressions;




namespace EditorLibrary {

    public class ToolAdorner : Adorner {

        private Point _Location;
        VisualCollection visualChildren;
        int _type = 0;
        private ViewModelBase _viewModel;

        public Point offset { get; set; }


        public ToolAdorner(UIElement adornedElement, ViewModelBase viewModel, Point position, string name, int type = 0) : base(adornedElement) {

            visualChildren = new VisualCollection(this);

            _Location = position;
            _type = type;
            Name = name;
            _viewModel = viewModel;           

            // TODO: Might be a memory leak
            MouseDown += new System.Windows.Input.MouseButtonEventHandler(curveAdorner_MouseDown);
        }


        public void UpdatePosition(Point location) {

            _Location = location;
            InvalidateVisual();
            InvalidateArrange();
        }

        //Redirects to viewModel
        private void curveAdorner_MouseDown(object sender, MouseButtonEventArgs e) {

            _viewModel.OnMouseDown(sender, e);
        }


        private void BuildHitBox(DrawingContext context, Point origin) {

            Rect rect = new Rect();
            rect.X = origin.X;
            rect.Y = origin.Y;
            rect.Height = 23;
            rect.Width = 23;

            Pen rectPen = new Pen(new SolidColorBrush(Colors.Black), 0.0);
            SolidColorBrush rectBrush = new SolidColorBrush(Colors.Black);
            rectBrush.Opacity = 0.2;

            context.DrawRectangle(rectBrush, rectPen, rect);
        }


        protected override void OnRender(DrawingContext context) {

            Point centre = _Location;
            Point left = new Point(centre.X - 33, centre.Y - 9);

            Polygon shape = new Polygon();
            Action<DrawingContext, Point, Point, Polygon, double> toolMethod = null;
            double angle = 0;

            switch (_type) {

                case 0:

                    toolMethod = MoveAdorner;
                    break;

                case 1:

                    toolMethod = ScaleAdorner;
                    break;

                default:
                    break;
            }

            switch (Name) {

                case "cross":

                    left = new Point(centre.X - 30, centre.Y + 2.5);
                    Point top = new Point(centre.X + 2.5, centre.Y - 25);
                    Point right = new Point(centre.X + 33, centre.Y + 2.5);
                    Point bottom = new Point(centre.X + 2.5, centre.Y + 30);

                    context.DrawLine(new Pen(new SolidColorBrush(Colors.Red), 1.0), left, right);
                    context.DrawLine(new Pen(new SolidColorBrush(Colors.LightGreen), 1.0), top, bottom);
                    return;

                case "left":

                    break;

                case "top":

                    angle = Math.PI / 2;
                    break;

                case "right":

                    angle = Math.PI;
                    break;

                case "bottom":

                    angle = (Math.PI * 3) / 2;
                    break;

                default:
                    return;
            }

            toolMethod(context, left, centre, shape, angle);
            DrawAdorner(context, shape);
        }


        private void DrawAdorner(DrawingContext context, Polygon shape) {

            SolidColorBrush arrowBrush = new SolidColorBrush(Colors.Red);
            Pen arrowPen = new Pen(new SolidColorBrush(Colors.Blue), 1.0);

            //Draws shape for the movement adorner
            List<StreamGeometry> geoList = new List<StreamGeometry>();
            StreamGeometry streamGeometry = new StreamGeometry();

            using (StreamGeometryContext geometryContext = streamGeometry.Open()) {
                geometryContext.BeginFigure(shape.Points[0], true, true);

                PointCollection points = null;

                if (_type == 0) {

                    points = new PointCollection{
                        shape.Points[1],
                        shape.Points[2]
                    };
                }
                else {

                    points = new PointCollection {
                        shape.Points[1],
                        shape.Points[2],
                        shape.Points[3]
                    };
                }

                geometryContext.PolyLineTo(points, true, true);
            }

            geoList.Add(streamGeometry);

            for (int i = 0; i < geoList.Count; i++) {

                context.DrawGeometry(arrowBrush, arrowPen, geoList[i]);
            }

        }


        private void MoveAdorner(DrawingContext context, Point position, Point origin, Polygon shape, double angle) {

            origin.X += 2.5;
            origin.Y += 2.5;
            var edge = new Point(position.X - 18, position.Y);
            var top = new Point(position.X, position.Y - 8);
            var bottom = new Point(position.X, position.Y + 8);

            edge = GetPosition(angle, edge, origin);
            top = GetPosition(angle, top, origin);
            bottom = GetPosition(angle, bottom, origin);

            var finalPosition = GetPosition(angle, position, origin);

            shape.Points.Add(edge);
            shape.Points.Add(top);
            shape.Points.Add(bottom);

            BuildHitBox(context, finalPosition);
        }


        private void ScaleAdorner(DrawingContext context, Point position, Point origin, Polygon shape, double angle) {

            double x;
            double y;

            for (int i = 1; i < shape.Points.Count - 1; i += 2) {

                if (shape.Points[i].X == shape.Points[i + 1].X) {

                    y = shape.Points[2].Y;
                    x = shape.Points[0].X;

                    shape.Points.Insert(3, new Point(x, y));
                    shape.Points[0] = new Point(x, shape.Points[1].Y);
                    break;
                }
                else if (shape.Points[i].Y == shape.Points[i].Y) {

                    x = shape.Points[2].X;
                    y = shape.Points[0].Y;

                    shape.Points.Insert(3, new Point(x, y));
                    shape.Points[0] = new Point(shape.Points[1].X, y);
                    break;
                }
            }
        }


        private Point GetPosition(double angle, Point position, Point origin) {

            var sin = Math.Sin(angle);
            var cos = Math.Cos(angle);

            var delta = new Point(position.X - origin.X, position.Y - origin.Y);

            double deltaX = delta.X * cos - delta.Y * sin;
            double deltaY = delta.X * sin + delta.Y * cos;    

            return position = new Point(deltaX + origin.X, deltaY + origin.Y);
        }
    }

}
