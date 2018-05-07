using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorLibrary {

    public class ProjectStateManager {

        private static ProjectStateManager instance;

        public List<StateManager> StateManagers { get; set; }

        public ProjectStateManager() {}

        public static ProjectStateManager Instance {

            get {
                if (instance == null) {

                    Console.WriteLine("Project State Manager Constructed");
                    instance = new ProjectStateManager();
                    instance.StateManagers = new List<StateManager>();
                }

                return instance;
            }
        }
    }
}
