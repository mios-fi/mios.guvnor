using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Guvnor.Configuration {
	public class ConfigurationElementCollection<T> : ConfigurationElementCollection, IEnumerable<T> where T : KeyedConfigurationElement, new() {
		public override ConfigurationElementCollectionType CollectionType {
			get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
		}
		protected override ConfigurationElement CreateNewElement() {
			return new T();
		}
		protected override Object GetElementKey(ConfigurationElement element) {
			return ((T)element).Key;
		}
		public T this[int index] {
			get { return (T)BaseGet(index); }
			set {
				if(BaseGet(index) != null) BaseRemoveAt(index);
				BaseAdd(index, value);
			}
		}
		new public T this[string type] {
			get { return (T)BaseGet(type); }
		}
		public int IndexOf(T paymentType) {
			return BaseIndexOf(paymentType);
		}
		public void Add(T paymentType) {
			BaseAdd(paymentType);
		}
		protected override void BaseAdd(ConfigurationElement element) {
			BaseAdd(element, false);
		}
		public void Remove(T paymentType) {
			if(BaseIndexOf(paymentType) >= 0) BaseRemove(paymentType.Key);
		}
		public void RemoveAt(int index) {
			BaseRemoveAt(index);
		}
		public void Remove(string type) {
			BaseRemove(type);
		}
		public void Clear() {
			BaseClear();
		}
		public IDictionary<TKey,TValue> ToDictionary<TKey,TValue>(Func<object,TKey> keySelector, Func<T,TValue> valueSelector) {
			return BaseGetAllKeys().ToDictionary(keySelector,t=>valueSelector((T)BaseGet(t)));
		}

		public new IEnumerator<T> GetEnumerator() {
			return BaseGetAllKeys().Select(t => BaseGet(t)).Cast<T>().GetEnumerator();
		}
	}
	public abstract class KeyedConfigurationElement : ConfigurationElement {
		public abstract Object Key { get; }
	}
}