using System;
using UnityEngine;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Game.Rust;
using Newtonsoft.Json;


namespace Oxide.Plugins
{
    [Info("Imperial Server", "Mixfair", "0.1.0")]
    [Description("Privvate stuff")]
    class ImperialStuff : RustPlugin
    {
        private void Init()
        {
            Puts("A baby plugin is born!");
            lang.SetServerLanguage("ru");
        
            
            // foreach(var st in FileSystem_Warmup.GetAssetList()){
            //     Debug.Log(st);
            // }
            
           // AddUniversalCommand("tpf", nameof(Tpf));
        }

        [ChatCommand("inv")]
        private void cmdChatInv(BasePlayer player, string cmd, string[] args)
        {
            Debug.Log("test");
            // Item item = item.Icon
            foreach (Item item in player.inventory.containerMain.itemList){
              
            }
        }

        private string spawnTarget = "";

        [ChatCommand("info")]
        private void cmdChatInfo(BasePlayer player, string cmd, string[] args)
        {
            String[] perms = permission.GetPermissions();
            //perms = permission.GetUserGroups(player.userID.ToString());

            foreach (var perm in perms)
            {
                Debug.Log(perm);
            }

             RelationshipManager.PlayerTeam playersTeam = RelationshipManager.Instance.FindPlayersTeam(76561198022469992);
            
            foreach (ulong member in playersTeam.members)
            {
                Debug.Log(member);
            }

        }

        // The rest of the code magic
    
        [ChatCommand("clone")]
        private void cmdChatClone(BasePlayer player, string cmd, string[] args)
        {

            String[] perms = permission.GetPermissions();
            
            // foreach (var perm in perms)
            // {
            //     Debug.Log(perm);
            // }

            //var player = (BasePlayer)caller.Object;
            //Debug.Log(player.userID); //ply stmid 
            

            //Debug.Log(permission.UserHasPermission(player.UserIDString, this.Title +".admin"));
            var layers =  LayerMask.GetMask("Construction", "Default", "Deployed", "Resource", "Terrain", "Water", "World", "Player (Server)", "AI", "Deployed", "Terrain", "World");
            //var playerLayer = LayerMask.GetMask("Player (Server)");
            //var targetLayer = LayerMask.GetMask("Player (Server)", "AI", "Deployed", "Construction");
            //var groundLayer = LayerMask.GetMask("Construction", "Terrain", "World");


            RaycastHit hit = new RaycastHit();

            if (Physics.Raycast(player.eyes.HeadRay(), out hit, float.MaxValue, layers))
            {
                var entity = hit.GetEntity();
                if (entity != null)
                {
                    PrintToChat(player, $"Cloned entity: {entity.PrefabName}");
                    this.spawnTarget = entity.PrefabName;
                }
            }
            
            

            

        }

        [ChatCommand("despawn")]
        private void cmdChatDespawn(BasePlayer player, string cmd, string[] args)
        {
            var layers =  LayerMask.GetMask("Construction", "Default", "Deployed", "Resource", "Terrain", "Water", "World", "Player (Server)", "AI", "Deployed", "Terrain", "World");
            
            RaycastHit hit = new RaycastHit();

            if (Physics.Raycast(player.eyes.HeadRay(), out hit, float.MaxValue, layers))
            {
                var entity = hit.GetEntity();
                if (entity != null && entity.IsDestroyed == false)
                {
                    PrintToChat(player, $"Despawned entity: {entity.PrefabName}");
                    entity.Kill();
                }
            }

           
            
            Puts("spawned");            

        }


        [ChatCommand("spawn")]
        private void cmdChatSpawn(BasePlayer player, string cmd, string[] args)
        {

            String[] perms = permission.GetPermissions();
            
            // foreach (var perm in perms)
            // {
            //     Debug.Log(perm);
            // }

            //var player = (BasePlayer)caller.Object;
            //Debug.Log(player.userID); //ply stmid 
            perms = permission.GetUserGroups(player.userID.ToString());

            var pos = new Vector3(player.IPlayer.Position().X + 1.5f, player.IPlayer.Position().Y, player.IPlayer.Position().Z );
            var ent = GameManager.server.CreateEntity(spawnTarget, pos);
           
            //MeshRenderer component = ent.GetComponent<MeshRenderer>();
            //Debug.Log(component.material.color);


            ent.Spawn(); 
            
            Puts("spawned");            

        }

    }

}