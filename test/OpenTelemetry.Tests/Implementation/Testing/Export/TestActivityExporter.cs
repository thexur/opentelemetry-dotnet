﻿// <copyright file="TestActivityExporter.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using OpenTelemetry.Trace.Export;

namespace OpenTelemetry.Testing.Export
{
    public class TestActivityExporter : ActivityExporter
    {
        private readonly ConcurrentQueue<Activity> activities = new ConcurrentQueue<Activity>();
        private readonly Action<IEnumerable<Activity>> onExport;

        public TestActivityExporter(Action<IEnumerable<Activity>> onExport)
        {
            this.onExport = onExport;
        }

        public Activity[] ExportedActivities => this.activities.ToArray();

        public bool WasShutDown { get; private set; } = false;

        public override Task<ExportResult> ExportAsync(IEnumerable<Activity> data, CancellationToken cancellationToken)
        {
            // Added a sleep for zero milliseconds to respect cancellation time set by export timeout.
            Thread.Sleep(0);
            if (cancellationToken.IsCancellationRequested)
            {
                return default;
            }

            this.onExport?.Invoke(data);

            foreach (var s in data)
            {
                this.activities.Enqueue(s);
            }

            return Task.FromResult(ExportResult.Success);
        }

        public override Task ShutdownAsync(CancellationToken cancellationToken)
        {
            this.WasShutDown = true;
#if NET452
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }
    }
}
