using Logger.Facade;
using UnityEngine;

namespace Storage.Application
{
    /// <summary>
    /// PlayerPrefs-backed preferences. Call <see cref="Save"/> after meaningful writes (or rely on periodic OS flush).
    /// </summary>
    public sealed class StorageService : IStorageService
    {
        private readonly ILoggerFacade _logger;

        public StorageService(ILoggerFacade logger)
        {
            _logger = logger;
        }

        public bool HasKey(string key)
        {
            if (!TryValidateKey(key, nameof(HasKey)))
                return false;
            return PlayerPrefs.HasKey(key);
        }

        public void DeleteKey(string key)
        {
            if (!TryValidateKey(key, nameof(DeleteKey)))
                return;
            PlayerPrefs.DeleteKey(key);
        }

        public void Save()
        {
            try
            {
                PlayerPrefs.Save();
            }
            catch (System.Exception ex)
            {
                _logger?.LogWarning($"[Storage] Save failed: {ex.Message}");
            }
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            if (!TryValidateKey(key, nameof(GetInt)))
                return defaultValue;
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public void SetInt(string key, int value)
        {
            if (!TryValidateKey(key, nameof(SetInt)))
                return;
            PlayerPrefs.SetInt(key, value);
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            if (!TryValidateKey(key, nameof(GetFloat)))
                return defaultValue;
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public void SetFloat(string key, float value)
        {
            if (!TryValidateKey(key, nameof(SetFloat)))
                return;
            PlayerPrefs.SetFloat(key, value);
        }

        public string GetString(string key, string defaultValue = null)
        {
            if (!TryValidateKey(key, nameof(GetString)))
                return defaultValue;
            return PlayerPrefs.GetString(key, defaultValue ?? string.Empty);
        }

        public void SetString(string key, string value)
        {
            if (!TryValidateKey(key, nameof(SetString)))
                return;
            PlayerPrefs.SetString(key, value ?? string.Empty);
        }

        private bool TryValidateKey(string key, string operation)
        {
            if (!string.IsNullOrEmpty(key))
                return true;
            _logger?.LogWarning($"[Storage] {operation}: key is null or empty.");
            return false;
        }
    }
}
