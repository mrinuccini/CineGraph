using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif

namespace CineGraph
{
    [NodeInfo(title: "Variable", menuItem: "Variable", hasFlowInput: false, numFlowOutput: 0)]
    public class VariableNode : CineGraphNode, ICineGraphCustomNodeUI
    {
        [ExposedPortProperty(PortType.Output, "Variable Output")]
        public CineGraphVariable Variable;

        public string GUID;

		public override void Initialise(CineGraphAsset asset, CineGraphRunner runner)
		{
			Variable = asset.GetVariableWithID(GUID);
		}

#if UNITY_EDITOR
		public void BuildCustomUI(Node node) 
		{
			CineGraphVariable variable = m_graphAsset.Variables.Find(x => x.GUID == GUID);
			(node.outputContainer[0] as Port).portName = variable.Name;

			variable.OnNameChanged += (string newName) => {
				(node.outputContainer[0] as Port).portName = newName;
			};

			VisualElement titleContainer = node.contentContainer.Q(name: "title");
			titleContainer.Clear();
			VisualElement titleParent = titleContainer.parent;
			titleParent.Remove(titleContainer);
		}
#endif
	}
}
