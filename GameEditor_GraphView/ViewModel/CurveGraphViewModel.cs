using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows;
using System.Windows.Documents;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Collections.Specialized;
using System.Timers;


using EditorLibrary;

using GameEditor_GraphView;
using GameEditor_GraphView.Model;

namespace GameEditor_GraphView.ViewModel {

    class CurveGraphViewModel : ViewModelBase {


        internal Canvas _Canvas;
        private Camera camera;
        private CurveGraphModel modelitems;
        private ObservableCollection<FrameworkElement> items;       
        

        private Point oldPosition;


        private double[] dragOrigin = new double[4];
        public double[] dragEnd = new double[2];   
        

        private Rectangle dragBox;
        private DateTime _time;        
        private object draggedItem;
        private int dragDirection;
        private int dragIndex = 0;
        private bool _dragDropped = false;
        private bool _DragSelection = false;


        private System.Timers.Timer _scrollTimer;
        private bool _timerRunning = false;


        private bool MiddleMouseUsed = false;


        private bool _alt = false;
        private bool _ctrl = false;
        private bool _shift = false;


        private List<Adorner> adornerArray = new List<Adorner>();
        private List<Point> movementQueue;
        private List<Point> selectedPos = new List<Point>();
        private ObservableCollection<Tuple<int, int>> selectedItems = new ObservableCollection<Tuple<int, int>>();


        public CurveGraphModel ModelItems { get { return modelitems; } set { modelitems = value; } }

        public Camera Camera { get { return camera; } set { camera = value; } }


        public ObservableCollection<FrameworkElement> Items {

            get {

                return items;
            }

            set {

                items = value;
            }
        }

        public event EventHandler UpdateRender;
        public delegate void EventHandler(object sender, EventArgs e);


        public CurveGraphViewModel() {

            items = new ObservableCollection<FrameworkElement>();
            movementQueue = new List<Point>();
            selectedItems.CollectionChanged += RenderAdorner;            
        }


        public void Add(List<FrameworkElement> list) {

            if (Items != null) {

                Items.Clear();
            }
            else {

                Items = new ObservableCollection<FrameworkElement>();
                modelitems = new CurveGraphModel();
            }

            foreach (FrameworkElement item in list) {

                Items.Add(item);
            }
        }


        public void Add(Shape shape) {

            items.Add(shape);
        }


        public void Add(CurveGraphModel model) {

            if (Items != null) {

                Items.Clear();
                modelitems = null;
            }
            else {

                Items = new ObservableCollection<FrameworkElement>();
                modelitems = new CurveGraphModel();
            }

            modelitems = model;

            GraphGrid grid = DrawGrid.RenderGrid(model, camera);
            model.Item.Grid = grid;

            if (UpdateRender != null) {

                UpdateRender(this, EventArgs.Empty);
            }
            
        }



        //TODO: Fix adorners not being drawn correctly after moving if scale != 1 or being redrawn on scale change

        /// <summary>
        /// Adds adorner to items in list
        /// </summary>
        /// <param name="modifiedList">List of item Points</param>
        private void RenderAdorner(object sender , NotifyCollectionChangedEventArgs e) {

            if (e.Action == NotifyCollectionChangedAction.Add && selectedItems.Count > 0) {

                ClearAdorner();

                Point pos;
                var item = Items[0];

                if (selectedPos.Count > 1) { pos = LerpMath.CalculateAverage(selectedPos); }
                else { pos = selectedPos[0]; }

                var myAdornerLayer = AdornerLayer.GetAdornerLayer(item);
                RenderDragHandles(item, myAdornerLayer, camera.OffsetPosition(pos, true));

                for (int i = 0; i < selectedPos.Count; i++) {

                    for (int j = 0; j < Items.Count; j++) {

                        if (Items[j] is Rectangle) {

                            var listItem = Items[j];
                            Point itemPos = camera.OffsetPosition(new Point(Canvas.GetLeft(listItem), Canvas.GetTop(listItem)));

                            if (LerpMath.CalculateDelta(selectedPos[i].X, itemPos.X) <= 2.5 && LerpMath.CalculateDelta(selectedPos[i].Y, itemPos.Y) <= 2.5) {

                                RenderSelection(listItem, AdornerLayer.GetAdornerLayer(item));
                                break;
                            }                           
                        }
                    }
                }
            }                
            else {

                ClearAdorner();
            }            
        }


