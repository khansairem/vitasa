using Foundation;
using System;
using System.Threading.Tasks;
using UIKit;
using zsquared;

namespace vitavol
{
    public partial class VC_SCSite : UIViewController
    {
        // Input
        //   DetailsCameFrom
        //   SelectedSite
        //   LoggedInUser

        C_Global Global;

        public VC_SCSite (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			AppDelegate myAppDelegate = (AppDelegate)UIApplication.SharedApplication.Delegate;
			Global = myAppDelegate.Global;

			// set the standard background color
            View.BackgroundColor = C_Common.StandardBackground;

			if (Global.DetailsCameFrom == E_CameFrom.Login)
                B_Back.SetTitle("< Login", UIControlState.Normal);
            else
                B_Back.SetTitle("< Back", UIControlState.Normal);

            B_Back.TouchUpInside += (sender, e) => 
            {
                if (Global.DetailsCameFrom == E_CameFrom.Login)
                    PerformSegue("Segue_SCSiteToLogin", this);
                else
                    PerformSegue("Segue_SCSiteToSCSites", this);
            };

            L_SiteName.Text = Global.SelectedSite.Name;

            EnableUI(true);

            B_Closed.TouchUpInside += async (sender, e) => 
            {
                E_SiteStatus newStatus = E_SiteStatus.Closed;
				EnableUI(false);
				AI_Busy.StartAnimating();

				bool success = await Global.SelectedSite.UpdateSiteStatus(newStatus, Global.LoggedInUser.Token);
				if (success)
					Global.SelectedSite.Status = newStatus;

				AI_Busy.StopAnimating();
				EnableUI(true);
			};

            B_Accepting.TouchUpInside += async (sender, e) => 
            {
				E_SiteStatus newStatus = E_SiteStatus.Accepting;
				EnableUI(false);
				AI_Busy.StartAnimating();

				bool success = await Global.SelectedSite.UpdateSiteStatus(newStatus, Global.LoggedInUser.Token);
				if (success)
					Global.SelectedSite.Status = newStatus;

				AI_Busy.StopAnimating();
				EnableUI(true);
			};

            B_NearLimit.TouchUpInside += async (sender, e) => 
            {
                E_SiteStatus newStatus = E_SiteStatus.NearLimit;
				EnableUI(false);
				AI_Busy.StartAnimating();

				bool success = await Global.SelectedSite.UpdateSiteStatus(newStatus, Global.LoggedInUser.Token);
				if (success)
					Global.SelectedSite.Status = newStatus;

				AI_Busy.StopAnimating();
				EnableUI(true);
			};

            B_AtLimit.TouchUpInside += async (sender, e) => 
            {
                E_SiteStatus newStatus = E_SiteStatus.NotAccepting;
				EnableUI(false);
				AI_Busy.StartAnimating();

				bool success = await Global.SelectedSite.UpdateSiteStatus(newStatus, Global.LoggedInUser.Token);
				if (success)
					Global.SelectedSite.Status = newStatus;

				AI_Busy.StopAnimating();
				EnableUI(true);
			};

            B_Volunteers.TouchUpInside += (sender, e) => 
            {
                // clear all the dirty flags in the WorkItems to avoid saving stuff we shouldn't
                Global.ClearDirtyFlagOnIntents();

                PerformSegue("Segue_SCSiteToSCVolunteers", this);
            };

            B_SiteCalendar.TouchUpInside += (sender, e) => 
            {
                PerformSegue("Segue_SCSiteToSCSiteCalendar", this);
            };

            B_EditSiteDetails.TouchUpInside += (sender, e) => 
            {
                PerformSegue("Segue_SCSiteToEditSiteDetails", this);
            };

            IMG_Closed.Image = UIImage.FromBundle("blackstatus.jpg");
            IMG_Accepting.Image = UIImage.FromBundle("greenstatus.jpg");
            IMG_NearLimit.Image = UIImage.FromBundle("yellowstatus.jpg");
            IMG_AtLimit.Image = UIImage.FromBundle("redstatus.jpg");
		}

        private void EnableUI(bool enable)
        {
			int isiteStatus = (int)Global.SelectedSite.Status;
			string ssitestatus = C_VitaSite.N_SiteStatus[isiteStatus];
			L_ClientStatus.Text = ssitestatus;

            switch (Global.SelectedSite.Status)
            {
                case E_SiteStatus.Closed:
                    IMG_Currently.Image = UIImage.FromBundle("blackstatus.jpg");
                    break;
                case E_SiteStatus.Accepting:
					IMG_Currently.Image = UIImage.FromBundle("greenstatus.jpg");
					break;
                case E_SiteStatus.NearLimit:
					IMG_Currently.Image = UIImage.FromBundle("yellowstatus.jpg");
					break;
				case E_SiteStatus.NotAccepting:
					IMG_Currently.Image = UIImage.FromBundle("redstatus.jpg");
					break;
			}

			B_Closed.Enabled = enable && Global.SelectedSite.Status != E_SiteStatus.Closed;
			B_Accepting.Enabled = enable && Global.SelectedSite.Status != E_SiteStatus.Accepting;
			B_NearLimit.Enabled = enable && Global.SelectedSite.Status != E_SiteStatus.NearLimit;
			B_AtLimit.Enabled = enable && Global.SelectedSite.Status != E_SiteStatus.NotAccepting;

            B_Volunteers.Enabled = enable;
            B_SiteCalendar.Enabled = enable;

            B_Back.Enabled = enable;
		}
    }
}