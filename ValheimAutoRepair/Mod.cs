using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

/*
 * Valheim_0.217.31.0
 * [Message:   BepInEx] BepInEx 5.4.22.0 - Valheim (10.11.2023 21:18:15)
 * [Message:   BepInEx] User is running BepInExPack Valheim version 5.4.2202 from Thunderstore
 * [Info   :   BepInEx] Running under Unity v2022.3.12.5236448
 * [Info   :   BepInEx] CLR runtime version: 4.0.30319.42000
*/
namespace ValheimAutoRepair
{
    [BepInPlugin("darman1136.ValheimAutoRepair", "Valheim Auto Repair Mod", "1.0.0")]
    [BepInProcess("valheim.exe")]
    public class Mod : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("darman1136.ValheimAutoRepair");

        void Awake()
        {
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Show))]
        class AutoRepairPatch
        {

            static void Postfix(Button ___m_repairButton)
            {
                Debug.Log($"We've opened an menu");
                Player localPlayer = Player.m_localPlayer;
				string localPlayerName = localPlayer.GetPlayerName();

                if(localPlayer.GetCurrentCraftingStation())
                {
					Debug.Log($"Player is at a crafting station");

					List<ItemDrop.ItemData> tempWornItems = new List<ItemDrop.ItemData>();
                    Player.m_localPlayer.GetInventory().GetWornItems(tempWornItems);

					Debug.Log($"Checking for items to repair");
					int itemsToRepair = 0;
                    foreach (ItemDrop.ItemData tempWornItem in tempWornItems)
                    {
                        if (CanRepair(tempWornItem))
                        {
							itemsToRepair++;
						}
                    }

					Debug.Log($"Items to repair: {itemsToRepair}");
					for (int i = 0; i < itemsToRepair; i++)
                    {
						___m_repairButton.onClick.Invoke();
					}
					Debug.Log($"Repaired items");
				} else
                {
					Debug.Log($"Player not at a crafting station");
				}
            }

			/**
			 * Copied from InventoryGui.CanRepair
			 */
			private static bool CanRepair(ItemDrop.ItemData item)
			{
				if (Player.m_localPlayer == null)
				{
					return false;
				}
				if (!item.m_shared.m_canBeReparied)
				{
					return false;
				}
				if (Player.m_localPlayer.NoCostCheat())
				{
					return true;
				}
				CraftingStation currentCraftingStation = Player.m_localPlayer.GetCurrentCraftingStation();
				if (currentCraftingStation == null)
				{
					return false;
				}
				Recipe recipe = ObjectDB.instance.GetRecipe(item);
				if (recipe == null)
				{
					return false;
				}
				if (recipe.m_craftingStation == null && recipe.m_repairStation == null)
				{
					return false;
				}
				if ((recipe.m_repairStation != null && recipe.m_repairStation.m_name == currentCraftingStation.m_name) || (recipe.m_craftingStation != null && recipe.m_craftingStation.m_name == currentCraftingStation.m_name) || item.m_worldLevel < Game.m_worldLevel)
				{
					if (Mathf.Min(currentCraftingStation.GetLevel(), 4) < recipe.m_minStationLevel)
					{
						return false;
					}
					return true;
				}
				return false;
			}
		}
    }
}
