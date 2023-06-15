using System;
using System.Collections.Generic;
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
}
