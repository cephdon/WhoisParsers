﻿// -----------------------------------------------------------------------
// <copyright file="RWhoisConsumer.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Geolocation.RWhois.Crawler
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using Microsoft.Geolocation.Whois.Parsers;
    using NetTools;
    using NLog;

    public class RWhoisConsumer : IObserver<RawWhoisSection>
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private IDisposable unsubscriber;

        private StreamWriter outFile;

        public RWhoisConsumer(string outFilePath)
        {
            this.outFile = new StreamWriter(outFilePath);
        }

        public virtual void Subscribe(IObservable<RawWhoisSection> provider)
        {
            if (provider != null)
            {
                this.unsubscriber = provider.Subscribe(this);
            }
        }

        public void OnCompleted()
        {
            this.outFile.Close();
            logger.Info("Done receiving data!");
            this.Unsubscribe();
        }

        public void OnError(Exception error)
        {
            this.outFile.Close();
        }

        public void OnNext(RawWhoisSection section)
        {
            if (section.Records != null)
            {
                StringBuilder rawNetwork;

                if (section.Records.TryGetValue("IP-Network", out rawNetwork))
                {
                    this.outFile.Write(section);
                    this.outFile.WriteLine();
                    this.outFile.Flush();

                    logger.Debug(string.Format(CultureInfo.InvariantCulture, "Received records for: {0}", IPAddressRange.Parse(rawNetwork.ToString())));
                }
                else
                {
                    logger.Debug("Received records but could not find IP-Network record");
                }
            }
            else
            {
                logger.Error("Received an item without records");
            }
        }

        public virtual void Unsubscribe()
        {
            this.unsubscriber.Dispose();
        }
    }
}