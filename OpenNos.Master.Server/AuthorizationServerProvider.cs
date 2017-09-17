using Microsoft.Owin.Security.OAuth;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OpenNos.Master.Server
{
    public class AuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {

            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });
            AccountDTO account = DAOFactory.AccountDAO.LoadByName(context.UserName);


            if (account != null && account.Password.ToLower().Equals(EncryptionBase.Sha512(context.Password)))
            {
                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, context.UserName));
                identity.AddClaim(new Claim(ClaimTypes.Role, account.Authority.ToString()));
                context.Validated(identity);
            }
            else
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;

            }
        }
    }
}