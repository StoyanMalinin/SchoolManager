using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using SchoolManager.School_Models;
using SchoolManager.School_Models.Higharchy;

namespace SchoolManager
{
    static class PerformanceTestPMGHaskovo
    {
        private static int workDays = 5;
        private static int groupsCnt = 28;

        class SuperGroupExcell : IEquatable<SuperGroupExcell>
        {
            public List<string> teachers;
            public List<Tuple<string, string>> groups;
            public List<Tuple<int, int, int>> positions;

            public SuperGroupExcell() { }
            public SuperGroupExcell(List <string> teachers, List <Tuple<string, string>> groups, List <Tuple <int, int, int>> positions)
            {
                this.positions = positions;
                this.teachers = teachers;
                this.groups = groups;
            }
            public SuperGroupExcell(string group, string subject, List <string> teachers, Tuple <int, int, int> pos)
            {
                this.teachers = teachers;
                this.positions = new List<Tuple<int, int, int>>() { pos };
                this.groups = new List<Tuple<string, string>>() { Tuple.Create(group, subject) };
            }

            public bool isIntersecting(SuperGroupExcell other)
            {
                if (groups.Any(g => other.groups.Any(x => x.Item1.Equals(g.Item1)==true) == true) == true) return true;
                if (teachers.Any(t => other.teachers.Contains(t) == true) == true) return true;

                return false;
            }

            public bool isSuper()
            {
                return (teachers.Count > 1 || groups.Count > 1);
            }

            public override string ToString()
            {
                return string.Join("|", groups) + " ## " + string.Join("|", teachers);
            }

            public SuperGroupExcell Clone()
            {
                return new SuperGroupExcell(teachers.Select(x => x.Clone().ToString()).ToList(), groups.Select(x => Tuple.Create(x.Item1, x.Item2)).ToList(),
                                                            positions.Select(t => Tuple.Create(t.Item1, t.Item2, t.Item3)).ToList());
            }

            public bool Equals([AllowNull] SuperGroupExcell other)
            {
                return this.ToString().Equals(other.ToString());
            }

            public override int GetHashCode()
            {
                return this.ToString().GetHashCode();
            }
        }

        private static SuperGroupExcell merge(SuperGroupExcell A, SuperGroupExcell B)
        {
            return new SuperGroupExcell(A.teachers.Concat(B.teachers).Distinct().ToList(),
                                        A.groups.Concat(B.groups).Distinct().ToList(),
                                        A.positions.Concat(B.positions).Distinct().ToList());
        }

        private static List<SuperGroupExcell> getSuperGroups(string filename)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @$"{filename}.xlsm");

