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
using System.Text.RegularExpressions;
using System.Windows.Interop;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Timers;


using GameEditor_GraphView.Model;
using GameEditor_GraphView.ViewModel;

namespace GameEditor_GraphView.View {
    /// <summary>
    /// Interaction logic for CurveGraphView.xaml
    /// </summary>
    public partial class CurveGraphView : UserControl {

        List<Point> pList = new List<Point>();

        public Canvas canvas;
        private Timer _UpdateTimer;
        private CurveGraphViewModel _CurveGraphViewModel = new CurveGraphViewModel();


        //public CurveGraphView(KeyframeList pList) {
        public CurveGraphView() {

            InitializeComponent();

            //TODO: Remove once integrated and input handling is implemented
            //Testing Points

            pList.Add(new Point(0, 0));
            pList.Add(new Point(250, 400));
            pList.Add(new Point(500, 200));
            pList.Add(new Point(700, 400));
            pList.Add(new Point(900, 350));
            //

            //TODO: Replace testingCamera with Camera once integrated with GameEditor
            _CurveGraphViewModel.Camera = new TestingCamera();
            _CurveGraphViewModel.Camera.SetTransform(0, 0);

            BezierCurveModelItemV2 item = new BezierCurveModelItemV2(new BezierCurveV2(pList));

            CurveGraphModel model = new CurveGraphModel();
            model.Item = item;
        
            _CurveGraphViewModel.Add(model);

            //this.DataContext = _CurveGraphViewModel;
            this.Loaded += View_Loaded;
            this.DataContextChanged += View_DataContextChanged;

            
        }


        private void Canvas_Loaded(object sender, RoutedEventArgs e) {

            canvas = (Canvas)e.OriginalSource;
            _CurveGraphViewModel._Canvas = canvas;
        }

        private void Canvas_Initialized(object sender, EventArgs e) {

            canvas = (Canvas)sender;
            _CurveGraphViewModel._Canvas = canvas;
        }


        private void View_Loaded(object sender, RoutedEventArgs e) {


            var window = GetWindowHandle();

            if (window == null) {
                return;
            }
            SetWindowHandle();
        }

        private void View_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {

            SetWindowHandle();
        }

        private void SetWindowHandle() {

            var window = GetWindowHandle();
            Console.WriteLine(window);
            GraphView_StateManager manager = ProjectStateManager.Instance.StateManagers.OfType<GraphView_StateManager>().First();
            manager.SetViewHandle(window);

            if (manager.ViewHandle != IntPtr.Zero) {

                //manager.StartD2D();
            }
            
        }

        private IntPtr GetWindowHandle() {

            var window = Window.GetWindow(this);
            if (window == null) { return IntPtr.Zero; }
            window.Closing += DisposeUnmanagedResources;
            Console.WriteLine("view handle aquired");
            return new WindowInteropHelper(window).Handle;
        }

        private void DisposeUnmanagedResources(object sender, EventArgs e) {

            GraphView_StateManager manager = ProjectStateManager.Instance.StateManagers.OfType<GraphView_StateManager>().First();
            manager.Dispose();
        }
    }
}
