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
        }

        public ConnectionDiagnosticModel(int successResultCount, int failedRequestCount, long totalEllapsedTime, ErrorCode errorCode = ErrorCode.None)
        {
            SuccessfullRequestCount = successResultCount;
            FailedRequestCount = failedRequestCount;
            TotalEllapsedTime = totalEllapsedTime;
            Error = errorCode;
            AverageEllapsedTime = TotalEllapsedTime / SuccessfullRequestCount;
            AverageEllapsedTimeInSeconds = TimeSpan.FromMilliseconds(AverageEllapsedTime).TotalSeconds;

            if (SuccessfullRequestCount == SuccessfullRequestCount + FailedRequestCount)
                Status = ConnectionStatus.Stable;
            else if (SuccessfullRequestCount >= Math.Round((decimal)(SuccessfullRequestCount / 2)))
                Status = ConnectionStatus.Unstable;
            else if (SuccessfullRequestCount < Math.Round((decimal)(SuccessfullRequestCount / 2)))
                Status = ConnectionStatus.Bad;
            else
                Status = ConnectionStatus.Broken;
        }

        //"Failed to reach the server. Please make sure you have internet connection and current network has access to the server";
        public enum ErrorCode
        {
            None, NoConfigurationInfo, RequestsFailed, UncaughtException
        }

        /*
        string status;
        if (successResultCount == 3)
            status = "Connection to the server is Stable";
        else if (successResultCount == 2)
            status = "Connection to the server is Unstable";
        else
            status = "Connection to the server is Broken. Please contact your system administrator to resolve the issue.";
        */
        public enum ConnectionStatus
        {
            Stable, Unstable, Bad, Broken
        }

        //"Failed to reach the server. Please make sure you have internet connection and current network has access to the server";

    }
}
