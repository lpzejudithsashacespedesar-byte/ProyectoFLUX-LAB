using UnityEngine;
using UnityEngine.UI;

namespace FluxLab
{
    /// <summary>
    /// Luma, el búho del taller del aprendiz. Cambia de gesto como el grillo y el gato:
    /// parpadea, guiña, se duerme, canta, se asusta y festeja. El dibujo es uno solo;
    /// los párpados, el cuerpo y las chispas arman cada expresión.
    /// </summary>
    public class BuhoLuma : MonoBehaviour
    {
        enum Expresion
        {
            Quieto,
            Curioso,
            Habla,
            Feliz,
            Sorpresa,
            Asustado,
            Guino,
            Duerme,
            Canta
        }

        RectTransform cuerpo;
        RectTransform sombra;
        RectTransform parpadoIzq;
        RectTransform parpadoDer;
        Image[] chispas;
        Text voz;
        Text zzz;
        Image sudor;
        Vector2 pies;
        Vector2 tamCuerpo;
        Expresion expresion = Expresion.Quieto;
        float hasta;
        float animo;
        float salto;
        float mira = -1f;
        float miraSuave = -1f;
        float vozRestante;
        float parpadeo = 1.6f;
        float parpadeoCierra;
        float cierre;
        float cierreDer;
        bool forzada;

        public static BuhoLuma Crear(Transform padre, Vector2 pies, float alto)
        {
            Texture2D textura = Nivel3UI.LeerTextura("Companeros/luma.png");
            if (textura == null)
                return null;

            var raiz = new GameObject("Luma", typeof(RectTransform));
            raiz.transform.SetParent(padre, false);
            var buho = raiz.AddComponent<BuhoLuma>();
            buho.pies = pies;
            buho.Armar(textura, alto);
            buho.Poner(Expresion.Habla, 1.3f, true);
            buho.Decir("¡Hola!");
            return buho;
        }

