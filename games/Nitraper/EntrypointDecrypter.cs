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

    public static class EntrypointDecrypter
    {
        const uint CallFuncSize = 0x0C;

        public static void DecryptFromCall(DataStream stream, AntiPiracyOverlayInfo config)
        {
            var reader = new DataReader(stream);

            // The game decrypt function destroys the three call instructions
            // (12 bytes) with 00 when this logic run so you don't see the call.
            // But don't worry, we won't do that.
            stream.Position += CallFuncSize;

            // Read offsets until we find a null offset
            uint offset = reader.ReadUInt32();
            uint size = reader.ReadUInt32();
            while (offset > 0x00) {
                // Deobfuscate the offset and size
                offset -= config.AddressOffset;
                size -= config.IntegerOffset + config.AddressOffset;
                Console.WriteLine($"* [0x{offset:X8}, 0x{offset + size:X8}) - 0x{size:X4} @ 0x{offset - config.OverlayRamAddress:X4}");

                // Convert the RAM address to a file address
                offset -= config.OverlayRamAddress;

                // Decrypt and return to current address
                stream.RunInPosition(() => Decrypt(stream, size, config.InfrastructureEncryption.KeySeed), offset);

                // Read next block
                offset = reader.ReadUInt32();
                size = reader.ReadUInt32();
            }
        }

        public static void Decrypt(DataStream stream, uint size, uint seed)
        {
            uint key = seed;
            var reader = new DataReader(stream);
            var writer = new DataWriter(stream);

            long endPosition = stream.Position + size;
            while (stream.Position < endPosition) {
                // Decrypt 32-bits with a XOR
                uint data = reader.ReadUInt32();
                data = data ^ key;

                // Overwrite with the decrypted version
                stream.Position -= 4;
                writer.Write(data);

                // Update the key
                key = key ^ (data - (data >> 8));
            }
        }
    }
}
