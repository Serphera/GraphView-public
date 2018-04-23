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

using GameEditor_GraphView.ViewModel;

namespace GameEditor_GraphView {
    class CurveToolAdorner : Adorner {

        private Point _Location;
        VisualCollection visualChildren;
        int _type = 0;
        private CurveGraphViewModel _viewModel;
        //public string _name;
        public Point offset { get; set; }


        public CurveToolAdorner(UIElement adornedElement, CurveGraphViewModel viewModel, Point position, string name, int type = 0) : base(adornedElement) {

            visualChildren = new VisualCollection(this);

            _Location = position;
            _type = type;
            Name = name;
            _viewModel = viewModel;           

            // TODO: Might be a memory leak
            this.MouseDown += new System.Windows.Input.MouseButtonEventHandler(curveAdorner_MouseDown);
        }


        public void UpdatePosition(Point location) {

            _Location = location;
            this.InvalidateVisual();
            this.InvalidateArrange();
        }

        //Redirects to viewModel
        private void curveAdorner_MouseDown(object sender, MouseButtonEventArgs e) {

            _viewModel.OnMouseDown(sender, e);
        }


        private Rect BuildHitBox(Point origin) {

            Rect rect = new Rect();
            rect.X = origin.X;
            rect.Y = origin.Y;
            rect.Height = 23;
            rect.Width = 23;

            return rect;
        }


        protected override void OnRender(DrawingContext drawingContext) {

            SolidColorBrush arrowBrush = new SolidColorBrush(Colors.Red);            
            SolidColorBrush rectBrush = new SolidColorBrush(Colors.Black);
            rectBrush.Opacity = 0.0;

            Pen arrowPen = new Pen(new SolidColorBrush(Colors.Blue), 1.0);
            Pen rectPen = new Pen(new SolidColorBrush(Colors.Black), 0.0);

            Point centre = _Location;

            Point left;
            Point top;
            Point right;
            Point bottom;
            Polygon shape = new Polygon();

            switch (this.Name) {

                case "cross":


                    left = new Point(centre.X - 30, centre.Y + 2.5);
                    top = new Point(centre.X + 2.5, centre.Y - 25);
                    right = new Point(centre.X + 33, centre.Y + 2.5);
                    bottom = new Point(centre.X + 2.5, centre.Y + 30);

                    drawingContext.DrawLine(new Pen(new SolidColorBrush(Colors.Red), 1.0), left, right);
                    drawingContext.DrawLine(new Pen(new SolidColorBrush(Colors.LightGreen), 1.0), top, bottom);
                    return;

                case "left":

                    left = new Point(centre.X - 33, centre.Y - 9);
                    Rect rLeft = BuildHitBox(left);

                    shape.Points.Add(new Point(left.X + 3, left.Y + 11));
                    shape.Points.Add(new Point(left.X + 18, left.Y + 4));
                    shape.Points.Add(new Point(left.X + 18, left.Y + 18));

                    drawingContext.DrawRectangle(rectBrush, rectPen, rLeft);
                    break;

                case "top":

                    top = new Point(centre.X - 9, centre.Y - 32);
                    Rect rTop = BuildHitBox(top);

                    shape.Points.Add(new Point(top.X + 11.5, top.Y + 3));
                    shape.Points.Add(new Point(top.X + 3.5, top.Y + 18));
                    shape.Points.Add(new Point(top.X + 19.5, top.Y + 18));

                    drawingContext.DrawRectangle(rectBrush, rectPen, rTop);
                    break;

                case "right":

                    right = new Point(centre.X + 13, centre.Y - 9);
                    Rect rRight = BuildHitBox(right);

                    shape.Points.Add(new Point(right.X + 20, right.Y + 11));
                    shape.Points.Add(new Point(right.X + 5, right.Y + 4));
                    shape.Points.Add(new Point(right.X + 5, right.Y + 18));

                    drawingContext.DrawRectangle(rectBrush, rectPen, rRight);
                    break;

                case "bottom":

                    bottom = new Point(centre.X - 9, centre.Y + 16);
                    Rect rBottom = BuildHitBox(new Point(bottom.X, bottom.Y));

                    shape.Points.Add(new Point(bottom.X + 11.5, bottom.Y + 20));
                    shape.Points.Add(new Point(bottom.X + 3.5, bottom.Y + 5));
                    shape.Points.Add(new Point(bottom.X + 19.5, bottom.Y + 5));

                    drawingContext.DrawRectangle(rectBrush, rectPen, rBottom);
                    break;

                default:
                    return;
            }

            //Scale Adorner
            if (_type == 1) {

                double x;
                double y;

                for (int i = 1; i < shape.Points.Count - 1; i+= 2) {

                    if (shape.Points[i].X == shape.Points[i+1].X) {

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

                drawingContext.DrawGeometry(arrowBrush, arrowPen, geoList[i]);
            }

        }

    }
}
