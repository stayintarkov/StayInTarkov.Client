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
            BepInLogger.LogDebug(nameof(CoopHealthController));
        }

        //public override bool ApplyItem(Item item, EBodyPart bodyPart, float? amount = null)
        //{
        //    return base.ApplyItem(item, bodyPart, amount);
        //}

        protected override void AddEffectToList(AbstractEffect effect)
        {
            if (BepInLogger != null)
                BepInLogger.LogDebug($"{nameof(CoopHealthController)}:{nameof(AddEffectToList)}");

            base.AddEffectToList(effect);

            if (effect == null)
                return;

            try
            {
                PlayerHealthEffectPacket packet = new PlayerHealthEffectPacket();
                packet.ProfileId = Player.ProfileId;
                packet.Add = true;
                packet.EffectType = effect.GetType().Name;
                packet.TimeLeft = effect.TimeLeft;
                packet.BodyPart = effect.BodyPart;
                GameClient.SendData(packet.Serialize());
            }
            catch { }

        }

        protected override bool RemoveEffectFromList(AbstractEffect effect)
        {
            if (BepInLogger != null)
                BepInLogger.LogDebug($"{nameof(CoopHealthController)}:{nameof(RemoveEffectFromList)}");

            var result = base.RemoveEffectFromList(effect);

            try
            {
                PlayerHealthEffectPacket packet = new PlayerHealthEffectPacket();
                packet.ProfileId = Player.ProfileId;
                packet.Add = false;
                packet.EffectType = effect.GetType().Name;
                packet.TimeLeft = effect.TimeLeft;
                packet.BodyPart = effect.BodyPart;
                GameClient.SendData(packet.Serialize());
            }
            catch { }

            return result;
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
