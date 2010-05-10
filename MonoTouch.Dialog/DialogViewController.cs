//
// DialogViewController.cs: drives MonoTouch.Dialog
//
// Author:
//   Miguel de Icaza
//
// Code to support pull-to-refresh based on Martin Bowling's TweetTableView
// which is based in turn in EGOTableViewPullRefresh code which was created
// by Devin Doty and is Copyrighted 2009 enormego and released under the
// MIT X11 license
//
using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;

namespace MonoTouch.Dialog
{
	public class CommitEditingStyleArgs : EventArgs
	{
		private UITableView tableView;
		private UITableViewCellEditingStyle editingStyle;
		private MonoTouch.Foundation.NSIndexPath indexPath;
		
		public CommitEditingStyleArgs(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
		{
			this.tableView = tableView;
			this.editingStyle = editingStyle;
			this.indexPath = indexPath;
			
		}
		
		public UITableView TableView
		{
			get
			{
				return tableView;
			}
			set
			{
				tableView = value;
			}
		}
		
		public UITableViewCellEditingStyle EditingStyle
		{
			get
			{
				return editingStyle;
			}
			set
			{
				editingStyle = value;
			}
		}
		
		public NSIndexPath IndexPath
		{
			get
			{
				return indexPath;
			}
			set
			{
				indexPath = value;
			}
		}
	    
	}

	
	public class DialogViewController : UITableViewController
	{
		public UITableViewStyle Style = UITableViewStyle.Grouped;
		UITableView tableView;
		RefreshTableHeaderView refreshView;
		RootElement root;
		bool pushing;
		bool dirty;
		bool reloading;

		public RootElement Root {
			get {
				return root;
			}
			set {
				if (root == value)
					return;
				if (root != null)
					root.Dispose ();
				
				root = value;
				root.TableView = tableView;					
				ReloadData ();
				NavigationItem.RightBarButtonItem = _buttonEdit;
			}
		}
		
		private UIBarButtonItem _buttonEdit;
		private UIBarButtonItem _buttonDone;
		
		public override void ViewDidLoad ()
		{
			if(enableEdit)
			{
				// Add Edit and Done buttons
				_buttonEdit = new UIBarButtonItem(UIBarButtonSystemItem.Edit);
				_buttonDone = new UIBarButtonItem(UIBarButtonSystemItem.Done);
				_buttonEdit.Clicked += Handle_buttonEditClicked;
				_buttonDone.Clicked += Handle_buttonDoneClicked;
				
			}
			
			base.ViewDidLoad ();
			
		} 

		EventHandler refreshRequested;
		public event EventHandler RefreshRequested {
			add {
				if (tableView != null)
					throw new ArgumentException ("You should set the handler before the controller is shown");
				refreshRequested += value; 
			}
			remove {
				refreshRequested -= value;
			}
		}
		
		public void TriggerRefresh ()
		{
			if (refreshRequested == null)
				return;

			if (reloading)
				return;
			
			reloading = true;
			if (refreshView != null)
				refreshView.SetActivity (true);
			refreshRequested (this, EventArgs.Empty);

			if (refreshView != null){
				UIView.BeginAnimations ("reloadingData");
				UIView.SetAnimationDuration (0.2);
				TableView.ContentInset = new UIEdgeInsets (60, 0, 0, 0);
				UIView.CommitAnimations ();
			}
		}
		
		public void ReloadComplete ()
		{
			if (refreshView != null)
				refreshView.LastUpdate = DateTime.Now;
			if (!reloading)
				return;

			reloading = false;
			if (refreshView == null)
				return;
			
			refreshView.SetActivity (false);
			refreshView.Flip (false);
			UIView.BeginAnimations ("doneReloading");
			UIView.SetAnimationDuration (0.3f);
			TableView.ContentInset = new UIEdgeInsets (0, 0, 0, 0);
			refreshView.SetStatus (RefreshViewStatus.PullToReload);
			UIView.CommitAnimations ();
		}
	
		private void Handle_buttonDoneClicked (object sender, EventArgs e)
		{
			if (refreshRequested == null)
				return;
			Editing = false;
			NavigationItem.RightBarButtonItem = _buttonEdit;
		}
		
		private void Handle_buttonEditClicked (object sender, EventArgs e)
		{
			Editing = true;
			NavigationItem.RightBarButtonItem = _buttonDone;
		}
		
		private bool rotateUIEnabled;

		public bool RotateUIEnabled {
			get { return rotateUIEnabled; }
			set { rotateUIEnabled = value; }
		}
		
		private	bool enableEdit;
		
		public bool EnableEdit {
			get { return enableEdit; }
			set { enableEdit = value; }
		}
		
		class Source : UITableViewSource {
			const float yboundary = 65;
			protected DialogViewController container;
			protected RootElement root;
			bool checkForRefresh;
			
			public event EventHandler<CommitEditingStyleArgs> OnCommitEditingStyle;
			
			public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				switch (editingStyle)
				{
					case UITableViewCellEditingStyle.Delete:
						this.root[indexPath.Section].RemoveRange(indexPath.Row,1,UITableViewRowAnimation.Fade);
						break;
					
					case UITableViewCellEditingStyle.Insert:
						Console.WriteLine("UITableViewCellEditingStyle:Insert Called");
						//this._tableItems[indexPath.Section].Items.Insert (indexPath.Row, new TableItem ());
						//---- insert a new row in the table
						//tableView.InsertRows (new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Fade);
						break;
					
					case UITableViewCellEditingStyle.None:
						// when is this called?
						Console.WriteLine("UITableViewCellEditingStyle:None Called");
						break;
				}
				
