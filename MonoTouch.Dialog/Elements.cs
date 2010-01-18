//
// Elements.cs: defines the various components of our view
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//
// Copyright 2010, Novell, Inc.
//
// Code licensed under the MIT X11 license
//
using System;
using System.Collections;
using System.Collections.Generic;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace MonoTouch.Dialog
{
	public class Element {
		public Element Parent;
		public string Caption;
		
		public Element (string caption)
		{
			this.Caption = caption;
		}	
		
		public virtual UITableViewCell GetCell (UITableView tv)
		{
			return new UITableViewCell (UITableViewCellStyle.Default, "xx");
		}
		
		static internal void RemoveTag (UITableViewCell cell, int tag)
		{
			var viewToRemove = cell.ContentView.ViewWithTag (tag);
			if (viewToRemove != null)
				viewToRemove.RemoveFromSuperview ();
		}
		
		public virtual string Summary ()
		{
			return "";
		}
		
		public virtual void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
		}
	}

	public class BooleanElement : Element {
		public bool Value;
		static NSString bkey = new NSString ("BooleanElement");
		UISwitch sw;
		
		public BooleanElement (string caption, bool value) : base (caption)
		{
			Value = value;
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			if (sw == null){
				sw = new UISwitch (new RectangleF (198, 12, 94, 27)){
					BackgroundColor = UIColor.Clear,
					Tag = 1,
					On = Value
				};
				sw.ValueChanged += delegate {
					Value = sw.On;
				};
			}
			
			var cell = tv.DequeueReusableCell (bkey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, bkey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			} else
				RemoveTag (cell, 1);
		
			cell.TextLabel.Text = Caption;
			cell.ContentView.AddSubview (sw);
			
			return cell;
		}
		
		public override string Summary ()
		{
			return Value ? "On" : "Off";
		}
	}

	public class FloatElement : Element {
		public float Value;
		public float MinValue, MaxValue;
		UIImage Left, Right;
		static NSString skey = new NSString ("FloatElement");
		UISlider slider;
		
		public FloatElement (UIImage left, UIImage right, float value) : base (null)
		{
			Left = left;
			Right = right;
			MinValue = 0;
			MaxValue = 1;
			Value = value;
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			if (slider == null){
				slider = new UISlider (new RectangleF (10f, 12f, 280f, 7f)){
					BackgroundColor = UIColor.Clear,
					MinValue = this.MinValue,
					MaxValue = this.MaxValue,
					Continuous = true,
					Value = this.Value,
					Tag = 1
				};
				slider.ValueChanged += delegate {
					Value = slider.Value;
				};
			}
			var cell = tv.DequeueReusableCell (skey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, skey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			} else
				RemoveTag (cell, 1);
			
			cell.ContentView.AddSubview (slider);
			return cell;
		}

		public override string Summary ()
		{
			return Value.ToString ();
		}
	}

	public class StringElement : Element {
		public string Value;
		
		public StringElement (string caption, string value) : base (caption)
		{
			Value = value;
		}
		
		public override string Summary ()
		{
			return Value;
		}
	}

	public class HtmlElement : Element {
		public string Url;
		static NSString hkey = new NSString ("HtmlElement");
		
		public HtmlElement (string caption, string url) : base (caption)
		{
			Url = url;
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (hkey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, hkey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
			}
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			
			cell.TextLabel.Text = Caption;
			return cell;
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			var vc = new UIViewController ();
			var web = new UIWebView (UIScreen.MainScreen.ApplicationFrame){
				BackgroundColor = UIColor.White,
				ScalesPageToFit = true,
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight
			};
			web.LoadStarted += delegate {
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
			};
			web.LoadFinished += delegate {
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
			};
			web.LoadError += (webview, args) => {
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
				web.LoadHtmlString (String.Format ("<html><center><font size=+5 color='red'>An error occurred:<br>{0}</font></center></html>", args.Error.LocalizedDescription), null);
			};
			vc.NavigationItem.Title = Caption;
			vc.View.AddSubview (web);
			
			dvc.ActivateController (vc);
			web.LoadRequest (NSUrlRequest.FromUrl (new NSUrl (Url)));
		}
	}

	public class RadioElement : Element {
		public string Group;
		static NSString rkey = new NSString ("RadioElement");
		internal int RadioIdx;
		
		public RadioElement (string caption, string group) : base (caption)
		{
			Group = group;
		}
		
		public event EventHandler Tapped;
		
		public RadioElement (string caption) : base (caption)
		{
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (rkey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, rkey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
			}
			var root = (RootElement) Parent.Parent;
			
			bool selected = RadioIdx == root.radio.Selected;
			cell.Accessory = selected ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
			
			cell.TextLabel.Text = Caption;
			return cell;
		}

		public override string Summary ()
		{
			return Caption;
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			if (Tapped != null)
				Tapped (this, EventArgs.Empty);
			
			RootElement root = (RootElement) Parent.Parent;
			if (RadioIdx != root.RadioSelected){
				var cell = tableView.CellAt (root.PathForRadio (root.RadioSelected));
				cell.Accessory = UITableViewCellAccessory.None;
				cell = tableView.CellAt (indexPath);
				cell.Accessory = UITableViewCellAccessory.Checkmark;
				root.RadioSelected = RadioIdx;
			}
			
			tableView.DeselectRow (indexPath, true);
		}
	}
	
	public class Section : Element, IEnumerable {
		public string Header, Footer;
		public List<Element> Elements = new List<Element> ();

		public Section () : base (null) {}
		
		public Section (string caption) : base (caption)
		{
		}

		public Section (string caption, string footer) : base (caption)
		{
			Footer = footer;
		}

		public void Add (Element element)
		{
			if (element == null)
				return;
			
			Elements.Add (element);
			element.Parent = this;
		}

		public IEnumerator GetEnumerator ()
		{
			foreach (var e in Elements)
				yield return e;
		}

		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = new UITableViewCell (UITableViewCellStyle.Default, "");
			cell.TextLabel.Text = "Section was used for Element";
			
			return cell;
		}
	}
	
	public class RadioGroup {
		public string Key;
		public int Selected;
		
		public RadioGroup (string key, int selected)
		{
			Key = key;
			Selected = selected;
		}
		
		public RadioGroup (int selected)
		{
			Selected = selected;
		}
	}
	
	public class RootElement : Element, IEnumerable {
		static NSString rkey = new NSString ("RootElement");
		public List<Section> Sections = new List<Section> ();
		int summarySection, summaryElement;
		internal RadioGroup radio;
		
		public RootElement (string caption) : base (caption)
		{
			summarySection = -1;
			Sections = new List<Section> ();
		}

		public RootElement (string caption, int section, int element) : base (caption)
		{
			summarySection = section;
			summaryElement = element;
		}
		
		public RootElement (string caption, RadioGroup radio) : base (caption)
		{
			this.radio = radio;
		}
		
		internal NSIndexPath PathForRadio (int idx)
		{
			if (radio == null)
				return null;
			
			uint current = 0, section = 0;
			foreach (Section s in Sections){
				uint row = 0;
				
				foreach (Element e in s.Elements){
					if (!(e is RadioElement))
						continue;
					
					if (current == idx)
						return new NSIndexPath ().FromIndexes (new uint [] { section, row});
					row++;
					current++;
				}
				section++;
			}
			return null;
		}
			
		internal void Prepare ()
		{
			if (radio == null)
				return;
			
			int current = 0;
			foreach (Section s in Sections)
				foreach (Element e in s.Elements){
					var re = e as RadioElement;
					if (re != null)
						re.RadioIdx = current++;
				}
		}
		
		public void Add (Section section)
		{
			if (section == null)
				return;
			
			Sections.Add (section);
			section.Parent = this;
		}

		public IEnumerator GetEnumerator ()
		{
			foreach (var s in Sections)
				yield return s;
		}

		public int RadioSelected {
			get {
				if (radio != null)
					return radio.Selected;
				return -1;
			}
			set {
				if (radio != null)
					radio.Selected = value;
			}
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (rkey);
			if (cell == null){
				var style = summarySection == -1 ? UITableViewCellStyle.Default : UITableViewCellStyle.Value1;
				
				cell = new UITableViewCell (style, rkey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			} 
		
			cell.TextLabel.Text = Caption;
			if (radio != null){
				int selected = radio.Selected;
				int current = 0;
				
				foreach (var s in Sections){
					foreach (var e in s.Elements){
						if (!(e is RadioElement))
							continue;
						
						if (current == selected){
							cell.DetailTextLabel.Text = e.Summary ();
							goto le;
						}
						current++;
					}
				}
			} else if (summarySection != -1 && summarySection < Sections.Count){
					var s = Sections [summarySection];
					if (summaryElement < s.Elements.Count)
						cell.DetailTextLabel.Text = s.Elements [summaryElement].Summary ();
			}
		le:
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			
			return cell;
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			var newDvc = new DialogViewController (this, true);
			dvc.ActivateController (newDvc);
		}
	}
}
