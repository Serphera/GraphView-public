
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;


using SharpDX.Direct2D1;
using SharpDX.Windows;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using SharpDX;

using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;

using GameEditor_GraphView.ViewModel;

using D3D11 = SharpDX.Direct3D11;

namespace GameEditor_GraphView {
    class D3D_Resource : IDisposable {

        private SharpDX.Direct3D11.Device d3dDevice;
        private SharpDX.Direct3D11.DeviceContext d3dDeviceContext;

        private SharpDX.Direct2D1.Device d2dDevice;
        private SharpDX.Direct2D1.DeviceContext d2dContext;

        private D3D11.VertexShader vertexShader;
        private D3D11.PixelShader pixelShader;

        private D3D11.Buffer triangleVertexBuffer;
        private Texture2D depthBuffer;

        private DepthStencilView depthView;

        //private SharpDX.Vector3[] vertices;
        private Vector3[] vertices = new Vector3[] { new Vector3(-0.5f, 0.5f, 1.0f), new Vector3(0.5f, 0.5f, 1.0f), new Vector3(0.0f, -0.5f, 1.0f) };

        private SwapChain swapChain;

        private D3D11.RenderTargetView renderTargetView;

        private D3D11.InputElement[] inputElements = new D3D11.InputElement[] {
            new D3D11.InputElement("POSITION", 0, Format.B8G8R8A8_UNorm, 0)
        };

        private ShaderSignature inputSignature;
        private D3D11.InputLayout inputLayout;

        private SharpDX.Viewport viewPort;

        private bool initialized = false;
        private int width;
        private int height;

        //TODO: Convert grid and graph drawing to D3D
        public D3D_Resource(GraphView_StateManager manager) {


            HwndSource source = HwndSource.FromHwnd(manager.ViewHandle);
            Window view = source.RootVisual as Window;

            width = (int)view.Width;
            height = (int)view.Height;

            ModeDescription backBufferDesc = new ModeDescription((int)view.Width + 250, (int)view.Height + 250, new Rational(20, 1), Format.B8G8R8A8_UNorm);

            SwapChainDescription swapChainDesc = new SwapChainDescription() {

                ModeDescription = backBufferDesc,
                Flags = SwapChainFlags.AllowModeSwitch,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputHandle = manager.ViewHandle,
                IsWindowed = true,
                SwapEffect = SwapEffect.Discard
            };



            D3D11.Device.CreateWithSwapChain(DriverType.Hardware, D3D11.DeviceCreationFlags.None | DeviceCreationFlags.BgraSupport,
                swapChainDesc, out d3dDevice, out swapChain);

            d3dDeviceContext = d3dDevice.ImmediateContext;

            //SharpDX.DXGI.Device2 dxgiDevice2 = d3dDevice.QueryInterface<SharpDX.DXGI.Device2>();

            //SharpDX.DXGI.Adapter dxgiAdapter = dxgiDevice2.Adapter;
            //SharpDX.DXGI.Factory2 dxgiFactory2 = dxgiAdapter.GetParent<SharpDX.DXGI.Factory2>();

            //d2dDevice = new SharpDX.Direct2D1.Device(dxgiDevice2);
            //d2dContext = new SharpDX.Direct2D1.DeviceContext(d2dDevice, SharpDX.Direct2D1.DeviceContextOptions.None);

            //Texture2D backbuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            //d2dContext.PrimitiveBlend = PrimitiveBlend.SourceOver;

            //var properties = new BitmapProperties1(new PixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied),
            //    (int)view.Width, (int)view.Height, BitmapOptions.Target | BitmapOptions.CannotDraw);

            //var _targ = new Bitmap1(d2dContext, swapChain.GetBackBuffer<Surface>(0), properties);
            //d2dContext.Target = _targ;

            //vertices = new SharpDX.Vector3[00];



            using (D3D11.Texture2D backBuffer = swapChain.GetBackBuffer<D3D11.Texture2D>(0)) {

                renderTargetView = new D3D11.RenderTargetView(d3dDevice, backBuffer);
            }

            depthBuffer = new Texture2D(d3dDevice, new Texture2DDescription() {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = width,
                Height = height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            // Create the depth buffer view
            depthView = new DepthStencilView(d3dDevice, depthBuffer);


            viewPort = new SharpDX.Viewport(0, 0, width, height);
            d3dDeviceContext.Rasterizer.SetViewport(viewPort);
        }

        private void InitializeShaders() {

            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile("vertexShader.hlsl", "main", "vs_4_0", ShaderFlags.None)) {

                inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                vertexShader = new D3D11.VertexShader(d3dDevice, vertexShaderByteCode);

            }

            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("pixelShader.hlsl", "main", "ps_4_0", ShaderFlags.None)) {

                pixelShader = new D3D11.PixelShader(d3dDevice, pixelShaderByteCode);
            }

            d3dDeviceContext.VertexShader.Set(vertexShader);
            d3dDeviceContext.PixelShader.Set(pixelShader);

            d3dDeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            inputLayout = new D3D11.InputLayout(d3dDevice, inputSignature, inputElements);
            d3dDeviceContext.InputAssembler.InputLayout = inputLayout;
        }


        private void InitializeTriangle() {


            triangleVertexBuffer = D3D11.Buffer.Create<SharpDX.Vector3>(d3dDevice, D3D11.BindFlags.VertexBuffer, vertices);
        }

