MonoTouch.Dialog
================

MonoTouch.Dialog is a foundation to create dialog boxes and show
table-based information without having to write dozens of delegates
and controllers for the user interface.  Currently this supports
creating Dialogs based on navigation controllers that support:

  * On/Off controls
  * Slider (floats)
  * String informational rendering
  * Text Entry
  * Password Entry
  * Jump to HTML page
  * Radio elements
  * Dates, Times and Dates+Times
  * Arbitary UIViews
  * Pull-to-refresh functionality.

Miguel (miguel@gnome.org)

Screenshot
==========

This [screenshot](http://tirania.org/tmp/a.png) was created with 
[this code](http://gist.github.com/281469)

Using MonoTouch.Dialog
======================

MonoTouch.Dialog core entry point is a UIViewController called the
MonoTouch.Dialog.DialogViewController.  You initialize instances of
this object from an object of type "RootElement".

RootElements can be created either manually with the "Elements" API by
creating the various nodes necessary to render the information.  You
would use this if you need control, if you want to extend the features
supported by MonoTouch.Dialogs or if you want to dynamically generate
the content for your dialog.

Additionally, there is a trivial Reflection-based constructor that can
be used for quickly putting together dialogs, for example, creating an
account page is as trivial as:

    class AccountInfo {
        [Section]
        public bool AirplaneMode;
    
        [Section ("Data Entry", "Your credentials")]
    
        [Entry ("Enter your login name")]
        public string Login;
    
        [Caption ("Password"), Password ("Enter your password")]
        public string passwd;

        [Section ("Travel options")]
        public SeatPreference preference;
  }

    void Setup ()
    {
        account = new AccountInfo ();
    
        var bc = new BindingContext (this, account, "Seat Selection");
    }

Which produces this UI:

![Rendering of AccountInfo](MonoTouch.Dialog/raw/master/sample.png)

Also see this [screenshot](http://tirania.org/tmp/a.png) was created
with [this code](http://gist.github.com/281469)

To create nested UIs that provide automatic navigation, you would just
create an instance of that class.  

Samples Included
----------------

The sample program exercises various features of the API and should be
a useful guide on how to implement certain features.  One of the demos
uses the Elements API to replicate the "Settings" application on the
iPhone.  

The High-Level Reflection API
=============================

The Reflection-based dialog construction is used by creating an object
of class MonoTouch.Dialog.BindingContext, the method takes three
parameters:

  * An object that will be used to resolve Tap targets.

  * The object that will be edited.

  * The title for the page to be rendered.

A very simple dialog that contains a checkbox is shown here:

    class Settings {
        public bool AirplaneMode;
    }

The above will generate a page that contains a single item with the
caption "Airplane Mode" and a on/off switch.   The caption is computed
based on the field name.  In this case "AirplaneMode" becomes
"Airplane Mode".   MonoTouch.Dialogs supports other conventions so
"AirplaneMode", "airplaneMode" and "airplane_mode" all produce the
same caption "Airplane Mode".

If you need to control the actual caption (for example to include
special characters, use a different spelling or you are reusing an
existing class) you just need to attach the [Caption] attribute to
your variable, like this:

        [Caption ("Your name is:")]
        string userName;

The dialog contents are rendered in the same order that the fields are
declared in the class.  You can use the [Section] attribute to
group information in sections that make sense.   You can use the
[Section] attribute in a few ways:

        [Section]

>>Creates a new section, with no headers or footers.

        [Section (header)]

>> Creates a new section, with the specified header and no footer.

        [Section (header, footer)]**
>> Creates a new section with the specified header and footer.

These are the current widgets supported by the Reflection API:

### String constants and Buttons. ###

  Use the string type.   If the type has a value, in 
  addition to showing the caption, it will render its
  value on the right.

  You can add the [OnTap] attribute to your string 
  to invoke a method on demand.

  You can add the [Multiline] attribute to your string
  to make the cell render in multiple lines.   And you 
  can use the [Html] attribute on a string, in that
  case the value of the string should contain the url
  to load in the embedded UIWebView. 

  Examples:

        public string Version = "1.2.3";

        [OnTap ("Login")]
        public string Login;

        [Caption ("(C) FooBar, Inc")]
        string copyright;

        [Caption ("This is a\nmultiline caption")]
        [Multiline]
        string multiline;

### Text Entry and Password Entries.###

  Use the string type for your field and annotate the 
  string with the [Entry] attribute.   If you provide
  an argument to the [Entry] attribute, it will be used
  as the greyed-out placeholder value for the UITextField.

  Use the [Password] attribute instead of [Entry] to 
  create a secure entry line.
 
  Examples:

        [Entry ("Your username")]
        public string Login;
  
        [Entry]
        public string StreetName;

        [Password, Caption ("Password")]
        public string passwd;

### On/off switches ###

  Use a bool value to store an on/off setting, by default you
  will get an On/off switch, but you can change this behavior to
  just show a checkbox instead by using the [Checkbox] attribute:

  Examples:

        bool OnOffSwitch;

        [Checkbox]
        bool ReadyToRun;

### Float values ###

  Using a float in your source will provide a slider on the 
  screen.   You can control the ranges of the values by
  using the [Range (low,high)] attribute.   Otherwise the
  default is to edit a value between 0 and 1.

  Examples:

        float brightness;

        [Range (0, 10), Caption ("Grade")]
        float studentGrade;

### Date Editing ###

  Use a "DateTime" object in your class to present a date
  picker.

  By default this will provide a date and time editor, if you
  only want to edit the date, set the [Date] attribute, if you
  only want to edit the time, set the [Time] attribute:

  Examples:

        [Date]
        DateTime birthday;

        [Time]
        DateTime alarm;

        [Caption ("Meeting Time")]
        DateTime meetingTime;

### Enumeration value ###

  Monotouch.Dialogs will automatically turn an enumeration
  into a radio selection.   Merely specify the enumeration
  in your file:

  Examples:

          enum SeatPreference { Window, Aisle, MiddleSeat }

          [Caption ("Seat Preference")]
          SeatPreference seat;

### Images ###

  Variables with type UIImage will render the image as a 
  thumbnail and will invoke the image picker if tapped on.

  Examples:

        UIImage ProfilePicture;

### Ignoring Some Fields ###

  If you want to ignore a particular field just apply the [Skip]
  attribute to the field.   

  Examples:
        [Skip] Guid UniquId;

### Nested Dialogs ###

  To create nested dialogs just use a nested class, the reflection
  binder will create the necessary navigation bits based on the
  container model.

  The value for a nested dialog must not be null.

  Examples:

	class MainSettings {
	    string Subject;
	    string RoomName;
	    TimeRange Time;
	}

	class TimeRange {
	    [Time] DateTime Start;
	    [Time] DateTime End;
	}

	To initialize:

	new MainSettings () {
	    Subject = "Review designs",
	    RoomName = "Conference Room II",
	    Time = new TimeRange {
	        Start = DateTime.Now,
		End   = DateTime.Now
	    }
        }

### IEnumerable as a Radio Source ###

You can use any type that implements IEnumerable, including
generics (which implement IEnumerable) as a source of values
for creating a one-of-many selector, similar to the radio-like
selection that you get from an enumeration.

To use this, you will need an int value that has the [RadioSelection]
attribute set to hold the value that will be selected on startup
and also to hold the new value when done.

For example:

        class MainSettings {
	    [RadioSelection ("Themes")]
	    public int CurrentTheme;
	    public IList<string> Themes;
	}

The value rendered is the value rendered by calling ToString() on the
value returned by IEnumerable.

Creating a Dialog From the Object
---------------------------------

Once you have created your class with the proper attributes, you
create a binding context, like this:

        BindingContext context;

        public void Setup ()
        {
            // Create the binder.
            context = new BindingContext (this, data, "Settings");

            // Create our UI
            // Pass our UI (context.Root) and request animation (true)
            var viewController = new DialogViewController (context.Root, true);

            navigation.PushViewController (viewController, true);
        }

This will render the information.   To fetch the values back after
editing you need to call context.Fetch ().   You can do this from your
favorite handler, and at that point you can also call
context.Dispose() to assist the GC in releasing any large resources it
might have held.

The Low-Level Elements API
==========================

All that the Reflection API does is create a set of nodes from the
Elements API.   

First a sample of how you would create a UI taking advantage of 
C# 3.0 initializers:

        var root = new RootElement ("Settings") {
          new Section (){
            new BooleanElement ("Airplane Mode", false),
            new RootElement ("Notifications", 0, 0) {
              new Section (null, 
                  "Turn off Notifications to disable Sounds\n" +
                  "Alerts and Home Screen Badges for the."){
                new BooleanElement ("Notifications", false)
              }
            }},
          new Section (){
            new RootElement ("Brightness"){
              new Section (){
                new FloatElement (null, null, 0.5f),
                new BooleanElement ("Auto-brightness", false),
		new UILabel ("I am a simple UILabel!"),
              }
            },
          },
          new Section () {
            new EntryElement ("Login", "enter", "miguel"),
            new EntryElement ("Password", "enter", "password", true),
            new DateElement ("Select Date", DateTime.Now),
            new TimeElement ("Select Time", DateTime.Now),
          },

You will need a RootElement to get things rolling.   The nested
structure created by Sections() and Elements() are merely calls to
either RootElement.Add () or Section.Add() that the C# compiler 
invokes for us.

Additionally notice that when adding elements to a section, you
can use either Elements or UIViews directly.   The UIViews are
just wrapped in a special UIViewElement element.

In addition

RootElement
-----------

RootElements are responsible for showing a full configuration page.

At least one RootElement is required to start the MonoTouch.Dialogs
process.

If a RootElement is initialized with a section/element value then
this value is used to locate a child Element that will provide
a summary of the configuration which is rendered on the right-side
of the display.

RootElements are also used to coordinate radio elements.  The
RadioElement members can span multiple Sections (for example to
implement something similar to the ring tone selector and separate
custom ring tones from system ringtones).  The summary view will show
the radio element that is currently selected.  To use this, create
the Root element with the Group constructor, like this:

       var root = new RootElement ("Meals", new RadioGroup ("myGroup", 0))

The name of the group in RadioGroup is used to show the selected value
in the containing page (if any) and the value, which is zero in this
case, is the index of the first selected item. 

Root elements can also be used inside Sections to trigger
loading a new nested configuration page.   When used in this mode
the caption provided is used while rendered inside a section and
is also used as the Title for the subpage.   For example:

	var root = new RootElement ("Meals") {
	    new Section ("Dinner"){
                new RootElement ("Desert", new RadioGroup ("desert", 2) {
                    new Section () {
                        new RadioElement ("Ice Cream", "desert"),
                        new RadioElement ("Milkshake", "desert"),
                        new RadioElement ("Chocolate Cake", "desert")
                    }
                }
            }
        }

In the above example, when the user taps on "Desert", MonoTouch.Dialog
will create a new page and navigate to it with the root being "Desert"
and having a radio group with three values.

In this particular sample, the radio group will select "Chocolate
Cake" in the "Desert" section because we passed the value "2" to the
RadioGroup.  This means pick the 3rd item on the list (zero-index). 

Sections are added by calling the Add method or using the C# 4
initializer syntax.  The Insert methods are provided to insert
sections with an animation.

Additionally, you can create RootElement by using LINQ, like this:

        new RootElement ("LINQ root") {
            from x in new string [] { "one", "two", "three" }
               select new Section (x) {
                  from y in "Hello:World".Split (':')
                    select (Element) new StringElement (y)
               }
        }

If you create the RootElement with a Group instance (instead of a
RadioGroup) the summary value of the RootElement when displayed in a
Section will be the cummulative count of all the BooleanElements and
CheckboxElements that have the same key as the Group.Key value.

Sections
--------

Sections are used to group elements in the screen and they are the
only valid direct child of the RootElement.    Sections can contain
any of the standard elements, including new RootElements.

RootElements embedded in a section are used to navigate to a new
deeper level.

Sections can have headers and footers either as strings, or as
UIViews.  Typically you will just use the strings, but to create
custom UIs you can use any UIView as the header or the footer.  You
can either use a string or a view, you would create them like this:

	var section = new Section ("Header", "Footer")

To use views, just pass the views to the constructor:

        var header = new UIImageView (Image.FromFile ("sample.png"));
        var section = new Section (image)


Standard Elements
-----------------

MonoTouch.Dialog comes with various standard elements that you can
use:

  * BooleanElement
  * CheckboxElement
  * FloatElement
  * HtmlElement (to load web pages)
  * ImageElement (to pick images)
  * StringElement
    To render static strings
    To render strings with a read-only value.
    To be used as "buttons", pass a delegate for this.
  * MultilineElement
    Derives from StringElement, used to render multi-line cells.
  * RadioElements (to provide a radio-button feature).
  * EntryElement (to enter one-line text or passwords)
  * DateTimeElement (to edit dates and times).
  * DateElement (to edit just dates)
  * TimeElement (to edit just times)
  * BadgeElement 
    To render images (57x57) or calendar entries next to the text.

Values
------

Elements that are used to capture user input expose a public "Value"
property that holds the current value of the element at any time.  It
is automatically updated as the user uses the application and does not
require any programmer intervention to fetch the state of the control.

This is the behavior for all of the Elements that are part of
MonoTouch.Dialog but it is not required for user-created elements.

EntryElement
------------

The EntryElement is used to get user input and is initialized with
three values: the caption for the entry that will be shown to the
user, a placeholder text (this is the greyed-out text that provides a
hint to the user) and the value of the text.

The placeholder and value can be null, the caption can not.

At any point, the value of the EntryElement can be retrieved by
accessing its Value property.

Additionally the KeyboardType property can be set at creation time to
the keyboard type style desired for the data entry.  This can be used
to configure the keyboard for numeric input, phone input, url input or
email address input (The values of UIKeyboardType).

UIViewElement
-------------

Use this element to quickly add a standard UIView as cell in a UITableView.



Booleans
--------

The BoolElement is the base class for both the UISwitch-based boolean
rendered image as well as the BooleanImageElement which is a boolean
that can render the stage using a string.

Validation
----------

Elements do not provide validation themselves as the models that are
well suited for web pages and desktop applications do not map
directly to the iPhone interaction model.

If you want to do data validation, you should do this when the user
triggers an action with the data entered.  For example a "Done" or
"Next" buttons on the top toolbar, or some StringElement used as a
button to go to the next stage.   

This is where you would perform basic input validation, and perhaps
more complicated validation like checking for the validity of a
user/password combination with a server.

How you notify the user of an error is application specific: you could
pop up a UIAlertView or show a hint.

Creating Your Own Elements
--------------------------

You can create your own element by deriving from either an existing
Element or by deriving from the root class Element.

To create your own Element, you will want to override the following
methods:

        // To release any resources 
        void Dispose (bool disposing);

        // To retrieve the UITableViewCell for your element
        UITableViewCell GetCell (UITableView tv)

        // To retrieve a "summary" that can be used with
        // a root element to render a summary one level up.  
        string Summary ()

        // To detect when the user has tapped on the cell
        void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)

Pull to Refresh Support
=======================

Pull to Refresh is a visual effect originally found in Tweetie2 which
became a popular effect among many applications.

To add automatic pull-to-refersh support to your dialogs, you only
need to do two things: hook up an event handler to be notified when
the user pulls the data and notify the DialogViewController when the
data has been loaded to go back to its default state.

Hooking up a notification is simple, just connect to the
RefreshRequested event on the DialogViewController, like this:

        dvc.RefreshRequested += OnUserRequestedRefresh;

Then on your method OnUserRequestedRefresh, you would queue some data
loading, request some data from the net, or spin a thread to compute
the data.  Once the data has been loaded, you must notify the
DialogViewController that the new data is in, and to restore the view
to its default state, you do this by calling ReloadComplete:

	dvc.ReloadComplete ();


Customizing the DialogViewController
====================================

Both the Reflection and the Elements API use the same
DialogViewController.  Sometimes you will want to customize the look
of the view.   

The DialogViewController is merely a subclass of the
UITableViewController and you can customize it in the same way that
you would customize a UITableViewController.

For example, if you wanted to change the list style to be either
Grouped or Plain, you could set this value by changing the property
when you create the controller, like this:

        var myController = new DialogViewController (root, true){
            Style = UITableViewStyle.Grouped;
        }

For more advanced customizations, like setting the default background
for the DialogViewController, you would need to create a subclass of
it and override the proper methods.   

This example shows how to use an image as the background for the
DialogViewController:

    class SpiffyDialogViewController : DialogViewController {
        UIImage image;
    
        public SpiffyDialogViewController (RootElement root, bool pushing, UIImage image) 
            : base (root, pushing) 
        {
            this.image = image;
        }
    
        public override LoadView ()
        {
            base.LoadView ();
            var color = UIColor.FromPatternImage(image);
            Root.TableView.BackgroundColor = UIColor.Clear;
            ParentViewController.View.BackgroundColor = color;
        }
    }
