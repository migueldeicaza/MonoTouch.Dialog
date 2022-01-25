using System;
using System.Drawing;

using UIKit;
using Foundation;
using CoreGraphics;
using ObjCRuntime;

namespace MonoTouch.Dialog {

	public partial class MessageSummaryView : UIView {
#if NET6_0 && !NET7_0
		static UIFont SenderFont = UIFont.BoldSystemFontOfSize (new NFloat (19));
		static UIFont SubjectFont = UIFont.SystemFontOfSize (new NFloat (14));
		static UIFont TextFont = UIFont.SystemFontOfSize (new NFloat (13));
		static UIFont CountFont = UIFont.BoldSystemFontOfSize (new NFloat (13));
#else
		static UIFont SenderFont = UIFont.BoldSystemFontOfSize (19);
		static UIFont SubjectFont = UIFont.SystemFontOfSize (14);
		static UIFont TextFont = UIFont.SystemFontOfSize (13);
		static UIFont CountFont = UIFont.BoldSystemFontOfSize (13);
#endif
		public string Sender { get; private set; }
		public string Body { get; private set; }
		public string Subject { get; private set; }
		public DateTime Date { get; private set; }
		public bool NewFlag  { get; private set; }
		public int MessageCount  { get; private set; }
		
		static CGGradient gradient;
		
		static MessageSummaryView ()
		{
			using (var colorspace = CGColorSpace.CreateDeviceRGB ()){
#if NET6_0 && !NET7_0
				gradient = new CGGradient (colorspace, new nfloat [] { /* first */ new NFloat (.52f), new NFloat (.69f), new NFloat (.96f), new NFloat (1), /* second */ new NFloat (.12f), new NFloat (.31f), new NFloat (.67f), new NFloat (1) }, null); //new float [] { 0, 1 });
#else
				gradient = new CGGradient (colorspace, new nfloat [] { /* first */ .52f, .69f, .96f, 1, /* second */ .12f, .31f, .67f, 1 }, null); //new float [] { 0, 1 });
#endif
			}
		}
		
		public MessageSummaryView ()
		{
			BackgroundColor = UIColor.White;
		}
		
		public void Update (string sender, string body, string subject, DateTime date, bool newFlag, int messageCount)
		{
			Sender = sender;
			Body = body;
			Subject = subject;
			Date = date;
			NewFlag = newFlag;
			MessageCount = messageCount;
			SetNeedsDisplay ();
		}
		
