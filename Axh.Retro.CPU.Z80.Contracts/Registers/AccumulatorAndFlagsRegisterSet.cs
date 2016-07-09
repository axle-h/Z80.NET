﻿using Axh.Retro.CPU.Common.Contracts.Util;
using Axh.Retro.CPU.Z80.Contracts.State;

namespace Axh.Retro.CPU.Z80.Contracts.Registers
{
    public class AccumulatorAndFlagsRegisterSet
    {
        public AccumulatorAndFlagsRegisterSet(IFlagsRegister flagsRegister)
        {
            Flags = flagsRegister;
        }

        public byte A { get; set; }

        public IFlagsRegister Flags { get; }

        public ushort AF
        {
            get { return BitConverterHelpers.To16Bit(A, Flags.Register); }
            set
            {
                var bytes = BitConverterHelpers.To8Bit(value);
                A = bytes[1];
                Flags.Register = bytes[0];
            }
        }

        public void Reset()
        {
            A = 0;
            Flags.ResetFlags();
        }

        public void ResetToState(AccumulatorAndFlagsRegisterState state)
        {
            A = state.A;
            Flags.Register = state.F;
        }

        public AccumulatorAndFlagsRegisterState GetRegisterState()
        {
            return new AccumulatorAndFlagsRegisterState { A = A, F = Flags.Register };
        }
    }
}