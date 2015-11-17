using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 빈 페이지 항목 템플릿은 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 에 문서화되어 있습니다.

namespace AniSched
{
    public sealed partial class MainPage : Page
    {
        Boolean netbl = false;
        Boolean colorB;


        public MainPage()
        {
            this.InitializeComponent();
        }

        private void ColorPick()
        {
            var MAColor = Windows.UI.Color.FromArgb(255, 254, 59, 114);
            var MBColor = Windows.UI.Color.FromArgb(255, 250, 250, 250);
            var MCColor = Windows.UI.Color.FromArgb(255, 25, 25, 25);

            if (colorB == true)
            {
                MAColor = Windows.UI.Color.FromArgb(255, 254, 59, 114);
                MBColor = Windows.UI.Color.FromArgb(255, 25, 25, 25);
                MCColor = Windows.UI.Color.FromArgb(255, 186, 186, 186);

                SolidColorBrush MSHA = new SolidColorBrush(MAColor);
                SolidColorBrush MSHB = new SolidColorBrush(MBColor);

                appbar_main.Background = MSHB;
                appbar_main.Foreground = MSHA;
            }

            ApplicationView AppVCon = ApplicationView.GetForCurrentView();

            AppVCon.TitleBar.BackgroundColor = MAColor;
            AppVCon.TitleBar.ButtonBackgroundColor = MAColor;
            AppVCon.TitleBar.ButtonHoverBackgroundColor = MAColor;
            AppVCon.TitleBar.ButtonInactiveBackgroundColor = MAColor;
            AppVCon.TitleBar.ButtonPressedBackgroundColor = MBColor;
            AppVCon.TitleBar.InactiveBackgroundColor = MAColor;
            AppVCon.TitleBar.ForegroundColor = MBColor;
            AppVCon.TitleBar.ButtonForegroundColor = MBColor;
            AppVCon.TitleBar.ButtonHoverForegroundColor = MCColor;
            AppVCon.TitleBar.ButtonInactiveForegroundColor = MBColor;
            AppVCon.TitleBar.ButtonPressedForegroundColor = MCColor;
            AppVCon.TitleBar.InactiveForegroundColor = MBColor;
        }

        private void page_main_Loading(FrameworkElement sender, object args)
        {
            var Time_Now = DateTime.Now.Hour;
            if (Time_Now >= 6)
            {
                if (Time_Now <= 17)
                {
                    page_main.RequestedTheme = ElementTheme.Light;
                    grid_main.RequestedTheme = ElementTheme.Light;
                    webview_day.Visibility = Visibility.Visible;
                    colorB = false;
                    ColorPick();
                }
                else
                {
                    page_main.RequestedTheme = ElementTheme.Dark;
                    grid_main.RequestedTheme = ElementTheme.Dark;
                    webview_night.Visibility = Visibility.Visible;
                    colorB = true;
                    ColorPick();
                }
            }
            else
            {
                page_main.RequestedTheme = ElementTheme.Dark;
                grid_main.RequestedTheme = ElementTheme.Dark;
                webview_night.Visibility = Visibility.Visible;
                colorB = true;
                ColorPick();
            }
        }
        
        private void runfail()
        {
            fail_txt.Visibility = Visibility.Visible;
            webview_day.Visibility = Visibility.Collapsed;
            webview_night.Visibility = Visibility.Collapsed;
        }

        private void webview_day_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            if (netbl == false)
            {
                webview_day.Refresh();
                netbl = true;
            }
            runfail();
        }

        private void webview_night_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            if (netbl == false)
            {
                webview_night.Refresh();
                netbl = true;
            }
            runfail();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            webview_night.Refresh();
            webview_day.Refresh();
        }
    }
}
