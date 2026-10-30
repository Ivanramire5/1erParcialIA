using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WolfAgent : MonoBehaviour, IGizmoProvider
{
    [Header("Stats")]
    [SerializeField] private float _speed = 4f;
    public float Speed => _speed;

    [Header("Percepcion y referencias")]
    [SerializeField] private Transform _hunterTarget; 
    public Transform HunterTarget => _hunterTarget;

    private FSMAgent _hunter;
    [SerializeField] private LayerMask _wallMask; 
    public LayerMask WallMask => _wallMask;       

    private VisionSensor _visionSensor;
    private ColorController _colorController;
    [Header("Feedback Visual")]


    [Header("Patrulla")]
    public List<Transform> waypoints;
    public float WaypointCheckDistance = 2f;

    public static Vector3? LastKnownHunterPosition { get; private set; }
    public static bool IsAlerted { get; private set; }

    public List<Node> CurrentActivePath { get; set; } = new();

    private readonly FiniteStateMachine _fsm = new();
    public FiniteStateMachine FSM => _fsm;
    
    public WolfPatrolState PatrolState { get; private set; }
    public WolfChaseState ChaseState { get; private set; }
    public WolfInvestigateState InvestigateState { get; private set; }

    private void Awake()
    {
       
        _colorController = GetComponent<ColorController>();
        _visionSensor = GetComponentInChildren<VisionSensor>();

        
    }

    private void Start()
    {
        PatrolState = new WolfPatrolState(this);
        ChaseState = new WolfChaseState(this);
        InvestigateState = new WolfInvestigateState(this);
     
        _fsm.AddState(PatrolState);
        _fsm.AddState(ChaseState);
        _fsm.AddState(InvestigateState);

        _fsm.ChangeState(PatrolState);
    }

    private void Update()
    {
        _fsm.Update();
    }

    public void Howl(Vector3 hunterPos)
    {
        LastKnownHunterPosition = hunterPos;
        IsAlerted = true;
       
        Debug.Log("¡AULLIDO DE LOBO! ¡Cazador detectado en " + hunterPos + "!");
    }

    public void SetColor(Color newColor)
    {
        if (_colorController != null)
        {
            _colorController.SetColor(newColor);
        }
      
    }
    public void SetHunterTarget(Transform newTarget)
    {
        _hunterTarget = newTarget;
    }

    public static void ResetAlert()
    {
        IsAlerted = false;
        LastKnownHunterPosition = null;
    }
  
    public bool DetectHunter(out Vector3 hunterPos)
    {
       
        hunterPos = default;
        if (_visionSensor == null) return false;

        Transform closestHunter = _visionSensor.GetClosestVisibleHunter(transform.position);

        if (closestHunter != null)
        {
           
            FSMAgent hunterComponent = closestHunter.GetComponentInParent<FSMAgent>();

      
            if (hunterComponent != null && hunterComponent.IsDead)
            {
                return false;
            }

            hunterPos = closestHunter.position;
            SetHunterTarget(closestHunter);
            return true;
        }

        return false;
    }

   
    public bool IsHunterInSafeZone()
    {
        if (HunterTarget == null)
        {
            GameObject hunterObj = GameObject.FindWithTag("Hunter");
            if (hunterObj != null) SetHunterTarget(hunterObj.transform) ;
        }

        if (HunterTarget != null && Graph.Current != null)
        {
            Node node = Graph.Current.GetClosestNode(HunterTarget.position);
            if (node != null && node.IsSafeZone) return true;
        }
        return false;
    }

    public IEnumerable<GizmoData> GetGizmosToDraw()
    {
        if (_visionSensor == null)
        {
            _visionSensor = GetComponentInChildren<VisionSensor>();
        }

        if (_visionSensor != null)
        {
            yield return new GizmoData(_visionSensor.ViewRadius, _visionSensor.ViewAngle, Color.cyan);
        }

        if (CurrentActivePath != null && CurrentActivePath.Count > 0)
        {
            List<Vector3> pathPositions = new List<Vector3>();
            foreach (var node in CurrentActivePath)
            {
                if (node != null) pathPositions.Add(node.transform.position);
            }
            yield return new GizmoData(pathPositions, Color.yellow);
        }
    }

}