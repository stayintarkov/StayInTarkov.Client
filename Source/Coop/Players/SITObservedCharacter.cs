using EFT.NextObservedPlayer;
using StayInTarkov.Coop.NetworkPacket.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.Players
{
    /// <summary>
    /// TODO: This will mark a significant change to Character system in SIT. Unfinished.
    /// </summary>
    public sealed class SITObservedCharacter : ObservedPlayerView
    {
        /// <summary>
        /// TODO: This will mark a significant change to Character system in SIT. Unfinished.
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="informationPacket"></param>
        /// <returns></returns>
        public static SITObservedCharacter Create(int playerId, PlayerInformationPacket informationPacket)
        {
            // TODO: This is missing the HandsController stuff
            return 
                ObservedPlayerView.Create<SITObservedCharacter>(playerId, new GMessage2() 
                { 
                    AccountId = informationPacket.AccountId,
                     ArmorPlateColliderMask = informationPacket.ArmorPlateColliderMask,
                      ArmorsInfo = informationPacket.ArmorsInfo,
                       BodyPosition = informationPacket.BodyPosition,
                        Customization = informationPacket.Customization,
                         GroupID = informationPacket.GroupID,
                          Inventory = informationPacket.Inventory,
                           IsAI = informationPacket.IsAI,
                            NickName = informationPacket.NickName,
                             ProfileID = informationPacket.ProfileId,
                              RemoteTime = informationPacket.RemoteTime,
                               Side = informationPacket.Side,
                                TeamID = informationPacket.TeamID,
                                 Voice = informationPacket.Voice,
                                  VoIPState = informationPacket.VoIPState,
                                   WildSpawnType = informationPacket.WildSpawnType,
                });
        }
    }
}
