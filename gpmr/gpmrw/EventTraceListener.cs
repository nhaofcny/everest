﻿/* 
 * Copyright 2008-2012 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: Justin Fyfe
 * Date: 09-26-2011
 **/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace gpmrw
{

    /// <summary>
    /// Trace listener event arguments
    /// </summary>
    class TraceListenerEventArgs : EventArgs
    {
        /// <summary>
        /// The message that was raised
        /// </summary>
        public string Message { get; set; }

    }

    /// <summary>
    /// An event based trace listener
    /// </summary>
    class EventTraceListener : TraceListener
    {

        /// <summary>
        /// Fired when a message is raised
        /// </summary>
        public event EventHandler<TraceListenerEventArgs> MessageRaised;
        
        public override void Write(string message)
        {
            if (MessageRaised != null)
                MessageRaised(this, new TraceListenerEventArgs() { Message = String.Format("{0}", message) });
        }

        public override void WriteLine(string message)
        {
            if (MessageRaised != null)
                MessageRaised(this, new TraceListenerEventArgs() { Message = String.Format("{0}\r\n", message) });
        }
    }
}
