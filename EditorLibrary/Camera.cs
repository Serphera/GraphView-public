using System;
using System.ComponentModel;
using System.Windows;

namespace EditorLibrary {

    [Serializable]
    class Camera : INotifyPropertyChanged {

            //TODO: Remove once solution is integrated with main project

            private double[][] transformMatrix;
            private double[] frustrumMatrix;
            private double margin = 1000;
            private double worldScale = 1;

            [field: NonSerialized]
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName) {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public double GetSetMargin { get { return this.margin; } set { this.margin = value; } }

            public double GetScale { get { return this.worldScale; } set { this.worldScale = Math.Round(value, 4); } }

            public Camera(double tX = 0.0, double tY = 0.0, double tZ = 0.0, double rX = 0.0, double rY = 0.0, double rZ = 0.0) {

                this.transformMatrix =
                    new double[][] {
                        new double[] { tX, tY, tZ } ,
                        new double[] { rX, rY, rZ }
                    };

                this.frustrumMatrix = new double[] {
                tX + margin, tY + margin, tX - margin, tY - margin
            };

                //Console.WriteLine("Camera Transform Matrix Created");
            }

            internal double GetTransform(int y, int x) {

                return this.transformMatrix[y][x];
            }

            internal void SetTransform(double x, double y) {

                transformMatrix[0][0] = x;
                transformMatrix[0][1] = y;

                //Console.WriteLine("Camera Transform Set!");
                //Updates Frustrum with new coords
                SetFrustrum(x, y);
                OnPropertyChanged("transformMatrix");
            }
            //Input should be canvas actualWidth
            protected void SetFrustrum(double x, double y) {

                //Max
                frustrumMatrix[0] = (transformMatrix[0][0] + margin) + x;
                frustrumMatrix[1] = (transformMatrix[0][1] + margin) + y;

                //Min
                frustrumMatrix[2] = (transformMatrix[0][0] - margin) + x;
                frustrumMatrix[3] = (transformMatrix[0][1] - margin) + y;

                //Console.WriteLine("Camera Frustrum Set!");
            }

            internal double GetFrustrum(int pos) {
                return this.frustrumMatrix[pos];
            }

            public Point OffsetPosition(Point pos, bool invert = false) {

                if (!invert) {

                    pos.X = pos.X - this.GetTransform(0, 0);
                    pos.Y = pos.Y - this.GetTransform(0, 1);
                }
                else {

                    pos.X = pos.X + this.GetTransform(0, 0);
                    pos.Y = pos.Y + this.GetTransform(0, 1);
                }


                return pos;
            }

    }
}


