namespace FluffRest.Settings
{
    public class FluffClientSettings
    {
        public FluffDuplicateParameterKeyHandling DuplicateParameterKeyHandling { get; private set; }
        public FluffDuplicateHeaderHandling DuplicateHeaderHandling { get; private set; }
        public FluffDuplicateWithDefaultHeaderHandling DuplicateDefaultHeaderHandling { get; private set; }
        public FluffAutoCancelHandling AutoCancelHandling { get; private set; }
        public bool EnsureSuccessCode { get; private set; }

        /// <summary>
        /// Configure settings of the client.
        /// </summary>
        /// <param name="duplicateHandling">How to handle duplicate in query parameters.</param>
        /// <param name="ensureSuccessCode">Ensure response is successfull.</param>
        /// <param name="duplicateDefaultHeaderHandling">How to handle duplicate request headers with default ones.</param>
        /// <param name="duplicateHeaderHandling">How to handle trying to add duplicate headers.</param>
        public FluffClientSettings(
            FluffDuplicateParameterKeyHandling duplicateHandling = default, 
            bool ensureSuccessCode = true, 
            FluffDuplicateWithDefaultHeaderHandling duplicateDefaultHeaderHandling = default, 
            FluffDuplicateHeaderHandling duplicateHeaderHandling = default,
            FluffAutoCancelHandling autoCancelHandling = default)
        {
            DuplicateParameterKeyHandling = duplicateHandling;
            EnsureSuccessCode = ensureSuccessCode;
            DuplicateDefaultHeaderHandling = duplicateDefaultHeaderHandling;
            DuplicateHeaderHandling = duplicateHeaderHandling;
            AutoCancelHandling = autoCancelHandling;
        }
    }
}
