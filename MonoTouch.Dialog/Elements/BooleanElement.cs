
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
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace MonoTouch.Dialog
{
	
	/// <summary>
	/// Used to display switch on the screen.
	/// </summary>
	public class BooleanElement : BoolElement {
		static NSString bkey = new NSString ("BooleanElement");
		UISwitch sw;
		
		public BooleanElement (string caption, bool value) : base (caption, value)
		{  }
		
		public BooleanElement (string caption, bool value, string key) : base (caption, value)
		{  }
		
		protected override NSString CellKey {
			get {
				return bkey;
			}
		}
		public override UITableViewCell GetCell (UITableView tv)
		{
			if (sw == null){
				sw = new UISwitch (){
					BackgroundColor = UIColor.Clear,
					Tag = 1,
					On = Value
				};
				sw.AddTarget (delegate {
					Value = sw.On;
				}, UIControlEvent.ValueChanged);
			} else
				sw.On = Value;
			
			var cell = tv.DequeueReusableCell (CellKey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, CellKey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			} else
				RemoveTag (cell, 1);
		
			cell.TextLabel.Text = Caption;
			cell.AccessoryView = sw;

			return cell;
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing){
				if (sw != null){
					sw.Dispose ();
					sw = null;
				}
			}
		}
		
		public override bool Value {
			get {
				return base.Value;
			}
			set {
				 base.Value = value;
				if (sw != null)
					sw.On = value;
			}
		}
	}
	
}
