#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#else
public class Node
{

}
#endif

namespace CineGraph 
{ 
	/// <summary>
	///		Allows to draw custom node UI
	/// </summary>
	public interface ICineGraphCustomNodeUI
	{
		public void BuildCustomUI(Node node) { }
	}
}
