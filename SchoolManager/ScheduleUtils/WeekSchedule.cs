using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManager.ScheduleUtils
{
    class WeekSchedule
    {
        public DaySchedule[] days;

        public WeekSchedule() { }
        public WeekSchedule(int workDays)
        {
            this.days = new DaySchedule[workDays+1];
        }
    }
}
