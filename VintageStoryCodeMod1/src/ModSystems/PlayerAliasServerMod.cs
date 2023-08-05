using Vintagestory.API.Common;
using Vintagestory.API.Common.CommandAbbr;
using Vintagestory.API.Server;
using Vintagestory.Client.NoObf;
using VintageStoryCodeMod1.src.Config;

namespace VintageStoryCodeMod1.src.ModSystems
{
    internal class PlayerAliasServerMod : PlayerAliasModSystem
    {
        private PlayerAliasConfig config;

        private ICoreServerAPI sapi;

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide == EnumAppSide.Server;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            sapi = api;
            config = PlayerAliasConfig.Load(api);

            sapi.ChatCommands.GetOrCreate("playeralias")
                .RequiresPrivilege(Privilege.chat)
                .RequiresPlayer()
                .BeginSub("set_alias")
                .WithDescription("Sets the alias to your playername")
                .WithArgs(new StringArgParser("newAlias", true))
                .HandleWith(SetAlias);

            sapi.ChatCommands.GetOrCreate("playeralias")
                .RequiresPrivilege(Privilege.chat)
                .RequiresPlayer()
                .BeginSub("remove_alias")
                .WithDescription("Resets the players name back to the default")
                .HandleWith(RemoveAlias);

            sapi.Event.PlayerNowPlaying += EventOnPlayerNowPlaying;
        }

        private void EventOnPlayerNowPlaying(IServerPlayer byplayer)
        {
            sapi.Network.GetChannel(FirstChannel).SendPacket(new OriginalUpdate()
            {
                PlayerAliases = config.PlayerAliases
            }, byplayer);
        }

        private TextCommandResult SetAlias(TextCommandCallingArgs args)
        {
            var newAlias = args[0] as string;
            var player = args.Caller.Player as IServerPlayer;

            PlayerAliasData playerData;
            if (config.PlayerAliases.TryGetValue(player.PlayerUID, out playerData))
            {
                playerData.Alias = newAlias;
                playerData.IsSet = true;
            }
            else
            {
                playerData = new PlayerAliasData()
                {
                    Alias = newAlias,
                    OriginalName = player.PlayerName,
                    IsSet = true
                };
                config.PlayerAliases.Add(player.PlayerUID, playerData);
            }

            config.Save(sapi);

            sapi.Network.GetChannel(AliasChannel).SendPacket(new AliasUpdate()
            {
                Alias = newAlias,
                PlayerUUID = player.PlayerUID
            }, sapi.World.AllOnlinePlayers as IServerPlayer[]);

            //ClientSettings.PlayerName = newAlias;
            return TextCommandResult.Success("Alias set to " + newAlias);
        }

        private TextCommandResult RemoveAlias(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;

            if (player == null)
            {
                return TextCommandResult.Error("RemoveAlias caller is not a player");
            }

            if (!config.PlayerAliases.TryGetValue(player.PlayerUID, out var playerData))
                return TextCommandResult.Error("Player alias not found");

            playerData.IsSet = false;
            config.Save(sapi);

            sapi.Network.GetChannel(AliasChannel).SendPacket(new AliasUpdate()
            {
                Alias = playerData.OriginalName,
                PlayerUUID = player.PlayerUID
            }, sapi.World.AllOnlinePlayers as IServerPlayer[]);
            //ClientSettings.PlayerName = playerData.OriginalName;
            return TextCommandResult.Success("Alias reset to " + playerData.OriginalName);
        }
    }
}
