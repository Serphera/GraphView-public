using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


using SharpDX.Direct2D1;
using System.Windows.Interop;


namespace GameEditor_GraphView.View {
    /// <summary>
    /// Interaction logic for GraphView_CurveView.xaml
    /// </summary>
    public partial class GraphView_CurveView : UserControl {

        public Canvas _canvas;

        public GraphView_CurveView() {

            InitializeComponent();
        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e) {

            Console.WriteLine("canvas ready");
            _canvas = (Canvas)e.OriginalSource;
        }

        private void Canvas_Initialized(object sender, EventArgs e) {

            Console.WriteLine("canvas ready");
            _canvas = (Canvas)sender;
        }

    }
}
