using System;
using System.Drawing;
using MonoTouch.CoreFoundation;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using MonoTouch.Foundation;

namespace Sample
{
	public partial class AppDelegate
	{
		private const string SmallText="Lorem ipsum dolor sit amet";
		private const string MediumText = "Integer molestie rhoncus bibendum. Cras ullamcorper magna a enim laoreet";
		private const string LargeText = "Phasellus laoreet, massa non cursus porttitor, sapien tellus placerat metus, vitae ornare urna mi sit amet dui.";
		private const string WellINeverWhatAWhopperString="Nulla mattis tempus placerat. Curabitur euismod ullamcorper lorem. Praesent massa magna, pulvinar nec condimentum ac, blandit blandit mi. Donec vulputate sapien a felis aliquam consequat. Sed sit amet libero non sem rhoncus semper sed at tortor.";		

		
		public void DemoOwnerDrawnElement () 
		{
			var root = new RootElement("Owner Drawn") {
				new Section() {
					new SampleOwnerDrawnElement("000 - "+SmallText, DateTime.Now, "David Black"),
					new SampleOwnerDrawnElement("001 - "+MediumText, DateTime.Now - TimeSpan.FromDays(1), "Peter Brian Telescope"),
					new SampleOwnerDrawnElement("002 - "+LargeText, DateTime.Now - TimeSpan.FromDays(3), "John Raw Vegitable"),
					new SampleOwnerDrawnElement("003 - "+SmallText, DateTime.Now - TimeSpan.FromDays(5), "Tarquin Fintimlinbinwhinbimlim Bus Stop F'tang  F'tang Ole  Biscuit-Barrel"),
					new SampleOwnerDrawnElement("004 - "+WellINeverWhatAWhopperString, DateTime.Now - TimeSpan.FromDays(9), "Kevin Phillips Bong"),
					new SampleOwnerDrawnElement("005 - "+LargeText, DateTime.Now - TimeSpan.FromDays(11), "Alan Jones"),
					new SampleOwnerDrawnElement("006 - "+MediumText, DateTime.Now - TimeSpan.FromDays(32), "Mrs Elsie Zzzzzzzzzzzzzzz"),
					new SampleOwnerDrawnElement("007 - "+SmallText, DateTime.Now - TimeSpan.FromDays(45), "Jeanette Walker"),
					new SampleOwnerDrawnElement("008 - "+MediumText, DateTime.Now - TimeSpan.FromDays(99), "Adrian  Blackpool Rock  Stoatgobblerk"),
					new SampleOwnerDrawnElement("009 - "+SmallText, DateTime.Now - TimeSpan.FromDays(123), "Thomas Moo"),
				}
			};
			root.UnevenRows = true;
			var dvc = new DialogViewController (root, true);
			
			navigation.PushViewController (dvc, true);
		}
	}
	
	
	/// <summary>
	/// This is an example of implementing the OwnerDrawnElement abstract class.
	/// It makes it very simple to create an element that you draw using CoreGraphics
	/// </summary>
	public class SampleOwnerDrawnElement : OwnerDrawnElement
	{
		CGGradient gradient;
		private UIFont subjectFont = UIFont.SystemFontOfSize(10.0f);
		private UIFont fromFont = UIFont.BoldSystemFontOfSize(14.0f);
		private UIFont dateFont = UIFont.BoldSystemFontOfSize(14.0f);
		

		public SampleOwnerDrawnElement (string text, DateTime sent, string from) : base(UITableViewCellStyle.Default, "sampleOwnerDrawnElement")
		{
			this.Subject = text;
			this.From = from;
			this.Sent = FormatDate(sent);
			CGColorSpace colorSpace = CGColorSpace.CreateDeviceRGB();
			gradient = new CGGradient(
			    colorSpace,
			    new float[] { 0.95f, 0.95f, 0.95f, 1, 
							  0.85f, 0.85f, 0.85f, 1},
				new float[] { 0, 1 } );
		}
		
		public string Subject
		{
			get; set; 
		}
		
		public string From
		{
			get; set; 
		}

		public string Sent
		{
			get; set; 
		}
		
		
		public string FormatDate (DateTime date)
		{
	
			if (DateTime.Today == date.Date) {
				return date.ToString ("hh:mm");
			} else if ((DateTime.Today - date.Date).TotalDays < 7) {
				return date.ToString ("ddd hh:mm");
			} else
			{
				return date.ToShortDateString ();			
			}
		}
		
		public override void Draw (RectangleF bounds, CGContext context, UIView view)
		{
			UIColor.White.SetFill ();
			context.FillRect (bounds);
			
			context.DrawLinearGradient (gradient, new PointF (bounds.Left, bounds.Top), new PointF (bounds.Left, bounds.Bottom), CGGradientDrawingOptions.DrawsAfterEndLocation);
			
			UIColor.Black.SetColor ();
			view.DrawString(this.From, new RectangleF(10, 5, bounds.Width/2, 10 ), fromFont, UILineBreakMode.TailTruncation);
			
			UIColor.Brown.SetColor ();
			view.DrawString(this.Sent, new RectangleF(bounds.Width/2, 5, (bounds.Width/2) - 10, 10 ), dateFont, UILineBreakMode.TailTruncation, UITextAlignment.Right);
			
			UIColor.DarkGray.SetColor();
			view.DrawString(this.Subject, new RectangleF(10, 30, bounds.Width - 20, TextHeight(bounds) ), subjectFont, UILineBreakMode.WordWrap);
		}
		
		public override float Height (RectangleF bounds)
		{
			var height = 40.0f + TextHeight (bounds);
			return height;
		}
		
		private float TextHeight (RectangleF bounds)
		{
			SizeF size;
			using (NSString str = new NSString (this.Subject))
			{
				size = str.StringSize (subjectFont, new SizeF (bounds.Width - 20, 1000), UILineBreakMode.WordWrap);
			}			
			return size.Height;
		}
		
		public override string ToString ()
		{
			return string.Format (Subject);
		}
	}
}

