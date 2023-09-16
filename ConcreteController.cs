using BusinessLayer;
using BusinessLayer.DAL;
using BusinessLayer.Entity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eSGBIZ.Controllers
{
    public class ConcreteController : BaseController
    {
        //
        // GET: /ConcreteSpecimens/

        public ActionResult ConcreteSpecimensEntry()
        {
            ViewBag.Header = "Concrete Specimens";
            Concrete_Specimen _Concrete = new Concrete_Specimen();


            List<GradeMaster> gradeList = new DAL_Common().GetGradeList();
            _Concrete.GRADE_LIST = new SelectList(gradeList, "Grade_Id", "Grade_Name");

            var empExceptionList = new List<string> { "CAL0229", "CAL0230" };
            List<EMPLOYEE_DETAILS> _empList = new DAL_Common().GetEmployee_List("", "", "", "", "", emp.Employee_Code, "").Where(x => x.activeFlag == "Y" && !empExceptionList.Contains(x.Employee_Code)).ToList<EMPLOYEE_DETAILS>();
            _Concrete.EMPLOYEE_LIST = new SelectList(_empList.OrderBy(x => x.EmployeeName), "Employee_Code", "EmployeeName");
            return View(_Concrete);
        }
        [HttpPost]
        [SubmitButtonSelector(Name = "Save")]
        [ActionName("ConcreteSpecimensEntry")]
        public ActionResult INSERT_CONCRETE_SPECIMEN_SAVE(Concrete_Specimen _CONSPECI)
        {
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();

            if (ModelState.IsValid)
            {
                try
                {
                    ResultMessage objMst;
                    string result = new DAL_CONCRETE_SPECIMEN().INSERT_CONCRETE_SPECIMEN(emp.Employee_Code, _CONSPECI, out objMst);

                    if (result == "")
                    {
                        Success(string.Format("<b>Concrete Specimen inserted successfully. Test NO : </b> <b style='color:red'> " + objMst.CODE + "</b>"), true);
                        return RedirectToAction("ConcreteSpecimensList", "Concrete");
                    }
                    else
                    {
                        Danger(string.Format("<b>Error:</b>" + result), true);
                    }
                }
                catch (Exception ex)
                {
                    Danger(string.Format("<b>Error:</b>" + ex.Message), true);
                }
            }
            else
            {
                Danger(string.Format("<b>Error:102 :</b>" + string.Join("; ", ModelState.Values.SelectMany(z => z.Errors).Select(z => z.ErrorMessage))), true);
            }

            List<GradeMaster> gradeList = new DAL_Common().GetGradeList();
            _CONSPECI.GRADE_LIST = new SelectList(gradeList, "Grade_Id", "Grade_Name");

            var empExceptionList = new List<string> { "CAL0229", "CAL0230" };
            List<EMPLOYEE_DETAILS> _empList = new DAL_Common().GetEmployee_List("", "", "", "", "", emp.Employee_Code, "").Where(x => x.activeFlag == "Y" && !empExceptionList.Contains(x.Employee_Code)).ToList<EMPLOYEE_DETAILS>();
            _CONSPECI.EMPLOYEE_LIST = new SelectList(_empList.OrderBy(x => x.EmployeeName), "Employee_Code", "EmployeeName");


            return View(_CONSPECI);
        }

        [Authorize]
        public ActionResult ConcreteSpecimensList()
        {
            ViewBag.Header = "Concrete Specimen List";
            Concrete_List _CONSPECI = new Concrete_List();
            //_CONSPECI.FLAG = "A";

            List<GradeMaster> gradeList = new DAL_Common().GetGradeList();
            _CONSPECI.GRADE_LIST = new SelectList(gradeList, "Grade_Id", "Grade_Name");
            
            return View(_CONSPECI);

        }
        public ActionResult _ConcreteSpecimensList()
        {
            return PartialView("_ConcreteSpecimensList");
        }
        [HttpPost]
        public ActionResult _ConcreteSpecimens_Data_List(string grade, DateTime fDate, DateTime tDate, string flag)
        {
            // Server Side Processing
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string searchValue = Request["search[value]"];
            string sortColumnName = Request["columns[" + Request["order[0][column]"] + "][name]"];
            string sortDirection = Request["order[0][dir]"];
            int totalRow = 0;

            Concrete_List _SPCON = new Concrete_List();
            List<CONCRETE_SPECIFICATION_DATALIST> CONSPData = new List<CONCRETE_SPECIFICATION_DATALIST>();
            try
            {

                _SPCON.From_DT = fDate;
                _SPCON.To_DT = tDate;
                _SPCON.GRADE_ID = grade;
                //_SPCON.FLAG = "A";


                CONSPData = new DAL_CONCRETE_SPECIMEN().Select_Concrete_Specimen_List(_SPCON);

                totalRow = CONSPData.Count();
                //TempData["UPDATE"] = "A";

            }
            catch (Exception ex)
            {
                Danger(string.Format("<b>Exception occured.</b>"), true);
            }

            if (!string.IsNullOrEmpty(searchValue)) // Filter Operation
            {
                CONSPData = CONSPData.
                    Where(x => x.Test_No.ToLower().Contains(searchValue.ToLower())
                    || x.Date_Test.ToLower().Contains(searchValue.ToLower()) ||

                        x.Wet_Bulb.ToLower().Contains(searchValue.ToLower()) ||
                        x.Dry_Bulb.ToLower().Contains(searchValue.ToLower()) || x.GRADE_NAME.ToLower().Contains(searchValue.ToLower())
                         ).ToList<CONCRETE_SPECIFICATION_DATALIST>();

            }
            int totalRowFilter = CONSPData.Count();

            if (length == -1)
            {
                length = totalRow;
            }
            CONSPData = CONSPData.Skip(start).Take(length).ToList<CONCRETE_SPECIFICATION_DATALIST>();

            var jsonResult = Json(new { data = CONSPData, draw = Request["draw"], recordsTotal = totalRow, recordsFiltered = totalRowFilter }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult ConcreteSpecimenEdit(int Test_ID)
        {
            ViewBag.Header = "Concrete Specimen Updation";
            
          
       
            CONCRETE_SPECIFICATION_EDIT _objSPGRA = new CONCRETE_SPECIFICATION_EDIT();
            _objSPGRA = new DAL_CONCRETE_SPECIMEN().Edit_Concrete_Specimens(Test_ID);

             
                List<GradeMaster> gradeList = new DAL_Common().GetGradeList();
                _objSPGRA.GRADE_LIST = new SelectList(gradeList, "Grade_Id", "Grade_Name");

               
            return View(_objSPGRA);
        }
     

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[4096];
            while (true)
            {
                int read = input.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                {
                    return;
                }
                output.Write(buffer, 0, read);
            }
        }

        public string ConvertViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (StringWriter writer = new StringWriter())
            {
                ViewEngineResult vResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext vContext = new ViewContext(this.ControllerContext, vResult.View, ViewData, new TempDataDictionary(), writer);
                vResult.View.Render(vContext, writer);
                return writer.ToString();
            }
        }

        public FileResult ShowDocument(string FilePath)
        {
            string DMS_Path = ConfigurationManager.AppSettings["DMSPATH"].ToString();
            string directoryPath = DMS_Path + "REPORT\\CONCRETE SPECIMEN\\" + FilePath;
            return File(directoryPath, GetMimeType(FilePath));
        }

        private string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }





        //[HttpPost]
        //[SubmitButtonSelector(Name = "Update")]
        //[ActionName("ConcreteSpecimenEdit")]
        //public ActionResult Update_CONCRETE_SPECIMEN_SAVE(CONCRETE_SPECIFICATION_EDIT _CONSPECI)
        //{


        //    DataTable dtCS = new DataTable();
        //    dtCS.Columns.Add("Test_Dtl_ID", typeof(string));
        //    dtCS.Columns.Add("Test_ID", typeof(decimal));
        //    dtCS.Columns.Add("Client", typeof(string));
        //    dtCS.Columns.Add("Id_Mark", typeof(decimal));
        //    dtCS.Columns.Add("Dimension_L", typeof(decimal));
        //    dtCS.Columns.Add("Dimension_B", typeof(decimal));
        //    dtCS.Columns.Add("Dimension_H", typeof(decimal));
        //    dtCS.Columns.Add("Cast_Date", typeof(DateTime));
        //    dtCS.Columns.Add("Test_Age", typeof(decimal));
        //    dtCS.Columns.Add("Grade_ID", typeof(decimal));
        //    dtCS.Columns.Add("Weight", typeof(decimal));
        //    dtCS.Columns.Add("Crushing_Load", typeof(decimal));
        //    dtCS.Columns.Add("Compressive_strength", typeof(decimal));
        //    dtCS.Columns.Add("Avg_Compressive", typeof(decimal));
        //    dtCS.Columns.Add("Remarks", typeof(string));
        //    dtCS.Columns.Add("STATUS", typeof(string));

        //    DataRow dr = null;

        //    foreach (Concrete_Specimen_Dtl item in _CONSPECI.Concrete_Specimen_Dtl_List)
        //    {
        //        //_CONSPECI.FLAG = "A";
                
        //        dr = dtCS.NewRow();

        //        dr["Test_Dtl_ID"] = Convert.ToDouble(item.Test_Dtl_ID);
        //        dr["Test_ID"] = Convert.ToDouble(_CONSPECI.Test_ID);
        //        dr["Client"] = Convert.ToString(item.Client);
        //        dr["Id_Mark"] = Convert.ToDecimal(item.Id_Mark);
        //        dr["Dimension_L"] = Convert.ToDecimal(item.Dimension_L);
        //        dr["Dimension_B"] = Convert.ToDecimal(item.Dimension_B);
        //        dr["Dimension_H"] = Convert.ToDecimal(item.Dimension_H);
        //        dr["Cast_Date"] = Convert.ToDateTime(item.Date_Of_Casting);
        //        dr["Test_Age"] = Convert.ToDecimal(item.Age_Test);
        //        dr["Grade_ID"] = Convert.ToDouble(item.GRADE_ID);
        //        dr["Weight"] = Convert.ToDecimal(item.Weight);
        //        dr["Crushing_Load"] = Convert.ToDecimal(item.Crushing_Load);
        //        dr["Compressive_strength"] = Convert.ToDecimal(item.Compressive_Strength);
        //        dr["Avg_Compressive"] = Convert.ToDecimal(item.AVG_Compressive_Strength);
        //        dr["Remarks"] = Convert.ToString(item.Remarks);
        //        dr["STATUS"] = Convert.ToString(item.Status);
        //        //dr["FLAG"] = Convert.ToString(item.FLAG);
        //        dtCS.Rows.Add(dr);

        //        ModelState["UploadFile"] = new ModelState();
        //        var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();

        //        if (ModelState.IsValid)
        //        {

        //            try
        //            {
        //                //for (int i = 0; i > dtCS.Rows.Count; i++)
        //                //    {
        //                if (Convert.ToInt32(item.Test_Dtl_ID) > 0 && dtCS.Rows.Count > 0)
        //                {
        //                    //if (i > dtCS.Rows.Count )
        //                    //{
        //                    ResultMessage objMst;
        //                    string result = new DAL_CONCRETE_SPECIMEN().UPDATE_CONCRETE_SPECIMEN(emp.Employee_Code, _CONSPECI, out objMst, dtCS);
                           
        //                    if (result == "")
        //                    {
        //                        Success(string.Format("<b>Concrete Specimen Updated successfully. Test NO : </b> <b style='color:red'> " + objMst.CODE + "</b>"), true);
        //                        return RedirectToAction("ConcreteSpecimenEdit", "Concrete");
        //                    }
        //                    else
        //                    {

        //                        Danger(string.Format("<b>Error:</b>" + result), true);
        //                    }
        //                    //}

        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Danger(string.Format("<b>Error:</b>" + ex.Message), true);
        //            }
        //        }
        //        else
        //        {
        //            Danger(string.Format("<b>Error:102 :</b>" + string.Join("; ", ModelState.Values.SelectMany(z => z.Errors).Select(z => z.ErrorMessage))), true);
        //        }
        //    }
        //    //}

        //    List<GradeMaster> gradeList = new DAL_Common().GetGradeList();
        //    _CONSPECI.GRADE_LIST = new SelectList(gradeList, "Grade_Id", "Grade_Name");


        //    var empExceptionList = new List<string> { "CAL0229", "CAL0230" };
        //    List<EMPLOYEE_DETAILS> _empList = new DAL_Common().GetEmployee_List("", "", "", "", "", emp.Employee_Code, "").Where(x => x.activeFlag == "Y" && !empExceptionList.Contains(x.Employee_Code)).ToList<EMPLOYEE_DETAILS>();
        //    _CONSPECI.EMPLOYEE_LIST = new SelectList(_empList.OrderBy(x => x.EmployeeName), "Employee_Code", "EmployeeName");


        //    return View(_CONSPECI);
        //}

        [HttpPost]
        [SubmitButtonSelector(Name = "Update")]
        [ActionName("ConcreteSpecimenEdit")]
        public ActionResult Update_CONCRETE_SPECIMEN_SAVE(CONCRETE_SPECIFICATION_EDIT _CONSPECI)
        {

            ModelState["UploadFile"] = new ModelState();
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();

            if (ModelState.IsValid)
            {

                try
                {
                    ResultMessage objMst;
                    string result = new DAL_CONCRETE_SPECIMEN().UPDATE_CONCRETE_SPECIMEN(emp.Employee_Code, _CONSPECI, out objMst);

                    if (result == "")
                    {
                        Success(string.Format("<b>Concrete Specimen Updated successfully. Test NO : </b> <b style='color:red'> " + objMst.CODE + "</b>"), true);
                        return RedirectToAction("ConcreteSpecimenEdit", "Concrete");
                    }
                    else
                    {

                        Danger(string.Format("<b>Error:</b>" + result), true);
                    }
                    //}


                }
                catch (Exception ex)
                {
                    Danger(string.Format("<b>Error:</b>" + ex.Message), true);
                }
            }
            else
            {
                Danger(string.Format("<b>Error:102 :</b>" + string.Join("; ", ModelState.Values.SelectMany(z => z.Errors).Select(z => z.ErrorMessage))), true);
            }

            //}

            List<GradeMaster> gradeList = new DAL_Common().GetGradeList();
            _CONSPECI.GRADE_LIST = new SelectList(gradeList, "Grade_Id", "Grade_Name");


            var empExceptionList = new List<string> { "CAL0229", "CAL0230" };
            List<EMPLOYEE_DETAILS> _empList = new DAL_Common().GetEmployee_List("", "", "", "", "", emp.Employee_Code, "").Where(x => x.activeFlag == "Y" && !empExceptionList.Contains(x.Employee_Code)).ToList<EMPLOYEE_DETAILS>();
            _CONSPECI.EMPLOYEE_LIST = new SelectList(_empList.OrderBy(x => x.EmployeeName), "Employee_Code", "EmployeeName");


            return View(_CONSPECI);
        }



        public ActionResult ConcreteSpecimenView(decimal Test_ID)
        {
            Concrete_Specimen _objCONCSP = new Concrete_Specimen();
            _objCONCSP = new DAL_CONCRETE_SPECIMEN().View_Concrete_Specimens(Test_ID);
            _objCONCSP.FILE_PATH = _objCONCSP.FILE_PATH;
            _objCONCSP.IS_FILE_UPLOAD = _objCONCSP.IS_FILE_UPLOAD;
            return PartialView("ConcreteSpecimenView", _objCONCSP);
        }

    }
}
