﻿using OmniView.Library.Devices;
using OmniView.Library.Network;
using System.Threading.Tasks;

namespace OmniView.Library.DeviceDescriptions
{
    [Device("axis", "Axis")]
    public class AxisDevice : IDevice
    {
        private int currentPanSpeed;
        private int currentTiltSpeed;

        public DeviceClient Client {get;}
        public IDeviceDescription Description {get;}
        public DeviceCapabilities Capabilities {get;}
        public DeviceResolution? Resolution {get; private set;}

        public bool? ShowText {get; set;}
        public bool? ShowDate {get; set;}
        public bool? ShowClock {get; set;}


        public AxisDevice(IDeviceDescription description)
        {
            this.Description = description;

            Client = new DeviceClient();

            Capabilities = new DeviceCapabilities {
                SupportsPicture = true,
                SupportsVideo = true,
                SupportsPanTilt = false,
                SupportsZoom = false,
            };

            ShowText = false;
            ShowDate = false;
            ShowClock = false;
        }

        public UrlBuilder GetPictureUrl()
        {
            var builder = GetUrlBuilder("axis-cgi/jpg/image.cgi");

            if (Resolution.HasValue)
                builder.Query["resolution"] = $"{Resolution.Value.Width}x{Resolution.Value.Height}";

            if (ShowText.HasValue) builder.Query["text"] = ShowText.Value ? 1 : 0;
            if (ShowDate.HasValue) builder.Query["date"] = ShowDate.Value ? 1 : 0;
            if (ShowClock.HasValue) builder.Query["clock"] = ShowClock.Value ? 1 : 0;

            return builder;
        }

        public UrlBuilder GetVideoUrl()
        {
            var builder = GetUrlBuilder("axis-cgi/mjpg/video.cgi");

            if (Resolution.HasValue)
                builder.Query["resolution"] = $"{Resolution.Value.Width}x{Resolution.Value.Height}";

            if (ShowText.HasValue) builder.Query["text"] = ShowText.Value ? 1 : 0;
            if (ShowDate.HasValue) builder.Query["date"] = ShowDate.Value ? 1 : 0;
            if (ShowClock.HasValue) builder.Query["clock"] = ShowClock.Value ? 1 : 0;

            return builder;
        }

        public async Task SetResolution(DeviceResolution resolution)
        {
            await Task.Run(() => this.Resolution = resolution);
        }

        public async Task SetPtzDirectionAsync(PtzDirection direction)
        {
            var panSpeed = 0;
            var tiltSpeed = 0;

            if ((direction & PtzDirection.Left) == PtzDirection.Left)
                panSpeed--;

            if ((direction & PtzDirection.Right) == PtzDirection.Right)
                panSpeed++;

            if ((direction & PtzDirection.Up) == PtzDirection.Up)
                tiltSpeed++;

            if ((direction & PtzDirection.Down) == PtzDirection.Down)
                tiltSpeed--;

            if (panSpeed != currentPanSpeed || tiltSpeed != currentTiltSpeed) {
                await Ptz(panSpeed, tiltSpeed);
                currentPanSpeed = panSpeed;
                currentTiltSpeed = tiltSpeed;
            }
        }

        private async Task Ptz(int panSpeed, int tiltSpeed)
        {
            var url = GetUrlBuilder("axis-cgi/com/ptz.cgi");
            url.Query["continuouspantiltmove"] = $"{panSpeed},{tiltSpeed}";

            await Client.GetAsync(url.ToString());
        }

        private UrlBuilder GetUrlBuilder(string path)
        {
            var url = new UrlBuilder(Description.Url, path);

            //if (!string.IsNullOrEmpty(Description.Username))
            //    url.Query["user"] = Description.Username;

            //if (!string.IsNullOrEmpty(Description.Password))
            //    url.Query["pwd"] = Description.Password;

            return url;
        }
    }
}
