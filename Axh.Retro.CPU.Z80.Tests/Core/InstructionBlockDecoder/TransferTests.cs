﻿using Axh.Retro.CPU.Z80.Contracts.Config;
using Axh.Retro.CPU.Z80.Contracts.OpCodes;
using Axh.Retro.CPU.Z80.Contracts.Registers;
using Moq;
using NUnit.Framework;

namespace Axh.Retro.CPU.Z80.Tests.Core.InstructionBlockDecoder
{
    [TestFixture]
    public class TransferTests : InstructionBlockDecoderTestsBase<IZ80Registers>
    {
        public TransferTests() : base(CpuMode.Z80)
        {
        }

        [Test]
        public void LDD()
        {
            SetupRegisters();
            ResetMocks();

            RunWithHalt(4, 16, PrimaryOpCode.Prefix_ED, PrefixEdOpCode.LDD);

            GpRegisters.VerifySet(x => x.DE = DE - 1, Times.Once);
            GpRegisters.VerifySet(x => x.HL = HL - 1, Times.Once);
            GpRegisters.VerifySet(x => x.BC = BC - 1, Times.Once);

            Mmu.Verify(x => x.TransferByte(HL, DE), Times.Once);

            FlagsRegister.VerifySet(x => x.HalfCarry = false, Times.Once);
            FlagsRegister.VerifySet(x => x.ParityOverflow = true, Times.Once);
            FlagsRegister.VerifySet(x => x.Subtract = false, Times.Once);
        }


        [Test]
        public void LDD_BC0()
        {
            const ushort BC1 = 0x0001;

            SetupRegisters(BC1);
            ResetMocks();

            RunWithHalt(4, 16, PrimaryOpCode.Prefix_ED, PrefixEdOpCode.LDD);

            GpRegisters.VerifySet(x => x.DE = DE - 1, Times.Once);
            GpRegisters.VerifySet(x => x.HL = HL - 1, Times.Once);
            GpRegisters.VerifySet(x => x.BC = 0, Times.Once);

            Mmu.Verify(x => x.TransferByte(HL, DE), Times.Once);

            FlagsRegister.VerifySet(x => x.HalfCarry = false, Times.Once);
            FlagsRegister.VerifySet(x => x.ParityOverflow = false, Times.Once);
            FlagsRegister.VerifySet(x => x.Subtract = false, Times.Once);
        }

        [Test]
        public void LDDR()
        {
            const ushort Length = 0x0005;

            SetupRegisters(Length);
            ResetMocks();

            RunWithHalt((Length - 1) * 5 + 4, (Length - 1) * 21 + 16, PrimaryOpCode.Prefix_ED, PrefixEdOpCode.LDDR);

            for (var i = 1; i < Length; i++)
            {
                var index = i;
                GpRegisters.VerifySet(x => x.DE = (ushort) (DE - index), Times.Once);
                GpRegisters.VerifySet(x => x.HL = (ushort) (HL - index), Times.Once);
                GpRegisters.VerifySet(x => x.BC = (ushort) (Length - index), Times.Once);
            }

            for (var i = 0; i < Length; i++)
            {
                var index = i;
                Mmu.Verify(x => x.TransferByte((ushort) (HL - index), (ushort) (DE - index)), Times.Once);
            }

            FlagsRegister.VerifySet(x => x.HalfCarry = false, Times.Once);
            FlagsRegister.VerifySet(x => x.ParityOverflow = false, Times.Once);
            FlagsRegister.VerifySet(x => x.Subtract = false, Times.Once);
        }

        [Test]
        public void LDI()
        {
            SetupRegisters();
            ResetMocks();

            RunWithHalt(4, 16, PrimaryOpCode.Prefix_ED, PrefixEdOpCode.LDI);

            GpRegisters.VerifySet(x => x.DE = DE + 1, Times.Once);
            GpRegisters.VerifySet(x => x.HL = HL + 1, Times.Once);
            GpRegisters.VerifySet(x => x.BC = BC - 1, Times.Once);

            Mmu.Verify(x => x.TransferByte(HL, DE), Times.Once);

            FlagsRegister.VerifySet(x => x.HalfCarry = false, Times.Once);
            FlagsRegister.VerifySet(x => x.ParityOverflow = true, Times.Once);
            FlagsRegister.VerifySet(x => x.Subtract = false, Times.Once);
        }

        [Test]
        public void LDI_BC0()
        {
            const ushort BC1 = 0x0001;

            SetupRegisters(BC1);
            ResetMocks();

            RunWithHalt(4, 16, PrimaryOpCode.Prefix_ED, PrefixEdOpCode.LDI);

            GpRegisters.VerifySet(x => x.DE = DE + 1, Times.Once);
            GpRegisters.VerifySet(x => x.HL = HL + 1, Times.Once);
            GpRegisters.VerifySet(x => x.BC = 0, Times.Once);

            Mmu.Verify(x => x.TransferByte(HL, DE), Times.Once);

            FlagsRegister.VerifySet(x => x.HalfCarry = false, Times.Once);
            FlagsRegister.VerifySet(x => x.ParityOverflow = false, Times.Once);
            FlagsRegister.VerifySet(x => x.Subtract = false, Times.Once);
        }

        [Test]
        public void LDIR()
        {
            const ushort Length = 0x0005;

            SetupRegisters(Length);
            ResetMocks();

            RunWithHalt((Length - 1) * 5 + 4, (Length - 1) * 21 + 16, PrimaryOpCode.Prefix_ED, PrefixEdOpCode.LDIR);

            for (var i = 1; i < Length; i++)
            {
                var index = i;
                GpRegisters.VerifySet(x => x.DE = (ushort) (DE + index), Times.Once);
                GpRegisters.VerifySet(x => x.HL = (ushort) (HL + index), Times.Once);
                GpRegisters.VerifySet(x => x.BC = (ushort) (Length - index), Times.Once);
            }

            for (var i = 0; i < Length; i++)
            {
                var index = i;
                Mmu.Verify(x => x.TransferByte((ushort) (HL + index), (ushort) (DE + index)), Times.Once);
            }

            FlagsRegister.VerifySet(x => x.HalfCarry = false, Times.Once);
            FlagsRegister.VerifySet(x => x.ParityOverflow = false, Times.Once);
            FlagsRegister.VerifySet(x => x.Subtract = false, Times.Once);
        }
    }
}