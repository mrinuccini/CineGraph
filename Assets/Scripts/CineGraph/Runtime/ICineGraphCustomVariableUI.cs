using UnityEngine.UIElements;

namespace CineGraph
{
	/// <summary>
	///		Allows to draw custom interface for variables  in the blackboard
	/// </summary>
	public interface ICineGraphCustomVariableUI
	{
		void BuildCustomUI(VisualElement parent) { }
	}
}
