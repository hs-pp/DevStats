using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DevStatsSystem.Core.SerializedData
{
    // Add this to child SavedData classes that should be project-specific. It will create a new saved data file for
    // every unique project.
    internal class IsProjectSpecificAttribute : Attribute { }

    /// <summary>
    /// Quick singleton-ification of data that needs to be saved to disk. Saves to EditorPrefs.
    /// </summary>
    internal abstract class SavedData<T> where T : new()
    {
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

        public static void Reset()
        {
            m_instance = new T();
            string saveKey = GetSaveKey(typeof(T).GetCustomAttribute<IsProjectSpecificAttribute>() != null);
            EditorPrefs.SetString(saveKey, JsonUtility.ToJson(m_instance));
        }

        private static string GetSaveKey(bool isProjectSpecific)
        {
            return isProjectSpecific ? $"{Application.productName}_{typeof(T).Name}" : typeof(T).Name;
        }
        
        private static T Load()
        {
            string saveKey = GetSaveKey(typeof(T).GetCustomAttribute<IsProjectSpecificAttribute>() != null);
            if (EditorPrefs.HasKey(saveKey))
            {
                return JsonUtility.FromJson<T>(EditorPrefs.GetString(saveKey));
            }

            return new T();
        }

        public void Save()
        {
            string saveKey = GetSaveKey(GetType().GetCustomAttribute<IsProjectSpecificAttribute>() != null);
            EditorPrefs.SetString(saveKey, JsonUtility.ToJson(this));
        }
    }
}