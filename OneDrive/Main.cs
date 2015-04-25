using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;    
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Live;


namespace OneDrive
{
    public partial class Main : Form, IRefreshTokenHandler
    {
        public Main()
        {
            InitializeComponent();
        }
        
        public AuthResult authResult;
        string ClientID = "000000004814AD4C";
        MyBrowser myBrowser;
        LiveAuthClient liveAuthClient;
        LiveConnectClient liveConnectClient;
        private Microsoft.Live.RefreshTokenInfo refreshTokenInfo;

        string[] Scopes = { "wl.signin", "wl.basic", "wl.photos", "wl.share", "wl.skydrive", "wl.skydrive_update", "wl.work_profile" };
        LiveAuthClient AuthClient
        {
            get
            {
                if (this.liveAuthClient == null)
                {
                    this.AuthClient = new LiveAuthClient(ClientID, this);
                }

                return this.liveAuthClient;
            }

            set // удалена обработка ошибок
            {
                this.liveAuthClient = value;
                this.liveConnectClient = null;
            }
        }
        private void CleanupBrowser()
        {
            if (this.myBrowser != null)
            {
                this.myBrowser.Dispose();
                this.myBrowser = null;
            }
        } 
        private void button1_Click(object sender, EventArgs e)
        {
            string startUri = this.AuthClient.GetLoginUrl(Scopes);
            string endUri = "https://login.live.com/oauth20_desktop.srf";
            this.myBrowser = new MyBrowser(startUri, endUri, this.OnAuthCompleted);
            this.myBrowser.Show();
            this.myBrowser.MyBrowser_Load();
        }
        private async void OnAuthCompleted(AuthResult authResult)
        {
            this.CleanupBrowser();
            if (authResult.AuthorizeCode != null)
            {
                try
                {
                    LiveConnectSession session = await this.AuthClient.ExchangeAuthCodeAsync(authResult.AuthorizeCode);
                    this.liveConnectClient = new LiveConnectClient(session);
                    LiveOperationResult meRes = await this.liveConnectClient.GetAsync("me");
                    dynamic meData = meRes.Result;
                    this.nameLabel.Text = meData.name;
                    LiveDownloadOperationResult meImgResult = await this.liveConnectClient.DownloadAsync("me/picture");
                    this.meImage.Image = Image.FromStream(meImgResult.Stream);

                }
                catch(Exception e)
                {
                    MessageBox.Show("Error", e.Message);
                }
            }
        }
        Task IRefreshTokenHandler.SaveRefreshTokenAsync(RefreshTokenInfo tokenInfo)// хз
        {
            // Note: 
            // 1) In order to receive refresh token, wl.offline_access scope is needed.
            // 2) Alternatively, we can persist the refresh token.
            return Task.Factory.StartNew(() =>
            {
                this.refreshTokenInfo = tokenInfo;
            });
        }
        Task<RefreshTokenInfo> IRefreshTokenHandler.RetrieveRefreshTokenAsync() // хз
        {
            return Task.Factory.StartNew<RefreshTokenInfo>(() =>
            {
                return this.refreshTokenInfo;
            });
        }
    }
}
