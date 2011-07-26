using System;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.GameKit;

namespace MonoTouch.Dialog
{
	public class GameKitAchievementsElement : GameKitElement
	{
		static NSString mkey = new NSString ("GameKitAchievementsElement");
		GKAchievementViewController achievementController;
		
		public GameKitAchievementsElement ( string aCaption ) : base(aCaption)
		{
			
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{			
			// Lazy load it
			if ( achievementController == null )
			{
				achievementController = new GKAchievementViewController();
			}

		    if (achievementController != null)		
		    {					
				achievementController.DidFinish += delegate(object sender, EventArgs e) 
				{									 
					achievementController.DismissModalViewControllerAnimated(true);
				};
				dvc.PresentModalViewController(achievementController, true);						
		    }
		}
	}
}
