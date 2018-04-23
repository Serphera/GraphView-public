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

using GameEditor_GraphView.View;
using GameEditor_GraphView.ViewModel;
using GameEditor_GraphView.Model;


using System.Windows.Interop;

using System.Threading;

namespace GameEditor_GraphView.View {


    /// <summary>
    /// Interaction logic for GraphView_MainView.xaml
    /// </summary>
    public partial class GraphView_MainView : UserControl {

        //public Canvas GraphView { get; set; }
        public GraphView_CurveView GraphView { get; set; }
        public GraphView_MenuView MenuView { get; private set; }


        public GraphView_MainView() {

            InitializeComponent();

            //GraphView = new Canvas();
            GraphView = new GraphView_CurveView();
            GraphView.ClipToBounds = true;
            MenuView = new GraphView_MenuView(this);

            GraphMainGrid.Children.Add(MenuView);
            Grid.SetRow(MenuView, 0);

            GraphMainGrid.Children.Add(GraphView);
            Grid.SetRow(GraphView, 1);

            GraphView.MouseMove += MenuView.sodoff;
            this.Loaded += GraphView_CurveView_Loaded;
        }



        private void GraphView_CurveView_Loaded(object sender, RoutedEventArgs e) {

            Window window = Window.GetWindow(sender as UserControl);
            IntPtr handle = new WindowInteropHelper(window).Handle;
            HwndSource source = HwndSource.FromHwnd(handle);
            source.AddHook(new HwndSourceHook(WndProc));
        }

        private const int WM_ENTERSIZEMOVE = 0x0231;
        private const int WM_EXITSIZEMOVE = 0x0232;
        private bool resizing = false;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {

            if (msg == WM_ENTERSIZEMOVE) {

                resizing = true;
            }
            if (msg == WM_EXITSIZEMOVE) {

                if (resizing) {

                    SizeChangedArgs e = new SizeChangedArgs(new Size(GraphView.ActualWidth, GraphView.ActualHeight));
                    ResizeEnd(this, e);
                    resizing = false;
                }
            }

            return IntPtr.Zero;
        }


        public delegate void SizeChangeHandler(object sender, SizeChangedArgs e);
        public event SizeChangeHandler ResizeEnd;




    }
}
