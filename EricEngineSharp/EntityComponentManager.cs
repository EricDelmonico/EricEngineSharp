using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;

namespace EricEngineSharp
{
    // Doesn't need to do anything inherently
    internal interface IComponent 
    {
        bool IsValid { get; }
    }

    /// <summary>
    /// Manages component and entity memory. Contains helper methods for common ECS operations.
    /// </summary>
    internal class EntityComponentManager
    {
        private static EntityComponentManager instance;
        public static EntityComponentManager Instance => instance ?? (instance = new EntityComponentManager());

        /// <summary>
        /// The max amount of *each* component that can exist (as opposed to the max for all components collectively)
        /// </summary>
        public const int MaxComponents = 10000;
        /// <summary>
        /// Contains an array of each component type, which are created for every class implementing IComponent in this class's constructor
        /// </summary>
        private Dictionary<Type, object> componentArrays = new Dictionary<Type, object>();
        private List<Entity> entities = new List<Entity>();

        /// <summary>
        /// Creates an <see cref="EntityComponentManager"/> instance, filling out the <see cref="componentArrays"/> 
        /// Dictionary in the process.
        /// </summary>
        public EntityComponentManager()
        {
            var allComponentTypes = ECSInternalHelperMethods.GetAllComponentTypes();
            foreach (var componentType in allComponentTypes) 
                componentArrays.Add(componentType, Array.CreateInstance(elementType: componentType, length: MaxComponents));
        }

        public Entity AddEntity()
        {
            var e = new Entity();
            entities.Add(e);
            return e;
        }

        /// <summary>
        /// Adds the passed in component <paramref name="c"/> of type ComponentType to the entity <paramref name="e"/>
        /// </summary>
        /// <typeparam name="ComponentType">The type of the component to add</typeparam>
        /// <param name="e">The entity to add the component to</param>
        /// <param name="c">The component to add</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void AddComponent<ComponentType>(Entity e, ComponentType c) where ComponentType : IComponent
        {
            var componentArray = componentArrays[typeof(ComponentType)] as ComponentType[];
            // Find the next empty component spot
            int i = 0;
            while (i < componentArray.Length && componentArray[i] != null && componentArray[i].IsValid) i++;
            
            if (i >= componentArray.Length)
                throw new InvalidOperationException($"Component limit reached. There cannot be more than {MaxComponents} components of any one type.");

            // Store the component and keep track of the entity in charge of it
            componentArray[i] = c;
            e.componentIndices[typeof(ComponentType)] = i;
        }

        public ComponentType[] GetComponentArray<ComponentType>() where ComponentType : IComponent =>
            componentArrays[typeof(ComponentType)] as ComponentType[];
    }

    /// <summary>
    /// Tracks an index for each <see cref="IComponent"/> 
    /// </summary>
    internal class Entity
    {
        /// <summary>
        /// Index of this entity in 
        /// </summary>
        public int EntityIndex { get; set; }

        /// <summary>
        /// Indices for each type corresponding to the arrays in <see cref="EntityComponentManager.componentArrays"/>
        /// </summary>
        public Dictionary<Type, int> componentIndices = new Dictionary<Type, int>();

        /// <summary>
        /// Creates a new <see cref="Entity"/>, filling <see cref="componentIndices"/> with invalid componentIndices (-1)
        /// </summary>
        public Entity()
        {
            var allComponentTypes = ECSInternalHelperMethods.GetAllComponentTypes();
            foreach (var componentType in allComponentTypes)
                componentIndices.Add(key: componentType, value: -1);
        }
    }

    internal static class ECSInternalHelperMethods
    {
        /// <summary>
        /// Grab all types that implement <see cref="IComponent"/>
        /// </summary>
        /// <returns>An <see cref="IEnumerable{System.Type}"/> full of all component types</returns>
        public static IEnumerable<Type> GetAllComponentTypes()
        {
            var componentInterfaceType = typeof(IComponent);
            return Assembly.GetAssembly(componentInterfaceType)
                .GetTypes()
                .Where(t => componentInterfaceType.IsAssignableFrom(t) && !t.IsInterface);
        }
    }
}
