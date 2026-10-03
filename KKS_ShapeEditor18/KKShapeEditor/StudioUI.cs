using System;
using System.Collections.Generic;
using System.Linq;
using KKAPI.Studio;
using Studio;
using UnityEngine;
using AIChara;

namespace KKShapeEditor
{
	public static class StudioUI
	{
		public static void Init()
		{
			if (!StudioAPI.InsideStudio)
			{
				return;
			}
			StudioUI._window = new ShapeEditorWindow(1263730688, new Rect(20f, 20f, 380f, 600f));
			StudioUI._overlayGo = new GameObject("KKShapeEditor_StudioOverlay");
			UnityEngine.Object.DontDestroyOnLoad(StudioUI._overlayGo);
			StudioUI._overlay = StudioUI._overlayGo.AddComponent<ShapePaintOverlay>();
			StudioUI._overlay.Window = StudioUI._window;
			StudioUI._overlay.SelectionTool = new SelectionTool();
			StudioUI._overlay.Input = new InputHelper();
			StudioUI._overlay.Input.Init();
			StudioUI._overlay.OnRefreshRenderers = new Action(StudioUI.RefreshRenderers);
			StudioUI._overlay.GetCurrentSelection = new Func<object>(StudioUI.GetSelectedNode);
		}

		private static object GetSelectedNode()
		{
			global::Studio.Studio instance = Singleton<global::Studio.Studio>.Instance;
			if (instance == null || instance.treeNodeCtrl == null)
			{
				return null;
			}
			return instance.treeNodeCtrl.selectNode;
		}

		public static void RefreshRenderers()
		{
			if (StudioUI._window == null)
			{
				return;
			}
			IEnumerable<OCIChar> selectedCharacters = StudioAPI.GetSelectedCharacters();
			OCIChar[] array = ((selectedCharacters != null) ? selectedCharacters.ToArray<OCIChar>() : null);
			if (array != null && array.Length != 0)
			{
				ChaControl charInfo = array[0].charInfo;
				ShapeEditorController shapeEditorController = ((charInfo != null) ? charInfo.gameObject.GetComponent<ShapeEditorController>() : null);
				if (shapeEditorController != null)
				{
					shapeEditorController.ChaControl = charInfo;
					shapeEditorController.BindWindow(StudioUI._window, new Action(StudioUI.RefreshRenderers), ref StudioUI._subscribedCtrl);
					return;
				}
			}
			ShapeEditorController.UnbindCorruptionSubscription(new Action(StudioUI.RefreshRenderers), ref StudioUI._subscribedCtrl);
			StudioUI._window.Renderers.Clear();
			StudioUI._window.RendererPaths = new List<string>();
			StudioUI._window.RendererCategories = new List<string>();
			StudioUI._window.CorruptionState = null;
			IEnumerable<ObjectCtrlInfo> selectedObjects = StudioAPI.GetSelectedObjects();
			ObjectCtrlInfo[] array2 = ((selectedObjects != null) ? selectedObjects.ToArray<ObjectCtrlInfo>() : null);
			if (array2 != null && array2.Length != 0)
			{
				ObjectCtrlInfo[] array3 = array2;
				for (int i = 0; i < array3.Length; i++)
				{
					OCIItem ociitem = array3[i] as OCIItem;
					if (ociitem != null && ociitem.objectItem != null)
					{
						ItemShapeController itemShapeController = ociitem.objectItem.GetComponent<ItemShapeController>();
						if (itemShapeController == null)
						{
							itemShapeController = ociitem.objectItem.AddComponent<ItemShapeController>();
						}
						StudioUI._window.Renderers = itemShapeController.GetAllRenderers();
						Transform rootTransform = itemShapeController.RootTransform;
						List<string> list = new List<string>(StudioUI._window.Renderers.Count);
						List<string> list2 = new List<string>(StudioUI._window.Renderers.Count);
						for (int j = 0; j < StudioUI._window.Renderers.Count; j++)
						{
							Renderer renderer = StudioUI._window.Renderers[j];
							list.Add((renderer != null) ? ShapeEditorController.GetRelativePath(rootTransform, renderer.transform) : null);
							list2.Add("Item");
						}
						StudioUI._window.RendererPaths = list;
						StudioUI._window.RendererCategories = list2;
						break;
					}
				}
			}
			StudioUI._window.SanitizeSelectionAgainstRenderers();
			PresetTabState presetState = StudioUI._window.PresetState;
			if (presetState == null)
			{
				return;
			}
			presetState.ResetForOwnerSwitch();
		}

		public static ShapeEditorWindow Window
		{
			get
			{
				return StudioUI._window;
			}
		}

		public static ShapePaintOverlay Overlay
		{
			get
			{
				return StudioUI._overlay;
			}
		}

		public static void Toggle()
		{
			ShapeEditorWindow window = StudioUI._window;
			if (window == null)
			{
				return;
			}
			window.Toggle();
		}

		private static ShapeEditorWindow _window;
		private static GameObject _overlayGo;
		private static ShapePaintOverlay _overlay;
		private static ShapeEditorController _subscribedCtrl;
	}
}