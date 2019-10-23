using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using System.Collections.ObjectModel;
using System.Threading;
using Plugin.BLE.Abstractions;

namespace BLEEvents
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        IBluetoothLE ble;
        IAdapter adapter;
        ObservableCollection<IDevice> deviceList;
        IDevice device;

        bool _useAutoConnect;
        public bool UseAutoConnect
        {
            get => _useAutoConnect;

            set
            {
                if (_useAutoConnect == value)
                    return;

                _useAutoConnect = value;
                //RaisePropertyChanged();
            }
        }
        public MainPage()
        {
            InitializeComponent();
            ble = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;
            deviceList = new ObservableCollection<IDevice>();
            lv.ItemsSource = deviceList;
        }

        /// <summary>
        /// Scan the list of Devices
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnScan_Clicked(object sender, EventArgs e)
        {
            var state = ble.State;
            if (state == BluetoothState.Off)
            {
                DisplayAlert("Notice", "Your Bluetooth is off ! Turn it on !", "Error !");
            }
            else if (state == BluetoothState.On)
            {
                try
                {
                    btnScan.Text = "Scanning ...";
                    deviceList.Clear();

                    adapter.DeviceDiscovered += (s, a) =>
                    {
                        deviceList.Add(a.Device);
                    };
                    if (!ble.Adapter.IsScanning)
                    {
                        await adapter.StartScanningForDevicesAsync();

                    }
                }
                catch (Exception ex)
                {
                    DisplayAlert("Notice", ex.Message.ToString(), "Error !");
                }
                btnScan.Text = "Scan BLE Devices";
            }
        }

        /// <summary>
        /// Connect to a specific device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnConnect_Clicked(object sender, EventArgs e)
        {
            //CancellationTokenRegistration cancellationToken = new CancellationTokenRegistration();
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            try
            {
                if (device != null)
                {
                    btnConnect.Text = "Connecting...";
                    await adapter.StopScanningForDevicesAsync();
                    await adapter.ConnectToDeviceAsync(device, new ConnectParameters(autoConnect: true, forceBleTransport: true), cancellationToken.Token);
                    if (device.State == DeviceState.Connected || device.State == DeviceState.Limited)
                    {
                        DisplayAlert("Notice", "Connected !", "OK");
                        btnConnect.Text = "Connected";
                    }
                }
                else
                {
                    DisplayAlert("Notice", "No Device selected !", "OK");
                }
            }
            catch (DeviceConnectionException ex)
            {
                //Could not connect to the device
                DisplayAlert("Notice", ex.Message.ToString(), "OK");
            }
            btnConnect.Text = "Connect";
        }

        IList<IService> Services;
        IService Service;

        /// <summary>
        /// Get list of services
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnGetServices_Clicked(object sender, EventArgs e)
        {
            Guid guid = new Guid();
            Service = await device.GetServiceAsync(Guid.Parse(guid.ToString()));
            Service = await device.GetServiceAsync(device.Id);
        }

        IList<ICharacteristic> Characteristics;
        ICharacteristic Characteristic;
        /// <summary>
        /// Get Characteristics
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private async void btnGetcharacters_Clicked(object sender, EventArgs e)
        //{
        //    var characteristics = await Service.GetCharacteristicsAsync();
        //    Guid idGuid = Guid.Parse("guid");
        //    Characteristic = await Service.GetCharacteristicAsync(idGuid);
        //    //  Characteristic.CanRead
        //}

        IDescriptor descriptor;
        IList<IDescriptor> descriptors;

        //private async void btnDescriptors_Clicked(object sender, EventArgs e)
        //{
        //    //descriptors = await Characteristic.GetDescriptorsAsync();
        //    descriptor = await Characteristic.GetDescriptorAsync(Guid.Parse("guid"));

        //}

        //private async void btnDescRW_Clicked(object sender, EventArgs e)
        //{
        //    var bytes = await descriptor.ReadAsync();
        //    await descriptor.WriteAsync(bytes);
        //}

        //private async void btnGetRW_Clicked(object sender, EventArgs e)
        //{
        //    var bytes = await Characteristic.ReadAsync();
        //    await Characteristic.WriteAsync(bytes);
        //}

        //private async void btnUpdate_Clicked(object sender, EventArgs e)
        //{
        //    Characteristic.ValueUpdated += (o, args) =>
        //    {
        //        var bytes = args.Characteristic.Value;
        //    };
        //    await Characteristic.StartUpdatesAsync();
        //}

        /// <summary>
        /// Select Items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lv_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (lv.SelectedItem == null)
            {
                return;
            }

            device = lv.SelectedItem as IDevice;

        }

    }
}
