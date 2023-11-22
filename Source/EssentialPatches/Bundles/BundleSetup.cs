using System;
using System.Linq;

namespace StayInTarkov.Bundles
{
    public static class BundleSetup
    {
        public static Type IEasyBundleType { get; set; }
        public static Type IBundleLockType { get; set; }
        public static Type BundleLockType { get; set; }
        public static Type DependancyGraphType { get; set; }
        public static Type BindableStateType { get; set; }


        public static void Init()
        {
            IEasyBundleType = StayInTarkovHelperConstants.EftTypes.Single(x => x.IsInterface
                &&
                 (ReflectionHelpers.GetFieldFromType(x, "SameNameAsset") != null
                 || ReflectionHelpers.GetPropertyFromType(x, "SameNameAsset") != null)

                );

            StayInTarkovHelperConstants.Logger.LogInfo("BundleSetup.Init.IEasyBundleType:" + IEasyBundleType.Name);

            IBundleLockType = StayInTarkovHelperConstants.EftTypes.Single(x => x.IsInterface
                &&
                 (ReflectionHelpers.GetFieldFromType(x, "IsLocked") != null
                 || ReflectionHelpers.GetPropertyFromType(x, "IsLocked") != null)
                && ReflectionHelpers.GetAllMethodsForType(x).Any(x => x.Name == "Lock")
                );

            StayInTarkovHelperConstants.Logger.LogInfo("BundleSetup.Init.IBundleLockType:" + IBundleLockType.Name);

            BundleLockType = StayInTarkovHelperConstants.EftTypes.Single(x =>
                 (ReflectionHelpers.GetFieldFromType(x, "IsLocked") != null
                 || ReflectionHelpers.GetPropertyFromType(x, "IsLocked") != null)
                &&
                (ReflectionHelpers.GetFieldFromType(x, "MaxConcurrentOperations") != null
                 || ReflectionHelpers.GetPropertyFromType(x, "MaxConcurrentOperations") != null)
                &&
                ReflectionHelpers.GetAllMethodsForType(x).Any(x => x.Name == "Lock")
                );

            StayInTarkovHelperConstants.Logger.LogInfo("BundleSetup.Init.BundleLockType:" + BundleLockType.Name);

            DependancyGraphType = StayInTarkovHelperConstants.EftTypes.Single(x =>
                x.IsSealed
               &&
                ReflectionHelpers.GetAllMethodsForType(x).Any(x => x.Name == "Retain")
               &&
               ReflectionHelpers.GetAllMethodsForType(x).Any(x => x.Name == "RetainSeparate")
               &&
               ReflectionHelpers.GetAllMethodsForType(x).Any(x => x.Name == "GetNode")
               );

            StayInTarkovHelperConstants.Logger.LogInfo("BundleSetup.Init.DependancyGraphType:" + DependancyGraphType.Name);

            BindableStateType = StayInTarkovHelperConstants.EftTypes.Single(x =>
               x.IsSealed
              &&
               x.GetConstructors().Length >= 2
              &&
              x.IsGenericTypeDefinition
              && (ReflectionHelpers.GetFieldFromType(x, "Value") != null
                 || ReflectionHelpers.GetPropertyFromType(x, "Value") != null)
              && (ReflectionHelpers.GetFieldFromType(x, "HasHandlers") != null
                 || ReflectionHelpers.GetPropertyFromType(x, "HasHandlers") != null)
              );

            StayInTarkovHelperConstants.Logger.LogInfo("BundleSetup.Init.BindableStateType:" + BindableStateType.Name);
        }
    }
}
