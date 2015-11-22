﻿namespace Axh.Retro.CPU.Common.Contracts.Config
{
    using Axh.Retro.CPU.Common.Contracts.Memory;

    public interface IMemoryBankConfig
    {
        MemoryBankType Type { get; }

        byte? BankId { get; }

        ushort Address { get; }

        ushort Length { get; }

        /// <summary>
        /// Initial state of the memory bank.
        /// </summary>
        byte[] State { get; }
    }
}