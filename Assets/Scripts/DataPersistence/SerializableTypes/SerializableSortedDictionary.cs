using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableSortedDictionary<TKey, TValue> : SortedDictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();

    /**
     * Save the sorted directionary to serializable lists.
     */
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    /**
     * Load the sorted dictionary from serialized lists.
     */
    public void OnAfterDeserialize()
    {
        this.Clear();

        if (keys.Count != values.Count)
        {
            Debug.LogError("Tried to deserialize a SerializableDictionary, but the amount of keys (" + keys.Count + ") did not equal the amount of values (" + values.Count + ")." );
        }

        for (int i = 0; i < keys.Count; i++)
        {
            this.Add(keys[i], values[i]);
        }
    }
}
