﻿using BepInEx;
using LoveMachine.Core;
using LoveMachine.Core.NonPortable;

namespace LoveMachine.DJ
{
    [BepInPlugin(Globals.GUID, Globals.PluginName, Globals.Version)]
    internal class Plugin : LoveMachinePlugin<DatsuiJankenGame>
    { }
}