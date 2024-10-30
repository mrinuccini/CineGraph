using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CineGraph
{
	[CreateAssetMenu(menuName = "CineGraph/Graph", fileName = "New CineGraph")]
	public class CineGraphAsset : ScriptableObject
	{
		public List<CineGraphConnection> Connections => m_connections;
		public List<CineGraphNode> Nodes => m_nodes;
		public List<CineGraphVariable> Variables => m_variables;

		public string graphName = "";

		[SerializeReference]
		private List<CineGraphNode> m_nodes;

		[SerializeField]
		private List<CineGraphConnection> m_connections;

		[SerializeReference]
		private List<CineGraphVariable> m_variables;

		private Dictionary<string, CineGraphNode> m_nodeDictionnary;
		private Dictionary<string, CineGraphVariable> m_variableDictionnary;

		public CineGraphAsset()
		{
			m_nodes = new List<CineGraphNode>();
			m_connections = new List<CineGraphConnection>();
			m_variables = new List<CineGraphVariable>();
		}

		public void Init()
		{
			m_nodeDictionnary = new();
			m_variableDictionnary = new();

			foreach(CineGraphNode node in m_nodes)
			{
				m_nodeDictionnary.Add(node.guid, node);
			}

			foreach(CineGraphVariable variable in m_variables)
			{
				m_variableDictionnary.Add(variable.GUID, variable);
			}
		}

		/// <summary>
		///		returns the first node of type T. returns null if no node was found
		/// </summary>
		/// <typeparam name="T"> The type of node to find </typeparam>
		/// <returns> The first node of type T </returns>
		public CineGraphNode GetNodeOfType<T>() where T : CineGraphNode
		{
			T[] foundNodes = Nodes.OfType<T>().ToArray();

			if(foundNodes.Length == 0)
			{
				Debug.LogError($"Couldn't find any nodes of type {typeof(T).Name}.");
				return null;
			}

			return foundNodes[0];
		}

		public CineGraphNode GetStartNode() => GetNodeOfType<OnInteractNode>();

		public bool HasAnyNodeOfType<T>() where T : CineGraphNode => m_nodes.OfType<T>().Any();

		/// <summary>
		///		returns the node with a specific ID. Returns null if no node was found
		/// </summary>
		/// <param name="nextNodeId"> The ID to look for </param>
		/// <returns> The node with the corresponding ID </returns>
		public CineGraphNode GetNodeWithID(string nextNodeId)
		{
			if (!Application.isPlaying)
			{
				Debug.LogError("CineGraphNode.GetNodeWithID is a runtime only method.");
				return null;
			}

			if(m_nodeDictionnary.TryGetValue(nextNodeId, out CineGraphNode node))
			{
				return node;
			}

			return null;
		}

		/// <summary>
		///		Returns a variable with a specific ID. Returns null if no variable was found
		/// </summary>
		/// <param name="id"> The ID of the variable to search </param>
		/// <returns> The variable found </returns>
		public CineGraphVariable GetVariableWithID(string id)
		{
			if (!Application.isPlaying)
			{
				Debug.LogError("CineGraphNode.GetVariableWithID is a runtime only method.");
				return null;
			}

			if (m_variableDictionnary.TryGetValue(id, out CineGraphVariable variable)) return variable;

			Debug.LogError($"Invalid variable with ID {id}");

			return null;
		}

		/// <summary>
		///		Returns the node connected to a certain output
		/// </summary>
		/// <returns> The node connected to the output node </returns>
		public CineGraphNode GetNodeFromOutput(string outputNodeId, int outputPort)
		{
			if (!Application.isPlaying)
			{
				Debug.LogError("CineGraph.GetNodeFromOutput is a runtime only method.");
				return null;
			}

			foreach(CineGraphConnection connection in m_connections)
			{
				if(connection.outputPort.nodeId == outputNodeId && connection.outputPort.portIndex == outputPort)
				{
					string nodeId = connection.inputPort.nodeId;
					CineGraphNode inputNode = m_nodeDictionnary[nodeId];
					return inputNode;
				}
			}

			return null;
		}
	}
}
