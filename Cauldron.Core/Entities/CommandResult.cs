﻿using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.Entities
{
    class CommandResult
    {
        public static CommandResult Success() => new CommandResult() { IsSucceeded = true };

        public bool IsSucceeded { get; set; } = false;

        public string ErrorMessage { get; set; } = "";


        public GameContext GameEnvironment { get; set; }
    }
}
