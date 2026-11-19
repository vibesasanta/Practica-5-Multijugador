using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;

public class GestorPersistencia : MonoBehaviour
{
    [Header("Referencias UI")]
    public TMP_InputField inputUsuario;
    public TMP_InputField inputPassword;
    public TMP_Text textoEstado;
    public TMP_Text textoDatosJugador;

    // Datos del jugador que persistirán en la nube
    private int puntuacion = 0;
    private int nivel = 1;

    async void Start()
    {
        try
        {
            // Inicializar servicios de Unity Cloud
            await UnityServices.InitializeAsync();
            textoEstado.text = "Servicios de Unity iniciados. Inicia sesión o regístrate.";
        }
        catch (Exception e)
        {
            textoEstado.text = "Error al iniciar servicios: " + e.Message;
        }
    }

    /// <summary>
    /// Metodo para registrar al usuario
    /// </summary>
    public async void RegistrarUsuario()
    {
        string user = inputUsuario.text.Trim();
        string pass = inputPassword.text;

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            textoEstado.text = "Rellena usuario y contraseña.";
            return;
        }

        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(user, pass);
            textoEstado.text = $"Usuario '{user}' registrado. ID: {AuthenticationService.Instance.PlayerId}";

            // Inicializamos sus datos por defecto en Cloud Save la primera vez
            puntuacion = 0;
            nivel = 1;
            await GuardarDatosNube();
            ActualizarUI();
        }
        catch (AuthenticationException ex)
        {
            textoEstado.text = "Error de registro: " + ex.Message;
        }
        catch (RequestFailedException ex)
        {
            textoEstado.text = "Fallo en la petición: " + ex.Message;
        }
    }

    /// <summary>
    /// Metodo para iniciar la sesion
    /// </summary>
    public async void IniciarSesion()
    {
        string user = inputUsuario.text.Trim();
        string pass = inputPassword.text;

        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(user, pass);
            textoEstado.text = $"Sesión iniciada como '{user}'. Cargando datos...";

            // Al entrar, pillamos sus datos de la nube
            await CargarDatosNube();
        }
        catch (AuthenticationException ex)
        {
            textoEstado.text = "Usuario o contraseña incorrectos: " + ex.Message;
        }
        catch (RequestFailedException ex)
        {
            textoEstado.text = "Error de conexión: " + ex.Message;
        }
    }

    /// <summary>
    /// Metodo para cambiar la puntuacion y guardarla
    /// </summary>
    public async void SumarPuntosYGuardar()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            textoEstado.text = "Debes iniciar sesión primero.";
            return;
        }

        puntuacion += 100;
        if (puntuacion % 300 == 0) nivel++;

        ActualizarUI();
        await GuardarDatosNube();
    }

    private async System.Threading.Tasks.Task GuardarDatosNube()
    {
        try
        {
            var datos = new Dictionary<string, object>
            {
                { "puntuacion", puntuacion },
                { "nivel", nivel }
            };

            await CloudSaveService.Instance.Data.Player.SaveAsync(datos);
            textoEstado.text = "Datos guardados correctamente en la nube.";
        }
        catch (Exception e)
        {
            textoEstado.text = "Error al guardar: " + e.Message;
        }
    }

    private async System.Threading.Tasks.Task CargarDatosNube()
    {
        try
        {
            var claves = new HashSet<string> { "puntuacion", "nivel" };
            var datosGuardados = await CloudSaveService.Instance.Data.Player.LoadAsync(claves);

            if (datosGuardados.TryGetValue("puntuacion", out var itemPuntos))
                puntuacion = itemPuntos.Value.GetAs<int>();
            else
                puntuacion = 0;

            if (datosGuardados.TryGetValue("nivel", out var itemNivel))
                nivel = itemNivel.Value.GetAs<int>();
            else
                nivel = 1;

            ActualizarUI();
            textoEstado.text = "Datos recuperados de la nube.";
        }
        catch (Exception e)
        {
            textoEstado.text = "Error al cargar: " + e.Message;
        }
    }

    public void CerrarSesion()
    {
        AuthenticationService.Instance.SignOut();
        puntuacion = 0;
        nivel = 1;
        textoDatosJugador.text = "Sin sesión activa";
        textoEstado.text = "Sesión cerrada.";
    }

    private void ActualizarUI()
    {
        textoDatosJugador.text = $"Puntuación: {puntuacion} | Nivel: {nivel}";
    }
}