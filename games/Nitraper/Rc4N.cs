// Copyright (c) 2020 Benito Palacios Sánchez (pleonex)

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
namespace Nitraper
{
    using System;
    using Yarhl.IO;

    /// <summary>
    /// Variant of ARCFOUR found in anti-piracy code of Nintendo DS games.
    /// It looks like RC4-AccSuite (to be confirmed).
    /// </summary>
    public class Rc4N
    {
        const int S_LENGTH = 256;
        const int K_INIT = 170; // hard-coded in game code

        // The name are no the best but don't blame me.
        // These are the same name of the "original" ARCFOUR algorithm.
        readonly byte[] s;
        int i;
        int j;
        int k;

        private Rc4N(byte[] key)
        {
            i = 0;
            j = 0;
            k = K_INIT;

            // Initialize the 's' buffer
            s = new byte[S_LENGTH];
            KeyScheduling(s, key);
        }

        public static void Decrypt(uint seed, DataStream input, DataStream output)
        {
            // seed seems to be always the same but the input length is variable
            byte[] key = GenerateKey(seed, (uint)input.Length);

            var rc4n = new Rc4N(key);
            rc4n.Run(input, output);
        }

        static byte[] GenerateKey(uint seed, uint inputLength)
        {
            byte[] key = new byte[16]; // 16 * 8 = 128 bits of key
            uint keyPart = seed ^ inputLength;
            BitConverter.TryWriteBytes(key.AsSpan(0, 4), keyPart);

            keyPart = ((seed >> 24) | (seed << 8)) ^ inputLength;
            BitConverter.TryWriteBytes(key.AsSpan(4, 4), keyPart);

            keyPart = ((seed >> 16) | (seed << 16)) ^ inputLength;
            BitConverter.TryWriteBytes(key.AsSpan(8, 4), keyPart);

            keyPart = ((seed >> 8) | (seed << 24)) ^ inputLength;
            BitConverter.TryWriteBytes(key.AsSpan(12, 4), keyPart);

            return key;
        }

        static void KeyScheduling(byte[] s, byte[] key)
        {
            // The game has an optimization to initialize the buffer quickly
            // by writing 32-bit values (using the char * as uint *).
            // In C# that's not recommended so we do the byte a byte one.
            // Anyway it's just a quick 256 iteration loop...
            for (int i = 0; i < s.Length; i++) {
                s[i] = (byte)i;
            }

            // The difference with the "standard" ARCFOUR is that we iterate
            // backwards.
            int keyIdx = 0;
            int j = 0;
            for (int i = s.Length - 1; i >= 0; i--) {
                j = (j + s[i] + key[keyIdx]) % s.Length;
                keyIdx = keyIdx == key.Length - 1 ? 0 : keyIdx + 1;

                byte swap = s[i];
                s[i] = s[j];
                s[j] = swap;
            }
        }

        void  Run(DataStream input, DataStream output)
        {
            var reader = new DataReader(input);
            while (!input.EndOfStream) {
                uint data = reader.ReadUInt32();

                int mode = GetMode(data);
                if (mode == 0) {
                    Decrypt32(data, output);
                } else if (mode == 1 || mode == 3) {
                    Decrypt32Sum(data, output);
                } else if (mode == 2) {
                    Decrypt32Xored(data, output);
                }
            }
        }

        static int GetMode(uint data)
        {
            uint flag = data >> 24;
            if ((flag & 0x0E) != 0x0A) {
                return 0;
            }

            if ((flag & 0xF0) == 0xF0) {
                return 1;
            }

            if ((flag & 0x01) == 0x01) {
                return 2;
            }

            return 3;
        }

        byte NextRandom()
        {
            // Almost identical to the standard ARCFOUR PRGA but we sum to
            // both indexes a third constant 'k'.
            i = (k + i + 1) & 0xFF;
            j = (k + j + s[i]) & 0xFF;

            byte swap = s[i];
            s[i] = s[j];
            s[j] = swap;

            int index = (s[i] + s[j]) & 0xFF;
            return s[index];
        }

        void Decrypt32Sum(uint data, DataStream output)
        {
            // Get a decrypted 32-bits value by substracting and XOR'ing
            uint value = data & 0x00FFFFFF;
            value -= 2114;
            value &= 0x00FFFFFF;

            // In this case we sum instead of substract the K constant
            uint flag = data >> 24;
            k += (byte)flag;

            flag ^= 0x01;
            value |= flag << 24;
            output.Write(BitConverter.GetBytes(value), 0, 4);
        }

        void Decrypt32Xored(uint data, DataStream output)
        {
            // Decrypt bytes 0, 1 and 2 with regular ARCFOUR XOR operations NOT'ed
            // XOR'ing with 0x00 is the same as a NOT
            for (int i = 0; i < 3; i++) {
                byte encrypted = (byte)((data >> (i * 8)) & 0xFF);
                byte random = NextRandom();
                output.WriteByte((byte)(encrypted ^ random ^ 0x00));

                k = encrypted;
            }

            // Update constant K that it's also a decrypted byte XOR'ed
            byte flag = (byte)(data >> 24);
            k -= flag;
            output.WriteByte((byte)(flag ^ 0x01));
        }

        void Decrypt32(uint data, DataStream output)
        {
            // Decrypt bytes 0, 1 and 2 with regular ARCFOUR XOR operations
            for (int i = 0; i < 3; i++) {
                byte encrypted = (byte)((data >> (i * 8)) & 0xFF);
                byte random = NextRandom();
                output.WriteByte((byte)(encrypted ^ random));

                k = encrypted;
            }

            // Update constant K that it's also a decrypted byte
            byte flag = (byte)(data >> 24);
            k -= flag;
            output.WriteByte(flag);
        }
    }
}
