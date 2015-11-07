﻿namespace Axh.Retro.CPU.Z80.Contracts.Core
{
    using System.Threading.Tasks;

    using Axh.Retro.CPU.Z80.Contracts.Registers;

    public interface ICpuCore<TRegisters, TRegisterState>
        where TRegisters : IStateBackedRegisters<TRegisterState>
        where TRegisterState : struct
    {
        ICoreContext<TRegisters, TRegisterState> GetContext();

        Task StartCoreProcessAsync(ICoreContext<TRegisters, TRegisterState> context);
    }
}