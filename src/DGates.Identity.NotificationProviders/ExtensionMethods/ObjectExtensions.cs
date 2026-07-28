using Newtonsoft.Json;

namespace DGates.Identity.NotificationProviders.ExtensionMethods
{
    /// <summary>Extension methods for serializing arbitrary objects, primarily for structured debug/trace logging.</summary>
    public static class ObjectExtensions
    {
        /// <summary>Serializes <paramref name="value"/> to indented JSON, preserving object references. Returns an empty string on serialization failure rather than throwing.</summary>
        public static string ToJson(this object value)
        {
            return ToJson(value, PreserveReferencesHandling.Objects);
        }

        /// <summary>Serializes <paramref name="value"/> to indented JSON with the given reference-handling behavior. Returns an empty string on serialization failure rather than throwing.</summary>
        public static string ToJson(this object value, PreserveReferencesHandling referencesHandling)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    PreserveReferencesHandling = referencesHandling,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                return JsonConvert.SerializeObject(value, Formatting.Indented, settings);
            }
            catch (Exception)
            {
                return "";
            }
        }

    }
}