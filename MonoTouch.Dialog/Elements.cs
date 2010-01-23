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
	/// <summary>
	/// Base class for all elements in MonoTouch.Dialog
	/// </summary>
	public class Element : IDisposable {
		/// <summary>
		///  Handle to the container object.
		/// </summary>
		/// <remarks>
		/// For sections this points to a RootElement, for every
		/// other object this points to a Section and it is null
		/// for the root RootElement.
		/// </remarks>
		public Element Parent;
		
		/// <summary>
		///  The caption to display for this given element
		/// </summary>
		public string Caption;
		
		/// <summary>
		///  Initializes the element with the given caption.
		/// </summary>
		/// <param name="caption">
		/// The caption.
		/// </param>
		public Element (string caption)
		{
			this.Caption = caption;
		}	
		
		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
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
		
		/// <summary>
		/// Returns a summary of the value represented by this object, suitable 
		/// for rendering as the result of a RootElement with child objects.
		/// </summary>
		/// <returns>
		/// The return value must be a short description of the value.
		/// </returns>
		public virtual string Summary ()
		{
			return "";
		}
		
		/// <summary>
		/// Invoked when the given element has been tapped by the user.
		/// </summary>
		/// <param name="dvc">
		/// The <see cref="DialogViewController"/> where the selection took place
		/// </param>
		/// <param name="tableView">
		/// The <see cref="UITableView"/> that contains the element.
		/// </param>
		/// <param name="path">
		/// The <see cref="NSIndexPath"/> that contains the Section and Row for the element.
		/// </param>
		public virtual void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
		}
	}

	/// <summary>
	/// Used to display switch on the screen.
	/// </summary>
	public class BooleanElement : Element {
		public bool Value;
		public string Key;
		static NSString bkey = new NSString ("BooleanElement");
		UISwitch sw;
		
		public BooleanElement (string caption, bool value) : base (caption)
		{
			Value = value;
		}
		
		public BooleanElement (string caption, bool value, string key) : this (caption, value)
		{
			Key = key;
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
		
		protected override void Dispose (bool disposing)
		{
			if (disposing){
				sw.Dispose ();
				sw = null;
			}
		}
	}

	/// <summary>
	///  Used to display a slider on the screen.
	/// </summary>
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
		
		protected override void Dispose (bool disposing)
		{
			if (disposing){
				slider.Dispose ();
				slider = null;
			}
		}		
	}

	/// <summary>
	///  Used to display a cell that will launch a web browser when selected.
	/// </summary>
	public class HtmlElement : Element {
		public string Url;
		static NSString hkey = new NSString ("HtmlElement");
		UIWebView web;
		
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

		static bool NetworkActivity {
			set {
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = value;
			}
		}
		
		// We use this class to dispose the web control when it is not
		// in use, as it could be a bit of a pig, and we do not want to
		// wait for the GC to kick-in.
		class WebViewController : UIViewController {
			HtmlElement container;
			
			public WebViewController (HtmlElement container) : base ()
			{
				this.container = container;
			}
			
			public override void ViewWillDisappear (bool animated)
			{
				base.ViewWillDisappear (animated);
				NetworkActivity = false;
				container.web.StopLoading ();
				container.web.Dispose ();
				container.web = null;
			}

		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			var vc = new WebViewController (this);
			web = new UIWebView (UIScreen.MainScreen.ApplicationFrame){
				BackgroundColor = UIColor.White,
				ScalesPageToFit = true,
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight
			};
			web.LoadStarted += delegate {
				NetworkActivity = true;
			};
			web.LoadFinished += delegate {
				NetworkActivity = false;
			};
			web.LoadError += (webview, args) => {
				NetworkActivity = false;
				if (web != null)
					web.LoadHtmlString (String.Format ("<html><center><font size=+5 color='red'>An error occurred:<br>{0}</font></center></html>", args.Error.LocalizedDescription), null);
			};
			vc.NavigationItem.Title = Caption;
			vc.View.AddSubview (web);
			
			dvc.ActivateController (vc);
			web.LoadRequest (NSUrlRequest.FromUrl (new NSUrl (Url)));
		}
	}

	public class StringElement : Element {
		static NSString skey = new NSString ("StringElement");
		public string Value;
		public UITextAlignment Alignment = UITextAlignment.Left;
		
		public StringElement (string caption) : base (caption) {}
		
		public StringElement (string caption, string value) : base (caption)
		{
			Value = value;
		}
		
		public StringElement (string caption,  NSAction tapped) : base (caption)
		{
			Tapped += tapped;
		}
		
		public event NSAction Tapped;
		
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (skey);
			if (cell == null){
				cell = new UITableViewCell (Value == null ? UITableViewCellStyle.Default : UITableViewCellStyle.Value1, skey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
			}
			cell.Accessory = UITableViewCellAccessory.None;
			cell.TextLabel.Text = Caption;
			cell.TextLabel.TextAlignment = Alignment;
			if (Value != null)
				cell.DetailTextLabel.Text = Value;
			return cell;
		}

		public override string Summary ()
		{
			return Caption;
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			if (Tapped != null)
				Tapped ();
			tableView.DeselectRow (indexPath, true);
		}
	}
	
	public class RadioElement : StringElement {
		public string Group;
		static NSString rkey = new NSString ("RadioElement");
		internal int RadioIdx;
		
		public RadioElement (string caption, string group) : base (caption)
		{
			Group = group;
		}
				
		public RadioElement (string caption) : base (caption)
		{
		}

		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = base.GetCell (tv);			
			var root = (RootElement) Parent.Parent;
			
			if (!(root.group is RadioGroup))
				throw new Exception ("The RootElement's Group is null or is not a RadioGroup");
			
			bool selected = RadioIdx == ((RadioGroup)(root.group)).Selected;
			cell.Accessory = selected ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;

			return cell;
		}

		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			RootElement root = (RootElement) Parent.Parent;
			if (RadioIdx != root.RadioSelected){
				var cell = tableView.CellAt (root.PathForRadio (root.RadioSelected));
				cell.Accessory = UITableViewCellAccessory.None;
				cell = tableView.CellAt (indexPath);
				cell.Accessory = UITableViewCellAccessory.Checkmark;
				root.RadioSelected = RadioIdx;
			}
			
			base.Selected (dvc, tableView, indexPath);
		}
	}
	
	public class CheckboxElement : StringElement {
		public bool Value;
		public string Group;
		
		public CheckboxElement (string caption) : base (caption) {}
		public CheckboxElement (string caption, bool value) : base (caption)
		{
			Value = value;
		}
		
		public CheckboxElement (string caption, bool value, string group) : this (caption, value)
		{
			Group = group;
		}
		
		UITableViewCell ConfigCell (UITableViewCell cell)
		{
			cell.Accessory = Value ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
			return cell;
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			return  ConfigCell (base.GetCell (tv));
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			Value = !Value;
			var cell = tableView.CellAt (path);
			ConfigCell (cell);
			base.Selected (dvc, tableView, path);
		}

	}
	
	/// <summary>
	/// An element that can be used to enter text.
	/// </summary>
	/// <remarks>
	/// This element can be used to enter text both regular and password protected entries. 
	///     
	/// The Text fields in a given section are aligned with each other.
	/// </remarks>
	public class EntryElement : Element {
		public string Value;
		static NSString ekey = new NSString ("EntryElement");
		bool isPassword;
		UILabel label; UITextField entry;
		string placeholder;
		static UIFont font = UIFont.BoldSystemFontOfSize (17);
		
		/// <summary>
		/// Constructs an EntryElement with the given caption, placeholder and initial value.
		/// </summary>
		/// <param name="caption">
		/// The caption to use
		/// </param>
		/// <param name="placeholder">
		/// Placeholder to display.
		/// </param>
		/// <param name="value">
		/// Initial value.
		/// </param>
		public EntryElement (string caption, string placeholder, string value) : base (caption)
		{
			Value = value;
			this.placeholder = placeholder;
		}
		
		/// <summary>
		/// Constructs  an EntryElement for password entry with the given caption, placeholder and initial value.
		/// </summary>
		/// <param name="caption">
		/// The caption to use
		/// </param>
		/// <param name="placeholder">
		/// Placeholder to display.
		/// </param>
		/// <param name="value">
		/// Initial value.
		/// </param>
		/// <param name="isPassword">
		/// True if this should be used to enter a password.
		/// </param>
		public EntryElement (string caption, string placeholder, string value, bool isPassword) : base (caption)
		{
			Value = value;
			this.isPassword = isPassword;
			this.placeholder = placeholder;
		}

		public override string Summary ()
		{
			return Value;
		}

		// 
		// Computes the X position for the entry by aligning all the entries in the Section
		//
		SizeF ComputeEntryPosition (UITableView tv, UITableViewCell cell)
		{
			Section s = Parent as Section;
			if (s.EntryAlignment.Width != 0)
				return s.EntryAlignment;
			
			SizeF max = new SizeF (-1, -1);
			foreach (var e in s.Elements){
				var ee = e as EntryElement;
				if (ee == null)
					continue;
				
				var size = tv.StringSize (ee.Caption, font);
				if (size.Width > max.Width)
					max = size;				
			}
			s.EntryAlignment = new SizeF (25 + Math.Min (max.Width, 160), max.Height);
			return s.EntryAlignment;
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (ekey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, ekey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			} else 
				RemoveTag (cell, 1);
			
			if (entry == null){
				SizeF size = ComputeEntryPosition (tv, cell);
				entry = new UITextField (new RectangleF (size.Width, (cell.ContentView.Bounds.Height-size.Height)/2-1, 320-size.Width, size.Height)){
					Tag = 1,
					Placeholder = placeholder,
					SecureTextEntry = isPassword
				};
				entry.Text = Value ?? "";
				entry.AutoresizingMask = UIViewAutoresizing.FlexibleWidth |
					UIViewAutoresizing.FlexibleLeftMargin;
				
				entry.Ended += delegate {
					Value = entry.Text;
				};
				entry.ShouldReturn += delegate {
					EntryElement focus = null;
					foreach (var e in (Parent as Section).Elements){
						if (e == this)
							focus = this;
						else if (focus != null && e is EntryElement)
							focus = e as EntryElement;
					}
					if (focus != this)
						focus.entry.BecomeFirstResponder ();
					else
						focus.entry.ResignFirstResponder ();
					
					return true;
				};
			}
			
			cell.TextLabel.Text = Caption;
			cell.ContentView.AddSubview (entry);
			return cell;
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing){
				entry.Dispose ();
				entry = null;
			}
		}
	}
	
	public class DateTimeElement : StringElement {
		public DateTime DateValue;
		public UIDatePicker datePicker;
		protected internal NSDateFormatter fmt = new NSDateFormatter () {
			DateStyle = NSDateFormatterStyle.Short
		};
		
		public DateTimeElement (string caption, DateTime date) : base (caption)
		{
			DateValue = date;
			Value = FormatDate (date);
		}	
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			Value = FormatDate (DateValue);
			return base.GetCell (tv);
		}
 
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			if (disposing){
				fmt.Dispose ();
				fmt = null;
				if (datePicker != null){
					datePicker.Dispose ();
					datePicker = null;
				}
			}
		}
		
		public virtual string FormatDate (DateTime dt)
		{
			return fmt.ToString (dt) + " " + dt.ToLocalTime ().ToShortTimeString ();
		}
		
		public virtual UIDatePicker CreatePicker ()
		{
			var picker = new UIDatePicker (RectangleF.Empty){
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth,
				Mode = UIDatePickerMode.Date
			};
			return picker;
		}
		                                                                                                                                
		static RectangleF PickerFrameWithSize (SizeF size)
		{                                                                                                                                    
			var screenRect = UIScreen.MainScreen.ApplicationFrame;
			return new RectangleF (0f, screenRect.Height - 84f - size.Height, size.Width, size.Height);
		}                                                                                                                                    

		class MyViewController : UIViewController {
			DateTimeElement container;
			
			public MyViewController (DateTimeElement container)
			{
				this.container = container;
			}
			
			public override void ViewWillDisappear (bool animated)
			{
				base.ViewWillDisappear (animated);
				container.DateValue = container.datePicker.Date;
			}
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			var vc = new MyViewController (this);
			datePicker = CreatePicker ();
			datePicker.Frame = PickerFrameWithSize (datePicker.SizeThatFits (SizeF.Empty));
			                            
			vc.View.BackgroundColor = UIColor.Black;
			vc.View.AddSubview (datePicker);
			dvc.ActivateController (vc);
		}
	}
	
	public class DateElement : DateTimeElement {
		public DateElement (string caption, DateTime date) : base (caption, date)
		{
			fmt.DateStyle = NSDateFormatterStyle.Medium;
		}
		
		public override string FormatDate (DateTime dt)
		{
			return fmt.ToString (dt);
		}
		
		public override UIDatePicker CreatePicker ()
		{
			var picker = base.CreatePicker ();
			picker.Mode = UIDatePickerMode.Date;
			return picker;
		}
	}
	
	public class TimeElement : DateTimeElement {
		public TimeElement (string caption, DateTime date) : base (caption, date)
		{
		}
		
		public override string FormatDate (DateTime dt)
		{
			Console.WriteLine (dt.ToShortTimeString () + " - " + dt.ToLongTimeString ());
			return dt.ToLocalTime ().ToShortTimeString ();
		}
		
		public override UIDatePicker CreatePicker ()
		{
			var picker = base.CreatePicker ();
			picker.Mode = UIDatePickerMode.Time;
			return picker;
		}
	}
	
	/// <summary>
	/// Sections contain individual Element instances that are rendered by MonoTouch.Dialog
	/// </summary>
	/// <remarks>
	/// Sections are used to group elements in the screen and they are the
	/// only valid direct child of the RootElement.    Sections can contain
	/// any of the standard elements, including new RootElements.
	/// 
	/// RootElements embedded in a section are used to navigate to a new
	/// deeper level.
	/// </remarks>
	public class Section : Element, IEnumerable {
		public string Header, Footer;
		public List<Element> Elements = new List<Element> ();
		
		// X corresponds to the alignment, Y to the height of the password
		internal SizeF EntryAlignment;
		
		/// <summary>
		///  Constructs a Section without header or footers.
		/// </summary>
		public Section () : base (null) {}
		
		/// <summary>
		///  Constructs a Section with the specified header
		/// </summary>
		/// <param name="caption">
		/// The header to display
		/// </param>
		public Section (string caption) : base (caption)
		{
		}
		
		/// <summary>
		/// Constructs a Section with a header and a footer
		/// </summary>
		/// <param name="caption">
		/// The caption to display (or null to not display a caption)
		/// </param>
		/// <param name="footer">
		/// The footer to display.
		/// </param>
		public Section (string caption, string footer) : base (caption)
		{
			Footer = footer;
		}

		/// <summary>
		/// Adds a new child Element to the Section
		/// </summary>
		/// <param name="element">
		/// An element to add to the section.
		/// </param>
		public void Add (Element element)
		{
			if (element == null)
				return;
			
			Elements.Add (element);
			element.Parent = this;
		}

		/// <summary>
		/// Enumerator to get all the elements in the Section.
		/// </summary>
		/// <returns>
		/// A <see cref="IEnumerator"/>
		/// </returns>
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
	
	/// <summary>
	/// Used by root elements to fetch information when they need to
	/// render a summary (Checkbox count or selected radio group).
	/// </summary>
	public class Group {
		public string Key;
		public Group (string key)
		{
			Key = key;
		}
	}
	/// <summary>
	/// Captures the information about mutually exclusive elements in a RootElement
	/// </summary>
	public class RadioGroup : Group {
		public string Key;
		public int Selected;
		
		public RadioGroup (string key, int selected) : base (key)
		{
			Selected = selected;
		}
		
		public RadioGroup (int selected) : base (null)
		{
			Selected = selected;
		}
	}
	
	/// <summary>
	///    RootElements are responsible for showing a full configuration page.
	/// </summary>
	/// <remarks>
	///    At least one RootElement is required to start the MonoTouch.Dialogs
	///    process.   
	/// 
	///    RootElements can also be used inside Sections to trigger
	///    loading a new nested configuration page.   When used in this mode
	///    the caption provided is used while rendered inside a section and
	///    is also used as the Title for the subpage.
	/// 
	///    If a RootElement is initialized with a section/element value then
	///    this value is used to locate a child Element that will provide
	///    a summary of the configuration which is rendered on the right-side
	///    of the display.
	/// 
	///    RootElements are also used to coordinate radio elements.  The
	///    RadioElement members can span multiple Sections (for example to
	///    implement something similar to the ring tone selector and separate
	///    custom ring tones from system ringtones).
	/// 
	///    Sections are added by calling the Add method which supports the
	///    C# 4.0 syntax to initialize a RootElement in one pass.
	/// </remarks>
	public class RootElement : Element, IEnumerable {
		static NSString rkey = new NSString ("RootElement");
		int summarySection, summaryElement;
		internal Group group;
		
		/// <summary>
		///  Initializes a RootSection with a caption
		/// </summary>
		/// <param name="caption">
		///  The caption to render.
		/// </param>
		public RootElement (string caption) : base (caption)
		{
			summarySection = -1;
			Sections = new List<Section> ();
		}

		/// <summary>
		///   Initializes a RootElement with a caption with a summary fetched from the specified section and leement
		/// </summary>
		/// <param name="caption">
		/// The caption to render cref="System.String"/>
		/// </param>
		/// <param name="section">
		/// The section that contains the element with the summary.
		/// </param>
		/// <param name="element">
		/// The element index inside the section that contains the summary for this RootSection.
		/// </param>
		public RootElement (string caption, int section, int element) : base (caption)
		{
			summarySection = section;
			summaryElement = element;
		}
		
		/// <summary>
		/// Initializes a RootElement that renders the summary based on the radio settings of the contained elements. 
		/// </summary>
		/// <param name="caption">
		/// The caption to ender
		/// </param>
		/// <param name="group">
		/// The group that contains the checkbox or radio information.  This is used to display
		/// the summary information when a RootElement is rendered inside a section.
		/// </param>
		public RootElement (string caption, Group group) : base (caption)
		{
			this.group = group;
		}
		
		internal List<Section> Sections = new List<Section> ();

		internal NSIndexPath PathForRadio (int idx)
		{
			RadioGroup radio = group as RadioGroup;
			if (radio == null)
				return null;
			
			uint current = 0, section = 0;
			foreach (Section s in Sections){
				uint row = 0;
				
				foreach (Element e in s.Elements){
					if (!(e is RadioElement))
						continue;
					
					if (current == idx){
						return new NSIndexPath ().FromIndexes (new uint [] { section, row});
					}
					row++;
					current++;
				}
				section++;
			}
			return null;
		}
			
		internal void Prepare ()
		{
			var radio = group as RadioGroup;
			if (radio == null)
				return;
			
			int current = 0;
			foreach (Section s in Sections){
				int maxEntryWidth = -1;
				
				foreach (Element e in s.Elements){
					var re = e as RadioElement;
					if (re != null)
						re.RadioIdx = current++;
				}
			}
		}
		
		/// <summary>
		/// Adds a new section to this RootElement
		/// </summary>
		/// <param name="section">
		/// The section to add
		/// </param>
		public void Add (Section section)
		{
			if (section == null)
				return;
			
			Sections.Add (section);
			section.Parent = this;
		}

		/// <summary>
		/// Enumerator that returns all the sections in the RootElement.
		/// </summary>
		/// <returns>
		/// A <see cref="IEnumerator"/>
		/// </returns>
		public IEnumerator GetEnumerator ()
		{
			foreach (var s in Sections)
				yield return s;
		}

		/// <summary>
		/// The currently selected Radio item in the whole Root.
		/// </summary>
		public int RadioSelected {
			get {
				var radio = group as RadioGroup;
				if (radio != null)
					return radio.Selected;
				return -1;
			}
			set {
				var radio = group as RadioGroup;
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
			var radio = group as RadioGroup;
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
			} else if (group != null){
				int count = 0;
				
				foreach (var s in Sections){
					foreach (var e in s.Elements){
						var ce = e as CheckboxElement;
						if (ce != null){
							if (ce.Value)
								count++;
							continue;
						}
						var be = e as BooleanElement;
						if (be != null){
							if (be.Value)
								count++;
							continue;
						}
					}
				}
				cell.DetailTextLabel.Text = count.ToString ();
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
