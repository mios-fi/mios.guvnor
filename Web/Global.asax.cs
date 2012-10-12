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

namespace Guvnor {
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;
	using Guvnor.Infrastructure;
	using StructureMap;
	using StructureMap.Configuration.DSL;

	public class MvcApplication : HttpApplication {
		public static void RegisterRoutes(RouteCollection routes) {
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			routes.IgnoreRoute("favicon.ico");

			routes.MapRoute("Projects", "", new {controller = "Projects", action = "Index"});

			routes.MapRoute("info-refs", "{project}/info/refs",
			                new {controller = "InfoRefs", action = "Execute"},
			                new {method = new HttpMethodConstraint("GET")});

			routes.MapRoute("upload-pack", "{project}/git-upload-pack",
			                new {controller = "Rpc", action = "UploadPack"},
			                new {method = new HttpMethodConstraint("POST")});

			routes.MapRoute("receive-pack", "{project}/git-receive-pack",
			                new {controller = "Rpc", action = "ReceivePack"},
			                new {method = new HttpMethodConstraint("POST")});

			// Dumb protocol
			//routes.MapRoute("info-refs-dumb", "dumb/{project}/info/refs", new {controller = "Dumb", action = "InfoRefs"});
			routes.MapRoute("get-text-file", "{project}/HEAD", new{controller = "Dumb", action="GetTextFile" });
			routes.MapRoute("get-text-file2", "{project}/objects/info/alternates", new { controller = "Dumb", action = "GetTextFile" });
			routes.MapRoute("get-text-file3", "{project}/objects/info/http-alternates", new { controller = "Dumb", action = "GetTextFile" });

			routes.MapRoute("get-info-packs", "{project}/info/packs", new {controller = "Dumb", action = "GetInfoPacks"});

			routes.MapRoute("get-text-file4", "{project}/objects/info/{something}", new {controller = "Dumb", action = "GetTextFile"});

			routes.MapRoute("get-loose-object", "{project}/objects/{segment1}/{segment2}", 
				new {controller = "Dumb", action = "GetLooseObject"});

			routes.MapRoute("get-pack-file", "{project}/objects/pack/pack-{filename}.pack", 
				new { controller = "Dumb", action = "GetPackFile" });
			
			routes.MapRoute("get-idx-file", "{project}/objects/pack/pack-{filename}.idx", 
				new {controller = "Dumb", action = "GetIdxFile"});

			routes.MapRoute("projectExecute", "{project}/execute/{target}", new { Controller = "Projects", Action = "Execute" });
			routes.MapRoute("project", "{project}/{action}", new { Controller = "Projects", Action = "Item" });
		}

		protected void Application_Start() {
			AreaRegistration.RegisterAllAreas();

			RegisterRoutes(RouteTable.Routes);

			ControllerBuilder.Current.SetControllerFactory(new StructureMapControllerFactory());
		}
	}
}