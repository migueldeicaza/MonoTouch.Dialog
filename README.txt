
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

There are two layers, one is the tree API in Element.cs and a
second layer will be map .NET classes that have been decoared with
attributes to and from this tree. 

The sample program merely tries to exercise the capabilities by trying
to replicate the "Settings" application on the iPhone.   The reflection
API is still very incomplete, but it shows the general idea of how
it will work.

If no [Caption] attribute is provided, the caption is inferred from the
variable name (the fields airplaneMode, AirplaneMode and airplane_mode
all become "Airplane Mode").   Attributes are used to create sections
in the class:

	class Settings {
		[Section]
		bool AirplaneMode;
		
		[Section ("Data Entry")]
		
		[Entry ("Enter your login name")]
		string Login;
		
		[Caption ("Password"), Password ("Enter your password")]
		string passwd;
	}


- Miguel
