namespace Guvnor.Infrastructure {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using GitSharp.Core.Transport;

	public class PostReceiveFileHook : IPostReceiveHook {
		public string FullPath { get; set; }
		public TimeSpan Timeout { get; set; }

		public PostReceiveFileHook(string fullPath) {
			FullPath = fullPath;
			Timeout = TimeSpan.FromMinutes(10);
		}

		public void OnPostReceive(ReceivePack rp, ICollection<ReceiveCommand> commands) {
			var hooksDirectory = new DirectoryInfo(Path.Combine(FullPath, "hooks"));
			if(!hooksDirectory.Exists) {
				hooksDirectory = new DirectoryInfo(Path.Combine(FullPath, ".git/hooks"));
				if(!hooksDirectory.Exists) return;
			}
			var hooks = hooksDirectory
				.GetFiles("post-receive*")
				.Where(hook => !hook.FullName.EndsWith(".sample"));
			foreach (var hook in hooks) {
				Run(rp, hook.FullName);
			}
		}

		private void Run(ReceivePack rp, string hook) {
			var processStartInfo = new ProcessStartInfo {
				FileName = hook, UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true,
				RedirectStandardOutput = true, RedirectStandardError = true,
				RedirectStandardInput = true,
				WorkingDirectory = FullPath
			};
			var appSettings = AppSettings.Current;
			using(var process = new Process { StartInfo = processStartInfo }) {
				if(!appSettings.RunHooksSilently) {
					process.OutputDataReceived += (d, r) => { if(!String.IsNullOrWhiteSpace(r.Data)) rp.sendMessage(r.Data); };
					process.ErrorDataReceived += (d, r) => { if(!String.IsNullOrWhiteSpace(r.Data)) rp.sendError(r.Data); };
				}
				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				foreach(var command in rp.getAllCommands()) {
					process.StandardInput.WriteLine(command.getOldId().Name + " " + command.getNewId().Name + " " +
						command.getRefName());
				}
				process.StandardInput.Close();
				process.WaitForExit((int) appSettings.HookTimeout.TotalMilliseconds);
				process.WaitForExit();
				process.Close();
			}
		}
	}
}