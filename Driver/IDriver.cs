using System;
using System.Collections.Generic;
using System.Drawing;

namespace Driver
{
    public interface IDriver
    {
        List<string> SearchDevices();

        object Init(object id);

        object SetProperty(object key, object value);

        void Start(object id, Action<object, Bitmap> newFrameCallback);

        bool IsRunning();

        void Stop();
    }
}