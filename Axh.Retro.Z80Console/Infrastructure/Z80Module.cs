﻿namespace Axh.Retro.Z80Console.Infrastructure
{
    using Axh.Retro.CPU.Z80.Cache;
    using Axh.Retro.CPU.Z80.Contracts.Cache;
    using Axh.Retro.CPU.Z80.Contracts.Config;
    using Axh.Retro.CPU.Z80.Contracts.Core;
    using Axh.Retro.CPU.Z80.Contracts.Core.Timing;
    using Axh.Retro.CPU.Z80.Contracts.Factories;
    using Axh.Retro.CPU.Z80.Contracts.Registers;
    using Axh.Retro.CPU.Z80.Contracts.State;
    using Axh.Retro.CPU.Z80.Core;
    using Axh.Retro.CPU.Z80.Core.DynaRec;
    using Axh.Retro.CPU.Z80.Core.Timing;
    using Axh.Retro.CPU.Z80.Factories;
    using Axh.Retro.Z80Console.Config;
    using Axh.Retro.Z80Console.Factories;

    using Ninject.Modules;

    internal class Z80Module : NinjectModule
    {
        public override void Load()
        {
            // Core
            this.Bind<ICpuCore<IZ80Registers, Z80RegisterState>>().To<CachingCpuCore<IZ80Registers, Z80RegisterState>>();
            this.Bind<IInstructionBlockDecoder<IZ80Registers>>().To<DynaRecInstructionBlockDecoder<IZ80Registers>>();
            
            // Factories
            this.Bind<IPeripheralFactory>().To<PeripheralFactory>().InSingletonScope();
            this.Bind<ICoreContextFactory<IZ80Registers, Z80RegisterState>>().To<Z80ContextFactory>().InSingletonScope();

            // Config
            this.Bind<IInitialStateConfig<Z80RegisterState>>().To<Z80InitialStateConfig>().InSingletonScope();
            this.Bind<IPlatformConfig>().To<Z8064KBootstrappedConfig>().InSingletonScope();
            this.Bind<IRuntimeConfig>().To<RuntimeConfig>().InSingletonScope();
        }
    }
}
