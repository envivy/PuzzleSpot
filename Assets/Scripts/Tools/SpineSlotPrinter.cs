using Spine;
using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SpineSlotPrinter : MonoBehaviour
{
    [Serializable]
    public class SlotVisibility
    {
        public string slotName;
        public bool visible = true;
        public string defaultAttachmentName;
    }

    [SerializeField] private SkeletonGraphic skeletonGraphic;
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private bool printOnStart = true;
    [SerializeField] private bool includeCurrentAttachment = true;
    [SerializeField] private bool applyInEditMode = true;
    [SerializeField] private List<SlotVisibility> slots = new List<SlotVisibility>();

    public IReadOnlyList<SlotVisibility> Slots => slots;

    private void Reset()
    {
        skeletonGraphic = GetComponent<SkeletonGraphic>();
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        RefreshSlots();
    }

    private void Awake()
    {
        ResolveComponents();
    }

    private void Start()
    {
        if (printOnStart)
        {
            PrintSlots();
        }

        ApplySlotVisibility();
    }

    private void OnValidate()
    {
        ResolveComponents();

        if (!Application.isPlaying && applyInEditMode)
        {
            ApplySlotVisibility();
        }
    }

    [ContextMenu("Print Slots")]
    public void PrintSlots()
    {
        Skeleton skeleton = GetSkeleton();
        if (skeleton == null)
        {
            Debug.LogWarning($"[SpineSlotPrinter] No SkeletonGraphic or SkeletonAnimation found on {name}.", this);
            return;
        }

        Debug.Log($"[SpineSlotPrinter] {name} slot count: {skeleton.Slots.Count}", this);
        for (int i = 0; i < skeleton.Slots.Count; i++)
        {
            Slot slot = skeleton.Slots.Items[i];
            string attachmentName = slot.Attachment != null ? slot.Attachment.Name : "null";
            string message = includeCurrentAttachment
                ? $"[SpineSlotPrinter] #{i:00} Slot: {slot.Data.Name}, Attachment: {attachmentName}"
                : $"[SpineSlotPrinter] #{i:00} Slot: {slot.Data.Name}";

            Debug.Log(message, this);
        }
    }

    [ContextMenu("Refresh Slot List")]
    public void RefreshSlots()
    {
        Skeleton skeleton = GetSkeleton();
        if (skeleton == null)
        {
            return;
        }

        Dictionary<string, SlotVisibility> existingSlots = new Dictionary<string, SlotVisibility>();
        foreach (SlotVisibility slotVisibility in slots)
        {
            if (slotVisibility != null && !string.IsNullOrEmpty(slotVisibility.slotName))
            {
                existingSlots[slotVisibility.slotName] = slotVisibility;
            }
        }

        slots.Clear();
        for (int i = 0; i < skeleton.Slots.Count; i++)
        {
            Slot slot = skeleton.Slots.Items[i];
            string slotName = slot.Data.Name;
            string attachmentName = slot.Attachment != null ? slot.Attachment.Name : string.Empty;

            if (!existingSlots.TryGetValue(slotName, out SlotVisibility slotVisibility))
            {
                slotVisibility = new SlotVisibility();
                slotVisibility.slotName = slotName;
                slotVisibility.visible = true;
            }

            if (string.IsNullOrEmpty(slotVisibility.defaultAttachmentName))
            {
                slotVisibility.defaultAttachmentName = attachmentName;
            }

            slots.Add(slotVisibility);
        }
    }

    public void SetSlotVisible(string slotName, bool visible)
    {
        SlotVisibility slotVisibility = slots.Find(x => x.slotName == slotName);
        if (slotVisibility == null)
        {
            return;
        }

        slotVisibility.visible = visible;
        ApplySlotVisibility();
    }

    public void ApplySlotVisibility()
    {
        Skeleton skeleton = GetSkeleton();
        if (skeleton == null)
        {
            return;
        }

        foreach (SlotVisibility slotVisibility in slots)
        {
            if (slotVisibility == null || string.IsNullOrEmpty(slotVisibility.slotName))
            {
                continue;
            }

            if (slotVisibility.visible)
            {
                if (!string.IsNullOrEmpty(slotVisibility.defaultAttachmentName))
                {
                    skeleton.SetAttachment(slotVisibility.slotName, slotVisibility.defaultAttachmentName);
                }
            }
            else
            {
                skeleton.SetAttachment(slotVisibility.slotName, null);
            }
        }

        if (skeletonGraphic != null)
        {
            skeletonGraphic.SetVerticesDirty();
        }

        if (skeletonAnimation != null)
        {
            skeletonAnimation.AnimationState?.Apply(skeletonAnimation.Skeleton);
            skeletonAnimation.Update(0f);
        }
    }

    private void ResolveComponents()
    {
        if (skeletonGraphic == null)
        {
            skeletonGraphic = GetComponent<SkeletonGraphic>();
        }

        if (skeletonAnimation == null)
        {
            skeletonAnimation = GetComponent<SkeletonAnimation>();
        }
    }

    private Skeleton GetSkeleton()
    {
        ResolveComponents();

        if (skeletonGraphic != null)
        {
            skeletonGraphic.Initialize(false);
            return skeletonGraphic.Skeleton;
        }

        if (skeletonAnimation != null)
        {
            skeletonAnimation.Initialize(false);
            return skeletonAnimation.Skeleton;
        }

        return null;
    }
}
