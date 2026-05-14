using UnityEngine;
using UnityEngine.EventSystems;

public enum DragType
{
   Left,
   Right,
}

public class DragOperation : MonoBehaviour,IBeginDragHandler,IEndDragHandler,IDragHandler
{
   [Tooltip("最小滑动距离（像素）")]
   public float minSwipeDistance = 5f;
   public DragType dragType;
   private Vector2 _dragStartPosition;
    
   
    // 可选：如果需要更精确的开始位置，可以加上 IBeginDragHandler
   public void OnBeginDrag(PointerEventData eventData)
    {
       _dragStartPosition = eventData.position;
    }

    
   public void OnEndDrag(PointerEventData eventData)
   {
      // 计算滑动距离
      float swipeDistance = eventData.position.x - _dragStartPosition.x;
        
      // 只有滑动距离超过阈值才判断方向
      if (Mathf.Abs(swipeDistance) > minSwipeDistance)
      {
         if (swipeDistance > 0 && dragType == DragType.Right)
         {
            var elementTrigger = GetComponent<Element>();
            elementTrigger?.OnOperateFinished();
         }
         else if(swipeDistance < 0 && dragType == DragType.Left)
         {
            var elementTrigger = GetComponent<Element>();
            elementTrigger?.OnOperateFinished();
         }
      }
   }

   public void OnDrag(PointerEventData eventData) { }
}
