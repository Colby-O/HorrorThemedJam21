using PlazmaGames.Attribute;
using PlazmaGames.Runtime.DataStructures;
using UnityEngine;

namespace HTJ21
{
    public enum PickupableItem
    {
        FlashLight,
        BathroomKey
    }

    public class PickupManager : MonoBehaviour
    {
        [SerializeField, ReadOnly] SerializableDictionary<PickupableItem, bool> _items;

        public void Pickup(PickupableItem item)
        {
            if (_items.ContainsKey(item)) _items[item] = true;
            else _items.Add(item, true);
        }

        public void Drop(PickupableItem item)
        {
            if (_items.ContainsKey(item)) _items[item] = false;
            else _items.Add(item, false);
        }

        public bool HasItem(PickupableItem item)
        {
            return _items.ContainsKey(item) && _items[item];
        }

        private void Awake()
        {
            _items = new SerializableDictionary<PickupableItem, bool>();
        }
    }
}