        private void RenderSelection(FrameworkElement element, AdornerLayer layer) {

            SelectedItemAdorner adorner = new SelectedItemAdorner(element);
            layer.Add(adorner);
            adornerArray.Add(adorner);
        }


        private void RenderDragHandles(FrameworkElement element, AdornerLayer layer, Point position, int type = 0) {

            List<Adorner> list = new List<Adorner>();
            list.Add(new ToolAdorner(element, this, position, "cross"));
            list.Add(new ToolAdorner(element, this, position, "left", type));
            list.Add(new ToolAdorner(element, this, position, "top", type));
            list.Add(new ToolAdorner(element, this, position, "right", type));
            list.Add(new ToolAdorner(element, this, position, "bottom", type));

            for (int i = 0; i < list.Count; i++) {
                
                layer.Add(list[i]);
                adornerArray.Add(list[i]);
            }
        }


        /// <summary>
        /// Clears all adorners
        /// </summary>
        internal void ClearAdorner() {

            Console.WriteLine("clearing");
            for (int i = 0; i < Items.Count; i++) {

                if (AdornerLayer.GetAdornerLayer(Items[i]) != null) {

                    var adornerLayer = AdornerLayer.GetAdornerLayer(Items[i]);

                    for (int j = 0; j < adornerArray.Count; j++) {

                        adornerLayer.Remove(adornerArray[j]);
                    }
                }
            }
        }


        internal void ClearItems() {

            selectedItems.Clear();
            selectedPos.Clear();
        }
                
        
        /// <summary>
        /// Calculates delta movement
        /// </summary>
        /// <param name="model"></param>
        private void CalculatePosition(CurveGraphModel model) {

            Point dest;

            if (!MiddleMouseUsed) {

                dest = LerpMath.CalculateDelta(new Point(dragOrigin[0], dragOrigin[1]), new Point(dragEnd[0], dragEnd[1]), false, true);
            }
            else {

                dest = LerpMath.CalculateDelta(new Point(dragOrigin[2], dragOrigin[3]), new Point(dragEnd[0], dragEnd[1]), false, true);
                double offsetMultiplier = 0.55;

                dest = new Point(
                    (dest.X * offsetMultiplier),
                    (dest.Y * offsetMultiplier)
                    );

                MiddleMouseUsed = false;
                dragDirection = 0;
            }

            movementQueue.Add(new Point(dest.X, dest.Y));
        }


        /// <summary>
        /// Applies delta movement to items
        /// </summary>
        /// <param name="model"></param>
        /// <param name="modifiedList">List of items to be moved</param>
        private void ApplyMovement(ref CurveGraphModel model, ref List<Point> modifiedList) {

            for (int j = 0; j < 1; j++) {

                Point dest = movementQueue[j];                

                for (int i = 0; i < selectedItems.Count; i++) {

                    double[] original = new double[2];

                    var curveNr = Convert.ToInt32(selectedItems[i].Item1);
                    var pointNr = Convert.ToInt32(selectedItems[i].Item2);

                    original[0] = model.Item.Curve[curveNr].Points[pointNr].X;
                    original[1] = model.Item.Curve[curveNr].Points[pointNr].Y;

                    CheckDirection(original, ref dest);

                    Point dest2 = new Point(original[0] + dest.X, original[1] + dest.Y);
                    modifiedList.Add(dest2);
                    selectedPos[i] = dest2;

                    model.Item.Curve[curveNr].Points[pointNr] = dest2;  
                }
            }
            movementQueue.Clear();
        }


