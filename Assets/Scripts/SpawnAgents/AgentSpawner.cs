using UnityEngine;

public class AgentSpawner : MonoBehaviour
{
    [Header("Cofiguracion de los agentes")]
    public GameObject Agent;

    [Header("Cantidad de agentes")]
    [SerializeField] private int numberOfAgents = 6;

    [Header("Area de spawn de los agentes")]
    [SerializeField] public float spawnAreaWidth = 20f;
    [SerializeField] public float spawnAreaHeight = 20f;
}
