using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Mtgdb.Downloader
{
	public class Megatools
	{
		public Megatools(AppSourceConfig config)
		{
			_megatoolsUrl = config.MegatoolsUrl;
		}

		public void Download(string storageUrl, string targetDirectory, string name, bool silent = false, int? timeoutSec = null)
		{
			if (_process != null)
				throw new InvalidOperationException("Download is already running. Use another instance to start new download.");

			_silent = silent;

			lock (_syncSelfdownload)
				if (!File.Exists(MegadlExePath))
				{
					var webClient = new GdriveWebClient();
					webClient.DownloadAndExtract(_megatoolsUrl, AppDir.Update, "megatools.7z");
				}

			DownloadedCount = 0;

			string arguments;

			if (targetDirectory.Contains(' '))
				arguments = $@"--path=""{targetDirectory}"" --print-names {storageUrl}";
			else
				arguments = $@"--path={targetDirectory} --print-names {storageUrl}";

			_process = new Process
			{
				StartInfo = new ProcessStartInfo(MegadlExePath, arguments)
				{
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				},

				EnableRaisingEvents = true
			};

			_process.OutputDataReceived += downloadOutputReceived;
			_process.ErrorDataReceived += downloadErrorReceived;
			
			if (!_silent)
				Console.WriteLine("Downloading {0} from {1} to {2}", name, storageUrl, targetDirectory);

			AppDomain.CurrentDomain.ProcessExit += processExit;
			_process.Start();
			_process.BeginOutputReadLine();
			_process.BeginErrorReadLine();

			if (timeoutSec.HasValue)
				_process.WaitForExit(timeoutSec.Value * 1000);
			else
				_process.WaitForExit();

			Abort();
		}

		private void processExit(object sender, EventArgs e)
		{
			Abort();
		}

		public void Abort()
		{
			if (_process == null)
				return;

			_process.OutputDataReceived -= downloadOutputReceived;
			_process.ErrorDataReceived -= downloadErrorReceived;

			if (!_process.HasExited)
				_process.Kill();

			AppDomain.CurrentDomain.ProcessExit -= processExit;

			_process = null;
		}

		private static void downloadErrorReceived(object sender, DataReceivedEventArgs e)
		{
			if (string.IsNullOrEmpty(e.Data))
				return;

			if (e.Data.StartsWith("ERROR: File already exists at "))
				return;

			Console.WriteLine(e.Data);
		}

		private void downloadOutputReceived(object sender, DataReceivedEventArgs e)
		{
			DownloadedCount++;
			FileDownloaded?.Invoke();

			if (_silent)
				return;

			if (string.IsNullOrEmpty(e.Data))
				return;

			if (e.Data.StartsWith("F "))
				return;

			if (e.Data.StartsWith("D "))
				return;

			Console.WriteLine(e.Data);
		}



		private static string MegadlExePath =>
			AppDir.Update.AddPath(@"megatools-1.9.98-win32\megadl.exe");

		public int DownloadedCount { get; private set; }

		public event Action FileDownloaded;

		private Process _process;
		private bool _silent;
		private string _megatoolsUrl;
		private static object _syncSelfdownload = new object();
	}
}