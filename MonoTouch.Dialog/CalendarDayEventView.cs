using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.CoreAnimation;
using System.Linq;
using MonoTouch.EventKit;
using MonoTouch.EventKitUI;
using MonoTouch.ObjCRuntime;
using MonoTouch.CoreGraphics;

namespace MonoTouch.Dialog
{
    internal class Block
    {
        internal ArrayList Columns;
        public ArrayList events = new ArrayList();


        internal Block()
        {
        }

        internal void Add(CalendarDayEventView ev)
        {
            events.Add(ev);
            ArrangeColumns();
        }

        private BlockColumn createColumn()
        {
            BlockColumn col = new BlockColumn();
            this.Columns.Add(col);
            col.Block = this;

            return col;
        }

        public void ArrangeColumns()
        {
            // cleanup
            this.Columns = new ArrayList();

            foreach (CalendarDayEventView e in events)
                e.Column = null;

            // there always will be at least one column because arrangeColumns is called only from Add()
            createColumn();

            foreach (CalendarDayEventView e in events)
            {
                foreach (BlockColumn col in Columns)
                {
                    if (col.CanAdd(e))
                    {
                        col.Add(e);
                        break;
                    }
                }
                // it wasn't placed 
                if (e.Column == null)
                {
                    BlockColumn col = createColumn();
                    col.Add(e);
                }
            }
        }


        internal bool OverlapsWith(CalendarDayEventView e)
        {
            if (events.Count == 0)
                return false;

            return (this.BoxStart < e.BoxEnd && this.BoxEnd > e.startDate);
        }

        internal DateTime BoxStart
        {
            get
            {
                return (from CalendarDayEventView e in events.ToArray()
                        select e.BoxStart).Min();
            }
        }

        internal DateTime BoxEnd
        {
            get
            {
                return (from CalendarDayEventView e in events.ToArray()
                        select e.BoxEnd).Max();
            }
        }
    }

    internal class BlockColumn
    {
        private ArrayList events = new ArrayList();
        internal Block Block;

        internal BlockColumn()
        {
        }

        internal bool CanAdd(CalendarDayEventView e)
        {
            foreach (CalendarDayEventView ev in events)
            {
                if (ev.OverlapsWith(e))
                    return false;
            }
            return true;
        }

        internal void Add(CalendarDayEventView e)
        {
            if (e.Column != null)
                throw new ApplicationException("This Event was already placed into a Column.");

            events.Add(e);
            e.Column = this;
        }

        /// <summary>
        /// Gets the order number of the column.
        /// </summary>
        public int Number
        {
            get
            {
                if (Block == null)
                    throw new ApplicationException("This Column doesn't belong to any Block.");

                return Block.Columns.IndexOf(this);
            }
        }
    }
	
	public class RotatingCalendarView : RotatingViewController
	{	
		public EventClicked OnEventDoubleClicked;
		public EventClicked OnEventClicked;
		public DateTime CurrentDate{get;internal set;}
		public DateTime FirstDayOfWeek {get;set;}
		private UIBarButtonItem _leftButton, _rightButton;
		public CalendarDayTimelineView SingleDayView{get;set;}
		public TrueWeekView WeekView {get;set;}
		public TrueWeekViewController WeekController{get;set;}
		public NSAction AddNewEvent{get;set;}
		
		public override void ViewWillAppear (bool animated)
		{
			WeekView.isVisible = true;
			SingleDayView.isVisible = true;
			base.ViewWillAppear (animated);
		}
		public override void ViewWillDisappear (bool animated)
		{
			WeekView.isVisible = false;
			SingleDayView.isVisible = false;
			base.ViewWillDisappear (animated);
		}
		public RotatingCalendarView(RectangleF rect,  float tabBarHeight)
		{
			
			notificationObserver  = NSNotificationCenter.DefaultCenter
					.AddObserver("EKEventStoreChangedNotification", EventsChanged );
			
			CurrentDate	= DateTime.Today;
			SingleDayView  = new CalendarDayTimelineView(rect,tabBarHeight);
			WeekView = new TrueWeekView(CurrentDate);
			WeekView.UseCalendar = true;
			this.LandscapeLeftView = WeekView;
			this.LandscapeRightView = WeekView;
			this.PortraitView = SingleDayView;
			SingleDayView.OnEventClicked += (theEvent) => {
				if (theEvent != null)
				{
					if (OnEventClicked != null)
					{
						OnEventClicked(theEvent);	
					}
				}
			};
			
			WeekView.OnEventClicked += (theEvent) => {
				if (theEvent != null)
				{
					if (OnEventClicked != null)
					{
						OnEventClicked(theEvent);	
					}
				}
			};
			SingleDayView.dateChanged += (theDate) =>
			{
				CurrentDate = theDate;	
			};
		
		}
		private void landScapeNavBar()
		{
			_leftButton = new UIBarButtonItem(Util.FromResource(null,"leftarrow.png"), UIBarButtonItemStyle.Bordered,HandlePreviousWeekTouch);
			this.NavigationItem.LeftBarButtonItem = _leftButton;
			this.NavigationItem.Title = WeekView.FirstDayOfWeek.ToShortDateString() + " - " + WeekView.FirstDayOfWeek.AddDays(6).ToShortDateString();
            _rightButton = new UIBarButtonItem(Util.FromResource(null,"rightarrow.png"), UIBarButtonItemStyle.Bordered,HandleNextWeekTouch);
            this.NavigationItem.RightBarButtonItem = _rightButton;
		
		}
		
		private void portriatNavBar()
		{
            _leftButton = new UIBarButtonItem("Calendars",UIBarButtonItemStyle.Bordered,HandlePreviousDayTouch);
			this.NavigationItem.LeftBarButtonItem = _leftButton;
			this.NavigationItem.Title =  "";
            _rightButton = new UIBarButtonItem(UIBarButtonSystemItem.Add,AddNewEventClicked);
            this.NavigationItem.RightBarButtonItem = _rightButton;
		}
		
		private void EventsChanged(NSNotification notification)
		{
			WeekView.EventsNeedRefresh = true;
			SingleDayView.EventsNeedRefresh = true;
			switch (UIDevice.CurrentDevice.Orientation){
				case  UIDeviceOrientation.Portrait:
					SingleDayView.reloadDay();
					break;
				case UIDeviceOrientation.LandscapeLeft:
					WeekView.reloadDay();
					break;
				case UIDeviceOrientation.LandscapeRight:
					WeekView.reloadDay();
					break;
			}
			
			
		}
		
		public override void SetupNavBar ()
        {
			switch (this.InterfaceOrientation){

				case  UIInterfaceOrientation.Portrait:
					portriatNavBar();
					break;

				case UIInterfaceOrientation.LandscapeLeft:
					landScapeNavBar();
					break;
				case UIInterfaceOrientation.LandscapeRight:
					landScapeNavBar();
					break;
			}
			
	
        }
		private void setDate(DateTime date)
		{
			this.CurrentDate = date;
			WeekView.SetDayOfWeek(CurrentDate);
			SingleDayView.currentDate = CurrentDate;
			if (UIDevice.CurrentDevice.Orientation != UIDeviceOrientation.Portrait)
			{
				this.NavigationItem.Title = WeekView.FirstDayOfWeek.ToShortDateString() + " - " + WeekView.FirstDayOfWeek.AddDays(6).ToShortDateString();
				WeekView.ReDraw();
			}
		}
		void HandleNextWeekTouch(object sender, EventArgs e)
		{
			setDate(CurrentDate.AddDays(7));
		}
		void HandlePreviousWeekTouch(object sender, EventArgs e)
		{
			setDate( CurrentDate.AddDays(-7));
		}
		void HandlePreviousDayTouch (object sender, EventArgs e)
        {
			
			
        }
        void AddNewEventClicked (object sender, EventArgs e)
        {
			
			if (AddNewEvent != null)
				AddNewEvent();
				
        }
	}

	public class CalendarDayEventViewController : UIViewController
	{
		
