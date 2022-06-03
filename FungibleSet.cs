using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Collections;

namespace Tyfyter.Utils {
    [DebuggerDisplay("Count = {Count}, Total = {Total}")]
    public class FungibleSet<T> : IDictionary<T, int> {
        public readonly EqualityComparer<T> KeyComparer;
        public readonly bool AddOnSet = false;

        public Dictionary<T, int> Entries { get; private set; }
		public int this[T key] {
			get => Entries.ContainsKey(key) ? Entries[key] : 0;
			set {
				if (value > 0) {
					Entries[key] = value;
				} else {
					Entries.Remove(key);
				}
			}
		}
		public FungibleSet() {
			Entries = new Dictionary<T, int>();
		}
		public FungibleSet(Dictionary<T, int> entries) {
			Entries = entries;
		}
		public FungibleSet(IEnumerable<KeyValuePair<T, int>> entries) {
			Entries = new Dictionary<T, int>();
			foreach (KeyValuePair<T, int> entry in entries) {
				this[entry.Key] += entry.Value;
			}
		}
		public int Total => Entries.Values.Sum();
		public ICollection<T> Keys => Entries.Keys;
		public ICollection<int> Values => Entries.Values;
		public int Count => Entries.Count;
		public bool IsReadOnly => ((ICollection<KeyValuePair<T, int>>)Entries).IsReadOnly;
		public void Add(T key, int value) {
			Entries.Add(key, value);
		}
		public void Add(KeyValuePair<T, int> item) {
			((ICollection<KeyValuePair<T, int>>)Entries).Add(item);
		}
		public void Clear() {
			Entries.Clear();
		}
		public bool Contains(KeyValuePair<T, int> item) {
			return Entries.Contains(item);
		}
		public bool ContainsKey(T key) {
			return Entries.ContainsKey(key);
		}
		public void CopyTo(KeyValuePair<T, int>[] array, int arrayIndex) {
			((ICollection<KeyValuePair<T, int>>)Entries).CopyTo(array, arrayIndex);
		}
		public IEnumerator<KeyValuePair<T, int>> GetEnumerator() {
			return Entries.GetEnumerator();
		}
		public bool Remove(T key) {
			return Entries.Remove(key);
		}
		public bool Remove(KeyValuePair<T, int> item) {
			return ((ICollection<KeyValuePair<T, int>>)Entries).Remove(item);
		}
		public bool TryGetValue(T key, out int value) {
			return Entries.TryGetValue(key, out value);
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return Entries.GetEnumerator();
		}
	}
}