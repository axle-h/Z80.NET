﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Axh.Retro.CPU.Common.Contracts.Memory;
using Axh.Retro.GameBoy.Contracts.Devices;
using Axh.Retro.GameBoy.Registers.Interfaces;

namespace Axh.Retro.GameBoy.Devices
{
    public class HardwareRegisters : IHardwareRegisters
    {
        private const ushort Address = 0xff00;
        private const ushort Length = 0x80;
        private readonly IDictionary<ushort, IRegister> _registers;

        public HardwareRegisters(IEnumerable<IRegister> registers,
            IJoyPadRegister joyPad,
            ISerialPortRegister serialPort,
            IGpuRegisters gpuRegisters,
            IInterruptFlagsRegister interruptFlagsRegister)
        {
            JoyPad = joyPad;
            SerialPort = serialPort;
            _registers =
                registers.Concat(new[]
                {
                    joyPad,
                    serialPort,
                    serialPort.SerialData,
                    gpuRegisters.ScrollXRegister,
                    gpuRegisters.ScrollYRegister,
                    gpuRegisters.CurrentScanlineRegister,
                    gpuRegisters.LcdControlRegister,
                    gpuRegisters.LcdMonochromePaletteRegister,
                    gpuRegisters.LcdStatusRegister,
                    interruptFlagsRegister
                }).ToDictionary(x => (ushort) (x.Address - Address));
        }

        public MemoryBankType Type => MemoryBankType.Peripheral;

        ushort IAddressSegment.Address => Address;

        ushort IAddressSegment.Length => Length;

        public byte ReadByte(ushort address)
        {
            // TODO: remove check once all registers implemented.
            if (_registers.ContainsKey(address))
            {
                return _registers[address].Register;
            }
            Debug.WriteLine("Missing Hardware Register: 0x" + (address + Address).ToString("x4"));
            return 0x00;
        }

        public ushort ReadWord(ushort address)
        {
            var bytes = ReadBytes(address, 2);
            return BitConverter.ToUInt16(bytes, 0);
        }

        public byte[] ReadBytes(ushort address, int length)
        {
            var bytes = new byte[length];
            ReadBytes(address, bytes);
            return bytes;
        }

        public void ReadBytes(ushort address, byte[] buffer)
        {
            for (var i = 0; i < buffer.Length; i++)
            {
                buffer[i] = ReadByte(unchecked((ushort) (address + i)));
            }
        }

        public void WriteByte(ushort address, byte value)
        {
            // TODO: remove check once all registers implemented.
            if (_registers.ContainsKey(address))
            {
                _registers[address].Register = value;
            }
            /*
            else
            {
                Debug.WriteLine("Missing Hardware Register: 0x" + (address + Address).ToString("x4"));
            }*/
        }

        public void WriteWord(ushort address, ushort word)
        {
            var bytes = BitConverter.GetBytes(word);
            WriteBytes(address, bytes);
        }

        public void WriteBytes(ushort address, byte[] values)
        {
            for (var i = 0; i < values.Length; i++)
            {
                WriteByte(unchecked((ushort) (address + i)), values[i]);
            }
        }

        public IJoyPad JoyPad { get; }

        public ISerialPort SerialPort { get; }

        public override string ToString()
        {
            return $"{Type}: 0x{Address:x4} - 0x{Address + Length - 1:x4}";
        }
    }
}