using uLoaderCommon;

namespace SrcKTS
{
    public class PlugIn : PlugInULoader
    {
        public PlugIn()
            : base()
        {
            _Id = 102;

            registerType(10202, typeof(SrcKTSTUsql));
            registerType(10209, typeof(SrcKTSTUDsql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
