using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using HexLoader.Model;
using HexLoader.Properties;
using HexLoader.ViewModel.BaseVm;
using HexLoader.ViewModel.Services;
using Microsoft.Win32;

namespace HexLoader.ViewModel
{
    public class MainWindowVm : BaseVm.BaseVm
    {
        public ObservableCollection<string> PortsList { get; set; }

        private Thread _loadThread;

        public List<int> BaudRateList { get; set; } = new List<int>
        {
            110, 300, 600, 1200,
            2400, 4800, 9600, 14400,
            19200, 38400, 57600, 115200,
            128000, 256000
        };

        private SerialPortDevice _serialPortDevice;

        public List<BodySizePageList> BodySizePageList { get; set; } = new List<BodySizePageList>
        {
            new BodySizePageList
            {
                Value = "64 byte",
                BodySizePage = BodySizePage.B64
            },
            new BodySizePageList
            {
                Value = "128 byte",
                BodySizePage = BodySizePage.B128
            },
            new BodySizePageList
            {
                Value = "256 byte",
                BodySizePage = BodySizePage.B256
            }
        };

        public MainWindowVm()
        {
            PortsList = new ObservableCollection<string>(SerialPort.GetPortNames());
            try
            {
                BaudRate = Settings.Default.BaudRate;
                ComPort = Settings.Default.ComPort;
                BodySizePage = (BodySizePage) Settings.Default.BodySizePage;
                RetryLoadChar = Settings.Default.RetryLoadChar;
                ContinueLoadChar = Settings.Default.ContinueLoadChar;
                HexFilePath = Settings.Default.HexFilePath;
                PreRequest = Settings.Default.PreRequest;
                TimeOut = Settings.Default.TimeOut;
            }
            catch
            {
                //ignore
            }
        }

        #region Property

        public bool IsConnectDevice { get; set; }
        public bool IsActiveLoad { get; set; }
        public int PageCount { get; set; }
        public int CurrentPagerLoad { get; set; }

        private string _hexFilePath;

        public string HexFilePath
        {
            get => _hexFilePath;
            set
            {
                _hexFilePath = value;
                OnPropertyChanged(nameof(HexFilePath));
                Settings.Default.HexFilePath = value;
                Settings.Default.Save();
            }
        }


        private int _timeOut = 100;

        public int TimeOut
        {
            get => _timeOut;
            set
            {
                _timeOut = value;
                OnPropertyChanged(nameof(TimeOut));
                Settings.Default.TimeOut = value;
                Settings.Default.Save();
            }
        }

        private string _preRequest = string.Empty;

