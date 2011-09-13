using System;
using MonoTouch.Dialog;

namespace MonoTouch.Dialog
{
	public interface IDialogViewModel
	{
		RootElement CreateRoot();			
	}
	
	public abstract class DialogViewModel : IDialogViewModel
	{
		public abstract RootElement CreateRoot();
	}
}

