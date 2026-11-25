using System;
using TMPro;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#else
#error "Activa el paquete Input System en Active Input Handling"
#endif

// Lee la ESP32 con el Input System, aunque Unity la clasifique como Gamepad, Joystick o HID.
public class EntradaEsp32 : EntradaBrazo
{
    [SerializeField] private string nombreMando = "BrazoAR";
    [SerializeField] private float antirrebote = 0.25f;
    [SerializeField] private TMP_Text textoConexion;

    private InputDevice mando;
    private AxisControl ejeX;
    private AxisControl ejeY;
    private ButtonControl boton;
    private float ultimaPulsacion = float.NegativeInfinity;

    private void OnEnable()
    {
        InputSystem.onDeviceChange += AlCambiarDispositivo;
        RegistrarDispositivos();
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= AlCambiarDispositivo;
        Eje = Vector2.zero;
    }

    private void Update()
    {
        InputDevice encontrado = BuscarMando();

        if (encontrado == null)
        {
            // Si se pierde la conexión, el brazo debe quedarse quieto.
            mando = null;
            Eje = Vector2.zero;
            MostrarEstado("Conecta el gamepad BrazoAR-01");
            return;
        }

        if (encontrado != mando)
        {
            AsignarControles(encontrado);
        }

        if (ejeX == null || ejeY == null || boton == null)
        {
            Eje = Vector2.zero;
            MostrarEstado($"{mando.displayName}: faltan controles");
            return;
        }

        Eje = new Vector2(ejeX.ReadValue(), ejeY.ReadValue());

        if (boton.wasPressedThisFrame && Time.unscaledTime - ultimaPulsacion >= antirrebote)
        {
            ultimaPulsacion = Time.unscaledTime;
            RegistrarPulsacion();
        }

        MostrarEstado($"{mando.displayName} x={Eje.x:0.00} y={Eje.y:0.00} boton={(boton.isPressed ? 1 : 0)}");
    }

    private InputDevice BuscarMando()
    {
        InputDevice masReciente = null;
        double ultimaActividad = double.NegativeInfinity;

        foreach (InputDevice dispositivo in InputSystem.devices)
        {
            if (!dispositivo.enabled)
            {
                continue;
            }

            if (TieneNombre(dispositivo))
            {
                return dispositivo;
            }

            if (EsMando(dispositivo) && dispositivo.lastUpdateTime > ultimaActividad)
            {
                masReciente = dispositivo;
                ultimaActividad = dispositivo.lastUpdateTime;
            }
        }

        return masReciente;
    }

    private bool TieneNombre(InputDevice dispositivo)
    {
        return Contiene(dispositivo.displayName)
            || Contiene(dispositivo.name)
            || Contiene(dispositivo.description.product);
    }

    private bool Contiene(string texto)
    {
        return !string.IsNullOrEmpty(texto)
            && texto.IndexOf(nombreMando, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool EsMando(InputDevice dispositivo)
    {
        return dispositivo is Gamepad
            || dispositivo is Joystick
            || dispositivo.description.interfaceName == "HID";
    }

    private void AsignarControles(InputDevice dispositivo)
    {
        mando = dispositivo;
        ejeX = null;
        ejeY = null;
        boton = null;

        if (dispositivo is Gamepad gamepad)
        {
            ejeX = gamepad.leftStick.x;
            ejeY = gamepad.leftStick.y;
            boton = gamepad.buttonSouth;
        }
        else if (dispositivo is Joystick joystick)
        {
            ejeX = joystick.stick?.x;
            ejeY = joystick.stick?.y;
            boton = joystick.trigger;
        }
        else
        {
            BuscarControlesHid(dispositivo);
        }

        Debug.Log($"Mando elegido: {dispositivo.name}, tipo {dispositivo.GetType().Name}, "
            + $"X {ejeX?.path}, Y {ejeY?.path}, botón {boton?.path}");
    }

    private void BuscarControlesHid(InputDevice dispositivo)
    {
        foreach (InputControl control in dispositivo.allControls)
        {
            if (boton == null && control is ButtonControl botonEncontrado)
            {
                boton = botonEncontrado;
            }
            else if (control is AxisControl eje)
            {
                if (ejeX == null && eje.name.Equals("x", StringComparison.OrdinalIgnoreCase))
                {
                    ejeX = eje;
                }
                else if (ejeY == null && eje.name.Equals("y", StringComparison.OrdinalIgnoreCase))
                {
                    ejeY = eje;
                }
            }
        }
    }

    private void AlCambiarDispositivo(InputDevice dispositivo, InputDeviceChange cambio)
    {
        Debug.Log($"Dispositivo {cambio}: {Describir(dispositivo)}");
    }

    private void RegistrarDispositivos()
    {
        foreach (InputDevice dispositivo in InputSystem.devices)
        {
            Debug.Log($"Dispositivo detectado: {Describir(dispositivo)}");
        }
    }

    private static string Describir(InputDevice dispositivo)
    {
        return $"{dispositivo.name}, tipo {dispositivo.GetType().Name}, "
            + $"interfaz {dispositivo.description.interfaceName}, "
            + $"producto {dispositivo.description.product}";
    }

    private void MostrarEstado(string texto)
    {
        if (textoConexion != null)
        {
            textoConexion.text = texto;
        }
    }
}
