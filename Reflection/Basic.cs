﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace EpikV2.Reflection {
	public class Basic : ILoadable {
		public static FastFieldInfo<Delegate, object> _target;
		public void Load(Mod mod) {
			_target = new("_target", BindingFlags.NonPublic | BindingFlags.Instance);
		}
		public void Unload() {
			_target = null;
		}
	}
	public abstract class ReflectionLoader : ILoadable {
		public abstract Type ParentType { get; }
		public void Load(Mod mod) {
			LoadReflections(ParentType);
		}
		public void Unload() {
			UnloadReflections(ParentType);
		}
		~ReflectionLoader() {
			Unload();
		}
		public static T MakeInstanceCaller<T>(Type type, string name) where T : Delegate {
			return MakeInstanceCaller<T>(type.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
		}
		public static T MakeInstanceCaller<T>(MethodInfo method) where T : Delegate {
			string methodName = method.ReflectedType.FullName + ".call_" + method.Name;
			MethodInfo invoke = typeof(T).GetMethod("Invoke");
			ParameterInfo[] parameters = invoke.GetParameters();
			DynamicMethod getterMethod = new(methodName, invoke.ReturnType, parameters.Select(p => p.ParameterType).ToArray(), true);
			ILGenerator gen = getterMethod.GetILGenerator();

			for (int i = 0; i < parameters.Length; i++) {
				gen.Emit(OpCodes.Ldarg_S, i);
			}
			gen.Emit(OpCodes.Call, method);
			gen.Emit(OpCodes.Ret);

			return getterMethod.CreateDelegate<T>();
		}
		public static void LoadReflections(Type type) {
			foreach (var item in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)) {
				string name = item.GetCustomAttribute<ReflectionMemberNameAttribute>()?.MemberName ?? item.Name;
				if (item.FieldType.IsAssignableTo(typeof(Delegate))) {
					Type parentType = item.GetCustomAttribute<ReflectionParentTypeAttribute>().ParentType;
					ParameterInfo[] parameters = item.FieldType.GetMethod("Invoke").GetParameters();
					Type[] paramTypes = new Type[parameters.Length];
					//ParameterModifier paramMods = new ParameterModifier(parameters.Length);
					for (int i = 0; i < parameters.Length; i++) {
						paramTypes[i] = parameters[i].ParameterType;
						//paramMods[i] = parameters[i].ParameterType.IsByRef;
					}
					MethodInfo info = parentType.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, paramTypes);
					if (info.IsStatic) {
						item.SetValue(null, info.CreateDelegate(item.FieldType));
					} else {
						object target;
						if (item.GetCustomAttribute<ReflectionDefaultInstanceAttribute>() is ReflectionDefaultInstanceAttribute defaultObject) {
							static object GetValue(object source, string name) {
								if (source is Type type) {
									source = null;
								} else {
									type = source.GetType();
								}
								BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | (source is null ? BindingFlags.Static : BindingFlags.Instance);
								if (type.GetField(name, flags) is FieldInfo field) {
									return field.GetValue(source);
								} else {
									return type.GetProperty(name, flags).GetValue(source);
								}
							}
							target = GetValue(defaultObject.Type, defaultObject.FieldNames[0]);
							for (int i = 1; i < defaultObject.FieldNames.Length; i++) {
								target = GetValue(target, defaultObject.FieldNames[i]);
							}
						} else {
							target = Activator.CreateInstance(parentType);
						}
						item.SetValue(null, info.CreateDelegate(item.FieldType, target));
					}
				} else if (item.FieldType.IsGenericType) {
					Type genericType = item.FieldType.GetGenericTypeDefinition();
					if (genericType == typeof(FastFieldInfo<,>) || genericType == typeof(FastStaticFieldInfo<,>)) {
						item.SetValue(
							null,
							item.FieldType.GetConstructor([typeof(string), typeof(BindingFlags), typeof(bool)])
							.Invoke([name, BindingFlags.Public | BindingFlags.NonPublic, true])
						);
					} else if (genericType == typeof(FastStaticFieldInfo<>)) {
						item.SetValue(
							null,
							item.FieldType.GetConstructor([typeof(Type), typeof(string), typeof(BindingFlags), typeof(bool)])
							.Invoke([item.GetCustomAttribute<ReflectionParentTypeAttribute>().ParentType, name, BindingFlags.Public | BindingFlags.NonPublic, true])
						);
					}
				}
			}
		}
		public static void UnloadReflections(Type type) {
			foreach (var item in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)) {
				if (!item.FieldType.IsValueType) {
					item.SetValue(null, null);
				}
			}
		}
	}
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class ReflectionMemberNameAttribute : Attribute {
		public string MemberName { get; init; }
		public ReflectionMemberNameAttribute(string memberName) {
			MemberName = memberName;
		}
	}
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class ReflectionParentTypeAttribute : Attribute {
		public Type ParentType { get; init; }
		public ReflectionParentTypeAttribute(Type type) {
			ParentType = type;
		}
	}
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class ReflectionDefaultInstanceAttribute : Attribute {
		public Type Type { get; init; }
		public string[] FieldNames { get; init; }
		public ReflectionDefaultInstanceAttribute(Type type, params string[] fieldNames) {
			Type = type;
			FieldNames = fieldNames;
		}
	}
}
