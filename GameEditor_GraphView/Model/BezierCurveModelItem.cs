using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EditorLibrary;

namespace GameEditor_GraphView.Model
{
    class BezierCurveModelItem {

        private List<BezierCurve> _curve;
        private int _totalCount;

        public bool ScaleChanged { get; set; }

        public List<BezierCurve> Curve { get { return _curve; } private set { _curve = value; } }

        public int GetCount { get { return _totalCount; } }

        public GraphGrid Grid { get; set; }
        public double[][] Matrix { get; set; }


        public BezierCurveModelItem(BezierCurve curve, GraphGrid grid, double[][] cameraMatrix) {

            _curve = new List<BezierCurve>();
            _curve.Add(curve);
            Grid = grid;
            Matrix = cameraMatrix;
            _totalCount = curve.Points.Count;
        }


#if DEBUG
#endif
        public BezierCurveModelItem(BezierCurve curve) {

            _curve = new List<BezierCurve>();
            _curve.Add(curve);
            _totalCount = curve.Points.Count;
        }

        public void Add(BezierCurve curve) {

            _curve.Add(curve);
            _totalCount += curve.Points.Count;
        }
        
        public void Replace(BezierCurve item, int index) {

            //Curve = item;
            throw new NotImplementedException();
        }
    }
}
