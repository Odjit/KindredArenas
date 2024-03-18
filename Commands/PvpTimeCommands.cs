using KindredArenas.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VampireCommandFramework;

namespace KindredArenas.Commands
{
    [CommandGroup("pvptime")]
    internal class PvpTimeCommands
    {
        [Command("add", description: "Adds a PvP time", adminOnly: true)]
        public static void AddPvpTime(ChatCommandContext ctx, string daysOfTheWeek, string startTime, string endTime)
        {
            // Convert string of days of the weeks letters to enum
            var pvpTime = new PvpService.PvpTime();
            if (daysOfTheWeek.Contains("Su"))
            {
                pvpTime.DaysOfTheWeek |= DaysOfTheWeek.Sunday;
                daysOfTheWeek = daysOfTheWeek.Replace("Su", "");
            }
            if (daysOfTheWeek.Contains("M"))
            {
                pvpTime.DaysOfTheWeek |= DaysOfTheWeek.Monday;
                daysOfTheWeek = daysOfTheWeek.Replace("M", "");
            }
            if (daysOfTheWeek.Contains("Tu"))
            {
                pvpTime.DaysOfTheWeek |= DaysOfTheWeek.Tuesday;
                daysOfTheWeek = daysOfTheWeek.Replace("Tu", "");
            }
            if (daysOfTheWeek.Contains("W"))
            {
                pvpTime.DaysOfTheWeek |= DaysOfTheWeek.Wednesday;
                daysOfTheWeek = daysOfTheWeek.Replace("W", "");
            }
            if (daysOfTheWeek.Contains("Th"))
            {
                pvpTime.DaysOfTheWeek |= DaysOfTheWeek.Thursday;
                daysOfTheWeek = daysOfTheWeek.Replace("Th", "");
            }
            if (daysOfTheWeek.Contains("F"))
            {
                pvpTime.DaysOfTheWeek |= DaysOfTheWeek.Friday;
                daysOfTheWeek = daysOfTheWeek.Replace("F", "");
            }
            if (daysOfTheWeek.Contains("Sa"))
            {
                pvpTime.DaysOfTheWeek |= DaysOfTheWeek.Saturday;
                daysOfTheWeek = daysOfTheWeek.Replace("Sa", "");
            }

            if (daysOfTheWeek.Length > 0)
            {
                ctx.Reply($"Invalid '{daysOfTheWeek}' days of the week!  Should only contain the possible days 'SuMTuWThFSa'");
                return;
            }

            // Convert string of start time to hours and minutes
            var startSplit = startTime.Split(':');
            if (startSplit.Length != 2 && startSplit[0].Length >= 1 && startSplit[0].Length <= 2 && startSplit[1].Length == 4)
            {
                ctx.Reply($"Invalid start time '{startTime}'!  Should be in the format 'HH:MMam' or 'HH:MMpm'");
                return;
            }
            // Get the am/pm
            var hours = startSplit[0];
            var ampm = startSplit[1].Substring(2);
            var minutes = startSplit[1].Substring(0, 2);

            // Handle leading zeros if they exist
            if (hours[0] == '0')
            {
                hours = hours.Substring(1);
            }
            if (minutes[0] == '0')
            {
                minutes = minutes.Substring(1);
            }

            if ((ampm != "am" && ampm != "pm") ||
                !int.TryParse(hours, out var StartHour) ||
                !int.TryParse(minutes, out var StartMinute) ||
                StartHour < 1 || StartHour > 12 ||
                StartMinute < 0 || StartMinute > 59)
            {
                ctx.Reply($"Invalid start time '{startTime}'!  Should be in the format 'HH:MMam' or 'HH:MMpm'");
                return;
            }

            if(StartHour == 12)
                StartHour = 0;

            pvpTime.StartHour = StartHour + (ampm == "pm" ? 12 : 0);
            pvpTime.StartMinute = StartMinute;

            // Convert string of end time to hours and minutes
            var endSplit = endTime.Split(':');
            if (endSplit.Length != 2 && endSplit[0].Length >= 1 && endSplit[0].Length <= 2 && endSplit[1].Length == 4)
            {
                ctx.Reply($"Invalid end time '{endTime}'!  Should be in the format 'HH:MMam' or 'HH:MMpm'");
                return;
            }
            // Get the am/pm
            hours = endSplit[0];
            ampm = endSplit[1].Substring(2);
            minutes = endSplit[1].Substring(0, 2);

            // Handle leading zeros if they exist
            if (hours[0] == '0')
            {
                hours = hours.Substring(1);
            }
            if (minutes[0] == '0')
            {
                minutes = minutes.Substring(1);
            }

            if ((ampm != "am" && ampm != "pm") ||
                !int.TryParse(hours, out var EndHour) ||
                !int.TryParse(minutes, out var EndMinute) ||
                EndHour < 1 || EndHour > 12 ||
                EndMinute < 0 || EndMinute > 59)
            {
                ctx.Reply($"Invalid end time '{endTime}'!  Should be in the format 'HH:MMam' or 'HH:MMpm'");
                return;
            }

            if(EndHour == 12)
                EndHour = 0;

            pvpTime.EndHour = EndHour + (ampm == "pm" ? 12 : 0);
            pvpTime.EndMinute = EndMinute;

            Core.PvpService.AddPvpTime(pvpTime);
            ctx.Reply($"PvP time added");
        }

