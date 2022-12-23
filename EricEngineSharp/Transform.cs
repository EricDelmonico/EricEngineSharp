using NLog.Time;
using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EricEngineSharp
{
    internal class Transform
    {
        public Vector3D<float> position;
        public Vector3D<float> pitchYawRoll;
        public Vector3D<float> scale;

        public Matrix4X4<float> Matrix =>
            Matrix4X4<float>.Identity
            * Matrix4X4.CreateScale(scale)
            * Matrix4X4.CreateFromYawPitchRoll(pitchYawRoll.Y, pitchYawRoll.X, pitchYawRoll.Z)
            * Matrix4X4.CreateTranslation(position);
    }
}
