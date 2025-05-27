namespace CrumbleDB
{
    /// <summary>
    /// Represents the base entity type for Crumble database records. 
    /// Provides identity and value-based equality comparison using a unique identifier.
    /// </summary>
    public abstract class CrumbleEntity : IEquatable<CrumbleEntity>
    {
        /// <summary>
        /// Gets the unique identifier for the entity.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Determines whether the current entity is equal to another <see cref="CrumbleEntity"/> instance.
        /// </summary>
        /// <param name="other">The entity to compare with the current entity.</param>
        /// <returns><c>true</c> if the entities have the same <see cref="Id"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(CrumbleEntity? other)
        {
            if (other is null) return false;
            return Id == other.Id;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current entity.
        /// </summary>
        /// <param name="obj">The object to compare with the current entity.</param>
        /// <returns><c>true</c> if the object is a <see cref="CrumbleEntity"/> with the same <see cref="Id"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as CrumbleEntity);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current entity, based on its <see cref="Id"/>.</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

}
