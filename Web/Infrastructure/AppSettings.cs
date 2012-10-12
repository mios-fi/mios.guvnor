#region License

// Copyright 2010 Jeremy Skinner (http://www.jeremyskinner.co.uk)
//  
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://github.com/JeremySkinner/git-dot-aspx

#endregion

using System.Collections.Generic;
using System.IO;
using Guvnor.Configuration;

namespace Guvnor.Infrastructure {
	using System;
	using System.Linq;
	using System.Configuration;

	public class AppSettings {
		public bool UploadPack { get; protected set; }
		public bool ReceivePack { get; protected set; }
		public bool RunHooksSilently { get; protected set; }
		public TimeSpan HookTimeout { get; protected set; }
		public IDictionary<string, Repository> Repositories { get; protected set; }

		static AppSettings current;
		public static AppSettings Current {
			get {
				return current ?? (current=FromAppConfig());
			}
		}

		public string MSBuildPath { get; set; }

		private static AppSettings FromAppConfig() {
			var config = ConfigurationManager.GetSection("guvnor") as GuvnorConfigurationSection;
			if(config==null) {
				throw new ConfigurationErrorsException("Missing 'guvnor' section in .config file");
			}
			var repositories = config.Projects
				.ToDictionary(
					t => t.Name,
					t => new Repository(
						t.Name, new DirectoryInfo(t.Path), t.Parameters.ToDictionary(p => (string)p.Key, p => p.Value)
					) {
 						BuildFile = t.BuildFile==""?config.BuildFile:t.BuildFile
					}
				);
			return new AppSettings { 
				ReceivePack = config.ReceivePack,
				UploadPack = config.UploadPack,
				HookTimeout = config.HookTimeout,
				RunHooksSilently = config.RunHooksSilently,
				Repositories = repositories,
				MSBuildPath = config.MSBuildPath
			};
		}
	}
}