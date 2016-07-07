﻿using System.Collections.Generic;
using System.Drawing;
using Axh.Retro.GameBoy.Contracts.Graphics;

namespace Axh.Retro.GameBoy.Contracts.Config
{
    /// <summary>
    /// GameBoy specific config.
    /// </summary>
    public interface IGameBoyConfig
    {
        /// <summary>
        /// Gets the cartridge data.
        /// </summary>
        /// <value>
        /// The cartridge data.
        /// </value>
        byte[] CartridgeData { get; }

        /// <summary>
        /// Gets the type of the game boy.
        /// </summary>
        /// <value>
        /// The type of the game boy.
        /// </value>
        GameBoyType GameBoyType { get; }

        /// <summary>
        /// Gets a value indicating whether [run gpu].
        /// </summary>
        /// <value>
        /// <c>true</c> if [run gpu]; otherwise, <c>false</c>.
        /// </value>
        bool RunGpu { get; }

        /// <summary>
        /// Gets a value indicating whether [use game boy timings].
        /// </summary>
        /// <value>
        /// <c>true</c> if [use game boy timings]; otherwise, <c>false</c>.
        /// </value>
        bool UseGameBoyTimings { get; }

        /// <summary>
        /// Gets the monocrome palette.
        /// </summary>
        /// <value>
        /// The monocrome palette.
        /// </value>
        IDictionary<MonocromeShade, Color> MonocromePalette { get; }
    }
}