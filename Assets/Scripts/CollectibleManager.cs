using UnityEngine;
using System;
using System.Collections.Generic;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager Instance { get; private set; }

    // Fired when any type’s count changes: (type, newCount)
    public static event Action<CollectibleType, int> OnTypeCountChanged;

    // Internal counts for each type
    private Dictionary<CollectibleType, int> counts = new Dictionary<CollectibleType, int>()
    {
        { CollectibleType.Coin, 0 },
        { CollectibleType.Gem,  0 },
        { CollectibleType.Key,  0 }
    };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Call to add value for a given type.
    /// </summary>
    public void Add(CollectibleType type, int amount)
    {
        counts[type] += amount;
        OnTypeCountChanged?.Invoke(type, counts[type]);
    }

    /// <summary>
    /// Get the current count for a given type.
    /// </summary>
    public int GetCount(CollectibleType type)
    {
        return counts[type];
    }
}
