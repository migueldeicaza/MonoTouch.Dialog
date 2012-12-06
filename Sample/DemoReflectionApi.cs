//
// Sample showing the core Element-based API to create a dialog
//
using System;
using System.Collections.Generic;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Linq;

namespace Sample
{
	// Use the preserve attribute to inform the linker that even if I do not
	// use the fields, to not try to optimize them away.
	
	[Preserve (AllMembers=true)]
	class Settings {
	[Section]
		public bool AccountEnabled;
		[Skip] public bool Hidden;
				
	[Section ("Account", "Your credentials")]
		
		[Entry ("Enter your login name")]
		public string Login;
		
		[Password ("Enter your password")]
		public string Password;
		
	[Section ("Autocapitalize, autocorrect and clear button")]
		
		[Entry (Placeholder = "Enter your name", AutocorrectionType = UITextAutocorrectionType.Yes, AutocapitalizationType = UITextAutocapitalizationType.Words, ClearButtonMode = UITextFieldViewMode.WhileEditing)]
		public string Name;
		
	[Section ("Time Editing")]
		
		public TimeSettings TimeSamples;
		
	[Section ("Enumerations")]
		
		[Caption ("Favorite CLR type")]
		public TypeCode FavoriteType;
		
	[Section ("Checkboxes")]
		[Checkbox]
		bool English = true;
		
		[Checkbox]
		bool Spanish;
		
	[Section ("Image Selection")]
		public UIImage Top;
		public UIImage Middle;
		public UIImage Bottom;
		
	[Section ("Multiline")]
		[Caption ("This is a\nmultiline string\nall you need is the\n[Multiline] attribute")]
		[Multiline]
		public string multi;
		
	[Section ("IEnumerable")]
		[RadioSelection ("ListOfString")] 
		public int selected = 1;
		public IList<string> ListOfString;

	[Section ("Custom Element Attribute")]
		[MyFavorite]
		[Caption("I like ice cream")]
		public bool LikeIceCream = true;

		[MyFavorite]
		[Caption("I like veggies")]
		public bool LikeVegetables = false;

	[Section ("Index Tags: Animals")]

		[Checkbox]
		[IndexTags("animals,dogs,small")]
		public bool Chihuahua = false;

		[Checkbox]
		[IndexTags("animals,cats,small")]
		public bool Persian = false;

		[Checkbox]
		[IndexTags("animals,dogs,big")]
		public bool GermanShepherd = false;

		[Checkbox]
		[IndexTags("animals,cats,big")]
		public bool MaineCoon = false;

		[Section("")]
		[Caption("Kinds")]
		[IndexTags("kinds")]
		[Entry]
		public string Kinds = "";

		[Caption("Sizes")]
		[IndexTags("sizes")]
		[Entry]
		public string Sizes = "";

	}

	public class Callbacks {
		private BindingContext _context;

		public void Initalize (BindingContext context)
		{
			_context = context;
			foreach (Element el in _context.GetElementsForTag("animals")) {
				var chkEl = el as CheckboxElement;
				if (chkEl != null)
					chkEl.Tapped += UpdateCatsAndDogs;
			}
		}

		public void UpdateCatsAndDogs() {
			int dogs = _context.GetElementsForTag("dogs").Where(el => ((CheckboxElement) el).Value).Count();
			int cats = _context.GetElementsForTag("cats").Where(el => ((CheckboxElement) el).Value).Count();
			int small = _context.GetElementsForTag("small").Where(el => ((CheckboxElement) el).Value).Count();
			int big = _context.GetElementsForTag("big").Where(el => ((CheckboxElement) el).Value).Count();

			var kindsEl = _context.GetElementsForTag("kinds").FirstOrDefault() as EntryElement;
			var sizesEl = _context.GetElementsForTag("sizes").FirstOrDefault() as EntryElement;

			kindsEl.Value = string.Format("{0} dogs, {1} cats", dogs, cats);
			sizesEl.Value = string.Format("{0} small, {1} big", small, big);
		}
	}

	public class MyFavoriteAttribute : CustomElementAttribute {

		#region implemented abstract members of CustomElementAttribute		
		public override Element CreateElement (string caption, System.Reflection.MemberInfo forMember, Type memberType, object memberValue, object[] attributes)
		{
			bool value = (bool) Convert.ChangeType(memberValue, typeof(bool));
			UIImage favorite = UIImage.FromFile ("favorite.png");
			UIImage favorited = UIImage.FromFile ("favorited.png");
			return new BooleanImageElement(caption, value, favorited, favorite);
		}		

		public override object GetValue (Element element, Type resultType)
		{
			return Convert.ChangeType(((BooleanImageElement) element).Value, resultType);
		}		
		#endregion
	}

	public class TimeSettings {
		public DateTime Appointment;
		
		[Date]
		public DateTime Birthday;
		
		[Time]
		public DateTime Alarm;
	}

	
	
	public partial class AppDelegate 
	{
		Settings settings;
		
		public void DemoReflectionApi ()
		{	
			if (settings == null){
				var image = UIImage.FromFile ("monodevelop-32.png");
				
				settings = new Settings () {
					AccountEnabled = true,
					Login = "postmater@localhost.com",
					TimeSamples = new TimeSettings () {
						Appointment = DateTime.Now,
						Birthday = new DateTime (1980, 6, 24),
						Alarm = new DateTime (2000, 1, 1, 7, 30, 0, 0)
					},
					FavoriteType = TypeCode.Int32,
					Top = image,
					Middle = image,
					Bottom = image,
					ListOfString = new List<string> () { "One", "Two", "Three" }
				};
			}

			var cb = new Callbacks();
			var bc = new BindingContext (cb, settings, "Settings");
			cb.Initalize(bc);

			var dv = new DialogViewController (bc.Root, true);
			
			// When the view goes out of screen, we fetch the data.
			dv.ViewDisappearing += delegate {
				// This reflects the data back to the object instance
				bc.Fetch ();
				
				// Manly way of dumping the data.
				Console.WriteLine ("Current status:");
				Console.WriteLine (
				    "AccountEnabled:   {0}\n" +
				    "Login:            {1}\n" +
				    "Password:         {2}\n" +
					"Name:      	   {3}\n" +
				    "Appointment:      {4}\n" +
				    "Birthday:         {5}\n" +
				    "Alarm:            {6}\n" +
				    "Favorite Type:    {7}\n" + 
				    "IEnumerable idx:  {8}\n" +
					"I like ice cream: {9}\n" +
					"I like veggies:   {10}\n" +
					"Animal kinds:     {11}\n" +
					"Animal sizes:     {12}",
				    settings.AccountEnabled, settings.Login, settings.Password, settings.Name,
				    settings.TimeSamples.Appointment, settings.TimeSamples.Birthday, 
				    settings.TimeSamples.Alarm, settings.FavoriteType,
				    settings.selected, 
					settings.LikeIceCream, settings.LikeVegetables,
					settings.Kinds, settings.Sizes);
			};
			navigation.PushViewController (dv, true);	
		}
	}
}