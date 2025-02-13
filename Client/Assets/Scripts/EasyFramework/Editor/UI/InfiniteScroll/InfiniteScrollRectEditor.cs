using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.UI;

namespace Easy
{
    [CustomEditor(typeof(InfiniteScrollRect), true)]
    [CanEditMultipleObjects]
    public class InfiniteScrollRectEditor : Editor
    {
        SerializedProperty m_Item;
        SerializedProperty m_Padding;
        SerializedProperty m_RightPadding;
        SerializedProperty m_Spacing;
        SerializedProperty m_Content;
        SerializedProperty m_Horizontal;
        SerializedProperty m_Vertical;
        SerializedProperty m_MovementType;
        SerializedProperty m_Elasticity;
        SerializedProperty m_Inertia;
        SerializedProperty m_DecelerationRate;
        SerializedProperty m_ScrollSensitivity;
        SerializedProperty m_Viewport;
        SerializedProperty m_HorizontalScrollbar;
        SerializedProperty m_VerticalScrollbar;
        SerializedProperty m_HorizontalScrollbarVisibility;
        SerializedProperty m_VerticalScrollbarVisibility;
        SerializedProperty m_HorizontalScrollbarSpacing;
        SerializedProperty m_VerticalScrollbarSpacing;
        SerializedProperty m_OnValueChanged;
        AnimBool m_ShowElasticity;
        AnimBool m_ShowDecelerationRate;
        SerializedProperty m_AutoLocate;
        SerializedProperty m_DoLoacteTime;

        bool m_ViewportIsNotChild, m_HScrollbarIsNotChild, m_VScrollbarIsNotChild;
        static string s_HError = "For this visibility mode, the Viewport property and the Horizontal Scrollbar property both needs to be set to a Rect Transform that is a child to the Scroll Rect.";
        static string s_VError = "For this visibility mode, the Viewport property and the Vertical Scrollbar property both needs to be set to a Rect Transform that is a child to the Scroll Rect.";

        protected virtual void OnEnable()
        {
            m_Item                  = serializedObject.FindProperty("m_Item");
            m_Spacing               = serializedObject.FindProperty("m_Spacing");
            m_Padding               = serializedObject.FindProperty("m_Padding");
            m_RightPadding          = serializedObject.FindProperty("m_RightPadding");
            m_Content               = serializedObject.FindProperty("m_Content");
            m_Horizontal            = serializedObject.FindProperty("m_Horizontal");
            m_Vertical              = serializedObject.FindProperty("m_Vertical");
            m_MovementType          = serializedObject.FindProperty("m_MovementType");
            m_Elasticity            = serializedObject.FindProperty("m_Elasticity");
            m_Inertia               = serializedObject.FindProperty("m_Inertia");
            m_DecelerationRate      = serializedObject.FindProperty("m_DecelerationRate");
            m_ScrollSensitivity     = serializedObject.FindProperty("m_ScrollSensitivity");
            m_Viewport              = serializedObject.FindProperty("m_Viewport");
            m_HorizontalScrollbar   = serializedObject.FindProperty("m_HorizontalScrollbar");
            m_VerticalScrollbar     = serializedObject.FindProperty("m_VerticalScrollbar");
            m_HorizontalScrollbarVisibility = serializedObject.FindProperty("m_HorizontalScrollbarVisibility");
            m_VerticalScrollbarVisibility   = serializedObject.FindProperty("m_VerticalScrollbarVisibility");
            m_HorizontalScrollbarSpacing    = serializedObject.FindProperty("m_HorizontalScrollbarSpacing");
            m_VerticalScrollbarSpacing      = serializedObject.FindProperty("m_VerticalScrollbarSpacing");
            m_OnValueChanged        = serializedObject.FindProperty("m_OnValueChanged");
            m_AutoLocate            = serializedObject.FindProperty("m_AutoLocate");
            m_DoLoacteTime          = serializedObject.FindProperty("m_DoLoacteTime");
            m_ShowElasticity = new AnimBool(Repaint);
            m_ShowDecelerationRate = new AnimBool(Repaint);
            SetAnimBools(true);
        }

        protected virtual void OnDisable()
        {
            m_ShowElasticity.valueChanged.RemoveListener(Repaint);
            m_ShowDecelerationRate.valueChanged.RemoveListener(Repaint);
        }

