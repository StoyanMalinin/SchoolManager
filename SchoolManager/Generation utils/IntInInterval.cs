using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManager.Generation_utils
{
    class IntInInterval
    {
        public int l, r;

        public IntInInterval() { }
        public IntInInterval(int x)
        {
            this.l = x;
            this.r = x;
        }
        public IntInInterval(int l, int r)
        {
            this.l = l;
            this.r = r;
        }

        public static IntInInterval operator +(IntInInterval A, IntInInterval B)
        {
            return new IntInInterval(A.l + B.l, A.r + B.r);
        }
        public static IntInInterval operator -(IntInInterval A, IntInInterval B)
        {
            return new IntInInterval(A.l - B.r, A.r - B.l);
        }
    }
}
