﻿namespace Axh.Retro.CPU.X80.Contracts.Memory
{
    using System;

    public class AddressWriteEventArgs : EventArgs
    {
        public AddressWriteEventArgs(ushort address, ushort length)
        {
            this.Address = address;
            this.Length = length;
        }

        public ushort Address { get; }
        public ushort Length { get; }
    }
}
