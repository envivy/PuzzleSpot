using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum OperationType
{
   Drag,
   Click,
   DragEndTrigger,
   DragNoFollow,
}
public class Operation : MonoBehaviour,IDragHandler,IBeginDragHandler,IEndDragHandler,IPointerClickHandler
{
   public int operationID;
   public OperationType operationType = OperationType.Drag;
   public AudioClip audioClip;
   private RectTransform rectTransform;
   private List<GameObject> targets = new List<GameObject>();
   private Vector3 startPosition;
   private bool isDragging;
   private Rigidbody2D _rigidbody2D;
   private int _transformIndex;
   
   private void OnEnable()
   {
      _rigidbody2D = GetComponent<Rigidbody2D>();
      _transformIndex = transform.GetSiblingIndex();
      if(_rigidbody2D != null) _rigidbody2D.simulated = false;
      Invoke(nameof(ReActiveSimulated),1);
   }

   private void ReActiveSimulated()
   {
      if(_rigidbody2D != null) _rigidbody2D.simulated = true;
   }

   void Start()
   {
      rectTransform = GetComponent<RectTransform>();
      startPosition = rectTransform.localPosition;
   }

   private void OnTriggerEnter2D(Collider2D other)
   {
      if (!isDragging || !other.CompareTag("Element")) return;
      AddTarget(other.gameObject);
   }
   private void OnTriggerStay2D(Collider2D other)
   {
      if (!isDragging || !other.CompareTag("Element")) return;
      AddTarget(other.gameObject);
   }
   private void OnTriggerExit2D(Collider2D other)
   {
      if (!isDragging || !other.CompareTag("Element")) return;
      RemoveTarget(other.gameObject);
   }

   private void AddTarget(GameObject targetObject)
   {
      if(targets.Contains(targetObject)) return;
      targets.Add(targetObject);
      Debug.LogWarning("Last:"+targets[targets.Count - 1].name);
   }

   private void RemoveTarget(GameObject targetObject)
   {
      if (targets.Contains(targetObject))
      {
         targets.Remove(targetObject);
         Debug.LogWarning("RemoveTarget:"+targetObject.name);
      }
   }

   public void OnBeginDrag(PointerEventData eventData)
   {
      if (GameSet.instance.gameManager.teachTimer) GameSet.instance.gameManager.teachTimer = false;
      startPosition = rectTransform.localPosition;
      if (operationType == OperationType.Drag || operationType == OperationType.DragEndTrigger)
      {
         transform.SetAsLastSibling();
      }
      if(audioClip != null) GameSet.instance.audioManager.PlayAudio(audioClip);
   }

   public void OnDrag(PointerEventData eventData)
   {
      if(operationType == OperationType.Click || operationType == OperationType.DragNoFollow) return;
      if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
             rectTransform,
             eventData.position,
             eventData.pressEventCamera,
             out var worldPoint))
      {
         transform.position = worldPoint;
         isDragging = true;
      }
   }
   
   public void OnEndDrag(PointerEventData eventData)
   {
      if (operationType == OperationType.Drag || operationType == OperationType.DragEndTrigger || operationType == OperationType.DragNoFollow)
      {
         isDragging = false;
         rectTransform.localPosition = startPosition;
         transform.SetSiblingIndex(_transformIndex);
         if(!GameSet.instance.gameManager.startTimer) return;
         //只要拖拽结束就触发
         if (operationType == OperationType.DragEndTrigger || operationType == OperationType.DragNoFollow)
         {
            var elementTrigger = GetComponent<Element>();
            elementTrigger?.OnOperateFinished();
         }
         //要看目标对象
         else
         {
            if (targets.Count == 0) return;
            //自身是元素对象
            var element = GetComponent<Element>();
            if (element != null)
            {
               for (var i = targets.Count - 1; i >= 0; i--)
               {
                  if(targets.Count == 0) break;
                  var targetElement = targets[i].GetComponent<Element>();
                  if (!targetElement || !targetElement.enableOperateIDList.Contains(operationID)) continue;
                  gameObject.SetActive(false);
                  element.OnOperateFinished();
               }
            }
            //目标是元素对象
            else
            {
               for (var i = targets.Count - 1; i >= 0; i--)
               {
                  if(targets.Count == 0) break;
                  element = targets[i].GetComponent<Element>();
                  if (element == null || element.isFinished || !element.enableOperateIDList.Contains(operationID)) continue;
                  gameObject.SetActive(false);
                  element.OnOperateFinished();
                  break;
               }
            }
            targets.Clear();
         }
      }
   }

   public void OnPointerClick(PointerEventData eventData)
   {
      //20秒无操作结束引导
      if (GameSet.instance.gameManager.teachTimer) GameSet.instance.gameManager.teachTimer = false;
      if(!GameSet.instance.gameManager.startTimer) return;
      if (operationType != OperationType.Click) return;
      var element = GetComponent<Element>();
      if (element != null && !element.isFinished)
      {
         element.OnOperateFinished();
      }
   }
}
