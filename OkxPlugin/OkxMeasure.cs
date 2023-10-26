using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using OKX.Api.Models.MarketData;
using OKX.Api.Models.TradingAccount;

using Rainmeter;
using Okx;
using RMPlugin;

namespace OkxPlugin
{
    internal class Measure : IDisposable
    {
        static public implicit operator Measure(IntPtr data)
        {
            return (Measure)GCHandle.FromIntPtr(data).Target;
        }

        private readonly API m_Api;
        private readonly OkxClient m_Client;
        private bool m_Disposed;

        private readonly bool m_SpotEnabled;
        private List<OkxTicker> m_SpotTicker;

        private readonly bool m_BalanceEnabled;
        private OkxAccountBalance m_Balance;

        private readonly bool m_PositonEnabled;
        private List<OkxPosition> m_Positon;

        private readonly Dictionary<string, IntPtr> m_Cache = new Dictionary<string, IntPtr>() {
            { "default", Marshal.StringToHGlobalUni("-") },
        };

        public Measure(API api)
        {
            m_Api = api;

            var accessKey = m_Api.ReadString("OkxAccessKey", "");
            var secretKey = m_Api.ReadString("OkxSecretKey", "");
            var passphrase = m_Api.ReadString("OkxPassphrase", "");
            RMLog.NoticeF("OkxAccessKey `{0}` OkxSecretKey `{1}` OkxPassphrase `{2}`", accessKey, secretKey, passphrase);
            m_Client = new OkxClient(accessKey, secretKey, passphrase);

            if (m_SpotEnabled = m_Api.ReadInt("EnableSpot", 0) == 1) RMLog.Notice("Spot enabled.");
            if (m_BalanceEnabled = m_Api.ReadInt("EnableBalance", 0) == 1) RMLog.Notice("Balance enabled.");
            if (m_PositonEnabled = m_Api.ReadInt("EnablePosition", 0) == 1) RMLog.Notice("Positon enabled.");
        }

        ~Measure()
        {
            Dispose(false);
        }

        public void Update()
        {
            if (m_SpotEnabled) Task.Run(async () => m_SpotTicker = (await m_Client.GetSpotMarketTickersAsync()).ToList());
            if (m_BalanceEnabled) Task.Run(async () => m_Balance = await m_Client.GetAccountBalanceAsync());
            if (m_PositonEnabled) Task.Run(async () => m_Positon = (await m_Client.GetAccountPositionsAsync()).ToList());
        }

        public IntPtr GetSpotTicker(string coin) => FreeAndCachedGet(coin.WithCachePrefix("spot"), () =>
        {
            OkxTicker ticker = m_SpotTicker?.FirstOrDefault(it => it.Instrument == coin);
            if (ticker != null)
            {
                string lastPrice = ticker.LastPrice.ToUSDString();
                string percent = ((ticker.LastPrice - ticker.OpenPriceUtc0) / ticker.OpenPriceUtc0).ToPercentString();
                return $"{lastPrice} {percent}";
            }
            return null;
        });

        public IntPtr GetBalance(string coin) => FreeAndCachedGet(coin.WithCachePrefix("balance"), () =>
        {
            OkxAccountBalanceDetail balance = m_Balance?.Details?.FirstOrDefault(it => it.Currency == coin);
            if (balance != null)
            {
                return balance.UsdEquity?.ToUSDString();
            }
            return null;
        });

        public IntPtr GetPosition(string coin) => FreeAndCachedGet(coin.WithCachePrefix("position"), () =>
        {
            OkxPosition position = m_Positon?.FirstOrDefault(it => it.Instrument == coin);
            if (position != null)
            {
                string side = position.PositionSide.ToString();
                string lastPrice = position.UnrealizedProfitAndLoss?.ToUSDString();
                string percent = position.UnrealizedProfitAndLossRatio?.ToPercentString();
                return $"{side} {lastPrice} {percent}";
            }
            return null;
        });

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IntPtr FreeAndCachedGet(string key, Func<string> getter)
        {
            if (m_Cache.TryGetValue(key, out var ptr))
            {
                m_Cache.Remove(key);
                Marshal.FreeHGlobal(ptr);
            }
            var str = getter();
            return str != null ? m_Cache[key] = Marshal.StringToHGlobalUni(str) : m_Cache["default"];
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                foreach (var it in m_Cache) Marshal.FreeHGlobal(it.Value);
                m_Cache.Clear();
                m_Disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    internal static class FormatterExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToUSDString(this decimal value, string format = "#,##0.00") =>
            value.ToString(value > 0.005m ? format : $"0.0(e+0)");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToPercentString(this decimal value, string format = "#,##0.00%") =>
            value.ToString(format);
    }

    internal static class CacheExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string WithCachePrefix(this string value, string prefix) => value + prefix;
    }
}
