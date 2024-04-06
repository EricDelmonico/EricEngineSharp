using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Windowing;

namespace EricEngineSharp
{
    internal class D3DRenderer : IRenderer
    {
        private IWindow window;

        private ComPtr<ID3D11Device> device = new ComPtr<ID3D11Device>();
        private ComPtr<ID3D11DeviceContext> context = new ComPtr<ID3D11DeviceContext>();

        private ComPtr<IDXGISwapChain1> swapChain = new ComPtr<IDXGISwapChain1>();

        private ComPtr<ID3D11VertexShader> vs = new ComPtr<ID3D11VertexShader>();
        private ComPtr<ID3D11PixelShader> ps = new ComPtr<ID3D11PixelShader>();

        public D3DRenderer(IWindow window)
        {
            this.window = window;
        }

        public unsafe void Init()
        {
            // Set up device creation flags
            var creationFlags = CreateDeviceFlag.BgraSupport;
#if DEBUG
            creationFlags |= CreateDeviceFlag.Debug;
#endif
            // Specify feature levels
            D3DFeatureLevel[] featureLevels = new[]
            {
                D3DFeatureLevel.Level100,
            };

            // Create device
            fixed (D3DFeatureLevel* featureLevelPtr = featureLevels)
            {
                // Create the Direct3D 11 API device object and a corresponding context
                ThrowIfFailed(
                    D3D11.GetApi(window).CreateDevice(
                        null,
                        D3DDriverType.Hardware,
                        nint.Zero,
                        (uint)creationFlags,
                        featureLevelPtr,
                        (uint)featureLevels.Length,
                        7,
                        device.GetAddressOf(),
                        null,
                        context.GetAddressOf()));
            }

            //
            // Create swapchain!
            //

            // Need directX graphics infrastructure interface. D3Ddevice implments a COM interface for DXGI
            ComPtr<IDXGIDevice2> dxgiDevice = device.QueryInterface<IDXGIDevice2>();

            // Create adapter
            ComPtr<IDXGIAdapter> dxgiAdapter = new ComPtr<IDXGIAdapter>();
            ThrowIfFailed(dxgiDevice.GetAdapter(dxgiAdapter.GetAddressOf()));

            // Grab factory that created adapter interface
            ComPtr<IDXGIFactory2> dxgiFactory = new ComPtr<IDXGIFactory2>();
            fixed (Guid* dxgiFacUuid = &IDXGIFactory2.Guid)
            {
                ThrowIfFailed(dxgiAdapter.GetParent(dxgiFacUuid, (void**)dxgiFactory.GetAddressOf()));
            }

            // Create the actual swapchain
            SwapChainDesc1 scDesc = new SwapChainDesc1
            {
                Format = Format.FormatB8G8R8A8Unorm,
                Scaling = Scaling.None,
                Stereo = false,
                Width = 1920,
                Height = 1080,
                BufferCount = 1,
                BufferUsage = DXGI.UsageRenderTargetOutput,
            };
            ThrowIfFailed(
                dxgiFactory.CreateSwapChainForHwnd(
                    ComPtr.Downcast<ID3D11Device, IUnknown>(device), 
                    window.Handle, 
                    scDesc, 
                    null, 
                    null, 
                    ref swapChain.Handle));

            //
            // Compile shaders!!
            //

            ComPtr<ID3D10Blob> vsBlob = new ComPtr<ID3D10Blob>();
            ThrowIfFailed(
                D3DCompiler.GetApi().CompileFromFile(
                    "",
                    null,
                    ((ID3DInclude*)(UIntPtr)1),
                    "vs_main",
                    "vs_5_0",
                    0,
                    0,
                    vsBlob.GetAddressOf(),
                    null));

            
        }

        public void OnClose()
        {
        }

        public void Render(double obj)
        {
        }

        private void ThrowIfFailed(HResult hr, string methodName = "")
        {
            if (hr.IsFailure) throw new Exception(methodName + " FAILED with error: " + hr.Value);
        }
    }
}
