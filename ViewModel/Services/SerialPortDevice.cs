using System;
using System.IO.Ports;
using System.Threading;

namespace HexLoader.ViewModel.Services
{
    public class SerialPortDevice
    {
        private readonly SerialPort _serialPort;

        public SerialPortDevice(string comPort, int baudRate, int timeQut)
        {
            _serialPort = new SerialPort(comPort, baudRate)
            {
                ReadTimeout = timeQut,
                WriteTimeout = timeQut
            };
        }

        public void Send(byte[] buffer)
        {
            _serialPort.Write(buffer, 0, buffer.Length);
        }

        public void OpenSerialPort()
        {
            _serialPort.Open();
        }

        /// <summary>
        /// Отправляет запрос и читает ответ в виде отдного байта.
        /// По которому определяется успешность запроса.
        /// Если произошла ошибка, выкидываем из загрузки.
        /// </summary>
        /// <param name="buffer"></param>
        public byte WriteReadDevice(byte[] buffer)
        {
            Thread.Sleep(50);

            byte readByte;

            try
            {
                _serialPort.Write(buffer, 0, buffer.Length);
            }
            catch (Exception e)
            {
                throw new Exception("Ошибка записи на устройство", e);
            }

            try
            {
                while (true)
                {
                    try
                    {
                        readByte = (byte)_serialPort.ReadByte();

                        break;
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Ошибка чтения с устройства", e);
            }

            return readByte;
        }

        /// <summary>
        /// Read one byte
        /// </summary>
        /// <returns></returns>
        public byte ReadByte()
        {
            var b = (byte)_serialPort.ReadByte();
            Thread.Sleep(50);
            return b;
        }


        public void Close()
        {
            _serialPort.Close();
        }

        public bool IsOpen() => _serialPort.IsOpen;
    }
}