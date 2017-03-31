using System;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using System.IO;
using Java.Util;
using Android.Bluetooth;
using System.Threading.Tasks;




namespace BT_X
{
    [Activity(Label = "BT_X", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {

        TextView Result;
        private Java.Lang.String dataToSend;
        private BluetoothAdapter mBluetoothAdapter = null;
        private BluetoothSocket btSocket = null;
        private Stream outStream = null;
        private static string address = "00:13:01:07:01:59";
        private static UUID MY_UUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        private Stream inStream = null;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            // Get the UI controls from the loaded layout:
            RadioButton BTOff = FindViewById<RadioButton>(Resource.Id.BTOff);
            RadioButton BTOn = FindViewById<RadioButton>(Resource.Id.BTOn);
            RadioButton NotConnected = FindViewById<RadioButton>(Resource.Id.NotConnected);
            RadioButton Connected = FindViewById<RadioButton>(Resource.Id.Connected);
            Button LockOpen = FindViewById<Button>(Resource.Id.LockOpen);
            Button LockClose = FindViewById<Button>(Resource.Id.LockClose);
            //TextView TextDisplay = FindViewById<Button>(Resource.Id.TextDisplay);

            NotConnected.Click += doDisconnect;
            Connected.Click += doConnect;
            LockOpen.Click += doOpen;
            LockClose.Click += doClose;

            CheckBt();
        }

        void doOpen(object sender, EventArgs e)
        {
            dataToSend = new Java.Lang.String("open\n");
            writeData(dataToSend);
        }

        void doClose(object sender, EventArgs e)
        {
            dataToSend = new Java.Lang.String("close\n");
            writeData(dataToSend);
        }


        private void CheckBt()
        {
            mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            if (mBluetoothAdapter == null) {
                Toast.MakeText(this,
                    "Bluetooth not supported", ToastLength.Short)
                    .Show();
                return;
            }

            if (!mBluetoothAdapter.Enable()) {
                Toast.MakeText(this, "Enabling BlueTooth",
                    ToastLength.Short).Show();
                /*
                    Intent enableBtIntent = new Intent(BluetoothAdapter, ACTION_REQUEST_ENABLE);
                    StartActivity(enableBtIntent, REQUEST_ENABLE_BT);
                    */
            }
        }
/*
        void onActivityResult(int requestCode, 
                int resultCode,
                Intent data) { 
            if (resultCode == Result.Ok) {
                return;
            }
        }
        */
        void doConnect(object sender, EventArgs e)
        {
            if (btSocket == null || !btSocket.IsConnected) {
                Connect();
            }
        }

        void doDisconnect(object sender, EventArgs e)
        {
            if (btSocket.IsConnected) {
                try {
                    btSocket.Close();
                }
                catch (System.Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }
        }


        public void Connect()
        {

            var pairedDevices = mBluetoothAdapter.BondedDevices;
            BluetoothDevice device;

            if (pairedDevices.Count > 0) {
                // There are paired devices. Get the name and address of each paired device.
                int i = 0;
                foreach (BluetoothDevice deviceBT in pairedDevices) {
                    String deviceName = deviceBT.Name;
                    if (deviceName == "HC-05") {
                        String deviceHardwareAddress = deviceBT.Address; // MAC address
                        device = deviceBT;
                        try {
                            btSocket = device.CreateRfcommSocketToServiceRecord(MY_UUID);
                            btSocket.Connect();
                            System.Console.WriteLine("Connected to HC-05");
                        }
                        catch (System.Exception e) {
                            Console.WriteLine(e.Message);
                            try {
                                btSocket.Close();
                            }
                            catch (System.Exception) {
                                System.Console.WriteLine("Socket error on close");
                            }
                            System.Console.WriteLine("Socket failed to create");
                            return;
                        }
                        //beginListenForData();
                    }
                }
            }


        }

        public void beginListenForData()
        {
            try { 
                inStream = btSocket.InputStream;
            }
            catch (System.IO.IOException ex) { 
                Console.WriteLine(ex.Message);
            }
            Task.Factory.StartNew(() => {
                byte[] buffer = new byte[1024];
                int bytes;
                while (true) { 
                    try { 
                        bytes = inStream.Read(buffer, 0, buffer.Length);
                        if (bytes > 0) { 
                            RunOnUiThread(() => {
                                string valor = System.Text.Encoding.ASCII.GetString(buffer);
                               // Result.Text = Result.Text + "\n" + valor;
                               TextDisplay.
                            });
                        }
                    }
                    catch (Java.IO.IOException) { 
                        RunOnUiThread(() => {
                            Result.Text = string.Empty;
                        });
                        break;
                    }
                }
            });
        }

        private void writeData(Java.Lang.String data)
        {
            try { 
                outStream = btSocket.OutputStream;
            }
            catch (System.Exception e) { 
                System.Console.WriteLine("Error send" + e.Message);
            }

            Java.Lang.String message = data;

            byte[] msgBuffer = message.GetBytes();

            try { 
                outStream.Write(msgBuffer, 0, msgBuffer.Length);
            }
            catch (System.Exception e) { 
                System.Console.WriteLine("Error send" + e.Message);
            }
        }
    }
}
