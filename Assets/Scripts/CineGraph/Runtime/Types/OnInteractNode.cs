using System.Collections;
using UnityEngine;

namespace CineGraph
{
    [NodeInfo("On Interact", "Events/On Interact", hasFlowInput: false, allowMultiples: false)]
    public class OnInteractNode : CineGraphNode
    {
		public override IEnumerator OnProcess(CineGraphAsset graphInstance, CineGraphRunner runner,
			CineGraphProcess process)
		{
			yield return base.OnProcess(graphInstance, runner, process);
		}
	}
}
