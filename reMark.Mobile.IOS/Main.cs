using System.Threading;
using UIKit;

namespace reMark.Mobile.IOS
{
    public class Application
    {

        static void Main(string[] args)
        {
            ThreadPool.GetMinThreads(out int workerThreads, out int completionPortThreads);
            ThreadPool.SetMinThreads(workerThreads * 5, completionPortThreads * 5);

            UIApplication.Main(args, null, "AppDelegate");
        }
    }
}