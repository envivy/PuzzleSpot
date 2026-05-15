using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpineSlotPrinter))]
public class SpineSlotPrinterEditor : Editor
{
    private SerializedProperty skeletonGraphic;
    private SerializedProperty skeletonAnimation;
    private SerializedProperty printOnStart;
    private SerializedProperty includeCurrentAttachment;
    private SerializedProperty applyInEditMode;
    private SerializedProperty slots;

    private void OnEnable()
    {
        skeletonGraphic = serializedObject.FindProperty("skeletonGraphic");
        skeletonAnimation = serializedObject.FindProperty("skeletonAnimation");
        printOnStart = serializedObject.FindProperty("printOnStart");
        includeCurrentAttachment = serializedObject.FindProperty("includeCurrentAttachment");
        applyInEditMode = serializedObject.FindProperty("applyInEditMode");
        slots = serializedObject.FindProperty("slots");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(skeletonGraphic);
        EditorGUILayout.PropertyField(skeletonAnimation);
        EditorGUILayout.PropertyField(printOnStart);
        EditorGUILayout.PropertyField(includeCurrentAttachment);
        EditorGUILayout.PropertyField(applyInEditMode);

        EditorGUILayout.Space();
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Refresh Slots"))
            {
                foreach (Object targetObject in targets)
                {
                    SpineSlotPrinter printer = (SpineSlotPrinter)targetObject;
                    Undo.RecordObject(printer, "Refresh Spine Slots");
                    printer.RefreshSlots();
                    EditorUtility.SetDirty(printer);
                }
            }

            if (GUILayout.Button("Apply Visibility"))
            {
                foreach (Object targetObject in targets)
                {
                    ((SpineSlotPrinter)targetObject).ApplySlotVisibility();
                }
            }

            if (GUILayout.Button("Print Slots"))
            {
                foreach (Object targetObject in targets)
                {
                    ((SpineSlotPrinter)targetObject).PrintSlots();
                }
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Slots", EditorStyles.boldLabel);

        for (int i = 0; i < slots.arraySize; i++)
        {
            SerializedProperty slot = slots.GetArrayElementAtIndex(i);
            SerializedProperty slotName = slot.FindPropertyRelative("slotName");
            SerializedProperty visible = slot.FindPropertyRelative("visible");
            SerializedProperty defaultAttachmentName = slot.FindPropertyRelative("defaultAttachmentName");

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(visible, GUIContent.none, GUILayout.Width(24));
                EditorGUILayout.LabelField(slotName.stringValue, GUILayout.MinWidth(120));
                EditorGUILayout.PropertyField(defaultAttachmentName, GUIContent.none);
            }
        }

        if (serializedObject.ApplyModifiedProperties())
        {
            foreach (Object targetObject in targets)
            {
                SpineSlotPrinter printer = (SpineSlotPrinter)targetObject;
                if (!Application.isPlaying)
                {
                    printer.ApplySlotVisibility();
                }

                EditorUtility.SetDirty(printer);
            }
        }
    }
}
