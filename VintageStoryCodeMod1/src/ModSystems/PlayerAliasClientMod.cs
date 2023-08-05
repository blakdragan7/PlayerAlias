using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;
using VintageStoryCodeMod1.src.Config;

namespace VintageStoryCodeMod1.src.ModSystems
{
    internal class PlayerAliasDataClient
    {
        public IClientPlayer Player { get; set; }
        public string OriginalName { get; set; }
        public string Alias { get; set; }
        public bool IsSet { get; set; }
    }

    internal class PlayerAliasClientMod : PlayerAliasModSystem
    {
        private ICoreClientAPI capi;

        private Dictionary<string, PlayerAliasDataClient> playerAliases = new Dictionary<string, PlayerAliasDataClient>();

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide == EnumAppSide.Client;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            capi = api;
            capi.Event.PlayerEntitySpawn += EventOnPlayerEntitySpawn;
            api.Network.GetChannel(AliasChannel).SetMessageHandler<AliasUpdate>(AliasUpdate);
            api.Network.GetChannel(FirstChannel).SetMessageHandler<OriginalUpdate>(FirstUpdate);
        }

        private void EventOnPlayerEntitySpawn(IClientPlayer byplayer)
        {
            if (playerAliases.TryGetValue(byplayer.PlayerUID, out var data))
            {
                data.OriginalName = byplayer.PlayerName;

                if (data.IsSet)
                    UpdateAlias(data);
            }
            else
            {
                playerAliases.Add(byplayer.PlayerUID, new PlayerAliasDataClient()
                {
                    Player = byplayer,
                    OriginalName = byplayer.PlayerName,
                    IsSet = false
                });
            }
        }

        private async void UpdateAlias(PlayerAliasDataClient data)
        {
            await Task.Delay(1000);
            capi.Event.EnqueueMainThreadTask(() =>
            {
                var watchedAttributes = data.Player?.Entity?.WatchedAttributes;

                if (watchedAttributes == null)
                {
                    capi.Logger.Error($"Could not get player watched attribute when updating player {data.Player}");
                    return;
                }

                var attribute = watchedAttributes.GetTreeAttribute("nametag");
                if (attribute == null)
                {
                    capi.Logger.Error($"Could not get player attribute when updating player {data.Player}");
                    return;
                }

                attribute.SetString("name", data.Alias);
                watchedAttributes.MarkPathDirty("nametag");
            }, "update Alias");
        }

        private void AliasUpdate(AliasUpdate update)
        {
            PlayerAliasDataClient data;
            playerAliases.TryGetValue(update.PlayerUUID, out data);
            // there has to be a better way to do this but this is good enough
            if (data != null)
            {
                data.IsSet = data.Player.PlayerName != update.Alias;
                data.Alias = update.Alias;
            }
            else
            {
                var player = GetClientPlayer(update.PlayerUUID);
                data = new PlayerAliasDataClient()
                {
                    Alias = update.Alias,
                    Player = player,
                    IsSet = player.PlayerName != update.Alias
                };

                if (player == null)
                {
                    capi.Logger.Error("Could not find player to update alias for");
                }
            }

            playerAliases[update.PlayerUUID] = data;
            UpdateAlias(data);
        }

        private IClientPlayer GetClientPlayer(string uuid)
        {
            return (from player in capi.World.AllOnlinePlayers where player.PlayerUID == uuid select player as IClientPlayer).FirstOrDefault();
        }

        private void FirstUpdate(OriginalUpdate update)
        {
            foreach (var player in capi.World.AllOnlinePlayers)
            {
                update.PlayerAliases.TryGetValue(player.PlayerUID, out var data);

                PlayerAliasDataClient clientData;
                if (!playerAliases.TryGetValue(player.PlayerUID, out clientData))
                {
                    clientData = new PlayerAliasDataClient()
                    {
                        Player = player as IClientPlayer,
                    };
                }

                clientData.Alias = data?.Alias;
                clientData.IsSet = data?.IsSet ?? false;

                playerAliases[player.PlayerUID] = clientData;
                UpdateAlias(clientData);
            }
        }
    }
}
