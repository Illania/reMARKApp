//
// File: OptionalParameters.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

namespace Mark5.Mobile.Common.Model
{
    public abstract class OptionalParameters : ICopiable<OptionalParameters>
    {
        #region ICopiable

        public virtual OptionalParameters DeepCopy()
        {
            return ShallowCopy();
        }

        public abstract OptionalParameters ShallowCopy();

        #endregion
    }

    public class CalendarEventOptionalParameters : OptionalParameters
    {
        public bool CanContainAppointments { get; set; }
        public bool CanContainTasks { get; set; }

        #region ICopiable

        public override OptionalParameters ShallowCopy()
        {
            return new CalendarEventOptionalParameters
            {
                CanContainAppointments = CanContainAppointments,
                CanContainTasks = CanContainTasks
            };
        }

        #endregion
    }
}