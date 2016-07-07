using Axh.Retro.CPU.Common.Contracts.Memory;
using Axh.Retro.CPU.Common.Contracts.Timing;
using Axh.Retro.CPU.Z80.Contracts.Peripherals;
using Axh.Retro.CPU.Z80.Contracts.Registers;

namespace Axh.Retro.CPU.Z80.Contracts.Core
{
    public interface IInstructionBlock<in TRegisters> where TRegisters : IRegisters
    {
        ushort Address { get; }

        ushort Length { get; }

        bool HaltCpu { get; }

        bool HaltPeripherals { get; }

        string DebugInfo { get; }

        InstructionTimings ExecuteInstructionBlock(TRegisters registers, IMmu mmu, IAlu alu, IPeripheralManager peripheralManager);
    }
}