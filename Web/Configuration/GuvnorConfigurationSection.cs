using System;
using System.Configuration;

namespace Guvnor.Configuration {
	public class GuvnorConfigurationSection : ConfigurationSection {
		[ConfigurationProperty("projects", IsRequired=true, IsKey=true)]
		public ProjectConfigurationCollection Projects {
			get { return (ProjectConfigurationCollection)this["projects"]; }
			set { this["projects"] = value; }
		}
		[ConfigurationProperty("ReceivePack", DefaultValue=true)]
		public bool ReceivePack { 
			get { return (bool) this["ReceivePack"]; } 
			set { this["ReceivePack"] = value; } 
		}
		[ConfigurationProperty("UploadPack", DefaultValue=true)]
		public bool UploadPack { 
			get { return (bool) this["UploadPack"]; } 
			set { this["UploadPack"] = value; } 
		}
		[ConfigurationProperty("RunHooksSilently", DefaultValue=false)]
		public bool RunHooksSilently { 
			get { return (bool) this["RunHooksSilently"]; } 
			set { this["RunHooksSilently"] = value; } 
		}
		[ConfigurationProperty("HookTimeout", DefaultValue="0:10:00")]
		public TimeSpan HookTimeout { 
			get { return (TimeSpan) this["HookTimeout"]; } 
			set { this["HookTimeout"] = value; } 
		}
		[ConfigurationProperty("buildFile", DefaultValue="guvnor.msbuild")]
		public string BuildFile {
			get { return (string)this["buildFile"]; }
			set { this["buildFile"] = value; }
		}
		[ConfigurationProperty("msbuildPath", DefaultValue=@"C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe")]
		public string MSBuildPath {
			get { return (string)this["msbuildPath"]; }
			set { this["msbuildPath"] = value; }
		}
	}
	[ConfigurationCollection(typeof(ProjectConfigurationCollection))]
	public class ProjectConfigurationCollection : ConfigurationElementCollection<ProjectConfiguration> {
	}
	public class ProjectConfiguration : KeyedConfigurationElement {
		public ProjectConfiguration() { }
		public ProjectConfiguration(String name) { this["name"] = name; }
		public override object Key {
			get { return Name; }
		}
		[ConfigurationProperty("name", IsRequired=true, IsKey=true)]
		public string Name {
			get { return (string)this["name"]; }
			set { this["name"] = value; }
		}
		[ConfigurationProperty("path", IsRequired=true)]
		public string Path {
			get { return (string)this["path"]; }
			set { this["path"] = value; }
		}
		[ConfigurationProperty("parameters", IsRequired=true)]
		public ParameterConfigurationCollection Parameters {
			get { return (ParameterConfigurationCollection)this["parameters"]; }
			set { this["parameters"] = value; }
		}
		[ConfigurationProperty("buildFile")]
		public string BuildFile {
			get { return (string)this["buildFile"]; }
			set { this["buildFile"] = value; }
		}
	}
	[ConfigurationCollection(typeof(ParameterConfigurationCollection))]
	public class ParameterConfigurationCollection : ConfigurationElementCollection<ParameterConfiguration> {
	}
	public class ParameterConfiguration : KeyedConfigurationElement {
		public ParameterConfiguration() { }
		public ParameterConfiguration(String name) { this["name"] = name; }
		public override object Key {
			get { return Name; }
		}
		[ConfigurationProperty("name", IsRequired=true, IsKey=true)]
		public string Name {
			get { return (string)this["name"]; }
			set { this["name"] = value; }
		}
		[ConfigurationProperty("value", IsRequired=true)]
		public string Value {
			get { return (string)this["value"]; }
			set { this["value"] = value; }
		}
	}
}