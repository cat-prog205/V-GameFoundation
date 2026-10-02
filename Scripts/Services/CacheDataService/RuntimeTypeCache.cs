namespace VGameFoundation.Script.Services.CacheDataService
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A runtime cache system that groups and stores objects by their Class Type.
    /// Use this to manage collections like all Enemies, all Bullets, etc.
    /// </summary>
    public class RuntimeTypeCache : ICachedData
    {
        private readonly Dictionary<Type, List<object>> masterMap = new();
        
        public void Register<T>(T instance) where T : class
        {
            if (instance == null) return;

            var type = typeof(T);

            if (!this.masterMap.TryGetValue(type, out var list))
            {
                list                 = new List<object>();
                this.masterMap[type] = list;
            }

            if (!list.Contains(instance))
            {
                list.Add(instance);
            }
        }
        
        public void Unregister<T>(T instance) where T : class
        {
            if (instance == null) return;

            var type = typeof(T);
            if (this.masterMap.TryGetValue(type, out var list))
            {
                list.Remove(instance);

                if (list.Count == 0)
                {
                    this.masterMap.Remove(type);
                }
            }
        }
        
        public IEnumerable<T> GetAll<T>() where T : class
        {
            var type = typeof(T);
            if (this.masterMap.TryGetValue(type, out var list))
            {
                foreach (var item in list)
                {
                    yield return (T)item;
                }
            }
        }
        
        public void Initialize()
        {
        }
    }
}