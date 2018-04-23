using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEditor_GraphView.Model
{
    class BezierCurveModelItemV2
    {

        private BezierCurveV2 curve;

        public bool ScaleChanged { get; set; }

        public BezierCurveV2 Curve {
            get { return this.curve; }
            private set { this.curve = value; }
        }

        public GraphGrid Grid { get; set; }
        public double[][] Matrix { get; set; }


        public BezierCurveModelItemV2(BezierCurveV2 curve, GraphGrid grid, double[][] cameraMatrix) {

            Curve = curve;
            Grid = grid;
            Matrix = cameraMatrix;
        }


#if DEBUG
#endif
        public BezierCurveModelItemV2(BezierCurveV2 curve) {

            Curve = curve;
        }


        
        public void Replace(BezierCurveV2 item, int index) {

            Curve = item;
        }
    }
}
