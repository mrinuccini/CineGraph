using System;
using UnityEngine;

namespace CineGraph
{
    /// <summary>
    ///     Base class for any variables in the blackboard
    /// </summary>
    [System.Serializable]
    public class CineGraphVariable
    {
        public string Name {
            get => m_name;
            set {
                m_name = value;
                OnNameChanged?.Invoke(m_name);
            }
        }

        public string GUID;

        public event Action<string> OnNameChanged;

        [SerializeField] private string m_name;

        public virtual void OnDelete()
        {
            Debug.Log("Deleted Variable");
        }
        
        public CineGraphVariable(string name, string guid)
        {
            Name = name;
            GUID = guid;
        }
    }
}
