using LibAmiibo.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmiiGen
{
    /// <summary>
    /// Main program to generate Amiibo tags.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Downloads JSON data from the specified URL.
        /// </summary>
        /// <param name="url">The URL to download JSON from.</param>
        /// <returns>An instance of AmiiboDatabaseModel.</returns>
        static AmiiboDatabaseModel DownloadJson(string url = "https://raw.githubusercontent.com/N3evin/AmiiboAPI/master/database/amiibo.json")
        {
            using (var wc = new WebClient())
            {
                var json = wc.DownloadString(url);

                return JsonSerializer.Deserialize<AmiiboDatabaseModel>(json);
            }
        }

        /// <summary>
        /// Cleans a filename by replacing invalid characters.
        /// </summary>
        /// <param name="input">The input filename.</param>
        /// <param name="replacement">The character to replace invalid characters with.</param>
        /// <returns>The cleaned filename.</returns>
        static string CleanFilename(string input, char replacement = '_')
        {
            foreach (char c in Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()))
            {
                input = input.Replace(c, '_');
            }

            return input;
        }

        /// <summary>
        /// Main method.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            if (File.Exists("key_retail.bin"))
            {
                // Set the Amiibo keys file
                LibAmiibo.Settings.AmiiboKeys = "key_retail.bin";

                // Download the Amiibo JSON data
                var json = DownloadJson();

                // Process each Amiibo entry
                foreach (var amiibo in json.Amiibos)
                {
                    var amiiboId = amiibo.Key;
                    var seriesName = json.AmiiboSeries[$"0x{amiiboId.Substring(14, 2)}"];
                    var destPath = Path.Combine("dumps", CleanFilename(seriesName));
                    var destFile = Path.Combine(destPath, CleanFilename($"{json.Amiibos[amiiboId].Name} ({amiiboId}).bin"));

                    // Log to the console
                    Console.WriteLine(destFile);

                    // Create directory if it doesn't exist
                    Directory.CreateDirectory(destPath);

                    // Generate and write Amiibo tag
                    var tag = AmiiboTag.FromIdentificationBlock(amiiboId);
                    var encryptedTag = tag.EncryptWithKeys();
                    File.WriteAllBytes(destFile, encryptedTag);
                }
            }
            else
            {
                // Inform user that keys file is missing
                Console.WriteLine("key_retail.bin not found");
            }

            // Wait for user input before exiting
            Console.WriteLine("\nPress enter to exit");
            Console.ReadLine();
        }
    }

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
