using System;
using System.Text;

namespace Lachain.CommunicationHub.Net
{
    public class Hub
    {
        internal readonly Lazy<StartHub> StartHub;
        internal readonly Lazy<StopHub> StopHub;
        internal readonly Lazy<LogLevel> LogLevel;
        internal readonly Lazy<SendMessage> SendMessage;


        const string Lib = "hub";

        private static readonly Lazy<string> LibPathLazy = new Lazy<string>(() => LibPathResolver.Resolve(Lib));
        private static readonly Lazy<IntPtr> LibPtr = new Lazy<IntPtr>(() => LoadLibNative.LoadLib(LibPathLazy.Value));

        internal static Hub Imports = new Hub();

        private Hub()
        {
            // load all delegates
            StartHub = LazyDelegate<StartHub>();
            StopHub = LazyDelegate<StopHub>();
            LogLevel = LazyDelegate<LogLevel>();
            SendMessage = LazyDelegate<SendMessage>();
        }

        Lazy<TDelegate> LazyDelegate<TDelegate>()
        {
            var symbol = SymbolNameCache<TDelegate>.SymbolName;
            return new Lazy<TDelegate>(
                () => LoadLibNative.GetDelegate<TDelegate>(LibPtr.Value, symbol),
                true
            );
        }

        public static void Start(string grpcAddress, string bootstrapAddress)
        {
            unsafe
            {
                var grpcAddressBytes = Encoding.UTF8.GetBytes(grpcAddress);
                var bootstrapAddressBytes = Encoding.UTF8.GetBytes(bootstrapAddress);
                fixed (byte* grpcAddressPtr = grpcAddressBytes)
                fixed (byte* bootstrapAddressPtr = bootstrapAddressBytes)
                {
                    Imports.StartHub.Value(
                        grpcAddressPtr, grpcAddressBytes.Length,
                        bootstrapAddressPtr, bootstrapAddressBytes.Length
                    );
                }
            }
        }

        public static void Send(byte[] publicKey, byte[] data)
        {
            unsafe
            {
                fixed (byte* publicKeyPtr = publicKey)
                fixed (byte* dataPtr = data)
                {
                    Imports.SendMessage.Value(publicKeyPtr, publicKey.Length, dataPtr, data.Length);
                }
            }
        }

        public static void Stop()
        {
            Imports.StopHub.Value();
        }

        public static void SetLogLevel(string s)
        {
            unsafe
            {
                var bytes = Encoding.UTF8.GetBytes(s);
                fixed (byte* ptr = bytes)
                {
                    Imports.LogLevel.Value(ptr, bytes.Length);
                }
            }
        }
    }
}