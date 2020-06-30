using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Driver
{
    public class GenericWebcamDriver : IDriver
    {
        private Dictionary<string, string> _nameIdDictionary;
        private VideoCaptureDevice _currentDevice;
        private bool _isRunning = false;

        public List<string> SearchDevices()
        {
            _nameIdDictionary = new Dictionary<string, string>();

            FilterInfoCollection videoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            for (int i = 0; i < videoCaptureDevices.Count; i++)
                _nameIdDictionary.Add(videoCaptureDevices[i].Name, videoCaptureDevices[i].MonikerString);

            return videoCaptureDevices.Cast<FilterInfo>().Select(x => x.Name).ToList();
        }

        public object Init(object id)
        {
            throw new NotImplementedException();
        }

        public object SetProperty(object key, object value)
        {
            throw new NotImplementedException();
        }

        public void Start(object id, Action<object, Bitmap> newFrameCallback)
        {
            _currentDevice = new VideoCaptureDevice(_nameIdDictionary[(string)id]);
            _currentDevice.NewFrame += (sender, e) =>
            {
                newFrameCallback.Invoke(sender, e.Frame);
            };
            _currentDevice.Start();
            _isRunning = true;
        }

        public bool IsRunning()
        {
            return _isRunning;
        }

        public void Stop()
        {
            _currentDevice.Stop();
            _isRunning = false;
        }
    }
}