        void SetAnimBools(bool instant)
        {
            SetAnimBool(m_ShowElasticity, !m_MovementType.hasMultipleDifferentValues && m_MovementType.enumValueIndex == (int)InfiniteScrollRect.MovementType.Elastic, instant);
            SetAnimBool(m_ShowDecelerationRate, !m_Inertia.hasMultipleDifferentValues && m_Inertia.boolValue == true, instant);
        }

        void SetAnimBool(AnimBool a, bool value, bool instant)
        {
            if (instant)
                a.value = value;
            else
                a.target = value;
        }

        void CalculateCachedValues()
        {
            m_ViewportIsNotChild = false;
            m_HScrollbarIsNotChild = false;
            m_VScrollbarIsNotChild = false;
            if (targets.Length == 1)
            {
                Transform transform = ((InfiniteScrollRect)target).transform;
                if (m_Viewport.objectReferenceValue == null || ((RectTransform)m_Viewport.objectReferenceValue).transform.parent != transform)
                    m_ViewportIsNotChild = true;
                if (m_HorizontalScrollbar.objectReferenceValue == null || ((Scrollbar)m_HorizontalScrollbar.objectReferenceValue).transform.parent != transform)
                    m_HScrollbarIsNotChild = true;
                if (m_VerticalScrollbar.objectReferenceValue == null || ((Scrollbar)m_VerticalScrollbar.objectReferenceValue).transform.parent != transform)
                    m_VScrollbarIsNotChild = true;
            }

            if (m_Horizontal.boolValue)
            {
                m_Vertical.boolValue = false;
            }
            else
            {
                m_Vertical.boolValue = true;
            }
        }

        public override void OnInspectorGUI()
        {
            SetAnimBools(false);

            serializedObject.Update();
            // Once we have a reliable way to know if the object changed, only re-cache in that case.
            CalculateCachedValues();

            EditorGUILayout.PropertyField(m_Item);
            EditorGUILayout.PropertyField(m_Spacing);
            EditorGUILayout.PropertyField(m_Padding);
            EditorGUILayout.PropertyField(m_RightPadding);
            EditorGUILayout.PropertyField(m_Content);

            EditorGUILayout.PropertyField(m_Horizontal);
            EditorGUILayout.PropertyField(m_Vertical);
            EditorGUILayout.PropertyField(m_AutoLocate);
            EditorGUILayout.PropertyField(m_DoLoacteTime);

            EditorGUILayout.PropertyField(m_MovementType);
            if (EditorGUILayout.BeginFadeGroup(m_ShowElasticity.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_Elasticity);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.PropertyField(m_Inertia);
            if (EditorGUILayout.BeginFadeGroup(m_ShowDecelerationRate.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_DecelerationRate);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.PropertyField(m_ScrollSensitivity);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_Viewport);

            EditorGUILayout.PropertyField(m_HorizontalScrollbar);
            if (m_HorizontalScrollbar.objectReferenceValue && !m_HorizontalScrollbar.hasMultipleDifferentValues)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_HorizontalScrollbarVisibility, EditorGUIUtility.TrTextContent("Visibility"));

                if ((InfiniteScrollRect.ScrollbarVisibility)m_HorizontalScrollbarVisibility.enumValueIndex == InfiniteScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport
                    && !m_HorizontalScrollbarVisibility.hasMultipleDifferentValues)
                {
                    if (m_ViewportIsNotChild || m_HScrollbarIsNotChild)
                        EditorGUILayout.HelpBox(s_HError, MessageType.Error);
                    EditorGUILayout.PropertyField(m_HorizontalScrollbarSpacing, EditorGUIUtility.TrTextContent("Spacing"));
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(m_VerticalScrollbar);
            if (m_VerticalScrollbar.objectReferenceValue && !m_VerticalScrollbar.hasMultipleDifferentValues)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_VerticalScrollbarVisibility, EditorGUIUtility.TrTextContent("Visibility"));

                if ((InfiniteScrollRect.ScrollbarVisibility)m_VerticalScrollbarVisibility.enumValueIndex == InfiniteScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport
                    && !m_VerticalScrollbarVisibility.hasMultipleDifferentValues)
                {
                    if (m_ViewportIsNotChild || m_VScrollbarIsNotChild)
                        EditorGUILayout.HelpBox(s_VError, MessageType.Error);
                    EditorGUILayout.PropertyField(m_VerticalScrollbarSpacing, EditorGUIUtility.TrTextContent("Spacing"));
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_OnValueChanged);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
