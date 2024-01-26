using System;
using System.Reflection;

using UIKit;
using Foundation;

namespace MonoTouch.Dialog
{
	public enum RefreshViewStatus {
		ReleaseToReload,
		PullToReload,
		Loading
	}

	// This cute method will be added to UIImage.FromResource, but for old installs 
	// make a copy here
	internal static class Util {
		public static UIImage? FromResource (Assembly? assembly, string name)
		{
			if (name == null)
				throw new ArgumentNullException (nameof(name));
			assembly ??= Assembly.GetCallingAssembly ();
			var stream = assembly.GetManifestResourceStream (name);
			if (stream == null)
				return null;

			try
			{
				using var data = NSData.FromStream(stream);
				return data != null ? UIImage.LoadFromData(data) : null;
			} finally {
				stream.Dispose ();
			}
		}
	}
	
	public class SearchChangedEventArgs : EventArgs {
		public SearchChangedEventArgs (string text) 
		{
			Text = text;
		}
		public string Text { get; set; }
	}
}
