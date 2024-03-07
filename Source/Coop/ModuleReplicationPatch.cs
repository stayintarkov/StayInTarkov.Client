using Newtonsoft.Json;
using StayInTarkov.Coop.NetworkPacket.Player;
using StayInTarkov.Networking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace StayInTarkov.Coop
{
    /// <summary>
    /// Created by: Paulov
    /// Inherits: ModulePatch
    /// Description: Based on ModulePatch by the SPT-Aki team. This adds a Replicated method for packets to process through.
    /// </summary>
    public abstract class ModuleReplicationPatch : ModulePatch, IModuleReplicationPatch
    {
        public static Dictionary<string, ModuleReplicationPatch> Patches { get; } = new Dictionary<string, ModuleReplicationPatch>();

        public ModuleReplicationPatch()
        {
            if (Patches.Any(x => x.GetType() == this.GetType()))
            {
                //Logger.LogError($"Attempted to recreate {this.GetType()} Patch");
                return;
            }

            if (!DisablePatch && !Patches.ContainsKey(this.MethodName))
                Patches.Add(this.MethodName, this);

            LastSent.TryAdd(GetType(), new Dictionary<string, object>());
        }

        public abstract Type InstanceType { get; }
        public Type OverrideInstanceType { get; set; }
        public abstract string MethodName { get; }

        public virtual bool DisablePatch { get; } = false;

        protected static ConcurrentDictionary<Type, Dictionary<string, object>> LastSent = new();


        public static string SerializeObject(object o)
        {
            try
            {
                return o.SITToJson();
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
            return string.Empty;
        }

        public static T DeserializeObject<T>(string s)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(s, StayInTarkovHelperConstants.GetJsonSerializerSettings());
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
            return default(T);
        }

        public abstract void Replicated(EFT.Player player, Dictionary<string, object> dict);

        protected static ConcurrentDictionary<Type, ConcurrentDictionary<string, ConcurrentBag<long>>> ProcessedCalls = new();

        protected static bool HasProcessed(Type type, EFT.Player player, Dictionary<string, object> dict)
        {
            if (!ProcessedCalls.ContainsKey(type))
                ProcessedCalls.TryAdd(type, new ConcurrentDictionary<string, ConcurrentBag<long>>());

            var playerId = player.ProfileId;
            var timestamp = long.Parse(dict["t"].ToString());
            return HasProcessed(type, playerId, timestamp);

        }

        protected static bool HasProcessed(Type type, EFT.Player player, BasePlayerPacket playerPacket)
        {
            if (!ProcessedCalls.ContainsKey(type))
                ProcessedCalls.TryAdd(type, new ConcurrentDictionary<string, ConcurrentBag<long>>());

            var playerId = player.ProfileId;
            var timestamp = long.Parse(playerPacket.TimeSerializedBetter);
            return HasProcessed(type, playerId, timestamp);
        }

        protected static bool HasProcessed(Type type, string playerId, long timestamp)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (!ProcessedCalls.ContainsKey(type))
                ProcessedCalls.TryAdd(type, new ConcurrentDictionary<string, ConcurrentBag<long>>());

            if (!ProcessedCalls[type].ContainsKey(playerId))
            {
                ProcessedCalls[type].TryAdd(playerId, new ConcurrentBag<long>());
            }

            if (!ProcessedCalls[type][playerId].Contains(timestamp))
            {
                ProcessedCalls[type][playerId].Add(timestamp);
                return false;
            }

            if (ProcessedCalls[type][playerId].Count > 100)
            {
                Logger.LogDebug($"Processed calls for {type}{playerId} seem a little high. Lets trim this collection.");
            }

            if (stopwatch.ElapsedMilliseconds > 1)
                StayInTarkovHelperConstants.Logger.LogDebug($"HasProcessed {type} took {stopwatch.ElapsedMilliseconds}ms to process!");

            return true;
        }

        public static void Replicate(Type type, EFT.Player player, Dictionary<string, object> dict)
        {
            if (!Patches.Any(x => x.GetType().Equals(type)))
                return;

            var p = Patches.Single(x => x.GetType().Equals(type));
            p.Value.Replicated(player, dict);
        }

        public static bool IsHighPingOrAI(EFT.Player player)
        {
            if (AkiBackendCommunication.Instance.HighPingMode && player.IsYourPlayer)
                return true;

            return player.AIData != null && player.AIData.BotOwner != null;
        }

        public static bool IsHighPingOwnPlayerOrAI(EFT.Player player)
        {
            if (AkiBackendCommunication.Instance.HighPingMode && player.IsYourPlayer)
                return true;

            return player.AIData != null && player.AIData.BotOwner != null;
        }

        public override void Enable()
        {
            base.Enable();
            if (!Patches.ContainsKey(MethodName))
                Patches.Add(this.MethodName, this);
        }

        public override void Disable() { base.Disable(); if (Patches.ContainsKey(this.MethodName)) Patches.Remove(this.MethodName); }


        public override bool Equals(object obj)
        {
            if (obj.GetType() == this.GetType())
                return true;

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
