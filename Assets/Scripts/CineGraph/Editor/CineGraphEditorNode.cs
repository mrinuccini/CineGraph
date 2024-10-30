using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using System.Linq;

namespace CineGraph.Editor
{
	/// <summary>
	///		Represents a node in the GUI
	/// </summary>
    public class CineGraphEditorNode : Node
    {
		public CineGraphNode graphNode => m_graphNode;
		public List<Port> ports => m_ports;


		private CineGraphNode m_graphNode;
		private CineGraphEditorView m_view;

        private List<Port> m_outputPorts;
        private List<Port> m_ports;

		private SerializedProperty m_serializedProperty;
        private SerializedObject m_serializedObject;

        public CineGraphEditorNode(CineGraphNode node, SerializedObject cineGraphObject, CineGraphEditorView editorView)
        {
            this.AddToClassList("cine-graph-node");

            m_serializedObject = cineGraphObject;
            m_graphNode = node;
			m_view = editorView;

			m_outputPorts = new List<Port>();
            m_ports = new List<Port>();

            Type typeInfo = node.GetType();
            NodeInfoAttribute nodeInfo = typeInfo.GetCustomAttribute<NodeInfoAttribute>();

            title = nodeInfo.title;

            string[] depths = nodeInfo.menuItem.Split('/');
            foreach (string depth in depths)
            {
                this.AddToClassList(depth.ToLower().Replace(' ', '-'));
            }


			this.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>($"Assets/Scripts/CineGraph/Editor/USS/NodeStyle.uss"));
			StyleSheet style = AssetDatabase.LoadAssetAtPath<StyleSheet>($"Assets/Scripts/CineGraph/Editor/USS/{typeInfo.Name}Style.uss");
			if(style != null) this.styleSheets.Add(style);
			if (nodeInfo.customStyleSheet != null) styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(nodeInfo.customStyleSheet));

            this.name = typeInfo.Name;

			for(int i = 0; i < nodeInfo.numFlowOutput; i++)
				CreateFlowOutputPort();

			if (nodeInfo.hasFlowInput)
                CreateFlowInputPort();

			m_graphNode.NodeVariables.Clear();

            foreach(FieldInfo propery in typeInfo.GetFields())
            {
                if(propery.GetCustomAttribute<ExposedPropertyAttribute>() is ExposedPropertyAttribute exposedPropertyAtt)
                {
                    PropertyField propertyField = DrawProperty(propery.Name);
				}else if(propery.GetCustomAttribute<ExposedPortPropertyAttribute>() is ExposedPortPropertyAttribute exposedPortPropertyAtt)
				{
					switch (exposedPortPropertyAtt.portType)
					{
						case PortType.Output:
							(Port, int) outputVariable = CreatePort(Direction.Output, Port.Capacity.Multi, propery.FieldType, propery.Name, exposedPortPropertyAtt.tooltip, Color.cyan, outputContainer); 
							m_graphNode.NodeVariables.Add(new NodePortVariableData(propery.Name, outputVariable.Item2, PortType.Output));
							break;
						case PortType.Input:
							(Port, int) inputVariable = CreatePort(Direction.Input, Port.Capacity.Single, propery.FieldType, propery.Name, exposedPortPropertyAtt.tooltip, Color.cyan, inputContainer);
							m_graphNode.NodeVariables.Add(new NodePortVariableData(propery.Name, inputVariable.Item2, PortType.Input));
							break;
					}
				}
            }

			RefreshExpandedState();

			m_graphNode.m_graphAsset = m_serializedObject.targetObject as CineGraphAsset;
			
