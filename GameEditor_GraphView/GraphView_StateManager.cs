using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Timers;

using GameEditor_GraphView.Model;
using GameEditor_GraphView.View;
using GameEditor_GraphView.ViewModel;

using System.Windows.Controls;
using SharpDX.Direct3D11;
using SharpDX;

using System.Threading;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Input;

using EditorLibrary;

namespace GameEditor_GraphView {

    class GraphView_StateManager : StateManager {

        private GraphView_ToolHandler ToolHandler;
        private D3Drenderer _D3Drenderer;
        private IntPtr viewHandle;

        private Thread renderThread;

        private ManualResetEvent mre;

        public IntPtr ViewHandle { get { return viewHandle; } }



        public GraphView_StateManager() {

            Console.WriteLine("GraphView state manager constructed");
            CreateGraphView();
        }


        public void Configure(UserControl view, CurveGraphViewModel viewModel) {

            base.Configure(view, viewModel);
            ToolHandler = new GraphView_ToolHandler();

            ((GraphView_MainView)view).ResizeEnd += View_SizeChanged;
            viewModel.UpdateRender += ResumeRender;

            SetContext();
        }

        private void SetContext() {

            ((GameEditor_GraphView.ViewModel.CurveGraphViewModel)ViewModel)._Canvas = ((GraphView_MainView)View).GraphView._canvas;
            ((GraphView_MainView)View).GraphView.DataContext = (GameEditor_GraphView.ViewModel.CurveGraphViewModel)ViewModel;
        }


        public void StartD2D(GraphView_CurveView view) {

            
            renderThread = new Thread(() => {
                _D3Drenderer = new D3Drenderer(this, view);
                _D3Drenderer.Render(this, view);
                }
            );

            renderThread.SetApartmentState(ApartmentState.STA);
            renderThread.Start(); 
        }


        /// <summary>
        /// Sets up reference to renderThreads resetevent
        /// </summary>
        /// <param name="e"></param>
        public void SetupResetEvent(ref ManualResetEvent e) {

            mre = e;
        }



        public void RefRenderer(D3Drenderer renderer) {

            _D3Drenderer = renderer;
        }


        private void PauseRender() {

            if (_D3Drenderer != null) {

                mre.Reset();
            }
        }


