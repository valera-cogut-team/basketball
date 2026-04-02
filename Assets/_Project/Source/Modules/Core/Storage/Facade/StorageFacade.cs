using System;
using Storage.Application;

namespace Storage.Facade
{
    public sealed class StorageFacade : IStorageFacade
    {
        private readonly IStorageService _service;

        public StorageFacade(IStorageService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public bool HasKey(string key) => _service.HasKey(key);

        public void DeleteKey(string key) => _service.DeleteKey(key);

        public void Save() => _service.Save();

        public int GetInt(string key, int defaultValue = 0) => _service.GetInt(key, defaultValue);

        public void SetInt(string key, int value) => _service.SetInt(key, value);

        public float GetFloat(string key, float defaultValue = 0f) => _service.GetFloat(key, defaultValue);

        public void SetFloat(string key, float value) => _service.SetFloat(key, value);

        public string GetString(string key, string defaultValue = null) => _service.GetString(key, defaultValue);

        public void SetString(string key, string value) => _service.SetString(key, value);
    }
}
