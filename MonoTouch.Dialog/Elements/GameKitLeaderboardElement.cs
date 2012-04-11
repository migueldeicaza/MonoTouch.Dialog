using System;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.GameKit;

namespace MonoTouch.Dialog
{
	public class GameKitLeaderboardElement : GameKitElement
	{
		static NSString mkey = new NSString ("GameKitLeaderboardElement");
		GKLeaderboardViewController leaderboardController;
		
		public GameKitLeaderboardElement ( string aCaption ) : base(aCaption)
		{
			
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{			
			// Lazy load it
			if ( leaderboardController == null )
			{			    	
				leaderboardController = new GKLeaderboardViewController();
			}

		    if (leaderboardController != null)			
		    {
				leaderboardController.DidFinish += delegate(object sender, EventArgs e) 
				{
					leaderboardController.DismissModalViewControllerAnimated(true);
				};
				dvc.PresentModalViewController(leaderboardController, true);
		    }
		}
	}
}


