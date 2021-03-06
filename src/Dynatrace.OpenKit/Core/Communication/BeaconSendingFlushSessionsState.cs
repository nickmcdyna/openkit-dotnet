﻿//
// Copyright 2018-2019 Dynatrace LLC
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
//

using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Util;

namespace Dynatrace.OpenKit.Core.Communication
{
    /// <summary>
    /// In this state open sessions are finished. After that all sessions are sent to the server.
    /// 
    /// Transitions to:
    /// <ul>
    ///     <li><see cref="BeaconSendingTerminalState"/></li>
    /// </ul> 
    /// </summary>
    internal class BeaconSendingFlushSessionsState : AbstractBeaconSendingState
    {
        public const int BEACON_SEND_RETRY_ATTEMPTS = 0; // do not retry beacon sending on error

        public BeaconSendingFlushSessionsState() : base(false) { }

        internal override AbstractBeaconSendingState ShutdownState => new BeaconSendingTerminalState();

        protected override void DoExecute(IBeaconSendingContext context)
        {
            // first get all sessions that do not have any multiplicity explicitely set
            context.NewSessions.ForEach(newSession =>
            {
                var currentConfiguration = newSession.BeaconConfiguration;
                newSession.UpdateBeaconConfiguration(
                    new BeaconConfiguration(1, currentConfiguration.DataCollectionLevel, currentConfiguration.CrashReportingLevel));
            });

            // end all open sessions -> will be flushed afterwards
            context.OpenAndConfiguredSessions.ForEach(openSession =>
            {
                openSession.End();
            });

            // flush already finished (and previously ended) sessions
            var tooManyRequestsReceived = false;
            context.FinishedAndConfiguredSessions.ForEach(finishedSession =>
            {
                if (!tooManyRequestsReceived && finishedSession.IsDataSendingAllowed)
                {
                    var statusResponse = finishedSession.SendBeacon(context.HTTPClientProvider);
                    if (BeaconSendingResponseUtil.IsTooManyRequestsResponse(statusResponse))
                    {
                        tooManyRequestsReceived = true;
                    }
                }
                finishedSession.ClearCapturedData();
                context.RemoveSession(finishedSession);
            });

            // make last state transition to terminal state
            context.NextState = new BeaconSendingTerminalState();
        }
        public override string ToString()
        {
            return "FlushSessions";
        }
    }
}
