﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using zsquared;

namespace a_vitavol
{
    [Activity(Label = "VITA: Volunteer Options")]
    public class A_VolunteerActivity : Activity
    {
        C_Global Global;

        List<C_SignUp> OurComingWorkItems;

		Button B_ViewTodaySignups;
        Button B_CreateNewSignup;
        Button B_PostSuggestion;
        Button B_Suggestions;
		Button B_UpdateProfile;
        TextView L_TodaySignups;
        Button B_Resubmit;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

			MyAppDelegate g = (MyAppDelegate)Application;
			if (g.Global == null)
				g.Global = new C_Global();
			Global = g.Global;

			SetContentView(Resource.Layout.VolunteerOptions);

            B_ViewTodaySignups = FindViewById<Button>(Resource.Id.B_ViewTodaySignups);
            B_CreateNewSignup = FindViewById<Button>(Resource.Id.B_CreateNewSignup);
            B_PostSuggestion = FindViewById<Button>(Resource.Id.B_PostSuggestion);
            B_Suggestions = FindViewById<Button>(Resource.Id.B_Suggestions);
			B_UpdateProfile = FindViewById<Button>(Resource.Id.B_UpdateProfile);
			L_TodaySignups = FindViewById<TextView>(Resource.Id.L_SignsUpToday);
            B_Resubmit = FindViewById<Button>(Resource.Id.B_Resubmit);

			// get all workintents for this user
			List<C_SignUp> OurWorkItems = Global.GetSignUpsForUser(Global.LoggedInUserId);

			// make sure we only look at the current items (today and beyond)
			C_YMD today = C_YMD.Now;
			var ou = OurWorkItems.Where(wi => wi.Date >= today);
            OurComingWorkItems = ou.ToList();

            L_TodaySignups.Text = "You have " + OurComingWorkItems.Count.ToString() + " signups.";

			B_Resubmit.Click += async (sender, e) =>
			{
				var sharedPreferences = GetSharedPreferences("vitasa", FileCreationMode.MultiProcess);
				string token = sharedPreferences.GetString("firebasetoken", "");

                if (!string.IsNullOrEmpty(token))
                {
                    C_VitaUser user = Global.GetUserFromCacheNoFetch(Global.LoggedInUserId);

                    if (user != null)
                    {
                        
                        bool success = await C_Notifications.RegisterNotificationToken(C_Notifications.E_Platform.Android, token, user.Token);

                        if (success)
                        {
                            C_MessageBox mbox = new C_MessageBox(this, "Success", "Token was successfully registered.", E_MessageBoxButtons.Ok);
                            mbox.Show();
                        }
                        else
                        {
							C_MessageBox mbox = new C_MessageBox(this, "Error", "Failed to successfully register the Token.", E_MessageBoxButtons.Ok);
							mbox.Show();
						}
                    }
                }
			};

			B_ViewTodaySignups.Click += (sender, e) => 
            {
                Intent i = new Intent(this, typeof(A_MySignUps));
				StartActivity(i);
			};

            B_Suggestions.Click += (sender, e) => 
            {
                Intent i = new Intent(this, typeof(A_Suggestions));
				StartActivity(i);
			};

            B_UpdateProfile.Click += (sender, e) =>
			{
                Global.ViewCameFrom = E_ViewCameFrom.VolOptions;
				StartActivity(new Intent(this, typeof(A_Profile)));
			};

			B_PostSuggestion.Click += (sender, e) => 
            {
                C_Suggestion sug = new C_Suggestion(-1, C_YMD.Now, false);
                sug.Subject = "";
                sug.Text = "";
                sug.dirty = true;

                Global.SelectedSuggestion = sug;

                Intent i = new Intent(this, typeof(A_Suggestion));
				StartActivity(i);
			};

            B_CreateNewSignup.Click += (sender, e) => 
            {
                Intent i = new Intent(this, typeof(A_VolCalendar));
				StartActivity(i);
			};
		}

		public override void OnBackPressed()
		{
            Intent i = new Intent(this, typeof(MainActivity));
			StartActivity(i);
		}
	}
}
