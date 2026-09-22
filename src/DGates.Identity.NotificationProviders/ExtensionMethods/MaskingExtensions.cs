namespace DGates.Identity.NotificationProviders.ExtensionMethods
{
    /// <summary>Extension methods for masking recipient identifiers (email addresses, phone numbers) before they're logged.</summary>
    public static class MaskingExtensions
    {
        /// <summary>
        /// Masks all but the first character of the local part, e.g. "jane.doe@example.com" ->
        /// "j***@example.com". Returns the input unchanged if it doesn't look like an email address.
        /// </summary>
        public static string MaskEmail(this string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return email;
            }

            var atIndex = email.IndexOf('@');
            if (atIndex <= 0)
            {
                return email;
            }

            return $"{email[0]}***{email[atIndex..]}";
        }

        /// <summary>
        /// Masks the middle of a phone number, keeping the first five and last four characters,
        /// e.g. "+15555550100" -> "+1555***0100". Returns the input unchanged if it's too short
        /// to mask meaningfully.
        /// </summary>
        public static string MaskPhone(this string number)
        {
            if (string.IsNullOrEmpty(number) || number.Length <= 9)
            {
                return number;
            }

            return $"{number[..5]}***{number[^4..]}";
        }
    }
}