		public EventClicked OnEventDoubleClicked;
		public EventClicked OnEventClicked;
		public clicked AddNewEvent;
		private UIBarButtonItem _leftButton, _rightButton;
		public List<EKEvent> Events;
		public CalendarDayTimelineView DayView;
		public DateChanged theDateChanged;
		public  CalendarDayEventViewController (CalendarDayTimelineView timeline)
		{
			
			DayView = timeline;
			DayView.reloadDay();
			//LoadButtons();
			Console.WriteLine("Checking if eventlist is null");
			
			
			DayView.OnEventClicked += (theEvent) => {
				Console.WriteLine(theEvent.Title + " Has been Clicked");
				if(OnEventClicked != null)
				{
					this.OnEventClicked(theEvent);	
				}
			};
			DayView.OnEventDoubleClicked += (theEvent) => {
				Console.WriteLine(theEvent.Title + " Has been Double Clicked");	
				
				if(OnEventDoubleClicked != null)
				{
					this.OnEventDoubleClicked(theEvent);	
				}
			};
			DayView.dateChanged += (theDate) =>
			{
				if (theDateChanged != null)
				{
					theDateChanged(theDate);
				}
			};
			this.View.AddSubview(DayView);
		}
		
		
		void HandlePreviousDayTouch (object sender, EventArgs e)
        {
			
			
        }
        void HandleNextDayTouch (object sender, EventArgs e)
        {
			if (AddNewEvent != null)
				AddNewEvent();	
			
        }
		/*
		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{			
			base.DidRotate (fromInterfaceOrientation);
			DayView.reloadDay();
		}
		*/
			
	}
	
	public delegate void EventClicked (CalendarDayEventView theEvent);
	
	public delegate void clicked();
	
	public delegate void DateChanged(DateTime newDate);

    public class CalendarDayEventView : UIView
    {
		public EventClicked OnEventDoubleClicked;
		public EventClicked OnEventClicked;
		public object ParentView;
		
        private static float HORIZONTAL_OFFSET = 4.0f;
        private static float VERTICAL_OFFSET = 2.0f;
        private static float VERTICAL_DIFF = 50.0f;

        private static float FONT_SIZE = 12.0f;

        public int id { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
		public NSDate nsStartDate {get;set;}
		public NSDate nsEndDate {get;set;}
        public string Title { get; set; }
        public string location { get; set; }
		public UIColor color {get;set;}
        internal BlockColumn Column { get; set; }
		bool multipleTouches = false;
		bool twiFingerTapIsPossible = true;
		public string eventIdentifier {get;set;}
		public EKCalendar theCal{get;set;}
		
		
		public CalendarDayEventView(EKEvent theEvent)
		{
			if (theEvent != null)
			{
				eventIdentifier = theEvent.EventIdentifier;
				theCal = theEvent.Calendar;
				nsStartDate = theEvent.StartDate;
				nsEndDate = theEvent.EndDate;
				startDate = Util.NSDateToDateTime(theEvent.StartDate);
				endDate = Util.NSDateToDateTime(theEvent.EndDate);
				var dateDif = endDate - startDate;
				if (dateDif.Minutes < 30 && dateDif.Minutes > 1)
				{
					endDate = endDate.AddMinutes(30 - dateDif.Minutes);
				}
				Title = theEvent.Title;
				location = theEvent.Location;
				if (theEvent.Calendar != null)
				{
					color = new UIColor(theEvent.Calendar.CGColor);
				}
			}
            this.Frame = new RectangleF(0, 0, 0, 0);
            setupCustomInitialisation();
		}
		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			if (evt.TouchesForView(this).Count > 1)
			    multipleTouches = true;
			if (evt.TouchesForView(this).Count > 2)
				twiFingerTapIsPossible = true;
			base.TouchesBegan (touches, evt);
		}
		
		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			 bool allTouchesEnded = (touches.Count == evt.TouchesForView(this).Count);
		    
		    // first check for plain single/double tap, which is only possible if we haven't seen multiple touches
		    if (!multipleTouches) {
		        UITouch touch = (UITouch)touches.AnyObject;
		       // tapLocation = touch.LocationInView(this);
		        var myParentView =  this.ParentView as CalendarDayTimelineView;
		        if (touch.TapCount == 1) {
					if (myParentView != null)
					{
						if (myParentView.OnEventClicked != null)
							myParentView.OnEventClicked(this);
					}
					else if (this.OnEventClicked != null)
						this.OnEventClicked(this);
		        } else if(touch.TapCount == 2) {
					if (myParentView != null)
					{
						if (myParentView.OnEventDoubleClicked != null)
							myParentView.OnEventDoubleClicked(this);
					}
					else if (this.OnEventDoubleClicked != null)
						this.OnEventDoubleClicked(this);
		        }
		    }   
			
			base.TouchesEnded (touches, evt);
		}
		
        public CalendarDayEventView()
        {
            this.Frame = new RectangleF(0, 0, 0, 0);
            setupCustomInitialisation();
        }

        public CalendarDayEventView(RectangleF frame)
        {
            this.Frame = frame;
            setupCustomInitialisation();
        }

        public DateTime BoxStart
        {
            get
            {
				return startDate;
				/*
                if (startDate.Minute >= 30)
                    return new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, 30, 0);
                else
                    return new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, 0, 0);
                    */
            }
        }

        public bool OverlapsWith(CalendarDayEventView e)
        {
            return (this.BoxStart < e.BoxEnd && this.BoxEnd > e.startDate);
        }

        public DateTime BoxEnd
        {
            get
            {
				return endDate;
				/*
                if (endDate.Minute > 30)
                {
                    DateTime hourPlus = endDate.AddHours(1);
                    return new DateTime(hourPlus.Year, hourPlus.Month, hourPlus.Day, hourPlus.Hour, 0, 0);
                }
                else if (endDate.Minute > 0)
                {
                    return new DateTime(endDate.Year, endDate.Month, endDate.Day, endDate.Hour, 30, 0);
                }
                else
                {
                    return new DateTime(endDate.Year, endDate.Month, endDate.Day, endDate.Hour, 0, 0);
                }
                */
            }
        }


        public void setupCustomInitialisation()
        {
            // Initialization code
            //this.id = nil;
            //this.startDate = nil;
            //this.endDate = nil;
            //this.title = nil;
            //this.location = nil;

            //twoFingerTapIsPossible = false;

            this.BackgroundColor = color;
            this.Alpha = 0.8f;
            CALayer layer = this.Layer;
            layer.MasksToBounds = true;
            layer.CornerRadius = 5.0f;
            // You can even add a border
            layer.BorderWidth = 0.5f;
            layer.BorderColor = UIColor.LightGray.CGColor;
        }

