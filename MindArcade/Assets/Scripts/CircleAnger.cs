using UnityEngine;
using UnityEditor;

public class CircleArranger : EditorWindow
{
    private float radius = 5f;

    [MenuItem("Tools/Nesneleri Daire Yap")]
    public static void ShowWindow()
    {
        GetWindow<CircleArranger>("Daire Dizici");
    }

    private void OnGUI()
    {
        GUILayout.Label("Çember Ayarları", EditorStyles.boldLabel);
        radius = EditorGUILayout.FloatField("Çember Yarıçapı (Radius):", radius);

        if (GUILayout.Button("Seçili Nesneleri Çembere Diz"))
        {
            ArrangeInCircle();
        }
    }

    private void ArrangeInCircle()
    {
        // Sahne veya Hierarchy penceresinde seçili olan objeleri al
        Transform[] selectedTransforms = Selection.transforms;
        int count = selectedTransforms.Length;

        if (count == 0)
        {
            Debug.LogWarning("Lütfen önce çembere dizmek istediğiniz nesneleri seçin!");
            return;
        }

        // Çemberin merkez noktasını seçilen objelerin ortalaması olarak belirle
        Vector3 center = Vector3.zero;
        foreach (Transform t in selectedTransforms)
        {
            center += t.position;
        }
        center /= count;

        // Görseldeki gibi üstten görünüm (X ve Z ekseni) için dizilim yap
        for (int i = 0; i < count; i++)
        {
            // Dereceyi radyana çeviriyoruz: (2 * PI * i) / Toplam Nesne
            float angle = i * 2 * Mathf.PI / count;

            float x = center.x + Mathf.Cos(angle) * radius;
            float z = center.z + Mathf.Sin(angle) * radius;

            // Ctrl+Z ile geri alınabilmesi için Undo kaydı oluşturuyoruz
            Undo.RecordObject(selectedTransforms[i], "Circle Arrange");

            // Yeni pozisyonu ata (Y eksenindeki yüksekliği korur)
            selectedTransforms[i].position = new Vector3(x, selectedTransforms[i].position.y, z);
            
            // İsteğe bağlı: Nesnelerin merkeze doğru bakmasını istersen alttaki satırı açabilirsin
            // selectedTransforms[i].LookAt(new Vector3(center.x, selectedTransforms[i].position.y, center.z));
        }
    }
}