        public void Draw(CurveGraphViewModel vm, TestingCamera cam) {

            Console.WriteLine("GPU Drawing");
            d3dDeviceContext.OutputMerger.SetRenderTargets(renderTargetView);

            if (initialized) {

                //ConvertModelToDx(vm, cam);
                d3dDeviceContext.ClearRenderTargetView(renderTargetView, new SharpDX.Mathematics.Interop.RawColor4(0.50f, 0.50f, 0.75f, 1));
                d3dDeviceContext.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
                InitializeShaders();
                InitializeTriangle();
                d3dDeviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(triangleVertexBuffer, SharpDX.Utilities.SizeOf<SharpDX.Vector3>(), 0));
                d3dDeviceContext.Draw(3, 0);

            }
            else {

                initialized = true;
                d3dDeviceContext.ClearRenderTargetView(renderTargetView, new SharpDX.Mathematics.Interop.RawColor4(0.50f, 0.50f, 0.75f, 1));
                //d2dContext.Clear(new SharpDX.Mathematics.Interop.RawColor4(0.50f, 0.50f, 0.75f, 1));
            }

            swapChain.Present(1, PresentFlags.None);

        }



        /// <summary>
        /// Converts CurveGraphViewModel Items to DX objects and renders
        /// </summary>
        private void ConvertModelToDx(CurveGraphViewModel vm, TestingCamera cam) {

            //List<SharpDX.Vector3[]> objectList = new List<SharpDX.Vector3[]>();

            //SharpDX.DirectWrite.Factory _factory = new SharpDX.DirectWrite.Factory();

            //d2dContext.BeginDraw();
            //d2dContext.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
            //d2dContext.Clear(new SharpDX.Mathematics.Interop.RawColor4(0.50f, 0.50f, 0.75f, 1));

            //Lines
            //SharpDX.Direct2D1.SolidColorBrush lineBrush = new SharpDX.Direct2D1.SolidColorBrush(d2dContext, SharpDX.Color.LightGreen);


            SharpDX.Vector2 cameraOffset = new SharpDX.Vector2((float)cam.GetTransform(0, 0), (float)cam.GetTransform(0, 1));


            /*
            for (int i = 0; i < vm.ModelItems.Item.Curve.Points.Count - 2; i++) {

                SharpDX.Vector2 start = new SharpDX.Vector2((float)vm.ModelItems.Item.Curve.Points[i].X, (float)vm.ModelItems.Item.Curve.Points[i].Y);
                SharpDX.Vector2 end = new SharpDX.Vector2((float)vm.ModelItems.Item.Curve.Points[i + 1].X, (float)vm.ModelItems.Item.Curve.Points[i + 1].Y);

                start -= cameraOffset;
                end -= cameraOffset;

                d2dContext.DrawLine(start, end, lineBrush, 4);
            }
            */
            /*
           int index = 0;

           for (int i = 0; i < vm.ModelItems.Item.Curve.Points.Count - 2; i+= 3) {

               SharpDX.Vector2 start = new SharpDX.Vector2((float)vm.ModelItems.Item.Curve.Points[i].X, (float)vm.ModelItems.Item.Curve.Points[i].Y);
               SharpDX.Vector2 end = new SharpDX.Vector2((float)vm.ModelItems.Item.Curve.Points[i + 1].X, (float)vm.ModelItems.Item.Curve.Points[i + 1].Y);

               //start -= cameraOffset;
               //end -= cameraOffset;

               vertices[index] = new SharpDX.Vector3((float)start.X - 1f, (float)start.Y - 1f, 1.0f);
               vertices[index + 1] = new SharpDX.Vector3((float)start.X + 1f, (float)start.Y + 1f, 1.0f);
               vertices[index + 2] = new SharpDX.Vector3((float)end.X + 1f, (float)end.Y + 1f, 1.0f);
               vertices[index + 3] = new SharpDX.Vector3((float)end.X - 1f, (float)end.Y - 1f, 1.0f);

           }

           //lineBrush.Dispose();


           for (int i = 0; i < list.Count; i++) {

               var obj = list[i];

               switch (list[i].GetType().ToString()) {

                   case "System.Windows.Controls.TextBlock":

                       TextBlock blck = (TextBlock)obj;

                       SharpDX.DirectWrite.Factory fontFactory = new SharpDX.DirectWrite.Factory();
                       SharpDX.DirectWrite.TextFormat text = new SharpDX.DirectWrite.TextFormat(_factory, "Impact", 12f);

                       SharpDX.Mathematics.Interop.RawRectangleF textRect = 
                           new SharpDX.Mathematics.Interop.RawRectangleF(
                               (float)Canvas.GetLeft(blck), 
                               (float)Canvas.GetTop(blck), 
                               (float)Canvas.GetRight(blck), 
                               (float)Canvas.GetBottom(blck)
                               );

                       var textFormat = new SharpDX.DirectWrite.TextFormat(fontFactory, "Segoe UI", 24.0f);
                       Brush _brush = new Brush(d2dContext.NativePointer);
                       SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(fontFactory, blck.Text, textFormat, 200, 400);

                       //d2dContext.DrawTextLayout(new SharpDX.Vector2(300, 300), textLayout, _brush);

                       d2dContext.DrawText(blck.Text, textFormat, textRect, _brush);
                       break;

                   case "System.Windows.Shapes.Line":


                       break;

                   case "System.Windows.Shapes.Rectangle":


                       break;

                   default:
                       break;
               }

           }
           */
            //d2dContext.EndDraw();
            //_factory.Dispose();
            //d2dContext.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.Aliased;
        }


        public void Dispose() {
            try {
                renderTargetView.Dispose();
                swapChain.Dispose();
                d3dDevice.Dispose();
                d3dDeviceContext.Dispose();
                triangleVertexBuffer.Dispose();
            }
            catch (Exception) {

                //throw;
            }

        }
    }
}
