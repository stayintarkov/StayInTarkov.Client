using BepInEx.Logging;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using StayInTarkov.Coop.NetworkPacket.Player.Health;
using StayInTarkov.Networking;
using System.IO;

namespace StayInTarkov.Coop.Controllers
{
    public sealed class CoopHealthController : PlayerHealthController
    {
        public ManualLogSource BepInLogger { get; private set; }

        public CoopHealthController(Profile.ProfileHealth healthInfo, EFT.Player player, InventoryControllerClass inventoryController, SkillManager skillManager, bool aiHealth)
            : base(healthInfo, player, inventoryController, skillManager, aiHealth)
        {
            BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(CoopHealthController));
            BepInLogger.LogInfo(nameof(CoopHealthController));
        }

        //public override bool ApplyItem(Item item, EBodyPart bodyPart, float? amount = null)
        //{
        //    return base.ApplyItem(item, bodyPart, amount);
        //}

        protected override void AddEffectToList(AbstractEffect effect)
        {
            if (BepInLogger != null)
                BepInLogger.LogInfo($"{nameof(CoopHealthController)}:{nameof(AddEffectToList)}");

            PlayerHealthEffectPacket packet = new PlayerHealthEffectPacket();
            packet.Add = true;
            packet.Effect = effect;
            GameClient.SendData(packet.Serialize());

            base.AddEffectToList(effect);
        }

        protected override bool RemoveEffectFromList(AbstractEffect effect)
        {
            if (BepInLogger != null)
                BepInLogger.LogInfo($"{nameof(CoopHealthController)}:{nameof(RemoveEffectFromList)}");

            PlayerHealthEffectPacket packet = new PlayerHealthEffectPacket();
            packet.Add = false;
            packet.Effect = effect;
            GameClient.SendData(packet.Serialize());

            return base.RemoveEffectFromList(effect);
        }

        //public override void SetEncumbered(bool encumbered)
        //{
        //    base.SetEncumbered(encumbered);
        //}

        //public override void SetOverEncumbered(bool encumbered)
        //{
        //    base.SetOverEncumbered(encumbered);
        //}

    }
}
