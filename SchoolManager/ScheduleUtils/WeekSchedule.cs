using System;
using System.Collections.Generic;
using System.Text;
using OfficeOpenXml;
using System.IO;
using System.Reflection;
using System.Drawing;

namespace SchoolManager.ScheduleUtils
{
    class WeekSchedule
    {
        private int workDays;
        public DaySchedule[] days;

        public WeekSchedule() { }
        public WeekSchedule(int workDays)
        {
            this.workDays = workDays;
            this.days = new DaySchedule[workDays+1];
        }
    
        public void exportToExcell(string filename)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @$"{filename}.xlsx");
            ExcelPackage excel = new ExcelPackage(new FileInfo(path));

            if(File.Exists(path)==false)
            {
                excel.Workbook.Worksheets.Add("sheet1");
                excel.SaveAs(new FileInfo(path));    
            }
            else
            {
                excel.Workbook.Worksheets.Delete("sheet1");
                excel.Workbook.Worksheets.Add("sheet1");
            }

            excel.Workbook.Worksheets["sheet1"].Column(1).Width = 2;
            for(int t = 0;t<days[1].teachers.Count;t++)
            {
                excel.Workbook.Worksheets["sheet1"].Column(2+t).Width = 4;

                excel.Workbook.Worksheets["sheet1"].Cells[1, 2+t].Value = days[1].teachers[t].name;
                excel.Workbook.Worksheets["sheet1"].Cells[1, 2+t].Style.TextRotation = 180;
            }

            for(int day = 1;day<=workDays;day++)
            {
                for(int lesson = 1;lesson<=days[day].maxLessons;lesson++) 
                {
                    excel.Workbook.Worksheets["sheet1"].Cells[2+(day-1)*(days[day].maxLessons+1)+lesson-1, 1].Value = lesson;
                
                    Color colFromHex = Color.FromArgb(0, 255, 0);
                    excel.Workbook.Worksheets["sheet1"].Cells[2+(day-1)*(days[day].maxLessons+1)+lesson-1, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    excel.Workbook.Worksheets["sheet1"].Cells[2+(day-1)*(days[day].maxLessons+1)+lesson-1, 1].Style.Fill.BackgroundColor.SetColor(colFromHex);
                }   

                for(int lesson = 1;lesson<=days[day].maxLessons;lesson++)
                {
                    for(int t = 0;t<days[day].teachers.Count;t++)
                    {
                        object val = null;
                        if(days[day].lessonTeacher2Group[lesson, t]!=null) val = days[day].lessonTeacher2Group[lesson, t].name;
                        else if(days[day].lessonTeacher2SuperGroup[lesson, t]!=null) val = days[day].lessonTeacher2SuperGroup[lesson, t].name;

                        excel.Workbook.Worksheets["sheet1"].Cells[2+(day-1)*(days[day].maxLessons+1)+lesson-1, 2+t].Value = val;
                    }
                }
            }

            excel.Save();
        }

        public void print()
        {
            for(int day = 1;day<=days.Length-1;day++)
                days[day].print();
        }
    }
}
