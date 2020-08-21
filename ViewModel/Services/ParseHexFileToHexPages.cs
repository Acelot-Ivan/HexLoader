using HexLoader.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HexLoader.ViewModel.Services
{
    public static class ParseHexFileToHexPages
    {

        public static IEnumerable<HexLine> GetLines(string hexFile)
        {
            return hexFile
                .Replace(":00000001FF", "")
                .Split('\n')
                .Select(o => o.Replace("\r", string.Empty))
                .Select(o => o.Replace(":", string.Empty))
                .Where(item => !string.IsNullOrEmpty(item))
                .Select(StringToHexLine).ToArray();
        }

        private static HexLine StringToHexLine(string line)
        {
            var bytes = StringToByteArray(line);
            var bodyEndIndex = bytes.Length - 1; //без Crc

            var hexLine = new HexLine(
                bytes[0],
                new[] { bytes[1], bytes[2] },
                bytes[3],
                bytes.Skip(4).Take(bodyEndIndex - 4).ToArray(),
                bytes.Last());

            return hexLine;

        }

        private static byte[] StringToByteArray(string hex)
        {
            var numberChars = hex.Length;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}
