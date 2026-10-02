namespace VGameFoundation.Script.Services.CacheDataService
{
    using System;
    using System.Collections.Generic;
    using VContainer.Unity;
    using VGameFoundation.Scripts.Utilities.LogService;
    
    public class RuntimeSingletonCache : ICachedData
    {
        private readonly Dictionary<Type, object> singletons = new(); 

        public void AddSingleton<T>(T instance) where T : class
        {
            if (instance == null) return;

            var type = typeof(T);
            
            if (this.singletons.ContainsKey(type))
            {
                LogService.LogWarning($"[RuntimeSingletonCache] Overwriting existing singleton for type: {type.Name}");
                this.singletons[type] = instance;
            }
            else
            {
                this.singletons.Add(type, instance);
            }
        }
        
        public void RemoveSingleton<T>() where T : class
        {
            this.singletons.Remove(typeof(T));
        }
        
        public void RemoveSingleton<T>(T instance) where T : class
        {
            var type = typeof(T);
            if (this.singletons.TryGetValue(type, out var cachedInstance))
            {
                if (ReferenceEquals(cachedInstance, instance))
                {
                    this.singletons.Remove(type);
                }
            }
        }
        
        public bool TryGetSingleton<T>(out T result) where T : class
        {
            if (this.singletons.TryGetValue(typeof(T), out var obj))
            {
                result = (T)obj;
                return true;
            }
            result = null;
            return false;
        }
        
        public T GetSingleton<T>() where T : class
        {
            var type = typeof(T);
            if (this.singletons.TryGetValue(type, out var obj))
            {
                return (T)obj;
            }

            LogService.LogWarning($"[RuntimeSingletonCache] Singleton of type '{type.Name}' not found.");
            return null;
        }

        void IInitializable.Initialize()
        {
        }
    }
}