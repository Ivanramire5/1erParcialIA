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

    private void Start()
    {
        SpawnAgents();
    }

    private void SpawnAgents()
    {
        GameObject piso = GameObject.FindGameObjectWithTag("Suelo");
        float alturaDeSpawn = 0f;

        if (piso != null)
        {
            
            Collider colisionadorSuelo = piso.GetComponent<Collider>();
            if (colisionadorSuelo != null)
            {

                alturaDeSpawn = colisionadorSuelo.bounds.max.y + 0.1f; 
            }
            else
            {
                alturaDeSpawn = piso.transform.position.y + 0.1f;
            }
        }
        else
        {
            Debug.LogWarning("No se encontró el tag 'Suelo'. Spawneando en Y = 0.1f");
            alturaDeSpawn = 0.1f;
        }

        // 3. Spawneamos a los agentes
        for (int i = 0; i < numberOfAgents; i++)
        {
            float randomX = Random.Range(-spawnAreaWidth / 2f, spawnAreaWidth / 2f);
            float randomZ = Random.Range(-spawnAreaHeight / 2f, spawnAreaHeight / 2f);
            
            
            Vector3 spawnPosition = new Vector3(randomX, alturaDeSpawn, randomZ);

            Instantiate(Agent, spawnPosition, Quaternion.identity);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnAreaWidth, 0f, spawnAreaHeight));
    }
}
