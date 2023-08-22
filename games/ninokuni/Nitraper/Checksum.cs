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

    public static class Checksum
    {
        public static uint ComputeKind1(DataStream stream, int size)
        {
            if ((size % 4) != 0) {
                Console.WriteLine("!ERROR: Invalid size to calculate checksum");
                return 0xFFFFFFFF; // unsigned -1
            }

            var reader = new DataReader(stream);

            uint checksum = 0;
            for (int i = 0; i < size / 4; i++) {
                uint data = reader.ReadUInt32();

                // Move lower 5 bits into the higher part
                data = (data << 27) | (data >> 5);

                // XOR with the current result
                checksum ^= data;
            }

            return checksum;
        }

        public static uint ComputeKind2(DataStream stream, int size)
        {
            if ((size % 4) != 0) {
                throw new ArgumentException("!ERROR: Invalid size to calculate checksum", nameof(size));
            }

            var reader = new DataReader(stream);

            size /= 4;
            uint checksum = 0;
            do
            {
                uint data = reader.ReadUInt32();

                // shifting in C# more than 32 is problematic
                // https://stackoverflow.com/questions/64548071/why-would-a-32-bit-shift-in-c-sharp-return-the-value-it-was-originally-shifting
                int a = (byte)(0x20 - size);
                uint b = (a >= 0x20) ? 0 : (data << a);
                uint c = (size >= 0x20) ? 0 : (data >> size);
                checksum ^= (b | c);
            } while (--size > 0);

            return checksum;
        }
    }
}