        /// <summary>
        /// Checks if handle movement is vertical or horizontal
        /// </summary>
        /// <param name="original"></param>
        /// <param name="dest"></param>
        private void CheckDirection(double[] original, ref Point dest) {
            
            if (dragDirection == 1) { dest.Y = 0; }
            else if (dragDirection == 2) { dest.X = 0; }
        }


        /// <summary>
        /// Checks keyboard modifier keys
        /// </summary>
        /// <returns></returns>
        private void CheckModifier() {

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) {

                _shift = true;

                if (Keyboard.IsKeyDown(Key.RightAlt) && Keyboard.IsKeyDown(Key.LeftAlt)) { _alt = true; }
            }

            if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)) { _alt = true; }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) { _ctrl = true; }
        }


        //Calculates end point for object movement
        public void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e) {

            _Canvas = sender as Canvas;

            dragEnd[0] = e.GetPosition(_Canvas).X;
            dragEnd[1] = e.GetPosition(_Canvas).Y;

            if (_DragSelection) {

                if (dragBox != null) {

                    double width = LerpMath.CalculateDelta(dragOrigin[0], dragEnd[0]);
                    double height = LerpMath.CalculateDelta(dragOrigin[1], dragEnd[1]);

                    width = (width < 0) ? width * -1 : width;
                    height = (height < 0) ? height * -1 : height;

                    dragBox.Width = width;
                    dragBox.Height = height;

                    double offsetX = (dragOrigin[0] < dragEnd[0]) ? dragOrigin[0] : dragEnd[0];
                    double offsetY = (dragOrigin[1] < dragEnd[1]) ? dragOrigin[1] : dragEnd[1];

                    Canvas.SetLeft(dragBox, offsetX);
                    Canvas.SetTop(dragBox, offsetY);
                }
                else {

                    Rectangle rect = new Rectangle();
                    rect.IsHitTestVisible = false;
                    rect.Fill = new SolidColorBrush(Colors.Blue);
                    rect.Opacity = 0.4;
                    rect.Name = "dragArea";

                    rect.Width = LerpMath.CalculateDelta(dragOrigin[0], dragEnd[0]);
                    rect.Height = LerpMath.CalculateDelta(dragOrigin[1], dragEnd[1]);

                    Items.Add(rect);
                    dragBox = rect;

                    Canvas.SetLeft(rect, dragOrigin[0]);
                    Canvas.SetTop(rect, dragOrigin[1]);

                    dragIndex = Items.Count - 1;
                }             
            }
        }
        

        public override void OnMouseDown(object sender, MouseButtonEventArgs e) {

            CheckModifier();

            switch (GraphView_ToolHandler.ActiveTool) {

                //Select tool
                case 0:

                    //Toolhandle
                    if (e.ChangedButton == MouseButton.Left && e.OriginalSource.GetType() == typeof(ToolAdorner)) {

                        ToolAdorner rect = (ToolAdorner)e.OriginalSource;

                        if (Regex.IsMatch(rect.Name, "(left)?(right)?(top)?(bottom)?")) {

                            dragDirection = (rect.Name == "left" || rect.Name == "right") ? dragDirection = 1 : dragDirection = 2;

                            if (selectedPos.Count > 1) {

                                Point origin = LerpMath.CalculateAverage(selectedPos);
                                dragOrigin[0] = origin.X;
                                dragOrigin[1] = origin.Y;
                            }
                            else {

                                dragOrigin[0] = e.GetPosition(_Canvas).X;
                                dragOrigin[1] = e.GetPosition(_Canvas).Y;
                            }

                            draggedItem = rect;
                            return;
                        }
                        return;
                    }

                    //Drag select
                    if (e.ChangedButton == MouseButton.Left && !(e.OriginalSource.GetType() == typeof(Rectangle) &&
                        !(e.OriginalSource.GetType() == typeof(ToolAdorner)))) {

                        _DragSelection = true;
                        _time = DateTime.Now;

                        dragOrigin[0] = e.GetPosition(_Canvas).X;
                        dragOrigin[1] = e.GetPosition(_Canvas).Y;

                        return;
                    }

                    //move camera
                    if (_alt && e.ChangedButton == MouseButton.Middle) {

                        _alt = false;
                        oldPosition = Mouse.GetPosition(Application.Current.MainWindow);
                        return;
                    }

                    //drag move
                    if (e.ChangedButton == MouseButton.Middle && selectedItems.Count > 0) {

                        dragOrigin[2] = Mouse.GetPosition(Application.Current.MainWindow).X;
                        dragOrigin[3] = Mouse.GetPosition(Application.Current.MainWindow).Y;

                        MiddleMouseUsed = true;

                        return;
                    }
                    break;

                //Insert Point Tool
                case 1:

                    if (e.ChangedButton == MouseButton.Left) {

                        Point pos = camera.OffsetPosition(e.GetPosition(_Canvas));

                        CurveGraphModel model = ModelItems;
                        // TODO: Need to find ways to separate curves from each other
                        //BezierCurve curve = model.Item.Curve;

                        //curve.InsertPoint(pos);

                        //Add(model);
                    }
                    break;

                default:
                    break;
            }

           
        }


        public void OnMouseUp(object sender, MouseButtonEventArgs e) {

            List<Point> modifiedList = new List<Point>();

            switch (GraphView_ToolHandler.ActiveTool) {

                case 0:

                    if (dragIndex < Items.Count && Items[dragIndex].Name == "dragArea") { Items.RemoveAt(dragIndex); }

                    if (_DragSelection) {

                        _dragDropped = true;
                        _DragSelection = false;
                        dragBox = null;
                    }

                    //Move
                    if (draggedItem != null || MiddleMouseUsed || oldPosition.X != 0) {

                        CurveGraphModel model = new CurveGraphModel();
                        model = modelitems;

                        if (oldPosition.X != 0) {

                            Point delta = LerpMath.CalculateDelta
                                (
                                oldPosition, 
                                Mouse.GetPosition(Application.Current.MainWindow), 
                                true
                                );

                            double x = camera.GetTransform(0, 0);
                            double y = camera.GetTransform(0, 1);

                            camera.SetTransform(x + delta.X, y + delta.Y);
                            oldPosition.X = 0;
                        }

                        CalculatePosition(model);
                        ApplyMovement(ref model, ref modifiedList);
                        Add(model);

                        draggedItem = null;
                    }
                    //Selection
                    else {

                        HandleSelection(sender, e);                       
                    }

                    modifiedList.Clear();
                    break;

                case 1:

                    ClearItems();
                    break;

                default:
                    break;
            }


        }


        private void HandleSelection(object sender, MouseButtonEventArgs e) {

            //If rectangle hit
            if (e.OriginalSource.GetType() == typeof(Rectangle)) {
                                
                if (selectedItems.Count > 0 && !_shift) { ClearItems(); }

                Rectangle rect = (Rectangle)e.OriginalSource;
                selectedPos.Add(camera.OffsetPosition(new Point(Canvas.GetLeft(rect), Canvas.GetTop(rect))));

                if (rect != null && String.IsNullOrEmpty(rect.Name)) {

                    var myAdornerLayer = AdornerLayer.GetAdornerLayer(rect);

                    if (myAdornerLayer != null) {

                        if (selectedPos.Count > 1) {

                            Point avgPosition = LerpMath.CalculateAverage(selectedPos);

                            Point offset = LerpMath.CalculateDelta(
                                modelitems.Item.Curve[selectedItems[0].Item1].Points[selectedItems[0].Item2],
                                avgPosition,
                                true
                                );
                        }
                        else {

                            RenderSelection(rect, myAdornerLayer);
                        }

                        var matches = Regex.Matches(rect.Tag.ToString(), "[0-9]{1,3}");

                        if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift)) {

                            ClearItems();
                            selectedPos.Add(camera.OffsetPosition(new Point(Canvas.GetLeft(rect), Canvas.GetTop(rect))));
                        }

                        selectedItems.Add(new Tuple<int, int>(Convert.ToInt32(matches[0].ToString()), Convert.ToInt32(matches[1].ToString())));
                    }
                }

                _shift = false;
            }

            //Drag selection
            else if (_dragDropped && (3 * camera.GetScale) < LerpMath.CalculateDelta(dragOrigin[0], dragEnd[0]) 
                || (3 * camera.GetScale) < LerpMath.CalculateDelta(dragOrigin[1], dragEnd[1])) {

                if ((DateTime.Now - _time).Milliseconds > 100) {

                    if (!_shift) { ClearItems(); }

                    _shift = false;

                    for (int j = 0; j < modelitems.Item.Curve.Count; j++) {

                        var list = modelitems.Item.Curve[j].Original;

                        double minX = (dragOrigin[0] < dragEnd[0]) ? dragOrigin[0] : dragEnd[0];
                        double minY = (dragOrigin[1] < dragEnd[1]) ? dragOrigin[1] : dragEnd[1];

                        double maxX = (dragOrigin[0] > dragEnd[0]) ? dragOrigin[0] : dragEnd[0];
                        double maxY = (dragOrigin[1] > dragEnd[1]) ? dragOrigin[1] : dragEnd[1];


                        for (int i = 0; i < _Canvas.Children.Count; i++) {

                            if (_Canvas.Children[i] is Rectangle) {

                                Rectangle rect = (Rectangle)_Canvas.Children[i];

                                if ((minX - 3) < Canvas.GetLeft(rect) && (maxX + 3) > Canvas.GetLeft(rect)) {

                                    if ((minY - 3) < Canvas.GetTop(rect) && (maxY + 3) > Canvas.GetTop(rect)) {

                                        var matches = Regex.Matches(rect.Tag.ToString(), "[0-9]{1,3}");
                                        Tuple<int, int> curvePointNr = new Tuple<int, int>(Convert.ToInt32(matches[0].ToString()), Convert.ToInt32(matches[1].ToString()));
                                        bool exists = false;

                                        if (selectedItems.IndexOf(curvePointNr) != -1) { exists = true; }

                                        if (!exists) {

                                            selectedPos.Add(camera.OffsetPosition(new Point(Canvas.GetLeft(rect), Canvas.GetTop(rect))));
                                            selectedItems.Add(curvePointNr);

                                            dragOrigin[0] = Canvas.GetLeft(rect);
                                            dragOrigin[1] = Canvas.GetTop(rect);

                                            var myAdornerLayer = AdornerLayer.GetAdornerLayer(rect);
                                            RenderSelection(rect, myAdornerLayer);
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }
            //Remove selection
            else {

                ClearItems();
                draggedItem = null;
            }
        }


        public void OnMouseWheel(object sender, MouseWheelEventArgs e ) {

            
            if (!_timerRunning) {
                
                if (camera.GetScale <= 0.01 && e.Delta > 0) {

                    camera.GetScale = 0.01;
                    return;
                }
                
                StartTimer();
            }
            if (camera.GetScale > 0.01) {

                if (_timerRunning) {

                    scrollScale = (e.Delta < 0) ? scrollScale * 1.1 : scrollScale * 0.9;
                    _scrollTimer.Dispose();
                    StartTimer();
                    return;
                }                
            }                    
        }

        private void StartTimer() {

            //Console.WriteLine("\n spawning timer \n");
            _timerRunning = true;
            _scrollTimer = new System.Timers.Timer();
            _scrollTimer.Interval = 90;
            _scrollTimer.Start();
            _scrollTimer.Elapsed += OnWheelTimerElapsed;
        }

        private double scrollScale = 1;

        private void OnWheelTimerElapsed(object sender, ElapsedEventArgs e) {

            _scrollTimer.Stop();
            _timerRunning = false;
            camera.GetScale = scrollScale;
            Application.Current.Dispatcher.Invoke(() => Add(modelitems));

        }



    }
}
