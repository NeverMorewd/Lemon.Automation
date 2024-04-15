using FlaUI.Core.AutomationElements;
using Newtonsoft.Json.Linq;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.Framework.AutomationCore.Domains
{
    public interface IAutomationService
    {
        Observable<JObject> ObserveElementObjectsFromCurrentPoint();
        Observable<AutomationElement> ObserveElementsFromCurrentPoint();
    }
}
