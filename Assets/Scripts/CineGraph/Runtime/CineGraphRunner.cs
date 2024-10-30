using System.Collections;
using UnityEngine;

namespace CineGraph
{
	/// <summary>
	///		Allows to run CineGraph assets
	/// </summary>
    public class CineGraphRunner : MonoBehaviour
    {
		private CineGraphAsset m_graphInstance;

		public void LoadAsset(CineGraphAsset graphAsset)
		{
			m_graphInstance = Instantiate(graphAsset);
			m_graphInstance.Init();

			foreach(var node in m_graphInstance.Nodes)
			{
				node.Initialise(m_graphInstance, this);
			}
		}

		/// <summary>
		///		Runs the graph  from the start note (<see cref="OnInteractNode"/>>)
		/// </summary>
		public CineGraphProcess Run() => RunFromNode<OnInteractNode>();

		/// <summary>
		///		Runs the graph from the node of a given type
		/// </summary>
		/// <remarks> You should a node type that can only be present once in the graph to avoid bugs </remarks>
		/// <typeparam name="T"> The type of the start node </typeparam>
		public CineGraphProcess RunFromNode<T>() where T : CineGraphNode =>
			RunFromNode(m_graphInstance.GetNodeOfType<T>().guid);

		public CineGraphProcess RunFromNode(string nodeGuid)
		{
			CineGraphProcess process = new CineGraphProcess(this, m_graphInstance, nodeGuid);
			process.routine = StartCoroutine(process.Execute());
			return process;
		}
	}
	
	/// <summary>
	///		Stores data relative to a running Cinegraph process
	/// </summary>
	public class CineGraphProcess
	{
		public string NextNodeID
		{
			set => m_nextNodeID = value;
		}
		
		public string CurrentNodeID => m_currentNodeID;

		public string PreviousNodeID => m_previousNodeID;

		public bool Running => m_running;

		public Coroutine routine;
		
		private readonly CineGraphAsset m_graphInstance;
		private readonly CineGraphRunner m_runner;
		private string m_nextNodeID;
		private string m_currentNodeID;
		private string m_previousNodeID;
		private bool m_running = true;
		
		public CineGraphProcess(CineGraphRunner runner, CineGraphAsset graphInstance, string startNode)
		{
			m_graphInstance = graphInstance;
			m_runner = runner;
			m_currentNodeID = startNode;
		}

		public IEnumerator Execute()
		{
			yield return new WaitForEndOfFrame();
			
			CineGraphNode currentNode = m_graphInstance.GetNodeWithID(m_currentNodeID);

			if (currentNode == null) yield break;

			while (m_running)
			{
				currentNode.FetchVariables(m_graphInstance);
				yield return currentNode.OnProcess(m_graphInstance, m_runner, this);

				if (string.IsNullOrEmpty(m_nextNodeID)) break;

				m_previousNodeID = m_currentNodeID;
				m_currentNodeID = m_nextNodeID;
				currentNode = m_graphInstance.GetNodeWithID(m_currentNodeID);
			}
		}
	
		/// <summary>
		///		Stops the process
		/// </summary>
		public void Drop()
		{
			m_running = false;
			m_runner.StopCoroutine(routine);
		}
	}
}
