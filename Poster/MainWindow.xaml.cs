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

namespace Poster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static DiscogsRelease currentRelease;

        public MainWindow()
        {
            InitializeComponent();
            InitializeStylesComponent();
            TextBox_Url.Text = "https://www.discogs.com/Gong-Flying-Teapot/release/1879546";
        }

        private void InitializeStylesComponent()
        {
            var gridView = new GridView();
            gridView.Columns.Add(new GridViewColumn { Header = "Style", DisplayMemberBinding = new Binding("Name") });

            StylesList.View = gridView;
            StylesList.AddHandler(MouseDoubleClickEvent, new RoutedEventHandler(ListView1_MouseLeftButtonDown));
        }

        private void ListView1_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            DependencyObject depObj = e.OriginalSource as DependencyObject;
            if (depObj != null)
            {
                // go up the visual hierarchy until we find the list view item the click came from  
                // the click might have been on the grid or column headers so we need to cater for this  
                DependencyObject current = depObj;
                while (current != null && current != StylesList)
                {
                    ListViewItem clickedItem = current as ListViewItem;
                    if (clickedItem != null)
                    {
                        StylesList.Items.Remove(clickedItem.Content);
                        return;
                    }
                    current = VisualTreeHelper.GetParent(current);
                }
            }
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
            ///
            /// TODO
            currentRelease._Styles = new List<Style>();
            for (int i = 0; i < currentRelease.Styles.Length; i++)
            {
                currentRelease._Styles.Add(new Style { Name = currentRelease.Styles[i] });
            }
            ///
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
            TextBox_Styles.Text = "";
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

            for (int i = 0; i < info.Styles.Length; i++)
            {
                StylesList.Items.Add(new Style { Name = info.Styles[i] });
                TextBox_Styles.Text += info.Styles[i];
                if (i == info.Styles.Length - 1)
                {
                    break;
                }
                TextBox_Styles.Text += ", ";
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

        private object getAttribute(Dictionary<string, object> collection, string attribute)
        {
            return collection.Keys.Contains(attribute) && collection[attribute].ToString().Length > 0 ? collection[attribute] : string.Empty;
        }

        private object getString(Dictionary<string, object> collection, string attribute)
        {
            return collection.Keys.Contains(attribute) && collection[attribute].ToString().Length > 0 ? collection[attribute] : string.Empty;
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
            sb.Append(GenerateTopTags());
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

            if (!string.IsNullOrEmpty(TextBox_Styles.Text))
            {
                sb.AppendLine($"Styles: {TextBox_Styles.Text}");
            }

            sb.AppendLine();
            sb.AppendLine(GenerateBottomTags());

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

        private string GenerateTopTags()
        {
            if (currentRelease._Styles.Count == 0)
            {
                throw new NotImplementedException();
            }

            var sb = new StringBuilder();

            foreach (var style in StylesList.Items)
            {
                sb.Append($"#{(style as Style).Name.Replace(" ", "_")}");
                if (style != StylesList.Items[StylesList.Items.Count - 1])
                {
                    sb.Append($" / ");
                }
            }

            //for (int i = 0; i < currentRelease.Styles.Length; i++)
            //{
            //    sb.Append($"#{currentRelease.Styles[i].Replace(" ", "_")}");
            //    if (i == currentRelease.Styles.Length - 1)
            //    {
            //        break;
            //    }
            //    sb.Append($" / ");
            //}

            return sb.ToString();
        }

        private string GenerateBottomTags()
        {
            var sb = new StringBuilder();
            sb.Append($"#{TextBox_Artist.Text.Replace(" ", "_")}");
            if (TextBox_Artist.Text.IndexOf(" ") > -1)
            {
                sb.Append($" #{TextBox_Artist.Text.Replace(" ", "")}");
            }
            sb.Append($" #{TextBox_Album.Text.Replace(" ", "_")}");
            if (TextBox_Album.Text.IndexOf(" ") > -1)
            {
                sb.Append($" #{TextBox_Album.Text.Replace(" ", "")}");
            }
            sb.AppendLine();

            for (int i = 0; i < currentRelease.Styles.Length; i++)
            {
                if (currentRelease.Styles[i].IndexOf(" ") > -1)
                {
                    sb.Append($"#{currentRelease.Styles[i].Replace(" ", "")}");
                    if (i == currentRelease.Styles.Length - 1)
                    {
                        break;
                    }
                    sb.Append($" ");
                }
            }

            sb.Append(GenerateLabelTags(currentRelease.Labels));
            return sb.ToString();
        }

        private static string GenerateLabelTags(List<Label> labels)
        {
            var result = string.Empty;
            var last = labels.Last();

            foreach (var label in labels)
            {
                if (label.Name.IndexOf(" ") > -1)
                {
                    result = string.Concat(result, "#", label.Name.Replace(" ", "_"), " ");
                    result = string.Concat(result, "#", label.Name.Replace(" ", ""));
                }
                else
                {
                    result = string.Concat(result, "#", label.Name);
                }
                if (!label.Equals(last))
                {
                    result = string.Concat(result, " ");
                }
            }
            if (!string.IsNullOrEmpty(result))
            {
                result = string.Concat(Environment.NewLine, result);
            }
            return result;
        }
    }
}
