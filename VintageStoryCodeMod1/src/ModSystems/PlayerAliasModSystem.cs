using ProtoBuf;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.CommandAbbr;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;
using VintageStoryCodeMod1.src.Config;

namespace VintageStoryCodeMod1.src.ModSystems
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class AliasUpdate
    {
        public string Alias { get; set; }
        public string PlayerUUID { get; set; }
    }

    public class PlayerAliasModSystem : ModSystem
    {
        public override bool ShouldLoad(EnumAppSide forSide) => false;
        protected const string AliasChannel = "playeralias";

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.Network.RegisterChannel(AliasChannel).RegisterMessageType<AliasUpdate>();
        }
    }
}
