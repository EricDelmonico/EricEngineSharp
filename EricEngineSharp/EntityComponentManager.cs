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
    // Doesn't need to do anything inherently
    internal interface IComponent 
    {
        public Entity Container { get; }

        void Start();
        void Update(double dt);
        void OnDestroy();
    }

    /// <summary>
    /// Manages component and entity memory. Contains helper methods for common ECS operations.
    /// </summary>
    internal class EntityComponentManager
    {
        private static EntityComponentManager instance;
        public static EntityComponentManager Instance => instance ?? (instance = new EntityComponentManager());

        public List<Entity> Entities = new List<Entity>();

        public Entity AddEntity()
        {
            Entity e = new Entity();
            Entities.Add(e);
            return e;
        }

        public void DestroyEntity(Entity e)
        {
            Entities.Remove(e);
            e.OnDestroy();
        }
    }

    internal class Entity
    {
        private Dictionary<Type, IComponent> components = new Dictionary<Type, IComponent>();

        public Entity()
        {
            var componentTypes = ECSInternalHelperMethods.GetAllComponentTypes();
            foreach (var componentType in componentTypes)
            {
                components.Add(componentType, null);
            }
        }

        public ComponentType GetComponent<ComponentType>() where ComponentType : class, IComponent
        {
            return components[typeof(ComponentType)] as ComponentType;
        }

        public void AddComponent<ComponentType>(ComponentType component) where ComponentType : class, IComponent
        {
            if (components[typeof(ComponentType)] == null)
                components[typeof(ComponentType)] = component;
        }

        public void RemoveComponent<ComponentType>() where ComponentType : class, IComponent
        {
            components[typeof(ComponentType)] = null;
        }

        #region Component Functions
        public void Start()
        {
            foreach (var component in components)
            {
                component.Value?.Start();
            }
        }

        public void Update(double dt)
        {
            foreach (var component in components)
            {
                component.Value?.Update(dt);
            }
        }

        public void OnDestroy()
        {
            foreach (var component in components)
            {
                component.Value?.OnDestroy();
            }
        }
        #endregion
    }

    internal static class ECSInternalHelperMethods
    {
        private static List<Type> componentTypes;

        /// <summary>
        /// Grab all types that implement <see cref="IComponent"/>
        /// </summary>
        /// <returns>An <see cref="IEnumerable{System.Type}"/> full of all component types</returns>
        public static List<Type> GetAllComponentTypes()
        {
            if (componentTypes != null) return componentTypes;

            var componentInterfaceType = typeof(IComponent);
            return (componentTypes = Assembly.GetAssembly(componentInterfaceType)
                .GetTypes()
                .Where(t => componentInterfaceType.IsAssignableFrom(t) && !t.IsInterface).ToList());
        }
    }
}
