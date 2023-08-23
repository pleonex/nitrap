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

    public static class Ninokuni
    {
        public static void Decrypt(string overlay19)
        {
            var config = new AntiPiracyOverlayInfo {
                OverlayRamAddress = 0x02159FE0,
                AddressOffset = 0x2100,
                IntegerOffset = 0x0215C1CC,
                InfrastructureEncryption = new()
                {
                    KeySeed = 0x7FEC9DF1,
                },
            };

            using (var stream = DataStreamFactory.FromFile(overlay19, FileOpenMode.ReadWrite)) {
                DecryptEntrypoints(stream, config);
                VerifyJumpChecksums(stream, config);
                DecryptApCode(stream, config);
                PredictCorrectAnswer();
            }
        }

        static void DecryptEntrypoints(DataStream stream, AntiPiracyOverlayInfo config)
        {
            uint[] decryptionCalls = new uint[] {
                0x0215c104,
                0x0215b750,
                0x0215a750,
                0x0215ac5c,
                0x0215b428,
            };

            foreach (uint call in decryptionCalls) {
                stream.Position = call - config.OverlayRamAddress;
                EntrypointDecrypter.DecryptFromCall(stream, config);
            }
        }

        static void VerifyJumpChecksums(DataStream stream, AntiPiracyOverlayInfo config)
        {
            const int JumpFuncSize = 0x24;

            uint[,] checksums = new uint[,] {
            //  offset      checksum
              { 0x0215D44C, 0x2FBB82E1 },
              { 0x0215E18C, 0x2FBB82E1 },
              { 0x0215C760, 0x2FBB82E1 }
            };

            for (int i = 0; i < checksums.GetLength(0); i++) {
                uint offset = checksums[i, 0] - config.AddressOffset;
                stream.Position = offset - config.OverlayRamAddress;

                uint checksum = Checksum.ComputeKind1(stream, JumpFuncSize);
                Console.WriteLine($"Checksum: {checksum == checksums[i, 1]}");
            }
        }

        static void DecryptApCode(DataStream stream, AntiPiracyOverlayInfo config)
        {
            uint[,] encryptInfo = new uint[,] {
            //    seed        offset      length
                { 0x02175b00, 0x0215d0d8, 0x0215e44c },
                { 0x0216F306, 0x0215D87C, 0x0215E388 },
                { 0x0216D1F2, 0x0215C2F0, 0x0215E3DC },
                { 0x02175760, 0x0215CDA0, 0x0215E44C },
                { 0x0216F5B6, 0x0215DAB4, 0x0215E5F0 },
                { 0x0216D452, 0x0215C528, 0x0215E3DC },
            };

            for (int i = 0; i < encryptInfo.GetLength(0); i++) {
                uint seed = encryptInfo[i, 0] - config.IntegerOffset - config.AddressOffset;
                uint offset = encryptInfo[i, 1] - config.AddressOffset;
                uint length = encryptInfo[i, 2] - config.IntegerOffset - config.AddressOffset;

                offset -= config.OverlayRamAddress;
                using (var input = new DataStream(stream, offset, length))
                using (var output = new DataStream(stream, offset, length)) {
                    Console.WriteLine($"* RC4N {offset:X4}h - {length:X4}h");
                    Rc4N.Decrypt(seed, input, output);
                }
            }
        }

        static void PredictCorrectAnswer()
        {
            uint xBase = 0x0030EB87;
            Func<uint, uint> equation =
                (uint x) => x - (uint)((x * 0x10FEF011L) >> (32 + 4)) * 0xF1;
            Func<uint, uint, uint> total =
                (uint x1, uint x2) => equation(xBase + x1 + x2);

            Console.WriteLine(total(173 * 251, 48443));
            Console.WriteLine(total(173 * 241, 48443));
            Console.WriteLine(total(173 * 251, 46513));
            Console.WriteLine(total(173 * 241, 46513)); // good
        }
    }
}
