using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EricEngineSharp.Components
{
    internal class RenderComponent : IComponent
    {
        public Entity Container { get; }

        public Vector3D<float> position;
        public Vector3D<float> pitchYawRoll;
        public Vector3D<float> scale;

        public MeshGroup meshGroup;

        public Matrix4X4<float> Matrix =>
            Matrix4X4<float>.Identity
            * Matrix4X4.CreateScale(scale)
            * Matrix4X4.CreateFromYawPitchRoll(pitchYawRoll.Y, pitchYawRoll.X, pitchYawRoll.Z)
            * Matrix4X4.CreateTranslation(position);

        private double time = 0;

        public RenderComponent(MeshGroup meshGroup, Entity container, int x = 0, int y = 0, int z = 0)
        {
            this.meshGroup = meshGroup;

            position = new Vector3D<float> { X = x, Y = y, Z = z };
            scale = new Vector3D<float> { X = 1, Y = 1, Z = 1 };

            Container = container;
        }

        public void Start()
        {
            
        }

        public void Update(double dt)
        {
            time += dt;
            position.Y = MathF.Sin((float)time);
        }

        public void OnDestroy()
        {
            
        }
    }
}
