// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2026 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;

namespace WinEcoSensor.TrayApp
{
    /// <summary>
    /// QR Code generator - Version 1-L (21x21, Low error correction)
    /// Implements ISO/IEC 18004 QR Code specification for alphanumeric mode
    /// </summary>
    public static class QrCodeGenerator
    {
        private const int VERSION = 1;
        private const int SIZE = 21; // Version 1 is 21x21
        private const int DATA_CAPACITY = 19; // Data codewords for Version 1-L
        private const int EC_CODEWORDS = 7;   // Error correction codewords

        // Alphanumeric encoding table
        private const string ALPHANUMERIC_TABLE = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ $%*+-./:";

        // Reed-Solomon generator polynomial for 7 EC codewords
        private static readonly int[] RS_GENERATOR = { 87, 229, 146, 149, 238, 102, 21 };

        // GF(256) tables
        private static readonly int[] GF_EXP = new int[512];
        private static readonly int[] GF_LOG = new int[256];

        static QrCodeGenerator()
        {
            // Initialize Galois Field tables
            int x = 1;
            for (int i = 0; i < 255; i++)
            {
                GF_EXP[i] = x;
                GF_LOG[x] = i;
                x <<= 1;
                if (x >= 256) x ^= 0x11D;
            }
            for (int i = 255; i < 512; i++)
            {
                GF_EXP[i] = GF_EXP[i - 255];
            }
        }

        /// <summary>
        /// Generate a QR code bitmap from computer name and user name
        /// </summary>
        public static Bitmap Generate(string computerName, string userName, int scale = 8)
        {
            string deviceId = CreateDeviceId(computerName, userName);
            return GenerateQrCode(deviceId, scale);
        }

        /// <summary>
        /// Generate a QR code bitmap from text data
        /// </summary>
        public static Bitmap GenerateQrCode(string data, int scale = 8)
        {
            // Normalize to uppercase (alphanumeric mode)
            data = data.ToUpperInvariant();

            // Version 1-L can hold max 25 alphanumeric characters
            // But we limit to 14 for our device ID
            if (data.Length > 14)
                data = data.Substring(0, 14);

            // Create the QR matrix
            bool[,] matrix = new bool[SIZE, SIZE];
            bool[,] isFunction = new bool[SIZE, SIZE];

            // Step 1: Place finder patterns
            PlaceFinderPatterns(matrix, isFunction);

            // Step 2: Place timing patterns
            PlaceTimingPatterns(matrix, isFunction);

            // Step 3: Place dark module
            matrix[8, SIZE - 8] = true;
            isFunction[8, SIZE - 8] = true;

            // Step 4: Reserve format information area
            ReserveFormatArea(isFunction);

            // Step 5: Encode data
            byte[] codewords = EncodeAlphanumeric(data);

            // Step 6: Generate error correction
            byte[] ecCodewords = GenerateErrorCorrection(codewords);

            // Step 7: Combine data and EC
            byte[] allCodewords = new byte[DATA_CAPACITY + EC_CODEWORDS];
            Array.Copy(codewords, 0, allCodewords, 0, DATA_CAPACITY);
            Array.Copy(ecCodewords, 0, allCodewords, DATA_CAPACITY, EC_CODEWORDS);

            // Step 8: Place data bits
            PlaceDataBits(matrix, isFunction, allCodewords);

            // Step 9: Apply mask pattern 0 ((row + col) % 2 == 0)
            ApplyMask(matrix, isFunction);

            // Step 10: Place format information (L, mask 0)
            PlaceFormatInformation(matrix);

            // Render to bitmap
            return RenderBitmap(matrix, scale);
        }

        private static void PlaceFinderPatterns(bool[,] matrix, bool[,] isFunction)
        {
            // Three finder patterns at corners
            PlaceFinderPattern(matrix, isFunction, 0, 0);
            PlaceFinderPattern(matrix, isFunction, SIZE - 7, 0);
            PlaceFinderPattern(matrix, isFunction, 0, SIZE - 7);
        }

        private static void PlaceFinderPattern(bool[,] matrix, bool[,] isFunction, int col, int row)
        {
            for (int r = -1; r <= 7; r++)
            {
                for (int c = -1; c <= 7; c++)
                {
                    int rr = row + r;
                    int cc = col + c;

                    if (rr >= 0 && rr < SIZE && cc >= 0 && cc < SIZE)
                    {
                        // The finder pattern is:
                        // 1111111
                        // 1000001
                        // 1011101
                        // 1011101
                        // 1011101
                        // 1000001
                        // 1111111
                        // Plus white separator (the -1 positions)
                        bool black = false;

                        if (r >= 0 && r <= 6 && c >= 0 && c <= 6)
                        {
                            if (r == 0 || r == 6 || c == 0 || c == 6)
                                black = true;
                            else if (r >= 2 && r <= 4 && c >= 2 && c <= 4)
                                black = true;
                        }

                        matrix[cc, rr] = black;
                        isFunction[cc, rr] = true;
                    }
                }
            }
        }

