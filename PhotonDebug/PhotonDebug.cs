using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;

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
