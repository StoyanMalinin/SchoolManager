using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SchoolManager.School_Models
{
    class GroupUnion
    {
        public List<Group> groups;
        public List<int> groupSubject;
        public List<Teacher> teachers;

        public GroupUnion() { }
        public GroupUnion(List <Group> groups, List <int> groupSubject, List <Teacher> teachers)
        {
            this.groups = groups;
            this.groupSubject = groupSubject;
            this.teachers = teachers;
        }
    } 
}
