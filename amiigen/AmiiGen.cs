using CommandLine;
using LibAmiibo.Data;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;

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

        static void Exit(int exitCode, bool wait = true)
        {
            if (wait)
            {
                // Wait for user input before exiting
                Console.WriteLine("\nPress enter to exit");
                Console.ReadLine();
            }

            // Exit the application with the specified exit code
            Environment.Exit(exitCode);
        }

        /// <summary>
        /// Main method.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            var parsed = Parser.Default.ParseArguments<CommandLineOptions>(args);
            parsed.WithParsed(options =>
                {
                    if (string.IsNullOrWhiteSpace(options.KeyFile) || !File.Exists(options.KeyFile))
                    {
                        Console.WriteLine($"Key file '{options.KeyFile}' does not exist. Please provide a valid key file.");
                        Exit(1, options.WaitForExit);
                    }

                    // Set the Amiibo keys file
                    LibAmiibo.Settings.AmiiboKeys = options.KeyFile;

                    if ((options.CharacterId != null && options.AllCharacters))
                    {
                        Console.WriteLine("Please specify either a character ID with --single or use --all to generate dumps for all characters, but not both.");
                        Exit(1, options.WaitForExit);
                    }

                    if (options.CharacterId == null && !options.AllCharacters)
                    {
                        Console.WriteLine("No mode specified, defaulting to --all.");
                        options.AllCharacters = true;
                    }

                    if (options.AllCharacters)
                    {
                        Console.WriteLine("Generating dumps for all characters...");

                        // Download the Amiibo JSON data
                        var json = DownloadJson(options.DatabaseUrl.ToString());

                        // Process each Amiibo entry
                        foreach (var amiibo in json.Amiibos)
                        {
                            var amiiboId = amiibo.Key;
                            var seriesName = json.AmiiboSeries[$"0x{amiiboId.Substring(14, 2)}"];
                            var destPath = Path.Combine(string.IsNullOrWhiteSpace(options.OutputPath) ? "dumps" : options.OutputPath, CleanFilename(seriesName));
                            var destFile = Path.Combine(destPath, CleanFilename($"{json.Amiibos[amiiboId].Name} ({amiiboId}).bin"));

                            // Create directory if it doesn't exist
                            Directory.CreateDirectory(destPath);

                            // Generate and write Amiibo tag
                            var tag = AmiiboTag.FromIdentificationBlock(amiiboId);
                            var encryptedTag = tag.EncryptWithKeys();

                            // Write the encrypted tag
                            Console.WriteLine($"Writing: {Path.GetFullPath(destFile)}");
                            File.WriteAllBytes(destFile, encryptedTag);
                        }
                    }

                    if (options.CharacterId != null)
                    {
                        Console.WriteLine($"Generating dump for character ID: {options.CharacterId}...");

                        // Generate and write Amiibo tag
                        var tag = AmiiboTag.FromIdentificationBlock(options.CharacterId.ToString());
                        var encryptedTag = tag.EncryptWithKeys();

                        // Determine the destination path based on the output option
                        var destPath = new FileInfo(string.IsNullOrWhiteSpace(options.OutputPath) ? $"{options.CharacterId.ToString()}.bin" : options.OutputPath);

                        // Write the encrypted tag to the specified output path
                        Console.WriteLine($"Writing: {destPath.FullName}");
                        File.WriteAllBytes(destPath.FullName, encryptedTag);
                    }

                    Exit(0, options.WaitForExit);
                });
        }
    }
}
