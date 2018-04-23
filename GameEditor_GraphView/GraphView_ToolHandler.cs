using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEditor_GraphView {
    class GraphView_ToolHandler : ToolHandler {


        //0: Select
        //1: Insert Point
        public GraphView_ToolHandler() {

            this.name = "GraphView";
            ActiveTool = 0;
        }

        public void SetTool(int id) {

            ActiveTool = id;
        }
    }
}