		public override void Draw (CGRect rect)
		{
			const int padright = 21;
			var ctx = UIGraphics.GetCurrentContext ();
			nfloat boxWidth;
			CGSize ssize;
			
			if (MessageCount > 0){
				var ms = MessageCount.ToString ();
				ssize = ms.StringSize (CountFont);
#if NET6_0 && !NET7_0
				boxWidth = new NFloat (Math.Min (22 + ssize.Width.Value, 18));
				var crect = new CGRect (new NFloat (Bounds.Width.Value-20-boxWidth.Value), new NFloat (32), boxWidth, new NFloat (16));
#else
				boxWidth = (nfloat)Math.Min (22 + ssize.Width, 18);
				var crect = new CGRect (Bounds.Width-20-boxWidth, 32, boxWidth, 16);
#endif
				
				UIColor.Gray.SetFill ();
#if NET6_0 && !NET7_0
				GraphicsUtil.FillRoundedRect (ctx, crect, new NFloat (3));
#else
				GraphicsUtil.FillRoundedRect (ctx, crect, 3);
#endif
				UIColor.White.SetColor ();
#if NET6_0 && !NET7_0
				crect.X = new NFloat (crect.X.Value + 5);
#else
				crect.X += 5;
#endif
				ms.DrawString (crect, CountFont);
				
#if NET6_0 && !NET7_0
				boxWidth = new NFloat (boxWidth.Value + padright);
#else
				boxWidth += padright;
#endif
			} else
#if NET6_0 && !NET7_0
				boxWidth = new NFloat (0);
#else
				boxWidth = 0;
#endif
			
			UIColor.FromRGB (36, 112, 216).SetColor ();
			var diff = DateTime.Now - Date;
			var now = DateTime.Now;
			string label;
			if (now.Day == Date.Day && now.Month == Date.Month && now.Year == Date.Year)
				label = Date.ToShortTimeString ();
			else if (diff <= TimeSpan.FromHours (24))
				label = "Yesterday".GetText ();
			else if (diff < TimeSpan.FromDays (6))
				label = Date.ToString ("dddd");
			else
				label = Date.ToShortDateString ();
			ssize = label.StringSize (SubjectFont);
#if NET6_0 && !NET7_0
			nfloat dateSize = new NFloat (ssize.Width.Value + padright + 5);
			label.DrawString (new CGRect (new NFloat (Bounds.Width.Value-dateSize.Value), new NFloat (6), dateSize, new NFloat (14)), SubjectFont, UILineBreakMode.Clip, UITextAlignment.Left);
#else
			nfloat dateSize = ssize.Width + padright + 5;
			label.DrawString (new CGRect (Bounds.Width-dateSize, 6, dateSize, 14), SubjectFont, UILineBreakMode.Clip, UITextAlignment.Left);
#endif
			
			const int offset = 33;
#if NET6_0 && !NET7_0
			nfloat bw = new NFloat (Bounds.Width.Value-offset);
#else
			nfloat bw = Bounds.Width-offset;
#endif
			
			UIColor.Black.SetColor ();
#if NET6_0 && !NET7_0
			Sender.DrawString (new CGPoint (offset, 2), (float)(bw.Value-dateSize.Value), SenderFont, UILineBreakMode.TailTruncation);
			Subject.DrawString (new CGPoint (offset, 23), (float)(bw.Value-offset-boxWidth.Value), SubjectFont, UILineBreakMode.TailTruncation);
#else
			Sender.DrawString (new CGPoint (offset, 2), (float)(bw-dateSize), SenderFont, UILineBreakMode.TailTruncation);
			Subject.DrawString (new CGPoint (offset, 23), (float)(bw-offset-boxWidth), SubjectFont, UILineBreakMode.TailTruncation);
#endif
			
			//UIColor.Black.SetFill ();
			//ctx.FillRect (new CGRect (offset, 40, bw-boxWidth, 34));
			UIColor.Gray.SetColor ();
#if NET6_0 && !NET7_0
			Body.DrawString (new CGRect (new NFloat (offset), new NFloat (40), new NFloat (bw.Value-boxWidth.Value), new NFloat (34)), TextFont, UILineBreakMode.TailTruncation, UITextAlignment.Left);
#else
			Body.DrawString (new CGRect (offset, 40, bw-boxWidth, 34), TextFont, UILineBreakMode.TailTruncation, UITextAlignment.Left);
#endif
			
			if (NewFlag){
				ctx.SaveState ();
				ctx.AddEllipseInRect (new CGRect (10, 32, 12, 12));
				ctx.Clip ();
				ctx.DrawLinearGradient (gradient, new CGPoint (10, 32), new CGPoint (22, 44), CGGradientDrawingOptions.DrawsAfterEndLocation);
				ctx.RestoreState ();
			}
			
#if WANT_SHADOWS
			ctx.SaveState ();
			UIColor.FromRGB (78, 122, 198).SetStroke ();
			ctx.SetShadow (new CGSize (1, 1), 3);
			ctx.StrokeEllipseInRect (new CGRect (10, 32, 12, 12));
			ctx.RestoreState ();
#endif
		}
	}
		
	public class MessageElement : Element, IElementSizing {
		static NSString mKey = new NSString ("MessageElement");
		
		public string Sender, Body, Subject;
		public DateTime Date;
		public bool NewFlag;
		public int MessageCount;
		
		class MessageCell : UITableViewCell {
			MessageSummaryView view;
			
			public MessageCell () : base (UITableViewCellStyle.Default, mKey)
			{
				view = new MessageSummaryView ();
				ContentView.Add (view);
				Accessory = UITableViewCellAccessory.DisclosureIndicator;
			}
			
			public void Update (MessageElement me)
			{
				view.Update (me.Sender, me.Body, me.Subject, me.Date, me.NewFlag, me.MessageCount);
			}
			
			public override void LayoutSubviews ()
			{
				base.LayoutSubviews ();
				view.Frame = ContentView.Bounds;
				view.SetNeedsDisplay ();
			}
		}
		
		public MessageElement () : base ("")
		{
		}
		
		public MessageElement (Action<DialogViewController,UITableView,NSIndexPath> tapped) : base ("")
		{
			Tapped += tapped;
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (mKey) as MessageCell;
			if (cell == null)
				cell = new MessageCell ();
			cell.Update (this);
			return cell;
		}
		
		public nfloat GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
#if NET6_0 && !NET7_0
			return new NFloat (78);
#else
			return 78;
#endif
		}
		
		public event Action<DialogViewController, UITableView, NSIndexPath> Tapped;
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			if (Tapped != null)
				Tapped (dvc, tableView, path);
		}

		public override bool Matches (string text)
		{
			if (Sender != null && Sender.IndexOf (text, StringComparison.CurrentCultureIgnoreCase) != -1)
				return true;
			if (Body != null && Body.IndexOf (text, StringComparison.CurrentCultureIgnoreCase) != -1)
				return true;
			if (Subject != null && Subject.IndexOf (text, StringComparison.CurrentCultureIgnoreCase) != -1)
				return true;

			return false;
		}
	}
}

