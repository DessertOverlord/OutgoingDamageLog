using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.API.Common.Entities;
using Vintagestory;
using System.Collections.Generic;

namespace OutgoingDamageLog
{
    [HarmonyPatch]
    public class OutgoingDamageLogModSystem : ModSystem
    {
        public static ICoreAPI api;
        public Harmony harmony;
        public override void StartServerSide(ICoreServerAPI api)
        {
            OutgoingDamageLogModSystem.api = api;
            if (!Harmony.HasAnyPatches(Mod.Info.ModID))
            {
                harmony = new Harmony(Mod.Info.ModID);
                harmony.PatchAll();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Entity), nameof(Entity.OnHurt))]
        public static void OutgoingDamageLogPlayer(Entity __instance, DamageSource dmgSource, float damage)
        {
            IWorldAccessor World = __instance.World;
            if (World?.Side == EnumAppSide.Server && (dmgSource.Source == EnumDamageSource.Player || dmgSource.CauseEntity?.Class == "EntityPlayer") && damage >= 0)
            {
                string creatureName = Lang.Get("prefixandcreature-" + __instance.Code.Path.Replace("-", ""));
                EntityPlayer eplr = dmgSource.GetCauseEntity() as EntityPlayer;
                string damageTypeOutput = "";
                switch (dmgSource.Type)
                {
                    case EnumDamageType.SlashingAttack:
                        damageTypeOutput = Lang.Get("outgoingdamagelog:slashing");
                        break;
                    case EnumDamageType.PiercingAttack:
                        damageTypeOutput = Lang.Get("outgoingdamagelog:piercing");
                        break;
                    case EnumDamageType.BluntAttack:
                        damageTypeOutput = Lang.Get("outgoingdamagelog:blunt");
                        break;
                    default:
                        damageTypeOutput = Lang.Get("outgoingdamagelog:unknown");
                        break;
                }
                string msg = Lang.Get("outgoingdamagelog:damage-output", damage, damageTypeOutput, creatureName);
                string PlayerUID = eplr.PlayerUID;
                (World.PlayerByUid(PlayerUID) as IServerPlayer).SendMessage(GlobalConstants.DamageLogChatGroup, msg, EnumChatType.Notification);
            }
        }
        public override void Dispose()
        {
            harmony?.UnpatchAll(Mod.Info.ModID);
        }
    }
}

