using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using StayInTarkov.Coop.ItemControllerPatches;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Networking;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StayInTarkov.Coop
{
    internal sealed class CoopInventoryController
        : EFT.Player.PlayerInventoryController, ICoopInventoryController
    {
        ManualLogSource BepInLogger { get; set; }

        public HashSet<string> AlreadySent = new();


        public CoopInventoryController(EFT.Player player, Profile profile, bool examined) : base(player, profile, examined)
        {
            BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(CoopInventoryController));

            if (profile.ProfileId.StartsWith("pmc") && !IsDiscardLimitsFine(DiscardLimits))
                base.ResetDiscardLimits();
        }

        public override Task<IResult> LoadMagazine(BulletClass sourceAmmo, MagazineClass magazine, int loadCount, bool ignoreRestrictions)
        {
            //BepInLogger.LogInfo("LoadMagazine");
            return base.LoadMagazine(sourceAmmo, magazine, loadCount, ignoreRestrictions);
        }

        public override Task<IResult> UnloadMagazine(MagazineClass magazine)
        {
            Task<IResult> result;
            ItemControllerHandler_Move_Patch.DisableForPlayer.Add(Profile.ProfileId);

            BepInLogger.LogInfo("UnloadMagazine");
            ItemPlayerPacket unloadMagazinePacket = new(Profile.ProfileId, magazine.Id, magazine.TemplateId, "PlayerInventoryController_UnloadMagazine");
            var serialized = unloadMagazinePacket.Serialize();

            //if (AlreadySent.Contains(serialized))
            {
                result = base.UnloadMagazine(magazine);
                ItemControllerHandler_Move_Patch.DisableForPlayer.Remove(Profile.ProfileId);
            }

            //AlreadySent.Add(serialized);

            AkiBackendCommunication.Instance.SendDataToPool(serialized);
            result = base.UnloadMagazine(magazine);
            ItemControllerHandler_Move_Patch.DisableForPlayer.Remove(Profile.ProfileId);
            return result;
        }

        public override void ThrowItem(Item item, IEnumerable<ItemsCount> destroyedItems, Callback callback = null, bool downDirection = false)
        {
            base.ThrowItem(item, destroyedItems, callback, downDirection);
        }

        public void ReceiveUnloadMagazineFromServer(ItemPlayerPacket unloadMagazinePacket)
        {
            BepInLogger.LogInfo("ReceiveUnloadMagazineFromServer");
            if (ItemFinder.TryFindItem(unloadMagazinePacket.ItemId, out Item magazine))
            {
                ItemControllerHandler_Move_Patch.DisableForPlayer.Add(unloadMagazinePacket.ProfileId);
                base.UnloadMagazine((MagazineClass)magazine);
                ItemControllerHandler_Move_Patch.DisableForPlayer.Remove(unloadMagazinePacket.ProfileId);

            }
        }

        public static bool IsDiscardLimitsFine(Dictionary<string, int> DiscardLimits)
        {
            return DiscardLimits != null
                && DiscardLimits.Count > 0
                && DiscardLimits.ContainsKey("5449016a4bdc2d6f028b456f") // Roubles, Value: 20000
                && DiscardLimits.ContainsKey("5696686a4bdc2da3298b456a") // Dollars, Value: 0
                && DiscardLimits.ContainsKey("569668774bdc2da2298b4568") // Euros, Value: 0
                && DiscardLimits.ContainsKey("5448be9a4bdc2dfd2f8b456a") // RGD-5 Grenade, Value: 20
                && DiscardLimits.ContainsKey("5710c24ad2720bc3458b45a3") // F-1 Grenade, Value: 20
                && DiscardLimits.ContainsKey(DogtagComponent.BearDogtagsTemplate) // Value: 0
                && DiscardLimits.ContainsKey(DogtagComponent.UsecDogtagsTemplate); // Value: 0
        }
    }

    public interface ICoopInventoryController
    {
        public void ReceiveUnloadMagazineFromServer(ItemPlayerPacket unloadMagazinePacket);
    }
}
