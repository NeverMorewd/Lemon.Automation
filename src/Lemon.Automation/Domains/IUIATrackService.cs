using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.Domains
{
    public interface IUIATrackService
    {
        IUIAElement ElementFromPoint(int aX, int aY, bool isEnableDeepTraversal);

        Observable<IUIAElement> ObserveElementsFromCurrentPoint(TimeSpan interval,
            bool isEnableDeepTraversal,
            CancellationToken cancellationToken);

        Observable<IUIAElement> ObserveElementsByMouseMove(TimeSpan interval,
            bool isEnableDeepTraversal,
            CancellationToken cancellationToken);

    }
}
