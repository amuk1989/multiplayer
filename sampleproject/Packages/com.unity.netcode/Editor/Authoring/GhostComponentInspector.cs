using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.NetCode.Editor
{
    /// <summary>
    /// Helper class for the GhostAuthoringComponent. Show and manage the components generated by the
    /// ghost entity conversion.
    /// </summary>
    class GhostComponentInspector
    {
        private Vector2 scrollPosition = Vector2.zero;
        private const int ComponentScrollViewPixelsHeigth = 400;
        readonly private GhostAuthoringComponent authoringComponent;
        readonly private EntityPrefabComponentsPreview prefabPreview;
        private List<ComponentItem> componentItems;
        private GhostComponentVariantLookup variantLookup;

        public GhostComponentInspector(GhostAuthoringComponent authoring, GhostComponentVariantLookup lookup)
        {
            authoringComponent = authoring;
            prefabPreview = new EntityPrefabComponentsPreview(lookup);
            componentItems = new List<ComponentItem>();
            variantLookup = lookup;
        }

        public void OnInspectorGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUI.indentLevel++;
            for (int ci = 0; ci < componentItems.Count; ++ci)
            {
                ShowComponent(componentItems[ci]);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndScrollView();
        }

        public void UpdateComponentList()
        {
            componentItems = prefabPreview.GetComponentsPreview(authoringComponent);
        }

        //This must be called every time the serializedObject (aka GhostAuthoringComponent) change
        public void Refresh()
        {
            foreach (var componentItem in componentItems)
                componentItem.Update(authoringComponent);
        }

        private void ShowComponent(ComponentItem componentItem)
        {
            GUIStyle style = new GUIStyle(EditorStyles.foldoutHeader);
            style.fontStyle = componentItem.hasDataToSend
                ? (componentItem.prefabOverride == null ? FontStyle.Bold : FontStyle.BoldAndItalic)
                : (componentItem.prefabOverride == null ? FontStyle.Normal : FontStyle.Italic);

            componentItem.expanded = EditorGUILayout.BeginFoldoutHeaderGroup(componentItem.expanded, componentItem.header, style);
            EditorGUILayout.EndFoldoutHeaderGroup();
            var foldoutRect = GUILayoutUtility.GetLastRect();
            if (componentItem.expanded)
            {
                var disableScope = new EditorGUI.DisabledGroupScope(!componentItem.supportPrefabOverrides);
                var currentVariant = componentItem.Variant;
                EditorGUI.BeginChangeCheck();
                var prefabType = AuthoringEditorHelper.PrefabTypeDrawer(componentItem);
                var sendType = AuthoringEditorHelper.SendMaskDrawer(componentItem);
                var sendForChild = AuthoringEditorHelper.SendChildDrawer(componentItem);
                var newVariant = AuthoringEditorHelper.VariantDrawer(componentItem, variantLookup);
                bool changed = EditorGUI.EndChangeCheck();
                AuthoringEditorHelper.FieldDrawer(componentItem);
                disableScope.Dispose();

                if (changed)
                {
                    //If the prefab use default values there is not need to have a modifier. Remove
                    bool useAllDefaults = componentItem.useAllDefaults;
                    var variantChanged = newVariant != currentVariant;
                    if (useAllDefaults && componentItem.prefabOverride != null)
                    {
                        authoringComponent.RemovePrefabOverride(componentItem.prefabOverride);
                        componentItem.OnRemoveOverride();
                        EditorUtility.SetDirty(authoringComponent.gameObject);
                    }
                    else if (!useAllDefaults)
                    {
                        try
                        {
                            componentItem.prefabOverride ??= authoringComponent.AddPrefabOverride(componentItem.comp.name, componentItem.entityGuid);
                            componentItem.UpdatePrefabOverrides(prefabType, sendType, sendForChild, newVariant);
                            EditorUtility.SetDirty(authoringComponent.gameObject);
                        }
                        catch (ArgumentException ae)
                        {
                            Debug.LogException(ae);
                        }
                    }
                    if (variantChanged)
                    {
                        //Refresh the component fields values to reflect the current selected variant
                        prefabPreview.RefreshComponentInfo(componentItem);
                        componentItem.UpdateHeader();
                    }
                }
            }
            //Display context menu when user press the left mouse button that have some shortcuts. Options available:
            // - Remove the component from the ghost (set the prefab to type to 0)
            // - Reset the component to its default (remove the overrides)
            if(componentItem.supportPrefabOverrides && foldoutRect.Contains(Event.current.mousePosition) &&
               Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                ModifierMenu(componentItem);
            }
        }

        private void ModifierMenu(ComponentItem componentItem)
        {
            //Show context menu with some options
            var menu = new GenericMenu();
            if (componentItem.prefabOverride != null)
            {
                menu.AddItem(new GUIContent("Reset Default"), false, () =>
                {
                    authoringComponent.RemovePrefabOverride(componentItem.prefabOverride);
                    componentItem.OnRemoveOverride();
                    EditorUtility.SetDirty(authoringComponent.gameObject);
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Reset Default"));
            }

            menu.ShowAsContext();
        }
    }
}
