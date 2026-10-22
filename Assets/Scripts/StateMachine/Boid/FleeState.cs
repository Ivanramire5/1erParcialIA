using UnityEngine;

public class FleeState : State
{
    private Boid _boid;
    private Transform _currentHunter;

    public FleeState(Boid boid) { _boid = boid; }

    public override void Enter()
    {
        _boid.SetColor(Color.cyan);

        // Al asustarnos, guardamos en la memoria quién es el cazador
        _currentHunter = _boid.Sensor.GetClosestVisibleHunter(_boid.transform.position);
    }

    public override void Update()
    {
        if (_boid.IsDead) return;

        // Si por alguna razón el cazador es nulo (ej. fue destruido o desactivado)
        // intentamos buscar uno nuevo en la visión.
        if (_currentHunter == null || !_currentHunter.gameObject.activeInHierarchy)
        {
            _currentHunter = _boid.Sensor.GetClosestVisibleHunter(_boid.transform.position);

            // Si definitivamente no hay nadie, volvemos a la calma
            if (_currentHunter == null)
            {
                _boid.FSM.ChangeState(_boid.StateFlock);
                return;
            }
        }

        // Calculamos la distancia lineal hacia el cazador recordado
        float distanceToHunter = Vector3.Distance(_boid.transform.position, _currentHunter.position);

        // CONDICIÓN DE SALIDA: Si ya corrimos lo suficiente y estamos fuera de su radio
        // de visión/peligro, dejamos de huir.
        if (distanceToHunter > _boid.ViewRadius)
        {
            _boid.FSM.ChangeState(_boid.StateFlock);
            return;
        }

        // Mientras siga cerca, continuamos huyendo de su posición
        Vector3 steering = _boid.CalculateFlee(_currentHunter.position)
                         + _boid.CalculateSeparation(Boid.AllAgents, _boid.SeparationRadius) * 1.5f;

        _boid.ApplyMovement(steering);
    }

    public override void Exit()
    {
        _currentHunter = null;
    }
}