using System;
using System.Runtime.InteropServices;
using Rainmeter;
using OKX.Api.Models.TradingAccount;
using Nito.AsyncEx;
using RMPlugin;
using RMPlugin.RMType;
using OkxPlugin.Okx;

namespace OkxPlugin
{
    class Measure
    {
        private readonly API api;
        private readonly OkxClient client;

        private OkxAccountBalance Balance;

        public string BalanceStr;

        public Measure(API api)
        {
            this.api = api;
            client = SetupOkxClient();
        }

        public void Update()
        {
            Balance = AsyncContext.Run(client.GetAccountBalanceAsync);
            if (!(Balance is null))
            {
                BalanceStr = Balance.Details.ToString();
            }
        }

        private OkxClient SetupOkxClient()
        {
            var accessKey = api.ReadString("OkxAccessKey", "");
            var secretKey = api.ReadString("OkxSecretKey", "");
            var passphrase = api.ReadString("OkxPassphrase", "");
            RMLog.NoticeF("OkxAccessKey `{0}` OkxSecretKey `{1}` OkxPassphrase `{2}`", accessKey, secretKey, passphrase);

            return new OkxClient(accessKey, secretKey, passphrase);
        }

        static public implicit operator Measure(IntPtr data)
        {
            return (Measure)GCHandle.FromIntPtr(data).Target;
        }
    }

    public class Plugin
    {
        private static RMString balance;

        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            var api = (API)rm;
            RMLog.Api = rm;
            RMLog.Debug("Plugin initialize.");

            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure(api)));
        }

        [DllExport]
        public static void Finalize(IntPtr data)
        {
            RMLog.Debug("Plugin finalize.");

            balance?.Dispose();
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
        public static IntPtr GetString(IntPtr data)
        {
            Measure measure = (Measure)data;

            balance?.Dispose();
            balance = measure.BalanceStr;

            return balance;
        }
    }
}
