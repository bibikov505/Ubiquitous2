using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace UB.Model
{
    [ServiceContract(CallbackContract = typeof(IOBSCallback))]
    public interface IOBSPluginService
    {
        [OperationContract]
        ImageData GetImage();
        
        [OperationContract]
        ImageData GetFirstImage();
        
        [OperationContract]
        void SetConfig(OBSPluginConfig config);
        
        [OperationContract]
        OBSPluginConfig GetConfig();

        [OperationContract]
        bool Subscribe();

        [OperationContract]
        bool Unsubscribe();
    }
}
