using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace GEM.Emulation
{
    public class Cartridge
    {
        #region Fields
        private bool _isCartridgeLoaded;
        private byte[] _data;
        private byte[] _ram;
        private int _romBank;
        private int _ramBank;
        private bool _isRamEnabled;
        private int _bankMode;
        private int _mbc;
        public bool IsCGB;
        private string _saveFile;
        public string Title { get; private set; }
        #endregion

        #region Constructors
        public Cartridge()
        {
            _data = new byte[0x8000];
            _ram = new byte[0x2000];
            _isCartridgeLoaded = false;
            _romBank = 1;
            _ramBank = 0;
            Title = "";
        }
        #endregion

        #region Properties
        #endregion

        #region Methods
        public void Reset()
        {
            _data = new byte[0x8000];
            _ram = new byte[0x2000];
            _isCartridgeLoaded = false;
            _romBank = 1;
            _ramBank = 0;
            Title = "";
        }
        public void Load(string file)
        {
            if (file == null)
                return;

            // Game file
            if (File.Exists(file))
            {
                _data = File.ReadAllBytes(file);
                _isCartridgeLoaded = true;
                // Extract Title
                Title = "";
                for (ushort i = 0x134; i <= 0x142; i++)
                {
                    if (Read(i) != 0) { Title += (char)Read(i); }
                }
                _mbc = Read(0x147);
                IsCGB = (Read(0x143) & 0x80) == 1;
            }

            // Save file
            int slashPos = file.LastIndexOf('/') + 1;
            int dotPos = file.LastIndexOf('.');
            string baseName = file.Substring(slashPos, dotPos - slashPos);
            _saveFile = string.Format("save/{0}.sav", baseName);
            if (File.Exists(_saveFile))
            {
                _ram = File.ReadAllBytes(_saveFile);
            }
            else
            {
                int len = 0;
                switch (_mbc)
                {
                    default:
                    case 0x00: break;   // ROM ONLY
                    case 0x01: break;   // MBC1
                    case 0x02:          // MBC1+RAM
                    case 0x03:          // MBC1+RAM+BATTERY
                    case 0x1B:          // MBC5+RAM+BATTERY
                        len = 0xC000;
                        break;
                }
                _ram = new byte[len];
            }
        }

        public byte Read(ushort pos)
        {
            if (!_isCartridgeLoaded) return 0xFF;

            if (pos >= 0 && pos < 0x4000)
            {
                // ROM Bank 0
                switch (_mbc)
                {
                    default:
                    case 0x00:  // ROM ONLY
                        return _data[pos];
                    case 0x01:  // MBC1
                    case 0x02:  // MBC1+RAM
                    case 0x03:  // MBC1+RAM+BATTERY
                        if (_bankMode == 01) return _data[pos + (_romBank & 0x60) * 0x4000];
                        return _data[pos];
                    case 0x1B:  // MBC5+RAM+BATTERY
                        return _data[pos];
                }
            }
            else if (pos >= 0x4000 && pos < 0x8000)
            {
                // ROM Bank 1-n
                switch (_mbc)
                {
                    default:
                    case 0x00:  // ROM ONLY
                        return _data[pos];
                    case 0x01:  // MBC1
                    case 0x02:  // MBC1+RAM
                    case 0x03:  // MBC1+RAM+BATTERY
                    case 0x1B:  // MBC5+RAM+BATTERY
                        return _data[pos - 0x4000 + _romBank * 0x4000];
                }
            }
            else return 0xFF;
        }
        public byte ReadRAM(ushort pos)
        {
            // RAM Bank 0-n
            if (_isRamEnabled)
            {
                return _ram[pos + _ramBank * 0x2000];
            }
            else return 0xFF;
        }

        public void Write(ushort pos, byte value)
        {
            if (pos >= 0 && pos < 0x2000)
            {
                // RAM Enable
                if ((value & 0xA) == 0xA)
                {
                    _isRamEnabled = true;
                }
                else _isRamEnabled = false;
            }
            else if (pos >= 0x2000 && pos < 0x4000)
            {
                // ROM Bank Number (Bit 0 to 4)
                switch (_mbc)
                {
                    default:
                    case 0x00:  // ROM ONLY
                        break;
                    case 0x01:  // MBC1
                    case 0x02:  // MBC1+RAM
                    case 0x03:  // MBC1+RAM+BATTERY
                        if ((value & 0x1F) == 0) value++;
                        _romBank = (_romBank & 0x60) + (value & 0x1F);
                        break;
                    case 0x1B:  // MBC5+RAM+BATTERY
                        _romBank = value & 0x7F;
                        break;
                }
            }
            else if (pos >= 0x4000 && pos < 0x6000)
            {
                // RAM Bank Number
                switch (_bankMode)
                {
                    default:
                    case 0:
                        // ROM Banking Mode (Bit 5 to 6)
                        _romBank = (_romBank & 0x1F) + ((value & 0b11) << 5);
                        break;
                    case 1:
                        // RAM Banking Mode
                        _ramBank = value & 0b11;
                        break;
                }
            }
            else if (pos >= 0x6000 && pos < 0x8000)
            {
                // ROM/RAM Mode Select
                switch (value)
                {
                    default:
                    case 0:
                        // ROM Banking Mode
                        _bankMode = 0; break;
                    case 1:
                        // RAM Banking Mode
                        _bankMode = 1; break;
                }
            }
        }
        public void WriteRAM(ushort pos, byte value)
        {
            // RAM Bank 0-n
            _ram[pos + _ramBank * 0x2000] = value;
        }

        public void SaveToFile()
        {
            // currently called on PowerOff sequence
            if (!_ram.All(item => item == 0))   // RAM not empty
            {
                if (!Directory.Exists("save/"))
                    Directory.CreateDirectory("save/");
                File.WriteAllBytes(_saveFile, _ram);
            }
        }
        #endregion
    }
}
