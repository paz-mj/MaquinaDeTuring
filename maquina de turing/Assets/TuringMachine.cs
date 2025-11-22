using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TuringMachine : MonoBehaviour
{
    [Header("Configuración Física")]
    public Transform carrito;       // Arrastra aquí tu objeto "Carrito"
    public Cell[] cintaLeds;        // Arrastra aquí tus 21 esferas (Cell_0 a Cell_20)
    public float velocidad = 2.0f;  // Velocidad del movimiento

    // Variables internas
    private bool ejecutando = false;
    private int indiceCabezal = 0;  // En qué LED estamos (0 a 20)
    private int estadoActual = 0;   // Estado q0, q1, etc.

    // Estructura para guardar las reglas
    private struct Regla
    {
        public int escribir; // Qué color poner
        public int mover;    // -1 Izquierda, 1 Derecha, 0 Parar
        public int nuevoEstado;
    }

    // Tablas de reglas (Diccionarios)
    private Dictionary<string, Regla> reglasSuma = new Dictionary<string, Regla>();
    private Dictionary<string, Regla> reglasResta = new Dictionary<string, Regla>();

    void Start()
    {
        CargarReglas();
    }

    // --- FUNCIONES PARA LOS BOTONES ---
    public void BotonSumar()
    {
        if (!ejecutando) StartCoroutine(ProcesoMaquina(true));
    }

    public void BotonRestar()
    {
        if (!ejecutando) StartCoroutine(ProcesoMaquina(false));
    }

    public void BotonReset()
    {
        StopAllCoroutines();
        ejecutando = false;
        indiceCabezal = 0;
        estadoActual = 0;
        // Apagar todos los LEDs visualmente
        foreach (var c in cintaLeds) c.SetState(0);
        // Mover carrito al inicio instantáneamente
        ActualizarPosicionVisual(0);
    }

    // --- LÓGICA PRINCIPAL (CORRUTINA) ---
    IEnumerator ProcesoMaquina(bool esSuma)
    {
        ejecutando = true;
        indiceCabezal = 0;
        estadoActual = 0; // q0

        // Asegurar que visualmente estamos en el inicio
        yield return MoverCarritoVisual(0);

        while (ejecutando)
        {
            // 1. Leer el símbolo bajo el cabezal
            if (indiceCabezal < 0 || indiceCabezal >= cintaLeds.Length)
            {
                Debug.LogError("Error: Cabezal fuera de límites");
                break;
            }
            int simboloLeido = cintaLeds[indiceCabezal].state;

            // 2. Buscar la regla correspondiente
            string clave = estadoActual + "," + simboloLeido;
            Dictionary<string, Regla> tablaUsada = esSuma ? reglasSuma : reglasResta;

            if (tablaUsada.ContainsKey(clave))
            {
                Regla r = tablaUsada[clave];

                // 3. Escribir (Cambiar color LED)
                cintaLeds[indiceCabezal].SetState(r.escribir);

                // 4. Verificar parada (Halt)
                if (r.mover == 0)
                {
                    Debug.Log("Fin del proceso.");
                    ejecutando = false;
                    break;
                }

                // 5. Mover Cabezal (Lógica)
                indiceCabezal += r.mover;
                estadoActual = r.nuevoEstado;

                // 6. Mover Cabezal (Visual - Animación)
                yield return MoverCarritoVisual(indiceCabezal);
            }
            else
            {
                // Si no hay regla, se detiene (Halt implícito)
                Debug.Log("Halt (Sin regla definida).");
                ejecutando = false;
            }
        }
    }

    IEnumerator MoverCarritoVisual(int indiceDestino)
    {
        if (indiceDestino < 0 || indiceDestino >= cintaLeds.Length) yield break;

        // Obtener la posición X del LED destino
        Vector3 destino = cintaLeds[indiceDestino].transform.position;
        // Mantener la altura Y y profundidad Z del carrito original
        Vector3 posFinal = new Vector3(destino.x, carrito.position.y, carrito.position.z);

        Vector3 posInicial = carrito.position;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * velocidad;
            carrito.position = Vector3.Lerp(posInicial, posFinal, t);
            yield return null;
        }
        carrito.position = posFinal; // Asegurar posición exacta al final
    }

    void ActualizarPosicionVisual(int index)
    {
        if (index >= 0 && index < cintaLeds.Length)
        {
            Vector3 destino = cintaLeds[index].transform.position;
            carrito.position = new Vector3(destino.x, carrito.position.y, carrito.position.z);
        }
    }

    // --- CARGA DE REGLAS (Lógica Validada) ---
    void CargarReglas()
    {
        // Formato: AgregarRegla(Diccionario, EstadoActual, Lee, Escribe, Mueve, NuevoEstado)
        // Mueve: 1 = Derecha (R), -1 = Izquierda (L), 0 = Halt

        // === SUMA ===
        AgregarRegla(reglasSuma, 0, 0, 0, 1, 1); // q0, Lee 0(S) -> R, q1

        AgregarRegla(reglasSuma, 1, 1, 1, 1, 1); // Avanza 1s
        AgregarRegla(reglasSuma, 1, 2, 1, 1, 2); // Convierte Sep(2) en 1 -> q2
        AgregarRegla(reglasSuma, 1, 0, 0, 0, -1); // Halt si vacío

        AgregarRegla(reglasSuma, 2, 1, 1, 1, 2); // Avanza 2do bloque
        AgregarRegla(reglasSuma, 2, 0, 0, -1, 3); // Encuentra fin -> L, q3

        AgregarRegla(reglasSuma, 3, 1, 0, -1, 4); // Borra un 1 -> q4
        AgregarRegla(reglasSuma, 3, 2, 0, -1, 4); // Caso borde

        AgregarRegla(reglasSuma, 4, 1, 1, -1, 4); // Retorno
        AgregarRegla(reglasSuma, 4, 2, 1, -1, 4);
        AgregarRegla(reglasSuma, 4, 0, 0, 0, -1); // Llega al inicio (0) -> Halt (Simplificado para array)

        // === RESTA ===
        AgregarRegla(reglasResta, 0, 0, 0, 1, 1);

        AgregarRegla(reglasResta, 1, 1, 1, 1, 1);
        AgregarRegla(reglasResta, 1, 2, 2, 1, 2);
        AgregarRegla(reglasResta, 1, 0, 0, 0, -1);

        AgregarRegla(reglasResta, 2, 1, 1, 1, 2);
        AgregarRegla(reglasResta, 2, 0, 0, -1, 3);

        AgregarRegla(reglasResta, 3, 1, 0, -1, 4); // Borra de B
        AgregarRegla(reglasResta, 3, 2, 0, -1, 8); // B vacío -> Limpieza

        AgregarRegla(reglasResta, 4, 1, 1, -1, 4);
        AgregarRegla(reglasResta, 4, 0, 0, -1, 4);
        AgregarRegla(reglasResta, 4, 2, 2, -1, 5); // Cruza sep

        AgregarRegla(reglasResta, 5, 0, 0, -1, 5);
        AgregarRegla(reglasResta, 5, 1, 0, 1, 6); // Borra de A -> q6
                                                  // Si llegamos al inicio (indice 0) y leemos 0, es negativo -> q9
                                                  // Nota: En Unity el inicio es Cell 0. Si Cell 0 es estado 0...
                                                  // Agregamos caso especial en lógica de movimiento o asumimos Cell 0 es "S"

        AgregarRegla(reglasResta, 6, 0, 0, 1, 6);
        AgregarRegla(reglasResta, 6, 2, 2, 1, 2); // Reinicia ciclo

        AgregarRegla(reglasResta, 8, 1, 1, -1, 8);
        AgregarRegla(reglasResta, 8, 0, 0, -1, 8);
        AgregarRegla(reglasResta, 8, 2, 0, -1, 10);

        AgregarRegla(reglasResta, 10, 1, 0, 1, 10); // Limpieza final
        AgregarRegla(reglasResta, 10, 0, 0, 0, -1);
    }

    void AgregarRegla(Dictionary<string, Regla> tabla, int q, int lee, int escribe, int mueve, int qNext)
    {
        string clave = q + "," + lee;
        Regla r = new Regla { escribir = escribe, mover = mueve, nuevoEstado = qNext };
        tabla[clave] = r;
    }
}