        public string PreRequest
        {
            get => _preRequest;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {


                    try
                    {
                        var msg = value.Replace(" ", "");
                        var unused = StringToByteArray(msg);
                    }
                    catch
                    {
                        MessageBox.Show("Invalid hex value", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }


                _preRequest = value;
                OnPropertyChanged(nameof(PreRequest));
                Settings.Default.PreRequest = value;
                Settings.Default.Save();
            }
        }

        private string _comPort;

        public string ComPort
        {
            get => _comPort;
            set
            {
                _comPort = value;
                OnPropertyChanged(nameof(ComPort));
                Settings.Default.ComPort = value;
                Settings.Default.Save();
            }
        }

        private int _baudRate;

        public int BaudRate
        {
            get => _baudRate;
            set
            {
                _baudRate = value;
                OnPropertyChanged(nameof(BaudRate));
                Settings.Default.BaudRate = value;
                Settings.Default.Save();
            }
        }

        private BodySizePage _bodySizePage;

        public BodySizePage BodySizePage
        {
            get => _bodySizePage;
            set
            {
                _bodySizePage = value;
                OnPropertyChanged(nameof(BodySizePage));
                Settings.Default.BodySizePage = (int) value;
                Settings.Default.Save();
            }
        }


        private string _continueLoadChar;

        /// <summary>
        /// Символ продолжения загрузки, который должно прислать устройство
        /// 1 байт
        /// </summary>
        public string ContinueLoadChar
        {
            get => _continueLoadChar;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                //Проверка на валидность перевода в байт
                try
                {
                    var unused = Convert.ToByte(value, 16);
                }
                catch
                {
                    MessageBox.Show("Invalid Hex Byte value", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (value == RetryLoadChar)
                {
                    MessageBox.Show("RetryLoadChar и ContinueLoadChar  не должны быть одинаковыми.", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _continueLoadChar = value;
                OnPropertyChanged(nameof(ContinueLoadChar));
                Settings.Default.ContinueLoadChar = value;
                Settings.Default.Save();
            }
        }

        private string _retryLoadChar;

        /// <summary>
        /// Символ ошибки загрузки и требование отправить повторно страницу на устройство
        /// 1 байт
        /// </summary>
        public string RetryLoadChar
        {
            get => _retryLoadChar;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                //Проверка на валидность перевода в байт
                try
                {
                    var unused = Convert.ToByte(value, 16);
                }
                catch
                {
                    MessageBox.Show("Invalid Hex Byte value", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (value == ContinueLoadChar)
                {
                    MessageBox.Show("RetryLoadChar и ContinueLoadChar  не должны быть одинаковыми.", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _retryLoadChar = value;
                OnPropertyChanged(nameof(RetryLoadChar));
                Settings.Default.RetryLoadChar = value;
                Settings.Default.Save();
            }
        }

        #endregion

        #region Command

        public RelayCommand SetHexFilePathCommand => new RelayCommand(SetHexFilePath);

        public RelayCommand LoadHexFileOnDeviceCommand =>
            new RelayCommand(LoadHexFileOnDevice, LoadHexFileOnDeviceValidation);

        public RelayCommand UpdateComPortsSourceCommand => new RelayCommand(UpdateComPortsSource);

        public RelayCommand ConnectUnConnectCommand => new RelayCommand(ConnectUnConnect, ConnectUnConnectValidation);

        #endregion

        #region MethodCommand

        private void ConnectUnConnect()
        {
            var isOpen = _serialPortDevice != null && _serialPortDevice.IsOpen();

            if (!isOpen)
            {
                _serialPortDevice = new SerialPortDevice(ComPort, BaudRate, TimeOut);
                try
                {
                    _serialPortDevice.OpenSerialPort();
                    IsConnectDevice = true;
                }
                catch (Exception e)
                {
                    IsConnectDevice = false;
                    MessageBox.Show($"{e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (!_serialPortDevice.IsOpen())
                {
                    IsConnectDevice = false;
                    MessageBox.Show($"{ComPort} закрыт", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                _serialPortDevice.Close();
                IsConnectDevice = false;
            }
        }

        private bool ConnectUnConnectValidation() =>
            BaudRate != 0 && (ComPort != null && ComPort.Contains("COM")) && !IsActiveLoad;

        public void UpdateComPortsSource()
        {
            var newPortList = new ObservableCollection<string>(SerialPort.GetPortNames());

            //Добавляю новые итемы из полученной коллекции.
            foreach (var port in newPortList)
            {
                if (!PortsList.Contains(port))
                {
                    PortsList.Add(port);
                }
            }


            var deletePorts = new ObservableCollection<string>();

            //Записываю старые итемы в коллекцию на удаление
            foreach (var port in PortsList)
            {
                if (!newPortList.Contains(port))
                {
                    deletePorts.Add(port);
                }
            }

            //Удаляю лишние элементы
            foreach (var port in deletePorts)
            {
                PortsList.Remove(port);
            }
        }

        private void SetHexFilePath()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "hex files (*.hex)|*.hex",
                FilterIndex = 2,
            };

            if (!string.IsNullOrEmpty(HexFilePath) && File.Exists(HexFilePath))
            {
                openFileDialog.FileName = HexFilePath;
            }

            var showDialog = openFileDialog.ShowDialog();

            if (showDialog != null && showDialog.Value)
            {
                HexFilePath = openFileDialog.FileName;
                Settings.Default.HexFilePath = HexFilePath;
                Settings.Default.Save();
            }
            else
            {
                MessageBox.Show("Файл не выбран");
            }
        }

        private void LoadHexFileOnDevice()
        {
            IsActiveLoad = true;

            List<HexPage> pages;
            try
            {
                var fileContent = GetContentHexFile();

                var lines = ParseHexFileToHexPages.GetLines(fileContent);

                pages = ParseLinesToPages.GetHexPages(lines, BodySizePage).ToList();
            }
            catch (Exception e)
            {
                MessageBox.Show($"{e.Message}", "Ошибка обработки файла загрузки", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                IsActiveLoad = false;
                return;
            }



            var continueLoadChar = Convert.ToByte(ContinueLoadChar, 16);
            var retryLoadChar = Convert.ToByte(RetryLoadChar, 16);


            _loadThread?.Abort();
            _loadThread = new Thread(() => LoadFileAsync(pages, continueLoadChar, retryLoadChar));
            _loadThread.Start();

        }

        public void CloseProgram()
        {
            _loadThread?.Abort();
        }

        private void LoadFileAsync(List<HexPage> pages, byte continueLoadChar, byte retryLoadChar)
        {
            try
            {
                CurrentPagerLoad = 0;
                PageCount = pages.Count;

                //Отправляю пред запрос, если он включен.

                if (!string.IsNullOrEmpty(PreRequest))
                {
                    var msg = PreRequest.Replace(" ", "");
                    var buffer = StringToByteArray(msg);
                    _serialPortDevice.Send(buffer);
                }

                //Проверяю устройство на готовность загрузки.

                byte b;
                try
                {
                    b = _serialPortDevice.ReadByte();
                }
                catch (Exception e)
                {
                    throw new Exception($"Ошибка при чтении {ContinueLoadChar}. Устройство не готово к загрузке", e);
                }

                if (b != continueLoadChar)
                {

                    var answerByte = ByteArrayToString(new[] {b});
                    throw new Exception(
                        $"Устройство не готово к загрузке. Вместо Continue (hex)({ContinueLoadChar}) пришло ({answerByte})");
                }

                for (var i = 0; i < pages.Count; i++)
                {
                    var resultByte = _serialPortDevice.WriteReadDevice(pages[i].SendBody);
                    var attempt = 0;
                    CurrentPagerLoad = i + 1;

                    if (resultByte == continueLoadChar)
                    {
                        continue;
                    }

                    if (resultByte == retryLoadChar)
                    {
                        attempt++;
                        i--;

                        if (attempt > 3)
                        {
                            throw new Exception("Превышено количество попыток загрузки страницы");
                        }

                        continue;
                    }

                    throw new Exception(
                        $"Получен иной символ, вместо Символа повтора - {RetryLoadChar}/ и Символа успешной записи - {ContinueLoadChar}");
                }


                MessageBox.Show("Загрузка завершена", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception e)
            {
                IsActiveLoad = false;
                MessageBox.Show($"{e.Message}", "Ошибка при загрузке файла", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            IsActiveLoad = false;
        }

        private bool LoadHexFileOnDeviceValidation() =>
            HexFilePath != null && File.Exists(HexFilePath) && IsConnectDevice;

        #endregion

        private string GetContentHexFile()
        {
            string fileContent;

            var fileStream = File.OpenRead(HexFilePath);

            using (var reader = new StreamReader(fileStream))
            {
                fileContent = reader.ReadToEnd();
            }

            return fileContent;
        }

        public byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }
        public  string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}