using StayInTarkov;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using BindableState = BindableState<Diz.DependencyManager.ELoadState>;

namespace Aki.Custom.Utils
{
    public class EasyBundleHelper
    {
        private const BindingFlags NonPublicInstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo _pathField;
        private static readonly FieldInfo _keyWithoutExtensionField;
        private static readonly FieldInfo _bundleLockField;
        private static readonly PropertyInfo _dependencyKeysProperty;
        private static readonly PropertyInfo _keyProperty;
        private static readonly PropertyInfo _loadStateProperty;
        private static readonly MethodInfo _loadingCoroutineMethod;
        private readonly object _instance;
        public static readonly Type Type;

        static EasyBundleHelper()
        {
            _ = nameof(IBundleLock.IsLocked);
            _ = nameof(BindableState.Bind);

            Type = StayInTarkovHelperConstants.EftTypes.SingleCustom(x => !x.IsInterface && x.GetProperty("SameNameAsset", StayInTarkovHelperConstants.PublicDeclaredFlags) != null);

            _pathField = Type.GetField("string_1", NonPublicInstanceFlags);
            _keyWithoutExtensionField = Type.GetField("string_0", NonPublicInstanceFlags);
            _bundleLockField = Type.GetFields(NonPublicInstanceFlags).FirstOrDefault(x => x.FieldType == typeof(IBundleLock));
            _dependencyKeysProperty = Type.GetProperty("DependencyKeys");
            _keyProperty = Type.GetProperty("Key");
            _loadStateProperty = Type.GetProperty("LoadState");

            // Function with 0 params and returns task (usually method_0())
            var possibleMethods = Type.GetMethods(StayInTarkovHelperConstants.PublicDeclaredFlags).Where(x => x.GetParameters().Length == 0 && x.ReturnType == typeof(Task)).ToArray();
            if (possibleMethods.Length > 1)
            {
                throw new Exception($"Unable to find the Loading Coroutine method as there are multiple possible matches: {string.Join(",", possibleMethods.Select(x => x.Name))}");
            }

            if (possibleMethods.Length == 0)
            {
                throw new Exception("Unable to find the Loading Coroutine method as there are no matches");
            }

            _loadingCoroutineMethod = possibleMethods.Single();
        }

        public EasyBundleHelper(object easyBundle)
        {
            _instance = easyBundle;
        }

        public IEnumerable<string> DependencyKeys
        {
            get
            {
                return (IEnumerable<string>)_dependencyKeysProperty.GetValue(_instance);
            }
            set
            {
                _dependencyKeysProperty.SetValue(_instance, value);
            }
        }

        public IBundleLock BundleLock
        {
            get
            {
                return (IBundleLock)_bundleLockField.GetValue(_instance);
            }
            set
            {
                _bundleLockField.SetValue(_instance, value);
            }
        }

        public string Path
        {
            get
            {
                return (string)_pathField.GetValue(_instance);
            }
            set
            {
                _pathField.SetValue(_instance, value);
            }
        }

        public string Key
        {
            get
            {
                return (string)_keyProperty.GetValue(_instance);
            }
            set
            {
                _keyProperty.SetValue(_instance, value);
            }
        }

        public BindableState LoadState
        {
            get
            {
                return (BindableState)_loadStateProperty.GetValue(_instance);
            }
            set
            {
                _loadStateProperty.SetValue(_instance, value);
            }
        }

        public string KeyWithoutExtension
        {
            get
            {
                return (string)_keyWithoutExtensionField.GetValue(_instance);
            }
            set
            {
                _keyWithoutExtensionField.SetValue(_instance, value);
            }
        }

        public Task LoadingCoroutine(Dictionary<string, AssetBundle> bundles)
        {
            return (Task)_loadingCoroutineMethod.Invoke(_instance, new object[] { bundles });
        }
    }
}
