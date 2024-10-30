using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor.UIElements;
using System;
using System.Reflection;
using Codice.CM.SEIDInfo;

namespace CineGraph.Editor
{
    public class CineGraphEditorView : GraphView
    {
		private CineGraphAsset m_cineGraph;
		private SerializedObject m_serializedObject;
		private CineGraphEditorWindow m_window;

		public CineGraphEditorWindow window => m_window;

		public List<CineGraphEditorNode> m_graphNodes;
		public Dictionary<string, CineGraphEditorNode> m_nodeDictionnary;
		public Dictionary<Edge, CineGraphConnection> m_connectionDictionnary;
		public CineGraphAsset GraphAsset => m_cineGraph;

		private CineGraphWindowSearchProvider m_searchProvider; 

		public CineGraphEditorView(SerializedObject serializedObject, CineGraphEditorWindow window)
		{
			m_serializedObject = serializedObject;
			m_cineGraph = (CineGraphAsset)m_serializedObject.targetObject;
			m_window = window;

			m_graphNodes = new List<CineGraphEditorNode>();
			m_nodeDictionnary = new();
			m_connectionDictionnary = new();

			m_searchProvider = ScriptableObject.CreateInstance<CineGraphWindowSearchProvider>();
			m_searchProvider.graph = this;
			this.nodeCreationRequest = ShowSearchWindow;

			StyleSheet style = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/CineGraph/Editor/USS/CineGraphEditor.uss");
			styleSheets.Add(style);

			GridBackground background = new GridBackground();
			background.name = "Grid";
			Add(background);
			background.SendToBack();
	
			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());
			this.AddManipulator(new ClickSelector());
			this.AddManipulator(new ContentZoomer());

			DrawNodes();
			DrawConnections();

			graphViewChanged += OnGraphViewChangedEvent;
		}

		private GraphViewChange OnGraphViewChangedEvent(GraphViewChange graphViewChange)
		{
			if(graphViewChange.movedElements != null)
			{
				Undo.RecordObject(m_serializedObject.targetObject, "Removed Node");

				foreach(CineGraphEditorNode node in graphViewChange.movedElements.OfType<CineGraphEditorNode>().ToList())
				{
					node.SavePosition();
				}
			}

			if(graphViewChange.elementsToRemove != null)
			{
				List<CineGraphEditorNode> nodes = graphViewChange.elementsToRemove.OfType<CineGraphEditorNode>().ToList();

				if (nodes.Count > 0)
				{
					Undo.RecordObject(m_serializedObject.targetObject, "Removed Node");

					for (int i = nodes.Count - 1; i >= 0; i--)
					{
						RemoveNode(nodes[i]);
					}
				}

				foreach(var connection in graphViewChange.elementsToRemove.OfType<Edge>())
				{
					RemoveConnection(connection);
				}
			}

			if(graphViewChange.edgesToCreate != null)
			{
				Undo.RecordObject(m_serializedObject.targetObject, "Added Connections");

				foreach (Edge edge in graphViewChange.edgesToCreate)
				{
					CreateEdge(edge);
				}
			}

			return graphViewChange;
		}

		public void RemoveConnection(Edge e)
		{
			if (m_connectionDictionnary.TryGetValue(e, out CineGraphConnection connection))
			{
				m_cineGraph.Connections.Remove(connection);
				m_connectionDictionnary.Remove(e);
			}

			m_serializedObject.Update();
		}
		
		private void CreateEdge(Edge edge)
		{
			CineGraphEditorNode inputNode = (CineGraphEditorNode)edge.input.node;
			int inputIndex = inputNode.ports.IndexOf(edge.input);
			
			CineGraphEditorNode outputNode = (CineGraphEditorNode)edge.output.node;
			int outputIndex = outputNode.ports.IndexOf(edge.output);

			CineGraphConnection connection = new(inputNode.graphNode.guid, inputIndex, outputNode.graphNode.guid, outputIndex);
			m_cineGraph.Connections.Add(connection);
			m_connectionDictionnary.Add(edge, connection);
		}

		private void RemoveNode(CineGraphEditorNode cineGraphEditorNode)
		{
			m_cineGraph.Nodes.Remove(cineGraphEditorNode.graphNode);
			m_nodeDictionnary.Remove(cineGraphEditorNode.graphNode.guid);
			m_graphNodes.Remove(cineGraphEditorNode);

			m_serializedObject.Update();
		}

		private void DrawNodes()
		{
			foreach(var node in m_cineGraph.Nodes)
			{
				AddNodeToGraph(node);
			}

			Bind();
		}
		
		private void DrawConnections()
		{
			if (m_cineGraph.Connections == null) return;

			foreach(CineGraphConnection connection in m_cineGraph.Connections)
			{
				DrawConnection(connection);
			}
		}

		private void DrawConnection(CineGraphConnection connection)
		{
			CineGraphEditorNode inputNode = GetNodeByID(connection.inputPort.nodeId);
			CineGraphEditorNode outputNode = GetNodeByID(connection.outputPort.nodeId);

			if (inputNode == null || outputNode == null) return;

			Port inPort = inputNode.ports[connection.inputPort.portIndex];
			Port outPort = outputNode.ports[connection.outputPort.portIndex];

			Edge edge = inPort.ConnectTo(outPort);
			AddElement(edge);

			m_connectionDictionnary.Add(edge, connection);
		}

		private CineGraphEditorNode GetNodeByID(string nodeId)
		{
			CineGraphEditorNode node = null;
			m_nodeDictionnary.TryGetValue(nodeId, out node);
			return node;
		}

		private void ShowSearchWindow(NodeCreationContext context)
		{
			m_searchProvider.target = (VisualElement)focusController.focusedElement;
			SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), m_searchProvider);
		}
		
		public void AddNode(CineGraphNode node)
		{
			if(!node.GetType().GetCustomAttribute<NodeInfoAttribute>().allowMultiples && m_cineGraph.Nodes.Where(x => x.GetType() == node.GetType()).Any())
			{
				EditorUtility.DisplayDialog($"{node.GetType().Name} doesn't allow multiples", $"You cannot add multiples {node.GetType().Name} nodes to your graph.", "fuck you");
				return;
			}

			Undo.RecordObject(m_serializedObject.targetObject, "Added Node");
			m_cineGraph.Nodes.Add(node);

			m_serializedObject.Update();

			AddNodeToGraph(node);
			Bind();
		}
		
		/// <summary>
		///		Adds a new node graphically
		/// </summary>
		/// <param name="node"> The node to add </param>
		private void AddNodeToGraph(CineGraphNode node)
		{
			node.typeName = node.GetType().AssemblyQualifiedName;

			CineGraphEditorNode editorNode = new CineGraphEditorNode(node, m_serializedObject, this);
			editorNode.SetPosition(node.position);
			m_graphNodes.Add(editorNode);
			m_nodeDictionnary.Add(node.guid, editorNode);
			
			AddElement(editorNode);
		}

		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			List<Port> allPorts = new List<Port>();
			List<Port> ports = new List<Port>();

			foreach(var node in m_graphNodes)
			{
				allPorts.AddRange(node.ports);
			}

			foreach(Port port in allPorts)
			{
				if (startPort == port) continue;
				if (startPort.node == port.node) continue;
				if (startPort.direction == port.direction) continue;
				if (startPort.portType != port.portType) continue;

				ports.Add(port);
			}

			return ports;
		}
	
		public void Bind()
		{
			m_serializedObject.Update();
			this.Bind(m_serializedObject);
		}
	}
}
