using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DotNet.PlatformAbstractions;
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
                D3DFeatureLevel.Level111,
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
                Scaling = Scaling.Stretch,
                Stereo = false,
                Width = 1280,
                Height = 720,
                BufferCount = 2,
                BufferUsage = DXGI.UsageRenderTargetOutput,
                AlphaMode = AlphaMode.Ignore,
                SwapEffect = SwapEffect.FlipDiscard,
                SampleDesc = new SampleDesc(1, 0)
            };
            SwapChainFullscreenDesc desc = new SwapChainFullscreenDesc
            {
                Windowed = true,
                RefreshRate = new Rational(60, 1),
                Scaling = ModeScaling.Stretched,
                ScanlineOrdering = ModeScanlineOrder.Progressive
            };
            ThrowIfFailed(
                dxgiFactory.CreateSwapChainForHwnd<ID3D11Device, IDXGISwapChain1>(
                    device,
                    window.Native!.DXHandle!.Value,
                    &scDesc,
                    &desc,
                    ref Unsafe.NullRef<IDXGIOutput>(),
                    ref swapChain));

            //
            // Compile shaders!!
            //

            Debug.WriteLine("Shader file exists? "
                + File.Exists(
                    Path.Combine(ApplicationEnvironment.ApplicationBasePath,
                    "Shaders",
                    "shaders.hlsl")));

            string shaderSource;
            using (StreamReader reader = new StreamReader(
                Path.Combine(
                    ApplicationEnvironment.ApplicationBasePath,
                    "Shaders",
                    "shaders.hlsl")))
            {
                shaderSource = reader.ReadToEnd();
            }
            var shaderBytes = Encoding.ASCII.GetBytes(shaderSource);

            // Compile vertex shader
            ComPtr<ID3D10Blob> blob = new ComPtr<ID3D10Blob>();
            ComPtr<ID3D10Blob> shaderErrors = new ComPtr<ID3D10Blob>();
            //ThrowIfFailed(
            HResult hr = D3DCompiler.GetApi().Compile(
                in shaderBytes[0],
                (nuint)shaderBytes.Length,
                nameof(shaderSource),
                null,
                ref Unsafe.NullRef<ID3DInclude>(),
                "vs_main",
                "vs_5_0",
                0,
                0,
                ref blob,
                ref shaderErrors);//);

            // Check for compilation errors.
            if (hr.IsFailure)
            {
                if (shaderErrors.Handle is not null)
                {
                    Console.WriteLine(SilkMarshal.PtrToString((nint)shaderErrors.GetBufferPointer()));
                }

                hr.Throw();
            }

            device.CreateVertexShader(
                blob.GetBufferPointer(),
                blob.GetBufferSize(),
                ref Unsafe.NullRef<ID3D11ClassLinkage>(),
                ref vs);

            blob.Release();
            shaderErrors.Release();

            D3DCompiler.GetApi().Compile(
                in shaderBytes[0],
                (nuint)shaderBytes.Length,
                nameof(shaderSource),
                null,
                ref Unsafe.NullRef<ID3DInclude>(),
                "ps_main",
                "ps_5_0",
                0,
                0,
                ref blob,
                ref shaderErrors);

            // Check for compilation errors.
            if (hr.IsFailure)
            {
                if (shaderErrors.Handle is not null)
                {
                    Console.WriteLine(SilkMarshal.PtrToString((nint)shaderErrors.GetBufferPointer()));
                }

                hr.Throw();
            };

            device.CreatePixelShader(
                blob.GetBufferPointer(),
                blob.GetBufferSize(),
                ref Unsafe.NullRef<ID3D11ClassLinkage>(),
                ref ps);

            blob.Release();
            shaderErrors.Release();
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
