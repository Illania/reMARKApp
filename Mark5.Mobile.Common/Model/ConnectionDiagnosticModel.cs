using System;
namespace Mark5.Mobile.Common.Model
{
    /*
     *  POCO to report connection diagnostics results
     */
    public class ConnectionDiagnosticModel
    {
        public int SuccessfulRequestCount, FailedRequestCount;
        public long TotalElapsedTime, AverageElapsedTime;
        public ConnectionStatus Status;
        public ErrorCode Error;
        public double AverageElapsedTimeInSeconds;

        public ConnectionDiagnosticModel(ErrorCode errorCode)
        {
            Error = errorCode;
            Status = ConnectionStatus.Broken;
        }

        public ConnectionDiagnosticModel(int successResultCount, int failedRequestCount, long totalElapsedTime)
        {
            SuccessfulRequestCount = successResultCount;
            FailedRequestCount = failedRequestCount;
            TotalElapsedTime = totalElapsedTime;
            Error = ErrorCode.None;
            if (SuccessfulRequestCount > 0)
            {
                AverageElapsedTime = TotalElapsedTime / SuccessfulRequestCount;
                AverageElapsedTimeInSeconds = TimeSpan.FromMilliseconds(AverageElapsedTime).TotalSeconds;
            }

            if (SuccessfulRequestCount == SuccessfulRequestCount + FailedRequestCount)
                Status = ConnectionStatus.Stable;
            else if (SuccessfulRequestCount > 0 && SuccessfulRequestCount >= FailedRequestCount)
                Status = ConnectionStatus.Unstable;
            else if (SuccessfulRequestCount > 0 && SuccessfulRequestCount < FailedRequestCount)
                Status = ConnectionStatus.Bad;
            else
                Status = ConnectionStatus.Broken;
        }

        public enum ErrorCode
        {
            None, NoConfigurationInfo, UncaughtException
        }

        public enum ConnectionStatus
        {
            Stable, Unstable, Bad, Broken
        }
    }
}
