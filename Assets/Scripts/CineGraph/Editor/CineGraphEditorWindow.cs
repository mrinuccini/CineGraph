using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CineGraph.Editor
{
    public class CineGraphEditorWindow : EditorWindow
    {
		public static void Open(CineGraphAsset target)
		{
			CineGraphEditorWindow[] windows = Resources.FindObjectsOfTypeAll<CineGraphEditorWindow>();

			foreach (var w in windows)
			{
				if (w.currentGraph == target)
				{
					w.Focus();
					return;
				}
			}
			
			CineGraphEditorWindow window = CreateWindow<CineGraphEditorWindow>(typeof(CineGraphEditorWindow), typeof(SceneView));
			window.titleContent = new GUIContent($"{target.name}", EditorGUIUtility.ObjectContent(target, typeof(CineGraphAsset)).image);
			window.Load(target);
		}

		[SerializeField]
		private CineGraphAsset m_currentGraph;

		[SerializeField]
		private SerializedObject m_serializedObject;

		[SerializeField]
		private CineGraphEditorView m_currentView;

		public CineGraphAsset currentGraph => m_currentGraph;
	
		public void Load(CineGraphAsset target)
		{
			m_currentGraph = target;
			DrawGraph();
		}

		private void OnEnable()
		{
			if(m_currentGraph != null)
			{
				DrawGraph();
			} 
		}

		private void OnGUI()
		{
			if (m_currentGraph == null) return;

			this.hasUnsavedChanges = EditorUtility.IsDirty(m_currentGraph);
		}

		private void DrawGraph()
		{
			m_serializedObject = new SerializedObject(m_currentGraph);
			m_currentView = new CineGraphEditorView(m_serializedObject, this);
			m_currentView.graphViewChanged += OnChange;
			rootVisualElement.Add(m_currentView);
			
			var blackBoard = new CineGraphBlackBoard(m_currentView, m_serializedObject)
			{
				title = m_serializedObject.targetObject.name,
				subTitle = "Properties"
			};

			blackBoard.StretchToParentSize();
			m_currentView.Add(blackBoard);
		}

		private GraphViewChange OnChange(GraphViewChange graphViewChange)
		{
			EditorUtility.SetDirty(m_currentGraph);
			return graphViewChange;
		}
	}
}
