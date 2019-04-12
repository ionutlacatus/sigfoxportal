using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace sigfoxportal
{
    public partial class Startup
    {
        private DataAccess db = new DataAccess();
        public void ConfigureAuth(IAppBuilder app)
        {
            string ClientId = ConfigurationManager.AppSettings["ClientID"];
            string Authority = string.Format(ConfigurationManager.AppSettings["Authority"], ConfigurationManager.AppSettings["AADId"]);
            string AzureResourceManagerIdentifier = ConfigurationManager.AppSettings["AzureResourceManagerIdentifier"];

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions { });
            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = ClientId,
                    Authority = Authority,
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        RedirectToIdentityProvider = (context) =>
                        {
                            // This ensures that the address used for sign in and sign out is picked up dynamically from the request
                            // this allows you to deploy your app (to Azure Web Sites, for example) without having to change settings
                            // Remember that the base URL of the address used here must be provisioned in Azure AD beforehand.
                            //string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;

                            object obj = null;
                            if (context.OwinContext.Environment.TryGetValue("DomainHint", out obj))
                            {
                                string domainHint = obj as string;
                                if (domainHint != null)
                                {
                                    context.ProtocolMessage.SetParameter("domain_hint", domainHint);
                                }
                            }

                            context.ProtocolMessage.RedirectUri = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path);
                            context.ProtocolMessage.PostLogoutRedirectUri = new UrlHelper(HttpContext.Current.Request.RequestContext).Action
                                ("Index", "Home", null, HttpContext.Current.Request.Url.Scheme);
                            context.ProtocolMessage.Resource = AzureResourceManagerIdentifier;
                            return Task.FromResult(0);
                        },
                        AuthorizationCodeReceived = (context) =>
                        {
                            //X509Certificate2 keyCredential = new X509Certificate2(HttpContext.Current.Server.MapPath
                            //    (ConfigurationManager.AppSettings["KeyCredentialPath"]), "", X509KeyStorageFlags.MachineKeySet);
                            //ClientAssertionCertificate clientAssertion = new ClientAssertionCertificate(ClientId, keyCredential);

                            string signedInUserUniqueName = context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.Name).Value
                                .Split('#')[context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.Name).Value.Split('#').Length - 1];

                            var tokenCache = new ADALTokenCache(signedInUserUniqueName);
                            tokenCache.Clear();

                            AuthenticationContext authContext = new AuthenticationContext(Authority, tokenCache);
                            ClientCredential credentials = new ClientCredential(ConfigurationManager.AppSettings["ClientID"],
                                                                   ConfigurationManager.AppSettings["ClientSecret"]);
                            AuthenticationResult result = authContext.AcquireTokenByAuthorizationCodeAsync(
                                context.Code, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credentials).Result;

                            return Task.FromResult(0);
                        }
                    }
                });
        }
    }
}