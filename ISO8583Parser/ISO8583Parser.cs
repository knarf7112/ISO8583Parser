using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using IM.ISO8583.Utility;
using Kms.Crypto;
using OL_Autoload_Lib;
using Common.Logging;

namespace ALOLAsync
{
    public class ISO8583Parser
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ISO8583Parser));

        #region Field 宣告轉換ISO8583用的演算法物件

        Iso8583InfoGetter iso8583InfoGetter;
        Iso8583InfoGetter df61InfoGetter;

        BitWorker bitWorker;
        BitWorker df61BitWorker;

        BitMapHelper bitMapHelper;

        MainMsgWorker mainMsgWorker;
        Df61MsgWorker df61MsgWorker;
        #endregion

        public ISO8583Parser()
        {
            //初始化演算法物件
            iso8583InfoGetter = new Iso8583InfoGetter("IM.ISO8583.Utility.Config.iso8583Fn.xml",
                                                     @"//Message[@name='Common' and @peer='Common']");
            df61InfoGetter = new Iso8583InfoGetter("IM.ISO8583.Utility.Config.iso8583Fn.xml",
                                                  @"//Message[@name='DF61' and @peer='Common']");

            bitWorker = new BitWorker(iso8583InfoGetter);
            df61BitWorker = new BitWorker(df61InfoGetter);

            bitMapHelper = new BitMapHelper()
            {
                BitMapper = new BitMapper() { HexConverter = new HexConverter() },
                HexConverter = new HexConverter()
            };

            mainMsgWorker = new MainMsgWorker()
            {
                BitMapHelper = bitMapHelper,
                BitWorker = bitWorker
            };

            df61MsgWorker = new Df61MsgWorker()
            {
                BitMapHelper = bitMapHelper,
                Df61BitWorker = df61BitWorker
            };
        }

        #region Parse Message

        /// <summary>
        /// 電文轉物件(0110/0130 | 0430 | 0800 | 0302)
        /// </summary>
        /// <typeparam name="T">(0110/0130/0430(AutoloadRqt_2Bank) | 0800(Sign_Domain) | 0302(AutoloadRqt_FBank))</typeparam>
        /// <param name="messageType">轉換的格式</param>
        /// <param name="msgString">電文字串</param>
        /// <returns>AutoloadRqt_2Bank/Sign_Domain/AutoloadRqt_FBank POCO(要自己輸入要轉哪種型別)</returns>
        public T ParseMsg<T>(string messageType, string msgString)
        {
            try
            {
                switch (messageType)
                {
                    case "0110":
                    case "0130":
                        return (T)ParseALOLResponse(msgString);
                    case "0430":
                        return (T)ParseRALOLResponse(msgString);
                    case "0800":
                        return (T)ParseSignRequest(msgString);
                    case "0302":
                        return (T)ParseLossReportOrAddRejectListRequest(msgString);
                    default:
                        throw new Exception("[ParseMsg<T>] Message Type not defined:" + messageType);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 電文轉物件(0110/0130 | 0430 | 0800 | 0302)~~自己轉型吧
        /// </summary>
        /// <typeparam name="T">(0110/0130/0430(AutoloadRqt_2Bank) | 0800(Sign_Domain) | 0302(AutoloadRqt_FBank))</typeparam>
        /// <param name="messageType">轉換的格式</param>
        /// <param name="msgString">電文字串</param>
        /// <returns>AutoloadRqt_2Bank/Sign_Domain/AutoloadRqt_FBank POCO(要自己輸入要轉哪種型別)</returns>
        public object ParseMsg(string messageType, string msgString)
        {
            try
            {
                log.Debug("messageType:" + messageType);
                switch (messageType)
                {
                    case "0110":
                    case "0130":
                        return ParseALOLResponse(msgString);
                    case "0430":
                        return ParseRALOLResponse(msgString);
                    case "0800":
                        return ParseSignRequest(msgString);
                    case "0302":
                        return ParseLossReportOrAddRejectListRequest(msgString);
                    default:
                        throw new Exception("[ParseMsg<T>] Message Type not defined:" + messageType);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private object ParseLossReportOrAddRejectListRequest(string msgString)
        {
            try
            {
                string iCashCode = string.Empty;
                
                AutoloadRqt_FBank autoloadRqt_FBankRequest = new AutoloadRqt_FBank();
                log.Debug("開始轉換成 連線掛失/取消掛失/增加拒絕授權名單 Request物件");

                MsgContext msgContextMain = this.mainMsgWorker.Parse(msgString);
                log.Debug("轉換完成 開始Mapping物件資料");
                if (msgContextMain.FromTo.Length != 8)
                {
                    throw new Exception("Message Header length is not 8");
                }
                iCashCode = msgContextMain.FromTo.Substring(4, msgContextMain.FromTo.Length - 4 );       //4 bytes
                if (!iCashCode.Equals("8888"))
                {
                    throw new Exception("Message Header formatter is Error:" + iCashCode + "(iCash code:8888)");
                };
                autoloadRqt_FBankRequest.BANK_CODE = msgContextMain.FromTo.Substring(0, 4);                             //4 bytes   //銀行代碼
                autoloadRqt_FBankRequest.MESSAGE_TYPE = msgContextMain.Mti;                                             //4 bytes   //Message Type
                IList<IsoField> fList = msgContextMain.FieldList.ToList();
                foreach (IsoField field in fList)
                {
                    if (field.FNo == 2) { autoloadRqt_FBankRequest.ICC_NO = field.FData; continue; }                    //16 bytes  //卡號
                    if (field.FNo == 3) { autoloadRqt_FBankRequest.PROCESSING_CODE = field.FData; continue; }           // 6 bytes  //Trans Type
                    if (field.FNo == 7) { autoloadRqt_FBankRequest.TRANS_DATETIME = field.FData; continue; }            //10 bytes  //交易日期時間
                    if (field.FNo == 11) { autoloadRqt_FBankRequest.STAN = field.FData; continue; }                     // 6 bytes  //STAN
                    if (autoloadRqt_FBankRequest.PROCESSING_CODE == "990176" && field.FNo == 14)
                    { 
                        autoloadRqt_FBankRequest.VALID_DATE = field.FData; continue;                                    // 4 bytes //卡片有效日期"YYMM"
                    }
                    if (field.FNo == 37) { autoloadRqt_FBankRequest.RRN = field.FData; continue; }                      //12 bytes //RRN
                    if (field.FNo == 91) { autoloadRqt_FBankRequest.FILE_UPDATE_CODE = field.FData; continue; }         // 1 bytes //File Update Code (1/3)
                }

                return autoloadRqt_FBankRequest;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private object ParseSignRequest(string msgString)
        {
            try
            {
                string iCashCode = string.Empty;
                log.Debug("開始轉換成 Sign On/Off Request物件");
                Sign_Domain sign_Domain = new Sign_Domain();

                MsgContext msgContextMain = this.mainMsgWorker.Parse(msgString);
                log.Debug("轉換完成 開始Mapping物件資料");
                if (msgContextMain.FromTo.Length != 8)
                {
                    throw new Exception("Message Header length is not 8");
                }
                iCashCode = msgContextMain.FromTo.Substring(4, (msgContextMain.FromTo.Length - 4));       //4 bytes
                if (!iCashCode.Equals("8888"))
                {
                    throw new Exception("Message Header formatter is Error:" + iCashCode + "(iCash code:8888)");
                };
                sign_Domain.BankCode = msgContextMain.FromTo.Substring(0, 4);                       //4 bytes   //銀行代碼
                sign_Domain.COM_Type = msgContextMain.Mti;                                          //4 bytes   //Message Type
                IList<IsoField> fList = msgContextMain.FieldList.ToList();
                foreach (IsoField field in fList)
                {
                    if (field.FNo == 7) { sign_Domain.transDateTime = field.FData; continue; }      //10 bytes  //時間
                    if (field.FNo == 11) { sign_Domain.traceNumber = field.FData; continue; }       // 6 bytes  //STAN
                    if (field.FNo == 39) { sign_Domain.RC = field.FData; continue; }                // 2 bytes  //Return Code(00(正常)/N0(代行))
                    if (field.FNo == 70) { sign_Domain.infCode = field.FData; continue; }           // 3 bytes  //Network management information code(071/072/301)
                }

                return sign_Domain;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private object ParseRALOLResponse(string msgString)
        {
            try
            {
                string iCashCode = string.Empty;
                log.Debug("開始轉換 自動加值沖正授權 Response物件");
                AutoloadRqt_2Bank autoloadRqt_2BankResponse = new AutoloadRqt_2Bank();
                autoloadRqt_2BankResponse.ORI_dtat = new ORI_DATA();

                MsgContext msgContextMain = this.mainMsgWorker.Parse(msgString);
                log.Debug("轉換完成 開始Mapping物件資料");
                if (msgContextMain.FromTo.Length != 8)
                {
                    throw new Exception("Message Header length is not 8");
                }
                iCashCode = msgContextMain.FromTo.Substring(4, (msgContextMain.FromTo.Length - 4));//4 bytes
                if (!iCashCode.Equals("8888"))
                {
                    throw new Exception("Message Header formatter is Error:" + iCashCode + "(iCash code:8888)");
                };

                autoloadRqt_2BankResponse.BANK_CODE = msgContextMain.FromTo.Substring(0, 4);                    //4 bytes   //銀行代碼
                autoloadRqt_2BankResponse.MESSAGE_TYPE = msgContextMain.Mti;                                    //4 bytes   //格式
                IList<IsoField> fList = msgContextMain.FieldList.ToList();
                foreach (IsoField field in fList)
                {
                    if (field.FNo == 2) { autoloadRqt_2BankResponse.ICC_NO = field.FData; continue; }           //16 bytes  //卡號
                    if (field.FNo == 3) { autoloadRqt_2BankResponse.PROCESSING_CODE = field.FData; continue; }  // 6 bytes  //Trans Type
                    if (field.FNo == 4) { autoloadRqt_2BankResponse.AMOUNT = field.FData; continue; }           //12 bytes  //
                    if (field.FNo == 7) { autoloadRqt_2BankResponse.TRANS_DATETIME = field.FData; continue; }   //10 bytes
                    if (field.FNo == 11) { autoloadRqt_2BankResponse.STAN = field.FData; continue; }            // 6 bytes
                    if (field.FNo == 32) { autoloadRqt_2BankResponse.STORE_NO = field.FData; continue; }        //("st")2+10 bytes
                    if (field.FNo == 37) { autoloadRqt_2BankResponse.RRN = field.FData; continue; }             //12 bytes
                    if (field.FNo == 39) { autoloadRqt_2BankResponse.RC = field.FData; continue; }              // 2 bytes
                    if (field.FNo == 41) { autoloadRqt_2BankResponse.POS_NO = field.FData; continue; }          // 8 bytes
                    if (field.FNo == 42) { autoloadRqt_2BankResponse.MERCHANT_NO = field.FData; continue; }     //15 bytes
                    if (field.FNo == 90)
                    {
                        autoloadRqt_2BankResponse.ORI_dtat.MESSAGE_TYPE = field.FData.Substring(0, 4);          // 4 bytes
                        autoloadRqt_2BankResponse.ORI_dtat.TRANSACTION_DATE = field.FData.Substring(4, 10);     //10 bytes
                        autoloadRqt_2BankResponse.ORI_dtat.STAN = field.FData.Substring(14, 6);                 // 6 bytes
                        autoloadRqt_2BankResponse.ORI_dtat.STORE_NO = field.FData.Substring(20, 8);             // 8 bytes
                        autoloadRqt_2BankResponse.ORI_dtat.RRN = field.FData.Substring(28, 12);                 //12 bytes
                        continue;
                    }
                }
                log.Debug("轉換完成");
                return autoloadRqt_2BankResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 自動加值/代行取得授權的Response電文轉物件
        /// </summary>
        /// <param name="msgString">Response電文字串</param>
        /// <returns>自動加值/代行取得授權的Response物件</returns>
        private object ParseALOLResponse(string msgString)
        {
            try
            {
                string iCashCode = string.Empty;
                log.Debug("開始轉換 自動加值/代行授權 Response物件");
                AutoloadRqt_2Bank autoloadRqt_2BankResponse = new AutoloadRqt_2Bank();

                MsgContext msgContextMain = this.mainMsgWorker.Parse(msgString);
                log.Debug("轉換完成 開始Mapping物件資料");
                if(msgContextMain.FromTo.Length != 8)
                {
                    throw new Exception("Message Header length is not 8");
                }
                iCashCode = msgContextMain.FromTo.Substring(4, (msgContextMain.FromTo.Length - 4));                   //4 bytes
                if(!iCashCode.Equals("8888"))
                {
                    throw new Exception("Message Header formatter is Error:" + iCashCode +"(iCash code:8888)");
                };
                autoloadRqt_2BankResponse.BANK_CODE = msgContextMain.FromTo.Substring(0, 4);                    //4 bytes
                autoloadRqt_2BankResponse.MESSAGE_TYPE = msgContextMain.Mti;                                    //4 bytes
                IList<IsoField> fList = msgContextMain.FieldList.ToList();
                foreach (IsoField field in fList)
                {
                    if (field.FNo == 2) { autoloadRqt_2BankResponse.ICC_NO = field.FData; continue; }           //16 bytes
                    if (field.FNo == 3) { autoloadRqt_2BankResponse.PROCESSING_CODE = field.FData; continue; }  // 6 bytes
                    if (field.FNo == 4) { autoloadRqt_2BankResponse.AMOUNT = field.FData; continue; }           //12 bytes
                    if (field.FNo == 7) { autoloadRqt_2BankResponse.TRANS_DATETIME = field.FData; continue; }   //10 bytes
                    if (field.FNo == 11) { autoloadRqt_2BankResponse.STAN = field.FData; continue; }            // 6 bytes
                    if (field.FNo == 32) { autoloadRqt_2BankResponse.STORE_NO = field.FData; continue; }        //("st")2+10 bytes
                    if (field.FNo == 37) { autoloadRqt_2BankResponse.RRN = field.FData; continue; }             //12 bytes
                    if (field.FNo == 39) { autoloadRqt_2BankResponse.RC = field.FData; continue; }              // 2 bytes
                    if (field.FNo == 41) { autoloadRqt_2BankResponse.POS_NO = field.FData; continue; }          // 8 bytes
                    if (field.FNo == 42) { autoloadRqt_2BankResponse.MERCHANT_NO = field.FData; continue; }     //15 bytes
                }

                return autoloadRqt_2BankResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Build Message
        /// <summary>
        /// 物件轉換成電文(授權/代行授權/沖正授權)
        /// </summary>
        /// <param name="messageType">要求格式(0100/0120/0121 | 0420/0421 | 0810 | 0312)</param>
        /// <param name="requestToBank">要求(授權/代行授權 | 沖正授權 | Sign On/Off/Echo | )物件</param>
        /// <returns>Response to Bank電文</returns>
        public string BuildMsg(string messageType, AutoloadRqt_2Bank requestToBank = null, AutoloadRqt_FBank responseFromBank = null,Sign_Domain responseSign = null)
        {
            try
            {       
                switch (messageType)
                {
                    case "0100":
                    case "0120":
                    case "0121":
                        return ConvertALOLRequest(requestToBank);
                    case "0420":
                    case "0421":
                        return ConvertRALOLRequest(requestToBank);
                    case "0810":
                        return ConvertSignResponse(responseSign);
                    case "0312":
                        return ConvertLossReportOrAddRejectList(responseFromBank);
                    default:
                        throw new Exception("[GetRequestMsg] Message Type not defined:" + messageType);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 轉換連線掛失/掛失取消/新增拒絕代行授權名單 Response 電文
        /// </summary>
        /// <param name="responseFromBank">POCO物件</param>
        /// <returns>Response電文字串</returns>
        private string ConvertLossReportOrAddRejectList(AutoloadRqt_FBank responseFromBank)
        {
            try
            {
                if (responseFromBank == null)
                {
                    throw new Exception("AutoloadRqt_FBank POCO is null");
                }
                log.Debug("開始轉換 連線掛失/掛失取消/增加拒絕代行授權 Response電文");
                //Message Header
                string fromTo = "8888" + responseFromBank.BANK_CODE.PadLeft(4, '0');//8888表示愛金卡機構代號
                //initial BitMap List
                string[] srcList = new string[65];//129];
                for (int i = 0; i < srcList.Length; i++)
                {
                    srcList[i] = "";
                }
                srcList[2] = responseFromBank.ICC_NO;               //16碼 //"0417149984000007"//"0000000000000000";
                srcList[3] = responseFromBank.PROCESSING_CODE;      // 6碼//"990174"//"990174";
                srcList[7] = responseFromBank.TRANS_DATETIME;       //10碼//"0115135959"//"0128180006";
                srcList[11] = responseFromBank.STAN;                // 6碼//"005009"//"666666";
                //連線掛失有Field 14,增加拒絕代行授權名單沒有Field 14
                if (responseFromBank.PROCESSING_CODE == "990176")
                {
                    srcList[14] = responseFromBank.VALID_DATE;      // 4碼
                }
                srcList[37] = responseFromBank.RRN;                 //12碼//"501513005009"//"502818666666";
                srcList[39] = responseFromBank.RC;                  // 2碼//"00000001"//"00000001";

                //init Field 61
                string[] srcListDf61 = new string[65];
                for (int i = 0; i < srcListDf61.Length; i++)
                {
                    srcListDf61[i] = "";
                }
                srcListDf61[8] = responseFromBank.ICC_info.TX_DATETIME;//14碼//"20151231235959";
                MsgContext msgContextDf61 = df61MsgWorker.Build(null, null, srcListDf61);

                srcList[61] = msgContextDf61.SrcMessage;//"0100000000000000"(second bimap) + "20151231235959"(Tx Date Time)

                MsgContext msgResult = mainMsgWorker.Build(fromTo, responseFromBank.MESSAGE_TYPE, srcList);//"88880000","0312"
                log.Debug("訊息(Length:" + msgResult.SrcMessage.Length + "): " + msgResult.SrcMessage);
                return msgResult.SrcMessage;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 轉換成Sign On/Off Resposne電文
        /// </summary>
        /// <param name="responseSign">response物件</param>
        /// <returns>Sign Response 電文字串</returns>
        private string ConvertSignResponse(Sign_Domain responseSign)
        {
            try
            {
                if (responseSign == null)
                {
                    throw new Exception("Sign_Domain POCO is null");
                }
                log.Debug("開始轉換Sign On/Off Response電文");
                //Message Header
                string fromTo = "8888" + responseSign.BankCode.PadLeft(4, '0');//8888表示愛金卡機構代號
                //initial BitMap List
                string[] srcList = new string[129];
                for (int i = 0; i < srcList.Length; i++)
                {
                    srcList[i] = "";
                }
                srcList[7] = responseSign.transDateTime;//10碼//"0115135959"//"0128180006";
                srcList[11] = responseSign.traceNumber; // 6碼//"005009"//"666666"//STAN;
                srcList[39] = responseSign.RC;          // 2碼;
                srcList[70] = responseSign.infCode;     // 3碼//"000000022555003"//"000000022555003";
                MsgContext msgResult = mainMsgWorker.Build(fromTo, responseSign.COM_Type, srcList);
                log.Debug("Sign Response Msg訊息(Length:" + msgResult.SrcMessage.Length + "): " + msgResult.SrcMessage);
                return msgResult.SrcMessage;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 沖正Request
        /// </summary>
        /// <param name="requestToBank">沖正物件</param>
        /// <returns>沖正電文</returns>
        private string ConvertRALOLRequest(AutoloadRqt_2Bank requestToBank)
        {
            try
            {
                log.Debug("開始轉換沖正物件");
                //Message Header
                string fromTo = "8888" + requestToBank.BANK_CODE.PadLeft(4, '0');//8888表示愛金卡機構代號
                //initial BitMap List
                string[] srcList = new string[129];
                for (int i = 0; i < srcList.Length; i++)
                {
                    srcList[i] = "";
                }
                srcList[2] = requestToBank.ICC_NO;          //16碼//"0417149984000007"//"0000000000000000";
                srcList[3] = requestToBank.PROCESSING_CODE; // 6碼//"990174"//"990174";
                srcList[4] = requestToBank.AMOUNT;//"000000000500"//"000000000055";
                srcList[7] = requestToBank.TRANS_DATETIME;//"0115135959"//"0128180006";
                srcList[11] = requestToBank.STAN;//"005009"//"666666";
                srcList[32] = requestToBank.STORE_NO;//"st00896159"// "st00000001";
                srcList[37] = requestToBank.RRN;//"501513005009"//"502818666666";
                srcList[41] = requestToBank.POS_NO;//"00000001"//"00000001";
                srcList[42] = requestToBank.MERCHANT_NO;//"000000022555003"//"000000022555003";

                //init Field 61
                string[] srcListDf61 = new string[65];
                for (int i = 0; i < srcListDf61.Length; i++)
                {
                    srcListDf61[i] = "";
                }
                srcListDf61[9] = requestToBank.ICC_info.RETURN_CODE;//"00000000"//"00000000";
                MsgContext msgContextDf61 = df61MsgWorker.Build(null, null, srcListDf61);

                srcList[61] = msgContextDf61.SrcMessage;//"808000000000000000000000" //"008000000000000000000000";
                srcList[90] = requestToBank.ORI_dtat.MESSAGE_TYPE + requestToBank.ORI_dtat.TRANSACTION_DATE + requestToBank.ORI_dtat.STAN + requestToBank.ORI_dtat.STORE_NO + requestToBank.ORI_dtat.RRN + "  ";
                //"0120" + "0115135959" + "005002" + "00896159" + "501513005002" + "  ";
                //"0100" + "0128183005" + "555555" + "00000001" + "502818666666" + "  ";

                MsgContext msgResult = mainMsgWorker.Build(fromTo, requestToBank.MESSAGE_TYPE, srcList);//"88880000","0420"
                log.Debug("轉換後的銀行訊息(Length:" + msgResult.SrcMessage.Length + "): " + msgResult.SrcMessage);
                return msgResult.SrcMessage;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 請求授權(自動加值/代行)
        /// </summary>
        /// <param name="requestToBank">請求授權物件</param>
        /// <returns>請求授權電文</returns>
        private string ConvertALOLRequest(AutoloadRqt_2Bank requestToBank)
        {
            try
            {
                if (requestToBank == null)
                {
                    throw new Exception("AutoloadRqt_2Bank is null!!");
                }
                log.Debug("開始轉換要求授權物件");
                //Message Header
                string fromTo = "8888" + requestToBank.BANK_CODE.PadLeft(4, '0');//8888表示愛金卡機構代號
                //initial BitMap List
                string[] srcList = new string[65];//129];
                for (int i = 0; i < srcList.Length; i++)
                {
                    srcList[i] = "";
                }
                srcList[2] = requestToBank.ICC_NO;//"0417149984000007"//"0000000000000000";
                srcList[3] = requestToBank.PROCESSING_CODE;//"990174"//"990174";
                srcList[4] = requestToBank.AMOUNT;//"000000000500"//"000000000055";
                srcList[7] = requestToBank.TRANS_DATETIME;//"0115135959"//"0128180006";
                srcList[11] = requestToBank.STAN;//"005009"//"666666";
                srcList[32] = requestToBank.STORE_NO;//"st00896159"// "st00000001";
                srcList[37] = requestToBank.RRN;//"501513005009"//"502818666666";
                srcList[41] = requestToBank.POS_NO;//"00000001"//"00000001";
                srcList[42] = requestToBank.MERCHANT_NO;//"000000022555003"//"000000022555003";

                //init Field 61
                string[] srcListDf61 = new string[65];
                for (int i = 0; i < srcListDf61.Length; i++)
                {
                    srcListDf61[i] = "";
                }
                srcListDf61[3] = requestToBank.ICC_info.STORE_NO;   // 8碼
                srcListDf61[4] = requestToBank.ICC_info.REG_ID;     // 3碼
                srcListDf61[8] = requestToBank.ICC_info.TX_DATETIME;//14碼
                srcListDf61[10] = requestToBank.ICC_info.ICC_NO;    //16碼
                srcListDf61[11] = requestToBank.ICC_info.AMT;       // 8碼,"00000000"//"00000000";
                srcListDf61[35] = requestToBank.ICC_info.NECM_ID;   //20碼
                MsgContext msgContextDf61 = df61MsgWorker.Build(null, null, srcListDf61);

                srcList[61] = msgContextDf61.SrcMessage;            //16碼"808000000000000000000000" //"008000000000000000000000";
                //"0120" + "0115135959" + "005002" + "00896159" + "501513005002" + "  ";
                //"0100" + "0128183005" + "555555" + "00000001" + "502818666666" + "  ";

                MsgContext msgResult = mainMsgWorker.Build(fromTo, requestToBank.MESSAGE_TYPE, srcList);//"88880000","0420"
                log.Debug("轉換後的銀行訊息(Length:" + msgResult.SrcMessage.Length + "): " + msgResult.SrcMessage);
                return msgResult.SrcMessage;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}

