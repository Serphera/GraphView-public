using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EditorLibrary;

namespace GameEditor_GraphView.Model
{
    class BezierCurveModelItem
    {

        private BezierCurve curve;

        public bool ScaleChanged { get; set; }

        public BezierCurve Curve {
            get { return this.curve; }
            private set { this.curve = value; }
        }

        public GraphGrid Grid { get; set; }
        public double[][] Matrix { get; set; }


        public BezierCurveModelItem(BezierCurve curve, GraphGrid grid, double[][] cameraMatrix) {

            Curve = curve;
            Grid = grid;
            Matrix = cameraMatrix;
        }


#if DEBUG
#endif
        public BezierCurveModelItem(BezierCurve curve) {

            Curve = curve;
        }


        
        public void Replace(BezierCurve item, int index) {

            Curve = item;
        }
    }
}
