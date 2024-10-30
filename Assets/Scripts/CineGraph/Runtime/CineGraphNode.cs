using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CineGraph
{
	/// <summary>
	///		Base class for all CineGraph Nodes
	/// </summary>
    [Serializable]
    public class CineGraphNode
    {
		public string typeName;
		public string guid => m_guid;
		public Rect position => m_position;
		public List<NodePortVariableData> NodeVariables => m_nodeVariables;
		public CineGraphAsset m_graphAsset;

		[SerializeField]
        private string m_guid;

		[SerializeField]
		private Rect m_position;

		[SerializeField]
		private List<NodePortVariableData> m_nodeVariables;

        public CineGraphNode()
        {
			m_nodeVariables = new List<NodePortVariableData>();
            GenerateNewGUID();
        }

		public virtual void Initialise(CineGraphAsset asset, CineGraphRunner runner)
		{

		}

		public virtual IEnumerator OnProcess(CineGraphAsset graphInstance, CineGraphRunner runner,
			CineGraphProcess process)
		{
			CineGraphNode nextNodeInFlow = graphInstance.GetNodeFromOutput(m_guid, 0);

			if (nextNodeInFlow != null)
			{
				process.NextNodeID = nextNodeInFlow.guid;
				yield break;
			}

			process.NextNodeID = string.Empty;
		}

		public void FetchVariables(CineGraphAsset graphInstance)
		{
			foreach(NodePortVariableData variable in m_nodeVariables)
			{
				if (variable.portType == PortType.Output) continue;

				int inputPortID = variable.portID;
				foreach(CineGraphConnection connection in graphInstance.Connections)
				{
					if (connection.inputPort.nodeId != m_guid) continue;
					if (connection.inputPort.portIndex != inputPortID) continue;

					CineGraphNode outputNode = graphInstance.GetNodeWithID(connection.outputPort.nodeId);
					NodePortVariableData outputVariale = outputNode.NodeVariables.Find(x => x.portID == connection.outputPort.portIndex);

					if (outputVariale == null) continue;

					FieldInfo field = outputNode.GetType().GetField(outputVariale.name);

					if (field.GetValue(outputNode) == null)
					{
						outputNode.FetchVariables(graphInstance);
					}

					object fieldValue = field.GetValue(outputNode);
					if (this.GetType().GetField(variable.name) != null) this.GetType().GetField(variable.name).SetValue(this, fieldValue);
					variable.Value = fieldValue;
				}
			}
		}

		public void SetPosition(Rect position)
		{
			m_position = position;
		}

		private void GenerateNewGUID()
		{
            m_guid = Guid.NewGuid().ToString();
		}
	}

	/// <summary>
	///		Stores data relative to variables defined with the <see cref="ExposedPortPropertyAttribute"/>
	/// </summary>
	[System.Serializable]
	public class NodePortVariableData
	{
		public string name;
		public int portID;
		public PortType portType;
		object m_runtimeValue;

		public object Value
		{
			get
			{
				if (!Application.isPlaying)
				{
					Debug.LogError("NodeVariable.Value.Get is a runtime only method");
					return null;
				}

				return m_runtimeValue;	
			}
			set 
			{
				if (!Application.isPlaying)
				{
					Debug.LogError("NodeVariable.Value.Set is a runtime only method");
					return;
				}

				m_runtimeValue = value;
			}
		}

		public NodePortVariableData(string name, int portID, PortType portType)
		{
			this.name = name;
			this.portID = portID;
			this.portType = portType;	
		}
	}
}
