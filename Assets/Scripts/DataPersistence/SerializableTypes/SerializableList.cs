using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[System.Serializable]
public class SerializableList<TValue> : List<TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TValue> values = new List<TValue>();

    /**
     * Save the list to serializable lists.
     */
    public void OnBeforeSerialize()
    {
        values.Clear();

        foreach (TValue value in this)
        {
            values.Add(value);
        }
    }

    /**
     * Load the sorted dictionary from serialized lists.
     */
    public void OnAfterDeserialize()
    {
        this.Clear();

        for (int i = 0; i < values.Count; i++)
        {
            this.Add(values[i]);
        }
    }
}
