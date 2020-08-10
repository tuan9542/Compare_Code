using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Utilities.IO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using WebCallLog.Helpers;
using WebCallLog.Models;

namespace WebCallLog.Controllers
{
    #region ======ConfigController======
    public class RegistrationFormController : Controller
    {
        CallLogDbContext g_CallLogDbContext = new CallLogDbContext();
        SqlDBHelper g_SqlDBHelper = new SqlDBHelper();
        string authority = string.Format("{0}://{1}", System.Web.HttpContext.Current.Request.Url.Scheme, System.Web.HttpContext.Current.Request.Url.Authority);
        //▼	Add - TuanNA89 - 22/07/2019 - Lưu Token của TPBank
        static string g__Token__TPBank = "";
        static DateTime g__TimeCreateToken_TPBank = DateTime.Now;
        static int g__TimeExpires__Second = 0;
        //▲	Add - TuanNA89 - 22/07/2019 - Lưu Token của TPBank
        // TuanNA89 - 06/08/2020 - Tối ưu lại 1 số Code liên quan đến TPBank
        static string g__isUseFwd = ConfigurationManager.AppSettings["TPB_IsFwd"].ToString();
        static string g__UrlFwd = ConfigurationManager.AppSettings["TPFicoPwd_Url"].ToString();
        static string g__Url = ConfigurationManager.AppSettings["TPFico_Url"].ToString();

