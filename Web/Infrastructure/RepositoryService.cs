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

using System.Configuration;
using Guvnor.Configuration;

namespace Guvnor.Infrastructure {
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public class RepositoryService {
		public IEnumerable<Repository> GetAllRepositories() {
			var config = ConfigurationManager.GetSection("guvnor") as GuvnorConfigurationSection;
			if(config==null) return Enumerable.Empty<Repository>();
			return config.Projects
				.Select(RepositoryFromConfiguration)
				.ToList();
		}

		public Repository GetRepository(string project) {
			var config = ConfigurationManager.GetSection("guvnor") as GuvnorConfigurationSection;
			if(config==null) return null;
			var projectCfg = config.Projects.FirstOrDefault(t => t.Name==project);
			if(projectCfg==null) return null;
			return RepositoryFromConfiguration(projectCfg);
		}

		private static Repository RepositoryFromConfiguration(ProjectConfiguration project) {
			return Repository.Open(
				project.Name,
				new DirectoryInfo(project.Path),
				project.Parameters.ToDictionary(p => (string)p.Key, p => p.Value));
		}
	}
}