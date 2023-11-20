using BepInEx.Logging;
using Comfort.Common;
using EFT;
using StayInTarkov.Coop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace StayInTarkov.AI.PMCLogic.Friendly.Companion
{
    /// <summary>
    /// Created by: Paulov
    /// Description: TODO: Work in Progress: This is an idea to try and use our own logic for behavior
    /// </summary>
    internal class SITCompanionComponent : MonoBehaviour
    {
        public CoopPlayer CoopPlayer { get; set; }

        public CompanionLogicState CompanionLogicState { get; set; }

        private Vector3? FollowPosition { get; set; }
        private NavMeshPath navMeshPath { get; set; }

        private NavMeshAgent navMeshAgent { get; set; }

        private ManualLogSource Logger { get; set; }

        void Awake()
        {
        }

        void Start()
        {
            navMeshPath = new NavMeshPath();
        }

        void Update()
        {
            if (CoopPlayer == null) 
                return;

            if (Logger == null)
                Logger = BepInEx.Logging.Logger.CreateLogSource($"SITCompanionComponent:{CoopPlayer.ProfileId}");

            navMeshAgent = CoopPlayer.GetOrAddComponent<NavMeshAgent>();
            if (navMeshAgent.isOnNavMesh == false)
            {
                navMeshAgent.Warp(CoopPlayer.Position);
            }

            // second test
            if (navMeshAgent.isOnNavMesh == false)
            {
                //Logger.LogError("navmeshagent is not on the mesh?");
                return;
            }

            // We should equip something right?
            if (CoopPlayer.HandsController is EFT.Player.EmptyHandsController)
            {
                CoopPlayer.TryProceed(CoopPlayer.Equipment.GetSlot(EFT.InventoryLogic.EquipmentSlot.FirstPrimaryWeapon).ContainedItem, (result) => { }, true);
            }

            switch (CompanionLogicState)
            {
                case CompanionLogicState.Follow:
                    if (!FollowPosition.HasValue)
                    {
                        var aRandomUserPlayer = Singleton<GameWorld>.Instance.AllAlivePlayersList.First(x => x.ProfileId.StartsWith("pmc"));
                        if (NavMesh.SamplePosition(aRandomUserPlayer.Position, out var navHit, 100f, NavMesh.AllAreas))
                        {
                            FollowPosition = navHit.position;
                        }
                    }
                    else
                    {
                        if (NavMesh.CalculatePath(CoopPlayer.Position, FollowPosition.Value, NavMesh.AllAreas, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
                        {
                            
                            navMeshAgent.SetDestination(FollowPosition.Value);
                            //Logger.LogDebug($"{CoopPlayer.name} going to {FollowPosition.Value}");
                        }
                    }


                    break;
            }

            if(navMeshAgent.nextPosition != Vector3.zero)
            {
                CoopPlayer.Move((navMeshAgent.nextPosition - CoopPlayer.Position).normalized);
            }
        }
    }

    public enum CompanionLogicState
    {
        Follow,
        CombatEnemy,
        CoverAndHeal,
    }
}
