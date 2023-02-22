using Assimp;
using Assimp.Configs;
using System.Reflection;
using Silk.NET.Maths;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Diagnostics;

namespace EricEngineSharp
{
    /// <summary>
    /// Loads assets using <see cref="Assimp"/>'s <see cref="AssimpContext"/>. Caches any loaded assets in memory.
    /// </summary>
    internal class AssetLoader
    {
        private static AssetLoader instance;
        public static AssetLoader Instance => instance ?? (instance = new AssetLoader());

        private AssimpContext importer;
        private Dictionary<string, MeshGroup> loadedMeshes = new Dictionary<string, MeshGroup>();

        private readonly string basePath;
        private const string AssetsFolderName = "Assets";

        private const string EricMeshExtension = ".ericmesh";

        private static int currentMeshID = 0;

        /// <summary>
        /// Initializes <see cref="importer"/> to a new <see cref="AssimpContext"/> and sets it up.
        /// Also sets <see cref="basePath"/> to the executing assembly location + <see cref="AssetsFolderName"/>.
        /// </summary>
        /// <remarks>Note: this is private to enforce singleton. Only one <see cref="instance"/> should ever be created.</remarks>
        private AssetLoader()
        {
            importer = new AssimpContext();
            importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));

            basePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), AssetsFolderName);
        }

        /// <summary>
        /// Loads a MeshList from the filename passed in and caches it, or gets the cached mesh from <see cref="loadedMeshes"/>.
        /// </summary>
        /// <param name="filename">The filename to read a mesh from/the key to access <see cref="loadedMeshes"/> with.</param>
        public MeshGroup LoadOrGetMeshList(string filename)
        {
            if (loadedMeshes.ContainsKey(filename)) return loadedMeshes[filename];

            filename = Path.Combine(basePath, "Models", filename);

            // If we have an ericmesh file created already, load from that,
            // otherwise use Assimp to load the asset in from a file
            MeshGroup? ericMesh = LoadEricMeshGroup(filename);
            if (ericMesh == null)
            {
                ericMesh = LoadMeshGroupUsingAssimp(filename);
            }

            // Don't cache a null mesh
            if (ericMesh == null) return null;

            loadedMeshes[filename] = ericMesh;
            return loadedMeshes[filename];
        }

        /// <summary>
        /// Loads a <see cref="MeshGroup"/> from Assimp compatible file formats (.obj, etc.)
        /// </summary>
        /// <param name="filename">The full name of the file to load in</param>
        /// <returns>The model file converted to a <see cref="MeshGroup"/></returns>
        private MeshGroup LoadMeshGroupUsingAssimp(string filename)
        {
            Scene scene = importer.ImportFile(filename, PostProcessPreset.TargetRealTimeMaximumQuality);

            // Load in each mesh thats in the scene
            var meshes = new Mesh[scene.MeshCount];
            for (int i = 0; i < scene.MeshCount; i++)
            {
                meshes[i] = scene.Meshes[i].GetEricEngineMesh();
            }

            // Cache the mesh
            return new MeshGroup(meshes, filename);
        }

        /// <summary>
        /// Saves a <see cref="MeshGroup"/> to a file called <paramref name="filename"/>.ericmesh.
        /// </summary>
        /// <param name="filename">The name of the file the mesh group will be saved.</param>
        private void SaveEricMeshGroup(string filename)
        {
            var options = new JsonSerializerOptions()
            {
                IncludeFields = true,
                WriteIndented = true,
            };
            string file = JsonSerializer.Serialize(loadedMeshes[filename], options);
            using (StreamWriter sw = new StreamWriter(filename + EricMeshExtension))
            {
                sw.WriteLine(file);
            }
        }

        /// <summary>
        /// Loads in a <see cref="MeshGroup"/> from a file
        /// </summary>
        /// <param name="filename">The full name of the file to load a <see cref="MeshGroup"/> from</param>
        /// <returns><see cref="MeshGroup"/> loaded in from <paramref name="filename"/></returns>
        private MeshGroup? LoadEricMeshGroup(string filename)
        {
            filename = filename + EricMeshExtension;
            try
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    string meshGroup = sr.ReadToEnd();
                    var options = new JsonSerializerOptions()
                    {
                        IncludeFields = true,
                    };
                    MeshGroup mg = JsonSerializer.Deserialize<MeshGroup>(meshGroup, options);
                    return mg;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }
    }

    internal static class AssimpVectorExtensions
    {
        /// <summary>
        /// Converts an <see cref="Assimp.Mesh"/> object to a <see cref="Mesh"/> object that works with
        /// <see cref="Silk.NET.Maths"/>
        /// </summary>
        /// <param name="m">The <see cref="Assimp.Mesh"/> to convert to a <see cref="Mesh"/>.</param>
        /// <returns>The converted <see cref="Mesh"/> object.</returns>
        public static Mesh GetEricEngineMesh(this Assimp.Mesh m)
        {
            Mesh ericEngineMesh = new Mesh();
            ericEngineMesh.vertices = m.GetEricEngineVertices();
            ericEngineMesh.indices = m.GetUnsignedIndices();
            return ericEngineMesh;
        }

        /// <summary>
        /// Grabs vertices/normals/tangents/uv's from <paramref name="m"/> (of type <see cref="Assimp.Vector3D"/>) 
        /// and converts them to <see cref="Vertex"/> objects with appropriate <see cref="Silk.NET.Maths"/> types.
        /// </summary>
        /// <param name="m">The <see cref="Assimp.Mesh"/> to convert vertices from.</param>
        /// <returns>An array of <see cref="Vertex"/> objects that correspond to the given <see cref="Assimp.Mesh"/> <paramref name="m"/></returns>
        public static Vertex[] GetEricEngineVertices(this Assimp.Mesh m)
        {
            var vertices = new Vertex[m.VertexCount];
            var uvs = m.TextureCoordinateChannels[0].GetSilkVectorArray();
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vertex();
                vertices[i].position = m.Vertices[i].GetSilkVector();
                vertices[i].normal = m.Normals[i].GetSilkVector();
                vertices[i].tangent = m.Tangents[i].GetSilkVector();
                vertices[i].uv = new Vector2D<float> { X = uvs[i].X, Y = uvs[i].Y };
            }
            return vertices;
        }

        /// <summary>
        /// Converts an <see cref="Assimp.Vector3D"/> to a <see cref="Silk.NET.Maths"/> <see cref="Vector3D{T}"/> where T is <see cref="float"/>.
        /// </summary>
        /// <param name="assimpVec">The <see cref="Assimp.Vector3D"/> to convert.</param>
        /// <returns>The converted <see cref="Vector3D{T}"/> where T is <see cref="float"/>.</returns>
        public static Vector3D<float> GetSilkVector(this Assimp.Vector3D assimpVec)
        {
            return new Vector3D<float> { X = assimpVec.X, Y = assimpVec.Y, Z = assimpVec.Z };
        }

        /// <summary>
        /// Converts a <see cref="List{T}"/> of <see cref="Assimp.Vector3D"/>s to an array of <see cref="Silk.NET.Maths"/> <see cref="Vector3D{T}"/> where T is <see cref="float"/>.
        /// </summary>
        /// <param name="vector3Ds">The <see cref="List{T}"/> of <see cref="Assimp.Vector3D"/>s to convert.</param>
        /// <returns>The converted array of <see cref="Silk.NET.Maths"/> <see cref="Vector3D{T}"/> where T is <see cref="float"/></returns>
        public static Vector3D<float>[] GetSilkVectorArray(this List<Assimp.Vector3D> vector3Ds) 
        {
            return vector3Ds.Select(x => x.GetSilkVector()).ToArray();
        }
    }
}
