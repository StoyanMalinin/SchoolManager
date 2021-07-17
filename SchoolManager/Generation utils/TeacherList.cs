using SchoolManager.School_Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManager.Generation_utils
{
    class TeacherList : IEquatable<TeacherList>
    {
        public int id { get; set; }
        public bool isGood { get; set; }
        public List<Tuple<int, Subject>> l { get; set; }

        public TeacherList() { }
        public TeacherList(int id, List<Tuple<int, Subject>> l)
        {
            this.id = id;
            this.l = l;

            this.isGood = checkGood();
        }
        public TeacherList(int id, List<Tuple<int, Subject>> l, bool isGood)
        {
            this.isGood = isGood;
            this.id = id;
            this.l = l;
        }

        public override int GetHashCode()
        {
            long h = 0;
            long key = 1009, mod = (long)1e9 + 7;

            foreach (var x in l)
            {
                h = (h * key + x.Item1) % mod;
            }

            return (int)h;
        }

        public bool Equals(TeacherList other)
        {
            if (other.l.Count != l.Count) return false;
            for (int i = 0; i < l.Count; i++)
                if (l[i].Item1 != other.l[i].Item1)
                    return false;

            return true;
        }

        private bool checkGood()
        {
            Dictionary<int, int> mp = new Dictionary<int, int>();
            foreach (var x in l)
            {
                if (mp.ContainsKey(x.Item1) == false) mp.Add(x.Item1, 0);
                mp[x.Item1]++;
            }

            foreach (var item in mp)
            {
                int blocks = 0;
                for (int i = 0; i < l.Count;)
                {
                    if (l[i].Item1 != item.Key)
                    {
                        i++;
                        continue;
                    }

                    blocks++;
                    while (i < l.Count && l[i].Item1 == item.Key) i++;
                }

                if (item.Value <= 2 && blocks > 1) return false;
                if (blocks > 2) return false;
            }

            return true;
        }
    }
}
