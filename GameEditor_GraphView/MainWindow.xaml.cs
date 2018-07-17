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
using System.ComponentModel;
using System.Windows.Interop;


using GameEditor_GraphView.View;
using GameEditor_GraphView.ViewModel;
using GameEditor_GraphView.Model;

using EditorLibrary;


namespace GameEditor_GraphView {
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        //TODO: Integrate functionality into main GameEditor MainWindow

        ProjectStateManager ProjectStateManager;

        public MainWindow() {

            InitializeComponent();

            ProjectStateManager = ProjectStateManager.Instance;

            GraphView_StateManager graphView_StateManager = new GraphView_StateManager();
            ProjectStateManager.StateManagers.Add(graphView_StateManager);

            GraphViewMainGrid.Children.Add(graphView_StateManager.View);
            Closing += graphView_StateManager.Dispose;
        }

    }
}
