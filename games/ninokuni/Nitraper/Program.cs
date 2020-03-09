namespace Nitraper
{
    using System;
    using Yarhl.IO;

    class Program
    {
        static void Main(string[] args)
        {
            DecryptNinokuni(args[0]);
        }

        static void DecryptNinokuni(string overlay19)
        {
            const uint BaseAddress = 0x02159FE0;
            uint[] decryptionCalls = new uint[] {
               0x0215c104,
               0x0215b750,
               0x0215a750,
               0x0215ac5c,
               0x0215b428,
            };
            uint[,] checksums = new uint[,] {
               { 0x215B34C, 0x2FBB82E1 }
            };

            using (var stream = DataStreamFactory.FromFile(overlay19, FileOpenMode.ReadWrite)) {
               foreach (uint call in decryptionCalls) {
                   stream.Position = call - BaseAddress;
                   Decrypter.DecryptFromCall(stream, BaseAddress);
               }

               for (int i = 0; i < checksums.GetLength(0); i++) {
                   stream.Position = checksums[i, 0] - BaseAddress;
                   uint checksum = Checksum.Run(stream);
                   Console.WriteLine($"Checksum: {checksum == checksums[i, 1]}");
               }
            }

            uint seed = 0x02175b00;
            uint offset = 0x0215d0d8;
            uint length = 0x0215e44c;

            seed -= 0x0215C1CC + 0x2100;
            offset -= 0x2100;
            length -= 0x215C1CC + 0x2100;

            offset -= BaseAddress;
            using (var file = DataStreamFactory.FromFile(overlay19, FileOpenMode.ReadWrite))
            using (var input = new DataStream(file, offset, length))
            using (var output = new DataStream(file, offset, length))
            {
                Rc4N.Decrypt(seed, input, output);
            }
        }
    }
}
