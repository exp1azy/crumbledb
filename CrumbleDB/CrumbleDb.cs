namespace CrumbleDB
{
    /// <summary>
    /// Provides a static entry point for opening and initializing a Crumble database instance.
    /// </summary>
    public sealed class CrumbleDb
    {
        /// <summary>
        /// Opens a Crumble database located in the specified folder.
        /// If the folder does not exist, it will be created.
        /// </summary>
        /// <param name="folder">The file system path to the folder where the database is or will be stored.</param>
        /// <returns>
        /// A <see cref="CrumbleDbCore"/> instance that provides access to collections stored in the specified folder.
        /// </returns>
        public static CrumbleDbCore Open(string folder)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return new CrumbleDbCore(folder);
        }
    }

}