        private static void PlaceTimingPatterns(bool[,] matrix, bool[,] isFunction)
        {
            for (int i = 8; i < SIZE - 8; i++)
            {
                bool black = (i % 2 == 0);
                matrix[i, 6] = black;
                matrix[6, i] = black;
                isFunction[i, 6] = true;
                isFunction[6, i] = true;
            }
        }

        private static void ReserveFormatArea(bool[,] isFunction)
        {
            // Around top-left finder
            for (int i = 0; i <= 8; i++)
            {
                isFunction[i, 8] = true;
                isFunction[8, i] = true;
            }

            // Top-right
            for (int i = SIZE - 8; i < SIZE; i++)
            {
                isFunction[i, 8] = true;
            }

            // Bottom-left
            for (int i = SIZE - 8; i < SIZE; i++)
            {
                isFunction[8, i] = true;
            }
        }

        private static byte[] EncodeAlphanumeric(string data)
        {
            var bits = new System.Collections.Generic.List<bool>();

            // Mode indicator: 0010 (Alphanumeric)
            AddBits(bits, 0b0010, 4);

            // Character count (9 bits for Version 1-9)
            AddBits(bits, data.Length, 9);

            // Encode character pairs
            for (int i = 0; i < data.Length; i += 2)
            {
                if (i + 1 < data.Length)
                {
                    // Two characters -> 11 bits
                    int val = ALPHANUMERIC_TABLE.IndexOf(data[i]) * 45 +
                              ALPHANUMERIC_TABLE.IndexOf(data[i + 1]);
                    AddBits(bits, val, 11);
                }
                else
                {
                    // Single character -> 6 bits
                    int val = ALPHANUMERIC_TABLE.IndexOf(data[i]);
                    AddBits(bits, val, 6);
                }
            }

            // Add terminator (up to 4 zeros)
            int remainingCapacity = DATA_CAPACITY * 8 - bits.Count;
            int terminatorLength = Math.Min(4, remainingCapacity);
            AddBits(bits, 0, terminatorLength);

            // Pad to byte boundary
            while (bits.Count % 8 != 0)
                bits.Add(false);

            // Pad with alternating 0xEC and 0x11
            byte[] padBytes = { 0xEC, 0x11 };
            int padIndex = 0;
            while (bits.Count < DATA_CAPACITY * 8)
            {
                AddBits(bits, padBytes[padIndex], 8);
                padIndex = 1 - padIndex;
            }

            // Convert to bytes
            byte[] result = new byte[DATA_CAPACITY];
            for (int i = 0; i < DATA_CAPACITY; i++)
            {
                int val = 0;
                for (int j = 0; j < 8; j++)
                {
                    val = (val << 1) | (bits[i * 8 + j] ? 1 : 0);
                }
                result[i] = (byte)val;
            }

            return result;
        }

        private static void AddBits(System.Collections.Generic.List<bool> bits, int value, int length)
        {
            for (int i = length - 1; i >= 0; i--)
            {
                bits.Add(((value >> i) & 1) == 1);
            }
        }

        private static byte[] GenerateErrorCorrection(byte[] data)
        {
            byte[] coefficients = new byte[EC_CODEWORDS];

            for (int i = 0; i < data.Length; i++)
            {
                int factor = data[i] ^ coefficients[0];

                // Shift coefficients
                for (int j = 0; j < EC_CODEWORDS - 1; j++)
                {
                    coefficients[j] = coefficients[j + 1];
                }
                coefficients[EC_CODEWORDS - 1] = 0;

                if (factor != 0)
                {
                    int logFactor = GF_LOG[factor];
                    for (int j = 0; j < EC_CODEWORDS; j++)
                    {
                        coefficients[j] ^= (byte)GF_EXP[logFactor + RS_GENERATOR[j]];
                    }
                }
            }

            return coefficients;
        }

        private static void PlaceDataBits(bool[,] matrix, bool[,] isFunction, byte[] codewords)
        {
            int bitIndex = 0;
            int totalBits = codewords.Length * 8;

            // Traverse in a zig-zag pattern from bottom-right
            for (int right = SIZE - 1; right >= 1; right -= 2)
            {
                // Skip the vertical timing pattern column
                if (right == 6) right = 5;

                for (int vert = 0; vert < SIZE; vert++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        int col = right - j;
                        // Determine direction: upward for even column pairs, downward for odd
                        bool upward = ((SIZE - 1 - right) / 2) % 2 == 0;
                        int row = upward ? SIZE - 1 - vert : vert;

                        if (!isFunction[col, row] && bitIndex < totalBits)
                        {
                            int byteIndex = bitIndex / 8;
                            int bitPosition = 7 - (bitIndex % 8);
                            matrix[col, row] = ((codewords[byteIndex] >> bitPosition) & 1) == 1;
                            bitIndex++;
                        }
                    }
                }
            }
        }

