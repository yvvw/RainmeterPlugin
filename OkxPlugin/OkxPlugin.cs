using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rainmeter;
using OKX.Api.Models.TradingAccount;
using OKX.Api.Models.MarketData;
using RMPlugin;
using OkxPlugin.Okx;

namespace OkxPlugin
{
    class Measure : IDisposable
    {
        static public implicit operator Measure(IntPtr data)
        {
            return (Measure)GCHandle.FromIntPtr(data).Target;
        }

        private readonly API m_api;
        private readonly OkxClient m_client;
        private bool m_disposed;


        private bool m_spot_enabled;
        private List<OkxTicker> m_spot_ticker;
        private Dictionary<string, IntPtr> m_spot_tickers = new Dictionary<string, IntPtr>();

        private bool m_balance_enabled;
        private OkxAccountBalance m_balance;
        private Dictionary<string, IntPtr> m_balances = new Dictionary<string, IntPtr>();

        private bool m_positon_enabled;
        private List<OkxPosition> m_positon;
        private Dictionary<string, IntPtr> m_positons = new Dictionary<string, IntPtr>();

        public Measure(API api)
        {
            m_api = api;
            m_client = SetupOkxClient();
            CheckEnabled();
        }

        ~Measure()
        {
            Dispose(false);
        }

        private OkxClient SetupOkxClient()
        {
            var accessKey = m_api.ReadString("OkxAccessKey", "");
            var secretKey = m_api.ReadString("OkxSecretKey", "");
            var passphrase = m_api.ReadString("OkxPassphrase", "");
            RMLog.NoticeF("OkxAccessKey `{0}` OkxSecretKey `{1}` OkxPassphrase `{2}`", accessKey, secretKey, passphrase);

            return new OkxClient(accessKey, secretKey, passphrase);
        }

        private void CheckEnabled()
        {
            if (m_spot_enabled = m_api.ReadInt("EnableSpot", 0) == 1) RMLog.Notice("Spot enabled.");
            if (m_balance_enabled = m_api.ReadInt("EnableBalance", 0) == 1) RMLog.Notice("Balance enabled.");
            if (m_positon_enabled = m_api.ReadInt("EnablePosition", 0) == 1) RMLog.Notice("Positon enabled.");
        }

        public void Update()
        {

            if (m_spot_enabled) Task.Run(async () => m_spot_ticker = (await m_client.GetSpotMarketTickersAsync()).ToList());
            if (m_balance_enabled) Task.Run(async () => m_balance = await m_client.GetAccountBalanceAsync());
            if (m_positon_enabled) Task.Run(async () => m_positon = (await m_client.GetAccountPositionsAsync()).ToList());
        }

        public IntPtr GetSpotTicker(string coin)
        {
            if (m_spot_tickers.TryGetValue(coin, out var value))
            {
                m_spot_tickers.Remove(coin);
                Marshal.FreeHGlobal(value);
            }
            IntPtr tickerPtr = IntPtr.Zero;
            if (m_spot_ticker != null)
            {
                OkxTicker ticker = m_spot_ticker.FirstOrDefault(it => it.Instrument == coin);
                if (ticker != null)
                {
                    string lastPrice = FormatUsd(ticker.LastPrice);
                    string percent = FormatPercent((ticker.LastPrice - ticker.OpenPriceUtc0) / ticker.OpenPriceUtc0);
                    tickerPtr = Marshal.StringToHGlobalUni($"{lastPrice} {percent}");
                }
            }
            if (tickerPtr == IntPtr.Zero) tickerPtr = Marshal.StringToHGlobalUni("-");
            return tickerPtr;
        }

        public IntPtr GetBalance(string coin)
        {
            if (m_balances.TryGetValue(coin, out var value))
            {
                m_balances.Remove(coin);
                Marshal.FreeHGlobal(value);
            }
            IntPtr balancePtr = IntPtr.Zero;
            if (m_balance != null)
            {
                OkxAccountBalanceDetail coinBalance = m_balance.Details.FirstOrDefault(it => it.Currency == coin);
                if (coinBalance != null)
                {
                    string balance = FormatUsd(coinBalance.UsdEquity);
                    balancePtr = Marshal.StringToHGlobalUni(balance);
                }
            }
            if (balancePtr == IntPtr.Zero) balancePtr = Marshal.StringToHGlobalUni("-");
            return balancePtr;
        }

        public IntPtr GetPosition(string coin)
        {
            if (m_positons.TryGetValue(coin, out var value))
            {
                m_positons.Remove(coin);
                Marshal.FreeHGlobal(value);
            }
            IntPtr positonPtr = IntPtr.Zero;
            if (m_positon != null)
            {
                OkxPosition position = m_positon.FirstOrDefault(it => it.Instrument == coin);
                if (position != null)
                {
                    string side = position.PositionSide.ToString();
                    string lastPrice = FormatUsd(position.UnrealizedProfitAndLoss);
                    string percent = FormatPercent(position.UnrealizedProfitAndLossRatio);
                    positonPtr = Marshal.StringToHGlobalUni($"{side} {lastPrice} {percent}");
                }
            }
            if (positonPtr == IntPtr.Zero) positonPtr = Marshal.StringToHGlobalUni("-");
            return positonPtr;
        }

        private string FormatUsd(Decimal? number) => number?.ToString("#,##0.00");

        private string FormatPercent(Decimal? number) => number?.ToString("#,##0.00%");

        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                foreach (var ticker in m_spot_tickers) Marshal.FreeHGlobal(ticker.Value);
                foreach (var balance in m_balances) Marshal.FreeHGlobal(balance.Value);
                foreach (var positon in m_positons) Marshal.FreeHGlobal(positon.Value);
                m_disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class Plugin
    {
        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            API api = rm;
            RMLog.Api = api;
            RMLog.Debug("Plugin initialize.");
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure(api)));
        }

        [DllExport]
        public static void Finalize(IntPtr data)
        {
            RMLog.Debug("Plugin finalize.");
            ((Measure)data).Dispose();
            GCHandle.FromIntPtr(data).Free();
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            RMLog.Debug("Plugin reload.");
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            RMLog.Debug("Plugin update.");
            Measure measure = (Measure)data;
            measure.Update();
            return 0.0;
        }

        [DllExport]
        public static IntPtr GetSpotTicker(IntPtr data, int argc,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 1)] string[] argv)
        {
            Measure measure = (Measure)data;
            return argc == 1 ? measure.GetSpotTicker(argv[0]) : IntPtr.Zero;
        }

        [DllExport]
        public static IntPtr GetBalance(IntPtr data, int argc,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 1)] string[] argv)
        {
            Measure measure = (Measure)data;
            return argc == 1 ? measure.GetBalance(argv[0]) : IntPtr.Zero;
        }

        [DllExport]
        public static IntPtr GetPosition(IntPtr data, int argc,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 1)] string[] argv)
        {
            Measure measure = (Measure)data;
            return argc == 1 ? measure.GetPosition(argv[0]) : IntPtr.Zero;
        }
    }
}
