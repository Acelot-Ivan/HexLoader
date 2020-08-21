using System;
using System.Collections.Generic;
using System.Linq;
using HexLoader.Model;

namespace HexLoader.ViewModel.Services
{
    public static class ParseLinesToPages
    {
        private static readonly byte[] DefaultEndLine =
        {
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF
        };

        private const byte DefaultEndChar = 0xFF;


        public static IEnumerable<HexPage> GetHexPages(IEnumerable<HexLine> hexLines, BodySizePage bodySizePage)
        {
            //Дополняю последнюю строку до 16 байт
            var lines = hexLines.ToList();
            if (lines.Last().Body.Length != 16)
            {
                var body = lines.Last().Body.ToList();
                while (body.Count != 16)
                {
                    body.Add(DefaultEndChar);
                }

                lines.Last().Body = body.ToArray();
            }

            switch (bodySizePage)
            {
                case BodySizePage.B64:
                    return CombineLines64(lines);

                case BodySizePage.B128:
                    return CombineLines128(lines);

                case BodySizePage.B256:
                    return CombineLines256(lines);

                default:
                    throw new ArgumentOutOfRangeException(nameof(bodySizePage), bodySizePage, null);
            }
        }

        private static IEnumerable<HexPage> CombineLines64(List<HexLine> hexLines)
        {
            //Дополняю необходимое кол-во строк, для создания страниц
            while (hexLines.Count() % 4 != 0)
            {
                hexLines.Add(new HexLine(DefaultEndLine));
            }

            var pages = new List<HexPage>();

            for (var i = 0; i < hexLines.Count; i += 4)
            {
                var location = hexLines[i].LocationBytes;
                var body = hexLines[i].Body.Concat(hexLines[i + 1].Body).Concat(hexLines[i + 2].Body)
                    .Concat(hexLines[i + 3].Body).ToArray();
                var lrc = CalculateLrc(location.Concat(body).ToArray());
                pages.Add(new HexPage(location, body, lrc));
            }

            return pages;
        }

        private static IEnumerable<HexPage> CombineLines128(List<HexLine> hexLines)
        {
            //Дополняю необходимое кол-во строк, для создания страниц
            while (hexLines.Count() % 8 != 0)
            {
                hexLines.Add(new HexLine(DefaultEndLine));
            }

            var pages = new List<HexPage>();

            for (var i = 0; i < hexLines.Count; i += 8)
            {
                var location = hexLines[i].LocationBytes;
                var body = hexLines[i].Body.Concat(hexLines[i + 1].Body).Concat(hexLines[i + 2].Body)
                    .Concat(hexLines[i + 3].Body).Concat(hexLines[i + 4].Body).Concat(hexLines[i + 5].Body)
                    .Concat(hexLines[i + 6].Body).Concat(hexLines[i + 7].Body).ToArray();
                var lrc = CalculateLrc(location.Concat(body).ToArray());
                pages.Add(new HexPage(location, body, lrc));
            }

            return pages;
        }

        private static IEnumerable<HexPage> CombineLines256(List<HexLine> hexLines)
        {
            //Дополняю необходимое кол-во строк, для создания страниц
            while (hexLines.Count() % 16 != 0)
            {
                hexLines.Add(new HexLine(DefaultEndLine));
            }

            var pages = new List<HexPage>();

            for (var i = 0; i < hexLines.Count; i += 16)
            {
                var location = hexLines[i].LocationBytes;
                var body = hexLines[i].Body.Concat(hexLines[i + 1].Body).Concat(hexLines[i + 2].Body)
                    .Concat(hexLines[i + 3].Body).Concat(hexLines[i + 4].Body).Concat(hexLines[i + 5].Body)
                    .Concat(hexLines[i + 6].Body).Concat(hexLines[i + 7].Body).Concat(hexLines[i + 8].Body)
                    .Concat(hexLines[i + 9].Body).Concat(hexLines[i + 10].Body).Concat(hexLines[i + 11].Body)
                    .Concat(hexLines[i + 12].Body).Concat(hexLines[i + 13].Body).Concat(hexLines[i + 14].Body)
                    .ToArray();
                var lrc = CalculateLrc(location.Concat(body).ToArray());
                pages.Add(new HexPage(location, body, lrc));
            }

            return pages;
        }


        private static byte CalculateLrc(byte[] bytes) =>
            (byte) bytes.Aggregate(0, (current, t) => current - t);
    }

    public enum BodySizePage
    {
        B64,
        B128,
        B256
    }

    public class BodySizePageList
    {
        public string Value { set; get; }
        public BodySizePage BodySizePage { set; get; }
    }
}