        private static void ApplyMask(bool[,] matrix, bool[,] isFunction)
        {
            // Mask pattern 0: (row + col) % 2 == 0
            for (int row = 0; row < SIZE; row++)
            {
                for (int col = 0; col < SIZE; col++)
                {
                    if (!isFunction[col, row] && (row + col) % 2 == 0)
                    {
                        matrix[col, row] = !matrix[col, row];
                    }
                }
            }
        }

        private static void PlaceFormatInformation(bool[,] matrix)
        {
            // Format info for Level L (01) and Mask 0 (000): 01000
            // After BCH encoding and XOR mask: 111011111000100
            int formatBits = 0x77C4;

            // Place around top-left finder
            // Horizontal: row 8, cols 0-5, 7, 8
            int[] hCols = { 0, 1, 2, 3, 4, 5, 7, 8 };
            for (int i = 0; i < 8; i++)
            {
                matrix[hCols[i], 8] = ((formatBits >> (14 - i)) & 1) == 1;
            }

            // Vertical: col 8, rows 0-5, 7, 8
            int[] vRows = { 0, 1, 2, 3, 4, 5, 7, 8 };
            for (int i = 0; i < 8; i++)
            {
                matrix[8, vRows[7 - i]] = ((formatBits >> (14 - i)) & 1) == 1;
            }

            // Place along top-right (row 8, cols SIZE-8 to SIZE-1)
            for (int i = 0; i < 8; i++)
            {
                matrix[SIZE - 8 + i, 8] = ((formatBits >> i) & 1) == 1;
            }

            // Place along bottom-left (col 8, rows SIZE-7 to SIZE-1)
            for (int i = 0; i < 7; i++)
            {
                matrix[8, SIZE - 7 + i] = ((formatBits >> (14 - i)) & 1) == 1;
            }
        }

        private static Bitmap RenderBitmap(bool[,] matrix, int scale)
        {
            int quietZone = 4; // White border around QR code
            int imageSize = (SIZE + quietZone * 2) * scale;
            var bitmap = new Bitmap(imageSize, imageSize);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);

                using (var brush = new SolidBrush(Color.Black))
                {
                    for (int row = 0; row < SIZE; row++)
                    {
                        for (int col = 0; col < SIZE; col++)
                        {
                            if (matrix[col, row])
                            {
                                g.FillRectangle(brush,
                                    (quietZone + col) * scale,
                                    (quietZone + row) * scale,
                                    scale, scale);
                            }
                        }
                    }
                }
            }

            return bitmap;
        }

        /// <summary>
        /// Create a unique device ID from computer name and user (14 chars max for QR)
        /// </summary>
        public static string CreateDeviceId(string computerName, string userName)
        {
            string input = $"{computerName}|{userName}";

            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder();
                for (int i = 0; i < 7; i++)
                    sb.Append(hash[i].ToString("X2"));
                return sb.ToString(); // 14 hex characters (0-9, A-F)
            }
        }

        /// <summary>
        /// Create encrypted device info string
        /// </summary>
        public static string EncryptDeviceInfo(string computerName, string userName)
        {
            string plainText = $"{computerName}|{userName}|{DateTime.UtcNow:yyyyMMddHHmmss}";

            using (var aes = Aes.Create())
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes("WinEcoSensor2026!");
                byte[] key = new byte[32];
                byte[] iv = new byte[16];
                Array.Copy(keyBytes, 0, key, 0, Math.Min(keyBytes.Length, 32));
                Array.Copy(keyBytes, 0, iv, 0, Math.Min(keyBytes.Length, 16));

                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }

        /// <summary>
        /// Decrypt device info
        /// </summary>
        public static string DecryptDeviceInfo(string encryptedData)
        {
            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedData);

                using (var aes = Aes.Create())
                {
                    byte[] keyBytes = Encoding.UTF8.GetBytes("WinEcoSensor2026!");
                    byte[] key = new byte[32];
                    byte[] iv = new byte[16];
                    Array.Copy(keyBytes, 0, key, 0, Math.Min(keyBytes.Length, 32));
                    Array.Copy(keyBytes, 0, iv, 0, Math.Min(keyBytes.Length, 16));

                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var decryptor = aes.CreateDecryptor())
                    {
                        byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                        return Encoding.UTF8.GetString(decryptedBytes);
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
