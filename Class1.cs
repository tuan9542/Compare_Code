public async Task<FRTUService_RES_DTO<object>> Integration_DuyetTyLeTieuChuanDinhGiaBanMayCu_ToPOS(int TicketId)
{
    var res = new FRTUService_RES_DTO<object>();
    try
    {
        string l__ProcessKey = "DuyetTyLeTieuChuanDinhGiaBanMayCu";

        string l__PhaseName = "POS xử lý";

        #region ===Check Input===
        if (TicketId <= 0)
        {
            res.Result = 0;
            res.Msg += " - Call " + MethodBase.GetCurrentMethod().Name + " Fail ( - TicketId: Phải lớn hơn 0)!";
            res.Data = null;
            return res;
        }
        #endregion ===Check Input===

        int l__TicketInfos__ProcessId = 0;

        #region ===Call FRTUService_Ticket_Detail===
        var l__Ticket_Detail = await FRTUService_Ticket_Detail__V2(new FRTUService_Ticket_Detail_REQ_DTO
        {
            Email = g__Email__System,
            Token = g__Token__System,
            Id = TicketId,
            GetHidden = true
        });
        if (l__Ticket_Detail.Result != 1)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Ticket_Detail Fail ( - Result: " + l__Ticket_Detail.Result + " - Msg: " + l__Ticket_Detail.Msg + ")!";
            res.Data = null;
            return res;
        }
        if (l__Ticket_Detail.Data == null)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Ticket_Detail Fail ( - Data: null)!";
            res.Data = null;
            return res;
        }

        var l__Ticket_Detail__Data = l__Ticket_Detail.Data;
        if (l__Ticket_Detail__Data.ResultCode != 200)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Ticket_Detail Fail ( - ResultCode: " + l__Ticket_Detail__Data.ResultCode + " - Message: " + l__Ticket_Detail__Data.Message + ")!";
            res.Data = null;
            return res;
        }
        if (l__Ticket_Detail__Data.TicketInfos.TicketId <= 0)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Ticket_Detail Fail ( - TicketInfos.TicketId: " + l__Ticket_Detail__Data.TicketInfos.TicketId + ")!";
            res.Data = null;
            return res;
        }
        if (l__Ticket_Detail__Data.TicketInfos.ProcessId <= 0)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Ticket_Detail Fail ( - TicketInfos.ProcessId: " + l__Ticket_Detail__Data.TicketInfos.ProcessId + ")!";
            res.Data = null;
            return res;
        }
        l__TicketInfos__ProcessId = l__Ticket_Detail__Data.TicketInfos.ProcessId;
        if (string.IsNullOrEmpty(l__Ticket_Detail__Data.TicketInfos.Current_Phase))
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Ticket_Detail Fail ( - TicketInfos.Current_Phase: null/rỗng)!";
            res.Data = null;
            return res;
        }
        if (l__Ticket_Detail__Data.TicketInfos.Current_Phase != l__PhaseName)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Ticket_Detail Fail ( - TicketInfos.Current_Phase: Không phải \"" + l__PhaseName + "\")!";
            res.Data = null;
            return res;
        }

        #endregion ===Call FRTUService_Ticket_Detail===

        #region ===Call FRTUService_Process_GetIdByProcessKey===
        var l__GetIdByProcessKey = await FRTUService_Process_GetIdByProcessKey__V2(l__ProcessKey, g__Email__System);
        if (l__GetIdByProcessKey.Result != 1)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Process_GetIdByProcessKey Fail ( - Result: " + l__GetIdByProcessKey.Result + " - Msg: " + l__GetIdByProcessKey.Msg + ")!";
            res.Data = null;
            return res;
        }
        if (l__GetIdByProcessKey.Data == null)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Process_GetIdByProcessKey Fail ( - Data: null)!";
            res.Data = null;
            return res;
        }

        var l__GetIdByProcessKey__Data = l__GetIdByProcessKey.Data;
        if (l__GetIdByProcessKey__Data.Result != 1)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Process_GetIdByProcessKey Fail ( - Data.Result: " + l__GetIdByProcessKey__Data.Result + " - Data.Msg: " + l__GetIdByProcessKey__Data.Msg + ")!";
            res.Data = null;
            return res;
        }
        if (l__GetIdByProcessKey__Data.ProcessId <= 0)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Process_GetIdByProcessKey Fail ( - Data.ProcessId: <= 0)!";
            res.Data = null;
            return res;
        }
        if (l__GetIdByProcessKey__Data.ProcessId != l__TicketInfos__ProcessId)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Process_GetIdByProcessKey Fail ( - ProcessId: Khác loại quy trình tích hợp)!";
            res.Data = null;
            return res;
        }
        #endregion ===Call FRTUService_Process_GetIdByProcessKey===

        #region ===Call FRTUService_Ticket_TicketWorkflowWithData===
        var l__TicketWorkflowWithData = await FRTUService_Ticket_TicketWorkflowWithData__V2(new FRTUService_Ticket_TicketWorkflowWithData_REQ_DTO
        {
            Email = g__Email__System,
            Token = g__Token__System,
            TicketId = TicketId
        });
        if (l__TicketWorkflowWithData.Result != 1)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Ticket_TicketWorkflowWithData Fail ( - Result: " + l__TicketWorkflowWithData.Result + " - Msg: " + l__TicketWorkflowWithData.Msg + ")!";
            res.Data = null;
            return res;
        }
        if (l__TicketWorkflowWithData.Data == null)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Ticket_TicketWorkflowWithData Fail ( - Data: null)!";
            res.Data = null;
            return res;
        }

        var l__TicketWorkflowWithData__Data = l__TicketWorkflowWithData.Data;
        if (l__TicketWorkflowWithData__Data.ResultCode != 200)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Ticket_TicketWorkflowWithData Fail ( - ResultCode: " + l__TicketWorkflowWithData__Data.ResultCode + ")!";
            res.Data = null;
            return res;
        }

        var l__Relationship_1020 = l__TicketWorkflowWithData__Data.Data.Relationships.Where(p => p.Id == -1020).FirstOrDefault();
        if (l__Relationship_1020 == null)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Ticket_TicketWorkflowWithData Fail ( - Data.Relationships: Id -1020 không tồn tại)!";
            res.Data = null;
            return res;
        }
        if (l__Relationship_1020.RelationshipId <= 0)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Ticket_TicketWorkflowWithData Fail ( - Data.Relationships: Id -1020 - RelationshipId: <= 0)!";
            res.Data = null;
            return res;
        }
        var l__StatusApprove = l__Relationship_1020.Detail.Individual.Where(p => p.DefinedId == -30000).FirstOrDefault().Value;

        var l__Relative_10 = l__TicketWorkflowWithData__Data.Data.Relatives.Where(p => p.Id == -10).FirstOrDefault();
        if (l__Relative_10 == null)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Ticket_TicketWorkflowWithData Fail ( - Data.Relatives: Id -10 không tồn tại)!";
            res.Data = null;
            return res;
        }
        if (string.IsNullOrEmpty(l__Relative_10.Summary))
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Ticket_TicketWorkflowWithData Fail ( - Data.Relatives: Id -10 - Summary: null/rỗng)!";
            res.Data = null;
            return res;
        }
        var l__Approver = l__Relative_10.Owner;


        #endregion ===Call FRTUService_Ticket_TicketWorkflowWithData===

        #region ===Call POS_DuyetTyLeTieuChuanDinhGiaBanMayCu_UpdateStatus===
        var l__PushInfoToPOS = await POS_Order_DuyetTyLeGiaBanMayCu(new POS_Order_DuyetTyLeGiaBanMayCu_REQ_DTO
        {
            ticketId = TicketId.ToString(),
            docentry = "",
            status = l__StatusApprove,
            user = l__Approver
        });
        if (l__PushInfoToPOS.Result != 1)
        {
            res.Result = 0;
            res.Msg += " - Call POS_Order_DuyetTyLeGiaBanMayCu Fail ( - Result: " + l__PushInfoToPOS.Result + " - Msg: " + l__PushInfoToPOS.Msg + ")!";
            res.Data = null;
            return res;
        }
        if (l__PushInfoToPOS.Data == null)
        {
            res.Result = 0;
            res.Msg += " - Call POS_Order_DuyetTyLeGiaBanMayCu Fail ( - Data: null)!";
            res.Data = null;
            return res;
        }
        #endregion ===Call POS_DuyetTyLeTieuChuanDinhGiaBanMayCu_UpdateStatus===  

        var l__PushInfoToPOS__Data0 = l__PushInfoToPOS.Data[0];
        if (l__PushInfoToPOS__Data0.Result == 1)
        {
            res.Result = 1;
            res.Msg += " - Call POS_Order_DuyetTyLeGiaBanMayCu Success ( - Result: " + l__PushInfoToPOS__Data0.Result + " - Msg: " + l__PushInfoToPOS__Data0.Msg + ")!";
            res.Data = null;
        }
        else
        {
            res.Result = 0;
            res.Msg += " - Call POS_Order_DuyetTyLeGiaBanMayCu Fail ( - Result: " + l__PushInfoToPOS__Data0.Result + " - Msg: " + l__PushInfoToPOS__Data0.Msg + ")!";
            res.Data = null;
        }

        var l__Relationship_2001 = l__TicketWorkflowWithData__Data.Data.Relationships.Where(p => p.Id == -2001).FirstOrDefault();

        if (l__Relationship_2001 == null)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Ticket_TicketWorkflowWithData Fail ( - Data.Relationships: Id -2001 không tồn tại)!";
            res.Data = null;
            return res;
        }
        if (l__Relationship_2001.RelationshipId <= 0)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Ticket_TicketWorkflowWithData Fail ( - Data.Relationships: Id -2001 - RelationshipId: <= 0)!";
            res.Data = null;
            return res;
        }
        var l__Relationship_DoApprove__PhaseOutputId = l__Relationship_2001.RelationshipId;

        JObject l__DetailTemplate = new JObject();

        #region ===Call FRTUService_Relationship_GetDetailTemplate===
        var l__Relationship_GetDetailTemplate = await FRTUService_Relationship_GetDetailTemplate__V2(new FRTUService_Relationship_Detail_REQ_DTO
        {
            Email = g__Email__System,
            Token = g__Token__System,
            PhaseOutputId = l__Relationship_DoApprove__PhaseOutputId
        });
        if (l__Relationship_GetDetailTemplate.Result != 1)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Relationship_GetDetailTemplate Fail ( - Result: " + l__Relationship_GetDetailTemplate.Result + " - Msg: " + l__Relationship_GetDetailTemplate.Msg + ")!";
            res.Data = null;
            return res;
        }
        if (l__Relationship_GetDetailTemplate.Data == null)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Relationship_GetDetailTemplate Fail ( - Data: null)!";
            res.Data = null;
            return res;
        }
        l__DetailTemplate = JObject.FromObject(l__Relationship_GetDetailTemplate.Data);
        #endregion ===Call FRTUService_Relationship_GetDetailTemplate===

        string l__Relationship_DoApprove__Detail = "{}";

        #region ===Xử lý Detail Template===
        if (l__DetailTemplate != null && l__DetailTemplate.ToString() != "{}")
        {
            var l__JArray__Individual = (JArray)l__DetailTemplate["individual"];
            if (l__JArray__Individual != null)
            {
                var l__30200 = l__JArray__Individual.Where(p => p["id"].ToString() == "-30200").FirstOrDefault();
                if (l__30200 != null && l__30200["value"] != null) l__30200["value"] = (l__PushInfoToPOS__Data0.Result + " - " + l__PushInfoToPOS__Data0.Msg);
            }
            l__Relationship_DoApprove__Detail = l__DetailTemplate.ToString();
        }
        #endregion ===Xử lý Detail Template===

        #region ===Call FRTUService_Relationship_DoApprove===
        var l__Relationship_DoApprove = await FRTUService_Relationship_DoApprove__V2(new FRTUService_Relationship_DoApprove_REQ_DTO
        {
            Email = g__Email__System,
            Token = g__Token__System,
            PhaseOutputId = l__Relationship_DoApprove__PhaseOutputId,
            Detail = l__Relationship_DoApprove__Detail
        });
        if (l__Relationship_DoApprove.Result != 1)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Relationship_DoApprove Fail ( - Result: " + l__Relationship_DoApprove.Result + " - Msg: " + l__Relationship_DoApprove.Msg + ")!";
            res.Data = null;
            return res;
        }
        if (l__Relationship_DoApprove.Data == null)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Relationship_DoApprove Fail ( - Data: null)!";
            res.Data = null;
            return res;
        }
        if (l__Relationship_DoApprove.Data.ResultCode != 200)
        {
            res.Result = 0;
            res.Msg += " - Call FRTUService_Relationship_DoApprove Fail ( - ResultCode: " + l__Relationship_DoApprove.Data.ResultCode + " - Message: " + l__Relationship_DoApprove.Data.Message + ")!";
            res.Data = null;
            return res;
        }
        #endregion ===Call FRTUService_Relationship_DoApprove===
    }
    catch (Exception ex)
    {
        Logger.LogError("FRTUService.AppService - " + MethodBase.GetCurrentMethod().Name + "() - TicketId: {0} - ex: {1}", TicketId, ex);

        res.Result = 0;
        res.Msg += " - Call " + MethodBase.GetCurrentMethod().Name + " Error ( - ex.Message: " + ex.Message + ")!";
        res.Data = null;
    }
    return res;
}