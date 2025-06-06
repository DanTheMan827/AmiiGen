using CommandLine;

namespace AmiiGen
{
    public class CommandLineOptions
    {
        /// <summary>
        /// The character ID used to generate a single dump.
        /// </summary>
        [Option('s', "single", Required = false, HelpText = "Generate a single dump with the specified character id.", SetName = "mode")]
        public CharacterId CharacterId { get; set; } = null;

        /// <summary>
        /// Whether dumps should be generated for all characters.
        /// </summary>
        [Option('a', "all", Required = false, HelpText = "Generate dumps for all characters.", SetName = "mode")]
        public bool AllCharacters { get; set; }

        /// <summary>
        /// The URL of the Amiibo database.
        /// </summary>
        [Option('u', "database-url", Required = false, HelpText = "Specify a custom URL for the Amiibo database.", Default = "https://raw.githubusercontent.com/N3evin/AmiiboAPI/master/database/amiibo.json")]
        public string DatabaseUrl { get; set; }

        /// <summary>
        /// The file containing the cryptographic key.
        /// </summary>
        /// <remarks>The file specified by this property is expected to contain the key used for
        /// encryption or decryption operations.  Ensure the file exists and is accessible before setting this
        /// property.</remarks>
        [Option('k', "key-file", Required = false, HelpText = "Specify the key file for encryption/decryption operations.", Default = "key_retail.bin")]
        public string KeyFile { get; set; }

        /// <summary>
        /// The output path for the operation.
        /// </summary>
        [Option('o', "output", Required = false, HelpText = "Specify the output path.  If --all, this is a directory, otherwise it's a file name.")]
        public string OutputPath { get; set; } = null;

        [Option('w', "wait", Required = false, HelpText = "Require pressing the enter key to exit after running.")]
        public bool WaitForExit { get; set; } = false;
    }
}
