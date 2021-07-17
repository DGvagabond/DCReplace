using MEC;
using System.Linq;
using System;
using CISpy.API;
using System.Collections.Generic;
using Exiled.Events.EventArgs;
using Exiled.API.Features;
using Exiled.API.Enums;
using Exiled.Loader;

namespace DCReplace
{
	class EventHandlers
	{
		private bool isContain106;
		private bool isRoundStarted;

		private List<Player> TryGetSH() => SerpentsHand.API.GetSHPlayers();

		private Dictionary<ReferenceHub, bool> TryGetSpies() =>  SpyData.GetSpies();

		private void TrySpawnSpy(Player player, ReferenceHub dc, Dictionary<ReferenceHub, bool> spies) => SpyData.MakeSpy(player.ReferenceHub, spies[dc], false);

		private void TrySpawnSH(Player player) => SerpentsHand.API.SpawnPlayer(player, false);

		private void TrySpawn035(Player player) => Scp035.API.Spawn035(player);

		public void OnRoundStart()
		{
			isContain106 = false;
			isRoundStarted = true;
		}

		public void OnRoundEnd() => isRoundStarted = false;

		public void OnContain106(ContainingEventArgs ev) => isContain106 = true;

		public void OnPlayerDestroying(DestroyingEventArgs ev)
		{
			if (!isRoundStarted || ev.Player.Role == RoleType.Spectator || ev.Player.IsInPocketDimension || ev.Player.CurrentRoom.Zone == ZoneType.LightContainment && Map.IsLCZDecontaminated) return;

			var is035 = false;
			var isSH = false;
			if (isContain106 && ev.Player.Role == RoleType.Scp106) return;
			Dictionary<ReferenceHub, bool> spies = null;
			var role = Loader.Plugins.FirstOrDefault(pl => pl.Name == "EasyEvents")?.Assembly.GetType("EasyEvents.Util")?.GetMethod("GetRole")?.Invoke(null, new object[] {ev.Player});
			try
			{
				is035 = Scp035.API.IsScp035(ev.Player);
			}
			catch (Exception x)
			{
				Log.Debug("SCP-035 is not installed, skipping method call...");
			}

			try
			{
				isSH = TryGetSH().Contains(ev.Player);
			}
			catch (Exception x)
			{
				Log.Debug("Serpents Hand is not installed, skipping method call...");
			}

			try
			{
				spies = TryGetSpies();
			}
			catch (Exception x)
			{
				Log.Debug("CISpy is not installed, skipping method call...");
			}
			
			var player = Player.List.FirstOrDefault(x => x.Role == RoleType.Spectator && x.UserId != string.Empty && x.UserId != ev.Player.UserId && !x.IsOverwatchEnabled);
			if (player != null)
			{
				if (isSH)
				{
					try
					{
						TrySpawnSH(player);
					}
					catch (Exception x)
					{
						Log.Debug("Serpents Hand is not installed, skipping method call...");
					}
				}
				else player.SetRole(ev.Player.Role);
				if (spies != null && spies.ContainsKey(ev.Player.ReferenceHub))
				{
					try
					{
						TrySpawnSpy(player, ev.Player.ReferenceHub, spies);
					}
					catch (Exception x)
					{
						Log.Debug("CISpy is not installed, skipping method call...");
					}
				}
				if (is035)
				{
					try
					{
						TrySpawn035(player);
					}
					catch (Exception x)
					{
						Log.Debug("SCP-035 is not installed, skipping method call...");
					}
				}

				// save info
				var pos = ev.Player.Position;
				var inventory = ev.Player.Inventory.items.Select(x => x.id).ToList();
				var health = ev.Player.Health;
				var ammo1 = ev.Player.Ammo[(int)AmmoType.Nato556];
				var ammo2 = ev.Player.Ammo[(int)AmmoType.Nato762];
				var ammo3 = ev.Player.Ammo[(int)AmmoType.Nato9];

				Timing.CallDelayed(0.3f, () =>
				{
					player.Position = pos;
					player.ClearInventory();
					player.ResetInventory(inventory);
					player.Health = health;
					player.Ammo[(int)AmmoType.Nato556] = ammo1;
					player.Ammo[(int)AmmoType.Nato762] = ammo2;
					player.Ammo[(int)AmmoType.Nato9] = ammo3;
					player.Broadcast(5, "<i>You have replaced a player who has disconnected.</i>");
					if(role != null) Loader.Plugins.FirstOrDefault(pl => pl.Name == "EasyEvents")?.Assembly.GetType("EasyEvents.CustomRoles")?.GetMethod("ChangeRole")?.Invoke(null, new object[] {player, role});
				});
			}
		}
	}
}
