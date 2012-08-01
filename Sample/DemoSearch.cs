using System;
using System.Drawing;
using System.Linq;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Collections.Generic;

namespace Sample
{
	public partial class AppDelegate
	{
		public void DemoExternalSearchBar () 
		{
			UIViewController vcSearch = new UIViewController()
			{
				Title = "External Search Bar"
			};

			UISearchBar searchBar = new UISearchBar(RectangleF.FromLTRB(0, 0, navigation.View.Bounds.Width, 44)) {
				Tag = 1
			};

			// add search bar
			vcSearch.View.AddSubview(
				searchBar
			);

			// add container for dvc tableView
			vcSearch.View.AddSubview(
				new UIView(RectangleF.FromLTRB(0, searchBar.Frame.Height, navigation.View.Bounds.Width, navigation.View.Frame.Height - searchBar.Frame.Height)) { 
					Tag = 2
				}
			);

			// create dvc and attach search bar
			var dvc = new DialogViewController(new RootElement ("") {
				from sh in "ABCDEFGHIJKLMNOPQRSTUVWXYZ" 
				    select new Section (sh + " - Section") {
					   from filler in "12345" 
						select (Element) new StringElement (sh + " - " + filler)
				}
			}, false) {
				EnableSearch = true, 
				ExternalSearchBar = searchBar
			};

			// add dvc to container view
			vcSearch.View.ViewWithTag(2).AddSubview(dvc.TableView);

			navigation.PushViewController (vcSearch, true);
		}

	}
}

