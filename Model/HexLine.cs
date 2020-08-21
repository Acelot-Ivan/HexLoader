namespace HexLoader.Model
{
    public class HexLine
    {
        /// <summary>
        /// Конкструктор для вызова при чтении Hex файла. Для создания коллекции строк.
        /// </summary>
        /// <param name="bodySize"></param>
        /// <param name="locationBytes"></param>
        /// <param name="typeMessage"></param>
        /// <param name="body"></param>
        /// <param name="lrc"></param>
        public HexLine(byte bodySize, byte[] locationBytes, byte typeMessage, byte[] body, byte lrc)
        {
            BodySize = bodySize;
            LocationBytes = locationBytes;
            TypeMessage = typeMessage;
            Body = body;
            Lrc = lrc;
        }

        /// <summary>
        /// Конструктор для создания дополнительных строк, для кратности при преобразовании в страницы.
        /// </summary>
        /// <param name="body"></param>
        public HexLine(byte[] body)
        {
            Body = body;
        }

        public byte BodySize { get; set; }
        public byte[] LocationBytes { get; set; }
        public byte TypeMessage { get; set; }
        public byte[] Body { get; set; }
        public byte Lrc { get; set; }
    }
}
