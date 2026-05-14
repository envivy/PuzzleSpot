using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum RemainingDistanceType
{
    /// <summary>
    /// �����ɫָ����Ŀ�ĵأ���ģʽ�����ɫλ�õ�Ŀ�ĵ�֮��ľ��롣
    /// �����ɫָ�����ƶ�·������ģʽ�����ɫλ�õ��ƶ�·���յ�֮��ľ��롣
    /// </summary>
    PathEnd,

    /// <summary>
    /// �����ɫָ����Ŀ�ĵأ���ģʽ�����ɫλ�õ�Ŀ�ĵ�֮��ľ��롣
    /// �����ɫָ�����ƶ�·������ģʽ�����ɫλ�õ���һ��·����֮��ľ��롣
    /// </summary>
    WayPoint,

    /// <summary>
    /// ��ɫλ�õ���һ��ת���֮���λ�á�
    /// ת���������ƶ��켣�е�����λ�ã���һ���Ǹ�����·���е�·���㡣
    /// </summary>
    SteeringPoint
}


[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class RootMotionNavMeshAgent : MonoBehaviour
{
    /// <summary>
    /// ����ɫ��ǰ������Ŀ�곯��֮��ļн�С�ڴ˽Ƕȣ���ʹ��ɫֹͣת��
    /// </summary>
    public const float RotateStopAngle = 1f;

    /// <summary>
    /// �����Ƿ񼤻������
    /// </summary>
    public bool Enabled
    {
        get
        {
            return enabled;
        }
        set
        {
            if (enabled && value == false)
            {
                StopMoving();
            }

            enabled = value;
        }
    }

    /// <summary>
    /// Ŀ�곯��
    /// </summary>
    public Vector3? TargetForward { get; private set; }

    /// <summary>
    /// Ŀ�ĵء�
    /// </summary>
    public Vector3? Destination { get; private set; }

    /// <summary>
    /// ����·���㣨�� <see cref="NavMeshAgent"/> ����ó�����
    /// </summary>
    public IEnumerable<Vector3> NavPathCorners { get { return _navMeshAgent.path.corners; } }

    /// <summary>
    /// �ƶ�·���㣨���ⲿָ������
    /// </summary>
    public IEnumerable<Vector3> MovingPath { get { return _movingPath; } }

    /// <summary>
    /// ת���ٶȣ��Ƕ�/�룩��
    /// </summary>
    public float AngularSpeed
    {
        get { return _navMeshAgent.angularSpeed; }
        set { _navMeshAgent.angularSpeed = value; }
    }

    /// <summary>
    /// ����ɫ��ǰλ����Ŀ�ĵصľ��루<see cref="GetRemainingLinearDistance"/>��С�ڴ�ֵ���ף�������Ϊ����Ŀ�ĵأ�ʹ��ɫֹͣ�ƶ���
    /// </summary>
    public float StoppingDistance
    {
        get { return _navMeshAgent.stoppingDistance; }
        set { _navMeshAgent.stoppingDistance = value; }
    }

    /// <summary>
    /// ����ɫ��ǰλ����Ŀ�ĵصľ��루<see cref="GetRemainingLinearDistance"/>��С�ڴ�ֵ���ף�������Ϊ�ӽ�Ŀ��㣬ʹ��ɫ��ʼ���١�
    /// ���ظ���·�����ƶ������ڽӽ�Ŀ���ʱ��ʹ��ɫ��ʼ��·���е���һ��Ŀ����ƶ���
    /// </summary>
    public float ApproachDistance
    {
        get { return _approachDistance; }
        set { _approachDistance = value; }
    }

    /// <summary>
    /// ���ƽ�ɫ�ƶ��Ķ����������ơ�
    /// </summary>
    public string AnimationLocomotionParam
    {
        get { return _animationLocomotionParam; }
        set
        {
            _animationLocomotionParam = value;
            _animationLocomotionParamHash = Animator.StringToHash(_animationLocomotionParam);
        }
    }

    /// <summary>
    /// ʹ��ɫԭ��վ���Ķ�������ֵ��
    /// </summary>
    public float AnimationStandingValue
    {
        get { return _animationStandingValue; }
        set { _animationStandingValue = value; }
    }

    /// <summary>
    /// ʹ��ɫ��ǰ�ƶ��Ķ�������ֵ��
    /// </summary>
    public float AnimationMovingValue
    {
        get { return _animationMovingValue; }
        set { _animationMovingValue = value; }
    }

    /// <summary>
    /// ʹ��ɫ��ǰ�ƶ��Ķ���������Сֵ��
    /// </summary>
    public float AnimationMovingMinValue
    {
        get { return _animationMovingMinValue; }
        set { _animationMovingMinValue = value; }
    }

    /// <summary>
    /// ��ɫ�ƶ�����������Dampʱ�����룩��
    /// </summary>
    public float AnimationLocomotionValueDampTime
    {
        get { return _animationLocomotionValueDampTime; }
        set { _animationLocomotionValueDampTime = value; }
    }

    /// <summary>
    /// ���ݸ�������NavMesh�ϲ���λ��ʱ������ʹ�õ����ƫ�ƾ��루�ף���
    /// </summary>
    public float MaxNavMeshSampleDistance
    {
        get { return _maxNavMeshSampleDistance; }
        set { _maxNavMeshSampleDistance = value; }
    }

    /// <summary>
    /// ��NavMesh�ϲ���λ��ʱ��ʹ�õ��������֡�
    /// </summary>
    public int NavMeshAreaMask
    {
        get { return _navMeshAreaMask; }
        set { _navMeshAreaMask = value; }
    }


    /// <summary>
    /// ���ƽ�ɫ�ƶ��Ķ����������ơ�
    /// </summary>
    [SerializeField]
    private string _animationLocomotionParam = "F_MoveSpeed";

    /// <summary>
    /// ʹ��ɫԭ��վ���Ķ�������ֵ��
    /// </summary>
    [SerializeField]
    private float _animationStandingValue = 0.0f;

    /// <summary>
    /// ʹ��ɫ��ǰ�ƶ��Ķ�������ֵ��
    /// </summary>
    [SerializeField]
    private float _animationMovingValue = 1.0f;

    /// <summary>
    /// ʹ��ɫ��ǰ�ƶ��Ķ���������Сֵ��
    /// </summary>
    [SerializeField]
    private float _animationMovingMinValue = 0.3f;

    /// <summary>
    /// ��ɫ�ƶ�����������Dampʱ�����룩��
    /// </summary>
    [Range(0f, 0.5f)]
    [SerializeField]
    private float _animationLocomotionValueDampTime = 0.1f;

    /// <summary>
    /// ���ݸ�������NavMesh�ϲ���λ��ʱ������ʹ�õ����ƫ�ƾ��루�ף���
    /// </summary>
    [SerializeField]
    private float _maxNavMeshSampleDistance = 0.5f;

    /// <summary>
    /// ��NavMesh�ϲ���λ��ʱ��ʹ�õ��������֡�
    /// </summary>
    [SerializeField]
    private int _navMeshAreaMask = NavMesh.AllAreas;

    /// <summary>
    /// ����ɫ��ǰλ����Ŀ�ĵصľ��루<see cref="GetRemainingLinearDistance"/>��С�ڴ�ֵ���ף�������Ϊ�ӽ�Ŀ��㣬ʹ��ɫ��ʼ���١�
    /// ���ظ���·�����ƶ������ڽӽ�Ŀ���ʱ��ʹ��ɫ��ʼ��·���е���һ��Ŀ����ƶ���
    /// </summary>
    [SerializeField]
    private float _approachDistance = 0.8f;

    /// <summary>
    /// ��������
    /// </summary>
    private Animator _animator;

    /// <summary>
    /// ��������
    /// </summary>
    private NavMeshAgent _navMeshAgent;

    /// <summary>
    /// �ƶ�·���㣨���ⲿָ������
    /// </summary>
    private readonly Queue<Vector3> _movingPath = new Queue<Vector3>();

    /// <summary>
    /// ת��ֹͣʱ�Ļص���
    /// ����1���Ƿ���ת��Ŀ�귽��
    /// </summary>
    private Action<bool> _rotateStopCallback;

    /// <summary>
    /// �ƶ�ֹͣʱ�Ļص���
    /// ����1���Ƿ�ִ�Ŀ�ĵء�
    /// </summary>
    private Action<bool> _moveStopCallback;

    /// <summary>
    /// ���ƽ�ɫ�ƶ��Ķ�������Hash��
    /// </summary>
    private int _animationLocomotionParamHash;

    /// <summary>
    /// �������ƶ�����ֵ��
    /// </summary>
    private float _animationLocomotionValue;

    /// <summary>
    /// �������ƶ�����ֵDamp�����ֶΡ�
    /// </summary>
    private float _animationLocomotionValueDampVelocity;

    // ���Դ���
    //[SerializeField]
    //private List<Vector3> _pathBuffer = new List<Vector3>();



    /// <summary>
    /// ���ý�ɫ����
    /// </summary>
    /// <param name="forward">Ŀ�곯��</param>
    /// <param name="stopMoving">�Ƿ�ֹͣ���ڽ��еĵ�����</param>
    /// <param name="onRotateStop">ת��ֹͣʱ�Ļص�������1���Ƿ���ת��Ŀ�귽��</param>
    /// <returns></returns>
    public bool SetForward(Vector3 forward, bool stopMoving = false, Action<bool> onRotateStop = null)
    {
        StopRotating();

        forward.y = 0;

        if (forward.sqrMagnitude < Mathf.Epsilon)
        {
            Debug.LogError("ERROR: Can't set forward to zero.");
            onRotateStop?.Invoke(true);
            return false;
        }

        if (Destination.HasValue)
        {
            if (stopMoving)
            {
                StopMoving();
            }
            else
            {
                Debug.LogError("ERROR: Can't set forward during the navigation.");
                onRotateStop?.Invoke(false);
                return false;
            }
        }

        TargetForward = forward.normalized;
        _rotateStopCallback = onRotateStop;

        return true;
    }

    /// <summary>
    /// ����Ŀ�ĵء�
    /// </summary>
    /// <param name="destination">Ŀ�ĵء�</param>
    /// <param name="onMovingStop">����ֹͣʱ�Ļص�������1���Ƿ񵽴�Ŀ�ĵء�</param>
    /// <returns></returns>
    public bool SetDestination(Vector3 destination, Action<bool> onMovingStop = null)
    {
        StopMoving();

        if (_navMeshAgent.SetDestination(destination))
        {
            // ע�ⲻҪֱ��ʹ�� destination �������ܲ���NavMesh��
            Destination = _navMeshAgent.destination;
            _moveStopCallback = onMovingStop;

            return true;
        }

        onMovingStop?.Invoke(false);

        return false;
    }

    /// <summary>
    /// �����ƶ�·����
    /// </summary>
    /// <param name="wayPoints">�ƶ�·����</param>
    /// <param name="onMovingStop">����ֹͣʱ�Ļص�������1���Ƿ񵽴�Ŀ�ĵء�</param>
    public void SetPath(IEnumerable<Vector3> wayPoints, Action<bool> onMovingStop = null)
    {
        StopMoving();

        _movingPath.Clear();
        foreach (var wayPoint in wayPoints)
        {
            _movingPath.Enqueue(wayPoint);
        }

        if (_movingPath.Count == 0)
        {
            onMovingStop?.Invoke(true);
            return;
        }

        while (_movingPath.Count > 0)
        {
            Destination = _movingPath.Dequeue();
            if (_navMeshAgent.SetDestination(Destination.Value))
            {
                Destination = _navMeshAgent.destination;
                _moveStopCallback = onMovingStop;
                return;
            }

            Debug.LogError($"ERROR: Skip unreachable moving path point `{Destination.Value}`.", gameObject);
        }

        Debug.LogError("ERROR: There is no reachable point in path.", gameObject);

        // ����·�����ڵ��������϶����ɴ�
        Destination = null;
        onMovingStop?.Invoke(false);
    }

    /// <summary>
    /// ֹͣת��
    /// </summary>
    public void StopRotating()
    {
        if (!TargetForward.HasValue)
        {
            return;
        }

        if (_rotateStopCallback == null)
        {
           //Debug.LogError(Vector3.Angle(transform.forward, TargetForward.Value) < RotateStopAngle ? "### ת�����" : "### ת���ж�");

           TargetForward = null;
            return;
        }

        var deflectionAngle = Vector3.Angle(transform.forward, TargetForward.Value);
        var tempRotateStopCallback = _rotateStopCallback;

       //Debug.LogError(deflectionAngle < RotateStopAngle ? "### ת�����" : "### ת���ж�");

       _rotateStopCallback = null;
        TargetForward = null;
        tempRotateStopCallback(deflectionAngle < RotateStopAngle);
    }

    /// <summary>
    /// ֹͣ�ƶ���
    /// </summary>
    public void StopMoving()
    {
        if (!Destination.HasValue)
        {
            return;
        }

        if (_moveStopCallback == null)
        {
           //if (GetRemainingLinearSqrDistance(RemainingDistanceType.PathEnd) > StoppingDistance * StoppingDistance)
           //{
           //    Debug.LogError("### NavStop: δ����");
           //}
           //else
           //{
           //    Debug.LogError("### NavStop: ����");
           //}

           Destination = null;
            _movingPath.Clear();
            _navMeshAgent.ResetPath();

            return;
        }

        var tempNavStopCallback = _moveStopCallback;
        _moveStopCallback = null;
        _navMeshAgent.ResetPath();

        if (GetRemainingLinearSqrDistance(RemainingDistanceType.PathEnd) > StoppingDistance * StoppingDistance)
        {
           //Debug.LogError("### NavStop: δ����");

           Destination = null;
            _movingPath.Clear();
            tempNavStopCallback(false);
        }
        else
        {
           //Debug.LogError("### NavStop: ����");

           Destination = null;
            _movingPath.Clear();
            tempNavStopCallback(true);
        }
    }

    /// <summary>
    /// �ƶ���ɫλ�á�
    /// ����ɫ��δֹͣ�ƶ�����������Ҫ�� LateUpdate �����е��ô˷�����
    /// </summary>
    /// <param name="targetPosition">��ɫĿ��λ�á�</param>
    /// <param name="stopMoving">�Ƿ�ֹͣ���ڽ��еĵ�����</param>
    /// <returns></returns>
    public bool MoveCharacter(Vector3 targetPosition, bool stopMoving = false)
    {
        if (NavMesh.SamplePosition(targetPosition, out var hit, MaxNavMeshSampleDistance, NavMeshAreaMask))
        {
            transform.position = hit.position;

            // �����������ᵼ��NavMeshAgent����;���ϰ��ﵲס
            //_navMeshAgent.nextPosition = hit.position;
            //_navMeshAgent.Move(hit.position - transform.position);

            if (stopMoving)
            {
                _navMeshAgent.Warp(hit.position);
                StopMoving();
            }
            else if (Destination.HasValue)
            {
                // Warp�����ᵼ�µ���ֹͣ
                _navMeshAgent.Warp(hit.position);
                _navMeshAgent.SetDestination(Destination.Value);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// ��ȡ��ɫ��ǰ����λ�õ�Ŀ�ĵص�ֱ�߾��루�ף���
    /// ע��˷������صĲ��� <see cref="NavMeshAgent.remainingDistance"/> ������֮������
    /// </summary>
    /// <param name="distanceType">����ʣ�����ķ�ʽ��</param>
    /// <returns></returns>
    public float GetRemainingLinearDistance(RemainingDistanceType distanceType)
    {
        return Mathf.Sqrt(GetRemainingLinearSqrDistance(distanceType));
    }

    /// <summary>
    /// ��ȡ��ɫ��ǰ����λ�õ�Ŀ�ĵص�ֱ�߾��루�ף���ƽ����
    /// ע��˷������صĲ��� <see cref="NavMeshAgent.remainingDistance"/> ��ƽ��ֵ������֮������
    /// </summary>
    /// <param name="distanceType">����ʣ�����ķ�ʽ��</param>
    /// <returns></returns>
    public float GetRemainingLinearSqrDistance(RemainingDistanceType distanceType)
    {
        if (Destination.HasValue)
        {
            switch (distanceType)
            {
                case RemainingDistanceType.PathEnd:
                    if (_movingPath.Count > 0)
                    {
                        return Vector3.SqrMagnitude(transform.position - _movingPath.Last());
                    }
                    else
                    {
                        return Vector3.SqrMagnitude(transform.position - Destination.Value);
                    }

                case RemainingDistanceType.WayPoint:
                    return Vector3.SqrMagnitude(transform.position - Destination.Value);

                case RemainingDistanceType.SteeringPoint:
                    return Vector3.SqrMagnitude(transform.position - _navMeshAgent.steeringTarget);

                default:
                    throw new ArgumentOutOfRangeException(nameof(distanceType), distanceType, null);
            }
        }

        return 0;
    }


    private void Reset()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();

        if (ApproachDistance < StoppingDistance)
        {
            ApproachDistance = StoppingDistance + 1e-5f;
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            _animationLocomotionParamHash = Animator.StringToHash(AnimationLocomotionParam);
        }

        if (AnimationMovingMinValue < AnimationStandingValue)
        {
            AnimationMovingMinValue = AnimationStandingValue;
        }

        _navMeshAgent = GetComponent<NavMeshAgent>();
        if (ApproachDistance < StoppingDistance)
        {
            ApproachDistance = StoppingDistance + 1e-5f;
        }
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _animationLocomotionParamHash = Animator.StringToHash(AnimationLocomotionParam);

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.updatePosition = false;
        _navMeshAgent.updateRotation = false; // ����ת��̫�����ֶ�ת��

        if (ApproachDistance < StoppingDistance)
        {
            ApproachDistance = StoppingDistance + 1e-5f;
        }
    }


    public void SetCanUpDatePosition(bool canUpDate) {
        _navMeshAgent.updatePosition = canUpDate;
    }

    bool isLife = true;
    public void SetIsLife(bool islife)
    {
        this.isLife = islife;
        _navMeshAgent.enabled = false;
    }

    public void SetStopDistance(float distance)
    {
        _navMeshAgent.stoppingDistance = distance;
        ApproachDistance = distance;
    }

    public void CleanCallBack() {
        _moveStopCallback = null;
    }


    private void Update()
    {
        if (isLife==false) {
            return;
        }
        // �����ɫת��
        RotateCharacter(out float deflectionAngle);

        // �����Ŀ�ĵأ�ֹͣ�ƶ�
        if (!Destination.HasValue)
        {
            SetAnimationLocomotionValueWithDamp(AnimationStandingValue);
            return;
        }

        // ����ʣ�����
        var remainingSqrDistance = GetRemainingLinearSqrDistance(RemainingDistanceType.WayPoint);

        // ���ȴ���·������Ϊ·��Ҳ��ʹ�� NavDestination ����
        if (_movingPath.Count > 0)
        {
            if (remainingSqrDistance < ApproachDistance * ApproachDistance)
            {
                var hasDestination = false;
                while (_movingPath.Count > 0)
                {
                    Destination = _movingPath.Dequeue();
                    if (_navMeshAgent.SetDestination(Destination.Value))
                    {
                        Destination = _navMeshAgent.destination;
                        hasDestination = true;
                        break;
                    }

                    Debug.LogError($"ERROR: Skip unreachable moving path point `{Destination.Value}`.");
                }

                // ����·�����ڵ��������϶����ɴֹͣ����
                if (!hasDestination)
                {
                    StopMoving();
                    return;
                }
            }
        }
        // ���Ŵ�����Ŀ�ĵ�
        // �ѵ���Ŀ�ĵ�
        else if (remainingSqrDistance < StoppingDistance * StoppingDistance)
        {
            StopMoving();

            return;
        }

        // ����ת���ٶ�˥���ͽӽ�Ŀ�����ٶ�˥��
        var animationMovingValue = Mathf.Min(DampMovingSpeedByDeflection(AnimationMovingValue, deflectionAngle),
            DampMovingSpeedByRemainingDistance());
        //var animationMovingValue = DampMovingSpeedByDeflection(AnimationMovingValue, deflectionAngle);

        // �����ƶ�����
        SetAnimationLocomotionValueWithDamp(animationMovingValue);

    }

    private void OnAnimatorMove()
    {
        // ��ʹ�õ����߶ȣ�ʹ��ɫ���ϵ�������
        var pos = _animator.rootPosition;
        var rotate = _animator.rootRotation;
        if (isLife)
        {
            pos.y = _navMeshAgent.nextPosition.y;
            // ʹ�������ϸ��˶�
            _navMeshAgent.nextPosition = transform.position;

            // ʹ�ø��˶��ٶ����������ٶ�
            _navMeshAgent.velocity = _animator.velocity;
        }
        else {
            transform.rotation = rotate;
        }

        transform.position = pos;

       

    }



    /// <summary>
    /// �����ɫת��
    /// </summary>
    /// <param name="deflectionAngle"></param>
    private void RotateCharacter(out float deflectionAngle)
    {
        deflectionAngle = 0;

        // ����Ŀ�ĵأ�������һ��Ҫ�����·����
        if (Destination.HasValue)
        {
            var targetForward = _navMeshAgent.steeringTarget - transform.position;
            targetForward.y = 0;
            targetForward.Normalize();

            // ����ת��
            transform.forward = Vector3.RotateTowards(transform.forward, targetForward,
                Time.deltaTime * AngularSpeed * Mathf.Deg2Rad, float.MaxValue);

            deflectionAngle = Vector3.Angle(transform.forward, targetForward);
        }
        // ������ָ���ķ���
        else if (TargetForward.HasValue)
        {
            // ����ת��
            transform.forward = Vector3.RotateTowards(transform.forward, TargetForward.Value,
                Time.deltaTime * AngularSpeed * Mathf.Deg2Rad, float.MaxValue);

            deflectionAngle = Vector3.Angle(transform.forward, TargetForward.Value);
            if (deflectionAngle < RotateStopAngle)
            {
                //Debug.LogError("### ת�����");

                if (_rotateStopCallback == null)
                {
                    TargetForward = null;
                }
                else
                {
                    var tempRotateStopCallback = _rotateStopCallback;
                    _rotateStopCallback = null;
                    TargetForward = null;
                    tempRotateStopCallback(true);
                }
            }
        }
    }

    /// <summary>
    /// ����ɫ��ǰ������Ŀ���ƶ������ƫת�Ǵ����ƶ�����˥����
    /// </summary>
    /// <param name="originalSpeed">˥��ǰ���ƶ����ʣ���/�룩��</param>
    /// <param name="deflectionAngle">ƫת�ǣ��Ƕȣ���</param>
    /// <returns></returns>
    private float DampMovingSpeedByDeflection(float originalSpeed, float deflectionAngle)
    {
        deflectionAngle = Mathf.Abs(deflectionAngle);

        // ����EaseOutIn˥��
        //var progress = 1 - deflectionAngle / 180;
        //var dampCoef = progress < 0.5f
        //    ? progress * 2 - Mathf.Pow(2, 3 - 1) * Mathf.Pow(progress, 3) // 2^(n-1)*(progress^n)
        //    : progress * 2 - (1 - Mathf.Pow(-2 * progress + 2, 3) / 2); // 1-((-2*x+2)^n)/2

        // ����˥��
        var dampCoef = 1 - deflectionAngle / 180;

        var dampedSpeed = originalSpeed * dampCoef;

        return dampedSpeed;
    }

    /// <summary>
    /// ����ɫ��ǰ���뵼��Ŀ���ľ����������ƶ�����˥����
    /// </summary>
    /// <returns></returns>
    private float DampMovingSpeedByRemainingDistance()
    {
        if (ApproachDistance < StoppingDistance)
        {
            Debug.LogError($"ERROR: {nameof(StoppingDistance)}({StoppingDistance}) should be less than {nameof(ApproachDistance)}({ApproachDistance}).", gameObject);
        }

        // ע�⣬Ӧ�ø��ݽ�ɫ�ƶ��ٶ������������ٶȼ������

        var remainingSqrDistance = GetRemainingLinearSqrDistance(RemainingDistanceType.WayPoint);
        if (remainingSqrDistance < StoppingDistance * StoppingDistance)
        {
            return 0;
        }

        if (remainingSqrDistance > ApproachDistance * ApproachDistance)
        {
            return AnimationMovingValue;
        }

        // ֻ�ڽӽ��յ������;·����ʱ���м��٣��ɵ������ɵ�steeringTarget�������ٴ�����Ϊ�߹�ͷ��ҲûɶӰ�죩

        // �Ĵη�EaseOutIn˥��
        var remainingDistance = Mathf.Sqrt(remainingSqrDistance);
        var progress = remainingDistance / ApproachDistance;
        var dampCoef = progress < 0.5f
            ? progress * 2 - Mathf.Pow(2, 4 - 1) * Mathf.Pow(progress, 4) // 2^(n-1)*(progress^n)
            : progress * 2 - (1 - Mathf.Pow(-2 * progress + 2, 4) / 2); // 1-((-2*x+2)^n)/2

        // ����˥��
        //var dampCoef = remainingDistance / approachDistance;

        // ����ӽ�Ŀ���ʱ���޼���
        var dampedSpeed = Mathf.Max(AnimationMovingValue * dampCoef, AnimationMovingMinValue);

        return dampedSpeed;
    }

    /// <summary>
    /// �Դ���Damp����ʽ���ö������ƶ�������
    /// ��Ҫÿ֡���ò���ʹDamp��Ч��
    /// </summary>
    /// <param name="value"></param>
    private void SetAnimationLocomotionValueWithDamp(float value)
    {
        _animationLocomotionValue = Mathf.SmoothDamp(_animationLocomotionValue, value,
            ref _animationLocomotionValueDampVelocity, AnimationLocomotionValueDampTime);

        _animator.SetFloat(_animationLocomotionParamHash, _animationLocomotionValue);
    }
}

