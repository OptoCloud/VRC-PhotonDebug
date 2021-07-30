using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

					if (objType == Type_Il2CppHashtable)
					{
						jobj["type"] = "HashTable";

						try
						{
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
						catch (Exception ex)
						{
							jobj["exception"] = ex.ToString();
						}
					}
					else if (objType == Type_Il2CppDictionary_String_Il2CppObject)
					{
						jobj["type"] = "Dictionary<String, Object>";

						try
						{
							var dictionary = obj.Cast<Il2CppSystem.Collections.Generic.Dictionary<String, Il2CppSystem.Object>>();

							var jdata = new JObject();
							foreach (var entry in dictionary)
							{
								jdata[entry.Key] = ParseIl2CppObject(entry.Value);
							}

							jobj["data"] = jdata;
						}
						catch (Exception ex)
						{
							jobj["exception"] = ex.ToString();
						}
					}
					else if (objType == Type_Il2CppDictionary_Byte_Il2CppObject)
					{
						jobj["type"] = "Dictionary<Byte, Object>";

						try
						{
							var dictionary = obj.Cast<Il2CppSystem.Collections.Generic.Dictionary<Byte, Il2CppSystem.Object>>();

							var jdata = new JObject();
							foreach (var entry in dictionary)
							{
								jdata[entry.Key.ToString()] = ParseIl2CppObject(entry.Value);
							}

							jobj["data"] = jdata;
						}
						catch (Exception ex)
						{
							jobj["exception"] = ex.ToString();
						}
					}
					else if (objType == Type_Il2CppDictionary_Entry)
					{
						jobj["type"] = "DictionaryEntry";

						try
						{
							var entry = obj.Cast<Il2CppSystem.Collections.DictionaryEntry>();

							jobj["data"] = new JObject
							{
								new JProperty("key", ParseIl2CppObject(entry.Key)),
								new JProperty("value", ParseIl2CppObject(entry.Key))
							};
						}
						catch (Exception ex)
						{
							jobj["exception"] = ex.ToString();
						}
					}
					else if (objTypeString == "System.String")
					{
						jobj["type"] = "String";

						try
						{
							jobj["data"] = Il2CppSystem.Convert.ToString(obj);
						}
						catch (Exception ex)
						{
							jobj["exception"] = ex.ToString();
						}
					}
					else if (objTypeString == "System.Byte[]")
					{
						jobj["type"] = "Byte[]";

						try
						{
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
						catch (Exception ex)
						{
							jobj["exception"] = ex.ToString();
						}
					}
					else if (objTypeString == "System.String[][]")
					{
						jobj["type"] = "String[][]";

						try
						{
							JArray outerArray = new JArray();
							UnhollowerBaseLib.Il2CppArrayBase<UnhollowerBaseLib.Il2CppStringArray> a = obj.Cast<UnhollowerBaseLib.Il2CppArrayBase<UnhollowerBaseLib.Il2CppStringArray>>();
							for (int i = 0; i < a.Count; i++)
							{
								JArray innerArray = new JArray();

								for (int j = 0; j < a[i].Length; j++)
								{
									innerArray[j] = a[i][j];
								}

								outerArray[i] = innerArray;
							}

							jobj["data"] = outerArray;
						}
						catch (Exception ex)
						{
							jobj["exception"] = ex.ToString();
						}
					}
					else if (objTypeString == "System.Int32")
					{
						jobj["type"] = "Int32";

						try
						{
							jobj["data"] = Il2CppSystem.Convert.ToInt32(obj);
						}
						catch (Exception ex)
						{
							jobj["exception"] = ex.ToString();
						}
					}
					else if (objTypeString == "System.Byte")
					{
						jobj["type"] = "Byte";

						try
						{
							jobj["data"] = Il2CppSystem.Convert.ToByte(obj);
						}
						catch (Exception ex)
						{
							jobj["exception"] = ex.ToString();
						}
					}
					else if (objTypeString == "System.Boolean")
					{
						jobj["type"] = "Boolean";

						try
						{
							jobj["data"] = Il2CppSystem.Convert.ToBoolean(obj);
						}
						catch (Exception ex)
						{
							jobj["exception"] = ex.ToString();
						}
					}
					else
					{
						jobj["type"] = objTypeString;

						try
						{
							jobj["data"] = obj.ToString();
						}
						catch (Exception ex)
						{
							jobj["exception"] = ex.ToString();
						}
					}
				}
				else
				{
					jobj = new JObject(
						new JProperty("type", "Il2CppSystem.Object"),
						new JProperty("data", "null")
						);
				}
			}
			catch (Exception ex)
			{
				jobj = new JObject(
					new JProperty("type", "Il2CppSystem.Object"),
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
		private static void OnEventPatch(ExitGames.Client.Photon.EventData eventData)
		{
			var sinceEpoch = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1));

			LogJson(new JObject(
					new JProperty("utc_time", sinceEpoch.TotalSeconds),
					new JProperty("patch_name", "OnEventPatch"),
					new JProperty("patch_args", new JObject(
						new JProperty("eventData", new JObject(
							new JProperty("Code", eventData.Code),
							new JProperty("Parameters", ParseIl2CppObject(eventData.Parameters)),
							new JProperty("SenderKey", eventData.SenderKey),
							new JProperty("sender", eventData.sender),
							new JProperty("CustomDataKey", eventData.CustomDataKey),
							new JProperty("CustomData", ParseIl2CppObject(eventData.customData)),
							new JProperty("Sender", eventData.Sender),
							new JProperty("CustomData", ParseIl2CppObject(eventData.CustomData))
							))
					))
				));
		}

		[MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
		private static void OnOperationResponsePatch(ExitGames.Client.Photon.OperationResponse operationResponse)
		{
			var sinceEpoch = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1));

			LogJson(new JObject(
					new JProperty("utc_time", sinceEpoch.TotalSeconds),
					new JProperty("patch_name", "OnOperationResponsePatch"),
					new JProperty("patch_args", new JObject(
						new JProperty("operationResponse", new JObject(
							new JProperty("OperationCode", operationResponse.OperationCode),
							new JProperty("ReturnCode", operationResponse.ReturnCode),
							new JProperty("DebugMessage", operationResponse.DebugMessage),
							new JProperty("Parameters", ParseIl2CppObject(operationResponse.Parameters))
							))
					))
				));
		}

		[MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
		private static void OnStatusChangedPatch(ExitGames.Client.Photon.StatusCode statusCode)
		{
			var sinceEpoch = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1));

			LogJson(new JObject(
					new JProperty("utc_time", sinceEpoch.TotalSeconds),
					new JProperty("patch_name", "OnStatusChangedPatch"),
					new JProperty("patch_args", new JObject(
						new JProperty("statusCode", $"{(int)statusCode} ({statusCode})")
					))
				));
		}
	}
}
