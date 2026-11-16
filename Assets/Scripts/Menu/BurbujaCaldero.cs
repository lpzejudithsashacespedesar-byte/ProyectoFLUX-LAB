using UnityEngine;

namespace FluxLab
{
    public class BurbujaCaldero : MonoBehaviour
    {
        public float velocidad = 28f;
        public float amplitud = 12f;

        private RectTransform rect;
        private float posicionInicialX;
        private float fase;

        void Start()
        {
            rect = GetComponent<RectTransform>();
            posicionInicialX = rect.anchoredPosition.x;
            fase = Random.Range(0f, Mathf.PI * 2f);
        }

        void Update()
        {
            if (rect == null)
                return;

            Vector2 posicion = rect.anchoredPosition;

            posicion.y += velocidad * Time.deltaTime;
            posicion.x = posicionInicialX +
                Mathf.Sin(Time.time * 1.5f + fase) * amplitud;

            if (posicion.y > 120f)
            {
                posicion.y = Random.Range(-175f, -165f);
                posicionInicialX = Random.Range(345f, 455f);
                posicion.x = posicionInicialX;
            }

            rect.anchoredPosition = posicion;
        }
    }
}