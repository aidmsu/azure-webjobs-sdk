﻿using System;
using System.Collections.Generic;

namespace Dashboard.ViewModels
{
    public class FunctionInstanceDetailsViewModel
    {
        public InvocationLogViewModel Invocation { get; set; }

        public ParamModel[] Parameters { get; set; }

        public IEnumerable<Guid> ChildrenIds { get; set; }

        public InvocationLogViewModel Ancestor { get; set; }

        public TriggerReasonViewModel TriggerReason { get; set; }

        public string Trigger { get; set; }

        public bool IsAborted { get; set; }
    }
}