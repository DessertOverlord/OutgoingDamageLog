using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.API.Common.Entities;
using Vintagestory;

namespace OutgoingDamageLog
{
    [HarmonyPatch]
    public class OutgoingDamageLogModSystem : ModSystem
    {
        public static ICoreAPI api;
        public Harmony harmony;
        public override void Start(ICoreAPI api)
        {
            OutgoingDamageLogModSystem.api = api;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Entity), nameof(Entity.OnHurt))]
        public static void OutgoingDamageLogPlayer(Entity __instance, DamageSource dmgSource, float damage)
        {
            IWorldAccessor World = __instance.World;
            if (damage != 0 && World?.Side == EnumAppSide.Server)
            {
                string creatureName = Lang.Get("prefixandcreature-" + __instance.Code.Path.Replace("-", ""));
                EntityPlayer eplr = dmgSource.GetCauseEntity() as EntityPlayer;
                string msg = "";

                if (dmgSource.Source == EnumDamageSource.Player || dmgSource.CauseEntity.Class == "EntityPlayer")
                {
                    string damageTypeOutput = "";
                    switch (dmgSource.Type)
                    {
                        case EnumDamageType.SlashingAttack:
                            damageTypeOutput = "Slashing";
                            break;
                        case EnumDamageType.PiercingAttack:
                            damageTypeOutput = "Piercing";
                            break;
                        case EnumDamageType.BluntAttack:
                            damageTypeOutput = "Blunt";
                            break;
                        default:
                            damageTypeOutput = "Unknown";
                            break;
                    }
                    msg = "Dealt " + damage + " " + damageTypeOutput + " damage to " + creatureName;
                } 
                string PlayerUID = eplr.PlayerUID;
                (World.PlayerByUid(PlayerUID) as IServerPlayer).SendMessage(GlobalConstants.DamageLogChatGroup, msg, EnumChatType.Notification);
            }
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            if (!Harmony.HasAnyPatches(Mod.Info.ModID))
            {
                harmony = new Harmony(Mod.Info.ModID);
                harmony.PatchAll();
            }
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
        }

    }
}
