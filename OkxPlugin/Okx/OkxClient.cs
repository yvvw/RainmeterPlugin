using System.Threading.Tasks;
using OKX.Api;
using OKX.Api.Authentication;
using OKX.Api.Models.TradingAccount;
using RMPlugin;

namespace OkxPlugin.Okx
{
    public class OkxClient
    {
        private readonly OKXRestApiClient client;

        public OkxClient(string accessKey, string secretKey, string passphrase)
        {
            client = new OKXRestApiClient(new OKXRestApiClientOptions
            {
                RawResponse = true,
                ApiCredentials = new OkxApiCredentials(accessKey, secretKey, passphrase)
            });
        }

        public async Task<OkxAccountBalance> GetAccountBalanceAsync()
        {
            var result = await client.TradingAccount.GetAccountBalanceAsync();
            RMLog.NoticeF("raw {0}", result.Raw);
            if (result.GetResultOrError(out var balance, out var error))
            {
                return balance;
            }
            else
            {
                RMLog.ErrorF("GetAccountBalance failed {0}", error);
                return null;
            }
        }
    }
}
