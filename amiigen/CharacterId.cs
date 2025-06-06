using System;
using System.Globalization;
using System.Linq;

namespace AmiiGen
{
    /// <summary>
    /// Represents a unique identifier for a character, consisting of 8 bytes, where the last byte must be <see
    /// langword="0x02"/>.
    /// </summary>
    /// <remarks>A <see cref="CharacterId"/> can be created from either a hexadecimal string or a byte array.
    /// The identifier is immutable, and its value is validated during construction: <list type="bullet">
    /// <item><description>When created from a hexadecimal string, the string must be exactly 16 hex characters long
    /// (excluding optional "0x" prefix) and contain only valid hexadecimal characters.</description></item>
    /// <item><description>When created from a byte array, the array must be exactly 8 bytes long, and the last byte
    /// must be <see langword="0x02"/>.</description></item> </list> The <see cref="Bytes"/> property provides a copy of
    /// the underlying byte array, ensuring immutability.</remarks>
    public class CharacterId
    {
        private readonly byte[] _bytes;

        /// <summary>
        /// Gets a copy of the byte array representing the data.
        /// </summary>
        public byte[] Bytes => (byte[])_bytes.Clone();

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacterId"/> class using a hexadecimal string representation.
        /// </summary>
        /// <remarks>This constructor validates the input string to ensure it represents a valid character
        /// ID.  The input string is cleaned by removing any whitespace and an optional "0x" prefix before validation. 
        /// The last byte of the parsed value must be <c>0x02</c>, which is a required constraint for a valid <see
        /// cref="CharacterId"/>.</remarks>
        /// <param name="hex">A string containing the hexadecimal representation of the character ID.  The string must be exactly 16
        /// hexadecimal characters long (8 bytes) and may optionally start with "0x".</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="hex"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="hex"/> is not exactly 16 hexadecimal characters long (excluding whitespace and an
        /// optional "0x" prefix), contains invalid hexadecimal characters, or if the last byte of the parsed value is
        /// not <c>0x02</c>.</exception>
        public CharacterId(string hex)
        {
            if (hex == null)
                throw new ArgumentNullException(nameof(hex));

            // Clean the input string by removing whitespace.
            string cleaned = new string(hex.Where(c => !char.IsWhiteSpace(c)).ToArray());

            // Check if the string starts with "0x" and remove it if present.
            if (cleaned.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                cleaned = cleaned.Substring(2);

            // Validate the string length.
            if (cleaned.Length != 16)
                throw new ArgumentException("CharacterId must be exactly 8 bytes (16 hex characters) long.", nameof(hex));

            // Validate that the string contains only hex characters. [0-9, A-F, a-f]
            if (!IsHex(cleaned))
                throw new ArgumentException("CharacterId contains invalid hex characters.", nameof(hex));

            _bytes = Enumerable.Range(0, 8)
                .Select(i => byte.Parse(cleaned.Substring(i * 2, 2), NumberStyles.HexNumber))
                .ToArray();

            if (_bytes[7] != 0x02)
                throw new ArgumentException("The last byte of CharacterId must be 0x02.", nameof(hex));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacterId"/> class using the specified byte array.
        /// </summary>
        /// <remarks>The provided byte array is cloned to ensure immutability of the <see
        /// cref="CharacterId"/> instance.</remarks>
        /// <param name="bytes">A byte array representing the character identifier. The array must be exactly 8 bytes long,  and the last
        /// byte must be <see langword="0x02"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="bytes"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="bytes"/> is not exactly 8 bytes long or if the last byte is not <see
        /// langword="0x02"/>.</exception>
        public CharacterId(byte[] bytes)
        {
            // Validate the input byte array.
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            // Check if the byte array is exactly 8 bytes long.
            if (bytes.Length != 8)
                throw new ArgumentException("CharacterId must be exactly 8 bytes long.", nameof(bytes));

            // Check if the last byte is 0x02.
            if (bytes[7] != 0x02)
                throw new ArgumentException("The last byte of CharacterId must be 0x02.", nameof(bytes));

            // Clone the byte array to ensure immutability.
            _bytes = (byte[])bytes.Clone();
        }

        /// <summary>
        /// Converts a <see cref="CharacterId"/> instance to a byte array.
        /// </summary>
        /// <param name="id">The <see cref="CharacterId"/> instance to convert.</param>
        public static implicit operator byte[](CharacterId id) => id.Bytes;

        /// <summary>
        /// Converts the specified <see cref="CharacterId"/> instance to its string representation.  See <see cref="ToString"/> for the format.
        /// </summary>
        /// <param name="id">The <see cref="CharacterId"/> instance to convert.</param>
        public static implicit operator string(CharacterId id) => id.ToString();

        /// <summary>
        /// Returns a string representation of the object in hexadecimal format.
        /// </summary>
        /// <remarks>The returned string is prefixed with "0x" and represents the byte array as a sequence
        /// of  two-character hexadecimal values. Each byte is formatted using uppercase letters.</remarks>
        /// <returns>A string in the format "0xHH..." where each "HH" represents a byte in the array as a two-character
        /// hexadecimal value.</returns>
        public override string ToString()
        {
            // Convert the byte array to a hexadecimal string.
            return "0x" + string.Concat(_bytes.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));
        }

        /// <summary>
        /// Determines whether the specified string consists solely of valid hexadecimal characters.
        /// </summary>
        /// <param name="s">The string to validate. Must not be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the string contains only hexadecimal characters (0-9, A-F, a-f);  otherwise, <see
        /// langword="false"/>.</returns>
        private static bool IsHex(string s)
        {
            const string allowedChars = "0123456789ABCDEFabcdef";
            return s.All(c => allowedChars.Contains(c));
        }
    }
}
