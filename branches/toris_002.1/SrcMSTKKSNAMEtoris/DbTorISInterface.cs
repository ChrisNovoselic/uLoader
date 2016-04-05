using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data;
using System.Threading;
using System.Data.Common;
using System.Data.OleDb;

using HClassLibrary;

namespace SrcMSTKKSNAMEtoris
{
    public class DbTorISInterface : DbInterface
    {
        public DbTorISInterface(string name)
            : base(name)
        {
        }

        public override void SetConnectionSettings(object mcHostName, bool bStarted)
        {
            lock (lockConnectionSettings) // ��������� �������� ����������� � ����������� ����� ��� ��������������� - ��������� ��������
            {
                m_connectionSettings = mcHostName;

                needReconnect = true;
            }

            SetConnectionSettings();
        }


        protected override bool Connect()
        {
            if (!(((string)m_connectionSettings).Length > 0))
                return false;
            else
                ;

            bool result = false, bRes = false;

            try
            {
                if (bRes == true)
                    return bRes;
                else
                    bRes = true;
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, "DbMCInterface::Connect ()");
            }

            lock (lockConnectionSettings)
            {
                if (needReconnect == true) // ���� ����� �������� � ������ ����� �������� ���� �������� ���������, �� ����������� �� ������� ����������� �� ������
                    return false;
                else
                    ;
            }

            try
            {
                ModesApiFactory.Initialize((string)m_connectionSettings);

                Logging.Logg().Debug("���������� � Modes-Centre (" + (string)m_connectionSettings + ")", Logging.INDEX_MESSAGE.NOT_SET);
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, "������ ���������� � Modes-Centre (" + (string)m_connectionSettings + ")");
            }

            bRes = 
            result =
            ModesApiFactory.IsInitilized;

            if (bRes == true)
            {
                m_MCApi = ModesApiFactory.GetModesApi();
                m_MCTimeSlice = m_MCApi.GetModesTimeSlice(DateTime.Now.Date.LocalHqToSystemEx(), SyncZone.First, TreeContent.PGObjects, true);
                m_listPFI = m_MCApi.GetPlanFactors();
            }
            else
                ;

            Logging.Logg().Debug("���������� � Modes-Centre(" + (string)m_connectionSettings + ") - �����", Logging.INDEX_MESSAGE.NOT_SET);

