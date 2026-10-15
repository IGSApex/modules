// Integrated from DrakiaXYZ/SPT-SearchOpenContainers 1.5.0 (MIT).
// Copyright (c) 2023-2026 DrakiaXYZ.
// SPDX-License-Identifier: MIT
using EFT;
using EFT.Interactive;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SPT.Custom.Patches;

internal class SearchOpenContainersPatch : ModulePatch
{
    private static readonly InteractionResult OpenInteraction = new InteractionResult(EInteractionType.Open);

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.GetDeclaredMethods(typeof(InteractionContextHelper)).FirstOrDefault(method =>
        {
            var parameters = method.GetParameters();
            return parameters.Length == 2
                && parameters[0].Name == "owner"
                && parameters[1].Name == "container";
        });
    }

    [PatchPostfix]
    public static void PatchPostfix(ref AvailableInteractionState __result, GamePlayerOwner owner, LootableContainer container)
    {
        if (__result == null || container.DoorState != EDoorState.Open)
        {
            return;
        }

        var handler = new SearchActionHandler
        {
            Owner = owner,
            Container = container,
            InitialDistance = Vector3.Distance(owner.Player.Transform.position, container.transform.position),
        };
        __result.Actions.Insert(0, new InteractionAction
        {
            Name = "Search".Localized(),
            Action = handler.StartOpenContainer,
        });
    }

    private sealed class SearchActionHandler
    {
        internal GamePlayerOwner Owner;
        internal LootableContainer Container;
        internal float InitialDistance;

        internal void StartOpenContainer()
        {
            Owner.Player.ExecuteInteraction(Container, OpenInteraction);
            Owner.Player.SetCallbackForInteraction(new Action<Action>(OpenContainerCallback));
            Owner.Player.TryInteractionCallback(Container);
        }

        private void OpenContainerCallback(Action callback)
        {
            InteractionContextHelper.OnContainerOpen(Owner, callback, Container, InitialDistance);
        }
    }
}
