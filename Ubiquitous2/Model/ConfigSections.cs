﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UB.Model
{
    [Serializable]
    [XmlRoot(ElementName = "ChatConfigs")]
    public class ConfigSections
    {
        [XmlElement]
        public List<ChatConfig> ChatConfigs;
    }
}