            return result;
        }

        protected override bool Disconnect()
        {
            bool result = true
                , bRes = false;

            return result;
        }

        public override void Disconnect(out int err)
        {
            err = 0;
        }

        Modes.BusinessLogic.IGenObject addIGO (int idInnner)
        {
            foreach (Modes.BusinessLogic.IGenObject igo in m_MCTimeSlice.GenTree)
            {
                Console.WriteLine(igo.Description + " [" + igo.GenObjType.Description + "]");
                //ProcessParams(IGO);
                ProcessChilds(igo, 1, idInnner);
            }

            if ((m_listIGO.Count > 0) && (m_listIGO[m_listIGO.Count - 1].IdInner == idInnner))
                return m_listIGO[m_listIGO.Count - 1];
            else
                return null;
        }

        Modes.BusinessLogic.IGenObject findIGO(int idInnner)
        {
            foreach (Modes.BusinessLogic.IGenObject igo in m_listIGO)
            {
                if (igo.IdInner == idInnner)
                    return igo;
                else
                    ;
            }

            return null;
        }

        public class PPBR_Record
        {
            public DateTime date_time,
                            wr_date_time;
            public string PBR_number;

            public double? pbr;
            public double? pmin;
            public double? pmax;

            //public int? idInner; //??? ��� TEC5_TG36 2 ������� (TG34 + TG56), �.�. 2 IGO

            public PPBR_Record()
            {
            }
        }

        protected override bool GetData(DataTable table, object query)
        {
            bool result = false;
            int i = -1;
            Modes.BusinessLogic.IGenObject igo;

            table.Reset();
            table.Locale = System.Globalization.CultureInfo.CurrentCulture;

            string [] args = ((string)query).Split (';');

            Logging.Logg().Debug("DbMCInterface::GetData () - " + query + ")", Logging.INDEX_MESSAGE.NOT_SET);

            switch (args[0])
            {
                case "InitIGO":
                    //InitIGO (arg);
                    break;
                case "PPBR":
                    table.Columns.Add("DATE_PBR", typeof (DateTime));
                    table.Columns.Add("WR_DATE_TIME", typeof(DateTime));
                    table.Columns.Add("PBR", typeof(double));
                    table.Columns.Add("Pmin", typeof(double));
                    table.Columns.Add("Pmax", typeof(double));
                    table.Columns.Add("PBR_NUMBER", typeof(string));
                    //table.Columns.Add("ID_COMPONENT", typeof(Int32));
                    
                    ////������� �1
                    //PPBR_Record ppbr = null;
                    //SortedList<DateTime, PPBR_Record> srtListPPBR = new SortedList<DateTime,PPBR_Record> ();
                    //������� �2
                    DataRow [] ppbr_rows = null;

                    DateTime date = DateTime.FromOADate (Double.Parse (args [2]));
                    string [] idsInner = args[1].Split(',');
                    bool valid = false;
                    int idInner = -1;

                    for (i = 0; i < idsInner.Length; i ++)
                    {
                        valid = Int32.TryParse(idsInner[i], out idInner);
                        
                        if (valid == false)
                            continue;
                        else
                            ;

                        igo = findIGO(idInner);

                        if (igo == null)
                            igo = addIGO (idInner);
                        else
                            ;

                        if (!(igo == null)) {
                            result = true;

                            IList<PlanValueItem> listPVI = m_MCApi.GetPlanValuesActual(date.LocalHqToSystemEx(), date.AddDays(1).LocalHqToSystemEx(), igo);

                            DateTime dateCurrent;

                            if (listPVI.Count == 0)
                                Console.WriteLine("    ��� ���������� ���������!");
                            else
                                ;

                            foreach (PlanValueItem pvi in listPVI.OrderBy(RRR => RRR.DT))
                            {
                                Console.WriteLine("    " + pvi.DT.SystemToLocalHqEx().ToString() + " " +
                                                            pvi.Type.ToString() + " [" + m_listPFI[pvi.ObjFactor].Description + "] " +
                                                            /*it.ObjName ��� id ������������� �������*/
                                                            m_listPFI[pvi.ObjFactor].Name + " =" + pvi.Value.ToString());

                                dateCurrent = pvi.DT.SystemToLocalHqEx();

                                //��������� ������ � ������� ����������� �� ��� �����
                                ////������� �1
                                //if (srtListPPBR.ContainsKey(dateCurrent))
                                //    ppbr = srtListPPBR.First(item => item.Key == dateCurrent).Value;
                                //else
                                //    ;

                                //������� �2
                                if (table.Rows.Count > 0)
                                    ppbr_rows = table.Select("DATE_PBR='" + dateCurrent.ToString() + "'");
                                else
                                    ;

                                //��������� ��������� ������
                                ////������� �1
                                //if (ppbr == null)
                                //{
                                //    ppbr = new PPBR_Record();
                                //    ppbr.date_time = dateCurrent;
                                //    ppbr.wr_date_time = DateTime.Now;
                                //    ppbr.PBR_number = pvi.Type.ToString();
                                //    //ppbr.idInner = igo.IdInner; //??? ��� TEC5_TG36 2 ������� (TG34 + TG56), �.�. 2 IGO

                                //    //����� ���������� ����� ���������� �������������� ��������� ������ - � ��������� �� �� ����� ������ ��������.
                                //    srtListPPBR.Add(dateCurrent, ppbr);
                                //}
                                //else
                                //    ;

                                //������� �2
                                if ((ppbr_rows == null) || (ppbr_rows.Length == 0))
                                {
                                    table.Rows.Add(new object[] { dateCurrent, DateTime.Now, 0.0, 0.0, 0.0, pvi.Type.ToString()/*, igo.IdInner*/});
                                    ppbr_rows = table.Select("DATE_PBR='" + dateCurrent.ToString() + "'");
                                }
                                else
                                    ;

                                ppbr_rows [0]["PBR_NUMBER"] = pvi.Type.ToString();

                                switch (m_listPFI[pvi.ObjFactor].Id)
                                {
                                    case 0:
                                        //else.ppbr.pbr = pvi.Value; //������� �1
                                        ppbr_rows [0]["PBR"] = (double)ppbr_rows [0]["PBR"] + pvi.Value; //������� �2
                                        break;
                                    case 1:
                                        //ppbr.pmin = pvi.Value; //������� �1
                                        ppbr_rows [0]["PMIN"] = (double)ppbr_rows [0]["PMIN"] + pvi.Value; //������� �2
                                        break;
                                    case 2:
                                        //ppbr.pmax = pvi.Value; //������� �1
                                        ppbr_rows [0]["PMAX"] = (double)ppbr_rows [0]["PMAX"] + pvi.Value; //������� �2
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        else
                            result = false; //igo == null
                    }
                    break;
                default:
                    break;
            }

            return result;
        }

        void ProcessChilds(Modes.BusinessLogic.IGenObject IGO, int Level, int idInner)
        {
            foreach (Modes.BusinessLogic.IGenObject IGOch in IGO.Children)
            {
                if (!(IGOch.GenObjType.Id == 15))      //������������ ���� ��� ��������� - �� ��� ��� �� ����������, �� �������� ���������
                {
                    Console.WriteLine(new System.String('-', Level) + IGOch.Description + " [" + IGOch.GenObjType.Description + "]  P:" + IGOch.VarParams.Count.ToString() + " Id:" + IGOch.Id.ToString() + " IdInner:" + IGOch.IdInner.ToString());
                    //ProcessParams(IGOch);
                    if ((IGOch.GenObjType.Id == 3) && (IGOch.IdInner == idInner))
                    {
                        //� ������������ ���� �������������� (id=1) ��� ���������� - ������ �������� ��������
                        m_listIGO.Add (IGOch);
                    }
                    else
                        ;

                    ProcessChilds(IGOch, Level + 1, idInner);
                }
                else
                    ;
            }
        }

        private bool InitIGO (string ids)
        {
            bool bRes = false;

            string [] arId = ids.Split (',');

            foreach (string id in arId)
            {
                foreach (Modes.BusinessLogic.IGenObject IGO in m_MCTimeSlice.GenTree)
                {
                    Console.WriteLine(IGO.Description + " [" + IGO.GenObjType.Description + "]");
                    //ProcessParams(IGO);
                    ProcessChilds(IGO, 1, Convert.ToInt32 (id));
                }
            }

            return bRes;
        }
    }
}
