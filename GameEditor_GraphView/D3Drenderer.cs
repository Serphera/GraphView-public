using System;
using System.Windows;
using System.Windows.Forms;
using System.IO;

using System.Windows.Interop;
using windows = System.Windows;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;


using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using D2D1 = SharpDX.Direct2D1;
using SharpDX.WIC;
using SharpDX.DXGI;
using SharpDX.Windows;


using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace GameEditor_GraphView {


    class D3Drenderer {

        public RenderForm _RenderForm;

        public ConcurrentStack<Tuple<int, int>> MessageQueue = new ConcurrentStack<Tuple<int, int>>();

        private Device _device;
        private DeviceContext _context;

        private SwapChainDescription _SwapDesc;
        private SwapChain _swapChain;

        private VertexShader _VertexShader;
        private PixelShader _PixelShader;

        private Buffer _VertexBuffer;
        private Buffer _ConstantBuffer;

        private Vector4[] vertices = new Vector4[1];       
        

        private bool IsViewModelChanged = true;

        private Thread displayThread;

        private ManualResetEvent mre;

        private float screenWidth;
        private float screenHeight;

        //private Vertex[] vertices = new Vertex[1];

        struct Vertex {

            public Vector4 pos;
            Vector4 col;            
            Vector2 tex;

            public Vertex(Vector4 position, Vector4 colour, Vector2 texture) {

                pos = position;
                col = colour;
                tex = texture;
            }
        }


        // TODO: Switch to lower quality pixel Format to reduce memory overhead
        // TODO: Find memory leak on move and resize
        public D3Drenderer(GraphView_StateManager manager, View.GraphView_CurveView View) {

            ((GameEditor_GraphView.ViewModel.CurveGraphViewModel)manager.ViewModel).Items.CollectionChanged += ViewModelChanged;
            Configuration.EnableObjectTracking = true;

            screenWidth = (float)SystemParameters.PrimaryScreenWidth;
            screenHeight = (float)SystemParameters.PrimaryScreenHeight;

            _RenderForm = new RenderForm("");
            SetRenderDimensions(2048, 1280);

            //_RenderForm.
            _RenderForm.TopLevel = false;
            _RenderForm.FormBorderStyle = FormBorderStyle.None;

            CreateSwapChain();

            manager.RefRenderer(this);
            mre = new ManualResetEvent(false);
            manager.SetupResetEvent(ref mre);

            // Create Device and set device context
            // D3D11 Device
            Device.CreateWithSwapChain(
                DriverType.Hardware, 
                DeviceCreationFlags.BgraSupport,
                _SwapDesc, 
                out _device, out _swapChain);

            _context = _device.ImmediateContext;
        }


        private void SetRenderDimensions(int width, int height) {

            _RenderForm.Width = width;
            _RenderForm.Height = height;
        }


        private void CreateSwapChain(bool IsAlpha = false) {

            ModeDescription mode;

            if (IsAlpha) {

                mode = new ModeDescription(
                        _RenderForm.ClientSize.Width,
                        _RenderForm.ClientSize.Height,
                        new Rational(5, 1),
                        Format.A8_UNorm);
            }
            else {

                mode = new ModeDescription(
                        _RenderForm.ClientSize.Width,
                        _RenderForm.ClientSize.Height,
                        new Rational(5, 1),
                        Format.B8G8R8A8_UNorm);
            }

            _SwapDesc = new SwapChainDescription() {
                BufferCount = 1,
                ModeDescription = mode,
                IsWindowed = true,
                OutputHandle = _RenderForm.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };
        }


        /// <summary>
        /// Starts render loop
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="View"></param>
        public void Render(GraphView_StateManager manager, View.GraphView_CurveView View) {

            var signature = CreateShaders("Smooth.fx");
            //var signature = CreateShaders();
            var layout = new InputLayout(_device, signature, new[] {

                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32B32A32_Float, 32, 0)
            });

            SetSamplerState();

            // Instantiate buffers
            CreateVertexBuffer();
            CreateConstantBuffer();

            // Prepare Context
            SetUpContext(layout);

            // Prepare matrices
            var view = Matrix.OrthoLH(_RenderForm.ClientSize.Width, _RenderForm.ClientSize.Width, 0.00f, 10000.0f);
            Matrix proj = Matrix.Identity;

            // Declare texture for rendering
            bool userResized = true;
            Texture2D backBuffer = null;
            RenderTargetView renderView = null;
            Texture2D depthBuffer = null;
            DepthStencilView depthView = null;

            var camera = ((ViewModel.CurveGraphViewModel)manager.ViewModel).Camera;

            var rastDesc = new RasterizerStateDescription {
                
                FillMode = FillMode.Solid,
                CullMode = CullMode.None,
                IsAntialiasedLineEnabled = true,
                IsMultisampleEnabled = true
            };

            var viewPort = new Viewport(0, 0, _RenderForm.ClientSize.Width, _RenderForm.ClientSize.Height);

            _RenderForm.UserResized += (sender, args) => userResized = true;


            // Main loop
            RenderLoop.Run(_RenderForm, () => {
     
                //TODO: Check for if collectionchanged event has been raised and trigger vertex recalculation

                if (IsViewModelChanged || !MessageQueue.IsEmpty || true) {

                    Thread vertexThread = new Thread(() => CalculateQuads(manager));
                    vertexThread.Start();

                    CreateSwapChain();

                    IsViewModelChanged = false;

                    if (true) {

                        if (WindowResized()) { userResized = true; }

                        SetSamplerState();
                    }

                    // If Form resized
                    if (userResized) {

                        Console.WriteLine("resizing");
                        // Dispose all previous allocated resources
                        Utilities.Dispose(ref backBuffer);
                        Utilities.Dispose(ref renderView);
                        Utilities.Dispose(ref depthBuffer);
                        Utilities.Dispose(ref depthView);

                        // Resizing buffers
                        _swapChain.ResizeBuffers(
                            _SwapDesc.BufferCount,
                            2048,
                            1280,
                            Format.B8G8R8A8_UNorm,
                            SwapChainFlags.None
                            );

                        backBuffer = Texture2D.FromSwapChain<Texture2D>(_swapChain, 0);
                        renderView = new RenderTargetView(_device, backBuffer);

                        // Create the depth buffer
                        depthBuffer = new Texture2D(_device, new Texture2DDescription() {
                            Format = Format.D32_Float_S8X24_UInt,
                            ArraySize = 1,
                            MipLevels = 1,
                            Width = 2048,
                            Height = 1280,
                            SampleDescription = new SampleDescription(1, 0),
                            Usage = ResourceUsage.Default,
                            BindFlags = BindFlags.DepthStencil,
                            CpuAccessFlags = CpuAccessFlags.None,
                            OptionFlags = ResourceOptionFlags.None
                        });

                        // Create the depth buffer view
                        depthView = new DepthStencilView(_device, depthBuffer);
                        viewPort.Width = _RenderForm.ClientSize.Width;
                        viewPort.Height = _RenderForm.ClientSize.Height;

                        // Setup targets and viewport for rendering
                        _context.Rasterizer.SetViewport(viewPort);
                        _context.OutputMerger.SetTargets(depthView, renderView);
                        _context.Rasterizer.State = new RasterizerState(_device, rastDesc);

                        userResized = false;
                    }

                    // World Matrix     
                    // The World Matrix translates the position of your vertices from model space to World space. 
                    // That means it applies its position in the world and its rotation
                    var world = CalculateWorldTransform(camera);

                    // A view matrix tells the GPU the position of the camera, the point the camera is facing, 
                    // and the up vector for the camera
                    view = Matrix.LookAtLH(new Vector3(0, 0, -100), new Vector3(0, 0, 0), Vector3.Up);

                    // A projection matrix tells the GPU the FOV, aspect ratio, and near and far clipping planes for the camera
                    //proj = Matrix.OrthoLH(1280, 720, 0.00f, 1000.0f);
                    proj = Matrix.OrthoLH(screenWidth, screenHeight, 0.00f, 1000.0f);
                    var viewProj = Matrix.Multiply(view, proj);

                    //var worldViewProj = Matrix.Multiply(world, proj);
                    var worldViewProj = Matrix.Multiply(world, viewProj);

                    // Clear views and set background as transparent
                    _context.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
                    _context.ClearRenderTargetView(renderView, new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0.0f));

                    var screenPoints = PointToScreenSpace(manager, viewPort, worldViewProj);

                    worldViewProj.Transpose();

                    _context.UpdateSubresource(ref worldViewProj, _ConstantBuffer);

                    // Instantiate Vertex buffer from vertex data
                    vertexThread.Join();
                    CreateVertexBuffer();
                    SetUpContext(layout);

                    // Draw
                    _context.Draw(vertices.Length, 0);

                    // Convert backbuffer to texture, then to bitmap for display
                    D2D1.BitmapRenderTarget target = RenderBitmap();

                    // Get byte array for bitmap
                    byte[] data = CalculateBitmapBytes(backBuffer);

                    DisplayBitmap(target, data, screenPoints, manager);

                    // Dispose all previous allocated resources
                    _context.Flush();
                    screenPoints = null;
                    GC.Collect();

                    if (displayThread != null) { displayThread.Join(); }

                    mre.Reset();
                    mre.WaitOne();
                }
                // End of renderloop
            });

            // Release all resources
            _context.ClearState();
            _context.Flush();
            DisposeResources();

            signature.Dispose();
            layout.Dispose();

            depthBuffer.Dispose();
            backBuffer.Dispose();

            depthView.Dispose();
            renderView.Dispose();
        }


        /// <summary>
        /// Creates vertex and pixel shaders
        /// </summary>
        /// <returns></returns>
        private ShaderSignature CreateShaders(string file = "MiniCube.fx") {

            ShaderSignature signature;

            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile(file, "VS", "vs_4_0")) {

                _VertexShader = new VertexShader(_device, vertexShaderByteCode);
                signature = ShaderSignature.GetInputOutputSignature(vertexShaderByteCode);
            };

            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile(file, "PS", "ps_4_0")) {

                _PixelShader = new PixelShader(_device, pixelShaderByteCode);
            };

            return signature;
        }


        private void SetSamplerState() {

            var SamplerStateDesc = new SamplerStateDescription {

                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = Filter.Anisotropic,
                MaximumAnisotropy = 16
            };

            using (var SamplerState = new SamplerState(_device, SamplerStateDesc)) {

                _context.PixelShader.SetSampler(0, SamplerState);
            }
        }


        private void CreateVertexBuffer() {

            if (_VertexBuffer != null) { _VertexBuffer.Dispose(); }

            _VertexBuffer = Buffer.Create(
                _device, BindFlags.VertexBuffer, 
                vertices);
        }


        private void CreateConstantBuffer() {

            _ConstantBuffer = new Buffer(
                _device,
                Utilities.SizeOf<Matrix>(),
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.Shared,
                0                
                );
        }


        /// <summary>
        /// Used to set up device context variables
        /// </summary>
        /// <param name="layout"></param>
        private void SetUpContext(InputLayout layout) {

            _context.InputAssembler.InputLayout = layout;
            _context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            _context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_VertexBuffer, Utilities.SizeOf<Vector4>() * 3, 0));
            _context.VertexShader.SetConstantBuffer(0, _ConstantBuffer);
            _context.VertexShader.Set(_VertexShader);
            _context.PixelShader.Set(_PixelShader);
        }


        /// <summary>
        /// Calculates Vertices from model curve
        /// </summary>
        /// <param name="manager"></param>
        private void CalculateQuads(GraphView_StateManager manager) {

            int index = 0;
            try {

                // TODO: Create method to change start/end coords if the angle is too tight, to avoid dissappearing line
                //var points = ((GameEditor_GraphView.ViewModel.CurveGraphViewModel)manager.ViewModel).ModelItems.Item.Curve.Points;
                //var points = DrawGraphV2.Draw(((ViewModel.CurveGraphViewModel)manager.ViewModel).ModelItems.Item.Curve.Original)
                vertices = null;

                // Number of points minus 1, times the numbers of points for 2 triangles, 
                // times vector attributes in shader (3 for location, 1 for color)
                vertices = new Vector4[(points.Count - 1) * 18];


                var color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);

                for (int i = 0; i < points.Count - 2; i++) {

                    Vector2 start = new Vector2((float)points[i].X, (float)points[i].Y);
                    Vector2 end = new Vector2((float)points[i + 1].X, (float)points[i + 1].Y);


                    // 2 triangles form one quad
                    // Triangle 1
                    vertices[index] = new Vector4(start.X, start.Y - 2.5f, 1.0f, 1.0f);
                    vertices[index + 1] = color;
                    vertices[index + 2] = new Vector4(1.0f, 0.0f, 1.0f, 1.0f);

                    vertices[index + 3] = new Vector4(start.X, start.Y + 2.5f, 1.0f, 1.0f);
                    vertices[index + 4] = color;
                    vertices[index + 5] = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);

                    vertices[index + 6] = new Vector4(end.X, end.Y + 2.5f, 1.0f, 1.0f);
                    vertices[index + 7] = color;
                    vertices[index + 8] = new Vector4(0.0f, 1.0f, 1.0f, 1.0f);

                    //Triangle 2
                    vertices[index + 9] = vertices[index + 6];
                    vertices[index + 10] = color;
                    vertices[index + 11] = vertices[index + 8];

                    vertices[index + 15] = vertices[index];
                    vertices[index + 16] = color;
                    vertices[index + 17] = vertices[index + 2];

                    vertices[index + 12] = new Vector4(end.X, end.Y - 2.5f, 1.0f, 1.0f);
                    vertices[index + 13] = color;
                    vertices[index + 14] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

                    index += 18;
                }
            }
            catch (Exception) {

                throw;
            }
        }


        /// <summary>
        /// Updates Render Dimensions if window resized
        /// </summary>
        /// <returns></returns>
        private bool WindowResized() {

            var IsResized = false;

            if (!MessageQueue.IsEmpty) {

                MessageQueue.TryPop(out Tuple<int, int> newSize);
                SetRenderDimensions(newSize.Item1, newSize.Item2);
                MessageQueue.Clear();

                IsResized = true;
            }

            return IsResized;
        }


        private Matrix CalculateWorldTransform(TestingCamera camera) {

            return Matrix.Scaling(new Vector3(1.0f / (float)camera.GetScale, 1.0f / (float)camera.GetScale, 1.0f)) *
                        Matrix.Translation(new Vector3(-(float)camera.GetTransform(0, 0), -(float)camera.GetTransform(0, 1), 0)) *
                        Matrix.RotationX(0) *
                        Matrix.RotationY(0) *
                        Matrix.RotationZ(0);
        }


        /// <summary>
        /// Converts rendered image to bitmap
        /// </summary>
        /// <returns></returns>
        private D2D1.BitmapRenderTarget RenderBitmap() {

            using (var textureToBitmap = Texture2D.FromSwapChain<Texture2D>(_swapChain, 0)) {

                using (var surface = textureToBitmap.QueryInterface<Surface>()) {

                    using (D2D1.Factory factory = new D2D1.Factory1()) {

                        var target = new D2D1.RenderTarget(
                            factory,
                            surface,
                            new D2D1.RenderTargetProperties(
                                new D2D1.PixelFormat(Format.B8G8R8A8_UNorm, D2D1.AlphaMode.Ignore)
                            )
                        );

                        D2D1.BitmapRenderTarget tes = null;



                        tes = new D2D1.BitmapRenderTarget(
                            target,
                            D2D1.CompatibleRenderTargetOptions.None,
                            new D2D1.PixelFormat(
                                Format.B8G8R8A8_UNorm,
                                D2D1.AlphaMode.Ignore));

                        target.Dispose();

                        return tes;
                    }
                }
            }

        }


        /// <summary>
        /// Converts rendered image to bitmap byte array
        /// </summary>
        /// <param name="backBuffer"></param>
        /// <returns></returns>
        private byte[] CalculateBitmapBytes(Texture2D backBuffer) {

            byte[] data = null;

            // We want to copy the texture from the back buffer
            Texture2DDescription desc = backBuffer.Description;
            desc.CpuAccessFlags = CpuAccessFlags.Read;
            desc.Usage = ResourceUsage.Staging;
            desc.OptionFlags = ResourceOptionFlags.None;
            desc.BindFlags = BindFlags.None;

            using (var texture = new Texture2D(_device, desc)) {

                _context.CopyResource(backBuffer, texture);

                using (var surface = texture.QueryInterface<Surface>()) {

                    texture.Dispose();

                    DataStream dataStream;

                    var map = surface.Map(SharpDX.DXGI.MapFlags.Read, out dataStream);
                    int lines = (int)(dataStream.Length / map.Pitch);

                    data = new byte[surface.Description.Width * surface.Description.Height * 4];                    

                    int dataCounter = 0;

                    // Width of the surface - 4 bytes per pixel. Red, Green, Blue, Alpha(transparency) one byte each
                    int actualWidth = surface.Description.Width * 4;

                    for (int y = 0; y < lines; y++) {

                        for (int x = 0; x < map.Pitch; x++) {

                            if (x < actualWidth) {

                                data[dataCounter++] = dataStream.Read<byte>();
                            }
                            else {

                                dataStream.Read<byte>();
                            }



                        }
                    }

                    dataStream.Dispose();
                    surface.Unmap();
                }
            }

            return data;
        }


        /// <summary>
        /// Sends bitmap to ViewModel through manager
        /// </summary>
        /// <param name="bitmapTarget"></param>
        /// <param name="data"></param>
        /// <param name="screenPoints"></param>
        /// <param name="manager"></param>
        private void DisplayBitmap(D2D1.BitmapRenderTarget bitmapTarget, byte[] data, List<windows.Point> screenPoints, GraphView_StateManager manager) {

            if (bitmapTarget != null && data != null) {

                int height = 0;
                int width = 0;
                float dpiHeight = 0;
                float dpiWidth = 0;

                using (var bitmap = bitmapTarget.Bitmap) {

                    height = bitmap.PixelSize.Height;
                    width = bitmap.PixelSize.Width;

                    dpiHeight = bitmap.DotsPerInch.Height;
                    dpiWidth = bitmap.DotsPerInch.Width;
                }

                bitmapTarget.Dispose();

                displayThread = new Thread(() => manager.SetupView(height, width, dpiHeight, dpiWidth, data, screenPoints));
                displayThread.SetApartmentState(ApartmentState.STA);
                displayThread.Start();
            }
        }


        /// <summary>
        /// Converts D3D coords to Canvas coords
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="viewPort"></param>
        /// <param name="worldViewProj"></param>
        /// <returns></returns>
        private List<windows.Point> PointToScreenSpace(GraphView_StateManager manager, Viewport viewPort, Matrix worldViewProj) {

            List<windows.Point> list = new List<windows.Point>();

            var points = ((GameEditor_GraphView.ViewModel.CurveGraphViewModel)manager.ViewModel).ModelItems.Item.Curve.Points;
            var camera = ((ViewModel.CurveGraphViewModel)manager.ViewModel).Camera;

            for (int i = 0; i < points.Count; i++) {

                // Converts point in D3D space to screen space
                //var vector = new Vector3((float)(points[i].X - camera.GetTransform(0, 0)), (float)(points[i].Y - camera.GetTransform(0, 1)), 1);
                var vector = new Vector3((float)points[i].X, (float)points[i].Y, 1);

                var space = Vector3.Project(vector, 0, 1, viewPort.Width, viewPort.Height, 0.0f, 100.0f, worldViewProj);

                list.Add(new windows.Point(space.X, space.Y ));
            }

            return list; 
        }
        

        private void ViewModelChanged(object sender, NotifyCollectionChangedEventArgs e) {

            IsViewModelChanged = true;
        }


        /// <summary>
        /// Removes resources used for rendering
        /// </summary>
        private void DisposeResources() {

            Utilities.Dispose(ref _swapChain);

            Utilities.Dispose(ref _PixelShader);
            Utilities.Dispose(ref _VertexShader);

            Utilities.Dispose(ref _device);
            Utilities.Dispose(ref _context);

            Utilities.Dispose(ref _ConstantBuffer);
            Utilities.Dispose(ref _VertexBuffer);
        }




        public void Dispose() {

            try {

                // Release all resources
                _context.ClearState();
                _context.Flush();
                Utilities.Dispose(ref _context);
                DisposeResources();
                Utilities.Dispose(ref _swapChain);
                Utilities.Dispose(ref _device);
            }
            catch (Exception) {

                throw;
            }
        }
    }
}
 