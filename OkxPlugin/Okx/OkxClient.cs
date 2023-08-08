using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ApiSharp.Models;
using OKX.Api;
using OKX.Api.Authentication;
using OKX.Api.Models.MarketData;
using OKX.Api.Models.TradingAccount;
using RMPlugin;

namespace OkxPlugin.Okx
{
    public class OkxClient
    {
        private readonly OKXRestApiClient client;

        public OkxClient(string accessKey, string secretKey, string passphrase)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;
            client = new OKXRestApiClient(new OKXRestApiClientOptions
            {
                ApiCredentials = new OkxApiCredentials(accessKey, secretKey, passphrase)
            });
        }

        public async Task<IEnumerable<OkxTicker>> GetSpotMarketTickersAsync() =>
            ExtractResult(await client.OrderBookTrading.MarketData.GetTickersAsync(
                OKX.Api.Enums.OkxInstrumentType.Spot));

        public async Task<OkxAccountBalance> GetAccountBalanceAsync() =>
            ExtractResult(await client.TradingAccount.GetAccountBalanceAsync());

        public async Task<IEnumerable<OkxPosition>> GetAccountPositionsAsync() =>
            ExtractResult(await client.TradingAccount.GetAccountPositionsAsync());

        private T ExtractResult<T>(RestCallResult<T> result)
        {
            if (result.GetResultOrError(out T data, out var error)) return data;
            RMLog.ErrorF("failed {0}", error);
            return default;
        }
    }
}