        [Command("list", description: "Lists all PvP times")]
        public static void ListPvpTimes(ChatCommandContext ctx)
        {
            var sb = new StringBuilder();
            sb.AppendLine("PvP Times");
            int count = 0;
            foreach (var pvpTime in Core.PvpService.GetPvpTimes())
            {
                // Convert the enum to a string of days of the week
                var dayString = "";
                if (pvpTime.DaysOfTheWeek.HasFlag(DaysOfTheWeek.Sunday)) dayString += "Su";
                if (pvpTime.DaysOfTheWeek.HasFlag(DaysOfTheWeek.Monday)) dayString += "M";
                if (pvpTime.DaysOfTheWeek.HasFlag(DaysOfTheWeek.Tuesday)) dayString += "Tu";
                if (pvpTime.DaysOfTheWeek.HasFlag(DaysOfTheWeek.Wednesday)) dayString += "W";
                if (pvpTime.DaysOfTheWeek.HasFlag(DaysOfTheWeek.Thursday)) dayString += "Th";
                if (pvpTime.DaysOfTheWeek.HasFlag(DaysOfTheWeek.Friday)) dayString += "F";
                if (pvpTime.DaysOfTheWeek.HasFlag(DaysOfTheWeek.Saturday)) dayString += "Sa";

                var startHour = pvpTime.StartHour > 12 ? pvpTime.StartHour - 12 : pvpTime.StartHour;
                if (startHour == 0) startHour = 12;
                var startMinute = pvpTime.StartMinute < 10 ? $"0{pvpTime.StartMinute}" : pvpTime.StartMinute.ToString();
                var startAmPm = pvpTime.StartHour > 12 ? "pm" : "am";

                var endHour = pvpTime.EndHour > 12 ? pvpTime.EndHour - 12 : pvpTime.EndHour;
                if (endHour == 0) endHour = 12;
                var endMinute = pvpTime.EndMinute < 10 ? $"0{pvpTime.EndMinute}" : pvpTime.EndMinute.ToString();
                var endAmPm = pvpTime.EndHour > 12 ? "pm" : "am";

                var pvpActive = Core.PvpService.IsPvpActiveDuringTime(pvpTime, DateTime.Now);

                sb.AppendLine($"{count + 1}{(pvpActive ? "*" : "")}: {dayString} - {startHour}:{startMinute}{startAmPm} - {endHour}:{endMinute}{endAmPm}");
                count++;
                if (count % 7 == 0)
                {
                    ctx.Reply(sb.ToString());
                    sb.Clear();
                }
            }
            if (sb.Length > 0)
            {
                ctx.Reply(sb.ToString());
            }
        }

        [Command("remove", description: "Removes a PvP time", adminOnly: true)]
        public static void RemovePvpTime(ChatCommandContext ctx, int index)
        {
            if(Core.PvpService.RemovePvpTime(index - 1))
            {
                ctx.Reply($"PvP time removed");
            }
            else
            {
                ctx.Reply($"PvP time not found");
            }
        }
    }
}
