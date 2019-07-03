using System;
namespace Mark5.Mobile.Common.Model
{
    /*
     *  POCO to report connection diagnostics results
     */
    public class ConnectionDiagnosticModel
    {
        public int SuccessfullRequestCount, FailedRequestCount;
        public long TotalEllapsedTime, AverageEllapsedTime;
        public ConnectionStatus Status;
        public ErrorCode Error;
        public double AverageEllapsedTimeInSeconds;

        public ConnectionDiagnosticModel() { }

        public ConnectionDiagnosticModel(ErrorCode errorCode)
        {
            Error = errorCode;
            Status = ConnectionStatus.Broken;
        }

        public ConnectionDiagnosticModel(int successResultCount, int failedRequestCount, long totalEllapsedTime, ErrorCode errorCode = ErrorCode.None)
        {
            SuccessfullRequestCount = successResultCount;
            FailedRequestCount = failedRequestCount;
            TotalEllapsedTime = totalEllapsedTime;
            Error = errorCode;
            if (SuccessfullRequestCount > 0)
            {
                AverageEllapsedTime = TotalEllapsedTime / SuccessfullRequestCount;
                AverageEllapsedTimeInSeconds = TimeSpan.FromMilliseconds(AverageEllapsedTime).TotalSeconds;
            }

            if (SuccessfullRequestCount == SuccessfullRequestCount + FailedRequestCount)
                Status = ConnectionStatus.Stable;
            else if (SuccessfullRequestCount > 0 && SuccessfullRequestCount >= FailedRequestCount)
                Status = ConnectionStatus.Unstable;
            else if (SuccessfullRequestCount > 0 && SuccessfullRequestCount < FailedRequestCount)
                Status = ConnectionStatus.Bad;
            else
                Status = ConnectionStatus.Broken;
        }

        public enum ErrorCode
        {
            None, NoConfigurationInfo, RequestsFailed, UncaughtException
        }

        public enum ConnectionStatus
        {
            Stable, Unstable, Bad, Broken
        }
    }
}
