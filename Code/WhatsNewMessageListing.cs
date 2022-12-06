// <copyright file="WhatsNewMessageListing.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace LifecycleRebalance
{
    using System;
    using AlgernonCommons.Notifications;

    /// <summary>
    /// "What's new" update messages.
    /// </summary>
    internal class WhatsNewMessageListing
    {
        /// <summary>
        /// Gets the list of versions and associated update message lines (as translation keys).
        /// </summary>
        internal WhatsNewMessage[] Messages => new WhatsNewMessage[]
        {
            new WhatsNewMessage
            {
                Version = new Version("1.6.0.0"),
                MessagesAreKeys = true,
                Messages = new string[]
                {
                    "LBR_160_NT1",
                    "LBR_160_NT2",
                    "LBR_160_NT3",
                    "LBR_160_NT4",
                },
            },
            new WhatsNewMessage
            {
                Version = new Version("1.5.4.0"),
                MessagesAreKeys = true,
                Messages = new string[]
                {
                    "LBR_154_NT1",
                    "LBR_154_NT2",
                },
            },
            new WhatsNewMessage
            {
                Version = new Version("1.5.2.0"),
                MessagesAreKeys = true,
                Messages = new string[]
                {
                    "LBR_152_NT1",
                    "LBR_152_NT2",
                    "LBR_152_NT3",
                },
            },
            new WhatsNewMessage
            {
                Version = new Version("1.5.1.0"),
                MessagesAreKeys = true,
                Messages = new string[]
                {
                    "LBR_151_NT1",
                },
            },
            new WhatsNewMessage
            {
                Version = new Version("1.5.0.0"),
                MessagesAreKeys = true,
                Messages = new string[]
                {
                    "LBR_150_NT1",
                    "LBR_150_NT2",
                },
            },
        };
    }
}