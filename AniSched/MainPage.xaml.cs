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

        MessageDialog DiagNoSubLink = new MessageDialog("자막 링크가 등록되어 있지 않습니다.", "알림");

        int value = -1;

        Boolean NoSub = false, IsEndAnime = false, Is404 = false, IsNoList = false, IsAllList = false;

        string Json_Data, Json_sub_Data, SearchString = "";

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
            RunClient(true, value);
        }

        private void TableView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (Is404 == false && IsNoList == false)
            {
                RootObject mk = e.ClickedItem as RootObject;
                Title_Image.Source = new BitmapImage(new Uri("http://novang.dlinkddns.com/anisched/" + mk.i + ".jpg", UriKind.Absolute));
                SubView_T.Text = mk.a + (mk.a == "" ? ""  : " ") + mk.s;
                SubView_ID.Text = Convert.ToString(mk.i);
                SubView_L.Text = mk.l;
                SubView_S.Text = (mk.sd.Length == 8 ? "20" + mk.sd : mk.sd);
                SubView_E.Text = (mk.ed.Length == 8 ? "20" + mk.ed : mk.ed);
                RunClient(false, mk.i);
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

        private void SearchList_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Is404 == false && IsNoList == false && SearchString != SearchList.Text)
            {
                SearchString = SearchList.Text;
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
                if (mk.a != "http://")
                {
                    await Windows.System.Launcher.LaunchUriAsync(new Uri(mk.a));
                }
                else
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
            await Windows.System.Launcher.LaunchUriAsync(new Uri(mk.Text));
        }

        private void ColorPick()
        {
            var MAColor = Windows.UI.Color.FromArgb(255, 254, 59, 114);
            var MBColor = Windows.UI.Color.FromArgb(255, 250, 250, 250);
            var MCColor = Windows.UI.Color.FromArgb(255, 25, 25, 25);

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

        private async Task<string> RequestPage(bool k, int i)
        {
            if (k == true)
            {
                if (IsEndAnime == true)
                {
                    Response = await Request.GetAsync(new Uri("http://www.anissia.net/anitime/end?p=" + i));
                }
                else
                {
                    Response = await Request.GetAsync(new Uri("http://www.anissia.net/anitime/list?w=" + i));
                }
            }
            else
            {
                Response = await Request.GetAsync(new Uri("http://www.anissia.net/anitime/cap?i=" + i));
            }
            
            return await Response.Content.ReadAsStringAsync();
        }

        private void resetBtn()
        {
            SUN_BTN.Foreground = WHIT; SUN_BTN.Background = PINK;
            MON_BTN.Foreground = WHIT; MON_BTN.Background = PINK;
            TUE_BTN.Foreground = WHIT; TUE_BTN.Background = PINK;
            WED_BTN.Foreground = WHIT; WED_BTN.Background = PINK;
            THU_BTN.Foreground = WHIT; THU_BTN.Background = PINK;
            FRI_BTN.Foreground = WHIT; FRI_BTN.Background = PINK;
            SAT_BTN.Foreground = WHIT; SAT_BTN.Background = PINK;
            OVA_BTN.Foreground = WHIT; OVA_BTN.Background = PINK;
            NEW_BTN.Foreground = WHIT; NEW_BTN.Background = PINK;
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

        private void Title_Image_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Image_Animation.Resume();
        }

        private void Title_Image_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Image_Animation.Pause();
        }

        //2
        private void SetDest(string s)
        {
            if (s == "SUN_BTN" || s == "Sunday")
            {
                value = 0; SUN_BTN.Foreground = PINK; SUN_BTN.Background = WHIT;
            }
            else if (s == "MON_BTN" || s == "Monday")
            {
                value = 1; MON_BTN.Foreground = PINK; MON_BTN.Background = WHIT;
            }
            else if (s == "TUE_BTN" || s == "Tuesday")
            {
                value = 2; TUE_BTN.Foreground = PINK; TUE_BTN.Background = WHIT;
            }
            else if (s == "WED_BTN" || s == "Wednesday")
            {
                value = 3; WED_BTN.Foreground = PINK; WED_BTN.Background = WHIT;
            }
            else if (s == "THU_BTN" || s == "Thursday")
            {
                value = 4; THU_BTN.Foreground = PINK; THU_BTN.Background = WHIT;
            }
            else if (s == "FRI_BTN" || s == "Friday")
            {
                value = 5; FRI_BTN.Foreground = PINK; FRI_BTN.Background = WHIT;
            }
            else if (s == "SAT_BTN" || s == "Saturday")
            {
                value = 6; SAT_BTN.Foreground = PINK; SAT_BTN.Background = WHIT;
            }
            else if (s == "OVA_BTN")
            {
                value = 7; OVA_BTN.Foreground = PINK; OVA_BTN.Background = WHIT;
            }
            else if (s == "NEW_BTN")
            {
                value = 8; NEW_BTN.Foreground = PINK; NEW_BTN.Background = WHIT;
            }
        }

        //3
        private async void RunClient(bool k, int i)
        {
            if (k == true)
            {
                Json_Data = "";

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
                Json_sub_Data = "";

                Json_sub_Data = await RequestPage(k, i);

                GetJson(false);
                if (NoSub == false)
                {
                    AlignString(false);
                }
                DrawList(false);
                SearchList.IsEnabled = false;
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

                while (Reader.Read())
                {
                    JsonSerializer Serializer = new JsonSerializer();
                    RootObject rootObject = Serializer.Deserialize<RootObject>(Reader);

                    rootObjects.Add(rootObject);
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
                int tmp1 = Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd")), tmp2, tmp3;
                int i = TABLE_DATA.Count();
                for (int j = 0; j < i; j++)
                {
                    TABLE_DATA[j].s = ((TABLE_DATA[j].s.Replace("&lt;", "<")).Replace("&gt;", ">")).Replace("&amp;", "&");
                    TABLE_DATA[j].t = TABLE_DATA[j].t.Insert(2, ":");
                    TABLE_DATA[j].l = (TABLE_DATA[j].l.Trim()).Insert(0, "http://");
                    if (value == 7 || value == 8 || IsEndAnime == true || IsAllList == true)
                    {
                        TABLE_DATA[j].sd = ((((TABLE_DATA[j].sd.Remove(0, 2)).Insert(2, "/")).Insert(5, "/")).Replace("99/99/99", "미정")).Replace("/99", "/--");
                        TABLE_DATA[j].ed = ((((TABLE_DATA[j].ed.Remove(0, 2)).Insert(2, "/")).Insert(5, "/")).Replace("99/99/99", "미정")).Replace("/99", "/--");
                        TABLE_DATA[j].c = "Transparent";
                        TABLE_DATA[j].t = TABLE_DATA[j].sd;
                    }
                    else
                    {
                        TABLE_DATA[j].sd = (((TABLE_DATA[j].sd.Insert(4, "/")).Insert(7, "/")).Replace("/99/99", "/--")).Replace("/99", "/--");
                        TABLE_DATA[j].ed = (((TABLE_DATA[j].ed.Insert(4, "/")).Insert(7, "/")).Replace("/99/99", "/--")).Replace("/99", "/--");
                        tmp2 = Convert.ToInt32(TABLE_DATA[j].ed.Replace("/", ""));
                        tmp3 = Convert.ToInt32(TABLE_DATA[j].sd.Replace("/", ""));
                        TABLE_DATA[j].a = (TABLE_DATA[j].a == "true" ? (tmp1 >= tmp2 && tmp2 != 0 ? "[完]" : (tmp3 > tmp1 && tmp3 != 0 ? "[" + TABLE_DATA[j].sd.Remove(0, 5) + "]" : "")) : "[결방]");
                        TABLE_DATA[j].c = TABLE_DATA[j].a == "[完]" ? "#FF455A64" : (TABLE_DATA[j].a == "[결방]" ? "#FFFF1744" : TABLE_DATA[j].a == "" ? "#FF00E676" : "#FF00B0FF");
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
                if (IsNoList == true)
                {
                    listOfAnimes.Add(new RootObject { a = "", i = 0, s = "목록 없음", t = "", g = "", l = "", sd = "", ed = "" });
                }
                else if (Is404 == true)
                {
                    listOfAnimes.Add(new RootObject { a = "", i = 0, s = "점검중", t = "", g = "", l = "", sd = "", ed = "" });
                }
                else
                {
                    foreach (RootObject k in TABLE_DATA)
                    {
                        if (k.s.Contains(SearchString) == true || k.t.Contains(SearchString) == true || k.g.Contains(SearchString) == true)
                        {
                            listOfAnimes.Add(new RootObject { c = k.c, a = (value == 7 || value == 8 || IsEndAnime == true || IsAllList == true ? "" : k.a), i = k.i, s = k.s, t = k.t, g = k.g, l = k.l, sd = k.sd, ed = k.ed });
                        }
                    }
                }
                TableView.ItemsSource = listOfAnimes;
            }
            else if (m == false)
            {
                if (NoSub == false)
                {
                    List<RootObject_S> listOfSubs = new List<RootObject_S>();

                    foreach (RootObject_S k in SUB_DATA)
                    {
                        listOfSubs.Add(new RootObject_S { s = k.s, d = k.d, a = k.a, n = k.n });
                    }

                    SubView.ItemsSource = listOfSubs;
                }
                else if (NoSub == true)
                {
                    List<RootObject_S> listOfSubs = new List<RootObject_S>();
                    listOfSubs.Add(new RootObject_S { s = "", d = "", a = "", n = "자막 없음" });
                    SubView.ItemsSource = listOfSubs;
                }
            }
        }
    }
}