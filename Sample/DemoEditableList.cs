//
// Sample showing the core Element-based API to create a dialog
//
using System;
using System.Collections.Generic;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace Sample
{
	public class SavedLogin
	{
		public string Login;
		public string Description;
	}

	// Use the preserve attribute to inform the linker that even if I do not
	// use the fields, to not try to optimize them away.

	public partial class AppDelegate
	{
		private List<SavedLogin> _SavedLoginCol;
		private EntryElement _Login;
		private EntryElement _Description;
		private EntryElement _Password;
		private RootElement _SavedLogins;
		private BooleanImageElement _SaveLogin;

		public void DemoEditableList ()
		{
			_SavedLoginCol = new List<SavedLogin> ();
			
			ShowLogin(true);
		}

		private Section BuildLogins (bool ShowSavedLogins)
		{
			if (_Description != null)
				_Description = null;
			
			if (_SavedLogins != null)
				_SavedLogins = null;
			
			_Password = new EntryElement ("Password", "Enter Password", "", true);
			_SaveLogin = new BooleanImageElement ("Save Login", _SaveLogin == null ? false : _SaveLogin.Value, UIImage.FromFile ("checked.png"), UIImage.FromFile ("unchecked.png"), ShowLogin);
			
			Section sect = new Section ();
			
			if (_SavedLoginCol.Count == 0 || _SavedLoginCol.Count > 0 && !ShowSavedLogins) {
				_Login = new EntryElement ("Login", "Enter Login", _Login == null ? "" : _Login.Value);
				_Description = new EntryElement ("Description", "Enter Description", "");
				
				sect.Add (_Login);
				sect.Add (_Password);
				
				if (_SaveLogin.Value)
					sect.Add (_Description);
				
				sect.Add (_SaveLogin);
				
			} else {
				Section lSect = new Section ();
				
				foreach (SavedLogin x in _SavedLoginCol) {
					lSect.Add (new RadioElement (x.Description, x.Login));
				}
				
				_SavedLogins = new RootElement ("Login", new RadioGroup ("Login", 0), true) { lSect };
				
				_SavedLogins.OnCommitEditingStyle += Handle_SavedLoginsOnCommitEditingStyle;
				
				sect.Add (_SavedLogins);
				sect.Add (_Password);
				sect.Add (new StringElement ("Use new login", ShowLogin) { Alignment = UITextAlignment.Center });
				
			}
			
			return sect;
			
		}

		void Handle_SavedLoginsOnCommitEditingStyle (object sender, CommitEditingStyleArgs e)
		{
			_SavedLoginCol.RemoveAt(e.IndexPath.Row);
			
			if (_SavedLoginCol.Count == 0)
				ShowLogin (true);
			
		}
		
		public void DoLogin ()
		{
			string Login = string.Empty;
			string Password = string.Empty;
			string Description = string.Empty;
			
			if(_SavedLogins == null)
			{
				if(_Login == null)
				{
					Utilities.UnsuccessfulMessage("Login not defined");
					return;
				}
				
				Login = _Login.Value;
				
			}
			else
			{
				Login = _SavedLoginCol[_SavedLogins.RadioSelected].Login;
				
			}
			
			if(_Password == null)
			{
				Utilities.UnsuccessfulMessage("Password not defined");
				return;
			}
			
			Password = _Password.Value;
		
			if(_SaveLogin.Value)
			{
				
				if(_Description != null)
				{
					if(!string.IsNullOrEmpty(_Description.Value))
					{
						Description = _Description.Value;
					}
				}
				
				if(Description.Length > 25)
				{
					Utilities.UnsuccessfulMessage("You cannot have a description longer than 25 characters");
					return;
				}
				
				_SavedLoginCol.Add(new SavedLogin(){Login=Login,Description=Description});
			}
			
			ShowLogin(true);
			
		}
		
		public void ShowLogin()
		{
			ShowLogin(false);
		}
		
		public void ShowLogin (bool ShowSavedLogins)
		{
			
			RootElement root = new RootElement ("Login") { 
				BuildLogins (ShowSavedLogins), 
				new Section { 
					new StringElement ("Login", DoLogin) { 
						Alignment = UITextAlignment.Center 
					} 
				} 
			};
			
			var dv = new DialogViewController (root, false);
			
			navigation.PushViewController (dv, false);
			
		}
		
	}
	
}