            using (var package = new ExcelPackage(new FileInfo(path)))
            {
                var sheetTeacher = package.Workbook.Worksheets["По класове"];
                var sheetSubject = package.Workbook.Worksheets["ХЕИ"];

                Dictionary<SuperGroupExcell, List<SuperGroupExcell>> superGroups = new Dictionary<SuperGroupExcell, List<SuperGroupExcell>>();
                for (int l = 1; l <= 7; l++)
                {
                    for (int day = 1; day <= workDays; day++)
                    {
                        List<SuperGroupExcell> currSuperGroups = new List<SuperGroupExcell>();
                        for (int g = 0; g < groupsCnt; g++)
                        {
                            string subject = sheetSubject.Cells[20 + g * 9 + l - 1, 3 + day - 1].Text;
                            List<string> teachers = sheetTeacher.Cells[2 + g * 9 + l, 3 * day].Text.Split('/').ToList();

                            if (subject == "") continue;
                            if(subject=="0") continue;
                            if(subject=="ИТ") subject = "ИТ/ИТ";

                            SuperGroupExcell sg = new SuperGroupExcell(sheetTeacher.Cells[2 + g * 9, 1].Text, subject, teachers, Tuple.Create(g, l, day));
                            int ind = currSuperGroups.FindIndex(x => x.isIntersecting(sg) == true);

                            if (ind == -1) currSuperGroups.Add(sg);
                            else currSuperGroups[ind] = merge(currSuperGroups[ind], sg);
                        }

                        foreach(var x in currSuperGroups.Where(x => x.isSuper() == true))
                        {
                            if (superGroups.ContainsKey(x) == false) superGroups.Add(x, new List<SuperGroupExcell>());
                            superGroups[x].Add(x);
                        }
                    }
                }

                List<SuperGroupExcell> output = new List<SuperGroupExcell>();
                foreach(var item in superGroups)
                {
                    SuperGroupExcell x = item.Value[0];
                    for (int i = 1; i < item.Value.Count; i++) x = merge(x, item.Value[i]);

                    output.Add(x);
                }

                return output;
            }
        }

        private static List<string> getSubjects(string filename)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @$"{filename}.xlsm");

            List<string> subjects = new List<string>();
            using (var package = new ExcelPackage(new FileInfo(path)))
            {
                var sheetSubject = package.Workbook.Worksheets["ХЕИ"];

                for (int l = 1; l <= 7; l++)
                {
                    for (int day = 1; day <= workDays; day++)
                    {
                        for (int g = 0; g < groupsCnt; g++)
                        {
                            subjects.Add(sheetSubject.Cells[20 + g * 9 + l - 1, 3 + day - 1].Text);
                        }
                    }
                }
            }

            return subjects.Where(x => x!="" && x!="0" && x!="ИТ").Distinct().ToList();
        }

        private static List <string> getTeachers(string filename)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @$"{filename}.xlsm");

            List<string> teachers = new List<string>();
            using (var package = new ExcelPackage(new FileInfo(path)))
            {
                var sheetTeacher = package.Workbook.Worksheets["По класове"];

                for (int l = 1; l <= 7; l++)
                {
                    for (int day = 1; day <= workDays; day++)
                    {
                        for (int g = 0; g < groupsCnt; g++)
                        {
                            sheetTeacher.Cells[2 + g * 9 + l, 3 * day].Text.Split('/').ToList().ForEach(t => teachers.Add(t));
                        }
                    }
                }
            }

            return teachers.Distinct().ToList();
        }

        private static List <Group> getGroups(string filename, List <SuperGroupExcell> superGroupExcells, 
                                              List <Subject> subjects, List <Teacher> teachers,
                                              List <LimitationGroup> limitationGroups, List <string> subjectNames)
        {
            List<Group> groups = new List<Group>();

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @$"{filename}.xlsm");

            using (var package = new ExcelPackage(new FileInfo(path)))
            {
                var sheetTeacher = package.Workbook.Worksheets["По класове"];
                var sheetSubject = package.Workbook.Worksheets["ХЕИ"];

                for (int g = 0; g < groupsCnt; g++)
                {
                    string gName = sheetTeacher.Cells[2 + g * 9, 1].Text;

                    List<string> allSubjects = new List<string>();
                    List<Tuple<string, string>> subject2TeacherName = new List<Tuple<string, string>>();
                    
                    System.Console.WriteLine($"Group {gName}");
                    for (int l = 1; l <= 7; l++)
                    {
                        for (int day = 1; day <= workDays; day++)
                        {
                            string subject = sheetSubject.Cells[20 + g * 9 + l - 1, 3 + day - 1].Text;
                            List<string> teacherNames = sheetTeacher.Cells[2 + g * 9 + l, 3 * day].Text.Split('/').ToList();
                            
                            //System.Console.WriteLine($"lesson {l} day {day} => {subject}");

                            if (subject == "") continue;
                            if(subject=="0") continue;
                            if(subject=="ИТ") subject = "ИТ/ИТ";

                            allSubjects.Add(subject);

                            if (superGroupExcells.Any(x => x.positions.Any(p => p.Item1 == g && p.Item2 == l && p.Item3 == day) == true) == true)
                            {
                                continue;
                            }

                            string tName = teacherNames[0];
                            if(tName=="Петров ФВС") tName = "Петров";

                            subject2TeacherName.Add(Tuple.Create(subject, tName));

                        }
                    }

                    var subject2Teacher = subjects.Select(s => Tuple.Create(s,
                                                          ((subject2TeacherName.Any(x => x.Item1 == s.name) == false) ? null 
                                                          : teachers.First(t => t.name== subject2TeacherName.First(x => x.Item1 == s.name).Item2)))).ToList();

                    //var weekLims = limitationGroups.Select(lg => Tuple.Create(lg, allSubjects.Count(x => x==lg.name))).ToList();
                    System.Console.WriteLine(string.Join("|", allSubjects));
                    List <Tuple <LimitationGroup, int>> weekLims = new List<Tuple<LimitationGroup, int>>();
                    foreach(LimitationGroup lg in limitationGroups)
                    {
                        if(subjectNames.Any(s => s==lg.name)==false) 
                        {
                            System.Console.WriteLine($"am ai sq de {lg.name}");
                            weekLims.Add(Tuple.Create(lg, 6969));
                        }
                        else weekLims.Add(Tuple.Create(lg, allSubjects.Count(x => x==lg.name)));
                    }
                    
                    List <Tuple<LimitationGroup, int>> dayLims;
                    if(gName.Length<=3 && gName[0]!='8' && gName.Last()!='д') 
                    {
                        dayLims = dayLimsGeneral;
                        System.Console.WriteLine($"{gName} sa generali");
                    }
                    else if(gName.Length<=3 && gName.Last()=='д' && gName[0]!='8')
                    {
                        dayLims = dayLimsBiology;
                        System.Console.WriteLine($"{gName} sa biolozi");
                    }
                    else if(gName[0]=='8')
                    {
                        dayLims = dayLims8Grade;
                        System.Console.WriteLine($"{gName} sa osmi");
                    }
                    else
                    {
                        dayLims = dayLimsClashers;
                        System.Console.WriteLine($"{gName} sa clashari");
                    }
                    //else dayLims = weekLims.Select(t => Tuple.Create(t.Item1, Math.Min(t.Item2, 5))).ToList();

                    //Console.WriteLine($"{gName} -> {string.Join(", ", subject2TeacherName)}");
                   // Console.WriteLine(string.Join("\n", weekLims.Select(t => $"{t.Item1.name} -> {t.Item2}")));
                    //subject2Teacher.ForEach(x => Console.WriteLine($"{x.Item1.name} - {((x.Item2 is null)?"null":x.Item2.name)}"));
                    //Console.WriteLine();

                    groups.Add(new Group(6, gName, dayLims, weekLims, subject2Teacher));
                }
            }

            return groups;
        }

        private static List<SuperGroup> superGroups;
        private static List<Teacher> teachers;
        private static List<LimitationGroup> limitationGroups;
        private static List<Subject> subjects;
        private static List<Group> groups;
        private static LimitationTreeNode higharchy = new LimitationTreeNode("root", null);

        private static List <Tuple <LimitationGroup, int>> dayLimsGeneral, dayLimsBiology, dayLims8Grade, dayLimsClashers;

        private static void loadDayLimsGeneral()
        {
            dayLimsGeneral = new List<Tuple<LimitationGroup, int>>();
            dayLimsGeneral.Add(Tuple.Create(new LimitationGroup("to4ni"), 4));
            dayLimsGeneral.Add(Tuple.Create(new LimitationGroup("razkazvatelni"), 3));
            dayLimsGeneral.Add(Tuple.Create(new LimitationGroup("ezici"), 4));
            dayLimsGeneral.Add(Tuple.Create(new LimitationGroup("математика"), 2));
            dayLimsGeneral.Add(Tuple.Create(new LimitationGroup("бълг. език"), 2));
            dayLimsGeneral.Add(Tuple.Create(new LimitationGroup("информатика"), 2));
            dayLimsGeneral.Add(Tuple.Create(new LimitationGroup("II - ри чужд език"), 2));

            foreach(Subject s in subjects.Where(x => !(x is null)).Where(x => dayLimsGeneral.Any(t => t.Item1.name==x.name)==false))
                dayLimsGeneral.Add(Tuple.Create(new LimitationGroup(s.name), 2));
        }

        private static void loadDayLimsBiology()
        {
            dayLimsBiology = new List<Tuple<LimitationGroup, int>>();
            dayLimsBiology.Add(Tuple.Create(new LimitationGroup("to4ni"), 3));
            dayLimsBiology.Add(Tuple.Create(new LimitationGroup("razkazvatelni"), 4));
            dayLimsBiology.Add(Tuple.Create(new LimitationGroup("ezici"), 4));
            dayLimsBiology.Add(Tuple.Create(new LimitationGroup("математика"), 2));
            dayLimsBiology.Add(Tuple.Create(new LimitationGroup("бълг. език"), 2));
            dayLimsBiology.Add(Tuple.Create(new LimitationGroup("информатика"), 2));
            dayLimsBiology.Add(Tuple.Create(new LimitationGroup("II - ри чужд език"), 2));

            foreach(Subject s in subjects.Where(x => !(x is null)).Where(x => dayLimsBiology.Any(t => t.Item1.name==x.name)==false))
                dayLimsBiology.Add(Tuple.Create(new LimitationGroup(s.name), 2));
        }

        private static void loadDayLims8Grade()
        {
            dayLims8Grade = new List<Tuple<LimitationGroup, int>>();
            dayLims8Grade.Add(Tuple.Create(new LimitationGroup("to4ni"), 2));
            dayLims8Grade.Add(Tuple.Create(new LimitationGroup("razkazvatelni"), 4));
            dayLims8Grade.Add(Tuple.Create(new LimitationGroup("ezici"), 4));
            dayLims8Grade.Add(Tuple.Create(new LimitationGroup("математика"), 2));
            dayLims8Grade.Add(Tuple.Create(new LimitationGroup("бълг. език"), 2));
            dayLims8Grade.Add(Tuple.Create(new LimitationGroup("информатика"), 2));
            dayLims8Grade.Add(Tuple.Create(new LimitationGroup("II - ри чужд език"), 2));
            dayLims8Grade.Add(Tuple.Create(new LimitationGroup("английски език"), 4));

            foreach(Subject s in subjects.Where(x => !(x is null)).Where(x => dayLims8Grade.Any(t => t.Item1.name==x.name)==false))
                dayLims8Grade.Add(Tuple.Create(new LimitationGroup(s.name), 2));
        }

        private static void loadDayLimsClashers()
        {
            dayLimsClashers = new List<Tuple<LimitationGroup, int>>();
            dayLimsClashers.Add(Tuple.Create(new LimitationGroup("to4ni"), 2));
            dayLimsClashers.Add(Tuple.Create(new LimitationGroup("razkazvatelni"), 2));
            dayLimsClashers.Add(Tuple.Create(new LimitationGroup("ezici"), 4));
            dayLimsClashers.Add(Tuple.Create(new LimitationGroup("математика"), 2));
            dayLimsClashers.Add(Tuple.Create(new LimitationGroup("бълг. език"), 2));
            dayLimsClashers.Add(Tuple.Create(new LimitationGroup("информатика"), 2));
            dayLimsClashers.Add(Tuple.Create(new LimitationGroup("II - ри чужд език"), 2));
            dayLimsClashers.Add(Tuple.Create(new LimitationGroup("английски език"), 4));

            foreach(Subject s in subjects.Where(x => !(x is null)).Where(x => dayLimsClashers.Any(t => t.Item1.name==x.name)==false))
                dayLimsClashers.Add(Tuple.Create(new LimitationGroup(s.name), 2));
        }

        private static void loadDayLims()
        {
            loadDayLimsGeneral();
            loadDayLimsBiology();
            loadDayLims8Grade();
            loadDayLimsClashers();
        }

        private static void loadHigharcy()
        {
            limitationGroups.Where(lg => subjects.Any(s => s.name==lg.name)==false).ToList().ForEach(lg => higharchy.addChild(new LimitationTreeNode(lg)));
            
            Dictionary <string, string> subject2LimGroup = new Dictionary<string, string>();
            subject2LimGroup.Add("математика", "tochni");
            subject2LimGroup.Add("география", "razkazvatelni");
            subject2LimGroup.Add("история", "razkazvatelni");
            subject2LimGroup.Add("физика", "razkazvatelni");
            subject2LimGroup.Add("биология", "razkazvatelni");
            subject2LimGroup.Add("химия", "razkazvatelni");
            subject2LimGroup.Add("английски език", "ezici");

            foreach(Subject s in subjects)
            {
                TreeNode dfs(TreeNode x, string name)
                {
                    if(x.GetType()==typeof(LimitationTreeNode) && (x as LimitationTreeNode).name==name) return x;

                    TreeNode res = null;
                    foreach(TreeNode y in x.children)
                    {
                        TreeNode curr = dfs(y, name);
                        if(!(curr is null)) 
                        {
                            res = curr;
                            break;
                        }
                    }

                    return res;
                }

                TreeNode node = null;
                if(subject2LimGroup.ContainsKey(s.name)==true) node = dfs(higharchy, subject2LimGroup[s.name]);

                if(node==null) node = higharchy;
                else s.limGroups.Add((node as LimitationTreeNode).lg);

                System.Console.WriteLine($"{s.name} --> {node.name}");
                (node as LimitationTreeNode).addChild(new SubjectTreeNode(s));
            }
        }

        private static void loadSchedule(string filename)
        {
            List<SuperGroupExcell> excellSuperGroups = getSuperGroups(filename).ToList();   
            Console.WriteLine($"superGroupsCnt = {excellSuperGroups.Count}");
            //Console.WriteLine(string.Join("\n", excellSuperGroups));
            for(int i = 0;i<excellSuperGroups.Count;i++) System.Console.WriteLine($"{i} -> {excellSuperGroups[i].ToString()}");
            Console.WriteLine();

            List<string> subjectNames = getSubjects(filename);
            Console.WriteLine($"subjectsCnt = {subjectNames.Count}");
            Console.WriteLine(string.Join("\n", subjectNames));
            Console.WriteLine();

            List<string> teacherNames = getTeachers(filename);
            Console.WriteLine($"teachersCnt = {teacherNames.Count}");
            Console.WriteLine(string.Join(", ", teacherNames));
            Console.WriteLine();

            limitationGroups = subjectNames.Select(s => new LimitationGroup(s)).ToList();
            limitationGroups.Add(new LimitationGroup("tochni"));
            limitationGroups.Add(new LimitationGroup("razkazvatelni"));
            limitationGroups.Add(new LimitationGroup("ezici"));

            teachers = teacherNames.Select(t => new Teacher(t, new List<Subject>() { })).ToList();
            subjects = subjectNames.Select(s => new Subject(s, new List<LimitationGroup>() { limitationGroups.First(lg => lg.name==s)})).ToList();
            loadHigharcy();
            loadDayLims();
            
            groups = getGroups(filename, excellSuperGroups, subjects, teachers, limitationGroups, subjectNames);
            foreach(Group g in groups)
            {
                g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, g.subject2Teacher.First(t => t.Item1.name=="бълг. език").Item2, 
                                                                               g.subject2Teacher.First(t => t.Item1.name=="бълг. език").Item1, 
                                                                               new Generation_utils.IntInInterval(2, 2)));

                if(g.name.Last()!='д')
                { 
                     g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, g.subject2Teacher.First(t => t.Item1.name=="математика").Item2, 
                                                                               g.subject2Teacher.First(t => t.Item1.name=="математика").Item1, 
                                                                               new Generation_utils.IntInInterval(2, 2)));
                }

                if(g.name.Length<=3 && g.name[0]!='8')
                {
                    g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, g.subject2Teacher.First(t => t.Item1.name=="английски език").Item2, 
                                                                               g.subject2Teacher.First(t => t.Item1.name=="английски език").Item1, 
                                                                               new Generation_utils.IntInInterval(2, 2)));
                }
            }

            superGroups = new List<SuperGroup>();
            for(int i = 0;i<excellSuperGroups.Count;i++)
            {
                List <int> multilessons = new List<int>();

                System.Console.WriteLine($"---------- {excellSuperGroups[i].groups[0].Item2}");
                if(excellSuperGroups[i].groups[0].Item2=="II - ри чужд език") 
                {
                    System.Console.WriteLine("ima go");
                    multilessons.Add(2);
                }

                SuperGroup sg = new SuperGroup(i.ToString(), excellSuperGroups[i].groups.Select(t => Tuple.Create(groups.First(g => g.name == t.Item1), subjects.First(s => s.name == t.Item2))).ToList(),
                                               excellSuperGroups[i].teachers.Select(t => teachers.First(x => x.name==t)).ToList(), excellSuperGroups[i].positions.Count/excellSuperGroups[i].groups.Count, multilessons);

                superGroups.Add(sg);
                //Console.WriteLine($"{string.Join(", ", excellSuperGroups[i].groups)} -> {sg.weekLessons}");
            }
        }

        public static void test(string filename)
        {
            loadSchedule(filename);

            List<Generation_utils.Multilesson>[] multilessons = new List<Generation_utils.Multilesson>[5 + 1];
            for (int day = 1; day <= 5; day++)
                multilessons[day] = new List<Generation_utils.Multilesson>();

            Generation_utils.ScheduleGenerator4 sg = new Generation_utils.ScheduleGenerator4(groups, teachers, subjects,
                                                                                             higharchy, multilessons, superGroups);

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            sw.Start();
            SchoolManager.ScheduleUtils.WeekSchedule res = sg.gen();
            
            if(!(res is null))
            {
                res.print();
                res.exportToExcell($"{filename}_myVersion");
            }


            System.Console.WriteLine($"Total ellapsed time = {sw.ElapsedMilliseconds}");
            sw.Stop();
        }
    }
}
