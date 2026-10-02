namespace VGameFoundation.Scripts.Services.LocalData
{
    using System;
    using Cysharp.Threading.Tasks;

    public interface IHandleUserDataServices
    {
        /// <summary>
        /// True when at least one key could not be read and was reset to defaults. A remote sync
        /// should refuse to upload in that case, or it would overwrite a healthy remote save with
        /// what is effectively a brand new one.
        /// </summary>
        bool HasCorruptedData { get; }

        /// <summary>
        /// Save a class data to local
        /// </summary>
        /// <param name="data">class data</param>
        /// <param name="force"> if true, save data immediately to local</param>
        /// <typeparam name="T"> type of class</typeparam>
        public UniTask Save<T>(T data, bool force = false) where T : class, ILocalData;

        /// <summary>
        /// Load data from local
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public UniTask<T> Load<T>() where T : class, ILocalData;

        public UniTask<ILocalData[]> Load(params Type[] types);

        public UniTask SaveAll();
    }
}
