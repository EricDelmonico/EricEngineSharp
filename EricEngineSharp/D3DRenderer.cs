using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace EricEngineSharp
{
    internal class D3DRenderer : IRenderer
    {
        public unsafe void Init()
        {
            // Set up device creation flags
            var creationFlags = CreateDeviceFlag.BgraSupport;
#if DEBUG
            creationFlags |= CreateDeviceFlag.Debug;
#endif
            // Specify feature levels
            fixed D3DFeatureLevel[] featureLevels = new[]
            {
                D3DFeatureLevel.Level122,
            };

            // Create the Direct3D 11 API device object and a corresponding context
            ComPtr<ID3D11Device> device = new ComPtr<ID3D11Device>();
            ComPtr<ID3D11DeviceContext> context = new ComPtr<ID3D11DeviceContext>();
            D3D11.GetApi().CreateDevice(
                null,
                D3DDriverType.Hardware,
                nint.Zero,
                creationFlags,
                &featureLevels[0],
                (uint)featureLevels.Length,
                7,
                device.GetAddressOf(),
                null,
                context.GetAddressOf());
        }

        public void OnClose()
        {
        }

        public void Render(double obj)
        {
        }
    }
}
