namespace Storage.Application
{
    public interface IStorageService
    {
        bool HasKey(string key);
        void DeleteKey(string key);
        void Save();

        int GetInt(string key, int defaultValue = 0);
        void SetInt(string key, int value);

        float GetFloat(string key, float defaultValue = 0f);
        void SetFloat(string key, float value);

        string GetString(string key, string defaultValue = null);
        void SetString(string key, string value);
    }
}
