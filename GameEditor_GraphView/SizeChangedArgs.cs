using System;
using System.Windows;

namespace GameEditor_GraphView.View {
    public class SizeChangedArgs : EventArgs {

        public Size NewSize { get; private set; }

        public SizeChangedArgs(Size _size) {

            NewSize = _size;
        }
    }
}