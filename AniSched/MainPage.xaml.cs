using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Phone.UI.Input;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


// 빈 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace AniSched
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        SolidColorBrush PINK = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 254, 59, 114));
        SolidColorBrush WHIT = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 250, 250, 250));

        HttpClient Request = new HttpClient();
        HttpResponseMessage Response;

        DispatcherTimer WhatTimeIs;

        MessageDialog DiagNoSubLink = new MessageDialog("링크가 등록되어 있지 않습니다.", "알림");

        int value = -1, value_t = -1, WhatDate, WhatMin;

        Boolean NoSub = false, IsEndAnime = false, Is404 = false, IsNoList = false, IsAllList = false, IsError = false;

        string Json_Data, Json_sub_Data, SearchString = "", Title;

        RootObject[] TABLE_DATA;
        RootObject_S[] SUB_DATA;

        public class RootObject
        {
            public int i { get; set; }      // ID
            public string c { get; set; }   // Color Tag
            public string s { get; set; }   // Title
            public string t { get; set; }   // Time
            public string g { get; set; }   // Genre
            public string l { get; set; }   // Site
            public string a { get; set; }   // Status
            public string sd { get; set; }  // Start
            public string ed { get; set; }  // End
        }

        public class RootObject_S
        {
            public string s { get; set; }   // Chapter
            public string d { get; set; }   // Date
            public string a { get; set; }   // Link
            public string n { get; set; }   // Start
        }

        public MainPage()
        {
            this.InitializeComponent();
            ColorPick();
            SetDest(DateTime.Now.DayOfWeek.ToString());
            value_t = value;
            RefreshWhat();
            RunClient(true, value);
            WhatTimeIs = new DispatcherTimer();
            WhatTimeIs.Tick += WhatTimeIs_Tick;
            WhatTimeIs.Interval = TimeSpan.FromMinutes(1);
            WhatTimeIs.Start();
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += OnHardwareButtonsBackPressed;
            }
        }

        private void WhatTimeIs_Tick(object sender, object e)
        {
            RefreshWhat();
        }

        private void RefreshWhat()
        {
            WhatDate = Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"));
            WhatMin = (Convert.ToInt32(DateTime.Now.ToString("HH")) * 60) + Convert.ToInt32(DateTime.Now.ToString("mm"));
        }

        private void TableView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (Is404 == false && IsNoList == false)
            {
                RootObject mk = e.ClickedItem as RootObject;
                Title_Image.Source = new BitmapImage(new Uri("http://anisched.moeru.ga/" + mk.i + ".jpg", UriKind.Absolute));
                Title = mk.s;
                SubView_T.Text = mk.a + (mk.a == "" ? ""  : " ") + mk.s;
                SubView_ID.Text = Convert.ToString(mk.i);
                SubView_L.Text = mk.l != "http://" ? mk.l : "링크 없음";
                SubView_S.Text = (mk.sd.Length == 8 ? "20" + mk.sd : mk.sd);
                SubView_E.Text = (mk.ed.Length == 8 ? "20" + mk.ed : mk.ed);
                RunClient(false, mk.i);
            }
            SearchList.IsEnabled = false;
        }

        private void Animation_Begin(object sender, RoutedEventArgs e)
        {
            var mk = sender as Grid;
            
            if (mk.Name == "SubView_Gridnamuwiki")
            {
                NamuWiki_Animation.Begin();
            }
            else if (mk.Name == "SubView_Gridnaver")
            {
                Naver_Animation.Begin();
            }
        }

        private void Animation_Stop(object sender, RoutedEventArgs e)
        {
            var mk = sender as Grid;
            if (mk.Name == "SubView_Gridnamuwiki")
            {
                NamuWiki_Animation.Stop();
            }
            else if (mk.Name == "SubView_Gridnaver")
            {
                Naver_Animation.Stop();
            }
        }

        private async void GoSearch(object sender, PointerRoutedEventArgs e)
        {
            var mk = sender as Grid;

            if (mk.Name == "SubView_Gridnamuwiki")
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("https://namu.wiki/go/" + Title));
            }
            else if (mk.Name == "SubView_Gridnaver")
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("https://search.naver.com/search.naver?&query=" + Title));
            }
        }
        
        private void Title_Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            Image_Animation.Begin();
        }

        private void Exit_Grid_C_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Image_Animation.Stop();
            ChildGrid.Visibility = Visibility.Collapsed;
            Grid_Animation.Stop();
            SearchList.IsEnabled = true;
        }
        
        private void SearchList_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            Debug.WriteLine(e.Key);
            SearchString = SearchList.Text;
            if (e.Key == VirtualKey.Enter && Is404 == false && IsNoList == false)
            {
                RunClient(true, value);
            }
        }

        private void SearchList_GotFocus(object sender, RoutedEventArgs e)
        {
            if (IsAllList == false)
            {
                IsAllList = true;
                resetBtn();
                RunClient(true, value);
            }
        }

        private async void SubView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (NoSub == false)
            {
                RootObject_S mk = e.ClickedItem as RootObject_S;

                try
                {
                    await Windows.System.Launcher.LaunchUriAsync(new Uri(mk.a));
                }
                catch (System.UriFormatException)
                {
                    await DiagNoSubLink.ShowAsync();
                }
            }
        }

        private void LIST_BTN_Click(object sender, RoutedEventArgs e)
        {
            IsEndAnime = IsEndAnime == true ? false : true;
            LIST_BTN.Content = IsEndAnime == true ? "종영 애니" : "방영 애니";

            RunClient(true, value);
        }

        private async void SubView_L_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            TextBlock mk = sender as TextBlock;
            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri(mk.Text));
            }
            catch (System.UriFormatException) { }
        }
        
        private void ColorPick()
        {
            var MAColor = Windows.UI.Color.FromArgb(255, 254, 59, 114);     // TH
            var MBColor = Windows.UI.Color.FromArgb(255, 250, 250, 250);    // FG
            var MCColor = Windows.UI.Color.FromArgb(255, 25, 25, 25);       // BG
            
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
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
                AppVCon.SetPreferredMinSize(new Size(480, 500));
            }

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                if (statusBar != null)
                {
                    statusBar.BackgroundOpacity = 1.0f;
                    statusBar.BackgroundColor = MAColor;
                    statusBar.ForegroundColor = MBColor;
                }
            }
        }

        private void OnHardwareButtonsBackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;

            if (ChildGrid.Visibility == Visibility.Visible)
            {
                Image_Animation.Stop();
                ChildGrid.Visibility = Visibility.Collapsed;
                Grid_Animation.Stop();
                SearchList.IsEnabled = true;
            }
            else
            {
                App.Current.Exit();
            }
        }

        private async Task<string> RequestPage(bool k, int i)
        {
            try
            {
                Response = await Request.GetAsync(new Uri("http://www.anissia.net/anitime/"
                    + (k == true ? (IsEndAnime == true ? "end?p=" : "list?w=") : "cap?i=")
                    + i));
            }
            catch (System.Net.Http.HttpRequestException)
            {
                return null;
            }
            return await Response.Content.ReadAsStringAsync();
        }

        private void resetBtn()
        {
            SUN_BTN.BorderThickness = new Thickness(0);
            MON_BTN.BorderThickness = new Thickness(0);
            TUE_BTN.BorderThickness = new Thickness(0);
            WED_BTN.BorderThickness = new Thickness(0);
            THU_BTN.BorderThickness = new Thickness(0);
            FRI_BTN.BorderThickness = new Thickness(0);
            SAT_BTN.BorderThickness = new Thickness(0);
            OVA_BTN.BorderThickness = new Thickness(0);
            NEW_BTN.BorderThickness = new Thickness(0);
        }

        //1
        private void BtnClick(object sender, RoutedEventArgs e)
        {
            IsAllList = false;
            resetBtn();
            Button mk = sender as Button;

            SetDest(mk.Name);
            RunClient(true, value);
        }
        
        //2
        private void SetDest(string s)
        {
            if (s == "SUN_BTN" || s == "Sunday")
            {
                value = 0; SUN_BTN.BorderThickness = new Thickness(0, 0, 0, 4);
            }
            else if (s == "MON_BTN" || s == "Monday")
            {
                value = 1; MON_BTN.BorderThickness = new Thickness(0, 0, 0, 4);
            }
            else if (s == "TUE_BTN" || s == "Tuesday")
            {
                value = 2; TUE_BTN.BorderThickness = new Thickness(0, 0, 0, 4);
            }
            else if (s == "WED_BTN" || s == "Wednesday")
            {
                value = 3; WED_BTN.BorderThickness = new Thickness(0, 0, 0, 4);
            }
            else if (s == "THU_BTN" || s == "Thursday")
            {
                value = 4; THU_BTN.BorderThickness = new Thickness(0, 0, 0, 4);
            }
            else if (s == "FRI_BTN" || s == "Friday")
            {
                value = 5; FRI_BTN.BorderThickness = new Thickness(0, 0, 0, 4);
            }
            else if (s == "SAT_BTN" || s == "Saturday")
            {
                value = 6; SAT_BTN.BorderThickness = new Thickness(0, 0, 0, 4);
            }
            else if (s == "OVA_BTN")
            {
                value = 7; OVA_BTN.BorderThickness = new Thickness(0, 0, 0, 4);
            }
            else if (s == "NEW_BTN")
            {
                value = 8; NEW_BTN.BorderThickness = new Thickness(0, 0, 0, 4);
            }
        }

        //3
        private async void RunClient(bool k, int i)
        {
            if (k == true)
            {
                if (IsAllList == true)
                {
                    for(int j = 0; j <= 8; j++)
                    {
                        Json_Data = Json_Data + await RequestPage(k, j);
                    }
                }
                else
                {
                    Json_Data = await RequestPage(k, i);
                }

                if (Json_Data != null)
                {
                    Is404 = false;
                    GetJson(true);
                    AlignString(true);
                }
                else
                {
                    Is404 = true;
                }
                DrawList(true);
                SearchList.IsEnabled = true;
            }
            else
            {
                Json_sub_Data = await RequestPage(k, i);

                GetJson(false);
                if (NoSub == false)
                {
                    AlignString(false);
                }
                DrawList(false);
                ChildGrid.Visibility = Visibility.Visible;
                Grid_Animation.Begin();
            }
        }

        //4
        // GET JSON DATA FROM HTTPCLIENT
        private void GetJson(bool m)
        {
            if (m == true)
            {
                Json_Data = ((Json_Data.Replace("[", "")).Replace("]", "")).Replace("},{", "}{");
                IsNoList = Json_Data == "" ? true : false;

                if (Is404 == false || IsNoList == false)
                {
                    ParseJson(true);
                }
            }
            else if (m == false)
            {
                Json_sub_Data = ((Json_sub_Data.Replace("[", "")).Replace("]", "")).Replace("},{", "}{");
                if (Json_sub_Data != "")
                {
                    NoSub = false;
                    ParseJson(false);
                }
                else
                {
                    NoSub = true;
                }
            }
        }

        //5
        //PARSE JSON DATA
        private void ParseJson(bool m)
        {
            if (m == true)
            {
                IList<RootObject> rootObjects = new List<RootObject>();
                JsonTextReader Reader = new JsonTextReader(new StringReader(Json_Data));
                Reader.SupportMultipleContent = true;

                try
                {
                    while (Reader.Read())
                    {
                        JsonSerializer Serializer = new JsonSerializer();
                        RootObject rootObject = Serializer.Deserialize<RootObject>(Reader);

                        rootObjects.Add(rootObject);
                    }
                    IsError = false;
                }
                catch (Newtonsoft.Json.JsonReaderException)
                {
                    IsError = true;
                }

                TABLE_DATA = new RootObject[rootObjects.Count];
                rootObjects.CopyTo(TABLE_DATA, 0);
            }
            else
            {
                IList<RootObject_S> rootObjects = new List<RootObject_S>();
                JsonTextReader Reader = new JsonTextReader(new StringReader(Json_sub_Data));
                Reader.SupportMultipleContent = true;

                while (Reader.Read())
                {
                    JsonSerializer Serializer = new JsonSerializer();
                    RootObject_S rootObject = Serializer.Deserialize<RootObject_S>(Reader);

                    rootObjects.Add(rootObject);
                }

                SUB_DATA = new RootObject_S[rootObjects.Count];
                rootObjects.CopyTo(SUB_DATA, 0);
            }
        }

        //6
        // ALIGN STRING
        private void AlignString(bool m)
        {
            if (m == true)
            {
                RefreshWhat();
                int tmp2, tmp3, hmp2;
                int i = TABLE_DATA.Count();
                for (int j = 0; j < i; j++)
                {
                    hmp2 = (Convert.ToInt32(TABLE_DATA[j].t.Substring(0, 2)) * 60) + Convert.ToInt32(TABLE_DATA[j].t.Substring(2, 2));
                    TABLE_DATA[j].s = ((TABLE_DATA[j].s.Replace("&lt;", "<")).Replace("&gt;", ">")).Replace("&amp;", "&");
                    TABLE_DATA[j].t = TABLE_DATA[j].t.Insert(2, ":");
                    TABLE_DATA[j].l = (TABLE_DATA[j].l.Trim()).Insert(0, "http://");
                    TABLE_DATA[j].sd = ((TABLE_DATA[j].sd.Insert(4, "/")).Insert(7, "/"));
                    TABLE_DATA[j].ed = ((TABLE_DATA[j].ed.Insert(4, "/")).Insert(7, "/"));
                    tmp2 = Convert.ToInt32(TABLE_DATA[j].ed.Replace("/", ""));
                    tmp3 = Convert.ToInt32(TABLE_DATA[j].sd.Replace("/", ""));
                    TABLE_DATA[j].sd = ((TABLE_DATA[j].sd.Replace("/99/99", "/##/##")).Replace("/99", "/#")).Replace("2099", "20##");
                    TABLE_DATA[j].ed = ((TABLE_DATA[j].ed.Replace("/99/99", "/##/##")).Replace("/99", "/#")).Replace("2099", "20##");

                    if (value == 7 || value == 8 || IsEndAnime == true || IsAllList == true)
                    {
                        TABLE_DATA[j].t = TABLE_DATA[j].sd;
                    }
                    TABLE_DATA[j].c = (TABLE_DATA[j].a == "true" ? (WhatDate >= tmp2 && tmp2 != 0 ? "#FF455A64" : (tmp3 > WhatDate && tmp3 != 0 ? "#FF00B0FF" : "#FF00E676")) : "#FFFF1744");

                    if(value == value_t && TABLE_DATA[j].c == "#FF00E676" && WhatMin >= hmp2 && WhatMin < hmp2 + 30)
                    {
                        TABLE_DATA[j].c = "#FFFFEA00";
                    }
                }
            }
            else if (m == false)
            {
                int i = SUB_DATA.Count();
                for (int j = 0; j < i; j++)
                {
                    if (SUB_DATA[j].s.Length == 5)
                    {
                        if (SUB_DATA[j].s == "00000")
                        {
                            SUB_DATA[j].s = "예정";
                        }
                        else
                        {
                            int NUM = 0;
                            for (int k = 0; k <= 3; k++)
                            {
                                NUM = SUB_DATA[j].s.Substring(k, 1) == "0" ? NUM : k;
                                if(NUM != 0)
                                {
                                    break;
                                }
                                else if (k == 3)
                                {
                                    NUM = 3;
                                }
                            }
                            SUB_DATA[j].s = (SUB_DATA[j].s.Substring(0, 4)).Remove(0, NUM) + (SUB_DATA[j].s.Remove(0, 4) == "0" ? "" : ("." + SUB_DATA[j].s.Remove(0, 4))) + "화";
                        }
                    }
                    SUB_DATA[j].d = ((((SUB_DATA[j].d.Remove(0, 4)).Insert(2, "/")).Insert(5, "-")).Insert(8, ":")).Insert(11, ":");
                    SUB_DATA[j].a = (SUB_DATA[j].a.Trim()).Insert(0, "http://");
                    SUB_DATA[j].s = (SUB_DATA[j].s == "9999화" ? "TVA/BD" : SUB_DATA[j].s);
                    SUB_DATA[j].n = (SUB_DATA[j].n == null ? "제작자 정보 없음" : SUB_DATA[j].n);
                }
            }
        }

        //7
        // DRAW TABLE DATA ON LISTVIEW
        private void DrawList(bool m)
        {
            if (m == true)
            {
                List<RootObject> listOfAnimes = new List<RootObject>();
                if (IsNoList == true || Is404 == true || IsError == true)
                {
                    listOfAnimes.Add(new RootObject {c = "red", a = "", i = 0, s = (IsNoList == true ? "목록 없음" : "서버응답오류 / 점검중"), t = "상태", g = (IsError == true ? "서버로부터 올바른 값을 받지 못했습니다." : "알 수 없음"), l = "", sd = "", ed = ""});
                }
                else
                {
                    foreach (RootObject k in TABLE_DATA)
                    {
                        if (k.s.Contains(SearchString) == true || k.t.Contains(SearchString) == true || k.g.Contains(SearchString) == true)
                        {
                            listOfAnimes.Add(new RootObject {c = k.c, a = "", i = k.i, s = k.s, t = k.t, g = k.g, l = k.l, sd = k.sd, ed = k.ed });
                        }
                    }
                }
                TableView.ItemsSource = listOfAnimes;
                Json_Data = null;
                TABLE_DATA = null;
            }
            else if (m == false)
            {
                List<RootObject_S> listOfSubs = new List<RootObject_S>();
                if (NoSub == false)
                {
                    foreach (RootObject_S k in SUB_DATA)
                    {
                        listOfSubs.Add(new RootObject_S { s = k.s, d = k.d, a = k.a, n = k.n });
                    }
                }
                else if (NoSub == true)
                {
                    listOfSubs.Add(new RootObject_S { s = "", d = "", a = "", n = "자막 없음" });
                }
                SubView.ItemsSource = listOfSubs;
                Json_sub_Data = null;
                SUB_DATA = null;
            }
        }
    }
}