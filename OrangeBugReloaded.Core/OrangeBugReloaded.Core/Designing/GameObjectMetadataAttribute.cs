using System;

namespace OrangeBugReloaded.Core.Designing
{
    /// <summary>
    /// Game Object metadata.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class GameObjectMetadataAttribute : Attribute
    {
        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description.
        /// </summary>
        public string Description { get; set; }
    }
}