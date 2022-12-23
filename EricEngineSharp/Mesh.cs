using Silk.NET.Maths;
using System.Security.Cryptography.X509Certificates;

namespace EricEngineSharp
{
    /// <summary>
    /// Represents a mesh made out of an array of <see cref="Vertex"/> objects (<see cref="vertices"/>) and a <see cref="uint"/> array of indices (<see cref="indices"/>)
    /// </summary>
    internal class Mesh
    {
        public Vertex[] vertices;
        public uint[] indices;
    }

    /// <summary>
    /// <see cref="AssetLoader"/>/<see cref="Assimp.AssimpContext"/> will load a list of meshes if a file has multiple. 
    /// This structure is to represent that concept.
    /// </summary>
    internal class MeshGroup
    {
        public string name;
        public Mesh[] meshes;

        public MeshGroup(Mesh[] meshes, string name)
        {
            this.meshes = meshes;
            this.name = name;
        }
    }
}
