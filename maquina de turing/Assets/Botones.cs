using UnityEngine;

public class BotonFisico : MonoBehaviour
{
    // Definimos qué tipo de botón es este
    public enum TipoFuncion { Sumar, Restar, Reset }
    public TipoFuncion funcion;

    // Referencia al cerebro de la máquina
    public TuringMachine maquina;

    // Efecto visual simple (color al pasar el mouse)
    private Color colorOriginal;
    private Renderer _rend;

    void Start()
    {
        _rend = GetComponent<Renderer>();
        colorOriginal = _rend.material.color;
    }

    void OnMouseEnter()
    {
        // Se ilumina un poco cuando pasas el mouse
        _rend.material.color = Color.yellow;
    }

    void OnMouseExit()
    {
        _rend.material.color = colorOriginal;
    }

    void OnMouseDown()
    {
        // Al hacer clic, empujamos el botón visualmente y llamamos a la máquina
        Debug.Log("Botón presionado: " + funcion);

        if (maquina == null)
        {
            Debug.LogError("¡No has conectado la Máquina al botón en el Inspector!");
            return;
        }

        switch (funcion)
        {
            case TipoFuncion.Sumar:
                maquina.BotonSumar();
                break;
            case TipoFuncion.Restar:
                maquina.BotonRestar();
                break;
            case TipoFuncion.Reset:
                maquina.BotonReset();
                break;
        }
    }
}