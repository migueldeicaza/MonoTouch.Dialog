using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

namespace MonoTouch.Dialog
{
	public class NullableDateTimeElement : Element
	{
		static UIFont font = UIFont.BoldSystemFontOfSize (17);
		
		public DateTime? DateValue;
		public DateTime DefaultDateValue = DateTime.Today;
		public UIDatePicker datePicker;
		public UITextField entry;
		public bool AllowNull = true;
		public string NullValueLabel = "(no selection)";
		
		private UITextField dummyEntry;
		
		protected internal NSDateFormatter fmt = new NSDateFormatter() {
			DateStyle = NSDateFormatterStyle.Short
		};
		
		public NullableDateTimeElement(string caption, DateTime? date)
			: base (caption)
		{
			DateValue = date;
			Value = FormatDate(date);
		}

		public NullableDateTimeElement(string caption, DateTime? date, bool allowNull)
			: this(caption, date)
		{
			AllowNull = allowNull;
		}
		
		#region Properties
		public string Value { 
			get {
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
		static NSString entryKey = new NSString ("NullableDateTimeElement");
		protected virtual NSString EntryKey {
			get {
				return entryKey;
			}
		}

		#endregion
		
		public string FormatDate(DateTime? dt)
		{
			if (!dt.HasValue)
				return (NullValueLabel);
			return (fmt.ToString(dt.Value) + " " + dt.Value.ToLocalTime().ToShortTimeString());
		}
		
		private bool becomeResponder = false;
		
		public override UITableViewCell GetCell(UITableView tv)
		{
			Value = FormatDate(DateValue);
			
			var cell = tv.DequeueReusableCell (CellKey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, CellKey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			} else 
				RemoveTag (cell, 1);
			
			if (entry == null){
				SizeF size = ComputeEntryPosition (tv, cell);
				float yOffset = (cell.ContentView.Bounds.Height - size.Height) / 2 - 1;
				float width = cell.ContentView.Bounds.Width - size.Width - 5;
				
				entry = CreateTextField (new RectangleF (size.Width, yOffset, width, size.Height));
				dummyEntry = CreateTextField (new RectangleF (size.Width, yOffset, width, size.Height));
				entry.Alpha = 0;
				
				dummyEntry.ShouldBeginEditing = delegate {
					DateValue = (DateValue ?? DefaultDateValue);
					entry.BecomeFirstResponder();
					return (false);
				};
				dummyEntry.ShouldClear = delegate {
					DateValue = null;
					return (true);
				};
				dummyEntry.EditingChanged += delegate(object sender, EventArgs e) {
					if (string.IsNullOrEmpty((sender as UITextField).Text))
					{
						dummyEntry.Text = FormatDate(DateValue);
						dummyEntry.ClearButtonMode = UITextFieldViewMode.Never;
						entry.ResignFirstResponder();
					}
				};
				datePicker = CreatePicker();
				entry.InputView = datePicker;
			}
			if (becomeResponder){
				entry.BecomeFirstResponder ();
				becomeResponder = false;
			}
			cell.TextLabel.Text = Caption;
			cell.ContentView.AddSubview (entry);
			cell.ContentView.AddSubview (dummyEntry);

			return cell;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing) {
				if (fmt != null) {
					fmt.Dispose();
					fmt = null;
				}
				if (datePicker != null) {
					datePicker.Dispose();
					datePicker = null;
				}
			}
		}

		public virtual UIDatePicker CreatePicker()
		{
			var picker = new UIDatePicker(RectangleF.Empty){
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth,
				Mode = UIDatePickerMode.DateAndTime,
				Date = (DateValue ?? DefaultDateValue)
			};
			picker.ValueChanged += delegate(object sender, EventArgs e) {
				entry.Text = dummyEntry.Text = FormatDate((sender as UIDatePicker).Date);
				dummyEntry.ClearButtonMode = UITextFieldViewMode.Always;
			};
			return picker;
		}

		SizeF ComputeEntryPosition(UITableView tv, UITableViewCell cell)
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

		protected virtual UITextField CreateTextField(RectangleF frame)
		{
			return new UITextField (frame) {
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleLeftMargin,
				Placeholder = "",
				SecureTextEntry = false,
				Text = Value ?? "",
				Tag = 1,
				TextAlignment = UITextAlignment.Right,
				ClearButtonMode = (AllowNull && DateValue.HasValue ? UITextFieldViewMode.Always : UITextFieldViewMode.Never)
			};
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			BecomeFirstResponder(true);
			tableView.DeselectRow (indexPath, true);
		}

		public void BecomeFirstResponder (bool animated)
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

		public void ResignFirstResponder (bool animated)
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
