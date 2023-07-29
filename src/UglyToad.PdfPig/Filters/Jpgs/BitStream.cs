namespace BigGustave.Jpgs
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class BitStream
    {

        private bool isLittleEndian = BitConverter.IsLittleEndian;

        private readonly BitArray bitArray;

        private int bitOffset;
        private readonly IReadOnlyList<byte> data;

        public BitStream(IReadOnlyList<byte> data)
        {
            this.data = data;
            bitArray = new BitArray(data.ToArray());
        }

        public int Read()
        {
            var byteIndex = bitOffset / 8;

            if (byteIndex >= data.Count)
            {
                return -1;
            }

            var withinByteIndex = bitOffset - (byteIndex * 8);

            bitOffset++;

            // TODO: LSB?
            return bitArray[bitOffset + 7 - withinByteIndex] ? 1 : 0;
            //var byteVal = data[byteIndex];
            //return ((1 << (7 - withinByteIndex)) & byteVal) > 0 ? 1 : 0;
        }

        public int ReadNBits(int length)
        {
            var result = 0;
            for (var i = 0; i < length; i++)
            {
                var bit = Read();

                if (bit < 0)
                {
                    return 0;
                    throw new InvalidOperationException($"Encountered end of bit stream while trying to read {length} bytes.");
                }

                result = (result << 1) + bit;
            }

            return result;
        }
    }
}