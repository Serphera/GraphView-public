using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Shapes;

namespace GameEditor_GraphView {
    class SelectedItemAdorner : Adorner {

        VisualCollection visualChildren;

        public SelectedItemAdorner(UIElement adornedElement) : base(adornedElement) {

            visualChildren = new VisualCollection(this);
        }

        protected override void OnRender(DrawingContext drawingContext) {

            SolidColorBrush renderBrush = new SolidColorBrush(Colors.Red);
            renderBrush.Opacity = 0.8;


            Rect adornedRect = new Rect(this.AdornedElement.DesiredSize);

            drawingContext.DrawLine(new Pen(renderBrush, 1.0), adornedRect.TopLeft, adornedRect.TopRight);
            drawingContext.DrawLine(new Pen(renderBrush, 1.0), adornedRect.TopRight, adornedRect.BottomRight);
            drawingContext.DrawLine(new Pen(renderBrush, 1.0), adornedRect.BottomRight, adornedRect.BottomLeft);
            drawingContext.DrawLine(new Pen(renderBrush, 1.0), adornedRect.BottomLeft, adornedRect.TopLeft);
        }
    }
}
