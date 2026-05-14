using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RuleOperation : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public string actorId;
    public RuleInteractionType interactionType = RuleInteractionType.Click;
    public RuleSwipeDirection swipeDirection = RuleSwipeDirection.Any;
    public float minSwipeDistance = 5f;
    public bool followDrag = true;

    private RuleLevelController _controller;
    private RectTransform _rectTransform;
    private Vector3 _startLocalPosition;
    private Vector2 _startDragPosition;
    private readonly List<string> _targets = new List<string>();

    private void Awake()
    {
        RuleInteractionAutoSetup.SetupOperation(gameObject);
        _controller = GetComponentInParent<RuleLevelController>();
        _rectTransform = GetComponent<RectTransform>();
        if (_rectTransform != null)
        {
            _startLocalPosition = _rectTransform.localPosition;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _startDragPosition = eventData.position;
        if (_rectTransform != null)
        {
            _startLocalPosition = _rectTransform.localPosition;
            transform.SetAsLastSibling();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!followDrag || interactionType == RuleInteractionType.Click || _rectTransform == null)
        {
            return;
        }

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_rectTransform, eventData.position, eventData.pressEventCamera, out Vector3 worldPoint))
        {
            transform.position = worldPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_rectTransform != null)
        {
            _rectTransform.localPosition = _startLocalPosition;
        }

        if (_controller == null)
        {
            return;
        }

        switch (interactionType)
        {
            case RuleInteractionType.DragToTarget:
            {
                string targetId = _targets.Count > 0 ? _targets[_targets.Count - 1] : null;
                if (string.IsNullOrEmpty(targetId) && _rectTransform != null)
                {
                    targetId = RuleInteractionAutoSetup.FindUiTargetAtScreenPosition(eventData.position, eventData.pressEventCamera);
                }
                _controller.TryHandleInteraction(actorId, RuleInteractionType.DragToTarget, targetId, this);
                _targets.Clear();
                break;
            }
            case RuleInteractionType.Swipe:
            {
                Vector2 delta = eventData.position - _startDragPosition;
                if (delta.magnitude >= minSwipeDistance)
                {
                    _controller.TryHandleInteraction(actorId, RuleInteractionType.Swipe, null, this, GetDirection(delta));
                }
                break;
            }
            case RuleInteractionType.DragEnd:
                _controller.TryHandleInteraction(actorId, RuleInteractionType.DragEnd, null, this);
                break;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_controller != null && interactionType == RuleInteractionType.Click)
        {
            _controller.TryHandleInteraction(actorId, RuleInteractionType.Click, null, this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        RuleTarget target = other.GetComponent<RuleTarget>();
        if (target != null && !_targets.Contains(target.targetId))
        {
            _targets.Add(target.targetId);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        RuleTarget target = other.GetComponent<RuleTarget>();
        if (target != null)
        {
            _targets.Remove(target.targetId);
        }
    }

    private static RuleSwipeDirection GetDirection(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
        {
            return delta.x >= 0 ? RuleSwipeDirection.Right : RuleSwipeDirection.Left;
        }

        return delta.y >= 0 ? RuleSwipeDirection.Up : RuleSwipeDirection.Down;
    }
}