        /// <summary>GET: /RegistrationForm/Manager</summary>
        public ActionResult Manager()
        {
            if (UserManager.CurrentUser == null)
                return Redirect("/Users/Login?u=" + Request.RawUrl);
            if (!UserManager.CheckPermisionMenu(Request.RawUrl))
            {
                TempData["Message"] = String.Format("Bạn không có quyền trên màn hình {0}", Request.RawUrl);
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        /// <summary>GET: /RegistrationForm/LoadCongTy</summary>
        public ActionResult LoadCongTy()
        {
            DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp_RegistrationForm_Get_Company", CommandType.StoredProcedure, null);

            if (l_DataTable != null && l_DataTable.Rows.Count > 0)
            {
                return Json(l_DataTable.EParseToObjects(), JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        /// <summary>GET: /RegistrationForm/LoadTrangThaiYeuCau</summary>
        public ActionResult LoadTrangThaiYeuCau()
        {
            DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("Status_Get", CommandType.StoredProcedure, null);

            foreach (DataRow l_DataRow in l_DataTable.Rows)
            {
                //  Bỏ 5-Hủy
                if (l_DataRow["Code"].ToString() == "5")
                {
                    l_DataTable.Rows.Remove(l_DataRow);
                    break;
                }
            }

            if (l_DataTable != null && l_DataTable.Rows.Count > 0)
            {
                return Json(l_DataTable.EParseToObjects(), JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        /// <summary>GET: /RegistrationForm/LoadTrangThaiPhieuDangKy</summary>
        public ActionResult LoadTrangThaiPhieuDangKy()
        {
            DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp_RegistrationForm_Get_Status", CommandType.StoredProcedure, null);

            if (l_DataTable != null && l_DataTable.Rows.Count > 0)
            {
                return Json(l_DataTable.EParseToObjects(), JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        /// <summary>GET: /RegistrationForm/SearchRegistrationForm</summary>
        public ActionResult SearchRegistrationForm(
            string p_MaCongTy, string p_CMND, string p_MaNV, string p_SDT
            , string p_MaCallLog, string p_SoSO, string p_SoHD
            , string p_NgayHD_From, string p_NgayHD_To
            , string p_TrangThaiCallLog, string p_TrangThaiPhieuDangKy
        )
        {
            double l_MaCongTy = double.TryParse(p_MaCongTy, out l_MaCongTy) ? l_MaCongTy : 0;
            double l_MaCallLog = double.TryParse(p_MaCallLog, out l_MaCallLog) ? l_MaCallLog : 0;
            double l_SoSO = double.TryParse(p_SoSO, out l_SoSO) ? l_SoSO : 0;
            DateTime l_NgayHD_From = DateTime.TryParse(p_NgayHD_From, out l_NgayHD_From) ? l_NgayHD_From : DateTime.Now;
            DateTime l_NgayHD_To = DateTime.TryParse(p_NgayHD_To, out l_NgayHD_To) ? l_NgayHD_To : DateTime.Now;
            int l_TrangThaiCallLog = int.TryParse(p_TrangThaiCallLog, out l_TrangThaiCallLog) ? l_TrangThaiCallLog : 0;
            int l_TrangThaiPhieuDangKy = int.TryParse(p_TrangThaiPhieuDangKy, out l_TrangThaiPhieuDangKy) ? l_TrangThaiPhieuDangKy : -1;

            SqlParameter[] l_SqlParameter = new SqlParameter[]{
                new SqlParameter("@MaCongTy", l_MaCongTy),
                new SqlParameter("@CMND", p_CMND),
                new SqlParameter("@MaNV", p_MaNV),
                new SqlParameter("@SDT", p_SDT),
                new SqlParameter("@MaCallLog", l_MaCallLog),
                new SqlParameter("@SoSO", l_SoSO),
                new SqlParameter("@SoHD", p_SoHD),
                new SqlParameter("@NgayHD_From", l_NgayHD_From),
                new SqlParameter("@NgayHD_To", p_NgayHD_To),
                new SqlParameter("@TrangThaiCallLog", l_TrangThaiCallLog),
                new SqlParameter("@TrangThaiPhieuDangKy", l_TrangThaiPhieuDangKy),
            };

            DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp_RegistrationForm_Search", CommandType.StoredProcedure, l_SqlParameter);

            if (l_DataTable != null && l_DataTable.Rows.Count > 0)
            {
                var l_JsonResult = Json(l_DataTable.EParseToObjects(), JsonRequestBehavior.AllowGet);
                l_JsonResult.MaxJsonLength = int.MaxValue;
                return l_JsonResult;
            }
            return null;
        }

        [HttpPost]
        /// <summary>POST: /RegistrationForm/SaveRegistrationForm</summary>
        public ActionResult SaveRegistrationForm(IEnumerable<RegistrationForm_Save> p_IE_RegistrationForm_Save)
        {
            try
            {
                if (p_IE_RegistrationForm_Save == null)
                {
                    return null;
                }

                using (DataTable l_DataTable = new DataTable())
                {
                    l_DataTable.Columns.Add("SoSO", typeof(double));
                    l_DataTable.Columns.Add("LyDo", typeof(string));
                    l_DataTable.Columns.Add("Status", typeof(int));

                    foreach (RegistrationForm_Save l_RegistrationForm_Save in p_IE_RegistrationForm_Save)
                    {
                        l_DataTable.Rows.Add(
                            l_RegistrationForm_Save.SoSO
                            , l_RegistrationForm_Save.LyDo
                            , l_RegistrationForm_Save.Status
                            );
                    }

                    SqlParameter[] l_SqlParameter = new SqlParameter[]{
                        new SqlParameter("@Table", l_DataTable),
                        new SqlParameter("@UpdateBy", UserManager.CurrentUser.InsideCode),
                    };

                    DataTable l_DataTable_Result = g_SqlDBHelper.ExecuteCommand("sp_RegistrationForm_Save", CommandType.StoredProcedure, l_SqlParameter);

                    if (l_DataTable_Result != null && l_DataTable_Result.Rows.Count > 0)
                    {
                        return Json(l_DataTable_Result.EParseToObjects(), JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(Json(ex.ToString()), JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        /// <summary>GET: /RegistrationForm/SearchRegistrationFormBySOBarCode</summary>
        public ActionResult SearchRegistrationFormBySOBarCode(string p_SoSO)
        {
            double l_SoSO = double.TryParse(p_SoSO, out l_SoSO) ? l_SoSO : 0;

            SqlParameter[] l_SqlParameter = new SqlParameter[]{
                new SqlParameter("@SoSO", l_SoSO),
            };

            DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp_RegistrationForm_Search_BySOBarCode", CommandType.StoredProcedure, l_SqlParameter);

            if (l_DataTable != null && l_DataTable.Rows.Count > 0)
            {
                return Json(l_DataTable.EParseToObjects(), JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        /// <summary>GET: /RegistrationForm/DownloadRegistrationForm</summary>
        public ActionResult DownloadRegistrationForm()
        {
            string l_Temp_FilePath = "/TemplateExcel/RegistrationForm_SoSO.xlsx";
            FileInfo l_FileInfo = new FileInfo(HttpContext.Server.MapPath(l_Temp_FilePath));

            if (l_FileInfo.Exists)
            {
                using (ExcelPackage l_ExcelPackage = new ExcelPackage(l_FileInfo))
                {
                    Response.Clear();
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("Content-Disposition", "attachment; filename=RegistrationForm_SoSO_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx");
                    Response.BinaryWrite(l_ExcelPackage.GetAsByteArray());
                    Response.Flush();
                    Response.End();
                }
            }

            return RedirectToAction("Manager", "RegistrationForm");
        }

        [HttpPost]
        /// <summary>POST: /RegistrationForm/SearchRegistrationFormBySOList</summary>
        public ActionResult SearchRegistrationFormBySOList(IEnumerable<sp_RegistrationForm_Search_BySOList> p_IE_sp_RegistrationForm_Search_BySOList)
        {
            try
            {
                if (p_IE_sp_RegistrationForm_Search_BySOList == null)
                {
                    return null;
                }

                using (DataTable l_DataTable = new DataTable())
                {
                    l_DataTable.Columns.Add("SoSO", typeof(double));

                    foreach (sp_RegistrationForm_Search_BySOList l_sp_RegistrationForm_Search_BySOList in p_IE_sp_RegistrationForm_Search_BySOList)
                    {
                        l_DataTable.Rows.Add(l_sp_RegistrationForm_Search_BySOList.SoSO);
                    }

                    SqlParameter[] l_SqlParameter = new SqlParameter[]{
                        new SqlParameter("@Table", l_DataTable),
                        new SqlParameter("@UpdateBy", UserManager.CurrentUser.InsideCode),
                    };

                    DataTable l_DataTable_Result = g_SqlDBHelper.ExecuteCommand("sp_RegistrationForm_Search_BySOList", CommandType.StoredProcedure, l_SqlParameter);

                    if (l_DataTable_Result != null && l_DataTable_Result.Rows.Count > 0)
                    {
                        return Json(l_DataTable_Result.EParseToObjects(), JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(Json(ex.ToString()), JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        [HttpPost]
        /// <summary>POST: /RegistrationForm/ExportRegistrationForm</summary>
        public ActionResult ExportRegistrationForm(FormCollection p_FormCollection)
        {
            double l_MaCongTy = double.TryParse(p_FormCollection["cboCongTy"].ToString(), out l_MaCongTy) ? l_MaCongTy : 0;
            string l_CMND = p_FormCollection["txtCMND"].ToString();
            string l_MaNV = p_FormCollection["txtMaNV"].ToString();
            string l_SDT = p_FormCollection["txtSDT"].ToString();
            double l_MaCallLog = double.TryParse(p_FormCollection["txtMaCallLog"].ToString(), out l_MaCallLog) ? l_MaCallLog : 0;
            double l_SoSO = double.TryParse(p_FormCollection["txtSO"].ToString(), out l_SoSO) ? l_SoSO : 0;
            string l_SoHD = p_FormCollection["txtSoHD"].ToString();
            DateTime l_NgayHD_From = DateTime.TryParse(p_FormCollection["dtpNgayHD_Start"].ToString(), out l_NgayHD_From) ? l_NgayHD_From : DateTime.Now;
            DateTime l_NgayHD_To = DateTime.TryParse(p_FormCollection["dtpNgayHD_End"].ToString(), out l_NgayHD_To) ? l_NgayHD_To : DateTime.Now;
            int l_TrangThaiCallLog = int.TryParse(p_FormCollection["cboTrangThaiCallLog"].ToString(), out l_TrangThaiCallLog) ? l_TrangThaiCallLog : 0;
            int l_TrangThaiPhieuDangKy = int.TryParse(p_FormCollection["cboTrangThaiPhieuDangKy"].ToString(), out l_TrangThaiPhieuDangKy) ? l_TrangThaiPhieuDangKy : -1;

            string l_Temp_FilePath = "/TemplateExcel/QuanLyPhieuDangKyMuaHang_Export.xlsx";
            FileInfo l_FileInfo = new FileInfo(HttpContext.Server.MapPath(l_Temp_FilePath));

            SqlParameter[] l_SqlParameter = new SqlParameter[]{
                new SqlParameter("@MaCongTy", l_MaCongTy),
                new SqlParameter("@CMND", l_CMND),
                new SqlParameter("@MaNV", l_MaNV),
                new SqlParameter("@SDT", l_SDT),
                new SqlParameter("@MaCallLog", l_MaCallLog),
                new SqlParameter("@SoSO", l_SoSO),
                new SqlParameter("@SoHD", l_SoHD),
                new SqlParameter("@NgayHD_From", l_NgayHD_From),
                new SqlParameter("@NgayHD_To", l_NgayHD_To),
                new SqlParameter("@TrangThaiCallLog", l_TrangThaiCallLog),
                new SqlParameter("@TrangThaiPhieuDangKy", l_TrangThaiPhieuDangKy),
            };

            DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp_RegistrationForm_Search", CommandType.StoredProcedure, l_SqlParameter);

            if (l_FileInfo.Exists)
            {
                using (ExcelPackage l_ExcelPackage = new ExcelPackage(l_FileInfo))
                {
                    ExcelWorksheet l_ExcelWorksheet = l_ExcelPackage.Workbook.Worksheets["QuanLyPhieuDangKyMuaHang_Export"];

                    if (l_DataTable != null && l_DataTable.Rows.Count > 0)
                    {
                        int l_Row = 2;

                        foreach (DataRow l_DataRow in l_DataTable.Rows)
                        {
                            int l_Col = 0;
                            l_Col++; l_ExcelWorksheet.Cells[l_Row, l_Col].Value = l_DataRow["TenCongTy"].ToString();
                            l_Col++; l_ExcelWorksheet.Cells[l_Row, l_Col].Value = l_DataRow["TenKhachHang"].ToString();
                            l_Col++; l_ExcelWorksheet.Cells[l_Row, l_Col].Value = l_DataRow["SoHoaDon"].ToString();
                            l_Col++; l_ExcelWorksheet.Cells[l_Row, l_Col].Value = l_DataRow["SoSO"].ToString();

                            //▼	Edit - VietMXH - 28/03/2018 - Thêm cột Trạng thái đơn hàng==================================================
                            l_Col++; l_ExcelWorksheet.Cells[l_Row, l_Col].Value = l_DataRow["TrangThaiDH"].ToString();
                            //▲	Edit - VietMXH - 28/03/2018 - Thêm cột Trạng thái đơn hàng==================================================

                            l_Col++; l_ExcelWorksheet.Cells[l_Row, l_Col].Value = l_DataRow["NgayHT"].ToString();

                            //▼	Edit - VietMXH - 04/12/2017 - RegistrationForm/Manager==================================================
                            l_Col++; l_ExcelWorksheet.Cells[l_Row, l_Col].Value = l_DataRow["TenShopBanHang"].ToString();
                            l_Col++; l_ExcelWorksheet.Cells[l_Row, l_Col].Value = l_DataRow["TenNVBanHang"].ToString();
                            //▲	Edit - VietMXH - 04/12/2017 - RegistrationForm/Manager==================================================

                            l_Col++; l_ExcelWorksheet.Cells[l_Row, l_Col].Value = l_DataRow["TenTrangThaiPhieuDK"].ToString();
                            l_Col++; l_ExcelWorksheet.Cells[l_Row, l_Col].Value = l_DataRow["CMND"].ToString();
                            l_Col++; l_ExcelWorksheet.Cells[l_Row, l_Col].Value = l_DataRow["SDT"].ToString();
                            l_Col++; l_ExcelWorksheet.Cells[l_Row, l_Col].Value = l_DataRow["MaNV"].ToString();
                            l_Col++; l_ExcelWorksheet.Cells[l_Row, l_Col].Value = l_DataRow["SoCallLog"].ToString();
                            l_Col++; l_ExcelWorksheet.Cells[l_Row, l_Col].Value = l_DataRow["TenTrangThaiCallLog"].ToString();
                            l_Col++; l_ExcelWorksheet.Cells[l_Row, l_Col].Value = l_DataRow["TenNguoiUpHinh"].ToString();
                            l_Col++; l_ExcelWorksheet.Cells[l_Row, l_Col].Value = l_DataRow["TenShopUpHinh"].ToString();
                            l_Col++; l_ExcelWorksheet.Cells[l_Row, l_Col].Value = l_DataRow["NgayUpHinh"].ToString();
                            l_Col++; l_ExcelWorksheet.Cells[l_Row, l_Col].Value = l_DataRow["SoNgayChuaGuiChungTuVe"].ToString();
                            l_Col++; l_ExcelWorksheet.Cells[l_Row, l_Col].Value = l_DataRow["LyDo"].ToString();
                            l_Col++; l_ExcelWorksheet.Cells[l_Row, l_Col].Value = l_DataRow["TenNguoiCapNhat"].ToString();
                            l_Col++; l_ExcelWorksheet.Cells[l_Row, l_Col].Value = l_DataRow["NgayCapNhat"].ToString();

                            l_Row++;
                        }
                    }

                    Response.Clear();
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("Content-Disposition", "attachment; filename=QuanLyPhieuDangKyMuaHang_Export_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx");
                    Response.BinaryWrite(l_ExcelPackage.GetAsByteArray());
                    Response.Flush();
                    Response.End();
                }
            }

            return RedirectToAction("Manager", "RegistrationForm");
        }

        //▼	Edit - TuanNA89 - 04/03/2019 - TPBank==================================================
        //▼	Edit - VietMXH - 03/12/2018 - TPBank==================================================
        #region ===TPBank===
        #region ===Function cho js dùng===
        /// <summary>GET: /RegistrationForm/InstallmentTPBank</summary>
        public ActionResult InstallmentTPBank()
        {
            if (UserManager.CurrentUser == null)
                return Redirect("/Users/Login?u=" + Request.RawUrl);
            if (!UserManager.CheckPermisionMenu(Request.RawUrl))
            {
                TempData["Message"] = String.Format("Bạn không có quyền trên màn hình {0}", Request.RawUrl);
                return RedirectToAction("Index", "Home");
            }
            ViewBag.g__View__Key = UserManager.CurrentUser.LoginDateTime.ToString("yyyyMMddHHmmss");
            ViewBag.g__View__UserCode = UserManager.CurrentUser.InsideCode;
            ViewBag.g__View__ShopCode = UserManager.CurrentUser.WarehouseCode;
            return View();
        }
        /// <summary>GET: /RegistrationForm/InstallmentTPBank_BangKeHangHoa</summary>
        public ActionResult InstallmentTPBank_BangKeHangHoa(int p_ID_Final)
        {
            SqlParameter[] l_SqlParameter = new SqlParameter[]{
                new SqlParameter("@ID_Final", p_ID_Final)
            };
            DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_InBangKeHangHoa", CommandType.StoredProcedure, l_SqlParameter);
            if (l_DataTable != null && l_DataTable.Rows.Count > 0)
            {
                ViewBag.CustName = l_DataTable.Rows[0]["CardName"].ToString();
                ViewBag.CMND = l_DataTable.Rows[0]["CMND"].ToString();
                ViewBag.NgayCapCMND = l_DataTable.Rows[0]["NgayCapCMND"].ToString();
                ViewBag.NoiCapCMND = l_DataTable.Rows[0]["NoiCapCMND"].ToString();
                ViewBag.TongCong = l_DataTable.Rows[0]["SoTienDonHang"].ToString();
                ViewBag.SoHopDong = l_DataTable.Rows[0]["SoHopDong"].ToString();
                ViewBag.MaNV = l_DataTable.Rows[0]["Bank_MSNVTinDung"].ToString();
                ViewBag.DiaChiGH = l_DataTable.Rows[0]["DiaChiGH"].ToString();
                ViewBag.DetailPurchase = l_DataTable.Rows;
            }
            return View();
        }
        /// <summary>GET: /RegistrationForm/InstallmentTPBank_DeNghiVayVon</summary>
        public ActionResult InstallmentTPBank_DeNghiVayVon(int p_ID_Final)
        {
            SqlParameter[] l_SqlParameter = new SqlParameter[]{
                new SqlParameter("@ID_Final", p_ID_Final)
            };
            DataSet l_DataSet = g_SqlDBHelper.ExecuteCommandDataSet("sp_InstallmentTPBank_InDeNghiVayVon", CommandType.StoredProcedure, l_SqlParameter);

            if (l_DataSet != null && l_DataSet.Tables.Count > 0)
            {
                DataTable l_ThongTinKH = l_DataSet.Tables[0];
                DataTable l_ThongTinDonHang = l_DataSet.Tables[1];
                if (l_ThongTinKH != null && l_ThongTinKH.Rows.Count > 0)
                {
                    ViewBag.SoHopDong = l_ThongTinKH.Rows[0]["SoHopDong"].ToString();
                    ViewBag.CardName = l_ThongTinKH.Rows[0]["CardName"].ToString();
                    ViewBag.Birthday = l_ThongTinKH.Rows[0]["Birthday"].ToString();
                    ViewBag.Gender = l_ThongTinKH.Rows[0]["Gender"].ToString();
                    ViewBag.TinhTrangHonNhan = l_ThongTinKH.Rows[0]["TinhTrangHonNhan"].ToString();
                    ViewBag.CMND = l_ThongTinKH.Rows[0]["CMND"].ToString();
                    ViewBag.NgayCapCMND = l_ThongTinKH.Rows[0]["NgayCapCMND"].ToString();
                    ViewBag.NoiCapCMND = l_ThongTinKH.Rows[0]["NoiCapCMND"].ToString();
                    ViewBag.MaNV = l_ThongTinKH.Rows[0]["MaNV"].ToString();
                    ViewBag.DiaChiTamTru = l_ThongTinKH.Rows[0]["DiaChiTamTru"].ToString();
                    ViewBag.QuanHuyenTamTru = l_ThongTinKH.Rows[0]["QuanHuyenTamTru"].ToString();
                    ViewBag.TinhThanhPhoTamTru = l_ThongTinKH.Rows[0]["TinhThanhPhoTamTru"].ToString();
                    ViewBag.ThoiGianCuTru_Nam = l_ThongTinKH.Rows[0]["ThoiGianCuTru_Nam"].ToString();
                    ViewBag.ThoiGianCuTru_Thang = l_ThongTinKH.Rows[0]["ThoiGianCuTru_Thang"].ToString();
                    ViewBag.DiaChi = l_ThongTinKH.Rows[0]["DiaChi"].ToString();
                    ViewBag.QuanHuyen = l_ThongTinKH.Rows[0]["QuanHuyen"].ToString();
                    ViewBag.TinhThanhPho = l_ThongTinKH.Rows[0]["TinhThanhPho"].ToString();
                    ViewBag.CMND_ChuHo = l_ThongTinKH.Rows[0]["CMND_ChuHo"].ToString();
                    ViewBag.DiaChiLienHe_Type = l_ThongTinKH.Rows[0]["DiaChiLienHe_Type"].ToString();
                    ViewBag.SDT = l_ThongTinKH.Rows[0]["SDT"].ToString();
                    ViewBag.NguoiHonPhoi__Name = l_ThongTinKH.Rows[0]["NguoiHonPhoi__Name"].ToString();
                    ViewBag.NguoiHonPhoi__CMND = l_ThongTinKH.Rows[0]["NguoiHonPhoi__CMND"].ToString();
                    ViewBag.NguoiHonPhoi__SDT = l_ThongTinKH.Rows[0]["NguoiHonPhoi__SDT"].ToString();
                    ViewBag.NguoiLienHe_1__Name = l_ThongTinKH.Rows[0]["NguoiLienHe_1__Name"].ToString();
                    ViewBag.NguoiLienHe_1__SDT = l_ThongTinKH.Rows[0]["NguoiLienHe_1__SDT"].ToString();
                    ViewBag.NguoiLienHe_2__Name = l_ThongTinKH.Rows[0]["NguoiLienHe_2__Name"].ToString();
                    ViewBag.NguoiLienHe_2__SDT = l_ThongTinKH.Rows[0]["NguoiLienHe_2__SDT"].ToString();
                    ViewBag.SPVay = l_ThongTinKH.Rows[0]["SPVay"].ToString();
                    ViewBag.MaSoNVTinDung = l_ThongTinKH.Rows[0]["MaSoNVTinDung"].ToString();
                    ViewBag.SoTienVay = l_ThongTinKH.Rows[0]["SoTienVay"].ToString();
                    ViewBag.KyHan = l_ThongTinKH.Rows[0]["KyHan"].ToString();
                    ViewBag.SoTienTraTruoc = l_ThongTinKH.Rows[0]["SoTienTraTruoc"].ToString();
                    ViewBag.NgayDuyetHoSo = l_ThongTinKH.Rows[0]["NgayDuyetHoSo"].ToString();
                    ViewBag.NgayThanhToanHangThang = l_ThongTinKH.Rows[0]["NgayThanhToanHangThang"].ToString();
                    ViewBag.SoTienDonHang = l_ThongTinKH.Rows[0]["SoTienDonHang"].ToString();
                    ViewBag.LaiSuatNam = l_ThongTinKH.Rows[0]["LaiSuatNam"].ToString();
                    ViewBag.LaiSuatThang = l_ThongTinKH.Rows[0]["LaiSuatThang"].ToString();
                    ViewBag.SoTienTraHangThang = l_ThongTinKH.Rows[0]["SoTienTraHangThang"].ToString();
                    ViewBag.ThuNhapHangThang = l_ThongTinKH.Rows[0]["ThuNhapHangThang"].ToString();
                }

                if (l_ThongTinDonHang != null && l_ThongTinDonHang.Rows.Count > 0)
                {
                    foreach (DataRow dr in l_ThongTinDonHang.Rows)
                    {
                        ViewData["TenSP" + dr["STT"].ToString()] = dr["TenSanPham"].ToString();
                        ViewData["GiaSP" + dr["STT"].ToString()] = dr["GiaSanPham"].ToString();
                        ViewData["MaSP" + dr["STT"].ToString()] = dr["MaSanPham"].ToString();
                    }
                }
            }
            return View();
        }
        /// <summary>GET: /RegistrationForm/InstallmentTPBank</summary>
        public ActionResult InstallmentTPBankGiaiNgan()
        {
            if (UserManager.CurrentUser == null)
                return Redirect("/Users/Login?u=" + Request.RawUrl);
            //if (!UserManager.CheckPermisionMenu(Request.RawUrl))
            //{
            //    TempData["Message"] = String.Format("Bạn không có quyền trên màn hình {0}", Request.RawUrl);
            //    return RedirectToAction("Index", "Home");
            //}            

            ViewBag.g__View__Key = UserManager.CurrentUser.LoginDateTime.ToString("yyyyMMddHHmmss");
            ViewBag.g__View__UserCode = UserManager.CurrentUser.InsideCode;
            return View();
        }
        /// <summary>GET: /RegistrationForm/Vendor__Get</summary>
        public ActionResult Vendor__Get()
        {
            DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp__WEB__RegistrationForm__InstallmentTPBank__Vendor__Get", CommandType.StoredProcedure, null);
            if (l_DataTable != null && l_DataTable.Rows.Count > 0)
            {
                var l_JsonResult = Json(l_DataTable.EParseToObjects(), JsonRequestBehavior.AllowGet);
                l_JsonResult.MaxJsonLength = int.MaxValue;
                return l_JsonResult;
            }
            return null;
        }
        /// <summary>GET: /RegistrationForm/DiaChiLienHe_Type__Get</summary>
        public ActionResult DiaChiLienHe_Type__Get()
        {
            DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp__WEB__RegistrationForm__InstallmentTPBank__DiaChiLienHe_Type__Get", CommandType.StoredProcedure, null);
            if (l_DataTable != null && l_DataTable.Rows.Count > 0)
            {
                var l_JsonResult = Json(l_DataTable.EParseToObjects(), JsonRequestBehavior.AllowGet);
                l_JsonResult.MaxJsonLength = int.MaxValue;
                return l_JsonResult;
            }
            return null;
        }
        /// <summary>GET: /RegistrationForm/SanPhamVay__Get</summary>
        public ActionResult SanPhamVay__Get(int p_KyHan)
        {
            SqlParameter[] l_SqlParameter = new SqlParameter[]{
                new SqlParameter("@KyHan", p_KyHan)
            };
            DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp__WEB__RegistrationForm__InstallmentTPBank__SanPhamVay__Get", CommandType.StoredProcedure, l_SqlParameter);
            if (l_DataTable != null && l_DataTable.Rows.Count > 0)
            {
                var l_JsonResult = Json(l_DataTable.EParseToObjects(), JsonRequestBehavior.AllowGet);
                l_JsonResult.MaxJsonLength = int.MaxValue;
                return l_JsonResult;
            }
            return null;
        }
        //▼	Edit - TuanNA89 - 26/06/2019 - Lấy mã nhân viên tín dụng
        /// <summary>GET: /RegistrationForm/NhanVienTinDung__Get</summary>
        public ActionResult NhanVienTinDung__Get()
        {
            DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp__WEB__RegistrationForm__InstallmentTPBank__NhanVienTinDung_Get", CommandType.StoredProcedure, null);
            if (l_DataTable != null && l_DataTable.Rows.Count > 0)
            {
                var l_JsonResult = Json(l_DataTable.EParseToObjects(), JsonRequestBehavior.AllowGet);
                l_JsonResult.MaxJsonLength = int.MaxValue;
                return l_JsonResult;
            }
            return null;
        }
        //▲	Edit - TuanNA89 - 26/06/2019 - Lấy mã nhân viên tín dụng
        [HttpPost]
        /// <summary>POST: /RegistrationForm/InstallmentTPBank_InsertOrUpdate</summary>
        public ActionResult InstallmentTPBank_InsertOrUpdate(FormCollection p_FormCollection)
        {
            TPFico_Customer_Result l__result = new TPFico_Customer_Result();
            string fName = string.Empty;
            string listFName = string.Empty;
            string l_FolderFileAttach = Keyword.FolderFileAttach;

            var l_StrDatas = p_FormCollection["FormData"].ToString();
            var l_Datas = JsonConvert.DeserializeObject<dynamic>(l_StrDatas);

            try
            {
                Image l_Image;
                foreach (string l_FileName in Request.Files)
                {
                    HttpPostedFileBase l_FileBase = Request.Files[l_FileName];

                    fName = l_FileName;
                    if (l_FileBase != null && l_FileBase.ContentLength > 0)
                    {
                        // Save file
                        var l_OriginalDirectory = new DirectoryInfo(Server.MapPath(l_FolderFileAttach));
                        string l_PathString = System.IO.Path.Combine(l_OriginalDirectory.ToString(), "");
                        var l_Path_Folder = string.Format("{0}{1}", l_PathString, "TPBank");
                        if (!Directory.Exists(l_Path_Folder))
                        {
                            Directory.CreateDirectory(l_Path_Folder);
                        }
                        var l_Path = string.Format("{0}{1}/{2}", l_PathString, "TPBank", fName);  //  NOTE: Thư mục gốc có dấu '/' đuôi, nên không cần Format "{0}{1}"

                        //Giảm dung lượng ảnh xuống
                        l_Image = Image.FromStream(l_FileBase.InputStream, true, true);
                        // Save the image with a quality of 50% 
                        SaveJpeg(l_Path, l_Image, 50);

                        //l_FileBase.SaveAs(l_Path);                        
                    }
                }

                double l__IDCardCode = double.TryParse(l_Datas[0].IDCardCode.ToString(), out l__IDCardCode) ? l__IDCardCode : 0;
                double l__VendCode = double.TryParse(l_Datas[0].VendCode.ToString(), out l__VendCode) ? l__VendCode : 0;
                DateTime l__Birthday = l_Datas[0].Birthday;
                // Edit - TuanNA89 - 21/06/2019 - Fix lỗi dùng sai kiểu biến. Bool -> int
                int l__Gender = int.TryParse(l_Datas[0].Gender.ToString(), out l__Gender) ? l__Gender : 0;
                DateTime l__NgayCapCMND = l_Datas[0].NgayCapCMND;

                int l__NoiCapCMND = int.TryParse(l_Datas[0].NoiCapCMND.ToString(), out l__NoiCapCMND) ? l__NoiCapCMND : 0;
                int l__TinhTrangHonNhan = int.TryParse(l_Datas[0].TinhTrangHonNhan.ToString(), out l__TinhTrangHonNhan) ? l__TinhTrangHonNhan : 0;
                int l__TamTru_TinhThanh = int.TryParse(l_Datas[0].TamTru_TinhThanh.ToString(), out l__TamTru_TinhThanh) ? l__TamTru_TinhThanh : 0;
                int l__TamTru_QuanHuyen = int.TryParse(l_Datas[0].TamTru_QuanHuyen.ToString(), out l__TamTru_QuanHuyen) ? l__TamTru_QuanHuyen : 0;
                int l__ThoiGianCuTru_Nam = int.TryParse(l_Datas[0].ThoiGianCuTru_Nam.ToString(), out l__ThoiGianCuTru_Nam) ? l__ThoiGianCuTru_Nam : 0;
                int l__ThoiGianCuTru_Thang = int.TryParse(l_Datas[0].ThoiGianCuTru_Thang.ToString(), out l__ThoiGianCuTru_Thang) ? l__ThoiGianCuTru_Thang : 0;
                int l__HoKhau_TinhThanh = int.TryParse(l_Datas[0].HoKhau_TinhThanh.ToString(), out l__HoKhau_TinhThanh) ? l__HoKhau_TinhThanh : 0;
                int l__HoKhau_QuanHuyen = int.TryParse(l_Datas[0].HoKhau_QuanHuyen.ToString(), out l__HoKhau_QuanHuyen) ? l__HoKhau_QuanHuyen : 0;
                int l__DiaChiLienHe_Type = int.TryParse(l_Datas[0].DiaChiLienHe_Type.ToString(), out l__DiaChiLienHe_Type) ? l__DiaChiLienHe_Type : 0;
                int l__LamViec_TinhThanh = int.TryParse(l_Datas[0].LamViec_TinhThanh.ToString(), out l__LamViec_TinhThanh) ? l__LamViec_TinhThanh : 0;
                int l__LamViec_QuanHuyen = int.TryParse(l_Datas[0].LamViec_QuanHuyen.ToString(), out l__LamViec_QuanHuyen) ? l__LamViec_QuanHuyen : 0;
                double l__ID_Final = double.TryParse(l_Datas[0].ID_Final.ToString(), out l__ID_Final) ? l__ID_Final : 0;
                decimal l__ThanhTien = decimal.TryParse(l_Datas[0].ThanhTien.ToString(), out l__ThanhTien) ? l__ThanhTien : 0;
                decimal l__TraTruoc = decimal.TryParse(l_Datas[0].TraTruoc.ToString(), out l__TraTruoc) ? l__TraTruoc : 0;
                int l__KyHan = int.TryParse(l_Datas[0].KyHan.ToString(), out l__KyHan) ? l__KyHan : 0;
                decimal l__Bank_LaiSuatNam = decimal.TryParse(l_Datas[0].Bank_LaiSuatNam.ToString(), out l__Bank_LaiSuatNam) ? l__Bank_LaiSuatNam : 0;
                string l__Url_CRD_MT_CMND = string.IsNullOrEmpty(l_Datas[0].Url_CRD_MT_CMND.ToString()) ? null : string.Format("/{0}/{1}", "TPBank", l_Datas[0].Url_CRD_MT_CMND.ToString());
                string l__Url_CRD_MS_CMND = string.IsNullOrEmpty(l_Datas[0].Url_CRD_MS_CMND.ToString()) ? null : string.Format("/{0}/{1}", "TPBank", l_Datas[0].Url_CRD_MS_CMND.ToString());
                string l__Url_CRD_KH_CMND = string.IsNullOrEmpty(l_Datas[0].Url_CRD_KH_CMND.ToString()) ? null : string.Format("/{0}/{1}", "TPBank", l_Datas[0].Url_CRD_KH_CMND.ToString());
                string l__Url_CRD_CD_DKMoThe = string.IsNullOrEmpty(l_Datas[0].Url_CRD_CD_DKMoThe.ToString()) ? null : string.Format("/{0}/{1}", "TPBank", l_Datas[0].Url_CRD_CD_DKMoThe.ToString());
                string l__Url_CRD_SHK_1 = string.IsNullOrEmpty(l_Datas[0].Url_CRD_SHK_1.ToString()) ? null : string.Format("/{0}/{1}", "TPBank", l_Datas[0].Url_CRD_SHK_1.ToString());
                string l__Url_CRD_SHK_2 = string.IsNullOrEmpty(l_Datas[0].Url_CRD_SHK_2.ToString()) ? null : string.Format("/{0}/{1}", "TPBank", l_Datas[0].Url_CRD_SHK_2.ToString());
                string l__Url_CRD_SHK_3 = string.IsNullOrEmpty(l_Datas[0].Url_CRD_SHK_3.ToString()) ? null : string.Format("/{0}/{1}", "TPBank", l_Datas[0].Url_CRD_SHK_3.ToString());
                string l__Url_CRD_SHK_4 = string.IsNullOrEmpty(l_Datas[0].Url_CRD_SHK_4.ToString()) ? null : string.Format("/{0}/{1}", "TPBank", l_Datas[0].Url_CRD_SHK_4.ToString());
                string l__Url_CRD_SHK_5 = string.IsNullOrEmpty(l_Datas[0].Url_CRD_SHK_5.ToString()) ? null : string.Format("/{0}/{1}", "TPBank", l_Datas[0].Url_CRD_SHK_5.ToString());
                string l__Url_CRD_SHK_6 = string.IsNullOrEmpty(l_Datas[0].Url_CRD_SHK_6.ToString()) ? null : string.Format("/{0}/{1}", "TPBank", l_Datas[0].Url_CRD_SHK_6.ToString());
                string l__Url_CRD_SHK_7 = string.IsNullOrEmpty(l_Datas[0].Url_CRD_SHK_7.ToString()) ? null : string.Format("/{0}/{1}", "TPBank", l_Datas[0].Url_CRD_SHK_7.ToString());
                string l__Bank_KetQuaHoSo = l_Datas[0].Bank_KetQuaHoSo.ToString();
                //Add - TuanNA89 - 02 / 07 / 2019 - Thêm field mới
                decimal l__ThuNhapHangThang = decimal.TryParse(l_Datas[0].ThuNhapHangThang.ToString(), out l__ThuNhapHangThang) ? l__ThuNhapHangThang : 0;
                //Add - TuanNA89 - 18/07/2019 - sản phẩm vay CD05_SAMSUNG
                decimal l__PhiHoSo = decimal.TryParse(l_Datas[0].PhiHoSo.ToString(), out l__PhiHoSo) ? l__PhiHoSo : 0;

                DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_InsertOrUpdate", System.Data.CommandType.StoredProcedure, new SqlParameter[]{
                new SqlParameter("@IDCardCode", l__IDCardCode),
                new SqlParameter("@CMND", l_Datas[0].CMND.ToString()),
                new SqlParameter("@VendCode", l__VendCode),
                new SqlParameter("@CardName", l_Datas[0].CardName.ToString()),
                new SqlParameter("@Birthday", l__Birthday),
                new SqlParameter("@Gender", l__Gender),
                new SqlParameter("@NgayCapCMND", l__NgayCapCMND),
                new SqlParameter("@NoiCapCMND", l__NoiCapCMND),
                new SqlParameter("@TinhTrangHonNhan", l__TinhTrangHonNhan),
                new SqlParameter("@MaNVKH", l_Datas[0].MaNVKH.ToString()),
                new SqlParameter("@SDT", l_Datas[0].SDT.ToString()),
                new SqlParameter("@TamTru_TinhThanh", l__TamTru_TinhThanh),
                new SqlParameter("@TamTru_QuanHuyen", l__TamTru_QuanHuyen),
                new SqlParameter("@TamTru_PhuongXa", l_Datas[0].TamTru_PhuongXa.ToString()),
                new SqlParameter("@TamTru_DuongPho", l_Datas[0].TamTru_DuongPho.ToString()),
                new SqlParameter("@TamTru_SoNha", l_Datas[0].TamTru_SoNha.ToString()),
                new SqlParameter("@HuongDanDuongDi", l_Datas[0].HuongDanDuongDi.ToString()),
                new SqlParameter("@ThoiGianCuTru_Nam", l__ThoiGianCuTru_Nam),
                new SqlParameter("@ThoiGianCuTru_Thang", l__ThoiGianCuTru_Thang),
                new SqlParameter("@HoKhau_TinhThanh", l__HoKhau_TinhThanh),
                new SqlParameter("@HoKhau_QuanHuyen", l__HoKhau_QuanHuyen),
                new SqlParameter("@HoKhau_PhuongXa", l_Datas[0].HoKhau_PhuongXa.ToString()),
                new SqlParameter("@HoKhau_DuongPho", l_Datas[0].HoKhau_DuongPho.ToString()),
                new SqlParameter("@HoKhau_SoNha", l_Datas[0].HoKhau_SoNha.ToString()),
                new SqlParameter("@CMND_ChuHo", l_Datas[0].CMND_ChuHo.ToString()),
                new SqlParameter("@DiaChiLienHe_Type", l__DiaChiLienHe_Type),
                new SqlParameter("@LamViec_TinhThanh", l__LamViec_TinhThanh),
                new SqlParameter("@LamViec_QuanHuyen", l__LamViec_QuanHuyen),
                new SqlParameter("@LamViec_PhuongXa", l_Datas[0].LamViec_PhuongXa.ToString()),
                new SqlParameter("@LamViec_DuongPho", l_Datas[0].LamViec_DuongPho.ToString()),
                new SqlParameter("@LamViec_SoNha", l_Datas[0].LamViec_SoNha.ToString()),
                new SqlParameter("@NguoiHonPhoi_HoTen", l_Datas[0].NguoiHonPhoi_HoTen.ToString()),
                new SqlParameter("@NguoiHonPhoi_CMND", l_Datas[0].NguoiHonPhoi_CMND.ToString()),
                new SqlParameter("@NguoiHonPhoi_SDT", l_Datas[0].NguoiHonPhoi_SDT.ToString()),
                new SqlParameter("@NguoiThan1_HoTen", l_Datas[0].NguoiThan1_HoTen.ToString()),
                new SqlParameter("@NguoiThan1_SDT", l_Datas[0].NguoiThan1_SDT.ToString()),
                new SqlParameter("@NguoiThan2_HoTen", l_Datas[0].NguoiThan2_HoTen.ToString()),
                new SqlParameter("@NguoiThan2_SDT", l_Datas[0].NguoiThan2_SDT.ToString()),
                new SqlParameter("@ID_Final", l__ID_Final),
                new SqlParameter("@DSSanPham", l_Datas[0].DSSanPham.ToString()),
                new SqlParameter("@ThanhTien", l__ThanhTien),
                new SqlParameter("@TraTruoc", l__TraTruoc),
                new SqlParameter("@KyHan", l__KyHan),
                new SqlParameter("@Bank_SPVay", l_Datas[0].Bank_SPVay.ToString()),
                new SqlParameter("@Bank_LaiSuatNam", l__Bank_LaiSuatNam),
                new SqlParameter("@Bank_MSNVTinDung", l_Datas[0].Bank_MSNVTinDung.ToString()),
                new SqlParameter("@SoPO_SamSung", l_Datas[0].SoPO_SamSung.ToString()),
                new SqlParameter("@MaNVBH", l_Datas[0].MaNVBH.ToString()),
                new SqlParameter("@DiaChiGiaoHang", l_Datas[0].DiaChiGiaoHang.ToString()),
                new SqlParameter("@UserCode", l_Datas[0].UserCode.ToString()),
                new SqlParameter("@ShopCode", l_Datas[0].ShopCode.ToString()),
                new SqlParameter("@FromForm", l_Datas[0].FromForm.ToString()),
                new SqlParameter("@Url_CRD_MT_CMND", l__Url_CRD_MT_CMND),
                new SqlParameter("@Url_CRD_MS_CMND", l__Url_CRD_MS_CMND),
                new SqlParameter("@Url_CRD_KH_CMND", l__Url_CRD_KH_CMND),
                new SqlParameter("@Url_CRD_CD_DKMoThe", l__Url_CRD_CD_DKMoThe),
                new SqlParameter("@Url_CRD_SHK_1", l__Url_CRD_SHK_1),
                new SqlParameter("@Url_CRD_SHK_2", l__Url_CRD_SHK_2),
                new SqlParameter("@Url_CRD_SHK_3", l__Url_CRD_SHK_3),
                new SqlParameter("@Url_CRD_SHK_4", l__Url_CRD_SHK_4),
                new SqlParameter("@Url_CRD_SHK_5", l__Url_CRD_SHK_5),
                new SqlParameter("@Url_CRD_SHK_6", l__Url_CRD_SHK_6),
                new SqlParameter("@Url_CRD_SHK_7", l__Url_CRD_SHK_7),
                //Add - TuanNA89 - 02 / 07 / 2019 - Thêm field mới
                new SqlParameter("@ThuNhapHangThang", l__ThuNhapHangThang),
                //Add - TuanNA89 - 18/07/2019 - sản phẩm vay CD05_SAMSUNG
                new SqlParameter("@PhiHoSo", l__PhiHoSo),

                });
                if (l_DataTable != null && l_DataTable.Rows.Count > 0)
                {

                    #region ===PUSH API TPFico===
                    if (l_DataTable.Rows[0]["Result"].ToString() == "1")
                    {
                        if (l__Bank_KetQuaHoSo == null || l__Bank_KetQuaHoSo == "0")
                        {
                            //TuanNA89 - 06/08/2020 - Tối ưu lại 1 số Code liên quan đến TPBank
                            var apiResult = InstallmentTPBank_TPFico_SendCustInfo(l_DataTable.Rows[0]["Id_Final"].ToString());
                            l__result = new JavaScriptSerializer().Deserialize<TPFico_Customer_Result>(apiResult);
                        }
                        else
                        {
                            l__result.status = l_DataTable.Rows[0]["Result"].ToString();
                            l__result.messages = l_DataTable.Rows[0]["Msg"].ToString();
                        }

                        if (l__result != null && l__result.status != "")
                        {
                            var jsonResult = Json(l__result, JsonRequestBehavior.AllowGet);
                            jsonResult.MaxJsonLength = int.MaxValue;
                            return jsonResult;
                        }
                    }
                    #endregion
                    else
                    {
                        l__result = new TPFico_Customer_Result()
                        {
                            status = "0",
                            messages = l_DataTable.Rows[0]["Msg"].ToString()
                        };
                    }
                }
                else
                {
                    l__result = new TPFico_Customer_Result()
                    {
                        status = "0",
                        messages = "Lỗi lưu thông tin khách hàng"
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogError("RegistrationForm - " + MethodBase.GetCurrentMethod().Name + "()", ex.ToString());
                l__result = new TPFico_Customer_Result()
                {
                    status = "0",
                    messages = ex.ToString()
                };
            }

            return Json(l__result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>GET: /RegistrationForm/InstallmentTPBank_HistoryPurchase</summary>
        public ActionResult InstallmentTPBank_HistoryPurchase(int p_IDCardCode)
        {
            SqlParameter[] l_SqlParameter = new SqlParameter[]{
                new SqlParameter("@IDCardCode", p_IDCardCode)
            };
            DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_HistoryPurchase", CommandType.StoredProcedure, l_SqlParameter);
            if (l_DataTable != null && l_DataTable.Rows.Count > 0)
            {
                var l_JsonResult = Json(l_DataTable.EParseToObjects(), JsonRequestBehavior.AllowGet);
                l_JsonResult.MaxJsonLength = int.MaxValue;
                return l_JsonResult;
            }
            return null;
        }
        /// <summary>GET: /RegistrationForm/InstallmentTPBank_CustInfo</summary>
        public ActionResult InstallmentTPBank_CustInfo(string p_CMND)
        {
            SqlParameter[] l_SqlParameter = new SqlParameter[]{
                new SqlParameter("@CMND", p_CMND)
                // Add - TuanNA89 - 28/06/2019 - Thêm rule check KH CIC
                , new SqlParameter("@User", UserManager.CurrentUser.InsideCode)
            };
            DataSet l_DataSet = g_SqlDBHelper.ExecuteCommandDataSet("sp_InstallmentTPBank_CustInfo", CommandType.StoredProcedure, l_SqlParameter);
            if (l_DataSet != null && l_DataSet.Tables.Count > 0)
            {
                #region ====== Xóa khi được yêu cầu up lại field ======
                if (l_DataSet.Tables[0].Rows.Count > 0 && l_DataSet.Tables[0].Rows[0]["Bank_KetQuaHoSo"].ToString() == "6")
                {
                    if (l_DataSet.Tables[2].Rows.Count > 0)
                    {
                        List<string> l__DsHinhCanXoa = new List<string>();
                        foreach (DataRow dr in l_DataSet.Tables[2].Rows)
                        {
                            if (dr["TableName"].ToString() == "HinhAnhXacNhanDangKy")
                            {
                                if (dr["OldValue"].ToString() != "")
                                {
                                    l__DsHinhCanXoa.Add(dr["OldValue"].ToString());
                                }
                            }
                        }
                        DeleteListImages(l__DsHinhCanXoa);
                    }
                }
                #endregion
                var l_JsonResult = Json(JsonConvert.SerializeObject(l_DataSet), JsonRequestBehavior.AllowGet);
                l_JsonResult.MaxJsonLength = int.MaxValue;
                return l_JsonResult;
            }
            return null;
        }
        /// <summary>GET: /RegistrationForm/InstallmentTPBank_CustInfo</summary>
        public ActionResult InstallmentTPBank_CustInfo_GiaiNgan(string p_CMND)
        {
            SqlParameter[] l_SqlParameter = new SqlParameter[]{
                new SqlParameter("@CMND", p_CMND)
            };
            DataSet l_DataSet = g_SqlDBHelper.ExecuteCommandDataSet("sp_InstallmentTPBank_CustInfo_GiaiNgan", CommandType.StoredProcedure, l_SqlParameter);

            if (l_DataSet != null && l_DataSet.Tables.Count > 0)
            {
                #region ====== Xóa khi được yêu cầu up lại field ======
                if (l_DataSet.Tables[0].Rows.Count > 0 && l_DataSet.Tables[0].Rows[0]["Bank_KetQuaHoSo"].ToString() == "7")
                {
                    if (l_DataSet.Tables[1].Rows.Count > 0)
                    {
                        List<string> l__DsHinhCanXoa = new List<string>();
                        foreach (DataRow dr in l_DataSet.Tables[1].Rows)
                        {
                            if (dr["TableName"].ToString() == "HinhAnhXacNhanDangKy")
                            {
                                if (dr["OldValue"].ToString() != "")
                                {
                                    l__DsHinhCanXoa.Add(dr["OldValue"].ToString());
                                }
                            }
                        }
                        DeleteListImages(l__DsHinhCanXoa);
                    }
                }
                #endregion
                var l_JsonResult = Json(JsonConvert.SerializeObject(l_DataSet), JsonRequestBehavior.AllowGet);
                l_JsonResult.MaxJsonLength = int.MaxValue;
                return l_JsonResult;
            }
            return null;
        }
        /// <summary>GET: /RegistrationForm/InstallmentTPBank_DetailPurchase</summary>
        public ActionResult InstallmentTPBank_DetailPurchase(int p_SoDonHang)
        {
            SqlParameter[] l_SqlParameter = new SqlParameter[]{
                new SqlParameter("@SoDonHang", p_SoDonHang)
            };
            DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_DetailPurchase", CommandType.StoredProcedure, l_SqlParameter);
            if (l_DataTable != null && l_DataTable.Rows.Count > 0)
            {
                var l_JsonResult = Json(l_DataTable.EParseToObjects(), JsonRequestBehavior.AllowGet);
                l_JsonResult.MaxJsonLength = int.MaxValue;
                return l_JsonResult;
            }
            return null;
        }
        [HttpPost]
        /// <summary>POST: /RegistrationForm/InstallmentTPBank_GiaiNgan</summary>
        public ActionResult InstallmentTPBank_GiaiNgan(FormCollection p_FormCollection)
        {
            string fName = string.Empty;
            string listFName = string.Empty;
            string l_FolderFileAttach = Keyword.FolderFileAttach;
            var l_StrDatas = p_FormCollection["FormData"].ToString();
            var l_Datas = JsonConvert.DeserializeObject<dynamic>(l_StrDatas);
            TPFico_Customer_Result l__result = new TPFico_Customer_Result();
            #region ===== Save File To Server
            try
            {
                foreach (string l_FileName in Request.Files)
                {
                    HttpPostedFileBase l_FileBase = Request.Files[l_FileName];
                    fName = l_FileName;
                    if (l_FileBase != null && l_FileBase.ContentLength > 0)
                    {
                        // Save file
                        var l_OriginalDirectory = new DirectoryInfo(Server.MapPath(l_FolderFileAttach));
                        string l_PathString = System.IO.Path.Combine(l_OriginalDirectory.ToString(), "");
                        var l_Path_Folder = string.Format("{0}{1}", l_PathString, "TPBank");
                        if (!Directory.Exists(l_Path_Folder))
                        {
                            Directory.CreateDirectory(l_Path_Folder);
                        }
                        var l_Path = string.Format("{0}{1}/{2}", l_PathString, "TPBank", fName);  //  NOTE: Thư mục gốc có dấu '/' đuôi, nên không cần Format "{0}{1}"
                        l_FileBase.SaveAs(l_Path);
                        // Save file attach fo database
                    }
                }


                #region ===== Lưu Data và push sang TPFico
                double l__IDCardCode = double.TryParse(l_Datas[0].IDCardCode.ToString(), out l__IDCardCode) ? l__IDCardCode : 0;
                double l__ID_Final = double.TryParse(l_Datas[0].ID_Final.ToString(), out l__ID_Final) ? l__ID_Final : 0;

                string l__CMND__KH = string.IsNullOrEmpty(l_Datas[0].CMND.ToString()) ? null : l_Datas[0].CMND.ToString();

                string l__CRD_GiayDeNghiVayVon1 = string.IsNullOrEmpty(l_Datas[0].Url_CRD_GiayDeNghiVayVon1.ToString()) ? null : l_Datas[0].Url_CRD_GiayDeNghiVayVon1.ToString();
                string l__Url_CRD_GiayDeNghiVayVon1 = string.IsNullOrEmpty(l__CRD_GiayDeNghiVayVon1) ? null : string.Format("/{0}/{1}", "TPBank", l__CRD_GiayDeNghiVayVon1.Replace("/TPBank/", ""));

                string l__CRD_GiayDeNghiVayVon2 = string.IsNullOrEmpty(l_Datas[0].Url_CRD_GiayDeNghiVayVon2.ToString()) ? null : l_Datas[0].Url_CRD_GiayDeNghiVayVon2.ToString();
                string l__Url_CRD_GiayDeNghiVayVon2 = string.IsNullOrEmpty(l__CRD_GiayDeNghiVayVon2) ? null : string.Format("/{0}/{1}", "TPBank", l__CRD_GiayDeNghiVayVon2.Replace("/TPBank/", ""));

                string l__CRD_GiayDeNghiVayVon3 = string.IsNullOrEmpty(l_Datas[0].Url_CRD_GiayDeNghiVayVon3.ToString()) ? null : l_Datas[0].Url_CRD_GiayDeNghiVayVon3.ToString();
                string l__Url_CRD_GiayDeNghiVayVon3 = string.IsNullOrEmpty(l__CRD_GiayDeNghiVayVon3) ? null : string.Format("/{0}/{1}", "TPBank", l__CRD_GiayDeNghiVayVon3.Replace("/TPBank/", ""));

                string l__CRD_GiayDeNghiVayVon4 = string.IsNullOrEmpty(l_Datas[0].Url_CRD_GiayDeNghiVayVon4.ToString()) ? null : l_Datas[0].Url_CRD_GiayDeNghiVayVon4.ToString();
                string l__Url_CRD_GiayDeNghiVayVon4 = string.IsNullOrEmpty(l__CRD_GiayDeNghiVayVon4) ? null : string.Format("/{0}/{1}", "TPBank", l__CRD_GiayDeNghiVayVon4.Replace("/TPBank/", ""));

                string l__CRD_ChuKyKH = string.IsNullOrEmpty(l_Datas[0].Url_CRD_ChuKyKH.ToString()) ? null : l_Datas[0].Url_CRD_ChuKyKH.ToString();
                string l__Url_CRD_ChuKyKH = string.IsNullOrEmpty(l__CRD_ChuKyKH) ? null : string.Format("/{0}/{1}", "TPBank", l__CRD_ChuKyKH.Replace("/TPBank/", ""));

                string l__CRD_BangKeMuaHH = string.IsNullOrEmpty(l_Datas[0].Url_CRD_BangKeMuaHH.ToString()) ? null : l_Datas[0].Url_CRD_BangKeMuaHH.ToString();
                string l__Url_CRD_BangKeMuaHH = string.IsNullOrEmpty(l__CRD_BangKeMuaHH) ? null : string.Format("/{0}/{1}", "TPBank", l__CRD_BangKeMuaHH.Replace("/TPBank/", ""));

                string l__CRD_GiayGiaoHang = string.IsNullOrEmpty(l_Datas[0].Url_CRD_GiayGiaoHang.ToString()) ? null : l_Datas[0].Url_CRD_GiayGiaoHang.ToString();
                string l__Url_CRD_GiayGiaoHang = string.IsNullOrEmpty(l__CRD_GiayGiaoHang) ? null : string.Format("/{0}/{1}", "TPBank", l__CRD_GiayGiaoHang.Replace("/TPBank/", ""));

                DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_GiaiNgan_Update_Link", System.Data.CommandType.StoredProcedure, new SqlParameter[]{
                new SqlParameter("@IDCardCode", l__IDCardCode),
                new SqlParameter("@ID_Final", l__ID_Final),
                new SqlParameter("@UserCode", l_Datas[0].UserCode.ToString()),
                new SqlParameter("@FromForm", l_Datas[0].FromForm.ToString()),
                new SqlParameter("@Url_CRD_DXTNTD", l__Url_CRD_GiayDeNghiVayVon1),
                new SqlParameter("@Url_CRD_DXTNTD_2", l__Url_CRD_GiayDeNghiVayVon2),
                new SqlParameter("@Url_CRD_DXTNTD_3", l__Url_CRD_GiayDeNghiVayVon3),
                new SqlParameter("@Url_CRD_DXTNTD_4", l__Url_CRD_GiayDeNghiVayVon4),
                new SqlParameter("@Url_CRD_MoThe", l__Url_CRD_ChuKyKH),
                new SqlParameter("@Url_CRD_XacNhanThanhToan", l__Url_CRD_BangKeMuaHH),
                new SqlParameter("@Url_CRD_GiayGiaoHang", l__Url_CRD_GiayGiaoHang),
                });
                if (l_DataTable != null && l_DataTable.Rows.Count > 0)
                {

                    #region ===PUSH API TPFico===
                    if (l_DataTable.Rows[0]["Result"].ToString() == "1")
                    {
                        //TuanNA89 - 06/08/2020 - Tối ưu lại 1 số Code liên quan đến TPBank
                        var apiResult = InstallmentTPBank_TPFico_SendDocumentInfo(l__ID_Final.ToString());
                        l__result = JsonConvert.DeserializeObject<TPFico_Customer_Result>(apiResult.ToString());
                    }
                    #endregion
                }
                else
                {
                    l__result = new TPFico_Customer_Result()
                    {
                        status = "0",
                        messages = l_DataTable.Rows[0]["Msg"].ToString()
                    };
                }
                #endregion
            }
            catch (Exception ex)
            {
                Logger.WriteLogError("RegistrationForm_GiaiNgan - " + MethodBase.GetCurrentMethod().Name + "()", ex.ToString());
                l__result = new TPFico_Customer_Result()
                {
                    status = "0",
                    messages = ex.ToString()
                };
            }
            #endregion

            return Json(l__result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        /// <summary>POST: /RegistrationForm/InstallmentTPBank_UpdateData</summary>
        public ActionResult InstallmentTPBank_UpdateData(FormCollection p_FormCollection)
        {
            TPFico_Customer_Result l__result = new TPFico_Customer_Result();
            string fName = string.Empty;
            string listFName = string.Empty;
            string l_FolderFileAttach = Keyword.FolderFileAttach;
            var l_StrDatas = p_FormCollection["FormData"].ToString();
            var l__ListUpdate = JsonConvert.DeserializeObject<TPFico_ListFieldUpdate>(l_StrDatas);
            var l__ThongTinVayVon = JsonConvert.DeserializeObject<dynamic>(l_StrDatas);
            try
            {
                foreach (string l_FileName in Request.Files)
                {
                    HttpPostedFileBase l_FileBase = Request.Files[l_FileName];
                    fName = l_FileName;
                    if (l_FileBase != null && l_FileBase.ContentLength > 0)
                    {
                        // Save file
                        var l_OriginalDirectory = new DirectoryInfo(Server.MapPath(l_FolderFileAttach));
                        string l_PathString = System.IO.Path.Combine(l_OriginalDirectory.ToString(), "");
                        var l_Path_Folder = string.Format("{0}{1}", l_PathString, "TPBank");
                        if (!Directory.Exists(l_Path_Folder))
                        {
                            Directory.CreateDirectory(l_Path_Folder);
                        }
                        var l_Path = string.Format("{0}{1}/{2}", l_PathString, "TPBank", fName);  //  NOTE: Thư mục gốc có dấu '/' đuôi, nên không cần Format "{0}{1}"
                        l_FileBase.SaveAs(l_Path);
                        // Save file attach fo database
                    }
                }

                double l__IDFinal = double.TryParse(l__ListUpdate.IDFinal, out l__IDFinal) ? l__IDFinal : 0;

                if (l__ListUpdate.ListUpdate != null && l__ListUpdate.ListUpdate.Count() > 0)
                {
                    string l__xmlErrors = "";
                    using (var stringwriter = new System.IO.StringWriter())
                    {
                        var serializer = new XmlSerializer(l__ListUpdate.GetType());
                        serializer.Serialize(stringwriter, l__ListUpdate);
                        l__xmlErrors = stringwriter.ToString();
                    };

                    DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_UpdateData", System.Data.CommandType.StoredProcedure, new SqlParameter[]{
                    new SqlParameter("@CustId", l__IDFinal),
                    new SqlParameter("@strFieldUpdate", l__xmlErrors ),
                    new SqlParameter("@User", UserManager.CurrentUser.InsideCode)
                });
                    if (l_DataTable != null && l_DataTable.Rows.Count > 0)
                    {
                        #region ===PUSH API TPFico===
                        if (l_DataTable.Rows[0]["Result"].ToString() == "1")
                        {
                            //TuanNA89 - 06/08/2020 - Tối ưu lại 1 số Code liên quan đến TPBank
                            var apiResult = InstallmentTPBank_TPFico_TPFico_SendDataUpdate(l__IDFinal.ToString(), l__ListUpdate.Screen);
                            l__result = JsonConvert.DeserializeObject<TPFico_Customer_Result>(apiResult.ToString());
                            //l__result = InstallmentTPBank_TPFico_TPFico_SendDataUpdate(l__IDFinal.ToString(), l__ListUpdate.Screen);
                        }
                        else
                        {
                            l__result = new TPFico_Customer_Result()
                            {
                                status = "0",
                                messages = l_DataTable.Rows[0]["Msg"].ToString()
                            };
                        }
                        #endregion
                    }
                }
                else
                {
                    //Edit - TuanNA89 - 25/06/2019 - Fix lỗi trả về sai status
                    l__result.status = "0";
                    l__result.messages = "Lỗi dữ liệu rỗng";
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogError("RegistrationForm - " + MethodBase.GetCurrentMethod().Name + "()", ex.ToString());
                l__result.status = "0";
                l__result.messages = "Lỗi: " + ex.ToString();
            }

            return Json(l__result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        /// <summary>POST: /RegistrationForm/InstallmentTPBank_UpdateData</summary>
        public ActionResult InstallmentTPBank_SaveImei(FormCollection p_FormCollection)
        {
            TPFico_Customer_Result l__result = new TPFico_Customer_Result();
            var l_StrDatas = p_FormCollection["FormData"].ToString();
            var l_Datas = JsonConvert.DeserializeObject<dynamic>(l_StrDatas);
            try
            {
                double l__ID_Final = double.TryParse(l_Datas.ID_Final.ToString(), out l__ID_Final) ? l__ID_Final : 0;

                DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_SaveImei", System.Data.CommandType.StoredProcedure, new SqlParameter[]{
                    new SqlParameter("@Id_Final", l__ID_Final),
                    new SqlParameter("@DSImei", l_Datas.DSImei.ToString() ),
                    new SqlParameter("@User", UserManager.CurrentUser.InsideCode),
                    new SqlParameter("@So_POSamSung", l_Datas.ThongTinVayVon[0].SoPO_SamSung.ToString() ),
                    new SqlParameter("@MaNVBH", l_Datas.ThongTinVayVon[0].MaNVBH.ToString() ),
                    new SqlParameter("@DiaChiGiaoHang", l_Datas.ThongTinVayVon[0].DiaChiGiaoHang.ToString() ),
                });

                if (l_DataTable != null && l_DataTable.Rows.Count > 0)
                {
                    l__result = new TPFico_Customer_Result()
                    {
                        status = l_DataTable.Rows[0]["Result"].ToString(),
                        messages = l_DataTable.Rows[0]["Msg"].ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogError("RegistrationForm - " + MethodBase.GetCurrentMethod().Name + "()", ex.ToString());
                l__result = new TPFico_Customer_Result()
                {
                    status = "0",
                    messages = "Lỗi " + ex.ToString()
                };
            }

            return Json(l__result, JsonRequestBehavior.AllowGet);
        }
        //▼ Add - TuanNA89 - 28/06/2019 - Thêm rule check KH CIC
        [HttpPost]
        /// <summary>POST: /RegistrationForm/InstallmentTPBank_InsertKHCIC</summary>
        public ActionResult InstallmentTPBank_InsertKHCIC(FormCollection p_FormCollection)
        {
            TPFico_Customer_Result l__result = new TPFico_Customer_Result();

            var l_StrDatas = p_FormCollection["FormData"].ToString();
            var l_Datas = JsonConvert.DeserializeObject<dynamic>(l_StrDatas);

            try
            {
                double l__VendCode = double.TryParse(l_Datas[0].VendCode.ToString(), out l__VendCode) ? l__VendCode : 0;
                DateTime l__Birthday = l_Datas[0].Birthday;
                // Edit - TuanNA89 - 21/06/2019 - Fix lỗi dùng sai kiểu biến. Bool -> int
                int l__Gender = int.TryParse(l_Datas[0].Gender.ToString(), out l__Gender) ? l__Gender : 0;
                DateTime l__NgayCapCMND = l_Datas[0].NgayCapCMND;

                int l__NoiCapCMND = int.TryParse(l_Datas[0].NoiCapCMND.ToString(), out l__NoiCapCMND) ? l__NoiCapCMND : 0;
                int l__TinhTrangHonNhan = int.TryParse(l_Datas[0].TinhTrangHonNhan.ToString(), out l__TinhTrangHonNhan) ? l__TinhTrangHonNhan : 0;

                DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_InsertKHCIC", System.Data.CommandType.StoredProcedure, new SqlParameter[]{
                    new SqlParameter("@CMND", l_Datas[0].CMND.ToString()),
                    new SqlParameter("@VendCode", l__VendCode),
                    new SqlParameter("@CardName", l_Datas[0].CardName.ToString()),
                    new SqlParameter("@Birthday", l__Birthday),
                    new SqlParameter("@Gender", l__Gender),
                    new SqlParameter("@NgayCapCMND", l__NgayCapCMND),
                    new SqlParameter("@NoiCapCMND", l__NoiCapCMND),
                    new SqlParameter("@TinhTrangHonNhan", l__TinhTrangHonNhan),
                    new SqlParameter("@MaNVKH", l_Datas[0].MaNVKH.ToString()),
                    new SqlParameter("@SDT", l_Datas[0].SDT.ToString()),
                    new SqlParameter("@UserCode", l_Datas[0].UserCode.ToString())
                });

                if (l_DataTable != null && l_DataTable.Rows.Count > 0)
                {
                    l__result = new TPFico_Customer_Result()
                    {
                        status = l_DataTable.Rows[0]["Result"].ToString(),
                        messages = l_DataTable.Rows[0]["Msg"].ToString()
                    };
                }
                else
                {
                    l__result = new TPFico_Customer_Result()
                    {
                        status = "0",
                        messages = "Lỗi lưu thông tin khách hàng"
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogError("RegistrationForm - " + MethodBase.GetCurrentMethod().Name + "()", ex.ToString());
                l__result = new TPFico_Customer_Result()
                {
                    status = "0",
                    messages = ex.ToString()
                };
            }

            return Json(l__result, JsonRequestBehavior.AllowGet);
        }
        //▲ Add - TuanNA89 - 28/06/2019 - Thêm rule check KH CIC

        //▼	Edit - VietMXH - 02/07/2019 - Danh sách Hồ sơ TPBank==================================================
        /// <summary>GET: /RegistrationForm/InstallmentTPBank_MonitorProfile</summary>
        public ActionResult InstallmentTPBank_MonitorProfile(string p_Date)
        {
            if (UserManager.CurrentUser != null)
            {
                SqlParameter[] l_SqlParameter = new SqlParameter[] {
                new SqlParameter("@User", UserManager.CurrentUser.InsideCode),
                new SqlParameter("@DateTime", p_Date)
            };
            DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_MonitorProfile", CommandType.StoredProcedure, l_SqlParameter);
            if (l_DataTable != null && l_DataTable.Rows.Count > 0)
            {
                var l_JsonResult = Json(l_DataTable.EParseToObjects(), JsonRequestBehavior.AllowGet);
                l_JsonResult.MaxJsonLength = int.MaxValue;
                return l_JsonResult;
            }
            }
            return null;
        }
        //▲	Edit - VietMXH - 02/07/2019 - Danh sách Hồ sơ TPBank==================================================

        //▼	Edit - TuanNA89 - 08/07/2019 - Search Danh sách Hồ sơ TPBank
        /// <summary>GET: /RegistrationForm/InstallmentTPBank_MonitorProfile</summary>
        public ActionResult InstallmentTPBank_GetProfile(string p_CMND, string p_Date)
        {
            SqlParameter[] l_SqlParameter = new SqlParameter[] {
                new SqlParameter("@CMND", p_CMND),
                new SqlParameter("@User", UserManager.CurrentUser.InsideCode),
                new SqlParameter("@DateTime", p_Date)
            };
            DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_GetProfile", CommandType.StoredProcedure, l_SqlParameter);
            if (l_DataTable != null && l_DataTable.Rows.Count > 0)
            {
                var l_JsonResult = Json(l_DataTable.EParseToObjects(), JsonRequestBehavior.AllowGet);
                l_JsonResult.MaxJsonLength = int.MaxValue;
                return l_JsonResult;
            }
            return null;
        }
        //▲	Edit - TuanNA89 - 08/07/2019 - Search Danh sách Hồ sơ TPBank
        #endregion

        #region ===Function gọi Api TPBank===
        // TuanNA89 - 06/08/2020 - Tối ưu lại 1 số Code liên quan đến TPBank
        #region === Code cũ ===
        //▼	Add - TuanNA89 - 22/07/2019 - Lưu Token của TPBank
        //public string InstallmentTPBank_TPFico_GetAuthorization()
        //{
        //    /*
        //     * Rule:
        //        B1: gán dữ liệu từ biến token global -> token local
        //        B2: nếu token có data -> lấy (time hiện tại - time lấy token)
        //                - nếu kết quả > time hết hạn -> set token rỗng
        //        B3: 
        //            check nếu token rỗng -> gọi API lấy token -> set thời gian lấy token + thời gian hết hạn
        //     */
        //    string l_Authorization = "";
        //    string l_Function = MethodBase.GetCurrentMethod().Name, l_Method = "POST", l_Url = "", l_jsData = "", l_Error = "";
        //    try
        //    {
        //        l_Authorization = g__Token__TPBank;

        //        if (l_Authorization != "")
        //        {
        //            var seconds = (DateTime.Now - DateTime.Parse(g__TimeCreateToken_TPBank.ToString())).TotalSeconds;
        //            if (seconds >= g__TimeExpires__Second)
        //            {
        //                l_Authorization = "";
        //            }
        //        }

        //        if (l_Authorization == "")
        //        {
        //            l_Url = ConfigurationManager.AppSettings["TPFico_Url"].ToString() + "/api/auth/oauth/token";
        //            //▼ Add - TuanNA89 - 08/11/2019 - Fix lỗi mất kết nối API TPBank
        //            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        //            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
        //                delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
        //                                        System.Security.Cryptography.X509Certificates.X509Chain chain,
        //                                        System.Net.Security.SslPolicyErrors sslPolicyErrors)
        //                {
        //                    return true; // **** Always accept
        //                };
        //            System.Net.ServicePointManager.Expect100Continue = false;
        //            //▲ Add - TuanNA89 - 08/11/2019 - Fix lỗi mất kết nối API TPBank
        //            var l__HttpWebReq = (HttpWebRequest)WebRequest.Create(l_Url);
        //            l__HttpWebReq.KeepAlive = false;
        //            l__HttpWebReq.Method = l_Method;
        //            l__HttpWebReq.ContentType = "application/x-www-form-urlencoded";

        //            string Username = "3p-service-fpt";
        //            string Password = "3p-service-fpt";

        //            string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Username + ":" + Password));
        //            l__HttpWebReq.Headers.Add("Authorization", "Basic " + svcCredentials);

        //            string urlEncode = "grant_type=" + HttpUtility.UrlEncode("client_credentials");

        //            using (StreamWriter stOut = new StreamWriter(l__HttpWebReq.GetRequestStream(), System.Text.Encoding.ASCII))
        //            {
        //                stOut.Write(urlEncode);
        //                stOut.Close();
        //            }

        //            using (HttpWebResponse l__oResp = l__HttpWebReq.GetResponse() as HttpWebResponse)
        //            {
        //                using (StreamReader l__StreamReader = new StreamReader(l__oResp.GetResponseStream()))
        //                {
        //                    var l_ResponseFromServer = l__StreamReader.ReadToEnd();
        //                    if (l__oResp.StatusCode == HttpStatusCode.OK)
        //                    {
        //                        var result = JObject.Parse(l_ResponseFromServer);
        //                        l_Authorization = result["token_type"].ToString() + " " + result["access_token"].ToString();

        //                        g__TimeExpires__Second = int.TryParse(result["expires_in"].ToString(), out g__TimeExpires__Second) ? g__TimeExpires__Second : 0;
        //                        g__TimeCreateToken_TPBank = DateTime.Now;

        //                        if (l_Authorization != g__Token__TPBank)
        //                        {
        //                            g__Token__TPBank = l_Authorization;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        WriteLog_TPBank(l_Function, l_Method, l_Url, l_jsData, "", l__oResp.StatusCode.ToString() + "-" + l_ResponseFromServer);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.WriteLogError("RegistrationForm - " + MethodBase.GetCurrentMethod().Name + "()", "Link API: " + l_Url + " - Error:" + ex.ToString());
        //        WriteLog_TPBank(l_Function, l_Method, l_Url, l_jsData, "", ex.ToString());
        //    }
        //    return l_Authorization;
        //}
        ////▲	Add - TuanNA89 - 22/07/2019 - Lưu Token của TPBank
        ////▼	Add - TuanNA89 - 21/10/2019 - Thêm code hỗ trợ việc gọi API TPBank
        //public TPFico_Customer_Result InstallmentTPBank_TPFico_SendCustInfo(string p_IdFinal)
        //{
        //    TPFico_Customer_Result l__result = new TPFico_Customer_Result();
        //    int l__IdLog = 0;
        //    l__IdLog = WriteLog(0, 0, "InstallmentTPBank_TPFico_SendCustInfo", p_IdFinal, "");
        //    string l__Step = "";
        //    HttpWebResponse l__oResp = null;
        //    try
        //    {
        //        SqlParameter[] l_SqlParameter = new SqlParameter[]{
        //            new SqlParameter("@Id_Final", p_IdFinal)
        //        };
        //        l__Step = "Gọi store";
        //        DataSet l_DataSet = g_SqlDBHelper.ExecuteCommandDataSet("sp_InstallmentTPBank_CustInfo_ForAPI", CommandType.StoredProcedure, l_SqlParameter);
        //        if (l_DataSet != null && l_DataSet.Tables.Count > 0)
        //        {
        //            DataTable l_CustInfo = l_DataSet.Tables[0];
        //            DataTable l_ProdDetails = l_DataSet.Tables[1];
        //            DataTable l_Ref_Address = l_DataSet.Tables[3];
        //            if (l_CustInfo != null && l_CustInfo.Rows.Count > 0)
        //            {
        //                l__Step = "Lấy token";
        //                string l_Key = InstallmentTPBank_TPFico_GetAuthorization();

        //                if (l_Key != null && l_Key != "")
        //                {
        //                    #region == Add data ==
        //                    l__Step = "Add detail product";
        //                    #region == Add Detail Products ==
        //                    List<TPFico_CustInfo_ProductDetail> l_ListProducts = new List<TPFico_CustInfo_ProductDetail> { };
        //                    if (l_ProdDetails != null && l_ProdDetails.Rows.Count > 0)
        //                    {
        //                        foreach (DataRow item in l_ProdDetails.Rows)
        //                        {
        //                            l_ListProducts.Add(new TPFico_CustInfo_ProductDetail
        //                            {
        //                                model = item["TenSanPham"].ToString(),//Model of the goods
        //                                goodCode = item["MaSanPham"].ToString(),//Goods code of dealer
        //                                goodType = item["LoaiHang"].ToString(),//Type of goods: Non portable/portable
        //                                quantity = item["SoLuong"].ToString(),//Quantity
        //                                goodPrice = item["GiaSanPham"].ToString()//Price of goods
        //                            });

        //                        }
        //                    }
        //                    #endregion
        //                    l__Step = "Add Referenses";
        //                    #region == Add References ==
        //                    var l__ListReferences = new List<TPFico_CustInfo_Reference>();
        //                    if (l_CustInfo.Rows[0]["NguoiThan1_HoTen"].ToString() != "")
        //                    {
        //                        l__ListReferences.Add(new TPFico_CustInfo_Reference()
        //                        {
        //                            fullName = l_CustInfo.Rows[0]["NguoiThan1_HoTen"].ToString(),//Reference person name
        //                            phoneNumber = l_CustInfo.Rows[0]["NguoiThan1_SDT"].ToString(),//Reference person’ phone number
        //                            relation = l_CustInfo.Rows[0]["NguoiThan1_QuanHe"].ToString(),//Relationship with owner. “husband, wife, workmate, relatives”
        //                            personalId = l_CustInfo.Rows[0]["NguoiThan1_CMND"].ToString()//National ID of the reference                                    
        //                        });
        //                    }

        //                    if (l_CustInfo.Rows[0]["NguoiThan2_HoTen"].ToString() != "")
        //                    {
        //                        l__ListReferences.Add(new TPFico_CustInfo_Reference()
        //                        {
        //                            fullName = l_CustInfo.Rows[0]["NguoiThan2_HoTen"].ToString(),//Reference person name
        //                            phoneNumber = l_CustInfo.Rows[0]["NguoiThan2_SDT"].ToString(),//Reference person’ phone number
        //                            relation = l_CustInfo.Rows[0]["NguoiThan2_QuanHe"].ToString(),//Relationship with owner. “husband, wife, workmate, relatives”
        //                            personalId = l_CustInfo.Rows[0]["NguoiThan2_CMND"].ToString()//National ID of the reference                                    
        //                        });
        //                    }

        //                    if (l_CustInfo.Rows[0]["NguoiHonPhoi_HoTen"].ToString() != "")
        //                    {
        //                        l__ListReferences.Add(new TPFico_CustInfo_Reference()
        //                        {
        //                            fullName = l_CustInfo.Rows[0]["NguoiHonPhoi_HoTen"].ToString(),//Reference person name
        //                            phoneNumber = l_CustInfo.Rows[0]["NguoiHonPhoi_SDT"].ToString(),//Reference person’ phone number
        //                            relation = l_CustInfo.Rows[0]["NguoiHonPhoi_QuanHe"].ToString(),
        //                            personalId = l_CustInfo.Rows[0]["NguoiHonPhoi_CMND"].ToString()//National ID of the reference                                    
        //                        });
        //                    }
        //                    #endregion
        //                    l__Step = "Add Address";
        //                    #region == Add Address ==
        //                    var l__ListAddresses = new List<TPFico_CustInfo_Address>();
        //                    l__ListAddresses.Add(new TPFico_CustInfo_Address
        //                    {
        //                        addressType = "Current Address",//Current, family book
        //                        address1 = l_CustInfo.Rows[0]["TamTru_ToaNha"].ToString(),//Address number & Street
        //                        address2 = l_CustInfo.Rows[0]["TamTru_DiaChi"].ToString(),
        //                        ward = l_CustInfo.Rows[0]["TamTru_PhuongXa"].ToString(),//Ward
        //                        district = l_CustInfo.Rows[0]["TamTru_QuanHuyen"].ToString(),//District
        //                        province = l_CustInfo.Rows[0]["TamTru_TinhThanh"].ToString()//City
        //                    });
        //                    l__ListAddresses.Add(new TPFico_CustInfo_Address
        //                    {
        //                        addressType = "Family Book Address",//Current, family book
        //                        address1 = l_CustInfo.Rows[0]["HoKhau_ToaNha"].ToString(),
        //                        address2 = l_CustInfo.Rows[0]["HoKhau_DiaChi"].ToString(),
        //                        ward = l_CustInfo.Rows[0]["HoKhau_PhuongXa"].ToString(),//Ward
        //                        district = l_CustInfo.Rows[0]["HoKhau_QuanHuyen"].ToString(),//District
        //                        province = l_CustInfo.Rows[0]["HoKhau_TinhThanh"].ToString()//City
        //                    });

        //                    l__ListAddresses.Add(new TPFico_CustInfo_Address
        //                    {
        //                        addressType = "Working Address",//Current, family book
        //                        address1 = l_Ref_Address.Rows[0]["LamViec_ToaNha"].ToString(),
        //                        address2 = l_Ref_Address.Rows[0]["LamViec_DiaChi"].ToString(),
        //                        ward = l_Ref_Address.Rows[0]["LamViec_PhuongXa"].ToString(),//Ward
        //                        district = l_Ref_Address.Rows[0]["LamViec_QuanHuyen"].ToString(),//District
        //                        province = l_Ref_Address.Rows[0]["LamViec_TinhThanh"].ToString()//City
        //                    });

        //                    if (l_CustInfo.Rows[0]["NguoiHonPhoi_HoTen"].ToString() != "")
        //                    {
        //                        l__ListAddresses.Add(new TPFico_CustInfo_Address
        //                        {
        //                            addressType = "Spouse Address",//Current, family book
        //                            address1 = l_CustInfo.Rows[0]["HonPhoi_ToaNha"].ToString(),
        //                            address2 = l_CustInfo.Rows[0]["HonPhoi_DiaChi"].ToString(),
        //                            ward = l_CustInfo.Rows[0]["HonPhoi_PhuongXa"].ToString(),//Ward
        //                            district = l_CustInfo.Rows[0]["HonPhoi_QuanHuyen"].ToString(),//District
        //                            province = l_CustInfo.Rows[0]["HonPhoi_TinhThanh"].ToString()//City
        //                        });
        //                    }


        //                    #endregion
        //                    l__Step = "Add Images";
        //                    #region == Add Url Images ==
        //                    var l__ListPhotos = new List<TPFico_CustInfo_Photo>();
        //                    List<string> l__Files = new List<string> { };
        //                    string l__Url__NewImage = "";
        //                    l__ListPhotos.Add(new TPFico_CustInfo_Photo
        //                    {
        //                        link = (l_CustInfo.Rows[0]["Url_CRD_CD_DKMoThe"].ToString() == "") ? "" : (authority + l_CustInfo.Rows[0]["ImagePath"].ToString() + l_CustInfo.Rows[0]["Url_CRD_CD_DKMoThe"].ToString()),
        //                        documentType = "selfie"
        //                    });

        //                    l__ListPhotos.Add(new TPFico_CustInfo_Photo
        //                    {
        //                        link = (l_CustInfo.Rows[0]["Url_CRD_KH_CMND"].ToString() == "") ? "" : (authority + l_CustInfo.Rows[0]["ImagePath"].ToString() + l_CustInfo.Rows[0]["Url_CRD_KH_CMND"].ToString()),
        //                        documentType = "employeecard"
        //                    });

        //                    string date = DateTime.Now.ToString("yyyyMMdd");

        //                    #region == Gộp CMND ==

        //                    l__Files.Add(l_CustInfo.Rows[0]["Url_CRD_MT_CMND"].ToString());
        //                    l__Files.Add(l_CustInfo.Rows[0]["Url_CRD_MS_CMND"].ToString());
        //                    l__Url__NewImage = MergeImages(l__Files, "/TPBank/" + date + "/", "CMND");
        //                    if (l__Url__NewImage != "")
        //                    {
        //                        l__ListPhotos.Add(new TPFico_CustInfo_Photo
        //                        {
        //                            link = (l__Url__NewImage == "") ? "" : authority + l_CustInfo.Rows[0]["ImagePath"].ToString() + l__Url__NewImage,
        //                            documentType = "National_ID"
        //                        });
        //                        #endregion

        //                        #region == Gộp Sổ Hộ Khẩu ==
        //                        l__Files.Clear();
        //                        if ((l_CustInfo.Rows[0]["Url_CRD_SHK_1"].ToString() != null) && (l_CustInfo.Rows[0]["Url_CRD_SHK_1"].ToString() != ""))
        //                            l__Files.Add(l_CustInfo.Rows[0]["Url_CRD_SHK_1"].ToString());

        //                        if ((l_CustInfo.Rows[0]["Url_CRD_SHK_2"].ToString() != null) && (l_CustInfo.Rows[0]["Url_CRD_SHK_2"].ToString() != ""))
        //                            l__Files.Add(l_CustInfo.Rows[0]["Url_CRD_SHK_2"].ToString());

        //                        if ((l_CustInfo.Rows[0]["Url_CRD_SHK_3"].ToString() != null) && (l_CustInfo.Rows[0]["Url_CRD_SHK_3"].ToString() != ""))
        //                            l__Files.Add(l_CustInfo.Rows[0]["Url_CRD_SHK_3"].ToString());

        //                        if ((l_CustInfo.Rows[0]["Url_CRD_SHK_4"].ToString() != null) && (l_CustInfo.Rows[0]["Url_CRD_SHK_4"].ToString() != ""))
        //                            l__Files.Add(l_CustInfo.Rows[0]["Url_CRD_SHK_4"].ToString());

        //                        if ((l_CustInfo.Rows[0]["Url_CRD_SHK_5"].ToString() != null) && (l_CustInfo.Rows[0]["Url_CRD_SHK_5"].ToString() != ""))
        //                            l__Files.Add(l_CustInfo.Rows[0]["Url_CRD_SHK_5"].ToString());

        //                        if ((l_CustInfo.Rows[0]["Url_CRD_SHK_6"].ToString() != null) && (l_CustInfo.Rows[0]["Url_CRD_SHK_6"].ToString() != ""))
        //                            l__Files.Add(l_CustInfo.Rows[0]["Url_CRD_SHK_6"].ToString());

        //                        if ((l_CustInfo.Rows[0]["Url_CRD_SHK_7"].ToString() != null) && (l_CustInfo.Rows[0]["Url_CRD_SHK_7"].ToString() != ""))
        //                            l__Files.Add(l_CustInfo.Rows[0]["Url_CRD_SHK_7"].ToString());

        //                        if (l__Files.Count > 0)
        //                        {
        //                            l__Url__NewImage = MergeImages(l__Files, "/TPBank/" + date + "/", "SHK");
        //                            l__ListPhotos.Add(new TPFico_CustInfo_Photo
        //                            {
        //                                link = (l__Url__NewImage == "") ? "" : authority + l_CustInfo.Rows[0]["ImagePath"].ToString() + l__Url__NewImage,
        //                                documentType = "fb"
        //                            });
        //                        }
        //                        #endregion

        //                        #endregion
        //                        l__Step = "Add Order Infor";
        //                        var l_ObjectData = new TPFico_CustInfo()
        //                        {
        //                            custId = l_CustInfo.Rows[0]["CustId"].ToString(),//Loan id
        //                            lastName = l_CustInfo.Rows[0]["LastName"].ToString(),//Last name of Customer
        //                            firstName = l_CustInfo.Rows[0]["FirstName"].ToString(),//First name of Customer
        //                            middleName = l_CustInfo.Rows[0]["MidName"].ToString(),//Middle name of Customer
        //                            gender = l_CustInfo.Rows[0]["Gender"].ToString(),//Gender
        //                            dateOfBirth = l_CustInfo.Rows[0]["Birthday"].ToString(),//Date of Birth
        //                            nationalId = l_CustInfo.Rows[0]["CMND"].ToString(),//National ID of Customer
        //                            issueDate = l_CustInfo.Rows[0]["NgayCapCMND"].ToString(),//Issued Date of ID
        //                            issuePlace = l_CustInfo.Rows[0]["NoiCapCMND"].ToString(),
        //                            employeeCard = l_CustInfo.Rows[0]["MaNV_KH"].ToString(),//Employee card number
        //                            mobilePhone = l_CustInfo.Rows[0]["SDT"].ToString(),//Mobile Phone
        //                            durationYear = l_CustInfo.Rows[0]["ThoiGianCuTru_Nam"].ToString(),//Duration of living at current address (Year)
        //                            durationMonth = l_CustInfo.Rows[0]["ThoiGianCuTru_Thang"].ToString(),//Duration of living at current address (Month)
        //                            map = l_CustInfo.Rows[0]["DiaChi_HuongDan"].ToString(),//Guideline to the address
        //                            ownerNationalId = l_CustInfo.Rows[0]["CMND_ChuHo"].ToString(),//Owner National ID
        //                            contactAddress = l_CustInfo.Rows[0]["DiaChiLienHe_TypeName"].ToString(),//Contact Address
        //                            maritalStatus = l_CustInfo.Rows[0]["maritalStatus"].ToString(),
        //                            dsaCode = l_CustInfo.Rows[0]["MaNVFPT"].ToString(),//Code of Sale
        //                            companyName = l_CustInfo.Rows[0]["TenCongTyKHLamViec"].ToString(),
        //                            taxCode = l_CustInfo.Rows[0]["MSTCtyKH"].ToString(),
        //                            salary = l_CustInfo.Rows[0]["ThuNhapHangThang"].ToString(),// Add - TuanNA89 - 02/07/2019 - Thêm thu nhập hàng tháng
        //                            loanDetail = new TPFico_CustInfo_LoanDetail
        //                            {
        //                                product = l_CustInfo.Rows[0]["SanPhamVay"].ToString(),//Product Applied
        //                                loanAmount = l_CustInfo.Rows[0]["TienVay"].ToString(),//Loan amount requested
        //                                downPayment = l_CustInfo.Rows[0]["TraTruoc"].ToString(),//Amount of down payment for total bill
        //                                annualr = l_CustInfo.Rows[0]["LaiSuat"].ToString(),//Annual interest rate
        //                                dueDate = l_CustInfo.Rows[0]["NgayThanhToanHangThang"].ToString(),
        //                                tenor = l_CustInfo.Rows[0]["KyHan"].ToString(),//Tenor of the loan
        //                            },
        //                            addresses = l__ListAddresses,
        //                            photos = l__ListPhotos,
        //                            productDetails = l_ListProducts, //Product details
        //                            references = l__ListReferences
        //                        };
        //                        #endregion
        //                        l__Step = "Add xong data rồi";
        //                        #region == Push data ==
        //                        var l__jsData = new JavaScriptSerializer().Serialize(l_ObjectData);
        //                        l__Step = "Mã hoá data";
        //                        var l__encryptData = PGPEncrypt_SignAndEncrypt(l__jsData);

        //                        string l__ReqUriStr = ConfigurationManager.AppSettings["TPFico_Url"].ToString() + Api_TPBank.GuiThongTinKH;
        //                        l__Step = "Lấy link bước 1";
        //                        //▼ Add - TuanNA89 - 08/11/2019 - Fix lỗi mất kết nối API TPBank
        //                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        //                        System.Net.ServicePointManager.ServerCertificateValidationCallback +=
        //                            delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
        //                                                    System.Security.Cryptography.X509Certificates.X509Chain chain,
        //                                                    System.Net.Security.SslPolicyErrors sslPolicyErrors)
        //                            {
        //                                return true; // **** Always accept
        //                            };
        //                        System.Net.ServicePointManager.Expect100Continue = false;
        //                        //▲ Add - TuanNA89 - 08/11/2019 - Fix lỗi mất kết nối API TPBank
        //                        WriteLog(0, 0, "InstallmentTPBank_TPFico_SendCustInfo", l__ReqUriStr, "");
        //                        var l__HttpWebReq = (HttpWebRequest)WebRequest.Create(l__ReqUriStr);
        //                        l__HttpWebReq.KeepAlive = false;
        //                        l__HttpWebReq.Method = "POST";
        //                        l__HttpWebReq.ContentType = "application/json";
        //                        l__HttpWebReq.Headers.Add("Authorization", l_Key);
        //                        l__Step = "Bắt đầu gọi API";

        //                        using (Stream stm = l__HttpWebReq.GetRequestStream())
        //                        {
        //                            using (StreamWriter stmw = new StreamWriter(stm))
        //                            {
        //                                stmw.Write(l__encryptData);
        //                            }
        //                        }
        //                        HttpStatusCode? StatusCode = null;
        //                        l__Step = "Lấy chuỗi response";
        //                        try
        //                        {
        //                            l__Step = "Bắt đầu chạy API";
        //                            l__oResp = (HttpWebResponse)l__HttpWebReq.GetResponse();
        //                        }
        //                        catch (WebException we)
        //                        {
        //                            l__Step = "API lỗi";
        //                            l__oResp = (HttpWebResponse)we.Response;
        //                        }
        //                        l__Step = "Lấy chuỗi Stream của API";
        //                        var webResponseStream = l__oResp.GetResponseStream();
        //                        l__Step = "Check chuỗi ResponseStream";
        //                        if (webResponseStream != null && webResponseStream != Stream.Null)
        //                        {
        //                            l__Step = "Chuỗi ResponseStream không null";
        //                            l__Step = "Lấy StatusCode";
        //                            StatusCode = l__oResp.StatusCode;
        //                        }
        //                        int? statusCode = null;
        //                        string statusName = null;
        //                        l__Step = "Check StatusCode";
        //                        if (StatusCode != null)
        //                        {
        //                            l__Step = "StatusCode không null";
        //                            statusCode = (int)StatusCode;
        //                            statusName = StatusCode.ToString();
        //                        }
        //                        if ((StatusCode == HttpStatusCode.Created) || (StatusCode == HttpStatusCode.OK))
        //                        {
        //                            l__Step = "Thành công";
        //                            l__result = new TPFico_Customer_Result()
        //                            {
        //                                status = "1",
        //                                messages = "Gửi thông tin khách hàng sang TPBank thành công! Mã khách hàng TPBank: " + l_CustInfo.Rows[0]["CustId"].ToString()
        //                            };
        //                            Status status = Status.PROCESSING;
        //                            statusCode = (int)status;

        //                            g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_TPFico_PushResult", CommandType.StoredProcedure, new SqlParameter[]{
        //                                new SqlParameter("@Result", statusCode),
        //                                new SqlParameter("@Msg", "Gửi thông tin thành công!"),
        //                                new SqlParameter("@CustId", l_ObjectData.custId),
        //                            });
        //                        }
        //                        else
        //                        {
        //                            l__Step = "Status trả về lỗi";
        //                            StreamReader l__StreamReader = new StreamReader(l__oResp.GetResponseStream());

        //                            var l_data = l__StreamReader.ReadToEnd();
        //                            l__Step = "Giải mã chuỗi Response lỗi";
        //                            var l__Json_Decrypt = PGPEncrypt_SignAndDecrypt(l_data);
        //                            l__Step = "Gán vào list lỗi";
        //                            l__result = new JavaScriptSerializer().Deserialize<TPFico_Customer_Result>(l__Json_Decrypt);
        //                            WriteLog(0, 0, "InstallmentTPBank_TPFico_SendCustInfo", string.Format("{0} - {1}", statusCode.ToString(), statusName), l__Json_Decrypt);
        //                            l__Step = "Trả lỗi về";
        //                            l__result.status = "0";
        //                            //Add - TuanNA89 - 31/07/2019 - Thêm Note nơi trả về lỗi
        //                            l__result.messages = "Lỗi TPBank trả về: " + l__result.messages;
        //                            //Add - TuanNA89 - 31/07/2019 - Check thêm trường hợp TPBank trả về lỗi "Lỗi gọi dịch vụ web" thì vẫn tính là thành công
        //                            if (l__result.messages.Contains("Lỗi gọi dịch vụ web cust id"))
        //                            {
        //                                l__Step = "Thành công";
        //                                l__result = new TPFico_Customer_Result()
        //                                {
        //                                    status = "1",
        //                                    messages = "Gửi thông tin khách hàng sang TPBank thành công! Mã khách hàng TPBank: " + l_CustInfo.Rows[0]["CustId"].ToString()
        //                                };
        //                                Status status = Status.PROCESSING;
        //                                statusCode = (int)status;

        //                                g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_TPFico_PushResult", CommandType.StoredProcedure, new SqlParameter[]{
        //                                new SqlParameter("@Result", statusCode),
        //                                new SqlParameter("@Msg", "Gửi thông tin thành công!"),
        //                                new SqlParameter("@CustId", l_ObjectData.custId),
        //                            });
        //                            }
        //                            //Add - TuanNA89 - 31/07/2019 - Check thêm trường hợp TPBank trả về lỗi "Lỗi gọi dịch vụ web" thì vẫn tính là thành công

        //                        }
        //                        #endregion
        //                    }
        //                    else
        //                    {
        //                        l__result = new TPFico_Customer_Result()
        //                        {
        //                            status = "0",
        //                            messages = "Xảy ra lỗi khi gộp hình"
        //                        };
        //                        WriteLog(0, 0, "InstallmentTPBank_TPFico_SendCustInfo", "", "Xảy ra lỗi khi gộp hình");
        //                    }
        //                }
        //                else
        //                {
        //                    l__result = new TPFico_Customer_Result()
        //                    {
        //                        status = "0",
        //                        messages = "Không lấy được chuỗi kết nối TPBank"
        //                    };
        //                    WriteLog(0, 0, "InstallmentTPBank_TPFico_SendCustInfo", "", "Không lấy được chuỗi kết nối TPBank");
        //                }
        //            }
        //            else
        //            {
        //                l__result = new TPFico_Customer_Result()
        //                {
        //                    status = "0",
        //                    messages = "Không tìm thấy thông tin khách hàng"
        //                };
        //                WriteLog(0, 0, "InstallmentTPBank_TPFico_SendCustInfo", "", "Không tìm thấy thông tin khách hàng");
        //            }
        //        }
        //        else
        //        {
        //            l__result = new TPFico_Customer_Result()
        //            {
        //                status = "0",
        //                messages = "Không lấy được thông tin khách hàng"
        //            };
        //            WriteLog(0, 0, "InstallmentTPBank_TPFico_SendCustInfo", "", "Không lấy được thông tin khách hàng");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string l__error = "- Bước bị lỗi: " + l__Step + "- Lỗi: " + ex.ToString();
        //        Logger.WriteLogError("RegistrationForm - " + MethodBase.GetCurrentMethod().Name + "()", l__error);
        //        string msgError = "";
        //        if (ex.ToString().Contains("Unable to connect to the remote server"))
        //        {
        //            msgError = "Không kết nối được tới TPBank";
        //        }
        //        else
        //        {
        //            msgError = ex.ToString();
        //        }

        //        l__result = new TPFico_Customer_Result()
        //        {
        //            status = "0",
        //            messages = msgError
        //        };
        //        WriteLog(0, 0, "InstallmentTPBank_TPFico_SendCustInfo", "", l__error);
        //    }
        //    finally
        //    {
        //        if (l__oResp != null)
        //        {
        //            l__oResp.Close();
        //        }
        //    }
        //    return l__result;
        //}
        //public TPFico_Customer_Result InstallmentTPBank_TPFico_SendDocumentInfo(string p_IdFinal)
        //{
        //    TPFico_Customer_Result l__result = new TPFico_Customer_Result();
        //    int l__IdLog = 0;
        //    l__IdLog = WriteLog(0, 0, "InstallmentTPBank_TPFico_SendDocumentInfo", p_IdFinal, "");
        //    HttpWebResponse l__oResp = null;
        //    string l__Step = "";
        //    try
        //    {
        //        l__Step = "Lấy token";
        //        string l_Authorization = InstallmentTPBank_TPFico_GetAuthorization();
        //        if (l_Authorization == null || l_Authorization == "")
        //        {
        //            l__result = new TPFico_Customer_Result()
        //            {
        //                status = "0",
        //                messages = "Không lấy được chuỗi kết nối TPBank"
        //            };
        //        }
        //        else
        //        {
        //            SqlParameter[] l_SqlParameter = new SqlParameter[]{
        //                new SqlParameter("@Id_Final", p_IdFinal)
        //            };
        //            l__Step = "Gọi store lấy data";
        //            DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_CustInfo_ForAPI", CommandType.StoredProcedure, l_SqlParameter);
        //            if (l_DataTable != null && l_DataTable.Rows.Count > 0)
        //            {
        //                #region == Gộp hình ==

        //                List<string> l__Files = new List<string> { };
        //                string l__Url__NewImage = "";
        //                l__Step = "Add hình";
        //                #region == Gộp hình đề nghị vay vốn ==                        
        //                if ((l_DataTable.Rows[0]["Url_CRD_DXTNTD"].ToString() != null) && (l_DataTable.Rows[0]["Url_CRD_DXTNTD"].ToString() != ""))
        //                    l__Files.Add(l_DataTable.Rows[0]["Url_CRD_DXTNTD"].ToString());

        //                if ((l_DataTable.Rows[0]["Url_CRD_DXTNTD_2"].ToString() != null) && (l_DataTable.Rows[0]["Url_CRD_DXTNTD_2"].ToString() != ""))
        //                    l__Files.Add(l_DataTable.Rows[0]["Url_CRD_DXTNTD_2"].ToString());

        //                if ((l_DataTable.Rows[0]["Url_CRD_DXTNTD_3"].ToString() != null) && (l_DataTable.Rows[0]["Url_CRD_DXTNTD_3"].ToString() != ""))
        //                    l__Files.Add(l_DataTable.Rows[0]["Url_CRD_DXTNTD_3"].ToString());

        //                if ((l_DataTable.Rows[0]["Url_CRD_DXTNTD_4"].ToString() != null) && (l_DataTable.Rows[0]["Url_CRD_DXTNTD_4"].ToString() != ""))
        //                    l__Files.Add(l_DataTable.Rows[0]["Url_CRD_DXTNTD_4"].ToString());

        //                string date = DateTime.Now.ToString("yyyyMMdd");
        //                l__Step = "Gộp hình";
        //                if (l__Files.Count > 0)
        //                    l__Url__NewImage = MergeImages(l__Files, "/TPBank/" + date + "/", "ACCA");
        //                #endregion
        //                #endregion

        //                if (l__Url__NewImage != "")
        //                {
        //                    #region == Add Data ==
        //                    l__Step = "Gộp Images";
        //                    TPFico_DocumentInfo[] l_ObjectData =
        //                    {
        //                        new TPFico_DocumentInfo{
        //                            file = (l__Url__NewImage == "" ) ? "" : authority + l_DataTable.Rows[0]["ImagePath"].ToString() + l__Url__NewImage,
        //                            documentCode= "ACCA"
        //                        },
        //                        new TPFico_DocumentInfo{
        //                            file= (l_DataTable.Rows[0]["Url_CRD_MoThe"].ToString() == "") ? "" : (authority + l_DataTable.Rows[0]["ImagePath"].ToString() + l_DataTable.Rows[0]["Url_CRD_MoThe"].ToString()),
        //                            documentCode= "Signature"
        //                        },
        //                        //Edit - TuanNA89 - 13/08/2019 - Bỏ bảng kê mua hàng hoá
        //                        //new TPFico_DocumentInfo{
        //                        //    file= (l_DataTable.Rows[0]["Url_CRD_XacNhanThanhToan"].ToString() == "") ? "" : (authority + l_DataTable.Rows[0]["ImagePath"].ToString() + l_DataTable.Rows[0]["Url_CRD_XacNhanThanhToan"].ToString()),
        //                        //    documentCode= "Detail of stock"
        //                        //},
        //                        //Edit - TuanNA89 - 13/08/2019 - Bỏ bảng kê mua hàng hoá
        //                        new TPFico_DocumentInfo{
        //                            file= (l_DataTable.Rows[0]["Url_CRD_GiayGiaoHang"].ToString() == "") ? "" : (authority + l_DataTable.Rows[0]["ImagePath"].ToString() + l_DataTable.Rows[0]["Url_CRD_GiayGiaoHang"].ToString()),
        //                            documentCode= "Delivery note"
        //                        },
        //                    };
        //                    #endregion

        //                    l__Step = "Đẩy data giải ngân sang TPBank";
        //                    #region == Push data to TPFico ==
        //                    var l__jsData = new JavaScriptSerializer().Serialize(l_ObjectData);
        //                    l__Step = "Mã hoá data";
        //                    var l__encryptData = PGPEncrypt_SignAndEncrypt(l__jsData);

        //                    string l__ReqUriStr = ConfigurationManager.AppSettings["TPFico_Url"].ToString() + String.Format(Api_TPBank.GuiThongTinGiaiNgan, l_DataTable.Rows[0]["CustId"].ToString());//Customer id
        //                    //▼ Add - TuanNA89 - 08/11/2019 - Fix lỗi mất kết nối API TPBank
        //                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        //                    System.Net.ServicePointManager.ServerCertificateValidationCallback +=
        //                        delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
        //                                                System.Security.Cryptography.X509Certificates.X509Chain chain,
        //                                                System.Net.Security.SslPolicyErrors sslPolicyErrors)
        //                        {
        //                            return true; // **** Always accept
        //                        };
        //                    System.Net.ServicePointManager.Expect100Continue = false;
        //                    //▲ Add - TuanNA89 - 08/11/2019 - Fix lỗi mất kết nối API TPBank
        //                    var l__HttpWebReq = (HttpWebRequest)WebRequest.Create(l__ReqUriStr);
        //                    l__HttpWebReq.KeepAlive = false;
        //                    l__HttpWebReq.Method = "POST";

        //                    l__HttpWebReq.ContentType = "application/json";
        //                    l__HttpWebReq.Headers.Add("Authorization", l_Authorization);
        //                    l__Step = "đang chạy";
        //                    using (Stream stm = l__HttpWebReq.GetRequestStream())
        //                    {
        //                        using (StreamWriter stmw = new StreamWriter(stm))
        //                        {
        //                            stmw.Write(l__encryptData);
        //                        }
        //                    }

        //                    HttpStatusCode? StatusCode = null;
        //                    l__Step = "Lấy chuỗi response";
        //                    try
        //                    {
        //                        l__Step = "Bắt đầu chạy API";
        //                        l__oResp = (HttpWebResponse)l__HttpWebReq.GetResponse();
        //                    }
        //                    catch (WebException we)
        //                    {
        //                        l__Step = "API lỗi";
        //                        l__oResp = (HttpWebResponse)we.Response;
        //                    }
        //                    l__Step = "Lấy chuỗi Stream của API";
        //                    var webResponseStream = l__oResp.GetResponseStream();
        //                    l__Step = "Check chuỗi ResponseStream";
        //                    if (webResponseStream != null && webResponseStream != Stream.Null)
        //                    {
        //                        l__Step = "Chuỗi ResponseStream không null";
        //                        l__Step = "Lấy StatusCode";
        //                        StatusCode = l__oResp.StatusCode;
        //                    }
        //                    int? statusCode = null;
        //                    string statusName = null;
        //                    l__Step = "Check StatusCode";
        //                    if (StatusCode != null)
        //                    {
        //                        l__Step = "StatusCode không null";
        //                        statusCode = (int)StatusCode;
        //                        statusName = StatusCode.ToString();
        //                    }

        //                    if ((StatusCode == HttpStatusCode.Created) || (StatusCode == HttpStatusCode.OK))
        //                    {
        //                        l__Step = "Thành công";
        //                        l__result = new TPFico_Customer_Result()
        //                        {
        //                            status = "1",
        //                            messages = "Gửi thông tin giải ngân sang TPBank thành công!"
        //                        };

        //                        g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_TPFico_PushResult", CommandType.StoredProcedure, new SqlParameter[]{
        //                            new SqlParameter("@Result", 8),
        //                            new SqlParameter("@Msg", "Gửi thông tin giải ngân thành công!"),
        //                            new SqlParameter("@CustId",  l_DataTable.Rows[0]["CustId"].ToString()),
        //                        });
        //                    }
        //                    else
        //                    {
        //                        l__Step = "Lấy chuỗi response lỗi";
        //                        StreamReader l__StreamReader = new StreamReader(l__oResp.GetResponseStream());

        //                        var l_data = l__StreamReader.ReadToEnd();
        //                        l__Step = "Giải mã";
        //                        var l__Json_Decrypt = PGPEncrypt_SignAndDecrypt(l_data);
        //                        l__Step = "Gán object";
        //                        var l__Data = new JavaScriptSerializer().Deserialize<TPFico_Customer_Result>(l__Json_Decrypt);
        //                        WriteLog(0, 0, "InstallmentTPBank_TPFico_SendDocumentInfo", string.Format("{0} - {1}", statusCode.ToString(), statusName), l__Json_Decrypt);
        //                        l__result = new TPFico_Customer_Result()
        //                        {
        //                            status = "0",
        //                            //Edit - TuanNA89 - 31/07/2019 - Thêm Note nơi trả về lỗi
        //                            messages = "Lỗi TPBank trả về: " + l__Data.messages
        //                        };

        //                        //Add - TuanNA89 - 31/07/2019 - Check thêm trường hợp TPBank trả về lỗi "Lỗi gọi dịch vụ web" thì vẫn tính là thành công
        //                        if (l__result.messages.Contains("Lỗi gọi dịch vụ web cust id"))
        //                        {
        //                            l__Step = "Thành công";
        //                            l__result = new TPFico_Customer_Result()
        //                            {
        //                                status = "1",
        //                                messages = "Gửi thông tin giải ngân sang TPBank thành công!"
        //                            };
        //                            Status status = Status.PROCESSING;
        //                            statusCode = (int)status;

        //                            g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_TPFico_PushResult", CommandType.StoredProcedure, new SqlParameter[]{
        //                            new SqlParameter("@Result", 8),
        //                            new SqlParameter("@Msg", "Gửi thông tin giải ngân thành công!"),
        //                            new SqlParameter("@CustId",  l_DataTable.Rows[0]["CustId"].ToString()),
        //                            });
        //                        }
        //                        //Add - TuanNA89 - 31/07/2019 - Check thêm trường hợp TPBank trả về lỗi "Lỗi gọi dịch vụ web" thì vẫn tính là thành công
        //                    }
        //                    #endregion
        //                }
        //                else
        //                {
        //                    l__result = new TPFico_Customer_Result()
        //                    {
        //                        status = "0",
        //                        messages = "Xảy ra lỗi khi gộp hình"
        //                    };
        //                    WriteLog(0, 0, "InstallmentTPBank_TPFico_SendDocumentInfo", "", "Xảy ra lỗi khi gộp hình");
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string l__error = "- Bước bị lỗi: " + l__Step + "- Lỗi: " + ex.ToString();
        //        Logger.WriteLogError("RegistrationForm - " + MethodBase.GetCurrentMethod().Name + "()", l__error);
        //        string msgError = "";
        //        if (ex.ToString().Contains("Unable to connect to the remote server"))
        //        {
        //            msgError = "Không kết nối được tới TPBank";
        //        }
        //        else
        //        {
        //            msgError = ex.ToString();
        //        }

        //        l__result = new TPFico_Customer_Result()
        //        {
        //            status = "0",
        //            messages = msgError
        //        };
        //        WriteLog(0, 0, "InstallmentTPBank_TPFico_SendDocumentInfo", "", l__error);
        //    }
        //    finally
        //    {
        //        if (l__oResp != null)
        //        {
        //            l__oResp.Close();
        //        }
        //    }
        //    return l__result;
        //}
        //public string InstallmentTPBank_TPFico_GetCustInfo(string p_IdFinal)
        //{
        //    string l__Step = "";
        //    string xml = "";
        //    HttpWebResponse l__oResp = null;
        //    try
        //    {
        //        string p_Token = InstallmentTPBank_TPFico_GetAuthorization();
        //        string l_data = "";

        //        #region == Get data from TPFico ==

        //        string l__ReqUriStr = ConfigurationManager.AppSettings["TPFico_Url"].ToString() + String.Format(Api_TPBank.LayThongTinDangKyKH, p_IdFinal);//Customer id
        //        //▼ Add - TuanNA89 - 08/11/2019 - Fix lỗi mất kết nối API TPBank
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        //        System.Net.ServicePointManager.ServerCertificateValidationCallback +=
        //            delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
        //                                    System.Security.Cryptography.X509Certificates.X509Chain chain,
        //                                    System.Net.Security.SslPolicyErrors sslPolicyErrors)
        //            {
        //                return true; // **** Always accept
        //            };
        //        System.Net.ServicePointManager.Expect100Continue = false;
        //        //▲ Add - TuanNA89 - 08/11/2019 - Fix lỗi mất kết nối API TPBank

        //        var l__HttpWebReq = (HttpWebRequest)WebRequest.Create(l__ReqUriStr);
        //        l__HttpWebReq.KeepAlive = false;
        //        l__HttpWebReq.Method = "GET";

        //        l__HttpWebReq.Headers.Add("Authorization", p_Token);

        //        HttpStatusCode? StatusCode = null;
        //        l__Step = "Lấy chuỗi response";
        //        try
        //        {
        //            l__Step = "Bắt đầu chạy API";
        //            l__oResp = (HttpWebResponse)l__HttpWebReq.GetResponse();
        //        }
        //        catch (WebException we)
        //        {
        //            l__Step = "API lỗi";
        //            l__oResp = (HttpWebResponse)we.Response;
        //        }
        //        l__Step = "Lấy chuỗi Stream của API";
        //        var webResponseStream = l__oResp.GetResponseStream();
        //        l__Step = "Check chuỗi ResponseStream";
        //        if (webResponseStream != null && webResponseStream != Stream.Null)
        //        {
        //            l__Step = "Chuỗi ResponseStream không null";
        //            l__Step = "Lấy StatusCode";
        //            StatusCode = l__oResp.StatusCode;
        //        }
        //        int? statusCode = null;
        //        string statusName = null;
        //        l__Step = "Check StatusCode";
        //        if (StatusCode != null)
        //        {
        //            l__Step = "StatusCode không null";
        //            statusCode = (int)StatusCode;
        //            statusName = StatusCode.ToString();
        //        }
        //        if (StatusCode == HttpStatusCode.OK)
        //        {
        //            StreamReader l__StreamReader = new StreamReader(l__oResp.GetResponseStream());

        //            l_data = l__StreamReader.ReadToEnd();
        //            var l_dataDecrypt = "";
        //            l_dataDecrypt = PGPEncrypt_SignAndDecrypt(l_data);
        //            var l_custInfo = new TPFico_CustInfo_GetAllData();
        //            l_custInfo = new JavaScriptSerializer().Deserialize<TPFico_CustInfo_GetAllData>(l_dataDecrypt);

        //            using (var stringwriter = new System.IO.StringWriter())
        //            {
        //                var serializer = new XmlSerializer(l_custInfo.GetType());
        //                serializer.Serialize(stringwriter, l_custInfo);
        //                xml = stringwriter.ToString();
        //            };
        //        }
        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        string l__error = "- Bước bị lỗi: " + l__Step + "- Lỗi: " + ex.InnerException;
        //        Logger.WriteLogError("RegistrationForm - " + MethodBase.GetCurrentMethod().Name + "()", l__error);
        //        WriteLog(0, 0, MethodBase.GetCurrentMethod().Name, "", l__error);
        //    }
        //    finally
        //    {
        //        if (l__oResp != null)
        //        {
        //            l__oResp.Close();
        //        }
        //    }
        //    return xml;
        //}
        //public TPFico_Customer_Result InstallmentTPBank_TPFico_TPFico_SendDataUpdate(string p_IdFinal, string p_Screen)
        //{
        //    int l__IdLog = 0;
        //    TPFico_Customer_Result l__result = new TPFico_Customer_Result();
        //    l__IdLog = WriteLog(0, 0, "InstallmentTPBank_TPFico_TPFico_SendDataUpdate", p_IdFinal, "");
        //    string l__Step = "";
        //    HttpWebResponse l__oResp = null;
        //    try
        //    {
        //        SqlParameter[] l_SqlParameter = new SqlParameter[]{
        //            new SqlParameter("@CustId", p_IdFinal)
        //            ,new SqlParameter("@Screen", p_Screen)
        //        };
        //        l__Step = "Gọi store";
        //        DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_TPFico_GetDataUpdate", CommandType.StoredProcedure, l_SqlParameter);
        //        if (l_DataTable != null && l_DataTable.Rows.Count > 0)
        //        {
        //            l__Step = "Lấy token";
        //            string l_Key = InstallmentTPBank_TPFico_GetAuthorization();

        //            if (l_Key != null && l_Key != "")
        //            {
        //                #region == (Đối với lỗi liên quan đến up hình - Gộp hình) ==
        //                List<string> l__FileCMND = new List<string> { };
        //                List<string> l__FileSHK = new List<string> { };
        //                List<string> l__FileACCA = new List<string> { };

        //                foreach (DataRow dr in l_DataTable.Rows)
        //                {
        //                    if (dr["Id_TagHtml"].ToString() == "CMND")
        //                    {
        //                        l__FileCMND.Add(dr["NewValue"].ToString());
        //                    }
        //                    else if (dr["Id_TagHtml"].ToString() == "SHK")
        //                    {
        //                        l__FileSHK.Add(dr["NewValue"].ToString());
        //                    }
        //                    else if (dr["Id_TagHtml"].ToString() == "ACCA")
        //                    {
        //                        l__FileACCA.Add(dr["NewValue"].ToString());
        //                    }
        //                }

        //                string l__Url__NewImage_CMND = "";
        //                string l__Url__NewImage_SHK = "";
        //                string l__Url__NewImage_ACCA = "";
        //                string date = DateTime.Now.ToString("yyyyMMdd");
        //                string l_error_GopHinh = "";
        //                l__Step = "Gộp hình CMND";
        //                if (l__FileCMND.Count > 0)
        //                {
        //                    l__Url__NewImage_CMND = MergeImages(l__FileCMND, "/TPBank/" + date + "/", "CMND");
        //                    if (l__Url__NewImage_CMND == "")
        //                    {
        //                        l_error_GopHinh = l_error_GopHinh + "- Lỗi gộp hình CMND";
        //                    }
        //                }

        //                l__Step = "Gộp hình SHK";
        //                if (l__FileSHK.Count > 0)
        //                {
        //                    l__Url__NewImage_SHK = MergeImages(l__FileSHK, "/TPBank/" + date + "/", "SHK");
        //                    if (l__Url__NewImage_SHK == "")
        //                    {
        //                        l_error_GopHinh = l_error_GopHinh + "- Lỗi gộp hình SHK";
        //                    }
        //                }

        //                l__Step = "Gộp hình ACCA";
        //                if (l__FileACCA.Count > 0)
        //                {
        //                    l__Url__NewImage_ACCA = MergeImages(l__FileACCA, "/TPBank/" + date + "/", "ACCA");
        //                    if (l__Url__NewImage_ACCA == "")
        //                    {
        //                        l_error_GopHinh = l_error_GopHinh + "- Lỗi gộp hình ACCA";
        //                    }
        //                }
        //                #endregion

        //                if (l_error_GopHinh == "")
        //                {
        //                    #region == Add data ==
        //                    List<TPFico_FieldPushReturn> l_ListDataUpdate = new List<TPFico_FieldPushReturn>() { };
        //                    TPFico_FieldPushReturn l_ItemDataUpdate = new TPFico_FieldPushReturn { };
        //                    l__Step = "Add data";
        //                    foreach (DataRow dr in l_DataTable.Rows)
        //                    {
        //                        //▼	Edit - TuanNA89 - 08/07/2019 - Thêm Path vào Url hình ảnh
        //                        if (dr["Id_TagHtml"].ToString() == "CMND")
        //                        {
        //                            l_ItemDataUpdate = new TPFico_FieldPushReturn()
        //                            {
        //                                code = dr["SubCode"].ToString(),
        //                                comment = (l__Url__NewImage_CMND == "") ? "" : authority + l_DataTable.Rows[0]["ImagePath"].ToString() + l__Url__NewImage_CMND
        //                            };
        //                        }
        //                        else if (dr["Id_TagHtml"].ToString() == "SHK")
        //                        {
        //                            l_ItemDataUpdate = new TPFico_FieldPushReturn()
        //                            {
        //                                code = dr["SubCode"].ToString(),
        //                                comment = (l__Url__NewImage_SHK == "") ? "" : authority + l_DataTable.Rows[0]["ImagePath"].ToString() + l__Url__NewImage_SHK
        //                            };
        //                        }
        //                        else if (dr["Id_TagHtml"].ToString() == "ACCA")
        //                        {
        //                            l_ItemDataUpdate = new TPFico_FieldPushReturn()
        //                            {
        //                                code = dr["SubCode"].ToString(),
        //                                comment = (l__Url__NewImage_ACCA == "") ? "" : authority + l_DataTable.Rows[0]["ImagePath"].ToString() + l__Url__NewImage_ACCA
        //                            };
        //                        }
        //                        else
        //                        {
        //                            l_ItemDataUpdate = new TPFico_FieldPushReturn()
        //                            {
        //                                code = dr["SubCode"].ToString(),
        //                                comment = dr["NewValue"].ToString()
        //                            };
        //                        }
        //                        //▲	Edit - TuanNA89 - 08/07/2019 - Thêm Path vào Url hình ảnh
        //                        if (dr["Rank"].ToString() == "1")
        //                        {
        //                            l_ListDataUpdate.Add(l_ItemDataUpdate);
        //                        }
        //                    }

        //                    #endregion

        //                    #region == Push data ==
        //                    var l__jsData = new JavaScriptSerializer().Serialize(l_ListDataUpdate);
        //                    WriteLog(0, 0, "InstallmentTPBank_TPFico_TPFico_SendDataUpdate", l__jsData, "");
        //                    l__Step = "Mã hoá";
        //                    var l__encryptData = PGPEncrypt_SignAndEncrypt(l__jsData);
        //                    string l__ReqUriStr = "";
        //                    if (p_Screen == "TPB_DangKy")
        //                    {
        //                        l__ReqUriStr = ConfigurationManager.AppSettings["TPFico_Url"].ToString() + String.Format(Api_TPBank.GuiThongTinKH_BiLoi, l_DataTable.Rows[0]["CustId"].ToString());//Customer id
        //                    }
        //                    else if (p_Screen == "TPB_GiaiNgan")
        //                    {
        //                        l__ReqUriStr = ConfigurationManager.AppSettings["TPFico_Url"].ToString() + String.Format(Api_TPBank.GuiThongTinGiaiNgan_BiLoi, l_DataTable.Rows[0]["CustId"].ToString());//Customer id
        //                    }

        //                    //▼ Add - TuanNA89 - 08/11/2019 - Fix lỗi mất kết nối API TPBank
        //                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        //                    System.Net.ServicePointManager.ServerCertificateValidationCallback +=
        //                        delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
        //                                                System.Security.Cryptography.X509Certificates.X509Chain chain,
        //                                                System.Net.Security.SslPolicyErrors sslPolicyErrors)
        //                        {
        //                            return true; // **** Always accept
        //                        };
        //                    System.Net.ServicePointManager.Expect100Continue = false;
        //                    //▲ Add - TuanNA89 - 08/11/2019 - Fix lỗi mất kết nối API TPBank
        //                    var l__HttpWebReq = (HttpWebRequest)WebRequest.Create(l__ReqUriStr);
        //                    l__HttpWebReq.KeepAlive = false;
        //                    l__HttpWebReq.Method = "PUT";


        //                    l__HttpWebReq.ContentType = "application/json";
        //                    l__HttpWebReq.Headers.Add("Authorization", l_Key);

        //                    using (Stream stm = l__HttpWebReq.GetRequestStream())
        //                    {
        //                        using (StreamWriter stmw = new StreamWriter(stm))
        //                        {
        //                            stmw.Write(l__encryptData);
        //                        }
        //                    }

        //                    HttpStatusCode? StatusCode = null;
        //                    l__Step = "Lấy chuỗi response";
        //                    try
        //                    {
        //                        l__Step = "Bắt đầu chạy API";
        //                        l__oResp = (HttpWebResponse)l__HttpWebReq.GetResponse();
        //                    }
        //                    catch (WebException we)
        //                    {
        //                        l__Step = "API lỗi";
        //                        l__oResp = (HttpWebResponse)we.Response;
        //                    }
        //                    l__Step = "Lấy chuỗi Stream của API";
        //                    var webResponseStream = l__oResp.GetResponseStream();
        //                    l__Step = "Check chuỗi ResponseStream";
        //                    if (webResponseStream != null && webResponseStream != Stream.Null)
        //                    {
        //                        l__Step = "Chuỗi ResponseStream không null";
        //                        l__Step = "Lấy StatusCode";
        //                        StatusCode = l__oResp.StatusCode;
        //                    }
        //                    int? statusCode = null;
        //                    string statusName = null;
        //                    l__Step = "Check StatusCode";
        //                    if (StatusCode != null)
        //                    {
        //                        l__Step = "StatusCode không null";
        //                        statusCode = (int)StatusCode;
        //                        statusName = StatusCode.ToString();
        //                    }
        //                    if ((StatusCode == HttpStatusCode.Created) || (StatusCode == HttpStatusCode.OK))
        //                    {
        //                        int l__StatusChange = 0;
        //                        if (p_Screen == "TPB_DangKy")
        //                        {
        //                            l__StatusChange = 1; // Processing bước đăng ký
        //                        }
        //                        else if (p_Screen == "TPB_GiaiNgan")
        //                        {
        //                            l__StatusChange = 8; // Processing bước giải ngân
        //                        }
        //                        g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_TPFico_PushResult", CommandType.StoredProcedure, new SqlParameter[]{
        //                            new SqlParameter("@Result", l__StatusChange),
        //                            new SqlParameter("@Msg", "Gửi thông tin thành công!"),
        //                            new SqlParameter("@CustId", p_IdFinal),
        //                        });

        //                        l__result = new TPFico_Customer_Result()
        //                        {
        //                            status = "1",
        //                            messages = "Gửi thành công"
        //                        };
        //                    }
        //                    else
        //                    {
        //                        StreamReader l__StreamReader = new StreamReader(l__oResp.GetResponseStream());

        //                        var l__Response = l__StreamReader.ReadToEnd();
        //                        var l__Json_Decrypt = PGPEncrypt_SignAndDecrypt(l__Response);
        //                        WriteLog(0, 0, "InstallmentTPBank_TPFico_TPFico_SendDataUpdate", string.Format("{0} - {1}", statusCode.ToString(), statusName), l__Json_Decrypt);
        //                        l__result = new JavaScriptSerializer().Deserialize<TPFico_Customer_Result>(l__Json_Decrypt);
        //                        l__result.status = "0";
        //                        //Add - TuanNA89 - 31/07/2019 - Thêm Note nơi trả về lỗi
        //                        l__result.messages = "Lỗi TPBank trả về: " + l__result.messages;
        //                    }
        //                    #endregion
        //                }
        //                else
        //                {
        //                    l__result = new TPFico_Customer_Result()
        //                    {
        //                        status = "0",
        //                        messages = "Xảy ra lỗi khi gộp hình"
        //                    };

        //                    WriteLog(0, 0, "InstallmentTPBank_TPFico_TPFico_SendDataUpdate", "", l_error_GopHinh);
        //                }
        //            }
        //            else
        //            {
        //                l__result = new TPFico_Customer_Result()
        //                {
        //                    status = "0",
        //                    messages = "Không lấy được chuỗi kết nối TPBank"
        //                };
        //                WriteLog(0, 0, "InstallmentTPBank_TPFico_TPFico_SendDataUpdate", "", "Không lấy được chuỗi kết nối TPBank");
        //            }
        //        }
        //        else
        //        {
        //            l__result = new TPFico_Customer_Result()
        //            {
        //                status = "0",
        //                messages = "Không tìm thấy thông tin"
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string l__error = "- Bước bị lỗi: " + l__Step + "- Lỗi: " + ex.ToString();
        //        Logger.WriteLogError("RegistrationForm - " + MethodBase.GetCurrentMethod().Name + "()", l__error);
        //        string msgError = "";
        //        if (ex.ToString().Contains("Unable to connect to the remote server"))
        //        {
        //            msgError = "Không kết nối được tới TPBank";
        //        }
        //        else
        //        {
        //            msgError = ex.ToString();
        //        }

        //        l__result = new TPFico_Customer_Result()
        //        {
        //            status = "0",
        //            messages = msgError
        //        };
        //        WriteLog(0, 0, "InstallmentTPBank_TPFico_SendDocumentInfo", "", l__error);
        //    }
        //    finally
        //    {
        //        if (l__oResp != null)
        //        {
        //            l__oResp.Close();
        //        }
        //    }
        //    return l__result;
        //}
        //▲	Add - TuanNA89 - 21/10/2019 - Thêm code hỗ trợ việc gọi API TPBank
        #endregion
        #region === Code mới ===
        public string InstallmentTPBank_TPFico_TestConnection()
        {
            string l_Authorization = "";
            string l_Function = MethodBase.GetCurrentMethod().Name, l_Method = "GET", l_Url = "", l_jsData = "", l_Error = "";
            try
            {
                if (l_Authorization == "")
                {
                    l_Url = ConfigurationManager.AppSettings["TPFico_Url"].ToString() + Api_TPBank.LayToken;
                    var TPFico_ClientId = ConfigurationManager.AppSettings["TPFico_ClientId"].ToString();
                    var TPFico_ClientSecret = ConfigurationManager.AppSettings["TPFico_ClientSecret"].ToString();

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                        delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                                                System.Security.Cryptography.X509Certificates.X509Chain chain,
                                                System.Net.Security.SslPolicyErrors sslPolicyErrors)
                        {
                            return true; // **** Always accept
                        };
                    System.Net.ServicePointManager.Expect100Continue = false;
                    var l__HttpWebReq = (HttpWebRequest)WebRequest.Create(l_Url);
                    l__HttpWebReq.KeepAlive = false;
                    l__HttpWebReq.Method = l_Method;
                    l__HttpWebReq.ContentType = "application/x-www-form-urlencoded";

                    string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(TPFico_ClientId + ":" + TPFico_ClientSecret));
                    l__HttpWebReq.Headers.Add("Authorization", "Basic " + svcCredentials);
                    string urlEncode = "grant_type=" + HttpUtility.UrlEncode("client_credentials");
                    using (StreamWriter stOut = new StreamWriter(l__HttpWebReq.GetRequestStream(), System.Text.Encoding.ASCII))
                    {
                        stOut.Write(urlEncode);
                        stOut.Close();
                    }
                    using (HttpWebResponse l__oResp = l__HttpWebReq.GetResponse() as HttpWebResponse)
                    {
                        using (StreamReader l__StreamReader = new StreamReader(l__oResp.GetResponseStream()))
                        {
                            var l_ResponseFromServer = l__StreamReader.ReadToEnd();
                            if (l__oResp.StatusCode == HttpStatusCode.OK)
                            {
                                var result = JObject.Parse(l_ResponseFromServer);
                                l_Authorization = result["token_type"].ToString() + " " + result["access_token"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                l_Authorization = ex.ToString();
                Logger.WriteLogError("RegistrationForm - " + MethodBase.GetCurrentMethod().Name + "()", "Link API: " + l_Url + " - Error:" + ex.ToString());
                WriteLog_TPBank(l_Function, l_Method, l_Url, l_jsData, "", ex.ToString());
            }
            return l_Authorization;
        }
        public string InstallmentTPBank_TPFico_GetAuthorization_Beta()
        {
            /*
             * Rule:
                B1: gán dữ liệu từ biến token global -> token local
                B2: nếu token có data -> lấy (time hiện tại - time lấy token)
                        - nếu kết quả > time hết hạn -> set token rỗng
                B3: 
                    check nếu token rỗng -> gọi API lấy token -> set thời gian lấy token + thời gian hết hạn
             */
            string l_Authorization = "";
            string l_Function = MethodBase.GetCurrentMethod().Name, l_Method = "GET", l_Url = "", l_jsData = "", l_Error = "";
            try
            {
                l_Authorization = g__Token__TPBank;

                if (l_Authorization != "")
                {
                    var seconds = (DateTime.Now - DateTime.Parse(g__TimeCreateToken_TPBank.ToString())).TotalSeconds;
                    if (seconds >= g__TimeExpires__Second)
                    {
                        l_Authorization = "";
                    }
                }

                if (l_Authorization == "")
                {
                    //l_Url = ConfigurationManager.AppSettings["TPFico_Url"].ToString() + Api_TPBank.LayToken;
                    var TPFico_ClientId = ConfigurationManager.AppSettings["TPFico_ClientId"].ToString();
                    var TPFico_ClientSecret = ConfigurationManager.AppSettings["TPFico_ClientSecret"].ToString();

                    string p_Api = Api_TPBank.LayToken + String.Format("?grant_type=client_credentials&client_id={0}&client_secret={1}", TPFico_ClientId, TPFico_ClientSecret);

                    var resultAPI = CallApiTPBank(l_Function, l_Method, p_Api, "", "", "");

                    var result = JObject.Parse((JObject.Parse(resultAPI))["dataResponse"].ToString());

                    l_Authorization = result["token_type"].ToString() + " " + result["access_token"].ToString();

                    g__TimeExpires__Second = int.TryParse(result["expires_in"].ToString(), out g__TimeExpires__Second) ? g__TimeExpires__Second : 0;
                    g__TimeCreateToken_TPBank = DateTime.Now;

                    if (l_Authorization != g__Token__TPBank)
                    {
                        g__Token__TPBank = l_Authorization;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogError("RegistrationForm - " + MethodBase.GetCurrentMethod().Name + "()", "Link API: " + l_Url + " - Error:" + ex.ToString());
                WriteLog_TPBank(l_Function, l_Method, l_Url, l_jsData, "", ex.ToString());
            }
            return l_Authorization;
        }
        public string InstallmentTPBank_TPFico_GetAuthorization()
        {
            /*
             * Rule:
                B1: gán dữ liệu từ biến token global -> token local
                B2: nếu token có data -> lấy (time hiện tại - time lấy token)
                        - nếu kết quả > time hết hạn -> set token rỗng
                B3: 
                    check nếu token rỗng -> gọi API lấy token -> set thời gian lấy token + thời gian hết hạn
             */
            string l_Authorization = "";
            string l_Function = MethodBase.GetCurrentMethod().Name, l_Method = "POST", l_Url = "", l_jsData = "", l_Error = "";
            try
            {
                l_Authorization = g__Token__TPBank;

                if (l_Authorization != "")
                {
                    var seconds = (DateTime.Now - DateTime.Parse(g__TimeCreateToken_TPBank.ToString())).TotalSeconds;
                    if (seconds >= g__TimeExpires__Second)
                    {
                        l_Authorization = "";
                    }
                }

                if (l_Authorization == "")
                {
                    l_Url = ConfigurationManager.AppSettings["TPFico_Url"].ToString() + Api_TPBank.LayToken;
                    var TPFico_ClientId = ConfigurationManager.AppSettings["TPFico_ClientId"].ToString();
                    var TPFico_ClientSecret = ConfigurationManager.AppSettings["TPFico_ClientSecret"].ToString();

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                        delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                                                System.Security.Cryptography.X509Certificates.X509Chain chain,
                                                System.Net.Security.SslPolicyErrors sslPolicyErrors)
                        {
                            return true; // **** Always accept
                        };
                    System.Net.ServicePointManager.Expect100Continue = false;
                    var l__HttpWebReq = (HttpWebRequest)WebRequest.Create(l_Url);
                    l__HttpWebReq.KeepAlive = false;
                    l__HttpWebReq.Method = l_Method;
                    l__HttpWebReq.ContentType = "application/x-www-form-urlencoded";

                    string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(TPFico_ClientId + ":" + TPFico_ClientSecret));
                    l__HttpWebReq.Headers.Add("Authorization", "Basic " + svcCredentials);
                    string urlEncode = "grant_type=" + HttpUtility.UrlEncode("client_credentials");
                    using (StreamWriter stOut = new StreamWriter(l__HttpWebReq.GetRequestStream(), System.Text.Encoding.ASCII))
                    {
                        stOut.Write(urlEncode);
                        stOut.Close();
                    }
                    using (HttpWebResponse l__oResp = l__HttpWebReq.GetResponse() as HttpWebResponse)
                    {
                        using (StreamReader l__StreamReader = new StreamReader(l__oResp.GetResponseStream()))
                        {
                            var l_ResponseFromServer = l__StreamReader.ReadToEnd();
                            if (l__oResp.StatusCode == HttpStatusCode.OK)
                            {
                                var result = JObject.Parse(l_ResponseFromServer);
                                l_Authorization = result["token_type"].ToString() + " " + result["access_token"].ToString();

                                g__TimeExpires__Second = int.TryParse(result["expires_in"].ToString(), out g__TimeExpires__Second) ? g__TimeExpires__Second : 0;
                                g__TimeCreateToken_TPBank = DateTime.Now;

                                if (l_Authorization != g__Token__TPBank)
                                {
                                    g__Token__TPBank = l_Authorization;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogError("RegistrationForm - " + MethodBase.GetCurrentMethod().Name + "()", "Link API: " + l_Url + " - Error:" + ex.ToString());
                WriteLog_TPBank(l_Function, l_Method, l_Url, l_jsData, "", ex.ToString());
            }
            return l_Authorization;
        }
        public string InstallmentTPBank_TPFico_SendCustInfo(string p_IdFinal)
        {
            TPFico_Customer_Result l__result = new TPFico_Customer_Result();
            string l__Step = "";
            string l_Function = MethodBase.GetCurrentMethod().Name, l_Method = "POST", l_Url = "", l_jsData = "", l_Error = "";
            try
            {
                l__Step = "Lấy token";
                string l_Key = InstallmentTPBank_TPFico_GetAuthorization();

                if (!string.IsNullOrEmpty(l_Key.Trim()))
                {
                    SqlParameter[] l_SqlParameter = new SqlParameter[]{
                        new SqlParameter("@Id_Final", p_IdFinal)
                    };
                    l__Step = "Gọi store";
                    DataSet l_DataSet = g_SqlDBHelper.ExecuteCommandDataSet("sp_InstallmentTPBank_CustInfo_ForAPI", CommandType.StoredProcedure, l_SqlParameter);
                    if (l_DataSet != null && l_DataSet.Tables.Count > 0)
                    {
                        DataTable l_CustInfo = l_DataSet.Tables[0];
                        DataTable l_ProdDetails = l_DataSet.Tables[1];
                        DataTable l_Ref_Address = l_DataSet.Tables[3];

                        l__Step = "Add Images";
                        var l__ListPhotos = new List<TPFico_CustInfo_Photo>();
                        List<string> l__Files = new List<string> { };
                        string l__Url__NewImage__CMND = "", l__Url__NewImage__SHK = "";
                        string date = DateTime.Now.ToString("yyyyMMdd");

                        #region == Gộp CMND ==
                        l__Files.Add(l_CustInfo.Rows[0]["Url_CRD_MT_CMND"].ToString());
                        l__Files.Add(l_CustInfo.Rows[0]["Url_CRD_MS_CMND"].ToString());
                        l__Url__NewImage__CMND = MergeImages(l__Files, "/TPBank/" + date + "/", "CMND");
                        #endregion

                        #region == Gộp Sổ Hộ Khẩu ==
                        l__Files.Clear();
                        if ((l_CustInfo.Rows[0]["Url_CRD_SHK_1"].ToString() != null) && (l_CustInfo.Rows[0]["Url_CRD_SHK_1"].ToString() != ""))
                            l__Files.Add(l_CustInfo.Rows[0]["Url_CRD_SHK_1"].ToString());

                        if ((l_CustInfo.Rows[0]["Url_CRD_SHK_2"].ToString() != null) && (l_CustInfo.Rows[0]["Url_CRD_SHK_2"].ToString() != ""))
                            l__Files.Add(l_CustInfo.Rows[0]["Url_CRD_SHK_2"].ToString());

                        if ((l_CustInfo.Rows[0]["Url_CRD_SHK_3"].ToString() != null) && (l_CustInfo.Rows[0]["Url_CRD_SHK_3"].ToString() != ""))
                            l__Files.Add(l_CustInfo.Rows[0]["Url_CRD_SHK_3"].ToString());

                        if ((l_CustInfo.Rows[0]["Url_CRD_SHK_4"].ToString() != null) && (l_CustInfo.Rows[0]["Url_CRD_SHK_4"].ToString() != ""))
                            l__Files.Add(l_CustInfo.Rows[0]["Url_CRD_SHK_4"].ToString());

                        if ((l_CustInfo.Rows[0]["Url_CRD_SHK_5"].ToString() != null) && (l_CustInfo.Rows[0]["Url_CRD_SHK_5"].ToString() != ""))
                            l__Files.Add(l_CustInfo.Rows[0]["Url_CRD_SHK_5"].ToString());

                        if ((l_CustInfo.Rows[0]["Url_CRD_SHK_6"].ToString() != null) && (l_CustInfo.Rows[0]["Url_CRD_SHK_6"].ToString() != ""))
                            l__Files.Add(l_CustInfo.Rows[0]["Url_CRD_SHK_6"].ToString());

                        if ((l_CustInfo.Rows[0]["Url_CRD_SHK_7"].ToString() != null) && (l_CustInfo.Rows[0]["Url_CRD_SHK_7"].ToString() != ""))
                            l__Files.Add(l_CustInfo.Rows[0]["Url_CRD_SHK_7"].ToString());

                        if (l__Files.Count > 0)
                        {
                            l__Url__NewImage__SHK = MergeImages(l__Files, "/TPBank/" + date + "/", "SHK");
                        }
                        #endregion

                        if (l__Url__NewImage__CMND == "" || ((l__Files.Count > 0) && (l__Url__NewImage__SHK == "")))
                        {
                            var strHinhLoi = "";
                            if (l__Url__NewImage__CMND == "") strHinhLoi = "CMND";
                            else if (l__Url__NewImage__SHK == "") strHinhLoi = "Sổ hộ khẩu";
                            l__result = new TPFico_Customer_Result()
                            {
                                status = "0",
                                messages = "Lỗi gộp hình " + strHinhLoi
                            };
                        }
                        else
                        {
                            #region == Add Url Images ==
                            l__ListPhotos.Add(new TPFico_CustInfo_Photo
                            {
                                link = (l_CustInfo.Rows[0]["Url_CRD_CD_DKMoThe"].ToString() == "") ? "" : (authority + l_CustInfo.Rows[0]["ImagePath"].ToString() + l_CustInfo.Rows[0]["Url_CRD_CD_DKMoThe"].ToString()),
                                documentType = "selfie"
                            });

                            l__ListPhotos.Add(new TPFico_CustInfo_Photo
                            {
                                link = (l_CustInfo.Rows[0]["Url_CRD_KH_CMND"].ToString() == "") ? "" : (authority + l_CustInfo.Rows[0]["ImagePath"].ToString() + l_CustInfo.Rows[0]["Url_CRD_KH_CMND"].ToString()),
                                documentType = "employeecard"
                            });
                            if (l__Url__NewImage__SHK != "")
                            {
                                l__ListPhotos.Add(new TPFico_CustInfo_Photo
                                {
                                    link = authority + l_CustInfo.Rows[0]["ImagePath"].ToString() + l__Url__NewImage__SHK,
                                    documentType = "fb"
                                });
                            }
                            if (l__Url__NewImage__CMND != "")
                            {
                                l__ListPhotos.Add(new TPFico_CustInfo_Photo
                                {
                                    link = (l__Url__NewImage__CMND == "") ? "" : authority + l_CustInfo.Rows[0]["ImagePath"].ToString() + l__Url__NewImage__CMND,
                                    documentType = "National_ID"
                                });
                            }
                            #endregion
                            l__Step = "Add detail product";
                            #region == Add Detail Products ==
                            List<TPFico_CustInfo_ProductDetail> l_ListProducts = new List<TPFico_CustInfo_ProductDetail> { };
                            if (l_ProdDetails != null && l_ProdDetails.Rows.Count > 0)
                            {
                                foreach (DataRow item in l_ProdDetails.Rows)
                                {
                                    l_ListProducts.Add(new TPFico_CustInfo_ProductDetail
                                    {
                                        model = item["TenSanPham"].ToString(),//Model of the goods
                                        goodCode = item["MaSanPham"].ToString(),//Goods code of dealer
                                        goodType = item["LoaiHang"].ToString(),//Type of goods: Non portable/portable
                                        quantity = item["SoLuong"].ToString(),//Quantity
                                        goodPrice = item["GiaSanPham"].ToString()//Price of goods
                                    });

                                }
                            }
                            #endregion
                            l__Step = "Add Referenses";
                            #region == Add References ==
                            var l__ListReferences = new List<TPFico_CustInfo_Reference>();
                            if (l_CustInfo.Rows[0]["NguoiThan1_HoTen"].ToString() != "")
                            {
                                l__ListReferences.Add(new TPFico_CustInfo_Reference()
                                {
                                    fullName = l_CustInfo.Rows[0]["NguoiThan1_HoTen"].ToString(),//Reference person name
                                    phoneNumber = l_CustInfo.Rows[0]["NguoiThan1_SDT"].ToString(),//Reference person’ phone number
                                    relation = l_CustInfo.Rows[0]["NguoiThan1_QuanHe"].ToString(),//Relationship with owner. “husband, wife, workmate, relatives”
                                    personalId = l_CustInfo.Rows[0]["NguoiThan1_CMND"].ToString()//National ID of the reference                                    
                                });
                            }

                            if (l_CustInfo.Rows[0]["NguoiThan2_HoTen"].ToString() != "")
                            {
                                l__ListReferences.Add(new TPFico_CustInfo_Reference()
                                {
                                    fullName = l_CustInfo.Rows[0]["NguoiThan2_HoTen"].ToString(),//Reference person name
                                    phoneNumber = l_CustInfo.Rows[0]["NguoiThan2_SDT"].ToString(),//Reference person’ phone number
                                    relation = l_CustInfo.Rows[0]["NguoiThan2_QuanHe"].ToString(),//Relationship with owner. “husband, wife, workmate, relatives”
                                    personalId = l_CustInfo.Rows[0]["NguoiThan2_CMND"].ToString()//National ID of the reference                                    
                                });
                            }

                            if (l_CustInfo.Rows[0]["NguoiHonPhoi_HoTen"].ToString() != "")
                            {
                                l__ListReferences.Add(new TPFico_CustInfo_Reference()
                                {
                                    fullName = l_CustInfo.Rows[0]["NguoiHonPhoi_HoTen"].ToString(),//Reference person name
                                    phoneNumber = l_CustInfo.Rows[0]["NguoiHonPhoi_SDT"].ToString(),//Reference person’ phone number
                                    relation = l_CustInfo.Rows[0]["NguoiHonPhoi_QuanHe"].ToString(),
                                    personalId = l_CustInfo.Rows[0]["NguoiHonPhoi_CMND"].ToString()//National ID of the reference                                    
                                });
                            }
                            #endregion
                            l__Step = "Add Address";
                            #region == Add Address ==
                            var l__ListAddresses = new List<TPFico_CustInfo_Address>();
                            l__ListAddresses.Add(new TPFico_CustInfo_Address
                            {
                                addressType = "Current Address",//Current, family book
                                address1 = l_CustInfo.Rows[0]["TamTru_ToaNha"].ToString(),//Address number & Street
                                address2 = l_CustInfo.Rows[0]["TamTru_DiaChi"].ToString(),
                                ward = l_CustInfo.Rows[0]["TamTru_PhuongXa"].ToString(),//Ward
                                district = l_CustInfo.Rows[0]["TamTru_QuanHuyen"].ToString(),//District
                                province = l_CustInfo.Rows[0]["TamTru_TinhThanh"].ToString()//City
                            });
                            l__ListAddresses.Add(new TPFico_CustInfo_Address
                            {
                                addressType = "Family Book Address",//Current, family book
                                address1 = l_CustInfo.Rows[0]["HoKhau_ToaNha"].ToString(),
                                address2 = l_CustInfo.Rows[0]["HoKhau_DiaChi"].ToString(),
                                ward = l_CustInfo.Rows[0]["HoKhau_PhuongXa"].ToString(),//Ward
                                district = l_CustInfo.Rows[0]["HoKhau_QuanHuyen"].ToString(),//District
                                province = l_CustInfo.Rows[0]["HoKhau_TinhThanh"].ToString()//City
                            });

                            l__ListAddresses.Add(new TPFico_CustInfo_Address
                            {
                                addressType = "Working Address",//Current, family book
                                address1 = l_Ref_Address.Rows[0]["LamViec_ToaNha"].ToString(),
                                address2 = l_Ref_Address.Rows[0]["LamViec_DiaChi"].ToString(),
                                ward = l_Ref_Address.Rows[0]["LamViec_PhuongXa"].ToString(),//Ward
                                district = l_Ref_Address.Rows[0]["LamViec_QuanHuyen"].ToString(),//District
                                province = l_Ref_Address.Rows[0]["LamViec_TinhThanh"].ToString()//City
                            });

                            if (l_CustInfo.Rows[0]["NguoiHonPhoi_HoTen"].ToString() != "")
                            {
                                l__ListAddresses.Add(new TPFico_CustInfo_Address
                                {
                                    addressType = "Spouse Address",//Current, family book
                                    address1 = l_CustInfo.Rows[0]["HonPhoi_ToaNha"].ToString(),
                                    address2 = l_CustInfo.Rows[0]["HonPhoi_DiaChi"].ToString(),
                                    ward = l_CustInfo.Rows[0]["HonPhoi_PhuongXa"].ToString(),//Ward
                                    district = l_CustInfo.Rows[0]["HonPhoi_QuanHuyen"].ToString(),//District
                                    province = l_CustInfo.Rows[0]["HonPhoi_TinhThanh"].ToString()//City
                                });
                            }


                            #endregion
                            l__Step = "Add Order Infor";
                            #region == Add Data
                            var l_ObjectData = new TPFico_CustInfo()
                            {
                                custId = l_CustInfo.Rows[0]["CustId"].ToString(),//Loan id
                                lastName = l_CustInfo.Rows[0]["LastName"].ToString(),//Last name of Customer
                                firstName = l_CustInfo.Rows[0]["FirstName"].ToString(),//First name of Customer
                                middleName = l_CustInfo.Rows[0]["MidName"].ToString(),//Middle name of Customer
                                gender = l_CustInfo.Rows[0]["Gender"].ToString(),//Gender
                                dateOfBirth = l_CustInfo.Rows[0]["Birthday"].ToString(),//Date of Birth
                                nationalId = l_CustInfo.Rows[0]["CMND"].ToString(),//National ID of Customer
                                issueDate = l_CustInfo.Rows[0]["NgayCapCMND"].ToString(),//Issued Date of ID
                                issuePlace = l_CustInfo.Rows[0]["NoiCapCMND"].ToString(),
                                employeeCard = l_CustInfo.Rows[0]["MaNV_KH"].ToString(),//Employee card number
                                mobilePhone = l_CustInfo.Rows[0]["SDT"].ToString(),//Mobile Phone
                                durationYear = l_CustInfo.Rows[0]["ThoiGianCuTru_Nam"].ToString(),//Duration of living at current address (Year)
                                durationMonth = l_CustInfo.Rows[0]["ThoiGianCuTru_Thang"].ToString(),//Duration of living at current address (Month)
                                map = l_CustInfo.Rows[0]["DiaChi_HuongDan"].ToString(),//Guideline to the address
                                ownerNationalId = l_CustInfo.Rows[0]["CMND_ChuHo"].ToString(),//Owner National ID
                                contactAddress = l_CustInfo.Rows[0]["DiaChiLienHe_TypeName"].ToString(),//Contact Address
                                maritalStatus = l_CustInfo.Rows[0]["maritalStatus"].ToString(),
                                dsaCode = l_CustInfo.Rows[0]["MaNVFPT"].ToString(),//Code of Sale
                                companyName = l_CustInfo.Rows[0]["TenCongTyKHLamViec"].ToString(),
                                taxCode = l_CustInfo.Rows[0]["MSTCtyKH"].ToString(),
                                salary = l_CustInfo.Rows[0]["ThuNhapHangThang"].ToString(),// Add - TuanNA89 - 02/07/2019 - Thêm thu nhập hàng tháng
                                loanDetail = new TPFico_CustInfo_LoanDetail
                                {
                                    product = l_CustInfo.Rows[0]["SanPhamVay"].ToString(),//Product Applied
                                    loanAmount = l_CustInfo.Rows[0]["TienVay"].ToString(),//Loan amount requested
                                    downPayment = l_CustInfo.Rows[0]["TraTruoc"].ToString().Replace(",", "."),//Amount of down payment for total bill
                                    annualr = l_CustInfo.Rows[0]["LaiSuat"].ToString(),//Annual interest rate
                                    dueDate = l_CustInfo.Rows[0]["NgayThanhToanHangThang"].ToString(),
                                    tenor = l_CustInfo.Rows[0]["KyHan"].ToString(),//Tenor of the loan
                                },
                                addresses = l__ListAddresses,
                                photos = l__ListPhotos,
                                productDetails = l_ListProducts, //Product details
                                references = l__ListReferences
                            };
                            #endregion
                            l__Step = "Add xong data rồi";
                            #region == Push data ==
                            l_Url = Api_TPBank.GuiThongTinKH;
                            l_jsData = new JavaScriptSerializer().Serialize(l_ObjectData);
                            WriteLog_TPBank(l_Function, l_Method, l_Url, l_jsData, p_IdFinal, "Trước khi gọi API: " + l_jsData);
                            var resultAPI = CallApiTPBank(l_Function, l_Method, l_Url, l_Key, p_IdFinal, l_jsData);
                            WriteLog_TPBank(l_Function, l_Method, l_Url, l_jsData, p_IdFinal, "Sau khi gọi API: " + resultAPI);
                            var result = new JavaScriptSerializer().Deserialize<TPFico_Customer_Result>(resultAPI);
                            l__result = new TPFico_Customer_Result()
                            {
                                status = result.status,
                                messages = result.messages,
                                StatusCode = result.StatusCode
                            };
                            if (l__result.status == "1")
                            {
                                l__Step = "Thành công";
                                l__result = new TPFico_Customer_Result()
                                {
                                    status = "1",
                                    messages = "Gửi thông tin khách hàng sang TPBank thành công! Mã khách hàng TPBank: " + l_CustInfo.Rows[0]["CustId"].ToString()
                                };

                                g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_TPFico_PushResult", CommandType.StoredProcedure, new SqlParameter[]{
                                        new SqlParameter("@Result", (int)Status.PROCESSING),
                                        new SqlParameter("@Msg", "Gửi thông tin thành công!"),
                                        new SqlParameter("@CustId", p_IdFinal),
                                    });
                            }

                            WriteLog_TPBank(l_Function, l_Method, l_Url, l_jsData, p_IdFinal, new JavaScriptSerializer().Serialize(l__result));
                            #endregion
                        }
                    }
                    else
                    {
                        l__result = new TPFico_Customer_Result()
                        {
                            status = "0",
                            messages = "Không lấy được thông tin khách hàng"
                        };
                    }
                }
                else
                {
                    l__result = new TPFico_Customer_Result()
                    {
                        status = "0",
                        messages = "Không tìm thấy thông tin khách hàng"
                    };
                }
            }
            catch (Exception ex)
            {
                string l__error = "- Bước bị lỗi: " + l__Step + "- Lỗi: " + ex.ToString();
                Logger.WriteLogError("RegistrationForm - " + MethodBase.GetCurrentMethod().Name + "()", l__error);
                l__result = new TPFico_Customer_Result()
                {
                    status = "0",
                    messages = ex.ToString()
                };
                WriteLog_TPBank(l_Function, l_Method, l_Url, l_jsData, p_IdFinal, l__error);
            }
            //var json = Json(l__result, JsonRequestBehavior.AllowGet);
            //json.MaxJsonLength = int.MaxValue;
            return JsonConvert.SerializeObject(l__result);
        }
        public string InstallmentTPBank_TPFico_SendDocumentInfo(string p_IdFinal)
        {
            TPFico_Customer_Result l__result = new TPFico_Customer_Result();
            string l__Step = "";
            string l_Function = MethodBase.GetCurrentMethod().Name, l_Method = "POST", l_Url = "", l_jsData = "", l_Error = "";
            try
            {
                l__Step = "Lấy token";
                string l_Key = InstallmentTPBank_TPFico_GetAuthorization();
                if (!string.IsNullOrEmpty(l_Key.Trim()))
                {
                    SqlParameter[] l_SqlParameter = new SqlParameter[]{
                        new SqlParameter("@Id_Final", p_IdFinal)
                    };
                    l__Step = "Gọi store lấy data";
                    DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_CustInfo_ForAPI", CommandType.StoredProcedure, l_SqlParameter);
                    if (l_DataTable != null && l_DataTable.Rows.Count > 0)
                    {
                        #region == Gộp hình ==
                        List<string> l__Files = new List<string> { };
                        string l__Url__NewImage = "";
                        l__Step = "Add hình";
                        #region == Gộp hình đề nghị vay vốn ==                        
                        if ((l_DataTable.Rows[0]["Url_CRD_DXTNTD"].ToString() != null) && (l_DataTable.Rows[0]["Url_CRD_DXTNTD"].ToString() != ""))
                            l__Files.Add(l_DataTable.Rows[0]["Url_CRD_DXTNTD"].ToString());

                        if ((l_DataTable.Rows[0]["Url_CRD_DXTNTD_2"].ToString() != null) && (l_DataTable.Rows[0]["Url_CRD_DXTNTD_2"].ToString() != ""))
                            l__Files.Add(l_DataTable.Rows[0]["Url_CRD_DXTNTD_2"].ToString());

                        if ((l_DataTable.Rows[0]["Url_CRD_DXTNTD_3"].ToString() != null) && (l_DataTable.Rows[0]["Url_CRD_DXTNTD_3"].ToString() != ""))
                            l__Files.Add(l_DataTable.Rows[0]["Url_CRD_DXTNTD_3"].ToString());

                        if ((l_DataTable.Rows[0]["Url_CRD_DXTNTD_4"].ToString() != null) && (l_DataTable.Rows[0]["Url_CRD_DXTNTD_4"].ToString() != ""))
                            l__Files.Add(l_DataTable.Rows[0]["Url_CRD_DXTNTD_4"].ToString());

                        string date = DateTime.Now.ToString("yyyyMMdd");
                        l__Step = "Gộp hình";
                        if (l__Files.Count > 0)
                            l__Url__NewImage = MergeImages(l__Files, "/TPBank/" + date + "/", "ACCA");
                        #endregion
                        #endregion

                        if (l__Url__NewImage != "")
                        {
                            #region == Add Data ==
                            l__Step = "Gộp Images";
                            TPFico_DocumentInfo[] l_ObjectData =
                            {
                                new TPFico_DocumentInfo{
                                    file = (l__Url__NewImage == "" ) ? "" : authority + l_DataTable.Rows[0]["ImagePath"].ToString() + l__Url__NewImage,
                                    documentCode= "ACCA"
                                },
                                new TPFico_DocumentInfo{
                                    file= (l_DataTable.Rows[0]["Url_CRD_MoThe"].ToString() == "") ? "" : (authority + l_DataTable.Rows[0]["ImagePath"].ToString() + l_DataTable.Rows[0]["Url_CRD_MoThe"].ToString()),
                                    documentCode= "Signature"
                                },
                                new TPFico_DocumentInfo{
                                    file= (l_DataTable.Rows[0]["Url_CRD_GiayGiaoHang"].ToString() == "") ? "" : (authority + l_DataTable.Rows[0]["ImagePath"].ToString() + l_DataTable.Rows[0]["Url_CRD_GiayGiaoHang"].ToString()),
                                    documentCode= "Delivery note"
                                },
                            };
                            #endregion

                            l__Step = "Đẩy data giải ngân sang TPBank";
                            #region == Push data ==
                            l_Url = String.Format(Api_TPBank.GuiThongTinGiaiNgan, l_DataTable.Rows[0]["CustId"].ToString());
                            l_jsData = new JavaScriptSerializer().Serialize(l_ObjectData);
                            WriteLog_TPBank(l_Function, l_Method, l_Url, l_jsData, p_IdFinal, "Trước khi gọi API: " + l_jsData);
                            var resultAPI = CallApiTPBank(l_Function, l_Method, l_Url, l_Key, p_IdFinal, l_jsData);
                            WriteLog_TPBank(l_Function, l_Method, l_Url, l_jsData, p_IdFinal, "Sau khi gọi API: " + resultAPI);
                            var result = new JavaScriptSerializer().Deserialize<TPFico_Customer_Result>(resultAPI);
                            l__result = new TPFico_Customer_Result()
                            {
                                status = result.status,
                                messages = result.messages,
                                StatusCode = result.StatusCode
                            };
                            if (l__result.status == "1")
                            {
                                int l__StatusChange = (int)Status.WaitingDiburse;

                                g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_TPFico_PushResult", CommandType.StoredProcedure, new SqlParameter[]{
                                    new SqlParameter("@Result", l__StatusChange),
                                    new SqlParameter("@Msg", "Gửi thông tin thành công!"),
                                    new SqlParameter("@CustId", p_IdFinal),
                                });

                                l__result = new TPFico_Customer_Result()
                                {
                                    status = "1",
                                    messages = "Gửi thành công"
                                };
                            }
                            #endregion
                        }
                        else
                        {
                            l__result = new TPFico_Customer_Result()
                            {
                                status = "0",
                                messages = "Xảy ra lỗi khi gộp hình"
                            };
                        }
                    }
                }
                else
                {
                    l__result = new TPFico_Customer_Result()
                    {
                        status = "0",
                        messages = "Không lấy được chuỗi kết nối TPBank"
                    };
                }
            }
            catch (Exception ex)
            {
                string l__error = "- Bước bị lỗi: " + l__Step + "- Lỗi: " + ex.ToString();
                Logger.WriteLogError("RegistrationForm - " + MethodBase.GetCurrentMethod().Name + "()", l__error);
                string msgError = "";
                if (ex.ToString().Contains("Unable to connect to the remote server"))
                {
                    msgError = "Không kết nối được tới TPBank";
                }
                else
                {
                    msgError = ex.ToString();
                }

                l__result = new TPFico_Customer_Result()
                {
                    status = "0",
                    messages = msgError
                };
                WriteLog_TPBank(l_Function, l_Method, l_Url, l_jsData, p_IdFinal, l__error);
            }

            //var json = Json(l__result, JsonRequestBehavior.AllowGet);
            //json.MaxJsonLength = int.MaxValue;
            return JsonConvert.SerializeObject(l__result);
        }
        public string InstallmentTPBank_TPFico_TPFico_SendDataUpdate(string p_IdFinal, string p_Screen)
        {
            TPFico_Customer_Result l__result = new TPFico_Customer_Result();
            string l__Step = "";
            HttpWebResponse l__oResp = null;
            string l_Function = MethodBase.GetCurrentMethod().Name, l_Method = "PUT", l_Url = "", l_jsData = "", l_Error = "";
            try
            {
                string l_Key = InstallmentTPBank_TPFico_GetAuthorization();
                if (!string.IsNullOrEmpty(l_Key.Trim()))
                {
                    SqlParameter[] l_SqlParameter = new SqlParameter[]{
                        new SqlParameter("@CustId", p_IdFinal)
                        ,new SqlParameter("@Screen", p_Screen)
                    };
                    l__Step = "Gọi store";
                    DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_TPFico_GetDataUpdate", CommandType.StoredProcedure, l_SqlParameter);
                    if (l_DataTable != null && l_DataTable.Rows.Count > 0)
                    {
                        #region == (Đối với lỗi liên quan đến up hình - Gộp hình) ==
                        List<string> l__FileCMND = new List<string> { };
                        List<string> l__FileSHK = new List<string> { };
                        List<string> l__FileACCA = new List<string> { };

                        foreach (DataRow dr in l_DataTable.Rows)
                        {
                            if (dr["Id_TagHtml"].ToString() == "CMND")
                            {
                                l__FileCMND.Add(dr["NewValue"].ToString());
                            }
                            else if (dr["Id_TagHtml"].ToString() == "SHK")
                            {
                                l__FileSHK.Add(dr["NewValue"].ToString());
                            }
                            else if (dr["Id_TagHtml"].ToString() == "ACCA")
                            {
                                l__FileACCA.Add(dr["NewValue"].ToString());
                            }
                        }

                        string l__Url__NewImage_CMND = "";
                        string l__Url__NewImage_SHK = "";
                        string l__Url__NewImage_ACCA = "";
                        string date = DateTime.Now.ToString("yyyyMMdd");
                        string l_error_GopHinh = "";
                        l__Step = "Gộp hình CMND";
                        if (l__FileCMND.Count > 0)
                        {
                            l__Url__NewImage_CMND = MergeImages(l__FileCMND, "/TPBank/" + date + "/", "CMND");
                            if (l__Url__NewImage_CMND == "")
                            {
                                l_error_GopHinh = l_error_GopHinh + "- Lỗi gộp hình CMND";
                            }
                        }

                        l__Step = "Gộp hình SHK";
                        if (l__FileSHK.Count > 0)
                        {
                            l__Url__NewImage_SHK = MergeImages(l__FileSHK, "/TPBank/" + date + "/", "SHK");
                            if (l__Url__NewImage_SHK == "")
                            {
                                l_error_GopHinh = l_error_GopHinh + "- Lỗi gộp hình SHK";
                            }
                        }

                        l__Step = "Gộp hình ACCA";
                        if (l__FileACCA.Count > 0)
                        {
                            l__Url__NewImage_ACCA = MergeImages(l__FileACCA, "/TPBank/" + date + "/", "ACCA");
                            if (l__Url__NewImage_ACCA == "")
                            {
                                l_error_GopHinh = l_error_GopHinh + "- Lỗi gộp hình ACCA";
                            }
                        }
                        #endregion

                        if (l_error_GopHinh == "")
                        {
                            #region == Add data ==
                            List<TPFico_FieldPushReturn> l_ListDataUpdate = new List<TPFico_FieldPushReturn>() { };
                            TPFico_FieldPushReturn l_ItemDataUpdate = new TPFico_FieldPushReturn { };
                            l__Step = "Add data";
                            foreach (DataRow dr in l_DataTable.Rows)
                            {
                                //▼	Edit - TuanNA89 - 08/07/2019 - Thêm Path vào Url hình ảnh
                                if (dr["Id_TagHtml"].ToString() == "CMND")
                                {
                                    l_ItemDataUpdate = new TPFico_FieldPushReturn()
                                    {
                                        code = dr["SubCode"].ToString(),
                                        comment = (l__Url__NewImage_CMND == "") ? "" : authority + l_DataTable.Rows[0]["ImagePath"].ToString() + l__Url__NewImage_CMND
                                    };
                                }
                                else if (dr["Id_TagHtml"].ToString() == "SHK")
                                {
                                    l_ItemDataUpdate = new TPFico_FieldPushReturn()
                                    {
                                        code = dr["SubCode"].ToString(),
                                        comment = (l__Url__NewImage_SHK == "") ? "" : authority + l_DataTable.Rows[0]["ImagePath"].ToString() + l__Url__NewImage_SHK
                                    };
                                }
                                else if (dr["Id_TagHtml"].ToString() == "ACCA")
                                {
                                    l_ItemDataUpdate = new TPFico_FieldPushReturn()
                                    {
                                        code = dr["SubCode"].ToString(),
                                        comment = (l__Url__NewImage_ACCA == "") ? "" : authority + l_DataTable.Rows[0]["ImagePath"].ToString() + l__Url__NewImage_ACCA
                                    };
                                }
                                else
                                {
                                    l_ItemDataUpdate = new TPFico_FieldPushReturn()
                                    {
                                        code = dr["SubCode"].ToString(),
                                        comment = dr["NewValue"].ToString()
                                    };
                                }
                                //▲	Edit - TuanNA89 - 08/07/2019 - Thêm Path vào Url hình ảnh
                                if (dr["Rank"].ToString() == "1")
                                {
                                    l_ListDataUpdate.Add(l_ItemDataUpdate);
                                }
                            }

                            #endregion

                            l__Step = "Mã hoá";
                            var l__encryptData = PGPEncrypt_SignAndEncrypt(l_jsData);
                            string l__ReqUriStr = "";

                            #region == Push data ==
                            if (p_Screen == "TPB_DangKy")
                            {
                                l_Url = String.Format(Api_TPBank.GuiThongTinKH_BiLoi, l_DataTable.Rows[0]["CustId"].ToString());//Customer id
                            }
                            else if (p_Screen == "TPB_GiaiNgan")
                            {
                                l_Url = String.Format(Api_TPBank.GuiThongTinGiaiNgan_BiLoi, l_DataTable.Rows[0]["CustId"].ToString());//Customer id
                            }
                            l_jsData = new JavaScriptSerializer().Serialize(l_ListDataUpdate);
                            WriteLog_TPBank(l_Function, l_Method, l_Url, l_jsData, p_IdFinal, "Trước khi gọi API: " + l_jsData);
                            var resultAPI = CallApiTPBank(l_Function, l_Method, l_Url, l_Key, p_IdFinal, l_jsData);
                            WriteLog_TPBank(l_Function, l_Method, l_Url, l_jsData, p_IdFinal, "Sau khi gọi API: " + resultAPI);
                            var result = new JavaScriptSerializer().Deserialize<TPFico_Customer_Result>(resultAPI);
                            l__result = new TPFico_Customer_Result()
                            {
                                status = result.status,
                                messages = result.messages,
                                StatusCode = result.StatusCode
                            };
                            if (l__result.status == "1")
                            {
                                int l__StatusChange = 0;
                                if (p_Screen == "TPB_DangKy")
                                {
                                    l__StatusChange = (int)Status.PROCESSING; // Processing bước đăng ký
                                }
                                else if (p_Screen == "TPB_GiaiNgan")
                                {
                                    l__StatusChange = (int)Status.WaitingDiburse; // Processing bước giải ngân
                                }
                                g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_TPFico_PushResult", CommandType.StoredProcedure, new SqlParameter[]{
                                    new SqlParameter("@Result", l__StatusChange),
                                    new SqlParameter("@Msg", "Gửi thông tin thành công!"),
                                    new SqlParameter("@CustId", p_IdFinal),
                                });

                                l__result = new TPFico_Customer_Result()
                                {
                                    status = "1",
                                    messages = "Gửi thành công"
                                };
                            }
                            #endregion
                        }
                        else
                        {
                            l__result = new TPFico_Customer_Result()
                            {
                                status = "0",
                                messages = "Xảy ra lỗi khi gộp hình"
                            };
                            WriteLog_TPBank(l_Function, l_Method, l_Url, l_jsData, p_IdFinal, l_error_GopHinh);
                        }
                    }
                    else
                    {
                        l__result = new TPFico_Customer_Result()
                        {
                            status = "0",
                            messages = "Không tìm thấy thông tin"
                        };
                        WriteLog_TPBank(l_Function, l_Method, l_Url, l_jsData, p_IdFinal, "Không tìm thấy thông tin");
                    }
                }
                else
                {
                    l__result = new TPFico_Customer_Result()
                    {
                        status = "0",
                        messages = "Không lấy được chuỗi token"
                    };
                    WriteLog_TPBank(l_Function, l_Method, l_Url, l_jsData, p_IdFinal, "Không nhận được chuỗi Token");
                }
            }
            catch (Exception ex)
            {
                string l__error = "- Bước bị lỗi: " + l__Step + "- Lỗi: " + ex.ToString();
                string msgError = "";
                if (ex.ToString().Contains("Unable to connect to the remote server"))
                {
                    msgError = "Không kết nối được tới TPBank";
                }
                else
                {
                    msgError = l__error;
                }

                l__result = new TPFico_Customer_Result()
                {
                    status = "0",
                    messages = msgError
                };
                WriteLog_TPBank(l_Function, l_Method, l_Url, l_jsData, p_IdFinal, msgError);
            }
            //var json = Json(l__result, JsonRequestBehavior.AllowGet);
            //json.MaxJsonLength = int.MaxValue;
            return JsonConvert.SerializeObject(l__result);
        }
        public void InstallmentTPBank_TPFico_GetAndUpdateCustInfo(string p_IdFinal)
        {
            string l__Step = "";
            string l_Function = MethodBase.GetCurrentMethod().Name, l_Method = "GET", l_Url = "", l_jsData = "", l_Error = "";
            try
            {
                string l_Key = InstallmentTPBank_TPFico_GetAuthorization();
                if (!string.IsNullOrEmpty(l_Key.Trim()))
                {
                    #region == Get data from TPFico ==
                    l_Url = String.Format(Api_TPBank.LayThongTinDangKyKH, p_IdFinal);
                    var l_custInfo = new TPFico_CustInfo_GetAllData();
                    WriteLog_TPBank(l_Function, l_Method, l_Url, l_jsData, p_IdFinal, "Trước khi gọi API: " + l_jsData);
                    var resultAPI = CallApiTPBank(l_Function, l_Method, l_Url, l_Key, p_IdFinal, l_jsData);
                    WriteLog_TPBank(l_Function, l_Method, l_Url, l_jsData, p_IdFinal, "Sau khi gọi API: " + resultAPI);
                    var result = new JavaScriptSerializer().Deserialize<TPFico_Customer_Result>(resultAPI);
                    if (result.status == "1")
                    {
                        l_custInfo = new JavaScriptSerializer().Deserialize<TPFico_CustInfo_GetAllData>(result.dataResponse);
                        string xml = "";
                        using (var stringwriter = new System.IO.StringWriter())
                        {
                            var serializer = new XmlSerializer(l_custInfo.GetType());
                            serializer.Serialize(stringwriter, l_custInfo);
                            xml = stringwriter.ToString();
                        };
                        if (xml != "")
                        {
                            g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_UpdateAll_CustomerInfor"
                                                        , CommandType.StoredProcedure
                                                        , new SqlParameter[]{
                                                                new SqlParameter("@strInfoCust_TPBank", xml)
                                                    }
                            );
                        }
                    }
                    #endregion
                }
                else
                {
                    WriteLog_TPBank(l_Function, l_Method, l_Url, l_jsData, p_IdFinal, "Không nhận được chuỗi Token");
                }
            }
            catch (Exception ex)
            {
                string l__error = "- Bước bị lỗi: " + l__Step + "- Lỗi: " + ex.InnerException;
                WriteLog_TPBank(l_Function, l_Method, l_Url, l_jsData, p_IdFinal, l__error);
            }
        }
        #endregion
        // TuanNA89 - 06/08/2020 - Tối ưu lại 1 số Code liên quan đến TPBank
        #endregion

        #region ===Function để TPBank gọi===
        [HttpPost]
        public ActionResult InstallmentTPBank_TPFico_PushResult()
        {
            var l__req_Input = Request.InputStream;
            var l__req_StreamReader = new StreamReader(l__req_Input).ReadToEnd();
            int LogId = WriteLog(0, 0, "InstallmentTPBank_TPFico_PushResult", l__req_StreamReader, "");
            var l__ObjResult = new TPFico_Customer_Result();
            string l__Result = "";
            try
            {
                var l__Json_Decrypt = PGPEncrypt_SignAndDecrypt(l__req_StreamReader);
                var l__Data = new JavaScriptSerializer().Deserialize<TPFico_PushResult>(l__Json_Decrypt);
                WriteLog(0, 0, "InstallmentTPBank_TPFico_PushResult", l__Json_Decrypt, "");
                bool isStatusDefined;
                int statusCode;
                if (l__Data.status == null)
                {
                    l__ObjResult = new TPFico_Customer_Result
                    {
                        status = "0",
                        messages = "Status is not null!!"
                    };
                }
                else
                    if (l__Data.custId == null || l__Data.custId == "0")
                {
                    l__ObjResult = new TPFico_Customer_Result
                    {
                        status = "0",
                        messages = "Thiếu custId!"
                    };
                }
                else if (l__Data.status == null)
                {
                    l__ObjResult = new TPFico_Customer_Result
                    {
                        status = "0",
                        messages = "Thiếu Result!"
                    };
                }
                else
                {

                    isStatusDefined = Enum.IsDefined(typeof(Status), l__Data.status);

                    if (!isStatusDefined)
                    {
                        l__ObjResult = new TPFico_Customer_Result
                        {
                            status = "0",
                            messages = "Status chưa được khai báo"
                        };
                    }
                    else
                    {
                        if (l__Data.status == Status.APPROVED.ToString()) // duyệt
                        {
                            //TuanNA89 - 06/08/2020 - Tối ưu lại 1 số Code liên quan đến TPBank
                            InstallmentTPBank_TPFico_GetAndUpdateCustInfo(l__Data.custId);
                            //string l__XmlCustInfo = "";
                            //l__XmlCustInfo = InstallmentTPBank_TPFico_GetCustInfo(l__Data.custId);

                            //if (l__XmlCustInfo != "")
                            //{
                            //    g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_UpdateAll_CustomerInfor"
                            //                                , CommandType.StoredProcedure
                            //                                , new SqlParameter[]{
                            //                                    new SqlParameter("@strInfoCust_TPBank", l__XmlCustInfo)
                            //                            }
                            //    );
                            //}
                        }

                        Status status = (Status)Enum.Parse(typeof(Status), l__Data.status);
                        statusCode = (int)status;

                        SqlParameter[] l_SqlParameter = new SqlParameter[]{
                                    new SqlParameter("@Result", statusCode),
                                    new SqlParameter("@Msg", l__Data.message),
                                    new SqlParameter("@CustId", l__Data.custId),
                                };
                        DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_TPFico_PushResult", CommandType.StoredProcedure, l_SqlParameter);
                        if (l_DataTable != null && l_DataTable.Rows.Count > 0)
                        {
                            l__ObjResult = new TPFico_Customer_Result
                            {
                                status = l_DataTable.Rows[0]["Result"].ToString(),
                                messages = l_DataTable.Rows[0]["Msg"].ToString()
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(0, 0, "InstallmentTPBank_TPFico_PushResult", l__req_StreamReader, ex.ToString());
                l__ObjResult = new TPFico_Customer_Result
                {
                    status = "0",
                    messages = ex.ToString()
                };

            }
            if (l__ObjResult.status == "0")
            {
                // TuanNA89 - 06/08/2020 - Tối ưu lại 1 số Code liên quan đến TPBank
                //Response.StatusCode = 400;
                l__Result = PGPEncrypt_SignAndEncrypt(JsonConvert.SerializeObject(l__ObjResult));
            }
            if (l__ObjResult.status == "1") Response.StatusCode = (int)HttpStatusCode.OK;

            return Json(l__Result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult InstallmentTPBank_TPFico_PushResult_UpdateDetail()
        {
            var l__req_Input = Request.InputStream;
            var l__req_StreamReader = new StreamReader(l__req_Input).ReadToEnd();
            var l__ObjResult = new TPFico_Customer_Result();
            string l__Result = "";
            try
            {
                var l__Json_Decrypt = PGPEncrypt_SignAndDecrypt(l__req_StreamReader);
                var p_Data = new JavaScriptSerializer().Deserialize<TPFico_PushResult>(l__Json_Decrypt);
                WriteLog(0, 0, "InstallmentTPBank_TPFico_PushData_UpdateField", l__Json_Decrypt, "");

                if (p_Data.custId == null || p_Data.custId == "0")
                {
                    l__ObjResult = new TPFico_Customer_Result
                    {
                        status = "0",
                        messages = "Thiếu custId!"
                    };
                }
                else
                {
                    var writer = new StringWriter();
                    var serializer = new XmlSerializer(p_Data.GetType());
                    serializer.Serialize(writer, p_Data);
                    string strFieldUpdate = writer.ToString();

                    WriteLog(0, 0, "InstallmentTPBank_TPFico_PushData_UpdateField", strFieldUpdate, "");

                    SqlParameter[] l_SqlParameter = new SqlParameter[]{
                            new SqlParameter("@CustId", p_Data.custId),
                            new SqlParameter("@strFieldUpdate", strFieldUpdate),
                    };
                    DataTable l_DataTable = g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_TPFico_PushData_UpdateField", CommandType.StoredProcedure, l_SqlParameter);
                    if (l_DataTable != null && l_DataTable.Rows.Count > 0)
                    {
                        l__ObjResult = new TPFico_Customer_Result
                        {
                            status = l_DataTable.Rows[0]["Result"].ToString(),
                            messages = l_DataTable.Rows[0]["Msg"].ToString()
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(0, 0, "InstallmentTPBank_TPFico_PushData_UpdateField", l__req_StreamReader, ex.ToString());
                l__ObjResult = new TPFico_Customer_Result
                {
                    status = "0",
                    messages = ex.ToString()
                };
            }
            if (l__ObjResult.status == "0")
            {
                Response.StatusCode = 400;
                l__Result = PGPEncrypt_SignAndEncrypt(JsonConvert.SerializeObject(l__ObjResult));
            }
            if (l__ObjResult.status == "1") Response.StatusCode = (int)HttpStatusCode.OK;

            return Json(l__Result, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult InstallmentTPBank_TPFico_Download()
        {
            var l__req_Input = Request.InputStream;
            var l__req_StreamReader = new StreamReader(l__req_Input).ReadToEnd();
            int LogId = WriteLog(0, 0, "InstallmentTPBank_TPFico_DownloadFile", l__req_StreamReader, "");
            var l__ObjResult = new TPFico_Customer_Result();
            string l__Result = "";
            try
            {
                var l__Json_Decrypt = l__req_StreamReader;// PGPEncrypt_SignAndDecrypt(l__req_StreamReader);
                var p_Data = new JavaScriptSerializer().Deserialize<List<TPFico_CustInfo_Photo_GetData>>(l__Json_Decrypt);
                string p__Link = "";
                foreach (var item in p_Data)
                {
                    if ((!string.IsNullOrEmpty(item.link)) && (!string.IsNullOrWhiteSpace(item.link)))
                    {
                        p__Link = item.link.Replace(authority, "");
                        item.Data = GetFile_StringBase64(p__Link);
                    }
                }

                l__Result = JsonConvert.SerializeObject(p_Data);
                l__ObjResult = new TPFico_Customer_Result
                {
                    status = "1",
                    messages = l__Result
                };
                Response.StatusCode = (int)HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                WriteLog(0, 0, "InstallmentTPBank_TPFico_DownloadFile", l__req_StreamReader, ex.ToString());
                l__ObjResult = new TPFico_Customer_Result
                {
                    status = "0",
                    messages = ex.ToString()
                };
                Response.StatusCode = 400;
            }

            return Json(l__Result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult InstallmentTPBank_TPFico_Delete()
        {
            var l__req_Input = Request.InputStream;
            var l__req_StreamReader = new StreamReader(l__req_Input).ReadToEnd();
            int LogId = WriteLog(0, 0, "InstallmentTPBank_TPFico_Delete", l__req_StreamReader, "");
            var l__ObjResult = new TPFico_Customer_Result();
            string l__Result = "";
            try
            {
                var l__Json_Decrypt = l__req_StreamReader;// PGPEncrypt_SignAndDecrypt(l__req_StreamReader);
                var p_Data = new JavaScriptSerializer().Deserialize<TPFico_CustInfo_Photo_Delete>(l__Json_Decrypt);
                string p__Link = "";
                var l_FolderFileAttach = Keyword.FolderFileAttach;
                List<string> l__ListRemove = new List<string>();
                string l__RemovePath = authority + "/" + l_FolderFileAttach;
                foreach (var item in p_Data.ListRemove)
                {
                    if ((!string.IsNullOrEmpty(item.link)) && (!string.IsNullOrWhiteSpace(item.link)))
                    {
                        if ((item.documentType == "National_ID") || (item.documentType == "fb") || (item.documentType == "ACCA"))
                        {
                            p__Link = item.link.Replace(l__RemovePath, "");
                            //p__Link = p__Link.Replace(l__RemovePath, "");
                            l__ListRemove.Add(p__Link);
                        }
                    }
                }
                DeleteListImages(l__ListRemove);

                l__ObjResult = new TPFico_Customer_Result
                {
                    status = "1",
                    messages = "Success"
                };
                Response.StatusCode = (int)HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                WriteLog(0, 0, "InstallmentTPBank_TPFico_Delete", l__req_StreamReader, ex.ToString());
                l__ObjResult = new TPFico_Customer_Result
                {
                    status = "0",
                    messages = ex.ToString()
                };
                Response.StatusCode = 400;
            }
            l__Result = JsonConvert.SerializeObject(l__ObjResult);
            return Json(l__Result, JsonRequestBehavior.AllowGet);
        }
        //▼	Add - TuanNA89 - 08/07/2019 - Download file bằng link Url
        [HttpGet]
        public ActionResult InstallmentTPBank_TPFico_DownloadFile(string fileName = "", bool isBase64 = false, string Url = "")
        {
            try
            {
                string p__FilePath = "";
                if (Url.Contains("/TPBank/"))
                    p__FilePath = Url.Replace(authority, "");
                var l_OriginalDirectory = new DirectoryInfo(Server.MapPath(p__FilePath));
                var l__FullPath = System.IO.Path.Combine(l_OriginalDirectory.ToString(), "");
                if (System.IO.File.Exists(l__FullPath))
                {
                    FileInfo file = new FileInfo(l__FullPath);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(l__FullPath);
                    var cd = new System.Net.Mime.ContentDisposition
                    {
                        FileName = file.Name,
                        Inline = false,
                    };
                    Response.StatusCode = 200;
                    fileName = fileName + file.Extension;
                    if (isBase64)
                    {
                        string base64 = Convert.ToBase64String(fileBytes);
                        var result = new
                        {
                            base64 = base64
                        };
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                    }
                }
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogError("RegistrationForm - " + MethodBase.GetCurrentMethod().Name + "(" + Url + ")", ex.ToString());
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }
        //▲	Add - TuanNA89 - 08/07/2019 - Download file bằng link Url

        public string InstallmentTPBank_TPFico_DeleteFolder()
        {
            string result = "";
            var l_FolderFileAttach = Keyword.FolderFileAttach;
            var l_OriginalDirectory = new DirectoryInfo(Server.MapPath(l_FolderFileAttach));
            string l_PathString = System.IO.Path.Combine(l_OriginalDirectory.ToString(), "");
            var l_Path_Folder = string.Format("{0}{1}", l_PathString, "TPBank");
            DateTime TimeOldest = DateTime.Now;
            DirectoryInfo di = new DirectoryInfo(l_Path_Folder);
            DirectoryInfo[] subDirs = di.GetDirectories();
            string folderOldest = "";

            if (subDirs.Length > 0)
            {
                foreach (DirectoryInfo subDir in subDirs)
                {
                    if (subDir.CreationTime < TimeOldest)
                    {
                        TimeOldest = subDir.CreationTime;
                        folderOldest = subDir.Name;
                    }
                }
            }

            result = "folder oldest = " + folderOldest + ", create time = " + TimeOldest.ToShortDateString();

            return result;
        }
        #endregion

        #region ===Function hỗ trợ===
        //  =================================================================
        private static PgpPrivateKey FindSecretKey(PgpSecretKeyRingBundle pgpSec, long keyId, char[] pass)
        {
            PgpSecretKey pgpSecKey = pgpSec.GetSecretKey(keyId);

            if (pgpSecKey == null)
                return null;

            return pgpSecKey.ExtractPrivateKey(pass);
        }
        public byte[] Decrypt(byte[] inputData, Stream keyIn)
        {
            Stream inputStream = new MemoryStream(inputData);
            inputStream = PgpUtilities.GetDecoderStream(inputStream);
            MemoryStream decoded = new MemoryStream();


            PgpObjectFactory pgpF = new PgpObjectFactory(inputStream);
            PgpEncryptedDataList enc;
            PgpObject o = pgpF.NextPgpObject();

            if (o is PgpEncryptedDataList)
                enc = (PgpEncryptedDataList)o;
            else
                enc = (PgpEncryptedDataList)pgpF.NextPgpObject();


            PgpPrivateKey sKey = null;
            PgpPublicKeyEncryptedData pbe = null;

            PgpSecretKeyRingBundle pgpSec = new PgpSecretKeyRingBundle(
            PgpUtilities.GetDecoderStream(keyIn));


            foreach (PgpPublicKeyEncryptedData pked in enc.GetEncryptedDataObjects())
            {
                sKey = FindSecretKey(pgpSec, pked.KeyId, "123456a@".ToCharArray());
                if (sKey != null)
                {
                    pbe = pked;
                    break;
                }
            }

            Stream clear = pbe.GetDataStream(sKey);
            PgpObjectFactory plainFact = new PgpObjectFactory(clear);
            PgpObject message = plainFact.NextPgpObject();

            if (message is PgpCompressedData)
            {
                PgpCompressedData cData = (PgpCompressedData)message;
                PgpObjectFactory pgpFact = new PgpObjectFactory(cData.GetDataStream());
                message = pgpFact.NextPgpObject();
                if (message is PgpLiteralData)
                {
                    PgpLiteralData ld = (PgpLiteralData)message;
                    Stream unc = ld.GetInputStream();
                    Streams.PipeAll(unc, decoded);
                }
                else if (message is PgpOnePassSignatureList)
                {
                    message = pgpFact.NextPgpObject();
                    PgpLiteralData Ld = (PgpLiteralData)message;
                    Stream unc = Ld.GetInputStream();
                    Streams.PipeAll(unc, decoded);
                }
            }

            return decoded.ToArray();
        }

        [HttpGet]
        public void Test(string pathToImage, string destImagePath)
        {
            Image myImage = Image.FromFile(pathToImage, true);
            // Save the image with a quality of 50% 
            SaveJpeg(destImagePath, myImage, 50);
        }
        public string PGPEncrypt_SignAndEncrypt(string p_Text)
        {
            string pubPath = "/Key/tpf_fpt-pub.asc";
            FileInfo pubfile = new FileInfo(HttpContext.Server.MapPath(pubPath));
            pubPath = pubfile.FullName;

            string privPath = "/Key/frt_priv.asc";
            FileInfo privfile = new FileInfo(HttpContext.Server.MapPath(privPath));
            privPath = privfile.FullName;

            PgpEncryptionKeys keys = new PgpEncryptionKeys(pubPath, privPath, "123456a@");
            PgpEncrypt pgpEncrypt = new PgpEncrypt(keys);

            string strEncrypt = pgpEncrypt.SignAndEncrypt(Encoding.UTF8.GetBytes(p_Text));

            return strEncrypt;
        }
        public string PGPEncrypt_SignAndDecrypt(string p_Text)
        {
            string pubPath = "/Key/tpf_fpt-pub.asc";
            FileInfo pubfile = new FileInfo(HttpContext.Server.MapPath(pubPath));
            pubPath = pubfile.FullName;

            string privPath = "/Key/frt_priv.asc";
            FileInfo privfile = new FileInfo(HttpContext.Server.MapPath(privPath));
            privPath = privfile.FullName;

            PgpEncryptionKeys keys = new PgpEncryptionKeys(pubPath, privPath, "123456a@");
            PgpEncrypt pgpEncrypt = new PgpEncrypt(keys);
            // TuanNA89 - 06/08/2020 - Tối ưu lại 1 số Code liên quan đến TPBank
            string strDecrypt = "";
            try
            {
                strDecrypt = Encoding.UTF8.GetString(Decrypt(Encoding.UTF8.GetBytes(p_Text), new FileStream(privPath, FileMode.Open, FileAccess.Read)));
            }
            catch (Exception ex)
            {
                strDecrypt = p_Text;
            }            
            return strDecrypt;
        }
        int WriteLog(int IdLog, int IdApiDoiTacTraVe, string Title, string Content, string Error)
        {
            ServiceDataController service = new ServiceDataController();
            int result = service.WriteLogApi(IdLog, IdApiDoiTacTraVe, Title, Content, Error);
            return result;
        }
        public string MergeImages(List<string> p__FileNames, string p__Path_Folder, string p__Category)
        {
            string time1 = "", time2 = "", l_FolderFileAttach = "", l_PathString = "", l_NewPath = "", l_NewName = "", l_User = "";
            FileInfo[] files;
            List<FileInfo> tempList = new List<FileInfo>();
            string l__Step = "";
            try
            {
                #region == Get data hình ==
                l_NewName = p__Category;
                l__Step = "Lấy thời gian";
                time1 = DateTime.Now.ToString("yyyyMMddHHmmss");
                if ((UserManager.CurrentUser != null) && (UserManager.CurrentUser.LoginDateTime != null))
                    time2 = UserManager.CurrentUser.LoginDateTime.ToString("yyyyMMddHHmmss");

                if ((UserManager.CurrentUser != null) && (UserManager.CurrentUser.InsideCode != null))
                    l_User = UserManager.CurrentUser.InsideCode;
                l__Step = "Lấy link folder chứa ảnh";
                l_FolderFileAttach = Keyword.FolderFileAttach;
                if (l_FolderFileAttach == null)
                    l__Step = "l_FolderFileAttach bị null";

                l__Step = "Lấy full link folder chứa ảnh";
                var l_OriginalDirectory = new DirectoryInfo(Server.MapPath(l_FolderFileAttach));
                if (l_OriginalDirectory == null)
                {
                    l__Step = "l_OriginalDirectory bị null";
                    WriteLog(0, 0, "MergeImages", "", l__Step);
                    return "";
                }

                l__Step = "Lấy full link folder chứa ảnh";
                l_PathString = System.IO.Path.Combine(l_OriginalDirectory.ToString(), "");
                if (l_PathString == null)
                {
                    l__Step = "l_PathString bị null";
                    WriteLog(0, 0, "MergeImages", "", l__Step);
                    return "";
                }

                l__Step = "Lấy FileInfo ảnh";
                foreach (string l__FileName in p__FileNames)
                {
                    FileInfo f_CMND = new FileInfo(l_PathString + l__FileName);
                    tempList.Add(f_CMND);
                }

                l__Step = "Ghép path ảnh gộp";
                l_PathString = l_PathString + p__Path_Folder;
                l__Step = "Link sau khi gộp: " + l_PathString.ToString();

                l_NewName = l_NewName + "_" + time1 + "_" + time2 + "_" + l_User + ".jpg";
                l_NewPath = string.Format("{0}/{1}", l_PathString, l_NewName);

                if (!Directory.Exists(l_PathString))
                {
                    Directory.CreateDirectory(l_PathString);
                }

                files = tempList.ToArray();
                //TuanNA89 - 06/08/2020 - Tối ưu lại 1 số Code liên quan đến TPBank
                #endregion
                #region == Merge hình ==
                string finalImage = l_NewPath;
                List<int> imageWidths = new List<int>();
                int nIndex = 0;
                int width = 0;
                int height = 0;

                foreach (FileInfo file in files)
                {
                    Image img = Image.FromFile(file.FullName);

                    imageWidths.Add(img.Width);
                    height += img.Height;

                    img.Dispose();
                }

                imageWidths.Sort();
                width = imageWidths[imageWidths.Count - 1];
                Bitmap img3 = new Bitmap(width, height);

                Graphics g = Graphics.FromImage(img3);
                g.Clear(SystemColors.AppWorkspace);
                long l__byteAllImages = 0;

                foreach (FileInfo file in files)
                {
                    Image img = Image.FromFile(file.FullName);
                    l__byteAllImages += file.Length;
                    if (nIndex == 0)
                    {
                        g.DrawImage(img, 0, 0, img.Width, img.Height);
                        nIndex++;
                        height = img.Height;
                    }
                    else
                    {
                        g.DrawImage(img, 0, height, img.Width, img.Height);
                        height += img.Height;
                    }
                    img.Dispose();
                }
                g.Dispose();

                //int l__Quality = GetQualityNewImageWithRule(l__byteAllImages, p__Category);                
                #region == Tính toán độ phân giải độ phân giải ==
                int l__Quality = 90;
                double l__MaxSize = 1.5 * 1024 * 1024;
                if (p__Category == "Signature")
                {
                    l__MaxSize = 0.6 * 1024 * 1024;
                }

                if (l__byteAllImages > l__MaxSize)
                    l__Quality = (int)(l__MaxSize * 100 / l__byteAllImages);

                if (l__Quality > 90)
                    l__Quality = 90;
                #endregion
                Image l__img = (Image)img3;
                SaveJpeg(finalImage, l__img, l__Quality);

                img3.Dispose();
                #endregion
                //CombineImages(files, l_NewPath, p__Category);

            }
            catch (Exception ex)
            {
                string l__error = "- Bước bị lỗi: " + l__Step + "- Lỗi: " + ex.ToString();
                Logger.WriteLogError("RegistrationForm - " + MethodBase.GetCurrentMethod().Name + "()", l__error);
                WriteLog(0, 0, "MergeImages", "", l__error);
                l_NewName = "";
                p__Path_Folder = "";
            }

            return p__Path_Folder + l_NewName;
        }
        private int GetQualityNewImageWithRule(long p__length, string p__Category)
        {
            int quality = 90;
            double l__MaxSize = 1.5 * 1024 * 1024;
            if (p__Category == "Signature")
            {
                l__MaxSize = 0.6 * 1024 * 1024;
            }

            if (p__length > l__MaxSize)
                quality = (int)(l__MaxSize * 100 / p__length);

            if (quality > 90)
                quality = 90;

            return quality;
        }
        private void CombineImages(FileInfo[] files, string p__NewPath, string p__Category)
        {
            string finalImage = p__NewPath;
            List<int> imageWidths = new List<int>();
            int nIndex = 0;
            int width = 0;
            int height = 0;
            try
            {
                foreach (FileInfo file in files)
                {
                    Image img = Image.FromFile(file.FullName);

                    imageWidths.Add(img.Width);
                    height += img.Height;

                    img.Dispose();
                }

                imageWidths.Sort();
                width = imageWidths[imageWidths.Count - 1];
                Bitmap img3 = new Bitmap(width, height);

                Graphics g = Graphics.FromImage(img3);
                g.Clear(SystemColors.AppWorkspace);
                long l__byteAllImages = 0;

                foreach (FileInfo file in files)
                {
                    Image img = Image.FromFile(file.FullName);
                    l__byteAllImages += file.Length;
                    if (nIndex == 0)
                    {
                        g.DrawImage(img, 0, 0, img.Width, img.Height); // Edit - TuanNA89 - 18/05/2020 - Fix lỗi gộp sai kích thước hình
                        nIndex++;
                        height = img.Height;
                    }
                    else
                    {
                        g.DrawImage(img, 0, height, img.Width, img.Height); // Edit - TuanNA89 - 18/05/2020 - Fix lỗi gộp sai kích thước hình
                        height += img.Height;
                    }
                    img.Dispose();
                }
                g.Dispose();


                int l__Quality = GetQualityNewImageWithRule(l__byteAllImages, p__Category);
                Image l__img = (Image)img3;
                SaveJpeg(finalImage, l__img, l__Quality);

                img3.Dispose();
            }
            catch (Exception ex)
            {
                Logger.WriteLogError("RegistrationForm - " + MethodBase.GetCurrentMethod().Name + "()", ex.ToString());
                WriteLog(0, 0, "CombineImages", "", ex.ToString());
            }
        }
        public void DeleteListImages(List<string> p__FileNames)
        {
            try
            {
                var l_FolderFileAttach = Keyword.FolderFileAttach;
                var l_OriginalDirectory = new DirectoryInfo(Server.MapPath(l_FolderFileAttach));
                var l_PathString = System.IO.Path.Combine(l_OriginalDirectory.ToString(), "");
                string l__Path = "";

                foreach (string l__FileName in p__FileNames)
                {
                    l__Path = string.Format("{0}/{1}", l_PathString, l__FileName);
                    if (System.IO.File.Exists(l__Path))
                    {
                        System.IO.File.Delete(l__Path);
                    }
                }
            }
            catch (Exception ex)
            {
                string l__LogInput = JsonConvert.SerializeObject(p__FileNames);
                Logger.WriteLogError("RegistrationForm - " + MethodBase.GetCurrentMethod().Name + "( p__FileNames = " + l__LogInput + "):", ex.ToString());
            }
        }
        public static void SaveJpeg(string path, Image img, int quality)
        {
            try
            {
                if (quality < 0 || quality > 100)
                    throw new ArgumentOutOfRangeException("quality must be between 0 and 100.");

                // Encoder parameter for image quality 
                EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                // JPEG image codec 
                ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");
                EncoderParameters encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = qualityParam;
                img.Save(path, jpegCodec, encoderParams);
            }
            catch (Exception ex)
            {
                Logger.WriteLogError("RegistrationForm - " + MethodBase.GetCurrentMethod().Name + "()", ex.ToString());
            }
        }
        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats 
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec 
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];

            return null;
        }
        public string GetFile_StringBase64(string p__Path)
        {
            string base64 = "";
            try
            {
                var l_OriginalDirectory = new DirectoryInfo(Server.MapPath(p__Path));
                var l__FullPath = System.IO.Path.Combine(l_OriginalDirectory.ToString(), "");
                if (System.IO.File.Exists(l__FullPath))
                {
                    byte[] imageArray = System.IO.File.ReadAllBytes(l__FullPath);
                    base64 = Convert.ToBase64String(imageArray);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogError("RegistrationForm - " + MethodBase.GetCurrentMethod().Name + "()", ex.ToString());
                WriteLog(0, 0, "GetFile_StringBase64", "", ex.ToString());
            }
            return base64;
        }
        // TuanNA89 - 06/08/2020 - Tối ưu lại 1 số Code liên quan đến TPBank
        #region Code mới
        // Function gọi API
        public string CallApiTPBank(string p_FunctionCall, string p_Method, string p_Api, string p_Auth, string p_CustId, string l__jsData)
        {
            var l__Step = "";
            HttpWebResponse l__oResp = null;
            var result = new TPFico_Customer_Result();
            string l_Url = "";

            var l_urlmoiFwd = "";
            if (g__isUseFwd == "1") // Sử dụng domain Fwd TPBank nếu isUseFwd = 1
            {
                l_Url = g__UrlFwd;
                l_urlmoiFwd = g__Url;
            }
            else
            {
                l_Url = g__Url;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                    delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                                            System.Security.Cryptography.X509Certificates.X509Chain chain,
                                            System.Net.Security.SslPolicyErrors sslPolicyErrors)
                    {
                        return true; // **** Always accept
                    };
                System.Net.ServicePointManager.Expect100Continue = false;
            }

            #region == Push data ==
            try
            {

                p_FunctionCall = MethodBase.GetCurrentMethod().Name + "-" + p_FunctionCall;
                string l__ReqUriStr = l_Url + p_Api;
                l__Step = "Lấy link bước 1";
                var l__HttpWebReq = WebRequest.Create(l__ReqUriStr) as HttpWebRequest;
                l__HttpWebReq.KeepAlive = false;
                l__HttpWebReq.Method = p_Method;
                l__HttpWebReq.Headers.Add("Authorization", p_Auth);
                if (g__isUseFwd == "1")
                {
                    l__HttpWebReq.Headers.Add("urlmoi", g__Url);
                }
                l__Step = "Bắt đầu gọi API";
                if (p_Method == "POST" || p_Method == "PUT")
                {
                    l__Step = "Mã hoá data";
                    var l__encryptData = PGPEncrypt_SignAndEncrypt(l__jsData);
                    l__HttpWebReq.ContentType = "application/json";

                    WriteLog_TPBank(p_FunctionCall, p_Method, l_Url, l__encryptData, p_CustId, l__Step);
                    using (Stream stm = l__HttpWebReq.GetRequestStream())
                    {
                        using (StreamWriter stmw = new StreamWriter(stm))
                        {
                            stmw.Write(l__encryptData);
                        }
                    }
                }
                HttpStatusCode? StatusCode = new HttpStatusCode();
                int statusCode;
                string statusName;
                l__Step = "Lấy chuỗi response";
                try
                {
                    l__Step = "Chạy GetResponse()";
                    l__oResp = (HttpWebResponse)l__HttpWebReq.GetResponse();
                }
                catch (WebException we)
                {
                    l__Step = "Nhảy vào WebException";
                    l__oResp = (HttpWebResponse)we.Response;
                    WriteLog_TPBank(p_FunctionCall, p_Method, l_Url, l__jsData, p_CustId, "WebException: " + we.ToString());
                }
                l__Step = "Check Response";
                if (l__oResp != null)
                {
                    l__Step = "Có Response -> Check StatusCode";
                    StatusCode = l__oResp.StatusCode;
                    string l_data = null;
                    if (StatusCode != null)
                    {
                        l__Step = "Có StatusCode -> Lấy status + stream reader";
                        statusCode = (int)StatusCode;
                        statusName = StatusCode.ToString();
                        using (StreamReader l__StreamReader = new StreamReader(l__oResp.GetResponseStream()))
                        {
                            l_data = l__StreamReader.ReadToEnd();
                        }
                    }

                    l__Step = "Giải mã chuỗi stream Reader";
                    string decryptResult = "", l__Json_Decrypt = "";
                    WriteLog_TPBank(p_FunctionCall, p_Method, l_Url, l_data, p_CustId, "Response chưa mã hóa");
                    if (p_FunctionCall.Contains("InstallmentTPBank_TPFico_GetAuthorization"))
                    {
                        decryptResult = l_data;
                    }
                    else
                    {
                        l__Json_Decrypt = PGPEncrypt_SignAndDecrypt(l_data);
                        decryptResult = l__Json_Decrypt;
                    }

                    if ((new List<HttpStatusCode?> { HttpStatusCode.Created, HttpStatusCode.OK }.IndexOf(StatusCode) > -1)
                        || decryptResult.Contains("Lỗi gọi dịch vụ web cust id"))
                    {
                        result = new TPFico_Customer_Result()
                        {
                            status = "1",
                            messages = "Thành công",
                            dataResponse = decryptResult
                        };
                    }
                    else if (StatusCode == HttpStatusCode.InternalServerError)
                    {
                        l__Step = "Gán vào list lỗi";
                        WriteLog_TPBank(p_FunctionCall, p_Method, l_Url, l__jsData, p_CustId, "Lỗi 500 InternalServerError");
                        result.status = "0";
                        result.messages = "Lỗi không kết nối được TPBank (Error 500)";
                        result.StatusCode = StatusCode;
                    }
                    else
                    {
                        l__Step = "Gán vào list lỗi";
                        WriteLog_TPBank(p_FunctionCall, p_Method, l_Url, l__jsData, p_CustId, l__Json_Decrypt);
                        result.status = "0";
                        result.messages = "Lỗi TPBank trả về: " + decryptResult;
                        result.StatusCode = StatusCode;
                    }
                }
                else
                {
                    result = new TPFico_Customer_Result()
                    {
                        status = "0",
                        messages = "Không nhận được chuỗi response"
                    };
                    WriteLog_TPBank(p_FunctionCall, p_Method, l_Url, l__jsData, p_CustId, "Không nhận được chuỗi response");
                }
            }
            catch (Exception ex)
            {
                string l__error = "- Bước bị lỗi: " + l__Step + "- Lỗi: " + ex.ToString();
                Logger.WriteLogError("RegistrationForm - " + MethodBase.GetCurrentMethod().Name + "()", l__error);
                string msgError = "";
                if (ex.ToString().Contains("Unable to connect to the remote server"))
                {
                    msgError = "Không kết nối được tới TPBank";
                }
                else
                {
                    msgError = ex.ToString();
                }

                result = new TPFico_Customer_Result()
                {
                    status = "0",
                    messages = msgError
                };
                WriteLog_TPBank(p_FunctionCall, p_Method, l_Url, l__jsData, p_CustId, l__error);
            }
            if (l__oResp != null)
            {
                l__oResp.Close();
            }
            #endregion

            return new JavaScriptSerializer().Serialize(result);
        }
        void WriteLog_TPBank(string p_Function, string p_Method, string p_Url, string p_ObjectData, string p_IdFinal, string p_Error)
        {
            g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_WriteLog", CommandType.StoredProcedure, new SqlParameter[] {
                new SqlParameter("@Function", p_Function),
                new SqlParameter("@Method", p_Method),
                new SqlParameter("@Url", p_Url),
                new SqlParameter("@ObjectData", p_ObjectData),
                new SqlParameter("@IdFinal", p_IdFinal),
                new SqlParameter("@Error", p_Error),
            });
        }
        #endregion
        #endregion

        #region ===Function app gọi===
        public ActionResult InstallmentTPBank_TPFico_AutoTuChoi()
        {
            int l__IdLog = 0;
            TPFico_Customer_Result l__result = new TPFico_Customer_Result();
            l__IdLog = WriteLog(0, 0, "InstallmentTPBank_TPFico_AutoTuChoi", "", "");
            string l__Step = "";
            try
            {
                SqlParameter[] l_SqlParameter = new SqlParameter[] { };
                l__Step = "Gọi store";
                DataTable l_DataTable = null;// g_SqlDBHelper.ExecuteCommand("sp_InstallmentTPBank_Job_AutoTuChoi", CommandType.StoredProcedure, l_SqlParameter);
                if (l_DataTable != null && l_DataTable.Rows.Count > 0)
                {
                    l__result = new TPFico_Customer_Result()
                    {
                        status = "1",
                        messages = "Từ chối OK"
                    };
                }
                else
                {
                    l__result = new TPFico_Customer_Result()
                    {
                        status = "0",
                        messages = "Không tìm thấy thông tin"
                    };
                }
            }
            catch (Exception ex)
            {
                string l__error = "- Bước bị lỗi: " + l__Step + "- Lỗi: " + ex.ToString();
                Logger.WriteLogError("RegistrationForm - " + MethodBase.GetCurrentMethod().Name + "()", l__error);
                l__result = new TPFico_Customer_Result()
                {
                    status = "0",
                    messages = ex.ToString()
                };
                WriteLog(l__IdLog, 0, "InstallmentTPBank_TPFico_AutoTuChoi", "", l__error);
            }
            string l__strResult = JsonConvert.SerializeObject(l__result);
            return Json(l__strResult, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #endregion
        //▲	Edit - VietMXH - 03/12/2018 - TPBank==================================================
        //▲	Edit - TuanNA89 - 04/03/2019 - TPBank==================================================
    }
    #endregion

    #region ======CLASS======
    #region ===RegistrationForm_Save===
    public class RegistrationForm_Save
    {
        public double SoSO { set; get; }
        public string LyDo { set; get; }
        public int Status { set; get; }
    }
    #endregion

    #region ===sp_RegistrationForm_Search_BySOList===
    public class sp_RegistrationForm_Search_BySOList
    {
        public double SoSO { set; get; }
    }
    #endregion

    #region ===TPFico_CustInfo===
    [XmlRoot("DATA")]
    public class TPFico_CustInfo
    {
        public string custId { set; get; }
        public string lastName { set; get; }
        public string firstName { set; get; }
        public string middleName { set; get; }
        public string gender { set; get; }
        public string dateOfBirth { set; get; }
        public string nationalId { set; get; }
        public string issueDate { set; get; }
        public string issuePlace { set; get; }
        public string employeeCard { set; get; }
        public string maritalStatus { set; get; }
        public string mobilePhone { set; get; }
        public string durationYear { set; get; }
        public string durationMonth { set; get; }
        public string map { set; get; }
        public string ownerNationalId { set; get; }
        public string contactAddress { set; get; }
        public string dsaCode { set; get; }
        public string companyName { set; get; }
        public string taxCode { set; get; }
        //Add - TuanNA89 - 02/07/2019 - Thêm thu nhập hàng tháng
        public string salary { set; get; }

        [XmlElement("loanDetail")]
        public TPFico_CustInfo_LoanDetail loanDetail { get; set; }
        [XmlArrayItem("address")]
        public List<TPFico_CustInfo_Address> addresses { set; get; }
        [XmlArrayItem("photo")]
        public List<TPFico_CustInfo_Photo> photos { set; get; }
        [XmlArrayItem("productDetail")]
        public List<TPFico_CustInfo_ProductDetail> productDetails { set; get; }
        [XmlArrayItem("reference")]
        public List<TPFico_CustInfo_Reference> references { set; get; }
    }
    public class TPFico_CustInfo_Photo
    {
        public string documentType { get; set; }
        public string link { get; set; }
    }
    public class TPFico_CustInfo_Address
    {
        public string addressType { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string ward { get; set; }
        public string district { get; set; }
        public string province { get; set; }
    }
    public class TPFico_CustInfo_ProductDetail
    {
        public string model { get; set; }
        public string goodCode { get; set; }
        public string goodType { get; set; }
        public string goodPrice { get; set; }
        public string quantity { get; set; }
    }
    public class TPFico_CustInfo_Reference
    {
        public string fullName { get; set; }
        public string phoneNumber { get; set; }
        public string relation { get; set; }
        public string personalId { get; set; }
    }
    public class TPFico_CustInfo_LoanDetail
    {
        public string product { get; set; }
        public string loanAmount { get; set; }
        public string downPayment { get; set; }
        public string annualr { get; set; }
        public string dueDate { get; set; }
        public string tenor { get; set; }
    }
    public class TPFico_CustInfo_Photo_GetData : TPFico_CustInfo_Photo
    {
        public string Data { get; set; }
    }
    public class TPFico_CustInfo_Photo_Delete
    {
        public string CustId { get; set; }
        public List<TPFico_CustInfo_Photo> ListRemove { get; set; }
    }
    #endregion

    #region ===TPFico_PushResult===
    [XmlRoot("RESULT")]
    public class TPFico_PushResult
    {
        public string custId { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        [XmlArrayItem("ERROR")]
        public List<TPFico_FieldPushReturn> errors { get; set; }
    }
    public class TPFico_FieldPushReturn
    {
        public string code { get; set; }
        public string comment { get; set; }
    }
    [XmlRoot("LISTUPDATE")]
    public class TPFico_ListFieldUpdate
    {
        public string IDFinal { get; set; }
        public string Screen { get; set; }
        [XmlArrayItem("UPDATE")]
        public List<TPFico_FieldUpdate> ListUpdate { get; set; }
    }
    public class TPFico_FieldUpdate
    {
        public string Id_Update { get; set; }
        public string Value { get; set; }
    }
    #endregion

    #region ===TPFico_Customer_Result===
    public class TPFico_Customer_Result
    {
        public string status { get; set; }
        public string messages { get; set; }
        // TuanNA89 - 06/08/2020 - Tối ưu lại 1 số Code liên quan đến TPBank
        public HttpStatusCode? StatusCode { get; set; }
        public string dataResponse { get; set; }
    }
    #endregion

    #region ===TPFico_DocumentInfo===
    public class TPFico_DocumentInfo
    {
        public string documentCode { get; set; }
        public string file { get; set; }
    }
    #endregion

    #region ===TPFico_CustInfo_GetAllData===
    [XmlRoot("DATA")]
    public class TPFico_CustInfo_GetAllData
    {
        public string custId { set; get; }
        public string appId { set; get; }
        public string lastName { set; get; }
        public string firstName { set; get; }
        public string middleName { set; get; }
        public string gender { set; get; }
        public string dateOfBirth { set; get; }
        public string nationalId { set; get; }
        public string issueDate { set; get; }
        public string issuePlace { set; get; }
        public string employeeCard { set; get; }
        public string maritalStatus { set; get; }
        public string mobilePhone { set; get; }
        public string durationYear { set; get; }
        public string durationMonth { set; get; }
        public string map { set; get; }
        public string ownerNationalId { set; get; }
        public string contactAddress { set; get; }
        public string dsaCode { set; get; }
        public string companyName { set; get; }
        public string taxCode { set; get; }
        //Add - TuanNA89 - 02/07/2019 - Thêm thu nhập hàng tháng
        public string salary { set; get; }

        [XmlElement("loanDetail")]
        public TPFico_CustInfo_LoanDetail_GetAllData loanDetail { get; set; }
        [XmlArrayItem("address")]
        public List<TPFico_CustInfo_Address> addresses { set; get; }
        [XmlArrayItem("photo")]
        public List<TPFico_CustInfo_Photo> photos { set; get; }
        [XmlArrayItem("productDetail")]
        public List<TPFico_CustInfo_ProductDetail> productDetails { set; get; }
        [XmlArrayItem("reference")]
        public List<TPFico_CustInfo_Reference> references { set; get; }
    }
    public class TPFico_CustInfo_LoanDetail_GetAllData : TPFico_CustInfo_LoanDetail
    {
        string _emi = null;
        public string emi
        {
            get { return _emi ?? "0"; }
            set { _emi = value; }
        }
    }

    #endregion

    #endregion

    #region =======ENUM=======
    enum Status
    {
        PROCESSING = 1,
        APPROVED = 2,
        DISBURSED = 3,
        CANCELLED = 4,
        REJECTED = 5,
        RETURNED = 6,
        SUPPLEMENT = 7,
        WaitingDiburse = 8
    }
    enum MaritalStatus
    {
        Unmaried = 0,
        Maried = 1
    }
    #endregion

    #region =======LINK API=======
    public sealed class Api_TPBank
    {
        private Api_TPBank() { }
        // TuanNA89 - 06/08/2020 - Tối ưu lại 1 số Code liên quan đến TPBank
        /*
        public static readonly string GuiThongTinKH = "/api/fpt/customers_pgp";
        public static readonly string LayThongTinDangKyKH = "/api/fpt/customers_pgp/{0}/acca";
        public static readonly string GuiThongTinGiaiNgan = "/api/fpt/customers_pgp/{0}/doc_post_approved";
        public static readonly string GuiThongTinKH_BiLoi = "/api/fpt/customers_pgp/{0}/returned_for_data_pre_approved";
        public static readonly string GuiThongTinGiaiNgan_BiLoi = "/api/fpt/customers_pgp/{0}/supplement_for_document_post_approved";
        */
        // TuanNA89 - 19/06/2020 - TPBank đổi link Api
        public static readonly string LayToken = "/auth/oauth/token";
        public static readonly string GuiThongTinKH = "/fpt/customers_pgp";
        public static readonly string LayThongTinDangKyKH = "/fpt/customers_pgp/{0}/acca";
        public static readonly string GuiThongTinGiaiNgan = "/fpt/customers_pgp/{0}/doc_post_approved";
        public static readonly string GuiThongTinKH_BiLoi = "/fpt/customers_pgp/{0}/returned_for_data_pre_approved";
        public static readonly string GuiThongTinGiaiNgan_BiLoi = "/fpt/customers_pgp/{0}/supplement_for_document_post_approved";
    }

    #endregion
}