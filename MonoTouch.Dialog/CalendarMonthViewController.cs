
using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
namespace MonoTouch.Dialog
{
	public class CalendarMonthViewController : UIViewController
    {
		DateTimeElement container;
		DateTime CurrentDate;
		public CalendarMonthViewController ()
		{
			CurrentDate = DateTime.Today;
		}
		public CalendarMonthViewController(DateTimeElement currentDate)
		{
			container = currentDate;
			CurrentDate = container.DateValue;
		}
        public CalendarMonthView MonthView;

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
			container.DateValue = CurrentDate;
			container.Value = CurrentDate.ToString("MM/dd/yyyy");
		}
        public override void ViewDidLoad()
        {
            MonthView = new CalendarMonthView(CurrentDate);
			MonthView.OnDateSelected += (date) => {
				CurrentDate = date;
				Console.WriteLine(String.Format("Selected {0}", date.ToShortDateString()));
			};
			MonthView.OnFinishedDateSelection += (date) => {
				Console.WriteLine(String.Format("Finished selecting {0}", date.ToShortDateString()));
			};
			MonthView.IsDayMarkedDelegate += (date) => {
				return (date.Day % 2==0) ? true : false;
			};
            View.AddSubview(MonthView);
        }
		
        public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
        {
            return false;
        }

    }
}
