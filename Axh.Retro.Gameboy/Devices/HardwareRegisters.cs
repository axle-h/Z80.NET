﻿namespace Axh.Retro.GameBoy.Devices
{
    using System;

    using Axh.Retro.CPU.Common.Contracts.Memory;
    using Axh.Retro.GameBoy.Contracts.Devices;
    using Axh.Retro.GameBoy.Contracts.Peripherals;

    public class HardwareRegisters : IHardwareRegisters
    {
        private const ushort P1 = 0xff00; // Register for reading joy pad info and determining system type. (R/W)
        private const ushort SB = 0xff01; // Serial transfer data (R/W)
        private const ushort SC = 0xff02; // SIO control (R/W)

        public HardwareRegisters(IJoyPad joyPad, ISerialPort serialPort)
        {
            JoyPad = joyPad;
            SerialPort = serialPort;
        }

        private const ushort Address = 0xff00;
        private const ushort Length = 0x7f;
        
        public MemoryBankType Type => MemoryBankType.Peripheral;

        ushort IAddressSegment.Address => Address;

        ushort IAddressSegment.Length => Length;

        public byte ReadByte(ushort address)
        {
            switch (address)
            {
                case P1:
                    return this.JoyPad.Register;
                case SB:
                    return this.SerialPort.SerialData;
                case SC:
                    return this.SerialPort.ControlRegister;
                default:
                    throw new NotImplementedException();
            }
        }

        public ushort ReadWord(ushort address)
        {
            var bytes = this.ReadBytes(address, 2);
            return BitConverter.ToUInt16(bytes, 0);
        }

        public byte[] ReadBytes(ushort address, int length)
        {
            var bytes = new byte[length];
            for (var i = 0; i < length; i++)
            {
                bytes[i] = this.ReadByte(unchecked((ushort)(address + i)));
            }
            return bytes;
        }

        public void WriteByte(ushort address, byte value)
        {
            switch (address)
            {
                case P1:
                    this.JoyPad.Register = value;
                    break;
                case SB:
                    this.SerialPort.SerialData = value;
                    break;
                case SC:
                    this.SerialPort.ControlRegister = value;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void WriteWord(ushort address, ushort word)
        {
            var bytes = BitConverter.GetBytes(word);
            this.WriteBytes(address, bytes);
        }

        public void WriteBytes(ushort address, byte[] values)
        {
            for (var i = 0; i < values.Length; i++)
            {
                this.WriteByte(unchecked((ushort)(address + i)), values[i]);
            }
        }

        public IJoyPad JoyPad { get; }

        public ISerialPort SerialPort { get; }
    }
}
