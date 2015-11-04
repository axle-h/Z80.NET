﻿namespace Axh.Retro.CPU.X80.Contracts.Config
{
    using System;

    public interface IRuntimeConfig
    {
        /// <summary>
        /// Maximum lifetime of an instruciton block cache item that is never accessed
        /// </summary>
        TimeSpan? InstructionCacheSlidingExpiration { get; }
    }
}