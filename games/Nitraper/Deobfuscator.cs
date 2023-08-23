// Copyright (c) 2023 Benito Palacios Sánchez (pleonex)

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
using System;
using Yarhl.FileFormat;
using Yarhl.IO;

namespace Nitraper;

public class Deobfuscator : IConverter<IBinary, BinaryFormat>
{
    private AntiPiracyOverlayInfo info;

    public Deobfuscator(AntiPiracyOverlayInfo info)
    {
        this.info = info ?? throw new ArgumentNullException(nameof(info));
    }

    public BinaryFormat Convert(IBinary source)
    {
        // Do not overwrite same file, create a copy
        var deobfuscated = new BinaryFormat();
        var newStream = deobfuscated.Stream;
        source.Stream.WriteTo(newStream);

        // 1.- Decrypt AP infrastructure code
        DecryptInfrastructureCode(newStream);

        // 2.- Validate jump functions
        VerifyJumpChecksums(newStream);

        // 3.- Decrypt AP code
        // DecryptApCode(newStream, info);

        return deobfuscated;
    }

    private void DecryptInfrastructureCode(DataStream stream)
    {
        foreach (var decryptionCall in info.InfrastructureEncryption.DecryptionCallers) {
            stream.Position = decryptionCall - info.OverlayRamAddress;

            Console.WriteLine($"Decrypting group from 0x{decryptionCall:X8}");
            EntrypointDecrypter.DecryptFromCall(stream, info);
        }
    }

    private void VerifyJumpChecksums(DataStream stream)
    {
        foreach (var jumpFunc in info.JumpFunctions) {
            uint offset = jumpFunc.Address;
            stream.Position = offset - info.OverlayRamAddress;

            uint actual = 0;
            if (jumpFunc.Kind == CodeChecksumKind.Ninokuni) {
                actual = Checksum.ComputeKind1(stream, jumpFunc.Length * 4);
            } else if (jumpFunc.Kind == CodeChecksumKind.VPY) {
                actual = Checksum.ComputeKind2(stream, jumpFunc.Length * 4);
            }

            if (actual != jumpFunc.ExpectedChecksum) {
                throw new FormatException($"Invalid checksum for {jumpFunc.Address}");
            } else {
                Console.WriteLine($"Valid checksum for jump: 0x{jumpFunc.Address:X8}");
            }
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
}
