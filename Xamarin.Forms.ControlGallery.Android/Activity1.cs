﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Java.Interop;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Android;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.AppLinks;
using Droid = Android;
using Resource = Xamarin.Forms.Platform.Android.Resource;

[assembly: Dependency(typeof(CacheService))]
[assembly: Dependency(typeof(TestCloudService))]
[assembly: Dependency(typeof(StringProvider))]
[assembly: ExportRenderer(typeof(DisposePage), typeof(DisposePageRenderer))]
[assembly: ExportRenderer(typeof(DisposeLabel), typeof(DisposeLabelRenderer))]
[assembly: ExportRenderer(typeof(CustomButton), typeof(CustomButtonRenderer))]
[assembly: ExportEffect(typeof(BorderEffect), "BorderEffect")]

namespace Xamarin.Forms.ControlGallery.Android
{
	public class BorderEffect : PlatformEffect
	{
		protected override void OnAttached()
		{
			Control.SetBackgroundColor(global::Android.Graphics.Color.Aqua);
		}

		protected override void OnDetached()
		{
		}

		protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
		{
			base.OnElementPropertyChanged(args);
		}
	}

	public class CacheService : ICacheService
	{
		public void ClearImageCache()
		{
			DeleteFilesInDirectory("ImageLoaderCache");
		}

		static void DeleteFilesInDirectory(string directory)
		{
			using (IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
			{
				if (isolatedStorage.DirectoryExists(directory))
				{
					string[] files = isolatedStorage.GetFileNames(Path.Combine(directory, "*"));
					foreach (string file in files)
					{
						isolatedStorage.DeleteFile(Path.Combine(directory, file));
					}
				}
			}
		}
	}

	public class DisposePageRenderer : PageRenderer
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				((DisposePage)Element).SendRendererDisposed();
			}
			base.Dispose(disposing);
		}
	}

	public class DisposeLabelRenderer : LabelRenderer
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				((DisposeLabel)Element).SendRendererDisposed();
			}
			base.Dispose(disposing);
		}
	}

	public class StringProvider : IStringProvider
	{
		public string CoreGalleryTitle
		{
			get { return "Android CoreGallery"; }
		}
	}

	public class TestCloudService : ITestCloudService
	{
		public string GetTestCloudDevice()
		{
			return System.Environment.GetEnvironmentVariable("XTC_DEVICE");
		}

		public string GetTestCloudDeviceName()
		{
			return System.Environment.GetEnvironmentVariable("XTC_DEVICE_NAME");
		}

		public bool IsOnTestCloud()
		{
			string isInTestCloud = System.Environment.GetEnvironmentVariable("XAMARIN_TEST_CLOUD");

			return isInTestCloud != null && isInTestCloud.Equals("1");
		}
	}

