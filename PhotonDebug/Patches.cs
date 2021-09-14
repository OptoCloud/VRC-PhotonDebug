using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace PhotonDebug
{
	public class Patches
	{
		public static string logFileName { get; private set; }

		private const String logFileDateFormat = "yyyyMMddTHHmmss";
		private static object logFileLock = new object();

		private static T FromByteArray<T>(Byte[] data)
		{
			T result;
			if (data == null)
			{
				result = default;
			}
			else
			{
				var binaryFormatter = new BinaryFormatter();
				using (var memoryStream = new MemoryStream(data))
				{
					Object obj = binaryFormatter.Deserialize(memoryStream);
					result = (T)((Object)obj);
				}
			}
			return result;
		}
		private static Byte[] ToByteArray(Object obj)
		{
			Byte[] result = null;
			if (obj != null)
			{
				var binaryFormatter = new BinaryFormatter();
				var memoryStream = new MemoryStream();
				binaryFormatter.Serialize(memoryStream, obj);
				result = memoryStream.ToArray();
			}
			return result;
		}
		private static T FromIL2CPPToManaged<T>(Object obj)
		{
			return FromByteArray<T>(ToByteArray(obj));
		}

		private static HarmonyLib.HarmonyMethod GetPatch(String name)
		{
			return new HarmonyLib.HarmonyMethod(typeof(Patches).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic));
		}

		private static readonly Il2CppSystem.Type Type_Il2CppHashtable = UnhollowerRuntimeLib.Il2CppType.Of<Il2CppSystem.Collections.Hashtable>();
		private static readonly Il2CppSystem.Type Type_Il2CppDictionary_Entry = UnhollowerRuntimeLib.Il2CppType.Of<Il2CppSystem.Collections.DictionaryEntry>();
		private static readonly Il2CppSystem.Type Type_Il2CppDictionary_Byte_Il2CppObject = UnhollowerRuntimeLib.Il2CppType.Of<Il2CppSystem.Collections.Generic.Dictionary<Byte, Il2CppSystem.Object>>();
		private static readonly Il2CppSystem.Type Type_Il2CppDictionary_String_Il2CppObject = UnhollowerRuntimeLib.Il2CppType.Of<Il2CppSystem.Collections.Generic.Dictionary<String, Il2CppSystem.Object>>();
		private static JObject ParseIl2CppObject(Il2CppSystem.Object obj)
		{
			JObject jobj;

			try
			{
				if (obj != null)
				{
					Il2CppSystem.Type objType = obj.GetIl2CppType();
					String objTypeString = objType.ToString();
					jobj = new JObject();

					try
					{
						if (objType == Type_Il2CppHashtable)
						{
							jobj["type"] = "HashTable";

							Il2CppSystem.Collections.Hashtable hashtable = obj.Cast<Il2CppSystem.Collections.Hashtable>();

							Il2CppSystem.Collections.IEnumerator enumerator = hashtable.System_Collections_IEnumerable_GetEnumerator();

							var jdata = new JObject();
							while (enumerator.MoveNext())
							{
								var entry = enumerator.Current.Cast<Il2CppSystem.Collections.DictionaryEntry>();
								jdata[entry.Key.ToString()] = ParseIl2CppObject(entry.Value);
							}

							jobj["data"] = jdata;
						}
						else if (objType == Type_Il2CppDictionary_String_Il2CppObject)
						{
							jobj["type"] = "Dictionary<String, Object>";

							var dictionary = obj.Cast<Il2CppSystem.Collections.Generic.Dictionary<String, Il2CppSystem.Object>>();

							var jdata = new JObject();
							foreach (var entry in dictionary)
							{
								jdata[entry.Key] = ParseIl2CppObject(entry.Value);
							}

							jobj["data"] = jdata;
						}
						else if (objType == Type_Il2CppDictionary_Byte_Il2CppObject)
						{
							jobj["type"] = "Dictionary<Byte, Object>";

							var dictionary = obj.Cast<Il2CppSystem.Collections.Generic.Dictionary<Byte, Il2CppSystem.Object>>();

							var jdata = new JObject();
							foreach (var entry in dictionary)
							{
								jdata[entry.Key.ToString()] = ParseIl2CppObject(entry.Value);
							}

							jobj["data"] = jdata;
						}
						else if (objType == Type_Il2CppDictionary_Entry)
						{
							jobj["type"] = "DictionaryEntry";

							var entry = obj.Cast<Il2CppSystem.Collections.DictionaryEntry>();

							jobj["data"] = new JObject
							{
								new JProperty("key", ParseIl2CppObject(entry.Key)),
								new JProperty("value", ParseIl2CppObject(entry.Key))
							};
						}
						else if (objTypeString == "System.Boolean")
						{
							jobj["type"] = "Boolean";
							jobj["data"] = Il2CppSystem.Convert.ToBoolean(obj);
						}
						else if (objTypeString == "System.Byte")
						{
							jobj["type"] = "Byte";
							jobj["data"] = Il2CppSystem.Convert.ToByte(obj);
						}
						else if (objTypeString == "System.Int32")
						{
							jobj["type"] = "Int32";
							jobj["data"] = Il2CppSystem.Convert.ToInt32(obj);
						}
						else if (objTypeString == "System.Double")
						{
							jobj["type"] = "Double";
							jobj["data"] = Il2CppSystem.Convert.ToDouble(obj);
						}
						else if (objTypeString == "System.String")
						{
							jobj["type"] = "String";
							jobj["data"] = Il2CppSystem.Convert.ToString(obj);
						}
						else if (objTypeString == "System.Byte[]")
						{
							jobj["type"] = "Byte[]";

							/*
							UnhollowerBaseLib.Il2CppArrayBase<Byte> a = obj.Cast<UnhollowerBaseLib.Il2CppArrayBase<Byte>>();
							Byte[] cpy = new Byte[a.Count];
							for (int i = 0; i < cpy.Length; i++)
							{
								cpy[i] = a[i];
							}
							Console.WriteLine(cpy.Length + " " + BitConverter.ToString(cpy).Replace("-", ""));
							*/
							// The following code should act identically to the above code, but faster
							byte[] array;
							unsafe
							{
								IntPtr objPtr = obj.Pointer;
								Int32 arraySize = *(Int32*)(objPtr + 24);
								array = new byte[arraySize];
								Marshal.Copy(objPtr + 32, array, 0, arraySize);
							}
							//Console.WriteLine(array.Length + " " + BitConverter.ToString(array).Replace("-", ""));
							jobj["data"] = Convert.ToBase64String(array);
						}
						else if (objTypeString == "System.Int32[]")
						{
							jobj["type"] = "Int32[]";

							JArray array = new JArray();
							Il2CppSystem.Collections.IEnumerator e1 = obj.Cast<Il2CppSystem.Collections.IEnumerable>().GetEnumerator();

							while (e1.MoveNext())
							{
								array.Add(Il2CppSystem.Convert.ToInt32(e1.Current));
							}

							jobj["data"] = array;
						}
						else if (objTypeString == "System.String[]")
						{
							jobj["type"] = "String[]";

							JArray array = new JArray();
							Il2CppSystem.Collections.IEnumerator e1 = obj.Cast<Il2CppSystem.Collections.IEnumerable>().GetEnumerator();

							while (e1.MoveNext())
							{
								array.Add(Il2CppSystem.Convert.ToString(e1.Current));
							}

							jobj["data"] = array;
						}
						else if (objTypeString == "System.String[][]")
						{
							jobj["type"] = "String[][]";

							JArray outerArray = new JArray();
							Console.WriteLine(obj.ToString());
							Il2CppSystem.Collections.IEnumerator e1 = obj.Cast<Il2CppSystem.Collections.IEnumerable>().GetEnumerator();

							while (e1.MoveNext())
							{
								JArray innerArray = new JArray();

								UnhollowerBaseLib.Il2CppStringArray e2 = e1.Current.Cast<UnhollowerBaseLib.Il2CppStringArray>();
								Console.WriteLine(e2.Length);
								foreach (var str in e2)
								{
									innerArray.Add(str);
								}

								outerArray.Add(innerArray);
							}

							jobj["data"] = outerArray;
						}
						else if (objTypeString == "System.Object[]")
						{
							jobj["type"] = "Object[]";

							JArray array = new JArray();
							Il2CppSystem.Collections.IEnumerator e1 = obj.Cast<Il2CppSystem.Collections.IEnumerable>().GetEnumerator();

							while (e1.MoveNext())
							{
								array.Add(ParseIl2CppObject(e1.Current));
							}

							jobj["data"] = array;
						}
						else
						{
							jobj["type"] = objTypeString;
							jobj["data"] = obj.ToString();
						}
					}
					catch (Exception ex)
					{
						if (!jobj.ContainsKey("type"))
						{
							jobj["type"] = objTypeString;
						}
						jobj["exception"] = ex.ToString();
					}
				}
				else
				{
					jobj = new JObject(
						new JProperty("type", "Object"),
						new JProperty("data", "null")
						);
				}
			}
			catch (Exception ex)
			{
				jobj = new JObject(
					new JProperty("type", "Object"),
					new JProperty("exception", ex.ToString())
					);
			}

			return jobj;
		}

		private static Byte[] stupid = new Byte[] { (Byte)',', (Byte)'\n', (Byte)']' };
		private static void LogJson(JObject obj)
		{
			Byte[] jsondata = Encoding.UTF8.GetBytes(obj.ToString(Formatting.None));

			lock (logFileLock)
			{
				using (var fileStream = new FileStream(logFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
				{
					// Don't ask, this is to make sure that json data get appended properly
					if (fileStream.Length <= 3)
					{
						Console.WriteLine("First write!");
						fileStream.Seek(2, SeekOrigin.Begin);
					}
					else
					{
						fileStream.Seek(-3, SeekOrigin.End);
						if (fileStream.ReadByte() == '}')
						{
							fileStream.Write(stupid, 0, 2);
						}
					}

					fileStream.WriteByte((byte)'\t');
					fileStream.Write(jsondata, 0, jsondata.Length);
					fileStream.Write(stupid, 1, 2);
				}
			}
		}

		internal static void Init()
		{
			logFileName = $"PhotonDebug/log_{DateTime.UtcNow.ToString(logFileDateFormat)}.json";
			File.WriteAllText(logFileName, "[\n]");

			MelonLogger.Msg("Creating HarmonyInstance");

			var harmonyInstane = new HarmonyLib.Harmony("PhotonDebug");
			harmonyInstane.Patch(typeof(ExitGames.Client.Photon.PhotonPeer).GetMethod("Connect", BindingFlags.Public | BindingFlags.Instance), GetPatch("ConnectPatch"));
			harmonyInstane.Patch(typeof(ExitGames.Client.Photon.PhotonPeer).GetMethod("SendOperation", BindingFlags.Public | BindingFlags.Instance), GetPatch("SendOperationPatch"));
			harmonyInstane.Patch(typeof(Photon.Realtime.LoadBalancingClient).GetMethod("OnEvent", BindingFlags.Public | BindingFlags.Instance), GetPatch("OnEventPatch"));
			harmonyInstane.Patch(typeof(Photon.Realtime.LoadBalancingClient).GetMethod("OnOperationResponse", BindingFlags.Public | BindingFlags.Instance), GetPatch("OnOperationResponsePatch"));
			harmonyInstane.Patch(typeof(Photon.Realtime.LoadBalancingClient).GetMethod("OnStatusChanged", BindingFlags.Public | BindingFlags.Instance), GetPatch("OnStatusChangedPatch"));
			harmonyInstane.Patch(typeof(Photon.Realtime.Player).GetMethod("Method_Public_Void_Hashtable_0", BindingFlags.Public | BindingFlags.Instance), GetPatch("SetAppSettingsPatch"));
		}

		[MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
		private static void ConnectPatch(string serverAddress, string proxyServerAddress, string applicationName, Object custom)
		{
			var sinceEpoch = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1));

			LogJson(new JObject(
					new JProperty("utc_time", sinceEpoch.TotalSeconds),
					new JProperty("patch_name", "ConnectPatch"),
					new JProperty("patch_args", new JObject(
						new JProperty("serverAddress", serverAddress),
						new JProperty("proxyServerAddress", proxyServerAddress),
						new JProperty("applicationName", applicationName),
						new JProperty("custom", custom)
					))
				));
		}

		[MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
		private static void SendOperationPatch(byte operationCode, Il2CppSystem.Collections.Generic.Dictionary<byte, Il2CppSystem.Object> operationParameters, ExitGames.Client.Photon.SendOptions sendOptions)
		{
			var sinceEpoch = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1));

			LogJson(new JObject(
					new JProperty("utc_time", sinceEpoch.TotalSeconds),
					new JProperty("patch_name", "SendOperationPatch"),
					new JProperty("patch_args", new JObject(
						new JProperty("operationCode", operationCode),
						new JProperty("operationParameters", ParseIl2CppObject(operationParameters)),
						new JProperty("sendOptions", new JObject(
							new JProperty("Channel", sendOptions.Channel),
							new JProperty("DeliveryMode", sendOptions.DeliveryMode.ToString()),
							new JProperty("Encrypt", sendOptions.Encrypt),
							new JProperty("Reliability", sendOptions.Reliability)
							)
						))
					)
				));
		}

		[MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
		private static void OnEventPatch(ExitGames.Client.Photon.EventData param_1)
		{
			var sinceEpoch = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1));

			LogJson(new JObject(
					new JProperty("utc_time", sinceEpoch.TotalSeconds),
					new JProperty("patch_name", "OnEventPatch"),
					new JProperty("patch_args", new JObject(
						new JProperty("eventData", new JObject(
							new JProperty("Code", param_1.Code),
							new JProperty("Parameters", ParseIl2CppObject(param_1.Parameters)),
							new JProperty("SenderKey", param_1.SenderKey),
							new JProperty("sender", param_1.sender),
							new JProperty("CustomDataKey", param_1.CustomDataKey),
							new JProperty("customData", ParseIl2CppObject(param_1.customData)),
							new JProperty("Sender", param_1.Sender),
							new JProperty("CustomData", ParseIl2CppObject(param_1.CustomData))
							))
					))
				));
		}

		[MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
		private static void OnOperationResponsePatch(ExitGames.Client.Photon.OperationResponse param_1)
		{
			var sinceEpoch = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1));

			LogJson(new JObject(
					new JProperty("utc_time", sinceEpoch.TotalSeconds),
					new JProperty("patch_name", "OnOperationResponsePatch"),
					new JProperty("patch_args", new JObject(
						new JProperty("operationResponse", new JObject(
							new JProperty("OperationCode", param_1.OperationCode),
							new JProperty("ReturnCode", param_1.ReturnCode),
							new JProperty("DebugMessage", param_1.DebugMessage),
							new JProperty("Parameters", ParseIl2CppObject(param_1.Parameters))
							))
					))
				));
		}

		[MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
		private static void OnStatusChangedPatch(ExitGames.Client.Photon.StatusCode param_1)
		{
			var sinceEpoch = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1));

			LogJson(new JObject(
					new JProperty("utc_time", sinceEpoch.TotalSeconds),
					new JProperty("patch_name", "OnStatusChangedPatch"),
					new JProperty("patch_args", new JObject(
						new JProperty("statusCode", $"{(int)param_1} ({param_1})")
					))
				));
		}

		[MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
		private static void SetAppSettingsPatch(Photon.Realtime.AppSettings param_1)
		{
			var sinceEpoch = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1));

			LogJson(new JObject(
					new JProperty("utc_time", sinceEpoch.TotalSeconds),
					new JProperty("patch_name", "SetAppSettingsPatch"),
					new JProperty("patch_args", new JObject(
						new JProperty("appSettings", new JObject(
							new JProperty("NetworkLogging", param_1.NetworkLogging),
							new JProperty("AppIdChat", param_1.AppIdChat),
							new JProperty("AppIdVoice", param_1.AppIdVoice),
							new JProperty("AppVersion", param_1.AppVersion),
							new JProperty("UseNameServer", param_1.UseNameServer),
							new JProperty("FixedRegion", param_1.FixedRegion),
							new JProperty("BestRegionSummaryFromStorage", param_1.BestRegionSummaryFromStorage),
							new JProperty("IsMasterServerAddress", param_1.IsMasterServerAddress),
							new JProperty("Server", param_1.Server),
							new JProperty("AppIdRealtime", param_1.AppIdRealtime),
							new JProperty("Protocol", param_1.Protocol),
							new JProperty("EnableProtocolFallback", param_1.EnableProtocolFallback),
							new JProperty("AuthMode", param_1.AuthMode),
							new JProperty("IsBestRegion", param_1.IsBestRegion),
							new JProperty("EnableLobbyStatistics", param_1.EnableLobbyStatistics),
							new JProperty("Port", param_1.Port),
							new JProperty("ProxyServer", param_1.ProxyServer),
							new JProperty("IsDefaultPort", param_1.IsDefaultPort),
							new JProperty("IsDefaultNameServer", param_1.IsDefaultNameServer)
							))
					))
				));
		}
	}
}
