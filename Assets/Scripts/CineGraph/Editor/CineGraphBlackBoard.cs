using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;

namespace CineGraph.Editor
{
    public class CineGraphBlackBoard : Blackboard
    {
        private SerializedObject m_serializedObject;
        private GenericMenu m_menu;
		private CineGraphAsset m_asset;

        public CineGraphBlackBoard(GraphView graphView, SerializedObject serializedObject)
		{ 
            m_serializedObject = serializedObject;
			m_asset = (CineGraphAsset)m_serializedObject.targetObject;
            this.graphView = graphView;

            this.AddToClassList("cine-graph-blackboard");
            this.scrollable = true;

            BuildTypeMenu();
			DrawSavedVariables();

			this.addItemRequested += (Blackboard _) => OnAddVariableRequested();
            this.editTextRequested += OnVariableNameEdited;
			this.RegisterCallback<KeyDownEvent>(evt => {
				if (evt.keyCode == KeyCode.Delete || evt.keyCode == KeyCode.Backspace) OnDeleteInput(evt);
			});
        }

		void OnDeleteInput(KeyDownEvent evt)
		{
			foreach(var element in selection)
			{
				if (element is BlackboardField field)
				{
					CineGraphVariable variable = (CineGraphVariable)field.userData;
					variable.OnDelete();
					string variableGuid = variable.GUID;

					m_asset.Variables.RemoveAll(x => x.GUID == variableGuid);
				}
			}

			Clear();
			DrawSavedVariables();
			MarkDirtyRepaint();
		}
		
        void BuildTypeMenu()
        {
            m_menu = new GenericMenu();
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (Assembly assembly in assemblies)
			{
				foreach (Type type in assembly.GetTypes())
				{
					if (type.CustomAttributes.ToList() == null) continue;

					var attribute = type.GetCustomAttribute(typeof(CineGraphVariableAttribute));

					if (attribute == null) continue;

					CineGraphVariableAttribute att = attribute as CineGraphVariableAttribute;
					var node = Activator.CreateInstance(type);

					if (string.IsNullOrEmpty(att.name)) continue;

					m_menu.AddItem(new GUIContent(att.name, att.tooltip), false, () => CreateNewVariable(type));
				}
			}
		}

		void OnAddVariableRequested()
        { 
            m_menu.ShowAsContext();
        }

        void CreateNewVariable(Type type)
        {
			string guid = Guid.NewGuid().ToString();

			CineGraphVariable instance = Activator.CreateInstance(type, "New Variable", guid) as CineGraphVariable;

			m_asset.Variables.Add(instance);

			DrawVariable(instance);
		}
        
		void DrawSavedVariables()
		{
			CineGraphAsset asset = (CineGraphAsset)m_serializedObject.targetObject;
			foreach(CineGraphVariable variable in asset.Variables)
			{
				DrawVariable(variable);
			}
		}

		void DrawVariable(CineGraphVariable variable)
		{
			Type type = variable.GetType();

			BlackboardField blackboardField = new BlackboardField { text = variable.Name, typeText = type.Name };
			blackboardField.userData = variable;
			blackboardField.RegisterCallback<MouseDownEvent>(evt =>
			{
				if (evt.button == (int)MouseButton.MiddleMouse)
				{
					CineGraphEditorView view = (CineGraphEditorView)this.graphView;
					VariableNode newVariableNode = new VariableNode
					{
						GUID = variable.GUID
					};
					var windowMousePosition = graphView.ChangeCoordinatesTo(graphView, Event.current.mousePosition - ((CineGraphEditorView)graphView).window.position.position);
					var graphMousePosition = graphView.contentViewContainer.WorldToLocal(windowMousePosition) + new Vector2(800, 0);
					newVariableNode.SetPosition(new Rect(graphMousePosition, new Vector2()));
					view.AddNode(newVariableNode);
				}

				evt.StopPropagation();
			});

			VisualElement container = new VisualElement
			{
				userData = variable.GUID
			};

			BlackboardRow blackboardRow = new BlackboardRow(blackboardField, container)
			{
				userData = variable.GUID
			};

			if (type.GetInterface("ICineGraphCustomVariableUI") is Type customUIInterface)
				customUIInterface.GetMethod("BuildCustomUI")?.Invoke(variable, new object[] { container });

			this.Add(blackboardRow);
		}

		void OnVariableNameEdited(Blackboard blackboard, VisualElement element, string newName)
		{
			string variableGuid = ((CineGraphVariable)element.userData).GUID;

			m_asset.Variables.Find(x => x.GUID == variableGuid).Name = newName;

			((BlackboardField)element).text = newName;
		}
	}
}
