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
using System.Reflection;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using System.Drawing;
using System.Collections.Generic;
using MonoTouch.Foundation;

namespace MonoTouch.Dialog
{
	/// <summary>
	///   The DialogViewController is the main entry point to use MonoTouch.Dialog,
	///   it provides a simplified API to the UITableViewController.
	/// </summary>
	public class DialogViewController : UITableViewController
	{
		public UITableViewStyle Style = UITableViewStyle.Grouped;
		public event Action<NSIndexPath> OnSelection;
		UISearchBar searchBar;
		UITableView tableView;
		RefreshTableHeaderView refreshView;
		RootElement root;
		bool pushing;
		bool dirty;
		bool reloading;

		/// <summary>
		/// The root element displayed by the DialogViewController, the value can be changed during runtime to update the contents.
		/// </summary>
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
			}
		} 

		EventHandler refreshRequested;
		/// <summary>
		/// If you assign a handler to this event before the view is shown, the
		/// DialogViewController will have support for pull-to-refresh UI.
		/// </summary>
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
		
		// If the value is true, we are enabled, used in the source for quick computation
		bool enableSearch;
		public bool EnableSearch {
			get {
			   return enableSearch;
			}
			set {
				if (enableSearch == value)
					return;
				
				// After MonoTouch 3.0, we can allow for the search to be enabled/disable
				if (tableView != null)
					throw new ArgumentException ("You should set EnableSearch before the controller is shown");
				enableSearch = value;
			}
		}
		
		// If set, we automatically scroll the content to avoid showing the search bar until 
		// the user manually pulls it down.
		public bool AutoHideSearch { get; set; }
		
		public string SearchPlaceholder { get; set; }
		
		// If set, we automatically hide the keyboard after the user scrolls the table.
		// Also hides the keybaord when the keyboard's "search" button is tapped.
		public bool AutoHideSearchKeyboard { get; set; }		
		
		/// <summary>
		/// Hides the search keyboard.
		/// </summary>
		public void HideSearchKeyboard ()
		{
			searchBar.ResignFirstResponder ();
			
			if (!String.IsNullOrEmpty(searchBar.Text))
				EnableSearchCancelButton ();
		}
		
		public void EnableSearchCancelButton ()
		{
			foreach (UIView subview in searchBar.Subviews) {
				if (subview is UIButton) {
					((UIButton)subview).Enabled = true;
					return;
				}
			}
		}
		
		/// <summary>
		/// Invoke this method to trigger a data refresh.   
		/// </summary>
		/// <remarks>
		/// This will invoke the RefreshRequested event handler, the code attached to it
		/// should start the background operation to fetch the data and when it completes
		/// it should call ReloadComplete to restore the control state.
		/// </remarks>
		public void TriggerRefresh ()
		{
			TriggerRefresh (false);
		}
		
		/// <summary>
		/// Invoke this method to trigger a data refresh that shows the pull-to-refresh
		/// status display.
		/// </summary>
		/// <remarks>
		/// This will invoke the RefreshRequested event handler, the code attached to it
		/// should start the background operation to fetch the data and when it completes
		/// it should call ReloadComplete to restore the control state.
		/// </remarks>
		public void TriggerRefreshSimulatePulldown()
		{
			ConfigureTableView ();
			
			TriggerRefresh (true);
		}
		
		void TriggerRefresh (bool showStatus)
		{
			if (refreshRequested == null)
				return;

			if (reloading)
				return;
			
			reloading = true;
			if (refreshView != null)
				refreshView.SetActivity (true);
			refreshRequested (this, EventArgs.Empty);

			if (reloading && showStatus && refreshView != null){
				UIView.BeginAnimations ("reloadingData");
				UIView.SetAnimationDuration (0.2);
				TableView.ContentInset = new UIEdgeInsets (60, 0, 0, 0);
				UIView.CommitAnimations ();
			}
		}
		
		/// <summary>
		/// Invoke this method to signal that a reload has completed, this will update the UI accordingly.
		/// </summary>
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
		
		/// <summary>
		/// Controls whether the DialogViewController should auto rotate
		/// </summary>
		public bool Autorotate { get; set; }
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return Autorotate || toInterfaceOrientation == UIInterfaceOrientation.Portrait;
		}
		
		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate (fromInterfaceOrientation);
			
			//Fixes the RefreshView's size if it is shown during rotation
			if (refreshView != null) {
				var bounds = View.Bounds;
				
				refreshView.Frame = new RectangleF (0, -bounds.Height, bounds.Width, bounds.Height);
			}
			
			ReloadData ();
		}
		
		Section [] originalSections;
		Element [][] originalElements;
		
		/// <summary>
		/// Allows caller to programatically activate the search bar and start the search process
		/// </summary>
		public void StartSearch ()
		{
			if (originalSections != null)
				return;
			
			searchBar.BecomeFirstResponder ();
			originalSections = Root.Sections.ToArray ();
			originalElements = new Element [originalSections.Length][];
			for (int i = 0; i < originalSections.Length; i++)
				originalElements [i] = originalSections [i].Elements.ToArray ();
		}
		
		/// <summary>
		/// Allows the caller to programatically stop searching.
		/// </summary>
		public virtual void FinishSearch ()
		{
			if (originalSections == null)
				return;
			
			Root.Sections = new List<Section> (originalSections);
			originalSections = null;
			originalElements = null;
			searchBar.ResignFirstResponder ();
			ReloadData ();
		}
		
		public delegate void SearchTextEventHandler (object sender, SearchChangedEventArgs args);
		public event SearchTextEventHandler SearchTextChanged;
		
		public virtual void OnSearchTextChanged (string text)
		{
			if (SearchTextChanged != null)
				SearchTextChanged (this, new SearchChangedEventArgs (text));
		}
		                                     
		public void PerformFilter (string text)
		{
			if (originalSections == null)
				return;
			
			OnSearchTextChanged (text);
			
			var newSections = new List<Section> ();
			
			for (int sidx = 0; sidx < originalSections.Length; sidx++){
				Section newSection = null;
				var section = originalSections [sidx];
				Element [] elements = originalElements [sidx];
				
				for (int eidx = 0; eidx < elements.Length; eidx++){
					if (elements [eidx].Matches (text)){
						if (newSection == null){
							newSection = new Section (section.Header, section.Footer){
								FooterView = section.FooterView,
								HeaderView = section.HeaderView,
								Caption = section.Caption
							};
							newSections.Add (newSection);
						}
						newSection.Add (elements [eidx]);
					}
				}
			}
			
			Root.Sections = newSections;
			
			ReloadData ();
		}
		
		public virtual void SearchButtonClicked (string text)
		{
		}
			
		class SearchDelegate : UISearchBarDelegate {
			DialogViewController container;
			
			public SearchDelegate (DialogViewController container)
			{
				this.container = container;
			}
			
			public override void OnEditingStarted (UISearchBar searchBar)
			{
				searchBar.ShowsCancelButton = true;
				container.StartSearch ();
			}
			
			public override void OnEditingStopped (UISearchBar searchBar)
			{
				if (String.IsNullOrEmpty(searchBar.Text)) {
					searchBar.ShowsCancelButton = false;
					container.FinishSearch ();
				}
			}
			
			public override void TextChanged (UISearchBar searchBar, string searchText)
			{
				container.PerformFilter (searchText ?? "");
			}
			
			public override void CancelButtonClicked (UISearchBar searchBar)
			{
				searchBar.Text = "";
				searchBar.ShowsCancelButton = false;
				
				if (container.AutoHideSearch && container.EnableSearch) {
					if (container.TableView.ContentOffset.Y < 44)
						container.TableView.ContentOffset = new PointF (0, 44);
				}
				
				container.FinishSearch ();
				searchBar.ResignFirstResponder ();
			}
			
			public override void SearchButtonClicked (UISearchBar searchBar)
			{
				container.SearchButtonClicked (searchBar.Text);
				
				if (container.AutoHideSearchKeyboard)
					searchBar.ResignFirstResponder ();
				
				if (!String.IsNullOrEmpty(searchBar.Text))
					container.EnableSearchCancelButton ();
			}
		}
				
		public class Source : UITableViewSource {
			const float yboundary = 65;
			protected DialogViewController Container;
			protected RootElement Root;
			bool checkForRefresh;
			
			public Source (DialogViewController container)
			{
				this.Container = container;
				Root = container.root;
			}
			
			public override void AccessoryButtonTapped (UITableView tableView, NSIndexPath indexPath)
			{
				var section = Root.Sections [indexPath.Section];
				var element = (section.Elements [indexPath.Row] as StyledStringElement);
				if (element != null)
					element.AccessoryTap ();
			}
			
			public override int RowsInSection (UITableView tableview, int section)
			{
				var s = Root.Sections [section];
				var count = s.Elements.Count;
				
				return count;
			}

			public override int NumberOfSections (UITableView tableView)
			{
				return Root.Sections.Count;
			}

			public override string TitleForHeader (UITableView tableView, int section)
			{
				return Root.Sections [section].Caption;
			}

			public override string TitleForFooter (UITableView tableView, int section)
			{
				return Root.Sections [section].Footer;
			}

			public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				var section = Root.Sections [indexPath.Section];
				var element = section.Elements [indexPath.Row];
				
				return element.GetCell (tableView);
			}
			
			public override void WillDisplay (UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
			{
				if (Root.NeedColorUpdate){
					var section = Root.Sections [indexPath.Section];
					var element = section.Elements [indexPath.Row];
					var colorized = element as IColorizeBackground;
					if (colorized != null)
						colorized.WillDisplay (tableView, cell, indexPath);
				}
			}
			
			public override void RowDeselected (UITableView tableView, NSIndexPath indexPath)
			{
				Container.Deselected (indexPath);
			}
			
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				var onSelection = Container.OnSelection;
				if (onSelection != null)
					onSelection (indexPath);
				Container.Selected (indexPath);
			}			
			
			public override UIView GetViewForHeader (UITableView tableView, int sectionIdx)
			{
				var section = Root.Sections [sectionIdx];
				return section.HeaderView;
			}

			public override float GetHeightForHeader (UITableView tableView, int sectionIdx)
			{
				var section = Root.Sections [sectionIdx];
				if (section.HeaderView == null)
					return -1;
				return section.HeaderView.Frame.Height;
			}

			public override UIView GetViewForFooter (UITableView tableView, int sectionIdx)
			{
				var section = Root.Sections [sectionIdx];
				return section.FooterView;
			}
			
			public override float GetHeightForFooter (UITableView tableView, int sectionIdx)
			{
				var section = Root.Sections [sectionIdx];
				if (section.FooterView == null)
					return -1;
				return section.FooterView.Frame.Height;
			}
			
			#region Pull to Refresh support
			public override void Scrolled (UIScrollView scrollView)
			{
				if (Container.EnableSearch && Container.AutoHideSearchKeyboard){
					Container.HideSearchKeyboard ();
				}				
				
				if (!checkForRefresh)
					return;
				if (Container.reloading)
					return;
				var view  = Container.refreshView;
				if (view == null)
					return;
				
				var point = Container.TableView.ContentOffset;
				
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
				if (Container.refreshView == null)
					return;
				
				checkForRefresh = false;
				if (Container.TableView.ContentOffset.Y > -yboundary)
					return;
				Container.TriggerRefresh (true);
			}
			#endregion
		}
		
		//
		// Performance trick, if we expose GetHeightForRow, the UITableView will
		// probe *every* row for its size;   Avoid this by creating a separate
		// model that is used only when we have items that require resizing
		//
		public class SizingSource : Source {
			public SizingSource (DialogViewController controller) : base (controller) {}
			
			public override float GetHeightForRow (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				var section = Root.Sections [indexPath.Section];
				var element = section.Elements [indexPath.Row];
				
				var sizable = element as IElementSizing;
				if (sizable == null)
					return tableView.RowHeight;
				return sizable.GetHeight (tableView, indexPath);
			}
		}
			
		/// <summary>
		/// Activates a nested view controller from the DialogViewController.   
		/// If the view controller is hosted in a UINavigationController it
		/// will push the result.   Otherwise it will show it as a modal
		/// dialog
		/// </summary>
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

		/// <summary>
		/// Dismisses the view controller.   It either pops or dismisses
		/// based on the kind of container we are hosted in.
		/// </summary>
		public void DeactivateController (bool animated)
		{
			var parent = ParentViewController;
			var nav = parent as UINavigationController;
			
			if (nav != null)
				nav.PopViewControllerAnimated (animated);
			else
				DismissModalViewControllerAnimated (animated);
		}

		void SetupSearch ()
		{
			if (enableSearch) {
				// Wrap search bar in UIView to allow searchbar resizing (to make space for an index)
				var searchBarWrapper = new SearchBarBackgroundView(new RectangleF (0, 0, tableView.Bounds.Width, 44)) {
					AutosizesSubviews = true
				};
				
				searchBar = new UISearchBar (new RectangleF (0, 0, tableView.Bounds.Width, 44)) {
					Delegate = new SearchDelegate (this)
				};
				
				// Determine if index titles will be displayed and adjust search width appropriately
				SetSearchSize ();
				
				// Make the search bar background transparent (wrapper UIView has the gradient background)
				searchBar.BackgroundColor = UIColor.Clear;
				searchBar.Subviews[0].RemoveFromSuperview();
				
				if (SearchPlaceholder != null)
					searchBar.Placeholder = this.SearchPlaceholder;
				
				searchBarWrapper.AddSubview (searchBar);
				tableView.TableHeaderView = searchBarWrapper;
			} else {
				// Does not work with current Monotouch, will work with 3.0
				// tableView.TableHeaderView = null;
			}
		}
		
		public class SearchBarBackgroundView : UIView {
		
			static CGGradient gradient;
			
			public SearchBarBackgroundView (RectangleF frame) : base (frame)
			{
				using (var colorspace = CGColorSpace.CreateDeviceRGB ()){
					gradient = new CGGradient (
						colorspace, 
						new float [] { 
							.839f, .866f, .886f, 1.0f,
							.776f, .811f, .831f, 1.0f,
							.701f, .745f, .772f, 1.0f },
						new float [] { 0f, 0.5f, 1.0f });
				}
			}
			
			public override void Draw (RectangleF rect)
			{
				var ctx = UIGraphics.GetCurrentContext ();
				ctx.DrawLinearGradient (gradient, new PointF (0, 0), new PointF (0, rect.Height), 0);
			}
		}
		
		bool UsingIndexedTitles ()
		{
			// Determine if index titles have been provided. This is done by:
			// 	- checking if the SectionIndexTitles() method is overridden in the tableView's source 
			//	  instance using reflection (if not there will be no titles)
			//  - calling the inherited implementation's SectionIndexTitles() method to determine if 
			//	  any indexes titles are returned
			
			// This method of determining if indexed titles are displayed is likely not optimal as it depends 
			// on an implementation detail; the base SectionIndexTitles() method to be implementated
			// in UITableViewSource.
			
			MethodInfo m = tableView.Source.GetType().GetMethod("SectionIndexTitles", BindingFlags.Instance | BindingFlags.Public);
			
			if (!m.DeclaringType.Name.Equals(typeof(UITableViewSource).Name)) {
				if (tableView.Source.SectionIndexTitles(tableView).Length > 0) {
					return true;
				}
			}
						
			return false;
		}
		
		void SetSearchSize ()
		{
			const int padRight = 29;
				
			if (searchBar == null)
				return;
			
			// Determine if index titles will be displayed and adjust search width appropriately
			float searchWidth = tableView.Bounds.Width;
			if (UsingIndexedTitles ())
				searchWidth = searchWidth - padRight;
			
			searchBar.Frame = new RectangleF (0, 0, searchWidth, 44);
			searchBar.SetNeedsLayout ();
		}
		
		public virtual void Deselected (NSIndexPath indexPath)
		{
			var section = root.Sections [indexPath.Section];
			var element = section.Elements [indexPath.Row];
			
			element.Deselected (this, tableView, indexPath);
		}
		
		public virtual void Selected (NSIndexPath indexPath)
		{
			var section = root.Sections [indexPath.Section];
			var element = section.Elements [indexPath.Row];
			
			if (EnableSearch && AutoHideSearchKeyboard)
				HideSearchKeyboard ();
			
			element.Selected (this, tableView, indexPath);
		}
		
		public virtual UITableView MakeTableView (RectangleF bounds, UITableViewStyle style)
		{
			return new UITableView (bounds, style);
		}
		
		public override void LoadView ()
		{
			tableView = MakeTableView (UIScreen.MainScreen.Bounds, Style);
			tableView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin;
			tableView.AutosizesSubviews = true;
			
			if (root != null)
				root.Prepare ();
			
			UpdateSource ();
			View = tableView;
			SetupSearch ();
			ConfigureTableView ();
			
			if (root == null)
				return;
			root.TableView = tableView;
		}
		
		void ConfigureTableView ()
		{
			if (refreshRequested != null){
				// The dimensions should be large enough so that even if the user scrolls, we render the
				// whole are with the background color.
				var bounds = View.Bounds;
				refreshView = MakeRefreshTableHeaderView (new RectangleF (0, -bounds.Height, bounds.Width, bounds.Height));
				if (reloading)
					refreshView.SetActivity (true);
				TableView.AddSubview (refreshView);
			}
		}
		
		public virtual RefreshTableHeaderView MakeRefreshTableHeaderView (RectangleF rect)
		{
			return new RefreshTableHeaderView (rect);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			if (AutoHideSearch){
				if (enableSearch){
					if (TableView.ContentOffset.Y < 44)
						TableView.ContentOffset = new PointF (0, 44);
				}
			}
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

		public virtual Source CreateSizingSource (bool unevenRows)
		{
			return unevenRows ? new SizingSource (this) : new Source (this);
		}
		
		Source TableSource;
		
		void UpdateSource ()
		{
			if (root == null)
				return;
			
			TableSource = CreateSizingSource (root.UnevenRows);
			tableView.Source = TableSource;
			
			if (enableSearch)
				SetSearchSize ();
		}

		public void ReloadData ()
		{
			if (root == null)
				return;
			
			if(root.Caption != null) 
				NavigationItem.Title = root.Caption;
			
			root.Prepare ();
			if (tableView != null){
				UpdateSource ();
				tableView.ReloadData ();
			}
			dirty = false;
		}
		
		public event EventHandler ViewDisappearing;
		
		[Obsolete ("Use the ViewDisappearing event instead")]
		public event EventHandler ViewDissapearing {
			add {
				ViewDisappearing += value;
			}
			remove {
				ViewDisappearing -= value;
			}
		}
		
		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
			if (ViewDisappearing != null)
				ViewDisappearing (this, EventArgs.Empty);
		}
		
		public DialogViewController (RootElement root) : base (UITableViewStyle.Grouped)
		{
			this.root = root;
		}
		
		public DialogViewController (UITableViewStyle style, RootElement root) : base (style)
		{
			Style = style;
			this.root = root;
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
			this.root = root;
		}

		public DialogViewController (UITableViewStyle style, RootElement root, bool pushing) : base (style)
		{
			Style = style;
			this.pushing = pushing;
			this.root = root;
		}
	}
}
