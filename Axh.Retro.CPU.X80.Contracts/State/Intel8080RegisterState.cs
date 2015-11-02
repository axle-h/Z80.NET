﻿namespace Axh.Retro.CPU.X80.Contracts.State
{
    using Axh.Retro.CPU.X80.Contracts.Registers;

    public struct Intel8080RegisterState
    {
        public GeneralPurposeRegisterState GeneralPurposeRegisterState { get; set; }

        public AccumulatorAndFlagsRegisterState AccumulatorAndFlagsRegisterState { get; set; }

        public ushort StackPointer { get; set; }
        public ushort ProgramCounter { get; set; }

        //Interrupt Flip-Flops
        public bool InterruptFlipFlop1 { get; set; }
        public bool InterruptFlipFlop2 { get; set; }
        public InterruptMode InterruptMode { get; set; }
    }
}
