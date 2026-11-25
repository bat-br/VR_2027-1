using UnityEngine;
using System;
using System.IO;
using System.Text;
using TMPro;
using SQLite;

public class BDSQL : MonoBehaviour
{
    //Campos de UI
    [SerializeField] TMP_InputField textoNombre;
    [SerializeField] TMP_InputField textoEdad;
    [SerializeField] TMP_InputField textoIdActualizar;
    [SerializeField] TMP_Text textoSalida;

    public bool registroBoleano = true;

    //Conexion con BD
    private SQLiteConnection bd; //Conexion abierta a base de datos
    private string rutaBD; //Ruta del archivo persistente BD

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake() //Establecer configuraciones de la escena
    {
        rutaBD = Path.Combine(Application.persistentDataPath, "MiBaseDatos.db");

        //Abre o crea el archivo de la bd en la ruta
        bd = new SQLiteConnection(rutaBD);

        //Activa las llaves foraneas en SQLlite
        bd.Execute("PRAGMA foreign_keys = ON");

        Debug.Log("Base de datos en: " + rutaBD);

        //Crear las tablas
        CrearTablas();
    }

    private void CrearTablas()
    {
        //Tabla ESCUELA: Cada ESCUELA aparece solo una vez nombre UNICO
        bd.Execute(@"
                CREATE TABLE IF NOT EXISTS Escuela(
                    id      INTEGER PRIMARY KEY AUTOINCREMENT,
                    nombre  TEXT NOT NULL UNIQUE,
                    anio    INTEGER NOT NULL 
                );");

        //Tabla USUARIO: Cada CORREO aparece solo una vez nombre UNICO
        bd.Execute(@"
                CREATE TABLE IF NOT EXISTS Usuario(
                    id      INTEGER PRIMARY KEY AUTOINCREMENT,
                    user_name  TEXT NOT NULL,
                    email    TEXT NOT NULL UNIQUE 
                );");

        //Tabla REGISTRO: Cada CORREO aparece solo una vez nombre UNICO
        bd.Execute(@"
                CREATE TABLE IF NOT EXISTS Registro(
                    id      INTEGER PRIMARY KEY AUTOINCREMENT,
                    nombre  TEXT NOT NULL,
                    edad    INTEGER NOT NULL,
                    activo  INTEGER NOT NULL DEFAULT 1,
                    escuela_id INTEGER,
                    fecha   TEXT DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (escuela_id) REFERENCES Escuela(id)
                );");
    }

    //Se conecta al evento del toggle
    public void Booleano(bool toogleB)
    {
        registroBoleano = toogleB;

    }

    //Crea registros INSERTAR
    public void Registro()
    {
        //Validar que la edad sea un numero entero
        if (!int.TryParse(textoEdad.text, out int edad))
        {
            Debug.Log("edad no valida");
            return;
        }

        try
        {
            bd.RunInTransaction(() =>
            {
                //1 Escuela: INsertar un valor predeterminado
                bd.Execute("INSERT OR IGNORE INTO Escuela (nombre, anio) VALUES (?,?);", "UNAM", 2025);

                Debug.Log("Dato Escuela registrado: UNAM");
                //Obtiene el id de la ecuela para relacionarla con el registro
                int escuelaId = ObtenerEscuela("UNAM");

                //2 Registro de alumno 
                bd.Execute("INSERT INTO Registro (nombre, edad, activo, escuela_id) VALUES(?, ?, ?, ?);",
                    textoNombre.text, edad, registroBoleano, escuelaId
                    );

                //3 El usuario se inserta solo si el email no existe todavia
                //Objeto usuario se va a crear
                Usuario usuario = new Usuario("Pepe", "pepe@gmail.com");
                bd.Execute("INSERT OR IGNORE INTO Usuario (user_name, email) VALUES (? ,?);",
                    usuario.UserName, usuario.Email);
            });
        }
        catch (Exception e)
        {
            Debug.Log("Error al registrar: " + e.Message);
            return;
        }
    }

    //Actualizar datos del registro
    //Parte A: Actualiza la pila completa en tabla escuela con el primer Id
    //Parte B: Actualiza un solo valor de la tabla registro- Nombre

    public void ActualizarRegistro()
    {
        //Obtener y validar el nuevo nombre
        string nuevoNombre = textoNombre.text.Trim();
        if (string.IsNullOrEmpty(nuevoNombre))
        {
            return;
        }

        //Obtener ID de la primera escuela registrada
        int idRegistro;
        int idEscuela = bd.ExecuteScalar<int>("SELECT IFNULL(MIN(id), 0) FROM Escuela");
        if (textoIdActualizar == null || !int.TryParse(textoIdActualizar.text, out idRegistro))
        {
            idRegistro = bd.ExecuteScalar<int>("SELECT IFNULL(MAX(id), 0) FROM Registro");
        }
        else
        { 
            idRegistro = int.Parse(textoIdActualizar.text);
        }
            
        //Alguna tabla vacia
        if (idEscuela == 0 || idRegistro == 0)
        {
            Debug.Log("No hay datos para actualizar");
            return;
        }
        try
        {
            bd.RunInTransaction(() =>
            {
                //Parte A: Actualizacion de la pila completa en la tabla escuela
                int filaEscuela = bd.Execute(@"
                    UPDATE Escuela
                    SET nombre = ?,
                        anio = ?
                    WHERE id = ?;",
                    "POLI",
                    2026,
                    idEscuela);

                if (filaEscuela == 0)
                {
                    throw new Exception($"No existe una escuela con ese ID {idEscuela}");
                }
                Debug.Log($"[Parte A] Escuela {idEscuela} actualizada a: nombre = Poli, anio = 2026");

                //Parte B: actualizacion de un solo registro :  nombre

                //Leer nombre actual antes de cambiarlo
                string nombreActual = bd.ExecuteScalar<string>("" +
                    "SELECT nombre FROM Registro WHERE id = ?;", idRegistro);

                int filasRegistro = bd.Execute(
                    "UPDATE Registro SET nombre = ? WHERE id = ?;",
                    nuevoNombre, idRegistro);

                if (filasRegistro == 0)
                {
                    throw new Exception($"No existe un registro con ese ID {idRegistro}");
                }

                string nombreDeseus = bd.ExecuteScalar<string>("" +
                    "SELECT nombre FROM Registro WHERE id = ?;", idRegistro);

                Debug.Log($"[Parte B] Registro {idRegistro}, columna nombre : Se Actualizó");

            });
        }
        catch (SQLiteException e) when (e.Result == SQLite3.Result.Constraint)
        {
            Debug.Log("No se pudo actualizar, ya existen los datos o no hay concordancia");
            return;
        }
        catch (Exception e)
        {
            Debug.Log("Error al actualizar: " + e.Message);
            return;
        }
    }

    public void LoadFromBD()
    {
        try
        {
            var sb = new StringBuilder();


            //Tabla escuela
            var escuelas = bd.Query<FilaEscuela>(@"
                SELECT id AS Id, typeof(id) AS TipoId,
                        nombre AS Nombre, typeof(nombre) AS TipoNombre,
                        anio As Anio, typeof(anio) AS TipoAnio
                FROM Escuela
                ORDER By id;");

            sb.AppendLine($"Escuelas ({escuelas.Count})");
            if (escuelas.Count == 0)
            {
                sb.AppendLine("Registro encontrado");
            }
            foreach (var esc in escuelas)
            {
                sb.AppendLine($"Escuela #{esc.Id}");
                sb.AppendLine(Campo("Id", esc.Id, esc.TipoId));
                sb.AppendLine(Campo("Nombre", esc.Nombre, esc.TipoNombre));
                sb.AppendLine(Campo("Año", esc.Anio, esc.TipoAnio));

            }



            //Tabla Registro
            var filas = bd.Query<FilaRegistro>(@"
                SELECT r.id AS Id, typeof(r.id) AS TipoId,
                        r.nombre AS Nombre, typeof(r.nombre) AS TipoNombre,
                        r.edad AS Edad, typeof(r.edad) AS TipoEdad,
                        r.activo AS Activo, typeof(r.activo) AS TipoActivo,
                        r.fecha AS Fecha, typeof(r.fecha) AS TipoFecha,
                        e.nombre AS Escuela, typeof(e.nombre) AS TipoEscuela,
                        e.anio AS Anio, typeof(e.anio) AS TipoAnio
                FROM Registro r
                LEFT JOIN Escuela e ON r.escuela_id = e.id
                ORDER By r.id;");

            sb.AppendLine($"Registro ({filas.Count})");
            if (filas.Count == 0)
            {
                sb.AppendLine("Registro encontrado");
            }
            foreach (var f in filas)
            {
                sb.AppendLine($"Registro #{f.Id}");
                sb.AppendLine(Campo("Id", f.Id, f.TipoId));
                sb.AppendLine(Campo("Nombre", f.Nombre, f.TipoNombre));
                sb.AppendLine(Campo("Edad", f.Edad, f.TipoEdad));
                sb.AppendLine(Campo("Activo", f.Activo, f.TipoActivo));
                sb.AppendLine(Campo("Fecha", f.Fecha, f.TipoFecha));
                sb.AppendLine(Campo("Escuela", f.Escuela, f.TipoEscuela));
                sb.AppendLine(Campo("Año", f.Anio, f.TipoAnio));

            }

            //Objeto usuario
            var usuarios = bd.Query<Usuario>("SELECT user_name AS UserName, email AS EMAIL FROM Usuario");
            foreach (var u in usuarios)
            {
                sb.AppendLine($"Objeto #{u.GetType().Name}");
                sb.AppendLine(Campo("UserName", u.UserName, "text"));
                sb.AppendLine(Campo("email", u.Email, "text"));
            }

            Mostrar(sb.ToString());
        }
        catch (Exception e)
        {
            Debug.Log("Error al leer la base de datos: " + e.Message);
        }
    }


    //Metodo para mostrar objeto texto de la interfaz
    public void Mostrar(string mensaje)
    {
        Debug.Log(mensaje);
        textoSalida.text = mensaje;
    }

    public int ObtenerEscuela(string nombre)
    {
        return bd.ExecuteScalar<int>("SELECT id FROM Escuela WHERE nombre = ?;", nombre);
    }

    public string Campo(string etiqueta, object valor, string tipoSQL)
    {
        string textoValor = valor == null ? "NULO" : valor.ToString();
        string tipoCSharp = valor == null ? "NULO" : valor.GetType().Name;
        return $"{etiqueta} : {textoValor} (C# {tipoCSharp}), SQL {tipoSQL}";
    }



    // Update is called once per frame
    void Update()
    {

    }


    //Recibe cada fila de la tabla escuela
    [UnityEngine.Scripting.Preserve]
    public class FilaEscuela
    {
        [UnityEngine.Scripting.Preserve] public int Id { get; set; }
        [UnityEngine.Scripting.Preserve] public string Nombre { get; set; }
        [UnityEngine.Scripting.Preserve] public string Anio { get; set; }

        [UnityEngine.Scripting.Preserve] public string TipoId { get; set; }
        [UnityEngine.Scripting.Preserve] public string TipoNombre { get; set; }
        [UnityEngine.Scripting.Preserve] public string TipoAnio { get; set; }


    }

    //Recibe cada fila el JOIN de registro + escuela de pertenencia
    [UnityEngine.Scripting.Preserve]
    public class FilaRegistro
    {
        [UnityEngine.Scripting.Preserve] public int Id { get; set; }
        [UnityEngine.Scripting.Preserve] public string Nombre { get; set; }
        [UnityEngine.Scripting.Preserve] public int? Edad { get; set; }
        [UnityEngine.Scripting.Preserve] public bool Activo { get; set; }
        [UnityEngine.Scripting.Preserve] public string Fecha { get; set; }
        [UnityEngine.Scripting.Preserve] public string Escuela { get; set; }
        [UnityEngine.Scripting.Preserve] public int? Anio { get; set; }

        //Tipo de almacenamiento en SQLite
        [UnityEngine.Scripting.Preserve] public string TipoId { get; set; }
        [UnityEngine.Scripting.Preserve] public string TipoNombre { get; set; }
        [UnityEngine.Scripting.Preserve] public string TipoEdad { get; set; }
        [UnityEngine.Scripting.Preserve] public string TipoActivo { get; set; }
        [UnityEngine.Scripting.Preserve] public string TipoFecha { get; set; }
        [UnityEngine.Scripting.Preserve] public string TipoEscuela { get; set; }
        [UnityEngine.Scripting.Preserve] public string? TipoAnio { get; set; }

    }
    public class Usuario
    {
        [UnityEngine.Scripting.Preserve] public string UserName { get; set; }
        [UnityEngine.Scripting.Preserve] public string Email { get; set; }
        [UnityEngine.Scripting.Preserve] public Usuario() { }

        public Usuario(string username, string email)
        {
            UserName = username;
            Email = email;
        }


    }



}