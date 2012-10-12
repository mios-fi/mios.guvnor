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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Web.Mvc;
using Guvnor.Infrastructure;
using System.Linq;
using Microsoft.Web.Administration;

namespace Guvnor.Controllers {

	public class ProjectsController : Controller {

		public ActionResult Index() {
			return View(new IndexModel {
				Projects = AppSettings.Current.Repositories.Values
					.Select(t => new IndexModel.Project { 
						Name = t.Name,
						Commit = t.ListCommits().Select(c=>new IndexModel.Commit {
							Id = c.Id,
							Message = c.Message,
							Date = c.Date
						}).FirstOrDefault()
					}).ToArray()
			});
		}
		public class IndexModel {
			public IEnumerable<Project> Projects { get; set; }

			public class Project {
				public string Name { get; set; }
				public Commit Commit { get; set; }
			}

			public class Commit {
				public string Id { get; set; }
				public string Message { get; set; }
				public DateTimeOffset Date { get; set; }
			}
		}

		public ActionResult Item(string project) {
			var repository = AppSettings.Current.Repositories[project];
			if(repository==null) {
				return new NotFoundResult();
			}
			using(var mgr = new ServerManager()) {
				return View(new ItemModel {
					Name = repository.Name,
					Commits = repository.ListCommits()
						.Take(10)
						.Select(t => new ItemModel.Commit {
							Id = t.Id,
							Message = t.Message,
							Date = t.Date
						}).ToArray()
				});
			}
		}

		public class ItemModel {
			public string Name { get; set; }
			public IEnumerable<Commit> Commits { get; set; }

			public class Commit {
				public string Id { get; set; }
				public string Message { get; set; }
				public DateTimeOffset Date { get; set; }

				public IDictionary<string,string> Tags { get; set; }
			}
		}
		
		public ActionResult Execute(string project, string target) {
			var repository = AppSettings.Current.Repositories[project];
			if(repository==null) {
				return new NotFoundResult();
			}
			var parameters = new Dictionary<string, string>(repository.Parameters);
			foreach(string key in Request.QueryString.Keys) {
				parameters[key] = Request.QueryString[key];
			}
			var parameterString = String.Join(" ", parameters.Select(t => "/p:"+t.Key+"="+t.Value).ToArray());
			var processStartInfo = new ProcessStartInfo {
				FileName = AppSettings.Current.MSBuildPath,
				Arguments = repository.BuildFile+" /m /nologo /t:"+target+" "+parameterString,
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden, 
				CreateNoWindow = true,
				RedirectStandardOutput = true, 
				RedirectStandardError = true,
				WorkingDirectory = repository.FullPath
			};
			var output = new StringBuilder();
			using(var process = new Process { StartInfo = processStartInfo }) {
				process.OutputDataReceived += (d, r) => { if(!String.IsNullOrWhiteSpace(r.Data)) output.AppendLine(r.Data); };
				process.ErrorDataReceived += (d, r) => { if(!String.IsNullOrWhiteSpace(r.Data)) output.AppendLine(r.Data); };
				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				process.WaitForExit(100000);
				process.Close();
			}
			return View("Execute",(object)output.ToString().Trim());
		}
	}
}