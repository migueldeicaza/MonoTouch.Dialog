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
				Console.WriteLine ("at {0}, {1} -> {2}", indexPath.Section, indexPath.Row, element.GetType ());
				
				return element.GetCell (tableView);
			}
			
			public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				var section = root.Sections [indexPath.Section];
				var element = section.Elements [indexPath.Row];

				if (element is RadioElement){
					var re = element as RadioElement;
					
					// Hook to do something interesting with it.
					re.Clicked ();

					if (re.RadioIdx != root.radio.Selected){
						var cell = tableView.CellAt (root.PathForRadio (root.radio.Selected));
						cell.Accessory = UITableViewCellAccessory.None;
						cell = tableView.CellAt (indexPath);
						cell.Accessory = UITableViewCellAccessory.Checkmark;
						root.radio.Selected = re.RadioIdx;
					}
					
					tableView.DeselectRow (indexPath, true);
				}
				
				if (!(element is RootElement))
					return;
				
				var dvc = new DialogViewController ((RootElement) element, true);
				var parent = container.ParentViewController;
				var nav = parent as UINavigationController;
				if (nav != null)
					nav.PushViewController (dvc, true);
				else
					parent.PresentModalViewController (dvc, true);
			}
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
		
		internal DialogViewController (RootElement root, bool pushing)
		{
			PrepareRoot (root);
			this.pushing = pushing;
		}
	}
}