        private void ResumeRender(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {

            mre.Set();
        }

        private void ResumeRender(object sender, EventArgs e) {

            mre.Set();
        }


        public void ResumeRender() {

            mre.Set();
        }

        /// <summary>
        /// Gets Bitmap from source
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public Bitmap GetBitmap(BitmapSource source) {

            unsafe {

                Bitmap bmp = new Bitmap(
                    source.PixelWidth,
                    source.PixelHeight,
                    PixelFormat.Format32bppArgb);

                BitmapData data = bmp.LockBits(
                  new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
                  ImageLockMode.WriteOnly,
                  PixelFormat.Format32bppPArgb);

                source.CopyPixels(
                  Int32Rect.Empty,
                  data.Scan0,
                  data.Height * data.Stride,
                  data.Stride);

                bmp.UnlockBits(data);
                source = null;
                data = null;

                
                return bmp;
            }
        }

        /// <summary>
        /// Bitmap to Image
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        BitmapImage BitmapToImageSource(Bitmap bitmap) {

            BitmapImage bitmapimage = new BitmapImage();
            //PixelFormat format = PixelFormat.Format32bppPArgb;
            using (MemoryStream memory = new MemoryStream()) {

                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();                
            }

            return bitmapimage;
        }


        //public void SetupView(SharpDX.Direct2D1.Bitmap.PixelSize bitmap, byte[] data, List<System.Windows.Point> list) {
        public void SetupView(int height, int width, float dpiHeight, float dpiWidth, byte[] data, List<List<System.Windows.Point>> list) {

            if (data == null) { return; }

            var source = BitmapSource.Create(
              
                width,
                height,
                dpiWidth,
                dpiHeight,
                System.Windows.Media.PixelFormats.Bgra32, 
                null,
                data,
                4 * width
            );

            var image = GetBitmap(source);
            data = null;
            source = null;
            
            System.Windows.Media.Effects.BlurEffect blur = new System.Windows.Media.Effects.BlurEffect();
            blur.Radius = 0.5f;           
            
            App.Current.Dispatcher.Invoke(() => UpdateGraphView(image, list));
        }


        public void UpdateGraphView(Bitmap image, List<List<System.Windows.Point>> pList) {

            
            SetContext();
            var view = ((GraphView_MainView)View).GraphView;
            var final = new System.Windows.Controls.Image();
            final.Source = BitmapToImageSource(image);

            System.Windows.Media.Effects.BlurEffect blur = new System.Windows.Media.Effects.BlurEffect();
            blur.Radius = 2f;

            final.Effect = blur;
            var list = new List<FrameworkElement>();
            list.Add(final);

            for (int i = 0; i < pList.Count; i++) {

                list.AddRange(DrawInteractiveUI.DrawRectangles((CurveGraphViewModel)ViewModel, pList[i], i));
            }
            

            ((GameEditor_GraphView.ViewModel.CurveGraphViewModel)ViewModel).Add(list);

            final = null;
            list.Clear();
            pList.Clear();
            image.Dispose();
            PauseRender();
        }


        public void SetViewHandle(IntPtr handle) {

            viewHandle = handle;
        }


        const float ratio = 1.25f;


        private void View_SizeChanged(object sender, SizeChangedArgs e) {

            // Ensures that RenderForm dimensions are never below 1 px
            var width = (((int)e.NewSize.Width) > 1) ? (int)e.NewSize.Width : 1;
            var height = (((int)e.NewSize.Height) > 1) ? (int)e.NewSize.Height : 1;

            //_D3Drenderer.MessageQueue.Enqueue(new Tuple<int, int>(width, height));
            _D3Drenderer.MessageQueue.Push(new Tuple<int, int>(width, height));
            ResumeRender();
        }

        public void CreateGraphView() {

            List<System.Windows.Point> pList = new List<System.Windows.Point>();
            List<System.Windows.Point> pList2 = new List<System.Windows.Point>();

            GraphView_MainView _GraphView = new GraphView_MainView();
            CurveGraphViewModel _GraphViewModel = new CurveGraphViewModel();

            pList.Add(new System.Windows.Point(0, 0));
            pList.Add(new System.Windows.Point(250, 400));
            pList.Add(new System.Windows.Point(500, 200));
            pList.Add(new System.Windows.Point(700, 400));
            pList.Add(new System.Windows.Point(900, 350));

            pList2.Add(new System.Windows.Point(0, 100));
            pList2.Add(new System.Windows.Point(250, 500));
            pList2.Add(new System.Windows.Point(500, 300));
            pList2.Add(new System.Windows.Point(700, 500));
            pList2.Add(new System.Windows.Point(900, 450));

            // TODO: Replace Camera with Camera once integrated with GameEditor
            _GraphViewModel.Camera = new Camera();
            _GraphViewModel.Camera.SetTransform(0, 0);

            BezierCurveModelItem item = new BezierCurveModelItem(new BezierCurve(pList));

            item.Add(new BezierCurve(pList2));

            CurveGraphModel model = new CurveGraphModel();
            model.Item = item;
           

            _GraphViewModel.Add(model);

            Configure(_GraphView, _GraphViewModel);

            StartD2D(_GraphView.GraphView);
        }


        public void Dispose() {

            if (_D3Drenderer != null) {

                _D3Drenderer.Dispose();
                renderThread.Abort();
            }            
        }


        public void Dispose(object sender, EventArgs e) {

            if (_D3Drenderer != null) {

                _D3Drenderer.Dispose();
                renderThread.Abort();
            }
        }



    }
}
