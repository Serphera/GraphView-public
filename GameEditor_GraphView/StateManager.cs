using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace GameEditor_GraphView {
   public class StateManager {

        public UserControl View { get; private set; }
        public ViewModelBase ViewModel { get; private set; }

        public StateManager() {

        }

        protected void Configure(UserControl view, ViewModelBase viewModel) {

            this.View = view;
            this.ViewModel = viewModel;
        }
    }
}
