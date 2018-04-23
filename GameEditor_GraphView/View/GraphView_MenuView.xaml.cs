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
using GameEditor_GraphView.ViewModel;

namespace GameEditor_GraphView.View {


    /// <summary>
    /// Interaction logic for GraphView_MenuView.xaml
    /// </summary>
    public partial class GraphView_MenuView : UserControl {

        private GraphView_MainView _View;


        public GraphView_MenuView(GraphView_MainView view) {
            InitializeComponent();

            _View = view;
        }

        
        public void sodoff(object sender, MouseEventArgs e) {

            GraphView_StateManager manager = ProjectStateManager.Instance.StateManagers.OfType<GraphView_StateManager>().First();
            CurveGraphViewModel vm = ((CurveGraphViewModel)manager.ViewModel);

            if (vm == null) {
                return;
            }
            Point pos = e.GetPosition(sender as Canvas);

            pos.X -= vm.Camera.GetTransform(0, 0);
            pos.Y -= vm.Camera.GetTransform(0, 1);

            double value = 0;

            for (int i = 0; i < vm.ModelItems.Item.Curve.Points.Count; i++) {

                if (i != 0 && pos.X < vm.ModelItems.Item.Curve.Points[i].X) {

                    Point point1 = vm.ModelItems.Item.Curve.Points[i - 1];
                    Point point2 = vm.ModelItems.Item.Curve.Points[i];

                    value = MathLerp.Lerp(point1, point2, pos.X / point2.X).Y;
                    break;
                }
            }

            double[] camCoords = new double[] {
                vm.Camera.GetTransform(0, 0),
                vm.Camera.GetTransform(0, 1)
            };
            double scale = vm.Camera.GetScale;
            
            xLabel.Content = String.Format("Mouse: {0:N2} {1:N2} | Camera: {2:N2} {3:N2} | Scale: {4:N2} |  Curve value: {5:N2}",
                (e.GetPosition(_View.GraphView).X - camCoords[0]) * scale,
                (e.GetPosition(_View.GraphView).Y - camCoords[1]) * scale,
                camCoords[0],
                camCoords[1],
                scale,
                value
                );
                
        }
        

        private void Button_Click(object sender, RoutedEventArgs e) {

            var manager = ProjectStateManager.Instance.StateManagers.OfType<GraphView_StateManager>().First();
            var vm = ((CurveGraphViewModel)manager.ViewModel);
            var camera = vm.Camera;

            switch (((Button)sender).Content) {

                case "left":

                    vm.Camera.SetTransform(camera.GetTransform(0, 0) - 10, camera.GetTransform(0, 1));
                    break;

                case "right":

                    vm.Camera.SetTransform(camera.GetTransform(0, 0) + 10, camera.GetTransform(0, 1));
                    break;

                case "up":

                    vm.Camera.SetTransform(camera.GetTransform(0, 0), camera.GetTransform(0, 1) + 10);
                    break;

                case "down":

                    vm.Camera.SetTransform(camera.GetTransform(0, 0), camera.GetTransform(0, 1) - 10);
                    break;
                default:
                    break;
            }

            manager.ResumeRender();
            
        }

        private void InsertTool_Click(object sender, RoutedEventArgs e) {

            GraphView_ToolHandler.ActiveTool = 1;
        }

        private void SelectTool_Click(object sender, RoutedEventArgs e) {

            GraphView_ToolHandler.ActiveTool = 0;
        }
    }
}
