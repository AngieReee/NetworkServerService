using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkServerService.Models
{
    public class Crc16
    {
        private const ushort Polynomial = 0xA001;

        public static ushort Calculate(byte[] data)
        {
            ushort crc = 0xFFFF;

            foreach (byte b in data)
            {
                crc ^= b;

                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc >>= 1;
                        crc ^= Polynomial;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }

            return crc;
        }

        public static byte[] ToBytes(ushort crc)
        {
            return new byte[2]
            {
            (byte)(crc & 0xFF),
            (byte)((crc >> 8) & 0xFF)
            };
        }
    }
}
