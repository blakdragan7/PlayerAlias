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
using VintageStoryCodeMod1.Utils;

namespace VintageStoryCodeMod1.src.ModSystems
{
    internal class PlayerAliasDataClient
    {
        public string PlayerId { get; set; }
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
            api.Network.GetChannel(AliasChannel).SetMessageHandler<AliasUpdate>(AliasUpdateReceived);
            api.Network.GetChannel(FirstChannel).SetMessageHandler<OriginalUpdate>(FirstUpdateReceived);
        }

        private void EventOnPlayerEntitySpawn(IClientPlayer player)
        {
            if (playerAliases.TryGetValue(player.PlayerUID, out var data))
            {
                if (data.IsSet)
                    UpdateAlias(data);
            }
            else
            {
                playerAliases.Add(player.PlayerUID, new PlayerAliasDataClient()
                {
                    PlayerId = player.PlayerUID,
                    OriginalName = player.PlayerName,
                    IsSet = false
                });
            }
        }

        private void UpdateAlias(PlayerAliasDataClient data)
        {
            MainThreadTimer.Dispatch(capi, deltaTime =>
            {
                var player = GetClientPlayer(data.PlayerId);
                var watchedAttributes = player?.Entity?.WatchedAttributes;

                if (watchedAttributes == null)
                {
                    capi.Logger.Warning($"Could not get player watched attribute when updating player {player}");
                    return true;
                }

                var attribute = watchedAttributes.GetTreeAttribute("nametag");
                if (attribute == null)
                {
                    capi.Logger.Warning($"Could not get player attribute when updating player {player}");
                    return true;
                }

                attribute.SetString("name", data.Alias);
                watchedAttributes.MarkPathDirty("nametag");

                return false;
            }, 100);
        }

        private void AliasUpdateReceived(AliasUpdate update)
        {
            PlayerAliasDataClient data;
            playerAliases.TryGetValue(update.PlayerUUID, out data);
            var player = GetClientPlayer(update.PlayerUUID);

            if (player == null)
            {
                capi.Logger.Error($"Could not find player to update {update.PlayerUUID}");
            }
            
            // there has to be a better way to do this but this is good enough
            if (data != null)
            {
                data.IsSet = player.PlayerName != update.Alias;
                data.Alias = update.Alias;
            }
            else
            {
                data = new PlayerAliasDataClient()
                {
                    Alias = update.Alias,
                    PlayerId = player.PlayerUID,
                    IsSet = player.PlayerName != update.Alias
                };
            }

            playerAliases[update.PlayerUUID] = data;
            UpdateAlias(data);
        }

        private IClientPlayer GetClientPlayer(string uuid)
        {
            return (from player in capi.World.AllOnlinePlayers where player.PlayerUID == uuid select player as IClientPlayer).FirstOrDefault();
        }

        private void FirstUpdateReceived(OriginalUpdate update)
        {
            if (update?.PlayerAliases == null)
                return;

            MainThreadTimer.Dispatch(capi,deltaTime =>
            {
                // we wait for the world to be started
                if (capi.World?.AllOnlinePlayers == null)
                    return true;

                foreach (var player in capi.World.AllOnlinePlayers)
                {
                    update.PlayerAliases.TryGetValue(player.PlayerUID, out var data);

                    if (!playerAliases.TryGetValue(player.PlayerUID, out var clientData))
                    {
                        clientData = new PlayerAliasDataClient()
                        {
                            PlayerId = player.PlayerUID,
                        };
                    }

                    clientData.Alias = data?.Alias;
                    clientData.IsSet = data?.IsSet ?? false;

                    playerAliases[player.PlayerUID] = clientData;
                    UpdateAlias(clientData);
                }

                return false;
            }, 100);
        }
    }
}
