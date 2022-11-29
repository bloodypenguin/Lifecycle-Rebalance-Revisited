// <copyright file="WG_XMLBaseVersion.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard) and Whitefang Greytail. All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented
namespace LifecycleRebalance
{
    using System.Xml;

    public abstract class WG_XMLBaseVersion
    {
        public WG_XMLBaseVersion()
        {
        }

        public abstract void ReadXML(XmlDocument doc);

        public abstract bool WriteXML(string fullPathFileName);
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented