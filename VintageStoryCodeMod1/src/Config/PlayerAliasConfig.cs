using System;
using System.Collections.Generic;
using ProtoBuf;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace VintageStoryCodeMod1.src.Config
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PlayerAliasData
    {
        public string OriginalName { get; set; }
        public string Alias { get; set; }
        public bool IsSet { get; set; }
    }

    public class PlayerAliasConfig
    {
        public Dictionary<string, PlayerAliasData> PlayerAliases = new Dictionary<string, PlayerAliasData>();
        private const string ConfigFileName = "playeralias.json";
        public static PlayerAliasConfig Load(ICoreServerAPI api)
        {
            try
            {
                return api.LoadModConfig<PlayerAliasConfig>(ConfigFileName) ?? new PlayerAliasConfig();
            }
            catch (Exception e)
            {
                api.Logger.Error($"Failed to load config {e}");
                return new PlayerAliasConfig();
            }
        }

        public void Save(ICoreServerAPI api)
        {
            api.StoreModConfig(this, ConfigFileName);
        }
    }
}
