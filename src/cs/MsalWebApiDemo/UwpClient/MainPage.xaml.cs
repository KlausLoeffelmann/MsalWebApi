using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UwpClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string URL_PREFIX= "https://corewebapimsal.azurewebsites.net/api/";
        private const string secret= "hzoyJQA2+=*honVIDY7875]";


        AuthenticationResult myAr;
        private SynchronizationContext mySyncContext = SynchronizationContext.Current;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void btnTestButton1_Click(object sender, RoutedEventArgs e)
        {
            var httpClient = new System.Net.Http.HttpClient();
            System.Net.Http.HttpResponseMessage response;

            try
            {
                var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, URL_PREFIX + "values");
                response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                Debug.Print(content.ToString());
            }

            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
        }

        private async void btnTestButton2_Click(object sender, RoutedEventArgs e)
        {
            var httpClient = new System.Net.Http.HttpClient();
            System.Net.Http.HttpResponseMessage response;

            try
            {
                var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, URL_PREFIX + "authvalues");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", myAr.AccessToken);
                response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                Debug.Print(content.ToString());
            }

            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
        }

        /// <summary>
        /// Perform an HTTP GET request to a URL using an HTTP Authorization header
        /// </summary>
        /// <param name="url">The URL</param>
        /// <param name="token">The token</param>
        /// <returns>String containing the results of the GET operation</returns>
        public async Task<string> GetHttpContentWithToken(string url, string token)
        {
            var httpClient = new System.Net.Http.HttpClient();
            System.Net.Http.HttpResponseMessage response;

            try
            {
                var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
                //Add the token in Authorization header
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }

            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            // let's see if we have a user in our belly already
            try
            {
                txtStatusLine.Text = "Initializing...";
                await Task.Delay(3000);

                txtStatusLine.Text = "Trying to log you in silently...";

                myAr = await App.PCA.AcquireTokenSilentAsync(App.Scopes, App.PCA.Users.FirstOrDefault());
                await RefreshUserDataAsync(myAr.AccessToken);
                Debug.Print("Silent login succeeded.");
                txtStatusLine.Text = "Logged in silently.";
                //btnLogout.IsEnabled = true;
            }
            catch (Exception eOuter)
            {
                Debug.Print(eOuter.Message);

                try
                {
                    //No, we don't - so we need to login interactively.
                    txtStatusLine.Text = "Trying to log you in silently failed. Manual Login...";

                    myAr = await App.PCA.AcquireTokenAsync(App.Scopes, App.UiParent);
                    await RefreshUserDataAsync(myAr.AccessToken);
                    Debug.Print("Manual login succeeded.");
                    //btnLogout.IsEnabled = true;
                    txtStatusLine.Text = "Logged in succeeded.";
                }
                catch (Exception eInner)
                {
                    Debug.Print(eInner.Message);
                }
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            SignOut();
            txtStatusLine.Text = "Logged out.";
        }

        public async Task RefreshUserDataAsync(string token)
        {
            //get data from API
            HttpClient client = new HttpClient();
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me");
            message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);
            HttpResponseMessage response = await client.SendAsync(message);
            string responseString = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    JObject user = JObject.Parse(responseString);
                    Debug.Print("Parsing responseString was OK.");
                    Debug.Print($"User's Displayname:{user["displayName"]}");
                    Debug.Print($"User's ID:{user["id"]}");
                    Debug.Print($"User's Surname:{user["surname"]}");
                    Debug.Print($"User's Principal Name:{user["userPrincipalName"]}");
                }
                catch (Exception ex)
                {
                    Debug.Print("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    Debug.Print("Something went wrong parsing the Response String!");
                    Debug.Print($"Exception Message:{ex.Message}");
                    Debug.Print($"Let's log out...");
                    Debug.Print("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    SignOut();
                }
            }
            else
            {
                Debug.Print("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Debug.Print("Something went wrong with the API call!");
                Debug.Print("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            }
        }

        private void SignOut()
        {
            try
            {
                foreach (var user in App.PCA.Users)
                {
                    App.PCA.Remove(user);
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }
    }
}
