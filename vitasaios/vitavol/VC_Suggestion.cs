using Foundation;
using System;
using UIKit;
using Xamarin.Forms;
using System.Threading.Tasks;
using zsquared;
using static zsquared.C_MessageBox;

namespace vitavol
{
    public partial class VC_Suggestion : UIViewController
    {
		C_Global Global;
        C_VitaUser LoggedInUser;
        bool Dirty;

		public VC_Suggestion (IntPtr handle) : base (handle)
        {
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			AppDelegate myAppDelegate = (AppDelegate)UIApplication.SharedApplication.Delegate;
            Global = myAppDelegate.Global;

            LoggedInUser = Global.GetUserFromCacheNoFetch(Global.LoggedInUserId);

			// ----------- init the button handlers --------

			B_Back.TouchUpInside += async (sender, e) =>
			{
                if (!Dirty)
                {
                    PerformSegue("Segue_SuggestionToSuggestions", this);
                    return;
                }

                E_MessageBoxResults mbres = await MessageBox(this,
                      "Changes have been made",
                      "Changes were made to the suggestion and not saved. Save them?",
                       E_MessageBoxButtons.YesNo);
                if (mbres != E_MessageBoxResults.Yes)
				{
                    // the user doesn't want us to save any changes
					PerformSegue("Segue_SuggestionToSuggestions", this);
					return;
				}

				Global.SelectedSuggestion.Subject = TB_Title.Text;
                Global.SelectedSuggestion.Text = TxV_Body.Text;

                AI_Busy.StartAnimating();
                EnableUI(false);

                bool success = await SaveSuggestion();

                AI_Busy.StopAnimating();
                EnableUI(true);

                if (!success)
                {
                    E_MessageBoxResults mbres1 = await MessageBox(this,
                          "Error",
                          "Unable to add or update the suggestion",
                          E_MessageBoxButtons.Ok);
                    return;
                }

                if (Global.ViewCameFrom == E_ViewCameFrom.VolOptions)
    				PerformSegue("Segue_SuggestionToVolunteerOptions", this);
                else
					PerformSegue("Segue_SuggestionToSuggestions", this);
			};

			B_DeleteThisSuggestion.TouchUpInside += async (sender, e) =>
			{
                E_MessageBoxResults mbRes = await MessageBox(this, 
                                                               "Delete item?", 
                                                               "Are you sure you want to delete this suggestion?", 
                                                               E_MessageBoxButtons.YesNo);
                
                if ((mbRes == E_MessageBoxResults.No) || (Global.SelectedSuggestion.id == -1))
                {
					PerformSegue("Segue_SuggestionToSuggestions", this);

                    return;
				}

                AI_Busy.StartAnimating();
                EnableUI(false);

                bool success = await Global.SelectedSuggestion.RemoveSuggestion(LoggedInUser.Token);
                LoggedInUser.Suggestions.Remove(Global.SelectedSuggestion);

                AI_Busy.StopAnimating();
                EnableUI(true);

                if (!success)
                {
                    E_MessageBoxResults mbres = await MessageBox(this,
                                                                  "Error",
                                                                  "Unable to delete the suggestion.",
                                                                   E_MessageBoxButtons.Ok);
                }

				PerformSegue("Segue_SuggestionToSuggestions", this);
			};

            B_Save.TouchUpInside += async (sender, e) => 
            {
				Global.SelectedSuggestion.Subject = TB_Title.Text;
				Global.SelectedSuggestion.Text = TxV_Body.Text;

                AI_Busy.StartAnimating();
                EnableUI(false);

				bool success = await SaveSuggestion();

				AI_Busy.StopAnimating();
				EnableUI(true);

				if (!success)
                {
                    E_MessageBoxResults mbres = await MessageBox(this, 
                         "Error", 
                         "Unable to add or update the suggestion", 
                         E_MessageBoxButtons.Ok);
					return;
                }

				PerformSegue("Segue_SuggestionToSuggestions", this);
			};

			TB_Title.AddTarget((sender, e) =>
			{
                Dirty = true;
			}, UIControlEvent.EditingChanged);

            TxV_Body.Changed += (sender, e) => 
            {
                Dirty = true;
            };

			// ---------- init the fields --------
            // we only allow changing and editing if the suggestion is still in the Open state

			L_Submitter.Text = LoggedInUser.Name;
			L_Date.Text = Global.SelectedSuggestion.CreateDate.ToString();
			L_Status.Text = Global.SelectedSuggestion.Status.ToString();
			TB_Title.Text = Global.SelectedSuggestion.Subject;
			TxV_Body.Text = Global.SelectedSuggestion.Text;

            Dirty = Global.SelectedSuggestion.id == -1;

            if (Global.SelectedSuggestion.Status != E_SuggestionStatus.Open)
            {
                Dirty = false;
                TB_Title.Enabled = false;
                TxV_Body.UserInteractionEnabled = false;
                B_Save.Enabled = false;
                B_DeleteThisSuggestion.Enabled = false;
            }
		}

        public override void ViewDidAppear(bool animated)
        {
			// set the standard background color
			View.BackgroundColor = C_Common.StandardBackground;
		}

        private void EnableUI(bool en)
        {
            TB_Title.Enabled = en;
            TxV_Body.UserInteractionEnabled = en;
            B_Back.Enabled = en;
            B_Save.Enabled = en;
            B_DeleteThisSuggestion.Enabled = en;
        }

        private async Task<bool> SaveSuggestion()
        {
			bool success = false;
            try
            {
                if (Global.SelectedSuggestion.id == -1)
                {
                    success = await Global.SelectedSuggestion.AddSuggestion(LoggedInUser.Token);
                    //success = await Global.LoggedInUser.AddSuggestion(Global.SelectedSuggestion);
                    LoggedInUser.Suggestions.Add(Global.SelectedSuggestion);
                }
                else
                    success = await Global.SelectedSuggestion.UpdateSuggestion(LoggedInUser.Token);
                    //success = await Global.LoggedInUser.UpdateSuggestion(Global.SelectedSuggestion);
                Dirty = false;
            }
            catch 
            {
                success = false;
            }

            return success;
		}
 	}
}