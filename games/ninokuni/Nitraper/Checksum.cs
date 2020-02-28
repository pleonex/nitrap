namespace Nitraper
{
    using Yarhl.IO;

    public static class Checksum
    {
        const uint DATA_SIZE = 0x24;

        public static uint Run(DataStream stream)
        {
            var reader = new DataReader(stream);

            uint checksum = 0;
            for (int i = 0; i < DATA_SIZE / 4; i++) {
                uint data = reader.ReadUInt32();
                data = (data << 27) | (data >> 5);
                checksum ^= data;
            }

            return checksum;
        }
    }
}