using EFT.HealthSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.Player.Health
{
    /// <summary>
    /// This patch overrides the original "Make" that is used when creating BSG IEffect network packets
    /// The reason is that we/Aki are publicizing all type/methods/properties but this class requires non publicized properties
    /// Not doing this will cause an error to the EFT.Player.ComplexLateUpdate when using _sendNetworkSyncPackets on the HealthController
    /// </summary>
    public class EffectMakerPatch : ModulePatch
    {
        private static readonly Type[] type_0 = (from t in typeof(ActiveHealthController).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public)
                                                 where !t.IsAbstract && typeof(IEffect).IsAssignableFrom(t)
                                                 orderby t.Name
                                                 select t).ToArray();

        private static readonly Dictionary<string, byte> dictionary_0 = type_0.ToDictionary((Type t) => t.Name, (Type t) => (byte)Array.IndexOf(type_0, t));

        private static readonly Dictionary<byte, string> dictionary_1 = dictionary_0.ToDictionary((KeyValuePair<string, byte> kv) => kv.Value, (KeyValuePair<string, byte> kv) => kv.Key);


        public EffectMakerPatch() { }

        protected override MethodBase GetTargetMethod()
        {
            var type = ReflectionHelpers.EftTypes.FirstOrDefault(
                x => 
                //x.Attributes == TypeAttributes.BeforeFieldInit
                //&& 
                ReflectionHelpers.GetMethodForType(x, "Make") != null
                && ReflectionHelpers.GetMethodForType(x, "Resolve") != null
                && ReflectionHelpers.GetMethodForType(x, "Make").IsStatic
                );
#if DEBUG
            Logger.LogDebug($"{this.GetType().Name}:{type.FullName}");
#endif

            var method = ReflectionHelpers.GetMethodForType(type, "Make");

#if DEBUG
            Logger.LogDebug($"{this.GetType().Name}:{type.FullName}:{method.Name}");
#endif

            return method;
        }

        [PatchPrefix]
        public static bool Prefix(IEffect effect, ref byte __result)
        {
#if DEBUG
            Logger.LogDebug($"{nameof(EffectMakerPatch)}:{nameof(Prefix)}");
#endif
            __result = dictionary_0[effect.GetType().Name];
            return false;
        }

    }
}
