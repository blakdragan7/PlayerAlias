using Vintagestory.API.Common;
using Vintagestory.API.Common.CommandAbbr;
using Vintagestory.API.Server;

[assembly: ModInfo(name: "Remove Other Player Pins", modID: "hidemapplayers",
    Side = "Server",
    Authors = new[] { "blakdragan7" },
    Description = "Removes other players from the worldmap and minimap.",
    Version = "1.0.0")]

namespace HideMapPlayers
{
    public class HideMapPlayers : ModSystem
    {
        private ICoreServerAPI sapi;
        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide == EnumAppSide.Server;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            sapi = api;

            api.ChatCommands.GetOrCreate("mapHideOtherPlayers")
                .RequiresPrivilege(Privilege.commandplayer)
                .WithDescription("controls if players are shown on the map")
                .WithArgs(new BoolArgParser("isOn", "true", true))
                .HandleWith(OnMapHideOtherPlayersSet);
        }

        private TextCommandResult OnMapHideOtherPlayersSet(TextCommandCallingArgs args)
        {
            var shouldBeOn = (bool)args[0];

            sapi.World.Config.SetBool("mapHideOtherPlayers", shouldBeOn);
            return TextCommandResult.Success();
        }
    }
}
