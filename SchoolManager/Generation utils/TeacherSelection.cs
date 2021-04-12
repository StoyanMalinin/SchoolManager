using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManager.Generation_utils
{
    class TeacherSelection
    {
        public bool[] isSelected;
        public HashSet<long> failedStates;

        public Dictionary <long, HashSet <long>> branchesLeft;

        public TeacherSelection(int allTeachersCnt, List <int> selectedTeacherInds) 
        {
            this.failedStates = new HashSet<long>();
            this.isSelected = new bool[allTeachersCnt];
            this.branchesLeft = new Dictionary<long, HashSet<long>>();
            for (int i = 0; i < this.isSelected.Length; i++) isSelected[i] = false;

            foreach (int ind in selectedTeacherInds)
                isSelected[ind] = true;
        }
    }
}
