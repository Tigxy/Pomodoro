using Dapper;
using System;
using System.Data;

namespace Pomodoro.DB.Handlers
{
    public class TimeSpanHandler : SqlMapper.TypeHandler<TimeSpan>
    {
        public override TimeSpan Parse(object value)
        {
            return TimeSpan.FromSeconds(Convert.ToInt32(value));
        }

        public override void SetValue(IDbDataParameter parameter, TimeSpan value)
        {
            parameter.Value = value.TotalSeconds;
        }
    }
}