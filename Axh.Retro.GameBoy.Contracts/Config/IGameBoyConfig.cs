﻿namespace Axh.Retro.GameBoy.Contracts.Config
{
    public interface IGameBoyConfig
    {
        byte[] CartridgeData { get; }

        GameBoyType GameBoyType { get; }

        bool RunGpu { get; }

        bool UseGameBoyTimings { get; }
    }
}
