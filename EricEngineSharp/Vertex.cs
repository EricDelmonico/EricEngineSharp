using Silk.NET.Maths;
using System.Runtime.InteropServices;
using System.Reflection;

namespace EricEngineSharp
{
    /// <summary>
    /// <para>
    ///   Structure is:
    ///   <list type="number">
    ///     <item><see cref="position"/></item>
    ///     <item><see cref="normal"/></item>
    ///     <item><see cref="tangent"/></item>
    ///     <item><see cref="uv"/></item>
    ///   </list>
    ///   </para>
    /// </summary>
    internal struct Vertex
    {
        public Vector3D<float> position;
        public Vector3D<float> normal;
        public Vector3D<float> tangent;
        public Vector2D<float> uv;

        private static int? stride;
        public static int Stride => stride.HasValue ? stride.Value : (stride = Marshal.SizeOf(typeof(Vertex))).Value;
    }
}
