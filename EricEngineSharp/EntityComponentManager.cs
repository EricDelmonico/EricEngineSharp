using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using System.ComponentModel;

namespace EricEngineSharp
{
    using Entity = Int32;

    // Doesn't need to do anything inherently
    internal interface IComponent { }

    /// <summary>
    /// Manages component and entity memory. Contains helper methods for common ECS operations.
    /// </summary>
    internal class EntityComponentManager
    {
        private static EntityComponentManager instance;
        public static EntityComponentManager Instance => instance ?? (instance = new EntityComponentManager());

        /// <summary>
        /// The max amount of entities and of *each* component type that can exist (as opposed to the max for all components collectively)
        /// </summary>
        public const int MaxEntities = 1024;

        /// <summary>
        /// Contains an array of each component type, which are created for every class implementing IComponent in this class's constructor
        /// </summary>
        private Dictionary<Type, IComponent[]> componentArrays = new Dictionary<Type, IComponent[]>();

        private bool[] entities = new bool[MaxEntities];

        /// <summary>
        /// Contains a list of entities that contain the component of the corresponding type
        /// </summary>
        private Dictionary<Type, List<Entity>> componentEntities = new Dictionary<Type, List<Entity>>();

        /// <summary>
        /// Creates an <see cref="EntityComponentManager"/> instance, filling out the <see cref="componentArrays"/> 
        /// Dictionary in the process.
        /// </summary>
        public EntityComponentManager()
        {
            var allComponentTypes = ECSInternalHelperMethods.GetAllComponentTypes();
            foreach (var componentType in allComponentTypes)
            {
                componentArrays.Add(componentType, Array.CreateInstance(elementType: componentType, length: MaxEntities) as IComponent[]);
                componentEntities.Add(componentType, new List<Entity>());
            }
        }

        public Entity AddEntity()
        {
            int nextIndex = 0;
            while (entities[nextIndex] && nextIndex < MaxEntities)
            {
                nextIndex++;
            }

            if (nextIndex >= MaxEntities) return -1;

            entities[nextIndex] = true;
            return nextIndex;
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
            componentArrays[typeof(ComponentType)][e] = c;
            componentEntities[typeof(ComponentType)].Add(e);
        }

        public List<Entity> GetEntitiesWithComponent<ComponentType>() where ComponentType : IComponent => componentEntities[typeof(ComponentType)];

        public List<Entity> GetEntitiesWithComponents(params Type[] componentTypes)
        {
            IEnumerable<Entity> intersection = componentEntities[componentTypes[0]];
            for (int i = 1; i < componentTypes.Length; i++)
            {
                var nextEntityList = componentEntities[componentTypes[i]];
                intersection = intersection.Intersect(nextEntityList);
            }
            return intersection.ToList();
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
