namespace FluffRest.Settings
{
    /// <summary>
    /// Tells client how to handle conflicts in header names.
    /// </summary>
    public enum FluffDuplicateHeaderHandling : short
    {
        /// <summary>
        /// If two header are added with the same name, and exception will be thrown.
        /// </summary>
        Throw = default,
        /// <summary>
        /// New value will not replace old value.
        /// </summary>
        Ignore = 1,
        /// <summary>
        /// New value will replace existing value.
        /// </summary>
        Replace = 2
    }

    /// <summary>
    /// Tells client how to handle duplicates header betwwen request specific ones and default client ones.
    /// </summary>
    public enum FluffDuplicateWithDefaultHeaderHandling : short
    {
        /// <summary>
        /// Will throw in case of a conflict.
        /// </summary>
        Throw = default,
        /// <summary>
        /// Will ignore request header.
        /// </summary>
        Ignore = 1,
        /// <summary>
        /// Will override default header.
        /// </summary>
        Replace = 2
    }
}
