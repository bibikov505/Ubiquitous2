using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    interface IOBSCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnImageChanged();
        [OperationContract(IsOneWay = true)]
        void OnConfigChanged();
    }
}
