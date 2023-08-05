using ProtoBuf;
using System.Collections.Generic;
using Vintagestory.API.Common;
using VintageStoryCodeMod1.src.Config;
using VintageStoryCodeMod1.src.ModSystems;

namespace VintageStoryCodeMod1.src.ModSystems
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class AliasUpdate
    {
        public string Alias { get; set; }
        public string PlayerUUID { get; set; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class OriginalUpdate
    {
        public Dictionary<string, PlayerAliasData> PlayerAliases { get; set; }
    };

    public class PlayerAliasModSystem : ModSystem
    {
        public override bool ShouldLoad(EnumAppSide forSide) => false;
        protected const string AliasChannel = "playeralias";
        protected const string FirstChannel = "playeraliasf";

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.Network.RegisterChannel(AliasChannel).RegisterMessageType<AliasUpdate>();
            api.Network.RegisterChannel(FirstChannel).RegisterMessageType<OriginalUpdate>();
        }
    }
}
