using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Gms.Common;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using System.Collections.Generic;
using System.Linq;

namespace mapTestDemo
{
	[Activity (Label = "mapTestDemo", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity,IOnMapReadyCallback, Android.Gms.Maps.GoogleMap.IInfoWindowAdapter
	{
		int count = 1;
		public static readonly int InstallGooglePlayServicesId = 1000;
		private bool _isGooglePlayServicesInstalled;

		private MapFragment _mapFragment;

		public GoogleMap map { get; private set; }

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			if (isGPSEnabled ()) {

				_isGooglePlayServicesInstalled = TestIfGooglePlayServicesIsInstalled ();

				if (_isGooglePlayServicesInstalled) {
					//since we use a test key the map won't work on all test devices. Only at Kapil his device.
					//This will work when we move the android version to the real parse.com db.
					InitMapFragment ();
				} else {
					AlertDialog.Builder dialog = new AlertDialog.Builder (this, AlertDialog.ThemeHoloLight);
					dialog.SetTitle ("map Error");
					dialog.SetMessage ("Map error message");
					dialog.SetCancelable (false);
					dialog.SetPositiveButton ("ok", delegate {
						return;
					});
					dialog.SetNegativeButton ("cancel", delegate {
						return;
					});
					dialog.Show ();
				}
			}
		}

		public bool isGPSEnabled(){
			LocationManager LocMgr = Android.App.Application.Context.GetSystemService (Context.LocationService) as LocationManager;
			bool enabled = LocMgr.IsProviderEnabled (LocationManager.GpsProvider);

			if (!enabled) {
				AlertDialog.Builder dialog = new AlertDialog.Builder(this, AlertDialog.ThemeHoloLight);
				dialog.SetTitle ("GPS Alert");
				dialog.SetMessage ("Your GPS is OFF Mode");
				dialog.SetCancelable (false);
				dialog.SetPositiveButton ("GOTO GPS Settting", delegate {
					Intent intent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
					StartActivity(intent);
				});
				dialog.SetNegativeButton ("Cancel", delegate { return; });
				dialog.Show ();
			}

			return enabled;
		}

		private bool TestIfGooglePlayServicesIsInstalled ()
		{
			try {
				int queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable (this);
				if (queryResult == ConnectionResult.Success) {
					Console.WriteLine ("Google Play Services is installed on this device.");
					return true;
				}

				if (GoogleApiAvailability.Instance.IsUserResolvableError (queryResult)) {
					string errorString = GoogleApiAvailability.Instance.GetErrorString (queryResult);
					Console.WriteLine ("There is a problem with Google Play Services on this device: {0} - {1}", queryResult, errorString);
					Dialog errorDialog = GoogleApiAvailability.Instance.GetErrorDialog (this, queryResult, InstallGooglePlayServicesId);
					errorDialog.Show ();
				}
			} catch (Exception ex) {
				Console.WriteLine ("Test for google play services {0}",ex.ToString());
			}
			return false;
		}

		private void InitMapFragment()
		{
			_mapFragment = FragmentManager.FindFragmentByTag("map") as MapFragment;

			if (_mapFragment == null)
			{
				GoogleMapOptions mapOptions = new GoogleMapOptions()
					.InvokeMapType(GoogleMap.MapTypeNormal)
					.InvokeZoomControlsEnabled(true).InvokeScrollGesturesEnabled(true)
					.InvokeCompassEnabled(true);

				FragmentTransaction fragTx = FragmentManager.BeginTransaction();
				_mapFragment = MapFragment.NewInstance(mapOptions);
				fragTx.Add(Resource.Id.map, _mapFragment, "map");
				fragTx.Commit();
			}

			_mapFragment.GetMapAsync (this);

		}

		public void OnMapReady (GoogleMap googleMap)
		{
			map = googleMap;
			map.MyLocationEnabled = true;
			map.MyLocationChange += Map_MyLocationChange;
		}

		void Map_MyLocationChange (object sender, GoogleMap.MyLocationChangeEventArgs e)
		{
			map.MyLocationChange -= Map_MyLocationChange;
			LatLng latLong =new LatLng(e.Location.Latitude,e.Location.Longitude);
			CameraPosition.Builder builder = CameraPosition.InvokeBuilder();

			builder.Target(latLong);
			builder.Zoom(10);
			builder.Bearing(155);
			builder.Tilt(65);
			CameraPosition cameraPosition = builder.Build();
			MarkerOptions markerOpt = new MarkerOptions ();
			markerOpt.SetPosition (latLong);
			markerOpt.SetIcon (BitmapDescriptorFactory.FromResource (Resource.Drawable.Map_icon));
			map.AddMarker (markerOpt);
			map.SetInfoWindowAdapter (this);
			map.AnimateCamera(CameraUpdateFactory.NewCameraPosition(cameraPosition));
		}

		public View GetInfoContents (Marker marker){
			var inflater = Application.Context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;

			View v = inflater.Inflate(Resource.Layout.MapInfoView, null);

			Location locationA = new Location("point A");
			locationA.Latitude = -36.722195;
			locationA.Longitude = 174.706166;

			Location locationB = new Location("point B");
			locationB.Latitude = marker.Position.Latitude;
			locationB.Longitude = marker.Position.Longitude;

			float distance = locationA.DistanceTo(locationB) / 1000;

			TextView title = (TextView) v.FindViewById(Resource.Id.Phone);

			DateTime now = DateTime.Now.ToLocalTime();
			if (DateTime.Now.IsDaylightSavingTime () == true) {
				now = now.AddHours (1);
			}
			var utc = DateTime.UtcNow;

			title.Text = "lat:" + marker.Position.Latitude + " long:" + marker.Position.Longitude;

			TextView description = (TextView) v.FindViewById(Resource.Id.Web);
			description.Text = "Local Time:"+now+ "\nUTC Time:"+utc+"\nDistance:" + distance;

			return v;	
		}

		public View GetInfoWindow (Marker marker){
			return null;
		}
	}
}
	