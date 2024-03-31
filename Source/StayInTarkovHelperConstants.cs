using BepInEx.Logging;
using Comfort.Common;
using EFT;
using FilesChecker;
using Newtonsoft.Json;
using StayInTarkov.Coop.SITGameModes;
using StayInTarkov.EssentialPatches;
using StayInTarkov.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static StayInTarkov.PaulovJsonConverters;

namespace StayInTarkov
{
    /// <summary>
    /// Credit: SPT-Aki
    /// Modified heavily by: Paulov
    /// </summary>
    public static class StayInTarkovHelperConstants
    {
        public static BindingFlags PrivateFlags { get; } = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        public static BindingFlags PublicDeclaredFlags { get; } = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;
        
        public static Type SessionInterfaceType { get; private set; }
        public static Type BackendProfileInterfaceType => EftTypes.SingleCustom(x => x.GetMethods().Length == 2 && x.GetMethods().Select(y => y.Name).Contains("get_Profile") && x.IsInterface);
        public static Type BackendProfilePetInterfaceType { get; private set; }

        private static Type[] _eftTypes;
        public static Type[] EftTypes
        {
            get
            {
                if (_eftTypes == null)
                {
                    _eftTypes = typeof(AbstractGame).Assembly.GetTypes().OrderBy(t => t.Name).ToArray();
                }

                return _eftTypes;
            }
        }

        public static Type[] FilesCheckerTypes { get; private set; }

        private static Type[] _sitTypes;
        public static Type[] SITTypes
        {
            get
            {
                if (_sitTypes == null)
                {
                    _sitTypes = typeof(ISITGame).Assembly.GetTypes().OrderBy(t => t.Name).ToArray();
                }

                return _sitTypes;
            }
        }


        /// <summary>
        /// A Key/Value dictionary of storing & obtaining an array of types by name
        /// </summary>
        public static Dictionary<string, Type[]> TypesDictionary { get; } = new();

        /// <summary>
        /// A Key/Value dictionary of storing & obtaining a type by name
        /// </summary>
        public static Dictionary<string, Type> TypeDictionary { get; } = new();

        /// <summary>
        /// A Key/Value dictionary of storing & obtaining a method by type and name
        /// </summary>
        public static readonly Dictionary<(Type, string), MethodInfo> MethodDictionary = new();

        private static string backendUrl { get; set; }

        /// <summary>
        /// Method that returns the Backend Url (Example: https://127.0.0.1)
        /// </summary>
        private static string RealWSURL;
        public static string GetBackendUrl()
        {
            if (string.IsNullOrEmpty(backendUrl))
            {
                backendUrl = DetectBackendUrlAndToken.GetBackendConnection().BackendUrl;
            }
            return backendUrl;
        }
        public static string GetREALWSURL() //cut the server address obtained from GetBackendUrl and convert it to "ws://" w
        {
            if (string.IsNullOrEmpty(RealWSURL))
            {
                RealWSURL = DetectBackendUrlAndToken.GetBackendConnection().BackendUrl;
                int colonIndex = RealWSURL.LastIndexOf(':');
                if (colonIndex != -1)
                {
                    RealWSURL = RealWSURL.Substring(0, colonIndex);
                }
                RealWSURL = RealWSURL.Replace("http", "ws");
            }
            return RealWSURL;
        }
        public static string GetPHPSESSID()
        {
            if (DetectBackendUrlAndToken.GetBackendConnection() == null)
                Logger.LogError("Cannot get Backend Info");

            return DetectBackendUrlAndToken.GetBackendConnection().PHPSESSID;
        }

        public static ManualLogSource Logger { get; private set; }

        public static Type JsonConverterType { get; }
        public static Newtonsoft.Json.JsonConverter[] JsonConverterDefault { get; }

        private static ISession _backEndSession;
        public static ISession BackEndSession
        {
            get
            {
                if (_backEndSession == null && Singleton<TarkovApplication>.Instantiated)
                {
                    _backEndSession = Singleton<TarkovApplication>.Instance.GetClientBackEndSession();
                }

                if (_backEndSession == null && Singleton<ClientApplication<ISession>>.Instantiated)
                {
                    _backEndSession = Singleton<ClientApplication<ISession>>.Instance.GetClientBackEndSession();
                }

                return _backEndSession;
            }
        }

        public static Newtonsoft.Json.JsonConverter[] GetJsonConvertersBSG()
        {
            return JsonConverterDefault;
        }

        public static List<Newtonsoft.Json.JsonConverter> GetJsonConvertersPaulov()
        {
            var converters = new List<Newtonsoft.Json.JsonConverter>();
            converters.Add(new DateTimeOffsetJsonConverter());
            converters.Add(new SimpleCharacterControllerJsonConverter());
            converters.Add(new CollisionFlagsJsonConverter());
            converters.Add(new PlayerJsonConverter());
            converters.Add(new NotesJsonConverter());
            return converters;
        }

        private static List<Newtonsoft.Json.JsonConverter> SITSerializerConverters;

