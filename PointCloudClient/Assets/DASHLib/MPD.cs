using System.Collections.Generic;
using System.Xml;
using System;
using System.Linq;
using UnityEngine;
namespace KOM.DASHLib
{
    /// <summary>Holds meta data about a DASH stream.</summary>
    public class MPD
    {
        /// <summary>
        /// The base URI for all downloads.
        /// </summary>
        public Uri BaseURL;
        
        /// <summary>
        /// A list of media periods forming the media.
        /// </summary>
        public List<Period> Periods = new List<Period>();

        /// <summary>
        /// The total duration of the media in milliseconds.
        /// </summary>
        public long MediaPresentationDuration { get { return Periods.Sum(period => period.Duration);  } }

        /// <summary>Creates a new <c>MPD</c> object from a xml formated string.</summary>
        public MPD(string mpd)
        {
            this.ParseMPD(mpd);
        }

        /// <summary>
        /// Parses the MPD <c>string</c> and extracts meta data. 
        /// </summary>
        private void ParseMPD(string mpd)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(mpd);

            XmlElement root = xml.DocumentElement;
            this.BaseURL = new Uri(root.SelectNodes("BaseURL")[0].InnerText);

            XmlNodeList xmlPeriods = root.SelectNodes("Period");
            foreach (XmlNode xmlPeriod in xmlPeriods)
            {
                double start = XmlConvert.ToTimeSpan(xmlPeriod.Attributes["start"].Value).TotalSeconds;
                double end = XmlConvert.ToTimeSpan(xmlPeriod.Attributes["end"].Value).TotalSeconds;
                Period mpdPeriod = new Period((long)(start*1000), (long)(end*1000));
                XmlNodeList xmlAdaptationSets = xmlPeriod.SelectNodes("AdaptationSet");
                foreach (XmlNode xmlAdaptationSet in xmlAdaptationSets)
                {
                    AdaptationSet mpdAdaptationSet = new AdaptationSet(xmlAdaptationSet.Attributes["id"].Value, xmlAdaptationSet.Attributes["mimeType"].Value, mpdPeriod);
                    XmlNodeList xmlRepresentations = xmlAdaptationSet.SelectNodes("Representation");
                    foreach (XmlNode xmlRepresentation in xmlRepresentations)
                    {
                        IRepresentation mpdRepresentation = new GenericRepresentation(
                            xmlRepresentation.Attributes["id"].Value,
                            new Uri(this.BaseURL, xmlRepresentation.SelectNodes("BaseURL")[0].InnerText),
                            long.Parse(xmlRepresentation.Attributes["bandwidth"].Value),
                            mpdAdaptationSet
                        );
                        mpdAdaptationSet.Representations.Add(mpdRepresentation);
                    }
                    mpdAdaptationSet.Selection = mpdAdaptationSet.Representations.First();
                    mpdPeriod.AdaptationSets.Add(mpdAdaptationSet);
                }
                this.Periods.Add(mpdPeriod);
            }
        }

        /// <summary>
        /// Returns the period at the specified time in seconds.
        /// </summary>
        /// <param name="time">The time in seconds of the desired period.</param>
        /// <returns>
        /// The period at the specified time or <c>null</c> if it was not found.
        /// </returns>
        public Period GetPeriod(double time)
        {
            Period p = null;
            foreach (Period period in this.Periods)
            {
                if (time >= period.Start && time <= period.End)
                {
                    return period;
                }
                p = period;
            }
            return p;
        }
    }
}