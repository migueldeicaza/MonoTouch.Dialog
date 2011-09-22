using System;
using MonoTouch.Foundation;

namespace MonoTouch.Dialog
{
	public partial class Element
	{
		public object DataItem;
		
		public NSAction Deleted;
		
		public void DeleteSource()
		{
			if(Deleted != null)
				Deleted();
			
		}
		
	}
	
	public partial class RadioElement
	{
		public RadioElement(string caption, string group, NSAction tapped) : base(caption, group)
		{
			Tapped += tapped;
		
		}
		
	}
	
}