        public static JsonSerializerSettings GetJsonSerializerSettings()
        {
            if (SITSerializerConverters == null || SITSerializerConverters.Count == 0)
            {
                SITSerializerConverters = GetJsonConvertersBSG().ToList();
                var paulovconverters = GetJsonConvertersPaulov();
                SITSerializerConverters.AddRange(paulovconverters.ToArray());
            }

            return new JsonSerializerSettings()
            {
                Converters = SITSerializerConverters,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                FloatFormatHandling = FloatFormatHandling.DefaultValue,
                FloatParseHandling = FloatParseHandling.Double,
                Error = (serializer, err) =>
                {
                    Logger.LogError("SERIALIZATION ERROR");
                    Logger.LogError(err.ErrorContext.Error.ToString());
                }
            };
        }
        public static JsonSerializerSettings GetJsonSerializerSettingsWithoutBSG()
        {
            var converters = GetJsonConvertersPaulov();

            return new JsonSerializerSettings()
            {
                Converters = converters,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Error = (serializer, err) =>
                {
                    Logger.LogError($"JsonSerializer.Error {err.ErrorContext.Error}");
                }
            };
        }

        public static string SITToJson(this object o)
        {


            return JsonConvert.SerializeObject(o
                    , GetJsonSerializerSettings()
                );
        }

        public static async Task<string> SITToJsonAsync(this object o)
        {
            return await Task.Run(() =>
            {
                return SITToJson(o);
            });
        }

        public static T SITParseJson<T>(this string str)
        {
            return JsonConvert.DeserializeObject<T>(str
                    , GetJsonSerializerSettings()
                    );
        }

        public static bool TrySITParseJson<T>(this string str, out T result)
        {
            try
            {
                //result = JsonConvert.DeserializeObject<T>(str
                //        , new JsonSerializerSettings()
                //        {
                //            Converters = JsonConverterDefault
                //            ,
                //            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                //        }
                //        );
                result = SITParseJson<T>(str);
                return true;
            }
            catch (Exception ex)
            {
                Logger?.LogError(nameof(TrySITParseJson) + ": has failed to Parse Json");
                Logger?.LogError(nameof(TrySITParseJson) + ": " + str);
                Logger?.LogError(nameof(TrySITParseJson) + ": " + ex);
                result = default(T);
                return false;
            }
        }

        public static ClientApplication<ISession> GetClientApp()
        {
            return Singleton<ClientApplication<ISession>>.Instance;
        }

        public static TarkovApplication GetMainApp()
        {
            return GetClientApp() as TarkovApplication;
        }
        
        public static T SingleCustom<T>(this IEnumerable<T> types, Func<T, bool> predicate) where T : MemberInfo
        {
            if (types == null)
            {
                throw new ArgumentNullException(nameof(types));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            var matchingTypes = types.Where(predicate).ToArray();

            if (matchingTypes.Length > 1)
            {
                throw new InvalidOperationException($"More than one member matches the specified search pattern: {string.Join(", ", matchingTypes.Select(t => t.Name))}");
            }

            if (matchingTypes.Length == 0)
            {
                throw new InvalidOperationException("No members match the specified search pattern");
            }

            return matchingTypes[0];
        }

        static StayInTarkovHelperConstants()
        {
            if (Logger == null)
                Logger = BepInEx.Logging.Logger.CreateLogSource("SIT.Tarkov.Core.PatchConstants");

            TypesDictionary.Add("EftTypes", EftTypes);
            Logger.LogInfo($"PatchConstants: {EftTypes.Length} EftTypes found");

            FilesCheckerTypes = typeof(ICheckResult).Assembly.GetTypes();
            DisplayMessageNotifications.MessageNotificationType = EftTypes.Single(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public).Select(y => y.Name).Contains("DisplayMessageNotification"));

            JsonConverterType = typeof(AbstractGame).Assembly.GetTypes()
               .First(t => t.GetField("Converters", BindingFlags.Static | BindingFlags.Public) != null);
            JsonConverterDefault = JsonConverterType.GetField("Converters", BindingFlags.Static | BindingFlags.Public).GetValue(null) as JsonConverter[];
            Logger.LogInfo($"PatchConstants: {JsonConverterDefault.Length} JsonConverters found");
            SessionInterfaceType = EftTypes.Single(x => x.GetMethods().Select(y => y.Name).Contains("GetPhpSessionId") && x.IsInterface);
            //BackendSessionInterfaceType = EftTypes.Single(x => x.GetMethods().Select(y => y.Name).Contains("ChangeProfileStatus") && x.IsInterface);
            BackendProfilePetInterfaceType = EftTypes.Single(x => x.GetMethods().Select(y => y.Name).Contains("ReadEncyclopedia") && x.IsInterface);
            //BackendProfileInterfaceType = EftTypes.SingleCustom(x => x.GetMethods().Length == 2 && x.GetMethods().Select(y => y.Name).Contains("get_Profile") && x.IsInterface);

        }
    }
}