				if(OnCommitEditingStyle != null)
					OnCommitEditingStyle(this,new CommitEditingStyleArgs(tableView,editingStyle,indexPath));
				
			}
		
			public Source (DialogViewController container)
			{
				this.container = container;
				root = container.root;
			}
			
			public override int RowsInSection (UITableView tableview, int section)
			{
				var s = root.Sections [section];
				var count = s.Elements.Count;
				
				return count;
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
			
			public override UIView GetViewForHeader (UITableView tableView, int sectionIdx)
			{
				var section = root.Sections [sectionIdx];
				return section.HeaderView;
			}

			public override float GetHeightForHeader (UITableView tableView, int sectionIdx)
			{
				var section = root.Sections [sectionIdx];
				if (section.HeaderView == null)
					return -1;
				return section.HeaderView.Frame.Height;
			}

			public override UIView GetViewForFooter (UITableView tableView, int sectionIdx)
			{
				var section = root.Sections [sectionIdx];
				return section.FooterView;
			}
			
			public override float GetHeightForFooter (UITableView tableView, int sectionIdx)
			{
				var section = root.Sections [sectionIdx];
				if (section.FooterView == null)
					return -1;
				return section.FooterView.Frame.Height;
			}
			
			#region Pull to Refresh support
			public override void Scrolled (UIScrollView scrollView)
			{
				if (!checkForRefresh)
					return;
				if (container.reloading)
					return;
				var view  = container.refreshView;
				if (view == null)
					return;
				
				var point = container.TableView.ContentOffset;
				if (view.IsFlipped && point.Y > -yboundary && point.Y < 0){
					view.Flip (true);
					view.SetStatus (RefreshViewStatus.PullToReload);
				} else if (!view.IsFlipped && point.Y < -yboundary){
					view.Flip (true);
					view.SetStatus (RefreshViewStatus.ReleaseToReload);
				}
			}
			
			public override void DraggingStarted (UIScrollView scrollView)
			{
				checkForRefresh = true;
			}
			
			public override void DraggingEnded (UIScrollView scrollView, bool willDecelerate)
			{
				if (container.refreshView == null)
					return;
				
				checkForRefresh = false;
				if (container.TableView.ContentOffset.Y > -yboundary)
					return;
				container.TriggerRefresh ();
			}
			#endregion
		}

		//
		// Performance trick, if we expose GetHeightForRow, the UITableView will
		// probe *every* row for its size;   Avoid this by creating a separate
		// model that is used only when we have items that require resizing
		//
		class SizingSource : Source {
			public SizingSource (DialogViewController controller) : base (controller) {}
			
			public override float GetHeightForRow (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				var section = root.Sections [indexPath.Section];
				var element = section.Elements [indexPath.Row];
				
				var sizable = element as IElementSizing;
				if (sizable == null)
					return tableView.RowHeight;
				return sizable.GetHeight (tableView, indexPath);
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
			tableView = new UITableView (UIScreen.MainScreen.Bounds, Style) {
				AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin,
				AutosizesSubviews = true
			};

			UpdateSource ();
			View = tableView;
			if (root == null)
				return;
			
			root.TableView = tableView;
		}

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return rotateUIEnabled;
			
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			if (root == null)
				return;
			
			root.Prepare ();
			
			NavigationItem.HidesBackButton = !pushing;
			if (root.Caption != null)
				NavigationItem.Title = root.Caption;
			if (dirty){
				tableView.ReloadData ();
				dirty = false;
			}
		}

		Source TableSource;
		
		public event EventHandler<CommitEditingStyleArgs> OnCommitEditingStyle;
					
		void UpdateSource ()
		{
			if (root == null)
				return;
			
			TableSource = root.UnevenRows ? new SizingSource (this) : new Source (this);
			
			if(this.OnCommitEditingStyle != null)
				TableSource.OnCommitEditingStyle += this.OnCommitEditingStyle;
			
			tableView.Source = TableSource;
		}

		public void ReloadData ()
		{
			root.Prepare ();
			if (tableView != null){
				UpdateSource ();
				tableView.ReloadData ();
			}
			dirty = false;
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
			if (root != null)
				root.Prepare ();
		}
		
		public DialogViewController (RootElement root) : base (UITableViewStyle.Grouped)
		{
			PrepareRoot (root);
		}
		
		public DialogViewController (UITableViewStyle style, RootElement root) : base (style)
		{
			PrepareRoot (root);
		}
		
		/// <summary>
		///     Creates a new DialogViewController from a RootElement and sets the push status
		/// </summary>
		/// <param name="root">
		/// The <see cref="RootElement"/> containing the information to render.
		/// </param>
		/// <param name="pushing">
		/// A <see cref="System.Boolean"/> describing whether this is being pushed 
		/// (NavigationControllers) or not.   If pushing is true, then the back button 
		/// will be shown, allowing the user to go back to the previous controller
		/// </param>
		public DialogViewController (RootElement root, bool pushing) : base (UITableViewStyle.Grouped)
		{
			this.pushing = pushing;
			PrepareRoot (root);
		}

		public DialogViewController (UITableViewStyle style, RootElement root, bool pushing) : base (style)
		{
			this.pushing = pushing;
			PrepareRoot (root);
		}
	}
	
}
