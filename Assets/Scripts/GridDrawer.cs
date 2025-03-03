using UnityEngine;

public class GridDrawer : MonoBehaviour
{
    public int gridSize = 10;  // Setengah jumlah grid di setiap arah
    public float cellSize = 1.0f;  // Ukuran setiap sel
    public float lineWidth = 0.05f;  // Ketebalan garis
    public float dotSpacing = 0.2f;  // Jarak antar titik
    public Material lineMaterial;  // Material garis
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DrawDottedGrid();
    }

    // Update is called once per frame
    void DrawDottedGrid()
    {
        GameObject grid = new GameObject("DottedGrid");

        for (int i = -gridSize; i <= gridSize; i++)
        {
            // Garis vertikal (X tetap, Z bervariasi)
            DrawDottedLine(new Vector3(i * cellSize, 0, -gridSize * cellSize), new Vector3(i * cellSize, 0, gridSize * cellSize), grid);

            // Garis horizontal (Z tetap, X bervariasi)
            DrawDottedLine(new Vector3(-gridSize * cellSize, 0, i * cellSize), new Vector3(gridSize * cellSize, 0, i * cellSize), grid);
        }
    }

    void DrawDottedLine(Vector3 start, Vector3 end, GameObject parent)
    {
        float distance = Vector3.Distance(start, end);  // Panjang total garis
        int segments = Mathf.CeilToInt(distance / dotSpacing);  // Jumlah titik
        Vector3 direction = (end - start).normalized;  // Arah garis

        for (int i = 0; i <= segments; i++)
        {
            Vector3 dotPosition = start + direction * (i * dotSpacing);

            GameObject dot = new GameObject("Dot");
            dot.transform.parent = parent.transform;

            LineRenderer lr = dot.AddComponent<LineRenderer>();
            lr.material = lineMaterial;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.startColor = Color.black;
            lr.endColor = Color.black;  
            lr.positionCount = 2;  // Titik kecil menggunakan garis pendek
            lr.SetPosition(0, dotPosition);
            lr.SetPosition(1, dotPosition + Vector3.up * 0.01f);  // Garis vertikal kecil (titik)
        }
    }
}
