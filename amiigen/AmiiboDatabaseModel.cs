using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AmiiGen
{
    /// <summary>
    /// Represents the model for the Amiibo database.
    /// </summary>
    class AmiiboDatabaseModel
    {
        /// <summary>
        /// Represents an Amiibo entry.
        /// </summary>
        public class Amiibo
        {
            /// <summary>
            /// Gets or sets the name of the Amiibo.
            /// </summary>
            [JsonPropertyName("name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the release information of the Amiibo.
            /// </summary>
            [JsonPropertyName("release")]
            public Dictionary<string, string> Release { get; set; }
        }

        /// <summary>
        /// Gets or sets the dictionary of Amiibo series.
        /// </summary>
        [JsonPropertyName("amiibo_series")]
        public Dictionary<string, string> AmiiboSeries { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of Amiibos.
        /// </summary>
        [JsonPropertyName("amiibos")]
        public Dictionary<string, Amiibo> Amiibos { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of characters.
        /// </summary>
        [JsonPropertyName("characters")]
        public Dictionary<string, string> Characters { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of game series.
        /// </summary>
        [JsonPropertyName("game_series")]
        public Dictionary<string, string> GameSeries { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of types.
        /// </summary>
        [JsonPropertyName("types")]
        public Dictionary<string, string> Types { get; set; }
    }
}
