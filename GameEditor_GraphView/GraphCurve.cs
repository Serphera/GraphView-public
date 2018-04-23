using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;

namespace GameEditor_GraphView {
    class GraphCurve : INotifyPropertyChanged {

        private List<Point> pointList;

        public event PropertyChangedEventHandler PropertyChanged;

        public GraphCurve(Point start, Point end) {

            pointList = new List<Point>();

            pointList.Add(start);
            pointList.Add(end);
        }

        public void Add(Point p) {

            pointList.Add(p);
            OnPropertyChanged("Add");
        }

        public void Remove(int pos) {

            pointList.RemoveAt(pos);
            OnPropertyChanged("Remove");
        }

        public void Insert(int pos,Point p) {

            pointList.Insert(pos, p);
            OnPropertyChanged("Insert");
        }

        protected virtual void OnPropertyChanged(string propertyName, int pos = -1) {

            PropertyChanged?.Invoke(this, new GraphPropertyChangedEventArgs(propertyName, pos));
        }


    }
}
