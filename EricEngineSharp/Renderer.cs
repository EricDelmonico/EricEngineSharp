using NLog;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EricEngineSharp
{
    internal interface IRenderer
    {
        void Init();
        void Render(double obj);
        void OnClose();
    }

    internal class Renderer : IRenderer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private Dictionary<string, uint> vbos = new Dictionary<string, uint>();
        private Dictionary<string, uint> ebos = new Dictionary<string, uint>();
        private uint vao;
        private uint shaderProgram;

        private GL Gl;
        private IWindow window;

        private MeshGroup testCube;
        private List<MeshGroup> meshGroups = new List<MeshGroup>();
        private Transform cubeTransform = new Transform();

        //Vertex shaders are run on each vertex.
        private readonly string VertexShaderSource = @"
        #version 330 core //Using version GLSL version 3.3
        layout (location = 0) in vec3 vPos;
        layout (location = 1) in vec3 vNormal;
        layout (location = 2) in vec3 vTangent;
        layout (location = 3) in vec2 vUV;
        
        uniform mat4 model;
        uniform mat4 view;
        uniform mat4 perspective;

        out vec3 vertColor;

        void main()
        {
            mat4 mvp = perspective * view * model;
            gl_Position = mvp * vec4(vPos.x, vPos.y, vPos.z, 1.0);
            vertColor = 0.5 * (vPos + vec3(1, 1, 1));
        }
        ";

        //Fragment shaders are run on each fragment/pixel of the geometry.
        private readonly string FragmentShaderSource = @"
        #version 330 core

        in vec3 vertColor;

        out vec4 FragColor;

        void main()
        {
            FragColor = vec4(vertColor, 1.0f);
        }
        ";

        public unsafe void Render(double obj)
        {
            bool createdBuffersForMesh = false;
            foreach (var mg in meshGroups)
            {
                createdBuffersForMesh |= CreateVertexAndElementBufferObjects(mg);
            }
            if (createdBuffersForMesh) return;

            // Clear the color channel and the depth channel
            Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

            // Bind geometry and shader
            Gl.BindVertexArray(vao);
            
            Gl.UseProgram(shaderProgram);

            cubeTransform.pitchYawRoll.Y += (float)((MathF.PI / 2) * obj);
            cubeTransform.pitchYawRoll.Z += (float)((MathF.PI / 2) * obj);
            int loc = Gl.GetUniformLocation(shaderProgram, "model");
            var modelMat = cubeTransform.Matrix;
            Gl.UniformMatrix4(loc, 1, false, (float*)&modelMat);

            var viewMat = Matrix4X4<float>.Identity * Matrix4X4.CreateTranslation(0.0f, 0, -2);
            var perspectiveMat = Matrix4X4.CreatePerspectiveFieldOfView(MathF.PI / 2, 16.0f / 9.0f, 0.1f, 100.0f);
            loc = Gl.GetUniformLocation(shaderProgram, "view");
            Gl.UniformMatrix4(loc, 1, false, (float*)&viewMat);
            loc = Gl.GetUniformLocation(shaderProgram, "perspective");
            Gl.UniformMatrix4(loc, 1, false, (float*)&perspectiveMat);

            foreach (var mg in meshGroups)
            {
                Gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbos[mg.name]);
                Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebos[mg.name]);
            }

            // Draw the geometry
            Gl.DrawElements(PrimitiveType.Triangles, (uint)testCube.meshes[0].indices.Length, DrawElementsType.UnsignedInt, null);
        }

        private unsafe bool CreateVertexAndElementBufferObjects(MeshGroup meshGroup)
        {
            if (vbos.ContainsKey(meshGroup.name) && ebos.ContainsKey(meshGroup.name)) return false;

            // Initializing a buffer that holds the vertex data
            uint vbo = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            fixed (void* v = &meshGroup.meshes[0].vertices[0])
            {
                Gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(meshGroup.meshes[0].vertices.Length * Vertex.Stride), v, BufferUsageARB.DynamicDraw);
            }

            // Initializing buffer that holds the index data
            uint ebo = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
            fixed (void* i = &meshGroup.meshes[0].indices[0])
            {
                Gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(meshGroup.meshes[0].indices.Length * sizeof(uint)), i, BufferUsageARB.DynamicDraw);
            }

            // Add our created objects to the corresponding arrays
            vbos.Add(meshGroup.name, vbo);
            ebos.Add(meshGroup.name, ebo);

            // Tell OpenGL how to give the data to the shaders
            int offset = sizeof(Vector3D<float>);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)Vertex.Stride, null);
            Gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, (uint)Vertex.Stride, (void*)offset);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, (uint)Vertex.Stride, (void*)(offset * 2));
            Gl.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, (uint)Vertex.Stride, (void*)(offset * 3));
            Gl.EnableVertexArrayAttrib(vao, 0);
            Gl.EnableVertexArrayAttrib(vao, 1);
            Gl.EnableVertexArrayAttrib(vao, 2);
            Gl.EnableVertexArrayAttrib(vao, 3);

            return true;
        }

        #region Initialization Code
        public Renderer(IWindow window)
        {
            this.window = window;
        }

        public void OnClose()
        {
            foreach (var v in vbos.Values) Gl.DeleteBuffer(v);
            foreach (var v in ebos.Values) Gl.DeleteBuffer(v);
            Gl.DeleteVertexArray(vao);
            Gl.DeleteProgram(shaderProgram);
        }

        /// <summary>
        /// To be called on <see cref="App"/> Load. Inits GL objects
        /// </summary>
        public unsafe void Init()
        {
            // Create GL API for drawing to the screen
            Gl = GL.GetApi(window);

            // Creating a vertex array
            vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            testCube = AssetLoader.Instance.LoadOrGetMeshList("cube.obj");

            meshGroups.Add(testCube);

            uint fragment = CreateShader("FragmentShader", FragmentShaderSource);
            uint vertex = CreateShader("VertexShader", VertexShaderSource, ShaderType.VertexShader);

            CreateProgram(vertex, fragment);

            // Delete individual shaders
            DetachAndDeleteShader(vertex);
            DetachAndDeleteShader(fragment);

            cubeTransform.position = new Vector3D<float> { X= 0, Y = 0, Z= 0 };
            cubeTransform.scale = new Vector3D<float> { X = 1, Y = 1, Z= 1 };
            cubeTransform.pitchYawRoll = Vector3D<float>.Zero;

            Gl.CullFace(CullFaceMode.Front);
        }

        private uint CreateShader(string shaderName, string shaderCode, ShaderType shaderType = ShaderType.FragmentShader)
        {
            uint shaderPtr = Gl.CreateShader(shaderType);
            Gl.ShaderSource(shaderPtr, shaderCode);
            Gl.CompileShader(shaderPtr);

            // Log any errors
            var infoLog = Gl.GetShaderInfoLog(shaderPtr);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                Logger.Warn($"Error compiling shader {shaderName} : {infoLog}");
            }

            return shaderPtr;
        }

        private void DetachAndDeleteShader(uint shader)
        {
            Gl.DetachShader(shaderProgram, shader);
            Gl.DeleteShader(shader);
        }

        private void CreateProgram(params uint[] shaders)
        {
            if (shaderProgram != 0) Gl.DeleteProgram(shaderProgram);

            // Create shader program and attach passed in shaders
            shaderProgram = Gl.CreateProgram();
            foreach (var shader in shaders) Gl.AttachShader(shaderProgram, shader);

            // Attempt to link
            Gl.LinkProgram(shaderProgram);

            // Check for errors and log
            Gl.GetProgram(shaderProgram, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Logger.Warn($"Error linking shader {Gl.GetProgramInfoLog(shaderProgram)}");
            }
        }
        #endregion
    }
}
