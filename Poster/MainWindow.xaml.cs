using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Web.Script.Serialization;
using System.Collections;
using System.Diagnostics;

namespace Poster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DiscogsRelease currentRelease;
        public bool IsStyleFlyoutShown = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeStylesComponent();
            TextBox_Url.Text = "https://www.discogs.com/Gong-Flying-Teapot/release/1879546";
        }

        private void InitializeStylesComponent()
        {
            var gridView = new GridView();
            gridView.Columns.Add(new GridViewColumn { Header = "Style", DisplayMemberBinding = new Binding() });

            StylesList.View = gridView;
            StylesList.AddHandler(MouseDoubleClickEvent, new RoutedEventHandler(ListView1_MouseDoubleClick));
        }

        private void ListView1_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            DependencyObject depObj = e.OriginalSource as DependencyObject;
            if (depObj != null)
            {
                DependencyObject current = depObj;
                while (current != null && current != StylesList)
                {
                    ListViewItem clickedListViewItem = current as ListViewItem;
                    if (clickedListViewItem != null)
                    {
                        StylesList.Items.Remove(clickedListViewItem.Content);
                        return;
                    }

                    ScrollViewer clickedScrollViewer = current as ScrollViewer;
                    if (clickedScrollViewer != null)
                    {
                        ShowStyleFlyout();
                        return;
                    }

                    current = VisualTreeHelper.GetParent(current);
                }
            }
        }

        private void ShowStyleFlyout()
        {
            if (IsStyleFlyoutShown == false)
            {
                TextBox element = new TextBox();
                element.Name = "textBox_flyout";
                RegisterName("textBox_flyout", element);
                element.Height = 23;
                element.Width = 160;
                element.HorizontalAlignment = HorizontalAlignment.Stretch;
                element.VerticalContentAlignment = VerticalAlignment.Center;
                element.AddHandler(KeyDownEvent, new KeyEventHandler(FlyoutButtonPressed));

                Grid.SetColumn(element, 1);
                StylesGrid.Children.Add(element);
                element.Focus();
                IsStyleFlyoutShown = true;
            }
        }

        private void FlyoutButtonPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tb = (TextBox)FindName("textBox_flyout");
                if (tb != null)
                {
                    string newStyle = tb.Text;
                    if (!string.IsNullOrEmpty(newStyle))
                    {
                        AddStyle(newStyle);
                    }

                    StylesGrid.Children.Remove(tb);
                    UnregisterName("textBox_flyout");
                    IsStyleFlyoutShown = false;
                }
            }
        }

        private void AddStyle(string newStyle)
        {
            if (IsDuplicate(newStyle) == false)
            {
                StylesList.Items.Add(newStyle);
            }
        }

        private bool IsDuplicate(string newStyle)
        {
            return false;
            /// TODO: 
            /// Levenstein Distance
            /// 80% similarity
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RefreshForm();
            var url = TextBox_Url.Text;
            bool valid = isValidUrl(url);

            var parts = url.Split('/');
            var releaseId = parts[Array.IndexOf(parts, "release") + 1];
            var releaseInfo = getRelease(releaseId);
            if (releaseInfo == null)
            {
                return;
            }
            currentRelease = fromJSON(releaseInfo);

            FillForm(currentRelease);
        }

        private void RefreshForm()
        {
            TextBox_Artist.Text = "";
            TextBox_Album.Text = "";
            TextBox_Country.Text = "";
            TextBox_Year.Text = "";
            TextBox_Label.Text = "";
            TextBox_CatNo.Text = "";
            TextBox_Genres.Text = "";
            TextBox_Result.Text = "";
            StylesList.Items.Clear();
        }

        private void FillForm(DiscogsRelease info)
        {
            TextBox_Artist.Text = info.Artists.FirstOrDefault().Name;
            TextBox_Album.Text = info.Title;
            TextBox_Country.Text = info.Country;
            TextBox_Year.Text = info.Year.ToString();
            TextBox_Label.Text = info.Labels.FirstOrDefault().Name;
            TextBox_CatNo.Text = info.Labels.FirstOrDefault().CatNo;

            if (info.Styles != null)
            {
                foreach (var style in info.Styles)
                {
                    StylesList.Items.Add(style);
                }
            }

            for (int i = 0; i < info.Genres.Length; i++)
            {
                TextBox_Genres.Text += info.Genres[i];
                if (i == info.Genres.Length - 1)
                {
                    break;
                }
                TextBox_Genres.Text += ", ";
            }
        }

        private bool isValidUrl(string url)
        {
            if (url.Length != 0)
            {
                Uri uriResult;
                bool result1 = Uri.TryCreate(url, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttps;

                if (result1 && url.IndexOf("discogs") > -1 && url.IndexOf("release") > -1)
                {
                    return true;
                }
            }
            else
            {
                MessageBox.Show("Add at least one url.");
            }
            return false;
        }

        private string getRelease(string id)
        {
            string url = GlobalVar.DISCOGS_API_URL + "/releases/" + id;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/json; charset=utf-8";
            request.UserAgent = "Discogs Test Client";
            string responce = string.Empty;
            try
            {
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    responce = reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                {
                    var resp = (HttpWebResponse)ex.Response;
                    if (resp.StatusCode == HttpStatusCode.NotFound)
                    {
                        MessageBoxResult result = MessageBox.Show(this, "Release is not found.\r\nCheck the link.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        if (result == MessageBoxResult.OK)
                        {
                            return null;
                        }
                    }
                }
            }
            return responce;
        }

        private DiscogsRelease fromJSON(string json)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<DiscogsRelease>(json);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(Tagger.GetHeaderTags(this));
            sb.AppendLine();
            sb.AppendLine($"Artist: {TextBox_Artist.Text}");
            sb.AppendLine($"Album: {TextBox_Album.Text}");
            if (!string.IsNullOrEmpty(TextBox_Country.Text))
            {
                sb.AppendLine($"Country: {TextBox_Country.Text}");

                if (!string.IsNullOrEmpty(TextBox_Year.Text))
                {
                    sb.Append($" '{TextBox_Year.Text.Substring(2, 2)}");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(TextBox_Year.Text))
                {
                    sb.AppendLine($"Year: {TextBox_Year.Text.Substring(2, 2)}");
                }
            }
            if (!string.IsNullOrEmpty(TextBox_Label.Text))
            {
                sb.AppendLine($"Label: {TextBox_Label.Text}");
                if (!string.IsNullOrEmpty(TextBox_CatNo.Text))
                {
                    sb.Append($" — {TextBox_CatNo.Text}");
                }
            }

            if (StylesList.Items.Count > 0)
            {
                sb.AppendLine($"Styles: ");
                string stylesLine = StringBuilder.GetSeparatedLine(StylesList.Items, ", ");
                sb.Append(stylesLine);
            }

            sb.AppendLine();
            sb.AppendLine(Tagger.GetFooterTags(this));

            var result = sb.ToString();

            // result = Sanitize(result);

            TextBox_Result.Text = result;
        }

        private string Sanitize(string text)
        {
            string[] result = new string[] { };
            if (!string.IsNullOrEmpty(text))
            {
                result = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                for (int i = 0; i < result.Length; i++)
                {
                    if (!string.IsNullOrEmpty(result[i]))
                    {
                        while (result[i][result[i].Length - 1] == ' ')
                        {
                            result[i] = result[i].Remove(result[i].Length - 1);
                        }
                    }
                }
            }

            return string.Join("", result);
        }
    }
}
