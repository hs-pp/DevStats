using UnityEditor;
using UnityEngine;

namespace DevStatsSystem.Core.SerializedData
{
    internal class SavedData<T> where T : new()
    {
        private static string SAVEKEY = typeof(T).Name;
        
        private static T m_instance;
        public static T Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = Load();
                }

                return m_instance;
            }
        }
        
        private static T Load()
        {
            if (EditorPrefs.HasKey(SAVEKEY))
            {
                return JsonUtility.FromJson<T>(EditorPrefs.GetString(SAVEKEY));
            }

            return new T();
        }

        public void Save()
        {
            EditorPrefs.SetString(SAVEKEY, JsonUtility.ToJson(this));
        }
    }
}