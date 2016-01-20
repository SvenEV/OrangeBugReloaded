using System;

namespace OrangeBugReloaded.Core.Entities
{
    /// <summary>
    /// The enum values describe different subsets of <see cref="Entity"/> types.
    /// </summary>
    public enum EntityFilterMode
    {
        /// <summary>
        /// Includes all <see cref="Entity"/> types.
        /// </summary>
        Entities = 0,

        /// <summary>
        /// Includes all <see cref="Entity"/> types
        /// except for <see cref="PlayerEntity"/>.
        /// </summary>
        EntitiesExceptPlayer = 1,

        /// <summary>
        /// Includes only <see cref="PlayerEntity"/>.
        /// </summary>
        Player = 2
    }

    /// <summary>
    /// Provides extension methods for <see cref="EntityFilterMode"/>.
    /// </summary>
    public static class EntityFilterModeExtensions
    {
        /// <summary>
        /// Determines whether the <see cref="EntityFilterMode"/>
        /// includes the specified <see cref="Entity"/> type.
        /// </summary>
        /// <param name="filterMode">Filter mode</param>
        /// <typeparam name="T"><see cref="Entity"/> type</typeparam>
        /// <returns>True if included, false otherwise</returns>
        public static bool Includes<T>(this EntityFilterMode filterMode) where T : Entity
            => filterMode.Includes(typeof(T));

        /// <summary>
        /// Determines whether the <see cref="EntityFilterMode"/>
        /// includes the specified <see cref="Entity"/> type.
        /// </summary>
        /// <param name="filterMode">Filter mode</param>
        /// <param name="entityType"><see cref="Entity"/> type</param>
        /// <returns>True if included, false otherwise</returns>
        public static bool Includes(this EntityFilterMode filterMode, Type entityType)
        {
            switch (filterMode)
            {
                case EntityFilterMode.Entities:
                    return true;

                case EntityFilterMode.EntitiesExceptPlayer:
                    return entityType != typeof(PlayerEntity);

                case EntityFilterMode.Player:
                    return entityType == typeof(PlayerEntity);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
