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
            registerType(10212, typeof(SrcKTSTUDsql_BL_2_5));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
