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
    class Program
    {
        static readonly byte[] tagHeader = new byte[] { 0x00, 0x00, 0x00, 0x88, 0x00, 0x00, 0x00, 0x00, 0x00, 0x48, 0x0f, 0xe0, 0xf1, 0x10, 0xff, 0xee, 0xa5, 0x00, 0x00, 0x00 };
        static readonly byte[] tagFooter = new byte[] { 0x01, 0x00, 0x0F, 0xBD, 0x00, 0x00, 0x00, 0x04, 0x5F, 0x00, 0x00, 0x00 };

        static byte[] GetRandomBytes(int count)
        {
            var output = new byte[count];
            var rnd = new Random();

            rnd.NextBytes(output); 

            return output;
        }
        
        static byte[] GenerateBin(params byte[] identificationBlock)
        {
            if (identificationBlock.Length != 8)
            {
                throw new ArgumentException("Invalid identification block length");
            }

            var decrypted = new byte[540];

            tagHeader.CopyTo(decrypted, 0);
            identificationBlock.CopyTo(decrypted, 84);
            GetRandomBytes(32).CopyTo(decrypted, 96);
            tagFooter.CopyTo(decrypted, 520);

            var tag = AmiiboTag.FromNtagData(decrypted);

            return tag.EncryptWithKeys().Take(540).ToArray();
        }

        static AmiiboDatabaseModel DownloadJson(string url = "https://raw.githubusercontent.com/N3evin/AmiiboAPI/master/database/amiibo.json")
        {
            using (var wc = new WebClient())
            {
                var json = wc.DownloadString(url);

                return JsonSerializer.Deserialize<AmiiboDatabaseModel>(json);
            }
        }

        static string CleanFilename(string input, char replacement = '_')
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                input = input.Replace(c, '_');
            }

            return input;
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        static void Main(string[] args)
        {
            if (File.Exists("key_retail.bin"))
            {
                var json = DownloadJson();
                foreach (var amiibo in json.Amiibos)
                {
                    var amiiboId = amiibo.Key;
                    var seriesName = json.AmiiboSeries[$"0x{amiiboId.Substring(14, 2)}"];
                    var destPath = Path.Combine("dumps", CleanFilename(seriesName));
                    var destFile = Path.Combine(destPath, CleanFilename($"{json.Amiibos[amiiboId].Name} ({amiiboId}).bin"));
                    var identificationBlock = StringToByteArray(amiiboId.Substring(2));

                    Console.WriteLine(destFile);

                    Directory.CreateDirectory(destPath);
                    File.WriteAllBytes(destFile, GenerateBin(identificationBlock));
                }
            }
            else
            {
                Console.WriteLine("key_retail.bin not found");
            }

            Console.WriteLine("\nPress enter to exit");
            Console.ReadLine();
        }
    }

    class AmiiboDatabaseModel
    {
        public class Amiibo
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("release")]
            public Dictionary<string, string> Release { get; set; }
        }

        [JsonPropertyName("amiibo_series")]
        public Dictionary<string, string> AmiiboSeries { get; set; }

        [JsonPropertyName("amiibos")]
        public Dictionary<string, Amiibo> Amiibos { get; set; }

        [JsonPropertyName("characters")]
        public Dictionary<string, string> Characters { get; set; }

        [JsonPropertyName("game_series")]
        public Dictionary<string, string> GameSeries { get; set; }

        [JsonPropertyName("types")]
        public Dictionary<string, string> Types { get; set; }
    }
}
