using System;
using System.Collections.Generic;
using System.Text;
using Facepunch;
using Newtonsoft.Json;
using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Group Limits", "misticos", "3.0.0")]
    [Description("Prevent rulebreakers from breaking group limits on your server and notify your staff")]
    class GroupLimits : CovalencePlugin
    {
        #region Variables
        
        [PluginReference]
        // ReSharper disable once InconsistentNaming
        private Plugin DiscordMessages = null;

        private const string PermissionIgnore = "grouplimits.ignore";

        private static GroupLimits _ins;
        
        #endregion
        
        #region Configuration

        private Configuration _config;

        private class Configuration
        {
            [JsonProperty(PropertyName = "Limits", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<Limit> Limits = new List<Limit> {new Limit()};

            [JsonProperty(PropertyName = "Log Format")]
            public string LogFormat = "[{time}] {id} ({name}) authorized on {shortname}/{entid} ({type}) at ({position})";
            
            public class Limit
            {
                [JsonProperty(PropertyName = "Type Name")]
                public string Name = "Any";

                [JsonProperty(PropertyName = "Max Authorized")]
                public int MaxAuthorized = 3;
                
                [JsonProperty(PropertyName = "Shortnames", ObjectCreationHandling = ObjectCreationHandling.Replace)]
                public List<string> Shortnames = new List<string> {"global"};
                
                [JsonProperty(PropertyName = "Disable For Decaying Structures")]
                public bool NoDecaying = true;
                
                [JsonProperty(PropertyName = "Notify Player")]
                public bool NotifyPlayer = true;
                
                [JsonProperty(PropertyName = "Notify Owner")]
                public bool NotifyOwner = true;
                
                [JsonProperty(PropertyName = "Enforce")]
                public bool Enforce = false;
                
                [JsonProperty(PropertyName = "Deauthorize")]
                public bool Deauthorize = true;
                
                [JsonProperty(PropertyName = "Deauthorize All")]
                public bool DeauthorizeAll = false;
                
                [JsonProperty(PropertyName = "Discord")]
                public Discord Webhook = new Discord();

                [JsonProperty(PropertyName = "Log To File")]
                public bool File = false;

                public static Limit Find(string shortname)
                {
                    var cLimit = (Limit) null;
                    foreach (var limit in _ins._config.Limits)
                    {
                        if (limit.Shortnames.Contains("global"))
                            cLimit = limit;

                        if (limit.Shortnames.Contains(shortname))
                            return limit;
                    }

                    return cLimit;
                }

                public class Discord
                {
                    [JsonProperty(PropertyName = "Webhook")]
                    public string Webhook = string.Empty;

                    [JsonProperty(PropertyName = "Inline")]
                    public bool Inline = true;
                    
                    [JsonProperty(PropertyName = "Title")]
                    public string Title = "Group Limit: Exceeded or deauthorized";
                    
                    [JsonProperty(PropertyName = "Color")]
                    public int Color = 0;
                    
                    [JsonProperty(PropertyName = "Player Title")]
                    public string PlayerTitle = "Player";
                    
                    [JsonProperty(PropertyName = "Player")]
                    public string Player = "{name}/{id}";
                    
                    [JsonProperty(PropertyName = "Entity Title")]
                    public string EntityTitle = "Entity";
                    
                    [JsonProperty(PropertyName = "Entity")]
                    public string Entity = "{shortname}/{id} ({type})";
                    
                    [JsonProperty(PropertyName = "Position Title")]
                    public string PositionTitle = "Position";
                    
                    [JsonProperty(PropertyName = "Position")]
                    public string Position = "teleportpos {position}";
                }
            }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                _config = Config.ReadObject<Configuration>();
                if (_config == null) throw new Exception();
                SaveConfig();
            }
            catch
            {
                PrintError("Your configuration file contains an error. Using default configuration values.");
                LoadDefaultConfig();
            }
        }

        protected override void SaveConfig() => Config.WriteObject(_config);

        protected override void LoadDefaultConfig() => _config = new Configuration();

        #endregion
        
        #region Hooks

        private void Init()
        {
            _ins = this;
            
            permission.RegisterPermission(PermissionIgnore, this);
        }

        private void Unload()
        {
            _ins = null;
        }

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                {"Notify: Player", "You are trying to exceed the group limit on our server."},
                {"Notify: Owner", "{name} tried to exceed the group limit on your entity at {position}. (Type: {type})"},
                {"Notify: Deauthorize Player", "One person was deauthorized, try to authorize again if you were not."},
                {"Notify: Deauthorize Owner", "{name} tried to authorize on your entity. One person was deauthorized on your entity at {position}. (Type: {type})"}
            }, this);
        }

        private object OnCupboardAuthorize(BuildingPrivlidge privilege, BasePlayer player)
        {
            if (player.IPlayer.HasPermission(PermissionIgnore))
                return null;
            
            var limit = Configuration.Limit.Find(privilege.ShortPrefabName);
            if (limit == null)
                return null;

            // Ensure there are no duplicates
            var alreadyAuthed = Pool.GetList<ulong>();
            for (var i = privilege.authorizedPlayers.Count - 1; i >= 0; i--)
            {
                var authed = privilege.authorizedPlayers[i];
                if (!alreadyAuthed.Contains(authed.userid))
                {
                    alreadyAuthed.Add(authed.userid);
                    continue;
                }
                
                privilege.authorizedPlayers.RemoveAt(i);
            }
            Pool.FreeList(ref alreadyAuthed);

            if (privilege.authorizedPlayers.Count < limit.MaxAuthorized)
                return null;

            if (limit.NoDecaying && IsDecaying(privilege))
                return null;

            if (limit.Deauthorize)
            {
                if (privilege.authorizedPlayers.Count > 0)
                {
                    if (limit.DeauthorizeAll)
                        privilege.authorizedPlayers.Clear();
                    else
                        privilege.authorizedPlayers.RemoveAt(0);
                    
                    privilege.SendNetworkUpdate();
                }

                NotifyDeauthorize(limit, privilege, player);
            }
            else
            {
                NotifyAuthorize(limit, privilege, player);
            }

            if (limit.Enforce)
                return true;

            return null;
        }

        private object OnCodeEntered(CodeLock codeLock, BasePlayer player, string code)
        {
            if (player.IPlayer.HasPermission(PermissionIgnore))
                return null;
            
            var isCodeAdmin = codeLock.code == code;
            var isCodeGuest = codeLock.guestCode == code;
            if (!isCodeAdmin && !isCodeGuest)
                return null;

            var limit = Configuration.Limit.Find(codeLock.ShortPrefabName);
            if (limit == null)
                return null;
            
            var total = codeLock.guestPlayers.Count + codeLock.whitelistPlayers.Count;
            if (total < limit.MaxAuthorized) 
                return null;
            
            var entity = codeLock.GetParentEntity();
            if (entity == null || !entity.IsValid())
                return null;


            if (limit.NoDecaying && IsDecaying(entity.GetBuildingPrivilege()))
                return null;

            if (limit.Deauthorize)
            {
                if (isCodeAdmin && codeLock.whitelistPlayers.Count > 0)
                {
                    if (limit.DeauthorizeAll)
                        codeLock.whitelistPlayers.Clear();
                    else
                        codeLock.whitelistPlayers.RemoveAt(0);
                    
                    codeLock.SendNetworkUpdate();
                }

                if (isCodeGuest && codeLock.guestPlayers.Count > 0)
                {
                    if (limit.DeauthorizeAll)
                        codeLock.guestPlayers.Clear();
                    else
                        codeLock.guestPlayers.RemoveAt(0);
                    
                    codeLock.SendNetworkUpdate();
                }

                NotifyDeauthorize(limit, entity, player);
            }
            else
            {
                NotifyAuthorize(limit, entity, player);
            }

            if (limit.Enforce)
                return true;
            
            return null;
        }

        private object OnTurretAuthorize(AutoTurret turret, BasePlayer player)
        {
            if (player.IPlayer.HasPermission(PermissionIgnore))
                return null;
            
            var limit = Configuration.Limit.Find(turret.ShortPrefabName);
            if (limit == null || turret.authorizedPlayers.Count < limit.MaxAuthorized)
                return null;

            if (limit.NoDecaying && IsDecaying(turret.GetBuildingPrivilege()))
                return null;

            if (limit.Deauthorize)
            {
                if (turret.authorizedPlayers.Count > 0)
                {
                    if (limit.DeauthorizeAll)
                        turret.authorizedPlayers.Clear();
                    else
                        turret.authorizedPlayers.RemoveAt(0);
                }

                NotifyDeauthorize(limit, turret, player);
            }
            else
            {
                NotifyAuthorize(limit, turret, player);
            }

            if (limit.Enforce)
                return true;
            
            return null;
        }
        
        #endregion
        
        #region Helpers

        private void NotifyAuthorize(Configuration.Limit limit, BaseEntity entity, BasePlayer basePlayer)
        {
            if (limit.NotifyPlayer)
            {
                var player = basePlayer?.IPlayer;
                if (player != null && player.IsConnected)
                {
                    player.Message(GetMsg("Notify: Player", player.Id));
                }
            }

            if (limit.NotifyOwner)
            {
                var player = players.FindPlayerById(entity.OwnerID.ToString());
                if (player != null && player.IsConnected)
                {
                    var sb = new StringBuilder(GetMsg("Notify: Owner", player.Id));
                    sb.Replace("{position}", entity.transform.position.ToString());
                    sb.Replace("{type}", limit.Name);
                    sb.Replace("{name}", basePlayer?.displayName ?? "Unknown");

                    player.Message(sb.ToString());
                }
            }
            
            NotifyLog(limit, entity, basePlayer);
            NotifyDiscord(limit, entity, basePlayer);
        }

        private void NotifyDeauthorize(Configuration.Limit limit, BaseEntity entity, BasePlayer basePlayer)
        {
            if (limit.NotifyPlayer)
            {
                var player = basePlayer?.IPlayer;
                if (player != null && player.IsConnected)
                {
                    player.Message(GetMsg("Notify: Deauthorize Player", player.Id));
                }
            }

            if (limit.NotifyOwner)
            {
                var player = players.FindPlayerById(entity.OwnerID.ToString());
                if (player != null && player.IsConnected)
                {
                    var sb = new StringBuilder(GetMsg("Notify: Deauthorize Owner", player.Id));
                    sb.Replace("{position}", entity.transform.position.ToString());
                    sb.Replace("{type}", limit.Name);
                    sb.Replace("{name}", basePlayer?.displayName ?? "Unknown");
                    
                    player.Message(sb.ToString());
                }
            }
            
            NotifyLog(limit, entity, basePlayer);
            NotifyDiscord(limit, entity, basePlayer);
        }

        private void NotifyLog(Configuration.Limit limit, BaseNetworkable entity, BasePlayer player)
        {
            if (!limit.File)
                return;

            var builder = new StringBuilder(_config.LogFormat);
            builder.Replace("{time}", DateTime.Now.ToLongTimeString());
            builder.Replace("{name}", player?.displayName ?? "Unknown");
            builder.Replace("{id}", player?.UserIDString ?? "0");
            builder.Replace("{shortname}", entity.ShortPrefabName);
            builder.Replace("{entid}", entity.net.ID.ToString());
            builder.Replace("{type}", limit.Name);
            builder.Replace("{position}", FormattedCoordinates(entity.transform.position));

            LogToFile("Log", builder.ToString(), this);
        }

        private void NotifyDiscord(Configuration.Limit limit, BaseNetworkable entity, BasePlayer player)
        {
            var discord = limit.Webhook;
            if (string.IsNullOrEmpty(discord.Webhook) || DiscordMessages == null || !DiscordMessages.IsLoaded)
                return;

            object fields = new[]
            {
                new
                {
                    name = discord.PlayerTitle,
                    value = discord.Player.Replace("{name}", player?.displayName ?? "Unknown")
                        .Replace("{id}", player?.UserIDString ?? "0"),
                    inline = discord.Inline
                },
                new
                {
                    name = discord.EntityTitle,
                    value = discord.Entity.Replace("{shortname}", entity.ShortPrefabName)
                        .Replace("{id}", entity.net.ID.ToString()).Replace("{type}", limit.Name),
                    inline = discord.Inline
                },
                new
                {
                    name = discord.PositionTitle,
                    value = discord.Position.Replace("{position}", FormattedCoordinates(entity.transform.position)),
                    inline = discord.Inline
                }
            };
            
            DiscordMessages.CallHook("API_SendFancyMessage", discord.Webhook, discord.Title, discord.Color, JsonConvert.SerializeObject(fields));
        }
        
        private string FormattedCoordinates(Vector3 pos) => $"{pos.x},{pos.y},{pos.z}";
        
        private bool IsDecaying(BuildingPrivlidge privilege) =>
            privilege == null || privilege.GetProtectedMinutes(true) <= 0;

        private string GetMsg(string key, string userId = null) => lang.GetMessage(key, this, userId);

        #endregion
    }
}