			if (typeInfo.GetInterface("ICineGraphCustomNodeUI") is Type customUIInterface) 
				customUIInterface.GetMethod("BuildCustomUI").Invoke(node, new object[] { this }); 
        }

		public (Port, int) CreateFlowInputPort() => CreatePort(Direction.Input, Port.Capacity.Multi, typeof(PortTypes.FlowPort), "In", "Flow Input", Color.yellow, inputContainer);

		public (Port, int) CreateContentFlowInputPort() => CreatePort(Direction.Input, Port.Capacity.Multi, typeof(PortTypes.FlowPort), "In", "Flow Input", Color.yellow, contentContainer);

		public (Port, int) CreateFlowOutputPort() => CreatePort(Direction.Output, Port.Capacity.Single, typeof(PortTypes.FlowPort), "Out", "Flow Output", Color.yellow, outputContainer);

		public (Port, int) CreateContentFlowOutputPort() => CreatePort(Direction.Output, Port.Capacity.Single, typeof(PortTypes.FlowPort), "Out", "Flow Output", Color.yellow, contentContainer);

		/// <summary>
		///		Creates a new port
		/// </summary>
		/// <param name="direction"> Direction of the port </param>
		/// <param name="capacity"> Capcity of the port </param>
		/// <param name="type"> Port data type </param>
		/// <param name="name"> Name of the port </param>
		/// <param name="tooltip"> Description of the port </param>
		/// <param name="color"> Color of the port </param>
		/// <param name="contentContainer"> Container of the port </param>
		/// <returns> The created port and its index </returns>
		public (Port, int) CreatePort(Direction direction, Port.Capacity capacity, Type type, string name, string tooltip, Color color, VisualElement contentContainer)
		{
			Port newPort = InstantiatePort(Orientation.Horizontal, direction, capacity, type);
			newPort.portName = name;
			newPort.tooltip = tooltip;
			newPort.portColor = color;
			m_ports.Add(newPort);

			if(direction == Direction.Output)
			{
				m_outputPorts.Add(newPort);
			}
			
			contentContainer.Add(newPort);

			RefreshPorts();

			return (newPort, m_ports.Count - 1);
		}

		public void RemovePort(Port port, VisualElement container, int portDecrement=1)
		{
			int portIndex = m_ports.IndexOf(port);
			CineGraphAsset asset = (CineGraphAsset)m_serializedObject.targetObject;

			for(int i = asset.Connections.Count - 1; i >= 0; i--)
			{
				CineGraphConnection connection = asset.Connections[i];
				CineGraphConnectionPort connectionPort = port.direction == Direction.Output ? connection.outputPort : connection.inputPort;

				if (connectionPort.nodeId != m_graphNode.guid) continue;

				if(connectionPort.portIndex == portIndex)
				{
					asset.Connections.RemoveAt(i);
					continue;
				}

				if(connectionPort.portIndex > portIndex)
					connectionPort.portIndex -= portDecrement;

				Edge edge = m_view.m_connectionDictionnary.FirstOrDefault(x => x.Value == connection).Key;
				if (edge != null) m_view.m_connectionDictionnary[edge] = connection;
			}

			m_view.DeleteElements(port.connections);

			m_serializedObject.Update();
			m_ports.RemoveAt(portIndex);
			if (port.direction == Direction.Output) m_outputPorts.Remove(port);
			container.Remove(port);
			RefreshPorts();
			RefreshExpandedState();
		}

		/// <summary>
		///		Show a window that allows the user to choose a variable
		/// </summary>
		/// <param name="filterType"> Shows variable only with this specific type </param>
		/// <param name="callback"> Fired when the user selects a variable. Contains the ID of the variable </param>
		public void ShowVariableSearchWindow(Type filterType, Action<string> callback)
		{
			SearchWindow.Open(new SearchWindowContext(Event.current.mousePosition),
				new CineGraphVariableSearchWindowProvider(m_view, callback, filterType));
		}

		public Port GetPort(int index) => m_ports[index];

		public void SavePosition()
		{
            m_graphNode.SetPosition(GetPosition());
		}

		private PropertyField DrawProperty(string propertyName)
		{
			if (m_serializedProperty == null)
			{
				FetchSerializedProperty();
			}

			SerializedProperty prop = m_serializedProperty.FindPropertyRelative(propertyName);

			PropertyField field = new PropertyField(prop);
			field.bindingPath = prop.propertyPath;
			extensionContainer.Add(field);
			return field;
		}

		private void FetchSerializedProperty()
		{
			SerializedProperty nodes = m_serializedObject.FindProperty("m_nodes");

			if (nodes.isArray)
			{
				int size = nodes.arraySize;

				for (int i = 0; i < size; i++)
				{
					var element = nodes.GetArrayElementAtIndex(i);
					var elementId = element.FindPropertyRelative("m_guid");
					if (elementId.stringValue == m_graphNode.guid)
					{
						m_serializedProperty = element;
					}
				}
			}
		}
	}
}
