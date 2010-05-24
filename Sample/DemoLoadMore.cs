using System;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

namespace Sample
{
	public partial class AppDelegate
	{
		public void DemoLoadMore () 
		{
			Section loadMore = new Section();
			
			loadMore.Add(new StringElement("Element 1"));
			loadMore.Add(new StringElement("Element 2"));
			loadMore.Add(new StringElement("Element 3"));
						
			loadMore.Add(new LoadMoreElement("Load More Elements...", "Loading Elements...", delegate {
				
				System.Threading.Thread.Sleep(2000);
				
				navigation.BeginInvokeOnMainThread(delegate {
					
					loadMore.Insert(loadMore.Count - 1, new StringElement("Element " + (loadMore.Count + 1)),
				    		            new StringElement("Element " + (loadMore.Count + 2)),
				            		    new StringElement("Element " + (loadMore.Count + 3)));
					
				});
				
			}, UIFont.BoldSystemFontOfSize(14.0f), UIColor.Blue));
						
			var root = new RootElement("Load More") {
				loadMore
			};
			
			var dvc = new DialogViewController (root, true);
			navigation.PushViewController (dvc, true);
		}
	}
}

