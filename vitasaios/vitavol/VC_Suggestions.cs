using Foundation;
using System;
using UIKit;
using System.Collections.Generic;
using System.Threading.Tasks;
using zsquared;

namespace vitavol
{
    public partial class VC_Suggestions : UIViewController
    {
        C_Global Global;
        C_VitaUser LoggedInUser;

        public VC_Suggestions (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			AppDelegate myAppDelegate = (AppDelegate)UIApplication.SharedApplication.Delegate;
            Global = myAppDelegate.Global;

            LoggedInUser = Global.GetUserFromCacheNoFetch(Global.LoggedInUserId);

            // set the standard background color
            View.BackgroundColor = C_Common.StandardBackground;

			// --------- button handlers -----------

			B_Back.TouchUpInside += (sender, e) => 
                PerformSegue("Segue_SuggestionsToVolunteerOptions", this);

            B_NewSuggestion.TouchUpInside += OnNewSuggestion;

			// since the login process provides us this users suggestions, we don't need to pull anything additional

            C_SuggestionsTableSource ts = new C_SuggestionsTableSource(Global, this, LoggedInUser.Suggestions);
			TV_ListOfSuggestions.Source = ts;
            TV_ListOfSuggestions.Delegate = new C_SuggestionsTableDelegate(Global, this, LoggedInUser.Suggestions, ts);
			TV_ListOfSuggestions.ReloadData();
		}

		public void OnNewSuggestion(object s, EventArgs e)
		{
			// create the new suggestion
            C_Suggestion sug = new C_Suggestion(LoggedInUser.id, C_YMD.Now, false)
			{
				Status = E_SuggestionStatus.Open,
				Subject = "",
				Text = "",
				dirty = true
			};
			Global.SelectedSuggestion = sug;

			// and move to the editor
			PerformSegue("Segue_SuggestionsToSuggestion", this);
		}

        public void EnableUI(bool en)
        {
            B_Back.Enabled = en;
            TV_ListOfSuggestions.UserInteractionEnabled = en;
            B_NewSuggestion.Enabled = en;
        }

		public class C_SuggestionsTableDelegate : UITableViewDelegate
		{
			readonly C_Global Global;
            readonly C_VitaUser LoggedInUser;
			readonly VC_Suggestions OurVC;
			readonly List<C_Suggestion> Suggestions;
			C_SuggestionsTableSource TableSource;

            public C_SuggestionsTableDelegate(C_Global global, VC_Suggestions vc, List<C_Suggestion> suggestions, C_SuggestionsTableSource tsource)
			{
				Global = global;
				OurVC = vc;
                Suggestions = suggestions;
				TableSource = tsource;

                LoggedInUser = Global.GetUserFromCacheNoFetch(Global.LoggedInUserId);
			}

			public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
			{
				UITableViewRowAction hiButton = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, "Remove",
                async delegate
                {
                    OurVC.AI_Busy.StartAnimating();
                    OurVC.EnableUI(false);

                    C_Suggestion suggestionToRemove = Suggestions[indexPath.Row];

                    bool success = await suggestionToRemove.RemoveSuggestion(LoggedInUser.Token);
                    LoggedInUser.Suggestions.Remove(suggestionToRemove);
                    //bool success = await Global.LoggedInUser.RemoveSuggestion(suggestionToRemove);

					UIApplication.SharedApplication.InvokeOnMainThread(
                    new Action(() =>
                    {
                        if (!success)
                        {
                            C_MessageBox.MessageBox(OurVC, 
                                            "Error",
                                            "Unable to remove the suggestion",
                                             C_MessageBox.E_MessageBoxButtons.Ok);
                        }

                        OurVC.TV_ListOfSuggestions.ReloadData();
                        OurVC.AI_Busy.StopAnimating();
                        OurVC.EnableUI(true);
                    }));

				});

				return new UITableViewRowAction[] { hiButton };
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{
				// identify the specific suggestion
                Global.SelectedSuggestion = Suggestions[indexPath.Row];

				OurVC.PerformSegue("Segue_SuggestionsToSuggestion", OurVC);
			}
		}

		public class C_SuggestionsTableSource : UITableViewSource
		{
            readonly C_Global Global;
            readonly UIViewController ourVC;
            readonly List<C_Suggestion> Suggestions;

            const string CellIdentifier = "TableCell";

			public C_SuggestionsTableSource(C_Global pac, UIViewController vc, List<C_Suggestion> suggestions)
			{
				Global = pac;
				Suggestions = suggestions;
				ourVC = vc;
			}

			public override nint RowsInSection(UITableView tableview, nint section)
			{
				int count = 0;
				if (Suggestions != null)
					count = Suggestions.Count;
				return count;
			}

			public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
			{
				UITableViewCell cell = tableView.DequeueReusableCell(CellIdentifier);
				//---- if there are no cells to reuse, create a new one
				if (cell == null)
					cell = new UITableViewCell(UITableViewCellStyle.Subtitle, CellIdentifier);

				C_Suggestion suggestion = Suggestions[indexPath.Row];

                string subject = suggestion.Subject ?? "<null>";
                string text = suggestion.Text ?? "<null>";

				cell.TextLabel.Text = subject;
                cell.DetailTextLabel.Text = suggestion.CreateDate.ToString("mmm dd,yyyy") + ":" + text;

				return cell;
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{
				Global.SelectedSuggestion = Suggestions[indexPath.Row];
                Global.ViewCameFrom = E_ViewCameFrom.Suggestions;
				ourVC.PerformSegue("Segue_SuggestionsToSuggestion", ourVC);
			}
		}
    }
}