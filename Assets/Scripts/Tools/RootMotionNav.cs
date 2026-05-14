using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RootMotionNav : MonoBehaviour
{
    Animator at;
    NavMeshAgent ag;

    Vector3 target;
    void Start()
    {
        at = this.GetComponent<Animator>();
        ag = this.GetComponent<NavMeshAgent>();
        ag.ResetPath();
        target = transform.position;
        ag.updatePosition = false;
    }
    void OnAnimatorMove()
    {
        Vector3 position = at.rootPosition;
        ag.nextPosition = new Vector3(position.x, ag.nextPosition.y, position.z);
        position.y = ag.nextPosition.y;
        transform.position = position;
    }
    void Update()
    {

        float dis = Vector3.Distance(transform.position, target);
        if (dis < 0.3f) {
            at.SetFloat("Speed", 0);
        }
        else if (dis>=0.3f&&dis < 2)
        {
            at.SetFloat("Speed", (dis/2.0f)*0.5f);
        }
        else {
            at.SetFloat("Speed", 0.5f);
        }


        // ÒÆ¶¯Î»ÖÃ
        if (Input.GetMouseButtonDown(1))
        {
            var ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100, -1))
            {
                target = hit.point;
                ag.SetDestination(hit.point);
            }
        }
    }

}
