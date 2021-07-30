using MelonLoader;
using System.IO;

[assembly: MelonInfo(typeof(PhotonDebug.PhotonDebug), "PhotonDebug", "1.0", "OptoCloud")]

namespace PhotonDebug
{
	public class PhotonDebug : MelonMod
	{
		public override void OnApplicationStart()
		{
			Directory.CreateDirectory("PhotonDebug");

			MelonLogger.Msg("Loading patches...");
			Patches.Init();
			MelonLogger.Msg("Loaded patches!");
		}
	}
}
