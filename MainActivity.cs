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
        private static UUID MY_UUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        private Stream inStream = null;
        private TextView TextDisplay = null;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            // Get the UI controls from the loaded layout:
            RadioButton BTOff = FindViewById<RadioButton>(Resource.Id.BTOff);
            RadioButton BTOn = FindViewById<RadioButton>(Resource.Id.BTOn);
            RadioButton NotConnected = FindViewById<RadioButton>(Resource.Id.NotConnected);
            RadioButton Connect = FindViewById<RadioButton>(Resource.Id.Connect);
            Button LockState = FindViewById<Button>(Resource.Id.LockState);
            Button LockID = FindViewById<Button>(Resource.Id.LockID);
            Button LockOpen = FindViewById<Button>(Resource.Id.LockOpen);
            Button LockClose = FindViewById<Button>(Resource.Id.LockClose);
            TextDisplay = FindViewById<TextView>(Resource.Id.TextDisplay);

            NotConnected.Click += doDisconnect;
            Connect.Click += doConnect;
            LockOpen.Click += doOpen;
            LockClose.Click += doClose;
            LockState.Click += doLockState;
            LockID.Click += doLockID;

            CheckBt();
        }

        void doLockState(object sender, EventArgs e)
        {
            dataToSend = new Java.Lang.String("state\n");
            writeData(dataToSend);
        }

        void doLockID(object sender, EventArgs e)
        {
            dataToSend = new Java.Lang.String("ID\n");
            writeData(dataToSend);
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
                if (Connect()) {
                    TextDisplay.Enabled = true;
                }
            }
        }

        void doDisconnect(object sender, EventArgs e)
        {
            if (btSocket != null && btSocket.IsConnected) {
                try {
                    btSocket.Close();
                }
                catch (System.Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }
        }


        public bool Connect()
        {

            var pairedDevices = mBluetoothAdapter.BondedDevices;
            BluetoothDevice device;

            if (pairedDevices.Count > 0) {
                // There are paired devices. Get the name and address of each paired device.
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
                            return false;
                        }
                        beginListenForData();
                        return true;
                    }
                }
            }
            TextDisplay.Text = "No Lock Found in bluetooth environment (HC-05)";
            return false;
        }

        public void beginListenForData()
        {
            try { 
                inStream = btSocket.InputStream;
            }
            catch (System.IO.IOException ex) { 
                Console.WriteLine(ex.Message);
            }
            TextDisplay.Text = "Lock: init communication";
            Task.Factory.StartNew(() => {
                byte[] buffer = new byte[1024];
                byte[] bufferCumulate = new byte[1024];
                int[] len = new int[10];
                int lenIndex = 0;   
                int cumul = 0;
                int bytes;
                bool eol = false;
                while (true) { 
                    try { 
                        bytes = inStream.Read(buffer, 0, buffer.Length);
                        len[lenIndex++] = bytes;
                        for (int i=0; i<bytes;i++) {
                            if (buffer[i] == '\n') {
                                eol = true;
                            }
                        }
                        Buffer.BlockCopy(buffer, 0 ,
                                        bufferCumulate, cumul,
                                            bytes);
                        cumul += bytes;
                        bufferCumulate[cumul] = 0;
                        if (eol) {
                            string valor = System.Text.Encoding.ASCII.GetString(bufferCumulate);
                            eol = false;
                            cumul = 0;
                            lenIndex = 0;
                            RunOnUiThread(() => {
                                // Result.Text = Result.Text + "\n" + valor;
                                TextDisplay.Text = valor;
                            });
                        }
                    }
                    catch (Java.IO.IOException) { 
                        RunOnUiThread(() => {
                            TextDisplay.Text = "Lock: end of communication";
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
