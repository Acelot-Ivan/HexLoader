using System.Linq;

namespace HexLoader.Model
{
    public class HexPage
    {
        public HexPage(byte[] location, byte[] body, byte lrc)
        {
            Location = location;
            Body = body;
            Lrc = lrc;

            SendBody = Location.Concat(Body).Concat(new[] { Lrc }).ToArray();
        }

        public byte[] Location { get; set; }
        public byte[] Body { get; set; }
        public byte Lrc { get; set; }


        /// <summary>
        /// Свойсво для сборки данных в один массив,
        /// для последующей отправки на устройство
        /// </summary>
        public byte[] SendBody { get; set; }
    }
}
