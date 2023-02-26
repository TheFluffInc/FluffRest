namespace FluffRest.Settings
{
    public class FluffClientSettings
    {
        public FluffDuplicateParameterKeyHandling DuplicateParameterKeyHandling { get; private set; }
        public bool EnsureSuccessCode { get; private set; }

        public FluffClientSettings(FluffDuplicateParameterKeyHandling duplicateHandling = default, bool ensureSuccessCode = true)
        {
            DuplicateParameterKeyHandling = duplicateHandling;
            EnsureSuccessCode = ensureSuccessCode;
        }
    }
}
