using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EricEngineSharp.Components
{
    internal class RenderComponent : IComponent
    {
        public bool IsValid { get; private set; }

        public Vector3D<float> position;
        public Vector3D<float> pitchYawRoll;
        public Vector3D<float> scale;

        public MeshGroup meshGroup;

        public Matrix4X4<float> Matrix =>
            Matrix4X4<float>.Identity
            * Matrix4X4.CreateScale(scale)
            * Matrix4X4.CreateFromYawPitchRoll(pitchYawRoll.Y, pitchYawRoll.X, pitchYawRoll.Z)
            * Matrix4X4.CreateTranslation(position);

        public RenderComponent(MeshGroup meshGroup)
        {
            this.meshGroup = meshGroup;
            IsValid = true;

            position = new Vector3D<float> { X = 0, Y = 0, Z = 0 };
            scale = new Vector3D<float> { X = 1, Y = 1, Z = 1 };
        }
    }
}
