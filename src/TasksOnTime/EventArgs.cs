//===============================================================================
// Microsoft patterns & practices
// CompositeUI Application Block
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.Collections.Generic;
using System.Text;

namespace TasksOnTime
{
    /// <summary>
    /// Generic arguments class to pass to event handlers that need to receive data.
    /// </summary>
    /// <typeparam name="TData">The type of data to pass.</typeparam>
    public class EventArgs<TData> : EventArgs
    {
        TData data;

        /// <summary>
        /// Initializes the EventArgs class.
        /// </summary>
        /// <param name="data">Information related to the event.</param>
        /// <exception cref="ArgumentNullException">The data is null.</exception>
        public EventArgs(TData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            this.data = data;
        }

        /// <summary>
        /// Gets the information related to the event.
        /// </summary>
        public TData Data
        {
            get { return data; }
        }

        /// <summary>
        /// Provides a string representation of the argument data.
        /// </summary>
        public override string ToString()
        {
            return data.ToString();
        }
    }
}
