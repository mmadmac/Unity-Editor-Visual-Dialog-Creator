using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    
    // Diccionario para almacenar Item -> Cantidad
    public Dictionary<string, int> inventory = new Dictionary<string, int>();

    void Awake() { 
        if (Instance == null) Instance = this; 
    }

    public void AddItem(string name, int amount) {
        if (inventory.ContainsKey(name)) inventory[name] += amount;
        else inventory.Add(name, amount);
        Debug.Log($"Inventario: {name} +{amount} (Total: {inventory[name]})");
    }

    public void RemoveItem(string name, int amount) {
        if (inventory.ContainsKey(name)) {
            inventory[name] -= amount;
            if (inventory[name] <= 0) inventory.Remove(name);
        }
    }

    public int GetAmount(string name) {
        return inventory.ContainsKey(name) ? inventory[name] : 0;
    }

    public bool HasEnough(string name, int amount) {
        return GetAmount(name) >= amount;
    }
}