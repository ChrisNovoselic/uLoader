using uLoaderCommon;

namespace SrcMST
{
    public class PlugIn : PlugInULoader
    {
        public PlugIn()
            : base()
        {
            _Id = 1003;

            registerType(10303, typeof(SrcMSTASUTPIDT5tg1sql));
            registerType(10304, typeof(SrcMSTKKSNAMEsql));
            registerType(10307, typeof(SrcMSTKKSNAMEtoris));
            registerType(10308, typeof(SrcMSTASUTPIDT5tg6sql));
            registerType(10314, typeof(SrcMSTASUTPIdT5tg1Dsql));
            registerType(10313, typeof(SrcMSTASUTPIdT5tg6Dsql));
        }

        public override void OnEvtDataRecievedHost(object obj)
        {
            base.OnEvtDataRecievedHost(obj);
        }
    }
}