        void Armar(Texture2D textura, float alto)
        {
            float ancho = alto * textura.width / Mathf.Max(1f, textura.height);
            tamCuerpo = new Vector2(ancho, alto);
            var sprite = Sprite.Create(textura, new Rect(0f, 0f, textura.width, textura.height), new Vector2(0.5f, 0.04f), 100f);

            var sombraGo = new GameObject("Sombra", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            sombraGo.transform.SetParent(transform, false);
            sombra = sombraGo.GetComponent<RectTransform>();
            sombra.anchorMin = sombra.anchorMax = new Vector2(0.5f, 0.5f);
            sombra.pivot = new Vector2(0.5f, 0.5f);
            sombra.anchoredPosition = pies + new Vector2(8f, 8f);
            sombra.sizeDelta = new Vector2(ancho * 0.62f, 34f);
            var mancha = sombraGo.GetComponent<Image>();
            mancha.sprite = Nivel3UI.LuzSuave();
            mancha.color = new Color(0.08f, 0.04f, 0.03f, 0.38f);
            mancha.raycastTarget = false;

            var cuerpoGo = new GameObject("Cuerpo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            cuerpoGo.transform.SetParent(transform, false);
            cuerpo = cuerpoGo.GetComponent<RectTransform>();
            cuerpo.anchorMin = cuerpo.anchorMax = new Vector2(0.5f, 0.5f);
            cuerpo.pivot = new Vector2(0.5f, 0.04f);
            cuerpo.anchoredPosition = pies;
            cuerpo.sizeDelta = tamCuerpo;
            var imagen = cuerpoGo.GetComponent<Image>();
            imagen.sprite = sprite;
            imagen.preserveAspect = true;
            imagen.raycastTarget = false;

            // Los ojos pintados quedan un poco arriba del centro. El párpado baja desde ahí.
            parpadoIzq = Parpado(cuerpo, "ParpadoIzq", new Vector2(-ancho * 0.15f, alto * 0.2f), new Vector2(ancho * 0.2f, alto * 0.13f));
            parpadoDer = Parpado(cuerpo, "ParpadoDer", new Vector2(ancho * 0.17f, alto * 0.2f), new Vector2(ancho * 0.2f, alto * 0.13f));

            sudor = Gota(cuerpo, new Vector2(ancho * 0.34f, alto * 0.28f));
            zzz = Letra(cuerpo, "Zzz", new Vector2(ancho * 0.28f, alto * 0.42f), 28, new Color(0.75f, 0.86f, 1f, 0f));

            chispas = new Image[7];
            for (int i = 0; i < chispas.Length; i++)
            {
                var luz = new GameObject("Chispa" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                luz.transform.SetParent(transform, false);
                var rect = luz.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(16f, 16f);
                var estrella = luz.GetComponent<Image>();
                estrella.sprite = Nivel3UI.LuzSuave();
                estrella.color = new Color(1f, 0.92f, 0.55f, 0f);
                estrella.raycastTarget = false;
                chispas[i] = estrella;
            }

            var vozGo = new GameObject("Voz", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Outline));
            vozGo.transform.SetParent(cuerpo, false);
            var marco = vozGo.GetComponent<RectTransform>();
            marco.anchorMin = marco.anchorMax = new Vector2(0.5f, 1f);
            marco.pivot = new Vector2(0.5f, 0f);
            marco.anchoredPosition = new Vector2(0f, 8f);
            marco.sizeDelta = new Vector2(320f, 54f);
            voz = vozGo.GetComponent<Text>();
            voz.font = Nivel3UI.Fuente;
            voz.fontSize = 32;
            voz.alignment = TextAnchor.MiddleCenter;
            voz.color = new Color(1f, 0.96f, 0.9f, 0f);
            voz.horizontalOverflow = HorizontalWrapMode.Overflow;
            voz.raycastTarget = false;
            var borde = vozGo.GetComponent<Outline>();
            borde.effectColor = new Color(0.2f, 0.08f, 0.04f, 0.9f);
            borde.effectDistance = new Vector2(2f, -2f);
        }

        RectTransform Parpado(Transform padre, string nombre, Vector2 centro, Vector2 tamano)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(padre, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = centro + new Vector2(0f, tamano.y * 0.45f);
            rect.sizeDelta = tamano;
            rect.localScale = new Vector3(1f, 0f, 1f);
            var imagen = go.GetComponent<Image>();
            imagen.sprite = Nivel3UI.LuzSuave();
            imagen.color = new Color(0.97f, 0.88f, 0.72f, 0.96f);
            imagen.raycastTarget = false;
            return rect;
        }

        Image Gota(Transform padre, Vector2 posicion)
        {
            var go = new GameObject("Sudor", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(padre, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = posicion;
            rect.sizeDelta = new Vector2(16f, 26f);
            var imagen = go.GetComponent<Image>();
            imagen.sprite = Nivel3UI.LuzSuave();
            imagen.color = new Color(0.55f, 0.82f, 1f, 0f);
            imagen.raycastTarget = false;
            return imagen;
        }

        Text Letra(Transform padre, string frase, Vector2 posicion, int tamano, Color color)
        {
            var go = new GameObject(frase, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Outline));
            go.transform.SetParent(padre, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = posicion;
            rect.sizeDelta = new Vector2(120f, 40f);
            var texto = go.GetComponent<Text>();
            texto.font = Nivel3UI.Fuente;
            texto.text = frase;
            texto.fontSize = tamano;
            texto.alignment = TextAnchor.MiddleCenter;
            texto.color = color;
            texto.raycastTarget = false;
            var borde = go.GetComponent<Outline>();
            borde.effectColor = new Color(0.15f, 0.22f, 0.4f, 0f);
            borde.effectDistance = new Vector2(1.4f, -1.4f);
            return texto;
        }

        public void MirarReceta()
        {
            mira = -1f;
            Poner(Expresion.Curioso, 2.4f, true);
            Decir("Mira bien...");
        }

        public void MirarCaldero()
        {
            mira = -0.35f;
            Poner(Expresion.Quieto, 1.2f, true);
            Decir("Tu turno");
        }

        public void Verter()
        {
            salto = Mathf.Max(salto, 1f);
            Poner(Expresion.Canta, 1.1f, true);
            Decir("¡Al caldero!");
        }

        public void Festejar()
        {
            animo = 1.6f;
            salto = 1.35f;
            Poner(Expresion.Feliz, 2.3f, true);
            Decir(Random.value < 0.5f ? "¡Eso es!" : "¡Muy bien!");
        }

        public void Asustarse()
        {
            animo = -1.4f;
            salto = 0.45f;
            Poner(Expresion.Asustado, 1.7f, true);
            Decir(Random.value < 0.5f ? "¡Uy!" : "¡Otra vez!");
        }

        void Poner(Expresion siguiente, float segundos, bool mandada)
        {
            expresion = siguiente;
            hasta = Time.time + segundos;
            forzada = mandada;
        }

        void Decir(string frase)
        {
            if (voz == null)
                return;
            voz.text = frase;
            vozRestante = 1.45f;
        }

        void ElegirOcio()
        {
            if (forzada && Time.time < hasta)
                return;

            forzada = false;
            float dado = Random.value;
            if (dado < 0.18f)
                Poner(Expresion.Guino, 0.7f, false);
            else if (dado < 0.34f)
                Poner(Expresion.Curioso, 1.5f, false);
            else if (dado < 0.48f)
                Poner(Expresion.Canta, 1.3f, false);
            else if (dado < 0.6f)
                Poner(Expresion.Habla, 1.1f, false);
            else if (dado < 0.72f)
                Poner(Expresion.Duerme, 2.4f, false);
            else if (dado < 0.84f)
                Poner(Expresion.Feliz, 1.2f, false);
            else
                Poner(Expresion.Quieto, Random.Range(1.4f, 2.6f), false);

            if (expresion == Expresion.Habla)
                Decir(Random.value < 0.5f ? "Uh uh" : "Mmm...");
            else if (expresion == Expresion.Canta)
                Decir("Tu-hú");
            else if (expresion == Expresion.Duerme)
                Decir("");
            else if (expresion == Expresion.Curioso)
                mira = Random.value < 0.7f ? -1f : 1f;
        }

        void Update()
        {
            if (cuerpo == null)
                return;

            if (Time.time >= hasta)
                ElegirOcio();

            animo = Mathf.MoveTowards(animo, 0f, Time.deltaTime * 0.45f);
            salto = Mathf.Max(0f, salto - Time.deltaTime * 1.7f);
            miraSuave = Mathf.MoveTowards(miraSuave, mira, Time.deltaTime * 2.4f);

            parpadeo -= Time.deltaTime;
            parpadeoCierra = Mathf.Max(0f, parpadeoCierra - Time.deltaTime);
            if (parpadeo <= 0f && expresion != Expresion.Duerme && expresion != Expresion.Sorpresa)
            {
                parpadeo = Random.Range(2.1f, 4.4f);
                parpadeoCierra = 0.14f;
            }

            float metaIzq = 0f;
            float metaDer = 0f;
            float extraY = 0f;
            float extraX = Mathf.Sin(Time.time * 0.7f) * 2f;
            float giro = Mathf.Lerp(-12f, 9f, (miraSuave + 1f) * 0.5f);
            float ancho = 1f;
            float alto = 1f + Mathf.Sin(Time.time * 2.2f) * 0.018f;
            float chispa = 0f;

            switch (expresion)
            {
                case Expresion.Curioso:
                    giro -= 8f;
                    extraX -= 6f;
                    alto += 0.04f;
                    ancho -= 0.03f;
                    break;
                case Expresion.Habla:
                    alto += Mathf.Abs(Mathf.Sin(Time.time * 14f)) * 0.035f;
                    giro += Mathf.Sin(Time.time * 14f) * 2f;
                    break;
                case Expresion.Feliz:
                    metaIzq = metaDer = 0.42f;
                    extraY += Mathf.Abs(Mathf.Sin(Time.time * 6f)) * 10f;
                    chispa = 1f;
                    break;
                case Expresion.Sorpresa:
                    ancho += 0.08f;
                    alto -= 0.06f;
                    extraY += 8f;
                    break;
                case Expresion.Asustado:
                    metaIzq = metaDer = 0.15f;
                    ancho -= 0.06f;
                    alto += 0.08f;
                    extraX += Mathf.Sin(Time.time * 28f) * 4f;
                    giro += 6f;
                    break;
                case Expresion.Guino:
                    metaIzq = 0.92f;
                    giro += 4f;
                    break;
                case Expresion.Duerme:
                    metaIzq = metaDer = 0.9f;
                    alto -= 0.03f;
                    extraY += Mathf.Sin(Time.time * 1.2f) * 3f;
                    break;
                case Expresion.Canta:
                    extraY += Mathf.Abs(Mathf.Sin(Time.time * 8f)) * 14f;
                    giro += Mathf.Sin(Time.time * 8f) * 6f;
                    chispa = 0.45f;
                    break;
            }

            if (parpadeoCierra > 0f)
                metaIzq = metaDer = 1f;

            if (animo > 0.5f)
            {
                metaIzq = Mathf.Max(metaIzq, 0.35f);
                metaDer = Mathf.Max(metaDer, 0.35f);
                chispa = 1f;
            }

            cierre = Mathf.MoveTowards(cierre, metaIzq, Time.deltaTime * (metaIzq > cierre ? 14f : 7f));
            cierreDer = Mathf.MoveTowards(cierreDer, metaDer, Time.deltaTime * (metaDer > cierreDer ? 14f : 7f));
            if (parpadoIzq != null)
                parpadoIzq.localScale = new Vector3(1f, cierre, 1f);
            if (parpadoDer != null)
                parpadoDer.localScale = new Vector3(1f, cierreDer, 1f);

            float sube = Mathf.Sin(Mathf.Clamp01(salto) * Mathf.PI);
            float aire = sube * (animo > 0.4f || expresion == Expresion.Feliz ? 48f : 22f);
            float encoge = animo < 0f ? 1f + animo * 0.12f : 1f;
            cuerpo.anchoredPosition = pies + new Vector2(extraX, extraY + aire + Mathf.Sin(Time.time * 2.1f) * 4f);
            cuerpo.localScale = new Vector3(encoge * ancho / Mathf.Max(0.2f, alto), encoge * alto, 1f);
            cuerpo.localRotation = Quaternion.Euler(0f, 0f, giro + Mathf.Sin(Time.time * 1.5f) * 1.6f);

            if (sombra != null)
            {
                float aplasta = 1f - sube * 0.35f - Mathf.Abs(extraY) * 0.004f;
                sombra.localScale = new Vector3(Mathf.Max(0.45f, aplasta), 1f, 1f);
                Color mancha = sombra.GetComponent<Image>().color;
                mancha.a = 0.38f * (1f - sube * 0.45f);
                sombra.GetComponent<Image>().color = mancha;
            }

            PintarVoz();
            PintarSuenio();
            PintarSudor();
            PintarChispas(chispa);
        }

        void PintarVoz()
        {
            if (voz == null)
                return;
            vozRestante -= Time.deltaTime;
            float alfa = Mathf.Clamp01(vozRestante / 0.35f);
            if (vozRestante > 1f)
                alfa = 1f;
            if (string.IsNullOrEmpty(voz.text))
                alfa = 0f;
            Color c = voz.color;
            c.a = alfa;
            voz.color = c;
            var borde = voz.GetComponent<Outline>();
            if (borde != null)
            {
                Color tinta = borde.effectColor;
                tinta.a = 0.9f * alfa;
                borde.effectColor = tinta;
            }
        }

        void PintarSuenio()
        {
            if (zzz == null)
                return;
            bool dormido = expresion == Expresion.Duerme;
            float alfa = dormido ? 0.55f + 0.45f * Mathf.Sin(Time.time * 3f) : 0f;
            var color = zzz.color;
            color.a = Mathf.MoveTowards(color.a, alfa, Time.deltaTime * 3f);
            zzz.color = color;
            zzz.rectTransform.anchoredPosition = new Vector2(tamCuerpo.x * 0.28f, tamCuerpo.y * 0.42f + Mathf.Sin(Time.time * 2f) * 8f);
            var borde = zzz.GetComponent<Outline>();
            if (borde != null)
            {
                Color tinta = borde.effectColor;
                tinta.a = color.a;
                borde.effectColor = tinta;
            }
        }

        void PintarSudor()
        {
            if (sudor == null)
                return;
            bool miedo = expresion == Expresion.Asustado || animo < -0.4f;
            var color = sudor.color;
            color.a = Mathf.MoveTowards(color.a, miedo ? 0.9f : 0f, Time.deltaTime * 4f);
            sudor.color = color;
            if (!miedo)
                return;
            var pos = sudor.rectTransform.anchoredPosition;
            pos.y -= 18f * Time.deltaTime;
            if (pos.y < tamCuerpo.y * 0.05f)
                pos.y = tamCuerpo.y * 0.28f;
            sudor.rectTransform.anchoredPosition = pos;
        }

        void PintarChispas(float fuerza)
        {
            if (chispas == null)
                return;
            for (int i = 0; i < chispas.Length; i++)
            {
                float ang = Time.time * (fuerza > 0.7f ? 2.4f : 1.2f) + i * 0.9f;
                float radio = tamCuerpo.y * (0.22f + 0.08f * Mathf.Sin(Time.time * 2f + i));
                var rect = chispas[i].rectTransform;
                rect.anchoredPosition = pies + new Vector2(Mathf.Cos(ang) * radio * 0.85f, tamCuerpo.y * 0.45f + Mathf.Sin(ang) * radio);
                var color = chispas[i].color;
                float brillo = fuerza * (0.35f + 0.65f * Mathf.Abs(Mathf.Sin(Time.time * 5f + i)));
                color.a = brillo;
                chispas[i].color = color;
                float lado = 10f + 12f * brillo;
                rect.sizeDelta = new Vector2(lado, lado);
            }
        }
    }
}
