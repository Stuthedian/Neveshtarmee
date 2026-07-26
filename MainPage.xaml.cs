using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace MauiApp2
{
    public partial class MainPage : ContentPage
    {
        string xml_doc = "data.xml";
        bool authorized = false;

        Entry? lastFocused;
        private static HttpClientHandler hch = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        private static HttpClient httpClient;

        public MainPage()
        {
            InitializeComponent();
            LoadBtn.IsEnabled = false;

            if (getAuth().Result.Item1)
            {
                authBut_Clicked(new object(), new EventArgs());
            }
        }

        private async Task createAuth(string? site, string? login, string? password)
        {
            Task.Run(async delegate
            {
                await SecureStorage.Default.SetAsync("uri", site);
                await SecureStorage.Default.SetAsync("user", login);
                await SecureStorage.Default.SetAsync(login, password);
            });
        }

        private Task<Tuple<bool, string?, string?>> getAuth()
        {
            return Task.Run(async delegate {

                string? username = await SecureStorage.Default.GetAsync("user");

                if (username == null)
                {
                    return Tuple.Create<bool, string?, string?>(false, null, null);
                }
                else
                {
                    string? pass = SecureStorage.Default.GetAsync(username).GetAwaiter().GetResult();
                    if (pass == null)
                    {
                        return Tuple.Create<bool, string?, string?>(false, null, null);
                    }
                    else
                    {
                        return Tuple.Create<bool, string?, string?>(true, username, pass);
                    }
                }
            });
        }

        private Task<string?> getAuthSite()
        {
            return Task.Run(async delegate
            {
                return await SecureStorage.Default.GetAsync("uri");
            });
        }

        private void LoadBut_Clicked(object? sender, EventArgs e)
        {
            GetAsync(httpClient, rootVSL, this, statuslabel);
            saveBut.IsEnabled = true;
        }

        static async Task GetAsync(HttpClient httpClient, VerticalStackLayout rootVsl, MainPage main, Label statuslabel)
        {
            using HttpResponseMessage response = await httpClient.GetAsync("data.xml");
         
            var result = await response.Content.ReadAsStringAsync();
            statuslabel.Text = response.StatusCode.ToString();
            using (StreamWriter sw = File.CreateText(Path.Combine(FileSystem.AppDataDirectory, "data.xml")))
            {
                sw.Write(result);
            }
            XDocument doc = XDocument.Load(Path.Combine(FileSystem.AppDataDirectory, "data.xml"));
            XElement root = doc.Element("root");
            XElement items = root.Element("items");
            XElement layer = items.Element("layer");
            
            rootVsl.Clear();
            main.readLayer(layer, rootVsl);        
        }

        private void readLayer(XElement layer, VerticalStackLayout vsl)
        {
            int i = -1;
            foreach (XElement entry in layer.Elements())
            {
                if (entry.Name == "entry")
                {
                    Entry en = createEntry(vsl, i);
                    en.Text = entry.Value;
                    
                }
                else
                {
                    VerticalStackLayout newvsl = new VerticalStackLayout();
                    double leftpad = Convert.ToDouble(entry.Attribute("padding").Value);
                    newvsl.Padding = new Thickness(leftpad, newvsl.Padding.Top, newvsl.Padding.Right, newvsl.Padding.Bottom);
                    readLayer(entry, newvsl);
                    vsl.Add(newvsl);
                }
                i++;
            }
        }

        static async Task PutAsync(HttpClient httpClient, XDocument doc,  string filepath, string filename, Label statuslabel)
        {
            StringContent stringContent = new StringContent(doc.ToString(), Encoding.UTF8, "application/binary");
            
            var md5 = MD5.Create();
            var stream = File.OpenRead(filepath);
            var hash = md5.ComputeHash(stream);

            SHA256 mySHA256 = SHA256.Create();
            byte[] hashValue = mySHA256.ComputeHash(stream);

            var putmsg = new HttpRequestMessage(HttpMethod.Put, httpClient.BaseAddress + "/" +  filename);
            putmsg.Content = stringContent;
            putmsg.Headers.Add("Etag",
                    BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant());
            putmsg.Headers.Add("Sha256",
                BitConverter.ToString(hashValue).Replace("-", String.Empty));

            stream.Close();

            using HttpResponseMessage httpResponse = await httpClient.SendAsync(putmsg);
            statuslabel.Text = httpResponse.StatusCode.ToString();
        }

        private void tabBut_Clicked(object? sender, EventArgs e)
        {
            if (lastFocused is not null && sender is not null)
            {
                HorizontalStackLayout hslParent = (HorizontalStackLayout)lastFocused.Parent;
                VerticalStackLayout vslParent = (VerticalStackLayout)hslParent.Parent;
                                
                int index = vslParent.IndexOf(hslParent);
                if (index == 0)
                {
                    return;
                }

                VerticalStackLayout? childVsl = null;
                if (vslParent.Children.Count > index + 1
                    && vslParent.Children[index+1] is VerticalStackLayout)
                {
                    childVsl = (VerticalStackLayout)vslParent.Children[index+1];
                    vslParent.Remove(childVsl);
                }

                if (vslParent.Children[index - 1] is HorizontalStackLayout)
                {
                    HorizontalStackLayout uphsl = (HorizontalStackLayout)vslParent.Children[index - 1];
                    ((Label)uphsl.Children[0]).Text = "˅";
                }

                vslParent.Remove(hslParent);

                VerticalStackLayout newVsl;
                if (vslParent.Children[index-1] is VerticalStackLayout)
                {
                    newVsl = (VerticalStackLayout)vslParent.Children[index - 1];
                    newVsl.Add(hslParent);
                    if (childVsl is not null)
                    {
                        newVsl.Add(childVsl);
                    }
                }
                else
                {
                    newVsl = new VerticalStackLayout();
                    newVsl.Padding = new Thickness(vslParent.Padding.Left + 10, vslParent.Padding.Top, vslParent.Padding.Right, vslParent.Padding.Bottom);

                    newVsl.Add(hslParent);
                    if (childVsl is not null)
                    {
                        childVsl.Padding = new Thickness(newVsl.Padding.Left + 10, newVsl.Padding.Top, newVsl.Padding.Right, newVsl.Padding.Bottom);
                        newVsl.Add(childVsl);
                    }
                    vslParent.Insert(index, newVsl);
                }               
            }
        }

        private void Entry_Focused(object? sender, FocusEventArgs e)
        {
            lastFocused = (Entry?)sender;
        }

        private void Entry_Completed(object? sender, EventArgs e)
        {
            if (sender is not null)
            {
                Entry compeletedEntry = (Entry)sender;
                HorizontalStackLayout hslParent = (HorizontalStackLayout)compeletedEntry.Parent;
                VerticalStackLayout vslParent = (VerticalStackLayout)hslParent.Parent;
                int idx = vslParent.IndexOf((IView)hslParent);

                Entry entry = createEntry(vslParent, idx);
                entry.Focus();
            }
        }

        private Entry createEntry(VerticalStackLayout vslParent, int idx)
        {
            Label label = new Label();
            label.Text = "•";
            label.VerticalTextAlignment = TextAlignment.Center;
            label.FontSize = 26;

            Entry entry = new Entry();
            entry.Completed += Entry_Completed;
            entry.Focused += Entry_Focused;

            HorizontalStackLayout newHsl = new HorizontalStackLayout();
            newHsl.Add(label);
            newHsl.Add(entry);

            if (vslParent.Children.Count > idx + 1)
            {
                IView view = vslParent.Children[idx + 1];
                if (view is VerticalStackLayout)
                {
                    idx++;
                }
            }

            vslParent.Insert(idx + 1, newHsl);

            return entry;
        }

        private void unTabBut_Clicked(object sender, EventArgs e)
        {
            if (lastFocused is not null)
            {
                HorizontalStackLayout hslParent = (HorizontalStackLayout)lastFocused.Parent;
                VerticalStackLayout vslParent = (VerticalStackLayout)hslParent.Parent;

                if (vslParent == rootVSL)
                {
                    return;
                }

                VerticalStackLayout targetVsl = (VerticalStackLayout)vslParent.Parent;

                vslParent.Remove(hslParent);

                int index = targetVsl.IndexOf(vslParent);
                targetVsl.Insert(index+1, hslParent);
                IView? child = vslParent.Children.Count > 0 ? vslParent.Children[0] : null;
                if (child is not null && child is VerticalStackLayout)
                {       
                    ((VerticalStackLayout)child).Padding 
                        = new Thickness(targetVsl.Padding.Left + 10, targetVsl.Padding.Top, targetVsl.Padding.Right, targetVsl.Padding.Bottom);
                    vslParent.RemoveAt(0);
                    targetVsl.Insert(index + 1 + 1, child);
                }

                if (vslParent.Children.Count == 0)
                {
                    targetVsl.Remove(vslParent);
                }
            }
        }

        private void unColBut_Clicked(object sender, EventArgs e)
        {
            if (lastFocused is not null)
            {
                HorizontalStackLayout hslParent = (HorizontalStackLayout)lastFocused.Parent;
                VerticalStackLayout vslParent = (VerticalStackLayout)hslParent.Parent;

                int index = vslParent.IndexOf(hslParent);
                IView? nextchild = vslParent.Children.Count > index+1 ? vslParent.Children[index+1] : null;
                if (nextchild is not null && nextchild is VerticalStackLayout)
                {
                    VerticalStackLayout subvsl = (VerticalStackLayout)nextchild;
                    if (subvsl.IsVisible)
                    {
                        subvsl.IsVisible = false;
                        ((Label)hslParent.Children[0]).Text = "˃";
                    }
                    else 
                    {
                        subvsl.IsVisible = true;
                        ((Label)hslParent.Children[0]).Text = "˅";
                    }
                    
                }

            }
        }

        private void saveBut_Clicked(object sender, EventArgs e)
        {
            XDocument doc = new XDocument();
            XElement root = new XElement("root");

            XElement xItems = new XElement("items");

            recordLayer(rootVSL, xItems);
            root.Add(xItems);
            doc.Add(root);
            string filepath = Path.Combine(FileSystem.AppDataDirectory, xml_doc);
            doc.Save(filepath);

            PutAsync(httpClient, doc, filepath, xml_doc, statuslabel);
        }

        private void recordLayer(VerticalStackLayout vsl, XElement parentXmlLayer)
        {
            XElement layer = new XElement("layer");
            XAttribute xPadding = new XAttribute("padding", vsl.Padding.Left);
            layer.Add(xPadding);
            foreach (IView entry in vsl.Children)
            {
                if (entry is HorizontalStackLayout)
                {
                    XElement xEntry = new XElement("entry", ((Entry)((HorizontalStackLayout)entry).Children[1]).Text);
                    layer.Add(xEntry);
                }
                else
                {
                    recordLayer((VerticalStackLayout)entry, layer);
                }
            }

            parentXmlLayer.Add(layer);
        }

        private void backupBut_Clicked(object sender, EventArgs e)
        {
            XDocument doc = new XDocument();
            XElement root = new XElement("root");

            XElement xItems = new XElement("items");
            recordLayer(rootVSL, xItems);

            root.Add(xItems);
            doc.Add(root);
            string filepostfix = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString()
                + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()
                + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString();
            string filepath = Path.Combine(FileSystem.AppDataDirectory, "data" + filepostfix + ".xml");
            doc.Save(filepath);
            PutAsync(httpClient, doc, filepath, "data" + filepostfix + ".xml", statuslabel);

            File.Delete(filepath);
        }

        private void resetAuthBut_Clicked(object sender, EventArgs e)
        {
            SecureStorage.Default.RemoveAll();
            LoadBtn.IsEnabled = false;
            saveBut.IsEnabled = false;
            authBut.IsEnabled = true;
        }

        private void authBut_Clicked(object sender, EventArgs e)
        {
            string? site = null;
            string? login = null;
            string? password = null;
            Tuple<bool, string?, string?> t = getAuth().Result;

            if (t.Item1)
            {
                authorized = true;
                LoadBtn.IsEnabled = true;
                login = t.Item2;
                password = t.Item3;
            }
            else
            {
                site = ((rootVSL.Children[0] as HorizontalStackLayout).Children[1] as Entry).Text;
                login = ((rootVSL.Children[1] as HorizontalStackLayout).Children[1] as Entry).Text;
                password = ((rootVSL.Children[2] as HorizontalStackLayout).Children[1] as Entry).Text;
                createAuth(site, login, password);
                authorized = true;
                LoadBtn.IsEnabled = true;

            }
            httpClient = new(hch)
            {
                BaseAddress = new Uri(getAuthSite().Result),
            };

            storeAuth(login, password);
            authBut.IsEnabled = false;

            rootVSL.Clear();
        }

        private void storeAuth(string login, string password)
        {
            httpClient.DefaultRequestHeaders.Add("Authorization",
                "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(login + ":" + password)));
        }
    }
}