#if PRE_APPLICATION_CLASS
	[Activity (Label = "Control Gallery", 
			   Icon = "@drawable/icon",
			   MainLauncher = true, 
			   HardwareAccelerated = true, 
			   ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class Activity1 : AndroidActivity
	{


		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			
			Forms.Init (this, bundle);
			FormsMaps.Init (this, bundle);

			SetPage (FormsApp.GetFormsApp ());

			MessagingCenter.Subscribe<RootPagesGallery, Type> (this, Messages.ChangeRoot, (sender, pageType) => {
				var page = ((Page)Activator.CreateInstance (pageType));
				SetPage (page);
			});

			MessagingCenter.Subscribe<RootPagesGallery, Type> (this, Messages.ChangeRoot, (sender, pageType) => {
				var page = ((Page)Activator.CreateInstance (pageType));
				SetPage (page);
			});

			MessagingCenter.Subscribe<HomeButton> (this, Messages.GoHome, (sender) => {
				var screen = FormsApp.GetFormsApp ();
				SetPage (screen);
 			});
		}
		
		public override void OnConfigurationChanged (global::Android.Content.Res.Configuration newConfig)
		{
			// we're good
			base.OnConfigurationChanged (newConfig);
		}
		
		protected override void OnDestroy ()
		{
			base.OnDestroy ();
		}
	}

#elif FORMS_APPLICATION_ACTIVITY
	[Activity(Label = "Control Gallery",
		   Icon = "@drawable/icon",
		//   Theme="@style/TestStyle",
		   MainLauncher = true,
		   HardwareAccelerated = true,
		   ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]

	public class Activity1 : FormsApplicationActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			//ToolbarResource = Resource.Layout.Toolbar;
			//TabLayoutResource = Resource.Layout.Tabbar;

			base.OnCreate(bundle);

			if (!Debugger.IsAttached)
				Insights.Initialize(App.Config["InsightsApiKey"], this.ApplicationContext);

			Forms.Init(this, bundle);
			FormsMaps.Init(this, bundle);
			Xamarin.Forms.Forms.ViewInitialized += (object sender, Xamarin.Forms.ViewInitializedEventArgs e) => {
				if (!string.IsNullOrWhiteSpace(e.View.StyleId))
				{
					e.NativeView.ContentDescription = e.View.StyleId;
				}
			};
			// uncomment to verify turning off title bar works. This is not intended to be dynamic really.
			//Forms.SetTitleBarVisibility (AndroidTitleBarVisibility.Never);

			var app = new App ();

			// When the native control gallery loads up, it'll let us know so we can add the nested native controls
			MessagingCenter.Subscribe<NestedNativeControlGalleryPage>(this, NestedNativeControlGalleryPage.ReadyForNativeControlsMessage, AddNativeControls);

			LoadApplication (app);
		}

		private void AddNativeControls (NestedNativeControlGalleryPage page)
		{
			if (page.NativeControlsAdded) {
				return;
			}

			StackLayout sl = page.Layout;

			// Create and add a native TextView
			var textView = new TextView (this) { Text = "I am a native TextView", TextSize = 14 };
			sl?.Children.Add (textView);

			// Create and add a native Button 
			var button = new global::Android.Widget.Button (this) { Text = "Click to change TextView font size" };
			float originalSize = textView.TextSize;
			button.Click += (sender, args) => { textView.TextSize = textView.TextSize == originalSize ? 24 : 14; };

			sl?.Children.Add (button.ToView ());

			// Create a control which we know doesn't behave correctly with regard to measurement
			var difficultControl0 = new BrokenNativeControl (this) {
				Text = "This native control doesn't play nice with sizing, which is why it's all squished to one side."
			};
			var difficultControl1 = new BrokenNativeControl (this) {
				Text = "Same control, but with a custom GetDesiredSize delegate to accomodate it's sizing problems."
			};

			// Add a misbehaving control 
			sl?.Children.Add (difficultControl0);

			// Add a misbehaving control with a custom delegate for GetDesiredSize
			sl?.Children.Add (difficultControl1, SizeBrokenControl);

			page.NativeControlsAdded = true;
		}

		private static SizeRequest? SizeBrokenControl (NativeViewWrapperRenderer renderer,
			int widthConstraint, int heightConstraint)
		{
			global::Android.Views.View nativeView = renderer.Control;

			if ((widthConstraint == 0 && heightConstraint == 0) || nativeView == null) {
				return null;
			}

			int width = global::Android.Views.View.MeasureSpec.GetSize (widthConstraint);
			int widthSpec = global::Android.Views.View.MeasureSpec.MakeMeasureSpec (width * 2,
				global::Android.Views.View.MeasureSpec.GetMode (widthConstraint));
			nativeView.Measure (widthSpec, heightConstraint);
			var size = new Size (nativeView.MeasuredWidth, nativeView.MeasuredHeight);
			return new SizeRequest (size);
		}

		public override void OnConfigurationChanged(global::Android.Content.Res.Configuration newConfig)
		{
			// we're good
			base.OnConfigurationChanged(newConfig);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
		}

		[Export("NavigateToTest")]
		public bool NavigateToTest(string test)
		{
			return _app.NavigateToTestPage(test);
		}
	}
