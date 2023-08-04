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
using VintageStoryCodeMod1.src.Config;

namespace VintageStoryCodeMod1.src.ModSystems
{
    internal class PlayerAliasClientMod : PlayerAliasModSystem
    {
        private ICoreClientAPI capi;

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide == EnumAppSide.Client;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            capi = api;
            api.Network.GetChannel(AliasChannel).SetMessageHandler<AliasUpdate>(AliasUpdate);
        }

        private void UpdateAlias(IClientPlayer byplayer, string alias)
        {
            if (byplayer == null) return;

            var clientPlayer = byplayer as ClientPlayer;
            var watchedAttributes = clientPlayer?.Entity?.WatchedAttributes;

            if (watchedAttributes == null)
            {
                capi.Logger.Error($"Could not get player watched attribute when updating player {byplayer}");
                return;
            }

            var attribute = watchedAttributes.GetTreeAttribute("nametag");
            if (attribute == null)
            {
                capi.Logger.Error($"Could not get player attribute when updating player {byplayer}");
                return;
            }

            attribute.SetString("name", alias);
            watchedAttributes.MarkPathDirty("nametag");
        }

        private void AliasUpdate(AliasUpdate update)
        {
            // there has to be a better way to do this but this is good enough
            foreach (var player in capi.World.AllOnlinePlayers)
            {
                if (player.PlayerUID == update.PlayerUUID)
                {
                    UpdateAlias(player as IClientPlayer, update.Alias);
                    break;
                }
            }
        }
    }
}
