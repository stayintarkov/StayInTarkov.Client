using StayInTarkov.AkiSupport.Singleplayer.Models.Healing;
using StayInTarkov.Networking;
using System.Collections.Generic;
using System.Linq;

namespace StayInTarkov.Health
{
    /// <summary>
    /// Created by: Paulov
    /// Description: A custom "HealthListener" that stores an Instance of the current player's health. This is to store changes that occur during when in the Menus and when leaving a Raid.
    /// </summary>
    public class HealthListener
    {
        private static object _lock = new();
        private static HealthListener _instance = null;
        public object MyHealthController { get; set; }

        public PlayerHealth CurrentHealth { get; } = new PlayerHealth();

        public static HealthListener Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new HealthListener();
                        }
                    }
                }

                return _instance;
            }
        }

        public void Init(object healthController, bool inRaid)
        {
            if (healthController != null && healthController == MyHealthController)
            {
                //PatchConstants.Logger.LogDebug("HealthListener is Same. Ignoring.");
                return;
            }

            StayInTarkovHelperConstants.Logger.LogDebug("HealthListener.Init");

            // init dependencies
            MyHealthController = healthController;

            CurrentHealth.IsAlive = true;

            Update(healthController, inRaid);

        }

        public void Update(object healthController, bool inRaid)
        {
            if (healthController == null)
            {
                StayInTarkovHelperConstants.Logger.LogInfo("HealthListener.Update: HealthController is NULL");
                return;
            }

            StayInTarkovHelperConstants.Logger.LogDebug("HealthListener.Update and Sync.");

            MyHealthController = healthController;

            SetCurrentHealth(MyHealthController, CurrentHealth.Health, EBodyPart.Head);
            SetCurrentHealth(MyHealthController, CurrentHealth.Health, EBodyPart.Chest);
            SetCurrentHealth(MyHealthController, CurrentHealth.Health, EBodyPart.Stomach);
            SetCurrentHealth(MyHealthController, CurrentHealth.Health, EBodyPart.LeftArm);
            SetCurrentHealth(MyHealthController, CurrentHealth.Health, EBodyPart.RightArm);
            SetCurrentHealth(MyHealthController, CurrentHealth.Health, EBodyPart.LeftLeg);
            SetCurrentHealth(MyHealthController, CurrentHealth.Health, EBodyPart.RightLeg);

            SetCurrent(MyHealthController, CurrentHealth, "Energy");
            SetCurrent(MyHealthController, CurrentHealth, "Hydration");

            _ = AkiBackendCommunication.Instance.PostJsonAsync("/player/health/sync", Instance.CurrentHealth.ToJson());
        }

        public static void SetCurrent(object healthController, PlayerHealth currentHealth, string v)
        {
            //PatchConstants.Logger.LogInfo("HealthListener:SetCurrent:" + v);

            if (ReflectionHelpers.GetAllPropertiesForObject(healthController).Any(x => x.Name == v))
            {
                var valuestruct = ReflectionHelpers.GetAllPropertiesForObject(healthController).FirstOrDefault(x => x.Name == v).GetValue(healthController);
                if (valuestruct == null)
                    return;

                var currentAmount = ReflectionHelpers.GetAllFieldsForObject(valuestruct).FirstOrDefault(x => x.Name == "Current").GetValue(valuestruct);
                //PatchConstants.Logger.LogInfo(currentAmount);
                if(currentHealth != null)
                    currentHealth.GetType().GetProperty(v).SetValue(currentHealth, float.Parse(currentAmount.ToString()));
            }
            else if (ReflectionHelpers.GetAllFieldsForObject(healthController).Any(x => x.Name == v))
            {
                var valuestruct = ReflectionHelpers.GetAllFieldsForObject(healthController).FirstOrDefault(x => x.Name == v).GetValue(healthController);
                if (valuestruct == null)
                    return;

                var currentAmount = ReflectionHelpers.GetAllFieldsForObject(valuestruct).FirstOrDefault(x => x.Name == "Current").GetValue(valuestruct);
                //PatchConstants.Logger.LogInfo(currentAmount);

                if(currentHealth != null)
                    currentHealth.GetType().GetProperty(v).SetValue(currentHealth, float.Parse(currentAmount.ToString()));
            }

        }

        public static void SetCurrentHealth(object healthController, IReadOnlyDictionary<EBodyPart, BodyPartHealth> dictionary, EBodyPart bodyPart)
        {
            if (healthController == null)
            {
                StayInTarkovHelperConstants.Logger.LogInfo("HealthListener:GetBodyPartHealth:HealthController is NULL");
                return;
            }

            //PatchConstants.Logger.LogInfo("HealthListener:GetBodyPartHealth");


            var getbodyparthealthmethod = healthController.GetType().GetMethod("GetBodyPartHealth"
                , System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.FlattenHierarchy
                );
            if (getbodyparthealthmethod == null)
            {
                StayInTarkovHelperConstants.Logger.LogInfo("HealthListener:GetBodyPartHealth not found!");
                return;
            }

            //PatchConstants.Logger.LogInfo("GetBodyPartHealth found!");

            var bodyPartHealth = getbodyparthealthmethod.Invoke(healthController, new object[] { bodyPart, false });
            var current = ReflectionHelpers.GetAllFieldsForObject(bodyPartHealth).FirstOrDefault(x => x.Name == "Current").GetValue(bodyPartHealth).ToString();
            var maximum = ReflectionHelpers.GetAllFieldsForObject(bodyPartHealth).FirstOrDefault(x => x.Name == "Maximum").GetValue(bodyPartHealth).ToString();

            dictionary[bodyPart].Initialize(float.Parse(current), float.Parse(maximum));

        }
        public static void HealHalfHealth(object healthController, IReadOnlyDictionary<EBodyPart, BodyPartHealth> dictionary, EBodyPart bodyPart)
        {
            if (healthController == null)
            {
                StayInTarkovHelperConstants.Logger.LogInfo("HealthListener:GetBodyPartHealth:HealthController is NULL");
                return;
            }

            //PatchConstants.Logger.LogInfo("HealthListener:GetBodyPartHealth");

            var getbodyparthealthmethod = healthController.GetType().GetMethod("GetBodyPartHealth"
                , System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.FlattenHierarchy
                );
            if (getbodyparthealthmethod == null)
            {
                StayInTarkovHelperConstants.Logger.LogInfo("HealthListener:GetBodyPartHealth not found!");
                return;
            }

            //PatchConstants.Logger.LogInfo("GetBodyPartHealth found!");

            var bodyPartHealth = getbodyparthealthmethod.Invoke(healthController, new object[] { bodyPart, false });
            var current = ReflectionHelpers.GetAllFieldsForObject(bodyPartHealth).FirstOrDefault(x => x.Name == "Current").GetValue(bodyPartHealth).ToString();
            var maximum = ReflectionHelpers.GetAllFieldsForObject(bodyPartHealth).FirstOrDefault(x => x.Name == "Maximum").GetValue(bodyPartHealth).ToString();
            var halfHealth = float.Parse(maximum) / 2;

            if (float.Parse(current) < halfHealth)
            {
                dictionary[bodyPart].Initialize(halfHealth, float.Parse(maximum));
            }

        }
    }
}