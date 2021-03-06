﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public interface IGeneralDataService
    {
        List<IService> Services { get; set; }
        IService GetService(string serviceName);
        void Start();
        void Stop();
    }
}
