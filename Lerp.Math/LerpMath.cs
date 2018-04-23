﻿using System.Collections.Generic;
using System.Windows;


namespace EditorLibrary
{
    public static class LerpMath {

        //Linear Interpolation
        public static Point Lerp(Point start, Point end, double step) {

            Point pos = new Point(

                start.X + (step * (end.X - start.X)),
                start.Y + (step * (end.Y - start.Y))
                );

            return pos;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Point GetY(Point start, Point end, double x) {

            double delta = CalculateDelta(start.X, x);

            return new Point(
                x,
                start.Y + (delta * ((end.Y - start.Y) / (end.X - start.X)))
                );
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="x">Time</param>
        /// <returns></returns>
        public static Point GetY(List<Point> curve, double x) {

            for (int i = 0; i < curve.Count; i++) {

                if (curve[i].X > x) {

                    return GetY(curve[i - 1], curve[i], x);
                }
            }

            return new Point(0, 0);
        }


        /// <summary>
        /// Calculates delta between two points
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="invert"></param>
        /// <returns></returns>
        public static Point CalculateDelta(Point start, Point end, bool invert = false, bool invertY = false) {

            double xDelta;
            double yDelta;

            if (!invertY) {

                if (!invert) {

                    xDelta = (start.X > end.X) ? (start.X - end.X) * -1 : (start.X - end.X) * -1;
                    yDelta = (start.Y > end.Y) ? (start.Y - end.Y) * -1 : (start.Y - end.Y) * -1;
                }
                else {

                    xDelta = (start.X > end.X) ? (start.X - end.X) : (start.X - end.X);
                    yDelta = (start.Y > end.Y) ? (start.Y - end.Y) : (start.Y - end.Y);
                }
            }
            else {

                if (!invert) {

                    xDelta = (start.X > end.X) ? (start.X - end.X) * -1 : (start.X - end.X) * -1;
                    yDelta = (start.Y > end.Y) ? -(start.Y - end.Y) * -1 : -(start.Y - end.Y) * -1;
                }
                else {

                    xDelta = (start.X > end.X) ? (start.X - end.X) : (start.X - end.X);
                    yDelta = (start.Y > end.Y) ? -(start.Y - end.Y) : -(start.Y - end.Y);
                }
            }


            return new Point(xDelta, yDelta);
        }


        /// <summary>
        /// Calculates average position for list of Points
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static Point CalculateAverage(List<Point> list) {

            double x = 0;
            double y = 0;

            for (int i = 0; i < list.Count; i++) {

                x += list[i].X;
                y += list[i].Y;
            }

            x = x / list.Count;
            y = y / list.Count;

            return new Point(x, y);
        }


        /// <summary>
        /// Calculates delta of two values
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="invert"></param>
        /// <returns></returns>
        public static double CalculateDelta(double x, double y, bool invert = false) {

            double delta = (x > y) ? x - y : y - x;
            delta = (invert) ? delta * -1 : delta;

            return delta;
        }
    }
}