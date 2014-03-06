﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Jobs;
using Microsoft.WindowsAzure.Jobs.Host;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dashboard.ViewModels
{
    public class DashboardIndexViewModel
    {
        public IEnumerable<InvocationLogViewModel> InvocationLogViewModels { get; set; }
        public IEnumerable<FunctionStatisticsViewModel> FunctionStatisticsViewModels { get; set; }
    }

    public class WebJobRunIdentifierViewModel
    {
        internal WebJobRunIdentifierViewModel(WebJobRunIdentifier id)
        {
            JobType = (WebJobTypes)id.JobType;
            JobName = id.JobName;
            RunId = id.RunId;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public WebJobTypes JobType { get; set; }

        public string JobName { get; set; }

        public string RunId { get; set; }
    }

    public class InvocationLogViewModel
    {
        internal InvocationLogViewModel(ExecutionInstanceLogEntity log)
        {
            Id = log.FunctionInstance.Id;
            FunctionName = log.FunctionInstance.Location.GetShortName();
            FunctionFullName = log.FunctionInstance.Location.ToString();
            FunctionDisplayTitle = BuildFunctionDisplayTitle(log.FunctionInstance);
            if (log.ExecutingJobRunId != null &&
                log.ExecutingJobRunId.WebSiteName == Environment.GetEnvironmentVariable(WebSitesKnownKeyNames.WebSiteNameKey))
            {
                ExecutingJobRunId = new WebJobRunIdentifierViewModel(log.ExecutingJobRunId);
            }
            Status = (FunctionInstanceStatus)log.GetStatus();
            switch (Status)
            {
                case FunctionInstanceStatus.Running:
                    WhenUtc = log.StartTime;
                    Duration = DateTime.UtcNow - log.StartTime;
                    break;
                case FunctionInstanceStatus.CompletedSuccess:
                    WhenUtc = log.EndTime;
                    Duration = log.GetDuration();
                    break;
                case FunctionInstanceStatus.CompletedFailed:
                    WhenUtc = log.EndTime;
                    Duration = log.GetDuration();
                    ExceptionType = log.ExceptionType;
                    ExceptionMessage = log.ExceptionMessage;
                    break;
                case FunctionInstanceStatus.NeverFinished:
                    WhenUtc = log.StartTime;
                    Duration = log.GetDuration();
                    break;
            }
        }

        public WebJobRunIdentifierViewModel ExecutingJobRunId { get; set; }

        private string BuildFunctionDisplayTitle(FunctionInvokeRequest functionInstance)
        {
            var name = new StringBuilder(functionInstance.Location.GetShortName());
            if (functionInstance.ParametersDisplayText != null)
            {
                name.Append(" (");
                if (functionInstance.ParametersDisplayText.Length > 20)
                {
                    name.Append(functionInstance.ParametersDisplayText.Substring(0, 18))
                        .Append(" ...");
                }
                else
                {
                    name.Append(functionInstance.ParametersDisplayText);
                }
                name.Append(")");
            }
            return name.ToString();
        }

        public Guid Id { get; set; }
        public string FunctionName { get; set; }
        public string FunctionFullName { get; set; }
        public string FunctionDisplayTitle { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public FunctionInstanceStatus Status { get; set; }
        public DateTime? WhenUtc { get; set; }
        [JsonConverter(typeof(DurationAsMillisecondsJsonConverter))]
        public TimeSpan? Duration { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionType { get; set; }
    }

    public class FunctionStatisticsViewModel
    {
        public string FunctionFullName { get; set; }
        public string FunctionName { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public bool IsRunning { get; set; }
        public bool IsOldHost { get; set; }
        public DateTime? LastStartTime { get; set; }
    }

    public class DurationAsMillisecondsJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var timespan = (TimeSpan)value;
            writer.WriteValue(timespan.TotalMilliseconds);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TimeSpan);
        }
    }
}