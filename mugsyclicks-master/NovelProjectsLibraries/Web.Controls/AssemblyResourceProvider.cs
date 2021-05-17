using System;
using System.Web;
using System.Web.Hosting;
using System.IO;
using System.Reflection;

namespace NovelProjects.Web
{
	public class AssemblyResourceProvider : VirtualPathProvider
	{
		public AssemblyResourceProvider() { }
		private bool IsAppResourcePath(string virtualPath)
		{
			String checkPath = VirtualPathUtility.ToAppRelative(virtualPath);
			return checkPath.Contains("NovelProjects.Web");
		}
		public override bool FileExists(string virtualPath)
		{
			return (IsAppResourcePath(virtualPath) || base.FileExists(virtualPath));
		}
		public override VirtualFile GetFile(string virtualPath)
		{
			if (IsAppResourcePath(virtualPath))
				return new AssemblyResourceVirtualFile(virtualPath);
			else
				return base.GetFile(virtualPath);
		}
		public override System.Web.Caching.CacheDependency GetCacheDependency(string virtualPath, System.Collections.IEnumerable virtualPathDependencies, DateTime utcStart)
		{
			if (IsAppResourcePath(virtualPath))
				return null;
			else
				return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
		}
	}

	class AssemblyResourceVirtualFile : VirtualFile
	{
		string path;
		public AssemblyResourceVirtualFile(string virtualPath) : base(virtualPath)
		{
			path = VirtualPathUtility.ToAppRelative(virtualPath);
		}
		public override System.IO.Stream Open()
		{
			string assemblyName = "NovelProjects.Web.Controls.dll";
			string resourceName = path.Substring(path.IndexOf("NovelProjects.Web"));

			assemblyName = Path.Combine(HttpRuntime.BinDirectory, assemblyName);

			Assembly assembly = Assembly.LoadFile(assemblyName);
			if (assembly != null)
			{
				return assembly.GetManifestResourceStream(resourceName);
			}
			return null;
		}
	}
}
