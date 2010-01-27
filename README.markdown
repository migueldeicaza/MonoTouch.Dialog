MonoTouch.Dialog
================

MonoTouch.Dialog is a foundation to create dialog boxes without
having to write dozens of delegates and controllers for a user
interface.   Currently this supports creating Dialogs based on
navigation controllers that support:

  * On/Off controls
  * Slider (floats)
  * String informational rendering.
  * Text Entry
  * Password Entry
  * Jump to HTML page
  * Radio elements.
  * Dates, Times and Dates+Times.

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

  Examples:

        public string Version = "1.2.3";

        [OnTap ("Login")]
        public string Login;

        [Caption ("(C) FooBar, Inc")]
        string copyright;

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

First a sample of how you would create a UI:

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
              }
            },
          },
          new Section () {
            new EntryElement ("Login", "enter", "miguel"),
            new EntryElement ("Password", "enter", "password", true),
            new DateElement ("Select Date", DateTime.Now),
            new TimeElement ("Select Time", DateTime.Now),
          },

You will need a RootElement to get things rolling.

RootElement
-----------

RootElements are responsible for showing a full configuration page.

At least one RootElement is required to start the MonoTouch.Dialogs
process.

RootElements can also be used inside Sections to trigger
loading a new nested configuration page.   When used in this mode
the caption provided is used while rendered inside a section and
is also used as the Title for the subpage.

If a RootElement is initialized with a section/element value then
this value is used to locate a child Element that will provide
a summary of the configuration which is rendered on the right-side
of the display.

RootElements are also used to coordinate radio elements.  The
RadioElement members can span multiple Sections (for example to
implement something similar to the ring tone selector and separate
custom ring tones from system ringtones).   The summary view will show
the radio element that is currently selected.

Sections are added by calling the Add method which supports the
C# 4.0 syntax to initialize a RootElement in one pass.

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
  * RadioElements (to provide a radio-button feature).
  * EntryElement (to enter one-line text or passwords)
  * DateTimeElement (to edit dates and times).
  * DateElement (to edit just dates)
  * TimeElement (to edit just times)


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