using System.Collections.Specialized;
using GitSharp.Core.RevPlot;
using GitSharp.Core.RevWalk;
using GitSharp.Core.Util;

namespace Guvnor.Infrastructure {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using GitSharp.Core;
	using GitSharp.Core.Transport;


	public class Repository {
		public IDictionary<string,string> Parameters { get; set; }
		public string BuildFile { get; set; }
		private DirectoryInfo directory;

		public static Repository Open(string name, DirectoryInfo directory, IDictionary<string,string> parameters) {

			return null;
		}

		public Repository(string name, DirectoryInfo directory, IDictionary<string,string> parameters) {
			if(!GitSharp.Repository.IsValid(directory.FullName)) {
				throw new InvalidOperationException(directory.FullName+" is not a valid Git repository");
			}
			Parameters = parameters ?? new Dictionary<string, string>();
			Name = name;
			this.directory = directory;
		}

		public void AdvertiseUploadPack(Stream output) {
			using (var repository = GetRepository()) {
				var pack = new UploadPack(repository);
				pack.sendAdvertisedRefs(new RefAdvertiser.PacketLineOutRefAdvertiser(new PacketLineOut(output)));
			}
		}

		public void AdvertiseReceivePack(Stream output) {
			using (var repository = GetRepository()) {
				var pack = new ReceivePack(repository);
				pack.SendAdvertisedRefs(new RefAdvertiser.PacketLineOutRefAdvertiser(new PacketLineOut(output)));
			}
		}

		public void Receive(Stream inputStream, Stream outputStream) {
			using (var repository = GetRepository()) {
				var pack = new ReceivePack(repository);
				pack.setPostReceiveHook(new PostReceiveFileHook(FullPath));
				pack.setBiDirectionalPipe(false);
				pack.receive(inputStream, outputStream, outputStream);
			}
		}

		public void Upload(Stream inputStream, Stream outputStream) {
			using (var repository = GetRepository()) {
				using (var pack = new UploadPack(repository)) {
					pack.setBiDirectionalPipe(false);
					pack.Upload(inputStream, outputStream, outputStream);
				}
			}
		}

		public void Checkout(string hash) {
			using(var repository = new GitSharp.Repository(FullPath)) {
				repository.CurrentBranch.Reset(hash, GitSharp.ResetBehavior.Hard);
			}
		}

		public IEnumerable<CommitInfo> ListCommits() {
			using(var repository = new GitSharp.Repository(FullPath)) {
				var w = new RevWalk(repository);
				w.markStart(((GitSharp.Core.Repository)repository).getAllRefsByPeeledObjectId().Keys.Select(w.parseCommit));
				return w.Select(t => new CommitInfo { 
					Id = t.getId().Name,
					Date = t.AsCommit(w).Author.When.MillisToDateTimeOffset(t.AsCommit(w).Author.TimeZoneOffset),
					Message = t.getShortMessage()
				}).ToArray();
			}
		}

		public CommitInfo GetLatestCommit() {
			using (var repository = new GitSharp.Repository(FullPath)) {
				var commit = repository.Head.CurrentCommit;

				if (commit == null) {
					return null;
				}

				return new CommitInfo {
					Id = commit.ShortHash,
					Message = commit.Message,
					Date = commit.CommitDate.DateTime
				};
			}
		}
		public class CommitInfo {
			public string Id { get; set; }
			public string Message { get; set; }
			public DateTimeOffset Date { get; set; }
		}

		private GitSharp.Core.Repository GetRepository() {
			return GitSharp.Core.Repository.Open(directory);
		}

		public string Name {
			get; set;
		}

		public string FullPath {
			get { return directory.FullName; }
		}

		public string GitDirectory() {
			if(FullPath.EndsWith(".git", StringComparison.OrdinalIgnoreCase)) {
				return FullPath;
			}

			return Path.Combine(FullPath, ".git");
		}

		public void UpdateServerInfo() {
			using (var rep = GetRepository()) {
				if (rep.ObjectDatabase is ObjectDirectory) {
					RefWriter rw = new SimpleRefWriter(rep, rep.getAllRefs().Values);
					rw.writePackedRefs();
					rw.writeInfoRefs();

					var packs = GetPackRefs(rep);
					WriteInfoPacks(packs, rep);
				}
			}
		}

		private void WriteInfoPacks(IEnumerable<string> packs, GitSharp.Core.Repository repository) {

			var w = new StringBuilder();

			foreach (string pack in packs) {
				w.Append("P ");
				w.Append(pack);
				w.Append('\n');
			}

			var infoPacksPath = Path.Combine(repository.ObjectsDirectory.FullName, "info/packs");
			var encoded = Encoding.ASCII.GetBytes(w.ToString());


			using (Stream fs = File.Create(infoPacksPath)) {
				fs.Write(encoded, 0, encoded.Length);
			}
		}

		private IEnumerable<string> GetPackRefs(GitSharp.Core.Repository repository) {
			var packDir = repository.ObjectsDirectory.GetDirectories().SingleOrDefault(x => x.Name == "pack");

			if(packDir == null) {
				return Enumerable.Empty<string>();
			}

			return packDir.GetFiles("*.pack").Select(x => x.Name).ToList();
		}
	}
}