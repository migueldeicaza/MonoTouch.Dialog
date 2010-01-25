using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace MonoTouch.Dialog
{
	public class DialogViewController : UITableViewController
	{
		UITableView tableView;
		RootElement root;
		bool pushing;
		bool dirty;
		
		class Source : UITableViewSource {
			DialogViewController container;
			RootElement root;
			
			public Source (DialogViewController container)
			{
				this.container = container;
				root = container.root;
			}
			
			public override int RowsInSection (UITableView tableview, int section)
			{
				var s = root.Sections [section];
				return s.Elements.Count;
			}

			public override int NumberOfSections (UITableView tableView)
			{
				return root.Sections.Count;
			}

			public override string TitleForHeader (UITableView tableView, int section)
			{
				return root.Sections [section].Caption;
			}

			public override string TitleForFooter (UITableView tableView, int section)
			{
				return root.Sections [section].Footer;
			}

			public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				var section = root.Sections [indexPath.Section];
				var element = section.Elements [indexPath.Row];
				
				return element.GetCell (tableView);
			}
			
			public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				var section = root.Sections [indexPath.Section];
				var element = section.Elements [indexPath.Row];

				element.Selected (container, tableView, indexPath);
			}
		}

		public void ActivateController (UIViewController controller)
		{
			dirty = true;
			
			var parent = ParentViewController;
			var nav = parent as UINavigationController;
			
			// We can not push a nav controller into a nav controller
			if (nav != null && !(controller is UINavigationController))
				nav.PushViewController (controller, true);
			else
				PresentModalViewController (controller, true);
		}
		
		public override void LoadView ()
		{
			tableView = new UITableView (UIScreen.MainScreen.Bounds, UITableViewStyle.Grouped) {
				AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin,
				AutosizesSubviews = true
			};

			tableView.Source = new Source (this);
			View = tableView;
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			
			NavigationItem.HidesBackButton = !pushing;
			if (root.Caption != null)
				NavigationItem.Title = root.Caption;
			if (dirty){
				tableView.ReloadData ();
				dirty = false;
			}
		}
		
		public event EventHandler ViewDissapearing;
		
		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
			if (ViewDissapearing != null)
				ViewDissapearing (this, EventArgs.Empty);
		}


		void PrepareRoot (RootElement root)
		{
			this.root = root;
			root.Prepare ();	
		}
		
		public DialogViewController (RootElement root)
		{			
			PrepareRoot (root);
		}
		
		public DialogViewController (RootElement root, bool pushing)
		{
			PrepareRoot (root);
			this.pushing = pushing;
		}
	}
}
