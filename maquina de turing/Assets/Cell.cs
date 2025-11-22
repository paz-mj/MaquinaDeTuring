using UnityEngine;

public class Cell : MonoBehaviour
{
    // Estado interno: 0 = Apagado, 1 = Rojo, 2 = Morado
    public int state = 0;

    // Aquí arrastraremos los materiales que creaste
    public Material matOff;
    public Material matOn;
    public Material matSep;

    private Renderer _rend;

    void Awake()
    {
        _rend = GetComponent<Renderer>();
        UpdateColor(); // Poner el color inicial
    }

    // Esta función se llama cuando haces clic sobre la esfera
    void OnMouseDown()
    {
        Debug.Log("¡ME HAS TOCADO! Soy la celda " + gameObject.name); // <--- AGREGA ESTO
        state++;
        if (state > 2) state = 0;
        UpdateColor();
    }

    // Esta función la usará la Máquina para cambiar el color automáticamente
    public void SetState(int newState)
    {
        state = newState;
        UpdateColor();
    }

    // Actualiza el material visual según el número del estado
    public void UpdateColor()
    {
        if (_rend == null) return;

        if (state == 0) _rend.material = matOff;
        else if (state == 1) _rend.material = matOn;
        else if (state == 2) _rend.material = matSep;
    }
}