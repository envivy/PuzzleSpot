using UnityEngine;
using UnityEngine.UI;

public static class RuleInteractionAutoSetup
{
    public static void SetupOperation(GameObject gameObject)
    {
        EnsurePointerGraphic(gameObject);

        if (gameObject.GetComponent<RectTransform>() != null)
        {
            RemoveAutoPhysics(gameObject);
            return;
        }

        BoxCollider2D collider = EnsureBoxCollider(gameObject);
        collider.isTrigger = true;

        Rigidbody2D rigidbody = gameObject.GetComponent<Rigidbody2D>();
        if (rigidbody == null)
        {
            rigidbody = gameObject.AddComponent<Rigidbody2D>();
        }

        rigidbody.bodyType = RigidbodyType2D.Kinematic;
        rigidbody.simulated = true;
        rigidbody.useFullKinematicContacts = true;
        rigidbody.gravityScale = 0f;
        rigidbody.freezeRotation = true;
    }

    public static void SetupTarget(GameObject gameObject)
    {
        if (gameObject.GetComponent<RectTransform>() != null)
        {
            RemoveAutoPhysics(gameObject);
            return;
        }

        BoxCollider2D collider = EnsureBoxCollider(gameObject);
        collider.isTrigger = true;
    }

    public static string FindUiTargetAtScreenPosition(Vector2 screenPosition, Camera eventCamera)
    {
        RuleTarget[] targets = Object.FindObjectsOfType<RuleTarget>(true);
        RuleTarget bestTarget = null;
        int bestPriority = int.MinValue;
        string bestPath = string.Empty;

        foreach (RuleTarget target in targets)
        {
            RectTransform rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform == null || string.IsNullOrEmpty(target.targetId))
            {
                continue;
            }

            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition, eventCamera))
            {
                string sortPath = GetHierarchySortPath(rectTransform);
                if (bestTarget == null ||
                    target.priority > bestPriority ||
                    (target.priority == bestPriority && string.CompareOrdinal(sortPath, bestPath) > 0))
                {
                    bestTarget = target;
                    bestPriority = target.priority;
                    bestPath = sortPath;
                }
            }
        }

        return bestTarget != null ? bestTarget.targetId : null;
    }

    private static void EnsurePointerGraphic(GameObject gameObject)
    {
        if (gameObject.GetComponent<Graphic>() != null)
        {
            return;
        }

        if (gameObject.GetComponent<RectTransform>() == null)
        {
            return;
        }

        Image image = gameObject.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = true;

        LayoutElement layoutElement = gameObject.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = gameObject.AddComponent<LayoutElement>();
        }

        layoutElement.ignoreLayout = true;
    }

    private static BoxCollider2D EnsureBoxCollider(GameObject gameObject)
    {
        BoxCollider2D collider = gameObject.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }

        ApplyColliderSize(gameObject, collider);
        return collider;
    }

    private static void RemoveAutoPhysics(GameObject gameObject)
    {
        Rigidbody2D rigidbody = gameObject.GetComponent<Rigidbody2D>();
        if (rigidbody != null)
        {
            Object.Destroy(rigidbody);
        }

        BoxCollider2D collider = gameObject.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            Object.Destroy(collider);
        }
    }

    private static void ApplyColliderSize(GameObject gameObject, BoxCollider2D collider)
    {
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Rect rect = rectTransform.rect;
            collider.size = new Vector2(Mathf.Max(1f, rect.width), Mathf.Max(1f, rect.height));
            collider.offset = rect.center;
            return;
        }

        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            collider.size = spriteRenderer.sprite.bounds.size;
            collider.offset = spriteRenderer.sprite.bounds.center;
            return;
        }

        Renderer renderer = gameObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            Bounds localBounds = TransformBoundsToLocal(gameObject.transform, renderer.bounds);
            collider.size = new Vector2(Mathf.Max(0.1f, localBounds.size.x), Mathf.Max(0.1f, localBounds.size.y));
            collider.offset = localBounds.center;
            return;
        }

        collider.size = Vector2.one;
        collider.offset = Vector2.zero;
    }

    private static Bounds TransformBoundsToLocal(Transform transform, Bounds worldBounds)
    {
        Vector3 min = transform.InverseTransformPoint(worldBounds.min);
        Vector3 max = transform.InverseTransformPoint(worldBounds.max);
        Bounds bounds = new Bounds((min + max) * 0.5f, Vector3.zero);
        bounds.Encapsulate(min);
        bounds.Encapsulate(max);
        return bounds;
    }

    private static string GetHierarchySortPath(Transform transform)
    {
        string path = string.Empty;
        Transform current = transform;
        while (current != null)
        {
            path = current.GetSiblingIndex().ToString("D6") + "/" + path;
            current = current.parent;
        }

        return path;
    }
}
