using System;
using System.Linq;
using System.Reflection;

namespace OrangeBugReloaded.Core.Foundation
{
    /// <summary>
    /// Provides easy access to common reflection tasks.
    /// This is the UWP version.
    /// </summary>
    public class Reflector
    {
        private Assembly _assembly;

        /// <summary>
        /// Obtains a <see cref="Reflector"/> for the assembly
        /// the <see cref="Reflector"/> type is contained in.
        /// </summary>
        public static Reflector Default { get; } = For<Reflector>();

        /// <summary>
        /// Obtains a <see cref="Reflector"/> for the assembly that
        /// contains the specified type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns><see cref="Reflector"/></returns>
        public static Reflector For<T>() => new Reflector { _assembly = typeof(T).GetTypeInfo().Assembly };

        /// <summary>
        /// Gets the type with the specified name.
        /// </summary>
        /// <param name="typeName">Type name</param>
        /// <returns>Type</returns>
        public Type GetType(string typeName)
        {
            return Type.GetType(typeName) ??
                _assembly.GetTypes().FirstOrDefault(o => o.Name == typeName);
        }

        /// <summary>
        /// Gets types that derive (directly or indirectly) from
        /// <typeparamref name="TBase"/>.
        /// </summary>
        /// <typeparam name="TBase">Base class</typeparam>
        /// <returns>Types</returns>
        public Type[] GetTypes<TBase>() => GetTypes(typeof(TBase));

        /// <summary>
        /// Gets types that derive (directly or indirectly) from
        /// <paramref name="baseType"/>.
        /// </summary>
        /// <param name="baseType">Base class</param>
        /// <returns>Types</returns>
        public Type[] GetTypes(Type baseType)
        {
            return _assembly
                .GetTypes()
                .Where(o => baseType.IsAssignableFrom(o))
                .ToArray();
        }
    }

    /// <summary>
    /// Provides extension methods for common reflection tasks.
    /// This is the UWP version.
    /// </summary>
    public static class ReflectorExtensions
    {
        /// <summary>
        /// Determines whether the type is a value type.
        /// </summary>
        /// <param name="t">Type</param>
        /// <returns>Bool</returns>
        public static bool CheckIsValueType(this Type t) => t.GetTypeInfo().IsValueType;

        /// <summary>
        /// Determines whether the type derives from <typeparamref name="TBase"/>.
        /// </summary>
        /// <typeparam name="TBase">Base class</typeparam>
        /// <param name="t">Type</param>
        /// <returns>Bool</returns>
        public static bool IsDerivedFrom<TBase>(this Type t) => typeof(TBase).IsAssignableFrom(t);

        /// <summary>
        /// Returns the first attribute that matches the
        /// specified type.
        /// </summary>
        /// <typeparam name="T">Attribute type</typeparam>
        /// <param name="member">Member</param>
        /// <returns>Attribute of type <typeparamref name="T"/></returns>
        public static T GetCustomAttribute<T>(this MemberInfo member) where T : Attribute
        {
            return member.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
        }

        /// <summary>
        /// Returns the first attribute that matches the
        /// specified type.
        /// </summary>
        /// <typeparam name="T">Attribute type</typeparam>
        /// <param name="type">Member</param>
        /// <returns>Attribute of type <typeparamref name="T"/></returns>
        public static T GetCustomAttribute<T>(this Type type) where T : Attribute =>
            type.GetTypeInfo().GetCustomAttribute<T>();
    }
}