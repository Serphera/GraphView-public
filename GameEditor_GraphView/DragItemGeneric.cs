using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEditor_GraphView {
    class DragItemGeneric<T> {

        public T Item { get; set; }

        public DragItemGeneric(T item) {

            Item = item;
        }
    }
}
