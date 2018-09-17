using System;
using System.ComponentModel;
using System.Windows;

namespace EditorLibrary {

    [Serializable]
    public class Camera : INotifyPropertyChanged {


        private double[][] transformMatrix;
        private double[] frustrumMatrix;
        private double margin = 1000;
        private double worldScale = 1;

        public Camera(double tX = 0.0, double tY = 0.0, double tZ = 0.0, double rX = 0.0, double rY = 0.0, double rZ = 0.0) {

            transformMatrix =
                new double[][] {

                        new double[] { tX, tY, tZ } ,
                        new double[] { rX, rY, rZ }
                };

            frustrumMatrix = new double[] {

                    tX + margin, tY + margin, tX - margin, tY - margin
                };
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName) {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public double GetSetMargin { get { return margin; } set { margin = value; } }

        public double GetScale { get { return worldScale; } set { worldScale = Math.Round(value, 4); } }

        
        public double GetTransform(int y, int x) {

            return transformMatrix[y][x];
        }


        public void SetTransform(double x, double y) {

            transformMatrix[0][0] = x;
            transformMatrix[0][1] = y;

            //Updates Frustrum with new coords
            SetFrustrum(x, y);
            OnPropertyChanged("transformMatrix");
        }


        //Input should be canvas actualWidth
        public void SetFrustrum(double x, double y) {

            //Max
            frustrumMatrix[0] = (transformMatrix[0][0] + margin) + x;
            frustrumMatrix[1] = (transformMatrix[0][1] + margin) + y;

            //Min
            frustrumMatrix[2] = (transformMatrix[0][0] - margin) + x;
            frustrumMatrix[3] = (transformMatrix[0][1] - margin) + y;
        }


        public double GetFrustrum(int pos) {

            return frustrumMatrix[pos];
        }


        public Point OffsetPosition(Point pos, bool invert = false) {

            if (!invert) {

                pos.X = pos.X - GetTransform(0, 0);
                pos.Y = pos.Y - GetTransform(0, 1);
            }
            else {

                pos.X = pos.X + GetTransform(0, 0);
                pos.Y = pos.Y + GetTransform(0, 1);
            }


            return pos;
        }

    }
}


