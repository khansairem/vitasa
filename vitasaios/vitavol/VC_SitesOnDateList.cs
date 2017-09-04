using Foundation;
using System;
using UIKit;
using System.Collections.Generic;
using System.Threading.Tasks;
using zsquared;

namespace vitavol
{
    public partial class VC_SitesOnDateList : UIViewController
    {
        C_Global Global;
        List<C_WorkItem> WorkItemsOnDateAllSites;

        public VC_SitesOnDateList (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			AppDelegate myAppDelegate = (AppDelegate)UIApplication.SharedApplication.Delegate;
			Global = myAppDelegate.Global;

            L_Date.Text = "Date: " + Global.SelectedDate.ToString("mmm dd, yyyy");

			B_Back.TouchUpInside += (sender, e) => 
            {
                Global.OpenSitesThatNeedHelp = null;
                PerformSegue("Segue_SitesOnDateListToCalendar", this);
            };

            B_ShowOnMap.TouchUpInside += (sender, e) => 
            {
                PerformSegue("Segue_SitesOnDateListToSitesOnDateMap", this);
            };

			// from the all sites list, select the ones that are open on this date
			List<C_VitaSite> openSites = C_VitaSite.SitesOpenOnDay(Global.SelectedDate, Global.AllSites);
			// and get the workitems for those sites; shouldn't be any work items on closed dates...
            WorkItemsOnDateAllSites = C_VitaSite.GetWorksItemsOnDateFromSites(Global.SelectedDate, openSites);

			// build a list of sites that need help on this date
			List<C_VitaSite> openSitesThatNeedHelp = new List<C_VitaSite>();
            foreach(C_VitaSite site in Global.AllSites)
            {
                int numRequired = site.GetNumEFilersRequiredOnDate(Global.SelectedDate);
                List<C_WorkItem> workItemsForSiteOnDate = site.GetWorkItemsOnDate(Global.SelectedDate);

                if (numRequired > workItemsForSiteOnDate.Count)
                    openSitesThatNeedHelp.Add(site);
			}

			Global.OpenSitesThatNeedHelp = openSitesThatNeedHelp; // we save this so Maps doesn't have to recompute

            TV_Sites.Source = new C_SitesTableSourceX(Global, this, openSitesThatNeedHelp, WorkItemsOnDateAllSites);
            TV_Sites.ReloadData();
		}

		public class C_SitesTableSourceX : UITableViewSource
		{
            readonly C_Global Global;
            readonly UIViewController ourVC;
			readonly List<C_VitaSite> Sites;
            readonly List<C_WorkItem> SignUpsOnDate;

            const string CellIdentifier = "TableCell";

            public C_SitesTableSourceX(C_Global pac, UIViewController vc, List<C_VitaSite> sites, List<C_WorkItem> signUpsOnDate)
			{
				Global = pac;
				ourVC = vc;
				Sites = sites;
                SignUpsOnDate = signUpsOnDate;
			}

			public override nint RowsInSection(UITableView tableview, nint section)
			{
				int count = 0;
				if (Sites != null)
					count = Sites.Count;
				return count;
			}

			public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
			{
				UITableViewCell cell = tableView.DequeueReusableCell(CellIdentifier);
				//---- if there are no cells to reuse, create a new one ---
				if (cell == null)
					cell = new UITableViewCell(UITableViewCellStyle.Subtitle, CellIdentifier);

				C_VitaSite site = Sites[indexPath.Row];

				cell.TextLabel.Text = site.Name;
                int dayOfWeek = (int)Global.SelectedDate.DayOfWeek;
                int numEF = site.GetNumEFilersRequiredOnDate(Global.SelectedDate);
				cell.DetailTextLabel.Text = numEF.ToString() + " needed.";

                switch (site.Status)
                {
                    case E_SiteStatus.Accepting:
                        cell.ImageView.Image = UIImage.FromBundle("greenstatus.jpg");
                        break;
                    case E_SiteStatus.NearLimit:
                        cell.ImageView.Image = UIImage.FromBundle("yellowstatus.jpg");
                        break;
                    case E_SiteStatus.NotAccepting:
                        cell.ImageView.Image = UIImage.FromBundle("redstatus.jpg");
                        break;
                    case E_SiteStatus.Closed:
                        cell.ImageView.Image = UIImage.FromBundle("blackstatus.jpg");
                        break;
                }

                return cell;
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{
                // Required for VC_SignUp
                // SelectedDate - came from Calendar
				Global.SelectedSite = Sites[indexPath.Row];
                Global.WorkItemsOnSiteOnDate = Global.SelectedSite.GetWorkItemsOnDate(Global.SelectedDate);

				Global.DetailsCameFrom = E_CameFrom.List;

				ourVC.PerformSegue("Segue_SitesOnDateListToSignUp", ourVC);
			}
		}
    }
}