#else

	[Activity(Label = "Control Gallery",
		Icon = "@drawable/icon",
		Theme = "@style/MyTheme",
		MainLauncher = true,
		HardwareAccelerated = true,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	[IntentFilter(new[] { Intent.ActionView },
			Categories = new[]
			{
				Intent.ActionView,
				Intent.CategoryDefault,
				Intent.CategoryBrowsable
			}
			,
			DataScheme = "http",
			DataHost = App.AppName,
			DataPathPrefix = "/gallery/"
		)
	]
	public class Activity1 : FormsAppCompatActivity
	{
		App _app;

		[Export("NavigateToTest")]
		public bool NavigateToTest(string test)
		{
			return _app.NavigateToTestPage(test);
		}

		public override void OnConfigurationChanged(Droid.Content.Res.Configuration newConfig)
		{
			// we're good
			base.OnConfigurationChanged(newConfig);z
		}

		[Export("Reset")]
		public void Reset()
		{
			_app.Reset();
		}

		protected override void OnCreate(Bundle bundle)
		{
			ToolbarResource = Resource.Layout.Toolbar;
			TabLayoutResource = Resource.Layout.Tabbar;

			base.OnCreate(bundle);

			if (!Debugger.IsAttached)
				Insights.Initialize(App.InsightsApiKey, ApplicationContext);

			Forms.Init(this, bundle);
			FormsMaps.Init(this, bundle);
			AndroidAppLinks.Init(this);
			Forms.ViewInitialized += (sender, e) =>
			{
				//				if (!string.IsNullOrWhiteSpace(e.View.StyleId)) {
				//					e.NativeView.ContentDescription = e.View.StyleId;
				//				}
			};
			// uncomment to verify turning off title bar works. This is not intended to be dynamic really.
			//Forms.SetTitleBarVisibility (AndroidTitleBarVisibility.Never);

			var app = new App();
			_app = app;

			// When the native control gallery loads up, it'll let us know so we can add the nested native controls
			MessagingCenter.Subscribe<NestedNativeControlGalleryPage>(this,
				NestedNativeControlGalleryPage.ReadyForNativeControlsMessage, AddNativeControls);

			// When the native binding gallery loads up, it'll let us know so we can set up the native bindings
			MessagingCenter.Subscribe<NativeBindingGalleryPage>(this,
				NativeBindingGalleryPage.ReadyForNativeBindingsMessage, AddNativeBindings);

			LoadApplication(app);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
		}

		void AddNativeBindings(NativeBindingGalleryPage page)
		{
			if (page.NativeControlsAdded)
				return;

			StackLayout sl = page.Layout;

			var textView = new TextView(this)
			{
				TextSize = 14,
				Text = "This will be text"
			};

			var viewGroup = new LinearLayout(this);
			viewGroup.AddView(textView);

			var buttonColor = new Droid.Widget.Button(this) { Text = "Change label Color" };
			buttonColor.Click += (sender, e) => textView.SetTextColor(Color.Blue.ToAndroid());

			var colorPicker = new ColorPickerView(this, 200, 200);

			textView.SetBinding(nameof(textView.Text), new Binding("NativeLabel"));
			//this doesn't work because there's not TextColor property
			//textView.SetBinding("TextColor", new Binding("NativeLabelColor", converter: new ColorConverter()));
			colorPicker.SetBinding(nameof(colorPicker.SelectedColor),
				new Binding("NativeLabelColor", BindingMode.TwoWay, new ColorConverter()), "ColorPicked");

			sl?.Children.Add(viewGroup);
			sl?.Children.Add(buttonColor.ToView());
			sl?.Children.Add(colorPicker);

			page.NativeControlsAdded = true;
		}

		void AddNativeControls(NestedNativeControlGalleryPage page)
		{
			if (page.NativeControlsAdded)
			{
				return;
			}

			StackLayout sl = page.Layout;

			// Create and add a native TextView
			var textView = new TextView(this) { Text = "I am a native TextView", TextSize = 14 };
			sl?.Children.Add(textView);

			// Create and add a native Button 
			var button = new Droid.Widget.Button(this) { Text = "Click to change TextView font size" };
			float originalSize = textView.TextSize;
			button.Click += (sender, args) => { textView.TextSize = textView.TextSize == originalSize ? 24 : 14; };

			sl?.Children.Add(button.ToView());

			// Create a control which we know doesn't behave correctly with regard to measurement
			var difficultControl0 = new BrokenNativeControl(this)
			{
				Text = "This native control doesn't play nice with sizing, which is why it's all squished to one side."
			};
			var difficultControl1 = new BrokenNativeControl(this)
			{
				Text = "Same control, but with a custom GetDesiredSize delegate to accomodate it's sizing problems."
			};

			// Add a misbehaving control 
			sl?.Children.Add(difficultControl0);

			// Add a misbehaving control with a custom delegate for GetDesiredSize
			sl?.Children.Add(difficultControl1, SizeBrokenControl);

			page.NativeControlsAdded = true;
		}

		static SizeRequest? SizeBrokenControl(NativeViewWrapperRenderer renderer,
			int widthConstraint, int heightConstraint)
		{
			Droid.Views.View nativeView = renderer.Control;

			if (widthConstraint == 0 && heightConstraint == 0 || nativeView == null)
			{
				return null;
			}

			int width = Droid.Views.View.MeasureSpec.GetSize(widthConstraint);
			int widthSpec = Droid.Views.View.MeasureSpec.MakeMeasureSpec(width * 2,
				Droid.Views.View.MeasureSpec.GetMode(widthConstraint));
			nativeView.Measure(widthSpec, heightConstraint);
			var size = new Size(nativeView.MeasuredWidth, nativeView.MeasuredHeight);
			return new SizeRequest(size);
		}

		public class ColorConverter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				if (value is Color)
					return ((Color)value).ToAndroid();

				return null;
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				if (value is Droid.Graphics.Color)
					return ((Droid.Graphics.Color)value).ToColor();

				return null;
			}
		}
	}
#endif
}