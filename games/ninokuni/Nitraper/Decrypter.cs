namespace Nitraper
{
    using System;
    using Yarhl.IO;

    public static class Decrypter
    {
        const uint SEED = 0x7FEC9DF1;
        const uint ADDRESS_OFFSET = 0x2100;
        const uint SIZE_CONSTANT = 0x0215C1CC;
        const uint CALL_SIZE = 0x0C;

        public static void DecryptFromCall(DataStream stream, uint baseAddress)
        {
            var reader = new DataReader(stream);

            // The decrypt function would destroy the three call instructions
            // (12 bytes) with 00. But we won't do that.
            stream.Position += CALL_SIZE;

            // Read the first block
            uint offset = reader.ReadUInt32();
            uint size = reader.ReadUInt32();
            while (offset > 0x00) {
                // Deobfuscate the offset and size
                offset -= ADDRESS_OFFSET;
                size -= SIZE_CONSTANT + ADDRESS_OFFSET;
                Console.WriteLine($"* [0x{offset:X8}, 0x{offset + size:X8}) - 0x{size:X4}");

                // Convert the RAM address to a file address
                offset -= baseAddress;

                // stream.RunInPosition(() => Decrypt(stream, size), offset);

                // Read next block
                offset = reader.ReadUInt32();
                size = reader.ReadUInt32();
            }
        }

        public static void Decrypt(DataStream stream, uint size)
        {
            uint key = SEED;
            var reader = new DataReader(stream);
            var writer = new DataWriter(stream);

            long endPosition = stream.Position + size;
            while (stream.Position < endPosition) {
                uint data = reader.ReadUInt32();
                data = data ^ key;
                key = key ^ (data - (data >> 8));

                stream.Position -= 4;
                writer.Write(data);
            }
        }
    }
}
