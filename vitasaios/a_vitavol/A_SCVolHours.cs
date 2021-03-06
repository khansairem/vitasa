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
    [Activity(Label = "VITA: Volunteer Hours")]
    public class A_SCVolHours : Activity
    {
		C_Global Global;

		ProgressDialog AI_Busy;

        TextView L_Date;
        TextView L_Site;
        TextView L_User;
        TextView L_Approval;
        TextView L_Phone;

        EditText TB_Hours;

        C_SignUp OurWorkItem;

		protected override void OnCreate(Bundle savedInstanceState)
        {
			base.OnCreate(savedInstanceState);

			MyAppDelegate g = (MyAppDelegate)Application;
			if (g.Global == null)
				g.Global = new C_Global();
			Global = g.Global;

			if (Global.SelectedDate == null)
				Global.SelectedDate = C_YMD.Now;

			// Set our view from the "main" layout resource
            SetContentView(Resource.Layout.SCVolHours);

			AI_Busy = new ProgressDialog(this);
			AI_Busy.SetMessage("Please wait...");
			AI_Busy.SetCancelable(false);
			AI_Busy.SetProgressStyle(ProgressDialogStyle.Spinner);

            OurWorkItem = Global.VolunteerSignUp;
            C_VitaUser OurUser = Global.GetUserFromCacheNoFetch(OurWorkItem.UserId);

            L_Date = FindViewById<TextView>(Resource.Id.L_Date);
            L_Site = FindViewById<TextView>(Resource.Id.L_Site);
            L_User = FindViewById<TextView>(Resource.Id.L_User);
            L_Approval = FindViewById<TextView>(Resource.Id.L_Approval);
			L_Phone = FindViewById<TextView>(Resource.Id.L_Phone);

			TB_Hours = FindViewById<EditText>(Resource.Id.TB_Hours);

			L_Date.Text = OurWorkItem.Date.ToString("mmm dd, yyyy");
			L_Site.Text = OurWorkItem.SiteName;
            L_User.Text = OurUser.Name;
            L_Phone.Text = OurUser.Phone;
            L_Approval.Text = OurWorkItem.Approved ? "Approved" : "Not Approved";

            TB_Hours.Text = OurWorkItem.Hours.ToString();
		}

		public override void OnBackPressed()
		{
            try { OurWorkItem.Hours = Convert.ToSingle(TB_Hours.Text); }
            catch {}                                           
            
            StartActivity(new Intent(this, typeof(A_SCSiteVol)));
		}
	}
}
