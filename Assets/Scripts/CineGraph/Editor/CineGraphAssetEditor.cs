using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace CineGraph.Editor
{
    [CustomEditor(typeof(CineGraphAsset))]
    public class CineGraphAssetEditor : UnityEditor.Editor
    {
		[OnOpenAsset]
		public static bool OnOpenAsset(int instanceId, int index)
		{
			Object asset = EditorUtility.InstanceIDToObject(instanceId);
			
			if(asset.GetType() == typeof(CineGraphAsset))
			{
				CineGraphEditorWindow.Open((CineGraphAsset)asset);
				return true;
			}

			return false;
		}

		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Open"))
			{
				CineGraphEditorWindow.Open(target as CineGraphAsset);
			}

			DrawDefaultInspector();
		}
	}
}
