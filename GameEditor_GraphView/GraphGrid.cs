using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows;

namespace GameEditor_GraphView {
    class GraphGrid {

        private List<FrameworkElement> item;
        private List<FrameworkElement> _vNumbers;
        private List<FrameworkElement> _hNumbers;

        public List<FrameworkElement> Item { get { return this.item; } }
        public List<FrameworkElement> vNumbers { get { return this._vNumbers; } }
        public List<FrameworkElement> hNumbers { get { return this._hNumbers; } }

        public GraphGrid (List<FrameworkElement> items, List<FrameworkElement> hList, List<FrameworkElement> vList) {

            item = items;
            _vNumbers = vList;
            _hNumbers = hList;
        }


        public GraphGrid ScaleAdjusted(double scale, TestingCamera cam = null) {

            //TODO: switch to this when camera is hooked up
            //GraphGrid scaledGrid = new GraphGrid(DrawGrid.Draw(cam.GetMatrix, 1000, 1000, scale));

            GraphGrid scaledGrid = null; //new GraphGrid(DrawGrid.Draw(new double[][] { new double[]{ 0, 0, 0}, new double[] { 0, 0, 0} }, 5000 / scale, 5000 / scale, scale));

            return scaledGrid;
        }
    }
}