        public override void Draw(RectangleF rect)
        {
            // Retrieve the graphics context 
            var context = UIGraphics.GetCurrentContext();
            //CGContextRef context = new UIGraphicsGetCurrentContext();

            // Save the context state 
            context.SaveState();


            // Set shadow
            context.SetShadowWithColor(new SizeF(0.0f, 1.0f), 0.7f, UIColor.Black.CGColor);

            // Set text color
            UIColor.White.SetColor();
			
            RectangleF titleRect = new RectangleF(this.Bounds.X + HORIZONTAL_OFFSET,
                                                  this.Bounds.Y + VERTICAL_OFFSET,
                                                  this.Bounds.Width - 2*HORIZONTAL_OFFSET,
                                                  FONT_SIZE + 4.0f);

            RectangleF locationRect = new RectangleF(this.Bounds.X + HORIZONTAL_OFFSET,
                                                     this.Bounds.Y + VERTICAL_OFFSET + FONT_SIZE + 4.0f,
                                                     this.Bounds.Width - 2*HORIZONTAL_OFFSET,
                                                     FONT_SIZE + 4.0f);

            // Drawing code
            if (this.Bounds.Height > VERTICAL_DIFF)
            {
                // Draw both title and location
                if (!string.IsNullOrEmpty(this.Title))
                {
                    DrawString(Title, titleRect, UIFont.BoldSystemFontOfSize(FONT_SIZE), UILineBreakMode.TailTruncation,
                               UITextAlignment.Left);
                }
                if (!string.IsNullOrEmpty(location))
                {
                    DrawString(location, locationRect, UIFont.SystemFontOfSize(FONT_SIZE),
                               UILineBreakMode.TailTruncation, UITextAlignment.Left);
                }
            }
            else
            {
                // Draw only title
                if (!string.IsNullOrEmpty(Title))
                {
                    DrawString(Title, titleRect, UIFont.BoldSystemFontOfSize(FONT_SIZE), UILineBreakMode.TailTruncation,
                               UITextAlignment.Left);
                }
            }

            // Restore the context state
            context.RestoreState();
        }
    }

    public class CalendarDayTimelineView : UIView
    {
		
        
        public static float HORIZONTAL_OFFSET = 3.0f;
        public static float VERTICAL_OFFSET = 5.0f;
        public static float TIME_WIDTH = 20.0f;
        public static float PERIOD_WIDTH = 26.0f;
		// org 50f
        private static float VERTICAL_DIFF = 45.0f;
        public static float FONT_SIZE = 14.0f;

        public static float HORIZONTAL_LINE_DIFF = 10.0f;

        public static float TIMELINE_HEIGHT = (24*VERTICAL_OFFSET) + (23*VERTICAL_DIFF);

        public static float EVENT_VERTICAL_DIFF = 0.0f;
        public static float EVENT_HORIZONTAL_DIFF = 2.0f;
        public List<CalendarDayEventView> events = new List<CalendarDayEventView>();
        public DateTime currentDate;
        private UIScrollView scrollView;
		UIView parentScrollView;
		private UIButton _leftButton;
		private UIButton _rightButton;
        private TimeLineView timelineView;
		private float NavBarHeight;
		public EventClicked OnEventClicked;
		public EventClicked OnEventDoubleClicked;
		public DateChanged dateChanged;
		public bool UseCalendar{get;set;}
		public bool EventsNeedRefresh {get;set;}
		RectangleF orgRect;
		public bool isVisible {get;set;}


// The designated initializer. Override to perform setup that is required before the view is loaded.
// Only when xibless (interface buildder)

        public CalendarDayTimelineView(RectangleF rect, float tabBarHeight)
        {
			orgRect = rect;
			this.NavBarHeight = tabBarHeight + 10;
            this.Frame = rect;
            setupCustomInitialisation();
			UseCalendar = true;
			EventsNeedRefresh = true;
			
			
			UISwipeGestureRecognizer recognizerLeft = new UISwipeGestureRecognizer(this,  new Selector("swipeLeft"));
			recognizerLeft.Direction = UISwipeGestureRecognizerDirection.Left;
			this.AddGestureRecognizer(recognizerLeft);
			
			UISwipeGestureRecognizer recognizerRight = new UISwipeGestureRecognizer(this,  new Selector("swipeRight"));
			recognizerRight.Direction = UISwipeGestureRecognizerDirection.Right;
			this.AddGestureRecognizer(recognizerRight);
        }
		
		
		
      [Export("swipeLeft")]
		public void swipeLeft(UISwipeGestureRecognizer sender)
		{
			this.currentDate = currentDate.AddDays(-1);
			reloadDay();
			dateChanged(currentDate);
		}
		
		[Export("swipeRight")]
		public void swipeRight(UISwipeGestureRecognizer sender)
		{
			this.currentDate = currentDate.AddDays(1);
			reloadDay();
			dateChanged(currentDate);
		}
		
		public CalendarDayTimelineView()
        {
			NavBarHeight = 0;
            this.Frame = new RectangleF(0, 0, 320, 480);
			orgRect = this.Frame;
            setupCustomInitialisation();
        }


        public void setupCustomInitialisation()
        {
			foreach (UIView v in Subviews)
			{
				v.RemoveFromSuperview();	
			}
            // Initialization code
           // events = new List<CalendarDayEventView>();
            // Add main scroll view
            this.AddSubview(getScrollView());
            // Add timeline view inside scrollview
			LoadButtons();
			//Frame = new RectangleF(this.Bounds.X ,this.Bounds.Y + 44 ,CurrentWidth,CurrentHeight - 44);
			
            scrollView.AddSubview(getTimeLineView());

					/*
            var imgRect = _shadow.Frame;
            imgRect.Y = rect.Size.Height - 132;
            _shadow.Frame = imgRect;
            */
        }
		
		public override void Draw (RectangleF rect)
		{
			Util.FromResource(null,"topbar.png").Draw(new PointF(0,0));
           // DrawDayLabels(rect);
            DrawMonthLabel(rect);
		//	base.Draw (rect);
		}
        private void DrawMonthLabel(RectangleF rect)
        {
            var r = new RectangleF(new PointF(0, 5), new SizeF {Width = CurrentWidth, Height = 35});
			UIColor.DarkGray.SetColor();
			var dateString = this.currentDate.DayOfWeek + " " + this.currentDate.ToShortDateString();
            DrawString(dateString, 
                r, UIFont.BoldSystemFontOfSize(20),
                UILineBreakMode.WordWrap, UITextAlignment.Center);
        }

        private void LoadButtons()
        {
            _leftButton = UIButton.FromType(UIButtonType.Custom);
            _leftButton.TouchUpInside += delegate{this.currentDate = currentDate.AddDays(-1);reloadDay();dateChanged(currentDate);};
            _leftButton.SetImage(Util.FromResource(null,"leftarrow.png"), UIControlState.Normal);
            AddSubview(_leftButton);
            _leftButton.Frame = new RectangleF(10, 0, 35, 35);

            _rightButton = UIButton.FromType(UIButtonType.Custom);
            _rightButton.TouchUpInside += delegate{this.currentDate = currentDate.AddDays(1);reloadDay();dateChanged(currentDate);};
            _rightButton.SetImage(Util.FromResource(null,"rightarrow.png"), UIControlState.Normal);
            AddSubview(_rightButton);
            _rightButton.Frame = new RectangleF(CurrentWidth - 56, 0, 35, 35);
        }       
		private UIView getScrollView()
        {
				parentScrollView = new UIView(new RectangleF(this.Bounds.X ,this.Bounds.Y + 35 ,CurrentWidth,CurrentHeight - 35));
                scrollView = new UIScrollView(parentScrollView.Bounds);
                scrollView.ContentSize = new SizeF(this.CurrentWidth, TIMELINE_HEIGHT);
                scrollView.ScrollEnabled = true;
				scrollView.MaximumZoomScale = 100f;
				scrollView.MinimumZoomScale = .01f;
                scrollView.BackgroundColor = UIColor.White;
                scrollView.AlwaysBounceVertical = true;
				parentScrollView.Add(scrollView);
            	
            return  parentScrollView;
        }




        private TimeLineView getTimeLineView()
        {
                timelineView = new TimeLineView(new RectangleF(this.Bounds.X,
                                                               this.Bounds.Y,
                                                               this.CurrentWidth,
                                                               TIMELINE_HEIGHT));
                timelineView.BackgroundColor = UIColor.White;
            

            return timelineView;
        }

        public override void MovedToWindow()
        {
            if (Window != null)
            {
				
				//setupCustomInitialisation();
                this.reloadDay();
            }
        }
		

        public void ScrollToTime(DateTime time)
        {
            scrollView.ScrollRectToVisible(new RectangleF(0, GetStartPosition(DateTime.Now), 300, this.CurrentHeight),
                                           false);
        }

        private float GetStartPosition(DateTime time)
        {
            Int32 hourStart = time.Hour;
            float hourStartPosition =
                (float) Math.Round((hourStart*VERTICAL_DIFF) + VERTICAL_OFFSET + ((FONT_SIZE + 4.0f)/2.0f));
            // Get the minute start position
            // Round minute to each 5
            Int32 minuteStart = time.Minute;
           // minuteStart = Convert.ToInt32(Math.Round(minuteStart/5.0f)*5);
			Double minDif = (Convert.ToDouble(minuteStart) / 60);
            float minuteStartPosition = (float)( minDif * VERTICAL_DIFF);

            return (hourStartPosition + minuteStartPosition + EVENT_VERTICAL_DIFF);
        }
		private  float CurrentWidth
		{
			get 
			 {
				switch (UIDevice.CurrentDevice.Orientation){

				case  UIDeviceOrientation.Portrait:
					 return orgRect.Size.Width;
					break;

				case UIDeviceOrientation.LandscapeLeft:
					return orgRect.Size.Height;

					break;
				case UIDeviceOrientation.LandscapeRight:
					return orgRect.Size.Height;
					break;
					}
					 return orgRect.Size.Width;
			}
			
		}
		private  float CurrentHeight
		{
			get 
			 {
				switch (UIDevice.CurrentDevice.Orientation){

				case  UIDeviceOrientation.Portrait:
					 return orgRect.Size.Height - NavBarHeight;
					break;

				case UIDeviceOrientation.LandscapeLeft:
					return orgRect.Size.Width - NavBarHeight;

					break;
				case UIDeviceOrientation.LandscapeRight:
					return orgRect.Size.Width - NavBarHeight;
					break;
					}
					 return orgRect.Size.Height - NavBarHeight;
			}
			
		}
		
		/// <summary>
		/// Returns an Array of events from the calendar for the current date. 
		/// </summary>
		/// <returns>
		/// A <see cref="EKEvent[]"/>
		/// </returns>
		EKEvent[] fetchEvents()
		{
			
    
			Console.WriteLine("Start Date");
		    var startDate = this.currentDate;
			
		    // endDate is 1 day = 60*60*24 seconds = 86400 seconds from startDate		   
			DateTime endDate = this.currentDate.AddSeconds(86400);
		    Console.WriteLine("Setting up the store");
		    // Create the predicate. Pass it the default calendar.
			Console.WriteLine("Getting Calendars");
			EKEventStore store = new EKEventStore();
			var calendarArray = store.Calendars;
			Console.Write("Predicate");
			//Convert to NSDate
			NSDate nstartDate = Util.DateTimeToNSDate(startDate);
			NSDate nendDate = Util.DateTimeToNSDate(endDate);
		   	NSPredicate predicate = store.PredicateForEvents(nstartDate,nendDate,calendarArray);
		    Console.WriteLine("Fetching Events");
		    // Fetch all events that match the predicate.
			var eventsArray = store.EventsMatching(predicate);
		 	Console.WriteLine("Returning results");
			if(eventsArray  == null)
			{
				eventsArray = new List<EKEvent>().ToArray();	
			}
		    return eventsArray;
		}
		
        public void reloadDay()
        {
			if (isVisible)
			{
				setupCustomInitialisation();
	            // If no current day was given
	            // Make it today
	            if (currentDate <= Util.DateTimeMin )
	            {
	                // Dont' want to inform the observer
	                currentDate = DateTime.Today;
	            }
				SetNeedsDisplay();
				if (UseCalendar)
				{
					if (EventsNeedRefresh)
					{
						events.Clear();
						foreach(EKEvent theEvent in  fetchEvents())
						{
							this.events.Add(new CalendarDayEventView(theEvent));	
						}
						EventsNeedRefresh = false;
					}
				}
				scrollView.Frame = new RectangleF(this.Bounds.X,this.Bounds.Y,CurrentWidth,CurrentHeight);
				scrollView.ContentSize = new SizeF(this.CurrentWidth, TIMELINE_HEIGHT);
	            // Remove all previous view event
	            foreach (var view in this.scrollView.Subviews)
	            {
	                if (view is TimeLineView)
	                {
						timelineView.Frame = new RectangleF(this.Bounds.X,
	                                                               this.Bounds.Y,
	                                                               this.CurrentWidth,
	                                                               TIMELINE_HEIGHT);
						timelineView.CurrentWidth = this.CurrentWidth;
	                }
	                else
	                {
	                    view.RemoveFromSuperview();
	                }
	            }
	
	            // Ask the delgate about the events that correspond
	            // the the currently displayed day view
	            if (events != null)
	            {
	               // _events = events.Where(x => x.startDate.Date == currentDate.Date || x.endDate.Date == currentDate.Date).OrderBy(x => x.startDate).ThenByDescending(x => x.endDate).ToList();
	
	                List<Block> blocks = new List<Block>();
	                Block lastBlock = new Block();
					// Removed the thenByDecending. Caused a crash on the device. This is needed for 2 events with the same start time though....
	               // foreach (CalendarDayEventView e in events.Where(x => x.startDate.Date == currentDate.Date || x.endDate.Date == currentDate.Date).OrderBy(x => x.startDate))//.ThenByDescending(x => x.endDate).ToList())
					foreach (CalendarDayEventView e in events.OrderBy(x => x.startDate))//.ThenByDescending(x => x.endDate).ToList())
	                {
	                    // if there is no block, create the first one
	                    if (blocks.Count == 0)
	                    {
	                        lastBlock = new Block();
	                        blocks.Add(lastBlock);
	                    }
	                        // or if the event doesn't overlap with the last block, create a new block
	                    else if (!lastBlock.OverlapsWith(e))
	                    {
	                        lastBlock = new Block();
	                        blocks.Add(lastBlock);
	                    }
	
	                    // any case, add it to some block
	                    lastBlock.Add(e);
	                }
	                foreach (Block theBlock in blocks)
	                {
	                    //theBlock.ArrangeColumns();
	                    foreach (CalendarDayEventView theEvent in theBlock.events)
	                    {
							theEvent.ParentView = this;
							
	                        // Making sure delgate sending date that match current day
	                        if (theEvent.startDate.Date == currentDate)
	                        {
	                            // Get the hour start position
	                            Int32 hourStart = theEvent.startDate.Hour;
	                            float hourStartPosition = (float) Math.Round((hourStart*VERTICAL_DIFF) + VERTICAL_OFFSET + ((FONT_SIZE + 4.0f)/2.0f));
	                            // Get the minute start position
	                            Int32 minuteStart = theEvent.startDate.Minute;
								float minuteStartPosition = (float) ((Convert.ToDouble(minuteStart) / 60) * VERTICAL_DIFF);
	
	
	                            // Get the hour end position
	                            Int32 hourEnd = theEvent.endDate.Hour;
	                            if (theEvent.startDate.Date != theEvent.endDate.Date)
	                            {
	                                hourEnd = 23;
	                            }
	                            float hourEndPosition =
	                                (float)
	                                Math.Round((hourEnd*VERTICAL_DIFF) + VERTICAL_OFFSET + ((FONT_SIZE + 4.0f)/2.0f));
	                            // Get the minute end position
	                            // Round minute to each 5
	                            Int32 minuteEnd = theEvent.endDate.Minute;
	                            if (theEvent.startDate.Date != theEvent.endDate.Date)
	                            {
	                                minuteEnd = 55;
	                            }
	                           // minuteEnd = Convert.ToInt32(Math.Round(minuteEnd/5.0)*5);
	                           // float minuteEndPosition = (float) Math.Round((minuteEnd < 30) ? 0 : VERTICAL_DIFF/2.0f);
	           					 float minuteEndPosition = (float) ((Convert.ToDouble(minuteEnd) / 60) * VERTICAL_DIFF);
	
	                            float eventHeight = 0.0f;
	
								// Calculate the event Height.
	                            eventHeight = (hourEndPosition + minuteEndPosition) - hourStartPosition -
	                                               minuteStartPosition;
								// Set the min Height to 30 min
								/*
								if (eventHeight <  (VERTICAL_DIFF/2) - (2*EVENT_VERTICAL_DIFF))
								{
									eventHeight = (VERTICAL_DIFF/2) - (2*EVENT_VERTICAL_DIFF);	
								}
								*/
	
	                            var availableWidth = this.CurrentWidth -
	                                                 (HORIZONTAL_OFFSET + TIME_WIDTH + PERIOD_WIDTH + HORIZONTAL_LINE_DIFF) -
	                                                 HORIZONTAL_LINE_DIFF - EVENT_HORIZONTAL_DIFF;
	                            var currentWidth = availableWidth/theBlock.Columns.Count;
	                            var currentInt = theEvent.Column.Number;
	                            var x = HORIZONTAL_OFFSET + TIME_WIDTH + PERIOD_WIDTH + HORIZONTAL_LINE_DIFF +
	                                    EVENT_HORIZONTAL_DIFF + (currentWidth*currentInt);
	                            var y = hourStartPosition + minuteStartPosition + EVENT_VERTICAL_DIFF;
	                            RectangleF eventFrame = new RectangleF(x,
	                                                                   y, currentWidth,
	                                                                   eventHeight);
	
	                            theEvent.Frame = eventFrame;
	                            //event.delegate = self;
	                            theEvent.SetNeedsDisplay();
	                            this.timelineView.AddSubview(theEvent);
	
	
	                            // Log the extracted date values
	                            Console.WriteLine("hourStart: {0} minuteStart: {1}", hourStart, minuteStart);
	                        }
	                    }
	                }
	            }
				ScrollToTime(DateTime.Now);
			}
        }


        internal class TimeLineView : UIView
        {
            private string[] _times;
            private string[] _periods;
// The designated initializer. Override to perform setup that is required before the view is loaded.
// Only when xibless (interface buildder)
            public TimeLineView(RectangleF rect)
            {
                this.Frame = rect;
                setupCustomInitialisation();
            }


            public void setupCustomInitialisation()
            {
                // Initialization code
            }

			
		public  float CurrentWidth {get;set;}

// Setup array consisting of string
// representing time aka 12 (12 am), 1 (1 am) ... 25 x

            public string[] times
            {
                get
                {
                    if (_times == null)
                    {
                        _times = new string[]
                                     {
                                         "12",
                                         "1",
                                         "2",
                                         "3",
                                         "4",
                                         "5",
                                         "6",
                                         "7",
                                         "8",
                                         "9",
                                         "10",
                                         "11",
                                         "Noon",
                                         "1",
                                         "2",
                                         "3",
                                         "4",
                                         "5",
                                         "6",
                                         "7",
                                         "8",
                                         "9",
                                         "10",
                                         "11",
                                         "12",
                                         ""
                                     };
                    }
                    return _times;
                }
            }

            // Setup array consisting of string
            // representing time periods aka AM or PM
            // Matching the array of times 25 x

            public string[] periods
            {
                get
                {
                    if (_periods == null)
                    {
                        _periods = new string[]
                                       {
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "AM",
                                           ""
                                       };
                    }
                    return _periods;
                }
            }

            public override void Draw(RectangleF rect)
            {
                // Drawing code
                // Here Draw timeline from 12 am to noon to 12 am next day
                // Times appearance

                UIFont timeFont = UIFont.BoldSystemFontOfSize(FONT_SIZE);
                UIColor timeColor = UIColor.Black;

                // Periods appearance
                UIFont periodFont = UIFont.SystemFontOfSize(FONT_SIZE);
                UIColor periodColor = UIColor.Gray;

                // Draw each times string
                for (Int32 i = 0; i < this.times.Length; i++)
                {
                    // Draw time
                    timeColor.SetColor();
                    string time = this.times[i];

                    RectangleF timeRect = new RectangleF(HORIZONTAL_OFFSET, VERTICAL_OFFSET + i*VERTICAL_DIFF,
                                                         TIME_WIDTH, FONT_SIZE + 4.0f);

                    // Find noon
                    if (i == 24/2)
                    {
                        timeRect = new RectangleF(HORIZONTAL_OFFSET, VERTICAL_OFFSET + i*VERTICAL_DIFF,
                                                  TIME_WIDTH + PERIOD_WIDTH, FONT_SIZE + 4.0f);
                    }

                    DrawString(time, timeRect, timeFont, UILineBreakMode.WordWrap, UITextAlignment.Right);


                    // Draw period
                    // Only if it is not noon
                    if (i != 24/2)
                    {
                        periodColor.SetColor();

                        string period = this.periods[i];
                        DrawString(period,
                                   new RectangleF(HORIZONTAL_OFFSET + TIME_WIDTH, VERTICAL_OFFSET + i*VERTICAL_DIFF,
                                                  PERIOD_WIDTH, FONT_SIZE + 4.0f), periodFont, UILineBreakMode.WordWrap,
                                   UITextAlignment.Right);
					}


                    var context = UIGraphics.GetCurrentContext();

                    // Save the context state 
                    context.SaveState();
                    context.SetStrokeColorWithColor(UIColor.LightGray.CGColor);

                    // Draw line with a black stroke color
                    // Draw line with a 1.0 stroke width
                    context.SetLineWidth(0.5f);
                    // Translate context for clear line
                    context.TranslateCTM(-0.5f, -0.5f);
                    context.BeginPath();
                    context.MoveTo(HORIZONTAL_OFFSET + TIME_WIDTH + PERIOD_WIDTH + HORIZONTAL_LINE_DIFF,
                                   VERTICAL_OFFSET + i*VERTICAL_DIFF + (float) Math.Round(((FONT_SIZE + 4.0)/2.0)));
                    context.AddLineToPoint(CurrentWidth,
                                           VERTICAL_OFFSET + i*VERTICAL_DIFF +
                                           (float) Math.Round((FONT_SIZE + 4.0)/2.0));
                    context.StrokePath();

                    if (i != this.times.Length - 1)
                    {
                        context.BeginPath();
                        context.MoveTo(HORIZONTAL_OFFSET + TIME_WIDTH + PERIOD_WIDTH + HORIZONTAL_LINE_DIFF,
                                       VERTICAL_OFFSET + i*VERTICAL_DIFF +
                                       (float) Math.Round(((FONT_SIZE + 4.0f)/2.0f)) +
                                       (float) Math.Round((VERTICAL_DIFF/2.0f)));
                        context.AddLineToPoint(CurrentWidth,
                                               VERTICAL_OFFSET + i*VERTICAL_DIFF +
                                               (float) Math.Round(((FONT_SIZE + 4.0f)/2.0f)) +
                                               (float) Math.Round((VERTICAL_DIFF/2.0f)));
                        float[] dash1 = {4.0f, 3.0f};
                        context.SetLineDash(0.0f, dash1, 2);
                        context.StrokePath();
                    }

                    // Restore the context state
                    context.RestoreState();
				}
                
            }

            ///////
        }
    }
	
	
	public class ScrollViewWithHeader : UIView 
	{
		UIScrollView _header;
		UIScrollView _rowHeader;
		UIScrollView _mainContent;
		UIView _content;
		UIView _headerContent;
		UIView _rowHeaderContent;
		RectangleF _headerFrame;
		RectangleF _rHeaderFrame;
		
		MyScrollViewDelegate _headerDelegate;
		MyScrollViewDelegate _rHeaderDelegate;
		MyScrollViewDelegate _mainContentDelegate;
		
		bool isZooming;
		
		
		public ScrollViewWithHeader(RectangleF rect,UIView header,UIView content,bool enableZoom) : base (rect)
		{
			SetupController(rect,header,null,content,enableZoom);
			
		}
		public bool isMoving()
		{
			if (_mainContent.Zooming)
				return true;
			return false;
		}
		
		public ScrollViewWithHeader(RectangleF rect,UIView header,UIView rowHeader,UIView content, bool enableZoom) : base (rect)
		{
			SetupController(rect,header,rowHeader,content,enableZoom);
			
		}
		
		public void SetupController(RectangleF rect,UIView header,UIView rowHeader,UIView content, bool enableZoom)
		{
			_content = content;
			_headerContent = header;
			_rowHeaderContent = rowHeader;
			
			_headerDelegate = new MyScrollViewDelegate();
			_headerDelegate.theView = _headerContent;
			_rHeaderDelegate = new MyScrollViewDelegate();
			if (rowHeader != null)
				_rHeaderDelegate.theView = _rowHeaderContent;
			_mainContentDelegate = new MyScrollViewDelegate();
			_mainContentDelegate.theView = _content;
			
			float minZoom = .4f;
			float maxZoom = 1.3f;
			
			SizeF hSize = header.Frame.Size;
			SizeF cSize = content.Frame.Size;
			SizeF rSize;
			if (rowHeader != null)
				rSize = rowHeader.Frame.Size;
			else 
				rSize = new SizeF(0,0);
			//Set the content width to match the top header width
			if (hSize.Width > cSize.Width)
				cSize.Width = hSize.Width;
			else 
				hSize.Width = cSize.Width;
			// Set the content height to match the
			if (rSize.Height > cSize.Height)
				cSize.Height = rSize.Height;
			else 
				rSize.Height = cSize.Height;
			// Create the viewable size based off of the current frame;
			RectangleF hRect = new RectangleF(rSize.Width,0,rect.Width - rSize.Width,hSize.Height);			
			RectangleF cRect = new RectangleF(rSize.Width,hSize.Height,rect.Width - rSize.Width,rect.Height - hSize.Height);
			RectangleF rRect = new RectangleF(0,hSize.Height,rSize.Width,rect.Height - hSize.Height);
			_headerFrame = hRect; 
			_rHeaderFrame = rRect;
			
			_header = new UIScrollView(hRect);
			_header.ContentSize = hSize;
			_header.Bounces = false;			
			// Hide scroll bars on the headers
			_header.ShowsVerticalScrollIndicator = false;
			_header.ShowsHorizontalScrollIndicator = false;
			if (enableZoom)
			{
				// Sets the zoom level
				_header.MaximumZoomScale = maxZoom;
				_header.MinimumZoomScale = minZoom;
				// create a delegate to return the zoom image.
				//_header.ViewForZoomingInScrollView += delegate {return _headerContent;};
				

				
			}
			
			_headerDelegate.Scrolling += delegate
			{
				if (!_mainContent.Zooming && !isZooming)
				{
					scrollContent();
					scrollHeader();	
				}
			};
			_header.Delegate = _headerDelegate;
			_header.AddSubview(header);
			
			
			
			
			_mainContent = new UIScrollView(cRect);
			_mainContent.ContentSize = cSize; 
			_mainContent.AddSubview(content);
			_mainContent.Bounces = false;
			if (enableZoom)
			{
				_mainContent.MaximumZoomScale = maxZoom;
				_mainContent.MinimumZoomScale = minZoom;	
				_mainContent.BouncesZoom = false;
				// create a delegate to return the zoom image.
				//_mainContent.ViewForZoomingInScrollView += delegate {return _content;};
				
				_mainContentDelegate.ZoomStarted += delegate {
					//Tell the class you are zooming
					isZooming = true;
					ZoomHeader();	
				};
				_mainContentDelegate.ZoomEnded += delegate {
					ZoomHeader();	
					isZooming = false;
					// Rescroll the content to make sure it lines up with the header
					
					//scrollContent();
				};
				
			}
			
			_mainContentDelegate.Scrolling += delegate {
				scrollHeader();	
				ZoomHeader();
				//Rescroll the content to make sure it lines up with the header
				if  (!_mainContent.Zooming && !isZooming)
					scrollContent();
			};
			_mainContent.Delegate = _mainContentDelegate;
			
			
			
			_rowHeader = new UIScrollView(rRect);
			_rowHeader.ContentSize = rSize;
			_rowHeader.Bounces = false;
			if (enableZoom)
			{
				_rowHeader.MaximumZoomScale = maxZoom;
				_rowHeader.MinimumZoomScale = minZoom;
				//if (rowHeader != null)
					//_rowHeader.ViewForZoomingInScrollView += delegate {return _rowHeaderContent;};
			}
			// Hide scroll bars on the headers
			_rowHeader.ShowsVerticalScrollIndicator = false;
			_rowHeader.ShowsHorizontalScrollIndicator = false;
			if (rowHeader != null)
				_rowHeader.AddSubview(rowHeader);
			
			_rHeaderDelegate.Scrolling+= delegate{
				if (!_mainContent.Zooming && !isZooming)
					scrollContent();	
			};
			_rowHeader.Delegate = _rHeaderDelegate;
			
			this.AddSubview(_header);
			this.AddSubview(_rowHeader);
			this.AddSubview(_mainContent);
		}
		
		// Sets the content scroll to match the headers
		private void scrollContent()
		{
			
			var hOffSet = _header.ContentOffset;
			var cOffSet = _mainContent.ContentOffset;
			PointF rOffSet = new PointF(0,cOffSet.Y);
			if (_rowHeader != null)
				rOffSet = _rowHeader.ContentOffset;
			
			if (cOffSet.X  != hOffSet.X || rOffSet.Y != cOffSet.Y)
			{
				var cFrame = _mainContent.Frame;
				cFrame.X = hOffSet.X;
				cFrame.Y = rOffSet.Y;
				_mainContent.ScrollRectToVisible(cFrame,false);	
			}
			
		}
		// Lines the headers up with the content
		private void scrollHeader()
		{	
			var hOffSet = _header.ContentOffset;
			var cOffSet = _mainContent.ContentOffset;
			PointF rOffSet = new PointF(0,cOffSet.Y);
			if (_rowHeader != null)
				rOffSet = _rowHeader.ContentOffset;
			
			if (cOffSet.X  != hOffSet.X)
			{
				var hFrame = _header.Frame;
				hFrame.X = cOffSet.X;
				hFrame.Y = hOffSet.Y;
				_header.ScrollRectToVisible(hFrame,false);	
			}
			if (rOffSet.Y != cOffSet.Y)
			{
				var rFrame = _rowHeader.Frame;
				rFrame.X = rFrame.X;
				rFrame.Y = cOffSet.Y;
				_rowHeader.ScrollRectToVisible(rFrame,false);	
			}
			
		}
		// Sets the zoom level of the headers so they match the content
		private void ZoomHeader()
		{
				var scale = _mainContent.ZoomScale;
			if (scale != _header.ZoomScale)
			{
				var headerFrame = _header.Frame;
				headerFrame.Height =  _headerFrame.Height * scale;
				
				var rHeaderFrame = _rowHeader.Frame;
				rHeaderFrame.Width = _rHeaderFrame.Width * scale;
				
				// Resize the frame to match the correct height
				headerFrame.X = rHeaderFrame.Width;
				headerFrame.Width = this.Frame.Width - rHeaderFrame.Width;
				_header.Frame = headerFrame;
				_header.SetZoomScale(scale,false);
				
				// resize the frame to match the corect width
				rHeaderFrame.Y = headerFrame.Height;
				rHeaderFrame.Height = this.Frame.Height - headerFrame.Height;
				_rowHeader.Frame = rHeaderFrame;
				_rowHeader.SetZoomScale(scale,false);
				
				// resize the content to take the left over area
				var mainFrame =  _mainContent.Frame;
				mainFrame.Height =  rHeaderFrame.Height;
				mainFrame.Width = headerFrame.Width;
				mainFrame.X = rHeaderFrame.Width;
				mainFrame.Y = headerFrame.Height;
				
				_mainContent.Frame = mainFrame;
				scrollHeader();
			
			}
			else
			{
				Console.WriteLine("skipped zooming");
			}
		}
		
		partial class MyScrollViewDelegate : UIScrollViewDelegate
		{
			public UIView theView {get;set;}
			public NSAction Scrolling {get;set;}
			public NSAction ZoomStarted {get;set;}
			public NSAction ZoomEnded {get;set;}
			
			public override void Scrolled (UIScrollView scrollView)
			{
				if (Scrolling != null)
				{
					Scrolling();	
				}
			}
			
			public override void ZoomingStarted (UIScrollView scrollView, UIView view)
			{
				if (ZoomStarted != null)
					ZoomStarted();
			}
			public override void ZoomingEnded (UIScrollView scrollView, UIView withView, float atScale)
			{
				if (ZoomEnded != null)
					ZoomEnded();
			}
			public override UIView ViewForZoomingInScrollView (UIScrollView scrollView)
			{
				return theView;
			}
			
		}
	}
	
	public class TrueWeekViewController :UIViewController
	{
		public DateTime CurrentDate{get;internal set;}
		public DateTime FirstDayOfWeek {get;set;}
		private TrueWeekView weekView;
		public TrueWeekViewController(DateTime date)
		{
			weekView = new TrueWeekView(date){Frame = new RectangleF(0,0,480,220)};
			SetCurrentDate(date);
			this.View.AddSubview(weekView);
		}
		public void SetCurrentDate(DateTime date)
		{
			CurrentDate = date;	
			FirstDayOfWeek = date.AddDays(-1 * (int)date.DayOfWeek);
			weekView.SetDayOfWeek(date);
		}
		public void ReDraw()
		{
			weekView.SetupWindow();
		}
	}
	
	public class TrueWeekView : UIView
	{
        public static float TIME_WIDTH = 15.0f;
        public static float PERIOD_WIDTH = 20.0f;
       	public static float TIMELINE_HEIGHT = (24*VERTICAL_OFFSET) + (23*VERTICAL_DIFF);
		public static float HORIZONTAL_OFFSET = 3.0f;
        public static float VERTICAL_OFFSET = 3f;

        private static float VERTICAL_DIFF = 50.0f;
        public static float FONT_SIZE = 10.0f;

        public static float HORIZONTAL_LINE_DIFF = 10.0f;
		public static float DayWidth = ((480)/3);
		public static float TotalWidth =  (7 * DayWidth) + 1;
		public bool isVisible {get;set;}
		
		public EventClicked OnEventClicked;
		public DateTime CurrentDate{get;internal set;}
		public DateTime FirstDayOfWeek{get;internal set;}
		public EventClicked ClickedEvent{get;set;}
		private WeekTopView header;
		private TimeView rowHeader;
		private GridLineView gridView;
		private List<CalendarDayEventView> events;
		private List<Block> blocks;
		private ScrollViewWithHeader myScrollView{get;set;}
		public bool UseCalendar{get;set;}
		public bool EventsNeedRefresh {get;set;}
		
		public TrueWeekView(DateTime date){
			SetDayOfWeek(date);
			this.BackgroundColor = UIColor.White;
		}
		public void SetDayOfWeek(DateTime date)
		{
			EventsNeedRefresh = true;
			CurrentDate = date;
			FirstDayOfWeek = date.AddDays(-1 * (int)date.DayOfWeek);
		}
		
		public override void MovedToWindow ()
		{
			if (isVisible)
			{
				reloadDay();
			}
		}
		
		public void SetupWindow()
		{
			foreach (UIView view in this.Subviews)
			{
				view.RemoveFromSuperview();	
			}
			
			header = new WeekTopView(this);
			rowHeader = new TimeView();
			gridView = new GridLineView(26);
			myScrollView = new ScrollViewWithHeader(this.Frame,header,rowHeader,gridView,true);
			this.AddSubview(myScrollView);
		}
		private void scrollToCurrentDay()
		{
			var x = (DayWidth * (int)CurrentDate.DayOfWeek);
		}
		
        public void reloadDay()
        {
			if (isVisible)
			{
				SetupWindow();
	            // If no current day was given
	            // Make it today
	            if (CurrentDate == Util.DateTimeMin)
	            {
	                // Dont' want to inform the observer
	                CurrentDate = DateTime.Today;
	            }
				
				if (UseCalendar)
				{
					if (EventsNeedRefresh)
					{
						events = new List<CalendarDayEventView>();
						foreach(EKEvent theEvent in  fetchEvents())
						{
							events.Add(new CalendarDayEventView(theEvent));	
						}
						
						if (events != null)
			            {
			               // _events = events.Where(x => x.startDate.Date == currentDate.Date || x.endDate.Date == currentDate.Date).OrderBy(x => x.startDate).ThenByDescending(x => x.endDate).ToList();
			
			                blocks = new List<Block>();
			                Block lastBlock = new Block();
							// Removed the thenByDecending. Caused a crash on the device. This is needed for 2 events with the same start time though....
			               // foreach (CalendarDayEventView e in events.Where(x => x.startDate.Date == currentDate.Date || x.endDate.Date == currentDate.Date).OrderBy(x => x.startDate))//.ThenByDescending(x => x.endDate).ToList())
							foreach (CalendarDayEventView e in events.OrderBy(x => x.startDate))//.ThenByDescending(x => x.endDate).ToList())
			                {
			                    // if there is no block, create the first one
			                    if (blocks.Count == 0)
			                    {
			                        lastBlock = new Block();
			                        blocks.Add(lastBlock);
			                    }
			                        // or if the event doesn't overlap with the last block, create a new block
			                    else if (!lastBlock.OverlapsWith(e))
			                    {
			                        lastBlock = new Block();
			                        blocks.Add(lastBlock);
			                    }
			
			                    // any case, add it to some block
			                    lastBlock.Add(e);
			                	}
						
							}
						
						EventsNeedRefresh = false;
						}
	            // Ask the delgate about the events that correspond
	            // the the currently displayed day view

	                foreach (Block theBlock in blocks)
	                {
						
						var dayColumn = (theBlock.BoxStart - FirstDayOfWeek).Days;
	                    //theBlock.ArrangeColumns();
	                    foreach (CalendarDayEventView theEvent in theBlock.events)
	                    {
							theEvent.ParentView = this;
							var startDate = theEvent.startDate;
	                        // Making sure delgate sending date that match current day
	                            // Get the hour start position
	                            Int32 hourStart = theEvent.startDate.Hour;
	                            float hourStartPosition = (float) Math.Round((hourStart*VERTICAL_DIFF) + VERTICAL_OFFSET + ((FONT_SIZE + 4.0f)/2.0f));
	                            // Get the minute start position
	                            Int32 minuteStart = theEvent.startDate.Minute;
								float minuteStartPosition = (float) ((Convert.ToDouble(minuteStart) / 60) * VERTICAL_DIFF);
	
	
	                            // Get the hour end position
	                            Int32 hourEnd = theEvent.endDate.Hour;
	                            if (theEvent.startDate.Date != theEvent.endDate.Date)
	                            {
	                                hourEnd = 23;
	                            }
	                            float hourEndPosition =
	                                (float)
	                                Math.Round((hourEnd*VERTICAL_DIFF) + VERTICAL_OFFSET + ((FONT_SIZE + 4.0f)/2.0f));
	                            // Get the minute end position
	                            // Round minute to each 5
	                            Int32 minuteEnd = theEvent.endDate.Minute;
	                            if (theEvent.startDate.Date != theEvent.endDate.Date)
	                            {
	                                minuteEnd = 55;
	                            }
	                           // minuteEnd = Convert.ToInt32(Math.Round(minuteEnd/5.0)*5);
	                           // float minuteEndPosition = (float) Math.Round((minuteEnd < 30) ? 0 : VERTICAL_DIFF/2.0f);
	           					 float minuteEndPosition = (float) ((Convert.ToDouble(minuteEnd) / 60) * VERTICAL_DIFF);
	
	                            float eventHeight = 0.0f;
	
								// Calculate the event Height.
	                            eventHeight = (hourEndPosition + minuteEndPosition) - hourStartPosition -
	                                               minuteStartPosition;
								// Set the min Height to 30 min
								/*
								if (eventHeight <  (VERTICAL_DIFF/2) - (2*EVENT_VERTICAL_DIFF))
								{
									eventHeight = (VERTICAL_DIFF/2) - (2*EVENT_VERTICAL_DIFF);	
								}
								*/
	
	                            var availableWidth = (TotalWidth / 7) - 2;
	                            var currentWidth = availableWidth/theBlock.Columns.Count;
	                            var currentInt = theEvent.Column.Number;
	                            var x =    ((currentWidth )*currentInt) + 2 + ((availableWidth + 2) * dayColumn);
	                            var y = hourStartPosition + minuteStartPosition ;
	                            RectangleF eventFrame = new RectangleF(x,
	                                                                   y, currentWidth,
	                                                                   eventHeight);
	
	                            theEvent.Frame = eventFrame;
								theEvent.OnEventClicked += (theDate) => {
									eventWasClicked(theDate);
								};
	                            //event.delegate = self;
	                            theEvent.SetNeedsDisplay();
	                            this.gridView.AddSubview(theEvent);
	
	
	                            // Log the extracted date values
	                            Console.WriteLine("hourStart: {0} minuteStart: {1}", hourStart, minuteStart);
	                        }
	                    
	                }
	            }
				//ScrollToTime(DateTime.Now);
			}
        }
		
		private void eventWasClicked(CalendarDayEventView theEvent)
		{
			if (!myScrollView.isMoving())
			{
				if(OnEventClicked != null)
					this.OnEventClicked(theEvent);	
			}
		}
		EKEvent[] fetchEvents()
			{

				Console.WriteLine("Start Date");
			    var startDate = this.FirstDayOfWeek;
				Console.WriteLine(startDate);
				
			    // endDate is 1 day = 60*60*24 seconds = 86400 seconds from startDate		   
				DateTime endDate = this.FirstDayOfWeek.AddDays(6).AddSeconds(86400);
			    Console.WriteLine(endDate);
			    // Create the predicate. Pass it the default calendar.
				Console.WriteLine("Getting Calendars");
				EKEventStore store = new EKEventStore();
				var calendarArray = store.Calendars;
				Console.Write("Predicate");
				//Convert to NSDate
				NSDate nstartDate = Util.DateTimeToNSDate(startDate);
				NSDate nendDate = Util.DateTimeToNSDate(endDate);
			   	NSPredicate predicate = store.PredicateForEvents(nstartDate,nendDate,calendarArray);
			    Console.WriteLine("Fetching Events");
			    // Fetch all events that match the predicate.
				var eventsArray = store.EventsMatching(predicate);
			 	Console.WriteLine("Returning results");
				if(eventsArray  == null)
				{
					eventsArray = new List<EKEvent>().ToArray();	
				}
			    return eventsArray;
			}
		
		public void ReDraw()
		{
			reloadDay();
		}
		
		private class WeekTopView :UIView
		{
			/*
			[Export ("layerClass")]
			public static Class LayerClass()
			{
				return new  Class( typeof(weekTopLevelLayer));	
			}
			*/
			public TrueWeekView parent{get;set;}
			public WeekTopView(TrueWeekView theParent)
			{
				

		        this.Opaque=true;
				
				parent = theParent;
				this.Frame = new RectangleF(0,0,TotalWidth,35);
				this.BackgroundColor = UIColor.White;
				/*
				
				weekTopLevelLayer tempTiledLayer = (weekTopLevelLayer)this.Layer;
		        tempTiledLayer.LevelsOfDetail = 5;
				tempTiledLayer.FirstDayOfWeek = parent.FirstDayOfWeek;
				tempTiledLayer.TotalWidth = TotalWidth;
				tempTiledLayer.DayWidth = DayWidth;
		        tempTiledLayer.LevelsOfDetailBias = 2;
		        */
			}
			
			public void ReDraw()
			{
				//this.Draw(this.Frame);
			}

			[Export("drawRect")]
			void DrawRect(RectangleF rect)
			{
				
			}
			[Export("drawInContext")]
			void DrawInContext(CALayer layer)				
			{
				
			}
			
			public override void Draw (RectangleF rect)
			{
				Util.FromResource(null,"topbar.png").Draw(new RectangleF(-25,0,(TotalWidth + 50) ,35));
				var context = UIGraphics.GetCurrentContext();
				
	            context.SetLineWidth(0.5f);
				for (var i = 0; i <= 8; i++)
				{
					var lineWidth =  (i*DayWidth) ;
					context.BeginPath();
					context.MoveTo( lineWidth + 1,0);
					context.AddLineToPoint(lineWidth + 1,this.Frame.Height);
					context.StrokePath();
					if (i <=7)
					{
						var theDay = parent.FirstDayOfWeek.AddDays(i);
						DrawDayLabel(new RectangleF(lineWidth,0,DayWidth,35),theDay);
					}
					
				}
			}
			
			
			private void DrawDayLabel(RectangleF rect,DateTime date)
	        {
	           // var r = new RectangleF(new PointF(0, 5), new SizeF {Width = CurrentWidth, Height = 35});
				if (date == DateTime.Today)
					UIColor.Blue.SetColor();
				else
					UIColor.DarkGray.SetColor();
				var dayRect = rect;
				dayRect.Height = dayRect.Height / 2;
				
	            DrawString(date.DayOfWeek.ToString(), 
	                dayRect, UIFont.BoldSystemFontOfSize(12),
	                UILineBreakMode.WordWrap, UITextAlignment.Center);
				
				var dateRect = dayRect;
				dateRect.Y += dayRect.Height;
				DrawString(date.ToString("M/d"), 
	                dateRect, UIFont.BoldSystemFontOfSize(12),
	                UILineBreakMode.WordWrap, UITextAlignment.Center);
				UIColor.Black.SetColor();
	        }
			
		}
        private class TimeView : UIView
        {
					
            private string[] _times;
            private string[] _periods;
            public TimeView()
            {
				this.BackgroundColor = UIColor.White;
                this.Frame = new RectangleF(0,0,TIME_WIDTH + VERTICAL_OFFSET+ PERIOD_WIDTH,(24*VERTICAL_OFFSET) + (23*VERTICAL_DIFF));
                setupCustomInitialisation();
            }


            public void setupCustomInitialisation()
            {
                // Initialization code
            }

			
			public void ReDraw()
			{
				this.Draw(this.Frame);
			}
			public  float CurrentWidth {get;set;}


            public string[] times
            {
                get
                {
                    if (_times == null)
                    {
                        _times = new string[]
                                     {
                                         "12",
                                         "1",
                                         "2",
                                         "3",
                                         "4",
                                         "5",
                                         "6",
                                         "7",
                                         "8",
                                         "9",
                                         "10",
                                         "11",
                                         "Noon",
                                         "1",
                                         "2",
                                         "3",
                                         "4",
                                         "5",
                                         "6",
                                         "7",
                                         "8",
                                         "9",
                                         "10",
                                         "11",
                                         "12",
                                         ""
                                     };
                    }
                    return _times;
                }
            }

            // Setup array consisting of string
            // representing time periods aka AM or PM
            // Matching the array of times 25 x

            public string[] periods
            {
                get
                {
                    if (_periods == null)
                    {
                        _periods = new string[]
                                       {
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "AM",
                                           ""
                                       };
                    }
                    return _periods;
                }
            }

            public override void Draw(RectangleF rect)
            {
                // Drawing code
                // Here Draw timeline from 12 am to noon to 12 am next day
                // Times appearance

                UIFont timeFont = UIFont.BoldSystemFontOfSize(FONT_SIZE);
                UIColor timeColor = UIColor.Black;

                // Periods appearance
                UIFont periodFont = UIFont.SystemFontOfSize(FONT_SIZE);
                UIColor periodColor = UIColor.Gray;

                // Draw each times string
                for (Int32 i = 0; i < this.times.Length; i++)
                {
                    // Draw time
                    timeColor.SetColor();
                    string time = this.times[i];

                    RectangleF timeRect = new RectangleF(HORIZONTAL_OFFSET, VERTICAL_OFFSET + i*VERTICAL_DIFF,
                                                         TIME_WIDTH, FONT_SIZE + 4.0f);

                    // Find noon
                    if (i == 24/2)
                    {
                        timeRect = new RectangleF(HORIZONTAL_OFFSET, VERTICAL_OFFSET + i*VERTICAL_DIFF,
                                                  TIME_WIDTH + PERIOD_WIDTH, FONT_SIZE + 4.0f);
                    }

                    DrawString(time, timeRect, timeFont, UILineBreakMode.WordWrap, UITextAlignment.Right);


                    // Draw period
                    // Only if it is not noon
                    if (i != 24/2)
                    {
                        periodColor.SetColor();

                        string period = this.periods[i];
                        DrawString(period,
                                   new RectangleF(HORIZONTAL_OFFSET + TIME_WIDTH, VERTICAL_OFFSET + i*VERTICAL_DIFF,
                                                  PERIOD_WIDTH, FONT_SIZE + 4.0f), periodFont, UILineBreakMode.WordWrap,
                                   UITextAlignment.Center);
					}

				}
                
            }

            ///////
        }
        private class GridLineView : UIView
        {
			private Int32 currentRows;
			
            public GridLineView( Int32 rows)
			{
				this.BackgroundColor = UIColor.White;
				currentRows = rows;
                this.Frame = new RectangleF(0,0,TotalWidth, (24*VERTICAL_OFFSET) + (23*VERTICAL_DIFF));
                setupCustomInitialisation();
            }
					
            public void setupCustomInitialisation()
            {
                // Initialization code
            }

			
			public  float CurrentWidth {get;set;}

            public override void Draw(RectangleF rect)
            {
                // Drawing code
                // Here Draw timeline from 12 am to noon to 12 am next day
                // Times appearance

                UIFont timeFont = UIFont.BoldSystemFontOfSize(FONT_SIZE);
                UIColor timeColor = UIColor.Black;

                // Periods appearance
                UIFont periodFont = UIFont.SystemFontOfSize(FONT_SIZE);
                UIColor periodColor = UIColor.Gray;
				//draw vertical lines
				var context = UIGraphics.GetCurrentContext();
				
	            context.SetLineWidth(0.5f);
				for (var i = 0; i <= 8; i++)
				{
					var lineWidth =  (i*DayWidth) ;
					context.BeginPath();
					context.MoveTo( lineWidth + 1,0);
					context.AddLineToPoint(lineWidth + 1,this.Frame.Height);
					context.StrokePath();
					
				}

                // Draw each times string
                for (Int32 i = 0; i < currentRows; i++)
                {
                    // Draw time
                    timeColor.SetColor();


                    // Save the context state 
                    context.SaveState();
                    context.SetStrokeColorWithColor(UIColor.LightGray.CGColor);

                    // Draw line with a black stroke color
                    // Draw line with a 1.0 stroke width
                    context.SetLineWidth(0.5f);
                    // Translate context for clear line
                    context.TranslateCTM(-0.5f, -0.5f);
                    context.BeginPath();
                    context.MoveTo(0,
                                   VERTICAL_OFFSET + i*VERTICAL_DIFF + (float) Math.Round(((FONT_SIZE + 4.0)/2.0)));
                    context.AddLineToPoint(TotalWidth,
                                           VERTICAL_OFFSET + i*VERTICAL_DIFF +
                                           (float) Math.Round((FONT_SIZE + 4.0)/2.0));
                    context.StrokePath();

                    if (i != currentRows - 1)
                    {
                        context.BeginPath();
                        context.MoveTo(0,
                                       VERTICAL_OFFSET + i*VERTICAL_DIFF +
                                       (float) Math.Round(((FONT_SIZE + 4.0f)/2.0f)) +
                                       (float) Math.Round((VERTICAL_DIFF/2.0f)));
                        context.AddLineToPoint(TotalWidth,
                                               VERTICAL_OFFSET + i*VERTICAL_DIFF +
                                               (float) Math.Round(((FONT_SIZE + 4.0f)/2.0f)) +
                                               (float) Math.Round((VERTICAL_DIFF/2.0f)));
                        float[] dash1 = {4.0f, 3.0f};
                        context.SetLineDash(0.0f, dash1, 2);
                        context.StrokePath();
                    }

                    // Restore the context state
                    context.RestoreState();
				}
                
            }

            ///////
        
	}
		
		
		
	}
}