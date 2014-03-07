 #region License
/*
The MIT License (MIT)

Copyright (c) 2013-2014 The SharpFlame Authors.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
 #endregion

using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using Ninject;
using Ninject.Extensions.Logging;

namespace SharpFlame.Old.Settings
{
    public class Keylogger
    {
        private readonly ILogger logger;

        public Keylogger(ILoggerFactory logFactory) 
        {
            logger = logFactory.GetCurrentClassLogger();
        }

        [EventSubscription(KeyboardManagerEvents.OnKeyDown, typeof(OnPublisher))]
        public void HandleKeyDown(object sender, KeyboardEventArgs e)
        {
            logger.Debug("KeyDown: \"{0}\"", e.Key.ToString());
        }

        [EventSubscription(KeyboardManagerEvents.OnKeyUp, typeof(OnPublisher))]
        public void HandleKeyUp(object sender, KeyboardEventArgs e)
        {
            logger.Debug("KeyUp: \"{0}\"", e.Key.ToString());
        }
    }
}