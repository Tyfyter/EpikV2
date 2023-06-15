using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EpikV2 {
	public static class Operators<T> {
		static Operators() {
			AssemblyLoadContext.GetLoadContext(typeof(T).Assembly).Unloading += Unload;
		}
		static void Unload(AssemblyLoadContext context) {
			Add = null;
			Subtract = null;
			Multiply = null;
			And = null;
			Or = null;
			Xor = null;
		}
		public static Func<T, T, T> Add { get; set; } = GenerateBinaryOperator(OpCodes.Add, "op_Addition");
		public static Func<T, T, T> Subtract { get; set; } = GenerateBinaryOperator(OpCodes.Sub, "op_Subtraction");
		public static Func<T, T, T> Multiply { get; set; } = GenerateBinaryOperator(OpCodes.Mul, "op_Multiply");
		public static Func<T, T, T> And { get; set; } = GenerateBinaryOperator(OpCodes.And, "op_Multiply");
		public static Func<T, T, T> Or { get; set; } = GenerateBinaryOperator(OpCodes.Or, "op_Multiply");
		public static Func<T, T, T> Xor { get; set; } = GenerateBinaryOperator(OpCodes.Xor, "op_Multiply");
		public static Func<T, T, bool> Ceq { get; set; } = GenerateComparisonOperator("op_Equality", OpCodes.Ceq);
		public static Func<T, T, bool> Cneq { get; set; } = GenerateComparisonOperator("op_Inequality", OpCodes.Ceq, OpCodes.Ldc_I4_0, OpCodes.Ceq);
		static Func<T, T, T> GenerateBinaryOperator(OpCode opCode, string opMethod) {
			MethodInfo meth = typeof(T).GetMethod(opMethod, new Type[] { typeof(T), typeof(T) });
			if (meth is null) {
				string methodName = $"Operator<{typeof(T).FullName}>._{opCode.Name.ToLower()}";
				DynamicMethod getterMethod = new(methodName, typeof(T), new Type[] { typeof(T), typeof(T) }, true);
				ILGenerator gen = getterMethod.GetILGenerator();

				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(opCode);
				gen.Emit(OpCodes.Ret);

				return getterMethod.CreateDelegate<Func<T, T, T>>();
			} else {
				return meth.CreateDelegate<Func<T, T, T>>();
			}
		}
		static Func<T, T, bool> GenerateComparisonOperator(string opMethod, params OpCode[] opCodes) {
			MethodInfo meth = typeof(T).GetMethod(opMethod, new Type[] { typeof(T), typeof(T) });
			if (meth is null) {
				string methodName = $"Operator<{typeof(T).FullName}>._{opMethod}";
				DynamicMethod getterMethod = new(methodName, typeof(T), new Type[] { typeof(T), typeof(T) }, true);
				ILGenerator gen = getterMethod.GetILGenerator();

				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldarg_1);
				for (int i = 0; i < opCodes.Length; i++) {
					gen.Emit(opCodes[i]);
				}
				gen.Emit(OpCodes.Ret);

				return getterMethod.CreateDelegate<Func<T, T, bool>>();
			} else {
				return meth.CreateDelegate<Func<T, T, bool>>();
			}
		}
	}
	public class MergingDictionary<TKey, TValue> : IDictionary<TKey, TValue> {
		readonly IDictionary<TKey, TValue> innerDict;
		readonly Func<TValue, TValue, TValue> mergeOperation;
		public MergingDictionary() : this(new Dictionary<TKey, TValue>(), MergeOperations.Add) { }
		public MergingDictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, MergeOperations.Add) { }
		public MergingDictionary(IDictionary<TKey, TValue> dictionary, MergeOperations operation) : this(dictionary, operation switch {
			MergeOperations.Add => Operators<TValue>.Add,
			MergeOperations.Subtract => Operators<TValue>.Subtract,
			MergeOperations.Mulitply => Operators<TValue>.Multiply,
			MergeOperations.And => Operators<TValue>.And,
			MergeOperations.Or => Operators<TValue>.Or,
			MergeOperations.Xor => Operators<TValue>.Xor,
			_ => throw new ArgumentException(operation.ToString(), nameof(operation)),
		}) {}
		public MergingDictionary(Func<TValue, TValue, TValue> operation) : this(new Dictionary<TKey, TValue>(), operation) {}
		public MergingDictionary(IDictionary<TKey, TValue> dictionary, Func<TValue, TValue, TValue> operation) {
			innerDict = dictionary;
			mergeOperation = operation;
		}
		public void Add(TKey key, TValue value) {
			if (TryGetValue(key, out TValue oldValue)) {
				innerDict[key] = mergeOperation(oldValue, value);
			} else {
				innerDict.Add(key, value);
			}
		}
		#region implementations
		public TValue this[TKey key] { get => innerDict[key]; set => innerDict[key] = value; }
		public ICollection<TKey> Keys => innerDict.Keys;
		public ICollection<TValue> Values => innerDict.Values;
		public int Count => innerDict.Count;
		public bool IsReadOnly => innerDict.IsReadOnly;
		public void Add(KeyValuePair<TKey, TValue> item) {
			innerDict.Add(item);
		}
		public void Clear() {
			innerDict.Clear();
		}
		public bool Contains(KeyValuePair<TKey, TValue> item) {
			return innerDict.Contains(item);
		}
		public bool ContainsKey(TKey key) {
			return innerDict.ContainsKey(key);
		}
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
			innerDict.CopyTo(array, arrayIndex);
		}
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
			return innerDict.GetEnumerator();
		}
		public bool Remove(TKey key) {
			return innerDict.Remove(key);
		}
		public bool Remove(KeyValuePair<TKey, TValue> item) {
			return innerDict.Remove(item);
		}
		public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) {
			return innerDict.TryGetValue(key, out value);
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return innerDict.GetEnumerator();
		}
		#endregion implementations
		public enum MergeOperations {
			Add,
			Subtract,
			Mulitply,
			And,
			Or,
			Xor
		}
	}
	public class MergingListDictionary<TKey, TValue> : IDictionary<TKey, List<TValue>> {
		readonly IDictionary<TKey, List<TValue>> innerDict;
		public MergingListDictionary() : this(new Dictionary<TKey, List<TValue>>()) { }
		public MergingListDictionary(IDictionary<TKey, List<TValue>> dictionary) {
			innerDict = dictionary;
		}
		public void Add(TKey key, TValue value) {
			if (ContainsKey(key)) {
				innerDict[key].Add(value);
			} else {
				innerDict.Add(key, new() { value });
			}
		}
		#region implementations
		public List<TValue> this[TKey key] { get => innerDict[key]; set => innerDict[key] = value; }
		public ICollection<TKey> Keys => innerDict.Keys;
		public ICollection<List<TValue>> Values => innerDict.Values;
		public int Count => innerDict.Count;
		public bool IsReadOnly => innerDict.IsReadOnly;
		public void Add(TKey key, List<TValue> value) {
			innerDict.Add(key, value);
		}
		public void Add(KeyValuePair<TKey, List<TValue>> item) {
			innerDict.Add(item);
		}
		public void Clear() {
			innerDict.Clear();
		}
		public bool Contains(KeyValuePair<TKey, List<TValue>> item) {
			return innerDict.Contains(item);
		}
		public bool ContainsKey(TKey key) {
			return innerDict.ContainsKey(key);
		}
		public void CopyTo(KeyValuePair<TKey, List<TValue>>[] array, int arrayIndex) {
			innerDict.CopyTo(array, arrayIndex);
		}
		public IEnumerator<KeyValuePair<TKey, List<TValue>>> GetEnumerator() {
			return innerDict.GetEnumerator();
		}
		public bool Remove(TKey key) {
			return innerDict.Remove(key);
		}
		public bool Remove(KeyValuePair<TKey, List<TValue>> item) {
			return innerDict.Remove(item);
		}
		public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out List<TValue> value) {
			return innerDict.TryGetValue(key, out value);
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return innerDict.GetEnumerator();
		}
		#endregion implementations
	}
}
