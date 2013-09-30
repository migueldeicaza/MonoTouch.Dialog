
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
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace MonoTouch.Dialog
{
	
	/// <summary>
	/// An element that can be used to enter text.
	/// </summary>
	/// <remarks>
	/// This element can be used to enter text both regular and password protected entries. 
	///     
	/// The Text fields in a given section are aligned with each other.
	/// </remarks>
	public class EntryElement : Element {
		/// <summary>
		///   The value of the EntryElement
		/// </summary>
		public string Value { 
			get {
				if (entry == null)
					return val;
				var newValue = entry.Text;
				if (newValue == val)
					return val;
				val = newValue;

				if (Changed != null)
					Changed (this, EventArgs.Empty);
				return val;
			}
			set {
				val = value;
				if (entry != null)
					entry.Text = value;
			}
		}
		protected string val;

		/// <summary>
		/// The key used for reusable UITableViewCells.
		/// </summary>
		static NSString entryKey = new NSString ("EntryElement");
		protected virtual NSString EntryKey {
			get {
				return entryKey;
			}
		}

		/// <summary>
		/// The type of keyboard used for input, you can change
		/// this to use this for numeric input, email addressed,
		/// urls, phones.
		/// </summary>
		public UIKeyboardType KeyboardType {
			get {
				return keyboardType;
			}
			set {
				keyboardType = value;
				if (entry != null)
					entry.KeyboardType = value;
			}
		}
		
		/// <summary>
		/// The type of Return Key that is displayed on the
		/// keyboard, you can change this to use this for
		/// Done, Return, Save, etc. keys on the keyboard
		/// </summary>
		public UIReturnKeyType? ReturnKeyType {
			get {
				return returnKeyType;
			}
			set {
				returnKeyType = value;
				if (entry != null && returnKeyType.HasValue)
					entry.ReturnKeyType = returnKeyType.Value;
			}
		}
		
		public UITextAutocapitalizationType AutocapitalizationType {
			get {
				return autocapitalizationType;	
			}
			set { 
				autocapitalizationType = value;
				if (entry != null)
					entry.AutocapitalizationType = value;
			}
		}
		
		public UITextAutocorrectionType AutocorrectionType { 
			get { 
				return autocorrectionType;
			}
			set { 
				autocorrectionType = value;
				if (entry != null)
					this.autocorrectionType = value;
			}
		}
		
		public UITextFieldViewMode ClearButtonMode { 
			get { 
				return clearButtonMode;
			}
			set { 
				clearButtonMode = value;
				if (entry != null)
					entry.ClearButtonMode = value;
			}
		}

		public UITextAlignment TextAlignment {
			get {
				return textalignment;
			}
			set{
				textalignment = value;
				if (entry != null) {
					entry.TextAlignment = textalignment;
				}
			}
		}
		UITextAlignment textalignment = UITextAlignment.Left;
		UIKeyboardType keyboardType = UIKeyboardType.Default;
		UIReturnKeyType? returnKeyType = null;
		UITextAutocapitalizationType autocapitalizationType = UITextAutocapitalizationType.Sentences;
		UITextAutocorrectionType autocorrectionType = UITextAutocorrectionType.Default;
		UITextFieldViewMode clearButtonMode = UITextFieldViewMode.Never;
		bool isPassword, becomeResponder;
		UITextField entry;
		string placeholder;
		static UIFont font = UIFont.BoldSystemFontOfSize (17);

		public event EventHandler Changed;
		public event Func<bool> ShouldReturn;
		public EventHandler EntryStarted {get;set;}
		public EventHandler EntryEnded {get;set;}
		/// <summary>
		/// Constructs an EntryElement with the given caption, placeholder and initial value.
		/// </summary>
		/// <param name="caption">
		/// The caption to use
		/// </param>
		/// <param name="placeholder">
		/// Placeholder to display when no value is set.
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
		/// Constructs an EntryElement for password entry with the given caption, placeholder and initial value.
		/// </summary>
		/// <param name="caption">
		/// The caption to use.
		/// </param>
		/// <param name="placeholder">
		/// Placeholder to display when no value is set.
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
			
			// If all EntryElements have a null Caption, align UITextField with the Caption
			// offset of normal cells (at 10px).
			SizeF max = new SizeF (-15, tv.StringSize ("M", font).Height);
			foreach (var e in s.Elements){
				var ee = e as EntryElement;
				if (ee == null)
					continue;
				
				if (ee.Caption != null) {
					var size = tv.StringSize (ee.Caption, font);
					if (size.Width > max.Width)
						max = size;
				}
			}
			s.EntryAlignment = new SizeF (25 + Math.Min (max.Width, 160), max.Height);
			return s.EntryAlignment;
		}

		protected virtual UITextField CreateTextField (RectangleF frame)
		{
			return new UITextField (frame) {
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleLeftMargin,
				Placeholder = placeholder ?? "",
				SecureTextEntry = isPassword,
				Text = Value ?? "",
				Tag = 1,
				TextAlignment = textalignment,
				ClearButtonMode = ClearButtonMode
			};
		}

		static readonly NSString passwordKey = new NSString ("EntryElement+Password");
		static readonly NSString cellkey = new NSString ("EntryElement");
		
		protected override NSString CellKey {
			get {
				return isPassword ? passwordKey : cellkey;
			}
		}

		UITableViewCell cell;
		public override UITableViewCell GetCell (UITableView tv)
		{
			if (cell == null) {
				cell = new UITableViewCell (UITableViewCellStyle.Default, CellKey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
				cell.TextLabel.Font = font;

			} 
			cell.TextLabel.Text = Caption;

			var offset = (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone) ? 20 : 90;
			cell.Frame = new RectangleF(cell.Frame.X, cell.Frame.Y, tv.Frame.Width-offset, cell.Frame.Height);
			SizeF size = ComputeEntryPosition (tv, cell);
			float yOffset = (cell.ContentView.Bounds.Height - size.Height) / 2 - 1;
			float width = cell.ContentView.Bounds.Width - size.Width;
			if (textalignment == UITextAlignment.Right) {
				// Add padding if right aligned
				width -= 10;
			}
			var entryFrame = new RectangleF (size.Width, yOffset, width, size.Height);

			if (entry == null) {
				entry = CreateTextField (entryFrame);
				entry.ValueChanged += delegate {
					FetchValue ();
				};
				entry.Ended += delegate {					
					FetchValue ();
					if (EntryEnded != null) {
						EntryEnded (this, null);
					}
				};
				entry.ShouldReturn += delegate {
					
					if (ShouldReturn != null)
						return ShouldReturn ();
					
					RootElement root = GetImmediateRootElement ();
					EntryElement focus = null;
					
					if (root == null)
						return true;
					
					foreach (var s in root.Sections) {
						foreach (var e in s.Elements) {
							if (e == this) {
								focus = this;
							} else if (focus != null && e is EntryElement) {
								focus = e as EntryElement;
								break;
							}
						}
						
						if (focus != null && focus != this)
							break;
					}
					
					if (focus != this)
						focus.BecomeFirstResponder (true);
					else 
						focus.ResignFirstResponder (true);
					
					return true;
				};
				entry.Started += delegate {
					EntryElement self = null;
					
					if (EntryStarted != null) {
						EntryStarted (this, null);
					}
					
					if (!returnKeyType.HasValue) {
						var returnType = UIReturnKeyType.Default;
						
						foreach (var e in (Parent as Section).Elements) {
							if (e == this)
								self = this;
							else if (self != null && e is EntryElement)
								returnType = UIReturnKeyType.Next;
						}
						entry.ReturnKeyType = returnType;
					} else
						entry.ReturnKeyType = returnKeyType.Value;
					
					tv.ScrollToRow (IndexPath, UITableViewScrollPosition.Middle, true);
				};
				cell.ContentView.AddSubview (entry);
			} else
				entry.Frame = entryFrame;

			if (becomeResponder){
				entry.BecomeFirstResponder ();
				becomeResponder = false;
			}
			entry.KeyboardType = KeyboardType;
			
			entry.AutocapitalizationType = AutocapitalizationType;
			entry.AutocorrectionType = AutocorrectionType;

			return cell;
		}
		
		/// <summary>
		///  Copies the value from the UITextField in the EntryElement to the
		//   Value property and raises the Changed event if necessary.
		/// </summary>
		public void FetchValue ()
		{
			if (entry == null)
				return;

			var newValue = entry.Text;
			if (newValue == Value)
				return;
			
			Value = newValue;
			
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing){
				if (entry != null){
					entry.Dispose ();
					entry = null;
				}
			}
		}

		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			BecomeFirstResponder(true);
			tableView.DeselectRow (indexPath, true);
		}
		
		public override bool Matches (string text)
		{
			return (Value != null ? Value.IndexOf (text, StringComparison.CurrentCultureIgnoreCase) != -1: false) || base.Matches (text);
		}
		
		/// <summary>
		/// Makes this cell the first responder (get the focus)
		/// </summary>
		/// <param name="animated">
		/// Whether scrolling to the location of this cell should be animated
		/// </param>
		public virtual void BecomeFirstResponder (bool animated)
		{
			becomeResponder = true;
			var tv = GetContainerTableView ();
			if (tv == null)
				return;
			tv.ScrollToRow (IndexPath, UITableViewScrollPosition.Middle, animated);
			if (entry != null){
				entry.BecomeFirstResponder ();
				becomeResponder = false;
			}
		}

		public virtual void ResignFirstResponder (bool animated)
		{
			becomeResponder = false;
			var tv = GetContainerTableView ();
			if (tv == null)
				return;
			tv.ScrollToRow (IndexPath, UITableViewScrollPosition.Middle, animated);
			if (entry != null)
				entry.ResignFirstResponder ();
		}
	}
	
}
