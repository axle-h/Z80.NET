﻿using System;
using System.Collections.Generic;
using System.Linq;
using Axh.Retro.CPU.Common.Contracts.Config;
using Axh.Retro.CPU.Common.Contracts.Exceptions;
using Axh.Retro.CPU.Common.Contracts.Memory;

namespace Axh.Retro.CPU.Common.Memory
{
    public class BankedReadOnlyMemoryBank : IReadableAddressSegment
    {
        private readonly IDictionary<byte, byte[]> banks;
        private readonly IMemoryBankController memoryBankController;

        private byte[] bank;

        public BankedReadOnlyMemoryBank(ICollection<IMemoryBankConfig> bankConfigs,
                                        IMemoryBankController memoryBankController)
        {
            this.memoryBankController = memoryBankController;
            if (bankConfigs == null)
            {
                throw new ArgumentNullException(nameof(bankConfigs));
            }

            if (!bankConfigs.Any())
            {
                throw new ArgumentException("No configs.");
            }

            if (!bankConfigs.All(x => x.BankId.HasValue))
            {
                throw new ArgumentException("All configs must have a bank id.");
            }

            if (bankConfigs.Select(x => x.BankId.Value).Distinct().Count() != bankConfigs.Count)
            {
                throw new ArgumentException("All bank id's must be unique");
            }

            var distinct = bankConfigs.Select(x => new {x.Address, x.Length}).Distinct().ToArray();
            if (distinct.Length > 1)
            {
                throw new ArgumentException("All configs must have same address and length.");
            }

            Address = distinct[0].Address;
            Length = distinct[0].Length;

            var badBank = bankConfigs.FirstOrDefault(x => x.State == null || x.Length != Length);
            if (badBank != null)
            {
                throw new MemoryConfigStateException(Address, Length, badBank.State?.Length ?? 0);
            }

            banks = bankConfigs.ToDictionary(x => x.BankId.Value,
                                             x =>
                                             {
                                                 var memory = new byte[Length];
                                                 Array.Copy(x.State, 0, memory, 0, Length);
                                                 return memory;
                                             });

            bank = banks[memoryBankController.RomBankNumber];

            memoryBankController.MemoryBankSwitch += MemoryBankControllerEventHandler;
        }

        public MemoryBankType Type => MemoryBankType.BankedReadOnlyMemory;

        public ushort Address { get; }

        public ushort Length { get; }

        public byte ReadByte(ushort address)
        {
            return bank[address];
        }

        public ushort ReadWord(ushort address)
        {
            // Construct 16 bit value in little endian.
            return BitConverter.ToUInt16(bank, address);
        }

        public byte[] ReadBytes(ushort address, int length)
        {
            var bytes = new byte[length];
            Array.Copy(bank, address, bytes, 0, length);
            return bytes;
        }

        public void ReadBytes(ushort address, byte[] buffer)
        {
            Array.Copy(bank, address, buffer, 0, buffer.Length);
        }

        public override string ToString()
        {
            return $"{Type}: 0x{Address:x4} - 0x{Address + Length - 1:x4}";
        }

        private void MemoryBankControllerEventHandler(object sender, MemoryBankControllerEventArgs args)
        {
            if (args.Target != MemoryBankControllerEventTarget.RomBankSwitch)
            {
                return;
            }

            bank = banks[memoryBankController.RomBankNumber];
        }
    }
}