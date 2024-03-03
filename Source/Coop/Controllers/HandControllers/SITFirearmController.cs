using BepInEx.Logging;
using Comfort.Common;
using EFT.InventoryLogic;
using EFT.UI;
using StayInTarkov.Coop.NetworkPacket.Player.Weapons;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.Controllers.HandControllers
{
    public class SITFirearmController : EFT.Player.FirearmController
    {
        ManualLogSource BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(SITFirearmController));

        public override void Spawn(float animationSpeed, Action callback)
        {
            ConsoleScreen.Log($"{this.GetType().Name}:{nameof(Spawn)}");
            BepInLogger.LogDebug($"{nameof(SITFirearmController)}:{nameof(Spawn)}");
            base.Spawn(animationSpeed, callback);
        }

        public override void Execute(IOperation1 operation, Callback callback)
        {
            BepInLogger.LogDebug($"{nameof(SITFirearmController)}:{nameof(Execute)}");
            base.Execute(operation, callback);
        }

        public override void Drop(float animationSpeed, Action callback, bool fastDrop = false, Item nextControllerItem = null)
        {
            base.Drop(animationSpeed, callback, fastDrop, nextControllerItem);
            BepInLogger.LogDebug($"{nameof(SITFirearmController)}:{nameof(Drop)}");
        }

        public override void QuickReloadMag(MagazineClass magazine, Callback callback)
        {
            BepInLogger.LogDebug($"{nameof(SITFirearmController)}:{nameof(QuickReloadMag)}");
            if (CanStartReload())
            {
                base.QuickReloadMag(magazine, callback);
                QuickReloadMagPacket quickReloadMagPacket = new QuickReloadMagPacket();
                quickReloadMagPacket.ProfileId = _player.ProfileId;
                quickReloadMagPacket.ItemId = magazine.Id;
                GameClient.SendData(quickReloadMagPacket.Serialize());
            }
        }

        public override void ReloadMag(MagazineClass magazine, GridItemAddress gridItemAddress, Callback callback)
        {
            if (CanStartReload())
            {
                base.ReloadMag(magazine, gridItemAddress, callback);
                ReloadMagPacket reloadMagPacket = new ReloadMagPacket(_player.ProfileId, magazine.Id, gridItemAddress);
                reloadMagPacket.ProfileId = _player.ProfileId;
                reloadMagPacket.ItemId = magazine.Id;
                reloadMagPacket.GridItemAddress = gridItemAddress;
                GameClient.SendData(reloadMagPacket.Serialize());
            }
        }

        public override void ReloadWithAmmo(AmmoPack ammoPack, Callback callback)
        {
            if (CanStartReload())
            {
                base.ReloadWithAmmo(ammoPack, callback);
                StayInTarkov.Coop.NetworkPacket.Player.Weapons.ReloadWithAmmoPacket packet = new (_player.ProfileId, ammoPack.GetReloadingAmmoIds());
                GameClient.SendData(packet.Serialize());
            }
        }
    }
}
