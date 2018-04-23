using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace GameEditor_GraphView {
    class GraphPropertyChangedEventArgs : PropertyChangedEventArgs {

        private int _pos;
        public GraphPropertyChangedEventArgs(string propertyName, int pos) : base(propertyName) {

            _pos = pos;
        }
    }
}
