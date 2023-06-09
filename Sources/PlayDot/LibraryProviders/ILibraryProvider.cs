﻿using System.Collections.Generic;
using System.Drawing;

namespace PlayDot.LibraryProviders
{
    internal interface ILibraryProvider
    {
        public Color BrandColor { get; }
        public IEnumerable<GameDescriptor> CollectInstalledGames();
        public void RunGame(GameDescriptor game);
    }
}
